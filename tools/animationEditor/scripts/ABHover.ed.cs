function ABHover::onMouseOver(%this, %pos)
{
   if(!%this.hoveringImage)
   {
      %this.setFrame(%this.hoverImage);
      %this.hoveringImage = true;      
   }
}

function ABHover::onMouseLeave(%this, %pos)
{
   if(%this.hoveringImage)
   {
      %this.setFrame(%this.normalImage);
      %this.hoveringImage = false;      
   }
}

function ABHover::onMouseDown(%this, %pos)
{
   if(!%this.depressedImage)
   {
      %this.setFrame(%this.downImage);
      %this.depressedImage = true;      
   }
}

function ABHover::onMouseUp(%this, %pos)
{
   if(%this.depressedImage)
   {
      if(!%this.hoveringImage)
      {
         %this.setFrame(%this.normalImage);
      } else
      {
         %this.setFrame(%this.hoverImage);        
      }
      
      %this.depressedImage = false; 
   }
}

function ABHoverSelectionBar::onMouseOver(%this, %pos)
{
   if(!%this.hoveringImage)
   {
      %this.setFrame(%this.hoverImage);
      %this.parentObj.selectionBarTop.setFrame(%this.parentObj.selectionBarTop.hoverImage);
      %this.parentObj.selectionBarBottom.setFrame(%this.parentObj.selectionBarBottom.hoverImage);
      %this.hoveringImage = true;      
   }
}



function ABHoverSelectionBar::onMouseLeave(%this, %pos)
{
   if(%this.hoveringImage)
   {
      %this.setFrame(%this.normalImage);
      %this.parentObj.selectionBarTop.setFrame(%this.parentObj.selectionBarTop.normalImage);
      %this.parentObj.selectionBarBottom.setFrame(%this.parentObj.selectionBarBottom.normalImage);
      %this.hoveringImage = false;      
   }
   
   %this.selected = false;
}

function ABHoverSelectionBar::onMouseDragged(%this, %pos)
{
   if(!%this.selected)
      return;
   
   %posY = getWord(%pos, 1);
   
   %scrollBarPos = %this.getPosition();
   %scrollBarPosY = getWord(%scrollBarPos, 1);
   
   %diff = %posY - %this.selectedY;
   %this.selectedY = %posY; 
   %posY = %scrollBarPosY + %diff;
   
   %this.parentObj.setSelectionPosition(0 SPC %posY);
}

function ABHoverSelectionBar::onMouseDown(%this, %pos)
{
   %posY = getWord(%pos, 1);
   
   %this.selectedY = %posY;   
   
   %this.selected = true;
}

function ABHoverSelectionBar::onMouseUp(%this, %pos)
{
}

function ABHoverSelectionBarCap::onMouseOver(%this, %pos)
{
   if(!%this.parentObj.selectionBar.hoveringImage)
   {
      %this.parentObj.selectionBar.setFrame(%this.parentObj.selectionBar.hoverImage);
      %this.parentObj.selectionBarTop.setFrame(%this.parentObj.selectionBarTop.hoverImage);
      %this.parentObj.selectionBarBottom.setFrame(%this.parentObj.selectionBarBottom.hoverImage);
      %this.parentObj.selectionBar.hoveringImage = true;      
   }
}

function ABHoverSelectionBarCap::onMouseLeave(%this, %pos)
{
   if(%this.parentObj.selectionBar.hoveringImage)
   {
      %this.parentObj.selectionBar.setFrame(%this.parentObj.selectionBar.normalImage);
      %this.parentObj.selectionBarTop.setFrame(%this.parentObj.selectionBarTop.normalImage);
      %this.parentObj.selectionBarBottom.setFrame(%this.parentObj.selectionBarBottom.normalImage);
      %this.parentObj.selectionBar.hoveringImage = false;      
   }
}

function ABHoverSelectionBarCap::onMouseDragged(%this, %pos)
{
   ABHoverSelectionBar::onMouseDragged(%this.parentObj.selectionBar, %pos);
}