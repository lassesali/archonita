//-----------------------------------------------------------------------------
// Animation Builder Script Class
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------
$AnimationBuilder::Group            = "AnimationBuilderGroup";
$AnimationBuilder::SourceGroup      = "AnimationBuilderSourceGroup";
$AnimationBuilder::StoryboardGroup  = "AnimationBuilderStoryboardGroup";
$AnimationBuilder::ImagePreviewGroup  = "AnimationBuilderImagePreviewGroup";
$AnimationBuilder::SceneGraph       = "AnimationBuilderSceneGraph";


//-----------------------------------------------------------------------------
// Initialize AnimationBuilder and allocate resources.
//
// Returns : Nothing.
//-----------------------------------------------------------------------------  
function AnimationBuilder::Init()
{
   // Ensure AnimationBuilderGroup Exists.
   if( !isObject( $AnimationBuilder::Group ) )
      new SimGroup( $AnimationBuilder::Group );
      
   // Ensure AnimationBuilderGroup Exists.
   if( !isObject( $AnimationBuilder::SourceGroup ) )
      new SimGroup( $AnimationBuilder::SourceGroup );

   // Ensure AnimationBuilderGroup Exists.
   if( !isObject( $AnimationBuilder::StoryboardGroup ) )
      new SimGroup( $AnimationBuilder::StoryboardGroup );
      
      // Ensure AnimationBuilderGroup Exists.
   if( !isObject( $AnimationBuilder::ImagePreviewGroup ) )
      new SimGroup( $AnimationBuilder::ImagePreviewGroup );
   
   // Add Source Group to Container
   $AnimationBuilder::Group.add( $AnimationBuilder::SourceGroup );

   // Add Storyboard Group to Container
   $AnimationBuilder::Group.add( $AnimationBuilder::StoryboardGroup );
   
   // Add Image Preview Group to Container
   $AnimationBuilder::Group.add( $AnimationBuilder::ImagePreviewGroup );
      
   // Ensure the Animation Builder Scenegraph Exists.
   if( !isObject( $AnimationBuilder::SceneGraph ) )
   {
      %sceneGraph = new t2dSceneGraph( $AnimationBuilder::SceneGraph );
      $AnimationBuilder::Group.add( %sceneGraph );
   }
   
   // Initialize the time at 1
   animationDatablockTime.setText(1);
   
   // Initialize the value to false
   animationBuilderChkUseAllFrames.setValue(false);
   
   $AnimationBuilder::alreadyDone = false;
}

//-----------------------------------------------------------------------------
// Purge AnimationBuilder Resources
//
// %resourceGroup - The SimGroup Containing the Resources to be Purged.
//
// Returns : Nothing.
//-----------------------------------------------------------------------------
function AnimationBuilder::PurgeResources( %resourceGroup )
{
   if(!isObject(%resourceGroup))
      return;
   
   // Destroy all resources except group.
   %count = %resourceGroup.getCount();
   
   while( %resourceGroup.getCount() > 0 )
   {
      %object = %resourceGroup.getObject( 0 );
      
      %resourceGroup.remove( %object );
      
      if( %object.isMemberOfClass("t2dSceneObject") )
         %object.safeDelete();
      else
         %object.delete();
   }
}

//-----------------------------------------------------------------------------
// Destroy AnimationBuilder Resources
//
// Returns : Nothing.
//-----------------------------------------------------------------------------
function AnimationBuilder::Destroy()
{
   // Destroy all resources.
   AnimationBuilder::PurgeResources( $AnimationBuilder::SourceGroup );
   AnimationBuilder::PurgeResources( $AnimationBuilder::StoryboardGroup );
   AnimationBuilder::PurgeResources( $AnimationBuilder::ImagePreviewGroup );
   
   // Destroy the scenegraph
   if( isObject( $AnimationBuilder::SceneGraph ) )
      $AnimationBuilder::SceneGraph.delete();
      
   // Destroy the container groups.
   if( isObject( $AnimationBuilder::Group ) )
      $AnimationBuilder::Group.delete();
}

//-----------------------------------------------------------------------------
// Build an animation and return the result in the callback.
//
// Returns : Nothing.
//
// Callback Parameters provided
// %callbackFunction( %success [true/false], %datablockName [string] );
//-----------------------------------------------------------------------------
function AnimationBuilder::BuildAnimation( %callbackFunction )
{
}


//-----------------------------------------------------------------------------
// Edit an animation and return the result in the callback.
//
// Returns : True if the edit attempt is succesful or false if there
//           was an error.
//
// Callback Parameters provided
// %callbackFunction( %success [true/false], %datablockName [string] );
//-----------------------------------------------------------------------------
function AnimationBuilder::EditAnimation( %datablockName, %callbackFunction )
{
}

//-----------------------------------------------------------------------------
// Load the managed animations datablocks.
//
// Returns : True if the edit attempt is succesful or false if there
//           was an error.
//
// Callback Parameters provided
// %callbackFunction( %success [true/false], %datablockName [string] );
//-----------------------------------------------------------------------------
function AnimationBuilder::EditAnimation( %datablockName, %callbackFunction )
{
}