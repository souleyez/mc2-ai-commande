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
    $OutputDir = Join-Path $RepoRoot "analysis-output\f72-pc-route-audit-fixes"
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

$f71AuditReportRel = "analysis-output/f71-pc-route-evidence-audit/report.json"
$f72FixReportRel = "analysis-output/f72-pc-route-audit-fixes/report.json"
$f72OutputRel = "analysis-output/f72-pc-route-audit-fixes/"
$f72ScriptRel = "scripts/unity/check_f72_pc_route_audit_fixes.ps1"
$f73Task = "F73 refresh PC controlled-demo investor route evidence after F71 audit fixes"

$f71AuditMarkers = @(
    "F71RouteEvidenceAudit=pass-with-followups source=analysis-output/f71-pc-route-evidence-audit/report.json completed=F71 next=F72 noUnityLaunch=True mobile=landscape-only",
    "F71RouteEvidenceAuditFinding=F70-traceability status=pass detail=F70 consumes F68 audit and F69 fixes",
    "F71RouteEvidenceAuditFinding=route-proof-clarity status=pass detail=spawn>hangar-contact>damage-demo>solo-order>solo-return",
    "F71RouteEvidenceAuditFinding=damage-ejection-proof status=pass detail=section-loss+cockpit-ejection+wreck-salvage+repair-line",
    "F71RouteEvidenceAuditFinding=mobile-landscape-proof status=pass detail=first phone version remains landscape-only",
    "F71RouteEvidenceAuditFinding=public-safe-proxy-boundary status=pass detail=proxy-only visuals with unchanged collision/pathing",
    "F71RouteEvidenceAuditFinding=windows-path-budget status=pass detail=F70 script and output moved to short paths",
    "F71RouteEvidenceAuditFollowUp=P1 area=audit-visibility next=F72-doc-gate-visibility",
    "F71RouteEvidenceAuditFollowUp=P2 area=next-refresh-contract next=F73-consume-F71-audit-report",
    "F71RouteEvidenceAuditFollowUp=P2 area=path-budget next=F72-keep-new-F-artifacts-short"
)

$f72ClosureMarkers = @(
    "F72RouteEvidenceAuditFixes=pass source=analysis-output/f72-pc-route-audit-fixes/report.json completed=F72 next=F73 noUnityLaunch=True mobile=landscape-only",
    "F72RouteEvidenceAuditClosure=F72-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate sourceAudit=analysis-output/f71-pc-route-evidence-audit/report.json",
    "F72RouteEvidenceAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/f71-pc-route-evidence-audit/report.json sourceFixes=analysis-output/f72-pc-route-audit-fixes/report.json nextRefresh=F73-consume-F71-audit-report",
    "F72RouteEvidenceAuditClosure=path-budget status=closed script=scripts/unity/check_f72_pc_route_audit_fixes.ps1 output=analysis-output/f72-pc-route-audit-fixes/",
    "F72RouteEvidenceAuditClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False"
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
    Write-Host "F72 PC route audit fixes plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$f71AuditReport = Read-RepoJson -RelativePath $f71AuditReportRel
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

if ($null -ne $f71AuditReport) {
    Assert-Equals -Actual ([string]$f71AuditReport.schema) -Expected "F71PCRouteEvidenceRefreshAudit" -Label "F71 audit schema"
    Assert-Equals -Actual ([string]$f71AuditReport.result) -Expected "pass-with-followups" -Label "F71 audit result"
    Assert-Equals -Actual ([string]$f71AuditReport.completedTask) -Expected "F71 audit post-F70 PC controlled-demo investor route evidence refresh" -Label "F71 completed task"
    Assert-Equals -Actual ([string]$f71AuditReport.nextFormalTask) -Expected "F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes" -Label "F71 next task"
    Assert-Equals -Actual ([bool]$f71AuditReport.noUnityLaunch) -Expected $true -Label "F71 no Unity launch"

    foreach ($area in @("audit-visibility", "next-refresh-contract", "path-budget")) {
        $matches = @($f71AuditReport.followUps | Where-Object { [string]$_.area -eq $area })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "F71 follow-up $area"
    }
}

foreach ($surface in @($readme, $buildWin, $buildMobile, $evidenceDoc, $routeDoc, $handoffDoc)) {
    Assert-All -Text $surface -Label "F71 audit visibility surface" -Needles $f71AuditMarkers
    Assert-All -Text $surface -Label "F72 closure visibility surface" -Needles $f72ClosureMarkers
}

Assert-All -Text $masterPlan -Label "master F72/F73 queue" -Needles @(
    "2026-06-13 v123",
    "PC1-PC72",
    '| 150 | Done | `Implement post-F71 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| 151 | Next | `Refresh PC controlled-demo investor route evidence after F71 audit fixes` |',
    'Formal next task: `F73 refresh PC controlled-demo investor route evidence after F71 audit fixes`',
    "Mobile phones remain landscape-only for the first playable target"
)

Assert-All -Text $detailedPlan -Label "detailed F72/F73 queue" -Needles @(
    "2026-06-13 v132",
    "PC1-PC72",
    '| F72 | Done | `Implement post-F71 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| F73 | Next | `Refresh PC controlled-demo investor route evidence after F71 audit fixes` |',
    'formal next task: `F73 refresh PC controlled-demo investor route evidence after F71 audit fixes`',
    "Mobile phones remain first-version landscape-only"
)

Assert-All -Text $mobilePlan -Label "mobile F72/F73 status" -Needles @(
    "PC1-PC72",
    "F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes",
    $f73Task,
    "first phone version is landscape-only",
    "horizontal phone game"
)

Assert-All -Text $pcPlan -Label "PC plan F72/F73 status" -Needles @(
    "PC1-PC72",
    "F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes",
    $f73Task,
    "landscape-only phone scope"
)

Assert-All -Text $handoffDoc -Label "handoff F73 next" -Needles @(
    'Latest sealed PC/mobile wait-state checkpoint: `PC1-PC72`',
    'Current formal next development task after handoff: `F73 refresh PC controlled-demo investor route evidence after F71 audit fixes`',
    'Next planned work: `F73 refresh PC controlled-demo investor route evidence after F71 audit fixes`',
    "first phone version is landscape-only"
)

Assert-All -Text $currentGate -Label "current gate F72 plan marker" -Needles @(
    "check_f72_pc_route_audit_fixes.ps1",
    "F72 PC route audit fixes plan OK."
)

Assert-All -Text $queueScript -Label "queue F72/F73 marker" -Needles @(
    "F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes",
    $f73Task,
    '| F72 | Done | `Implement post-F71 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| F73 | Next | `Refresh PC controlled-demo investor route evidence after F71 audit fixes` |'
)

Assert-All -Text $handoffScript -Label "handoff script F72/F73 marker" -Needles @(
    "check_f72_pc_route_audit_fixes.ps1",
    "F72 PC route audit fixes check OK.",
    $f73Task
)

Assert-Contains -Text $gitignore -Needle $f72OutputRel -Label ".gitignore F72 output"
Assert-PathBudget -RelativePath $f72ScriptRel -MaxLength 96 -Label "F72 script path budget"
Assert-PathBudget -RelativePath $f72OutputRel -MaxLength 72 -Label "F72 output path budget"
Assert-PathBudget -RelativePath $f72FixReportRel -MaxLength 84 -Label "F72 report path budget"

if ($failures.Count -gt 0) {
    Write-Host "F72 PC route audit fixes check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) F72 PC route audit fixes check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "F72PCRouteEvidenceAuditFixes"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes"
    nextFormalTask = $f73Task
    noUnityLaunch = $true
    sourceAuditReport = $f71AuditReportRel
    outputReport = $f72FixReportRel
    closures = @(
        "F72-doc-gate-visibility",
        "F73-consume-F71-audit-report",
        "F72-keep-new-F-artifacts-short",
        "mobile-landscape-preserved"
    )
    closureMarkers = $f72ClosureMarkers
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# F72 PC Route Evidence Audit Fixes")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes`')
$markdownLines.Add('Next formal task: `' + $f73Task + '`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source audit report: `' + $f71AuditReportRel + '`')
$markdownLines.Add('Output report: `' + $f72FixReportRel + '`')
$markdownLines.Add("")
$markdownLines.Add("## Closures")
foreach ($marker in $report.closureMarkers) {
    $markdownLines.Add("- $marker")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "F72 PC route audit fixes check OK."
Write-Host "Report: $reportJsonPath"
