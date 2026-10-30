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
      %sankari = HeroGroup.getObject(game.getVuorossaOlevaPelaaja());

                                %sankari.setValittuRuutu("-1 -1");
   

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
                     
                        
                        if(%obj.isValintakehys)
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
                        %sankari.setValittuRuutu(%obj.positionOnBoard);

                     
                }
        
}


function mouseOverGreenGrid( %mod, %worldPos )
{
                                 %sankari = HeroGroup.getObject(0);
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
        


                                %sankari.setValittuRuutu("-1 -1");
                                for (%ux=0; %ux<4; %ux++) 
                                {
                                    for (%uy=2; %uy<4; %uy++) 
                                    {
                                       if ($board.obstacles[%ux,%uy]==0) 
                                       {
                                          if(isObject($green[%ux,%uy])) {
                                             $green[%ux,%uy].setFrame("0");
                                          }
                                       }

                                    }
                                }
                                
                                for (%j=0; %j<3; %j++) 
                                {
                                  $setupNappulaSprite[%j].setVisible(false);
                                }

                        if(%obj.isValintakehys)
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
                        %sankari.setValittuRuutu(%obj.positionOnBoard);
                     
                     if($mouseObj::isObjMounted) {
                        %objMounted=$mouseObj::objMounted;
                        $setupNappulaSprite[%objMounted.creatureLevel - 1].setPosition(%sankari.getValittuRuutuX()*8-16 SPC %sankari.getValittuRuutuY()*8-18 );
                        $setupNappulaSprite[%objMounted.creatureLevel - 1].setVisible(true);
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

         if (isObject(mouseTrail)) {
            
            mouseTrail.setPosition(%worldPos);
         }

         if (getGameState() == $gamePreBattleState) 
         {
            mouseOverGreenGrid(%mod, %worldPos);
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


function sceneWindow2D::onMouseDown( %this, %mod, %worldPos, %mouseClicks )
{
        
        if (getGameState() == $gameMainMenuState) 
        {
            gotoSelectHeroScreen();
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
                  %obj = $mouseObj::objMounted;
                
                  if($debugMsg::mouse::selection)
                        echo("unselecting");

                  if($mouseObj::isObjMounted)
                  {       
                        if(isObject(%obj))
                        {
                                                            %sankari = HeroGroup.getObject(0);
                           if(%sankari.getValittuRuutu != "-1 -1") 
                           {
                        
                        
                                if($debugMsg::mouse::selection)
                                        echo("dismounting object");

                                %obj.dismount();
                                $mouseObj::isObjMounted = false;
                                
                                if (isObject($numb[%obj.index]))
                                    $numb[%obj.index].delete();

                                lisaaUusiNappulaLaudalle(%obj.creatureLevel, %sankari.getValittuRuutuX(), %sankari.getValittuRuutuY(), 0);
                                updatePisteetGridsIcons();
                                 
                                if (isObject(%obj)) 
                                    %obj.delete();
                                
                                dismountobject();
                                %this.objectSelected = false;
                           }
                        
                        
                        }
                  }
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

                           if($debugMsg::mouse::selection) 
                                  echo("echo attempting to mount:" SPC %obj SPC " to mouse obj:" 
                                            SPC $mouseObj::object);
                                
                           %obj.mount($mouseObj::object, $mouseObj::mount::offset, 
                                                $mouseObj::mount::force, $mouseObj::mount::trackRotation);
                                                
                           if (getGameState() == $gamePrebattleState) 
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
                              tarkastavalittukortti();

                           }

                }
        }
        


}
function setNappiReleasedOff() {
   $hiiri.nappireleased = false;
}

function sceneWindow2D::onMouseUp( %this, %mod, %worldPos, %mouseClicks )
{
                                    %sankari = HeroGroup.getObject(game.getVuorossaOlevaPelaaja());
            %v1=%sankari.getValittuRuutu();
            %v2=$nappulaSprite[game.getNappulaSelected()].positionOnBoard;
           $hiiri.nappi = false;
           $hiiri.nappireleased = true;
           schedule(100,0,setNappiReleasedOff);

        if($debugMsg::mouse::callBacks) 
        {
                echo("Mouse Up"); 
        }
      
                           if ( getGameState() == $gameBattleState && game.getHighlightedActionIcon() > 0 ) 
                           {
                              doAction();
                           }
                           
         if ( getGameState() == $gameBattleState && %sankari.getValittuRuutuX() >= 0) { 
                              
           if ( t2dVectorCompare(%v1, %v2) == 0 )
           {                                       
                           doMovement( %sankari.getValittuRuutuX(), %sankari.getValittuRuutuY() );
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
        $mouseObj::object = new t2dSceneObject() { scenegraph = t2dScene; };
        $mouseObj::object.setSize("1 1");
        $mouseObj::object.setLayer(31);
        $mouseObj::follow = true;
        $mouseObj::speed::max = 250;
        $mouseObj::speed::default = 250;
        $mouseObj::mount::force = 0;
        $mouseObj::mount::offset = "7 7";
        $mouseObj::mount::trackRotation = true;
        
        $hiiri = new ScriptObject();
        $hiiri.nappi = false;
        $hiiri.nappireleased = false;
        
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

