//---------------------------------------------------------------------------------------------
// Archonita - Game Session State Object
// Central session tracker for turns, active player, initiative queues, timers, and pieces.
//---------------------------------------------------------------------------------------------

new ScriptObject(game) 
{ 
   highlightedIcon = 0;
   selectedAction = 0;
   selectedPiece = 0;
   activePlayer = 0;
   activeOpponent = 1;
   turn = 0;
   gameTimer = 0;
   turnQueue = "";
   state = "";
   pieceCount = 0;
};

//---------------------------------------------------------------------------------------------
// Action Icon Highlighting & Selection
//---------------------------------------------------------------------------------------------
function game::setHighlightedActionIcon(%this, %icon)
{
   %this.highlightedIcon = %icon;
   %this.highlightedicon = %icon;
}

function game::getHighlightedActionIcon(%this)
{
   return %this.highlightedIcon;
}

function game::setSelectedAction(%this, %action)
{
   %this.selectedAction = %action;
}

function game::getSelectedAction(%this)
{
   if (%this.selectedAction $= "")
      return 0;
   return %this.selectedAction;
}

//---------------------------------------------------------------------------------------------
// Selected Piece ID
//---------------------------------------------------------------------------------------------
function game::setSelectedPiece(%this, %pieceId)
{
   %this.selectedPiece = %pieceId;
   %this.nappulaSelected = %pieceId;
}

function game::getSelectedPiece(%this)
{
   if (%this.selectedPiece $= "")
      %this.selectedPiece = (%this.nappulaSelected !$= "") ? %this.nappulaSelected : 0;
   return %this.selectedPiece;
}

function game::setNappulaSelected(%this, %id) { %this.setSelectedPiece(%id); }
function game::getNappulaSelected(%this)     { return %this.getSelectedPiece(); }

//---------------------------------------------------------------------------------------------
// Active Player (0 = Human, 1 = AI)
//---------------------------------------------------------------------------------------------
function game::setActivePlayer(%this, %playerId)
{
   %this.activePlayer = %playerId;
   %this.vuorossaOlevaPelaaja = %playerId;
}

function game::getActivePlayer(%this)
{
   return %this.activePlayer;
}

function game::setVuorossaOlevaPelaaja(%this, %p) { %this.setActivePlayer(%p); }
function game::getVuorossaOlevaPelaaja(%this)     { return %this.getActivePlayer(); }

//---------------------------------------------------------------------------------------------
// Active Opponent (1 - Active Player)
//---------------------------------------------------------------------------------------------
function game::setActiveOpponent(%this, %opponentId)
{
   %this.activeOpponent = %opponentId;
   %this.vuorossaOlevanPelaajanVastustaja = %opponentId;
}

function game::getActiveOpponent(%this)
{
   return %this.activeOpponent;
}

function game::setVuorossaOlevanPelaajanVastustaja(%this, %o) { %this.setActiveOpponent(%o); }
function game::getVuorossaOlevanPelaajanVastustaja(%this)     { return %this.getActiveOpponent(); }

//---------------------------------------------------------------------------------------------
// Turn Counter
//---------------------------------------------------------------------------------------------
function game::setTurn(%this, %turn)
{
   %this.turn = %turn;
   %this.vuoro = %turn;
}

function game::getTurn(%this)
{
   return %this.turn;
}

function game::addTurn(%this, %add)
{
   if (%add $= "") %add = 1;
   %this.setTurn(%this.getTurn() + %add);
}

function game::setVuoro(%this, %v) { %this.setTurn(%v); }
function game::getVuoro(%this)     { return %this.getTurn(); }
function game::addVuoro(%this, %a) { %this.addTurn(%a); }

//---------------------------------------------------------------------------------------------
// Turn Queue / Initiative Vector
//---------------------------------------------------------------------------------------------
function game::setTurnQueue(%this, %queue)
{
   %this.turnQueue = %queue;
   %this.vuorotVector = %queue;
}

function game::getTurnQueue(%this)
{
   return %this.turnQueue;
}

function game::setVuorotVector(%this, %v) { %this.setTurnQueue(%v); }
function game::getVuorotVector(%this)     { return %this.getTurnQueue(); }

//---------------------------------------------------------------------------------------------
// Countdown Timer (Pre-battle placement)
//---------------------------------------------------------------------------------------------
function game::setGameTimer(%this, %time)
{
   %this.gameTimer = %time;
   %this.timer = %time;
}

function game::getGameTimer(%this)
{
   return %this.gameTimer;
}

function game::addGameTimer(%this, %add)
{
   %this.setGameTimer(%this.getGameTimer() + %add);
}

function game::subGameTimer(%this, %sub)
{
   %this.setGameTimer(%this.getGameTimer() - %sub);
}

function game::setPeliTimer(%this, %t) { %this.setGameTimer(%t); }
function game::getPeliTimer(%this)     { return %this.getGameTimer(); }
function game::addPeliTimer(%this, %a) { %this.addGameTimer(%a); }
function game::subPeliTimer(%this, %s) { %this.subGameTimer(%s); }

//---------------------------------------------------------------------------------------------
// Game State
//---------------------------------------------------------------------------------------------
function game::setState(%this, %state)
{
   %this.state = %state;
}

function game::getState(%this)
{
   return %this.state;
}

//---------------------------------------------------------------------------------------------
// Piece Count Tracker
//---------------------------------------------------------------------------------------------
function game::setPieceCount(%this, %count)
{
   %this.pieceCount = %count;
   %this.countOfNappula = %count;
}

function game::getPieceCount(%this)
{
   return %this.pieceCount;
}

function game::addPieceCount(%this, %add)
{
   if (%add $= "") %add = 1;
   %this.setPieceCount(%this.getPieceCount() + %add);
}

function game::setCountOfNappula(%this, %c) { %this.setPieceCount(%c); }
function game::getCountOfNappula(%this)     { return %this.getPieceCount(); }
function game::addCountOfNappula(%this, %a) { %this.addPieceCount(%a); }
