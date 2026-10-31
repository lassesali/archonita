//---------------------------------------------------------------------------------------------
// Archonita - TurnManager.cs
// Session lifecycle, turn sequencing, game states, card dealing, and initiative queue.
//---------------------------------------------------------------------------------------------

function TurnManager::onAdd(%this)
{
   %this.turnTimerEvent = 0;
   %this.cardDealEvent = 0;
   %this.endBattleEvent = 0;
}

function TurnManager::onRemove(%this)
{
   if (isEventPending(%this.turnTimerEvent))
      cancel(%this.turnTimerEvent);
   if (isEventPending(%this.cardDealEvent))
      cancel(%this.cardDealEvent);
   if (isEventPending(%this.endBattleEvent))
      cancel(%this.endBattleEvent);
}

function TurnManager::shutdown(%this)
{
   shutdownProject();
}

function TurnManager::initStateMachine(%this)
{
   $gameIntroState = new ScriptObject() { };
   $gameMainMenuState = new ScriptObject() { };
   $gameCardSelectState = new ScriptObject() { };
   $gamePreBattleState = new ScriptObject() { };
   $gamePreBattleTimerState = new ScriptObject() { };
   $gameTimerBattleState = new ScriptObject() { };
   $gameBattleState = new ScriptObject() { };
   $gameEndState = new ScriptObject() { };  
   
   $gameBattleState.name = "Battle State";
   $gameEndState.name = "End State";
}

function TurnManager::setGameState(%this, %state)
{
   game.setState(%state);
}

function TurnManager::getGameState(%this)
{
   return game.getState();
}

function TurnManager::initNewGame(%this)
{
   game.setTurn(0);

   $Sprite = new Sprite() { scene = t2dScene; };
   $Kortti = new Sprite() { scene = t2dScene; };

   // Check player hero archetype selection ($selectedHero or legacy $sankarivalittu)
   %heroChoice = ($selectedHero !$= "") ? $selectedHero : $sankarivalittu;

   // Player chose warrior card (Archetype 1)
   if (%heroChoice == 1) {
      HeroGroup.getObject(0).addCreaturePoints(1);
   }

   // Player chose mage card (Archetype 2)
   if (%heroChoice == 2) {
      HeroGroup.getObject(0).addMana(1);
      HeroGroup.getObject(0).setHeroClass(1);
   }

   // Initialize creature animation states
   InitAnimStates();

   // AI hero archetype (AI always selects warrior)
   HeroGroup.getObject(1).addCreaturePoints(1);

   // Update hero portrait frame
   heroicon.setFrame(HeroGroup.getObject(0).getHeroClass());

   // Update HUD & Board
   updateCreaturePoints();
   updateCreatureIcons();
   updateActionIcons();
   initBoard();
   updatePlacementGrids();
   createSetupPieces();
}

function TurnManager::initGameForBattle(%this)
{
   createAIPieces();
   %this.distributeTurnQueue();
   
   %this.setGameState($gameBattleState);
   deleteSetupPieces();
   %this.nextTurn();
   evaluateTacticalMoves();
}

function TurnManager::updateCountdownTimer(%this)
{
   game.subGameTimer(1);
   secondCounter.setFrame(game.getGameTimer());

   if (game.getGameTimer() == 0) {
      secondText.setVisible(false);
      secondCounter.setVisible(false);
      %this.setGameState($gameTimerBattleState);
      updateActionIcons();
      %this.initGameForBattle();
   } else {
      %this.turnTimerEvent = %this.schedule(1000, "updateCountdownTimer");
      $secondSound = playArchonSound(ambient4Audio);
   }
}

function TurnManager::dealCards(%this)
{
   $cardCount++;
   $kortitnum = $cardCount;
   
   if ($cardCount < 6) {
      %this.cardDealEvent = %this.schedule(450, "dealCards");
   }
   
   %i = $cardCount;
   
   $specialcard[%i] = new Sprite() { scene = t2dScene; class = Card; superclass = Kortti; };
   $specialcard[%i].setImageMap("cardsImageMap");
   %posx = -20 - getRandom(50);
   %posy = 80;

   $specialcard[%i].setPosition(%posx SPC %posy);
   $specialcard[%i].setSize("24 24");
   $specialcard[%i].setFrame(0);
   $specialcard[%i].setVisible(true);
   $specialcard[%i].setLayer(10 - %i);
   $specialcard[%i].setUseMouseEvents(0);
   $specialcard[%i].isSelectable = false;
   $specialcard[%i].moveto("-40" SPC (0 + (%i * 0.5)), 50, true, true, true);
}

function TurnManager::updateActionIcons(%this)
{
   if (%this.getGameState() == $gamePreBattleState || %this.getGameState() == $gamePreBattleTimerState) {
      iconAttack1.setVisible(false);
      iconAttack2.setVisible(false);
      iconDefence1.setVisible(false);
      iconDefence2.setVisible(false);
      iconMagic1.setVisible(false);
      iconMagic2.setVisible(false);
      iconMagic3.setVisible(false);
      hero2icon.setVisible(false);
      hero2icongrid.setVisible(false);
   } 

   if (%this.getGameState() == $gameTimerBattleState) {
      iconAttack1.setVisible(true);
      iconAttack2.setVisible(true);
      iconDefence1.setVisible(true);
      iconDefence2.setVisible(true);
      iconMagic1.setVisible(true);
      iconMagic2.setVisible(true);
      iconMagic3.setVisible(true);
      hero2icon.setVisible(true);
      hero2icongrid.setVisible(true);
   } 
}

function TurnManager::areUnitsAlive(%this)
{
   %playerVector = 0;
   %aiVector = 0;
   for (%i = 1; %i <= game.getPieceCount(); %i++)
   {
      if ($nappulaSprite[%i].groupid == 0 && $nappulaSprite[%i].alive == 1)
      {
         if (%playerVector == 0) 
         { 
            %playerVector = %i;
         }
         else
         {
            %playerVector = %playerVector SPC %i;
         }
      }
      
      if ($nappulaSprite[%i].groupid == 1 && $nappulaSprite[%i].alive == 1)
      {
         if (%aiVector == 0) 
         {
            %aiVector = %i;
         }
         else
         {
            %aiVector = %aiVector SPC %i;
         }
      }
   }
   
   %aicount = getwordcount(%aiVector);
   %playercount = getwordcount(%playerVector);
   
   if (%aicount == 0 || %playercount == 0) {
      return 0;
   } 
   else
   {
      return 1;
   }
}

function TurnManager::distributeTurnQueue(%this)
{
   echo("distributeTurnQueue()");

   %playerVector = "";
   %aiVector = "";
   for (%i = 1; %i <= game.getPieceCount(); %i++)
   {
      if ($nappulaSprite[%i].groupid == 0 && $nappulaSprite[%i].alive == 1)
      {
         if (%playerVector $= "" || %playerVector == 0) 
         { 
            %playerVector = %i;
         }
         else
         {
            %playerVector = %playerVector SPC %i;
         }
      }
      
      if ($nappulaSprite[%i].groupid == 1 && $nappulaSprite[%i].alive == 1)
      {
         if (%aiVector $= "" || %aiVector == 0) 
         { 
            %aiVector = %i;
         }
         else
         {
            %aiVector = %aiVector SPC %i;
         }
      }
   }

   %aicount = getwordcount(%aiVector);
   %playercount = getwordcount(%playerVector);
   
   if (%aicount > 0 && %playercount > 0) 
   {
      if (%aicount > %playercount) 
      {
         for (%i = 0; %i < %aicount - %playercount; %i++)
         {
            %playerVector = %playerVector SPC "0";
         }
      } 
      else if (%aicount < %playercount) 
      {
         for (%i = 0; %i < %playercount - %aicount; %i++)
         {
            %aiVector = %aiVector SPC "0";
         }
      }

      %vectorSize = getwordcount(%playerVector);
      game.setTurnQueue(getword(%playerVector, 0) SPC getword(%aiVector, 0));
      for (%i = 1; %i < %vectorSize; %i++)
      {
         if (getword(%playerVector, %i) $= "0") 
         {
            game.setTurnQueue(game.getTurnQueue() SPC getword(%aiVector, %i));
         }
         else if (getword(%aiVector, %i) $= "0") 
         {
            game.setTurnQueue(game.getTurnQueue() SPC getword(%playerVector, %i));
         }
         else
         {
            game.setTurnQueue(game.getTurnQueue() SPC getword(%playerVector, %i) SPC getword(%aiVector, %i));
         }
      }
   } 
   else 
   {
      %this.setGameState($gameEndState);
      %this.endBattle();
   }
}

function TurnManager::removePieceTurn(%this, %pieceId)
{
   for (%i = 0; %i < getWordCount(game.getTurnQueue()); %i++)
   {
      if (getWord(game.getTurnQueue(), %i) == %pieceId) {
         game.setTurnQueue(removeWord(game.getTurnQueue(), %i));  
      }
   }
}

function TurnManager::nextTurn(%this)
{
   game.addTurn(1);

   deleteGrids(1);
   
   HeroGroup.getObject(0).setSelectedTile("-1 -1");
   HeroGroup.getObject(1).setSelectedTile("-1 -1");

   if (getWordCount(game.getTurnQueue()) == 0) 
   {
      %this.distributeTurnQueue();
   }

   $nappulaSprite[game.getSelectedPiece()].hasMoved = 0;
   $nappulaSprite[game.getSelectedPiece()].hasDoneAction = 0;
   
   if (%this.areUnitsAlive() == 1) {
      if (game.getState() != $gameEndState) {
         %pieceId = getword(game.getTurnQueue(), 0);
         
         echo("!!! pieceId:" SPC %pieceId SPC "turnQueue:" SPC game.getTurnQueue());
         
         game.setTurnQueue(removeWord(game.getTurnQueue(), 0));
         
         if ($nappulaSprite[%pieceId].alive == 0) {
            endgame();
         }
         
         game.setSelectedPiece(%pieceId);
         game.setActivePlayer($nappulaSprite[%pieceId].groupid);
         game.setActiveOpponent(1 - game.getActivePlayer());
          
         %piecePos = $nappulaSprite[%pieceId].getPosition();
         %piecePosX = $nappulaSprite[%pieceId].getPositionX();
         %piecePosY = $nappulaSprite[%pieceId].getPositionY();
               
         selectPiece(%pieceId);

         if (game.getActivePlayer() == 0) {
            sceneWindow2D.setUseWindowMouseEvents(true);
         } else {
            sceneWindow2D.setUseWindowMouseEvents(false);
         }
      }
   }
   else
   {
      game.setState($gameEndState);
      %this.endBattle();
   }
}

function TurnManager::endBattle(%this)
{
   sceneWindow2D.setUseWindowMouseEvents(false);
   %this.endBattleEvent = schedule(2000, 0, creditsscreen);
}

//---------------------------------------------------------------------------------------------
// Global Delegation Wrapper Stubs (Backward Compatibility)
//---------------------------------------------------------------------------------------------
function shutdown()                { TurnManager.shutdown(); }
function initStateMachine()        { TurnManager.initStateMachine(); }
function setGameState(%s)          { TurnManager.setGameState(%s); }
function getGameState()            { return TurnManager.getGameState(); }
function InitNewGame()             { TurnManager.initNewGame(); }
function InitGameForBattle()       { TurnManager.initGameForBattle(); }
function updateCountdownTimer()    { TurnManager.updateCountdownTimer(); }
function sekuntiLaskuri()          { TurnManager.updateCountdownTimer(); }
function dealCards()               { TurnManager.dealCards(); }
function jaaKortit()               { TurnManager.dealCards(); }
function updateActionIcons()       { TurnManager.updateActionIcons(); }
function areUnitsAlive()           { return TurnManager.areUnitsAlive(); }
function onkoNappulatElossa()      { return TurnManager.areUnitsAlive(); }
function distributeTurnQueue()     { TurnManager.distributeTurnQueue(); }
function jaaNappuloidenVuorot()    { TurnManager.distributeTurnQueue(); }
function removePieceTurn(%id)      { TurnManager.removePieceTurn(%id); }
function poistaNappulanVuoro(%id)  { TurnManager.removePieceTurn(%id); }
function nextTurn()                { TurnManager.nextTurn(); }
function seuraavaVuoro()           { TurnManager.nextTurn(); }
function endBattle()               { TurnManager.endBattle(); }
function endbattle()               { TurnManager.endBattle(); }

// Initialize Singleton
if (!isObject(TurnManager))
{
   new ScriptObject(TurnManager) { };
}
