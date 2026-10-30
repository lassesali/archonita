@echo off
set "T2D_DIR=C:\Users\lkarh\Downloads\Torque2D-4.0-EA4-Windows-x64\Torque2D-4.0-EA4-Windows-x64"
cd /d "%T2D_DIR%"
copy /y play_archon.cs _boot.cs >nul
start "" Torque2D.exe
