//---------------------------------------------------------------------------------------------
// Archonita - Fade In / Fade Out Utilities (Torque2D 4.0)
//---------------------------------------------------------------------------------------------

function SceneObject::startFadeOut(%this, %speed, %blend)
{
   if (!isObject(%this))
      return;

   if (isEventPending(%this._fadeSchedule))
      cancel(%this._fadeSchedule);

   %this.fadingOut = true;
   
   if (%blend $= "")
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

   %this._fadeSchedule = %this.schedule(50, fadeOut, %speed);
}

function SceneObject::fadeOut(%this, %speed)
{   
   if (!isObject(%this))
      return;

   if (%this.fadingOut)
   {
      %alpha = %this.getBlendAlpha();
      %alpha -= %speed;
      
      if (%alpha < 0)
      {
         %this.fadingOut = false;
         %this._fadeSchedule = "";
      } 
      else
      {
         %this.setBlendAlpha(%alpha);
         %this._fadeSchedule = %this.schedule(50, fadeOut, %speed);
      }
   }
}

function SceneObject::stopFadeOut(%this)
{
   if (!isObject(%this))
      return;

   %this.fadingOut = false;
   if (isEventPending(%this._fadeSchedule))
   {
      cancel(%this._fadeSchedule);
      %this._fadeSchedule = "";
   }
}

function SceneObject::startFadeIn(%this, %speed, %blend)
{
   if (!isObject(%this))
      return;

   if (isEventPending(%this._fadeSchedule))
      cancel(%this._fadeSchedule);

   %this.fadingIn = true;
   
   if (%blend $= "")
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

   %this._fadeSchedule = %this.schedule(50, fadeIn, %speed);
}

function SceneObject::fadeIn(%this, %speed)
{   
   if (!isObject(%this))
      return;

   if (%this.fadingIn)
   {
      %alpha = %this.getBlendAlpha();
      %alpha += %speed;
      
      if (%alpha > 1)
      {
         %this.fadingIn = false;
         %this._fadeSchedule = "";
         if (%this.isMethod("onFadeInEnd"))
            %this.onFadeInEnd();
      } 
      else
      {
         %this.setBlendAlpha(%alpha);
         %this._fadeSchedule = %this.schedule(50, fadeIn, %speed);
      }
   }
}

function SceneObject::stopFadeIn(%this)
{
   if (!isObject(%this))
      return;

   %this.fadingIn = false;
   if (isEventPending(%this._fadeSchedule))
   {
      cancel(%this._fadeSchedule);
      %this._fadeSchedule = "";
   }
}

// Legacy aliases (t2dSceneObject -> SceneObject)
function t2dSceneObject::startFadeOut(%this, %s, %b) { if (isObject(%this)) SceneObject::startFadeOut(%this, %s, %b); }
function t2dSceneObject::fadeOut(%this, %s) { if (isObject(%this)) SceneObject::fadeOut(%this, %s); }
function t2dSceneObject::stopFadeOut(%this) { if (isObject(%this)) SceneObject::stopFadeOut(%this); }
function t2dSceneObject::startFadeIn(%this, %s, %b) { if (isObject(%this)) SceneObject::startFadeIn(%this, %s, %b); }
function t2dSceneObject::fadeIn(%this, %s) { if (isObject(%this)) SceneObject::fadeIn(%this, %s); }
function t2dSceneObject::stopFadeIn(%this) { if (isObject(%this)) SceneObject::stopFadeIn(%this); }
