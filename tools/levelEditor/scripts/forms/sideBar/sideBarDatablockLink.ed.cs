//-----------------------------------------------------------------------------
// Script Class Namespace Object Linking
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Register Form Content.
//-----------------------------------------------------------------------------
GuiFormManager::AddFormContent( "LevelBuilder", "DatablockConfigLink", "LBDatablockConfigLink::CreateForm", "", 2 );


//-----------------------------------------------------------------------------
// Form Content Creation Function
//-----------------------------------------------------------------------------
function LBDatablockConfigLink::CreateForm( %formCtrl )
{    
   %base = new GuiControl() 
   {
      class = "LevelBuilderDatablockConfigLink";
      canSaveDynamicFields = "0";
      Profile = "EditorPanelLight";
      HorizSizing = "width";
      VertSizing = "height";
      position = "0 0";
      Extent = "300 25";
      MinExtent = "150 25";
      canSave = "1";
      visible = "1";
      internalName = "DatablockConfigLink";
      tooltipprofile = "GuiInputCtrlProfile";
      tooltip = "This Panel Links Datablocks to newly created objects to define common attributes.";
      hovertime = "1000";
   };
   %formCtrl.add(%base);
   
   %classMenu = new GuiPopUpMenuCtrlEx() 
   {
      canSaveDynamicFields = "0";
      Profile = "EditorPopupMenuLarge";
      internalName = "ClassList";
      class = "LevelBuilderDatablockLinkMenu";
      HorizSizing = "width";
      VertSizing = "bottom";
      position = "95 3";
      Extent = "95 19";
      MinExtent = "8 2";
      canSave = "1";
      Visible = "1";
      hovertime = "1000";
      maxLength = "1024";
      maxPopupHeight = "200";
   };
   
   %base.add( %classMenu );
   %classCaption = new GuiTextCtrl() 
   {
      canSaveDynamicFields = "0";
      Profile = "EditorTextHLBoldRight";
      internalName = "Caption";
      HorizSizing = "right";
      VertSizing = "bottom";
      position = "10 3";
      Extent = "65 19";
      MinExtent = "8 2";
      justify = "right";
      autoSizeWidth = false;
      canSave = "1";
      Visible = "1";
      hovertime = "1000";
      text = "Datablock";
      maxLength = "1024";
   };
   %base.add( %classCaption );
   
   
   // Resize as appropriate.
   if( %formCtrl.isMethod("sizeContentsToFit") )
      %formctrl.sizeContentsToFit(%base, %formCtrl.contentID.margin);

   //*** Return back the base control to indicate we were successful
   return %base;

}

//-----------------------------------------------------------------------------
// Form Content Save Function
//-----------------------------------------------------------------------------
function LBClassConfigLink::SaveForm( %formCtrl, %contentObj )
{
}

//-----------------------------------------------------------------------------
// Form Content Messaging Function
//-----------------------------------------------------------------------------
function LevelBuilderDatablockConfigLink::onContentMessage( %this, %sender, %message )
{

   %command = GetWord( %message, 0 );
   %value   = GetWord( %message, 1 );

   switch$( %command )
   {
      case "update":
         %list = %this.findObjectByInternalName( "ClassList" );
         %list.updateDatablockList();
   }
}

//
//
//
function LevelBuilderDatablockLinkMenu::onSelect(%this, %id)
{
   // Short circuit if there are no classes in list.  
   if( %id == -2 )
      %text = "";
   else      
      %text = %this.getTextById(%id);

   // Store new Link   
   LevelBuilderToolManager::updateConfigDatablockLink( %text );
   
}

function LevelBuilderDatablockLinkMenu::onWake( %this )
{
   %this.updateDatablockList();   
}


function LevelBuilderDatablockLinkMenu::updateDatablockList( %this )
{
   // Clear the List
   %this.clear();
   
   // Add the none option
   %this.add("None", -2);
   
   %datablockList = getT2DDatablockSet();
   %count = %datablockList.getCount();
   %currId = 0;
   
   for (%i = 0; %i < %count; %i++)
   {
      %datablock = %datablockList.getObject(%i);
      if( %datablock.isMemberOfClass("t2dSceneObjectDatablock") )
         %this.add(%datablock.getName(), %currId++);
   }
             
   // Sort the list.
   %this.sort();  
}
