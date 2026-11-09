//---------------------------------------------------------------------------------------------
// Archonita - OptionsMenuGui.cs
// Modal Options Dialog for selecting Aspect Ratio (16:9, 16:10, 21:9, 32:9, 4:3),
// Display Resolution, Windowed/Fullscreen Mode, and Audio Mute states.
//---------------------------------------------------------------------------------------------

function OptionsMenuGui::onAdd(%this)
{
   %this.pendingAspect = "16:9";
   %this.pendingResolution = "1280 720";
   %this.pendingFullScreen = false;

   %this.borderProfile = "";
   %this.backdropProfile = "";
   %this.panelProfile = "";
   %this.headerProfile = "";
   %this.sectionProfile = "";
   %this.btnNormalProfile = "";
   %this.btnSelectedProfile = "";
   %this.btnActionProfile = "";

   %this.backdropCtrl = "";
   %this.panelCtrl = "";
   %this.headerLabel = "";
   %this.aspectLabel = "";
   %this.resLabel = "";
   %this.modeLabel = "";
   %this.audioLabel = "";

   for (%i = 0; %i < 5; %i++)
   {
      %this.aspectBtn[%i] = "";
      %this.resBtn[%i] = "";
   }

   %this.btnWindowed = "";
   %this.btnFullscreen = "";
   %this.btnOptSFX = "";
   %this.btnOptMusic = "";
   %this.btnApply = "";
   %this.btnClose = "";

   %this.createProfiles();
   %this.createControls();
}

function OptionsMenuGui::onRemove(%this)
{
   if (isObject(%this.backdropCtrl))
      %this.backdropCtrl.delete();
   if (isObject(%this.btnActionProfile))
      %this.btnActionProfile.delete();
   if (isObject(%this.btnSelectedProfile))
      %this.btnSelectedProfile.delete();
   if (isObject(%this.btnNormalProfile))
      %this.btnNormalProfile.delete();
   if (isObject(%this.sectionProfile))
      %this.sectionProfile.delete();
   if (isObject(%this.headerProfile))
      %this.headerProfile.delete();
   if (isObject(%this.panelProfile))
      %this.panelProfile.delete();
   if (isObject(%this.backdropProfile))
      %this.backdropProfile.delete();
   if (isObject(%this.borderProfile))
      %this.borderProfile.delete();
}

function OptionsMenuGui::createProfiles(%this)
{
   if (!isObject(%this.borderProfile))
   {
      %this.borderProfile = new GuiBorderProfile(OptionsMenuBorderProfile)
      {
         border = 2;
         borderHL = 2;
         borderSL = 2;
         borderNA = 1;
         borderColor = "190 155 85 220";
         borderColorHL = "250 220 135 255";
         borderColorSL = "255 240 175 255";
         borderColorNA = "95 100 110 130";
         underfill = true;
      };
   }

   if (!isObject(%this.backdropProfile))
   {
      %this.backdropProfile = new GuiControlProfile(OptionsMenuBackdropProfile : GuiDefaultProfile)
      {
         fillColor = "0 0 0 175";
         border = false;
         useInput = true;
      };
   }

   if (!isObject(%this.panelProfile))
   {
      %this.panelProfile = new GuiControlProfile(OptionsMenuPanelProfile : GuiDefaultProfile)
      {
         fillColor = "14 18 28 246";
         borderLeft = %this.borderProfile;
         borderRight = %this.borderProfile;
         borderTop = %this.borderProfile;
         borderBottom = %this.borderProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.headerProfile))
   {
      %this.headerProfile = new GuiControlProfile(OptionsMenuHeaderProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 20;
         fontColor = "255 225 140 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.sectionProfile))
   {
      %this.sectionProfile = new GuiControlProfile(OptionsMenuSectionProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 14;
         fontColor = "205 215 235 255";
         align = "left";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.btnNormalProfile))
   {
      %this.btnNormalProfile = new GuiControlProfile(OptionsMenuBtnNormalProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 13;
         fillColor = "26 34 48 230";
         fillColorHL = "42 54 78 245";
         fillColorSL = "58 74 106 255";
         fillColorNA = "20 24 30 140";
         fontColor = "210 218 230 255";
         fontColorHL = "255 240 195 255";
         fontColorSL = "255 255 255 255";
         fontColorNA = "100 105 115 140";
         align = "center";
         vAlign = "middle";
         borderLeft = %this.borderProfile;
         borderRight = %this.borderProfile;
         borderTop = %this.borderProfile;
         borderBottom = %this.borderProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.btnSelectedProfile))
   {
      %this.btnSelectedProfile = new GuiControlProfile(OptionsMenuBtnSelectedProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 13;
         fillColor = "68 54 22 245";
         fillColorHL = "88 70 28 255";
         fillColorSL = "108 86 34 255";
         fillColorNA = "20 24 30 140";
         fontColor = "255 235 145 255";
         fontColorHL = "255 250 205 255";
         fontColorSL = "255 255 255 255";
         fontColorNA = "100 105 115 140";
         align = "center";
         vAlign = "middle";
         borderLeft = %this.borderProfile;
         borderRight = %this.borderProfile;
         borderTop = %this.borderProfile;
         borderBottom = %this.borderProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.btnActionProfile))
   {
      %this.btnActionProfile = new GuiControlProfile(OptionsMenuBtnActionProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 15;
         fillColor = "34 62 46 240";
         fillColorHL = "48 86 64 255";
         fillColorSL = "62 110 82 255";
         fillColorNA = "24 28 34 140";
         fontColor = "185 255 200 255";
         fontColorHL = "230 255 235 255";
         fontColorSL = "255 255 255 255";
         fontColorNA = "110 115 125 150";
         align = "center";
         vAlign = "middle";
         borderLeft = %this.borderProfile;
         borderRight = %this.borderProfile;
         borderTop = %this.borderProfile;
         borderBottom = %this.borderProfile;
         useInput = true;
      };
   }
}

function OptionsMenuGui::createControls(%this)
{
   if (isObject(%this.backdropCtrl))
      %this.backdropCtrl.delete();

   %mainW = 1024;
   %mainH = 768;
   if (isObject(mainScreenGui))
   {
      %mainW = getWord(mainScreenGui.extent, 0);
      %mainH = getWord(mainScreenGui.extent, 1);
   }

   %panelW = 540;
   %panelH = 420;
   %panelX = mFloor((%mainW - %panelW) / 2);
   %panelY = mFloor((%mainH - %panelH) / 2);

   %this.backdropCtrl = new GuiControl(OptionsMenuBackdrop)
   {
      Profile = %this.backdropProfile;
      HorizSizing = "width";
      VertSizing = "height";
      Position = "0 0";
      Extent = %mainW SPC %mainH;
      Visible = "0";
   };

   %this.panelCtrl = new GuiControl(OptionsMenuPanel)
   {
      Profile = %this.panelProfile;
      HorizSizing = "center";
      VertSizing = "center";
      Position = %panelX SPC %panelY;
      Extent = %panelW SPC %panelH;
      Visible = "1";
   };

   %this.headerLabel = new GuiControl()
   {
      Profile = %this.headerProfile;
      Position = "20 14";
      Extent = "500 30";
      Text = "VIDEO & AUDIO OPTIONS";
   };
   %this.panelCtrl.add(%this.headerLabel);

   // 1. Aspect Ratio Section (16:9 vs 16:10 vs 21:9 vs 32:9 vs 4:3)
   %this.aspectLabel = new GuiControl()
   {
      Profile = %this.sectionProfile;
      Position = "24 54";
      Extent = "492 22";
      Text = "Aspect Ratio:";
   };
   %this.panelCtrl.add(%this.aspectLabel);

   %aspectNames = "16:9" TAB "16:10" TAB "21:9" TAB "32:9" TAB "4:3";
   for (%i = 0; %i < 5; %i++)
   {
      %aName = getField(%aspectNames, %i);
      %bx = 24 + (%i * 100);
      %this.aspectBtn[%i] = new GuiButtonCtrl()
      {
         Profile = %this.btnNormalProfile;
         Position = %bx SPC "80";
         Extent = "92 32";
         Text = %aName;
         Command = "$OptionsMenuGui.onSelectAspect(\"" @ %aName @ "\");";
      };
      %this.panelCtrl.add(%this.aspectBtn[%i]);
   }

   // 2. Resolution Section
   %this.resLabel = new GuiControl()
   {
      Profile = %this.sectionProfile;
      Position = "24 124";
      Extent = "492 22";
      Text = "Resolution:";
   };
   %this.panelCtrl.add(%this.resLabel);

   for (%j = 0; %j < 5; %j++)
   {
      %col = %j % 3;
      %row = mFloor(%j / 3);
      %rx = 24 + (%col * 168);
      %ry = 150 + (%row * 38);
      %this.resBtn[%j] = new GuiButtonCtrl()
      {
         Profile = %this.btnNormalProfile;
         Position = %rx SPC %ry;
         Extent = "156 32";
         Text = "";
         Command = "$OptionsMenuGui.onSelectResolutionIndex(" @ %j @ ");";
      };
      %this.panelCtrl.add(%this.resBtn[%j]);
   }

   // 3. Display Mode Section
   %this.modeLabel = new GuiControl()
   {
      Profile = %this.sectionProfile;
      Position = "24 234";
      Extent = "492 22";
      Text = "Display Mode:";
   };
   %this.panelCtrl.add(%this.modeLabel);

   %this.btnWindowed = new GuiButtonCtrl()
   {
      Profile = %this.btnNormalProfile;
      Position = "24 260";
      Extent = "240 32";
      Text = "WINDOWED";
      Command = "$OptionsMenuGui.onSelectFullScreen(0);";
   };
   %this.panelCtrl.add(%this.btnWindowed);

   %this.btnFullscreen = new GuiButtonCtrl()
   {
      Profile = %this.btnNormalProfile;
      Position = "276 260";
      Extent = "240 32";
      Text = "FULLSCREEN";
      Command = "$OptionsMenuGui.onSelectFullScreen(1);";
   };
   %this.panelCtrl.add(%this.btnFullscreen);

   // 4. Audio Mute Section
   %this.audioLabel = new GuiControl()
   {
      Profile = %this.sectionProfile;
      Position = "24 302";
      Extent = "492 22";
      Text = "Audio Controls:";
   };
   %this.panelCtrl.add(%this.audioLabel);

   %this.btnOptSFX = new GuiButtonCtrl()
   {
      Profile = %this.btnNormalProfile;
      Position = "24 328";
      Extent = "240 32";
      Text = "SFX: ON";
      Command = "$OptionsMenuGui.onToggleSFX();";
   };
   %this.panelCtrl.add(%this.btnOptSFX);

   %this.btnOptMusic = new GuiButtonCtrl()
   {
      Profile = %this.btnNormalProfile;
      Position = "276 328";
      Extent = "240 32";
      Text = "MUSIC: ON";
      Command = "$OptionsMenuGui.onToggleMusic();";
   };
   %this.panelCtrl.add(%this.btnOptMusic);

   // 5. Apply & Close Buttons
   %this.btnApply = new GuiButtonCtrl(OptionsMenuApplyBtn)
   {
      Profile = %this.btnActionProfile;
      Position = "24 372";
      Extent = "240 36";
      Text = "APPLY VIDEO SETTINGS";
      Command = "$OptionsMenuGui.onApplyClicked();";
   };
   %this.panelCtrl.add(%this.btnApply);

   %this.btnClose = new GuiButtonCtrl(OptionsMenuCloseBtn)
   {
      Profile = %this.btnNormalProfile;
      Position = "276 372";
      Extent = "240 36";
      Text = "CLOSE";
      Command = "$OptionsMenuGui.onCloseClicked();";
   };
   %this.panelCtrl.add(%this.btnClose);

   %this.backdropCtrl.add(%this.panelCtrl);

   if (isObject(mainScreenGui))
   {
      mainScreenGui.add(%this.backdropCtrl);
      mainScreenGui.pushToBack(%this.backdropCtrl);
   }
}

function OptionsMenuGui::reanchor(%this)
{
   if (!isObject(%this.backdropCtrl) || !isObject(mainScreenGui))
      return;

   %mainW = getWord(mainScreenGui.extent, 0);
   %mainH = getWord(mainScreenGui.extent, 1);
   %this.backdropCtrl.Position = "0 0";
   %this.backdropCtrl.Extent = %mainW SPC %mainH;

   if (isObject(%this.panelCtrl))
   {
      %panelW = getWord(%this.panelCtrl.extent, 0);
      %panelH = getWord(%this.panelCtrl.extent, 1);
      %this.panelCtrl.Position = mFloor((%mainW - %panelW) / 2) SPC mFloor((%mainH - %panelH) / 2);
   }

   if (%this.backdropCtrl.isVisible())
      mainScreenGui.pushToBack(%this.backdropCtrl);
}

function OptionsMenuGui::isVisible(%this)
{
   return (isObject(%this.backdropCtrl) && %this.backdropCtrl.isVisible());
}

function OptionsMenuGui::show(%this, %visible)
{
   if (!isObject(%this.backdropCtrl))
      %this.createControls();

   if (%visible)
   {
      if (isObject($ResolutionManager))
      {
         %this.pendingAspect = $ResolutionManager.currentAspectRatio;
         %this.pendingResolution = $ResolutionManager.currentResolution;
         %this.pendingFullScreen = $ResolutionManager.currentFullScreen;
      }
      %this.updateVisuals();
      %this.reanchor();
      %this.backdropCtrl.setVisible(true);
      mainScreenGui.pushToBack(%this.backdropCtrl);
   }
   else
   {
      %this.backdropCtrl.setVisible(false);
   }
}

function OptionsMenuGui::updateVisuals(%this)
{
   %aspectNames = "16:9" TAB "16:10" TAB "21:9" TAB "32:9" TAB "4:3";
   for (%i = 0; %i < 5; %i++)
   {
      %aName = getField(%aspectNames, %i);
      if (isObject(%this.aspectBtn[%i]))
      {
         %isSel = (%aName $= %this.pendingAspect);
         %this.aspectBtn[%i].Profile = (%isSel ? %this.btnSelectedProfile : %this.btnNormalProfile);
      }
   }

   %resList = "1280 720" TAB "1600 900" TAB "1920 1080" TAB "2560 1440" TAB "3840 2160";
   if (isObject($ResolutionManager))
      %resList = $ResolutionManager.getResolutionsForAspect(%this.pendingAspect);

   %resCount = getFieldCount(%resList);
   for (%j = 0; %j < 5; %j++)
   {
      if (!isObject(%this.resBtn[%j]))
         continue;

      if (%j < %resCount)
      {
         %resPair = getField(%resList, %j);
         %rw = getWord(%resPair, 0);
         %rh = getWord(%resPair, 1);
         %this.resBtn[%j].resValue = %resPair;
         %this.resBtn[%j].setText(%rw @ " x " @ %rh);
         %this.resBtn[%j].setVisible(true);
         %this.resBtn[%j].setActive(true);
         %isResSel = (%resPair $= %this.pendingResolution);
         %this.resBtn[%j].Profile = (%isResSel ? %this.btnSelectedProfile : %this.btnNormalProfile);
      }
      else
      {
         %this.resBtn[%j].resValue = "";
         %this.resBtn[%j].setText("");
         %this.resBtn[%j].setVisible(false);
         %this.resBtn[%j].setActive(false);
      }
   }

   if (isObject(%this.btnWindowed))
      %this.btnWindowed.Profile = (!%this.pendingFullScreen ? %this.btnSelectedProfile : %this.btnNormalProfile);
   if (isObject(%this.btnFullscreen))
      %this.btnFullscreen.Profile = (%this.pendingFullScreen ? %this.btnSelectedProfile : %this.btnNormalProfile);

   %sfxMuted = (isObject($AudioManager) && $AudioManager.isSFXMuted());
   %musMuted = (isObject($AudioManager) && $AudioManager.isMusicMuted());

   if (isObject(%this.btnOptSFX))
   {
      %this.btnOptSFX.setText(%sfxMuted ? "SFX: MUTED" : "SFX: ON");
      %this.btnOptSFX.Profile = (%sfxMuted ? %this.btnNormalProfile : %this.btnSelectedProfile);
   }

   if (isObject(%this.btnOptMusic))
   {
      %this.btnOptMusic.setText(%musMuted ? "MUSIC: MUTED" : "MUSIC: ON");
      %this.btnOptMusic.Profile = (%musMuted ? %this.btnNormalProfile : %this.btnSelectedProfile);
   }
}

function OptionsMenuGui::onSelectAspect(%this, %aspectStr)
{
   playArchonSound(fxSword);
   %this.pendingAspect = %aspectStr;
   if (isObject($ResolutionManager))
   {
      %resList = $ResolutionManager.getResolutionsForAspect(%aspectStr);
      %this.pendingResolution = getField(%resList, 0);
   }
   %this.updateVisuals();
}

function OptionsMenuGui::onSelectResolutionIndex(%this, %idx)
{
   if (!isObject(%this.resBtn[%idx]))
      return;
   %val = %this.resBtn[%idx].resValue;
   if (%val $= "")
      return;

   playArchonSound(fxSword);
   %this.pendingResolution = %val;
   %this.updateVisuals();
}

function OptionsMenuGui::onSelectFullScreen(%this, %fs)
{
   playArchonSound(fxSword);
   %this.pendingFullScreen = (%fs == 1);
   %this.updateVisuals();
}

function OptionsMenuGui::onToggleSFX(%this)
{
   if (isObject($AudioManager))
   {
      $AudioManager.toggleSFXMute();
      if (!$AudioManager.isSFXMuted())
         playArchonSound(fxSword);
   }
   %this.updateVisuals();
}

function OptionsMenuGui::onToggleMusic(%this)
{
   if (isObject($AudioManager))
      $AudioManager.toggleMusicMute();
   %this.updateVisuals();
}

function OptionsMenuGui::onApplyClicked(%this)
{
   playArchonSound(fxSword);
   if (isObject($ResolutionManager))
      $ResolutionManager.applyDisplaySettings(%this.pendingAspect, %this.pendingResolution, %this.pendingFullScreen);
   %this.updateVisuals();
}

function OptionsMenuGui::onCloseClicked(%this)
{
   playArchonSound(fxSword);
   %this.show(false);
}

if (!isObject($OptionsMenuGui))
{
   $OptionsMenuGui = new ScriptObject(OptionsMenuGui) { };
}
