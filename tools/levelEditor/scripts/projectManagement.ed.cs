//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------
function LBProject::onAdd( %this )
{
   %this.activeProject = "";
   
   // Set flag to true to always persist objects when we're deleted
   %this.persistOnRemove = false;
}

function LBProject::onRemove( %this ) 
{
   %this.activeProject = "";
   
   // Persist at option
   if( %this.persistOnRemove == true )
      %this.persistToDisk( true, true, true );
}

// Have we been initialized?
function LBProject::isActive( %this ) 
{
   if( %this.activeProject !$= "" )
      return true;
   else
      return false;
}

//
// Persist managed to disk
//
function LBProject::persistToDisk( %this, %bDatablocks, %bResources, %bPersistent )
{
   if (%this.activeProject $= "" || !%this.findProject( %this.activeProject ) )
   {
      editorDebugError("LBProject::PersistToDisk - No current project!");
      return false;
   }
  
   // Save Persistent Objects  
   if( %bDatablocks == true  )
   {
      %datablocksFile = %this.activeProject @ "/managed/datablocks.cs";
      %validFiles     = %this.validateManagedSet( $managedDatablockSet, "Datablocks" );
      if( %validFiles &&  isWriteableFileName( %datablocksFile ) )
      {
         %fo = new FileObject();
         if( %fo.openForWrite( %datablocksFile ) )
         {      
            %fo.writeObject($managedDatablockSet, "$managedDatablockSet = ");
            %fo.close();
         }
         %fo.delete();
      }
   }

   if( %bResources == true )
   {
      %resourcesFile = %this.activeProject @ "/managed/resources.cs";
      %validFiles     = %this.validateManagedSet( $dependentResourceGroup, "Resources" );
      if( %validFiles &&  isWriteableFileName( %resourcesFile ) )
      {
         %fo = new FileObject();
         if( %fo.openForWrite( %resourcesFile ) )
         {      
            %fo.writeObject($dependentResourceGroup, "$dependentResourceGroup = ");
            %fo.close();
         }
         %fo.delete();
      }
   }

   if( %bPersistent == true )
   {
      %persistentFiles = %this.activeProject @ "/managed/persistent.cs";
      %validFiles     = %this.validateManagedSet( $persistentObjectSet, "Persistent" );
      if( %validFiles &&  isWriteableFileName( %persistentFiles ) )
      {
         %fo = new FileObject();
         if( %fo.openForWrite( %persistentFiles ) )
         {      
            %fo.writeObject($persistentObjectSet, "$persistentObjectSet = ");
            %fo.close();
         }
         %fo.delete();
      }
   }   
}


//
// Open a project from the levelBuilder - returns True/False
//
function LBProject::open( %this, %projectName )
{   
   // Does the new project exist?
   if( %projectName $= "" || !%this.findProject( %projectName ) )
   {
      editorDebugError("LBProject::open - Invalid Project!");
      return false;
   }
   
   // If we've already got a loaded project, restart the sim with a new one
   if ( %this.activeProject !$= "" )
   {      
      // Store startup project for restart
      $pref::startupProject = %projectName;
      
      // Restart it!
      restartInstance();
          
      // Return Success
      return true;
   }
   
   // Store active project
   %this.activeProject = %projectName;


   //   
   // Managed Datablocks Validation
   //
   if( !isObject( $managedDatablockSet ) )   
      $managedDatablockSet = new SimSet();
      
   // Mark Set Valid   
   %this.createValidatorToken( $managedDatablockSet, "Datablocks" );


   //   
   // Managed Datablocks Validation
   //
   if( !isObject( $persistentObjectSet ) )   
      $persistentObjectSet = new SimSet();
      
   // Mark Set Valid   
   %this.createValidatorToken( $persistentObjectSet, "Persistent" );


   //   
   // Managed Datablocks Validation
   //
   if( !isObject( $dependentResourceGroup ) )   
      $dependentResourceGroup = new SimGroup();
      
   // Mark Set Valid   
   %this.createValidatorToken( $dependentResourceGroup, "Resources" );

}


//
// Validate a managed set
//
function LBProject::validateManagedSet( %this, %object, %type )
{
   // Must Exist
   if( !isObject( %object ) ) 
      return false;
           
   // Correct Token Type?
   if( %object.setType !$= %type )
      return false;
   
   // Success, it's valid.
   return true;
}


//
// Force a valid token (legacy support)
//
function LBProject::createValidatorToken( %this, %object, %type )
{
   // Must Exist
   if( !isObject( %object ) ) 
      return false;
   
   // Tag as valid. - This should be improved at some point - JDD
   %object.setType = %type;
   
   // Success, it's now valid.
   return true;
}



// Return true/false
function LBProject::findProject( %this, %name ) 
{
   %projects = getDirectoryList( "/", 1 );
   %projectCount = getWordCount( %projects );
   
   for( %i = 0; %i < %projectCount; %i++ )
   {
      if( getWord( %projects, %i ) $= %name )
         return true;
   }
   
   return false;
}



//
// Datablock Set Manipulators
//

function LBProject::addDatablock( %this, %datablock, %persistNow )
{
   if ( !isObject($managedDatablockSet) )
      return false;
      
   if( !%this.validateManagedSet( $managedDatablockSet, "Datablocks" ) )
      return false;
      
   if( !$managedDatablockSet.isMember( %datablock ) )
      $managedDatablockSet.add( %datablock );
      
   // Persist if desired
   if( %persistNow == true )
      %this.persistToDisk( true, false, false );

   // Success      
   return true;
}

function LBProject::removeDatablock( %this, %datablock, %persistNow )
{
   if ( !isObject($managedDatablockSet) )
      return false;
      
   if( !%this.validateManagedSet( $managedDatablockSet, "Datablocks" ) )
      return false;
      
   if( $managedDatablockSet.isMember( %datablock ) )
      $managedDatablockSet.remove( %datablock );
      
   // Persist if desired
   if( %persistNow == true )
      %this.persistToDisk( true, false, false );

   // Success      
   return true;
}


//
// Resources Set Manipulators
//

function LBProject::addResource( %this, %datablock, %persistNow )
{
   if ( !isObject($dependentResourceGroup) )
      return false;
      
   if( !%this.validateManagedSet( $dependentResourceGroup, "Resources" ) )
      return false;
      
   if( !$dependentResourceGroup.isMember( %datablock ) )
      $dependentResourceGroup.add( %datablock );
      
   // Persist if desired
   if( %persistNow == true )
      %this.persistToDisk( false, true, false );

   // Success      
   return true;

}

function LBProject::removeResource( %this, %datablock, %persistNow )
{
   if ( !isObject($dependentResourceGroup) )
      return false;
      
   if( !%this.validateManagedSet( $dependentResourceGroup, "Resources" ) )
      return false;
      
   if( $dependentResourceGroup.isMember( %datablock ) )
      $dependentResourceGroup.remove( %datablock );
      
   // Persist if desired
   if( %persistNow == true )
      %this.persistToDisk( false, true, false );

   // Success      
   return true;   
}

//
// Persistent Object Set Manipulators
//
function LBProject::addPersistent( %this, %datablock, %persistNow )
{
   if ( !isObject($persistentObjectSet) )
      return false;
      
   if( !%this.validateManagedSet( $persistentObjectSet, "Persistent" ) )
      return false;
      
   if( !$persistentObjectSet.isMember( %datablock ) )
      $persistentObjectSet.add( %datablock );
      
   // Persist if desired
   if( %persistNow == true )
      %this.persistToDisk( false, false, true );

   // Success      
   return true;

}

function LBProject::removePersistent( %this, %datablock, %persistNow )
{
   if ( !isObject($persistentObjectSet) )
      return false;
      
   if( !%this.validateManagedSet( $persistentObjectSet, "Persistent" ) )
      return false;
      
   if( $persistentObjectSet.isMember( %datablock ) )
      $persistentObjectSet.remove( %datablock );
      
   // Persist if desired
   if( %persistNow == true )
      %this.persistToDisk( false, false, true );

   // Success      
   return true;   
}


function t2dSceneObject::setPersistent(%this, %value)
{     
   if (%value)
      LBProjectObj.addPersistent(%this, true);
   else
      LBProjectObj.removePersistent(%this, true);
}

function t2dSceneObject::getPersistent(%this, %value)
{
   
   if ( !isObject($persistentObjectSet) )
      return false;
      
   if( !LBProjectObj.validateManagedSet( $persistentObjectSet, "Persistent" ) )
      return false;
        
   if ($persistentObjectSet.isMember(%this))
      return true;
   
   return false;
}

// Ensure singleton exists
if( !isObject( LBProjectObj ) )
   new ScriptObject( LBProjectObj ) { class=LBProject; };

