//---------------------------------------------------------------------------------------------
// Archonita - Deck.cs
// Spell card deck orchestrator (ScriptObject subclass).
// Conforms strictly to TORQUE_SCRIPT.md:
// - One class per file, matching filename.
// - Lifecycle in onAdd / onRemove using house init() convention.
// - Owns card references until popped/cast; relinquishes lifetime to Scene & Card upon discard.
//---------------------------------------------------------------------------------------------

function Deck::init(%this)
{
   if (!isObject(%this.cards))
      %this.cards = new SimSet();
}

function Deck::onAdd(%this)
{
   %this.init();
}

function Deck::onRemove(%this)
{
   // Cleanup any un-dealt or un-transferred cards still owned by this Deck
   if (isObject(%this.cards))
   {
      while (%this.cards.getCount() > 0)
      {
         %card = %this.cards.getObject(0);
         %this.cards.remove(%card);
         if (isObject(%card))
            %card.delete();
      }
      %this.cards.delete();
      %this.cards = "";
   }
}

//---------------------------------------------------------------------------------------------
// Card Stack Operations
//---------------------------------------------------------------------------------------------
function Deck::pushCard(%this, %card)
{
   if (isObject(%this.cards) && isObject(%card))
      %this.cards.add(%card);
}

function Deck::popTopCard(%this)
{
   if (!isObject(%this.cards) || %this.cards.getCount() == 0)
      return 0;

   %lastIndex = %this.cards.getCount() - 1;
   %topCard = %this.cards.getObject(%lastIndex);
   %this.cards.remove(%topCard);
   return %topCard;
}

function Deck::getTopCard(%this)
{
   if (!isObject(%this.cards) || %this.cards.getCount() == 0)
      return 0;

   %lastIndex = %this.cards.getCount() - 1;
   return %this.cards.getObject(%lastIndex);
}

function Deck::getCardCount(%this)
{
   if (isObject(%this.cards))
      return %this.cards.getCount();
   return 0;
}

//---------------------------------------------------------------------------------------------
// Post-Cast Discard Orchestration
//---------------------------------------------------------------------------------------------
function Deck::discardTopCard(%this, %cameraBounds)
{
   %topCard = %this.popTopCard();
   if (isObject(%topCard))
   {
      // Relinquish ownership from Deck:
      // The Scene and the Card's out-of-bounds lifecycle handle its deletion.
      %topCard.discardAfterCast(%cameraBounds);
   }
   return %topCard;
}
