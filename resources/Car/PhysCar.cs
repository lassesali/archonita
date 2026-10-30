// PhysCar Datablock definition
// play with these variables to change the handling of your car!
datablock t2dSceneObjectDatablock(PhysCarDatablock)
{
   class = "PhysCar"; // associate this datablock with the PhysCar namespace
   maxSteerAngle = "180"; // the maximum angular force used for steering the car (not an angle)
   cTrac = "25"; // coefficient of traction (max speed)
   maxDriftVel = "20"; // maximum speed of drift
   cMass = "0.015"; // mass used in drift calculations
   latDamp = "0.05"; // rate lateral drift fades
   cSlip = "18"; // coefficient of rotational slip
   size = "1.981 4.643";
};

datablock t2dSceneObjectDatablock(TrackDatablock)
{
   size = "75 75";
};

// PhysCar onLevelLoaded callback
function PhysCar::onLevelLoaded(%this, %scenegraph)
{   
   // init car toggle vars
   %this.gasPedal = false;
   %this.brake = false;
   %this.left = false;
   %this.right = false;
   
   // init car input vars
   %this.throttle = 0; // ranges from -1 to 1 (full reverse to full forward)
   %this.steering = 0; // ranges from -1 to 1 (full left to full right)
   
   // init car physics vars
   %this.latSpeed = 0;
   
   // map keys
   moveMap.bindCmd(keyboard, "up", %this @ ".throttleOn();", %this @ ".throttleOff();");
   moveMap.bindCmd(keyboard, "down", %this @ ".brakeOn();", %this @ ".brakeOff();");
   moveMap.bindCmd(keyboard, "left", %this @ ".leftOn();", %this @ ".leftOff();");
   moveMap.bindCmd(keyboard, "right", %this @ ".rightOn();", %this @ ".rightOff();");
   
   // enable the timer callback for this object for every 25 milliseconds
   %this.setTimerOn(25);
}

// PhysCar onTimer callback
function PhysCar::onTimer(%this)
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
   // get current forward traction force (speed)
   %traction = %this.cTrac * %this.throttle;
   
   // get longitudinal force vector
   // remember that neg Y is up, so we need -cos(rotation) for Y
   %longVelX = mSin(mDegToRad(%this.getRotation())) * %traction;
   %longVelY = -mCos(mDegToRad(%this.getRotation())) * %traction;
   
   // 3. steering / angular vel
   // our angular velocity from steering will increase at a rate of 
   // (steering * speed^2) and max out at maxSteerAngle
   // get current speed
   %currSpeed = t2dVectorLength(%longVelX SPC %longVelY);
   
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
 
   // 4. get lateral drift
   // get forward momentum
   %momentum = %currSpeed * %this.cMass;
   
   // increment magnitude of our current lateral (sideways) force vector
   // by (momentum scaled by steering and throttle)
   %this.latSpeed += %momentum * mAbs(%this.steering) * %this.throttle;
   
   // clamp magnitude of drift by maxDriftVel
   if(%this.latSpeed > %this.maxDriftVel)
   {
      %this.latSpeed = %this.maxDriftVel;
   }
   
   // if magnitude of drift is greater than 0.15 
   // (which servers as our drift threshold)
   if(%this.latSpeed > 0.15)
   {
      // dampen drift by latDamp scaled by drift magnitude
      %this.latSpeed -= (%this.latDamp * %this.latSpeed) + 0.05;
   }
   // else zero out drift
   else
   {
      %this.latSpeed = 0;
   }
   
   %drift = %this.latSpeed;
   
   // if we're steering right, switch and drift left
   if(%this.steering > 0)
   {
      %drift = -%drift;
   }
   
   // get lateral drift vector
   %latVelX = mCos(mDegToRad(%this.getRotation())) * %drift;
   %latVelY = mSin(mDegToRad(%this.getRotation())) * %drift;
   
   // get total linear force vector
   %totalForceX = %latVelX + %longVelX;
   %totalForceY = %latVelY + %longVelY;
   
   // 5. get slip
   // get slip ammount based on drift force:
   // if drift magnitude is greater than 2
   if(%this.latSpeed > 2)
   {
      // add a little rotational slip
      %slip = %this.cSlip * %this.latSpeed * %driftDir;
   }
   else
   {
      %slip = 0;
   }
   
   // get total angular velocity
   %angVel = %steerVel + %slip;
   
   // 6. apply forces
   // apply linear velocity
   %this.setLinearVelocity(%totalForceX SPC %totalForceY);
   
   // apply angular velocity
   %this.setAngularVelocity(%angVel);
}


// ____ input functions ____
// gas
function PhysCar::throttleOn(%this)
{
   %this.gasPedal = true;
}
function PhysCar::throttleOff(%this)
{
   %this.gasPedal = false;
}

// brake / reverse
function PhysCar::brakeOn(%this)
{
   %this.brake = true;
}
function PhysCar::brakeOff(%this)
{
   %this.brake = false;
}

// steer left
function PhysCar::leftOn(%this)
{
   %this.left = true;
}
function PhysCar::leftOff(%this)
{
   %this.left = false;
}

// steer right
function PhysCar::rightOn(%this)
{
   %this.right = true;
}
function PhysCar::rightOff(%this)
{
   %this.right = false;
}

