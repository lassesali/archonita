//---------------------------------------------------------------------------------------------
// Archonita - Projectile.cs
// Particle/emitter datablocks, reusable Sprite projectile class, trajectory math,
// per-frame movement updates, impact resolution, and leak-free cleanup.
// Conforms to TORQUE_SCRIPT.md: single-class module (Projectile::*), onAdd/onRemove lifecycle.
//---------------------------------------------------------------------------------------------

datablock SimDataBlock(ProjectileTrailParticleData)
{
   class = "ParticleData";
   textureName = "Archon:particles2ImageMap";
   imageFrame = 0;
   lifetimeMS = 350;
   lifetimeVarianceMS = 150;
   spinRandomMin = -90;
   spinRandomMax = 90;
   colors[0] = "0.95 0.88 0.72 0.75";
   colors[1] = "0.85 0.75 0.55 0.35";
   colors[2] = "0.60 0.50 0.40 0.0";
   sizes[0] = 1.6;
   sizes[1] = 1.2;
   sizes[2] = 0.3;
   times[0] = 0.0;
   times[1] = 0.6;
   times[2] = 1.0;
};

datablock SimDataBlock(ProjectileSparkParticleData)
{
   class = "ParticleData";
   textureName = "Archon:particles3ImageMap";
   imageFrame = 0;
   lifetimeMS = 280;
   lifetimeVarianceMS = 100;
   spinRandomMin = -180;
   spinRandomMax = 180;
   colors[0] = "1.0 0.90 0.55 0.90";
   colors[1] = "1.0 0.75 0.30 0.50";
   colors[2] = "0.9 0.40 0.10 0.0";
   sizes[0] = 1.4;
   sizes[1] = 0.8;
   sizes[2] = 0.1;
   times[0] = 0.0;
   times[1] = 0.6;
   times[2] = 1.0;
};

datablock SimDataBlock(ProjectileHitParticleData)
{
   class = "ParticleData";
   textureName = "Archon:particles3ImageMap";
   imageFrame = 3;
   lifetimeMS = 300;
   lifetimeVarianceMS = 100;
   colors[0] = "1.0 0.85 0.40 1.0";
   colors[1] = "1.0 0.50 0.20 0.6";
   colors[2] = "0.8 0.20 0.10 0.0";
   sizes[0] = 1.2;
   sizes[1] = 2.2;
   sizes[2] = 0.2;
   times[0] = 0.0;
   times[1] = 0.5;
   times[2] = 1.0;
};

datablock SimDataBlock(ProjectileTrailEmitterData)
{
   class = "ParticleEmitterData";
   ejectionPeriodMS = 20;
   periodVarianceMS = 5;
   ejectionVelocity = 2.5;
   velocityVariance = 1.0;
   thetaMin = 165;
   thetaMax = 195;
   particles = "ProjectileTrailParticleData ProjectileSparkParticleData";
   particleAsset = "Archon:projectile_trail";
};

datablock SimDataBlock(ProjectileSparkEmitterData)
{
   class = "ParticleEmitterData";
   ejectionPeriodMS = 25;
   periodVarianceMS = 5;
   ejectionVelocity = 3.5;
   velocityVariance = 1.5;
   thetaMin = 160;
   thetaMax = 200;
   particles = "ProjectileSparkParticleData";
   particleAsset = "Archon:projectile_trail";
};

datablock SimDataBlock(ProjectileHitEmitterData)
{
   class = "ParticleEmitterData";
   ejectionPeriodMS = 10;
   ejectionVelocity = 9.0;
   velocityVariance = 4.0;
   thetaMin = 0;
   thetaMax = 360;
   particles = "ProjectileHitParticleData";
   particleAsset = "Archon:projectile_hit";
   lifetimeMS = 400;
};

datablock SimDataBlock(DefaultProjectileTemplate)
{
   class = "ProjectileTemplate";
   imageMap = "Archon:particlesAtlasImageMap";
   imageFrame = 9;
   size = "4.0 1.6";
   sceneLayer = 1;
   speed = 55.0;
   hitRadius = 1.2;
   maxFlightDuration = 3000;
   tickMs = 16;
   trailEmitterData = "ProjectileTrailEmitterData";
   hitEmitterData = "ProjectileHitEmitterData";
};

function Projectile::onAdd(%this)
{
   %this.init();
}

function Projectile::init(%this)
{
   if (%this.isInitialized)
      return;
   %this.isInitialized = true;

   %tpl = isObject(%this.template) ? %this.template : DefaultProjectileTemplate;

   %this.setBodyType("static");
   %this.setCollisionSuppress(true);
   %this.setUseMouseEvents(0);
   %this.isSelectable = 0;

   %layer = (%this.sceneLayer !$= "") ? %this.sceneLayer : %tpl.sceneLayer;
   %this.setSceneLayer(%layer);

   %size = (%this.projectileSize !$= "") ? %this.projectileSize : %tpl.size;
   %this.setSize(%size);

   %img = (%this.projectileImage !$= "") ? %this.projectileImage : %tpl.imageMap;
   %frame = (%this.projectileFrame !$= "") ? %this.projectileFrame : %tpl.imageFrame;
   if (isObject(AssetDatabase))
   {
      if (%img $= "Archon:particlesAtlasImageMap" && !AssetDatabase.isDeclaredAsset(%img))
      {
         %img = "Archon:particles3ImageMap";
         %frame = 1;
      }
      if (AssetDatabase.isDeclaredAsset(%img))
      {
         %this.setImage(%img);
         %this.setImageFrame(%frame);
      }
   }

   if (%this.speed $= "" || %this.speed <= 0)
      %this.speed = %tpl.speed;
   if (%this.hitRadius $= "" || %this.hitRadius <= 0)
      %this.hitRadius = %tpl.hitRadius;
   if (%this.maxFlightDuration $= "" || %this.maxFlightDuration <= 0)
      %this.maxFlightDuration = %tpl.maxFlightDuration;
   if (%this.tickMs $= "" || %this.tickMs <= 0)
      %this.tickMs = %tpl.tickMs;
   if (%this.trailEmitterData $= "")
      %this.trailEmitterData = %tpl.trailEmitterData;
   if (%this.hitEmitterData $= "")
      %this.hitEmitterData = %tpl.hitEmitterData;

   %this.hasImpacted = 0;
   %this.isCleaningUp = 0;
   %this.elapsedTime = 0;
   %this.updateEvent = 0;
   %this.trailEffect = "";

   %this.applyVisualStyle(%this.visualType);
}

function Projectile::onRemove(%this)
{
   if (isEventPending(%this.updateEvent))
   {
      cancel(%this.updateEvent);
      %this.updateEvent = 0;
   }

   if (isObject(%this.trailEffect))
   {
      %trail = %this.trailEffect;
      %this.trailEffect = "";
      %trail.delete();
   }

   if (isObject(CombatEngine) && isObject(CombatEngine.activeProjectiles))
   {
      if (CombatEngine.activeProjectiles.isMember(%this))
         CombatEngine.activeProjectiles.remove(%this);
   }
}

function Projectile::applyVisualStyle(%this, %visualType)
{
   if (!isObject(AssetDatabase))
      return;

   %useAtlas = AssetDatabase.isDeclaredAsset("Archon:particlesAtlasImageMap");

   switch$ (%visualType)
   {
      case "fire":
         %img = %useAtlas ? "Archon:particlesAtlasImageMap" : "Archon:particles2ImageMap";
         %frame = %useAtlas ? 6 : 2;
         %size = "4.5 2.0";
         %r = 1.0; %g = 0.55; %b = 0.15; %a = 1.0;

      case "beam" or "magic":
         %img = %useAtlas ? "Archon:particlesAtlasImageMap" : "Archon:particles3ImageMap";
         %frame = %useAtlas ? 9 : 1;
         %size = "4.2 1.8";
         %r = 0.55; %g = 0.85; %b = 1.0; %a = 1.0;

      case "stone":
         %img = %useAtlas ? "Archon:particlesAtlasImageMap" : "Archon:particles1ImageMap";
         %frame = 0;
         %size = "2.2 2.2";
         %r = 0.85; %g = 0.80; %b = 0.70; %a = 1.0;

      default:
         %img = %useAtlas ? "Archon:particlesAtlasImageMap" : "Archon:particles3ImageMap";
         %frame = %useAtlas ? 9 : 1;
         %size = "4.0 1.6";
         %r = 1.0; %g = 0.95; %b = 0.80; %a = 1.0;
   }

   if (AssetDatabase.isDeclaredAsset(%img))
   {
      %this.setImage(%img);
      %this.setImageFrame(%frame);
   }
   %this.setSize(%size);
   %this.setBlendColor(%r, %g, %b, %a);
}

function Projectile::attachTrailEmitter(%this)
{
   if (isObject(%this.trailEffect))
      return;

   %scene = %this.getScene();
   if (!isObject(%scene) && isObject(t2dScene))
      %scene = t2dScene;

   if (!isObject(%scene))
      return;

   %assetId = "Archon:projectile_trail";
   if (isObject(%this.trailEmitterData) && %this.trailEmitterData.particleAsset !$= "")
      %assetId = %this.trailEmitterData.particleAsset;

   if (!isObject(AssetDatabase) || !AssetDatabase.isDeclaredAsset(%assetId))
      return;

   %trail = new ParticlePlayer()
   {
      Particle = %assetId;
      SceneLayer = %this.getSceneLayer();
      Position = %this.getPosition();
      Size = "6 6";
      ParticleInterpolation = false;
   };
   if (!isObject(%trail))
      return;

   %trail.setBodyType("static");
   %trail.setCollisionSuppress(true);
   %scene.add(%trail);
   %trail.setAngle(%this.getAngle());
   if (!%trail.getIsPlaying())
      %trail.play(false);

   %this.trailEffect = %trail;
}

function Projectile::launch(%this, %startPos, %targetPos, %speed)
{
   %this.init();

   if (%speed !$= "" && %speed > 0)
      %this.speed = %speed;

   %this.startPos = %startPos;
   %this.targetPos = %targetPos;
   %this.setPosition(%startPos);

   if (!isObject(%this.getScene()) && isObject(t2dScene))
      t2dScene.add(%this);

   // Compute normalized flight direction and angle toward target
   %delta = VectorSub(%targetPos, %startPos);
   %dx = getWord(%delta, 0);
   %dy = getWord(%delta, 1);
   %dist = VectorLen(%delta);

   if (%dist > 0.0001)
      %dir = VectorNormalize(%delta);
   else
      %dir = "1 0 0";

   %dirX = getWord(%dir, 0);
   %dirY = getWord(%dir, 1);
   %this.direction = %dirX SPC %dirY;
   %this.velocity = (%dirX * %this.speed) SPC (%dirY * %this.speed);

   %angleRad = mAtan2(%dy, %dx);
   %angleDeg = mRadToDeg(%angleRad);
   %this.flightAngle = %angleDeg;
   %this.setAngle(%angleDeg);

   %this.attachTrailEmitter();

   %this.elapsedTime = 0;
   %this.lastTickTime = getSimTime();

   if (isEventPending(%this.updateEvent))
      cancel(%this.updateEvent);

   %this.updateEvent = %this.schedule(%this.tickMs, "onUpdateTick");
}

function Projectile::onUpdateTick(%this)
{
   if (%this.hasImpacted || %this.isCleaningUp)
      return;

   %this.inTickCallback = 1;
   %this.stepUpdateTick();
   if (isObject(%this))
      %this.inTickCallback = 0;
}

function Projectile::stepUpdateTick(%this)
{
   if (%this.hasImpacted || %this.isCleaningUp)
      return;

   %scene = %this.getScene();
   if (isObject(%scene) && %scene.getScenePause())
   {
      %this.lastTickTime = getSimTime();
      %this.updateEvent = %this.schedule(%this.tickMs, "onUpdateTick");
      return;
   }

   if (%this.trackTargetUnit && %this.hasTargetUnit)
   {
      if (!isObject(%this.targetUnit) || (%this.targetUnit.alive !$= "" && %this.targetUnit.alive == 0))
      {
         %this.cleanup("target_invalid");
         return;
      }

      if (%this.homing)
      {
         %this.targetPos = %this.targetUnit.getPosition();
         %reDelta = VectorSub(%this.targetPos, %this.getPosition());
         if (VectorLen(%reDelta) > 0.0001)
         {
            %reDir = VectorNormalize(%reDelta);
            %this.direction = getWord(%reDir, 0) SPC getWord(%reDir, 1);
            %this.velocity = (getWord(%reDir, 0) * %this.speed) SPC (getWord(%reDir, 1) * %this.speed);
            %angleDeg = mRadToDeg(mAtan2(getWord(%reDelta, 1), getWord(%reDelta, 0)));
            %this.flightAngle = %angleDeg;
            %this.setAngle(%angleDeg);
         }
      }
   }

   %now = getSimTime();
   %dtMs = %now - %this.lastTickTime;
   if (%dtMs <= 0)
      %dtMs = %this.tickMs;
   if (%dtMs > 100)
      %dtMs = 100;

   %this.lastTickTime = %now;
   %this.elapsedTime += %dtMs;

   if (%this.elapsedTime >= %this.maxFlightDuration)
   {
      %this.cleanup("timeout");
      return;
   }

   %dt = %dtMs / 1000.0;
   %curPos = %this.getPosition();
   %toTarget = VectorSub(%this.targetPos, %curPos);
   %remainingDist = VectorLen(%toTarget);
   %stepDist = %this.speed * %dt;

   // Trigger impact before advancing past the target in a single step
   if (%remainingDist <= %this.hitRadius || %stepDist >= %remainingDist)
   {
      %this.setPosition(%this.targetPos);
      if (isObject(%this.trailEffect))
         %this.trailEffect.setPosition(%this.targetPos);
      %this.onImpact();
      return;
   }

   %vx = getWord(%this.velocity, 0);
   %vy = getWord(%this.velocity, 1);
   %newX = getWord(%curPos, 0) + (%vx * %dt);
   %newY = getWord(%curPos, 1) + (%vy * %dt);
   %newPos = %newX SPC %newY;

   %this.setPosition(%newPos);
   if (isObject(%this.trailEffect))
   {
      %this.trailEffect.setPosition(%newPos);
      %this.trailEffect.setAngle(%this.getAngle());
   }

   if (VectorDist(%newPos, %this.targetPos) <= %this.hitRadius)
   {
      %this.setPosition(%this.targetPos);
      if (isObject(%this.trailEffect))
         %this.trailEffect.setPosition(%this.targetPos);
      %this.onImpact();
      return;
   }

   %this.updateEvent = %this.schedule(%this.tickMs, "onUpdateTick");
}

function Projectile::spawnHitEffect(%this, %pos)
{
   %scene = %this.getScene();
   if (!isObject(%scene) && isObject(t2dScene))
      %scene = t2dScene;

   %assetId = "Archon:projectile_hit";
   if (isObject(%this.hitEmitterData) && %this.hitEmitterData.particleAsset !$= "")
      %assetId = %this.hitEmitterData.particleAsset;

   if (!isObject(%scene) || !isObject(AssetDatabase) || !AssetDatabase.isDeclaredAsset(%assetId))
      return "";

   if (isObject(SpellEffectManager))
      return SpellEffectManager.spawnEffectAtPosition(%assetId, %pos, %this.getSceneLayer(), 220, 480);

   %fx = new ParticlePlayer()
   {
      scene = %scene;
      Particle = %assetId;
      Position = %pos;
      Size = "6 6";
      SceneLayer = %this.getSceneLayer();
   };
   if (isObject(%fx))
      schedule(450, 0, "deleteSimObject", %fx);
   return %fx;
}

function Projectile::onImpact(%this)
{
   if (%this.hasImpacted || %this.isCleaningUp)
      return;
   %this.hasImpacted = 1;

   if (isEventPending(%this.updateEvent))
   {
      cancel(%this.updateEvent);
      %this.updateEvent = 0;
   }

   %this.spawnHitEffect(%this.targetPos);

   if (%this.isCombatAttack && isObject(%this.combatEngine))
   {
      %this.combatEngine.onProjectileImpact(%this);
   }
   else if (isObject(%this.targetUnit))
   {
      if (%this.damage > 0 && %this.targetUnit.hp !$= "")
      {
         %this.targetUnit.hp -= %this.damage;
         if (%this.targetUnit.hp <= 0)
         {
            %this.targetUnit.hp = 0;
            %this.targetUnit.alive = 0;
         }
      }
   }

   %this.cleanup("impact");
}

function Projectile::cleanup(%this, %reason)
{
   if (%this.isCleaningUp)
      return;
   %this.isCleaningUp = 1;
   %this.cleanupReason = %reason;

   if (isEventPending(%this.updateEvent))
   {
      cancel(%this.updateEvent);
      %this.updateEvent = 0;
   }

   if (isObject(%this.trailEffect))
   {
      %trail = %this.trailEffect;
      %this.trailEffect = "";
      %trail.delete();
   }

   if (isObject(CombatEngine) && isObject(CombatEngine.activeProjectiles))
   {
      if (CombatEngine.activeProjectiles.isMember(%this))
         CombatEngine.activeProjectiles.remove(%this);
   }

   %this.setVisible(false);

   // Defer deletion outside an active SimConsoleEvent callback on %this to avoid self-delete asserts
   if (%this.inTickCallback)
      schedule(1, 0, "deleteSimObject", %this);
   else
      %this.delete();
}
