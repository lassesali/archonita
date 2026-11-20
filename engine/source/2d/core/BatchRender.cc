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

#include "BatchRender.h"

#ifndef _SHADER_MANAGER_H_
#include "graphics/ShaderManager.h"
#endif

#ifndef _GL_DSA_H_
#include "graphics/glDSA.h"
#endif

#ifndef _SCENE_OBJECT_H_
#include "2d/sceneobject/SceneObject.h"
#endif

// Debug Profiling.
#include "debug/profiler.h"

//-----------------------------------------------------------------------------

BatchRender::BatchRender() :
    mTriangleCount( 0 ),
    mVertexCount( 0 ),
    mIndexCount( 0 ),
    NoColor( -1.0f, -1.0f, -1.0f ),
    mStrictOrderMode( false ),
    mpDebugStats( NULL ),
    mBlendMode( true ),
    mSrcBlendFactor( GL_SRC_ALPHA ),
    mDstBlendFactor( GL_ONE_MINUS_SRC_ALPHA ),
    mBlendColor( ColorF(1.0f,1.0f,1.0f,1.0f) ),
    mAlphaTestMode( -1.0f ),
    mWireframeMode( false ),
    mBatchEnabled( true )
{
}

//-----------------------------------------------------------------------------

BatchRender::~BatchRender()
{
    // Destroy index vectors in texture batch map.
    for ( textureBatchType::iterator itr = mTextureBatchMap.begin(); itr != mTextureBatchMap.end(); ++itr )
    {
        delete itr->value;
    }
    mTextureBatchMap.clear();

    // Destroy index vectors in index vector pool.
    for ( VectorPtr< indexVectorType* >::iterator itr = mIndexVectorPool.begin(); itr != mIndexVectorPool.end(); ++itr )
    {
        delete (*itr);
    }
    mIndexVectorPool.clear();
}

//-----------------------------------------------------------------------------

void BatchRender::setBlendMode( const SceneRenderRequest* pSceneRenderRequest )
{
    // Are we blending?
    if ( pSceneRenderRequest->mBlendMode )
    {
        // Yes, so set blending to standard alpha-blending.
        setBlendMode(
            pSceneRenderRequest->mSrcBlendFactor,
            pSceneRenderRequest->mDstBlendFactor,
            pSceneRenderRequest->mBlendColor );                            
    }
    else
    {
        // No, so turn-off blending.
        setBlendOff();
    }
}

//-----------------------------------------------------------------------------

void BatchRender::setAlphaTestMode( const SceneRenderRequest* pSceneRenderRequest )
{
    // Set alpha-test mode.
    setAlphaTestMode( pSceneRenderRequest->mAlphaTest );
}

//-----------------------------------------------------------------------------

void BatchRender::SubmitTriangles(
        const U32 vertexCount,
        const Vector2* pVertexArray,
        const Vector2* pTextureArray,
		const ColorF*  pColorArray,
		TextureHandle& texture)
{
    // Debug Profiling.
    PROFILE_SCOPE(BatchRender_SubmitTriangles);

    // Sanity!
    AssertFatal( mpDebugStats != NULL, "Debug stats have not been configured." );
    AssertFatal( vertexCount % 3 == 0, "BatchRender::SubmitTriangles() - Invalid vertex count, cannot represent whole triangles." );
    AssertFatal( vertexCount <= BATCHRENDER_BUFFERSIZE, "BatchRender::SubmitTriangles() - Invalid vertex count." );

    // Calculate triangle count.
    const U32 triangleCount = vertexCount / 3;

    if ( (mTriangleCount + triangleCount) > BATCHRENDER_MAXTRIANGLES || (mVertexCount + vertexCount) > BATCHRENDER_BUFFERSIZE )
    {
        // No room in the batch for the incoming request, so flush the current batch contents.
        flush( mpDebugStats->batchBufferFullFlush );
    }

    // Strict order mode?
    if ( mStrictOrderMode )
    {
        // Yes, so is there a texture change?
        if ( texture != mStrictOrderTextureHandle && mTriangleCount > 0 )
        {
            // Yes, so flush.
            flush( mpDebugStats->batchTextureChangeFlush );
        }

        // Fetch vertex index.
        U16 vertexIndex = (U16)mVertexCount;

        // Add new indices.
        for( U32 n = 0; n < triangleCount; ++n )
        {
            mIndexBuffer[mIndexCount++] = vertexIndex++;
            mIndexBuffer[mIndexCount++] = vertexIndex++;
            mIndexBuffer[mIndexCount++] = vertexIndex++;
        }

        // Set strict order mode texture handle.
        mStrictOrderTextureHandle = texture;
    }
    else
    {
        // No, so add triangle run.
        findTextureBatch( texture )->push_back( TriangleRun( TriangleRun::TRIANGLE, triangleCount, mVertexCount ) );
    }

    // Write interleaved vertices directly into the batch buffer.
    const ColorF defaultColor = mBlendMode ? mBlendColor : ColorF( 1.0f, 1.0f, 1.0f, 1.0f );
    DGLVertex* dst = &mVertexBuffer[mVertexCount];
    for( U32 i = 0; i < vertexCount; ++i )
    {
        const Vector2& pos = pVertexArray[i];
        const Vector2 uv = pTextureArray ? pTextureArray[i] : Vector2::getZero();
        const ColorF& col = pColorArray ? pColorArray[i] : defaultColor;
        dst[i].set( pos.x, pos.y, 0.0f, uv.x, uv.y, col.red, col.green, col.blue, col.alpha );
    }
    mVertexCount += vertexCount;

    // Stats.
    mpDebugStats->batchTrianglesSubmitted += triangleCount;

    // Increase triangle count.
    mTriangleCount += triangleCount;

    // Have we reached the buffer limit?
    if ( mTriangleCount == BATCHRENDER_MAXTRIANGLES )
    {
        // Yes, so flush.
        flush( mpDebugStats->batchBufferFullFlush );
    }
    // Is batching enabled?
    else if ( !mBatchEnabled )
    {
        // No, so flush immediately.
        // NOTE: Technically this is still batching but will still revert to using
        // more draw calls therefore can be used in comparison.
        flushInternal();
    }
}

//-----------------------------------------------------------------------------

void BatchRender::SubmitQuad(
        const Vector2& vertexPos0,
        const Vector2& vertexPos1,
        const Vector2& vertexPos2,
        const Vector2& vertexPos3,
        const Vector2& texturePos0,
        const Vector2& texturePos1,
        const Vector2& texturePos2,
        const Vector2& texturePos3,
        TextureHandle& texture,
		const ColorF& color0,
		const ColorF& color1,
		const ColorF& color2,
		const ColorF& color3)
{
    // Sanity!
    AssertFatal( mpDebugStats != NULL, "Debug stats have not been configured." );

    // Debug Profiling.
    PROFILE_SCOPE(BatchRender_SubmitQuad);

    // Would we exceed the triangle or vertex buffer size?
    if ( (mTriangleCount + 2) > BATCHRENDER_MAXTRIANGLES || (mVertexCount + 4) > BATCHRENDER_BUFFERSIZE )
    {
        // Yes, so flush.
        flush( mpDebugStats->batchBufferFullFlush );
    }

    // Strict order mode?
    if ( mStrictOrderMode )
    {
        // Yes, so is there a texture change?
        if ( texture != mStrictOrderTextureHandle && mTriangleCount > 0 )
        {
            // Yes, so flush.
            flush( mpDebugStats->batchTextureChangeFlush );
        }

        // Add new indices.
        const U16 baseIndex = (U16)mVertexCount;
        mIndexBuffer[mIndexCount++] = baseIndex;
        mIndexBuffer[mIndexCount++] = baseIndex + 1;
        mIndexBuffer[mIndexCount++] = baseIndex + 2;
        mIndexBuffer[mIndexCount++] = baseIndex + 3;
        mIndexBuffer[mIndexCount++] = baseIndex + 2;
        mIndexBuffer[mIndexCount++] = baseIndex + 1;

        // Set strict order mode texture handle.
        mStrictOrderTextureHandle = texture;
    }
    else
    {
        // No, so add triangle run.
        findTextureBatch( texture )->push_back( TriangleRun( TriangleRun::QUAD, 1, mVertexCount ) );
    }

    // Resolve per-vertex colors (preserving exact #2/#3 swap and mBlendColor default).
    const ColorF defaultColor = mBlendMode ? mBlendColor : ColorF( 1.0f, 1.0f, 1.0f, 1.0f );
    const ColorF* c0 = &defaultColor;
    const ColorF* c1 = &defaultColor;
    const ColorF* c2 = &defaultColor; // written at slot 2 (swapped #3)
    const ColorF* c3 = &defaultColor; // written at slot 3 (swapped #2)
    if ( color0 != NoColor )
    {
        if ( color1 != NoColor && color2 != NoColor && color3 != NoColor )
        {
            c0 = &color0;
            c1 = &color1;
            c2 = &color3;
            c3 = &color2;
        }
        else
        {
            c0 = c1 = c2 = c3 = &color0;
        }
    }

    // Write interleaved vertices directly into the batch buffer.
    // NOTE: We swap #2/#3 here.
    DGLVertex* dst = &mVertexBuffer[mVertexCount];
    dst[0].set( vertexPos0.x, vertexPos0.y, 0.0f, texturePos0.x, texturePos0.y, c0->red, c0->green, c0->blue, c0->alpha );
    dst[1].set( vertexPos1.x, vertexPos1.y, 0.0f, texturePos1.x, texturePos1.y, c1->red, c1->green, c1->blue, c1->alpha );
    dst[2].set( vertexPos3.x, vertexPos3.y, 0.0f, texturePos3.x, texturePos3.y, c2->red, c2->green, c2->blue, c2->alpha );
    dst[3].set( vertexPos2.x, vertexPos2.y, 0.0f, texturePos2.x, texturePos2.y, c3->red, c3->green, c3->blue, c3->alpha );
    mVertexCount += 4;

    // Stats.
    mpDebugStats->batchTrianglesSubmitted+=2;

    // Increase triangle count.
    mTriangleCount += 2;

    // Have we reached the buffer limit?
    if ( mTriangleCount == BATCHRENDER_MAXTRIANGLES )
    {
        // Yes, so flush.
        flush( mpDebugStats->batchBufferFullFlush );
    }
    // Is batching enabled?
    else if ( !mBatchEnabled )
    {
        // No, so flush immediately.
        // NOTE: Technically this is still batching but will still revert to using
        // more draw calls therefore can be used in comparison.
        flushInternal();
    }
}

//-----------------------------------------------------------------------------

void BatchRender::flush( U32& reasonMetric )
{
    // Finish if no triangles to flush.
    if ( mTriangleCount == 0 )
        return;

    // Increase reason metric.
    reasonMetric++;

    // Flush.
    flushInternal();
}

//-----------------------------------------------------------------------------

void BatchRender::flush( void )
{
    // Finish if no triangles to flush.
    if ( mTriangleCount == 0 )
        return;

    // Increase reason metric.
    mpDebugStats->batchAnonymousFlush++;

    // Flush.
    flushInternal();
}

//-----------------------------------------------------------------------------

void BatchRender::flushInternal( void )
{
    // Debug Profiling.
    PROFILE_SCOPE(T2D_BatchRender_flush);

    // Finish if no triangles to flush.
    if ( mTriangleCount == 0 )
        return;

    // Stats.
    mpDebugStats->batchFlushes++;

    const bool textured = !mWireframeMode;
    ShaderManager::setTextureMode( textured ? DGL_TEX_MODULATE : DGL_TEX_NONE );

    // Set blend mode.
    if ( mBlendMode )
    {
        glEnable( GL_BLEND );
        glBlendFunc( mSrcBlendFactor, mDstBlendFactor );
    }
    else
    {
        glDisable( GL_BLEND );
    }

    // Set alpha-test threshold in shader.
    ShaderManager::setAlphaThreshold( mAlphaTestMode >= 0.0f ? mAlphaTestMode : -1.0f );

    // Strict order mode?
    if ( mStrictOrderMode )
    {
        // Bind the texture if not in wireframe mode.
        if ( textured )
            GLDSA::bindTextureUnit( 0, mStrictOrderTextureHandle.getGLName() );

        // Draw the triangles.
        ShaderManager::drawIndexedVertices( GL_TRIANGLES, mVertexBuffer, mVertexCount, mIndexBuffer, mIndexCount, mWireframeMode );

        // Stats.
        mpDebugStats->batchDrawCallsStrict++;

        // Stats.
        const U32 trianglesDrawn = mIndexCount / 3;
        if ( trianglesDrawn > mpDebugStats->batchMaxTriangleDrawn )
            mpDebugStats->batchMaxTriangleDrawn = trianglesDrawn;

        // Stats.
        if ( mVertexCount > mpDebugStats->batchMaxVertexBuffer )
            mpDebugStats->batchMaxVertexBuffer = mVertexCount;
    }
    else
    {
        // Upload the shared interleaved vertex buffer once for all texture buckets.
        ShaderManager::beginIndexedDraw( mVertexBuffer, mVertexCount );

        // Stats.
        if ( mVertexCount > mpDebugStats->batchMaxVertexBuffer )
            mpDebugStats->batchMaxVertexBuffer = mVertexCount;

        // Iterate texture batch map.
        for( textureBatchType::iterator batchItr = mTextureBatchMap.begin(); batchItr != mTextureBatchMap.end(); ++batchItr )
        {
            // Reset index count.
            mIndexCount = 0;

            // Fetch index vector.
            indexVectorType* pIndexVector = batchItr->value;

            // Iterate indexes.
            for( indexVectorType::iterator indexItr = pIndexVector->begin(); indexItr != pIndexVector->end(); ++indexItr )
            {
                // Fetch triangle run.
                const TriangleRun& triangleRun = *indexItr;

                // Fetch primitivecount.
                const U32 primitiveCount = triangleRun.mPrimitiveCount;

                // Fetch triangle index start.
                U16 triangleIndex = (U16)triangleRun.mStartIndex;

                // Fetch primitive mode.
                const TriangleRun::PrimitiveMode& primitiveMode = triangleRun.mPrimitiveMode;

                // Handle primitive mode.
                if ( primitiveMode == TriangleRun::QUAD )
                {
                    // Add triangle run for quad.
                    for( U32 n = 0; n < primitiveCount; ++n )
                    {
                        mIndexBuffer[mIndexCount++] = triangleIndex;
                        mIndexBuffer[mIndexCount++] = triangleIndex + 1;
                        mIndexBuffer[mIndexCount++] = triangleIndex + 2;
                        mIndexBuffer[mIndexCount++] = triangleIndex + 3;
                        mIndexBuffer[mIndexCount++] = triangleIndex + 2;
                        mIndexBuffer[mIndexCount++] = triangleIndex + 1;
                        triangleIndex += 4;
                    }
                }
                else if ( primitiveMode == TriangleRun::TRIANGLE )
                {
                    // Add triangle run for triangles.
                    for( U32 n = 0; n < primitiveCount; ++n )
                    {
                        // Add new indices.
                        mIndexBuffer[mIndexCount++] = triangleIndex++;
                        mIndexBuffer[mIndexCount++] = triangleIndex++;
                        mIndexBuffer[mIndexCount++] = triangleIndex++;
                    }
                }
                else
                {
                    // Sanity!
                    AssertFatal( false, "BatchRender::flushInternal() - Unrecognized primitive mode encountered for triangle run." );
                }
            }

            // Sanity!
            AssertFatal( mIndexCount > 0, "No batching indexes are present." );

            // Bind the texture if not in wireframe mode.
            if ( textured )
                GLDSA::bindTextureUnit( 0, batchItr->key );

            // Draw the triangles using the already-uploaded vertex buffer.
            ShaderManager::drawUploadedIndices( GL_TRIANGLES, mIndexBuffer, mIndexCount, mWireframeMode );

            // Stats.
            mpDebugStats->batchDrawCallsSorted++;

            // Stats.
            const U32 trianglesDrawn = mIndexCount / 3;
            if ( trianglesDrawn > mpDebugStats->batchMaxTriangleDrawn )
                mpDebugStats->batchMaxTriangleDrawn = trianglesDrawn;

            // Return index vector to pool.
            pIndexVector->clear();
            mIndexVectorPool.push_back( pIndexVector );
        }

        // Clear texture batch map.
        mTextureBatchMap.clear();
    }

    // Reset common render state.
    ShaderManager::setAlphaThreshold( -1.0f );
    ShaderManager::setTextureMode( DGL_TEX_NONE );
    glDisable( GL_BLEND );

    // Reset batch state.
    mTriangleCount = 0;
    mVertexCount = 0;
    mIndexCount = 0;
}

//-----------------------------------------------------------------------------

BatchRender::indexVectorType* BatchRender::findTextureBatch( TextureHandle& handle )
{
    // Fetch texture binding.
    const U32 textureBinding = handle.getGLName();

    indexVectorType* pIndexVector = NULL;

    // Find texture binding.
    textureBatchType::iterator itr = mTextureBatchMap.find( textureBinding );

    // Did we find a texture binding?
    if ( itr == mTextureBatchMap.end() )
    {
        // No, so fetch index vector pool count.
        const U32 indexVectorPoolCount = mIndexVectorPool.size();

        // Do we have any in the index vector pool?
        if ( indexVectorPoolCount > 0 )
        {
            // Yes, so use it.
            pIndexVector = mIndexVectorPool[indexVectorPoolCount-1];
            mIndexVectorPool.pop_back();
        }
        else
        {
            // No, so generate one.
            pIndexVector = new indexVectorType( 6 * 6 );
        }

        // Insert into texture batch map.
        mTextureBatchMap.insert( textureBinding, pIndexVector );
    }
    else
    {
        // Yes, so fetch it.
        pIndexVector = itr->value;
    }

    return pIndexVector;
}


