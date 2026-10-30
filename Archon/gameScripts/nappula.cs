function Sprite::onPositionTarget(%this, %mod, %worldPos) {
   setNappulaState( %this, $olentoStandState[ %this.creatureID ] );
}

function Sprite::onAnimationEnd(%this, %mod, %worldPos) {
   
   //echo("animationname " @ %this.animationname @ "has ended.");

   //echo("state " @ %this.state.name);
   
   if (%this.state == $olentoDieState[%this.creatureID]) 
   {
      %this.setVisible(false);
      //echo(%this @ "'s visibility is set to false.");
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


function Kortti::onPositionTarget(%this, %mod, %worldPos) {
   %this.setFrame("0");
}