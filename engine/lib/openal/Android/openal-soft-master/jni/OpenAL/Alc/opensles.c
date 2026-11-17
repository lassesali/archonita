/*
 * Copyright (C) 2011 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* This is an OpenAL backend for Android using the native audio APIs based on OpenSL ES 1.0.1.
 * It is based on source code for the native-audio sample app bundled with NDK.
 */

#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>

#include <dlfcn.h>

#include "alMain.h"
#include "AL/al.h"
#include "AL/alc.h"

#include <pthread.h>
#include <sched.h>
#include <sys/prctl.h>

#include <jni.h>
#include <android/log.h>


#define LOG_NDEBUG 0
#define LOG_TAG "OpenAL_SLES"

#if 1
#define LOGV(...) __android_log_print(ANDROID_LOG_INFO, LOG_TAG, __VA_ARGS__)
#ifndef LOGE
#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)
#endif
#else
#define LOGV(...)
#ifndef LOGE
#define LOGE(...)
#endif
#endif

// for native audio
#include <SLES/OpenSLES.h>
#include <SLES/OpenSLES_Android.h>

#include "apportable_openal_funcs.h"

#define MAKE_SYM_POINTER(sym) static typeof(sym) * p##sym = NULL
MAKE_SYM_POINTER(SL_IID_ENGINE);
MAKE_SYM_POINTER(SL_IID_ANDROIDSIMPLEBUFFERQUEUE);
MAKE_SYM_POINTER(SL_IID_PLAY);
MAKE_SYM_POINTER(SL_IID_BUFFERQUEUE);
MAKE_SYM_POINTER(slCreateEngine);

// engine interfaces
static SLObjectItf engineObject = NULL;
static SLEngineItf engineEngine;

// output mix interfaces
static SLObjectItf outputMixObject = NULL;

// JNI stuff so we can get the runtime OS version number
static JavaVM* javaVM = NULL;

static int alc_opensles_get_android_api()
{
    jclass androidVersionClass = NULL;
    jfieldID androidSdkIntField = NULL;
    int androidApiLevel = 0;
    JNIEnv* env = NULL;

    if (javaVM == NULL)
        return 0;

    if ((*javaVM)->GetEnv(javaVM, (void**)&env, JNI_VERSION_1_4) != JNI_OK || env == NULL)
        return 0;

    androidVersionClass = (*env)->FindClass(env, "android/os/Build$VERSION");
    if (androidVersionClass)
    {
        androidSdkIntField = (*env)->GetStaticFieldID(env, androidVersionClass, "SDK_INT", "I");
        if (androidSdkIntField != NULL)
        {
            androidApiLevel = (int)((*env)->GetStaticIntField(env, androidVersionClass, androidSdkIntField));
        }
        (*env)->DeleteLocalRef(env, androidVersionClass);
    }
    LOGV("API:%d", androidApiLevel);
    return androidApiLevel;
}
static char *androidModel = NULL;
static char *alc_opensles_get_android_model()
{
    if (!androidModel) {
        jclass androidBuildClass = NULL;
        jfieldID androidModelField = NULL;
        jstring androidModelString = NULL;
        JNIEnv* env = NULL;

        if (javaVM == NULL)
            return NULL;

        if ((*javaVM)->GetEnv(javaVM, (void**)&env, JNI_VERSION_1_4) != JNI_OK || env == NULL)
            return NULL;

        if ((*env)->PushLocalFrame(env, 5) < 0)
            return NULL;
        androidBuildClass = (*env)->FindClass(env, "android/os/Build");
        if (androidBuildClass)
        {
            androidModelField = (*env)->GetStaticFieldID(env, androidBuildClass, "MODEL", "Ljava/lang/String;");
            if (androidModelField != NULL)
            {
                androidModelString = (*env)->GetStaticObjectField(env, androidBuildClass, androidModelField);
                if (androidModelString != NULL)
                {
                    const char *unichars = (*env)->GetStringUTFChars(env, androidModelString, NULL);
                    if (unichars != NULL)
                    {
                        if (!(*env)->ExceptionOccurred(env))
                        {
                            jsize sz = (*env)->GetStringUTFLength(env, androidModelString);
                            androidModel = malloc(sz+1);
                            if (androidModel) {
                                memcpy(androidModel, unichars, sz);
                                androidModel[sz] = '\0';
                            }
                        }
                        (*env)->ReleaseStringUTFChars(env, androidModelString, unichars);
                    }
                }
            }
        }
        (*env)->PopLocalFrame(env, NULL);
    }
    LOGV("Model:%s", androidModel ? androidModel : "(unknown)");
    return androidModel;
}

static long timespecdiff(struct timespec *starttime, struct timespec *finishtime)
{
  long msec;
  msec=(finishtime->tv_sec-starttime->tv_sec)*1000;
  msec+=(finishtime->tv_nsec-starttime->tv_nsec)/1000000;
  return msec;
 }

// Cannot be a constant because we need to tweak differently depending on OS version.
static size_t bufferCount = 8;
static size_t bufferSize = (1024*4);
static size_t defaultBufferSize = (1024*4);
static size_t premixCount = 3;
#define bufferSizeMax (1024*4)

typedef enum {
    OUTPUT_BUFFER_STATE_UNKNOWN,
    OUTPUT_BUFFER_STATE_FREE,
    OUTPUT_BUFFER_STATE_MIXED,
    OUTPUT_BUFFER_STATE_ENQUEUED,
} outputBuffer_state_t;

typedef struct outputBuffer_s {
    pthread_mutex_t mutex;
    pthread_cond_t  cond;
    outputBuffer_state_t state;
    char buffer[bufferSizeMax];
} outputBuffer_t;

// Will dynamically create the number of buffers (array elements) based on OS version.


typedef struct {
    pthread_t playbackThread;
    char threadShouldRun;
    char threadIsReady;
    int lastBufferEnqueued;
    int lastBufferMixed;

    outputBuffer_t *outputBuffers;

    // buffer queue player interfaces
    SLObjectItf bqPlayerObject;
    SLPlayItf bqPlayerPlay;
    SLAndroidSimpleBufferQueueItf bqPlayerBufferQueue;
} opesles_data_t;
#define MAX_DEVICES 3
static ALCdevice *deviceList[MAX_DEVICES] = {NULL};
static pthread_mutex_t deviceListMutex = PTHREAD_MUTEX_INITIALIZER;

typedef void (*deviceListFn)(ALCdevice *);

static void devlist_add(ALCdevice *pDevice) {
    int i;
    pthread_mutex_lock(&(deviceListMutex));
    for (i = 0; i < MAX_DEVICES; i++) {
        if (deviceList[i] == pDevice) {
            break;
        } else if (deviceList[i] == NULL) {
            deviceList[i] = pDevice;
            break;
        }
    }
    pthread_mutex_unlock(&(deviceListMutex));
}

static void devlist_remove(ALCdevice *pDevice) {
    int i;
    pthread_mutex_lock(&(deviceListMutex));
    for (i = 0; i < MAX_DEVICES; i++) {
        if (deviceList[i] == pDevice) {
            deviceList[i] = NULL;
        }
    }
    pthread_mutex_unlock(&(deviceListMutex));
}

static void devlist_process(deviceListFn mapFunction) {
    int i;
    pthread_mutex_lock(&(deviceListMutex));    
    for (i = 0; i < MAX_DEVICES; i++) {
        if (deviceList[i]) {
            pthread_mutex_unlock(&(deviceListMutex));
            mapFunction(deviceList[i]);
            pthread_mutex_lock(&(deviceListMutex));
        }
    }
    pthread_mutex_unlock(&(deviceListMutex));
}


static void *playback_function(void * context) {
    LOGV("playback_function started");
    outputBuffer_t *buffer = NULL;
    SLresult result;
    struct timespec ts;
    assert(NULL != context);
    ALCdevice *pDevice = (ALCdevice *) context;
    opesles_data_t *devState = (opesles_data_t *) pDevice->ExtraData;
    unsigned int bufferIndex = devState->lastBufferMixed;

    ALint frameSize = FrameSizeFromDevFmt(pDevice->FmtChans, pDevice->FmtType);

    // Show a sensible name for the thread in debug tools
    prctl(PR_SET_NAME, (unsigned long)"OpenAL/sl/m", 0, 0, 0);

    while (1) {
        if (devState->threadShouldRun == 0) {
            return NULL;
        }

        bufferIndex = (++bufferIndex) % bufferCount;
        buffer = &(devState->outputBuffers[bufferIndex]);

        pthread_mutex_lock(&(buffer->mutex));


        while (1) {
            if (devState->threadShouldRun == 0) {
                pthread_mutex_unlock(&(buffer->mutex));
                return NULL;
            }

            // This is a little hacky, but here we avoid mixing too much data            
            if (buffer->state == OUTPUT_BUFFER_STATE_FREE) {
                unsigned int offset = (unsigned int)(premixCount % bufferCount);
                unsigned int i = (bufferIndex + (unsigned int)bufferCount - offset) % (unsigned int)bufferCount;
                outputBuffer_t *buffer1 = &(devState->outputBuffers[i]);
                if (buffer1->state == OUTPUT_BUFFER_STATE_ENQUEUED ||
                    buffer1->state == OUTPUT_BUFFER_STATE_FREE) {
                    break;
                }
            } 

            // No buffer available, wait for a buffer to become available
            // or until playback is stopped/suspended
            clock_gettime(CLOCK_REALTIME, &ts);
            ts.tv_nsec += 5000000;
            if (ts.tv_nsec >= 1000000000L) {
                ts.tv_sec += ts.tv_nsec / 1000000000L;
                ts.tv_nsec %= 1000000000L;
            }
            pthread_cond_timedwait(&(buffer->cond), &(buffer->mutex), &ts);
        }
        devState->threadIsReady = 1;

        aluMixData(pDevice, buffer->buffer, bufferSize/frameSize);
        buffer->state = OUTPUT_BUFFER_STATE_MIXED;
        pthread_cond_signal(&(buffer->cond));
        pthread_mutex_unlock(&(buffer->mutex));

        devState->lastBufferMixed = bufferIndex;
    }
}

SLresult alc_opensles_init_extradata(ALCdevice *pDevice)
{
    opesles_data_t *devState = NULL;
    int i;
    devState = malloc(sizeof(opesles_data_t));
    if (!devState) {
        return SL_RESULT_MEMORY_FAILURE;
    }
    memset(devState, 0, sizeof(opesles_data_t));
    devState->outputBuffers = (outputBuffer_t*) malloc(sizeof(outputBuffer_t)*bufferCount);
    if (!devState->outputBuffers) {
        free(devState);
        return SL_RESULT_MEMORY_FAILURE;
    }
    pDevice->ExtraData = devState;
    memset(devState->outputBuffers, 0, sizeof(outputBuffer_t)*bufferCount);
    devState->lastBufferEnqueued = -1;
    devState->lastBufferMixed = -1;
    for (i = 0; i < bufferCount; i++) {
        if (pthread_mutex_init(&(devState->outputBuffers[i].mutex), (pthread_mutexattr_t*) NULL) != 0) {
            LOGV("Error on init of mutex");
            free(devState->outputBuffers);
            free(devState);
            return SL_RESULT_UNKNOWN_ERROR;
        }
        if (pthread_cond_init(&(devState->outputBuffers[i].cond), (pthread_condattr_t*) NULL) != 0) {
            LOGV("Error on init of cond");
            free(devState->outputBuffers);
            free(devState);
            return SL_RESULT_UNKNOWN_ERROR;
        }
        devState->outputBuffers[i].state = OUTPUT_BUFFER_STATE_FREE;
    }
    // For the Android suspend/resume functionaly, keep track of all device contexts
    devlist_add(pDevice);
    return SL_RESULT_SUCCESS;
}

static void start_playback(ALCdevice *pDevice) {
    opesles_data_t *devState = NULL;

    if (pDevice->ExtraData == NULL) {
        if (alc_opensles_init_extradata(pDevice) != SL_RESULT_SUCCESS) {
            LOGE("start_playback failed to init extradata");
            return;
        }
        devState = (opesles_data_t *) pDevice->ExtraData;
        if (!devState) {
            return;
        }
    } else {
        devState = (opesles_data_t *) pDevice->ExtraData;
    }

    if (devState->threadShouldRun == 1) {
        // Gratuitous resume
        return;
    }

    // start/restart playback thread
    devState->threadIsReady = 0;
    devState->threadShouldRun = 1;

    pthread_attr_t playbackThreadAttr;
    pthread_attr_init(&playbackThreadAttr);
    struct sched_param playbackThreadParam;
    playbackThreadParam.sched_priority = sched_get_priority_max(SCHED_RR);
    pthread_attr_setschedpolicy(&playbackThreadAttr, SCHED_RR);
    pthread_attr_setschedparam(&playbackThreadAttr, &playbackThreadParam);
    int rc = pthread_create(&(devState->playbackThread), &playbackThreadAttr, playback_function, (void *) pDevice);
    pthread_attr_destroy(&playbackThreadAttr);
    if (rc != 0) {
        // Unprivileged Android processes cannot acquire SCHED_RR; fall back to normal scheduling
        rc = pthread_create(&(devState->playbackThread), NULL, playback_function, (void *) pDevice);
        if (rc != 0) {
            LOGE("Failed to start OpenSL ES playback thread (rc=%d)", rc);
            devState->threadShouldRun = 0;
            return;
        }
    }
    while (devState->threadShouldRun && (0 == devState->threadIsReady))
    {
        sched_yield();
    }
}

static void stop_playback(ALCdevice *pDevice) {
    opesles_data_t *devState = (opesles_data_t *) pDevice->ExtraData;
    if (!devState || devState->threadShouldRun == 0) {
        return;
    }
    devState->threadShouldRun = 0;
    pthread_join(devState->playbackThread, NULL);
    devState->threadIsReady = 0;
    return;
}

// this callback handler is called every time a buffer finishes playing
static void opensles_callback(SLAndroidSimpleBufferQueueItf bq, void *context)
{
    ALCdevice *pDevice = (ALCdevice *) context;
    opesles_data_t *devState = (opesles_data_t *) pDevice->ExtraData;
    unsigned int bufferIndex = devState->lastBufferEnqueued;
    unsigned int i;
    struct timespec ts;
    int rc;
    SLresult result;
    outputBuffer_t *buffer = NULL;

    bufferIndex = (++bufferIndex) % bufferCount;
    buffer = &(devState->outputBuffers[bufferIndex]);

    pthread_mutex_lock(&(buffer->mutex));
    // We will block until 'next' buffer has mixed audio, but first flag oldest equeued buffer as free 
    for (i = 1; i <= bufferCount; i++) {
        unsigned int j = (devState->lastBufferEnqueued+i) % bufferCount;
        outputBuffer_t *bufferFree = &(devState->outputBuffers[j]);
        if (bufferFree->state == OUTPUT_BUFFER_STATE_ENQUEUED) {
            bufferFree->state = OUTPUT_BUFFER_STATE_FREE;
            break;
        } 
    }
    while (buffer->state != OUTPUT_BUFFER_STATE_MIXED) {
        clock_gettime(CLOCK_REALTIME, &ts);
        ts.tv_nsec += 100000;
        if (ts.tv_nsec >= 1000000000L) {
            ts.tv_sec += ts.tv_nsec / 1000000000L;
            ts.tv_nsec %= 1000000000L;
        }
        rc = pthread_cond_timedwait(&(buffer->cond), &(buffer->mutex), &ts);
        if (rc != 0) {
            if (devState->threadShouldRun == 0) {
                // we are probably suspended
                pthread_mutex_unlock(&(buffer->mutex));
                return;
            }
        }
    }

    if (devState->bqPlayerBufferQueue) {
        result = (*devState->bqPlayerBufferQueue)->Enqueue(devState->bqPlayerBufferQueue, buffer->buffer, bufferSize);
        if (SL_RESULT_SUCCESS == result) {
            buffer->state = OUTPUT_BUFFER_STATE_ENQUEUED;
            devState->lastBufferEnqueued = bufferIndex;
            pthread_cond_signal(&(buffer->cond));
        } else {
            bufferIndex--;
        }
    }
    pthread_mutex_unlock(&(buffer->mutex));
}


static const ALCchar opensles_device[] = "OpenSL ES";

// Apportable extensions
SLresult alc_opensles_create_native_audio_engine()
{
    if (engineObject && engineEngine && outputMixObject)
        return SL_RESULT_SUCCESS;

    SLresult result;

    // create engine
    if (!engineObject) {
        result = pslCreateEngine(&engineObject, 0, NULL, 0, NULL, NULL);
        if (SL_RESULT_SUCCESS != result || engineObject == NULL) {
            LOGE("slCreateEngine failed: 0x%x", (unsigned int)result);
            engineObject = NULL;
            return result != SL_RESULT_SUCCESS ? result : SL_RESULT_UNKNOWN_ERROR;
        }

        // realize the engine
        result = (*engineObject)->Realize(engineObject, SL_BOOLEAN_FALSE);
        if (SL_RESULT_SUCCESS != result) {
            LOGE("engineObject->Realize failed: 0x%x", (unsigned int)result);
            (*engineObject)->Destroy(engineObject);
            engineObject = NULL;
            return result;
        }
    }

    // get the engine interface, which is needed in order to create other objects
    if (!engineEngine) {
        result = (*engineObject)->GetInterface(engineObject, *pSL_IID_ENGINE, &engineEngine);
        if (SL_RESULT_SUCCESS != result || engineEngine == NULL) {
            LOGE("engineObject->GetInterface(ENGINE) failed: 0x%x", (unsigned int)result);
            return result != SL_RESULT_SUCCESS ? result : SL_RESULT_UNKNOWN_ERROR;
        }
    }

    // create output mix
    if (!outputMixObject) {
        result = (*engineEngine)->CreateOutputMix(engineEngine, &outputMixObject, 0, NULL, NULL);
        if (SL_RESULT_SUCCESS != result || outputMixObject == NULL) {
            LOGE("CreateOutputMix failed: 0x%x", (unsigned int)result);
            outputMixObject = NULL;
            return result != SL_RESULT_SUCCESS ? result : SL_RESULT_UNKNOWN_ERROR;
        }

        // realize the output mix
        result = (*outputMixObject)->Realize(outputMixObject, SL_BOOLEAN_FALSE);
        if (SL_RESULT_SUCCESS != result) {
            LOGE("outputMixObject->Realize failed: 0x%x", (unsigned int)result);
            (*outputMixObject)->Destroy(outputMixObject);
            outputMixObject = NULL;
            return result;
        }
    }

    return SL_RESULT_SUCCESS;
}

// Backend functions, in same order as type BackendFuncs
static ALCboolean opensles_open_playback(ALCdevice *pDevice, const ALCchar *deviceName)
{
    LOGV("opensles_open_playback pDevice=%p, deviceName=%s", pDevice, deviceName ? deviceName : "(null)");

    // Check if probe has linked the opensl symbols
    if (pslCreateEngine == NULL) {
        alc_opensles_probe(DEVICE_PROBE);
        if (pslCreateEngine == NULL) {
            LOGE("opensles_open_playback failed: OpenSL ES symbols not resolved");
            return ALC_FALSE;
        }
    }

    if (pDevice->ExtraData == NULL) {
        if (alc_opensles_init_extradata(pDevice) != SL_RESULT_SUCCESS) {
            return ALC_FALSE;
        }
    }

    // create the engine and output mix objects
    if (alc_opensles_create_native_audio_engine() != SL_RESULT_SUCCESS) {
        return ALC_FALSE;
    }

    if (!pDevice->szDeviceName) {
        pDevice->szDeviceName = strdup(opensles_device);
    }

    return ALC_TRUE;
}


static void opensles_close_playback(ALCdevice *pDevice)
{
    LOGV("opensles_close_playback pDevice=%p", pDevice);
    opesles_data_t *devState = (opesles_data_t *) pDevice->ExtraData;

    if (devState != NULL) {
        stop_playback(pDevice);

        // destroy buffer queue audio player object, and invalidate all associated interfaces
        if (devState->bqPlayerObject != NULL) {
            (*devState->bqPlayerObject)->Destroy(devState->bqPlayerObject);
            devState->bqPlayerObject = NULL;
            devState->bqPlayerPlay = NULL;
            devState->bqPlayerBufferQueue = NULL;
        }
    }

    devlist_remove(pDevice);
}

static ALCboolean opensles_reset_playback(ALCdevice *pDevice)
{
    if (pDevice == NULL) {
        LOGE("Received a NULL ALCdevice! Returning ALC_FALSE from opensles_reset_playback");
        return ALC_FALSE;
    }
    LOGV("opensles_reset_playback pDevice=%p", pDevice);
    opesles_data_t *devState;
    pDevice->FmtChans = DevFmtStereo;
    pDevice->FmtType = DevFmtShort;
    unsigned bits = BytesFromDevFmt(pDevice->FmtType) * 8;
    unsigned channels = ChannelsFromDevFmt(pDevice->FmtChans);
    unsigned samples = pDevice->UpdateSize;
    unsigned size = samples * channels * bits / 8;
	SLuint32 sampling_rate = pDevice->Frequency * 1000;
	SLresult result;
    LOGV("bits=%u, channels=%u, samples=%u, size=%u, freq=%u", bits, channels, samples, size, pDevice->Frequency);
    if (pDevice->Frequency <= 22050) {
        bufferSize = defaultBufferSize / 2;
    }

    devState = (opesles_data_t *) pDevice->ExtraData;
    if (!devState) {
        LOGE("opensles_reset_playback: ExtraData is NULL");
        return ALC_FALSE;
    }

    // create buffer queue audio player

    // configure audio source
    SLDataLocator_AndroidSimpleBufferQueue loc_bufq = {SL_DATALOCATOR_ANDROIDSIMPLEBUFFERQUEUE, 2};
//    SLDataFormat_PCM format_pcm = {SL_DATAFORMAT_PCM, 2, SL_SAMPLINGRATE_44_1,
    SLDataFormat_PCM format_pcm = {SL_DATAFORMAT_PCM, 2, sampling_rate,
        SL_PCMSAMPLEFORMAT_FIXED_16, SL_PCMSAMPLEFORMAT_FIXED_16,
        SL_SPEAKER_FRONT_LEFT|SL_SPEAKER_FRONT_RIGHT, SL_BYTEORDER_LITTLEENDIAN};
    SLDataSource audioSrc = {&loc_bufq, &format_pcm};

    // configure audio sink
    SLDataLocator_OutputMix loc_outmix = {SL_DATALOCATOR_OUTPUTMIX, outputMixObject};
    SLDataSink audioSnk = {&loc_outmix, NULL};

    // create audio player
    LOGV("create audio player");
    const SLInterfaceID ids[1] = {*pSL_IID_ANDROIDSIMPLEBUFFERQUEUE};
    const SLboolean req[1] = {SL_BOOLEAN_TRUE};
    result = (*engineEngine)->CreateAudioPlayer(engineEngine, &devState->bqPlayerObject, &audioSrc, &audioSnk,
        1, ids, req);
    if ((result != SL_RESULT_SUCCESS) || (devState->bqPlayerObject == NULL)) {
        LOGE("create audio player is null or errored: 0x%x", (unsigned int)result);
        return ALC_FALSE;
    }

    // realize the player
    result = (*devState->bqPlayerObject)->Realize(devState->bqPlayerObject, SL_BOOLEAN_FALSE);
    if (SL_RESULT_SUCCESS != result) {
        LOGE("bqPlayerObject->Realize failed: 0x%x", (unsigned int)result);
        return ALC_FALSE;
    }

    // get the play interface
    result = (*devState->bqPlayerObject)->GetInterface(devState->bqPlayerObject, *pSL_IID_PLAY, &devState->bqPlayerPlay);
    if ((SL_RESULT_SUCCESS != result) || (devState->bqPlayerPlay == NULL)) {
        LOGE("bqPlayerObject->GetInterface(PLAY) failed: 0x%x", (unsigned int)result);
        return ALC_FALSE;
    }

    // get the buffer queue interface (use ANDROIDSIMPLEBUFFERQUEUE matching ids[0]; fall back to BUFFERQUEUE)
    result = (*devState->bqPlayerObject)->GetInterface(devState->bqPlayerObject, *pSL_IID_ANDROIDSIMPLEBUFFERQUEUE,
            &devState->bqPlayerBufferQueue);
    if ((result != SL_RESULT_SUCCESS) || (devState->bqPlayerBufferQueue == NULL)) {
        result = (*devState->bqPlayerObject)->GetInterface(devState->bqPlayerObject, *pSL_IID_BUFFERQUEUE,
                &devState->bqPlayerBufferQueue);
    }
    if ((result != SL_RESULT_SUCCESS) || (devState->bqPlayerBufferQueue == NULL)) {
        LOGE("get the buffer queue interface is null or errored: 0x%x", (unsigned int)result);
        return ALC_FALSE;
    }

    // register callback on the buffer queue
    result = (*devState->bqPlayerBufferQueue)->RegisterCallback(devState->bqPlayerBufferQueue, opensles_callback, (void *) pDevice);
    if (SL_RESULT_SUCCESS != result) {
        LOGE("RegisterCallback failed: 0x%x", (unsigned int)result);
        return ALC_FALSE;
    }

    // playback_lock = createThreadLock();
    start_playback(pDevice);

    // set the player's state to playing
    result = (*devState->bqPlayerPlay)->SetPlayState(devState->bqPlayerPlay, SL_PLAYSTATE_PLAYING);
    if (SL_RESULT_SUCCESS != result) {
        LOGE("SetPlayState(PLAYING) failed: 0x%x", (unsigned int)result);
        return ALC_FALSE;
    }

    // enqueue one silent stereo 16-bit PCM frame (4 bytes) to kick off the callbacks
    static const SLuint32 silenceFrame = 0;
    result = (*devState->bqPlayerBufferQueue)->Enqueue(devState->bqPlayerBufferQueue, &silenceFrame, sizeof(silenceFrame));
    if (SL_RESULT_SUCCESS != result) {
        LOGE("initial Enqueue failed: 0x%x", (unsigned int)result);
        return ALC_FALSE;
    }


    SetDefaultWFXChannelOrder(pDevice);
    devlist_add(pDevice);

    return ALC_TRUE;
}


static void opensles_stop_playback(ALCdevice *pDevice)
{
    LOGV("opensles_stop_playback device=%p", pDevice);
    stop_playback(pDevice);
}

static ALCboolean opensles_open_capture(ALCdevice *pDevice, const ALCchar *deviceName)
{
    LOGV("opensles_open_capture  device=%p, deviceName=%s", pDevice, deviceName);
    return ALC_FALSE;
}

static void opensles_close_capture(ALCdevice *pDevice)
{
    LOGV("opensles_closed_capture device=%p", pDevice);
}

static void opensles_start_capture(ALCdevice *pDevice)
{
    LOGV("opensles_start_capture device=%p", pDevice);
}

static void opensles_stop_capture(ALCdevice *pDevice)
{
    LOGV("opensles_stop_capture device=%p", pDevice);
}

static void opensles_capture_samples(ALCdevice *pDevice, ALCvoid *pBuffer, ALCuint lSamples)
{
    LOGV("opensles_capture_samples device=%p, pBuffer=%p, lSamples=%u", pDevice, pBuffer, lSamples);
}

static ALCuint opensles_available_samples(ALCdevice *pDevice)
{
    LOGV("opensles_available_samples device=%p", pDevice);
    return 0;
}

// table of backend function pointers

BackendFuncs opensles_funcs = {
    opensles_open_playback,
    opensles_close_playback,
    opensles_reset_playback,
    opensles_stop_playback,
    opensles_open_capture,
    opensles_close_capture,
    opensles_start_capture,
    opensles_stop_capture,
    opensles_capture_samples,
    opensles_available_samples
};

// global entry points called from XYZZY

 
static void suspend_device(ALCdevice *pDevice) {
    SLresult result;
    if (pDevice) {
        opesles_data_t *devState = (opesles_data_t *) pDevice->ExtraData;
        if (devState && devState->bqPlayerPlay) {
            result = (*devState->bqPlayerPlay)->SetPlayState(devState->bqPlayerPlay, SL_PLAYSTATE_PAUSED);
            if ((SL_RESULT_SUCCESS == result) && (devState->bqPlayerBufferQueue)) {
                (*devState->bqPlayerBufferQueue)->Clear(devState->bqPlayerBufferQueue);
            }
        }
        stop_playback(pDevice);
    }
}

static void resume_device(ALCdevice *pDevice) {
    SLresult result;
    if (pDevice) {
        opesles_data_t *devState = (opesles_data_t *) pDevice->ExtraData;
        if (devState && devState->bqPlayerPlay) {
            result = (*devState->bqPlayerPlay)->SetPlayState(devState->bqPlayerPlay, SL_PLAYSTATE_PLAYING);
            // Pump one silent stereo 16-bit PCM frame to stimulate the callback
            if ((SL_RESULT_SUCCESS == result) && (devState->bqPlayerBufferQueue)) {
                static const SLuint32 silenceFrame = 0;
                (*devState->bqPlayerBufferQueue)->Enqueue(devState->bqPlayerBufferQueue, &silenceFrame, sizeof(silenceFrame));
            }
        }
        start_playback(pDevice);
     }
 }

void alc_opensles_suspend()
{
    devlist_process(&suspend_device);
}
 
void alc_opensles_resume()
{
    devlist_process(&resume_device);
}

static void alc_opensles_set_java_vm(JavaVM *vm)
{
    // Called once and only once from JNI_OnLoad
    javaVM = vm;
    int i;
    char *android_model;
    char *low_buffer_models[] = {
        "GT-I9300",
        "GT-I9305",
        "SHV-E210",
        "SGH-T999",
        "SGH-I747",
        "SGH-N064",
        "SC-06D",
        "SGH-N035",
        "SC-03E",
        "SCH-R530",
        "SCH-I535",
        "SPH-L710",
        "GT-I9308",
        "SCH-I939",
        "Kindle Fire",
        NULL};

	if(NULL != javaVM)
	{
		int android_os_version = alc_opensles_get_android_api();
		// If running on 4.1 (Jellybean) or later, use 8 buffers to avoid breakup/stuttering.
		if(android_os_version >= 16)
		{
			premixCount = 5;
		}
		// Else, use 4 buffers to reduce latency
		else if (android_os_version > 0)
		{
			premixCount = 1;
		}
        android_model = alc_opensles_get_android_model();
        if (android_model != NULL) {
            for (i = 0; low_buffer_models[i] != NULL; i++) {
                if (strncmp(android_model, low_buffer_models[i], strlen(low_buffer_models[i])) == 0) {
                    LOGV("Using less buffering");
                    defaultBufferSize = 1024;
                    bufferSize = 1024;
                    premixCount = 1;
                    break;
                }
            }
        }
	}
}

void alc_opensles_init(BackendFuncs *func_list)
{
    LOGV("alc_opensles_init");

    *func_list = opensles_funcs;

	// We need the JavaVM for JNI so we can detect the OS version number at runtime.
	// This is because we need to use different bufferCount values for Android 4.1 vs. pre-4.1.
	// This must be set at constructor time before JNI_OnLoad is invoked.
	apportableOpenALFuncs.alc_android_set_java_vm = alc_opensles_set_java_vm;
	apportableOpenALFuncs.alc_android_suspend = alc_opensles_suspend;
	apportableOpenALFuncs.alc_android_resume = alc_opensles_resume;
}

void alc_opensles_deinit(void)
{
    LOGV("alc_opensles_deinit");

    // destroy output mix object, and invalidate all associated interfaces
    if (outputMixObject != NULL) {
        (*outputMixObject)->Destroy(outputMixObject);
        outputMixObject = NULL;
    }

    // destroy engine object, and invalidate all associated interfaces
    if (engineObject != NULL) {
        (*engineObject)->Destroy(engineObject);
        engineObject = NULL;
        engineEngine = NULL;
    }
}

void alc_opensles_probe(int type)
{
    char *error;

    if (pslCreateEngine == NULL) {
        dlerror(); // Clear dl errors
        void *dlHandle = dlopen("libOpenSLES.so", RTLD_NOW | RTLD_GLOBAL);
        if (!dlHandle || (error = (typeof(error))dlerror()) != NULL) {
            LOGE("OpenSLES could not be loaded: %s", error ? error : "(unknown)");
            return;
        }

#define LOAD_SYM_POINTER(sym) \
        do { \
            dlerror(); \
            p##sym = dlsym(dlHandle, #sym); \
            if((error=(typeof(error))dlerror()) != NULL) { \
                LOGE("alc_opensles_probe could not load %s, error: %s", #sym, error); \
                dlclose(dlHandle); \
                return; \
            } \
        } while(0)

        LOAD_SYM_POINTER(slCreateEngine);
        LOAD_SYM_POINTER(SL_IID_ENGINE);
        LOAD_SYM_POINTER(SL_IID_ANDROIDSIMPLEBUFFERQUEUE);
        LOAD_SYM_POINTER(SL_IID_PLAY);
        LOAD_SYM_POINTER(SL_IID_BUFFERQUEUE);
#undef LOAD_SYM_POINTER
    }

    apportableOpenALFuncs.alc_android_suspend = alc_opensles_suspend;
    apportableOpenALFuncs.alc_android_resume = alc_opensles_resume;

    switch (type) {
    case DEVICE_PROBE:
        LOGV("alc_opensles_probe DEVICE_PROBE");
        AppendDeviceList(opensles_device);
        break;
    case ALL_DEVICE_PROBE:
        LOGV("alc_opensles_probe ALL_DEVICE_PROBE");
        AppendAllDeviceList(opensles_device);
        break;
    default:
        LOGV("alc_opensles_probe type=%d", type);
        break;
    }
}
