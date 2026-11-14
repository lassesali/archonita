//---------------------------------------------------------------------------------------------
// Archonita - RangedUnit.cs
// Ranged unit projectile firing, muzzle offset math, and attack animation frame hooks.
// Conforms to TORQUE_SCRIPT.md: single-class module (RangedUnit::*), init/onAdd/onRemove lifecycle.
//---------------------------------------------------------------------------------------------

function RangedUnit::onAdd(%this)
{
   %this.init();
}

function RangedUnit::init(%this)
{
   if (%this.isRangedInitialized)
      return;
   %this.isRangedInitialized = true;

   if (%this.muzzleOffsetDistance $= "")
      %this.muzzleOffsetDistance = 1.5;
   if (%this.muzzleOffsetY $= "")
      %this.muzzleOffsetY = 0.2;
   if (%this.projectileSpeed $= "")
      %this.projectileSpeed = DefaultProjectileTemplate.speed;
   if (%this.releaseFrame $= "")
      %this.releaseFrame = 6;
   if (%this.projectileVisualType $= "")
      %this.projectileVisualType = RangedUnit::resolveVisualType(%this);

   %this.pendingShotActive = 0;
   %this.attackLaunchSchedule = 0;
}

function RangedUnit::onRemove(%this)
{
   if (isEventPending(%this.frameWatchSchedule))
   {
      cancel(%this.frameWatchSchedule);
      %this.frameWatchSchedule = 0;
   }
   if (isEventPending(%this.attackLaunchSchedule))
   {
      cancel(%this.attackLaunchSchedule);
      %this.attackLaunchSchedule = 0;
   }
   %this.pendingShotActive = 0;
}

function RangedUnit::resolveVisualType(%this)
{
   switch (%this.creatureID)
   {
      case 9:
         return "fire";
      case 13:
         return "beam";
   }

   if (%this.unitType $= "slinger")
      return "stone";

   return "arrow";
}

function RangedUnit::getMuzzlePosition(%this, %targetPos)
{
   %unitPos = %this.getPosition();
   %offsetDist = (%this.muzzleOffsetDistance !$= "") ? %this.muzzleOffsetDistance : 1.5;
   %offsetY = (%this.muzzleOffsetY !$= "") ? %this.muzzleOffsetY : 0.2;

   %delta = VectorSub(%targetPos, %unitPos);
   if (VectorLen(%delta) > 0.0001)
      %dir = VectorNormalize(%delta);
   else
      %dir = "1 0 0";

   %muzzleX = getWord(%unitPos, 0) + (getWord(%dir, 0) * %offsetDist);
   %muzzleY = getWord(%unitPos, 1) + (getWord(%dir, 1) * %offsetDist) + %offsetY;
   return %muzzleX SPC %muzzleY;
}

function RangedUnit::fireProjectile(%this, %targetX, %targetY, %damage, %targetUnit)
{
   RangedUnit::init(%this);

   // Allow passing a space-separated "x y" vector as %targetX
   if (%targetY $= "" && getWordCount(%targetX) >= 2)
   {
      %targetY = getWord(%targetX, 1);
      %targetX = getWord(%targetX, 0);
   }

   %targetPos = %targetX SPC %targetY;
   %muzzlePos = RangedUnit::getMuzzlePosition(%this, %targetPos);

   %scene = %this.getScene();
   if (!isObject(%scene) && isObject(t2dScene))
      %scene = t2dScene;

   %resolvedDamage = (%damage !$= "") ? %damage : ((%this.att > 0) ? %this.att : 1);
   %resolvedSpeed = (%this.projectileSpeed !$= "" && %this.projectileSpeed > 0) ? %this.projectileSpeed : DefaultProjectileTemplate.speed;
   %resolvedVisual = (%this.projectileVisualType !$= "") ? %this.projectileVisualType : RangedUnit::resolveVisualType(%this);

   %projectile = new Sprite()
   {
      class = "Projectile";
      sourceUnit = %this;
      targetUnit = isObject(%targetUnit) ? %targetUnit : "";
      hasTargetUnit = isObject(%targetUnit) ? 1 : 0;
      damage = %resolvedDamage;
      speed = %resolvedSpeed;
      visualType = %resolvedVisual;
      sceneLayer = 1;
   };

   if (!isObject(%projectile))
      return "";

   if (isObject(%scene))
      %scene.add(%projectile);

   if (isObject(CombatEngine) && isObject(CombatEngine.activeProjectiles))
      CombatEngine.activeProjectiles.add(%projectile);

   %projectile.launch(%muzzlePos, %targetPos, %resolvedSpeed);
   %this.lastFiredProjectile = %projectile;
   return %projectile;
}

function RangedUnit::armAttackProjectile(%this, %targetX, %targetY, %releaseFrame, %damage, %targetUnit, %fallbackDelayMs)
{
   RangedUnit::init(%this);

   if (isEventPending(%this.attackLaunchSchedule))
   {
      cancel(%this.attackLaunchSchedule);
      %this.attackLaunchSchedule = 0;
   }

   %this.pendingShotActive = 1;
   %this.pendingTargetX = %targetX;
   %this.pendingTargetY = %targetY;
   %this.pendingDamage = %damage;
   %this.pendingTargetUnit = %targetUnit;
   %this.releaseFrame = (%releaseFrame !$= "") ? %releaseFrame : 6;

   %this.setFrameChangeCallback(true);

   if (%fallbackDelayMs !$= "" && %fallbackDelayMs > 0)
      %this.attackLaunchSchedule = %this.schedule(%fallbackDelayMs, "releaseArmedProjectile");
}

function RangedUnit::playAttackWithProjectile(%this, %animName, %targetX, %targetY, %releaseFrame, %damage, %targetUnit)
{
   RangedUnit::armAttackProjectile(%this, %targetX, %targetY, %releaseFrame, %damage, %targetUnit, "");
   if (%animName !$= "")
      %this.playAnimation(%animName, false);
}

function RangedUnit::releaseArmedProjectile(%this)
{
   if (isEventPending(%this.attackLaunchSchedule))
   {
      cancel(%this.attackLaunchSchedule);
      %this.attackLaunchSchedule = 0;
   }

   if (!%this.pendingShotActive)
      return "";

   %this.pendingShotActive = 0;
   %this.setFrameChangeCallback(false);

   return RangedUnit::fireProjectile(%this, %this.pendingTargetX, %this.pendingTargetY, %this.pendingDamage, %this.pendingTargetUnit);
}

function RangedUnit::onAnimationFrame(%this, %frameIndex)
{
   if (%this.pendingShotActive && %frameIndex >= %this.releaseFrame)
      return RangedUnit::releaseArmedProjectile(%this);
   return "";
}
