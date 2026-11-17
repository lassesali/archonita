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

#ifndef GL_GROUP_BEGIN
#define GL_GROUP_BEGIN( flag )
#define UNDEF_MODERN_BEGIN
#endif

#ifndef GL_GROUP_END
#define GL_GROUP_END()
#define UNDEF_MODERN_END
#endif

// Core OpenGL 2.0 - 3.3 / OpenGL ES 3.0 - 3.2 programmable pipeline functions
GL_GROUP_BEGIN(GL_CORE_PROGRAMMABLE)
GL_FUNCTION(void,           glActiveTexture, (GLenum texture), return; )
GL_FUNCTION(void,           glCompressedTexImage2D, (GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLint border, GLsizei imageSize, const void *data), return; )
GL_FUNCTION(void,           glGenBuffers, (GLsizei n, GLuint *buffers), return; )
GL_FUNCTION(void,           glDeleteBuffers, (GLsizei n, const GLuint *buffers), return; )
GL_FUNCTION(void,           glBindBuffer, (GLenum target, GLuint buffer), return; )
GL_FUNCTION(void,           glBufferData, (GLenum target, GLsizeiptr size, const void *data, GLenum usage), return; )
GL_FUNCTION(void,           glBufferSubData, (GLenum target, GLintptr offset, GLsizeiptr size, const void *data), return; )
GL_FUNCTION(void,           glGenVertexArrays, (GLsizei n, GLuint *arrays), return; )
GL_FUNCTION(void,           glDeleteVertexArrays, (GLsizei n, const GLuint *arrays), return; )
GL_FUNCTION(void,           glBindVertexArray, (GLuint array), return; )
GL_FUNCTION(void,           glEnableVertexAttribArray, (GLuint index), return; )
GL_FUNCTION(void,           glDisableVertexAttribArray, (GLuint index), return; )
GL_FUNCTION(void,           glVertexAttribPointer, (GLuint index, GLint size, GLenum type, GLboolean normalized, GLsizei stride, const void *pointer), return; )
GL_FUNCTION(void,           glVertexAttrib4f, (GLuint index, GLfloat x, GLfloat y, GLfloat z, GLfloat w), return; )
GL_FUNCTION(GLuint,         glCreateShader, (GLenum type), return 0; )
GL_FUNCTION(void,           glDeleteShader, (GLuint shader), return; )
GL_FUNCTION(void,           glShaderSource, (GLuint shader, GLsizei count, const GLchar *const*string, const GLint *length), return; )
GL_FUNCTION(void,           glCompileShader, (GLuint shader), return; )
GL_FUNCTION(void,           glGetShaderiv, (GLuint shader, GLenum pname, GLint *params), return; )
GL_FUNCTION(void,           glGetShaderInfoLog, (GLuint shader, GLsizei bufSize, GLsizei *length, GLchar *infoLog), return; )
GL_FUNCTION(GLuint,         glCreateProgram, (void), return 0; )
GL_FUNCTION(void,           glDeleteProgram, (GLuint program), return; )
GL_FUNCTION(void,           glAttachShader, (GLuint program, GLuint shader), return; )
GL_FUNCTION(void,           glLinkProgram, (GLuint program), return; )
GL_FUNCTION(void,           glUseProgram, (GLuint program), return; )
GL_FUNCTION(void,           glGetProgramiv, (GLuint program, GLenum pname, GLint *params), return; )
GL_FUNCTION(void,           glGetProgramInfoLog, (GLuint program, GLsizei bufSize, GLsizei *length, GLchar *infoLog), return; )
GL_FUNCTION(GLint,          glGetUniformLocation, (GLuint program, const GLchar *name), return -1; )
GL_FUNCTION(void,           glUniform1i, (GLint location, GLint v0), return; )
GL_FUNCTION(void,           glUniform1f, (GLint location, GLfloat v0), return; )
GL_FUNCTION(void,           glUniform4f, (GLint location, GLfloat v0, GLfloat v1, GLfloat v2, GLfloat v3), return; )
GL_FUNCTION(void,           glUniform4fv, (GLint location, GLsizei count, const GLfloat *value), return; )
GL_FUNCTION(void,           glUniformMatrix4fv, (GLint location, GLsizei count, GLboolean transpose, const GLfloat *value), return; )
GL_FUNCTION(const GLubyte*, glGetStringi, (GLenum name, GLuint index), return NULL; )
GL_GROUP_END()

// OpenGL 4.2+ / OpenGL ES 3.0+ Immutable Texture Storage
GL_GROUP_BEGIN(GL_CORE_TEX_STORAGE)
GL_FUNCTION(void,           glTexStorage2D, (GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height), return; )
GL_GROUP_END()

// OpenGL 4.5+ / 4.6 Core Direct State Access (ARB_direct_state_access)
GL_GROUP_BEGIN(GL_CORE_DSA)
GL_FUNCTION(void,           glCreateBuffers, (GLsizei n, GLuint *buffers), return; )
GL_FUNCTION(void,           glNamedBufferData, (GLuint buffer, GLsizeiptr size, const void *data, GLenum usage), return; )
GL_FUNCTION(void,           glNamedBufferSubData, (GLuint buffer, GLintptr offset, GLsizeiptr size, const void *data), return; )
GL_FUNCTION(void,           glCreateVertexArrays, (GLsizei n, GLuint *arrays), return; )
GL_FUNCTION(void,           glCreateTextures, (GLenum target, GLsizei n, GLuint *textures), return; )
GL_FUNCTION(void,           glTextureStorage2D, (GLuint texture, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height), return; )
GL_FUNCTION(void,           glTextureSubImage2D, (GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLenum type, const void *pixels), return; )
GL_FUNCTION(void,           glTextureParameteri, (GLuint texture, GLenum pname, GLint param), return; )
GL_FUNCTION(void,           glTextureParameteriv, (GLuint texture, GLenum pname, const GLint *param), return; )
GL_FUNCTION(void,           glBindTextureUnit, (GLuint unit, GLuint texture), return; )
GL_GROUP_END()

#ifdef UNDEF_MODERN_BEGIN
#undef GL_GROUP_BEGIN
#undef UNDEF_MODERN_BEGIN
#endif

#ifdef UNDEF_MODERN_END
#undef GL_GROUP_END
#undef UNDEF_MODERN_END
#endif
