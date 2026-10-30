//---------------------------------------------------------------------------------------------
// Archonita - Torque2D 4.0 Compatibility & Translation Layer
// Bridges legacy TGB APIs to modern MIT Torque2D 4.0 EA4
//---------------------------------------------------------------------------------------------

// Coordinate mapping helper for 4x4 board (inverts Y for Box2D Y-up)
function boardToWorld(%bx, %by)
{
   %wx = %bx * 8 - 16;
   %wy = -(%by * 8 - 16);
   return %wx SPC %wy;
}

function boardToWorldUnit(%bx, %by)
{
   %wx = %bx * 8 - 16;
   %wy = -(%by * 8 - 18);
   return %wx SPC %wy;
}

// Sprite image and animation setters
function Sprite::setImageMap(%this, %map)
{
   if (%map $= "")
      return;
   %name = %map;
   if (isObject(%map))
      %name = %map.getName();
   %this.setImage("Archon:" @ %name);
}

function Sprite::playAnimation(%this, %anim, %restore)
{
   if (%anim $= "")
      return;
   %asset = %anim;
   if (isObject(%anim))
      %asset = %anim.getName();
   if (strstr(%asset, "Archon:") == -1)
      %asset = "Archon:" @ %asset;
   %this.Animation = %asset;
   SpriteBase::playAnimation(%this, %asset);
}

function Sprite::setAnimationName(%this, %anim)
{
   %this.playAnimation(%anim);
}

function Sprite::setFrame(%this, %frame)
{
   %this.setImageFrame(%frame);
}

function Sprite::getFrame(%this)
{
   return %this.getImageFrame();
}

function SceneObject::setUseMouseEvents(%this, %val)
{
   %this.UseMouseEvents = %val;
}

function SceneObject::setCollisionActiveSend(%this, %val)
{
}

function SceneObject::setCollisionActiveReceive(%this, %val)
{
}

function SceneObject::setFrameChangeCallback(%this, %val)
{
}

function SceneObject::setBlending(%this, %a, %b)
{
}

function SceneObject::setLayer(%this, %layer)
{
   %this.setSceneLayer(%layer);
}

function SceneObject::getLayer(%this)
{
   return %this.getSceneLayer();
}

function SceneObject::safeDelete(%this)
{
   if (isObject(%this))
      %this.delete();
}

function SceneObject::mount(%this, %target, %a, %b, %c, %d, %e, %f)
{
   %this.mountedTarget = %target;
   if (getWordCount(%a) >= 2)
   {
      %this.mountOffsetX = getWord(%a, 0);
      %this.mountOffsetY = getWord(%a, 1);
   }
   else
   {
      %this.mountOffsetX = (%a !$= "") ? %a : 0;
      %this.mountOffsetY = (%b !$= "") ? %b : 0;
   }
   if (isObject(%target))
   {
      %pos = %target.getPosition();
      %size = %target.getSize();
      %hw = getWord(%size, 0) * 0.5;
      %hh = getWord(%size, 1) * 0.5;

      if (mAbs(%this.mountOffsetX) <= 1.0 && mAbs(%this.mountOffsetY) <= 1.0 && %hw > 1)
      {
         %wx = getWord(%pos, 0) + (%this.mountOffsetX * %hw);
         %wy = getWord(%pos, 1) + (%this.mountOffsetY * %hh);
      }
      else
      {
         %wx = getWord(%pos, 0) + %this.mountOffsetX;
         %wy = getWord(%pos, 1) + %this.mountOffsetY;
      }
      %this.setPosition(%wx SPC %wy);
   }
}

function SceneObject::dismount(%this)
{
   %this.mountedTarget = "";
}

// ParticlePlayer compatibility
function ParticlePlayer::stopEffect(%this, %a, %b)
{
   if (isObject(%this))
   {
      %this.stop(%a, %b);
   }
}

// SceneObject fading bindings
function SceneObject::startFadeOut(%this, %speed, %blend)
{
   t2dSceneObject::startFadeOut(%this, %speed, %blend);
}

function SceneObject::startFadeIn(%this, %speed, %blend)
{
   t2dSceneObject::startFadeIn(%this, %speed, %blend);
}

function SceneObject::stopFadeOut(%this)
{
   t2dSceneObject::stopFadeOut(%this);
}

function SceneObject::stopFadeIn(%this)
{
   t2dSceneObject::stopFadeIn(%this);
}

// Touch to Mouse event forwarding for SceneWindow
function sceneWindow2D::onTouchDown(%this, %touchID, %worldPos)
{
   sceneWindow2D::onMouseDown(%this, 0, %worldPos, 1);
}

function sceneWindow2D::onTouchUp(%this, %touchID, %worldPos)
{
   sceneWindow2D::onMouseUp(%this, 0, %worldPos, 1);
}

function sceneWindow2D::onTouchMove(%this, %touchID, %worldPos)
{
   sceneWindow2D::onMouseMove(%this, 0, %worldPos, 0);
}

function sceneWindow2D::onTouchMoved(%this, %touchID, %worldPos)
{
   sceneWindow2D::onMouseMove(%this, 0, %worldPos, 0);
}

function sceneWindow2D::onTouchDragged(%this, %touchID, %worldPos)
{
   sceneWindow2D::onMouseDragged(%this, 0, %worldPos, 1);
}

function SceneWindow::onTouchDown(%this, %touchID, %worldPos)
{
   sceneWindow2D::onTouchDown(%this, %touchID, %worldPos);
}

function SceneWindow::onTouchUp(%this, %touchID, %worldPos)
{
   sceneWindow2D::onTouchUp(%this, %touchID, %worldPos);
}

function SceneWindow::onTouchMove(%this, %touchID, %worldPos)
{
   sceneWindow2D::onTouchMove(%this, %touchID, %worldPos);
}

function SceneWindow::onTouchMoved(%this, %touchID, %worldPos)
{
   sceneWindow2D::onTouchMoved(%this, %touchID, %worldPos);
}

function SceneWindow::onTouchDragged(%this, %touchID, %worldPos)
{
   sceneWindow2D::onTouchDragged(%this, %touchID, %worldPos);
}

function SceneWindow::setUseWindowMouseEvents(%this, %val)
{
   %this.useWindowMouseEvents = %val;
}

function SceneWindow::setUseObjectMouseEvents(%this, %val)
{
   %this.useObjectMouseEvents = %val;
}

// Vector math compatibility functions
function t2dVectorCompare(%v1, %v2, %epsilon)
{
   if (%epsilon $= "")
      %epsilon = 0.0001;
   %x1 = getWord(%v1, 0);
   %y1 = getWord(%v1, 1);
   %x2 = getWord(%v2, 0);
   %y2 = getWord(%v2, 1);
   if (mAbs(%x1 - %x2) <= %epsilon && mAbs(%y1 - %y2) <= %epsilon)
      return 1;
   return 0;
}

function t2dVectorDistance(%v1, %v2)
{
   %x1 = getWord(%v1, 0);
   %y1 = getWord(%v1, 1);
   %x2 = getWord(%v2, 0);
   %y2 = getWord(%v2, 1);
   %dx = %x2 - %x1;
   %dy = %y2 - %y1;
   return mSqrt((%dx * %dx) + (%dy * %dy));
}

// Audio helper for Archon assets
function playArchonSound(%name)
{
   if (%name $= "" || %name $= "0") return 0;
   if (strstr(%name, "Archon:") == -1)
      %name = "Archon:" @ %name;
   return alxPlay(%name);
}
