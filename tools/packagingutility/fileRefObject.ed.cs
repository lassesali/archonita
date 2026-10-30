//-----------------------------------------------------------------------------
// Torque2D Packaging Utility
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

function pathModDir( %filePATH )
{
   if( strpos( %filePATH, "/" ) != -1 )
      return getSubStr( %filePATH, 0, strpos( %filePATH, "/" ) );
   else
      return %filePATH;
}

//-----------------------------------------------------------------------------

function createFileRefObject( %filename )
{
   %ret = new ScriptObject()
   {
      class = "FileRef";
   };
   
   %ret.setFileName( %filename );
   
   return %ret;
}

//-----------------------------------------------------------------------------

function FileRef::setFileName( %this, %filename )
{
   %this._fileName = %filename;
   %this._path = filePath( %this._fileName );
   
   %this._modDir = pathModDir( %this._path );
}

//-----------------------------------------------------------------------------

function FileRef::onAdd( %this )
{
   %this._references = new SimSet();
   %this._referencedBy = new SimSet();
   %this._skipRefCountStuff = false;
}

//-----------------------------------------------------------------------------

function FileRef::onRemove( %this )
{
   if( !%this._skipRefCountStuff )
   {
      %this._gettingRemoved = true;
      
      if( isObject( %this._references ) )
      {
         for( %i = 0; %i < %this._references.getCount(); %i++ )
         {
            %file = %this._references.getObject( %i );
            %file._referencedBy.remove( %this );
            
            if( !%file._gettingRemoved && %file.getRefCount() == 0 )
               %file.delete();
         }
      }
   }
   
   if( isObject( %this._references ) )
      %this._references.delete();
   if( isObject( %this._referencedBy ) )
         %this._referencedBy.delete();
}

//-----------------------------------------------------------------------------

function FileRef::safeDelete( %this )
{
   %this._skipRefCountStuff = true;
   %this.delete();
}

//-----------------------------------------------------------------------------

function FileRef::getRefCount( %this )
{
   return %this._referencedBy.getCount();
}

//-----------------------------------------------------------------------------

function FileRef::getFileName( %this )
{
   return %this._fileName;
}

//-----------------------------------------------------------------------------

function FileRef::getShortFileName( %this )
{
   return fileName(%this._fileName);
}

//-----------------------------------------------------------------------------

function FileRef::getPath( %this )
{
   return %this._path;
}

//-----------------------------------------------------------------------------

function FileRef::getModDir( %this )
{
   return strlwr(%this._modDir);
}

//-----------------------------------------------------------------------------

function FileRef::addRef( %this, %object )
{
   %this._references.add( %object );
   %object._referencedBy.add( %this );
}

//-----------------------------------------------------------------------------

function FileRef::delRef( %this, %object )
{
   %this._references.remove( %object );
   %object._referencedBy.remove( %this );
}

//-----------------------------------------------------------------------------

function FileRef::addToRemoveGroup( %this, %group )
{
   for( %i = 0; %i < %this._references.getCount(); %i++ )
   {
      %file = %this._references.getObject( %i );
      %file._referencedBy.remove( %this );
      
      if( %file.getRefCount() == 0 )
         %file.addToRemoveGroup( %group );
   }
   
   %group.add( %this );
}
