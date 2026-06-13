param(
    [string]$RepoRoot = "",
    [string]$OutputDir = "",
    [string[]]$Presets = @("spawn", "hangar-contact", "damage-demo"),
    [int]$Width = 1280,
    [int]$Height = 720,
    [int]$CaptureTimeoutSeconds = 75,
    [switch]$SkipRun,
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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-visual-readability-fixes"
}

$captureDir = Join-Path $OutputDir "captures"
$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-visual-readability-fixes.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-visual-readability-fixes.md"
$captureScript = Join-Path $RepoRoot "scripts\unity\capture_reference_visuals.ps1"
$sanityScript = Join-Path $RepoRoot "scripts\unity\check_pc_visual_capture_sanity.ps1"
$requiredPresets = @("spawn", "hangar-contact", "damage-demo")

function Normalize-CapturePreset {
    param([string]$Preset)
    if ([string]::IsNullOrWhiteSpace($Preset)) {
        return "spawn"
    }

    return $Preset.Trim().ToLowerInvariant().Replace("_", "-")
}

function Expand-CapturePresets {
    param([string[]]$RawPresets)

    $expanded = New-Object System.Collections.Generic.List[string]
    foreach ($rawPreset in $RawPresets) {
        foreach ($part in ($rawPreset -split ",")) {
            $normalized = Normalize-CapturePreset $part
            if (-not [string]::IsNullOrWhiteSpace($normalized)) {
                $expanded.Add($normalized)
            }
        }
    }

    return $expanded.ToArray()
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

function Convert-ToRepoRelativePath {
    param([string]$Path)

    $fullPath = (Resolve-Path -LiteralPath $Path).Path
    if ($fullPath.StartsWith($RepoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $fullPath.Substring($RepoRoot.Length).TrimStart("\", "/")
    }

    return $fullPath
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

function Test-F30Sidecar {
    param(
        [string]$Preset,
        [object]$Sidecar
    )

    if ([string]$Sidecar.flowScreen -ne "Battle") {
        throw "$Preset expected Battle flow, got $($Sidecar.flowScreen)"
    }

    $terrain = [string]$Sidecar.terrainReadability
    Require-Text -Text $terrain -Needle "texture=composite" -Label "$Preset terrainReadability"
    Require-Text -Text $terrain -Needle "style=land-outline+runway-contrast+water-muted" -Label "$Preset terrainReadability"
    Require-Text -Text $terrain -Needle "shoreContrast=raised" -Label "$Preset terrainReadability"
    Require-Text -Text $terrain -Needle "pathing=unchanged" -Label "$Preset terrainReadability"
    $textureStrength = Read-SummaryNumber -Summary $terrain -Pattern "textureStrength=([0-9.]+)" -Label "$Preset terrainReadability"
    if ($textureStrength -lt 0.34) {
        throw "$Preset terrain textureStrength must be at least 0.34 after F30, got $textureStrength"
    }

    $structure = [string]$Sidecar.structureReadability
    Require-Text -Text $structure -Needle "ObjectiveNearOcclusion=fade+height-clamp+tone-down" -Label "$Preset structureReadability"
    Require-Text -Text $structure -Needle "visualOnly=yes" -Label "$Preset structureReadability"
    Require-Text -Text $structure -Needle "collision=unchanged" -Label "$Preset structureReadability"
    Require-Text -Text $structure -Needle "blockerGeometry=unchanged" -Label "$Preset structureReadability"
    $objectiveNearProps = Read-SummaryNumber -Summary $structure -Pattern "ObjectiveNearOcclusion=fade\+height-clamp\+tone-down props=([0-9]+)" -Label "$Preset objective-near props"
    $treeClamps = Read-SummaryNumber -Summary $structure -Pattern "treeClamp=([0-9]+)" -Label "$Preset objective-near treeClamp"
    if ($objectiveNearProps -lt 1 -or $treeClamps -lt 1) {
        throw "$Preset expected objective-near prop/tree readability fixes, got props=$objectiveNearProps treeClamp=$treeClamps"
    }

    $unit = [string]$Sidecar.unitReadability
    Require-Text -Text $unit -Needle "alpha=high-contact" -Label "$Preset unitReadability"
    Require-Text -Text $unit -Needle "style=grounded-silhouette+friend-foe-footprint+high-contact-rings" -Label "$Preset unitReadability"
    Require-Text -Text $unit -Needle "damagePalette=amber+pilot-cyan" -Label "$Preset unitReadability"
    Require-Text -Text $unit -Needle "labels=no" -Label "$Preset unitReadability"
    Require-Text -Text $unit -Needle "pathing=unchanged" -Label "$Preset unitReadability"
    Require-Text -Text $unit -Needle "collision=unchanged" -Label "$Preset unitReadability"

    $hud = [string]$Sidecar.battleHud
    Require-Text -Text $hud -Needle "BattleHud=active controls=statusRows+jet+map+bay+system" -Label "$Preset battleHud"
    Require-Text -Text $hud -Needle "statusRailDensity=compact-pc" -Label "$Preset battleHud"
    Require-Text -Text $hud -Needle "combatLogVisible=no" -Label "$Preset battleHud"
    Require-Text -Text $hud -Needle "SparseBattleUi=statusRows+sections+solo" -Label "$Preset battleHud"
    Require-Text -Text $hud -Needle "objective=compactObjective" -Label "$Preset battleHud"
    $statusRailWidth = Read-SummaryNumber -Summary $hud -Pattern "statusRailW=([0-9.]+)" -Label "$Preset battleHud"
    $statusRailShare = Read-SummaryNumber -Summary $hud -Pattern "statusRailShare1280=([0-9.]+)" -Label "$Preset battleHud"
    if ($statusRailWidth -gt 300 -or $statusRailShare -gt 0.24) {
        throw "$Preset status rail is still too wide: width=$statusRailWidth share=$statusRailShare"
    }

    if ($Preset -eq "damage-demo") {
        $damage = [string]$Sidecar.damageReadability
        Require-Text -Text $damage -Needle "cuePalette=command-blue target-red damage-amber hostile-magenta pilot-cyan" -Label "$Preset damageReadability"
        Require-Text -Text $damage -Needle "sectionConsequences arms-firepower legs-mobility cockpit-ejection wreck-salvage" -Label "$Preset damageReadability"
        Require-Text -Text $damage -Needle "Cockpit=breach+ejection-pod+chute+landing+arc+distress+escape-column+route" -Label "$Preset damageReadability"
    }

    return [pscustomobject]@{
        preset = $Preset
        flow = [string]$Sidecar.flowScreen
        textureStrength = $textureStrength
        objectiveNearProps = $objectiveNearProps
        treeClamps = $treeClamps
        statusRailWidth = $statusRailWidth
        statusRailShare1280 = $statusRailShare
        activeHostiles = [int]$Sidecar.activeHostileCount
        visibleHostiles = [int]$Sidecar.visibleHostileCount
        terrainReadability = $terrain
        unitReadability = $unit
        structureReadability = $structure
        battleHud = $hud
        damageReadability = [string]$Sidecar.damageReadability
    }
}

$expandedPresets = @(Expand-CapturePresets -RawPresets $Presets)
foreach ($requiredPreset in $requiredPresets) {
    if ($expandedPresets -notcontains $requiredPreset) {
        throw "F30 fixes check requires preset '$requiredPreset'. Presets: $($expandedPresets -join ',')"
    }
}

Require-File -Path $captureScript -Label "reference visual capture script"
Require-File -Path $sanityScript -Label "PC visual capture sanity script"

if ($PlanOnly) {
    Write-Host "PC controlled-demo visual readability fixes plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "CaptureDir: $captureDir"
    Write-Host "ReportJson: $reportJsonPath"
    Write-Host "ReportMarkdown: $reportMarkdownPath"
    Write-Host "Presets: $($expandedPresets -join ',')"
    Write-Host "WidthHeight: ${Width}x${Height}"
    return
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

if (-not $SkipRun) {
    & $captureScript -RepoRoot $RepoRoot -OutputDir $captureDir -Presets $expandedPresets -Width $Width -Height $Height -CaptureTimeoutSeconds $CaptureTimeoutSeconds
}

& $sanityScript -RepoRoot $RepoRoot -CaptureDir $captureDir -Presets $expandedPresets -ExpectedWidth $Width -ExpectedHeight $Height

$results = New-Object System.Collections.Generic.List[object]
foreach ($preset in $expandedPresets) {
    $jsonPath = Join-Path $captureDir "$preset.json"
    Require-File -Path (Join-Path $captureDir "$preset.png") -Label "$preset screenshot"
    Require-File -Path $jsonPath -Label "$preset sidecar"
    Require-File -Path (Join-Path $captureDir "$preset.log") -Label "$preset capture log"

    $sidecar = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
    [void]$results.Add((Test-F30Sidecar -Preset $preset -Sidecar $sidecar))
}

$report = [pscustomobject]@{
    schema = "PCControlledDemoVisualReadabilityFixes"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    width = $Width
    height = $Height
    presets = $expandedPresets
    captureDir = Convert-ToRepoRelativePath -Path $captureDir
    completedTask = "F30 implement PC controlled-demo visual readability fixes"
    nextFormalTask = "F31 refresh PC controlled-demo visual evidence after readability fixes"
    fixedAreas = @(
        "terrain contrast and shoreline readability",
        "objective-near tree/prop occlusion",
        "high-contact unit footprint rings",
        "damage/ejection cue palette separation",
        "compact PC status rail"
    )
    presetResults = $results
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
[void]$markdownLines.Add("# PC Controlled Demo Visual Readability Fixes")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("Result: pass")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("Evidence: $(Convert-ToRepoRelativePath -Path $captureDir)")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("## Fixed Areas")
foreach ($area in $report.fixedAreas) {
    [void]$markdownLines.Add("- $area")
}

[void]$markdownLines.Add("")
[void]$markdownLines.Add("## Presets")
foreach ($result in $results) {
    [void]$markdownLines.Add(("- {0}: textureStrength={1}, objectiveNearProps={2}, treeClamps={3}, statusRailW={4}, statusRailShare1280={5}, hostiles={6}/{7} visible" -f `
        $result.preset,
        $result.textureStrength,
        $result.objectiveNearProps,
        $result.treeClamps,
        $result.statusRailWidth,
        $result.statusRailShare1280,
        $result.visibleHostiles,
        $result.activeHostiles))
}

$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo visual readability fixes check OK."
Write-Host "Report: $reportJsonPath"
