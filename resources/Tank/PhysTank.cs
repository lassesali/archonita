// Tank Datablock definition
// play with these variables to change the handling of your Tank!
datablock t2dSceneObjectDatablock(PhysTankDatablock)
{
   class = "PhysTank"; // associate this datablock with the PhysTank namespace
   maxSteerAngle = "55"; // the maximum angular force used for steering the tank (not an angle)
   cTrac = "15"; // coefficient of traction (max speed)
   size = "8.120 8.120"; // default size
};

// Turret Datablock definition
datablock t2dSceneObjectDatablock(PhysTankTurretDatablock)
{
   turnSpeed = "80"; // speed at which turret turns
   size = "2.755 11.132"; // default size
};

// PhysTank onLevelLoaded callback
function PhysTank::onLevelLoaded(%this, %scenegraph)
{   
   // set this tank's name so SceneWindow2D can find it
   %this.setName("MyTank");
   
   // init tank toggle vars
   %this.gasPedal = false;
   %this.brake = false;
   %this.left = false;
   %this.right = false;
   %this.turretLeft = false;
   %this.turretRight = false;
   
   // init tank input vars
   %this.throttle = 0; // ranges from -1 to 1 (full reverse to full forward)
   %this.steering = 0; // ranges from -1 to 1 (full left to full right)
   %this.lastMousePos = "0 0";
      
   // init tank physics vars
   %this.latSpeed = 0;
   
   // set up turret
   %this.initializeTurret();
   
   // map keys
   moveMap.bindCmd(keyboard, "w", %this @ ".throttleOn();", %this @ ".throttleOff();");
   moveMap.bindCmd(keyboard, "s", %this @ ".brakeOn();", %this @ ".brakeOff();");
   moveMap.bindCmd(keyboard, "a", %this @ ".leftOn();", %this @ ".leftOff();");
   moveMap.bindCmd(keyboard, "d", %this @ ".rightOn();", %this @ ".rightOff();");
   
   // enable the timer callback for this object for every 25 milliseconds
   %this.setTimerOn(25);
}

// create and attach a turret to this tank
function PhysTank::initializeTurret(%this)
{
   // create the new turret and store it on this object
   %this.turret = new t2dStaticSprite()
   {
      sceneGraph = %this.scenegraph; // make sure to set the scenegraph
      ImageMap = "tankTurretImageMap"; // set the image to use
      config = "PhysTankTurretDatablock"; // set the config datablock
   };

   // mount the turret to the tank and set 'trackRotation' (fourth argument)
   // to false so we can rotate our turret independently
   %this.turret.mount(%this, "0 -0.1", 0, false, true, true, true);
}

// this is called every time the mouse cursor is moved
function sceneWindow2D::onMouseMove(%this, %modifier, %worldPos, %clicks)
{
   // update MyTank's lastMousePos variable with fresh coordinates
   MyTank.lastMousePos = %worldPos;
}

// this is our update turret function to rotate the turret 
// towards the mouse pointer
function PhysTank::updateTurret(%this, %mousePos)
{
   // get a vector from the tank to the mouse pointer
   %vec = t2dVectorSub(%this.getPosition(), %mousePos);
   
   // get the desired angle required to point at the mouse pointer   
   %angle = -mRadToDeg(mATan(getWord(%vec,0),getWord(%vec,1)));
   
   // tell the tank turret to rotate towards that angle at a rate of 
   // 'turret.turnSpeed' which was set in our turret's datablock
   %this.turret.rotateTo(%angle, %this.turret.turnSpeed);
}

// PhysTank onTimer callback
function PhysTank::onTimer(%this)
{
   // 1. interpret input flags
   // Throttle:
   // if only the gas pedal is pressed and we aren't reversing
   if((%this.gasPedal && !%this.brake) && %this.throttle >=0)
   {
      // increment trottle (forward)
      %this.throttle += 0.005;
      
      // clamp throttle at 1 for full forward
      if(%this.throttle > 1)
      {
         %this.throttle = 1;
      }
   }
   // if only the 'brake' is pressed and we aren't going forward
   else if((%this.brake && !%this.gasPedal) && %this.throttle <= 0)
   {
      // decrement throttle (reverse)
      %this.throttle -= 0.005;
      
      // clamp throttle at -1 for full reverse
      if(%this.throttle < -1)
      {
         %this.throttle = -1;
      }
   }
   // either *neither* or *both* gas petal and throttle are activated
   // -or- we are trying to throttle in the opposite direction we are moving
   else
   {
      // lower the throttle from either direction by 0.02
      // if magnitude of throttle is less than 0.02 then set to 0
      // Note: without checking for this, our throttle could skip between
      // negative and positive as it tries to hit zero. 
      // for example, immagine the throttle is 0.1
      if(%this.throttle < -0.02)
      {
         %this.throttle += 0.02;
      }
      else if(%this.throttle > 0.02)
      {
         %this.throttle -= 0.02;
      }
      else
      {
         %this.throttle = 0;
      }
   }
   
   // Steering:
   // if only pressing left and steering isnt to the right
   if((%this.left && !%this.right) && %this.steering <= 0)
   {
      // decrement steering (left)
      %this.steering -= 0.025;
      
      // clamp steering at -1 (full left)
      if(%this.steering < -1)
      {
         %this.steering = -1;
      }
   }
   // if only pressing right and steering isnt to the left
   else if((%this.right && !%this.left) && %this.steering >= 0)
   {
      // increment steering (right)
      %this.steering += 0.025;
      
      // clamp stering at 1 (full right)
      if(%this.steering > 1)
      {
         %this.steering = 1;
      }
   }
   else
   {
      // lower the steering from either direction by 0.02
      // if magnitude of steering is less than 0.02 then set to 0
      if(%this.steering < -0.05)
      {
         %this.steering += 0.05;
      }
      else if(%this.steering > 0.05)
      {
         %this.steering -= 0.05;
      }
      else
      {
         %this.steering = 0;
      }
   }
   
   // 2. calculate longitudinal force
   // get current forward traction force (speed)
   %traction = %this.cTrac * %this.throttle;
   
   // get longitudinal force vector
   // remember that neg Y is up, so we need -cos(rotation) for Y
   %longVelX = mSin(mDegToRad(%this.getRotation())) * %traction;
   %longVelY = -mCos(mDegToRad(%this.getRotation())) * %traction;
   
   // 3. steering / angular vel   
   // adjust steering force:
   // get a base steering force based on speed of movement
   %steerVel = %this.steering * %this.maxSteerAngle;
   
   // 4. apply forces
   // apply linear velocity
   %this.setLinearVelocity(%longVelX SPC %longVelY);
   
   // apply angular velocity
   %this.setAngularVelocity(%steerVel);
   
   // 5. set turret's rotation!
   %this.updateTurret(%this.lastMousePos);
}


// ____ input functions ____
// gas
function PhysTank::throttleOn(%this)
{
   %this.gasPedal = true;
}
function PhysTank::throttleOff(%this)
{
   %this.gasPedal = false;
}

// brake / reverse
function PhysTank::brakeOn(%this)
{
   %this.brake = true;
}
function PhysTank::brakeOff(%this)
{
   %this.brake = false;
}

// steer left
function PhysTank::leftOn(%this)
{
   %this.left = true;
}
function PhysTank::leftOff(%this)
{
   %this.left = false;
}

// steer right
function PhysTank::rightOn(%this)
{
   %this.right = true;
}
function PhysTank::rightOff(%this)
{
   %this.right = false;
}

// rotate turret left
function PhysTank::turretLeftOn(%this)
{
   %this.turretLeft = true;
}
function PhysTank::turretLeftOff(%this)
{
   %this.turretLeft = false;
}

// rotate turret right
function PhysTank::turretRightOn(%this)
{
   %this.turretRight = true;
}
function PhysTank::turretRightOff(%this)
{
   %this.turretRight = false;
}