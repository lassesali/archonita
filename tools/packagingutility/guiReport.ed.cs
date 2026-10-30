//-----------------------------------------------------------------------------
// Torque2D Packaging Utility
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

function configurePackagingOptions()
{
   Canvas.pushDialog(packagingOptionsDlg);

   // Clear Ignored Directories List
   ignoredDirectories.clearItems();

   // Clear Ignored Files List
   ignoredFiles.clearItems();

   %iDirectories  = $Packaging::Options::IgnoreDir;
   %iFiles        = $Packaging::Options::IgnoreFiles;

   // Add Ignored Directories
   %iter = 0;
   while( ( %word = GetWord(%iDirectories, %iter) ) !$= "" )
   {
      ignoredDirectories.addItem( %word );
      %iter++;
   }

   // Add Ignored Files
   %iter = 0;
   while( ( %word = GetWord(%iFiles, %iter) ) !$= "" )
   {
      ignoredFiles.addItem( %word );
      %iter++;
   }
}

function savePackagingOptions()
{
   %iDirectories  = "";
   %iFiles        = "";

   // Save Ignored Directories
   %count = ignoredDirectories.getItemCount();
   for( %i = 0; %i < %count; %i++ )
      %iDirectories = %iDirectories @ ignoredDirectories.getItemText(%i) @ " ";

   // Save Ignored Files
   %count = ignoredFiles.getItemCount();
   for( %i = 0; %i < %count; %i++ )
      %iFiles = %iFiles @ ignoredFiles.getItemText(%i) @ " ";

   // Store the settings
   $Packaging::Options::IgnoreDir   = %iDirectories;
   $Packaging::Options::IgnoreFiles = %iFiles;

   // Save the settings
   export("$Packaging::Options::*", "./packagingPrefs.cs", false);

   // Finally, pop our dialog
   Canvas.popDialog(packagingOptionsDlg);
}

function configurePackagingExtensions()
{
   Canvas.pushDialog(packagingExtensionsDlg);


   // Clear Ignored Directories List
   scriptExtensionList.clearItems();

   // Clear Ignored Files List
   imageExtensionList.clearItems();

   %scriptExt  = $Packaging::Options::ScriptExtensions;
   %imageExt = $Packaging::Options::ImageExtensions;

   // Add Ignored Directories
   %iter = 0;
   while( ( %word = GetWord(%scriptExt, %iter) ) !$= "" )
   {
      scriptExtensionList.addItem( %word );
      %iter++;
   }

   // Add Ignored Files
   %iter = 0;
   while( ( %word = GetWord(%imageExt, %iter) ) !$= "" )
   {
      imageExtensionList.addItem( %word );
      %iter++;
   }
}

function savePackagingExtensions()
{
   %scriptExt = "";
   %imageExt = "";
   
   // Save Script Extensions
   %count = scriptExtensionList.getItemCount();
   for( %i = 0; %i < %count; %i++ )
      %scriptExt = %scriptExt SPC scriptExtensionList.getItemText(%i);

   // Save Image Extensions
   %count = imageExtensionList.getItemCount();
   for( %i = 0; %i < %count; %i++ )
      %imageExt = %imageExt SPC imageExtensionList.getItemText(%i);
      
   $Packaging::Options::ScriptExtensions = trim(%scriptExt);
   $Packaging::Options::ImageExtensions = trim(%imageExt);
   
   // Save the settings
   export("$Packaging::Options::*", "./packagingPrefs.cs", false);
   
   // Finally, pop our dialog
   Canvas.popDialog(packagingExtensionsDlg);
}

//-----------------------------------------------------------------------------

function cancelPackaging()
{
   Canvas.popDialog( packagingUtilityDlg );
   Canvas.popDialog( packagingOptionsDlg );
   
   $packagingUtilityActive = false;
}

//-----------------------------------------------------------------------------

function setGuiProgress( %progress )
{
   packagingProgress.setValue( %progress );
   Canvas.repaint();
}

//-----------------------------------------------------------------------------

function setGuiText( %text )
{
   packagingProgress.setText( %text );
   Canvas.repaint();
}

function addPackagingStatus( %text )
{
   packagingStatusList.addRow( 0, %text );
   packagingStatusScroll.scrolltoBottom();
}

//-----------------------------------------------------------------------------

function reportFileList( %group, %groupTitle, %collapsed, %defaultCheck )
{
   %pane = new GuiRolloutCtrl() {
      profile = "EditorRolloutProfile";
      horizSizing = "width";
      vertSizing = "bottom";
      Margin = "7 4";
      minExtent = "10 10";
      visible = "1";
      caption = %groupTitle;
      collapsable = "1";
      dragSizable = false;
      barBehindText = "1";
   };
   packagingBuildStack.add( %pane );

   %stack = new GuiStackControl() {
      profile = "EditorTransparentProfile";
      horizSizing = "width";
      vertSizing = "height";
      position = "2 2";
      minExtent = "60 60";
      visible = "1";
      padding = "5 0";
      stackFromBottom = "0";
   };
   %pane.add( %stack );
   
   %yIncrement = 20;
   %x = 0;
   %y = %yIncrement;
   
   for( %i = 0; %i < %group.getCount(); %i++ )
   {
      %obj = %group.getObject( %i );
      
      %chk = new GuiCheckBoxCtrl() 
      {
         profile = "EditorCheckBoxBold";
         horizSizing = "width";
         vertSizing = "bottom";
         position = "0 0";
         extent = getWord( %pane.extent, 0 ) SPC %yIncrement;
         minExtent = "8 15";
         visible = "1";
         text = %obj.getFileName();
         groupNum = "-1";
         buttonType = "ToggleButton";
         
         _fileObject = %obj;
         _defaultVal = %defaultCheck;
      };
      %chk.setValue( %defaultCheck );
      %stack.add( %chk );
   }
    
   if(%pane.isExpanded() && %collapsed)
      %pane.collapse();
   else if(!%pane.isExpanded() && !%collapsed)
      %pane.collapse();


   %pane.sizeToContents();
   %stack.updateStack();
   packagingBuildStack.updateStack();
}

//-----------------------------------------------------------------------------

function postProcessGUI( %BuildList, %PruneList )
{
   // packagingBuildStack->pane->stack->checkboxes
   for( %i = 0; %i < packagingBuildStack.getCount(); %i++ )
   {
      // pane
      %pane = packagingBuildStack.getObject( %i );
      
      for( %j = 0; %j < %pane.getCount(); %j++ )
      {
         // stack
         %stack = %pane.getObject( %j );
         
         for( %k = 0; %k < %stack.getCount(); %k++ )
         {
            // checkbox
            %chk = %stack.getObject( %k );
            
            if( %chk.getValue() != %chk._defaultVal )
            {
               if( %chk.getValue() )
               {
                  %BuildList.add( %chk._fileObject );
               }
               else
               {
                  %PruneList.add( %chk._fileObject );
               }
            }
         }
      }
   }
}
