function ABArrow::onMouseOver(%this, %pos)
{
   ABHover::onMouseOver(%this);   
}

function ABArrow::onMouseLeave(%this, %pos)
{
   ABHover::onMouseLeave(%this);   
}

function ABArrow::onMouseDown(%this, %pos)
{
   ABHover::onMouseDown(%this);   
   
   if(!%this.downAction)
   {
      %this.scroll(%this.direc);
      %this.downAction = true;
   }
}

function ABArrow::onMouseUp(%this, %pos)
{
   ABHover::onMouseUp(%this);   
   
   if(%this.downAction)
   {
      if(isEventPending(%this.downSchedule))
         cancel(%this.downSchedule);

      %this.downAction = false;
   }
}

function ABArrow::scroll(%this, %direc)
{
   switch$(%direc)
   {
      case "up":
         %selectionPercent = %this.parentObj.getSelectionPosition();
         %selectionPercent /= 100;
         %selectionPercent -= 0.05;
         %this.parentObj.setSelectionPercent(%selectionPercent);   
      
      case "down":
         %selectionPercent = %this.parentObj.getSelectionPosition();
         %selectionPercent /= 100;
         %selectionPercent += 0.05;
         %this.parentObj.setSelectionPercent(%selectionPercent);   
   }
   
   %this.downSchedule = %this.schedule(100, "scroll", %direc);
}