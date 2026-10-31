//---------------------------------------------------------------------------------------------
// Archonita - Hero Data Structure
// Defines player and AI hero stats, selected board coordinates, mana, and creature points.
//---------------------------------------------------------------------------------------------

new ScriptObject(Hero) 
{ 
};

new SimGroup(HeroGroup) 
{
   new ScriptObject(Player) 
   { 
      class = Hero;
      creaturePoints = 5;
      olentoPisteet = 5;

      selectedTile = "-1 -1";
      selectedTileX = -1;
      selectedTileY = -1;
      valittuRuutu  = "-1 -1";
      valittuRuutuX = -1;
      valittuRuutuY = -1;

      mana = 0;
      manaMAX = 0;
      heroClass = 0;
      ammatti = 0;
      pieceCount = 0;
      nappulatLkm = 0;
   };

   new ScriptObject(AI) 
   { 
      class = Hero;
      creaturePoints = 5;
      olentoPisteet = 5;

      selectedTile = "-1 -1";
      selectedTileX = -1;
      selectedTileY = -1;
      valittuRuutu  = "-1 -1";
      valittuRuutuX = -1;
      valittuRuutuY = -1;

      mana = 0;
      manaMAX = 0;
      heroClass = 0;
      ammatti = 0;
      pieceCount = 0;
      nappulatLkm = 0;
   };
};

//---------------------------------------------------------------------------------------------
// Creature Points (Recruitment Budget)
//---------------------------------------------------------------------------------------------
function Hero::addCreaturePoints(%this, %add)
{
   %this.creaturePoints = %this.creaturePoints + %add;
   %this.olentoPisteet = %this.creaturePoints;
}

function Hero::subCreaturePoints(%this, %sub)
{
   %this.creaturePoints = %this.creaturePoints - %sub;
   %this.olentoPisteet = %this.creaturePoints;
}

function Hero::getCreaturePoints(%this)
{
   if (%this.creaturePoints $= "")
      %this.creaturePoints = (%this.olentoPisteet !$= "") ? %this.olentoPisteet : 0;
   return %this.creaturePoints;
}

// Backward-compatible aliases for Creature Points
function Hero::addOlentopisteet(%this, %add) { %this.addCreaturePoints(%add); }
function Hero::subOlentopisteet(%this, %sub) { %this.subCreaturePoints(%sub); }
function Hero::getOlentopisteet(%this)       { return %this.getCreaturePoints(); }

//---------------------------------------------------------------------------------------------
// Mana
//---------------------------------------------------------------------------------------------
function Hero::addMana(%this, %add)
{
   %this.mana = %this.mana + %add;
   %this.manaMAX = %this.manaMAX + %add;
}

function Hero::getMana(%this)
{
   return %this.mana;
}

//---------------------------------------------------------------------------------------------
// Piece Count
//---------------------------------------------------------------------------------------------
function Hero::addPieceCount(%this, %add)
{
   %this.pieceCount = %this.pieceCount + %add;
   %this.nappulatLkm = %this.pieceCount;
}

function Hero::subPieceCount(%this, %sub)
{
   %this.pieceCount = %this.pieceCount - %sub;
   %this.nappulatLkm = %this.pieceCount;
}

function Hero::getPieceCount(%this)
{
   if (%this.pieceCount $= "")
      %this.pieceCount = (%this.nappulatLkm !$= "") ? %this.nappulatLkm : 0;
   return %this.pieceCount;
}

// Backward-compatible aliases for Piece Count
function Hero::addNappulatLkm(%this, %add) { %this.addPieceCount(%add); }
function Hero::subNappulatLkm(%this, %sub) { %this.subPieceCount(%sub); }
function Hero::getNappulatLkm(%this)       { return %this.getPieceCount(); }

//---------------------------------------------------------------------------------------------
// Selected Board Coordinates
//---------------------------------------------------------------------------------------------
function Hero::setSelectedTile(%this, %xy)
{
   %this.selectedTile = %xy;
   %this.valittuRuutu = %xy;
   %this.setSelectedTileX(getWord(%xy, 0));
   %this.setSelectedTileY(getWord(%xy, 1));
}

function Hero::getSelectedTile(%this)
{
   return %this.selectedTile;
}

function Hero::setSelectedTileX(%this, %x)
{
   %this.selectedTileX = %x;
   %this.valittuRuutuX = %x;
}

function Hero::getSelectedTileX(%this)
{
   return %this.selectedTileX;
}

function Hero::setSelectedTileY(%this, %y)
{
   %this.selectedTileY = %y;
   %this.valittuRuutuY = %y;
}

function Hero::getSelectedTileY(%this)
{
   return %this.selectedTileY;
}

// Backward-compatible aliases for Selected Board Coordinates
function Hero::setValittuRuutu(%this, %xy) { %this.setSelectedTile(%xy); }
function Hero::getValittuRuutu(%this)      { return %this.getSelectedTile(); }
function Hero::setValittuRuutuX(%this, %x) { %this.setSelectedTileX(%x); }
function Hero::getValittuRuutuX(%this)     { return %this.getSelectedTileX(); }
function Hero::setValittuRuutuY(%this, %y) { %this.setSelectedTileY(%y); }
function Hero::getValittuRuutuY(%this)     { return %this.getSelectedTileY(); }

//---------------------------------------------------------------------------------------------
// Hero Profession / Class (0 = Warrior, 1 = Mage)
//---------------------------------------------------------------------------------------------
function Hero::setHeroClass(%this, %classId)
{
   %this.heroClass = %classId;
   %this.ammatti = %classId;
}

function Hero::getHeroClass(%this)
{
   if (%this.heroClass $= "")
      %this.heroClass = (%this.ammatti !$= "") ? %this.ammatti : 0;
   return %this.heroClass;
}

// Backward-compatible aliases for Hero Class
function Hero::setAmmatti(%this, %a) { %this.setHeroClass(%a); }
function Hero::getAmmatti(%this)     { return %this.getHeroClass(); }
