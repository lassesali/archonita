//-----------------------------------------------------------------------------
// Gui SceneGraphEditCtrl Form Content
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Register Form Content.
//-----------------------------------------------------------------------------
$LBCreateSiderBar = GuiFormManager::AddFormContent( "LevelBuilderSidebar", "Create", "LBSideBarCreate::CreateForm", "LBSideBarCreate::SaveForm", 2 );


//-----------------------------------------------------------------------------
// Form Content Creation Function
//-----------------------------------------------------------------------------
function LBSideBarCreate::CreateForm( %contentCtrl )
{    
   %base = new GuiTabPageCtrl() 
   {
      Text = "Create";
      internalName = "SideBarCreatePage";
      canSaveDynamicFields = "0";
      Profile = "EditorPanelLight";
      HorizSizing = "width";
      VertSizing = "height";
      Position = "0 0";
      Extent = "500 400";
      MinExtent = "8 2";
      canSave = "1";
      Visible = "1";
      hovertime = "1000";
   };
   %scroll = new GuiScrollCtrl() 
   {
      canSaveDynamicFields = "0";
      Profile = "EditorTransparentScrollProfile";
      HorizSizing = "width";
      VertSizing = "height";
      position = "0 24";
      Extent = "500 376";
      MinExtent = "72 64";
      canSave = "1";
      visible = "1";
      hovertime = "1000";
      willFirstRespond = "1";
      hScrollBar = "alwaysOff";
      vScrollBar = "alwaysOn";
      constantThumbHeight = "1";
      childMargin = "4 4";
   };
   %base.add(%scroll);
   
   %stack = new GuiStackControl() 
   {
      StackingType = "Vertical";
      HorizStacking = "Left to Right";
      VertStacking = "Top to Bottom";
      Padding = "0";
      canSaveDynamicFields = "0";
      internalName = "editStack";
      class = "LBSideBarCreateMessaging";
      Profile = "EditorTransparentProfile";
      HorizSizing = "width";
      VertSizing = "height";
      Position = "0 0";
      Extent = "467 400";
      MinExtent = "24 24";
      canSave = "1";
      Visible = "1";
      hovertime = "1000";
   };
   %scroll.add( %stack );
   
   
   %contentToolbar = new GuiStackControl() 
   {
      StackingType = "Horizontal";
      HorizStacking = "Left to Right";
      VertStacking = "Top to Bottom";
      Padding = "3";
      canSaveDynamicFields = "0";
      Profile = "EditorButtonLeft";
      HorizSizing = "width";
      VertSizing = "bottom";
      Position = "0 0";
      Extent = "500 24";
      MinExtent = "24 24";
      canSave = "1";
      Visible = "1";
      hovertime = "1000";
   };
   %base.add( %contentToolbar );
  
   
   //-----------------------------------------------------------------------------
   // Add Base to Form
   //-----------------------------------------------------------------------------
   %contentCtrl.add( %base );
   
   //-----------------------------------------------------------------------------
   // Create 'Datablock' Link
   //-----------------------------------------------------------------------------
   %datablockLinkContent = GuiFormManager::FindFormContent( "LevelBuilder", "DatablockConfigLink" );
   if( %datablockLinkContent == 0 || !isObject( %datablockLinkContent ) )
   {
      error("LevelBuilder SideBar Create - Unable to find Class Configuration Link Content!");
   }
   else
   {
      if( %datablockLinkContent.CreateFunction !$= "" )
      {
         %result = eval( %datablockLinkContent.CreateFunction @ "(" @ %stack @ ");" );
         if( isObject( %result ) )
            GuiFormManager::AddContentReference( "LevelBuilder", "DatablockConfigLink", %result );
      }     
   }
   


   %sortStack = new SimSet();
   //-----------------------------------------------------------------------------
   // Load dynamic create bar panels
   //-----------------------------------------------------------------------------
   %count = GuiFormManager::GetFormContentCount( "LevelBuilderSidebarCreate" );
   for( %i = 0; %i < %count; %i++ )
   {
      %contentObj = GuiFormManager::GetFormContentByIndex( "LevelBuilderSidebarCreate", %i );
      if( !isObject( %contentObj ) )
         continue;

      if( %contentObj.CreateFunction !$= "" )
      {
         %result = eval( %contentObj.CreateFunction @ "(" @ %sortStack @ ");" );
         if( isObject( %result ) )
         {
            GuiFormManager::AddContentReference( "LevelBuilderSidebarCreate", %contentObj.Name, %result );               
         }
      }
   }

   // Sort the objects by their requested sort order.
   while (%sortStack.getCount())
   {
      %lowestSortOrder = 100;
      %lowestIndex = -1;
      %count = %sortStack.getCount();
      for (%i = 0; %i < %count; %i++)
      {
         %object = %sortStack.getObject(%i);
         if (%object.sortOrder < %lowestSortOrder)
         {
            %lowestIndex = %i;
            %lowestSortOrder = %object.sortOrder;
         }
      }
      %addObject = %sortStack.getObject(%lowestIndex);
      %stack.add(%addObject);
      %sortStack.remove(%addObject);
   }
   %sortStack.delete();

   //-----------------------------------------------------------------------------
   // Populate 'ContentManagement' Toolbar
   //-----------------------------------------------------------------------------
   %contentManagementContent = GuiFormManager::FindFormContent( "LevelBuilder", "ContentManagementToolBar" );
   if( %contentManagementContent == 0 || !isObject( %contentManagementContent ) )
   {
      error("LevelBuilder SideBar Create - Unable to find ContentManagementToolBar Content!");
   }
   else
   {
      if( %contentManagementContent.CreateFunction !$= "" )
      {
         %result = eval( %contentManagementContent.CreateFunction @ "(" @ %contentToolbar @ ");" );
         if( isObject( %result ) )
            GuiFormManager::AddContentReference( "LevelBuilder", "ContentManagementToolBar", %result );
      }     
   }


   %base.MessageControl = %stack;
     
   return %base;

}


//-----------------------------------------------------------------------------
// Form Content Save Function
//-----------------------------------------------------------------------------
function LBSideBarCreate::SaveForm( %formCtrl, %contentObj )
{
   %formCtrl.lastObjectTab = %contentObj.lastObjectTab;
}

//-----------------------------------------------------------------------------
// Content Message Callback
//-----------------------------------------------------------------------------
function LBSideBarCreateMessaging::onContentMessage(%this, %sender, %message)
{
   %messageCommand = GetWord( %message, 0 );
   switch$( %messageCommand )
   {
      case "refresh":
         %this.sizeStack();
         %parent = %this.getParent();
         if( isObject( %parent ) )
         {
            %pos = %parent.getPosition();
            %ext = %parent.getExtent();
            %parent.resize( GetWord(%pos,0), GetWord(%pos,1),GetWord(%ext,0), GetWord(%ext,1)  );
         }
         
      case "clearSelections":
         for (%i = 0; %i < %this.getCount(); %i++)
         {
            %object = %this.getObject(%i);
            if (%object.isMethod("clearSelections"))
               %object.clearSelections();
         }
         
      case "changeProfiles":
         for (%i = 0; %i < %this.getCount(); %i++)
         {
            %object = %this.getObject(%i);
            if (%object.isMethod("changeProfiles"))
               %object.changeProfiles( getWord(%message, 1) );
         }
         
      case "onLibraryDestroyed":
         for (%i = 0; %i < %this.getCount(); %i++)
         {
            %object = %this.getObject(%i);
            if (%object.isMethod("destroy"))
               %object.destroy();
         }
      
      case "refreshAll":
         // Refresh the resource manager.
         setModPaths(getModPaths());
         
         // Refresh the object types.
         for (%i = 0; %i < %this.getCount(); %i++)
         {
            %object = %this.getObject(%i);
            if (%object.isMethod("refresh"))
               %object.refresh();
         }
         
         // Resize the object library rollouts.
         %this.sizeStack();
         %parent = %this.getParent();
         if( isObject( %parent ) )
         {
            %pos = %parent.getPosition();
            %ext = %parent.getExtent();
            %parent.resize( GetWord(%pos,0), GetWord(%pos,1),GetWord(%ext,0), GetWord(%ext,1)  );
         }
   }
}

//-----------------------------------------------------------------------------
// Properly Size Create Content
//-----------------------------------------------------------------------------
function LBSideBarCreateMessaging::sizeStack( %this )
{
   // Early Out.
   if( !isObject( %this ) )
      return;
      
   // Get to the children first.   
   %count = %this.getCount();
   for( %i = 0; %i < %count; %i++ )
      LBSideBarCreateMessaging::sizeStack( %this.getObject( %i ) );

   // Now act appropriately.   
   if( %this.getClassName() $= "GuiStackControl" )
      %this.updateStack();
   else if( %this.getClassName() $= "GuiRolloutCtrl" && %this.isExpanded() )
      %this.sizeToContents();
   else if( %this.getClassName() $= "GuiDynamicCtrlArrayControl" )
      %this.refresh();
      
}

//-----------------------------------------------------------------------------
// Object Library Thumbnail Callbacks
//-----------------------------------------------------------------------------
function ObjectBrowserItem::onMouseLeave( %this )
{
   %datablockName = %this.datablockName;
   %toolType      = %this.toolType;

   switch$( %toolType )
   {
      case "t2dStaticSprite":
      case "t2dAnimatedSprite":
      case "t2dChunkedSprite":
      case "t2dScroller":
         %object = %this.getSceneObject();
         %object.setScroll(0,0);
      case "t2dTileLayer":
      case "t2dParticleEffect":
         if( isDebugBuild() || $levelEditor::ObjectViewPlayParticles == false )
         {
            %object= %this.getSceneObject();
            %object.stopEffect();
         }
      case "t2dShape3D":
         levelEditor3DShapeTool.setShape( %datablockName );
      case "t2dSceneObject":
      case "t2dTrigger":
   }

}

function ObjectBrowserItem::onMouseEnter( %this )
{
   %datablockName = %this.datablockName;
   %toolType      = %this.toolType;

   switch$( %toolType )
   {
      case "t2dStaticSprite":
      case "t2dAnimatedSprite":
      case "t2dChunkedSprite":
      case "t2dScroller":
         %object = %this.getSceneObject();
         %object.setScroll(10, 0);
      case "t2dTileLayer":
      case "t2dParticleEffect":
         if( isDebugBuild() || $levelEditor::ObjectViewPlayParticles == false )
         {
            %object= %this.getSceneObject();
            %object.playEffect();
         }
      case "t2dShape3D":
      case "t2dSceneObject":
      case "t2dTrigger":
   }

}

function ObjectBrowserItem::onClick( %this )
{
   %datablockName = %this.datablockName;
   %toolType      = %this.toolType;

   if( %datablockName $= "" )
   {
      error("ObjectBrowserItem::onClick - Invalid datablock specified!" SPC %datablockName );
      return;
   }

   switch$( %toolType )
   {
      case "t2dStaticSprite":
         //
         // If the object is already selected and it's a CELL image map, move to the next FRAME.
         //
         if( %this.getValue() )
         {
            %object = %this.getSceneObject();
            %imageMode = %datablockName.getImageMapMode();
            if( (%imageMode $= "CELL") || (%imageMode $= "LINK") || (%imageMode $= "KEY") )
            {
               %currentFrame = %object.getFrame();
               %totalFrames  = %datablockName.getFrameCount();
               if( %currentFrame >= ( %totalFrames - 1 ) )
                  %nextFrame = 0;
               else
                  %nextFrame = %currentFrame + 1;

               levelEditorStaticSpriteTool.setImageMap( %datablockName, %nextFrame );   
               %object.setImageMap( %datablockName, %nextFrame );
            }    
         }
         else
         {
            $selectedStaticSprite = %datablockName;
            levelEditorStaticSpriteTool.setImageMap( %datablockName, 0 );
         }
      case "t2dAnimatedSprite":
         $selectedAnimatedSprite = %datablockName;
         levelEditorAnimatedSpriteTool.setAnimation( %datablockName );
      case "t2dChunkedSprite":
         $selectedChunkedSprite = %datablockName;
         levelEditorChunkedSpriteTool.setImageMap( %datablockName );        
      case "t2dScroller":
         $selectedScroller = %datablockName;
         levelEditorScrollerTool.setImageMap( %datablockName );
      case "t2dTileLayer":
         $selectedTileMap = %datablockName;
         levelEditorTileMapTool.setTileLayerFile( %datablockName );
      case "t2dParticleEffect":
         $selectedParticleEffect = %datablockName;
         levelEditorParticleTool.setEffect( %datablockName );
      case "t2dShape3D":
         $selectedShape3D = %datablockName;
         levelEditor3DShapeTool.setShape( %datablockName );
      case "t2dSceneObject":
      case "t2dTrigger":
   }

   // Set the proper tool for this object.
   LevelBuilderToolManager::setCreateTool( %toolType );
}
