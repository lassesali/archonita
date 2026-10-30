//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------
$currentProject = "";

function loadStartupProject()
{
   if ($startupProject !$= "")
      %game = $startupProject;
   else
      %game = $pref::startupProject;
      
   if ((!isFile(%game @ "/main.cs")) && (!isFile(%game @ "/main.cs.dso")))
      %game = $defaultGame;
   

   // Load Directory
   loaddir(%game);
   
   
   // Load Resources used by this project. Must be done before datablocks and persistent objects are loaded.
   if( isFile( %game @ "/managed/resources.cs" ) || isFile( %game @ "/managed/resources.cs.dso" ) )
   {
      $dependentResourceGroup = 0;
      exec( %game @ "/managed/resources.cs" );
      if( isObject( $dependentResourceGroup ) && $dependentResourceGroup.getCount() > 0 )
      {
         for( %i = 0; %i < $dependentResourceGroup.getCount(); %i++ )
         {
            %resourceLink = $dependentResourceGroup.getObject( %i );
            if( !isObject( %resourceLink ) )
               continue;
            
            // Resource Name Exists
            if( %resourceLink.Name !$= "" )
            {
               %res = ResourceObject::load( %game, %resourceLink.Name );
               if( !isObject( %res ) )
                  error("Error : Loading Resource" SPC %resourceLink.Name SPC "for project" SPC %game );
            }
         }
      }
   }     
     
   // Load managed project datablocks
   if ((isFile(%game @ "/managed/datablocks.cs")) || (isFile(%game @ "/managed/datablocks.cs.dso")))
      exec(%game @ "/managed/datablocks.cs");
   
   // Load managed persistent object set
   if ((isFile(%game @ "/managed/persistent.cs")) || (isFile(%game @ "/managed/persistent.cs.dso")))
      exec(%game @ "/managed/persistent.cs");
   
   // Load up keybinds.
   activatePackage(KeybindPackage);
   loadKeybindings();
   deactivatePackage(KeybindPackage);
   
   if (isFile("~/preferences/bind.cs"))
      exec("~/preferences/bind.cs");
   
   if (isFile(%project @ "/managed/bind.cs"))
      exec(%project @ "/managed/bind.cs");
      
   $currentProject = %game;
   
   initializeProject();
   
}

function exitDemo()
{
   if($runWithEditors)
      toggleLevelEditor();
   else
      quit();
}