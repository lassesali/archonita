# ==============================================================================
# Archonita - Build Release Distribution Script
# ==============================================================================
param(
    [string]$DestinationDir = "M:\Archonita\games\dist",
    [switch]$DeployToParentDist = $false
)

$ErrorActionPreference = "Stop"
$SourceRoot = "M:\Archonita\games"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " Building Archonita Release Distribution " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Target: $DestinationDir"

# 1. Clean / Recreate Destination
if (Test-Path $DestinationDir) {
    Write-Host "Cleaning existing dist directory..."
    Remove-Item -Path $DestinationDir -Recurse -Force
}
New-Item -ItemType Directory -Path $DestinationDir -Force | Out-Null

# 2. Copy Executables & DLLs
Write-Host "Copying engine binaries and libraries..."
Copy-Item "$SourceRoot\Torque2D.exe" "$DestinationDir\Archonita.exe"
Copy-Item "$SourceRoot\Torque2D.exe" "$DestinationDir\Torque2D.exe"
Copy-Item "$SourceRoot\OpenAL32.dll" "$DestinationDir\OpenAL32.dll"

# 3. Create Root main.cs (Release Direct Boot)
Write-Host "Creating release bootstrapper (main.cs)..."
$mainCsContent = @'
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
'@
Set-Content -Path "$DestinationDir\main.cs" -Value $mainCsContent -Encoding ASCII

# 4. Create Clean preferences.cs
Write-Host "Creating default preferences..."
$prefContent = @'
$pref::Video::appliedPref = 0;
$pref::Video::defaultResolution = "1024 768";
$pref::Video::disableVerticalSync = 0;
$pref::Video::displayDevice = "OpenGL";
$pref::Video::fullScreen = 0;
$pref::Video::preferOpenGL = 1;
$pref::Video::windowedRes = "1024 768 32";
'@
Set-Content -Path "$DestinationDir\preferences.cs" -Value $prefContent -Encoding ASCII

# 5. Create Convenience Batch Launcher
Write-Host "Creating Windows launcher..."
$launcherContent = @'
@echo off
cd /d "%~dp0"
start "" Archonita.exe
'@
Set-Content -Path "$DestinationDir\Play Archonita.bat" -Value $launcherContent -Encoding ASCII

# 6. Copy Archon Module (Filtered)
Write-Host "Copying Archon module..."
$archonSource = "$SourceRoot\Archon"
$archonDest = "$DestinationDir\Archon"
New-Item -ItemType Directory -Path $archonDest -Force | Out-Null

$excludeExtensions = @(".dso", ".bak", ".jbf")
$excludeNames = @("Thumbs.db", "1280", "800", "resolutionpatch_1280x1024.zip", "resolutionpatch_800x600.zip")

Get-ChildItem -Path $archonSource -Recurse | ForEach-Object {
    $item = $_
    $relPath = $item.FullName.Substring($archonSource.Length)
    
    # Check if inside excluded directory
    $skip = $false
    foreach ($ex in $excludeNames) {
        if ($relPath -match "\\$ex(\\|$)" -or $item.Name -eq $ex) {
            $skip = $true
            break
        }
    }
    
    if (-not $skip -and $excludeExtensions -contains $item.Extension) {
        $skip = $true
    }
    
    if (-not $skip) {
        $targetPath = Join-Path $archonDest $relPath
        if ($item.PSIsContainer) {
            if (-not (Test-Path $targetPath)) {
                New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
            }
        } else {
            $parent = Split-Path $targetPath -Parent
            if (-not (Test-Path $parent)) {
                New-Item -ItemType Directory -Path $parent -Force | Out-Null
            }
            Copy-Item -Path $item.FullName -Destination $targetPath -Force
        }
    }
}

# 7. Copy library/AppCore Module
Write-Host "Copying library/AppCore..."
$appCoreSource = "$SourceRoot\library\AppCore"
$appCoreDest = "$DestinationDir\library\AppCore"
New-Item -ItemType Directory -Path $appCoreDest -Force | Out-Null

Get-ChildItem -Path $appCoreSource -Recurse | ForEach-Object {
    $item = $_
    $relPath = $item.FullName.Substring($appCoreSource.Length)
    
    $skip = $false
    if ($excludeNames -contains $item.Name -or $excludeExtensions -contains $item.Extension) {
        $skip = $true
    }
    
    if (-not $skip) {
        $targetPath = Join-Path $appCoreDest $relPath
        if ($item.PSIsContainer) {
            if (-not (Test-Path $targetPath)) {
                New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
            }
        } else {
            $parent = Split-Path $targetPath -Parent
            if (-not (Test-Path $parent)) {
                New-Item -ItemType Directory -Path $parent -Force | Out-Null
            }
            Copy-Item -Path $item.FullName -Destination $targetPath -Force
        }
    }
}

# Ensure AppCore module.taml has Project="Archonita"
$appCoreTamlPath = "$appCoreDest\module.taml"
$tamlContent = Get-Content $appCoreTamlPath -Raw
$tamlContent = $tamlContent -replace 'Project="[^"]*"', 'Project="Archonita"'
$tamlContent = $tamlContent -replace 'ProjectDescription="[^"]*"', 'ProjectDescription="Archonita - 2D Turn-Based Tactical Board Game"'
Set-Content -Path $appCoreTamlPath -Value $tamlContent -Encoding UTF8

# 8. Copy library/Audio Module
Write-Host "Copying library/Audio..."
$audioSource = "$SourceRoot\library\Audio"
$audioDest = "$DestinationDir\library\Audio"
if (Test-Path $audioSource) {
    Copy-Item -Path $audioSource -Destination $audioDest -Recurse -Force
}

# 9. Copy library/themes Module
Write-Host "Copying library/themes..."
$libThemesSource = "$SourceRoot\library\themes"
$libThemesDest = "$DestinationDir\library\themes"
if (Test-Path $libThemesSource) {
    Copy-Item -Path $libThemesSource -Destination $libThemesDest -Recurse -Force
}

# 10. Copy root themes (Base.taml and cursors)
Write-Host "Setting up root themes and cursor art..."
$rootThemesDest = "$DestinationDir\themes"
New-Item -ItemType Directory -Path "$rootThemesDest\cursors\Base" -Force | Out-Null

if (Test-Path "$SourceRoot\library\themes\Base.taml") {
    Copy-Item "$SourceRoot\library\themes\Base.taml" "$rootThemesDest\Base.taml" -Force
} elseif (Test-Path "C:\Users\lkarh\Downloads\Torque2D-4.0-EA4-Windows-x64\Torque2D-4.0-EA4-Windows-x64\themes\Base.taml") {
    Copy-Item "C:\Users\lkarh\Downloads\Torque2D-4.0-EA4-Windows-x64\Torque2D-4.0-EA4-Windows-x64\themes\Base.taml" "$rootThemesDest\Base.taml" -Force
}

# Copy cursor art from AppCore stock cursors
$stockCursors = "$SourceRoot\library\AppCore\gui\images\cursors"
if (Test-Path $stockCursors) {
    Get-ChildItem -Path $stockCursors -Filter "*.png" | ForEach-Object {
        Copy-Item $_.FullName "$rootThemesDest\cursors\Base\$($_.Name)" -Force
    }
}

# 11. Create Release README.txt
Write-Host "Creating release README.txt..."
$readmeContent = @'
================================================================================
                           ARCHONITA (2006 - 2026)
                        20th Anniversary Edition
                 Ported to 64-Bit Torque2D 4.0 (EA4)
================================================================================

ABOUT THE GAME
--------------
Archonita is a tactical turn-based grid strategy game originally developed by
Gamelon Productions for the Suomipelit.com Pikkukesäkilpailu 2006 game jam.
Blending the strategic depth of classic board games (such as Archon) with real-time
dice rolls, hero abilities, and spellcasting, Archonita challenges you to deploy
your fantasy army and dominate the tactical battlefield.

This modern release ports the original 2006 32-bit Torque Game Builder (TGB)
codebase to a native 64-bit Torque2D 4.0 runtime with modern OpenGL rendering,
OpenAL audio, and widescreen/high-resolution display support.

HOW TO PLAY
-----------
1. Double-click "Archonita.exe" or run "Play Archonita.bat".
2. Select your Hero (Warrior or Wizard).
3. Purchase and recruit your fantasy units using Creature Points.
4. Deploy units to legal green tiles on your starting rows.
5. Move across the 4x4 tactical grid, engage enemy units in dynamic dice combat,
   and conquer the realm!

SYSTEM REQUIREMENTS
-------------------
- Operating System: Windows 10 / 11 (64-bit)
- Graphics: OpenGL 3.3+ compatible GPU
- Audio: OpenAL compatible sound card / device
- Storage: ~60 MB free disk space

CREDITS & ACKNOWLEDGEMENTS
--------------------------
- Original Game: Gamelon Productions (2006)
- Engine: Torque2D 4.0 ("Rocket Edition" Early Access 4) by GarageGames / Torque2D Community
- 2026 Engine Migration & Modernization Update

Enjoy the game!
================================================================================
'@
Set-Content -Path "$DestinationDir\README.txt" -Value $readmeContent -Encoding UTF8

# 12. Optional deployment to parent dist folder
if ($DeployToParentDist) {
    $parentDist = "M:\Archonita\dist"
    Write-Host "Deploying release to parent folder: $parentDist"
    
    # Backup old 2006 dist if present and not already backed up
    $parentBackup = "M:\Archonita\dist_2006_backup"
    if ((Test-Path $parentDist) -and (-not (Test-Path $parentBackup))) {
        Write-Host "Backing up legacy 2006 distribution to $parentBackup..."
        Move-Item -Path $parentDist -Destination $parentBackup -Force
    } elseif (Test-Path $parentDist) {
        Remove-Item -Path $parentDist -Recurse -Force
    }
    
    New-Item -ItemType Directory -Path $parentDist -Force | Out-Null
    Copy-Item -Path "$DestinationDir\*" -Destination $parentDist -Recurse -Force
    Write-Host "Parent dist deployed cleanly at: $parentDist"
}

Write-Host "==========================================" -ForegroundColor Green
Write-Host " Build Complete! " -ForegroundColor Green
Write-Host " Release files located at: $DestinationDir" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
