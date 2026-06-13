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
    $OutputDir = Join-Path $RepoRoot "analysis-output\f80-pc-route-evidence-refresh-audit"
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
$f77AuditReportRel = "analysis-output/f77-pc-route-evidence-refresh-audit/report.json"
$f78FixReportRel = "analysis-output/f78-pc-route-audit-fixes/report.json"
$f79RefreshReportRel = "analysis-output/f79-pc-route-evidence-refresh/report.json"
$f79RefreshMarkdownRel = "analysis-output/f79-pc-route-evidence-refresh/report.md"
$f80OutputRel = "analysis-output/f80-pc-route-evidence-refresh-audit/"
$f80ScriptRel = "scripts/unity/audit_f80_pc_route_evidence_refresh.ps1"

$f79RefreshMarkers = @(
    "F79RouteEvidenceRefresh=ready source=analysis-output/f78-pc-route-audit-fixes/report.json completed=F79 next=F80 noUnityLaunch=True mobile=landscape-only",
    "F79RouteEvidenceRefreshSource=audit sourceAudit=analysis-output/f77-pc-route-evidence-refresh-audit/report.json sourceFixes=analysis-output/f78-pc-route-audit-fixes/report.json",
    "F79RouteEvidenceRefreshClosure=route-proof-preserved route=spawn>hangar-contact>damage-demo>solo-order>solo-return damage=section-loss+cockpit-ejection+wreck-salvage+repair-line publicSafe=proxy-only",
    "F79RouteEvidenceRefreshClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False"
)

$auditMarkers = @(
    "F80RouteEvidenceAudit=pass-with-followups source=analysis-output/f80-pc-route-evidence-refresh-audit/report.json completed=F80 next=F81 noUnityLaunch=True mobile=landscape-only",
    "F80RouteEvidenceAuditFinding=F79-traceability status=pass detail=F79 consumes F77 audit and F78 fixes",
    "F80RouteEvidenceAuditFinding=route-proof-clarity status=pass detail=spawn>hangar-contact>damage-demo>solo-order>solo-return",
    "F80RouteEvidenceAuditFinding=damage-ejection-proof status=pass detail=section-loss+cockpit-ejection+wreck-salvage+repair-line",
    "F80RouteEvidenceAuditFinding=mobile-landscape-proof status=pass detail=first phone version remains landscape-only",
    "F80RouteEvidenceAuditFinding=phone-horizontal-product-decision status=pass detail=phone-first-version-horizontal-only; portrait out of first slice",
    "F80RouteEvidenceAuditFinding=public-safe-proxy-boundary status=pass detail=proxy-only visuals with unchanged collision/pathing",
    "F80RouteEvidenceAuditFinding=windows-path-budget status=pass detail=F79 and F80 script/output paths stay short",
    "F80RouteEvidenceAuditFollowUp=P1 area=audit-visibility next=F81-doc-gate-visibility",
    "F80RouteEvidenceAuditFollowUp=P2 area=next-refresh-contract next=F82-consume-F80-audit-report",
    "F80RouteEvidenceAuditFollowUp=P2 area=path-budget next=F81-keep-new-F-artifacts-short"
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
    Write-Host "F80 PC route evidence refresh audit plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandReport = Read-RepoJson -RelativePath $commandReportRel
$commandMarkdown = Read-RepoText -RelativePath $commandMarkdownRel
$f77AuditReport = Read-RepoJson -RelativePath $f77AuditReportRel
$f78FixReport = Read-RepoJson -RelativePath $f78FixReportRel
$f79RefreshReport = Read-RepoJson -RelativePath $f79RefreshReportRel
$f79RefreshMarkdown = Read-RepoText -RelativePath $f79RefreshMarkdownRel
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

Assert-All -Text $commandMarkdown -Label "command markdown F79 audit input" -Needles @(
    'Completed task: `F79 refresh PC controlled-demo investor route evidence after F77 audit fixes`',
    'Next formal task: `F80 audit post-F79 PC controlled-demo investor route evidence refresh`',
    'Source F77 route evidence audit report: "analysis-output/f77-pc-route-evidence-refresh-audit/report.json"',
    'Source F78 route audit fixes report: "analysis-output/f78-pc-route-audit-fixes/report.json"',
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity'
)
Assert-All -Text $commandMarkdown -Label "command markdown F79 refresh closure" -Needles $f79RefreshMarkers

Assert-All -Text $f79RefreshMarkdown -Label "F79 refresh report markdown" -Needles @(
    'Completed task: `F79 refresh PC controlled-demo investor route evidence after F77 audit fixes`',
    'Next formal task: `F80 audit post-F79 PC controlled-demo investor route evidence refresh`',
    'Source command evidence: `analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json`',
    'Source F77 audit: `analysis-output/f77-pc-route-evidence-refresh-audit/report.json`',
    'Source F78 fixes: `analysis-output/f78-pc-route-audit-fixes/report.json`'
)
Assert-All -Text $f79RefreshMarkdown -Label "F79 refresh report markers" -Needles $f79RefreshMarkers

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.schema) -Expected "PCControlledDemoCommandEvidenceRefresh" -Label "command report schema"
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F79 refresh PC controlled-demo investor route evidence after F77 audit fixes" -Label "command report completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected "F80 audit post-F79 PC controlled-demo investor route evidence refresh" -Label "command report next task"
    Assert-Equals -Actual ([string]$commandReport.sourceF77RouteEvidenceAuditReport) -Expected $f77AuditReportRel -Label "command report source F77 audit"
    Assert-Equals -Actual ([string]$commandReport.sourceF78RouteAuditFixesReport) -Expected $f78FixReportRel -Label "command report source F78 fixes"
    Assert-Equals -Actual ([int]$commandReport.width) -Expected 1280 -Label "command report width"
    Assert-Equals -Actual ([int]$commandReport.height) -Expected 720 -Label "command report height"

    $evidenceRows = @($commandReport.evidence)
    Assert-Equals -Actual $evidenceRows.Count -Expected $requiredPresets.Count -Label "command preset count"

    foreach ($marker in $f79RefreshMarkers) {
        $matches = @($commandReport.f78RouteEvidenceAuditClosure | Where-Object { [string]$_ -eq $marker })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "command report F79 refresh closure $marker"
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

if ($null -ne $f79RefreshReport) {
    Assert-Equals -Actual ([string]$f79RefreshReport.schema) -Expected "F79PCRouteEvidenceRefresh" -Label "F79 refresh report schema"
    Assert-Equals -Actual ([string]$f79RefreshReport.result) -Expected "pass" -Label "F79 refresh report result"
    Assert-Equals -Actual ([string]$f79RefreshReport.completedTask) -Expected "F79 refresh PC controlled-demo investor route evidence after F77 audit fixes" -Label "F79 refresh completed task"
    Assert-Equals -Actual ([string]$f79RefreshReport.nextFormalTask) -Expected "F80 audit post-F79 PC controlled-demo investor route evidence refresh" -Label "F79 refresh next task"
    Assert-Equals -Actual ([bool]$f79RefreshReport.noUnityLaunch) -Expected $true -Label "F79 refresh no Unity launch"
    Assert-Equals -Actual ([string]$f79RefreshReport.sourceCommandEvidenceReport) -Expected $commandReportRel -Label "F79 refresh source command evidence"
    Assert-Equals -Actual ([string]$f79RefreshReport.sourceF77AuditReport) -Expected $f77AuditReportRel -Label "F79 refresh source F77 audit"
    Assert-Equals -Actual ([string]$f79RefreshReport.sourceF78FixReport) -Expected $f78FixReportRel -Label "F79 refresh source F78 fixes"

    foreach ($marker in $f79RefreshMarkers) {
        $matches = @($f79RefreshReport.refreshMarkers | Where-Object { [string]$_ -eq $marker })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "F79 refresh report marker $marker"
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
    Assert-All -Text $surface -Label "F80 audit marker surface" -Needles $auditMarkers
}

Assert-All -Text $masterPlan -Label "master F80/F81 queue" -Needles @(
    "2026-06-13 v131",
    "PC1-PC80",
    '| 158 | Done | `Audit post-F79 PC controlled-demo investor route evidence refresh` |',
    '| 159 | Next | `Implement post-F80 PC controlled-demo investor route evidence refresh audit fixes` |',
    'Formal next task: `F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes`'
)
Assert-All -Text $detailedPlan -Label "detailed F80/F81 queue" -Needles @(
    "2026-06-13 v140",
    "PC1-PC80",
    '| F80 | Done | `Audit post-F79 PC controlled-demo investor route evidence refresh` |',
    '| F81 | Next | `Implement post-F80 PC controlled-demo investor route evidence refresh audit fixes` |',
    'formal next task: `F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes`'
)
Assert-All -Text $mobilePlan -Label "mobile F80/F81 status" -Needles @(
    "PC1-PC80",
    "F80 audit post-F79 PC controlled-demo investor route evidence refresh",
    "F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes",
    "first phone version is landscape-only"
)
Assert-All -Text $handoffDoc -Label "handoff F81 next" -Needles @(
    'Latest sealed PC/mobile wait-state checkpoint: `PC1-PC80`',
    'Current formal next development task after handoff: `F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes`',
    'Next planned work: `F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes`'
)
Assert-All -Text $currentGate -Label "current gate F80 plan marker" -Needles @(
    "audit_f80_pc_route_evidence_refresh.ps1",
    "F80 PC route evidence refresh audit plan OK."
)
Assert-All -Text $queueScript -Label "queue F80/F81 marker" -Needles @(
    "F80 audit post-F79 PC controlled-demo investor route evidence refresh",
    "F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes",
    '| F80 | Done | `Audit post-F79 PC controlled-demo investor route evidence refresh` |',
    '| F81 | Next | `Implement post-F80 PC controlled-demo investor route evidence refresh audit fixes` |'
)
Assert-All -Text $handoffScript -Label "handoff script F80/F81 marker" -Needles @(
    "audit_f80_pc_route_evidence_refresh.ps1",
    "F80 PC route evidence refresh audit OK.",
    "F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes"
)
Assert-Contains -Text $gitignore -Needle $f80OutputRel -Label ".gitignore F80 output"
Assert-PathBudget -RelativePath $f80ScriptRel -MaxLength 96 -Label "F80 script path budget"
Assert-PathBudget -RelativePath $f80OutputRel -MaxLength 72 -Label "F80 output path budget"

if ($failures.Count -gt 0) {
    Write-Host "F80 PC route evidence refresh audit failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) F80 PC route evidence refresh audit check(s) failed."
}

Add-Finding -Area "F79-traceability" -Status "pass" -Detail "F79 command evidence consumes F77 audit and F78 fixes."
Add-Finding -Area "route-proof-clarity" -Status "pass" -Detail "Route remains spawn>hangar-contact>damage-demo>solo-order>solo-return."
Add-Finding -Area "damage-ejection-proof" -Status "pass" -Detail "Damage demo keeps section loss, cockpit ejection, wreck salvage and repair-line proof."
Add-Finding -Area "mobile-landscape-proof" -Status "pass" -Detail "First phone version remains landscape-only."
Add-Finding -Area "phone-horizontal-product-decision" -Status "pass" -Detail "Phone first version is horizontal-only; portrait is out of first-slice scope."
Add-Finding -Area "public-safe-proxy-boundary" -Status "pass" -Detail "Proxy visuals preserve collision/pathing/BattleCore unchanged wording."
Add-Finding -Area "windows-path-budget" -Status "pass" -Detail "F79 and F80 script/output paths stay within Windows-safe budget."

Add-FollowUp -Priority "P1" -Area "audit-visibility" -Issue "F80 audit findings must stay visible in plan, evidence and handoff docs." -NextFix "F81-doc-gate-visibility"
Add-FollowUp -Priority "P2" -Area "next-refresh-contract" -Issue "Next route refresh must consume this F80 audit report." -NextFix "F82-consume-F80-audit-report"
Add-FollowUp -Priority "P2" -Area "path-budget" -Issue "Keep new F81/F82 artifact paths short." -NextFix "F81-keep-new-F-artifacts-short"

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "F80PCRouteEvidenceRefreshAudit"
    result = "pass-with-followups"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F80 audit post-F79 PC controlled-demo investor route evidence refresh"
    nextFormalTask = "F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes"
    noUnityLaunch = $true
    sourceCommandEvidenceReport = $commandReportRel
    sourceF77AuditReport = $f77AuditReportRel
    sourceF78FixReport = $f78FixReportRel
    sourceF79RefreshReport = $f79RefreshReportRel
    auditMarkers = $auditMarkers
    findings = $findings.ToArray()
    followUps = $followUps.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# F80 PC Route Evidence Refresh Audit")
$markdownLines.Add("")
$markdownLines.Add("Result: pass-with-followups")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F80 audit post-F79 PC controlled-demo investor route evidence refresh`')
$markdownLines.Add('Next formal task: `F81 implement post-F80 PC controlled-demo investor route evidence refresh audit fixes`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source command evidence: `' + $commandReportRel + '`')
$markdownLines.Add('Source F77 audit: `' + $f77AuditReportRel + '`')
$markdownLines.Add('Source F78 fixes: `' + $f78FixReportRel + '`')
$markdownLines.Add('Source F79 refresh: `' + $f79RefreshReportRel + '`')
$markdownLines.Add("")
$markdownLines.Add("## Findings")
foreach ($finding in $findings) {
    $markdownLines.Add("- $($finding.area): $($finding.status) - $($finding.detail)")
}
$markdownLines.Add("")
$markdownLines.Add("## Follow-ups")
foreach ($followUp in $followUps) {
    $markdownLines.Add("- $($followUp.priority) $($followUp.area): $($followUp.issue) Next: $($followUp.nextFix)")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "F80 PC route evidence refresh audit OK."
Write-Host "Report: $reportJsonPath"
