//-----------------------------------------------------------------------------
// Torque2D Packaging Utility
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

function createHTMLReport( %filename, %title )
{
   %ret = new ScriptObject()
   {
      class = "HTMLReport";
   };
   
   %ret.fileObject = new FileObject();
   if( !%ret.fileObject.openForWrite( %filename ) )
   {
      %ret.delete();
      error( "[HTMLReport]: Could not open file '" @ %filename @"' for writing." );
      return 0;
   }
   
   %ret.fileObject.writeLine( "<HTML><HEAD><TITLE>" @ %title @ "</TITLE></HEAD>" );
   %ret.fileObject.writeLine( "<H1>" @ %title @ "</H1>" );
   
   return %ret;
}

function HTMLReport::writeFileList( %this, %group, %groupTitle )
{
   %this.fileObject.writeLine( "<hr><h2>" @ %groupTitle @ "</h2>" );
   %this.fileObject.writeLine( "<ul>" );
   
   for( %i = 0; %i < %group.getCount(); %i++ )
   {
      %obj = %group.getObject( %i );
      %this.fileObject.writeLine( "<li>" @ %obj.getFileName() );
   }
   
   %this.fileObject.writeLine( "</ul>" );
}

function HTMLReport::close( %this )
{
   %this.fileObject.writeLine( "</HTML>" );
   %this.fileObject.close();
   %this.fileObject.delete();
   %this.delete();
}
