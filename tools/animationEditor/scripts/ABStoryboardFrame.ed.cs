function ABStoryboardFrame::onMouseUp(%this, %pos)
{    
   if($AnimationBuilder::following)
   {
      // this means we are dropping a frame on the story board frame
      %image = new t2dStaticSprite(StoryboardImageFrame)
      {
         sceneGraph = $AnimationBuilder::sceneGraph;
         class = ABStoryBoardFrame;   
         upResponse = true;      
      };
      
      %image.setImageMap($AnimationBuilder::followMouseObj.getImageMap(), $AnimationBuilder::followMouseObj.getFrame());
      %image.setSize($AnimationBuilder::followMouseObj.getSize());
      
      $AnimationBuilder::storyboardWindow.changeFrameImage(%image, %this.index);            
   } else
   {
      // this means we are selecting a frame
      $AnimationBuilder::storyboardWindow.selectFrame(%this);
   }
}

function ABStoryboardFrame::onSelected(%this)
{
   %this.stopFading = false;
   %this.fadeToTransparent($AnimationBuilder::time::selectionPulseTime);   
}

function ABStoryboardFrame::onDeSelected(%this)
{
   %this.stopFade();   
}

function ABStoryboardFrame::stopFade(%this)
{
   %this.stopFading = true;   
}

function ABStoryboardFrame::fadeToTransparent(%this, %time)
{
   if(%this.stopFading)
      return;
   
   %this.leftLine.fadeToAlpha(%time, 0, 20, %this @ "." @ "fadeToVisible(" @ %time @ ");");      
   %this.rightLine.fadeToAlpha(%time, 0, 20, %this @ "." @ "fadeToVisible(" @ %time @ ");");
   %this.topLine.fadeToAlpha(%time, 0, 20, %this @ "." @ "fadeToVisible(" @ %time @ ");");
   %this.bottomLine.fadeToAlpha(%time, 0, 20, %this @ "." @ "fadeToVisible(" @ %time @ ");");
}

function ABStoryboardFrame::fadeToVisible(%this, %time)
{      
   %this.leftLine.fadeToAlpha(%time, 1, 20, %this @ "." @ "fadeToTransparent(" @ %time @ ");");
   %this.rightLine.fadeToAlpha(%time, 1, 20, %this @ "." @ "fadeToTransparent(" @ %time @ ");");
   %this.topLine.fadeToAlpha(%time, 1, 20, %this @ "." @ "fadeToTransparent(" @ %time @ ");");
   %this.bottomLine.fadeToAlpha(%time, 1, 20, %this @ "." @ "fadeToTransparent(" @ %time @ ");");
}