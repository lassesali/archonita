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

#include "console/console.h"
#include "console/consoleTypes.h"
#include "graphics/dgl.h"
#include "graphics/TextureManager.h"
#include "gui/guiSliderCtrl.h"
#include "gui/guiDefaultControlRender.h"
#include "platform/event.h"

IMPLEMENT_CONOBJECT(GuiSliderCtrl);

//----------------------------------------------------------------------------
GuiSliderCtrl::GuiSliderCtrl(void)
{
    mActive = true;
    mRange.set(0.0f, 1.0f);
    mTicks = 10;
    mValue = 0.5f;
    mThumbSize.set(8, 20);
    mShiftPoint = 5;
    mShiftExtent = 10;
    mDisplayValue = false;
    mMouseOver = false;
    mDepressed = false;
    mThumbProfile = NULL;
	mRendersChildren = false;
	mIsContainer = false;

    // Default the thumb to the shared scroll-thumb profile so un-themed sliders
    // still get a sensible raised thumb; a theme assigns its <Name>SliderThumbProfile.
    setField("thumbProfile", "GuiScrollThumbProfile");
}

//----------------------------------------------------------------------------
void GuiSliderCtrl::initPersistFields()
{
    Parent::initPersistFields();

    addGroup("Slider");
    addField("range", TypePoint2F, Offset(mRange, GuiSliderCtrl));
    addField("ticks", TypeS32, Offset(mTicks, GuiSliderCtrl));
    addField("value", TypeF32, Offset(mValue, GuiSliderCtrl));
    addField("thumbProfile", TypeGuiProfile, Offset(mThumbProfile, GuiSliderCtrl));
    endGroup("Slider");
}

//----------------------------------------------------------------------------
ConsoleMethod( GuiSliderCtrl, getValue, F32, 2, 2, "() Use the getValue method to get the current value of the slider.\n"
        "@return Returns current value of control (position of slider)"){
    return object->getValue();
}

//----------------------------------------------------------------------------
void GuiSliderCtrl::setScriptValue(const char *val)
{
    mValue = dAtof(val);
    updateThumb(mValue, false);
}

//----------------------------------------------------------------------------
bool GuiSliderCtrl::onWake()
{
    if (!Parent::onWake())
        return false;

    if (mThumbProfile != NULL)
        mThumbProfile->incRefCount();

    if (mThumbSize.y + mProfile->getFont(mFontSizeAdjust)->getHeight() - 4 <= (U32) mBounds.extent.y)
        mDisplayValue = true;
    else
        mDisplayValue = false;

    updateThumb(mValue, false, true);

    mHasTexture = mProfile->constructBitmapArray() >= NumBitmaps;
    if (mHasTexture)
        mBitmapBounds = mProfile->mBitmapArrayRects.address();

    return true;
}

//----------------------------------------------------------------------------
void GuiSliderCtrl::onSleep()
{
    Parent::onSleep();

    if (mThumbProfile != NULL)
        mThumbProfile->decRefCount();
}

//----------------------------------------------------------------------------
void GuiSliderCtrl::onTouchDown(const GuiEvent &event)
{
    if (!mActive || !mAwake || !mVisible)
        return;

    mouseLock();
    setFirstResponder();
    mDepressed = true;

    Point2I curMousePos = globalToLocalCoord(event.mousePoint);
    F32 value;
    if (mBounds.extent.x >= mBounds.extent.y)
        value = F32(curMousePos.x - mShiftPoint) / F32(mBounds.extent.x - mShiftExtent) * (mRange.y - mRange.x) + mRange.x;
    else
        value = F32(curMousePos.y) / F32(mBounds.extent.y) * (mRange.y - mRange.x) + mRange.x;

    updateThumb(value, !(event.modifier & SI_SHIFT));
}

//----------------------------------------------------------------------------
void GuiSliderCtrl::onTouchDragged(const GuiEvent &event)
{
    if (!mActive || !mAwake || !mVisible)
        return;

    Point2I curMousePos = globalToLocalCoord(event.mousePoint);
    F32 value;
    if (mBounds.extent.x >= mBounds.extent.y)
        value = F32(curMousePos.x - mShiftPoint) / F32(mBounds.extent.x - mShiftExtent) * (mRange.y - mRange.x) + mRange.x;
    else
        value = F32(curMousePos.y) / F32(mBounds.extent.y) * (mRange.y - mRange.x) + mRange.x;

    if (value > mRange.y)
        value = mRange.y;
    else if (value < mRange.x)
        value = mRange.x;

    if (!(event.modifier & SI_SHIFT) && mTicks > 2)
    {
        // If the shift key is held, snap to the nearest tick, if any are being drawn

        F32 tickStep = (mRange.y - mRange.x) / F32(mTicks + 1);

        F32 tickSteps = (value - mRange.x) / tickStep;
        S32 actualTick = S32(tickSteps + 0.5);

        value = actualTick * tickStep + mRange.x;
        AssertFatal(value <= mRange.y && value >= mRange.x, "Error, out of bounds value generated from shift-snap of slider");
    }

    Con::executef(this, 1, "onMouseDragged");

    updateThumb(value, !(event.modifier & SI_SHIFT));
}

//----------------------------------------------------------------------------
void GuiSliderCtrl::onTouchUp(const GuiEvent &)
{
    if (!mActive || !mAwake || !mVisible)
        return;
    mDepressed = false;
    mouseUnlock();
    execConsoleCallback();
}

void GuiSliderCtrl::onTouchEnter(const GuiEvent &event)
{
    setUpdate();
    if (isMouseLocked())
    {
        mDepressed = true;
        mMouseOver = true;
    }
    else
    {
        mMouseOver = true;
    }
}

void GuiSliderCtrl::onTouchLeave(const GuiEvent &)
{
    setUpdate();
    if (isMouseLocked())
        mDepressed = false;
    mMouseOver = false;
}

//----------------------------------------------------------------------------
void GuiSliderCtrl::updateThumb(F32 _value, bool snap, bool onWake)
{
    if (snap && mTicks > 1)
    {
        // If the shift key is held, snap to the nearest tick, if any are being drawn

        F32 tickStep = (mRange.y - mRange.x) / F32(mTicks + 1);

        F32 tickSteps = (_value - mRange.x) / tickStep;
        S32 actualTick = S32(tickSteps + 0.5);

        _value = actualTick * tickStep + mRange.x;
        AssertFatal(_value <= mRange.y && _value >= mRange.x, "Error, out of bounds value generated from shift-snap of slider");
    }


    mValue = _value;
    // clamp the thumb to legal values
    if (mValue < mRange.x) mValue = mRange.x;
    if (mValue > mRange.y) mValue = mRange.y;

    Point2I ext = mBounds.extent;
    ext.x -= (mShiftExtent + mThumbSize.x) / 2;
    // update the bounding thumb rect
    if (mBounds.extent.x >= mBounds.extent.y)
    {  // HORZ thumb
        S32 mx = (S32) ((F32(ext.x) * (mValue - mRange.x) / (mRange.y - mRange.x)));
        S32 my = ext.y / 2;
        if (mDisplayValue)
            my = mThumbSize.y / 2;

        mThumb.point.x = mx - (mThumbSize.x / 2);
        mThumb.point.y = my - (mThumbSize.y / 2);
        mThumb.extent = mThumbSize;
    }
    else
    {  // VERT thumb
        S32 mx = ext.x / 2;
        S32 my = (S32) ((F32(ext.y) * (mValue - mRange.x) / (mRange.y - mRange.x)));
        mThumb.point.x = mx - (mThumbSize.y / 2);
        mThumb.point.y = my - (mThumbSize.x / 2);
        mThumb.extent.x = mThumbSize.y;
        mThumb.extent.y = mThumbSize.x;
        // Keep the thumb inside the control at the travel extremes so it isn't
        // half-clipped by the control's clip rect. The value mapping is
        // unchanged; only the drawn rect is nudged in at the very ends.
        if (mThumb.point.y < 0)
            mThumb.point.y = 0;
        if (mThumb.point.y + mThumb.extent.y > mBounds.extent.y)
            mThumb.point.y = mBounds.extent.y - mThumb.extent.y;
    }
    setFloatVariable(mValue);
    setUpdate();

    // Use the alt console command if you want to continually update:
    if (!onWake)
        execAltConsoleCallback();
}

//----------------------------------------------------------------------------
void GuiSliderCtrl::onRender(Point2I offset, const RectI &updateRect)
{
    Point2I pos(offset.x + mShiftPoint, offset.y);
    Point2I ext(mBounds.extent.x - mShiftExtent, mBounds.extent.y);
    RectI thumb = mThumb;

    if (mHasTexture)
    {
        if (mTicks > 0)
        {
            // TODO: tick marks should be positioned based on the bitmap dimentions.
            Point2I mid(ext.x, ext.y / 2);
            Point2I oldpos = pos;
            pos += Point2I(1, 0);
            const ColorI darkTick(0, 0, 0, 255);
            const ColorI lightTick(230, 230, 230, 255);

            // tick marks
            for (U32 t = 0; t <= (mTicks + 1); t++)
            {
                S32 x = (S32)(F32(mid.x + 1) / F32(mTicks + 1) * F32(t)) + pos.x;
                S32 y = pos.y + mid.y;
                dglDrawLine(x, y + mShiftPoint, x, y + mShiftPoint * 2 + 2, darkTick);
            }
            for (U32 t = 0; t <= (mTicks + 1); t++)
            {
                S32 x = (S32)(F32(mid.x + 1) / F32(mTicks + 1) * F32(t)) + pos.x + 1;
                S32 y = pos.y + mid.y + 1;
                dglDrawLine(x, y + mShiftPoint, x, y + mShiftPoint * 2 + 3, lightTick);
            }
            pos = oldpos;
        }

        S32 index = SliderButtonNormal;
        if (mMouseOver)
            index = SliderButtonHighlight;
        dglClearBitmapModulation();

        //left border
        dglDrawBitmapSR(mProfile->mTextureHandle, Point2I(offset.x, offset.y + (mBounds.extent.y / 4)), mBitmapBounds[SliderLineLeft]);
        //right border
        dglDrawBitmapSR(mProfile->mTextureHandle, Point2I(offset.x + mBounds.extent.x - mBitmapBounds[SliderLineRight].extent.x, offset.y + (mBounds.extent.y / 4)), mBitmapBounds[SliderLineRight]);


        //draw our center piece to our slider control's border and stretch it
        RectI destRect;
        destRect.point.x = offset.x + mBitmapBounds[SliderLineLeft].extent.x;
        destRect.extent.x = mBounds.extent.x - mBitmapBounds[SliderLineLeft].extent.x - mBitmapBounds[SliderLineRight].extent.x;
        destRect.point.y = offset.y + (mBounds.extent.y / 4);
        destRect.extent.y = mBitmapBounds[SliderLineCenter].extent.y;

        RectI stretchRect;
        stretchRect = mBitmapBounds[SliderLineCenter];
        stretchRect.inset(1, 0);

        dglDrawBitmapStretchSR(mProfile->mTextureHandle, destRect, stretchRect);

        //draw our control slider button
        thumb.point += pos;
        dglDrawBitmapSR(mProfile->mTextureHandle, Point2I(thumb.point.x, offset.y), mBitmapBounds[index]);
    }
    else
    {
        // we're not usina a bitmap, draw procedurally.
        // Draw a themed groove/track from the main (Slider) profile beneath the
        // rule and tick marks; the thumb below uses the dedicated thumb profile.
        const S32 grooveThickness = 4;
        // renderUniversalRect takes RectI& by (non-const) reference, so the rect
        // must be a named lvalue -- a temporary won't bind on clang/gcc.
        RectI grooveRect = (mBounds.extent.x >= mBounds.extent.y)
            ? RectI(Point2I(pos.x, pos.y + (ext.y / 2) - (grooveThickness / 2)), Point2I(ext.x, grooveThickness))
            : RectI(Point2I(pos.x + (ext.x / 2) - (grooveThickness / 2), pos.y), Point2I(grooveThickness, ext.y));
        renderUniversalRect(grooveRect, mProfile, NormalState);

        const ColorI tickColor(0, 0, 0, 255);
        if (mBounds.extent.x >= mBounds.extent.y)
        {
            Point2I mid(ext.x, ext.y / 2);
            if (mDisplayValue)
                mid.set(ext.x, mThumbSize.y / 2);

            // horz rule
            dglDrawLine(pos.x, pos.y + mid.y, pos.x + mid.x, pos.y + mid.y, tickColor);

            // tick marks
            for (U32 t = 0; t <= (mTicks + 1); t++)
            {
                S32 x = (S32)(F32(mid.x - 1) / F32(mTicks + 1) * F32(t));
                dglDrawLine(pos.x + x, pos.y + mid.y - mShiftPoint,
                            pos.x + x, pos.y + mid.y + mShiftPoint, tickColor);
            }
        }
        else
        {
            Point2I mid(ext.x / 2, ext.y);

            // vert rule
            dglDrawLine(pos.x + mid.x, pos.y, pos.x + mid.x, pos.y + mid.y, tickColor);

            // tick marks
            for (U32 t = 0; t <= (mTicks + 1); t++)
            {
                S32 y = (S32)(F32(mid.y - 1) / F32(mTicks + 1) * F32(t));
                dglDrawLine(pos.x + mid.x - mShiftPoint, pos.y + y,
                            pos.x + mid.x + mShiftPoint, pos.y + y, tickColor);
            }
            mDisplayValue = false;
        }

        // draw the thumb using its dedicated profile (falls back to the main
        // profile if none is set).
        thumb.point += pos;
        GuiControlProfile* thumbProfile = mThumbProfile ? mThumbProfile : mProfile;
        GuiControlState thumbState = mDepressed ? SelectedState : (mMouseOver ? HighlightState : NormalState);
        renderUniversalRect(thumb, thumbProfile, thumbState);
    }

    if (mDisplayValue)
    {
        char buf[20];
        dSprintf(buf, sizeof(buf), "%0.3g", mValue);

        Point2I textStart = thumb.point;

        S32 txt_w = mProfile->getFont(mFontSizeAdjust)->getStrWidth((const UTF8 *) buf);

        textStart.x += (S32) ((thumb.extent.x / 2.0f));
        textStart.y += thumb.extent.y - 2; //19
        textStart.x -= (txt_w / 2);
        if (textStart.x < offset.x)
            textStart.x = offset.x;
        else if (textStart.x + txt_w > offset.x + mBounds.extent.x)
            textStart.x -= ((textStart.x + txt_w) - (offset.x + mBounds.extent.x));

        dglSetBitmapModulation(getFontColor(mProfile));
        dglDrawText(mProfile->getFont(mFontSizeAdjust), textStart, buf, mProfile->mFontColors);
    }
    renderChildControls(offset, mBounds, updateRect);
}

