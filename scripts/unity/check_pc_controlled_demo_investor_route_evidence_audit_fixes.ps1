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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fixes"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([char[]]@("\", "/"))
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-investor-route-evidence-audit-fixes.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-investor-route-evidence-audit-fixes.md"
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

if ($PlanOnly) {
    Write-Host "PC controlled-demo investor route evidence audit fixes plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$routeDoc = Read-RepoText -RelativePath "docs-pc-investor-demo-route-2026-06-13.md"
$playableEvidence = Read-RepoText -RelativePath "docs-playable-demo-investor-evidence-2026-06-07.md"
$handoff = Read-RepoText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$auditReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-refresh-audit\pc-controlled-demo-investor-route-evidence-refresh-audit.json"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queueScript = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$gitignore = Read-RepoText -RelativePath ".gitignore"

$routeAuditMarkers = @(
    "## Route Audit Findings",
    "RouteAudit=pass-with-followups source=analysis-output/pc-controlled-demo-investor-route-evidence-refresh-audit/pc-controlled-demo-investor-route-evidence-refresh-audit.json",
    "RouteAuditFinding=route-evidence-envelope status=pass",
    "RouteAuditFinding=presentation-route status=pass",
    "RouteAuditFinding=damage-proof status=pass",
    "RouteAuditFinding=mobile-landscape-proof status=pass",
    "RouteAuditFinding=public-safe-proxy-boundary status=pass",
    "RouteAuditFollowUp=P1 area=audit-fixes next=F48-doc-visibility",
    "RouteAuditFollowUp=P2 area=gate-runtime next=keep-route-gates-focused",
    "RouteAuditFix=F48 visibility=investor-route+playable-evidence+handoff noUnityLaunch=True mobile=landscape-only next=F49-route-refresh"
)

Assert-All -Text $routeDoc -Label "PC investor route F48 audit visibility" -Needles $routeAuditMarkers
Assert-All -Text $playableEvidence -Label "playable investor evidence F48 audit visibility" -Needles $routeAuditMarkers
Assert-All -Text $handoff -Label "handoff F48 audit visibility" -Needles $routeAuditMarkers

Assert-All -Text $handoff -Label "handoff F48/F49 task state" -Needles @(
    'Current formal next development task after handoff: `F50 audit post-F49 PC controlled-demo investor route evidence refresh`',
    'Next planned work: `F50 audit post-F49 PC controlled-demo investor route evidence refresh`',
    "F48 implement post-F47 PC controlled-demo investor route evidence audit fixes",
    "check_pc_controlled_demo_investor_route_evidence_audit_fixes.ps1",
    "PC controlled-demo investor route evidence audit fixes check OK"
)

if ($null -ne $auditReport) {
    Assert-Equals -Actual ([string]$auditReport.result) -Expected "pass-with-followups" -Label "F47 route audit result"
    Assert-Equals -Actual ([string]$auditReport.completedTask) -Expected "F47 audit post-F46 PC controlled-demo investor route evidence refresh" -Label "F47 route audit completed task"
    Assert-Equals -Actual ([string]$auditReport.nextFormalTask) -Expected "F48 implement post-F47 PC controlled-demo investor route evidence audit fixes" -Label "F47 route audit next task"

    foreach ($area in @("route-evidence-envelope", "presentation-route", "damage-proof", "mobile-landscape-proof", "public-safe-proxy-boundary")) {
        $matches = @($auditReport.findings | Where-Object { [string]$_.area -eq $area -and [string]$_.status -eq "pass" })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "F47 route audit finding $area"
    }

    foreach ($area in @("audit-fixes", "gate-runtime")) {
        $matches = @($auditReport.followUps | Where-Object { [string]$_.area -eq $area })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "F47 route audit follow-up $area"
    }
}

Assert-All -Text $currentGate -Label "current gate F48 plan marker" -Needles @(
    "check_pc_controlled_demo_investor_route_evidence_audit_fixes.ps1",
    "PC controlled-demo investor route evidence audit fixes plan OK."
)

Assert-All -Text $queueScript -Label "queue F48/F49 marker" -Needles @(
    "F48 implement post-F47 PC controlled-demo investor route evidence audit fixes",
    "F49 refresh PC controlled-demo investor route evidence after audit fixes",
    '| F48 | Done | `Implement post-F47 PC controlled-demo investor route evidence audit fixes` |',
    '| F49 | Next | `Refresh PC controlled-demo investor route evidence after audit fixes` |'
)

Assert-All -Text $handoffScript -Label "handoff script F48/F49 marker" -Needles @(
    "check_pc_controlled_demo_investor_route_evidence_audit_fixes.ps1",
    "PC controlled-demo investor route evidence audit fixes check OK",
    "F49 refresh PC controlled-demo investor route evidence after audit fixes"
)

Assert-Contains -Text $gitignore -Needle "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fixes/" -Label ".gitignore F48 output"

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor route evidence audit fixes check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor route evidence audit fixes check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorRouteEvidenceAuditFixes"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F48 implement post-F47 PC controlled-demo investor route evidence audit fixes"
    nextFormalTask = "F49 refresh PC controlled-demo investor route evidence after audit fixes"
    noUnityLaunch = $true
    fixedAreas = @(
        "F47 route-audit findings are visible in the investor route doc",
        "F47 route-audit findings are visible in the playable investor evidence page",
        "F47 route-audit findings are visible in the machine handoff doc",
        "the P1 audit-fixes follow-up is closed by F48 doc visibility markers",
        "the P2 gate-runtime follow-up stays scoped to the route evidence path",
        "the first phone version remains landscape-only"
    )
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Route Evidence Audit Fixes")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F48 implement post-F47 PC controlled-demo investor route evidence audit fixes`')
$markdownLines.Add('Next formal task: `F49 refresh PC controlled-demo investor route evidence after audit fixes`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add("## Fixed Areas")
foreach ($area in $report.fixedAreas) {
    $markdownLines.Add("- $area")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo investor route evidence audit fixes check OK."
Write-Host "Report: $reportJsonPath"
