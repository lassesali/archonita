//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

function loadPackageFile()
{
   if (($currentProject !$= "") && isFile($currentProject @ "/managed/build.cfg"))
      loadBuildSet($currentProject @ "/managed/build.cfg");
   
   else
      getLoadFilename("~/packagingutility/builds/*", loadBuildSet);
}

function savePackageFile()
{
   if ($currentProject !$= "")
      saveBuildSet($currentProject @ "/managed/build.cfg");
   else
      getSaveFilename("~/packagingutility/builds/*", saveBuildSet);
}

//---------------------------------------------------------------------------------------------
// Saves a build configuration to a file.
//---------------------------------------------------------------------------------------------
function saveBuildSet(%filename)
{
   echo("Saving build configuration to " @ %filename);
   
   // Process any changes in the gui.
   postProcessGUI( $Packaging::BuildList, $Packaging::RemoveList );
   
   // Refresh file list.
   packagingBuildStack.clear();
   reportFileList( $Packaging::BuildList, "Files in Build", false, true );
   reportFileList( $Packaging::RemoveList, "Pruned Files", false );
   reportFileList( $Packaging::UnknownList, "Unknown files", true );
   reportFileList( $Packaging::IgnoreList, "Ignored files", true );
   
   // Create a sim group to be saved.
   new SimGroup(PackageConfiguration)
   {
      _game = $packaging::MainGame;
      _mods = $packaging::gameDirs;
      _outDir = outDirectory.getText();
      _platform  = $packaging::buildPlatform;
   };
   
   // Set names for the various groups for reloading ease.
   $packaging::BuildList._simGroup.setName(BuildList);
   $packaging::RemoveList._simGroup.setName(RemoveList);
   $packaging::UnknownList._simGroup.setName(UnknownList);
   $packaging::IgnoreList._simGroup.setName(IgnoreList);
   
   // Add to the main group.
   PackageConfiguration.add(BuildList);
   PackageConfiguration.add(RemoveList);
   PackageConfiguration.add(UnknownList);
   PackageConfiguration.add(IgnoreList);
   
   // And save.
   PackageConfiguration.save(%filename);
}

//---------------------------------------------------------------------------------------------
// Loads a build configuration from a file.
//---------------------------------------------------------------------------------------------
function loadBuildSet(%filename)
{
   echo("Loading build configuration from " @ %filename);
   // Clean up a potential previous analysis.
   packagingLoadCleanup();
   
   // Load up the saved stuff.
   exec(%filename);
   
   // Dynamically build the handler list
   $Packaging::FileHandlerList = "";
   
   %fileSpec = expandFilename( "tools/packagingUtility/" ) @ "*.cs" SPC expandFilename( "tools/packagingUtility/" ) @ "*.edso";
   for( %file = findFirstFileMultiExpr( %fileSpec ); %file !$= ""; %file = findNextFileMultiExpr( %fileSpec ) )
   {
      %loc = strstr( fileBase( %file ), "_Handler" );
      if( %loc != -1 )
      {
         exec( %file );
   
         $Packaging::FileHandlerList = $Packaging::FileHandlerList SPC getSubStr( fileBase( %file ), 0, %loc );
      }
   }
   
   $Packaging::FileHandlerList = trim( $Packaging::FileHandlerList );
   
   // Recreate the file ref lists.
   $Packaging::BuildList = createFileRefList();
   $Packaging::IgnoreList = createFileRefList();
   $Packaging::UnknownList = createFileRefList();
   $Packaging::RemoveList = createFileRefList();
   
   // Redirect the SimSets.
   $packaging::BuildList._simGroup = BuildList.getId();
   $packaging::RemoveList._simGroup = RemoveList.getId();
   $packaging::UnknownList._simGroup = UnknownList.getId();
   $packaging::IgnoreList._simGroup = IgnoreList.getId();
   
   // Load up configuration settings.
   $packaging::MainGame = PackageConfiguration._game;
   $packaging::gameDirs = PackageConfiguration._mods;
   $packaging::buildPlatform = PackageConfiguration._platform;
   
   // Set gui stuff.
   outDirectory.setText(PackageConfiguration._outDir);
   
   // Recreate the gui.
   packagingBuildStack.clear();
   reportFileList( $Packaging::BuildList, "Files in Build", false, true );
   reportFileList( $Packaging::RemoveList, "Pruned Files", false );
   reportFileList( $Packaging::UnknownList, "Unknown files", true );
   reportFileList( $Packaging::IgnoreList, "Ignored files", true );
}

function packagingLoadCleanup()
{
   if (isObject($packaging::BuildList))
      $packaging::BuildList.delete();
   if (isObject($packaging::RemoveList))
      $packaging::RemoveList.delete();
   if (isObject($packaging::UnknownList))
      $packaging::UnknownList.delete();
   if (isObject($packaging::IgnoreList))
      $packaging::IgnoreList.delete();
}
