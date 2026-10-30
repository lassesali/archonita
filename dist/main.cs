//-----------------------------------------------------------------------------
// Archonita - 2D Turn-Based Tactical Board Game
// Torque2D 4.0 Release Build
//-----------------------------------------------------------------------------

// Set random seed
setRandomSeed();

// Log mode: 2 = file output only (console.log)
setLogMode(2);

// Silence execution echo and tracing for optimal release performance
setScriptExecEcho(false);
trace(false);

// Ignore compiled DSOs to prevent stale bytecode execution
$Scripts::ignoreDSOs = true;

// Identity
setCompanyAndProduct("Torque Game Engines", "Archonita");

// Module & Asset Database configuration
ModuleDatabase.EchoInfo = false;
AssetDatabase.EchoInfo = false;
AssetDatabase.IgnoreAutoUnload = true;

// Scan modules
ModuleDatabase.scanModules("./library");
ModuleDatabase.scanModules("./Archon");

// Boot AppCore (initializes Canvas, cursors, theme, then loads group "launch")
ModuleDatabase.loadExplicit("AppCore");

// Shutdown handler
function onExit()
{
    if (isObject(AppCore))
    {
        ModuleDatabase.unloadExplicit("AppCore");
    }
}
