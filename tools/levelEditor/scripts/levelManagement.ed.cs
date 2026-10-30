//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

$currentLevelFile = "";
$currentSceneGraph = "";


function getLevelSaveName( %callback )
{
   %dialog       =  SaveFileDlgEx;
   %filterList   =  %dialog.findObjectByInternalName("FilterList", true);
      
   if( $currentLevelFile !$= "" )
      %currentFile = $currentLevelFile;
   else
      %currentFile = $currentProject @ "/data/levels/untitled.t2d";
   
   // Clear Filters
   %dialog.ClearFilters();
   
   // Add Filters
   %filter = %dialog.AddFilter("*.t2d","TGB Level Files");
 
   // Select Filter.
   %filterList.setSelected( %filter );
   
   getSaveFileName("*.t2d", %callback, %currentFile);   
   
}

function getLevelOpenName( %callback )
{
   %dialog       =  OpenFileDlgEx;
   %filterList   =  %dialog.findObjectByInternalName("FilterList", true);
      
   if( $currentLevelFile !$= "" )
      %currentFile = $currentLevelFile;
   else
      %currentFile = $currentProject @ "/data/levels/";

   // Clear Filters
   %dialog.ClearFilters();
   
   // Add Filters
   %filter = %dialog.AddFilter("*.t2d","TGB Level Files");
   
   // Select Filter.
   %filterList.setSelected( %filter );

 
   getLoadFileName("*.t2d", %callback, %currentFile);   
     
}

//---------------------------------------------------------------------------------------------
// New Level 
// Will create a new level after verifying changes to existing level if any.
//---------------------------------------------------------------------------------------------
function newLevel()
{
   %levelName = $currentLevelFile;
   if( %levelName $= "" )
      %levelName = "(Untitled)";

   if( LevelBuilderUndoManager.getUndoCount() > 0 || LevelBuilderUndoManager.getRedoCount() > 0 )
   {
      // Ask user to save or not to save... that is the question.
      checkSaveChanges( %levelName, doNewLevelSave, doNewLevel );
      return;
   }  

   // No changes, just do new level.  
   doNewLevel();
}

function doNewLevelSave( %data )
{
   if ($currentLevelFile $= "" && !isFile( %data ) )
   {
      getLevelSaveName( doNewLevelSave );
      return;
   }
   
   if( isFile( %data ) )
      $currentLevelFile = %data;
      
   saveLevel( doNewLevel );

}
function doNewLevel()
{
   // Make sure we're on selection tool (deactivate active tool so it cleans up)
   LevelBuilderToolManager::setTool( LevelEditorSelectionTool );
   
   // Open the Current SceneGraph, if any.
   %lastWindow = ToolManager.getLastWindow();
   if( ! isObject( %lastWindow ) )
   {
      MessageBoxOk("Error","Cannot Create New Level, No Scene Views" );
      return false;
   }
   
   %lastWindow.endLevel();
      
   %lastWindow.scenegraph = new t2dSceneGraph();
   %lastWindow.scenegraph.cameraPosition = "0 0";
   %lastWindow.scenegraph.cameraSize = "100 75";
   %lastWindow.setSceneGraph(%lastWindow.scenegraph);
   
   %lastWindow.setCurrentCameraPosition(%lastWindow.scenegraph.cameraPosition,
                                                   %lastWindow.scenegraph.cameraSize);
                                                   
   $currentSceneGraph = %lastWindow.scenegraph;
   $currentLevelFile = "";
                                                   
   // Notify Views of Load
   GuiFormManager::SendContentMessage( $LBSceneViewContent, 0, "updateScenegraph " @ %lastWindow.scenegraph );
   // Notify TreeView of Load
   GuiFormManager::SendContentMessage( $LBTreeViewContent, 0, "openCurrentGraph" );

   LevelBuilderUndoManager.clearAll();
}

function saveLevel( %callback )
{
   // Make sure we're on selection tool (deactivate active tool so it cleans up)
   LevelBuilderToolManager::setTool( LevelEditorSelectionTool );
   
   if ($currentLevelFile $= "")
   {
      saveLevelAs( %callback );
      return;
   }
   
   // Open the Current SceneGraph, if any.
   %lastWindow = ToolManager.getLastWindow();
   if( !isObject( %lastWindow ) || !isObject( %lastWindow.getScenegraph() ) )
   {
      // Nope, create the level for them.
      MessageBoxOk( "Problem Saving Level", "There was a problem saving your level as one did not exist.\nOne has been created for you to work in and you may now save your level properly" );
      
      newLevel();
      
      return;
   }
   
   %sceneGraph = %lastWindow.getScenegraph();
   
   // Persist Objects
   if( isObject( $persistentObjectSet ) && isWriteableFileName($currentProject @ "/managed/persistent.cs") )
   {
      %fo = new FileObject();
      %fo.openForWrite($currentProject @ "/managed/persistent.cs");
      %fo.writeObject($persistentObjectSet, "$persistentObjectSet = ");
      %fo.close();
      %fo.delete();
      
      %count = $persistentObjectSet.getCount();
      for (%i = 0; %i < %count; %i++)
         %scenegraph.removeFromScene($persistentObjectSet.getObject(%i));
   }
   
   // Save the Level
   if( isWriteableFileName( $currentLevelFile ) )
   {
      %fo = new FileObject();
      %fo.openForWrite($currentLevelFile);
      %fo.writeObject(%scenegraph, "%levelContent = ");
      %fo.close();
      %fo.delete();
   }

   if( isFile( $currentProject @ "/managed/persistent.cs" ) )
   {
      // Reload 
      exec($currentProject @ "/managed/persistent.cs");   
      for (%i = 0; %i < %count; %i++)
         %scenegraph.addToScene($persistentObjectSet.getObject(%i));          
   }  
   
   $levelEditor::LastLevel[$currentProject] = $currentLevelFile;
   
   // Clear Dirty Flags and Undo's
   LevelBuilderUndoManager.clearAll();
   
   // Level was saved.
   if( %callback !$= "" )
      eval( %callback @ "();" );
}

function saveLevelAs( %callback )
{
   getLevelSaveName( saveLevelCallback );
   //getSaveFilename("", saveLevelCallback);
}

function saveLevelAsAndTest()
{
   getLevelSaveName( saveLevelAndTestCallback );
}

function saveLevelCallback(%filename)
{
   $currentLevelFile = %filename;
   saveLevel();
}

function saveLevelAndTestCallback(%filename)
{
   $currentLevelFile = %filename;
   runGame();
}

function loadLevelCallback(%filename)
{
   %lastWindow = ToolManager.getLastWindow();
   if( !isObject( %lastWindow ) )
   {
      error("loadLevelCallback - No Valid Window!");
      return false;
   }
   
   // Make sure we're on selection tool (deactivate active tool so it cleans up)
   LevelBuilderToolManager::setTool( LevelEditorSelectionTool );  

   %scenegraph = %lastWindow.loadLevel(%filename);
   
   if (isObject(%scenegraph))
   {
      $currentLevelFile = %filename;
      $currentSceneGraph = %scenegraph;

      // Notify Views of Load
      GuiFormManager::SendContentMessage( $LBSceneViewContent, 0, "updateScenegraph " @ %scenegraph );
      // Notify TreeView of Load
      GuiFormManager::SendContentMessage( $LBTreeViewContent, 0, "openCurrentGraph" );
      
      showTriggers(%scenegraph);
   }
   else
      return false;
   
   $levelEditor::LastLevel[$currentProject] = $currentLevelFile;

   LevelBuilderUndoManager.clearAll();
   
   return true;
}

function addToLevelCallback(%filename)
{
   %lastWindow = ToolManager.getLastWindow();
   if( !isObject( %lastWindow ) )
   {
      error("loadLevelCallback - No Valid Window!");
      return false;
   }

   %scenegraph = %lastWindow.addToLevel(%filename);

   // Notify TreeView of Load
   GuiFormManager::SendContentMessage( $LBTreeViewContent, 0, "openCurrentGraph" );
   
   showTriggers(%scenegraph);
}

function showTriggers(%scenegraph)
{
   %count = %scenegraph.getSceneObjectCount();
   for (%i = 0; %i < %count; %i++)
   {
      %obj = %scenegraph.getSceneObject(%i);
      if( !isObject( %obj ) ) 
         continue;
      if ((%obj.getClassName() $= "t2dSceneObject") || (%obj.getClassName() $= "t2dTrigger") || (%obj.getClassname() $= "t2dPath"))
         %obj.setDebugOn(1);
   }
}

$runningGame = false;
function runGame()
{
   if ($currentLevelFile $= "")
   {
      MessageBoxOK("Save Level", "You must save your level before it can be tested.", "saveLevelAsAndTest();");
      return;
   }
   else
      saveLevel();

   %oldPref = $Scripts::ignoreDSOs;
   
   $Scripts::ignoreDSOs = true;

   exec($currentProject @ "/gameScripts/game.cs");
   
   toggleLevelEditor();
   
   $runningGame = true;
      
   startGame($currentLevelFile);
   
   $Scripts::ignoreDSOs = ( %oldPref $= "" ) ? false : %oldPref;

   
   if( isObject( LBPlayWindowCtrlStack ) )
      sceneWindow2D.add(LBPlayWindowCtrlStack);
      
   checkForScriptErrors();
}

$ScriptErrorMessageHash = 999;
function checkForScriptErrors()
{
   if ($levelEditor::DisplayScriptErrors == false)
   {
      $ScriptErrorMessageHash = $ScriptErrorHash;
      return;
   }
      
   if (($ScriptErrorHash != $ScriptErrorMessageHash) && ($ScriptErrorHash != 0))
   {
      %count = getFieldCount($ScriptError);
      %numErrors = $ScriptErrorHash - $ScriptErrorMessageHash;
      %errorString = "";
      for (%i = 0; %i < %numErrors; %i++)
      {
         %field = %count - %numErrors + %i;
         if (%errorString !$= "")
            %errorString = %errorString @ "\n" @ getField($ScriptError, %field);
         else
            %errorString = getField($ScriptError, %field);
      }
      MessageBoxOK("Script Errors Found!", %errorString @ "\n\n" @ "Check the console (Press '~') for details.");
         
      $ScriptErrorMessageHash = $ScriptErrorHash;
   }
}

function endRunGame()
{
   if ($runningGame)
      endGame();

   if( isObject( LBPlayWindowCtrlStack ) && sceneWindow2D.isMember( LBPlayWindowCtrlStack ) )
      sceneWindow2D.remove(LBPlayWindowCtrlStack);
   
   exec($currentProject @ "/managed/persistent.cs");   
   %count = $persistentObjectSet.getCount();
   for (%i = 0; %i < %count; %i++)
      %sceneGraph.addToScene($persistentObjectSet.getObject(%i));
      
   $runningGame = false;
}

function pauseRunGame()
{
   if ($runningGame)
   {
      // Somehow need to get the users scenegraph to pause it.
   }
}

function saveSelectedGroup()
{
   if (isObject(ToolManager.getAcquiredGroup()))
      getSaveFilename("", saveSelectedGroupCallback);
}

function saveSelectedGroupCallback(%filename)
{
   %group = ToolManager.getAcquiredGroup();
   if (isObject(%group))
   {
      %fo = new FileObject();
      %fo.openForWrite(%filename);
      %fo.writeObject(%group, "%levelContent = ");
      %fo.close();
      %fo.delete();
   }
}

function saveSelectedObjects()
{
   if (ToolManager.getAcquiredObjectCount() > 0)
      getSaveFilename("", saveSelectedObjectsCallback);
}

function saveSelectedObjectsCallback(%filename)
{
   %group = ToolManager.getAcquiredGroup();
   if (isObject(%group))
   {
      saveSelectedGroupCallback(%filename);
      return;
   }
   
   %objects = ToolManager.getAcquiredObjects();
   
   if (!isObject(%objects))
      return;

   if( isFile( %fileName @ ".dso" ) )
      fileDelete( %fileName @ ".dso" );

   %count = %objects.getCount();
   // A single object can just be saved out on its own.
   if (%count == 1)
   {
      %fo = new FileObject();
      %fo.openForWrite(%filename);
      %fo.writeObject(%objects.getObject(0), "%levelContent = ");
      %fo.close();
      %fo.delete();
   }
   
   // Multiple objects we're going to put in a single group for saving.
   else if (%count > 1)
   {
      // Create a group to house the selected objects.
      %newGroup = new t2dSceneObjectGroup();
      
      // Temporarily move stuff to the new group.
      for (%i = 0; %i < %count; %i++)
      {
         %object = %objects.getObject(%i);
         %oldGroup[%i] = %object.getGroup();
         if (!isObject(%oldGroup[%i]))
            %oldGroup[%i] = ToolManager.getLastWindow().getSceneGraph();
         %newGroup.add(%object);
      }
      
      %fo = new FileObject();
      %fo.openForWrite(%filename);
      %fo.writeObject(%newGroup, "%levelContent = ");
      %fo.close();
      %fo.delete();
      
      // And move stuff back to its original group, or at least out of the new group.
      for (%i = 0; %i < %count; %i++)
      {
         %object = %objects.getObject(%i);
         %newGroup.remove(%object);
         if (isObject(%oldGroup[%i]))
            %oldGroup[%i].add(%object);
      }
      
      // Kill the new group.
      %newGroup.delete();
   }
}

$layerToSave = -1;
function saveLayer(%layer)
{
   $layerToSave = %layer;
   getSaveFilename("", saveLayerCallback);
}

function saveLayerCallback(%filename)
{
   // Create a group to house the selected objects.
   %newGroup = new t2dSceneObjectGroup();

   // Open the Current SceneGraph, if any.
   %lastWindow = ToolManager.getLastWindow();
   if( !isObject( %lastWindow ) || !isObject( %lastWindow.getScenegraph() ) )
   {
      error("SaveGraphGroup - No Valid SceneGraph Found!");
      return;
   }
   %sceneGraph = %lastWindow.getScenegraph();

   %count = %scenegraph.getSceneObjectCount();   
   // Temporarily move stuff to the new group.
   for (%i = 0; %i < %count; %i++)
   {
      %object = %scenegraph.getSceneObject(%i);
      
      if (%object.getLayer() == $layerToSave)
      {
         %oldGroup[%i] = %object.getGroup();
         %newGroup.add(%object);
      }
   }
   
   // Save the new group.
   if (%newGroup.getCount() > 0)
   {
      %fo = new FileObject();
      %fo.openForWrite(%filename);
      %fo.writeObject(%newGroup, "%levelContent = ");
      %fo.close();
      %fo.delete();
   }
   
   // And move stuff back to its original group, or at least out of the new group.
   for (%i = 0; %i < %count; %i++)
   {
      %object = %scenegraph.getSceneObject(%i);
      
      if (%object.getLayer() == $layerToSave)
      {
         %newGroup.remove(%object);
         if (isObject(%oldGroup[%i]))
            %oldGroup[%i].add(%object);
      }
   }
   
   // Kill the new group.
   %newGroup.delete();
   $layerToSave = -1;
}

$graphGroupToSave = -1;
function saveGraphGroup(%graphGroup)
{
   $graphGroupToSave = %graphGroup;
   getSaveFilename("", saveGraphGroupCallback);
}

function saveGraphGroupCallback(%filename)
{
   // Create a group to house the selected objects.
   %newGroup = new t2dSceneObjectGroup();

   // Open the Current SceneGraph, if any.
   %lastWindow = ToolManager.getLastWindow();
   if( !isObject( %lastWindow ) || !isObject( %lastWindow.getScenegraph() ) )
   {
      error("SaveGraphGroup - No Valid SceneGraph Found!");
      return;
   }
   %sceneGraph = %lastWindow.getScenegraph();

   %count = %scenegraph.getSceneObjectCount();   
   // Temporarily move stuff to the new group.
   for (%i = 0; %i < %count; %i++)
   {
      %object = %scenegraph.getSceneObject(%i);
      
      if (%object.getGraphGroup() == $groupToSave)
      {
         %oldGroup[%i] = %object.getGroup();
         %newGroup.add(%object);
      }
   }
   
   // Save the new group.
   if (%newGroup.getCount() > 0)
   {
      %fo = new FileObject();
      %fo.openForWrite(%filename);
      %fo.writeObject(%newGroup, "%levelContent = ");
      %fo.close();
      %fo.delete();
   }
   
   // And move stuff back to its original group, or at least out of the new group.
   for (%i = 0; %i < %count; %i++)
   {
      %object = %scenegraph.getSceneObject(%i);
      
      if (%object.getGraphGroup() == $groupToSave)
      {
         %newGroup.remove(%object);
         if (isObject(%oldGroup[%i]))
            %oldGroup[%i].add(%object);
      }
   }
   
   // Kill the new group.
   %newGroup.delete();
   $graphGroupToSave = -1;
}
