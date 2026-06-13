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
    $OutputDir = Join-Path $RepoRoot "analysis-output\f73-pc-route-evidence-refresh"
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
$f71AuditReportRel = "analysis-output/f71-pc-route-evidence-audit/report.json"
$f72FixReportRel = "analysis-output/f72-pc-route-audit-fixes/report.json"
$f73OutputRel = "analysis-output/f73-pc-route-evidence-refresh/"

$refreshMarkers = @(
    "F73RouteEvidenceRefresh=ready source=analysis-output/f72-pc-route-audit-fixes/report.json completed=F73 next=F74 noUnityLaunch=True mobile=landscape-only",
    "F73RouteEvidenceRefreshSource=audit sourceAudit=analysis-output/f71-pc-route-evidence-audit/report.json sourceFixes=analysis-output/f72-pc-route-audit-fixes/report.json",
    "F73RouteEvidenceRefreshClosure=route-proof-preserved route=spawn>hangar-contact>damage-demo>solo-order>solo-return damage=section-loss+cockpit-ejection+wreck-salvage+repair-line publicSafe=proxy-only",
    "F73RouteEvidenceRefreshClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False"
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

if ($PlanOnly) {
    Write-Host "F73 PC route evidence refresh plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandCapture = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$commandMarkdown = Read-RepoText -RelativePath $commandMarkdownRel
$commandReport = Read-RepoJson -RelativePath $commandReportRel
$f71AuditReport = Read-RepoJson -RelativePath $f71AuditReportRel
$f72FixReport = Read-RepoJson -RelativePath $f72FixReportRel
$masterPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RepoText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
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

Assert-All -Text $commandCapture -Label "command capture F73 metadata" -Needles @(
    'F73 refresh PC controlled-demo investor route evidence after F71 audit fixes',
    'F74 audit post-F73 PC controlled-demo investor route evidence refresh',
    '$f71RouteEvidenceAuditReportPath',
    '$f72RouteAuditFixesReportPath',
    'sourceF71RouteEvidenceAuditReport',
    'sourceF72RouteAuditFixesReport',
    'f72RouteEvidenceAuditClosure'
)

Assert-All -Text $commandMarkdown -Label "command markdown F73 route evidence" -Needles @(
    'Completed task: `F73 refresh PC controlled-demo investor route evidence after F71 audit fixes`',
    'Next formal task: `F74 audit post-F73 PC controlled-demo investor route evidence refresh`',
    'Source F71 route evidence audit report: "analysis-output/f71-pc-route-evidence-audit/report.json"',
    'Source F72 route audit fixes report: "analysis-output/f72-pc-route-audit-fixes/report.json"',
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity'
)
Assert-All -Text $commandMarkdown -Label "command markdown F73 closure markers" -Needles $refreshMarkers

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.schema) -Expected "PCControlledDemoCommandEvidenceRefresh" -Label "command report schema"
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F73 refresh PC controlled-demo investor route evidence after F71 audit fixes" -Label "command report completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected "F74 audit post-F73 PC controlled-demo investor route evidence refresh" -Label "command report next task"
    Assert-Equals -Actual ([string]$commandReport.sourceF71RouteEvidenceAuditReport) -Expected $f71AuditReportRel -Label "command report source F71 audit"
    Assert-Equals -Actual ([string]$commandReport.sourceF72RouteAuditFixesReport) -Expected $f72FixReportRel -Label "command report source F72 fixes"
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
        }
    }

    foreach ($marker in $refreshMarkers) {
        $matches = @($commandReport.f72RouteEvidenceAuditClosure | Where-Object { [string]$_ -eq $marker })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "command report F73 closure $marker"
    }
}

if ($null -ne $f71AuditReport) {
    Assert-Equals -Actual ([string]$f71AuditReport.result) -Expected "pass-with-followups" -Label "F71 audit result"
    Assert-Equals -Actual ([string]$f71AuditReport.completedTask) -Expected "F71 audit post-F70 PC controlled-demo investor route evidence refresh" -Label "F71 completed task"
    Assert-Equals -Actual ([string]$f71AuditReport.nextFormalTask) -Expected "F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes" -Label "F71 next task"
}

if ($null -ne $f72FixReport) {
    Assert-Equals -Actual ([string]$f72FixReport.result) -Expected "pass" -Label "F72 fixes result"
    Assert-Equals -Actual ([string]$f72FixReport.completedTask) -Expected "F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes" -Label "F72 completed task"
    Assert-Equals -Actual ([string]$f72FixReport.nextFormalTask) -Expected "F73 refresh PC controlled-demo investor route evidence after F71 audit fixes" -Label "F72 next task"
}

foreach ($surface in @($readme, $buildWin, $buildMobile, $evidenceDoc, $routeDoc, $handoffDoc)) {
    Assert-All -Text $surface -Label "F73 route evidence refresh surface" -Needles $refreshMarkers
}

Assert-All -Text $masterPlan -Label "master F73/F74 queue" -Needles @(
    "2026-06-13 v124",
    "PC1-PC73",
    '| 151 | Done | `Refresh PC controlled-demo investor route evidence after F71 audit fixes` |',
    '| 152 | Next | `Audit post-F73 PC controlled-demo investor route evidence refresh` |',
    'Formal next task: `F74 audit post-F73 PC controlled-demo investor route evidence refresh`'
)
Assert-All -Text $detailedPlan -Label "detailed F73/F74 queue" -Needles @(
    "2026-06-13 v133",
    "PC1-PC73",
    '| F73 | Done | `Refresh PC controlled-demo investor route evidence after F71 audit fixes` |',
    '| F74 | Next | `Audit post-F73 PC controlled-demo investor route evidence refresh` |',
    'formal next task: `F74 audit post-F73 PC controlled-demo investor route evidence refresh`'
)
Assert-All -Text $mobilePlan -Label "mobile F73/F74 status" -Needles @(
    "PC1-PC73",
    "F73 refresh PC controlled-demo investor route evidence after F71 audit fixes",
    "F74 audit post-F73 PC controlled-demo investor route evidence refresh",
    "first phone version is landscape-only"
)
Assert-All -Text $handoffDoc -Label "handoff F74 next" -Needles @(
    'Latest sealed PC/mobile wait-state checkpoint: `PC1-PC73`',
    'Current formal next development task after handoff: `F74 audit post-F73 PC controlled-demo investor route evidence refresh`',
    'Next planned work: `F74 audit post-F73 PC controlled-demo investor route evidence refresh`'
)
Assert-All -Text $currentGate -Label "current gate F73 plan marker" -Needles @(
    "check_f73_pc_route_evidence_refresh.ps1",
    "F73 PC route evidence refresh plan OK."
)
Assert-All -Text $queueScript -Label "queue F73/F74 marker" -Needles @(
    "F73 refresh PC controlled-demo investor route evidence after F71 audit fixes",
    "F74 audit post-F73 PC controlled-demo investor route evidence refresh",
    '| F73 | Done | `Refresh PC controlled-demo investor route evidence after F71 audit fixes` |',
    '| F74 | Next | `Audit post-F73 PC controlled-demo investor route evidence refresh` |'
)
Assert-All -Text $handoffScript -Label "handoff script F73/F74 marker" -Needles @(
    "check_f73_pc_route_evidence_refresh.ps1",
    "F73 PC route evidence refresh check OK.",
    "F74 audit post-F73 PC controlled-demo investor route evidence refresh"
)
Assert-Contains -Text $gitignore -Needle $f73OutputRel -Label ".gitignore F73 output"

if ($failures.Count -gt 0) {
    Write-Host "F73 PC route evidence refresh check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) F73 PC route evidence refresh check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "F73PCRouteEvidenceRefresh"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F73 refresh PC controlled-demo investor route evidence after F71 audit fixes"
    nextFormalTask = "F74 audit post-F73 PC controlled-demo investor route evidence refresh"
    noUnityLaunch = $true
    sourceCommandEvidenceReport = $commandReportRel
    sourceF71AuditReport = $f71AuditReportRel
    sourceF72FixReport = $f72FixReportRel
    refreshMarkers = $refreshMarkers
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# F73 PC Route Evidence Refresh")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F73 refresh PC controlled-demo investor route evidence after F71 audit fixes`')
$markdownLines.Add('Next formal task: `F74 audit post-F73 PC controlled-demo investor route evidence refresh`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source command evidence: `' + $commandReportRel + '`')
$markdownLines.Add('Source F71 audit: `' + $f71AuditReportRel + '`')
$markdownLines.Add('Source F72 fixes: `' + $f72FixReportRel + '`')
$markdownLines.Add("")
$markdownLines.Add("## Refresh Markers")
foreach ($marker in $refreshMarkers) {
    $markdownLines.Add("- $marker")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "F73 PC route evidence refresh check OK."
Write-Host "Report: $reportJsonPath"
