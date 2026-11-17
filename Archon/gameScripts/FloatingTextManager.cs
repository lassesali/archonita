//---------------------------------------------------------------------------------------------
// Archonita - FloatingTextManager.cs
// Lightweight, asynchronous floating combat text (FCT) manager for visual spell feedback.
// Conforms to TORQUE_SCRIPT.md: ScriptObject subclass, lifecycle in onAdd/onRemove.
//---------------------------------------------------------------------------------------------

function FloatingTextManager::init(%this)
{
   if (!isObject(%this.activeTexts))
      %this.activeTexts = new SimSet();
   %this.updateSchedule = 0;
   %this.tickInterval = 30; // ~33 FPS animation tick
   %this.nextInstanceId = 0;

   // Base transparent container profile
   if (!isObject(FloatingTextContainerProfile))
   {
      new GuiControlProfile(FloatingTextContainerProfile : GuiDefaultProfile)
      {
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }
}

function FloatingTextManager::onAdd(%this)
{
   %this.init();
}

function FloatingTextManager::onRemove(%this)
{
   %this.stopUpdateLoop();

   if (isObject(%this.activeTexts))
   {
      while (%this.activeTexts.getCount() > 0)
      {
         %item = %this.activeTexts.getObject(0);
         %this.cleanupTextInstance(%item);
      }
      %this.activeTexts.delete();
      %this.activeTexts = "";
   }
}

//---------------------------------------------------------------------------------------------
// Coordinate Conversion: World Units -> GUI Viewport Coordinates
//---------------------------------------------------------------------------------------------
function FloatingTextManager::worldToScreenPoint(%this, %wx, %wy)
{
   if (isObject(sceneWindow2D))
   {
      %pt = sceneWindow2D.getWindowPoint(%wx SPC %wy);
      if (%pt !$= "")
         return %pt;

      %ext = sceneWindow2D.getExtent();
      %winW = getWord(%ext, 0);
      %winH = getWord(%ext, 1);
   }
   else
   {
      %winW = 1024;
      %winH = 768;
   }

   if (%winW <= 0) %winW = 1024;
   if (%winH <= 0) %winH = 768;

   // Camera center is (0, 0), camera view area is (100, 75)
   %sx = (%winW * 0.5) + (%wx * (%winW / 100.0));
   %sy = (%winH * 0.5) - (%wy * (%winH / 75.0));

   return mFloor(%sx) SPC mFloor(%sy);
}

//---------------------------------------------------------------------------------------------
// Spawning Methods
//---------------------------------------------------------------------------------------------
function FloatingTextManager::spawnText(%this, %worldX, %worldY, %text, %colorRgb, %durationMs)
{
   if (!isObject(mainScreenGui))
      return;

   if (!isObject(%this.activeTexts))
      %this.init();

   if (%durationMs $= "" || %durationMs <= 0)
      %durationMs = 1300; // 1.3s default animation duration

   if (%colorRgb $= "")
      %colorRgb = "255 255 255"; // Default pure white

   %colorR = getWord(%colorRgb, 0);
   %colorG = getWord(%colorRgb, 1);
   %colorB = getWord(%colorRgb, 2);

   // Convert world coordinates to screen space
   %screenPos = %this.worldToScreenPoint(%worldX, %worldY);
   %screenX = getWord(%screenPos, 0);
   %screenY = getWord(%screenPos, 1);

   %boxWidth = 240;
   %boxHeight = 32;
   %startX = mFloor(%screenX - (%boxWidth * 0.5));
   %startY = mFloor(%screenY - 35); // Spawn slightly above target

   %instId = %this.nextInstanceId++;

   // Unique GUI profiles for this instance to prevent cross-instance alpha interference
   %fgProfileName = "FCT_FG_" @ %instId;
   %shProfileName = "FCT_SH_" @ %instId;

   %fgProfile = new GuiControlProfile(%fgProfileName : GuiDefaultProfile)
   {
      fontType = "Arial";
      fontSize = 18;
      fontColor = %colorR SPC %colorG SPC %colorB SPC "255";
      fontColorHL = %colorR SPC %colorG SPC %colorB SPC "255";
      fontColorNA = %colorR SPC %colorG SPC %colorB SPC "255";
      align = "center";
      vAlign = "middle";
      fillColor = "0 0 0 0";
      border = false;
      useInput = false;
   };

   %shProfile = new GuiControlProfile(%shProfileName : GuiDefaultProfile)
   {
      fontType = "Arial";
      fontSize = 18;
      fontColor = "0 0 0 255";
      fontColorHL = "0 0 0 255";
      fontColorNA = "0 0 0 255";
      align = "center";
      vAlign = "middle";
      fillColor = "0 0 0 0";
      border = false;
      useInput = false;
   };

   // GUI Container (GuiControl is the standard text-rendering control in Torque2D 4.0)
   %container = new GuiControl()
   {
      Profile = FloatingTextContainerProfile;
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = %startX SPC %startY;
      Extent = %boxWidth SPC %boxHeight;
      MinExtent = "8 2";
      Visible = "1";
      isContainer = "1";

      // 4-point dark shadow / outline for high contrast against any background
      new GuiControl()
      {
         Profile = %shProfile;
         HorizSizing = "width";
         VertSizing = "height";
         Position = "1 0";
         Extent = %boxWidth SPC %boxHeight;
         text = %text;
      };
      new GuiControl()
      {
         Profile = %shProfile;
         HorizSizing = "width";
         VertSizing = "height";
         Position = "-1 0";
         Extent = %boxWidth SPC %boxHeight;
         text = %text;
      };
      new GuiControl()
      {
         Profile = %shProfile;
         HorizSizing = "width";
         VertSizing = "height";
         Position = "0 1";
         Extent = %boxWidth SPC %boxHeight;
         text = %text;
      };
      new GuiControl()
      {
         Profile = %shProfile;
         HorizSizing = "width";
         VertSizing = "height";
         Position = "0 -1";
         Extent = %boxWidth SPC %boxHeight;
         text = %text;
      };
      new GuiControl()
      {
         Profile = %shProfile;
         HorizSizing = "width";
         VertSizing = "height";
         Position = "1 1";
         Extent = %boxWidth SPC %boxHeight;
         text = %text;
      };

      // Foreground colored label
      new GuiControl()
      {
         Profile = %fgProfile;
         HorizSizing = "width";
         VertSizing = "height";
         Position = "0 0";
         Extent = %boxWidth SPC %boxHeight;
         text = %text;
      };
   };

   // Add to main screen GUI and bring to the very front so it renders on top of sceneWindow2D
   mainScreenGui.add(%container);
   mainScreenGui.pushToBack(%container);

   %instance = new ScriptObject()
   {
      class = "FloatingTextInstance";
      manager = %this;
      container = %container;
      fgProfile = %fgProfile;
      shadowProfile = %shProfile;
      startX = %startX;
      startY = %startY;
      floatDistance = 35; // 35 pixels upward float
      duration = %durationMs;
      fadeDuration = 500; // Final 500ms fade window
      elapsed = 0;
      colorR = %colorR;
      colorG = %colorG;
      colorB = %colorB;
   };

   %this.activeTexts.add(%instance);
   %this.startUpdateLoop();
}

function FloatingTextManager::spawnAbovePiece(%this, %piece, %text, %colorRgb, %durationMs)
{
   if (!isObject(%piece))
      return;

   %pos = %piece.getPosition();
   %wx = getWord(%pos, 0);
   %wy = getWord(%pos, 1) + 4.5; // Top-center of creature

   %this.spawnText(%wx, %wy, %text, %colorRgb, %durationMs);
}

function FloatingTextManager::spawnAboveAvatar(%this, %avatar, %text, %colorRgb, %durationMs)
{
   if (isObject(%avatar))
   {
      %pos = %avatar.getPosition();
      %wx = getWord(%pos, 0);
      %wy = getWord(%pos, 1) + 8.5; // Top-center of hero portrait
   }
   else
   {
      %wx = -40;
      %wy = -15.5;
   }

   %this.spawnText(%wx, %wy, %text, %colorRgb, %durationMs);
}

//---------------------------------------------------------------------------------------------
// Update Loop & Animation Progression (Non-Blocking)
//---------------------------------------------------------------------------------------------
function FloatingTextManager::startUpdateLoop(%this)
{
   if (!isEventPending(%this.updateSchedule))
      %this.updateSchedule = %this.schedule(%this.tickInterval, "onUpdateTick");
}

function FloatingTextManager::stopUpdateLoop(%this)
{
   if (isEventPending(%this.updateSchedule))
   {
      cancel(%this.updateSchedule);
      %this.updateSchedule = 0;
   }
}

function FloatingTextManager::onUpdateTick(%this)
{
   %this.updateSchedule = 0;

   if (!isObject(%this.activeTexts) || %this.activeTexts.getCount() == 0)
      return;

   %count = %this.activeTexts.getCount();
   for (%i = %count - 1; %i >= 0; %i--)
   {
      %inst = %this.activeTexts.getObject(%i);
      %inst.elapsed += %this.tickInterval;

      if (%inst.elapsed >= %inst.duration)
      {
         %this.cleanupTextInstance(%inst);
         continue;
      }

      // Smooth upward motion over duration
      %progress = %inst.elapsed / %inst.duration;
      %curY = %inst.startY - (%progress * %inst.floatDistance);
      if (isObject(%inst.container))
         %inst.container.setPosition(%inst.startX, mFloor(%curY));

      // Alpha fade during final 500ms
      %remaining = %inst.duration - %inst.elapsed;
      if (%remaining < %inst.fadeDuration)
      {
         %alphaFrac = %remaining / %inst.fadeDuration;
         if (%alphaFrac < 0) %alphaFrac = 0;
         if (%alphaFrac > 1) %alphaFrac = 1;
         %alpha = mFloor(%alphaFrac * 255);

         if (isObject(%inst.fgProfile))
         {
            %inst.fgProfile.fontColor = %inst.colorR SPC %inst.colorG SPC %inst.colorB SPC %alpha;
            %inst.fgProfile.fontColorHL = %inst.colorR SPC %inst.colorG SPC %inst.colorB SPC %alpha;
            %inst.fgProfile.fontColorNA = %inst.colorR SPC %inst.colorG SPC %inst.colorB SPC %alpha;
         }

         if (isObject(%inst.shadowProfile))
         {
            %inst.shadowProfile.fontColor = "0 0 0" SPC %alpha;
            %inst.shadowProfile.fontColorHL = "0 0 0" SPC %alpha;
            %inst.shadowProfile.fontColorNA = "0 0 0" SPC %alpha;
         }
      }
   }

   if (%this.activeTexts.getCount() > 0)
      %this.updateSchedule = %this.schedule(%this.tickInterval, "onUpdateTick");
}

function FloatingTextManager::cleanupTextInstance(%this, %inst)
{
   if (!isObject(%inst))
      return;

   if (isObject(%inst.container))
      %inst.container.delete();

   if (isObject(%inst.fgProfile))
      %inst.fgProfile.delete();

   if (isObject(%inst.shadowProfile))
      %inst.shadowProfile.delete();

   if (isObject(%this.activeTexts))
      %this.activeTexts.remove(%inst);

   %inst.delete();
}

function FloatingTextManager::clearAll(%this)
{
   %this.stopUpdateLoop();

   if (isObject(%this.activeTexts))
   {
      while (%this.activeTexts.getCount() > 0)
      {
         %inst = %this.activeTexts.getObject(0);
         %this.cleanupTextInstance(%inst);
      }
   }
}

//---------------------------------------------------------------------------------------------
// Global Delegation Wrapper Stubs (Backward Compatibility)
//---------------------------------------------------------------------------------------------
function spawnFloatingText(%wx, %wy, %text, %color, %duration)
{
   if (isObject(FloatingTextManager))
      FloatingTextManager.spawnText(%wx, %wy, %text, %color, %duration);
}

function spawnFloatingTextAbovePiece(%piece, %text, %color, %duration)
{
   if (isObject(FloatingTextManager))
      FloatingTextManager.spawnAbovePiece(%piece, %text, %color, %duration);
}

function spawnFloatingTextAboveAvatar(%avatar, %text, %color, %duration)
{
   if (isObject(FloatingTextManager))
      FloatingTextManager.spawnAboveAvatar(%avatar, %text, %color, %duration);
}

// Initialize Singleton
if (!isObject(FloatingTextManager))
{
   new ScriptObject(FloatingTextManager) { };
}
if (!isObject($FloatingTextManager))
{
   $FloatingTextManager = FloatingTextManager;
}
FloatingTextManager.init();
