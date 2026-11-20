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

#include "2d/sceneobject/ParticlePlayer.h"
#include "graphics/dgl.h"

// Script bindings.
#include "2d/sceneobject/ParticlePlayer_ScriptBinding.h"


//------------------------------------------------------------------------------

void ParticlePlayer::EmitterNode::reserveAdditional( const U32 count )
{
    if ( count == 0 )
        return;

    const U32 oldCap = mParticles.capacity();
    mParticles.reserve( (U32)mParticles.size() + count );
    const U32 newCap = mParticles.capacity();
    if ( newCap != oldCap && ParticleSystem::Instance != NULL )
        ParticleSystem::Instance->adjustAllocatedParticleCount( (S32)(newCap - oldCap) );
}

//------------------------------------------------------------------------------

ParticleSystem::ParticleNode* ParticlePlayer::EmitterNode::allocateParticleSlot( void )
{
    const U32 oldCap = mParticles.capacity();
    mParticles.increment();
    const U32 newCap = mParticles.capacity();

    if ( ParticleSystem::Instance != NULL )
    {
        if ( newCap != oldCap )
            ParticleSystem::Instance->adjustAllocatedParticleCount( (S32)(newCap - oldCap) );
        ParticleSystem::Instance->adjustActiveParticleCount( 1 );
    }

    return &mParticles.last();
}

//------------------------------------------------------------------------------

void ParticlePlayer::EmitterNode::compactToSize( const U32 newSize )
{
    const U32 oldSize = (U32)mParticles.size();
    if ( newSize >= oldSize )
        return;

    mParticles.setSize( newSize );
    if ( ParticleSystem::Instance != NULL )
        ParticleSystem::Instance->adjustActiveParticleCount( -(S32)(oldSize - newSize) );
}

//------------------------------------------------------------------------------

void ParticlePlayer::EmitterNode::freeAllParticles( void )
{
    const U32 count = (U32)mParticles.size();
    if ( count == 0 )
        return;

    if ( ParticleSystem::Instance != NULL )
        ParticleSystem::Instance->adjustActiveParticleCount( -(S32)count );

    mParticles.clear();
}

//------------------------------------------------------------------------------

IMPLEMENT_CONOBJECT(ParticlePlayer);

//------------------------------------------------------------------------------

ParticlePlayer::ParticlePlayer() :
                    mPlaying( false ),
                    mPaused( false ),
                    mAge( 0.0f ),
                    mParticleInterpolation( false ),
                    mRenderInterpDelta( 0.0f ),
                    mCameraIdleDistance( 0.0f ),
                    mCameraIdle( false ),
                    mWaitingForParticles( false ),
                    mWaitingForDelete( false )
{
    // Fetch the particle player scales.
    mEmissionRateScale = Con::getFloatVariable( PARTICLE_PLAYER_EMISSION_RATE_SCALE, 1.0f );
    mSizeScale         = Con::getFloatVariable( PARTICLE_PLAYER_SIZE_SCALE, 1.0f );
    mForceScale        = Con::getFloatVariable( PARTICLE_PLAYER_FORCE_SCALE, 1.0f );
    mTimeScale         = Con::getFloatVariable( PARTICLE_PLAYER_TIME_SCALE, 1.0f );
     
    // Register for refresh notifications.
    mParticleAsset.registerRefreshNotify( this );
}

//------------------------------------------------------------------------------

ParticlePlayer::~ParticlePlayer()
{
    // Destroy the particle asset.
    destroyParticleAsset();
}

//------------------------------------------------------------------------------

void ParticlePlayer::initPersistFields()
{
    // Call parent.
    Parent::initPersistFields();

    addProtectedField( "Particle", TypeParticleAssetPtr, Offset(mParticleAsset, ParticlePlayer), &setParticle, &defaultProtectedGetFn, defaultProtectedWriteFn, "" );
    addProtectedField( "CameraIdleDistance", TypeF32, Offset(mCameraIdleDistance, ParticlePlayer),&defaultProtectedSetFn, &defaultProtectedGetFn, &writeCameraIdleDistance,"" );
    addProtectedField( "ParticleInterpolation", TypeBool, Offset(mParticleInterpolation, ParticlePlayer), &defaultProtectedSetFn, &defaultProtectedGetFn, &writeParticleInterpolation,"" );
    addProtectedField( "EmissionRateScale", TypeF32, Offset(mEmissionRateScale, ParticlePlayer), &defaultProtectedSetFn, &defaultProtectedGetFn, &writeEmissionRateScale, "" );
    addProtectedField( "SizeScale", TypeF32, Offset(mSizeScale, ParticlePlayer), &defaultProtectedSetFn, &defaultProtectedGetFn, &writeSizeScale, "" );
    addProtectedField( "ForceScale", TypeF32, Offset(mForceScale, ParticlePlayer), &defaultProtectedSetFn, &defaultProtectedGetFn, &writeForceScale, "" );
    addProtectedField( "TimeScale", TypeF32, Offset(mTimeScale, ParticlePlayer), &defaultProtectedSetFn, &defaultProtectedGetFn, &writeTimeScale, "" );
}

//------------------------------------------------------------------------------

void ParticlePlayer::copyTo(SimObject* object)
{
    // Fetch particle asset object.
   ParticlePlayer* pParticlePlayer = static_cast<ParticlePlayer*>( object );

   // Sanity!
   AssertFatal( pParticlePlayer != NULL, "ParticlePlayer::copyTo() - Object is not the correct type.");

   // Copy parent.
   Parent::copyTo( object );

   // Copy the fields.
   pParticlePlayer->setParticle( getParticle() );
   pParticlePlayer->setCameraIdleDistance( getCameraIdleDistance() );
   pParticlePlayer->setParticleInterpolation( getParticleInterpolation() );
   pParticlePlayer->setEmissionRateScale( getEmissionRateScale() );
   pParticlePlayer->setSizeScale( getSizeScale() );
   pParticlePlayer->setForceScale( getForceScale() );
   pParticlePlayer->setTimeScale( getTimeScale() );
}

//------------------------------------------------------------------------------

void ParticlePlayer::onAssetRefreshed( AssetPtrBase* pAssetPtrBase )
{
    // Initialize the particle asset.
    initializeParticleAsset();
}

//-----------------------------------------------------------------------------

void ParticlePlayer::safeDelete( void )
{
    // Finish if we already waiting for delete.
    if ( mWaitingForDelete )
        return;

    // Is the player plating?
    if ( mPlaying )
    {
        // Yes, so stop playing and allow it to kill itself.
        stop(true, true);
        return;
    }

    // Call parent which will deal with the deletion.
    Parent::safeDelete();
}

//-----------------------------------------------------------------------------

void ParticlePlayer::OnRegisterScene( Scene* pScene )
{
    // Call parent.
    Parent::OnRegisterScene( pScene );

    // Add always in scope.
    pScene->getWorldQuery()->addAlwaysInScope( this );

    // Play the the particles if appropriate.
    if ( mParticleAsset.notNull() && mParticleAsset->getEmitterCount() > 0 )
        play( true );
}

//-----------------------------------------------------------------------------

void ParticlePlayer::OnUnregisterScene( Scene* pScene )
{
    // Stop the player.
    stop( false, false );

    // Remove always in scope.
    pScene->getWorldQuery()->removeAlwaysInScope( this );

    // Call parent.
    Parent::OnUnregisterScene( pScene );
}

//-----------------------------------------------------------------------------

void ParticlePlayer::preIntegrate( const F32 totalTime, const F32 elapsedTime, DebugStats* pDebugStats )
{
    // Call Parent.
    Parent::preIntegrate( totalTime, elapsedTime, pDebugStats );

    // Finish if the camera idle distance is zero.
    if ( mIsZero(mCameraIdleDistance) || !validRender() )
        return;

    // Fetch current position.
    const Vector2 position = getPosition();

    // Calculate camera idle distance squared.
    const F32 cameraIdleDistanceSqr = mCameraIdleDistance * mCameraIdleDistance;

    // Fetch scene windows.
    SimSet& sceneWindows = getScene()->getAttachedSceneWindows();

    // Find a scene window that stops the pause.
    for( SimSet::iterator itr = sceneWindows.begin(); itr != sceneWindows.end(); itr++ )
    {
        // Fetch the scene window.
        SceneWindow* pSceneWindow = static_cast<SceneWindow*>(*itr);

        // Are we within the camera distance?
        if ( (pSceneWindow->getCameraPosition() - position).LengthSquared() < cameraIdleDistanceSqr )
        {
            // Yes, so play.
            if ( !getIsPlaying() )
                play( true );

            // Flag as not camera idle.
            mCameraIdle = false;

            return;
        }
    }

    // If playing then stop.
    if ( getIsPlaying() )
        stop( false, false );

    // Flag as camera idle.
    mCameraIdle = true;
}

//------------------------------------------------------------------------------

void ParticlePlayer::integrateObject( const F32 totalTime, const F32 elapsedTime, DebugStats* pDebugStats )
{
    // Call parent.
    Parent::integrateObject( totalTime, elapsedTime, pDebugStats );

    // Finish if no need to integrate.
    if (    !mPlaying ||
            mPaused ||
            mEmitters.size() == 0 )
        return;

    // Calculate scaled time.
    const F32 scaledTime = elapsedTime * mTimeScale;

    // Fetch particle asset.
    ParticleAsset* pParticleAsset = mParticleAsset;

    // Finish if no particle asset assigned.
    if ( pParticleAsset == NULL )
        return;

    // Reset active particle count.
    U32 activeParticleCount = 0;

    // Is the camera idle?
    if ( !mCameraIdle )
    {
        // A new physics tick has stepped positions to the tick boundary.
        mRenderInterpDelta = 0.0f;

        // No, so update the particle player age.
        mAge += scaledTime;

        // Evaluate effect-age alpha scale once for all emitters and particles in this tick.
        const F32 alphaScale = pParticleAsset->getAlphaChannelScaleField().getFieldValue( mAge );

        // Iterate the emitters.
        for( typeEmitterVector::iterator emitterItr = mEmitters.begin(); emitterItr != mEmitters.end(); ++emitterItr )
        {
            // Fetch the emitter node.
            EmitterNode* pEmitterNode = *emitterItr;

            // Fetch the asset emitter.
            ParticleAssetEmitter* pParticleAssetEmitter = pEmitterNode->getAssetEmitter();

            // Hoist invariant emitter curve pointers and bounds once for the entire emitter.
            EmitterTickContext ctx;
            buildEmitterTickContext( pParticleAsset, pParticleAssetEmitter, scaledTime, alphaScale, ctx );

            // Linearly compact expired particles while integrating survivors in cache order.
            ParticleSystem::ParticleNode* particles = pEmitterNode->getParticles();
            const U32 particleCount = pEmitterNode->getParticleCount();
            U32 writeIndex = 0;
            for ( U32 readIndex = 0; readIndex < particleCount; ++readIndex )
            {
                ParticleSystem::ParticleNode& src = particles[readIndex];
                src.mParticleAge += scaledTime;

                if ( (!ctx.singleParticle && src.mParticleAge > src.mParticleLifetime) ||
                     mIsZero( src.mParticleLifetime ) )
                {
                    continue;
                }

                if ( writeIndex != readIndex )
                    particles[writeIndex] = src;

                ParticleSystem::ParticleNode* pDst = &particles[writeIndex];
                integrateParticle( ctx, pDst, pDst->mParticleAge / pDst->mParticleLifetime, scaledTime );
                writeIndex++;
            }

            if ( writeIndex != particleCount )
                pEmitterNode->compactToSize( writeIndex );

            activeParticleCount += writeIndex;

            // Skip generating new particles if the emitter is paused.
            if ( pEmitterNode->getPaused() )
                continue;

            // Are we in single-particle mode?
            if ( ctx.singleParticle )
            {
                // Yes, so do we have a single particle yet?
                if ( !pEmitterNode->getActiveParticles() )
                {
                    ParticleSystem::ParticleNode* pNode = pEmitterNode->allocateParticleSlot();
                    configureParticle( pEmitterNode, pNode, ctx );
                }
            }
            else
            {
                // Accumulate the last generation time as we need to handle very small time-integrations correctly.
                //
                // NOTE:    We need to do this if there's an emission target but the time-integration is so small
                //          that rounding results in no emission.  Downside to good FPS!
                pEmitterNode->setTimeSinceLastGeneration( pEmitterNode->getTimeSinceLastGeneration() + scaledTime );

                // Fetch the particle player age.
                const F32 particlePlayerAge = mAge;

                // Fetch the quantity base and variation fields.
                const ParticleAssetField& quantityBaseField = pParticleAssetEmitter->getQuantityBaseField();
                const ParticleAssetField& quantityVaritationField = pParticleAssetEmitter->getQuantityVariationField();

                // Fetch the emissions.
                const F32 baseEmission = quantityBaseField.getFieldValue( particlePlayerAge );
                const F32 varEmission = quantityVaritationField.getFieldValue( particlePlayerAge ) * 0.5f;
                const F32 randomizedEmission = (varEmission != 0.0f) ? (baseEmission + CoreMath::mGetRandomF(-varEmission, varEmission)) : baseEmission;

                // Fetch the emission scale.
                const F32 effectEmission = pParticleAsset->getQuantityScaleField().getFieldValue( particlePlayerAge ) * getEmissionRateScale();

                // Calculate the local emission.
                const F32 localEmission = mClampF(  randomizedEmission * effectEmission,
                                                    quantityBaseField.getMinValue(),
                                                    quantityBaseField.getMaxValue() );

                // Calculate the final time-independent emission count.
                const U32 emissionCount = U32(mFloor( localEmission * pEmitterNode->getTimeSinceLastGeneration() ));

                // Do we have an emission?
                if ( emissionCount > 0 )
                {
                    // Yes, so remove this emission from accumulated time.
                    pEmitterNode->setTimeSinceLastGeneration( getMax(0.0f, pEmitterNode->getTimeSinceLastGeneration() - (emissionCount / localEmission) ) );

                    // Suppress Precision Errors.
                    if ( mIsZero( pEmitterNode->getTimeSinceLastGeneration() ) )
                        pEmitterNode->setTimeSinceLastGeneration( 0.0f );

                    // Pre-reserve all new slots in one allocation before configuring particles.
                    pEmitterNode->reserveAdditional( emissionCount );
                    for ( U32 n = 0; n < emissionCount; n++ )
                    {
                        ParticleSystem::ParticleNode* pNode = pEmitterNode->allocateParticleSlot();
                        configureParticle( pEmitterNode, pNode, ctx );
                    }
                }
            }
        }
    }

    // Fetch the particle life-mode.
    const ParticleAsset::LifeMode lifeMode = pParticleAsset->getLifeMode();

    // Are we waiting for particles and there are non left?
    if ( mWaitingForParticles )
    {
        // Yes, so are there any particles left?
        if ( activeParticleCount == 0 )
        {
            // No, so stop the player immediately.
            stop( false, mWaitingForDelete );
        }

        return;
    }

    // Finish if the particle player is in "infinite" mode.
    if ( lifeMode == ParticleAsset::INFINITE )
        return;

    // Fetch the particle lifetime.
    const F32 lifetime = pParticleAsset->getLifetime();

    // Cycle life-mode?
    if ( lifeMode == ParticleAsset::CYCLE )
    {
        // Has the age expired?
        if ( mAge >= lifetime )
        {
            // Yes, so restart the particle.
            play( false );
        }
        return;
    }

    // Stop life-mode?
    if ( lifeMode == ParticleAsset::STOP )
    {
        // Has the age expired?
        if ( mAge >= lifetime )
        {
            // Yes, so stop the particle.
            stop( true, false );
        }
        return;
    }

    // kill life-mode?
    if ( lifeMode == ParticleAsset::KILL )
    {
        // Has the age expired?
        if ( mAge >= lifetime )
        {
            // Yes, so kill the particle.
            stop( true, true );
        }
        return;
    }
}

//------------------------------------------------------------------------------

void ParticlePlayer::interpolateObject( const F32 timeDelta )
{    
    // Call parent.
    Parent::interpolateObject( timeDelta );

    // Finish if no need to interpolate.
    if ( !mParticleInterpolation || !mPlaying || mCameraIdle || mPaused )
        return;

    mRenderInterpDelta = timeDelta;
}

//-----------------------------------------------------------------------------

void ParticlePlayer::sceneRender( const SceneRenderState* pSceneRenderState, const SceneRenderRequest* pSceneRenderRequest, BatchRender* pBatchRenderer )
{
    // Finish if we can't render.
    if ( !mPlaying || mCameraIdle )
        return;

    // Fetch emitter count and render-time interpolation parameters.
    const U32 emitterCount = mEmitters.size();
    const bool cullParticles = ( pSceneRenderState != NULL && pSceneRenderState->mCullParticles );
    const bool useInterp = ( mParticleInterpolation && mRenderInterpDelta != 0.0f );
    const F32 interpDelta = mRenderInterpDelta;
    const F32 oneMinusInterp = 1.0f - interpDelta;

    const F32 camMinX = cullParticles ? pSceneRenderState->mCameraAABB.lowerBound.x : 0.0f;
    const F32 camMaxX = cullParticles ? pSceneRenderState->mCameraAABB.upperBound.x : 0.0f;
    const F32 camMinY = cullParticles ? pSceneRenderState->mCameraAABB.lowerBound.y : 0.0f;
    const F32 camMaxY = cullParticles ? pSceneRenderState->mCameraAABB.upperBound.y : 0.0f;

    // Render all the emitters.
    for ( U32 emitterIndex = 0; emitterIndex < emitterCount; ++emitterIndex )
    {
        // Fetch the emitter node.
        EmitterNode* pEmitterNode = mEmitters[emitterIndex];

        // Fetch the particle emitter.
        ParticleAssetEmitter* pParticleAssetEmitter = pEmitterNode->getAssetEmitter();

        // Skip if the emitter is not visible.
        if ( !pEmitterNode->getVisible() )
            continue;

        // Skip if there are no active particles.
        const S32 particleCount = (S32)pEmitterNode->getParticleCount();
        if ( particleCount == 0 )
            continue;

        // Fetch both image and animation assets.
        const AssetPtr<ImageAsset>& imageAsset = pParticleAssetEmitter->getImageAsset();
        const AssetPtr<AnimationAsset>& animationAsset = pParticleAssetEmitter->getAnimationAsset();

        // Fetch static mode and hoist shared texture / static UVs outside the particle loop.
        const bool isStaticFrameProvider = pParticleAssetEmitter->isStaticFrameProvider();
        const bool isRandomFrame = isStaticFrameProvider && pParticleAssetEmitter->getRandomImageFrame();
        TextureHandle* pEmitterTexture = NULL;
        Vector2 staticUv0, staticUv1, staticUv2, staticUv3;
        U32 staticImageFrameCount = 0;

        bool animNamedCells = false;
        S32 animValidatedCount = 0;
        U32 animImageFrameCount = 0;
        const Vector<S32>* pAnimValidatedFrames = NULL;
        const Vector<StringTableEntry>* pAnimValidatedNamedFrames = NULL;
        ImageAsset* pAnimImageAsset = NULL;

        if ( isStaticFrameProvider )
        {
            // Skip if no image or frames available.
            if ( imageAsset.isNull() || imageAsset->getFrameCount() == 0 )
                continue;

            staticImageFrameCount = imageAsset->getFrameCount();
            pEmitterTexture = &(imageAsset->getImageTexture());
            if ( pEmitterTexture->IsNull() )
                continue;

            if ( !isRandomFrame )
            {
                const ImageAsset::FrameArea::TexelArea* pTexelArea = NULL;
                if ( pParticleAssetEmitter->isUsingNamedImageFrame() )
                {
                    const char* namedFrame = pParticleAssetEmitter->getNamedImageFrame();
                    if ( !namedFrame || namedFrame == StringTable->EmptyString || !imageAsset->containsFrame( namedFrame ) )
                        continue;
                    pTexelArea = &(imageAsset->getImageFrameArea( namedFrame ).mTexelArea);
                }
                else
                {
                    const U32 imageFrame = pParticleAssetEmitter->getImageFrame();
                    if ( imageFrame >= staticImageFrameCount )
                        continue;
                    pTexelArea = &(imageAsset->getImageFrameArea( imageFrame ).mTexelArea);
                }

                const Vector2& texLower = pTexelArea->mTexelLower;
                const Vector2& texUpper = pTexelArea->mTexelUpper;
                staticUv0 = Vector2( texLower.x, texUpper.y );
                staticUv1 = Vector2( texUpper.x, texUpper.y );
                staticUv2 = Vector2( texUpper.x, texLower.y );
                staticUv3 = Vector2( texLower.x, texLower.y );
            }
        }
        else
        {
            // Skip if no animation or backing image available.
            if ( animationAsset.isNull() )
                continue;

            const AssetPtr<ImageAsset>& animImage = animationAsset->getImage();
            if ( animImage.isNull() || animImage->getFrameCount() == 0 )
                continue;

            animNamedCells = animationAsset->getNamedCellsMode();
            pAnimValidatedFrames = &(animationAsset->getValidatedAnimationFrames());
            pAnimValidatedNamedFrames = &(animationAsset->getValidatedNamedAnimationFrames());
            animValidatedCount = animNamedCells ? pAnimValidatedNamedFrames->size() : pAnimValidatedFrames->size();
            if ( animValidatedCount == 0 )
                continue;

            pAnimImageAsset = animImage;
            animImageFrameCount = animImage->getFrameCount();
            pEmitterTexture = &(animImage->getImageTexture());
            if ( pEmitterTexture->IsNull() )
                continue;
        }

        // Precompute per-emitter blend, culling, pivot bounds, and attachment transforms once.
        const bool intenseParticles = pParticleAssetEmitter->getIntenseParticles();
        const bool emitterBlendMode = pParticleAssetEmitter->getBlendMode();
        const GLenum emitterSrcBlend = (GLenum)pParticleAssetEmitter->getSrcBlendFactor();
        const GLenum emitterDstBlend = (GLenum)pParticleAssetEmitter->getDstBlendFactor();
        const bool canCullZeroAlpha = canCullZeroAlphaForBlend( intenseParticles, emitterBlendMode, emitterSrcBlend, emitterDstBlend );

        const bool attachPosition = pParticleAssetEmitter->getAttachPositionToEmitter();
        const bool attachRotation = attachPosition && pParticleAssetEmitter->getAttachRotationToEmitter();
        const Vector2 renderPosition = attachPosition ? getRenderPosition() : Vector2::getZero();
        const F32 renderAngle = attachRotation ? getRenderAngle() : 0.0f;
        const F32 rotCos = attachRotation ? mCos( renderAngle ) : 1.0f;
        const F32 rotSin = attachRotation ? mSin( renderAngle ) : 0.0f;

        const F32 localMinX = pParticleAssetEmitter->getLocalPivotAABB0().x;
        const F32 localMaxX = pParticleAssetEmitter->getLocalPivotAABB1().x;
        const F32 localMinY = pParticleAssetEmitter->getLocalPivotAABB0().y;
        const F32 localMaxY = pParticleAssetEmitter->getLocalPivotAABB2().y;

        F32 cachedAngleDeg = 0.0f;
        F32 cachedCos = 1.0f;
        F32 cachedSin = 0.0f;
        Vector2 quadVerts[4];
        bool emitterStateApplied = false;

        // With newest particles appended at the tail, drawing oldestInFront walks backward
        // (newest drawn first, oldest last on top); otherwise forward (oldest drawn first).
        const bool oldestInFront = pParticleAssetEmitter->getOldestInFront();
        const S32 startIndex = oldestInFront ? (particleCount - 1) : 0;
        const S32 endIndex   = oldestInFront ? -1 : particleCount;
        const S32 stepIndex  = oldestInFront ? -1 : 1;
        const ParticleSystem::ParticleNode* pParticles = pEmitterNode->getParticles();

        for ( S32 i = startIndex; i != endIndex; i += stepIndex )
        {
            const ParticleSystem::ParticleNode& particle = pParticles[i];

            if ( particle.mRenderSize.x <= 0.0f || particle.mRenderSize.y <= 0.0f )
                continue;

            if ( canCullZeroAlpha && particle.mColor.alpha <= 0.001f )
                continue;

            const Vector2 renderPos = useInterp
                ? Vector2(
                    interpDelta * particle.mPreTickPosition.x + oneMinusInterp * particle.mPosition.x,
                    interpDelta * particle.mPreTickPosition.y + oneMinusInterp * particle.mPosition.y )
                : particle.mPosition;

            if ( !attachRotation && particle.mOrientationAngle == 0.0f )
            {
                const F32 wx = attachPosition ? (renderPos.x + renderPosition.x) : renderPos.x;
                const F32 wy = attachPosition ? (renderPos.y + renderPosition.y) : renderPos.y;
                const F32 x0 = wx + localMinX * particle.mRenderSize.x;
                const F32 x1 = wx + localMaxX * particle.mRenderSize.x;
                const F32 y0 = wy + localMinY * particle.mRenderSize.y;
                const F32 y1 = wy + localMaxY * particle.mRenderSize.y;

                if ( cullParticles && (x1 < camMinX || x0 > camMaxX || y1 < camMinY || y0 > camMaxY) )
                    continue;

                quadVerts[0].Set( x0, y0 );
                quadVerts[1].Set( x1, y0 );
                quadVerts[2].Set( x1, y1 );
                quadVerts[3].Set( x0, y1 );
            }
            else
            {
                computeParticleQuad(
                    renderPos, particle.mRenderSize, particle.mOrientationAngle,
                    localMinX, localMaxX, localMinY, localMaxY,
                    attachPosition, attachRotation, renderPosition,
                    rotCos, rotSin,
                    cachedAngleDeg, cachedCos, cachedSin,
                    quadVerts );

                if ( cullParticles && !isQuadVisibleInAABB( quadVerts, pSceneRenderState->mCameraAABB ) )
                    continue;
            }

            Vector2 uv0, uv1, uv2, uv3;
            const Vector2* pUv0 = &staticUv0;
            const Vector2* pUv1 = &staticUv1;
            const Vector2* pUv2 = &staticUv2;
            const Vector2* pUv3 = &staticUv3;

            if ( !isStaticFrameProvider )
            {
                if ( particle.mFrameIndex < 0 || particle.mFrameIndex >= animValidatedCount )
                    continue;

                const ImageAsset::FrameArea::TexelArea* pTexelArea = NULL;
                if ( animNamedCells )
                {
                    const StringTableEntry frameName = (*pAnimValidatedNamedFrames)[particle.mFrameIndex];
                    if ( !pAnimImageAsset->containsFrame( frameName ) )
                        continue;
                    pTexelArea = &(pAnimImageAsset->getImageFrameArea( frameName ).mTexelArea);
                }
                else
                {
                    const S32 resolvedFrame = (*pAnimValidatedFrames)[particle.mFrameIndex];
                    if ( resolvedFrame < 0 || (U32)resolvedFrame >= animImageFrameCount )
                        continue;
                    pTexelArea = &(pAnimImageAsset->getImageFrameArea( (U32)resolvedFrame ).mTexelArea);
                }

                const Vector2& texLower = pTexelArea->mTexelLower;
                const Vector2& texUpper = pTexelArea->mTexelUpper;
                uv0 = Vector2( texLower.x, texUpper.y );
                uv1 = Vector2( texUpper.x, texUpper.y );
                uv2 = Vector2( texUpper.x, texLower.y );
                uv3 = Vector2( texLower.x, texLower.y );
                pUv0 = &uv0;
                pUv1 = &uv1;
                pUv2 = &uv2;
                pUv3 = &uv3;
            }
            else if ( isRandomFrame )
            {
                if ( particle.mFrameIndex < 0 || (U32)particle.mFrameIndex >= staticImageFrameCount )
                    continue;

                const ImageAsset::FrameArea::TexelArea& texelFrameArea = imageAsset->getImageFrameArea( (U32)particle.mFrameIndex ).mTexelArea;
                const Vector2& texLower = texelFrameArea.mTexelLower;
                const Vector2& texUpper = texelFrameArea.mTexelUpper;
                uv0 = Vector2( texLower.x, texUpper.y );
                uv1 = Vector2( texUpper.x, texUpper.y );
                uv2 = Vector2( texUpper.x, texLower.y );
                uv3 = Vector2( texLower.x, texLower.y );
                pUv0 = &uv0;
                pUv1 = &uv1;
                pUv2 = &uv2;
                pUv3 = &uv3;
            }

            // Defer emitter blend/alpha state changes until at least one particle survives culling.
            if ( !emitterStateApplied )
            {
                if ( intenseParticles )
                {
                    // Additive blending for intense particles.
                    pBatchRenderer->setBlendMode( GL_SRC_ALPHA, GL_ONE );
                }
                else if ( emitterBlendMode )
                {
                    pBatchRenderer->setBlendMode( emitterSrcBlend, emitterDstBlend );
                }
                else
                {
                    pBatchRenderer->setBlendOff();
                }

                // Set alpha-testing.
                pBatchRenderer->setAlphaTestMode( pParticleAssetEmitter->getAlphaTest() );
                emitterStateApplied = true;
            }

            // Submit batched quad.
            pBatchRenderer->SubmitQuad(
                quadVerts[0],
                quadVerts[1],
                quadVerts[2],
                quadVerts[3],
                *pUv0,
                *pUv1,
                *pUv2,
                *pUv3,
                *pEmitterTexture,
                particle.mColor );
        }
    }
}

//-----------------------------------------------------------------------------

void ParticlePlayer::sceneRenderOverlay( const SceneRenderState* sceneRenderState )
{
    // Call parent.
    Parent::sceneRenderOverlay( sceneRenderState );

    // Get Scene.
    Scene* pScene = getScene();

    // Finish if no scene.
    if ( !pScene )
        return;

    // Finish if we shouldn't be drawing the debug overlay.
    if ( !pScene->getIsEditorScene() || mLessThanOrEqual( mCameraIdleDistance, 0.0f ) || !isEnabled() || !getVisible() )
        return;

    // Draw camera pause distance.
    pScene->mDebugDraw.DrawCircle( getRenderPosition(), mCameraIdleDistance, ColorF(1.0f, 1.0f, 0.0f ) );
}

//-----------------------------------------------------------------------------

void ParticlePlayer::setParticle( const char* pAssetId )
{
    // Sanity!
    AssertFatal( pAssetId != NULL, "ParticlePlayer::setParticle() - Cannot use a NULL asset Id." );

    // Set asset Id.
    mParticleAsset = pAssetId;

    // Initialize the particle.
    initializeParticleAsset();
}

//-----------------------------------------------------------------------------

void ParticlePlayer::setEmitterPaused( const bool paused, const U32 emitterIndex )
{
    // Is the emitter index valid?
    if ( emitterIndex >= getEmitterCount() )
    {
        // No, so warn.
        Con::warnf( "ParticlePlayer::setEmitterPaused() - Emitter index is out of bounds." );
        return;
    }

    mEmitters[emitterIndex]->setPaused( paused );
}

//-----------------------------------------------------------------------------

bool ParticlePlayer::getEmitterPaused( const U32 emitterIndex )
{
    // Is the emitter index valid?
    if ( emitterIndex >= getEmitterCount() )
    {
        // No, so warn.
        Con::warnf( "ParticlePlayer::getEmitterPaused() - Emitter index is out of bounds." );
        return false;
    }

    return mEmitters[emitterIndex]->getPaused();
}

//-----------------------------------------------------------------------------

void ParticlePlayer::setEmitterVisible( const bool visible, const U32 emitterIndex )
{
    // Is the emitter index valid?
    if ( emitterIndex >= getEmitterCount() )
    {
        // No, so warn.
        Con::warnf( "ParticlePlayer::setEmitterVisible() - Emitter index is out of bounds." );
        return;
    }

    mEmitters[emitterIndex]->setVisible( visible );
}

//-----------------------------------------------------------------------------

bool ParticlePlayer::getEmitterVisible( const U32 emitterIndex )
{
    // Is the emitter index valid?
    if ( emitterIndex >= getEmitterCount() )
    {
        // No, so warn.
        Con::warnf( "ParticlePlayer::getEmitterVisible() - Emitter index is out of bounds." );
        return false;
    }

    return mEmitters[emitterIndex]->getVisible();
}

//-----------------------------------------------------------------------------

bool ParticlePlayer::play( const bool resetParticles )
{
    // Cannot do anything if we've not got any emitters!
    if ( mParticleAsset.isNull() || mParticleAsset->getEmitterCount() == 0 )
    {
        // Warn.
        Con::warnf("ParticlePlayer::play() - Cannot play; no emitters!");
        return false;
    }

    // Are we in a scene?
    if ( getScene() == NULL )
    {
        // No, so warn.
        Con::warnf("ParticlePlayer::play() - Cannot play when not in a scene!");
        return false;
    }

    // Reset the age.
    mAge = 0.0f;
    mRenderInterpDelta = 0.0f;

    // Iterate the emitters.
    for( typeEmitterVector::iterator emitterItr = mEmitters.begin(); emitterItr != mEmitters.end(); ++emitterItr )
    {
        // Fetch the emitter node.
        EmitterNode* pEmitterNode = *emitterItr;
        pEmitterNode->setPaused(false);
        
        // Reset the time since last generation.
        pEmitterNode->setTimeSinceLastGeneration( 0.0f );
    }

    // Reset Waiting for Particles.
    mWaitingForParticles = false;

    // Reset Waiting for delete.
    mWaitingForDelete = false;

    // Flag as playing.
    mPlaying = true;

    // Turn-off paused.
    mPaused = false;

    // Set unsafe delete status.
    setSafeDelete(false);

    return true;
}

//-----------------------------------------------------------------------------

void ParticlePlayer::stop( const bool waitForParticles, const bool killEffect )
{
    // Finish if we're not playing and there's no kill command.
    if ( !mPlaying && !killEffect )
        return;

    // Fetch emitter count.
    const U32 emitterCount = mEmitters.size();

    // Are we waiting for particles to end?
    if ( waitForParticles )
    {
        // Yes, so pause all the emitters.
        for ( U32 emitterIndex = 0; emitterIndex < emitterCount; ++emitterIndex )
        {
            // Fetch the emitter.
            mEmitters[emitterIndex]->setPaused( true );
        }

        // Set waiting for particles.
        mWaitingForParticles = true;

        // Flag as waiting for deletion if killing effect.
        if ( killEffect )
            mWaitingForDelete = true;

        return;
    }

    // No, so free all particles.
    for ( U32 emitterIndex = 0; emitterIndex < emitterCount; ++emitterIndex )
    {
        mEmitters[emitterIndex]->freeAllParticles();
    }

    // Reset the age.
    mAge = 0.0f;
    mRenderInterpDelta = 0.0f;

    // Flag as stopped and not waiting.
    mPlaying = mWaitingForParticles = mWaitingForDelete = false;

    // Turn off paused.
    mPaused = false;

    // Set safe deletion.
    setSafeDelete(true);

    // Perform the callback.
    if( isMethod( "onStopParticlePlayer" ) )
        Con::executef( this, 1, "onStopParticlePlayer" );

    // Flag for immediate deletion if killing.
    if ( killEffect )
        safeDelete();
}

//------------------------------------------------------------------------------

void ParticlePlayer::buildEmitterTickContext(
    ParticleAsset* pParticleAsset,
    ParticleAssetEmitter* pParticleAssetEmitter,
    const F32 scaledTime,
    const F32 alphaScale,
    EmitterTickContext& outCtx ) const
{
    outCtx.pAssetEmitter = pParticleAssetEmitter;

    outCtx.sizeXLife = &pParticleAssetEmitter->getSizeXLifeField();
    outCtx.sizeYLife = &pParticleAssetEmitter->getSizeYLifeField();
    outCtx.speedLife = &pParticleAssetEmitter->getSpeedLifeField();
    outCtx.fixedForceLife = &pParticleAssetEmitter->getFixedForceLifeField();
    outCtx.randomMotionLife = &pParticleAssetEmitter->getRandomMotionLifeField();
    outCtx.spinLife = &pParticleAssetEmitter->getSpinLifeField();

    outCtx.sizeXMin = pParticleAssetEmitter->getSizeXBaseField().getMinValue();
    outCtx.sizeXMax = pParticleAssetEmitter->getSizeXBaseField().getMaxValue();
    outCtx.sizeYMin = pParticleAssetEmitter->getSizeYBaseField().getMinValue();
    outCtx.sizeYMax = pParticleAssetEmitter->getSizeYBaseField().getMaxValue();
    outCtx.speedMin = pParticleAssetEmitter->getSpeedBaseField().getMinValue();
    outCtx.speedMax = pParticleAssetEmitter->getSpeedBaseField().getMaxValue();
    outCtx.fixedForceMin = pParticleAssetEmitter->getFixedForceBaseField().getMinValue();
    outCtx.fixedForceMax = pParticleAssetEmitter->getFixedForceBaseField().getMaxValue();
    outCtx.randomMotionMin = pParticleAssetEmitter->getRandomMotionBaseField().getMinValue();
    outCtx.randomMotionMax = pParticleAssetEmitter->getRandomMotionBaseField().getMaxValue();

    outCtx.redLife = &pParticleAssetEmitter->getRedChannelLifeField();
    outCtx.greenLife = &pParticleAssetEmitter->getGreenChannelLifeField();
    outCtx.blueLife = &pParticleAssetEmitter->getBlueChannelLifeField();
    outCtx.alphaLife = &pParticleAssetEmitter->getAlphaChannelLifeField();

    outCtx.redMin = outCtx.redLife->getMinValue();
    outCtx.redMax = outCtx.redLife->getMaxValue();
    outCtx.greenMin = outCtx.greenLife->getMinValue();
    outCtx.greenMax = outCtx.greenLife->getMaxValue();
    outCtx.blueMin = outCtx.blueLife->getMinValue();
    outCtx.blueMax = outCtx.blueLife->getMaxValue();
    outCtx.alphaMin = outCtx.alphaLife->getMinValue();
    outCtx.alphaMax = outCtx.alphaLife->getMaxValue();

    outCtx.redConst = outCtx.redLife->isConstant();
    outCtx.greenConst = outCtx.greenLife->isConstant();
    outCtx.blueConst = outCtx.blueLife->isConstant();
    outCtx.alphaConst = outCtx.alphaLife->isConstant();

    outCtx.constRed = outCtx.redConst ? mClampF( outCtx.redLife->getConstantValue(), outCtx.redMin, outCtx.redMax ) : 0.0f;
    outCtx.constGreen = outCtx.greenConst ? mClampF( outCtx.greenLife->getConstantValue(), outCtx.greenMin, outCtx.greenMax ) : 0.0f;
    outCtx.constBlue = outCtx.blueConst ? mClampF( outCtx.blueLife->getConstantValue(), outCtx.blueMin, outCtx.blueMax ) : 0.0f;
    outCtx.constAlpha = outCtx.alphaConst ? mClampF( outCtx.alphaLife->getConstantValue() * alphaScale, outCtx.alphaMin, outCtx.alphaMax ) : 0.0f;

    outCtx.alphaScale = alphaScale;
    outCtx.alignedAngleOffset = pParticleAssetEmitter->getAlignedAngleOffset();
    outCtx.fixedForceDirScaled = pParticleAssetEmitter->getFixedForceDirection() * (getForceScale() * scaledTime);

    outCtx.fixedAspect = pParticleAssetEmitter->getFixedAspect();
    outCtx.singleParticle = pParticleAssetEmitter->getSingleParticle();
    outCtx.isStaticProvider = pParticleAssetEmitter->isStaticFrameProvider();
    outCtx.keepAligned = pParticleAssetEmitter->getKeepAligned() &&
                         (pParticleAssetEmitter->getOrientationType() == ParticleAssetEmitter::ALIGNED_ORIENTATION);

    outCtx.animTotalTime = 0.0f;
    outCtx.animFrameCount = 0;
    outCtx.animCycle = false;
    outCtx.animRandomStart = false;
    if ( !outCtx.isStaticProvider )
    {
        const AssetPtr<AnimationAsset>& animAsset = pParticleAssetEmitter->getAnimationAsset();
        if ( animAsset.notNull() && animAsset->getImage().notNull() )
        {
            outCtx.animFrameCount = animAsset->getNamedCellsMode()
                ? (U32)animAsset->getValidatedNamedAnimationFrames().size()
                : (U32)animAsset->getValidatedAnimationFrames().size();
            outCtx.animTotalTime = animAsset->getAnimationTime();
            outCtx.animCycle = animAsset->getAnimationCycle();
            outCtx.animRandomStart = animAsset->getRandomStart();
        }
    }
}

//------------------------------------------------------------------------------

void ParticlePlayer::configureParticle( EmitterNode* pEmitterNode, ParticleSystem::ParticleNode* pParticleNode, const EmitterTickContext& ctx )
{
    // Fetch the particle player age.
    const F32 particlePlayerAge = mAge;

    // Fetch the particle player position.
    const Vector2& particlePlayerPosition = getPosition();

    // Fetch particle asset.
    ParticleAsset* pParticleAsset = mParticleAsset;

    // Fetch the asset emitter.
    ParticleAssetEmitter* pParticleAssetEmitter = ctx.pAssetEmitter;


    // **********************************************************************************************************************
    // Calculate Particle Position.
    // **********************************************************************************************************************

    // Fetch attachment options.
    const bool attachPositionToEmitter = pParticleAssetEmitter->getAttachPositionToEmitter();

    // Fetch the emitter offset, angle and size
    const Vector2& emitterOffset = pParticleAssetEmitter->getEmitterOffset() * getSizeScale();
    const Vector2& emitterSize = pParticleAssetEmitter->getEmitterSize() * getSizeScale();
    const F32 emitterAngle = mDegToRad(pParticleAssetEmitter->getEmitterAngle());

    // Are we using Single Particle?
    if ( ctx.singleParticle )
    {
        // Determine whether to use world-space or emitter-space.
        if ( attachPositionToEmitter )
        {
            pParticleNode->mPosition = emitterOffset;
        }
        else
        {
            pParticleNode->mPosition = particlePlayerPosition + emitterOffset;
        }
    }
    else
    {
        // No, so select Emitter-Type.
        switch( pParticleAssetEmitter->getEmitterType() )
        {
            // Emit at a point defined by the emitters position.
            case ParticleAssetEmitter::POINT_EMITTER:
            {
                // Are we attaching the position to the emitter?
                if ( attachPositionToEmitter )
                {
                    // Yes, so transform the particle into emitter-space only.
                    pParticleNode->mPosition = emitterOffset;
                }
                else
                {
                    // No, so transform the particle into world-space here.
                    pParticleNode->mPosition = emitterOffset + particlePlayerPosition;
                }

            } break;

            // Emit along a line defined by the emitters width.
            case ParticleAssetEmitter::LINE_EMITTER:
            {
                // Calculate half-width.
                const F32 halfWidth = emitterSize.x * 0.5f;

                // Calculate emitter position.
                Vector2 emissionPosition( CoreMath::mGetRandomF( -halfWidth, halfWidth ), 0.0f );

                // Transform particle position in emitter-space.
                pParticleNode->mPosition = b2Mul( b2Rot(emitterAngle), emissionPosition ) + emitterOffset;

                // Are we attaching the position to the emitter?
                if ( !attachPositionToEmitter )
                {
                    // No, so transform the particle into world-space here.
                    b2Transform xform( particlePlayerPosition, b2Rot( getAngle()) );
                    pParticleNode->mPosition = b2Mul( xform, pParticleNode->mPosition );
                }

            } break;

            // Emit inside a box defined by the emitters size.
            case ParticleAssetEmitter::BOX_EMITTER:
            {
                // Calculate half-width/height.
                const F32 halfWidth = emitterSize.x * 0.5f;
                const F32 halfHeight = emitterSize.y * 0.5f;

                // Calculate emitter position.
                Vector2 emissionPosition( CoreMath::mGetRandomF( -halfWidth, halfWidth ), CoreMath::mGetRandomF( -halfHeight, halfHeight ) );

                // Transform particle position in emitter-space.
                pParticleNode->mPosition = b2Mul( b2Rot(emitterAngle), emissionPosition ) + emitterOffset;

                // Are we attaching the position to the emitter?
                if ( !attachPositionToEmitter )
                {
                    // No, so transform the particle into world-space here.
                    b2Transform xform( particlePlayerPosition, b2Rot( getAngle()) );
                    pParticleNode->mPosition = b2Mul( xform, pParticleNode->mPosition );
                }

            } break;

            // Emit from an ellipse with a radii defined by the emitters size.
            case ParticleAssetEmitter::DISK_EMITTER:
            {
                // Calculate the random angle.
                const F32 angle = CoreMath::mGetRandomF( 0.0f, b2_pi2 );
#if 1
                // Calculate the uniform distribution scale.
                const F32 distributionScale = mSqrt( CoreMath::mGetRandomF(0.0f, 1.0f) );

                // Calculate the radii.
                const F32 radiusX = emitterSize.x * 0.5f * distributionScale;
                const F32 radiusY = emitterSize.y * 0.5f * distributionScale;
#else
                // Calculate the radii.
                const F32 radiusX = emitterSize.x * 0.5f;
                const F32 radiusY = emitterSize.y * 0.5f;
#endif
                // Calculate emitter position using a uniform distribution.
                Vector2 emissionPosition( radiusX * mCos(angle), radiusY * mSin(angle) );

                // Transform particle position in emitter-space.
                pParticleNode->mPosition = b2Mul( b2Rot(emitterAngle), emissionPosition ) + emitterOffset;

                // Are we attaching the position to the emitter?
                if ( !attachPositionToEmitter )
                {
                    // No, so transform the particle into world-space here.
                    b2Transform xform( particlePlayerPosition, b2Rot( getAngle()) );
                    pParticleNode->mPosition = b2Mul( xform, pParticleNode->mPosition );
                }

            } break;

            // Emit from an ellipse with a radii defined by the emitters size.
            case ParticleAssetEmitter::ELLIPSE_EMITTER:
            {
                // Calculate the random angle.
                const F32 angle = CoreMath::mGetRandomF( 0.0f, b2_pi2 );

                // Calculate emitter position using a uniform distribution.
                Vector2 emissionPosition( emitterSize.x * 0.5f * mCos(angle), emitterSize.y * 0.5f * mSin(angle) );

                // Transform particle position in emitter-space.
                pParticleNode->mPosition = b2Mul( b2Rot(emitterAngle), emissionPosition ) + emitterOffset;

                // Are we attaching the position to the emitter?
                if ( !attachPositionToEmitter )
                {
                    // No, so transform the particle into world-space here.
                    b2Transform xform( particlePlayerPosition, b2Rot( getAngle()) );
                    pParticleNode->mPosition = b2Mul( xform, pParticleNode->mPosition );
                }

            } break;

            // Emit inside a torus with an outer-diameter defined by the emitters major size axis and an inner-diameter defined by the emitters minor size axis..
            case ParticleAssetEmitter::TORUS_EMITTER:
            {
                // Calculate the random angle.
                const F32 angle = CoreMath::mGetRandomF( 0.0f, b2_pi2 );

                // Calculate the inner and outer radii.
                const F32 outerRadii = emitterSize.getMajorAxis() * 0.5f;
                const F32 innerRadii = emitterSize.getMinorAxis() * 0.5f;
#if 1
                // Calculate the radius as a uniform distribution.
                const F32 radius = innerRadii + ( mSqrt( CoreMath::mGetRandomF(0.0f, 1.0f) ) * (outerRadii-innerRadii) );
#else
                // Calculate the radius as a non-uniform distribution.
                const F32 radius = CoreMath::mGetRandomF( innerRadii, outerRadii );
#endif
                // Calculate emitter position using a uniform distribution.
                Vector2 emissionPosition( radius * mCos(angle), radius * mSin(angle) );

                // Are we attaching the position to the emitter?
                if ( attachPositionToEmitter )
                {
                    // Yes, so transform the particle into emitter-space only.
                    pParticleNode->mPosition = emissionPosition + emitterOffset;
                }
                else
                {
                    // No, so transform the particle into world-space here.
                    pParticleNode->mPosition = emissionPosition + emitterOffset + particlePlayerPosition;
                }

            } break;
                
            default:
                pParticleNode->mPosition.SetZero();
                break;
        }
    }

    // **********************************************************************************************************************
    // Calculate Particle Lifetime.
    // **********************************************************************************************************************

    pParticleNode->mParticleAge = 0.0f;
    pParticleNode->mParticleLifetime = ParticleAssetField::calculateFieldBVE(   pParticleAssetEmitter->getParticleLifeBaseField(),
                                                                                pParticleAssetEmitter->getParticleLifeVariationField(),
                                                                                pParticleAsset->getParticleLifeScaleField(),
                                                                                particlePlayerAge );


    // **********************************************************************************************************************
    // Calculate Particle Size-X / Y.
    // **********************************************************************************************************************

    pParticleNode->mSize.x = ParticleAssetField::calculateFieldBVE( pParticleAssetEmitter->getSizeXBaseField(),
                                                                    pParticleAssetEmitter->getSizeXVariationField(),
                                                                    pParticleAsset->getSizeXScaleField(),
                                                                    particlePlayerAge ) * getSizeScale();

    if ( ctx.fixedAspect )
    {
        pParticleNode->mSize.y = pParticleNode->mSize.x;
    }
    else
    {
        pParticleNode->mSize.y = ParticleAssetField::calculateFieldBVE( pParticleAssetEmitter->getSizeYBaseField(),
                                                                        pParticleAssetEmitter->getSizeYVariationField(),
                                                                        pParticleAsset->getSizeYScaleField(),
                                                                        particlePlayerAge ) * getSizeScale();
    }

    pParticleNode->mRenderSize.Set(-1.0f, -1.0f);


    // **********************************************************************************************************************
    // Calculate Speed, Random Motion and Emission Angle.
    // **********************************************************************************************************************

    F32 emissionForce = 0.0f;
    F32 emissionAngle = 0.0f;
    F32 emissionArc = 0.0f;

    if ( !ctx.singleParticle )
    {
        pParticleNode->mSpeed = ParticleAssetField::calculateFieldBVE(  pParticleAssetEmitter->getSpeedBaseField(),
                                                                        pParticleAssetEmitter->getSpeedVariationField(),
                                                                        pParticleAsset->getSpeedScaleField(),
                                                                        particlePlayerAge ) * getForceScale();

        pParticleNode->mRandomMotion = ParticleAssetField::calculateFieldBVE(   pParticleAssetEmitter->getRandomMotionBaseField(),
                                                                                pParticleAssetEmitter->getRandomMotionVariationField(),
                                                                                pParticleAsset->getRandomMotionScaleField(),
                                                                                particlePlayerAge ) * getForceScale();

        emissionForce = ParticleAssetField::calculateFieldBV(   pParticleAssetEmitter->getEmissionForceForceBaseField(),
                                                                pParticleAssetEmitter->getEmissionForceVariationField(),
                                                                particlePlayerAge) * getForceScale();

        if (pParticleAssetEmitter->getIsTargeting())
        {
           Vector2 tPos = pParticleAssetEmitter->getTargetPosition();
           Vector2 pPos = pParticleNode->mPosition;
           Vector2 subVec = tPos - pPos;
           F32 vecN = mAtan(subVec.x, subVec.y);
           F32 vecDeg = mRadToDeg(vecN);
           emissionAngle = vecDeg;
        }
        else
        {
           emissionAngle = ParticleAssetField::calculateFieldBV(  pParticleAssetEmitter->getEmissionAngleBaseField(),
                                                                  pParticleAssetEmitter->getEmissionAngleVariationField(),
                                                                  particlePlayerAge);
        }

        // Half the emission arc.
        emissionArc = ParticleAssetField::calculateFieldBV( pParticleAssetEmitter->getEmissionArcBaseField(),
                                                            pParticleAssetEmitter->getEmissionArcVariationField(),
                                                            particlePlayerAge ) * 0.5f;

        // Add the particle player's own angle if the emission rotation is linked.
        emissionAngle = calculateEmissionAngle( emissionAngle, getAngle(), pParticleAssetEmitter->getLinkEmissionRotation() );

        // Calculate the final emission angle choosing random Arc.
        const F32 randomEmissionAngle = (emissionArc != 0.0f)
            ? CoreMath::mGetRandomF( emissionAngle - emissionArc, emissionAngle + emissionArc )
            : emissionAngle;
        emissionAngle = mFmod( randomEmissionAngle, 360.0f );

        // Calculate the particle velocity.
        if ( emissionForce != 0.0f )
        {
            const F32 emissionAngleRadians = mDegToRad( emissionAngle );
            pParticleNode->mVelocity.Set( emissionForce * mCos( emissionAngleRadians ), emissionForce * mSin( emissionAngleRadians ) );
        }
        else
        {
            pParticleNode->mVelocity.SetZero();
        }
    }
    else
    {
        pParticleNode->mSpeed = 0.0f;
        pParticleNode->mRandomMotion = 0.0f;
        pParticleNode->mVelocity.SetZero();
    }


    // **********************************************************************************************************************
    // Calculate Spin and Fixed-Force.
    // **********************************************************************************************************************

    pParticleNode->mSpin = ParticleAssetField::calculateFieldBVE(   pParticleAssetEmitter->getSpinBaseField(),
                                                                    pParticleAssetEmitter->getSpinVariationField(),
                                                                    pParticleAsset->getSpinScaleField(),
                                                                    particlePlayerAge );

    pParticleNode->mFixedForce = ParticleAssetField::calculateFieldBVE( pParticleAssetEmitter->getFixedForceBaseField(),
                                                                        pParticleAssetEmitter->getFixedForceVariationField(),
                                                                        pParticleAsset->getFixedForceScaleField(),
                                                                        particlePlayerAge ) * getForceScale();


    // **********************************************************************************************************************
    // Calculate Orientation Angle.
    // **********************************************************************************************************************

    pParticleNode->mOrientationAngle = 0.0f;
    switch( pParticleAssetEmitter->getOrientationType() )
    {
        // Aligned to initial emission.
        case ParticleAssetEmitter::ALIGNED_ORIENTATION:
        {
            // Use the emission angle with fixed offset.
            pParticleNode->mOrientationAngle = mFmod( emissionAngle - ctx.alignedAngleOffset, 360.0f );

        } break;

        // Fixed orientation.
        case ParticleAssetEmitter::FIXED_ORIENTATION:
        {
            // Use a fixed angle.
            pParticleNode->mOrientationAngle = mFmod( pParticleAssetEmitter->getFixedAngleOffset(), 360.0f );

        } break;

        // Random with constraints.
        case ParticleAssetEmitter::RANDOM_ORIENTATION:
        {
            // Used a random angle/arc.
            const F32 randomArc = pParticleAssetEmitter->getRandomArc() * 0.5f;
            pParticleNode->mOrientationAngle = mFmod( CoreMath::mGetRandomF( pParticleAssetEmitter->getRandomAngleOffset() - randomArc, pParticleAssetEmitter->getRandomAngleOffset() + randomArc ), 360.0f );

        } break;
        
        default:
            break;
    }

    // **********************************************************************************************************************
    // Image, Frame and Animation Controller.
    // **********************************************************************************************************************

    if ( ctx.isStaticProvider )
    {
        pParticleNode->mAnimationTime = 0.0f;
        pParticleNode->mAnimationFinished = true;
        if ( pParticleAssetEmitter->getRandomImageFrame() )
        {
            const AssetPtr<ImageAsset>& imageAsset = pParticleAssetEmitter->getImageAsset();
            const U32 frameCount = imageAsset.notNull() ? imageAsset->getFrameCount() : 0;
            pParticleNode->mFrameIndex = (frameCount > 0) ? CoreMath::mGetRandomI( 0, (S32)frameCount - 1 ) : 0;
        }
        else
        {
            pParticleNode->mFrameIndex = (S32)pParticleAssetEmitter->getImageFrame();
        }
    }
    else
    {
        if ( ctx.animFrameCount > 0 )
        {
            pParticleNode->mAnimationTime = (ctx.animRandomStart && ctx.animTotalTime > 0.0f)
                ? CoreMath::mGetRandomF( 0.0f, ctx.animTotalTime * 0.999f )
                : 0.0f;
            pParticleNode->mAnimationFinished = false;
            pParticleNode->mFrameIndex = 0;
        }
        else
        {
            pParticleNode->mAnimationTime = 0.0f;
            pParticleNode->mAnimationFinished = true;
            pParticleNode->mFrameIndex = -1;
        }
    }

    // Reset tick position and integrate age 0 to populate initial render size, color, and animation frame.
    pParticleNode->mPreTickPosition = pParticleNode->mPosition;
    integrateParticle( ctx, pParticleNode, 0.0f, 0.0f );
}

//------------------------------------------------------------------------------

void ParticlePlayer::integrateParticle( const EmitterTickContext& ctx, ParticleSystem::ParticleNode* pParticleNode, const F32 particleAge, const F32 elapsedTime )
{
    // Record pre-tick position before this tick's integration step.
    pParticleNode->mPreTickPosition = pParticleNode->mPosition;

    // **********************************************************************************************************************
    // Scale Size.
    // **********************************************************************************************************************

    pParticleNode->mRenderSize.x = (pParticleNode->mSize.x != 0.0f || ctx.sizeXMin > 0.0f)
        ? mClampF( pParticleNode->mSize.x * ctx.sizeXLife->getFieldValue( particleAge ), ctx.sizeXMin, ctx.sizeXMax )
        : 0.0f;

    if ( ctx.fixedAspect )
    {
        pParticleNode->mRenderSize.y = pParticleNode->mRenderSize.x;
    }
    else
    {
        pParticleNode->mRenderSize.y = (pParticleNode->mSize.y != 0.0f || ctx.sizeYMin > 0.0f)
            ? mClampF( pParticleNode->mSize.y * ctx.sizeYLife->getFieldValue( particleAge ), ctx.sizeYMin, ctx.sizeYMax )
            : 0.0f;
    }


    // **********************************************************************************************************************
    // Calculate RGBA Components (hoisted constant channels and effect-age alpha scale).
    // **********************************************************************************************************************

    pParticleNode->mColor.red   = ctx.redConst   ? ctx.constRed   : mClampF( ctx.redLife->getFieldValue( particleAge ), ctx.redMin, ctx.redMax );
    pParticleNode->mColor.green = ctx.greenConst ? ctx.constGreen : mClampF( ctx.greenLife->getFieldValue( particleAge ), ctx.greenMin, ctx.greenMax );
    pParticleNode->mColor.blue  = ctx.blueConst  ? ctx.constBlue  : mClampF( ctx.blueLife->getFieldValue( particleAge ), ctx.blueMin, ctx.blueMax );
    pParticleNode->mColor.alpha = ctx.alphaConst ? ctx.constAlpha : mClampF( ctx.alphaLife->getFieldValue( particleAge ) * ctx.alphaScale, ctx.alphaMin, ctx.alphaMax );


    // **********************************************************************************************************************
    // Update Animation.
    // **********************************************************************************************************************

    if ( !ctx.isStaticProvider && ctx.animFrameCount > 0 )
    {
        pParticleNode->mFrameIndex = stepParticleAnimation(
            pParticleNode->mAnimationTime,
            pParticleNode->mAnimationFinished,
            elapsedTime,
            ctx.animTotalTime,
            ctx.animFrameCount,
            ctx.animCycle );
    }


    // **********************************************************************************************************************
    // Calculate Velocity and Position.
    // **********************************************************************************************************************

    if ( !ctx.singleParticle && elapsedTime != 0.0f )
    {
        if ( pParticleNode->mRandomMotion != 0.0f || ctx.randomMotionMin > 0.0f )
        {
            const F32 renderRandomMotion = mClampF( pParticleNode->mRandomMotion * ctx.randomMotionLife->getFieldValue( particleAge ), ctx.randomMotionMin, ctx.randomMotionMax );
            if ( mNotZero( renderRandomMotion ) )
            {
                const F32 halfRandom = renderRandomMotion * 0.5f * elapsedTime;
                pParticleNode->mVelocity.x += CoreMath::mGetRandomF( -halfRandom, halfRandom );
                pParticleNode->mVelocity.y += CoreMath::mGetRandomF( -halfRandom, halfRandom );
            }
        }

        if ( pParticleNode->mFixedForce != 0.0f || ctx.fixedForceMin > 0.0f || ctx.fixedForceMax < 0.0f )
        {
            const F32 renderFixedForce = mClampF( pParticleNode->mFixedForce * ctx.fixedForceLife->getFieldValue( particleAge ), ctx.fixedForceMin, ctx.fixedForceMax );
            if ( mNotZero( renderFixedForce ) )
            {
                pParticleNode->mVelocity += ctx.fixedForceDirScaled * renderFixedForce;
            }
        }

        if ( (pParticleNode->mVelocity.x != 0.0f || pParticleNode->mVelocity.y != 0.0f) &&
             (pParticleNode->mSpeed != 0.0f || ctx.speedMin > 0.0f) )
        {
            const F32 renderSpeed = mClampF( pParticleNode->mSpeed * ctx.speedLife->getFieldValue( particleAge ), ctx.speedMin, ctx.speedMax );
            if ( renderSpeed != 0.0f )
            {
                const F32 step = renderSpeed * elapsedTime;
                pParticleNode->mPosition.x += pParticleNode->mVelocity.x * step;
                pParticleNode->mPosition.y += pParticleNode->mVelocity.y * step;
            }
        }
    }


    // **********************************************************************************************************************
    // Orientation / Spin.
    // **********************************************************************************************************************

    if ( ctx.keepAligned )
    {
        if ( pParticleNode->mVelocity.x != 0.0f || pParticleNode->mVelocity.y != 0.0f )
        {
            F32 movementAngle = mRadToDeg( mAtan( pParticleNode->mVelocity.x, pParticleNode->mVelocity.y ) );
            if ( movementAngle < 0.0f )
                movementAngle += 360.0f;
            pParticleNode->mOrientationAngle = movementAngle - ctx.alignedAngleOffset;
        }
        else
        {
            pParticleNode->mOrientationAngle = -ctx.alignedAngleOffset;
        }
    }
    else if ( pParticleNode->mSpin != 0.0f && elapsedTime != 0.0f )
    {
        const F32 renderSpin = pParticleNode->mSpin * ctx.spinLife->getFieldValue( particleAge );
        if ( mNotZero( renderSpin ) )
        {
            pParticleNode->mOrientationAngle += renderSpin * elapsedTime;
            if ( pParticleNode->mOrientationAngle >= 360.0f || pParticleNode->mOrientationAngle <= -360.0f )
                pParticleNode->mOrientationAngle = mFmod( pParticleNode->mOrientationAngle, 360.0f );
        }
    }
}

//-----------------------------------------------------------------------------

void ParticlePlayer::onTamlAddParent( SimObject* pParentObject )
{
    // Call parent.
    Parent::onTamlAddParent( pParentObject );

    // Play  automatically when added to a parent.
    play( true );
}

//-----------------------------------------------------------------------------

void ParticlePlayer::initializeParticleAsset( void )
{
    // Note if we were playing.
    const bool wasPlaying = getIsPlaying();

    // Destroy any existing particle asset.
    destroyParticleAsset();

    // Finish if no particle asset.
    if ( mParticleAsset.isNull() )
        return;

    // Fetch the particle asset.
    ParticleAsset* pParticleAsset = mParticleAsset;

    // Fetch the emitter count.
    const U32 emitterCount = pParticleAsset->getEmitterCount();

    // Finish if no emitters found.
    if ( emitterCount == 0 )
        return;

    // Add each emitter reference.
    for( U32 emitterIndex = 0; emitterIndex < emitterCount; ++emitterIndex )
    {
        // Fetch the asset emitter.
        ParticleAssetEmitter* pParticleAssetEmitter = pParticleAsset->getEmitter( emitterIndex );

        // Fetch both image and animation assets.
        const AssetPtr<ImageAsset>& imageAsset = pParticleAssetEmitter->getImageAsset();
        const AssetPtr<AnimationAsset>& animationAsset = pParticleAssetEmitter->getAnimationAsset();

        // Skip if the emitter does not have a valid assigned asset to render.
        if (( pParticleAssetEmitter->isStaticFrameProvider() && (imageAsset.isNull() || imageAsset->getFrameCount() == 0 ) ) ||
            ( !pParticleAssetEmitter->isStaticFrameProvider() && (animationAsset.isNull() || (animationAsset->getValidatedAnimationFrames().size() == 0 && animationAsset->getValidatedNamedAnimationFrames().size() == 0)) ) )
            continue;

        // Create a new emitter node.
        EmitterNode* pEmitterNode = new EmitterNode( this, pParticleAssetEmitter );

        // Store new emitter node.
        mEmitters.push_back( pEmitterNode );
    }

    // Start playing if we were playing before the update.
    if ( wasPlaying )
        play( false );
}

//-----------------------------------------------------------------------------

void ParticlePlayer::destroyParticleAsset( void )
{
    // Stop playing.
    stop( false, false );

    // Destroy all emitters.
    while( mEmitters.size() > 0 )
    {
        delete mEmitters[mEmitters.size()-1];
        mEmitters.pop_back();
    }
    mEmitters.clear();
}
