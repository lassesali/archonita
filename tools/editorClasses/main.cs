//-----------------------------------------------------------------------------
// Torque2D Editor Classes
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

function initializeEditorClasses()
{
   echo( "\n--------- Initializing Editor Classes ---------" );


   $EditorClassesGroup = "EditorClassesCleanup";


   if( !isObject( $EditorClassesGroup ) )
      new SimGroup( $EditorClassesGroup );


   //-----------------------------------------------------------------------------
   // Load Editor Profiles
   //-----------------------------------------------------------------------------
   exec("./gui/editorProfiles.ed.cs");

   //-----------------------------------------------------------------------------
   // Load Form Managers
   //-----------------------------------------------------------------------------
   exec("./scripts/guiFormLibraryManager.ed.cs");
   exec("./scripts/guiFormContentManager.ed.cs");
   exec("./scripts/guiFormReferenceManager.ed.cs");
   exec("./scripts/guiFormLayoutManager.ed.cs");
   exec("./scripts/guiFormMessageManager.ed.cs");
   exec("./scripts/scriptClassManager.ed.cs");
   exec("./scripts/contextDialogContainer.ed.cs");
   exec("./scripts/resourceFinder.ed.cs");
   exec("./scripts/debugOutput.ed.cs");
   
   // Form Class
   exec("./scripts/guiFormClass.ed.cs");
   
   exec("./scripts/RSSNews/RSSFeedScript.ed.cs");

   // Initialize.
   GuiFormManager::Init();


}

function destroyEditorClasses()
{

   // Destroy.
   GuiFormManager::Destroy();
}
