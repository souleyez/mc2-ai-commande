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
    $OutputDir = Join-Path $RepoRoot "analysis-output\f81-pc-route-audit-fixes"
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

$f80AuditReportRel = "analysis-output/f80-pc-route-evidence-refresh-audit/report.json"
$f81FixReportRel = "analysis-output/f81-pc-route-audit-fixes/report.json"
$f81OutputRel = "analysis-output/f81-pc-route-audit-fixes/"
$f81ScriptRel = "scripts/unity/check_f81_pc_route_audit_fixes.ps1"
$f82Task = "F82 refresh PC controlled-demo investor route evidence after F80 audit fixes"

$f80AuditMarkers = @(
    "F80RouteEvidenceAudit=pass-with-followups source=analysis-output/f80-pc-route-evidence-refresh-audit/report.json completed=F80 next=F81 noUnityLaunch=True mobile=landscape-only",
    "F80RouteEvidenceAuditFinding=F79-traceability status=pass detail=F79 consumes F77 audit and F78 fixes",
    "F80RouteEvidenceAuditFinding=route-proof-clarity status=pass detail=spawn>hangar-contact>damage-demo>solo-order>solo-return",
    "F80RouteEvidenceAuditFinding=damage-ejection-proof status=pass detail=section-loss+cockpit-ejection+wreck-salvage+repair-line",
    "F80RouteEvidenceAuditFinding=mobile-landscape-proof status=pass detail=first phone version remains landscape-only",
    "F80RouteEvidenceAuditFinding=phone-horizontal-product-decision status=pass detail=phone-first-version-horizontal-only; portrait out of first slice",
    "F80RouteEvidenceAuditFinding=public-safe-proxy-boundary status=pass detail=proxy-only visuals with unchanged collision/pathing",
    "F80RouteEvidenceAuditFinding=windows-path-budget status=pass detail=F79 and F80 script/output paths stay short",
    "F80RouteEvidenceAuditFollowUp=P1 area=audit-visibility next=F81-doc-gate-visibility",
    "F80RouteEvidenceAuditFollowUp=P2 area=next-refresh-contract next=F82-consume-F80-audit-report",
    "F80RouteEvidenceAuditFollowUp=P2 area=path-budget next=F81-keep-new-F-artifacts-short"
)

$f81ClosureMarkers = @(
    "F81RouteEvidenceAuditFixes=pass source=analysis-output/f81-pc-route-audit-fixes/report.json completed=F81 next=F82 noUnityLaunch=True mobile=landscape-only",
    "F81RouteEvidenceAuditClosure=F81-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate sourceAudit=analysis-output/f80-pc-route-evidence-refresh-audit/report.json",
    "F81RouteEvidenceAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/f80-pc-route-evidence-refresh-audit/report.json sourceFixes=analysis-output/f81-pc-route-audit-fixes/report.json nextRefresh=F82-consume-F80-audit-report",
    "F81RouteEvidenceAuditClosure=path-budget status=closed script=scripts/unity/check_f81_pc_route_audit_fixes.ps1 output=analysis-output/f81-pc-route-audit-fixes/",
    "F81RouteEvidenceAuditClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False",
    "F81RouteEvidenceAuditClosure=phone-horizontal status=preserved firstPhoneVersion=horizontal-only portraitSupport=False"
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
    Write-Host "F81 PC route audit fixes plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$f80AuditReport = Read-RepoJson -RelativePath $f80AuditReportRel
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

if ($null -ne $f80AuditReport) {
    Assert-Equals -Actual ([string]$f80AuditReport.schema) -Expected "F80PCRouteEvidenceRefreshAudit" -Label "F80 audit schema"
    Assert-Equals -Actual ([string]$f80AuditReport.result) -Expected "pass-with-followups" -Label "F80 audit result"
    Assert-Equals -Actual ([string]$f80AuditReport.completedTask) -Expected "F80 audit post-F79 PC controlled-demo investor route evidence refresh" -Label "F80 completed task"
    Assert-Equals -Actual ([string]$f80AuditReport.nextFormalTask) -Expected "F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes" -Label "F80 next task"
    Assert-Equals -Actual ([bool]$f80AuditReport.noUnityLaunch) -Expected $true -Label "F80 no Unity launch"

    foreach ($area in @("audit-visibility", "next-refresh-contract", "path-budget")) {
        $matches = @($f80AuditReport.followUps | Where-Object { [string]$_.area -eq $area })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "F80 follow-up $area"
    }
}

foreach ($surface in @($readme, $buildWin, $buildMobile, $evidenceDoc, $routeDoc, $handoffDoc)) {
    Assert-All -Text $surface -Label "F80 audit visibility surface" -Needles $f80AuditMarkers
    Assert-All -Text $surface -Label "F81 closure visibility surface" -Needles $f81ClosureMarkers
}

Assert-All -Text $masterPlan -Label "master F81/F82 queue" -Needles @(
    "2026-06-13 v132",
    "PC1-PC81",
    '| 159 | Done | `Implement post-F80 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| 160 | Next | `Refresh PC controlled-demo investor route evidence after F80 audit fixes` |',
    'Formal next task: `F82 refresh PC controlled-demo investor route evidence after F80 audit fixes`',
    "Mobile phones remain landscape-only for the first playable target"
)

Assert-All -Text $detailedPlan -Label "detailed F81/F82 queue" -Needles @(
    "2026-06-13 v141",
    "PC1-PC81",
    '| F81 | Done | `Implement post-F80 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| F82 | Next | `Refresh PC controlled-demo investor route evidence after F80 audit fixes` |',
    'formal next task: `F82 refresh PC controlled-demo investor route evidence after F80 audit fixes`',
    "Mobile phones remain first-version landscape-only"
)

Assert-All -Text $mobilePlan -Label "mobile F81/F82 status" -Needles @(
    "PC1-PC81",
    "F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes",
    $f82Task,
    "first phone version is landscape-only",
    "horizontal phone game"
)

Assert-All -Text $pcPlan -Label "PC plan F81/F82 status" -Needles @(
    "PC1-PC81",
    "F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes",
    $f82Task,
    "landscape-only phone scope"
)

Assert-All -Text $handoffDoc -Label "handoff F82 next" -Needles @(
    'Latest sealed PC/mobile wait-state checkpoint: `PC1-PC81`',
    'Current formal next development task after handoff: `F82 refresh PC controlled-demo investor route evidence after F80 audit fixes`',
    'Next planned work: `F82 refresh PC controlled-demo investor route evidence after F80 audit fixes`',
    "first phone version is landscape-only"
)

Assert-All -Text $currentGate -Label "current gate F81 plan marker" -Needles @(
    "check_f81_pc_route_audit_fixes.ps1",
    "F81 PC route audit fixes plan OK."
)

Assert-All -Text $queueScript -Label "queue F81/F82 marker" -Needles @(
    "F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes",
    $f82Task,
    '| F81 | Done | `Implement post-F80 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| F82 | Next | `Refresh PC controlled-demo investor route evidence after F80 audit fixes` |'
)

Assert-All -Text $handoffScript -Label "handoff script F81/F82 marker" -Needles @(
    "check_f81_pc_route_audit_fixes.ps1",
    "F81 PC route audit fixes check OK.",
    $f82Task
)

Assert-Contains -Text $gitignore -Needle $f81OutputRel -Label ".gitignore F81 output"
Assert-PathBudget -RelativePath $f81ScriptRel -MaxLength 96 -Label "F81 script path budget"
Assert-PathBudget -RelativePath $f81OutputRel -MaxLength 72 -Label "F81 output path budget"
Assert-PathBudget -RelativePath $f81FixReportRel -MaxLength 84 -Label "F81 report path budget"

if ($failures.Count -gt 0) {
    Write-Host "F81 PC route audit fixes check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) F81 PC route audit fixes check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "F81PCRouteEvidenceAuditFixes"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes"
    nextFormalTask = $f82Task
    noUnityLaunch = $true
    sourceAuditReport = $f80AuditReportRel
    outputReport = $f81FixReportRel
    closures = @(
        "F81-doc-gate-visibility",
        "F82-consume-F80-audit-report",
        "F81-keep-new-F-artifacts-short",
        "mobile-landscape-preserved",
        "phone-horizontal-preserved"
    )
    closureMarkers = $f81ClosureMarkers
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# F81 PC Route Evidence Audit Fixes")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes`')
$markdownLines.Add('Next formal task: `' + $f82Task + '`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source audit report: `' + $f80AuditReportRel + '`')
$markdownLines.Add('Output report: `' + $f81FixReportRel + '`')
$markdownLines.Add("")
$markdownLines.Add("## Closures")
foreach ($marker in $report.closureMarkers) {
    $markdownLines.Add("- $marker")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "F81 PC route audit fixes check OK."
Write-Host "Report: $reportJsonPath"
