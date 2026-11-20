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

#ifndef _SHADERMANAGER_H_
#define _SHADERMANAGER_H_

#ifndef _PLATFORM_H_
#include "platform/platform.h"
#endif

#ifndef _PLATFORMGL_H_
#include "platform/platformGL.h"
#endif

#ifndef _MMATH_H_
#include "math/mMath.h"
#endif

#ifndef _COLOR_H_
#include "graphics/gColor.h"
#endif

//-----------------------------------------------------------------------------
// Interleaved vertex format for 2D / 3D programmable pipeline rendering
//-----------------------------------------------------------------------------
struct DGLVertex
{
   F32 x, y, z;
   F32 u, v;
   F32 r, g, b, a;

   inline void set(F32 px, F32 py, F32 pz, F32 pu, F32 pv, F32 pr, F32 pg, F32 pb, F32 pa)
   {
      x = px; y = py; z = pz;
      u = pu; v = pv;
      r = pr; g = pg; b = pb; a = pa;
   }
};

//-----------------------------------------------------------------------------
// Shader texture environment modes
//-----------------------------------------------------------------------------
enum DGLTexMode
{
   DGL_TEX_NONE     = 0, // Untextured (vertex color only)
   DGL_TEX_MODULATE = 1, // frag = tex * vColor
   DGL_TEX_BLEND    = 2, // frag = vec4(mix(vColor.rgb, uTexEnvColor.rgb, tex.rgb), vColor.a * tex.a)
   DGL_TEX_REPLACE  = 3, // frag = tex
   DGL_TEX_ADD      = 4  // frag = vec4(vColor.rgb + tex.rgb, vColor.a * tex.a)
};

//-----------------------------------------------------------------------------
// Core Shader & Streaming VAO/VBO/IBO Manager for OpenGL 4.6 Core / GLES 3.2
//-----------------------------------------------------------------------------
namespace ShaderManager
{
   // Lifecycle
   bool init();
   void shutdown();
   bool isInitialized();

   // Preprocessor: injects #version 460 core (desktop) or #version 320 es +
   // precision highp float; (mobile GLES 3.2) before shaderBody.
   bool buildPreprocessedSource(const char* shaderBody,
                                bool isGLES,
                                U32 majorVer,
                                U32 minorVer,
                                char* outBuffer,
                                U32 outBufferSize);

   // Shader compilation & linking helpers
   GLuint compileShader(GLenum shaderType, const char* shaderBody);
   GLuint createProgram(const char* vertexBody, const char* fragmentBody);

   // Render state controls for the built-in 2D Uber-Shader
   void setTextureMode(DGLTexMode mode);
   DGLTexMode getTextureMode();
   void setTextureEnvColor(const ColorF& col);
   void setAlphaThreshold(F32 threshold);
   F32  getAlphaThreshold();

   // User clip planes (replaces fixed-function GL_CLIP_PLANE0..3)
   void setClipPlane(U32 index, F64 a, F64 b, F64 c, F64 d);
   void enableClipPlane(U32 index, bool enable);
   void clearClipPlanes();

   // Streaming VAO/VBO/IBO draw calls
   void drawVertices(GLenum mode, const DGLVertex* vertices, U32 vertexCount);
   void drawIndexedVertices(GLenum mode,
                            const DGLVertex* vertices,
                            U32 vertexCount,
                            const U16* indices,
                            U32 indexCount,
                            bool wireframe = false);

   // Two-step indexed drawing: upload vertex data and bind VAO/shader once,
   // then issue one or more drawUploadedIndices() calls (e.g., per texture bucket).
   void beginIndexedDraw(const DGLVertex* vertices, U32 vertexCount);
   void drawUploadedIndices(GLenum mode,
                            const U16* indices,
                            U32 indexCount,
                            bool wireframe = false);

   // Specialized batch submission for BatchRender
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
                  bool wireframe);

   // Immediate-mode replacement builder (converts GL_QUADS/GL_POLYGON to triangles)
   void begin(GLenum mode);
   void texCoord2f(F32 u, F32 v);
   void color4f(F32 r, F32 g, F32 b, F32 a);
   void color3f(F32 r, F32 g, F32 b);
   void color4ub(U8 r, U8 g, U8 b, U8 a);
   void color3ub(U8 r, U8 g, U8 b);
   void colorI(const ColorI& col);
   void colorF(const ColorF& col);
   void vertex3f(F32 x, F32 y, F32 z);
   void vertex2f(F32 x, F32 y);
   void vertex2i(S32 x, S32 y);
   void end();
}

#endif // _SHADERMANAGER_H_
