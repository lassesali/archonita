// boat Datablock definition
// play with these variables to change the handling of your boat!
datablock t2dSceneObjectDatablock(PhysBoatDatablock)
{
   class = "PhysBoat"; // associate this datablock with the PhysBoat namespace
   maxSteerAngle = "2.5"; // the maximum angular force used for steering the boat (not an angle)
   cTrac = "0.8"; // coefficient of traction (max acceleration)
   cRes = "0.025"; // coefficient of water resistance
   cRotRes = "0.04"; // coefficient of rotational resistance
   size = "2.830 10.200"; // default size
};


// PhysBoat onLevelLoaded callback
function PhysBoat::onLevelLoaded(%this, %scenegraph)
{   
   // set this boat's name so SceneWindow2D can find it
   %this.setName("Myboat");
   
   // init boat toggle vars
   %this.gas = false;
   %this.brake = false;
   %this.left = false;
   %this.right = false;
   
   // init boat input vars
   %this.throttle = 0; // ranges from -1 to 1 (full reverse to full forward)
   %this.steering = 0; // ranges from -1 to 1 (full left to full right)
   
   // map keys
   moveMap.bindCmd(keyboard, "w", %this @ ".throttleOn();", %this @ ".throttleOff();");
   moveMap.bindCmd(keyboard, "s", %this @ ".brakeOn();", %this @ ".brakeOff();");
   moveMap.bindCmd(keyboard, "a", %this @ ".leftOn();", %this @ ".leftOff();");
   moveMap.bindCmd(keyboard, "d", %this @ ".rightOn();", %this @ ".rightOff();");
   
   // enable the timer callback for this object for every 25 milliseconds
   %this.setTimerOn(25);
}


// PhysBoat onTimer callback
function PhysBoat::onTimer(%this)
{
   // 1. interpret input flags
   // Throttle:
   // if only the gas is pressed and we aren't reversing
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
   // either *neother* or *both* gas petal and throttle are activated
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
      %this.steering -= 0.075;
      
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
      %this.steering += 0.075;
      
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
      if(%this.steering < -0.2)
      {
         %this.steering += 0.2;
      }
      else if(%this.steering > 0.2)
      {
         %this.steering -= 0.2;
      }
      else
      {
         %this.steering = 0;
      }
   }
   
   // 2. calculate longitudinal force
   // get current forward traction force (acceleration)
   %traction = %this.cTrac * %this.throttle;
   
   // get longitudinal force vector
   // remember that neg Y is up, so we need -cos(rotation) for Y
   %longForceX = mSin(mDegToRad(%this.getRotation() + (%this.steering * %this.maxSteerAngle / 25))) * %traction;
   %longForceY = -mCos(mDegToRad(%this.getRotation() + (%this.steering * %this.maxSteerAngle / 25))) * %traction;
   
   // add new force to existing velocity vector
   %velocity = t2dVectorAdd(%longForceX SPC %longForceY, %this.getLinearVelocity());
   
   // get the current speed
   %currSpeed = t2dVectorLength(%velocity);
   
   // subtract resistance from current speed
   %currSpeed -= %this.cRes * %currSpeed;
   
   // scale the velocity vector based on our new speed
   %velocity = t2dVectorScale(t2dVectorNormalise(%velocity), %currSpeed);
   
   // 3. steering / angular vel
   // our angular velocity from steering will increase at a rate of 
   // (steering * speed^2) and max out at maxSteerAngle
   // get current speed
   %currSpeed = t2dVectorLength(%velocity);
   
   // adjust steering force:
   // get a base steering force based on speed of movement
   %steerVel = %this.steering * (%currSpeed * %currSpeed);
   
   // the direction of rotation should be opposite if we are in revere
   if(%this.throttle < 0)
   {
      %steerVel = -%steerVel;
   }
   
   // clamp angular velocity
   if(%steerVel > %this.maxSteerAngle)
   {
      %steerVel = %this.maxSteerAngle;
   }
   else if(%steerVel < -%this.maxSteerAngle)
   {
      %steerVel = -%this.maxSteerAngle;
   }
   
   // add steerVel to current angular velocity
   %angVel = %steerVel + %this.getAngularVelocity();
   
   // dampen angular velocity
   %angVel -=  %this.cRotRes * %angVel;
   
   // 4. apply forces
   // apply linear velocity
   %this.setLinearVelocity(%velocity);
   
   // apply angular velocity
   %this.setAngularVelocity(%angVel);
}


// ____ input functions ____
// gas
function PhysBoat::throttleOn(%this)
{
   %this.gasPedal = true;
}
function PhysBoat::throttleOff(%this)
{
   %this.gasPedal = false;
}

// brake / reverse
function PhysBoat::brakeOn(%this)
{
   %this.brake = true;
}
function PhysBoat::brakeOff(%this)
{
   %this.brake = false;
}

// steer left
function PhysBoat::leftOn(%this)
{
   %this.left = true;
}
function PhysBoat::leftOff(%this)
{
   %this.left = false;
}

// steer right
function PhysBoat::rightOn(%this)
{
   %this.right = true;
}
function PhysBoat::rightOff(%this)
{
   %this.right = false;
}