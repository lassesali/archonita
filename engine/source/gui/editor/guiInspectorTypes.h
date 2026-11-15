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
#ifndef _GUI_INSPECTOR_TYPES_H_
#define _GUI_INSPECTOR_TYPES_H_

#ifndef _GUI_INSPECTOR_H_
#include "gui/editor/guiInspector.h"
#endif

#ifndef _ANIMATION_ASSET_H_
#include "2d/assets/AnimationAsset.h"
#endif

#ifndef _FONT_ASSET_H_
#include "2d/assets/FontAsset.h"
#endif // !_FONT_ASSET_H_

#ifndef _PARTICLE_ASSET_H_
#include "2d/assets/ParticleAsset.h"
#endif // !_PARTICLE_ASSET_H_


#ifndef _GUICONTROL_H_
#include "gui/guiControl.h"
#endif

#ifndef _H_GUIDEFAULTCONTROLRENDER_
#include "gui/guiDefaultControlRender.h"
#endif

#ifndef _GUISCROLLCTRL_H_
#include "gui/containers/guiScrollCtrl.h"
#endif

#ifndef _GUITEXTEDITCTRL_H_
#include "gui/guiTextEditCtrl.h"
#endif

#ifndef _GUIDROPDOWNCTRL_H_
#include "gui/buttons/guiDropDownCtrl.h"
#endif

#ifndef _GUIGRIDCTRL_H_
#include "gui/containers/guiGridCtrl.h"
#endif

#ifndef _GUICHECKBOXCTRL_H_
#include "gui/buttons/guiCheckBoxCtrl.h"
#endif

#ifndef _GUICOLORPICKERCTRL_H_
#include "gui/guiColorPickerCtrl.h"
#endif // !_GUICOLORPICKERCTRL_H_

#include "gui/guiColorPopupCtrl.h"



class GuiInspectorTypeEnum : public GuiInspectorField
{
private:
   typedef GuiInspectorField Parent;
public:
   DECLARE_CONOBJECT(GuiInspectorTypeEnum);
   static void consoleInit();

   virtual GuiControl*        constructEditControl(S32 width);
   virtual void               setData( StringTableEntry data );
   virtual StringTableEntry   getData();
   virtual void               updateValue( StringTableEntry newValue );
};

class GuiInspectorTypeCheckBox : public GuiInspectorField
{
private:
   typedef GuiInspectorField Parent;
public:
   DECLARE_CONOBJECT(GuiInspectorTypeCheckBox);
   static void consoleInit();

   virtual GuiControl* constructEditControl(S32 width);
};

class GuiInspectorTypeGuiProfile : public GuiInspectorTypeEnum
{
private:
   typedef GuiInspectorTypeEnum Parent;
public:
   DECLARE_CONOBJECT(GuiInspectorTypeGuiProfile);
   static void consoleInit();

   virtual GuiControl* constructEditControl(S32 width);
};

class GuiInspectorTypeGuiBorderProfile : public GuiInspectorTypeEnum
{
private:
   typedef GuiInspectorTypeEnum Parent;
public:
   DECLARE_CONOBJECT(GuiInspectorTypeGuiBorderProfile);
   static void consoleInit();

   virtual GuiControl* constructEditControl(S32 width);
};

class GuiInspectorTypeGuiCursor : public GuiInspectorTypeEnum
{
private:
	typedef GuiInspectorTypeEnum Parent;
public:
	DECLARE_CONOBJECT(GuiInspectorTypeGuiCursor);
	static void consoleInit();

	virtual GuiControl* constructEditControl(S32 width);
};

class GuiInspectorTypeFileName : public GuiInspectorField
{
private:
   typedef GuiInspectorField Parent;
public:
   DECLARE_CONOBJECT(GuiInspectorTypeFileName);
   static void consoleInit();

   SimObjectPtr<GuiButtonCtrl>   mBrowseButton;

   virtual GuiControl*        constructEditControl(S32 width);
   virtual void               resize(const Point2I &newPosition, const Point2I &newExtent);
};

class GuiInspectorTypeColor : public GuiInspectorField
{
private:
   typedef GuiInspectorField Parent;
protected:
	GuiColorPopupCtrl* mColorPopup;
	GuiTextEditCtrl* mRedEdit;
	GuiTextEditCtrl* mGreenEdit;
	GuiTextEditCtrl* mBlueEdit;
	GuiTextEditCtrl* mAlphaEdit;

	void constructColorPopup(GuiControl* retCtrl);
	void constructColorEditBoxes(GuiControl* retCtrl, S32 width);
	GuiTextEditCtrl* constructColorEditBox(GuiControl* retCtrl, S32 posX, S32 width, StringTableEntry text);
public:
   StringTableEntry  mColorFunction;

   virtual GuiControl*        constructEditControl(S32 width);
   virtual void               finishControlConstruction() = 0;
};

class GuiInspectorTypeColorI : public GuiInspectorTypeColor
{
private:
   typedef GuiInspectorTypeColor Parent;
public:
   GuiInspectorTypeColorI();

   DECLARE_CONOBJECT(GuiInspectorTypeColorI);
   static void consoleInit();
   void updateValue(StringTableEntry newValue);
   void finishControlConstruction();
};

class GuiInspectorTypeColorF : public GuiInspectorTypeColor
{
private:
   typedef GuiInspectorTypeColor Parent;
public:
   GuiInspectorTypeColorF();

   DECLARE_CONOBJECT(GuiInspectorTypeColorF);
   static void consoleInit();
   void updateValue(StringTableEntry newValue);
   void finishControlConstruction();
};

class GuiInspectorTypeFluidColorI : public GuiInspectorTypeColor
{
private:
	typedef GuiInspectorTypeColor Parent;
public:
	GuiInspectorTypeFluidColorI();

	DECLARE_CONOBJECT(GuiInspectorTypeFluidColorI);
	static void consoleInit();
	void updateValue(StringTableEntry newValue);
	void finishControlConstruction();
};

class GuiInspectorTypeSimObjectPtr : public GuiInspectorField
{
private:
   typedef GuiInspectorField Parent;
public:
   DECLARE_CONOBJECT(GuiInspectorTypeSimObjectPtr);
   static void consoleInit();

   virtual GuiControl*        constructEditControl(S32 width);
   virtual StringTableEntry   getData();
};

class GuiInspectorTypeS32 : public GuiInspectorField
{
private:
   typedef GuiInspectorField Parent;
public:
   DECLARE_CONOBJECT(GuiInspectorTypeS32);
   static void consoleInit();

   virtual GuiControl*        constructEditControl(S32 width);
   virtual const char*        getData();
};

class GuiInspectorTypeDualValue : public GuiInspectorField
{
protected: 
	GuiTextEditCtrl* mCtrlX;
	GuiTextEditCtrl* mCtrlY;

public:
	virtual void		constructEditControlChildren(GuiControl* retCtrl, S32 width);
	virtual void		updateValue(StringTableEntry newValue);
};

class GuiInspectorTypePoint2I : public GuiInspectorTypeDualValue
{
private:
   typedef GuiInspectorField Parent;
public:
   DECLARE_CONOBJECT(GuiInspectorTypePoint2I);
   static void consoleInit();

   virtual GuiControl*        constructEditControl(S32 width);
};

class GuiInspectorTypePoint2F : public GuiInspectorTypeDualValue
{
private:
   typedef GuiInspectorField Parent;
public:
   DECLARE_CONOBJECT(GuiInspectorTypePoint2F);
   static void consoleInit();

   virtual GuiControl*        constructEditControl(S32 width);
};

class GuiInspectorTypeVector2 : public GuiInspectorTypeDualValue
{
private:
   typedef GuiInspectorField Parent;
public:
   DECLARE_CONOBJECT(GuiInspectorTypeVector2);
   static void consoleInit();

   virtual GuiControl*        constructEditControl(S32 width);
};

class GuiInspectorTypeAsset : public GuiInspectorField
{
private:
   typedef GuiInspectorField Parent;
public:
   DECLARE_CONOBJECT(GuiInspectorTypeAsset);

   StringTableEntry mAssetType;

   SimObjectPtr<GuiButtonCtrl>   mBrowseButton;

   virtual GuiControl*        constructEditControl(S32 width);
};

class GuiInspectorTypeImageAssetPtr : public GuiInspectorTypeAsset
{
private:
   typedef GuiInspectorTypeAsset Parent;
public:
   GuiInspectorTypeImageAssetPtr();

   DECLARE_CONOBJECT(GuiInspectorTypeImageAssetPtr);
   static void consoleInit();
};

class GuiInspectorTypeAnimationAssetPtr : public GuiInspectorTypeAsset
{
private:
   typedef GuiInspectorTypeAsset Parent;
public:
   GuiInspectorTypeAnimationAssetPtr();

   DECLARE_CONOBJECT(GuiInspectorTypeAnimationAssetPtr);
   static void consoleInit();
};

class GuiInspectorTypeAudioAssetPtr : public GuiInspectorTypeAsset
{
private:
   typedef GuiInspectorTypeAsset Parent;
public:
   GuiInspectorTypeAudioAssetPtr();

   DECLARE_CONOBJECT(GuiInspectorTypeAudioAssetPtr);
   static void consoleInit();
};

class GuiInspectorTypeFontAssetPtr : public GuiInspectorTypeAsset
{
private:
   typedef GuiInspectorTypeAsset Parent;
public:
   GuiInspectorTypeFontAssetPtr();

   DECLARE_CONOBJECT(GuiInspectorTypeFontAssetPtr);
   static void consoleInit();
};

class GuiInspectorTypeParticleAssetPtr : public GuiInspectorTypeAsset
{
private:
   typedef GuiInspectorTypeAsset Parent;
public:
   GuiInspectorTypeParticleAssetPtr();

   DECLARE_CONOBJECT(GuiInspectorTypeParticleAssetPtr);
   static void consoleInit();
};

#endif
