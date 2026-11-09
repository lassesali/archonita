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

   // Load up the in game gui and resolution preferences.
   exec("./gameScripts/t2d_compat.cs");
   exec("./gameScripts/ResolutionManager.cs");
   if (!isObject($ResolutionManager))
      $ResolutionManager = new ScriptObject(ResolutionManager) { };

   %bootW = getWord($ResolutionManager.currentResolution, 0);
   %bootH = getWord($ResolutionManager.currentResolution, 1);
   if (%bootW <= 0 || %bootH <= 0)
   {
      %bootW = 1280;
      %bootH = 720;
   }
   setScreenMode( %bootW, %bootH, 32, $ResolutionManager.currentFullScreen );

   OpenALInitDriver();
   Canvas.setCursor(DefaultCursor);
   Canvas.showCursor();
   exec("./gui/ggSplash.gui");
   exec("./gui/mainScreen.gui");
   if (isObject(mainScreenGui))
      mainScreenGui.Extent = %bootW SPC %bootH;
   if (isObject(sceneWindow2D))
      sceneWindow2D.Extent = %bootW SPC %bootH;
      
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
   exec("./gameScripts/Card.cs");
   exec("./gameScripts/Deck.cs");
   exec("./gameScripts/AudioManager.cs");
   if (!isObject($AudioManager))
      $AudioManager = new ScriptObject() { class = "AudioManager"; };
   exec("./gameScripts/versionConfig.cs");
   if (!isObject($VersionConfig))
      $VersionConfig = new ScriptObject() { class = "VersionConfig"; };
   exec("./gameScripts/AssetResolutionManager.cs");
   if (!isObject($AssetResolutionManager))
      $AssetResolutionManager = new ScriptObject() { class = "AssetResolutionManager"; };
   exec("./gameScripts/FloatingTextManager.cs");
   if (!isObject($FloatingTextManager))
      $FloatingTextManager = FloatingTextManager;
   exec("./gameScripts/metrics.cs");
   if (!isObject($Metrics))
      $Metrics = Metrics;
   exec("./gameScripts/BoardAudioHud.cs");
   if (!isObject($BoardAudioHud))
      $BoardAudioHud = new ScriptObject(BoardAudioHud) { };
   exec("./gameScripts/PauseMenuManager.cs");
   if (!isObject($PauseMenuManager))
      $PauseMenuManager = new ScriptObject(PauseMenuManager) { };
   exec("./gameScripts/TitleMenuManager.cs");
   if (!isObject($TitleMenuManager))
      $TitleMenuManager = new ScriptObject(TitleMenuManager) { };
   exec("./gameScripts/OptionsMenuGui.cs");
   if (!isObject($OptionsMenuGui))
      $OptionsMenuGui = new ScriptObject(OptionsMenuGui) { };
   exec("./gameScripts/FactionManager.cs");
   if (!isObject($FactionManager))
      $FactionManager = new ScriptObject() { class = "FactionManager"; };
   exec("./gameScripts/FactionSelectGui.cs");
   if (!isObject($FactionSelectGui))
      $FactionSelectGui = new ScriptObject(FactionSelectGui) { };

   if (isObject($ResolutionManager))
      $ResolutionManager.reanchorAllGuis();
      
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
   if (isObject($FactionSelectGui))
      $FactionSelectGui.delete();
   if (isObject($FactionManager))
      $FactionManager.delete();
   if (isObject($OptionsMenuGui))
      $OptionsMenuGui.delete();
   if (isObject($TitleMenuManager))
      $TitleMenuManager.delete();
   if (isObject($PauseMenuManager))
      $PauseMenuManager.delete();
   if (isObject($BoardAudioHud))
      $BoardAudioHud.delete();
   if (isObject($ResolutionManager))
      $ResolutionManager.delete();
   if (isObject($VersionConfig))
      $VersionConfig.delete();
   if (isObject($AudioManager))
      $AudioManager.delete();
   if (isObject($AssetResolutionManager))
      $AssetResolutionManager.delete();
   if (isObject($FloatingTextManager))
      $FloatingTextManager.delete();
   if (isObject($Metrics))
      $Metrics.delete();
   endGame();
}

//---------------------------------------------------------------------------------------------
// setupKeybinds
// Bind keys to actions here..
//---------------------------------------------------------------------------------------------
function setupKeybinds()
{

}
