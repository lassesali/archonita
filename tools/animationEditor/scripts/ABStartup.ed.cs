function launchAnimationEditor()
{
   // Make sure our necessaries exist.
   AnimationBuilder::Init();
   
   // Reset the editing state
   // Editing should only be true when launched though 
   // launchEditAnimation()
   $AnimationBuilder::edit = false;
   $AnimationBuilder::editID = 0;
   
   ABDeleteAnimationButton.setVisible(false);
   
   AnimationBuilderUI.setVisible(false);
   
	  Canvas.pushDialog( AnimationEditorGUI );
   
   AnimationBuilderSelectImageWindow.setVisible(true);
   
   loadAnimationBuilderSelection();
}

function AnimationBuilderUI::onStartUp( %this )
{
   $AnimationBuilder::allFrames = false;
   
   AnimationBuilder::hidePreviewControls();
   $AnimationBuilder::previewWindow.isWindowActive = false;
   $AnimationBuilder::previewOpen = false;
   
   AnimationBuilderSelectImageWindow.setVisible(false);
   AnimationBuilderUI.setVisible(true);
   
   AnimationBuilder::initCanvas();
   AnimationBuilder::loadSelectionFrames();
   AnimationBuilder::initStoryboard();
   
   %this.setupDatablockOptions();
}

function AnimationBuilderUI::setupDatablockOptions( %this )
{
   // Locate Children.
   %datablockContainer = %this.findObjectByinternalName("datablockContainer");
   %animationControl = %datablockContainer.findObjectByInternalName("AnimationControl");
   %randomStart        = %animationControl.findObjectByinternalName("randomStart");
   %animationCycle     = %animationControl.findObjectByinternalName("animationCycle");

   // Setup Datablock Options
   %randomStart.clear();
   %randomStart.add("False", 0);
   %randomStart.add("True", 1);
   %randomStart.setSelected(0);   

   %animationCycle.setValue(1);
}
 
function AnimationBuilder::useAllFrames()
{
   %allFrames = AnimationBuilderChkUseAllFrames.getValue();

   if(%allFrames)
   {
      if(!$AnimationBuilder::alreadyDone)
      {
         AnimationBuilder::loadDefaultFrames($selectedImageMap);
         $AnimationBuilder::alreadyDone = true;            
         $AnimationBuilder::addedFrame = false;
      }
   } else
   {
      $AnimationBuilder::alreadyDone = false;  
      if(!$AnimationBuilder::addedFrame)
      {
         $AnimationBuilder::storyboardWindow.addStoryBoardFrame();   
         $AnimationBuilder::addedFrame = true;         
      }
   }
   
   $AnimationBuilder::allFrames = %allFrames;
   
   $AnimationBuilder::storyboardWindow.updateFramesText();
}