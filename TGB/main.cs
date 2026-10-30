//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

new t2dSceneObjectDatablock(TestDB)
{
   class = "TestClass";
   linearVelocity = "10 10";
   testField = "TestValue";
};

//---------------------------------------------------------------------------------------------
// initializeProject
// Perform game initialization here.
//---------------------------------------------------------------------------------------------
function initializeProject()
{
   // Load up the in game gui.
   exec("~/gui/mainScreen.gui");
   
   // Exec game scripts.
   exec("./gameScripts/game.cs");
   
   // Remove the following four lines if you would like to start the game without running the
   // level builder.
 /*  if ($runWithEditors)
   {
      toggleLevelEditor();
      return;
   }
   */
   // This is where the game starts. Right now, we are just starting the first level. You will
   // want to expand this to load up a splash screen followed by a main menu depending on the
   // specific needs of your game. Most likely, a menu button will start the actual game, which
   // is where startGame should be called from.
   
   startGame($levelEditor::LastLevel[$currentProject]);
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
   new ActionMap(moveMap);
   //moveMap.bind("keyboard", "a", "doAction", "Action Description");
}
