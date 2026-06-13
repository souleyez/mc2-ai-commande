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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-command-readability-fixes"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd("\", "/")
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-command-readability-fixes.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-command-readability-fixes.md"
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
        $failures.Add("$RelativePath missing")
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
        $failures.Add("$Label missing marker: $Needle")
        return
    }

    $rows.Add([pscustomobject]@{
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
$mission = Read-RepoText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\BattleMission.cs"
$captureReference = Read-RepoText -RelativePath "scripts\unity\capture_reference_visuals.ps1"
$captureEvidence = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_visual_evidence.ps1"
$visualFixes = Read-RepoText -RelativePath "scripts\unity\check_pc_controlled_demo_visual_readability_fixes.ps1"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$currentQueue = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$masterPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$readme = Read-RepoText -RelativePath "README.md"
$buildWin = Read-RepoText -RelativePath "BUILD-WIN.md"
$handoff = Read-RepoText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"

Assert-All -Text $mission -Label "BattleMission formation spacing" -Needles @(
    "SquadMoveFormationSpacing = 320f",
    "SquadAttackFormationSpacing = 340f"
)

Assert-All -Text $bootstrap -Label "Mc2DemoBootstrap command readability" -Needles @(
    'CapturePresetSoloOrder = "solo-order"',
    "RunStartupSoloOrderCapturePrelude()",
    'RunStartupCommanderCommand("unit unit-2 move 3136 -1700")',
    "commandReadability = BuildCaptureCommandReadabilitySummary()",
    "commandCuePalette = CommandCuePaletteSummary()",
    "commanderFollow = BuildCaptureCommanderFollowSummary()",
    "CommandReadability=all+single+jet+focus+commander-follow+formation",
    "formation=move-320+attack-340",
    "CommandCuePalette=command-blue+target-red+damage-amber+hostile-magenta",
    "CommanderFollow=unit-1+first-sort+fixed-view",
    "cuePalette=command-blue target-red damage-amber hostile-magenta pilot-cyan",
    "new Color(0.20f, 0.82f, 1f, 0.58f)",
    "new Color(0.64f, 0.30f, 1f, 0.22f)"
)

Assert-All -Text $captureReference -Label "capture_reference_visuals solo-order preset" -Needles @(
    'elseif ($ExpectedPreset -eq "solo-order")',
    "Test-SoloOrderCaptureSidecar",
    "solo=1",
    "CommandReadability=all+single+jet+focus+commander-follow+formation",
    "CommanderFollow=unit-1+first-sort+fixed-view",
    "CommandCuePalette=command-blue+target-red+damage-amber+hostile-magenta"
)

Assert-All -Text $captureEvidence -Label "PC visual evidence solo-order preset" -Needles @(
    '@("spawn", "hangar-contact", "damage-demo", "solo-order")',
    '$requiredPresets = @("spawn", "hangar-contact", "damage-demo", "solo-order")',
    "solo=1",
    "SoloOrder=ring+beacon",
    "SoloReturn=ring+beacon",
    "CommandCuePalette=command-blue+target-red+damage-amber+hostile-magenta"
)

Assert-Contains -Text $visualFixes -Needle "cuePalette=command-blue target-red damage-amber hostile-magenta pilot-cyan" -Label "F30 visual fixes palette"

foreach ($doc in @(
    @{ Name = "README.md"; Text = $readme },
    @{ Name = "BUILD-WIN.md"; Text = $buildWin },
    @{ Name = "docs-machine-handoff-plan-2026-06-07.md"; Text = $handoff },
    @{ Name = "docs-ai-rts-commander-current-master-plan-2026-06-07.md"; Text = $masterPlan },
    @{ Name = "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"; Text = $detailedPlan }
)) {
    Assert-Contains -Text $doc.Text -Needle "F33 implement PC controlled-demo command readability and formation fixes" -Label "$($doc.Name) F33"
    Assert-Contains -Text $doc.Text -Needle "F34 refresh PC controlled-demo command evidence after readability fixes" -Label "$($doc.Name) F34"
}

Assert-All -Text $currentGate -Label "current plan gate F33" -Needles @(
    "check_pc_controlled_demo_command_readability_fixes.ps1",
    "PC controlled-demo command readability fixes plan OK"
)

Assert-All -Text $currentQueue -Label "current plan queue F33/F34" -Needles @(
    "F33 implement PC controlled-demo command readability and formation fixes",
    "F34 refresh PC controlled-demo command evidence after readability fixes",
    "horizontal phone game"
)

Assert-All -Text $handoffScript -Label "handoff script F33/F34" -Needles @(
    "F33 implement PC controlled-demo command readability and formation fixes",
    "F34 refresh PC controlled-demo command evidence after readability fixes",
    "check_pc_controlled_demo_command_readability_fixes.ps1",
    "PC controlled-demo command readability fixes check OK"
)

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo command readability fixes check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo command readability fixes check(s) failed."
}

if ($PlanOnly) {
    Write-Host "PC controlled-demo command readability fixes plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    return
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoCommandReadabilityFixes"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F33 implement PC controlled-demo command readability and formation fixes"
    nextFormalTask = "F34 refresh PC controlled-demo command evidence after readability fixes"
    fixedAreas = @(
        "widened player squad formation spacing",
        "added command readability sidecar fields",
        "added commander follow sidecar summary",
        "added solo-order capture preset",
        "separated command, target, damage and hostile pressure cue palette"
    )
    checks = $rows
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Command Readability Fixes")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F33 implement PC controlled-demo command readability and formation fixes`')
$markdownLines.Add('Next formal task: `F34 refresh PC controlled-demo command evidence after readability fixes`')
$markdownLines.Add("")
$markdownLines.Add("## Fixed Areas")
foreach ($area in $report.fixedAreas) {
    $markdownLines.Add("- $area")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo command readability fixes check OK."
Write-Host "Report: $reportJsonPath"
