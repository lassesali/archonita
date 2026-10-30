//-----------------------------------------------------------------------------
// Gui Scene TreeView Form Content
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Register Form Content.
//-----------------------------------------------------------------------------
$LBTreeViewContent = GuiFormManager::AddFormContent( "LevelBuilderSidebarEdit", "Scene TreeView", "LBSceneTreeView::CreateForm", "LBSceneTreeView::SaveForm", 2 );

//-----------------------------------------------------------------------------
// Form Content Creation Function
//-----------------------------------------------------------------------------
function LBSceneTreeView::CreateForm( %formCtrl )
{    

   %base = new GuiRolloutCtrl() 
   {
      canSaveDynamicFields = "0";
      Profile = "EditorRolloutProfile";
      HorizSizing = "width";
      VertSizing = "height";
      Position = "0 0";
      Extent = "323 231";
      MinExtent = "8 2";
      canSave = "1";
      Visible = "1";
      hovertime = "1000";
      Caption = "Object Tree";
      Margin = "6 4";
      DragSizable = true;
      DefaultHeight = "280";
   };

   %scroll = new GuiScrollCtrl() 
   {
      canSaveDynamicFields = "0";
      Profile = "EditorTransparentScrollProfile";
      HorizSizing = "width";
      VertSizing = "bottom";
      position = "0 0";
      Extent = "323 231";
      MinExtent = "72 400";
      canSave = "1";
      visible = "1";
      hovertime = "1000";
      willFirstRespond = "1";
      hScrollBar = "dynamic";
      vScrollBar = "alwaysOn";
      constantThumbHeight = "0";
      childMargin = "0 0";
   };
   %base.add(%scroll);

   %treeObj = new GuiTreeViewCtrl() 
   {
      canSaveDynamicFields = "0";
      Profile = "EditorTreeViewProfile";
      class = "LevelBuilderSceneTreeView";
      HorizSizing = "width";
      VertSizing = "height";
      position = "2 2";
      Extent = "171 21";
      MinExtent = "8 2";
      canSave = "1";
      visible = "1";
      hovertime = "1000";
      tabSize = "16";
      textOffset = "2";
      fullRowSelect = "1";
      itemHeight = "21";
      destroyTreeOnSleep = "1";
      MouseDragging = "0";
      MultipleSelections = "1";
      DeleteObjectAllowed = "0";
      DragToItemAllowed = "0";
      scroll = %scroll;
      base = %base;
      parent = %formCtrl;     
   };

   %scroll.add( %treeObj );
   %formCtrl.add( %base );

   // Open the Current SceneGraph, if any.
   %lastWindow = ToolManager.getLastWindow();
   if( isObject( %lastWindow ) && isObject( %lastWindow.getScenegraph() ) )
      %treeObj.open( %lastWindow.getSceneGraph() );


   // Specify Message Control (Override getObject(0) on new Content which is default message control)
   %base.MessageControl = %treeObj;

   //*** Return back the base control to indicate we were successful
   return %base;

}

//-----------------------------------------------------------------------------
// Form Content Save Function
//-----------------------------------------------------------------------------
function LBSceneTreeView::SaveForm( %formCtrl )
{
   // Nothing.
}

//-----------------------------------------------------------------------------
// Form Content Functionality
//-----------------------------------------------------------------------------
function LevelBuilderSceneTreeView::onContentMessage( %this, %sender, %message )
{

   %command = GetWord( %message, 0 );
   %value   = GetWord( %message, 1 );

   switch$( %command )
   {
      case "openCurrentGraph":
         // Open the Current SceneGraph, if any.
         %lastWindow = ToolManager.getLastWindow();
         if( isObject( %lastWindow ) && isObject( %lastWindow.getScenegraph() ) )
            %this.open( %lastWindow.getSceneGraph() );
      case "openObject":
         %this.open( %value );
   }
   
   %this.scroll.scrollToBottom();
   %this.base.sizeToContents();
   %this.parent.updateStack();
   
}


function LevelBuilderSceneTreeView::onWake( %this )
{
   // Open the Current SceneGraph, if any.
   %lastWindow = ToolManager.getLastWindow();
   if( isObject( %lastWindow ) && isObject( %lastWindow.getScenegraph() ) )
      %this.open( %lastWindow.getSceneGraph() );
}

function LevelBuilderSceneTreeView::onSelect(%this, %object)
{
   ToolManager.clearAcquisition();

   %lastWindow = ToolManager.getLastWindow();  
   // Tell the Inspector to Inspect the SceneGraph
   if ( isObject( %lastWindow ) && %object == %lastWindow.getScenegraph())
   {
      GuiFormManager::SendContentMessage( $LBQuickEdit, %this, "inspect" SPC %object );
      return;
   }
   else 
   {
      ToolManager.acquireObject(%object);
      
      GuiFormManager::SendContentMessage( $LBQuickEdit, %this, "inspect" SPC %object );
   }
}

function LevelBuilderSceneTreeView::onUnSelect(%this, %object)
{
   ToolManager.clearAcquisition(%object);
}

function LevelBuilderSceneTreeView::onAddSelection(%this, %object)
{
   ToolManager.acquireObject(%object);
}

function LevelBuilderSceneTreeView::onRemoveSelection(%this, %object)
{
   ToolManager.clearAcquisition(%object);
}
