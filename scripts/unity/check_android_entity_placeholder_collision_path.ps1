param(
    [string]$RepoRoot = "",
    [string]$SidecarDirectory = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

$failures = New-Object System.Collections.Generic.List[string]
$rows = New-Object System.Collections.Generic.List[object]

function Add-Failure {
    param([string]$Message)
    [void]$failures.Add($Message)
}

function Add-Row {
    param(
        [string]$Check,
        [string]$Detail
    )

    [void]$rows.Add([pscustomobject]@{
        Check = $Check
        Status = "OK"
        Detail = $Detail
    })
}

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Read-RequiredText {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "$RelativePath missing"
        return ""
    }

    return Get-Content -LiteralPath $path -Raw
}

function Assert-Contains {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or -not $Text.Contains($Needle)) {
        Add-Failure "$Label missing marker: $Needle"
        return
    }

    Add-Row -Check $Label -Detail $Needle
}

function Assert-NotContains {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if (-not [string]::IsNullOrWhiteSpace($Text) -and $Text.Contains($Needle)) {
        Add-Failure "$Label still contains forbidden marker: $Needle"
        return
    }

    Add-Row -Check "$Label forbidden marker" -Detail $Needle
}

function Assert-Matches {
    param(
        [string]$Text,
        [string]$Pattern,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or $Text -notmatch $Pattern) {
        Add-Failure "$Label missing pattern: $Pattern"
        return
    }

    Add-Row -Check $Label -Detail $Pattern
}

function Assert-SidecarText {
    param(
        [string]$Text,
        [string[]]$Fragments,
        [string]$Label
    )

    foreach ($fragment in $Fragments) {
        if ([string]::IsNullOrWhiteSpace($Text) -or -not $Text.Contains($fragment)) {
            Add-Failure "$Label missing sidecar fragment: $fragment"
            return
        }
    }

    Add-Row -Check $Label -Detail ($Fragments -join "; ")
}

$battleMission = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\BattleMission.cs"
foreach ($marker in @(
    "private const float UnitCollisionInfantryRadius = 24f;",
    "private const float UnitCollisionVehicleRadius = 54f;",
    "private const float UnitCollisionMechRadius = 64f;",
    "private const float UnitCollisionMaxPushPerPass = 42f;",
    "private const int UnitCollisionPasses = 4;",
    "private const float StructureCollisionPadding = 35f;",
    "private const float StructureCollisionMaxPushPerPass = 70f;",
    "private const float TerrainObjectCollisionMaxPushPerPass = 45f;",
    "private readonly List<TerrainObjectObstacle> terrainObjectObstacles = new();",
    "TryCreateTerrainObjectObstacle(spawn, out TerrainObjectObstacle obstacle)",
    "&& !IsCoveredByStructureCenter(obstacle.Position)"
)) {
    Assert-Contains -Text $battleMission -Needle $marker -Label "BattleMission collision constants and inputs"
}

Assert-Matches `
    -Text $battleMission `
    -Pattern "ResolveUnitCollisions\(\);\s+ResolveStructureCollisions\(\);\s+ResolveTerrainObjectCollisions\(\);\s+ResolveUnitCollisions\(\);" `
    -Label "BattleMission per-tick collision order"

foreach ($marker in @(
    "private void ResolveUnitCollisions()",
    "float minimumDistance = UnitCollisionRadius(first) + UnitCollisionRadius(second);",
    "first.ApplyCollisionDisplacement(-direction * push);",
    "second.ApplyCollisionDisplacement(direction * push);",
    "private void ResolveStructureCollisions()",
    "float minimumDistance = StructureCollisionRadius(structure) + UnitCollisionRadius(unit);",
    "bool shiftMoveTarget = IsPointInsideStructureObstacle(unit.MoveTarget, unit, structure);",
    "unit.ApplyCollisionDisplacement(direction * push, shiftMoveTarget);",
    "private void ResolveTerrainObjectCollisions()",
    "float minimumDistance = obstacle.Radius + UnitCollisionRadius(unit);",
    "bool shiftMoveTarget = IsPointInsideTerrainObjectObstacle(unit.MoveTarget, unit, obstacle);",
    "private bool IsPointInsideTerrainObjectObstacle(Vector2 point, UnitState unit, TerrainObjectObstacle obstacle)",
    "private static float StructureCollisionRadius(StructureState structure)",
    "private static bool TryCreateTerrainObjectObstacle(TerrainObjectSpawn spawn, out TerrainObjectObstacle obstacle)",
    "radius = TerrainBuildingObstacleRadius(name);",
    "&& IsHardTerrainTreeObstacle(name)",
    "radius = TerrainHardTreeObstacleRadius(name);",
    "private static float TerrainBuildingObstacleRadius(string name)",
    "private static bool IsHardTerrainTreeObstacle(string name)",
    "private static float TerrainHardTreeObstacleRadius(string name)"
)) {
    Assert-Contains -Text $battleMission -Needle $marker -Label "BattleMission collision solver path"
}

foreach ($marker in @(
    'return "BattleOccupancy=units "',
    '+ " unitRadii infantry="',
    '+ " structures "',
    '+ " hardProps "',
    '+ " building="',
    '+ " aircraft="',
    '+ " barricade="',
    '+ " other="',
    '+ " maxPropRadius="',
    '+ " destinationFallback=structure+hardProp";',
    'return "ContactClearance=players "',
    '+ " overlaps="',
    '+ " status="'
)) {
    Assert-Contains -Text $battleMission -Needle $marker -Label "BattleMission occupancy sidecar summary"
}

foreach ($marker in @(
    "public IEnumerable<BattleOccupancyRegion> OccupancyPlaceholderRegions()",
    "UnitOccupancyPlaceholderKind(unit)",
    "UnitCollisionRadius(unit));",
    "`"structure`",",
    "StructureCollisionRadius(structure));",
    "foreach (TerrainObjectObstacle obstacle in terrainObjectObstacles)",
    "`"hardProp`",",
    "obstacle.Radius);"
)) {
    Assert-Contains -Text $battleMission -Needle $marker -Label "BattleMission occupancy placeholder regions"
}

$bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
foreach ($marker in @(
    'string battleSummary = mission == null ? "BattleOccupancy=mission unavailable" : mission.OccupancySummary();',
    "return battleSummary + `"; `" + DemoTerrainView.CurrentLandingAuditSummary();",
    'SummaryHas(occupancySummary, "BattleOccupancy=units")',
    'SummaryHas(occupancySummary, "unitRadii")',
    'SummaryHas(occupancySummary, "structures ")',
    'SummaryHas(occupancySummary, "hardProps ")',
    'SummaryHas(occupancySummary, "destinationFallback=structure+hardProp")',
    'SummaryHas(contactClearanceSummary, "ContactClearance=players")',
    'SummaryHas(contactClearanceSummary, "overlaps=0")',
    'SummaryHas(contactClearanceSummary, "status=separated")',
    '+ " visualOnly=yes pathing=unchanged collision=unchanged"',
    'return mission == null ? "ContactClearance=mission unavailable" : mission.ContactClearanceSummary();',
    "foreach (BattleOccupancyRegion region in mission.OccupancyPlaceholderRegions())",
    'GameObject marker = DemoPrimitiveVisualFactory.Create(PrimitiveType.Cylinder, "Occupancy " + region.Kind + " " + region.Id);',
    "float diameter = Mathf.Clamp(region.Radius / 50f, 0.26f, 5.2f);",
    "List<Vector2> blockedMarkers = DemoTerrainView.CurrentLandingReviewBlockedMarkers(OccupancyLandingReviewMarkerCount);",
    'GameObject marker = DemoPrimitiveVisualFactory.Create(PrimitiveType.Cube, "Occupancy landingBlocked " + index.ToString(CultureInfo.InvariantCulture));',
    '" source=BattleMission.OccupancyPlaceholderRegions+DemoTerrainView.LandingReviewBlockedMarkers"',
    'GameObject prop = DemoPrimitiveVisualFactory.Create(primitive, terrainObject.objectId + " " + terrainObject.fileName);'
)) {
    Assert-Contains -Text $bootstrap -Needle $marker -Label "Mc2DemoBootstrap occupancy capture path"
}

$factory = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\DemoPrimitiveVisualFactory.cs"
foreach ($marker in @(
    "GameObject visual = new(objectName);",
    "AddComponent<MeshFilter>",
    "AddComponent<MeshRenderer>",
    "Resources.GetBuiltinResource<Mesh>"
)) {
    Assert-Contains -Text $factory -Needle $marker -Label "visual primitive factory"
}
Assert-NotContains -Text $factory -Needle "AddComponent<Collider>" -Label "visual primitive factory"
Assert-NotContains -Text $factory -Needle "GameObject.CreatePrimitive" -Label "visual primitive factory"

$captureScript = Read-RequiredText -RelativePath "scripts\unity\capture_reference_visuals.ps1"
foreach ($marker in @(
    "Test-BattleOccupancyCaptureSidecar -Sidecar `$sidecar -Path `$Path",
    "Test-ContactClearanceCaptureSidecar -Sidecar `$sidecar -Path `$Path",
    "Test-FirstMapVisualCaptureSidecar -Sidecar `$sidecar -Path `$Path",
    '"BattleOccupancy=units"',
    '"unitRadii infantry=24 vehicle=54 mech=64"',
    '"structures "',
    '"hardProps "',
    '"destinationFallback=structure+hardProp"',
    '"Landing=DemoTerrainView"',
    '"externalPredicate=water+mapBounds"',
    '"ContactClearance=players"',
    '"overlaps="',
    '"status=separated"',
    'if ($overlaps -ne 0)',
    '"OccupancyPlaceholders=enabled"',
    '" landingBlockedMarkers "',
    '"source=BattleMission.OccupancyPlaceholderRegions+DemoTerrainView.LandingReviewBlockedMarkers"',
    '"visualOnly=yes"',
    '"pathing=unchanged"',
    '"collision=unchanged"'
)) {
    Assert-Contains -Text $captureScript -Needle $marker -Label "reference visual sidecar gate"
}

$combatNoiseScript = Read-RequiredText -RelativePath "scripts\unity\check_android_combat_effect_log_noise.ps1"
foreach ($marker in @(
    "DemoPrimitiveVisualFactory.Create(PrimitiveType.Cylinder, `"Occupancy `" + region.Kind + `" `" + region.Id)",
    "class 'CapsuleCollider' doesn't exist",
    "class 'SphereCollider' doesn't exist",
    "class 'BoxCollider' doesn't exist",
    "Android combat effect log noise check OK."
)) {
    Assert-Contains -Text $combatNoiseScript -Needle $marker -Label "Android collider-log guard"
}

if (-not [string]::IsNullOrWhiteSpace($SidecarDirectory)) {
    $resolvedSidecarDirectory = (Resolve-Path -LiteralPath $SidecarDirectory).Path
    $sidecars = @(Get-ChildItem -LiteralPath $resolvedSidecarDirectory -Filter *.json -File -ErrorAction Stop)
    if ($sidecars.Count -eq 0) {
        Add-Failure "No sidecar JSON files found in $resolvedSidecarDirectory"
    }

    foreach ($sidecar in $sidecars) {
        $text = Get-Content -LiteralPath $sidecar.FullName -Raw -Encoding UTF8
        Assert-SidecarText `
            -Text $text `
            -Fragments @(
                "BattleOccupancy=units",
                "unitRadii infantry=24 vehicle=54 mech=64",
                "destinationFallback=structure+hardProp",
                "ContactClearance=players",
                "overlaps=0",
                "status=separated",
                "OccupancyPlaceholders=enabled",
                "source=BattleMission.OccupancyPlaceholderRegions+DemoTerrainView.LandingReviewBlockedMarkers",
                "visualOnly=yes pathing=unchanged collision=unchanged"
            ) `
            -Label "sidecar $($sidecar.Name)"
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Android entity placeholder collision path check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android entity placeholder collision path check(s) failed."
}

Write-Host "Android entity placeholder collision path check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
