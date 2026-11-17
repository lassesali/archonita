//---------------------------------------------------------------------------------------------
// Archonita - Android Boot Entry Script
// Copied to src/main/game/main.cs by Gradle's copyGame task (desktop main.cs stays untouched).
//---------------------------------------------------------------------------------------------

setRandomSeed();
setLogMode(2);
$Scripts::ignoreDSOs = true;
setCompanyAndProduct("Torque Game Engines", "Archonita");

ModuleDatabase.scanModules("./library");
ModuleDatabase.scanModules("./Archon");
ModuleDatabase.loadExplicit("AppCore");

function onExit()
{
    if (isObject(ModuleDatabase))
        ModuleDatabase.unloadExplicit("AppCore");
}

function androidBackButton(%val)
{
    // Act once on release (val == false) to mirror desktop Escape navigation.
    if (!%val)
    {
        if (isFunction("onEscapeKeyPressed"))
            onEscapeKeyPressed();
        else
            quit();
    }
}
