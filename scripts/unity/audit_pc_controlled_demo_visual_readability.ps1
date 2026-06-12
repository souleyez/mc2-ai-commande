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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-visual-readability-audit"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
$captureDir = Join-Path $OutputDir "captures"
$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-visual-readability-audit.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-visual-readability-audit.md"
$captureScript = Join-Path $RepoRoot "scripts\unity\capture_reference_visuals.ps1"
$sanityScript = Join-Path $RepoRoot "scripts\unity\check_pc_visual_capture_sanity.ps1"
$requiredPresets = @("spawn", "hangar-contact", "damage-demo")

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Convert-ToRepoRelativePath {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    if ($fullPath.StartsWith($RepoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $fullPath.Substring($RepoRoot.Length).TrimStart("\", "/")
    }

    return $fullPath
}

function Normalize-CapturePreset {
    param([string]$Preset)
    if ([string]::IsNullOrWhiteSpace($Preset)) {
        return ""
    }

    return $Preset.Trim().ToLowerInvariant().Replace("_", "-")
}

function Expand-CapturePresets {
    param([string[]]$RawPresets)

    $expanded = New-Object System.Collections.Generic.List[string]
    foreach ($rawPreset in $RawPresets) {
        foreach ($part in ($rawPreset -split ",")) {
            $normalized = Normalize-CapturePreset -Preset $part
            if (-not [string]::IsNullOrWhiteSpace($normalized)) {
                [void]$expanded.Add($normalized)
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

    if ($Summary -match $Pattern) {
        return [double]$Matches[1]
    }

    throw "$Label missing numeric pattern: $Pattern"
}

function Measure-CaptureImage {
    param([string]$Path)

    Add-Type -AssemblyName System.Drawing
    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        $unique = New-Object 'System.Collections.Generic.HashSet[int]'
        $centerUnique = New-Object 'System.Collections.Generic.HashSet[int]'
        $lumaValues = New-Object System.Collections.Generic.List[double]
        $samples = 0
        $centerSamples = 0
        $centerLit = 0
        $magentaSamples = 0
        $monochromeSamples = 0
        $step = 8
        $centerLeft = [int]($bitmap.Width * 0.32)
        $centerRight = [int]($bitmap.Width * 0.72)
        $centerTop = [int]($bitmap.Height * 0.24)
        $centerBottom = [int]($bitmap.Height * 0.76)

        for ($y = 0; $y -lt $bitmap.Height; $y += $step) {
            for ($x = 0; $x -lt $bitmap.Width; $x += $step) {
                $color = $bitmap.GetPixel($x, $y)
                $samples++
                [void]$unique.Add($color.ToArgb())

                $maxChannel = [Math]::Max($color.R, [Math]::Max($color.G, $color.B))
                $minChannel = [Math]::Min($color.R, [Math]::Min($color.G, $color.B))
                if (($maxChannel - $minChannel) -lt 8) {
                    $monochromeSamples++
                }

                if ($color.R -gt 180 -and $color.B -gt 180 -and $color.G -lt 110) {
                    $magentaSamples++
                }

                $luma = ($color.R * 0.2126) + ($color.G * 0.7152) + ($color.B * 0.0722)
                $lumaValues.Add($luma)

                if ($x -ge $centerLeft -and $x -le $centerRight -and $y -ge $centerTop -and $y -le $centerBottom) {
                    [void]$centerUnique.Add($color.ToArgb())
                    if ($luma -gt 18) {
                        $centerLit++
                    }

                    $centerSamples++
                }
            }
        }

        $averageLuma = ($lumaValues | Measure-Object -Average).Average
        $lumaVariance = (($lumaValues | ForEach-Object { [Math]::Pow($_ - $averageLuma, 2) }) | Measure-Object -Average).Average
        $lumaStdDev = [Math]::Sqrt($lumaVariance)
        $centerLitRatio = if ($centerSamples -le 0) { 0.0 } else { $centerLit / [double]$centerSamples }
        $magentaRatio = if ($samples -le 0) { 0.0 } else { $magentaSamples / [double]$samples }
        $monochromeRatio = if ($samples -le 0) { 0.0 } else { $monochromeSamples / [double]$samples }

        return [pscustomobject]@{
            width = $bitmap.Width
            height = $bitmap.Height
            uniqueColors = $unique.Count
            centerUniqueColors = $centerUnique.Count
            centerLitRatio = [Math]::Round($centerLitRatio, 3)
            lumaStdDev = [Math]::Round($lumaStdDev, 2)
            magentaRatio = [Math]::Round($magentaRatio, 4)
            monochromeRatio = [Math]::Round($monochromeRatio, 3)
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Add-FollowUp {
    param(
        [System.Collections.Generic.List[object]]$Items,
        [string]$Priority,
        [string]$Area,
        [string]$Preset,
        [string]$Issue,
        [string]$NextFix
    )

    [void]$Items.Add([pscustomobject]@{
        priority = $Priority
        area = $Area
        preset = $Preset
        issue = $Issue
        nextFix = $NextFix
    })
}

function Test-CommonBattleReadability {
    param(
        [string]$Preset,
        [object]$Sidecar,
        [object]$ImageMetrics,
        [System.Collections.Generic.List[object]]$FollowUps
    )

    if ([string]$Sidecar.flowScreen -ne "Battle") {
        throw "$Preset audit expected Battle flow, got: $($Sidecar.flowScreen)"
    }

    if ([int]$Sidecar.screenWidth -ne $Width -or [int]$Sidecar.screenHeight -ne $Height) {
        throw "$Preset audit expected ${Width}x${Height}, got $($Sidecar.screenWidth)x$($Sidecar.screenHeight)"
    }

    if ($ImageMetrics.uniqueColors -lt 180 -or $ImageMetrics.centerUniqueColors -lt 80 -or $ImageMetrics.centerLitRatio -lt 0.15 -or $ImageMetrics.lumaStdDev -lt 12.0) {
        throw "$Preset audit image metrics are below readable thresholds: colors=$($ImageMetrics.uniqueColors), center=$($ImageMetrics.centerUniqueColors), lit=$($ImageMetrics.centerLitRatio), luma=$($ImageMetrics.lumaStdDev)"
    }

    if ($ImageMetrics.magentaRatio -gt 0.03 -or $ImageMetrics.monochromeRatio -gt 0.90) {
        throw "$Preset audit image indicates fallback color or flat monochrome: magenta=$($ImageMetrics.magentaRatio), monochrome=$($ImageMetrics.monochromeRatio)"
    }

    foreach ($fragment in @(
        "FirstMapVisual=terrain+unit+structure+sparse-ui+occupancy+contact",
        "flow=Battle",
        "status=ready",
        "terrain=ready",
        "unit=ready",
        "structure=ready",
        "sparseHud=ready",
        "occupancy=ready",
        "contact=separated",
        "visualOnly=yes",
        "pathing=unchanged",
        "collision=unchanged"
    )) {
        Require-Text -Text ([string]$Sidecar.firstMapVisual) -Needle $fragment -Label "$Preset firstMapVisual"
    }

    foreach ($fragment in @(
        "TerrainReadability=",
        "texture=composite",
        "waterSurface=readable-overlay",
        "style=land-outline+runway-contrast+water-muted",
        "pathing=unchanged"
    )) {
        Require-Text -Text ([string]$Sidecar.terrainReadability) -Needle $fragment -Label "$Preset terrainReadability"
    }

    foreach ($fragment in @(
        "UnitReadability=contact-shadow+faction-footprint-ring",
        "factionRing=player-cyan+hostile-red",
        "style=grounded-silhouette+friend-foe-footprint",
        "pathing=unchanged",
        "collision=unchanged"
    )) {
        Require-Text -Text ([string]$Sidecar.unitReadability) -Needle $fragment -Label "$Preset unitReadability"
    }

    foreach ($fragment in @(
        "StructureReadability=base-shadow+target-footprint",
        "target=distinct",
        "color=category-tone-separation",
        "visualOnly=yes",
        "collision=unchanged",
        "blockerGeometry=unchanged"
    )) {
        Require-Text -Text ([string]$Sidecar.structureReadability) -Needle $fragment -Label "$Preset structureReadability"
    }

    foreach ($fragment in @(
        "BattleHud=active",
        "SparseBattleUi=statusRows+sections+solo",
        "combatLog=hidden",
        "objective=compactObjective",
        "overlays=hidden"
    )) {
        Require-Text -Text ([string]$Sidecar.battleHud) -Needle $fragment -Label "$Preset battleHud"
    }

    foreach ($fragment in @(
        "ContactClearance=players",
        "overlaps=0",
        "status=separated"
    )) {
        Require-Text -Text ([string]$Sidecar.contactClearance) -Needle $fragment -Label "$Preset contactClearance"
    }

    $textureStrength = Read-SummaryNumber -Summary ([string]$Sidecar.terrainReadability) -Pattern "textureStrength=([0-9.]+)" -Label "$Preset terrainReadability"
    if ($textureStrength -lt 0.24) {
        throw "$Preset terrain texture strength is too low for the current readable baseline: $textureStrength"
    }

    if ($textureStrength -lt 0.34) {
        Add-FollowUp -Items $FollowUps -Priority "P1" -Area "terrain" -Preset $Preset -Issue "Terrain textureStrength=$textureStrength leaves land/water/shore contrast readable but still soft." -NextFix "Raise terrain texture and shoreline contrast while preserving water mute and pathing unchanged."
    }

    $treeObjects = Read-SummaryNumber -Summary ([string]$Sidecar.structureReadability) -Pattern "treeObjects=([0-9]+)" -Label "$Preset structureReadability"
    if ($treeObjects -gt 500) {
        Add-FollowUp -Items $FollowUps -Priority "P1" -Area "occlusion" -Preset $Preset -Issue "Dense treeObjects=$treeObjects around the objective can hide units and wreck cues." -NextFix "Add objective-near prop fade/height clamp for tall trees and dense barricade clusters."
    }

    if ([int]$Sidecar.activeHostileCount -gt 0 -and [int]$Sidecar.visibleHostileCount -lt 8) {
        throw "$Preset contact scene has too few visible hostiles: $($Sidecar.visibleHostileCount)"
    }

    if ([int]$Sidecar.activeHostileCount -ge 20) {
        Add-FollowUp -Items $FollowUps -Priority "P2" -Area "units" -Preset $Preset -Issue "High-contact scene uses low-alpha footprint rings while 20 hostiles are active." -NextFix "Increase hostile/player ring contrast under contact pressure without adding text labels."
    }
}

function Test-DamageReadability {
    param(
        [string]$Preset,
        [object]$Sidecar,
        [System.Collections.Generic.List[object]]$FollowUps
    )

    foreach ($fragment in @(
        "DamageStory=units",
        "left-arm-lost",
        "legs-lost",
        "cockpit-lost",
        "lostSections=",
        "pilotRisk=",
        "destroyedUnits="
    )) {
        Require-Text -Text ([string]$Sidecar.damageStory) -Needle $fragment -Label "$Preset damageStory"
    }

    foreach ($fragment in @(
        "DamageReadability=weaponFamilies energy+missile+ballistic+explosive",
        "sectionConsequences arms-firepower legs-mobility cockpit-ejection wreck-salvage",
        "hud=section-bars+short-labels+sparse",
        "Arms=missing-socket+flag+flight+landing-debris+firepower-marker",
        "Legs=collapse+red-cross+skid+dust+danger-ring+mobility-beacon",
        "Cockpit=breach+ejection-pod+chute+landing+arc+distress+escape-column+route",
        "SectionStatus=bar+short-label+critical+destroyed"
    )) {
        Require-Text -Text ([string]$Sidecar.damageReadability) -Needle $fragment -Label "$Preset damageReadability"
    }

    Add-FollowUp -Items $FollowUps -Priority "P1" -Area "damage-fx" -Preset $Preset -Issue "Damage-demo proves arm/leg/cockpit story cues, but red target and damage rings visually compete around the hangar." -NextFix "Separate damage/ejection cue colors from target-hot rings and reduce overlapping red fill near the objective footprint."
}

$expandedPresets = @(Expand-CapturePresets -RawPresets $Presets)
foreach ($requiredPreset in $requiredPresets) {
    if ($expandedPresets -notcontains $requiredPreset) {
        throw "F29 audit requires preset '$requiredPreset'. Presets: $($expandedPresets -join ',')"
    }
}

Require-File -Path $captureScript -Label "reference visual capture script"
Require-File -Path $sanityScript -Label "PC visual capture sanity script"

if ($PlanOnly) {
    Write-Host "PC controlled-demo visual readability audit plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "CaptureDir: $captureDir"
    Write-Host "ReportJson: $reportJsonPath"
    Write-Host "ReportMarkdown: $reportMarkdownPath"
    Write-Host "Presets: $($expandedPresets -join ',')"
    Write-Host "WidthHeight: ${Width}x${Height}"
    Write-Host "CaptureScript: $captureScript"
    Write-Host "SanityScript: $sanityScript"
    return
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

if (-not $SkipRun) {
    & $captureScript -RepoRoot $RepoRoot -OutputDir $captureDir -Presets $expandedPresets -Width $Width -Height $Height -CaptureTimeoutSeconds $CaptureTimeoutSeconds
}

& $sanityScript -RepoRoot $RepoRoot -CaptureDir $captureDir -Presets $expandedPresets -ExpectedWidth $Width -ExpectedHeight $Height

$followUps = New-Object System.Collections.Generic.List[object]
$presetResults = New-Object System.Collections.Generic.List[object]
foreach ($preset in $expandedPresets) {
    $pngPath = Join-Path $captureDir "$preset.png"
    $jsonPath = Join-Path $captureDir "$preset.json"
    $logPath = Join-Path $captureDir "$preset.log"
    Require-File -Path $pngPath -Label "$preset screenshot"
    Require-File -Path $jsonPath -Label "$preset sidecar"
    Require-File -Path $logPath -Label "$preset capture log"

    $sidecar = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $metrics = Measure-CaptureImage -Path $pngPath
    Test-CommonBattleReadability -Preset $preset -Sidecar $sidecar -ImageMetrics $metrics -FollowUps $followUps
    if ($preset -eq "damage-demo") {
        Test-DamageReadability -Preset $preset -Sidecar $sidecar -FollowUps $followUps
    }

    [void]$presetResults.Add([pscustomobject]@{
        preset = $preset
        png = Convert-ToRepoRelativePath -Path $pngPath
        json = Convert-ToRepoRelativePath -Path $jsonPath
        log = Convert-ToRepoRelativePath -Path $logPath
        flow = [string]$sidecar.flowScreen
        missionTimeSeconds = [Math]::Round([double]$sidecar.missionTimeSeconds, 2)
        playerUnits = [int]$sidecar.playerUnitCount
        activeHostiles = [int]$sidecar.activeHostileCount
        visibleHostiles = [int]$sidecar.visibleHostileCount
        targetableStructures = [int]$sidecar.targetableStructureCount
        image = $metrics
        terrainReadability = [string]$sidecar.terrainReadability
        unitReadability = [string]$sidecar.unitReadability
        structureReadability = [string]$sidecar.structureReadability
        battleHud = [string]$sidecar.battleHud
        contactClearance = [string]$sidecar.contactClearance
        damageStory = [string]$sidecar.damageStory
        firstMapVisual = [string]$sidecar.firstMapVisual
    })
}

Add-FollowUp -Items $followUps -Priority "P2" -Area "hud" -Preset "all" -Issue "Sparse HUD is functional, but the left status rail still consumes about one quarter of the 1280px battle view." -NextFix "Prototype a denser PC status rail or temporary collapse state without adding combat log noise."

if ($followUps.Count -lt 3) {
    throw "F29 audit must produce a concrete next visual fix list."
}

$report = [pscustomobject]@{
    schema = "PCControlledDemoVisualReadabilityAudit"
    result = "pass-with-followups"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    width = $Width
    height = $Height
    presets = $expandedPresets
    captureDir = Convert-ToRepoRelativePath -Path $captureDir
    readabilityBaseline = "pass"
    nextFormalTask = "F30 implement PC controlled-demo visual readability fixes"
    presetResults = $presetResults
    followUpItems = $followUps
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
[void]$markdownLines.Add("# PC Controlled Demo Visual Readability Audit")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("Result: pass-with-followups")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("Evidence: $(Convert-ToRepoRelativePath -Path $captureDir)")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("## Presets")
foreach ($result in $presetResults) {
    [void]$markdownLines.Add(("- {0}: units={1}, hostiles={2}/{3} visible, targetableStructures={4}, colors={5}, centerColors={6}, lumaStdDev={7}" -f `
        $result.preset,
        $result.playerUnits,
        $result.visibleHostiles,
        $result.activeHostiles,
        $result.targetableStructures,
        $result.image.uniqueColors,
        $result.image.centerUniqueColors,
        $result.image.lumaStdDev))
}

[void]$markdownLines.Add("")
[void]$markdownLines.Add("## Next Visual Fix List")
foreach ($item in $followUps) {
    [void]$markdownLines.Add(("- {0} {1} [{2}]: {3} Next: {4}" -f $item.priority, $item.area, $item.preset, $item.issue, $item.nextFix))
}

[System.IO.File]::WriteAllLines($reportMarkdownPath, $markdownLines, [System.Text.UTF8Encoding]::new($false))

Write-Host "PC controlled-demo visual readability audit OK."
Write-Host "Repo: $RepoRoot"
Write-Host "CaptureDir: $captureDir"
Write-Host "ReportJson: $reportJsonPath"
Write-Host "ReportMarkdown: $reportMarkdownPath"
Write-Host "PresetCount: $($presetResults.Count)"
Write-Host "FollowUpCount: $($followUps.Count)"
