//---------------------------------------------------------------------------------------------
// Archonita - Version Configuration & Main Menu Watermark
// Reads external configuration (version.json / version.env) and manages UI watermark
//---------------------------------------------------------------------------------------------

function VersionConfig::onAdd(%this)
{
   %this.stage = "";
   %this.build = "";
   %this.version = "";
   %this.watermarkCtrl = "";

   %this.loadConfig();
   %this.createWatermarkUI();
}

function VersionConfig::onRemove(%this)
{
   if (isObject(%this.watermarkCtrl))
   {
      %this.watermarkCtrl.delete();
      %this.watermarkCtrl = "";
   }
}

function VersionConfig::loadConfig(%this)
{
   %foundPath = "";
   %paths = "Archon/version.json" TAB "./version.json" TAB "../version.json" TAB "version.json" TAB "Archon/version.env" TAB "./version.env" TAB "version.env";
   
   for (%i = 0; %i < getFieldCount(%paths); %i++)
   {
      %p = getField(%paths, %i);
      if (isFile(%p))
      {
         %foundPath = %p;
         break;
      }
   }

   if (%foundPath !$= "")
   {
      %file = new FileObject();
      if (%file.openForRead(%foundPath))
      {
         while (!%file.isEOF())
         {
            %line = trim(%file.readLine());
            if (%line $= "" || getSubStr(%line, 0, 1) $= "#" || getSubStr(%line, 0, 2) $= "//")
               continue;

            if (strpos(%line, ":") != -1)
            {
               %colonIdx = strpos(%line, ":");
               %rawKey = getSubStr(%line, 0, %colonIdx);
               %rawVal = getSubStr(%line, %colonIdx + 1, strlen(%line));

               %key = stripChars(trim(%rawKey), "\"'");
               %val = trim(stripChars(trim(%rawVal), "\"',"));

               if (%key $= "stage" || %key $= "channel")
                  %this.stage = %val;
               else if (%key $= "build")
                  %this.build = %val;
               else if (%key $= "version")
                  %this.version = %val;
            }
            else if (strpos(%line, "=") != -1)
            {
               %eqIdx = strpos(%line, "=");
               %rawKey = getSubStr(%line, 0, %eqIdx);
               %rawVal = getSubStr(%line, %eqIdx + 1, strlen(%line));

               %key = stripChars(trim(%rawKey), "\"'");
               %val = trim(stripChars(trim(%rawVal), "\"',"));

               if (%key $= "APP_STAGE" || %key $= "APP_CHANNEL" || %key $= "stage" || %key $= "channel")
                  %this.stage = %val;
               else if (%key $= "APP_BUILD" || %key $= "build")
                  %this.build = %val;
               else if (%key $= "APP_VERSION" || %key $= "version")
                  %this.version = %val;
            }
         }
         %file.close();
      }
      %file.delete();
   }

   // Fallback defaults if configuration file was empty or missing keys
   if (%this.stage $= "")
      %this.stage = "Early Access";
   if (%this.build $= "")
      %this.build = "101";
   if (%this.version $= "")
      %this.version = "2.0.1-ea4";

   $App::Channel = %this.stage;
   $App::Build = %this.build;
   $App::Version = %this.version;
}

function VersionConfig::createWatermarkUI(%this)
{
   if (isObject(MainMenuWatermark))
      MainMenuWatermark.delete();

   // Pure white font profiles with Arial font rendering
   if (!isObject(WatermarkWhiteHeaderProfile))
   {
      new GuiControlProfile(WatermarkWhiteHeaderProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 14;
         fontColor = "255 255 255 255";
         fontColorHL = "255 255 255 255";
         fontColorNA = "255 255 255 255";
         align = "right";
         vAlign = "middle";
         autoSizeWidth = false;
         autoSizeHeight = false;
         fillColor = "0 0 0 0";
         border = false;
      };
   }

   if (!isObject(WatermarkWhiteSubProfile))
   {
      new GuiControlProfile(WatermarkWhiteSubProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 12;
         fontColor = "255 255 255 255";
         fontColorHL = "255 255 255 255";
         fontColorNA = "255 255 255 255";
         align = "right";
         vAlign = "middle";
         autoSizeWidth = false;
         autoSizeHeight = false;
         fillColor = "0 0 0 0";
         border = false;
      };
   }

   // Drop shadow profiles for maximum readability against any background
   if (!isObject(WatermarkShadowHeaderProfile))
   {
      new GuiControlProfile(WatermarkShadowHeaderProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 14;
         fontColor = "0 0 0 255";
         fontColorHL = "0 0 0 255";
         fontColorNA = "0 0 0 255";
         align = "right";
         vAlign = "middle";
         autoSizeWidth = false;
         autoSizeHeight = false;
         fillColor = "0 0 0 0";
         border = false;
      };
   }

   if (!isObject(WatermarkShadowSubProfile))
   {
      new GuiControlProfile(WatermarkShadowSubProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 12;
         fontColor = "0 0 0 255";
         fontColorHL = "0 0 0 255";
         fontColorNA = "0 0 0 255";
         align = "right";
         vAlign = "middle";
         autoSizeWidth = false;
         autoSizeHeight = false;
         fillColor = "0 0 0 0";
         border = false;
      };
   }

   // Semi-transparent background container profile
   if (!isObject(WatermarkContainerProfile))
   {
      new GuiControlProfile(WatermarkContainerProfile : GuiDefaultProfile)
      {
         fillColor = "0 0 0 100";
         border = false;
      };
   }

   // Anchored firmly to the bottom-right corner of the viewport
   // HorizSizing = "left" anchors to the right edge; VertSizing = "top" anchors to the bottom edge.
   %container = new GuiControl(MainMenuWatermark)
   {
      Profile = "WatermarkContainerProfile";
      HorizSizing = "left";
      VertSizing = "top";
      Position = "744 694";
      Extent = "264 60";
      MinExtent = "8 2";
      Visible = "1";
      isContainer = "1";

      // Stage / Channel: 'Early Access' (Shadow + Pure White Foreground)
      new GuiControl()
      {
         Profile = "WatermarkShadowHeaderProfile";
         HorizSizing = "width";
         VertSizing = "bottom";
         Position = "1 3";
         Extent = "254 18";
         text = %this.stage;
      };
      new GuiControl()
      {
         Profile = "WatermarkWhiteHeaderProfile";
         HorizSizing = "width";
         VertSizing = "bottom";
         Position = "0 2";
         Extent = "254 18";
         text = %this.stage;
      };

      // Version Number (Shadow + Pure White Foreground)
      new GuiControl()
      {
         Profile = "WatermarkShadowSubProfile";
         HorizSizing = "width";
         VertSizing = "bottom";
         Position = "1 21";
         Extent = "254 18";
         text = "Version: " @ %this.version;
      };
      new GuiControl()
      {
         Profile = "WatermarkWhiteSubProfile";
         HorizSizing = "width";
         VertSizing = "bottom";
         Position = "0 20";
         Extent = "254 18";
         text = "Version: " @ %this.version;
      };

      // Build Number (Shadow + Pure White Foreground)
      new GuiControl()
      {
         Profile = "WatermarkShadowSubProfile";
         HorizSizing = "width";
         VertSizing = "bottom";
         Position = "1 39";
         Extent = "254 18";
         text = "Build: " @ %this.build;
      };
      new GuiControl()
      {
         Profile = "WatermarkWhiteSubProfile";
         HorizSizing = "width";
         VertSizing = "bottom";
         Position = "0 38";
         Extent = "254 18";
         text = "Build: " @ %this.build;
      };
   };

   %this.watermarkCtrl = %container;

   if (isObject(mainScreenGui))
   {
      mainScreenGui.add(%container);
      mainScreenGui.pushToBack(%container);
   }
}

function VersionConfig::showWatermark(%this, %show)
{
   if (!isObject(%this.watermarkCtrl) || !isObject(MainMenuWatermark))
      %this.createWatermarkUI();

   if (isObject(MainMenuWatermark))
   {
      MainMenuWatermark.setVisible(%show);
      if (%show && isObject(mainScreenGui))
      {
         // Bring watermark to the front of rendering (drawn after sceneWindow2D)
         mainScreenGui.pushToBack(MainMenuWatermark);
      }
   }
}

//---------------------------------------------------------------------------------------------
// Global Convenience Wrapper
//---------------------------------------------------------------------------------------------

function showVersionWatermark(%show)
{
   if (isObject($VersionConfig))
      $VersionConfig.showWatermark(%show);
}
