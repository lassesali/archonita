   new ScriptObject(Hero) 
   { 
   };

new SimGroup(HeroGroup) {
      


   new ScriptObject(Player) 
   { 
      class = Hero;
      olentoPisteet = "5";
      valittuRuutuX = "-1";
      valittuRuutuY = "-1";
      valittuRuutu  = "-1 -1";
      mana = "0";
      manaMAX = "0";
      ammatti = "0";     
   };

   new ScriptObject(AI) 
   { 
      class = Hero;
      olentoPisteet = "5";
      valittuRuutuX = "-1";
      valittuRuutuY = "-1";
      valittuRuutu  = "-1 -1";
      mana = "0";
      manaMAX = "0";
      ammatti = "0";
   };



};



function Hero::addOlentopisteet(%this, %add)
{
   %this.olentoPisteet = %this.olentoPisteet + %add;
}

function Hero::addMana(%this, %add)
{
   %this.mana = %this.mana + %add;
   %this.manaMAX = %this.manaMAX + %add;
}

function Hero::subOlentopisteet(%this, %sub)
{
   %this.olentoPisteet = %this.olentoPisteet - %sub;
}


function Hero::addNappulatLkm(%this, %add)
{
   %this.nappulatLkm = %this.nappulatLkm + %add;
}

function Hero::setValittuRuutu(%this, %xy)
{
   %this.valittuRuutu = %xy;
   %this.setValittuRuutuX( getWord(%xy, 0) );
   %this.setValittuRuutuY( getWord(%xy, 1) );
}

function Hero::getValittuRuutu(%this)
{
   return %this.valittuRuutu;
}


function Hero::setValittuRuutuY(%this, %y)
{
   %this.valittuRuutuY = %y;
}

function Hero::getValittuRuutuY(%this)
{
   return %this.valittuRuutuY;
}


function Hero::setValittuRuutuX(%this, %x)
{
   %this.valittuRuutuX = %x;
}

function Hero::getValittuRuutuX(%this)
{
   return %this.valittuRuutuX;
}


function Hero::setAmmatti(%this, %a)
{
   %this.ammatti = %a;
}


function Hero::getOlentopisteet(%this)
{
   return %this.olentoPisteet;
}

function Hero::getAmmatti(%this)
{
   return %this.ammatti;
}