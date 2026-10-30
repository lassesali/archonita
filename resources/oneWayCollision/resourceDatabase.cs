
// Create Resource Descriptor
$instantResource = new ScriptObject()
{
   Class = "oneWayCollision";
   Name = "oneWaycollision";
   User = "TGB";
   LoadFunction = "oneWayCollision::LoadResource";
   UnloadFunction = "oneWayCollision::UnloadResource";
};

// Load Resource Function - Hooks into game
function oneWayCollision::LoadResource( %this )
{
   exec("./scripts.cs");
}


// Unload Resource Function - Remove from game Sim.
function oneWayCollision::UnloadResource( %this )
{
   // We must clean up all the mess we've made in the Sim.
   if( isObject( %this.Data ) && %this.Data.GetCount() > 0 )
   {      
      while( %this.Data.getCount() > 0 )
      {
         %datablockObj = %this.Data.getObject( 0 );
         %this.Data.remove( %datablockObj );
         if( isObject( %datablockObj ) )
            %datablockObj.delete();
      }
   }
}

// Resource Data
$instantResource.Data = new SimGroup() 
{
   canSaveDynamicFields = "1";

   new t2dSceneObjectDatablock(OneWayPlatform)
   {
      class = "Platform";
      CollisionActiveReceive = "1";
      Immovable = "1";
   };
   new t2dSceneObjectDatablock(PlayerConfig)
   {
      class = "Player";
      CollisionActiveSend = "1";
      ConstantForce = "0 250";
      ConstantForceGravitic = "1";
      moveSpeed = "25";
      jumpHeight = "15";
   };
   new t2dImageMapDatablock(PlayerImageMap) {
      canSaveDynamicFields = "1";
      imageName = "./images/player";
      imageMode = "FULL";
      frameCount = "-1";
      filterMode = "SMOOTH";
      filterPad = "1";
      preferPerf = "0";
      cellRowOrder = "1";
      cellOffsetX = "0";
      cellOffsetY = "0";
      cellStrideX = "0";
      cellStrideY = "0";
      cellCountX = "-1";
      cellCountY = "-1";
      cellWidth = "0";
      cellHeight = "0";
      preload = "1";
      allowUnload = "0";
      defaultConfig = "PlayerConfig";
   };
   new t2dImageMapDatablock(PlatformImageMap) {
      canSaveDynamicFields = "1";
      imageName = "./images/platform";
      imageMode = "FULL";
      frameCount = "-1";
      filterMode = "SMOOTH";
      filterPad = "0";
      preferPerf = "0";
      cellRowOrder = "1";
      cellOffsetX = "0";
      cellOffsetY = "0";
      cellStrideX = "0";
      cellStrideY = "0";
      cellCountX = "-1";
      cellCountY = "-1";
      cellWidth = "0";
      cellHeight = "0";
      preload = "1";
      allowUnload = "0";
      defaultConfig = "OneWayPlatform";
   };
};
