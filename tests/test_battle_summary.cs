//-----------------------------------------------------------------------------
// tests/test_battle_summary.cs
// Verification suite for TurnManager::endBattle, BattleSummaryGui Victory/Defeat
// banner, surviving unit tally, turn-queue exhaustion trigger, and navigation.
//-----------------------------------------------------------------------------

$Scripts::ignoreDSOs = true;
setLogMode(1);
$Archon::skipSplash = true;

$battleSummaryTestPath = pathConcat(getMainDotCsDir(), "tests/battle_summary_test_result.txt");
$battleSummaryTestFile = new FileObject();
$battleSummaryTestFile.openForWrite($battleSummaryTestPath);
$battleSummaryTestFile.writeLine("BATTLE_SUMMARY_TEST_START");

function logSummaryResult(%msg)
{
   echo(%msg);
   if (isObject($battleSummaryTestFile))
      $battleSummaryTestFile.writeLine(%msg);
}

function failSummaryTest(%msg)
{
   logSummaryResult("TEST_FAIL: " @ %msg);
   if (isObject($battleSummaryTestFile))
   {
      $battleSummaryTestFile.close();
      $battleSummaryTestFile.delete();
   }
   quit();
}

function passSummaryTest(%msg)
{
   logSummaryResult("TEST_PASS: " @ %msg);
}

function assertSummaryCond(%label, %cond)
{
   if (!%cond)
      failSummaryTest(%label);
   passSummaryTest(%label);
}

function makeTestUnit(%idx, %groupId, %creatureId, %name, %lvl, %hp, %hpMax, %atk, %def, %alive)
{
   if (isObject($nappulaSprite[%idx]))
      $nappulaSprite[%idx].delete();

   $nappulaSprite[%idx] = new Sprite()
   {
      scene = t2dScene;
      class = "Piece";
      superclass = "Nappula";
      Position = ((%idx - 3) * 8) SPC (%groupId == 0 ? "-8" : "8");
      Size = "8 8";
      SceneLayer = (%alive == 1 ? 2 : 3);
      index = %idx;
      groupid = %groupId;
      creatureID = %creatureId;
      name = %name;
      level = %lvl;
      hp = %hp;
      hpMAX = %hpMax;
      attack = %atk;
      attackMAX = %atk;
      defence = %def;
      defenceMAX = %def;
      alive = %alive;
      boardX = %idx - 1;
      boardY = (%groupId == 0 ? 2 : 0);
      positionOnBoard = ((%idx - 1) SPC (%groupId == 0 ? 2 : 0));
   };
   return $nappulaSprite[%idx];
}

setCompanyAndProduct("Torque Game Engines", "Archonita");
ModuleDatabase.EchoInfo = false;
AssetDatabase.EchoInfo = false;

ModuleDatabase.scanModules(pathConcat(getMainDotCsDir(), "library"));
ModuleDatabase.scanModules(pathConcat(getMainDotCsDir(), "Archon"));
ModuleDatabase.LoadExplicit("AppCore");

if (isObject($LocalizationManager))
   $LocalizationManager.setLanguage("en", true);

logSummaryResult("==========================================");
logSummaryResult("Archonita - Battle Summary Verification Suite");
logSummaryResult("==========================================");

// 1. Verify BattleSummaryGui singleton, profiles, controls, and reanchor
assertSummaryCond("$BattleSummaryGui singleton exists", isObject($BattleSummaryGui) && isObject(BattleSummaryGui));
assertSummaryCond("BattleSummaryGui profiles created",
   isObject($BattleSummaryGui.goldBorderProfile) &&
   isObject($BattleSummaryGui.crimsonBorderProfile) &&
   isObject($BattleSummaryGui.victoryBannerProfile) &&
   isObject($BattleSummaryGui.defeatBannerProfile) &&
   isObject($BattleSummaryGui.victoryTitleProfile) &&
   isObject($BattleSummaryGui.defeatTitleProfile) &&
   isObject($BattleSummaryGui.btnPrimaryProfile) &&
   isObject($BattleSummaryGui.btnSecondaryProfile));
assertSummaryCond("BattleSummaryGui controls created and attached to mainScreenGui",
   isObject($BattleSummaryGui.backdropCtrl) &&
   isObject($BattleSummaryGui.panelCtrl) &&
   isObject($BattleSummaryGui.bannerBox) &&
   isObject($BattleSummaryGui.bannerTitleLabel) &&
   isObject($BattleSummaryGui.playerTallyLabel) &&
   isObject($BattleSummaryGui.enemyTallyLabel) &&
   isObject($BattleSummaryGui.unitRowLabel[0]) &&
   isObject($BattleSummaryGui.unitRowLabel[5]) &&
   isObject($BattleSummaryGui.btnPlayAgain) &&
   isObject($BattleSummaryGui.btnMainMenu) &&
   mainScreenGui.isMember($BattleSummaryGui.backdropCtrl));
assertSummaryCond("BattleSummaryGui starts hidden", !$BattleSummaryGui.isVisible());

$ResolutionManager.reanchorAllGuis();
assertSummaryCond("ResolutionManager.reanchorAllGuis reanchors BattleSummaryGui cleanly",
   getWord($BattleSummaryGui.backdropCtrl.Extent, 0) > 0 &&
   getWord($BattleSummaryGui.backdropCtrl.Extent, 1) > 0);

// Prepare mock battle scene
if (isObject(t2dScene))
{
   t2dScene.clear();
   t2dScene.delete();
}
new Scene(t2dScene);
TurnManager.initStateMachine();
CombatEngine.initAnimStates();

// 2. Victory Scenario: 2/3 Player surviving (Paladin 2/3 HP, Archer 1/1 HP, fallen Warrior) vs Undead Legion 0/2
game.reset();
game.setState($gameBattleState);
game.setTurn(7);
game.setActivePlayer(0);
game.setPieceCount(5);

HeroGroup.getObject(0).reset();
HeroGroup.getObject(0).setHeroClass(1); // Mage
$FactionManager.setEnemyFaction("undead");

makeTestUnit(1, 0, 3, "paladin", 4, 2, 3, 4, 5, 1);
makeTestUnit(2, 0, 1, "archer", 2, 1, 1, 2, 2, 1);
makeTestUnit(3, 0, 0, "warrior", 1, 0, 1, 2, 3, 0);
makeTestUnit(4, 1, 4, "skeletonwarrior", 1, 0, 1, 2, 3, 0);
makeTestUnit(5, 1, 7, "ancientghost", 4, 0, 3, 3, 6, 0);

TurnManager.endBattle();

assertSummaryCond("Victory: game state transitioned to $gameEndState", game.getState() == $gameEndState);
assertSummaryCond("Victory: TurnManager did not schedule abrupt 2000ms auto-exit", !isEventPending(TurnManager.endBattleEvent));
assertSummaryCond("Victory: TurnManager outcome is VICTORY", TurnManager.getLastBattleOutcome() $= "VICTORY");
assertSummaryCond("Victory: Player tally 2/3 and Enemy tally 0/2",
   TurnManager.getPlayerSurvivingCount() == 2 &&
   TurnManager.getPlayerTotalCount() == 3 &&
   TurnManager.getEnemySurvivingCount() == 0 &&
   TurnManager.getEnemyTotalCount() == 2 &&
   TurnManager.getSurvivingUnitCount() == 2);
assertSummaryCond("Victory: BattleSummaryGui visible with VICTORY banner",
   $BattleSummaryGui.isVisible() &&
   $BattleSummaryGui.getOutcome() $= "VICTORY" &&
   $BattleSummaryGui.getBannerTitleText() $= "VICTORY" &&
   strpos($BattleSummaryGui.getBannerSubtitleText(), "Undead Legion") != -1);
assertSummaryCond("Victory: Summary cards display accurate unit tallies and HP/Fallen counts",
   $BattleSummaryGui.getPlayerTallyText() $= "2 / 3 Units Surviving" &&
   $BattleSummaryGui.getEnemyTallyText() $= "0 / 2 Units Surviving" &&
   BattleSummaryPlayerDetail.getText() $= "Remaining HP: 3  |  Fallen: 1" &&
   BattleSummaryEnemyDetail.getText() $= "Remaining HP: 0  |  Fallen: 2" &&
   strpos(BattleSummaryBannerMeta.getText(), "Mage") != -1 &&
   strpos(BattleSummaryBannerMeta.getText(), "7") != -1);
assertSummaryCond("Victory: Surviving unit roster lists Paladin (2/3) and Archer (1/1)",
   $BattleSummaryGui.getSurvivingUnitCount() == 2 &&
   strpos($BattleSummaryGui.getSurvivingUnitRowText(0), "Paladin") != -1 &&
   strpos($BattleSummaryGui.getSurvivingUnitRowText(0), "2/3") != -1 &&
   strpos($BattleSummaryGui.getSurvivingUnitRowText(1), "Archer") != -1 &&
   strpos($BattleSummaryGui.getSurvivingUnitRowText(1), "1/1") != -1 &&
   $BattleSummaryGui.getSurvivingUnitRowText(2) $= "");

// 3. Defeat Scenario: 0/2 Player surviving vs Dragonfolk Faction 2/3 surviving (Gold Dragon 3/4, Red Dragon 2/2, fallen Dragon Folk)
$BattleSummaryGui.show(false);
game.reset();
game.setState($gameBattleState);
game.setTurn(9);
game.setActivePlayer(1);
game.setPieceCount(5);

HeroGroup.getObject(0).reset(); // Warrior
$FactionManager.setEnemyFaction("dragonfolk");

makeTestUnit(1, 0, 0, "warrior", 1, 0, 1, 2, 3, 0);
makeTestUnit(2, 0, 3, "paladin", 4, 0, 3, 4, 5, 0);
makeTestUnit(3, 1, 11, "golddragon", 4, 3, 4, 4, 5, 1);
makeTestUnit(4, 1, 9, "reddragon", 2, 2, 2, 2, 3, 1);
makeTestUnit(5, 1, 8, "dragonfolk", 1, 0, 2, 2, 3, 0);

TurnManager.endBattle();

assertSummaryCond("Defeat: TurnManager outcome is DEFEAT", TurnManager.getLastBattleOutcome() $= "DEFEAT");
assertSummaryCond("Defeat: Player tally 0/2 and Enemy tally 2/3",
   TurnManager.getPlayerSurvivingCount() == 0 &&
   TurnManager.getPlayerTotalCount() == 2 &&
   TurnManager.getEnemySurvivingCount() == 2 &&
   TurnManager.getEnemyTotalCount() == 3);
assertSummaryCond("Defeat: BattleSummaryGui visible with DEFEAT banner and Dragonfolk header",
   $BattleSummaryGui.isVisible() &&
   $BattleSummaryGui.getOutcome() $= "DEFEAT" &&
   $BattleSummaryGui.getBannerTitleText() $= "DEFEAT" &&
   strpos($BattleSummaryGui.getBannerSubtitleText(), "Dragonfolk Faction") != -1 &&
   strpos(BattleSummaryEnemyHeader.getText(), "DRAGONFOLK FACTION") != -1);
assertSummaryCond("Defeat: Summary cards display 0/2 Player and 2/3 Enemy",
   $BattleSummaryGui.getPlayerTallyText() $= "0 / 2 Units Surviving" &&
   $BattleSummaryGui.getEnemyTallyText() $= "2 / 3 Units Surviving");
assertSummaryCond("Defeat: Surviving unit roster lists Gold Dragon (3/4) and Red Dragon (2/2)",
   strpos($BattleSummaryGui.getSurvivingUnitRowText(0), "Gold Dragon") != -1 &&
   strpos($BattleSummaryGui.getSurvivingUnitRowText(0), "3/4") != -1 &&
   strpos($BattleSummaryGui.getSurvivingUnitRowText(1), "Red Dragon") != -1 &&
   strpos($BattleSummaryGui.getSurvivingUnitRowText(1), "2/2") != -1);

// 4. Turn queue exhaustion trigger via TurnManager::nextTurn / distributeTurnQueue
$BattleSummaryGui.show(false);
game.setState($gameBattleState);
game.setTurnQueue("");
game.setTurn(4);
TurnManager.nextTurn();
assertSummaryCond("Turn queue exhaustion triggers endBattle and displays DEFEAT summary",
   game.getState() == $gameEndState &&
   $BattleSummaryGui.isVisible() &&
   $BattleSummaryGui.getBannerTitleText() $= "DEFEAT" &&
   !isEventPending(TurnManager.endBattleEvent));

// Also verify nextTurn when turnQueue is non-empty and one army is eliminated
$BattleSummaryGui.show(false);
game.setState($gameBattleState);
game.setTurnQueue("3");
TurnManager.nextTurn();
assertSummaryCond("Non-empty turn queue with eliminated army triggers endBattle summary",
   game.getState() == $gameEndState &&
   $BattleSummaryGui.isVisible() &&
   $BattleSummaryGui.getBannerTitleText() $= "DEFEAT" &&
   !isEventPending(TurnManager.endBattleEvent));

// 5. Navigation actions: Play Again, Return to Main Menu, and Escape key
$BattleSummaryGui.onPlayAgainClicked();
assertSummaryCond("Play Again hides summary, enters Card Select state, and enables window mouse events",
   !$BattleSummaryGui.isVisible() &&
   getGameState() == $gameCardSelectState &&
   sceneWindow2D.getUseWindowMouseEvents() == true);

TurnManager.endBattle();
assertSummaryCond("Re-entering endBattle shows BattleSummaryGui again", $BattleSummaryGui.isVisible());
$BattleSummaryGui.onMainMenuClicked();
assertSummaryCond("Return to Main Menu hides summary and enters Main Menu state",
   !$BattleSummaryGui.isVisible() &&
   getGameState() == $gameMainMenuState);

TurnManager.endBattle();
assertSummaryCond("Summary visible before Escape key press", $BattleSummaryGui.isVisible());
onEscapeKeyPressed();
assertSummaryCond("Escape key dismisses BattleSummaryGui to Main Menu",
   !$BattleSummaryGui.isVisible() &&
   getGameState() == $gameMainMenuState);

// Clean up before exiting
if ($bubbleSchedule !$= "")
{
   cancel($bubbleSchedule);
   $bubbleSchedule = "";
}
stopBackgroundMusic();
alxStopAll();
if (isObject(t2dScene))
{
   t2dScene.clear();
   t2dScene.delete();
}

logSummaryResult("==========================================");
logSummaryResult("ALL BATTLE SUMMARY TESTS PASSED!");
logSummaryResult("==========================================");

if (isObject($battleSummaryTestFile))
{
   $battleSummaryTestFile.close();
   $battleSummaryTestFile.delete();
}

quit();
