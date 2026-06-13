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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-evidence-refresh"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([char[]]@("\", "/"))
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-investor-evidence-refresh.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-investor-evidence-refresh.md"
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

function Read-RepoJson {
    param([string]$RelativePath)

    $text = Read-RepoText -RelativePath $RelativePath
    if ([string]::IsNullOrWhiteSpace($text)) {
        return $null
    }

    try {
        return $text | ConvertFrom-Json
    }
    catch {
        [void]$failures.Add("$RelativePath is not valid JSON: $($_.Exception.Message)")
        return $null
    }
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

function Assert-Equals {
    param(
        [object]$Actual,
        [object]$Expected,
        [string]$Label
    )

    if ($Actual -ne $Expected) {
        [void]$failures.Add("$Label expected '$Expected', got '$Actual'")
        return
    }

    [void]$rows.Add([pscustomobject]@{
        check = $Label
        status = "OK"
        detail = [string]$Expected
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
    Write-Host "PC controlled-demo investor evidence refresh plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandCapture = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$commandMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.md"
$commandReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.json"
$f42Fixes = Read-RepoText -RelativePath "scripts\unity\check_pc_controlled_demo_investor_evidence_package_fixes.ps1"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$gitignore = Read-RepoText -RelativePath ".gitignore"

Assert-All -Text $commandCapture -Label "command evidence F43 source" -Needles @(
    'F43 refresh PC controlled-demo investor evidence package after fixes',
    'F44 audit post-F43 PC controlled-demo investor evidence refresh',
    'SkipBuildFreshnessCheck',
    'NoUnityLaunch: $SkipRun'
)

Assert-All -Text $commandMarkdown -Label "command evidence F43 markdown" -Needles @(
    'Completed task: `F43 refresh PC controlled-demo investor evidence package after fixes`',
    'Next formal task: `F44 audit post-F43 PC controlled-demo investor evidence refresh`',
    'InvestorDemoSummary=ready',
    'DamageInvestorCallout=section-loss+cockpit-ejection+wreck-salvage+repair-line',
    'ProxyVisualIdentity=mech-silhouette+vehicle-hull+infantry-fireteam+tree-canopy+building-roof+hardprop-stripe',
    'FastInvestorEvidenceGate=check_pc_controlled_demo_investor_evidence_package_fixes.ps1',
    '| hangar-contact | Contact pressure without extra UI clutter |',
    '| damage-demo | Arm/leg/cockpit consequences, ejection, wreck and repair line |',
    '| solo-order | Status-row single-unit order without drag selection |',
    '| solo-return | Ordered unit automatically returns to squad control |',
    'mobileLandscapeOnly=True'
)

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F43 refresh PC controlled-demo investor evidence package after fixes" -Label "command report completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected "F44 audit post-F43 PC controlled-demo investor evidence refresh" -Label "command report next task"
    Assert-Equals -Actual ([int]$commandReport.width) -Expected 1280 -Label "command report width"
    Assert-Equals -Actual ([int]$commandReport.height) -Expected 720 -Label "command report height"
    Assert-Equals -Actual (@($commandReport.evidence).Count) -Expected 5 -Label "command report preset count"

    foreach ($preset in @("spawn", "hangar-contact", "damage-demo", "solo-order", "solo-return")) {
        $match = @($commandReport.evidence | Where-Object { [string]$_.preset -eq $preset })
        Assert-Equals -Actual $match.Count -Expected 1 -Label "command report preset $preset"
    }
}

Assert-All -Text $f42Fixes -Label "F42 fixes remain referenced by F43 refresh" -Needles @(
    'PC controlled-demo investor evidence package fixes check OK.',
    'mobile landscape-only boundary preserved',
    'FastInvestorEvidenceGate=check_pc_controlled_demo_investor_evidence_package_fixes.ps1'
)

Assert-All -Text $currentGate -Label "current gate F43 fast plan" -Needles @(
    "check_pc_controlled_demo_investor_evidence_refresh.ps1",
    "PC controlled-demo investor evidence refresh plan OK."
)

Assert-Contains -Text $gitignore -Needle "analysis-output/pc-controlled-demo-investor-evidence-refresh/" -Label ".gitignore F43 output"

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor evidence refresh check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor evidence refresh check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorEvidenceRefresh"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F43 refresh PC controlled-demo investor evidence package after fixes"
    nextFormalTask = "F44 audit post-F43 PC controlled-demo investor evidence refresh"
    sourceCommandEvidenceReport = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json"
    refreshedAreas = @(
        "command evidence metadata advanced to F43/F44",
        "executive investor summary present in markdown",
        "preset highlights present for contact, damage, solo-order and solo-return",
        "damage/ejection/debrief investor callout represented",
        "public-safe proxy visual identity represented",
        "mobile landscape-only boundary preserved"
    )
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Evidence Refresh")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F43 refresh PC controlled-demo investor evidence package after fixes`')
$markdownLines.Add('Next formal task: `F44 audit post-F43 PC controlled-demo investor evidence refresh`')
$markdownLines.Add("")
$markdownLines.Add("## Refreshed Areas")
foreach ($area in $report.refreshedAreas) {
    $markdownLines.Add("- $area")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo investor evidence refresh check OK."
Write-Host "Report: $reportJsonPath"
