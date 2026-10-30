datablock t2dImageMapDatablock(AnimationBuilderRightArrowImageMap)
{
   filterpad = false;
   imageMode = full;
   imageName = "./../images/rightarrow";
};

datablock t2dImageMapDatablock(imageborderImageMap)
{
   filterpad = false;
   imageMode = full;
   imageName = "./../images/imageborder";
};

datablock t2dImageMapDatablock(selectedimageborderImageMap)
{
   filterpad = false;
   imageMode = full;
   imageName = "./../images/selectedimageborder";
};

datablock t2dImageMapDatablock(blankImageMap)
{
   filterpad = false;
   imageMode = full;
   imageName = "./../images/blank";
};


datablock t2dImageMapDatablock(AnimationBuilderThemeLight)
{
   filterpad = true;
   imageMode = key;
   imageName = "tools/editorClasses/gui/images/panel_light";
};

datablock t2dImageMapDatablock(AnimationBuilderThemeMedium)
{
   filterpad = true;
   imageMode = key;
   imageName = "tools/editorClasses/gui/images/panel_medium";
};

datablock t2dImageMapDatablock(AnimationBuilderThemeDark)
{
   filterpad = true;
   imageMode = key;
   imageName = "tools/editorClasses/gui/images/panel_dark";
};

datablock t2dImageMapDatablock(AnimationBuilderThemeScrollBar)
{
   filterpad = true;
   imageMode = key;
   imageName = "tools/editorClasses/gui/images/scroll";
};

new t2dImageMapDatablock(ABFontImageMap) {
   canSaveDynamicFields = "1";
   imageName = "tools/animationEditor/data/images/font1";
   imageMode = "CELL";
   frameCount = "-1";
   filterMode = "SMOOTH";
   filterPad = "0";
   preferPerf = "0";
   cellRowOrder = "1";
   cellOffsetX = "0";
   cellOffsetY = "0";
   cellStrideX = "0";
   cellStrideY = "0";
   cellCountX = "-1";
   cellCountY = "-1";
   cellWidth = "16";
   cellHeight = "16";
   preload = "1";
   allowUnload = "0";
   filterpad = true;
};

$ignoredDatablockSet.add(imageborderImageMap);
$ignoredDatablockSet.add(selectedimageborderImageMap);
$ignoredDatablockSet.add(blankImageMap);

$ignoredDatablockSet.add(AnimationBuilderRightArrowImageMap);
$ignoredDatablockSet.add(AnimationBuilderThemeLight);
$ignoredDatablockSet.add(AnimationBuilderThemeMedium);
$ignoredDatablockSet.add(AnimationBuilderThemeDark);
$ignoredDatablockSet.add(AnimationBuilderThemeScrollBar);
$ignoredDatablockSet.add(ABFontImageMap);