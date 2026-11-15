@echo off
set "T2D_DIR=%~dp0"
cd /d "%T2D_DIR%"
(
echo setLogMode^(2^);
echo ModuleDatabase.scanModules^("library"^);
echo ModuleDatabase.scanModules^("Archon"^);
echo ModuleDatabase.loadExplicit^("AppCore"^);
) > "_boot.cs"
start "" "%T2D_DIR%Torque2D.exe" _boot.cs
