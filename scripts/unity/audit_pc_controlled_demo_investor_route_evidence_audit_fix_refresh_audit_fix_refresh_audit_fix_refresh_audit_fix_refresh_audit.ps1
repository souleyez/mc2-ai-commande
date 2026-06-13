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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit"
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
    Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.md"
$commandReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.json"
$f58RefreshReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh\report.json"
$f58RefreshMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh\report.md"
$f56AuditReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit\report.json"
$f57FixReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes\report.json"
$commandCaptureScript = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$masterPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RepoText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$handoffDoc = Read-RepoText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$queueScript = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$gitignore = Read-RepoText -RelativePath ".gitignore"

$closureMarkers = @(
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditRefresh=ready source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json completed=F58 next=F59 noUnityLaunch=True mobile=landscape-only",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=F57-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json sourceFixes=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json"
)

Assert-All -Text $commandCaptureScript -Label "command capture F58/F59 source contract" -Needles @(
    'F58 refresh PC controlled-demo investor route evidence after F56 audit fixes',
    'F59 audit post-F58 PC controlled-demo investor route evidence refresh',
    'sourceRouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditReport = Convert-ToRepoRelativePath -Path $routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditReportPath',
    'sourceRouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesReport = Convert-ToRepoRelativePath -Path $routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesReportPath',
    'routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure = $routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure',
    '## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Closure'
)
Assert-All -Text $commandCaptureScript -Label "command capture F58 closure source" -Needles $closureMarkers

Assert-All -Text $commandMarkdown -Label "command markdown F58 route evidence" -Needles @(
    'Completed task: `F58 refresh PC controlled-demo investor route evidence after F56 audit fixes`',
    'Next formal task: `F59 audit post-F58 PC controlled-demo investor route evidence refresh`',
    'Source route audit fix refresh audit fix refresh audit fix refresh audit report: "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json"',
    'Source route audit fix refresh audit fix refresh audit fix refresh audit fixes report: "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json"',
    '## Investor Route Summary',
    '## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Closure',
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return launch=scripts/unity/run_windows_demo.ps1 evidence=command-report+screenshots+sidecars',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png sidecar=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json log=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log callout=section-loss+cockpit-ejection+wreck-salvage+repair-line repairCost=9288',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only'
)
Assert-All -Text $commandMarkdown -Label "command markdown F58 closure markers" -Needles $closureMarkers

Assert-All -Text $f58RefreshMarkdown -Label "F58 refresh gate markdown" -Needles @(
    'Completed task: `F58 refresh PC controlled-demo investor route evidence after F56 audit fixes`',
    'Next formal task: `F59 audit post-F58 PC controlled-demo investor route evidence refresh`',
    'NoUnityLaunch: True',
    'command evidence metadata advanced to F58/F59',
    'F56 audit report is an explicit command evidence source',
    'F57 fix report is an explicit command evidence source',
    'mobile landscape-only proof remains visible'
)

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F58 refresh PC controlled-demo investor route evidence after F56 audit fixes" -Label "command report completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected "F59 audit post-F58 PC controlled-demo investor route evidence refresh" -Label "command report next task"
    Assert-Equals -Actual ([string]$commandReport.sourceRouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditReport) -Expected "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json" -Label "command report source F56 audit"
    Assert-Equals -Actual ([string]$commandReport.sourceRouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesReport) -Expected "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json" -Label "command report source F57 fixes"
    Assert-Equals -Actual ([int]$commandReport.width) -Expected 1280 -Label "command report width"
    Assert-Equals -Actual ([int]$commandReport.height) -Expected 720 -Label "command report height"

    foreach ($marker in $closureMarkers) {
        $matches = @($commandReport.routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure | Where-Object { [string]$_ -eq $marker })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "command report F58 closure $marker"
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

if ($null -ne $f58RefreshReport) {
    Assert-Equals -Actual ([string]$f58RefreshReport.schema) -Expected "PCControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefresh" -Label "F58 refresh report schema"
    Assert-Equals -Actual ([string]$f58RefreshReport.result) -Expected "pass" -Label "F58 refresh report result"
    Assert-Equals -Actual ([string]$f58RefreshReport.completedTask) -Expected "F58 refresh PC controlled-demo investor route evidence after F56 audit fixes" -Label "F58 refresh completed task"
    Assert-Equals -Actual ([string]$f58RefreshReport.nextFormalTask) -Expected "F59 audit post-F58 PC controlled-demo investor route evidence refresh" -Label "F58 refresh next task"
    Assert-Equals -Actual ([bool]$f58RefreshReport.noUnityLaunch) -Expected $true -Label "F58 refresh no Unity launch"
}

if ($null -ne $f56AuditReport) {
    Assert-Equals -Actual ([string]$f56AuditReport.result) -Expected "pass-with-followups" -Label "F56 audit result"
    Assert-Equals -Actual ([string]$f56AuditReport.completedTask) -Expected "F56 audit post-F55 PC controlled-demo investor route evidence refresh" -Label "F56 audit completed task"
    Assert-Equals -Actual ([string]$f56AuditReport.nextFormalTask) -Expected "F57 implement post-F56 PC controlled-demo investor route evidence refresh audit fixes" -Label "F56 audit next task"
}

if ($null -ne $f57FixReport) {
    Assert-Equals -Actual ([string]$f57FixReport.result) -Expected "pass" -Label "F57 fixes result"
    Assert-Equals -Actual ([string]$f57FixReport.completedTask) -Expected "F57 implement post-F56 PC controlled-demo investor route evidence refresh audit fixes" -Label "F57 fixes completed task"
    Assert-Equals -Actual ([string]$f57FixReport.nextFormalTask) -Expected "F58 refresh PC controlled-demo investor route evidence after F56 audit fixes" -Label "F57 fixes next task"
}

Assert-All -Text $masterPlan -Label "master F59/F60 queue" -Needles @(
    '| 137 | Done | `Audit post-F58 PC controlled-demo investor route evidence refresh` |',
    '| 138 | Next | `Implement post-F59 PC controlled-demo investor route evidence refresh audit fixes` |',
    'Mobile phones remain landscape-only for the first playable target'
)
Assert-All -Text $detailedPlan -Label "detailed F59/F60 queue" -Needles @(
    '| F59 | Done | `Audit post-F58 PC controlled-demo investor route evidence refresh` |',
    '| F60 | Next | `Implement post-F59 PC controlled-demo investor route evidence refresh audit fixes` |'
)
Assert-All -Text $mobilePlan -Label "mobile F59/F60 status" -Needles @(
    'F59 audit post-F58 PC controlled-demo investor route evidence refresh',
    'F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes',
    'first phone version is landscape-only',
    'horizontal phone game'
)
Assert-All -Text $handoffDoc -Label "handoff F60 next" -Needles @(
    'Current formal next development task after handoff: `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes`',
    'Next planned work: `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes`',
    'first phone version is landscape-only'
)
Assert-All -Text $queueScript -Label "queue F59/F60 markers" -Needles @(
    'F59 audit post-F58 PC controlled-demo investor route evidence refresh',
    'F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes',
    '| F59 | Done | `Audit post-F58 PC controlled-demo investor route evidence refresh` |',
    '| F60 | Next | `Implement post-F59 PC controlled-demo investor route evidence refresh audit fixes` |'
)
Assert-All -Text $currentGate -Label "current gate F59 audit plan" -Needles @(
    'audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1',
    'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit plan OK.'
)
Assert-All -Text $handoffScript -Label "handoff script F59/F60 markers" -Needles @(
    'audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1',
    'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK',
    'F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes'
)
Assert-Contains -Text $gitignore -Needle 'analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/' -Label ".gitignore F59 output"

Add-Finding -Area "route-proof-clarity" -Status "pass" -Detail "F58 command evidence preserves the five-preset Windows investor route and links command report, screenshots, sidecars and logs"
Add-Finding -Area "damage-ejection-proof" -Status "pass" -Detail "damage-demo still carries section loss, cockpit ejection, wreck salvage and repair-cost proof"
Add-Finding -Area "mobile-landscape-proof" -Status "pass" -Detail "F58 command evidence and plan surfaces keep first phone version landscape-only as a horizontal phone build"
Add-Finding -Area "public-safe-proxy-boundary" -Status "pass" -Detail "proxy visuals remain public-safe stand-ins and state collision, pathing and BattleCore are unchanged"
Add-Finding -Area "audit-fix-refresh-closure" -Status "pass" -Detail "F58 command report and markdown carry F56 audit and F57 fix reports as explicit sources"

Add-FollowUp -Priority "P1" -Area "audit-visibility" -Issue "F59 audit findings should be made visible in plan, evidence and handoff surfaces before the next refresh step." -NextFix "Implement F60 as a narrow doc/gate update that records F59 audit findings, keeps no-Unity-launch evidence, and preserves the landscape-only phone scope."
Add-FollowUp -Priority "P2" -Area "next-refresh-contract" -Issue "The next command-evidence refresh should consume the F59 audit report explicitly so the audit closure can be traced." -NextFix "When F60 is complete, require its ignored report before advancing command evidence metadata again."

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAudit"
    result = "pass-with-followups"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F59 audit post-F58 PC controlled-demo investor route evidence refresh"
    nextFormalTask = "F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes"
    sourceCommandEvidenceReport = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json"
    sourceF58RefreshReport = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh/report.json"
    sourceF56AuditReport = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json"
    sourceF57FixReport = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json"
    noUnityLaunch = $true
    auditedPresets = $requiredPresets
    findings = $findings.ToArray()
    followUps = $followUps.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Route Evidence Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit")
$markdownLines.Add("")
$markdownLines.Add("Result: pass-with-followups")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F59 audit post-F58 PC controlled-demo investor route evidence refresh`')
$markdownLines.Add('Next formal task: `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source command evidence: `analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json`')
$markdownLines.Add('Source F58 refresh: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh/report.json`')
$markdownLines.Add('Source F56 audit: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json`')
$markdownLines.Add('Source F57 fixes: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json`')
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

Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK."
Write-Host "Report: $reportJsonPath"
