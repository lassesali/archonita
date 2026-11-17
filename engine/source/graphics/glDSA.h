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

#ifndef _GLDSA_H_
#define _GLDSA_H_

#ifndef _PLATFORM_H_
#include "platform/platform.h"
#endif

#ifndef _PLATFORMGL_H_
#include "platform/platformGL.h"
#endif

//-----------------------------------------------------------------------------
// OpenGL 4.5+ Direct State Access (DSA) cross-platform abstraction layer.
// On Desktop OpenGL 4.5/4.6 Core Profile (when gGLState.suppDSA is true),
// these wrappers dispatch directly to ARB_direct_state_access / GL 4.5 entry
// points (glCreateBuffers, glNamedBufferData, glCreateTextures, etc.).
// On OpenGL ES 3.2 (Android/iOS/WebGL2) or pre-4.5 fallback contexts, they
// transparently dispatch to standard bind-based OpenGL ES 3.2 / GL 3.3 calls.
//-----------------------------------------------------------------------------
namespace GLDSA
{
   bool isSupported();

   // Buffer object wrappers
   GLuint createBuffer();
   void deleteBuffer(GLuint buffer);
   void namedBufferData(GLuint buffer, GLenum fallbackTarget, GLsizeiptr size, const void* data, GLenum usage);
   void namedBufferSubData(GLuint buffer, GLenum fallbackTarget, GLintptr offset, GLsizeiptr size, const void* data);

   // Vertex Array Object wrappers
   GLuint createVertexArray();
   void deleteVertexArray(GLuint vao);

   // 2D Texture wrappers
   GLuint createTexture2D();
   void deleteTexture(GLuint texture);
   void textureStorage2D(GLuint texture, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height);
   void textureSubImage2D(GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLenum type, const void* pixels);
   void textureImage2D(GLuint texture, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, const void* pixels);
   void textureParameteri(GLuint texture, GLenum pname, GLint param);
   void textureParameteriv(GLuint texture, GLenum pname, const GLint* params);
   void textureParameterf(GLuint texture, GLenum pname, GLfloat param);
   void setTextureSwizzle(GLuint texture, GLint r, GLint g, GLint b, GLint a);
   void bindTextureUnit(GLuint unit, GLuint texture);
}

#endif // _GLDSA_H_
