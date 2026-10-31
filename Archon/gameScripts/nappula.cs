//---------------------------------------------------------------------------------------------
// Archonita - Unit & Piece Callbacks (Torque2D 4.0)
// Binds piece movement & animation completion callbacks to state transitions
//---------------------------------------------------------------------------------------------

function Sprite::onPositionTarget(%this, %mod, %worldPos)
{
   if (%this.creatureID !$= "")
      setPieceState(%this, $olentoStandState[%this.creatureID]);
}

function SceneObject::onPositionTarget(%this, %mod, %worldPos)
{
   if (%this.creatureID !$= "")
      setPieceState(%this, $olentoStandState[%this.creatureID]);
}

//---------------------------------------------------------------------------------------------
// Piece Class Callbacks (formerly Nappula)
//---------------------------------------------------------------------------------------------
function Piece::onPositionTarget(%this, %mod, %worldPos)
{
   if (%this.creatureID !$= "")
      setPieceState(%this, $olentoStandState[%this.creatureID]);
}

function Piece::onMoveToComplete(%this)
{
   %this.onPositionTarget();
}

function Piece::onAnimationEnd(%this, %animationAssetID)
{
   Sprite::onAnimationEnd(%this, %animationAssetID);
}

// Backward-compatible Nappula class delegation
function Nappula::onPositionTarget(%this, %mod, %worldPos) { Piece::onPositionTarget(%this, %mod, %worldPos); }
function Nappula::onMoveToComplete(%this)                  { Piece::onMoveToComplete(%this); }
function Nappula::onAnimationEnd(%this, %animId)           { Piece::onAnimationEnd(%this, %animId); }

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
