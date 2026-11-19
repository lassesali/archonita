//---------------------------------------------------------------------------------------------
// Archonita - TurnManager.cs
// Session lifecycle, turn sequencing, game states, card dealing, and initiative queue.
//---------------------------------------------------------------------------------------------

function TurnManager::onAdd(%this)
{
   %this.turnTimerEvent = 0;
   %this.cardDealEvent = 0;
   %this.endBattleEvent = 0;
   %this.lastBattleOutcome = "VICTORY";
   %this.lastBattleIsVictory = true;
   %this.lastBattleTurns = 0;
   %this.lastPlayerSurviving = 0;
   %this.lastPlayerTotal = 0;
   %this.lastPlayerCasualties = 0;
   %this.lastPlayerSurvivingHP = 0;
   %this.lastEnemySurviving = 0;
   %this.lastEnemyTotal = 0;
   %this.lastEnemyCasualties = 0;
   %this.lastEnemySurvivingHP = 0;
   %this.lastSurvivingUnitCount = 0;
   %this.lastEnemyFactionName = "Undead Legion";
   %this.lastHeroClassName = "Warrior";
   %this.lastSurvivingTallyText = "";
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
   $gameCreditsState = new ScriptObject() { };
   $gameCardSelectState = new ScriptObject() { };
   $gamePreBattleState = new ScriptObject() { };
   $gamePreBattleTimerState = new ScriptObject() { };
   $gameTimerBattleState = new ScriptObject() { };
   $gameBattleState = new ScriptObject() { };
   $gameEndState = new ScriptObject() { };  
   
   $gameCreditsState.name = "Credits State";
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
   if (isEventPending(%this.endBattleEvent))
   {
      cancel(%this.endBattleEvent);
      %this.endBattleEvent = 0;
   }
   if (isObject($BattleSummaryGui))
      $BattleSummaryGui.show(false);
   if (isObject($UnitStatsHud))
   {
      $UnitStatsHud.clearBadges();
      $UnitStatsHud.showBadges(true);
   }

   if (isObject(game))
      game.reset();
   if (isObject(HeroGroup))
   {
      if (isObject(HeroGroup.getObject(0)))
         HeroGroup.getObject(0).reset();
      if (isObject(HeroGroup.getObject(1)))
         HeroGroup.getObject(1).reset();
   }

   game.setTurn(0);
   %this.shuffleSpellDeck();

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
   if (isObject(secondCounter))
      secondCounter.setBlendAlpha(0);

   if (game.getGameTimer() == 0) {
      secondText.setVisible(false);
      secondCounter.setVisible(false);
      if (isObject($SceneTextOverlayGui))
         $SceneTextOverlayGui.refreshAll();
      %this.setGameState($gameTimerBattleState);
      updateActionIcons();
      %this.initGameForBattle();
   } else {
      if (isObject($SceneTextOverlayGui))
         $SceneTextOverlayGui.refreshAll();
      %this.turnTimerEvent = %this.schedule(1000, "updateCountdownTimer");
      $secondSound = playArchonSound(ambient4Audio);
   }
}

function TurnManager::shuffleSpellDeck(%this)
{
   %frames = "3 4 5 6 7 8";
   for (%i = 1; %i <= 6; %i++)
   {
      %count = getWordCount(%frames);
      %pick = getRandom(%count - 1);
      $spellDeck[%i] = getWord(%frames, %pick);
      %frames = removeWord(%frames, %pick);
   }
   $cardCount = 0;
   $topCardIndex = 6;
   game.setCanCastSpell(false);
   game.setSpellRevealed(false);
   updateMagicButtonParticleState();
}

function TurnManager::dealCards(%this)
{
   if ($cardCount $= "" || $spellDeck[1] $= "")
      %this.shuffleSpellDeck();

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
   $specialcard[%i].spellFrame = $spellDeck[%i];
   $specialcard[%i].setVisible(true);
   $specialcard[%i].setLayer(10 - %i);
   $specialcard[%i].setUseMouseEvents(0);
   $specialcard[%i].isSelectable = false;
   $specialcard[%i].moveto("-40" SPC (0 + (%i * 0.5)), 50, true, true, 0.1);
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
   %playerVector = "";
   %aiVector = "";
   for (%i = 1; %i <= game.getPieceCount(); %i++)
   {
      if (!isObject($nappulaSprite[%i]))
         continue;

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

      if ($topCardIndex >= 1)
      {
         game.setCanCastSpell(true);
         game.setSpellRevealed(false);
      }
      updateMagicButtonParticleState();
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

   if (game.getState() == $gameEndState)
      return;

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

         if ($nappulaSprite[%pieceId].skipNextTurn == 1)
         {
            $nappulaSprite[%pieceId].skipNextTurn = 0;
            $nappulaSprite[%pieceId].hasMoved = 1;
            $nappulaSprite[%pieceId].hasDoneAction = 1;
            if (isObject(SpellEffectManager))
               SpellEffectManager.spawnEffectOnPiece($nappulaSprite[%pieceId], "Archon:spell_disease");
            if (isObject($UnitStatsHud))
               $UnitStatsHud.refreshAll();
            sceneWindow2D.setUseWindowMouseEvents(false);
            schedule(800, 0, "doDefence", game.getTurn());
            return;
         }

         if (game.getActivePlayer() == 0) {
            sceneWindow2D.setUseWindowMouseEvents(true);
         } else {
            sceneWindow2D.setUseWindowMouseEvents(false);
         }
         updateMagicButtonParticleState();
         if (isObject($UnitStatsHud))
            $UnitStatsHud.refreshAll();
      }
   }
   else
   {
      game.setState($gameEndState);
      %this.endBattle();
   }
}

function TurnManager::calculateBattleSummary(%this)
{
   for (%k = 0; %k < %this.lastSurvivingUnitCount; %k++)
      %this.survivingUnit[%k] = "";

   %pieceCount = isObject(game) ? game.getPieceCount() : 0;
   %playerTotal = 0;
   %playerSurviving = 0;
   %playerSurvivingHP = 0;
   %enemyTotal = 0;
   %enemySurviving = 0;
   %enemySurvivingHP = 0;
   %survivingCount = 0;

   for (%i = 1; %i <= %pieceCount; %i++)
   {
      %piece = $nappulaSprite[%i];
      if (!isObject(%piece))
         continue;

      if (%piece.groupid == 0)
      {
         %playerTotal++;
         if (%piece.alive == 1)
         {
            %playerSurviving++;
            %hp = (%piece.hp > 0) ? %piece.hp : 0;
            %playerSurvivingHP += %hp;
            %this.survivingUnit[%survivingCount] = %piece;
            %survivingCount++;
         }
      }
      else if (%piece.groupid == 1)
      {
         %enemyTotal++;
         if (%piece.alive == 1)
         {
            %enemySurviving++;
            %hp = (%piece.hp > 0) ? %piece.hp : 0;
            %enemySurvivingHP += %hp;
            %this.survivingUnit[%survivingCount] = %piece;
            %survivingCount++;
         }
      }
   }

   %isVictory = (%playerSurviving > 0 && (%enemySurviving == 0 || %playerSurviving >= %enemySurviving));
   %outcome = %isVictory ? "VICTORY" : "DEFEAT";
   %turns = isObject(game) ? game.getTurn() : 0;
   %enemyFactionName = isObject($FactionManager) ? $FactionManager.getEnemyFactionName() : "Undead Legion";

   %heroClassId = 0;
   if (isObject(HeroGroup) && HeroGroup.getCount() > 0 && isObject(HeroGroup.getObject(0)))
      %heroClassId = HeroGroup.getObject(0).getHeroClass();
   %heroClassName = (%heroClassId == 1) ? "Mage" : "Warrior";

   %tallyText = "";
   if (%survivingCount == 0)
   {
      %tallyText = isObject($LocalizationManager) ? $LocalizationManager.getText("summary_none_short") : "None";
   }
   else
   {
      for (%k = 0; %k < %survivingCount; %k++)
      {
         %unit = %this.survivingUnit[%k];
         %row = isObject($BattleSummaryGui)
            ? $BattleSummaryGui.formatSurvivingUnitRow(%unit)
            : (%unit.name @ " (HP: " @ %unit.hp @ ")");
         %tallyText = (%tallyText $= "") ? %row : (%tallyText @ "\n" @ %row);
      }
   }

   %this.lastBattleOutcome = %outcome;
   %this.lastBattleIsVictory = %isVictory;
   %this.lastBattleTurns = %turns;
   %this.lastPlayerSurviving = %playerSurviving;
   %this.lastPlayerTotal = %playerTotal;
   %this.lastPlayerCasualties = (%playerTotal > %playerSurviving) ? (%playerTotal - %playerSurviving) : 0;
   %this.lastPlayerSurvivingHP = %playerSurvivingHP;
   %this.lastEnemySurviving = %enemySurviving;
   %this.lastEnemyTotal = %enemyTotal;
   %this.lastEnemyCasualties = (%enemyTotal > %enemySurviving) ? (%enemyTotal - %enemySurviving) : 0;
   %this.lastEnemySurvivingHP = %enemySurvivingHP;
   %this.lastSurvivingUnitCount = %survivingCount;
   %this.lastEnemyFactionName = %enemyFactionName;
   %this.lastHeroClassName = %heroClassName;
   %this.lastSurvivingTallyText = %tallyText;

   return %outcome;
}

function TurnManager::getLastBattleOutcome(%this)      { return %this.lastBattleOutcome; }
function TurnManager::getPlayerSurvivingCount(%this)  { return %this.lastPlayerSurviving; }
function TurnManager::getPlayerTotalCount(%this)      { return %this.lastPlayerTotal; }
function TurnManager::getEnemySurvivingCount(%this)   { return %this.lastEnemySurviving; }
function TurnManager::getEnemyTotalCount(%this)       { return %this.lastEnemyTotal; }
function TurnManager::getSurvivingUnitCount(%this)    { return %this.lastSurvivingUnitCount; }
function TurnManager::getSurvivingUnitTallyText(%this){ return %this.lastSurvivingTallyText; }

function TurnManager::endBattle(%this)
{
   if (isEventPending(%this.endBattleEvent))
   {
      cancel(%this.endBattleEvent);
      %this.endBattleEvent = 0;
   }
   if (isEventPending(%this.turnTimerEvent))
   {
      cancel(%this.turnTimerEvent);
      %this.turnTimerEvent = 0;
   }
   if (isEventPending(%this.cardDealEvent))
   {
      cancel(%this.cardDealEvent);
      %this.cardDealEvent = 0;
   }

   if (isObject(game))
      game.setState($gameEndState);

   $GameSession::hasActiveSession = false;
   if (isObject($BoardAudioHud))
      $BoardAudioHud.show(false);
   if (isObject($UnitStatsHud))
      $UnitStatsHud.clearBadges();
   if (isObject($SceneTextOverlayGui))
      $SceneTextOverlayGui.refreshAll();
   if (isObject(CombatEngine))
   {
      CombatEngine.cancelDiceRoll();
      CombatEngine.cleanupProjectiles();
   }
   if (isObject(TacticalAI))
      TacticalAI.cancelPendingEvents();
   if (isObject(BoardManager))
      BoardManager.deleteGrids(1);
   if (isObject(sceneWindow2D))
      sceneWindow2D.setUseWindowMouseEvents(false);
   updateMagicButtonParticleState();
   if (isObject(FloatingTextManager))
      FloatingTextManager.clearAll();
   if (isObject(SpellEffectManager))
      SpellEffectManager.clearAll();

   %this.calculateBattleSummary();

   if (isObject($BattleSummaryGui) && isObject(mainScreenGui))
   {
      $BattleSummaryGui.showSummary();
   }
   else
   {
      %this.endBattleEvent = schedule(2000, 0, mainmenuScreen);
   }
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
function shuffleSpellDeck()        { TurnManager.shuffleSpellDeck(); }
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
