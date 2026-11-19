//---------------------------------------------------------------------------------------------
// Archonita - SceneTextOverlayGui.cs
// Dynamically renders localized scene banners (Select Hero, Place Creatures, Countdown),
// hero & spell card text (titles, descriptions, stat modifiers), and Credits labels at runtime.
// Replaces all burned-in text previously baked into text.png, texts.png, cards.png,
// cards_2.png, and credits.png.
// Conforms strictly to TORQUE_SCRIPT.md: single class ScriptObject, onAdd/onRemove lifecycle.
//---------------------------------------------------------------------------------------------

function SceneTextOverlayGui::init(%this)
{
   %this.maxCardSlots = 8;
   %this.tickInterval = 60;
   %this.updateSchedule = 0;

   %this.layerProfile = "";
   %this.bannerShadowProfile = "";
   %this.bannerTextProfile = "";

   %this.cardTitleLargeProfile = "";
   %this.cardTitleMedProfile = "";
   %this.cardTitleSmallProfile = "";
   %this.cardDescLargeProfile = "";
   %this.cardDescMedProfile = "";
   %this.cardDescSmallProfile = "";
   %this.cardStatMedProfile = "";
   %this.cardStatSmallProfile = "";

   %this.creditsTitleProfile = "";
   %this.creditsTitleShadowProfile = "";
   %this.creditsRoleProfile = "";
   %this.creditsRoleShadowProfile = "";

   %this.overlayRoot = "";
   %this.bannerShadow = "";
   %this.bannerLabel = "";
   %this.creditsContainer = "";
   %this.creditsTitleShadow = "";
   %this.creditsTitleLabel = "";

   for (%i = 0; %i < 7; %i++)
   {
      %this.creditsRoleShadow[%i] = "";
      %this.creditsRoleLabel[%i] = "";
   }

   for (%s = 0; %s < %this.maxCardSlots; %s++)
   {
      %this.cardSlotCtrl[%s] = "";
      %this.cardTitleCtrl[%s] = "";
      %this.cardBodyCtrl[%s] = "";
   }

   %this.createProfiles();
   %this.createControls();
}

function SceneTextOverlayGui::onAdd(%this)
{
   %this.init();
}

function SceneTextOverlayGui::onRemove(%this)
{
   %this.stopTracking();

   if (isObject(%this.overlayRoot))
      %this.overlayRoot.delete();

   if (isObject(%this.creditsRoleShadowProfile))
      %this.creditsRoleShadowProfile.delete();
   if (isObject(%this.creditsRoleProfile))
      %this.creditsRoleProfile.delete();
   if (isObject(%this.creditsTitleShadowProfile))
      %this.creditsTitleShadowProfile.delete();
   if (isObject(%this.creditsTitleProfile))
      %this.creditsTitleProfile.delete();

   if (isObject(%this.cardStatSmallProfile))
      %this.cardStatSmallProfile.delete();
   if (isObject(%this.cardStatMedProfile))
      %this.cardStatMedProfile.delete();
   if (isObject(%this.cardDescSmallProfile))
      %this.cardDescSmallProfile.delete();
   if (isObject(%this.cardDescMedProfile))
      %this.cardDescMedProfile.delete();
   if (isObject(%this.cardDescLargeProfile))
      %this.cardDescLargeProfile.delete();
   if (isObject(%this.cardTitleSmallProfile))
      %this.cardTitleSmallProfile.delete();
   if (isObject(%this.cardTitleMedProfile))
      %this.cardTitleMedProfile.delete();
   if (isObject(%this.cardTitleLargeProfile))
      %this.cardTitleLargeProfile.delete();

   if (isObject(%this.bannerTextProfile))
      %this.bannerTextProfile.delete();
   if (isObject(%this.bannerShadowProfile))
      %this.bannerShadowProfile.delete();
   if (isObject(%this.layerProfile))
      %this.layerProfile.delete();
}

function SceneTextOverlayGui::createProfiles(%this)
{
   if (!isObject(%this.layerProfile))
   {
      %this.layerProfile = new GuiControlProfile(SceneTextLayerProfile : GuiDefaultProfile)
      {
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.bannerShadowProfile))
   {
      %this.bannerShadowProfile = new GuiControlProfile(SceneTextBannerShadowProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 24;
         fontColor = "0 0 0 235";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.bannerTextProfile))
   {
      %this.bannerTextProfile = new GuiControlProfile(SceneTextBannerProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 24;
         fontColor = "255 255 255 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.cardTitleLargeProfile))
   {
      %this.cardTitleLargeProfile = new GuiControlProfile(SceneCardTitleLargeProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 18;
         fontColor = "18 18 22 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.cardTitleMedProfile))
   {
      %this.cardTitleMedProfile = new GuiControlProfile(SceneCardTitleMedProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 10;
         fontColor = "18 18 22 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.cardTitleSmallProfile))
   {
      %this.cardTitleSmallProfile = new GuiControlProfile(SceneCardTitleSmallProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 9;
         fontColor = "18 18 22 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.cardDescLargeProfile))
   {
      %this.cardDescLargeProfile = new GuiControlProfile(SceneCardDescLargeProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 15;
         fontColor = "20 22 26 255";
         align = "left";
         vAlign = "top";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.cardDescMedProfile))
   {
      %this.cardDescMedProfile = new GuiControlProfile(SceneCardDescMedProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 10;
         fontColor = "20 22 26 255";
         align = "left";
         vAlign = "top";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.cardDescSmallProfile))
   {
      %this.cardDescSmallProfile = new GuiControlProfile(SceneCardDescSmallProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 9;
         fontColor = "20 22 26 255";
         align = "left";
         vAlign = "top";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.cardStatMedProfile))
   {
      %this.cardStatMedProfile = new GuiControlProfile(SceneCardStatMedProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 10;
         fontColor = "12 14 18 255";
         align = "left";
         vAlign = "bottom";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.cardStatSmallProfile))
   {
      %this.cardStatSmallProfile = new GuiControlProfile(SceneCardStatSmallProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 9;
         fontColor = "12 14 18 255";
         align = "left";
         vAlign = "bottom";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.creditsTitleShadowProfile))
   {
      %this.creditsTitleShadowProfile = new GuiControlProfile(SceneCreditsTitleShadowProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 28;
         fontColor = "45 95 210 220";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.creditsTitleProfile))
   {
      %this.creditsTitleProfile = new GuiControlProfile(SceneCreditsTitleProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 28;
         fontColor = "240 248 255 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.creditsRoleShadowProfile))
   {
      %this.creditsRoleShadowProfile = new GuiControlProfile(SceneCreditsRoleShadowProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 18;
         fontColor = "25 35 65 220";
         align = "left";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.creditsRoleProfile))
   {
      %this.creditsRoleProfile = new GuiControlProfile(SceneCreditsRoleProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 18;
         fontColor = "255 255 255 255";
         align = "left";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }
}

function SceneTextOverlayGui::createControls(%this)
{
   if (isObject(%this.overlayRoot))
      %this.overlayRoot.delete();

   %mainW = 1024;
   %mainH = 768;
   if (isObject(mainScreenGui))
   {
      %mainW = getWord(mainScreenGui.extent, 0);
      %mainH = getWord(mainScreenGui.extent, 1);
   }

   %this.overlayRoot = new GuiControl(SceneTextOverlayRoot)
   {
      Profile = %this.layerProfile;
      HorizSizing = "width";
      VertSizing = "height";
      Position = "0 0";
      Extent = %mainW SPC %mainH;
      Visible = "1";
      useInput = false;
      isContainer = "1";
   };

   // Top Scene Banner
   %this.bannerShadow = new GuiControl(SceneTextBannerShadow)
   {
      Profile = %this.bannerShadowProfile;
      Position = "0 18";
      Extent = %mainW SPC "44";
      Visible = "0";
      useInput = false;
      text = "";
   };
   %this.overlayRoot.add(%this.bannerShadow);

   %this.bannerLabel = new GuiControl(SceneTextBannerLabel)
   {
      Profile = %this.bannerTextProfile;
      Position = "0 16";
      Extent = %mainW SPC "44";
      Visible = "0";
      useInput = false;
      text = "";
   };
   %this.overlayRoot.add(%this.bannerLabel);

   // Dynamic Card Slots
   for (%s = 0; %s < %this.maxCardSlots; %s++)
   {
      %this.cardSlotCtrl[%s] = new GuiControl("SceneCardOverlaySlot" @ %s)
      {
         Profile = %this.layerProfile;
         Position = "0 0";
         Extent = "128 128";
         Visible = "0";
         useInput = false;
         isContainer = "1";
      };

      %this.cardTitleCtrl[%s] = new GuiControl("SceneCardOverlayTitle" @ %s)
      {
         Profile = %this.cardTitleMedProfile;
         Position = "0 0";
         Extent = "100 18";
         useInput = false;
         text = "";
      };

      %this.cardBodyCtrl[%s] = new GuiControl("SceneCardOverlayBody" @ %s)
      {
         Profile = %this.cardDescMedProfile;
         Position = "0 0";
         Extent = "100 60";
         textWrap = "1";
         useInput = false;
         text = "";
      };

      %this.cardSlotCtrl[%s].add(%this.cardTitleCtrl[%s]);
      %this.cardSlotCtrl[%s].add(%this.cardBodyCtrl[%s]);
      %this.overlayRoot.add(%this.cardSlotCtrl[%s]);
   }

   // Credits Screen Labels
   %this.creditsContainer = new GuiControl(SceneCreditsOverlayContainer)
   {
      Profile = %this.layerProfile;
      Position = "0 0";
      Extent = %mainW SPC %mainH;
      Visible = "0";
      useInput = false;
      isContainer = "1";
   };

   %this.creditsTitleShadow = new GuiControl(SceneCreditsTitleShadow)
   {
      Profile = %this.creditsTitleShadowProfile;
      Position = "0 20";
      Extent = %mainW SPC "48";
      useInput = false;
      text = "";
   };
   %this.creditsContainer.add(%this.creditsTitleShadow);

   %this.creditsTitleLabel = new GuiControl(SceneCreditsTitleLabel)
   {
      Profile = %this.creditsTitleProfile;
      Position = "0 18";
      Extent = %mainW SPC "48";
      useInput = false;
      text = "";
   };
   %this.creditsContainer.add(%this.creditsTitleLabel);

   for (%i = 0; %i < 7; %i++)
   {
      %this.creditsRoleShadow[%i] = new GuiControl("SceneCreditsRoleShadow" @ %i)
      {
         Profile = %this.creditsRoleShadowProfile;
         Position = "100 100";
         Extent = "360 34";
         useInput = false;
         text = "";
      };
      %this.creditsContainer.add(%this.creditsRoleShadow[%i]);

      %this.creditsRoleLabel[%i] = new GuiControl("SceneCreditsRoleLabel" @ %i)
      {
         Profile = %this.creditsRoleProfile;
         Position = "100 100";
         Extent = "360 34";
         useInput = false;
         text = "";
      };
      %this.creditsContainer.add(%this.creditsRoleLabel[%i]);
   }

   %this.overlayRoot.add(%this.creditsContainer);

   if (isObject(mainScreenGui))
      mainScreenGui.add(%this.overlayRoot);
}

//---------------------------------------------------------------------------------------------
// Lifecycle & Z-Order Synchronization
//---------------------------------------------------------------------------------------------
function SceneTextOverlayGui::startTracking(%this)
{
   %this.refreshAll();
   if (!isEventPending(%this.updateSchedule))
      %this.updateSchedule = %this.schedule(%this.tickInterval, "onUpdateTick");
}

function SceneTextOverlayGui::stopTracking(%this)
{
   if (isEventPending(%this.updateSchedule))
   {
      cancel(%this.updateSchedule);
      %this.updateSchedule = 0;
   }
}

function SceneTextOverlayGui::onUpdateTick(%this)
{
   %this.updateSchedule = 0;
   %this.refreshAll();

   %state = isObject(game) ? game.getState() : "";
   %needsLoop = false;
   if (%state !$= "")
   {
      if (%state == $gameCardSelectState ||
          %state == $gamePreBattleState ||
          %state == $gamePreBattleTimerState ||
          %state == $gameTimerBattleState ||
          %state == $gameBattleState ||
          %state == $gameCreditsState)
      {
         %needsLoop = true;
      }
   }

   if (%needsLoop)
      %this.updateSchedule = %this.schedule(%this.tickInterval, "onUpdateTick");
}

function SceneTextOverlayGui::isModalBlocking(%this)
{
   if ((isObject($PauseMenuManager) && $PauseMenuManager.isPaused) ||
       (isObject($OptionsMenuGui) && $OptionsMenuGui.isVisible()) ||
       (isObject($FactionSelectGui) && $FactionSelectGui.isVisible()) ||
       (isObject($BattleSummaryGui) && $BattleSummaryGui.isVisible()))
   {
      return true;
   }
   return false;
}

function SceneTextOverlayGui::syncZOrder(%this)
{
   if (!isObject(mainScreenGui) || !isObject(%this.overlayRoot))
      return;

   if (!mainScreenGui.isMember(%this.overlayRoot))
      mainScreenGui.add(%this.overlayRoot);

   mainScreenGui.pushToBack(%this.overlayRoot);

   // Keep interactive HUD bars and tooltips above passive scene text overlays
   if (isObject(UnitStatsBadgeLayer) && UnitStatsBadgeLayer.isVisible())
      mainScreenGui.pushToBack(UnitStatsBadgeLayer);
   if (isObject(UnitStatsTooltipPanel) && UnitStatsTooltipPanel.isVisible())
      mainScreenGui.pushToBack(UnitStatsTooltipPanel);
   if (isObject(BoardAudioHudContainer) && BoardAudioHudContainer.isVisible())
      mainScreenGui.pushToBack(BoardAudioHudContainer);
   if (isObject(FpsCounterOverlay) && FpsCounterOverlay.isVisible())
      mainScreenGui.pushToBack(FpsCounterOverlay);
}

function SceneTextOverlayGui::reanchor(%this)
{
   if (!isObject(%this.overlayRoot) || !isObject(mainScreenGui))
      return;

   %mainW = getWord(mainScreenGui.extent, 0);
   %mainH = getWord(mainScreenGui.extent, 1);
   %this.overlayRoot.Position = "0 0";
   %this.overlayRoot.Extent = %mainW SPC %mainH;

   if (isObject(%this.creditsContainer))
   {
      %this.creditsContainer.Position = "0 0";
      %this.creditsContainer.Extent = %mainW SPC %mainH;
   }

   %this.refreshAll();
}

function SceneTextOverlayGui::worldToScreenPoint(%this, %wx, %wy)
{
   if (isObject(sceneWindow2D))
   {
      %pt = sceneWindow2D.getWindowPoint(%wx SPC %wy);
      if (%pt !$= "")
         return %pt;

      %ext = sceneWindow2D.getExtent();
      %winW = getWord(%ext, 0);
      %winH = getWord(%ext, 1);
   }
   else
   {
      %winW = 1024;
      %winH = 768;
   }

   if (%winW <= 0) %winW = 1024;
   if (%winH <= 0) %winH = 768;

   %camW = isObject($ResolutionManager) ? $ResolutionManager.getCameraWidth() : 100.0;
   %camH = (isObject($ResolutionManager) && $ResolutionManager.baseCameraHeight > 0) ? $ResolutionManager.baseCameraHeight : 75.0;
   if (%camW <= 0) %camW = 100.0;

   %sx = (%winW * 0.5) + (%wx * (%winW / %camW));
   %sy = (%winH * 0.5) - (%wy * (%winH / %camH));
   return mFloor(%sx) SPC mFloor(%sy);
}

//---------------------------------------------------------------------------------------------
// Master Refresh Dispatcher
//---------------------------------------------------------------------------------------------
function SceneTextOverlayGui::refreshAll(%this)
{
   if (!isObject(%this.overlayRoot))
      %this.createControls();

   if (!isObject(%this.overlayRoot))
      return;

   if (isObject(mainScreenGui))
   {
      %mainW = getWord(mainScreenGui.extent, 0);
      %mainH = getWord(mainScreenGui.extent, 1);
      %this.overlayRoot.Position = "0 0";
      %this.overlayRoot.Extent = %mainW SPC %mainH;
   }

   if (%this.isModalBlocking())
   {
      %this.overlayRoot.setVisible(false);
      return;
   }

   %this.overlayRoot.setVisible(true);
   %this.updateBannerOverlay();
   %this.updateCardOverlays();
   %this.updateCreditsOverlay();
   %this.syncZOrder();
}

//---------------------------------------------------------------------------------------------
// 1. Dynamic Scene Banners ("Choose a Hero", "Place creatures", "Game starts in N...")
//---------------------------------------------------------------------------------------------
function SceneTextOverlayGui::setBannerTextAndPos(%this, %text, %wx, %wy, %ww)
{
   %pt = %this.worldToScreenPoint(%wx, %wy);
   %leftPt = %this.worldToScreenPoint(%wx - (%ww * 0.5), %wy);
   %rightPt = %this.worldToScreenPoint(%wx + (%ww * 0.5), %wy);

   %cx = getWord(%pt, 0);
   %cy = getWord(%pt, 1);
   %sw = getWord(%rightPt, 0) - getWord(%leftPt, 0);

   %bannerScale = isObject($ResolutionManager) ? $ResolutionManager.getViewportSafeUiScale(560, 44, 24) : 1.0;
   %minSw = mFloor(520 * %bannerScale + 0.5);
   if (%sw < %minSw) %sw = %minSw;
   if (isObject(mainScreenGui))
   {
      %mainW = getWord(mainScreenGui.extent, 0);
      if (%mainW > 24 && %sw > %mainW - 24)
         %sw = %mainW - 24;
   }

   %sh = mFloor(44 * %bannerScale + 0.5);
   if (%sh < 24) %sh = 24;
   %shadowOff = mFloor(2 * %bannerScale + 0.5);
   if (%shadowOff < 1) %shadowOff = 1;
   %bx = mFloor(%cx - (%sw * 0.5));
   %by = mFloor(%cy - (%sh * 0.5));

   if (isObject(%this.bannerShadow))
   {
      %this.bannerShadow.Position = (%bx + %shadowOff) SPC (%by + %shadowOff);
      %this.bannerShadow.Extent = %sw SPC %sh;
      %this.bannerShadow.fontSizeAdjust = %bannerScale;
      %this.bannerShadow.setText(%text);
      %this.bannerShadow.setVisible(true);
   }
   if (isObject(%this.bannerLabel))
   {
      %this.bannerLabel.Position = %bx SPC %by;
      %this.bannerLabel.Extent = %sw SPC %sh;
      %this.bannerLabel.fontSizeAdjust = %bannerScale;
      %this.bannerLabel.setText(%text);
      %this.bannerLabel.setVisible(true);
   }
}

function SceneTextOverlayGui::hideBanner(%this)
{
   if (isObject(%this.bannerShadow))
   {
      %this.bannerShadow.setVisible(false);
      %this.bannerShadow.setText("");
   }
   if (isObject(%this.bannerLabel))
   {
      %this.bannerLabel.setVisible(false);
      %this.bannerLabel.setText("");
   }
}

function SceneTextOverlayGui::updateBannerOverlay(%this)
{
   if (!isObject(game) || !isObject($LocalizationManager))
   {
      %this.hideBanner();
      return;
   }

   %state = game.getState();

   if ($gameCardSelectState !$= "" && %state == $gameCardSelectState &&
       isObject(teksti) && teksti.getVisible() && teksti.getBlendAlpha() > 0.1)
   {
      %pos = teksti.getPosition();
      %bannerStr = $LocalizationManager.getText("banner_select_hero");
      %this.setBannerTextAndPos(%bannerStr, getWord(%pos, 0), getWord(%pos, 1), 86);
      return;
   }

   if (isObject(sijoitaText) && sijoitaText.getVisible() &&
       (($gamePreBattleState !$= "" && %state == $gamePreBattleState) ||
        ($gamePreBattleTimerState !$= "" && %state == $gamePreBattleTimerState)))
   {
      %pos = sijoitaText.getPosition();
      %bannerStr = $LocalizationManager.getText("banner_place_creatures");
      %this.setBannerTextAndPos(%bannerStr, getWord(%pos, 0), getWord(%pos, 1), 90);
      return;
   }

   if (isObject(secondText) && secondText.getVisible())
   {
      if (isObject(secondCounter))
         secondCounter.setBlendAlpha(0);

      %sec = game.getGameTimer();
      if (%sec $= "" && isObject(secondCounter))
         %sec = secondCounter.getFrame();
      if (%sec $= "")
         %sec = 0;

      %pos = secondText.getPosition();
      %bannerStr = $LocalizationManager.getPluralText("banner_countdown", %sec);
      %this.setBannerTextAndPos(%bannerStr, getWord(%pos, 0), getWord(%pos, 1), 96);
      return;
   }

   %this.hideBanner();
}

//---------------------------------------------------------------------------------------------
// 2. Dynamic Hero & Spell Card Overlays
//---------------------------------------------------------------------------------------------
function SceneTextOverlayGui::computeCardFitFontAdjust(%this, %text, %baseFontPx, %desiredScale, %boxW, %boxH, %singleLine)
{
   if (%baseFontPx <= 0)
      return 1.0;
   if (%desiredScale <= 0)
      %desiredScale = 1.0;
   if (%boxW <= 4 || %boxH <= 4)
      return 1.0;

   %minPx = 8.0;
   %desiredPx = %baseFontPx * %desiredScale;
   if (%desiredPx < %minPx)
      %desiredPx = %minPx;

   if (%singleLine)
   {
      %len = strlen(%text);
      if (%len < 1) %len = 1;
      %maxByH = %boxH - 2;
      %maxByW = %boxW / (%len * 0.54);
      %fitPx = %desiredPx;
      if (%fitPx > %maxByH) %fitPx = %maxByH;
      if (%fitPx > %maxByW) %fitPx = %maxByW;
      if (%fitPx < %minPx) %fitPx = %minPx;
      return %fitPx / (%baseFontPx * 1.0);
   }

   %startPx = mFloor(%desiredPx + 0.5);
   if (%startPx < %minPx)
      %startPx = %minPx;

   %wCount = getWordCount(%text);
   %longestWord = 1;
   for (%i = 0; %i < %wCount; %i++)
   {
      %wl = strlen(getWord(%text, %i));
      if (%wl > %longestWord)
         %longestWord = %wl;
   }

   for (%px = %startPx; %px >= %minPx; %px--)
   {
      %maxLines = mFloor(%boxH / (%px * 1.05));
      %cpl = mFloor(%boxW / (%px * 0.48));
      if (%maxLines < 1 || %cpl < %longestWord)
         continue;

      %lines = 1;
      %curLen = 0;
      for (%w = 0; %w < %wCount; %w++)
      {
         %wLen = strlen(getWord(%text, %w));
         if (%curLen == 0)
            %curLen = %wLen;
         else if (%curLen + 1 + %wLen <= %cpl)
            %curLen += 1 + %wLen;
         else
         {
            %lines++;
            %curLen = %wLen;
         }
      }

      if (%lines <= %maxLines)
         return %px / (%baseFontPx * 1.0);
   }

   return %minPx / (%baseFontPx * 1.0);
}

function SceneTextOverlayGui::renderCardToSlot(%this, %slot, %cardSprite)
{
   if (%slot < 0 || %slot >= %this.maxCardSlots || !isObject(%cardSprite))
      return false;

   if (!%cardSprite.getVisible() || %cardSprite.getBlendAlpha() <= 0.15)
      return false;

   if (mAbs(%cardSprite.getAngle()) >= 25)
      return false;

   %frame = %cardSprite.getImageFrame();
   if (%frame $= "" || %frame < 1 || %frame > 8)
      return false;

   %title = $LocalizationManager.getCardTitle(%frame);
   if (%title $= "")
      return false;

   %isStat = $LocalizationManager.isStatCard(%frame);
   %body = %isStat ? $LocalizationManager.getCardStatText(%frame) : $LocalizationManager.getCardDescription(%frame);

   %pos = %cardSprite.getPosition();
   %size = %cardSprite.getSize();
   %wx = getWord(%pos, 0);
   %wy = getWord(%pos, 1);
   %ww = getWord(%size, 0);
   %wh = getWord(%size, 1);
   if (%ww <= 0 || %wh <= 0)
      return false;

   %tl = %this.worldToScreenPoint(%wx - (%ww * 0.5), %wy + (%wh * 0.5));
   %br = %this.worldToScreenPoint(%wx + (%ww * 0.5), %wy - (%wh * 0.5));
   %sx = getWord(%tl, 0);
   %sy = getWord(%tl, 1);
   %sw = getWord(%br, 0) - %sx;
   %sh = getWord(%br, 1) - %sy;
   if (%sw < 80 || %sh < 80)
      return false;

   %slotCtrl = %this.cardSlotCtrl[%slot];
   %titleCtrl = %this.cardTitleCtrl[%slot];
   %bodyCtrl = %this.cardBodyCtrl[%slot];

   %slotCtrl.Position = %sx SPC %sy;
   %slotCtrl.Extent = %sw SPC %sh;

   %uiScale = isObject($ResolutionManager) ? $ResolutionManager.getUiScaleFactor() : 1.0;

   if (%frame <= 2)
   {
      %refW = 614.0;
      %titleProfile = %this.cardTitleLargeProfile;
      %titleBasePx = 18;
      %bodyProfile = %this.cardDescLargeProfile;
      %bodyBasePx = 15;
      %titleH = mFloor(%sh * 0.062);
      %bodyY = mFloor(%sh * 0.734);
      %bodyH = mFloor(%sh * 0.144);
   }
   else
   {
      %refW = 230.0;
      %titleProfile = %this.cardTitleMedProfile;
      %titleBasePx = 10;
      %bodyProfile = %isStat ? %this.cardStatMedProfile : %this.cardDescMedProfile;
      %bodyBasePx = 10;
      %titleH = mFloor(%sh * 0.072);
      %bodyY = mFloor(%sh * 0.594);
      %bodyH = mFloor(%sh * 0.276);
   }

   if (%titleH < 14) %titleH = 14;
   %resScale = %sw / %refW;
   if (%resScale < 0.65) %resScale = 0.65;
   if (%resScale > 3.5) %resScale = 3.5;
   %desiredScale = %resScale * %uiScale;

   // Title pill spans card image interior X=0.238..0.756 (width 0.518), center Y=0.153
   %titleX = mFloor(%sw * 0.238);
   %titleW = mFloor(%sw * 0.518);
   %titleY = mFloor((%sh * 0.153) - (%titleH * 0.5));
   if (%titleY < 0) %titleY = 0;

   %titleAdj = %this.computeCardFitFontAdjust(%title, %titleBasePx, %desiredScale, %titleW - 4, %titleH, true);
   %titleCtrl.setProfile(%titleProfile);
   %titleCtrl.fontSizeAdjust = %titleAdj;
   %titleCtrl.Position = %titleX SPC %titleY;
   %titleCtrl.Extent = %titleW SPC %titleH;
   %titleCtrl.setText(%title);

   // Lower text box spans X=0.252..0.742 (width 0.490)
   %bodyX = mFloor(%sw * 0.252);
   %bodyW = mFloor(%sw * 0.490);
   if (%bodyY + %bodyH > %sh) %bodyH = %sh - %bodyY;
   if (%bodyH < 14) %bodyH = 14;

   %bodyAdj = %this.computeCardFitFontAdjust(%body, %bodyBasePx, %desiredScale, %bodyW - 4, %bodyH, false);
   %bodyCtrl.setProfile(%bodyProfile);
   %bodyCtrl.fontSizeAdjust = %bodyAdj;
   %bodyCtrl.setTextWrap(true);
   %bodyCtrl.Position = %bodyX SPC %bodyY;
   %bodyCtrl.Extent = %bodyW SPC %bodyH;
   %bodyCtrl.setText(%body);

   %slotCtrl.setVisible(true);
   return true;
}

function SceneTextOverlayGui::hideCardSlot(%this, %slot)
{
   if (isObject(%this.cardSlotCtrl[%slot]))
      %this.cardSlotCtrl[%slot].setVisible(false);
   if (isObject(%this.cardTitleCtrl[%slot]))
      %this.cardTitleCtrl[%slot].setText("");
   if (isObject(%this.cardBodyCtrl[%slot]))
      %this.cardBodyCtrl[%slot].setText("");
}

function SceneTextOverlayGui::updateCardOverlays(%this)
{
   %nextSlot = 0;

   if (isObject(game) && isObject($LocalizationManager))
   {
      %state = game.getState();

      if ($gameCardSelectState !$= "" && %state == $gameCardSelectState)
      {
         if (isObject(card_0) && %this.renderCardToSlot(%nextSlot, card_0))
            %nextSlot++;
         if (isObject(card_1) && %this.renderCardToSlot(%nextSlot, card_1))
            %nextSlot++;
      }

      if (($gamePreBattleState !$= "" && %state == $gamePreBattleState) ||
          ($gamePreBattleTimerState !$= "" && %state == $gamePreBattleTimerState) ||
          ($gameTimerBattleState !$= "" && %state == $gameTimerBattleState) ||
          ($gameBattleState !$= "" && %state == $gameBattleState))
      {
         for (%i = 1; %i <= 6 && %nextSlot < %this.maxCardSlots; %i++)
         {
            if (isObject($specialcard[%i]) && %this.renderCardToSlot(%nextSlot, $specialcard[%i]))
               %nextSlot++;
         }
      }
   }

   for (%s = %nextSlot; %s < %this.maxCardSlots; %s++)
      %this.hideCardSlot(%s);
}

//---------------------------------------------------------------------------------------------
// 3. Credits Screen Localized Labels (Preserving Emoji Artwork)
//---------------------------------------------------------------------------------------------
function SceneTextOverlayGui::updateCreditsOverlay(%this)
{
   if (!isObject(%this.creditsContainer))
      return;

   if (!isObject(game) || !isObject($LocalizationManager) ||
       $gameCreditsState $= "" || game.getState() != $gameCreditsState ||
       !isObject(credits1) || !credits1.getVisible() || credits1.getBlendAlpha() <= 0.1)
   {
      %this.creditsContainer.setVisible(false);
      return;
   }

   %pos = credits1.getPosition();
   %size = credits1.getSize();
   %wx = getWord(%pos, 0);
   %wy = getWord(%pos, 1);
   %ww = getWord(%size, 0);
   %wh = getWord(%size, 1);

   %tl = %this.worldToScreenPoint(%wx - (%ww * 0.5), %wy + (%wh * 0.5));
   %br = %this.worldToScreenPoint(%wx + (%ww * 0.5), %wy - (%wh * 0.5));
   %cx = getWord(%tl, 0);
   %cy = getWord(%tl, 1);
   %cw = getWord(%br, 0) - %cx;
   %ch = getWord(%br, 1) - %cy;

   %uiScale = isObject($ResolutionManager) ? $ResolutionManager.getUiScaleFactor() : 1.0;
   %resScale = %ch / 576.0;
   if (%resScale < 0.65) %resScale = 0.65;
   if (%resScale > 3.5) %resScale = 3.5;
   %desiredScale = %resScale * %uiScale;

   %titleStr = $LocalizationManager.getText("credits_title");
   %titleY = %cy + mFloor(%ch * 0.055);
   %titleH = mFloor(%ch * 0.12);
   if (%titleH < 24) %titleH = 24;

   %titleAdj = %desiredScale;
   %maxTitleAdj = (%titleH - 2) / 28.0;
   if (%titleAdj > %maxTitleAdj) %titleAdj = %maxTitleAdj;
   if (%titleAdj < 0.5) %titleAdj = 0.5;
   %titleOff = mFloor(2 * %titleAdj + 0.5);
   if (%titleOff < 1) %titleOff = 1;

   if (isObject(%this.creditsTitleShadow))
   {
      %this.creditsTitleShadow.Position = (%cx + %titleOff) SPC (%titleY + %titleOff);
      %this.creditsTitleShadow.Extent = %cw SPC %titleH;
      %this.creditsTitleShadow.fontSizeAdjust = %titleAdj;
      %this.creditsTitleShadow.setText(%titleStr);
   }
   if (isObject(%this.creditsTitleLabel))
   {
      %this.creditsTitleLabel.Position = %cx SPC %titleY;
      %this.creditsTitleLabel.Extent = %cw SPC %titleH;
      %this.creditsTitleLabel.fontSizeAdjust = %titleAdj;
      %this.creditsTitleLabel.setText(%titleStr);
   }

   %roleKeys = "credits_role_designer" TAB "credits_role_programmer" TAB "credits_role_artist" TAB
               "credits_role_music" TAB "credits_role_level" TAB "credits_role_qa" TAB "credits_role_thanks";
   %roleCenters = "0.256 0.343 0.429 0.515 0.602 0.689 0.862";

   %rx = %cx + mFloor(%cw * 0.155);
   %rw = mFloor(%cw * 0.450);
   %rh = mFloor(%ch * 0.080);
   if (%rh < 24) %rh = 24;

   %roleAdj = %desiredScale;
   %maxRoleAdj = (%rh - 2) / 18.0;
   if (%roleAdj > %maxRoleAdj) %roleAdj = %maxRoleAdj;
   if (%roleAdj < 0.5) %roleAdj = 0.5;
   %roleOff = mFloor(2 * %roleAdj + 0.5);
   if (%roleOff < 1) %roleOff = 1;

   for (%i = 0; %i < 7; %i++)
   {
      %rStr = $LocalizationManager.getText(getField(%roleKeys, %i));
      %centerFrac = getWord(%roleCenters, %i);
      %ry = %cy + mFloor((%ch * %centerFrac) - (%rh * 0.5));

      if (isObject(%this.creditsRoleShadow[%i]))
      {
         %this.creditsRoleShadow[%i].Position = (%rx + %roleOff) SPC (%ry + %roleOff);
         %this.creditsRoleShadow[%i].Extent = %rw SPC %rh;
         %this.creditsRoleShadow[%i].fontSizeAdjust = %roleAdj;
         %this.creditsRoleShadow[%i].setText(%rStr);
      }
      if (isObject(%this.creditsRoleLabel[%i]))
      {
         %this.creditsRoleLabel[%i].Position = %rx SPC %ry;
         %this.creditsRoleLabel[%i].Extent = %rw SPC %rh;
         %this.creditsRoleLabel[%i].fontSizeAdjust = %roleAdj;
         %this.creditsRoleLabel[%i].setText(%rStr);
      }
   }

   %this.creditsContainer.setVisible(true);
}

//---------------------------------------------------------------------------------------------
// Inspection Getters for Automated Verification
//---------------------------------------------------------------------------------------------
function SceneTextOverlayGui::isBannerVisible(%this)
{
   return (isObject(%this.overlayRoot) && %this.overlayRoot.isVisible() &&
           isObject(%this.bannerLabel) && %this.bannerLabel.isVisible());
}

function SceneTextOverlayGui::getBannerText(%this)
{
   return isObject(%this.bannerLabel) ? %this.bannerLabel.getText() : "";
}

function SceneTextOverlayGui::isCardSlotVisible(%this, %slot)
{
   return (isObject(%this.overlayRoot) && %this.overlayRoot.isVisible() &&
           %slot >= 0 && %slot < %this.maxCardSlots &&
           isObject(%this.cardSlotCtrl[%slot]) && %this.cardSlotCtrl[%slot].isVisible());
}

function SceneTextOverlayGui::getCardSlotTitleText(%this, %slot)
{
   if (%slot < 0 || %slot >= %this.maxCardSlots || !isObject(%this.cardTitleCtrl[%slot]))
      return "";
   return %this.cardTitleCtrl[%slot].getText();
}

function SceneTextOverlayGui::getCardSlotBodyText(%this, %slot)
{
   if (%slot < 0 || %slot >= %this.maxCardSlots || !isObject(%this.cardBodyCtrl[%slot]))
      return "";
   return %this.cardBodyCtrl[%slot].getText();
}

function SceneTextOverlayGui::getCardSlotTitleBounds(%this, %slot)
{
   if (%slot < 0 || %slot >= %this.maxCardSlots || !isObject(%this.cardTitleCtrl[%slot]))
      return "";
   return %this.cardTitleCtrl[%slot].Position SPC %this.cardTitleCtrl[%slot].Extent;
}

function SceneTextOverlayGui::getCardSlotBodyBounds(%this, %slot)
{
   if (%slot < 0 || %slot >= %this.maxCardSlots || !isObject(%this.cardBodyCtrl[%slot]))
      return "";
   return %this.cardBodyCtrl[%slot].Position SPC %this.cardBodyCtrl[%slot].Extent;
}

function SceneTextOverlayGui::getCreditsTitleText(%this)
{
   return isObject(%this.creditsTitleLabel) ? %this.creditsTitleLabel.getText() : "";
}

function SceneTextOverlayGui::getCreditsRoleText(%this, %idx)
{
   if (%idx < 0 || %idx >= 7 || !isObject(%this.creditsRoleLabel[%idx]))
      return "";
   return %this.creditsRoleLabel[%idx].getText();
}

// Initialize Singleton
if (!isObject(SceneTextOverlayGui))
{
   new ScriptObject(SceneTextOverlayGui) { };
}
if (!isObject($SceneTextOverlayGui))
{
   $SceneTextOverlayGui = SceneTextOverlayGui;
}
