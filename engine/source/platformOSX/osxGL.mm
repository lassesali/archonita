//-----------------------------------------------------------------------------
// Copyright (c) 2013 GarageGames, LLC
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

// Many OSX frameworks include OpenTransport, and OT's new operator conflicts 
// with our redefinition of 'new', so we have to pre-include platformMacCarb.h,
// which contains the workaround.
#include <OpenGL/OpenGL.h>
#include "platformOSX/platformOSX.h"
#include "platformOSX/platformGL.h"
#include "console/console.h"
#include "graphics/dgl.h"

GLState gGLState;

bool gOpenGLDisablePT          = false;
bool gOpenGLDisableCVA         = false;
bool gOpenGLDisableTEC         = false;
bool gOpenGLDisableARBMT       = false;
bool gOpenGLDisableFC          = true; //false;
bool gOpenGLDisableTCompress   = false;
bool gOpenGLNoEnvColor         = false;
float gOpenGLGammaCorrection   = 0.5;
bool gOpenGLNoDrawArraysAlpha  = false;

//-----------------------------------------------------------------------------
/// These stubs are legacy stuff for the d3d wrapper layer.
// The code that requires these stubs should probably be ifdef'd out of any non w32 build
//-----------------------------------------------------------------------------
GLboolean glAvailableVertexBufferEXT() {   return(false); }
GLint glAllocateVertexBufferEXT(GLsizei size, GLint format, GLboolean preserve) {   return(0); }
void* glLockVertexBufferEXT(GLint handle, GLsizei size) {   return(NULL); }
void glUnlockVertexBufferEXT(GLint handle) {}
void glSetVertexBufferEXT(GLint handle) {}
void glOffsetVertexBufferEXT(GLint handle, GLuint offset) {}
void glFillVertexBufferEXT(GLint handle, GLint first, GLsizei count) {}
void glFreeVertexBufferEXT(GLint handle) {}

//-----------------------------------------------------------------------------
/// Change FSAA level.
/// Currently, this will only work for ATI Radeon chipsets.
/// On other chipsets, this has no effect.
/// Pass in 1 for normal/no-AA, 2 for 2x-AA, 4 for 4x-AA.
//-----------------------------------------------------------------------------
void dglSetFSAASamples(GLint aasamp)
{
    if (gGLState.maxFSAASamples < 2)
        return;
    
    // fix out of range values
    if (aasamp < 1)
        aasamp = 1;
    else if (aasamp == 3)
        aasamp = 2;
    else if (aasamp > 4)
        aasamp = 4;
    
    /* 
    THIS IS CARBON
     aglSetInteger(platState.ctx, ATI_FSAA_LEVEL, &aasamp);
    
    IT NEEDS OT BE REPLACED WITH SOMETHING LIKE THIS:
     const char *glRenderer = (const char *) glGetString(GL_RENDERER);
     
     if (strstr(glRenderer, "ATI"))
     {
        NSOpenGLContext* testContext; /// <<< Need to get ahold of the current context
       [testContext setValues:&aasamp forParameter:ATI_FSAA_LEVEL];
     }
     */
    
    Con::printf(">>Number of FSAA samples is now [%d].", aasamp);
    Con::setIntVariable("$pref::OpenGL::numFSAASamples", aasamp);
}


//-----------------------------------------------------------------------------
// helper function for getGLCapabilities.
//  returns a new CGL context for platState.hDisplay
CGLContextObj getContextForCapsCheck()
{   
    // silently create an opengl context on the current display, so that
    // we can get valid renderer and capability info
    // some of the following code is from:
    //  http://developer.apple.com/technotes/tn2002/tn2080.html#TAN55
   
    osxPlatState * platState = [osxPlatState sharedPlatState];
    
    // From the CG display id, we can create a pixel format & context
    // and with that context we can check opengl capabilities
    CGOpenGLDisplayMask cglDisplayMask = CGDisplayIDToOpenGLDisplayMask([platState cgDisplay]);
    CGLPixelFormatAttribute attribs[] = {
        kCGLPFAOpenGLProfile, (CGLPixelFormatAttribute)kCGLOGLPVersion_GL4_Core,
        kCGLPFADisplayMask, (CGLPixelFormatAttribute)cglDisplayMask,
        (CGLPixelFormatAttribute)0
    };

    // create the pixel format. we will not use numPixelFormats, but the docs
    // do not say we're allowed to pass NULL for the 3rd arg.
    GLint numPixelFormats = 0;
    CGLPixelFormatObj pixelFormat = NULL;
    CGLChoosePixelFormat(attribs, &pixelFormat, &numPixelFormats);
    if (!pixelFormat)
    {
        attribs[1] = (CGLPixelFormatAttribute)kCGLOGLPVersion_3_2_Core;
        CGLChoosePixelFormat(attribs, &pixelFormat, &numPixelFormats);
    }
    if (!pixelFormat)
    {
        CGLPixelFormatAttribute legacyAttribs[] = {
            kCGLPFADisplayMask, (CGLPixelFormatAttribute)cglDisplayMask,
            (CGLPixelFormatAttribute)0
        };
        CGLChoosePixelFormat(legacyAttribs, &pixelFormat, &numPixelFormats);
    }

    if (pixelFormat)
    {
        // create a context. we don't need the pixel format once the context is created.
        CGLContextObj ctx;
        CGLCreateContext(pixelFormat, NULL, &ctx);
        CGLDestroyPixelFormat(pixelFormat);
        
        if (ctx)
            return ctx;
    }
    
    // if we can't get a good context, we can't check caps... this won't be good.
    Con::errorf("getContextForCapsCheck could not create a cgl context on the display for gl capabilities checking!");
    return NULL;
}


static bool hasGLExtension(const char* pExtString, const char* extName)
{
    if (gGLState.glMajorVersion >= 3)
    {
        GLint numExt = 0;
        glGetIntegerv(GL_NUM_EXTENSIONS, &numExt);
        for (GLint i = 0; i < numExt; ++i)
        {
            const char* ext = (const char*) glGetStringi(GL_EXTENSIONS, (GLuint)i);
            if (ext && dStrcmp(ext, extName) == 0)
                return true;
        }
        return false;
    }
    return (pExtString && dStrstr(pExtString, extName) != NULL);
}

// Find out which extensions are available for this renderer.
void getGLCapabilities()
{
    osxPlatState * platState;
    platState = [osxPlatState sharedPlatState];
    
    AssertFatal([platState cgDisplay], "getGLCapabilities() was called before a monitor was chosen!");
    
    // silently create an opengl context on the current display,
    // so that we can get valid renderer and capability info.
    // we save off the current context so that we can silently restore it.
    // the user should not be aware of this little shuffle.
    CGLContextObj curr_ctx = CGLGetCurrentContext ();
    CGLContextObj temp_ctx =  getContextForCapsCheck();
   
    if (!temp_ctx)
    {
        Con::errorf("OpenGL may not be set up correctly!");
        return;
    }
   
    CGLSetCurrentContext(temp_ctx);
    
    // Get the OpenGL info strings we'll need
    const char* pVendString = (const char*) glGetString(GL_VENDOR);
    const char* pRendString = (const char*) glGetString(GL_RENDERER);
    const char* pVersString = (const char*) glGetString(GL_VERSION);
    
    // Output some driver info to the console:
    Con::printf("OpenGL driver information:");
    if (pVendString)
        Con::printf("  Vendor: %s", pVendString);
    if (pRendString)
        Con::printf("  Renderer: %s", pRendString);
    if (pVersString)
        Con::printf("  Version: %s", pVersString);
    
    // pre-clear the structure
    dMemset(&gGLState, 0, sizeof(gGLState));

    gGLState.glMajorVersion = 1;
    gGLState.glMinorVersion = 1;
    if (pVersString && pVersString[0] >= '3')
    {
        glGetIntegerv(GL_MAJOR_VERSION, &gGLState.glMajorVersion);
        glGetIntegerv(GL_MINOR_VERSION, &gGLState.glMinorVersion);
        if (gGLState.glMajorVersion > 3 || (gGLState.glMajorVersion == 3 && gGLState.glMinorVersion >= 2))
        {
            GLint profileMask = 0;
            glGetIntegerv(GL_CONTEXT_PROFILE_MASK, &profileMask);
            gGLState.isCoreProfile = ((profileMask & GL_CONTEXT_CORE_PROFILE_BIT) != 0)
                                  || (dStrstr(pVersString, "Core") != NULL);
        }
    }
    gGLState.suppDSA = false;

    // In OpenGL 3.0+ Core Profile, glGetString(GL_EXTENSIONS) is invalid (GL_INVALID_ENUM).
    const char* pExtString = (gGLState.glMajorVersion < 3)
        ? (const char*) glGetString(GL_EXTENSIONS)
        : NULL;

    gGLState.suppTexStorage = (gGLState.glMajorVersion > 4 || (gGLState.glMajorVersion == 4 && gGLState.glMinorVersion >= 2))
                           || hasGLExtension(pExtString, "GL_ARB_texture_storage");

    // EXT_paletted_texture ========================================
    if (hasGLExtension(pExtString, "GL_EXT_paletted_texture"))
        gGLState.suppPalettedTexture = true;

    // EXT_compiled_vertex_array ========================================
    gGLState.suppLockedArrays = false;
    if (hasGLExtension(pExtString, "GL_EXT_compiled_vertex_array"))
        gGLState.suppLockedArrays = true;

    // ARB_multitexture ========================================
    if ((gGLState.glMajorVersion >= 2) || hasGLExtension(pExtString, "GL_ARB_multitexture"))
        gGLState.suppARBMultitexture = true;

    // EXT_blend_color
    if ((gGLState.glMajorVersion >= 2) || hasGLExtension(pExtString, "GL_EXT_blend_color"))
        gGLState.suppEXTblendcolor = true;

    // EXT_blend_minmax
    if ((gGLState.glMajorVersion >= 2) || hasGLExtension(pExtString, "GL_EXT_blend_minmax"))
        gGLState.suppEXTblendminmax = true;

    // NV_vertex_array_range ========================================
    // does not appear to be supported by apple, at all. (as of 10.4.3)
    // GL_APPLE_vertex_array_range is similar, and may be nearly identical.
    if (hasGLExtension(pExtString, "GL_NV_vertex_array_range"))
        gGLState.suppVertexArrayRange = true;

    // EXT_fog_coord ========================================
    if (hasGLExtension(pExtString, "GL_EXT_fog_coord"))
        gGLState.suppFogCoord = true;

    // ARB_texture_compression ========================================
    if ((gGLState.glMajorVersion >= 3) || hasGLExtension(pExtString, "GL_ARB_texture_compression"))
        gGLState.suppTextureCompression = true;

    // 3DFX_texture_compression_FXT1 ========================================
    if (hasGLExtension(pExtString, "GL_3DFX_texture_compression_FXT1"))
        gGLState.suppFXT1 = true;

    // EXT_texture_compression_S3TC ========================================
    if (hasGLExtension(pExtString, "GL_EXT_texture_compression_s3tc"))
        gGLState.suppS3TC = true;

    // EXT_vertex_buffer ========================================
    // This extension is deprecated, and not supported by Apple. (10.4.3)
    // Instead, the ARB Vertex Buffer extension is supported.
    // The new extension has a different API, so TGE should be updated to use it.
    if (hasGLExtension(pExtString, "GL_EXT_vertex_buffer"))
        gGLState.suppVertexBuffer = true;

    // Anisotropic filtering ========================================
    gGLState.suppTexAnisotropic = hasGLExtension(pExtString, "GL_EXT_texture_filter_anisotropic");
    if (gGLState.suppTexAnisotropic)
        glGetFloatv(GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, &gGLState.maxAnisotropy);

    // Binary states, i.e., no supporting functions  ========================================
    // NOTE:
    // Some of these have multiple representations, via EXT and|or ARB and|or NV and|or SGIS ... etc.
    // Check all relative versions.

    gGLState.suppPackedPixels = (gGLState.glMajorVersion >= 2)
                             || hasGLExtension(pExtString, "GL_EXT_packed_pixels")
                             || hasGLExtension(pExtString, "GL_APPLE_packed_pixel");

    gGLState.suppTextureEnvCombine = hasGLExtension(pExtString, "GL_EXT_texture_env_combine")
                                  || hasGLExtension(pExtString, "GL_ARB_texture_env_combine");

    gGLState.suppEdgeClamp = (gGLState.glMajorVersion >= 2)
                          || hasGLExtension(pExtString, "GL_EXT_texture_edge_clamp")
                          || hasGLExtension(pExtString, "GL_SGIS_texture_edge_clamp")
                          || hasGLExtension(pExtString, "GL_ARB_texture_border_clamp");

    gGLState.suppTexEnvAdd = hasGLExtension(pExtString, "GL_ARB_texture_env_add")
                          || hasGLExtension(pExtString, "GL_EXT_texture_env_add");

    // Texture combine units  ========================================
    if (gGLState.glMajorVersion >= 3)
        glGetIntegerv(GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS, &gGLState.maxTextureUnits);
    else if (gGLState.suppARBMultitexture)
        glGetIntegerv(GL_MAX_TEXTURE_UNITS_ARB, &gGLState.maxTextureUnits);
    else
        gGLState.maxTextureUnits = 1;
    
    // Swap interval ========================================
    // Mac inherently supports a swap interval via AGL-set-integer.
    gGLState.suppSwapInterval = true;
    
    // FSAA support, TODO: check for ARB multisample support
    // multisample support should be checked via CGL
    gGLState.maxFSAASamples = 4;
    
    // dump found extensions to the console...
    Con::printf("OpenGL Init: Enabled Extensions");
    if (gGLState.suppARBMultitexture)
        Con::printf("  ARB_multitexture (Max Texture Units: %d)", gGLState.maxTextureUnits);
    
    if (gGLState.suppEXTblendcolor)
        Con::printf("  EXT_blend_color");
    
    if (gGLState.suppEXTblendminmax)
        Con::printf("  EXT_blend_minmax");
    
    if (gGLState.suppPalettedTexture)
        Con::printf("  EXT_paletted_texture");
    
    if (gGLState.suppLockedArrays)
        Con::printf("  EXT_compiled_vertex_array");
    
    if (gGLState.suppVertexArrayRange)
        Con::printf("  NV_vertex_array_range");
    
    if (gGLState.suppTextureEnvCombine)
        Con::printf("  EXT_texture_env_combine");
    
    if (gGLState.suppPackedPixels)
        Con::printf("  EXT_packed_pixels");
    
    if (gGLState.suppFogCoord)
        Con::printf("  EXT_fog_coord");
    
    if (gGLState.suppTextureCompression)
        Con::printf("  ARB_texture_compression");
    
    if (gGLState.suppS3TC)
        Con::printf("  EXT_texture_compression_s3tc");
    
    if (gGLState.suppFXT1)
        Con::printf("  3DFX_texture_compression_FXT1");
    
    if (gGLState.suppTexEnvAdd)
        Con::printf("  (ARB|EXT)_texture_env_add");
    
    if (gGLState.suppTexAnisotropic)
        Con::printf("  EXT_texture_filter_anisotropic (Max anisotropy: %f)", gGLState.maxAnisotropy);
    
    if (gGLState.suppSwapInterval)
        Con::printf("  Vertical Sync");
    
    if (gGLState.maxFSAASamples)
        Con::printf("  ATI_FSAA");
    
    Con::warnf("OpenGL Init: Disabled Extensions");
    
    if (!gGLState.suppARBMultitexture)
        Con::warnf("  ARB_multitexture");
    
    if (!gGLState.suppEXTblendcolor)
        Con::warnf("  EXT_blend_color");
    
    if (!gGLState.suppEXTblendminmax)
        Con::warnf("  EXT_blend_minmax");
    
    if (!gGLState.suppPalettedTexture)
        Con::warnf("  EXT_paletted_texture");
    
    if (!gGLState.suppLockedArrays)
        Con::warnf("  EXT_compiled_vertex_array");
    
    if (!gGLState.suppVertexArrayRange)
        Con::warnf("  NV_vertex_array_range");
    
    if (!gGLState.suppTextureEnvCombine)
        Con::warnf("  EXT_texture_env_combine");
    
    if (!gGLState.suppPackedPixels)
        Con::warnf("  EXT_packed_pixels");
    
    if (!gGLState.suppFogCoord)
        Con::warnf("  EXT_fog_coord");
    
    if (!gGLState.suppTextureCompression)
        Con::warnf("  ARB_texture_compression");
    
    if (!gGLState.suppS3TC)
        Con::warnf("  EXT_texture_compression_s3tc");
    
    if (!gGLState.suppFXT1)
        Con::warnf("  3DFX_texture_compression_FXT1");
    
    if (!gGLState.suppTexEnvAdd)
        Con::warnf("  (ARB|EXT)_texture_env_add");
    
    if (!gGLState.suppTexAnisotropic)
        Con::warnf("  EXT_texture_filter_anisotropic");
    
    if (!gGLState.suppSwapInterval)
        Con::warnf("  Vertical Sync");
    
    if (!gGLState.maxFSAASamples)
        Con::warnf("  ATI_FSAA");
    
    Con::printf("");

    // Set some console variables:
    Con::setBoolVariable("$FogCoordSupported", gGLState.suppFogCoord);
    Con::setBoolVariable("$TextureCompressionSupported", gGLState.suppTextureCompression);
    Con::setBoolVariable("$AnisotropySupported", gGLState.suppTexAnisotropic);
    Con::setBoolVariable("$PalettedTextureSupported", gGLState.suppPalettedTexture);
    Con::setBoolVariable("$SwapIntervalSupported", gGLState.suppSwapInterval);
    
    if (!gGLState.suppPalettedTexture && Con::getBoolVariable("$pref::OpenGL::forcePalettedTexture",false))
    {
        Con::setBoolVariable("$pref::OpenGL::forcePalettedTexture", false);
        Con::setBoolVariable("$pref::OpenGL::force16BitTexture", true);
    }
    
    // get fsaa samples. default to normal, no antialiasing
    // TODO: clamp this against ARB_multisample capabilities.
    gFSAASamples = Con::getIntVariable("$pref::OpenGL::numFSAASamples", 1);
    
    // done. silently restore the old cgl context.
    CGLSetCurrentContext(curr_ctx);
    CGLDestroyContext(temp_ctx);
}