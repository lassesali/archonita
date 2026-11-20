//-----------------------------------------------------------------------------
// tests/test_particle_optimizations.cs
// Verification suite for particle atlas consolidation, emitter optimizations,
// blood/magicButton/mouseTrail idle lifecycle, projectile trail settings,
// AoE spell scaling, and persistent particle density preference.
//-----------------------------------------------------------------------------

$Scripts::ignoreDSOs = true;
setLogMode(1);
$Archon::skipSplash = true;

function logOptResult(%msg)
{
   echo(%msg);
}

function failOptTest(%msg)
{
   logOptResult("TEST_FAIL: " @ %msg);
   quit();
}

function passOptTest(%msg)
{
   logOptResult("TEST_PASS: " @ %msg);
}

function assertOptCond(%label, %cond)
{
   if (!%cond)
   {
      failOptTest(%label);
      return;
   }
   passOptTest(%label);
}

setCompanyAndProduct("Torque Game Engines", "Archonita");
ModuleDatabase.EchoInfo = false;
AssetDatabase.EchoInfo = false;

ModuleDatabase.scanModules(pathConcat(getMainDotCsDir(), "library"));
ModuleDatabase.scanModules(pathConcat(getMainDotCsDir(), "Archon"));
ModuleDatabase.LoadExplicit("AppCore");

logOptResult("==========================================");
logOptResult("Archonita - Particle Optimizations Suite");
logOptResult("==========================================");

// Save initial particle density preference to restore cleanly before exit
$savedInitialParticleDensity = ($pref::T2D::ParticlePlayerEmissionRateScale !$= "")
   ? $pref::T2D::ParticlePlayerEmissionRateScale
   : 1.0;

// 1. Verify Archon:particlesAtlasImageMap declaration and 37 explicit frames
%atlasId = "Archon:particlesAtlasImageMap";
assertOptCond("particlesAtlasImageMap is declared as ImageAsset",
   AssetDatabase.isDeclaredAsset(%atlasId) && AssetDatabase.getAssetType(%atlasId) $= "ImageAsset");
%atlasObj = AssetDatabase.acquireAsset(%atlasId);
assertOptCond("particlesAtlasImageMap has exactly 37 frames",
   isObject(%atlasObj) && %atlasObj.getFrameCount() == 37);
AssetDatabase.releaseAsset(%atlasId);

// 2. Verify all 11 Archon particle assets point to particlesAtlasImageMap with valid numeric frames
%allAssets = "Archon:blood Archon:bubbles Archon:magic_button_glow Archon:magic_trail Archon:projectile_trail Archon:projectile_hit Archon:spell_disease Archon:spell_mana Archon:spell_power Archon:spell_shield Archon:spell_volcano";
%assetCount = getWordCount(%allAssets);
for (%a = 0; %a < %assetCount; %a++)
{
   %assetId = getWord(%allAssets, %a);
   assertOptCond("Declared ParticleAsset " @ %assetId,
      AssetDatabase.isDeclaredAsset(%assetId) && AssetDatabase.getAssetType(%assetId) $= "ParticleAsset");
   %assetObj = AssetDatabase.acquireAsset(%assetId);
   %eCount = isObject(%assetObj) ? %assetObj.getEmitterCount() : 0;
   if (%assetId $= "Archon:projectile_trail" || %assetId $= "Archon:projectile_hit")
      assertOptCond(%assetId @ " has 2 emitters", %eCount == 2);
   else if (%assetId $= "Archon:spell_volcano")
      assertOptCond(%assetId @ " has 3 emitters", %eCount == 3);
   else
      assertOptCond(%assetId @ " has >= 1 emitter", %eCount >= 1);

   for (%e = 0; %e < %eCount; %e++)
   {
      %em = %assetObj.getEmitter(%e);
      %img = isObject(%em) ? %em.getImage() : "";
      %frm = isObject(%em) ? %em.getImageFrame() : -1;
      assertOptCond(%assetId @ " emitter " @ %e @ " uses atlas with valid frame (" @ %frm @ ")",
         isObject(%em) && %img $= %atlasId && !%em.isUsingNamedImageFrame() && %frm >= 0 && %frm < 37);
   }
   AssetDatabase.releaseAsset(%assetId);
}

// 3. Verify Archon:spell_volcano emitter ordering (VolcanicSmoke normal blend first, then additive emitters)
%volcanoAsset = AssetDatabase.acquireAsset("Archon:spell_volcano");
%vEm0 = %volcanoAsset.getEmitter(0);
%vEm1 = %volcanoAsset.getEmitter(1);
%vEm2 = %volcanoAsset.getEmitter(2);
assertOptCond("spell_volcano emitter 0 is VolcanicSmoke with IntenseParticles=false",
   isObject(%vEm0) && %vEm0.getEmitterName() $= "VolcanicSmoke" && !%vEm0.getIntenseParticles());
assertOptCond("spell_volcano emitters 1 & 2 are LavaFlames/LavaEmbers with IntenseParticles=true",
   isObject(%vEm1) && %vEm1.getEmitterName() $= "LavaFlames" && %vEm1.getIntenseParticles() &&
   isObject(%vEm2) && %vEm2.getEmitterName() $= "LavaEmbers" && %vEm2.getIntenseParticles());
AssetDatabase.releaseAsset("Archon:spell_volcano");

// Set up clean scene for runtime lifecycle tests
if (isObject(t2dScene))
{
   t2dScene.clear();
   t2dScene.delete();
}
new Scene(t2dScene);
sceneWindow2D.setScene(t2dScene);
TurnManager.initStateMachine();
CombatEngine.initAnimStates();
game.reset();
$ResolutionManager.setParticleDensity(1.0, true);

// 4. Test blood ParticlePlayer lifecycle & Sprite fallback safety
if (isObject(blood))
   blood.delete();
new ParticlePlayer(blood)
{
   scene = t2dScene;
   Particle = "Archon:blood";
   Position = "-50 -80";
   Size = "11.3 10.8";
   SceneLayer = 1;
};
blood.stop(false, false);
assertOptCond("blood ParticlePlayer stops cleanly after board-style stop",
   isObject(blood) && !blood.getIsPlaying());

CombatEngine.triggerBloodEffect("10 20", 120);
assertOptCond("CombatEngine.triggerBloodEffect positions and plays blood ParticlePlayer",
   blood.getIsPlaying() && t2dVectorCompare(blood.getPosition(), "10 20", 0.01));

CombatEngine.hideBlood();
assertOptCond("CombatEngine.hideBlood pauses blood emission without killing object",
   isObject(blood) && blood.getEmitterPaused(0) == true);

// Sprite fallback safety for blood
blood.delete();
new Sprite(blood)
{
   scene = t2dScene;
   Position = "0 0";
   Size = "8 8";
   SceneLayer = 1;
};
CombatEngine.triggerBloodEffect("4 6", 120);
assertOptCond("CombatEngine.triggerBloodEffect safely positions Sprite blood fallback",
   t2dVectorCompare(blood.getPosition(), "4 6", 0.01));
CombatEngine.hideBlood();
assertOptCond("CombatEngine.hideBlood moves Sprite blood fallback off-screen to -50 -80",
   t2dVectorCompare(blood.getPosition(), "-50 -80", 0.01));
blood.delete();

// 5. Test magicButtonEffect creation, self-delete fix, play/stop gating, and EmissionRateScale
if (isObject(magicButtonEffect))
   magicButtonEffect.delete();
game.setState($gameBattleState);
game.setActivePlayer(0);
game.setCanCastSpell(false);
game.setSpellRevealed(false);
$topCardIndex = 1;

CombatEngine.updateMagicButtonParticles();
assertOptCond("magicButtonEffect created while ineligible does not self-delete and starts stopped",
   isObject(magicButtonEffect) && !magicButtonEffect.getIsPlaying());

game.setCanCastSpell(true);
CombatEngine.updateMagicButtonParticles();
assertOptCond("magicButtonEffect plays when eligible with EmissionRateScale=1.0",
   magicButtonEffect.getIsPlaying() &&
   !magicButtonEffect.getEmitterPaused(0) &&
   mAbs(magicButtonEffect.getEmissionRateScale() - 1.0) < 0.001);

game.setSpellRevealed(true);
CombatEngine.updateMagicButtonParticles();
assertOptCond("magicButtonEffect scales EmissionRateScale to 1.7 when spell is revealed",
   mAbs(magicButtonEffect.getEmissionRateScale() - 1.7) < 0.001);

game.setCanCastSpell(false);
CombatEngine.updateMagicButtonParticles();
assertOptCond("magicButtonEffect pauses emission when ineligible",
   magicButtonEffect.getEmitterPaused(0) == true);

// 6. Test Projectile trail ParticleInterpolation=false and atlas image
%projUnit = new Sprite()
{
   scene = t2dScene;
   class = "RangedUnit";
   Position = "0 0";
   Size = "8 8";
   SceneLayer = 2;
   projectileSpeed = 40;
   creatureID = 1;
};
%proj = %projUnit.fireProjectile(25, 0);
assertOptCond("Projectile uses particlesAtlasImageMap and trail disables ParticleInterpolation",
   isObject(%proj) &&
   %proj.getImage() $= %atlasId &&
   isObject(%proj.trailEffect) &&
   !%proj.trailEffect.getParticleInterpolation());
%proj.cleanup("test_cleanup");
%projUnit.delete();

// 7. Test SpellEffectManager.spawnEffectOnGroup AoE scaling (2 -> 1.0, 6 -> 0.5, 10 -> 0.45)
function spawnAoEScalingMockUnits(%count)
{
   for (%i = 1; %i <= 10; %i++)
   {
      if (isObject($nappulaSprite[%i]))
         $nappulaSprite[%i].delete();
      $nappulaSprite[%i] = "";
   }
   game.setPieceCount(%count);
   for (%i = 1; %i <= %count; %i++)
   {
      $nappulaSprite[%i] = new Sprite()
      {
         scene = t2dScene;
         Position = (%i * 4) SPC "0";
         Size = "8 8";
         SceneLayer = 2;
         alive = 1;
         groupid = 1;
      };
   }
}

SpellEffectManager.clearAll();
spawnAoEScalingMockUnits(2);
%cnt2 = SpellEffectManager.spawnEffectOnGroup(1, "Archon:spell_volcano");
%scale2 = (SpellEffectManager.activeEffects.getCount() > 0) ? SpellEffectManager.activeEffects.getObject(0).getEmissionRateScale() : -1;
assertOptCond("AoE spell with 2 living targets uses EmissionRateScale=1.0",
   %cnt2 == 2 && mAbs(%scale2 - 1.0) < 0.001);
SpellEffectManager.clearAll();

spawnAoEScalingMockUnits(6);
%cnt6 = SpellEffectManager.spawnEffectOnGroup(1, "Archon:spell_volcano");
%scale6 = (SpellEffectManager.activeEffects.getCount() > 0) ? SpellEffectManager.activeEffects.getObject(0).getEmissionRateScale() : -1;
assertOptCond("AoE spell with 6 living targets scales EmissionRateScale to 0.5",
   %cnt6 == 6 && mAbs(%scale6 - 0.5) < 0.001);
SpellEffectManager.clearAll();

spawnAoEScalingMockUnits(10);
%cnt10 = SpellEffectManager.spawnEffectOnGroup(1, "Archon:spell_volcano");
%scale10 = (SpellEffectManager.activeEffects.getCount() > 0) ? SpellEffectManager.activeEffects.getObject(0).getEmissionRateScale() : -1;
assertOptCond("AoE spell with 10 living targets clamps EmissionRateScale at 0.45",
   %cnt10 == 10 && mAbs(%scale10 - 0.45) < 0.001);
SpellEffectManager.clearAll();

for (%i = 1; %i <= 10; %i++)
{
   if (isObject($nappulaSprite[%i]))
      $nappulaSprite[%i].delete();
   $nappulaSprite[%i] = "";
}

// 8. Test ResolutionManager particle density preference scaling & round-trip persistence
%liveFx = new ParticlePlayer()
{
   scene = t2dScene;
   Particle = "Archon:magic_button_glow";
   Position = "0 0";
   Size = "8 8";
   SceneLayer = 1;
};
$ResolutionManager.setParticleDensity(0.5, false);
assertOptCond("setParticleDensity(0.5, false) updates pref and scales live ParticlePlayers",
   mAbs($pref::T2D::ParticlePlayerEmissionRateScale - 0.5) < 0.001 &&
   mAbs($ResolutionManager.getParticleDensity() - 0.5) < 0.001 &&
   mAbs(%liveFx.getEmissionRateScale() - 0.5) < 0.001);

$ResolutionManager.setParticleDensity(1.0, true);
$ResolutionManager.loadPreferences();
assertOptCond("loadPreferences restores persisted particle density 0.5",
   mAbs($ResolutionManager.getParticleDensity() - 0.5) < 0.001 &&
   mAbs($pref::T2D::ParticlePlayerEmissionRateScale - 0.5) < 0.001);

$ResolutionManager.setParticleDensity(1.0, true);
%liveFx.delete();

// 9. Test mouseTrail idle pause, resume while draining, and onMouseLeave pause (async schedule)
if (isObject(mouseTrail))
   mouseTrail.delete();
new ParticlePlayer(mouseTrail)
{
   scene = t2dScene;
   Particle = "Archon:magic_trail";
   Position = "0 0";
   Size = "16 16";
   SceneLayer = 1;
};
game.setState($gameMainMenuState);
initMouse();
assertOptCond("mouseTrail starts stopped after initMouse()",
   isObject(mouseTrail) && !mouseTrail.getIsPlaying());

sceneWindow2D.onMouseMove(0, "5 5", 0);
assertOptCond("mouseTrail plays on sceneWindow2D.onMouseMove",
   mouseTrail.getIsPlaying() && !mouseTrail.getEmitterPaused(0));

schedule(260, 0, "checkMouseTrailIdlePhase");

function checkMouseTrailIdlePhase()
{
   assertOptCond("mouseTrail pauses emission after ~260ms idle",
      isObject(mouseTrail) && mouseTrail.getEmitterPaused(0) == true);

   sceneWindow2D.onMouseMove(0, "12 12", 0);
   assertOptCond("mouseTrail resumes immediately on follow-up onMouseMove while draining",
      mouseTrail.getIsPlaying() && !mouseTrail.getEmitterPaused(0));

   sceneWindow2D.onMouseLeave(0, "12 12", 0);
   assertOptCond("mouseTrail pauses on sceneWindow2D.onMouseLeave",
      mouseTrail.getEmitterPaused(0) == true);

   // 10. Live OpenGL particle batching check (via Scene::getBatchStats)
   t2dScene.clear();
   Canvas.setContent(mainScreenGui);
   sceneWindow2D.setScene(t2dScene);
   sceneWindow2D.setCameraPosition(0, 0);
   sceneWindow2D.setCameraSize(100, 75);

   for (%s = 0; %s < 3; %s++)
   {
      $batchTestShield[%s] = new ParticlePlayer()
      {
         scene = t2dScene;
         Particle = "Archon:spell_shield";
         Position = ((%s - 1) * 12) SPC "0";
         Size = "8 8";
         SceneLayer = 1;
      };
   }

   schedule(180, 0, "checkLiveShieldBatchPhase");
}

function checkLiveShieldBatchPhase()
{
   %stats = t2dScene.getBatchStats();
   %flushes = getWord(%stats, 0);
   %isolated = getWord(%stats, 1);
   %blendFlushes = getWord(%stats, 2);
   %colorFlushes = getWord(%stats, 3);
   %alphaFlushes = getWord(%stats, 4);
   %tris = getWord(%stats, 8);
   logOptResult("INFO: 3x Archon:spell_shield (6 emitters) getBatchStats() = [" @ %stats @ "]");

   assertOptCond("3 same-layer spell_shield ParticlePlayers render with batchIsolatedFlush == 0",
      %isolated == 0);
   assertOptCond("3 same-layer spell_shield ParticlePlayers render with batchColorStateFlush == 0",
      %colorFlushes == 0);
   assertOptCond("3 same-layer spell_shield ParticlePlayers submit visible particle triangles",
      %tris > 0);
   assertOptCond("3 same-layer spell_shield ParticlePlayers merge into 1 layer flush",
      %flushes == 1 && %blendFlushes == 0 && %alphaFlushes == 0);

   for (%s = 0; %s < 3; %s++)
   {
      if (isObject($batchTestShield[%s]))
         $batchTestShield[%s].delete();
      $batchTestShield[%s] = "";
   }

   t2dScene.clear();
   for (%v = 0; %v < 2; %v++)
   {
      $batchTestVolcano[%v] = new ParticlePlayer()
      {
         scene = t2dScene;
         Particle = "Archon:spell_volcano";
         Position = ((%v * 16) - 8) SPC "0";
         Size = "8 8";
         SceneLayer = 1;
      };
      $batchTestShield[%v] = new ParticlePlayer()
      {
         scene = t2dScene;
         Particle = "Archon:spell_shield";
         Position = ((%v * 16) - 8) SPC "12";
         Size = "8 8";
         SceneLayer = 1;
      };
   }

   schedule(180, 0, "checkLiveMixedParticleBatchPhase");
}

function checkLiveMixedParticleBatchPhase()
{
   %stats = t2dScene.getBatchStats();
   %flushes = getWord(%stats, 0);
   %isolated = getWord(%stats, 1);
   %blendFlushes = getWord(%stats, 2);
   %colorFlushes = getWord(%stats, 3);
   %tris = getWord(%stats, 8);
   logOptResult("INFO: 2x spell_volcano + 2x spell_shield (10 emitters) getBatchStats() = [" @ %stats @ "]");

   assertOptCond("Mixed volcano + shield ParticlePlayers render with batchIsolatedFlush == 0",
      %isolated == 0);
   assertOptCond("Mixed volcano + shield ParticlePlayers render with batchColorStateFlush == 0",
      %colorFlushes == 0);
   assertOptCond("Mixed volcano + shield ParticlePlayers submit visible particle triangles",
      %tris > 0);
   assertOptCond("Mixed volcano + shield ParticlePlayers reduce flushes dramatically (<= 4 total)",
      %flushes > 0 && %flushes <= 4 && %blendFlushes <= 3);

   for (%v = 0; %v < 2; %v++)
   {
      if (isObject($batchTestVolcano[%v]))
         $batchTestVolcano[%v].delete();
      if (isObject($batchTestShield[%v]))
         $batchTestShield[%v].delete();
      $batchTestVolcano[%v] = "";
      $batchTestShield[%v] = "";
   }

   // Restore original particle density preference cleanly before exit
   $ResolutionManager.setParticleDensity($savedInitialParticleDensity, false);

   t2dScene.clear();
   t2dScene.delete();

   logOptResult("==========================================");
   logOptResult("ALL PARTICLE OPTIMIZATION TESTS PASSED!");
   logOptResult("==========================================");

   quit();
}
