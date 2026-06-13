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
    $OutputDir = Join-Path $RepoRoot "analysis-output\f79-pc-route-evidence-refresh"
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
$requiredPresets = @("spawn", "hangar-contact", "damage-demo", "solo-order", "solo-return")

$commandReportRel = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json"
$commandMarkdownRel = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.md"
$f77AuditReportRel = "analysis-output/f77-pc-route-evidence-refresh-audit/report.json"
$f78FixReportRel = "analysis-output/f78-pc-route-audit-fixes/report.json"
$f79OutputRel = "analysis-output/f79-pc-route-evidence-refresh/"
$f79ScriptRel = "scripts/unity/check_f79_pc_route_evidence_refresh.ps1"
$f80Task = "F80 audit post-F79 PC controlled-demo investor route evidence refresh"

$f78ClosureMarkers = @(
    "F78RouteEvidenceAuditFixes=pass source=analysis-output/f78-pc-route-audit-fixes/report.json completed=F78 next=F79 noUnityLaunch=True mobile=landscape-only",
    "F78RouteEvidenceAuditClosure=F78-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate sourceAudit=analysis-output/f77-pc-route-evidence-refresh-audit/report.json",
    "F78RouteEvidenceAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/f77-pc-route-evidence-refresh-audit/report.json sourceFixes=analysis-output/f78-pc-route-audit-fixes/report.json nextRefresh=F79-consume-F77-audit-report",
    "F78RouteEvidenceAuditClosure=path-budget status=closed script=scripts/unity/check_f78_pc_route_audit_fixes.ps1 output=analysis-output/f78-pc-route-audit-fixes/",
    "F78RouteEvidenceAuditClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False"
)

$refreshMarkers = @(
    "F79RouteEvidenceRefresh=ready source=analysis-output/f78-pc-route-audit-fixes/report.json completed=F79 next=F80 noUnityLaunch=True mobile=landscape-only",
    "F79RouteEvidenceRefreshSource=audit sourceAudit=analysis-output/f77-pc-route-evidence-refresh-audit/report.json sourceFixes=analysis-output/f78-pc-route-audit-fixes/report.json",
    "F79RouteEvidenceRefreshClosure=route-proof-preserved route=spawn>hangar-contact>damage-demo>solo-order>solo-return damage=section-loss+cockpit-ejection+wreck-salvage+repair-line publicSafe=proxy-only",
    "F79RouteEvidenceRefreshClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False"
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

function Assert-FileExists {
    param(
        [string]$RelativePath,
        [string]$Label
    )

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "$Label missing: $RelativePath"
        return
    }

    $file = Get-Item -LiteralPath $path
    if ($file.Length -le 0) {
        Add-Failure "$Label is empty: $RelativePath"
        return
    }

    Add-Row -Check $Label -Detail $RelativePath
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
    Write-Host "F79 PC route evidence refresh plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandCapture = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$commandMarkdown = Read-RepoText -RelativePath $commandMarkdownRel
$commandReport = Read-RepoJson -RelativePath $commandReportRel
$f77AuditReport = Read-RepoJson -RelativePath $f77AuditReportRel
$f78FixReport = Read-RepoJson -RelativePath $f78FixReportRel
$masterPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RepoText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$pcPlan = Read-RepoText -RelativePath "docs-pc-optimization-plan-2026-06-11.md"
$handoffDoc = Read-RepoText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$evidenceDoc = Read-RepoText -RelativePath "docs-playable-demo-investor-evidence-2026-06-07.md"
$routeDoc = Read-RepoText -RelativePath "docs-pc-investor-demo-route-2026-06-13.md"
$readme = Read-RepoText -RelativePath "README.md"
$buildWin = Read-RepoText -RelativePath "BUILD-WIN.md"
$buildMobile = Read-RepoText -RelativePath "BUILD-MOBILE.md"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queueScript = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$gitignore = Read-RepoText -RelativePath ".gitignore"

Assert-All -Text $commandCapture -Label "command capture F79 metadata" -Needles @(
    'F79 refresh PC controlled-demo investor route evidence after F77 audit fixes',
    'F80 audit post-F79 PC controlled-demo investor route evidence refresh',
    '$f77RouteEvidenceAuditReportPath',
    '$f78RouteAuditFixesReportPath',
    'sourceF77RouteEvidenceAuditReport',
    'sourceF78RouteAuditFixesReport',
    'f78RouteEvidenceAuditClosure'
)

Assert-All -Text $commandMarkdown -Label "command markdown F79 route evidence" -Needles @(
    'Completed task: `F79 refresh PC controlled-demo investor route evidence after F77 audit fixes`',
    'Next formal task: `F80 audit post-F79 PC controlled-demo investor route evidence refresh`',
    'Source F77 route evidence audit report: "analysis-output/f77-pc-route-evidence-refresh-audit/report.json"',
    'Source F78 route audit fixes report: "analysis-output/f78-pc-route-audit-fixes/report.json"',
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity'
)
Assert-All -Text $commandMarkdown -Label "command markdown F79 refresh markers" -Needles $refreshMarkers

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.schema) -Expected "PCControlledDemoCommandEvidenceRefresh" -Label "command report schema"
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F79 refresh PC controlled-demo investor route evidence after F77 audit fixes" -Label "command report completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected $f80Task -Label "command report next task"
    Assert-Equals -Actual ([string]$commandReport.sourceF77RouteEvidenceAuditReport) -Expected $f77AuditReportRel -Label "command report source F77 audit"
    Assert-Equals -Actual ([string]$commandReport.sourceF78RouteAuditFixesReport) -Expected $f78FixReportRel -Label "command report source F78 fixes"
    Assert-Equals -Actual ([int]$commandReport.width) -Expected 1280 -Label "command report width"
    Assert-Equals -Actual ([int]$commandReport.height) -Expected 720 -Label "command report height"
    Assert-Equals -Actual (@($commandReport.evidence).Count) -Expected $requiredPresets.Count -Label "command preset count"

    foreach ($preset in $requiredPresets) {
        $matches = @($commandReport.evidence | Where-Object { [string]$_.preset -eq $preset })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "command report preset $preset"
        if ($matches.Count -eq 1) {
            Assert-FileExists -RelativePath ([string]$matches[0].screenshot) -Label "$preset screenshot"
            Assert-FileExists -RelativePath ([string]$matches[0].sidecar) -Label "$preset sidecar"
            Assert-FileExists -RelativePath ([string]$matches[0].log) -Label "$preset log"
            Assert-Contains -Text ([string]$matches[0].playableFlowPolish) -Needle "mobileLandscapeOnly=True orientation=landscape" -Label "$preset landscape proof"
            Assert-Contains -Text ([string]$matches[0].investorProxyVisuals) -Needle "publicSafe=proxy-only" -Label "$preset public-safe proof"
            Assert-Contains -Text ([string]$matches[0].investorProxyVisuals) -Needle "collision=unchanged pathing=unchanged BattleCore=unchanged" -Label "$preset gameplay boundary"
        }
    }

    foreach ($marker in $refreshMarkers) {
        $matches = @($commandReport.f78RouteEvidenceAuditClosure | Where-Object { [string]$_ -eq $marker })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "command report F79 closure $marker"
    }
}

if ($null -ne $f77AuditReport) {
    Assert-Equals -Actual ([string]$f77AuditReport.result) -Expected "pass-with-followups" -Label "F77 audit result"
    Assert-Equals -Actual ([string]$f77AuditReport.completedTask) -Expected "F77 audit post-F76 PC controlled-demo investor route evidence refresh" -Label "F77 completed task"
    Assert-Equals -Actual ([string]$f77AuditReport.nextFormalTask) -Expected "F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes" -Label "F77 next task"
}

if ($null -ne $f78FixReport) {
    Assert-Equals -Actual ([string]$f78FixReport.result) -Expected "pass" -Label "F78 fixes result"
    Assert-Equals -Actual ([string]$f78FixReport.completedTask) -Expected "F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes" -Label "F78 completed task"
    Assert-Equals -Actual ([string]$f78FixReport.nextFormalTask) -Expected "F79 refresh PC controlled-demo investor route evidence after F77 audit fixes" -Label "F78 next task"
}

foreach ($surface in @($readme, $buildWin, $buildMobile, $evidenceDoc, $routeDoc, $handoffDoc)) {
    Assert-All -Text $surface -Label "F79 route evidence refresh surface" -Needles $refreshMarkers
}

Assert-All -Text $masterPlan -Label "master F79/F80 queue" -Needles @(
    "2026-06-13 v130",
    "PC1-PC79",
    '| 157 | Done | `Refresh PC controlled-demo investor route evidence after F77 audit fixes` |',
    '| 158 | Next | `Audit post-F79 PC controlled-demo investor route evidence refresh` |',
    'Formal next task: `F80 audit post-F79 PC controlled-demo investor route evidence refresh`',
    "Mobile phones remain landscape-only for the first playable target"
)
Assert-All -Text $detailedPlan -Label "detailed F79/F80 queue" -Needles @(
    "2026-06-13 v139",
    "PC1-PC79",
    '| F79 | Done | `Refresh PC controlled-demo investor route evidence after F77 audit fixes` |',
    '| F80 | Next | `Audit post-F79 PC controlled-demo investor route evidence refresh` |',
    'formal next task: `F80 audit post-F79 PC controlled-demo investor route evidence refresh`',
    "Mobile phones remain first-version landscape-only"
)
Assert-All -Text $mobilePlan -Label "mobile F79/F80 status" -Needles @(
    "PC1-PC79",
    "F79 refresh PC controlled-demo investor route evidence after F77 audit fixes",
    $f80Task,
    "first phone version is landscape-only",
    "horizontal phone game"
)
Assert-All -Text $pcPlan -Label "PC plan F79/F80 status" -Needles @(
    "PC1-PC79",
    "F79 refresh PC controlled-demo investor route evidence after F77 audit fixes",
    $f80Task,
    "landscape-only phone scope"
)
Assert-All -Text $handoffDoc -Label "handoff F80 next" -Needles @(
    'Latest sealed PC/mobile wait-state checkpoint: `PC1-PC79`',
    'Current formal next development task after handoff: `F80 audit post-F79 PC controlled-demo investor route evidence refresh`',
    'Next planned work: `F80 audit post-F79 PC controlled-demo investor route evidence refresh`',
    "first phone version is landscape-only"
)
Assert-All -Text $currentGate -Label "current gate F79 plan marker" -Needles @(
    "check_f79_pc_route_evidence_refresh.ps1",
    "F79 PC route evidence refresh plan OK."
)
Assert-All -Text $queueScript -Label "queue F79/F80 marker" -Needles @(
    "F79 refresh PC controlled-demo investor route evidence after F77 audit fixes",
    $f80Task,
    '| F79 | Done | `Refresh PC controlled-demo investor route evidence after F77 audit fixes` |',
    '| F80 | Next | `Audit post-F79 PC controlled-demo investor route evidence refresh` |'
)
Assert-All -Text $handoffScript -Label "handoff script F79/F80 marker" -Needles @(
    "check_f79_pc_route_evidence_refresh.ps1",
    "F79 PC route evidence refresh check OK.",
    $f80Task
)
Assert-Contains -Text $gitignore -Needle $f79OutputRel -Label ".gitignore F79 output"
Assert-PathBudget -RelativePath $f79ScriptRel -MaxLength 96 -Label "F79 script path budget"
Assert-PathBudget -RelativePath $f79OutputRel -MaxLength 72 -Label "F79 output path budget"

if ($failures.Count -gt 0) {
    Write-Host "F79 PC route evidence refresh check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) F79 PC route evidence refresh check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "F79PCRouteEvidenceRefresh"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F79 refresh PC controlled-demo investor route evidence after F77 audit fixes"
    nextFormalTask = $f80Task
    noUnityLaunch = $true
    sourceCommandEvidenceReport = $commandReportRel
    sourceF77AuditReport = $f77AuditReportRel
    sourceF78FixReport = $f78FixReportRel
    refreshMarkers = $refreshMarkers
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# F79 PC Route Evidence Refresh")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F79 refresh PC controlled-demo investor route evidence after F77 audit fixes`')
$markdownLines.Add('Next formal task: `' + $f80Task + '`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source command evidence: `' + $commandReportRel + '`')
$markdownLines.Add('Source F77 audit: `' + $f77AuditReportRel + '`')
$markdownLines.Add('Source F78 fixes: `' + $f78FixReportRel + '`')
$markdownLines.Add("")
$markdownLines.Add("## Refresh Markers")
foreach ($marker in $refreshMarkers) {
    $markdownLines.Add("- $marker")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "F79 PC route evidence refresh check OK."
Write-Host "Report: $reportJsonPath"
