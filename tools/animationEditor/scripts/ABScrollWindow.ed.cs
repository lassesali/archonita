function ABScrollWindow::getSceneGraph(%this)
{
   return $AnimationBuilder::SceneGraph;   
}

function ABScrollWindow::getWindowSize(%this)
{
   return %this.getSize();   
}

function ABScrollWindow::getContentSize(%this)
{
   return %this.contentSize;   
}

function ABScrollWindow::setContentSize(%this, %sizeY)
{
   %size = %this.contentSize;
   %sizeX = getWord(%size, 0);
   
   %this.contentSize = %sizeX SPC %sizeY;
}

function ABScrollWindow::addFrame(%this, %obj)
{
   %scenegraph = $AnimationBuilder::SceneGraph;
   
   %obj.leftLine = %this.createLine(%scenegraph);
   %obj.rightLine = %this.createLine(%scenegraph);
   %obj.topLine = %this.createLine(%scenegraph);
   %obj.bottomLine = %this.createLine(%scenegraph);
   
   %objSize = %obj.getSize();
   %objSizeX = $AnimationBuilder::sizes::frameHeight; //getWord(%objSize, 0);
   %objSizeY = $AnimationBuilder::sizes::frameHeight; //getWord(%objSize, 1);
   
   %objPos = %obj.getPosition();
   %objPosX = getWord(%objPos, 0);
   %objPosY = getWord(%objPos, 1);
   
   %borderSize = $AnimationBuilder::sizes::frameBorderSize;
   
   %leftLineOffsetX = %objPosX - (%objSizeX / 2) - (%borderSize / 2);
   %leftLineOffsetY = %objPosY;
   
   %rightLineOffsetX = %objPosX + (%objSizeX / 2) + (%borderSize / 2);
   %rightLineOffsetY = %objPosY;
   
   %topLineOffsetX = %objPosX;
   %topLineOffsetY = %objPosY - (%objSizeY / 2) - (%borderSize / 2);
   
   %bottomLineOffsetX = %objPosX;
   %bottomLineOffsetY = %objPosY + (%objSizeY / 2) + (%borderSize / 2);
   
   %vertWidth = %borderSize;
   %vertHeight = %objSizeX + (%borderSize * 2);
   %horizWidth = %objSizeY;
   %horizHeight = %borderSize;
   
   %obj.leftLine.setSize(%vertWidth, %vertHeight);
   %obj.rightLine.setSize(%vertWidth, %vertHeight);
   %obj.topLine.setSize(%horizWidth, %horizHeight);
   %obj.bottomLine.setSize(%horizWidth, %horizHeight);
   
   %obj.leftLine.mount(%obj, %obj.getLocalPoint(%leftLineOffsetX, %leftLineOffsetY));
   %obj.rightLine.mount(%obj, %obj.getLocalPoint(%rightLineOffsetX, %rightLineOffsetY));
   %obj.topLine.mount(%obj, %obj.getLocalPoint(%topLineOffsetX, %topLineOffsetY));
   %obj.bottomLine.mount(%obj, %obj.getLocalPoint(%bottomLineOffsetX, %bottomLineOffsetY));
   
   $AnimationBuilder::StoryboardGroup.add(%obj);
}

function ABScrollWindow::createLine(%this, %scenegraph)
{
   %line = new t2dStaticSprite()
   {
      sceneGraph = %scenegraph;
   };
   
   %line.setLayer($AnimationBuilder::layers::imageBorder);
   %line.setImageMap(blankImageMap);
   %line.setBlendColor(0, 0, 0);
   
   $AnimationBuilder::StoryboardGroup.add(%line);
   
   return %line;
}

function ABScrollWindow::init(%this, %color, %scrollBar)
{
   if(%scrollBar $= "")
      %scrollBar = true;
   
   %thisSize = %this.getSize();
   %thisSizeX = getWord(%thisSize, 0);
   %thisSizeY = getWord(%thisSize, 1);
   
   if(%thisSizeX < 20)
      %thisSizeX = 20;
      
   if(%thisSizeY < 20)
      %thisSizeY = 20;
      
   %this.setSize(%thisSizeX, %thisSizeY);
   
   if(%color $= "")
      %color = "dark";
   
   switch$ (%color)
   {
      case "light":
         %imageMap = $AnimationBuilder::theme::Light;
      
      case "medium":
         %imageMap = $AnimationBuilder::theme::Medium;
      
      case "dark":
         %imageMap = $AnimationBuilder::theme::Dark;
   }
   
   %this.contentSize = "0 150";
   %this.objList = new SimSet();
   $AnimationBuilder::StoryboardGroup.add(%this.objList);
   
   // Background
   %border = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
   };
   %border.setImageMap(%imageMap, $AnimationBuilder::frames::panel::background);
   %border.setLayer($AnimationBuilder::layers::scrollWindow);
   %this.background = %border;
   $AnimationBuilder::StoryboardGroup.add(%border);
   
   // Top left corner
   %border = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
   };
   %border.setImageMap(%imageMap, $AnimationBuilder::frames::panel::topLeft);
   %border.setLayer($AnimationBuilder::layers::scrollBorder);
   %this.topLeftCorner = %border;
   $AnimationBuilder::StoryboardGroup.add(%border);
   
   // Top center border
   %border = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
   };
   %border.setImageMap(%imageMap, $AnimationBuilder::frames::panel::topCenter);
   %border.setLayer($AnimationBuilder::layers::scrollBorder);
   %this.topBorder = %border;
   $AnimationBuilder::StoryboardGroup.add(%border);
   
   // Top right corner
   %border = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
   };
   %border.setImageMap(%imageMap, $AnimationBuilder::frames::panel::topRight);
   %border.setLayer($AnimationBuilder::layers::scrollBorder);
   %this.topRightCorner = %border;
   $AnimationBuilder::StoryboardGroup.add(%border);
   
   // Left border
   %border = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
   };
   %border.setImageMap(%imageMap, $AnimationBuilder::frames::panel::leftBorder);
   %border.setLayer($AnimationBuilder::layers::scrollBorder);
   %this.leftBorder = %border;
   $AnimationBuilder::StoryboardGroup.add(%border);
   
   // Right border
   %border = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
   };
   %border.setImageMap(%imageMap, $AnimationBuilder::frames::panel::rightBorder);
   %border.setLayer($AnimationBuilder::layers::scrollBorder);
   %this.rightBorder = %border;
   $AnimationBuilder::StoryboardGroup.add(%border);
      
   // Bottom left corner
   %border = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
   };
   %border.setImageMap(%imageMap, $AnimationBuilder::frames::panel::bottomLeft);
   %border.setLayer($AnimationBuilder::layers::scrollBorder);
   %this.bottomLeftCorner = %border;
   $AnimationBuilder::StoryboardGroup.add(%border);
   
   // Bottom center border
   %border = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
   };
   %border.setImageMap(%imageMap, $AnimationBuilder::frames::panel::bottomCenter);
   %border.setLayer($AnimationBuilder::layers::scrollBorder);
   %this.bottomBorder = %border;
   $AnimationBuilder::StoryboardGroup.add(%border);
   
   // Bottom right corner
   %border = new t2dStaticSprite()
   {
      sceneGraph = %this.getScenegraph();
   };
   %border.setImageMap(%imageMap, $AnimationBuilder::frames::panel::bottomRight);
   %border.setLayer($AnimationBuilder::layers::scrollBorder);
   %this.bottomRightCorner = %border;
   $AnimationBuilder::StoryboardGroup.add(%border);
   
   %this.scrollBase = new t2dSceneObject()
   {
      sceneGraph = %this.getSceneGraph();
      parentObj = %this;      
   };
   $AnimationBuilder::StoryboardGroup.add(%this.scrollBase);
   
   %this.positionBorders();
   
   %this.scrollBar = new t2dSceneObject()
   {
      sceneGraph = %this.getScenegraph();
      class = ABScrollBar;
      parentObj = %this;
   };
   $AnimationBuilder::StoryboardGroup.add(%this.scrollBar);
   
   if(%scrollBar)
      %this.scrollBar.init();
}

function ABScrollWindow::positionBorders(%this)
{
   %pos = %this.getPosition();
   %posX = getWord(%pos, 0);
   %posY = getWord(%pos, 1);
   
   %size = %this.getSize();
   %width = getWord(%size, 0);
   %height = getWord(%size, 1);
   
   %cornerSize = $AnimationBuilder::sizes::corner;
   %cornerSizeX = getWord(%cornerSize, 0);
   %cornerSizeY = getWord(%cornerSize, 1);

   %backgroundWidth = %width - (%cornerSizeX * 2);
   %backgroundHeight = %height - (%cornerSizeY * 2);
   
   %this.background.setSize(%backgroundWidth, %backgroundHeight);
   %this.background.mount(%this, %this.getLocalPoint(%posX, %posY));
   
   %this.topLeftCorner.setSize(%cornerSize);
   %this.topRightCorner.setSize(%cornerSize);
   %this.bottomLeftCorner.setSize(%cornerSize);
   %this.bottomRightCorner.setSize(%cornerSize);
   
   %topLeftCornerX = %posX - (%width / 2); 
   %topLeftCornerY = %posY - (%height / 2);
   
   %topRightCornerX = %posX + (%width / 2);
   %topRightCornerY = %posY - (%height / 2);
   
   %bottomLeftCornerX = %posX - (%width / 2);
   %bottomLeftCornerY = %posY + (%height / 2);
   
   %bottomRightCornerX = %posX + (%width / 2);
   %bottomRightCornerY = %posY + (%height / 2);
   
   %topLeftCornerSize = %this.topLeftCorner.getSize();
   %topLeftCornerSizeX = getWord(%topLeftCornerSize, 0);
   %topLeftCornerSizeY = getWord(%topLeftCornerSize, 1);
   
   %topLeftCornerPosX = %topLeftCornerX + (%topLeftCornerSizeX / 2);
   %topLeftCornerPosY = %topLeftCornerY + (%topLeftCornerSizeY / 2);
   
   %topRightCornerSize = %this.topRightCorner.getSize();
   %topRightCornerSizeX = getWord(%topRightCornerSize, 0);
   %topRightCornerSizeY = getWord(%topRightCornerSize, 1);
   
   %topRightCornerPosX = %topRightCornerX - (%topRightCornerSizeX / 2);
   %topRightCornerPosY = %topRightCornerY + (%topRightCornerSizeY / 2);
   
   %bottomLeftCornerSize = %this.bottomLeftCorner.getSize();
   %bottomLeftCornerSizeX = getWord(%bottomLeftCornerSize, 0);
   %bottomLeftCornerSizeY = getWord(%bottomLeftCornerSize, 1);
   
   %bottomLeftCornerPosX = %bottomLeftCornerX + (%bottomLeftCornerSizeX / 2);
   %bottomLeftCornerPosY = %bottomLeftCornerY - (%bottomLeftCornerSizeY / 2);
   
   %bottomRightCornerSize = %this.bottomRightCorner.getSize();
   %bottomRightCornerSizeX = getWord(%bottomRightCornerSize, 0);
   %bottomRightCornerSizeY = getWord(%bottomRightCornerSize, 1);
   
   %scrollBaseSize = %this.scrollBase.getSize();
   %scrollBaseSizeX = getWord(%scrollBaseSize, 0);
   %scrollBaseSizeY = getWord(%scrollBaseSize, 1);
   
   %bottomRightCornerPosX = %bottomRightCornerX - (%bottomRightCornerSizeX / 2);
   %bottomRightCornerPosY = %bottomRightCornerY - (%bottomRightCornerSizeY / 2);
   
   %this.topLeftCorner.mount(%this, %this.getLocalPoint(%topLeftCornerPosX, %topLeftCornerPosY));
   %this.topRightCorner.mount(%this, %this.getLocalPoint(%topRightCornerPosX, %topRightCornerPosY));
   %this.bottomLeftCorner.mount(%this, %this.getLocalPoint(%bottomLeftCornerPosX, %bottomLeftCornerPosY));
   %this.bottomRightCorner.mount(%this, %this.getLocalPoint(%bottomRightCornerPosX, %bottomRightCornerPosY));
   
   %cornerWidth = getWord($AnimationBuilder::sizes::corner, 0);
   %cornerHeight = getWord($AnimationBuilder::sizes::corner, 1);
   
   %topBorderX = %posX;
   %topBorderY = %posY - (%height / 2) + (%cornerHeight / 2);
   
   %topBorderWidth = %width - (%cornerWidth * 2);
   %topBorderHeight = %cornerHeight;
   
   %bottomBorderX = %posX;
   %bottomBorderY = %posY + (%height / 2) - (%cornerHeight / 2);
   
   %bottomBorderWidth = %width - (%cornerWidth * 2);
   %bottomBorderHeight = %cornerHeight;
   
   %leftBorderX = %posX - (%width / 2) + (%cornerWidth / 2);
   %leftBorderY = %posY;
   
   %leftBorderWidth = %cornerWidth;
   %leftBorderHeight = %height - (%cornerHeight * 2);
   
   %rightBorderX = %posX + (%width / 2) - (%cornerWidth / 2);
   %rightBorderY = %posY;
   
   %rightBorderWidth = %cornerWidth;
   %rightBorderHeight = %height - (%cornerHeight * 2);
   
   %scrollBaseX = %posX;
   %scrollBaseY = %posY - (%height / 2) + (%scrollBaseSizeY / 2);
   
   %this.topBorder.mount(%this, %this.getLocalPoint(%topBorderX, %topBorderY));
   %this.topBorder.setSize(%topBorderWidth, %topBorderHeight);
   %this.bottomBorder.mount(%this, %this.getLocalPoint(%bottomBorderX, %bottomBorderY));
   %this.bottomBorder.setSize(%bottomBorderWidth, %bottomBorderHeight);
   %this.leftBorder.mount(%this, %this.getLocalPoint(%leftBorderX, %leftBorderY));
   %this.leftBorder.setSize(%leftBorderWidth, %leftBorderHeight);
   %this.rightBorder.mount(%this, %this.getLocalPoint(%rightBorderX, %rightBorderY));
   %this.rightBorder.setSize(%rightBorderWidth, %rightBorderHeight);
   %this.scrollBase.mount(%this, %this.getLocalPoint(%scrollBaseX, %scrollBaseY));
   
   %this.scrollBase.startPosition = %scrollBaseX SPC %scrollBaseY; 
}
function ABScrollWindow::positionScrollBase(%this)
{
   %selectionPos = %this.scrollBar.getSelectionPosition(); 
   %contentSize = %this.getContentSize();
   %contentHeight = getWord(%contentSize, 1);
   
   %windowSize = %this.getSize();
   %windowHeight = getWord(%windowSize, 1);
   
   %windowPos = %this.getPosition();
   %windowPosX = getWord(%windowPos, 0);
   
   %scrollPos = %this.scrollBase.startPosition;
   %scrollX = getWord(%scrollPos, 0);
   
   %scrollBaseSize = %this.scrollBase.getSize();
   %scrollBaseSizeY = getWord(%scrollBaseSize, 1);
   
   %scrollableHeight = %contentHeight - %windowHeight;

   %selectedPos = -((%selectionPos / 100) * %scrollableHeight) - (%windowHeight / 2) - (%scrollBaseSizeY / 2);
   
   %this.scrollBase.mount(%this, %this.getLocalPoint(%scrollX, %selectedPos));
}

function ABScrollWindow::setWindowLayers(%this, %background, %borders)
{
   %this.leftBorder.setLayer(%borders);   
   %this.rightBorder.setLayer(%borders);
   %this.topBorder.setLayer(%borders);
   %this.bottomBorder.setLayer(%borders);
   %this.background.setLayer(%background);
}
