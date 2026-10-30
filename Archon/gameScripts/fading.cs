function t2dSceneObject::startFadeOut(%this, %speed, %blend)
{
      %this.fadingOut = true;
      
      if(%blend $= "")
      {
         %this.setBlendAlpha(1.0);
      }
      else if (%blend $= "current")
      {
          // Do nothing. Leave the alpha-value as it is
      }
      else
      {
         %this.setBlendAlpha(%blend);
      } 

      %this.schedule(50,fadeOut, %speed);
}

function t2dSceneObject::fadeOut(%this, %speed)
{   
   if(%this.fadingOut)
   {
      %alpha = %this.getBlendAlpha();
      %alpha -= %speed;
      
      if(%alpha < 0)
      {
         %this.fadingOut = false;
         //%this.onFadeOutEnd();
      } else
      {
         %this.setBlendAlpha( %alpha );
         %this.schedule(50, fadeOut, %speed);
      }
   }
}

function t2dSceneObject::stopFadeOut(%this)
{
   %this.fadingOut = false;
}

function t2dSceneObject::startFadeIn(%this, %speed, %blend)
{
      %this.fadingIn = true;
      
      if(%blend $= "")
      {
         %this.setBlendAlpha(0.0);
      }
      else if (%blend $= "current")
      {
          // Do nothing. Leave the alpha-value as it is
      }
      else
      {
         %this.setBlendAlpha(%blend);
      } 

      %this.schedule(50, fadeIn, %speed);
}

function t2dSceneObject::fadeIn(%this, %speed)
{   
   if(%this.fadingIn)
   {
      %alpha = %this.getBlendAlpha();
      %alpha += %speed;
      
      if(%alpha > 1)
      {
         %this.fadingIn = false;
         %this.onFadeInEnd();
      } else
      {
         %this.setBlendAlpha( %alpha );
         %this.schedule(50, fadeIn, %speed);
      }
   }
}

function t2dSceneObject::stopFadeIn(%this)
{
   %this.fadingIn = false;
}