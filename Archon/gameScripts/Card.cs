//---------------------------------------------------------------------------------------------
// Archonita - Card.cs
// Spell card Sprite representation with post-cast discard trajectory and self-cleanup.
// Conforms strictly to TORQUE_SCRIPT.md:
// - One class per file, matching filename.
// - Lifecycle in onAdd / onRemove using house init() convention.
// - All state stored on %this (no globals).
// - Repeating schedule explicitly tracked and cancelled in onRemove.
// - Self-cleanup via %this.safeDelete() once completely outside viewport AABB.
//---------------------------------------------------------------------------------------------

function Card::init(%this)
{
   %this.discardTickEvent = 0;
   %this.currentScale = 1.0;
   %this.maxScale = 2.5;
   %this.scaleStep = 0.04;
   %this.spinSpeed = 240;        // Continuous axial rotation (deg/sec)
   %this.discardAngle = 135;     // Trajectory heading toward top-left outer edge (0 deg = +X, CCW)
   %this.discardSpeed = 48;      // Velocity in world units/sec
   %this.cameraBounds = "";
}

function Card::onAdd(%this)
{
   %this.init();
}

function Card::onRemove(%this)
{
   // Strict cleanup: cancel any pending repeating schedules
   if (isEventPending(%this.discardTickEvent))
   {
      cancel(%this.discardTickEvent);
      %this.discardTickEvent = 0;
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
   %this.baseWidth = getWord(%size, 0);
   %this.baseHeight = getWord(%size, 1);
   %this.currentScale = 1.0;

   // 4. Resolve camera/viewport bounding box (minX minY maxX maxY)
   if (%cameraBounds $= "")
   {
      if (isObject(sceneWindow2D))
         %cameraBounds = sceneWindow2D.getCurrentCameraArea();
      else
         %cameraBounds = "-50 -37.5 50 37.5";
   }
   %this.cameraBounds = %cameraBounds;

   // 5. Apply continuous axial rotation and launch along trajectory
   %this.setAngularVelocity(%this.spinSpeed);
   %this.setLinearVelocityPolar(%this.discardAngle, %this.discardSpeed);

   // 6. Start the self-rescheduling tick loop for scale interpolation & bounds check
   if (isEventPending(%this.discardTickEvent))
      cancel(%this.discardTickEvent);

   %this.discardTickEvent = %this.schedule(16, "updateDiscard");
}

//---------------------------------------------------------------------------------------------
// Scale Interpolation & Viewport Boundary Check
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

   // 2. Check if the card's current AABB has completely cleared the visible viewport
   %cardAABB = %this.getAABB();
   %cardMinX = getWord(%cardAABB, 0);
   %cardMinY = getWord(%cardAABB, 1);
   %cardMaxX = getWord(%cardAABB, 2);
   %cardMaxY = getWord(%cardAABB, 3);

   %camMinX = getWord(%this.cameraBounds, 0);
   %camMinY = getWord(%this.cameraBounds, 1);
   %camMaxX = getWord(%this.cameraBounds, 2);
   %camMaxY = getWord(%this.cameraBounds, 3);

   // Completely outside if any side of the card has passed beyond the viewport edge
   %isOffScreen = (%cardMaxX < %camMinX) || (%cardMinX > %camMaxX) ||
                  (%cardMaxY < %camMinY) || (%cardMinY > %camMaxY);

   if (%isOffScreen)
   {
      // Relinquish scene presence and self-clean
      %this.safeDelete();
      return;
   }

   // 3. Reschedule next tick (16ms ~ 60 FPS)
   %this.discardTickEvent = %this.schedule(16, "updateDiscard");
}
