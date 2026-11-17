//---------------------------------------------------------------------------------------------
// Archonita - ResolutionManager.cs
// Manages display resolutions, aspect ratios (16:9, 16:10, 21:9, 32:9, 4:3),
// dynamic 2D camera viewport scaling, and persistent user preferences.
//---------------------------------------------------------------------------------------------

function ResolutionManager::onAdd(%this)
{
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

   %this.loadPreferences();
}

function ResolutionManager::onRemove(%this)
{
   // No owned SimObjects or repeating schedules to tear down
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

   // Sync global prefs
   $pref::Video::AspectRatio = %this.currentAspectRatio;
   $pref::Video::Resolution = %this.currentResolution;
   $pref::Video::fullScreen = %this.currentFullScreen;
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
      %file.close();
   }
   %file.delete();
}

function ResolutionManager::getAspectRatioValue(%this, %aspectStr, %resStr)
{
   %w = getWord(%resStr, 0);
   %h = getWord(%resStr, 1);

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
   if (%aspectRatio !$= "" && %this.resList[%aspectRatio] !$= "")
      %this.currentAspectRatio = %aspectRatio;

   if (%resolution !$= "" && getWordCount(%resolution) >= 2)
      %this.currentResolution = getWord(%resolution, 0) SPC getWord(%resolution, 1);
   else
      %this.currentResolution = getField(%this.getResolutionsForAspect(%this.currentAspectRatio), 0);

   if (%fullScreen !$= "")
      %this.currentFullScreen = (%fullScreen == 1 || %fullScreen $= "true");

   $pref::Video::AspectRatio = %this.currentAspectRatio;
   $pref::Video::Resolution = %this.currentResolution;
   $pref::Video::fullScreen = %this.currentFullScreen;

   %w = getWord(%this.currentResolution, 0);
   %h = getWord(%this.currentResolution, 1);

   if (%w > 0 && %h > 0)
   {
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
   if (isObject($BoardAudioHud))
      $BoardAudioHud.reanchor();
   if (isObject($TitleMenuManager))
      $TitleMenuManager.reanchor();
   if (isObject($OptionsMenuGui))
      $OptionsMenuGui.reanchor();
   if (isObject($FactionSelectGui))
      $FactionSelectGui.reanchor();
   if (isObject($PauseMenuManager))
      $PauseMenuManager.reanchor();
   if (isObject(MainMenuWatermark) && isObject(mainScreenGui))
   {
      %mainW = getWord(mainScreenGui.extent, 0);
      %mainH = getWord(mainScreenGui.extent, 1);
      MainMenuWatermark.Position = (%mainW - 280) SPC (%mainH - 74);
      mainScreenGui.pushToBack(MainMenuWatermark);
   }
}

if (!isObject($ResolutionManager))
{
   $ResolutionManager = new ScriptObject(ResolutionManager) { };
}
