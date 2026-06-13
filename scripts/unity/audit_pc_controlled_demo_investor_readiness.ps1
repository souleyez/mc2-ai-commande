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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-readiness-audit"
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
$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-investor-readiness-audit.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-investor-readiness-audit.md"
$commandEvidenceScript = Join-Path $RepoRoot "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$requiredPresets = @("spawn", "hangar-contact", "damage-demo", "solo-order", "solo-return")
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
        throw "$Label missing marker '$Needle': $Text"
    }
}

function Read-PropertyValue {
    param(
        [object]$Object,
        [string]$Name,
        [string]$Label
    )

    if ($null -eq $Object) {
        throw "$Label missing object for property: $Name"
    }

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property -or $null -eq $property.Value) {
        throw "$Label missing property: $Name"
    }

    return $property.Value
}

function Read-StringProperty {
    param(
        [object]$Object,
        [string]$Name,
        [string]$Label
    )

    return [string](Read-PropertyValue -Object $Object -Name $Name -Label $Label)
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

function Test-SourceAndDocBoundary {
    $bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
    foreach ($marker in @(
        'CapturePresetSoloReturn = "solo-return"',
        "PlayableFlowPolish=contact-pressure+damage-debrief+solo-return+hud-density+handoff",
        "mobileLandscapeOnly=True",
        "statusRailShare1280=",
        "Solo return settled: unit-2 back in squad"
    )) {
        Require-Text -Text $bootstrap -Needle $marker -Label "Mc2DemoBootstrap.cs"
    }

    $investorEvidence = Read-RequiredText -RelativePath "docs-playable-demo-investor-evidence-2026-06-07.md"
    foreach ($marker in @(
        "coherent investor/demo evidence slice",
        "prototype art",
        "Public or commercial builds must use project-owned or properly licensed replacement content packs"
    )) {
        Require-Text -Text $investorEvidence -Needle $marker -Label "investor evidence public boundary"
    }

    Add-Check -Area "source-and-public-boundary" -Preset "all" -Status "pass" -Detail "F38 audits demo evidence while preserving the prototype-art and replaceable-content boundary"
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
        throw "$Preset missing investor-demo battle context: player=$($Sidecar.playerUnitCount) objectives=$($Sidecar.currentObjectiveCount) structures=$($Sidecar.targetableStructureCount)"
    }

    $firstMapVisual = Read-StringProperty -Object $Sidecar -Name "firstMapVisual" -Label "$Preset sidecar"
    foreach ($marker in @(
        "FirstMapVisual=terrain+unit+structure+sparse-ui+occupancy+contact",
        "flow=Battle",
        "status=ready",
        "image=external-script-gated",
        "png=nonblank+unique-colors",
        "privateReference=replaceable",
        "visualOnly=yes",
        "pathing=unchanged",
        "collision=unchanged"
    )) {
        Require-Text -Text $firstMapVisual -Needle $marker -Label "$Preset firstMapVisual"
    }

    $terrain = Read-StringProperty -Object $Sidecar -Name "terrainReadability" -Label "$Preset sidecar"
    Require-Text -Text $terrain -Needle "TerrainReadability=samples 10000" -Label "$Preset terrainReadability"
    Require-Text -Text $terrain -Needle "waterSurface=readable-overlay" -Label "$Preset terrainReadability"
    $textureStrength = Read-SummaryNumber -Summary $terrain -Pattern "textureStrength=([0-9.]+)" -Label "$Preset terrainReadability"
    if ($textureStrength -lt 0.30) {
        throw "$Preset terrain texture strength too low for investor readback: $textureStrength"
    }

    $unit = Read-StringProperty -Object $Sidecar -Name "unitReadability" -Label "$Preset sidecar"
    foreach ($marker in @(
        "UnitReadability=contact-shadow+faction-footprint-ring",
        "sectionDamage=overlays",
        "labels=no",
        "pathing=unchanged",
        "collision=unchanged"
    )) {
        Require-Text -Text $unit -Needle $marker -Label "$Preset unitReadability"
    }

    $structure = Read-StringProperty -Object $Sidecar -Name "structureReadability" -Label "$Preset sidecar"
    foreach ($marker in @(
        "StructureReadability=base-shadow+target-footprint",
        "ObjectiveNearOcclusion=fade+height-clamp+tone-down",
        "ReferenceStructures=loaded",
        "ReferenceProps=loaded",
        "visualOnly=yes",
        "collision=unchanged"
    )) {
        Require-Text -Text $structure -Needle $marker -Label "$Preset structureReadability"
    }

    $occupancyPlaceholders = Read-StringProperty -Object $Sidecar -Name "occupancyPlaceholders" -Label "$Preset sidecar"
    foreach ($marker in @(
        "OccupancyPlaceholders=enabled",
        "source=BattleMission.OccupancyPlaceholderRegions+DemoTerrainView.LandingReviewBlockedMarkers"
    )) {
        Require-Text -Text $occupancyPlaceholders -Needle $marker -Label "$Preset occupancyPlaceholders"
    }

    $contactClearance = Read-StringProperty -Object $Sidecar -Name "contactClearance" -Label "$Preset sidecar"
    Require-Text -Text $contactClearance -Needle "overlaps=0" -Label "$Preset contactClearance"
    Require-Text -Text $contactClearance -Needle "status=separated" -Label "$Preset contactClearance"

    $battleHud = Read-StringProperty -Object $Sidecar -Name "battleHud" -Label "$Preset sidecar"
    foreach ($marker in @(
        "BattleHud=active controls=statusRows+jet+map+bay+system",
        "combatLogVisible=no",
        "objectivePanel=compactObjective",
        "missionMap=closed",
        "saveUi=disabled",
        "SparseBattleUi=statusRows+sections+solo",
        "accountUi=hidden",
        "economyUi=funds-only",
        "debugOccupancy=sidecar-only",
        "overlays=hidden"
    )) {
        Require-Text -Text $battleHud -Needle $marker -Label "$Preset battleHud"
    }

    $statusRailWidth = Read-SummaryNumber -Summary $battleHud -Pattern "statusRailW=([0-9.]+)" -Label "$Preset battleHud"
    $statusRailShare = Read-SummaryNumber -Summary $battleHud -Pattern "statusRailShare1280=([0-9.]+)" -Label "$Preset battleHud"
    if ($statusRailWidth -gt 300 -or $statusRailShare -gt 0.24) {
        throw "$Preset battle HUD is too wide for investor demo readability: width=$statusRailWidth share=$statusRailShare"
    }

    $mobileTouch = Read-StringProperty -Object $Sidecar -Name "mobileTouch" -Label "$Preset sidecar"
    foreach ($marker in @(
        "MobileTouchUi=ready",
        "orientation=landscape",
        "landscapeOnly=yes",
        "noDragBox=yes",
        "combatLog=hidden",
        "status=ready"
    )) {
        Require-Text -Text $mobileTouch -Needle $marker -Label "$Preset mobileTouch"
    }

    $command = Read-StringProperty -Object $Sidecar -Name "commandReadability" -Label "$Preset sidecar"
    foreach ($marker in @(
        "CommandReadability=all+single+jet+focus+commander-follow+formation",
        "default=all-squad",
        "noDragBox=yes",
        "maxSquad=6",
        "formation=move-320+attack-340",
        "statusRows=select-unit+detached-border",
        "CommandCuePalette=command-blue+target-red+damage-amber+hostile-magenta",
        "CommanderFollow=unit-1+first-sort+fixed-view"
    )) {
        Require-Text -Text $command -Needle $marker -Label "$Preset commandReadability"
    }

    $playableFlow = Read-StringProperty -Object $Sidecar -Name "playableFlowPolish" -Label "$Preset sidecar"
    foreach ($marker in @(
        "PlayableFlowPolish=contact-pressure+damage-debrief+solo-return+hud-density+handoff",
        "damageStatus=status-rail-section-bars",
        "statusRail=denser-context",
        "battleHud=sparse",
        "evidence=unified-playable-flow",
        "mobileLandscapeOnly=True",
        "orientation=landscape"
    )) {
        Require-Text -Text $playableFlow -Needle $marker -Label "$Preset playableFlowPolish"
    }

    $referenceAssets = Read-PropertyValue -Object $Sidecar -Name "referenceAssets" -Label "$Preset sidecar"
    $referenceTerrain = Read-StringProperty -Object $referenceAssets -Name "terrain" -Label "$Preset referenceAssets"
    $referenceScale = Read-StringProperty -Object $referenceAssets -Name "scale" -Label "$Preset referenceAssets"
    Require-Text -Text $referenceTerrain -Needle "loadedSamples=10000" -Label "$Preset referenceAssets.terrain"
    Require-Text -Text $referenceTerrain -Needle "missingSamples=0" -Label "$Preset referenceAssets.terrain"
    Require-Text -Text $referenceScale -Needle "ReferenceUnits=mech" -Label "$Preset referenceAssets.scale"
    Require-Text -Text $referenceScale -Needle "ReferencePropScale=structure" -Label "$Preset referenceAssets.scale"

    Add-Check -Area "common-investor-readiness" -Preset $Preset -Status "pass" -Detail "visuals, sparse HUD, command model, collision, landscape mobile contract and replaceable content boundary are present"
}

function Test-SpecificSidecar {
    param(
        [string]$Preset,
        [object]$Sidecar
    )

    $command = Read-StringProperty -Object $Sidecar -Name "commandReadability" -Label "$Preset sidecar"
    $playableFlow = Read-StringProperty -Object $Sidecar -Name "playableFlowPolish" -Label "$Preset sidecar"
    $status = Read-StringProperty -Object $Sidecar -Name "status" -Label "$Preset sidecar"

    if ($Preset -eq "spawn") {
        if ([int]$Sidecar.activeHostileCount -ne 0 -or [int]$Sidecar.visibleHostileCount -ne 0) {
            throw "spawn should be a quiet first read: active=$($Sidecar.activeHostileCount) visible=$($Sidecar.visibleHostileCount)"
        }

        Require-Text -Text $status -Needle "Loaded mc2_01" -Label "spawn status"
        Add-Check -Area "demo-opening" -Preset $Preset -Status "pass" -Detail "quiet map/opening state is readable before contact"
    }

    if ($Preset -eq "hangar-contact") {
        if ([int]$Sidecar.activeHostileCount -lt 16 -or [int]$Sidecar.visibleHostileCount -lt 12) {
            throw "hangar-contact should prove visible pressure: active=$($Sidecar.activeHostileCount) visible=$($Sidecar.visibleHostileCount)"
        }

        Require-Text -Text $status -Needle "under attack" -Label "hangar-contact status"
        Require-Text -Text $playableFlow -Needle "contactPressure=ContactPressureCue=objective-panel+in-world" -Label "hangar-contact playableFlowPolish"
        Require-Text -Text $playableFlow -Needle "tempo=fire" -Label "hangar-contact playableFlowPolish"
        Add-Check -Area "contact-pressure" -Preset $Preset -Status "pass" -Detail "hostile pressure is visible without reintroducing a combat log"
    }

    if ($Preset -eq "damage-demo") {
        if ([int]$Sidecar.activeHostileCount -lt 16 -or [int]$Sidecar.visibleHostileCount -lt 12) {
            throw "damage-demo should retain pressure while showing consequences: active=$($Sidecar.activeHostileCount) visible=$($Sidecar.visibleHostileCount)"
        }

        $damageStory = Read-StringProperty -Object $Sidecar -Name "damageStory" -Label "damage-demo sidecar"
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

        $damageReadability = Read-StringProperty -Object $Sidecar -Name "damageReadability" -Label "damage-demo sidecar"
        foreach ($marker in @(
            "arms-firepower",
            "legs-mobility",
            "cockpit-ejection",
            "wreck-salvage",
            "SectionStatus=bar+short-label+critical+destroyed"
        )) {
            Require-Text -Text $damageReadability -Needle $marker -Label "damage-demo damageReadability"
        }

        $debriefReward = Read-StringProperty -Object $Sidecar -Name "debriefReward" -Label "damage-demo sidecar"
        foreach ($marker in @(
            "DamageDebrief=section-status+repair-line",
            "Repair -9,288",
            "cockpitEjection=ready",
            "MobileLandscapeOnly: True"
        )) {
            Require-Text -Text $debriefReward -Needle $marker -Label "damage-demo debriefReward"
        }

        Require-Text -Text $playableFlow -Needle "damageStory=section-loss+ejection+wreck" -Label "damage-demo playableFlowPolish"
        Require-Text -Text $playableFlow -Needle "debrief=repair-line" -Label "damage-demo playableFlowPolish"
        Add-Check -Area "damage-and-repair-story" -Preset $Preset -Status "pass" -Detail "arm, leg, cockpit, ejection, wreck and repair cost are present under pressure"
    }

    if ($Preset -eq "solo-order") {
        if ([int]$Sidecar.activeHostileCount -ne 0 -or [int]$Sidecar.visibleHostileCount -ne 0) {
            throw "solo-order should isolate single-unit command readability: active=$($Sidecar.activeHostileCount) visible=$($Sidecar.visibleHostileCount)"
        }

        Require-Text -Text $command -Needle "solo=1" -Label "solo-order commandReadability"
        Require-Text -Text $playableFlow -Needle "soloReturn=order-active" -Label "solo-order playableFlowPolish"
        Require-Text -Text $playableFlow -Needle "detached=1" -Label "solo-order playableFlowPolish"
        Require-Text -Text $status -Needle "move accepted" -Label "solo-order status"
        Add-Check -Area "solo-order" -Preset $Preset -Status "pass" -Detail "single-unit detached order is visible and still no-drag-box"
    }

    if ($Preset -eq "solo-return") {
        if ([int]$Sidecar.activeHostileCount -ne 0 -or [int]$Sidecar.visibleHostileCount -ne 0) {
            throw "solo-return should isolate return-to-squad readability: active=$($Sidecar.activeHostileCount) visible=$($Sidecar.visibleHostileCount)"
        }

        Require-Text -Text $command -Needle "solo=0" -Label "solo-return commandReadability"
        Require-Text -Text $playableFlow -Needle "soloReturn=returned" -Label "solo-return playableFlowPolish"
        Require-Text -Text $playableFlow -Needle "detached=0" -Label "solo-return playableFlowPolish"
        Require-Text -Text $status -Needle "Solo return settled" -Label "solo-return status"
        Add-Check -Area "solo-return" -Preset $Preset -Status "pass" -Detail "detached command resolves back into squad state"
    }
}

Require-File -Path $commandEvidenceScript -Label "F37 command evidence script"

if ($PlanOnly) {
    Write-Host "PC controlled-demo investor readiness audit plan OK."
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
        throw "F37 command evidence refresh failed before F38 investor readiness audit."
    }
}

Require-File -Path $commandEvidenceReportPath -Label "F37 command evidence report"
Require-File -Path $visualEvidenceReportPath -Label "source visual evidence report"

$commandReport = Get-Content -LiteralPath $commandEvidenceReportPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string]$commandReport.result -ne "pass") {
    throw "F37 command evidence report must pass before F38 audit: $commandEvidenceReportPath"
}

if ([string]$commandReport.completedTask -ne "F37 refresh PC controlled-demo playable-flow evidence after polish fixes") {
    throw "F37 command evidence completedTask mismatch: $($commandReport.completedTask)"
}

if ([string]$commandReport.nextFormalTask -ne "F38 audit post-F37 PC controlled-demo investor readiness") {
    throw "F37 command evidence nextFormalTask mismatch: $($commandReport.nextFormalTask)"
}

$visualReport = Get-Content -LiteralPath $visualEvidenceReportPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string]$visualReport.result -ne "pass") {
    throw "Source visual evidence report must pass before F38 audit: $visualEvidenceReportPath"
}

Test-SourceAndDocBoundary

$evidencePresets = @($commandReport.evidence | ForEach-Object { [string]$_.preset })
foreach ($preset in $requiredPresets) {
    if ($evidencePresets -notcontains $preset) {
        throw "F37 command evidence report missing preset: $preset"
    }

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
Add-Check -Area "f37-evidence-package" -Preset "all" -Status "pass" -Detail "F37 command evidence covers spawn, hangar-contact, damage-demo, solo-order and solo-return"

$damageEvidence = @($commandReport.evidence | Where-Object { [string]$_.preset -eq "damage-demo" } | Select-Object -First 1)
if ($damageEvidence.Count -eq 0 -or [string]::IsNullOrWhiteSpace([string]$damageEvidence[0].debriefRewardSummary)) {
    Add-FollowUp `
        -Priority "P1" `
        -Area "investor-report-summary" `
        -Preset "damage-demo" `
        -Issue "The sidecar proves the repair/debrief line, but the F37 command evidence table leaves debriefRewardSummary empty." `
        -NextFix "Expose the sidecar debriefReward line in the command evidence report so an investor reviewer sees payout, repair and cockpit-ejection consequence without opening raw JSON."
}

$damageSidecar = Get-Content -LiteralPath (Join-Path $captureDir "damage-demo.json") -Raw -Encoding UTF8 | ConvertFrom-Json
$damageReferenceAssets = Read-PropertyValue -Object $damageSidecar -Name "referenceAssets" -Label "damage-demo sidecar"
$damageProps = Read-StringProperty -Object $damageReferenceAssets -Name "props" -Label "damage-demo referenceAssets"
$damageScale = Read-StringProperty -Object $damageReferenceAssets -Name "scale" -Label "damage-demo referenceAssets"
$propFallback = Read-SummaryNumber -Summary $damageProps -Pattern "ReferenceProps=loaded [0-9]+ fallback ([0-9]+)" -Label "damage-demo referenceAssets.props"
$infantryFallback = Read-SummaryNumber -Summary $damageScale -Pattern "infantry [0-9]+/([0-9]+)" -Label "damage-demo referenceAssets.scale"
if ($propFallback -gt 0 -or $infantryFallback -gt 0) {
    Add-FollowUp `
        -Priority "P1" `
        -Area "visual-fidelity" `
        -Preset "all" `
        -Issue "The demo is readable, but the first-map reference scale still reports fallback props or infantry placeholders, which keeps the scene from feeling like final 3D battlefield art." `
        -NextFix "Replace the remaining high-visibility fallback silhouettes with project-owned or licensed proxy meshes/materials while keeping private reference assets ignored and swappable."
}

Add-FollowUp `
    -Priority "P2" `
    -Area "investor-demo-route" `
    -Preset "all" `
    -Issue "The evidence is machine-verifiable, but the investor-facing three-minute route still depends on existing docs instead of a concise F37/F38 report summary." `
    -NextFix "Generate a one-page demo route that references the five F37 presets, the strongest talking points and the public-art caveat."

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorReadinessAudit"
    result = "pass-with-followups"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    width = $Width
    height = $Height
    completedTask = "F38 audit post-F37 PC controlled-demo investor readiness"
    nextFormalTask = "F39 implement post-F37 PC controlled-demo investor readiness fixes"
    sourceCommandEvidenceReport = Convert-ToRepoRelativePath -Path $commandEvidenceReportPath
    sourceVisualEvidenceReport = Convert-ToRepoRelativePath -Path $visualEvidenceReportPath
    captureDir = Convert-ToRepoRelativePath -Path $captureDir
    checks = $checks
    followUps = $followUps
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
[void]$markdownLines.Add("# PC Controlled Demo Investor Readiness Audit")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("Result: pass-with-followups")
[void]$markdownLines.Add("")
[void]$markdownLines.Add('Completed task: `F38 audit post-F37 PC controlled-demo investor readiness`')
[void]$markdownLines.Add('Next formal task: `F39 implement post-F37 PC controlled-demo investor readiness fixes`')
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

Write-Host "PC controlled-demo investor readiness audit OK."
Write-Host "Result: pass-with-followups"
Write-Host "Report: $reportJsonPath"
