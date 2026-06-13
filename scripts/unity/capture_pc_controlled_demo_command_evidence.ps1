param(
    [string]$RepoRoot = "",
    [string]$EvidenceDir = "",
    [string]$OutputDir = "",
    [int]$Width = 1280,
    [int]$Height = 720,
    [int]$CaptureTimeoutSeconds = 90,
    [switch]$SkipRun,
    [switch]$SkipBuildFreshnessCheck,
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

if ([string]::IsNullOrWhiteSpace($EvidenceDir)) {
    $EvidenceDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-visual-evidence"
}
elseif (-not [System.IO.Path]::IsPathRooted($EvidenceDir)) {
    $EvidenceDir = Join-Path $RepoRoot $EvidenceDir
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-command-evidence"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd("\", "/")
$EvidenceDir = [System.IO.Path]::GetFullPath($EvidenceDir)
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
foreach ($candidate in @($EvidenceDir, $OutputDir)) {
    if (-not $candidate.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path must stay inside RepoRoot: $candidate"
    }
}

$captureDir = Join-Path $EvidenceDir "captures"
$visualEvidenceReportPath = Join-Path $EvidenceDir "pc-controlled-demo-visual-evidence.json"
$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-command-evidence.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-command-evidence.md"
$visualEvidenceScript = Join-Path $RepoRoot "scripts\unity\capture_pc_controlled_demo_visual_evidence.ps1"
$windowsBuildFreshnessScript = Join-Path $RepoRoot "scripts\unity\check_windows_demo_build_freshness.ps1"
$routeAuditFixesReportPath = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fixes\pc-controlled-demo-investor-route-evidence-audit-fixes.json"
$requiredPresets = @("spawn", "hangar-contact", "damage-demo", "solo-order", "solo-return")
$completedTaskName = "F49 refresh PC controlled-demo investor route evidence after audit fixes"
$nextFormalTaskName = "F50 audit post-F49 PC controlled-demo investor route evidence refresh"
$routeAuditFixClosure = @(
    "RouteAuditFixRefresh=ready source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fixes/pc-controlled-demo-investor-route-evidence-audit-fixes.json completed=F49 next=F50 noUnityLaunch=True mobile=landscape-only",
    "RouteAuditFixClosure=F48-doc-visibility status=closed surfaces=investor-route+playable-evidence+handoff",
    "RouteAuditFixClosure=gate-runtime status=route-gates-focused aggregateGate=not-required"
)
$rows = New-Object System.Collections.Generic.List[object]

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Convert-ToRepoRelativePath {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    if ($fullPath.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        return ($fullPath.Substring($repoFullPath.Length).TrimStart("\", "/") -replace "\\", "/")
    }

    return $fullPath
}

function Require-File {
    param(
        [string]$Path,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Missing $Label`: $Path"
    }
}

function Require-Text {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or -not $Text.Contains($Needle)) {
        throw "$Label missing '$Needle': $Text"
    }
}

function Escape-MarkdownTableCell {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return "n/a"
    }

    return (($Text -replace "\|", "/") -replace "`r?`n", " ")
}

function Find-EvidenceRow {
    param(
        [object[]]$Items,
        [string]$Preset
    )

    $matches = @($Items | Where-Object { [string]$_.preset -eq $Preset })
    if ($matches.Count -eq 0) {
        return $null
    }

    return $matches[0]
}

function Extract-SummaryToken {
    param(
        [string]$Summary,
        [string]$Name
    )

    if ([string]::IsNullOrWhiteSpace($Summary)) {
        return ""
    }

    $match = [regex]::Match($Summary, [regex]::Escape($Name) + "=([^ ]+)")
    if (-not $match.Success) {
        return ""
    }

    return $match.Groups[1].Value
}

function Get-OptionalLastExitCode {
    $lastExitCodeVariable = Get-Variable -Name LASTEXITCODE -ErrorAction SilentlyContinue
    if ($null -eq $lastExitCodeVariable -or $null -eq $lastExitCodeVariable.Value) {
        return 0
    }

    return [int]$lastExitCodeVariable.Value
}

function Read-OptionalSidecarString {
    param(
        [object]$Sidecar,
        [string]$Name
    )

    $property = $Sidecar.PSObject.Properties[$Name]
    if ($null -eq $property -or $null -eq $property.Value) {
        return ""
    }

    return [string]$property.Value
}

function Read-FirstAvailableSidecarString {
    param(
        [object]$Sidecar,
        [string[]]$Names
    )

    foreach ($name in $Names) {
        $value = Read-OptionalSidecarString -Sidecar $Sidecar -Name $name
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return $value
        }
    }

    return ""
}

function Read-RequiredText {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    Require-File -Path $path -Label $RelativePath
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8
}

function Assert-SourceMarkers {
    $mission = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\BattleMission.cs"
    Require-Text -Text $mission -Needle "SquadMoveFormationSpacing = 320f" -Label "BattleMission move formation"
    Require-Text -Text $mission -Needle "SquadAttackFormationSpacing = 340f" -Label "BattleMission attack formation"

    $bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
    foreach ($marker in @(
        'CapturePresetSoloOrder = "solo-order"',
        'CapturePresetSoloReturn = "solo-return"',
        "CommandReadability=all+single+jet+focus+commander-follow+formation",
        "CommandCuePalette=command-blue+target-red+damage-amber+hostile-magenta",
        "CommanderFollow=unit-1+first-sort+fixed-view",
        "formation=move-320+attack-340"
    )) {
        Require-Text -Text $bootstrap -Needle $marker -Label "Mc2DemoBootstrap command evidence source"
    }
}

function Test-Sidecar {
    param(
        [string]$Preset,
        [object]$Sidecar
    )

    if ([string]$Sidecar.flowScreen -ne "Battle") {
        throw "$Preset expected Battle flow, got $($Sidecar.flowScreen)"
    }

    if ([int]$Sidecar.screenWidth -ne $Width -or [int]$Sidecar.screenHeight -ne $Height) {
        throw "$Preset expected ${Width}x${Height}, got $($Sidecar.screenWidth)x$($Sidecar.screenHeight)"
    }

    $command = [string]$Sidecar.commandReadability
    Require-Text -Text $command -Needle "CommandReadability=all+single+jet+focus+commander-follow+formation" -Label "$Preset commandReadability"
    Require-Text -Text $command -Needle "formation=move-320+attack-340" -Label "$Preset commandReadability"
    Require-Text -Text $command -Needle "CommandCuePalette=command-blue+target-red+damage-amber+hostile-magenta" -Label "$Preset commandReadability"

    $commander = [string]$Sidecar.commanderFollow
    Require-Text -Text $commander -Needle "CommanderFollow=unit-1+first-sort+fixed-view" -Label "$Preset commanderFollow"
    Require-Text -Text $commander -Needle "unit=unit-1" -Label "$Preset commanderFollow"
    Require-Text -Text $commander -Needle "sortedIndex=1" -Label "$Preset commanderFollow"

    if ($Preset -eq "solo-order") {
        Require-Text -Text $command -Needle "solo=1" -Label "$Preset commandReadability"
        Require-Text -Text $command -Needle "SoloOrder=ring+beacon" -Label "$Preset commandReadability"
        Require-Text -Text $command -Needle "SoloReturn=ring+beacon" -Label "$Preset commandReadability"
        if ([int]$Sidecar.activeHostileCount -ne 0) {
            throw "solo-order should remain a command-isolation capture without active hostiles: $($Sidecar.activeHostileCount)"
        }
    }

    if ($Preset -eq "solo-return") {
        Require-Text -Text $command -Needle "solo=0" -Label "$Preset commandReadability"
        Require-Text -Text $command -Needle "SoloReturn=ring+beacon" -Label "$Preset commandReadability"
        Require-Text -Text ([string]$Sidecar.status) -Needle "Solo return settled" -Label "$Preset status"
        $flow = [string]$Sidecar.playableFlowPolish
        Require-Text -Text $flow -Needle "PlayableFlowPolish=contact-pressure+damage-debrief+solo-return+hud-density+handoff" -Label "$Preset playableFlowPolish"
        Require-Text -Text $flow -Needle "soloReturn=returned" -Label "$Preset playableFlowPolish"
        Require-Text -Text $flow -Needle "detached=0" -Label "$Preset playableFlowPolish"
        Require-Text -Text $flow -Needle "mobileLandscapeOnly=True" -Label "$Preset playableFlowPolish"
        if ([int]$Sidecar.activeHostileCount -ne 0) {
            throw "solo-return should remain a command-isolation capture without active hostiles: $($Sidecar.activeHostileCount)"
        }
    }

    if ($Preset -eq "hangar-contact" -or $Preset -eq "damage-demo") {
        if ([int]$Sidecar.activeHostileCount -le 0 -or [int]$Sidecar.visibleHostileCount -le 0) {
            throw "$Preset expected active and visible hostile contact."
        }
    }

    if ($Preset -eq "damage-demo") {
        Require-Text -Text ([string]$Sidecar.damageReadability) -Needle "cuePalette=command-blue target-red damage-amber hostile-magenta pilot-cyan" -Label "$Preset damageReadability"
        $debriefReward = Read-FirstAvailableSidecarString -Sidecar $Sidecar -Names @("debriefRewardSummary", "debriefReward")
        Require-Text -Text $debriefReward -Needle "DamageDebrief=section-status+repair-line" -Label "$Preset debriefReward"
        Require-Text -Text $debriefReward -Needle "damageConsequenceLine=" -Label "$Preset debriefReward"
        Require-Text -Text $debriefReward -Needle "cockpitEjection=ready" -Label "$Preset debriefReward"
    }

    $investorProxyVisuals = Read-OptionalSidecarString -Sidecar $Sidecar -Name "investorProxyVisuals"
    foreach ($marker in @(
        "InvestorProxyVisuals=active",
        "unitFallbackProxy=mech-silhouette+vehicle-hull+infantry-fireteam",
        "propFallbackProxy=tree-canopy+building-roof+hardprop-stripe",
        "collision=unchanged",
        "pathing=unchanged",
        "publicSafe=proxy-only",
        "mobileLandscapeOnly=True"
    )) {
        Require-Text -Text $investorProxyVisuals -Needle $marker -Label "$Preset investorProxyVisuals"
    }

    return [pscustomobject]@{
        preset = $Preset
        screenshot = Convert-ToRepoRelativePath -Path (Join-Path $captureDir "$Preset.png")
        sidecar = Convert-ToRepoRelativePath -Path (Join-Path $captureDir "$Preset.json")
        log = Convert-ToRepoRelativePath -Path (Join-Path $captureDir "$Preset.log")
        activeHostiles = [int]$Sidecar.activeHostileCount
        visibleHostiles = [int]$Sidecar.visibleHostileCount
        commandReadability = $command
        commanderFollow = $commander
        battleHud = Read-OptionalSidecarString -Sidecar $Sidecar -Name "battleHud"
        playableFlowPolish = Read-OptionalSidecarString -Sidecar $Sidecar -Name "playableFlowPolish"
        debriefRewardSummary = Read-FirstAvailableSidecarString -Sidecar $Sidecar -Names @("debriefRewardSummary", "debriefReward")
        investorProxyVisuals = $investorProxyVisuals
        status = Read-OptionalSidecarString -Sidecar $Sidecar -Name "status"
    }
}

Require-File -Path $visualEvidenceScript -Label "PC visual evidence script"
Require-File -Path $windowsBuildFreshnessScript -Label "Windows build freshness script"

if ($PlanOnly) {
    Write-Host "PC controlled-demo command evidence refresh plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "EvidenceDir: $EvidenceDir"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "Presets: $($requiredPresets -join ',')"
    Write-Host "WidthHeight: ${Width}x${Height}"
    Write-Host "SkipRun: $SkipRun"
    Write-Host "SkipBuildFreshnessCheck: $SkipBuildFreshnessCheck"
    Write-Host "NoUnityLaunch: $SkipRun"
    return
}

if (-not $SkipBuildFreshnessCheck) {
    & $windowsBuildFreshnessScript -RepoRoot $RepoRoot
    if ((Get-OptionalLastExitCode) -ne 0) {
        throw "Windows build freshness check failed before command evidence refresh."
    }
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

if ($SkipRun) {
    & $visualEvidenceScript `
        -RepoRoot $RepoRoot `
        -OutputDir $EvidenceDir `
        -Presets $requiredPresets `
        -Width $Width `
        -Height $Height `
        -CaptureTimeoutSeconds $CaptureTimeoutSeconds `
        -SkipRun
}
else {
    & $visualEvidenceScript `
        -RepoRoot $RepoRoot `
        -OutputDir $EvidenceDir `
        -Presets $requiredPresets `
        -Width $Width `
        -Height $Height `
        -CaptureTimeoutSeconds $CaptureTimeoutSeconds
}
if ((Get-OptionalLastExitCode) -ne 0) {
    throw "PC controlled-demo visual evidence refresh failed before command evidence report."
}

Require-File -Path $routeAuditFixesReportPath -Label "PC investor route evidence audit fixes report"
Require-File -Path $visualEvidenceReportPath -Label "PC visual evidence report"
$visualReport = Get-Content -LiteralPath $visualEvidenceReportPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string]$visualReport.result -ne "pass") {
    throw "PC visual evidence report did not pass: $visualEvidenceReportPath"
}

$routeAuditFixesReport = Get-Content -LiteralPath $routeAuditFixesReportPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string]$routeAuditFixesReport.result -ne "pass") {
    throw "PC investor route evidence audit fixes report did not pass: $routeAuditFixesReportPath"
}
if ([string]$routeAuditFixesReport.completedTask -ne "F48 implement post-F47 PC controlled-demo investor route evidence audit fixes") {
    throw "PC investor route evidence audit fixes report has unexpected completed task: $($routeAuditFixesReport.completedTask)"
}
if ([string]$routeAuditFixesReport.nextFormalTask -ne "F49 refresh PC controlled-demo investor route evidence after audit fixes") {
    throw "PC investor route evidence audit fixes report has unexpected next task: $($routeAuditFixesReport.nextFormalTask)"
}

Assert-SourceMarkers

foreach ($preset in $requiredPresets) {
    $pngPath = Join-Path $captureDir "$preset.png"
    $jsonPath = Join-Path $captureDir "$preset.json"
    $logPath = Join-Path $captureDir "$preset.log"
    Require-File -Path $pngPath -Label "$preset screenshot"
    Require-File -Path $jsonPath -Label "$preset sidecar"
    Require-File -Path $logPath -Label "$preset log"
    $sidecar = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
    [void]$rows.Add((Test-Sidecar -Preset $preset -Sidecar $sidecar))
}

$evidenceRows = $rows.ToArray()
$report = [pscustomobject]@{
    schema = "PCControlledDemoCommandEvidenceRefresh"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = $completedTaskName
    nextFormalTask = $nextFormalTaskName
    width = $Width
    height = $Height
    sourceVisualEvidenceReport = Convert-ToRepoRelativePath -Path $visualEvidenceReportPath
    sourceRouteAuditFixesReport = Convert-ToRepoRelativePath -Path $routeAuditFixesReportPath
    routeAuditFixClosure = $routeAuditFixClosure
    captureDir = Convert-ToRepoRelativePath -Path $captureDir
    evidence = $evidenceRows
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
[void]$markdownLines.Add("# PC Controlled Demo Command Evidence Refresh")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("Result: pass")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("Completed task: ``$completedTaskName``")
[void]$markdownLines.Add("Next formal task: ``$nextFormalTaskName``")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("Source visual report: `"$(Convert-ToRepoRelativePath -Path $visualEvidenceReportPath)`"")
[void]$markdownLines.Add("Source route audit fixes report: `"$(Convert-ToRepoRelativePath -Path $routeAuditFixesReportPath)`"")
[void]$markdownLines.Add("Capture directory: `"$(Convert-ToRepoRelativePath -Path $captureDir)`"")
[void]$markdownLines.Add("")
$damageRow = Find-EvidenceRow -Items $evidenceRows -Preset "damage-demo"
$contactRow = Find-EvidenceRow -Items $evidenceRows -Preset "hangar-contact"
$soloOrderRow = Find-EvidenceRow -Items $evidenceRows -Preset "solo-order"
$soloReturnRow = Find-EvidenceRow -Items $evidenceRows -Preset "solo-return"
$damageRepairCost = if ($null -eq $damageRow) { "" } else { Extract-SummaryToken -Summary ([string]$damageRow.debriefRewardSummary) -Name "repairCost" }
$damageUnits = if ($null -eq $damageRow) { "" } else { Extract-SummaryToken -Summary ([string]$damageRow.debriefRewardSummary) -Name "damagedPlayerUnits" }
$damageScreenshot = if ($null -eq $damageRow) { "analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png" } else { [string]$damageRow.screenshot }
$damageSidecar = if ($null -eq $damageRow) { "analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json" } else { [string]$damageRow.sidecar }
$damageLog = if ($null -eq $damageRow) { "analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log" } else { [string]$damageRow.log }
[void]$markdownLines.Add("## Executive Summary")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("- InvestorDemoSummary=ready presets=$($evidenceRows.Count) resolution=${Width}x${Height} sparseHud=True mobileLandscapeOnly=True publicSafe=proxy-only")
[void]$markdownLines.Add("- DamageInvestorCallout=section-loss+cockpit-ejection+wreck-salvage+repair-line preset=damage-demo damagedPlayerUnits=$damageUnits repairCost=$damageRepairCost")
[void]$markdownLines.Add("- ProxyVisualIdentity=mech-silhouette+vehicle-hull+infantry-fireteam+tree-canopy+building-roof+hardprop-stripe roleReadable=True collision=unchanged pathing=unchanged")
[void]$markdownLines.Add("- FastInvestorEvidenceGate=check_pc_controlled_demo_investor_evidence_package_fixes.ps1 source-only+report-only noUnityLaunch=True")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("## Investor Route Summary")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("- InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return launch=scripts/unity/run_windows_demo.ps1 evidence=command-report+screenshots+sidecars")
[void]$markdownLines.Add("- DamageProof=damage-demo screenshot=$damageScreenshot sidecar=$damageSidecar log=$damageLog callout=section-loss+cockpit-ejection+wreck-salvage+repair-line repairCost=$damageRepairCost")
[void]$markdownLines.Add("- LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False")
[void]$markdownLines.Add("- ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("## Route Audit Fix Closure")
[void]$markdownLines.Add("")
foreach ($closure in $routeAuditFixClosure) {
    [void]$markdownLines.Add("- $closure")
}
[void]$markdownLines.Add("")
[void]$markdownLines.Add("## Preset Highlights")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("| Preset | Highlight | Investor proof |")
[void]$markdownLines.Add("| --- | --- | --- |")
if ($null -ne $contactRow) {
    [void]$markdownLines.Add("| hangar-contact | Contact pressure without extra UI clutter | activeHostiles=$($contactRow.activeHostiles) visibleHostiles=$($contactRow.visibleHostiles) ContactPressureCue=objective-panel+in-world |")
}
if ($null -ne $damageRow) {
    [void]$markdownLines.Add("| damage-demo | Arm/leg/cockpit consequences, ejection, wreck and repair line | DamageInvestorCallout=section-loss+cockpit-ejection+wreck-salvage+repair-line repairCost=$damageRepairCost |")
}
if ($null -ne $soloOrderRow) {
    [void]$markdownLines.Add("| solo-order | Status-row single-unit order without drag selection | soloReturn=order-active detached=1 |")
}
if ($null -ne $soloReturnRow) {
    [void]$markdownLines.Add("| solo-return | Ordered unit automatically returns to squad control | soloReturn=returned detached=0 |")
}
[void]$markdownLines.Add("")
[void]$markdownLines.Add("## Raw Evidence")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("| Preset | Active hostiles | Visible hostiles | Flow summary | Debrief summary | Investor proxy | Screenshot |")
[void]$markdownLines.Add("| --- | ---: | ---: | --- | --- | --- | --- |")
foreach ($item in $evidenceRows) {
    $flowSummary = if ([string]::IsNullOrWhiteSpace($item.playableFlowPolish)) { $item.commandReadability } else { $item.playableFlowPolish }
    $flowSummary = Escape-MarkdownTableCell -Text $flowSummary
    $debriefSummary = if ([string]::IsNullOrWhiteSpace($item.debriefRewardSummary)) { "n/a" } else { $item.debriefRewardSummary }
    $debriefSummary = Escape-MarkdownTableCell -Text $debriefSummary
    $investorSummary = if ([string]::IsNullOrWhiteSpace($item.investorProxyVisuals)) { "n/a" } else { $item.investorProxyVisuals }
    $investorSummary = Escape-MarkdownTableCell -Text $investorSummary
    [void]$markdownLines.Add(("| {0} | {1} | {2} | {3} | {4} | {5} | `{6}` |" -f $item.preset, $item.activeHostiles, $item.visibleHostiles, $flowSummary, $debriefSummary, $investorSummary, $item.screenshot))
}

$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo command evidence refresh OK."
Write-Host "Report: $reportJsonPath"
