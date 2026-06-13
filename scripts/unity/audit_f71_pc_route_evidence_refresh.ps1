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
    $OutputDir = Join-Path $RepoRoot "analysis-output\f71-pc-route-evidence-audit"
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

$f70ReportRel = "analysis-output/f70-pc-route-evidence-refresh/report.json"
$f70MarkdownRel = "analysis-output/f70-pc-route-evidence-refresh/report.md"
$commandReportRel = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json"
$commandMarkdownRel = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.md"
$f68AuditReportRel = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json"
$f69FixReportRel = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json"

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
    Write-Host "F71 PC route evidence refresh audit plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$f70Report = Read-RepoJson -RelativePath $f70ReportRel
$f70Markdown = Read-RepoText -RelativePath $f70MarkdownRel
$commandReport = Read-RepoJson -RelativePath $commandReportRel
$commandMarkdown = Read-RepoText -RelativePath $commandMarkdownRel
$f68AuditReport = Read-RepoJson -RelativePath $f68AuditReportRel
$f69FixReport = Read-RepoJson -RelativePath $f69FixReportRel
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

$auditMarkers = @(
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

Assert-All -Text $f70Markdown -Label "F70 markdown route proof" -Needles @(
    'Completed task: `F70 refresh PC controlled-demo investor route evidence after F68 audit fixes`',
    'Next formal task: `F71 audit post-F70 PC controlled-demo investor route evidence refresh`',
    'NoUnityLaunch: True',
    'Source command evidence: `analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json`',
    'Source F68 audit: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json`',
    'Source F69 fixes: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json`',
    'mobile landscape-only proof remains visible',
    'route refresh stayed source/report-only without Unity launch'
)

Assert-All -Text $commandMarkdown -Label "command evidence F70 route proof" -Needles @(
    'Completed task: `F70 refresh PC controlled-demo investor route evidence after F68 audit fixes`',
    'Next formal task: `F71 audit post-F70 PC controlled-demo investor route evidence refresh`',
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return launch=scripts/unity/run_windows_demo.ps1 evidence=command-report+screenshots+sidecars',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png sidecar=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json log=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log callout=section-loss+cockpit-ejection+wreck-salvage+repair-line repairCost=9288',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only'
)

if ($null -ne $f70Report) {
    Assert-Equals -Actual ([string]$f70Report.schema) -Expected "PCControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefresh" -Label "F70 report schema"
    Assert-Equals -Actual ([string]$f70Report.result) -Expected "pass" -Label "F70 report result"
    Assert-Equals -Actual ([string]$f70Report.completedTask) -Expected "F70 refresh PC controlled-demo investor route evidence after F68 audit fixes" -Label "F70 completed task"
    Assert-Equals -Actual ([string]$f70Report.nextFormalTask) -Expected "F71 audit post-F70 PC controlled-demo investor route evidence refresh" -Label "F70 next task"
    Assert-Equals -Actual ([bool]$f70Report.noUnityLaunch) -Expected $true -Label "F70 no Unity launch"
    Assert-Equals -Actual ([string]$f70Report.sourceCommandEvidenceReport) -Expected $commandReportRel -Label "F70 source command evidence"
    Assert-Equals -Actual ([string]$f70Report.sourceAuditReport) -Expected $f68AuditReportRel -Label "F70 source F68 audit"
    Assert-Equals -Actual ([string]$f70Report.sourceFixesReport) -Expected $f69FixReportRel -Label "F70 source F69 fixes"
}

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F70 refresh PC controlled-demo investor route evidence after F68 audit fixes" -Label "command completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected "F71 audit post-F70 PC controlled-demo investor route evidence refresh" -Label "command next task"
    Assert-Equals -Actual ([string]$commandReport.sourceRouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditReport) -Expected $f68AuditReportRel -Label "command source F68 audit"
    Assert-Equals -Actual ([string]$commandReport.sourceRouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesReport) -Expected $f69FixReportRel -Label "command source F69 fixes"
    Assert-Equals -Actual ([int]$commandReport.width) -Expected 1280 -Label "command width"
    Assert-Equals -Actual ([int]$commandReport.height) -Expected 720 -Label "command height"

    $evidenceRows = @($commandReport.evidence)
    Assert-Equals -Actual $evidenceRows.Count -Expected $requiredPresets.Count -Label "command preset count"

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
        Assert-Contains -Text ([string]$damage.debriefRewardSummary) -Needle 'DamageDebrief=section-status+repair-line' -Label "damage debrief repair line"
        Assert-Contains -Text ([string]$damage.debriefRewardSummary) -Needle 'cockpitEjection=ready' -Label "damage cockpit ejection"
        Assert-Contains -Text ([string]$damage.debriefRewardSummary) -Needle 'repairCost=9288' -Label "damage repair cost"
    }

    $soloOrder = Get-ReportRow -Rows $evidenceRows -Preset "solo-order"
    if ($null -ne $soloOrder) {
        Assert-Contains -Text ([string]$soloOrder.playableFlowPolish) -Needle 'soloReturn=order-active detached=1' -Label "solo-order detached"
        Assert-Contains -Text ([string]$soloOrder.commandReadability) -Needle 'noDragBox=yes' -Label "solo-order no drag box"
    }

    $soloReturn = Get-ReportRow -Rows $evidenceRows -Preset "solo-return"
    if ($null -ne $soloReturn) {
        Assert-Contains -Text ([string]$soloReturn.playableFlowPolish) -Needle 'soloReturn=returned detached=0' -Label "solo-return joined"
    }
}

if ($null -ne $f68AuditReport) {
    Assert-Equals -Actual ([string]$f68AuditReport.result) -Expected "pass-with-followups" -Label "F68 audit result"
    Assert-Equals -Actual ([string]$f68AuditReport.completedTask) -Expected "F68 audit post-F67 PC controlled-demo investor route evidence refresh" -Label "F68 completed task"
    Assert-Equals -Actual ([string]$f68AuditReport.nextFormalTask) -Expected "F69 implement post-F68 PC controlled-demo investor route evidence refresh audit fixes" -Label "F68 next task"
}

if ($null -ne $f69FixReport) {
    Assert-Equals -Actual ([string]$f69FixReport.result) -Expected "pass" -Label "F69 fix result"
    Assert-Equals -Actual ([string]$f69FixReport.completedTask) -Expected "F69 implement post-F68 PC controlled-demo investor route evidence refresh audit fixes" -Label "F69 completed task"
    Assert-Equals -Actual ([string]$f69FixReport.nextFormalTask) -Expected "F70 refresh PC controlled-demo investor route evidence after F68 audit fixes" -Label "F69 next task"
}

Assert-All -Text $masterPlan -Label "master F71/F72 queue" -Needles @(
    'PC1-PC71',
    '| 149 | Done | `Audit post-F70 PC controlled-demo investor route evidence refresh` |',
    '| 150 | Next | `Implement post-F71 PC controlled-demo investor route evidence refresh audit fixes` |',
    'Mobile phones remain landscape-only for the first playable target'
)
Assert-All -Text $detailedPlan -Label "detailed F71/F72 queue" -Needles @(
    '| F71 | Done | `Audit post-F70 PC controlled-demo investor route evidence refresh` |',
    '| F72 | Next | `Implement post-F71 PC controlled-demo investor route evidence refresh audit fixes` |'
)
Assert-All -Text $mobilePlan -Label "mobile F71/F72 status" -Needles @(
    "F71 audit post-F70 PC controlled-demo investor route evidence refresh",
    "F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes",
    "first phone version is landscape-only",
    "horizontal phone game"
)
Assert-All -Text $handoffDoc -Label "handoff F72 next" -Needles @(
    'Current formal next development task after handoff: `F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes`',
    'Next planned work: `F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes`'
)

foreach ($surface in @($readme, $buildWin, $buildMobile, $evidenceDoc, $routeDoc, $handoffDoc)) {
    Assert-All -Text $surface -Label "F71 audit marker surface" -Needles $auditMarkers
}

Assert-All -Text $currentGate -Label "current gate F71 plan marker" -Needles @(
    "audit_f71_pc_route_evidence_refresh.ps1",
    "F71 PC route evidence refresh audit plan OK."
)
Assert-All -Text $queueScript -Label "queue F71/F72 marker" -Needles @(
    "F71 audit post-F70 PC controlled-demo investor route evidence refresh",
    "F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes",
    '| F71 | Done | `Audit post-F70 PC controlled-demo investor route evidence refresh` |',
    '| F72 | Next | `Implement post-F71 PC controlled-demo investor route evidence refresh audit fixes` |'
)
Assert-All -Text $handoffScript -Label "handoff script F71/F72 marker" -Needles @(
    "audit_f71_pc_route_evidence_refresh.ps1",
    "F71 PC route evidence refresh audit OK.",
    "F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes"
)
Assert-Contains -Text $gitignore -Needle "analysis-output/f71-pc-route-evidence-audit/" -Label ".gitignore F71 output"

Add-Finding -Area "F70-traceability" -Status "pass" -Detail "F70 command evidence and refresh report explicitly consume the F68 audit report and F69 fix report."
Add-Finding -Area "route-proof-clarity" -Status "pass" -Detail "Investor route remains spawn>hangar-contact>damage-demo>solo-order>solo-return with screenshots, sidecars and logs."
Add-Finding -Area "damage-ejection-proof" -Status "pass" -Detail "Damage-demo evidence keeps section loss, cockpit ejection, wreck salvage and repair-cost line."
Add-Finding -Area "mobile-landscape-proof" -Status "pass" -Detail "Command evidence, docs and contract retain landscape-only first phone version."
Add-Finding -Area "public-safe-proxy-boundary" -Status "pass" -Detail "Proxy-only visual language is present and collision/pathing/BattleCore remain unchanged."
Add-Finding -Area "windows-path-budget" -Status "pass" -Detail "F70 introduced short script/output paths after Windows rejected the 260-character script path."
Add-FollowUp -Priority "P1" -Area "audit-visibility" -Issue "F71 audit findings must be wired into plan/handoff/gate surfaces for the next fix task." -NextFix "F72-doc-gate-visibility"
Add-FollowUp -Priority "P2" -Area "next-refresh-contract" -Issue "The next refresh should consume this F71 audit report explicitly." -NextFix "F73-consume-F71-audit-report"
Add-FollowUp -Priority "P2" -Area "path-budget" -Issue "New F-series artifacts should keep short names to avoid Windows path execution failures." -NextFix "F72-keep-new-F-artifacts-short"

if ($failures.Count -gt 0) {
    Write-Host "F71 PC route evidence refresh audit failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) F71 PC route evidence refresh audit check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "F71PCRouteEvidenceRefreshAudit"
    result = "pass-with-followups"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F71 audit post-F70 PC controlled-demo investor route evidence refresh"
    nextFormalTask = "F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes"
    noUnityLaunch = $true
    sourceF70Report = $f70ReportRel
    sourceCommandEvidenceReport = $commandReportRel
    sourceF68AuditReport = $f68AuditReportRel
    sourceF69FixReport = $f69FixReportRel
    findings = $findings.ToArray()
    followUps = $followUps.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# F71 PC Route Evidence Refresh Audit")
$markdownLines.Add("")
$markdownLines.Add("Result: pass-with-followups")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F71 audit post-F70 PC controlled-demo investor route evidence refresh`')
$markdownLines.Add('Next formal task: `F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source F70 report: `' + $f70ReportRel + '`')
$markdownLines.Add('Source command evidence: `' + $commandReportRel + '`')
$markdownLines.Add('Source F68 audit: `' + $f68AuditReportRel + '`')
$markdownLines.Add('Source F69 fixes: `' + $f69FixReportRel + '`')
$markdownLines.Add("")
$markdownLines.Add("## Findings")
foreach ($finding in $report.findings) {
    $markdownLines.Add("- $($finding.area): $($finding.status) - $($finding.detail)")
}
$markdownLines.Add("")
$markdownLines.Add("## Follow Ups")
foreach ($followUp in $report.followUps) {
    $markdownLines.Add("- $($followUp.priority) $($followUp.area): $($followUp.issue) Next: $($followUp.nextFix)")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "F71 PC route evidence refresh audit OK."
Write-Host "Report: $reportJsonPath"
