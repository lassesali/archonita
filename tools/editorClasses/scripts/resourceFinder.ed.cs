//---------------------------------------------------------------------------------------------
// Torque GameBuilder. 
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

//---------------------------------------------------------------------------------------------
// Load a resource object into the active resource tree
//---------------------------------------------------------------------------------------------
function ResourceFinder::enumResources( %callback )
{
   // Generate resource file path
   %resourcesPath = $currentProject @ "/resources/";
   %resourcesList = getDirectoryList( %resourcesPath );
   
   %wordCount = getWordCount( %resourcesList );
   for( %i = 0; %i < %wordCount; %i++ )
   {
      %resource = GetWord( %resourcesList, %i );
      if( isFile( %resourcesPath @ %resource @ "/resourceDatabase.cs") )
         eval( %callback @ "(" @ %resource @ ");" );
   }
   
   %resourcesPath = "resources/";
   %resourcesList = getDirectoryList( %resourcesPath );
   
   %wordCount = getWordCount( %resourcesList );
   for( %i = 0; %i < %wordCount; %i++ )
   {
      %resource = GetWord( %resourcesList, %i );
      if( isFile( %resourcesPath @ %resource @ "/resourceDatabase.cs") )
         eval( %callback @ "(" @ %resource @ ");" );
   }

}


function ResourceFinder::isActive( %resourceName )
{
   if( !isObject( $resourceGroup ) )
      return false;
      
   for( %i = 0; %i < $resourceGroup.getCount(); %i++ )
   {
      %resourceObject = $resourceGroup.getObject( %i );
      if( !isObject( %resourceObject ) )
         continue;
      
      if( %resourceObject.Name $= %resourceName )         
         return true;
   }

   return false;

}