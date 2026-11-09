# tests/lib/DpiAwareInput.ps1
# High-DPI Scaling aware input and QA automation harness for Windows 11
# Dot-sources tests/lib/input.ps1 and adds DPI normalization, overflow recovery, and bounds checking.

. "$PSScriptRoot\input.ps1"

Add-Type @"
using System;
using System.Runtime.InteropServices;

public class WinDpiHelper {
    [DllImport("user32.dll")]
    public static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

    [DllImport("user32.dll")]
    public static extern uint GetDpiForSystem();

    [DllImport("user32.dll")]
    public static extern uint GetDpiForWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowDpiAwarenessContext(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int GetAwarenessFromDpiAwarenessContext(IntPtr ctx);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
"@ -ErrorAction SilentlyContinue

# Ensure PowerShell runner is Per-Monitor V2 aware
[WinDpiHelper]::SetProcessDpiAwarenessContext([IntPtr](-4)) | Out-Null

function Get-DisplayDpiMetrics {
    $dpi = [WinDpiHelper]::GetDpiForSystem()
    if ($dpi -eq 0) { $dpi = 96 }
    $scale = [math]::Round($dpi / 96.0, 4)
    $w = [WinDpiHelper]::GetSystemMetrics(0) # SM_CXSCREEN
    $h = [WinDpiHelper]::GetSystemMetrics(1) # SM_CYSCREEN

    return [PSCustomObject]@{
        Dpi         = $dpi
        ScaleFactor = $scale
        ScreenWidth = $w
        ScreenHeight= $h
    }
}

function Get-WindowDpiMetadata {
    param([IntPtr]$Hwnd)

    $dpi = [WinDpiHelper]::GetDpiForWindow($Hwnd)
    if ($dpi -eq 0) { $dpi = [WinDpiHelper]::GetDpiForSystem() }
    $scale = [math]::Round($dpi / 96.0, 4)

    $ctx = [WinDpiHelper]::GetWindowDpiAwarenessContext($Hwnd)
    $aw = [WinDpiHelper]::GetAwarenessFromDpiAwarenessContext($ctx)

    $rect = New-Object WinDpiHelper+RECT
    [WinDpiHelper]::GetWindowRect($Hwnd, [ref]$rect) | Out-Null

    $rawBounds = @($rect.Left, $rect.Top, $rect.Right, $rect.Bottom)
    $adjBounds = @(
        [math]::Round($rect.Left / $scale, 2),
        [math]::Round($rect.Top / $scale, 2),
        [math]::Round($rect.Right / $scale, 2),
        [math]::Round($rect.Bottom / $scale, 2)
    )

    return [PSCustomObject]@{
        Hwnd           = $Hwnd
        Dpi            = $dpi
        ScaleFactor    = $scale
        AwarenessId    = $aw
        RawBounds      = $rawBounds
        AdjustedBounds = $adjBounds
    }
}

function Send-EngineNormalizedClick {
    param([IntPtr]$Hwnd, [double]$RawX, [double]$RawY, [double]$Scale = 0)

    if ($Scale -le 0) {
        $meta = Get-WindowDpiMetadata -Hwnd $Hwnd
        $Scale = $meta.ScaleFactor
    }

    # Coordinate Normalization Formula: X_adj = X_raw / S, Y_adj = Y_raw / S
    $adjX = [int][math]::Round($RawX / $Scale)
    $adjY = [int][math]::Round($RawY / $Scale)

    Send-EngineClick -Hwnd $Hwnd -X $adjX -Y $adjY
}

function Send-EngineNormalizedDrag {
    param([IntPtr]$Hwnd, [double]$FromX, [double]$FromY, [double]$ToX, [double]$ToY, [double]$Scale = 0, [int]$Steps = 8)

    if ($Scale -le 0) {
        $meta = Get-WindowDpiMetadata -Hwnd $Hwnd
        $Scale = $meta.ScaleFactor
    }

    $adjFromX = [int][math]::Round($FromX / $Scale)
    $adjFromY = [int][math]::Round($FromY / $Scale)
    $adjToX   = [int][math]::Round($ToX / $Scale)
    $adjToY   = [int][math]::Round($ToY / $Scale)

    Send-EngineDrag -Hwnd $Hwnd -FromX $adjFromX -FromY $adjFromY -ToX $adjToX -ToY $adjToY -Steps $Steps
}

function Test-WindowOverflow {
    param([IntPtr]$Hwnd)

    $disp = Get-DisplayDpiMetrics
    $rect = New-Object WinDpiHelper+RECT
    [WinDpiHelper]::GetWindowRect($Hwnd, [ref]$rect) | Out-Null

    $w = $rect.Right - $rect.Left
    $h = $rect.Bottom - $rect.Top

    $clipL = [math]::Max(0, -$rect.Left)
    $clipT = [math]::Max(0, -$rect.Top)
    $clipR = [math]::Max(0, $rect.Right - $disp.ScreenWidth)
    $clipB = [math]::Max(0, $rect.Bottom - $disp.ScreenHeight)

    $isOverflowing = ($clipL -gt 0 -or $clipT -gt 0 -or $clipR -gt 0 -or $clipB -gt 0)

    $targetX = [math]::Max(0, [int](($disp.ScreenWidth - $w) / 2))
    $targetY = [math]::Max(0, [int](($disp.ScreenHeight - $h) / 2))

    $deltaX = $targetX - $rect.Left
    $deltaY = $targetY - $rect.Top

    return [PSCustomObject]@{
        IsOverflowing = $isOverflowing
        ClipLeft      = $clipL
        ClipTop       = $clipT
        ClipRight     = $clipR
        ClipBottom    = $clipB
        DeltaX        = $deltaX
        DeltaY        = $deltaY
    }
}

function Recenter-EngineWindow {
    param([IntPtr]$Hwnd)

    $overflow = Test-WindowOverflow -Hwnd $Hwnd
    if (-not $overflow.IsOverflowing) { return $true }

    $rect = New-Object WinDpiHelper+RECT
    [WinDpiHelper]::GetWindowRect($Hwnd, [ref]$rect) | Out-Null
    $newX = $rect.Left + $overflow.DeltaX
    $newY = $rect.Top + $overflow.DeltaY

    # SWP_NOSIZE (0x0001) | SWP_NOZORDER (0x0004) | SWP_NOACTIVATE (0x0010)
    $flags = 0x0001 -bor 0x0004 -bor 0x0010
    $res = [WinDpiHelper]::SetWindowPos($Hwnd, [IntPtr]::Zero, $newX, $newY, 0, 0, [uint32]$flags)
    return $res
}
