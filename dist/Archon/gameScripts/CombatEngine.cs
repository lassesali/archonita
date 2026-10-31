//---------------------------------------------------------------------------------------------
// Archonita - CombatEngine.cs
// Unit actions, combat resolution, dice rolling, damage resolution, piece movement, and animation states.
//---------------------------------------------------------------------------------------------

function CombatEngine::onAdd(%this)
{
   %this.dice = new SimSet();
   %this.hideBloodEvent = 0;
   %this.damagePieceEvent = 0;
}

function CombatEngine::onRemove(%this)
{
   if (isObject(%this.dice))
      %this.dice.delete();
   if (isEventPending(%this.hideBloodEvent))
      cancel(%this.hideBloodEvent);
   if (isEventPending(%this.damagePieceEvent))
      cancel(%this.damagePieceEvent);
}

function CombatEngine::doAction(%this)
{
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
   if (%turn == game.getTurn()) 
   {
      $nappulaSprite[game.getSelectedPiece()].hasDoneAction = 1;

      echo("function doAttack(" @ %targetEnemy @ "," @ %turn @ ")");  
      
      %pos1 = $nappulaSprite[game.getSelectedPiece()].positionOnBoard;
      %pos2 = %targetEnemy.positionOnBoard;

      $nappulaSprite[game.getSelectedPiece()].facing = %this.facing(%pos1, %pos2);
      %targetEnemy.facing = %this.facing(%pos2, %pos1);
      
      echo("setPieceState(" @ $nappulaSprite[game.getSelectedPiece()] @ "," @ $olentoAttackState[$nappulaSprite[game.getSelectedPiece()].creatureID] @ ")");
      %this.setPieceState($nappulaSprite[game.getSelectedPiece()], $olentoAttackState[$nappulaSprite[game.getSelectedPiece()].creatureID]);
       
      echo("setPieceState(" @ %targetEnemy @ "," @ $olentoBeenHitState[%targetEnemy.creatureID] @ ")");
      schedule(50, 0, "setPieceState", %targetEnemy, $olentoBeenHitState[%targetEnemy.creatureID]);
     
      if (game.getActivePlayer() == 0) {
         %result = %this.rollAttackDice($nappulaSprite[game.getSelectedPiece()], %targetEnemy);

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
         %result = %this.rollDefenseDice(%targetEnemy, $nappulaSprite[game.getSelectedPiece()]);
         
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
      $noppa[%i] = new Sprite() { scene = t2dScene; };
      $noppa[%i].setImageMap("diceredImageMap");
      $noppa[%i].setPosition(%i * 10 - 24 SPC "30");
      $noppa[%i].setSize("8 8");
      $noppa[%i].setRotation(getRandom(360));
      $noppa[%i].setAngularVelocity(2 * 36);
      %roll = getRandom(3);
      $noppa[%i].setFrame(%roll);
      $noppa[%i].setVisible(true);
      $noppa[%i].setLayer(10);
      $noppa[%i].setUseMouseEvents(0);
      $noppa[%i].isSelectable = false;

      if (%roll == 0 || %roll == 2) %att++;
   }
   
   for (%i = 0; %i < %defender.defence; %i++)
   {
      $noppadef[%i] = new Sprite() { scene = t2dScene; };
      $noppadef[%i].setImageMap("diceblueImageMap");
      $noppadef[%i].setPosition(%i * 10 - 24 SPC "-30");
      $noppadef[%i].setSize("8 8");
      $noppadef[%i].setRotation(getRandom(360));
      $noppadef[%i].setAngularVelocity(2 * 36);
      %roll = getRandom(3);
      $noppadef[%i].setFrame(%roll);
      $noppadef[%i].setVisible(true);
      $noppadef[%i].setLayer(10);
      $noppadef[%i].setUseMouseEvents(0);
      $noppadef[%i].isSelectable = false;
      
      if (%roll == 3) %def++;
   }
   
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
      $noppadef[%i] = new Sprite() { scene = t2dScene; };
      $noppadef[%i].setImageMap("diceredImageMap");
      $noppadef[%i].setPosition(%i * 10 - 24 SPC "30");
      $noppadef[%i].setSize("8 8");
      $noppadef[%i].setRotation(getRandom(360));
      $noppadef[%i].setAngularVelocity(2 * 36);
      %roll = getRandom(3);
      $noppadef[%i].setFrame(%roll);
      $noppadef[%i].setVisible(true);
      $noppadef[%i].setLayer(10);
      $noppadef[%i].setUseMouseEvents(0);
      $noppadef[%i].isSelectable = false;
      if (%roll == 1) %def++;
   }

   for (%i = 0; %i < %attacker.attack; %i++)
   {
      $noppa[%i] = new Sprite() { scene = t2dScene; };
      $noppa[%i].setImageMap("diceblueImageMap");
      $noppa[%i].setPosition(%i * 10 - 24 SPC "-30");
      $noppa[%i].setSize("8 8");
      $noppa[%i].setRotation(getRandom(360));
      $noppa[%i].setAngularVelocity(2 * 36);
      %roll = getRandom(3);
      $noppa[%i].setFrame(%roll);
      $noppa[%i].setVisible(true);
      $noppa[%i].setLayer(10);
      $noppa[%i].setUseMouseEvents(0);
      $noppa[%i].isSelectable = false;
      if (%roll <= 1) %att++;
   }
   
   if (%def >= %att) {
      return 1;
   }
   else 
   {
      return 0;
   }
}

function CombatEngine::doMovement(%this, %x, %y)
{
   %hero = HeroGroup.getObject(game.getActivePlayer());
   
   // Attack target if tile contains enemy
   if ($board.obstacles[%x, %y] == 255 && game.getActivePlayer() == 0) {
      if ($nappulaSprite[game.getSelectedPiece()].hasDoneAction == 0) 
      {
         echo("%hero=" @ %hero);
         echo("%hero.getSelectedTileX()=" @ %hero.getSelectedTileX());
         echo("%hero.getSelectedTileY()=" @ %hero.getSelectedTileY());
         echo("$board.nappulat[%hero.getSelectedTileX(),%hero.getSelectedTileY()]=" @ $board.nappulat[%hero.getSelectedTileX(), %hero.getSelectedTileY()]);            
         %enemy = $nappulaSprite[$board.nappulat[%hero.getSelectedTileX(), %hero.getSelectedTileY()]];
         echo("%enemy=" @ %enemy);
         %this.doAttack(%enemy, game.getTurn());
      }
      
      $nappulaSprite[game.getSelectedPiece()].hasDoneAction = 1;
      
      if (game.getState() != $gameEndState) { 
         evaluateTacticalMoves(); 
      }
   } else

   // Walk to empty tile
   if ($board.obstacles[%x, %y] == 0) {
      if ($nappulaSprite[game.getSelectedPiece()].hasMoved == 0) 
      {
         %this.movePiece(boardToWorldUnit(%hero.getSelectedTileX(), %hero.getSelectedTileY()),
            %hero.getSelectedTileX() SPC %hero.getSelectedTileY());
         
         deleteGrids(1);
         
         %pos = $nappulaSprite[game.getSelectedPiece()].positionOnBoard;
         %posx = getword(%pos, 0);
         %posy = getword(%pos, 1);

         $board.obstacles[%posx, %posy] = 0;
         $board.nappulat[%posx, %posy] = 0;
         $board.pieces[%posx, %posy] = 0;
         
         $board.obstacles[%hero.getSelectedTileX(), %hero.getSelectedTileY()] = 255;
         $board.nappulat[%hero.getSelectedTileX(), %hero.getSelectedTileY()] = game.getSelectedPiece();
         $board.pieces[%hero.getSelectedTileX(), %hero.getSelectedTileY()] = game.getSelectedPiece();
         
         $nappulaSprite[game.getSelectedPiece()].BoardX = %hero.getSelectedTileX();
         $nappulaSprite[game.getSelectedPiece()].BoardY = %hero.getSelectedTileY();
         $nappulaSprite[game.getSelectedPiece()].positionOnBoard = 
            $nappulaSprite[game.getSelectedPiece()].BoardX SPC $nappulaSprite[game.getSelectedPiece()].BoardY;
               
         $nappulaSprite[game.getSelectedPiece()].hasMoved = 1;
      }

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

function CombatEngine::movePiece(%this, %pos, %boardpos)
{
   %piece = $nappulaSprite[game.getSelectedPiece()];

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
}

function CombatEngine::damagePiece(%this, %piece)
{
   echo("damagePiece:" SPC %piece);
   %piece.hp = %piece.hp - 1;
   
   if (%piece.hp <= 0) {
      %piece.alive = 0;
      schedule(100, 0, "setPieceState", %piece, $olentoDieState[%piece.creatureID]);
      echo("setPieceState(" @ %piece @ "," @ $olentoDieState[%piece.creatureID] @ ")");
      %this.hideBloodEvent = schedule(500, 0, hideBlood);
      blood.setPosition(%piece.getPosition());
      $soitadie = playArchonSound(fxDie);

      $nappulaSprite[game.getSelectedPiece()].hasMoved = 1;
      $nappulaSprite[game.getSelectedPiece()].hasDoneAction = 1;

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
function doMovement(%x, %y)                        { CombatEngine.doMovement(%x, %y); }
function movePiece(%pos, %bpos)                    { CombatEngine.movePiece(%pos, %bpos); }
function moveNappula(%pos, %bpos)                  { CombatEngine.movePiece(%pos, %bpos); }
function facing(%p1, %p2)                          { return CombatEngine.facing(%p1, %p2); }
function InitAnimStates()                          { CombatEngine.initAnimStates(); }
function setPieceState(%piece, %state)             { CombatEngine.setPieceState(%piece, %state); }
function setNappulaState(%piece, %state)           { CombatEngine.setPieceState(%piece, %state); }
function damagePiece(%piece)                       { CombatEngine.damagePiece(%piece); }
function vahingoitaNappula(%piece)                 { CombatEngine.damagePiece(%piece); }

// Initialize Singleton
if (!isObject(CombatEngine))
{
   new ScriptObject(CombatEngine) { };
}
