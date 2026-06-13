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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([char[]]@("\", "/"))
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit.md"
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
    Write-Host "PC controlled-demo investor route evidence audit fix refresh audit plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.md"
$commandReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.json"
$auditFixRefreshReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh\pc-controlled-demo-investor-route-evidence-audit-fix-refresh.json"
$auditFixRefreshMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh\pc-controlled-demo-investor-route-evidence-audit-fix-refresh.md"
$auditFixesReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fixes\pc-controlled-demo-investor-route-evidence-audit-fixes.json"
$commandCaptureScript = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$auditFixRefreshCheckScript = Read-RepoText -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1"
$investorRouteDoc = Read-RepoText -RelativePath "docs-pc-investor-demo-route-2026-06-13.md"
$playableEvidenceDoc = Read-RepoText -RelativePath "docs-playable-demo-investor-evidence-2026-06-07.md"
$handoffDoc = Read-RepoText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$gitignore = Read-RepoText -RelativePath ".gitignore"

$closureMarkers = @(
    "RouteAuditFixRefresh=ready source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fixes/pc-controlled-demo-investor-route-evidence-audit-fixes.json completed=F49 next=F50 noUnityLaunch=True mobile=landscape-only",
    "RouteAuditFixClosure=F48-doc-visibility status=closed surfaces=investor-route+playable-evidence+handoff",
    "RouteAuditFixClosure=gate-runtime status=route-gates-focused aggregateGate=not-required"
)

Assert-All -Text $commandCaptureScript -Label "command capture F49/F50 metadata" -Needles @(
    'F49 refresh PC controlled-demo investor route evidence after audit fixes',
    'F50 audit post-F49 PC controlled-demo investor route evidence refresh',
    'sourceRouteAuditFixesReport = Convert-ToRepoRelativePath -Path $routeAuditFixesReportPath',
    'routeAuditFixClosure = $routeAuditFixClosure',
    '## Route Audit Fix Closure'
)
Assert-All -Text $commandCaptureScript -Label "command capture closure source" -Needles $closureMarkers

Assert-All -Text $commandMarkdown -Label "command markdown F49 route evidence" -Needles @(
    'Completed task: `F49 refresh PC controlled-demo investor route evidence after audit fixes`',
    'Next formal task: `F50 audit post-F49 PC controlled-demo investor route evidence refresh`',
    'Source route audit fixes report: "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fixes/pc-controlled-demo-investor-route-evidence-audit-fixes.json"',
    '## Investor Route Summary',
    '## Route Audit Fix Closure',
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return launch=scripts/unity/run_windows_demo.ps1 evidence=command-report+screenshots+sidecars',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png sidecar=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json log=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log callout=section-loss+cockpit-ejection+wreck-salvage+repair-line repairCost=9288',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only'
)
Assert-All -Text $commandMarkdown -Label "command markdown closure markers" -Needles $closureMarkers

Assert-All -Text $auditFixRefreshMarkdown -Label "F49 refresh markdown" -Needles @(
    'Completed task: `F49 refresh PC controlled-demo investor route evidence after audit fixes`',
    'Next formal task: `F50 audit post-F49 PC controlled-demo investor route evidence refresh`',
    'NoUnityLaunch: True',
    'command evidence metadata advanced to F49/F50',
    'route audit fix closure is present in command markdown',
    'route audit fix closure is present in command JSON',
    'mobile landscape-only proof remains visible'
)

Assert-All -Text $auditFixRefreshCheckScript -Label "F49 refresh checker coverage" -Needles @(
    'PC controlled-demo investor route evidence audit fix refresh check OK.',
    'command report completed task',
    'command report next task',
    'command evidence F49 closure markdown',
    'Route Audit Fix Closure',
    'analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh/'
)

Assert-All -Text $investorRouteDoc -Label "investor route doc closure" -Needles @(
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'RouteAuditFix=F48 visibility=investor-route+playable-evidence+handoff noUnityLaunch=True mobile=landscape-only next=F49-route-refresh'
)

Assert-All -Text $playableEvidenceDoc -Label "playable evidence closure" -Needles @(
    'RouteAuditFix=F48 visibility=investor-route+playable-evidence+handoff noUnityLaunch=True mobile=landscape-only next=F49-route-refresh',
    'F49 refresh PC controlled-demo investor route evidence after audit fixes',
    'PC controlled-demo investor route evidence audit fix refresh check OK',
    'F50 audit post-F49 PC controlled-demo investor route evidence refresh'
)

Assert-All -Text $handoffDoc -Label "handoff route closure" -Needles @(
    'Current formal next development task after handoff: `F51 implement post-F50 PC controlled-demo investor route evidence refresh audit fixes`',
    'F50 audit post-F49 PC controlled-demo investor route evidence refresh',
    'PC controlled-demo investor route evidence audit fix refresh audit OK',
    'RouteAuditFix=F48 visibility=investor-route+playable-evidence+handoff noUnityLaunch=True mobile=landscape-only next=F49-route-refresh',
    'first phone version is landscape-only'
)

Assert-Contains -Text $gitignore -Needle 'analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh/' -Label ".gitignore F49 output"
Assert-Contains -Text $gitignore -Needle 'analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit/' -Label ".gitignore F50 output"

if ($null -ne $auditFixRefreshReport) {
    Assert-Equals -Actual ([string]$auditFixRefreshReport.schema) -Expected "PCControlledDemoInvestorRouteEvidenceAuditFixRefresh" -Label "F49 refresh report schema"
    Assert-Equals -Actual ([string]$auditFixRefreshReport.result) -Expected "pass" -Label "F49 refresh report result"
    Assert-Equals -Actual ([string]$auditFixRefreshReport.completedTask) -Expected "F49 refresh PC controlled-demo investor route evidence after audit fixes" -Label "F49 refresh report completed task"
    Assert-Equals -Actual ([string]$auditFixRefreshReport.nextFormalTask) -Expected "F50 audit post-F49 PC controlled-demo investor route evidence refresh" -Label "F49 refresh report next task"
    Assert-Equals -Actual ([bool]$auditFixRefreshReport.noUnityLaunch) -Expected $true -Label "F49 refresh no Unity launch"
    Assert-Equals -Actual ([string]$auditFixRefreshReport.sourceCommandEvidenceReport) -Expected "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json" -Label "F49 refresh source command evidence"
    Assert-Equals -Actual ([string]$auditFixRefreshReport.sourceRouteAuditFixesReport) -Expected "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fixes/pc-controlled-demo-investor-route-evidence-audit-fixes.json" -Label "F49 refresh source route audit fixes"
    Assert-Equals -Actual (@($auditFixRefreshReport.refreshedAreas).Count) -Expected 6 -Label "F49 refreshed area count"
}

if ($null -ne $auditFixesReport) {
    Assert-Equals -Actual ([string]$auditFixesReport.result) -Expected "pass" -Label "F48 audit fixes report result"
    Assert-Equals -Actual ([string]$auditFixesReport.completedTask) -Expected "F48 implement post-F47 PC controlled-demo investor route evidence audit fixes" -Label "F48 audit fixes completed task"
    Assert-Equals -Actual ([string]$auditFixesReport.nextFormalTask) -Expected "F49 refresh PC controlled-demo investor route evidence after audit fixes" -Label "F48 audit fixes next task"
}

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F49 refresh PC controlled-demo investor route evidence after audit fixes" -Label "command report completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected "F50 audit post-F49 PC controlled-demo investor route evidence refresh" -Label "command report next task"
    Assert-Equals -Actual ([string]$commandReport.sourceRouteAuditFixesReport) -Expected "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fixes/pc-controlled-demo-investor-route-evidence-audit-fixes.json" -Label "command report source route audit fixes"
    Assert-Equals -Actual ([int]$commandReport.width) -Expected 1280 -Label "command report width"
    Assert-Equals -Actual ([int]$commandReport.height) -Expected 720 -Label "command report height"

    foreach ($marker in $closureMarkers) {
        $matches = @($commandReport.routeAuditFixClosure | Where-Object { [string]$_ -eq $marker })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "command report closure $marker"
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
        Assert-Contains -Text ([string]$row.investorProxyVisuals) -Needle 'InvestorProxyVisuals=active' -Label "$preset proxy active"
        Assert-Contains -Text ([string]$row.investorProxyVisuals) -Needle 'publicSafe=proxy-only' -Label "$preset proxy public boundary"
        Assert-Contains -Text ([string]$row.investorProxyVisuals) -Needle 'collision=unchanged pathing=unchanged BattleCore=unchanged' -Label "$preset proxy gameplay boundary"
    }

    $hangar = Get-ReportRow -Rows $evidenceRows -Preset "hangar-contact"
    if ($null -ne $hangar) {
        Assert-Contains -Text ([string]$hangar.playableFlowPolish) -Needle 'ContactPressureCue=objective-panel+in-world' -Label "hangar-contact pressure cue"
    }

    $damage = Get-ReportRow -Rows $evidenceRows -Preset "damage-demo"
    if ($null -ne $damage) {
        Assert-Equals -Actual ([string]$damage.screenshot) -Expected "analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png" -Label "damage screenshot path"
        Assert-Equals -Actual ([string]$damage.sidecar) -Expected "analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json" -Label "damage sidecar path"
        Assert-Equals -Actual ([string]$damage.log) -Expected "analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log" -Label "damage log path"
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

$damageSidecar = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-visual-evidence\captures\damage-demo.json"
if ($null -ne $damageSidecar) {
    Assert-Contains -Text ([string]$damageSidecar.damageStory) -Needle 'arms=1 legs=1 cockpit=1' -Label "damage sidecar section losses"
    Assert-Contains -Text ([string]$damageSidecar.damageReadability) -Needle 'Cockpit=breach+ejection-pod+chute+landing+arc+distress+escape-column+route' -Label "damage sidecar cockpit ejection visual"
    Assert-Contains -Text ([string]$damageSidecar.damageReadability) -Needle 'Wreck=blast+smoke+marker+debris+salvage' -Label "damage sidecar wreck salvage"
}

Add-Finding -Area "route-proof-clarity" -Status "pass" -Detail "F49 command evidence preserves the exact Windows route, launch path, command report, screenshots and sidecars"
Add-Finding -Area "damage-ejection-proof" -Status "pass" -Detail "damage-demo still links screenshot, sidecar, log, section loss, cockpit ejection, wreck salvage and repair cost"
Add-Finding -Area "mobile-landscape-proof" -Status "pass" -Detail "F49 command markdown, report rows and handoff docs keep first phone version landscape-only as a horizontal phone game"
Add-Finding -Area "public-safe-proxy-boundary" -Status "pass" -Detail "Proxy visuals remain public-safe stand-ins and state collision, pathing and BattleCore are unchanged"
Add-Finding -Area "audit-fix-closure" -Status "pass" -Detail "F49 command report and markdown carry the F48 route-audit fix closure markers and source F48 audit-fix report"

Add-FollowUp -Priority "P1" -Area "audit-visibility" -Issue "F50 audit results should be made visible in plan, evidence and handoff surfaces before the next refresh step." -NextFix "Implement F51 as a narrow doc/gate update that records F50 audit findings, keeps the route gates focused, and preserves no-Unity-launch evidence."
Add-FollowUp -Priority "P2" -Area "next-refresh-contract" -Issue "The next evidence refresh should consume the F50 audit report explicitly so the F51 closure can be traced." -NextFix "When F51 is complete, require its ignored report before advancing command evidence metadata again."

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor route evidence audit fix refresh audit failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor route evidence audit fix refresh audit check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorRouteEvidenceAuditFixRefreshAudit"
    result = "pass-with-followups"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F50 audit post-F49 PC controlled-demo investor route evidence refresh"
    nextFormalTask = "F51 implement post-F50 PC controlled-demo investor route evidence refresh audit fixes"
    sourceCommandEvidenceReport = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json"
    sourceAuditFixRefreshReport = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh/pc-controlled-demo-investor-route-evidence-audit-fix-refresh.json"
    sourceRouteAuditFixesReport = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fixes/pc-controlled-demo-investor-route-evidence-audit-fixes.json"
    noUnityLaunch = $true
    auditedPresets = $requiredPresets
    findings = $findings.ToArray()
    followUps = $followUps.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Route Evidence Audit Fix Refresh Audit")
$markdownLines.Add("")
$markdownLines.Add("Result: pass-with-followups")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F50 audit post-F49 PC controlled-demo investor route evidence refresh`')
$markdownLines.Add('Next formal task: `F51 implement post-F50 PC controlled-demo investor route evidence refresh audit fixes`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add('Source command evidence: `analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json`')
$markdownLines.Add('Source audit-fix refresh: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh/pc-controlled-demo-investor-route-evidence-audit-fix-refresh.json`')
$markdownLines.Add('Source route audit fixes: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fixes/pc-controlled-demo-investor-route-evidence-audit-fixes.json`')
$markdownLines.Add("")
$markdownLines.Add("## Findings")
foreach ($finding in $report.findings) {
    $markdownLines.Add("- [$($finding.status)] $($finding.area): $($finding.detail)")
}
$markdownLines.Add("")
$markdownLines.Add("## Follow-Ups")
foreach ($followUp in $report.followUps) {
    $markdownLines.Add("- [$($followUp.priority)] $($followUp.area): $($followUp.issue) Next: $($followUp.nextFix)")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo investor route evidence audit fix refresh audit OK."
Write-Host "Report: $reportJsonPath"
