//-----------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

function initializeTileEditor()
{
   echo( "\n--------- Initializing Tile Editor ---------" );
   
   // Load Preferences.
   exec("./preferences/defaultPrefs.ed.cs");
   exec("./preferences/prefs.cs");

   // Load-up Datablocks.
   exec("./data/content/datablocks.ed.cs");
   
   // Fetch Datablock Set.
   $datablockSet = getT2DDatablockSet();
   
   // Load-up GUIs.
   exec("./gui/tileScreenGui.ed.gui");
   exec("./gui/newTileDlg.ed.gui");
   exec("./gui/newLayerDlg.ed.gui");
   exec("./gui/viewDlg.ed.gui");
   exec("./gui/tileHelpDlg.ed.gui");
   exec("./gui/tileAssignBrushDlg.ed.gui");
   exec("./gui/resizeLayerDlg.ed.gui");
   exec("./gui/resizeTilesDlg.ed.gui");
   exec("./gui/layerBlendDlg.ed.gui");
   exec("./gui/editTileDlg.ed.gui");
   
   // Initialise Editor Code.
   exec("./scripts/editorScreen.ed.cs");
   exec("./scripts/imageSelection.ed.cs");
   
   // Tile-Editor Action-Map.	
   GlobalActionMap.bindCmd(keyboard, "f12", "toggleTileEditor();", "");
   
   // Create t2dSceneGraphs.
   new t2dSceneGraph(tileEditorSceneGraph2D);
   new t2dSceneGraph(tileImagesSceneGraph2D);
   
   // Associate Scenegraph with Window.
   tileSceneWindow2D.setSceneGraph( tileEditorSceneGraph2D );
   
   // Set Camera Position.
   tileSceneWindow2D.setCurrentCameraPosition( "0 0" SPC $tileEditor2D::viewWidth SPC $tileEditor2D::viewHeight );
   
   // Set Camera Zoom.
   tileSceneWindow2D.setCurrentCameraZoom( $tileEditor2D::viewZoom );
   
   // Associate Scenegraph with Window.
   imageSceneWindow2D.setSceneGraph( tileImagesSceneGraph2D );
   
   // Move to Top/Left Tile-Layer.
   tileLayerTopLeft();
   
   // Initialise Editor.
   initialiseEditor();
   
   // Pause the scenes by default.
   tileEditorSceneGraph2D.setScenePause( true );
   tileImagesSceneGraph2D.setScenePause( true );
}

function destroyTileEditor()
{
   // Destroy Editor.
   destroyEditor();
   
   // Destroy Editor t2dSceneGraphs.
   if (isObject(tileSceneWindow2D))
      tileSceneWindow2D.delete();
   if (isObject(tileImagesSceneGraph2D))
      tileImagesSceneGraph2D.delete();

   // Export Preferences.
   echo("Exporting Tile Editor preferences.");
   export("$tileEditor2D::*", "./Preferences/prefs.cs", false);
}

// --------------------------------------------------------------------
// Show Tile Editor.
// --------------------------------------------------------------------
function showTileEditor()
{
   // Unpause the tile-editor scenes.
   tileEditorSceneGraph2D.setScenePause( false );
   tileImagesSceneGraph2D.setScenePause( false );
   // Push Tile Editor-Key.
   tileEditorMap.push();
   
   // Show Tile-Editor GUI.
   Canvas.pushDialog( tileEditorScreenGUI );	
   
   // Set Tile-Editor State.
   $tileEditorActive = true;
}


// --------------------------------------------------------------------
// Hide Tile Editor.
// --------------------------------------------------------------------
function hideTileEditor()
{
   // Pause the tile-editor scenes.
   tileEditorSceneGraph2D.setScenePause( true );
   tileImagesSceneGraph2D.setScenePause( true );
   // Pop Tile Editor-Key.
   tileEditorMap.pop();
   
   // Hide Tile-Editor GUI.
   Canvas.popDialog( tileEditorScreenGUI );
   
   // Set Tile-Editor State.
   $tileEditorActive = false;
}

// --------------------------------------------------------------------
// Toggle Tile Editor.
// --------------------------------------------------------------------
$tileEditorActive = false;
function toggleTileEditor()
{
   // Ignore if we've not got a tile-editor GUI.
   if ( !isObject( tileEditorScreenGUI ) )
      return;
   
   // Toggle Tile-Editor. 	
   if ( $tileEditorActive )
      hideTileEditor();
   else
      showTileEditor();
}
