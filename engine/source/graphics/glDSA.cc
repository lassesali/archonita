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

#include "graphics/glDSA.h"

namespace GLDSA
{

bool isSupported()
{
#if defined(TORQUE_OS_WIN) || defined(TORQUE_OS_LINUX)
   return gGLState.suppDSA;
#else
   return false;
#endif
}

GLuint createBuffer()
{
   GLuint buf = 0;
#if defined(TORQUE_OS_WIN) || defined(TORQUE_OS_LINUX)
   if (gGLState.suppDSA)
   {
      glCreateBuffers(1, &buf);
      return buf;
   }
#endif
   glGenBuffers(1, &buf);
   return buf;
}

void deleteBuffer(GLuint buffer)
{
   if (buffer != 0)
      glDeleteBuffers(1, &buffer);
}

void namedBufferData(GLuint buffer, GLenum fallbackTarget, GLsizeiptr size, const void* data, GLenum usage)
{
#if defined(TORQUE_OS_WIN) || defined(TORQUE_OS_LINUX)
   if (gGLState.suppDSA)
   {
      glNamedBufferData(buffer, size, data, usage);
      return;
   }
#endif
   glBindBuffer(fallbackTarget, buffer);
   glBufferData(fallbackTarget, size, data, usage);
}

void namedBufferSubData(GLuint buffer, GLenum fallbackTarget, GLintptr offset, GLsizeiptr size, const void* data)
{
#if defined(TORQUE_OS_WIN) || defined(TORQUE_OS_LINUX)
   if (gGLState.suppDSA)
   {
      glNamedBufferSubData(buffer, offset, size, data);
      return;
   }
#endif
   glBindBuffer(fallbackTarget, buffer);
   glBufferSubData(fallbackTarget, offset, size, data);
}

GLuint createVertexArray()
{
   GLuint vao = 0;
#if defined(TORQUE_OS_WIN) || defined(TORQUE_OS_LINUX)
   if (gGLState.suppDSA)
   {
      glCreateVertexArrays(1, &vao);
      return vao;
   }
   glGenVertexArrays(1, &vao);
#elif defined(TORQUE_OS_ANDROID)
   glGenVertexArrays(1, &vao);
#endif
   return vao;
}

void deleteVertexArray(GLuint vao)
{
#if defined(TORQUE_OS_WIN) || defined(TORQUE_OS_LINUX) || defined(TORQUE_OS_ANDROID)
   if (vao != 0)
      glDeleteVertexArrays(1, &vao);
#endif
}

GLuint createTexture2D()
{
   GLuint tex = 0;
#if defined(TORQUE_OS_WIN) || defined(TORQUE_OS_LINUX)
   if (gGLState.suppDSA)
   {
      glCreateTextures(GL_TEXTURE_2D, 1, &tex);
      return tex;
   }
#endif
   glGenTextures(1, &tex);
   glBindTexture(GL_TEXTURE_2D, tex);
   return tex;
}

void deleteTexture(GLuint texture)
{
   if (texture != 0)
      glDeleteTextures(1, &texture);
}

void textureStorage2D(GLuint texture, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height)
{
#if defined(TORQUE_OS_WIN) || defined(TORQUE_OS_LINUX)
   if (gGLState.suppDSA)
   {
      glTextureStorage2D(texture, levels, internalformat, width, height);
      return;
   }
   glBindTexture(GL_TEXTURE_2D, texture);
   glTexStorage2D(GL_TEXTURE_2D, levels, internalformat, width, height);
#elif defined(TORQUE_OS_ANDROID)
   glBindTexture(GL_TEXTURE_2D, texture);
   glTexStorage2D(GL_TEXTURE_2D, levels, internalformat, width, height);
#endif
}

void textureSubImage2D(GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLenum type, const void* pixels)
{
#if defined(TORQUE_OS_WIN) || defined(TORQUE_OS_LINUX)
   if (gGLState.suppDSA)
   {
      glTextureSubImage2D(texture, level, xoffset, yoffset, width, height, format, type, pixels);
      return;
   }
#endif
   glBindTexture(GL_TEXTURE_2D, texture);
   glTexSubImage2D(GL_TEXTURE_2D, level, xoffset, yoffset, width, height, format, type, pixels);
}

void textureImage2D(GLuint texture, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, const void* pixels)
{
   glBindTexture(GL_TEXTURE_2D, texture);
   glTexImage2D(GL_TEXTURE_2D, level, internalformat, width, height, border, format, type, pixels);
}

void textureParameteri(GLuint texture, GLenum pname, GLint param)
{
#if defined(TORQUE_OS_WIN) || defined(TORQUE_OS_LINUX)
   if (gGLState.suppDSA)
   {
      glTextureParameteri(texture, pname, param);
      return;
   }
#endif
   glBindTexture(GL_TEXTURE_2D, texture);
   glTexParameteri(GL_TEXTURE_2D, pname, param);
}

void textureParameteriv(GLuint texture, GLenum pname, const GLint* params)
{
#if defined(TORQUE_OS_WIN) || defined(TORQUE_OS_LINUX)
   if (gGLState.suppDSA)
   {
      glTextureParameteriv(texture, pname, params);
      return;
   }
#endif
   glBindTexture(GL_TEXTURE_2D, texture);
   glTexParameteriv(GL_TEXTURE_2D, pname, params);
}

void textureParameterf(GLuint texture, GLenum pname, GLfloat param)
{
   glBindTexture(GL_TEXTURE_2D, texture);
   glTexParameterf(GL_TEXTURE_2D, pname, param);
}

void setTextureSwizzle(GLuint texture, GLint r, GLint g, GLint b, GLint a)
{
   textureParameteri(texture, GL_TEXTURE_SWIZZLE_R, r);
   textureParameteri(texture, GL_TEXTURE_SWIZZLE_G, g);
   textureParameteri(texture, GL_TEXTURE_SWIZZLE_B, b);
   textureParameteri(texture, GL_TEXTURE_SWIZZLE_A, a);
}

void bindTextureUnit(GLuint unit, GLuint texture)
{
#if defined(TORQUE_OS_WIN) || defined(TORQUE_OS_LINUX)
   if (gGLState.suppDSA)
   {
      glBindTextureUnit(unit, texture);
      return;
   }
   glActiveTexture(GL_TEXTURE0 + unit);
#elif defined(TORQUE_OS_ANDROID) || defined(TORQUE_OS_EMSCRIPTEN) || defined(TORQUE_OS_IOS) || defined(TORQUE_OS_OSX)
   glActiveTexture(GL_TEXTURE0 + unit);
#endif
   glBindTexture(GL_TEXTURE_2D, texture);
}

} // namespace GLDSA
