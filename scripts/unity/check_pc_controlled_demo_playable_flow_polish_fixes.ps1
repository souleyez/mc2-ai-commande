param(
    [string]$RepoRoot = "",
    [string]$OutputDir = "",
    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-playable-flow-polish-fixes"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd("\", "/")
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-playable-flow-polish-fixes.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-playable-flow-polish-fixes.md"
$failures = New-Object System.Collections.Generic.List[string]
$rows = New-Object System.Collections.Generic.List[object]

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Read-RepoText {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        [void]$failures.Add("$RelativePath missing")
        return ""
    }

    return Get-Content -LiteralPath $path -Raw -Encoding UTF8
}

function Assert-Contains {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or -not $Text.Contains($Needle)) {
        [void]$failures.Add("$Label missing marker: $Needle")
        return
    }

    [void]$rows.Add([pscustomobject]@{
        Check = $Label
        Status = "OK"
        Detail = $Needle
    })
}

function Assert-All {
    param(
        [string]$Text,
        [string[]]$Needles,
        [string]$Label
    )

    foreach ($needle in $Needles) {
        Assert-Contains -Text $Text -Needle $needle -Label $Label
    }
}

$bootstrap = Read-RepoText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$captureReference = Read-RepoText -RelativePath "scripts\unity\capture_reference_visuals.ps1"
$captureVisual = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_visual_evidence.ps1"
$captureCommand = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$currentQueue = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$mobileLandscape = Read-RepoText -RelativePath "scripts\unity\check_mobile_landscape_contract.ps1"
$readme = Read-RepoText -RelativePath "README.md"
$buildWin = Read-RepoText -RelativePath "BUILD-WIN.md"
$buildMobile = Read-RepoText -RelativePath "BUILD-MOBILE.md"
$handoff = Read-RepoText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$masterPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RepoText -RelativePath "docs-mobile-first-plan-2026-06-10.md"

Assert-All -Text $bootstrap -Label "Mc2DemoBootstrap playable flow polish" -Needles @(
    'CapturePresetSoloReturn = "solo-return"',
    "RunStartupSoloReturnCapturePrelude()",
    "Solo return settled: unit-2 back in squad",
    "playableFlowPolish = playableFlowPolishSummary",
    "PlayableFlowPolish=contact-pressure+damage-debrief+solo-return+hud-density+handoff",
    "BuildPlayableContactPressureCueSummary()",
    "CompactContactPressureLine()",
    "ContactPressureCue=",
    "objective-panel+in-world",
    "DamageDebrief=section-status+repair-line",
    "MissionResultDamageConsequenceLine",
    "statusRailContext=sections+solo+contact",
    "statusRail=denser-context",
    "PcBattleStatusRailWidth = 280f",
    "MobileTouchUi=ready orientation=landscape",
    "landscapeOnly=yes",
    "Screen.autorotateToPortrait = false;",
    "Screen.autorotateToLandscapeLeft = true;"
)

Assert-All -Text $captureReference -Label "capture_reference_visuals solo-return" -Needles @(
    'elseif ($ExpectedPreset -eq "solo-return")',
    "Test-SoloReturnCaptureSidecar",
    "soloReturn=returned",
    "detached=0",
    "mobileLandscapeOnly=True",
    "orientation=landscape"
)

foreach ($script in @(
    @{ Name = "capture_pc_controlled_demo_visual_evidence.ps1"; Text = $captureVisual },
    @{ Name = "capture_pc_controlled_demo_command_evidence.ps1"; Text = $captureCommand }
)) {
    Assert-All -Text $script.Text -Label "$($script.Name) solo-return evidence" -Needles @(
        '@("spawn", "hangar-contact", "damage-demo", "solo-order", "solo-return")',
        "PlayableFlowPolish=contact-pressure+damage-debrief+solo-return+hud-density+handoff",
        "soloReturn=returned",
        "detached=0",
        "mobileLandscapeOnly=True",
        "Solo return settled"
    )
}

Assert-All -Text $mobileLandscape -Label "mobile landscape contract preserved" -Needles @(
    "defaultScreenOrientation: 3",
    "allowedAutorotateToPortrait: 0",
    "allowedAutorotateToLandscapeRight: 1",
    "MobileTouchUi=ready orientation=landscape",
    "landscapeOnly=yes",
    "horizontal phone game"
)

foreach ($doc in @(
    @{ Name = "README.md"; Text = $readme },
    @{ Name = "BUILD-WIN.md"; Text = $buildWin },
    @{ Name = "BUILD-MOBILE.md"; Text = $buildMobile },
    @{ Name = "docs-machine-handoff-plan-2026-06-07.md"; Text = $handoff },
    @{ Name = "docs-ai-rts-commander-current-master-plan-2026-06-07.md"; Text = $masterPlan },
    @{ Name = "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"; Text = $detailedPlan },
    @{ Name = "docs-mobile-first-plan-2026-06-10.md"; Text = $mobilePlan }
)) {
    Assert-All -Text $doc.Text -Label "$($doc.Name) F36/F37 plan" -Needles @(
        "F36 implement post-F34 PC controlled-demo playable flow polish fixes",
        "F37 refresh PC controlled-demo playable-flow evidence after polish fixes",
        "Mobile phones remain first-version landscape-only",
        "portrait is not a first-slice support target"
    )
}

Assert-All -Text $currentGate -Label "current plan gate F36" -Needles @(
    "check_pc_controlled_demo_playable_flow_polish_fixes.ps1",
    "PC controlled-demo playable flow polish fixes plan OK"
)

Assert-All -Text $currentQueue -Label "current plan queue F36/F37" -Needles @(
    "F36 implement post-F34 PC controlled-demo playable flow polish fixes",
    "F37 refresh PC controlled-demo playable-flow evidence after polish fixes",
    "horizontal phone game"
)

Assert-All -Text $handoffScript -Label "handoff script F36/F37" -Needles @(
    "check_pc_controlled_demo_playable_flow_polish_fixes.ps1",
    "PC controlled-demo playable flow polish fixes check OK",
    "F37 refresh PC controlled-demo playable-flow evidence after polish fixes"
)

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo playable flow polish fixes check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo playable flow polish fixes check(s) failed."
}

if ($PlanOnly) {
    Write-Host "PC controlled-demo playable flow polish fixes plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    return
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoPlayableFlowPolishFixes"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F36 implement post-F34 PC controlled-demo playable flow polish fixes"
    nextFormalTask = "F37 refresh PC controlled-demo playable-flow evidence after polish fixes"
    fixedAreas = @(
        "objective-panel contact pressure cue",
        "damage status and debrief repair consequence line",
        "solo-return capture preset",
        "denser PC status rail",
        "unified playable-flow sidecar field",
        "mobile landscape-only contract preserved"
    )
    checks = $rows
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Playable Flow Polish Fixes")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F36 implement post-F34 PC controlled-demo playable flow polish fixes`')
$markdownLines.Add('Next formal task: `F37 refresh PC controlled-demo playable-flow evidence after polish fixes`')
$markdownLines.Add("")
$markdownLines.Add("## Fixed Areas")
foreach ($area in $report.fixedAreas) {
    $markdownLines.Add("- $area")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo playable flow polish fixes check OK."
Write-Host "Report: $reportJsonPath"
