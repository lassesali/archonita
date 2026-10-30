//-----------------------------------------------------------------------------
// Quick Edit Base Field Type
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------
$LB::QuickEditGroup = "LBQuickEditGroup";


if( !isObject( $LB::QuickEditGroup ) )
   new SimGroup( $LB::QuickEditGroup );



function LBQuickEditClass::createBaseStack(%this, %class, %object)
{
   %stack = new GuiStackControl() {
      StackingType = "Vertical";
      class = %class;
      superClass = "LBQuickEditContent";
      HorizStacking = "Left to Right";
      VertStacking = "Top to Bottom";
      Padding = "0";
      canSaveDynamicFields = "0";
      Profile = "EditorTransparentProfile";
      HorizSizing = "width";
      VertSizing = "height";
      Position = "0 0";
      Extent = "300 250";
      MinExtent = "150 10";
      canSave = "1";
      Visible = "1";
      hovertime = "1000";
      object = %object;
      container = true;
   };
   
   %stack.spatialProperties = new SimSet();
   %stack.statusCheck = new SimSet();
   %stack.properties = new SimSet();
   
   $LB::QuickEditGroup.add( %stack.spatialProperties );
   $LB::QuickEditGroup.add( %stack.statusCheck );
   $LB::QuickEditGroup.add( %stack.properties );
   
   return %stack;
}

function LBQuickEditContent::onContentMessage( %this, %sender, %message )
{
   %command = getWord(%message, 0);
   %value = getWord(%message, 1);
 
   switch$ (%command)
   {
      case "syncQuickEdit":
      if (isObject(%value))
         %this.syncQuickEdit(%value);
         
      case "updateQuickEdit":
      if (isObject(%value))
         %this.updateFields(%value);
      
      case "updateSpatialQuickEdit":
      if (isObject(%value))
         %this.updateSpatial(%value);
      case "updateEffectObject":
         $editingeffectObject = %value;

   }
}

function LBQuickEditContent::updateFields(%this, %object)
{
   %this.object = %object;
   %count = %this.statusCheck.getCount();
   for (%i = 0; %i < %count; %i++)
   {
      %control = %this.statusCheck.getObject(%i);
      %control.object = %object;
      %control.updateFields(%object);
   }
}

function LBQuickEditContent::updateSpatial(%this, %object)
{
   %this.object = %object;
   %count = %this.spatialProperties.getCount();
   for (%i = 0; %i < %count; %i++)
   {
      %control = %this.spatialProperties.getObject(%i);
      %control.object = %object;
      if (%control.container)
         %control.updateSpatial(%object);
      else
         %control.setProperty(%object);
   }
}

function LBQuickEditContent::syncQuickEdit(%this, %object)
{
   %this.object = %object;
   %count = %this.properties.getCount();
   for (%i = 0; %i < %count; %i++)
   {
      %control = %this.properties.getObject(%i);
      %control.object = %object;
      %control.setProperty(%object);
   }
   
   %this.updateFields(%object);
}

function LBQuickEditContent::addProperty(%this, %control, %spatial)
{
   %this.properties.add(%control);
   
   if (%spatial !$= "")
      %this.spatialProperties.add(%control);
}

function LBQuickEditContent::addStatusCheck(%this, %control)
{
   %this.statusCheck.add(%control);
}

function QuickEditField::updateProperty(%this, %object, %newValue, %oldValue)
{
   if (%oldValue $= "")
      %oldValue = QuickEditField::getObjectValue(%this, %object);
   
   %checkOldValue = %oldValue;
   %checkNewValue = %newValue;
   if ((%this.precision !$= "") && (%this.precision !$= "TEXT"))
   {
      %checkOldValue = mFloatLength(%oldValue, %this.precision);
      %checkNewValue = mFloatLength(%newValue, %this.precision);
   }
   
   if (%checkOldValue !$= %checkNewValue)
   {
      if (((%oldValue !$= "") && (%newValue !$= "")) || (%this.precision $= "TEXT"))
      {
         QuickEditField::setObjectValue(%this, %object, %newValue);
         QuickEditField::addUndo(%this, %object, %oldValue, %newValue);
         ToolManager.onQuickEdit(%object);
      }
   }
   
   %this.setProperty(%object);
   
   if (isObject(%this.getParent().hideControl))
      %this.getParent().hideControl.updateFields();
}

function QuickEditField::getObjectValue(%this, %object )
{
   if( !isObject( %object ) )
      return "";
      
   if( %this.isProperty == true )
   {
      %value = %object.getFieldValue( %this.accessor );
   }
   else
   {
      %command = "%value = %object.get" @ %this.accessor @ "();";
      eval(%command);
   }
   
   if ((%this.precision !$= "") && (%this.precision !$= "TEXT"))
      %value = mFloatLength(%value, %this.precision);
   
   return %value;
}

function QuickEditField::setObjectValue(%this, %object, %value )
{
   if( %this.isProperty == true )
   {
      %object.setFieldValue( %this.accessor, %value );
   }
   else
   {
      %command = "%object.set" @ %this.accessor @ "(\"" @ %value @ "\");";
      eval(%command);
   }
}

function QuickEdit::setObjectProperty(%object, %accessor, %value)
{
   %oldValue = QuickEdit::getObjectProperty(%object, %accessor);
   %command = "%object.set" @ %accessor @ "(\"" @ %value @ "\");";
   eval(%command);
   %newValue = QuickEdit::getObjectProperty(%object, %accessor);
   
   if (%newValue !$= %oldValue)
   {
      QuickEdit::addUndo(%object, %accessor, %oldValue, %newValue);
      ToolManager.onObjectChanged(%object);
   }
   
   GuiFormManager::BroadcastContentMessage( "LevelBuilderQuickEditClasses", %this, "updateQuickEdit" SPC %this.object );
}

function QuickEdit::getObjectProperty(%object, %accessor)
{
   %command = "%value = %object.get" @ %accessor @ "();";
   eval(%command);
   return %value;
}

function QuickEdit::addUndo(%object, %accessor, %oldValue, %newValue)
{
   %count = 1;
   if (%object.isMemberOfClass("t2dSceneObjectSet"))
   {
      %objectSet = new t2dSceneObjectSet();
      %count = %object.getCount();
      for (%i = 0; %i < %count; %i++)
         %objectSet.add(%object.getObject(%i));
         
      %object = %objectSet;
   }
   
   %undo = new UndoScriptAction()
   {
      actionName = "Changed " @ %accessor;
      class = UndoQuickEdit;
      objectCount = %count;
      object = %object;
      command = "set" @ %accessor;
      oldValue = %oldValue;
      newValue = %newValue;
   };
   
   %undo.addToManager(LevelBuilderUndoManager);
}

function QuickEditField::addUndo(%this, %object, %oldValue, %newValue)
{
   %count = 1;
   if (%object.isMemberOfClass("t2dSceneObjectSet"))
   {
      %objectSet = new t2dSceneObjectSet();
      %count = %object.getCount();
      for (%i = 0; %i < %count; %i++)
         %objectSet.add(%object.getObject(%i));
         
      %object = %objectSet;
   }
   
   %undo = new UndoScriptAction()
   {
      actionName = "Changed " @ %this.undoLabel;
      class = UndoQuickEdit;
      objectCount = %count;
      object = %object;
      command = "set" @ %this.accessor;
      oldValue = %oldValue;
      newValue = %newValue;
      control = %this;
   };
   
   %undo.addToManager(LevelBuilderUndoManager);
}

function UndoQuickEdit::undo(%this)
{
   %command = "%this.object." @ %this.command @ "(\"" @ %this.oldValue @ "\");";
   eval(%command);

   %checkObject = %this.object;
   if (%checkObject.getClassName() $= "t2dParticleEmitter")
      %checkObject = %this.object.getParentEffect();

   // If any of the objects are acquired, update the acquired object set.
   %updateObject = "";
   if (%this.objectCount > 1)
   {
      for (%i = 0; %i < %this.objectCount; %i++)
      {
         if (ToolManager.isAcquired(%this.object.getObject(%i)))
         {
            %updateObject = ToolManager.getAcquiredObjects();
            break;
         }
      }
   }
   else if (ToolManager.isAcquired(%checkObject))
   {
      // If the object is acquired and there are others acquired, update the set.
      if (ToolManager.getAcquiredObjectCount() > 1)
         %updateObject = ToolManager.getAcquiredObjects();
         
      // Otherwise just update the object.
      else
         %updateObject = %this.object;
   }
   
   if (isObject(%updateObject))
   {
      if (isObject(%this.control))
      {
         ToolManager.onQuickEdit(%updateObject);
         %this.control.setProperty(%updateObject);
      }
      else
         ToolManager.onObjectChanged(%updateObject);
   
      GuiFormManager::BroadcastContentMessage( "LevelBuilderQuickEditClasses", %this, "updateQuickEdit" SPC %updateObject );
   }
}

function UndoQuickEdit::redo(%this)
{
   %command = "%this.object." @ %this.command @ "(\"" @ %this.newValue @ "\");";
   eval(%command);

   %checkObject = %this.object;
   if (%checkObject.getClassName() $= "t2dParticleEmitter")
      %checkObject = %this.object.getParentEffect();

   // If any of the objects are acquired, update the acquired object set.
   %updateObject = "";
   if (%this.objectCount > 1)
   {
      for (%i = 0; %i < %this.objectCount; %i++)
      {
         if (ToolManager.isAcquired(%this.object.getObject(%i)))
         {
            %updateObject = ToolManager.getAcquiredObjects();
            break;
         }
      }
   }
   else if (ToolManager.isAcquired(%checkObject))
   {
      if (ToolManager.getAcquiredObjectCount() > 1)
         %updateObject = ToolManager.getAcquiredObjects();
      else
         %updateObject = %this.object;
   }
   
   if (isObject(%updateObject))
   {
      if (isObject(%this.control))
      {
         ToolManager.onQuickEdit(%updateObject);
         %this.control.setProperty(%updateObject);
      }
      else
         ToolManager.onObjectChanged(%updateObject);
         
      GuiFormManager::BroadcastContentMessage( "LevelBuilderQuickEditClasses", %this, "updateQuickEdit" SPC %updateObject );
   }
}