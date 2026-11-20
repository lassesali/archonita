//---------------------------------------------------------------------------------------------
// Archonita - SpellEffectManager.cs
// Manages Torque2D ParticlePlayer visual effects for spell resolution and afflictions.
// Conforms to TORQUE_SCRIPT.md: single-class ScriptObject, lifecycle in onAdd/onRemove.
//---------------------------------------------------------------------------------------------

function SpellEffectManager::onAdd(%this)
{
   %this.initialized = false;
   %this.defaultEmitDurationMs = 650;
   %this.defaultCleanupDelayMs = 1900;
   %this.pauseRetryDelayMs = 250;
   %this.boardEffectLayer = 1;
   %this.avatarEffectLayer = 28;
   %this.preloadedAssetCount = 0;

   if (!isObject(%this.activeEffects))
      %this.activeEffects = new SimSet();

   %this.init();
}

function SpellEffectManager::init(%this)
{
   if (%this.initialized)
      return;

   if (!isObject(%this.activeEffects))
      %this.activeEffects = new SimSet();

   if (isObject(AssetDatabase))
   {
      %assetList = "Archon:spell_volcano Archon:spell_disease Archon:spell_shield Archon:spell_mana Archon:spell_power";
      %count = getWordCount(%assetList);
      %idx = 0;
      for (%i = 0; %i < %count; %i++)
      {
         %assetId = getWord(%assetList, %i);
         if (AssetDatabase.isDeclaredAsset(%assetId))
         {
            %asset = AssetDatabase.acquireAsset(%assetId);
            if (isObject(%asset))
            {
               %this.preloadedAssetId[%idx] = %assetId;
               %this.preloadedAsset[%idx] = %asset;
               %idx++;
            }
         }
      }
      %this.preloadedAssetCount = %idx;
   }

   %this.initialized = true;
}

function SpellEffectManager::onRemove(%this)
{
   %this.clearAll();

   if (isObject(%this.activeEffects))
   {
      %this.activeEffects.delete();
      %this.activeEffects = "";
   }

   if (isObject(AssetDatabase))
   {
      for (%i = 0; %i < %this.preloadedAssetCount; %i++)
      {
         if (%this.preloadedAssetId[%i] !$= "")
            AssetDatabase.releaseAsset(%this.preloadedAssetId[%i]);
         %this.preloadedAssetId[%i] = "";
         %this.preloadedAsset[%i] = "";
      }
   }
   %this.preloadedAssetCount = 0;
   %this.initialized = false;
}

function SpellEffectManager::getSpellAssetId(%this, %spellFrame)
{
   switch (%spellFrame)
   {
      case 3: return "Archon:spell_shield";   // Taikamakkara (+1 Defence Roll)
      case 4: return "Archon:spell_mana";     // Taikahattu (+1 Mana)
      case 5: return "Archon:spell_power";    // Taikatikkari (+1 Spell Damage)
      case 6: return "Archon:spell_volcano";  // Tulivuori (Volcano damage)
      case 7: return "Archon:spell_mana";     // Voikukan juuri (+1 Mana)
      case 8: return "Archon:spell_disease";  // Vatsatauti (Skip Next Turn)
   }
   return "";
}

function SpellEffectManager::spawnEffectAtPosition(%this, %assetId, %position, %sceneLayer, %emitDurationMs, %cleanupDelayMs, %emissionScale)
{
   if (!isObject(t2dScene) || %assetId $= "" || %position $= "")
      return "";

   if (!isObject(AssetDatabase) || !AssetDatabase.isDeclaredAsset(%assetId))
      return "";

   if (%sceneLayer $= "")
      %sceneLayer = %this.boardEffectLayer;
   if (%emitDurationMs $= "" || %emitDurationMs <= 0)
      %emitDurationMs = %this.defaultEmitDurationMs;
   if (%cleanupDelayMs $= "" || %cleanupDelayMs <= %emitDurationMs)
      %cleanupDelayMs = %emitDurationMs + 1250;
   if (%emissionScale $= "" || %emissionScale <= 0)
      %emissionScale = 1.0;

   if (!isObject(%this.activeEffects))
      %this.activeEffects = new SimSet();

   %effect = new ParticlePlayer()
   {
      scene = t2dScene;
      Particle = %assetId;
      Position = %position;
      Size = "10 10";
      SceneLayer = %sceneLayer;
   };

   if (!isObject(%effect))
      return "";

   %baseDensity = isObject($ResolutionManager)
      ? $ResolutionManager.getParticleDensity()
      : (($pref::T2D::ParticlePlayerEmissionRateScale !$= "") ? $pref::T2D::ParticlePlayerEmissionRateScale : 1.0);
   if (%effect.isMethod("setEmissionRateScale"))
      %effect.setEmissionRateScale(%baseDensity * %emissionScale);

   %this.activeEffects.add(%effect);
   %effect.stopEventId = %this.schedule(%emitDurationMs, "stopEffectEmission", %effect);
   %effect.cleanupEventId = %this.schedule(%cleanupDelayMs, "cleanupEffect", %effect);

   return %effect;
}

function SpellEffectManager::spawnEffectOnPiece(%this, %piece, %assetId, %emissionScale)
{
   if (!isObject(%piece) || %assetId $= "")
      return "";

   return %this.spawnEffectAtPosition(%assetId, %piece.getPosition(), %this.boardEffectLayer, %this.defaultEmitDurationMs, %this.defaultCleanupDelayMs, %emissionScale);
}

function SpellEffectManager::spawnEffectOnGroup(%this, %groupId, %assetId)
{
   if (!isObject(game) || %assetId $= "")
      return 0;

   %pieceCount = game.getPieceCount();
   %targetCount = 0;
   for (%i = 1; %i <= %pieceCount; %i++)
   {
      %unit = $nappulaSprite[%i];
      if (isObject(%unit) && %unit.alive == 1 && %unit.groupid == %groupId)
         %targetCount++;
   }

   if (%targetCount == 0)
      return 0;

   %aoeScale = (%targetCount > 3) ? (3.0 / %targetCount) : 1.0;
   if (%aoeScale < 0.45)
      %aoeScale = 0.45;

   %spawnedCount = 0;
   for (%i = 1; %i <= %pieceCount; %i++)
   {
      %unit = $nappulaSprite[%i];
      if (isObject(%unit) && %unit.alive == 1 && %unit.groupid == %groupId)
      {
         %effect = %this.spawnEffectOnPiece(%unit, %assetId, %aoeScale);
         if (isObject(%effect))
            %spawnedCount++;
      }
   }
   return %spawnedCount;
}

function SpellEffectManager::spawnEffectOnAvatar(%this, %assetId)
{
   if (%assetId $= "")
      return "";

   %pos = isObject(heroicon) ? heroicon.getPosition() : "-40 -16";
   return %this.spawnEffectAtPosition(%assetId, %pos, %this.avatarEffectLayer, %this.defaultEmitDurationMs, %this.defaultCleanupDelayMs);
}

function SpellEffectManager::playSpellEffect(%this, %spellFrame, %targetPiece)
{
   %assetId = %this.getSpellAssetId(%spellFrame);
   if (%assetId $= "")
      return 0;

   switch (%spellFrame)
   {
      case 3: // Taikamakkara - buff all living friendly units
         return %this.spawnEffectOnGroup(0, %assetId);

      case 4: // Taikahattu - mana buff on hero avatar
         return isObject(%this.spawnEffectOnAvatar(%assetId)) ? 1 : 0;

      case 5: // Taikatikkari - spell damage buff on hero avatar
         return isObject(%this.spawnEffectOnAvatar(%assetId)) ? 1 : 0;

      case 6: // Tulivuori - volcanic eruption on all living enemy units
         return %this.spawnEffectOnGroup(1, %assetId);

      case 7: // Voikukan juuri - mana buff on hero avatar
         return isObject(%this.spawnEffectOnAvatar(%assetId)) ? 1 : 0;

      case 8: // Vatsatauti - disease miasma on afflicted target unit
         return isObject(%this.spawnEffectOnPiece(%targetPiece, %assetId)) ? 1 : 0;
   }

   return 0;
}

function SpellEffectManager::stopEffectEmission(%this, %effect)
{
   if (!isObject(%effect))
      return;

   %scene = %effect.getScene();
   if (isObject(%scene) && %scene.getScenePause())
   {
      %effect.stopEventId = %this.schedule(%this.pauseRetryDelayMs, "stopEffectEmission", %effect);
      return;
   }

   %effect.stopEventId = "";
   %effect.stop(true, false);
}

function SpellEffectManager::cleanupEffect(%this, %effect)
{
   if (!isObject(%effect))
      return;

   %scene = %effect.getScene();
   if (isObject(%scene) && %scene.getScenePause())
   {
      %effect.cleanupEventId = %this.schedule(%this.pauseRetryDelayMs, "cleanupEffect", %effect);
      return;
   }

   if (isEventPending(%effect.stopEventId))
      cancel(%effect.stopEventId);
   %effect.stopEventId = "";
   %effect.cleanupEventId = "";

   if (isObject(%this.activeEffects))
      %this.activeEffects.remove(%effect);

   %effect.safeDelete();
}

function SpellEffectManager::clearAll(%this)
{
   if (!isObject(%this.activeEffects))
      return;

   while (%this.activeEffects.getCount() > 0)
   {
      %effect = %this.activeEffects.getObject(0);
      %this.activeEffects.remove(%effect);

      if (isObject(%effect))
      {
         if (isEventPending(%effect.stopEventId))
            cancel(%effect.stopEventId);
         if (isEventPending(%effect.cleanupEventId))
            cancel(%effect.cleanupEventId);
         %effect.stopEventId = "";
         %effect.cleanupEventId = "";
         %effect.safeDelete();
      }
   }
}

if (!isObject(SpellEffectManager))
{
   new ScriptObject(SpellEffectManager) { };
}
if (!isObject($SpellEffectManager))
{
   $SpellEffectManager = SpellEffectManager;
}
