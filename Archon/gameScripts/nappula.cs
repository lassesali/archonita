//---------------------------------------------------------------------------------------------
// Archonita - Unit & Piece Callbacks (Torque2D 4.0)
// Binds piece movement & animation completion callbacks to state transitions
//---------------------------------------------------------------------------------------------

function Sprite::onPositionTarget(%this, %mod, %worldPos)
{
   if (%this.creatureID !$= "")
   {
      setPieceState(%this, $olentoStandState[%this.creatureID]);
      if (isObject($UnitStatsHud))
         $UnitStatsHud.refreshAll();
   }
}

function SceneObject::onPositionTarget(%this, %mod, %worldPos)
{
   if (%this.creatureID !$= "")
   {
      setPieceState(%this, $olentoStandState[%this.creatureID]);
      if (isObject($UnitStatsHud))
         $UnitStatsHud.refreshAll();
   }
}

//---------------------------------------------------------------------------------------------
// Piece Class Callbacks (formerly Nappula)
//---------------------------------------------------------------------------------------------
function Piece::onPositionTarget(%this, %mod, %worldPos)
{
   if (%this.creatureID !$= "")
   {
      setPieceState(%this, $olentoStandState[%this.creatureID]);
      if (isObject($UnitStatsHud))
         $UnitStatsHud.refreshAll();
   }
}

function Piece::onMoveToComplete(%this)
{
   %this.onPositionTarget();
}

function Piece::onAnimationEnd(%this, %animationAssetID)
{
   if (%this.state.name $= "die" || %this.state == $olentoDieState[%this.creatureID])
   {
      %this.setLayer("3");
      %this.setSceneLayerDepthBack();
   }
}

function Piece::getMuzzlePosition(%this, %targetPos)
{
   return RangedUnit::getMuzzlePosition(%this, %targetPos);
}

function Piece::resolveVisualType(%this)
{
   return RangedUnit::resolveVisualType(%this);
}

function Piece::fireProjectile(%this, %targetX, %targetY, %damage, %targetUnit)
{
   return RangedUnit::fireProjectile(%this, %targetX, %targetY, %damage, %targetUnit);
}

function Piece::armAttackProjectile(%this, %targetX, %targetY, %releaseFrame, %damage, %targetUnit, %fallbackDelayMs)
{
   RangedUnit::armAttackProjectile(%this, %targetX, %targetY, %releaseFrame, %damage, %targetUnit, %fallbackDelayMs);
}

function Piece::playAttackWithProjectile(%this, %animName, %targetX, %targetY, %releaseFrame, %damage, %targetUnit)
{
   RangedUnit::playAttackWithProjectile(%this, %animName, %targetX, %targetY, %releaseFrame, %damage, %targetUnit);
}

function Piece::releaseArmedProjectile(%this)
{
   return RangedUnit::releaseArmedProjectile(%this);
}

function Piece::onAnimationFrame(%this, %frameIndex)
{
   return RangedUnit::onAnimationFrame(%this, %frameIndex);
}

// Backward-compatible Nappula class delegation
function Nappula::onPositionTarget(%this, %mod, %worldPos) { Piece::onPositionTarget(%this, %mod, %worldPos); }
function Nappula::onMoveToComplete(%this)                  { Piece::onMoveToComplete(%this); }
function Nappula::onAnimationEnd(%this, %animId)           { Piece::onAnimationEnd(%this, %animId); }
function Nappula::fireProjectile(%this, %tx, %ty, %d, %t)  { return Piece::fireProjectile(%this, %tx, %ty, %d, %t); }
function Nappula::onAnimationFrame(%this, %frameIndex)     { return Piece::onAnimationFrame(%this, %frameIndex); }

//---------------------------------------------------------------------------------------------
// Card Class Callbacks (formerly Kortti)
//---------------------------------------------------------------------------------------------
function Card::onMoveToComplete(%this)
{
   %this.onPositionTarget();
}

function Card::onPositionTarget(%this, %mod, %worldPos)
{
   %this.setFrame("0");
}

// Backward-compatible Kortti class delegation
function Kortti::onMoveToComplete(%this)                   { Card::onMoveToComplete(%this); }
function Kortti::onPositionTarget(%this, %mod, %worldPos) { Card::onPositionTarget(%this, %mod, %worldPos); }
