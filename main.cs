//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

//---------------------------------------------------------------------------------------------
// Set the mods to run.
// $runWithEditors specifies whether or not the tools mod should be loaded.
// $defaultGame is the main game that will be run at startup.
// Additional mods can be specified with the -mod <mod_name> command line switch.
//---------------------------------------------------------------------------------------------
$runWithEditors = true;
$defaultGame = "TGB";
$startupProject = "";
$defaultMods = "";

//---------------------------------------------------------------------------------------------
// Enable profiling.
//---------------------------------------------------------------------------------------------
//profilerEnable(true);

//---------------------------------------------------------------------------------------------
// displayHelp, parseArgs, onStart, and onExit should be redefined by mod packages that need to
// display help information, handle command line arguments, or hook into the startup and
// shutdown of the engine.
//---------------------------------------------------------------------------------------------
function onStart()
{
}

function onExit()
{
}

function parseArgs()
{
   for (%i = 1; %i < $Game::argc; %i++)
   {
      // Grab the argument.
      %arg = $Game::argv[%i];
      // Grab the next argument for those args that require parameters.
      %nextArg = $Game::argv[%i + 1];
      // Check if there is another argument.
      %hasNextArg = $Game::argc - %i > 1;
      
      // Handle the argument.
      switch$ (%arg)
      {
         // Set the console logging mode.
         case "-log":
            $argUsed[%i]++;
            if (%hasNextArg)
            {
               if (%nextArg != 0)
                  %nextArg += 4;
                  
               setLogMode(%nextArg);
               $logModeSpecified = true;
               $argUsed[%i + 1]++;
               %i++;
            }
            else
               error("Error: Missing Command Line argument. Usage: -log <Mode: 0,1,2>");
         
         
         // Add an additional mod.
         case "-mod":
            // Handled by the setupMods function.
      
            
         // Change the default game.
         case "-game":
            $argUsed[%i]++;
            if (%hasNextArg)
            {
               $startupProject = %nextArg;
               $argUsed[%i + 1]++;
               %i++;
            }
            else
               error("Error: Missing Command Line argument. Usage: -game <game_name>");
         
         
         // Start up with a console window.
         case "-console":
            enableWinConsole(true);
            $argUsed[%i]++;
            
         // Save a journal of this run of the engine.
         case "-jSave":
            $argUsed[%i]++;
            if (%hasNextArg)
            {
               echo("Saving event log to journal: " @ %nextArg);
               saveJournal(%nextArg);
               $argUsed[%i + 1]++;
               %i++;
            }
            else
               error("Error: Missing Command Line argument. Usage: -jSave <journal_name>");
      
            
         // Play back a previously recorded journal.
         case "-jPlay":
            $argUsed[%i]++;
            if (%hasNextArg)
            {
               playJournal(%nextArg, false);
               $argUsed[%i+1]++;
               %i++;
            }
            else
               error("Error: Missing Command Line argument. Usage: -jPlay <journal_name>");
      
            
         // Play back a previously recorded journal in debug mode.
         case "-jDebug":
            $argUsed[%i]++;
            if (%hasNextArg)
            {
               playJournal(%nextArg, true);
               $argUsed[%i + 1]++;
               %i++;
            }
            else
               error("Error: Missing Command Line argument. Usage: -jDebug <journal_name>");
         
         
         // Display the command line help.
         case "-help":
            $displayHelp = true;
            $argUsed[%i]++;
      }
   }
}   

package Help
{
   function onExit()
   {
      // When the help is being displayed, the game never started up, so with this we make sure
      // no superfluous shutdown and cleanup takes place.
   }
};

function displayHelp()
{
   // Activate the Help Package.
   activatePackage(Help);
   
   error(
   "TGB command line options:\n"@
   "  -log <logmode>         Logging behavior; see main.cs comments for details\n"@
   "  -game <game_name>      Reset list of mods to only contain <game_name>\n"@
   "  -mod <mod_name>        Add <mod_name> to list of mods\n"@
   "  -console               Open a separate console\n"@
   "  -jSave  <file_name>    Record a journal\n"@
   "  -jPlay  <file_name>    Play back a journal\n"@
   "  -jDebug <file_name>    Play back a journal and issue an int3 at the end\n"@
   "  -help                  Display this help message\n"
   );
}

//---------------------------------------------------------------------------------------------
// setupMods
// Sets the mod directories based on the default game, command line parameters, and whether or
// not the tools should be loaded.
//---------------------------------------------------------------------------------------------
function setupMods()
{
   // Search for command line switches that would change the mod paths.
   for (%i = 1; %i < $Game::argc; %i++)
   {
      // Look for additional mods.
      if ($Game::argv[%i] $= "-mod")
      {
         $argUsed[%i]++;
         if (($Game::argc - %i) > 1)
         {
            $defaultMods = $defaultMods SPC $Game::argv[%i + 1];
            $argUsed[%i + 1]++;
            %i++;
         }
         else
            error("Error: Missing Command Line argument. Usage: -mod <mod_name>");
      }
      
      if ($game::argv[%i] $= "-notools")
      {
         $argUsed[%i]++;
         $runWithEditors = false;
      }
   }
   
   // Set the default mods.
   %mods = "common";
   
   // Add editors if it is so desired.
   if ($runWithEditors)
      %mods = %mods SPC "tools";
   
   // Add default and command line mods.
   if ($defaultMods !$= "")
      %mods = %mods SPC trim($defaultMods);
      
   loadMods(%mods);
}

//---------------------------------------------------------------------------------------------
// loadMods
// Execute startup scripts for each mod.
//---------------------------------------------------------------------------------------------
function loadMods(%modPath)
{
   %count = getWordCount(%modPath);
   if (%count == 0)
   {
      if ($defaultGame $= "")
      {
         $displayHelp = true;
         return;
      }
      else
      {
         %modPath = $defaultGame;
         %count = 1;
      }
   }
   
   setModPaths(strreplace(%modPath, " ", ";"));
   
   for (%i = 0; %i < %count; %i++)
   {
      %mod = getWord(%modPath, %i);
      if (!exec(%mod @ "/main.cs"))
         error("Error: Unable to find specified mod: " @ %mod);
   }
}

// Defaults that command line arguments may overwrite.
$displayHelp = false;
$logModeSpecified = false;

//---------------------------------------------------------------------------------------------
// Set the mod directories.
//---------------------------------------------------------------------------------------------
setupMods();

//---------------------------------------------------------------------------------------------
// Parse the command line arguments.
//---------------------------------------------------------------------------------------------
echo("--------- Parsing Arguments ---------");
parseArgs();

// Display an error message for unused arguments.
for ($i = 1; $i < $Game::argc; $i++)
{
   if (!$argUsed[$i])
      error("Error: Unknown command line argument: " @ $Game::argv[$i]);
}

if (!$logModeSpecified)
   setLogMode(6);

//---------------------------------------------------------------------------------------------
// Either display the help message or startup the application.
//---------------------------------------------------------------------------------------------
if ($displayHelp)
{
   enableWinConsole(true);
   displayHelp();
   quit();
}
else
{
   onStart();
   echo("\nTorque Game Builder (" @ getT2DVersion() @ ") initialized...");
   loadStartupProject();
}


