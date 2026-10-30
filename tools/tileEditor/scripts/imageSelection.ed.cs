//-----------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------


// --------------------------------------------------------------------
// Image Selection State.
// --------------------------------------------------------------------
$fixedImageTileSize = 128;	
$fixedImageTileGutter = 1;
$fixedImageTileReduction = 16;

$defaultBrushSizeX = 1;
$defaultBrushSizeY = 1;

// Collision Draw/Suppress Flags.
$collisionDrawStatus = false;
$collisionDrawSuppressStatus = false;


// Don't change this unless the brush details change as you will
// invalidate previously saved brushes.
$brushVersion = 1;

// Directory where brushes are stored.
$currentBrushDirectory = "tools/tileEditor/data/brushes/";

// Directories to find maps/layers.
$currentMapDirectory = "TGB/data/maps/";
$currentLayerDirectory = "TGB/data/layers/";

// If this is increased, only brushes 1-9 will have keyboard shortcuts.
// Don't decrease it below 9!
$maxBrushes = 20;
// Don't Change this line...
$tempBrushIndex = $maxBrushes+1;

// Old Extents.
$oldImageSelectionExtents = "";


// --------------------------------------------------------------------
// Initialise All Brushes.
// --------------------------------------------------------------------
function initialiseBrushes()
{
	// Initialise All Brushes.
	// NOTE:-	Brush#0 is default brush.
	//			Brush#1-9 are custom area brushes.
	for ( %n = 1; %n <= $maxBrushes; %n++ )
		resetBrush(%n);
}


// --------------------------------------------------------------------
// Destroy Brushes.
// --------------------------------------------------------------------
function destroyBrushes()
{
	// Destroy All Brushes.
	for ( %n = 1; %n <= $maxBrushes; %n++ )
		destroyBrush(%n);	
}


// --------------------------------------------------------------------
// Reset Brush.
//
// NOTE:-	Brush Definition is:
//
//				.name				= Brush Name.
//				.width				= Width of brush (in tiles).
//				.height				= Height of brush (in tiles).
//				.tileDef[%row,%col]	= Tile-Definition array at %row/%col offset. 
// --------------------------------------------------------------------
function resetBrush( %brushIndex )
{
	// Remove Brush (if it exists).
	destroyBrush( %brushIndex );
	
	// Create Brush.
	createBrush( %brushIndex );
}


// --------------------------------------------------------------------
// Create Brush.
// --------------------------------------------------------------------
function createBrush( %brushIndex )
{
	// Create Inactive Brush Object.
	$brush[ %brushIndex ] = new ScriptObject() { class = tileMapBrush; };
	// Set Brush Name.
	$brush[ %brushIndex ].name = "Brush #" @ %brushIndex;
	// Set Width/Height.
	$brush[ %brushIndex ].width = 0;
	$brush[ %brushIndex ].height = 0;
	// Reset Brush Definition.
	$brush[ %brushIndex ].active = false;		
}

// --------------------------------------------------------------------
// Destroy Brush.
// --------------------------------------------------------------------
function destroyBrush( %brushIndex )
{
	// Remove Brush (if it exists).
	if ( isObject($brush[%brushIndex]) )
	{
		// Get Brush Ref.
		%brush = $brush[%brushIndex];
		
		// Destroy Tile Definitions.
		for ( %x = 0; %x < %brush.width; %x++ )
			for ( %y = 0; %y < %brush.height; %y++ )
				// Tile Def?
				if ( isObject(%brush.tileDef[%x,%y]) )
					// Yes, so remove.
					%brush.tileDef[%x,%y].delete();

		// Now remove brush object.					
		$brush[%brushIndex].delete();
	}
}


// --------------------------------------------------------------------
// Read Image Selection.
// --------------------------------------------------------------------
function readImageSelection()
{
	// Clear Tile Image Selection.
	comTileImageIndex.clear();

	// Enumerate imageMap Datablocks.
	for ( %n = 0; %n < $datablockSet.getCount(); %n++ )
	{
		// Fetch Datablock.
		%datablock = $datablockSet.getObject(%n);
		// Fetch Classname.
		%className = %datablock.getClassname();
		
		// Is it an imagemap?
		if  (%className $= "t2dImageMapDatablock" )
			// Yes, so select it. 		
			comTileImageIndex.add( %datablock.getName(), %datablock.getId() );
		else if  (%className $= "t2dAnimationDatablock" )
			// Yes, so select it. 		
			comTileImageIndex.add( "*" SPC %datablock.getName(), %datablock.getId() );
	}
		
	// Select first tile-layer (even if there arn't any).
	comTileImageIndex.setSelected( 0 );
}


// --------------------------------------------------------------------
// Select Tile Image Set.
// --------------------------------------------------------------------
function selectTileImageSet( %id )
{
	// Ignore non-selections.
	if ( %id == 0 )
		return;
	
	// Delete Image-Group if it exists.
	if ( isObject(imageTileGroup) )
		imageTileGroup.delete();
		
	// Create new Image-Group.
	new SimGroup(imageTileGroup);
	
	// Handle Image-Datablock Appropriately.
	switch$( %id.getClassName() )
	{
		// Image-Map.
		case "t2dImageMapDatablock":
		
			// Add Image-Tiles.
			for ( %n = 0; %n < %id.getFrameCount(); %n++ )
			{
				// Create Image-Tile.
				%imageTile = new t2dStaticSprite() { scenegraph = tileImagesSceneGraph2D; };
				%imageTile.setGraphGroup( 0 );
				%imageTile.setLayer( 0 );
				%imageTile.setImageMap( %id.getName(), %n );
				%imageTile.setCollisionActive( false, true );
				%imageTile.tagId = %id;
				%imageTile.tagFrame = %n;
				// Add to image-tile Group.
				imageTileGroup.add( %imageTile );
			}
			
		
		// Animation.
		case "t2dAnimationDatablock":
		
			// Create Image-Tile.
			%imageTile = new t2dAnimatedSprite() { scenegraph = tileImagesSceneGraph2D; };
			%imageTile.setGraphGroup( 0 );
			%imageTile.setLayer( 0 );
			%imageTile.playAnimation( %id.getName() );
			%imageTile.setCollisionActive( false, true );
			%imageTile.tagId = %id;
			%imageTile.tagFrame = -1;	// Not Used.
			// Add to image-tile Group.
			imageTileGroup.add( %imageTile );
		
	}
	
	// Repaginate Image-Tiles.
	repaginateImageTiles( $fixedImageTileSize );
}


// --------------------------------------------------------------------
// Repaginate Image-Tiles.
// --------------------------------------------------------------------
function repaginateImageTiles( %imageTileSize )
{
	// Finish if there's no image-group.
	if ( !isObject(imageTileGroup) )
		return;
		
	// Finish if there's no image-tiles.
	if ( imageTileGroup.getCount() == 0 )
		return;
	
	// Fetch Window Extents (size).
	%viewSize = getWords( imageSceneWindow2D.getWindowExtents(), 2, 3 );
	%viewSizeX = getWord( %viewSize, 0 );
	%viewSizeY = getWord( %viewSize, 1 );
	
	// Calculate Tile Stride.
	%tileStride = %imageTileSize + $fixedImageTileGutter;
	
	// Calculate Fit.
	%fitX = mFloor(%viewSizeX / %tileStride);
	%fitY = mFloor(%viewSizeY / %tileStride);

	if ( %imageTileSize > ($fixedImageTileReduction+4) )
	{
		// Would the current sized tiles fit?
		if ( (%fitX*%fitY) < imageTileGroup.getCount() )
		{
			// No, so try smaller Image-Tiles.
			repaginateImageTiles( %imageTileSize - $fixedImageTileReduction );
			// Finish here (stack unroll).
			return;
		}
	}

	// Calculate Tile Offset.
	%tileOffset = %imageTileSize * 0.5;
	
	// Calculate Tile-Image Fit.
	%tilesX = mFloor( %viewSizeX / %tileStride );
	%tilesY = mFloor( %viewSizeY / %tileStride );

	// Reset Cursor.
	%cursorX = 0;
	%cursorY = 0;
	
	// Reposition Image-Tiles.
	for ( %n = 0; %n < imageTileGroup.getCount(); %n++ )
	{
		// Fetch Image-Tile.
		%imageTile = imageTileGroup.getObject(%n);
		// Reposition.
		%imageTile.setPosition( (%cursorX+%tileOffset) SPC (%cursorY+%tileOffset) );
		// Resize.
		%imageTile.setSize( %imageTileSize SPC %imageTileSize );
		
		// Stride Cursor X
		%cursorX += %tileStride;
		
		// Check for edge X.
		if ( (%cursorX+%imageTileSize) >= %viewSizeX )
		{
			// Reset Cursor X.
			%cursorX = 0;

			// Stride Cursor Y
			%cursorY += %tileStride;
		}
	}
}


// --------------------------------------------------------------------
// Resize Image Scene Window.
// --------------------------------------------------------------------
function imageSceneWindow2D::onExtentChange( %this, %newExtent )
{
	// Ignore if the same as last extents.
	if ( $oldImageSelectionExtents $= %newExtent )
		return;
		
	// Store new Extents.
	$oldImageSelectionExtents = %newExtent;
		
	// Setup Camera View.
	imageSceneWindow2D.setCurrentCameraArea( "0 0" SPC  getWord(%newExtent,2) SPC getWord(%newExtent,3) );
	
	// Repaginate Image-Tiles.
	repaginateImageTiles( $fixedImageTileSize );
}


//-----------------------------------------------------------------------------
// Left Mouse Down.
//-----------------------------------------------------------------------------
function imageSceneWindow2D::onMouseDown( %this, %modifier, %worldPosition, %mouseClicks )
{
	// Pick Image-Tiles.
	%pickList = tileImagesSceneGraph2D.pickPoint( %worldPosition );
	
	// Finish if no image-tile selected.
	if ( getWordCount( %pickList ) < 1 )
		return;
	
	// Get Pick Object.
	%pickObj = getWord( %pickList, 0 );
	
	// Is the current brush active?
	if ( $tileEditor2D::confirmBrushOverwrites && $brush[$currentBrush].active )
	{
		// Query about brush overwrite.
		MessageBoxYesNo("Assign Brush...", "Are you sure you want to overwrite the current brush '" @ $brush[$currentBrush].name @ "'?", "assignBrushFromLibrary(" @ %pickObj @ ");", "" );
		return;
	}
	
	// Assign Brush from library.
	assignBrushFromLibrary( %pickObj );
}


//-----------------------------------------------------------------------------
// Assign Brush From Library.
//-----------------------------------------------------------------------------
function assignBrushFromLibrary( %pickObj )
{
	// Reset Brush.
	resetBrush( $currentBrush );
		
	// Setup Default Brush.
	%brush					= $brush[$currentBrush];
	%brush.name				= "Brush #" @ $currentBrush;
	%brush.width			= $defaultBrushSizeX;
	%brush.height			= $defaultBrushSizeY;
	
	for ( %x = 0; %x < $defaultBrushSizeX; %x++ )
	{
		for ( %y = 0; %y < $defaultBrushSizeY; %y++ )
		{
			// Create Single Tile-Def.
			%brush.tileDef[%x,%y] = new ScriptObject() { class = tileMapBrushDef; };
			
			// Fetch Tile-Def.
			%tileDef = %brush.tileDef[%x,%y];
			
			// Set Collision Defaults.
			%tileDef.collisionActive	= $collisionDrawStatus;
			%tileDef.collisionScale	= "1 1";
			%tileDef.collisionPoly 	= 4;
			%tileDef.collisionDef	= "-1 -1 1 -1 1 1 -1 1";
			
			// Set Tile Flip.
			%tileDef.flipX			= false;
			%tileDef.flipY			= false;
			
			// Set Script Default.
			%tileDef.script			= "";
			
			// Set Custom-Data Default.
			%tileDef.customData		= "";
			
			// Setup Tile-Def Content.
			switch$( %pickObj.tagId.getClassName() )
			{
				// Image-Map.
				case "t2dImageMapDatablock":
					%tileDef.active	= true;
					%tileDef.type	= "static";
					%tileDef.name	= %pickObj.tagId.getName();
					%tileDef.frame	= %pickObj.tagFrame; 
				
				// Animation.
				case "t2dAnimationDatablock":
					%tileDef.active	= true;
					%tileDef.type	= "animated"; 
					%tileDef.name	= %pickObj.tagId.getName();
					%tileDef.frame	= -1;
			}
            
            // Make the Brush Def compatible with the tile-type returned from the tile-layer.
            %tileDef.tileType = %tileDef.type SPC %tileDef.name SPC %tileDef.frame;
		}
	}
	
	// Activate Brush.
	%brush.active = true;
	
	// Read Brush Selection.
	readBrushSelection();	
	
	// Reselect Active Brush (for check mark).
	selectActiveBrush( $currentBrush );
}


//-----------------------------------------------------------------------------
// Right Mouse Down.
//-----------------------------------------------------------------------------
function imageSceneWindow2D::onRightMouseDown( %this, %modifier, %worldPosition, %mouseClicks )
{
}


//-----------------------------------------------------------------------------
// Select Brush.
//-----------------------------------------------------------------------------
function selectActiveBrush( %brushIndex )
{
	// Validate Brush Index.
	if ( %brushIndex < 1 || %brushIndex > $maxBrushes )
	{
		// Yes, so warn.
		MessageBoxOK("Active Brush Error", "Attempt to activate an invalid brush! (" @ %brushIndex @ ")", "");
		// Finish Here.
		return;
	}
	
	// Set Active Brush.
	$currentBrush = %brushIndex;
	
	// Check Appropriate Brush.
	for ( %n = 1; %n <= $maxBrushes; %n++ )
		tileScreenMenu.setMenuItemChecked("Brush", $brush[%n].name, %n == $currentBrush);

	// Set Collision Draw Suppress Status.
	setCollisionDrawSuppress( $collisionDrawSuppressStatus );

	// Set Brush Status.
	setBrushStatusText( $brush[$currentBrush].name SPC "W:" @ $brush[$currentBrush].width @ "/ H:" @ $brush[$currentBrush].height SPC "(#" @ $currentBrush @ ")" );		
}


//-----------------------------------------------------------------------------
// Read Brush Selection.
//-----------------------------------------------------------------------------
function readBrushSelection()
{
	// Clear Menu Items.
	tileScreenMenu.clearMenuItems("Brush");
	// Setup Brush Menu.
	tileScreenMenu.addMenuItem("Brush", "Reset Brushes...", 0);
	tileScreenMenu.addMenuItem("Brush", "-", 0);
	tileScreenMenu.addMenuItem("Brush", "Load Brushes...", 1);
	tileScreenMenu.addMenuItem("Brush", "Save Brushes...", 2);
	tileScreenMenu.addMenuItem("Brush", "-", 0);
	tileScreenMenu.addMenuItem("Brush", "Suppress Collision Draw", 3);
	tileScreenMenu.addMenuItem("Brush", "Confirm Brush Overwrites", 4);
	tileScreenMenu.addMenuItem("Brush", "-", 0);
	// Custom Brushes.
	for ( %n = 1; %n <= $maxBrushes; %n++ )
		tileScreenMenu.addMenuItem("Brush", $brush[%n].name, 4+%n);
		
	// Load/Save Brushes.
	tileScreenMenu.scriptCommand["Brush", 1] = "getLoadFilename(\"tileeditor/client/brushes/*.bru\", loadBrushes);";		
	tileScreenMenu.scriptCommand["Brush", 2] = "getSaveFilename(\"tileeditor/client/brushes/*.bru\", saveBrushes, $currentBrushDirectory);";		
	tileScreenMenu.scriptCommand["Brush", 3] = "setCollisionDrawSuppress(!$collisionDrawSuppressStatus);";		
	tileScreenMenu.scriptCommand["Brush", 4] = "setBrushOverwrites(!$tileEditor2D::confirmBrushOverwrites);";		
		
	// Brush Menu Commands.
	tileScreenMenu.scriptCommand["Brush", 0] = "MessageBoxYesNo( \"Reset Brushes\", \"Are you sure you want to reset all brushes?\", \"initialiseBrushes(); readBrushSelection(); selectActiveBrush(1);\", \"\" );";
	// Select Brush Commands.
	for ( %n = 1; %n <= $maxBrushes; %n++ )
		tileScreenMenu.scriptCommand["Brush", 4+%n] = "selectActiveBrush(" @ %n @ ");";
		
	// Handle Checks.
	setCollisionDrawSuppress( $collisionDrawSuppressStatus );
	setBrushOverwrites( $tileEditor2D::confirmBrushOverwrites );
}



//-----------------------------------------------------------------------------
// Set Collision Draw Suppress.
//-----------------------------------------------------------------------------
function setCollisionDrawSuppress( %status )
{
	// Set Collision Draw Suppress Status.
	$collisionDrawSuppressStatus = %status;
	
	// Check Collision Draw Flag.
	tileScreenMenu.setMenuItemChecked("Brush", "Suppress Collision Draw", $collisionDrawSuppressStatus );
}


//-----------------------------------------------------------------------------
// Set Brush Overwrite.
//-----------------------------------------------------------------------------
function setBrushOverwrites( %status )
{
	// Set Collision Draw Suppress Status.
	$tileEditor2D::confirmBrushOverwrites = %status;
	
	// Check Collision Draw Flag.
	tileScreenMenu.setMenuItemChecked("Brush", "Confirm Brush Overwrites", $tileEditor2D::confirmBrushOverwrites );
}


//-----------------------------------------------------------------------------
// Load Brushes.
//-----------------------------------------------------------------------------
function loadBrushes( %brushFile )
{
	// Open File Object.
	%file = new FileObject();
	// Open Brush File.
	%file.openForRead( ExpandFilename(%brushFile) );

	// Sanity!
	if ( %file.readLine() $= "T2D_BRUSH_START" )
	{	
		// Check Version.
		if ( %file.readLine() != $brushVersion )
		{
			// Confirm.
			MessageBoxOK("Load Brush Error", "Brush-File Version is incompatible!", "");
			// Close File.
			%file.close();
			%file.delete();
			// Finish Here.
			return;
		}
		
		// Read Brushe Count.
		%brushCount = %file.readLine();

		// Check we've got enough brushes.
		if ( %brushCount > $maxBrushes )
		{
			// Confirm.
			MessageBoxOK("Load Brush Error", "Too many brushes in Brush-File! ", "");
			// Close File.
			%file.close();
			%file.delete();
			// Finish Here.
			return;
		}
		
		// Read Brushes.
		for ( %n = 1; %n <= %brushCount; %n++ )
		{
			// Reset Brush.
			resetBrush( %n );
			
			// Fetch Brush.
			%brush = $brush[%n];
			
			// Read Brush.
			%brush.active	= %file.readLine();
			%brush.name		= %file.readLine();
			%brush.width	= %file.readLine();
			%brush.height	= %file.readLine();
	
			// Is the brush active?
			if ( %brush.active )
			{
				// Yes, so read Brush Defs.
				for ( %x = 0; %x < %brush.width; %x++ )
				{
					for ( %y = 0; %y < %brush.height; %y++ )
					{
						// Create Single Tile-Def.
						%brush.tileDef[%x,%y] = new ScriptObject() { class = tileMapBrushDef; };
						
						// Fetch Tile-Def.
						%tileDef = %brush.tileDef[%x,%y];
						
						// Read Tile-Def.
						%tileDef.active				= %file.readLine();
						
						// Is the Tile-Def Active?
						if ( %tileDef.active )
						{
							// Yes, so read Tile-Def.
							%tileDef.collisionActive	= %file.readLine();
							%tileDef.collisionPoly		= %file.readLine();
							%tileDef.collisionDef		= %file.readLine();
							%tileDef.collisionScale		= %file.readLine();
							%tileDef.flipX				= %file.readLine();
							%tileDef.flipY				= %file.readLine();
							%tileDef.script				= %file.readLine();
							%tileDef.customData			= %file.readLine();
							%tileDef.type				= %file.readLine();
							%tileDef.name				= %file.readLine();
							%tileDef.frame				= %file.readLine();
						}
					}
				}
			}
		}

		// Sanity!
		if ( trim(%file.readLine()) !$= "T2D_BRUSH_END" )
		{
			// Warning.
			warn( "TileEditor - Invalid Brush Footer for [" @ %brushFile @ "]!" );
			warn( "TileEditor - Reseting Brushes!" );
			
			// Initialise Brushes.
			initialiseBrushes();
		}
	}
	else
	{
		// Warning.
		warn( "Invalid Brush Header for [" @ %brushFile @ "]!" );
	}

	// Close File.
	%file.close();
	%file.delete();
	
	// Read Brush Selection.
	readBrushSelection();

	// Select Active Brush.
	selectActiveBrush( 1 );	
}


//-----------------------------------------------------------------------------
// Save Brushes.
//-----------------------------------------------------------------------------
function saveBrushes( %brushFile )
{
	// Open File Object.
	%file = new FileObject();
	// Open Brush File.
	%file.openForWrite( ExpandFilename(%brushFile) );
	
	// Write Header.
	%file.writeLine( "T2D_BRUSH_START" );
	
	// Write Version
	%file.writeLine( $brushVersion );
	
	// Write Brush Count.
	%file.writeLine( $maxBrushes );
	
	// Write All Brushes.
	for ( %n = 1; %n <= $maxBrushes; %n++ )
	{
		// Fetch Brush.
		%brush = $brush[%n];
		
		// Write Brush.
		%file.writeLine( %brush.active );
		%file.writeLine( %brush.name );
		%file.writeLine( %brush.width );
		%file.writeLine( %brush.height );
		
		// Is the brush active?
		if ( %brush.active )
		{
			// Yes, so write Brush Defs.
			for ( %x = 0; %x < %brush.width; %x++ )
			{
				for ( %y = 0; %y < %brush.height; %y++ )
				{
					// Fetch Tile-Def.
					%tileDef = %brush.tileDef[%x,%y];
					// Write Tile-Def.
					%file.writeLine( %tileDef.active );

					// Is the Tile-Def Active?					
					if ( %tileDef.active )
					{
						// Yes, so write Tile-Def.
						%file.writeLine( %tileDef.collisionActive );
						%file.writeLine( %tileDef.collisionPoly );
						%file.writeLine( %tileDef.collisionDef );
						%file.writeLine( %tileDef.collisionScale );
						%file.writeLine( %tileDef.flipX );
						%file.writeLine( %tileDef.flipY );
						%file.writeLine( %tileDef.script );
						%file.writeLine( %tileDef.customData );
						%file.writeLine( %tileDef.type );
						%file.writeLine( %tileDef.name );
						%file.writeLine( %tileDef.frame );
					}
				}
			}
		}
	}
	
	// Write Footer.
	%file.writeLine( "T2D_BRUSH_END" );
	
	// Close File.
	%file.close();
	%file.delete();
	
	// Confirm.
	MessageBoxOK("Confirmation", "All Brushes have been saved.", "");
}



