param(
    [string]$RepoRoot = "",
    [string]$EvidenceDir = "",
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

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-command-readability-audit"
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
$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-command-readability-audit.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-command-readability-audit.md"
$visualEvidenceScript = Join-Path $RepoRoot "scripts\unity\capture_pc_controlled_demo_visual_evidence.ps1"
$requiredPresets = @("spawn", "hangar-contact", "damage-demo")
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

function Read-OptionalSummaryNumber {
    param(
        [string]$Summary,
        [string]$Pattern
    )

    $match = [regex]::Match($Summary, $Pattern)
    if (-not $match.Success) {
        return $null
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
    $commandPort = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\CommanderCommandPort.cs"
    foreach ($marker in @(
        "MoveSquad(Vector2 missionPoint)",
        "MoveUnit(string unitId, Vector2 missionPoint)",
        "JumpSquad(Vector2 missionPoint)",
        "JumpUnit(string unitId, Vector2 missionPoint)",
        "AttackUnit(string unitId, string targetUnitId)",
        "AttackStructure(string unitId, string structureId)",
        "mission.IssueDetachedMove(command.UnitId, command.MissionPoint)",
        "mission.IssueSquadMove(command.MissionPoint)",
        "mission.IssueDetachedJump(command.UnitId, command.MissionPoint, jumpDistance, isLandingAllowed)",
        "mission.IssueSquadJump(command.MissionPoint, jumpDistance, isLandingAllowed)",
        "mission.IssueDetachedAttackUnit(command.UnitId, command.TargetId)",
        "mission.IssueSquadAttackUnit(command.TargetId)",
        "mission.IssueDetachedAttackStructure(command.UnitId, command.TargetId)",
        "mission.IssueSquadAttackStructure(command.TargetId)"
    )) {
        Require-Text -Text $commandPort -Needle $marker -Label "CommanderCommandPort.cs"
    }
    Add-Check -Area "source-command-port" -Preset "all" -Status "pass" -Detail "squad/unit move, jump, unit attack and structure attack ports present"

    $startupScript = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\StartupCommanderScript.cs"
    foreach ($marker in @(
        'string.Equals(verb, "command"',
        'string.Equals(verb, "status-row"',
        'string.Equals(verb, "battle-click"',
        'string.Equals(verb, "battle-target"',
        "AssertCommandResult"
    )) {
        Require-Text -Text $startupScript -Needle $marker -Label "StartupCommanderScript.cs"
    }
    Add-Check -Area "source-command-file" -Preset "all" -Status "pass" -Detail "command-file smoke can exercise commands, row select, map clicks and targets"

    $presentation = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
    foreach ($marker in @(
        'GUI.Button(new Rect(x, y, commandButtonWidth, commandButtonHeight), "All")',
        'pendingDetachedUnitId = null;',
        'pendingJumpOrder = true;',
        '"Squad move"',
        '"Detached order: "',
        '"Squad focus: "',
        '"Focus " + target.UnitType + " with "',
        '"Squad attack: "',
        '"Attack " + target.ObjectType + " with "',
        '"Squad jet: "',
        '"Jet order: "',
        'SpawnCommandResultCue(CommanderCommand.SquadMove(target), result)',
        'SpawnCommandResultCue(CommanderCommand.UnitMove(detachedUnitId, target), result)',
        'SpawnCommandResultCue(CommanderCommand.AttackUnit(null, target.Id), result)',
        'SpawnCommandResultCue(CommanderCommand.AttackUnit(detachedUnitId, target.Id), result)',
        'SpawnCommandResultCue(CommanderCommand.UnitJump(detachedUnitId, target), result)',
        'SpawnCommandResultCue(command, result)',
        '"  DETACHED"',
        '"Solo order"',
        'DrawRectBorder(rowRect, rowCue, unit.IsDetached ? 2f : 1f)',
        'FollowCommander()',
        'FirstPlayerUnit()',
        'cameraFollowWorldOffset',
        'CameraObjectiveCompositionOffset(commander)'
    )) {
        Require-Text -Text $presentation -Needle $marker -Label "Mc2DemoBootstrap.cs"
    }
    Add-Check -Area "source-presentation" -Preset "all" -Status "pass" -Detail "all/single/jet/focus UI path and commander camera follow markers present"
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

    $battleHud = [string]$Sidecar.battleHud
    foreach ($marker in @(
        "BattleHud=active controls=statusRows+jet+map+bay+system",
        "SparseBattleUi=statusRows+sections+solo",
        "controls=all+jet+map+bay+system",
        "combatLogVisible=no",
        "combatLog=hidden",
        "objective=compactObjective",
        "missionMap=available-closed",
        "saveUi=disabled",
        "debugOccupancy=sidecar-only",
        "overlays=hidden"
    )) {
        Require-Text -Text $battleHud -Needle $marker -Label "$Preset battleHud"
    }

    $statusRailWidth = Read-SummaryNumber -Summary $battleHud -Pattern "statusRailW=([0-9.]+)" -Label "$Preset battleHud"
    $statusRailShare = Read-SummaryNumber -Summary $battleHud -Pattern "statusRailShare1280=([0-9.]+)" -Label "$Preset battleHud"
    if ($statusRailWidth -gt 300 -or $statusRailShare -gt 0.24) {
        throw "$Preset status rail too wide for command readability: width=$statusRailWidth share=$statusRailShare"
    }
    Add-Check -Area "hud-command-readability" -Preset $Preset -Status "pass" -Detail "status rail=$statusRailWidth share=$statusRailShare controls all+jet+map+bay+system"

    $mobileTouch = [string]$Sidecar.mobileTouch
    foreach ($marker in @(
        "MobileTouchUi=ready",
        "orientation=landscape",
        "commandTargets=44",
        "statusRows=44",
        "primaryButtons=44",
        "mapBack=44",
        "systemButtons=44",
        "landscapeOnly=yes",
        "noDragBox=yes",
        "combatLog=hidden",
        "status=ready"
    )) {
        Require-Text -Text $mobileTouch -Needle $marker -Label "$Preset mobileTouch"
    }
    Add-Check -Area "landscape-command-model" -Preset $Preset -Status "pass" -Detail "landscape-only no-drag-box command targets >=44"

    $clearance = [string]$Sidecar.contactClearance
    Require-Text -Text $clearance -Needle "ContactClearance=" -Label "$Preset contactClearance"
    Require-Text -Text $clearance -Needle "status=separated" -Label "$Preset contactClearance"
    $overlaps = Read-SummaryNumber -Summary $clearance -Pattern "overlaps=([0-9]+)" -Label "$Preset contactClearance"
    $worstClearance = Read-SummaryNumber -Summary $clearance -Pattern "worstClearance=([0-9.]+)" -Label "$Preset contactClearance"
    if ($overlaps -ne 0) {
        throw "$Preset command/formation evidence has physical overlaps: $clearance"
    }

    $spread = [string]$Sidecar.contactSpread
    Require-Text -Text $spread -Needle "ContactSpread=" -Label "$Preset contactSpread"
    $playerSpan = Read-SummaryNumber -Summary $spread -Pattern "playerSpan=([0-9.]+)" -Label "$Preset contactSpread"
    $nearestPp = Read-OptionalSummaryNumber -Summary $spread -Pattern "nearestPP=([0-9.]+)"
    $nearestPh = Read-OptionalSummaryNumber -Summary $spread -Pattern "nearestPH=([0-9.]+)"
    Add-Check -Area "formation-clearance" -Preset $Preset -Status "pass" -Detail "overlaps=0 worstClearance=$worstClearance nearestPP=$nearestPp playerSpan=$playerSpan nearestPH=$nearestPh"

    if ($null -ne $nearestPp -and $nearestPp -le 130) {
        Add-FollowUp `
            -Priority "P1" `
            -Area "formation-feel" `
            -Preset $Preset `
            -Issue "Player mechs are physically separated but nearestPP=$nearestPp is just touching current radii; it can still look stacked." `
            -NextFix "Widen default formation slots or add clearer slot/arrival spacing for PC and landscape-phone evidence."
    }

    if ($Preset -ne "spawn" -and $null -ne $nearestPh -and $nearestPh -lt 160) {
        Add-FollowUp `
            -Priority "P1" `
            -Area "contact-pressure" `
            -Preset $Preset `
            -Issue "Nearest player-hostile distance is $nearestPh under contact; command target and damage cues can visually crowd the same area." `
            -NextFix "Separate command target pulse from hostile pressure/damage rings and record the cue palette in sidecar."
    }

    $firstMapVisual = [string]$Sidecar.firstMapVisual
    foreach ($marker in @(
        "contact=separated",
        "sparseUi=statusRows+compactObjective",
        "pathing=unchanged",
        "collision=unchanged"
    )) {
        Require-Text -Text $firstMapVisual -Needle $marker -Label "$Preset firstMapVisual"
    }

    if ($null -eq $Sidecar.camera -or $null -eq $Sidecar.camera.followOffset -or $null -eq $Sidecar.camera.compositionOffset) {
        throw "$Preset camera follow evidence is missing followOffset/compositionOffset"
    }

    $pitch = [double]$Sidecar.camera.pitch
    $zoomScale = [double]$Sidecar.camera.zoomScale
    $ortho = [double]$Sidecar.camera.orthographicSize
    if ($pitch -lt 54 -or $pitch -gt 72 -or $zoomScale -lt 0.72 -or $zoomScale -gt 1.25 -or $ortho -lt 18 -or $ortho -gt 34) {
        throw "$Preset camera outside controlled fixed-view range: pitch=$pitch zoom=$zoomScale ortho=$ortho"
    }
    Add-Check -Area "commander-camera" -Preset $Preset -Status "pass" -Detail "fixed pitch=$pitch zoom=$zoomScale ortho=$ortho followOffset present"

    if ($Preset -eq "spawn") {
        if ([int]$Sidecar.playerUnitCount -lt 3 -or [int]$Sidecar.activeHostileCount -ne 0) {
            throw "spawn expected 3 player units and no active hostiles"
        }

        Require-Text -Text ([string]$Sidecar.status) -Needle "Loaded" -Label "spawn status"
        Add-Check -Area "default-squad-command" -Preset $Preset -Status "pass" -Detail "spawn begins with full squad, no hostiles, objective visible"
    }
    elseif ($Preset -eq "hangar-contact") {
        if ([int]$Sidecar.activeHostileCount -lt 20 -or [int]$Sidecar.visibleHostileCount -lt 16 -or [int]$Sidecar.targetableStructureCount -lt 1) {
            throw "hangar-contact expected active hostiles, visible targets, and a targetable structure"
        }

        Require-Text -Text ([string]$Sidecar.status) -Needle "under attack" -Label "hangar-contact status"
        Require-Text -Text (($Sidecar.currentObjectives | Out-String)) -Needle "Destroy Hangar" -Label "hangar-contact objective"
        Add-Check -Area "focus-target-readability" -Preset $Preset -Status "pass" -Detail "hostiles=$($Sidecar.visibleHostileCount)/$($Sidecar.activeHostileCount) targetableStructures=$($Sidecar.targetableStructureCount)"
    }
    elseif ($Preset -eq "damage-demo") {
        $damageStory = [string]$Sidecar.damageStory
        foreach ($marker in @(
            "lostSections=3",
            "arms=1",
            "legs=1",
            "cockpit=1",
            "pilotRisk=1",
            "destroyedUnits=1"
        )) {
            Require-Text -Text $damageStory -Needle $marker -Label "damage-demo damageStory"
        }

        Add-Check -Area "damage-command-readability" -Preset $Preset -Status "pass" -Detail "damage story exposes arm, legs, cockpit and destroyed-unit state"
    }
}

Require-File -Path $visualEvidenceScript -Label "F31 visual evidence script"

if ($PlanOnly) {
    Write-Host "PC controlled-demo command readability formation audit plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "EvidenceDir: $EvidenceDir"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "ReportJson: $reportJsonPath"
    Write-Host "ReportMarkdown: $reportMarkdownPath"
    Write-Host "RequiredPresets: $($requiredPresets -join ',')"
    Write-Host "WidthHeight: ${Width}x${Height}"
    return
}

if ($RefreshEvidence) {
    if ($SkipEvidenceRun) {
        & $visualEvidenceScript -RepoRoot $RepoRoot -OutputDir $EvidenceDir -Width $Width -Height $Height -SkipRun
    }
    else {
        & $visualEvidenceScript -RepoRoot $RepoRoot -OutputDir $EvidenceDir -Width $Width -Height $Height
    }
}

Require-File -Path $visualEvidenceReportPath -Label "F31 visual evidence report"
$visualReport = Get-Content -LiteralPath $visualEvidenceReportPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string]$visualReport.result -ne "pass") {
    throw "F31 visual evidence report must pass before F32 audit: $visualEvidenceReportPath"
}

if ([string]$visualReport.completedTask -ne "F31 refresh PC controlled-demo visual evidence after readability fixes") {
    throw "F31 visual evidence completedTask mismatch: $($visualReport.completedTask)"
}

if ([string]$visualReport.nextFormalTask -ne "F32 audit PC controlled-demo command readability and formation feel") {
    throw "F31 visual evidence nextFormalTask mismatch: $($visualReport.nextFormalTask)"
}

Test-SourceBoundary

foreach ($preset in $requiredPresets) {
    $pngPath = Join-Path $captureDir "$preset.png"
    $jsonPath = Join-Path $captureDir "$preset.json"
    $logPath = Join-Path $captureDir "$preset.log"
    Require-File -Path $pngPath -Label "$preset screenshot"
    Require-File -Path $jsonPath -Label "$preset sidecar"
    Require-File -Path $logPath -Label "$preset capture log"
    $sidecar = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([string]$sidecar.preset -ne $preset) {
        throw "$preset sidecar preset mismatch: $($sidecar.preset)"
    }

    Test-Sidecar -Preset $preset -Sidecar $sidecar
}

Add-FollowUp `
    -Priority "P1" `
    -Area "command-sidecar" `
    -Preset "all" `
    -Issue "The current sidecar proves sparse HUD and command model markers, but it does not yet emit a dedicated CommandReadability summary." `
    -NextFix "Add CommandReadability=all+single+jet+focus+commander-follow+formation markers to capture sidecars."

Add-FollowUp `
    -Priority "P2" `
    -Area "solo-order-clarity" `
    -Preset "all" `
    -Issue "Source has Solo order text, DETACHED row labels, row border and solo beacons, but F31 battle presets do not include a live solo-order screenshot state." `
    -NextFix "Capture a PC preset with one detached unit active, then verify solo count, solo beacon and automatic return cue."

Add-FollowUp `
    -Priority "P2" `
    -Area "commander-follow-evidence" `
    -Preset "all" `
    -Issue "Camera followOffset is present and source follows the first player unit, but the sidecar does not name commander unit id or sorted squad index." `
    -NextFix "Emit CommanderFollow=unit-1+first-sort+fixed-view in sidecar and audit it from PC and landscape-phone captures."

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoCommandReadabilityFormationAudit"
    result = "pass-with-followups"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    width = $Width
    height = $Height
    completedTask = "F32 audit PC controlled-demo command readability and formation feel"
    nextFormalTask = "F33 implement PC controlled-demo command readability and formation fixes"
    evidenceDir = Convert-ToRepoRelativePath -Path $EvidenceDir
    captureDir = Convert-ToRepoRelativePath -Path $captureDir
    sourceVisualEvidenceReport = Convert-ToRepoRelativePath -Path $visualEvidenceReportPath
    checks = $checks
    followUps = $followUps
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
[void]$markdownLines.Add("# PC Controlled Demo Command Readability And Formation Audit")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("Result: pass-with-followups")
[void]$markdownLines.Add("")
[void]$markdownLines.Add('Completed task: `F32 audit PC controlled-demo command readability and formation feel`')
[void]$markdownLines.Add('Next formal task: `F33 implement PC controlled-demo command readability and formation fixes`')
[void]$markdownLines.Add("")
[void]$markdownLines.Add("Evidence source: `"$(Convert-ToRepoRelativePath -Path $visualEvidenceReportPath)`"")
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

Write-Host "PC controlled-demo command readability formation audit OK."
Write-Host "Result: pass-with-followups"
Write-Host "Report: $reportJsonPath"
