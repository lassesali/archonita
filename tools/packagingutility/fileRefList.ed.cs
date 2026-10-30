//-----------------------------------------------------------------------------
// Torque2D Packaging Utility
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

function createFileRefList()
{
   %ret = new ScriptObject()
   {
      class = "FileRefList";
   };
   
   return %ret;
}

//-----------------------------------------------------------------------------

function FileRefList::onAdd( %this )
{
   %this._simGroup = new SimGroup();
}

//-----------------------------------------------------------------------------

function FileRefList::onRemove( %this )
{
   // Use the safe clear!
   %this.clear();
   if (isObject(%this._simGroup))
      %this._simGroup.delete();
}

//-----------------------------------------------------------------------------

function FileRefList::add( %this, %obj )
{
   %this._simGroup.add( %obj );
}

//-----------------------------------------------------------------------------

function FileRefList::remove( %this, %obj )
{
   return %this._simGroup.remove( %obj );
}

//-----------------------------------------------------------------------------

function FileRefList::getCount( %this )
{
   if( isObject( %this._simGroup ) )
      return %this._simGroup.getCount();
   else
      return 0;
}

//-----------------------------------------------------------------------------

function FileRefList::getObject( %this, %index )
{
   // Let SimGroup do the error handling
   if( isObject( %this._simGroup ) )
      return %this._simGroup.getObject( %index );
   else
      return 0;
}

//-----------------------------------------------------------------------------

function FileRefList::isMember( %this, %obj )
{
   // Let SimGroup do the error handling
   if( isObject( %this._simGroup ) )
      return %this._simGroup.isMember( %obj );
   else
      return 0;
}

//-----------------------------------------------------------------------------

function FileRefList::clear( %this )
{
   // This is a safe version of clear for file-ref objects
   for( %i = 0; %i < %this.getCount(); %i++ )
   {
      %fro = %this.getObject( %i );
      
      %fro.safeDelete();
   }
}
