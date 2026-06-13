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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-evidence-refresh-audit"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([char[]]@("\", "/"))
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-investor-evidence-refresh-audit.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-investor-evidence-refresh-audit.md"
$requiredPresets = @("spawn", "hangar-contact", "damage-demo", "solo-order", "solo-return")
$failures = New-Object System.Collections.Generic.List[string]
$findings = New-Object System.Collections.Generic.List[object]
$followUps = New-Object System.Collections.Generic.List[object]

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Convert-ToRepoRelativePath {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    if ($fullPath.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        return ($fullPath.Substring($repoFullPath.Length).TrimStart([char[]]@("\", "/")) -replace "\\", "/")
    }

    return $fullPath
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

function Assert-PositiveInt {
    param(
        [int]$Value,
        [string]$Label
    )

    if ($Value -le 0) {
        Add-Failure "$Label must be positive, got $Value"
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

if ($PlanOnly) {
    Write-Host "PC controlled-demo investor evidence refresh audit plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.md"
$commandReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.json"
$refreshReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-evidence-refresh\pc-controlled-demo-investor-evidence-refresh.json"
$refreshMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-investor-evidence-refresh\pc-controlled-demo-investor-evidence-refresh.md"
$commandCaptureScript = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$refreshCheckScript = Read-RepoText -RelativePath "scripts\unity\check_pc_controlled_demo_investor_evidence_refresh.ps1"
$bootstrap = Read-RepoText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$gitignore = Read-RepoText -RelativePath ".gitignore"

Assert-Contains -Text $commandCaptureScript -Needle 'F43 refresh PC controlled-demo investor evidence package after fixes' -Label "command capture metadata"
Assert-Contains -Text $commandCaptureScript -Needle 'F44 audit post-F43 PC controlled-demo investor evidence refresh' -Label "command capture next task"
Assert-Contains -Text $refreshCheckScript -Needle 'PC controlled-demo investor evidence refresh check OK.' -Label "F43 refresh gate"
Assert-Contains -Text $refreshCheckScript -Needle 'InvestorDemoSummary=ready' -Label "F43 refresh investor summary gate"
Assert-Contains -Text $refreshCheckScript -Needle 'DamageInvestorCallout=section-loss+cockpit-ejection+wreck-salvage+repair-line' -Label "F43 refresh damage gate"
Assert-Contains -Text $gitignore -Needle 'analysis-output/pc-controlled-demo-investor-evidence-refresh/' -Label ".gitignore refresh output"

if ($null -ne $refreshReport) {
    Assert-Equals -Actual ([string]$refreshReport.result) -Expected "pass" -Label "F43 refresh report result"
    Assert-Equals -Actual ([string]$refreshReport.completedTask) -Expected "F43 refresh PC controlled-demo investor evidence package after fixes" -Label "F43 refresh report completed task"
    Assert-Equals -Actual ([string]$refreshReport.nextFormalTask) -Expected "F44 audit post-F43 PC controlled-demo investor evidence refresh" -Label "F43 refresh report next task"
    Assert-Equals -Actual (@($refreshReport.refreshedAreas).Count) -Expected 6 -Label "F43 refreshed area count"
}

Assert-Contains -Text $refreshMarkdown -Needle 'Completed task: `F43 refresh PC controlled-demo investor evidence package after fixes`' -Label "F43 refresh markdown completed task"
Assert-Contains -Text $refreshMarkdown -Needle 'Next formal task: `F44 audit post-F43 PC controlled-demo investor evidence refresh`' -Label "F43 refresh markdown next task"
Assert-Contains -Text $refreshMarkdown -Needle 'damage/ejection/debrief investor callout represented' -Label "F43 refresh markdown damage summary"
Assert-Contains -Text $refreshMarkdown -Needle 'public-safe proxy visual identity represented' -Label "F43 refresh markdown proxy summary"
Assert-Contains -Text $refreshMarkdown -Needle 'mobile landscape-only boundary preserved' -Label "F43 refresh markdown landscape summary"

Assert-Contains -Text $commandMarkdown -Needle '## Executive Summary' -Label "command markdown summary"
Assert-Contains -Text $commandMarkdown -Needle 'InvestorDemoSummary=ready presets=5 resolution=1280x720 sparseHud=True mobileLandscapeOnly=True publicSafe=proxy-only' -Label "command markdown investor summary"
Assert-Contains -Text $commandMarkdown -Needle 'DamageInvestorCallout=section-loss+cockpit-ejection+wreck-salvage+repair-line' -Label "command markdown damage callout"
Assert-Contains -Text $commandMarkdown -Needle 'ProxyVisualIdentity=mech-silhouette+vehicle-hull+infantry-fireteam+tree-canopy+building-roof+hardprop-stripe' -Label "command markdown proxy identity"
Assert-Contains -Text $commandMarkdown -Needle 'FastInvestorEvidenceGate=check_pc_controlled_demo_investor_evidence_package_fixes.ps1 source-only+report-only noUnityLaunch=True' -Label "command markdown fast gate"
Assert-Contains -Text $commandMarkdown -Needle '| damage-demo | Arm/leg/cockpit consequences, ejection, wreck and repair line |' -Label "command markdown damage preset"
Assert-Contains -Text $commandMarkdown -Needle '| solo-return | Ordered unit automatically returns to squad control |' -Label "command markdown solo return preset"

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F43 refresh PC controlled-demo investor evidence package after fixes" -Label "command report completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected "F44 audit post-F43 PC controlled-demo investor evidence refresh" -Label "command report next task"
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
        Assert-Contains -Text ([string]$row.playableFlowPolish) -Needle 'mobileLandscapeOnly=True' -Label "$preset playable flow landscape"
        Assert-Contains -Text ([string]$row.investorProxyVisuals) -Needle 'InvestorProxyVisuals=active' -Label "$preset proxy active"
        Assert-Contains -Text ([string]$row.investorProxyVisuals) -Needle 'publicSafe=proxy-only' -Label "$preset proxy public boundary"
    }

    $hangar = Get-ReportRow -Rows $evidenceRows -Preset "hangar-contact"
    if ($null -ne $hangar) {
        Assert-PositiveInt -Value ([int]$hangar.activeHostiles) -Label "hangar-contact active hostiles"
        Assert-PositiveInt -Value ([int]$hangar.visibleHostiles) -Label "hangar-contact visible hostiles"
        Assert-Contains -Text ([string]$hangar.playableFlowPolish) -Needle 'ContactPressureCue=objective-panel+in-world' -Label "hangar-contact pressure cue"
    }

    $damage = Get-ReportRow -Rows $evidenceRows -Preset "damage-demo"
    if ($null -ne $damage) {
        Assert-Contains -Text ([string]$damage.playableFlowPolish) -Needle 'damageStory=section-loss+ejection+wreck' -Label "damage-demo flow story"
        Assert-Contains -Text ([string]$damage.debriefRewardSummary) -Needle 'DamageDebrief=section-status+repair-line' -Label "damage-demo debrief repair line"
        Assert-Contains -Text ([string]$damage.debriefRewardSummary) -Needle 'cockpitEjection=ready' -Label "damage-demo cockpit ejection"
        Assert-Contains -Text ([string]$damage.debriefRewardSummary) -Needle 'repairCost=9288' -Label "damage-demo repair cost"
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

Assert-Contains -Text $bootstrap -Needle 'BuildCaptureInvestorProxyVisualSummary()' -Label "bootstrap proxy source"
Assert-Contains -Text $bootstrap -Needle 'return "InvestorProxyVisuals="' -Label "bootstrap proxy summary prefix"
Assert-Contains -Text $bootstrap -Needle 'string state = unitProxyCount > 0 || propProxyCount > 0 ? "active" : "not-needed";' -Label "bootstrap proxy active source"
Assert-Contains -Text $bootstrap -Needle 'unitFallbackProxy=mech-silhouette+vehicle-hull+infantry-fireteam' -Label "bootstrap proxy unit language"
Assert-Contains -Text $bootstrap -Needle 'proxyIdentity=role-silhouette+faction-color+scale-language' -Label "bootstrap proxy identity source"
Assert-Contains -Text $bootstrap -Needle 'materialLanguage=painted-panels+emissive-cockpit+weapon-hardpoints' -Label "bootstrap proxy material source"
Assert-Contains -Text $bootstrap -Needle 'propFallbackProxy=tree-canopy+building-roof+hardprop-stripe' -Label "bootstrap proxy prop language"
Assert-Contains -Text $bootstrap -Needle 'propIdentity=canopy+roof+hazard-stripe' -Label "bootstrap prop identity source"
Assert-Contains -Text $bootstrap -Needle 'DamageInvestorCallout=section-loss+cockpit-ejection+wreck-salvage+repair-line' -Label "bootstrap damage investor callout"
Assert-Contains -Text $bootstrap -Needle 'SparseBattleUi=statusRows+sections+solo' -Label "bootstrap sparse HUD marker"

Add-Finding -Area "evidence-envelope" -Status "pass" -Detail "F43 command evidence is pass, 1280x720, five required presets, and points to existing screenshot/sidecar/log files"
Add-Finding -Area "investor-summary" -Status "pass" -Detail "Markdown has executive summary, preset highlights, investor proxy identity, and damage callout"
Add-Finding -Area "damage-ejection-debrief" -Status "pass" -Detail "damage-demo proves arm/leg/cockpit loss, cockpit ejection, wreck salvage language, and repair-line debrief"
Add-Finding -Area "public-safe-proxy-visuals" -Status "pass" -Detail "Proxy-only identity is explicit and keeps collision/pathing/BattleCore unchanged"
Add-Finding -Area "sparse-hud-landscape" -Status "pass" -Detail "All five rows keep SparseBattleUi and mobileLandscapeOnly=True/orientation=landscape markers"

Add-FollowUp -Priority "P1" -Area "raw-sidecar-proxy-language" -Issue "Current refreshed sidecars carry proxy identity through investorProxyVisuals and source markers, but not a separate raw proxyIdentity/materialLanguage/propIdentity tuple." -NextFix "When it is safe to recapture Unity evidence, emit the split proxy identity fields into each sidecar for easier investor-review parsing."
Add-FollowUp -Priority "P1" -Area "route-summary-compactness" -Issue "The evidence is complete, but the investor route could use one compact route-level section linking summary, preset highlights, screenshots, and demo launch path." -NextFix "Add a short investor route summary to the playable demo evidence doc without adding runtime UI."
Add-FollowUp -Priority "P2" -Area "damage-proof-link" -Issue "Damage callout is present, but the report can make the exact sidecar/screenshot reference more visible." -NextFix "Add explicit damage-demo screenshot and sidecar links to the investor evidence markdown."
Add-FollowUp -Priority "P2" -Area "mobile-landscape-proof-line" -Issue "Landscape-only proof exists in raw rows and scripts; the investor-facing markdown should keep the phone-horizontal claim visible." -NextFix "Carry the mobileLandscapeOnly=True proof line into the compact route summary."

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor evidence refresh audit failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor evidence refresh audit check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorEvidenceRefreshAudit"
    result = "pass-with-followups"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F44 audit post-F43 PC controlled-demo investor evidence refresh"
    nextFormalTask = "F45 implement post-F44 PC controlled-demo investor evidence polish fixes"
    sourceCommandEvidenceReport = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json"
    sourceRefreshReport = "analysis-output/pc-controlled-demo-investor-evidence-refresh/pc-controlled-demo-investor-evidence-refresh.json"
    noUnityLaunch = $true
    auditedPresets = $requiredPresets
    findings = $findings.ToArray()
    followUps = $followUps.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Evidence Refresh Audit")
$markdownLines.Add("")
$markdownLines.Add("Result: pass-with-followups")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F44 audit post-F43 PC controlled-demo investor evidence refresh`')
$markdownLines.Add('Next formal task: `F45 implement post-F44 PC controlled-demo investor evidence polish fixes`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add('Source command evidence: `analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json`')
$markdownLines.Add('Source refresh report: `analysis-output/pc-controlled-demo-investor-evidence-refresh/pc-controlled-demo-investor-evidence-refresh.json`')
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

Write-Host "PC controlled-demo investor evidence refresh audit OK."
Write-Host "Report: $reportJsonPath"
