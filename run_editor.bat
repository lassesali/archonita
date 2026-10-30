@echo off
set "T2D_DIR=C:\Users\lkarh\Downloads\Torque2D-4.0-EA4-Windows-x64\Torque2D-4.0-EA4-Windows-x64"
cd /d "%T2D_DIR%"
if exist _boot.cs del _boot.cs
start "" Torque2D.exe
