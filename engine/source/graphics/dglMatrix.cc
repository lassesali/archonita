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

#include "platform/platform.h"
#include "platform/platformGL.h"
#include "math/mMatrix.h"
#include "graphics/dgl.h"

namespace
{
   enum
   {
      MAX_MATRIX_STACK_DEPTH = 32
   };

   struct MatrixStack
   {
      MatrixF stack[MAX_MATRIX_STACK_DEPTH];
      S32 top;
      U32 generation;

      MatrixStack() : top(0), generation(1)
      {
         for (S32 i = 0; i < MAX_MATRIX_STACK_DEPTH; ++i)
            stack[i].identity();
      }

      inline void markDirty()
      {
         if (++generation == 0)
            generation = 1;
      }

      void reset()
      {
         top = 0;
         stack[0].identity();
         markDirty();
      }

      MatrixF& current()
      {
         return stack[top];
      }

      const MatrixF& current() const
      {
         return stack[top];
      }

      void push()
      {
         AssertFatal(top + 1 < MAX_MATRIX_STACK_DEPTH, "dglPushMatrix: Matrix stack overflow!");
         if (top + 1 < MAX_MATRIX_STACK_DEPTH)
         {
            stack[top + 1] = stack[top];
            ++top;
         }
      }

      void pop()
      {
         AssertFatal(top > 0, "dglPopMatrix: Matrix stack underflow!");
         if (top > 0)
         {
            --top;
            markDirty();
         }
      }

      S32 depth() const
      {
         return top + 1;
      }
   };

   static MatrixStack sModelviewStack;
   static MatrixStack sProjectionStack;
   static MatrixStack sTextureStack;
   static GLenum sCurrentMatrixMode = GL_MODELVIEW;

   static MatrixStack& getActiveStack()
   {
      switch (sCurrentMatrixMode)
      {
         case GL_PROJECTION:
            return sProjectionStack;
         case GL_TEXTURE:
            return sTextureStack;
         case GL_MODELVIEW:
         default:
            return sModelviewStack;
      }
   }
} // anonymous namespace

void dglMatrixMode(GLenum mode)
{
   sCurrentMatrixMode = mode;
}

GLenum dglGetMatrixMode()
{
   return sCurrentMatrixMode;
}

void dglPushMatrix()
{
   getActiveStack().push();
}

void dglPopMatrix()
{
   getActiveStack().pop();
}

void dglLoadIdentity()
{
   MatrixStack& stack = getActiveStack();
   stack.current().identity();
   stack.markDirty();
}

void dglLoadMatrix(const MatrixF *m)
{
   if (m)
   {
      MatrixStack& stack = getActiveStack();
      stack.current() = *m;
      stack.markDirty();
   }
}

void dglLoadMatrixf(const F32 *mColMajor)
{
   if (!mColMajor)
      return;
   MatrixF mat;
   dMemcpy((F32*)mat, mColMajor, 16 * sizeof(F32));
   mat.transpose();
   MatrixStack& stack = getActiveStack();
   stack.current() = mat;
   stack.markDirty();
}

void dglMultMatrix(const MatrixF *m)
{
   if (m)
   {
      MatrixStack& stack = getActiveStack();
      stack.current().mul(*m);
      stack.markDirty();
   }
}

void dglMultMatrixf(const F32 *mColMajor)
{
   if (!mColMajor)
      return;
   MatrixF mat;
   dMemcpy((F32*)mat, mColMajor, 16 * sizeof(F32));
   mat.transpose();
   MatrixStack& stack = getActiveStack();
   stack.current().mul(mat);
   stack.markDirty();
}

void dglOrtho(F64 left, F64 right, F64 bottom, F64 top, F64 nearVal, F64 farVal)
{
   const F64 rl = right - left;
   const F64 tb = top - bottom;
   const F64 fn = farVal - nearVal;
   if (mFabs((F32)rl) < 1e-9f || mFabs((F32)tb) < 1e-9f || mFabs((F32)fn) < 1e-9f)
      return;

   MatrixF ortho(true);
   F32* m = ortho;
   m[0]  = (F32)(2.0 / rl);
   m[1]  = 0.0f;
   m[2]  = 0.0f;
   m[3]  = (F32)(-(right + left) / rl);

   m[4]  = 0.0f;
   m[5]  = (F32)(2.0 / tb);
   m[6]  = 0.0f;
   m[7]  = (F32)(-(top + bottom) / tb);

   m[8]  = 0.0f;
   m[9]  = 0.0f;
   m[10] = (F32)(-2.0 / fn);
   m[11] = (F32)(-(farVal + nearVal) / fn);

   m[12] = 0.0f;
   m[13] = 0.0f;
   m[14] = 0.0f;
   m[15] = 1.0f;

   MatrixStack& stack = getActiveStack();
   stack.current().mul(ortho);
   stack.markDirty();
}

void dglFrustum(F64 left, F64 right, F64 bottom, F64 top, F64 nearVal, F64 farVal)
{
   const F64 rl = right - left;
   const F64 tb = top - bottom;
   const F64 fn = farVal - nearVal;
   if (mFabs((F32)rl) < 1e-9f || mFabs((F32)tb) < 1e-9f || mFabs((F32)fn) < 1e-9f)
      return;

   MatrixF frust(false);
   F32* m = frust;
   m[0]  = (F32)((2.0 * nearVal) / rl);
   m[1]  = 0.0f;
   m[2]  = (F32)((right + left) / rl);
   m[3]  = 0.0f;

   m[4]  = 0.0f;
   m[5]  = (F32)((2.0 * nearVal) / tb);
   m[6]  = (F32)((top + bottom) / tb);
   m[7]  = 0.0f;

   m[8]  = 0.0f;
   m[9]  = 0.0f;
   m[10] = (F32)(-(farVal + nearVal) / fn);
   m[11] = (F32)(-(2.0 * farVal * nearVal) / fn);

   m[12] = 0.0f;
   m[13] = 0.0f;
   m[14] = -1.0f;
   m[15] = 0.0f;

   MatrixStack& stack = getActiveStack();
   stack.current().mul(frust);
   stack.markDirty();
}

void dglTranslatef(F32 x, F32 y, F32 z)
{
   MatrixF trans(true);
   F32* m = trans;
   m[3]  = x;
   m[7]  = y;
   m[11] = z;
   MatrixStack& stack = getActiveStack();
   stack.current().mul(trans);
   stack.markDirty();
}

void dglRotatef(F32 angleDeg, F32 x, F32 y, F32 z)
{
   F32 len = mSqrt(x * x + y * y + z * z);
   if (len < 1e-7f)
      return;
   x /= len;
   y /= len;
   z /= len;

   const F32 rad = mDegToRad(angleDeg);
   const F32 c = mCos(rad);
   const F32 s = mSin(rad);
   const F32 t = 1.0f - c;

   MatrixF rot(true);
   F32* m = rot;
   m[0]  = t * x * x + c;
   m[1]  = t * x * y - s * z;
   m[2]  = t * x * z + s * y;
   m[3]  = 0.0f;

   m[4]  = t * x * y + s * z;
   m[5]  = t * y * y + c;
   m[6]  = t * y * z - s * x;
   m[7]  = 0.0f;

   m[8]  = t * x * z - s * y;
   m[9]  = t * y * z + s * x;
   m[10] = t * z * z + c;
   m[11] = 0.0f;

   m[12] = 0.0f;
   m[13] = 0.0f;
   m[14] = 0.0f;
   m[15] = 1.0f;

   MatrixStack& stack = getActiveStack();
   stack.current().mul(rot);
   stack.markDirty();
}

void dglScalef(F32 x, F32 y, F32 z)
{
   MatrixStack& stack = getActiveStack();
   stack.current().scale(Point3F(x, y, z));
   stack.markDirty();
}

void dglGetModelview(MatrixF *m)
{
   if (m)
      *m = sModelviewStack.current();
}

void dglGetProjection(MatrixF *m)
{
   if (m)
      *m = sProjectionStack.current();
}

const MatrixF& dglGetCurrentModelview()
{
   return sModelviewStack.current();
}

const MatrixF& dglGetCurrentProjection()
{
   return sProjectionStack.current();
}

void dglGetModelviewColumnMajor(F32 outColMajor[16])
{
   sModelviewStack.current().transposeTo(outColMajor);
}

void dglGetProjectionColumnMajor(F32 outColMajor[16])
{
   sProjectionStack.current().transposeTo(outColMajor);
}

void dglGetMVPColumnMajor(F32 outColMajor[16])
{
   MatrixF mvp;
   mvp.mul(sProjectionStack.current(), sModelviewStack.current());
   mvp.transposeTo(outColMajor);
}

S32 dglGetModelviewStackDepth()
{
   return sModelviewStack.depth();
}

S32 dglGetProjectionStackDepth()
{
   return sProjectionStack.depth();
}

S32 dglGetTextureStackDepth()
{
   return sTextureStack.depth();
}

void dglResetMatrixStacks()
{
   sModelviewStack.reset();
   sProjectionStack.reset();
   sTextureStack.reset();
   sCurrentMatrixMode = GL_MODELVIEW;
}

U32 dglGetModelviewGeneration()
{
   return sModelviewStack.generation;
}

U32 dglGetProjectionGeneration()
{
   return sProjectionStack.generation;
}
