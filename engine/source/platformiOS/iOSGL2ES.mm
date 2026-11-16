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

//
//  iOSGL2ES.mm
//  TGE
//
//  Created by Puap Puap on 7/31/08.
//  Copyright 2008 Pick Up And Play. All rights reserved.
//

#import "iOSGL2ES.h"
#import "platform/platformAssert.h"
#import "iOSOGLVideo.h"
#include "graphics/dgl.h"
#include "graphics/ShaderManager.h"

#include "math/mMatrix.h"
#include "math/mPoint.h"


#define	INV_255		0.0039215686f
#define	INV_32767	3.0518509476e-5f

/* EAGL and GL functions calling wrappers that log on error */
#define CALL_EAGL_FUNCTION(__FUNC__, ...) ({ EAGLError __error = __FUNC__( __VA_ARGS__ ); if(__error != kEAGLErrorSuccess) Con::printf("%s() called from %s returned error %i\n", #__FUNC__, __FUNCTION__, __error); (__error ? 0 : 1); })
#define CHECK_GL_ERROR() ({ int __error = glGetError(); if(__error) Con::printf("OpenGL error 0x%04X in %s\n", __error, __FUNCTION__); (__error ? 0 : 1); })
// define this to print out glErrors, un-define to get rid of it
#define TEST_FOR_OPENGL_ERRORS CHECK_GL_ERROR();

    
/////////////////////////////////////////////////////////////////////////////
//
//    Matrix stack compatibility wrappers delegated to dgl CPU matrix stacks
//
extern "C"
{
#undef glPushMatrix
void iPhoneGLPushMatrix()
{
    dglPushMatrix();
}
#define glPushMatrix iPhoneGLPushMatrix
    
#undef glPopMatrix
void iPhoneGLPopMatrix()
{
    dglPopMatrix();
}
#define glPopMatrix iPhoneGLPopMatrix

#undef glMatrixMode
void iPhoneGLMatrixMode( GLenum mode )
{
    dglMatrixMode( mode );
}
#define glMatrixMode iPhoneGLMatrixMode
}

/////////////////////////////////////////////////////////////////////////////
//
//    Compatibility wrappers delegating legacy GL1/ES1 calls to dgl/ShaderManager
//
static bool sIOSInBeginEnd = false;

void glColor4f( GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha )
{
    ShaderManager::color4f( red, green, blue, alpha );
}

void glColor4ub( GLubyte red, GLubyte green, GLubyte blue, GLubyte alpha )
{
    ShaderManager::color4ub( red, green, blue, alpha );
}

void glOrthof( GLfloat left, GLfloat right, GLfloat bottom, GLfloat top, GLfloat zNear, GLfloat zFar )
{
    dglOrtho( left, right, bottom, top, zNear, zFar );
}

void glFrustumf( GLfloat left, GLfloat right, GLfloat bottom, GLfloat top, GLfloat zNear, GLfloat zFar )
{
    dglFrustum( left, right, bottom, top, zNear, zFar );
}

void glLoadIdentity()
{
    dglLoadIdentity();
}

void glLoadMatrixf( const GLfloat *m )
{
    dglLoadMatrixf( m );
}

void glMultMatrixf( const GLfloat *m )
{
    dglMultMatrixf( m );
}

void glTranslatef( GLfloat x, GLfloat y, GLfloat z )
{
    dglTranslatef( x, y, z );
}

void glRotatef( GLfloat angle, GLfloat x, GLfloat y, GLfloat z )
{
    dglRotatef( angle, x, y, z );
}

void glScalef( GLfloat x, GLfloat y, GLfloat z )
{
    dglScalef( x, y, z );
}

void glTexEnvi( GLenum target, GLenum pname, GLint param )
{
    (void)target;
    (void)pname;
    switch (param)
    {
        case 0x2100: // GL_MODULATE
            ShaderManager::setTextureMode( DGL_TEX_MODULATE );
            break;
        case 0x0BE2: // GL_BLEND
            ShaderManager::setTextureMode( DGL_TEX_BLEND );
            break;
        case 0x1E01: // GL_REPLACE
            ShaderManager::setTextureMode( DGL_TEX_REPLACE );
            break;
        case 0x0104: // GL_ADD
            ShaderManager::setTextureMode( DGL_TEX_ADD );
            break;
        default:
            break;
    }
}

void glTexEnvfv( GLenum target, GLenum pname, const GLfloat *params )
{
    (void)target;
    if (pname == 0x2201 && params) // GL_TEXTURE_ENV_COLOR
        ShaderManager::setTextureEnvColor( ColorF(params[0], params[1], params[2], params[3]) );
}

void glAlphaFunc( GLenum func, GLclampf ref )
{
    if (func == GL_GREATER || func == GL_GEQUAL)
        ShaderManager::setAlphaThreshold( ref );
    else
        ShaderManager::setAlphaThreshold( -1.0f );
}

void glPointSize( GLfloat size )
{
    (void)size;
}

void glEnableClientState( GLenum array )
{
    (void)array;
}

void glDisableClientState( GLenum array )
{
    (void)array;
}

void glVertexPointer( GLint size, GLenum type, GLsizei stride, const GLvoid *pointer )
{
    (void)size; (void)type; (void)stride; (void)pointer;
}

void glColorPointer( GLint size, GLenum type, GLsizei stride, const GLvoid *pointer )
{
    (void)size; (void)type; (void)stride; (void)pointer;
}

void glTexCoordPointer( GLint size, GLenum type, GLsizei stride, const GLvoid *pointer )
{
    (void)size; (void)type; (void)stride; (void)pointer;
}

void glBegin( GLint mode )
{
    if (sIOSInBeginEnd) {
        AssertFatal(0, "glBegin(): called without a glEnd");
    }
    sIOSInBeginEnd = true;
    ShaderManager::begin( (GLenum)mode );
}

void glEnd()
{
    if (!sIOSInBeginEnd) {
        AssertFatal(0, "glEnd(): called without a glBegin");
    }
    ShaderManager::end();
    sIOSInBeginEnd = false;
}

void glTexCoord2f( GLfloat x, GLfloat y)
{
    AssertFatal( sIOSInBeginEnd, "glTexCoord2f(): called outside glBegin/glEnd");
    ShaderManager::texCoord2f( x, y );
}
void glVertex4f( GLfloat x, GLfloat y, GLfloat z, GLfloat w)
{
    AssertFatal( sIOSInBeginEnd, "glVertex4f(): called outside glBegin/glEnd");
    if (w != 0.0f && w != 1.0f)
        ShaderManager::vertex3f( x / w, y / w, z / w );
    else
        ShaderManager::vertex3f( x, y, z );
}
void glVertex2f( GLfloat x, GLfloat y )
{
    AssertFatal( sIOSInBeginEnd, "glVertex2f(): called outside glBegin/glEnd");
    ShaderManager::vertex2f( x, y );
}
void glVertex3f( GLfloat x, GLfloat y, GLfloat z )
{
    AssertFatal( sIOSInBeginEnd, "glVertex3f(): called outside glBegin/glEnd");
    ShaderManager::vertex3f( x, y, z );
}
void glVertex2fv( const F32 * pv )
{
    AssertFatal( sIOSInBeginEnd, "glVertex2fv(): called outside glBegin/glEnd");
    ShaderManager::vertex2f( pv[0], pv[1] );
}
void glVertex2i( GLint x, GLint y)
{
    AssertFatal( sIOSInBeginEnd, "glVertex2i(): called outside glBegin/glEnd");
    ShaderManager::vertex2i( x, y );
}
void glVertex3fv( const F32 * pv )
{
    AssertFatal( sIOSInBeginEnd, "glVertex3fv(): called outside glBegin/glEnd");
    ShaderManager::vertex3f( pv[0], pv[1], pv[2] );
}
void glVertex4fv( const F32 * pv )
{
    AssertFatal( sIOSInBeginEnd, "glVertex4fv(): called outside glBegin/glEnd");
    glVertex4f( pv[0], pv[1], pv[2], pv[3] );
}
void glNormal3f( GLfloat x, GLfloat y, GLfloat z )
{
    (void)x; (void)y; (void)z;
}
void glNormal3fv( const F32 * pv)
{
    (void)pv;
}
void glColor3fv( const F32 * pv)
{
    ShaderManager::color3f( pv[0], pv[1], pv[2] );
}
void glColor4fv( const F32 * pv)
{
    ShaderManager::color4f( pv[0], pv[1], pv[2], pv[3] );
}
void glActiveTextureARB( GLint index )
{
    glActiveTexture( (GLenum)index );
}
void glMultiTexCoord2fARB( GLint, GLfloat, GLfloat)
{
    AssertWarn( 0, "glMultiTexCoord2fARB(): this method call needs to be implemented");
}
void glPushAttrib( GLint )
{
    AssertWarn( 0, "glPushAttrib(): this method call needs to be implemented");
}
void glPopAttrib()
{
    AssertWarn( 0, "glPopAttrib(): this method call needs to be implemented");
}
void glClientActiveTextureARB( GLint texture )
{
    (void)texture;
}
void glColor3i( GLint  r, GLint g, GLint b )
{
    ShaderManager::color4f( ((GLfloat)r) * INV_32767, ((GLfloat)g) * INV_32767, ((GLfloat)b) * INV_32767, 1.0f );
}
void glColor3ubv( const GLubyte *v )
{
    ShaderManager::color3ub( v[0], v[1], v[2] );
}
void glColor4ubv( const GLubyte *v )
{
    ShaderManager::color4ub( v[0], v[1], v[2], v[3] );
}
void glRecti( GLint x1, GLint y1, GLint x2, GLint y2 )
{
    AssertFatal( !sIOSInBeginEnd, "glRecti(): called inside glBegin/glEnd");
    ShaderManager::begin(GL_LINE_LOOP);
    ShaderManager::vertex2i(x1, y1);
    ShaderManager::vertex2i(x2, y1);
    ShaderManager::vertex2i(x2, y2);
    ShaderManager::vertex2i(x1, y2);
    ShaderManager::end();
}

void glGetDoublev( GLint pname, GLdouble * params )
{
    if (pname == 0x0BA7) // GL_PROJECTION_MATRIX
    {
        F32 mat[16];
        dglGetProjectionColumnMajor(mat);
        for (int i = 0; i < 16; ++i)
            params[i] = (GLdouble)mat[i];
        return;
    }
    if (pname == 0x0BA6) // GL_MODELVIEW_MATRIX
    {
        F32 mat[16];
        dglGetModelviewColumnMajor(mat);
        for (int i = 0; i < 16; ++i)
            params[i] = (GLdouble)mat[i];
        return;
    }

    GLfloat fparams[16];
    glGetFloatv( (GLenum)pname, fparams );
    AssertFatal((glGetError() == GL_NO_ERROR), "glGetDoublev parameter invalid");
    params[0] = (double)fparams[0];
    if ((pname == GL_ALIASED_POINT_SIZE_RANGE) ||
        (pname == GL_ALIASED_LINE_WIDTH_RANGE) ||
        (pname == GL_DEPTH_RANGE) ||
        (pname == GL_MAX_VIEWPORT_DIMS)) {
        params[1] = (double)fparams[1];
        return;
    }
    if ((pname == GL_COLOR_CLEAR_VALUE) ||
        (pname == GL_COLOR_WRITEMASK) ||
        (pname == GL_SCISSOR_BOX) ||
        (pname == GL_VIEWPORT) ) {
        params[1] = (double)fparams[1];
        params[2] = (double)fparams[2];
        params[3] = (double)fparams[3];
        return;
    }
}
void glPolygonMode( GLint, GLint )
{
}
void glLockArraysEXT( GLint, GLint )
{
    AssertFatal( 0, "glLockArraysEXT(): this method call needs to be implemented");
}
void glUnlockArraysEXT()
{
    AssertFatal( 0, "glUnlockArraysEXT(): this method call needs to be implemented");
}
void glColor3ub( GLint r, GLint g, GLint b )
{
    ShaderManager::color3ub( (U8)r, (U8)g, (U8)b );
}
void glFogi( GLint pname, GLint param )
{
    (void)pname;
    (void)param;
}
/*	-Mat note: only guiCanvas uses this, and before adding this GL2ES stuff we didn't need to use it (it was one of the first things if'ed out)
void glDrawBuffer( GLint buffid )
{
    ((EAGLView *)platState.ctx).currentRenderBuffer = buffid;
    TEST_FOR_OPENGL_ERRORS
}
*/
void glColorTableEXT( GLint, GLint, GLint, GLint, GLint, const ColorI * )
{
    AssertFatal( 0, "glColorTableEXT(): this method call needs to be implemented");
}
void glBlendColorEXT( GLfloat, GLfloat, GLfloat, GLfloat )
{
    AssertFatal( 0, "glBlendColorEXT(): this method call needs to be implemented");
}
void glBlendEquationEXT( GLint )
{
    AssertFatal( 0, "glBlendEquationEXT(): this method call needs to be implemented");
}
void glArrayElement( GLint )
{
    AssertFatal( 0, "glArrayElement(): this method call needs to be implemented");
}
void glFogCoordPointerEXT(GLenum, GLsizei, void *)
{
    AssertFatal( 0, "glFogCoordPointerEXT(): this method call needs to be implemented");
}
void glTexGeni( GLenum coord, GLenum pname, GLint param )
{
    AssertFatal( 0, "glTexGeni(): this method call needs to be implemented");
}
void glTexGenfv( GLenum coord, GLenum pname, const GLfloat *params )
{
    AssertFatal( 0, "glTexGenfv(): this method call needs to be implemented");
}
void glClipPlane( GLuint plane, GLdouble * equation )
{
    U32 index = (plane >= 0x3000) ? (plane - 0x3000) : plane; // GL_CLIP_PLANE0
    ShaderManager::setClipPlane( index, equation[0], equation[1], equation[2], equation[3] );
}
GLboolean glAreTexturesResident( GLsizei n, const GLuint * textures, GLboolean* residences )
{
    GLboolean allResident = GL_TRUE;
    for (GLsizei i = 0; i < n; ++i)
    {
        GLboolean resident = glIsTexture(textures[i]);
        if (residences)
            residences[i] = resident;
        if (!resident)
            allResident = GL_FALSE;
    }
    return allResident;
}

void gluOrtho2D( GLfloat left, GLfloat right, GLfloat bottom, GLfloat top )
{
    dglOrtho( left, right, bottom, top, -1.0f, 1.0f );
}
GLint gluProject( GLdouble objx, GLdouble objy, GLdouble objz, const F64 *model, const F64 * proj, const GLint * vp, F64 * winx, F64 * winy, F64 * winz )
{
    Vector4F v = Vector4F( objx, objy, objz, 1.0f );
    MatrixF pmat = MatrixF( false );
        for (int i=0; i<16; i++) { ((F32*)pmat)[i] = (float)proj[i]; }
    MatrixF mmat = MatrixF( false );
        for (int i=0; i<16; i++) { ((F32*)mmat)[i] = (float)model[i]; }
        
    //Luma:	Projection fix
    mmat.transpose();
    pmat.transpose();
    (pmat.mul(mmat)).mul(v);
    
    //Luma:	Projection fix
    if(v.w == 0.0f)
    {
        return GL_FALSE;
    }
    F32		invW = 1.0f / v.w;
    v.x *= invW;
    v.y *= invW;
    v.z *= invW;
        
    *winx = (GLfloat)vp[0] + (GLfloat)vp[2] * (v.x + 1.0f) * 0.5f;
    *winy = (GLfloat)vp[1] + (GLfloat)vp[3] * (v.y + 1.0f) * 0.5f;
    *winz = (v.z + 1.0f) * 0.5f;
    int glError;
    glError = TEST_FOR_OPENGL_ERRORS
    return GL_TRUE;
}
GLint gluUnProject( GLdouble winx, GLdouble winy, GLdouble winz, const F64 *model, const F64 * proj, const GLint * vp, F64 * x, F64 * y, F64 * z )
{
    Vector4F v = Vector4F( 2.0f*(winx-vp[0])/vp[2] - 1.0f, 2.0f*(winy-vp[1])/vp[2] - 1.0f, 2.0f*vp[2] - 1.0f, 1.0f );
    MatrixF pmat = MatrixF( false );
    for (int i=0; i<16; i++) { ((F32*)pmat)[i] = (float)proj[i]; }
    MatrixF mmat = MatrixF( false );
    for (int i=0; i<16; i++) { ((F32*)mmat)[i] = (float)model[i]; }
    mmat = pmat.mul(mmat);

    mmat = mmat.inverse();
    mmat.mul( v );
    *x = v.x;
    *y = v.y;
    *z = v.z;
    int glError;
    glError = TEST_FOR_OPENGL_ERRORS
    return GL_TRUE;
}
