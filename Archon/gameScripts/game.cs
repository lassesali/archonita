//---------------------------------------------------------------------------------------------
// Torque GameBuilder / Torque2D 4.0
// Archonita - Level Loading & Session Lifecycle
//---------------------------------------------------------------------------------------------

//---------------------------------------------------------------------------------------------
// SceneWindow Level Management (TGB -> Torque2D 4.0 compatibility)
//---------------------------------------------------------------------------------------------
function SceneWindow::loadLevel(%this, %level)
{
   if (isObject(t2dScene))
   {
      t2dScene.clear();
      t2dScene.delete();
   }

   // Check for .scene.taml alternative first
   %tamlPath = filePath(%level) @ "/" @ fileBase(%level) @ ".scene.taml";
   if (isFile(%tamlPath))
   {
      %sceneObj = TamlRead(%tamlPath);
      if (isObject(%sceneObj))
      {
         %this.setScene(%sceneObj);
         %this.setCameraPosition(0, 0);
         %this.setCameraSize(100, 75);
         return;
      }
   }

   // Fallback: execute modern script scene
   if (isFile(%level))
   {
      exec(%level);
      if (isObject(t2dScene))
      {
         %this.setScene(t2dScene);
         %this.setCameraPosition(0, 0);
         %this.setCameraSize(100, 75);
      }
   }
}

function SceneWindow::endLevel(%this)
{
   if (isObject(t2dScene))
   {
      t2dScene.clear();
      t2dScene.delete();
   }
}

// Native OpenAL audio is provided by Torque2D C++ engine (alxPlay, alxStop, alxStopAll)

//---------------------------------------------------------------------------------------------
// startGame
// All game logic should be set up here.
//---------------------------------------------------------------------------------------------
function startGame(%level)
{
   // Set The GUI.
   Canvas.setContent(mainScreenGui);
   Canvas.setCursor(DefaultCursor);
   Canvas.showCursor(); 
   
   if( isFile( %level ) || isFile( %level @ ".scene.taml" ))
      sceneWindow2D.loadLevel(%level);

   initMouse();
}

//---------------------------------------------------------------------------------------------
// endGame
// Game cleanup should be done here.
//---------------------------------------------------------------------------------------------
function endGame()
{
   stopBackgroundMusic();
   alxStopAll();
   if (isObject(testActionMap))
      testActionMap.pop();
   sceneWindow2D.endLevel();
   quit();
}
