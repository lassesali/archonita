<#
.SYNOPSIS
    Builds the unified 512x512 particle texture atlas and ImageAsset TAML for Archonita.
#>
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing

$RepoRoot = Split-Path -Parent $PSScriptRoot
$ImagesDir = Join-Path $RepoRoot 'Archon\data\images'
$AtlasPngPath = Join-Path $ImagesDir 'particlesAtlas.png'
$AtlasTamlPath = Join-Path $ImagesDir 'particlesAtlasImageMap.asset.taml'

$Regions = @(
    @{ Name = 'p1'; File = 'particles1.png'; DestX = 0;   DestY = 256; Width = 128; Height = 128; CellW = 64;  CellH = 64 }
    @{ Name = 'p2'; File = 'particles2.png'; DestX = 0;   DestY = 0;   Width = 256; Height = 256; CellW = 128; CellH = 128 }
    @{ Name = 'p3'; File = 'particles3.png'; DestX = 256; DestY = 0;   Width = 256; Height = 256; CellW = 128; CellH = 128 }
    @{ Name = 'p4'; File = 'particles4.png'; DestX = 128; DestY = 256; Width = 128; Height = 128; CellW = 64;  CellH = 64 }
    @{ Name = 'p5'; File = 'particles5.png'; DestX = 256; DestY = 256; Width = 128; Height = 128; CellW = 64;  CellH = 64 }
    @{ Name = 'p6'; File = 'particles6.png'; DestX = 384; DestY = 256; Width = 128; Height = 128; CellW = 64;  CellH = 64 }
    @{ Name = 'p7'; File = 'particles7.png'; DestX = 0;   DestY = 384; Width = 128; Height = 128; CellW = 64;  CellH = 64 }
    @{ Name = 'p8'; File = 'particles8.png'; DestX = 128; DestY = 384; Width = 128; Height = 128; CellW = 64;  CellH = 64 }
    @{ Name = 'p9'; File = 'particles9.png'; DestX = 256; DestY = 384; Width = 128; Height = 128; CellW = 64;  CellH = 64 }
)

$AtlasBmp = New-Object System.Drawing.Bitmap(512, 512, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$Graphics = [System.Drawing.Graphics]::FromImage($AtlasBmp)

try {
    $Graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
    $Graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $Graphics.Clear([System.Drawing.Color]::FromArgb(0, 0, 0, 0))

    # Downsample blood.png (256x256 -> 128x128) cleanly
    $bloodPath = Join-Path $ImagesDir 'blood.png'
    if (-not (Test-Path $bloodPath)) {
        throw "Missing source blood texture: $bloodPath"
    }

    $bloodBmp = [System.Drawing.Bitmap]::FromFile($bloodPath)
    $imgAttr = New-Object System.Drawing.Imaging.ImageAttributes
    try {
        $imgAttr.SetWrapMode([System.Drawing.Drawing2D.WrapMode]::TileFlipXY)
        $Graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $Graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
        $bloodDest = New-Object System.Drawing.Rectangle(384, 384, 128, 128)
        $Graphics.DrawImage($bloodBmp, $bloodDest, 0, 0, $bloodBmp.Width, $bloodBmp.Height, [System.Drawing.GraphicsUnit]::Pixel, $imgAttr)
    }
    finally {
        $imgAttr.Dispose()
        $bloodBmp.Dispose()
    }
}
finally {
    $Graphics.Dispose()
}

# Bit-exact lossless 1:1 pixel copy for particles1..particles9 (avoids GDI+ premultiplied alpha rounding)
$AtlasRect = New-Object System.Drawing.Rectangle(0, 0, 512, 512)
$DstData = $AtlasBmp.LockBits($AtlasRect, [System.Drawing.Imaging.ImageLockMode]::ReadWrite, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
try {
    foreach ($r in $Regions) {
        $srcPath = Join-Path $ImagesDir $r.File
        if (-not (Test-Path $srcPath)) {
            throw "Missing source particle texture: $srcPath"
        }

        $srcBmp = [System.Drawing.Bitmap]::FromFile($srcPath)
        try {
            if ($srcBmp.Width -ne $r.Width -or $srcBmp.Height -ne $r.Height) {
                throw "$($r.File) dimensions ($($srcBmp.Width)x$($srcBmp.Height)) do not match expected ($($r.Width)x$($r.Height))"
            }
            $srcRect = New-Object System.Drawing.Rectangle(0, 0, $r.Width, $r.Height)
            $srcData = $srcBmp.LockBits($srcRect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
            try {
                $rowBytes = $r.Width * 4
                $rowBuf = New-Object byte[] $rowBytes
                for ($y = 0; $y -lt $r.Height; $y++) {
                    $srcPtr = [IntPtr]($srcData.Scan0.ToInt64() + ($y * $srcData.Stride))
                    $dstPtr = [IntPtr]($DstData.Scan0.ToInt64() + (($r.DestY + $y) * $DstData.Stride) + ($r.DestX * 4))
                    [System.Runtime.InteropServices.Marshal]::Copy($srcPtr, $rowBuf, 0, $rowBytes)
                    [System.Runtime.InteropServices.Marshal]::Copy($rowBuf, 0, $dstPtr, $rowBytes)
                }
            }
            finally {
                $srcBmp.UnlockBits($srcData)
            }
        }
        finally {
            $srcBmp.Dispose()
        }
    }
}
finally {
    $AtlasBmp.UnlockBits($DstData)
}

$AtlasBmp.Save($AtlasPngPath, [System.Drawing.Imaging.ImageFormat]::Png)
$AtlasBmp.Dispose()

# Generate particlesAtlasImageMap.asset.taml with ExplicitMode="1" and 37 cells in exact index order
$CellLines = New-Object System.Collections.Generic.List[string]
foreach ($r in $Regions) {
    $frameIdx = 0
    for ($cy = 0; $cy -lt 2; $cy++) {
        for ($cx = 0; $cx -lt 2; $cx++) {
            $ox = $r.DestX + ($cx * $r.CellW)
            $oy = $r.DestY + ($cy * $r.CellH)
            $regionName = "$($r.Name)_$frameIdx"
            $CellLines.Add("        <Cell RegionName=`"$regionName`" Offset=`"$ox $oy`" Width=`"$($r.CellW)`" Height=`"$($r.CellH)`" />")
            $frameIdx++
        }
    }
}
$CellLines.Add("        <Cell RegionName=`"blood_0`" Offset=`"384 384`" Width=`"128`" Height=`"128`" />")

$TamlLines = @(
    '<ImageAsset'
    '    AssetName="particlesAtlasImageMap"'
    '    ImageFile="particlesAtlas.png"'
    '    ExplicitMode="1">'
    '    <ImageAsset.Cells>'
) + $CellLines + @(
    '    </ImageAsset.Cells>'
    '</ImageAsset>'
)

[System.IO.File]::WriteAllText($AtlasTamlPath, ($TamlLines -join "`r`n") + "`r`n", (New-Object System.Text.UTF8Encoding($false)))

# Built-in verification against the saved PNG on disk
$VerifyAtlas = [System.Drawing.Bitmap]::FromFile($AtlasPngPath)
try {
    if ($VerifyAtlas.Width -ne 512 -or $VerifyAtlas.Height -ne 512) {
        throw "Atlas dimensions mismatch: $($VerifyAtlas.Width)x$($VerifyAtlas.Height)"
    }

    $mismatches = 0
    foreach ($r in $Regions) {
        $srcPath = Join-Path $ImagesDir $r.File
        $srcBmp = [System.Drawing.Bitmap]::FromFile($srcPath)
        try {
            for ($y = 0; $y -lt $r.Height; $y++) {
                for ($x = 0; $x -lt $r.Width; $x++) {
                    $sp = $srcBmp.GetPixel($x, $y)
                    $ap = $VerifyAtlas.GetPixel($r.DestX + $x, $r.DestY + $y)
                    if ($sp.ToArgb() -ne $ap.ToArgb()) {
                        $mismatches++
                        if ($mismatches -le 5) {
                            Write-Warning "Pixel mismatch in $($r.File) at ($x,$y): src=$($sp.ToArgb()) atlas=$($ap.ToArgb())"
                        }
                    }
                }
            }
        }
        finally {
            $srcBmp.Dispose()
        }
    }

    if ($mismatches -gt 0) {
        throw "Verification failed: $mismatches mismatched pixels between source textures and particlesAtlas.png"
    }

    $bloodNonTransparent = 0
    for ($y = 384; $y -lt 512; $y++) {
        for ($x = 384; $x -lt 512; $x++) {
            if ($VerifyAtlas.GetPixel($x, $y).A -gt 0) {
                $bloodNonTransparent++
            }
        }
    }
    if ($bloodNonTransparent -eq 0) {
        throw "Verification failed: blood_0 region has no non-transparent pixels"
    }

    Write-Output "Atlas generated and verified: 512x512, 37 cells, 0 mismatched pixels, blood_0 non-transparent pixels=$bloodNonTransparent"
}
finally {
    $VerifyAtlas.Dispose()
}
