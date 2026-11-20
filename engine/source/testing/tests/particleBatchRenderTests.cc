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

#ifndef TORQUE_SHIPPING

#include <memory>
#include <type_traits>

#ifndef _UNIT_TESTING_H_
#include "testing/unitTesting.h"
#endif

#ifndef _DGL_H_
#include "graphics/dgl.h"
#endif

#ifndef _BATCH_RENDER_H_
#include "2d/core/BatchRender.h"
#endif

#ifndef _PARTICLE_PLAYER_H_
#include "2d/sceneobject/ParticlePlayer.h"
#endif

#ifndef _DEBUG_STATS_H_
#include "2d/scene/DebugStats.h"
#endif

#ifndef _TEXTURE_HANDLE_H_
#include "graphics/TextureHandle.h"
#endif

namespace
{
   void transformMatrixPoint2D( const F32 m[16], const Vector2& p, Vector2& out )
   {
      out.x = m[0] * p.x + m[4] * p.y + m[12];
      out.y = m[1] * p.x + m[5] * p.y + m[13];
   }

   void expectVertexNear( const DGLVertex& v, F32 x, F32 y, F32 u, F32 vTex, F32 r, F32 g, F32 b, F32 a )
   {
      EXPECT_FLOAT_EQ( v.x, x );
      EXPECT_FLOAT_EQ( v.y, y );
      EXPECT_FLOAT_EQ( v.z, 0.0f );
      EXPECT_FLOAT_EQ( v.u, u );
      EXPECT_FLOAT_EQ( v.v, vTex );
      EXPECT_FLOAT_EQ( v.r, r );
      EXPECT_FLOAT_EQ( v.g, g );
      EXPECT_FLOAT_EQ( v.b, b );
      EXPECT_FLOAT_EQ( v.a, a );
   }
}

TEST( DglMatrixGenerationTests, TracksMutationsAcrossStacksWithoutTouchingPush )
{
   dglResetMatrixStacks();

   const U32 initMv = dglGetModelviewGeneration();
   const U32 initProj = dglGetProjectionGeneration();
   EXPECT_NE( initMv, 0U );
   EXPECT_NE( initProj, 0U );

   // Switching matrix mode and pushing an identical matrix must not invalidate uniforms.
   dglMatrixMode( GL_MODELVIEW );
   dglPushMatrix();
   EXPECT_EQ( dglGetModelviewGeneration(), initMv );
   EXPECT_EQ( dglGetProjectionGeneration(), initProj );

   dglTranslatef( 5.0f, -3.0f, 0.0f );
   const U32 afterTrans = dglGetModelviewGeneration();
   EXPECT_NE( afterTrans, initMv );
   EXPECT_EQ( dglGetProjectionGeneration(), initProj );

   dglRotatef( 45.0f, 0.0f, 0.0f, 1.0f );
   const U32 afterRot = dglGetModelviewGeneration();
   EXPECT_NE( afterRot, afterTrans );

   dglScalef( 2.0f, 2.0f, 1.0f );
   const U32 afterScale = dglGetModelviewGeneration();
   EXPECT_NE( afterScale, afterRot );

   dglLoadIdentity();
   const U32 afterIdent = dglGetModelviewGeneration();
   EXPECT_NE( afterIdent, afterScale );

   dglPopMatrix();
   const U32 afterPop = dglGetModelviewGeneration();
   EXPECT_NE( afterPop, afterIdent );
   EXPECT_EQ( dglGetProjectionGeneration(), initProj );

   dglMatrixMode( GL_PROJECTION );
   dglPushMatrix();
   EXPECT_EQ( dglGetProjectionGeneration(), initProj );

   dglOrtho( -10.0, 10.0, -10.0, 10.0, 0.0, 10.0 );
   const U32 afterOrtho = dglGetProjectionGeneration();
   EXPECT_NE( afterOrtho, initProj );
   EXPECT_EQ( dglGetModelviewGeneration(), afterPop );

   dglLoadIdentity();
   const U32 afterProjIdent = dglGetProjectionGeneration();
   EXPECT_NE( afterProjIdent, afterOrtho );

   dglFrustum( -1.0, 1.0, -1.0, 1.0, 1.0, 100.0 );
   const U32 afterFrustum = dglGetProjectionGeneration();
   EXPECT_NE( afterFrustum, afterProjIdent );

   dglPopMatrix();
   EXPECT_NE( dglGetProjectionGeneration(), afterFrustum );
   EXPECT_EQ( dglGetModelviewGeneration(), afterPop );

   dglResetMatrixStacks();
}

TEST( BatchRenderInterleavedTests, MixedBlendColorsAndVertexColorsDoNotFlush )
{
   // BatchRender embeds large arrays (~2.4 MB); always heap-allocate to avoid thread stack overflow.
   std::unique_ptr<BatchRender> batch( new BatchRender() );
   DebugStats stats;
   batch->setDebugStats( &stats );
   batch->setStrictOrderMode( true, true );

   // Changing alpha test on an empty batch updates state without counting an alpha-state flush.
   batch->setAlphaTestMode( 0.5f );
   EXPECT_FLOAT_EQ( batch->getAlphaTestMode(), 0.5f );
   EXPECT_EQ( stats.batchAlphaStateFlush, 0U );
   batch->setAlphaTestMode( -1.0f );

   TextureHandle emptyTex;

   // Quad 0: untinted quad with red mBlendColor.
   batch->setBlendMode( GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA, ColorF( 1.0f, 0.0f, 0.0f, 0.5f ) );
   batch->SubmitQuad(
      Vector2( 0.0f, 0.0f ), Vector2( 1.0f, 0.0f ),
      Vector2( 1.0f, 1.0f ), Vector2( 0.0f, 1.0f ),
      Vector2( 0.0f, 1.0f ), Vector2( 1.0f, 1.0f ),
      Vector2( 1.0f, 0.0f ), Vector2( 0.0f, 0.0f ),
      emptyTex );

   // Quad 1: changing only mBlendColor with same GL blend factors must NOT flush.
   batch->setBlendMode( GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA, ColorF( 0.0f, 1.0f, 0.0f, 0.25f ) );
   batch->SubmitQuad(
      Vector2( 2.0f, 0.0f ), Vector2( 3.0f, 0.0f ),
      Vector2( 3.0f, 1.0f ), Vector2( 2.0f, 1.0f ),
      Vector2( 0.0f, 1.0f ), Vector2( 1.0f, 1.0f ),
      Vector2( 1.0f, 0.0f ), Vector2( 0.0f, 0.0f ),
      emptyTex );

   // Quad 2: single explicit vertex color mixed directly after untinted quads without color flush.
   batch->SubmitQuad(
      Vector2( 4.0f, 0.0f ), Vector2( 5.0f, 0.0f ),
      Vector2( 5.0f, 1.0f ), Vector2( 4.0f, 1.0f ),
      Vector2( 0.0f, 1.0f ), Vector2( 1.0f, 1.0f ),
      Vector2( 1.0f, 0.0f ), Vector2( 0.0f, 0.0f ),
      emptyTex,
      ColorF( 0.2f, 0.4f, 0.6f, 0.8f ) );

   // Quad 3: four explicit vertex colors preserving #2/#3 swap in interleaved buffer.
   batch->SubmitQuad(
      Vector2( 6.0f, 0.0f ), Vector2( 7.0f, 0.0f ),
      Vector2( 7.0f, 1.0f ), Vector2( 6.0f, 1.0f ),
      Vector2( 0.1f, 0.9f ), Vector2( 0.9f, 0.9f ),
      Vector2( 0.9f, 0.1f ), Vector2( 0.1f, 0.1f ),
      emptyTex,
      ColorF( 1.0f, 0.0f, 0.0f, 1.0f ),
      ColorF( 0.0f, 1.0f, 0.0f, 1.0f ),
      ColorF( 0.0f, 0.0f, 1.0f, 1.0f ),
      ColorF( 1.0f, 1.0f, 0.0f, 1.0f ) );

   EXPECT_EQ( stats.batchFlushes, 0U );
   EXPECT_EQ( stats.batchBlendStateFlush, 0U );
   EXPECT_EQ( stats.batchColorStateFlush, 0U );
   EXPECT_EQ( stats.batchAlphaStateFlush, 0U );
   EXPECT_EQ( stats.batchTrianglesSubmitted, 8U );
   EXPECT_EQ( batch->getTriangleCount(), 8U );
   EXPECT_EQ( batch->getVertexCount(), 16U );
   EXPECT_EQ( batch->getIndexCount(), 24U );

   const DGLVertex* verts = batch->getVertexBuffer();
   const U16* indices = batch->getIndexBuffer();

   // Quad 0 (red mBlendColor, slot 2 holds vertexPos3 / uv3 and slot 3 holds vertexPos2 / uv2).
   expectVertexNear( verts[0], 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.5f );
   expectVertexNear( verts[1], 1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.5f );
   expectVertexNear( verts[2], 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.5f );
   expectVertexNear( verts[3], 1.0f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.5f );

   // Quad 1 (green mBlendColor).
   expectVertexNear( verts[4], 2.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.25f );
   expectVertexNear( verts[7], 3.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.25f );

   // Quad 2 (single explicit color).
   expectVertexNear( verts[8],  4.0f, 0.0f, 0.0f, 1.0f, 0.2f, 0.4f, 0.6f, 0.8f );
   expectVertexNear( verts[11], 5.0f, 1.0f, 1.0f, 0.0f, 0.2f, 0.4f, 0.6f, 0.8f );

   // Quad 3 (4 explicit colors with #2/#3 swap: slot 2 gets color3, slot 3 gets color2).
   expectVertexNear( verts[12], 6.0f, 0.0f, 0.1f, 0.9f, 1.0f, 0.0f, 0.0f, 1.0f );
   expectVertexNear( verts[13], 7.0f, 0.0f, 0.9f, 0.9f, 0.0f, 1.0f, 0.0f, 1.0f );
   expectVertexNear( verts[14], 6.0f, 1.0f, 0.1f, 0.1f, 1.0f, 1.0f, 0.0f, 1.0f );
   expectVertexNear( verts[15], 7.0f, 1.0f, 0.9f, 0.1f, 0.0f, 0.0f, 1.0f, 1.0f );

   // Index ordering for every quad q: [base, base+1, base+2, base+3, base+2, base+1].
   for ( U32 q = 0; q < 4; ++q )
   {
      const U16 base = (U16)(q * 4);
      EXPECT_EQ( indices[q * 6 + 0], base );
      EXPECT_EQ( indices[q * 6 + 1], base + 1 );
      EXPECT_EQ( indices[q * 6 + 2], base + 2 );
      EXPECT_EQ( indices[q * 6 + 3], base + 3 );
      EXPECT_EQ( indices[q * 6 + 4], base + 2 );
      EXPECT_EQ( indices[q * 6 + 5], base + 1 );
   }
}

TEST( BatchRenderInterleavedTests, BlendOffUsesWhiteDefaultAndPreservesExplicitColors )
{
   std::unique_ptr<BatchRender> batch( new BatchRender() );
   DebugStats stats;
   batch->setDebugStats( &stats );
   batch->setStrictOrderMode( true, true );
   batch->setBlendMode( GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA, ColorF( 0.3f, 0.2f, 0.1f, 0.5f ) );
   batch->setBlendOff();

   TextureHandle emptyTex;
   batch->SubmitQuad(
      Vector2( 0.0f, 0.0f ), Vector2( 1.0f, 0.0f ),
      Vector2( 1.0f, 1.0f ), Vector2( 0.0f, 1.0f ),
      Vector2( 0.0f, 1.0f ), Vector2( 1.0f, 1.0f ),
      Vector2( 1.0f, 0.0f ), Vector2( 0.0f, 0.0f ),
      emptyTex );

   batch->SubmitQuad(
      Vector2( 1.0f, 0.0f ), Vector2( 2.0f, 0.0f ),
      Vector2( 2.0f, 1.0f ), Vector2( 1.0f, 1.0f ),
      Vector2( 0.0f, 1.0f ), Vector2( 1.0f, 1.0f ),
      Vector2( 1.0f, 0.0f ), Vector2( 0.0f, 0.0f ),
      emptyTex,
      ColorF( 0.25f, 0.5f, 0.75f, 1.0f ) );

   EXPECT_EQ( stats.batchFlushes, 0U );
   EXPECT_EQ( stats.batchColorStateFlush, 0U );
   EXPECT_EQ( batch->getVertexCount(), 8U );

   const DGLVertex* verts = batch->getVertexBuffer();
   expectVertexNear( verts[0], 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f );
   expectVertexNear( verts[4], 1.0f, 0.0f, 0.0f, 1.0f, 0.25f, 0.5f, 0.75f, 1.0f );
}

TEST( ParticlePlayerBatchMathTests, UsesPerEmitterBlendStateInsteadOfSceneObjectState )
{
   SceneObject sceneObject;
   ParticlePlayer particlePlayer;

   EXPECT_TRUE( sceneObject.usesObjectBlendState() );
   EXPECT_FALSE( particlePlayer.usesObjectBlendState() );
}

TEST( ParticlePlayerBatchMathTests, AttachedOOBBTransformMatchesDglMatrixStack )
{
   const Vector2 localOOBB[4] = {
      Vector2( -1.5f, -0.75f ),
      Vector2(  2.0f, -0.75f ),
      Vector2(  2.0f,  1.25f ),
      Vector2( -1.5f,  1.25f )
   };
   const Vector2 playerPos( 12.5f, -7.25f );

   // Translation only (attachPosition=true, attachRotation=false).
   {
      Vector2 cpuOut[4];
      ParticlePlayer::transformAttachedOOBB( localOOBB, cpuOut, playerPos, mDegToRad( 45.0f ), false );

      dglResetMatrixStacks();
      dglMatrixMode( GL_MODELVIEW );
      dglLoadIdentity();
      dglTranslatef( playerPos.x, playerPos.y, 0.0f );
      F32 mv[16];
      dglGetModelviewColumnMajor( mv );

      for ( U32 i = 0; i < 4; ++i )
      {
         Vector2 dglOut;
         transformMatrixPoint2D( mv, localOOBB[i], dglOut );
         EXPECT_NEAR( cpuOut[i].x, dglOut.x, 1e-5f );
         EXPECT_NEAR( cpuOut[i].y, dglOut.y, 1e-5f );
      }
   }

   // Translation + Z rotation across representative angles.
   const F32 anglesDeg[] = { 0.0f, 45.0f, 90.0f, -135.0f };
   for ( F32 angleDeg : anglesDeg )
   {
      const F32 angleRad = mDegToRad( angleDeg );
      Vector2 cpuOut[4];
      ParticlePlayer::transformAttachedOOBB( localOOBB, cpuOut, playerPos, angleRad, true );

      dglResetMatrixStacks();
      dglMatrixMode( GL_MODELVIEW );
      dglLoadIdentity();
      dglTranslatef( playerPos.x, playerPos.y, 0.0f );
      dglRotatef( mRadToDeg( angleRad ), 0.0f, 0.0f, 1.0f );
      F32 mv[16];
      dglGetModelviewColumnMajor( mv );

      for ( U32 i = 0; i < 4; ++i )
      {
         Vector2 dglOut;
         transformMatrixPoint2D( mv, localOOBB[i], dglOut );
         EXPECT_NEAR( cpuOut[i].x, dglOut.x, 1e-4f );
         EXPECT_NEAR( cpuOut[i].y, dglOut.y, 1e-4f );
      }
   }

   dglResetMatrixStacks();
}

TEST( ParticlePlayerBatchMathTests, QuadFrustumOverlapAndZeroAlphaCullingPredicates )
{
   b2AABB cameraAABB;
   cameraAABB.lowerBound.Set( -50.0f, -37.5f );
   cameraAABB.upperBound.Set(  50.0f,  37.5f );

   // Strictly inside camera.
   const Vector2 inside[4] = {
      Vector2( -5.0f, -5.0f ), Vector2( 5.0f, -5.0f ),
      Vector2(  5.0f,  5.0f ), Vector2( -5.0f, 5.0f )
   };
   EXPECT_TRUE( ParticlePlayer::isQuadVisibleInAABB( inside, cameraAABB ) );

   // Edge touching right edge.
   const Vector2 touchEdge[4] = {
      Vector2( 50.0f, -2.0f ), Vector2( 55.0f, -2.0f ),
      Vector2( 55.0f,  2.0f ), Vector2( 50.0f,  2.0f )
   };
   EXPECT_TRUE( ParticlePlayer::isQuadVisibleInAABB( touchEdge, cameraAABB ) );

   // Partial overlap on left edge (rotated diamond).
   const Vector2 overlap[4] = {
      Vector2( -55.0f, 0.0f ), Vector2( -51.0f, -4.0f ),
      Vector2( -49.0f, 0.0f ), Vector2( -51.0f,  4.0f )
   };
   EXPECT_TRUE( ParticlePlayer::isQuadVisibleInAABB( overlap, cameraAABB ) );

   // Giant quad enclosing camera entirely.
   const Vector2 enclosing[4] = {
      Vector2( -100.0f, -100.0f ), Vector2( 100.0f, -100.0f ),
      Vector2(  100.0f,  100.0f ), Vector2( -100.0f, 100.0f )
   };
   EXPECT_TRUE( ParticlePlayer::isQuadVisibleInAABB( enclosing, cameraAABB ) );

   // Off-screen in all four directions.
   const Vector2 offLeft[4]   = { Vector2( -60.0f, -1.0f ), Vector2( -51.0f, -1.0f ), Vector2( -51.0f,  1.0f ), Vector2( -60.0f,  1.0f ) };
   const Vector2 offRight[4]  = { Vector2(  51.0f, -1.0f ), Vector2(  60.0f, -1.0f ), Vector2(  60.0f,  1.0f ), Vector2(  51.0f,  1.0f ) };
   const Vector2 offBottom[4] = { Vector2(  -1.0f, -45.0f ), Vector2(  1.0f, -45.0f ), Vector2(  1.0f, -38.0f ), Vector2( -1.0f, -38.0f ) };
   const Vector2 offTop[4]    = { Vector2(  -1.0f,  38.0f ), Vector2(  1.0f,  38.0f ), Vector2(  1.0f,  45.0f ), Vector2( -1.0f,  45.0f ) };
   EXPECT_FALSE( ParticlePlayer::isQuadVisibleInAABB( offLeft, cameraAABB ) );
   EXPECT_FALSE( ParticlePlayer::isQuadVisibleInAABB( offRight, cameraAABB ) );
   EXPECT_FALSE( ParticlePlayer::isQuadVisibleInAABB( offBottom, cameraAABB ) );
   EXPECT_FALSE( ParticlePlayer::isQuadVisibleInAABB( offTop, cameraAABB ) );

   // Zero-alpha blend safety check.
   EXPECT_TRUE( ParticlePlayer::canCullZeroAlphaForBlend( true, false, GL_ONE, GL_ZERO ) );
   EXPECT_TRUE( ParticlePlayer::canCullZeroAlphaForBlend( false, true, GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA ) );
   EXPECT_TRUE( ParticlePlayer::canCullZeroAlphaForBlend( false, true, GL_SRC_ALPHA, GL_ONE ) );
   EXPECT_FALSE( ParticlePlayer::canCullZeroAlphaForBlend( false, false, GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA ) );
   EXPECT_FALSE( ParticlePlayer::canCullZeroAlphaForBlend( false, true, GL_ONE, GL_ONE ) );
   EXPECT_FALSE( ParticlePlayer::canCullZeroAlphaForBlend( false, true, GL_ONE, GL_ONE_MINUS_SRC_ALPHA ) );
   EXPECT_FALSE( ParticlePlayer::canCullZeroAlphaForBlend( false, true, GL_SRC_ALPHA, GL_ZERO ) );
}

TEST( ParticleSimulationOptTests, ParticleNodeCompactLayoutAndTelemetry )
{
   EXPECT_LE( sizeof( ParticleSystem::ParticleNode ), 96U );
   EXPECT_FALSE( std::is_polymorphic<ParticleSystem::ParticleNode>::value );
   EXPECT_TRUE( std::is_trivially_destructible<ParticleSystem::ParticleNode>::value );

   ParticleSystem sys;
   EXPECT_EQ( sys.getAllocatedParticleCount(), 0U );
   EXPECT_EQ( sys.getActiveParticleCount(), 0U );

   sys.adjustAllocatedParticleCount( 64 );
   sys.adjustActiveParticleCount( 12 );
   EXPECT_EQ( sys.getAllocatedParticleCount(), 64U );
   EXPECT_EQ( sys.getActiveParticleCount(), 12U );

   sys.adjustActiveParticleCount( -5 );
   EXPECT_EQ( sys.getActiveParticleCount(), 7U );

   // Underflow-safe clamping when decrement exceeds count.
   sys.adjustActiveParticleCount( -100 );
   sys.adjustAllocatedParticleCount( -100 );
   EXPECT_EQ( sys.getActiveParticleCount(), 0U );
   EXPECT_EQ( sys.getAllocatedParticleCount(), 0U );
}

TEST( ParticleSimulationOptTests, ComputeParticleQuadMatchesOOBBAndAttachedTransform )
{
   const Vector2 pivot( 0.25f, -0.3f );
   const Vector2 localPivotAABB[4] = {
      Vector2( -0.5f + pivot.x, -0.5f + pivot.y ),
      Vector2(  0.5f + pivot.x, -0.5f + pivot.y ),
      Vector2(  0.5f + pivot.x,  0.5f + pivot.y ),
      Vector2( -0.5f + pivot.x,  0.5f + pivot.y )
   };
   const F32 localMinX = localPivotAABB[0].x;
   const F32 localMaxX = localPivotAABB[1].x;
   const F32 localMinY = localPivotAABB[0].y;
   const F32 localMaxY = localPivotAABB[2].y;

   const Vector2 pos( 3.25f, -4.5f );
   const Vector2 renderSize( 2.5f, 1.75f );
   const Vector2 playerPos( 14.0f, -6.25f );
   const F32 playerAngleRad = mDegToRad( 63.0f );
   const F32 playerCos = mCos( playerAngleRad );
   const F32 playerSin = mSin( playerAngleRad );

   const F32 anglesDeg[] = { 0.0f, 37.0f, -125.0f };
   const bool attachPositions[] = { false, true };
   const bool attachRotations[] = { false, true };

   F32 cachedAngleDeg = 0.0f;
   F32 cachedCos = 1.0f;
   F32 cachedSin = 0.0f;

   for ( bool attachPos : attachPositions )
   {
      for ( bool attachRot : attachRotations )
      {
         const bool effectiveAttachRot = attachPos && attachRot;
         for ( F32 angleDeg : anglesDeg )
         {
            Vector2 scaledAABB[4];
            for ( U32 i = 0; i < 4; ++i )
               scaledAABB[i] = localPivotAABB[i] * renderSize;

            b2Transform xf;
            xf.Set( pos, mDegToRad( angleDeg ) );
            Vector2 refLocalOOBB[4];
            CoreMath::mCalculateOOBB( scaledAABB, xf, refLocalOOBB );

            Vector2 refWorldOOBB[4];
            if ( attachPos )
               ParticlePlayer::transformAttachedOOBB( refLocalOOBB, refWorldOOBB, playerPos, playerAngleRad, effectiveAttachRot );
            else
               for ( U32 i = 0; i < 4; ++i )
                  refWorldOOBB[i] = refLocalOOBB[i];

            Vector2 fastUncached[4];
            ParticlePlayer::computeParticleQuad(
               pos, renderSize, angleDeg,
               localMinX, localMaxX, localMinY, localMaxY,
               attachPos, effectiveAttachRot,
               playerPos, playerCos, playerSin,
               fastUncached );

            Vector2 fastCached[4];
            ParticlePlayer::computeParticleQuad(
               pos, renderSize, angleDeg,
               localMinX, localMaxX, localMinY, localMaxY,
               attachPos, effectiveAttachRot,
               playerPos, playerCos, playerSin,
               cachedAngleDeg, cachedCos, cachedSin,
               fastCached );

            for ( U32 i = 0; i < 4; ++i )
            {
               EXPECT_NEAR( fastUncached[i].x, refWorldOOBB[i].x, 1e-4f );
               EXPECT_NEAR( fastUncached[i].y, refWorldOOBB[i].y, 1e-4f );
               EXPECT_FLOAT_EQ( fastCached[i].x, fastUncached[i].x );
               EXPECT_FLOAT_EQ( fastCached[i].y, fastUncached[i].y );
            }
         }
      }
   }
}

TEST( ParticleSimulationOptTests, StepParticleAnimationHandlesCyclingNonCyclingAndEdgeCases )
{
   // Cycling 4-frame animation over 1.0s (0.25s/frame).
   F32 animTime = 0.0f;
   bool animFinished = false;
   EXPECT_EQ( ParticlePlayer::stepParticleAnimation( animTime, animFinished, 0.0f, 1.0f, 4U, true ), 0 );
   EXPECT_FALSE( animFinished );
   EXPECT_EQ( ParticlePlayer::stepParticleAnimation( animTime, animFinished, 0.30f, 1.0f, 4U, true ), 1 );
   EXPECT_EQ( ParticlePlayer::stepParticleAnimation( animTime, animFinished, 0.30f, 1.0f, 4U, true ), 2 );
   EXPECT_EQ( ParticlePlayer::stepParticleAnimation( animTime, animFinished, 0.30f, 1.0f, 4U, true ), 3 );
   // Wrap around: 0.90 + 0.20 = 1.10 -> fmod 0.10 -> frame 0.
   EXPECT_EQ( ParticlePlayer::stepParticleAnimation( animTime, animFinished, 0.20f, 1.0f, 4U, true ), 0 );
   EXPECT_FALSE( animFinished );
   // Multi-cycle step: 0.10 + 2.50 = 2.60 -> fmod 0.60 -> frame 2.
   EXPECT_EQ( ParticlePlayer::stepParticleAnimation( animTime, animFinished, 2.50f, 1.0f, 4U, true ), 2 );
   EXPECT_FALSE( animFinished );

   // Non-cycling 4-frame animation stops and holds final frame 3.
   animTime = 0.0f;
   animFinished = false;
   EXPECT_EQ( ParticlePlayer::stepParticleAnimation( animTime, animFinished, 0.55f, 1.0f, 4U, false ), 2 );
   EXPECT_FALSE( animFinished );
   EXPECT_EQ( ParticlePlayer::stepParticleAnimation( animTime, animFinished, 0.60f, 1.0f, 4U, false ), 3 );
   EXPECT_TRUE( animFinished );
   EXPECT_EQ( ParticlePlayer::stepParticleAnimation( animTime, animFinished, 0.50f, 1.0f, 4U, false ), 3 );
   EXPECT_TRUE( animFinished );

   // Edge cases: 0 frames and 0 total time.
   animTime = 0.0f;
   animFinished = false;
   EXPECT_EQ( ParticlePlayer::stepParticleAnimation( animTime, animFinished, 0.1f, 1.0f, 0U, true ), -1 );
   EXPECT_TRUE( animFinished );

   animTime = 0.0f;
   animFinished = false;
   EXPECT_EQ( ParticlePlayer::stepParticleAnimation( animTime, animFinished, 0.1f, 0.0f, 4U, true ), 0 );
   EXPECT_FALSE( animFinished );
}

#endif // TORQUE_SHIPPING
