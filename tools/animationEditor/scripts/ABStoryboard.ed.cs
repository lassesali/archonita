function ABStoryboard::addObject(%this, %obj, %arrow)
{
   if(%arrow $= "")
      %arrow = true;
   
   %colCount = $AnimationBuilder::storyboard::columnCount;
   
   %count = %this.objList.getCount();
   %spacingMult = mFloor(%count / %colCount);
 
   %colSpot = %count - (%colCount * %spacingMult) - 1;
   %rowSpot = %spacingMult;
 
   %this.objList.add(%obj);
   
   %pos = %this.scrollBase.getPosition();
   %posX = getWord(%pos, 0);
   %posY = getWord(%pos, 1);
   
   %size = %this.scrollBase.getSize();
   %sizeX = getWord(%size, 0);
   %sizeY = getWord(%size, 1);
   
   %frameHeight = $AnimationBuilder::sizes::frameHeight;
   %frameSpacing = $AnimationBuilder::sizes::frameSpacing;
   %framePadding = $AnimationBuilder::sizes::framePadding;

   %newPosX = $AnimationBuidler::position::startContentOffsetCol + (%colSpot * (%frameHeight + %frameSpacing)) - (%sizeY / 2) + (%frameHeight / 2);
   %newPosY = $AnimationBuidler::position::startContentOffset + (%posY + (%rowSpot * (%frameHeight + %framePadding))) - (%sizeY / 2) + (%frameHeight / 2);
   
   if(%obj.getImageMap() !$= "")
   {
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
      
      %noImage = false;
   } else
   {
      %width = %frameHeight;      
      %height = %frameHeight;
      %noImage = true;
   }
   
   %obj.index = %count;
   %obj.setSize(%width, %height);
   %obj.setLayer($AnimationBuilder::layers::scrollContent);
   %this.addFrame(%obj);
   
   if(!%noImage)
      %this.addText(%obj, %count);
   
   if(%arrow && %count != 0)
   {
      %arrowObj = %this.createArrow();
      %arrowObj.index = %count;
      
      if(!(%count % 3))
      {
         %arrowX = $AnimationBuidler::position::startContentOffsetCol + (1 * (%frameHeight + %frameSpacing)) - (%sizeY / 2) + %frameHeight + (%frameSpacing / 2);
         %arrowY = %newPosY - %frameHeight - %framePadding;   
      } else
      {
         %arrowX = %newPosX - (%frameHeight / 2) - (%frameSpacing / 2);
         %arrowY = %newPosY;      
      }
      
      %obj.arrow = %arrowObj;
      
      %arrowObj.mount(%this.scrollBase, %this.scrollBase.getLocalPoint(%arrowX, %arrowY));
      
      $AnimationBuilder::StoryboardGroup.add(%arrowObj);
   }
   
   %obj.mount(%this.scrollBase, %this.scrollBase.getLocalPoint(%newPosX, %newPosY));
   
   %count = %this.objList.getCount();
   %contentSize = 0.5 + (%rowSpot + 1) * ($AnimationBuilder::sizes::frameHeight + $AnimationBuilder::sizes::framePadding);
   
   %this.setContentSize(%contentSize);
   %this.scrollBar.positionBar();
   %this.positionScrollBase();
   
   $AnimationBuilder::StoryboardGroup.add(%obj);
}

function ABStoryboard::addText(%this, %obj, %num)
{
   %size = "17 1.6";
   
   %fontObj = new t2dSceneObject()
   {
      scenegraph = %obj.scenegraph;      
      class = ABFont;
      size = %size;
   };
   
   %obj.fontObj = %fontObj;

   %pos = %obj.bottomLine.getPosition();
   %posX = getWord(%pos, 0);
   %posY = getWord(%pos, 1);
   
   %objSize = %obj.getSize();
   %objSizeX = getWord(%objSize, 0);
   %objSizeY = getWord(%objSize, 1);

   %posX += 0.8;
   %posY += 1;

   %fontObj.mount(%obj, %obj.getLocalPoint(%posX, %posY));
   
   %numLen = strLen(%num);
   
   %fontObj.buildText(%num + 1 @ "    frame: " @ %obj.getFrame(), %size, true, %numLen);
   
   $AnimationBuilder::StoryboardGroup.add(%fontObj);
}

function ABStoryboard::clearObjects(%this, %deleteSource)
{
   %objCount = %this.objList.getCount();
   
   for(%i=0;%i<%objCount;%i++)
   {
      %obj = %this.objList.getObject(%i);
      
      %obj.leftLine.safeDelete();      
      %obj.rightLine.safeDelete();
      %obj.topLine.safeDelete();
      %obj.bottomLine.safeDelete();
      
      if(isObject(%obj.fontObj))
         %obj.fontObj.clearText();
      
      if(isObject(%obj.arrow))
         %obj.arrow.safeDelete();
         
      if(%deleteSource)
         %obj.safeDelete();
   }
   
   %this.objList.clear();
}

function ABStoryboard::createArrow(%this)
{
   %arrow = new t2dStaticSprite(StoryboardArrow)
   {
      sceneGraph = $AnimationBuilder::sceneGraph;
      class = ABStoryboardArrow;    
      upResponse = true;
      hoverResponse = true;  
      dragResponse = true;
   };
   
   %arrow.setBlendColor($AnimationBuilder::colors::arrowNormal);
   %arrow.setLayer($AnimationBuilder::layers::scrollContent);
   %arrow.setImageMap(AnimationBuilderRightArrowImageMap);
   %arrow.setSize($AnimationBuilder::sizes::arrowSize);
   
   $AnimationBuilder::StoryboardGroup.add(%arrow);
   
   return %arrow;   
}

function ABStoryboard::AddStoryBoardFrame(%this)
{
   %obj = new t2dStaticSprite(StoryboardEmptyFrame)
   {
      sceneGraph = $AnimationBuilder::SceneGraph; 
      class = ABStoryboardFrame;  
      upResponse = true;   
   };
   
   %this.addLabel(%obj);
   $AnimationBuilder::StoryboardGroup.add(%obj);
   $AnimationBuilder::StoryboardWindow.addObject(%obj);
}

function ABStoryboard::addLabel(%this, %obj)
{
   %size = "17 1.6";
   
   %fontObj = new t2dSceneObject()
   {
      scenegraph = %obj.scenegraph;      
      class = ABFont;
      size = %size;
   };
   
   %obj.fontObj = %fontObj;

   %pos = %obj.getPosition();
   %posX = getWord(%pos, 0);
   %posY = getWord(%pos, 1);
   
   %objSize = %obj.getSize();
   %objSizeX = getWord(%objSize, 0);
   %objSizeY = getWord(%objSize, 1);

   %posX += 1;
   //%posY += 1;

   %fontObj.mount(%obj, %obj.getLocalPoint(%posX, %posY));
   
   %fontObj.buildText("Drop Frame Here", %size, false);
   
   $AnimationBuilder::StoryboardGroup.add(%fontObj);
}

function ABStoryBoard::insertFrameImage(%this, %imageObj, %index)
{
   %this.changeFrameImage(%imageObj, %index, true);   
}

function ABStoryboard::insertEmptyFrame(%this, %index)
{
   %image = new t2dStaticSprite(StoryboardEmptyFrame)
   {
      sceneGraph = $AnimationBuilder::sceneGraph;
      class = ABStoryBoardFrame;   
      upResponse = true;      
   };   
   
   $AnimationBuilder::StoryboardGroup.add(%image);
   %this.insertFrameImage(%image, %index);
}

function ABStoryboard::changeFrameImage(%this, %imageObj, %index, %keepFrame)
{
   if(!isObject(%imageObj))
      return;
 
   if($AnimationBuilder::allFrames)
   {
      MessageBoxYesNo("Warning!", "Currently you have set 'Use All Frames', if you add a new frame this will be disabled..." SPC
         "would you like to disable 'Use All Frames' and add this frame?", 
         "AnimationBuilderChkUseAllFrames.setValue(false); AnimationBuilder::useAllFrames();" SPC 
         "$AnimationBuilder::storyboardWindow.changeFrameImage(" @ %imageObj @ ", " @ %index @ ", \"" @ %keepFrame @ "\");", %imageObj @ ".safeDelete();");
            
      return;
   } 
 
   if(%keepFrame $= "")
      %keepFrame = false;
   
   %objCount = %this.objList.getCount();
   
   %tempList = new SimSet();
   
   for(%i=0;%i<%objCount;%i++)
   {
      %obj = %this.objList.getObject(%i);
      
      if(%i == %index)
      {
         %tempList.add(%imageObj);    
         
         if(%keepFrame)
         {
            %tempList.add(%obj);
         } else
         {
            %deleteObj = %obj;               
         }
      } else
      {
         %tempList.add(%obj);         
      }
   }
   
   // if we're adding an image to the end then add a new empty frame
   if(%index == %objCount - 1)
      %addFrameToEnd = true;

   %this.clearObjects();
   
   if(isObject(%deleteObj))
   {
      %deleteObj.safeDelete();      
   }
   
   
   %objCount = %tempList.getCount();

   for(%i=0;%i<%objCount;%i++)
   {
      %obj = %tempList.getObject(%i);
      
      %this.addObject(%obj);
   }
   
   // clear and delete our temp list
   %tempList.clear();
   %tempList.delete();
   
   if(%addFrameToEnd && !%keepFrame)
      %this.AddStoryBoardFrame();
      
   if(isObject(%this.selectedFrame))
   {
      %this.selectFrame(%this.selectedFrame);      
   }
   
   %this.updateFramesText();

   $AnimationBuilder::StoryboardGroup.add(%imageObj);
}

function ABStoryBoard::deleteFrame(%this, %index)
{   
   %objCount = %this.objList.getCount();
 
   if(%index == (%objCount - 1) && !$AnimationBuilder::allFrames)
      return;
 
   if(%objCount <= 1)
      return;
   
   %tempList = new SimSet();
   
   for(%i=0;%i<%objCount;%i++)
   {
      %obj = %this.objList.getObject(%i);
      
      if(%i == %index)
      {
         %deleteObj = %obj;
      } else
      {
         %tempList.add(%obj);         
      }
   }

   %this.clearObjects();
   
   if(isObject(%deleteObj))
   {
      %deleteObj.safeDelete();      
   }
   
   %objCount = %tempList.getCount();

   for(%i=0;%i<%objCount;%i++)
   {
      %obj = %tempList.getObject(%i);
      
      %this.addObject(%obj);
   }
   
   // clear and delete our temp list
   %tempList.clear();
   %tempList.delete();  
   
   %selected = %this.selectFrameByIndex(%index);
   
   if(!%selected)
   {
      %this.selectFrameByIndex(%index-1);
   }
   
   %this.updateFramesText();
}

function ABStoryboard::selectFrame(%this, %frame)
{
   %prevFrame = %this.selectedFrame;
   %this.deSelectFrame();
   
   if(%prevFrame != %frame)
   {
      %this.selectedFrame = %frame;
      %frame.onSelected();
   }
}

function ABStoryboard::selectFrameByIndex(%this, %index)
{
   %frame = %this.objList.getObject(%index);
   
   if(!isObject(%frame))
   {
      return false;
   } else 
   {
      %this.selectFrame(%frame);   
      return true;
   }
}

function ABStoryboard::deSelectFrame(%this)
{
   if(isObject(%this.selectedFrame))
   {
      %this.selectedFrame.onDeSelected();      
   }
   
   %this.selectedFrame = "";   
}

function ABStoryboard::updateFramesText(%this)
{
   %frames = %this.objList.getCount();
   
   if(!$AnimationBuilder::allFrames)
      %frames--;
   
   animationDatablockFrames.setText(%frames);   
}