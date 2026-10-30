new ScriptObject(game) 
{ 
};
   
   
function game::setHighlightedActionIcon(%this, %icon)
{
   %this.highlightedicon = %icon;
}

function game::getHighlightedActionIcon(%this)
{
   return %this.highlightedicon;
}


function game::setNappulaSelected(%this, %nappulaid)
{
   %this.nappulaSelected = %nappulaid;
}

function game::getNappulaSelected(%this)
{
   return %this.nappulaSelected;
}


function game::setVuorossaOlevaPelaaja(%this, %a)
{
   %this.vuorossaOlevaPelaaja = %a;
}

function game::getVuorossaOlevaPelaaja(%this)
{
   return %this.vuorossaOlevaPelaaja;
}

function game::setVuorossaOlevanPelaajanVastustaja(%this, %a)
{
   %this.vuorossaOlevanPelaajanVastustaja = %a;
}

function game::getVuorossaOlevanPelaajanVastustaja(%this)
{
   return %this.vuorossaOlevanPelaajanVastustaja;
}

function game::addVuoro(%this, %add)
{
   %this.vuoro = %this.vuoro + %a;
}

function game::addPeliTimer(%this, %add)
{
   %this.timer = %this.timer + %add;
}

function game::setPeliTimer(%this, %a)
{
   %this.timer = %a;
}

function game::getPeliTimer(%this)
{
   return %this.timer;
}


function game::subPeliTimer(%this, %sub)
{
   %this.timer = %this.timer - %sub;
}
      
         
function game::setVuoro(%this, %a)
{
   %this.vuoro = %a;
}

function game::getVuoro(%this)
{

   return %this.vuoro;
   
}

function game::setVuorotVector(%this, %a)
{
   %this.vuorotVector = %a;
}

function game::getVuorotVector(%this)
{
   
   return %this.vuorotVector;
   
}
         
function game::setState(%this, %state)
{
   %this.state = %state;
}

function game::getState(%this)
{

   return %this.state;
   
}


function game::setCountOfNappula(%this, %a)
{
   
   %this.countOfNappula = %a;

}

function game::getCountOfNappula(%this)
{

   return %this.countOfNappula;
   
}

function game::addCountOfNappula(%this, %add)
{

   %this.countOfNappula = %this.countOfNappula + %add;
   
}