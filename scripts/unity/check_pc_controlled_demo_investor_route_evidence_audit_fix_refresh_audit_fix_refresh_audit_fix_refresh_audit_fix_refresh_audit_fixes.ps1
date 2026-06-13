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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([char[]]@("\", "/"))
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "report.json"
$reportMarkdownPath = Join-Path $OutputDir "report.md"
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
    Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$routeDoc = Read-RepoText -RelativePath "docs-pc-investor-demo-route-2026-06-13.md"
$playableEvidence = Read-RepoText -RelativePath "docs-playable-demo-investor-evidence-2026-06-07.md"
$handoff = Read-RepoText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$masterPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RepoText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$pcPlan = Read-RepoText -RelativePath "docs-pc-optimization-plan-2026-06-11.md"
$f59AuditReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit\report.json"
$f59AuditMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit\report.md"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queueScript = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$gitignore = Read-RepoText -RelativePath ".gitignore"

$f59AuditMarkers = @(
    "## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Findings",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAudit=pass-with-followups source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json completed=F59 next=F60 noUnityLaunch=True mobile=landscape-only",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=route-proof-clarity status=pass",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=damage-ejection-proof status=pass",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=mobile-landscape-proof status=pass",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=public-safe-proxy-boundary status=pass",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=audit-fix-refresh-closure status=pass",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFollowUp=P1 area=audit-visibility next=F60-doc-gate-visibility",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFollowUp=P2 area=next-refresh-contract next=F61-consume-F59-audit-report",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFix=F60 visibility=plan+evidence+handoff+gate noUnityLaunch=True mobile=landscape-only next=F61-route-refresh"
)

Assert-All -Text $routeDoc -Label "PC investor route F60 audit-fix visibility" -Needles $f59AuditMarkers
Assert-All -Text $playableEvidence -Label "playable investor evidence F60 audit-fix visibility" -Needles $f59AuditMarkers
Assert-All -Text $handoff -Label "handoff F60 audit-fix visibility" -Needles $f59AuditMarkers

Assert-All -Text $masterPlan -Label "master F60/F61 state" -Needles @(
    "2026-06-13 v111",
    '| 138 | Done | `Implement post-F59 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| 139 | Next | `Refresh PC controlled-demo investor route evidence after F59 audit fixes` |',
    'Formal next task: `F61 refresh PC controlled-demo investor route evidence after F59 audit fixes`',
    "Mobile phones remain landscape-only for the first playable target"
)

Assert-All -Text $detailedPlan -Label "detailed F60/F61 state" -Needles @(
    "2026-06-13 v120",
    '| F60 | Done | `Implement post-F59 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| F61 | Next | `Refresh PC controlled-demo investor route evidence after F59 audit fixes` |',
    'formal next task: `F61 refresh PC controlled-demo investor route evidence after F59 audit fixes`',
    "Mobile phones remain first-version landscape-only"
)

Assert-All -Text $mobilePlan -Label "mobile F60/F61 state" -Needles @(
    "F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes",
    "F61 refresh PC controlled-demo investor route evidence after F59 audit fixes",
    "first phone version is landscape-only",
    "horizontal phone game"
)

Assert-All -Text $pcPlan -Label "PC plan F60/F61 state" -Needles @(
    "F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes",
    "F61 refresh PC controlled-demo investor route evidence after F59 audit fixes",
    "landscape-only phone scope"
)

Assert-All -Text $handoff -Label "handoff F61 next task" -Needles @(
    'Current formal next development task after handoff: `F61 refresh PC controlled-demo investor route evidence after F59 audit fixes`',
    'Next planned work: `F61 refresh PC controlled-demo investor route evidence after F59 audit fixes`',
    "first phone version is landscape-only"
)

if ($null -ne $f59AuditReport) {
    Assert-Equals -Actual ([string]$f59AuditReport.schema) -Expected "PCControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAudit" -Label "F59 audit schema"
    Assert-Equals -Actual ([string]$f59AuditReport.result) -Expected "pass-with-followups" -Label "F59 audit result"
    Assert-Equals -Actual ([string]$f59AuditReport.completedTask) -Expected "F59 audit post-F58 PC controlled-demo investor route evidence refresh" -Label "F59 audit completed task"
    Assert-Equals -Actual ([string]$f59AuditReport.nextFormalTask) -Expected "F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes" -Label "F59 audit next task"
    Assert-Equals -Actual ([bool]$f59AuditReport.noUnityLaunch) -Expected $true -Label "F59 audit no Unity launch"

    foreach ($area in @("route-proof-clarity", "damage-ejection-proof", "mobile-landscape-proof", "public-safe-proxy-boundary", "audit-fix-refresh-closure")) {
        $matches = @($f59AuditReport.findings | Where-Object { [string]$_.area -eq $area -and [string]$_.status -eq "pass" })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "F59 audit finding $area"
    }

    foreach ($area in @("audit-visibility", "next-refresh-contract")) {
        $matches = @($f59AuditReport.followUps | Where-Object { [string]$_.area -eq $area })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "F59 audit follow-up $area"
    }
}

Assert-All -Text $f59AuditMarkdown -Label "F59 audit markdown follow-ups" -Needles @(
    'Completed task: `F59 audit post-F58 PC controlled-demo investor route evidence refresh`',
    'Next formal task: `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes`',
    "route-proof-clarity",
    "damage-ejection-proof",
    "mobile-landscape-proof",
    "public-safe-proxy-boundary",
    "audit-fix-refresh-closure",
    "audit-visibility",
    "next-refresh-contract"
)

Assert-All -Text $currentGate -Label "current gate F60 plan marker" -Needles @(
    "check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1",
    "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes plan OK."
)

Assert-All -Text $queueScript -Label "queue F60/F61 marker" -Needles @(
    "F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes",
    "F61 refresh PC controlled-demo investor route evidence after F59 audit fixes",
    '| F60 | Done | `Implement post-F59 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| F61 | Next | `Refresh PC controlled-demo investor route evidence after F59 audit fixes` |'
)

Assert-All -Text $handoffScript -Label "handoff script F60/F61 marker" -Needles @(
    "check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1",
    "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes check OK",
    "F61 refresh PC controlled-demo investor route evidence after F59 audit fixes"
)

Assert-Contains -Text $gitignore -Needle "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/" -Label ".gitignore F60 output"

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixes"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes"
    nextFormalTask = "F61 refresh PC controlled-demo investor route evidence after F59 audit fixes"
    noUnityLaunch = $true
    sourceAuditReport = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json"
    fixedAreas = @(
        "F59 route audit findings are visible in the investor route doc",
        "F59 route audit findings are visible in the playable investor evidence page",
        "F59 route audit findings are visible in the machine handoff doc",
        "the P1 audit-visibility follow-up is closed by F60 plan/evidence/handoff/gate visibility markers",
        "the P2 next-refresh-contract follow-up now points the next refresh at the F59 audit report and F60 fix report",
        "the first phone version remains landscape-only"
    )
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Route Evidence Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fixes")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes`')
$markdownLines.Add('Next formal task: `F61 refresh PC controlled-demo investor route evidence after F59 audit fixes`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source audit report: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json`')
$markdownLines.Add("")
$markdownLines.Add("## Fixed Areas")
foreach ($area in $report.fixedAreas) {
    $markdownLines.Add("- $area")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes check OK."
Write-Host "Report: $reportJsonPath"
