function launchEditAnimation(%anim)
{
   $AnimationBuilder::edit = true;
   $AnimationBuilder::editID = %anim.getID();
   AnimationBuilder::Init();
   
   ABDeleteAnimationButton.setVisible(true);
   AnimationBuilderUI.setVisible(true);
	  Canvas.pushDialog( AnimationEditorGUI );
   
   AnimationBuilder::hidePreviewControls();
   AnimationBuilderSelectImageWindow.setVisible(false);
   AnimationBuilderUI.setVisible(true);
   
   AnimationBuilderUI.setupDatablockOptions();
   
   AnimationBuilder::initCanvas();
   AnimationBuilder::loadAnimation(%anim);
   AnimationBuilder::initStoryboard();
}

function AnimationBuilder::loadAnimation(%anim)
{
   $AnimationBuilder::animName = %anim.getName();
   
   %animFrames = %anim.animationFrames;
   %frameCount = getWordCount(%animFrames);
   
   %imageMap = %anim.imageMap;
   
   %time = %anim.animationTime;
   %cycle = %anim.animationCycle;
   %random = %anim.randomStart;
   
   animationDatablockTime.setValue(%time);
   animationDatablockCycleMode.setValue(%cycle);
   animationDatablockRandomStart.setSelected(%random);
   
   $AnimationBuilder::selectedImageMap = %imageMap;
   
   AnimationBuilder::loadSelectionFrames();

   if(%anim.animationFrames $= "")
   {
      AnimationBuilderChkUseAllFrames.setValue(true);
      AnimationBuilder::useAllFrames();
   } else
   {
      for(%i=0;%i<%frameCount;%i++)
      {
         %frame = getWord(%animFrames, %i);      
      
         %obj = new t2dStaticSprite()
         {
            sceneGraph = $AnimationBuilder::SceneGraph; 
            class = ABStoryboardFrame;  
            upResponse = true;   
         };
 
         %obj.setImageMap(%imageMap, %frame);
   
         $AnimationBuilder::StoryboardWindow.addObject(%obj);      
      }
   }
   
   if(%anim.animationFrames !$= "")
   {
      // Insert an empty frame to start editing
      $AnimationBuilder::storyboardWindow.addStoryBoardFrame();
   }
}

function AnimationBuilder::loadDefaultFrames()
{
   %imageMap = $AnimationBuilder::selectedImageMap;
   
   %frameCount = %imageMap.getFrameCount();
   
   $AnimationBuilder::storyboardWindow.clearObjects(true);
   
   for(%i=0;%i<%frameCount;%i++)
   {

      %obj = new t2dStaticSprite()
      {
         sceneGraph = $AnimationBuilder::SceneGraph; 
         class = ABStoryboardFrame;  
         upResponse = true;   
      };
 
      %obj.setImageMap(%imageMap, %i);
   
      $AnimationBuilder::StoryboardWindow.addObject(%obj);      
   }      
}