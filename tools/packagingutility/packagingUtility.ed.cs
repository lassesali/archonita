//-----------------------------------------------------------------------------
// Torque2D Packaging Utility
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Packaging Utility State.
//-----------------------------------------------------------------------------
$packagingUtilityActive = false;

//-----------------------------------------------------------------------------
// Toggle Packaging Utility.
//-----------------------------------------------------------------------------
function togglePackagingUtility()
{
	// Ignore if we've not got a packaging-utility GUI.
	if ( !isObject( packagingUtilityDlg ) )
		return;
	
	// Toggle Packaging-Utility. 	
	if ( $packagingUtilityActive )
		hidePackagingUtility();
	else
		showPackagingUtility();
}

//-----------------------------------------------------------------------------
// Show Packaging Utility
//-----------------------------------------------------------------------------
function showPackagingUtility()
{
   // Put up report window
   if( !isObject( packagingUtilityDlg ) )
   {
      error("Packaging Utility Dialog Does Not Exist!");
      return;
   }

   Canvas.pushDialog( packagingUtilityDlg );
   packagingBuildStack.clear();
   
   setGuiProgress( 0 );
   setGuiText( "" );
   
   targetPlatformDropDown.clear();
   targetPlatformDropDown.add( "Windows", 0 );
   targetPlatformDropDown.add( "Mac OSX", 1 );
//   targetPlatformDropDown.add( "Linux", 2 );
   
   // Set the default as the current platform
   switch$( $platform )
   {
      case "windows":
         targetPlatformDropDown.setSelected( 0 );
      case "linux":
         MessageBoxOk("Functionality Coming Soon","Linux Packaging Is not yet supported.\r\rComing Soon!");
         hidePackagingUtility();
         return;
         //   TBD Soon - JDD.
         //targetPlatformDropDown.setSelected( 2 );
      case "macos":
         targetPlatformDropDown.setSelected( 1 );
   }
   
   %buildFile = $currentProject @ "/managed/build.cfg";
   if(isFile(%buildFile))
      loadBuildSet(%buildFile);

   // Set State.
   $packagingUtilityActive = true;
}

//-----------------------------------------------------------------------------
// Hide Packaging Utility
//-----------------------------------------------------------------------------
function hidePackagingUtility()
{
   // Hide.
   Canvas.popDialog( packagingUtilityDlg );

   // Set State.
   $packagingUtilityActive = false;

}


//-----------------------------------------------------------------------------
// Packaging Utility Control Profiles
//-----------------------------------------------------------------------------
if( !isObject( GuiHeaderTextProfile ) ) new GuiControlProfile ( GuiHeaderTextProfile: GuiTextProfile )
{
   fontType = "Arial";
   fontSize = 16;
   fontColor = "64 64 64";
};

if( !isObject( GuiPackagingButtonProfile ) ) new GuiControlProfile ( GuiPackagingButtonProfile: GuiButtonProfile )
{
   fontType = "Arial";
   fontSize = 16;
   fontColor = "64 64 64";
};

if( !isObject( GuiPackagingProgressProfile ) ) new GuiControlProfile ( GuiPackagingProgressProfile )
{
   border         = true;
   borderColor    = "40 40 40 100";
   fillColor      = "240 240 240";
   fontType       = "Arial";
   fontSize       = 14;
   justify        = "center";
   fontColor      = "20 20 20";
};

function isFileInIgnoreDir( %filename )
{
   for( %i = 0; %i < getWordCount( $Packaging::Options::IgnoreDir ); %i++ )
   {
      if( strstr( %filename, getWord( $Packaging::Options::IgnoreDir, %i ) ) != -1 )
      {
         return true;
      }
   }
   
   return false;
}

//-----------------------------------------------------------------------------

function isFileIgnored( %filename )
{
   for( %i = 0; %i < getWordCount( $Packaging::Options::IgnoreFiles ); %i++ )
   {
      if( strstr( fileExt( %filename ), getWord( $Packaging::Options::IgnoreFiles, %i ) ) != -1 )
      {
         return true;
      }
   }
   
   for( %i = 0; %i < getWordCount( $Packaging::Options::IgnoreFileNames ); %i++ )
   {
      if( strstr( fileName( %filename ), getWord( $Packaging::Options::IgnoreFileNames, %i ) ) != -1 )
      {
         return true;
      }
   }
   
   return false;
}

//-----------------------------------------------------------------------------

function preAnalyzeBuild()
{
   if (isDebugBuild())
      MessageBoxOKCancel("Warning", "You are trying to package a debug build. While this is " @
                         "possible, it may take a considerable amount of time. Continue?", "analyzeBuild();");
   else if (dbgIsConnected())
      MessageBoxOKCancel("Warning", "You are trying to package a build with a debugger attached. While this is " @
                         "possible, your game may appear to freeze for a long period of time while packaging." @
                         "This is because of the extra debugger overhead. Continue?", "analyzeBuild();");                       
   else
      analyzeBuild();
}

function analyzeBuild()
{
   $Packaging::MainGame = $currentProject;

   //%i = 0;
   //%modIter = GetWord( %modCount, %i );
   //%gameDirs = "";
   //while( %modIter !$= "" )
   //{
      //%gameName = packagingGameList.getItemText( %modIter );
      //if( %gameName !$= "" && %gameName !$= $Packaging::MainGame )
         //%gameDirs = %gameDirs @ %gameName @ " ";
      //%i++;   
      //%modIter = GetWord( %modCount, %i );
   //}

   $Packaging::gameDirs = %gameDirs;

   %gameDirs = %gameDirs @ $Packaging::MainGame;

   // Set the build type
   switch( targetPlatformDropDown.getSelected() )
   {
      case 0:
         $Packaging::buildPlatform = "windows";
      case 1:
         $Packaging::buildPlatform = "macos";
      case 2:
         $Packaging::buildPlatform = "linux";
      default:
         MessageBoxOK("Analyze Build Package", "Invalid Target Platform Specified", "");
         return;
   }      
   
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
   
   // The three results for files that are processed
   $Packaging::BuildList = createFileRefList();
   $Packaging::IgnoreList = createFileRefList();
   $Packaging::UnknownList = createFileRefList();
   $Packaging::RemoveList = createFileRefList();
   
   for( %j = 0; %j < GetWordCount(%gameDirs); %j++ )
   {
      %currDir = GetWord( %gameDirs, %j );
      
      // Build the scan path
      %path = "*";
      if( %currDir !$= "" )
         %path = %currDir @ "/*";
      
      %fileCount = getFileCount( %path );
      setGuiText( "Scanning: " @ %path );
            
      // Scan through all the files!
      for( %file = findFirstFile( %path ); %file !$= ""; %file = findNextFile( %path ) )
      {
         //echo("Scanning " @ %file );
         setGuiProgress( %curFileNum++ / %fileCount );
         
         // Check to see if this file we want to ignore
         if( isFileInIgnoreDir( %file ) || isFileIgnored( %file ) )
         {
            %temp = createFileRefObject( %file );
            $Packaging::IgnoreList.add( %temp );
            continue;
         }
         
         // Now determine what action should be taken with this file
         %fileHandled = false;
         
         for( %i = 0; %i < getWordCount( $Packaging::FileHandlerList ); %i++ )
         {
            eval( "%ret = " @ getWord( $Packaging::FileHandlerList, %i ) @ "::isHandled( \"" @ %file @ "\" );" );
            
            // Check to see if we can find a handler for this file type
            if( %ret )
            {
               // Add to the list of files that are in the build
               %temp = createFileRefObject( %file );
               $Packaging::BuildList.add( %temp );
               
               %fileHandled = true;
               break;
            }
         }
         
         // Hmm, couldn't figure out what to do with this one...
         if( !%fileHandled )
         { 
            // Add to a list of unknowns
            %temp = createFileRefObject( %file );
            $Packaging::UnknownList.add( %temp );
         }
      }
   }
   // Now here we do post-processing on the build list to see if there's things we can cut out
   // Or things we need to add
   if( !%skipPostProcess )
   {
      for( %i = 0; %i < getWordCount( $Packaging::FileHandlerList ); %i++ )
      {
         setGuiText( "" );
         setGuiProgress( 0.0 );
         eval( getWord( $Packaging::FileHandlerList, %i ) @ "::postProcessBuildList( " @ $Packaging::BuildList @ ", " @ $Packaging::RemoveList @ " );" );
      }
   }
   
   // Present the GUI report
   if( !%skipGUIReport )
   {
      packagingBuildStack.clear();
      reportFileList( $Packaging::BuildList, "Files in Build", false, true );
      reportFileList( $Packaging::RemoveList, "Pruned Files", false );
      reportFileList( $Packaging::UnknownList, "Unknown files", true );
      reportFileList( $Packaging::IgnoreList, "Ignored files", true );
   }
   
   setGuiText( "Analysis Complete." );
   setGuiProgress( 100.0 );

}

//-----------------------------------------------------------------------------

function processUserBuildModification()
{
   postProcessGUI( $Packaging::BuildList, $Packaging::RemoveList );
   doFinalBuild();
}

//-----------------------------------------------------------------------------

function cleanUpBuild()
{
   cancelPackaging();
    
   // Clean up
   if (isObject($packaging::BuildList))
      $packaging::BuildList.delete();
   if (isObject($packaging::RemoveList))
      $packaging::RemoveList.delete();
   if (isObject($packaging::UnknownList))
      $packaging::UnknownList.delete();
   if (isObject($packaging::IgnoreList))
      $packaging::IgnoreList.delete();

   // Ok this is is wild, I don't know why in the hell this happens, but it does
   // it will crash if we don't do this. I think there is some subtle bug in
   // SimGroup/SimSet that nobody has tracked down yet because occasionally weird
   // stuff like this happens.
   //$Packaging::BuildList.add( new SimObject() );
   //$Packaging::BuildList.delete();
}

//-----------------------------------------------------------------------------

function doFinalBuild()
{

   if( outDirectory.getText() $= "" )
   {
      MessageBoxOK("Error!","You must specify an output directory!","");
      return;
   }
   
                          
   Canvas.pushDialog( packagingStatusDlg );
   statusOkButton.setActive(false);
   packagingStatusList.clear();
   
   // Finally, do the actual build
   %errors = false;
   
   for( %i = 0; %i < getWordCount( $Packaging::FileHandlerList ); %i++ )
   {
      eval( "%result = " @ getWord( $Packaging::FileHandlerList, %i ) @ "::processFiles( " @ $Packaging::BuildList @ " );" );
      
      if( %result == -1 )
      {
         // Show some error thingy
         MessageBoxOk( "Critical error.","An Error was encountered that the packager cannot recover from. Build aborted.", "" );
         return;
      }
      else if( %result == 1 )
      {
         // Do some kind of console log consumer here to log errors
         addPackagingStatus("Error while packaging, unable to complete packaging!");
         %errors |= true;
      }
   }
   
   // If there are errors, stop the build here
   if( %errors )
   {
      MessageBoxOk( "Critical error.","An Error was encountered that the packager cannot recover from. Build aborted.", "" );
      // Display the error report screen thingy
      return;
   }
   
   // Copy the files to a new directory
   %baseBuildDir = outDirectory.getText();
   
   // Create the build directory
   createPath(%baseBuildDir @ "/");
   
   // Copy the executable.
   %buildDir = copyExecutableAndReturnNewBuildDir(%baseBuildDir);
   
   // Copy all the build files
   for( %i = 0; %i < $Packaging::BuildList.getCount(); %i++ )
   {
      %file = $Packaging::BuildList.getObject( %i );
      
      %newFileName = %buildDir @ "/" @ %file.getFileName();
      
      // Create the directory path to the file.
      createPath( %newFileName );

      addPackagingStatus("Copying File : " @ %file.getFileName());

      // Skip 0 Byte files, pathCopy will fail on them      
      if( fileSize( %file.getFileName() ) == 0 || !isFile( %file.getFileName() ) )
      {
         addPackagingStatus("Skipping File of 0 Size : " @ %file.getFileName());
      }
      else if( ! pathCopy( %file.getFileName(), %newFileName ) )
      {
         addPackagingStatus("Failed copying file : " @ %file.getFileName());
         MessageBoxOk( "Critical error.","An Error was encountered that the packager cannot recover from. Build aborted.", "" );
         return;
      }
   }
   
   
   // Copy over resources that this project uses.
   if( isObject( $dependentResourceGroup ) && $dependentResourceGroup.getCount() > 0 )
   {
      for( %i = 0; %i < $dependentResourceGroup.getCount(); %i++ )
      {
         %resourceLink = $dependentResourceGroup.getObject( %i );
         if( !isObject( %resourceLink ) )
            continue;
         
         // Resource Name Exists
         if( %resourceLink.Name !$= "" )
         {
            addPackagingStatus("Copying Resource : " @ %resourceLink.name );
            // Generate resource file path
            %resourceFile = "resources/" @ %resourceLink.Name @ "/resourceDatabase.cs";
            if( !isFile( %resourceFile ) )
               %resourceFile = $currentProject @ "/resources/" @ %resourceLink.Name @ "/resourceDatabase.cs";
            
            if( isFile( %resourceFile ) )
            {
               // copy over resource
               %destResPath = %buildDir  @ "/resources/" @ %resourceLink.Name;
               createPath( %destResPath );
               %srcResPath = filePath( %resourceFile );
               if( ! pathCopy( %srcResPath, %destResPath ) )
               {
                  addPackagingStatus("Failed Copying Resource : " @ %resourceLink.Name );
                  MessageBoxOk( "Critical error.","An Error was encountered that the packager cannot recover from. Build aborted.", "" );
                  return;
               }
            }
         }
      }
   }   
   
   addPackagingStatus("Creating Packaging HTML Report");
   // And now do the HTML report here
   %html = createHTMLReport( %baseBuildDir @ "/buildLog.html", "Build Log for: " @ $Packaging::MainGame );
   %html.writeFileList( $Packaging::BuildList, "Files in Build" );
   %html.writeFileList( $Packaging::RemoveList, "Pruned Files" );
   %html.writeFileList( $Packaging::UnknownList, "Unknown files" );
   %html.writeFileList( $Packaging::IgnoreList, "Ignored files" );
   %html.close();
   
   gotoWebPage( "file://" @ getWorkingDirectory() @ "/" @ %baseBuildDir @ "/buildLog.html" );
   
   cleanUpBuild();
   
   // Copy common directory, do this AFTER CLEANUP
   copyCommonToDir( %buildDir );
}

//-----------------------------------------------------------------------------

function copyCommonToDir( %buildDir )
{
   %CommonBuildList = new SimGroup();
   
   %path = "common/*";
   
   %fileCount = getFileCount( %path );
   
   // Get the number of mods we're packaging ( + 2 for common  and main mod)
   %modCount = getWordCount( $Packaging::gameDirs ) + 2;
   // Replace the spaces with semicolons to separate mods
   %modDirs = strreplace( $Packaging::GameDirs, " ", ";" );
   // Update GUI
   setGuiText( "Scanning: " @ %path );
   
   for( %file = findFirstFile( %path ); %file !$= ""; %file = findNextFile( %path ) )
   {
      setGuiProgress( %curFileNum++ / %fileCount );
      
      // Check to see if this file we want to ignore
      if( isFileInIgnoreDir( %file ) || isFileIgnored( %file ) )
         continue;

      
      for( %i = 0; %i < getWordCount( $Packaging::FileHandlerList ); %i++ )
      {
         eval( "%ret = " @ getWord( $Packaging::FileHandlerList, %i ) @ "::isHandled( \"" @ %file @ "\" );" );
         
         // Check to see if we can find a handler for this file type
         if( %ret )
         {
            // Add to the list of files that are in the build
            %temp = createFileRefObject( %file );
            %CommonBuildList.add( %temp );
            break;
         }
      }
   }
   
   // No post-processing for common
   
   // Finally, do the actual build
   %errors = false;
   
   for( %i = 0; %i < getWordCount( $Packaging::FileHandlerList ); %i++ )
   {
      eval( "%result = " @ getWord( $Packaging::FileHandlerList, %i ) @ "::processFiles( " @ %CommonBuildList @ " );" );
      
      if( %result == -1 )
      {
         // Show some error thingy
         error( "Critical error. Build aborted." );
         return;
      }
      else if( %result == 1 )
      {
         // Do some kind of console log consumer here to log errors
         error( "There were build errors. Well crap." );
         %errors |= true;
      }
   }
   
   // If there are errors, stop the build here
   if( %errors )
   {
      // Display the error report screen thingy
      return;
   }
   
   // Copy all the build files
   for( %i = 0; %i < %CommonBuildList.getCount(); %i++ )
   {
      %file = %CommonBuildList.getObject( %i );
      
      %newFileName = %buildDir @ "/" @ %file.getFileName();
      
      // Ceate the directory that will contain the new file
      createPath(%newFileName);
      
      addPackagingStatus("Copying File : " @ %file.getFileName() );
      
      // TODO: CHECK FOR COPY ERRORS!!
      pathCopy( %file.getFileName(), %newFileName );
   }
   
   // Copy over main.cs
   addPackagingStatus("Copying Main.cs");
   %fo = new FileObject();
   %fo.openForWrite( %buildDir @ "/main.cs" );
   
   %fo2 = new FileObject();
   %fo2.openForRead( "tools/packagingutility/main_template.cs" );
  
   // Do the copy line by line so we can replace macros
   while( !%fo2.isEOF() )
   {
      %line = %fo2.readLine();
      %line = strreplace( %line, "<MODS>", %modDirs );
      %line = strreplace( %line, "<MAINGAME>", $Packaging::MainGame );
      %line = strreplace( %line, "<CURRENTLEVEL>", $levelEditor::LastLevel[$currentProject] );
      %fo.writeLine( %line );
   }
   
   // Close the files
   %fo.close();
   %fo2.close();
   
   // Delete the file objects
   %fo.delete();
   %fo2.delete();
   
   addPackagingStatus("Packaging Complete!");
   statusOkButton.setActive(true);
}

function copyExecutableAndReturnNewBuildDir(%buildDir)
{
   // win & lin won't need to modify the build dir.
   %newBuildDir=%buildDir;
   
   addPackagingStatus("Copying Executable and dependencies");
   switch$( $Packaging::buildPlatform )
   {
      case "windows":
         // Copy over required dependencies
         pathCopy( "/OpenAL32.dll", %buildDir @ "/OpenAL32.dll" );
         pathCopy( "/" @ getExecutableName(), %buildDir @ "/" @ $Packaging::MainGame @ ".exe" );
         if( isDebugBuild() )
         {
            pathCopy( "/glu2d3d_Debug.dll", %buildDir @ "/glu2d3d_Debug.dll" );
            pathCopy( "/opengl2d3d_Debug.dll", %buildDir @ "/opengl2d3d_Debug.dll" );          
         }
         else
         {
            pathCopy( "/glu2d3d.dll", %buildDir @ "/glu2d3d.dll" );
            pathCopy( "/opengl2d3d.dll", %buildDir @ "/opengl2d3d.dll" );          
         }        
      case "linux":
         pathCopy( "/" @ getExecutableName(), %buildDir @ "/" @ $Packaging::MainGame );
      case "macos":
         %appPackage = %buildDir @ "/" @ $Packaging::MainGame @ ".app";
         %execName = strreplace(getExecutableName(), ".app", "");
         %pathToExec = %appPackage @ "/Contents/MacOS/";

         // copy the .app
         addPackagingStatus("Copying the .app bundle");
         pathCopy( getExecutableName(), %appPackage );

         // rename the executable
         addPackagingStatus("Renaming the executable");
         pathCopy( %pathToExec @ %execName, %pathToExec @ $Packaging::MainGame);
         fileDelete( %pathToExec @ %execName );

         // Replace Info.plist with our processed plist template
         addPackagingStatus("Processing Info.plist entries...");
         %templ = new FileObject();
         %plist = new FileObject();
         %templ.openForRead("./info_template.plist");
         %plist.openForWrite( %appPackage @ "/Contents/Info.plist");        

         while( !%templ.isEOF() )
         {
            %line = %templ.readLine();
            %line = strreplace( %line, "<MAINGAME>", $Packaging::MainGame );
            %plist.writeLine( %line );
         }
         %templ.close();
         %plist.close();
         %templ.delete();
         %plist.delete();
         // now modify build dir to put content files inside the bundle
         %newBuildDir = %appPackage @ "/Contents/Resources/";
   }
   return %newBuildDir;
}

//-----------------------------------------------------------------------------
// this function will trigger when you select a main game, it will add all the mods
// besides the game you selected to the mod selection list
//-----------------------------------------------------------------------------
function packagingMainGameMenu::onSelect(%this, %id, %text)
{
   // first lets clear the mod list
   packagingGameList.clearItems();

   // we loop through all of the mods stored
   for(%i=0;%i<%this.modCount;%i++)
   {
      // lets grab the mod
      %mod = %this.mods[%i];

      // add the mod to the mod list
      packagingGameList.addItem(%mod);
   }
}