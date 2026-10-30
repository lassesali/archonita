//----------------------------------------
function GuiEditorStartCreate()
{
   NewGuiDialogClass.setText("GuiControl");
   NewGuiDialogClass.sort();
   NewGuiDialogName.setValue("NewGui");
   Canvas.pushDialog(NewGuiDialog);
}

//----------------------------------------
function GuiEditorCreate()
{
   %name = NewGuiDialogName.getValue();
   %class = NewGuiDialogClass.getText();
   Canvas.popDialog(NewGuiDialog);
   %obj = eval("return new " @ %class @ "(" @ %name @ ");");
   GuiEditorOpen(%obj);
}

//----------------------------------------
function GuiEditorSaveGui()
{
   %currentObject = GuiEditorContent.getObject( 0 );
   
   if( %currentObject == -1 || %currentObject.getName() $= "")
      return;
      
   %name = %currentObject.getName() @ ".gui";
      
   %dialog       =  SaveFileDlgEx;
   %filterList   =  %dialog.findObjectByInternalName("FilterList", true);
      
   if( $Pref::GuiEditor::LastPath !$= "" )
      %currentFile = $Pref::GuiEditor::LastPath @ %name;
   else
      %currentFile = $currentProject @ "/gui/" @ %name;
   
   // Clear Filters
   %dialog.ClearFilters();
   
   // Add Filters
   %filter = %dialog.AddFilter("*.gui","Torque GUI Files");
 
   // Select Filter.
   %filterList.setSelected( %filter );
      
   
   getSaveFilename("*.gui", "GuiEditorSaveGuiCallback", %currentFile);
}

function GuiEditorSaveGuiCallback(%name)
{
   
   
   %obj = GuiEditorContent.getObject(0);
   $Pref::GuiEditor::LastPath = filePath( %name );
   
   // make sure it is saved...
   if(!%obj.save(%name))
   {
      MessageBoxOK("GuiEditor Save Failure", "Failed to save '" @ %name @ "'. The file may be read-only.");
   }
}   

//----------------------------------------
function GuiEdit(%val)
{
   if(%val != 0)
      return;
      
   %content = Canvas.getContent();
   
   if(%content == GuiEditorGui.getId())
   {
      //GlobalActionMap.bind(mouse, button1, mouselook);

      %obj = GuiEditorContent.getObject(0);
      if(%obj != -1)
      {
         GuiGroup.add(%obj);
         Canvas.setContent(%obj);
      }
      GlobalActionMap.unbind( keyboard, "delete" );
   }
   else
   {
      //GlobalActionMap.unbind(mouse, button1);
      GuiEditorOpen(%content);
   }
}

package GuiEditor_BlockDialogs
{

function GuiCanvas::pushDialog()
{

}

function GuiCanvas::popDialog()
{

}

};

//----------------------------------------
function GuiEditorOpen(%content)
{
   GuiEditorMenuBar.clearMenus();
   GuiEditorMenuBar.addMenu("File", 0);
   GuiEditorMenuBar.addMenuItem("File", "New GUI...", 1);
   GuiEditorMenuBar.scriptCommand["File", 1] = "GuiEditorStartCreate();";
   GuiEditorMenuBar.addMenuItem("File", "Save GUI...", 2);
   GuiEditorMenuBar.scriptCommand["File", 2] = "GuiEditorSaveGui();";
   GuiEditorMenuBar.addMenuItem("File", "-", 0);
   GuiEditorMenuBar.addMenuItem("File", "GUI Editor Help...", 3, "F1");
   GuiEditorMenuBar.scriptCommand["File", 3] = "getHelp(\"3. Gui Editor\");";
   GuiEditorMenuBar.addMenuItem("File", "Toggle GUI Editor...", 4, "F10");
   GuiEditorMenuBar.scriptCommand["File", 4] = "GuiEdit(0);";

   GuiEditorMenuBar.addMenu("Edit", 1);
   GuiEditorMenuBar.addMenuItem("Edit", "Cut", 3, "Ctrl X");
   GuiEditorMenuBar.scriptCommand["Edit", 3] = "GuiEditor.saveSelection($GUI::clipboardFile); GuiEditor.deleteSelection();";
   GuiEditorMenuBar.setMenuItemBitmap("Edit", "Cut", 3);
   GuiEditorMenuBar.addMenuItem("Edit", "Copy", 4, "Ctrl C");
   GuiEditorMenuBar.scriptCommand["Edit", 4] = "GuiEditor.saveSelection($GUI::clipboardFile);";
   GuiEditorMenuBar.setMenuItemBitmap("Edit", "Copy", 4);
   GuiEditorMenuBar.addMenuItem("Edit", "Paste", 5, "Ctrl V");
   GuiEditorMenuBar.scriptCommand["Edit", 5] = "GuiEditor.loadSelection($GUI::clipboardFile);";
   GuiEditorMenuBar.setMenuItemBitmap("Edit", "Paste", 5);
   GuiEditorMenuBar.addMenuItem("Edit", "-", 0);
   GuiEditorMenuBar.addMenuItem("Edit", "Select All", 6, "Ctrl A");
   GuiEditorMenuBar.scriptCommand["Edit", 6] = "GuiEditor.selectAll();";

   GuiEditorMenuBar.addMenu("Layout", 2);
   GuiEditorMenuBar.addMenuItem("Layout", "Align Left", 1, "Ctrl L");
   GuiEditorMenuBar.scriptCommand["Layout", 1] = "GuiEditor.Justify(0);";
   GuiEditorMenuBar.addMenuItem("Layout", "Align Right", 2, "Ctrl R");
   GuiEditorMenuBar.scriptCommand["Layout", 2] = "GuiEditor.Justify(2);";
   GuiEditorMenuBar.addMenuItem("Layout", "Align Top", 3, "Ctrl T");
   GuiEditorMenuBar.scriptCommand["Layout", 3] = "GuiEditor.Justify(3);";
   GuiEditorMenuBar.addMenuItem("Layout", "Align Bottom", 4, "Ctrl B");
   GuiEditorMenuBar.scriptCommand["Layout", 4] = "GuiEditor.Justify(4);";
   GuiEditorMenuBar.addMenuItem("Layout", "-", 0);
   GuiEditorMenuBar.addMenuItem("Layout", "Center Horizontally", 5);
   GuiEditorMenuBar.scriptCommand["Layout", 5] = "GuiEditor.Justify(1);";
   GuiEditorMenuBar.addMenuItem("Layout", "Space Vertically", 6);
   GuiEditorMenuBar.scriptCommand["Layout", 6] = "GuiEditor.Justify(5);";
   GuiEditorMenuBar.addMenuItem("Layout", "Space Horizontally", 7);
   GuiEditorMenuBar.scriptCommand["Layout", 7] = "GuiEditor.Justify(6);";
   GuiEditorMenuBar.addMenuItem("Layout", "-", 0);
   GuiEditorMenuBar.addMenuItem("Layout", "Bring to Front", 8);
   GuiEditorMenuBar.scriptCommand["Layout", 8] = "GuiEditor.BringToFront();";
   GuiEditorMenuBar.addMenuItem("Layout", "Send to Back", 9);
   GuiEditorMenuBar.scriptCommand["Layout", 9] = "GuiEditor.PushToBack();";
   GuiEditorMenuBar.addMenuItem("Layout", "Lock Selection", 10);
   GuiEditorMenuBar.scriptCommand["Layout",10] = "GuiEditorTreeView.lockSelection(true);";
   GuiEditorMenuBar.addMenuItem("Layout", "Unlock Selection", 11);
   GuiEditorMenuBar.scriptCommand["Layout",11] = "GuiEditorTreeView.lockSelection(false);";
   GuiEditorMenuBar.addMenu("Move", 3);
   GuiEditorMenuBar.addMenuItem("Move", "Nudge Left", 1, "Left");
   GuiEditorMenuBar.scriptCommand["Move", 1] = "GuiEditor.moveSelection(-1,0);";
   GuiEditorMenuBar.addMenuItem("Move", "Nudge Right", 2, "Right");
   GuiEditorMenuBar.scriptCommand["Move", 2] = "GuiEditor.moveSelection(1,0);";
   GuiEditorMenuBar.addMenuItem("Move", "Nudge Up", 3, "Up");
   GuiEditorMenuBar.scriptCommand["Move", 3] = "GuiEditor.moveSelection(0,-1);";
   GuiEditorMenuBar.addMenuItem("Move", "Nudge Down", 4, "Down");
   GuiEditorMenuBar.scriptCommand["Move", 4] = "GuiEditor.moveSelection(0,1);";
   GuiEditorMenuBar.addMenuItem("Move", "-", 0);
   GuiEditorMenuBar.addMenuItem("Move", "Big Nudge Left", 5, "Shift Left");
   GuiEditorMenuBar.scriptCommand["Move", 5] = "GuiEditor.moveSelection(-10,0);";
   GuiEditorMenuBar.addMenuItem("Move", "Big Nudge Right", 6, "Shift Right");
   GuiEditorMenuBar.scriptCommand["Move", 6] = "GuiEditor.moveSelection(10,0);";
   GuiEditorMenuBar.addMenuItem("Move", "Big Nudge Up", 7, "Shift Up");
   GuiEditorMenuBar.scriptCommand["Move", 7] = "GuiEditor.moveSelection(0,-10);";
   GuiEditorMenuBar.addMenuItem("Move", "Big Nudge Down", 8, "Shift Down");
   GuiEditorMenuBar.scriptCommand["Move", 8] = "GuiEditor.moveSelection(0,10);";

   Canvas.setContent(GuiEditorGui);
   while((%obj = GuiEditorContent.getObject(0)) != -1)
      GuiGroup.add(%obj); // get rid of anything being edited

   %i = 0;
   GuiEditorContentList.clear();
   while((%obj = GuiGroup.getObject(%i)) != -1)
   {
      if(%obj.getName() !$= Canvas)
      {
         if(%obj.getName() $= "")
            %name = "(unnamed) - " @ %obj;
         else
            %name = %obj.getName() @ " - " @ %obj;

         GuiEditorContentList.add(%name, %obj);
      }
      %i++;
   }
   activatePackage(GuiEditor_BlockDialogs);
   GuiEditorContent.add(%content);
   deactivatePackage(GuiEditor_BlockDialogs);
   GuiEditorContentList.sort();
   GuiEditorClassPopup.sort();
   GuiEditorResList.clear();
   GuiEditorResList.add("640 x 480", 640);
   GuiEditorResList.add("800 x 600", 800);
   GuiEditorResList.add("1024 x 768", 1024);
   %ext = $Pref::GuiEditor::PreviewResolution;
   if( %ext $= "" )
   {
   %ext = GuiEditorRegion.getExtent();
   echo("extent is " @ %ext );
   switch(getWord(%ext, 0))
   {
      case 640:
         GuiEditorResList.setText("640 x 480");
      case 800:
         GuiEditorResList.setText("800 x 600");
      case 1024:
         GuiEditorResList.setText("1024 x 768");
   }
   }
   else
   {
      GuiEditorResList.setText( getWord(%ext,0) @ " x " @ getWord(%ext, 1) );
   }
   if(%content.getName() $= "")
      %name = "(unnamed) - " @ %content;
   else
      %name = %content.getName() @ " - " @ %content;
   
   GuiEditorContentList.setText(%name);
   GuiEditorClassPopup.setText("New Control");
   GuiEditor.setRoot(%content);
   GuiEditorRegion.resize(0,0,getWord(%ext,0), getWord(%ext, 1));
   GuiEditorContent.getObject(0).resize(0,0,getWord(%ext,0), getWord(%ext, 1));

//   %content.resize(0,0,getWord(%ext,0), getWord(%ext, 1));

   GuiEditorTreeView.open(%content);

}

function GuiEditorMenuBar::onMenuItemSelect(%this, %menuId, %menu, %itemId, %item)
{
   if(%this.scriptCommand[%menu, %itemId] !$= "")
      eval(%this.scriptCommand[%menu, %itemId]);
   else
      error("No script command defined for menu " @ %menu  @ " item " @ %item);
}

//----------------------------------------
function GuiEditorContentList::onSelect(%this, %id)
{
   GuiEditorOpen(%id);
}

//----------------------------------------
function GuiEditorClassPopup::onSelect(%this, %id)
{
   %class = %this.getText();
   %obj = eval("return new " @ %class @ "();");
   GuiEditor.addNewCtrl(%obj);
   GuiEditorClassPopup.setText("New Control");
}

function GuiEditorResList::onSelect(%this, %id)
{
   switch(%id)
   {
      case 640:
         GuiEditorRegion.resize(0,0,640,480);
         GuiEditorContent.getObject(0).resize(0,0,640,480);
         $Pref::GuiEditor::PreviewResolution = "640 480";
      case 800:
         GuiEditorRegion.resize(0,0,800,600);
         GuiEditorContent.getObject(0).resize(0,0,800,600);
         $Pref::GuiEditor::PreviewResolution = "800 600";
      case 1024:
         GuiEditorRegion.resize(0,0,1024,768);
         GuiEditorContent.getObject(0).resize(0,0,1024,768);
         $Pref::GuiEditor::PreviewResolution = "1024 768";
   }
}

//----------------------------------------
// defines the icons to be used in the tree view control
// provide the paths to each icon minus the file extension
// seperate them with : 
// the order of the icons must correspond to the bit array defined
// in the GuiTreeViewCtrl.h
function GuiEditorTreeView::onDefineIcons(%this)
{
   //%icons = "common/ui/shll_icon_passworded_hi:common/ui/shll_icon_passworded:common/ui/shll_icon_notqueried_hi:common/ui/shll_icon_notqueried:common/ui/shll_icon_favorite_hi:common/ui/shll_icon_default:";
   //GuiEditorTreeView.buildIconTable(%icons);
}
function GuiEditorTreeView::onRightMouseDown(%this, %item, %pts, %obj)
{
   if(%obj)
   {
      GuiEditor.setCurrentAddSet(%obj);
   }
}
function GuiEditorTreeView::onAddSelection(%this,%ctrl)
{
   GuiEditor.addSelection(%ctrl);
}
function GuiEditorTreeView::onRemoveSelection(%this,%ctrl)
{
   GuiEditor.removeSelection(%ctrl);
}
function GuiEditor::onClearSelected(%this)
{ 
   GuiEditorTreeView.clearSelection();
}
function GuiEditor::onAddSelected(%this,%ctrl)
{
   GuiEditorTreeView.addSelection(%ctrl);
}
function GuiEditor::onRemoveSelected(%this,%ctrl)
{
   GuiEditorTreeView.removeSelection(%ctrl); 
}
function GuiEditor::onDelete(%this)
{
   GuiEditorTreeView.deleteSelection();
}
function GuiEditorTreeView::onDeleteSelection(%this)
{ 
   GuiEditor.clearSelection();
}
function GuiEditorTreeView::onSelect(%this, %obj)
{

   if(isObject(%obj))
   {
      GuiEditorInspectFields.inspect(%obj);
      GuiEditorInspectName.setValue(%obj.getName());
      GuiEditor.select(%obj);
   }
}

//----------------------------------------
function GuiEditorInspectApply()
{
   GuiEditorInspectFields.setName(GuiEditorInspectName.getValue());
}

//----------------------------------------
function GuiEditor::onSelect(%this, %ctrl)
{
   
   GuiEditorInspectFields.inspect(%ctrl);
   GuiEditorInspectName.setValue(%ctrl.getName());
   GuiEditor.clearSelection();
   GuiEditor.select(%ctrl);
   GuiEditorTreeView.addSelection(%ctrl); 
}

//----------------------------------------
//function GuiEditorDeleteSelected( %val ) {
 //  if( %val )
  //    GuiEditor.deleteSelection();
//}      

GlobalActionMap.bind(keyboard, "f10", GuiEdit);
