//-----------------------------------------------------------------------------
// tests/test_factions.cs
// Verification of FactionManager, UnitProperties, CombatEngine, and FactionSelectGui
//-----------------------------------------------------------------------------

$Archon::skipSplash = true;

%testFile = new FileObject();
%testFile.openForWrite("tests/faction_test_result.txt");
%testFile.writeLine("TEST_SUITE_START");

function logResult(%msg)
{
   echo(%msg);
   if (isObject($testFileObj))
      $testFileObj.writeLine(%msg);
}

$testFileObj = %testFile;

function failTest(%msg)
{
   logResult("TEST_FAIL: " @ %msg);
   if (isObject($testFileObj))
   {
      $testFileObj.close();
      $testFileObj.delete();
   }
   quitWithCode(1);
}

function passTest(%msg)
{
   logResult("TEST_PASS: " @ %msg);
}

setCompanyAndProduct("Torque Game Engines", "Archonita");
ModuleDatabase.EchoInfo = false;
AssetDatabase.EchoInfo = false;

ModuleDatabase.scanModules("./library");
ModuleDatabase.scanModules("./Archon");
ModuleDatabase.LoadExplicit("AppCore");

logResult("==========================================");
logResult("Archonita - Factions Verification Suite");
logResult("==========================================");

// 1. Verify FactionManager singleton
if (!isObject($FactionManager))
   failTest("FactionManager singleton not created");
passTest("FactionManager singleton exists");

// 2. Verify Faction Keys and Counts
if ($FactionManager.getFactionCount() != 3)
   failTest("Expected 3 factions, found " @ $FactionManager.getFactionCount());
passTest("FactionManager reports 3 factions");

// 3. Verify Default Faction is Undead
if ($FactionManager.getEnemyFaction() !$= "undead")
   failTest("Expected default faction 'undead', got '" @ $FactionManager.getEnemyFaction() @ "'");
if ($FactionManager.getEnemyFactionOffset() != 4)
   failTest("Expected default offset 4, got " @ $FactionManager.getEnemyFactionOffset());
passTest("Default enemy faction is undead (offset 4)");

// 4. Test Setting Dragonfolk Faction
$FactionManager.setEnemyFaction("dragonfolk");
if ($FactionManager.getEnemyFaction() !$= "dragonfolk")
   failTest("Failed setting faction to dragonfolk");
if ($FactionManager.getEnemyFactionOffset() != 8)
   failTest("Expected dragonfolk offset 8, got " @ $FactionManager.getEnemyFactionOffset());
if ($FactionManager.getEnemyFactionName() !$= "Dragonfolk Faction")
   failTest("Expected Dragonfolk Faction name, got " @ $FactionManager.getEnemyFactionName());
passTest("Dragonfolk faction verified (offset 8)");

// 5. Test Setting Gremlin Faction
$FactionManager.setEnemyFaction("gremlin");
if ($FactionManager.getEnemyFaction() !$= "gremlin")
   failTest("Failed setting faction to gremlin");
if ($FactionManager.getEnemyFactionOffset() != 12)
   failTest("Expected gremlin offset 12, got " @ $FactionManager.getEnemyFactionOffset());
if ($FactionManager.getEnemyFactionName() !$= "Gremlin Faction")
   failTest("Expected Gremlin Faction name, got " @ $FactionManager.getEnemyFactionName());
passTest("Gremlin faction verified (offset 12)");

// 6. Test Invalid Faction Key Fallback
$FactionManager.setEnemyFaction("invalid_faction_xyz");
if ($FactionManager.getEnemyFaction() !$= "undead")
   failTest("Expected fallback to undead for invalid faction");
passTest("Invalid faction correctly falls back to undead");

// 7. Verify UnitProperties for all 16 units
for (%i = 0; %i < 16; %i++)
{
   if (UnitProperties.hp[%i] $= "" || UnitProperties.hp[%i] < 1)
      failTest("Invalid HP for unit " @ %i);
   if (UnitProperties.attack[%i] $= "" || UnitProperties.attack[%i] < 1)
      failTest("Invalid attack for unit " @ %i);
   if (UnitProperties.defence[%i] $= "" || UnitProperties.defence[%i] < 1)
      failTest("Invalid defence for unit " @ %i);
   if (UnitProperties.lvl[%i] $= "" || UnitProperties.lvl[%i] < 1 || UnitProperties.lvl[%i] > 4)
      failTest("Invalid lvl for unit " @ %i);
   if (UnitProperties.groupid[%i] $= "")
      failTest("Invalid groupid for unit " @ %i);
}
passTest("All 16 unit properties validated");

// Verify Dragonfolk and Gremlin specific stats
// Dragonfolk (8): Lvl 1, Melee, Group 1
if (UnitProperties.lvl[8] != 1 || UnitProperties.groupid[8] != 1 || UnitProperties.missile[8] != 0)
   failTest("Dragon Folk unit stats mismatch");
// Red Dragon (9): Lvl 2, Missile 1
if (UnitProperties.lvl[9] != 2 || UnitProperties.missile[9] != 1)
   failTest("Red Dragon unit stats mismatch");
// Air Dragon (10): Lvl 3, Mana 1, Spells 1
if (UnitProperties.lvl[10] != 3 || UnitProperties.mana[10] != 1)
   failTest("Air Dragon unit stats mismatch");
// Gold Dragon (11): Lvl 4, Mana 1, Spells 2
if (UnitProperties.lvl[11] != 4 || UnitProperties.hp[11] != 4)
   failTest("Gold Dragon unit stats mismatch");

// Gremlin (12): Lvl 1, Melee
if (UnitProperties.lvl[12] != 1 || UnitProperties.groupid[12] != 1 || UnitProperties.missile[12] != 0)
   failTest("Gremlin unit stats mismatch");
// Horned One-Eye (13): Lvl 2, Missile 1
if (UnitProperties.lvl[13] != 2 || UnitProperties.missile[13] != 1)
   failTest("Horned One-Eye unit stats mismatch");
// Mistress (14): Lvl 3, Mana 1
if (UnitProperties.lvl[14] != 3 || UnitProperties.mana[14] != 1)
   failTest("Mistress unit stats mismatch");
// Demon (15): Lvl 4, Mana 1, Spells 2
if (UnitProperties.lvl[15] != 4 || UnitProperties.hp[15] != 4)
   failTest("Demon unit stats mismatch");
passTest("Faction-specific creature stats verified");

// 8. Verify CombatEngine Animation States (0..15)
CombatEngine.initAnimStates();
if (getWordCount($unitNames) != 16)
   failTest("Expected 16 unit names in $unitNames, found " @ getWordCount($unitNames));

for (%i = 0; %i < 16; %i++)
{
   if (!isObject($olentoStandState[%i]))
      failTest("Missing $olentoStandState for index " @ %i);
   if (!isObject($olentoWalkState[%i]))
      failTest("Missing $olentoWalkState for index " @ %i);
   if (!isObject($olentoBeenHitState[%i]))
      failTest("Missing $olentoBeenHitState for index " @ %i);
   if (!isObject($olentoAttackState[%i]))
      failTest("Missing $olentoAttackState for index " @ %i);
   if (!isObject($olentoDieState[%i]))
      failTest("Missing $olentoDieState for index " @ %i);
}
passTest("CombatEngine all 16 creature animation states verified");

// 9. Verify FactionSelectGui
if (!isObject($FactionSelectGui))
   failTest("FactionSelectGui singleton missing");
$FactionSelectGui.show(true);
if (!$FactionSelectGui.isVisible())
   failTest("FactionSelectGui failed to become visible");
$FactionSelectGui.onSelectFaction("dragonfolk");
if ($FactionManager.getEnemyFaction() !$= "dragonfolk")
   failTest("FactionSelectGui failed to select dragonfolk");
$FactionSelectGui.show(false);
if ($FactionSelectGui.isVisible())
   failTest("FactionSelectGui failed to hide");
passTest("FactionSelectGui show/select/hide lifecycle verified");

// 10. Verify OptionsMenuGui faction buttons
if (isObject($OptionsMenuGui))
{
   $OptionsMenuGui.onSelectFaction("gremlin");
   if ($FactionManager.getEnemyFaction() !$= "gremlin")
      failTest("OptionsMenuGui failed to select gremlin");
   $OptionsMenuGui.updateVisuals();
   passTest("OptionsMenuGui faction selector verified");
}

logResult("==========================================");
logResult("ALL FACTION VERIFICATION TESTS PASSED!");
logResult("==========================================");

if (isObject($testFileObj))
{
   $testFileObj.close();
   $testFileObj.delete();
}

quitWithCode(0);
