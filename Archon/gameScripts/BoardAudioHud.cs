//---------------------------------------------------------------------------------------------
// Archonita - BoardAudioHud.cs
// Top-right HUD overlay on the Game Board screen for Mute SFX, Mute Music, and Pause Menu.
//---------------------------------------------------------------------------------------------

function BoardAudioHud::onAdd(%this)
{
   %this.borderProfile = "";
   %this.containerProfile = "";
   %this.btnOnProfile = "";
   %this.btnOffProfile = "";
   %this.btnMenuProfile = "";
   %this.hudContainer = "";
   %this.btnLang = "";
   %this.btnPauseMenu = "";
   %this.btnMuteSFX = "";
   %this.btnMuteMusic = "";

   %this.createProfiles();
   %this.createHudControls();
   %this.refreshLocalization();
}

function BoardAudioHud::onRemove(%this)
{
   if (isObject(%this.hudContainer))
      %this.hudContainer.delete();
   if (isObject(%this.btnOnProfile))
      %this.btnOnProfile.delete();
   if (isObject(%this.btnOffProfile))
      %this.btnOffProfile.delete();
   if (isObject(%this.btnMenuProfile))
      %this.btnMenuProfile.delete();
   if (isObject(%this.containerProfile))
      %this.containerProfile.delete();
   if (isObject(%this.borderProfile))
      %this.borderProfile.delete();
}

function BoardAudioHud::createProfiles(%this)
{
   if (!isObject(%this.borderProfile))
   {
      %this.borderProfile = new GuiBorderProfile(BoardHudBorderProfile)
      {
         border = 1;
         borderHL = 1;
         borderSL = 1;
         borderNA = 1;
         borderColor = "180 150 90 190";
         borderColorHL = "240 210 120 255";
         borderColorSL = "255 230 150 255";
         borderColorNA = "100 100 100 120";
         underfill = true;
      };
   }

   if (!isObject(%this.containerProfile))
   {
      %this.containerProfile = new GuiControlProfile(BoardHudContainerProfile : GuiDefaultProfile)
      {
         fillColor = "12 16 24 185";
         borderLeft = %this.borderProfile;
         borderRight = %this.borderProfile;
         borderTop = %this.borderProfile;
         borderBottom = %this.borderProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.btnOnProfile))
   {
      %this.btnOnProfile = new GuiControlProfile(BoardHudBtnOnProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 13;
         fillColor = "24 46 34 230";
         fillColorHL = "36 68 50 245";
         fillColorSL = "48 90 66 255";
         fillColorNA = "25 25 25 140";
         fontColor = "150 255 165 255";
         fontColorHL = "210 255 215 255";
         fontColorSL = "255 255 255 255";
         fontColorNA = "120 120 120 160";
         align = "center";
         vAlign = "middle";
         borderLeft = %this.borderProfile;
         borderRight = %this.borderProfile;
         borderTop = %this.borderProfile;
         borderBottom = %this.borderProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.btnOffProfile))
   {
      %this.btnOffProfile = new GuiControlProfile(BoardHudBtnOffProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 13;
         fillColor = "56 24 28 230";
         fillColorHL = "82 34 40 245";
         fillColorSL = "104 42 48 255";
         fillColorNA = "25 25 25 140";
         fontColor = "255 135 135 255";
         fontColorHL = "255 185 185 255";
         fontColorSL = "255 220 220 255";
         fontColorNA = "120 120 120 160";
         align = "center";
         vAlign = "middle";
         borderLeft = %this.borderProfile;
         borderRight = %this.borderProfile;
         borderTop = %this.borderProfile;
         borderBottom = %this.borderProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.btnMenuProfile))
   {
      %this.btnMenuProfile = new GuiControlProfile(BoardHudBtnMenuProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 13;
         fillColor = "30 38 54 230";
         fillColorHL = "46 58 82 245";
         fillColorSL = "64 80 110 255";
         fillColorNA = "25 25 25 140";
         fontColor = "240 215 145 255";
         fontColorHL = "255 240 190 255";
         fontColorSL = "255 255 255 255";
         fontColorNA = "120 120 120 160";
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

function BoardAudioHud::createHudControls(%this)
{
   if (isObject(%this.hudContainer))
      %this.hudContainer.delete();

   %mainW = 1024;
   if (isObject(mainScreenGui))
      %mainW = getWord(mainScreenGui.extent, 0);

   %hudW = 348;
   %hudH = 34;
   %posX = %mainW - %hudW - 12;
   %posY = 10;

   %this.hudContainer = new GuiControl(BoardAudioHudContainer)
   {
      Profile = %this.containerProfile;
      HorizSizing = "left";
      VertSizing = "bottom";
      Position = %posX SPC %posY;
      Extent = %hudW SPC %hudH;
      MinExtent = "8 8";
      Visible = "0";
   };

   %this.btnLang = new GuiButtonCtrl(BoardHudLangBtn)
   {
      Profile = %this.btnMenuProfile;
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "4 4";
      Extent = "58 26";
      Text = "FI";
      Command = "$BoardAudioHud.onCycleLanguage();";
   };

   %this.btnPauseMenu = new GuiButtonCtrl(BoardHudMenuBtn)
   {
      Profile = %this.btnMenuProfile;
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "66 4";
      Extent = "68 26";
      Text = "MENU";
      Command = "$BoardAudioHud.onMenuClicked();";
   };

   %this.btnMuteSFX = new GuiButtonCtrl(BoardHudMuteSFXBtn)
   {
      Profile = %this.btnOnProfile;
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "138 4";
      Extent = "100 26";
      Text = "SFX: ON";
      Command = "$BoardAudioHud.onToggleSFX();";
   };

   %this.btnMuteMusic = new GuiButtonCtrl(BoardHudMuteMusicBtn)
   {
      Profile = %this.btnOnProfile;
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "242 4";
      Extent = "102 26";
      Text = "MUSIC: ON";
      Command = "$BoardAudioHud.onToggleMusic();";
   };

   %this.hudContainer.add(%this.btnLang);
   %this.hudContainer.add(%this.btnPauseMenu);
   %this.hudContainer.add(%this.btnMuteSFX);
   %this.hudContainer.add(%this.btnMuteMusic);

   if (isObject(mainScreenGui))
   {
      mainScreenGui.add(%this.hudContainer);
      mainScreenGui.pushToBack(%this.hudContainer);
   }
}

function BoardAudioHud::reanchor(%this)
{
   if (!isObject(%this.hudContainer) || !isObject(mainScreenGui))
      return;

   %scale = isObject($ResolutionManager) ? $ResolutionManager.getViewportSafeUiScale(348, 34, 24) : 1.0;
   if (isObject($ResolutionManager))
      $ResolutionManager.applyUiScaleToTree(%this.hudContainer, %scale, false);

   %mainW = getWord(mainScreenGui.extent, 0);
   %hudW = getWord(%this.hudContainer.extent, 0);
   %marginX = mFloor(12 * %scale + 0.5);
   %marginY = mFloor(10 * %scale + 0.5);
   %posX = %mainW - %hudW - %marginX;
   if (%posX < 8) %posX = 8;
   %this.hudContainer.Position = %posX SPC %marginY;
   mainScreenGui.pushToBack(%this.hudContainer);
}

function BoardAudioHud::show(%this, %visible)
{
   if (!isObject(%this.hudContainer))
      %this.createHudControls();

   if (isObject(%this.hudContainer))
   {
      if (%visible)
      {
         %this.reanchor();
         %this.updateVisuals();
      }
      %this.hudContainer.setVisible(%visible);
   }
}

function BoardAudioHud::refreshLocalization(%this)
{
   if (isObject(%this.btnLang) && isObject($LocalizationManager))
      %this.btnLang.setText($LocalizationManager.getLanguageShortCode());
   if (isObject(%this.btnPauseMenu) && isObject($LocalizationManager))
      %this.btnPauseMenu.setText($LocalizationManager.getText("hud_menu"));

   %this.updateVisuals();
}

function BoardAudioHud::updateVisuals(%this)
{
   %sfxMuted = (isObject($AudioManager) && $AudioManager.isSFXMuted());
   %musicMuted = (isObject($AudioManager) && $AudioManager.isMusicMuted());

   %sfxOnText = isObject($LocalizationManager) ? $LocalizationManager.getText("hud_sfx_on") : "SFX: ON";
   %sfxMutedText = isObject($LocalizationManager) ? $LocalizationManager.getText("hud_sfx_muted") : "SFX: MUTED";
   %musOnText = isObject($LocalizationManager) ? $LocalizationManager.getText("hud_music_on") : "MUSIC: ON";
   %musMutedText = isObject($LocalizationManager) ? $LocalizationManager.getText("hud_music_muted") : "MUSIC: MUTED";

   if (isObject(%this.btnMuteSFX))
   {
      %this.btnMuteSFX.setText(%sfxMuted ? %sfxMutedText : %sfxOnText);
      %this.btnMuteSFX.Profile = (%sfxMuted ? %this.btnOffProfile : %this.btnOnProfile);
   }

   if (isObject(%this.btnMuteMusic))
   {
      %this.btnMuteMusic.setText(%musicMuted ? %musMutedText : %musOnText);
      %this.btnMuteMusic.Profile = (%musicMuted ? %this.btnOffProfile : %this.btnOnProfile);
   }
}

function BoardAudioHud::onCycleLanguage(%this)
{
   playArchonSound(fxSword);
   if (isObject($LocalizationManager))
      $LocalizationManager.cycleLanguage(1);
}

function BoardAudioHud::onToggleSFX(%this)
{
   if (isObject($AudioManager))
   {
      $AudioManager.toggleSFXMute();
      if (!$AudioManager.isSFXMuted())
         playArchonSound(fxSword);
   }
   %this.updateVisuals();
}

function BoardAudioHud::onToggleMusic(%this)
{
   if (isObject($AudioManager))
      $AudioManager.toggleMusicMute();
   %this.updateVisuals();
}

function BoardAudioHud::onMenuClicked(%this)
{
   playArchonSound(fxSword);
   if (isObject($PauseMenuManager))
      $PauseMenuManager.togglePauseMenu();
}

if (!isObject($BoardAudioHud))
{
   $BoardAudioHud = new ScriptObject(BoardAudioHud) { };
}
