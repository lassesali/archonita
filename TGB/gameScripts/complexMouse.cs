function sceneWindow2D::onMouseMove( %this, %mod, %worldPos, %mouseClicks )
{
        if($debugMsg::mouse::callBacks)
                echo("Mouse Moving");

        //store the mouse position
        $mouseObj::pos = %worldPos;

        //reset stopped to false so it checks
        $mouseObj::stopped = false;
}

function sceneWindow2D::onMouseDragged( %this, %mod, %worldPos, %mouseClicks )
{
        if($debugMsg::mouse::callBacks)
                echo("Mouse Dragging");

        //store the mouse position
        $mouseObj::pos = %worldPos;

        //reset stopped to false so it checks
        $mouseObj::stopped = false;
}

function sceneWindow2D::onMouseDown( %this, %mod, %worldPos, %mouseClicks )
{
        if($debugMsg::mouse::callBacks)
                echo("Mouse Down");

        //check to see if we have an object already selected, 
        //if so we don't want to do anything expect set it to false so it doesn't follow anymore
        if(%this.objectSelected)
        {
                if($debugMsg::mouse::selection)
                        echo("unselecting");

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

                %this.objectSelected = false;
        } else
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
                        $mouseObj::isObjMounted = true;
                        $mouseObj::objMounted = %obj;

                }
        }
}

function sceneWindow2D::onMouseUp( %this, %mod, %worldPos, %mouseClicks )
{
        if($debugMsg::mouse::callBacks)
                echo("Mouse Up");
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
        $mouseObj::mount::offset = "0 0";
        $mouseObj::mount::trackRotation = true;
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

function complexMouse()
{
        $test = new t2dStaticSprite() { sceneGraph = t2dScene; };
        $test.setImageMap( diceredImageMap );
        $test.setSize( "16 16" );
        $test.setPosition( "0 0" );
        $test.isSelectable = true;
        $test.objectName = "test box";
}



/*function SceneWindow2D::mouseMoveGFXCursor(%this, %mod, %worldPosition)
{
   %movement = t2dvectorsub(%worldPosition, $lastMousePos);  
   mousePointer.setImpulseForce(%movement);
   %this.setMousePosition(mousePointer.getPosition());
   Canvas.hideCursor();
   $lastMousePos = mousePointer.getPosition();
}
*/
/*
function hiirirajat(%WorldPosition) {
   %mxpos = getWord(%WorldPosition, 0);
   %mypos = getWord(%WorldPosition, 1);
   if (%mxpos < -49 || %mxpos > 49 || %mypos < -36 || %mypos > 36) {
      mousePointer.setVisible(false);
         
   } else {
      mousePointer.setVisible(true);
   }
}
*/

/*

function sceneWindow2D::onMouseMove( %this, %mod, %worldPos, %mouseClicks )
{

   //%this.mouseMoveGFXCursor(%mod, %worldPos);        
         
        if($debugMsg::mouse::callBacks)
                echo("Mouse Moving");

        //store the mouse position
        $mouseObj::pos = %worldPos;

        //reset stopped to false so it checks
        $mouseObj::stopped = false;
         if ($hiiri.piilota) 
            Canvas.hideCursor();
         //mousePointer.setPosition(%worldPos);
         //mouseTrail.setPosition(%worldPos);
         //hiirirajat(%worldPos);
}

function sceneWindow2D::onMouseDragged( %this, %mod, %worldPos, %mouseClicks )
{

   //%this.mouseMoveGFXCursor(%mod, %worldPos);

        if($debugMsg::mouse::callBacks)
                echo("Mouse Dragging");



        //store the mouse position
        $mouseObj::pos = %worldPos;

        //reset stopped to false so it checks
        $mouseObj::stopped = false;
         if ($hiiri.piilota) 
            Canvas.hideCursor();
                 //mousePointer.setPosition(%worldPos);
                 //                    hiirirajat(%worldPos);
}

function sceneWindow2D::onMouseDown( %this, %mod, %worldPos, %mouseClicks )
{
                $hiiri.nappi = true;    

        if($debugMsg::mouse::callBacks)
                echo("Mouse Down");

        //check to see if we have an object already selected, 
        //if so we don't want to do anything expect set it to false so it doesn't follow anymore
        if(%this.objectSelected)
        {
                if($debugMsg::mouse::selection)
                        echo("unselecting");

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

                %this.objectSelected = false;
        } else
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
                        $mouseObj::isObjMounted = true;
                        $mouseObj::objMounted = %obj;

                }
        }
}

function sceneWindow2D::onMouseUp( %this, %mod, %worldPos, %mouseClicks )
{
           $hiiri.nappi = false;
        if($debugMsg::mouse::callBacks)
                echo("Mouse Up");
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
        $mouseObj::mount::offset = "0 0";
        $mouseObj::mount::trackRotation = true;
        
        $hiiri = new ScriptObject();
        $hiiri.nappi = false;

                        //lockMouse(true);
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

function complexMouse()
{

}

*/
