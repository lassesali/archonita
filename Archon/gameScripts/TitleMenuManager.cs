//---------------------------------------------------------------------------------------------
// Archonita - TitleMenuManager.cs
// Manages the Title Screen GUI menu ("Play", "Resume Game", "Options", "Credits", "Quit")
// and new-game overwrite confirmation modal.
//---------------------------------------------------------------------------------------------

function TitleMenuManager::onAdd(%this)
{
   %this.borderProfile = "";
   %this.panelProfile = "";
   %this.headerProfile = "";
   %this.subTextProfile = "";
   %this.btnPrimaryProfile = "";
   %this.btnResumeProfile = "";
   %this.btnSecondaryProfile = "";
   %this.btnDisabledProfile = "";
   %this.modalBackdropProfile = "";

   %this.menuPanel = "";
   %this.btnPlay = "";
   %this.btnResume = "";
   %this.btnOptions = "";
   %this.btnCredits = "";
   %this.btnQuit = "";

   %this.confirmBackdrop = "";
   %this.confirmPanel = "";
   %this.confirmTitle = "";
   %this.confirmText = "";
   %this.btnConfirmYes = "";
   %this.btnConfirmNo = "";

   %this.createProfiles();
   %this.createControls();
   %this.updateVisuals();
}

function TitleMenuManager::onRemove(%this)
{
   if (isObject(%this.confirmBackdrop))
      %this.confirmBackdrop.delete();
   if (isObject(%this.menuPanel))
      %this.menuPanel.delete();
   if (isObject(%this.modalBackdropProfile))
      %this.modalBackdropProfile.delete();
   if (isObject(%this.btnDisabledProfile))
      %this.btnDisabledProfile.delete();
   if (isObject(%this.btnSecondaryProfile))
      %this.btnSecondaryProfile.delete();
   if (isObject(%this.btnResumeProfile))
      %this.btnResumeProfile.delete();
   if (isObject(%this.btnPrimaryProfile))
      %this.btnPrimaryProfile.delete();
   if (isObject(%this.subTextProfile))
      %this.subTextProfile.delete();
   if (isObject(%this.headerProfile))
      %this.headerProfile.delete();
   if (isObject(%this.panelProfile))
      %this.panelProfile.delete();
   if (isObject(%this.borderProfile))
      %this.borderProfile.delete();
}

function TitleMenuManager::createProfiles(%this)
{
   if (!isObject(%this.borderProfile))
   {
      %this.borderProfile = new GuiBorderProfile(TitleMenuBorderProfile)
      {
         border = 2;
         borderHL = 2;
         borderSL = 2;
         borderNA = 1;
         borderColor = "190 155 85 215";
         borderColorHL = "250 220 135 255";
         borderColorSL = "255 240 175 255";
         borderColorNA = "90 95 105 130";
         underfill = true;
      };
   }

   if (!isObject(%this.panelProfile))
   {
      %this.panelProfile = new GuiControlProfile(TitleMenuPanelProfile : GuiDefaultProfile)
      {
         fillColor = "14 18 28 215";
         borderLeft = %this.borderProfile;
         borderRight = %this.borderProfile;
         borderTop = %this.borderProfile;
         borderBottom = %this.borderProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.modalBackdropProfile))
   {
      %this.modalBackdropProfile = new GuiControlProfile(TitleMenuModalBackdropProfile : GuiDefaultProfile)
      {
         fillColor = "0 0 0 165";
         border = false;
         useInput = true;
      };
   }

   if (!isObject(%this.headerProfile))
   {
      %this.headerProfile = new GuiControlProfile(TitleMenuHeaderProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 18;
         fontColor = "255 225 140 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.subTextProfile))
   {
      %this.subTextProfile = new GuiControlProfile(TitleMenuSubTextProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 13;
         fontColor = "225 230 240 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.btnPrimaryProfile))
   {
      %this.btnPrimaryProfile = new GuiControlProfile(TitleMenuPrimaryBtnProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 16;
         fillColor = "42 54 80 240";
         fillColorHL = "64 82 118 255";
         fillColorSL = "84 108 150 255";
         fillColorNA = "26 30 38 140";
         fontColor = "255 235 165 255";
         fontColorHL = "255 250 215 255";
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

   if (!isObject(%this.btnResumeProfile))
   {
      %this.btnResumeProfile = new GuiControlProfile(TitleMenuResumeBtnProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 16;
         fillColor = "28 62 44 240";
         fillColorHL = "42 90 64 255";
         fillColorSL = "56 116 82 255";
         fillColorNA = "24 28 34 140";
         fontColor = "165 255 185 255";
         fontColorHL = "220 255 228 255";
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

   if (!isObject(%this.btnSecondaryProfile))
   {
      %this.btnSecondaryProfile = new GuiControlProfile(TitleMenuSecondaryBtnProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 15;
         fillColor = "28 36 52 230";
         fillColorHL = "46 58 84 245";
         fillColorSL = "62 78 110 255";
         fillColorNA = "24 28 34 140";
         fontColor = "230 235 245 255";
         fontColorHL = "255 245 205 255";
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

   if (!isObject(%this.btnDisabledProfile))
   {
      %this.btnDisabledProfile = new GuiControlProfile(TitleMenuDisabledBtnProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 15;
         fillColor = "22 25 32 160";
         fillColorHL = "22 25 32 160";
         fillColorSL = "22 25 32 160";
         fillColorNA = "22 25 32 160";
         fontColor = "105 110 120 155";
         fontColorHL = "105 110 120 155";
         fontColorSL = "105 110 120 155";
         fontColorNA = "105 110 120 155";
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

function TitleMenuManager::createControls(%this)
{
   if (isObject(%this.menuPanel))
      %this.menuPanel.delete();
   if (isObject(%this.confirmBackdrop))
      %this.confirmBackdrop.delete();

   %mainW = 1024;
   %mainH = 768;
   if (isObject(mainScreenGui))
   {
      %mainW = getWord(mainScreenGui.extent, 0);
      %mainH = getWord(mainScreenGui.extent, 1);
   }

   %panelW = 290;
   %panelH = 276;
   %panelX = mFloor(%mainW * 0.56 - (%panelW / 2));
   %panelY = mFloor(%mainH * 0.56 - (%panelH / 2));

   %this.menuPanel = new GuiControl(TitleMenuPanel)
   {
      Profile = %this.panelProfile;
      HorizSizing = "center";
      VertSizing = "center";
      Position = %panelX SPC %panelY;
      Extent = %panelW SPC %panelH;
      Visible = "0";
   };

   %this.btnPlay = new GuiButtonCtrl(TitleMenuPlayBtn)
   {
      Profile = %this.btnPrimaryProfile;
      HorizSizing = "width";
      VertSizing = "bottom";
      Position = "20 18";
      Extent = "250 42";
      Text = "PLAY";
      Command = "$TitleMenuManager.onPlayClicked();";
   };

   %this.btnResume = new GuiButtonCtrl(TitleMenuResumeBtn)
   {
      Profile = %this.btnDisabledProfile;
      HorizSizing = "width";
      VertSizing = "bottom";
      Position = "20 70";
      Extent = "250 42";
      Text = "RESUME GAME";
      Active = "0";
      Command = "$TitleMenuManager.onResumeClicked();";
   };

   %this.btnOptions = new GuiButtonCtrl(TitleMenuOptionsBtn)
   {
      Profile = %this.btnSecondaryProfile;
      HorizSizing = "width";
      VertSizing = "bottom";
      Position = "20 122";
      Extent = "250 38";
      Text = "OPTIONS";
      Command = "$TitleMenuManager.onOptionsClicked();";
   };

   %this.btnCredits = new GuiButtonCtrl(TitleMenuCreditsBtn)
   {
      Profile = %this.btnSecondaryProfile;
      HorizSizing = "width";
      VertSizing = "bottom";
      Position = "20 170";
      Extent = "250 38";
      Text = "CREDITS";
      Command = "$TitleMenuManager.onCreditsClicked();";
   };

   %this.btnQuit = new GuiButtonCtrl(TitleMenuQuitBtn)
   {
      Profile = %this.btnSecondaryProfile;
      HorizSizing = "width";
      VertSizing = "bottom";
      Position = "20 218";
      Extent = "250 38";
      Text = "QUIT";
      Command = "$TitleMenuManager.onQuitClicked();";
   };

   %this.menuPanel.add(%this.btnPlay);
   %this.menuPanel.add(%this.btnResume);
   %this.menuPanel.add(%this.btnOptions);
   %this.menuPanel.add(%this.btnCredits);
   %this.menuPanel.add(%this.btnQuit);

   // Confirmation Modal for starting a New Game when a session is active
   %cW = 400;
   %cH = 176;
   %cX = mFloor((%mainW - %cW) / 2);
   %cY = mFloor((%mainH - %cH) / 2);

   %this.confirmBackdrop = new GuiControl(TitleMenuConfirmBackdrop)
   {
      Profile = %this.modalBackdropProfile;
      HorizSizing = "width";
      VertSizing = "height";
      Position = "0 0";
      Extent = %mainW SPC %mainH;
      Visible = "0";
   };

   %this.confirmPanel = new GuiControl(TitleMenuConfirmPanel)
   {
      Profile = %this.panelProfile;
      HorizSizing = "center";
      VertSizing = "center";
      Position = %cX SPC %cY;
      Extent = %cW SPC %cH;
      Visible = "1";
   };

   %this.confirmTitle = new GuiControl()
   {
      Profile = %this.headerProfile;
      HorizSizing = "width";
      VertSizing = "bottom";
      Position = "16 14";
      Extent = "368 28";
      Text = "START NEW GAME?";
   };

   %this.confirmText = new GuiControl()
   {
      Profile = %this.subTextProfile;
      HorizSizing = "width";
      VertSizing = "bottom";
      Position = "16 52";
      Extent = "368 44";
      Text = "A game is in progress. Starting a new game will discard it.";
   };

   %this.btnConfirmYes = new GuiButtonCtrl(TitleConfirmYesBtn)
   {
      Profile = %this.btnPrimaryProfile;
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "24 116";
      Extent = "168 38";
      Text = "NEW GAME";
      Command = "$TitleMenuManager.onConfirmNewGame();";
   };

   %this.btnConfirmNo = new GuiButtonCtrl(TitleConfirmNoBtn)
   {
      Profile = %this.btnSecondaryProfile;
      HorizSizing = "left";
      VertSizing = "bottom";
      Position = "208 116";
      Extent = "168 38";
      Text = "CANCEL";
      Command = "$TitleMenuManager.onCancelNewGame();";
   };

   %this.confirmPanel.add(%this.confirmTitle);
   %this.confirmPanel.add(%this.confirmText);
   %this.confirmPanel.add(%this.btnConfirmYes);
   %this.confirmPanel.add(%this.btnConfirmNo);
   %this.confirmBackdrop.add(%this.confirmPanel);

   if (isObject(mainScreenGui))
   {
      mainScreenGui.add(%this.menuPanel);
      mainScreenGui.add(%this.confirmBackdrop);
      mainScreenGui.pushToBack(%this.menuPanel);
      mainScreenGui.pushToBack(%this.confirmBackdrop);
   }
}

function TitleMenuManager::reanchor(%this)
{
   if (!isObject(mainScreenGui))
      return;

   %mainW = getWord(mainScreenGui.extent, 0);
   %mainH = getWord(mainScreenGui.extent, 1);

   if (isObject(%this.menuPanel))
   {
      %panelW = getWord(%this.menuPanel.extent, 0);
      %panelH = getWord(%this.menuPanel.extent, 1);
      %panelX = mFloor(%mainW * 0.56 - (%panelW / 2));
      %panelY = mFloor(%mainH * 0.56 - (%panelH / 2));
      %this.menuPanel.Position = %panelX SPC %panelY;
      if (%this.menuPanel.isVisible())
         mainScreenGui.pushToBack(%this.menuPanel);
   }

   if (isObject(%this.confirmBackdrop))
   {
      %this.confirmBackdrop.Position = "0 0";
      %this.confirmBackdrop.Extent = %mainW SPC %mainH;
      if (isObject(%this.confirmPanel))
      {
         %cW = getWord(%this.confirmPanel.extent, 0);
         %cH = getWord(%this.confirmPanel.extent, 1);
         %this.confirmPanel.Position = mFloor((%mainW - %cW) / 2) SPC mFloor((%mainH - %cH) / 2);
      }
      if (%this.confirmBackdrop.isVisible())
         mainScreenGui.pushToBack(%this.confirmBackdrop);
   }
}

function TitleMenuManager::updateVisuals(%this)
{
   %hasSession = (isObject($PauseMenuManager) && $PauseMenuManager.hasSavedSession());

   if (isObject(%this.btnResume))
   {
      %this.btnResume.setActive(%hasSession);
      %this.btnResume.Profile = (%hasSession ? %this.btnResumeProfile : %this.btnDisabledProfile);
      %this.btnResume.setText(%hasSession ? "RESUME GAME" : "RESUME GAME (NONE)");
   }
}

function TitleMenuManager::show(%this, %visible)
{
   if (!isObject(%this.menuPanel))
      %this.createControls();

   if (isObject(%this.confirmBackdrop))
      %this.confirmBackdrop.setVisible(false);

   if (isObject(%this.menuPanel))
   {
      if (%visible)
      {
         %this.updateVisuals();
         %this.reanchor();
      }
      %this.menuPanel.setVisible(%visible);
   }
}

function TitleMenuManager::onPlayClicked(%this)
{
   playArchonSound(fxSword);

   if (isObject($PauseMenuManager) && $PauseMenuManager.hasSavedSession())
   {
      if (isObject(%this.confirmBackdrop))
      {
         %this.reanchor();
         %this.confirmBackdrop.setVisible(true);
         mainScreenGui.pushToBack(%this.confirmBackdrop);
      }
      return;
   }

   %this.show(false);
   gotoSelectHeroScreen();
}

function TitleMenuManager::onConfirmNewGame(%this)
{
   playArchonSound(fxSword);
   if (isObject(%this.confirmBackdrop))
      %this.confirmBackdrop.setVisible(false);

   if (isObject($PauseMenuManager))
      $PauseMenuManager.discardSavedSession();

   %this.show(false);
   gotoSelectHeroScreen();
}

function TitleMenuManager::onCancelNewGame(%this)
{
   playArchonSound(fxSword);
   if (isObject(%this.confirmBackdrop))
      %this.confirmBackdrop.setVisible(false);
}

function TitleMenuManager::onResumeClicked(%this)
{
   if (!isObject($PauseMenuManager) || !$PauseMenuManager.hasSavedSession())
      return;

   playArchonSound(fxSword);
   $PauseMenuManager.resumeToBoardFromTitle();
}

function TitleMenuManager::onOptionsClicked(%this)
{
   playArchonSound(fxSword);
   if (isObject($OptionsMenuGui))
      $OptionsMenuGui.show(true);
}

function TitleMenuManager::onCreditsClicked(%this)
{
   playArchonSound(fxSword);
   %this.show(false);
   creditsScreen();
}

function TitleMenuManager::onQuitClicked(%this)
{
   playArchonSound(fxSword);
   %this.show(false);
   if (isObject($PauseMenuManager))
      $PauseMenuManager.discardSavedSession();
   creditsScreen();
}

if (!isObject($TitleMenuManager))
{
   $TitleMenuManager = new ScriptObject(TitleMenuManager) { };
}
