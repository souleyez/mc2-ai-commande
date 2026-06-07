param(
    [string]$RepoRoot = "",
    [string]$OutputDir = "",
    [string[]]$Presets = @("spawn", "airfield", "hangar-contact", "north-patrol", "damage-demo"),
    [int]$Width = 1280,
    [int]$Height = 720,
    [int]$CaptureTimeoutSeconds = 45,
    [switch]$NoOccupancyPlaceholders,
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
        [int]$TimeoutSeconds = 45
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

function Stop-CapturePlayer {
    param([string]$ExecutablePath)

    $resolvedExe = (Resolve-Path -LiteralPath $ExecutablePath).Path
    $buildDir = Split-Path -Parent $resolvedExe
    Get-Process | Where-Object {
        try {
            if ($_.Path -eq $resolvedExe) {
                $true
            }
            elseif ($_.ProcessName -eq "UnityCrashHandler64" -and $_.Path) {
                $_.Path.StartsWith($buildDir, [StringComparison]::OrdinalIgnoreCase)
            }
            else {
                $false
            }
        }
        catch {
            $false
        }
    } | Stop-Process -Force -ErrorAction SilentlyContinue
}

function Test-CaptureSidecar {
    param(
        [string]$Path,
        [string]$ExpectedPreset,
        [bool]$ExpectOccupancyPlaceholders = $true
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

    if ($ExpectedPreset -eq "mechlab") {
        Test-MechLabCaptureSidecar -Sidecar $sidecar -Path $Path
    }
    elseif ($ExpectedPreset -eq "damage-demo") {
        Test-DamageDemoCaptureSidecar -Sidecar $sidecar -Path $Path
    }

    if ($sidecar.flowScreen -eq "Battle") {
        Test-BattleHudCaptureSidecar -Sidecar $sidecar -Path $Path
    }

    $placeholderSummary = [string]$sidecar.occupancyPlaceholders
    if ($ExpectOccupancyPlaceholders) {
        foreach ($fragment in @(
            "OccupancyPlaceholders=enabled",
            " units ",
            " structures ",
            " hardProps ",
            " landingBlockedMarkers ",
            "source=BattleMission.OccupancyPlaceholderRegions+DemoTerrainView.LandingReviewBlockedMarkers"
        )) {
            if ($placeholderSummary -notlike "*$fragment*") {
                throw "Sidecar occupancy placeholder summary missing '$fragment': $Path -> $placeholderSummary"
            }
        }
    }
    elseif ($placeholderSummary -notlike "OccupancyPlaceholders=disabled*") {
        throw "Sidecar occupancy placeholders should be disabled: $Path -> $placeholderSummary"
    }

    return $sidecar
}

function Test-BattleHudCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    $summary = [string]$Sidecar.battleHud
    foreach ($fragment in @(
        "BattleHud=active",
        "controls=statusRows+jet+map+bay+system",
        "combatLogVisible=no",
        "objectivePanel=compactObjective",
        "saveUi=disabled"
    )) {
        if ($summary -notlike "*$fragment*") {
            throw "Battle HUD sidecar summary missing '$fragment': $Path -> $summary"
        }
    }

    if ($summary -notmatch "combatPanel=h([0-9.]+)") {
        throw "Battle HUD sidecar summary missing combat panel height: $Path -> $summary"
    }

    $height = [double]$Matches[1]
    if ($height -gt 84.0) {
        throw "Battle HUD combat panel is too tall for sparse mode: $Path -> $summary"
    }
}

function Test-DamageDemoCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    if ($Sidecar.flowScreen -ne "Battle") {
        throw "Damage demo capture did not stay in battle flow: $Path -> $($Sidecar.flowScreen)"
    }

    if ([double]$Sidecar.camera.zoomScale -lt 1.05) {
        throw "Damage demo capture is too zoomed out for section-damage readability: $Path -> zoom=$($Sidecar.camera.zoomScale)"
    }

    $summary = [string]$Sidecar.damageStory
    foreach ($fragment in @(
        "DamageStory=units",
        "left-arm-lost",
        "legs-lost",
        "cockpit-lost",
        "lostSections=",
        "pilotRisk=",
        "destroyedUnits=",
        "story="
    )) {
        if ($summary -notlike "*$fragment*") {
            throw "Damage demo sidecar summary missing '$fragment': $Path -> $summary"
        }
    }

    foreach ($pattern in @(
        "lostSections=[1-9]",
        "arms=[1-9]",
        "legs=[1-9]",
        "cockpit=[1-9]",
        "pilotRisk=[1-9]"
    )) {
        if ($summary -notmatch $pattern) {
            throw "Damage demo sidecar summary did not prove '$pattern': $Path -> $summary"
        }
    }
}

function Test-MechLabCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    if ($Sidecar.flowScreen -ne "Mech Lab") {
        throw "MechLab capture did not open the Mech Lab flow: $Path -> $($Sidecar.flowScreen)"
    }

    $summary = [string]$Sidecar.mechLab
    foreach ($fragment in @(
        "MechLabCapture=open",
        "weaponBlock=",
        "fillers=A+/C+",
        "fit=",
        "pressure=H ",
        " W ",
        "alwaysMounted=weapons",
        "noToggle=yes"
    )) {
        if ($summary -notlike "*$fragment*") {
            throw "MechLab sidecar summary missing '$fragment': $Path -> $summary"
        }
    }

    if ($summary -notmatch "\d+x\d+") {
        throw "MechLab sidecar summary missing block shape label: $Path -> $summary"
    }

    foreach ($forbidden in @("toggle", "enable", "disable", "unmount")) {
        if ($summary -match "(?i)(^|[\s=_-])$forbidden($|[\s=_-])") {
            throw "MechLab sidecar summary contains forbidden weapon toggle copy '$forbidden': $Path -> $summary"
        }
    }

    foreach ($forbidden in @("启用", "关闭")) {
        if ($summary.IndexOf($forbidden, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
            throw "MechLab sidecar summary contains forbidden weapon toggle copy '$forbidden': $Path -> $summary"
        }
    }
}

$results = New-Object System.Collections.Generic.List[object]
$expectOccupancyPlaceholders = -not $NoOccupancyPlaceholders.IsPresent
foreach ($normalizedPreset in (Expand-CapturePresets $Presets)) {
    $pngPath = Join-Path $OutputDir "$normalizedPreset.png"
    $jsonPath = Join-Path $OutputDir "$normalizedPreset.json"
    $logPath = Join-Path $OutputDir "$normalizedPreset.log"

    if (-not $SkipRun) {
        foreach ($stalePath in @($pngPath, $jsonPath, $logPath)) {
            if (Test-Path $stalePath) {
                Remove-Item -LiteralPath $stalePath -Force
            }
        }

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
        if ($expectOccupancyPlaceholders) {
            $args += "-mc2ShowOccupancyReviewLayer"
        }

        $previousOccupancyPlaceholderEnv = $env:MC2_SHOW_OCCUPANCY_PLACEHOLDERS
        try {
            if ($NoOccupancyPlaceholders) {
                Remove-Item Env:\MC2_SHOW_OCCUPANCY_PLACEHOLDERS -ErrorAction SilentlyContinue
            }
            else {
                $env:MC2_SHOW_OCCUPANCY_PLACEHOLDERS = "1"
            }

            try {
                & $gameExe @args
                $exitCodeVariable = Get-Variable -Name LASTEXITCODE -ErrorAction SilentlyContinue
                $exitCode = if ($null -eq $exitCodeVariable) { 0 } else { [int]$exitCodeVariable.Value }
                if ($exitCode -ne 0) {
                    throw "Capture preset '$normalizedPreset' failed with exit code $exitCode. Log: $logPath"
                }

                Wait-CaptureFile -Path $pngPath -TimeoutSeconds $CaptureTimeoutSeconds
                Wait-CaptureFile -Path $jsonPath -TimeoutSeconds $CaptureTimeoutSeconds
            }
            catch {
                Stop-CapturePlayer -ExecutablePath $gameExe
                throw
            }
        }
        finally {
            if ($null -eq $previousOccupancyPlaceholderEnv) {
                Remove-Item Env:\MC2_SHOW_OCCUPANCY_PLACEHOLDERS -ErrorAction SilentlyContinue
            }
            else {
                $env:MC2_SHOW_OCCUPANCY_PLACEHOLDERS = $previousOccupancyPlaceholderEnv
            }
        }

        Stop-CapturePlayer -ExecutablePath $gameExe
    }

    $imageCheck = Test-CaptureImage -Path $pngPath -ExpectedWidth $Width -ExpectedHeight $Height
    $sidecar = Test-CaptureSidecar -Path $jsonPath -ExpectedPreset $normalizedPreset -ExpectOccupancyPlaceholders $expectOccupancyPlaceholders
    $results.Add([pscustomobject]@{
        Preset = $normalizedPreset
        Png = $pngPath
        Json = $jsonPath
        MissionTime = [Math]::Round([double]$sidecar.missionTimeSeconds, 2)
        ActiveHostiles = [int]$sidecar.activeHostileCount
        VisibleHostiles = [int]$sidecar.visibleHostileCount
        UniqueColors = $imageCheck.UniqueColors
        CenterLitRatio = $imageCheck.CenterLitRatio
        MechLab = if ($normalizedPreset -eq "mechlab") { [string]$sidecar.mechLab } else { "" }
    })
}

$results | Format-Table -AutoSize
Write-Host "MC2 reference visual captures passed: $($results.Count) preset(s) -> $OutputDir"
