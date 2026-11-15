@echo off
set "T2D_DIR=%~dp0"
cd /d "%T2D_DIR%"
if exist _boot.cs del _boot.cs
start "" "%T2D_DIR%Torque2D.exe"
