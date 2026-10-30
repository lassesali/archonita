//-----------------------------------------------------------------------------
// Gui SceneGraphEditCtrl Form Content
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Register Form Content.
//-----------------------------------------------------------------------------
$LBQEParticleEffect = GuiFormManager::AddFormContent( "LevelBuilderQuickEditClasses", "t2dParticleEffect", "LBQEParticleEffect::CreateForm", "LBQEParticleEffect::SaveForm", 2 );

//-----------------------------------------------------------------------------
// Form Content Creation Function
//-----------------------------------------------------------------------------
function LBQEParticleEffect::CreateForm( %contentCtrl, %quickEditObj )
{    
   
   %base = %contentCtrl.createBaseStack("LBQESceneObjectClass", %quickEditObj);
   %rollout = %base.createRolloutStack("Particle Effect", true);

   // Create Effect Graph Editing Button
   %button = new GuiIconButtonCtrl(ShowEffectGraphButton) {
      canSaveDynamicFields = "0";
      class  = TGBEffectFieldList;
      superClass = TGBGraphFieldList;
      Profile = "EditorButton";
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "6 4";
      Extent = "16 16";
      canSave = "1";
      Visible = "1";
      hovertime = "100";
      iconBitmap = "~/levelEditor/gui/images/iconGraphLine.png";
      sizeIconToButton = "1";
      ButtonMargin = "0 0";
      fieldsObject = %quickEditObj;
   };
   %rollout.rolloutCtrl.add( %button );
   
   // Create Effect Persistence Buttons
   %emitterStack = %rollout.createEffectPersistence( %quickEditObj );
     
   // Effect Life Mode
   %effectModeCtrl = %rollout.createEnumList("effectMode", true, "Effect Mode", "The Life Mode of the Selected Particle Effect", "t2dParticleEffect", "effectMode");
   %effectTimeStack = %rollout.createHideableStack("(" @ %base @ ".object.effectMode $= \"INFINITE\");");
   %effectTimeStack.addControlDependency(%effectModeCtrl);
   %effectTimeStack.createTextEditProperty( "effectTime", 3, "Effect Lifetime", "This determines the length of the particle effect life");
   %rollout.createCheckBox("effectCollisionStatus", "Use Effect Collisions");
     
   // Create Emitter Chain
   %emitterStack = %rollout.createParticleEmitterStack();
      
   %contentCtrl.add(%base);

   //*** Return back the base control to indicate we were successful
   return %base;
}

//-----------------------------------------------------------------------------
// Form Content Save Function
//-----------------------------------------------------------------------------
function LBParticleEditToolProperties::SaveForm( %formCtrl )
{
   // Nothing.
}


function LBQuickEditContent::createEffectPersistence(%this, %object )
{
   %container = new GuiControl() 
   {
      canSaveDynamicFields = "0";
      Profile = "EditorPanelLight";
      HorizSizing = "right";
      VertSizing = "bottom";
      position = "324 186";
      Extent = "230 37";
      MinExtent = "8 2";
      canSave = "1";
      Visible = "1";
      hovertime = "1000";
   };
   
   %loadEffectButton = new GuiIconButtonCtrl() 
   {
      canSaveDynamicFields = "0";
      Profile = "EditorButton";
      HorizSizing = "right";
      VertSizing = "bottom";
      class = ParticleLoadEffectButton;
      position = "6 7";
      Extent = "94 23";
      MinExtent = "8 2";
      canSave = "1";
      Visible = "1";
      hovertime = "1000";
      text = "Load Effect";
      groupNum = "-1";
      buttonType = "PushButton";
      buttonMargin = "4 4";
      iconBitmap = "tools/levelEditor/gui/images/iconOpen.png";
      iconLocation = "Left";
      sizeIconToButton = "0";
      textLocation = "Right";
      textMargin = "4";
      parentObject = %this;
   };
   
   %saveEffectButton = new GuiIconButtonCtrl() 
   {
      canSaveDynamicFields = "0";
      Profile = "EditorButton";
      class = ParticleSaveEffectButton;
      HorizSizing = "left";
      VertSizing = "bottom";
      position = "130 7";
      Extent = "94 23";
      MinExtent = "8 2";
      canSave = "1";
      Visible = "1";
      hovertime = "1000";
      text = "Save Effect";
      groupNum = "-1";
      buttonType = "PushButton";
      buttonMargin = "4 4";
      iconBitmap = "tools/levelEditor/gui/images/iconSave.png";
      iconLocation = "Left";
      sizeIconToButton = "0";
      textLocation = "Right";
      textMargin = "4";
      parentObject = %this;      
   };
   
   %this.effectObject = %object;
   
   %container.add(%loadEffectButton);
   %container.add(%saveEffectButton);
   
   %this.add(%container);
   
   return %container;
}


function ParticleSaveEffectButton::onClick( %this )
{
   // getSaveFileName Here result to below
   %callback = %this @ ".doSaveEffect";
   %currentFile = $currentProject @ "/data/particles/" @ %object.effectFile;
   
   %dialog       =  SaveFileDlgEx;
   %filterList   =  %dialog.findObjectByInternalName("FilterList", true);
   
   // Clear Filters
   %dialog.ClearFilters();
   
   // Add Filters
   %filter = %dialog.AddFilter("*.eff","TGB Particle Effects");
   
   // Select Filter.
   %filterList.setSelected( %filter );
 
   // On acceptance, set the effect on this object.
   getSaveFileName("*.eff", %callback, %currentFile);
   
}

function ParticleSaveEffectButton::doSaveEffect( %this, %fileName )
{
   if( !isObject( $editingeffectObject ) )
      return;
      
   %effectObject = $editingeffectObject;
      
   %effectObject.saveEffect( %fileName );
   %effectObject.loadEffect( %fileName );
   %effectObject.playEffect( true );
   
   // refresh the particle object library
   GuiFormManager::BroadcastContentMessage( "LevelBuilderSidebarCreate", 0, "refresh" );

}

function ParticleLoadEffectButton::onClick( %this )
{
   // getLoadFileName Here result to doLoadEffect
   %callback = %this @ ".doLoadEffect";
   %currentFile = $currentProject @ "/data/particles/" @ %object.effectFile;
   
   %dialog       =  OpenFileDlgEx;
   %filterList   =  %dialog.findObjectByInternalName("FilterList", true);
   
   // Clear Filters
   %dialog.ClearFilters();
   
   // Add Filters
   %filter = %dialog.AddFilter("*.eff","TGB Particle Effects");
   
   // Select Filter.
   %filterList.setSelected( %filter );
 
   // On acceptance, set the effect on this object.
   getLoadFileName("*.eff", %callback, %currentFile);
   
}

function ParticleLoadEffectButton::doLoadEffect( %this, %fileName )
{
   if( !isObject( $editingeffectObject ) )
      return;
      
   %effectObject = $editingeffectObject;
   
   %position = %effectObject.getPosition();
   %size = %effectObject.getSize();
      
   %effectObject.loadEffect( %fileName );
   %effectObject.playEffect( true );
   
   %effectObject.setSize( GetWord( %size, 0 ), GetWord( %size, 1 ) );
   %effectObject.setPosition( GetWord( %position, 0 ), GetWord( %position, 1 ) );
      

   // Update Global Effect Var      
   GuiFormManager::BroadcastContentMessage( "LevelBuilderQuickEditClasses", %this, "updateEffectObject" SPC %effectObject );   
   // Update Quick Edit
   GuiFormManager::BroadcastContentMessage( "LevelBuilderQuickEditClasses", %this, "syncQuickEdit" SPC %effectObject );
   // refresh the particle object library
   GuiFormManager::BroadcastContentMessage( "LevelBuilderSidebarCreate", 0, "refresh" );

}