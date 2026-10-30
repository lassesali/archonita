function loadAnimationBuilderSelection()
{
   $AnimationBuilder::selectionBasePos = "2000 2000";
   $AnimationBuilder::selectionBaseExtent = "400 400";
   
   AnimationBuilderImageSceneWindow.setSceneGraph( $AnimationBuilder::SceneGraph );
   AnimationBuilderImageSceneWindow.setCurrentCameraPosition($AnimationBuilder::selectionBasePos SPC $AnimationBuilder::selectionBaseExtent);
   
   AnimationBuilder::populateSelectionList();
}

function AnimationBuilder::populateSelectionList()
{
   %datablockSet = getT2DDatablockSet();
   %datablockCount = %datablockSet.getCount();
   
   AnimationBuilderImageList.clearItems();
   
   for(%i=0;%i<%datablockCount;%i++)
   {
      %datablockID = %datablockSet.getObject(%i).getID();
      %datablock = %datablockID.getName();
      if(%datablockID.getClassName() $= "t2dImageMapDatablock" && %datablockID.getImageMapMode() !$= "FULL" && !$ignoredDatablockSet.isMember(%datablockID))
         AnimationBuilderImageList.addItem(%datablock);      
   }   
}

function AnimationBuilder::refreshSelectionPreview()
{
   AnimationBuilder::PurgeResources( $AnimationBuilder::ImagePreviewGroup );
   
   %basePos = $AnimationBuilder::selectionBasePos;
   %baseX = getWord(%basePos, 0);
   %baseY = getWord(%basePos, 1);
   
   %baseExtent = $AnimationBuilder::selectionBaseExtent;
   %baseWidth = getWord(%baseExtent, 0);
   %baseHeight = getWord(%baseExtent, 1);
   
   %colSpace = 2;
   %rowSpace = 2;
   
   %imageMap = $AnimationBuilder::selectedImageMap;
   
   if(!isObject(%imageMap))
      return;
      
   if(%imageMap.getImageMapMode() $= "FULL")
   {
      %sprite = new t2dStaticSprite(ImagePreviewSprite) { sceneGraph = $AnimationBuilder::SceneGraph; };
      %sprite.setSize(%baseExtent);
      %sprite.setPosition(%basePos);
      %sprite.setImageMap(%imageMap);
      $AnimationBuilder::ImagePreviewGroup.add(%sprite);
   } else
   {
      %frameCount = %imageMap.getFrameCount();
         
      %sqrt = mSqrt(%frameCount);
      %div = mCeil(%sqrt);

      %objWidth = (%baseWidth / %div) - (%rowSpace);
      %objHeight = (%baseHeight / %div) - (%colSpace);
         
      %posX = %baseX - (%baseWidth/2) + (%objWidth/2);
      %posY = %baseY - (%baseHeight/2) + (%objHeight/2);
         
      %colCount = 0;
      %rowCount = 0;
         
      for(%i=0;%i<%frameCount;%i++)
      {
         %sprite = new t2dStaticSprite(ImagePreviewSprite) { sceneGraph = $AnimationBuilder::SceneGraph; };
         %sprite.setSize(%objWidth, %objHeight);
         %sprite.setPosition(%posX, %posY);
         %sprite.setImageMap(%imageMap, %i);
         $AnimationBuilder::ImagePreviewGroup.add(%sprite);
            
         if(%colCount >= %div-1)
         {
            %rowCount++;
            %colCount = 0;
            %posX = %baseX - (%baseWidth/2) + (%objWidth/2);
            %posY = %baseY - (%baseHeight/2) + (%objHeight/2) + ((%objHeight + %rowSpace) * %rowCount);            
         } else
         {
            %colCount++;
            %posX = %baseX - (%baseWidth/2) + (%objWidth/2) + ((%objWidth + %colSpace) * %colCount);
         }
      }
   }
}

function AnimationBuilderImageList::onSelect(%this, %id, %text)
{
   $AnimationBuilder::selectedImageMap = %text;
   AnimationBuilder::refreshSelectionPreview();      
}

function AnimationBuilder::SelectImage()
{
   $selectedImageMap = $AnimationBuilder::selectedImageMap;
   AnimationBuilder::generateName( $selectedImageMap );
   AnimationBuilderUI.onStartUp();   
}

function AnimationBuilder::generateName( %imageMap )
{
   if($AnimationBuilder::edit)
   {
      %name = $AnimationBuilder::editName.getName();  
   } else
   {
      %imageMap = $selectedImagemap;
   
      // lets make our name the bame of the image
      %name = %imageMap.getSrcBitmapName();
      %name = AnimationBuilder::getFileName(%name);
   
      // if there is already an object with this name
      // then lets look through the names with numbers
      // appended to them until we have a valid name
      %base = %name @ "Animation";
      if(isObject(%base))
      {
         %base = %name @ "Animation";
         for(%i=0;isObject(%name);%i++)
            %name = %base @ %i;
      } else
      {
         %name = %base;
      }
   }
   
   $AnimationBuilder::animName = %name;
}

function AnimationBuilder::getFileName( %file )
{
   %len = strLen(%file);
   
   for(%i=%len-1;%i>=0;%i--)
   {
      %char = getSubStr(%file, %i, 1);
      
      if(%char $= "/")
      {
         %returnString = getSubStr(%file, %i + 1, %len - %i);
         return %returnString;
      }
   }
   
   return %file;
}