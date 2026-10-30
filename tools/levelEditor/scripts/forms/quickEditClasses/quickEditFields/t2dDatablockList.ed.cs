function LBQuickEditContent::createT2DDatablockList(%this, %accessor, %label, %filter, %tooltip)
{
   %container = new GuiControl() {
      canSaveDynamicFields = "0";
      Profile = "EditorContainerProfile";
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "0 0";
      Extent = "300 35";
      MinExtent = "8 2";
      canSave = "1";
      Visible = "1";
      hovertime = "100";
      tooltip = %tooltip;
      tooltipProfile = "EditorToolTipProfile";
   };

   %labelControl = new GuiTextCtrl() {
      canSaveDynamicFields = "0";
      Profile = "EditorFontHLBold";
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "16 8";
      Extent = "128 18";
      MinExtent = "8 2";
      canSave = "1";
      Visible = "1";
      hovertime = "100";
      tooltip = %tooltip;
      tooltipProfile = "EditorToolTipProfile";
      text = %label;
      maxLength = "1024";
   };
   %imageMapList = new GuiPopUpMenuCtrlEx() 
   {
      canSaveDynamicFields = "0";
      DatablockFilter = %filter;
      Profile = "EditorPopupMenuLarge";
      Class = "QuickEditT2DList";
      internalName = %property @ "DBList";
      HorizSizing = "width";
      VertSizing = "bottom";
      position = "128 7";
      Extent = "152 20";
      MinExtent = "8 2";
      canSave = "1";
      Visible = "1";
      hovertime = "100";
      tooltip = %tooltip;
      tooltipProfile = "EditorToolTipProfile";
      text = "";
      accessor = %accessor;
      undoLabel = %label;
      object = %this.object;
   };
   %imageMapList.refresh();
   
   %imageMapList.command = "ToolManager.getLastWindow().setFirstResponder(); " @ %imageMapList @ ".updateProperty(" @ %imageMaplist @ ".object" @ ");";
   
   %container.add(%labelControl);
   %container.add(%imageMapList);
   
   %imageMapList.setProperty(%this.object);
   %this.addProperty(%imageMapList);
   %this.add(%container);
   return %container;
}

function QuickEditT2DList::refresh(%this)
{
   %this.clear();
   
   %dbs = getT2DDatablockSet();
   %count = %dbs.getCount();
   for (%i = 0; %i < %count; %i++)
   {
      %db = %dbs.getObject(%i);
      if ((%db.getClassName() $= %this.DatablockFilter) && !$ignoredDatablockSet.isMember(%db))
         %this.add(%db.getName(), %i);
   }
}

function QuickEditT2DList::setProperty(%this, %object)
{
   %this.refresh();
   
   %value = QuickEditField::getObjectValue(%this, %object);
   %index = %this.findText(%value);
   %this.setSelected(%index);
}

function QuickEditT2DList::updateProperty(%this, %object)
{
   %value = %this.getTextById(%this.getSelected());
   %oldValue = QuickEditField::getObjectValue(%this, %object);
   if (%oldValue $= "")
      %oldValue = "-";
   
   QuickEditField::updateProperty(%this, %object, %this.getTextById(%this.getSelected()), %oldValue);
}
