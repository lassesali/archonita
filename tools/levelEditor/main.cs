//-----------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

$LevelBuilder::Version = "v1.0.0 [RC3]";

function initializeLevelEditor()
{   
   echo( "\n--------- Initializing Level Editor ---------" );
   
   // Load Preferences.
   exec("./preferences/defaultPrefs.ed.cs");
   exec("./preferences/prefs.cs");

   // Create Form Content Library.
   GuiFormManager::RegisterLibrary("LevelBuilder", "~/levelEditor/data/layouts/" );
   GuiFormManager::RegisterLibrary("LevelBuilderToolProperties", "~/levelEditor/data/layouts/" );
   GuiFormManager::RegisterLibrary("LevelBuilderSidebar", "~/levelEditor/data/layouts/sidebar/" );
   GuiFormManager::RegisterLibrary("LevelBuilderSidebarEdit", "~/levelEditor/data/layouts/sidebar/edit/" );
   GuiFormManager::RegisterLibrary("LevelBuilderSidebarCreate", "~/levelEditor/data/layouts/sidebar/create/" );
   GuiFormManager::RegisterLibrary("LevelBuilderQuickEditClasses" );

   // Load Up Available Layouts.
   GuiFormManager::InitLayouts( "LevelBuilder" );
   
   // Load scripts.
   exec("./gui/profiles.ed.cs");
   exec("./scripts/datablocks.ed.cs");
   exec("./scripts/projectManagement.ed.cs");
   exec("./scripts/levelEditor.ed.cs");
   exec("./scripts/levelEditorWindow.ed.cs");
   exec("./scripts/levelManagement.ed.cs");
   exec("./scripts/tools.ed.cs");
   exec("./scripts/options.ed.cs");
   exec("./scripts/edit.ed.cs");
   exec("./scripts/news.ed.cs");

   // Form Contents.
   %formFileSpec = expandFilename( "./scripts/forms/" ) @ "*.cs" SPC expandFilename( "./scripts/forms/" ) @ "*.edso";
   for (%file = findFirstFileMultiExpr(%formFileSpec); %file !$= ""; %file = findNextfileMultiExpr(%formFileSpec))
   {      
      if( !isFile( %file ) )
         continue;
         
      %dsoCheck = strstr( %file, ".edso" );
      if( %dsoCheck != -1 )
      {
         %csOriginal = strreplace( %file, ".edso", "" );
         if( !isFile( %csOriginal ) )
            exec( %csOriginal );
      }
      else
         exec( %file );
   }
   
   // Load guis.
   exec("./gui/LevelBuilderBase.ed.gui");
   exec("./gui/LBPlayLevel.ed.gui");
   exec("./gui/options.ed.gui");
   exec("./gui/RSSFeedGui.ed.gui");
     
   OptionsTabBook.add(LevelEditorOptionsPage);
   OptionsTabBook.selectPage(0);
   
   // Build LevelBuilder Menu
   initializeLevelBuilderMenu(levelBuilderMenu);

   // Create Tool Manager.
   if (!isObject(ToolManager))
   {
      new LevelBuilderSceneEdit( ToolManager );
      LevelBuilderToolManager::initializeTools( ToolManager );
   }
 
   applyOptions();

   // Activate Default Layout
   GuiFormManager::ActivateLayout( "LevelBuilder", "Default", levelBuilderViewContainer );
   
   // Set Creator Panel Content
   %contentObj = GuiFormManager::FindFormContent( "LevelBuilder", "SideBar" );
   if( %contentObj == 0 )
      GuiFormClass::ClearControlContent( sideBarContentContainer, "LevelBuilder" );
   else
      GuiFormClass::SetControlContent( sideBarContentContainer, "LevelBuilder", %contentObj );
      
   // Level editor hotkey.
   GlobalActionMap.bindCmd(keyboard, "f9", "toggleLevelEditor();", "");
   
   // Create the default config datablock. This is used to mask
   // the saving of unchanged scene object fields.
   setDefaultSceneObjectDatablock(new t2dSceneObjectDatablock());
   
   // Check for RSS News updates.
   RSSUpdate::initialize( "RSSUpdate::onNewNewsItems" );
         
}

function destroyLevelEditor()
{
   if (isObject(ToolManager.getLastWindow()))
      ToolManager.getLastWindow().endLevel();
   
   // Export Preferences.
   echo("Exporting Level Editor preferences.");
   export("$levelEditor::*", "./preferences/prefs.cs", false);
   setCanvasTitle("TGB");
   
   // Destroy RSS News Updater
   RSSUpdate::destroy();
}

function initializeLevelBuilderMenu( %menuBarObj )
{
   %menuBarObj.clearMenus();

   %menuCount = 0;
   
   //-----------------------------------------------------------------------------
   // File Menu
   //-----------------------------------------------------------------------------
   %menuBarObj.addMenu("File", %menuCount);
   
   // Command Subsection
   %menuBarObj.addMenuItem("File", "New Level...", 0);
   %menuBarObj.addMenuItem("File", "Open Level...", 5, "Ctrl O");
   
   // Divider
   %menuBarObj.addMenuItem("File", "-", 0);
   
   // Project Subsection
   %menuBarObj.addMenuItem("File", "New Project...", 9);
   %menuBarObj.addMenuItem("File", "Open Project...", 10);
   %menuBarObj.addMenuItem("File", "Build Project...", 11);   
   // Divider
   %menuBarObj.addMenuItem("File", "-", 0);
   
   // Saving Subsection
   %menuBarObj.addMenuItem("File", "Save", 1, "Ctrl S");
   %menuBarObj.addMenuItem("File", "Save As...", 2);
   %menuBarObj.addMenuItem("File", "Save Selected Group...", 3);
   %menuBarObj.addMenuItem("File", "Save Selected Objects...", 4);
   
   // Divider
   %menuBarObj.addMenuItem("File", "-", 0);
   
   %menuBarObj.addMenuItem("File", "Import...", 6);
   
   %menuBarObj.addMenuItem("File", "-", 0);
   
   %menuBarObj.addMenuItem("File", "Quit", 8);
   
   // File Menu Commands
   %menuBarObj.scriptCommand["File", 9] = "newProject();";
   %menuBarObj.scriptCommand["File", 10] = "openProject();";
   %menuBarObj.scriptCommand["File", 11] = "togglePackagingUtility();";
   %menuBarObj.scriptCommand["File", 0] = "newLevel();";
   %menuBarObj.scriptCommand["File", 1] = "saveLevel();";
   %menuBarObj.scriptCommand["File", 2] = "saveLevelAs();";
   %menuBarObj.scriptCommand["File", 3] = "saveSelectedGroup();";
   %menuBarObj.scriptCommand["File", 4] = "saveSelectedObjects();";
   %menuBarObj.scriptCommand["File", 5] = "getLevelOpenName(loadLevelCallback);";
   %menuBarObj.scriptCommand["File", 6] = "getLevelOpenName(addToLevelCallback);";
   %menuBarObj.scriptCommand["File", 8] = "quit();";

   // Increment Menu Count
   %menuCount++;

   //-----------------------------------------------------------------------------
   // Edit Menu
   //-----------------------------------------------------------------------------
   %menuBarObj.addMenu("Edit", %menuCount);
   %menuBarObj.addMenuItem("Edit", "Undo", 2);
   %menuBarObj.addMenuItem("Edit", "Redo", 3);
   %menuBarObj.addMenuItem("Edit", "-", 0);
   %menuBarObj.addMenuItem("Edit", "Cut", 4);
   %menuBarObj.addMenuItem("Edit", "Copy", 5);
   %menuBarObj.addMenuItem("Edit", "Paste", 6);
   %menuBarObj.addMenuItem("Edit", "-", 0);
   %menuBarObj.addMenuItem("Edit", "Group Objects", 0);
   %menuBarObj.addMenuItem("Edit", "Ungroup Objects", 1);
   %menuBarObj.addMenuItem("Edit", "-", 0);
   %menuBarObj.addMenuItem("Edit", "Send to back", 9, "Ctrl B");
   %menuBarObj.addMenuItem("Edit", "Bring to front", 10, "Ctrl F");
   %menuBarObj.addMenuItem("Edit", "-", 0);
   %menuBarObj.addMenuItem("Edit", "Preferences...", 8);

   // Edit Menu Commands
   %menuBarObj.scriptCommand["Edit", 0] = "ToolManager.groupObjects();";
   %menuBarObj.scriptCommand["Edit", 1] = "ToolManager.breakApart();";
   %menuBarObj.scriptCommand["Edit", 2] = "levelBuilderUndo(1);";
   %menuBarObj.scriptCommand["Edit", 3] = "levelBuilderRedo(1);";
   %menuBarObj.scriptCommand["Edit", 4] = "levelBuilderCut(1);";
   %menuBarObj.scriptCommand["Edit", 5] = "levelBuilderCopy(1);";
   %menuBarObj.scriptCommand["Edit", 6] = "levelBuilderPaste(1);";
   %menuBarObj.scriptCommand["Edit", 8] = "Canvas.pushDialog(optionsDlg);";
   %menuBarObj.scriptCommand["Edit", 9] = "sendToBack(true);";
   %menuBarObj.scriptCommand["Edit", 10] = "bringToFront(true);";
   
   // Increment Menu Count
   %menuCount++;

   //-----------------------------------------------------------------------------
   // Project Menu
   //-----------------------------------------------------------------------------
   %menuBarObj.addMenu("Project", %menuCount);
   %menuBarObj.addMenuItem("Project", "Run Game", 1);
   %menuBarObj.addMenuItem("Project", "-", 0);
   %menuBarObj.addMenuItem("Project", "GUI Builder...", 5);
   %menuBarObj.addMenuItem("Project", "Tile Map Builder...", 6);
   %menuBarObj.addMenuItem("Project", "-", 0);
   %menuBarObj.addMenuItem("Project", "Image Map Builder...", 7);
   %menuBarObj.addMenuItem("Project", "Animation Builder...", 9);
   %menuBarObj.addMenuItem("Project", "-", 0);
   %menuBarObj.addMenuItem("Project", "Refresh Object Library", 11);
   %menuBarObj.addMenuItem("Project", "Reload Textures", 12);
   %menuBarObj.addMenuItem("Project", "-", 0);
   %menuBarObj.addMenuItem("Project", "Resources...", 10, "Ctrl R");
   
   // Launch Menu Commands
   %menuBarObj.scriptCommand["Project", 1] = "runGame();";
   %menuBarObj.scriptCommand["Project", 2] = "runFishDemo();";
   %menuBarObj.scriptCommand["Project", 3] = "runSpaceScroller();";
   %menuBarObj.scriptCommand["Project", 4] = "runCheckersDemo();";
   %menuBarObj.scriptCommand["Project", 5] = "GuiEdit();";
   %menuBarObj.scriptCommand["Project", 6] = "toggleTileEditor();";
   %menuBarObj.scriptCommand["Project", 7] = "launchNewImageMap();";
   %menuBarObj.scriptCommand["Project", 8] = "launchNewLinkImageMap();";
   %menuBarObj.scriptCommand["Project", 9] = "launchAnimationEditor();";
   %menuBarObj.scriptCommand["Project", 10] = "Canvas.pushDialog(projectOptionsDlg);";
   %menuBarObj.scriptCommand["Project", 11] = "GuiFormManager::SendContentMessage($LBCreateSiderBar, %this, \"refreshall\");";
   %menuBarObj.scriptCommand["Project", 12] = "reloadImageMaps();";
   
   
   // Increment Menu Count
   %menuCount++;

   //-----------------------------------------------------------------------------
   // View Menu
   //-----------------------------------------------------------------------------
   %menuBarObj.addMenu("View", %menuCount);
   %menuBarObj.addMenuItem("View", "Home", 1);
   %menuBarObj.addMenuItem("View", "Show All", 2);
   %menuBarObj.addMenuItem("View", "Show Selected", 3);
   %menuBarObj.addMenuItem("View", "-", 0);
   %menuBarObj.addMenuItem("View", "Zoom 25%", 4);
   %menuBarObj.addMenuItem("View", "Zoom 50%", 5);
   %menuBarObj.addMenuItem("View", "Zoom 100%", 6);
   %menuBarObj.addMenuItem("View", "Zoom 200%", 7);
   %menuBarObj.addMenuItem("View", "Zoom 400%", 8);

   // Edit Menu Commands
   %menuBarObj.scriptCommand["View", 1] = "levelBuilderHomeView(1);";
   %menuBarObj.scriptCommand["View", 2] = "levelBuilderZoomToFit(1);";
   %menuBarObj.scriptCommand["View", 3] = "levelBuilderZoomToSelected(1);";
   %menuBarObj.scriptCommand["View", 4] = "levelBuilderZoomView(0.25);";
   %menuBarObj.scriptCommand["View", 5] = "levelBuilderZoomView(0.5);";
   %menuBarObj.scriptCommand["View", 6] = "levelBuilderZoomView(1.0);";
   %menuBarObj.scriptCommand["View", 7] = "levelBuilderZoomView(2.0);";
   %menuBarObj.scriptCommand["View", 8] = "levelBuilderZoomView(4.0);";
   
   // Increment Menu Count
   %menuCount++;

   //-----------------------------------------------------------------------------
   // Layout Menu
   //-----------------------------------------------------------------------------
   //%menuBarObj.addMenu("Layout", %menuCount);
   //%menuBarObj.addMenuItem("Layout", "Save Layout", 0);
   //%menuBarObj.addMenuItem("Layout", "Reload Layout", 1);
   // Edit Menu Commands
   //%menuBarObj.scriptCommand["Layout", 0] = "GuiFormManager::SaveLayout(LevelBuilder,Default);";
   //%menuBarObj.scriptCommand["Layout", 1] = "GuiFormManager::ReloadLayout(LevelBuilder,Default,levelBuilderViewContainer);";
   
   // Increment Menu Count
   //%menuCount++;

   //-----------------------------------------------------------------------------
   // Help Menu
   //-----------------------------------------------------------------------------
   %menuBarObj.addMenu("Help", %menuCount);
   %menuBarObj.addMenuItem("Help", "Documentation", 7);
   %menuBarObj.addMenuItem("Help", "TGB Reference", 5);
   %menuBarObj.addMenuItem("Help", "-", 0);
   %menuBarObj.addMenuItem("Help", "Keyboard Shortcuts...", 4);
   %menuBarObj.addMenuItem("Help", "-", 0);
   %menuBarObj.addMenuItem("Help", "Sign-up for TGB Announcements", 1);
   %menuBarObj.addMenuItem("Help", "-", 0);
   %menuBarObj.addMenuItem("Help", "GarageGames Community Page", 6);
   %menuBarObj.addMenuItem("Help", "TGB Developer Forums", 2);
   %menuBarObj.addMenuItem("Help", "GarageGames News", 3);
   %menuBarObj.addMenuItem("Help", "-", 0);
   %menuBarObj.addMenuItem("Help", "About...", 0);
   // Help Menu Commands
   %menuBarObj.scriptCommand["Help", 0] = "Canvas.pushDialog(TGBAboutDialog);";
   %menuBarObj.scriptCommand["Help", 1] = "gotoWebpage(\"http://www.garagegames.com/developer/torque/t2d/\");";
   %menuBarObj.scriptCommand["Help", 2] = "gotoWebpage(\"http://www.garagegames.com/mg/forums/result.area.php?qa=42\");";
   %menuBarObj.scriptCommand["Help", 3] = "RSSUpdate::showNews();";
   %menuBarObj.scriptCommand["Help", 4] = "Canvas.pushDialog(optionsdlg);OptionsTabBook.selectPage(1);";
   %menuBarObj.scriptCommand["Help", 7] = "gotowebpage(\"tools/help.html\");";
   %menuBarObj.scriptCommand["Help", 5] = "gotowebpage(\"tools/reference.html\");";
   %menuBarObj.scriptCommand["Help", 6] = "gotoWebpage(\"http://www.garagegames.com/mg/whatsnew/\");";
}

function GuiMenuBar::onMenuItemSelect(%this, %menuId, %menu, %itemId, %item)
{
   if(%this.scriptCommand[%menu, %itemId] !$= "")
      eval(%this.scriptCommand[%menu, %itemId]);
}

function runSpaceScroller()
{
   LBProjectObj.open( "spacescroller" );
}

function runFishDemo()
{
   LBProjectObj.open( "fishdemo" ); 
}

function runCheckersDemo()
{
   LBProjectObj.open( "checkersdemo" ); 
}

// --------------------------------------------------------------------
// Show Level Editor.
// --------------------------------------------------------------------

function showLevelEditor()
{
   endRunGame();
   
   $pref::startupProject = $currentProject;
   
   Canvas.setContent(LevelBuilderBase);
   setCanvasTitle( $currentProject @ " - TGB Level Builder" );
   
   applyOptions();
   levelEditorMap.push();
   
   // Set Level-Editor State.
   $LevelEditorActive = true;
   
   // Make sure the last level is both an existing file and in the current project.
   if (isFile($levelEditor::LastLevel[$currentProject]))
   {
      if (!loadLevelCallback($levelEditor::LastLevel[$currentProject]))
         newLevel();
   }
   else
      newLevel();
   
   ToolManager.clearAcquisition();
   GuiFormManager::SendContentMessage( $LBQuickEdit, %this, "inspect" SPC ToolManager.getLastWindow().getSceneGraph() );
   
   LevelBuilderToolManager::setTool(LevelEditorSelectionTool);
   
   if( !LBProjectObj.isActive() )
      LBProjectObj.open( $currentProject );
   
}

// --------------------------------------------------------------------
// Hide Level Editor.
// --------------------------------------------------------------------
function hideLevelEditor()
{
   GuiFormManager::SendContentMessage( $LBQuickEdit, 0, "clearCache" );
   
   if (isObject(ToolManager.getLastWindow()))
      ToolManager.getLastWindow().endLevel();
      
   Canvas.setContent(mainScreenGui);
   setCanvasTitle( $currentProject );
   
   // Set Level-Editor State.
   $LevelEditorActive = false;

   levelEditorMap.pop();
}

// --------------------------------------------------------------------
// Toggle Level Editor.
// --------------------------------------------------------------------
$LevelEditorActive = false;
function toggleLevelEditor()
{
   // Toggle Level-Editor.
   if ($LevelEditorActive)
      hideLevelEditor();
   else
      showLevelEditor();
}
