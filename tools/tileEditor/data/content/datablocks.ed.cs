//-----------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

// --------------------------------------------------------------------
// Datablocks.
// --------------------------------------------------------------------

// Tile-Cursor.
datablock t2dImageMapDatablock(tileEditorCursorImageMap)
{
   filterPad = false;
   imageName = "~/TileEditor/Data/Images/tilecursor";
   imageMode = full;
};

// Tile Icons.
datablock t2dImageMapDatablock(tileEditorTileIconsImageMap)
{
   imageName = "~/TileEditor/Data/Images/tileicons";
   imageMode = cell;
   cellWidth = 32;
   cellHeight = 32;
   tileCountX = 2;
};

$ignoredDatablockSet.add(tileEditorCursorImageMap);
$ignoredDatablockSet.add(tileEditorTileIconsImageMap);
