// Defines functionality for project options dialog window

// The dialog becomes active - fill resources lists.
function projectOptionsDlg::onWake( %this )
{
   // Add dependent Resources to list  
   %this.addDependentResources();
   
   // Clear unused resource list
   projectAvailableResources.clearItems();
   
   // Add All unused resources
   ResourceFinder::enumResources( %this @ ".addResource" );
}

function projectOptionsDlg::addResource( %this, %resource )
{
   if( projectDependentResourcesList.findItemText( %resource ) == -1 &&
       projectAvailableResources.findItemText( %resource ) == -1 )
      projectAvailableResources.addItem( %resource );
}

function projectOptionsDlg::addDependentResources( %this )
{
   projectDependentResourcesList.clearItems();
   // Load Resources used by this project.
   if( isObject( $dependentResourceGroup ) && $dependentResourceGroup.getCount() > 0 )
   {
      for( %i = 0; %i < $dependentResourceGroup.getCount(); %i++ )
      {
            %resourceLink = $dependentResourceGroup.getObject( %i );
            if( !isObject( %resourceLink ) )
               continue;
            
            // Resource Name Exists
            if( %resourceLink.Name !$= "" && projectDependentResourcesList.findItemText( %resourceLink.Name) == -1 )
               projectDependentResourcesList.addItem( %resourceLink.Name );
      }
   }   
}

// Save project resource list and apply
function saveProjectResourcesButton::onClick( %this )
{
   // First unload any dependent resources that were removed.
   if( isObject( $dependentResourceGroup ) && $dependentResourceGroup.getCount() > 0 )
   {
      %count = $dependentResourceGroup.getCount();
      %resource = 0;
      for( %i = 0; %i < %count; %i++ )
      {
         %resourceLink = $dependentResourceGroup.getObject( %resource );
         if( !isObject( %resourceLink ) )
            continue;
         
         // Resource Name Exists
         %listIndex = projectAvailableResources.findItemText( %resourceLink.Name );
         %validResource = ResourceFinder::isActive( %resourceLink.Name );
         if( %validResource == true && %listIndex != -1 )
         {
            ResourceObject::Unload( %resourceLink.Name );
            //projectDependentResourcesList.deleteItem( %listIndex );
         }
         else
            %resource++;
      }
   }
   
   // Load new dependencies.
   %listCount = projectDependentResourcesList.getItemCount();
   for( %i = 0; %i < %listCount; %i++ )
   {
      %word = projectDependentResourcesList.getItemText( %i );
      if( %word !$= "" && !ResourceFinder::isActive( %word ) )
      {
         %result = ResourceObject::load( $currentProject, %word );
         if( isObject( %result ) )
         {
            %resNameLink = new ScriptObject()
            {
               Name = %result.Name;
            };
            
            LBProjectObj.addResource( %resNameLink, false );
         }
      }
   }
   
   
   // Persist Resources
   LBProjectObj.persistToDisk( false, true, false );
   
   // Pop Resource Dialog
   Canvas.popDialog( projectOptionsDlg );
   
}

// User canceled, merely cleanup and exit
function cancelProjectResourcesButton::onClick( %this )
{
   Canvas.popDialog( projectOptionsDlg );
}

// Add an inactive resource to the projects used resources list
function setResourceActiveButton::onClick( %this )
{
   // Get selected item.
   %selItem = projectAvailableResources.getSelectedItem();
   if( %selItem != -1 ) 
   {
      // Grab Resource Name
      %resName = projectAvailableResources.getItemText( %selItem );
      
      // Remove from active resource list.
      projectAvailableResources.deleteItem( %selItem );   
      
      // Add to used list.
      if( projectDependentResourcesList.findItemText( %resName ) == -1 )
         projectDependentResourcesList.addItem( %resName );
   }
   
}

// Remove an active resource from the project and move to unused list
function setResourceInactiveButton::onClick( %this )
{
   // Get selected item.
   %selItem = projectDependentResourcesList.getSelectedItem();
   if( %selItem != -1 ) 
   {
      // Grab Resource Name
      %resName = projectDependentResourcesList.getItemText( %selItem );
      
      // Remove from active resource list.
      projectDependentResourcesList.deleteItem( %selItem );   
      
      // Add to used list.
      if( projectAvailableResources.findItemText( %resName ) == -1 )
         projectAvailableResources.addItem( %resName );
   }
}


function projectAvailableResources::onDoubleClick( %this )
{
   setResourceActiveButton.onClick();
}

function projectDependentResourcesList::onDoubleClick( %this )
{
   setResourceInactiveButton.onClick();
}
