//-----------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

$levelBuilderClipboardFile = "tools/levelEditor/data/clipboard.cs";
   
function reloadImageMaps()
{
   flushTextureCache();
   recompileAllImageMaps();
}

function LevelBuilderSceneEdit::groupObjects(%this)
{
   %this.groupAcquiredObjects();
   RefreshTreeView();
}

function LevelBuilderSceneEdit::breakApart(%this)
{
   %this.breakApartAcquiredObjects();
   RefreshTreeView();
}

function RefreshTreeView()
{
   GuiFormManager::SendContentMessage($LBTreeViewContent, "", "openCurrentGraph");
}

function levelBuilderUndo(%val)
{
   if (%val)
      ToolManager.undo();
}

function levelBuilderRedo(%val)
{
   if (%val)
      ToolManager.redo();
}

function levelBuilderCut(%val)
{
   if (%val)
   {
      levelBuilderCopy(1);
      // Undo will be handled by this call.
      ToolManager.deleteAcquiredObjects();
   }
}

function levelBuilderCopy(%val)
{
   if (%val)
      saveSelectedObjectsCallback($levelBuilderClipboardFile);
}

function levelBuilderPaste(%val)
{
   if (%val)
   {
      %undo = new UndoScriptAction() { actionName = "Paste"; class = UndoPasteAction; };
      %undo.objectList = new SimSet();
      // When we make the switch to fileObject.writeObject for loading and saving, this
      // will be much nicer.
      %objectList = $currentSceneGraph.getSceneObjectList();
      addToLevelCallback($levelBuilderClipboardFile);
      %newObjectList = $currentSceneGraph.getSceneObjectList();
      
      // Anything in the new list but not in the old list was just added.
      for (%i = 0; %i < getWordCount(%newObjectList); %i++)
      {
         %obj = getWord(%newObjectList, %i);
         %found = false;
         for (%j = 0; %j < getWordCount(%objectList); %j++)
         {
            if (getWord(%objectList, %j) == %obj)
               %found = true;
         }
         
         if (!%found)
            %undo.objectList.add(%obj);
      }
      
      if (%undo.objectList.getCount() > 0)
         %undo.addToManager(LevelBuilderUndoManager);
   }
}

function UndoPasteAction::undo(%this)
{
   ToolManager.clearAcquisition();
   for (%i = 0; %i < %this.objectList.getCount(); %i++)
      ToolManager.moveToRecycleBin(%this.objectList.getObject(%i));
}

function UndoPasteAction::redo(%this)
{
   for (%i = 0; %i < %this.objectList.getCount(); %i++)
      ToolManager.moveFromRecycleBin(%this.objectList.getObject(%i));
}

function levelBuilderSetSelectionTool(%val)
{
   if (%val && (ToolManager.getActiveTool() != LevelEditorSelectionTool.getId()))
      LevelBuilderToolManager::setTool(LevelEditorSelectionTool);
}

function levelBuilderHomeView(%val)
{
   if (%val)
   {
      %sceneWindow = ToolManager.getLastWindow();
      %scenegraph = %sceneWindow.getSceneGraph();
      %cameraPosition = "0 0";
      %cameraSize = "100 75";
      if (%scenegraph.cameraPosition)
         %cameraPosition = %scenegraph.cameraPosition;
      if (%scenegraph.cameraSize)
         %cameraSize = %scenegraph.cameraSize;
         
      %sceneWindow.setTargetCameraPosition(%cameraPosition, %cameraSize);
      %sceneWindow.setTargetCameraZoom(1.0);
      %sceneWindow.startCameraMove(0.5);
   }
}

function levelBuilderZoomView(%amount)
{
   %sceneWindow = ToolManager.getLastWindow();
   %scenegraph = %sceneWindow.getSceneGraph();
   %cameraPosition = ToolManager.getLastWindow().getCurrentCameraPosition();
   %cameraSize = "100 75";
   if (%scenegraph.cameraSize)
      %cameraSize = %scenegraph.cameraSize;
      
   %sceneWindow.setTargetCameraPosition(%cameraPosition, %cameraSize);
   %sceneWindow.setTargetCameraZoom(%amount);
   %sceneWindow.startCameraMove(0.5);
}

function levelBuilderZoomToFit(%val)
{
   if (%val)
   {
      %sceneWindow = ToolManager.getLastWindow();
      %sceneGraph = %sceneWindow.getSceneGraph();
      if (!isObject(%sceneWindow) || !isObject(%sceneGraph))
         return;
      
      %count = %sceneGraph.getSceneObjectCount();
      if (%count < 1)
         return;
      
      %minX = 0;
      %minY = 0;
      %maxX = 1;
      %maxY = 1;
      for (%i = 0; %i < %count; %i++)
      {
         %object = %sceneGraph.getSceneObject(%i);
         if (%i == 0)
         {
            %minX = %object.getPositionX() - (%object.getWidth() / 2);
            %minY = %object.getPositionY() - (%object.getHeight() / 2);
            %maxX = %object.getPositionX() + (%object.getWidth() / 2);
            %maxY = %object.getPositionY() + (%object.getHeight() / 2);
         }
         else
         {
            %newMinX = %object.getPositionX() - (%object.getWidth() / 2);
            %newMinY = %object.getPositionY() - (%object.getHeight() / 2);
            %newMaxX = %object.getPositionX() + (%object.getWidth() / 2);
            %newMaxY = %object.getPositionY() + (%object.getHeight() / 2);
            if (%newMinX < %minX)
               %minX = %newMinX;
            if (%newMaxX > %maxX)
               %maxX = %newMaxX;
            if (%newMinY < %minY)
               %minY = %newMinY;
            if (%newMaxY > %maxY)
               %maxY = %newMaxY;
         }
      }
      
      %sceneWindow.setTargetCameraArea(%minX, %minY, %maxX, %maxY);
      %sceneWindow.setTargetCameraZoom(1.0);
      %sceneWindow.startCameraMove(0.5);
   }
}

function levelBuilderZoomToSelected(%val)
{
   if (%val)
   {
      %sceneWindow = ToolManager.getLastWindow();
      if (!isObject(%sceneWindow))
         return;
      
      %selectedObjects = ToolManager.getAcquiredObjects();
      %count = %selectedObjects.getCount();
      if (%count < 1)
         return;
      
      %minX = 0;
      %minY = 0;
      %maxX = 1;
      %maxY = 1;
      for (%i = 0; %i < %count; %i++)
      {
         %object = %selectedObjects.getObject(%i);
         if (%i == 0)
         {
            %minX = %object.getPositionX() - (%object.getWidth() / 2);
            %minY = %object.getPositionY() - (%object.getHeight() / 2);
            %maxX = %object.getPositionX() + (%object.getWidth() / 2);
            %maxY = %object.getPositionY() + (%object.getHeight() / 2);
         }
         else
         {
            %newMinX = %object.getPositionX() - (%object.getWidth() / 2);
            %newMinY = %object.getPositionY() - (%object.getHeight() / 2);
            %newMaxX = %object.getPositionX() + (%object.getWidth() / 2);
            %newMaxY = %object.getPositionY() + (%object.getHeight() / 2);
            if (%newMinX < %minX)
               %minX = %newMinX;
            if (%newMaxX > %maxX)
               %maxX = %newMaxX;
            if (%newMinY < %minY)
               %minY = %newMinY;
            if (%newMaxY > %maxY)
               %maxY = %newMaxY;
         }
      }
      
      %sceneWindow.setTargetCameraArea(%minX, %minY, %maxX, %maxY);
      %sceneWindow.setTargetCameraZoom(1.0);
      %sceneWindow.startCameraMove(0.5);
   }
}

function levelBuilderSetEditPanel(%val)
{
   if (%val)
      GuiFormManager::SendContentMessage($LBSideBarContent, "", "setTabPage" SPC "edit");
}

function levelBuilderSetProjectPanel(%val)
{
   if (%val)
      GuiFormManager::SendContentMessage($LBSideBarContent, "", "setTabPage" SPC "project");
}

function levelBuilderSetCreatePanel(%val)
{
   if (%val)
      GuiFormManager::SendContentMessage($LBSideBarContent, "", "setTabPage" SPC "create");
}

function levelBuilderZoomViewIn(%val)
{
   if (%val)
   {
      %camera = ToolManager.getLastWindow();
      %zoom = %camera.getCurrentCameraZoom();
      %amount = 120 * 0.001 * %zoom;
      %camera.setTargetCameraZoom(%zoom + %amount);
      %camera.startCameraMove(0.1);
   }
}

function levelBuilderZoomViewOut(%val)
{
   if (%val)
   {
      %camera = ToolManager.getLastWindow();
      %zoom = %camera.getCurrentCameraZoom();
      %amount = -120 * 0.001 * %zoom;
      %camera.setTargetCameraZoom(%zoom + %amount);
      %camera.startCameraMove(0.1);
   }
}

function levelBuilderViewLeft(%val)
{
   if (%val && (ToolManager.getAcquiredObjectCount() == 0))
   {
      %camera = ToolManager.getLastWindow();
      %position = %camera.getCurrentCameraPosition();
      %zoom = %camera.getCurrentCameraZoom();
      %amount = (-10 + %zoom) SPC 0;
      %camera.setTargetCameraPosition(t2dVectorAdd(%position, %amount));
      %camera.startCameraMove(0.1);
   }
}

function levelBuilderViewRight(%val)
{
   if (%val && (ToolManager.getAcquiredObjectCount() == 0))
   {
      %camera = ToolManager.getLastWindow();
      %position = %camera.getCurrentCameraPosition();
      %zoom = %camera.getCurrentCameraZoom();
      %amount = (10 - %zoom) SPC 0;
      %camera.setTargetCameraPosition(t2dVectorAdd(%position, %amount));
      %camera.startCameraMove(0.1);
   }
}

function levelBuilderViewUp(%val)
{
   if (%val && (ToolManager.getAcquiredObjectCount() == 0))
   {
      %camera = ToolManager.getLastWindow();
      %position = %camera.getCurrentCameraPosition();
      %zoom = %camera.getCurrentCameraZoom();
      %amount = 0 SPC (-10 + %zoom);
      %camera.setTargetCameraPosition(t2dVectorAdd(%position, %amount));
      %camera.startCameraMove(0.1);
   }
}

function levelBuilderViewDown(%val)
{
   if (%val && (ToolManager.getAcquiredObjectCount() == 0))
   {
      %camera = ToolManager.getLastWindow();
      %position = %camera.getCurrentCameraPosition();
      %zoom = %camera.getCurrentCameraZoom();
      %amount = 0 SPC (10 - %zoom);
      %camera.setTargetCameraPosition(t2dVectorAdd(%position, %amount));
      %camera.startCameraMove(0.1);
   }
}
