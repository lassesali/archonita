//-----------------------------------------------------------------------------
// Torque2D Packaging Utility
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

function Image::isHandled( %file )
{
   // Check if the files extention matches
   for( %i = 0; %i < getWordCount( $packaging::Options::ImageExtensions ); %i++ )
   {
      if( strlwr(fileExt( %file )) $= getWord( $packaging::Options::ImageExtensions, %i ) )
      {
         return true;
      }
   }

   if( fileExt(%file) $= "" )
   {
      // Check if the files extention matches
      for( %i = 0; %i < getWordCount( $packaging::Options::ImageExtensions ); %i++ )
      {
         if( isFile( %file @ getWord( $packaging::Options::ImageExtensions, %i ) ) )
         {
            return true;
         }
      }
   }
   
   return false;
}

//-----------------------------------------------------------------------------

function Image::processFiles( %buildList )
{
   // No error
   return 0;
}

//-----------------------------------------------------------------------------

function Image::postProcessBuildList( %buildList, %removeList )
{
   // We're going to go through all the script files and look for references to
   // all .cs and .gui files, and then try to resolve their relative paths
   for( %i = 0; %i < %buildList.getCount(); %i++ )
   {
      %file = %buildList.getObject( %i );
    
      // Do we handle this?
      if( Script::isHandled( %file.getFileName() ) )
      {
         %fileObject = new FileObject();
         %fileObject.openForRead( %file.getFileName() );
         
         setGuiText( "Analyzing: " @ %file.getFileName() );
         setGuiProgress( %i / %buildList.getCount() );
         
         while( !%fileObject.isEOF() )
         {
            %line = %fileObject.readLine();
            %line = strlwr( trim( %line ) );
            
            // Ignore if it's a comment
            if( getSubStr( %line, 0, 2 ) !$= "//" )
            {
               for( %j = 0; %j < %buildList.getCount(); %j++ )
               {
                  %file2 = %buildList.getObject( %j );
                  
                  if( !Image::isHandled( %file2.getFileName() ) )
                     continue;
                  
                  %imageFileBase = strlwr( fileBase( %file2.getShortFileName() ) );

                  if(strlen(%imageFileBase) > 2)
                  {
                     // Special case for bitmap buttons, they use _n _h and _d postfixes
                     if( getSubStr( %imageFileBase, strlen(%imageFileBase) - 2, 2 ) $= "_n"  )
                        %imageFileBase = getSubStr( %imageFileBase, 0, strlen(%imageFileBase) - 2 );
                     else if( getSubStr( %imageFileBase, strlen(%imageFileBase) - 2, 2 ) $= "_h"  )
                        %imageFileBase = getSubStr( %imageFileBase, 0, strlen(%imageFileBase) - 2 );
                     else if( getSubStr( %imageFileBase, strlen(%imageFileBase) - 2, 2 ) $= "_d"  )
                        %imageFileBase = getSubStr( %imageFileBase, 0, strlen(%imageFileBase) - 2 );
                     else if( getSubStr( %imageFileBase, strlen(%imageFileBase) - 2, 2 ) $= "_i"  )
                        %imageFileBase = getSubStr( %imageFileBase, 0, strlen(%imageFileBase) - 2 );
                  
                     if( strpos( %line, %imageFileBase ) != -1 )
                     {
                        // Aw shit we may have a reference, try to isolate the path
                        %posPath = getSubStr( %line, 0, strpos( %line, %imageFileBase ) );

                        // It is reasonable to assume that there will only be one file reference
                        // per line. Otherwise, I hate you.
                        %quote = strpos( %posPath, "\"" );
                        %paren = strpos( %posPath, "(" );
                     
                        if( %quote > %paren )
                           %seek = %quote;
                        else
                           %seek = %paren;
                     
                        while( %seek != -1 )
                        {
                           %seek++;
                           %posPath = getSubStr( %posPath, %seek, strlen( %posPath ) - %seek );
                        
                           %quote = strpos( %posPath, "\"" );
                           %paren = strpos( %posPath, "(" );
                        
                           if( %quote > %paren )
                              %seek = %quote;
                           else
                              %seek = %paren;
                        }
                     
                        %posPath = strreplace( %posPath, "./", strlwr(%file.getPath()) @ "/" );
                        %posPath = strreplace( %posPath, "~/", strlwr(%file.getModDir()) @ "/" );
                     
                        if( strlen( %posPath ) == 0 )
                           continue;
                     
                        if( getSubStr( %posPath, strlen( %posPath ) - 1, 1 ) $= "/" )
                           %posPath = getSubStr( %posPath, 0, strlen( %posPath ) - 1 );
                     
                        //echo( "Possible reference to file '" @ %file2.getFileName() @ "' in file '" @ %file.getFileName() @ "'" );
                        //echo( "Testing path '" @ %file2.getPath() @ "' against path '" @ %posPath @ "'" );
                     
                        if( %posPath $= strlwr(%file2.getPath()) )
                        {
                           //echo( "File '" @ %file2.getFileName() @ "' is referenced in file '" @ %file.getFileName() @ "'" );
                           %file.addRef( %file2 );
                        }
                     }
                  }
               }
            }
         }
         
         %fileObject.close();
         %fileObject.delete();
      }
   }
   
   // Now go in and remove all objects with a 0-refcount, and then decrement
   // the ref-count for all of the files that that the 0-refcount references
   for( %i = 0; %i < %buildList.getCount(); %i++ )
   {
      %file = %buildList.getObject( %i );
      
      if( Image::isHandled( %file.getFileName() ) && %file.getRefCount() == 0 )
      {
         %file.addToRemoveGroup( %removeList );
      }
   }
}