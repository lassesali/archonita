//-----------------------------------------------------------------------------
// Copyright (c) 2013 GarageGames, LLC
// Portions Copyright (c) 2014 James S Urquhart
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

// Many OSX frameworks include OpenTransport, and OT's new operator conflicts 
// with our redefinition of 'new', so we have to pre-include platformAndroid.h,
// which contains the workaround.
#include "platformEmscripten/platformEmscripten.h"
#include "platformEmscripten/platformGL.h"

#include "console/console.h"
#include "graphics/dgl.h"

#define TEST_FOR_OPENGL_ERRORS 0;

GLState gGLState;

bool gOpenGLDisablePT          = false;
bool gOpenGLDisableCVA         = false;
bool gOpenGLDisableTEC         = false;
bool gOpenGLDisableARBMT       = false;
bool gOpenGLDisableFC          = true; //false;
bool gOpenGLDisableTCompress   = false;
bool gOpenGLNoEnvColor         = false;
float gOpenGLGammaCorrection   = 0.5;
bool gOpenGLNoDrawArraysAlpha  = false;

// Find out which extensions are available for this renderer. 
static bool hasGLExtension(const char* pExtString, const char* extName)
{
   if (gGLState.glMajorVersion >= 3)
   {
      GLint numExt = 0;
      glGetIntegerv(GL_NUM_EXTENSIONS, &numExt);
      for (GLint i = 0; i < numExt; ++i)
      {
         const char* ext = (const char*) glGetStringi(GL_EXTENSIONS, (GLuint)i);
         if (ext && dStrcmp(ext, extName) == 0)
            return true;
      }
      return false;
   }
   return (pExtString && dStrstr(pExtString, extName) != NULL);
}

void getGLCapabilities( )
{
   // (No per-monitor GL context to guard on here: the web build renders into a
   // single WebGL canvas. The macOS-derived platState.engine check was stale —
   // EmscriptenPlatState has no such member — so it's dropped.)

   // Get the OpenGL info strings we'll need
   const char* pVendString    = (const char*) glGetString( GL_VENDOR );
   const char* pRendString    = (const char*) glGetString( GL_RENDERER );
   const char* pVersString    = (const char*) glGetString( GL_VERSION );

   // Output some driver info to the console:
   Con::printf( "OpenGL driver information:" );
   if ( pVendString )
      Con::printf( "  Vendor: %s", pVendString );
   if ( pRendString )
      Con::printf( "  Renderer: %s", pRendString );
   if ( pVersString )
      Con::printf( "  Version: %s", pVersString );

   // pre-clear the structure
   dMemset(&gGLState, 0, sizeof(gGLState));

   gGLState.suppDSA = false;
   gGLState.isCoreProfile = false;
   gGLState.glMajorVersion = 3;
   gGLState.glMinorVersion = 0;
   if (pVersString && dStrstr(pVersString, "OpenGL ES 2") != NULL && dStrstr(pVersString, "OpenGL ES 3") == NULL)
   {
      gGLState.glMajorVersion = 2;
      gGLState.glMinorVersion = 0;
   }
   else
   {
      GLint maj = 0;
      GLint min = 0;
      glGetIntegerv(GL_MAJOR_VERSION, &maj);
      glGetIntegerv(GL_MINOR_VERSION, &min);
      if (glGetError() == GL_NO_ERROR && maj >= 3)
      {
         gGLState.glMajorVersion = maj;
         gGLState.glMinorVersion = min;
      }
   }
   gGLState.suppTexStorage = (gGLState.glMajorVersion >= 3);

   // In WebGL 2 / OpenGL ES 3.0, glGetString(GL_EXTENSIONS) is invalid; query via glGetStringi instead.
   const char* pExtString = (gGLState.glMajorVersion < 3)
      ? (const char*) glGetString( GL_EXTENSIONS )
      : NULL;

   // EXT_paletted_texture ========================================
   if (hasGLExtension(pExtString, "GL_EXT_paletted_texture"))
      gGLState.suppPalettedTexture = true;

   // EXT_compiled_vertex_array ========================================
   gGLState.suppLockedArrays = false;
   if (hasGLExtension(pExtString, "GL_EXT_compiled_vertex_array"))
      gGLState.suppLockedArrays = true;

   // ARB_multitexture ========================================
   if (hasGLExtension(pExtString, "GL_ARB_multitexture"))
      gGLState.suppARBMultitexture = true;

   // EXT_blend_color
   if (hasGLExtension(pExtString, "GL_EXT_blend_color"))
      gGLState.suppEXTblendcolor = true;

   // EXT_blend_minmax
   if (hasGLExtension(pExtString, "GL_EXT_blend_minmax"))
      gGLState.suppEXTblendminmax = true;

   // NV_vertex_array_range ========================================
   if (hasGLExtension(pExtString, "GL_NV_vertex_array_range"))
      gGLState.suppVertexArrayRange = true;

   // EXT_fog_coord ========================================
   if (hasGLExtension(pExtString, "GL_EXT_fog_coord"))
      gGLState.suppFogCoord = true;

   // ARB_texture_compression ========================================
   if ((gGLState.glMajorVersion >= 3) || hasGLExtension(pExtString, "GL_ARB_texture_compression"))
      gGLState.suppTextureCompression = true;

   // 3DFX_texture_compression_FXT1 ========================================
   if (hasGLExtension(pExtString, "GL_3DFX_texture_compression_FXT1"))
      gGLState.suppFXT1 = true;

   // EXT_texture_compression_S3TC ========================================
   if (hasGLExtension(pExtString, "GL_EXT_texture_compression_s3tc") ||
       hasGLExtension(pExtString, "GL_WEBGL_compressed_texture_s3tc"))
      gGLState.suppS3TC = true;

   // EXT_vertex_buffer ========================================
   if (hasGLExtension(pExtString, "GL_EXT_vertex_buffer"))
      gGLState.suppVertexBuffer = true;

   // Anisotropic filtering ========================================
   gGLState.suppTexAnisotropic = hasGLExtension(pExtString, "GL_EXT_texture_filter_anisotropic");
   if (gGLState.suppTexAnisotropic)
      glGetFloatv(GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, &gGLState.maxAnisotropy);

   // Binary states, i.e., no supporting functions ========================================
   gGLState.suppPackedPixels       = (gGLState.glMajorVersion >= 2)
                                  || hasGLExtension(pExtString, "GL_EXT_packed_pixels")
                                  || hasGLExtension(pExtString, "GL_APPLE_packed_pixel");

   gGLState.suppTextureEnvCombine  = hasGLExtension(pExtString, "GL_EXT_texture_env_combine")
                                  || hasGLExtension(pExtString, "GL_ARB_texture_env_combine");

   gGLState.suppEdgeClamp          = true;

   gGLState.suppTexEnvAdd          = hasGLExtension(pExtString, "GL_ARB_texture_env_add")
                                  || hasGLExtension(pExtString, "GL_EXT_texture_env_add");
   
   // Texture combine units  ========================================
//   if (gGLState.suppARBMultitexture)
//      glGetIntegerv(GL_MAX_TEXTURE_UNITS_ARB, &gGLState.maxTextureUnits);
	  gGLState.suppARBMultitexture = false;
      gGLState.maxTextureUnits = 1;

   // Swap interval ========================================
   // None to set on the web: vertical sync is the main loop's timing there,
   // which cannot be set until the loop starts (see
   // OpenGLDevice::setVerticalSync, EmscriptenOGLVideo.cpp).
   gGLState.suppSwapInterval = false;

   // FSAA support, TODO: check for ARB multisample support
   // multisample support should be checked via CGL
   gGLState.maxFSAASamples = 4;

   // dump found extensions to the console... 
   Con::printf("OpenGL Init: Enabled Extensions");
   if (gGLState.suppARBMultitexture)    Con::printf("  ARB_multitexture (Max Texture Units: %d)", gGLState.maxTextureUnits);
   if (gGLState.suppEXTblendcolor)      Con::printf("  EXT_blend_color");
   if (gGLState.suppEXTblendminmax)     Con::printf("  EXT_blend_minmax");
   if (gGLState.suppPalettedTexture)    Con::printf("  EXT_paletted_texture");
   if (gGLState.suppLockedArrays)       Con::printf("  EXT_compiled_vertex_array");
   if (gGLState.suppVertexArrayRange)   Con::printf("  NV_vertex_array_range");
   if (gGLState.suppTextureEnvCombine)  Con::printf("  EXT_texture_env_combine");
   if (gGLState.suppPackedPixels)       Con::printf("  EXT_packed_pixels");
   if (gGLState.suppFogCoord)           Con::printf("  EXT_fog_coord");
   if (gGLState.suppTextureCompression) Con::printf("  ARB_texture_compression");
   if (gGLState.suppS3TC)               Con::printf("  EXT_texture_compression_s3tc");
   if (gGLState.suppFXT1)               Con::printf("  3DFX_texture_compression_FXT1");
   if (gGLState.suppTexEnvAdd)          Con::printf("  (ARB|EXT)_texture_env_add");
   if (gGLState.suppTexAnisotropic)     Con::printf("  EXT_texture_filter_anisotropic (Max anisotropy: %f)", gGLState.maxAnisotropy);
   if (gGLState.suppSwapInterval)       Con::printf("  Vertical Sync");
   if (gGLState.maxFSAASamples)         Con::printf("  ATI_FSAA");

   Con::warnf("OpenGL Init: Disabled Extensions");
   if (!gGLState.suppARBMultitexture)    Con::warnf("  ARB_multitexture");
   if (!gGLState.suppEXTblendcolor)      Con::warnf("  EXT_blend_color");
   if (!gGLState.suppEXTblendminmax)     Con::warnf("  EXT_blend_minmax");
   if (!gGLState.suppPalettedTexture)    Con::warnf("  EXT_paletted_texture");
   if (!gGLState.suppLockedArrays)       Con::warnf("  EXT_compiled_vertex_array");
   if (!gGLState.suppVertexArrayRange)   Con::warnf("  NV_vertex_array_range");
   if (!gGLState.suppTextureEnvCombine)  Con::warnf("  EXT_texture_env_combine");
   if (!gGLState.suppPackedPixels)       Con::warnf("  EXT_packed_pixels");
   if (!gGLState.suppFogCoord)           Con::warnf("  EXT_fog_coord");
   if (!gGLState.suppTextureCompression) Con::warnf("  ARB_texture_compression");
   if (!gGLState.suppS3TC)               Con::warnf("  EXT_texture_compression_s3tc");
   if (!gGLState.suppFXT1)               Con::warnf("  3DFX_texture_compression_FXT1");
   if (!gGLState.suppTexEnvAdd)          Con::warnf("  (ARB|EXT)_texture_env_add");
   if (!gGLState.suppTexAnisotropic)     Con::warnf("  EXT_texture_filter_anisotropic");
   if (!gGLState.suppSwapInterval)       Con::warnf("  Vertical Sync");
   if (!gGLState.maxFSAASamples)         Con::warnf("  ATI_FSAA");
   Con::printf("");

   // Set some console variables:
   Con::setBoolVariable( "$FogCoordSupported", gGLState.suppFogCoord );
   Con::setBoolVariable( "$TextureCompressionSupported", gGLState.suppTextureCompression );
   Con::setBoolVariable( "$AnisotropySupported", gGLState.suppTexAnisotropic );
   Con::setBoolVariable( "$PalettedTextureSupported", gGLState.suppPalettedTexture );
   Con::setBoolVariable( "$SwapIntervalSupported", gGLState.suppSwapInterval );

   if (!gGLState.suppPalettedTexture && Con::getBoolVariable("$pref::OpenGL::forcePalettedTexture",false))
   {
      Con::setBoolVariable("$pref::OpenGL::forcePalettedTexture", false);
      Con::setBoolVariable("$pref::OpenGL::force16BitTexture", true);
   }

   // get fsaa samples. default to normal, no antialiasing
   // TODO: clamp this against ARB_multisample capabilities.
   gFSAASamples = Con::getIntVariable("$pref::OpenGL::numFSAASamples", 1);

}

void dglSetFSAASamples(GLint aasamp)
{
    if (gGLState.maxFSAASamples < 2)
        return;
    
    // fix out of range values
    if (aasamp < 1)
        aasamp = 1;
    else if (aasamp == 3)
        aasamp = 2;
    else if (aasamp > 4)
        aasamp = 4;
    
    /* 
    THIS IS CARBON
     aglSetInteger(platState.ctx, ATI_FSAA_LEVEL, &aasamp);
    
    IT NEEDS OT BE REPLACED WITH SOMETHING LIKE THIS:
     const char *glRenderer = (const char *) glGetString(GL_RENDERER);
     
     if (strstr(glRenderer, "ATI"))
     {
        NSOpenGLContext* testContext; /// <<< Need to get ahold of the current context
       [testContext setValues:&aasamp forParameter:ATI_FSAA_LEVEL];
     }
     */
    
    Con::printf(">>Number of FSAA samples is now [%d].", aasamp);
    Con::setIntVariable("$pref::OpenGL::numFSAASamples", aasamp);
}

#ifdef DUMMY_GL

// declare stub functions
#define GL_FUNCTION(fn_return, fn_name, fn_args, fn_value) fn_return fn_name fn_args{ printf(" fn_name \n"); fn_value }
#include "platform/GLCoreFunc.h"
#include "platform/GLExtFunc.h"
#undef GL_FUNCTION

void gluOrtho2D( GLfloat left, GLfloat right, GLfloat bottom, GLfloat top ) {}
GLint gluProject( GLdouble objx, GLdouble objy, GLdouble objz, const F64 *model, const F64 * proj, const GLint * vp, F64 * winx, F64 * winy, F64 * winz ) { return 0; }
GLint gluUnProject( GLdouble winx, GLdouble winy, GLdouble winz, const F64 *model, const F64 * proj, const GLint * vp, F64 * x, F64 * y, F64 * z ) { return 0; }

#endif
