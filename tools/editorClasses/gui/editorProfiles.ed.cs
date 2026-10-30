//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

if(!isObject(GuiSeparatorProfile)) new GuiControlProfile (GuiSeparatorProfile)
{
   opaque = false;
   border = false;
};


//---------------------------------------------------------------------------------------------
// Fill Colors
//---------------------------------------------------------------------------------------------
if(!isObject(EditorFillColors)) new GuiControlProfile (EditorFillColors)
{
   fillColor = "102 153 204 100";
   fillColorHL = "102 153 204 100";
   borderColor = "0 51 153";
   borderColorHL = "0 51 153";
};


//---------------------------------------------------------------------------------------------
// Font Styles
//---------------------------------------------------------------------------------------------
if(!isObject(EditorFontHL)) new GuiControlProfile (EditorFontHL : EditorFillColors ) 
{ 
   fontType    = "Arial";
   fontSize    = "15";
   fontColorHL = "25 25 25 220";
   fontColorNA = "128 128 128";
   fontColor = "0 0 0 150";

};
if(!isObject(EditorTextHLLeft)) new GuiControlProfile (EditorTextHLLeft : EditorFontHL ) { justify = "left"; };
if(!isObject(EditorTextHLCenter)) new GuiControlProfile (EditorTextHLCenter : EditorFontHL ) { justify = "center"; };
if(!isObject(EditorTextHLRight)) new GuiControlProfile (EditorTextHLRight : EditorFontHL ) { justify = "right"; };


if(!isObject(EditorDecorativeTextHLCenter12)) new GuiControlProfile (EditorDecorativeTextHLCenter12 : EditorTextHLCenter ) { modal = false; active=false; fontSize = "12"; };

if(!isObject(EditorFontHLBold)) new GuiControlProfile (EditorFontHLBold : EditorFontHL ) 
{ 
   fontType    = "Arial Bold";
};

if(!isObject(EditorFontHLBoldTransparent)) new GuiControlProfile (EditorFontHLBoldTransparent : EditorFontHLBold ) 
{ 
   border = false;
   fillColor = "102 153 204 0";
   fillColorHL = "102 153 204 0";
   borderColor = "0 51 153 0";
   borderColorHL = "0 51 153 0";
   
};
if(!isObject(EditorTextHLBoldLeft)) new GuiControlProfile (EditorTextHLBoldLeft : EditorFontHLBold ) { justify = "left"; };
if(!isObject(EditorTextHLBoldCenter)) new GuiControlProfile (EditorTextHLBoldCenter : EditorFontHLBold ) { justify = "center"; };
if(!isObject(EditorTextHLBoldRight)) new GuiControlProfile (EditorTextHLBoldRight : EditorFontHLBold ) { justify = "right"; };


// 
// Register Dialog Scheme
//
if(!isObject(EditorTextWhiteBoldCenter)) new GuiControlProfile( EditorTextWhiteBoldCenter : EditorTextHLBoldCenter )
{
   fontColorHL = "25 25 25 220";
   fontColorNA = "128 128 128";
   fontColor = "255 255 255";   
};

if(!isObject(EditorTextWhiteRight)) new GuiControlProfile( EditorTextWhiteRight : EditorTextHLRight )
{
   fontColorHL = "25 25 25 220";
   fontColorNA = "128 128 128";
   fontColor = "255 255 255";   
};

if(!isObject(EditorRegisterPanelButton) ) new GuiControlProfile( EditorRegisterPanelButton : EditorTextHLBoldCenter )
{  
   fontSize    = "20";
   opaque      = false;
   border      = -2;
   bitmap      = "./images/panel_button";
};


// Decorative (Click-Through Text)
if(!isObject(EditorDecorativeTextHLLeft)) new GuiControlProfile (EditorDecorativeTextHLLeft : EditorTextHLLeft ) { modal = false; };

//---------------------------------------------------------------------------------------------
// Text Edit Styles
//---------------------------------------------------------------------------------------------
if (!isObject(EditorTextEdit)) new GuiControlProfile (EditorTextEdit : EditorTextHLLeft )
{
   fillColor = "255 255 255 200";
   canKeyFocus = true;
   tab         = true;
   opaque = true;  
   returnTab  = true; 
   bitmap = "./images/textEdit";
   border = -2;
};

if (!isObject(EditorTextEditBold)) new GuiControlProfile (EditorTextEditBold : EditorTextHLBoldLeft )
{
   fillColor = "255 255 255 200";
   canKeyFocus = true;
   tab         = true;
   opaque      = true;
   bitmap = "./images/textEdit";
   border = -2;
};
if(!isObject(EditorTextEditBold22) ) new GuiControlProfile( EditorTextEditBold22 : EditorTextEditBold )
{
   fillColor = "255 255 255";   
   fontSize    = "22";
};

//---------------------------------------------------------------------------------------------
// Container Panel Styles
//---------------------------------------------------------------------------------------------
if(!isObject(EditorPanelLight)) new GuiControlProfile (EditorPanelLight : EditorFontHLBold) 
{
   opaque = false;
   bitmap = "./images/panel_light";
   border = -2;
};

if(!isObject(EditorPanelMedium)) new GuiControlProfile (EditorPanelMedium : EditorFontHLBold) 
{
   opaque = false;
   bitmap = "./images/panel_medium";
   border = -2;
};

if(!isObject(EditorPanelDark)) new GuiControlProfile (EditorPanelDark : EditorFontHLBold) 
{
   opaque = false;
   bitmap = "./images/panel_dark";
   border = -2;
};

//---------------------------------------------------------------------------------------------
// Button Styles
//---------------------------------------------------------------------------------------------
if( !isObject( EditorButton ) ) new GuiControlProfile(EditorButton : EditorTextHLBoldCenter)
{
   border = -2;
   opaque = true;
   bitmap = "./images/button";
};

if( !isObject( EditorButtonToolbar ) ) new GuiControlProfile(EditorButtonToolbar : EditorButton)
   { bitmap = "./images/button_toolbar"; };

if( !isObject( EditorButtonLeft ) ) new GuiControlProfile(EditorButtonLeft : EditorButton) 
   { bitmap = "./images/button_left"; };
if( !isObject( EditorButtonMiddle ) ) new GuiControlProfile(EditorButtonMiddle : EditorButton) 
   { bitmap = "./images/button_middle"; };
if( !isObject( EditorButtonRight ) ) new GuiControlProfile(EditorButtonRight : EditorButton)
   { bitmap = "./images/button_right"; };

//---------------------------------------------------------------------------------------------
// Window Style
//---------------------------------------------------------------------------------------------
if(!isObject(EditorWindowProfile)) new GuiControlProfile (EditorWindowProfile : EditorTextHLBoldCenter )
{
   bitmap = "./images/window";
   textOffset = "5 5";
   fillColor = "52 52 74 175";
};


//
//
//
if(!isObject(EditorRolloutProfile)) new GuiControlProfile (EditorRolloutProfile : EditorTextHLBoldLeft )
{
   opaque = false;
   border = false;
  
   bitmap = "./images/rollout";
   
   textOffset = "30 2";
 };

//
if(!isObject(EditorRolloutProfileDark)) new GuiControlProfile (EditorRolloutProfileDark : EditorTextHLBoldLeft )
{
   opaque = false;
   border = false;
  
   bitmap = "./images/rollout_dark";
   
   textOffset = "30 2";
 };


///
//---------------------------------------------------------------------------------------------
// Window Style
//---------------------------------------------------------------------------------------------
if (!isObject(EditorTreeViewProfile)) new GuiControlProfile ( EditorTreeViewProfile : EditorFontHL )
{
   bitmap            = "./images/treeView";
   autoSizeHeight    = true;
   canKeyFocus       = true;
   
   
   fontColorHL = "25 25 25 220";
   fontColorNA = "128 128 128";
   fontColor = "0 0 0 150";
  
  
   fillColor = "102 153 204 100";
   fillColorHL = "12 63 114 100";
//   borderColor = "0 51 153";
//   borderColorHL = "0 51 153";
  
  
   fontColorHL = "102 153 204 100";
   fontColorNA = "240 240 240";
   fontColorSEL= "50 50 50"; 
   fillColorHL = "255 255 255 100";   
};



if(!isObject(GuiParticleListBoxProfile)) new GuiControlProfile (GuiParitcleListBoxProfile : GuiListBoxProfile)
{
   tab = true;
   canKeyFocus = true;
   fontColor = "0 0 0";
   fontColorHL = "25 25 25 220";
   fontColorNA = "128 128 128";
   fontColor = "0 0 0 150";
};

if(!isObject(EditorScrollProfile)) new GuiControlProfile (EditorScrollProfile : EditorFillColors )
{
   fillColor = "255 255 255";
   borderColor = "51 51 53 0";
   borderColorHL = "51 51 53 0";
   bitmap = "./images/scroll";
};

if(!isObject(EditorTransparentScrollProfile)) new GuiControlProfile (EditorTransparentScrollProfile : EditorScrollProfile)
{
   opaque = false;
   border = false;
};

if(!isObject(EditorTransparentProfile)) new GuiControlProfile (EditorTransparentProfile : EditorFontHL )
{
   border = false;
   opaque = false;
};

if(!isObject(EditorStackProfile)) new GuiControlProfile ( EditorStackProfile : EditorPanelDark){};

if( !isObject(EditorToolbarButtonProfile) ) new GuiControlProfile( EditorToolbarButtonProfile : EditorPanelDark ){};


//---------------------------------------------------------------------------------------------
// Editor Tab Book Profiles
//---------------------------------------------------------------------------------------------
if(!isObject(EditorTabBackground)) new GuiControlProfile (EditorTabBackground : EditorPanelLight){};
if(!isObject(EditorTabBook)) new GuiControlProfile (EditorTabBook : EditorTextHLBoldCenter )
{
   bitmap = "./images/tabBook";
   tabPosition = "Top";
   tabRotation = "Horizontal";
   profileForChildren = EditorTabBackground;
   cankeyfocus = true;
};
if(!isObject(EditorTabPage)) new GuiControlProfile ( EditorTabPage : EditorPanelLight ){ tab = true; };

if(!isObject(EditorToolTipProfile)) new GuiControlProfile (EditorToolTipProfile : GuiToolTipProfile)
{
};

if(!isObject(EditorContainerProfile)) new GuiControlProfile (EditorContainerProfile)
{
   border = 0;
};

if(!isObject(EditorCheckBox)) new GuiControlProfile (EditorCheckBox : GuiCheckBoxProfile)
{
   fontType    = "Arial";
   fontSize    = "15";
   fontColorHL = "25 25 25 220";
   fontColorNA = "128 128 128";
   fontColor   = "0 0 0 150";
};

if(!isObject(EditorCheckBoxBold)) new GuiControlProfile (EditorCheckBoxBold : EditorCheckBox)
{
   fontType    = "Arial Bold";
   fontSize    = "15";
   fontColorHL = "25 25 25 220";
   fontColorNA = "128 128 128";
   fontColor   = "0 0 0 150";
};


if(!isObject(EditorPopupMenuItemBorder)) new GuiControlProfile ( EditorPopupMenuItemBorder : EditorButton )
{
   borderColor = "51 51 53 200";
   borderColorHL = "51 51 53 200";

};
if(!isObject(EditorPopupMenu)) new GuiControlProfile (EditorPopupMenu : EditorTextHLCenter )
{
   opaque = true;
   mouseOverSelected = true;
   textOffset = "3 3";
   border = 4;
   borderThickness = 2;
   fixedExtent = true;
   bitmap = "./images/scroll";
   hasBitmapArray = true;
   profileForChildren = EditorPopupMenuItemBorder;
   fillColor = "255 255 255 200";
   fontColorHL = "128 128 128";
   borderColor = "151 151 153 175";
   borderColorHL = "151 151 153 175";

};


if(!isObject(EditorPopupMenuLarge)) new GuiControlProfile (EditorPopupMenuLarge : EditorPopupMenu)
{
   textOffset         = "6 3";
   bitmap             = "./images/dropDown";
   hasBitmapArray     = true;
   border             = -3;
   profileForChildren = EditorPopupMenu;
};


if(!isObject(EditorSlider)) new GuiControlProfile (EditorSlider : EditorFontHL )
{
   bitmap = "./images/slider";
};

if(!isObject(EditorListBox)) new GuiControlProfile (EditorListBox : EditorFontHL)
{
   tab = true;
   canKeyFocus = true;
};


if (!isObject(EditorRolloutProfile)) new GuiControlProfile (EditorRolloutProfile : GuiRolloutProfile)
{
};

if (!isObject(EditorListBoxProfile)) new GuiControlProfile (EditorListBoxProfile : GuiPopUpMenuProfile)
{
};

if (!isObject(EditorMLTextProfile)) new GuiControlProfile (EditorMLTextProfile: EditorTextWhiteBoldCenter)
{
   border = false;
   fillColor = "102 153 204 0";
   fillColorHL = "102 153 204 0";
   borderColor = "0 51 153 0";
   borderColorHL = "0 51 153 0";   
};

if(!isObject(GuiFormProfile)) new GuiControlProfile(GuiFormProfile)
{
   opaque = false;
   border = 5;
   
   bitmap = "./images/form";
   hasBitmapArray = true;

   fontColor = "0 0 0";
   fontColorHL = "0 0 0";

   justify = "center";
   
   profileForChildren = EditorButtonRight;
   
   // border color
   borderColor   = "153 153 153"; 
   borderColorHL = "230 230 230";
   borderColorNA = "126 79 37";
};


new GuiControlProfile (GuiFormMenuBarProfile)
{
   opaque = true;
   fontType = "Arial";
   fontSize = 15;
   opaque = true;
   fillColor = "239 237 222";
   fillColorHL = "102 153 204";
   borderColor = "138 134 122";
   borderColorHL = "0 51 153";
   border = 5;
   fontColor = "0 0 0";
   fontColorHL = "255 255 255";
   fontColorNA = "128 128 128";
   fixedExtent = true;
   justify = "center";
   canKeyFocus = false;
   mouseOverSelected = true;
   bitmap = "./images/formmenu";
   hasBitmapArray = true;
};
