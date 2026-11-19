//---------------------------------------------------------------------------------------------
// Archonita - ResolutionManager.cs
// Manages display resolutions, aspect ratios (16:9, 16:10, 21:9, 32:9, 4:3),
// dynamic 2D camera viewport scaling, and persistent user preferences.
//---------------------------------------------------------------------------------------------

function ResolutionManager::onAdd(%this)
{
   if ($platform $= "Android")
      %this.prefFile = getUserDataDirectory() @ "/preferences.cs";
   else
      %this.prefFile = "Archon/preferences.cs";
   %this.baseCameraHeight = 75.0;

   // Aspect Ratio Catalog
   %this.aspectCount = 5;
   %this.aspectName[0] = "16:9";
   %this.aspectName[1] = "16:10";
   %this.aspectName[2] = "21:9";
   %this.aspectName[3] = "32:9";
   %this.aspectName[4] = "4:3";

   // Resolutions per Aspect Ratio (space-separated width height pairs, tab-delimited)
   %this.resList["16:9"]  = "1280 720" TAB "1600 900" TAB "1920 1080" TAB "2560 1440" TAB "3840 2160";
   %this.resList["16:10"] = "1280 800" TAB "1440 900" TAB "1680 1050" TAB "1920 1200" TAB "2560 1600";
   %this.resList["21:9"]  = "2560 1080" TAB "3440 1440" TAB "5120 2160";
   %this.resList["32:9"]  = "3840 1080" TAB "5120 1440";
   %this.resList["4:3"]   = "800 600" TAB "1024 768" TAB "1280 960";

   // Default settings before loading preferences
   %this.currentAspectRatio = "16:9";
   %this.currentResolution = "1280 720";
   %this.currentFullScreen = false;
   %this.currentLanguage = "fi";
   %this.minUiScale = 75;
   %this.maxUiScale = 150;
   %this.baseUiScale = 75;
   %this.currentUiScale = 75;

   %this.loadPreferences();
}

function ResolutionManager::onRemove(%this)
{
   // No owned SimObjects or repeating schedules to tear down
}

function ResolutionManager::getAndroidScreenResolution(%this)
{
   %extent = isObject(Canvas) ? Canvas.getExtent() : "";
   %w = getWord(%extent, 0);
   %h = getWord(%extent, 1);
   if (%w <= 0 || %h <= 0)
   {
      %res = GetAndroidResolution();
      %w = getWord(%res, 0);
      %h = getWord(%res, 1);
   }
   if (%w <= 0 || %h <= 0)
      return "1280 720";
   return %w SPC %h;
}

function ResolutionManager::loadPreferences(%this)
{
   if (isFile(%this.prefFile))
      exec(%this.prefFile);

   if ($pref::Video::AspectRatio !$= "")
      %this.currentAspectRatio = $pref::Video::AspectRatio;

   if ($pref::Video::Resolution !$= "")
      %this.currentResolution = $pref::Video::Resolution;

   if ($pref::Video::fullScreen !$= "")
      %this.currentFullScreen = ($pref::Video::fullScreen == 1 || $pref::Video::fullScreen $= "true");

   // Validate aspect ratio key
   if (%this.resList[%this.currentAspectRatio] $= "")
      %this.currentAspectRatio = "16:9";

   if ($platform $= "Android")
   {
      %this.currentResolution = %this.getAndroidScreenResolution();
      %this.currentFullScreen = true;
   }

   // Sync global prefs
   $pref::Video::AspectRatio = %this.currentAspectRatio;
   $pref::Video::Resolution = %this.currentResolution;
   $pref::Video::fullScreen = %this.currentFullScreen;

   if ($pref::Language !$= "")
      %this.currentLanguage = $pref::Language;
   else if (%this.currentLanguage $= "")
      %this.currentLanguage = "fi";

   if (isObject($LocalizationManager))
   {
      %this.currentLanguage = $LocalizationManager.normalizeLanguage(%this.currentLanguage);
      $LocalizationManager.setLanguage(%this.currentLanguage, true);
   }
   $pref::Language = %this.currentLanguage;

   if ($pref::UI::Scale !$= "")
      %this.currentUiScale = %this.normalizeUiScale($pref::UI::Scale);
   else
      %this.currentUiScale = %this.baseUiScale;
   $pref::UI::Scale = %this.currentUiScale;

   if (isObject(mainScreenGui))
      %this.reanchorAllGuis();
}

function ResolutionManager::savePreferences(%this)
{
   %file = new FileObject();
   if (%file.openForWrite(%this.prefFile))
   {
      %file.writeLine("// Archonita User Preferences (Auto-generated)");
      %file.writeLine("$pref::Video::AspectRatio = \"" @ %this.currentAspectRatio @ "\";");
      %file.writeLine("$pref::Video::Resolution = \"" @ %this.currentResolution @ "\";");
      %file.writeLine("$pref::Video::fullScreen = " @ (%this.currentFullScreen ? 1 : 0) @ ";");
      %sfxMuted = (isObject($AudioManager) && $AudioManager.isSFXMuted()) ? 1 : 0;
      %musMuted = (isObject($AudioManager) && $AudioManager.isMusicMuted()) ? 1 : 0;
      %file.writeLine("$pref::Audio::sfxMuted = " @ %sfxMuted @ ";");
      %file.writeLine("$pref::Audio::musicMuted = " @ %musMuted @ ";");
      %lang = isObject($LocalizationManager) ? $LocalizationManager.getLanguage() : (($pref::Language !$= "") ? $pref::Language : "fi");
      %this.currentLanguage = %lang;
      $pref::Language = %lang;
      %file.writeLine("$pref::Language = \"" @ %lang @ "\";");
      %uiScale = %this.getUiScalePercent();
      %this.currentUiScale = %uiScale;
      $pref::UI::Scale = %uiScale;
      %file.writeLine("$pref::UI::Scale = " @ %uiScale @ ";");
      %file.close();
   }
   %file.delete();
}

function ResolutionManager::normalizeUiScale(%this, %percent)
{
   if (%percent $= "")
      return %this.baseUiScale;

   %val = mFloor(%percent + 0.5);
   if (%val < %this.minUiScale)
      %val = %this.minUiScale;
   if (%val > %this.maxUiScale)
      %val = %this.maxUiScale;
   return %val;
}

function ResolutionManager::getUiScalePercent(%this)
{
   return %this.normalizeUiScale(%this.currentUiScale);
}

function ResolutionManager::getUiScaleFactor(%this)
{
   return %this.getUiScalePercent() / (%this.baseUiScale * 1.0);
}

function ResolutionManager::getViewportSafeUiScale(%this, %baseW, %baseH, %margin)
{
   if (%margin $= "")
      %margin = 24;

   %scale = %this.getUiScaleFactor();
   %mainW = 1024;
   %mainH = 768;
   if (isObject(mainScreenGui))
   {
      %mw = getWord(mainScreenGui.extent, 0);
      %mh = getWord(mainScreenGui.extent, 1);
      if (%mw > 0) %mainW = %mw;
      if (%mh > 0) %mainH = %mh;
   }

   if (%baseW > 0 && %mainW > %margin)
   {
      %maxWScale = (%mainW - %margin) / (%baseW * 1.0);
      if (%scale > %maxWScale)
         %scale = %maxWScale;
   }

   if (%baseH > 0 && %mainH > %margin)
   {
      %maxHScale = (%mainH - %margin) / (%baseH * 1.0);
      if (%scale > %maxHScale)
         %scale = %maxHScale;
   }

   if (%scale < 0.5)
      %scale = 0.5;
   return %scale;
}

function ResolutionManager::snapshotUiBaseTree(%this, %ctrl)
{
   if (!isObject(%ctrl))
      return;

   if (%ctrl._uiBaseCaptured !$= "1")
   {
      %ctrl._uiBasePos = %ctrl.Position;
      %ctrl._uiBaseExt = %ctrl.Extent;
      %ctrl._uiBaseFontAdj = (%ctrl.fontSizeAdjust !$= "" && %ctrl.fontSizeAdjust > 0) ? %ctrl.fontSizeAdjust : 1.0;
      %ctrl._uiBaseCaptured = 1;
   }

   %count = %ctrl.getCount();
   for (%i = 0; %i < %count; %i++)
      %this.snapshotUiBaseTree(%ctrl.getObject(%i));
}

function ResolutionManager::_applyUiScaleRecursive(%this, %ctrl, %scale, %scaleRootPos)
{
   if (!isObject(%ctrl))
      return;

   %basePos = (%ctrl._uiBasePos !$= "") ? %ctrl._uiBasePos : %ctrl.Position;
   %baseExt = (%ctrl._uiBaseExt !$= "") ? %ctrl._uiBaseExt : %ctrl.Extent;
   %baseAdj = (%ctrl._uiBaseFontAdj !$= "" && %ctrl._uiBaseFontAdj > 0) ? %ctrl._uiBaseFontAdj : 1.0;

   %bx = getWord(%basePos, 0);
   %by = getWord(%basePos, 1);
   %bw = getWord(%baseExt, 0);
   %bh = getWord(%baseExt, 1);

   %sw = mFloor((%bw * %scale) + 0.5);
   %sh = mFloor((%bh * %scale) + 0.5);
   if (%sw < 1) %sw = 1;
   if (%sh < 1) %sh = 1;

   if (%scaleRootPos)
   {
      %sx = mFloor((%bx * %scale) + 0.5);
      %sy = mFloor((%by * %scale) + 0.5);
      %ctrl.Position = %sx SPC %sy;
   }

   %ctrl.Extent = %sw SPC %sh;
   %ctrl.fontSizeAdjust = %baseAdj * %scale;

   %count = %ctrl.getCount();
   for (%i = 0; %i < %count; %i++)
      %this._applyUiScaleRecursive(%ctrl.getObject(%i), %scale, true);
}

function ResolutionManager::applyUiScaleToTree(%this, %ctrl, %scale, %scaleRootPos)
{
   if (!isObject(%ctrl))
      return;

   if (%scale $= "" || %scale <= 0)
      %scale = 1.0;

   %this.snapshotUiBaseTree(%ctrl);
   %this._applyUiScaleRecursive(%ctrl, %scale, %scaleRootPos);
}

function ResolutionManager::setUiScale(%this, %percent, %skipSave)
{
   %norm = %this.normalizeUiScale(%percent);
   %this.currentUiScale = %norm;
   $pref::UI::Scale = %norm;

   %this.reanchorAllGuis();

   if (isObject($OptionsMenuGui) && $OptionsMenuGui.isMethod("updateVisuals"))
      $OptionsMenuGui.updateVisuals();

   if (!%skipSave)
      %this.savePreferences();

   if (isObject(Canvas))
      Canvas.repaint();

   return %norm;
}

function ResolutionManager::getAspectRatioValue(%this, %aspectStr, %resStr)
{
   %w = getWord(%resStr, 0);
   %h = getWord(%resStr, 1);

   // Android stays locked to the device surface aspect ratio rather than desktop presets.
   if ($platform $= "Android" && %w > 0 && %h > 0)
      return %w / %h;

   if (%aspectStr $= "16:9")
      return 16.0 / 9.0;
   if (%aspectStr $= "16:10")
      return 16.0 / 10.0;
   if (%aspectStr $= "21:9")
   {
      if (%w > 0 && %h > 0)
         return %w / %h;
      return 21.0 / 9.0;
   }
   if (%aspectStr $= "32:9")
      return 32.0 / 9.0;
   if (%aspectStr $= "4:3")
      return 4.0 / 3.0;

   if (%w > 0 && %h > 0)
      return %w / %h;

   return 4.0 / 3.0;
}

function ResolutionManager::getResolutionsForAspect(%this, %aspectStr)
{
   if (%this.resList[%aspectStr] !$= "")
      return %this.resList[%aspectStr];
   return %this.resList["16:9"];
}

function ResolutionManager::getCameraWidth(%this)
{
   %aspect = %this.getAspectRatioValue(%this.currentAspectRatio, %this.currentResolution);
   return mFloatLength(%this.baseCameraHeight * %aspect, 2);
}

function ResolutionManager::applyDisplaySettings(%this, %aspectRatio, %resolution, %fullScreen)
{
   if ($platform $= "Android")
   {
      %this.currentResolution = %this.getAndroidScreenResolution();
      %this.currentFullScreen = true;
   }
   else
   {
      if (%aspectRatio !$= "" && %this.resList[%aspectRatio] !$= "")
         %this.currentAspectRatio = %aspectRatio;

      if (%resolution !$= "" && getWordCount(%resolution) >= 2)
         %this.currentResolution = getWord(%resolution, 0) SPC getWord(%resolution, 1);
      else
         %this.currentResolution = getField(%this.getResolutionsForAspect(%this.currentAspectRatio), 0);

      if (%fullScreen !$= "")
         %this.currentFullScreen = (%fullScreen == 1 || %fullScreen $= "true");
   }

   $pref::Video::AspectRatio = %this.currentAspectRatio;
   $pref::Video::Resolution = %this.currentResolution;
   $pref::Video::fullScreen = %this.currentFullScreen;

   %w = getWord(%this.currentResolution, 0);
   %h = getWord(%this.currentResolution, 1);

   if (%w > 0 && %h > 0)
   {
      if ($platform !$= "Android")
         setScreenMode(%w, %h, 32, %this.currentFullScreen);
      if (isObject(mainScreenGui))
         mainScreenGui.Extent = %w SPC %h;
      if (isObject(sceneWindow2D))
         sceneWindow2D.Extent = %w SPC %h;
   }

   if (isObject(sceneWindow2D))
      %this.applyCameraAspect(sceneWindow2D);

   %this.adaptSceneBackground();
   %this.reanchorAllGuis();
   %this.savePreferences();

   if (isObject(Canvas))
      Canvas.repaint();
}

function ResolutionManager::applyCameraAspect(%this, %window)
{
   if (!isObject(%window))
      %window = sceneWindow2D;
   if (!isObject(%window))
      return;

   %camW = %this.getCameraWidth();
   %camH = %this.baseCameraHeight;
   %window.setCameraPosition(0, 0);
   %window.setCameraSize(%camW, %camH);
}

function ResolutionManager::adaptSceneBackground(%this)
{
   %camW = %this.getCameraWidth();
   %bgW = (%camW > 102) ? (%camW + 4) : 102;

   // Board screen background
   if (isObject(background))
      background.setSize(%bgW SPC "76");

   // Title screen background
   if (isObject(kuva))
   {
      %titleW = (%camW > 100) ? (%camW + 2) : 100;
      kuva.setSize(%titleW SPC "75");
   }
}

function ResolutionManager::reanchorAllGuis(%this)
{
   if (isObject($SceneTextOverlayGui))
      $SceneTextOverlayGui.reanchor();
   if (isObject($UnitStatsHud))
      $UnitStatsHud.reanchor();
   if (isObject($BoardAudioHud))
      $BoardAudioHud.reanchor();
   if (isObject($TitleMenuManager))
      $TitleMenuManager.reanchor();
   if (isObject($OptionsMenuGui))
      $OptionsMenuGui.reanchor();
   if (isObject($FactionSelectGui))
      $FactionSelectGui.reanchor();
   if (isObject($BattleSummaryGui))
      $BattleSummaryGui.reanchor();
   if (isObject($PauseMenuManager))
      $PauseMenuManager.reanchor();
   if (isObject($VersionConfig) && $VersionConfig.isMethod("reanchor"))
      $VersionConfig.reanchor();
   else if (isObject(MainMenuWatermark) && isObject(mainScreenGui))
   {
      %mainW = getWord(mainScreenGui.extent, 0);
      %mainH = getWord(mainScreenGui.extent, 1);
      MainMenuWatermark.Position = (%mainW - 280) SPC (%mainH - 74);
      mainScreenGui.pushToBack(MainMenuWatermark);
   }
   if (isObject($Metrics) && $Metrics.isMethod("reanchor"))
      $Metrics.reanchor();
}

if (!isObject($ResolutionManager))
{
   $ResolutionManager = new ScriptObject(ResolutionManager) { };
}
