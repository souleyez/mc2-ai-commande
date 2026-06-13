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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-readiness-fixes"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd("\", "/")
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-investor-readiness-fixes.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-investor-readiness-fixes.md"
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
$captureCommand = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$routeDoc = Read-RepoText -RelativePath "docs-pc-investor-demo-route-2026-06-13.md"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$currentQueue = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$masterPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RepoText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$gitignore = Read-RepoText -RelativePath ".gitignore"

Assert-All -Text $bootstrap -Label "Mc2DemoBootstrap investor proxy visuals" -Needles @(
    "public string investorProxyVisuals;",
    "lastInvestorProxyVisualSummary",
    "ResetInvestorProxyVisualCounters();",
    "BuildCaptureInvestorProxyVisualSummary()",
    "ApplyInvestorUnitProxyVisual(unit, unitObject);",
    "ApplyInvestorTerrainProxyVisual(terrainObject, prop);",
    "CreateInvestorProxyChild(",
    "unitFallbackProxy=mech-silhouette+vehicle-hull+infantry-fireteam",
    "propFallbackProxy=tree-canopy+building-roof+hardprop-stripe",
    "collision=unchanged pathing=unchanged BattleCore=unchanged publicSafe=proxy-only",
    "mobileLandscapeOnly=True"
)

Assert-All -Text $captureCommand -Label "command evidence debrief summary" -Needles @(
    "Read-FirstAvailableSidecarString",
    '@("debriefRewardSummary", "debriefReward")',
    "Debrief summary",
    "DamageDebrief=section-status+repair-line",
    "damageConsequenceLine=",
    "cockpitEjection=ready",
    "investorProxyVisuals"
)

Assert-All -Text $routeDoc -Label "investor demo route doc" -Needles @(
    "# PC Investor Demo Route",
    '`spawn`',
    '`hangar-contact`',
    '`damage-demo`',
    '`solo-order`',
    '`solo-return`',
    "first phone version is landscape-only",
    "Public/commercial builds need owned, commissioned, or licensed replacement content packs",
    "PC controlled-demo investor readiness fixes check OK."
)

foreach ($doc in @(
    @{ Name = "docs-ai-rts-commander-current-master-plan-2026-06-07.md"; Text = $masterPlan },
    @{ Name = "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"; Text = $detailedPlan },
    @{ Name = "docs-mobile-first-plan-2026-06-10.md"; Text = $mobilePlan }
)) {
    Assert-All -Text $doc.Text -Label "$($doc.Name) F39/F40 plan" -Needles @(
        "F39 implement post-F37 PC controlled-demo investor readiness fixes",
        "PC controlled-demo investor readiness fixes check OK",
        "F40 refresh PC controlled-demo investor-readiness evidence after fixes",
        "Mobile phones remain first-version landscape-only"
    )
}

Assert-All -Text $detailedPlan -Label "detailed queue F39/F40" -Needles @(
    '| F39 | Done | `Implement post-F37 PC controlled-demo investor readiness fixes` |',
    '| F40 | Next | `Refresh PC controlled-demo investor-readiness evidence after fixes` |',
    "docs-pc-investor-demo-route-2026-06-13.md"
)

Assert-All -Text $currentGate -Label "current plan gate F39" -Needles @(
    "check_pc_controlled_demo_investor_readiness_fixes.ps1",
    "PC controlled-demo investor readiness fixes plan OK."
)

Assert-All -Text $currentQueue -Label "current plan queue F39/F40" -Needles @(
    "F39 implement post-F37 PC controlled-demo investor readiness fixes",
    "F40 refresh PC controlled-demo investor-readiness evidence after fixes",
    "check_pc_controlled_demo_investor_readiness_fixes.ps1",
    "PC controlled-demo investor readiness fixes check OK"
)

Assert-All -Text $handoffScript -Label "handoff script F39/F40" -Needles @(
    "F39 implement post-F37 PC controlled-demo investor readiness fixes",
    "check_pc_controlled_demo_investor_readiness_fixes.ps1",
    "PC controlled-demo investor readiness fixes check OK",
    "F40 refresh PC controlled-demo investor-readiness evidence after fixes"
)

Assert-Contains -Text $gitignore -Needle "analysis-output/pc-controlled-demo-investor-readiness-fixes/" -Label ".gitignore F39 output"

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor readiness fixes check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor readiness fixes check(s) failed."
}

if ($PlanOnly) {
    Write-Host "PC controlled-demo investor readiness fixes plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    return
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorReadinessFixes"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F39 implement post-F37 PC controlled-demo investor readiness fixes"
    nextFormalTask = "F40 refresh PC controlled-demo investor-readiness evidence after fixes"
    fixedAreas = @(
        "command evidence debrief sidecar fallback",
        "damage-demo repair and cockpit ejection summary markers",
        "investor proxy visual sidecar field",
        "development-safe fallback unit and prop proxy visuals",
        "one-page PC investor demo route",
        "mobile landscape-only contract preserved"
    )
    checks = $rows
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Readiness Fixes")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F39 implement post-F37 PC controlled-demo investor readiness fixes`')
$markdownLines.Add('Next formal task: `F40 refresh PC controlled-demo investor-readiness evidence after fixes`')
$markdownLines.Add("")
$markdownLines.Add("## Fixed Areas")
foreach ($area in $report.fixedAreas) {
    $markdownLines.Add("- $area")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo investor readiness fixes check OK."
Write-Host "Report: $reportJsonPath"
