function ABFrameSetFrame::onMouseUp(%this, %pos)
{
   
}

function ABFramesetFrame::onMouseDown(%this, %pos)
{
   $AnimationBuilder::storyboardWindow.deSelectFrame();
   
   %imageMap = %this.getImageMap();
   %frame = %this.getFrame();
   
   %sprite = new t2dStaticSprite(FramesetImage)
   {
      sceneGraph = $AnimationBuilder::sceneGraph;      
   };
   
   %sprite.setBlendAlpha($AnimationBuilder::colors::dragImageAlpha);
   %sprite.setSize(%this.getSize());
   %sprite.setImageMap(%imageMap, %frame);
   %sprite.setPosition(%pos);
   
   AnimationBuilder::followMouse(%sprite);
   
   $AnimationBuilder::StoryboardGroup.add(%sprite);
}