param(
    [string]$RepoRoot = "",
    [string]$OutputDir = "",
    [string[]]$Presets = @("mechlab", "spawn", "airfield", "hangar-contact", "damage-demo", "north-patrol"),
    [int]$Width = 1280,
    [int]$Height = 720,
    [int]$CaptureTimeoutSeconds = 75,
    [switch]$NoOccupancyPlaceholders,
    [switch]$SkipRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot "analysis-output\reference-visual-captures"
}

$gameExe = Join-Path $RepoRoot "unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe"
if (-not (Test-Path $gameExe)) {
    throw "Missing Unity player build: $gameExe"
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
Add-Type -AssemblyName System.Drawing

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
            if (-not [string]::IsNullOrWhiteSpace($part)) {
                $expanded.Add((Normalize-CapturePreset $part))
            }
        }
    }

    return $expanded.ToArray()
}

function Test-CaptureImage {
    param(
        [string]$Path,
        [int]$ExpectedWidth,
        [int]$ExpectedHeight
    )

    if (-not (Test-Path $Path)) {
        throw "Missing capture image: $Path"
    }

    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        if ($bitmap.Width -ne $ExpectedWidth -or $bitmap.Height -ne $ExpectedHeight) {
            throw "Unexpected image size for $Path`: $($bitmap.Width)x$($bitmap.Height), expected ${ExpectedWidth}x${ExpectedHeight}"
        }

        $unique = New-Object 'System.Collections.Generic.HashSet[int]'
        $centerUnique = New-Object 'System.Collections.Generic.HashSet[int]'
        $centerLit = 0
        $centerSamples = 0
        $step = 8
        $centerLeft = [int]($bitmap.Width * 0.32)
        $centerRight = [int]($bitmap.Width * 0.72)
        $centerTop = [int]($bitmap.Height * 0.24)
        $centerBottom = [int]($bitmap.Height * 0.76)

        for ($y = 0; $y -lt $bitmap.Height; $y += $step) {
            for ($x = 0; $x -lt $bitmap.Width; $x += $step) {
                $color = $bitmap.GetPixel($x, $y)
                [void]$unique.Add($color.ToArgb())
                if ($x -ge $centerLeft -and $x -le $centerRight -and $y -ge $centerTop -and $y -le $centerBottom) {
                    [void]$centerUnique.Add($color.ToArgb())
                    $luma = ($color.R * 0.2126) + ($color.G * 0.7152) + ($color.B * 0.0722)
                    if ($luma -gt 18) {
                        $centerLit++
                    }

                    $centerSamples++
                }
            }
        }

        if ($unique.Count -lt 64) {
            throw "Image looks too flat: $Path uniqueColors=$($unique.Count)"
        }

        if ($centerUnique.Count -lt 24) {
            throw "Center view looks occluded or blank: $Path centerUniqueColors=$($centerUnique.Count)"
        }

        $litRatio = if ($centerSamples -le 0) { 0.0 } else { $centerLit / [double]$centerSamples }
        if ($litRatio -lt 0.04) {
            throw "Center view is too dark or occluded: $Path litRatio=$([Math]::Round($litRatio, 3))"
        }

        return [pscustomobject]@{
            Width = $bitmap.Width
            Height = $bitmap.Height
            UniqueColors = $unique.Count
            CenterUniqueColors = $centerUnique.Count
            CenterLitRatio = [Math]::Round($litRatio, 3)
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Wait-CaptureFile {
    param(
        [string]$Path,
        [int]$TimeoutSeconds = 45
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    $lastLength = -1
    while ((Get-Date) -lt $deadline) {
        if (Test-Path $Path) {
            $item = Get-Item $Path
            if ($item.Length -gt 0 -and $item.Length -eq $lastLength) {
                return
            }

            $lastLength = $item.Length
        }

        Start-Sleep -Milliseconds 200
    }

    throw "Timed out waiting for capture file: $Path"
}

function Stop-CapturePlayer {
    param([string]$ExecutablePath)

    $resolvedExe = (Resolve-Path -LiteralPath $ExecutablePath).Path
    $buildDir = Split-Path -Parent $resolvedExe
    Get-Process | Where-Object {
        try {
            if ($_.Path -eq $resolvedExe) {
                $true
            }
            elseif ($_.ProcessName -eq "UnityCrashHandler64" -and $_.Path) {
                $_.Path.StartsWith($buildDir, [StringComparison]::OrdinalIgnoreCase)
            }
            else {
                $false
            }
        }
        catch {
            $false
        }
    } | Stop-Process -Force -ErrorAction SilentlyContinue
}

function Test-CaptureSidecar {
    param(
        [string]$Path,
        [string]$ExpectedPreset,
        [bool]$ExpectOccupancyPlaceholders = $true
    )

    if (-not (Test-Path $Path)) {
        throw "Missing capture sidecar: $Path"
    }

    $sidecar = Get-Content -Path $Path -Raw | ConvertFrom-Json
    if ($sidecar.preset -ne $ExpectedPreset) {
        throw "Unexpected sidecar preset for $Path`: $($sidecar.preset), expected $ExpectedPreset"
    }

    if ($sidecar.screenWidth -le 0 -or $sidecar.screenHeight -le 0) {
        throw "Sidecar has invalid screen dimensions: $Path"
    }

    if ($null -eq $sidecar.camera -or $sidecar.camera.orthographicSize -le 0) {
        throw "Sidecar has invalid camera state: $Path"
    }

    if ($ExpectedPreset -eq "mechlab") {
        Test-MechLabCaptureSidecar -Sidecar $sidecar -Path $Path
    }
    elseif ($ExpectedPreset -eq "damage-demo") {
        Test-DamageDemoCaptureSidecar -Sidecar $sidecar -Path $Path
    }
    elseif ($ExpectedPreset -eq "solo-order") {
        Test-SoloOrderCaptureSidecar -Sidecar $sidecar -Path $Path
    }
    elseif ($ExpectedPreset -eq "solo-return") {
        Test-SoloReturnCaptureSidecar -Sidecar $sidecar -Path $Path
    }
    elseif ($ExpectedPreset -eq "hangar-contact") {
        Test-HangarContactCaptureSidecar -Sidecar $sidecar -Path $Path
    }

    if ($sidecar.flowScreen -eq "Battle") {
        Test-BattleHudCaptureSidecar -Sidecar $sidecar -Path $Path
        Test-MobileTouchCaptureSidecar -Sidecar $sidecar -Path $Path
        Test-BattleOccupancyCaptureSidecar -Sidecar $sidecar -Path $Path
        Test-ContactClearanceCaptureSidecar -Sidecar $sidecar -Path $Path
        Test-TerrainReadabilityCaptureSidecar -Sidecar $sidecar -Path $Path
        Test-UnitReadabilityCaptureSidecar -Sidecar $sidecar -Path $Path
        Test-StructureReadabilityCaptureSidecar -Sidecar $sidecar -Path $Path
        Test-FirstMapVisualCaptureSidecar -Sidecar $sidecar -Path $Path
    }

    $placeholderSummary = [string]$sidecar.occupancyPlaceholders
    if ($ExpectOccupancyPlaceholders) {
        foreach ($fragment in @(
            "OccupancyPlaceholders=enabled",
            " units ",
            " structures ",
            " hardProps ",
            " landingBlockedMarkers ",
            "source=BattleMission.OccupancyPlaceholderRegions+DemoTerrainView.LandingReviewBlockedMarkers"
        )) {
            if ($placeholderSummary -notlike "*$fragment*") {
                throw "Sidecar occupancy placeholder summary missing '$fragment': $Path -> $placeholderSummary"
            }
        }
    }
    elseif ($placeholderSummary -notlike "OccupancyPlaceholders=disabled*") {
        throw "Sidecar occupancy placeholders should be disabled: $Path -> $placeholderSummary"
    }

    return $sidecar
}

function Test-MobileTouchCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    $summary = [string]$Sidecar.mobileTouch
    foreach ($fragment in @(
        "MobileTouchUi=ready",
        "orientation=landscape",
        "commandTargets=44",
        "statusRows=44",
        "primaryButtons=44",
        "mapBack=44",
        "systemButtons=44",
        "mechLabBack=44",
        "mechLabGridCell>=36",
        "touchRatios=16:9+19.5:9+20:9",
        "landscapeOnly=yes",
        "noDragBox=yes",
        "combatLog=hidden",
        "status=ready"
    )) {
        if ($summary -notlike "*$fragment*") {
            throw "Mobile touch sidecar summary missing '$fragment': $Path -> $summary"
        }
    }
}

function Read-SummaryNumber {
    param(
        [string]$Summary,
        [string]$Pattern,
        [string]$Context
    )

    if ($Summary -notmatch $Pattern) {
        throw "Missing numeric summary pattern '$Pattern' in $Context`: $Summary"
    }

    return [double]$Matches[1]
}

function Test-BattleHudCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    $summary = [string]$Sidecar.battleHud
    foreach ($fragment in @(
        "BattleHud=active",
        "controls=statusRows+jet+map+bay+system",
        "combatLogVisible=no",
        "objectivePanel=compactObjective",
        "saveUi=disabled",
        "SparseBattleUi=statusRows+sections+solo",
        "controls=all+jet+map+bay+system",
        "combatLog=hidden",
        "objective=compactObjective",
        "missionMap=available-closed",
        "accountUi=hidden",
        "economyUi=funds-only",
        "debugOccupancy=sidecar-only",
        "overlays=hidden"
    )) {
        if ($summary -notlike "*$fragment*") {
            throw "Battle HUD sidecar summary missing '$fragment': $Path -> $summary"
        }
    }

    foreach ($forbidden in @(
        "combatLogVisible=yes",
        "combatLog=visible",
        "saveUi=enabled",
        "accountUi=visible",
        "debugOccupancy=visible",
        "overlays=visible"
    )) {
        if ($summary -like "*$forbidden*") {
            throw "Battle HUD sidecar summary contains forbidden sparse-mode fragment '$forbidden': $Path -> $summary"
        }
    }

    if ($summary -notmatch "combatPanel=h([0-9.]+)") {
        throw "Battle HUD sidecar summary missing combat panel height: $Path -> $summary"
    }

    $height = [double]$Matches[1]
    if ($height -gt 84.0) {
        throw "Battle HUD combat panel is too tall for sparse mode: $Path -> $summary"
    }
}

function Test-TerrainReadabilityCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    $summary = [string]$Sidecar.terrainReadability
    foreach ($fragment in @(
        "TerrainReadability=samples",
        "texture=",
        "textureStrength=",
        "waterSurface=readable-overlay",
        "water=",
        "shore=",
        "runway=",
        "style=land-outline+runway-contrast+water-muted",
        "pathing=unchanged"
    )) {
        if ($summary -notlike "*$fragment*") {
            throw "Terrain readability sidecar summary missing '$fragment': $Path -> $summary"
        }
    }

    $water = Read-SummaryNumber -Summary $summary -Pattern "water=([0-9]+)" -Context $Path
    $shore = Read-SummaryNumber -Summary $summary -Pattern "shore=([0-9]+)" -Context $Path
    $runway = Read-SummaryNumber -Summary $summary -Pattern "runway=([0-9]+)" -Context $Path
    if ($water -le 0 -or $shore -le 0 -or $runway -le 0) {
        throw "Terrain readability sidecar did not prove water, shore and runway coverage: $Path -> $summary"
    }
}

function Test-BattleOccupancyCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    $summary = [string]$Sidecar.occupancy
    foreach ($fragment in @(
        "BattleOccupancy=units",
        "unitRadii infantry=24 vehicle=54 mech=64",
        "structures ",
        "maxStructureRadius=",
        "hardProps ",
        "building=",
        "aircraft=",
        "barricade=",
        "other=",
        "maxPropRadius=",
        "destinationFallback=structure+hardProp",
        "Landing=DemoTerrainView",
        "externalPredicate=water+mapBounds"
    )) {
        if ($summary -notlike "*$fragment*") {
            throw "Battle occupancy sidecar summary missing '$fragment': $Path -> $summary"
        }
    }

    $units = Read-SummaryNumber -Summary $summary -Pattern "BattleOccupancy=units ([0-9]+)" -Context $Path
    $structures = Read-SummaryNumber -Summary $summary -Pattern "structures ([0-9]+)" -Context $Path
    $hardProps = Read-SummaryNumber -Summary $summary -Pattern "hardProps ([0-9]+)" -Context $Path
    $buildingProps = Read-SummaryNumber -Summary $summary -Pattern "building=([0-9]+)" -Context $Path
    $aircraftProps = Read-SummaryNumber -Summary $summary -Pattern "aircraft=([0-9]+)" -Context $Path
    $barricades = Read-SummaryNumber -Summary $summary -Pattern "barricade=([0-9]+)" -Context $Path
    if ($units -le 0 -or $structures -le 0 -or $hardProps -le 0 -or $buildingProps -le 0 -or $aircraftProps -le 0 -or $barricades -le 0) {
        throw "Battle occupancy sidecar did not prove unit, structure and hard-prop coverage: $Path -> $summary"
    }
}

function Test-UnitReadabilityCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    $summary = [string]$Sidecar.unitReadability
    foreach ($fragment in @(
        "UnitReadability=contact-shadow+faction-footprint-ring",
        "contactShadow=low-black",
        "factionRing=player-cyan+hostile-red",
        "labels=no",
        "sectionDamage=overlays",
        "battleCore=unchanged",
        "units=",
        "activeViews=",
        "player=",
        "hostile=",
        "tall=",
        "vehicle=",
        "infantry=",
        "style=grounded-silhouette+friend-foe-footprint",
        "pathing=unchanged",
        "collision=unchanged"
    )) {
        if ($summary -notlike "*$fragment*") {
            throw "Unit readability sidecar summary missing '$fragment': $Path -> $summary"
        }
    }

    $activeUnits = Read-SummaryNumber -Summary $summary -Pattern "units=([0-9]+)" -Context $Path
    $activeViews = Read-SummaryNumber -Summary $summary -Pattern "activeViews=([0-9]+)" -Context $Path
    $playerUnits = Read-SummaryNumber -Summary $summary -Pattern "player=([0-9]+)" -Context $Path
    if ($activeUnits -le 0 -or $activeViews -le 0 -or $playerUnits -le 0) {
        throw "Unit readability sidecar did not prove active player unit cues: $Path -> $summary"
    }

    if ($Sidecar.preset -ne "spawn" -and $Sidecar.preset -ne "solo-order" -and $Sidecar.preset -ne "solo-return") {
        $hostileUnits = Read-SummaryNumber -Summary $summary -Pattern "hostile=([0-9]+)" -Context $Path
        if ($hostileUnits -le 0) {
            throw "Unit readability sidecar did not prove hostile unit cues: $Path -> $summary"
        }
    }
}

function Test-StructureReadabilityCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    $summary = [string]$Sidecar.structureReadability
    foreach ($fragment in @(
        "StructureReadability=base-shadow+target-footprint",
        "baseShadow=low-black",
        "targetFootprint=amber",
        "target=distinct",
        "labels=no",
        "battleCore=unchanged",
        "structures=",
        "structureViews=",
        "targetable=",
        "terrainProps=",
        "hardProps=",
        "building=",
        "aircraft=",
        "barricade=",
        "treeObjects=",
        "color=category-tone-separation",
        "textureTint=category-base",
        "visualOnly=yes",
        "collision=unchanged",
        "blockerGeometry=unchanged",
        "ReferenceStructures=loaded",
        "ReferenceProps=loaded",
        "ReferencePropScale="
    )) {
        if ($summary -notlike "*$fragment*") {
            throw "Structure readability sidecar summary missing '$fragment': $Path -> $summary"
        }
    }

    $structures = Read-SummaryNumber -Summary $summary -Pattern "structures=([0-9]+)" -Context $Path
    $structureViews = Read-SummaryNumber -Summary $summary -Pattern "structureViews=([0-9]+)" -Context $Path
    $targetable = Read-SummaryNumber -Summary $summary -Pattern "targetable=([0-9]+)" -Context $Path
    $terrainProps = Read-SummaryNumber -Summary $summary -Pattern "terrainProps=([0-9]+)" -Context $Path
    $hardProps = Read-SummaryNumber -Summary $summary -Pattern "hardProps=([0-9]+)" -Context $Path
    $buildings = Read-SummaryNumber -Summary $summary -Pattern "building=([0-9]+)" -Context $Path
    $aircraft = Read-SummaryNumber -Summary $summary -Pattern "aircraft=([0-9]+)" -Context $Path
    $barricades = Read-SummaryNumber -Summary $summary -Pattern "barricade=([0-9]+)" -Context $Path
    $treeObjects = Read-SummaryNumber -Summary $summary -Pattern "treeObjects=([0-9]+)" -Context $Path
    if ($structures -le 0 -or $structureViews -lt $structures -or $targetable -le 0) {
        throw "Structure readability sidecar did not prove target structure cues: $Path -> $summary"
    }

    if ($terrainProps -le 0 -or $hardProps -le 0 -or $buildings -le 0 -or $aircraft -le 0 -or $barricades -le 0 -or $treeObjects -le 0) {
        throw "Structure readability sidecar did not prove map prop category coverage: $Path -> $summary"
    }
}

function Test-FirstMapVisualCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    $summary = [string]$Sidecar.firstMapVisual
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
        "damageStory=ready",
        "image=external-script-gated",
        "png=nonblank+unique-colors",
        "battleCore=occupancy+contact-clearance",
        "sparseUi=statusRows+compactObjective",
        "privateReference=replaceable",
        "visualOnly=yes",
        "pathing=unchanged",
        "collision=unchanged",
        "playerUnits=",
        "activeHostiles=",
        "visibleHostiles=",
        "targetableStructures="
    )) {
        if ($summary -notlike "*$fragment*") {
            throw "First map visual sidecar summary missing '$fragment': $Path -> $summary"
        }
    }

    $players = Read-SummaryNumber -Summary $summary -Pattern "playerUnits=([0-9]+)" -Context $Path
    $targetable = Read-SummaryNumber -Summary $summary -Pattern "targetableStructures=([0-9]+)" -Context $Path
    if ($players -le 0 -or $targetable -le 0) {
        throw "First map visual sidecar did not prove player units and objective structure: $Path -> $summary"
    }

    if ($Sidecar.preset -ne "spawn" -and $Sidecar.preset -ne "solo-order" -and $Sidecar.preset -ne "solo-return") {
        $activeHostiles = Read-SummaryNumber -Summary $summary -Pattern "activeHostiles=([0-9]+)" -Context $Path
        if ($activeHostiles -le 0) {
            throw "First map visual sidecar did not prove hostile presence: $Path -> $summary"
        }
    }
}

function Test-ContactClearanceCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    $summary = [string]$Sidecar.contactClearance
    foreach ($fragment in @(
        "ContactClearance=players",
        "hostiles",
        "nearestPH=",
        "nearestHH=",
        "nearestPP=",
        "distance=",
        "radii=",
        "clearance=",
        "overlaps=",
        "worstClearance=",
        "status=separated"
    )) {
        if ($summary -notlike "*$fragment*") {
            throw "Contact clearance sidecar summary missing '$fragment': $Path -> $summary"
        }
    }

    $overlaps = Read-SummaryNumber -Summary $summary -Pattern "overlaps=([0-9]+)" -Context $Path
    if ($overlaps -ne 0) {
        throw "Contact clearance sidecar reports overlapping unit pairs: $Path -> $summary"
    }

    if ($summary -match "worstClearance=([-0-9.]+)") {
        $worstClearance = [double]$Matches[1]
        if ($worstClearance -lt -0.1) {
            throw "Contact clearance sidecar reports negative worst clearance: $Path -> $summary"
        }
    }
    else {
        throw "Contact clearance sidecar missing numeric worst clearance: $Path -> $summary"
    }
}

function Test-HangarContactCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    if ($Sidecar.flowScreen -ne "Battle") {
        throw "Hangar contact capture did not stay in battle flow: $Path -> $($Sidecar.flowScreen)"
    }

    if ([int]$Sidecar.activeHostileCount -lt 12 -or [int]$Sidecar.visibleHostileCount -lt 8) {
        throw "Hangar contact capture has too little visible contact pressure: $Path -> active=$($Sidecar.activeHostileCount) visible=$($Sidecar.visibleHostileCount)"
    }

    $occupancy = [string]$Sidecar.occupancy
    foreach ($fragment in @(
        "BattleOccupancy=units",
        "unitRadii infantry=24 vehicle=54 mech=64",
        "structures 1",
        "hardProps ",
        "building=",
        "aircraft=",
        "barricade=",
        "other=",
        "destinationFallback=structure+hardProp",
        "Landing=DemoTerrainView",
        "externalPredicate=water+mapBounds"
    )) {
        if ($occupancy -notlike "*$fragment*") {
            throw "Hangar contact occupancy summary missing '$fragment': $Path -> $occupancy"
        }
    }

    $placeholderSummary = [string]$Sidecar.occupancyPlaceholders
    $landingBlocked = Read-SummaryNumber -Summary $placeholderSummary -Pattern "landingBlockedMarkers ([0-9]+)" -Context $Path
    if ($landingBlocked -le 0) {
        throw "Hangar contact sidecar did not expose landing-blocked markers: $Path -> $placeholderSummary"
    }

    $spread = [string]$Sidecar.contactSpread
    foreach ($fragment in @(
        "ContactSpread=players",
        "hostiles",
        "nearestPH=",
        "nearestHH=",
        "nearestPP=",
        "playerSpan=",
        "hostileSpan=",
        "centroidDistance="
    )) {
        if ($spread -notlike "*$fragment*") {
            throw "Hangar contact spread summary missing '$fragment': $Path -> $spread"
        }
    }

    $nearestPlayerHostile = Read-SummaryNumber -Summary $spread -Pattern "nearestPH=([0-9.]+)" -Context $Path
    $playerSpan = Read-SummaryNumber -Summary $spread -Pattern "playerSpan=([0-9.]+)" -Context $Path
    $hostileSpan = Read-SummaryNumber -Summary $spread -Pattern "hostileSpan=([0-9.]+)" -Context $Path
    if ($nearestPlayerHostile -lt 90.0 -or $playerSpan -lt 100.0 -or $hostileSpan -lt 1000.0) {
        throw "Hangar contact sidecar still reads as a one-point pile: $Path -> $spread"
    }
}

function Test-DamageDemoCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    if ($Sidecar.flowScreen -ne "Battle") {
        throw "Damage demo capture did not stay in battle flow: $Path -> $($Sidecar.flowScreen)"
    }

    if ([double]$Sidecar.camera.zoomScale -lt 1.05) {
        throw "Damage demo capture is too zoomed out for section-damage readability: $Path -> zoom=$($Sidecar.camera.zoomScale)"
    }

    $summary = [string]$Sidecar.damageStory
    foreach ($fragment in @(
        "DamageStory=units",
        "left-arm-lost",
        "legs-lost",
        "cockpit-lost",
        "lostSections=",
        "pilotRisk=",
        "destroyedUnits=",
        "story="
    )) {
        if ($summary -notlike "*$fragment*") {
            throw "Damage demo sidecar summary missing '$fragment': $Path -> $summary"
        }
    }

    foreach ($pattern in @(
        "lostSections=[1-9]",
        "arms=[1-9]",
        "legs=[1-9]",
        "cockpit=[1-9]",
        "pilotRisk=[1-9]"
    )) {
        if ($summary -notmatch $pattern) {
            throw "Damage demo sidecar summary did not prove '$pattern': $Path -> $summary"
        }
    }

    $readability = [string]$Sidecar.damageReadability
    foreach ($fragment in @(
        "DamageReadability=weaponFamilies energy+missile+ballistic+explosive",
        "weaponShapes beam+arc+tracer+shock",
        "hitCues direction+severity+muzzle",
        "sectionConsequences arms-firepower legs-mobility cockpit-ejection wreck-salvage",
        "hud=section-bars+short-labels+sparse",
        "Energy=beam+pillar+muzzle+flash+scorch+direction-core",
        "Missile=arc+blast+salvo-spread+crater+approach-pips",
        "Ballistic=tracer+sparks+muzzle+punch+debris+snap-line",
        "Arms=missing-socket+flag+flight+landing-debris+firepower-marker",
        "Legs=collapse+red-cross+skid+dust+danger-ring+mobility-beacon",
        "Cockpit=breach+ejection-pod+chute+landing+arc+distress+escape-column+route",
        "SectionStatus=bar+short-label+critical+destroyed"
    )) {
        if ($readability -notlike "*$fragment*") {
            throw "Damage demo readability summary missing '$fragment': $Path -> $readability"
        }
    }

    foreach ($pattern in @(
        "story=units [1-9]/",
        "arms=[1-9]",
        "legs=[1-9]",
        "cockpit=[1-9]"
    )) {
        if ($readability -notmatch $pattern) {
            throw "Damage demo readability summary did not prove '$pattern': $Path -> $readability"
        }
    }
}

function Test-SoloOrderCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    if ($Sidecar.flowScreen -ne "Battle") {
        throw "Solo-order capture did not stay in battle flow: $Path -> $($Sidecar.flowScreen)"
    }

    $command = [string]$Sidecar.commandReadability
    foreach ($fragment in @(
        "CommandReadability=all+single+jet+focus+commander-follow+formation",
        "solo=1",
        "statusRows=select-unit+detached-border",
        "SoloOrder=ring+beacon",
        "SoloReturn=ring+beacon",
        "CommandCuePalette=command-blue+target-red+damage-amber+hostile-magenta",
        "CommanderFollow=unit-1+first-sort+fixed-view"
    )) {
        if ($command -notlike "*$fragment*") {
            throw "Solo-order command readability summary missing '$fragment': $Path -> $command"
        }
    }

    $commander = [string]$Sidecar.commanderFollow
    foreach ($fragment in @(
        "CommanderFollow=unit-1+first-sort+fixed-view",
        "unit=unit-1",
        "sortedIndex=1",
        "followOffset=",
        "compositionOffset="
    )) {
        if ($commander -notlike "*$fragment*") {
            throw "Solo-order commander follow summary missing '$fragment': $Path -> $commander"
        }
    }
}

function Test-SoloReturnCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    if ($Sidecar.flowScreen -ne "Battle") {
        throw "Solo-return capture did not stay in battle flow: $Path -> $($Sidecar.flowScreen)"
    }

    $command = [string]$Sidecar.commandReadability
    foreach ($fragment in @(
        "CommandReadability=all+single+jet+focus+commander-follow+formation",
        "solo=0",
        "SoloReturn=ring+beacon",
        "CommanderFollow=unit-1+first-sort+fixed-view"
    )) {
        if ($command -notlike "*$fragment*") {
            throw "Solo-return command readability summary missing '$fragment': $Path -> $command"
        }
    }

    $flow = [string]$Sidecar.playableFlowPolish
    foreach ($fragment in @(
        "PlayableFlowPolish=contact-pressure+damage-debrief+solo-return+hud-density+handoff",
        "soloReturn=returned",
        "detached=0",
        "mobileLandscapeOnly=True",
        "orientation=landscape"
    )) {
        if ($flow -notlike "*$fragment*") {
            throw "Solo-return playable-flow summary missing '$fragment': $Path -> $flow"
        }
    }

    if ([string]$Sidecar.status -notlike "*Solo return settled*") {
        throw "Solo-return capture did not expose settled status: $Path -> $($Sidecar.status)"
    }
}

function Test-MechLabCaptureSidecar {
    param(
        [object]$Sidecar,
        [string]$Path
    )

    if ($Sidecar.flowScreen -ne "Mech Lab") {
        throw "MechLab capture did not open the Mech Lab flow: $Path -> $($Sidecar.flowScreen)"
    }

    $summary = [string]$Sidecar.mechLab
    foreach ($fragment in @(
        "MechLabCapture=open",
        "weaponBlock=",
        "fillers=A+/C+",
        "fit=",
        "pressure=H ",
        " W ",
        "layout=pressure-cards+whole-blocks+single-fillers",
        "alwaysMounted=weapons",
        "noToggle=yes"
    )) {
        if ($summary -notlike "*$fragment*") {
            throw "MechLab sidecar summary missing '$fragment': $Path -> $summary"
        }
    }

    if ($summary -notmatch "\d+x\d+") {
        throw "MechLab sidecar summary missing block shape label: $Path -> $summary"
    }

    foreach ($forbidden in @("toggle", "enable", "disable", "unmount")) {
        if ($summary -match "(?i)(^|[\s=_-])$forbidden($|[\s=_-])") {
            throw "MechLab sidecar summary contains forbidden weapon toggle copy '$forbidden': $Path -> $summary"
        }
    }

    foreach ($forbidden in @("启用", "关闭")) {
        if ($summary.IndexOf($forbidden, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
            throw "MechLab sidecar summary contains forbidden weapon toggle copy '$forbidden': $Path -> $summary"
        }
    }
}

$results = New-Object System.Collections.Generic.List[object]
$expectOccupancyPlaceholders = -not $NoOccupancyPlaceholders.IsPresent
foreach ($normalizedPreset in (Expand-CapturePresets $Presets)) {
    $pngPath = Join-Path $OutputDir "$normalizedPreset.png"
    $jsonPath = Join-Path $OutputDir "$normalizedPreset.json"
    $logPath = Join-Path $OutputDir "$normalizedPreset.log"

    if (-not $SkipRun) {
        foreach ($stalePath in @($pngPath, $jsonPath, $logPath)) {
            if (Test-Path $stalePath) {
                Remove-Item -LiteralPath $stalePath -Force
            }
        }

        $args = @(
            "-screen-width", $Width.ToString([Globalization.CultureInfo]::InvariantCulture),
            "-screen-height", $Height.ToString([Globalization.CultureInfo]::InvariantCulture),
            "-screen-fullscreen", "0",
            "-mc2CapturePreset", $normalizedPreset,
            "-mc2CaptureScreenshot", $pngPath,
            "-mc2CaptureSidecar", $jsonPath,
            "-mc2CaptureQuit",
            "-logFile", $logPath
        )
        if ($expectOccupancyPlaceholders) {
            $args += "-mc2ShowOccupancyReviewLayer"
        }

        $previousOccupancyPlaceholderEnv = $env:MC2_SHOW_OCCUPANCY_PLACEHOLDERS
        try {
            if ($NoOccupancyPlaceholders) {
                Remove-Item Env:\MC2_SHOW_OCCUPANCY_PLACEHOLDERS -ErrorAction SilentlyContinue
            }
            else {
                $env:MC2_SHOW_OCCUPANCY_PLACEHOLDERS = "1"
            }

            try {
                & $gameExe @args
                $exitCodeVariable = Get-Variable -Name LASTEXITCODE -ErrorAction SilentlyContinue
                $exitCode = if ($null -eq $exitCodeVariable) { 0 } else { [int]$exitCodeVariable.Value }
                if ($exitCode -ne 0) {
                    throw "Capture preset '$normalizedPreset' failed with exit code $exitCode. Log: $logPath"
                }

                Wait-CaptureFile -Path $pngPath -TimeoutSeconds $CaptureTimeoutSeconds
                Wait-CaptureFile -Path $jsonPath -TimeoutSeconds $CaptureTimeoutSeconds
            }
            catch {
                Stop-CapturePlayer -ExecutablePath $gameExe
                throw
            }
        }
        finally {
            if ($null -eq $previousOccupancyPlaceholderEnv) {
                Remove-Item Env:\MC2_SHOW_OCCUPANCY_PLACEHOLDERS -ErrorAction SilentlyContinue
            }
            else {
                $env:MC2_SHOW_OCCUPANCY_PLACEHOLDERS = $previousOccupancyPlaceholderEnv
            }
        }

        Stop-CapturePlayer -ExecutablePath $gameExe
    }

    $imageCheck = Test-CaptureImage -Path $pngPath -ExpectedWidth $Width -ExpectedHeight $Height
    $sidecar = Test-CaptureSidecar -Path $jsonPath -ExpectedPreset $normalizedPreset -ExpectOccupancyPlaceholders $expectOccupancyPlaceholders
    $results.Add([pscustomobject]@{
        Preset = $normalizedPreset
        Png = $pngPath
        Json = $jsonPath
        MissionTime = [Math]::Round([double]$sidecar.missionTimeSeconds, 2)
        ActiveHostiles = [int]$sidecar.activeHostileCount
        VisibleHostiles = [int]$sidecar.visibleHostileCount
        UniqueColors = $imageCheck.UniqueColors
        CenterLitRatio = $imageCheck.CenterLitRatio
        MechLab = if ($normalizedPreset -eq "mechlab") { [string]$sidecar.mechLab } else { "" }
    })
}

$results | Format-Table -AutoSize
Write-Host "MC2 reference visual captures passed: $($results.Count) preset(s) -> $OutputDir"
