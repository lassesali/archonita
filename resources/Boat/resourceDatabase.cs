
// Create Resource Descriptor
$instantResource = new ScriptObject()
{
   Class = "Boat";
   Name = "Boat";
   User = "TGB";
   LoadFunction = "Boat::LoadResource";
   UnloadFunction = "Boat::UnloadResource";
};

// Load Resource Function - Hooks into game
function Boat::LoadResource( %this )
{
   exec("./PhysBoat.cs");
}


// Unload Resource Function - Remove from game Sim.
function Boat::UnloadResource( %this )
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
   new t2dImageMapDatablock(speedboatImageMap) {
      canSaveDynamicFields = "1";
      imageName = "./images/speedboat";
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
      defaultConfig = "PhysBoatDatablock";
   };
};