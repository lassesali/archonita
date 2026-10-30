//-----------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

function initializeGuiEditor()
{
   echo( "\n--------- Initializing GUI Editor ---------" );
   
   // Load Client Scripts.
   exec("./Gui/guiEditor.ed.gui");
   exec("./Gui/newGuiDialog.ed.gui");
   exec("./Gui/guiEditor.ed.cs");
}

function destroyGuiEditor()
{
}
