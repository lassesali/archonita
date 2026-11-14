//-----------------------------------------------------------------------------
// tests/test_spell_particles.cs
// Verification of spell particle assets, SpellEffectManager, CombatEngine & TurnManager hooks.
//-----------------------------------------------------------------------------

setLogMode(1);
$Archon::skipSplash = true;

$spellTestResultPath = pathConcat(getMainDotCsDir(), "tests/spell_particle_test_result.txt");
$spellTestFile = new FileObject();
$spellTestFile.openForWrite($spellTestResultPath);
$spellTestFile.writeLine("SPELL_PARTICLE_TEST_START");

function logSpellResult(%msg)
{
   echo(%msg);
   if (isObject($spellTestFile))
      $spellTestFile.writeLine(%msg);
}

function failSpellTest(%msg)
{
   logSpellResult("TEST_FAIL: " @ %msg);
   if (isObject($spellTestFile))
   {
      $spellTestFile.close();
      $spellTestFile.delete();
   }
   quit();
}

function passSpellTest(%msg)
{
   logSpellResult("TEST_PASS: " @ %msg);
}

function assertSpellCond(%label, %cond)
{
   if (!%cond)
      failSpellTest(%label);
   passSpellTest(%label);
}

setCompanyAndProduct("Torque Game Engines", "Archonita");
ModuleDatabase.EchoInfo = false;
AssetDatabase.EchoInfo = false;

ModuleDatabase.scanModules(pathConcat(getMainDotCsDir(), "library"));
ModuleDatabase.scanModules(pathConcat(getMainDotCsDir(), "Archon"));
ModuleDatabase.LoadExplicit("AppCore");

logSpellResult("==========================================");
logSpellResult("Archonita - Spell Particle Effects Suite");
logSpellResult("==========================================");

// 1. Verify SpellEffectManager singleton initialization & preloaded assets
assertSpellCond("SpellEffectManager singleton exists", isObject(SpellEffectManager));
assertSpellCond("$SpellEffectManager global ref matches", isObject($SpellEffectManager) && $SpellEffectManager.getId() == SpellEffectManager.getId());
assertSpellCond("SpellEffectManager initialized flag set", SpellEffectManager.initialized == true);
assertSpellCond("All 5 spell particle assets preloaded", SpellEffectManager.preloadedAssetCount == 5);

// 2. Verify all 5 ParticleAssets are declared and valid in AssetDatabase
%expectedAssets = "Archon:spell_volcano Archon:spell_disease Archon:spell_shield Archon:spell_mana Archon:spell_power";
for (%i = 0; %i < getWordCount(%expectedAssets); %i++)
{
   %assetId = getWord(%expectedAssets, %i);
   assertSpellCond("Declared ParticleAsset " @ %assetId, AssetDatabase.isDeclaredAsset(%assetId));
   assertSpellCond("Asset type is ParticleAsset for " @ %assetId, AssetDatabase.getAssetType(%assetId) $= "ParticleAsset");
   %assetObj = AssetDatabase.acquireAsset(%assetId);
   assertSpellCond("Acquired ParticleAsset " @ %assetId, isObject(%assetObj));
   assertSpellCond("Emitter count > 0 for " @ %assetId, %assetObj.getEmitterCount() > 0);
   AssetDatabase.releaseAsset(%assetId);
}

// 3. Verify spellFrame -> assetId mapping
assertSpellCond("Frame 3 maps to spell_shield", SpellEffectManager.getSpellAssetId(3) $= "Archon:spell_shield");
assertSpellCond("Frame 4 maps to spell_mana", SpellEffectManager.getSpellAssetId(4) $= "Archon:spell_mana");
assertSpellCond("Frame 5 maps to spell_power", SpellEffectManager.getSpellAssetId(5) $= "Archon:spell_power");
assertSpellCond("Frame 6 maps to spell_volcano", SpellEffectManager.getSpellAssetId(6) $= "Archon:spell_volcano");
assertSpellCond("Frame 7 maps to spell_mana", SpellEffectManager.getSpellAssetId(7) $= "Archon:spell_mana");
assertSpellCond("Frame 8 maps to spell_disease", SpellEffectManager.getSpellAssetId(8) $= "Archon:spell_disease");
assertSpellCond("Unknown frame returns empty string", SpellEffectManager.getSpellAssetId(99) $= "");

// 4. Create mock battle scene, avatar, and units
if (isObject(t2dScene))
{
   t2dScene.clear();
   t2dScene.delete();
}
new Scene(t2dScene);

if (isObject(heroicon))
   heroicon.delete();
new Sprite(heroicon)
{
   scene = t2dScene;
   Position = "-40 -16";
   Size = "16 16";
   SceneLayer = 31;
};

if (isObject(blood))
   blood.delete();
new Sprite(blood)
{
   scene = t2dScene;
   Position = "-50 -80";
   Size = "8 8";
   SceneLayer = 1;
};

TurnManager.initStateMachine();
game.reset();
game.setState($gameBattleState);
game.setActivePlayer(0);
game.setPieceCount(3);

// Piece 1: friendly unit
$nappulaSprite[1] = new Sprite()
{
   scene = t2dScene;
   Position = "-12 4";
   Size = "8 8";
   SceneLayer = 2;
   alive = 1;
   groupid = 0;
   hp = 3;
   defence = 2;
   defenceMAX = 2;
   creatureID = 0;
   facing = "w";
   index = 1;
   boardX = 1;
   boardY = 1;
};

// Piece 2: living enemy unit A
$nappulaSprite[2] = new Sprite()
{
   scene = t2dScene;
   Position = "12 4";
   Size = "8 8";
   SceneLayer = 2;
   alive = 1;
   groupid = 1;
   hp = 3;
   defence = 2;
   defenceMAX = 2;
   creatureID = 4;
   facing = "w";
   index = 2;
   boardX = 3;
   boardY = 1;
};

// Piece 3: dead enemy unit B (must NOT receive particles)
$nappulaSprite[3] = new Sprite()
{
   scene = t2dScene;
   Position = "12 -12";
   Size = "8 8";
   SceneLayer = 3;
   alive = 0;
   groupid = 1;
   hp = 0;
   defence = 2;
   defenceMAX = 2;
   creatureID = 5;
   index = 3;
   boardX = 3;
   boardY = 3;
};

// 5. Test playSpellEffect placement, layers, and ParticlePlayer playback
SpellEffectManager.clearAll();
%spawned3 = SpellEffectManager.playSpellEffect(3, "");
assertSpellCond("Taikamakkara spawns 1 shield effect on living friendly", %spawned3 == 1 && SpellEffectManager.activeEffects.getCount() == 1);
%fx3 = SpellEffectManager.activeEffects.getObject(0);
assertSpellCond("Shield effect uses Archon:spell_shield", %fx3.getParticleAsset() $= "Archon:spell_shield");
assertSpellCond("Shield effect renders on board layer 1", %fx3.getSceneLayer() == 1);
assertSpellCond("Shield effect auto-plays on spawn", %fx3.getIsPlaying());
assertSpellCond("Shield effect tracks stop & cleanup events", isEventPending(%fx3.stopEventId) && isEventPending(%fx3.cleanupEventId));

SpellEffectManager.clearAll();
assertSpellCond("clearAll removes all tracked effects", SpellEffectManager.activeEffects.getCount() == 0);

// Avatar mana spells (Frames 4 & 7) and spell power (Frame 5)
%spawned4 = SpellEffectManager.playSpellEffect(4, "");
assertSpellCond("Taikahattu spawns 1 mana effect on avatar", %spawned4 == 1 && SpellEffectManager.activeEffects.getCount() == 1);
%fx4 = SpellEffectManager.activeEffects.getObject(0);
assertSpellCond("Mana effect uses Archon:spell_mana", %fx4.getParticleAsset() $= "Archon:spell_mana");
assertSpellCond("Mana effect renders on avatar layer 28", %fx4.getSceneLayer() == 28);
assertSpellCond("Mana effect positioned on heroicon", t2dVectorCompare(%fx4.getPosition(), heroicon.getPosition(), 0.01));

SpellEffectManager.clearAll();
%spawned5 = SpellEffectManager.playSpellEffect(5, "");
assertSpellCond("Taikatikkari spawns 1 power effect on avatar", %spawned5 == 1 && SpellEffectManager.activeEffects.getCount() == 1);
assertSpellCond("Power effect uses Archon:spell_power", SpellEffectManager.activeEffects.getObject(0).getParticleAsset() $= "Archon:spell_power");

SpellEffectManager.clearAll();
%spawned7 = SpellEffectManager.playSpellEffect(7, "");
assertSpellCond("Voikukan juuri spawns 1 mana effect on avatar", %spawned7 == 1 && SpellEffectManager.activeEffects.getCount() == 1);
assertSpellCond("Voikukan juuri uses Archon:spell_mana", SpellEffectManager.activeEffects.getObject(0).getParticleAsset() $= "Archon:spell_mana");

// Volcano (Frame 6): targets only living enemies (Piece 2 alive, Piece 3 dead)
SpellEffectManager.clearAll();
%spawned6 = SpellEffectManager.playSpellEffect(6, "");
assertSpellCond("Tulivuori spawns only on living enemy units", %spawned6 == 1 && SpellEffectManager.activeEffects.getCount() == 1);
%fx6 = SpellEffectManager.activeEffects.getObject(0);
assertSpellCond("Volcano effect uses Archon:spell_volcano", %fx6.getParticleAsset() $= "Archon:spell_volcano");
assertSpellCond("Volcano effect positioned at living enemy", t2dVectorCompare(%fx6.getPosition(), $nappulaSprite[2].getPosition(), 0.01));

// Vatsatauti (Frame 8): targets clicked unit
SpellEffectManager.clearAll();
%spawned8 = SpellEffectManager.playSpellEffect(8, $nappulaSprite[2]);
assertSpellCond("Vatsatauti spawns 1 disease effect on target", %spawned8 == 1 && SpellEffectManager.activeEffects.getCount() == 1);
assertSpellCond("Disease effect uses Archon:spell_disease", SpellEffectManager.activeEffects.getObject(0).getParticleAsset() $= "Archon:spell_disease");

// 6. Test pause-awareness and stop/cleanup helpers
t2dScene.setScenePause(true);
%fxPause = SpellEffectManager.activeEffects.getObject(0);
%prevStopId = %fxPause.stopEventId;
SpellEffectManager.stopEffectEmission(%fxPause);
assertSpellCond("stopEffectEmission postpones when scene is paused", %fxPause.getIsPlaying() && isEventPending(%fxPause.stopEventId) && %fxPause.stopEventId != %prevStopId);
t2dScene.setScenePause(false);

SpellEffectManager.stopEffectEmission(%fxPause);
assertSpellCond("stopEffectEmission executes and clears stopEventId when unpaused", %fxPause.stopEventId $= "" && !isEventPending(%fxPause.stopEventId));
SpellEffectManager.cleanupEffect(%fxPause);
assertSpellCond("cleanupEffect removes from activeEffects", SpellEffectManager.activeEffects.getCount() == 0);

// 7. Test end-to-end integration through CombatEngine::castRevealedSpell for all 6 spells
if (!isObject(board))
   new ScriptObject(board);
$board = board;
CombatEngine.initAnimStates();

function makeMockSpellCard(%frame)
{
   %c = new Sprite()
   {
      scene = t2dScene;
      class = "Card";
      Position = "38 12";
      Size = "12 18";
      spellFrame = %frame;
   };
   return %c;
}

// Cast all non-lethal spells via CombatEngine::castRevealedSpell
%frameList = "3 4 5 7 8";
%catList = "board avatar avatar avatar unit";
%assetCheckList = "Archon:spell_shield Archon:spell_mana Archon:spell_power Archon:spell_mana Archon:spell_disease";
for (%k = 0; %k < getWordCount(%frameList); %k++)
{
   %f = getWord(%frameList, %k);
   %cat = getWord(%catList, %k);
   %expectedAsset = getWord(%assetCheckList, %k);
   %target = (%cat $= "unit") ? $nappulaSprite[1] : "";

   SpellEffectManager.clearAll();
   $specialcard[1] = makeMockSpellCard(%f);
   $topCardIndex = 1;
   game.setCanCastSpell(true);
   game.setSpellRevealed(true);
   %castOk = CombatEngine.castRevealedSpell(%cat, %target);
   assertSpellCond("CombatEngine castRevealedSpell(frame " @ %f @ ") succeeded", %castOk == true);
   assertSpellCond("Frame " @ %f @ " spawned expected asset " @ %expectedAsset,
      SpellEffectManager.activeEffects.getCount() == 1 &&
      SpellEffectManager.activeEffects.getObject(0).getParticleAsset() $= %expectedAsset);
   if (isObject($specialcard[1]))
      $specialcard[1].safeDelete();
}
assertSpellCond("Vatsatauti set skipNextTurn flag", $nappulaSprite[1].skipNextTurn == 1);

// 8. Test TurnManager::nextTurn disease skip spawns disease particle effect
SpellEffectManager.clearAll();
$nappulaSprite[2].alive = 1;
$nappulaSprite[2].hp = 2;
$nappulaSprite[1].skipNextTurn = 1;
game.setSelectedPiece(0);
game.setState($gameBattleState);
game.setTurnQueue("1 2");
TurnManager.nextTurn();
assertSpellCond("TurnManager::nextTurn cleared skipNextTurn flag", $nappulaSprite[1].skipNextTurn == 0);
assertSpellCond("TurnManager::nextTurn spawned disease puff on skipped unit", SpellEffectManager.activeEffects.getCount() == 1 && SpellEffectManager.activeEffects.getObject(0).getParticleAsset() $= "Archon:spell_disease");

// Also verify lethal Tulivuori (Frame 6) still spawns volcano before marking last enemy dead
SpellEffectManager.clearAll();
$nappulaSprite[2].hp = 1;
$specialcard[1] = makeMockSpellCard(6);
$topCardIndex = 1;
game.setCanCastSpell(true);
game.setSpellRevealed(true);
%castVolcano = CombatEngine.castRevealedSpell("board", "");
assertSpellCond("CombatEngine castRevealedSpell(Tulivuori) succeeded", %castVolcano == true);
assertSpellCond("Tulivuori spawned volcano effect even on lethal cast", SpellEffectManager.activeEffects.getCount() == 1 && SpellEffectManager.activeEffects.getObject(0).getParticleAsset() $= "Archon:spell_volcano");
assertSpellCond("Lethal Tulivuori marked enemy dead", $nappulaSprite[2].alive == 0);
if (isObject($specialcard[1]))
   $specialcard[1].safeDelete();

// 9. Test TurnManager::endBattle clears active spell effects
TurnManager.endBattle();
if (isEventPending(TurnManager.endBattleEvent))
   cancel(TurnManager.endBattleEvent);
assertSpellCond("TurnManager::endBattle cleared active spell effects", SpellEffectManager.activeEffects.getCount() == 0);

// 10. Verify real scheduled lifecycle progression (emit stop -> cleanup)
$timedFx = SpellEffectManager.spawnEffectAtPosition("Archon:spell_shield", "0 0", 1, 150, 450);
assertSpellCond("Timed effect spawned for lifecycle test", isObject($timedFx) && SpellEffectManager.activeEffects.getCount() == 1);
schedule(700, 0, "finishTimedSpellParticleTest");

function finishTimedSpellParticleTest()
{
   assertSpellCond("Scheduled lifecycle automatically deleted effect and drained activeEffects",
      !isObject($timedFx) && SpellEffectManager.activeEffects.getCount() == 0);

   t2dScene.clear();
   t2dScene.delete();

   logSpellResult("==========================================");
   logSpellResult("ALL SPELL PARTICLE VERIFICATION TESTS PASSED!");
   logSpellResult("==========================================");

   if (isObject($spellTestFile))
   {
      $spellTestFile.close();
      $spellTestFile.delete();
   }

   quit();
}
