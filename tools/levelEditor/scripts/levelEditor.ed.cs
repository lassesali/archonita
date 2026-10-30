//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

function LevelBuilderSceneEdit::setHistoryUndoManager(%this, %undoManager)
{
   GuiFormManager::SendContentMessage($LBHistoryViewContent, %this, "setUndoManager" SPC %undoManager);
}

function LevelBuilderSceneEdit::onAcquireObject(%this, %object)
{   
   if (%object.getClassName() $= "t2dTileMap")
   {
      %this.clearAcquisition(%object);
      return;
   }

   GuiFormManager::SendContentMessage( $LBQuickEdit, %this, "inspect" SPC %object );
   
   //if (%object.getClassName() $= "t2dParticleEffect")
   //{
      //LevelBuilderParticleEditor.onEffectAcquired(%object);
   //}
}

function LevelBuilderSceneEdit::onAcquireObjectSet(%this, %object)
{
   GuiFormManager::SendContentMessage( $LBQuickEdit, %this, "inspect" SPC %object );
   
}

function LevelBuilder::reloadTileLayers(%layerFile)
{
   if (isObject(ToolManager) && isObject(ToolManager.getLastWindow()))
   {
      %scenegraph = ToolManager.getLastWindow().getSceneGraph();
      if (isObject(%scenegraph))
      {
         %count = %scenegraph.getSceneObjectCount();
         for (%i = 0; %i < %count; %i++)
         {
            %object = %scenegraph.getSceneObject(%i);
            if (%object.getClassName() $= "t2dTileLayer")
            {
               if (%object.layerFile $= %layerFile)
               {
                  %object.loadTileLayer(%layerFile);
               }
            }
         }
      }
   }
}

function LevelBuilderSceneEdit::onQuickEdit(%this, %object)
{
   ToolManager.updateAcquiredObjectSet();
}

function LevelBuilderSceneEdit::onObjectChanged(%this, %object)
{
   GuiFormManager::SendContentMessage( $LBQuickEdit, %this, "inspectUpdate" );
}

function LevelBuilderSceneEdit::onObjectSpatialChanged(%this, %object)
{
   GuiFormManager::SendContentMessage( $LBQuickEdit, %this, "inspectSpatial" SPC %object );
   
   ToolManager.updateAcquiredObjectSet();
}

function LevelBuilderCameraTool::onCameraChanged(%this, %cameraPos, %cameraSize)
{
   if (ToolManager.getAcquiredObjectCount() < 1)
   {
      %scenegraph = ToolManager.getLastWindow().getSceneGraph();
      %scenegraph.cameraPosition = %cameraPos;
      %scenegraph.cameraSize = %cameraSize;
      GuiFormManager::SendContentMessage( $LBQuickEdit, %this, "inspect" );
   }
}

function LevelBuilderCreateTool::onObjectCreated(%this, %object, %class)
{
   if (%class $= "t2dPath")
      ToolManager.editPath(%object);
   else
      LevelBuilderToolManager::setTool(LevelEditorSelectionTool);
}

function LevelBuilderSceneEdit::onRelinquishObject(%this, %object)
{
   %count = %this.getAcquiredObjectCount();
   %acquiredObjects = %this.getAcquiredObjects();
   %acquiredGroup = %this.getAcquiredGroup();
   if (%count == 0)
      GuiFormManager::SendContentMessage( $LBQuickEdit, %this, "inspect" );
   else if (%count == 1)
      GuiFormManager::SendContentMessage( $LBQuickEdit, %this, "inspect" SPC %acquiredObjects.getObject(0));
   else if (isObject(%acquiredGroup))
      GuiFormManager::SendContentMessage( $LBQuickEdit, %this, "inspect" SPC %acquiredGroup);
   else
      GuiFormManager::SendContentMessage( $LBQuickEdit, %this, "inspect" SPC %acquiredObjects);
}

function LevelBuilderSceneEdit::onObjectDeleted(%this, %objects)
{
   GuiFormManager::SendContentMessage( $LBQuickEdit, %this, "inspect" );
}

function LevelBuilderSceneEdit::editCollisionPoly(%this, %object)
{
   LevelBuilderToolManager::setTool(LevelEditorPolyTool);
   LevelEditorPolyTool.editObject(%object);
}

function LevelBuilderSceneEdit::editLinkPoints(%this, %object)
{
   LevelBuilderToolManager::setTool(LevelEditorLinkPointTool);
   LevelEditorLinkPointTool.editObject(%object);
}

function LevelBuilderSceneEdit::mountObject(%this, %object)
{
   LevelBuilderToolManager::setTool(LevelEditorMountTool);
   LevelEditorMountTool.editObject(%object);
}

function LevelBuilderSceneEdit::dismountObject(%this, %object)
{
   LevelEditorMountTool.dismountObject(%this, %object);
}

function LevelBuilderSceneEdit::editWorldLimit(%this, %object)
{
   LevelBuilderToolManager::setTool(LevelEditorWorldLimitTool);
   LevelEditorWorldLimitTool.editObject(%object);
}

function LevelBuilderSceneEdit::editTileLayer(%this, %object)
{
   toggleTileEditor();
   loadLayer(%object.layerFile);
}

function LevelBuilderSceneEdit::editPath(%this, %object)
{
   LevelBuilderToolManager::setTool(LevelEditorPathEditTool);
   LevelEditorPathEditTool.editObject(%object);
}

function LevelBuilderMountTool::onFinishEdit(%this, %object)
{
   LevelBuilderToolManager::setTool(LevelEditorSelectionTool);
   LevelEditorSelectionTool.refreshWidgets();
}

function LevelBuilderSceneEdit::editImageMap(%this, %object)
{
   launchEditImageMap(%object.getImageMap());
}

function LevelBuilderSceneEdit::editAnimation(%this, %object)
{
   launchEditAnimation(%object.getAnimationName());
}