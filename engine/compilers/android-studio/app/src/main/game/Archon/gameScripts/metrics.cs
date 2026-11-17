//---------------------------------------------------------------------------------------------
// Archonita - metrics.cs
// Developer console support (~ key) and real-time FPS tracking / low-FPS warning logger.
// Conforms to TORQUE_SCRIPT.md: ScriptObject subclass, lifecycle in onAdd/onRemove.
//---------------------------------------------------------------------------------------------

function Metrics::init(%this)
{
   if (%this.initialized)
      return;
   %this.initialized = true;

   // Ensure console output is flushed to console.log
   setLogMode(2);

   // Tracking configuration
   if (%this.tickInterval $= "" || %this.tickInterval <= 0)
      %this.tickInterval = 500;
   if (%this.lowFpsThreshold $= "" || %this.lowFpsThreshold <= 0)
      %this.lowFpsThreshold = 30;
   if (%this.useEngineOverlay $= "")
      %this.useEngineOverlay = false;

   %this.currentFPS = 0;
   %this.minFPS = 0;
   %this.maxFPS = 0;
   %this.lastRealTime = getRealTime();
   %this.lastFrameCount = $fps::frameCount;
   %this.updateSchedule = 0;

   // Build GUI profiles, developer console dialog, and on-screen FPS counter
   %this.createProfiles();
   %this.createConsoleUI();
   %this.createFpsOverlayUI();

   // Bind the tilde (~) key globally to toggle the in-game developer console
   %this.bindConsoleKey();

   // Start continuous framerate monitoring loop
   %this.startTracking();
}

function Metrics::onAdd(%this)
{
   %this.init();
}

function Metrics::onRemove(%this)
{
   %this.stopTracking();

   if (isObject(GlobalActionMap))
      GlobalActionMap.unbind("keyboard", "tilde");

   if (isObject(%this.consoleDialog))
   {
      if (isObject(Canvas) && %this.consoleDialog.isAwake())
         Canvas.popDialog(%this.consoleDialog);
      %this.consoleDialog.delete();
      %this.consoleDialog = "";
   }

   if (isObject(%this.fpsContainer))
   {
      %this.fpsContainer.delete();
      %this.fpsContainer = "";
   }

   if (isObject(%this.fpsContainerProfile))
      %this.fpsContainerProfile.delete();
   if (isObject(%this.fpsTextProfile))
      %this.fpsTextProfile.delete();
   if (isObject(%this.fpsWarnProfile))
      %this.fpsWarnProfile.delete();
   if (isObject(%this.fpsShadowProfile))
      %this.fpsShadowProfile.delete();
   if (isObject(%this.consoleOverlayProfile))
      %this.consoleOverlayProfile.delete();
   if (isObject(%this.consoleScrollProfile))
      %this.consoleScrollProfile.delete();
   if (isObject(%this.consoleTextProfile))
      %this.consoleTextProfile.delete();
   if (isObject(%this.consoleEditProfile))
      %this.consoleEditProfile.delete();
}

//---------------------------------------------------------------------------------------------
// GUI Profiles
//---------------------------------------------------------------------------------------------
function Metrics::createProfiles(%this)
{
   if (!isObject(%this.fpsContainerProfile))
   {
      %this.fpsContainerProfile = new GuiControlProfile(MetricsFpsContainerProfile : GuiDefaultProfile)
      {
         fillColor = "0 0 0 110";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.fpsTextProfile))
   {
      %this.fpsTextProfile = new GuiControlProfile(MetricsFpsTextProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 14;
         fontColor = "120 255 120 255";
         fontColorHL = "120 255 120 255";
         fontColorNA = "120 255 120 255";
         align = "left";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.fpsWarnProfile))
   {
      %this.fpsWarnProfile = new GuiControlProfile(MetricsFpsWarnProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 14;
         fontColor = "255 85 85 255";
         fontColorHL = "255 85 85 255";
         fontColorNA = "255 85 85 255";
         align = "left";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.fpsShadowProfile))
   {
      %this.fpsShadowProfile = new GuiControlProfile(MetricsFpsShadowProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 14;
         fontColor = "0 0 0 255";
         fontColorHL = "0 0 0 255";
         fontColorNA = "0 0 0 255";
         align = "left";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.consoleOverlayProfile))
   {
      %this.consoleOverlayProfile = new GuiControlProfile(MetricsConsoleOverlayProfile : GuiDefaultProfile)
      {
         fillColor = "15 18 24 225";
         border = true;
         borderColor = "70 85 110 255";
         opaque = true;
      };
   }

   if (!isObject(%this.consoleScrollProfile))
   {
      %this.consoleScrollProfile = new GuiControlProfile(MetricsConsoleScrollProfile : GuiDefaultProfile)
      {
         fillColor = "15 18 24 225";
         border = false;
         opaque = false;
      };
   }

   if (!isObject(%this.consoleTextProfile))
   {
      %this.consoleTextProfile = new GuiControlProfile(MetricsConsoleTextProfile : GuiDefaultProfile)
      {
         fontType = "Lucida Console";
         fontSize = 13;
         fontColor = "225 230 235 255";
         fontColorHL = "255 215 80 255";
         fontColorNA = "255 95 95 255";
         fillColor = "0 0 0 0";
         border = false;
      };
   }

   if (!isObject(%this.consoleEditProfile))
   {
      %this.consoleEditProfile = new GuiControlProfile(MetricsConsoleEditProfile : GuiDefaultProfile)
      {
         fontType = "Lucida Console";
         fontSize = 14;
         fontColor = "255 255 255 255";
         fontColorHL = "255 255 255 255";
         fillColor = "30 36 48 255";
         fillColorHL = "50 90 140 255";
         cursorColor = "255 255 255 255";
         border = true;
         borderColor = "90 120 160 255";
         opaque = true;
         canKeyFocus = true;
      };
   }
}

//---------------------------------------------------------------------------------------------
// Task 1: In-Game Developer Console & Tilde (~) Key Binding
//---------------------------------------------------------------------------------------------
function Metrics::bindConsoleKey(%this)
{
   if (isObject(GlobalActionMap))
   {
      GlobalActionMap.bind("keyboard", "tilde", "toggleConsole");
   }
}

function Metrics::createConsoleUI(%this)
{
   if (isObject(%this.consoleDialog))
      return;

   %this.consoleDialog = new GuiControl(ArchonConsoleDialog)
   {
      Profile = %this.consoleOverlayProfile;
      HorizSizing = "width";
      VertSizing = "bottom";
      Position = "0 0";
      Extent = "1024 420";
      MinExtent = "320 200";
      Visible = "1";
   };

   %this.consoleScroller = new GuiScrollCtrl()
   {
      Profile = %this.consoleScrollProfile;
      HorizSizing = "width";
      VertSizing = "height";
      Position = "0 0";
      Extent = "1024 388";
      MinExtent = "220 160";
      hScrollBar = "alwaysOff";
      vScrollBar = "alwaysOn";
      constantThumbHeight = "0";
      showArrowButtons = "1";
   };
   %this.consoleDialog.add(%this.consoleScroller);

   %this.consoleLog = new GuiConsole()
   {
      Profile = %this.consoleTextProfile;
      HorizSizing = "width";
      VertSizing = "height";
      Position = "0 0";
      Extent = "1024 388";
      Visible = "1";
   };
   %this.consoleScroller.add(%this.consoleLog);

   %this.consoleEntry = new GuiConsoleEditCtrl()
   {
      Profile = %this.consoleEditProfile;
      HorizSizing = "width";
      VertSizing = "top";
      Position = "0 388";
      Extent = "1024 32";
      MinExtent = "120 24";
      ReturnCommand = "$Metrics.evalConsole();";
      MaxLength = "255";
      Active = "1";
   };
   %this.consoleDialog.add(%this.consoleEntry);
}

function Metrics::toggleConsole(%this)
{
   if (!isObject(Canvas))
      return;

   if (!isObject(%this.consoleDialog))
      %this.createConsoleUI();

   if (%this.consoleDialog.isAwake())
   {
      if ($enableDirectInput)
         activateKeyboard();
      Canvas.popDialog(%this.consoleDialog);
      return;
   }

   if ($enableDirectInput)
      deactivateKeyboard();

   Canvas.pushDialog(%this.consoleDialog);
   if (isObject(%this.consoleScroller))
      %this.consoleScroller.scrollToBottom();
   if (isObject(%this.consoleEntry))
      %this.consoleEntry.setFirstResponder();
}

function Metrics::evalConsole(%this)
{
   if (!isObject(%this.consoleEntry))
      return;

   %text = trim(%this.consoleEntry.getValue());
   if (%text $= "")
      return;

   if (strpos(%text, "(") == -1 && strpos(%text, "=") == -1 && strpos(%text, " ") == -1 && strpos(%text, "{") == -1 && strpos(%text, "}") == -1)
   {
      %text = %text @ "()";
   }

   %pos = strlen(%text) - 1;
   if (strpos(%text, ";", %pos) == -1 && strpos(%text, "}") == -1)
   {
      %text = %text @ ";";
   }

   echo("==> " @ %text);
   eval(%text);
   %this.consoleEntry.setValue("");

   if (isObject(%this.consoleScroller))
      %this.consoleScroller.scrollToBottom();
}

//---------------------------------------------------------------------------------------------
// Task 2: On-Screen FPS Counter (GUI Text Control + Engine Metrics Overlay Support)
// Note: In Torque2D 4.0 EA4, GuiTextCtrl was merged into the base GuiControl class, which
// renders its .text field directly via setText().
//---------------------------------------------------------------------------------------------
function Metrics::createFpsOverlayUI(%this)
{
   if (!isObject(mainScreenGui))
      return;

   if (isObject(%this.fpsContainer))
   {
      if (!mainScreenGui.isMember(%this.fpsContainer))
         mainScreenGui.add(%this.fpsContainer);
      mainScreenGui.pushToBack(%this.fpsContainer);
      return;
   }

   %this.fpsContainer = new GuiControl(FpsCounterOverlay)
   {
      Profile = %this.fpsContainerProfile;
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "10 10";
      Extent = "120 26";
      MinExtent = "8 2";
      Visible = "1";
      isContainer = "1";
      useInput = false;
   };

   %this.fpsShadowLabel = new GuiControl()
   {
      Profile = %this.fpsShadowProfile;
      HorizSizing = "width";
      VertSizing = "height";
      Position = "9 4";
      Extent = "108 20";
      text = "FPS: --";
      useInput = false;
   };
   %this.fpsContainer.add(%this.fpsShadowLabel);

   %this.fpsLabel = new GuiControl()
   {
      Profile = %this.fpsTextProfile;
      HorizSizing = "width";
      VertSizing = "height";
      Position = "8 3";
      Extent = "108 20";
      text = "FPS: --";
      useInput = false;
   };
   %this.fpsContainer.add(%this.fpsLabel);

   mainScreenGui.add(%this.fpsContainer);
   mainScreenGui.pushToBack(%this.fpsContainer);
}

function Metrics::setEngineMetricsOverlay(%this, %enabled)
{
   %this.useEngineOverlay = %enabled;
   if (isObject(t2dScene))
   {
      if (%enabled)
         t2dScene.setDebugOn("fps");
      else
         t2dScene.setDebugOff("fps");
   }
}

//---------------------------------------------------------------------------------------------
// Tasks 3 & 4: Continuous Framerate Tracking Loop & Low-FPS (< 30) Warning Logger
//---------------------------------------------------------------------------------------------
function Metrics::startTracking(%this)
{
   if (!isEventPending(%this.updateSchedule))
      %this.updateSchedule = %this.schedule(%this.tickInterval, "onUpdateTick");
}

function Metrics::stopTracking(%this)
{
   if (isEventPending(%this.updateSchedule))
   {
      cancel(%this.updateSchedule);
      %this.updateSchedule = 0;
   }
}

function Metrics::sampleFPS(%this)
{
   %fps = 0;

   // 1. Query active Scene FPS stats if available (Torque2D 4.0 Scene::getFPS)
   if (isObject(t2dScene))
   {
      %sceneFps = t2dScene.getFPS();
      if (%sceneFps > 0)
      {
         %fps = %sceneFps;
         %this.minFPS = t2dScene.getMinFPS();
         %this.maxFPS = t2dScene.getMaxFPS();
      }
   }

   // 2. Fallback to Torque2D 4.0 engine global $fps::framePeriod (or legacy $fps::real)
   if (%fps <= 0)
   {
      if ($fps::framePeriod > 0)
         %fps = $fps::framePeriod;
      else if ($fps::real > 0)
         %fps = $fps::real;
   }

   %this.currentFPS = %fps;
   return %fps;
}

function Metrics::onUpdateTick(%this)
{
   %this.updateSchedule = 0;

   // Ensure GUI overlay is attached and kept at the front of mainScreenGui
   if (isObject(mainScreenGui))
   {
      if (!isObject(%this.fpsContainer))
         %this.createFpsOverlayUI();
      else
         mainScreenGui.pushToBack(%this.fpsContainer);
   }

   // Keep built-in Scene FPS debug overlay synchronized across level transitions if enabled
   if (%this.useEngineOverlay && isObject(t2dScene))
      t2dScene.setDebugOn("fps");

   %fps = %this.sampleFPS();
   if (%fps > 0)
   {
      %fpsFormatted = mFloatLength(%fps, 1);
      %labelText = "FPS: " @ %fpsFormatted;

      if (isObject(%this.fpsShadowLabel))
         %this.fpsShadowLabel.setText(%labelText);

      if (isObject(%this.fpsLabel))
      {
         %this.fpsLabel.setText(%labelText);
         if (%fps < %this.lowFpsThreshold)
            %this.fpsLabel.Profile = %this.fpsWarnProfile;
         else
            %this.fpsLabel.Profile = %this.fpsTextProfile;
      }

      // Task 4: Detect when FPS drops below 30 and log a timestamped warning to console.log
      if (%fps < %this.lowFpsThreshold)
      {
         %timestamp = getLocalTime() @ " | " @ getSimTime() @ "ms";
         echo("[WARNING] [" @ %timestamp @ "] Low framerate detected: " @ %fpsFormatted @ " FPS (below " @ %this.lowFpsThreshold @ " FPS threshold)");
      }
   }

   // Reschedule next monitoring tick
   %this.updateSchedule = %this.schedule(%this.tickInterval, "onUpdateTick");
}

//---------------------------------------------------------------------------------------------
// Global ActionMap Bind Target (Required by Engine ActionMap Dispatch)
//---------------------------------------------------------------------------------------------
function toggleConsole(%make)
{
   if (!%make)
      return;

   if (isObject($Metrics))
      $Metrics.toggleConsole();
   else if (isObject(Metrics))
      Metrics.toggleConsole();
}

// Initialize Singleton
if (!isObject(Metrics))
{
   new ScriptObject(Metrics) { };
}
if (!isObject($Metrics))
{
   $Metrics = Metrics;
}
Metrics.init();
