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

#include "graphics/ShaderManager.h"
#include "graphics/glDSA.h"
#include "graphics/dgl.h"
#include "console/console.h"
#include "collection/vector.h"

namespace ShaderManager
{

namespace
{
   static bool sInitialized = false;
   static GLuint sDefaultProgram = 0;
   static GLuint sDynamicVAO = 0;
   static GLuint sDynamicVBO = 0;
   static GLuint sDynamicIBO = 0;
   static U32 sVBOCapacityBytes = 0;
   static U32 sIBOCapacityBytes = 0;

   // Uniform locations
   static GLint sLocMVP = -1;
   static GLint sLocModelView = -1;
   static GLint sLocTexture0 = -1;
   static GLint sLocTexMode = -1;
   static GLint sLocTexEnvColor = -1;
   static GLint sLocAlphaThreshold = -1;
   static GLint sLocClipPlaneCount = -1;
   static GLint sLocClipPlanes = -1;

   // Current state
   static DGLTexMode sCurrentTexMode = DGL_TEX_NONE;
   static ColorF sCurrentTexEnvColor(1.0f, 1.0f, 1.0f, 1.0f);
   static F32 sCurrentAlphaThreshold = -1.0f;

   // Clip planes
   static F32 sClipPlanes[4][4] = {
      { 0.0f, 0.0f, 0.0f, 0.0f },
      { 0.0f, 0.0f, 0.0f, 0.0f },
      { 0.0f, 0.0f, 0.0f, 0.0f },
      { 0.0f, 0.0f, 0.0f, 0.0f }
   };
   static bool sClipPlaneEnabled[4] = { false, false, false, false };

   // Immediate mode builder state
   static bool sInBeginEnd = false;
   static GLenum sImmediateMode = GL_TRIANGLES;
   static F32 sImmU = 0.0f;
   static F32 sImmV = 0.0f;
   static F32 sImmR = 1.0f;
   static F32 sImmG = 1.0f;
   static F32 sImmB = 1.0f;
   static F32 sImmA = 1.0f;
   static Vector<DGLVertex> sImmVertices(__FILE__, __LINE__);
   static Vector<DGLVertex> sScratchVertices(__FILE__, __LINE__);
   static Vector<U16> sScratchIndices(__FILE__, __LINE__);

   static const char* sDefaultVertexShaderBody =
      "layout(location = 0) in vec3 aPosition;\n"
      "layout(location = 1) in vec2 aTexCoord;\n"
      "layout(location = 2) in vec4 aColor;\n"
      "\n"
      "uniform mat4 uMVP;\n"
      "uniform mat4 uModelView;\n"
      "\n"
      "out vec2 vTexCoord;\n"
      "out vec4 vColor;\n"
      "out vec4 vEyePos;\n"
      "\n"
      "void main()\n"
      "{\n"
      "   vec4 pos = vec4(aPosition, 1.0);\n"
      "   gl_Position = uMVP * pos;\n"
      "   vEyePos = uModelView * pos;\n"
      "   vTexCoord = aTexCoord;\n"
      "   vColor = aColor;\n"
      "}\n";

   static const char* sDefaultFragmentShaderBody =
      "in vec2 vTexCoord;\n"
      "in vec4 vColor;\n"
      "in vec4 vEyePos;\n"
      "\n"
      "uniform sampler2D uTexture0;\n"
      "uniform int uTexMode;\n"
      "uniform vec4 uTexEnvColor;\n"
      "uniform float uAlphaThreshold;\n"
      "uniform int uClipPlaneCount;\n"
      "uniform vec4 uClipPlanes[4];\n"
      "\n"
      "layout(location = 0) out vec4 fragColor;\n"
      "\n"
      "void main()\n"
      "{\n"
      "   if (uClipPlaneCount > 0)\n"
      "   {\n"
      "      for (int i = 0; i < 4; ++i)\n"
      "      {\n"
      "         if (i < uClipPlaneCount && dot(vEyePos, uClipPlanes[i]) < 0.0)\n"
      "            discard;\n"
      "      }\n"
      "   }\n"
      "\n"
      "   vec4 outColor = vColor;\n"
      "   if (uTexMode == 1)\n"
      "   {\n"
      "      outColor = texture(uTexture0, vTexCoord) * vColor;\n"
      "   }\n"
      "   else if (uTexMode == 2)\n"
      "   {\n"
      "      vec4 tex = texture(uTexture0, vTexCoord);\n"
      "      outColor = vec4(mix(vColor.rgb, uTexEnvColor.rgb, tex.rgb), vColor.a * tex.a);\n"
      "   }\n"
      "   else if (uTexMode == 3)\n"
      "   {\n"
      "      outColor = texture(uTexture0, vTexCoord);\n"
      "   }\n"
      "   else if (uTexMode == 4)\n"
      "   {\n"
      "      vec4 tex = texture(uTexture0, vTexCoord);\n"
      "      outColor = vec4(vColor.rgb + tex.rgb, vColor.a * tex.a);\n"
      "   }\n"
      "\n"
      "   if (uAlphaThreshold >= 0.0 && outColor.a <= uAlphaThreshold)\n"
      "      discard;\n"
      "\n"
      "   fragColor = outColor;\n"
      "}\n";

   static void bindProgramAndSyncUniforms()
   {
      if (!sInitialized)
      {
         if (!init())
            return;
      }

      glUseProgram(sDefaultProgram);

      F32 mvp[16];
      F32 mv[16];
      dglGetMVPColumnMajor(mvp);
      dglGetModelviewColumnMajor(mv);

      if (sLocMVP >= 0)
         glUniformMatrix4fv(sLocMVP, 1, GL_FALSE, mvp);
      if (sLocModelView >= 0)
         glUniformMatrix4fv(sLocModelView, 1, GL_FALSE, mv);
      if (sLocTexture0 >= 0)
         glUniform1i(sLocTexture0, 0);
      if (sLocTexMode >= 0)
         glUniform1i(sLocTexMode, (GLint)sCurrentTexMode);
      if (sLocTexEnvColor >= 0)
         glUniform4f(sLocTexEnvColor, sCurrentTexEnvColor.red, sCurrentTexEnvColor.green, sCurrentTexEnvColor.blue, sCurrentTexEnvColor.alpha);
      if (sLocAlphaThreshold >= 0)
         glUniform1f(sLocAlphaThreshold, sCurrentAlphaThreshold);

      if (sLocClipPlaneCount >= 0)
      {
         F32 activePlanes[4][4];
         GLint count = 0;
         for (U32 i = 0; i < 4; ++i)
         {
            if (sClipPlaneEnabled[i])
            {
               activePlanes[count][0] = sClipPlanes[i][0];
               activePlanes[count][1] = sClipPlanes[i][1];
               activePlanes[count][2] = sClipPlanes[i][2];
               activePlanes[count][3] = sClipPlanes[i][3];
               ++count;
            }
         }
         glUniform1i(sLocClipPlaneCount, count);
         if (count > 0 && sLocClipPlanes >= 0)
            glUniform4fv(sLocClipPlanes, count, &activePlanes[0][0]);
      }
   }

   static void configureVAOAttributes()
   {
      glBindVertexArray(sDynamicVAO);
      glBindBuffer(GL_ARRAY_BUFFER, sDynamicVBO);
      glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, sDynamicIBO);

      const GLsizei stride = (GLsizei)sizeof(DGLVertex);
      glEnableVertexAttribArray(0);
      glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, stride, (const void*)0);

      glEnableVertexAttribArray(1);
      glVertexAttribPointer(1, 2, GL_FLOAT, GL_FALSE, stride, (const void*)(3 * sizeof(F32)));

      glEnableVertexAttribArray(2);
      glVertexAttribPointer(2, 4, GL_FLOAT, GL_FALSE, stride, (const void*)(5 * sizeof(F32)));
   }

   static void ensureVBOCapacity(U32 requiredBytes)
   {
      if (requiredBytes > sVBOCapacityBytes)
      {
         sVBOCapacityBytes = getMax(requiredBytes, sVBOCapacityBytes * 2U);
         GLDSA::namedBufferData(sDynamicVBO, GL_ARRAY_BUFFER, (GLsizeiptr)sVBOCapacityBytes, NULL, GL_STREAM_DRAW);
      }
   }

   static void ensureIBOCapacity(U32 requiredBytes)
   {
      if (requiredBytes > sIBOCapacityBytes)
      {
         sIBOCapacityBytes = getMax(requiredBytes, sIBOCapacityBytes * 2U);
         GLDSA::namedBufferData(sDynamicIBO, GL_ELEMENT_ARRAY_BUFFER, (GLsizeiptr)sIBOCapacityBytes, NULL, GL_STREAM_DRAW);
      }
   }
} // anonymous namespace

bool buildPreprocessedSource(const char* shaderBody,
                             bool isGLES,
                             U32 majorVer,
                             U32 minorVer,
                             char* outBuffer,
                             U32 outBufferSize)
{
   if (!shaderBody || !outBuffer || outBufferSize == 0)
      return false;

   if (isGLES)
   {
      U32 maj = (majorVer >= 3) ? majorVer : 3;
      U32 min = (majorVer >= 3) ? minorVer : 2;
      dSprintf(outBuffer, outBufferSize,
               "#version %u%u0 es\n"
               "precision highp float;\n"
               "precision highp int;\n"
               "precision highp sampler2D;\n"
               "\n%s",
               maj, min, shaderBody);
   }
   else
   {
      U32 maj = (majorVer >= 3) ? majorVer : 4;
      U32 min = (majorVer >= 3) ? minorVer : 6;
      dSprintf(outBuffer, outBufferSize,
               "#version %u%u0 core\n"
               "\n%s",
               maj, min, shaderBody);
   }
   return true;
}

GLuint compileShader(GLenum shaderType, const char* shaderBody)
{
#if defined(TORQUE_OS_ANDROID) || defined(TORQUE_OS_IOS) || defined(TORQUE_OS_EMSCRIPTEN)
   const bool isGLES = true;
#else
   const bool isGLES = false;
#endif

   const U32 majorVer = (gGLState.glMajorVersion >= 3) ? (U32)gGLState.glMajorVersion : (isGLES ? 3U : 4U);
   const U32 minorVer = (gGLState.glMajorVersion >= 3) ? (U32)gGLState.glMinorVersion : (isGLES ? 2U : 6U);

   char sourceBuf[8192];
   if (!buildPreprocessedSource(shaderBody, isGLES, majorVer, minorVer, sourceBuf, sizeof(sourceBuf)))
      return 0;

   GLuint shader = glCreateShader(shaderType);
   if (shader == 0)
      return 0;

   const GLchar* srcPtr = sourceBuf;
   glShaderSource(shader, 1, &srcPtr, NULL);
   glCompileShader(shader);

   GLint compiled = GL_FALSE;
   glGetShaderiv(shader, GL_COMPILE_STATUS, &compiled);
   if (compiled != GL_TRUE)
   {
      char logBuf[2048];
      GLsizei logLen = 0;
      glGetShaderInfoLog(shader, sizeof(logBuf) - 1, &logLen, logBuf);
      logBuf[logLen] = '\0';
      Con::errorf("ShaderManager::compileShader failed: %s", logBuf);
      glDeleteShader(shader);
      return 0;
   }

   return shader;
}

GLuint createProgram(const char* vertexBody, const char* fragmentBody)
{
   GLuint vs = compileShader(GL_VERTEX_SHADER, vertexBody);
   if (vs == 0)
      return 0;

   GLuint fs = compileShader(GL_FRAGMENT_SHADER, fragmentBody);
   if (fs == 0)
   {
      glDeleteShader(vs);
      return 0;
   }

   GLuint prog = glCreateProgram();
   glAttachShader(prog, vs);
   glAttachShader(prog, fs);
   glLinkProgram(prog);

   glDeleteShader(vs);
   glDeleteShader(fs);

   GLint linked = GL_FALSE;
   glGetProgramiv(prog, GL_LINK_STATUS, &linked);
   if (linked != GL_TRUE)
   {
      char logBuf[2048];
      GLsizei logLen = 0;
      glGetProgramInfoLog(prog, sizeof(logBuf) - 1, &logLen, logBuf);
      logBuf[logLen] = '\0';
      Con::errorf("ShaderManager::createProgram link failed: %s", logBuf);
      glDeleteProgram(prog);
      return 0;
   }

   return prog;
}

bool init()
{
   if (sInitialized)
      return true;

   sDefaultProgram = createProgram(sDefaultVertexShaderBody, sDefaultFragmentShaderBody);
   if (sDefaultProgram == 0)
      return false;

   sLocMVP            = glGetUniformLocation(sDefaultProgram, "uMVP");
   sLocModelView      = glGetUniformLocation(sDefaultProgram, "uModelView");
   sLocTexture0       = glGetUniformLocation(sDefaultProgram, "uTexture0");
   sLocTexMode        = glGetUniformLocation(sDefaultProgram, "uTexMode");
   sLocTexEnvColor    = glGetUniformLocation(sDefaultProgram, "uTexEnvColor");
   sLocAlphaThreshold = glGetUniformLocation(sDefaultProgram, "uAlphaThreshold");
   sLocClipPlaneCount = glGetUniformLocation(sDefaultProgram, "uClipPlaneCount");
   sLocClipPlanes     = glGetUniformLocation(sDefaultProgram, "uClipPlanes");

   sVBOCapacityBytes = 65536 * sizeof(DGLVertex);
   sIBOCapacityBytes = 65536 * sizeof(U16);

   sDynamicVAO = GLDSA::createVertexArray();
   sDynamicVBO = GLDSA::createBuffer();
   sDynamicIBO = GLDSA::createBuffer();

   GLDSA::namedBufferData(sDynamicVBO, GL_ARRAY_BUFFER, (GLsizeiptr)sVBOCapacityBytes, NULL, GL_STREAM_DRAW);
   GLDSA::namedBufferData(sDynamicIBO, GL_ELEMENT_ARRAY_BUFFER, (GLsizeiptr)sIBOCapacityBytes, NULL, GL_STREAM_DRAW);

   configureVAOAttributes();

   sInitialized = true;
   return true;
}

void shutdown()
{
   if (!sInitialized)
      return;

   glUseProgram(0);
   glBindVertexArray(0);

   GLDSA::deleteBuffer(sDynamicIBO);
   GLDSA::deleteBuffer(sDynamicVBO);
   GLDSA::deleteVertexArray(sDynamicVAO);

   sDynamicIBO = 0;
   sDynamicVBO = 0;
   sDynamicVAO = 0;
   sVBOCapacityBytes = 0;
   sIBOCapacityBytes = 0;

   if (sDefaultProgram != 0)
   {
      glDeleteProgram(sDefaultProgram);
      sDefaultProgram = 0;
   }

   sInitialized = false;
}

bool isInitialized()
{
   return sInitialized;
}

void setTextureMode(DGLTexMode mode)
{
   sCurrentTexMode = mode;
}

DGLTexMode getTextureMode()
{
   return sCurrentTexMode;
}

void setTextureEnvColor(const ColorF& col)
{
   sCurrentTexEnvColor = col;
}

void setAlphaThreshold(F32 threshold)
{
   sCurrentAlphaThreshold = threshold;
}

F32 getAlphaThreshold()
{
   return sCurrentAlphaThreshold;
}

void setClipPlane(U32 index, F64 a, F64 b, F64 c, F64 d)
{
   if (index >= 4)
      return;
   sClipPlanes[index][0] = (F32)a;
   sClipPlanes[index][1] = (F32)b;
   sClipPlanes[index][2] = (F32)c;
   sClipPlanes[index][3] = (F32)d;
}

void enableClipPlane(U32 index, bool enable)
{
   if (index >= 4)
      return;
   sClipPlaneEnabled[index] = enable;
}

void clearClipPlanes()
{
   for (U32 i = 0; i < 4; ++i)
      sClipPlaneEnabled[i] = false;
}

void drawVertices(GLenum mode, const DGLVertex* vertices, U32 vertexCount)
{
   if (!vertices || vertexCount == 0)
      return;

   // Automatically convert legacy GL_QUADS / GL_QUAD_STRIP / GL_POLYGON for Core Profile & GLES 3.2
   if (mode == GL_QUADS)
   {
      const U32 quadCount = vertexCount / 4;
      if (quadCount == 0)
         return;
      sScratchVertices.clear();
      sScratchVertices.reserve(quadCount * 6);
      for (U32 q = 0; q < quadCount; ++q)
      {
         const DGLVertex& v0 = vertices[q * 4 + 0];
         const DGLVertex& v1 = vertices[q * 4 + 1];
         const DGLVertex& v2 = vertices[q * 4 + 2];
         const DGLVertex& v3 = vertices[q * 4 + 3];
         sScratchVertices.push_back(v0);
         sScratchVertices.push_back(v1);
         sScratchVertices.push_back(v2);
         sScratchVertices.push_back(v0);
         sScratchVertices.push_back(v2);
         sScratchVertices.push_back(v3);
      }
      drawVertices(GL_TRIANGLES, sScratchVertices.address(), sScratchVertices.size());
      return;
   }
   else if (mode == GL_QUAD_STRIP)
   {
      mode = GL_TRIANGLE_STRIP;
   }
   else if (mode == GL_POLYGON)
   {
      mode = GL_TRIANGLE_FAN;
   }

   bindProgramAndSyncUniforms();

   const U32 byteSize = vertexCount * sizeof(DGLVertex);
   ensureVBOCapacity(byteSize);
   // Buffer orphaning + sub-data upload for zero-stall streaming
   GLDSA::namedBufferData(sDynamicVBO, GL_ARRAY_BUFFER, (GLsizeiptr)sVBOCapacityBytes, NULL, GL_STREAM_DRAW);
   GLDSA::namedBufferSubData(sDynamicVBO, GL_ARRAY_BUFFER, 0, (GLsizeiptr)byteSize, vertices);

   glBindVertexArray(sDynamicVAO);
   glDrawArrays(mode, 0, (GLsizei)vertexCount);
}

void drawIndexedVertices(GLenum mode,
                         const DGLVertex* vertices,
                         U32 vertexCount,
                         const U16* indices,
                         U32 indexCount,
                         bool wireframe)
{
   if (!vertices || vertexCount == 0 || !indices || indexCount == 0)
      return;

   bindProgramAndSyncUniforms();

   const U32 vboBytes = vertexCount * sizeof(DGLVertex);
   ensureVBOCapacity(vboBytes);
   GLDSA::namedBufferData(sDynamicVBO, GL_ARRAY_BUFFER, (GLsizeiptr)sVBOCapacityBytes, NULL, GL_STREAM_DRAW);
   GLDSA::namedBufferSubData(sDynamicVBO, GL_ARRAY_BUFFER, 0, (GLsizeiptr)vboBytes, vertices);

   glBindVertexArray(sDynamicVAO);
   glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, sDynamicIBO);

   if (wireframe && mode == GL_TRIANGLES)
   {
      const U32 triCount = indexCount / 3;
      sScratchIndices.clear();
      sScratchIndices.reserve(triCount * 6);
      for (U32 t = 0; t < triCount; ++t)
      {
         const U16 i0 = indices[t * 3 + 0];
         const U16 i1 = indices[t * 3 + 1];
         const U16 i2 = indices[t * 3 + 2];
         sScratchIndices.push_back(i0);
         sScratchIndices.push_back(i1);
         sScratchIndices.push_back(i1);
         sScratchIndices.push_back(i2);
         sScratchIndices.push_back(i2);
         sScratchIndices.push_back(i0);
      }
      const U32 iboBytes = sScratchIndices.size() * sizeof(U16);
      ensureIBOCapacity(iboBytes);
      GLDSA::namedBufferData(sDynamicIBO, GL_ELEMENT_ARRAY_BUFFER, (GLsizeiptr)sIBOCapacityBytes, NULL, GL_STREAM_DRAW);
      GLDSA::namedBufferSubData(sDynamicIBO, GL_ELEMENT_ARRAY_BUFFER, 0, (GLsizeiptr)iboBytes, sScratchIndices.address());
      glDrawElements(GL_LINES, (GLsizei)sScratchIndices.size(), GL_UNSIGNED_SHORT, (const void*)0);
   }
   else
   {
      const U32 iboBytes = indexCount * sizeof(U16);
      ensureIBOCapacity(iboBytes);
      GLDSA::namedBufferData(sDynamicIBO, GL_ELEMENT_ARRAY_BUFFER, (GLsizeiptr)sIBOCapacityBytes, NULL, GL_STREAM_DRAW);
      GLDSA::namedBufferSubData(sDynamicIBO, GL_ELEMENT_ARRAY_BUFFER, 0, (GLsizeiptr)iboBytes, indices);
      glDrawElements(mode, (GLsizei)indexCount, GL_UNSIGNED_SHORT, (const void*)0);
   }
}

void drawBatch(GLenum mode,
               const Point2F* positions,
               const ColorF* colors,
               const Point2F* texCoords,
               U32 vertexCount,
               const U16* indices,
               U32 indexCount,
               bool textured,
               const ColorF& defaultColor,
               F32 alphaThreshold,
               bool wireframe)
{
   if (!positions || vertexCount == 0)
      return;

   sScratchVertices.clear();
   sScratchVertices.setSize(vertexCount);
   for (U32 i = 0; i < vertexCount; ++i)
   {
      DGLVertex& v = sScratchVertices[i];
      v.x = positions[i].x;
      v.y = positions[i].y;
      v.z = 0.0f;
      if (textured && texCoords)
      {
         v.u = texCoords[i].x;
         v.v = texCoords[i].y;
      }
      else
      {
         v.u = 0.0f;
         v.v = 0.0f;
      }

      if (colors)
      {
         v.r = colors[i].red;
         v.g = colors[i].green;
         v.b = colors[i].blue;
         v.a = colors[i].alpha;
      }
      else
      {
         v.r = defaultColor.red;
         v.g = defaultColor.green;
         v.b = defaultColor.blue;
         v.a = defaultColor.alpha;
      }
   }

   const DGLTexMode prevTexMode = sCurrentTexMode;
   const F32 prevAlpha = sCurrentAlphaThreshold;

   sCurrentTexMode = textured ? DGL_TEX_MODULATE : DGL_TEX_NONE;
   sCurrentAlphaThreshold = alphaThreshold;

   if (indices && indexCount > 0)
      drawIndexedVertices(mode, sScratchVertices.address(), vertexCount, indices, indexCount, wireframe);
   else
      drawVertices(mode, sScratchVertices.address(), vertexCount);

   sCurrentTexMode = prevTexMode;
   sCurrentAlphaThreshold = prevAlpha;
}

void begin(GLenum mode)
{
   sInBeginEnd = true;
   sImmediateMode = mode;
   sImmVertices.clear();
}

void texCoord2f(F32 u, F32 v)
{
   sImmU = u;
   sImmV = v;
}

void color4f(F32 r, F32 g, F32 b, F32 a)
{
   sImmR = r;
   sImmG = g;
   sImmB = b;
   sImmA = a;
}

void color3f(F32 r, F32 g, F32 b)
{
   color4f(r, g, b, 1.0f);
}

void color4ub(U8 r, U8 g, U8 b, U8 a)
{
   color4f((F32)r / 255.0f, (F32)g / 255.0f, (F32)b / 255.0f, (F32)a / 255.0f);
}

void color3ub(U8 r, U8 g, U8 b)
{
   color4ub(r, g, b, 255);
}

void colorI(const ColorI& col)
{
   color4ub(col.red, col.green, col.blue, col.alpha);
}

void colorF(const ColorF& col)
{
   color4f(col.red, col.green, col.blue, col.alpha);
}

void vertex3f(F32 x, F32 y, F32 z)
{
   if (!sInBeginEnd)
      return;
   DGLVertex v;
   v.x = x;
   v.y = y;
   v.z = z;
   v.u = sImmU;
   v.v = sImmV;
   v.r = sImmR;
   v.g = sImmG;
   v.b = sImmB;
   v.a = sImmA;
   sImmVertices.push_back(v);
}

void vertex2f(F32 x, F32 y)
{
   vertex3f(x, y, 0.0f);
}

void vertex2i(S32 x, S32 y)
{
   vertex3f((F32)x, (F32)y, 0.0f);
}

void end()
{
   if (!sInBeginEnd)
      return;
   sInBeginEnd = false;
   if (!sImmVertices.empty())
      drawVertices(sImmediateMode, sImmVertices.address(), sImmVertices.size());
   sImmVertices.clear();
}

} // namespace ShaderManager
