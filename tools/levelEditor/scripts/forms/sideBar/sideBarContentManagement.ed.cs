//-----------------------------------------------------------------------------
// Script Class Namespace Object Linking
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Register Form Content.
//-----------------------------------------------------------------------------
GuiFormManager::AddFormContent( "LevelBuilder", "ContentManagementToolBar", "LBContentManagementToolBar::CreateForm", "", 2 );


//-----------------------------------------------------------------------------
// Form Content Creation Function
//-----------------------------------------------------------------------------
function LBContentManagementToolBar::CreateForm( %formCtrl )
{    

   //---------------------------------------------------------------------------------------------
   // Toolbar Buttons - Utility
   //---------------------------------------------------------------------------------------------
   // Trash Can
   LevelBuilderToolManager::addButtonToBar( %formCtrl, $LevelBuilder::GuiPath @ "/images/iconTrashCan", "MessageBoxOk(\"Removing Items From Your Library\", \"Drag an object from the library here to remove it.\");",  "Drag an object from the library here to remove it.", true, false, "SBCreateTrash" );
   // New ImageMap
   LevelBuilderToolManager::addButtonToBar( %formCtrl, $LevelBuilder::GuiPath @ "/images/iconImageAdd", "launchNewImageMap();",  "Create a new ImageMap" );
   // New Linked Image Map
   LevelBuilderToolManager::addButtonToBar( %formCtrl, $LevelBuilder::GuiPath @ "/images/iconImageLinked", "launchNewLinkImageMap();",  "Create a new Linked ImageMap" );
   // New Animation
   LevelBuilderToolManager::addButtonToBar( %formCtrl, $LevelBuilder::GuiPath @ "/images/iconAnimationAdd", "launchAnimationEditor();",  "Create a new Animation" );

   // Resize as appropriate.
   if( %formCtrl.isMethod("sizeContentsToFit") )
      %formctrl.sizeContentsToFit(%base, %formCtrl.contentID.margin);

   //*** Return back the base control to indicate we were successful
   return %base;

}

//-----------------------------------------------------------------------------
// Form Content Save Function
//-----------------------------------------------------------------------------
function LBContentManagementToolBar::SaveForm( %formCtrl, %contentObj )
{
}


function SBCreateTrash::onControlDropped(%this, %control, %position)
{
   if (isObject(%control.sceneObject))
   {
      
      %datablockName = %control.datablockName;
      %toolType      = %control.toolType;
      
      if (isObject($currentSceneGraph))
         %sceneGraph = $currentSceneGraph;
      else 
      {
         // No scenegraph so it couldn't be referenced.
         MessageBoxYesNo( "Delete Library Object?", "Are you sure you'd like to delete object" SPC %datablockName SPC "from your library?", %datablockName @ ".delete();" );
         return;
      }
   
      switch$( %toolType )
      {
         case "t2dStaticSprite":
            checkImageMapReference(%datablockName, %sceneGraph );
         case "t2dAnimatedSprite":
            checkImageMapReference(%datablockName, %sceneGraph );
         case "t2dChunkedSprite":
            checkImageMapReference(%datablockName, %sceneGraph );
         case "t2dScroller":
            checkImageMapReference(%datablockName, %sceneGraph );
         case "t2dSceneObject":
            %datablockName.delete();
         case "t2dTrigger":
            %datablockName.delete();
         case "t2dParticleEffect":
         case "t2dShape3D":
      }
   }
}
