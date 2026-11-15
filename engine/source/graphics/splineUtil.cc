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

#include "graphics/splineUtil.h"
#include "graphics/ShaderManager.h"
#include "platform/platformGL.h"

namespace SplineUtil{


//------------------------------------------------------------------------------
// Draws strip of specified width along spline.  Polys on strip segments are
//    front-facing (billboarded)
//------------------------------------------------------------------------------
void drawSplineBeam( const Point3F& camPos, U32 numSegments,
                     F32 width, SplinePatch &spline, F32 uvOffset, F32 numTexRep )
{

   Point3F beginPoint, endPoint;
   spline.calc( 0.0, beginPoint );
   spline.calc( 1.0, endPoint );

   F32 approxBeamLength = (beginPoint - endPoint).len();
   F32 texRepFactor = approxBeamLength * numTexRep;


   ShaderManager::begin(GL_TRIANGLE_STRIP);
   ShaderManager::color4f(1.0f, 1.0f, 1.0f, 1.0f);


   for( U32 i=0; i<numSegments; i++ )
   {
      F32 t = ((F32)i) / ((F32)(numSegments - 1));

      Point3F curPoint;
      spline.calc( t, curPoint );

      Point3F segmentDir;

      // handle last segment case
      Point3F nextPoint;
      if( i == (numSegments - 1) )
      {
         F32 modT = ((F32)(numSegments - 1)) / ((F32)numSegments);
         spline.calc( modT, nextPoint );
         segmentDir = curPoint - nextPoint;
      }
      else
      {
         F32 modT = t + (1.0f / numSegments);
         spline.calc( modT, nextPoint );
         segmentDir = nextPoint - curPoint;
      }

      if( segmentDir.isZero() ) continue;
      segmentDir.normalize();


      Point3F dirFromCam = curPoint - camPos;
      Point3F crossVec;
      mCross(dirFromCam, segmentDir, &crossVec);
      crossVec.normalize();

      crossVec *= width * 0.5f;

      F32 u = uvOffset + texRepFactor * t;

      const Point3F p0 = curPoint + crossVec;
      const Point3F p1 = curPoint - crossVec;

      ShaderManager::texCoord2f( u, 0.0f );
      ShaderManager::vertex3f( p0.x, p0.y, p0.z );

      ShaderManager::texCoord2f( u, 1.0f );
      ShaderManager::vertex3f( p1.x, p1.y, p1.z );

   }

   ShaderManager::end();


}

//------------------------------------------------------------------------------
// Draws strip of specified width along spline.  Polys on strip segments are
//    front-facing (billboarded)
//------------------------------------------------------------------------------
void drawSplineBeam( SplineBeamInfo &sbi )
{
   if( !sbi.camPos || !sbi.spline ) return;

   Point3F beginPoint, endPoint;
   sbi.spline->calc( 0.0, beginPoint );
   sbi.spline->calc( 1.0, endPoint );

   F32 approxBeamLength = (beginPoint - endPoint).len();
   F32 texRepFactor = approxBeamLength * sbi.numTexRep;


   ShaderManager::begin(GL_TRIANGLE_STRIP);


   for( U32 i=0; i<sbi.numSegments; i++ )
   {
      F32 t = ((F32)i) / ((F32)(sbi.numSegments - 1));

      Point3F curPoint;
      sbi.spline->calc( t, curPoint );

      Point3F segmentDir;

      // handle last segment case
      Point3F nextPoint;
      if( i == (sbi.numSegments - 1) )
      {
         F32 modT = ((F32)(sbi.numSegments - 1)) / ((F32)sbi.numSegments);
         sbi.spline->calc( modT, nextPoint );
         segmentDir = curPoint - nextPoint;
      }
      else
      {
         F32 modT = t + (1.0f / sbi.numSegments);
         sbi.spline->calc( modT, nextPoint );
         segmentDir = nextPoint - curPoint;
      }

      if( segmentDir.isZero() ) continue;
      segmentDir.normalize();


      Point3F dirFromCam = curPoint - *sbi.camPos;
      Point3F crossVec;
      mCross(dirFromCam, segmentDir, &crossVec);
      crossVec.normalize();

      crossVec *= sbi.width * 0.5f;

      F32 u = sbi.uvOffset + texRepFactor * t;

      if( i== 0 && sbi.zeroAlphaStart )
      {
         ShaderManager::color4f( sbi.color.red, sbi.color.green, sbi.color.blue, 0.0f );
      }
      else
      {
         ShaderManager::colorF( sbi.color );
      }

      const Point3F p0 = curPoint + crossVec;
      const Point3F p1 = curPoint - crossVec;

      ShaderManager::texCoord2f( u, 0.0 );
      ShaderManager::vertex3f( p0.x, p0.y, p0.z );

      ShaderManager::texCoord2f( u, 1.0 );
      ShaderManager::vertex3f( p1.x, p1.y, p1.z );

   }

   ShaderManager::end();

}





} // end SplineUtil namespace
