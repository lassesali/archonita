function AnimationBuilder::createAnimationDatablock()
{
   txtAnimationName.setText($AnimationBuilder::animName);
   CreateAnimationWindow.setVisible(true);   
}

function AnimationBuilder::deleteAnimation()
{
  checkAnimationReference($AnimationBuilder::editID.getName(), Toolmanager.getLastWindow().getScenegraph(), "");  
}

function AnimationBuilder::showPreviewControls()
{
   AnimationBuilderAnimationControl.setVisible(false);
   AnimationBuilderStoryBoardFrameControl.setVisible(false);
   AnimationBuilderPreviewControl.setVisible(true);  
}

function AnimationBuilder::hidePreviewControls()
{
   AnimationBuilderPreviewControl.setVisible(false);
   AnimationBuilderStoryBoardFrameControl.setVisible(true);
   AnimationBuilderAnimationControl.setVisible(true);
}

function AnimationBuilderPreviewSlider::onMouseDragged(%this)
{
   AnimationBuilder::onPreviewSliderMove();   
}

function AnimationBuilderPreviewSpeedSlider::onMouseDragged(%this)
{
   AnimationBuilder::onPreviewSliderTimeMove();   
}

function AnimationBuilder::setupPreviewSliders()
{   
   %slider = AnimationBuilderPreviewSlider;
   
   if($AnimationBuilder::allFrames)
   {
      %imageMap = $AnimationBuilder::selectedImageMap;
      %frameCount = %imageMap.getFrameCount();   
   } else
   {
      %frameCount = $AnimationBuilder::storyboardWindow.objList.getCount() - 1;
   }

   %slider.ticks = %frameCount-1;
   %slider.range = "0" SPC %frameCount-1;
   %slider.setValue("0");
   
   %slider = AnimationBuilderPreviewSpeedSlider;
   
   %slider.ticks = "15";
   %slider.range = "0 2";
   %slider.setValue("1");
}

function AnimationBuilder::onPreviewSliderMove()
{
   %slider = AnimationBuilderPreviewSlider;
   
   %value = %slider.getValue();
   
   %frame = mFloor(%value + 0.5);
   
   if($AnimationBuilder::previewWindow.previewAnimSprite.getIsAnimationFinished())
   {
      $AnimationBuilder::previewWindow.previewAnimSprite.playAnimation($AnimationBuilder::previewWindow.previewAnimDB);
      $AnimationBuilder::previewWindow.reset();
      $AnimationBuilder::previewWindow.playing = true;
      $AnimationBuilder::previewWindow.pauseButton();
   }
   
   $AnimationBuilder::previewWindow.previewAnimSprite.setAnimationFrame(%frame);
}

function AnimationBuilder::onPreviewSliderTimeMove()
{
   %slider = AnimationBuilderPreviewSpeedSlider;
   
   %value = %slider.getValue();
   
   // grab the time that's set in the builder
   %time = animationDatablockTime.getValue();
   
   %ratio = 1 / %value;
   
   %newTime = %time * %ratio;
   
   $AnimationBuilder::previewWindow.previewAnimDB.animationTime = %newTime;  
   
   $AnimationBuilder::previewWindow.previewAnimSprite.playAnimation($AnimationBuilder::previewWindow.previewAnimDB, false, $AnimationBuilder::previewWindow.previewAnimSprite.getAnimationFrame()); 
}

function exitAnimationEditor()
{
   Canvas.popDialog( AnimationEditorGUI ); 
   
   AnimationBuilder::Destroy(); 
}

function AnimationBuilder::createFinalAnimation()
{
   if($AnimationBuilder::edit)
   {
      // if editing, store the old name and then delete the old animation
      %oldName = $AnimationBuilder::editID.getName();
      $AnimationBuilder::editID.delete();
   }
   
   //grab the animation name from the dialog
   $AnimationBuilder::animName = txtAnimationName.getValue();
   
   //create the animation
   %animation = AnimationBuilder::createNewAnimation(%oldName);
   
   //if we are editing we must check every reference to this animation
   //and change them
   if($AnimationBuilder::edit)
   {
      %list = getAnimReferenceList(%oldName, Toolmanager.getLastWindow().getScenegraph());  
      
      if(isObject(%list))
      {    
         %listCount = %list.getCount();
      
         for(%i=0;%i<%listCount;%i++)
         {
            %anim = %list.getObject(%i);  
            
            %anim.playAnimation($AnimationBuilder::animName);        
         }
      }
   }
 
   // Add to project managed
   if( isObject( LBProjectObj ) )
      LBProjectObj.addDatablock( %animation, true );
   else 
      error("ERROR: Unable to add animation to project datablock set, project object doesn't exist!");
      
   GuiFormManager::SendContentMessage($LBCAnimatedSprite, "", "refresh");
   
   CreateAnimationWindow.setVisible(false);
   exitAnimationEditor();
}

function AnimationBuilder::createNewAnimation(%oldName)
{
   if(!$AnimationBuilder::edit)
   {
      for(%i=0;isObject($AnimationBuilder::animName);%i++)
      {
         $AnimationBuilder::animName = $AnimationBuilder::animName @ %i;         
      }
   } else
   {
      for(%i=0;(isObject($AnimationBuilder::animName) && $AnimationBuilder::animName !$= %oldName);%i++)
      {
         $AnimationBuilder::animName = $AnimationBuilder::animName @ %i;         
      }
   }
   
   %anim = $AnimationBuilder::previewWindow.createPreviewAnim($AnimationBuilder::animName);
   
   return %anim;   
}

function AnimationBuilder::createEditedAnimation()
{
 
}

function getAnimReferenceList(%anim, %sceneGraph)
{
   %objCount = %sceneGraph.getSceneObjectCount();
   
   $AnimationBuilder::animationObjectReferenceList = new SimSet();
   %list = $AnimationBuilder::animationObjectReferenceList;
   
   for(%i=0;%i<%objCount;%i++)
   {
      %obj = %sceneGraph.getSceneObject(%i);
      
      switch$ (%obj.getClassName())
      {
         case "t2dAnimatedSprite":       
            %animation = %obj.getAnimationName();
            if(%animation $= %anim.getName())
               %list.add(%obj);
      }
   }
   
   if(%list.getCount() > 0)
      return %list;  
   else
      return -1;
}

function checkAnimationReference(%anim, %sceneGraph, %callback)
{
   %val = getAnimReferenceList(%anim, %sceneGraph);
   
   $referenceRemoveScene = %sceneGraph;
   $referenceimagemap = %imageMap;
   
   if(%val == -1)
   {
      // No references to this animation 
      %message = "Are you sure you want to delete this animation?";
      
      %okCallback = %anim @ ".delete();";
   } else
   {
      %list = $AnimationBuilder::animationObjectReferenceList;
      %animSpriteCount = %list.getCount(); 
    
      if(%animSpriteCount > 0)
      {
         %details = %details @ "Animated Sprites:\n";
      
         for(%i=0;%i<%animSpriteCount;%i++)
         {
            %obj = %list.getObject(%i);
            %details = %details @ %obj.getName() @ "(object ID = " @ %obj.getId() @ ")\n";         
         }
      
         %details = %details @ "\n";
      }
    
      %message = "You currently have " @ %animSpriteCount SPC "animated sprite(s) referencing this image." @ 
                 "  If you proceed all of these animated sprite(s) will be deleted, are you sure you want to delete this animation?" @
                 "(click details to see more info)";
      %okCallback = "clearReferenceListObjectsName(animated);" @ 
                    %anim @ ".delete();";
   }
      
   %okCallback = "LBProjectObj.removeDatablock(" @ %anim @ ", true);" @ "destroyAllObjectLibrary();" @ %okCallback @ "refreshAllObjectLibrary();" @ %callback @ "exitAnimationEditor();";
   
   MessageBoxOKCancelDetails("Delete Animation?", %message, %details, %okCallback, %cancelCallback);
}

//echo(checkImageMapReference(imageMap1234, 2535));
function getImageReferenceList(%imageMap, %sceneGraph)
{
   %objCount = %sceneGraph.getSceneObjectCount();
   
   $AnimationBuilder::objectImageReferenceList = new SimSet();
   %list = $AnimationBuilder::objectImageReferenceList;
   
   $AnimationBuilder::animDatablockImageReferenceList = new SimSet();
   %animDatablockList = $AnimationBuilder::animDatablockImageReferenceList;
   
   $AnimationBuilder::linkDatablockImageReferenceList = new SimSet();
   %linkDatablockList = $AnimationBuilder::linkDatablockImageReferenceList;
   
   $AnimationBuilder::animatedObjectImageReferenceList = new SimSet();
   %animObjList = $AnimationBuilder::animatedObjectImageReferenceList;
   
   for(%i=0;%i<%objCount;%i++)
   {
      %obj = %sceneGraph.getSceneObject(%i);
      
      switch$ (%obj.getClassName())
      {
         case "t2dStaticSprite":
            %image = %obj.getImageMap();
            if(%image.getName() $= %imageMap.getName())
               %list.add(%obj);
         
         case "t2dScroller":
            %image = %obj.getImageMap();
            if(%image.getName() $= %imageMap.getName())
               %list.add(%obj);       
      }
   } 
   
   %datablockSet = getT2DDatablockSet();
   %datablockCount = %datablockSet.getCount();
   
   for(%i=0;%i<%datablockCount;%i++)
   {
      %datablockID = %datablockSet.getObject(%i);
      %datablock = %datablockID.getName();
      
      if( !isObject( %datablock ) )
         continue;
         
      switch$ (%datablockID.getClassName()) 
      {
         case "t2dAnimationDatablock":  
            if(%datablock.imageMap $= %imageMap.getName())
            {
               %animDatablockList.add(%datablock);
            }
            
         case "t2dImageMapDatablock":
            %mode = %datablock.getImageMapMode();
            
            if(%mode $= "LINK")
            {
               %linkList = %datablock.linkImageMaps;
               %linkCount = getWordCount(%linkList);
               for(%j=0;%j<%linkCount;%j++)
               {
                  %linkImage = getWord(%linkList, %j);
                  
                  if(%linkImage $= %imageMap.getName())
                  {
                     %linkDatablockList.add(%datablock);   
                  }
               }
            }
      }
   }

   if(%animDatablockList.getCount() > 0)
   {
      %objCount = %sceneGraph.getSceneObjectCount();
      
      for(%i=0;%i<%objCount;%i++)
      {
         %obj = %sceneGraph.getSceneObject(%i);
         
         switch$ (%obj.getClassName())
         {
            case "t2dAnimatedSprite":
               
               %animation = %obj.getAnimationName();
            
               for(%j=0;%j<%animDatablockList.getCount();%j++)
               {
                  %db = %animDatablockList.getObject(%j);

                  if(%animation $= %db.getName())
                  {
                     %animObjList.add(%obj);
                     %j = %animDatablockList.getCount();                     
                  }
               }
         
         }
      }          
   }
   
   %return = 0;
   
   if(%list.getCount() > 0 )
      %return++;   
       
   if(%animDatablockList.getCount() > 0)
      %return += 2;
      
   if(%animObjList.getCount() > 0)
      %return += 4;
      
   if(%linkDatablockList.getCount() > 0)
      %return += 8;
      
   return %return;
}

function checkImageMapReference(%imageMap, %sceneGraph, %callback)
{
   %val = getImageReferenceList(%imageMap, %sceneGraph);

   $referenceRemoveScene = %sceneGraph;
   $referenceimagemap = %imageMap;
   
   %staticSpriteList = $AnimationBuilder::objectImageReferenceList;
   %animSpriteList = $AnimationBuilder::animatedObjectImageReferenceList;
   %animDBList = $AnimationBuilder::animDatablockImageReferenceList;
   %linkDBList = $AnimationBuilder::linkDatablockImageReferenceList;
   
   %staticSpriteCount = %staticSpriteList.getCount();
   %animSpriteCount = %animSpriteList.getCount();
   %animDBCount = %animDBList.getCount();
   %linkDBCount = %linkDBList.getCount();
   
   if(%staticSpriteCount > 0)
   {
      %details = %details @ "Static Sprites:\n";
      
      for(%i=0;%i<%staticSpriteCount;%i++)
      {
         %obj = %staticSpriteList.getObject(%i);
         %details = %details @ %obj.getName() @ "  (object ID = " @ %obj @ ") \n";         
      }
      
      %details = %details @ "\n";
   }
   
   if(%animSpriteCount > 0)
   {
      %details = %details @ "Animated Sprites:\n";
      
      for(%i=0;%i<%animSpriteCount;%i++)
      {
         %obj = %animSpriteList.getObject(%i);
         %details = %details @ %obj.getName() @ "  (object ID = " @ %obj @ ") \n";         
      }
      
      %details = %details @ "\n";
   }
   
   if(%animDBCount > 0)
   {
      %details = %details @ "Animations:\n";
      
      for(%i=0;%i<%animDBCount;%i++)
      {
         %obj = %animDBList.getObject(%i);
         %details = %details @ %obj.getName() @ "  (object ID = " @ %obj @ ") \n";         
      }
      
      %details = %details @ "\n";
   }
   
   if(%linkDBCount > 0)
   {
      %details = %details @ "Link Images:\n";
      
      for(%i=0;%i<%linkDBCount;%i++)
      {
         %obj = %linkDBList.getObject(%i);
         %details = %details @ %obj.getName() @ "  (object ID = " @ %obj @ ") \n";         
      }
      
      %details = %details @ "\n";
   }
   
   switch (%val)
   {
      case 0:
         // no references found
         %message = "Are you sure you want to delete this image?";
         %okCallback = %imageMap @ ".delete();";
      
      case 1:
         // only static sprites  
         %message = "You currently have " @ %staticSpriteCount SPC "static sprite(s) referencing this image." @ 
                    "  If you proceed all of these static sprite(s) will be deleted, are you sure you want to delete this image?" @
                    "(click details to see more info)";
         %okCallback = "clearReferenceListObjectsName(staticSprites);" @ 
                       %imageMap @ ".delete();";

         
      case 2:
         // only animation datablocks (no animated sprites referencing datablocks)
         %message = "You currently  have " @ %animDBCount SPC "animation(s) referencing this image." @  
                    "  If you proceed all of these animation(s) will be deleted" @ 
                    ", are you sure you want to delete this image?" @
                    "\n(click details to see more info)";
         %okCallback = "clearReferenceListObjectsName(animation);" @
                       %imageMap @ ".delete();";
         
      case 3:
         // static sprites and animation datablocks (no animated sprites referencing datablocks)
         %message = "You currently have " @ %staticSpriteCount SPC "static sprites referencing this image." @
                    "You also have " @ %animDBCount SPC "animation(s) referencing this image." @  
                    "  If you proceed all of these static sprite(s) and animation(s) will be deleted" @ 
                    ", are you sure you want to delete this image?" @
                    "\n(click details to see more info)";
         %okCallback = "clearReferenceListObjectsName(staticSprites);" @
                       "clearReferenceListObjectsName(animation);" @
                       %imageMap @ ".delete();";
         
      case 4:
         // INVALID: only animated sprites with references of animated datablocks (should never be just this, must
         // also have animated datablocks, somethings wrong with the check if this happens)
         
      case 5:
         // INVALID: static sprites and animated sprites, no animated datablocks, again this should never happen,
         // there must be some animated datablock and the check is invalid
         
      case 6:
         // animation datablocks and animated sprites
         %message = "You currently have " @ %animDBCount SPC "animation(s) referencing this image and " @
                    %animSpriteCount SPC "animated sprite(s) referencing those animation(s)." @  
                    "  If you proceed all of these animation(s)" @ 
                    "and animated sprite(s) will be deleted" @ 
                    ", are you sure you want to delete this image?" @
                    "\n(click details to see more info)";
         %okCallback = "clearReferenceListObjectsName(animatedSprites);" @ 
                       "clearReferenceListObjectsName(animation);" @
                       %imageMap @ ".delete();";
         
      case 7:
         // static sprites, animation datablocks, and animated sprites found
         %message = "You currently have " @ %staticSpriteCount SPC "static sprite(s) referencing this image." @
                    "You also have " @ %animDBCount SPC "animation(s) referencing this image and " @
                    %animSpriteCount SPC "animated sprite(s) referencing those animation(s)." @  
                    "  If you proceed all of these static sprite(s), animation(s), " @ 
                    "and animated sprite(s) will be deleted" @ 
                    ", are you sure you want to delete this image?" @
                    "\n(click details to see more info)";
         %okCallback = "clearReferenceListObjectsName(staticSprites);" @
                       "clearReferenceListObjectsName(animatedSprites);" @ 
                       "clearReferenceListObjectsName(animation);" @
                       %imageMap @ ".delete();";
         
      case 8:
         // only link image maps found
         %message = "You currently have " @ %linkDBCount SPC "link images referencing this image." @  
                    "  If you proceed this image will be removed from " @ 
                    "the link image map(s), are you sure you want to delete this image?" @
                    "\n(click details to see more info)";
         %okCallback = "clearReferenceListObjectsName(link);" @ 
                       %imageMap @ ".delete();";
         
      case 9:
         // link image maps found as well as static sprites
         %message = "You currently have " @ %staticSpriteCount SPC "static sprites referencing this image." @
                    "  You also have " @ %linkDBCount SPC "link images referencing this image." @  
                    "  If you proceed all of these static sprite(s) will be deleted and this image will be removed from " @ 
                    "the link image(s), are you sure you want to delete this image?" @
                    "\n(click details to see more info)";
         %okCallback = "clearReferenceListObjectsName(staticSprites);" @
                       "clearReferenceListObjectsName(link);" @ 
                       %imageMap @ ".delete();";
         
      case 10:
         // link image maps found as well as animation datablocks
         %message = "You currently  have " @ %animDBCount SPC "animation(s) referencing this image." @  
                    "  You also have " @ %linkDBCount SPC "link image(s) referencing this image." @
                    "  If you proceed all of these animation(s) will be deleted" @ 
                    " and this image will be removed from the link image(s)," @
                    " are you sure you want to delete this image?" @
                    "\n(click details to see more info)";
         %okCallback = "clearReferenceListObjectsName(animation);" @
                       "clearReferenceListObjectsName(link);" @
                       %imageMap @ ".delete();";
         
      case 11:
         // link image maps, static sprites, and animation datablocks found
         %message = "You currently have " @ %staticSpriteCount SPC "static sprites referencing this image," @
                    " as well as " @ %animDBCount SPC "animation(s) referencing this image." @  
                    "  You also have " @ %linkDBCount SPC "link image(s) referencing this image." @
                    "  If you proceed all of these these static sprite(s) will be deleted and animation datablocks will be deleted," @ 
                    " this image will also be removed from the link image(s)," @
                    " are you sure you want to delete this image?" @
                    "\n(click details to see more info)";
         %okCallback = "clearReferenceListObjectsName(staticSprites);" @
                       "clearReferenceListObjectsName(animation);" @
                       "clearReferenceListObjectsName(link);" @
                       %imageMap @ ".delete();";
         
      case 12:
         // INVALID: link image maps and animated sprites, this should NOT happen since an animated sprite
         // requires an animation datablock
         
      case 13:
         // INVALID: link image maps, animated sprites, and static sprites - no animation datablocks (though
         // there should be if an animated sprite was found
         
      case 14:
         // link image maps, animation datablocks, and animated sprites found
         %message = "You currently have " @ %animDBCount SPC "animation(s) referencing this image and " @
                    %animSpriteCount SPC "animated sprite(s) referencing those animation(s)." @  
                    "  You also have " @ %linkDBCount SPC "link image(s) referencing this image." @
                    "  If you proceed all of these animated sprite(s) will be deleted and animation datablocks will be deleted," @ 
                    " this image will also be removed from the link image(s)," @
                    " are you sure you want to delete this image?" @
                    "\n(click details to see more info)";
         %okCallback = "clearReferenceListObjectsName(animatedSprites);" @
                       "clearReferenceListObjectsName(animation);" @
                       "clearReferenceListObjectsName(link);" @
                       %imageMap @ ".delete();";
         
      case 15:
         // All --- link image maps, static sprites, animation datablocks, as well as animated sprites found
         %message = "You currently have " @ %staticSpriteCount SPC "static sprites and " 
                    @ %animDBCount SPC "animation(s) referencing this image and " @
                    %animSpriteCount SPC "animated sprite(s) referencing those animation(s)." @  
                    "  You also have " @ %linkDBCount SPC "link image(s) referencing this image." @
                    "  If you proceed all of these these static sprite(s) and animated sprite(s) will be deleted and animation datablocks will be deleted," @ 
                    " this image will also be removed from the link image(s)," @
                    " are you sure you want to delete this image?" @
                    "\n(click details to see more info)";
         %okCallback = "clearReferenceListObjectsName(staticSprites);" @
                       "clearReferenceListObjectsName(animatedSprites);" @
                       "clearReferenceListObjectsName(animation);" @
                       "clearReferenceListObjectsName(link);" @
                       %imageMap @ ".delete();";
   }
   
   %okCallback = "destroyAllObjectLibrary();" @ %okCallback @ "LBProjectObj.persistToDisk(true,false,true);" @ %callback @ "refreshAllObjectLibrary();";
   
   MessageBoxOKCancelDetails("Delete Image?", %message, %details, %okCallback , %cancelCallback);
}

function destroyAllObjectLibrary()
{
   GuiFormManager::sendContentMessage($LBCStaticSprite, "", "destroy");
   GuiFormManager::sendContentMessage($LBCScroller, "", "destroy");
   GuiFormManager::sendContentMessage($LBCAnimatedSprite, "", "destroy");
}

function refreshAllObjectLibrary()
{
   GuiFormManager::sendContentMessage($LBCStaticSprite, "", "refresh"); 
   GuiFormManager::sendContentMessage($LBCScroller, "", "refresh");
   GuiFormManager::sendContentMessage($LBCAnimatedSprite, "", "refresh");   
}

function clearReferenceListObjectsName(%name)
{
   //error("clearing " @ %name);
   
   switch$ (%name)
   {
      case "link":
         clearReferenceListObjects($AnimationBuilder::linkDatablockImageReferenceList);
      
      case "staticSprites":
         clearReferenceListObjects($AnimationBuilder::objectImageReferenceList);
      
      case "animatedSprites":
         clearReferenceListObjects($AnimationBuilder::animatedObjectImageReferenceList);
      
      case "animation":
         clearReferenceListObjects($AnimationBuilder::animDatablockImageReferenceList);     
         
      case "animated":
         clearReferenceListObjects($AnimationBuilder::animationObjectReferenceList);
   }
}

function clearReferenceListObjects(%list)
{
   %count = %list.getCount();
   
   for(%i=0;%i<%count;%i++)
   {
      %obj = %list.getObject(%i);
      
      switch$ (%obj.getClassName())
      {
         case "t2dStaticSprite":
            %obj.safeDelete();
         
         case "t2dAnimatedSPrite":
            %obj.safeDelete();
            
         case "t2dAnimationDatablock":
            %obj.delete();
            
         case "t2dImageMapDatablock":
            %linkList = %obj.linkImageMaps;
            %linkCount = getWordCount(%linkList);
            
            for(%j=0;%j<%linkCount;%j++)
            {
               %linkImage = getWord(%linkList, %j);
               
               if(%linkImage !$= $referenceImageMap)
               {
                  if(%linkString $= "")
                  {
                     %linkString = %linkImage;
                  } else
                  {
                     %linkString = %linkString SPC %linkImage;                     
                  }
               }
            }
            
            %obj.linkImageMaps = %linkString;
            
            %sceneGraph = $referenceRemoveScene;
            
            %obj.compile();
            
            %objCount = %sceneGraph.getSceneObjectCount();
            
            %obj.dump();
      }
   }
}

function AnimationBuilder::frameTimeCallback()
{
   %time = animationDatablockTime.getValue();
   
   %time = AnimationBuilder::enforceNumberFormat(%time);
   
   animationDatablockTime.setText(%time);   
}

function AnimationBuilder::enforceNumberFormat(%num)
{
   %firstChar = getSubStr(%num, 0, 1);
   
   if(%firstChar $= ".")
   {
      %num = "0" @ %num;      
   } else if(%num <= 0)
   {
      %num = 0.1;
   }
   
   return %num;
}