function mouseOverActionIcons( %mod, %worldPos ) 
{

                                 iconDefence2.setLayer("31");
                                 iconAttack2.setLayer("31");
                                 iconMagic3.setLayer("31");
                                 iconDefence2.setSize("16 16");
                                 iconAttack2.setSize("16 16");
                                 iconMagic3.setSize("16 16"); 
                                 iconDefence2.setBlendAlpha("0.7");
                                 iconAttack2.setBlendAlpha("0.7");
                                 iconMagic3.setBlendAlpha("0.7");
                                 game.setHighlightedActionIcon(0);


                //lets get a list of all the objects at the clicked point in the t2dScene
                %objList = t2dScene.pickPoint(%worldPos);
                //lets get a count of how many objects in the list
                %objCount = getWordCount(%objList);
                
                
        
                //we will start looping through the list
                for(%i=0;%i<%objCount;%i++)
                {
                        //grabing the entry corresponding to the loop
                        %obj = getWord(%objList, %i);
                
                        //if we find an object in the list that "isSelectable = true"
                     
                        
                        if(%obj.isIconDefence == true || %obj.isIconMagic == true || %obj.isIconAttack == true )
                        {

                                 game.setHighlightedActionIcon(%obj.isIconDefence + %obj.isIconMagic*2 + %obj.isIconAttack*3);
                                 %selected = true;

                                //we toggle a value so we know we found an object that "isSelectable"
                                %obj.setLayer("30");
                                %obj.setBlendAlpha("1");
                                %obj.setSize("18 18");
                                //we kick out of the loop

                                %i = %objCount;
                                 
                        }                         
                }
        



}



function mouseOverMovementGrid( %mod, %worldPos )
{
      %hero = HeroGroup.getObject(game.getActivePlayer());

                                %hero.setSelectedTile("-1 -1");
   

                                for (%j=0;%j<4;%j++)
                                {
                                   if (isObject($movementgrid[%j])) 
                                   {
                                     $movementgrid[%j].setFrame($movementgrid[%j].defaultFrame);
                                   }
                                }


                //lets get a list of all the objects at the clicked point in the t2dScene
                %objList = t2dScene.pickPoint(%worldPos);
                //lets get a count of how many objects in the list
                %objCount = getWordCount(%objList);

                
        
                //we will start looping through the list
                for(%i=0;%i<%objCount;%i++)
                {
                        //grabing the entry corresponding to the loop
                        %obj = getWord(%objList, %i);
                
                        //if we find an object in the list that "isSelectable = true"
                     
                        
                        if(%obj.isSelectionFrame || %obj.isValintakehys)
                        {
                

                                 
                                //we toggle a value so we know we found an object that "isSelectable"
                                %selected = true;
                                //we kick out of the loop
                                %i = %objCount;
                                 
                        }                         
                }
        
                //if we found an "isSelectable" object
                if(%selected)
                {
                        %obj.setFrame("1");
                        %hero.setSelectedTile(%obj.positionOnBoard);

                     
                }
        
}


function mouseOverGreenGrid( %mod, %worldPos )
{
   %hero = HeroGroup.getObject(0);
   %hero.setSelectedTile("-1 -1");

   %width = (isObject(BoardConfig) && BoardConfig.width !$= "") ? BoardConfig.width : 4;
   %minY = (isObject(BoardConfig) && BoardConfig.playerPlacementMinY !$= "") ? BoardConfig.playerPlacementMinY : 2;
   %maxY = (isObject(BoardConfig) && BoardConfig.playerPlacementMaxY !$= "") ? BoardConfig.playerPlacementMaxY : 3;

   for (%ux = 0; %ux < %width; %ux++) 
   {
      for (%uy = %minY; %uy <= %maxY; %uy++) 
      {
         if ($board.obstacles[%ux, %uy] == 0) 
         {
            if (isObject($green[%ux, %uy])) 
            {
               $green[%ux, %uy].setFrame("0");
            }
         }
      }
   }

   for (%j = 0; %j < 4; %j++) 
   {
      if (isObject($setupNappulaSprite[%j]))
         $setupNappulaSprite[%j].setVisible(false);
   }

   %selected = false;
   %selectedObj = "";
   %objList = t2dScene.pickPoint(%worldPos);
   %objCount = getWordCount(%objList);

   for (%i = 0; %i < %objCount; %i++)
   {
      %obj = getWord(%objList, %i);
      if (%obj.isSelectionFrame || %obj.isValintakehys)
      {
         %selected = true;
         %selectedObj = %obj;
         %i = %objCount;
      }
   }

   if (%selected)
   {
      %selectedObj.setFrame("1");
      %hero.setSelectedTile(%selectedObj.positionOnBoard);

      if ($mouseObj::isObjMounted) 
      {
         %objMounted = $mouseObj::objMounted;
         if (isObject(%objMounted))
         {
            %lvlIndex = %objMounted.creatureLevel - 1;
            if (isObject($setupNappulaSprite[%lvlIndex]))
            {
               $setupNappulaSprite[%lvlIndex].setPosition(boardToWorldUnit(%hero.getSelectedTileX(), %hero.getSelectedTileY()));
               $setupNappulaSprite[%lvlIndex].setVisible(true);
            }
         }
      }
   }
}


function sceneWindow2D::onMouseMove( %this, %mod, %worldPos, %mouseClicks )
{

       
         
        if($debugMsg::mouse::callBacks)
                echo("Mouse Moving");

        //store the mouse position
        $mouseObj::pos = %worldPos;

        //reset stopped to false so it checks
        $mouseObj::stopped = false;
         if ($hiiri.piilota) 
            Canvas.hideCursor();
         else
            Canvas.showCursor();

         if (isObject(mouseTrail)) {
            
            mouseTrail.setPosition(%worldPos);
         }

         if (isObject($mouseObj::object)) {
            $mouseObj::object.setPosition(%worldPos);
            $mouseObj::object.updateMountedChildren();
         }

         if (getGameState() == $gamePreBattleState) 
         {
            mouseOverGreenGrid(%mod, %worldPos);
            if (%this.objectSelected)
            {
               if (%this.mouseDownPos $= "" || distBetween(%this.mouseDownPos, %worldPos) > 2.0)
                  %this.dragStarted = true;
            }
         }

         if (getGameState() == $gameBattleState) 
         {
            mouseOverMovementGrid(%mod, %worldPos);
            mouseOverActionIcons(%mod, %worldPos);
            
         }
         

}

function sceneWindow2D::onMouseDragged( %this, %mod, %worldPos, %mouseClicks )
{

        if($debugMsg::mouse::callBacks)
                echo("Mouse Dragging");



        //store the mouse position
        $mouseObj::pos = %worldPos;

        //reset stopped to false so it checks
        $mouseObj::stopped = false;
         if ($hiiri.piilota) 
            Canvas.hideCursor();
         else
            Canvas.showCursor();

         if (isObject(mouseTrail)) {
            mouseTrail.setPosition(%worldPos);
         }

         if (isObject($mouseObj::object)) {
            $mouseObj::object.setPosition(%worldPos);
            $mouseObj::object.updateMountedChildren();
         }

         if (getGameState() == $gamePreBattleState) 
         {
            mouseOverGreenGrid(%mod, %worldPos);
            if (%this.objectSelected)
            {
               if (%this.mouseDownPos $= "" || distBetween(%this.mouseDownPos, %worldPos) > 2.0)
                  %this.dragStarted = true;
            }
         }

         if (getGameState() == $gameBattleState) 
         {
            mouseOverMovementGrid(%mod, %worldPos);
            mouseOverActionIcons(%mod, %worldPos);
         }

}

function dismountobject() {
   
   
                if($mouseObj::isObjMounted)
                {       
                        if(isObject($mouseObj::objMounted))
                        {
                                if($debugMsg::mouse::selection)
                                        echo("dismounting object");

                                $mouseObj::objMounted.dismount();
                                $mouseObj::isObjMounted = false;
                        }
                }
                sceneWindow2D.objectSelected = False;
                %selected = false;

}


function sceneWindow2D::placeSelectedCharacter(%this, %obj)
{
   if (!isObject(%obj))
      %obj = $mouseObj::objMounted;

   if (!isObject(%obj))
   {
      %this.objectSelected = false;
      return false;
   }

   if (!isObject(HeroGroup))
   {
      %this.cancelCharacterPlacement(%obj);
      return false;
   }

   %hero = HeroGroup.getObject(0);
   if (!isObject(%hero))
   {
      %this.cancelCharacterPlacement(%obj);
      return false;
   }

   %targetRuutu = %hero.getSelectedTile();
   %targetX = %hero.getSelectedTileX();
   %targetY = %hero.getSelectedTileY();

   %width = (isObject(BoardConfig) && BoardConfig.width !$= "") ? BoardConfig.width : 4;
   %minY = (isObject(BoardConfig) && BoardConfig.playerPlacementMinY !$= "") ? BoardConfig.playerPlacementMinY : 2;
   %maxY = (isObject(BoardConfig) && BoardConfig.playerPlacementMaxY !$= "") ? BoardConfig.playerPlacementMaxY : 3;

   // Strict validation of target tile:
   // 1. Must not be unselected (-1 -1 or empty)
   // 2. Must fall within board array bounds: 0 <= x < width, minY <= y <= maxY (player deployment area)
   // 3. Tile must be free of obstacles ($board.obstacles[x, y] == 0)
   // 4. Must specifically correspond to a valid active green placement tile ($green[x, y])
   %isValidTile = false;
   if (%targetRuutu !$= "-1 -1" && %targetRuutu !$= "")
   {
      if (%targetX >= 0 && %targetX < %width && %targetY >= %minY && %targetY <= %maxY)
      {
         if ($board.obstacles[%targetX, %targetY] == 0 && isObject($green[%targetX, %targetY]))
         {
            %isValidTile = true;
         }
      }
   }

   if (!%isValidTile)
   {
      %this.cancelCharacterPlacement(%obj);
      return false;
   }

   %creatureLvl = %obj.creatureLevel;
   %iconIndex = %obj.index;

   // Attempt placement on board
   %placed = spawnPieceOnBoard(%creatureLvl, %targetX, %targetY, 0);
   if (!%placed)
   {
      %this.cancelCharacterPlacement(%obj);
      return false;
   }

   %obj.dismount();
   $mouseObj::isObjMounted = false;
   $mouseObj::objMounted = "";

   if (isObject($numb[%iconIndex]))
      $numb[%iconIndex].delete();

   updatePlacementHUD();

   if (isObject(%obj))
      %obj.delete();

   dismountobject();
   %this.objectSelected = false;
   %this.dragStarted = false;

   for (%j = 0; %j < 4; %j++)
   {
      if (isObject($setupNappulaSprite[%j]))
         $setupNappulaSprite[%j].setVisible(false);
   }

   %hero.setSelectedTile("-1 -1");
   return true;
}

function sceneWindow2D::cancelCharacterPlacement(%this, %obj)
{
   if (!isObject(%obj))
      %obj = $mouseObj::objMounted;

   if (isObject(%obj))
   {
      %obj.dismount();
      %trayPos = "";
      if (isObject($gridback[%obj.index]))
         %trayPos = $gridback[%obj.index].getPosition();
      else if (isObject($grid[%obj.index]))
         %trayPos = $grid[%obj.index].getPosition();

      if (%trayPos !$= "")
      {
         %obj.setPosition(%trayPos);
         if (isObject($grid[%obj.index]))
         {
            $grid[%obj.index].dismount();
            $grid[%obj.index].setPosition(%trayPos);
            $grid[%obj.index].mount(%obj, 0, 0, 0, true, true, true, true);
         }
      }

      if (isObject($icon[%obj.index]))
         $icon[%obj.index].setLayer("6");
      if (isObject($grid[%obj.index]))
         $grid[%obj.index].setLayer("5");
   }

   $mouseObj::isObjMounted = false;
   $mouseObj::objMounted = "";
   %this.selectedObj = "";
   %this.objectSelected = false;
   %this.dragStarted = false;

   for (%j = 0; %j < 4; %j++)
   {
      if (isObject($setupNappulaSprite[%j]))
         $setupNappulaSprite[%j].setVisible(false);
   }

   %width = (isObject(BoardConfig) && BoardConfig.width !$= "") ? BoardConfig.width : 4;
   %minY = (isObject(BoardConfig) && BoardConfig.playerPlacementMinY !$= "") ? BoardConfig.playerPlacementMinY : 2;
   %maxY = (isObject(BoardConfig) && BoardConfig.playerPlacementMaxY !$= "") ? BoardConfig.playerPlacementMaxY : 3;

   for (%ux = 0; %ux < %width; %ux++)
   {
      for (%uy = %minY; %uy <= %maxY; %uy++)
      {
         if ($board.obstacles[%ux, %uy] == 0 && isObject($green[%ux, %uy]))
            $green[%ux, %uy].setFrame("0");
      }
   }

   if (isObject(HeroGroup))
   {
      %hero = HeroGroup.getObject(0);
      if (isObject(%hero))
         %hero.setSelectedTile("-1 -1");
   }
}

function sceneWindow2D::onMouseDown( %this, %mod, %worldPos, %mouseClicks )
{
        if (getGameState() == $gameIntroState) 
        {
            skipIntro();
            return;
        }

        if (getGameState() == $gameMainMenuState) 
        {
            if ($introSkipTime !$= "" && (getSimTime() - $introSkipTime) < 350)
            {
               return;
            }
            gotoSelectHeroScreen();
            return;
        }
        
        
        
                $hiiri.nappi = true;    

        if($debugMsg::mouse::callBacks)
                echo("Mouse Down");

        //check to see if we have an object already selected, 
        //if so we don't want to do anything expect set it to false so it doesn't follow anymore
        if(%this.objectSelected)
        {
                if (getGameState() == $gamePreBattleState) 
                {
                   mouseOverGreenGrid(%mod, %worldPos);
                   %obj = $mouseObj::objMounted;
                   %this.placeSelectedCharacter(%obj);
                }
        } 
        else
        {
                //lets get a list of all the objects at the clicked point in the t2dScene
                %objList = t2dScene.pickPoint(%worldPos);
                //lets get a count of how many objects in the list
                %objCount = getWordCount(%objList);
        
                //we will start looping through the list
                for(%i=0;%i<%objCount;%i++)
                {
                        //grabing the entry corresponding to the loop
                        %obj = getWord(%objList, %i);
                
                        //if we find an object in the list that "isSelectable = true"
        
                        if(%obj.isSelectable)
                        {
                                //we toggle a value so we know we found an object that "isSelectable"
                                %selected = true;
                                //we kick out of the loop
                                %i = %objCount;

                        } else {
                        }
                        
                }
        
                //if we found an "isSelectable" object
                if(%selected)
                {
                        //we then store that object as the selectedObj
                        %this.selectedObj = %obj;
                
                        if($debugMsg::mouse::selection)
                                echo("You have selected:" SPC %obj SPC "with the name:" SPC %obj.objectName);

                        %this.objectSelected = true;
                        %this.mouseDownPos = %worldPos;
                        %this.dragStarted = false;

                        if (isObject($mouseObj::object))
                           $mouseObj::object.setPosition(%worldPos);

                        if($debugMsg::mouse::selection) 
                               echo("echo attempting to mount:" SPC %obj SPC " to mouse obj:" 
                                         SPC $mouseObj::object);
                             
                        %obj.mount($mouseObj::object, $mouseObj::mount::offset, 
                                             $mouseObj::mount::force, $mouseObj::mount::trackRotation);
                                             
                        if (getGameState() == $gamePreBattleState || getGameState() == $gamePrebattleState) 
                        {
                           iconSendToFront(%obj.index);
                        }
                                                
                           $mouseObj::isObjMounted = true;
                           $mouseObj::objMounted = %obj;
                        
                           if (getGameState() == $gameCardSelectState) 
                           {
                              card_0.valittu = false;
                              card_1.valittu = false;
                              %obj.valittu = true;
                              checkSelectedCard();

                           }

                }
        }
        


}
function setNappiReleasedOff() {
   $hiiri.nappireleased = false;
}

function sceneWindow2D::onMouseUp( %this, %mod, %worldPos, %mouseClicks )
{
         if ( getGameState() == $gameIntroState )
         {
            return;
         }

         if ( getGameState() == $gamePreBattleState && %this.objectSelected && %this.dragStarted )
         {
            mouseOverGreenGrid(%mod, %worldPos);
            %this.placeSelectedCharacter($mouseObj::objMounted);
         }

         $hiiri.nappi = false;
         $hiiri.nappireleased = true;
         schedule(100,0,setNappiReleasedOff);

         if($debugMsg::mouse::callBacks) 
         {
                 echo("Mouse Up"); 
         }

         if ( getGameState() == $gameBattleState ) 
         {
            mouseOverMovementGrid(%mod, %worldPos);
            mouseOverActionIcons(%mod, %worldPos);

            if (isObject(HeroGroup) && isObject(game))
            {
               %hero = HeroGroup.getObject(game.getActivePlayer());
               if (isObject(%hero))
               {
                  %v1 = %hero.getSelectedTile();
                  %selectedNappula = game.getNappulaSelected();
                  if (%selectedNappula !$= "" && isObject($nappulaSprite[%selectedNappula]))
                     %v2 = $nappulaSprite[%selectedNappula].positionOnBoard;
                  else
                     %v2 = "";

                  if (game.getHighlightedActionIcon() > 0) 
                  {
                     doAction();
                  }

                  if (%hero.getSelectedTileX() >= 0) 
                  { 
                     if (%v1 !$= "" && %v2 !$= "" && t2dVectorCompare(%v1, %v2) == 0)
                     {                                       
                        doMovement( %hero.getSelectedTileX(), %hero.getSelectedTileY() );
                     }
                  }
               }
            }
         }
}

function sceneWindow2D::onMouseEnter( %this, %mod, %worldPos, %mouseClicks )
{
        if($debugMsg::mouse::callBacks)
                echo("Mouse Entered... Knew it would come crawling back");
                
                       
}

function sceneWindow2D::onMouseLeave( %this, %mod, %worldPos, %mouseClicks )
{

            
        if($debugMsg::mouse::callBacks)
                echo("Mouse Left... Go Get It!");
                
                     
}

function initMouse()
{
        $mouseObj::object = new SceneObject() { scene = t2dScene; };
        if (isObject(t2dScene)) t2dScene.add($mouseObj::object);
        $mouseObj::object.setSize("1 1");
        $mouseObj::object.setLayer(31);
        $mouseObj::follow = true;
        $mouseObj::speed::max = 250;
        $mouseObj::speed::default = 250;
        $mouseObj::mount::force = 0;
        $mouseObj::mount::offset = "3.5 -3.5";
        $mouseObj::mount::trackRotation = true;
        
        sceneWindow2D.objectSelected = false;
        sceneWindow2D.dragStarted = false;
        $mouseObj::isObjMounted = false;
        $mouseObj::objMounted = "";

        if (!isObject($hiiri))
           $hiiri = new ScriptObject();
        $hiiri.nappi = false;
        $hiiri.nappireleased = false;
        $hiiri.piilota = false;
        Canvas.setCursor(DefaultCursor);
        Canvas.showCursor();
        
        //$debugMsg::mouse::selection=true;
        //$debugMsg::mouse::callbacks=true;

}

function t2dScene::onUpdateScene(%this)
{
        if(($mouseObj::follow) && (!$mouseObj::stopped))
                mouseObjCheck();        
}

function mouseObjCheck()
{

        // Get the position of the Mouse itself
        %x = getWord($mouseObj::pos, 0);
        %y = getWord($mouseObj::pos, 1);
                
        // Get the Position of the mounted scene object
        %objPos = $mouseObj::object.getPosition();
        %objX = getWord(%objPos, 0);
        %objY = getWord(%objPos, 1);

        // calculate the distance between the two (for speed calculations)
        %dist = distBetween(%objPos, $mouseObj::pos);
        
        // Set values for easy configuration
        // set speed = distance * 20, that way it slows when approach - no bobbing around
        %speed = %dist * 20;

        // limit speed if we want to
        if(%speed > $mouseObj::speed::max)
                %speed = $mouseObj::speed::default;

        %space = 0.25;
        
        if((%objX > (%x - %space)) && (%objX < (%x + %space)) && 
            (%objY > (%y - %space)) && (%objY < (%y + %space)))
        {
                if($debugMsg::mouse::object)
                        echo("object at mouse position, halting");


                $mouseObj::object.setLinearVelocityPolar(0,0);
                $mouseObj::stopped = true; 
        } else
        {
                if($debugMsg::mouse::object)
                        echo("object seeking mouse at " @ angleBetween(%objPos, $mouseObj::pos));
                                
                //set the polar velocity towards the mouse      
                $mouseObj::object.setLinearVelocityPolar(angleBetween(%objPos, $mouseObj::pos),%speed); 
        }        

}

function angleBetween(%playerPos, %mousePos)
{
        // Seperate Mouse Position
        %mxpos = getWord(%mousePos,0);
        %mypos = getWord(%mousePos,1);

        // Seperate Player Position
        %px = getWord(%playerPos,0);
        %py = getWord(%playerPos,1);

        // Calculate Angle from player to mouse (convert to degrees).
        %angle = mRadToDeg( mAtan( %mxpos-%px, %py-%mypos ) );

        return %angle;
}

function distBetween(%loc1, %loc2)
{       
        %x1 = getWord(%loc1, 0);
        %y1 = getWord(%loc1, 1);

        %x2 = getWord(%loc2, 0);
        %y2 = getWord(%loc2, 1);

        %xd = %x2 - %x1;
        %yd = %y2 - %y1;
        
        return mSqrt((mPow(%xd,2)) + (mPow(%yd,2)));
}

