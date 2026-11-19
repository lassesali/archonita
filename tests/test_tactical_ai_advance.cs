//-----------------------------------------------------------------------------
// tests/test_tactical_ai_advance.cs
// Verification suite for TacticalAI passive melee stalling fix:
// Manhattan-distance advance, same-turn follow-up attack, diagonal advance,
// obstacle sidestep without oscillation, ranged alignment, and post-attack guard.
//-----------------------------------------------------------------------------

$Scripts::ignoreDSOs = true;
setLogMode(1);
$Archon::skipSplash = true;

$aiTestResultPath = pathConcat(getMainDotCsDir(), "tests/tactical_ai_advance_test_result.txt");
$aiTestFile = new FileObject();
$aiTestFile.openForWrite($aiTestResultPath);
$aiTestFile.writeLine("TACTICAL_AI_ADVANCE_TEST_START");

function logAIResult(%msg)
{
   echo(%msg);
   if (isObject($aiTestFile))
      $aiTestFile.writeLine(%msg);
}

function failAITest(%msg)
{
   logAIResult("TEST_FAIL: " @ %msg);
   if (isObject($aiTestFile))
   {
      $aiTestFile.close();
      $aiTestFile.delete();
   }
   quit();
}

function passAITest(%msg)
{
   logAIResult("TEST_PASS: " @ %msg);
}

function assertAICond(%label, %cond)
{
   if (!%cond)
      failAITest(%label);
   passAITest(%label);
}

function resetAITestBoard()
{
   if (isObject(TacticalAI))
      TacticalAI.cancelPendingEvents();
   if (isObject(CombatEngine))
      CombatEngine.cancelDiceRoll();

   for (%i = 1; %i <= 10; %i++)
   {
      if (isObject($nappulaSprite[%i]))
      {
         $nappulaSprite[%i].delete();
         $nappulaSprite[%i] = "";
      }
   }

   if (!isObject($board))
      $board = new ScriptObject();

   for (%x = 0; %x < 4; %x++)
   {
      for (%y = 0; %y < 4; %y++)
      {
         $board.obstacles[%x, %y] = 0;
         $board.nappulat[%x, %y] = 0;
         $board.pieces[%x, %y] = 0;
         $board.threat[%x, %y] = 0;
         $board.vaara[%x, %y] = 0;
      }
   }

   game.reset();
   game.setState($gameBattleState);
   game.setTurn(1);
   game.setActivePlayer(1);
   game.setActiveOpponent(0);
   game.setPieceCount(0);
   HeroGroup.getObject(0).reset();
   HeroGroup.getObject(1).reset();
}

function spawnAITestUnit(%idx, %groupId, %creatureId, %name, %bx, %by, %missile)
{
   if (%idx > game.getPieceCount())
      game.setPieceCount(%idx);

   $board.obstacles[%bx, %by] = 255;
   $board.nappulat[%bx, %by] = %idx;
   $board.pieces[%bx, %by] = %idx;

   $nappulaSprite[%idx] = new Sprite()
   {
      scene = t2dScene;
      class = "Piece";
      superclass = "Nappula";
      Position = boardToWorldUnit(%bx, %by);
      Size = "8 8";
      SceneLayer = 2;
      index = %idx;
      groupid = %groupId;
      creatureID = %creatureId;
      name = %name;
      level = 1;
      hp = 2;
      hpMAX = 2;
      attack = 2;
      attackMAX = 2;
      defence = 3;
      defenceMAX = 3;
      missile = %missile;
      alive = 1;
      hasMoved = 0;
      hasDoneAction = 0;
      skipNextTurn = 0;
      facing = (%groupId == 0 ? "n" : "s");
      boardX = %bx;
      boardY = %by;
      positionOnBoard = %bx SPC %by;
   };
   return $nappulaSprite[%idx];
}

setCompanyAndProduct("Torque Game Engines", "Archonita");
ModuleDatabase.EchoInfo = false;
AssetDatabase.EchoInfo = false;

ModuleDatabase.scanModules(pathConcat(getMainDotCsDir(), "library"));
ModuleDatabase.scanModules(pathConcat(getMainDotCsDir(), "Archon"));
ModuleDatabase.LoadExplicit("AppCore");

logAIResult("==========================================");
logAIResult("Archonita - Tactical AI Advance Suite");
logAIResult("==========================================");

if (isObject(t2dScene))
{
   t2dScene.clear();
   t2dScene.delete();
}
new Scene(t2dScene);
sceneWindow2D.setScene(t2dScene);
$ResolutionManager.applyCameraAspect(sceneWindow2D);

if (isObject(blood))
   blood.delete();
new Sprite(blood) { scene = t2dScene; Position = "-50 -80"; Size = "8 8"; SceneLayer = 1; };

if (isObject(heroicon))
   heroicon.delete();
new Sprite(heroicon) { scene = t2dScene; Position = "-40 -16"; Size = "16 16"; SceneLayer = 31; };

TurnManager.initStateMachine();
CombatEngine.initAnimStates();

// 1. All-melee player army in Row Y=3 vs AI melee unit at (1, 1)
resetAITestBoard();
spawnAITestUnit(1, 0, 0, "warrior", 0, 3, 0);
spawnAITestUnit(2, 0, 0, "warrior", 1, 3, 0);
spawnAITestUnit(3, 0, 0, "warrior", 2, 3, 0);
spawnAITestUnit(4, 0, 0, "warrior", 3, 3, 0);
spawnAITestUnit(5, 1, 4, "skeletonwarrior", 1, 1, 0);
game.setSelectedPiece(5);

TacticalAI.evaluateTacticalMoves();
assertAICond("AI melee at (1,1) advances South to (1,2) against Row Y=3 melee army",
   $nappulaSprite[5].positionOnBoard $= "1 2" &&
   $nappulaSprite[5].hasMoved == 1 &&
   $board.nappulat[1, 1] == 0 &&
   $board.nappulat[1, 2] == 5 &&
   !isEventPending(TacticalAI.aiDefenseEvent) &&
   isEventPending(TacticalAI.aiMoveFollowupEvent));

// 2. Same-turn attack after advancing adjacent to enemy
TacticalAI.cancelPendingEvents();
TacticalAI.evaluateTacticalMoves();
assertAICond("After advancing adjacent at (1,2), AI schedules attack on Row Y=3 unit",
   isEventPending(TacticalAI.aiAttackEvent) &&
   !isEventPending(TacticalAI.aiDefenseEvent));

// 3. Back-row diagonal advance: AI melee at (0,0) vs player melee at (2,3)
resetAITestBoard();
spawnAITestUnit(1, 1, 4, "skeletonwarrior", 0, 0, 0);
spawnAITestUnit(2, 0, 0, "warrior", 2, 3, 0);
game.setSelectedPiece(1);

TacticalAI.evaluateTacticalMoves();
assertAICond("Back-row AI melee at (0,0) advances South to (0,1) toward (2,3)",
   $nappulaSprite[1].positionOnBoard $= "0 1" &&
   $nappulaSprite[1].hasMoved == 1 &&
   !isEventPending(TacticalAI.aiDefenseEvent));

// 4. Obstacle sidestep & non-oscillation: pillar at (1,2), AI melee at (1,1), player melee at (1,3)
resetAITestBoard();
$board.obstacles[1, 2] = 2;
spawnAITestUnit(1, 1, 4, "skeletonwarrior", 1, 1, 0);
spawnAITestUnit(2, 0, 0, "warrior", 1, 3, 0);
game.setSelectedPiece(1);

TacticalAI.evaluateTacticalMoves();
assertAICond("Blocked by obstacle at (1,2), AI melee sidesteps East to (2,1) instead of stalling/retreating",
   $nappulaSprite[1].positionOnBoard $= "2 1" &&
   $nappulaSprite[1].hasMoved == 1 &&
   !isEventPending(TacticalAI.aiDefenseEvent));

// Next turn from (2,1): advance South to (2,2) past obstacle without oscillating back to (1,1)
TacticalAI.cancelPendingEvents();
$nappulaSprite[1].hasMoved = 0;
$nappulaSprite[1].hasDoneAction = 0;
game.addTurn(1);
TacticalAI.evaluateTacticalMoves();
assertAICond("On next turn at (2,1), AI advances South to (2,2) past obstacle without oscillating",
   $nappulaSprite[1].positionOnBoard $= "2 2" &&
   $nappulaSprite[1].hasMoved == 1 &&
   !isEventPending(TacticalAI.aiDefenseEvent));

// 5. Ranged AI alignment without player ranged threat: AI archer at (0,0) vs player melee at (1,3)
resetAITestBoard();
spawnAITestUnit(1, 1, 5, "skeletonarcher", 0, 0, 1);
spawnAITestUnit(2, 0, 0, "warrior", 1, 3, 0);
game.setSelectedPiece(1);

TacticalAI.evaluateTacticalMoves();
assertAICond("AI ranged unit at (0,0) steps East to (1,0) to align column X=1",
   $nappulaSprite[1].positionOnBoard $= "1 0" &&
   $nappulaSprite[1].hasMoved == 1 &&
   !isEventPending(TacticalAI.aiDefenseEvent));

TacticalAI.cancelPendingEvents();
TacticalAI.evaluateTacticalMoves();
assertAICond("Aligned AI ranged unit at (1,0) schedules ranged attack against (1,3)",
   isEventPending(TacticalAI.aiAttackEvent) &&
   !isEventPending(TacticalAI.aiDefenseEvent));

// 6. Post-attack stay-put guard: AI melee at (1,2) with hasDoneAction=1 stays put & schedules defense
resetAITestBoard();
spawnAITestUnit(1, 1, 4, "skeletonwarrior", 1, 2, 0);
spawnAITestUnit(2, 0, 0, "warrior", 1, 3, 0);
$nappulaSprite[1].hasMoved = 0;
$nappulaSprite[1].hasDoneAction = 1;
game.setSelectedPiece(1);

TacticalAI.evaluateTacticalMoves();
assertAICond("AI melee with hasDoneAction=1 stays at (1,2) and schedules aiDefenseEvent",
   $nappulaSprite[1].positionOnBoard $= "1 2" &&
   $nappulaSprite[1].hasMoved == 0 &&
   isEventPending(TacticalAI.aiDefenseEvent));

// Clean up
TacticalAI.cancelPendingEvents();
CombatEngine.cancelDiceRoll();
stopBackgroundMusic();
alxStopAll();
if (isObject(t2dScene))
{
   t2dScene.clear();
   t2dScene.delete();
}

logAIResult("==========================================");
logAIResult("ALL TACTICAL AI ADVANCE TESTS PASSED!");
logAIResult("==========================================");

if (isObject($aiTestFile))
{
   $aiTestFile.close();
   $aiTestFile.delete();
}

quit();
