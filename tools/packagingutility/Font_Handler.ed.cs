//-----------------------------------------------------------------------------
// Torque2D Packaging Utility
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------
$Font::extentions = ".gft .uft";

function Font::isHandled( %file )
{
   // Check if the files extention matches
   for( %i = 0; %i < getWordCount( $Font::extentions ); %i++ )
   {
      if( strlwr(fileExt( %file )) $= getWord( $Font::extentions, %i ) )
      {
         return true;
      }
   }
   
   return false;
}

//-----------------------------------------------------------------------------

function Font::processFiles( %buildList )
{
   // No error
   return 0;
}

//-----------------------------------------------------------------------------

function Font::postProcessBuildList( %buildList, %removedList )
{
   // TODO: Do same thing as images etc maybe?
}
