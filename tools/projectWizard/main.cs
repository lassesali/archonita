//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

$templatesDirectory = "tools/projectWizard/templates";
$newProjectTemplate["Empty Game"] = $templatesDirectory @ "/base";
$newProjectTemplate["Empty Game - No LevelBuilder"] = $templatesDirectory @ "/notools";
$newProjectTemplate["Fish"] = $templatesDirectory @ "/Fish";

function initializeProjectWizard()
{
   exec("./gui/newProjectDlg.ed.gui");
   exec("./gui/openProjectDlg.ed.gui");
   exec("./gui/projectOptionsDlg.ed.gui");
   exec("./gui/projectOptionsDlg.ed.cs");
}

function destroyProjectWizard()
{
}

function newProject()
{
   Canvas.pushDialog(NewProjectDlg);
   
   templateListBox.clear();
   templateListBox.add("Empty Game", 0);
   templateListBox.add("Empty Game - No LevelBuilder", 1);
   templateListBox.add("Fish", 2);
   templateListBox.setSelected( 0 );
}

// Return true/false
function findProject( %name ) 
{
   %projects = getDirectoryList( "/", 1 );
   %projectCount = getWordCount( %projects );
   
   for( %i = 0; %i < %projectCount; %i++ )
   {
      if( getWord( %projects, %i ) $= %name )
         return true;
   }
   
   return false;
}

function createNewProject(%name, %template)
{
   // Names must be only one word.
   if( getWordCount(%name) > 1 )
   {
      MessageBoxOK("Error Creating Project", "Project names cannot contain spaces" );
      return;
   }
   
   // The name must not already be a project in the system.
   if( findProject( %name ) == true )
   {
      MessageBoxOK("Error Creating Project", "A Project with the name" SPC %name SPC "already exists.\n\n Please choose a different name" );
      return;
   }
   
   // The name must not be empty
   if( %name $= "")
   {
      MessageBoxOK("Error Creating Project", "Please choose a name" );
      return;
   }   
   
   // Pop this dialog
   Canvas.popDialog(NewProjectDlg);
      
   // Create Template
   if (pathCopy($newProjectTemplate[%template], %name, true))
      LBProjectObj.open( %name ); 
   
}

function openProject()
{
   Canvas.pushDialog(OpenProjectDlg);
   
   projectList.clearItems();
   %modList = getDirectoryList("");
   
   %count = getWordCount(%modList);
   for (%i = 0; %i < %count; %i++)
   {
      %path = getWord(%modList, %i);

      if (isValidProject(%path) && (%path !$= $currentProject))
         projectList.addItem(%path);
   }
}

function isValidProject(%project)
{
   // Should be more robust check.
   return isFile(%project @ "/managed/project.t2d");
}
