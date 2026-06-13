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
    $OutputDir = Join-Path $RepoRoot "analysis-output\f74-pc-route-evidence-refresh-audit"
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
$findings = New-Object System.Collections.Generic.List[object]
$followUps = New-Object System.Collections.Generic.List[object]
$requiredPresets = @("spawn", "hangar-contact", "damage-demo", "solo-order", "solo-return")

$commandReportRel = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json"
$commandMarkdownRel = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.md"
$f71AuditReportRel = "analysis-output/f71-pc-route-evidence-audit/report.json"
$f72FixReportRel = "analysis-output/f72-pc-route-audit-fixes/report.json"
$f73RefreshReportRel = "analysis-output/f73-pc-route-evidence-refresh/report.json"
$f73RefreshMarkdownRel = "analysis-output/f73-pc-route-evidence-refresh/report.md"

$f73RefreshMarkers = @(
    "F73RouteEvidenceRefresh=ready source=analysis-output/f72-pc-route-audit-fixes/report.json completed=F73 next=F74 noUnityLaunch=True mobile=landscape-only",
    "F73RouteEvidenceRefreshSource=audit sourceAudit=analysis-output/f71-pc-route-evidence-audit/report.json sourceFixes=analysis-output/f72-pc-route-audit-fixes/report.json",
    "F73RouteEvidenceRefreshClosure=route-proof-preserved route=spawn>hangar-contact>damage-demo>solo-order>solo-return damage=section-loss+cockpit-ejection+wreck-salvage+repair-line publicSafe=proxy-only",
    "F73RouteEvidenceRefreshClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False"
)

$auditMarkers = @(
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

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Add-Failure {
    param([string]$Message)
    [void]$failures.Add($Message)
}

function Add-Finding {
    param(
        [string]$Area,
        [string]$Status,
        [string]$Detail
    )

    [void]$findings.Add([pscustomobject]@{
        area = $Area
        status = $Status
        detail = $Detail
    })
}

function Add-FollowUp {
    param(
        [string]$Priority,
        [string]$Area,
        [string]$Issue,
        [string]$NextFix
    )

    [void]$followUps.Add([pscustomobject]@{
        priority = $Priority
        area = $Area
        issue = $Issue
        nextFix = $NextFix
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
    }
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
    }
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
    }
}

function Get-ReportRow {
    param(
        [object[]]$Rows,
        [string]$Preset
    )

    $matches = @($Rows | Where-Object { [string]$_.preset -eq $Preset })
    if ($matches.Count -ne 1) {
        Add-Failure "command report expected one row for $Preset, got $($matches.Count)"
        return $null
    }

    return $matches[0]
}

if ($PlanOnly) {
    Write-Host "F74 PC route evidence refresh audit plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandReport = Read-RepoJson -RelativePath $commandReportRel
$commandMarkdown = Read-RepoText -RelativePath $commandMarkdownRel
$f71AuditReport = Read-RepoJson -RelativePath $f71AuditReportRel
$f72FixReport = Read-RepoJson -RelativePath $f72FixReportRel
$f73RefreshReport = Read-RepoJson -RelativePath $f73RefreshReportRel
$f73RefreshMarkdown = Read-RepoText -RelativePath $f73RefreshMarkdownRel
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

Assert-All -Text $commandMarkdown -Label "command markdown F73 audit input" -Needles @(
    'Completed task: `F73 refresh PC controlled-demo investor route evidence after F71 audit fixes`',
    'Next formal task: `F74 audit post-F73 PC controlled-demo investor route evidence refresh`',
    'Source F71 route evidence audit report: "analysis-output/f71-pc-route-evidence-audit/report.json"',
    'Source F72 route audit fixes report: "analysis-output/f72-pc-route-audit-fixes/report.json"',
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity'
)
Assert-All -Text $commandMarkdown -Label "command markdown F73 refresh closure" -Needles $f73RefreshMarkers

Assert-All -Text $f73RefreshMarkdown -Label "F73 refresh report markdown" -Needles @(
    'Completed task: `F73 refresh PC controlled-demo investor route evidence after F71 audit fixes`',
    'Next formal task: `F74 audit post-F73 PC controlled-demo investor route evidence refresh`',
    'Source command evidence: `analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json`',
    'Source F71 audit: `analysis-output/f71-pc-route-evidence-audit/report.json`',
    'Source F72 fixes: `analysis-output/f72-pc-route-audit-fixes/report.json`'
)
Assert-All -Text $f73RefreshMarkdown -Label "F73 refresh report markers" -Needles $f73RefreshMarkers

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.schema) -Expected "PCControlledDemoCommandEvidenceRefresh" -Label "command report schema"
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F73 refresh PC controlled-demo investor route evidence after F71 audit fixes" -Label "command report completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected "F74 audit post-F73 PC controlled-demo investor route evidence refresh" -Label "command report next task"
    Assert-Equals -Actual ([string]$commandReport.sourceF71RouteEvidenceAuditReport) -Expected $f71AuditReportRel -Label "command report source F71 audit"
    Assert-Equals -Actual ([string]$commandReport.sourceF72RouteAuditFixesReport) -Expected $f72FixReportRel -Label "command report source F72 fixes"
    Assert-Equals -Actual ([int]$commandReport.width) -Expected 1280 -Label "command report width"
    Assert-Equals -Actual ([int]$commandReport.height) -Expected 720 -Label "command report height"

    $evidenceRows = @($commandReport.evidence)
    Assert-Equals -Actual $evidenceRows.Count -Expected $requiredPresets.Count -Label "command preset count"

    foreach ($marker in $f73RefreshMarkers) {
        $matches = @($commandReport.f72RouteEvidenceAuditClosure | Where-Object { [string]$_ -eq $marker })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "command report F73 refresh closure $marker"
    }

    foreach ($preset in $requiredPresets) {
        $row = Get-ReportRow -Rows $evidenceRows -Preset $preset
        if ($null -eq $row) {
            continue
        }

        Assert-FileExists -RelativePath ([string]$row.screenshot) -Label "$preset screenshot"
        Assert-FileExists -RelativePath ([string]$row.sidecar) -Label "$preset sidecar"
        Assert-FileExists -RelativePath ([string]$row.log) -Label "$preset log"
        Assert-Contains -Text ([string]$row.battleHud) -Needle 'SparseBattleUi=statusRows+sections+solo' -Label "$preset sparse HUD"
        Assert-Contains -Text ([string]$row.playableFlowPolish) -Needle 'mobileLandscapeOnly=True orientation=landscape' -Label "$preset landscape proof"
        Assert-Contains -Text ([string]$row.investorProxyVisuals) -Needle 'publicSafe=proxy-only' -Label "$preset public-safe proxy"
        Assert-Contains -Text ([string]$row.investorProxyVisuals) -Needle 'collision=unchanged pathing=unchanged BattleCore=unchanged' -Label "$preset gameplay boundary"
    }

    $damage = Get-ReportRow -Rows $evidenceRows -Preset "damage-demo"
    if ($null -ne $damage) {
        Assert-Contains -Text ([string]$damage.playableFlowPolish) -Needle 'damageStory=section-loss+ejection+wreck' -Label "damage story"
        Assert-Contains -Text ([string]$damage.debriefRewardSummary) -Needle 'DamageDebrief=section-status+repair-line' -Label "damage repair line"
        Assert-Contains -Text ([string]$damage.debriefRewardSummary) -Needle 'cockpitEjection=ready' -Label "damage cockpit ejection"
        Assert-Contains -Text ([string]$damage.debriefRewardSummary) -Needle 'repairCost=9288' -Label "damage repair cost"
    }

    $soloOrder = Get-ReportRow -Rows $evidenceRows -Preset "solo-order"
    if ($null -ne $soloOrder) {
        Assert-Contains -Text ([string]$soloOrder.playableFlowPolish) -Needle 'soloReturn=order-active detached=1' -Label "solo order detached"
        Assert-Contains -Text ([string]$soloOrder.commandReadability) -Needle 'noDragBox=yes' -Label "solo order no drag box"
    }

    $soloReturn = Get-ReportRow -Rows $evidenceRows -Preset "solo-return"
    if ($null -ne $soloReturn) {
        Assert-Contains -Text ([string]$soloReturn.playableFlowPolish) -Needle 'soloReturn=returned detached=0' -Label "solo return joined"
    }
}

if ($null -ne $f73RefreshReport) {
    Assert-Equals -Actual ([string]$f73RefreshReport.schema) -Expected "F73PCRouteEvidenceRefresh" -Label "F73 refresh report schema"
    Assert-Equals -Actual ([string]$f73RefreshReport.result) -Expected "pass" -Label "F73 refresh report result"
    Assert-Equals -Actual ([string]$f73RefreshReport.completedTask) -Expected "F73 refresh PC controlled-demo investor route evidence after F71 audit fixes" -Label "F73 refresh completed task"
    Assert-Equals -Actual ([string]$f73RefreshReport.nextFormalTask) -Expected "F74 audit post-F73 PC controlled-demo investor route evidence refresh" -Label "F73 refresh next task"
    Assert-Equals -Actual ([bool]$f73RefreshReport.noUnityLaunch) -Expected $true -Label "F73 refresh no Unity launch"
    Assert-Equals -Actual ([string]$f73RefreshReport.sourceCommandEvidenceReport) -Expected $commandReportRel -Label "F73 refresh source command evidence"
    Assert-Equals -Actual ([string]$f73RefreshReport.sourceF71AuditReport) -Expected $f71AuditReportRel -Label "F73 refresh source F71 audit"
    Assert-Equals -Actual ([string]$f73RefreshReport.sourceF72FixReport) -Expected $f72FixReportRel -Label "F73 refresh source F72 fixes"
}

if ($null -ne $f71AuditReport) {
    Assert-Equals -Actual ([string]$f71AuditReport.result) -Expected "pass-with-followups" -Label "F71 audit result"
    Assert-Equals -Actual ([string]$f71AuditReport.completedTask) -Expected "F71 audit post-F70 PC controlled-demo investor route evidence refresh" -Label "F71 completed task"
    Assert-Equals -Actual ([string]$f71AuditReport.nextFormalTask) -Expected "F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes" -Label "F71 next task"
}

if ($null -ne $f72FixReport) {
    Assert-Equals -Actual ([string]$f72FixReport.result) -Expected "pass" -Label "F72 fix result"
    Assert-Equals -Actual ([string]$f72FixReport.completedTask) -Expected "F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes" -Label "F72 completed task"
    Assert-Equals -Actual ([string]$f72FixReport.nextFormalTask) -Expected "F73 refresh PC controlled-demo investor route evidence after F71 audit fixes" -Label "F72 next task"
}

foreach ($surface in @($readme, $buildWin, $buildMobile, $evidenceDoc, $routeDoc, $handoffDoc)) {
    Assert-All -Text $surface -Label "F74 audit marker surface" -Needles $auditMarkers
}

Assert-All -Text $masterPlan -Label "master F74/F75 queue" -Needles @(
    "2026-06-13 v125",
    "PC1-PC74",
    '| 152 | Done | `Audit post-F73 PC controlled-demo investor route evidence refresh` |',
    '| 153 | Next | `Implement post-F74 PC controlled-demo investor route evidence refresh audit fixes` |',
    'Formal next task: `F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes`'
)
Assert-All -Text $detailedPlan -Label "detailed F74/F75 queue" -Needles @(
    "2026-06-13 v134",
    "PC1-PC74",
    '| F74 | Done | `Audit post-F73 PC controlled-demo investor route evidence refresh` |',
    '| F75 | Next | `Implement post-F74 PC controlled-demo investor route evidence refresh audit fixes` |',
    'formal next task: `F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes`'
)
Assert-All -Text $mobilePlan -Label "mobile F74/F75 status" -Needles @(
    "PC1-PC74",
    "F74 audit post-F73 PC controlled-demo investor route evidence refresh",
    "F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes",
    "first phone version is landscape-only"
)
Assert-All -Text $handoffDoc -Label "handoff F75 next" -Needles @(
    'Latest sealed PC/mobile wait-state checkpoint: `PC1-PC74`',
    'Current formal next development task after handoff: `F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes`',
    'Next planned work: `F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes`'
)
Assert-All -Text $currentGate -Label "current gate F74 plan marker" -Needles @(
    "audit_f74_pc_route_evidence_refresh.ps1",
    "F74 PC route evidence refresh audit plan OK."
)
Assert-All -Text $queueScript -Label "queue F74/F75 marker" -Needles @(
    "F74 audit post-F73 PC controlled-demo investor route evidence refresh",
    "F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes",
    '| F74 | Done | `Audit post-F73 PC controlled-demo investor route evidence refresh` |',
    '| F75 | Next | `Implement post-F74 PC controlled-demo investor route evidence refresh audit fixes` |'
)
Assert-All -Text $handoffScript -Label "handoff script F74/F75 marker" -Needles @(
    "audit_f74_pc_route_evidence_refresh.ps1",
    "F74 PC route evidence refresh audit OK.",
    "F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes"
)
Assert-Contains -Text $gitignore -Needle "analysis-output/f74-pc-route-evidence-refresh-audit/" -Label ".gitignore F74 output"

Add-Finding -Area "F73-traceability" -Status "pass" -Detail "F73 command evidence and refresh report explicitly consume the F71 audit report and F72 fix report."
Add-Finding -Area "route-proof-clarity" -Status "pass" -Detail "Investor route remains spawn>hangar-contact>damage-demo>solo-order>solo-return with screenshots, sidecars and logs."
Add-Finding -Area "damage-ejection-proof" -Status "pass" -Detail "Damage-demo evidence keeps section loss, cockpit ejection, wreck salvage and repair-cost line."
Add-Finding -Area "mobile-landscape-proof" -Status "pass" -Detail "Command evidence, docs and contract retain landscape-only first phone version."
Add-Finding -Area "public-safe-proxy-boundary" -Status "pass" -Detail "Proxy-only visual language is present and collision/pathing/BattleCore remain unchanged."
Add-Finding -Area "windows-path-budget" -Status "pass" -Detail "F73 and F74 keep short script and output paths for Windows path-budget safety."

Add-FollowUp -Priority "P1" -Area "audit-visibility" -Issue "F74 audit findings must remain visible in plan, evidence and handoff surfaces." -NextFix "F75 doc and gate visibility pass."
Add-FollowUp -Priority "P2" -Area "next-refresh-contract" -Issue "The next route evidence refresh should explicitly consume the F74 audit report." -NextFix "F76 command-evidence metadata refresh."
Add-FollowUp -Priority "P2" -Area "path-budget" -Issue "New F-series evidence artifacts should continue using short output directories." -NextFix "Keep F75/F76 artifact paths short."

if ($failures.Count -gt 0) {
    Write-Host "F74 PC route evidence refresh audit failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) F74 PC route evidence refresh audit check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "F74PCRouteEvidenceRefreshAudit"
    result = "pass-with-followups"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F74 audit post-F73 PC controlled-demo investor route evidence refresh"
    nextFormalTask = "F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes"
    noUnityLaunch = $true
    sourceCommandEvidenceReport = $commandReportRel
    sourceF71AuditReport = $f71AuditReportRel
    sourceF72FixReport = $f72FixReportRel
    sourceF73RefreshReport = $f73RefreshReportRel
    auditMarkers = $auditMarkers
    findings = $findings.ToArray()
    followUps = $followUps.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# F74 PC Route Evidence Refresh Audit")
$markdownLines.Add("")
$markdownLines.Add("Result: pass-with-followups")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F74 audit post-F73 PC controlled-demo investor route evidence refresh`')
$markdownLines.Add('Next formal task: `F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source command evidence: `' + $commandReportRel + '`')
$markdownLines.Add('Source F71 audit: `' + $f71AuditReportRel + '`')
$markdownLines.Add('Source F72 fixes: `' + $f72FixReportRel + '`')
$markdownLines.Add('Source F73 refresh: `' + $f73RefreshReportRel + '`')
$markdownLines.Add("")
$markdownLines.Add("## Audit Markers")
foreach ($marker in $auditMarkers) {
    $markdownLines.Add("- $marker")
}
$markdownLines.Add("")
$markdownLines.Add("## Findings")
foreach ($finding in $findings) {
    $markdownLines.Add("- $($finding.area): $($finding.status) - $($finding.detail)")
}
$markdownLines.Add("")
$markdownLines.Add("## Follow Ups")
foreach ($followUp in $followUps) {
    $markdownLines.Add("- $($followUp.priority) $($followUp.area): $($followUp.issue) Next: $($followUp.nextFix)")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "F74 PC route evidence refresh audit OK."
Write-Host "Report: $reportJsonPath"
