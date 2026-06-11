param(
    [string]$RepoRoot = "",
    [string]$CaptureDir = "",
    [string]$BuildExe = "",
    [string]$VisibleFlowLog = "",
    [int]$MinimumPngBytes = 100000
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}

if ([string]::IsNullOrWhiteSpace($CaptureDir)) {
    $CaptureDir = Join-Path $RepoRoot "analysis-output\reference-visual-captures"
}

if ([string]::IsNullOrWhiteSpace($BuildExe)) {
    $BuildExe = Join-Path $RepoRoot "unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe"
}

if ([string]::IsNullOrWhiteSpace($VisibleFlowLog)) {
    $VisibleFlowLog = Join-Path $RepoRoot "analysis-output\unity-player-pc-evidence-visible-flow.log"
}

$failures = New-Object System.Collections.Generic.List[string]
$rows = New-Object System.Collections.Generic.List[object]

function Add-Failure {
    param([string]$Message)
    [void]$failures.Add($Message)
}

function Assert-FileExists {
    param(
        [string]$Path,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Add-Failure "$Label missing: $Path"
        return $false
    }

    return $true
}

function Assert-Contains {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or -not $Text.Contains($Needle)) {
        Add-Failure "$Label missing marker: $Needle"
    }
}

function Read-CaptureSidecar {
    param([string]$Preset)

    $jsonPath = Join-Path $CaptureDir "$Preset.json"
    $pngPath = Join-Path $CaptureDir "$Preset.png"

    if (-not (Assert-FileExists -Path $jsonPath -Label "$Preset sidecar")) {
        return $null
    }

    if (-not (Assert-FileExists -Path $pngPath -Label "$Preset screenshot")) {
        return $null
    }

    $png = Get-Item -LiteralPath $pngPath
    if ($png.Length -lt $MinimumPngBytes) {
        Add-Failure "$Preset screenshot too small: $($png.Length) bytes, expected at least $MinimumPngBytes"
    }

    try {
        $json = Get-Content -LiteralPath $jsonPath -Raw | ConvertFrom-Json
    }
    catch {
        Add-Failure "$Preset sidecar is not valid JSON: $jsonPath"
        return $null
    }

    if ($json.preset -ne $Preset) {
        Add-Failure "$Preset sidecar preset mismatch: $($json.preset)"
    }

    if ([int]$json.screenWidth -ne 1280 -or [int]$json.screenHeight -ne 720) {
        Add-Failure "$Preset capture size mismatch: $($json.screenWidth)x$($json.screenHeight), expected 1280x720"
    }

    [void]$rows.Add([pscustomobject]@{
        Preset = $Preset
        Bytes = $png.Length
        Time = [math]::Round([double]$json.missionTimeSeconds, 2)
        ActiveHostiles = [int]$json.activeHostileCount
        VisibleHostiles = [int]$json.visibleHostileCount
        Flow = [string]$json.flowScreen
    })

    return $json
}

if (Assert-FileExists -Path $BuildExe -Label "Windows demo executable") {
    $dataDir = Join-Path (Split-Path -Parent $BuildExe) "MC2UnityDemo_Data"
    [void](Assert-FileExists -Path $dataDir -Label "Windows demo data folder")
}

if (Assert-FileExists -Path $VisibleFlowLog -Label "visible-flow smoke log") {
    $visibleLog = Get-Content -LiteralPath $VisibleFlowLog -Raw
    Assert-Contains -Text $visibleLog -Needle "MC2 demo smoke test exiting with code 0" -Label "visible-flow smoke"
    Assert-Contains -Text $visibleLog -Needle "MC2 debrief summary assertion OK" -Label "visible-flow smoke"
    Assert-Contains -Text $visibleLog -Needle "MC2 loadout compact assertion OK" -Label "visible-flow smoke"
}

$capturePresets = @("mechlab", "spawn", "airfield", "hangar-contact", "damage-demo", "north-patrol")
$captures = @{}
foreach ($preset in $capturePresets) {
    $capture = Read-CaptureSidecar -Preset $preset
    if ($null -ne $capture) {
        $captures[$preset] = $capture
        Assert-Contains -Text ([string]$capture.terrainReadability) -Needle "textureStrength=0.28" -Label "$preset terrain readability"
        Assert-Contains -Text ([string]$capture.terrainReadability) -Needle "pathing=unchanged" -Label "$preset terrain readability"
        Assert-Contains -Text ([string]$capture.contactClearance) -Needle "overlaps=0" -Label "$preset contact clearance"
        Assert-Contains -Text ([string]$capture.contactClearance) -Needle "status=separated" -Label "$preset contact clearance"
    }
}

foreach ($preset in @("spawn", "airfield", "hangar-contact", "damage-demo", "north-patrol")) {
    if ($captures.ContainsKey($preset)) {
        Assert-Contains -Text ([string]$captures[$preset].firstMapVisual) -Needle "status=ready" -Label "$preset first map visual"
        Assert-Contains -Text ([string]$captures[$preset].firstMapVisual) -Needle "sparseHud=ready" -Label "$preset first map visual"
        Assert-Contains -Text ([string]$captures[$preset].battleHud) -Needle "SparseBattleUi=statusRows+sections+solo" -Label "$preset sparse battle HUD"
        Assert-Contains -Text ([string]$captures[$preset].battleHud) -Needle "combatLog=hidden" -Label "$preset sparse battle HUD"
        Assert-Contains -Text ([string]$captures[$preset].battleHud) -Needle "saveUi=disabled" -Label "$preset sparse battle HUD"
    }
}

if ($captures.ContainsKey("mechlab")) {
    $mechLab = [string]$captures["mechlab"].mechLab
    Assert-Contains -Text $mechLab -Needle "MechLabCapture=open" -Label "mechlab sidecar"
    Assert-Contains -Text $mechLab -Needle "layout=pressure-cards+whole-blocks+single-fillers" -Label "mechlab sidecar"
    Assert-Contains -Text $mechLab -Needle "alwaysMounted=weapons 6/6" -Label "mechlab sidecar"
    Assert-Contains -Text $mechLab -Needle "noToggle=yes" -Label "mechlab sidecar"
}

if ($captures.ContainsKey("damage-demo")) {
    $damageStory = [string]$captures["damage-demo"].damageStory
    Assert-Contains -Text $damageStory -Needle "left-arm-lost" -Label "damage-demo sidecar"
    Assert-Contains -Text $damageStory -Needle "legs-lost" -Label "damage-demo sidecar"
    Assert-Contains -Text $damageStory -Needle "cockpit-lost" -Label "damage-demo sidecar"
    Assert-Contains -Text $damageStory -Needle "pilotRisk=1" -Label "damage-demo sidecar"
}

if ($failures.Count -gt 0) {
    Write-Host "Controlled demo evidence check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) controlled demo evidence check(s) failed."
}

Write-Host "Controlled demo evidence check OK."
Write-Host "Build: $BuildExe"
Write-Host "VisibleFlow: $VisibleFlowLog"
$rows | Format-Table -AutoSize
