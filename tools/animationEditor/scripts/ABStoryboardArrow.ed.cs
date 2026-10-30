function ABStoryboardArrow::onMouseUp(%this, %pos)
{   
   if($AnimationBuilder::following)
   {
      // this means we are dropping a frame on the story board frame
      %image = new t2dStaticSprite()
      {
         sceneGraph = $AnimationBuilder::sceneGraph;
         class = ABStoryBoardFrame;   
         upResponse = true;      
      };
      
      %image.setImageMap($AnimationBuilder::followMouseObj.getImageMap(), $AnimationBuilder::followMouseObj.getFrame());
      %image.setSize($AnimationBuilder::followMouseObj.getSize());
      
      $AnimationBuilder::storyboardWindow.insertFrameImage(%image, %this.index);            
   } else
   {
      // this means we are selecting a frame
      $AnimationBuilder::storyboardWindow.selectFrame($AnimationBuilder::storyboardWindow.objList.getObject(%this.index));
   }
}

function ABStoryboardArrow::onMouseMove(%this, %pos)
{
   
}

function ABStoryboardArrow::onMouseOver(%this, %pos)
{
   
}

function ABStoryboardArrow::onMouseDown(%this, %pos)
{
   
}

function ABStoryboardArrow::onMouseDragged(%this, %pos)
{
   %this.setBlendColor($AnimationBuilder::colors::arrowOver);   
}

function ABStoryboardArrow::onMouseLeave(%this, %pos)
{ 
   %this.setBlendColor($AnimationBuilder::colors::arrowNormal);
}

