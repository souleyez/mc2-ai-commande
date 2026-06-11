param(
    [string]$RepoRoot = "",
    [string]$CaptureDir = ""
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

function Assert-FileContains {
    param(
        [string]$RelativePath,
        [string[]]$Markers
    )

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Assert-FileExists -Path $path -Label $RelativePath)) {
        return
    }

    $text = Get-Content -LiteralPath $path -Raw
    foreach ($marker in $Markers) {
        Assert-Contains -Text $text -Needle $marker -Label $RelativePath
    }

    [void]$rows.Add([pscustomobject]@{
        Check = $RelativePath
        Status = "OK"
        Detail = "$($Markers.Count) marker(s)"
    })
}

function Read-CaptureSidecar {
    param([string]$Preset)

    $jsonPath = Join-Path $CaptureDir "$Preset.json"
    if (-not (Assert-FileExists -Path $jsonPath -Label "$Preset sidecar")) {
        return $null
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

    [void]$rows.Add([pscustomobject]@{
        Check = "$Preset sidecar"
        Status = "OK"
        Detail = "loaded"
    })

    return $json
}

$battlePresets = @("spawn", "airfield", "hangar-contact", "damage-demo", "north-patrol")
$battleHudMarkers = @(
    "BattleHud=active",
    "controls=statusRows+jet+map+bay+system",
    "SparseBattleUi=statusRows+sections+solo",
    "controls=all+jet+map+bay+system",
    "combatLog=hidden",
    "objective=compactObjective",
    "missionMap=available-closed",
    "accountUi=hidden",
    "saveUi=disabled",
    "overlays=hidden"
)

foreach ($preset in $battlePresets) {
    $capture = Read-CaptureSidecar -Preset $preset
    if ($null -eq $capture) {
        continue
    }

    $battleHud = [string]$capture.battleHud
    foreach ($marker in $battleHudMarkers) {
        Assert-Contains -Text $battleHud -Needle $marker -Label "$preset battle HUD"
    }

    [void]$rows.Add([pscustomobject]@{
        Check = "$preset battle command model"
        Status = "OK"
        Detail = "$($battleHudMarkers.Count) marker(s)"
    })
}

$mechLab = Read-CaptureSidecar -Preset "mechlab"
if ($null -ne $mechLab) {
    $mechLabSummary = [string]$mechLab.mechLab
    $mechLabMarkers = @(
        "MechLabCapture=open",
        "layout=pressure-cards+whole-blocks+single-fillers",
        "alwaysMounted=weapons 6/6",
        "noToggle=yes"
    )

    foreach ($marker in $mechLabMarkers) {
        Assert-Contains -Text $mechLabSummary -Needle $marker -Label "mechlab command model"
    }

    [void]$rows.Add([pscustomobject]@{
        Check = "mechlab low-complexity fitting"
        Status = "OK"
        Detail = "$($mechLabMarkers.Count) marker(s)"
    })
}

Assert-FileContains -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs" -Markers @(
    "BattleHud=active controls=statusRows+jet+map+bay+system",
    "SparseBattleUi=statusRows+sections+solo",
    "controls=all+jet+map+bay+system",
    "noToggle="
)

Assert-FileContains -RelativePath "README.md" -Markers @(
    "PC1-PC30",
    "check_mobile_command_model_preflight.ps1"
)

Assert-FileContains -RelativePath "BUILD-WIN.md" -Markers @(
    "check_mobile_command_model_preflight.ps1",
    "Mobile command model preflight OK"
)

Assert-FileContains -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md" -Markers @(
    "PC1-PC30",
    "Add mobile command model preflight",
    "check_mobile_command_model_preflight.ps1"
)

Assert-FileContains -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md" -Markers @(
    "PC1-PC30",
    "PC12: Add Mobile Command Model Preflight",
    "No drag-box selection.",
    "check_mobile_command_model_preflight.ps1"
)

if ($failures.Count -gt 0) {
    Write-Host "Mobile command model preflight failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) mobile command model preflight check(s) failed."
}

Write-Host "Mobile command model preflight OK."
Write-Host "Repo: $RepoRoot"
Write-Host "CaptureDir: $CaptureDir"
$rows | Format-Table -AutoSize
