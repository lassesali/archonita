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

// The debug render modes are only built in a debug build, 
// partially because a release build should not need them
// and partially because using aglMacro.h or cglMacro.h would prevent us from
// playing this little function-pointer-hijacking trick
#if defined(TORQUE_DEBUG)

#define NO_REDEFINE_GL_FUNCS

#include "platformEmscripten/platformGL.h"
#include "console/console.h"

bool gOutlineEnabled = false;

void (* glDrawElementsProcPtr) (GLenum mode, GLsizei count, GLenum type, const GLvoid *indices) = glDrawElements;
void (* glDrawArraysProcPtr) (GLenum mode, GLint first, GLsizei count) = glDrawArrays;
void (* glNormDrawElements) (GLenum mode, GLsizei count, GLenum type, const GLvoid *indices) = glDrawElements;
void (* glNormDrawArrays) (GLenum mode, GLint first, GLsizei count) = glDrawArrays;

void glOutlineDrawElements(GLenum mode, GLsizei count, GLenum type, const GLvoid *indices)
{
   // WebGL 2 / ES 3.0 has neither glPolygonMode nor glArrayElement, and
   // ShaderManager passes an IBO byte offset (often NULL) in `indices`, so
   // fallback directly to glDrawElements rather than dereferencing `indices`.
   glDrawElements(mode, count, type, indices);
}

void glOutlineDrawArrays(GLenum mode, GLint first, GLsizei count)
{
   glDrawArrays(mode, first, count);
}

ConsoleFunction(GLEnableOutline,void,2,2,"GLEnableOutline( true | false ) - sets whether to render wireframe")
{
   gOutlineEnabled = dAtob(argv[1]);
   if(gOutlineEnabled)
   {
      glDrawElementsProcPtr = glOutlineDrawElements;
      glDrawArraysProcPtr = glOutlineDrawArrays;
      Con::printf("swapped to outline mode funcs");
   } else {
      glDrawElementsProcPtr = glDrawElements;
      glDrawArraysProcPtr = glDrawArrays;
      Con::printf("swapped to normal funcs");
   }
}
#endif
