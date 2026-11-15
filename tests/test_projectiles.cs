//-----------------------------------------------------------------------------
// tests/test_projectiles.cs
// Verification suite for Projectile datablocks, ParticleAssets, Trajectory Math,
// Per-frame Flight Loop, Impact Damage/Effects, Memory-Safe Cleanup,
// Animation Frame Hook, and Ranged CombatEngine integration.
//-----------------------------------------------------------------------------

$Scripts::ignoreDSOs = true;
setLogMode(1);
$Archon::skipSplash = true;

function logProjResult(%msg)
{
   echo(%msg);
}

function failProjTest(%msg)
{
   logProjResult("TEST_FAIL: " @ %msg);
   quit();
}

function passProjTest(%msg)
{
   logProjResult("TEST_PASS: " @ %msg);
}

function assertProjCond(%label, %cond)
{
   if (!%cond)
      failProjTest(%label);
   passProjTest(%label);
}

setCompanyAndProduct("Torque Game Engines", "Archonita");
ModuleDatabase.EchoInfo = false;
AssetDatabase.EchoInfo = false;

ModuleDatabase.scanModules(pathConcat(getMainDotCsDir(), "library"));
ModuleDatabase.scanModules(pathConcat(getMainDotCsDir(), "Archon"));
ModuleDatabase.LoadExplicit("AppCore");

logProjResult("==========================================");
logProjResult("Archonita - Projectiles & Particle Trails Suite");
logProjResult("==========================================");

// ---------------------------------------------------------------------------
// PHASE 1: Datablocks, ParticleAssets, Vector/Trig Math & Angle Orientation
// ---------------------------------------------------------------------------

assertProjCond("ProjectileTrailParticleData exists (ParticleData)", isObject(ProjectileTrailParticleData) && ProjectileTrailParticleData.class $= "ParticleData");
assertProjCond("ProjectileSparkParticleData exists (ParticleData)", isObject(ProjectileSparkParticleData) && ProjectileSparkParticleData.class $= "ParticleData");
assertProjCond("ProjectileHitParticleData exists (ParticleData)", isObject(ProjectileHitParticleData) && ProjectileHitParticleData.class $= "ParticleData");
assertProjCond("ProjectileTrailEmitterData exists (ParticleEmitterData)", isObject(ProjectileTrailEmitterData) && ProjectileTrailEmitterData.class $= "ParticleEmitterData");
assertProjCond("ProjectileSparkEmitterData exists (ParticleEmitterData)", isObject(ProjectileSparkEmitterData) && ProjectileSparkEmitterData.class $= "ParticleEmitterData");
assertProjCond("ProjectileHitEmitterData exists (ParticleEmitterData)", isObject(ProjectileHitEmitterData) && ProjectileHitEmitterData.class $= "ParticleEmitterData");
assertProjCond("DefaultProjectileTemplate exists", isObject(DefaultProjectileTemplate) && DefaultProjectileTemplate.class $= "ProjectileTemplate");

%trailAssetId = "Archon:projectile_trail";
assertProjCond("Declared ParticleAsset " @ %trailAssetId, AssetDatabase.isDeclaredAsset(%trailAssetId));
assertProjCond("Asset type is ParticleAsset for " @ %trailAssetId, AssetDatabase.getAssetType(%trailAssetId) $= "ParticleAsset");
%trailAssetObj = AssetDatabase.acquireAsset(%trailAssetId);
assertProjCond("Acquired projectile_trail ParticleAsset with 2 emitters", isObject(%trailAssetObj) && %trailAssetObj.getEmitterCount() == 2);
AssetDatabase.releaseAsset(%trailAssetId);

%hitAssetId = "Archon:projectile_hit";
assertProjCond("Declared ParticleAsset " @ %hitAssetId, AssetDatabase.isDeclaredAsset(%hitAssetId));
assertProjCond("Asset type is ParticleAsset for " @ %hitAssetId, AssetDatabase.getAssetType(%hitAssetId) $= "ParticleAsset");
%hitAssetObj = AssetDatabase.acquireAsset(%hitAssetId);
assertProjCond("Acquired projectile_hit ParticleAsset with 2 emitters", isObject(%hitAssetObj) && %hitAssetObj.getEmitterCount() == 2);
AssetDatabase.releaseAsset(%hitAssetId);

// Verify mAtan2 returns radians across all 4 quadrants and mRadToDeg matches degrees
assertProjCond("mAtan2(0, 1) ~= 0", mAbs(mAtan2(0, 1)) < 0.001);
assertProjCond("mAtan2(1, 0) ~= pi/2", mAbs(mRadToDeg(mAtan2(1, 0)) - 90) < 0.1);
assertProjCond("mAtan2(0, -1) ~= pi", mAbs(mAbs(mRadToDeg(mAtan2(0, -1))) - 180) < 0.1);
assertProjCond("mAtan2(-1, 0) ~= -pi/2", mAbs(mRadToDeg(mAtan2(-1, 0)) + 90) < 0.1);
assertProjCond("mAtan2(1, 1) ~= pi/4", mAbs(mRadToDeg(mAtan2(1, 1)) - 45) < 0.1);

// Set up clean test scene
if (isObject(t2dScene))
{
   t2dScene.clear();
   t2dScene.delete();
}
new Scene(t2dScene);

$testUnit = new Sprite()
{
   scene = t2dScene;
   class = "RangedUnit";
   Position = "0 0";
   Size = "8 8";
   SceneLayer = 2;
   muzzleOffsetDistance = 0;
   muzzleOffsetY = 0;
   projectileSpeed = 40;
   creatureID = 1;
};

// Fire East (+X) -> 0 deg
%pEast = $testUnit.fireProjectile(20, 0);
assertProjCond("East projectile spawned & is Sprite", isObject(%pEast) && %pEast.getClassName() $= "Sprite");
assertProjCond("East projectile oriented at ~0 deg", mAbs(%pEast.getAngle() - 0) < 0.5);
assertProjCond("East projectile velocity points +X", getWord(%pEast.velocity, 0) > 39.0 && mAbs(getWord(%pEast.velocity, 1)) < 0.01);
assertProjCond("Projectile spawned trailing ParticlePlayer", isObject(%pEast.trailEffect) && %pEast.trailEffect.getParticleAsset() $= "Archon:projectile_trail");
%pEastTrail = %pEast.trailEffect;
%pEast.cleanup("test_cleanup");
assertProjCond("Cleanup deletes East projectile and trailEffect", !isObject(%pEast) && !isObject(%pEastTrail));

// Fire North (+Y) -> 90 deg
%pNorth = $testUnit.fireProjectile(0, 20);
assertProjCond("North projectile oriented at ~90 deg", isObject(%pNorth) && mAbs(%pNorth.getAngle() - 90) < 0.5);
%pNorth.cleanup("test_cleanup");

// Fire West (-X) -> +-180 deg
%pWest = $testUnit.fireProjectile(-20, 0);
assertProjCond("West projectile oriented at ~+-180 deg", isObject(%pWest) && mAbs(mAbs(%pWest.getAngle()) - 180) < 0.5);
%pWest.cleanup("test_cleanup");

// Fire South (-Y) -> -90 deg
%pSouth = $testUnit.fireProjectile(0, -20);
assertProjCond("South projectile oriented at ~-90 deg", isObject(%pSouth) && mAbs(%pSouth.getAngle() + 90) < 0.5);
%pSouth.cleanup("test_cleanup");

// Fire Diagonal NE (+X, +Y) -> 45 deg
%pNE = $testUnit.fireProjectile(20, 20);
assertProjCond("NE diagonal projectile oriented at ~45 deg", isObject(%pNE) && mAbs(%pNE.getAngle() - 45) < 0.5);
%pNE.cleanup("test_cleanup");

// Muzzle offset verification
$offsetUnit = new Sprite()
{
   scene = t2dScene;
   class = "RangedUnit";
   Position = "10 10";
   Size = "8 8";
   SceneLayer = 2;
   muzzleOffsetDistance = 2.0;
   muzzleOffsetY = 0.0;
   projectileSpeed = 40;
   creatureID = 9; // Red Dragon -> fire visual
};
%pMuzzle = $offsetUnit.fireProjectile(20, 10);
assertProjCond("Muzzle offset placed startPos at (12, 10)", t2dVectorCompare(%pMuzzle.getPosition(), "12 10", 0.1));
assertProjCond("CreatureID 9 (Red Dragon) resolves fire visualStyle", %pMuzzle.visualType $= "fire");
%pMuzzle.cleanup("test_cleanup");
$offsetUnit.delete();

// ---------------------------------------------------------------------------
// PHASE 2: Per-Frame Flight Movement, Trail Sync, Impact Damage & Auto-Delete
// ---------------------------------------------------------------------------

if (isObject(SpellEffectManager))
   SpellEffectManager.clearAll();

$flightTarget = new Sprite()
{
   scene = t2dScene;
   Position = "18 0";
   Size = "8 8";
   SceneLayer = 2;
   hp = 5;
   alive = 1;
};

$testUnit.projectileSpeed = 45;
$flightProj = $testUnit.fireProjectile(18, 0, 2, $flightTarget);
$flightTrail = $flightProj.trailEffect;
assertProjCond("Phase 2 projectile spawned with speed 45 toward (18,0)", isObject($flightProj) && isObject($flightTrail));

schedule(120, 0, "checkMidFlightPhase2");

function checkMidFlightPhase2()
{
   assertProjCond("Mid-flight projectile still active at 120ms", isObject($flightProj));
   %posX = getWord($flightProj.getPosition(), 0);
   assertProjCond("Mid-flight projectile advanced along +X (" @ %posX @ ")", %posX > 1.0 && %posX < 17.5);
   assertProjCond("Trail ParticlePlayer follows projectile", isObject($flightTrail) && VectorDist($flightTrail.getPosition(), $flightProj.getPosition()) < 0.5);

   schedule(500, 0, "checkImpactPhase2");
}

function checkImpactPhase2()
{
   assertProjCond("Projectile self-deleted after impact", !isObject($flightProj));
   assertProjCond("Trail ParticlePlayer deleted on projectile impact cleanup", !isObject($flightTrail));
   assertProjCond("Target took projectile impact damage (hp 5 -> 3)", $flightTarget.hp == 3);
   $flightTarget.delete();
   if (isObject(SpellEffectManager))
      SpellEffectManager.clearAll();

   startPhase3SafetyTests();
}

// ---------------------------------------------------------------------------
// PHASE 3: Memory Safety Cleanup (Target Invalidation & Max Flight Timeout)
// ---------------------------------------------------------------------------

function startPhase3SafetyTests()
{
   // 3A. Target Invalidation Cleanup
   $invalidTarget = new Sprite()
   {
      scene = t2dScene;
      Position = "60 0";
      Size = "8 8";
      hp = 3;
      alive = 1;
   };
   $testUnit.projectileSpeed = 20;
   $invalidProj = $testUnit.fireProjectile(60, 0, 1, $invalidTarget);
   $invalidProj.trackTargetUnit = 1;
   $invalidTrail = $invalidProj.trailEffect;
   assertProjCond("Target invalidation test projectile active", isObject($invalidProj) && isObject($invalidTrail));

   schedule(70, 0, "invalidateTargetNow");

   // 3B. Max Flight Duration Timeout Cleanup
   $timeoutProj = $testUnit.fireProjectile(500, 0);
   $timeoutProj.maxFlightDuration = 120;
   $timeoutTrail = $timeoutProj.trailEffect;
   assertProjCond("Timeout test projectile active with maxFlightDuration=120", isObject($timeoutProj) && isObject($timeoutTrail));

   schedule(250, 0, "checkPhase3Safety");
}

function invalidateTargetNow()
{
   if (isObject($invalidTarget))
      $invalidTarget.delete();
}

function checkPhase3Safety()
{
   assertProjCond("Projectile cleaned up immediately when target became invalid", !isObject($invalidProj) && !isObject($invalidTrail));
   assertProjCond("Projectile cleaned up automatically on maxFlightDuration timeout", !isObject($timeoutProj) && !isObject($timeoutTrail));

   startPhase4AnimHookTest();
}

// ---------------------------------------------------------------------------
// PHASE 4: Attack Animation Frame Hook Trigger
// ---------------------------------------------------------------------------

function startPhase4AnimHookTest()
{
   $animTarget = new Sprite()
   {
      scene = t2dScene;
      Position = "50 0";
      Size = "8 8";
      hp = 4;
      alive = 1;
   };

   $animUnit = new Sprite()
   {
      scene = t2dScene;
      class = "RangedUnit";
      Position = "-10 0";
      Size = "8 8";
      SceneLayer = 2;
      projectileSpeed = 40;
      creatureID = 1;
   };

   $animUnit.playAttackWithProjectile("Archon:archerAttackAnimation_e", 50, 0, 2, 1, $animTarget);
   assertProjCond("playAttackWithProjectile armed pendingShotActive and frame callback",
      $animUnit.pendingShotActive == 1 && $animUnit.frameChangeCallbackEnabled == 1);

   schedule(380, 0, "checkPhase4AnimHook");
}

function checkPhase4AnimHook()
{
   assertProjCond("Animation frame callback triggered projectile launch",
      isObject($animUnit.lastFiredProjectile) && $animUnit.pendingShotActive == 0);

   if (isObject($animUnit.lastFiredProjectile))
      $animUnit.lastFiredProjectile.cleanup("test_cleanup");
   $animUnit.delete();
   $animTarget.delete();

   startPhase5CombatEngineTest();
}

// ---------------------------------------------------------------------------
// PHASE 5: Piece & CombatEngine Ranged Attack Integration
// ---------------------------------------------------------------------------

function startPhase5CombatEngineTest()
{
   if (!isObject(board))
      new ScriptObject(board);
   $board = board;

   if (isObject(blood))
      blood.delete();
   new Sprite(blood)
   {
      scene = t2dScene;
      Position = "-50 -80";
      Size = "8 8";
      SceneLayer = 1;
   };

   if (isObject(heroicon))
      heroicon.delete();
   new Sprite(heroicon)
   {
      scene = t2dScene;
      Position = "-40 -16";
      Size = "16 16";
      SceneLayer = 31;
   };

   TurnManager.initStateMachine();
   CombatEngine.initAnimStates();
   game.reset();
   game.setState($gameBattleState);
   game.setTurn(1);
   game.setActivePlayer(0);
   game.setPieceCount(2);
   game.setSelectedPiece(1);

   $nappulaSprite[1] = new Sprite()
   {
      scene = t2dScene;
      class = "Piece";
      superclass = "Nappula";
      Position = "-12 4";
      Size = "8 8";
      SceneLayer = 2;
      alive = 1;
      groupid = 0;
      hp = 2;
      attack = 3;
      defence = 2;
      creatureID = 1; // archer
      missile = 1;
      index = 1;
      boardX = 1;
      boardY = 1;
      positionOnBoard = "1 1";
   };

   $nappulaSprite[2] = new Sprite()
   {
      scene = t2dScene;
      class = "Piece";
      superclass = "Nappula";
      Position = "12 4";
      Size = "8 8";
      SceneLayer = 2;
      alive = 1;
      groupid = 1;
      hp = 3;
      attack = 2;
      defence = 1;
      creatureID = 4; // skeletonwarrior
      missile = 0;
      index = 2;
      boardX = 3;
      boardY = 1;
      positionOnBoard = "3 1";
   };

   CombatEngine.unlockInput();
   CombatEngine.doAttack($nappulaSprite[2], game.getTurn());

   assertProjCond("CombatEngine::doAttack launched ranged combat projectile",
      isObject(CombatEngine.activeCombatProjectile) &&
      CombatEngine.pendingAttackIsRanged == 1 &&
      CombatEngine.isInputLocked() == true);

   schedule(750, 0, "checkPhase5CombatComplete");
}

function checkPhase5CombatComplete()
{
   assertProjCond("Combat projectile landed and deleted itself", !isObject(CombatEngine.activeCombatProjectile));
   assertProjCond("CombatEngine.activeProjectiles drained to 0", CombatEngine.activeProjectiles.getCount() == 0);
   assertProjCond("CombatEngine resolved ranged attack and unlocked input",
      CombatEngine.pendingAttackResolved == 1 && CombatEngine.isInputLocked() == false);

   // Test TurnManager::endBattle cleanup
   $leftoverProj = $testUnit.fireProjectile(50, 50);
   assertProjCond("Leftover projectile tracked in CombatEngine.activeProjectiles before endBattle",
      CombatEngine.activeProjectiles.getCount() == 1);
   TurnManager.endBattle();
   if (isEventPending(TurnManager.endBattleEvent))
      cancel(TurnManager.endBattleEvent);
   assertProjCond("TurnManager::endBattle cleans up all in-flight projectiles",
      !isObject($leftoverProj) && CombatEngine.activeProjectiles.getCount() == 0);

   finishProjectileTests();
}

function finishProjectileTests()
{
   // Verify t2dScene.clear() safely frees an active in-flight projectile and its trail
   $clearProj = $testUnit.fireProjectile(80, 80);
   $clearTrail = $clearProj.trailEffect;
   assertProjCond("Active in-flight projectile exists prior to scene clear", isObject($clearProj) && isObject($clearTrail));

   if (isObject($testUnit))
      $testUnit.delete();

   t2dScene.clear();
   assertProjCond("t2dScene.clear() cleanly destroyed active projectile & trail",
      !isObject($clearProj) && !isObject($clearTrail) && CombatEngine.activeProjectiles.getCount() == 0);
   t2dScene.delete();

   logProjResult("==========================================");
   logProjResult("ALL PROJECTILE & PARTICLE TRAIL TESTS PASSED!");
   logProjResult("==========================================");

   quit();
}
