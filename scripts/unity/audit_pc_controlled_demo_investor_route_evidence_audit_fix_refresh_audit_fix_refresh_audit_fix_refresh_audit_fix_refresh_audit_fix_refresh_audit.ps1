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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit"
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
$requiredPresets = @("spawn", "hangar-contact", "damage-demo", "solo-order", "solo-return")
$failures = New-Object System.Collections.Generic.List[string]
$findings = New-Object System.Collections.Generic.List[object]
$followUps = New-Object System.Collections.Generic.List[object]

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
    Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.md"
$commandReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.json"
$f61RefreshReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh\report.json"
$f61RefreshMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh\report.md"
$f59AuditReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit\report.json"
$f60FixReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes\report.json"
$commandCaptureScript = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$masterPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RepoText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$handoffDoc = Read-RepoText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$evidenceDoc = Read-RepoText -RelativePath "docs-playable-demo-investor-evidence-2026-06-07.md"
$routeDoc = Read-RepoText -RelativePath "docs-pc-investor-demo-route-2026-06-13.md"
$queueScript = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$gitignore = Read-RepoText -RelativePath ".gitignore"

$closureMarkers = @(
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditRefresh=ready source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json completed=F61 next=F62 noUnityLaunch=True mobile=landscape-only",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=F60-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json sourceFixes=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json"
)

Assert-All -Text $commandCaptureScript -Label "command capture F61/F62 source contract" -Needles @(
    'F61 refresh PC controlled-demo investor route evidence after F59 audit fixes',
    'F62 audit post-F61 PC controlled-demo investor route evidence refresh',
    'sourceRouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditReport = Convert-ToRepoRelativePath -Path $routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditReportPath',
    'sourceRouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesReport = Convert-ToRepoRelativePath -Path $routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesReportPath',
    'routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure = $routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure',
    '## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Closure'
)
Assert-All -Text $commandCaptureScript -Label "command capture F61 closure source" -Needles $closureMarkers

Assert-All -Text $commandMarkdown -Label "command markdown F61 route evidence" -Needles @(
    'Completed task: `F61 refresh PC controlled-demo investor route evidence after F59 audit fixes`',
    'Next formal task: `F62 audit post-F61 PC controlled-demo investor route evidence refresh`',
    'Source route audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit report: "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json"',
    'Source route audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes report: "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json"',
    '## Investor Route Summary',
    '## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Closure',
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return launch=scripts/unity/run_windows_demo.ps1 evidence=command-report+screenshots+sidecars',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png sidecar=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json log=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log callout=section-loss+cockpit-ejection+wreck-salvage+repair-line repairCost=9288',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only'
)
Assert-All -Text $commandMarkdown -Label "command markdown F61 closure markers" -Needles $closureMarkers

Assert-All -Text $f61RefreshMarkdown -Label "F61 refresh gate markdown" -Needles @(
    'Completed task: `F61 refresh PC controlled-demo investor route evidence after F59 audit fixes`',
    'Next formal task: `F62 audit post-F61 PC controlled-demo investor route evidence refresh`',
    'NoUnityLaunch: True',
    'command evidence metadata advanced to F61/F62',
    'F59 audit report is an explicit command evidence source',
    'F60 fix report is an explicit command evidence source',
    'mobile landscape-only proof remains visible'
)

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F61 refresh PC controlled-demo investor route evidence after F59 audit fixes" -Label "command report completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected "F62 audit post-F61 PC controlled-demo investor route evidence refresh" -Label "command report next task"
    Assert-Equals -Actual ([string]$commandReport.sourceRouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditReport) -Expected "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json" -Label "command report source F59 audit"
    Assert-Equals -Actual ([string]$commandReport.sourceRouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesReport) -Expected "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json" -Label "command report source F60 fixes"
    Assert-Equals -Actual ([int]$commandReport.width) -Expected 1280 -Label "command report width"
    Assert-Equals -Actual ([int]$commandReport.height) -Expected 720 -Label "command report height"

    foreach ($marker in $closureMarkers) {
        $matches = @($commandReport.routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure | Where-Object { [string]$_ -eq $marker })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "command report F61 closure $marker"
    }

    $evidenceRows = @($commandReport.evidence)
    Assert-Equals -Actual $evidenceRows.Count -Expected $requiredPresets.Count -Label "command report preset count"
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
        Assert-Contains -Text ([string]$soloOrder.playableFlowPolish) -Needle 'soloReturn=order-active detached=1' -Label "solo-order detached state"
        Assert-Contains -Text ([string]$soloOrder.commandReadability) -Needle 'noDragBox=yes' -Label "solo-order no drag box"
    }

    $soloReturn = Get-ReportRow -Rows $evidenceRows -Preset "solo-return"
    if ($null -ne $soloReturn) {
        Assert-Contains -Text ([string]$soloReturn.playableFlowPolish) -Needle 'soloReturn=returned detached=0' -Label "solo-return joined state"
    }
}

if ($null -ne $f61RefreshReport) {
    Assert-Equals -Actual ([string]$f61RefreshReport.schema) -Expected "PCControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefresh" -Label "F61 refresh report schema"
    Assert-Equals -Actual ([string]$f61RefreshReport.result) -Expected "pass" -Label "F61 refresh report result"
    Assert-Equals -Actual ([string]$f61RefreshReport.completedTask) -Expected "F61 refresh PC controlled-demo investor route evidence after F59 audit fixes" -Label "F61 refresh completed task"
    Assert-Equals -Actual ([string]$f61RefreshReport.nextFormalTask) -Expected "F62 audit post-F61 PC controlled-demo investor route evidence refresh" -Label "F61 refresh next task"
    Assert-Equals -Actual ([bool]$f61RefreshReport.noUnityLaunch) -Expected $true -Label "F61 refresh no Unity launch"
    Assert-Equals -Actual ([string]$f61RefreshReport.sourceAuditReport) -Expected "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json" -Label "F61 source F59 audit"
    Assert-Equals -Actual ([string]$f61RefreshReport.sourceFixesReport) -Expected "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json" -Label "F61 source F60 fixes"
}

if ($null -ne $f59AuditReport) {
    Assert-Equals -Actual ([string]$f59AuditReport.result) -Expected "pass-with-followups" -Label "F59 audit result"
    Assert-Equals -Actual ([string]$f59AuditReport.completedTask) -Expected "F59 audit post-F58 PC controlled-demo investor route evidence refresh" -Label "F59 audit completed task"
    Assert-Equals -Actual ([string]$f59AuditReport.nextFormalTask) -Expected "F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes" -Label "F59 audit next task"
}

if ($null -ne $f60FixReport) {
    Assert-Equals -Actual ([string]$f60FixReport.result) -Expected "pass" -Label "F60 fixes result"
    Assert-Equals -Actual ([string]$f60FixReport.completedTask) -Expected "F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes" -Label "F60 fixes completed task"
    Assert-Equals -Actual ([string]$f60FixReport.nextFormalTask) -Expected "F61 refresh PC controlled-demo investor route evidence after F59 audit fixes" -Label "F60 fixes next task"
}

Assert-All -Text $masterPlan -Label "master F62/F63 queue" -Needles @(
    'PC1-PC62',
    '| 140 | Done | `Audit post-F61 PC controlled-demo investor route evidence refresh` |',
    '| 141 | Next | `Implement post-F62 PC controlled-demo investor route evidence refresh audit fixes` |',
    'Mobile phones remain landscape-only for the first playable target'
)
Assert-All -Text $detailedPlan -Label "detailed F62/F63 queue" -Needles @(
    '| F62 | Done | `Audit post-F61 PC controlled-demo investor route evidence refresh` |',
    '| F63 | Next | `Implement post-F62 PC controlled-demo investor route evidence refresh audit fixes` |'
)
Assert-All -Text $mobilePlan -Label "mobile F62/F63 status" -Needles @(
    'PC1-PC62',
    'F62 audit post-F61 PC controlled-demo investor route evidence refresh',
    'F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes',
    'first phone version is landscape-only',
    'horizontal phone game'
)
Assert-All -Text $handoffDoc -Label "handoff F63 next" -Needles @(
    'Latest sealed PC/mobile wait-state checkpoint: `PC1-PC62`',
    'Current formal next development task after handoff: `F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes`',
    'Next planned work: `F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes`',
    'first phone version is landscape-only'
)
Assert-All -Text $routeDoc -Label "route doc F62 audit visibility" -Needles @(
    '## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Findings',
    'RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAudit=pass-with-followups source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json completed=F62 next=F63 noUnityLaunch=True mobile=landscape-only',
    'RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=route-proof-clarity status=pass',
    'RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFollowUp=P1 area=audit-visibility next=F63-doc-gate-visibility'
)
Assert-All -Text $evidenceDoc -Label "evidence doc F62 audit visibility" -Needles @(
    'RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAudit=pass-with-followups source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json completed=F62 next=F63 noUnityLaunch=True mobile=landscape-only',
    'RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=damage-ejection-proof status=pass',
    'RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFollowUp=P2 area=next-refresh-contract next=F64-consume-F62-audit-report'
)
Assert-All -Text $queueScript -Label "queue F62/F63 markers" -Needles @(
    'PC1-PC62',
    'F62 audit post-F61 PC controlled-demo investor route evidence refresh',
    'F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes',
    '| F62 | Done | `Audit post-F61 PC controlled-demo investor route evidence refresh` |',
    '| F63 | Next | `Implement post-F62 PC controlled-demo investor route evidence refresh audit fixes` |'
)
Assert-All -Text $currentGate -Label "current gate F62 audit plan" -Needles @(
    'audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1',
    'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit plan OK.'
)
Assert-All -Text $handoffScript -Label "handoff script F62/F63 markers" -Needles @(
    'audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1',
    'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK',
    'F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes'
)
Assert-Contains -Text $gitignore -Needle 'analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/' -Label ".gitignore F62 output"

Add-Finding -Area "route-proof-clarity" -Status "pass" -Detail "F61 command evidence keeps the five-preset Windows investor route and links command report, screenshots, sidecars and logs"
Add-Finding -Area "damage-ejection-proof" -Status "pass" -Detail "damage-demo still carries section loss, cockpit ejection, wreck salvage and repair-cost proof after the F61 refresh"
Add-Finding -Area "mobile-landscape-proof" -Status "pass" -Detail "F61 command evidence and plan surfaces keep first phone version landscape-only as a horizontal phone build"
Add-Finding -Area "public-safe-proxy-boundary" -Status "pass" -Detail "proxy visuals remain public-safe stand-ins and state collision, pathing and BattleCore are unchanged"
Add-Finding -Area "audit-fix-refresh-closure" -Status "pass" -Detail "F61 command report and markdown carry F59 audit and F60 fix reports as explicit sources"

Add-FollowUp -Priority "P1" -Area "audit-visibility" -Issue "F62 audit findings should be made visible in plan, evidence and handoff surfaces before the next refresh step." -NextFix "Implement F63 as a narrow doc/gate update that records F62 audit findings, keeps no-Unity-launch evidence, and preserves the landscape-only phone scope."
Add-FollowUp -Priority "P2" -Area "next-refresh-contract" -Issue "The next command-evidence refresh should consume the F62 audit report explicitly so the audit closure can be traced." -NextFix "When F63 is complete, require its ignored report before advancing command evidence metadata again."

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAudit"
    result = "pass-with-followups"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F62 audit post-F61 PC controlled-demo investor route evidence refresh"
    nextFormalTask = "F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes"
    sourceCommandEvidenceReport = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json"
    sourceF61RefreshReport = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh/report.json"
    sourceF59AuditReport = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json"
    sourceF60FixReport = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json"
    noUnityLaunch = $true
    auditedPresets = $requiredPresets
    findings = $findings.ToArray()
    followUps = $followUps.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Route Evidence Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit")
$markdownLines.Add("")
$markdownLines.Add("Result: pass-with-followups")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F62 audit post-F61 PC controlled-demo investor route evidence refresh`')
$markdownLines.Add('Next formal task: `F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source command evidence: `analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json`')
$markdownLines.Add('Source F61 refresh: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh/report.json`')
$markdownLines.Add('Source F59 audit: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json`')
$markdownLines.Add('Source F60 fixes: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json`')
$markdownLines.Add("")
$markdownLines.Add("## Findings")
foreach ($finding in $findings) {
    $markdownLines.Add("- $($finding.area): $($finding.status) - $($finding.detail)")
}
$markdownLines.Add("")
$markdownLines.Add("## Follow-Ups")
foreach ($followUp in $followUps) {
    $markdownLines.Add("- $($followUp.priority) $($followUp.area): $($followUp.issue) Next: $($followUp.nextFix)")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK."
Write-Host "Report: $reportJsonPath"
