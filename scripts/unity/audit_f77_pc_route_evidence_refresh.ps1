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
    $OutputDir = Join-Path $RepoRoot "analysis-output\f77-pc-route-evidence-refresh-audit"
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
$f74AuditReportRel = "analysis-output/f74-pc-route-evidence-refresh-audit/report.json"
$f75FixReportRel = "analysis-output/f75-pc-route-audit-fixes/report.json"
$f76RefreshReportRel = "analysis-output/f76-pc-route-evidence-refresh/report.json"
$f76RefreshMarkdownRel = "analysis-output/f76-pc-route-evidence-refresh/report.md"
$f77OutputRel = "analysis-output/f77-pc-route-evidence-refresh-audit/"
$f77ScriptRel = "scripts/unity/audit_f77_pc_route_evidence_refresh.ps1"

$f76RefreshMarkers = @(
    "F76RouteEvidenceRefresh=ready source=analysis-output/f75-pc-route-audit-fixes/report.json completed=F76 next=F77 noUnityLaunch=True mobile=landscape-only",
    "F76RouteEvidenceRefreshSource=audit sourceAudit=analysis-output/f74-pc-route-evidence-refresh-audit/report.json sourceFixes=analysis-output/f75-pc-route-audit-fixes/report.json",
    "F76RouteEvidenceRefreshClosure=route-proof-preserved route=spawn>hangar-contact>damage-demo>solo-order>solo-return damage=section-loss+cockpit-ejection+wreck-salvage+repair-line publicSafe=proxy-only",
    "F76RouteEvidenceRefreshClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False"
)

$auditMarkers = @(
    "F77RouteEvidenceAudit=pass-with-followups source=analysis-output/f77-pc-route-evidence-refresh-audit/report.json completed=F77 next=F78 noUnityLaunch=True mobile=landscape-only",
    "F77RouteEvidenceAuditFinding=F76-traceability status=pass detail=F76 consumes F74 audit and F75 fixes",
    "F77RouteEvidenceAuditFinding=route-proof-clarity status=pass detail=spawn>hangar-contact>damage-demo>solo-order>solo-return",
    "F77RouteEvidenceAuditFinding=damage-ejection-proof status=pass detail=section-loss+cockpit-ejection+wreck-salvage+repair-line",
    "F77RouteEvidenceAuditFinding=mobile-landscape-proof status=pass detail=first phone version remains landscape-only",
    "F77RouteEvidenceAuditFinding=public-safe-proxy-boundary status=pass detail=proxy-only visuals with unchanged collision/pathing",
    "F77RouteEvidenceAuditFinding=windows-path-budget status=pass detail=F76 and F77 script/output paths stay short",
    "F77RouteEvidenceAuditFollowUp=P1 area=audit-visibility next=F78-doc-gate-visibility",
    "F77RouteEvidenceAuditFollowUp=P2 area=next-refresh-contract next=F79-consume-F77-audit-report",
    "F77RouteEvidenceAuditFollowUp=P2 area=path-budget next=F78-keep-new-F-artifacts-short"
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

function Assert-PathBudget {
    param(
        [string]$RelativePath,
        [int]$MaxLength,
        [string]$Label
    )

    if ($RelativePath.Length -gt $MaxLength) {
        Add-Failure "$Label exceeds path budget: $($RelativePath.Length) > $MaxLength ($RelativePath)"
    }
}

if ($PlanOnly) {
    Write-Host "F77 PC route evidence refresh audit plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandReport = Read-RepoJson -RelativePath $commandReportRel
$commandMarkdown = Read-RepoText -RelativePath $commandMarkdownRel
$f74AuditReport = Read-RepoJson -RelativePath $f74AuditReportRel
$f75FixReport = Read-RepoJson -RelativePath $f75FixReportRel
$f76RefreshReport = Read-RepoJson -RelativePath $f76RefreshReportRel
$f76RefreshMarkdown = Read-RepoText -RelativePath $f76RefreshMarkdownRel
$masterPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RepoText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$handoffDoc = Read-RepoText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$evidenceDoc = Read-RepoText -RelativePath "docs-playable-demo-investor-evidence-2026-06-07.md"
$routeDoc = Read-RepoText -RelativePath "docs-pc-investor-demo-route-2026-06-13.md"
$pcOptimizationDoc = Read-RepoText -RelativePath "docs-pc-optimization-plan-2026-06-11.md"
$readme = Read-RepoText -RelativePath "README.md"
$buildWin = Read-RepoText -RelativePath "BUILD-WIN.md"
$buildMobile = Read-RepoText -RelativePath "BUILD-MOBILE.md"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queueScript = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$gitignore = Read-RepoText -RelativePath ".gitignore"

Assert-All -Text $commandMarkdown -Label "command markdown F76 audit input" -Needles @(
    'Completed task: `F76 refresh PC controlled-demo investor route evidence after F74 audit fixes`',
    'Next formal task: `F77 audit post-F76 PC controlled-demo investor route evidence refresh`',
    'Source F74 route evidence audit report: "analysis-output/f74-pc-route-evidence-refresh-audit/report.json"',
    'Source F75 route audit fixes report: "analysis-output/f75-pc-route-audit-fixes/report.json"',
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity'
)
Assert-All -Text $commandMarkdown -Label "command markdown F76 refresh closure" -Needles $f76RefreshMarkers

Assert-All -Text $f76RefreshMarkdown -Label "F76 refresh report markdown" -Needles @(
    'Completed task: `F76 refresh PC controlled-demo investor route evidence after F74 audit fixes`',
    'Next formal task: `F77 audit post-F76 PC controlled-demo investor route evidence refresh`',
    'Source command evidence: `analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json`',
    'Source F74 audit: `analysis-output/f74-pc-route-evidence-refresh-audit/report.json`',
    'Source F75 fixes: `analysis-output/f75-pc-route-audit-fixes/report.json`'
)
Assert-All -Text $f76RefreshMarkdown -Label "F76 refresh report markers" -Needles $f76RefreshMarkers

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.schema) -Expected "PCControlledDemoCommandEvidenceRefresh" -Label "command report schema"
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F76 refresh PC controlled-demo investor route evidence after F74 audit fixes" -Label "command report completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected "F77 audit post-F76 PC controlled-demo investor route evidence refresh" -Label "command report next task"
    Assert-Equals -Actual ([string]$commandReport.sourceF74RouteEvidenceAuditReport) -Expected $f74AuditReportRel -Label "command report source F74 audit"
    Assert-Equals -Actual ([string]$commandReport.sourceF75RouteAuditFixesReport) -Expected $f75FixReportRel -Label "command report source F75 fixes"
    Assert-Equals -Actual ([int]$commandReport.width) -Expected 1280 -Label "command report width"
    Assert-Equals -Actual ([int]$commandReport.height) -Expected 720 -Label "command report height"

    $evidenceRows = @($commandReport.evidence)
    Assert-Equals -Actual $evidenceRows.Count -Expected $requiredPresets.Count -Label "command preset count"

    foreach ($marker in $f76RefreshMarkers) {
        $matches = @($commandReport.f75RouteEvidenceAuditClosure | Where-Object { [string]$_ -eq $marker })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "command report F76 refresh closure $marker"
    }

    foreach ($preset in $requiredPresets) {
        $matches = @($evidenceRows | Where-Object { [string]$_.preset -eq $preset })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "command report preset $preset"
        if ($matches.Count -ne 1) {
            continue
        }

        $row = $matches[0]
        Assert-FileExists -RelativePath ([string]$row.screenshot) -Label "$preset screenshot"
        Assert-FileExists -RelativePath ([string]$row.sidecar) -Label "$preset sidecar"
        Assert-FileExists -RelativePath ([string]$row.log) -Label "$preset log"
        Assert-Contains -Text ([string]$row.battleHud) -Needle 'SparseBattleUi=statusRows+sections+solo' -Label "$preset sparse HUD"
        Assert-Contains -Text ([string]$row.playableFlowPolish) -Needle 'mobileLandscapeOnly=True orientation=landscape' -Label "$preset landscape proof"
        Assert-Contains -Text ([string]$row.investorProxyVisuals) -Needle 'publicSafe=proxy-only' -Label "$preset public-safe proxy"
        Assert-Contains -Text ([string]$row.investorProxyVisuals) -Needle 'collision=unchanged pathing=unchanged BattleCore=unchanged' -Label "$preset gameplay boundary"
    }

    $damage = @($evidenceRows | Where-Object { [string]$_.preset -eq "damage-demo" }) | Select-Object -First 1
    if ($null -ne $damage) {
        Assert-Contains -Text ([string]$damage.playableFlowPolish) -Needle 'damageStory=section-loss+ejection+wreck' -Label "damage story"
        Assert-Contains -Text ([string]$damage.debriefRewardSummary) -Needle 'DamageDebrief=section-status+repair-line' -Label "damage repair line"
        Assert-Contains -Text ([string]$damage.debriefRewardSummary) -Needle 'cockpitEjection=ready' -Label "damage cockpit ejection"
    }

    $soloOrder = @($evidenceRows | Where-Object { [string]$_.preset -eq "solo-order" }) | Select-Object -First 1
    if ($null -ne $soloOrder) {
        Assert-Contains -Text ([string]$soloOrder.playableFlowPolish) -Needle 'soloReturn=order-active detached=1' -Label "solo order detached"
        Assert-Contains -Text ([string]$soloOrder.commandReadability) -Needle 'noDragBox=yes' -Label "solo order no drag box"
    }

    $soloReturn = @($evidenceRows | Where-Object { [string]$_.preset -eq "solo-return" }) | Select-Object -First 1
    if ($null -ne $soloReturn) {
        Assert-Contains -Text ([string]$soloReturn.playableFlowPolish) -Needle 'soloReturn=returned detached=0' -Label "solo return joined"
    }
}

if ($null -ne $f76RefreshReport) {
    Assert-Equals -Actual ([string]$f76RefreshReport.schema) -Expected "F76PCRouteEvidenceRefresh" -Label "F76 refresh report schema"
    Assert-Equals -Actual ([string]$f76RefreshReport.result) -Expected "pass" -Label "F76 refresh report result"
    Assert-Equals -Actual ([string]$f76RefreshReport.completedTask) -Expected "F76 refresh PC controlled-demo investor route evidence after F74 audit fixes" -Label "F76 refresh completed task"
    Assert-Equals -Actual ([string]$f76RefreshReport.nextFormalTask) -Expected "F77 audit post-F76 PC controlled-demo investor route evidence refresh" -Label "F76 refresh next task"
    Assert-Equals -Actual ([bool]$f76RefreshReport.noUnityLaunch) -Expected $true -Label "F76 refresh no Unity launch"
    Assert-Equals -Actual ([string]$f76RefreshReport.sourceCommandEvidenceReport) -Expected $commandReportRel -Label "F76 refresh source command evidence"
    Assert-Equals -Actual ([string]$f76RefreshReport.sourceF74AuditReport) -Expected $f74AuditReportRel -Label "F76 refresh source F74 audit"
    Assert-Equals -Actual ([string]$f76RefreshReport.sourceF75FixReport) -Expected $f75FixReportRel -Label "F76 refresh source F75 fixes"

    foreach ($marker in $f76RefreshMarkers) {
        $matches = @($f76RefreshReport.refreshMarkers | Where-Object { [string]$_ -eq $marker })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "F76 refresh report marker $marker"
    }
}

if ($null -ne $f74AuditReport) {
    Assert-Equals -Actual ([string]$f74AuditReport.result) -Expected "pass-with-followups" -Label "F74 audit result"
    Assert-Equals -Actual ([string]$f74AuditReport.completedTask) -Expected "F74 audit post-F73 PC controlled-demo investor route evidence refresh" -Label "F74 completed task"
    Assert-Equals -Actual ([string]$f74AuditReport.nextFormalTask) -Expected "F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes" -Label "F74 next task"
}

if ($null -ne $f75FixReport) {
    Assert-Equals -Actual ([string]$f75FixReport.result) -Expected "pass" -Label "F75 fixes result"
    Assert-Equals -Actual ([string]$f75FixReport.completedTask) -Expected "F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes" -Label "F75 completed task"
    Assert-Equals -Actual ([string]$f75FixReport.nextFormalTask) -Expected "F76 refresh PC controlled-demo investor route evidence after F74 audit fixes" -Label "F75 next task"
}

foreach ($surface in @($readme, $buildWin, $buildMobile, $evidenceDoc, $routeDoc, $handoffDoc)) {
    Assert-All -Text $surface -Label "F77 audit marker surface" -Needles $auditMarkers
}

Assert-All -Text $masterPlan -Label "master F77/F78 queue" -Needles @(
    "2026-06-13 v128",
    "PC1-PC77",
    '| 155 | Done | `Audit post-F76 PC controlled-demo investor route evidence refresh` |',
    '| 156 | Next | `Implement post-F77 PC controlled-demo investor route evidence refresh audit fixes` |',
    'Formal next task: `F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes`'
)
Assert-All -Text $detailedPlan -Label "detailed F77/F78 queue" -Needles @(
    "2026-06-13 v137",
    "PC1-PC77",
    '| F77 | Done | `Audit post-F76 PC controlled-demo investor route evidence refresh` |',
    '| F78 | Next | `Implement post-F77 PC controlled-demo investor route evidence refresh audit fixes` |',
    'formal next task: `F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes`'
)
Assert-All -Text $mobilePlan -Label "mobile F77/F78 status" -Needles @(
    "PC1-PC77",
    "F77 audit post-F76 PC controlled-demo investor route evidence refresh",
    "F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes",
    "first phone version is landscape-only"
)
Assert-All -Text $handoffDoc -Label "handoff F78 next" -Needles @(
    'Latest sealed PC/mobile wait-state checkpoint: `PC1-PC77`',
    'Current formal next development task after handoff: `F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes`',
    'Next planned work: `F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes`'
)
Assert-Contains -Text $pcOptimizationDoc -Needle "sealed through PC1-PC77" -Label "PC optimization F77 checkpoint"
Assert-All -Text $currentGate -Label "current gate F77 plan marker" -Needles @(
    "audit_f77_pc_route_evidence_refresh.ps1",
    "F77 PC route evidence refresh audit plan OK."
)
Assert-All -Text $queueScript -Label "queue F77/F78 marker" -Needles @(
    "F77 audit post-F76 PC controlled-demo investor route evidence refresh",
    "F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes",
    '| F77 | Done | `Audit post-F76 PC controlled-demo investor route evidence refresh` |',
    '| F78 | Next | `Implement post-F77 PC controlled-demo investor route evidence refresh audit fixes` |'
)
Assert-All -Text $handoffScript -Label "handoff script F77/F78 marker" -Needles @(
    "audit_f77_pc_route_evidence_refresh.ps1",
    "F77 PC route evidence refresh audit OK.",
    "F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes"
)
Assert-Contains -Text $gitignore -Needle $f77OutputRel -Label ".gitignore F77 output"
Assert-PathBudget -RelativePath $f77ScriptRel -MaxLength 96 -Label "F77 script path budget"
Assert-PathBudget -RelativePath $f77OutputRel -MaxLength 72 -Label "F77 output path budget"

Add-Finding -Area "F76-traceability" -Status "pass" -Detail "F76 command evidence and refresh report explicitly consume the F74 audit report and F75 fix report."
Add-Finding -Area "route-proof-clarity" -Status "pass" -Detail "Investor route remains spawn>hangar-contact>damage-demo>solo-order>solo-return with screenshots, sidecars and logs."
Add-Finding -Area "damage-ejection-proof" -Status "pass" -Detail "Damage-demo evidence keeps section loss, cockpit ejection, wreck salvage and repair-line proof."
Add-Finding -Area "mobile-landscape-proof" -Status "pass" -Detail "Command evidence, docs and contract retain landscape-only first phone version."
Add-Finding -Area "public-safe-proxy-boundary" -Status "pass" -Detail "Proxy-only visual language is present and collision/pathing/BattleCore remain unchanged."
Add-Finding -Area "windows-path-budget" -Status "pass" -Detail "F76 and F77 keep short script and output paths for Windows path-budget safety."

Add-FollowUp -Priority "P1" -Area "audit-visibility" -Issue "F77 audit findings must remain visible in plan, evidence and handoff surfaces." -NextFix "F78 doc and gate visibility pass."
Add-FollowUp -Priority "P2" -Area "next-refresh-contract" -Issue "The next route evidence refresh should explicitly consume the F77 audit report." -NextFix "F79 command-evidence metadata refresh."
Add-FollowUp -Priority "P2" -Area "path-budget" -Issue "New F-series evidence artifacts should continue using short output directories." -NextFix "Keep F78/F79 artifact paths short."

if ($failures.Count -gt 0) {
    Write-Host "F77 PC route evidence refresh audit failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) F77 PC route evidence refresh audit check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "F77PCRouteEvidenceRefreshAudit"
    result = "pass-with-followups"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F77 audit post-F76 PC controlled-demo investor route evidence refresh"
    nextFormalTask = "F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes"
    noUnityLaunch = $true
    sourceCommandEvidenceReport = $commandReportRel
    sourceF74AuditReport = $f74AuditReportRel
    sourceF75FixReport = $f75FixReportRel
    sourceF76RefreshReport = $f76RefreshReportRel
    auditMarkers = $auditMarkers
    findings = $findings.ToArray()
    followUps = $followUps.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# F77 PC Route Evidence Refresh Audit")
$markdownLines.Add("")
$markdownLines.Add("Result: pass-with-followups")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F77 audit post-F76 PC controlled-demo investor route evidence refresh`')
$markdownLines.Add('Next formal task: `F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source command evidence: `' + $commandReportRel + '`')
$markdownLines.Add('Source F74 audit: `' + $f74AuditReportRel + '`')
$markdownLines.Add('Source F75 fixes: `' + $f75FixReportRel + '`')
$markdownLines.Add('Source F76 refresh: `' + $f76RefreshReportRel + '`')
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

Write-Host "F77 PC route evidence refresh audit OK."
Write-Host "Report: $reportJsonPath"
