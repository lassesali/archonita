//---------------------------------------------------------------------------------------------
// Archonita - BattleSummaryGui.cs
// Post-battle modal summary overlay showing Victory/Defeat banner, army casualty tallies,
// surviving unit roster breakdown, and Play Again / Return to Main Menu controls.
// Conforms to TORQUE_SCRIPT.md: single-class ScriptObject, lifecycle in onAdd/onRemove.
//---------------------------------------------------------------------------------------------

function BattleSummaryGui::onAdd(%this)
{
   %this.outcome = "VICTORY";
   %this.isVictory = true;
   %this.turns = 0;
   %this.playerSurviving = 0;
   %this.playerTotal = 0;
   %this.playerCasualties = 0;
   %this.playerSurvivingHP = 0;
   %this.enemySurviving = 0;
   %this.enemyTotal = 0;
   %this.enemyCasualties = 0;
   %this.enemySurvivingHP = 0;
   %this.survivingUnitCount = 0;
   %this.enemyFactionName = "Undead Legion";
   %this.heroClassName = "Warrior";

   %this.goldBorderProfile = "";
   %this.crimsonBorderProfile = "";
   %this.backdropProfile = "";
   %this.panelProfile = "";
   %this.victoryBannerProfile = "";
   %this.defeatBannerProfile = "";
   %this.victoryTitleProfile = "";
   %this.defeatTitleProfile = "";
   %this.subtitleProfile = "";
   %this.metaProfile = "";
   %this.cardProfile = "";
   %this.cardHeaderProfile = "";
   %this.tallyValueProfile = "";
   %this.tallyZeroProfile = "";
   %this.tallyDetailProfile = "";
   %this.rosterHeaderProfile = "";
   %this.rosterRowProfile = "";
   %this.btnPrimaryProfile = "";
   %this.btnSecondaryProfile = "";

   %this.backdropCtrl = "";
   %this.panelCtrl = "";
   %this.bannerBox = "";
   %this.bannerTitleLabel = "";
   %this.bannerSubtitleLabel = "";
   %this.bannerMetaLabel = "";
   %this.playerCard = "";
   %this.playerHeaderLabel = "";
   %this.playerTallyLabel = "";
   %this.playerDetailLabel = "";
   %this.enemyCard = "";
   %this.enemyHeaderLabel = "";
   %this.enemyTallyLabel = "";
   %this.enemyDetailLabel = "";
   %this.rosterBox = "";
   %this.rosterHeaderLabel = "";
   for (%i = 0; %i < 6; %i++)
      %this.unitRowLabel[%i] = "";
   %this.btnPlayAgain = "";
   %this.btnMainMenu = "";

   %this.createProfiles();
   %this.createControls();
   %this.refreshLocalization();
}

function BattleSummaryGui::onRemove(%this)
{
   if (isObject(%this.backdropCtrl))
      %this.backdropCtrl.delete();

   if (isObject(%this.btnSecondaryProfile))
      %this.btnSecondaryProfile.delete();
   if (isObject(%this.btnPrimaryProfile))
      %this.btnPrimaryProfile.delete();
   if (isObject(%this.rosterRowProfile))
      %this.rosterRowProfile.delete();
   if (isObject(%this.rosterHeaderProfile))
      %this.rosterHeaderProfile.delete();
   if (isObject(%this.tallyDetailProfile))
      %this.tallyDetailProfile.delete();
   if (isObject(%this.tallyZeroProfile))
      %this.tallyZeroProfile.delete();
   if (isObject(%this.tallyValueProfile))
      %this.tallyValueProfile.delete();
   if (isObject(%this.cardHeaderProfile))
      %this.cardHeaderProfile.delete();
   if (isObject(%this.cardProfile))
      %this.cardProfile.delete();
   if (isObject(%this.metaProfile))
      %this.metaProfile.delete();
   if (isObject(%this.subtitleProfile))
      %this.subtitleProfile.delete();
   if (isObject(%this.defeatTitleProfile))
      %this.defeatTitleProfile.delete();
   if (isObject(%this.victoryTitleProfile))
      %this.victoryTitleProfile.delete();
   if (isObject(%this.defeatBannerProfile))
      %this.defeatBannerProfile.delete();
   if (isObject(%this.victoryBannerProfile))
      %this.victoryBannerProfile.delete();
   if (isObject(%this.panelProfile))
      %this.panelProfile.delete();
   if (isObject(%this.backdropProfile))
      %this.backdropProfile.delete();
   if (isObject(%this.crimsonBorderProfile))
      %this.crimsonBorderProfile.delete();
   if (isObject(%this.goldBorderProfile))
      %this.goldBorderProfile.delete();
}

function BattleSummaryGui::createProfiles(%this)
{
   if (!isObject(%this.goldBorderProfile))
   {
      %this.goldBorderProfile = new GuiBorderProfile(BattleSummaryGoldBorderProfile)
      {
         border = 2;
         borderHL = 2;
         borderSL = 2;
         borderNA = 1;
         borderColor = "215 175 85 240";
         borderColorHL = "255 225 140 255";
         borderColorSL = "255 245 185 255";
         borderColorNA = "110 100 80 140";
         underfill = true;
      };
   }

   if (!isObject(%this.crimsonBorderProfile))
   {
      %this.crimsonBorderProfile = new GuiBorderProfile(BattleSummaryCrimsonBorderProfile)
      {
         border = 2;
         borderHL = 2;
         borderSL = 2;
         borderNA = 1;
         borderColor = "205 70 70 240";
         borderColorHL = "245 105 105 255";
         borderColorSL = "255 140 140 255";
         borderColorNA = "120 55 55 140";
         underfill = true;
      };
   }

   if (!isObject(%this.backdropProfile))
   {
      %this.backdropProfile = new GuiControlProfile(BattleSummaryBackdropProfile : GuiDefaultProfile)
      {
         fillColor = "0 0 0 175";
         border = false;
         useInput = true;
      };
   }

   if (!isObject(%this.panelProfile))
   {
      %this.panelProfile = new GuiControlProfile(BattleSummaryPanelProfile : GuiDefaultProfile)
      {
         fillColor = "14 18 28 246";
         borderLeft = %this.goldBorderProfile;
         borderRight = %this.goldBorderProfile;
         borderTop = %this.goldBorderProfile;
         borderBottom = %this.goldBorderProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.victoryBannerProfile))
   {
      %this.victoryBannerProfile = new GuiControlProfile(BattleSummaryVictoryBannerProfile : GuiDefaultProfile)
      {
         fillColor = "46 38 14 245";
         borderLeft = %this.goldBorderProfile;
         borderRight = %this.goldBorderProfile;
         borderTop = %this.goldBorderProfile;
         borderBottom = %this.goldBorderProfile;
         useInput = false;
      };
   }

   if (!isObject(%this.defeatBannerProfile))
   {
      %this.defeatBannerProfile = new GuiControlProfile(BattleSummaryDefeatBannerProfile : GuiDefaultProfile)
      {
         fillColor = "52 18 22 245";
         borderLeft = %this.crimsonBorderProfile;
         borderRight = %this.crimsonBorderProfile;
         borderTop = %this.crimsonBorderProfile;
         borderBottom = %this.crimsonBorderProfile;
         useInput = false;
      };
   }

   if (!isObject(%this.victoryTitleProfile))
   {
      %this.victoryTitleProfile = new GuiControlProfile(BattleSummaryVictoryTitleProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 28;
         fontColor = "255 225 110 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.defeatTitleProfile))
   {
      %this.defeatTitleProfile = new GuiControlProfile(BattleSummaryDefeatTitleProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 28;
         fontColor = "255 105 105 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.subtitleProfile))
   {
      %this.subtitleProfile = new GuiControlProfile(BattleSummarySubtitleProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 14;
         fontColor = "225 232 245 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.metaProfile))
   {
      %this.metaProfile = new GuiControlProfile(BattleSummaryMetaProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 12;
         fontColor = "180 195 215 240";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.cardProfile))
   {
      %this.cardProfile = new GuiControlProfile(BattleSummaryCardProfile : GuiDefaultProfile)
      {
         fillColor = "22 28 42 235";
         borderLeft = %this.goldBorderProfile;
         borderRight = %this.goldBorderProfile;
         borderTop = %this.goldBorderProfile;
         borderBottom = %this.goldBorderProfile;
         useInput = false;
      };
   }

   if (!isObject(%this.cardHeaderProfile))
   {
      %this.cardHeaderProfile = new GuiControlProfile(BattleSummaryCardHeaderProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 12;
         fontColor = "255 225 140 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.tallyValueProfile))
   {
      %this.tallyValueProfile = new GuiControlProfile(BattleSummaryTallyValueProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 16;
         fontColor = "150 255 175 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.tallyZeroProfile))
   {
      %this.tallyZeroProfile = new GuiControlProfile(BattleSummaryTallyZeroProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 16;
         fontColor = "255 130 130 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.tallyDetailProfile))
   {
      %this.tallyDetailProfile = new GuiControlProfile(BattleSummaryTallyDetailProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 12;
         fontColor = "195 205 220 240";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.rosterHeaderProfile))
   {
      %this.rosterHeaderProfile = new GuiControlProfile(BattleSummaryRosterHeaderProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 13;
         fontColor = "255 225 140 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.rosterRowProfile))
   {
      %this.rosterRowProfile = new GuiControlProfile(BattleSummaryRosterRowProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 13;
         fontColor = "235 240 250 255";
         align = "left";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.btnPrimaryProfile))
   {
      %this.btnPrimaryProfile = new GuiControlProfile(BattleSummaryBtnPrimaryProfile : GuiDefaultProfile)
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
         borderLeft = %this.goldBorderProfile;
         borderRight = %this.goldBorderProfile;
         borderTop = %this.goldBorderProfile;
         borderBottom = %this.goldBorderProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.btnSecondaryProfile))
   {
      %this.btnSecondaryProfile = new GuiControlProfile(BattleSummaryBtnSecondaryProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 15;
         fillColor = "30 38 56 240";
         fillColorHL = "46 58 84 255";
         fillColorSL = "62 78 112 255";
         fillColorNA = "24 28 34 140";
         fontColor = "235 240 250 255";
         fontColorHL = "255 245 205 255";
         fontColorSL = "255 255 255 255";
         fontColorNA = "110 115 125 150";
         align = "center";
         vAlign = "middle";
         borderLeft = %this.goldBorderProfile;
         borderRight = %this.goldBorderProfile;
         borderTop = %this.goldBorderProfile;
         borderBottom = %this.goldBorderProfile;
         useInput = true;
      };
   }
}

function BattleSummaryGui::createControls(%this)
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

   %panelW = 560;
   %panelH = 450;
   %panelX = mFloor((%mainW - %panelW) / 2);
   %panelY = mFloor((%mainH - %panelH) / 2);

   %this.backdropCtrl = new GuiControl(BattleSummaryBackdrop)
   {
      Profile = %this.backdropProfile;
      HorizSizing = "width";
      VertSizing = "height";
      Position = "0 0";
      Extent = %mainW SPC %mainH;
      Visible = "0";
   };

   %this.panelCtrl = new GuiControl(BattleSummaryPanel)
   {
      Profile = %this.panelProfile;
      HorizSizing = "center";
      VertSizing = "center";
      Position = %panelX SPC %panelY;
      Extent = %panelW SPC %panelH;
      Visible = "1";
   };

   // Banner Box
   %this.bannerBox = new GuiControl(BattleSummaryBannerBox)
   {
      Profile = %this.victoryBannerProfile;
      Position = "20 16";
      Extent = "520 86";
   };

   %this.bannerTitleLabel = new GuiControl(BattleSummaryBannerTitle)
   {
      Profile = %this.victoryTitleProfile;
      Position = "10 6";
      Extent = "500 34";
      Text = "VICTORY";
   };

   %this.bannerSubtitleLabel = new GuiControl(BattleSummaryBannerSubtitle)
   {
      Profile = %this.subtitleProfile;
      Position = "10 40";
      Extent = "500 22";
      Text = "";
   };

   %this.bannerMetaLabel = new GuiControl(BattleSummaryBannerMeta)
   {
      Profile = %this.metaProfile;
      Position = "10 62";
      Extent = "500 18";
      Text = "";
   };

   %this.bannerBox.add(%this.bannerTitleLabel);
   %this.bannerBox.add(%this.bannerSubtitleLabel);
   %this.bannerBox.add(%this.bannerMetaLabel);
   %this.panelCtrl.add(%this.bannerBox);

   // Player Army Tally Card
   %this.playerCard = new GuiControl(BattleSummaryPlayerCard)
   {
      Profile = %this.cardProfile;
      Position = "20 112";
      Extent = "252 74";
   };

   %this.playerHeaderLabel = new GuiControl(BattleSummaryPlayerHeader)
   {
      Profile = %this.cardHeaderProfile;
      Position = "8 6";
      Extent = "236 18";
      Text = "PLAYER FORCES (HUMAN ORDER)";
   };

   %this.playerTallyLabel = new GuiControl(BattleSummaryPlayerTally)
   {
      Profile = %this.tallyValueProfile;
      Position = "8 26";
      Extent = "236 22";
      Text = "0 / 0 Units Surviving";
   };

   %this.playerDetailLabel = new GuiControl(BattleSummaryPlayerDetail)
   {
      Profile = %this.tallyDetailProfile;
      Position = "8 50";
      Extent = "236 18";
      Text = "Remaining HP: 0  |  Fallen: 0";
   };

   %this.playerCard.add(%this.playerHeaderLabel);
   %this.playerCard.add(%this.playerTallyLabel);
   %this.playerCard.add(%this.playerDetailLabel);
   %this.panelCtrl.add(%this.playerCard);

   // Enemy Army Tally Card
   %this.enemyCard = new GuiControl(BattleSummaryEnemyCard)
   {
      Profile = %this.cardProfile;
      Position = "288 112";
      Extent = "252 74";
   };

   %this.enemyHeaderLabel = new GuiControl(BattleSummaryEnemyHeader)
   {
      Profile = %this.cardHeaderProfile;
      Position = "8 6";
      Extent = "236 18";
      Text = "ENEMY FORCES (UNDEAD LEGION)";
   };

   %this.enemyTallyLabel = new GuiControl(BattleSummaryEnemyTally)
   {
      Profile = %this.tallyZeroProfile;
      Position = "8 26";
      Extent = "236 22";
      Text = "0 / 0 Units Surviving";
   };

   %this.enemyDetailLabel = new GuiControl(BattleSummaryEnemyDetail)
   {
      Profile = %this.tallyDetailProfile;
      Position = "8 50";
      Extent = "236 18";
      Text = "Remaining HP: 0  |  Fallen: 0";
   };

   %this.enemyCard.add(%this.enemyHeaderLabel);
   %this.enemyCard.add(%this.enemyTallyLabel);
   %this.enemyCard.add(%this.enemyDetailLabel);
   %this.panelCtrl.add(%this.enemyCard);

   // Surviving Unit Tally Roster Box
   %this.rosterBox = new GuiControl(BattleSummaryRosterBox)
   {
      Profile = %this.cardProfile;
      Position = "20 196";
      Extent = "520 176";
   };

   %this.rosterHeaderLabel = new GuiControl(BattleSummaryRosterHeader)
   {
      Profile = %this.rosterHeaderProfile;
      Position = "12 8";
      Extent = "496 20";
      Text = "SURVIVING UNIT TALLY";
   };
   %this.rosterBox.add(%this.rosterHeaderLabel);

   for (%r = 0; %r < 6; %r++)
   {
      %ry = 32 + (%r * 22);
      %this.unitRowLabel[%r] = new GuiControl("BattleSummaryUnitRow" @ %r)
      {
         Profile = %this.rosterRowProfile;
         Position = "20" SPC %ry;
         Extent = "480 20";
         Text = "";
      };
      %this.rosterBox.add(%this.unitRowLabel[%r]);
   }
   %this.panelCtrl.add(%this.rosterBox);

   // Bottom Action Buttons
   %this.btnPlayAgain = new GuiButtonCtrl(BattleSummaryPlayAgainBtn)
   {
      Profile = %this.btnPrimaryProfile;
      Position = "20 384";
      Extent = "252 48";
      Text = "PLAY AGAIN";
      Command = "$BattleSummaryGui.onPlayAgainClicked();";
   };
   %this.panelCtrl.add(%this.btnPlayAgain);

   %this.btnMainMenu = new GuiButtonCtrl(BattleSummaryMainMenuBtn)
   {
      Profile = %this.btnSecondaryProfile;
      Position = "288 384";
      Extent = "252 48";
      Text = "RETURN TO MAIN MENU";
      Command = "$BattleSummaryGui.onMainMenuClicked();";
   };
   %this.panelCtrl.add(%this.btnMainMenu);

   %this.backdropCtrl.add(%this.panelCtrl);

   if (isObject(mainScreenGui))
   {
      mainScreenGui.add(%this.backdropCtrl);
      mainScreenGui.pushToBack(%this.backdropCtrl);
   }
}

function BattleSummaryGui::getUnitDisplayName(%this, %piece)
{
   if (!isObject(%piece))
      return isObject($LocalizationManager) ? $LocalizationManager.getText("unit_fallback") : "Unit";

   if (isObject($LocalizationManager))
      return $LocalizationManager.getUnitName(%piece.creatureID, %piece.name);

   if (%piece.creatureID !$= "")
   {
      switch (%piece.creatureID)
      {
         case 0:  return "Warrior";
         case 1:  return "Archer";
         case 2:  return "Cleric";
         case 3:  return "Paladin";
         case 4:  return "Skeleton Warrior";
         case 5:  return "Skeleton Archer";
         case 6:  return "Dread Zombie";
         case 7:  return "Ancient Ghost";
         case 8:  return "Dragon Folk";
         case 9:  return "Red Dragon";
         case 10: return "Air Dragon";
         case 11: return "Gold Dragon";
         case 12: return "Gremlin";
         case 13: return "Horned One-Eye";
         case 14: return "Mistress";
         case 15: return "Demon";
      }
   }

   %rawName = strlwr(trim(%piece.name));
   switch$ (%rawName)
   {
      case "warrior":         return "Warrior";
      case "archer":          return "Archer";
      case "cleric":          return "Cleric";
      case "paladin":         return "Paladin";
      case "skeletonwarrior": return "Skeleton Warrior";
      case "skeletonarcher":  return "Skeleton Archer";
      case "dreadzombie":     return "Dread Zombie";
      case "ancientghost":    return "Ancient Ghost";
      case "dragonfolk":      return "Dragon Folk";
      case "reddragon":       return "Red Dragon";
      case "airdragon":       return "Air Dragon";
      case "golddragon":      return "Gold Dragon";
      case "gremlin":         return "Gremlin";
      case "hornedoneye":     return "Horned One-Eye";
      case "mistress":        return "Mistress";
      case "demon":           return "Demon";
   }

   if (%piece.name !$= "")
      return %piece.name;

   return "Unit";
}

function BattleSummaryGui::formatSurvivingUnitRow(%this, %piece)
{
   if (!isObject(%piece))
      return "";

   %uName = %this.getUnitDisplayName(%piece);
   %lvl = (%piece.level !$= "") ? %piece.level : 1;
   %hp = (%piece.hp !$= "") ? %piece.hp : 0;
   %hpMax = (%piece.hpMAX !$= "") ? %piece.hpMAX : %hp;
   %atk = (%piece.attack !$= "") ? %piece.attack : 0;
   %def = (%piece.defence !$= "") ? %piece.defence : 0;

   if (isObject($LocalizationManager))
      return $LocalizationManager.getText("summary_unit_row", %uName, %lvl, %hp @ "/" @ %hpMax, %atk, %def);

   return "• " @ %uName @ " (Lvl " @ %lvl @ ") — HP: " @ %hp @ "/" @ %hpMax @ "  |  ATK: " @ %atk @ "  |  DEF: " @ %def;
}

function BattleSummaryGui::populateFromBattle(%this)
{
   if (!isObject(%this.backdropCtrl))
      %this.createControls();

   if (isObject(TurnManager))
   {
      TurnManager.calculateBattleSummary();
      %this.outcome = (TurnManager.lastBattleOutcome !$= "") ? TurnManager.lastBattleOutcome : "VICTORY";
      %this.isVictory = (%this.outcome $= "VICTORY");
      %this.turns = (TurnManager.lastBattleTurns !$= "") ? TurnManager.lastBattleTurns : 0;
      %this.playerSurviving = (TurnManager.lastPlayerSurviving !$= "") ? TurnManager.lastPlayerSurviving : 0;
      %this.playerTotal = (TurnManager.lastPlayerTotal !$= "") ? TurnManager.lastPlayerTotal : 0;
      %this.playerCasualties = (TurnManager.lastPlayerCasualties !$= "") ? TurnManager.lastPlayerCasualties : 0;
      %this.playerSurvivingHP = (TurnManager.lastPlayerSurvivingHP !$= "") ? TurnManager.lastPlayerSurvivingHP : 0;
      %this.enemySurviving = (TurnManager.lastEnemySurviving !$= "") ? TurnManager.lastEnemySurviving : 0;
      %this.enemyTotal = (TurnManager.lastEnemyTotal !$= "") ? TurnManager.lastEnemyTotal : 0;
      %this.enemyCasualties = (TurnManager.lastEnemyCasualties !$= "") ? TurnManager.lastEnemyCasualties : 0;
      %this.enemySurvivingHP = (TurnManager.lastEnemySurvivingHP !$= "") ? TurnManager.lastEnemySurvivingHP : 0;
      %this.survivingUnitCount = (TurnManager.lastSurvivingUnitCount !$= "") ? TurnManager.lastSurvivingUnitCount : 0;
      %this.enemyFactionName = (TurnManager.lastEnemyFactionName !$= "") ? TurnManager.lastEnemyFactionName : "Undead Legion";
      %this.heroClassName = (TurnManager.lastHeroClassName !$= "") ? TurnManager.lastHeroClassName : "Warrior";
   }

   if (isObject($LocalizationManager))
   {
      %enemyUpperName = isObject($FactionManager)
         ? $LocalizationManager.getText("faction_" @ $FactionManager.getEnemyFaction() @ "_upper")
         : strupr(%this.enemyFactionName);
      %heroKey = (isObject(HeroGroup) && HeroGroup.getCount() > 0 && isObject(HeroGroup.getObject(0)) && HeroGroup.getObject(0).getHeroClass() == 1)
         ? "hero_class_mage"
         : "hero_class_warrior";
      %locHeroName = $LocalizationManager.getText(%heroKey);
      %locEnemyName = isObject($FactionManager) ? $FactionManager.getEnemyFactionName() : %this.enemyFactionName;

      if (isObject(%this.bannerBox))
         %this.bannerBox.setProfile(%this.isVictory ? %this.victoryBannerProfile : %this.defeatBannerProfile);

      if (isObject(%this.bannerTitleLabel))
      {
         %this.bannerTitleLabel.setProfile(%this.isVictory ? %this.victoryTitleProfile : %this.defeatTitleProfile);
         %this.bannerTitleLabel.setText($LocalizationManager.getText(%this.isVictory ? "summary_victory" : "summary_defeat"));
      }

      if (isObject(%this.bannerSubtitleLabel))
      {
         %subText = $LocalizationManager.getText(%this.isVictory ? "summary_sub_victory" : "summary_sub_defeat", %locEnemyName);
         %this.bannerSubtitleLabel.setText(%subText);
      }

      if (isObject(%this.bannerMetaLabel))
      {
         %turnsText = $LocalizationManager.getPluralText("summary_turns", %this.turns);
         %this.bannerMetaLabel.setText($LocalizationManager.getText("summary_meta", %locHeroName, %turnsText));
      }

      if (isObject(%this.playerHeaderLabel))
         %this.playerHeaderLabel.setText($LocalizationManager.getText("summary_player_header", $LocalizationManager.getText("faction_human_order_upper")));

      if (isObject(%this.playerTallyLabel))
      {
         %this.playerTallyLabel.setProfile((%this.playerSurviving > 0) ? %this.tallyValueProfile : %this.tallyZeroProfile);
         %this.playerTallyLabel.setText($LocalizationManager.getPluralText("summary_surviving", %this.playerSurviving, %this.playerTotal));
      }

      if (isObject(%this.playerDetailLabel))
         %this.playerDetailLabel.setText($LocalizationManager.getText("summary_detail", %this.playerSurvivingHP, %this.playerCasualties));

      if (isObject(%this.enemyHeaderLabel))
         %this.enemyHeaderLabel.setText($LocalizationManager.getText("summary_enemy_header", %enemyUpperName));

      if (isObject(%this.enemyTallyLabel))
      {
         %this.enemyTallyLabel.setProfile((%this.enemySurviving > 0) ? %this.tallyValueProfile : %this.tallyZeroProfile);
         %this.enemyTallyLabel.setText($LocalizationManager.getPluralText("summary_surviving", %this.enemySurviving, %this.enemyTotal));
      }

      if (isObject(%this.enemyDetailLabel))
         %this.enemyDetailLabel.setText($LocalizationManager.getText("summary_detail", %this.enemySurvivingHP, %this.enemyCasualties));

      if (isObject(%this.rosterHeaderLabel))
         %this.rosterHeaderLabel.setText($LocalizationManager.getText("summary_roster_header"));
      if (isObject(%this.btnPlayAgain))
         %this.btnPlayAgain.setText($LocalizationManager.getText("summary_play_again"));
      if (isObject(%this.btnMainMenu))
         %this.btnMainMenu.setText($LocalizationManager.getText("summary_main_menu"));

      for (%r = 0; %r < 6; %r++)
      {
         if (!isObject(%this.unitRowLabel[%r]))
            continue;

         if (%this.survivingUnitCount == 0)
            %this.unitRowLabel[%r].setText((%r == 0) ? $LocalizationManager.getText("summary_none_survived") : "");
         else if (%this.survivingUnitCount <= 6)
            %this.unitRowLabel[%r].setText((%r < %this.survivingUnitCount && isObject(TurnManager)) ? %this.formatSurvivingUnitRow(TurnManager.survivingUnit[%r]) : "");
         else
         {
            if (%r < 5 && isObject(TurnManager))
               %this.unitRowLabel[%r].setText(%this.formatSurvivingUnitRow(TurnManager.survivingUnit[%r]));
            else
               %this.unitRowLabel[%r].setText($LocalizationManager.getPluralText("summary_more_units", %this.survivingUnitCount - 5));
         }
      }
      return;
   }

   // Banner styling & text
   if (isObject(%this.bannerBox))
      %this.bannerBox.setProfile(%this.isVictory ? %this.victoryBannerProfile : %this.defeatBannerProfile);

   if (isObject(%this.bannerTitleLabel))
   {
      %this.bannerTitleLabel.setProfile(%this.isVictory ? %this.victoryTitleProfile : %this.defeatTitleProfile);
      %this.bannerTitleLabel.setText(%this.isVictory ? "VICTORY" : "DEFEAT");
   }

   if (isObject(%this.bannerSubtitleLabel))
   {
      %subText = %this.isVictory
         ? ("The " @ %this.enemyFactionName @ " has been defeated!")
         : ("Your forces fell to the " @ %this.enemyFactionName @ ".");
      %this.bannerSubtitleLabel.setText(%subText);
   }

   if (isObject(%this.bannerMetaLabel))
   {
      %this.bannerMetaLabel.setText("Hero: " @ %this.heroClassName @ "  |  Turns Played: " @ %this.turns);
   }

   // Player & Enemy army tally cards
   if (isObject(%this.playerHeaderLabel))
      %this.playerHeaderLabel.setText("PLAYER FORCES (HUMAN ORDER)");

   if (isObject(%this.playerTallyLabel))
   {
      %this.playerTallyLabel.setProfile((%this.playerSurviving > 0) ? %this.tallyValueProfile : %this.tallyZeroProfile);
      %this.playerTallyLabel.setText(%this.playerSurviving @ " / " @ %this.playerTotal @ " Units Surviving");
   }

   if (isObject(%this.playerDetailLabel))
      %this.playerDetailLabel.setText("Remaining HP: " @ %this.playerSurvivingHP @ "  |  Fallen: " @ %this.playerCasualties);

   if (isObject(%this.enemyHeaderLabel))
      %this.enemyHeaderLabel.setText("ENEMY FORCES (" @ strupr(%this.enemyFactionName) @ ")");

   if (isObject(%this.enemyTallyLabel))
   {
      %this.enemyTallyLabel.setProfile((%this.enemySurviving > 0) ? %this.tallyValueProfile : %this.tallyZeroProfile);
      %this.enemyTallyLabel.setText(%this.enemySurviving @ " / " @ %this.enemyTotal @ " Units Surviving");
   }

   if (isObject(%this.enemyDetailLabel))
      %this.enemyDetailLabel.setText("Remaining HP: " @ %this.enemySurvivingHP @ "  |  Fallen: " @ %this.enemyCasualties);

   // Surviving unit roster breakdown (6 visible rows)
   for (%r = 0; %r < 6; %r++)
   {
      if (!isObject(%this.unitRowLabel[%r]))
         continue;

      if (%this.survivingUnitCount == 0)
      {
         %this.unitRowLabel[%r].setText((%r == 0) ? "No units survived the battle." : "");
      }
      else if (%this.survivingUnitCount <= 6)
      {
         if (%r < %this.survivingUnitCount && isObject(TurnManager))
            %this.unitRowLabel[%r].setText(%this.formatSurvivingUnitRow(TurnManager.survivingUnit[%r]));
         else
            %this.unitRowLabel[%r].setText("");
      }
      else
      {
         if (%r < 5 && isObject(TurnManager))
            %this.unitRowLabel[%r].setText(%this.formatSurvivingUnitRow(TurnManager.survivingUnit[%r]));
         else
            %this.unitRowLabel[%r].setText("... and " @ (%this.survivingUnitCount - 5) @ " more surviving units");
      }
   }
}

function BattleSummaryGui::reanchor(%this)
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

   %panelW = 560;
   %panelH = 450;
   %panelX = mFloor((%mainW - %panelW) / 2);
   %panelY = mFloor((%mainH - %panelH) / 2);

   %this.backdropCtrl.Position = "0 0";
   %this.backdropCtrl.Extent = %mainW SPC %mainH;
   %this.panelCtrl.Position = %panelX SPC %panelY;
   %this.panelCtrl.Extent = %panelW SPC %panelH;

   if (%this.backdropCtrl.isVisible() && isObject(mainScreenGui))
      mainScreenGui.pushToBack(%this.backdropCtrl);
}

function BattleSummaryGui::isVisible(%this)
{
   if (!isObject(%this.backdropCtrl))
      return false;
   return %this.backdropCtrl.isVisible();
}

function BattleSummaryGui::show(%this, %visible)
{
   if (!isObject(%this.backdropCtrl))
      %this.createControls();

   if (%visible)
   {
      if (isObject(mainScreenGui) && isObject(%this.backdropCtrl) && !mainScreenGui.isMember(%this.backdropCtrl))
         mainScreenGui.add(%this.backdropCtrl);

      %this.reanchor();
      %this.backdropCtrl.setVisible(true);
      if (isObject(mainScreenGui))
         mainScreenGui.pushToBack(%this.backdropCtrl);
   }
   else
   {
      if (isObject(%this.backdropCtrl))
         %this.backdropCtrl.setVisible(false);
   }
}

function BattleSummaryGui::showSummary(%this)
{
   %this.populateFromBattle();
   %this.show(true);
}

function BattleSummaryGui::refreshLocalization(%this)
{
   if (%this.isVisible() || isObject(TurnManager))
      %this.populateFromBattle();
}

function BattleSummaryGui::onPlayAgainClicked(%this)
{
   playArchonSound(fxSword);
   %this.show(false);
   if (isObject(TurnManager) && isEventPending(TurnManager.endBattleEvent))
   {
      cancel(TurnManager.endBattleEvent);
      TurnManager.endBattleEvent = 0;
   }
   selectHeroScreen();
}

function BattleSummaryGui::onMainMenuClicked(%this)
{
   playArchonSound(fxSword);
   %this.show(false);
   if (isObject(TurnManager) && isEventPending(TurnManager.endBattleEvent))
   {
      cancel(TurnManager.endBattleEvent);
      TurnManager.endBattleEvent = 0;
   }
   mainmenuScreen();
}

function BattleSummaryGui::getOutcome(%this)
{
   return %this.outcome;
}

function BattleSummaryGui::getBannerTitleText(%this)
{
   return isObject(%this.bannerTitleLabel) ? %this.bannerTitleLabel.getText() : "";
}

function BattleSummaryGui::getBannerSubtitleText(%this)
{
   return isObject(%this.bannerSubtitleLabel) ? %this.bannerSubtitleLabel.getText() : "";
}

function BattleSummaryGui::getPlayerTallyText(%this)
{
   return isObject(%this.playerTallyLabel) ? %this.playerTallyLabel.getText() : "";
}

function BattleSummaryGui::getEnemyTallyText(%this)
{
   return isObject(%this.enemyTallyLabel) ? %this.enemyTallyLabel.getText() : "";
}

function BattleSummaryGui::getSurvivingUnitCount(%this)
{
   return %this.survivingUnitCount;
}

function BattleSummaryGui::getSurvivingUnitRowText(%this, %idx)
{
   if (%idx < 0 || %idx >= 6 || !isObject(%this.unitRowLabel[%idx]))
      return "";
   return %this.unitRowLabel[%idx].getText();
}

// Initialize Singleton
if (!isObject(BattleSummaryGui))
{
   new ScriptObject(BattleSummaryGui) { };
}
if (!isObject($BattleSummaryGui))
{
   $BattleSummaryGui = BattleSummaryGui;
}
