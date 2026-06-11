param(
    [string]$RepoRoot = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

$failures = New-Object System.Collections.Generic.List[string]
$rows = New-Object System.Collections.Generic.List[object]

function Add-Failure {
    param([string]$Message)
    [void]$failures.Add($Message)
}

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot $RelativePath
}

function Read-RequiredText {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path)) {
        Add-Failure "Missing file: $RelativePath"
        return ""
    }

    return Get-Content -LiteralPath $path -Raw
}

function Get-TextSlice {
    param(
        [string]$Text,
        [string]$StartMarker,
        [string]$EndMarker,
        [string]$Label
    )

    $start = $Text.IndexOf($StartMarker, [StringComparison]::Ordinal)
    if ($start -lt 0) {
        Add-Failure "$Label missing start marker: $StartMarker"
        return ""
    }

    $end = $Text.IndexOf($EndMarker, $start + $StartMarker.Length, [StringComparison]::Ordinal)
    if ($end -lt 0) {
        Add-Failure "$Label missing end marker: $EndMarker"
        return $Text.Substring($start)
    }

    return $Text.Substring($start, $end - $start)
}

function Assert-Contains {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or $Text.IndexOf($Needle, [StringComparison]::Ordinal) -lt 0) {
        Add-Failure "$Label missing marker: $Needle"
    }
}

function Add-Row {
    param(
        [string]$Check,
        [string]$Detail
    )

    [void]$rows.Add([pscustomobject]@{
        Check = $Check
        Status = "OK"
        Detail = $Detail
    })
}

$bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$captureScript = Read-RequiredText -RelativePath "scripts\unity\capture_reference_visuals.ps1"
$mobileCommandScript = Read-RequiredText -RelativePath "scripts\unity\check_mobile_command_model_preflight.ps1"

$captureSummary = Get-TextSlice `
    -Text $bootstrap `
    -StartMarker "private string BuildCaptureBattleHudSummary()" `
    -EndMarker "private string BuildSparseBattleUiRegressionSummary()" `
    -Label "BuildCaptureBattleHudSummary"

$sparseSummary = Get-TextSlice `
    -Text $bootstrap `
    -StartMarker "private string BuildSparseBattleUiRegressionSummary()" `
    -EndMarker "private static bool SparseBattleUiRegressionSummaryOk(string summary)" `
    -Label "BuildSparseBattleUiRegressionSummary"

$sparseOk = Get-TextSlice `
    -Text $bootstrap `
    -StartMarker "private static bool SparseBattleUiRegressionSummaryOk(string summary)" `
    -EndMarker "private static string LoadoutCaptureFillerSummary" `
    -Label "SparseBattleUiRegressionSummaryOk"

$captureGate = Get-TextSlice `
    -Text $captureScript `
    -StartMarker "function Test-BattleHudCaptureSidecar" `
    -EndMarker "function Test-TerrainReadabilityCaptureSidecar" `
    -Label "Test-BattleHudCaptureSidecar"

$requiredRuntimeMarkers = @(
    "BattleHud=active controls=statusRows+jet+map+bay+system",
    "combatLogVisible=no",
    "SparseBattleUi=statusRows+sections+solo",
    "controls=all+jet+map+bay+system",
    "combatLog=hidden",
    "objective=compactObjective",
    "missionMap=available-closed",
    "accountUi=hidden",
    "economyUi=funds-only",
    "saveUi=disabled",
    "debugOccupancy=sidecar-only",
    "overlays=hidden"
)

foreach ($marker in @(
    "BattleHud=active controls=statusRows+jet+map+bay+system",
    "combatPanel=h",
    "combatLogVisible=no",
    "objectivePanel=",
    "missionMap=",
    "saveUi=",
    "BuildSparseBattleUiRegressionSummary()"
)) {
    Assert-Contains -Text $captureSummary -Needle $marker -Label "BuildCaptureBattleHudSummary"
}
Add-Row -Check "capture HUD summary source" -Detail "7 marker(s)"

foreach ($marker in @(
    "SparseBattleUi=statusRows+sections+solo",
    "controls=all+jet+map+bay+system",
    "combatPanel=h",
    "combatLog=hidden",
    "objective=",
    "available-closed",
    "accountUi=hidden",
    "economyUi=funds-only",
    "saveUi=",
    "debugOccupancy=sidecar-only",
    "overlays="
)) {
    Assert-Contains -Text $sparseSummary -Needle $marker -Label "BuildSparseBattleUiRegressionSummary"
}
Add-Row -Check "sparse HUD summary source" -Detail "11 marker(s)"

$okPredicateMarkers = @($requiredRuntimeMarkers | Where-Object { $_ -notlike "BattleHud=active*" -and $_ -ne "combatLogVisible=no" })
foreach ($marker in $okPredicateMarkers) {
    Assert-Contains -Text $sparseOk -Needle $marker -Label "SparseBattleUiRegressionSummaryOk"
}
Add-Row -Check "sparse HUD OK predicate" -Detail "10 marker(s)"

$captureGateMarkers = @(
    "BattleHud=active",
    "controls=statusRows+jet+map+bay+system",
    "combatLogVisible=no",
    "SparseBattleUi=statusRows+sections+solo",
    "controls=all+jet+map+bay+system",
    "combatLog=hidden",
    "objective=compactObjective",
    "missionMap=available-closed",
    "accountUi=hidden",
    "economyUi=funds-only",
    "saveUi=disabled",
    "debugOccupancy=sidecar-only",
    "overlays=hidden"
)

foreach ($marker in $captureGateMarkers) {
    Assert-Contains -Text $captureGate -Needle $marker -Label "capture battle HUD gate"
}

foreach ($forbidden in @(
    "combatLogVisible=yes",
    "combatLog=visible",
    "saveUi=enabled",
    "accountUi=visible",
    "debugOccupancy=visible",
    "overlays=visible"
)) {
    Assert-Contains -Text $captureGate -Needle $forbidden -Label "capture forbidden HUD gate"
}

Assert-Contains -Text $captureGate -Needle "combatPanel=h([0-9.]+)" -Label "capture combat panel height gate"
Assert-Contains -Text $captureGate -Needle "84.0" -Label "capture combat panel height gate"
Add-Row -Check "capture sparse HUD gate" -Detail "required/forbidden/height"

$mobileHudMarkers = @($requiredRuntimeMarkers | Where-Object { $_ -ne "combatLogVisible=no" -and $_ -ne "economyUi=funds-only" -and $_ -ne "debugOccupancy=sidecar-only" })
foreach ($marker in $mobileHudMarkers) {
    Assert-Contains -Text $mobileCommandScript -Needle $marker -Label "mobile command model preflight"
}
Add-Row -Check "mobile command HUD markers" -Detail "shares sparse contract"

if ($failures.Count -gt 0) {
    Write-Host "Battle HUD sparse contract check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) battle HUD sparse contract check(s) failed."
}

Write-Host "Battle HUD sparse contract check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
