
// Create Resource Descriptor
$instantResource = new ScriptObject()
{
   Class = "Tank";
   Name = "Tank";
   User = "TGB";
   LoadFunction = "Tank::LoadResource";
   UnloadFunction = "Tank::UnloadResource";
};

// Load Resource Function - Hooks into game
function Tank::LoadResource( %this )
{
   exec("./PhysTank.cs");
}


// Unload Resource Function - Remove from game Sim.
function Tank::UnloadResource( %this )
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
   
   new t2dImageMapDatablock(tankChassisImageMap) {
      canSaveDynamicFields = "1";
      imageName = "./images/tankChassis";
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
      defaultConfig = "PhysTankDatablock";
   };
   new t2dImageMapDatablock(tankTurretImageMap) {
      canSaveDynamicFields = "1";
      imageName = "./images/tankTurret";
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
      defaultConfig = "PhysTankTurretDatablock";
   };
};