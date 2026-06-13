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
    $OutputDir = Join-Path $RepoRoot "analysis-output\f84-pc-route-audit-fixes"
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

$f83AuditReportRel = "analysis-output/f83-pc-route-evidence-refresh-audit/report.json"
$f84FixReportRel = "analysis-output/f84-pc-route-audit-fixes/report.json"
$f84OutputRel = "analysis-output/f84-pc-route-audit-fixes/"
$f84ScriptRel = "scripts/unity/check_f84_pc_route_audit_fixes.ps1"
$f85Task = "F85 refresh PC controlled-demo investor route evidence after F83 audit fixes"

$f83AuditMarkers = @(
    "F83RouteEvidenceAudit=pass-with-followups source=analysis-output/f83-pc-route-evidence-refresh-audit/report.json completed=F83 next=F84 noUnityLaunch=True mobile=landscape-only",
    "F83RouteEvidenceAuditFinding=F82-traceability status=pass detail=F82 consumes F80 audit and F81 fixes",
    "F83RouteEvidenceAuditFinding=route-proof-clarity status=pass detail=spawn>hangar-contact>damage-demo>solo-order>solo-return",
    "F83RouteEvidenceAuditFinding=damage-ejection-proof status=pass detail=section-loss+cockpit-ejection+wreck-salvage+repair-line",
    "F83RouteEvidenceAuditFinding=mobile-landscape-proof status=pass detail=first phone version remains landscape-only",
    "F83RouteEvidenceAuditFinding=phone-horizontal-product-decision status=pass detail=phone-first-version-horizontal-only; portrait out of first slice",
    "F83RouteEvidenceAuditFinding=public-safe-proxy-boundary status=pass detail=proxy-only visuals with unchanged collision/pathing",
    "F83RouteEvidenceAuditFinding=windows-path-budget status=pass detail=F82 and F83 script/output paths stay short",
    "F83RouteEvidenceAuditFollowUp=P1 area=audit-visibility next=F84-doc-gate-visibility",
    "F83RouteEvidenceAuditFollowUp=P2 area=next-refresh-contract next=F85-consume-F83-audit-report",
    "F83RouteEvidenceAuditFollowUp=P2 area=path-budget next=F84-keep-new-F-artifacts-short"
)

$f84ClosureMarkers = @(
    "F84RouteEvidenceAuditFixes=pass source=analysis-output/f84-pc-route-audit-fixes/report.json completed=F84 next=F85 noUnityLaunch=True mobile=landscape-only",
    "F84RouteEvidenceAuditClosure=F84-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate sourceAudit=analysis-output/f83-pc-route-evidence-refresh-audit/report.json",
    "F84RouteEvidenceAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/f83-pc-route-evidence-refresh-audit/report.json sourceFixes=analysis-output/f84-pc-route-audit-fixes/report.json nextRefresh=F85-consume-F83-audit-report",
    "F84RouteEvidenceAuditClosure=path-budget status=closed script=scripts/unity/check_f84_pc_route_audit_fixes.ps1 output=analysis-output/f84-pc-route-audit-fixes/",
    "F84RouteEvidenceAuditClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False",
    "F84RouteEvidenceAuditClosure=phone-horizontal status=preserved firstPhoneVersion=horizontal-only portraitSupport=False"
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
    Write-Host "F84 PC route audit fixes plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$f83AuditReport = Read-RepoJson -RelativePath $f83AuditReportRel
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
$landscapeContract = Read-RepoText -RelativePath "scripts\unity\check_mobile_landscape_contract.ps1"
$projectSettings = Read-RepoText -RelativePath "unity-mc2-demo\ProjectSettings\ProjectSettings.asset"
$runtimeScript = Read-RepoText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$gitignore = Read-RepoText -RelativePath ".gitignore"

if ($null -ne $f83AuditReport) {
    Assert-Equals -Actual ([string]$f83AuditReport.schema) -Expected "F83PCRouteEvidenceRefreshAudit" -Label "F83 audit schema"
    Assert-Equals -Actual ([string]$f83AuditReport.result) -Expected "pass-with-followups" -Label "F83 audit result"
    Assert-Equals -Actual ([string]$f83AuditReport.completedTask) -Expected "F83 audit post-F82 PC controlled-demo investor route evidence refresh" -Label "F83 completed task"
    Assert-Equals -Actual ([string]$f83AuditReport.nextFormalTask) -Expected "F84 implement post-F83 PC controlled-demo investor route evidence refresh audit fixes" -Label "F83 next task"
    Assert-Equals -Actual ([bool]$f83AuditReport.noUnityLaunch) -Expected $true -Label "F83 no Unity launch"

    foreach ($area in @("audit-visibility", "next-refresh-contract", "path-budget")) {
        $matches = @($f83AuditReport.followUps | Where-Object { [string]$_.area -eq $area })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "F83 follow-up $area"
    }
}

foreach ($surface in @($readme, $buildWin, $buildMobile, $evidenceDoc, $routeDoc, $handoffDoc)) {
    Assert-All -Text $surface -Label "F83 audit visibility surface" -Needles $f83AuditMarkers
    Assert-All -Text $surface -Label "F84 closure visibility surface" -Needles $f84ClosureMarkers
}

Assert-All -Text $masterPlan -Label "master F84/F85 queue" -Needles @(
    "2026-06-13 v135",
    "PC1-PC84",
    '| 162 | Done | `Implement post-F83 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| 163 | Next | `Refresh PC controlled-demo investor route evidence after F83 audit fixes` |',
    'Formal next task: `F85 refresh PC controlled-demo investor route evidence after F83 audit fixes`',
    "Mobile phones remain landscape-only for the first playable target"
)

Assert-All -Text $detailedPlan -Label "detailed F84/F85 queue" -Needles @(
    "2026-06-13 v144",
    "PC1-PC84",
    '| F84 | Done | `Implement post-F83 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| F85 | Next | `Refresh PC controlled-demo investor route evidence after F83 audit fixes` |',
    'formal next task: `F85 refresh PC controlled-demo investor route evidence after F83 audit fixes`',
    "Mobile phones remain first-version landscape-only"
)

Assert-All -Text $mobilePlan -Label "mobile F84/F85 status" -Needles @(
    "PC1-PC84",
    "F84 implement post-F83 PC controlled-demo investor route evidence refresh audit fixes",
    $f85Task,
    "first phone version is landscape-only",
    "horizontal phone game"
)

Assert-All -Text $pcPlan -Label "PC plan F84/F85 status" -Needles @(
    "PC1-PC84",
    "F84 implement post-F83 PC controlled-demo investor route evidence refresh audit fixes",
    $f85Task,
    "landscape-only phone scope"
)

Assert-All -Text $handoffDoc -Label "handoff F85 next" -Needles @(
    'Latest sealed PC/mobile wait-state checkpoint: `PC1-PC84`',
    'Current formal next development task after handoff: `F85 refresh PC controlled-demo investor route evidence after F83 audit fixes`',
    'Next planned work: `F85 refresh PC controlled-demo investor route evidence after F83 audit fixes`',
    "first phone version is landscape-only"
)

Assert-All -Text $currentGate -Label "current gate F84 plan marker" -Needles @(
    "check_f84_pc_route_audit_fixes.ps1",
    "F84 PC route audit fixes plan OK."
)

Assert-All -Text $queueScript -Label "queue F84/F85 marker" -Needles @(
    "F84 implement post-F83 PC controlled-demo investor route evidence refresh audit fixes",
    $f85Task,
    '| F84 | Done | `Implement post-F83 PC controlled-demo investor route evidence refresh audit fixes` |',
    '| F85 | Next | `Refresh PC controlled-demo investor route evidence after F83 audit fixes` |'
)

Assert-All -Text $handoffScript -Label "handoff script F84/F85 marker" -Needles @(
    "check_f84_pc_route_audit_fixes.ps1",
    "F84 PC route audit fixes check OK.",
    $f85Task
)

Assert-All -Text $projectSettings -Label "Unity mobile landscape settings" -Needles @(
    "defaultScreenOrientation: 3",
    "allowedAutorotateToPortrait: 0",
    "allowedAutorotateToPortraitUpsideDown: 0",
    "allowedAutorotateToLandscapeRight: 1",
    "allowedAutorotateToLandscapeLeft: 1"
)

Assert-All -Text $runtimeScript -Label "Unity mobile runtime landscape guard" -Needles @(
    "Screen.autorotateToPortrait = false;",
    "Screen.autorotateToPortraitUpsideDown = false;",
    "Screen.autorotateToLandscapeLeft = true;",
    "Screen.autorotateToLandscapeRight = true;",
    "Screen.orientation = ScreenOrientation.AutoRotation;"
)

Assert-All -Text $landscapeContract -Label "mobile landscape contract checker" -Needles @(
    "defaultScreenOrientation: 3",
    "allowedAutorotateToPortrait: 0",
    "allowedAutorotateToLandscapeRight: 1",
    "horizontal phone game",
    "Mobile landscape contract check OK"
)

Assert-Contains -Text $gitignore -Needle $f84OutputRel -Label ".gitignore F84 output"
Assert-PathBudget -RelativePath $f84ScriptRel -MaxLength 96 -Label "F84 script path budget"
Assert-PathBudget -RelativePath $f84OutputRel -MaxLength 72 -Label "F84 output path budget"
Assert-PathBudget -RelativePath $f84FixReportRel -MaxLength 84 -Label "F84 report path budget"

if ($failures.Count -gt 0) {
    Write-Host "F84 PC route audit fixes check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) F84 PC route audit fixes check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "F84PCRouteEvidenceAuditFixes"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F84 implement post-F83 PC controlled-demo investor route evidence refresh audit fixes"
    nextFormalTask = $f85Task
    noUnityLaunch = $true
    sourceAuditReport = $f83AuditReportRel
    outputReport = $f84FixReportRel
    mobileContract = "landscape-only"
    closures = @(
        "F84-doc-gate-visibility",
        "F85-consume-F83-audit-report",
        "F84-keep-new-F-artifacts-short",
        "mobile-landscape-preserved",
        "phone-horizontal-preserved"
    )
    closureMarkers = $f84ClosureMarkers
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# F84 PC Route Evidence Audit Fixes")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F84 implement post-F83 PC controlled-demo investor route evidence refresh audit fixes`')
$markdownLines.Add('Next formal task: `' + $f85Task + '`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("MobileContract: landscape-only")
$markdownLines.Add("")
$markdownLines.Add('Source audit report: `' + $f83AuditReportRel + '`')
$markdownLines.Add('Output report: `' + $f84FixReportRel + '`')
$markdownLines.Add("")
$markdownLines.Add("## Closures")
foreach ($marker in $report.closureMarkers) {
    $markdownLines.Add("- $marker")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "F84 PC route audit fixes check OK."
Write-Host "Report: $reportJsonPath"
