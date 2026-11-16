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

#import "platformiOS/T2DView.h"
//#import <QuartzCore/QuartzCore.h>
//#include "platformiPhone/iPhoneEvents.h"

#import "platformiOS/platformiOS.h"
#import "platformiOS/platformGL.h"
#import "platformiOS/iOSOGLVideo.h"
#include "console/console.h"
#include "math/mPoint.h"
#include "platform/event.h"
#include "game/gameInterface.h"
#include "console/consoleInternal.h"
#include "console/ast.h"
#include "io/fileStream.h"
#include "platformiOS/iOSUtil.h"
#include "platformiOS/iOSEvents.h"
#include "graphics/dgl.h"
#include "debug/profiler.h"

#define USE_DEPTH_BUFFER 0

extern bool createMouseMoveEvent(S32 touchNumber, S32 x, S32 y, S32 lastX, S32 lastY);
extern bool createMouseDownEvent(S32 touchNumber, S32 x, S32 y, U32 numTouches);
extern bool createMouseUpEvent(S32 touchNumber, S32 x, S32 y, S32 lastX, S32 lastY, U32 numTouches); //EFM
extern void createMouseTapEvent( S32 nbrTaps, S32 x, S32 y );

bool retinaEnabled;

#define MAX_RAW_TOUCH_POINTERS 10

struct RawTouchEntry
{
    const void* touchPtr;
    S32 touchId;
    Point2I lastPoint;
};

static RawTouchEntry gRawTouches[MAX_RAW_TOUCH_POINTERS];
static S32 gNextRawTouchId = 1;

static int findRawTouchSlot(const void* touchPtr)
{
    if (!touchPtr)
        return -1;

    for (int i = 0; i < MAX_RAW_TOUCH_POINTERS; i++)
    {
        if (gRawTouches[i].touchPtr == touchPtr)
            return i;
    }
    return -1;
}

static S32 allocRawTouchId()
{
    for (int attempt = 0; attempt < MAX_RAW_TOUCH_POINTERS + 1; attempt++)
    {
        S32 candidate = gNextRawTouchId;
        gNextRawTouchId = (gNextRawTouchId < 0x7FFFFFFF) ? (gNextRawTouchId + 1) : 1;

        bool inUse = false;
        for (int i = 0; i < MAX_RAW_TOUCH_POINTERS; i++)
        {
            if (gRawTouches[i].touchPtr != NULL && gRawTouches[i].touchId == candidate)
            {
                inUse = true;
                break;
            }
        }
        if (!inUse)
            return candidate;
    }
    return 1;
}

static int allocRawTouchSlot(const void* touchPtr)
{
    int slot = findRawTouchSlot(touchPtr);
    if (slot != -1)
        return slot;

    for (int i = 0; i < MAX_RAW_TOUCH_POINTERS; i++)
    {
        if (gRawTouches[i].touchPtr == NULL)
        {
            gRawTouches[i].touchPtr = touchPtr;
            gRawTouches[i].touchId = allocRawTouchId();
            return i;
        }
    }
    return -1;
}

void iOSClearRawTouches(void)
{
    for (int i = 0; i < MAX_RAW_TOUCH_POINTERS; i++)
    {
        gRawTouches[i].touchPtr = NULL;
        gRawTouches[i].touchId = -1;
        gRawTouches[i].lastPoint.set(0, 0);
    }
}

void ConvertToRetina(CGPoint *p)
{
    CGFloat screenScale = [[UIScreen mainScreen] scale];
    p->x *= screenScale;
    p->y *= screenScale;
}

static Point2I extractTouchPoint(UITouch* touch, UIView* view, bool previous)
{
    // locationInView: already reports coordinates in the view's own space on iOS 8+.
    // Do not apply _iOSGetPortraitTouchoffset() here: physical tilt notifications
    // update _iOSGameGetOrientation() even when the UI stays locked to landscape.
    CGPoint point = previous ? [touch previousLocationInView:view] : [touch locationInView:view];
    if (retinaEnabled)
        ConvertToRetina(&point);

    return Point2I((S32)point.x, (S32)point.y);
}

@implementation T2DView

@synthesize context = _context;
@synthesize currentAngle;
@synthesize isLayedOut;

-(void)swapBuffers {
	if( self.isLayedOut == true ) {
		[self.context presentRenderbuffer:GL_RENDERBUFFER];
	}
}

- (void)layoutSubviews {
}

-(void)rotateByAngle:(CGFloat)angle {
	
	CGAffineTransform transform = self.transform;
    
	transform = CGAffineTransformRotate(transform, angle);
	self.transform = transform;
}

-(void)rotateToAngle:(CGFloat)angle {
	CGFloat rotateAmount = (angle - currentAngle);//need to make this work better
	if( rotateAmount == 0 ) {
		return;
	}
	currentAngle = angle;
	[self rotateByAngle:rotateAmount];
}

-(void)centerOnPoint:(CGPoint)point {
	self.center = point;
}

- (void)touchesBegan:(NSSet *)touches withEvent:(UIEvent *)event
{
    // Track each UITouch pointer across phases rather than passing the unordered
    // NSSet enumeration index (which resets to 0 on every callback).
    for (UITouch *touch in touches)
    {
        const void* touchPtr = (__bridge const void*)touch;
        Point2I point = extractTouchPoint(touch, self, false);
        int slot = allocRawTouchSlot(touchPtr);
        S32 touchId = (slot != -1) ? gRawTouches[slot].touchId : -1;
        if (slot != -1)
            gRawTouches[slot].lastPoint = point;

        int numTouches = (int)[touch tapCount];
        createMouseDownEvent(touchId, point.x, point.y, numTouches);
    }
}

- (void)touchesMoved:(NSSet *)touches withEvent:(UIEvent *)event
{
    for (UITouch *touch in touches)
    {
        const void* touchPtr = (__bridge const void*)touch;
        Point2I point = extractTouchPoint(touch, self, false);
        int slot = findRawTouchSlot(touchPtr);
        Point2I prevPoint = (slot != -1) ? gRawTouches[slot].lastPoint : extractTouchPoint(touch, self, true);
        if (slot == -1)
            slot = allocRawTouchSlot(touchPtr);

        S32 touchId = (slot != -1) ? gRawTouches[slot].touchId : -1;
        createMouseMoveEvent(touchId, point.x, point.y, prevPoint.x, prevPoint.y);

        if (slot != -1)
            gRawTouches[slot].lastPoint = point;
    }
}

- (void)handleTouchesEnded:(NSSet *)touches isCancelled:(BOOL)cancelled
{
    for (UITouch *touch in touches)
    {
        const void* touchPtr = (__bridge const void*)touch;
        Point2I point = extractTouchPoint(touch, self, false);
        int slot = findRawTouchSlot(touchPtr);
        Point2I prevPoint = (slot != -1) ? gRawTouches[slot].lastPoint : extractTouchPoint(touch, self, true);
        S32 touchId = (slot != -1) ? gRawTouches[slot].touchId : -1;
        if (slot != -1)
        {
            gRawTouches[slot].touchPtr = NULL;
            gRawTouches[slot].touchId = -1;
        }

        int tc = (int)[touch tapCount];
        createMouseUpEvent(touchId, point.x, point.y, prevPoint.x, prevPoint.y, tc);

        //Luma: Tap support
        if (!cancelled && tc > 0)
        {
            // this was a tap, so create a tap event
            createMouseTapEvent(tc, point.x, point.y);
        }
    }
}

- (void)touchesEnded:(NSSet *)touches withEvent:(UIEvent *)event
{
    [self handleTouchesEnded:touches isCancelled:NO];
}

- (void)touchesChangedWithEvent:(UIEvent *)event
{
    Con::printf("Touches Changed with Event");
}

- (void)touchesCancelled:(NSSet *)touches withEvent:(UIEvent *)event
{
    [self handleTouchesEnded:touches isCancelled:YES];
}

bool gScreenAutoRotate = false;
int currentRotate = 0;
#define ROTATE_LEFT		0
#define ROTATE_RIGHT	1
#define ROTATE_UP		2
@end
