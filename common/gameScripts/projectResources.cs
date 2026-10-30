//---------------------------------------------------------------------------------------------
// Torque GameBuilder. 
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------
if( !isObject( $resourceGroup ) )
   $resourceGroup = new SimGroup( "ResourceGroup" );

//---------------------------------------------------------------------------------------------
// Load a resource object into the active resource tree
//---------------------------------------------------------------------------------------------
function ResourceObject::load( %resourcePath, %resourceName )
{
   // Clear instant resource
   $instantResource = 0;
   
   // Generate resource file path
   %resourceFile = %resourcePath @ "/resources/" @ %resourceName @ "/resourceDatabase.cs";
   
   // Non-Existent Resource
   if( !isFile( %resourceFile ) && !isFile( %resourceFile @ ".dso" ) )
      %resourceFile = "resources/" @ %resourceName @ "/resourceDatabase.cs";
      
   if( !isFile( %resourceFile ) && !isFile( %resourceFile @ ".dso" ) )
      return 0;

   // Load resource script      
   exec( %resourceFile );
   
   // Validate resource object
   if( !isObject( $instantResource ) || !ResourceObject::Validate( $instantResource ) )
   {
      error("Resource with name" SPC %resourceName SPC "found but invalid resource object contained inside resource!");
      return 0;
   }
   
   // Add to active resource tree
   $resourceGroup.add( $instantResource );
      
   eval( $instantResource.LoadFunction @ "(" @ $instantResource @ ");" );
   
   if ($runWithEditors)
      GuiFormManager::BroadcastContentMessage( "LevelBuilderSidebarCreate", 0, "refresh" );
   
   %resourceObj = $instantResource;
   
   // Clear instant resource
   $instantResource = 0;         
   
   return %resourceObj;

}

//---------------------------------------------------------------------------------------------
// Unload a resource object from the active resource tree
//---------------------------------------------------------------------------------------------
function ResourceObject::Unload( %resourceName )
{
   if( !isObject( $resourceGroup ) )
      return false;
      
   for( %i = 0; %i < $resourceGroup.getCount(); %i++ )
   {
      %resourceObject = $resourceGroup.getObject( %i );
      if( !isObject( %resourceObject ) )
         continue;
      
      if( %resourceObject.Name $= %resourceName )
      {
         // Remove from active resource tree
         if( $resourceGroup.isMember( %resourceObject ) )
            $resourceGroup.remove( %resourceObject );

         // Remove from active resource tree
         if( isObject( $dependentResourceGroup ) )
         {
            for( %i = 0; %i < $dependentResourceGroup.getCount(); %i++ )
            {
               %obj = $dependentResourceGroup.getObject( %i );
               if( !isObject( %obj ) )
                  continue;
               
               if( %obj.Name == %resourceObject.Name )   
                  $dependentResourceGroup.remove( %obj );
            }
                     
         }
         
         // Invoke resource unload
         eval( %resourceObject.UnloadFunction @ "(" @ %resourceObject @ ");" );
         
         // Some resources may delete themselves, but we check to be sure.
         if( isObject( %resourceObject ) )
            %resourceObject.delete();
            
            
         if ($runWithEditors)
            GuiFormManager::BroadcastContentMessage( "LevelBuilderSidebarCreate", 0, "refresh" );
         
         return true;
      }
   }

   return false;
}

//---------------------------------------------------------------------------------------------
// Validate a resource object format
//---------------------------------------------------------------------------------------------
function ResourceObject::Validate( %resourceObject )
{
   if( !isObject( %resourceObject ) )
      return false;
      
   // Validate Resource Name
   if( %resourceObject.Name $= "" )
      return false;

   // Validate Resource User (Who makes use of this resource (TGB, TGE, TSE, etc)
   if( %resourceObject.User $= "" )
      return false;

   // Validate Resource LoadFunction
   if( %resourceObject.LoadFunction $= "" )
      return false;

   // Validate Resource UnloadFunction
   if( %resourceObject.UnloadFunction $= "" )
      return false;
   
   // Validate Resource Data Set
   if( %resourceObject.Data $= "" )
      return false;

   // Valid Resource   
   return true;
   
}