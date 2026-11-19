//---------------------------------------------------------------------------------------------
// Archonita - UnitStatsHud.cs
// On-board micro-HP/dice readout badges for all living grid pieces plus rich hover tooltips
// showing current HP, Attack/Defence dice with buff/debuff modifiers, combat role, and status.
// Conforms to TORQUE_SCRIPT.md: single-class ScriptObject, lifecycle in onAdd/onRemove.
//---------------------------------------------------------------------------------------------

function UnitStatsHud::onAdd(%this)
{
   %this.consoleBadgesEnabled = false;
   %this.badgesVisible = false;
   %this.hoveredPiece = "";
   %this.hoveredUnitType = "";
   %this.hoveredWorldPos = "";
   %this.maxTrackedPieceId = 0;
   %this.moveTrackEvent = 0;
   %this.moveTrackInterval = 40;
   %this.moveTrackElapsed = 0;
   %this.moveTrackDuration = 0;

   // Profile handles
   %this.layerProfile = "";
   %this.playerBorderProfile = "";
   %this.enemyBorderProfile = "";
   %this.activeBorderProfile = "";
   %this.sickBorderProfile = "";
   %this.badgePlayerBgProfile = "";
   %this.badgeEnemyBgProfile = "";
   %this.badgeActiveBgProfile = "";
   %this.badgeSickBgProfile = "";
   %this.hpTrackProfile = "";
   %this.hpHighProfile = "";
   %this.hpMidProfile = "";
   %this.hpLowProfile = "";
   %this.badgeTextProfile = "";

   %this.tooltipBorderProfile = "";
   %this.tooltipEnemyBorderProfile = "";
   %this.tooltipPanelProfile = "";
   %this.tooltipEnemyPanelProfile = "";
   %this.tooltipHeaderProfile = "";
   %this.tooltipSubProfile = "";
   %this.tooltipStatProfile = "";
   %this.tooltipBuffProfile = "";
   %this.tooltipDebuffProfile = "";
   %this.tooltipStatusProfile = "";

   // Root GUI handles
   %this.badgeLayer = "";
   %this.tooltipCtrl = "";
   %this.tooltipHeaderLabel = "";
   %this.tooltipTeamLabel = "";
   %this.tooltipHpLabel = "";
   %this.tooltipHpTrack = "";
   %this.tooltipHpFill = "";
   %this.tooltipAttackLabel = "";
   %this.tooltipDefenceLabel = "";
   %this.tooltipStatusLabel = "";

   %this.createProfiles();
   %this.createControls();
}

function UnitStatsHud::onRemove(%this)
{
   %this.stopMovementTracking();
   %this.clearBadges();

   if (isObject(%this.tooltipCtrl))
      %this.tooltipCtrl.delete();
   if (isObject(%this.badgeLayer))
      %this.badgeLayer.delete();

   if (isObject(%this.tooltipStatusProfile))
      %this.tooltipStatusProfile.delete();
   if (isObject(%this.tooltipDebuffProfile))
      %this.tooltipDebuffProfile.delete();
   if (isObject(%this.tooltipBuffProfile))
      %this.tooltipBuffProfile.delete();
   if (isObject(%this.tooltipStatProfile))
      %this.tooltipStatProfile.delete();
   if (isObject(%this.tooltipSubProfile))
      %this.tooltipSubProfile.delete();
   if (isObject(%this.tooltipHeaderProfile))
      %this.tooltipHeaderProfile.delete();
   if (isObject(%this.tooltipEnemyPanelProfile))
      %this.tooltipEnemyPanelProfile.delete();
   if (isObject(%this.tooltipPanelProfile))
      %this.tooltipPanelProfile.delete();
   if (isObject(%this.tooltipEnemyBorderProfile))
      %this.tooltipEnemyBorderProfile.delete();
   if (isObject(%this.tooltipBorderProfile))
      %this.tooltipBorderProfile.delete();

   if (isObject(%this.badgeTextProfile))
      %this.badgeTextProfile.delete();
   if (isObject(%this.hpLowProfile))
      %this.hpLowProfile.delete();
   if (isObject(%this.hpMidProfile))
      %this.hpMidProfile.delete();
   if (isObject(%this.hpHighProfile))
      %this.hpHighProfile.delete();
   if (isObject(%this.hpTrackProfile))
      %this.hpTrackProfile.delete();
   if (isObject(%this.badgeSickBgProfile))
      %this.badgeSickBgProfile.delete();
   if (isObject(%this.badgeActiveBgProfile))
      %this.badgeActiveBgProfile.delete();
   if (isObject(%this.badgeEnemyBgProfile))
      %this.badgeEnemyBgProfile.delete();
   if (isObject(%this.badgePlayerBgProfile))
      %this.badgePlayerBgProfile.delete();
   if (isObject(%this.sickBorderProfile))
      %this.sickBorderProfile.delete();
   if (isObject(%this.activeBorderProfile))
      %this.activeBorderProfile.delete();
   if (isObject(%this.enemyBorderProfile))
      %this.enemyBorderProfile.delete();
   if (isObject(%this.playerBorderProfile))
      %this.playerBorderProfile.delete();
   if (isObject(%this.layerProfile))
      %this.layerProfile.delete();
}

function UnitStatsHud::createProfiles(%this)
{
   if (!isObject(%this.layerProfile))
   {
      %this.layerProfile = new GuiControlProfile(UnitStatsLayerProfile : GuiDefaultProfile)
      {
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.playerBorderProfile))
   {
      %this.playerBorderProfile = new GuiBorderProfile(UnitStatsPlayerBorderProfile)
      {
         border = 1;
         borderHL = 1;
         borderSL = 1;
         borderNA = 1;
         borderColor = "95 190 255 225";
         borderColorHL = "95 190 255 225";
         borderColorSL = "95 190 255 225";
         borderColorNA = "95 190 255 225";
         underfill = true;
      };
   }

   if (!isObject(%this.enemyBorderProfile))
   {
      %this.enemyBorderProfile = new GuiBorderProfile(UnitStatsEnemyBorderProfile)
      {
         border = 1;
         borderHL = 1;
         borderSL = 1;
         borderNA = 1;
         borderColor = "235 95 95 225";
         borderColorHL = "235 95 95 225";
         borderColorSL = "235 95 95 225";
         borderColorNA = "235 95 95 225";
         underfill = true;
      };
   }

   if (!isObject(%this.activeBorderProfile))
   {
      %this.activeBorderProfile = new GuiBorderProfile(UnitStatsActiveBorderProfile)
      {
         border = 2;
         borderHL = 2;
         borderSL = 2;
         borderNA = 2;
         borderColor = "255 220 90 255";
         borderColorHL = "255 220 90 255";
         borderColorSL = "255 220 90 255";
         borderColorNA = "255 220 90 255";
         underfill = true;
      };
   }

   if (!isObject(%this.sickBorderProfile))
   {
      %this.sickBorderProfile = new GuiBorderProfile(UnitStatsSickBorderProfile)
      {
         border = 2;
         borderHL = 2;
         borderSL = 2;
         borderNA = 2;
         borderColor = "155 225 90 245";
         borderColorHL = "155 225 90 245";
         borderColorSL = "155 225 90 245";
         borderColorNA = "155 225 90 245";
         underfill = true;
      };
   }

   if (!isObject(%this.badgePlayerBgProfile))
   {
      %this.badgePlayerBgProfile = new GuiControlProfile(UnitStatsBadgePlayerBgProfile : GuiDefaultProfile)
      {
         fillColor = "10 18 30 218";
         borderLeft = %this.playerBorderProfile;
         borderRight = %this.playerBorderProfile;
         borderTop = %this.playerBorderProfile;
         borderBottom = %this.playerBorderProfile;
         useInput = false;
      };
   }

   if (!isObject(%this.badgeEnemyBgProfile))
   {
      %this.badgeEnemyBgProfile = new GuiControlProfile(UnitStatsBadgeEnemyBgProfile : GuiDefaultProfile)
      {
         fillColor = "28 12 16 218";
         borderLeft = %this.enemyBorderProfile;
         borderRight = %this.enemyBorderProfile;
         borderTop = %this.enemyBorderProfile;
         borderBottom = %this.enemyBorderProfile;
         useInput = false;
      };
   }

   if (!isObject(%this.badgeActiveBgProfile))
   {
      %this.badgeActiveBgProfile = new GuiControlProfile(UnitStatsBadgeActiveBgProfile : GuiDefaultProfile)
      {
         fillColor = "30 26 12 232";
         borderLeft = %this.activeBorderProfile;
         borderRight = %this.activeBorderProfile;
         borderTop = %this.activeBorderProfile;
         borderBottom = %this.activeBorderProfile;
         useInput = false;
      };
   }

   if (!isObject(%this.badgeSickBgProfile))
   {
      %this.badgeSickBgProfile = new GuiControlProfile(UnitStatsBadgeSickBgProfile : GuiDefaultProfile)
      {
         fillColor = "18 32 14 228";
         borderLeft = %this.sickBorderProfile;
         borderRight = %this.sickBorderProfile;
         borderTop = %this.sickBorderProfile;
         borderBottom = %this.sickBorderProfile;
         useInput = false;
      };
   }

   if (!isObject(%this.hpTrackProfile))
   {
      %this.hpTrackProfile = new GuiControlProfile(UnitStatsHpTrackProfile : GuiDefaultProfile)
      {
         fillColor = "18 20 26 240";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.hpHighProfile))
   {
      %this.hpHighProfile = new GuiControlProfile(UnitStatsHpHighProfile : GuiDefaultProfile)
      {
         fillColor = "76 210 112 255";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.hpMidProfile))
   {
      %this.hpMidProfile = new GuiControlProfile(UnitStatsHpMidProfile : GuiDefaultProfile)
      {
         fillColor = "242 192 58 255";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.hpLowProfile))
   {
      %this.hpLowProfile = new GuiControlProfile(UnitStatsHpLowProfile : GuiDefaultProfile)
      {
         fillColor = "235 74 68 255";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.badgeTextProfile))
   {
      %this.badgeTextProfile = new GuiControlProfile(UnitStatsBadgeTextProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 10;
         fontColor = "245 248 255 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.tooltipBorderProfile))
   {
      %this.tooltipBorderProfile = new GuiBorderProfile(UnitStatsTooltipBorderProfile)
      {
         border = 2;
         borderHL = 2;
         borderSL = 2;
         borderNA = 2;
         borderColor = "210 175 90 240";
         borderColorHL = "210 175 90 240";
         borderColorSL = "210 175 90 240";
         borderColorNA = "210 175 90 240";
         underfill = true;
      };
   }

   if (!isObject(%this.tooltipEnemyBorderProfile))
   {
      %this.tooltipEnemyBorderProfile = new GuiBorderProfile(UnitStatsTooltipEnemyBorderProfile)
      {
         border = 2;
         borderHL = 2;
         borderSL = 2;
         borderNA = 2;
         borderColor = "220 85 85 240";
         borderColorHL = "220 85 85 240";
         borderColorSL = "220 85 85 240";
         borderColorNA = "220 85 85 240";
         underfill = true;
      };
   }

   if (!isObject(%this.tooltipPanelProfile))
   {
      %this.tooltipPanelProfile = new GuiControlProfile(UnitStatsTooltipPanelProfile : GuiDefaultProfile)
      {
         fillColor = "12 16 26 244";
         borderLeft = %this.tooltipBorderProfile;
         borderRight = %this.tooltipBorderProfile;
         borderTop = %this.tooltipBorderProfile;
         borderBottom = %this.tooltipBorderProfile;
         useInput = false;
      };
   }

   if (!isObject(%this.tooltipEnemyPanelProfile))
   {
      %this.tooltipEnemyPanelProfile = new GuiControlProfile(UnitStatsTooltipEnemyPanelProfile : GuiDefaultProfile)
      {
         fillColor = "24 14 18 244";
         borderLeft = %this.tooltipEnemyBorderProfile;
         borderRight = %this.tooltipEnemyBorderProfile;
         borderTop = %this.tooltipEnemyBorderProfile;
         borderBottom = %this.tooltipEnemyBorderProfile;
         useInput = false;
      };
   }

   if (!isObject(%this.tooltipHeaderProfile))
   {
      %this.tooltipHeaderProfile = new GuiControlProfile(UnitStatsTooltipHeaderProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 28;
         fontColor = "255 230 140 255";
         align = "left";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.tooltipSubProfile))
   {
      %this.tooltipSubProfile = new GuiControlProfile(UnitStatsTooltipSubProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 22;
         fontColor = "175 195 225 255";
         align = "left";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.tooltipStatProfile))
   {
      %this.tooltipStatProfile = new GuiControlProfile(UnitStatsTooltipStatProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 24;
         fontColor = "235 242 252 255";
         align = "left";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.tooltipBuffProfile))
   {
      %this.tooltipBuffProfile = new GuiControlProfile(UnitStatsTooltipBuffProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 24;
         fontColor = "120 245 165 255";
         align = "left";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.tooltipDebuffProfile))
   {
      %this.tooltipDebuffProfile = new GuiControlProfile(UnitStatsTooltipDebuffProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 24;
         fontColor = "255 130 120 255";
         align = "left";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.tooltipStatusProfile))
   {
      %this.tooltipStatusProfile = new GuiControlProfile(UnitStatsTooltipStatusProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 22;
         fontColor = "255 215 120 255";
         align = "left";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }
}

function UnitStatsHud::createControls(%this)
{
   if (isObject(%this.tooltipCtrl))
      %this.tooltipCtrl.delete();
   if (isObject(%this.badgeLayer))
      %this.badgeLayer.delete();

   %mainW = 1024;
   %mainH = 768;
   if (isObject(mainScreenGui))
   {
      %mainW = getWord(mainScreenGui.extent, 0);
      %mainH = getWord(mainScreenGui.extent, 1);
   }

   %this.badgeLayer = new GuiControl(UnitStatsBadgeLayer)
   {
      Profile = %this.layerProfile;
      HorizSizing = "width";
      VertSizing = "height";
      Position = "0 0";
      Extent = %mainW SPC %mainH;
      Visible = "0";
      useInput = false;
      isContainer = "1";
   };

   %this.tooltipCtrl = new GuiControl(UnitStatsTooltipPanel)
   {
      Profile = %this.tooltipPanelProfile;
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "16 16";
      Extent = "520 270";
      Visible = "0";
      useInput = false;
      isContainer = "1";
   };

   %this.tooltipHeaderLabel = new GuiControl(UnitStatsTooltipHeader)
   {
      Profile = %this.tooltipHeaderProfile;
      Position = "16 14";
      Extent = "488 38";
      useInput = false;
      Text = "";
   };

   %this.tooltipTeamLabel = new GuiControl(UnitStatsTooltipTeam)
   {
      Profile = %this.tooltipSubProfile;
      Position = "16 54";
      Extent = "488 30";
      useInput = false;
      Text = "";
   };

   %this.tooltipHpLabel = new GuiControl(UnitStatsTooltipHp)
   {
      Profile = %this.tooltipStatProfile;
      Position = "16 90";
      Extent = "488 32";
      useInput = false;
      Text = "";
   };

   %this.tooltipHpTrack = new GuiControl(UnitStatsTooltipHpTrack)
   {
      Profile = %this.hpTrackProfile;
      Position = "16 126";
      Extent = "488 12";
      useInput = false;
      isContainer = "1";
   };

   %this.tooltipHpFill = new GuiControl(UnitStatsTooltipHpFill)
   {
      Profile = %this.hpHighProfile;
      Position = "0 0";
      Extent = "488 12";
      useInput = false;
   };
   %this.tooltipHpTrack.add(%this.tooltipHpFill);

   %this.tooltipAttackLabel = new GuiControl(UnitStatsTooltipAttack)
   {
      Profile = %this.tooltipStatProfile;
      Position = "16 146";
      Extent = "488 34";
      useInput = false;
      Text = "";
   };

   %this.tooltipDefenceLabel = new GuiControl(UnitStatsTooltipDefence)
   {
      Profile = %this.tooltipStatProfile;
      Position = "16 184";
      Extent = "488 34";
      useInput = false;
      Text = "";
   };

   %this.tooltipStatusLabel = new GuiControl(UnitStatsTooltipStatus)
   {
      Profile = %this.tooltipStatusProfile;
      Position = "16 222";
      Extent = "488 32";
      useInput = false;
      Text = "";
   };

   %this.tooltipCtrl.add(%this.tooltipHeaderLabel);
   %this.tooltipCtrl.add(%this.tooltipTeamLabel);
   %this.tooltipCtrl.add(%this.tooltipHpLabel);
   %this.tooltipCtrl.add(%this.tooltipHpTrack);
   %this.tooltipCtrl.add(%this.tooltipAttackLabel);
   %this.tooltipCtrl.add(%this.tooltipDefenceLabel);
   %this.tooltipCtrl.add(%this.tooltipStatusLabel);

   if (isObject(mainScreenGui))
   {
      mainScreenGui.add(%this.badgeLayer);
      mainScreenGui.add(%this.tooltipCtrl);
   }
}

//---------------------------------------------------------------------------------------------
// State & Coordinate Helpers
//---------------------------------------------------------------------------------------------
function UnitStatsHud::isBoardInteractiveState(%this)
{
   if (!isObject(game))
      return false;

   %state = game.getState();
   if (%state $= "")
      return false;

   if ((%state == $gamePreBattleState && $gamePreBattleState !$= "") ||
       (%state == $gamePreBattleTimerState && $gamePreBattleTimerState !$= "") ||
       (%state == $gameTimerBattleState && $gameTimerBattleState !$= "") ||
       (%state == $gameBattleState && $gameBattleState !$= ""))
   {
      return true;
   }
   return false;
}

function UnitStatsHud::isModalBlocking(%this)
{
   if ((isObject($PauseMenuManager) && $PauseMenuManager.isPaused) ||
       (isObject($OptionsMenuGui) && $OptionsMenuGui.isVisible()) ||
       (isObject($FactionSelectGui) && $FactionSelectGui.isVisible()) ||
       (isObject($BattleSummaryGui) && $BattleSummaryGui.isVisible()))
   {
      return true;
   }
   return false;
}

function UnitStatsHud::isAnyCharacterBusy(%this)
{
   if (isObject(CombatEngine))
   {
      if (CombatEngine.isInputLocked())
         return true;
      if (isEventPending(CombatEngine.diceRollEvent) ||
          isEventPending(CombatEngine.diceStopEvent) ||
          isEventPending(CombatEngine.rangedLaunchEvent) ||
          isEventPending(CombatEngine.rangedImpactTimeoutEvent))
      {
         return true;
      }
      if (isObject(CombatEngine.activeProjectiles) && CombatEngine.activeProjectiles.getCount() > 0)
         return true;
      if (isObject(CombatEngine.activeCombatProjectile))
         return true;
   }

   if (isObject(game))
   {
      %pieceCount = game.getPieceCount();
      for (%i = 1; %i <= %pieceCount; %i++)
      {
         %piece = $nappulaSprite[%i];
         if (!isObject(%piece) || %piece.alive != 1)
            continue;

         if (isObject(%piece.state) && %piece.state.name $= "walking")
            return true;

         %vel = %piece.getLinearVelocity();
         if (mAbs(getWord(%vel, 0)) > 0.01 || mAbs(getWord(%vel, 1)) > 0.01)
            return true;
      }
   }

   return false;
}

function UnitStatsHud::worldToScreenPoint(%this, %wx, %wy)
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

   %camW = isObject($ResolutionManager) ? $ResolutionManager.getCameraWidth() : 100.0;
   %camH = (isObject($ResolutionManager) && $ResolutionManager.baseCameraHeight > 0) ? $ResolutionManager.baseCameraHeight : 75.0;
   if (%camW <= 0) %camW = 100.0;

   %sx = (%winW * 0.5) + (%wx * (%winW / %camW));
   %sy = (%winH * 0.5) - (%wy * (%winH / %camH));
   return mFloor(%sx) SPC mFloor(%sy);
}

//---------------------------------------------------------------------------------------------
// Unit Stat Lookup & Formatting Helpers
//---------------------------------------------------------------------------------------------
function UnitStatsHud::getUnitDisplayNameById(%this, %creatureId, %fallbackName)
{
   if (isObject($LocalizationManager))
      return $LocalizationManager.getUnitName(%creatureId, %fallbackName);

   if (%creatureId !$= "")
   {
      switch (%creatureId)
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

   %raw = strlwr(trim(%fallbackName));
   switch$ (%raw)
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

   if (%fallbackName !$= "")
      return %fallbackName;
   return "Unit";
}

function UnitStatsHud::getUnitDisplayName(%this, %piece)
{
   if (!isObject(%piece))
      return "Unit";
   return %this.getUnitDisplayNameById(%piece.creatureID, %piece.name);
}

function UnitStatsHud::getBaseAttack(%this, %piece)
{
   if (!isObject(%piece))
      return 0;

   if (isObject(UnitProperties) && %piece.creatureID !$= "" && UnitProperties.attack[%piece.creatureID] !$= "")
      return UnitProperties.attack[%piece.creatureID];
   if (%piece.attackMAX !$= "")
      return %piece.attackMAX;
   return (%piece.attack !$= "") ? %piece.attack : 0;
}

function UnitStatsHud::getBaseDefence(%this, %piece)
{
   if (!isObject(%piece))
      return 0;

   if (isObject(UnitProperties) && %piece.creatureID !$= "" && UnitProperties.defence[%piece.creatureID] !$= "")
      return UnitProperties.defence[%piece.creatureID];
   if (%piece.defenceMAX !$= "")
      return %piece.defenceMAX;
   return (%piece.defence !$= "") ? %piece.defence : 0;
}

function UnitStatsHud::formatDiceStat(%this, %label, %current, %base)
{
   %cur = (%current !$= "") ? %current : 0;
   %b = (%base !$= "") ? %base : %cur;
   %delta = %cur - %b;

   if (isObject($LocalizationManager))
   {
      if (%delta > 0)
         return $LocalizationManager.getPluralText("dice_stat_mod", %cur, "+" @ %delta) !$= ""
            ? %this.formatLocalizedDiceStat(%label, %cur, "+" @ %delta)
            : %label @ ": " @ %cur @ " (+" @ %delta @ ")";
      if (%delta < 0)
         return %this.formatLocalizedDiceStat(%label, %cur, %delta);
      return %this.formatLocalizedDiceStat(%label, %cur, "");
   }

   %unitWord = (%cur == 1) ? "die" : "dice";

   if (%delta > 0)
      return %label @ ": " @ %cur @ " " @ %unitWord @ " (+" @ %delta @ ")";
   if (%delta < 0)
      return %label @ ": " @ %cur @ " " @ %unitWord @ " (" @ %delta @ ")";
   return %label @ ": " @ %cur @ " " @ %unitWord;
}

function UnitStatsHud::formatLocalizedDiceStat(%this, %label, %cur, %deltaStr)
{
   %lang = $LocalizationManager.getLanguage();
   %isSing = $LocalizationManager.isSingularCount(%lang, %cur);
   if (%deltaStr !$= "")
   {
      %key = %isSing ? "dice_stat_mod_one" : "dice_stat_mod_other";
      return $LocalizationManager.getText(%key, %label, %cur, %deltaStr);
   }
   %key = %isSing ? "dice_stat_one" : "dice_stat_other";
   return $LocalizationManager.getText(%key, %label, %cur);
}

function UnitStatsHud::getHpFillProfile(%this, %hp, %hpMax)
{
   if (%hpMax <= 0)
      return %this.hpLowProfile;
   %ratio = %hp / %hpMax;
   if (%ratio >= 0.67)
      return %this.hpHighProfile;
   if (%ratio > 0.34)
      return %this.hpMidProfile;
   return %this.hpLowProfile;
}

function UnitStatsHud::getStatProfile(%this, %current, %base)
{
   %delta = %current - %base;
   if (%delta > 0)
      return %this.tooltipBuffProfile;
   if (%delta < 0)
      return %this.tooltipDebuffProfile;
   return %this.tooltipStatProfile;
}

//---------------------------------------------------------------------------------------------
// On-Board Micro-HP & Dice Readout Badges
//---------------------------------------------------------------------------------------------
function UnitStatsHud::ensureBadgeForPiece(%this, %pieceId)
{
   if (!isObject(%this.badgeLayer))
      %this.createControls();

   if (isObject(%this.badgeCtrl[%pieceId]))
      return %this.badgeCtrl[%pieceId];

   %badge = new GuiControl()
   {
      Profile = %this.badgePlayerBgProfile;
      HorizSizing = "right";
      VertSizing = "bottom";
      Position = "0 0";
      Extent = "76 22";
      Visible = "0";
      useInput = false;
      isContainer = "1";
   };

   %hpTrack = new GuiControl()
   {
      Profile = %this.hpTrackProfile;
      Position = "3 3";
      Extent = "70 4";
      useInput = false;
      isContainer = "1";
   };

   %hpFill = new GuiControl()
   {
      Profile = %this.hpHighProfile;
      Position = "0 0";
      Extent = "70 4";
      useInput = false;
   };
   %hpTrack.add(%hpFill);

   %statLabel = new GuiControl()
   {
      Profile = %this.badgeTextProfile;
      Position = "2 8";
      Extent = "72 13";
      useInput = false;
      Text = "";
   };

   %badge.add(%hpTrack);
   %badge.add(%statLabel);
   %badge.hpTrack = %hpTrack;
   %badge.hpFill = %hpFill;
   %badge.statLabel = %statLabel;

   %this.badgeLayer.add(%badge);
   %this.badgeCtrl[%pieceId] = %badge;
   if (%pieceId > %this.maxTrackedPieceId)
      %this.maxTrackedPieceId = %pieceId;

   return %badge;
}

function UnitStatsHud::updatePieceBadge(%this, %pieceId)
{
   %piece = $nappulaSprite[%pieceId];
   if (!isObject(%piece) || %piece.alive != 1 || %piece.hp <= 0)
   {
      if (isObject(%this.badgeCtrl[%pieceId]))
         %this.badgeCtrl[%pieceId].setVisible(false);
      return;
   }

   %badge = %this.ensureBadgeForPiece(%pieceId);
   if (!isObject(%badge))
      return;

   %hp = %piece.hp;
   %hpMax = (%piece.hpMAX !$= "" && %piece.hpMAX > 0) ? %piece.hpMAX : %hp;
   if (%hpMax < %hp)
      %hpMax = %hp;

   %atk = (%piece.attack !$= "") ? %piece.attack : 0;
   %def = (%piece.defence !$= "") ? %piece.defence : 0;
   %baseAtk = %this.getBaseAttack(%piece);
   %baseDef = %this.getBaseDefence(%piece);

   %atkDelta = %atk - %baseAtk;
   %defDelta = %def - %baseDef;
   %atkPrefix = isObject($LocalizationManager) ? $LocalizationManager.getText("badge_atk_prefix") : "A";
   %defPrefix = isObject($LocalizationManager) ? $LocalizationManager.getText("badge_def_prefix") : "D";
   %skipTag = isObject($LocalizationManager) ? $LocalizationManager.getText("badge_skip") : "SKIP";
   %atkTag = %atkPrefix @ %atk @ ((%atkDelta > 0) ? "+" : ((%atkDelta < 0) ? "-" : ""));
   %defTag = %defPrefix @ %def @ ((%defDelta > 0) ? "+" : ((%defDelta < 0) ? "-" : ""));

   if (%piece.skipNextTurn == 1)
      %badgeText = %hp @ "/" @ %hpMax @ " " @ %skipTag;
   else
      %badgeText = %hp @ "/" @ %hpMax @ " " @ %atkTag @ " " @ %defTag;

   // Choose border/background style: Sick > Active Turn > Player/Enemy team color
   %isActive = (isObject(game) && game.getState() == $gameBattleState && game.getSelectedPiece() == %pieceId);
   if (%piece.skipNextTurn == 1)
      %badge.setProfile(%this.badgeSickBgProfile);
   else if (%isActive)
      %badge.setProfile(%this.badgeActiveBgProfile);
   else if (%piece.groupid == 0)
      %badge.setProfile(%this.badgePlayerBgProfile);
   else
      %badge.setProfile(%this.badgeEnemyBgProfile);

   %badgeScale = isObject($ResolutionManager) ? $ResolutionManager.getUiScaleFactor() : 1.0;
   if (%badgeScale < 1.0) %badgeScale = 1.0;
   if (%badgeScale > 2.0) %badgeScale = 2.0;
   if (isObject($ResolutionManager))
      $ResolutionManager.applyUiScaleToTree(%badge, %badgeScale, false);

   // Update micro HP bar
   %trackExt = isObject(%badge.hpTrack) ? %badge.hpTrack.extent : "70 4";
   %trackW = getWord(%trackExt, 0);
   %trackH = getWord(%trackExt, 1);
   if (%trackW <= 0) %trackW = 70;
   if (%trackH <= 0) %trackH = 4;
   %fillW = (%hpMax > 0) ? mFloor((%hp / %hpMax) * %trackW) : 0;
   if (%hp > 0 && %fillW < 4)
      %fillW = 4;
   if (%fillW > %trackW)
      %fillW = %trackW;

   if (isObject(%badge.hpFill))
   {
      %badge.hpFill.setProfile(%this.getHpFillProfile(%hp, %hpMax));
      %badge.hpFill.Extent = %fillW SPC %trackH;
   }

   if (isObject(%badge.statLabel))
      %badge.statLabel.setText(%badgeText);

   // Anchor micro-readout neatly at the base of the piece's tile
   %pos = %piece.getPosition();
   %wx = getWord(%pos, 0);
   %wy = getWord(%pos, 1) - 4.2;
   %screenPt = %this.worldToScreenPoint(%wx, %wy);
   %sx = getWord(%screenPt, 0);
   %sy = getWord(%screenPt, 1);

   %badgeExt = %badge.extent;
   %badgeW = getWord(%badgeExt, 0);
   %badgeH = getWord(%badgeExt, 1);
   if (%badgeW <= 0) %badgeW = 76;
   if (%badgeH <= 0) %badgeH = 22;
   %bx = mFloor(%sx - (%badgeW * 0.5));
   %by = mFloor(%sy - (%badgeH * 0.5));

   if (isObject(mainScreenGui))
   {
      %mainW = getWord(mainScreenGui.extent, 0);
      %mainH = getWord(mainScreenGui.extent, 1);
      if (%bx < 2) %bx = 2;
      if (%by < 2) %by = 2;
      if (%bx + %badgeW > %mainW - 2) %bx = %mainW - %badgeW - 2;
      if (%by + %badgeH > %mainH - 2) %by = %mainH - %badgeH - 2;
   }

   %badge.Position = %bx SPC %by;
   %badge.setVisible(true);
}

function UnitStatsHud::refreshAll(%this)
{
   if (!isObject(%this.badgeLayer))
      %this.createControls();

   if (!%this.isBoardInteractiveState() || %this.isModalBlocking())
   {
      if (isObject(%this.badgeLayer))
         %this.badgeLayer.setVisible(false);
      %this.hideTooltip();
      return;
   }

   if (%this.consoleBadgesEnabled && %this.badgesVisible)
   {
      if (isObject(mainScreenGui) && !mainScreenGui.isMember(%this.badgeLayer))
         mainScreenGui.add(%this.badgeLayer);

      %pieceCount = isObject(game) ? game.getPieceCount() : 0;
      %upper = (%pieceCount > %this.maxTrackedPieceId) ? %pieceCount : %this.maxTrackedPieceId;

      for (%i = 1; %i <= %upper; %i++)
      {
         if (%i <= %pieceCount)
         {
            %this.updatePieceBadge(%i);
         }
         else if (isObject(%this.badgeCtrl[%i]))
         {
            %this.badgeCtrl[%i].setVisible(false);
         }
      }

      %this.badgeLayer.setVisible(true);
   }
   else
   {
      if (isObject(%this.badgeLayer))
         %this.badgeLayer.setVisible(false);
   }

   // Keep hover tooltip live if hovered piece took damage or received a spell modifier,
   // but hide it immediately if any character is moving or doing an action
   if (%this.isAnyCharacterBusy())
   {
      %this.hideTooltip();
   }
   else if (%this.hoveredPiece !$= "")
   {
      if (isObject(%this.hoveredPiece) && %this.hoveredPiece.alive == 1 && %this.hoveredPiece.hp > 0)
         %this.showTooltipForPiece(%this.hoveredPiece);
      else
         %this.hideTooltip();
   }
}

function UnitStatsHud::clearBadges(%this)
{
   %this.stopMovementTracking();
   %this.badgesVisible = false;
   %this.hideTooltip();

   for (%i = 1; %i <= %this.maxTrackedPieceId; %i++)
   {
      if (isObject(%this.badgeCtrl[%i]))
      {
         %this.badgeCtrl[%i].delete();
         %this.badgeCtrl[%i] = "";
      }
   }
   %this.maxTrackedPieceId = 0;

   if (isObject(%this.badgeLayer))
      %this.badgeLayer.setVisible(false);
}

function UnitStatsHud::showBadges(%this, %visible)
{
   %this.badgesVisible = (%visible == 1 || %visible $= "true") ? true : false;
   if (!%this.badgesVisible)
   {
      %this.stopMovementTracking();
      %this.hideTooltip();
      if (isObject(%this.badgeLayer))
         %this.badgeLayer.setVisible(false);
      return;
   }

   %this.reanchor();
   %this.refreshAll();
}

function UnitStatsHud::setConsoleBadgesEnabled(%this, %enabled)
{
   %this.consoleBadgesEnabled = (%enabled == 1 || %enabled $= "true") ? true : false;
   echo("UnitStatsHud: On-board micro-readout badges " @ (%this.consoleBadgesEnabled ? "ENABLED" : "DISABLED"));
   %this.reanchor();
   %this.refreshAll();
   return %this.consoleBadgesEnabled;
}

function UnitStatsHud::areConsoleBadgesEnabled(%this)
{
   return (%this.consoleBadgesEnabled == 1 || %this.consoleBadgesEnabled $= "true");
}

function UnitStatsHud::toggleConsoleBadges(%this)
{
   return %this.setConsoleBadgesEnabled(!%this.areConsoleBadgesEnabled());
}

//---------------------------------------------------------------------------------------------
// Smooth Movement Tracking Loop (Self-Stopping & Cancelled in onRemove)
//---------------------------------------------------------------------------------------------
function UnitStatsHud::startMovementTracking(%this, %durationMs)
{
   if (%durationMs $= "" || %durationMs <= 0)
      %durationMs = 1100;

   %this.moveTrackElapsed = 0;
   %this.moveTrackDuration = %durationMs;
   %this.refreshAll();

   if (!isEventPending(%this.moveTrackEvent))
      %this.moveTrackEvent = %this.schedule(%this.moveTrackInterval, "onMoveTrackTick");
}

function UnitStatsHud::stopMovementTracking(%this)
{
   if (isEventPending(%this.moveTrackEvent))
   {
      cancel(%this.moveTrackEvent);
      %this.moveTrackEvent = 0;
   }
   %this.moveTrackElapsed = 0;
   %this.moveTrackDuration = 0;
}

function UnitStatsHud::onMoveTrackTick(%this)
{
   %this.moveTrackEvent = 0;
   if (!%this.badgesVisible || !%this.isBoardInteractiveState())
      return;

   %this.refreshAll();
   %this.moveTrackElapsed += %this.moveTrackInterval;
   if (%this.moveTrackElapsed < %this.moveTrackDuration)
      %this.moveTrackEvent = %this.schedule(%this.moveTrackInterval, "onMoveTrackTick");
}

//---------------------------------------------------------------------------------------------
// Hover Detection & Rich Stat Tooltip
//---------------------------------------------------------------------------------------------
function UnitStatsHud::findPieceAtWorldPos(%this, %worldPos)
{
   if (!isObject(game) || %worldPos $= "")
      return "";

   %wx = getWord(%worldPos, 0);
   %wy = getWord(%worldPos, 1);
   %pieceCount = game.getPieceCount();
   %bestPiece = "";
   %bestDist = 999999;

   for (%i = 1; %i <= %pieceCount; %i++)
   {
      %piece = $nappulaSprite[%i];
      if (!isObject(%piece) || %piece.alive != 1 || %piece.hp <= 0)
         continue;

      %pos = %piece.getPosition();
      %px = getWord(%pos, 0);
      %py = getWord(%pos, 1);
      %size = %piece.getSize();
      %halfW = (getWord(%size, 0) > 0) ? (getWord(%size, 0) * 0.5) : 4.0;
      %halfH = (getWord(%size, 1) > 0) ? (getWord(%size, 1) * 0.5) : 4.0;

      // Cover both the sprite quad and the lower portion of its occupied grid tile
      if (mAbs(%wx - %px) <= %halfW && %wy >= (%py - %halfH - 2.0) && %wy <= (%py + %halfH))
      {
         %dist = t2dVectorDistance(%worldPos, %pos);
         if (%dist < %bestDist)
         {
            %bestDist = %dist;
            %bestPiece = %piece;
         }
      }
   }

   return %bestPiece;
}

function UnitStatsHud::findSetupIconAtWorldPos(%this, %worldPos)
{
   if (%worldPos $= "")
      return "";

   %wx = getWord(%worldPos, 0);
   %wy = getWord(%worldPos, 1);

   for (%i = 0; %i < 12; %i++)
   {
      %icon = $icon[%i];
      if (!isObject(%icon) || !%icon.getVisible())
         continue;

      %pos = %icon.getPosition();
      %ix = getWord(%pos, 0);
      %iy = getWord(%pos, 1);
      if (mAbs(%wx - %ix) <= 4.0 && mAbs(%wy - %iy) <= 4.0)
         return %icon;
   }

   return "";
}

function UnitStatsHud::updateHover(%this, %worldPos)
{
   if (!%this.isBoardInteractiveState() || %this.isModalBlocking() || $mouseObj::isObjMounted || %this.isAnyCharacterBusy())
   {
      %this.hideTooltip();
      return;
   }

   %piece = %this.findPieceAtWorldPos(%worldPos);
   if (isObject(%piece))
   {
      %this.hoveredPiece = %piece;
      %this.hoveredUnitType = "";
      %this.showTooltipForPiece(%piece);
      return;
   }

   if (isObject(game) && game.getState() == $gamePreBattleState)
   {
      %icon = %this.findSetupIconAtWorldPos(%worldPos);
      if (isObject(%icon) && %icon.creatureLevel > 0)
      {
         %creatureId = %icon.creatureLevel - 1;
         %this.hoveredPiece = "";
         %this.hoveredUnitType = %creatureId;
         %this.hoveredWorldPos = %icon.getPosition();
         %this.showTooltipForUnitType(%creatureId, %this.hoveredWorldPos);
         return;
      }
   }

   %this.hideTooltip();
}

function UnitStatsHud::positionTooltipNearWorld(%this, %wx, %wy)
{
   if (!isObject(%this.tooltipCtrl))
      return;

   %screenPt = %this.worldToScreenPoint(%wx, %wy);
   %sx = getWord(%screenPt, 0);
   %sy = getWord(%screenPt, 1);

   %tipW = getWord(%this.tooltipCtrl.extent, 0);
   %tipH = getWord(%this.tooltipCtrl.extent, 1);
   %mainW = 1024;
   %mainH = 768;
   if (isObject(mainScreenGui))
   {
      %mainW = getWord(mainScreenGui.extent, 0);
      %mainH = getWord(mainScreenGui.extent, 1);
   }

   // Place tooltip to the right of the hovered unit; flip left if near right edge
   %tx = mFloor(%sx + 46);
   %ty = mFloor(%sy - (%tipH * 0.5));
   if (%tx + %tipW > %mainW - 12)
      %tx = mFloor(%sx - %tipW - 46);

   %minY = 52;
   if (isObject(BoardAudioHudContainer))
   {
      %hudBottom = getWord(BoardAudioHudContainer.Position, 1) + getWord(BoardAudioHudContainer.Extent, 1) + 8;
      if (%hudBottom > %minY)
         %minY = %hudBottom;
   }

   if (%tx < 12) %tx = 12;
   if (%ty < %minY) %ty = %minY; // Stay clear of top-right BoardAudioHud bar
   if (%ty + %tipH > %mainH - 12)
      %ty = %mainH - %tipH - 12;

   %this.tooltipCtrl.Position = %tx SPC %ty;
}

function UnitStatsHud::bringTooltipAboveBoard(%this)
{
   if (!isObject(mainScreenGui) || !isObject(%this.tooltipCtrl))
      return;

   if (!mainScreenGui.isMember(%this.tooltipCtrl))
      mainScreenGui.add(%this.tooltipCtrl);

   mainScreenGui.pushToBack(%this.tooltipCtrl);

   // Keep BoardAudioHud interactive buttons above passive hover tooltips
   if (isObject(BoardAudioHudContainer) && BoardAudioHudContainer.isVisible())
      mainScreenGui.pushToBack(BoardAudioHudContainer);
}

function UnitStatsHud::showTooltipForPiece(%this, %piece)
{
   if (!isObject(%piece) || %piece.alive != 1 || %piece.hp <= 0 || %this.isModalBlocking() || %this.isAnyCharacterBusy())
   {
      %this.hideTooltip();
      return;
   }

   if (!isObject(%this.tooltipCtrl))
      %this.createControls();

   %uName = %this.getUnitDisplayName(%piece);
   %lvl = (%piece.level !$= "") ? %piece.level : 1;
   %hp = %piece.hp;
   %hpMax = (%piece.hpMAX !$= "" && %piece.hpMAX > 0) ? %piece.hpMAX : %hp;
   if (%hpMax < %hp)
      %hpMax = %hp;

   %atk = (%piece.attack !$= "") ? %piece.attack : 0;
   %def = (%piece.defence !$= "") ? %piece.defence : 0;
   %baseAtk = %this.getBaseAttack(%piece);
   %baseDef = %this.getBaseDefence(%piece);

   %isEnemy = (%piece.groupid == 1);
   %humanFaction = isObject($LocalizationManager) ? $LocalizationManager.getText("faction_human_order") : "Human Order";
   %factionLabel = %isEnemy
      ? (isObject($FactionManager) ? $FactionManager.getEnemyFactionName() : "Undead Legion")
      : %humanFaction;

   if (isObject($LocalizationManager))
   {
      %roleLabel = (%piece.missile == 1) ? $LocalizationManager.getText("hud_role_ranged") : $LocalizationManager.getText("hud_role_melee");

      %statusText = "";
      if (%piece.skipNextTurn == 1)
         %statusText = $LocalizationManager.getText("status_sick");
      else if (isObject(game) && game.getState() == $gameBattleState && game.getSelectedPiece() == %piece.index)
      {
         if (%piece.hasMoved == 0 && %piece.hasDoneAction == 0)
            %statusText = $LocalizationManager.getText("status_turn_both");
         else if (%piece.hasMoved == 1 && %piece.hasDoneAction == 0)
            %statusText = $LocalizationManager.getText("status_turn_action");
         else if (%piece.hasMoved == 0 && %piece.hasDoneAction == 1)
            %statusText = $LocalizationManager.getText("status_turn_move");
         else
            %statusText = $LocalizationManager.getText("status_turn_finishing");
      }
      else if (%piece.spells $= "1")
         %statusText = $LocalizationManager.getText("trait_ghostly_touch");
      else if (%piece.spells $= "2")
         %statusText = $LocalizationManager.getText("trait_disease");
      else if (%piece.mana > 0)
         %statusText = $LocalizationManager.getText("status_mana", %piece.mana);
      else
         %statusText = $LocalizationManager.getText(%isEnemy ? "status_enemy_unit" : "status_allied_unit");

      %this.tooltipCtrl.setProfile(%isEnemy ? %this.tooltipEnemyPanelProfile : %this.tooltipPanelProfile);
      %this.tooltipHeaderLabel.setText($LocalizationManager.getText("tooltip_header", %uName, %lvl));
      %this.tooltipTeamLabel.setText($LocalizationManager.getText("tooltip_team_role", %factionLabel, %roleLabel));
      %this.tooltipHpLabel.setText($LocalizationManager.getText("tooltip_hp", %hp, %hpMax));

      %trackExt = isObject(%this.tooltipHpTrack) ? %this.tooltipHpTrack.extent : "488 12";
      %trackW = getWord(%trackExt, 0);
      %trackH = getWord(%trackExt, 1);
      if (%trackW <= 0) %trackW = 488;
      if (%trackH <= 0) %trackH = 12;
      %fillW = (%hpMax > 0) ? mFloor((%hp / %hpMax) * %trackW) : 0;
      if (%hp > 0 && %fillW < 6)
         %fillW = 6;
      if (%fillW > %trackW)
         %fillW = %trackW;
      %this.tooltipHpFill.setProfile(%this.getHpFillProfile(%hp, %hpMax));
      %this.tooltipHpFill.Extent = %fillW SPC %trackH;

      %this.tooltipAttackLabel.setProfile(%this.getStatProfile(%atk, %baseAtk));
      %this.tooltipAttackLabel.setText(%this.formatDiceStat($LocalizationManager.getText("stat_attack"), %atk, %baseAtk));

      %this.tooltipDefenceLabel.setProfile(%this.getStatProfile(%def, %baseDef));
      %this.tooltipDefenceLabel.setText(%this.formatDiceStat($LocalizationManager.getText("stat_defence"), %def, %baseDef));

      %this.tooltipStatusLabel.setText(%statusText);

      %pos = %piece.getPosition();
      %this.positionTooltipNearWorld(getWord(%pos, 0), getWord(%pos, 1));
      %this.tooltipCtrl.setVisible(true);
      %this.bringTooltipAboveBoard();
      return;
   }

   %roleLabel = (%piece.missile == 1) ? "Ranged" : "Melee";

   // Build status / turn / trait line
   %statusText = "";
   if (%piece.skipNextTurn == 1)
   {
      %statusText = "Status: Sick (Skips Next Turn)";
   }
   else if (isObject(game) && game.getState() == $gameBattleState && game.getSelectedPiece() == %piece.index)
   {
      if (%piece.hasMoved == 0 && %piece.hasDoneAction == 0)
         %statusText = "Active Turn: Move & Action Ready";
      else if (%piece.hasMoved == 1 && %piece.hasDoneAction == 0)
         %statusText = "Active Turn: Action Ready";
      else if (%piece.hasMoved == 0 && %piece.hasDoneAction == 1)
         %statusText = "Active Turn: Move Ready";
      else
         %statusText = "Active Turn: Finishing";
   }
   else if (%piece.spells $= "1")
   {
      %statusText = "Trait: Ghostly Touch";
   }
   else if (%piece.spells $= "2")
   {
      %statusText = "Trait: Disease";
   }
   else if (%piece.mana > 0)
   {
      %statusText = "Mana: " @ %piece.mana;
   }
   else
   {
      %statusText = %isEnemy ? "Enemy Unit" : "Allied Unit";
   }

   %this.tooltipCtrl.setProfile(%isEnemy ? %this.tooltipEnemyPanelProfile : %this.tooltipPanelProfile);
   %this.tooltipHeaderLabel.setText(%uName @ " (Lvl " @ %lvl @ ")");
   %this.tooltipTeamLabel.setText(%factionLabel @ "  |  " @ %roleLabel);
   %this.tooltipHpLabel.setText("HP: " @ %hp @ " / " @ %hpMax);

   %trackExt = isObject(%this.tooltipHpTrack) ? %this.tooltipHpTrack.extent : "488 12";
   %trackW = getWord(%trackExt, 0);
   %trackH = getWord(%trackExt, 1);
   if (%trackW <= 0) %trackW = 488;
   if (%trackH <= 0) %trackH = 12;
   %fillW = (%hpMax > 0) ? mFloor((%hp / %hpMax) * %trackW) : 0;
   if (%hp > 0 && %fillW < 6)
      %fillW = 6;
   if (%fillW > %trackW)
      %fillW = %trackW;
   %this.tooltipHpFill.setProfile(%this.getHpFillProfile(%hp, %hpMax));
   %this.tooltipHpFill.Extent = %fillW SPC %trackH;

   %this.tooltipAttackLabel.setProfile(%this.getStatProfile(%atk, %baseAtk));
   %this.tooltipAttackLabel.setText(%this.formatDiceStat("Attack", %atk, %baseAtk));

   %this.tooltipDefenceLabel.setProfile(%this.getStatProfile(%def, %baseDef));
   %this.tooltipDefenceLabel.setText(%this.formatDiceStat("Defence", %def, %baseDef));

   %this.tooltipStatusLabel.setText(%statusText);

   %pos = %piece.getPosition();
   %this.positionTooltipNearWorld(getWord(%pos, 0), getWord(%pos, 1));
   %this.tooltipCtrl.setVisible(true);
   %this.bringTooltipAboveBoard();
}

function UnitStatsHud::showTooltipForUnitType(%this, %creatureId, %worldPos)
{
   if (!isObject(UnitProperties) || %creatureId $= "" || %this.isModalBlocking() || %this.isAnyCharacterBusy())
   {
      %this.hideTooltip();
      return;
   }

   if (!isObject(%this.tooltipCtrl))
      %this.createControls();

   %uName = %this.getUnitDisplayNameById(%creatureId, "");
   %lvl = UnitProperties.lvl[%creatureId];
   %hp = UnitProperties.hp[%creatureId];
   %atk = UnitProperties.attack[%creatureId];
   %def = UnitProperties.defence[%creatureId];

   if (isObject($LocalizationManager))
   {
      %roleLabel = (UnitProperties.missile[%creatureId] == 1) ? $LocalizationManager.getText("hud_role_ranged") : $LocalizationManager.getText("hud_role_melee");
      %humanFaction = $LocalizationManager.getText("faction_human_order");

      %this.tooltipCtrl.setProfile(%this.tooltipPanelProfile);
      %this.tooltipHeaderLabel.setText($LocalizationManager.getText("tooltip_header", %uName, %lvl));
      %this.tooltipTeamLabel.setText($LocalizationManager.getText("tooltip_team_role", %humanFaction, %roleLabel));
      %this.tooltipHpLabel.setText($LocalizationManager.getText("tooltip_hp", %hp, %hp));

      %trackExt = isObject(%this.tooltipHpTrack) ? %this.tooltipHpTrack.extent : "488 12";
      %trackW = getWord(%trackExt, 0);
      %trackH = getWord(%trackExt, 1);
      if (%trackW <= 0) %trackW = 488;
      if (%trackH <= 0) %trackH = 12;
      %this.tooltipHpFill.setProfile(%this.hpHighProfile);
      %this.tooltipHpFill.Extent = %trackW SPC %trackH;

      %this.tooltipAttackLabel.setProfile(%this.tooltipStatProfile);
      %this.tooltipAttackLabel.setText(%this.formatDiceStat($LocalizationManager.getText("stat_attack"), %atk, %atk));

      %this.tooltipDefenceLabel.setProfile(%this.tooltipStatProfile);
      %this.tooltipDefenceLabel.setText(%this.formatDiceStat($LocalizationManager.getText("stat_defence"), %def, %def));

      %this.tooltipStatusLabel.setText($LocalizationManager.getText("status_recruit_cost", %lvl));

      %this.positionTooltipNearWorld(getWord(%worldPos, 0), getWord(%worldPos, 1));
      %this.tooltipCtrl.setVisible(true);
      %this.bringTooltipAboveBoard();
      return;
   }

   %roleLabel = (UnitProperties.missile[%creatureId] == 1) ? "Ranged" : "Melee";

   %this.tooltipCtrl.setProfile(%this.tooltipPanelProfile);
   %this.tooltipHeaderLabel.setText(%uName @ " (Lvl " @ %lvl @ ")");
   %this.tooltipTeamLabel.setText("Human Order  |  " @ %roleLabel);
   %this.tooltipHpLabel.setText("HP: " @ %hp @ " / " @ %hp);

   %trackExt = isObject(%this.tooltipHpTrack) ? %this.tooltipHpTrack.extent : "488 12";
   %trackW = getWord(%trackExt, 0);
   %trackH = getWord(%trackExt, 1);
   if (%trackW <= 0) %trackW = 488;
   if (%trackH <= 0) %trackH = 12;
   %this.tooltipHpFill.setProfile(%this.hpHighProfile);
   %this.tooltipHpFill.Extent = %trackW SPC %trackH;

   %this.tooltipAttackLabel.setProfile(%this.tooltipStatProfile);
   %this.tooltipAttackLabel.setText(%this.formatDiceStat("Attack", %atk, %atk));

   %this.tooltipDefenceLabel.setProfile(%this.tooltipStatProfile);
   %this.tooltipDefenceLabel.setText(%this.formatDiceStat("Defence", %def, %def));

   %this.tooltipStatusLabel.setText("Recruit Cost: " @ %lvl @ " CP");

   %this.positionTooltipNearWorld(getWord(%worldPos, 0), getWord(%worldPos, 1));
   %this.tooltipCtrl.setVisible(true);
   %this.bringTooltipAboveBoard();
}

function UnitStatsHud::hideTooltip(%this)
{
   %this.hoveredPiece = "";
   %this.hoveredUnitType = "";
   %this.hoveredWorldPos = "";
   if (isObject(%this.tooltipCtrl))
      %this.tooltipCtrl.setVisible(false);
}

function UnitStatsHud::refreshLocalization(%this)
{
   if (%this.hoveredUnitType !$= "" && %this.hoveredWorldPos !$= "" && isObject(%this.tooltipCtrl) && %this.tooltipCtrl.isVisible())
   {
      if (%this.badgesVisible)
         %this.refreshAll();
      %this.showTooltipForUnitType(%this.hoveredUnitType, %this.hoveredWorldPos);
      return;
   }
   %this.refreshAll();
}

function UnitStatsHud::reanchor(%this)
{
   if (!isObject(mainScreenGui))
      return;

   %mainW = getWord(mainScreenGui.extent, 0);
   %mainH = getWord(mainScreenGui.extent, 1);

   if (isObject(%this.badgeLayer))
   {
      %this.badgeLayer.Position = "0 0";
      %this.badgeLayer.Extent = %mainW SPC %mainH;
   }

   if (isObject(%this.tooltipCtrl))
   {
      %tipScale = isObject($ResolutionManager) ? $ResolutionManager.getViewportSafeUiScale(520, 270, 24) : 1.0;
      if (isObject($ResolutionManager))
         $ResolutionManager.applyUiScaleToTree(%this.tooltipCtrl, %tipScale, false);

      if (%this.isTooltipVisible())
      {
         if (isObject(%this.hoveredPiece))
            %this.showTooltipForPiece(%this.hoveredPiece);
         else if (%this.hoveredUnitType !$= "" && %this.hoveredWorldPos !$= "")
            %this.showTooltipForUnitType(%this.hoveredUnitType, %this.hoveredWorldPos);
      }
   }

   if (%this.badgesVisible)
      %this.refreshAll();
}

//---------------------------------------------------------------------------------------------
// Inspection Getters
//---------------------------------------------------------------------------------------------
function UnitStatsHud::isTooltipVisible(%this)
{
   return (isObject(%this.tooltipCtrl) && %this.tooltipCtrl.isVisible());
}

function UnitStatsHud::getHoveredPiece(%this)
{
   return %this.hoveredPiece;
}

function UnitStatsHud::getTooltipHeaderText(%this)
{
   return isObject(%this.tooltipHeaderLabel) ? %this.tooltipHeaderLabel.getText() : "";
}

function UnitStatsHud::getTooltipTeamText(%this)
{
   return isObject(%this.tooltipTeamLabel) ? %this.tooltipTeamLabel.getText() : "";
}

function UnitStatsHud::getTooltipHpText(%this)
{
   return isObject(%this.tooltipHpLabel) ? %this.tooltipHpLabel.getText() : "";
}

function UnitStatsHud::getTooltipAttackText(%this)
{
   return isObject(%this.tooltipAttackLabel) ? %this.tooltipAttackLabel.getText() : "";
}

function UnitStatsHud::getTooltipDefenceText(%this)
{
   return isObject(%this.tooltipDefenceLabel) ? %this.tooltipDefenceLabel.getText() : "";
}

function UnitStatsHud::getTooltipStatusText(%this)
{
   return isObject(%this.tooltipStatusLabel) ? %this.tooltipStatusLabel.getText() : "";
}

function UnitStatsHud::isBadgeVisible(%this, %pieceId)
{
   return (isObject(%this.badgeLayer) && %this.badgeLayer.isVisible() &&
           isObject(%this.badgeCtrl[%pieceId]) && %this.badgeCtrl[%pieceId].isVisible());
}

function UnitStatsHud::getBadgeText(%this, %pieceId)
{
   if (!isObject(%this.badgeCtrl[%pieceId]) || !isObject(%this.badgeCtrl[%pieceId].statLabel))
      return "";
   return %this.badgeCtrl[%pieceId].statLabel.getText();
}

function UnitStatsHud::getVisibleBadgeCount(%this)
{
   if (!isObject(%this.badgeLayer) || !%this.badgeLayer.isVisible())
      return 0;

   %count = 0;
   for (%i = 1; %i <= %this.maxTrackedPieceId; %i++)
   {
      if (isObject(%this.badgeCtrl[%i]) && %this.badgeCtrl[%i].isVisible())
         %count++;
   }
   return %count;
}

//---------------------------------------------------------------------------------------------
// Console Commands for On-Board Micro-Readout Badges
//---------------------------------------------------------------------------------------------
function enableUnitBadges()
{
   if (isObject($UnitStatsHud))
      return $UnitStatsHud.setConsoleBadgesEnabled(true);
   return false;
}

function disableUnitBadges()
{
   if (isObject($UnitStatsHud))
      return $UnitStatsHud.setConsoleBadgesEnabled(false);
   return false;
}

function toggleUnitBadges()
{
   if (isObject($UnitStatsHud))
      return $UnitStatsHud.toggleConsoleBadges();
   return false;
}

function showUnitBadges(%enabled)
{
   if (%enabled $= "")
      return enableUnitBadges();
   if (isObject($UnitStatsHud))
      return $UnitStatsHud.setConsoleBadgesEnabled(%enabled);
   return false;
}

// Initialize Singleton
if (!isObject(UnitStatsHud))
{
   new ScriptObject(UnitStatsHud) { };
}
if (!isObject($UnitStatsHud))
{
   $UnitStatsHud = UnitStatsHud;
}
