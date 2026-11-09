//---------------------------------------------------------------------------------------------
// Archonita - FactionSelectGui.cs
// Modal GUI allowing the player to select the opposing faction (Undead Legion,
// Dragonfolk Faction, or Gremlin Faction) before starting the tactical battle.
//---------------------------------------------------------------------------------------------

function FactionSelectGui::onAdd(%this)
{
   %this.borderProfile = "";
   %this.borderSelectedProfile = "";
   %this.backdropProfile = "";
   %this.panelProfile = "";
   %this.cardProfile = "";
   %this.cardSelectedProfile = "";
   %this.headerProfile = "";
   %this.subTextProfile = "";
   %this.cardTitleProfile = "";
   %this.cardSubtitleProfile = "";
   %this.unitTextProfile = "";
   %this.descTextProfile = "";
   %this.btnNormalProfile = "";
   %this.btnSelectedProfile = "";
   %this.btnActionProfile = "";

   %this.backdropCtrl = "";
   %this.panelCtrl = "";

   %this.createProfiles();
   %this.createControls();
   %this.updateVisuals();
}

function FactionSelectGui::onRemove(%this)
{
   if (isObject(%this.backdropCtrl))
      %this.backdropCtrl.delete();

   if (isObject(%this.btnActionProfile))
      %this.btnActionProfile.delete();
   if (isObject(%this.btnSelectedProfile))
      %this.btnSelectedProfile.delete();
   if (isObject(%this.btnNormalProfile))
      %this.btnNormalProfile.delete();
   if (isObject(%this.descTextProfile))
      %this.descTextProfile.delete();
   if (isObject(%this.unitTextProfile))
      %this.unitTextProfile.delete();
   if (isObject(%this.cardSubtitleProfile))
      %this.cardSubtitleProfile.delete();
   if (isObject(%this.cardTitleProfile))
      %this.cardTitleProfile.delete();
   if (isObject(%this.subTextProfile))
      %this.subTextProfile.delete();
   if (isObject(%this.headerProfile))
      %this.headerProfile.delete();
   if (isObject(%this.cardSelectedProfile))
      %this.cardSelectedProfile.delete();
   if (isObject(%this.cardProfile))
      %this.cardProfile.delete();
   if (isObject(%this.panelProfile))
      %this.panelProfile.delete();
   if (isObject(%this.backdropProfile))
      %this.backdropProfile.delete();
   if (isObject(%this.borderSelectedProfile))
      %this.borderSelectedProfile.delete();
   if (isObject(%this.borderProfile))
      %this.borderProfile.delete();
}

function FactionSelectGui::createProfiles(%this)
{
   if (!isObject(%this.borderProfile))
   {
      %this.borderProfile = new GuiBorderProfile(FactionSelectBorderProfile)
      {
         border = 2;
         borderHL = 2;
         borderSL = 2;
         borderNA = 1;
         borderColor = "190 155 85 220";
         borderColorHL = "250 220 135 255";
         borderColorSL = "255 240 175 255";
         borderColorNA = "95 100 110 130";
         underfill = true;
      };
   }

   if (!isObject(%this.borderSelectedProfile))
   {
      %this.borderSelectedProfile = new GuiBorderProfile(FactionSelectBorderSelectedProfile)
      {
         border = 3;
         borderHL = 3;
         borderSL = 3;
         borderNA = 2;
         borderColor = "255 215 80 255";
         borderColorHL = "255 240 140 255";
         borderColorSL = "255 255 190 255";
         borderColorNA = "180 140 50 200";
         underfill = true;
      };
   }

   if (!isObject(%this.backdropProfile))
   {
      %this.backdropProfile = new GuiControlProfile(FactionSelectBackdropProfile : GuiDefaultProfile)
      {
         fillColor = "0 0 0 185";
         border = false;
         useInput = true;
      };
   }

   if (!isObject(%this.panelProfile))
   {
      %this.panelProfile = new GuiControlProfile(FactionSelectPanelProfile : GuiDefaultProfile)
      {
         fillColor = "14 18 28 248";
         borderLeft = %this.borderProfile;
         borderRight = %this.borderProfile;
         borderTop = %this.borderProfile;
         borderBottom = %this.borderProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.cardProfile))
   {
      %this.cardProfile = new GuiControlProfile(FactionSelectCardProfile : GuiDefaultProfile)
      {
         fillColor = "20 25 38 230";
         borderLeft = %this.borderProfile;
         borderRight = %this.borderProfile;
         borderTop = %this.borderProfile;
         borderBottom = %this.borderProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.cardSelectedProfile))
   {
      %this.cardSelectedProfile = new GuiControlProfile(FactionSelectCardSelectedProfile : GuiDefaultProfile)
      {
         fillColor = "32 40 60 250";
         borderLeft = %this.borderSelectedProfile;
         borderRight = %this.borderSelectedProfile;
         borderTop = %this.borderSelectedProfile;
         borderBottom = %this.borderSelectedProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.headerProfile))
   {
      %this.headerProfile = new GuiControlProfile(FactionSelectHeaderProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 22;
         fontColor = "255 225 140 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.subTextProfile))
   {
      %this.subTextProfile = new GuiControlProfile(FactionSelectSubTextProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 13;
         fontColor = "205 215 235 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.cardTitleProfile))
   {
      %this.cardTitleProfile = new GuiControlProfile(FactionSelectCardTitleProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 17;
         fontColor = "255 230 150 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.cardSubtitleProfile))
   {
      %this.cardSubtitleProfile = new GuiControlProfile(FactionSelectCardSubtitleProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 12;
         fontColor = "180 195 215 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.unitTextProfile))
   {
      %this.unitTextProfile = new GuiControlProfile(FactionSelectUnitTextProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 12;
         fontColor = "235 240 250 255";
         align = "left";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.descTextProfile))
   {
      %this.descTextProfile = new GuiControlProfile(FactionSelectDescTextProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 11;
         fontColor = "190 200 215 230";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.btnNormalProfile))
   {
      %this.btnNormalProfile = new GuiControlProfile(FactionSelectBtnNormalProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 13;
         fillColor = "28 35 50 240";
         fillColorHL = "45 56 78 255";
         fillColorSL = "60 76 104 255";
         fillColorNA = "20 24 32 140";
         fontColor = "215 225 245 255";
         fontColorHL = "255 255 255 255";
         fontColorSL = "255 255 255 255";
         fontColorNA = "100 108 120 150";
         align = "center";
         vAlign = "middle";
         borderLeft = %this.borderProfile;
         borderRight = %this.borderProfile;
         borderTop = %this.borderProfile;
         borderBottom = %this.borderProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.btnSelectedProfile))
   {
      %this.btnSelectedProfile = new GuiControlProfile(FactionSelectBtnSelectedProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 13;
         fillColor = "65 85 125 255";
         fillColorHL = "80 105 150 255";
         fillColorSL = "95 125 175 255";
         fillColorNA = "20 24 32 140";
         fontColor = "255 240 160 255";
         fontColorHL = "255 255 220 255";
         fontColorSL = "255 255 255 255";
         fontColorNA = "100 108 120 150";
         align = "center";
         vAlign = "middle";
         borderLeft = %this.borderSelectedProfile;
         borderRight = %this.borderSelectedProfile;
         borderTop = %this.borderSelectedProfile;
         borderBottom = %this.borderSelectedProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.btnActionProfile))
   {
      %this.btnActionProfile = new GuiControlProfile(FactionSelectBtnActionProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 15;
         fillColor = "110 82 32 250";
         fillColorHL = "140 105 42 255";
         fillColorSL = "170 130 55 255";
         fillColorNA = "45 36 20 140";
         fontColor = "255 245 205 255";
         fontColorHL = "255 255 255 255";
         fontColorSL = "255 255 255 255";
         fontColorNA = "120 110 90 150";
         align = "center";
         vAlign = "middle";
         borderLeft = %this.borderProfile;
         borderRight = %this.borderProfile;
         borderTop = %this.borderProfile;
         borderBottom = %this.borderProfile;
         useInput = true;
      };
   }
}

function FactionSelectGui::createControls(%this)
{
   if (isObject(%this.backdropCtrl))
      %this.backdropCtrl.delete();

   %mainW = 1024;
   %mainH = 768;
   if (isObject(mainScreenGui))
   {
      %mainW = getWord(mainScreenGui.extent, 0);
      %mainH = getWord(mainScreenGui.extent, 1);
   }

   %panelW = 750;
   %panelH = 490;
   %panelX = mFloor((%mainW - %panelW) / 2);
   %panelY = mFloor((%mainH - %panelH) / 2);

   %this.backdropCtrl = new GuiControl(FactionSelectBackdrop)
   {
      Profile = %this.backdropProfile;
      HorizSizing = "width";
      VertSizing = "height";
      Position = "0 0";
      Extent = %mainW SPC %mainH;
      Visible = "0";
   };

   %this.panelCtrl = new GuiControl(FactionSelectPanel)
   {
      Profile = %this.panelProfile;
      HorizSizing = "center";
      VertSizing = "center";
      Position = %panelX SPC %panelY;
      Extent = %panelW SPC %panelH;
      Visible = "1";
   };

   // Header
   %this.headerLabel = new GuiControl()
   {
      Profile = %this.headerProfile;
      Position = "25 14";
      Extent = "700 28";
      Text = "CHOOSE ENEMY FACTION";
   };
   %this.panelCtrl.add(%this.headerLabel);

   %this.subHeaderLabel = new GuiControl()
   {
      Profile = %this.subTextProfile;
      Position = "25 44";
      Extent = "700 20";
      Text = "Select which opposing legion will confront you on the board";
   };
   %this.panelCtrl.add(%this.subHeaderLabel);

   // 3 Faction Cards
   %cardKeys = "undead" TAB "dragonfolk" TAB "gremlin";
   %cardTitles = "UNDEAD LEGION" TAB "DRAGONFOLK" TAB "GREMLIN HORDE";
   %cardSubtitles = "The Risen Dead" TAB "The Scaled Clan" TAB "The Underworld Horde";

   for (%i = 0; %i < 3; %i++)
   {
      %fKey = getField(%cardKeys, %i);
      %cx = 24 + (%i * 238);
      %cy = 72;

      %this.cardCtrl[%i] = new GuiControl()
      {
         Profile = %this.cardProfile;
         Position = %cx SPC %cy;
         Extent = "226 340";
      };

      %this.cardTitle[%i] = new GuiControl()
      {
         Profile = %this.cardTitleProfile;
         Position = "10 12";
         Extent = "206 24";
         Text = getField(%cardTitles, %i);
      };
      %this.cardCtrl[%i].add(%this.cardTitle[%i]);

      %this.cardSubtitle[%i] = new GuiControl()
      {
         Profile = %this.cardSubtitleProfile;
         Position = "10 36";
         Extent = "206 18";
         Text = getField(%cardSubtitles, %i);
      };
      %this.cardCtrl[%i].add(%this.cardSubtitle[%i]);

      // Unit Roster
      %units = $FactionManager.getFactionUnitList(%fKey);
      for (%u = 0; %u < 4; %u++)
      {
         %uName = getField(%units, %u);
         %uy = 64 + (%u * 32);

         %this.unitLabel[%i, %u] = new GuiControl()
         {
            Profile = %this.unitTextProfile;
            Position = "18" SPC %uy;
            Extent = "194 28";
            Text = "• " @ %uName;
         };
         %this.cardCtrl[%i].add(%this.unitLabel[%i, %u]);
      }

      // Tactical Specialty / Flavor
      %specText = "";
      if (%fKey $= "undead")
         %specText = "Necrotic afflictions: Disease (-1 ATK) and Ghostly Touch (-2 DEF).";
      else if (%fKey $= "dragonfolk")
         %specText = "Scaled fury: High durability, blazing breath & lightning strikes.";
      else if (%fKey $= "gremlin")
         %specText = "Underworld menace: Vicious swarm, eye-beams & infernal demon lord.";

      %this.descLabel[%i] = new GuiControl()
      {
         Profile = %this.descTextProfile;
         Position = "10 198";
         Extent = "206 72";
         Text = %specText;
      };
      %this.cardCtrl[%i].add(%this.descLabel[%i]);

      // Card Select Button
      %this.cardBtn[%i] = new GuiButtonCtrl()
      {
         Profile = %this.btnNormalProfile;
         Position = "18 284";
         Extent = "190 38";
         Text = "SELECT";
         Command = "$FactionSelectGui.onSelectFaction(\"" @ %fKey @ "\");";
      };
      %this.cardCtrl[%i].add(%this.cardBtn[%i]);

      %this.panelCtrl.add(%this.cardCtrl[%i]);
   }

   // Bottom Bar Buttons
   %this.btnBack = new GuiButtonCtrl(FactionSelectBackBtn)
   {
      Profile = %this.btnNormalProfile;
      Position = "140 430";
      Extent = "200 40";
      Text = "CHANGE HERO";
      Command = "$FactionSelectGui.onBackClicked();";
   };
   %this.panelCtrl.add(%this.btnBack);

   %this.btnStart = new GuiButtonCtrl(FactionSelectStartBtn)
   {
      Profile = %this.btnActionProfile;
      Position = "410 430";
      Extent = "200 40";
      Text = "BEGIN BATTLE";
      Command = "$FactionSelectGui.onStartClicked();";
   };
   %this.panelCtrl.add(%this.btnStart);

   %this.backdropCtrl.add(%this.panelCtrl);
   mainScreenGui.add(%this.backdropCtrl);
}

function FactionSelectGui::reanchor(%this)
{
   if (!isObject(%this.backdropCtrl) || !isObject(%this.panelCtrl))
      return;

   %mainW = 1024;
   %mainH = 768;
   if (isObject(mainScreenGui))
   {
      %mainW = getWord(mainScreenGui.extent, 0);
      %mainH = getWord(mainScreenGui.extent, 1);
   }

   %panelW = 750;
   %panelH = 490;
   %panelX = mFloor((%mainW - %panelW) / 2);
   %panelY = mFloor((%mainH - %panelH) / 2);

   %this.backdropCtrl.Position = "0 0";
   %this.backdropCtrl.Extent = %mainW SPC %mainH;
   %this.panelCtrl.Position = %panelX SPC %panelY;
   %this.panelCtrl.Extent = %panelW SPC %panelH;
}

function FactionSelectGui::isVisible(%this)
{
   if (!isObject(%this.backdropCtrl))
      return false;
   return %this.backdropCtrl.isVisible();
}

function FactionSelectGui::show(%this, %visible)
{
   if (!isObject(%this.backdropCtrl))
      %this.createControls();

   if (%visible)
   {
      %this.reanchor();
      %this.updateVisuals();
      %this.backdropCtrl.setVisible(true);
      mainScreenGui.pushToBack(%this.backdropCtrl);
   }
   else
   {
      %this.backdropCtrl.setVisible(false);
   }
}

function FactionSelectGui::updateVisuals(%this)
{
   if (!isObject($FactionManager))
      return;

   %currentFaction = $FactionManager.getEnemyFaction();
   %cardKeys = "undead" TAB "dragonfolk" TAB "gremlin";

   for (%i = 0; %i < 3; %i++)
   {
      %fKey = getField(%cardKeys, %i);
      %isSelected = (%fKey $= %currentFaction);

      if (isObject(%this.cardCtrl[%i]))
      {
         if (%isSelected)
            %this.cardCtrl[%i].setProfile(%this.cardSelectedProfile);
         else
            %this.cardCtrl[%i].setProfile(%this.cardProfile);
      }

      if (isObject(%this.cardBtn[%i]))
      {
         if (%isSelected)
         {
            %this.cardBtn[%i].setProfile(%this.btnSelectedProfile);
            %this.cardBtn[%i].setText("[ SELECTED ]");
         }
         else
         {
            %this.cardBtn[%i].setProfile(%this.btnNormalProfile);
            %this.cardBtn[%i].setText("SELECT");
         }
      }
   }
}

function FactionSelectGui::onSelectFaction(%this, %factionKey)
{
   playArchonSound(fxSword);
   if (isObject($FactionManager))
      $FactionManager.setEnemyFaction(%factionKey);
   %this.updateVisuals();
}

function FactionSelectGui::onStartClicked(%this)
{
   playArchonSound(fxSword);
   %this.show(false);

   // Fade out hero card screen and schedule board screen transition
   cardScreenFadeOut();
   if ($boardScreenSchedule !$= "")
      cancel($boardScreenSchedule);
   $boardScreenSchedule = schedule(1000, 0, showBoardScreen);
}

function FactionSelectGui::onBackClicked(%this)
{
   playArchonSound(fxSword);
   %this.show(false);

   // Re-enable hero card selection
   $cardSelectTransitioning = false;
   if (isObject(card_0))
      card_0.valittu = 0;
   if (isObject(card_1))
      card_1.valittu = 0;
}

// Global Singleton Initialization
if (!isObject($FactionSelectGui))
{
   $FactionSelectGui = new ScriptObject(FactionSelectGui) { };
}
