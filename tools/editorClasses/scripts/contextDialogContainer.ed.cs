//-----------------------------------------------------------------------------
// Context Dialog Container Class
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// ContextDialogContainer Class - Example Use
//-----------------------------------------------------------------------------
//
//%MyContextDialog = new ScriptObject() 
//{
//   class     = ContextDialogContainer;
//   superClass= MyCallbackNamespace;
//   dialog    = %myContainedDialog.getID();
//};
//
// %MyContextDialog.show( %positionX, %positionY );
// %MyContextDialog.hide();
//
//function MyCallbackNamespace::onContextActivate( %this ) 
//{
//    echo("Dialog has been pushed onto canvas, clicking outside of it will pop it!");
//}
//function MyCallbackNamespace::onContextDeactivate( %this )
//{
//    echo("Dialog has lost it's context and has thus been popped from the canvas!");
//}

function ContextDialogContainer::onAdd(%this)
{
   if( !isObject( %this.Dialog ) )
   {
      warn("ContextDialogContainer::onAdd - Invalid Context Dialog Specified!");
      return false;
   }
   
   // Add to our cleanup group.
   $EditorClassesGroup.add( %this );  
   
   %this.base = new GuiButtonBaseCtrl()
   {
      profile = GuiTransparentProfile;
      class = ContextDialogWatcher;
      parent = %this;
      modal = true;
   };
   
   // Flag not active.
   %this.isPushed = false;
   
   // Add to our cleanup group.
   $EditorClassesGroup.add( %this.base );

}

function ContextDialogContainer::onRemove(%this)
{
   %this.Hide();
   
   if( isObject( %this.base ) ) 
      %this.base.delete();
}

// Note : positionX/Y arguments are global coordinates 
// JDD    for the dialog placement on the context.
function ContextDialogContainer::Show( %this, %positionX, %positionY )
{
   if( %this.isPushed == true ) 
      return true;
      
   if( !isObject( %this.Dialog ) )
      return false;

   // Store old parent.
   %this.oldParent = %this.dialog.getParent();
         
   // Set new parent.
   %this.base.add( %this.Dialog );
   
   if( %positionX !$= "" && %positionY !$= "" )
      %this.Dialog.setPositionGlobal( %positionX, %positionY );
   
   Canvas.pushDialog( %this.base, 99 );
   
}

function ContextDialogContainer::Hide( %this )
{
   if( %this.isPushed == true )
      Canvas.popDialog( %this.base );
      
   // Restore Old Parent;
   if( isObject( %this.Dialog ) && isObject( %this.oldParent ) )
      %this.oldParent.add( %this.Dialog );
}


// ContextDialogWatcher Class - JDD
// CDW is a namespace link for the context background button to catch
//     event information and pass it back to the main class.
//
// onClick it will dismiss the parent
// onDialogPop it will cleanup and notify user of deactivation
// onDialogPush it will initialize state information and notify user of activation
function ContextDialogWatcher::onClick( %this )
{
   if( isObject( %this.parent ) )
      %this.parent.hide();
}

function ContextDialogWatcher::onDialogPop( %this )
{
   if( !isObject( %this.parent ) )
      return;

   %this.parent.isPushed = false;

   if( %this.parent.isMethod( "onContextDeactivate" ) )
      %this.parent.onContextDeactivate();
}

function ContextDialogWatcher::onDialogPush( %this )
{
   if( !isObject( %this.parent ) )
      return;
   
   %this.parent.isPushed = true;
   
   if( %this.parent.isMethod( "onContextActivate" ) )
      %this.parent.onContextActivate();

}