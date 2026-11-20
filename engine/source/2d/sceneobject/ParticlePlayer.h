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

#ifndef _PARTICLE_PLAYER_H_
#define _PARTICLE_PLAYER_H_

#ifndef _PARTICLE_ASSET_H_
#include "2d/assets/ParticleAsset.h"
#endif

#ifndef _PARTICLE_SYSTEM_H_
#include "2d/core/ParticleSystem.h"
#endif

//-----------------------------------------------------------------------------

#define PARTICLE_PLAYER_EMISSION_RATE_SCALE     "$pref::T2D::ParticlePlayerEmissionRateScale"
#define PARTICLE_PLAYER_SIZE_SCALE              "$pref::T2D::ParticlePlayerSizeScale"
#define PARTICLE_PLAYER_FORCE_SCALE             "$pref::T2D::ParticlePlayerForceScale"
#define PARTICLE_PLAYER_TIME_SCALE              "$pref::T2D::ParticlePlayerTimeScale"

//-----------------------------------------------------------------------------

class ParticlePlayer : public SceneObject, protected AssetPtrCallback
{
private:
    typedef SceneObject Parent;

    /// Emitter node storing active particles in contiguous cache-friendly memory.
    class EmitterNode
    {
    private:
        ParticlePlayer*                         mOwner;
        ParticleAssetEmitter*                   mpAssetEmitter;
        Vector<ParticleSystem::ParticleNode>    mParticles;
        F32                                     mTimeSinceLastGeneration;
        bool                                    mPaused;
        bool                                    mVisible;

    public:
        EmitterNode( ParticlePlayer* pParticlePlayer, ParticleAssetEmitter* pParticleAssetEmitter )
        {
            // Sanity!
            AssertFatal( pParticlePlayer != NULL, "EmitterNode() - Cannot have a NULL owner." );
            AssertFatal( pParticleAssetEmitter != NULL, "EmitterNode() - Cannot have a NULL particle asset emitter." );

            // Set owner.
            mOwner = pParticlePlayer;

            // Set asset emitter.
            mpAssetEmitter = pParticleAssetEmitter;

            // Set emitter not paused.
            mPaused = false;

            // Set emitter visible.
            mVisible = true;

            // Reset time since last generation.
            mTimeSinceLastGeneration = 0.0f;

            VECTOR_SET_ASSOCIATION( mParticles );
        }

        ~EmitterNode()
        {
            freeAllParticles();
            if ( mParticles.capacity() > 0 && ParticleSystem::Instance != NULL )
                ParticleSystem::Instance->adjustAllocatedParticleCount( -(S32)mParticles.capacity() );
        }

        inline ParticlePlayer* getOwner( void ) const { return mOwner; }
        inline ParticleAssetEmitter* getAssetEmitter( void ) const { return mpAssetEmitter; }

        inline bool getActiveParticles( void ) const { return !mParticles.empty(); }
        inline U32 getParticleCount( void ) const { return (U32)mParticles.size(); }
        inline ParticleSystem::ParticleNode* getParticles( void ) { return mParticles.address(); }
        inline const ParticleSystem::ParticleNode* getParticles( void ) const { return mParticles.address(); }

        inline void setTimeSinceLastGeneration( const F32 timeSinceLastGeneration ) { mTimeSinceLastGeneration = timeSinceLastGeneration; }
        inline F32 getTimeSinceLastGeneration( void ) const { return mTimeSinceLastGeneration; }

        inline void setPaused( const bool paused ) { mPaused = paused; }
        inline bool getPaused( void ) const { return mPaused; }

        inline void setVisible( const bool visible ) { mVisible = visible; }
        inline bool getVisible( void ) const { return mVisible; }

        void reserveAdditional( const U32 count );
        ParticleSystem::ParticleNode* allocateParticleSlot( void );
        void compactToSize( const U32 newSize );
        void freeAllParticles( void );
    };

    /// Per-emitter simulation state hoisted once per tick instead of re-read per particle.
    struct EmitterTickContext
    {
        ParticleAssetEmitter*       pAssetEmitter;
        const ParticleAssetField*   sizeXLife;
        const ParticleAssetField*   sizeYLife;
        const ParticleAssetField*   speedLife;
        const ParticleAssetField*   fixedForceLife;
        const ParticleAssetField*   randomMotionLife;
        const ParticleAssetField*   spinLife;
        const ParticleAssetField*   redLife;
        const ParticleAssetField*   greenLife;
        const ParticleAssetField*   blueLife;
        const ParticleAssetField*   alphaLife;
        F32                         sizeXMin, sizeXMax;
        F32                         sizeYMin, sizeYMax;
        F32                         speedMin, speedMax;
        F32                         fixedForceMin, fixedForceMax;
        F32                         randomMotionMin, randomMotionMax;
        F32                         redMin, redMax;
        F32                         greenMin, greenMax;
        F32                         blueMin, blueMax;
        F32                         alphaMin, alphaMax;
        F32                         constRed, constGreen, constBlue, constAlpha;
        F32                         alphaScale;
        F32                         alignedAngleOffset;
        Vector2                     fixedForceDirScaled;
        F32                         animTotalTime;
        U32                         animFrameCount;
        bool                        animCycle;
        bool                        animRandomStart;
        bool                        fixedAspect;
        bool                        singleParticle;
        bool                        isStaticProvider;
        bool                        keepAligned;
        bool                        redConst;
        bool                        greenConst;
        bool                        blueConst;
        bool                        alphaConst;
    };

    typedef Vector<EmitterNode*> typeEmitterVector;

    AssetPtr<ParticleAsset>     mParticleAsset;
    typeEmitterVector           mEmitters;

    bool                        mCameraIdle;
    F32                         mCameraIdleDistance;

    bool						mParticleInterpolation;
    F32                         mRenderInterpDelta;

    bool                        mPlaying;
    bool                        mPaused;
    F32                         mAge;
    F32                         mEmissionRateScale;
    F32                         mSizeScale;
    F32                         mForceScale;
    F32                         mTimeScale;

    bool                        mWaitingForParticles;
    bool                        mWaitingForDelete;

public:
    ParticlePlayer();
    virtual ~ParticlePlayer();

    static void initPersistFields();
    virtual void copyTo(SimObject* object);
    virtual void safeDelete( void );

    virtual void preIntegrate( const F32 totalTime, const F32 elapsedTime, DebugStats* pDebugStats );
    void integrateObject( const F32 totalTime, const F32 elapsedTime, DebugStats* pDebugStats );
    void interpolateObject( const F32 timeDelta );

    virtual bool validRender( void ) const { return mParticleAsset.notNull() && mParticleAsset->isAssetValid(); }
    virtual bool shouldRender( void ) const { return true; }
    virtual bool usesObjectBlendState( void ) const { return false; }
    virtual void sceneRender( const SceneRenderState* pSceneRenderState, const SceneRenderRequest* pSceneRenderRequest, BatchRender* pBatchRenderer );
    virtual void sceneRenderOverlay( const SceneRenderState* sceneRenderState );

    void setParticle( const char* pAssetId );
    const char* getParticle( void ) const { return mParticleAsset->getAssetId(); }

    inline void setCameraIdleDistance( const F32 idleDistance ) { mCameraIdleDistance = idleDistance; mCameraIdle = false; }
    inline F32 getCameraIdleDistance( void ) const { return mCameraIdleDistance; }

    inline void setParticleInterpolation( const bool interpolation ) { mParticleInterpolation = interpolation; }
    inline bool getParticleInterpolation( void ) const { return mParticleInterpolation; }

    inline void setEmissionRateScale( const F32 scale ) { mEmissionRateScale = scale; }
    inline F32 getEmissionRateScale( void  ) const { return mEmissionRateScale; }

    inline void setSizeScale( const F32 scale ) { mSizeScale = scale; }
    inline F32 getSizeScale( void  ) const { return mSizeScale; }

    inline void setForceScale( const F32 scale ) { mForceScale = scale; }
    inline F32 getForceScale( void  ) const { return mForceScale; }

    inline void setTimeScale( const F32 scale ) { mTimeScale = scale; }
    inline F32 getTimeScale( void  ) const { return mTimeScale; }

    inline const U32 getEmitterCount( void ) const { return (U32)mEmitters.size(); }

    void setEmitterPaused( const bool paused, const U32 emitterIndex );
    bool getEmitterPaused( const U32 emitterIndex );
    void setEmitterVisible( const bool visible, const U32 emitterIndex );
    bool getEmitterVisible( const U32 emitterIndex );

    bool play( const bool resetParticles );
    void stop( const bool waitForParticles, const bool killEffect );
    inline bool getIsPlaying( void ) const { return mPlaying; };
    inline void setPaused( const bool paused ) { mPaused = paused; }
    inline bool getPaused( void ) const { return mPaused; }

    /// Combine an emitter's authored emission angle with the player's own rotation.
    /// The authored angle is in DEGREES (that is how the field is edited, and how the
    /// rest of the emission math carries it), while a SceneObject's angle is in
    /// RADIANS, so the player angle has to be converted before the two are added.
    static inline F32 calculateEmissionAngle( const F32 emissionAngleDegrees, const F32 playerAngleRadians, const bool linkEmissionRotation )
    {
        return linkEmissionRotation ? emissionAngleDegrees + mRadToDeg( playerAngleRadians ) : emissionAngleDegrees;
    }

    /// Transform an attached particle quad's emitter-local OOBB into world space using
    /// precomputed rotation cosine/sine, matching dglTranslatef + dglRotatef(+Z).
    static inline void transformAttachedOOBBPrecomputed(
        const Vector2 srcOOBB[4],
        Vector2 dstOOBB[4],
        const Vector2& playerPosition,
        const F32 cosAngle,
        const F32 sinAngle,
        const bool attachRotation )
    {
        if ( attachRotation )
        {
            for ( U32 i = 0; i < 4; ++i )
            {
                const F32 x = srcOOBB[i].x;
                const F32 y = srcOOBB[i].y;
                dstOOBB[i].x = (cosAngle * x - sinAngle * y) + playerPosition.x;
                dstOOBB[i].y = (sinAngle * x + cosAngle * y) + playerPosition.y;
            }
        }
        else
        {
            for ( U32 i = 0; i < 4; ++i )
            {
                dstOOBB[i].x = srcOOBB[i].x + playerPosition.x;
                dstOOBB[i].y = srcOOBB[i].y + playerPosition.y;
            }
        }
    }

    /// Transform an attached particle quad's emitter-local OOBB into world space.
    static inline void transformAttachedOOBB(
        const Vector2 srcOOBB[4],
        Vector2 dstOOBB[4],
        const Vector2& playerPosition,
        const F32 playerAngleRadians,
        const bool attachRotation )
    {
        const F32 cosAngle = attachRotation ? mCos( playerAngleRadians ) : 1.0f;
        const F32 sinAngle = attachRotation ? mSin( playerAngleRadians ) : 0.0f;
        transformAttachedOOBBPrecomputed( srcOOBB, dstOOBB, playerPosition, cosAngle, sinAngle, attachRotation );
    }

    /// Return whether a 4-corner quad overlaps the camera world AABB.
    static inline bool isQuadVisibleInAABB( const Vector2 quad[4], const b2AABB& cameraAABB )
    {
        F32 minX = quad[0].x;
        F32 maxX = minX;
        F32 minY = quad[0].y;
        F32 maxY = minY;
        for ( U32 i = 1; i < 4; ++i )
        {
            const F32 x = quad[i].x;
            const F32 y = quad[i].y;
            if ( x < minX ) minX = x;
            else if ( x > maxX ) maxX = x;
            if ( y < minY ) minY = y;
            else if ( y > maxY ) maxY = y;
        }
        return !(maxX < cameraAABB.lowerBound.x ||
                 minX > cameraAABB.upperBound.x ||
                 maxY < cameraAABB.lowerBound.y ||
                 minY > cameraAABB.upperBound.y);
    }

    /// Return whether zero-alpha particles produce zero visual contribution under this blend setup.
    static inline bool canCullZeroAlphaForBlend(
        const bool intenseParticles,
        const bool blendMode,
        const GLenum srcBlendFactor,
        const GLenum dstBlendFactor )
    {
        if ( intenseParticles )
            return true;

        return blendMode &&
               srcBlendFactor == GL_SRC_ALPHA &&
               (dstBlendFactor == GL_ONE_MINUS_SRC_ALPHA || dstBlendFactor == GL_ONE);
    }

    /// Advance a lightweight per-particle animation clock and return the validated frame index.
    static inline S32 stepParticleAnimation(
        F32& animationTime,
        bool& animationFinished,
        const F32 elapsedTime,
        const F32 totalTime,
        const U32 validatedFrameCount,
        const bool animationCycle )
    {
        if ( validatedFrameCount == 0 )
        {
            animationFinished = true;
            return -1;
        }
        if ( totalTime <= 0.0f )
            return 0;

        const F32 frameTime = totalTime / (F32)validatedFrameCount;
        if ( !animationFinished )
        {
            animationTime += elapsedTime;
            if ( !animationCycle && mGreaterThanOrEqual( animationTime, totalTime ) )
            {
                animationFinished = true;
                animationTime = totalTime - (frameTime * 0.5f);
                return (S32)(validatedFrameCount - 1);
            }
            if ( animationTime >= totalTime || animationTime < 0.0f )
            {
                animationTime = mFmod( animationTime, totalTime );
                if ( animationTime < 0.0f )
                    animationTime += totalTime;
            }
        }
        else if ( !animationCycle )
        {
            return (S32)(validatedFrameCount - 1);
        }

        S32 frameIndex = (S32)(animationTime / frameTime);
        if ( frameIndex < 0 )
            frameIndex = 0;
        else if ( frameIndex >= (S32)validatedFrameCount )
            frameIndex = (S32)(validatedFrameCount - 1);
        return frameIndex;
    }

    /// Compute the 4 world-space quad vertices for a particle at render time, caching
    /// the most recent particle angle's cos/sin across particles that share an angle.
    static inline void computeParticleQuad(
        const Vector2& pos,
        const Vector2& renderSize,
        const F32 orientationAngleDeg,
        const F32 localMinX,
        const F32 localMaxX,
        const F32 localMinY,
        const F32 localMaxY,
        const bool attachPosition,
        const bool attachRotation,
        const Vector2& playerPosition,
        const F32 playerCos,
        const F32 playerSin,
        F32& cachedAngleDeg,
        F32& cachedCos,
        F32& cachedSin,
        Vector2 outQuad[4] )
    {
        const F32 lx0 = localMinX * renderSize.x;
        const F32 lx1 = localMaxX * renderSize.x;
        const F32 ly0 = localMinY * renderSize.y;
        const F32 ly1 = localMaxY * renderSize.y;

        F32 wx = pos.x;
        F32 wy = pos.y;
        if ( attachPosition )
        {
            if ( attachRotation )
            {
                wx = (playerCos * pos.x - playerSin * pos.y) + playerPosition.x;
                wy = (playerSin * pos.x + playerCos * pos.y) + playerPosition.y;
            }
            else
            {
                wx = pos.x + playerPosition.x;
                wy = pos.y + playerPosition.y;
            }
        }

        if ( !attachRotation && orientationAngleDeg == 0.0f )
        {
            outQuad[0].Set( wx + lx0, wy + ly0 );
            outQuad[1].Set( wx + lx1, wy + ly0 );
            outQuad[2].Set( wx + lx1, wy + ly1 );
            outQuad[3].Set( wx + lx0, wy + ly1 );
            return;
        }

        F32 c = 1.0f;
        F32 s = 0.0f;
        if ( orientationAngleDeg == 0.0f )
        {
            c = playerCos;
            s = playerSin;
        }
        else
        {
            if ( orientationAngleDeg != cachedAngleDeg )
            {
                cachedAngleDeg = orientationAngleDeg;
                const F32 rad = mDegToRad( orientationAngleDeg );
                cachedCos = mCos( rad );
                cachedSin = mSin( rad );
            }

            if ( attachRotation )
            {
                c = playerCos * cachedCos - playerSin * cachedSin;
                s = playerSin * cachedCos + playerCos * cachedSin;
            }
            else
            {
                c = cachedCos;
                s = cachedSin;
            }
        }

        const F32 cx0 = c * lx0;
        const F32 cx1 = c * lx1;
        const F32 sx0 = s * lx0;
        const F32 sx1 = s * lx1;
        const F32 sy0 = s * ly0;
        const F32 sy1 = s * ly1;
        const F32 cy0 = c * ly0;
        const F32 cy1 = c * ly1;

        outQuad[0].Set( wx + cx0 - sy0, wy + sx0 + cy0 );
        outQuad[1].Set( wx + cx1 - sy0, wy + sx1 + cy0 );
        outQuad[2].Set( wx + cx1 - sy1, wy + sx1 + cy1 );
        outQuad[3].Set( wx + cx0 - sy1, wy + sx0 + cy1 );
    }

    static inline void computeParticleQuad(
        const Vector2& pos,
        const Vector2& renderSize,
        const F32 orientationAngleDeg,
        const F32 localMinX,
        const F32 localMaxX,
        const F32 localMinY,
        const F32 localMaxY,
        const bool attachPosition,
        const bool attachRotation,
        const Vector2& playerPosition,
        const F32 playerCos,
        const F32 playerSin,
        Vector2 outQuad[4] )
    {
        F32 cachedAngleDeg = 0.0f;
        F32 cachedCos = 1.0f;
        F32 cachedSin = 0.0f;
        computeParticleQuad(
            pos, renderSize, orientationAngleDeg,
            localMinX, localMaxX, localMinY, localMaxY,
            attachPosition, attachRotation, playerPosition,
            playerCos, playerSin,
            cachedAngleDeg, cachedCos, cachedSin,
            outQuad );
    }

    /// Declare Console Object.
    DECLARE_CONOBJECT(ParticlePlayer);

protected:
    virtual void OnRegisterScene( Scene* pScene );
    virtual void OnUnregisterScene( Scene* pScene );

    virtual void onAssetRefreshed( AssetPtrBase* pAssetPtrBase );

    /// Particle Creation/Integration.
    void buildEmitterTickContext( ParticleAsset* pParticleAsset, ParticleAssetEmitter* pParticleAssetEmitter, const F32 scaledTime, const F32 alphaScale, EmitterTickContext& outCtx ) const;
    void configureParticle( EmitterNode* pEmitterNode, ParticleSystem::ParticleNode* pParticleNode, const EmitterTickContext& ctx );
    void integrateParticle( const EmitterTickContext& ctx, ParticleSystem::ParticleNode* pParticleNode, const F32 particleAge, const F32 elapsedTime );

    /// Persistence.
    virtual void onTamlAddParent( SimObject* pParentObject );

    static bool     setParticle(void* obj, const char* data)                                { static_cast<ParticlePlayer*>( obj )->setParticle(data); return false; };
    static bool     writeCameraIdleDistance( void* obj, StringTableEntry pFieldName )       { return static_cast<ParticlePlayer*>( obj )->getCameraIdleDistance() > 0.0f; }
    static bool     writeParticleInterpolation( void* obj, StringTableEntry pFieldName )    { return static_cast<ParticlePlayer*>( obj )->getParticleInterpolation(); }
    static bool     writeEmissionRateScale( void* obj, StringTableEntry pFieldName )        { return !mIsOne( static_cast<ParticlePlayer*>( obj )->getEmissionRateScale() ); }
    static bool     writeSizeScale( void* obj, StringTableEntry pFieldName )                { return !mIsOne( static_cast<ParticlePlayer*>( obj )->getSizeScale() ); }
    static bool     writeForceScale( void* obj, StringTableEntry pFieldName )               { return !mIsOne( static_cast<ParticlePlayer*>( obj )->getForceScale() ); }
    static bool     writeTimeScale( void* obj, StringTableEntry pFieldName )                { return !mIsOne( static_cast<ParticlePlayer*>( obj )->getTimeScale() ); }

private:
    void initializeParticleAsset( void );
    void destroyParticleAsset( void );
};

#endif // _PARTICLE_PLAYER_H_
