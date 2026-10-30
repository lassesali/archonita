function ABFrameSet::addObject(%this, %obj)
{
   %count = %this.objList.getCount();
 
   %this.objList.add(%obj);
   
   %pos = %this.scrollBase.getPosition();
   %posX = getWord(%pos, 0);
   %posY = getWord(%pos, 1);
   
   %size = %this.scrollBase.getSize();
   %sizeX = getWord(%size, 0);
   %sizeY = getWord(%size, 1);
   
   %frameHeight = $AnimationBuilder::sizes::frameHeight;

   %newPosX = %posX + $AnimationBuilder::position::startFrameOffsetX;
   %newPosY = $AnimationBuidler::position::startContentOffset + (%posY + (%count * (%frameHeight + $AnimationBuilder::sizes::framePadding))) - (%sizeY / 2) + (%frameHeight / 2);
   
   if(%obj.getImageMap().getImageMapMode() $= "FULL")
   {
      %imageSize = %obj.getImageMap().getSrcBitmapSize();
   } else
   {
      %imageSize = %obj.getImageMap().getFrameSize(%obj.getFrame());
   }
   
   %width = getWord(%imageSize, 0);
   %height = getWord(%imageSize, 1);
      
   if(%width > %height)
   {
      %ratio = %height / %width;
         
      %width = %frameHeight;
      %height = %frameHeight * %ratio;         
   } else
   {
      %ratio = %width / %height;
        
      %width = %frameHeight * %ratio;
      %height = %frameHeight; 
   }
   
   %obj.setSize(%width, %height);
   %obj.setLayer($AnimationBuilder::layers::scrollContent);
   %this.addFrame(%obj);
   %this.addText(%obj);
   
   %obj.mount(%this.scrollBase, %this.scrollBase.getLocalPoint(%newPosX, %newPosY));
   
   %count = %this.objList.getCount();
   %contentSize = 0.5 + %count * ($AnimationBuilder::sizes::frameHeight + $AnimationBuilder::sizes::framePadding);
   %this.setContentSize(%contentSize);
   %this.scrollBar.positionBar();
   
   $AnimationBuilder::StoryboardGroup.add(%obj);
}

function ABFrameSet::addText(%this, %obj)
{
   %size = "10 1.6";
   
   %fontObj = new t2dSceneObject()
   {
      scenegraph = %obj.scenegraph;      
      class = ABFont;
      size = %size;
   };

   %pos = %obj.bottomLine.getPosition();
   %posX = getWord(%pos, 0);
   %posY = getWord(%pos, 1);
   
   %objSize = %obj.getSize();
   %objSizeX = getWord(%objSize, 0);
   %objSizeY = getWord(%objSize, 1);

   %posX += 1;
   %posY += 1;

   %fontObj.mount(%obj, %obj.getLocalPoint(%posX, %posY));
   %fontObj.buildText("frame: " @ %obj.getFrame(), %size, false);

   $AnimationBuilder::StoryboardGroup.add(%fontObj);
}