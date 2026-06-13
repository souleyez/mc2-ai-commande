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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-evidence-package-fixes"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([char[]]@("\", "/"))
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-investor-evidence-package-fixes.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-investor-evidence-package-fixes.md"
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
        check = $Label
        status = "OK"
        detail = $Needle
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

if ($PlanOnly) {
    Write-Host "PC controlled-demo investor evidence package fixes plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$bootstrap = Read-RepoText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$commandCapture = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$commandMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.md"
$f41Audit = Read-RepoText -RelativePath "scripts\unity\audit_pc_controlled_demo_investor_evidence_package.ps1"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$gitignore = Read-RepoText -RelativePath ".gitignore"

Assert-All -Text $bootstrap -Label "Mc2DemoBootstrap F42 investor callout source" -Needles @(
    "public string damageInvestorCallout;",
    "BuildCaptureDamageInvestorCalloutSummary(",
    "DamageInvestorCallout=section-loss+cockpit-ejection+wreck-salvage+repair-line",
    "cockpitEjection=hero-callout",
    "repairConsequence=debrief-line",
    "salvageWreck=visible-marker",
    "proxyIdentity=role-silhouette+faction-color+scale-language",
    "materialLanguage=painted-panels+emissive-cockpit+weapon-hardpoints",
    "propIdentity=canopy+roof+hazard-stripe",
    "publicSafe=proxy-only",
    "mobileLandscapeOnly=True"
)

Assert-All -Text $commandCapture -Label "command evidence F42 summary source" -Needles @(
    "## Executive Summary",
    "InvestorDemoSummary=ready",
    "DamageInvestorCallout=section-loss+cockpit-ejection+wreck-salvage+repair-line",
    "ProxyVisualIdentity=mech-silhouette+vehicle-hull+infantry-fireteam+tree-canopy+building-roof+hardprop-stripe",
    "FastInvestorEvidenceGate=check_pc_controlled_demo_investor_evidence_package_fixes.ps1",
    "## Preset Highlights",
    "## Raw Evidence",
    "Escape-MarkdownTableCell",
    "Find-EvidenceRow"
)

Assert-All -Text $commandMarkdown -Label "command evidence F42 investor markdown" -Needles @(
    "## Executive Summary",
    "InvestorDemoSummary=ready",
    "DamageInvestorCallout=section-loss+cockpit-ejection+wreck-salvage+repair-line",
    "ProxyVisualIdentity=mech-silhouette+vehicle-hull+infantry-fireteam+tree-canopy+building-roof+hardprop-stripe",
    "FastInvestorEvidenceGate=check_pc_controlled_demo_investor_evidence_package_fixes.ps1",
    "## Preset Highlights",
    "| hangar-contact | Contact pressure without extra UI clutter |",
    "| damage-demo | Arm/leg/cockpit consequences, ejection, wreck and repair line |",
    "| solo-order | Status-row single-unit order without drag selection |",
    "| solo-return | Ordered unit automatically returns to squad control |",
    "## Raw Evidence"
)

Assert-All -Text $f41Audit -Label "F41 audit keeps F42 evidence package coverage" -Needles @(
    "mobile-landscape-boundary",
    "damage-debrief-story",
    "publicSafe=proxy-only",
    "PC controlled-demo investor evidence package audit OK"
)

Assert-All -Text $currentGate -Label "current gate F42 fast gate" -Needles @(
    "check_pc_controlled_demo_investor_evidence_package_fixes.ps1",
    "PC controlled-demo investor evidence package fixes plan OK."
)

Assert-Contains -Text $gitignore -Needle "analysis-output/pc-controlled-demo-investor-evidence-package-fixes/" -Label ".gitignore F42 output"

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor evidence package fixes check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor evidence package fixes check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorEvidencePackageFixes"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F42 implement post-F41 PC controlled-demo investor evidence package fixes"
    nextFormalTask = "F43 refresh PC controlled-demo investor evidence package after fixes"
    fixedAreas = @(
        "compact executive summary before the raw command evidence table",
        "preset highlights for contact, damage, solo-order and solo-return",
        "damage/ejection/debrief investor callout source marker",
        "public-safe proxy identity and material language markers",
        "fast investor-evidence gate independent from heavy aggregate validation",
        "mobile landscape-only boundary preserved"
    )
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Evidence Package Fixes")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F42 implement post-F41 PC controlled-demo investor evidence package fixes`')
$markdownLines.Add('Next formal task: `F43 refresh PC controlled-demo investor evidence package after fixes`')
$markdownLines.Add("")
$markdownLines.Add("## Fixed Areas")
foreach ($area in $report.fixedAreas) {
    $markdownLines.Add("- $area")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo investor evidence package fixes check OK."
Write-Host "Report: $reportJsonPath"
