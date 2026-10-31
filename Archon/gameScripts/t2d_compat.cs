//---------------------------------------------------------------------------------------------
// Archonita - Torque2D 4.0 Compatibility & Translation Layer
// Bridges legacy TGB APIs to modern MIT Torque2D 4.0 EA4
//---------------------------------------------------------------------------------------------

// Dynamic coordinate mapping helper for grid boards (inverts Y for Box2D Y-up)
function boardToWorld(%bx, %by)
{
   %tileSize = (isObject(BoardConfig) && BoardConfig.tileSize !$= "") ? BoardConfig.tileSize : 8;
   %width = (isObject(BoardConfig) && BoardConfig.width !$= "") ? BoardConfig.width : 4;
   %height = (isObject(BoardConfig) && BoardConfig.height !$= "") ? BoardConfig.height : 4;
   %halfWidth = (%width * %tileSize) * 0.5;
   %halfHeight = (%height * %tileSize) * 0.5;

   %wx = (%bx * %tileSize) - %halfWidth;
   %wy = -((%by * %tileSize) - %halfHeight);
   return %wx SPC %wy;
}

function boardToWorldUnit(%bx, %by)
{
   %tileSize = (isObject(BoardConfig) && BoardConfig.tileSize !$= "") ? BoardConfig.tileSize : 8;
   %width = (isObject(BoardConfig) && BoardConfig.width !$= "") ? BoardConfig.width : 4;
   %height = (isObject(BoardConfig) && BoardConfig.height !$= "") ? BoardConfig.height : 4;
   %unitOffsetY = (isObject(BoardConfig) && BoardConfig.unitOffsetY !$= "") ? BoardConfig.unitOffsetY : 2;
   %halfWidth = (%width * %tileSize) * 0.5;
   %halfHeight = (%height * %tileSize) * 0.5;

   %wx = (%bx * %tileSize) - %halfWidth;
   %wy = -((%by * %tileSize) - (%halfHeight + %unitOffsetY));
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

function SceneObject::setRotation(%this, %angle)
{
   %this.setAngle(%angle);
}

function SceneObject::setAutoRotation(%this, %val)
{
   %this.setAngularVelocity(-%val);
}

function Sprite::setAutoRotation(%this, %val)
{
   %this.setAngularVelocity(-%val);
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
   {
      if (isObject(%this.mountedChildren))
         %this.mountedChildren.delete();
      %this.dismount();
      %this.delete();
   }
}

function SceneObject::addMountedChild(%this, %obj)
{
   if (!isObject(%this.mountedChildren))
      %this.mountedChildren = new SimSet();
   if (!%this.mountedChildren.isMember(%obj))
      %this.mountedChildren.add(%obj);
}

function SceneObject::removeMountedChild(%this, %obj)
{
   if (isObject(%this.mountedChildren))
      %this.mountedChildren.remove(%obj);
}

function SceneObject::mount(%this, %target, %a, %b, %c, %d, %e, %f)
{
   if (isObject(%this.mountedTarget) && %this.mountedTarget != %target)
      %this.dismount();

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
      %target.addMountedChild(%this);
      %this.updateMountPosition();
   }
}

function SceneObject::dismount(%this)
{
   if (isObject(%this.mountedTarget))
      %this.mountedTarget.removeMountedChild(%this);
   %this.mountedTarget = "";
}

function SceneObject::updateMountPosition(%this)
{
   if (!isObject(%this.mountedTarget))
      return;

   %pos = %this.mountedTarget.getPosition();
   %size = %this.mountedTarget.getSize();
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

   %this.updateMountedChildren();
}

function SceneObject::updateMountedChildren(%this)
{
   if (isObject(%this.mountedChildren))
   {
      %count = %this.mountedChildren.getCount();
      for (%i = 0; %i < %count; %i++)
      {
         %child = %this.mountedChildren.getObject(%i);
         if (isObject(%child))
            %child.updateMountPosition();
      }
   }
}

// ParticlePlayer compatibility
function ParticlePlayer::stopEffect(%this, %a, %b)
{
   if (isObject(%this))
   {
      %this.stop(%a, %b);
   }
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
