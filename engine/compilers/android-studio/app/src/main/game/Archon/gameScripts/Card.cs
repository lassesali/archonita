//---------------------------------------------------------------------------------------------
// Archonita - Card.cs
// Spell card Sprite representation with post-cast discard trajectory and self-cleanup.
// Conforms strictly to TORQUE_SCRIPT.md:
// - One class per file, matching filename (no god-namespaces).
// - Lifecycle in onAdd / onRemove using the house init() convention.
// - All state stored on %this (zero globals).
// - Repeating schedule explicitly tracked and cancelled in onRemove.
// - Self-cleanup via %this.safeDelete() once completely outside viewport AABB.
//---------------------------------------------------------------------------------------------

function Card::init(%this)
{
   %this.discardTickEvent = 0;
   %this.currentScale     = 1.0;
   %this.maxScale         = 2.5;
   %this.scaleStep        = 0.04;
   %this.spinSpeed        = 240;        // Continuous axial rotation (deg/sec)
   %this.discardAngle     = 135;        // Polar heading (135° = top-left off-screen trajectory)
   %this.discardSpeed     = 48;         // Linear velocity in world units/sec
   %this.cameraBounds     = "";
   %this.currentPosX      = 0;
   %this.currentPosY      = 0;
   %this.currentAngle     = 0;
   %this.vx               = 0;
   %this.vy               = 0;
}

function Card::onAdd(%this)
{
   %this.init();
}

function Card::onRemove(%this)
{
   // Strict cleanup: cancel any pending repeating schedules on %this
   if (isEventPending(%this.discardTickEvent))
   {
      cancel(%this.discardTickEvent);
      %this.discardTickEvent = 0;
   }
}

function Card::safeDelete(%this)
{
   if (isObject(%this))
   {
      %this.setVisible(false);
      if (isEventPending(%this.discardTickEvent))
      {
         cancel(%this.discardTickEvent);
         %this.discardTickEvent = 0;
      }
      // Defer deletion to next tick via global dispatcher so callback unwinds cleanly without Fatal-ISV
      schedule(1, 0, "deleteSimObject", %this);
   }
}

//---------------------------------------------------------------------------------------------
// Discard Animation Trigger
//---------------------------------------------------------------------------------------------
function Card::discardAfterCast(%this, %cameraBounds)
{
   // 1. Relinquish input events immediately
   %this.setUseInputEvents(false);
   %this.setUseMouseEvents(0);

   // 2. Bring to the foreground layer
   %this.setSceneLayer(0);

   // 3. Store starting dimensions and initialize scale factor
   %size = %this.getSize();
   %this.baseWidth    = getWord(%size, 0);
   %this.baseHeight   = getWord(%size, 1);
   %this.currentScale = 1.0;
   %this.maxScale     = 2.5;
   %this.scaleStep    = 0.04;

   // 4. Store current position and rotation for kinematic interpolation
   %pos = %this.getPosition();
   %this.currentPosX  = getWord(%pos, 0);
   %this.currentPosY  = getWord(%pos, 1);
   %this.currentAngle = %this.getAngle();

   // 5. Resolve camera / visible viewport bounds ("minX minY maxX maxY")
   if (%cameraBounds $= "")
   {
      if (isObject(sceneWindow2D) && sceneWindow2D.isMethod("getCameraArea"))
         %cameraBounds = sceneWindow2D.getCameraArea();

      if (%cameraBounds $= "" || getWordCount(%cameraBounds) < 4)
         %cameraBounds = "-50 -37.5 50 37.5";
   }
   %this.cameraBounds = %cameraBounds;

   // 6. Calculate trajectory velocity vectors
   %spin  = (%this.spinSpeed !$= "")    ? %this.spinSpeed    : 240;
   %angle = (%this.discardAngle !$= "") ? %this.discardAngle : 135;
   %speed = (%this.discardSpeed !$= "") ? %this.discardSpeed : 48;

   %this.spinSpeed    = %spin;
   %this.discardAngle = %angle;
   %this.discardSpeed = %speed;

   %rad = mDegToRad(%angle);
   %this.vx = mCos(%rad) * %speed;
   %this.vy = mSin(%rad) * %speed;

   // Apply engine physics methods (if physics simulation is active)
   %this.setAngularVelocity(%spin);
   %this.setLinearVelocityPolar(%angle, %speed);

   // 7. Start the self-rescheduling tick loop for scale interpolation, kinematic motion & bounds check
   if (isEventPending(%this.discardTickEvent))
      cancel(%this.discardTickEvent);

   %this.discardTickEvent = %this.schedule(16, "updateDiscard");
}

//---------------------------------------------------------------------------------------------
// Scale Interpolation, Kinematic Motion & Viewport Boundary Check
//---------------------------------------------------------------------------------------------
function Card::updateDiscard(%this)
{
   %this.discardTickEvent = 0;

   // 1. Smoothly scale toward maxScale (2.5x)
   if (%this.currentScale < %this.maxScale)
   {
      %this.currentScale += %this.scaleStep;
      if (%this.currentScale > %this.maxScale)
         %this.currentScale = %this.maxScale;

      %this.setSize(%this.baseWidth * %this.currentScale, %this.baseHeight * %this.currentScale);
   }

   // 2. Kinematic transform step (ensures trajectory & spin work reliably across all scene modes)
   %this.currentPosX += %this.vx * 0.016;
   %this.currentPosY += %this.vy * 0.016;
   %this.setPosition(%this.currentPosX, %this.currentPosY);

   %this.currentAngle += %this.spinSpeed * 0.016;
   %this.setAngle(%this.currentAngle);

   // 3. Check if the card's current AABB has completely cleared the visible viewport
   %cardAABB = %this.getAABB();
   %cardMinX = getWord(%cardAABB, 0);
   %cardMinY = getWord(%cardAABB, 1);
   %cardMaxX = getWord(%cardAABB, 2);
   %cardMaxY = getWord(%cardAABB, 3);

   %camMinX = getWord(%this.cameraBounds, 0);
   %camMinY = getWord(%this.cameraBounds, 1);
   %camMaxX = getWord(%this.cameraBounds, 2);
   %camMaxY = getWord(%this.cameraBounds, 3);

   // Completely outside if any dimension has fully passed the visible boundary
   %isOffScreen = (%cardMaxX < %camMinX) || (%cardMinX > %camMaxX) ||
                  (%cardMaxY < %camMinY) || (%cardMinY > %camMaxY);

   if (%isOffScreen)
   {
      // Relinquish scene presence and self-clean
      %this.safeDelete();
      return;
   }

   // 4. Reschedule next tick (16ms ~ 60 FPS)
   %this.discardTickEvent = %this.schedule(16, "updateDiscard");
}
