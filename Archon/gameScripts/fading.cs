//---------------------------------------------------------------------------------------------
// Archonita - Fade In / Fade Out Utilities (Torque2D 4.0)
//---------------------------------------------------------------------------------------------

function SceneObject::startFadeOut(%this, %speed, %blend)
{
      %this.fadingOut = true;
      
      if(%blend $= "")
      {
         %this.setBlendAlpha(1.0);
      }
      else if (%blend $= "current")
      {
          // Leave alpha as is
      }
      else
      {
         %this.setBlendAlpha(%blend);
      } 

      %this.schedule(50, fadeOut, %speed);
}

function SceneObject::fadeOut(%this, %speed)
{   
   if(%this.fadingOut)
   {
      %alpha = %this.getBlendAlpha();
      %alpha -= %speed;
      
      if(%alpha < 0)
      {
         %this.fadingOut = false;
      } 
      else
      {
         %this.setBlendAlpha( %alpha );
         %this.schedule(50, fadeOut, %speed);
      }
   }
}

function SceneObject::stopFadeOut(%this)
{
   %this.fadingOut = false;
}

function SceneObject::startFadeIn(%this, %speed, %blend)
{
      %this.fadingIn = true;
      
      if(%blend $= "")
      {
         %this.setBlendAlpha(0.0);
      }
      else if (%blend $= "current")
      {
          // Leave alpha as is
      }
      else
      {
         %this.setBlendAlpha(%blend);
      } 

      %this.schedule(50, fadeIn, %speed);
}

function SceneObject::fadeIn(%this, %speed)
{   
   if(%this.fadingIn)
   {
      %alpha = %this.getBlendAlpha();
      %alpha += %speed;
      
      if(%alpha > 1)
      {
         %this.fadingIn = false;
         if (%this.isMethod("onFadeInEnd"))
            %this.onFadeInEnd();
      } 
      else
      {
         %this.setBlendAlpha( %alpha );
         %this.schedule(50, fadeIn, %speed);
      }
   }
}

function SceneObject::stopFadeIn(%this)
{
   %this.fadingIn = false;
}

// Legacy aliases
function t2dSceneObject::startFadeOut(%this, %s, %b) { %this.startFadeOut(%s, %b); }
function t2dSceneObject::fadeOut(%this, %s) { %this.fadeOut(%s); }
function t2dSceneObject::stopFadeOut(%this) { %this.stopFadeOut(); }
function t2dSceneObject::startFadeIn(%this, %s, %b) { %this.startFadeIn(%s, %b); }
function t2dSceneObject::fadeIn(%this, %s) { %this.fadeIn(%s); }
function t2dSceneObject::stopFadeIn(%this) { %this.stopFadeIn(); }
