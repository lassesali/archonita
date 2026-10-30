//---------------------------------------------------------------------------------------------
// Torque GameBuilder. 
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

//---------------------------------------------------------------------------------------------
// initializeProject
// Perform game initialization here.
//---------------------------------------------------------------------------------------------
function initializeProject()
{

   %res = getDesktopResolution();
   echo(%res);
   setScreenMode(  getWord( %res,0 ) , getWord( %res,1 ) , 32, true  );

   // Load up the in game gui.
   exec("~/gui/ggSplash.gui");
   exec("~/gui/mainScreen.gui");
      
   // Exec game scripts.
   exec("./gameScripts/intro.cs");
   exec("./gameScripts/createhero.cs");
   exec("./gameScripts/mainmenu.cs");
   exec("./gameScripts/credits.cs");
   exec("./gameScripts/exec.cs");
   exec("./gameScripts/game.cs");
   exec("./gameScripts/audiodatablocks.cs");
   exec("./gameScripts/fading.cs");
   exec("./gameScripts/complexmouse.cs");
   exec("./gameScripts/units.cs");
   exec("./gameScripts/hero.cs");
   exec("./gameScripts/nappula.cs");
   exec("./gameScripts/gameObject.cs");
      
   SetRandomSeed(getRealTime());
   loadGGsplash();

   
}



//---------------------------------------------------------------------------------------------
// shutdownProject
// Clean up your game objects here.
//---------------------------------------------------------------------------------------------
function shutdownProject()
{
   endGame();
}

//---------------------------------------------------------------------------------------------
// setupKeybinds
// Bind keys to actions here..
//---------------------------------------------------------------------------------------------
function setupKeybinds()
{

}
