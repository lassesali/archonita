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

#ifndef _GL_MODERN_TYPES_H_
#define _GL_MODERN_TYPES_H_

#include <stddef.h>

typedef char GLchar;
typedef ptrdiff_t GLintptr;
typedef ptrdiff_t GLsizeiptr;

// Context & Version queries (OpenGL 3.0+ / 4.6 Core & OpenGL ES 3.0+ / 3.2)
#ifndef GL_MAJOR_VERSION
#define GL_MAJOR_VERSION                        0x821B
#endif
#ifndef GL_MINOR_VERSION
#define GL_MINOR_VERSION                        0x821C
#endif
#ifndef GL_NUM_EXTENSIONS
#define GL_NUM_EXTENSIONS                       0x821D
#endif
#ifndef GL_CONTEXT_FLAGS
#define GL_CONTEXT_FLAGS                        0x821E
#endif
#ifndef GL_CONTEXT_PROFILE_MASK
#define GL_CONTEXT_PROFILE_MASK                 0x9126
#endif
#ifndef GL_CONTEXT_CORE_PROFILE_BIT
#define GL_CONTEXT_CORE_PROFILE_BIT             0x00000001
#endif
#ifndef GL_CONTEXT_COMPATIBILITY_PROFILE_BIT
#define GL_CONTEXT_COMPATIBILITY_PROFILE_BIT    0x00000002
#endif
#ifndef GL_SHADING_LANGUAGE_VERSION
#define GL_SHADING_LANGUAGE_VERSION             0x8B8C
#endif

// WGL_ARB_create_context / WGL_ARB_create_context_profile
#ifndef WGL_CONTEXT_MAJOR_VERSION_ARB
#define WGL_CONTEXT_MAJOR_VERSION_ARB           0x2091
#endif
#ifndef WGL_CONTEXT_MINOR_VERSION_ARB
#define WGL_CONTEXT_MINOR_VERSION_ARB           0x2092
#endif
#ifndef WGL_CONTEXT_LAYER_PLANE_ARB
#define WGL_CONTEXT_LAYER_PLANE_ARB             0x2093
#endif
#ifndef WGL_CONTEXT_FLAGS_ARB
#define WGL_CONTEXT_FLAGS_ARB                   0x2094
#endif
#ifndef WGL_CONTEXT_DEBUG_BIT_ARB
#define WGL_CONTEXT_DEBUG_BIT_ARB               0x0001
#endif
#ifndef WGL_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB
#define WGL_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB  0x0002
#endif
#ifndef WGL_CONTEXT_PROFILE_MASK_ARB
#define WGL_CONTEXT_PROFILE_MASK_ARB            0x9126
#endif
#ifndef WGL_CONTEXT_CORE_PROFILE_BIT_ARB
#define WGL_CONTEXT_CORE_PROFILE_BIT_ARB        0x00000001
#endif
#ifndef WGL_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB
#define WGL_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB 0x00000002
#endif

// Buffer Objects & Vertex Array Objects
#ifndef GL_ARRAY_BUFFER
#define GL_ARRAY_BUFFER                         0x8892
#endif
#ifndef GL_ELEMENT_ARRAY_BUFFER
#define GL_ELEMENT_ARRAY_BUFFER                 0x8893
#endif
#ifndef GL_UNIFORM_BUFFER
#define GL_UNIFORM_BUFFER                       0x8A11
#endif
#ifndef GL_STREAM_DRAW
#define GL_STREAM_DRAW                          0x88E0
#endif
#ifndef GL_STATIC_DRAW
#define GL_STATIC_DRAW                          0x88E4
#endif
#ifndef GL_DYNAMIC_DRAW
#define GL_DYNAMIC_DRAW                         0x88E8
#endif
#ifndef GL_VERTEX_ARRAY_BINDING
#define GL_VERTEX_ARRAY_BINDING                 0x85B5
#endif
#ifndef GL_ARRAY_BUFFER_BINDING
#define GL_ARRAY_BUFFER_BINDING                 0x8894
#endif
#ifndef GL_ELEMENT_ARRAY_BUFFER_BINDING
#define GL_ELEMENT_ARRAY_BUFFER_BINDING         0x8895
#endif

// Shaders & Programs
#ifndef GL_FRAGMENT_SHADER
#define GL_FRAGMENT_SHADER                      0x8B30
#endif
#ifndef GL_VERTEX_SHADER
#define GL_VERTEX_SHADER                        0x8B31
#endif
#ifndef GL_COMPILE_STATUS
#define GL_COMPILE_STATUS                       0x8B81
#endif
#ifndef GL_LINK_STATUS
#define GL_LINK_STATUS                          0x8B82
#endif
#ifndef GL_VALIDATE_STATUS
#define GL_VALIDATE_STATUS                      0x8B83
#endif
#ifndef GL_INFO_LOG_LENGTH
#define GL_INFO_LOG_LENGTH                      0x8B84
#endif
#ifndef GL_CURRENT_PROGRAM
#define GL_CURRENT_PROGRAM                      0x8B8D
#endif

// Textures, Sized Internal Formats & Swizzles (GL 4.6 Core & GLES 3.2)
#ifndef GL_TEXTURE0
#define GL_TEXTURE0                             0x84C0
#endif
#ifndef GL_TEXTURE1
#define GL_TEXTURE1                             0x84C1
#endif
#ifndef GL_ACTIVE_TEXTURE
#define GL_ACTIVE_TEXTURE                       0x84E0
#endif
#ifndef GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS
#define GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS     0x8B4D
#endif
#ifndef GL_RED
#define GL_RED                                  0x1903
#endif
#ifndef GL_RG
#define GL_RG                                   0x8227
#endif
#ifndef GL_R8
#define GL_R8                                   0x8229
#endif
#ifndef GL_RG8
#define GL_RG8                                  0x822B
#endif
#ifndef GL_RGB8
#define GL_RGB8                                 0x8051
#endif
#ifndef GL_RGBA8
#define GL_RGBA8                                0x8058
#endif
#ifndef GL_RGB565
#define GL_RGB565                               0x8D62
#endif
#ifndef GL_RGB5_A1
#define GL_RGB5_A1                              0x8057
#endif
#ifndef GL_RGBA4
#define GL_RGBA4                                0x8056
#endif
#ifndef GL_TEXTURE_SWIZZLE_R
#define GL_TEXTURE_SWIZZLE_R                    0x8E42
#endif
#ifndef GL_TEXTURE_SWIZZLE_G
#define GL_TEXTURE_SWIZZLE_G                    0x8E43
#endif
#ifndef GL_TEXTURE_SWIZZLE_B
#define GL_TEXTURE_SWIZZLE_B                    0x8E44
#endif
#ifndef GL_TEXTURE_SWIZZLE_A
#define GL_TEXTURE_SWIZZLE_A                    0x8E45
#endif
#ifndef GL_TEXTURE_SWIZZLE_RGBA
#define GL_TEXTURE_SWIZZLE_RGBA                 0x8E46
#endif
#ifndef GL_TEXTURE_IMMUTABLE_FORMAT
#define GL_TEXTURE_IMMUTABLE_FORMAT             0x912F
#endif

// Legacy enum compatibility guards for Core / GLES 3.2 headers
#ifndef GL_QUADS
#define GL_QUADS                                0x0007
#endif
#ifndef GL_QUAD_STRIP
#define GL_QUAD_STRIP                           0x0008
#endif
#ifndef GL_POLYGON
#define GL_POLYGON                              0x0009
#endif
#ifndef GL_MODELVIEW
#define GL_MODELVIEW                            0x1700
#endif
#ifndef GL_PROJECTION
#define GL_PROJECTION                           0x1701
#endif
#ifndef GL_MODELVIEW_MATRIX
#define GL_MODELVIEW_MATRIX                     0x0BA6
#endif
#ifndef GL_PROJECTION_MATRIX
#define GL_PROJECTION_MATRIX                    0x0BA7
#endif
#ifndef GL_TEXTURE_MATRIX
#define GL_TEXTURE_MATRIX                       0x0BA8
#endif
#ifndef GL_MODELVIEW_STACK_DEPTH
#define GL_MODELVIEW_STACK_DEPTH                0x0BA3
#endif
#ifndef GL_PROJECTION_STACK_DEPTH
#define GL_PROJECTION_STACK_DEPTH               0x0BA4
#endif
#ifndef GL_TEXTURE_STACK_DEPTH
#define GL_TEXTURE_STACK_DEPTH                  0x0BA5
#endif
#ifndef GL_MODULATE
#define GL_MODULATE                             0x2100
#endif
#ifndef GL_CLIP_PLANE0
#define GL_CLIP_PLANE0                          0x3000
#define GL_CLIP_PLANE1                          0x3001
#define GL_CLIP_PLANE2                          0x3002
#define GL_CLIP_PLANE3                          0x3003
#define GL_CLIP_PLANE4                          0x3004
#define GL_CLIP_PLANE5                          0x3005
#endif
#ifndef _GLMODERNTYPES_H_
#define _GLMODERNTYPES_H_
#endif

#ifndef GL_MAX_TEXTURE_IMAGE_UNITS
#define GL_MAX_TEXTURE_IMAGE_UNITS              0x8872
#endif
#ifndef GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS
#define GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS     0x8B4D
#endif
#ifndef GL_MAX_CLIP_PLANES
#define GL_MAX_CLIP_PLANES                      0x0D32
#endif

#endif // _GL_MODERN_TYPES_H_
