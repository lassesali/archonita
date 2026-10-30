//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

//---------------------------------------------------------------------------------------------
// startGame
// All game logic should be set up here. This will be called by the level builder when you
// select "Run Game" or by the startup process of your game to load the first level.
//---------------------------------------------------------------------------------------------
function startGame(%level)
{
	exec("./complexMouse.cs");


  // Set The GUI.
   Canvas.setContent(mainScreenGui);
   Canvas.setCursor(DefaultCursor);
   
   moveMap.push();
   
   if( isFile( %level ) || isFile( %level @ ".dso"))
      sceneWindow2D.loadLevel(%level);

	initMouse();
	complexMouse(); 
}

//---------------------------------------------------------------------------------------------
// endGame
// Game cleanup should be done here.
//---------------------------------------------------------------------------------------------
function endGame()
{
   sceneWindow2D.endLevel();
   moveMap.pop();
}
