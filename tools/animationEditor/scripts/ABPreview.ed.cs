function ABPreview::setupPreview(%this)
{
   if(!isObject(%this.previewAnimSprite))      
   {
      %this.previewAnimSprite = new t2dAnimatedSprite(previewAnimSprite)
      {
         sceneGraph = $AnimationBuilder::sceneGraph;
         class = ABPreviewAnim;
      };
      
      %this.previewAnimSprite.setFrameChangeCallback(true);
      
      %animDB = %this.previewAnimDB;
      %imageMap = %animDB.imageMap;
      
      if(%imageMap.getImageMapMode() $= "FULL")
      {
         %imageSize = %imageMap.getSrcBitmapSize();
      } else
      {
         %imageSize = %imageMap.getFrameSize(getWord(%animDB.animationFrames, 0));
      }
   
      %width = getWord(%imageSize, 0);
      %height = getWord(%imageSize, 1);
      
      if(%width > %height)
      {
         %this.previewAnimSprite.heightRatio = %height / %width;
         %this.previewAnimSprite.widthRatio = 0.75;     
      } else if(%height > %width)
      {
         %this.previewAnimSprite.heightRatio = 1; 
         %this.previewAnimSprite.widthRatio = (%width / %height) * 0.75;
      } else
      {
         %this.previewAnimSprite.heightRatio = 1;
         %this.previewAnimSprite.widthRatio = 0.75;
      }
      
      $AnimationBuilder::ImagePreviewGroup.add(%this.previewAnimSprite);
   }
   
   %this.playAnim();
   %this.previewAnimSprite.setPaused(true);
   
   %this.reset();
   
   if(!%this.isWindowActive())
      %this.loadWindow();
}

function ABPreview::loadWindow(%this)
{
   %this.isWindowActive = true;
   %this.setVisible(true);
   
   %targetPos = $AnimationBuilder::previewWindowPosition;
   %targetSize = $AnimationBuilder::previewWindowSize;
   
   %loadSteps = $AnimationBuilder::preview::loadSteps;
   %time = $AnimationBuilder::preview::time;
   
   %this.setAlpha(0);
   %this.fadeToAlpha(%time, 1, %loadSteps);
   %this.resizeWindow(%time, %loadSteps, %targetPos, %targetSize, true);
}

function ABPreview::resizeWindow(%this, %time, %loadSteps, %targetPos, %targetSize, %visible)
{   
   %targetPosX = getWord(%targetPos, 0);
   %targetPosY = getWord(%targetPos, 1);
   
   %targetWidth = getWord(%targetSize, 0);
   %targetHeight = getWord(%targetSize, 1);
   
   %startPos = %this.getPosition();
   %startSize = %this.getSize();
   
   %startPosX = getWord(%startPos, 0);
   %startPosY = getWord(%startPos, 1);
   
   %startWidth = getWord(%startSize, 0);
   %startHeight = getWord(%startSize, 1);
   
   %diffX = %targetPosX - %startPosX;
   %diffY = %targetPosY - %startPosY;
   
   %diffWidth = %targetWidth - %startWidth;
   %diffHeight = %targetHeight - %startHeight;
   
   %incX = %diffX / %loadSteps;
   %incY = %diffY / %loadSteps;
   %incWidth = %diffWidth / %loadSteps;
   %incHeight = %diffHeight / %loadSteps;
   
   %timeInc = (%time * 100) / %loadSteps;
   
   %this.loadingWindow(%timeInc, %loadSteps, %incX, %incY, %incWidth, %incHeight, %visible);   
}

function ABPreview::loadingWindow(%this, %timeInc, %loadSteps, %incX, %incY, %incWidth, %incHeight, %visible, %count)
{
   if(%count $= "")
      %count = 0;
   
   %pos = %this.getPosition();
   %size = %this.getSize();
   
   %posX = getWord(%pos, 0);
   %posY = getWord(%pos, 1);
   
   %width = getWord(%size, 0);
   %height = getWord(%size, 1);
   
   %posX += %incX;
   %posY += %incY;
   
   %width += %incWidth;
   %height += %incHeight;
   
   %this.setPosition(%posX, %posY);
   %this.setSize(%width, %height);
   %this.previewAnimSprite.setPosition(%posX, %posY);
   %this.previewAnimSprite.setSize((%height * %this.previewAnimSprite.widthRatio) * 0.95, (%height * %this.previewAnimSprite.heightRatio) * 0.95);

   %this.positionBorders();
   
   %count++;
   
   if(%count < %loadSteps)
   {
      %this.loadWindowSchedule = %this.schedule(%timeInc, "loadingWindow", %timeInc, %loadSteps, %incX, %incY, %incWidth, %incHeight, %visible, %count);
   } else
   {
      %this.onWindowLoaded(%visible);      
   }
}

function ABPreview::onWindowLoaded(%this, %visible)
{
   if(%visible)
   {
      %size = %this.getSize();
      %width = getWord(%size, 0);
      %height = getWord(%size, 1);
   
      %width *= %this.previewAnimSprite.widthRatio;
      %height *= %this.previewAnimSprite.heightRatio;
   
      %width *= 0.95;
      %height *= 0.95;
       
      %this.previewAnimSprite.setSize(%width, %height);
      %this.previewAnimSprite.setPaused(false);
   } else 
   {
      $AnimationBuilder::previewOpen = false;
      %this.setVisible(false);
      %this.previewAnimSprite.safeDelete();
      %this.previewAnimDB.delete();
   }
}

function ABPreview::closePreviewWindow(%this)
{
   %this.isWindowActive = false;

   %targetPos = $AnimationBuilder::preview::startPos;
   %targetSize = $AnimationBuilder::preview::startSize;

   %loadSteps = $AnimationBuilder::preview::loadSteps;
   %time = $AnimationBuilder::preview::time;
   
   %this.fadeToAlpha(%time, 0, %loadSteps);
   %this.resizeWindow(%time, %loadSteps, %targetPos, %targetSize, false);

   AnimationBuilder::hidePreviewControls();
}

function ABPreview::isWindowActive(%this)
{
   return %this.isWindowActive;   
}

function ABPreview::reset(%this)
{
   %this.paused = false;
   %this.playing = false;
   %this.stopped = false;      
}

function ABPreview::playButton(%this)
{
   if(%this.paused)
   {
      %this.previewAnimSprite.setPaused(false);
   } else
   {
      %this.previewAnimSprite.setPaused(false);
      %this.playAnim();
   }
   
   %this.reset();
   %this.playing = true;
}

function ABPreview::pauseButton(%this)
{
   if(%this.paused)
   {
      %this.previewAnimSprite.setPaused(false);  
      %this.reset();
      %this.playing = true;
   } else if(!%this.stopped)
   {
      %this.previewAnimSprite.setPaused(true);   
      %this.reset();
      %this.paused = true;
   }
}

function ABPreview::stopButton(%this)
{
   %this.reset();
   %this.stopped = true;
   %this.playAnim();
   %this.previewAnimSprite.setPaused(true);   
}

function ABPreview::playAnim(%this, %frame)
{
   if(%frame $= "")
      %frame = 0;
   
   %slider = AnimationBuilderPreviewSlider;
   
   %slider.setValue(%frame);
      
   %this.previewAnimSprite.playAnimation(%this.previewAnimDB, false, %frame);
}

function ABPreview::createPreviewAnim(%this, %name)
{
   if(!$AnimationBuilder::allFrames)
	  {
      %objList = $AnimationBuilder::storyboardWindow.objList;
   
      %count = %objList.getCount() - 1;
   
      %firstFrame = 0;
   
      for(%i=0;%i<%count;%i++)
      {
         %obj = %objList.getObject(%i);
      
         if(%obj.getClassName() !$= "t2dStaticSprite")
         {
            %firstFrame++;
            continue;
         }
         
         %frame = %obj.getFrame();
      
         if(%i == %firstFrame)
         {
            %frameList = %frame;         
         } else
         {
            %frameList = %frameList SPC %frame;
         }
      }
	  } else
	  {
      //%frameList = "\"\"";	     
      %frameList = "";
	  }
	  
   %imageMap = $AnimationBuilder::selectedImageMap;
   
   %time = animationDatablockTime.getValue();
   %cycle = animationDatablockCycleMode.getValue();
   %random = animationDatablockRandomStart.getSelected();
   
   if(%name $= "")
      %name = previewAnimDB;
   
   %eval = "datablock t2dAnimationDatablock(" @ %name @ ")\n";
   %eval = %eval @ "{\n";
   %eval = %eval @ "   imageMap = " @ %imageMap @ ";\n";
   %eval = %eval @ "   animationFrames = \"" @ %frameList @ "\";\n";
   %eval = %eval @ "   animationTime = " @ %time @ ";\n";
   %eval = %eval @ "   animationCycle = " @ %cycle @ ";\n";
   %eval = %eval @ "   randomStart = " @ %random @ ";\n";
   %eval = %eval @ "};";
   
   eval(%eval);
   
   %this.previewAnimDB = %name.getID();
   
   return %name.getID();
}

function ABPreview::fadeToAlpha(%this, %time, %alpha, %inc, %callback)
{
   // store the starting color
   %this.startAlphaBeforeFade = %this.getBlendAlpha();
   
   // grab the starting colors
   %startAlpha = %this.startAlphaBeforeFade;    

   // if our target alpha and our present alpha is the same then we need to do nothing
   if(%startAlpha == %alpha)
      return;
 
   // if we are already doing a color change then cancel it out
   if(isEventPending(%this.alphaFadeSchedule))
      cancel(%this.alphaFadeSchedule);
   
   // how many time incriments do we want
   if(%inc $= "" || %inc < 1 || %inc > 100)
      %inc = 10;
   
   // divide out what each time inc will be
   %timeInc = (%time * 100) / %inc;
   
   // get the different between starting and ending colors
   %alphaDiff = %alpha - %startAlpha;
   
   // get the ammount we must inc each color each scheduled change
   %alphaInc = %alphaDiff / %inc;
   
   // call the color change
   %this.fadingAlpha(%inc, %timeInc, %alphaInc, "", %callback);
}

function ABPreview::fadingAlpha(%this, %inc, %timeInc, %alphaInc, %count, %callback)
{
   // init the count if not specified
   if(%count $= "")
      %count = 0;
   
   // grab the current color
   %alpha = %this.getBlendAlpha();
   
   // divide up the colors and inc them
   %alpha = %alpha + %alphaInc;      
   
   // set the new colors
   %this.setAlpha(%alpha);
   
   // inc the count
   %count++;
   
   // if the count is less than the planned incs then schedule this to be called again
   if(%count < %inc)
   {
      %this.alphaFadeSchedule = %this.schedule(%timeInc, "fadingAlpha", %inc, %timeInc, %alphaInc, %count, %callback);
   } else
   {   
      schedule(0, %this, "eval", %callback);
   }
}

function ABPreview::setAlpha(%this, %alpha)
{
   %this.setBlendAlpha(%alpha);
   %this.background.setBlendAlpha(%alpha);   
   %this.leftBorder.setBlendAlpha(%alpha);   
   %this.rightBorder.setBlendAlpha(%alpha);
   %this.topBorder.setBlendAlpha(%alpha);
   %this.bottomBorder.setBlendAlpha(%alpha);
   %this.previewAnimSprite.setBlendAlpha(%alpha);
}