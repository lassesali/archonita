//-----------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Editor State.
//-----------------------------------------------------------------------------
$mouseMode = "";
$viewPosition = "0 0";
$collisionDrawStatus = false;
$viewGuideStatus = true;
$viewAllLayers = false;
$viewUptoCurrentLayer = false;
$tileInfoButtonFlag = true;

//-----------------------------------------------------------------------------
// Initialise Editor.
//-----------------------------------------------------------------------------
function initialiseEditor()
{
	// Create Cursor.
	createCursor();

	// Initialise Brushes.
	initialiseBrushes();

	// Initialise Editor Menu.
	initialiseEditorMenu();

	// Clear Undo/Redo.		
	clearUndoRedo();	
	
	// Set Default Brush.
	selectActiveBrush( 1 );
	
	// Initialise Tile-Map.
	initialiseTileMap( $tileEditor2D::defaultTileCountX, $tileEditor2D::defaultTileCountY, $tileEditor2D::defaultTileSizeX, $tileEditor2D::defaultTileSizeY, $tileEditor2D::defaultTileLayers );
	
	// Create a new action-map.
	new ActionMap(tileEditorMap);
	// Setup Tile-Editor Key-Bindings.
	tileEditorMap.bindCmd(keyboard, "pageup", "tileLayerPrevious();", "");
	tileEditorMap.bindCmd(keyboard, "pagedown", "tileLayerNext();", "");
	tileEditorMap.bindCmd(keyboard, "up", "tileLayerUp();", "");
	tileEditorMap.bindCmd(keyboard, "down", "tileLayerDown();", "");
	tileEditorMap.bindCmd(keyboard, "left", "tileLayerLeft();", "");
	tileEditorMap.bindCmd(keyboard, "right", "tileLayerRight();", "");
	tileEditorMap.bindCmd(keyboard, "home", "tileLayerTopLeft();", "");
	tileEditorMap.bindCmd(keyboard, "lbracket", "zoomOut();", "");
	tileEditorMap.bindCmd(keyboard, "rbracket", "zoomIn();", "");
	tileEditorMap.bindCmd(keyboard, "a", "toggleViewAllLayers();", "");
	tileEditorMap.bindCmd(keyboard, "c", "toggleUptoCurrentLayer();", "");
	tileEditorMap.bindCmd(keyboard, "s", "setCollisionDrawSuppress(!$collisionDrawSuppressStatus);", "");
	tileEditorMap.bindCmd(keyboard, "o", "setBrushOverwrites(!$tileEditor2D::confirmBrushOverwrites);", "");

	// Brush Shortcuts.
	// NOTE:-	We only map the first 9 brushes and we assume we've got
	//			at least 9 brushes.
	for ( %n = 1; %n < 10; %n++ )
		tileEditorMap.bindCmd(keyboard, %n, "selectActiveBrush(" @ %n @ ");", "");

	// Read Image Selection.
	readImageSelection();

	// Set Menu-Item Checked-Status.
	tileScreenMenu.setMenuItemChecked("View", "Border", $tileInfoButtonFlag);		
	// Set Menu-Item Checked-Status.
	tileScreenMenu.setMenuItemChecked("View", "Gridlines", $tileEditor2D::viewGridlines);
	// Set Menu-Item Checked-Status.
	tileScreenMenu.setMenuItemChecked("View", "Script Icons", $tileEditor2D::viewScriptIcons);
	// Set Menu-Item Checked-Status.
	tileScreenMenu.setMenuItemChecked("View", "Custom-Data Icons", $tileEditor2D::viewCustomIcons);
	// Set Menu-Item Checked-Status.
	tileScreenMenu.setMenuItemChecked("View", "Image Library", $tileEditor2D::viewImageLibrary);

	// Set Tile Border Appropriately.
	if ( $tileInfoButtonFlag )
		tileEditorSceneGraph2D.setDebugOn( 1, 5 );
	else
		tileEditorSceneGraph2D.setDebugOff( 1, 5 );
        
    // Set Image Library Statys.
    setImageLibraryStatus( $tileEditor2D::viewImageLibrary );
}


//-----------------------------------------------------------------------------
// Destroy Editor.
//-----------------------------------------------------------------------------
function destroyEditor()
{
	// Destroy Cursor.
	destroyCursor();
	
	// Destroy Brushes.
	destroyBrushes();
	
	// Destroy Editor Map.
	if ( isObject(tileEditorMap) )
		tileEditorMap.delete();

	// Reset Undo/Redo.		
	clearUndo();
	clearRedo();
}



//-----------------------------------------------------------------------------
// Initialise Editor Menu.
//-----------------------------------------------------------------------------
function initialiseEditorMenu()
{
	// Clear Menu.
	tileScreenMenu.clearMenus();
	
	// File Menu.
	tileScreenMenu.addMenu("File", 0);
	tileScreenMenu.addMenuItem("File", "New Tile-Map...", 0);
	tileScreenMenu.addMenuItem("File", "-", 0);
	tileScreenMenu.addMenuItem("File", "Load Tile-Map...", 1);
	tileScreenMenu.addMenuItem("File", "Load Tile-Layer...", 2);
	tileScreenMenu.addMenuItem("File", "Append Tile-Layer...", 3);
	tileScreenMenu.addMenuItem("File", "Save Tile-Map...", 4);
	tileScreenMenu.addMenuItem("File", "Save Tile-Layer...", 5);
	tileScreenMenu.addMenuItem("File", "-", 0);
	tileScreenMenu.addMenuItem("File", "Quit...", 6);
	// File Menu Commands.
	tileScreenMenu.scriptCommand["File", 0] = "launchNewTileDialog();";
	tileScreenMenu.scriptCommand["File", 1] = "TileMapEditor::loadFile(\"*.map\", loadMap, $currentProject @ \"/data/tilemaps/\");";
	tileScreenMenu.scriptCommand["File", 2] = "TileMapEditor::loadFile(\"*.lyr\", loadLayer, $currentProject @ \"/data/tilemaps/\");";
	tileScreenMenu.scriptCommand["File", 3] = "TileMapEditor::loadFile(\"*.lyr\", appendLoadLayer, $currentProject @ \"/data/tilemaps/\");";
	tileScreenMenu.scriptCommand["File", 4] = "TileMapEditor::saveMapWarning();";
	tileScreenMenu.scriptCommand["File", 5] = "TileMapEditor::saveFile(\"*.lyr\", saveTileLayer, $currentProject @ \"/data/tilemaps/\");";
	tileScreenMenu.scriptCommand["File", 6] = "toggleTileEditor();";

	
	// Edit Menu.
	tileScreenMenu.addMenu("Edit", 1);
	tileScreenMenu.addMenuItem("Edit", "Undo", 0);
	tileScreenMenu.addMenuItem("Edit", "Redo", 1);
	// Edit Menu Commands.
	tileScreenMenu.scriptCommand["Edit", 0] = "undo();";
	tileScreenMenu.scriptCommand["Edit", 1] = "redo();";
	
	
	// Layers Menu.
	tileScreenMenu.addMenu("Layers", 2);
	tileScreenMenu.addMenuItem("Layers", "Add Layer...", 0);
	tileScreenMenu.addMenuItem("Layers", "Remove Layer...", 1);
	tileScreenMenu.addMenuItem("Layers", "Clear Layer...", 2);
	tileScreenMenu.addMenuItem("Layers", "-", 0);
	tileScreenMenu.addMenuItem("Layers", "Resize Layer...", 3);
	tileScreenMenu.addMenuItem("Layers", "Resize Tiles...", 4);
	tileScreenMenu.addMenuItem("Layers", "-", 0);
	tileScreenMenu.addMenuItem("Layers", "Layer Blend...", 5);
	// Layers Menu Commands.
	tileScreenMenu.scriptCommand["Layers", 0] = "launchNewLayerDialog();";
	tileScreenMenu.scriptCommand["Layers", 1] = "MessageBoxYesNo( \"Remove Tile-Layer\", \"Are you sure you want to remove the current Tile-Layer?\", \"removeTileLayer();\", \"\" );";
	tileScreenMenu.scriptCommand["Layers", 2] = "MessageBoxYesNo( \"Clear Tile-Layer\", \"Are you sure you want to clear the current Tile-Layer?\", \"clearTileLayer();\", \"\" );";
	tileScreenMenu.scriptCommand["Layers", 3] = "resizeTileLayer();";
	tileScreenMenu.scriptCommand["Layers", 4] = "resizeTiles();";
	tileScreenMenu.scriptCommand["Layers", 5] = "layerBlend();";
	
	// Brushes Menu.
	tileScreenMenu.addMenu("Brush", 3);
	// Read Brush Selection.
	readBrushSelection();

	// View Menu.	
	tileScreenMenu.addMenu("View", 4);
	// Read Tile Layer Selection.
	readTileLayerSelection();
	
	// Help Menu.
	tileScreenMenu.addMenu("Help", 5);
	tileScreenMenu.addMenuItem("Help", "Shortcut Keys...", 0);
	tileScreenMenu.addMenuItem("Help", "-", 0);
	tileScreenMenu.addMenuItem("Help", "About...", 1);
	// Help Menu Commands.
	tileScreenMenu.scriptCommand["Help", 0] = "Canvas.pushDialog(tileHelpDlg);";
	tileScreenMenu.scriptCommand["Help", 1] = "MessageBoxOK(\"About...\", \"TGB Tile Builder v1.2\", \"\");";
}

function TileMapEditor::saveMapWarning()
{
   MessageBoxOKCancel("Warning", "Tile maps cannot be used with the level builder. For that, use tile layers instead.",
                      "TileMapEditor::saveFile(\"*.map\", saveMap, $currentProject @ \"/data/tilemaps/\");");
}

function TileMapEditor::loadFile(%ext, %callback, %currentFile)
{
   TileMapEditor::addFilter(0, %ext);
   
   getLoadFilename(%ext, %callback, %currentFile);
}

function TileMapEditor::saveFile(%ext, %callback, %currentFile)
{
   TileMapEditor::addFilter(1, %ext);
   
   getSaveFilename(%ext, %callback, %currentFile);
}

function TileMapEditor::addFilter(%dlg, %ext)
{
 if(%dlg)
 {
    %dialog       =  SaveFileDlgEx;
 } else
 {
    %dialog       =  OpenFileDlgEx;
 }
 %filterList   =  %dialog.findObjectByInternalName("FilterList", true);
 
 // Clear Filters
   %dialog.ClearFilters();
   
 // Add Filters
 // Select Filter.
 if(%ext $= "*.map")
 {
    %filter = %dialog.AddFilter("*.map","TGB Tile Map Files");
    %filterList.setSelected( %filter );   
 } else if (%ext $= "*.lyr")
 {
    %filter = %dialog.AddFilter("*.lyr","TGB Tile Layer Files");
    %filterList.setSelected ( %filter );
 }
}

//-----------------------------------------------------------------------------
// Set Image Library Status.
//-----------------------------------------------------------------------------
function setImageLibraryStatus( %status )
{
    // Set View Image Library Preference.
    $tileEditor2D::viewImageLibrary = %status;
    
    // Set Image Library Window Visibility.
    tileEditorImageLibrary.setVisible( %status );
}

//-----------------------------------------------------------------------------
// Menu Item Select Command Processing.
//-----------------------------------------------------------------------------
function tileScreenMenu::onMenuItemSelect(%this, %menuId, %menu, %itemId, %item)
{
	// Script-Command Available?
	if(%this.scriptCommand[%menu, %itemId] !$= "")
		// Yes, so execute it.
		eval(%this.scriptCommand[%menu, %itemId]);
	else
		// No, so error!
		error("No script command defined for menu " @ %menu  @ " item " @ %item);
}


//-----------------------------------------------------------------------------
// Update Undo/Redo Menu Enabled Status.
//-----------------------------------------------------------------------------
function updateUndoRedoMenu()
{
	// Update Undo/Redo Enabled Status.
	tileScreenMenu.setMenuItemEnable("Edit", "Undo", $undoAvailable );
	tileScreenMenu.setMenuItemEnable("Edit", "Redo", $redoAvailable );
}


//-----------------------------------------------------------------------------
// Store Undo.
//-----------------------------------------------------------------------------
function storeUndo()
{
	// Clear Undo/Redo.		
	clearUndoRedo();
	
	// Check Tile-Map.
	if ( !isObject($editorTileMap) )
		return;

	// Fetch Tile Layer Count.
	%layerCount = $editorTileMap.getTileLayerCount();
	
	// Sanity!
	if ( %layerCount == 0 )
		return;
	
	// Get Active Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );

	// Check Tile Layer.	
	if ( !isObject(%tileLayer) )
		return;
		
	// Save Tile Layer.
	if ( !%tileLayer.saveTileLayer( "~/client/undo.dat" ) )
		return;
		
	// Set Undo Available.
	$undoAvailable = true;
	
	// Update Undo/Redo Enabled Status.
	updateUndoRedoMenu();	
}


//-----------------------------------------------------------------------------
// Undo.
//-----------------------------------------------------------------------------
function undo()
{
	// Ignore if undo is not available.
	if ( !$undoAvailable )
		return;
		
	// Clear Undo.
	clearUndo();

	// Check Tile-Map.
	if ( !isObject($editorTileMap) )
		return;

	// Fetch Tile Layer Count.
	%layerCount = $editorTileMap.getTileLayerCount();
	
	// Sanity!
	if ( %layerCount == 0 )
		return;
	
	// Get Active Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );

	// Check Tile Layer.	
	if ( !isObject(%tileLayer) )
		return;
		
	// Store Redo.
	storeRedo();		
		
	// Load Tile Layer.
	if ( !%tileLayer.loadTileLayer( "~/client/undo.dat" ) )
		return;
		
	// Update Undo/Redo Enabled Status.
	updateUndoRedoMenu();	
}


//-----------------------------------------------------------------------------
// Clear Undo.
//-----------------------------------------------------------------------------
function clearUndo()
{
	// Reset Undo Availability.
	$undoAvailable = false;
	
	// Update Undo/Redo Enabled Status.
	updateUndoRedoMenu();	
}


//-----------------------------------------------------------------------------
// Store Redo.
//-----------------------------------------------------------------------------
function storeRedo()
{
	// Clear Redo.
	clearRedo();
	
	// Check Tile-Map.
	if ( !isObject($editorTileMap) )
		return;

	// Fetch Tile Layer Count.
	%layerCount = $editorTileMap.getTileLayerCount();
	
	// Sanity!
	if ( %layerCount == 0 )
		return;
	
	// Get Active Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );

	// Check Tile Layer.	
	if ( !isObject(%tileLayer) )
		return;
		
	// Save Tile Layer.
	if ( !%tileLayer.saveTileLayer( "~/client/redo.dat" ) )
		return;
		
	// Set Redo Available.
	$redoAvailable = true;
	
	// Update Undo/Redo Enabled Status.
	updateUndoRedoMenu();	
}


//-----------------------------------------------------------------------------
// Redo.
//-----------------------------------------------------------------------------
function redo()
{
	// Ignore if redo is not available.
	if ( !$redoAvailable )
		return;
		
	// Clear Redo.
	clearRedo();

	// Check Tile-Map.
	if ( !isObject($editorTileMap) )
		return;

	// Fetch Tile Layer Count.
	%layerCount = $editorTileMap.getTileLayerCount();
	
	// Sanity!
	if ( %layerCount == 0 )
		return;
	
	// Get Active Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );

	// Check Tile Layer.	
	if ( !isObject(%tileLayer) )
		return;

	// Store Undo.
	storeUndo();		
		
	// Load Tile Layer.
	if ( !%tileLayer.loadTileLayer( "~/client/redo.dat" ) )
		return;
		
	// Update Undo/Redo Enabled Status.
	updateUndoRedoMenu();	
}


//-----------------------------------------------------------------------------
// Clear Redo.
//-----------------------------------------------------------------------------
function clearRedo()
{
	// Reset Redo Availability.
	$redoAvailable = false;
	
	// Update Undo/Redo Enabled Status.
	updateUndoRedoMenu();	
}



//-----------------------------------------------------------------------------
// Clear Undo / Redo.
//-----------------------------------------------------------------------------
function clearUndoRedo()
{
	// Clear Undo/Redo.
	clearUndo();
	clearRedo();
}



//-----------------------------------------------------------------------------
// Load Map.
//-----------------------------------------------------------------------------
function loadMap( %mapFile )
{
	// Clear Undo/Redo.		
	clearUndoRedo();

	// Check Tile-Map.
	if ( isObject($editorTileMap) )
	{
		// Save Tile Map.
		if ( $editorTileMap.saveTileMap( "~/tileEditor/data/map.dat" ) )
			%mapRestore = true;
		else
			%mapRestore = false;
	}
	
	// Destroy Tile-Map (if it exists).
	if ( isObject($editorTileMap) )
		$editorTileMap.safeDelete();

	// Create new Tile-Map.
	$editorTileMap = new t2dTileMap() { scenegraph = tileEditorSceneGraph2D; };
	
	// Check Tile-Map.
	if ( !isObject($editorTileMap) )
	{
		// Problem, so warn.
		MessageBoxOK("Load Map", "There was a problem loading the map!", "");
		return;
	}
	
	// Load Tile Map.
	if ( !$editorTileMap.loadTileMap( %mapFile ) )
	{
		// Failed!
		%loadFailed = true;
		
		// Restore map is there wasn't a problem with it.
		if ( %mapRestore )
			// Restore Map.
			$editorTileMap.loadTileMap( "~/tileEditor/data/map.dat" );
	}
	else
	{
		// Didn't Fail.
		%loadFailed = false;
	}

	// Read Tile Layer Selection.
	readTileLayerSelection();
	
	// Select Last Layer.	
	selectTileLayer( $editorTileMap.getTileLayerCount() - 1 );
	
	// Move to Top/Left Tile-Layer.
	tileLayerTopLeft();
	
	// Handle Map Load Failed.
	if ( %loadFailed )
	{
		// Problem, so warn.
		MessageBoxOK("Load Map", "The load failed!", "");
		// Return Here.
		return;
	}

	// Initialise Map Details.
	%mapDetails = "";
	
	// Fetch Tile Layer Count.
	%layerCount = $editorTileMap.getTileLayerCount();
	
	// Any Layers?
	if ( %layerCount > 0 )
	{
		// Yes, so compile Layer Details.
		for ( %n = 0; %n < %layerCount; %n++ )
		{
			// Get Active Tile Layer.
			%tileLayer = $editorTileMap.getTileLayer( %n );
			
			// Get Underlying Tile-Layer Dimensions.
			%tileCount = %tileLayer.getTileCount();
			%tileSize = %tileLayer.getTileSize();
		
			// Append Map Details.
			%mapDetails = %mapDetails @ "\n\nLayer #" @ %n @ " - Quantity:" @ %tileCount SPC " / Size:" @ %tileSize;
		}		
	}
	
	// Confirm.	
	MessageBoxOK("Load Map", "The MAP was appended successfully." @ %mapDetails, "");
}


//-----------------------------------------------------------------------------
// Load Layer.
//-----------------------------------------------------------------------------
function loadLayer( %layerFile )
{
	// Clear Undo/Redo.		
	clearUndoRedo();
	
	// Check Tile-Map.
	if ( !isObject($editorTileMap) )
	{
		// Problem, so warn.
		MessageBoxOK("Load Layer", "There was a problem loading the layer!", "");
		return;
	}

	// Fetch Tile Layer Count.
	%layerCount = $editorTileMap.getTileLayerCount();
	
	// Sanity!
	if ( %layerCount == 0 )
	{
		// Problem, so warn.
		MessageBoxYesNo("Load Layer", "There are no layers to load into!  Create a new one?", "appendLoadLayer(" @ %layerFile @ ");", "" );
		// Finish Here.
		return;
	}
	
	// Get Active Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );
	
	// Check Tile Layer.
	if ( !isObject(%tileLayer) )
	{
		// Problem, so warn.
		MessageBoxOK("Load Layer", "There was a problem accessing the current layer!", "");
		// Finish Here.
		return;
	}
	
	// Store Undo.
	storeUndo();
	
	// Load Tile Layer.
	if ( !%tileLayer.loadTileLayer( %layerFile ) )
	{
		// Problem, so warn.
		MessageBoxOK("Load Layer", "The load failed!", "");
		// Restore Layer.
		undo();
		// Finish Here.		
		return;
	}	

	// Select Last Layer.	
	selectTileLayer( $editorTileMap.getTileLayerCount() - 1 );
	
	// Get Underlying Tile-Layer Dimensions.
	%tileCount = %tileLayer.getTileCount();
	%tileSize = %tileLayer.getTileSize();

	// Confirm.	
	MessageBoxOK("Load Layer", "The LAYER was loaded successfully.\n\nLayer #" @ $activeTileLayer @ " - Quantity:" @ %tileCount SPC " / Size:" @ %tileSize, "");
}


//-----------------------------------------------------------------------------
// Append Load Layer.
//-----------------------------------------------------------------------------
function appendLoadLayer( %layerFile )
{
	// Clear Undo/Redo.		
	clearUndoRedo();
	
	// Check Tile-Map.
	if ( !isObject($editorTileMap) )
	{
		// Problem, so warn.
		MessageBoxOK("Append Layer", "There was a problem loading the layer!", "");
		return;
	}
	
	// Store Active Layer Temporarily.
	%tempActiveLayer = $activeTileLayer;

	// Create Dummy Layer.
	createTileLayers( 1, 1, 1, 1, 1 );
	
	// Get Active Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );

	// Check Layer.	
	if ( !isObject(%tileLayer) )
	{
		// Problem, so warn.
		MessageBoxOK("Append Layer", "There was a problem accessing the current layer!", "");
		// Finish Here.
		return;
	}
	
	// Load Tile Layer.
	if ( !%tileLayer.loadTileLayer( %layerFile ) )
	{
		// Problem, so warn.
		MessageBoxOK("Append Layer", "The load failed!", "");
		
		// Remove the Tile Layer.
		$editorTileMap.deleteTileLayer( $activeTileLayer );

		// Read Tile Layer Selection.
		readTileLayerSelection();
		
		// Select Last Layer.	
		selectTileLayer( %tempActiveLayer );
		
		// Finish Here.
		return;
	}		

	// Read Tile Layer Selection.
	readTileLayerSelection();
	
	// Select Last Layer.	
	selectTileLayer( $editorTileMap.getTileLayerCount() - 1 );

	// Get Underlying Tile-Layer Dimensions.
	%tileCount = %tileLayer.getTileCount();
	%tileSize = %tileLayer.getTileSize();
	
	// Confirm.	
	MessageBoxOK("Append Layer", "The LAYER was appended successfully.\n\nLayer #" @ $activeTileLayer @ " - Quantity:" @ %tileCount SPC " / Size:" @ %tileSize, "");
}


//-----------------------------------------------------------------------------
// Save Map.
//-----------------------------------------------------------------------------
function saveMap( %mapFile )
{	
	// Check Tile-Map.
	if ( !isObject($editorTileMap) )
	{
		// Problem, so warn.
		MessageBoxOK("Save Map", "There was a problem saving the map!", "");
		return;
	}
	
	// Save Tile Map.
	if ( !$editorTileMap.saveTileMap( %mapFile ) )
	{
		// Problem, so warn.
		MessageBoxOK("Save Map", "The save failed!", "");
		return;
	}
	
	// Confirm.	
	MessageBoxOK("Save Map", "The MAP was saved successfully.", "");
}


//-----------------------------------------------------------------------------
// Save Layer.
//-----------------------------------------------------------------------------
function saveTileLayer( %layerFile )
{
	// Check Tile-Map.
	if ( !isObject($editorTileMap) )
	{
		// Problem, so warn.
		MessageBoxOK("Save Layer", "There was a problem saving the current layer!", "");
		return;
	}

	// Fetch Tile Layer Count.
	%layerCount = $editorTileMap.getTileLayerCount();
	
	// Sanity!
	if ( %layerCount == 0 )
	{
		// Problem, so warn.
		MessageBoxOK("Save Layer", "There are no layers to save!", "");
		// Finish Here.
		return;
	}
	
	// Get Active Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );
	
	// Check Layer.
	if ( !isObject(%tileLayer) )
	{
		// Problem, so warn.
		MessageBoxOK("Save Layer", "There was a problem accessing the current layer!", "");
		// Finish Here.
		return;
	}
	
	// Save Tile Layer.
	if ( !%tileLayer.saveTileLayer( %layerFile ) )
	{
		// Problem, so warn.
		MessageBoxOK("Save Layer", "The save failed!", "");
		return;
	}	

	// Confirm.	
	MessageBoxOK("Save Layer", "The LAYER was saved successfully.", "");
	
	LevelBuilder::reloadTileLayers(%layerFile);

 // refresh the tile map object library in the Level Builder
 GuiFormManager::sendContentMessage($LBCTileMap, "", "refresh");
}


//-----------------------------------------------------------------------------
// Initialise Tile Map.
//-----------------------------------------------------------------------------
function initialiseTileMap( %tileCountX, %tileCountY, %tileSizeX, %tileSizeY, %tileLayers )
{
	// Destroy Tile-Map (if it exists).
	if ( isObject($editorTileMap) )
		$editorTileMap.safeDelete();

	// Create new Tile-Map.
	$editorTileMap = new t2dTileMap() { scenegraph = tileEditorSceneGraph2D; };
	
	// Create Tile Layers.
	createTileLayers( %tileCountX, %tileCountY, %tileSizeX, %tileSizeY, %tileLayers );

	// Move to Top/Left Tile-Layer.
	tileLayerTopLeft();
}


// --------------------------------------------------------------------
// Create Tile Layers.
// --------------------------------------------------------------------
function createTileLayers( %tileCountX, %tileCountY, %tileSizeX, %tileSizeY, %tileLayers )
{
	// Clear Undo/Redo.		
	clearUndoRedo();
	
	// Calculate Layer Size.
	%layerSize = (%tileCountX*%tileSizeX) SPC (%tileCountY*%tileSizeY);
	// Calculate Layer Top/Left Origin Offset.
	%layerOffset = t2dVectorScale( %layerSize, 0.5 );
	
	// Create Tile Layers.
	for ( %n = 0; %n < %tileLayers; %n++ )
	{
		// Create Default Tile-Layer.
		%layer = $editorTileMap.createTileLayer( %tileCountX SPC %tileCountY SPC %tileSizeX SPC %tileSizeY );
		
		// Wrap Off.
		%layer.setWrap( false, false );

		// Disable Tile Layer.
		%layer.setEnabled( false );
	
		// Set Position/Size/Layer.	
		%layer.setPosition( %layerOffset );
		%layer.setSize( %layerSize );
		%layer.setLayer( 30 );
	}
	
	// Read Tile Layer Selection.
	readTileLayerSelection();
	
	// Select First Tile Layer we added.
	selectTileLayer( $editorTileMap.getTileLayerCount() - %tileLayers );
}


// --------------------------------------------------------------------
// Clear Tile Layer.
// --------------------------------------------------------------------
function clearTileLayer()
{
	// Store Undo.
	storeUndo();
	
	// Do we have a Tile-Map.
	if ( isObject($editorTileMap) )
	{
		// Fetch Tile Layer Count.
		%layerCount = $editorTileMap.getTileLayerCount();
		
		// Sanity!
		if ( %layerCount == 0 )
		{
			// Yes, so warn.
			MessageBoxOK("Clear Layer Error", "There are no layers to clear!", "");
			// Finish Here.
			return;
		}
		
		/// Get Active Tile Layer.
		%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );
		
		if ( !isObject(%tileLayer) )
		{
			// Yes, so warn.
			MessageBoxOK("Clear Layer Error", "There was a problem accessing the current layer!", "");
			// Finish Here.
			return;
		}
		
		// Clear Tile Layer.
		%tileLayer.clearLayer();
		// Inform.
		MessageBoxOK("Clear Layer", "The current tile layer has been cleared.", "");
	}
}


// --------------------------------------------------------------------
// Remove Tile Layer.
// --------------------------------------------------------------------
function removeTileLayer()
{
	// Clear Undo/Redo.		
	clearUndoRedo();
	
	// Do we have a Tile-Map.
	if ( isObject($editorTileMap) )
	{
		// Fetch Tile Layer Count.
		%layerCount = $editorTileMap.getTileLayerCount();
		
		// Sanity!
		if ( %layerCount == 0 )
		{
			// Yes, so warn.
			MessageBoxOK("Remove Layer Error", "There are no layers to remove!", "");
			// Finish Here.
			return;
		}
		
		/// Get Active Tile Layer.
		%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );
		
		if ( !isObject(%tileLayer) )
		{
			// Yes, so warn.
			MessageBoxOK("Remove Layer Error", "There was a problem accessing the current layer!", "");
			// Finish Here.
			return;
		}
		
		// Remove the Tile Layer.
		$editorTileMap.deleteTileLayer( $activeTileLayer );
		
		// Read Tile Layer Selection.
		readTileLayerSelection();
		
		// Calculate new current layer. 
		if ( $activeTileLayer < $editorTileMap.getTileLayerCount() )
			%newLayer = $activeTileLayer;
		else
			%newLayer = $activeTileLayer-1;
		
		// Select First Tile Layer we added.
		selectTileLayer( %newLayer );
		
		// Inform.
		MessageBoxOK("Remove Layer", "The current tile layer has been removed.", "");
	}
}


// --------------------------------------------------------------------
// Resize Tiles.
// --------------------------------------------------------------------
function resizeTiles()
{
	// Do we have a Tile-Map.
	if ( isObject($editorTileMap) )
	{
		// Fetch Tile Layer Count.
		%layerCount = $editorTileMap.getTileLayerCount();
		
		// Sanity!
		if ( %layerCount == 0 )
		{
			// Yes, so warn.
			MessageBoxOK("Resize Tiles Error", "There are no layers to resize!", "");
			// Finish Here.
			return;
		}
		
		/// Get Active Tile Layer.
		%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );
		
		// Valid Tile LAyer?
		if ( !isObject(%tileLayer) )
		{
			// No, so warn.
			MessageBoxOK("Resize Tiles Error", "There was a problem accessing the current layer!", "");
			// Finish Here.
			return;
		}

		// Get Tile Size.
		%tileSize = %tileLayer.getTileSize();
		
		// Setup Existing Counts.
		txtResizeTilesSizeX.setValue( getWord(%tileSize, 0) );
		txtResizeTilesSizeY.setValue( getWord(%tileSize, 1) );
		
		// Show Resize Tiles Dialog.
		Canvas.pushDialog( resizeTilesDlg );
	}
}


// --------------------------------------------------------------------
// Action Tiles Resize.
// --------------------------------------------------------------------
function actionResizeTiles()
{
	// Clear Undo/Redo.		
	clearUndoRedo();
	
	// Fetch New Tile Sizes.
	%tileSizeX = txtResizeTilesSizeX.getValue();
	%tileSizeY = txtResizeTilesSizeY.getValue();
	
	// Are the tiles illegally sized?
	if ( %tileSizeX == 0 || %tileSizeY == 0 )
	{
		// Yes, so warn.
		MessageBoxOK("Resize Tiles Error", "The tiles must be greater than zero in size in each axis!", "");
		// Finish Here.
		return;
	}
	
	/// Get Active Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );
	
	// Resize Tile Layer.
	%tileLayer.setTileSize( %tileSizeX SPC %tileSizeY );
	
	// Read Tile Layer Selection.
	readTileLayerSelection();
	
	// Select First Tile Layer we added.
	selectTileLayer( $activeTileLayer );
	
	// Confirm.
	MessageBoxOK("Resize Tiles", "The current layers' tiles have been resized.", "");
}


// --------------------------------------------------------------------
// Resize Tile Layer.
// --------------------------------------------------------------------
function resizeTileLayer()
{
	// Do we have a Tile-Map.
	if ( isObject($editorTileMap) )
	{
		// Fetch Tile Layer Count.
		%layerCount = $editorTileMap.getTileLayerCount();
		
		// Sanity!
		if ( %layerCount == 0 )
		{
			// Yes, so warn.
			MessageBoxOK("Resize Layer Error", "There are no layers to resize!", "");
			// Finish Here.
			return;
		}
		
		/// Get Active Tile Layer.
		%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );
		
		// Valid Tile Layer?
		if ( !isObject(%tileLayer) )
		{
			// No, so warn.
			MessageBoxOK("Resize Layer Error", "There was a problem accessing the current layer!", "");
			// Finish Here.
			return;
		}

		// Get Tile Count.
		%tileCount = %tileLayer.getTileCount();
		
		// Setup Existing Counts.
		txtResizeLayerCountX.setValue( getWord(%tileCount, 0) );
		txtResizeLayerCountY.setValue( getWord(%tileCount, 1) );
		
		// Show Resize Layer Dialog.
		Canvas.pushDialog( resizeLayerDlg );
	}
}


// --------------------------------------------------------------------
// Action Tile Layer Resize.
// --------------------------------------------------------------------
function actionResizeTileLayer()
{
	// Clear Undo/Redo.		
	clearUndoRedo();
	
	// Fetch New Tile Counts.
	%tileCountX = txtResizeLayerCountX.getValue();
	%tileCountY = txtResizeLayerCountY.getValue();
	
	// Are the tiles illegally sized?
	if ( %tileCountX == 0 || %tileCountY == 0 )
	{
		// Yes, so warn.
		MessageBoxOK("Resize Layer Error", "There must be at least a single tile in each axis!", "");
		// Finish Here.
		return;
	}
	
	/// Get Active Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );
	
	// Resize Tile Layer.
	%tileLayer.resizeLayer( %tileCountX SPC %tileCountY );
	
	// Read Tile Layer Selection.
	readTileLayerSelection();
	
	// Select First Tile Layer we added.
	selectTileLayer( $activeTileLayer );
	
	// Confirm.
	MessageBoxOK("Resize Layer", "The current tile layer has been resized.", "");
}


// --------------------------------------------------------------------
// Laye Blend.
// --------------------------------------------------------------------
function layerBlend()
{
	// Do we have a Tile-Map.
	if ( isObject($editorTileMap) )
	{
		// Fetch Tile Layer Count.
		%layerCount = $editorTileMap.getTileLayerCount();
		
		// Sanity!
		if ( %layerCount == 0 )
		{
			// Yes, so warn.
			MessageBoxOK("Blend Layer Error", "There are no layers to blend!", "");
			// Finish Here.
			return;
		}
		
		// Read Layer Blend Values.
		readTileEditorBlendValues();
		
		// Show Layer Blend Dialog.
		Canvas.pushDialog(layerBlendDlg);
	}
}


// --------------------------------------------------------------------
// Create Cursor.
// --------------------------------------------------------------------
function createCursor()
{
	new t2dStaticSprite(tileEditorCursor) { scenegraph = tileEditorSceneGraph2D; };
	tileEditorCursor.setImageMap( tileEditorCursorImageMap );
	tileEditorCursor.setLayer( 0 );
	tileEditorCursor.setPosition("0 0");
	tileEditorCursor.setSize( 1 );
	tileEditorCursor.setVisible(false);
	tileEditorCursor.setBlendColour("0 0 1 0.5");
}


// --------------------------------------------------------------------
// Destroy Cursor.
// --------------------------------------------------------------------
function destroyCursor()
{
	// Delete Cursor.
	if ( isObject(tileEditorCursor) )
		tileEditorCursor.delete();
}


// --------------------------------------------------------------------
// Set Cursor Position.
// --------------------------------------------------------------------
function setCursorPos( %position, %size )
{
	// Calculate Position Offset.
	%offset = t2dVectorScale( %size, 0.5 );
	
	// Set Position / Size.
	tileEditorCursor.setPosition( t2dVectorAdd( %position, %offset ) );
	tileEditorCursor.setSize( %size );
}


// --------------------------------------------------------------------
// Launch New Tile Dialog.
// --------------------------------------------------------------------
function launchNewTileDialog()
{
	// Set Dialog Values.
	txtTileCountX.setValue( $tileEditor2D::defaultTileCountX );
	txtTileCountY.setValue( $tileEditor2D::defaultTileCountY );
	txtTileSizeX.setValue( $tileEditor2D::defaultTileSizeX );
	txtTileSizeY.setValue( $tileEditor2D::defaultTileSizeY );
	txtTileLayers.setValue( $tileEditor2D::defaultTileLayers );
	
	// Launch Dialog.
	Canvas.pushDialog(newTileDlg);
}


// --------------------------------------------------------------------
// New Tile.
// --------------------------------------------------------------------
function actionNewTileMapDialog()
{
	// Clear Undo/Redo.		
	clearUndoRedo();
	
	// Get Dialog Values.
	%tileCountX	= txtTileCountX.getValue();
	%tileCountY	= txtTileCountY.getValue();
	%tileSizeX	= txtTileSizeX.getValue();
	%tileSizeY	= txtTileSizeY.getValue();
	%tileLayers	= txtTileLayers.getValue();
	
	// Initialise Tile Map.
	initialiseTileMap( %tileCountX, %tileCountY, %tileSizeX, %tileSizeY, %tileLayers );
}


// --------------------------------------------------------------------
// Launch New Layer Dialog.
// --------------------------------------------------------------------
function launchNewLayerDialog()
{
	// Fetch First Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer(0);
      
   // Valid Tile Layer?
   if ( !isObject(%tileLayer) )
   {
      // No, so use defaults.
      txtLayerCountX.setValue( $tileEditor2D::defaultTileCountX );
      txtLayerCountY.setValue( $tileEditor2D::defaultTileCountY );
      txtLayerSizeX.setValue( $tileEditor2D::defaultTileSizeX );
      txtLayerSizeY.setValue( $tileEditor2D::defaultTileSizeY );
      txtLayerLayers.setValue( $tileEditor2D::defaultTileLayers );
   }
   else
   {
      // Yes, so default to first tile layer size.
      %tileCount = %tileLayer.getTileCount();
      %tileSize = %tileLayer.getTileSize();
	  
      txtLayerCountX.setValue( getWord(%tileCount, 0) );
      txtLayerCountY.setValue( getWord(%tileCount, 1) );
      txtLayerSizeX.setValue( getWord(%tileSize, 0) );
      txtLayerSizeY.setValue( getWord(%tileSize, 1) );
      txtLayerLayers.setValue( $tileEditor2D::defaultTileLayers );
   }
	
	// Launch Dialog.
	Canvas.pushDialog(newLayerDlg);
}


// --------------------------------------------------------------------
// New Layer(s).
// --------------------------------------------------------------------
function actionNewLayerMapDialog()
{
	// Clear Undo/Redo.		
	clearUndoRedo();
	
	// Get Dialog Values.
	%tileCountX	= txtLayerCountX.getValue();
	%tileCountY	= txtLayerCountY.getValue();
	%tileSizeX	= txtLayerSizeX.getValue();
	%tileSizeY	= txtLayerSizeY.getValue();
	%tileLayers	= txtLayerLayers.getValue();

	// Create Tile Layers.	
	createTileLayers( %tileCountX, %tileCountY, %tileSizeX, %tileSizeY, %tileLayers );
}

// --------------------------------------------------------------------
// Read Tile-Layer Selection.
// --------------------------------------------------------------------
function readTileLayerSelection()
{
	// Clear Menu Items.
	tileScreenMenu.clearMenuItems("View");
	// Setup View Menu.
	tileScreenMenu.addMenuItem("View", "Zoom x0.1", 0);
	tileScreenMenu.addMenuItem("View", "Zoom x0.5", 1);
	tileScreenMenu.addMenuItem("View", "Zoom x1.0", 2);
	tileScreenMenu.addMenuItem("View", "Zoom x1.5", 3);
	tileScreenMenu.addMenuItem("View", "Zoom x2.0", 4);
	tileScreenMenu.addMenuItem("View", "-", 0);
	tileScreenMenu.addMenuItem("View", "Adjust...", 5);
	tileScreenMenu.addMenuItem("View", "Home!", 6);
	tileScreenMenu.addMenuItem("View", "Border", 7);
	tileScreenMenu.addMenuItem("View", "Gridlines", 8);
	tileScreenMenu.addMenuItem("View", "Script Icons", 9);
	tileScreenMenu.addMenuItem("View", "Custom-Data Icons", 10);
	tileScreenMenu.addMenuItem("View", "-", 0);
	tileScreenMenu.addMenuItem("View", "Image Library", 11);
	
	// View Menu Commands.
	tileScreenMenu.scriptCommand["View", 0] = "$tileEditor2D::viewZoom = 0.1; tileEditorSetView();";
	tileScreenMenu.scriptCommand["View", 1] = "$tileEditor2D::viewZoom = 0.5; tileEditorSetView();";
	tileScreenMenu.scriptCommand["View", 2] = "$tileEditor2D::viewZoom = 1.0; tileEditorSetView();";
	tileScreenMenu.scriptCommand["View", 3] = "$tileEditor2D::viewZoom = 1.5; tileEditorSetView();";
	tileScreenMenu.scriptCommand["View", 4] = "$tileEditor2D::viewZoom = 2.0; tileEditorSetView();";
	tileScreenMenu.scriptCommand["View", 5] = "tileEditorEditView();";
	tileScreenMenu.scriptCommand["View", 6] = "tileLayerTopLeft();";
	tileScreenMenu.scriptCommand["View", 7] = "toggleTileInfo();";
	tileScreenMenu.scriptCommand["View", 8] = "toggleGridLines();";
	tileScreenMenu.scriptCommand["View", 9] = "toggleScriptIcons();";
	tileScreenMenu.scriptCommand["View", 10] = "toggleCustomDataIcons();";
	tileScreenMenu.scriptCommand["View", 11] = "toggleImageLibraryStatus();";
	
	// Finish if no tile-map.
	if ( !isObject($editorTileMap) )
		return;

	// Add Addition Static Menu.
	tileScreenMenu.addMenuItem("View", "-", 0);
	tileScreenMenu.addMenuItem("View", "All Layers", 12);
	tileScreenMenu.addMenuItem("View", "Up to Current Layer", 13);
	tileScreenMenu.addMenuItem("View", "-", 0);
	
	// View-All Layers Command.
	tileScreenMenu.scriptCommand["View", 12] = "toggleViewAllLayers();";
	tileScreenMenu.scriptCommand["View", 13] = "toggleUptoCurrentLayer();";
	
	// Now Add Dynamic Layers.
	for ( %n = 0; %n < $editorTileMap.getTileLayerCount(); %n++ )
	{
		// Add Menu Item.
		tileScreenMenu.addMenuItem("View", "Layer #" @ %n, 14+%n);
		// Add Menu Item Command.
		tileScreenMenu.scriptCommand["View", 14+%n] = "selectTileLayer(" @ %n @ ");";
	}

	// Update View Layers Check.
	updateViewLayersCheck();
	
	// Set Menu-Item Checked-Status.
	tileScreenMenu.setMenuItemChecked("View", "Border", $tileInfoButtonFlag);		
	// Set Menu-Item Checked-Status.
	tileScreenMenu.setMenuItemChecked("View", "Gridlines", $tileEditor2D::viewGridlines);		
	// Set Menu-Item Checked-Status.
	tileScreenMenu.setMenuItemChecked("View", "Script Icons", $tileEditor2D::viewScriptIcons);
	// Set Menu-Item Checked-Status.
	tileScreenMenu.setMenuItemChecked("View", "Custom-Data Icons", $tileEditor2D::viewCustomIcons);
	// Set Menu-Item Checked-Status.
	tileScreenMenu.setMenuItemChecked("View", "Image Library", $tileEditor2D::viewImageLibrary);
}


// --------------------------------------------------------------------
// Update View Layers Check.
// --------------------------------------------------------------------
function updateViewLayersCheck()
{
	// Set Checks.	
	tileScreenMenu.setMenuItemChecked("View", "All Layers", $viewAllLayers);
	tileScreenMenu.setMenuItemChecked("View", "Up to Current Layer", $viewUptoCurrentLayer);
}


// --------------------------------------------------------------------
// Toggle View All Layers.
// --------------------------------------------------------------------
function toggleViewAllLayers()
{
	// Toggle View-All Layers.
	$viewAllLayers = !$viewAllLayers;

	// Turn-off "upto current layer" option if "view-all" is on.
	if ( $viewAllLayers )
		$viewUptoCurrentLayer = false;
	
	// Update View Layers Check.
	updateViewLayersCheck();

	// Reselect Current Layer (processes layer view status).	
	selectTileLayer( $activeTileLayer );
}


// --------------------------------------------------------------------
// Toggle Upto Current Layer.
// --------------------------------------------------------------------
function toggleUptoCurrentLayer()
{
	// Toggle Upto Current Layer.
	$viewUptoCurrentLayer = !$viewUptoCurrentLayer;
	
	// Turn-off "view all" if "upto current layer" option is on.
	if ( $viewUptoCurrentLayer )
		$viewAllLayers = false;
	
	// Update View-All Layers Check.
	updateViewLayersCheck();

	// Reselect Current Layer (processes layer view status).	
	selectTileLayer( $activeTileLayer );
}


// --------------------------------------------------------------------
// Select Tile Layer.
// --------------------------------------------------------------------
function selectTileLayer( %tileIndex )
{
	// Select Active Tile Layer.
	$activeTileLayer = %tileIndex;
	
	// No tile layers.
	if ( $activeTileLayer == -1 )
		return;
	
	// Fetch Tile Layer Count.
	%layerCount = $editorTileMap.getTileLayerCount();
	
	// Sanity!
	if ( %layerCount == 0 )
	{
		// Reset Layer.
		$activeTileLayer = -1;
		return;
	}
	
	// Check our layer is valid!
	if ( %tileIndex >= %layerCount )
	{
		// Warn.
		error( "TileEditor: Invalid Tile-Layer" SPC %tileIndex @ ", only" SPC %layerCount SPC "layer(s) available!" );
		// Set to last layer.
		$activeTileLayer = %layerCount - 1;
	}
	
	// Enable only selected Tile Layer, disable the rest.
	// NOTE:-	We deal with the view-layer options here as well.
	for ( %n = 0; %n < %layerCount; %n++ )
	{
		// Fetch Tile Layer.
		%tileLayer = $editorTileMap.getTileLayer( %n );
		// Enable/Disable Layer.
		%tileLayer.setEnabled( $viewAllLayers || ($viewUptoCurrentLayer && %n <= $activeTileLayer)  || %n == $activeTileLayer );
		// Set Layer.
		%tileLayer.setLayer( 30 );
		// Set Grid Status.
		%tileLayer.setGridActive( $tileEditor2D::viewGridlines && (%n == $activeTileLayer) );
		// Set Tile-Icon ImageMap.
		%tileLayer.setTileIcons( tileEditorTileIconsImageMap );
		// Set Custom-Icon Status.
		%tileLayer.setCustomIconActive( $tileEditor2D::viewCustomIcons && (%n == $activeTileLayer) );
		// Set Script-Icon Status.
		%tileLayer.setScriptIconActive( $tileEditor2D::viewScriptIcons && (%n == $activeTileLayer) );
        // Set Cursor-Icon Status.
		%tileLayer.setCursorIconActive( %n == $activeTileLayer );
	}
			
	// Hide Cursor.
	tileEditorCursor.setVisible( false );
	
	// Set Menu-Item Checked-Status if we've got a menu).
	for ( %n = 0; %n < %layerCount; %n++ )
		tileScreenMenu.setMenuItemChecked("View", "Layer #" @ %n, %n == $activeTileLayer);

	// Set Layer Status.
	setLayerStatusText( "Tile-Layer" SPC $activeTileLayer SPC "(" @ %layerCount SPC "total)" );
	
	/// Get Active Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );
	// Get Underlying Tile-Layer Dimensions.
	%tileCount = %tileLayer.getTileCount();
	%tileSize = %tileLayer.getTileSize();
	// Get Component Dimensions.
	%tileCountX = getWord(%tileCount, 0);
	%tileCountY = getWord(%tileCount, 1);
	%tileSizeX = getWord(%tileSize, 0);
	%tileSizeY = getWord(%tileSize, 1);
	
	// Calculate Layer Size.
	%layerSize = (%tileCountX*%tileSizeX) SPC (%tileCountY*%tileSizeY);
	// Calculate Layer Top/Left Origin Offset.
	%layerOffset = t2dVectorScale( %layerSize, 0.5 );
	// Update Layer Position / Size.
	%tileLayer.setPosition( %layerOffset );
	%tileLayer.setSize( %layerSize );
}


// --------------------------------------------------------------------
// Toggle Tile Info.
// --------------------------------------------------------------------
function toggleTileInfo()
{
	// Toggle Tile Border.
	if ( $tileInfoButtonFlag )
		tileEditorSceneGraph2D.setDebugOff( 1, 5 );
	else
		tileEditorSceneGraph2D.setDebugOn( 1, 5 );

	// Toggle Tile Info Status.
	$tileInfoButtonFlag = !$tileInfoButtonFlag;
		
	// Set Menu-Item Checked-Status.
	tileScreenMenu.setMenuItemChecked("View", "Border", $tileInfoButtonFlag);		
}


// --------------------------------------------------------------------
// Toggle Gridlines.
// --------------------------------------------------------------------
function toggleGridLines()
{
	// Toggle Gridlines.
	$tileEditor2D::viewGridlines = !$tileEditor2D::viewGridlines;

	// Set Menu-Item Checked-Status.
	tileScreenMenu.setMenuItemChecked("View", "Gridlines", $tileEditor2D::viewGridlines);		
	
	// Select First Tile Layer we added.
	selectTileLayer( $activeTileLayer );
}


// --------------------------------------------------------------------
// Toggle Custom-Data Icons.
// --------------------------------------------------------------------
function toggleCustomDataIcons()
{
	// Toggle Custom-Data Icons.
	$tileEditor2D::viewCustomIcons = !$tileEditor2D::viewCustomIcons;

	// Set Menu-Item Checked-Status.
	tileScreenMenu.setMenuItemChecked("View", "Custom-Data Icons", $tileEditor2D::viewCustomIcons);
	
	// Select First Tile Layer we added.
	selectTileLayer( $activeTileLayer );
}


// --------------------------------------------------------------------
// Toggle Script Icons.
// --------------------------------------------------------------------
function toggleScriptIcons()
{
	// Toggle Script Icons.
	$tileEditor2D::viewScriptIcons = !$tileEditor2D::viewScriptIcons;

	// Set Menu-Item Checked-Status.
	tileScreenMenu.setMenuItemChecked("View", "Script Icons", $tileEditor2D::viewScriptIcons);

	// Select First Tile Layer we added.
	selectTileLayer( $activeTileLayer );
}


// --------------------------------------------------------------------
// Toggle Image Library Status.
// --------------------------------------------------------------------
function toggleImageLibraryStatus()
{
    // Set Image Library Statys.
    setImageLibraryStatus( !$tileEditor2D::viewImageLibrary );
}


// --------------------------------------------------------------------
// Draw Brush.
// --------------------------------------------------------------------
function drawBrush( %brush, %worldPosition, %maxBrushSize )
{
	// Finish if no tile-layers.
	if ( $editorTileMap.getTileLayerCount() == 0 )
		return;
	
	// Is Brush Inactive?	
	if ( !$brush[$currentBrush].active )
	{
		// Yes, so warn.
		MessageBoxOK("Brush Error", "The current brush is not defined!", "");
		// Finish Here.
		return;
	}
	
	// Get Current Tile Layer Object.
	%layerObj = $editorTileMap.getTileLayer( $activeTileLayer );
	
	// Check Object is valid.
	if ( %layerObj )
	{
		// Is the brush active?
		if ( %brush.active )
		{
			// Yes, so find logical tile position.
			%tilePick = %layerObj.pickTile( %worldPosition );
			
			// Got a tile?
			if ( %tilePick !$= "" )
			{
				// Yes, so fetch tile-position components.
				%tileX = getWord( %tilePick, 0 );
				%tileY = getWord( %tilePick, 1 );
				
				// Yes, so get Tiles Count.
				%tileCount = %layerObj.getTileCount();
				// Fetch Tile Count Components.
				%tileCountX = getWord( %tileCount, 0 );
				%tileCountY = getWord( %tileCount, 1 );
				
				// Calculate allowed width/height we can draw.
				%drawWidth	= ( (%tileX + %brush.width) < %tileCountX ) ? %brush.width : %tileCountX - %tileX;
				%drawHeight	= ( (%tileY + %brush.height) < %tileCountY ) ? %brush.height : %tileCountY - %tileY;
                
                // Do we have a maximum draw-draw size?
                if ( %maxBrushSize > 0 )
                {
                    // Yes, so limit brush dimensions.
                    if ( %drawWidth > %maxBrushSize ) %drawWidth = %maxBrushSize;
                    if ( %drawHeight > %maxBrushSize ) %drawHeight = %maxBrushSize;
                }
				
				// Draw brush-defs.
				for ( %x = 0; %x < %drawWidth; %x++ )
				{
					for ( %y = 0; %y < %drawHeight; %y++ )
					{
						// Calculate Tile Position.
						%tilePos = (%x + %tileX) SPC (%y + %tileY);
						
						// Fetch Brush Tile-Def.
						%tileDef = %brush.tileDef[%x,%y];
						
						// Is Tile-Def Active?
						if ( %tileDef.active )
						{
							// Yes, so draw Tile-Def.
							switch$( %tileDef.type )
							{
								// Static-Tile (Image-Map).
								case "static":
									%layerObj.setStaticTile( %tilePos, %tileDef.name, %tileDef.frame );
								
								// Animated (Animation).
								case "animated":
									%layerObj.setAnimatedTile( %tilePos, %tileDef.name );
								
							}
							
							// Set Tile Script.
							%layerObj.setTileScript( %tilePos, %tileDef.script );
							// Set Tile Custom-Data.
							%layerObj.setTileCustomData( %tilePos, %tileDef.customData );
							// Set Tile Flip
							%layerObj.setTileFlip( %tilePos, %tileDef.flipX, %tileDef.flipY );
							
							// Set Tile Collision.
							// NOTE:- We don't draw collisions if "collision suppress is on".
							%layerObj.setTileCollisionActive( %tilePos, %tileDef.collisionActive && !$collisionDrawSuppressStatus );
							// Set Tile Collision Scale.
							%layerObj.setTileCollisionScale( %tilePos, %tileDef.collisionScale );
							// Collision Active?
							if ( %tileDef.collisionActive )
								%layerObj.setTileCollisionPolyCustom( %tilePos, %tileDef.collisionPoly, %tileDef.collisionDef );
								
						}
					}
				}
			}
		}
	}
}


// --------------------------------------------------------------------
// Eraser Brush.
// --------------------------------------------------------------------
function eraserBrush( %brush, %worldPosition )
{
	// Finish if no tile-layers.
	if ( $editorTileMap.getTileLayerCount() == 0 )
		return;
	
	// Is Brush Inactive?	
	if ( !$brush[$currentBrush].active )
	{
		// Yes, so warn.
         MessageBoxOK("Brush Error", "The current brush is not defined!", "");
		 // Finish Here.
		 return;
	}
	
	// Get Current Tile Layer Object.
	%layerObj = $editorTileMap.getTileLayer( $activeTileLayer );
	
	// Check Object is valid.
	if ( %layerObj )
	{
		// Is the brush active?
		if ( %brush.active )
		{
			// Yes, so find logical tile position.
			%tilePick = %layerObj.pickTile( %worldPosition );

			// Got a tile?
			if ( %tilePick !$= "" )
			{
				// Yes, so fetch tile-position components.
				%tileX = getWord( %tilePick, 0 );
				%tileY = getWord( %tilePick, 1 );
				
				// Yes, so get Tiles Count.
				%tileCount = %layerObj.getTileCount();
				// Fetch Tile Count Components.
				%tileCountX = getWord( %tileCount, 0 );
				%tileCountY = getWord( %tileCount, 1 );
				
				// Calculate allowed width/height we can draw.
				%drawWidth	= ( (%tileX + %brush.width) < %tileCountX ) ? %brush.width : %tileCountX - %tileX;
				%drawHeight	= ( (%tileY + %brush.height) < %tileCountY ) ? %brush.height : %tileCountY - %tileY;
				
				// Yes, so draw brush-defs.
				for ( %x = 0; %x < %drawWidth; %x++ )
				{
					for ( %y = 0; %y < %drawHeight; %y++ )
					{
						// Calculate Tile Position.
						%tilePos = (%x + %tileX) SPC (%y + %tileY);

						// Fetch Brush Tile-Def.
						%tileDef = %brush.tileDef[%x,%y];
						
						// Is Tile-Def Active?
						if ( %tileDef.active )
						{
							// Yes, so clear Tile.						
							%layerObj.clearTile( %tilePos );
						}
					}
				}
			}
		}
	}
}


//-----------------------------------------------------------------------------
// Draw Brush Area.
//-----------------------------------------------------------------------------
function drawBrushArea( %startPosition, %endPosition )
{
	// Is Brush Inactive?	
	if ( !$brush[$currentBrush].active )
	{
		// Yes, so warn.
         MessageBoxOK("Brush Error", "The current brush is not defined!", "");
		 // Finish Here.
		 return;
	}
	
	// Fetch Tile Size.
	%tileSize = $editorTileMap.getTileLayer( $activeTileLayer ).getTileSize();
	// Fetch Component Steps.
	%tileStepX = getWord( %tileSize, 0 ) * $brush[$currentBrush].width;
	%tileStepY = getWord( %tileSize, 1 ) * $brush[$currentBrush].height;
	
	// Fetch Component Start/End of Positions.
	%startX = getWord( %startPosition, 0 );
	%startY = getWord( %startPosition, 1 );
	%endX = getWord( %endPosition, 0 );
	%endY = getWord( %endPosition, 1 );
	
	// Draw current brush across area at tile-steps.
	for ( %x = %startX; %x <= %endX; %x += %tileStepX )
		for ( %y = %startY; %y <= %endY; %y += %tileStepY )
			// Draw Current Brush.
			drawBrush( $brush[$currentBrush], %x SPC %y, 0 );
}

// --------------------------------------------------------------------
// Create Brush Def.
// --------------------------------------------------------------------
function createBrushDef( %tileLayer, %tilePos )
{
    // Create Brush Def.
    %brushDef = new ScriptObject() { class = tileMapBrushDef; };
    
    // Fetch Tile Type.
    %tileType = %tileLayer.getTileType( %tilePos );
    
    // Set Brush Def Tile-Type.
    %brushDef.tileType = %tileType;
                    
    // Does Tile Exist?
    if ( %tileType !$= "" )
    {
        // Setup Tile-Def Content.
        switch$( getWord(%tileType, 0) )
        {
            // Static-Tile (Image-Map).
            case "static":
                %brushDef.type		= "static";
                %brushDef.name		= getWord(%tileType, 1);
                %brushDef.frame		= getWord(%tileType, 2); 
            
            // Animated-Tile (Animation).
            case "animated":
                %brushDef.type		= "animated"; 
                %brushDef.name		= getWord(%tileType, 1);
                %brushDef.frame		= -1;
                
            // Active-Tile (Active).
            case "active":
                %brushDef.type		= "active"; 
                %brushDef.name		= getWord(%tileType, 1);
                %brushDef.frame		= -1;
        }
    } 
    else
    {
        // Empty.
        %brushDef.type		= ""; 
        %brushDef.name		= "";
        %brushDef.frame		= -1;
    }
    
    // Return Brush Def.
    return %brushDef;
}


// --------------------------------------------------------------------
// Is Brush Def Here?
// --------------------------------------------------------------------
function isBrushDefHere( %searchTileDef, %tileLayer, %tilePos )
{
    // Fetch Tile Type.
    %tileType = %tileLayer.getTileType( %tilePos );
    
    // Does Tile Exist?
    if ( %searchTileDef.tileType !$= "" )
    {  
        // Setup Tile-Def Content.
        switch$( getWord(%tileType, 0) )
        {
            // Static-Tile (Image-Map).
            case "static":
                // Check Type.
                if (    %searchTileDef.type    $= "static" &&
                        %searchTileDef.name    $= getWord(%tileType, 1) &&
                        %searchTileDef.frame   $= getWord(%tileType, 2) )
                {        
                    // Yes, type is the same!
                    return true;
                }
            
            // Animated-Tile (Animation).
            case "animated":
                // Check Type.
                if (    %searchTileDef.type    $= "animated" &&
                        %searchTileDef.name    $= getWord(%tileType, 1) )
                {
                    // Yes, type is the same!
                    return true;
                }
            
            // Active-Tile (Active).
            case "active":
                // Check Type.
                if (    %searchTileDef.type    $= "active" &&
                        %searchTileDef.name    $= getWord(%tileType, 1) )
                {
                    // Yes, type is the same!
                    return true;
                }
        }
    }
    else
    {
        // Check Type.
        if ( %searchTileDef.type $= getWord(%tileType, 0) )
        {
            // Yes, type is the same!
            return true;
        }
    }
    
    // Not the same!
    return false;
}


// --------------------------------------------------------------------
// Create Flood Node.
// --------------------------------------------------------------------
function createFloodNode( %queue, %tilePos )
{
    // Create Flood Node.
    %floodNode = new ScriptObject() { class = particleEditorFloodNode; };
    // Set Position.
    %floodNode.position = %tilePos;
    // Add Node to Queue.
    %queue.add( %floodNode );
}


// --------------------------------------------------------------------
// Draw Flood-Fill Brush.
// --------------------------------------------------------------------
function drawFloodBrush( %floodBrush, %worldPosition )
{
	// Is Brush Inactive?	
	if ( !%floodBrush.active )
	{
		// Yes, so warn.
		MessageBoxOK("Brush Error", "The current brush is not defined!", "");
		// Finish Here.
		return;
	}
    
    // Fetch Flood Brush.
    %floodBrushDef = %floodBrush.tileDef[0,0];       
    
	// Is Brush Position (0,0) Inactive?	
	if ( !%floodBrushDef.active )
	{
		// Yes, so warn.
		MessageBoxOK("Brush Error", "The current brush sub-position (0,0) is empty!", "");
		// Finish Here.
		return;
	}
                
	// Any Tile?
	if ( $activeTileLayer != -1 )
	{
		// Yes, so get Current Tile Layer Object.
		%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );
		
		// Check Object is valid.
		if ( %tileLayer )
		{
            // Yes, so get layer tile count.
            %temp = %tileLayer.getTileCount();
            %tileCountX = getWord( %temp, 0 );
            %tileCountY = getWord( %temp, 1 );
            
			// Find logical tile position.
			%tilePos = %tileLayer.pickTile( %worldPosition );
			
			// Got a tile?
			if ( %tilePos !$= "" )
            {
                // Is the Flood Brush Def already here?
                if ( isBrushDefHere( %floodBrushDef, %tileLayer, %tilePos ) )
                {
                    // Yes, so finish.
                    return;
                }
                
                // Create Queue.
                %queue = new SimGroup();
                
                // Create Search Tile Def.
                %searchTileDef = createBrushDef( %tileLayer, %tilePos );

                // Create Flood Node.
                createFloodNode( %queue, %tilePos );
            
                // Process Flood Nodes.
                while( %queue.getCount() )
                {
                    // Fetch Flood Node.
                    %floodNode = %queue.getObject( 0 );
                    
                    // Fetch Tile Position.
                    %tilePos = %floodNode.position;
                    // Fetch Seperate Positions.
                    %posX = getWord( %tilePos, 0 );
                    %posY = getWord( %tilePos, 1 );
                    
                    // Delete Flood Node.
                    %floodNode.delete();

                    // Is Search Tile-Def Here?
                    if ( isBrushDefHere( %searchTileDef, %tileLayer, %tilePos ) )
                    {
                        // Yes, so reset Scan-Row Span.
                        %spanFromX = %spanToX = %posX;
                        
                        // Scan Left.
                        if ( %posX > 0 )
                        {
                            // Search Left.
                            for ( %x = %posX-1; %x >= 0; %x-- )
                            {
                                // Is Brush-Def Here?
                                if ( !isBrushDefHere( %searchTileDef, %tileLayer, %x SPC %posY ) )
                                {
                                    // No, so note span-from.
                                    %spanFromX = %x+1;
                                    // Quit here.
                                    break;
                                }
                                
                                // Are we at the West-Edge?
                                if ( %x == 0 )
                                {
                                    // Yes, so note span-from.
                                    %spanFromX = %x;
                                    // Quit here.
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // We're already at the left so note span-from.
                            %spanFromX = %posX;
                        }
    
                        // Scan Right.
                        if ( %posX < (%tileCountX-1) )
                        {
                            // Search Right.
                            for ( %x = %posX+1; %x < %tileCountX; %x++ )
                            {
                                // Is Brush-Def Here?
                                if ( !isBrushDefHere( %searchTileDef, %tileLayer, %x SPC %posY ) )
                                {
                                    // No, so note span-to.
                                    %spanToX = %x-1;
                                    // Quit here.
                                    break;
                                }
                                
                                // Are we at the East-Edge?
                                if ( %x == (%tileCountX-1) )
                                {
                                    // Yes, so note span-from.
                                    %spanToX = %x;
                                    // Quit here.
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // We're already at the right so note span-to.
                            %spanToX = %posX;
                        }
    
                        // Fill Span.
                        for ( %x = %spanFromX; %x <= %spanToX; %x++ )
                        {
                            // Calculate Span Position.
                            %spanPos = %x SPC %posY;
                            
                            // North Node.
                            if ( %posY > 0 )
                            {
                                // Calculate Position.
                                %floodPos = %x SPC (%posY-1);
                                
                                // Check North; is Brush-Def Here?
                                if ( isBrushDefHere( %searchTileDef, %tileLayer, %floodPos ) )
                                {
                                    // Yes, so create Flood Node.
                                    createFloodNode( %queue, %floodPos );
                                }
                            }
    
                            // South Node.
                            if ( %posY < (%tileCountY-1) )
                            {
                                // Calculate Position.
                                %floodPos = %x SPC (%posY+1);
                                
                                // Check South; is Brush-Def Here?
                                if ( isBrushDefHere( %searchTileDef, %tileLayer, %floodPos ) )
                                {
                                    // Yes, so create Flood Node.
                                    createFloodNode( %queue, %floodPos );
                                }
                            }                        
                            
                            // Draw Flood Brush.
                            switch$( %floodBrushDef.type )
                            {
                                // Static-Tile (Image-Map).
                                case "static":
                                    %tileLayer.setStaticTile( %spanPos, %floodBrushDef.name, %floodBrushDef.frame );
                                
                                // Animated (Animation).
                                case "animated":
                                    %tileLayer.setAnimatedTile( %spanPos, %floodBrushDef.name );
                                
                            }
                            
                            // Set Tile Script.
                            %tileLayer.setTileScript( %spanPos, %floodBrushDef.script );
                            // Set Tile Custom-Data.
                            %tileLayer.setTileCustomData( %spanPos, %floodBrushDef.customData );
                            // Set Tile Flip
                            %tileLayer.setTileFlip( %spanPos, %floodBrushDef.flipX, %floodBrushDef.flipY );
                            
                            // Set Tile Collision.
                            // NOTE:- We don't draw collisions if "collision suppress is on".
                            %tileLayer.setTileCollisionActive( %spanPos, %floodBrushDef.collisionActive && !$collisionDrawSuppressStatus );
                            // Set Tile Collision Scale.
                            %tileLayer.setTileCollisionScale( %spanPos, %floodBrushDef.collisionScale );
                            // Collision Active?
                            if ( %floodBrushDef.collisionActive )
                                %tileLayer.setTileCollisionPolyCustom( %spanPos, %floodBrushDef.collisionPoly, %floodBrushDef.collisionDef );                        
                        }
                    }
                }
                
                // Delete Search Tile.
                %searchTileDef.delete();
                // Delete Queue.
                %queue.delete();
            }
        }
    }
}


//-----------------------------------------------------------------------------
// Erase Brush Area.
//-----------------------------------------------------------------------------
function eraseBrushArea( %startPosition, %endPosition )
{
	// Is Brush Inactive?	
	if ( !$brush[$currentBrush].active )
	{
		// Yes, so warn.
         MessageBoxOK("Brush Error", "The current brush is not defined!", "");
		 // Finish Here.
		 return;
	}
	
	// Fetch Tile Size.
	%tileSize = $editorTileMap.getTileLayer( $activeTileLayer ).getTileSize();
	// Fetch Component Steps.
	%tileStepX = getWord( %tileSize, 0 );
	%tileStepY = getWord( %tileSize, 1 );

	// Fetch Component Start/End of Positions.
	%startX = getWord( %startPosition, 0 );
	%startY = getWord( %startPosition, 1 );
	%endX = getWord( %endPosition, 0 );
	%endY = getWord( %endPosition, 1 );
	
	// Erase current brush across area at tile-steps.
	for ( %x = %startX; %x <= %endX; %x += %tileStepX )
		for ( %y = %startY; %y <= %endY; %y += %tileStepY )
			// Current Brush Eraser.
			eraserBrush( $brush[$currentBrush], %x SPC %y );
}


//-----------------------------------------------------------------------------
// Create Brush Area.
//
// NOTE:-	We create the brush in area "99" and remove it if we don't
//			actually assign it to the permanent brushes 1-9.
//-----------------------------------------------------------------------------
function createBrushArea( %startPosition, %endPosition )
{
	// Fetch Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );
	
	// Is the brush area completely within the tile-layer?
	if (	%tileLayer.pickTile(%startPosition) $= "" ||
			%tileLayer.pickTile(%endPosition) $= "" )
	{
		// No, so we cannot define a brush area.
         MessageBoxOK("Create Brush Area Error", "The area must be completely within the tile-layer to create a brush!", "");
		 return;
	}
	
	// Fetch Tile Size.
	%tileSize = %tileLayer.getTileSize();
	// Fetch Component Steps.
	%tileStepX = getWord( %tileSize, 0 );
	%tileStepY = getWord( %tileSize, 1 );

	// Fetch Component Start/End of Positions.
	%startX = getWord( %startPosition, 0 );
	%startY = getWord( %startPosition, 1 );
	%endX = getWord( %endPosition, 0 );
	%endY = getWord( %endPosition, 1 );

	// Create Temporary Brush.
	resetBrush( $tempBrushIndex );
	
	// Reference Temporary Brush.
	%brush = $brush[ $tempBrushIndex ];
	
	// Set Brush Width/Height.
	%brush.width	= mFloor((%endX-%startX)/%tileStepX) + 1;
	%brush.height	= mFloor((%endY-%startY)/%tileStepY) + 1;
	
	// Define Brush Area...
	
	// Reset Brush Column.
	%brushX = 0;
	
	// Step through columns.
	for ( %x = %startX; %x <= %endX; %x += %tileStepX )
	{
		// Reset Brush Row.
		%brushY = 0;
		
		// Step through rows.
		for ( %y = %startY; %y <= %endY; %y += %tileStepY )
		{
			// Fetch Logical Tile Position.
			%tilePos = %tileLayer.pickTile( %x SPC %y );
			
			// Create Single Tile-Def.
			%brush.tileDef[%brushX, %brushY] = new ScriptObject() { class = tileMapBrushDef; };
			
			// Fetch Tile Type.
			%tileType = %tileLayer.getTileType( %tilePos );

			// Fetch Tile-Def.
			%tileDef = %brush.tileDef[%brushX, %brushY];
            
            // Set Tile Type.
            %tileDef.tileType = %tileType;
			
			// Does Tile Exist?
			if ( %tileType !$= "" )
			{
				// Yes, so set Collision Info.
				%tileCollision = %tileLayer.getTileCollision( %tilePos );
				%tileDef.collisionActive	= getWord( %tileCollision, 0 );
				%tileDef.collisionScale		= getWords( %tileCollision, 1, 2 );
				%tileDef.collisionPoly 		= getWord( %tileCollision, 3 );
				%tileDef.collisionDef		= getWords( %tileCollision, 4, getWordCount( %tileCollision ) - 1 );
				
				// Set Tile Flip.
				%tileFlip = %tileLayer.getTileFlip( %tilePos );
				%tileDef.flipX				= getWord( %tileFlip, 0 );
				%tileDef.flipY				= getWord( %tileFlip, 1 );
				
				// Set Script.
				%tileScript = %tileLayer.getTileScript( %tilePos );
				%tileDef.script				= (%tileScript $= "") ? "" : %tileScript;

				// Set Custom-Data.
				%tileDef.customData			= %tileLayer.getTileCustomData( %tilePos );
	
				// Setup Tile-Def Content.
				switch$( getWord(%tileType, 0) )
				{
					// Static-Tile (Image-Map).
					case "static":
						%tileDef.type		= "static";
						%tileDef.name		= getWord(%tileType, 1);
						%tileDef.frame		= getWord(%tileType, 2); 
					
					// Animated-Tile (Animation).
					case "animated":
						%tileDef.type		= "animated"; 
						%tileDef.name		= getWord(%tileType, 1);
						%tileDef.frame		= -1;
						
					// Active-Tile (Active).
					case "active":
						%tileDef.type		= "active"; 
						%tileDef.name		= getWord(%tileType, 1);
						%tileDef.frame		= -1;
				}
				
				// Set Tile-Def Active.
				%tileDef.active = true;
				
				// We've got at least a single brush element so activate the whole brush.
				%brush.active = true;
			}
			else
			{
				// Set Tile-Def Inactive.
				%tileDef.active = false;
			}
			
			// Next Row.
			%brushY++;
		}
		
		// Next Column.
		%brushX++;
	}

	// Is Brush Inactive?	
	if ( !%brush.active )
	{
		// Yes, so we didn't get any brush components.
         MessageBoxOK("Create Brush Area Error", "The area must contain at least a single tile!", "");
		 
		 // Destroy Temporary Brush.
		 destroyBrush( $tempBrushIndex );
		 // Finish Here.
		 return;
	}

	// List Assign Brushes.
	listAssignBrushes();	
	// Show Assign-Brush Dialog.
	Canvas.pushDialog( tileAssignBrushDlg );
}



//-----------------------------------------------------------------------------
// Assign Brush Target.
//-----------------------------------------------------------------------------
function assignBrushTarget( %brushIndex )
{
	// Fetch Brush Name.
	$brush[$tempBrushIndex].name = txtBrushTargetName.getValue();
	
	// Remove Brush (if it exists).
	destroyBrush( %brushIndex );
	
	// Copy across temporary brush.
	$brush[ %brushIndex ] = $brush[ $tempBrushIndex ];
	
	// Reset Brush Reference.
	$brush[ $tempBrushIndex ] = -1;
	
	// Read Brush Selection.
	readBrushSelection();

	// Select Active Brush.
	selectActiveBrush( comAssignBrushList.getSelected() );
}


//-----------------------------------------------------------------------------
// Edit Tile.
//-----------------------------------------------------------------------------
function editTile( %worldPosition )
{
	// Fetch Tile Layer Count.
	%layerCount = $editorTileMap.getTileLayerCount();
	
	// Sanity!
	if ( %layerCount == 0 )
	{
		// Problem, so warn.
		MessageBoxOK("Edit Tile", "There are no layers to edit!", "");
		// Finish Here.
		return;
	}
	
	// Get Active Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );
	
	// Check Object is valid.
	if ( %tileLayer )
	{
		// Yes, so find logical tile position.
		%tilePick = %tileLayer.pickTile( %worldPosition );
		
		// Got a tile?
		if ( %tilePick !$= "" )
		{
			// Is there a valid tile here?
			if ( %tileLayer.getTileType( %tilePick ) !$= "" )
			{
				// Yes, so populate dialog with tile info...
				
				// Script.
				txtTileEditScript.setValue( %tileLayer.getTileScript( %tilePick ) );
				
				// Custom-Data.
				txtTileEditCustomData.setValue( %tileLayer.getTileCustomData( %tilePick ) );
				
				// Tile Flip Options.
				%tileFlip = %tileLayer.getTileFlip( %tilePick );
				chkTileEditTileFlipX.setValue( getWord( %tileFlip, 0 ) );
				chkTileEditTileFlipY.setValue( getWord( %tileFlip, 1 ) );

				// Collisions.
				%tileCollision = %tileLayer.getTileCollision( %tilePick );
				chkTileEditCollisionActive.setValue( getWord( %tileCollision, 0 ) );
				txtTileEditPolyScale.setValue( getWords( %tileCollision, 1, 2 ) );
				txtTileEditPolyCount.setValue( getWord( %tileCollision, 3 ) );
				txtTileEditPolyDefinition.setValue( getWords( %tileCollision, 4, getWordCount( %tileCollision ) - 1 ) );
				
				// Set Tile Reference.
				editTileDlg.layerRef = %tileLayer;
				editTileDlg.tileRef = %tilePick;
				
				// Show edit-tile dialog.
				Canvas.pushDialog( editTileDlg );
			}
			else
			{
				// No, so warn.
				MessageBoxOK("Edit Tile", "There is not a valid tile under the cursor!\n\nYou must draw to a tile before you can edit it.", "");
				// Finish Here.
				return;
			}
		}
		else
		{
			// No, so warn.
			MessageBoxOK("Edit Tile", "There is not a valid tile under the cursor!", "");
			// Finish Here.
			return;
		}
	}
}


//-----------------------------------------------------------------------------
// Fetch Custom Tile Data.
//-----------------------------------------------------------------------------
function fetchCustomTileData()
{
	// Get Active Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );
	
	// Check Object is valid.
	if ( %tileLayer )
	{
		// Populate Custom-Data with existing tile data, removing duplicates.
		comTileEditCustomData.clear();
		
		// Get Layer Tile Count.
		%tileCount = %tileLayer.getTileCount();
		%tileCountX = getWord( %tileCount, 0 );
		%tileCountY = getWord( %tileCount, 1 );
		// Scan Tile-Layer for custom-data.
		for ( %x = 0; %x < %tileCountX; %x++ )
		{
			for ( %y = 0; %y < %tileCountY; %y++ )
			{
				// Fetch Custom-Data.
				%customData = %tileLayer.getTileCustomData( %x SPC %y );
				// Is there some custom-data here?
				if ( %customData !$= "" )
				{
					// Yes, so add Custom-Data if it's not already in the list.
					if ( comTileEditCustomData.findText( %customData ) == -1 )
						comTileEditCustomData.add( %customData, 0 );
				}	
			}
		}
		
		// Are there any custom-data entries to choose from?
		if ( comTileEditCustomData.size() > 0 )
		{
			// Yes, so sort custom-data list.
			comTileEditCustomData.sort();
			// Show Pop-up.
			comTileEditCustomData.forceOnAction();
		}
		else
		{
			// No, so inform user.
			MessageBoxOK("Scan Custom-Data", "There is no custom-data in this layer to list!\n\nWhen you have existing custom-data, use this to select it consistently.", "");			
		}
	}
}


//-----------------------------------------------------------------------------
// Fetch Script Tile Data.
//-----------------------------------------------------------------------------
function fetchScriptTileData()
{
	// Get Active Tile Layer.
	%tileLayer = $editorTileMap.getTileLayer( $activeTileLayer );
	
	// Check Object is valid.
	if ( %tileLayer )
	{
		// Populate Script-Data with existing tile data, removing duplicates.
		comTileEditScriptData.clear();
		
		// Get Layer Tile Count.
		%tileCount = %tileLayer.getTileCount();
		%tileCountX = getWord( %tileCount, 0 );
		%tileCountY = getWord( %tileCount, 1 );
		// Scan Tile-Layer for script.
		for ( %x = 0; %x < %tileCountX; %x++ )
		{
			for ( %y = 0; %y < %tileCountY; %y++ )
			{
				// Fetch Script.
				%script = %tileLayer.getTileScript( %x SPC %y );
				// Is there some script here?
				if ( %script !$= "" )
				{
					// Yes, so add Scriptif it's not already in the list.
					if ( comTileEditScriptData.findText( %script ) == -1 )
						comTileEditScriptData.add( %script, 0 );
				}	
			}
		}
		
		// Are there any script entries to choose from?
		if ( comTileEditScriptData.size() > 0 )
		{
			// Yes, so sort script list.
			comTileEditScriptData.sort();
			// Show Pop-up.
			comTileEditScriptData.forceOnAction();
		}
		else
		{
			// No, so inform user.
			MessageBoxOK("Scan Script", "There are no scripts in this layer to list!\n\nWhen you have existing scripts, use this to select it consistently.", "");			
		}
	}
}
				
				
//-----------------------------------------------------------------------------
// Assign Tile Edit.
//-----------------------------------------------------------------------------
function assignTileEdit()
{
	// Store Undo.
	storeUndo();

	// Fetch Tile Layer.
	%layer = editTileDlg.layerRef;
	// Fetch Tile Position.
	%tilePos = editTileDlg.tileRef;
	
	// Set Tile Script.
	%layer.setTileScript( %tilePos, txtTileEditScript.getValue() );
	
	// Set Tile Custom-Data.
	%layer.setTileCustomData( %tilePos, txtTileEditCustomData.getValue() );
	
	// Set Tile Flip Options.
	%layer.setTileFlip( %tilePos, chkTileEditTileFlipX.getValue(), chkTileEditTileFlipY.getValue() );
	
	// Set Tile Collision.
	if ( !%layer.setTileCollisionActive( %tilePos, chkTileEditCollisionActive.getValue() ) )
	{
		// Show edit-tile dialog.
		Canvas.pushDialog( editTileDlg );
		// No, so warn.
		MessageBoxOK("Edit Tile", "Couldn't set collision status!", "");
	}

	// Set Scale Okay?	
	if ( !%layer.setTileCollisionScale( %tilePos, txtTileEditPolyScale.getValue() ) )
	{
		// Show edit-tile dialog.
		Canvas.pushDialog( editTileDlg );
		// No, so warn.
		MessageBoxOK("Edit Tile", "Scale is invalid!", "");
	}
	
	// Fetch Polygon Definition.
	%polyCount = txtTileEditPolyCount.getValue();
	%polyDef = trim(txtTileEditPolyDefinition.getValue());
	%polyDefCount = getWordCount(%polyDef);
	
	// Check Vertices.
	if ( %polyDefCount != (%polyCount*2) )
	{
		// Show edit-tile dialog.
		Canvas.pushDialog( editTileDlg );
		// No, so warn.
		MessageBoxOK("Edit Tile", "Collision Polygon Definition has an invalid number of vertices!", "");
	}
	
	if ( !%layer.setTileCollisionPolyCustom( %tilePos, %polyCount, %polyDef ) )
	{
		// Show edit-tile dialog.
		Canvas.pushDialog( editTileDlg );
		// No, so warn.
		MessageBoxOK("Edit Tile", "Collision Polygon is invalid!", "");
	}
}

			
//-----------------------------------------------------------------------------
// Left Mouse Down.
//-----------------------------------------------------------------------------
function tileSceneWindow2D::onMouseDown( %this, %modifier, %worldPosition, %mouseClicks )
{
	// Set Mouse Mode.
	switch( %modifier )
	{
		// None (Draw).
		case 0:
			// Store Undo.
			storeUndo();
			// Draw Left Brush.
			drawBrush( $brush[$currentBrush], %worldPosition, 0 );
			
			
		// Shift (Draw Area).
		case 1:
			// Set Mouse Mode.
			$mouseMode = "drawarea";
			// Start of Area.
			$mouseStart = %worldPosition;

		// Ctrl (Edit).
		case 4:
			// Edit Tile.
			editTile( %worldPosition );
		
			
		// Alt (Define Brush).
		case 16:			
			// Set Mouse Mode.
			$mouseMode = "definebrush";
			// Start of Area.
			$mouseStart = %worldPosition;
	}  
}


//-----------------------------------------------------------------------------
// Left Mouse Dragged.
//-----------------------------------------------------------------------------
function tileSceneWindow2D::onMouseDragged( %this, %modifier, %worldPosition, %mouseClicks )
{
	// Any Tile Layer?
	if ( $activeTileLayer != -1 )
	{
		// Yes, so get Current Tile Layer Object.
		%layerObj = $editorTileMap.getTileLayer( $activeTileLayer );
		
		// Check Object is valid.
		if ( %layerObj )
		{
			// Yes, so find logical tile position.
			%tilePick = %layerObj.pickTile( %worldPosition );
			
			// Got a tile?
			if ( %tilePick !$= "" )
			{
                // Set Tile Cursor Position.
                %layerObj.setCursorPosition( %tilePick );
			}
        }
    }
    
	// Draw Area?
	if ( $mouseMode $= "drawarea" )
	{
		// End of Area.
		$mouseEnd = %worldPosition;
		// Calculate Normalised Rectangle.
		%rect = t2dRectNormalise( $mouseStart, $mouseEnd );
		%start = getWords( %rect, 0, 1 );
		%end = getWords( %rect, 2, 3 );
		
		// Update Cursor Position.
		setCursorPos( %start, t2dVectorSub( %end, %start ) );
		// Show Cursor.
		tileEditorCursor.setVisible( true );

		// Finish here.		
		return;
	}
	
	// Define Brush?
	if ( $mouseMode $= "definebrush" )
	{
		// End of Area.
		$mouseEnd = %worldPosition;
		// Calculate Normalised Rectangle.
		%rect = t2dRectNormalise( $mouseStart, $mouseEnd );
		%start = getWords( %rect, 0, 1 );
		%end = getWords( %rect, 2, 3 );
		
		// Update Cursor Position.
		setCursorPos( %start, t2dVectorSub( %end, %start ) );
		// Show Cursor.
		tileEditorCursor.setVisible( true );

		// Finish here.		
		return;
	}
	
	
	// Set Mouse Mode.
	switch( %modifier )
	{
		// None (Draw).
		case 0:
			// Draw Brush.
			drawBrush( $brush[$currentBrush], %worldPosition, 0 );
	}
}


//-----------------------------------------------------------------------------
// Left Mouse Up.
//-----------------------------------------------------------------------------
function tileSceneWindow2D::onMouseUp( %this, %modifier, %worldPosition, %mouseClicks )
{
	// Handle Mouse Mode.
	switch$( $mouseMode )
	{
		// Draw Area.
		case "drawarea":
				
			// End of Area.
			$mouseEnd = %worldPosition;
			// Calculate Normalised Rectangle.
			%rect = t2dRectNormalise( $mouseStart, $mouseEnd );
			$mouseStart = getWords( %rect, 0, 1 );
			$mouseEnd = getWords( %rect, 2, 3 );

			// Store Undo.
			storeUndo();
			// Draw Brush Area.			
			drawBrushArea( $mouseStart, $mouseEnd );
			
			// Hide Cursor.
			tileEditorCursor.setVisible( false );
			// Clear Mouse Mode.
			$mouseMode = "";
			
		// Define Brush.
		case "definebrush":
				
			// End of Area.
			$mouseEnd = %worldPosition;
			// Calculate Normalised Rectangle.
			%rect = t2dRectNormalise( $mouseStart, $mouseEnd );
			$mouseStart = getWords( %rect, 0, 1 );
			$mouseEnd = getWords( %rect, 2, 3 );

			// Create Brush.
			createBrushArea( $mouseStart, $mouseEnd );
			
			// Hide Cursor.
			tileEditorCursor.setVisible( false );
			// Clear Mouse Mode.
			$mouseMode = "";			
	}
}


//-----------------------------------------------------------------------------
// Right Mouse Down.
//-----------------------------------------------------------------------------
function tileSceneWindow2D::onRightMouseDown( %this, %modifier, %worldPosition, %mouseClicks )
{
	// Set Mouse Mode.
	switch( %modifier )
	{
		// None (Draw).
		case 0:
			// Store Undo.
			storeUndo();
			// Eraser Brush.
			eraserBrush( $brush[$currentBrush], %worldPosition );
			
			
		// Shift (Draw Area).
		case 1:
			// Set Mouse Mode.
			$mouseMode = "erasearea";
			// Start of Area.
			$mouseStart = %worldPosition;	
			
			
		// Ctrl (Move).
		case 4:
			// Set new view position.
			$viewPosition = %worldPosition;
			// Set View.
			tileEditorSetView();	
            
            
		// Alt (Flood Draw).
		case 16:			
			// Store Undo.
			storeUndo();
			// Draw Flood Brush.
			drawFloodBrush( $brush[$currentBrush], %worldPosition );
	}
}


//-----------------------------------------------------------------------------
// Right Mouse Dragged.
//-----------------------------------------------------------------------------
function tileSceneWindow2D::onRightMouseDragged( %this, %modifier, %worldPosition, %mouseClicks )
{
	// Any Tile Layer?
	if ( $activeTileLayer != -1 )
	{
		// Yes, so get Current Tile Layer Object.
		%layerObj = $editorTileMap.getTileLayer( $activeTileLayer );
		
		// Check Object is valid.
		if ( %layerObj )
		{
			// Yes, so find logical tile position.
			%tilePick = %layerObj.pickTile( %worldPosition );
			
			// Got a tile?
			if ( %tilePick !$= "" )
			{
                // Set Tile Cursor Position.
                %layerObj.setCursorPosition( %tilePick );
			}
        }
    }       
            
	// Erase Area?
	if ( $mouseMode $= "erasearea" )
	{
		// End of Area.
		$mouseEnd = %worldPosition;
		// Calculate Normalised Rectangle.
		%rect = t2dRectNormalise( $mouseStart, $mouseEnd );
		%start = getWords( %rect, 0, 1 );
		%end = getWords( %rect, 2, 3 );
		
		// Update Cursor Position.
		setCursorPos( %start, t2dVectorSub( %end, %start ) );
		// Show Cursor.
		tileEditorCursor.setVisible( true );

		// Finish here.		
		return;
	}
	
	// Set Mouse Mode.
	switch( %modifier )
	{
		// None (Draw).
		case 0:

			// Eraser Brush.
			eraserBrush( $brush[$currentBrush], %worldPosition );
	}
}


//-----------------------------------------------------------------------------
// Right Mouse Up.
//-----------------------------------------------------------------------------
function tileSceneWindow2D::onRightMouseUp( %this, %modifier, %worldPosition, %mouseClicks )
{
	// Handle Mouse Mode.
	switch$( $mouseMode )
	{
		// Draw Area.
		case "erasearea":
				
			// End of Area.
			$mouseEnd = %worldPosition;
			// Calculate Normalised Rectangle.
			%rect = t2dRectNormalise( $mouseStart, $mouseEnd );
			$mouseStart = getWords( %rect, 0, 1 );
			$mouseEnd = getWords( %rect, 2, 3 );

			// Store Undo.
			storeUndo();
			// Erase Brush Area.			
			eraseBrushArea( $mouseStart, $mouseEnd );
			
			// Hide Cursor.
			tileEditorCursor.setVisible( false );
			// Clear Mouse Mode.
			$mouseMode = "";
	}
}


//-----------------------------------------------------------------------------
// Mouse Move.
//-----------------------------------------------------------------------------
function tileSceneWindow2D::onMouseMove( %this, %modifier, %worldPosition, %mouseClicks )
{
	// Default, no status.
	%tileStatus = "";

	// Any Tile?
	if ( $activeTileLayer != -1 )
	{
		// Yes, so get Current Tile Layer Object.
		%layerObj = $editorTileMap.getTileLayer( $activeTileLayer );
		
		// Check Object is valid.
		if ( %layerObj )
		{
			// Yes, so find logical tile position.
			%tilePick = %layerObj.pickTile( %worldPosition );
			
			// Got a tile?
			if ( %tilePick !$= "" )
			{
				// Yes, so add to status.
				%tileStatus = %tileStatus SPC "TileX:" @ getWord(%tilePick,0) SPC "TileY:" @ getWord(%tilePick,1);
				// Fetch Tile-Layer Count.
				%tileCount = %layerObj.getTileCount();
				// Add Tile Size.
				%tileStatus = %tileStatus SPC "of [0-" @ (getWord(%tileCount,0)-1) @ ", 0-" @ (getWord(%tileCount,1)-1) @ "]";
				
				// Hide Cursor.
				//tileEditorCursor.setSize( $editorTileMap.getTileLayer( $activeTileLayer ).getTileSize() );
				//tileEditorCursor.setVisible( false );
                
                // Set Tile Cursor Position.
                %layerObj.setCursorPosition( %tilePick );
			}
		}
	}

	// Set Mouse Status.
	setMouseStatusText( %tileStatus );
}


//-----------------------------------------------------------------------------
// Set Mouse Status Text.
//-----------------------------------------------------------------------------
function setMouseStatusText( %statusText )
{
	// Set MouseStatus.
	lblTileMouseStatus.setValue( ">" SPC %statusText );	
}


//-----------------------------------------------------------------------------
// Set Layer Status Text.
//-----------------------------------------------------------------------------
function setLayerStatusText( %statusText )
{
	// Set Tile-Map Status.
	lblTileLayerStatus.setValue( ">" SPC %statusText );	
}


//-----------------------------------------------------------------------------
// Set Brush Status Text.
//-----------------------------------------------------------------------------
function setBrushStatusText( %statusText )
{
	// Set Tile-Map Status.
	lblTileBrushStatus.setValue( ">" SPC %statusText );	
}

//-----------------------------------------------------------------------------
// Set View.
//-----------------------------------------------------------------------------
function tileEditorSetView()
{
	// Set Camera Position.
	tileSceneWindow2D.setTargetCameraPosition( $viewPosition SPC $tileEditor2D::viewWidth SPC $tileEditor2D::viewHeight );
	
	// Setup Camera Zoom.
	tileSceneWindow2D.setTargetCameraZoom( $tileEditor2D::viewZoom );
	
	// Start Camera Move.
	tileSceneWindow2D.startCameraMove( $tileEditor2D::moveTime );
}


//-----------------------------------------------------------------------------
// Edit View.
//-----------------------------------------------------------------------------
function tileEditorEditView()
{
	// Set Dialog Values.
	sldTileViewWidth.setValue( $tileEditor2D::viewWidth );
	sldTileViewHeight.setValue( $tileEditor2D::viewHeight );
	sldTileViewZoom.setValue( $tileEditor2D::viewZoom );

	// Pop Tile Editor-Key.
	tileEditorMap.pop();
	
	// Show Tile-View Dialog.
	Canvas.pushDialog(tileViewDlg);	
}


//-----------------------------------------------------------------------------
// Try View.
//-----------------------------------------------------------------------------
function tileEditorTryView()
{
	// Setup Camera Position.
	tileSceneWindow2D.setCurrentCameraPosition( $viewPosition SPC sldTileViewWidth.getValue() SPC sldTileViewHeight.getValue() );
	
	// Setup Camera Zoom.
	tileSceneWindow2D.setCurrentCameraZoom( sldTileViewZoom.getValue() );
}


//-----------------------------------------------------------------------------
// Write View.
//-----------------------------------------------------------------------------
// altered this one to read $tileEditor2D::viewXXX to be read from the TextCtrls
// and not from the sliders. This way, one can write something into the TextCtrls
// and press the "save" button without pressing enter. Otherwise changes would be
// lost.
function tileEditorWriteView()
{
	// Validate Ranges
	tileEditorValidateViewTextCtrls("w");
	tileEditorValidateViewTextCtrls("h");
	tileEditorValidateViewTextCtrls("z");
	
	// Fetch Dialog Values.
	$tileEditor2D::viewWidth = txtTileViewWidth.getValue();
	$tileEditor2D::viewHeight = txtTileViewHeight.getValue();
	$tileEditor2D::viewZoom = txtTileViewZoom.getValue();
	
	// Set View.
	tileEditorSetView();
}


//-----------------------------------------------------------------------------
// Reset View.
//-----------------------------------------------------------------------------
// altered this one not to change $tileEditor2D::viewXXX. This way resetting the
// view can be undone by pressing the cancel button.
function tileEditorResetView()
{
	// Set Dialog Values.
	sldTileViewWidth.setValue( 100 );
	sldTileViewHeight.setValue( 75 );
	sldTileViewZoom.setValue( 1 );	

	// Try View.
	tileEditorTryView();	
}

// Validates input of the TextCtrls so that they obey to the same ranges as the sliders
// and a the same time prevents non-numeric input
function tileEditorValidateViewTextCtrls(%field)
{
	if(%field $= "w")
	{
		if( txtTileViewWidth.getValue() < getWord( sldTileViewWidth.range , 0 ) ) txtTileViewWidth.setValue( getWord( sldTileViewWidth.range , 0 ) );
		else
		if( txtTileViewWidth.getValue() > getWord( sldTileViewWidth.range , 1 ) ) txtTileViewWidth.setValue( getWord( sldTileViewWidth.range , 1 ) );
	}
	else
	if(%field $= "h")
	{
		if( txtTileViewHeight.getValue() < getWord( sldTileViewHeight.range , 0 ) ) txtTileViewHeight.setValue( getWord( sldTileViewHeight.range , 0 ) );
		else
		if( txtTileViewHeight.getValue() > getWord( sldTileViewHeight.range , 1 ) ) txtTileViewHeight.setValue( getWord( sldTileViewHeight.range , 1 ) );
	}
	else
	if(%field $= "z")
	{
		if( txtTileViewZoom.getValue() < getWord( sldTileViewZoom.range , 0 ) ) txtTileViewZoom.setValue( getWord( sldTileViewZoom.range , 0 ) );
		else
		if( txtTileViewZoom.getValue() > getWord( sldTileViewZoom.range , 1 ) ) txtTileViewZoom.setValue( getWord( sldTileViewZoom.range , 1 ) );
	}
}

// Updates the sliders if TextCtrls are changed
function tileEditorUpdateViewSliders(%field)
{
	tileEditorValidateViewTextCtrls(%field);	
	
	if(%field $= "w") sldTileViewWidth.setValue( txtTileViewWidth.getValue() );
	else
	if(%field $= "h") sldTileViewHeight.setValue( txtTileViewHeight.getValue() );
	else
	if(%field $= "z") sldTileViewZoom.setValue( txtTileViewZoom.getValue() );
}

// Updates the TextCtrls if sliders are changed
function tileEditorUpdateViewTextCtrls(%field)
{
	if(%field $= "w") txtTileViewWidth.setValue( sldTileViewWidth.getValue() );
	else
	if(%field $= "h") txtTileViewHeight.setValue( sldTileViewHeight.getValue() );
	else
	if(%field $= "z") txtTileViewZoom.setValue( sldTileViewZoom.getValue() );		
}


//-----------------------------------------------------------------------------
// Tile Layer Next.
//-----------------------------------------------------------------------------
function tileLayerNext()
{
	// Clear Undo/Redo.		
	clearUndoRedo();
	
	// Get Tile-Layer Count.
	%layerCount = $editorTileMap.getTileLayerCount();
	
	// Finish if no tile-layers.
	if ( %layerCount == 0 )
		return;
		
	// Finish if last layer.
	if ( $activeTileLayer >= (%layerCount-1) )
		return;
		
	// Select Previous Layer.
	selectTileLayer( $activeTileLayer + 1 );
}


//-----------------------------------------------------------------------------
// Tile Layer Previous.
//-----------------------------------------------------------------------------
function tileLayerPrevious()
{
	// Clear Undo/Redo.		
	clearUndoRedo();
	
	// Get Tile-Layer Count.
	%layerCount = $editorTileMap.getTileLayerCount();

	// Finish if no tile-layers.
	if ( %layerCount == 0 )
		return;
		
	// Finish if first layer.
	if ( $activeTileLayer == 0 )
		return;
		
	// Select Previous Layer.
	selectTileLayer( $activeTileLayer - 1 );
}


// --------------------------------------------------------------------
// Tile Layer View (Top-Left).
// --------------------------------------------------------------------
function tileLayerTopLeft()
{
	// Reset Zoom.
	$tileEditor2D::viewZoom = 1.0;
	
	// Calculate View Offset.
	%viewOffsetX = ($tileEditor2D::viewWidth/$tileEditor2D::viewZoom) * 0.5;
	%viewOffsetY = ($tileEditor2D::viewHeight/$tileEditor2D::viewZoom) * 0.5;
	
	// Set new View Position.
	$viewPosition = %viewOffsetX SPC %viewOffsetY;
	
	// Set View.
	tileEditorSetView();
}


//-----------------------------------------------------------------------------
// Tile Layer Left.
//-----------------------------------------------------------------------------
function tileLayerLeft()
{
	// Get Current Tile Layer Object.
	%layerObj = $editorTileMap.getTileLayer( $activeTileLayer );
	
	// Check Object is valid.
	if ( !%layerObj )
		return;
	
	// Calculate New Position.
	%newPosX = getWord($viewPosition,0) - (($tileEditor2D::viewWidth/$tileEditor2D::viewZoom) * 0.25);
	
	// Set new View Position.
	$viewPosition = %newPosX SPC getWord($viewPosition,1);
	
	// Set View.
	tileEditorSetView();
}


//-----------------------------------------------------------------------------
// Tile Layer Right.
//-----------------------------------------------------------------------------
function tileLayerRight()
{
	// Get Current Tile Layer Object.
	%layerObj = $editorTileMap.getTileLayer( $activeTileLayer );
	
	// Check Object is valid.
	if ( !%layerObj )
		return;
	
	// Calculate New Position.
	%newPosX = getWord($viewPosition,0) + (($tileEditor2D::viewWidth/$tileEditor2D::viewZoom) * 0.25);
	
	// Set new View Position.
	$viewPosition = %newPosX SPC getWord($viewPosition,1);
	
	// Set View.
	tileEditorSetView();
}


//-----------------------------------------------------------------------------
// Tile Layer Up.
//-----------------------------------------------------------------------------
function tileLayerUp()
{
	// Get Current Tile Layer Object.
	%layerObj = $editorTileMap.getTileLayer( $activeTileLayer );
	
	// Check Object is valid.
	if ( !%layerObj )
		return;
	
	// Calculate New Position.
	%newPosY = getWord($viewPosition,1) - (($tileEditor2D::viewHeight/$tileEditor2D::viewZoom) * 0.25);
	
	// Set new View Position.
	$viewPosition = getWord($viewPosition,0) SPC %newPosY;
	
	// Set View.
	tileEditorSetView();
}


//-----------------------------------------------------------------------------
// Tile Layer Down.
//-----------------------------------------------------------------------------
function tileLayerDown()
{
	// Get Current Tile Layer Object.
	%layerObj = $editorTileMap.getTileLayer( $activeTileLayer );
	
	// Check Object is valid.
	if ( !%layerObj )
		return;
	
	// Calculate New Position.
	%newPosY = getWord($viewPosition,1) + (($tileEditor2D::viewHeight/$tileEditor2D::viewZoom) * 0.25);
	
	// Set new View Position.
	$viewPosition = getWord($viewPosition,0) SPC %newPosY;
	
	// Set View.
	tileEditorSetView();
}


//-----------------------------------------------------------------------------
// Zoom Out.
//-----------------------------------------------------------------------------
function zoomOut()
{
	// Increase Zoom.
	$tileEditor2D::viewZoom -= $tileEditor2D::zoomStep;

	// Limit Zoom Out.
	if ( $tileEditor2D::viewZoom < 0.1 )
		$tileEditor2D::viewZoom = 0.1;
	
	// Setup Camera Zoom.
	tileSceneWindow2D.setTargetCameraZoom( $tileEditor2D::viewZoom );
	
	// Start Camera Move.
	tileSceneWindow2D.startCameraMove( $tileEditor2D::moveTime );
}


//-----------------------------------------------------------------------------
// Zoom In.
//-----------------------------------------------------------------------------
function zoomIn()
{
	// Increase Zoom.
	$tileEditor2D::viewZoom += $tileEditor2D::zoomStep;

	// Limit Zoom In.
	if ( $tileEditor2D::viewZoom > 100.0 )
		$tileEditor2D::viewZoom = 100.0;
	
	// Setup Camera Zoom.
	tileSceneWindow2D.setTargetCameraZoom( $tileEditor2D::viewZoom );
	
	// Start Camera Move.
	tileSceneWindow2D.startCameraMove( $tileEditor2D::moveTime );
}

