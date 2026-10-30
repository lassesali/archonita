//---------------------------------------------------------------------------------------------
// Archonita - Unit & Piece Callbacks (Torque2D 4.0)
// Binds piece movement & animation completion callbacks to state transitions
//---------------------------------------------------------------------------------------------

function Sprite::onPositionTarget(%this, %mod, %worldPos)
{
   if (%this.creatureID !$= "")
      setNappulaState( %this, $olentoStandState[ %this.creatureID ] );
}

function SceneObject::onPositionTarget(%this, %mod, %worldPos)
{
   if (%this.creatureID !$= "")
      setNappulaState( %this, $olentoStandState[ %this.creatureID ] );
}

function Nappula::onPositionTarget(%this, %mod, %worldPos)
{
   if (%this.creatureID !$= "")
      setNappulaState( %this, $olentoStandState[ %this.creatureID ] );
}

function Sprite::onMoveToComplete(%this)
{
   %this.onPositionTarget();
}

function SceneObject::onMoveToComplete(%this)
{
   %this.onPositionTarget();
}

function Nappula::onMoveToComplete(%this)
{
   %this.onPositionTarget();
}

function Kortti::onMoveToComplete(%this)
{
   %this.onPositionTarget();
}

function Sprite::onAnimationEnd(%this, %animationAssetID)
{
   if (%this.creatureID $= "")
      return;

   if (%this.state == $olentoDieState[%this.creatureID]) 
   {
      %this.setVisible(false);
   } 
   else if (%this.state == $olentoWalkState[%this.creatureID]) 
   {
      setNappulaState( %this, $olentoStandState[ %this.creatureID ] );
   } 
   else if (%this.state == $olentoBeenHitState[%this.creatureID]) 
   {
      setNappulaState( %this, $olentoStandState[ %this.creatureID ] );
   } 
   else if (%this.state == $olentoAttackState[%this.creatureID]) 
   {
      setNappulaState( %this, $olentoStandState[ %this.creatureID ] );
   }
}

function SceneObject::onAnimationEnd(%this, %animationAssetID)
{
   Sprite::onAnimationEnd(%this, %animationAssetID);
}

function Nappula::onAnimationEnd(%this, %animationAssetID)
{
   Sprite::onAnimationEnd(%this, %animationAssetID);
}

function Kortti::onPositionTarget(%this, %mod, %worldPos)
{
   %this.setFrame("0");
}
