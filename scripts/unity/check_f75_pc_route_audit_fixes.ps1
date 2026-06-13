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
    $OutputDir = Join-Path $RepoRoot "analysis-output\f75-pc-route-audit-fixes"
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

$f74AuditReportRel = "analysis-output/f74-pc-route-evidence-refresh-audit/report.json"
$f75FixReportRel = "analysis-output/f75-pc-route-audit-fixes/report.json"
$f75OutputRel = "analysis-output/f75-pc-route-audit-fixes/"
$f75ScriptRel = "scripts/unity/check_f75_pc_route_audit_fixes.ps1"
$f76Task = "F76 refresh PC controlled-demo investor route evidence after F74 audit fixes"

$f74AuditMarkers = @(
    "F74RouteEvidenceAudit=pass-with-followups source=analysis-output/f74-pc-route-evidence-refresh-audit/report.json completed=F74 next=F75 noUnityLaunch=True mobile=landscape-only",
    "F74RouteEvidenceAuditFinding=F73-traceability status=pass detail=F73 consumes F71 audit and F72 fixes",
    "F74RouteEvidenceAuditFinding=route-proof-clarity status=pass detail=spawn>hangar-contact>damage-demo>solo-order>solo-return",
    "F74RouteEvidenceAuditFinding=damage-ejection-proof status=pass detail=section-loss+cockpit-ejection+wreck-salvage+repair-line",
    "F74RouteEvidenceAuditFinding=mobile-landscape-proof status=pass detail=first phone version remains landscape-only",
    "F74RouteEvidenceAuditFinding=public-safe-proxy-boundary status=pass detail=proxy-only visuals with unchanged collision/pathing",
    "F74RouteEvidenceAuditFinding=windows-path-budget status=pass detail=F73 and F74 script/output paths stay short",
    "F74RouteEvidenceAuditFollowUp=P1 area=audit-visibility next=F75-doc-gate-visibility",
    "F74RouteEvidenceAuditFollowUp=P2 area=next-refresh-contract next=F76-consume-F74-audit-report",
    "F74RouteEvidenceAuditFollowUp=P2 area=path-budget next=F75-keep-new-F-artifacts-short"
)

$f75ClosureMarkers = @(
    "F75RouteEvidenceAuditFixes=pass source=analysis-output/f75-pc-route-audit-fixes/report.json completed=F75 next=F76 noUnityLaunch=True mobile=landscape-only",
    "F75RouteEvidenceAuditClosure=F75-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate sourceAudit=analysis-output/f74-pc-route-evidence-refresh-audit/report.json",
    "F75RouteEvidenceAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/f74-pc-route-evidence-refresh-audit/report.json sourceFixes=analysis-output/f75-pc-route-audit-fixes/report.json nextRefresh=F76-consume-F74-audit-report",
    "F75RouteEvidenceAuditClosure=path-budget status=closed script=scripts/unity/check_f75_pc_route_audit_fixes.ps1 output=analysis-output/f75-pc-route-audit-fixes/",
    "F75RouteEvidenceAuditClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False"
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
    Write-Host "F75 PC route audit fixes plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$f74AuditReport = Read-RepoJson -RelativePath $f74AuditReportRel
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

if ($null -ne $f74AuditReport) {
    Assert-Equals -Actual ([string]$f74AuditReport.schema) -Expected "F74PCRouteEvidenceRefreshAudit" -Label "F74 audit schema"
    Assert-Equals -Actual ([string]$f74AuditReport.result) -Expected "pass-with-followups" -Label "F74 audit result"
    Assert-Equals -Actual ([string]$f74AuditReport.completedTask) -Expected "F74 audit post-F73 PC controlled-demo investor route evidence refresh" -Label "F74 completed task"
    Assert-Equals -Actual ([string]$f74AuditReport.nextFormalTask) -Expected "F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes" -Label "F74 next task"
    Assert-Equals -Actual ([bool]$f74AuditReport.noUnityLaunch) -Expected $true -Label "F74 no Unity launch"

    foreach ($area in @("audit-visibility", "next-refresh-contract", "path-budget")) {
        $matches = @($f74AuditReport.followUps | Where-Object { [string]$_.area -eq $area })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "F74 follow-up $area"
    }
}

foreach ($surface in @($readme, $buildWin, $buildMobile, $evidenceDoc, $routeDoc, $handoffDoc)) {
    Assert-All -Text $surface -Label "F74 audit visibility surface" -Needles $f74AuditMarkers
    Assert-All -Text $surface -Label "F75 closure visibility surface" -Needles $f75ClosureMarkers
}

Assert-All -Text $masterPlan -Label "master F75/F76 queue" -Needles @(
    "2026-06-13 v126",
    "PC1-PC75",
    '| 153 | Done | `Implement post-F74 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| 154 | Next | `Refresh PC controlled-demo investor route evidence after F74 audit fixes` |',
    'Formal next task: `F76 refresh PC controlled-demo investor route evidence after F74 audit fixes`',
    "Mobile phones remain landscape-only for the first playable target"
)

Assert-All -Text $detailedPlan -Label "detailed F75/F76 queue" -Needles @(
    "2026-06-13 v135",
    "PC1-PC75",
    '| F75 | Done | `Implement post-F74 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| F76 | Next | `Refresh PC controlled-demo investor route evidence after F74 audit fixes` |',
    'formal next task: `F76 refresh PC controlled-demo investor route evidence after F74 audit fixes`',
    "Mobile phones remain first-version landscape-only"
)

Assert-All -Text $mobilePlan -Label "mobile F75/F76 status" -Needles @(
    "PC1-PC75",
    "F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes",
    $f76Task,
    "first phone version is landscape-only",
    "horizontal phone game"
)

Assert-All -Text $pcPlan -Label "PC plan F75/F76 status" -Needles @(
    "PC1-PC75",
    "F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes",
    $f76Task,
    "landscape-only phone scope"
)

Assert-All -Text $handoffDoc -Label "handoff F76 next" -Needles @(
    'Latest sealed PC/mobile wait-state checkpoint: `PC1-PC75`',
    'Current formal next development task after handoff: `F76 refresh PC controlled-demo investor route evidence after F74 audit fixes`',
    'Next planned work: `F76 refresh PC controlled-demo investor route evidence after F74 audit fixes`',
    "first phone version is landscape-only"
)

Assert-All -Text $currentGate -Label "current gate F75 plan marker" -Needles @(
    "check_f75_pc_route_audit_fixes.ps1",
    "F75 PC route audit fixes plan OK."
)

Assert-All -Text $queueScript -Label "queue F75/F76 marker" -Needles @(
    "F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes",
    $f76Task,
    '| F75 | Done | `Implement post-F74 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| F76 | Next | `Refresh PC controlled-demo investor route evidence after F74 audit fixes` |'
)

Assert-All -Text $handoffScript -Label "handoff script F75/F76 marker" -Needles @(
    "check_f75_pc_route_audit_fixes.ps1",
    "F75 PC route audit fixes check OK.",
    $f76Task
)

Assert-Contains -Text $gitignore -Needle $f75OutputRel -Label ".gitignore F75 output"
Assert-PathBudget -RelativePath $f75ScriptRel -MaxLength 96 -Label "F75 script path budget"
Assert-PathBudget -RelativePath $f75OutputRel -MaxLength 72 -Label "F75 output path budget"
Assert-PathBudget -RelativePath $f75FixReportRel -MaxLength 84 -Label "F75 report path budget"

if ($failures.Count -gt 0) {
    Write-Host "F75 PC route audit fixes check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) F75 PC route audit fixes check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "F75PCRouteEvidenceAuditFixes"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes"
    nextFormalTask = $f76Task
    noUnityLaunch = $true
    sourceAuditReport = $f74AuditReportRel
    outputReport = $f75FixReportRel
    closures = @(
        "F75-doc-gate-visibility",
        "F76-consume-F74-audit-report",
        "F75-keep-new-F-artifacts-short",
        "mobile-landscape-preserved"
    )
    closureMarkers = $f75ClosureMarkers
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# F75 PC Route Evidence Audit Fixes")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes`')
$markdownLines.Add('Next formal task: `' + $f76Task + '`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source audit report: `' + $f74AuditReportRel + '`')
$markdownLines.Add('Output report: `' + $f75FixReportRel + '`')
$markdownLines.Add("")
$markdownLines.Add("## Closures")
foreach ($marker in $report.closureMarkers) {
    $markdownLines.Add("- $marker")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "F75 PC route audit fixes check OK."
Write-Host "Report: $reportJsonPath"
