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
    $OutputDir = Join-Path $RepoRoot "analysis-output\f78-pc-route-audit-fixes"
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

$f77AuditReportRel = "analysis-output/f77-pc-route-evidence-refresh-audit/report.json"
$f78FixReportRel = "analysis-output/f78-pc-route-audit-fixes/report.json"
$f78OutputRel = "analysis-output/f78-pc-route-audit-fixes/"
$f78ScriptRel = "scripts/unity/check_f78_pc_route_audit_fixes.ps1"
$f79Task = "F79 refresh PC controlled-demo investor route evidence after F77 audit fixes"

$f77AuditMarkers = @(
    "F77RouteEvidenceAudit=pass-with-followups source=analysis-output/f77-pc-route-evidence-refresh-audit/report.json completed=F77 next=F78 noUnityLaunch=True mobile=landscape-only",
    "F77RouteEvidenceAuditFinding=F76-traceability status=pass detail=F76 consumes F74 audit and F75 fixes",
    "F77RouteEvidenceAuditFinding=route-proof-clarity status=pass detail=spawn>hangar-contact>damage-demo>solo-order>solo-return",
    "F77RouteEvidenceAuditFinding=damage-ejection-proof status=pass detail=section-loss+cockpit-ejection+wreck-salvage+repair-line",
    "F77RouteEvidenceAuditFinding=mobile-landscape-proof status=pass detail=first phone version remains landscape-only",
    "F77RouteEvidenceAuditFinding=public-safe-proxy-boundary status=pass detail=proxy-only visuals with unchanged collision/pathing",
    "F77RouteEvidenceAuditFinding=windows-path-budget status=pass detail=F76 and F77 script/output paths stay short",
    "F77RouteEvidenceAuditFollowUp=P1 area=audit-visibility next=F78-doc-gate-visibility",
    "F77RouteEvidenceAuditFollowUp=P2 area=next-refresh-contract next=F79-consume-F77-audit-report",
    "F77RouteEvidenceAuditFollowUp=P2 area=path-budget next=F78-keep-new-F-artifacts-short"
)

$f78ClosureMarkers = @(
    "F78RouteEvidenceAuditFixes=pass source=analysis-output/f78-pc-route-audit-fixes/report.json completed=F78 next=F79 noUnityLaunch=True mobile=landscape-only",
    "F78RouteEvidenceAuditClosure=F78-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate sourceAudit=analysis-output/f77-pc-route-evidence-refresh-audit/report.json",
    "F78RouteEvidenceAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/f77-pc-route-evidence-refresh-audit/report.json sourceFixes=analysis-output/f78-pc-route-audit-fixes/report.json nextRefresh=F79-consume-F77-audit-report",
    "F78RouteEvidenceAuditClosure=path-budget status=closed script=scripts/unity/check_f78_pc_route_audit_fixes.ps1 output=analysis-output/f78-pc-route-audit-fixes/",
    "F78RouteEvidenceAuditClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False"
)

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Add-Failure {
    param([string]$Message)
    [void]$failures.Add($Message)
}

function Add-Row {
    param(
        [string]$Check,
        [string]$Detail
    )

    [void]$rows.Add([pscustomobject]@{
        check = $Check
        status = "OK"
        detail = $Detail
    })
}

function Read-RepoText {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "$RelativePath missing"
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
        Add-Failure "$RelativePath is not valid JSON: $($_.Exception.Message)"
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
        Add-Failure "$Label missing marker: $Needle"
        return
    }

    Add-Row -Check $Label -Detail $Needle
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
        Add-Failure "$Label expected '$Expected', got '$Actual'"
        return
    }

    Add-Row -Check $Label -Detail ([string]$Expected)
}

function Assert-PathBudget {
    param(
        [string]$RelativePath,
        [int]$MaxLength,
        [string]$Label
    )

    if ($RelativePath.Length -gt $MaxLength) {
        Add-Failure "$Label exceeds path budget: $($RelativePath.Length) > $MaxLength ($RelativePath)"
        return
    }

    Add-Row -Check $Label -Detail "$($RelativePath.Length)/$MaxLength $RelativePath"
}

if ($PlanOnly) {
    Write-Host "F78 PC route audit fixes plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$f77AuditReport = Read-RepoJson -RelativePath $f77AuditReportRel
$readme = Read-RepoText -RelativePath "README.md"
$buildWin = Read-RepoText -RelativePath "BUILD-WIN.md"
$buildMobile = Read-RepoText -RelativePath "BUILD-MOBILE.md"
$masterPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RepoText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$pcPlan = Read-RepoText -RelativePath "docs-pc-optimization-plan-2026-06-11.md"
$handoffDoc = Read-RepoText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$evidenceDoc = Read-RepoText -RelativePath "docs-playable-demo-investor-evidence-2026-06-07.md"
$routeDoc = Read-RepoText -RelativePath "docs-pc-investor-demo-route-2026-06-13.md"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queueScript = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$gitignore = Read-RepoText -RelativePath ".gitignore"

if ($null -ne $f77AuditReport) {
    Assert-Equals -Actual ([string]$f77AuditReport.schema) -Expected "F77PCRouteEvidenceRefreshAudit" -Label "F77 audit schema"
    Assert-Equals -Actual ([string]$f77AuditReport.result) -Expected "pass-with-followups" -Label "F77 audit result"
    Assert-Equals -Actual ([string]$f77AuditReport.completedTask) -Expected "F77 audit post-F76 PC controlled-demo investor route evidence refresh" -Label "F77 completed task"
    Assert-Equals -Actual ([string]$f77AuditReport.nextFormalTask) -Expected "F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes" -Label "F77 next task"
    Assert-Equals -Actual ([bool]$f77AuditReport.noUnityLaunch) -Expected $true -Label "F77 no Unity launch"

    foreach ($area in @("audit-visibility", "next-refresh-contract", "path-budget")) {
        $matches = @($f77AuditReport.followUps | Where-Object { [string]$_.area -eq $area })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "F77 follow-up $area"
    }
}

foreach ($surface in @($readme, $buildWin, $buildMobile, $evidenceDoc, $routeDoc, $handoffDoc)) {
    Assert-All -Text $surface -Label "F77 audit visibility surface" -Needles $f77AuditMarkers
    Assert-All -Text $surface -Label "F78 closure visibility surface" -Needles $f78ClosureMarkers
}

Assert-All -Text $masterPlan -Label "master F78/F79 queue" -Needles @(
    "2026-06-13 v129",
    "PC1-PC78",
    '| 156 | Done | `Implement post-F77 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| 157 | Next | `Refresh PC controlled-demo investor route evidence after F77 audit fixes` |',
    'Formal next task: `F79 refresh PC controlled-demo investor route evidence after F77 audit fixes`',
    "Mobile phones remain landscape-only for the first playable target"
)

Assert-All -Text $detailedPlan -Label "detailed F78/F79 queue" -Needles @(
    "2026-06-13 v138",
    "PC1-PC78",
    '| F78 | Done | `Implement post-F77 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| F79 | Next | `Refresh PC controlled-demo investor route evidence after F77 audit fixes` |',
    'formal next task: `F79 refresh PC controlled-demo investor route evidence after F77 audit fixes`',
    "Mobile phones remain first-version landscape-only"
)

Assert-All -Text $mobilePlan -Label "mobile F78/F79 status" -Needles @(
    "PC1-PC78",
    "F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes",
    $f79Task,
    "first phone version is landscape-only",
    "horizontal phone game"
)

Assert-All -Text $pcPlan -Label "PC plan F78/F79 status" -Needles @(
    "PC1-PC78",
    "F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes",
    $f79Task,
    "landscape-only phone scope"
)

Assert-All -Text $handoffDoc -Label "handoff F79 next" -Needles @(
    'Latest sealed PC/mobile wait-state checkpoint: `PC1-PC78`',
    'Current formal next development task after handoff: `F79 refresh PC controlled-demo investor route evidence after F77 audit fixes`',
    'Next planned work: `F79 refresh PC controlled-demo investor route evidence after F77 audit fixes`',
    "first phone version is landscape-only"
)

Assert-All -Text $currentGate -Label "current gate F78 plan marker" -Needles @(
    "check_f78_pc_route_audit_fixes.ps1",
    "F78 PC route audit fixes plan OK."
)

Assert-All -Text $queueScript -Label "queue F78/F79 marker" -Needles @(
    "F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes",
    $f79Task,
    '| F78 | Done | `Implement post-F77 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| F79 | Next | `Refresh PC controlled-demo investor route evidence after F77 audit fixes` |'
)

Assert-All -Text $handoffScript -Label "handoff script F78/F79 marker" -Needles @(
    "check_f78_pc_route_audit_fixes.ps1",
    "F78 PC route audit fixes check OK.",
    $f79Task
)

Assert-Contains -Text $gitignore -Needle $f78OutputRel -Label ".gitignore F78 output"
Assert-PathBudget -RelativePath $f78ScriptRel -MaxLength 96 -Label "F78 script path budget"
Assert-PathBudget -RelativePath $f78OutputRel -MaxLength 72 -Label "F78 output path budget"
Assert-PathBudget -RelativePath $f78FixReportRel -MaxLength 84 -Label "F78 report path budget"

if ($failures.Count -gt 0) {
    Write-Host "F78 PC route audit fixes check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) F78 PC route audit fixes check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "F78PCRouteEvidenceAuditFixes"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes"
    nextFormalTask = $f79Task
    noUnityLaunch = $true
    sourceAuditReport = $f77AuditReportRel
    outputReport = $f78FixReportRel
    closures = @(
        "F78-doc-gate-visibility",
        "F79-consume-F77-audit-report",
        "F78-keep-new-F-artifacts-short",
        "mobile-landscape-preserved"
    )
    closureMarkers = $f78ClosureMarkers
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# F78 PC Route Evidence Audit Fixes")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes`')
$markdownLines.Add('Next formal task: `' + $f79Task + '`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source audit report: `' + $f77AuditReportRel + '`')
$markdownLines.Add('Output report: `' + $f78FixReportRel + '`')
$markdownLines.Add("")
$markdownLines.Add("## Closures")
foreach ($marker in $report.closureMarkers) {
    $markdownLines.Add("- $marker")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "F78 PC route audit fixes check OK."
Write-Host "Report: $reportJsonPath"
