//-----------------------------------------------------------------------------
// LevelBuilder Quick Edit t2dStaticSprite Class
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Register Form Content.
//-----------------------------------------------------------------------------
GuiFormManager::AddFormContent( "LevelBuilderQuickEditClasses", "t2dSceneGraph", "LBQESceneGraph::CreateContent", "LBQESceneGraph::SaveContent", 2 );

//-----------------------------------------------------------------------------
// Form Content Creation Function
//-----------------------------------------------------------------------------
function LBQESceneGraph::CreateContent( %contentCtrl, %quickEditObj )
{
   %base = %contentCtrl.createBaseStack("LBQESceneObjectClass", %quickEditObj);
   %rollout = %base.createRolloutStack("Camera", true);
   %rollout.createTextEdit2("CameraPositionX", "CameraPositionY", 3, "Camera", "X", "Y", "Set the camera Position");
   %rollout.createTextEdit2("CameraSizeX", "CameraSizeY", 3, "", "Width", "Height", "Set the camera Size");
   
   %scriptingRollout = %base.createRolloutStack("Scene Graph Scripting");
   %scriptingRollout.createTextEdit("Name", "TEXT", "Name", "Name the Object for Referencing in Script");
   %scriptingRollout.createTextEdit("Class", "TEXT", "Class", "Link this Object to a Class");
   %scriptingRollout.createTextEdit("SuperClass", "TEXT", "Super Class", "Link this Object to a Parent Class");
   
   %debugRollout = %base.createRolloutStack("Debug Rendering");
   %debugRollout.createCheckBox("ShowBoundingBoxes", "Bounding Boxes", "Show Object Bounding and Clipping Boxes");
   %debugRollout.createCheckBox("ShowLinkPoints", "Link Points", "Show Object Link Points");
   %debugRollout.createCheckBox("ShowWorldLimits", "World Limits", "Show Object World Limits");
   %debugRollout.createCheckBox("ShowCollisionBounds", "Collision Bounds", "Show Object Collision Poly or Ellipse");
   
   %dynamicFieldRollout = %base.createRolloutStack("Dynamic Fields");
   %dynamicFieldRollout.createDynamicFieldStack();
   
   // Return Ref to Base.
   return %base;

}

//-----------------------------------------------------------------------------
// Form Content Save Function
//-----------------------------------------------------------------------------
function LBQESceneGraph::SaveContent( %contentCtrl )
{
   // Nothing.
}

function t2dSceneGraph::setCameraPositionX(%this, %value)
{
   %this.cameraPosition = %value SPC %this.getCameraPositionY();
   if (ToolManager.getActiveTool().getID() == LevelEditorCameraTool.getID())
      LevelEditorCameraTool.setCameraPosition(%this.cameraPosition);
}

function t2dSceneGraph::setCameraPositionY(%this, %value)
{
   %this.cameraPosition = %this.getCameraPositionX() SPC %value;
   if (ToolManager.getActiveTool().getID() == LevelEditorCameraTool.getID())
      LevelEditorCameraTool.setCameraPosition(%this.cameraPosition);
}

function t2dSceneGraph::getCameraPositionX(%this)
{
   return getWord(%this.cameraPosition, 0);
}

function t2dSceneGraph::getCameraPositionY(%this)
{
   return getWord(%this.cameraPosition, 1);
}

function t2dSceneGraph::setCameraSizeX(%this, %value)
{
   %this.cameraSize = %value SPC %this.getCameraSizeY();
   if (ToolManager.getActiveTool().getID() == LevelEditorCameraTool.getID())
      LevelEditorCameraTool.setCameraSize(%this.cameraSize);
}

function t2dSceneGraph::setCameraSizeY(%this, %value)
{
   %this.cameraSize = %this.getCameraSizeX() SPC %value;
   if (ToolManager.getActiveTool().getID() == LevelEditorCameraTool.getID())
      LevelEditorCameraTool.setCameraSize(%this.cameraSize);
}

function t2dSceneGraph::getCameraSizeX(%this)
{
   return getWord(%this.cameraSize, 0);
}

function t2dSceneGraph::getCameraSizeY(%this)
{
   return getWord(%this.cameraSize, 1);
}

function t2dSceneGraph::setClass(%this, %class)
{
   %this.class = %class;
}

function t2dSceneGraph::getClass(%this, %class)
{
   return %this.class;
}

function t2dSceneGraph::setSuperClass(%this, %class)
{
   %this.superClass = %class;
}

function t2dSceneGraph::getSuperClass(%this, %class)
{
   return %this.superClass;
}

function t2dSceneGraph::setShowBoundingBoxes(%this, %value)
{
   if (%value)
      %this.setDebugOn(1);
   else
      %this.setDebugOff(1);
}

function t2dSceneGraph::setShowLinkPoints(%this, %value)
{
   if (%value)
      %this.setDebugOn(2);
   else
      %this.setDebugOff(2);
}

function t2dSceneGraph::setShowWorldLimits(%this, %value)
{
   if (%value)
      %this.setDebugOn(4);
   else
      %this.setDebugOff(4);
}

function t2dSceneGraph::setShowCollisionBounds(%this, %value)
{
   if (%value)
      %this.setDebugOn(5);
   else
      %this.setDebugOff(5);
}

function t2dSceneGraph::getShowBoundingBoxes(%this)
{
   return %this.getDebugOn(1);
}

function t2dSceneGraph::getShowLinkPoints(%this)
{
   return %this.getDebugOn(2);
}

function t2dSceneGraph::getShowWorldLimits(%this)
{
   return %this.getDebugOn(4);
}

function t2dSceneGraph::getShowCollisionBounds(%this)
{
   return %this.getDebugOn(5);
}
