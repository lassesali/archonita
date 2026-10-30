//-----------------------------------------------------------------------------
// Torque2D Packaging Utility
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------
$Candy::extentions = ".eff .wav .ogg .map .lyr .hfl .dts .ifl";

function Candy::isHandled( %file )
{
   // Check if the files extention matches
   for( %i = 0; %i < getWordCount( $Candy::extentions ); %i++ )
   {
      if( strlwr(fileExt( %file )) $= getWord( $Candy::extentions, %i ) )
      {
         return true;
      }
   }
   
   return false;
}

//-----------------------------------------------------------------------------

function Candy::processFiles( %buildList )
{ 
   return 0;
}

//-----------------------------------------------------------------------------

function Candy::postProcessBuildList( %buildList, %removeList )
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
                  
                  %candyFileBase = strlwr( fileBase( %file2.getShortFileName() ) );
                  
                  if( strpos( %line, %candyFileBase ) != -1 )
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
