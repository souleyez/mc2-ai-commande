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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fixes"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([char[]]@("\", "/"))
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fixes.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fixes.md"
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
    Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes plan OK."
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
$auditReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit.json"
$auditMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit.md"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queueScript = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$gitignore = Read-RepoText -RelativePath ".gitignore"

$f53AuditMarkers = @(
    "## Route Audit Fix Refresh Audit Fix Refresh Audit Findings",
    "RouteAuditFixRefreshAuditFixRefreshAudit=pass-with-followups source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit.json completed=F53 next=F54 noUnityLaunch=True mobile=landscape-only",
    "RouteAuditFixRefreshAuditFixRefreshAuditFinding=route-proof-clarity status=pass",
    "RouteAuditFixRefreshAuditFixRefreshAuditFinding=damage-ejection-proof status=pass",
    "RouteAuditFixRefreshAuditFixRefreshAuditFinding=mobile-landscape-proof status=pass",
    "RouteAuditFixRefreshAuditFixRefreshAuditFinding=public-safe-proxy-boundary status=pass",
    "RouteAuditFixRefreshAuditFixRefreshAuditFinding=audit-fix-refresh-closure status=pass",
    "RouteAuditFixRefreshAuditFixRefreshAuditFollowUp=P1 area=audit-visibility next=F54-doc-gate-visibility",
    "RouteAuditFixRefreshAuditFixRefreshAuditFollowUp=P2 area=next-refresh-contract next=F55-consume-F53-audit-report",
    "RouteAuditFixRefreshAuditFixRefreshAuditFix=F54 visibility=plan+evidence+handoff+gate noUnityLaunch=True mobile=landscape-only next=F55-route-refresh"
)

Assert-All -Text $routeDoc -Label "PC investor route F54 audit-fix visibility" -Needles $f53AuditMarkers
Assert-All -Text $playableEvidence -Label "playable investor evidence F54 audit-fix visibility" -Needles $f53AuditMarkers
Assert-All -Text $handoff -Label "handoff F54 audit-fix visibility" -Needles $f53AuditMarkers

Assert-All -Text $masterPlan -Label "master F54/F55 state" -Needles @(
    "2026-06-13 v105",
    '| 132 | Done | `Implement post-F53 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| 133 | Next | `Refresh PC controlled-demo investor route evidence after F53 audit fixes` |',
    'Formal next task: `F55 refresh PC controlled-demo investor route evidence after F53 audit fixes`',
    "Mobile phones remain landscape-only for the first playable target"
)

Assert-All -Text $detailedPlan -Label "detailed F54/F55 state" -Needles @(
    "2026-06-13 v114",
    '| F54 | Done | `Implement post-F53 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| F55 | Next | `Refresh PC controlled-demo investor route evidence after F53 audit fixes` |',
    'formal next task: `F55 refresh PC controlled-demo investor route evidence after F53 audit fixes`',
    "Mobile phones remain first-version landscape-only"
)

Assert-All -Text $mobilePlan -Label "mobile F54/F55 state" -Needles @(
    "F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes",
    "F55 refresh PC controlled-demo investor route evidence after F53 audit fixes",
    "first phone version is landscape-only",
    "horizontal phone game"
)

Assert-All -Text $pcPlan -Label "PC plan F54/F55 state" -Needles @(
    "F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes",
    "F55 refresh PC controlled-demo investor route evidence after F53 audit fixes",
    "landscape-only phone scope"
)

Assert-All -Text $handoff -Label "handoff F55 next task" -Needles @(
    'Current formal next development task after handoff: `F55 refresh PC controlled-demo investor route evidence after F53 audit fixes`',
    'Next planned work: `F55 refresh PC controlled-demo investor route evidence after F53 audit fixes`',
    "first phone version is landscape-only"
)

if ($null -ne $auditReport) {
    Assert-Equals -Actual ([string]$auditReport.schema) -Expected "PCControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAudit" -Label "F53 audit schema"
    Assert-Equals -Actual ([string]$auditReport.result) -Expected "pass-with-followups" -Label "F53 audit result"
    Assert-Equals -Actual ([string]$auditReport.completedTask) -Expected "F53 audit post-F52 PC controlled-demo investor route evidence refresh" -Label "F53 audit completed task"
    Assert-Equals -Actual ([string]$auditReport.nextFormalTask) -Expected "F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes" -Label "F53 audit next task"
    Assert-Equals -Actual ([bool]$auditReport.noUnityLaunch) -Expected $true -Label "F53 audit no Unity launch"

    foreach ($area in @("route-proof-clarity", "damage-ejection-proof", "mobile-landscape-proof", "public-safe-proxy-boundary", "audit-fix-refresh-closure")) {
        $matches = @($auditReport.findings | Where-Object { [string]$_.area -eq $area -and [string]$_.status -eq "pass" })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "F53 audit finding $area"
    }

    foreach ($area in @("audit-visibility", "next-refresh-contract")) {
        $matches = @($auditReport.followUps | Where-Object { [string]$_.area -eq $area })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "F53 audit follow-up $area"
    }
}

Assert-All -Text $auditMarkdown -Label "F53 audit markdown follow-ups" -Needles @(
    'Completed task: `F53 audit post-F52 PC controlled-demo investor route evidence refresh`',
    'Next formal task: `F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes`',
    "route-proof-clarity",
    "damage-ejection-proof",
    "mobile-landscape-proof",
    "public-safe-proxy-boundary",
    "audit-fix-refresh-closure",
    "audit-visibility",
    "next-refresh-contract"
)

Assert-All -Text $currentGate -Label "current gate F54 plan marker" -Needles @(
    "check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1",
    "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes plan OK."
)

Assert-All -Text $queueScript -Label "queue F54/F55 marker" -Needles @(
    "F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes",
    "F55 refresh PC controlled-demo investor route evidence after F53 audit fixes",
    '| F54 | Done | `Implement post-F53 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| F55 | Next | `Refresh PC controlled-demo investor route evidence after F53 audit fixes` |'
)

Assert-All -Text $handoffScript -Label "handoff script F54/F55 marker" -Needles @(
    "check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1",
    "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes check OK",
    "F55 refresh PC controlled-demo investor route evidence after F53 audit fixes"
)

Assert-Contains -Text $gitignore -Needle "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fixes/" -Label ".gitignore F54 output"

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixes"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes"
    nextFormalTask = "F55 refresh PC controlled-demo investor route evidence after F53 audit fixes"
    noUnityLaunch = $true
    sourceAuditReport = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit.json"
    fixedAreas = @(
        "F53 route audit findings are visible in the investor route doc",
        "F53 route audit findings are visible in the playable investor evidence page",
        "F53 route audit findings are visible in the machine handoff doc",
        "the P1 audit-visibility follow-up is closed by F54 plan/evidence/handoff/gate visibility markers",
        "the P2 next-refresh-contract follow-up now points the next refresh at the F53 audit report and F54 fix report",
        "the first phone version remains landscape-only"
    )
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Route Evidence Audit Fix Refresh Audit Fix Refresh Audit Fixes")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes`')
$markdownLines.Add('Next formal task: `F55 refresh PC controlled-demo investor route evidence after F53 audit fixes`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source audit report: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit.json`')
$markdownLines.Add("")
$markdownLines.Add("## Fixed Areas")
foreach ($area in $report.fixedAreas) {
    $markdownLines.Add("- $area")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes check OK."
Write-Host "Report: $reportJsonPath"
