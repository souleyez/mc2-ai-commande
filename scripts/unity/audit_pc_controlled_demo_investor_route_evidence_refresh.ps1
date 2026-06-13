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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-route-evidence-refresh-audit"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([char[]]@("\", "/"))
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-investor-route-evidence-refresh-audit.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-investor-route-evidence-refresh-audit.md"
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
    Write-Host "PC controlled-demo investor route evidence refresh audit plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandCaptureScript = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$commandMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.md"
$commandReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.json"
$routeRefreshReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-refresh\pc-controlled-demo-investor-route-evidence-refresh.json"
$routeRefreshMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-refresh\pc-controlled-demo-investor-route-evidence-refresh.md"
$routeCheckScript = Read-RepoText -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_refresh.ps1"
$investorRouteDoc = Read-RepoText -RelativePath "docs-pc-investor-demo-route-2026-06-13.md"
$playableEvidenceDoc = Read-RepoText -RelativePath "docs-playable-demo-investor-evidence-2026-06-07.md"
$gitignore = Read-RepoText -RelativePath ".gitignore"

Assert-All -Text $commandCaptureScript -Label "command capture F46/F47 metadata" -Needles @(
    'F46 refresh PC controlled-demo investor route evidence after polish fixes',
    'F47 audit post-F46 PC controlled-demo investor route evidence refresh',
    '## Investor Route Summary',
    'DamageProof=damage-demo screenshot=$damageScreenshot sidecar=$damageSidecar log=$damageLog',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only'
)

Assert-All -Text $commandMarkdown -Label "command markdown route evidence" -Needles @(
    'Completed task: `F46 refresh PC controlled-demo investor route evidence after polish fixes`',
    'Next formal task: `F47 audit post-F46 PC controlled-demo investor route evidence refresh`',
    '## Investor Route Summary',
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return launch=scripts/unity/run_windows_demo.ps1 evidence=command-report+screenshots+sidecars',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png sidecar=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json log=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log callout=section-loss+cockpit-ejection+wreck-salvage+repair-line repairCost=9288',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only'
)

Assert-All -Text $routeRefreshMarkdown -Label "route refresh markdown" -Needles @(
    'Completed task: `F46 refresh PC controlled-demo investor route evidence after polish fixes`',
    'Next formal task: `F47 audit post-F46 PC controlled-demo investor route evidence refresh`',
    'NoUnityLaunch: True',
    'Source command evidence: `analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json`',
    'command evidence metadata advanced to F46/F47',
    'investor route summary present in refreshed markdown',
    'damage-demo screenshot, sidecar and log links present in refreshed markdown',
    'horizontal-only phone proof line present in refreshed markdown',
    'proxy parsing source/fallback marker present in refreshed markdown'
)

Assert-All -Text $routeCheckScript -Label "route refresh checker coverage" -Needles @(
    'PC controlled-demo investor route evidence refresh check OK.',
    'command report completed task',
    'command report next task',
    'damage-demo screenshot link',
    'damage-demo landscape proof',
    'F45 polish report next task',
    'analysis-output/pc-controlled-demo-investor-route-evidence-refresh/'
)

Assert-All -Text $investorRouteDoc -Label "investor route doc" -Needles @(
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'Expected route gate: `PC controlled-demo investor route evidence refresh check OK.`'
)

Assert-All -Text $playableEvidenceDoc -Label "playable investor evidence doc" -Needles @(
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return',
    'F46 refresh PC controlled-demo investor route evidence after polish fixes',
    'PC controlled-demo investor route evidence refresh check OK',
    'F47 audit post-F46 PC controlled-demo investor route evidence refresh'
)

Assert-Contains -Text $gitignore -Needle 'analysis-output/pc-controlled-demo-investor-route-evidence-refresh/' -Label ".gitignore route refresh output"
Assert-Contains -Text $gitignore -Needle 'analysis-output/pc-controlled-demo-investor-route-evidence-refresh-audit/' -Label ".gitignore route refresh audit output"

if ($null -ne $routeRefreshReport) {
    Assert-Equals -Actual ([string]$routeRefreshReport.schema) -Expected "PCControlledDemoInvestorRouteEvidenceRefresh" -Label "route refresh report schema"
    Assert-Equals -Actual ([string]$routeRefreshReport.result) -Expected "pass" -Label "route refresh report result"
    Assert-Equals -Actual ([string]$routeRefreshReport.completedTask) -Expected "F46 refresh PC controlled-demo investor route evidence after polish fixes" -Label "route refresh report completed task"
    Assert-Equals -Actual ([string]$routeRefreshReport.nextFormalTask) -Expected "F47 audit post-F46 PC controlled-demo investor route evidence refresh" -Label "route refresh report next task"
    Assert-Equals -Actual ([bool]$routeRefreshReport.noUnityLaunch) -Expected $true -Label "route refresh no Unity launch"
    Assert-Equals -Actual ([string]$routeRefreshReport.sourceCommandEvidenceReport) -Expected "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json" -Label "route refresh source report"
    Assert-Equals -Actual (@($routeRefreshReport.refreshedAreas).Count) -Expected 5 -Label "route refresh area count"
}

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F46 refresh PC controlled-demo investor route evidence after polish fixes" -Label "command report completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected "F47 audit post-F46 PC controlled-demo investor route evidence refresh" -Label "command report next task"
    Assert-Equals -Actual ([int]$commandReport.width) -Expected 1280 -Label "command report width"
    Assert-Equals -Actual ([int]$commandReport.height) -Expected 720 -Label "command report height"

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

Add-Finding -Area "route-evidence-envelope" -Status "pass" -Detail "F46 route evidence is pass, 1280x720, five required presets, and linked to existing screenshots, sidecars and logs"
Add-Finding -Area "presentation-route" -Status "pass" -Detail "Investor route summary names the Windows route, demo launcher and command-report/screenshots/sidecars evidence path"
Add-Finding -Area "damage-proof" -Status "pass" -Detail "damage-demo has explicit screenshot, sidecar, log, repair cost, section loss, cockpit ejection and wreck salvage markers"
Add-Finding -Area "mobile-landscape-proof" -Status "pass" -Detail "Command markdown, report rows and investor route doc keep first phone version landscape-only"
Add-Finding -Area "public-safe-proxy-boundary" -Status "pass" -Detail "Proxy visuals remain public-safe stand-ins and state collision, pathing and BattleCore are unchanged"

Add-FollowUp -Priority "P1" -Area "audit-fixes" -Issue "F46 route evidence is complete, but the plan needs a narrow post-audit fix step before the next evidence refresh." -NextFix "Implement F48 as a source/doc polish step that makes the route-audit findings and follow-ups visible in the handoff and investor evidence docs."
Add-FollowUp -Priority "P2" -Area "gate-runtime" -Issue "The full current-plan aggregate gate can exceed practical local time limits because it launches many child gates." -NextFix "Keep F48 focused on route evidence and avoid expanding it into full aggregate gate restructuring unless needed for this route evidence path."

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor route evidence refresh audit failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor route evidence refresh audit check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorRouteEvidenceRefreshAudit"
    result = "pass-with-followups"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F47 audit post-F46 PC controlled-demo investor route evidence refresh"
    nextFormalTask = "F48 implement post-F47 PC controlled-demo investor route evidence audit fixes"
    sourceCommandEvidenceReport = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json"
    sourceRouteRefreshReport = "analysis-output/pc-controlled-demo-investor-route-evidence-refresh/pc-controlled-demo-investor-route-evidence-refresh.json"
    noUnityLaunch = $true
    auditedPresets = $requiredPresets
    findings = $findings.ToArray()
    followUps = $followUps.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Route Evidence Refresh Audit")
$markdownLines.Add("")
$markdownLines.Add("Result: pass-with-followups")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F47 audit post-F46 PC controlled-demo investor route evidence refresh`')
$markdownLines.Add('Next formal task: `F48 implement post-F47 PC controlled-demo investor route evidence audit fixes`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add('Source command evidence: `analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json`')
$markdownLines.Add('Source route refresh report: `analysis-output/pc-controlled-demo-investor-route-evidence-refresh/pc-controlled-demo-investor-route-evidence-refresh.json`')
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

Write-Host "PC controlled-demo investor route evidence refresh audit OK."
Write-Host "Report: $reportJsonPath"
