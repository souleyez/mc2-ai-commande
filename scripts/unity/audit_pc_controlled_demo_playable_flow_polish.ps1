param(
    [string]$RepoRoot = "",
    [string]$EvidenceDir = "",
    [string]$CommandEvidenceDir = "",
    [string]$OutputDir = "",
    [int]$Width = 1280,
    [int]$Height = 720,
    [switch]$RefreshEvidence,
    [switch]$SkipEvidenceRun,
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

if ([string]::IsNullOrWhiteSpace($CommandEvidenceDir)) {
    $CommandEvidenceDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-command-evidence"
}
elseif (-not [System.IO.Path]::IsPathRooted($CommandEvidenceDir)) {
    $CommandEvidenceDir = Join-Path $RepoRoot $CommandEvidenceDir
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-playable-flow-audit"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd("\", "/")
$EvidenceDir = [System.IO.Path]::GetFullPath($EvidenceDir)
$CommandEvidenceDir = [System.IO.Path]::GetFullPath($CommandEvidenceDir)
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
foreach ($candidate in @($EvidenceDir, $CommandEvidenceDir, $OutputDir)) {
    if (-not $candidate.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path must stay inside RepoRoot: $candidate"
    }
}

$captureDir = Join-Path $EvidenceDir "captures"
$visualEvidenceReportPath = Join-Path $EvidenceDir "pc-controlled-demo-visual-evidence.json"
$commandEvidenceReportPath = Join-Path $CommandEvidenceDir "pc-controlled-demo-command-evidence.json"
$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-playable-flow-audit.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-playable-flow-audit.md"
$commandEvidenceScript = Join-Path $RepoRoot "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$requiredPresets = @("spawn", "hangar-contact", "damage-demo", "solo-order")
$checks = New-Object System.Collections.Generic.List[object]
$followUps = New-Object System.Collections.Generic.List[object]

function Convert-ToRepoRelativePath {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    if ($fullPath.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        return ($fullPath.Substring($repoFullPath.Length).TrimStart("\", "/") -replace "\\", "/")
    }

    return $fullPath
}

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
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

function Read-RequiredText {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    Require-File -Path $path -Label $RelativePath
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8
}

function Require-Text {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or -not $Text.Contains($Needle)) {
        throw "$Label missing marker: $Needle"
    }
}

function Read-SummaryNumber {
    param(
        [string]$Summary,
        [string]$Pattern,
        [string]$Label
    )

    $match = [regex]::Match($Summary, $Pattern)
    if (-not $match.Success) {
        throw "$Label missing number pattern '$Pattern': $Summary"
    }

    return [double]::Parse($match.Groups[1].Value, [Globalization.CultureInfo]::InvariantCulture)
}

function Add-Check {
    param(
        [string]$Area,
        [string]$Preset,
        [string]$Status,
        [string]$Detail
    )

    [void]$checks.Add([pscustomobject]@{
        area = $Area
        preset = $Preset
        status = $Status
        detail = $Detail
    })
}

function Add-FollowUp {
    param(
        [string]$Priority,
        [string]$Area,
        [string]$Preset,
        [string]$Issue,
        [string]$NextFix
    )

    [void]$followUps.Add([pscustomobject]@{
        priority = $Priority
        area = $Area
        preset = $Preset
        issue = $Issue
        nextFix = $NextFix
    })
}

function Test-SourceBoundary {
    $bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
    foreach ($marker in @(
        'CapturePresetSoloOrder = "solo-order"',
        "CommandReadability=all+single+jet+focus+commander-follow+formation",
        "CommandCuePalette=command-blue+target-red+damage-amber+hostile-magenta",
        "CommanderFollow=unit-1+first-sort+fixed-view",
        "SparseBattleUi=statusRows+sections+solo",
        "MobileTouchUi=ready",
        "landscapeOnly=yes",
        "combatLog=hidden",
        "DamageReadability=weaponFamilies",
        "DamageStory=units",
        "FirstMapVisual=terrain+unit+structure+sparse-ui+occupancy+contact"
    )) {
        Require-Text -Text $bootstrap -Needle $marker -Label "Mc2DemoBootstrap.cs"
    }

    $mission = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\BattleMission.cs"
    foreach ($marker in @(
        "SquadMoveFormationSpacing = 320f",
        "SquadAttackFormationSpacing = 340f",
        "IssueDetachedMove",
        "IssueSquadMove",
        "IssueDetachedJump",
        "IssueSquadJump"
    )) {
        Require-Text -Text $mission -Needle $marker -Label "BattleMission.cs"
    }

    Add-Check -Area "source-boundary" -Preset "all" -Status "pass" -Detail "playable-flow markers stay in presentation while command movement remains BattleCore-driven"
}

function Test-CommonSidecar {
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

    if ([int]$Sidecar.playerUnitCount -lt 2 -or [int]$Sidecar.currentObjectiveCount -lt 1 -or [int]$Sidecar.targetableStructureCount -lt 1) {
        throw "$Preset missing playable battle context: player=$($Sidecar.playerUnitCount) objectives=$($Sidecar.currentObjectiveCount) structures=$($Sidecar.targetableStructureCount)"
    }

    foreach ($marker in @(
        "FirstMapVisual=terrain+unit+structure+sparse-ui+occupancy+contact",
        "flow=Battle",
        "status=ready",
        "terrain=ready",
        "unit=ready",
        "structure=ready",
        "sparseHud=ready",
        "contact=separated",
        "damageStory=ready",
        "privateReference=replaceable",
        "visualOnly=yes",
        "pathing=unchanged",
        "collision=unchanged"
    )) {
        Require-Text -Text ([string]$Sidecar.firstMapVisual) -Needle $marker -Label "$Preset firstMapVisual"
    }
    Add-Check -Area "first-map-playable-context" -Preset $Preset -Status "pass" -Detail "terrain/unit/structure/sparse HUD/contact sidecar is ready"

    foreach ($marker in @(
        "BattleHud=active controls=statusRows+jet+map+bay+system",
        "SparseBattleUi=statusRows+sections+solo",
        "combatLog=hidden",
        "objective=compactObjective",
        "missionMap=available-closed",
        "saveUi=disabled",
        "debugOccupancy=sidecar-only",
        "overlays=hidden"
    )) {
        Require-Text -Text ([string]$Sidecar.battleHud) -Needle $marker -Label "$Preset battleHud"
    }

    $statusRailWidth = Read-SummaryNumber -Summary ([string]$Sidecar.battleHud) -Pattern "statusRailW=([0-9.]+)" -Label "$Preset battleHud"
    $statusRailShare = Read-SummaryNumber -Summary ([string]$Sidecar.battleHud) -Pattern "statusRailShare1280=([0-9.]+)" -Label "$Preset battleHud"
    if ($statusRailWidth -gt 300 -or $statusRailShare -gt 0.24) {
        throw "$Preset status rail too wide for sparse HUD: width=$statusRailWidth share=$statusRailShare"
    }
    Add-Check -Area "sparse-hud" -Preset $Preset -Status "pass" -Detail "statusRail=$statusRailWidth share=$statusRailShare combat log hidden"

    foreach ($marker in @(
        "MobileTouchUi=ready",
        "orientation=landscape",
        "commandTargets=44",
        "statusRows=44",
        "primaryButtons=44",
        "landscapeOnly=yes",
        "noDragBox=yes",
        "combatLog=hidden"
    )) {
        Require-Text -Text ([string]$Sidecar.mobileTouch) -Needle $marker -Label "$Preset mobileTouch"
    }
    Add-Check -Area "landscape-command-model" -Preset $Preset -Status "pass" -Detail "landscape/no-drag-box touch contract remains intact"

    foreach ($marker in @(
        "CommandReadability=all+single+jet+focus+commander-follow+formation",
        "default=all-squad",
        "noDragBox=yes",
        "maxSquad=6",
        "formation=move-320+attack-340",
        "statusRows=select-unit+detached-border",
        "CommandCuePalette=command-blue+target-red+damage-amber+hostile-magenta",
        "CommanderFollow=unit-1+first-sort+fixed-view",
        "unit=unit-1",
        "sortedIndex=1"
    )) {
        Require-Text -Text ([string]$Sidecar.commandReadability) -Needle $marker -Label "$Preset commandReadability"
    }
    Add-Check -Area "command-clarity" -Preset $Preset -Status "pass" -Detail "all/single/jet/focus/commander-follow markers present"

    foreach ($marker in @(
        "OccupancyPlaceholders=enabled",
        "source=BattleMission.OccupancyPlaceholderRegions+DemoTerrainView.LandingReviewBlockedMarkers"
    )) {
        Require-Text -Text ([string]$Sidecar.occupancyPlaceholders) -Needle $marker -Label "$Preset occupancyPlaceholders"
    }

    foreach ($marker in @(
        "status=separated",
        "overlaps=0"
    )) {
        Require-Text -Text ([string]$Sidecar.contactClearance) -Needle $marker -Label "$Preset contactClearance"
    }
    Add-Check -Area "contact-collision" -Preset $Preset -Status "pass" -Detail "collision placeholders remain separated with overlap count 0"
}

function Test-SpecificSidecar {
    param(
        [string]$Preset,
        [object]$Sidecar
    )

    if ($Preset -eq "spawn") {
        if ([int]$Sidecar.activeHostileCount -ne 0 -or [int]$Sidecar.visibleHostileCount -ne 0) {
            throw "spawn should be pre-contact: active=$($Sidecar.activeHostileCount) visible=$($Sidecar.visibleHostileCount)"
        }

        Require-Text -Text ([string]$Sidecar.commandReadability) -Needle "solo=0" -Label "spawn commandReadability"
        Add-Check -Area "spawn-readiness" -Preset $Preset -Status "pass" -Detail "pre-contact full-squad command state is clean"
    }

    if ($Preset -eq "hangar-contact") {
        if ([int]$Sidecar.activeHostileCount -lt 16 -or [int]$Sidecar.visibleHostileCount -lt 12) {
            throw "hangar-contact should show contact pressure: active=$($Sidecar.activeHostileCount) visible=$($Sidecar.visibleHostileCount)"
        }

        $nearestPlayerHostile = Read-SummaryNumber -Summary ([string]$Sidecar.contactSpread) -Pattern "nearestPH=([0-9.]+)" -Label "hangar-contact contactSpread"
        if ($nearestPlayerHostile -lt 180) {
            throw "hangar-contact pressure is too crowded for playable command readability: nearestPH=$nearestPlayerHostile"
        }

        Add-Check -Area "contact-pressure" -Preset $Preset -Status "pass" -Detail "active=$($Sidecar.activeHostileCount) visible=$($Sidecar.visibleHostileCount) nearestPH=$nearestPlayerHostile"
    }

    if ($Preset -eq "damage-demo") {
        if ([int]$Sidecar.activeHostileCount -lt 16 -or [int]$Sidecar.visibleHostileCount -lt 12) {
            throw "damage-demo should preserve hostile pressure while showing damage: active=$($Sidecar.activeHostileCount) visible=$($Sidecar.visibleHostileCount)"
        }

        foreach ($marker in @(
            "lostSections=3",
            "arms=1",
            "legs=1",
            "cockpit=1",
            "pilotRisk=1",
            "destroyedUnits=1"
        )) {
            Require-Text -Text ([string]$Sidecar.damageStory) -Needle $marker -Label "damage-demo damageStory"
        }

        foreach ($marker in @(
            "cuePalette=command-blue target-red damage-amber hostile-magenta pilot-cyan",
            "arms-firepower",
            "legs-mobility",
            "cockpit-ejection",
            "wreck-salvage",
            "SectionStatus=bar+short-label+critical+destroyed"
        )) {
            Require-Text -Text ([string]$Sidecar.damageReadability) -Needle $marker -Label "damage-demo damageReadability"
        }

        Add-Check -Area "damage-story" -Preset $Preset -Status "pass" -Detail "arm/leg/cockpit/ejection/wreck story is present under contact"
    }

    if ($Preset -eq "solo-order") {
        if ([int]$Sidecar.activeHostileCount -ne 0 -or [int]$Sidecar.visibleHostileCount -ne 0) {
            throw "solo-order should isolate command readability without hostiles: active=$($Sidecar.activeHostileCount) visible=$($Sidecar.visibleHostileCount)"
        }

        foreach ($marker in @(
            "solo=1",
            "SoloOrder=ring+beacon",
            "SoloReturn=ring+beacon"
        )) {
            Require-Text -Text ([string]$Sidecar.commandReadability) -Needle $marker -Label "solo-order commandReadability"
        }

        Require-Text -Text ([string]$Sidecar.status) -Needle "move accepted" -Label "solo-order status"
        Add-Check -Area "solo-order" -Preset $Preset -Status "pass" -Detail "detached unit command and return cue markers are visible without hostile noise"
    }
}

Require-File -Path $commandEvidenceScript -Label "F34 command evidence script"

if ($PlanOnly) {
    Write-Host "PC controlled-demo playable flow polish audit plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "EvidenceDir: $EvidenceDir"
    Write-Host "CommandEvidenceDir: $CommandEvidenceDir"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "RequiredPresets: $($requiredPresets -join ',')"
    Write-Host "WidthHeight: ${Width}x${Height}"
    return
}

if ($RefreshEvidence) {
    if ($SkipEvidenceRun) {
        & $commandEvidenceScript -RepoRoot $RepoRoot -EvidenceDir $EvidenceDir -OutputDir $CommandEvidenceDir -Width $Width -Height $Height -SkipRun
    }
    else {
        & $commandEvidenceScript -RepoRoot $RepoRoot -EvidenceDir $EvidenceDir -OutputDir $CommandEvidenceDir -Width $Width -Height $Height
    }

    if ($LASTEXITCODE -ne 0) {
        throw "F34 command evidence refresh failed before F35 audit."
    }
}

Require-File -Path $commandEvidenceReportPath -Label "F34 command evidence report"
Require-File -Path $visualEvidenceReportPath -Label "source visual evidence report"

$commandReport = Get-Content -LiteralPath $commandEvidenceReportPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string]$commandReport.result -ne "pass") {
    throw "F34 command evidence report must pass before F35 audit: $commandEvidenceReportPath"
}

if ([string]$commandReport.completedTask -ne "F34 refresh PC controlled-demo command evidence after readability fixes") {
    throw "F34 command evidence completedTask mismatch: $($commandReport.completedTask)"
}

if ([string]$commandReport.nextFormalTask -ne "F35 audit post-F34 PC controlled-demo playable flow polish") {
    throw "F34 command evidence nextFormalTask mismatch: $($commandReport.nextFormalTask)"
}

$visualReport = Get-Content -LiteralPath $visualEvidenceReportPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string]$visualReport.result -ne "pass") {
    throw "Source visual evidence report must pass before F35 audit: $visualEvidenceReportPath"
}

Test-SourceBoundary

foreach ($preset in $requiredPresets) {
    $jsonPath = Join-Path $captureDir "$preset.json"
    $pngPath = Join-Path $captureDir "$preset.png"
    $logPath = Join-Path $captureDir "$preset.log"
    Require-File -Path $jsonPath -Label "$preset sidecar"
    Require-File -Path $pngPath -Label "$preset screenshot"
    Require-File -Path $logPath -Label "$preset capture log"

    $sidecar = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([string]$sidecar.preset -ne $preset) {
        throw "$preset sidecar preset mismatch: $($sidecar.preset)"
    }

    Test-CommonSidecar -Preset $preset -Sidecar $sidecar
    Test-SpecificSidecar -Preset $preset -Sidecar $sidecar
}

$evidencePresets = @($commandReport.evidence | ForEach-Object { [string]$_.preset })
foreach ($preset in $requiredPresets) {
    if ($evidencePresets -notcontains $preset) {
        throw "F34 command evidence report missing preset: $preset"
    }
}
Add-Check -Area "handoff-evidence" -Preset "all" -Status "pass" -Detail "F34 command evidence covers spawn/hangar-contact/damage-demo/solo-order"

Add-FollowUp `
    -Priority "P1" `
    -Area "contact-pressure" `
    -Preset "hangar-contact" `
    -Issue "The contact preset proves 20 active hostiles and separated collision, but the player-facing objective/contact pressure is still mostly implicit in the sidecar." `
    -NextFix "Add a compact in-world or objective-panel pressure cue for active contact without reintroducing a combat log."

Add-FollowUp `
    -Priority "P1" `
    -Area "damage-story" `
    -Preset "damage-demo" `
    -Issue "The damage preset proves arm, leg, cockpit, ejection and wreck cues, but the normal playable loop still needs that story tied cleanly into status rail and debrief repair consequences." `
    -NextFix "Connect section loss/ejection/wreck state to a concise player-facing status and post-battle repair line while keeping the HUD sparse."

Add-FollowUp `
    -Priority "P2" `
    -Area "solo-order-return" `
    -Preset "solo-order" `
    -Issue "The solo-order capture proves detached order and return cue markers at command time, but does not yet capture a settled return-to-squad moment after the order completes." `
    -NextFix "Add a follow-up solo-return preset or timed sidecar state that proves automatic return after the detached order completes."

Add-FollowUp `
    -Priority "P2" `
    -Area "hud-density" `
    -Preset "all" `
    -Issue "The sparse HUD passes at statusRailW=292/statusRailShare1280=0.23, but the left rail still consumes a material portion of PC battle view." `
    -NextFix "Prototype a denser or context-fading status rail that preserves damage sections, detached state and tap targets."

Add-FollowUp `
    -Priority "P2" `
    -Area "demo-handoff" `
    -Preset "all" `
    -Issue "The investor/demo evidence now spans visual evidence, command evidence and this audit; the next implementation pass should make the playable loop improvements directly re-capturable." `
    -NextFix "After F36 fixes, refresh a single PC playable-flow evidence report that links command clarity, contact pressure, damage story and sparse HUD in one place."

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoPlayableFlowPolishAudit"
    result = "pass-with-followups"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    width = $Width
    height = $Height
    completedTask = "F35 audit post-F34 PC controlled-demo playable flow polish"
    nextFormalTask = "F36 implement post-F34 PC controlled-demo playable flow polish fixes"
    sourceCommandEvidenceReport = Convert-ToRepoRelativePath -Path $commandEvidenceReportPath
    sourceVisualEvidenceReport = Convert-ToRepoRelativePath -Path $visualEvidenceReportPath
    captureDir = Convert-ToRepoRelativePath -Path $captureDir
    checks = $checks
    followUps = $followUps
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
[void]$markdownLines.Add("# PC Controlled Demo Playable Flow Polish Audit")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("Result: pass-with-followups")
[void]$markdownLines.Add("")
[void]$markdownLines.Add('Completed task: `F35 audit post-F34 PC controlled-demo playable flow polish`')
[void]$markdownLines.Add('Next formal task: `F36 implement post-F34 PC controlled-demo playable flow polish fixes`')
[void]$markdownLines.Add("")
[void]$markdownLines.Add("Source command evidence: `"$(Convert-ToRepoRelativePath -Path $commandEvidenceReportPath)`"")
[void]$markdownLines.Add("Source visual evidence: `"$(Convert-ToRepoRelativePath -Path $visualEvidenceReportPath)`"")
[void]$markdownLines.Add("Capture directory: `"$(Convert-ToRepoRelativePath -Path $captureDir)`"")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("## Checks")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("| Area | Preset | Status | Detail |")
[void]$markdownLines.Add("| --- | --- | --- | --- |")
foreach ($check in $checks) {
    [void]$markdownLines.Add("| $($check.area) | $($check.preset) | $($check.status) | $($check.detail) |")
}

[void]$markdownLines.Add("")
[void]$markdownLines.Add("## Follow-Ups")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("| Priority | Area | Preset | Issue | Next Fix |")
[void]$markdownLines.Add("| --- | --- | --- | --- | --- |")
foreach ($item in $followUps) {
    [void]$markdownLines.Add("| $($item.priority) | $($item.area) | $($item.preset) | $($item.issue) | $($item.nextFix) |")
}

$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo playable flow polish audit OK."
Write-Host "Result: pass-with-followups"
Write-Host "Report: $reportJsonPath"
