new ScriptClass(ABScrollWindow);
new ScriptClass(ABScrollBar);
new ScriptClass(ABHover);
new ScriptClass(ABHoverSelectionBar);
new ScriptClass(ABHoverSelectionBarCap);
new ScriptClass(ABFrameset);
new ScriptClass(ABFramesetFrame);
new ScriptClass(ABStoryboard);
new ScriptClass(ABStoryboardFrame);
new ScriptClass(ABStoryboardArrow);
new ScriptClass(ABPreview);

$AnimationBuilder::cameraPosition = "0 0 85 45";
$AnimationBuilder::frameWindowPosition = "-31.5 0";
$AnimationBuilder::frameWindowSize = "22.2 45.3";

$AnimationBuilder::StoryboardWindowPosition = "11 0";
$AnimationBuilder::StoryboardWindowSize = "61.5 45.3";

$AnimationBuilder::previewWindowPosition = "11 0";
$AnimationBuilder::previewWindowSize = "61.5 45.3";

$AnimationBuilder::preview::loadSteps = 50;
$AnimationBuilder::preview::startPos = "42.5 22.5";
$AnimationBuilder::preview::startSize = "0 0";
$AnimationBuilder::preview::time = "3";
$AnimationBuilder::preview::backgroundLayer = 3;
$AnimationBuilder::preview::borderLayer = 2;

$AnimationBuilder::storyboard::columnCount = 3;

$AnimationBuilder::hoverList = new SimSet();
$AnimationBuilder::hoverCheckList = new SimSet();
$AnimationBuilder::downList = new SimSet();
$AnimationBuilder::selectedObject = "";

$AnimationBuilder::theme::Light = AnimationBuilderThemeLight;
$AnimationBuilder::theme::Medium = AnimationBuilderThemeMedium;
$AnimationBuilder::theme::Dark = AnimationBuilderThemeDark;

$AnimationBuilder::theme::ScrollBar = AnimationBuilderThemeScrollBar;

$AnimationBuilder::layers::scrollWindow = 12;
$AnimationBuilder::layers::scrollBorder = 9;
$AnimationBuilder::layers::scrollBarBackground = 7;
$AnimationBuilder::layers::scrollBar = 6;
$AnimationBuilder::layers::scrollContent = 8;
$AnimationBuilder::layers::imageBorder = 5;

$AnimationBuilder::frames::panel::topLeft = 0;
$AnimationBuilder::frames::panel::topCenter = 1;
$AnimationBuilder::frames::panel::topRight = 2;
$AnimationBuilder::frames::panel::leftBorder = 3;
$AnimationBuilder::frames::panel::background = 4;
$AnimationBuilder::frames::panel::rightBorder = 5;
$AnimationBuilder::frames::panel::bottomLeft = 6;
$AnimationBuilder::frames::panel::bottomCenter = 7;
$AnimationBuilder::frames::panel::bottomRight = 8;

$AnimationBuilder::frames::scrollBar::upArrowNormal = 0;
$AnimationBuilder::frames::scrollBar::upArrowHighlight = 1;
$AnimationBuilder::frames::scrollBar::upArrowDepressed = 2;
$AnimationBuilder::frames::scrollBar::downArrowNormal = 3;
$AnimationBuilder::frames::scrollBar::downArrowHighlight = 4;
$AnimationBuilder::frames::scrollBar::downArrowDepressed = 5;
$AnimationBuilder::frames::scrollBar::leftArrowNormal = 18;
$AnimationBuilder::frames::scrollBar::leftArrowHighlight = 19;
$AnimationBuilder::frames::scrollBar::leftArrowDepressed = 20;
$AnimationBuilder::frames::scrollBar::rightArrowNormal = 21;
$AnimationBuilder::frames::scrollBar::rightArrowHighlight = 22;
$AnimationBuilder::frames::scrollBar::rightArrowDepressed = 23;
$AnimationBuilder::frames::scrollBar::selectionBarTopNormal = 6;
$AnimationBuilder::frames::scrollBar::selectionBarMidNormal = 9;
$AnimationBuilder::frames::scrollBar::selectionBarBottomNormal = 12;
$AnimationBuilder::frames::scrollBar::selectionBarTopHighlight = 7;
$AnimationBuilder::frames::scrollBar::selectionBarMidHighlight = 10;
$AnimationBuilder::frames::scrollBar::selectionBarBottomHighlight = 13;
$AnimationBuilder::frames::scrollBar::barBackground = 15;

$AnimationBuilder::sizes::corner = "0.55 0.55";
$AnimationBuilder::sizes::scroll = "1.3";
$AnimationBuilder::sizes::scrollArrow = "1.25";
$AnimationBuilder::sizes::scrollBarBorder = "0.5";
$AnimationBuilder::sizes::frameHeight = 12;
$AnimationBuilder::sizes::framePadding = 3;
$AnimationBuilder::sizes::frameSpacing = 3;
$AnimationBuilder::sizes::frameBorderSize = 0.25;
$AnimationBuilder::sizes::arrowSize = "2.5 1.5";

$AnimationBuilder::sizes::sizeScale = 1.3;

$AnimationBuilder::position::startFrameOffsetX = -1;
$AnimationBuidler::position::startContentOffset = 8.5;
$AnimationBuidler::position::startContentOffsetCol = 4.5;
$AnimationBuilder::position::frameX = 5;
$AnimationBuilder::position::arrowOffset = "0.5";

$AnimationBuilder::colors::arrowOver = "1 0 0";
$AnimationBuilder::colors::arrowNormal = "0 0 0";
$AnimationBuilder::colors::dragImageAlpha = 0.5;

$AnimationBuilder::time::selectionPulseTime = 5;

function AnimationBuilder::resizeScale()
{
   %scale = $AnimationBuilder::sizes::sizeScale;
   
   %corner = $AnimationBuilder::sizes::corner;
   %cornerX = getWord(%corner, 0); 
   %cornerY = getWord(%corner, 1);
   $AnimationBuilder::sizes::corner = %cornerX * %scale SPC %cornerY * %scale;
   
   $AnimationBuilder::sizes::scroll *= %scale;
   $AnimationBuilder::sizes::scrollArrow *= %scale;
   $AnimationBuilder::sizes::scrollBarBorder *= %scale;
   $AnimationBuilder::sizes::frameHeight *= %scale;
   $AnimationBuilder::sizes::framePadding *= %scale;
   $AnimationBuilder::sizes::frameSpacing *= %scale;  
   $AnimationBuilder::sizes::frameBorderSize *= %scale;
   
   $AnimationBuilder::position::startFrameOffsetX *= %scale;
   $AnimationBuidler::position::startContentOffset *= %scale;
   $AnimationBuidler::position::startContentOffsetCol *= %scale;
}

AnimationBuilder::resizeScale();

function AnimationBuilder::initCanvas()
{
   animDBSceneWindow.setScenegraph($AnimationBuilder::SceneGraph);
   animDBSceneWindow.setCurrentCameraPosition($AnimationBuilder::cameraPosition); 
   
   $AnimationBuilder::frameWindow = new t2dSceneObject()
   {
      sceneGraph = $AnimationBuilder::SceneGraph;      
      class = ABFrameset;
      superClass = ABScrollWindow;
   };
   
   $AnimationBuilder::frameWindow.setPosition($AnimationBuilder::frameWindowPosition);
   $AnimationBuilder::frameWindow.setSize($AnimationBuilder::frameWindowSize);
   
   $AnimationBuilder::frameWindow.init();
   
   $AnimationBuilder::StoryboardWindow = new t2dSceneObject()
   {
      sceneGraph = $AnimationBuilder::SceneGraph;      
      class = ABStoryboard;
      superClass = ABScrollWindow;
   };
   
   $AnimationBuilder::StoryboardWindow.setPosition($AnimationBuilder::StoryboardWindowPosition);
   $AnimationBuilder::StoryboardWindow.setSize($AnimationBuilder::StoryboardWindowSize);
   
   $AnimationBuilder::StoryboardWindow.init();
   
   $AnimationBuilder::previewWindow = new t2dSceneObject()
   {
      sceneGraph = $AnimationBuilder::SceneGraph;      
      class = ABPreview;
      superClass = ABScrollWindow;
   };
   
   $AnimationBuilder::previewWindow.setPosition($AnimationBuilder::preview::startPos);
   $AnimationBuilder::previewWindow.setSize($AnimationBuilder::preview::startSize);
   
   $AnimationBuilder::previewWindow.init("medium", false);
   
   $AnimationBuilder::previewWindow.setVisible(false);
   $AnimationBuilder::previewWindow.setWindowLayers($AnimationBuilder::preview::backgroundLayer, $AnimationBuilder::preview::borderLayer);
}

function AnimationBuilder::clearCanvas()
{
   $AnimationBuilder::frameWindow.safeDelete();
   $AnimationBuilder::storyboardWindow.safeDelete();
   $AnimationBuilder::sceneGraph.delete();   
}

function AnimationBuilder::loadSelectionFrames()
{
   %imageMap = $AnimationBuilder::selectedImageMap;
   %imageMode = %imageMap.getImageMapMode();
   
   if(%imageMode $= "FULL")
   {
      %sprite = new t2dStaticSprite()
      {
         sceneGraph = $AnimationBuilder::SceneGraph;
         class = ABFramesetFrame;
         downResponse = true;
      };
         
      %sprite.setImageMap(%imageMap);
         
      $AnimationBuilder.frameWindow.addObject(%sprite);
   } else
   {
      %frameCount = %imagemap.getFrameCount();
      
      for(%i=0;%i<%frameCount;%i++)
      {
         %sprite = new t2dStaticSprite()
         {
            sceneGraph = $AnimationBuilder::SceneGraph;   
            class = ABFramesetFrame;
            downResponse = true;         
         };
         
         %sprite.setImageMap(%imageMap, %i);
         
         $AnimationBuilder::frameWindow.addObject(%sprite);
      }
   }
}

function AnimationBuilder::initStoryboard()
{
   if(!$AnimationBuilder::edit)
   {
      $AnimationBuilder::storyboardWindow.addStoryboardFrame();      
   }
}

function AnimationBuilder::followMouse(%obj)
{
   if(isObject(%obj) && !$AnimationBuilder::following)
   {
      $AnimationBuilder::following = true;
      $AnimationBuilder::followMouseObj = %obj;         
   }
}

function animDBSceneWindow::onMouseMove(%this, %modifier, %worldPos, %mouseClicks)
{
   if(!isObject($AnimationBuilder::sceneGraph))
      return;
      
   %objList = $AnimationBuilder::sceneGraph.pickPoint(%worldPos);
   %objCount = getWordCount(%objList);
   
   $AnimationBuilder::hoverCheckList.clear();
   
   for(%i=0;%i<%objCount;%i++)
   {
      %obj = getWord(%objList, %i);
      
      if(%obj.hoverResponse)
      {
         $AnimationBuilder::hoverCheckList.add(%obj);
         if(isObject(%obj) && !$AnimationBuilder::hoverList.isMember(%obj))
         {
            $AnimationBuilder::hoverList.add(%obj);     
         }
      }
   }
   
   %objCount = $AnimationBuilder::hoverList.getCount();

   for(%i=%objCount-1;%i>=0;%i--)
   {
      %obj = $AnimationBuilder::hoverList.getObject(%i); 

      if(isObject(%obj) && !$AnimationBuilder::hoverCheckList.isMember(%obj))
      {
         $AnimationBuilder::hoverList.remove(%obj);
         %obj.onMouseLeave(%worldPos);
      }
   }
   
   %objCount = $AnimationBuilder::hoverList.getCount();

   for(%i=0;%i<%objCount;%i++)
   {
      %obj = $AnimationBuilder::hoverList.getObject(%i); 
      if(isObject(%obj))
      {
         %obj.onMouseOver(%worldPos);
      }
   }
}

function animDBSceneWindow::onMouseDown(%this, %modifier, %worldPos, %mouseClicks)
{
   if(!isObject($AnimationBuilder::sceneGraph))
      return;
      
   %objList = $AnimationBuilder::sceneGraph.pickPoint(%worldPos);
   %objCount = getWordCount(%objList);
   
   for(%i=0;%i<%objCount;%i++)
   {
      %obj = getWord(%objList, %i);
      
      if(%obj.dragResponse)
      {
         $AnimationBuilder::selectedObject = %obj;
         %obj.onMouseDown(%worldPos);   
      } else if(%obj.downResponse)
      {
         $AnimationBuilder::downList.add(%obj);
         %obj.onMouseDown(%worldPos);
      }
   }   
}

function animDBSceneWindow::onMouseDragged(%this, %modifier, %worldPos, %mouseClicks)
{
   if(!isObject($AnimationBuilder::sceneGraph))
      return;
   
   if(isObject($AnimationBuilder::selectedObject))
   {
      $AnimationBuilder::selectedObject.onMouseDragged(%worldPos);
   }
   
   if($AnimationBuilder::following && isObject($AnimationBuilder::followMouseObj))
   {
      $AnimationBuilder::followMouseObj.setPosition(%worldPos);      
   }
   
   %objList = $AnimationBuilder::sceneGraph.pickPoint(%worldPos);
   %objCount = getWordCount(%objList);
   
   for(%i=0;%i<%objCount;%i++)
   {
      %obj = getWord(%objList, %i);
      
      if(%obj.dragResponse)
      {
         %obj.onMouseDragged(%worldPos);   
      }
   }
}

function animDBSceneWindow::onMouseUp(%this, %modifier, %worldPos, %mouseClicks)
{
   if(!isObject($AnimationBuilder::sceneGraph))
      return;
   
   if(isObject($AnimationBuilder::selectedObject))
      $AnimationBuilder::selectedObject = "";  
   
   if($AnimationBuilder::following)
      %followSet = true;
   
   %objCount = $AnimationBuilder::downList.getCount();
   
   for(%i=%objCount-1;%i>=0;%i--)
   {
      %obj = $AnimationBuilder::downList.getObject(%i);
      
      if(isObject(%obj))
      {
         %obj.onMouseUp();
         $AnimationBuilder::downList.remove(%obj);
      }
   }
   
   %objList = $AnimationBuilder::sceneGraph.pickPoint(%worldPos);
   %objCount = getWordCount(%objList);
   
   for(%i=0;%i<%objCount;%i++)
   {
      %obj = getWord(%objList, %i);
      
      if(isObject(%obj) && %obj.upResponse)
      {
         %obj.onMouseUp(%worldPos);   
      }
   }
   
   if(%followSet && $AnimationBuilder::following && isObject($AnimationBuilder::followMouseObj))
   {
      $AnimationBuilder::followMouseObj.safeDelete();
      $AnimationBuilder::followMouseObj = "";
      $AnimationBuilder::following = false;      
   }
}

function AnimationBuilder::addStoryBoardFrame()
{
   if($AnimationBuilder::allFrames)
   {
      MessageBoxYesNo("Warning!", "Currently you have set 'Use All Frames', if you add a new frame this will be disabled..." SPC
         "would you like to disable 'Use All Frames' and add this frame?", 
         "AnimationBuilderChkUseAllFrames.setValue(false); AnimationBuilder::useAllFrames();" SPC 
         "AnimationBuilder::addStoryBoardFrame();", "");
            
      return;
   }  
   
   if(isObject($AnimationBuilder::storyboardWindow.selectedFrame))
   {
      $AnimationBuilder::storyboardWindow.insertEmptyFrame($AnimationBuilder::storyboardWindow.selectedFrame.index); 
   }
}

function AnimationBuilder::deleteStoryBoardFrame()
{
   if($AnimationBuilder::allFrames)
   {
      MessageBoxYesNo("Warning!", "Currently you have set 'Use All Frames', if you delete a frame this will be disabled..." SPC
         "would you like to disable 'Use All Frames' and delete this frame?", 
         "AnimationBuilderChkUseAllFrames.setValue(false); AnimationBuilder::useAllFrames();" SPC 
         "AnimationBuilder::deleteStoryBoardFrame();", "");
            
      return;
   }
   
   if(isObject($AnimationBuilder::storyboardWindow.selectedFrame))
   {
      $AnimationBuilder::storyboardWindow.deleteFrame($AnimationBuilder::storyboardWindow.selectedFrame.index); 
   }  
}

function AnimationBuilder::loadPreviewWindow()
{
   if(!$AnimationBuilder::allFrames)
   {   
      %frameCount = $AnimationBuilder::StoryboardWindow.objList.getCount() - 1;
      if(%frameCount <= 0)
      {
         MessageBoxOK("No Animation Frames...", "You have not set any animation frames to preview.", "");
         return;  
      }               
   }
   
   if(!$AnimationBuilder::previewOpen)
   {
      AnimationBuilder::frameTimeCallback();
      $AnimationBuilder::previewWindow.createPreviewAnim();
      $AnimationBuilder::previewWindow.setupPreview();
      AnimationBuilder::showPreviewControls();
      AnimationBuilder::setupPreviewSliders();
      $AnimationBuilder::previewOpen = true;
   } else
   {
      $AnimationBuilder::previewWindow.playButton();
   }
}