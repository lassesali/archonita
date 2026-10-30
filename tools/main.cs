//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

//---------------------------------------------------------------------------------------------
// Space separated list of editors to load.
//---------------------------------------------------------------------------------------------
$editors = "editorClasses debugger guiEditor packagingUtility particleEditor tileEditor leveleditor projectWizard animationEditor imageEditor";

//---------------------------------------------------------------------------------------------
// Tools Package.
//---------------------------------------------------------------------------------------------
package Tools
{
   function loadKeybindings()
   {
      Parent::loadKeybindings();
      
      if (isObject(levelEditorMap))
         return;
         
      new ActionMap(levelEditorMap);
      levelEditorMap.bind(keyboard, "lbracket", layerSelectionDown, "Move Selection Down One Layer");
      levelEditorMap.bind(keyboard, "rbracket", layerSelectionUp, "Move Selection Up One Layer");
      
      levelEditorMap.bind(keyboard, "escape", levelBuilderSetSelectionTool, "Activate the Selection Tool");
      
      levelEditorMap.bind(keyboard, "e", levelBuilderSetEditPanel, "Show the Edit Panel");
      levelEditorMap.bind(keyboard, "c", levelBuilderSetCreatePanel, "Show the Create Panel");
      levelEditorMap.bind(keyboard, "p", levelBuilderSetProjectPanel, "Show the Project Panel");
      
      levelEditorMap.bind(keyboard, "home", levelBuilderHomeView, "Home the View");
      levelEditorMap.bind(keyboard, "end", levelBuilderZoomToFit, "Show All Level Contents");
      levelEditorMap.bind(keyboard, "equals", levelBuilderZoomViewIn, "Zoom the View Window In");
      levelEditorMap.bind(keyboard, "minus", levelBuilderZoomViewOut, "Zoom the View Window Out");
      levelEditorMap.bind(keyboard, "left", levelBuilderViewLeft, "Move the View Window Left");
      levelEditorMap.bind(keyboard, "right", levelBuilderViewRight, "Move the View Window Right");
      levelEditorMap.bind(keyboard, "up", levelBuilderViewUp, "Move the View Window Up");
      levelEditorMap.bind(keyboard, "down", levelBuilderViewDown, "Move the View Window Down");
      levelEditorMap.bindCmd(keyboard, "F5", "runGame();", "");
      
      if($platform $= "macos")
      {
         levelEditorMap.bind(keyboard, "cmd z",       levelBuilderUndo, "Undo The Last Action");
         levelEditorMap.bind(keyboard, "cmd-shift z", levelBuilderRedo, "Redo an Undone Action");
         levelEditorMap.bind(keyboard, "cmd c",       levelBuilderCopy, "Copy the Selected Objects");
         levelEditorMap.bind(keyboard, "cmd x",       levelBuilderCut, "Cut the Selected Objects");
         levelEditorMap.bind(keyboard, "cmd v",       levelBuilderPaste, "Paste Cut or Copied Objects");
      }
      else
      {
         levelEditorMap.bind(keyboard, "ctrl z", levelBuilderUndo, "Undo The Last Action");
         levelEditorMap.bind(keyboard, "ctrl y", levelBuilderRedo, "Redo an Undone Action");
         levelEditorMap.bind(keyboard, "ctrl c", levelBuilderCopy, "Copy the Selected Objects");
         levelEditorMap.bind(keyboard, "ctrl x", levelBuilderCut, "Cut the Selected Objects");
         levelEditorMap.bind(keyboard, "ctrl v", levelBuilderPaste, "Paste Cut or Copied Objects");
      }

   }
   
   // Start-up.
   function onStart()
   {
      Parent::onStart();
      echo("\n--------- Initializing MOD: Tools ---------");
      
      // Common GUI stuff.
      exec("./gui/cursors.ed.cs");
      exec("./gui/profiles.ed.cs");
   
      $ignoredDatablockSet = new SimSet();
   
      %count = getWordCount($editors);
      for (%i = 0; %i < %count; %i++)
      {
         exec("./" @ getWord($editors, %i) @ "/main.cs");
         call("initialize" @ getWord($editors, %i));
      }

      // Make sure we get editor profiles before any GUI's
      exec("./gui/guiDialogs.ed.cs");

   }
   
   // Shutdown.
   function onExit()
   {
      GuiFormManager::SaveLayout(LevelBuilder, Default);
      
      %count = getWordCount($editors);
      for (%i = 0; %i < %count; %i++)
      {
         call("destroy" @ getWord($editors, %i));
      }
      
      // Export Preferences.
      echo("Exporting Gui preferences.");
      export("$Pref::FileDialogs::*", "./gui/preferences/prefs.cs", false);
	
      // Call Parent.
      Parent::onExit();
   }
};


//-----------------------------------------------------------------------------
// Activate Package.
//-----------------------------------------------------------------------------
activatePackage(Tools);

