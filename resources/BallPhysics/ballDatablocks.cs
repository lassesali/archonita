
datablock t2dSceneObjectDatablock( BowlingBall )
{
   Class = "BowlingBall";
   Size = "10 10";
   CollisionMaxIterations = "2";
   CollisionActiveSend = "true";
   CollisionActiveReceive = "true";
   CollisionDetectionMode = "CIRCLE";
   CollisionResponseMode = "RIGID";
   CollisionCircleSuperscribed = "false"; //For circle collisions
   CollisionPolyScale = "0.9 1";
   ConstantForce = "0 160";
   ConstantForceGravitic = "true";
   AutoMassInertia = "true";
   Density = "0.035";
   Friction = "0.3";
   Restitution = "0.8";
};

datablock t2dSceneObjectDatablock( BounceBall )
{
   Class = "BounceBall";
   Size = "3 3";
   CollisionMaxIterations = "2";
   CollisionActiveSend = "true";
   CollisionActiveReceive = "true";
   CollisionPhysicsReceive = "false";
   CollisionDetectionMode = "CIRCLE";
   CollisionResponseMode = "RIGID";
   CollisionCircleSuperscribed = "false"; //For circle collisions
   CollisionPolyScale = "0.9 1";
   ConstantForce = "0 120";
   ConstantForceGravitic = "true";
   AutoMassInertia = "true";
   Density = "0.003";
   Friction = "0.3";
   Restitution = "0.8";
};

function BowlingBall::onLevelLoaded(%this, %scenegraph)
{
   %scenegraph.setDebugOn(5);   
}
datablock t2dSceneObjectDatablock( TennisBall )
{
   Class = "TennisBall";
   Size = "4.5 4.5";
   CollisionActiveSend = "true";
   CollisionActiveReceive = "true";
   CollisionPhysicsReceive = "false";
   CollisionDetectionMode = "CIRCLE";
   CollisionResponseMode = "RIGID";
   CollisionCircleSuperscribed = "false"; //For circle collisions
   CollisionPolyScale = "0.9 1";
   ConstantForce = "0 120";
   ConstantForceGravitic = "true";
   AutoMassInertia = "true";
   Density = "0.005";
   Restitution = "0.8";
};

datablock t2dSceneObjectDatablock( BowlingPin )
{
   Class = "BowlingPin";
   Size = "6.5 15.9";
   CollisionActiveSend = "true";
   CollisionActiveReceive = "true";
   CollisionPhysicsSend = "true";
   CollisionPhysicsReceive = "true";
   CollisionDetectionMode = "POLYGON";
   CollisionResponseMode = "RIGID";
   CollisionPolyList = "-0.948 0.494 -0.531 -0.847 0.0104 -0.962 0.479 -0.843 0.938 0.455 0.708 0.953 -0.0104 0.974 -0.646 0.923";
   ConstantForce = "0 120";
   ConstantForceGravitic = "true";
   AutoMassInertia = "true";
   Density = "0.04";
   Friction = "0.075";
   Restitution = "0.6";
};

datablock t2dSceneObjectDatablock( FloorandWalls )
{
      CollisionActiveReceive = "true";
      CollisionPhysicsSend = "false";
      CollisionResponseMode = "RIGID";
      Immovable = "true";
};