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

#include "math/mPoint.h"
#include "graphics/TextureManager.h"
#include "graphics/dgl.h"
#include "graphics/gColor.h"
#include "graphics/ShaderManager.h"
#include "graphics/glDSA.h"
#include "math/mPoint.h"
#include "math/mRect.h"
#include "graphics/gFont.h"
#include "console/console.h"
#include "math/mMatrix.h"
#include "memory/frameAllocator.h"
#include "debug/profiler.h"
#include "string/unicode.h"

#include "dglMac_ScriptBinding.h"
#include "dgl_ScriptBinding.h"

#include <vector>

namespace {

ColorI sg_bitmapModulation(255, 255, 255, 255);
ColorI sg_textAnchorColor(255, 255, 255, 255);
ColorI sg_stackColor(255, 255, 255, 255);
RectI sgCurrentClipRect;

void flushGlyphBatch(TextureObject* tex, Vector<DGLVertex>& vertices, Vector<U16>& indices)
{
   if (tex && !vertices.empty() && !indices.empty())
   {
      GLDSA::bindTextureUnit(0, tex->getGLTextureName());
      ShaderManager::drawIndexedVertices(GL_TRIANGLES,
                                         vertices.address(), vertices.size(),
                                         indices.address(), indices.size());
   }
   vertices.clear();
   indices.clear();
}

} // namespace {}


//--------------------------------------------------------------------------
void dglSetBitmapModulation(const ColorF& in_rColor)
{
   ColorF c = in_rColor;
   c.clamp();
   sg_bitmapModulation = c;
   sg_textAnchorColor = sg_bitmapModulation;
}

void dglGetBitmapModulation(ColorF* color)
{
   *color = sg_bitmapModulation;
}

void dglGetBitmapModulation(ColorI* color)
{
   *color = sg_bitmapModulation;
}

void dglClearBitmapModulation()
{
   sg_bitmapModulation.set(255, 255, 255, 255);
}

void dglSetTextAnchorColor(const ColorF& in_rColor)
{
   ColorF c = in_rColor;
   c.clamp();
   sg_textAnchorColor = c;
}


//--------------------------------------------------------------------------
void dglDrawBitmapStretchSR(TextureObject* texture,
                       const RectI&   dstRect,
                       const RectI&   srcRect,
                       const U32   in_flip,
                       F32			 fSpin,
                       bool				bSilhouette)
{	
   AssertFatal(texture != NULL, "GSurface::drawBitmapStretchSR: NULL Handle");
   if(!dstRect.isValidRect())
      return;
   AssertFatal(srcRect.isValidRect() == true,
               "GSurface::drawBitmapStretchSR: routines assume normal rects");

   GLDSA::bindTextureUnit(0, texture->getGLTextureName());

   if (bSilhouette)
   {
      ShaderManager::setTextureMode(DGL_TEX_BLEND);

      ColorF kModulationColor;
      dglGetBitmapModulation(&kModulationColor);
      ShaderManager::setTextureEnvColor(kModulationColor);
   }
   else
   {
      ShaderManager::setTextureMode(DGL_TEX_MODULATE);
   }
   glEnable(GL_BLEND);
   glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

   F32 texLeft   = F32(srcRect.point.x)                    / F32(texture->getTextureWidth());
   F32 texRight  = F32(srcRect.point.x + srcRect.extent.x) / F32(texture->getTextureWidth());
   F32 texTop    = F32(srcRect.point.y)                    / F32(texture->getTextureHeight());
   F32 texBottom = F32(srcRect.point.y + srcRect.extent.y) / F32(texture->getTextureHeight());
     Point2F scrPoints[4];
   if(fSpin == 0.0f)
   {
            scrPoints[0].x = (F32)dstRect.point.x;
            scrPoints[0].y = (F32)dstRect.point.y;
            scrPoints[1].x = (F32)(dstRect.point.x + dstRect.extent.x);
            scrPoints[1].y = (F32)dstRect.point.y;
            scrPoints[2].x = (F32)dstRect.point.x;
            scrPoints[2].y = (F32)(dstRect.point.y + dstRect.extent.y);
            scrPoints[3].x = (F32)(dstRect.point.x + dstRect.extent.x);
            scrPoints[3].y = (F32)(dstRect.point.y + dstRect.extent.y);
        }
        else
   {
     //WE NEED TO IMPLEMENT A FAST 2D ROTATION -- NOT THIS SLOWER 3D ROTATION
     MatrixF rotMatrix( EulerF( 0.0, 0.0, mDegToRad(fSpin) ) );

     Point3F offset( dstRect.point.x + dstRect.extent.x / 2.0f,
                                         dstRect.point.y + dstRect.extent.y / 2.0f, 0.0 );
         Point3F points[4];
   
     points[0] = Point3F(-dstRect.extent.x / 2.0f,  dstRect.extent.y / 2.0f, 0.0);
     points[1] = Point3F( dstRect.extent.x / 2.0f,  dstRect.extent.y / 2.0f, 0.0);
     points[2] = Point3F(-dstRect.extent.x / 2.0f, -dstRect.extent.y / 2.0f, 0.0);
     points[3] = Point3F( dstRect.extent.x / 2.0f, -dstRect.extent.y / 2.0f, 0.0);

     for( int i=0; i<4; i++ )
     {
        rotMatrix.mulP( points[i] );
        points[i] += offset;
        scrPoints[i].x = points[i].x;
        scrPoints[i].y = points[i].y;
     }
   }
  

   if(in_flip & GFlip_X)
   {
      F32 temp = texLeft;
      texLeft = texRight;
      texRight = temp;
   }
   if(in_flip & GFlip_Y)
   {
      F32 temp = texTop;
      texTop = texBottom;
      texBottom = temp;
   }

   const ColorF modColor(sg_bitmapModulation);
   DGLVertex quadVerts[4];
   quadVerts[0].set(scrPoints[0].x, scrPoints[0].y, 0.0f, texLeft,  texTop,    modColor.red, modColor.green, modColor.blue, modColor.alpha);
   quadVerts[1].set(scrPoints[1].x, scrPoints[1].y, 0.0f, texRight, texTop,    modColor.red, modColor.green, modColor.blue, modColor.alpha);
   quadVerts[2].set(scrPoints[2].x, scrPoints[2].y, 0.0f, texLeft,  texBottom, modColor.red, modColor.green, modColor.blue, modColor.alpha);
   quadVerts[3].set(scrPoints[3].x, scrPoints[3].y, 0.0f, texRight, texBottom, modColor.red, modColor.green, modColor.blue, modColor.alpha);

   ShaderManager::drawVertices(GL_TRIANGLE_STRIP, quadVerts, 4);

   if (bSilhouette)
   {
      ShaderManager::setTextureEnvColor(ColorF(0.0f, 0.0f, 0.0f, 0.0f));
   }

   ShaderManager::setTextureMode(DGL_TEX_NONE);
   glDisable(GL_BLEND);
}

void dglDrawBitmap(TextureObject* texture, const Point2I& in_rAt, const U32 in_flip)
{
   AssertFatal(texture != NULL, "GSurface::drawBitmap: NULL Handle");

   // All non-StretchSR bitmaps are transformed into StretchSR calls...
   //
   RectI subRegion(0, 0,
                   texture->getBitmapWidth(),
                   texture->getBitmapHeight());
   RectI stretch(in_rAt.x, in_rAt.y,
                   texture->getBitmapWidth(),
                   texture->getBitmapHeight());
   dglDrawBitmapStretchSR(texture,
                          stretch,
                          subRegion,
                          in_flip);
}

void dglDrawBitmapTile(TextureObject* texture, const RectI& dstRect, const U32 in_flip, F32 fSpin, bool bSilhouette)
{
   AssertFatal(texture != NULL, "GSurface::drawBitmapTile: NULL Handle");
   // since the texture coords are calculated from the texture sub-rect we pass in,
   // but no actual indexing of that sub-rect happens, we can pass any texture coords
   // that we like to dglDrawBitmapSR(). We use this to force tiling.
   RectI subregion(0,0, dstRect.extent.x, dstRect.extent.y);
   dglDrawBitmapStretchSR(texture, dstRect, subregion, in_flip, fSpin, bSilhouette);
}

void dglDrawBitmapStretch(TextureObject* texture, const RectI& dstRect, const U32 in_flip, F32 fSpin, bool bSilhouette)
{
   AssertFatal(texture != NULL, "GSurface::drawBitmapStretch: NULL Handle");
   AssertFatal(dstRect.isValidRect() == true,
               "GSurface::drawBitmapStretch: routines assume normal rects");

   RectI subRegion(0, 0,
                   texture->getBitmapWidth(),
                   texture->getBitmapHeight());
   dglDrawBitmapStretchSR(texture,
                          dstRect,
                          subRegion,
                          in_flip,
                          fSpin,
                          bSilhouette);
}

void dglDrawBitmapSR(TextureObject *texture, const Point2I& in_rAt, const RectI& srcRect, const U32 in_flip)
{
   AssertFatal(texture != NULL, "GSurface::drawBitmapSR: NULL Handle");
   AssertFatal(srcRect.isValidRect() == true,
               "GSurface::drawBitmapSR: routines assume normal rects");

   RectI stretch(in_rAt.x, in_rAt.y,
                 srcRect.len_x(),
                 srcRect.len_y());
   dglDrawBitmapStretchSR(texture,
                          stretch,
                          srcRect,
                          in_flip);
}

U32 dglDrawText(GFont*   font,
          const Point2I& ptDraw,
          const UTF16*    in_string,
          const ColorI*  colorTable,
          const U32      maxColorIndex,
          F32            rot)
{
   return dglDrawTextN(font, ptDraw, in_string, dStrlen(in_string), colorTable, maxColorIndex, rot);
}

U32 dglDrawText(GFont*   font,
          const Point2I& ptDraw,
          const UTF8*    in_string,
          const ColorI*  colorTable,
          const U32      maxColorIndex,
          F32            rot)
{
   // Just a note - dStrlen(utf8) isn't strictly correct but it's guaranteed to be
   // as long or longer than the real length. dglDrawTextN fails gracefully
   // if you specify overlong, so this is ok.
   return dglDrawTextN(font, ptDraw, in_string, dStrlen((const UTF8 *) in_string), colorTable, maxColorIndex, rot);
}

struct TextVertex
{
   Point2F p;
   Point2F t;
   ColorI c;
    TextVertex() { set( 0.0f, 0.0f, 0.0f, 0.0f, ColorI(0, 0, 0) ); }
   void set(F32 x, F32 y, F32 tx, F32 ty, ColorI color)
   {
      p.x = x;
      p.y = y;
      t.x = tx;
      t.y = ty;
      c = color;
   }
};

//------------------------------------------------------------------------------

U32 dglDrawTextN(GFont*          font,
                 const Point2I&  ptDraw,
                 const UTF8*     in_string,
                 U32             n,
                 const ColorI*   colorTable,
                 const U32       maxColorIndex,
                 F32             rot)
{
   PROFILE_START(DrawText_UTF8);
   
   U32 len = dStrlen(in_string) + 1;
   FrameTemp<UTF16> ubuf(len);
   convertUTF8toUTF16(in_string, ubuf, len);
   U32 tmp = dglDrawTextN(font, ptDraw, ubuf, n, colorTable, maxColorIndex, rot);

   PROFILE_END();

   return tmp;
}

//-----------------------------------------------------------------------------

U32 dglDrawTextN(GFont*          font,
                 const Point2I&  ptDraw,
                 const UTF16*    in_string,
                 U32             n,
                 const ColorI*   colorTable,
                 const U32       maxColorIndex,
                 F32             rot)
{
   // return on zero length strings
   if( n < 1 )
      return ptDraw.x;

   PROFILE_START(DrawText);

   MatrixF rotMatrix( EulerF( 0.0, 0.0, mDegToRad( rot ) ) );
   Point3F offset( (F32)ptDraw.x, (F32)ptDraw.y, 0.0f );
   Point3F points[4];

   U32 nCharCount = 0;

   Point2I     pt;
   UTF16       c;
   pt.x                 = 0;

   ColorI currentColor = sg_bitmapModulation;
   TextureObject *lastTexture = NULL;

   static Vector<DGLVertex> sTextVertices(__FILE__, __LINE__);
   static Vector<U16>       sTextIndices(__FILE__, __LINE__);
   sTextVertices.clear();
   sTextIndices.clear();

   ShaderManager::setTextureMode(DGL_TEX_MODULATE);
   glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
   glEnable(GL_BLEND);

   // Build the point, color, and coord arrays
   U32 i;
   for(i = 0, c = in_string[i]; in_string[i] && i < n; i++, c = in_string[i])
   {
      nCharCount++;
      if(nCharCount > n)
          break;

      // We have to do a little dance here since \t = 0x9, \n = 0xa, and \r = 0xd
      if ((c >=  1 && c <=  7) ||
         (c >= 11 && c <= 12) ||
         (c == 14))
      {
         // Color code
         if (colorTable)
         {
            static U8 remap[15] =
            {
               0x0, // 0 special null terminator
               0x0, // 1 ascii start-of-heading??
               0x1, 
               0x2, 
               0x3, 
               0x4, 
               0x5, 
               0x6, 
               0x0, // 8 special backspace
               0x0, // 9 special tab
               0x0, // a special \n
               0x7, 
               0x8,
               0x0, // a special \r
               0x9 
            };

            U8 remapped = remap[c];
            // Ignore if the color is greater than the specified max index:
            if ( remapped <= maxColorIndex )
            {
               const ColorI &clr = colorTable[remapped];
               sg_bitmapModulation = clr;
               currentColor = clr;
            }
         }
         continue;
      }

      // reset color?
      if ( c == 15 )
      {
         currentColor = sg_textAnchorColor;
         sg_bitmapModulation = sg_textAnchorColor;
         continue;
      }

      // push color:
      if ( c == 16 )
      {
         sg_stackColor = sg_bitmapModulation;
         continue;
      }

      // pop color:
      if ( c == 17 )
      {
         currentColor = sg_stackColor;
         sg_bitmapModulation = sg_stackColor;
         continue;
      }

      // Tab character
      if ( c == dT('\t') ) 
      {
          const PlatformFont::CharInfo &ci = font->getCharInfo( dT(' ') );
          pt.x += ci.xIncrement * GFont::TabWidthInSpaces;
          continue;
      }

      if( !font->isValidChar( c ) )  
         continue;

      const PlatformFont::CharInfo &ci = font->getCharInfo(c);
      if(ci.bitmapIndex == -1)
      {
         pt.x += ci.xOrigin + ci.xIncrement;
         continue;
      }

      TextureObject *newObj = font->getTextureHandle(ci.bitmapIndex);
      if(newObj != lastTexture)
      {
         flushGlyphBatch(lastTexture, sTextVertices, sTextIndices);
         lastTexture = newObj;
      }
      if(ci.width != 0 && ci.height != 0 && newObj != NULL)
      {
         if (sTextVertices.size() + 4 > 65532)
            flushGlyphBatch(lastTexture, sTextVertices, sTextIndices);

         pt.y = font->getBaseline() - ci.yOrigin;
         pt.x += ci.xOrigin;

         F32 texLeft   = F32(ci.xOffset)             / F32(lastTexture->getTextureWidth());
         F32 texRight  = F32(ci.xOffset + ci.width)  / F32(lastTexture->getTextureWidth());
         F32 texTop    = F32(ci.yOffset)             / F32(lastTexture->getTextureHeight());
         F32 texBottom = F32(ci.yOffset + ci.height) / F32(lastTexture->getTextureHeight());

         F32 screenLeft   = (F32)pt.x;
         F32 screenRight  = (F32)(pt.x + ci.width);
         F32 screenTop    = (F32)pt.y;
         F32 screenBottom = (F32)(pt.y + ci.height);

         points[0] = Point3F(screenLeft,  screenTop,    0.0f);
         points[1] = Point3F(screenRight, screenTop,    0.0f);
         points[2] = Point3F(screenLeft,  screenBottom, 0.0f);
         points[3] = Point3F(screenRight, screenBottom, 0.0f);

         for( S32 j = 0; j < 4; ++j )
         {
            rotMatrix.mulP( points[j] );
            points[j] += offset;
         }

         const ColorF cFloat(currentColor);
         const U16 base = (U16)sTextVertices.size();
         DGLVertex v;
         v.set(points[0].x, points[0].y, 0.0f, texLeft,  texTop,    cFloat.red, cFloat.green, cFloat.blue, cFloat.alpha); sTextVertices.push_back(v);
         v.set(points[1].x, points[1].y, 0.0f, texRight, texTop,    cFloat.red, cFloat.green, cFloat.blue, cFloat.alpha); sTextVertices.push_back(v);
         v.set(points[2].x, points[2].y, 0.0f, texLeft,  texBottom, cFloat.red, cFloat.green, cFloat.blue, cFloat.alpha); sTextVertices.push_back(v);
         v.set(points[3].x, points[3].y, 0.0f, texRight, texBottom, cFloat.red, cFloat.green, cFloat.blue, cFloat.alpha); sTextVertices.push_back(v);

         sTextIndices.push_back(base + 0);
         sTextIndices.push_back(base + 1);
         sTextIndices.push_back(base + 2);
         sTextIndices.push_back(base + 2);
         sTextIndices.push_back(base + 1);
         sTextIndices.push_back(base + 3);

         pt.x += ci.xIncrement - ci.xOrigin;
      }
      else
         pt.x += ci.xIncrement;
   }

   flushGlyphBatch(lastTexture, sTextVertices, sTextIndices);

   ShaderManager::setTextureMode(DGL_TEX_NONE);
   glDisable(GL_BLEND);

   pt.x += ptDraw.x; // DAW: Account for the fact that we removed the drawing point from the text start at the beginning.

   AssertFatal(pt.x >= ptDraw.x, "How did this happen?");
   PROFILE_END();

   return pt.x - ptDraw.x;
}

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=- //
// Drawing primitives

void dglDrawLine(S32 x1, S32 y1, S32 x2, S32 y2, const ColorI &color)
{
   glEnable(GL_BLEND);
   glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
   ShaderManager::setTextureMode(DGL_TEX_NONE);

   ShaderManager::begin(GL_LINES);
   ShaderManager::colorI(color);
   ShaderManager::vertex2f((F32)x1 + 0.5f, (F32)y1 + 0.5f);
   ShaderManager::vertex2f((F32)x2 + 0.5f, (F32)y2 + 0.5f);
   ShaderManager::end();
}

void dglDrawLine(const Point2I &startPt, const Point2I &endPt, const ColorI &color)
{
    dglDrawLine(startPt.x, startPt.y, endPt.x, endPt.y, color);
}

void dglDrawTriangleFill(const Point2I &pt1, const Point2I &pt2, const Point2I &pt3, const ColorI &color)
{
	glEnable(GL_BLEND);
	glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
	ShaderManager::setTextureMode(DGL_TEX_NONE);

	ShaderManager::begin(GL_TRIANGLES);
	ShaderManager::colorI(color);
	ShaderManager::vertex2f((F32)pt1.x, (F32)pt1.y);
	ShaderManager::vertex2f((F32)pt2.x, (F32)pt2.y);
	ShaderManager::vertex2f((F32)pt3.x, (F32)pt3.y);
	ShaderManager::end();
}

void dglDrawRect(const Point2I &upperL, const Point2I &lowerR, const ColorI &color, const float &lineWidth)
{
   glEnable(GL_BLEND);
   glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
   ShaderManager::setTextureMode(DGL_TEX_NONE);

   if (!gGLState.isCoreProfile || lineWidth <= 1.0f)
      glLineWidth(lineWidth);
   else
      glLineWidth(1.0f);

   ShaderManager::begin(GL_LINE_LOOP);
      ShaderManager::colorI(color);
      ShaderManager::vertex2f((F32)upperL.x + 0.5f, (F32)upperL.y + 0.5f);
      ShaderManager::vertex2f((F32)lowerR.x + 0.5f, (F32)upperL.y + 0.5f);
      ShaderManager::vertex2f((F32)lowerR.x + 0.5f, (F32)lowerR.y + 0.5f);
      ShaderManager::vertex2f((F32)upperL.x + 0.5f, (F32)lowerR.y + 0.5f);
   ShaderManager::end();
}

// the fill convention for lined rects is that they outline the rectangle border of the
// filled region specified.

void dglDrawRect(const RectI &rect, const ColorI &color, const float &lineWidth)
{
   Point2I lowerR(rect.point.x + rect.extent.x - 1, rect.point.y + rect.extent.y - 1);
   dglDrawRect(rect.point, lowerR, color, lineWidth);
}

// the fill convention says that pixel at upperL will be filled and
// that pixel at lowerR will NOT be filled.

void dglDrawRectFill(const Point2I &upperL, const Point2I &lowerR, const ColorI &color)
{
   glEnable(GL_BLEND);
   glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
   ShaderManager::setTextureMode(DGL_TEX_NONE);

   ShaderManager::begin(GL_TRIANGLE_STRIP);
      ShaderManager::colorI(color);
      ShaderManager::vertex2f((F32)upperL.x, (F32)upperL.y);
      ShaderManager::vertex2f((F32)upperL.x, (F32)lowerR.y);
      ShaderManager::vertex2f((F32)lowerR.x, (F32)upperL.y);
      ShaderManager::vertex2f((F32)lowerR.x, (F32)lowerR.y);
   ShaderManager::end();
}
void dglDrawRectFill(const RectI &rect, const ColorI &color)
{
   Point2I lowerR(rect.point.x + rect.extent.x, rect.point.y + rect.extent.y);
   dglDrawRectFill(rect.point, lowerR, color);
}

//Start in the top left and move counter-clockwise around the quad.
void dglDrawQuadFill(const Point2I &point1, const Point2I &point2, const Point2I &point3, const Point2I &point4, const ColorI &color)
{
	glEnable(GL_BLEND);
	glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
	ShaderManager::setTextureMode(DGL_TEX_NONE);

	//Points 3 and 4 are switched by design.
	ShaderManager::begin(GL_TRIANGLE_STRIP);
		ShaderManager::colorI(color);
		ShaderManager::vertex2f((F32)point1.x, (F32)point1.y);
		ShaderManager::vertex2f((F32)point2.x, (F32)point2.y);
		ShaderManager::vertex2f((F32)point4.x, (F32)point4.y);
		ShaderManager::vertex2f((F32)point3.x, (F32)point3.y);
	ShaderManager::end();
}

void dglDrawDot(const Point2F &screenPoint,const ColorI &color)
{
   glEnable(GL_BLEND);
   glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
   ShaderManager::setTextureMode(DGL_TEX_NONE);

   ShaderManager::begin(GL_POINTS);
   ShaderManager::colorI(color);
   ShaderManager::vertex2f(screenPoint.x, screenPoint.y);
   ShaderManager::end();

   glDisable(GL_BLEND);
}

void dglDraw2DSquare( const Point2F &screenPoint, F32 width, F32 spinAngle )
{
   width *= 0.5f;

   MatrixF rotMatrix( EulerF( 0.0, 0.0, spinAngle ) );

   Point3F offset( screenPoint.x, screenPoint.y, 0.0 );
   Point3F points[4];

   points[0] = Point3F(-width, -width, 0.0);
   points[1] = Point3F(-width,  width, 0.0);
   points[2] = Point3F( width,  width, 0.0);
   points[3] = Point3F( width, -width, 0.0);

   for( int i=0; i<4; i++ )
   {
      rotMatrix.mulP( points[i] );
      points[i] += offset;
   }

   ShaderManager::begin(GL_TRIANGLE_FAN);
      ShaderManager::texCoord2f(0.0f, 0.0f);
      ShaderManager::vertex2f(points[0].x, points[0].y);

      ShaderManager::texCoord2f(0.0f, 1.0f);
      ShaderManager::vertex2f(points[1].x, points[1].y);

      ShaderManager::texCoord2f(1.0f, 1.0f);
      ShaderManager::vertex2f(points[2].x, points[2].y);

      ShaderManager::texCoord2f(1.0f, 0.0f);
      ShaderManager::vertex2f(points[3].x, points[3].y);
   ShaderManager::end();
}

void dglDrawBillboard( const Point3F &position, F32 width, F32 spinAngle )
{
   MatrixF modelview;
   dglGetModelview( &modelview );
   modelview.transpose();


   width *= 0.5f;
   Point3F points[4];
   points[0] = Point3F(-width, 0.0, -width);
   points[1] = Point3F(-width, 0.0,  width);
   points[2] = Point3F( width, 0.0,  width);
   points[3] = Point3F( width, 0.0, -width);


   MatrixF rotMatrix( EulerF( 0.0, spinAngle, 0.0 ) );

   for( int i=0; i<4; i++ )
   {
      rotMatrix.mulP( points[i] );
      modelview.mulP( points[i] );
      points[i] += position;
   }

   ShaderManager::begin(GL_TRIANGLE_FAN);
      ShaderManager::texCoord2f(0.0f, 1.0f);
      ShaderManager::vertex3f(points[0].x, points[0].y, points[0].z);

      ShaderManager::texCoord2f(0.0f, 0.0f);
      ShaderManager::vertex3f(points[1].x, points[1].y, points[1].z);

      ShaderManager::texCoord2f(1.0f, 0.0f);
      ShaderManager::vertex3f(points[2].x, points[2].y, points[2].z);

      ShaderManager::texCoord2f(1.0f, 1.0f);
      ShaderManager::vertex3f(points[3].x, points[3].y, points[3].z);
   ShaderManager::end();
}

void dglWireCube(const Point3F & extent, const Point3F & center)
{
   static Point3F cubePoints[8] =
   {
      Point3F(-1, -1, -1), Point3F(-1, -1,  1), Point3F(-1,  1, -1), Point3F(-1,  1,  1),
         Point3F( 1, -1, -1), Point3F( 1, -1,  1), Point3F( 1,  1, -1), Point3F( 1,  1,  1)
   };

   static U32 cubeFaces[6][4] =
   {
      { 0, 2, 6, 4 }, { 0, 2, 3, 1 }, { 0, 1, 5, 4 },
      { 3, 2, 6, 7 }, { 7, 6, 4, 5 }, { 3, 7, 5, 1 }
   };

   glDisable(GL_CULL_FACE);
   ShaderManager::setTextureMode(DGL_TEX_NONE);

   for (S32 i = 0; i < 6; i++)
   {
      ShaderManager::begin(GL_LINE_LOOP);
      for(int vert = 0; vert < 4; vert++)
      {
         int idx = cubeFaces[i][vert];
         ShaderManager::vertex3f(cubePoints[idx].x * extent.x + center.x,
            cubePoints[idx].y * extent.y + center.y,
            cubePoints[idx].z * extent.z + center.z);
      }
      ShaderManager::end();
   }
}


void dglSolidCube(const Point3F & extent, const Point3F & center)
{
   static Point3F cubePoints[8] =
   {
      Point3F(-1, -1, -1), Point3F(-1, -1,  1), Point3F(-1,  1, -1), Point3F(-1,  1,  1),
         Point3F( 1, -1, -1), Point3F( 1, -1,  1), Point3F( 1,  1, -1), Point3F( 1,  1,  1)
   };

   static U32 cubeFaces[6][4] =
   {
      { 0, 2, 6, 4 }, { 0, 2, 3, 1 }, { 0, 1, 5, 4 },
      { 3, 2, 6, 7 }, { 7, 6, 4, 5 }, { 3, 7, 5, 1 }
   };

   ShaderManager::setTextureMode(DGL_TEX_NONE);

   for (S32 i = 0; i < 6; i++)
   {
      ShaderManager::begin(GL_TRIANGLE_FAN);
      for(int vert = 0; vert < 4; vert++)
      {
         int idx = cubeFaces[i][vert];
         ShaderManager::vertex3f(cubePoints[idx].x * extent.x + center.x,
            cubePoints[idx].y * extent.y + center.y,
            cubePoints[idx].z * extent.z + center.z);
      }
      ShaderManager::end();
   }
}

//Draws an unfilled circle with line segments.
//Circle drawing code was modified from this source with gratitude. It is in the public domain.
//http://slabode.exofire.net/circle_draw.shtml
void dglDrawCircle(const Point2I &center, const F32 radius, const ColorI &color, const F32 &lineWidth)
{
	if(lineWidth <= 0)
	{
		return;
	}

	F32 adjustedRadius = radius - (lineWidth/2);
	// See dglDrawCircleFill: a non-positive radius yields <= 0 segments, which
	// would leave verts empty and make the verts[0]/verts[1] reads below run past
	// the end of the vector. Bail rather than crash.
	if (!(adjustedRadius > 0.0f))
		return;
	const S32 num_segments = (const S32)round(10 * sqrtf(adjustedRadius));
	if (num_segments <= 0)
		return;
	F32 theta = 2 * 3.1415926f / F32(num_segments);
	F32 c = cosf(theta);//precalculate the sine and cosine
	F32 s = sinf(theta);
	F32 t;

	F32 x = adjustedRadius;//we start at angle = 0 
	F32 y = 0;

	glEnable(GL_BLEND);
	glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
	ShaderManager::setTextureMode(DGL_TEX_NONE);

	if (!gGLState.isCoreProfile || lineWidth <= 1.0f)
		glLineWidth(lineWidth);
	else
		glLineWidth(1.0f);

	const ColorF cFloat(color);
	vector<DGLVertex> verts;
	verts.reserve(num_segments + 1);
	for (int ii = 0; ii < num_segments; ii++)
	{
		DGLVertex v;
		v.set(x + center.x, y + center.y, 0.0f, 0.0f, 0.0f, cFloat.red, cFloat.green, cFloat.blue, cFloat.alpha);
		verts.push_back(v);

		//apply the rotation matrix
		t = x;
		x = c * x - s * y;
		y = s * t + c * y;
	}
	verts.push_back(verts[0]);

	ShaderManager::drawVertices(GL_LINE_LOOP, verts.data(), (U32)verts.size());
}

void dglDrawCircleFill(const Point2I &center, const F32 radius, const ColorI &color)
{
	// A non-positive radius (e.g. a control squeezed to nothing by a padded
	// border) makes sqrtf() return 0 or NaN, so num_segments is <= 0. The vertex
	// loop below is then skipped, leaving verts holding only the center point --
	// and the verts[2]/verts[3] reads that follow run off the end of the vector.
	// Bail instead of crashing so no caller can be brought down by a bad radius.
	if (!(radius > 0.0f))
		return;
	const S32 num_segments = (const S32)round(10 * sqrtf(radius));
	if (num_segments <= 0)
		return;
	F32 theta = 2 * 3.1415926f / F32(num_segments);
	F32 c = cosf(theta);//precalculate the sine and cosine
	F32 s = sinf(theta);
	F32 t;

	F32 x = radius;//we start at angle = 0 
	F32 y = 0;

	glEnable(GL_BLEND);
	glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
	ShaderManager::setTextureMode(DGL_TEX_NONE);

	const ColorF cFloat(color);
	vector<DGLVertex> verts;
	verts.reserve(num_segments + 2);
	DGLVertex centerVert;
	centerVert.set((F32)center.x, (F32)center.y, 0.0f, 0.0f, 0.0f, cFloat.red, cFloat.green, cFloat.blue, cFloat.alpha);
	verts.push_back(centerVert);
	for (int ii = 0; ii < num_segments; ii++)
	{
		DGLVertex v;
		v.set(x + center.x, y + center.y, 0.0f, 0.0f, 0.0f, cFloat.red, cFloat.green, cFloat.blue, cFloat.alpha);
		verts.push_back(v);

		//apply the rotation matrix
		t = x;
		x = c * x - s * y;
		y = s * t + c * y;
	}
	verts.push_back(verts[1]);

	ShaderManager::drawVertices(GL_TRIANGLE_FAN, verts.data(), (U32)verts.size());
}

static RectI sgCurrentViewport(0, 0, 1, 1);
static F64 sgLeftFrustum   = -0.5;
static F64 sgRightFrustum  =  0.5;
static F64 sgBottomFrustum = -0.5;
static F64 sgTopFrustum    =  0.5;
static F64 sgNearDist      =  1.0;
static F64 sgFarDist       =  1000.0;
static bool sgIsOrtho      =  true;
static F32 sgPixelScale    =  1.0f;
static F32 sgWorldToScreenScale = 1.0f;

F32 dglGetPixelScale()
{
   return sgPixelScale;
}

F32 dglGetWorldToScreenScale()
{
   return sgWorldToScreenScale;
}

F32 dglProjectRadius(F32 dist, F32 radius)
{
   return (dist != 0.0f) ? ((radius / dist) * sgWorldToScreenScale) : 0.0f;
}

void dglSetViewport(const RectI &aViewPort)
{
   U32 screenHeight = Platform::getWindowSize().y;
   glViewport(aViewPort.point.x,
              screenHeight - (aViewPort.point.y + aViewPort.extent.y),
              aViewPort.extent.x,
              aViewPort.extent.y);

   sgCurrentViewport = aViewPort;
   sgPixelScale = aViewPort.extent.x / 640.0f;
   F64 frustumWidth = sgRightFrustum - sgLeftFrustum;
   sgWorldToScreenScale = (mFabs((F32)frustumWidth) > 1e-6f)
      ? (F32)((sgNearDist * aViewPort.extent.x) / frustumWidth)
      : 1.0f;
}

void dglGetViewport(RectI* outViewport)
{
   AssertFatal(outViewport != NULL, "Error, bad point in GetViewport");
   *outViewport = sgCurrentViewport;
}

void dglSetFrustum(F64 left, F64 right, F64 bottom, F64 top, F64 nearDist, F64 farDist, bool ortho)
{
   sgLeftFrustum   = left;
   sgRightFrustum  = right;
   sgBottomFrustum = bottom;
   sgTopFrustum    = top;
   sgNearDist      = nearDist;
   sgFarDist       = farDist;
   sgIsOrtho       = ortho;

   if (ortho)
      dglOrtho(left, right, bottom, top, nearDist, farDist);
   else
      dglFrustum(left, right, bottom, top, nearDist, farDist);
}

void dglGetFrustum(F64 *left, F64 *right, F64 *bottom, F64 *top, F64 *nearDist, F64 *farDist)
{
   if (left)     *left     = sgLeftFrustum;
   if (right)    *right    = sgRightFrustum;
   if (bottom)   *bottom   = sgBottomFrustum;
   if (top)      *top      = sgTopFrustum;
   if (nearDist) *nearDist = sgNearDist;
   if (farDist)  *farDist  = sgFarDist;
}

bool dglIsOrtho()
{
   return sgIsOrtho;
}

void dglSetClipRect(const RectI &clipRect)
{
   dglMatrixMode(GL_PROJECTION);
   dglLoadIdentity();

   dglSetFrustum(clipRect.point.x, clipRect.point.x + clipRect.extent.x,
                 clipRect.extent.y, 0,
                 0, 1, true);

   dglTranslatef(0.0f, (F32)-clipRect.point.y, 0.0f);

   dglMatrixMode(GL_MODELVIEW);
   dglLoadIdentity();

   dglSetViewport(clipRect);

   sgCurrentClipRect = clipRect;
}

const RectI& dglGetClipRect()
{
   return sgCurrentClipRect;
}

bool dglPointToScreen( Point3F &point3D, Point3F &screenPoint )
{
   F32 mvCol[16];
   F32 prCol[16];
   dglGetModelviewColumnMajor(mvCol);
   dglGetProjectionColumnMajor(prCol);

   RectI viewport;
   dglGetViewport(&viewport);

   GLint glVP[4];
   glVP[0] = viewport.point.x;
   glVP[1] = viewport.point.y + viewport.extent.y;
   glVP[2] = viewport.extent.x;
   glVP[3] = -viewport.extent.y;

   F32 eye[4];
   F32 clip[4];
   for (S32 i = 0; i < 4; ++i)
      eye[i] = mvCol[i] * point3D.x + mvCol[4 + i] * point3D.y + mvCol[8 + i] * point3D.z + mvCol[12 + i];
   for (S32 i = 0; i < 4; ++i)
      clip[i] = prCol[i] * eye[0] + prCol[4 + i] * eye[1] + prCol[8 + i] * eye[2] + prCol[12 + i] * eye[3];

   if (mIsZero(clip[3]))
      return false;

   const F32 invW = 1.0f / clip[3];
   const F32 ndcX = clip[0] * invW;
   const F32 ndcY = clip[1] * invW;
   const F32 ndcZ = clip[2] * invW;

   screenPoint.x = (F32)glVP[0] + (F32)glVP[2] * (ndcX + 1.0f) * 0.5f;
   screenPoint.y = (F32)glVP[1] + (F32)glVP[3] * (ndcY + 1.0f) * 0.5f;
   screenPoint.z = (ndcZ + 1.0f) * 0.5f;

   return true;
}



bool dglIsInCanonicalState()
{
   bool ret = true;

   // Canonical state:
   //  BLEND disabled
   //  cullface : disabled
   //  ShaderManager tex mode NONE & alpha test off

   ret &= glIsEnabled(GL_BLEND) == GL_FALSE;
   ret &= glIsEnabled(GL_CULL_FACE) == GL_FALSE;
   ret &= ShaderManager::getTextureMode() == DGL_TEX_NONE;
   ret &= ShaderManager::getAlphaThreshold() < 0.0f;

   return ret;
}


void dglSetCanonicalState()
{
   glDisable(GL_BLEND);
   glDisable(GL_CULL_FACE);
   glBlendFunc(GL_ONE, GL_ZERO);
   glActiveTexture(GL_TEXTURE0);
   ShaderManager::setTextureMode(DGL_TEX_NONE);
   ShaderManager::setAlphaThreshold(-1.0f);
   ShaderManager::clearClipPlanes();
}

void dglGetTransformState(S32* mvDepth,
                          S32* pDepth,
                          S32* t0Depth,
                          F32* t0Matrix,
                          S32* t1Depth,
                          F32* t1Matrix,
                          S32* vp)
{
   static const F32 sIdentity[16] = {
      1.0f, 0.0f, 0.0f, 0.0f,
      0.0f, 1.0f, 0.0f, 0.0f,
      0.0f, 0.0f, 1.0f, 0.0f,
      0.0f, 0.0f, 0.0f, 1.0f
   };

   *mvDepth = dglGetModelviewStackDepth();
   *pDepth  = dglGetProjectionStackDepth();
   *t0Depth = dglGetTextureStackDepth();
   dMemcpy(t0Matrix, sIdentity, sizeof(sIdentity));
   *t1Depth = 0;
   for (U32 i = 0; i < 16; i++)
      t1Matrix[i] = 0.0f;

   RectI v;
   dglGetViewport(&v);
   vp[0] = v.point.x;
   vp[1] = v.point.y;
   vp[2] = v.extent.x;
   vp[3] = v.extent.y;
}


bool dglCheckState(const S32 mvDepth, const S32 pDepth,
                   const S32 t0Depth, const F32* t0Matrix,
                   const S32 t1Depth, const F32* t1Matrix,
                   const S32* vp)
{
   static const F32 sIdentity[16] = {
      1.0f, 0.0f, 0.0f, 0.0f,
      0.0f, 1.0f, 0.0f, 0.0f,
      0.0f, 0.0f, 1.0f, 0.0f,
      0.0f, 0.0f, 0.0f, 1.0f
   };

   const S32 md = dglGetModelviewStackDepth();
   const S32 pd = dglGetProjectionStackDepth();
   const S32 t0d = dglGetTextureStackDepth();
   const S32 t1d = 0;
   F32 t1m[16];
   for (U32 i = 0; i < 16; i++)
      t1m[i] = 0.0f;

   RectI v;
   dglGetViewport(&v);

   return ((md == mvDepth) &&
           (pd == pDepth) &&
           (t0d == t0Depth) &&
           (dMemcmp(sIdentity, t0Matrix, sizeof(F32) * 16) == 0) &&
           (t1d == t1Depth) &&
           (dMemcmp(t1m, t1Matrix, sizeof(F32) * 16) == 0) &&
           ((v.point.x  == vp[0]) &&
            (v.point.y  == vp[1]) &&
            (v.extent.x == vp[2]) &&
            (v.extent.y == vp[3])));
}

#if defined(TORQUE_OS_IOS) || defined(TORQUE_OS_ANDROID) || defined(TORQUE_OS_EMSCRIPTEN)
GLfloat gVertexFloats[8];
GLfloat gTextureVerts[8];
#endif

//--------------------------------------------------------------------------
// Function to draw a box which can have 4 different colors in each corner blended together
void dglDrawBlendBox(const RectI& bounds, ColorF& c1, ColorF& c2, ColorF& c3, ColorF& c4)
{
	F32 l = (F32)bounds.point.x;
	F32 r = (F32)(bounds.point.x + bounds.extent.x);
	F32 t = (F32)bounds.point.y;
	F32 b = (F32)(bounds.point.y + bounds.extent.y);

	glEnable(GL_BLEND);
	glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
	ShaderManager::setTextureMode(DGL_TEX_NONE);

	ShaderManager::begin(GL_QUADS);
		ShaderManager::colorF(c1);
		ShaderManager::vertex2f(l, t);
		ShaderManager::colorF(c4);
		ShaderManager::vertex2f(l, b);
		ShaderManager::colorF(c3);
		ShaderManager::vertex2f(r, b);
		ShaderManager::colorF(c2);
		ShaderManager::vertex2f(r, t);
	ShaderManager::end();
}

//--------------------------------------------------------------------------
/// Function to draw a set of boxes blending throughout an array of colors
void dglDrawBlendRangeBox(const RectI& bounds, bool vertical, U8 numColors, ColorI* colors)
{
	F32 l = (F32)bounds.point.x;
	F32 r = (F32)(bounds.point.x + bounds.extent.x);
	F32 t = (F32)bounds.point.y;
	F32 b = (F32)(bounds.point.y + bounds.extent.y);

	// Calculate increment value
	F32 x_inc = F32((r - l) / (numColors - 1));
	F32 y_inc = F32((b - t) / (numColors - 1));

	glEnable(GL_BLEND);
	glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
	ShaderManager::setTextureMode(DGL_TEX_NONE);

	ShaderManager::begin(GL_QUADS);
	if (!vertical)  // Horizontal (+x)
	{
		ShaderManager::colorI(colors[0]);
		ShaderManager::vertex2f(l, t);
		ShaderManager::vertex2f(l, b);
		ShaderManager::vertex2f(l - 1, b);
		ShaderManager::vertex2f(l - 1, t);
		ShaderManager::vertex2f(r, t);
		ShaderManager::vertex2f(r, b);
		ShaderManager::vertex2f(r + 1, b);
		ShaderManager::vertex2f(r + 1, t);
	}
	else   // Vertical (+y)
	{
		ShaderManager::colorI(colors[0]);
		ShaderManager::vertex2f(l, t);
		ShaderManager::vertex2f(r, t);
		ShaderManager::vertex2f(r, t - 1);
		ShaderManager::vertex2f(l, t - 1);
		ShaderManager::vertex2f(l, b);
		ShaderManager::vertex2f(r, b);
		ShaderManager::vertex2f(r, b + 1);
		ShaderManager::vertex2f(l, b + 1);
	}

	for (U16 i = 0; i < numColors - 1; i++)
	{
		if (!vertical)  // Horizontal (+x)
		{
			// First color
			ShaderManager::colorI(colors[i]);
			ShaderManager::vertex2f(l, t);
			ShaderManager::vertex2f(l, b);
			// Second color
			ShaderManager::colorI(colors[i + 1]);
			ShaderManager::vertex2f(l + x_inc, b);
			ShaderManager::vertex2f(l + x_inc, t);
			l += x_inc;
		}
		else  // Vertical (+y)
		{
			// First color
			ShaderManager::colorI(colors[i]);
			ShaderManager::vertex2f(l, t);
			ShaderManager::vertex2f(r, t);
			// Second color
			ShaderManager::colorI(colors[i + 1]);
			ShaderManager::vertex2f(r, t + y_inc);
			ShaderManager::vertex2f(l, t + y_inc);
			t += y_inc;
		}
	}
	ShaderManager::end();
}

void dglRenderCheckers(const RectI& bounds, const U8 size, const ColorF& c1, const ColorF& c2)
{
	F32 l = (F32)bounds.point.x;
	F32 r = (F32)(bounds.point.x + bounds.extent.x);
	F32 t = (F32)bounds.point.y;
	F32 b = (F32)(bounds.point.y + bounds.extent.y);

	glDisable(GL_BLEND);
	ShaderManager::setTextureMode(DGL_TEX_NONE);

	static Vector<DGLVertex> sCheckerVerts(__FILE__, __LINE__);
	sCheckerVerts.clear();

	DGLVertex v;
	v.set(l, t, 0.0f, 0.0f, 0.0f, c2.red, c2.green, c2.blue, c2.alpha); sCheckerVerts.push_back(v);
	v.set(l, b, 0.0f, 0.0f, 0.0f, c2.red, c2.green, c2.blue, c2.alpha); sCheckerVerts.push_back(v);
	v.set(r, b, 0.0f, 0.0f, 0.0f, c2.red, c2.green, c2.blue, c2.alpha); sCheckerVerts.push_back(v);
	v.set(r, t, 0.0f, 0.0f, 0.0f, c2.red, c2.green, c2.blue, c2.alpha); sCheckerVerts.push_back(v);

	for (U32 y = 0; y < (U32)bounds.extent.y; y += size)
	{
		for (U32 x = ((y / size) % 2 == 0 ? 0 : size); x < (U32)bounds.extent.x; x += (size * 2))
		{
			const F32 x0 = l + x;
			const F32 y0 = t + y;
			const F32 x1 = getMin(r, l + x + size);
			const F32 y1 = getMin(b, t + y + size);
			v.set(x0, y0, 0.0f, 0.0f, 0.0f, c1.red, c1.green, c1.blue, c1.alpha); sCheckerVerts.push_back(v);
			v.set(x0, y1, 0.0f, 0.0f, 0.0f, c1.red, c1.green, c1.blue, c1.alpha); sCheckerVerts.push_back(v);
			v.set(x1, y1, 0.0f, 0.0f, 0.0f, c1.red, c1.green, c1.blue, c1.alpha); sCheckerVerts.push_back(v);
			v.set(x1, y0, 0.0f, 0.0f, 0.0f, c1.red, c1.green, c1.blue, c1.alpha); sCheckerVerts.push_back(v);
		}
	}

	ShaderManager::drawVertices(GL_QUADS, sCheckerVerts.address(), sCheckerVerts.size());
}
