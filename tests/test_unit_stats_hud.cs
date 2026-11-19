//-----------------------------------------------------------------------------
// tests/test_unit_stats_hud.cs
// Verification suite for UnitStatsHud: on-board micro-HP/dice readout badges,
// rich hover tooltips, spell buff/debuff modifier deltas, pre-battle icon hover,
// movement/damage/turn updates, and modal/endBattle cleanup.
//-----------------------------------------------------------------------------

$Scripts::ignoreDSOs = true;
setLogMode(1);
$Archon::skipSplash = true;

$unitStatsTestPath = pathConcat(getMainDotCsDir(), "tests/unit_stats_hud_test_result.txt");
$unitStatsTestFile = new FileObject();
$unitStatsTestFile.openForWrite($unitStatsTestPath);
$unitStatsTestFile.writeLine("UNIT_STATS_HUD_TEST_START");

function logStatsResult(%msg)
{
   echo(%msg);
   if (isObject($unitStatsTestFile))
      $unitStatsTestFile.writeLine(%msg);
}

function failStatsTest(%msg)
{
   logStatsResult("TEST_FAIL: " @ %msg);
   if (isObject($unitStatsTestFile))
   {
      $unitStatsTestFile.close();
      $unitStatsTestFile.delete();
   }
   quit();
}

function passStatsTest(%msg)
{
   logStatsResult("TEST_PASS: " @ %msg);
}

function assertStatsCond(%label, %cond)
{
   if (!%cond)
      failStatsTest(%label);
   passStatsTest(%label);
}

function makeStatsTestUnit(%idx, %groupId, %creatureId, %name, %bx, %by, %alive)
{
   if (isObject($nappulaSprite[%idx]))
      $nappulaSprite[%idx].delete();

   $nappulaSprite[%idx] = new Sprite()
   {
      scene = t2dScene;
      class = "Piece";
      superclass = "Nappula";
      Position = boardToWorldUnit(%bx, %by);
      Size = "8 8";
      SceneLayer = (%alive == 1 ? 2 : 3);
      index = %idx;
      groupid = %groupId;
      creatureID = %creatureId;
      name = %name;
      level = UnitProperties.lvl[%creatureId];
      hp = (%alive == 1 ? UnitProperties.hp[%creatureId] : 0);
      hpMAX = UnitProperties.hp[%creatureId];
      attack = UnitProperties.attack[%creatureId];
      attackMAX = UnitProperties.attack[%creatureId];
      defence = UnitProperties.defence[%creatureId];
      defenceMAX = UnitProperties.defence[%creatureId];
      mana = UnitProperties.mana[%creatureId];
      manaMAX = UnitProperties.mana[%creatureId];
      spells = UnitProperties.spells[%creatureId];
      missile = UnitProperties.missile[%creatureId];
      alive = %alive;
      hasMoved = 0;
      hasDoneAction = 0;
      skipNextTurn = 0;
      facing = (%groupId == 0 ? "n" : "s");
      boardX = %bx;
      boardY = %by;
      positionOnBoard = %bx SPC %by;
   };
   return $nappulaSprite[%idx];
}

function makeStatsMockCard(%frame)
{
   return new Sprite()
   {
      scene = t2dScene;
      class = "Card";
      Position = "-40 0";
      Size = "12 18";
      spellFrame = %frame;
   };
}

setCompanyAndProduct("Torque Game Engines", "Archonita");
ModuleDatabase.EchoInfo = false;
AssetDatabase.EchoInfo = false;

ModuleDatabase.scanModules(pathConcat(getMainDotCsDir(), "library"));
ModuleDatabase.scanModules(pathConcat(getMainDotCsDir(), "Archon"));
ModuleDatabase.LoadExplicit("AppCore");

if (isObject($LocalizationManager))
   $LocalizationManager.setLanguage("en", true);

logStatsResult("==========================================");
logStatsResult("Archonita - Unit Stats HUD Verification Suite");
logStatsResult("==========================================");

// 1. Verify singleton, profiles, controls, and reanchor
assertStatsCond("$UnitStatsHud singleton exists", isObject($UnitStatsHud) && isObject(UnitStatsHud));
assertStatsCond("UnitStatsHud controls created and attached to mainScreenGui",
   isObject($UnitStatsHud.badgeLayer) &&
   isObject($UnitStatsHud.tooltipCtrl) &&
   isObject($UnitStatsHud.tooltipHeaderLabel) &&
   isObject($UnitStatsHud.tooltipHpLabel) &&
   isObject($UnitStatsHud.tooltipAttackLabel) &&
   isObject($UnitStatsHud.tooltipDefenceLabel) &&
   isObject($UnitStatsHud.tooltipStatusLabel) &&
   mainScreenGui.isMember($UnitStatsHud.badgeLayer) &&
   mainScreenGui.isMember($UnitStatsHud.tooltipCtrl));
assertStatsCond("UnitStatsHud starts with tooltip hidden and 0 visible badges",
   !$UnitStatsHud.isTooltipVisible() && $UnitStatsHud.getVisibleBadgeCount() == 0);

$ResolutionManager.reanchorAllGuis();
assertStatsCond("ResolutionManager.reanchorAllGuis reanchors UnitStatsHud badgeLayer",
   getWord($UnitStatsHud.badgeLayer.Extent, 0) > 0 &&
   getWord($UnitStatsHud.badgeLayer.Extent, 1) > 0);

// 2. Prepare mock battle scene and units
if (isObject(t2dScene))
{
   t2dScene.clear();
   t2dScene.delete();
}
new Scene(t2dScene);
sceneWindow2D.setScene(t2dScene);
$ResolutionManager.applyCameraAspect(sceneWindow2D);

if (!isObject(board))
   new ScriptObject(board);
$board = board;

if (isObject(blood))
   blood.delete();
new Sprite(blood) { scene = t2dScene; Position = "-50 -80"; Size = "8 8"; SceneLayer = 1; };

if (isObject(heroicon))
   heroicon.delete();
new Sprite(heroicon) { scene = t2dScene; Position = "-40 -16"; Size = "16 16"; SceneLayer = 31; };

TurnManager.initStateMachine();
CombatEngine.initAnimStates();
$FactionManager.setEnemyFaction("undead");

game.reset();
game.setState($gameBattleState);
game.setTurn(1);
game.setActivePlayer(0);
game.setSelectedPiece(1);
game.setPieceCount(3);

makeStatsTestUnit(1, 0, 3, "paladin", 1, 2, 1);        // HP 3/3, ATK 4, DEF 5
makeStatsTestUnit(2, 1, 7, "ancientghost", 2, 1, 1);   // HP 3/3, ATK 3, DEF 6, Spells 1
makeStatsTestUnit(3, 1, 4, "skeletonwarrior", 0, 0, 0);// Dead unit

$UnitStatsHud.showBadges(true);

assertStatsCond("Micro-readout badges stay hidden by default until enabled from console",
   !$UnitStatsHud.areConsoleBadgesEnabled() &&
   $UnitStatsHud.getVisibleBadgeCount() == 0 &&
   !$UnitStatsHud.isBadgeVisible(1) &&
   !$UnitStatsHud.isBadgeVisible(2));

// Hover tooltip still works while micro-readout badges are disabled
$UnitStatsHud.updateHover($nappulaSprite[1].getPosition());
assertStatsCond("Hover tooltip remains active while console micro-readout badges are disabled",
   $UnitStatsHud.isTooltipVisible() &&
   $UnitStatsHud.getHoveredPiece() == $nappulaSprite[1]);

// Moving character hides and suppresses hover tooltips
$nappulaSprite[2].state = $olentoWalkState[7];
$UnitStatsHud.refreshAll();
assertStatsCond("Active character movement hides currently open hover tooltip",
   !$UnitStatsHud.isTooltipVisible() && $UnitStatsHud.isAnyCharacterBusy());
$UnitStatsHud.updateHover($nappulaSprite[1].getPosition());
assertStatsCond("Hovering while a character is moving keeps tooltip hidden",
   !$UnitStatsHud.isTooltipVisible());
$nappulaSprite[2].state = $olentoStandState[7];

// Doing an action (inputLocked / combat action) hides and suppresses hover tooltips
$UnitStatsHud.updateHover($nappulaSprite[1].getPosition());
assertStatsCond("Hover tooltip re-appears once movement stops",
   $UnitStatsHud.isTooltipVisible());
CombatEngine.lockInput();
assertStatsCond("Starting a character action hides currently open hover tooltip",
   !$UnitStatsHud.isTooltipVisible() && $UnitStatsHud.isAnyCharacterBusy());
$UnitStatsHud.updateHover($nappulaSprite[1].getPosition());
assertStatsCond("Hovering while a character is doing an action keeps tooltip hidden",
   !$UnitStatsHud.isTooltipVisible());
CombatEngine.unlockInput();
$UnitStatsHud.hideTooltip();

// Enable badges via console command
enableUnitBadges();

assertStatsCond("Enabling badges from console shows badges only for the 2 living units",
   $UnitStatsHud.areConsoleBadgesEnabled() &&
   $UnitStatsHud.getVisibleBadgeCount() == 2 &&
   $UnitStatsHud.isBadgeVisible(1) &&
   $UnitStatsHud.isBadgeVisible(2) &&
   !$UnitStatsHud.isBadgeVisible(3));
assertStatsCond("Paladin initial micro-badge reads '3/3 A4 D5'",
   $UnitStatsHud.getBadgeText(1) $= "3/3 A4 D5");
assertStatsCond("Ancient Ghost initial micro-badge reads '3/3 A3 D6'",
   $UnitStatsHud.getBadgeText(2) $= "3/3 A3 D6");
assertStatsCond("Active selected piece uses gold active badge profile",
   $UnitStatsHud.badgeCtrl[1].Profile.getId() == $UnitStatsHud.badgeActiveBgProfile.getId());

// 3. Damage and Spell Modifiers update micro-readout and tooltip live
CombatEngine.damagePiece($nappulaSprite[1]);
assertStatsCond("Damage updates Paladin micro-badge to '2/3 A4 D5'",
   $UnitStatsHud.getBadgeText(1) $= "2/3 A4 D5");

// Cast Taikamakkara (Frame 3: +1 Defence Roll to friendly units)
$specialcard[1] = makeStatsMockCard(3);
$topCardIndex = 1;
game.setCanCastSpell(true);
game.setSpellRevealed(true);
%castBuff = CombatEngine.castRevealedSpell("board", "");
assertStatsCond("Taikamakkara cast succeeded and updated Paladin badge to '2/3 A4 D6+'",
   %castBuff == true && $UnitStatsHud.getBadgeText(1) $= "2/3 A4 D6+");
if (isObject($specialcard[1]))
   $specialcard[1].safeDelete();

// Cast Vatsatauti (Frame 8: Skip Next Turn) on Ancient Ghost
$specialcard[1] = makeStatsMockCard(8);
$topCardIndex = 1;
game.setCanCastSpell(true);
game.setSpellRevealed(true);
%castSick = CombatEngine.castRevealedSpell("unit", $nappulaSprite[2]);
assertStatsCond("Vatsatauti updates Ancient Ghost micro-badge to '3/3 SKIP' with sick profile",
   %castSick == true &&
   $UnitStatsHud.getBadgeText(2) $= "3/3 SKIP" &&
   $UnitStatsHud.badgeCtrl[2].Profile.getId() == $UnitStatsHud.badgeSickBgProfile.getId());
if (isObject($specialcard[1]))
   $specialcard[1].safeDelete();

// Clear skipNextTurn and apply -1 Attack debuff to Ancient Ghost
$nappulaSprite[2].skipNextTurn = 0;
$nappulaSprite[2].attack = 2;
$UnitStatsHud.refreshAll();
assertStatsCond("Attack debuff updates Ancient Ghost micro-badge to '3/3 A2- D6'",
   $UnitStatsHud.getBadgeText(2) $= "3/3 A2- D6");

// 4. Hover Tooltip on Board Pieces
$UnitStatsHud.updateHover($nappulaSprite[1].getPosition());
assertStatsCond("Hover tooltip profiles use 2x font sizes and scaled panel/HP track layout",
   $UnitStatsHud.tooltipHeaderProfile.fontSize == 28 &&
   $UnitStatsHud.tooltipSubProfile.fontSize == 22 &&
   $UnitStatsHud.tooltipStatProfile.fontSize == 24 &&
   $UnitStatsHud.tooltipBuffProfile.fontSize == 24 &&
   $UnitStatsHud.tooltipDebuffProfile.fontSize == 24 &&
   $UnitStatsHud.tooltipStatusProfile.fontSize == 22 &&
   $UnitStatsHud.tooltipCtrl.extent $= "520 270" &&
   $UnitStatsHud.tooltipHpTrack.extent $= "488 12");
assertStatsCond("Hovering Paladin shows tooltip with Paladin header and Human Order Melee",
   $UnitStatsHud.isTooltipVisible() &&
   $UnitStatsHud.getHoveredPiece() == $nappulaSprite[1] &&
   $UnitStatsHud.getTooltipHeaderText() $= "Paladin (Lvl 4)" &&
   $UnitStatsHud.getTooltipTeamText() $= "Human Order  |  Melee");
assertStatsCond("Paladin tooltip displays live HP, unmodified Attack, and buffed Defence (+1)",
   $UnitStatsHud.getTooltipHpText() $= "HP: 2 / 3" &&
   $UnitStatsHud.getTooltipAttackText() $= "Attack: 4 dice" &&
   $UnitStatsHud.getTooltipDefenceText() $= "Defence: 6 dice (+1)" &&
   $UnitStatsHud.tooltipDefenceLabel.Profile.getId() == $UnitStatsHud.tooltipBuffProfile.getId() &&
   $UnitStatsHud.getTooltipStatusText() $= "Active Turn: Move & Action Ready");

$UnitStatsHud.updateHover($nappulaSprite[2].getPosition());
assertStatsCond("Hovering Ancient Ghost shows Undead Legion, debuffed Attack (-1), and Ghostly Touch trait",
   $UnitStatsHud.isTooltipVisible() &&
   $UnitStatsHud.getTooltipHeaderText() $= "Ancient Ghost (Lvl 4)" &&
   $UnitStatsHud.getTooltipTeamText() $= "Undead Legion  |  Melee" &&
   $UnitStatsHud.getTooltipHpText() $= "HP: 3 / 3" &&
   $UnitStatsHud.getTooltipAttackText() $= "Attack: 2 dice (-1)" &&
   $UnitStatsHud.tooltipAttackLabel.Profile.getId() == $UnitStatsHud.tooltipDebuffProfile.getId() &&
   $UnitStatsHud.getTooltipDefenceText() $= "Defence: 6 dice" &&
   $UnitStatsHud.getTooltipStatusText() $= "Trait: Ghostly Touch");

$UnitStatsHud.updateHover("-45 32");
assertStatsCond("Hovering empty space hides tooltip", !$UnitStatsHud.isTooltipVisible());

$UnitStatsHud.updateHover($nappulaSprite[1].getPosition());
assertStatsCond("Tooltip visible prior to onMouseLeave", $UnitStatsHud.isTooltipVisible());
sceneWindow2D.onMouseLeave(0, "0 0", 0);
assertStatsCond("sceneWindow2D::onMouseLeave hides tooltip", !$UnitStatsHud.isTooltipVisible());

// 5. Lethal damage hides badge and live hover tooltip
$UnitStatsHud.updateHover($nappulaSprite[1].getPosition());
assertStatsCond("Tooltip visible before lethal damage on Paladin", $UnitStatsHud.isTooltipVisible());
CombatEngine.damagePiece($nappulaSprite[1]);
CombatEngine.damagePiece($nappulaSprite[1]);
assertStatsCond("Lethal damage hides Paladin badge and active hover tooltip",
   !$UnitStatsHud.isBadgeVisible(1) &&
   $UnitStatsHud.getVisibleBadgeCount() == 1 &&
   !$UnitStatsHud.isTooltipVisible());

// 6. Pre-Battle Setup Icon Hover Tooltip
game.setState($gamePreBattleState);
$icon[0] = new Sprite()
{
   scene = t2dScene;
   Position = "-36 -28";
   Size = "8 8";
   Visible = true;
   creatureLevel = 2; // Archer (creatureID 1)
};
$mouseObj::isObjMounted = false;
$UnitStatsHud.updateHover("-36 -28");
assertStatsCond("Pre-battle recruitment icon hover displays Archer base stats and CP cost",
   $UnitStatsHud.isTooltipVisible() &&
   $UnitStatsHud.getTooltipHeaderText() $= "Archer (Lvl 2)" &&
   $UnitStatsHud.getTooltipTeamText() $= "Human Order  |  Ranged" &&
   $UnitStatsHud.getTooltipHpText() $= "HP: 1 / 1" &&
   $UnitStatsHud.getTooltipAttackText() $= "Attack: 2 dice" &&
   $UnitStatsHud.getTooltipDefenceText() $= "Defence: 2 dice" &&
   $UnitStatsHud.getTooltipStatusText() $= "Recruit Cost: 2 CP");

$mouseObj::isObjMounted = true;
$UnitStatsHud.updateHover("-36 -28");
assertStatsCond("Dragging recruitment icon suppresses hover tooltip", !$UnitStatsHud.isTooltipVisible());
$mouseObj::isObjMounted = false;
if (isObject($icon[0]))
   $icon[0].delete();

// 7. Modal blocking (PauseMenu) and TurnManager::endBattle cleanup
game.setState($gameBattleState);
$UnitStatsHud.updateHover($nappulaSprite[2].getPosition());
assertStatsCond("Tooltip visible on Ancient Ghost before opening Pause Menu", $UnitStatsHud.isTooltipVisible());
$PauseMenuManager.openPauseMenu();
assertStatsCond("Opening Pause Menu hides hover tooltip", !$UnitStatsHud.isTooltipVisible());
$UnitStatsHud.updateHover($nappulaSprite[2].getPosition());
assertStatsCond("Hovering while Pause Menu is open keeps tooltip hidden", !$UnitStatsHud.isTooltipVisible());
$PauseMenuManager.onResumeClicked();
assertStatsCond("Resuming from Pause Menu keeps living unit badges visible", $UnitStatsHud.isBadgeVisible(2));

disableUnitBadges();
assertStatsCond("Disabling badges from console hides living unit badges immediately",
   !$UnitStatsHud.areConsoleBadgesEnabled() && $UnitStatsHud.getVisibleBadgeCount() == 0);
toggleUnitBadges();
assertStatsCond("Toggling badges from console re-enables living unit badges",
   $UnitStatsHud.areConsoleBadgesEnabled() && $UnitStatsHud.isBadgeVisible(2));

TurnManager.endBattle();
assertStatsCond("TurnManager::endBattle clears all micro-readout badges and hides tooltip",
   $UnitStatsHud.getVisibleBadgeCount() == 0 && !$UnitStatsHud.isTooltipVisible());

// Cleanup
if (isObject($BattleSummaryGui))
   $BattleSummaryGui.show(false);
stopBackgroundMusic();
alxStopAll();
if (isObject(t2dScene))
{
   t2dScene.clear();
   t2dScene.delete();
}

logStatsResult("==========================================");
logStatsResult("ALL UNIT STATS HUD TESTS PASSED!");
logStatsResult("==========================================");

if (isObject($unitStatsTestFile))
{
   $unitStatsTestFile.close();
   $unitStatsTestFile.delete();
}

quit();
