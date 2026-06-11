param(
    [string]$RepoRoot = "",
    [string]$CaptureDir = "",
    [int]$ExpectedWidth = 1280,
    [int]$ExpectedHeight = 720,
    [int]$MinimumPngBytes = 100000
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
else {
    $CaptureDir = (Resolve-Path -LiteralPath $CaptureDir).Path
}

$expectedPresets = @("mechlab", "spawn", "airfield", "hangar-contact", "damage-demo", "north-patrol")
$battlePresets = @("spawn", "airfield", "hangar-contact", "damage-demo", "north-patrol")
$failures = New-Object System.Collections.Generic.List[string]
$rows = New-Object System.Collections.Generic.List[object]

function Add-Failure {
    param([string]$Message)
    [void]$failures.Add($Message)
}

function Add-Row {
    param(
        [string]$Preset,
        [object]$Sidecar,
        [long]$PngBytes
    )

    [void]$rows.Add([pscustomobject]@{
        Preset = $Preset
        Flow = [string]$Sidecar.flowScreen
        Size = "$($Sidecar.screenWidth)x$($Sidecar.screenHeight)"
        Time = [Math]::Round([double]$Sidecar.missionTimeSeconds, 2)
        Units = [int]$Sidecar.playerUnitCount
        Hostiles = [int]$Sidecar.activeHostileCount
        PngBytes = $PngBytes
    })
}

function Assert-FileExists {
    param(
        [string]$Path,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "$Label missing: $Path"
        return $false
    }

    return $true
}

function Assert-Text {
    param(
        [string]$Value,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        Add-Failure "$Label missing or blank"
    }
}

function Assert-Contains {
    param(
        [string]$Value,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Value) -or -not $Value.Contains($Needle)) {
        Add-Failure "$Label missing marker: $Needle"
    }
}

function Assert-NonNegativeInt {
    param(
        [object]$Value,
        [string]$Label
    )

    try {
        $number = [int]$Value
        if ($number -lt 0) {
            Add-Failure "$Label is negative: $number"
        }
    }
    catch {
        Add-Failure "$Label is not an integer: $Value"
    }
}

function Assert-PositiveNumber {
    param(
        [object]$Value,
        [string]$Label
    )

    try {
        $number = [double]$Value
        if ($number -le 0) {
            Add-Failure "$Label must be positive: $number"
        }
    }
    catch {
        Add-Failure "$Label is not numeric: $Value"
    }
}

function Get-PropertyValue {
    param(
        [object]$Object,
        [string]$Name,
        [string]$Label
    )

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        Add-Failure "$Label missing property: $Name"
        return $null
    }

    return $property.Value
}

function Assert-Vector3 {
    param(
        [object]$Object,
        [string]$Label
    )

    if ($null -eq $Object) {
        Add-Failure "$Label missing"
        return
    }

    foreach ($axis in @("x", "y", "z")) {
        $value = Get-PropertyValue -Object $Object -Name $axis -Label $Label
        if ($null -ne $value) {
            try {
                [void][double]$value
            }
            catch {
                Add-Failure "$Label.$axis is not numeric: $value"
            }
        }
    }
}

function Assert-ReferenceAssets {
    param(
        [object]$Assets,
        [string]$Preset
    )

    if ($null -eq $Assets) {
        Add-Failure "$Preset referenceAssets missing"
        return
    }

    foreach ($name in @("terrain", "structures", "props", "scale", "occlusion")) {
        $value = Get-PropertyValue -Object $Assets -Name $name -Label "$Preset referenceAssets"
        Assert-Text -Value ([string]$value) -Label "$Preset referenceAssets.$name"
    }
}

function Assert-Camera {
    param(
        [object]$Camera,
        [string]$Preset
    )

    if ($null -eq $Camera) {
        Add-Failure "$Preset camera missing"
        return
    }

    Assert-PositiveNumber -Value $Camera.orthographicSize -Label "$Preset camera.orthographicSize"
    Assert-PositiveNumber -Value $Camera.zoomScale -Label "$Preset camera.zoomScale"
    Assert-PositiveNumber -Value $Camera.pitch -Label "$Preset camera.pitch"
    [void](Get-PropertyValue -Object $Camera -Name "yaw" -Label "$Preset camera")
    Assert-Vector3 -Object $Camera.position -Label "$Preset camera.position"
    Assert-Vector3 -Object $Camera.rotationEuler -Label "$Preset camera.rotationEuler"
    Assert-Vector3 -Object $Camera.followOffset -Label "$Preset camera.followOffset"
    Assert-Vector3 -Object $Camera.compositionOffset -Label "$Preset camera.compositionOffset"
}

function Test-Sidecar {
    param([string]$Preset)

    $jsonPath = Join-Path $CaptureDir "$Preset.json"
    $pngPath = Join-Path $CaptureDir "$Preset.png"
    $logPath = Join-Path $CaptureDir "$Preset.log"

    $hasJson = Assert-FileExists -Path $jsonPath -Label "$Preset sidecar"
    $hasPng = Assert-FileExists -Path $pngPath -Label "$Preset screenshot"
    [void](Assert-FileExists -Path $logPath -Label "$Preset capture log")
    if (-not $hasJson) {
        return
    }

    try {
        $sidecar = Get-Content -LiteralPath $jsonPath -Raw | ConvertFrom-Json
    }
    catch {
        Add-Failure "$Preset sidecar is not valid JSON: $jsonPath"
        return
    }

    if ([string]$sidecar.preset -ne $Preset) {
        Add-Failure "$Preset sidecar preset mismatch: $($sidecar.preset)"
    }

    if ([int]$sidecar.screenWidth -ne $ExpectedWidth -or [int]$sidecar.screenHeight -ne $ExpectedHeight) {
        Add-Failure "$Preset capture size mismatch: $($sidecar.screenWidth)x$($sidecar.screenHeight), expected ${ExpectedWidth}x${ExpectedHeight}"
    }

    $screenshot = [string]$sidecar.screenshot
    Assert-Text -Value $screenshot -Label "$Preset screenshot field"
    if (-not [string]::IsNullOrWhiteSpace($screenshot)) {
        if ([System.IO.Path]::GetFileName($screenshot) -ne "$Preset.png") {
            Add-Failure "$Preset screenshot field does not point to $Preset.png`: $screenshot"
        }

        if ((Test-Path -LiteralPath $screenshot -PathType Leaf)) {
            $resolvedSidecarPath = (Resolve-Path -LiteralPath $screenshot).Path
            $resolvedExpectedPath = (Resolve-Path -LiteralPath $pngPath).Path
            if (-not $resolvedSidecarPath.Equals($resolvedExpectedPath, [StringComparison]::OrdinalIgnoreCase)) {
                Add-Failure "$Preset screenshot field points outside expected capture PNG: $screenshot"
            }
        }
        else {
            Add-Failure "$Preset screenshot field path does not exist: $screenshot"
        }
    }

    Assert-Text -Value ([string]$sidecar.missionId) -Label "$Preset missionId"
    Assert-Text -Value ([string]$sidecar.result) -Label "$Preset result"
    Assert-Text -Value ([string]$sidecar.status) -Label "$Preset status"
    Assert-NonNegativeInt -Value $sidecar.playerUnitCount -Label "$Preset playerUnitCount"
    Assert-NonNegativeInt -Value $sidecar.activeHostileCount -Label "$Preset activeHostileCount"
    Assert-NonNegativeInt -Value $sidecar.visibleHostileCount -Label "$Preset visibleHostileCount"
    Assert-NonNegativeInt -Value $sidecar.targetableStructureCount -Label "$Preset targetableStructureCount"
    Assert-NonNegativeInt -Value $sidecar.currentObjectiveCount -Label "$Preset currentObjectiveCount"

    if ([double]$sidecar.missionTimeSeconds -lt 0) {
        Add-Failure "$Preset missionTimeSeconds is negative: $($sidecar.missionTimeSeconds)"
    }

    if ($null -eq $sidecar.currentObjectives -or $sidecar.currentObjectives.Count -lt 1) {
        Add-Failure "$Preset currentObjectives missing"
    }

    Assert-Contains -Value ([string]$sidecar.firstMapVisual) -Needle "FirstMapVisual=" -Label "$Preset firstMapVisual"
    Assert-Contains -Value ([string]$sidecar.occupancy) -Needle "BattleOccupancy=" -Label "$Preset occupancy"
    Assert-Contains -Value ([string]$sidecar.occupancyPlaceholders) -Needle "OccupancyPlaceholders=enabled" -Label "$Preset occupancyPlaceholders"
    Assert-Contains -Value ([string]$sidecar.terrainReadability) -Needle "TerrainReadability=" -Label "$Preset terrainReadability"
    Assert-Contains -Value ([string]$sidecar.unitReadability) -Needle "UnitReadability=" -Label "$Preset unitReadability"
    Assert-Contains -Value ([string]$sidecar.structureReadability) -Needle "StructureReadability=" -Label "$Preset structureReadability"
    Assert-Contains -Value ([string]$sidecar.contactSpread) -Needle "ContactSpread=" -Label "$Preset contactSpread"
    Assert-Contains -Value ([string]$sidecar.contactClearance) -Needle "ContactClearance=" -Label "$Preset contactClearance"
    Assert-Contains -Value ([string]$sidecar.damageStory) -Needle "DamageStory=" -Label "$Preset damageStory"
    Assert-Contains -Value ([string]$sidecar.damageReadability) -Needle "DamageReadability=" -Label "$Preset damageReadability"
    Assert-Contains -Value ([string]$sidecar.battleHud) -Needle "BattleHud=" -Label "$Preset battleHud"
    Assert-Camera -Camera $sidecar.camera -Preset $Preset
    Assert-ReferenceAssets -Assets $sidecar.referenceAssets -Preset $Preset

    if ($battlePresets -contains $Preset) {
        if ([string]$sidecar.flowScreen -ne "Battle") {
            Add-Failure "$Preset flowScreen mismatch: $($sidecar.flowScreen)"
        }
        Assert-Contains -Value ([string]$sidecar.firstMapVisual) -Needle "status=ready" -Label "$Preset firstMapVisual"
        Assert-Contains -Value ([string]$sidecar.battleHud) -Needle "BattleHud=active" -Label "$Preset battleHud"
    }
    elseif ($Preset -eq "mechlab") {
        if ([string]$sidecar.flowScreen -ne "Mech Lab") {
            Add-Failure "$Preset flowScreen mismatch: $($sidecar.flowScreen)"
        }
        Assert-Contains -Value ([string]$sidecar.firstMapVisual) -Needle "FirstMapVisual=inactive" -Label "$Preset firstMapVisual"
        Assert-Contains -Value ([string]$sidecar.battleHud) -Needle "BattleHud=inactive" -Label "$Preset battleHud"
        Assert-Contains -Value ([string]$sidecar.mechLab) -Needle "MechLabCapture=open" -Label "$Preset mechLab"
    }

    $pngBytes = 0
    if ($hasPng) {
        $pngBytes = (Get-Item -LiteralPath $pngPath).Length
        if ($pngBytes -lt $MinimumPngBytes) {
            Add-Failure "$Preset screenshot too small: $pngBytes bytes, expected at least $MinimumPngBytes"
        }
    }

    Add-Row -Preset $Preset -Sidecar $sidecar -PngBytes $pngBytes
}

if (-not (Test-Path -LiteralPath $CaptureDir -PathType Container)) {
    throw "CaptureDir missing: $CaptureDir"
}

foreach ($preset in $expectedPresets) {
    Test-Sidecar -Preset $preset
}

if ($failures.Count -gt 0) {
    Write-Host "PC capture sidecar schema check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC capture sidecar schema check(s) failed."
}

Write-Host "PC capture sidecar schema check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "CaptureDir: $CaptureDir"
$rows | Format-Table -AutoSize
