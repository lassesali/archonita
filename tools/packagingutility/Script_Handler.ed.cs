//-----------------------------------------------------------------------------
// Torque2D Packaging Utility
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

function Script::isHandled( %file )
{
   // Check if the files extention matches
   for( %i = 0; %i < getWordCount( $packaging::Options::ScriptExtensions ); %i++ )
   {
      if( strlwr(fileExt( %file )) $= getWord( $packaging::Options::ScriptExtensions, %i ) )
      {
         return true;
      }
   }
   
   return false;
}

//-----------------------------------------------------------------------------

function Script::processFiles( %buildList )
{
   for( %i = 0; %i < %buildList.getCount(); %i++ )
   {
      %file = %buildList.getObject( %i );
      
      if( Script::isHandled( %file.getFileName() ) )
      {
         if( fileSize( %file.getFileName() ) > 0 )
         {
            addPackagingStatus("Compiling Script : " @ %file.getFileName() );
            %result = compile( %file.getFileName() );

            if( %result == false )
            {
               addPackagingStatus("ERROR Compiling Script : " @ %file.getFileName() );
               return 1;
            }
         }       
         %file.setFileName( %file.getFileName() @ ".dso" );
      }
   }
   
   return 0;
}

//-----------------------------------------------------------------------------

function Script::postProcessBuildList( %buildList, %removeList )
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
                  
                  if( !Script::isHandled( %file2.getFileName() ) )
                     continue;
                  
                  if( strpos( %line, strlwr(%file2.getShortFileName()) ) != -1 )
                  {
                     // Aw shit we may have a reference, try to isolate the path
                     %posPath = getSubStr( %line, 0, strpos( %line, strlwr(%file2.getShortFileName()) ) );
                     
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
         
         %fileObject.close();
         %fileObject.delete();
      }
   }
   return;
   
   // Now go in and remove all objects with a 0-refcount, and then decrement
   // the ref-count for all of the files that that the 0-refcount references
   setGuiText( "Analyzing script files..." );
   setGuiProgress( 0.0 );
   for( %i = 0; %i < %buildList.getCount(); %i++ )
   {
      setGuiProgress( %i / %buildList.getCount() );
      %file = %buildList.getObject( %i );
      
      if( Script::isHandled( %file.getFileName() ) && %file.getRefCount() == 0 )
      {
         // Always add basedir/main.cs as a referenced file
         if( strlwr(%file.getFileName()) !$= ( strlwr(%file.getModDir()) @ "/main.cs" ) )
         {
            echo( "Would normally remove '" @ %file.getFileName() @ "' here." );
            //%file.addToRemoveGroup( %removeList );
         }
      }
   }
}
