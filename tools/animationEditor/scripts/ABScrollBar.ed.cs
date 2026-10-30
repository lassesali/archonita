function ABScrollBar::init(%this)
{
   %parent = %this.parentObj;
   %imageMap = $AnimationBuilder::theme::ScrollBar;
   
   // Scroll background
   %background = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
      parentObj = %this;
   };
   %background.setImageMap(%imageMap, $AnimationBuilder::frames::scrollBar::barBackground);
   %background.setLayer($AnimationBuilder::layers::scrollBarBackground);
   %this.scrollBackground = %background;
   $AnimationBuilder::StoryboardGroup.add(%background);
   
   // Top Arrow
   %topArrow = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
      class = ABArrow;
      direc = "up";
      hoverResponse = true;
      downResponse = true;
      parentObj = %this;
   };
   %topArrow.setImageMap(%imageMap, $AnimationBuilder::frames::scrollBar::upArrowNormal);
   %topArrow.setLayer($AnimationBuilder::layers::scrollBar);
   %topArrow.normalImage = $AnimationBuilder::frames::scrollBar::upArrowNormal;
   %topArrow.hoverImage = $AnimationBuilder::frames::scrollBar::upArrowHighlight;
   %topArrow.downImage = $AnimationBuilder::frames::scrollBar::upArrowDepressed;
   %this.topArrow = %topArrow;
   $AnimationBuilder::StoryboardGroup.add(%topArrow);
   
   // Top Arrow
   %bottomArrow = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
      class = ABArrow;
      direc = "down";
      hoverResponse = true;
      downResponse = true;
      parentObj = %this;
   };
   %bottomArrow.setImageMap(%imageMap, $AnimationBuilder::frames::scrollBar::downArrowNormal);
   %bottomArrow.setLayer($AnimationBuilder::layers::scrollBar);
   %bottomArrow.normalImage = $AnimationBuilder::frames::scrollBar::downArrowNormal;
   %bottomArrow.hoverImage = $AnimationBuilder::frames::scrollBar::downArrowHighlight;
   %bottomArrow.downImage = $AnimationBuilder::frames::scrollBar::downArrowDepressed;
   %this.bottomArrow = %bottomArrow;
   $AnimationBuilder::StoryboardGroup.add(%bottomArrow);
   
   // Selection bar
   %selectionBar = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
      class = ABHoverSelectionBar;
      hoverResponse = true;
      dragResponse = true;
      parentObj = %this;
   };
   %selectionBar.setImageMap(%imageMap, $AnimationBuilder::frames::scrollBar::selectionBarMidNormal);
   %selectionBar.setLayer($AnimationBuilder::layers::scrollBar);
   %selectionBar.normalImage = $AnimationBuilder::frames::scrollBar::selectionBarMidNormal;
   %selectionBar.hoverImage = $AnimationBuilder::frames::scrollBar::selectionBarMidHighlight;
   %this.selectionBar = %selectionBar;
   $AnimationBuilder::StoryboardGroup.add(%selectionBar);
   
   // Selection bar top
   %selectionBarTop = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
      class = ABHoverSelectionBarCap;
      hoverResponse = true;
      parentObj = %this;
   };
   %selectionBarTop.setImageMap(%imageMap, $AnimationBuilder::frames::scrollBar::selectionBarTopNormal);
   %selectionBarTop.setLayer($AnimationBuilder::layers::scrollBar);
   %selectionBarTop.normalImage = $AnimationBuilder::frames::scrollBar::selectionBarTopNormal;
   %selectionBarTop.hoverImage = $AnimationBuilder::frames::scrollBar::selectionBarTopHighlight;
   %this.selectionBarTop = %selectionBarTop;
   $AnimationBuilder::StoryboardGroup.add(%selectionBarTop);
   
   // Selection bar bottom
   %selectionBarBottom = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
      class = ABHoverSelectionBarCap;
      hoverResponse = true;
      parentObj = %this;
   };
   %selectionBarBottom.setImageMap(%imageMap, $AnimationBuilder::frames::scrollBar::selectionBarBottomNormal);
   %selectionBarBottom.setLayer($AnimationBuilder::layers::scrollBar);
   %selectionBarBottom.normalImage = $AnimationBuilder::frames::scrollBar::selectionBarBottomNormal;
   %selectionBarBottom.hoverImage = $AnimationBuilder::frames::scrollBar::selectionBarBottomHighlight;
   %this.selectionBarBottom = %selectionBarBottom;
   $AnimationBuilder::StoryboardGroup.add(%selectionBarBottom);
   
   %this.setSelectionPosition(0);
   
   %this.positionBarBorder(); 
   %this.positionBar();
}

function ABScrollBar::positionBarBorder(%this)
{
   %parent = %this.parentObj;
   
   %pos = %parent.getPosition();
   %posX = getWord(%pos, 0);
   %posY = getWord(%pos, 1);
   
   %size = %parent.getSize();
   %width = getWord(%size, 0);
   %height = getWord(%size, 1);
   
   %scrollWidth = $AnimationBuilder::sizes::scroll;
   %arrowHeight = $AnimationBuilder::sizes::scrollArrow;
   
   %cornerSize = $AnimationBuilder::sizes::corner;
   %cornerWidth = getWord(%cornerSize, 0);
   %cornerHeight = getWord(%cornerSize, 1);
   
   %scrollBarPosX = %posX + (%width / 2) - (%scrollWidth / 2);
   
   %scrollBackgroundY = %posY;
   %scrollBackgroundHeight = %height - (%cornerHeight * 2);
   
   %topArrowY = %posY - (%height / 2) + (%arrowHeight / 2);
   %topArrowHeight = %arrowHeight;
   
   %bottomArrowY = %posY + (%height / 2) - (%arrowHeight / 2);
   %bottomArrowHeight = %arrowHeight;
   
   %this.scrollBackground.mount(%this, %this.getLocalPoint(%scrollBarPosX, %scrollBackgroundY));
   %this.scrollBackground.setSize(%scrollWidth, %scrollBackgroundHeight); 
   %this.topArrow.mount(%this, %this.getLocalPoint(%scrollBarPosX, %topArrowY));
   %this.topArrow.setSize(%scrollWidth, %topArrowHeight); 
   %this.bottomArrow.mount(%this, %this.getLocalPoint(%scrollBarPosX, %bottomArrowY));
   %this.bottomArrow.setSize(%scrollWidth, %bottomArrowHeight);    
}

function ABScrollBar::positionBar(%this)
{
   %windowSize = %this.parentObj.getWindowSize();      
   %width = getWord(%windowSize, 0);
   %height = getWord(%windowSize, 1);
   
   %contentSize = %this.parentObj.getContentSize();
   %contentWidth = getWord(%contentSize, 0);
   %contentHeight = getWord(%contentSize, 1);
   
   %selectionPos = %this.getSelectionPosition();
   
   %topArrowPos = %this.topArrow.getPosition();
   %bottomArrowPos = %this.bottomArrow.getPosition();
   %topArrowSize = %this.topArrow.getSize();
   %bottomArrowSize = %this.bottomArrow.getSize();
   
   %topArrowX = getWord(%topArrowPos, 0);
   %topArrowY = getWord(%topArrowPos, 1);
   
   %bottomArrowX = getWord(%bottomArrowPos, 0);
   %bottomArrowY = getWord(%bottomArrowPos, 1);
   
   %topArrowWidth = getWord(%topArrowSize, 0);
   %topArrowHeight = getWord(%topArrowSize, 1);
   
   %bottomArrowWidth = getWord(%bottomArrowSize, 0);
   %bottomArrowHeight = getWord(%bottomArrowSize, 1);
   
   %topBarBackgroundPosY = %topArrowY + (%topArrowHeight / 2);
   %bottomBarBackgroundPosY = %bottomArrowY - (%bottomArrowHeight / 2);
   
   %scrollCapHeight = $AnimationBuilder::sizes::scrollBarBorder;
   
   %scrollWidth = %topArrowWidth;
   %scrollPosX = %topArrowX;
   
   %selectionBarTopMaxPosY = %topBarBackgroundPosY + (%scrollCapHeight / 2);
   %selectionBarBottomMaxPosY = %bottomBarBackgroundPosY - (%scrollCapHeight / 2);
   %scrollBarMaxHeight = %selectionBarBottomMaxPosY - %selectionBarTopMaxPosY;
   
   if(%contentHeight <= %height)
   {
      // if the content height is less than or equal to the window height
      // then we need to size the bar to the maximum size
      
      %scrollBarMidHeight = %scrollBarMaxHeight;
      %selectionBarMidPosY = %selectionBarBottomMaxPosY - (%scrollBarMidHeight / 2);
   } else
   {
      // if the content height is less than or equal to the window height
      // then we need to size the bar to the maximum size
      
      // this is the percentage of our window height in our content height
      %windowRatio = %height / %contentHeight;
      %scrollBarMidHeight = %scrollBarMaxHeight * %windowRatio;
      
      
      %scrollLength = %scrollBarMaxHeight - %scrollBarMidHeight;
      %scrollBarStartY = %topBarBackgroundPosY + (%scrollCapHeight / 2) + (%scrollBarMidHeight / 2);
      %scrollBarEndY = %bottomBarBackgroundPosY - (%scrollCapHeight / 2) - (%scrollBarMidHeight / 2);
      
      %selectionBarMidPosY = %scrollBarStartY + (%scrollLength * (%selectionPos / 100));
   }
   
   %selectionBarTopPosY = %selectionBarMidPosY - (%scrollBarMidHeight / 2);
   %selectionBarBottomPosY = %selectionBarMidPosY + (%scrollBarMidHeight / 2);
   
   %this.selectionBar.setPosition(%scrollPosX, %selectionBarMidPosY);     
   %this.selectionBar.setSize(%scrollWidth, %scrollBarMidHeight);
   %this.selectionBarTop.mount(%this.selectionBar, %this.selectionBar.getLocalPoint(%scrollPosX, %selectionBarTopPosY));     
   %this.selectionBarTop.setSize(%scrollWidth, %scrollCapHeight);
   %this.selectionBarBottom.mount(%this.selectionBar, %this.selectionBar.getLocalPoint(%scrollPosX, %selectionBarBottomPosY));     
   %this.selectionBarBottom.setSize(%scrollWidth, %scrollCapHeight);
}
function ABScrollBar::getSceneGraph(%this)
{
   return $AnimationBuilder::SceneGraph;   
}

function ABScrollBar::onLevelLoaded(%this, %sceneGraph)
{
   %this.sceneGraph = %sceneGraph;   
}

function ABScrollBar::getSelectionPosition(%this)
{
   return %this.barPosition;   
}

function ABScrollBar::setSelectionPercent(%this, %percent)
{
   if(%percent > 1)
      %percent = 1;
   else if(%percent < 0)
      %percent = 0;

   %this.barPosition = %percent * 100;   
   %this.parentObj.positionScrollBase();
   %this.positionBar();
}

function ABScrollBar::setSelectionPosition(%this, %pos)
{
   %percent = %this.getSelectionPercent(%pos);
   %this.setSelectionPercent(%percent); 
}

function ABScrollBar::getSelectionPercent(%this, %pos)
{
   %posY = getWord(%pos, 1);
   
   %windowSize = %this.parentObj.getWindowSize();      
   %width = getWord(%windowSize, 0);
   %height = getWord(%windowSize, 1);
   
   %contentSize = %this.parentObj.getContentSize();
   %contentWidth = getWord(%contentSize, 0);
   %contentHeight = getWord(%contentSize, 1);
   
   %topArrowPos = %this.topArrow.getPosition();
   %bottomArrowPos = %this.bottomArrow.getPosition();
   %topArrowSize = %this.topArrow.getSize();
   %bottomArrowSize = %this.bottomArrow.getSize();
   
   %topArrowX = getWord(%topArrowPos, 0);
   %topArrowY = getWord(%topArrowPos, 1);  
   
   %bottomArrowX = getWord(%bottomArrowPos, 0);
   %bottomArrowY = getWord(%bottomArrowPos, 1);
   
   %topArrowWidth = getWord(%topArrowSize, 0);
   %topArrowHeight = getWord(%topArrowSize, 1);
   
   %bottomArrowWidth = getWord(%bottomArrowSize, 0);
   %bottomArrowHeight = getWord(%bottomArrowSize, 1);
   
   %topBarBackgroundPosY = %topArrowY + (%topArrowHeight / 2);
   %bottomBarBackgroundPosY = %bottomArrowY - (%bottomArrowHeight / 2);
   
   %scrollCapHeight = $AnimationBuilder::sizes::scrollBarBorder;

   %selectionBarTopMaxPosY = %topBarBackgroundPosY + (%scrollCapHeight / 2);
   %selectionBarBottomMaxPosY = %bottomBarBackgroundPosY - (%scrollCapHeight / 2);
   %scrollBarMaxHeight = %selectionBarBottomMaxPosY - %selectionBarTopMaxPosY;
      
   %windowRatio = %height / %contentHeight;
   %scrollBarMidHeight = %scrollBarMaxHeight * %windowRatio;
      
   %scrollLength = %scrollBarMaxHeight - %scrollBarMidHeight;
   %scrollBarStartY = %topBarBackgroundPosY + (%scrollCapHeight / 2) + (%scrollBarMidHeight / 2);
   %scrollBarEndY = %bottomBarBackgroundPosY - (%scrollCapHeight / 2) - (%scrollBarMidHeight / 2);
   
   if(%posY <= %selectionBarTopMaxPosY)
   {
      return 0;
   } else if(%posY >= %selectionBarBottomMaxPosY)
   {
      return 1;  
   } else
   {  
      %realBarPosY = %posY - %scrollBarStartY;
      %percent = %realBarPosY / %scrollLength;
      return %percent;   
   }   
}