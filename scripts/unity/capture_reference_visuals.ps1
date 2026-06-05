param(
    [string]$RepoRoot = "",
    [string]$OutputDir = "",
    [string[]]$Presets = @("spawn", "airfield", "hangar-contact", "north-patrol", "damage-demo"),
    [int]$Width = 1280,
    [int]$Height = 720,
    [switch]$SkipRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot "analysis-output\reference-visual-captures"
}

$gameExe = Join-Path $RepoRoot "unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe"
if (-not (Test-Path $gameExe)) {
    throw "Missing Unity player build: $gameExe"
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
Add-Type -AssemblyName System.Drawing

function Normalize-CapturePreset {
    param([string]$Preset)
    if ([string]::IsNullOrWhiteSpace($Preset)) {
        return "spawn"
    }

    return $Preset.Trim().ToLowerInvariant().Replace("_", "-")
}

function Expand-CapturePresets {
    param([string[]]$RawPresets)
    $expanded = New-Object System.Collections.Generic.List[string]
    foreach ($rawPreset in $RawPresets) {
        foreach ($part in ($rawPreset -split ",")) {
            if (-not [string]::IsNullOrWhiteSpace($part)) {
                $expanded.Add((Normalize-CapturePreset $part))
            }
        }
    }

    return $expanded.ToArray()
}

function Test-CaptureImage {
    param(
        [string]$Path,
        [int]$ExpectedWidth,
        [int]$ExpectedHeight
    )

    if (-not (Test-Path $Path)) {
        throw "Missing capture image: $Path"
    }

    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        if ($bitmap.Width -ne $ExpectedWidth -or $bitmap.Height -ne $ExpectedHeight) {
            throw "Unexpected image size for $Path`: $($bitmap.Width)x$($bitmap.Height), expected ${ExpectedWidth}x${ExpectedHeight}"
        }

        $unique = New-Object 'System.Collections.Generic.HashSet[int]'
        $centerUnique = New-Object 'System.Collections.Generic.HashSet[int]'
        $centerLit = 0
        $centerSamples = 0
        $step = 8
        $centerLeft = [int]($bitmap.Width * 0.32)
        $centerRight = [int]($bitmap.Width * 0.72)
        $centerTop = [int]($bitmap.Height * 0.24)
        $centerBottom = [int]($bitmap.Height * 0.76)

        for ($y = 0; $y -lt $bitmap.Height; $y += $step) {
            for ($x = 0; $x -lt $bitmap.Width; $x += $step) {
                $color = $bitmap.GetPixel($x, $y)
                [void]$unique.Add($color.ToArgb())
                if ($x -ge $centerLeft -and $x -le $centerRight -and $y -ge $centerTop -and $y -le $centerBottom) {
                    [void]$centerUnique.Add($color.ToArgb())
                    $luma = ($color.R * 0.2126) + ($color.G * 0.7152) + ($color.B * 0.0722)
                    if ($luma -gt 18) {
                        $centerLit++
                    }

                    $centerSamples++
                }
            }
        }

        if ($unique.Count -lt 64) {
            throw "Image looks too flat: $Path uniqueColors=$($unique.Count)"
        }

        if ($centerUnique.Count -lt 24) {
            throw "Center view looks occluded or blank: $Path centerUniqueColors=$($centerUnique.Count)"
        }

        $litRatio = if ($centerSamples -le 0) { 0.0 } else { $centerLit / [double]$centerSamples }
        if ($litRatio -lt 0.04) {
            throw "Center view is too dark or occluded: $Path litRatio=$([Math]::Round($litRatio, 3))"
        }

        return [pscustomobject]@{
            Width = $bitmap.Width
            Height = $bitmap.Height
            UniqueColors = $unique.Count
            CenterUniqueColors = $centerUnique.Count
            CenterLitRatio = [Math]::Round($litRatio, 3)
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Wait-CaptureFile {
    param(
        [string]$Path,
        [int]$TimeoutSeconds = 15
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    $lastLength = -1
    while ((Get-Date) -lt $deadline) {
        if (Test-Path $Path) {
            $item = Get-Item $Path
            if ($item.Length -gt 0 -and $item.Length -eq $lastLength) {
                return
            }

            $lastLength = $item.Length
        }

        Start-Sleep -Milliseconds 200
    }

    throw "Timed out waiting for capture file: $Path"
}

function Test-CaptureSidecar {
    param(
        [string]$Path,
        [string]$ExpectedPreset
    )

    if (-not (Test-Path $Path)) {
        throw "Missing capture sidecar: $Path"
    }

    $sidecar = Get-Content -Path $Path -Raw | ConvertFrom-Json
    if ($sidecar.preset -ne $ExpectedPreset) {
        throw "Unexpected sidecar preset for $Path`: $($sidecar.preset), expected $ExpectedPreset"
    }

    if ($sidecar.screenWidth -le 0 -or $sidecar.screenHeight -le 0) {
        throw "Sidecar has invalid screen dimensions: $Path"
    }

    if ($null -eq $sidecar.camera -or $sidecar.camera.orthographicSize -le 0) {
        throw "Sidecar has invalid camera state: $Path"
    }

    return $sidecar
}

$results = New-Object System.Collections.Generic.List[object]
foreach ($normalizedPreset in (Expand-CapturePresets $Presets)) {
    $pngPath = Join-Path $OutputDir "$normalizedPreset.png"
    $jsonPath = Join-Path $OutputDir "$normalizedPreset.json"
    $logPath = Join-Path $OutputDir "$normalizedPreset.log"

    if (-not $SkipRun) {
        $args = @(
            "-screen-width", $Width.ToString([Globalization.CultureInfo]::InvariantCulture),
            "-screen-height", $Height.ToString([Globalization.CultureInfo]::InvariantCulture),
            "-screen-fullscreen", "0",
            "-mc2CapturePreset", $normalizedPreset,
            "-mc2CaptureScreenshot", $pngPath,
            "-mc2CaptureSidecar", $jsonPath,
            "-mc2CaptureQuit",
            "-logFile", $logPath
        )

        & $gameExe @args
        $exitCodeVariable = Get-Variable -Name LASTEXITCODE -ErrorAction SilentlyContinue
        $exitCode = if ($null -eq $exitCodeVariable) { 0 } else { [int]$exitCodeVariable.Value }
        if ($exitCode -ne 0) {
            throw "Capture preset '$normalizedPreset' failed with exit code $exitCode. Log: $logPath"
        }

        Wait-CaptureFile -Path $pngPath
        Wait-CaptureFile -Path $jsonPath
    }

    $imageCheck = Test-CaptureImage -Path $pngPath -ExpectedWidth $Width -ExpectedHeight $Height
    $sidecar = Test-CaptureSidecar -Path $jsonPath -ExpectedPreset $normalizedPreset
    $results.Add([pscustomobject]@{
        Preset = $normalizedPreset
        Png = $pngPath
        Json = $jsonPath
        MissionTime = [Math]::Round([double]$sidecar.missionTimeSeconds, 2)
        ActiveHostiles = [int]$sidecar.activeHostileCount
        VisibleHostiles = [int]$sidecar.visibleHostileCount
        UniqueColors = $imageCheck.UniqueColors
        CenterLitRatio = $imageCheck.CenterLitRatio
    })
}

$results | Format-Table -AutoSize
Write-Host "MC2 reference visual captures passed: $($results.Count) preset(s) -> $OutputDir"
