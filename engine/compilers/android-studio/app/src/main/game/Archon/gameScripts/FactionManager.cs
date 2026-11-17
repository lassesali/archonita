//---------------------------------------------------------------------------------------------
// Archonita - FactionManager.cs
// Manages available enemy factions (Undead Legion, Dragonfolk Faction, Gremlin Faction)
// and tracks the current enemy faction selected by the player.
//---------------------------------------------------------------------------------------------

function FactionManager::onAdd(%this)
{
   %this.factionKeys = "undead" TAB "dragonfolk" TAB "gremlin";
   %this.selectedEnemyFaction = "undead";
   $selectedEnemyFaction = %this.selectedEnemyFaction;

   // 1. Undead Legion
   %this.name["undead"] = "Undead Legion";
   %this.subtitle["undead"] = "The Risen Dead";
   %this.offset["undead"] = 4;
   %this.units["undead"] = "Skeleton Warrior (Lvl 1)" TAB "Skeleton Archer (Lvl 2)" TAB "Dread Zombie (Lvl 3)" TAB "Ancient Ghost (Lvl 4)";
   %this.description["undead"] = "Cursed skeletal warriors and vengeful spectres arising from ancient crypts. Wields Ghostly Touch and Disease afflictions.";

   // 2. Dragonfolk Faction
   %this.name["dragonfolk"] = "Dragonfolk Faction";
   %this.subtitle["dragonfolk"] = "The Scaled Clan";
   %this.offset["dragonfolk"] = 8;
   %this.units["dragonfolk"] = "Dragon Folk (Lvl 1)" TAB "Red Dragon (Lvl 2)" TAB "Air Dragon (Lvl 3)" TAB "Gold Dragon (Lvl 4)";
   %this.description["dragonfolk"] = "Proud reptilian warriors, flame-breathing drakes, and sovereign gold dragons commanding elemental breath and draconic might.";

   // 3. Gremlin Faction
   %this.name["gremlin"] = "Gremlin Faction";
   %this.subtitle["gremlin"] = "The Underworld Horde";
   %this.offset["gremlin"] = 12;
   %this.units["gremlin"] = "Gremlin (Lvl 1)" TAB "Horned One-Eye (Lvl 2)" TAB "Mistress (Lvl 3)" TAB "Demon (Lvl 4)";
   %this.description["gremlin"] = "Crafty underworld imps, cyclopean eye-beam horrors, whips, and towering infernal demons surging from the subterranean abyss.";
}

function FactionManager::onRemove(%this)
{
   // No owned child SimObjects to delete
}

function FactionManager::getFactionCount(%this)
{
   return getFieldCount(%this.factionKeys);
}

function FactionManager::getFactionKeyByIndex(%this, %index)
{
   return getField(%this.factionKeys, %index);
}

function FactionManager::setEnemyFaction(%this, %factionKey)
{
   %key = strlwr(trim(%factionKey));
   if (%key !$= "undead" && %key !$= "dragonfolk" && %key !$= "gremlin")
   {
      echo("FactionManager::setEnemyFaction - Unknown faction key: " @ %factionKey @ ". Defaulting to 'undead'.");
      %key = "undead";
   }

   %this.selectedEnemyFaction = %key;
   $selectedEnemyFaction = %key;
   echo("FactionManager: Enemy faction set to " @ %this.getEnemyFactionName(%key) @ " (offset " @ %this.getEnemyFactionOffset(%key) @ ")");

   if (isObject($OptionsMenuGui))
      $OptionsMenuGui.updateVisuals();

   if (isObject($FactionSelectGui))
      $FactionSelectGui.updateVisuals();

   return %this.selectedEnemyFaction;
}

function FactionManager::getEnemyFaction(%this)
{
   if (%this.selectedEnemyFaction $= "")
      %this.selectedEnemyFaction = "undead";
   return %this.selectedEnemyFaction;
}

function FactionManager::getEnemyFactionOffset(%this, %factionKey)
{
   %key = (%factionKey !$= "") ? strlwr(trim(%factionKey)) : %this.getEnemyFaction();
   if (%this.offset[%key] !$= "")
      return %this.offset[%key];
   return 4;
}

function FactionManager::getEnemyFactionName(%this, %factionKey)
{
   %key = (%factionKey !$= "") ? strlwr(trim(%factionKey)) : %this.getEnemyFaction();
   if (%this.name[%key] !$= "")
      return %this.name[%key];
   return "Undead Legion";
}

function FactionManager::getEnemyFactionSubtitle(%this, %factionKey)
{
   %key = (%factionKey !$= "") ? strlwr(trim(%factionKey)) : %this.getEnemyFaction();
   return %this.subtitle[%key];
}

function FactionManager::getEnemyFactionDescription(%this, %factionKey)
{
   %key = (%factionKey !$= "") ? strlwr(trim(%factionKey)) : %this.getEnemyFaction();
   return %this.description[%key];
}

function FactionManager::getFactionUnitList(%this, %factionKey)
{
   %key = (%factionKey !$= "") ? strlwr(trim(%factionKey)) : %this.getEnemyFaction();
   return %this.units[%key];
}

// Global Singleton Initialization
if (!isObject($FactionManager))
{
   $FactionManager = new ScriptObject() { class = "FactionManager"; };
}
