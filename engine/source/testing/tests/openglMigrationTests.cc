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

#include <gtest/gtest.h>
#include <cstring>

#include "graphics/ShaderManager.h"
#include "graphics/dgl.h"
#include "platform/GLModernTypes.h"

namespace
{
   void transformColMajorPoint(const F32 m[16], F32 x, F32 y, F32 z, F32 out[4])
   {
      out[0] = m[0] * x + m[4] * y + m[8]  * z + m[12];
      out[1] = m[1] * x + m[5] * y + m[9]  * z + m[13];
      out[2] = m[2] * x + m[6] * y + m[10] * z + m[14];
      out[3] = m[3] * x + m[7] * y + m[11] * z + m[15];
   }
}

TEST(OpenGLMigrationShaderTest, BuildPreprocessedSourceDesktopGL46Core)
{
   const char* body = "void main() { gl_Position = vec4(0.0); }";
   char buffer[1024];
   dMemset(buffer, 0, sizeof(buffer));

   ASSERT_TRUE(ShaderManager::buildPreprocessedSource(body, false, 4, 6, buffer, sizeof(buffer)));
   EXPECT_EQ(dStrstr(buffer, "#version 460 core\n"), buffer);
   EXPECT_EQ(dStrstr(buffer, "precision highp float;"), (const char*)NULL);
   EXPECT_NE(dStrstr(buffer, body), (const char*)NULL);
}

TEST(OpenGLMigrationShaderTest, BuildPreprocessedSourceMobileGLES32)
{
   const char* body = "void main() { gl_Position = vec4(0.0); }";
   char buffer[1024];
   dMemset(buffer, 0, sizeof(buffer));

   ASSERT_TRUE(ShaderManager::buildPreprocessedSource(body, true, 3, 2, buffer, sizeof(buffer)));
   EXPECT_EQ(dStrstr(buffer, "#version 320 es\n"), buffer);
   EXPECT_NE(dStrstr(buffer, "precision highp float;\n"), (const char*)NULL);
   EXPECT_NE(dStrstr(buffer, "precision highp int;\n"), (const char*)NULL);
   EXPECT_NE(dStrstr(buffer, "precision highp sampler2D;\n"), (const char*)NULL);
   EXPECT_NE(dStrstr(buffer, body), (const char*)NULL);
}

TEST(OpenGLMigrationShaderTest, BuildPreprocessedSourceRejectsInvalidArgs)
{
   char buffer[64];
   EXPECT_FALSE(ShaderManager::buildPreprocessedSource(NULL, false, 4, 6, buffer, sizeof(buffer)));
   EXPECT_FALSE(ShaderManager::buildPreprocessedSource("void main(){}", false, 4, 6, NULL, sizeof(buffer)));
   EXPECT_FALSE(ShaderManager::buildPreprocessedSource("void main(){}", false, 4, 6, buffer, 0));
}

TEST(OpenGLMigrationStateTest, CpuRenderStateAccessors)
{
   ShaderManager::setTextureMode(DGL_TEX_ADD);
   EXPECT_EQ(ShaderManager::getTextureMode(), DGL_TEX_ADD);

   ShaderManager::setTextureMode(DGL_TEX_MODULATE);
   EXPECT_EQ(ShaderManager::getTextureMode(), DGL_TEX_MODULATE);

   ShaderManager::setAlphaThreshold(0.5f);
   EXPECT_FLOAT_EQ(ShaderManager::getAlphaThreshold(), 0.5f);

   ShaderManager::setAlphaThreshold(-1.0f);
   EXPECT_FLOAT_EQ(ShaderManager::getAlphaThreshold(), -1.0f);
}

TEST(OpenGLMigrationMatrixTest, StackDepthPushPopAndModeSwitching)
{
   dglResetMatrixStacks();

   EXPECT_EQ(dglGetMatrixMode(), (GLenum)GL_MODELVIEW);
   EXPECT_EQ(dglGetModelviewStackDepth(), 1);
   EXPECT_EQ(dglGetProjectionStackDepth(), 1);
   EXPECT_EQ(dglGetTextureStackDepth(), 1);

   dglPushMatrix();
   dglTranslatef(15.0f, -25.0f, 3.0f);
   EXPECT_EQ(dglGetModelviewStackDepth(), 2);

   F32 mv[16];
   dglGetModelviewColumnMajor(mv);
   EXPECT_FLOAT_EQ(mv[12], 15.0f);
   EXPECT_FLOAT_EQ(mv[13], -25.0f);
   EXPECT_FLOAT_EQ(mv[14], 3.0f);

   dglPopMatrix();
   EXPECT_EQ(dglGetModelviewStackDepth(), 1);
   dglGetModelviewColumnMajor(mv);
   EXPECT_FLOAT_EQ(mv[12], 0.0f);
   EXPECT_FLOAT_EQ(mv[13], 0.0f);
   EXPECT_FLOAT_EQ(mv[14], 0.0f);
   EXPECT_FLOAT_EQ(mv[0], 1.0f);
   EXPECT_FLOAT_EQ(mv[5], 1.0f);
   EXPECT_FLOAT_EQ(mv[10], 1.0f);
   EXPECT_FLOAT_EQ(mv[15], 1.0f);

   dglMatrixMode(GL_PROJECTION);
   EXPECT_EQ(dglGetMatrixMode(), (GLenum)GL_PROJECTION);
   dglPushMatrix();
   EXPECT_EQ(dglGetProjectionStackDepth(), 2);
   dglPopMatrix();
   EXPECT_EQ(dglGetProjectionStackDepth(), 1);

   dglResetMatrixStacks();
}

TEST(OpenGLMigrationMatrixTest, OrthoAndCombinedMVPTransform)
{
   dglResetMatrixStacks();

   dglMatrixMode(GL_PROJECTION);
   dglLoadIdentity();
   dglOrtho(0.0, 800.0, 600.0, 0.0, -1.0, 1.0);

   dglMatrixMode(GL_MODELVIEW);
   dglLoadIdentity();
   dglTranslatef(200.0f, 150.0f, 0.0f);
   dglScalef(2.0f, 2.0f, 1.0f);

   F32 mvp[16];
   dglGetMVPColumnMajor(mvp);

   // Local (100, 75, 0) -> world (200 + 200, 150 + 150) = (400, 300) -> NDC (0, 0)
   F32 centerNdc[4];
   transformColMajorPoint(mvp, 100.0f, 75.0f, 0.0f, centerNdc);
   EXPECT_NEAR(centerNdc[0], 0.0f, 1e-5f);
   EXPECT_NEAR(centerNdc[1], 0.0f, 1e-5f);
   EXPECT_NEAR(centerNdc[2], 0.0f, 1e-5f);
   EXPECT_NEAR(centerNdc[3], 1.0f, 1e-5f);

   // Local (-100, -75, 0) -> world (0, 0) -> NDC (-1, +1)
   F32 topLeftNdc[4];
   transformColMajorPoint(mvp, -100.0f, -75.0f, 0.0f, topLeftNdc);
   EXPECT_NEAR(topLeftNdc[0], -1.0f, 1e-5f);
   EXPECT_NEAR(topLeftNdc[1], 1.0f, 1e-5f);

   // Rotation check around Z: (1, 0, 0) rotated +90 deg -> (0, 1, 0)
   dglLoadIdentity();
   dglRotatef(90.0f, 0.0f, 0.0f, 1.0f);
   F32 mvRot[16];
   dglGetModelviewColumnMajor(mvRot);
   F32 rotOut[4];
   transformColMajorPoint(mvRot, 1.0f, 0.0f, 0.0f, rotOut);
   EXPECT_NEAR(rotOut[0], 0.0f, 1e-5f);
   EXPECT_NEAR(rotOut[1], 1.0f, 1e-5f);
   EXPECT_NEAR(rotOut[2], 0.0f, 1e-5f);

   dglResetMatrixStacks();
}

TEST(OpenGLMigrationConstantsTest, CoreSizedFormatsAndSwizzleEnumsDefined)
{
   EXPECT_EQ((U32)GL_R8, (U32)0x8229);
   EXPECT_EQ((U32)GL_RG8, (U32)0x822B);
   EXPECT_EQ((U32)GL_RGB8, (U32)0x8051);
   EXPECT_EQ((U32)GL_RGBA8, (U32)0x8058);
   EXPECT_EQ((U32)GL_RGB565, (U32)0x8D62);
   EXPECT_EQ((U32)GL_CLAMP_TO_EDGE, (U32)0x812F);
   EXPECT_EQ((U32)GL_TEXTURE_SWIZZLE_RGBA, (U32)0x8E46);
}
