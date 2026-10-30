//-----------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

function initializeImageEditor()
{   
   echo( "\n--------- Initializing Image Editor ---------" );

   exec("./data/content/datablocks.cs");

   exec("./scripts/editor.ed.cs");
   exec("./scripts/editorSettings.ed.cs");
   exec("./scripts/editorPreviewWindow.ed.cs");
   
   exec("./gui/OpenFileDlgExwPreview.ed.gui");
   exec("./gui/ImageBuilderGui.ed.gui");
}

function destroyImageEditor()
{
   // Export Preferences.
   echo("Exporting Image Editor preferences.");
   export("$imageEditor::*", "./preferences/prefs.cs", false);
}
