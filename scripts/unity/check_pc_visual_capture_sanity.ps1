param(
    [string]$RepoRoot = "",
    [string]$CaptureDir = "",
    [string[]]$Presets = @("mechlab", "spawn", "airfield", "hangar-contact", "damage-demo", "north-patrol"),
    [int]$ExpectedWidth = 1280,
    [int]$ExpectedHeight = 720,
    [int]$MinimumPngBytes = 120000,
    [int]$MinimumUniqueColors = 180,
    [int]$MinimumCenterUniqueColors = 80,
    [double]$MinimumCenterLitRatio = 0.15,
    [double]$MinimumLumaStdDev = 12.0,
    [double]$MaximumMagentaRatio = 0.03,
    [double]$MaximumMonochromeRatio = 0.90,
    [switch]$SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ([string]::IsNullOrWhiteSpace($CaptureDir)) {
    $CaptureDir = Join-Path $RepoRoot "analysis-output\reference-visual-captures"
}

Add-Type -AssemblyName System.Drawing

$failures = New-Object System.Collections.Generic.List[string]
$rows = New-Object System.Collections.Generic.List[object]

function Add-Failure {
    param([string]$Message)
    [void]$failures.Add($Message)
}

function Measure-CaptureImage {
    param([string]$Path)

    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        if ($bitmap.Width -ne $ExpectedWidth -or $bitmap.Height -ne $ExpectedHeight) {
            Add-Failure "Unexpected image size for $Path`: $($bitmap.Width)x$($bitmap.Height), expected ${ExpectedWidth}x${ExpectedHeight}."
        }

        $unique = New-Object 'System.Collections.Generic.HashSet[int]'
        $centerUnique = New-Object 'System.Collections.Generic.HashSet[int]'
        $lumaValues = New-Object System.Collections.Generic.List[double]
        $samples = 0
        $centerSamples = 0
        $centerLit = 0
        $magentaSamples = 0
        $monochromeSamples = 0
        $step = 8
        $centerLeft = [int]($bitmap.Width * 0.32)
        $centerRight = [int]($bitmap.Width * 0.72)
        $centerTop = [int]($bitmap.Height * 0.24)
        $centerBottom = [int]($bitmap.Height * 0.76)

        for ($y = 0; $y -lt $bitmap.Height; $y += $step) {
            for ($x = 0; $x -lt $bitmap.Width; $x += $step) {
                $color = $bitmap.GetPixel($x, $y)
                $samples++
                [void]$unique.Add($color.ToArgb())

                $maxChannel = [Math]::Max($color.R, [Math]::Max($color.G, $color.B))
                $minChannel = [Math]::Min($color.R, [Math]::Min($color.G, $color.B))
                if (($maxChannel - $minChannel) -lt 8) {
                    $monochromeSamples++
                }

                if ($color.R -gt 180 -and $color.B -gt 180 -and $color.G -lt 110) {
                    $magentaSamples++
                }

                $luma = ($color.R * 0.2126) + ($color.G * 0.7152) + ($color.B * 0.0722)
                $lumaValues.Add($luma)

                if ($x -ge $centerLeft -and $x -le $centerRight -and $y -ge $centerTop -and $y -le $centerBottom) {
                    [void]$centerUnique.Add($color.ToArgb())
                    if ($luma -gt 18) {
                        $centerLit++
                    }

                    $centerSamples++
                }
            }
        }

        $averageLuma = ($lumaValues | Measure-Object -Average).Average
        $lumaVariance = (($lumaValues | ForEach-Object { [Math]::Pow($_ - $averageLuma, 2) }) | Measure-Object -Average).Average
        $lumaStdDev = [Math]::Sqrt($lumaVariance)
        $centerLitRatio = if ($centerSamples -le 0) { 0.0 } else { $centerLit / [double]$centerSamples }
        $magentaRatio = if ($samples -le 0) { 0.0 } else { $magentaSamples / [double]$samples }
        $monochromeRatio = if ($samples -le 0) { 0.0 } else { $monochromeSamples / [double]$samples }

        return [pscustomobject]@{
            Width = $bitmap.Width
            Height = $bitmap.Height
            UniqueColors = $unique.Count
            CenterUniqueColors = $centerUnique.Count
            CenterLitRatio = [Math]::Round($centerLitRatio, 3)
            LumaStdDev = [Math]::Round($lumaStdDev, 2)
            MagentaRatio = [Math]::Round($magentaRatio, 4)
            MonochromeRatio = [Math]::Round($monochromeRatio, 3)
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function New-SanityTestImage {
    param(
        [string]$Path,
        [string]$Mode
    )

    $bitmap = [System.Drawing.Bitmap]::new($ExpectedWidth, $ExpectedHeight)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        try {
            if ($Mode -eq "valid") {
                $columns = 40
                $rows = 20
                $cellWidth = [Math]::Ceiling($ExpectedWidth / [double]$columns)
                $cellHeight = [Math]::Ceiling($ExpectedHeight / [double]$rows)
                for ($row = 0; $row -lt $rows; $row++) {
                    for ($column = 0; $column -lt $columns; $column++) {
                        $red = 20 + ((30 + ($column * 5) + ($row * 7)) % 170)
                        $green = 120 + ((50 + ($column * 11) + ($row * 3)) % 100)
                        $blue = 20 + ((70 + ($column * 2) + ($row * 13)) % 170)
                        $brush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, $red, $green, $blue))
                        try {
                            $graphics.FillRectangle($brush, $column * $cellWidth, $row * $cellHeight, $cellWidth, $cellHeight)
                        }
                        finally {
                            $brush.Dispose()
                        }
                    }
                }
            }
            elseif ($Mode -eq "flat") {
                $graphics.Clear([System.Drawing.Color]::FromArgb(255, 80, 80, 80))
            }
            elseif ($Mode -eq "magenta") {
                $graphics.Clear([System.Drawing.Color]::FromArgb(255, 255, 0, 255))
            }
            else {
                throw "Unknown sanity test image mode: $Mode"
            }
        }
        finally {
            $graphics.Dispose()
        }

        $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $bitmap.Dispose()
    }
}

function Assert-SelfTestMetric {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

if ($SelfTest) {
    $selfTestDir = Join-Path $RepoRoot "analysis-output\pc-visual-sanity-selftest"
    New-Item -ItemType Directory -Force -Path $selfTestDir | Out-Null

    $validPath = Join-Path $selfTestDir "valid.png"
    $flatPath = Join-Path $selfTestDir "flat.png"
    $magentaPath = Join-Path $selfTestDir "magenta.png"
    New-SanityTestImage -Path $validPath -Mode "valid"
    New-SanityTestImage -Path $flatPath -Mode "flat"
    New-SanityTestImage -Path $magentaPath -Mode "magenta"

    $valid = Measure-CaptureImage -Path $validPath
    Assert-SelfTestMetric -Condition ($valid.UniqueColors -ge $MinimumUniqueColors) -Message "Self-test valid image did not produce enough colors: $($valid.UniqueColors)."
    Assert-SelfTestMetric -Condition ($valid.CenterUniqueColors -ge $MinimumCenterUniqueColors) -Message "Self-test valid image center did not produce enough colors: $($valid.CenterUniqueColors)."
    Assert-SelfTestMetric -Condition ($valid.CenterLitRatio -ge $MinimumCenterLitRatio) -Message "Self-test valid image center lit ratio too low: $($valid.CenterLitRatio)."
    Assert-SelfTestMetric -Condition ($valid.LumaStdDev -ge $MinimumLumaStdDev) -Message "Self-test valid image luminance contrast too low: $($valid.LumaStdDev)."
    Assert-SelfTestMetric -Condition ($valid.MagentaRatio -le $MaximumMagentaRatio) -Message "Self-test valid image magenta ratio too high: $($valid.MagentaRatio)."
    Assert-SelfTestMetric -Condition ($valid.MonochromeRatio -le $MaximumMonochromeRatio) -Message "Self-test valid image monochrome ratio too high: $($valid.MonochromeRatio)."

    $flat = Measure-CaptureImage -Path $flatPath
    Assert-SelfTestMetric -Condition ($flat.UniqueColors -lt $MinimumUniqueColors) -Message "Self-test flat image was not detected by color-count threshold."
    Assert-SelfTestMetric -Condition ($flat.LumaStdDev -lt $MinimumLumaStdDev) -Message "Self-test flat image was not detected by luminance threshold."
    Assert-SelfTestMetric -Condition ($flat.MonochromeRatio -gt $MaximumMonochromeRatio) -Message "Self-test flat image was not detected by monochrome threshold."

    $magenta = Measure-CaptureImage -Path $magentaPath
    Assert-SelfTestMetric -Condition ($magenta.MagentaRatio -gt $MaximumMagentaRatio) -Message "Self-test magenta image was not detected by magenta threshold."

    Write-Host "PC visual capture sanity self-test OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "SelfTestDir: $selfTestDir"
    return
}

foreach ($preset in $Presets) {
    $pngPath = Join-Path $CaptureDir "$preset.png"
    if (-not (Test-Path -LiteralPath $pngPath -PathType Leaf)) {
        Add-Failure "$preset screenshot missing: $pngPath"
        continue
    }

    $png = Get-Item -LiteralPath $pngPath
    if ($png.Length -lt $MinimumPngBytes) {
        Add-Failure "$preset screenshot too small: $($png.Length) bytes, expected at least $MinimumPngBytes."
    }

    $metrics = Measure-CaptureImage -Path $pngPath
    if ($metrics.UniqueColors -lt $MinimumUniqueColors) {
        Add-Failure "$preset screenshot has too few sampled colors: $($metrics.UniqueColors), expected at least $MinimumUniqueColors."
    }

    if ($metrics.CenterUniqueColors -lt $MinimumCenterUniqueColors) {
        Add-Failure "$preset screenshot center is too flat: $($metrics.CenterUniqueColors) sampled colors, expected at least $MinimumCenterUniqueColors."
    }

    if ($metrics.CenterLitRatio -lt $MinimumCenterLitRatio) {
        Add-Failure "$preset screenshot center is too dark or occluded: $($metrics.CenterLitRatio), expected at least $MinimumCenterLitRatio."
    }

    if ($metrics.LumaStdDev -lt $MinimumLumaStdDev) {
        Add-Failure "$preset screenshot has too little luminance contrast: $($metrics.LumaStdDev), expected at least $MinimumLumaStdDev."
    }

    if ($metrics.MagentaRatio -gt $MaximumMagentaRatio) {
        Add-Failure "$preset screenshot has too much magenta fallback/pink-box color: $($metrics.MagentaRatio), expected at most $MaximumMagentaRatio."
    }

    if ($metrics.MonochromeRatio -gt $MaximumMonochromeRatio) {
        Add-Failure "$preset screenshot is too close to monochrome: $($metrics.MonochromeRatio), expected at most $MaximumMonochromeRatio."
    }

    [void]$rows.Add([pscustomobject]@{
        Preset = $preset
        Bytes = $png.Length
        UniqueColors = $metrics.UniqueColors
        CenterUniqueColors = $metrics.CenterUniqueColors
        CenterLitRatio = $metrics.CenterLitRatio
        LumaStdDev = $metrics.LumaStdDev
        MagentaRatio = $metrics.MagentaRatio
        MonochromeRatio = $metrics.MonochromeRatio
    })
}

if ($failures.Count -gt 0) {
    Write-Host "PC visual capture sanity check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC visual capture sanity check(s) failed."
}

Write-Host "PC visual capture sanity check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "CaptureDir: $CaptureDir"
$rows | Format-Table -AutoSize
