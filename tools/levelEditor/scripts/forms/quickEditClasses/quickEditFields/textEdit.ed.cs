function LBQuickEditContent::createTextEdit(%this, %accessor, %precision, %label, %tooltip, %spatial)
{
   %container = new GuiControl() {
      canSaveDynamicFields = "0";
      Profile = "EditorContainerProfile";
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "0 0";
      Extent = "300 32";
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
   
   %editControl = new GuiTextEditCtrl() {
      class = QuickEditTextEdit;
      internalName = %accessor @ "TextEdit";
      canSaveDynamicFields = "0";
      Profile = "EditorTextEdit";
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "128 5";
      Extent = "64 22";
      MinExtent = "8 2";
      canSave = "1";
      Visible = "1";
      hovertime = "100";
      tooltip = %tooltip;
      tooltipProfile = "EditorToolTipProfile";
      maxLength = "1024";
      historySize = "0";
      password = "0";
      tabComplete = "0";
      sinkAllKeyEvents = "0";
      password = "0";
      passwordMask = "*";
      precision = %precision;
      accessor = %accessor;
      undoLabel = %label;
      object = %this.object;
   };
   
   %editControl.setProperty(%this.object);
   %editControl.altCommand = "ToolManager.getLastWindow().setFirstResponder(); " @ %editControl @ ".updateProperty(" @ %editControl @ ".object" @ ");";
   %editControl.validate = %editControl @ ".updateProperty(" @ %editControl @ ".object" @ ");";
   
   %container.add(%labelControl);
   %container.add(%editControl);
   
   %this.addProperty(%editControl, %spatial);
   %this.add(%container);
   return %container;
}


function LBQuickEditContent::createTextEditProperty(%this, %accessor, %precision, %label, %tooltip, %spatial)
{
   %container = new GuiControl() {
      canSaveDynamicFields = "0";
      Profile = "EditorContainerProfile";
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "0 0";
      Extent = "300 32";
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
   
   %editControl = new GuiTextEditCtrl() {
      class = QuickEditTextEdit;
      internalName = %accessor @ "TextEdit";
      canSaveDynamicFields = "0";
      Profile = "EditorTextEdit";
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "128 5";
      Extent = "64 22";
      MinExtent = "8 2";
      canSave = "1";
      Visible = "1";
      hovertime = "100";
      tooltip = %tooltip;
      tooltipProfile = "EditorToolTipProfile";
      maxLength = "1024";
      historySize = "0";
      password = "0";
      tabComplete = "0";
      sinkAllKeyEvents = "0";
      password = "0";
      passwordMask = "*";
      precision = %precision;
      accessor = %accessor;
      isProperty = true;
      undoLabel = %label;
      object = %this.object;
   };
   
   %editControl.setProperty(%this.object);
   %editControl.altCommand = "ToolManager.getLastWindow().setFirstResponder(); " @ %editControl @ ".updateProperty(" @ %editControl @ ".object" @ ");";
   %editControl.validate = %editControl @ ".updateProperty(" @ %editControl @ ".object" @ ");";
   
   %container.add(%labelControl);
   %container.add(%editControl);
   
   %this.addProperty(%editControl, %spatial);
   %this.add(%container);
   return %container;
}


function QuickEditTextEdit::setProperty(%this, %object)
{
   %this.Text = QuickEditField::getObjectValue(%this, %object);
}

function QuickEditTextEdit::updateProperty(%this, %object)
{
   QuickEditField::updateProperty(%this, %object, %this.getText());
}
