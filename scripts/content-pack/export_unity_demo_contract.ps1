param(
    [Parameter(Mandatory = $false)]
    [string]$MissionAnalysisPath = "",

    [Parameter(Mandatory = $false)]
    [string]$OutputRoot = "",

    [Parameter(Mandatory = $false)]
    [string]$UnityProjectPath = "",

    [Parameter(Mandatory = $false)]
    [string]$TerrainObjectPacketPath = "",

    [Parameter(Mandatory = $false)]
    [string]$TerrainVertexPacketPath = "",

    [Parameter(Mandatory = $false)]
    [string]$BuildingCatalogPath = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

if ([string]::IsNullOrWhiteSpace($MissionAnalysisPath)) {
    $MissionAnalysisPath = Join-Path $repoRoot "analysis-output\mission-analysis\project-owned-linked-dev\mc2_01\mission-analysis.json"
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "analysis-output\unity-demo-contract"
}

if ([string]::IsNullOrWhiteSpace($UnityProjectPath)) {
    $UnityProjectPath = Join-Path $repoRoot "unity-mc2-demo"
}

if ([string]::IsNullOrWhiteSpace($TerrainObjectPacketPath)) {
    $packetCandidates = @(
        (Join-Path $repoRoot "analysis-output\pak-unpack\mc2_01-fixed-safe\packet_0001.bin"),
        (Join-Path $repoRoot "analysis-output\pak-unpack\mc2_01.pak-safe\packet_0001.bin")
    )
    foreach ($candidate in $packetCandidates) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            $TerrainObjectPacketPath = $candidate
            break
        }
    }
}

if ([string]::IsNullOrWhiteSpace($TerrainVertexPacketPath)) {
    $vertexPacketCandidates = @(
        (Join-Path $repoRoot "analysis-output\pak-unpack\mc2_01-fixed-safe\packet_0000.bin"),
        (Join-Path $repoRoot "analysis-output\pak-unpack\mc2_01.pak-safe\packet_0000.bin")
    )
    foreach ($candidate in $vertexPacketCandidates) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            $TerrainVertexPacketPath = $candidate
            break
        }
    }
}

if ([string]::IsNullOrWhiteSpace($BuildingCatalogPath)) {
    $BuildingCatalogPath = Join-Path $repoRoot "analysis-output\fst-unpack\art.fst\data\art\buildings.csv"
}

function Get-Value {
    param(
        [Parameter(Mandatory = $false)]
        $Object,

        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $false)]
        $Default = $null
    )

    if ($null -eq $Object) {
        return $Default
    }

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $Default
    }

    return $property.Value
}

function Convert-Condition {
    param(
        [Parameter(Mandatory = $true)]
        $Condition
    )

    $values = $Condition.values
    $kind = Get-Value -Object $values -Name "ConditionSpeciesString" -Default "Unknown"
    $result = [ordered]@{
        type = $kind
        sourceIndex = $Condition.index
    }

    if ($kind -like "Move*Area") {
        $result.targetArea = [ordered]@{
            x = Get-Value -Object $values -Name "TargetCenterX"
            y = Get-Value -Object $values -Name "TargetCenterY"
            radius = Get-Value -Object $values -Name "TargetRadius"
        }
    }
    elseif ($kind -eq "MoveAllSurvivingMechsToArea") {
        $result.targetArea = [ordered]@{
            x = Get-Value -Object $values -Name "TargetCenterX"
            y = Get-Value -Object $values -Name "TargetCenterY"
            radius = Get-Value -Object $values -Name "TargetRadius"
        }
    }
    elseif ($kind -eq "DestroySpecificEnemyUnit") {
        $result.targetUnit = [ordered]@{
            commander = Get-Value -Object $values -Name "Commander"
            group = Get-Value -Object $values -Name "Group"
            mate = Get-Value -Object $values -Name "Mate"
            position = [ordered]@{
                x = Get-Value -Object $values -Name "PositionX"
                y = Get-Value -Object $values -Name "PositionY"
            }
            cell = [ordered]@{
                x = Get-Value -Object $values -Name "CellX"
                y = Get-Value -Object $values -Name "CellY"
            }
        }
    }
    elseif ($kind -eq "DestroySpecificStructure") {
        $result.targetStructure = [ordered]@{
            commander = Get-Value -Object $values -Name "Commander"
            position = [ordered]@{
                x = Get-Value -Object $values -Name "PositionX"
                y = Get-Value -Object $values -Name "PositionY"
            }
            cell = [ordered]@{
                x = Get-Value -Object $values -Name "CellX"
                y = Get-Value -Object $values -Name "CellY"
            }
        }
    }

    return $result
}

function Convert-Action {
    param(
        [Parameter(Mandatory = $true)]
        $Action
    )

    $values = $Action.values
    $kind = Get-Value -Object $values -Name "ActionSpeciesString" -Default "Unknown"
    $result = [ordered]@{
        type = $kind
        sourceIndex = $Action.index
    }

    if ($kind -eq "SetBooleanFlag") {
        $result.flag = [ordered]@{
            id = Get-Value -Object $values -Name "FlagID"
            value = Get-Value -Object $values -Name "Value"
        }
    }

    return $result
}

function Get-MissionUnitActivation {
    param(
        [Parameter(Mandatory = $true)]
        $Unit,

        [Parameter(Mandatory = $true)]
        [string]$MissionId
    )

    $rule = [ordered]@{
        activationFlagId = ""
        activationObjectiveIndex = -1
    }

    if ($MissionId -ne "mc2_01" -or $Unit.playerPart) {
        return $rule
    }

    $brain = [string]$Unit.brain
    $x = [double]$Unit.position.x
    if ($brain -like "mc2_01_Pat1*") {
        $rule.activationFlagId = "0"
    }
    elseif ($brain -eq "mc2_01_LRMs" -and $x -gt 0) {
        $rule.activationFlagId = "0"
    }
    elseif ($brain -like "mc2_01_Pat2*" -or $brain -eq "mc2_01_Pat4") {
        $rule.activationFlagId = "0"
    }
    elseif ($brain -eq "mc2_01_Starslayer" -or $brain -eq "mc2_01_Urbies" -or ($brain -eq "mc2_01_LRMs" -and $x -lt 0)) {
        $rule.activationObjectiveIndex = 7
    }

    return $rule
}

function Read-BuildingCatalog {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $catalog = @{}
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $catalog
    }

    foreach ($row in (Import-Csv -LiteralPath $Path)) {
        if ([string]::IsNullOrWhiteSpace($row.FitID) -or $row.FitID -notmatch "^-?\d+$") {
            continue
        }

        $fitId = [int]$row.FitID
        $catalog[$fitId] = [ordered]@{
            fileName = $row.'File Name'
            groupId = if ($row.'Group ID' -match "^-?\d+$") { [int]$row.'Group ID' } else { -1 }
            nameId = if ($row.NameID -match "^-?\d+$") { [int]$row.NameID } else { -1 }
            objectClass = $row.Type
            fitId = $fitId
            specialType = $row.'SPECIAL TYPE'
            alignment = $row.'ALIGNMENT?'
            capturable = $row.CAPTURABLE
            drawOnTacMap = $row.'Draw Building On TacMap'
            textureName = $row.'TGA Filename (if any)'
        }
    }

    return $catalog
}

function Read-Int32LE {
    param([byte[]]$Bytes, [int]$Offset)
    return [BitConverter]::ToInt32($Bytes, $Offset)
}

function Read-SingleLE {
    param([byte[]]$Bytes, [int]$Offset)
    return [BitConverter]::ToSingle($Bytes, $Offset)
}

function Get-ReferenceAssetId {
    param([string]$Name)

    if ([string]::IsNullOrWhiteSpace($Name)) {
        return ""
    }

    return [System.IO.Path]::GetFileNameWithoutExtension($Name).Trim().ToLowerInvariant()
}

function Get-DistanceSquared {
    param(
        [double]$X1,
        [double]$Y1,
        [double]$X2,
        [double]$Y2
    )

    $dx = $X1 - $X2
    $dy = $Y1 - $Y2
    return ($dx * $dx) + ($dy * $dy)
}

function Find-MatchingTerrainBuilding {
    param(
        [array]$TerrainObjects,
        [double]$X,
        [double]$Y
    )

    return $TerrainObjects |
        Where-Object {
            $_.position -ne $null -and
            $_.objectClass -eq "BUILDING" -and
            (Get-DistanceSquared -X1 $_.position.x -Y1 $_.position.y -X2 $X -Y2 $Y) -lt (55 * 55)
        } |
        Sort-Object { Get-DistanceSquared -X1 $_.position.x -Y1 $_.position.y -X2 $X -Y2 $Y } |
        Select-Object -First 1
}

function Read-TerrainObjects {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PacketPath,

        [Parameter(Mandatory = $true)]
        [hashtable]$Catalog
    )

    if ([string]::IsNullOrWhiteSpace($PacketPath) -or -not (Test-Path -LiteralPath $PacketPath -PathType Leaf)) {
        return @()
    }

    $resolvedPacketPath = (Resolve-Path -LiteralPath $PacketPath).Path
    $bytes = [IO.File]::ReadAllBytes($resolvedPacketPath)
    if ($bytes.Length -lt 4) {
        throw "Terrain object packet is too small: $resolvedPacketPath"
    }

    $count = Read-Int32LE -Bytes $bytes -Offset 0
    $expectedBytes = 4 + ($count * 40)
    if ($count -lt 0 -or $expectedBytes -gt $bytes.Length) {
        throw "Terrain object packet has invalid count $count in $resolvedPacketPath"
    }

    $objects = @()
    for ($index = 0; $index -lt $count; $index++) {
        $offset = 4 + ($index * 40)
        $fitId = Read-Int32LE -Bytes $bytes -Offset $offset
        $info = $Catalog[$fitId]

        $objects += [ordered]@{
            objectId = "terrain-object-$index"
            sourceIndex = $index
            fitId = $fitId
            fileName = if ($info) { $info.fileName } else { "object-$fitId" }
            assetId = if ($info) { Get-ReferenceAssetId -Name $info.fileName } else { Get-ReferenceAssetId -Name "object-$fitId" }
            objectClass = if ($info) { $info.objectClass } else { "UNKNOWN" }
            specialType = if ($info) { $info.specialType } else { "" }
            textureName = if ($info) { $info.textureName } else { "" }
            teamId = Read-Int32LE -Bytes $bytes -Offset ($offset + 24)
            parentId = Read-Int32LE -Bytes $bytes -Offset ($offset + 28)
            damage = Read-Int32LE -Bytes $bytes -Offset ($offset + 20)
            position = [ordered]@{
                x = Read-SingleLE -Bytes $bytes -Offset ($offset + 4)
                y = Read-SingleLE -Bytes $bytes -Offset ($offset + 8)
                z = Read-SingleLE -Bytes $bytes -Offset ($offset + 12)
                rotation = Read-SingleLE -Bytes $bytes -Offset ($offset + 16)
            }
        }
    }

    return $objects
}

function Read-TerrainMesh {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PacketPath,

        [Parameter(Mandatory = $true)]
        $Analysis
    )

    if ([string]::IsNullOrWhiteSpace($PacketPath) -or -not (Test-Path -LiteralPath $PacketPath -PathType Leaf)) {
        return $null
    }

    $resolvedPacketPath = (Resolve-Path -LiteralPath $PacketPath).Path
    $bytes = [IO.File]::ReadAllBytes($resolvedPacketPath)
    $vertexSize = 32
    if (($bytes.Length % $vertexSize) -ne 0) {
        throw "Terrain vertex packet size is not divisible by $vertexSize`: $resolvedPacketPath"
    }

    $vertexCount = [int]($bytes.Length / $vertexSize)
    $sourceSide = [int][Math]::Sqrt($vertexCount)
    if (($sourceSide * $sourceSide) -ne $vertexCount) {
        throw "Terrain vertex packet does not contain a square vertex grid: $vertexCount vertices"
    }

    $samples = @()
    $elevationMin = [double]::PositiveInfinity
    $elevationMax = [double]::NegativeInfinity
    $terrainTypeCounts = @{}
    $waterCounts = @{}

    for ($index = 0; $index -lt $vertexCount; $index++) {
        $offset = $index * $vertexSize
        $elevation = Read-SingleLE -Bytes $bytes -Offset ($offset + 12)
        $textureData = [BitConverter]::ToUInt32($bytes, $offset + 16)
        $textureId = [int]($textureData -band 0xffff)
        $textureSet = [int](($textureData -shr 16) -band 0xffff)
        $light = [BitConverter]::ToUInt32($bytes, $offset + 20)
        $terrainType = [BitConverter]::ToUInt32($bytes, $offset + 24)
        $water = [int]$bytes[$offset + 29]

        if ($elevation -lt $elevationMin) {
            $elevationMin = $elevation
        }
        if ($elevation -gt $elevationMax) {
            $elevationMax = $elevation
        }

        $terrainKey = [string]$terrainType
        if (-not $terrainTypeCounts.ContainsKey($terrainKey)) {
            $terrainTypeCounts[$terrainKey] = 0
        }
        $terrainTypeCounts[$terrainKey]++

        $waterKey = [string]$water
        if (-not $waterCounts.ContainsKey($waterKey)) {
            $waterCounts[$waterKey] = 0
        }
        $waterCounts[$waterKey]++

        $samples += [ordered]@{
            elevation = $elevation
            terrainType = [int]$terrainType
            water = $water
            textureData = [int64]$textureData
            textureId = $textureId
            textureSet = $textureSet
            light = [int64]$light
        }
    }

    return [ordered]@{
        sourcePacket = $resolvedPacketPath
        sourceSide = $sourceSide
        sampleSide = $sourceSide
        sampleStep = 1
        worldUnitsPerVertex = 128
        minX = [float]$Analysis.terrain.minX
        minY = [float]$Analysis.terrain.minY
        elevationMin = [float]$elevationMin
        elevationMax = [float]$elevationMax
        terrainTypeCounts = $terrainTypeCounts
        waterCounts = $waterCounts
        samples = $samples
    }
}

$analysisFile = (Resolve-Path -LiteralPath $MissionAnalysisPath).Path
$analysis = Get-Content -LiteralPath $analysisFile -Raw | ConvertFrom-Json
$packId = $analysis.pack.id
$missionId = $analysis.missionId
$buildingCatalog = Read-BuildingCatalog -Path $BuildingCatalogPath
$terrainMesh = Read-TerrainMesh -PacketPath $TerrainVertexPacketPath -Analysis $analysis
$terrainObjects = @(Read-TerrainObjects -PacketPath $TerrainObjectPacketPath -Catalog $buildingCatalog)

$units = @($analysis.units | ForEach-Object {
    $activation = Get-MissionUnitActivation -Unit $_ -MissionId $MissionId
    [ordered]@{
        spawnId = "unit-$($_.index)"
        sourceIndex = $_.index
        teamId = $_.teamId
        commanderId = $_.commanderId
        pilotId = $_.pilot
        isPlayerUnit = $_.playerPart
        unitType = $_.csvFile
        objectProfile = $_.objectProfile
        objectNumber = $_.objectNumber
        variantNumber = $_.variantNumber
        brain = $_.brain
        squadId = $_.squadNum
        activationFlagId = $activation.activationFlagId
        activateOnObjective = $activation.activationObjectiveIndex -ge 0
        activationObjectiveIndex = $activation.activationObjectiveIndex
        position = [ordered]@{
            x = $_.position.x
            y = $_.position.y
            rotation = $_.position.rotation
        }
    }
})

$objectives = @($analysis.objectives | ForEach-Object {
    [ordered]@{
        id = "objective-$($_.index)"
        index = $_.index
        team = $_.team
        title = $_.title
        titleResourceStringId = $_.titleResourceStringId
        hidden = $_.hiddenTrigger
        activateOnFlag = $_.activateOnFlag
        activateFlagId = $_.activateFlagId
        requiresAllPreviousPrimary = $_.allPreviousPrimaryObjectivesMustBeComplete
        displayMarker = $_.displayMarker
        marker = $_.marker
        rewardResourcePoints = $_.resourcePoints
        conditions = @($_.conditions | Where-Object { $null -ne $_ } | ForEach-Object { Convert-Condition -Condition $_ })
        actions = @($_.actions | Where-Object { $null -ne $_ } | ForEach-Object { Convert-Action -Action $_ })
    }
})

$brainNames = @($units |
    Where-Object { -not $_.isPlayerUnit -and -not [string]::IsNullOrWhiteSpace($_.brain) } |
    ForEach-Object { $_.brain } |
    Sort-Object -Unique)

$staticObjects = @()
foreach ($objective in $objectives) {
    foreach ($condition in @($objective.conditions)) {
        if ($condition.type -ne "DestroySpecificStructure" -or $null -eq $condition.targetStructure -or $null -eq $condition.targetStructure.position) {
            continue
        }

        $objectType = if ($objective.title -match "(?i)hangar") { "Hangar" } else { "Structure" }
        $matchedTerrainObject = Find-MatchingTerrainBuilding `
            -TerrainObjects $terrainObjects `
            -X $condition.targetStructure.position.x `
            -Y $condition.targetStructure.position.y
        $objectProfile = if ($null -ne $matchedTerrainObject -and -not [string]::IsNullOrWhiteSpace($matchedTerrainObject.fileName)) {
            $matchedTerrainObject.fileName
        }
        else {
            $objectType
        }
        $objectRotation = if ($null -ne $matchedTerrainObject -and $null -ne $matchedTerrainObject.position) {
            [float]$matchedTerrainObject.position.rotation
        }
        else {
            0
        }

        $staticObjects += [ordered]@{
            objectId = "structure-$($objective.index)-$($condition.sourceIndex)"
            source = "objective-target"
            objectiveIndex = $objective.index
            objectType = $objectType
            objectProfile = $objectProfile
            assetId = Get-ReferenceAssetId -Name $objectProfile
            teamId = $condition.targetStructure.commander
            targetable = $true
            objectiveTarget = $true
            position = [ordered]@{
                x = $condition.targetStructure.position.x
                y = $condition.targetStructure.position.y
                rotation = $objectRotation
            }
            radius = 180
            maxStructure = 320
        }
    }
}

$contract = [ordered]@{
    schema = "mc2-unity-demo-contract-v1"
    createdAt = (Get-Date).ToString("o")
    source = [ordered]@{
        analysis = $analysisFile
        packId = $packId
        missionId = $missionId
        files = $analysis.files
        terrainVertexPacket = if ([string]::IsNullOrWhiteSpace($TerrainVertexPacketPath)) { $null } else { $TerrainVertexPacketPath }
        terrainObjectPacket = if ([string]::IsNullOrWhiteSpace($TerrainObjectPacketPath)) { $null } else { $TerrainObjectPacketPath }
        buildingCatalog = if (Test-Path -LiteralPath $BuildingCatalogPath -PathType Leaf) { $BuildingCatalogPath } else { $null }
    }
    mission = [ordered]@{
        id = $missionId
        displayName = $analysis.mission.name
        author = $analysis.mission.author
        scriptName = $analysis.mission.scenarioScript
        limits = [ordered]@{
            timeSeconds = $analysis.mission.timeLimit
            dropWeight = $analysis.mission.dropWeightLimit
            resourcePoints = $analysis.mission.resourcePoints
            maxTeams = $analysis.mission.maximumNumberOfTeams
            maxPlayers = $analysis.mission.maximumNumberOfPlayers
        }
        camera = $analysis.camera
        terrain = $analysis.terrain
    }
    units = $units
    objectives = $objectives
    objectiveEdges = $analysis.objectiveEdges
    staticObjects = $staticObjects
    terrainMesh = $terrainMesh
    terrainObjects = $terrainObjects
    navMarkers = $analysis.navMarkers
    forests = $analysis.forests
    aiHooks = [ordered]@{
        enemyBrains = $brainNames
        scriptSignals = @($analysis.script.signals | ForEach-Object { $_.name })
        voiceLines = $analysis.script.voiceLines
    }
    battleCoreBoundary = [ordered]@{
        owns = @(
            "unit state",
            "movement commands",
            "attack target selection",
            "damage and destruction events",
            "objective condition evaluation",
            "objective action dispatch",
            "mission result"
        )
        unityPresentationOwns = @(
            "terrain and prop rendering",
            "unit visuals and effects",
            "camera follow and zoom",
            "input translation",
            "status bar UI",
            "audio and voice-over playback"
        )
    }
}

$outputDir = Join-Path (Join-Path $OutputRoot $packId) $missionId
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
$contractPath = Join-Path $outputDir "mission-contract.json"
$contractJson = $contract | ConvertTo-Json -Depth 20
$contractJson | Set-Content -LiteralPath $contractPath -Encoding UTF8

$unityStreamingPath = Join-Path $UnityProjectPath "Assets\StreamingAssets\Missions\$missionId"
if (Test-Path -LiteralPath $UnityProjectPath -PathType Container) {
    New-Item -ItemType Directory -Path $unityStreamingPath -Force | Out-Null
    $unityContractPath = Join-Path $unityStreamingPath "mission-contract.json"
    $contractJson | Set-Content -LiteralPath $unityContractPath -Encoding UTF8
}
else {
    $unityContractPath = $null
}

Write-Output "Unity demo contract exported: $missionId"
Write-Output "JSON: $contractPath"
if ($unityContractPath) {
    Write-Output "Unity JSON: $unityContractPath"
}
Write-Output ("Units: {0}" -f $units.Count)
Write-Output ("Objectives: {0}" -f $objectives.Count)
Write-Output ("Static objects: {0}" -f $staticObjects.Count)
Write-Output ("Terrain mesh samples: {0}" -f $(if ($terrainMesh) { $terrainMesh.samples.Count } else { 0 }))
Write-Output ("Terrain objects: {0}" -f $terrainObjects.Count)
