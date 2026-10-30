function Platform::onLevelLoaded(%this, %scenegraph)
{
   %trigger = new t2dTrigger() { scenegraph = %scenegraph; class = OneWayTrigger; };
   %trigger.setCollisionActiveSend(false);
   %trigger.setPositionX(%this.getPositionX());
   %trigger.setPositionY(%this.getPositionY() + 1);
   %trigger.setWidth(%this.getWidth() + 2);
   %trigger.setHeight(%this.getHeight());
   %trigger.setGraphGroup(1);
}

function OneWayTrigger::onEnter(%this, %object)
{
   %object.setCollisionGroups(1);
}

function OneWayTrigger::onLeave(%this, %object)
{
   %object.setCollisionGroups(0, 1);
}

function Player::onLevelLoaded(%this, %scenegraph)
{
   $player = %this;

   if (isObject(oneWayPlatformMap))
      oneWayPlatformMap.delete();

   new ActionMap(oneWayPlatformMap);
   oneWayPlatformMap.bindcmd(keyboard, left, %this @ ".left = true;", %this @ ".left = false;");
   oneWayPlatformMap.bindcmd(keyboard, right, %this @ ".right = true;", %this @ ".right = false;");
   oneWayPlatformMap.bindcmd(keyboard, up, %this @ ".jump();", "");
   oneWayPlatformMap.push();

   %this.setCollisionMaxIterations(2);
}

function Player::jump(%this)
{
   %this.setLinearVelocityY(%this.jumpHeight * -10);
}

function Player::update(%this)
{
   %move = %this.right - %this.left;
   %this.setLinearVelocityX(%move * %this.moveSpeed);
}

function t2dSceneGraph::onUpdateScene(%this)
{
   if (isObject($player) && ($player.getSceneGraph() == %this))
      $player.update();
}
