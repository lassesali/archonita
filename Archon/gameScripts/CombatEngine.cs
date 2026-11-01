//---------------------------------------------------------------------------------------------
// Archonita - CombatEngine.cs
// Unit actions, combat resolution, dice rolling, damage resolution, piece movement, and animation states.
//---------------------------------------------------------------------------------------------

function CombatEngine::onAdd(%this)
{
   %this.dice = new SimSet();
   %this.hideBloodEvent = 0;
   %this.damagePieceEvent = 0;
   %this.diceRollEvent = 0;
   %this.diceStopEvent = 0;
   %this.inputLocked = false;
   $inputLocked = false;
   $scheduleID = 0;
   $diceRollScheduleID = 0;
}

function CombatEngine::onRemove(%this)
{
   if (isObject(%this.dice))
      %this.dice.delete();
   if (isEventPending(%this.hideBloodEvent))
      cancel(%this.hideBloodEvent);
   if (isEventPending(%this.damagePieceEvent))
      cancel(%this.damagePieceEvent);
   if (isEventPending(%this.diceRollEvent))
      cancel(%this.diceRollEvent);
   if (isEventPending(%this.diceStopEvent))
      cancel(%this.diceStopEvent);

   for (%i = 0; %i <= 8; %i++)
   {
      if (isObject($noppa[%i]))
         $noppa[%i].safeDelete();
      if (isObject($noppadef[%i]))
         $noppadef[%i].safeDelete();
   }

   %this.inputLocked = false;
   $inputLocked = false;
   $scheduleID = 0;
   $diceRollScheduleID = 0;
}

function CombatEngine::lockInput(%this)
{
   %this.inputLocked = true;
   $inputLocked = true;
   if (isObject(sceneWindow2D))
   {
      sceneWindow2D.setUseWindowMouseEvents(false);
      sceneWindow2D.objectSelected = false;
   }
}

function CombatEngine::unlockInput(%this)
{
   %this.inputLocked = false;
   $inputLocked = false;
   if (isObject(sceneWindow2D) && isObject(game) && game.getActivePlayer() == 0)
      sceneWindow2D.setUseWindowMouseEvents(true);
}

function CombatEngine::isInputLocked(%this)
{
   return (%this.inputLocked || $inputLocked);
}

function CombatEngine::cancelDiceRoll(%this)
{
   if (isEventPending(%this.diceRollEvent))
   {
      cancel(%this.diceRollEvent);
      %this.diceRollEvent = 0;
   }
   if (isEventPending($scheduleID))
   {
      cancel($scheduleID);
      $scheduleID = 0;
   }
   if (isEventPending($diceRollScheduleID))
   {
      cancel($diceRollScheduleID);
      $diceRollScheduleID = 0;
   }
   if (isEventPending(%this.diceStopEvent))
   {
      cancel(%this.diceStopEvent);
      %this.diceStopEvent = 0;
   }
   %this.stopDiceAnimation();
   %this.unlockInput();
}

function CombatEngine::playDiceRollSound(%this, %volume)
{
   return playDiceRollSound(%volume);
}

function CombatEngine::stopDiceAnimation(%this)
{
   if (isEventPending(%this.diceStopEvent))
   {
      cancel(%this.diceStopEvent);
      %this.diceStopEvent = 0;
   }

   for (%i = 0; %i <= 8; %i++)
   {
      if (isObject($noppa[%i]))
      {
         $noppa[%i].stopAnimation();
         if ($noppa[%i].dieType $= "red")
            $noppa[%i].setImage("Archon:diceredImageMap");
         else
            $noppa[%i].setImage("Archon:diceblueImageMap");
         %frame = ($noppa[%i].rollResult < 4) ? $noppa[%i].rollResult : ($noppa[%i].rollResult % 4);
         $noppa[%i].setImageFrame(%frame);
         $noppa[%i].setSize("8 8");
         $noppa[%i].setAngle(0);
         $noppa[%i].setAngularVelocity(0);
      }
      if (isObject($noppadef[%i]))
      {
         $noppadef[%i].stopAnimation();
         if ($noppadef[%i].dieType $= "red")
            $noppadef[%i].setImage("Archon:diceredImageMap");
         else
            $noppadef[%i].setImage("Archon:diceblueImageMap");
         %frame = ($noppadef[%i].rollResult < 4) ? $noppadef[%i].rollResult : ($noppadef[%i].rollResult % 4);
         $noppadef[%i].setImageFrame(%frame);
         $noppadef[%i].setSize("8 8");
         $noppadef[%i].setAngle(0);
         $noppadef[%i].setAngularVelocity(0);
      }
   }
}

function CombatEngine::doAction(%this)
{
   if (%this.isInputLocked())
      return;

   if (game.getHighlightedActionIcon() == 1)
   {
      %this.doDefence(game.getTurn());
   } 
   else if (game.getHighlightedActionIcon() == 2)
   {
      %this.doMagic();
   } 
   else if (game.getHighlightedActionIcon() == 3)
   {
   }
}

function CombatEngine::doDefence(%this, %turn)
{
   if (%this.isInputLocked())
      return;

   if (%turn == game.getTurn()) 
   {
      nextTurn();
      if (game.getState() != $gameEndState) { 
         evaluateTacticalMoves(); 
      }
   }
}

function CombatEngine::doMagic(%this)
{
}

function CombatEngine::doAttack(%this, %targetEnemy, %turn)
{
   if (%turn != game.getTurn() || %this.isInputLocked())
      return;

   %selectedPiece = game.getSelectedPiece();
   if (%selectedPiece $= "" || !isObject($nappulaSprite[%selectedPiece]))
      return;

   $nappulaSprite[%selectedPiece].hasDoneAction = 1;

   echo("function doAttack(" @ %targetEnemy @ "," @ %turn @ ")");  
   
   %pos1 = $nappulaSprite[%selectedPiece].positionOnBoard;
   %pos2 = %targetEnemy.positionOnBoard;

   $nappulaSprite[%selectedPiece].facing = %this.facing(%pos1, %pos2);
   %targetEnemy.facing = %this.facing(%pos2, %pos1);
   
   echo("setPieceState(" @ $nappulaSprite[%selectedPiece] @ "," @ $olentoAttackState[$nappulaSprite[%selectedPiece].creatureID] @ ")");
   %this.setPieceState($nappulaSprite[%selectedPiece], $olentoAttackState[$nappulaSprite[%selectedPiece].creatureID]);
    
   echo("setPieceState(" @ %targetEnemy @ "," @ $olentoBeenHitState[%targetEnemy.creatureID] @ ")");
   schedule(50, 0, "setPieceState", %targetEnemy, $olentoBeenHitState[%targetEnemy.creatureID]);
  
   // Lock player input & clear selection grids during the 500ms roll window
   %this.lockInput();
   deleteGrids(1);

   if (game.getActivePlayer() == 0) {
      %result = %this.rollAttackDice($nappulaSprite[%selectedPiece], %targetEnemy);
   } else {
      %result = %this.rollDefenseDice(%targetEnemy, $nappulaSprite[%selectedPiece]);
   }

   // Defer combat damage / outcome resolution by 500ms
   if (isEventPending(%this.diceRollEvent))
      cancel(%this.diceRollEvent);

   %this.diceRollEvent = %this.schedule(500, "onAttackRollComplete", %targetEnemy, %result, %turn);
   $scheduleID = %this.diceRollEvent;
   $diceRollScheduleID = %this.diceRollEvent;
}

function CombatEngine::onAttackRollComplete(%this, %targetEnemy, %result, %turn)
{
   %this.diceRollEvent = 0;
   $scheduleID = 0;
   $diceRollScheduleID = 0;

   // Stop dice animation and set static rolled frame
   %this.stopDiceAnimation();

   // Unlock input
   %this.unlockInput();

   if (%turn != game.getTurn() || !isObject(%targetEnemy) || game.getState() == $gameEndState)
      return;

   if (game.getActivePlayer() == 0) {
      // Attack succeeded
      if (%result == 1) 
      {  
         echo("damagePiece(" @ %targetEnemy @ ")");
         %this.damagePiece(%targetEnemy);
         $fxbeen2 = playArchonSound(fxBeenHit);
         blood.setPosition(%targetEnemy.getPosition());
         %this.hideBloodEvent = schedule(300, 0, hideBlood);
      } else {
         $fxsoita = playArchonSound(fxSword);
      }
   } else {
      // Defense failed
      if (%result == 0)
      {  
         echo("damagePiece(" @ %targetEnemy @ ")");
         %this.damagePiece(%targetEnemy);
         $fx2 = playArchonSound(fxBeenHit);
         blood.setPosition(%targetEnemy.getPosition());
         %this.hideBloodEvent = schedule(300, 0, hideBlood);
      } else {
         $fxsoita = playArchonSound(fxSword);
      }
   }

   if (game.getState() != $gameEndState) { 
      evaluateTacticalMoves(); 
   }
}

function CombatEngine::hideBlood(%this)
{
   blood.setPosition("-50 -80");
}

function CombatEngine::rollAttackDice(%this, %attacker, %defender)
{
   %att = 0;
   %def = 0;

   playDiceRollSound();

   for (%i = 0; %i <= 8; %i++)
   {
      if (isObject($noppa[%i])) {
         $noppa[%i].safeDelete();
      }
      if (isObject($noppadef[%i])) {
         $noppadef[%i].safeDelete();
      }
   }

   for (%i = 0; %i < %attacker.attack; %i++)
   {
      $noppa[%i] = new Sprite() { class = "DiceSprite"; scene = t2dScene; };
      $noppa[%i].playAnimation("Archon:redDiceRollAnimation");
      $noppa[%i].setAnimationFrame(getRandom(5));
      $noppa[%i].setPosition(%i * 10 - 24 SPC "30");
      $noppa[%i].setSize("8 8");
      $noppa[%i].setRotation(getRandom(360));
      $noppa[%i].setAngularVelocity(2 * 36);
      %roll = getRandom(3);
      $noppa[%i].rollResult = %roll;
      $noppa[%i].dieType = "red";
      $noppa[%i].setVisible(true);
      $noppa[%i].setLayer(10);
      $noppa[%i].setUseMouseEvents(0);
      $noppa[%i].isSelectable = false;

      if (%roll == 0 || %roll == 2) %att++;
   }
   
   for (%i = 0; %i < %defender.defence; %i++)
   {
      $noppadef[%i] = new Sprite() { class = "DiceSprite"; scene = t2dScene; };
      $noppadef[%i].playAnimation("Archon:blueDiceRollAnimation");
      $noppadef[%i].setAnimationFrame(getRandom(5));
      $noppadef[%i].setPosition(%i * 10 - 24 SPC "-30");
      $noppadef[%i].setSize("8 8");
      $noppadef[%i].setRotation(getRandom(360));
      $noppadef[%i].setAngularVelocity(2 * 36);
      %roll = getRandom(3);
      $noppadef[%i].rollResult = %roll;
      $noppadef[%i].dieType = "blue";
      $noppadef[%i].setVisible(true);
      $noppadef[%i].setLayer(10);
      $noppadef[%i].setUseMouseEvents(0);
      $noppadef[%i].isSelectable = false;
      
      if (%roll == 3) %def++;
   }
   
   if (isEventPending(%this.diceStopEvent))
      cancel(%this.diceStopEvent);
   %this.diceStopEvent = %this.schedule(500, "stopDiceAnimation");

   if (%att > %def) {
      return 1;
   }
   else 
   {
      return 0;
   }
}

function CombatEngine::rollDefenseDice(%this, %defender, %attacker)
{
   %att = 0;
   %def = 0;

   playDiceRollSound();

   for (%i = 0; %i <= 8; %i++)
   {
      if (isObject($noppa[%i])) {
         $noppa[%i].safeDelete();
      }
      if (isObject($noppadef[%i])) {
         $noppadef[%i].safeDelete();
      }
   }

   for (%i = 0; %i < %defender.defence; %i++)
   {
      $noppadef[%i] = new Sprite() { class = "DiceSprite"; scene = t2dScene; };
      $noppadef[%i].playAnimation("Archon:redDiceRollAnimation");
      $noppadef[%i].setAnimationFrame(getRandom(5));
      $noppadef[%i].setPosition(%i * 10 - 24 SPC "30");
      $noppadef[%i].setSize("8 8");
      $noppadef[%i].setRotation(getRandom(360));
      $noppadef[%i].setAngularVelocity(2 * 36);
      %roll = getRandom(3);
      $noppadef[%i].rollResult = %roll;
      $noppadef[%i].dieType = "red";
      $noppadef[%i].setVisible(true);
      $noppadef[%i].setLayer(10);
      $noppadef[%i].setUseMouseEvents(0);
      $noppadef[%i].isSelectable = false;
      if (%roll == 1) %def++;
   }

   for (%i = 0; %i < %attacker.attack; %i++)
   {
      $noppa[%i] = new Sprite() { class = "DiceSprite"; scene = t2dScene; };
      $noppa[%i].playAnimation("Archon:blueDiceRollAnimation");
      $noppa[%i].setAnimationFrame(getRandom(5));
      $noppa[%i].setPosition(%i * 10 - 24 SPC "-30");
      $noppa[%i].setSize("8 8");
      $noppa[%i].setRotation(getRandom(360));
      $noppa[%i].setAngularVelocity(2 * 36);
      %roll = getRandom(3);
      $noppa[%i].rollResult = %roll;
      $noppa[%i].dieType = "blue";
      $noppa[%i].setVisible(true);
      $noppa[%i].setLayer(10);
      $noppa[%i].setUseMouseEvents(0);
      $noppa[%i].isSelectable = false;
      if (%roll <= 1) %att++;
   }
   
   if (isEventPending(%this.diceStopEvent))
      cancel(%this.diceStopEvent);
   %this.diceStopEvent = %this.schedule(500, "stopDiceAnimation");

   if (%def >= %att) {
      return 1;
   }
   else 
   {
      return 0;
   }
}

function CombatEngine::rollMovementDice(%this, %player)
{
   playDiceRollSound();

   for (%i = 0; %i <= 8; %i++)
   {
      if (isObject($noppa[%i])) {
         $noppa[%i].safeDelete();
      }
      if (isObject($noppadef[%i])) {
         $noppadef[%i].safeDelete();
      }
   }

   $noppa[0] = new Sprite() { class = "DiceSprite"; scene = t2dScene; };
   if (%player == 0)
   {
      $noppa[0].playAnimation("Archon:blueDiceRollAnimation");
      $noppa[0].dieType = "blue";
      $noppa[0].setPosition("-24 30");
   }
   else
   {
      $noppa[0].playAnimation("Archon:redDiceRollAnimation");
      $noppa[0].dieType = "red";
      $noppa[0].setPosition("24 30");
   }
   $noppa[0].setAnimationFrame(getRandom(5));
   $noppa[0].setSize("8 8");
   $noppa[0].setRotation(getRandom(360));
   $noppa[0].setAngularVelocity(2 * 36);
   %roll = getRandom(5);
   $noppa[0].rollResult = %roll;
   $noppa[0].setVisible(true);
   $noppa[0].setLayer(10);
   $noppa[0].setUseMouseEvents(0);
   $noppa[0].isSelectable = false;

   if (isEventPending(%this.diceStopEvent))
      cancel(%this.diceStopEvent);
   %this.diceStopEvent = %this.schedule(500, "stopDiceAnimation");
   return %roll;
}

function CombatEngine::doMovement(%this, %x, %y)
{
   if (%this.isInputLocked())
      return;

   %hero = HeroGroup.getObject(game.getActivePlayer());
   %selectedPiece = game.getSelectedPiece();
   if (%selectedPiece $= "" || !isObject($nappulaSprite[%selectedPiece]))
      return;

   // Attack target if tile contains enemy
   if ($board.obstacles[%x, %y] == 255 && game.getActivePlayer() == 0) {
      if ($nappulaSprite[%selectedPiece].hasDoneAction == 0) 
      {
         echo("%hero=" @ %hero);
         echo("%hero.getSelectedTileX()=" @ %hero.getSelectedTileX());
         echo("%hero.getSelectedTileY()=" @ %hero.getSelectedTileY());
         echo("$board.nappulat[%hero.getSelectedTileX(),%hero.getSelectedTileY()]=" @ $board.nappulat[%hero.getSelectedTileX(), %hero.getSelectedTileY()]);            
         %enemy = $nappulaSprite[$board.nappulat[%hero.getSelectedTileX(), %hero.getSelectedTileY()]];
         echo("%enemy=" @ %enemy);
         %this.doAttack(%enemy, game.getTurn());
      }
      return;
   } else

   // Walk to empty tile
   if ($board.obstacles[%x, %y] == 0) {
      if ($nappulaSprite[%selectedPiece].hasMoved == 0) 
      {
         %piece = $nappulaSprite[%selectedPiece];
         %this.movePiece(boardToWorldUnit(%x, %y), %x SPC %y);
         
         deleteGrids(1);
         
         %pos = %piece.positionOnBoard;
         %posx = getWord(%pos, 0);
         %posy = getWord(%pos, 1);

         $board.obstacles[%posx, %posy] = 0;
         $board.nappulat[%posx, %posy] = 0;
         $board.pieces[%posx, %posy] = 0;
         
         $board.obstacles[%x, %y] = 255;
         $board.nappulat[%x, %y] = %selectedPiece;
         $board.pieces[%x, %y] = %selectedPiece;
         
         %piece.BoardX = %x;
         %piece.BoardY = %y;
         %piece.positionOnBoard = %x SPC %y;
               
         %piece.hasMoved = 1;

         if (game.getState() != $gameEndState) { 
            if (game.getActivePlayer() == 0) 
            {   
               evaluateTacticalMoves(); 
            }
            else
            {
               schedule(1000, 0, evaluateTacticalMoves); 
            }
         }
      }
   }
}

function CombatEngine::movePiece(%this, %pos, %boardpos)
{
   %selectedPiece = game.getSelectedPiece();
   if (%selectedPiece $= "" || !isObject($nappulaSprite[%selectedPiece]))
      return;

   %piece = $nappulaSprite[%selectedPiece];

   %piece.setLayer("2");
   %piece.setSceneLayerDepthFront();

   %piece.facing = %this.facing(%piece.positionOnBoard, %boardpos);
   %piece.moveto(%pos, 10, true, true, true);
   
   %this.setPieceState(%piece, $olentoWalkState[%piece.creatureID]);
   $fxfoot = playArchonSound(fxFootStep);
}

function CombatEngine::facing(%this, %pos1, %pos2)
{
   %fromx = getWord(%pos1, 0);
   %fromy = getWord(%pos1, 1);
   %targetx = getWord(%pos2, 0);
   %targety = getWord(%pos2, 1);
   
   if (%targetx > %fromx) 
   {
      return "e";
   } 
   if (%targetx < %fromx)
   {
      return "w";
   }
   if (%targety > %fromy) 
   {
      return "s";
   } 
   if (%targety < %fromy)
   {
      return "n";
   } 
   if (%targety == %fromy && %targetx == %fromx)
   {
      return "";
   }
}

function CombatEngine::initAnimStates(%this)
{
   $unitNames = "warrior archer cleric paladin skeletonwarrior skeletonarcher dreadzombie ancientghost";
   $nappulaNimi = $unitNames;

   for (%i = 0; %i < 8; %i++)
   {
      %name = getword($unitNames, %i);
      $olentoStandState[%i] = new ScriptObject() {};
      $olentoStandState[%i].anim = %name @ "StoppedAnimation_";
      $olentoStandState[%i].restore = 0;
      $olentoStandState[%i].name = "stopped";

      $olentoWalkState[%i] = new ScriptObject() {}; 
      $olentoWalkState[%i].anim = %name @ "WalkingAnimation_";
      $olentoWalkState[%i].name = "walking";
      $olentoWalkState[%i].restore = 0;

      $olentoBeenHitState[%i] = new ScriptObject() {}; 
      $olentoBeenHitState[%i].anim = %name @ "BeenHitAnimation_";
      $olentoBeenHitState[%i].restore = 0;
      $olentoBeenHitState[%i].name = "beenhit";

      $olentoAttackState[%i] = new ScriptObject() {}; 
      $olentoAttackState[%i].anim = %name @ "AttackAnimation_";
      $olentoAttackState[%i].restore = 0;
      $olentoAttackState[%i].name = "attack";

      $olentoDieState[%i] = new ScriptObject() {}; 
      $olentoDieState[%i].anim = %name @ "TippingOverAnimation_";
      $olentoDieState[%i].restore = 0;
      $olentoDieState[%i].name = "die";
   }
}

function CombatEngine::setPieceState(%this, %piece, %state)
{
   if (!%piece || !%state)
   {
      endgame();
   }
   
   echo("");
   echo("param: %piece, %state=" @ %piece @ ", " @ %state @ "(" @ %state.name @ ")");
   echo("setPieceState, pieceIndex=" @ %piece.index);
   
   %piece.state = %state;
   %restore = %piece.state.restore;
   %playanim = %state.anim @ %piece.facing;
   
   echo("playanim, pieceIndex=" @ %playanim);
   echo("");
   
   %piece.playAnimation(%playanim, %restore);
   %piece.setFrameChangeCallback(1);

   if (%state.name $= "die" || %state == $olentoDieState[%piece.creatureID])
   {
      %piece.setLayer("3");
      %piece.setSceneLayerDepthBack();
   }
   else if (%piece.alive)
   {
      %piece.setLayer("2");
      %piece.setSceneLayerDepthFront();
   }
}

function CombatEngine::damagePiece(%this, %piece)
{
   echo("damagePiece:" SPC %piece);
   %piece.hp = %piece.hp - 1;
   
   if (%piece.hp <= 0) {
      %piece.alive = 0;
      %piece.setLayer("3");
      %piece.setSceneLayerDepthBack();
      schedule(100, 0, "setPieceState", %piece, $olentoDieState[%piece.creatureID]);
      echo("setPieceState(" @ %piece @ "," @ $olentoDieState[%piece.creatureID] @ ")");
      %this.hideBloodEvent = schedule(500, 0, hideBlood);
      blood.setPosition(%piece.getPosition());
      $soitadie = playArchonSound(fxDie);

      %selectedPiece = game.getSelectedPiece();
      if (%selectedPiece !$= "" && isObject($nappulaSprite[%selectedPiece]))
      {
         $nappulaSprite[%selectedPiece].hasMoved = 1;
         $nappulaSprite[%selectedPiece].hasDoneAction = 1;
      }

      $board.obstacles[%piece.boardX, %piece.boardY] = 0;
      $board.nappulat[%piece.boardX, %piece.boardY] = 0;
      $board.pieces[%piece.boardX, %piece.boardY] = 0;

      removePieceTurn(%piece.index);

      if (game.getState() != $gameEndState) 
      {
         evaluateTacticalMoves();
      }
   }
}

//---------------------------------------------------------------------------------------------
// Global Delegation Wrapper Stubs (Backward Compatibility)
//---------------------------------------------------------------------------------------------
function doAction()                                { CombatEngine.doAction(); }
function doDefence(%turn)                          { CombatEngine.doDefence(%turn); }
function doDefense(%turn)                          { CombatEngine.doDefence(%turn); }
function doMagic()                                 { CombatEngine.doMagic(); }
function doAttack(%target, %turn)                  { CombatEngine.doAttack(%target, %turn); }
function hideBlood()                               { CombatEngine.hideBlood(); }
function piilotaveri()                             { CombatEngine.hideBlood(); }
function rollAttackDice(%att, %def)                { return CombatEngine.rollAttackDice(%att, %def); }
function rollDicesRedAttacks(%att, %def)           { return CombatEngine.rollAttackDice(%att, %def); }
function rollDefenseDice(%def, %att)               { return CombatEngine.rollDefenseDice(%def, %att); }
function rollDicesRedDefends(%def, %att)           { return CombatEngine.rollDefenseDice(%def, %att); }
function rollMovementDice(%player)                 { return CombatEngine.rollMovementDice(%player); }
function doMovement(%x, %y)                        { CombatEngine.doMovement(%x, %y); }
function movePiece(%pos, %bpos)                    { CombatEngine.movePiece(%pos, %bpos); }
function moveNappula(%pos, %bpos)                  { CombatEngine.movePiece(%pos, %bpos); }
function facing(%p1, %p2)                          { return CombatEngine.facing(%p1, %p2); }
function InitAnimStates()                          { CombatEngine.initAnimStates(); }
function setPieceState(%piece, %state)             { CombatEngine.setPieceState(%piece, %state); }
function setNappulaState(%piece, %state)           { CombatEngine.setPieceState(%piece, %state); }
function damagePiece(%piece)                       { CombatEngine.damagePiece(%piece); }
function vahingoitaNappula(%piece)                 { CombatEngine.damagePiece(%piece); }
function cancelDiceRoll()                          { CombatEngine.cancelDiceRoll(); }
function stopDiceAnimation()                       { CombatEngine.stopDiceAnimation(); }
function isInputLocked()                           { return CombatEngine.isInputLocked(); }
function lockInput()                               { CombatEngine.lockInput(); }
function unlockInput()                             { CombatEngine.unlockInput(); }

// Initialize Singleton
if (!isObject(CombatEngine))
{
   new ScriptObject(CombatEngine) { };
}
