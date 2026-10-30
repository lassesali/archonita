//---------------------------------------------------------------------------------------------
// Torque GameBuilder. 
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

//---------------------------------------------------------------------------------------------
// startGame
// All game logic should be set up here. This will be called by the level builder when you
// select "Run Game" or by the startup process of your game to load the first level.
//---------------------------------------------------------------------------------------------
function startGame(%level)
{

   // Set The GUI.
   Canvas.setContent(mainScreenGui);
   Canvas.setCursor(DefaultCursor);
   Canvas.hideCursor(); 
   //moveMap.push();
   
   if( isFile( %level ) || isFile( %level @ ".dso"))
      sceneWindow2D.loadLevel(%level);

   initMouse();
   
      
}




//---------------------------------------------------------------------------------------------
// endGame
// Game cleanup should be done here.
//---------------------------------------------------------------------------------------------
function endGame()
{
   alxStopAll();
   testActionMap.pop();
   sceneWindow2D.endLevel();
   quit();
}
