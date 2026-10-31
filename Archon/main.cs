//---------------------------------------------------------------------------------------------
// Torque GameBuilder. 
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

//---------------------------------------------------------------------------------------------
// initializeProject
// Perform game initialization here.
//---------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------
// Torque2D 4.0 Module Lifecycle Callbacks
//---------------------------------------------------------------------------------------------
function Archon::create(%this)
{
   initializeProject();
}

function Archon::destroy(%this)
{
   shutdownProject();
}

function initializeProject()
{

   %res = getDesktopResolution();
   echo(%res);
   %fs = false;
   if ($pref::Video::fullScreen !$= "")
      %fs = $pref::Video::fullScreen;
   setScreenMode( 1024, 768, 32, %fs );

   // Load up the in game gui.
   exec("./gameScripts/t2d_compat.cs");
   OpenALInitDriver();
   Canvas.setCursor(DefaultCursor);
   Canvas.showCursor();
   exec("./gui/ggSplash.gui");
   exec("./gui/mainScreen.gui");
      
   // Exec game scripts.
   exec("./gameScripts/intro.cs");
   exec("./gameScripts/createhero.cs");
   exec("./gameScripts/mainmenu.cs");
   exec("./gameScripts/credits.cs");
   exec("./gameScripts/BoardManager.cs");
   exec("./gameScripts/TurnManager.cs");
   exec("./gameScripts/CombatEngine.cs");
   exec("./gameScripts/TacticalAI.cs");
   exec("./gameScripts/game.cs");
   exec("./gameScripts/audiodatablocks.cs");
   exec("./gameScripts/fading.cs");
   exec("./gameScripts/complexmouse.cs");
   exec("./gameScripts/units.cs");
   exec("./gameScripts/hero.cs");
   exec("./gameScripts/nappula.cs");
   exec("./gameScripts/gameObject.cs");
   exec("./gameScripts/AudioManager.cs");
   if (!isObject($AudioManager))
      $AudioManager = new ScriptObject() { class = "AudioManager"; };
   exec("./gameScripts/versionConfig.cs");
   if (!isObject($VersionConfig))
      $VersionConfig = new ScriptObject() { class = "VersionConfig"; };
   exec("./gameScripts/AssetResolutionManager.cs");
   if (!isObject($AssetResolutionManager))
      $AssetResolutionManager = new ScriptObject() { class = "AssetResolutionManager"; };
      
   SetRandomSeed(getRealTime());
   if (!$Archon::skipSplash)
      loadGGsplash();

   
}



//---------------------------------------------------------------------------------------------
// shutdownProject
// Clean up your game objects here.
//---------------------------------------------------------------------------------------------
function shutdownProject()
{
   if (isObject($VersionConfig))
      $VersionConfig.delete();
   if (isObject($AudioManager))
      $AudioManager.delete();
   if (isObject($AssetResolutionManager))
      $AssetResolutionManager.delete();
   endGame();
}

//---------------------------------------------------------------------------------------------
// setupKeybinds
// Bind keys to actions here..
//---------------------------------------------------------------------------------------------
function setupKeybinds()
{

}
