function LBQuickEditContent::createCheckBox(%this, %accessor, %label, %tooltip, %spatial)
{
   %container = new GuiControl() {
      canSaveDynamicFields = "0";
      Profile = "EditorContainerProfile";
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "0 0";
      Extent = "300 30";
      MinExtent = "8 2";
      canSave = "1";
      Visible = "1";
      hovertime = "100";
      tooltip = %tooltip;
      tooltipProfile = "EditorToolTipProfile";
   };
   
   %checkBox = new GuiCheckBoxCtrl() {
      canSaveDynamicFields = "0";
      class = QuickEditCheckBox;
      internalName = %accessor @ "CheckBox";
      Profile = "EditorCheckBox";
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "10 3";
      Extent = "323 24";
      MinExtent = "8 2";
      canSave = "1";
      Visible = "1";
      hovertime = "100";
      tooltip = %tooltip;
      tooltipProfile = "EditorToolTipProfile";
      text = %label;
      groupNum = "-1";
      buttonType = "ToggleButton";
      undoLabel = %label;
      accessor = %accessor;
      object = %this.object;
   };
   %checkBox.command = "ToolManager.getLastWindow().setFirstResponder(); " @ %checkBox @ ".updateProperty(" @ %checkBox @ ".object" @ ");";
   
   %container.add(%checkBox);
   
   %checkBox.setProperty(%this.object);
   %this.addProperty(%checkBox, %spatial);
   %this.add(%container);
   return %container;
}

function QuickEditCheckBox::setProperty(%this, %object)
{
   %this.setStateOn(QuickEditField::getObjectValue(%this, %object));
}

function QuickEditCheckBox::updateProperty(%this, %object)
{
   QuickEditField::updateProperty(%this, %object, %this.getValue());
}
