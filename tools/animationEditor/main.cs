//-----------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

function initializeAnimationEditor()
{   
   echo( "\n--------- Initializing Animation Builder ---------" );
   
   exec("./scripts/animationBuilder.ed.cs");
   exec("./scripts/animationBuilderCanvas.ed.cs");
   exec("./scripts/ABEdit.ed.cs");
   exec("./scripts/ABScrollWindow.ed.cs");
   exec("./scripts/ABScrollBar.ed.cs");
   exec("./scripts/ABHover.ed.cs");
   exec("./scripts/ABArrow.ed.cs");
   exec("./scripts/ABStoryboard.ed.cs");
   exec("./scripts/ABStoryboardFrame.ed.cs");
   exec("./scripts/ABStoryboardArrow.ed.cs");
   exec("./scripts/ABFrameset.ed.cs");
   exec("./scripts/ABFramesetFrame.ed.cs");
   exec("./scripts/ABPreview.ed.cs");
   exec("./scripts/ABPreviewAnim.ed.cs");
   exec("./scripts/ABGuiScripts.ed.cs");
   exec("./scripts/ABStartup.ed.cs");
   exec("./scripts/ABPreviewList.ed.cs");
   exec("./scripts/ABFont.ed.cs");
   
   exec("./gui/AnimationEditorGUI.ed.gui");
   
   exec("./data/content/datablocks.ed.cs");
   
   // Initialize
   AnimationBuilder::Init();
}

function destroyAnimationEditor()
{
   // Export Preferences.
   echo("Exporting Animation Builder preferences.");
   export("$animationBuilder::*", "./preferences/prefs.cs", false);
   
   AnimationBuilder::Destroy();
}
