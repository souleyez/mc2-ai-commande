param(
    [Parameter(Mandatory = $false)]
    [string]$MissionExtractPath = "",

    [Parameter(Mandatory = $false)]
    [string]$OutputRoot = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

if ([string]::IsNullOrWhiteSpace($MissionExtractPath)) {
    $MissionExtractPath = Join-Path $repoRoot "analysis-output\mission-extract\project-owned-linked-dev\mc2_01"
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "analysis-output\mission-analysis"
}

function ConvertTo-RelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,

        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $rootWithSlash = $Root.TrimEnd("\") + "\"
    return $Path.Substring($rootWithSlash.Length).Replace("\", "/")
}

function ConvertFrom-FitScalar {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RawValue
    )

    $value = $RawValue.Trim()
    if ($value.EndsWith(",")) {
        $parts = $value.Split(",") |
            ForEach-Object { $_.Trim() } |
            Where-Object { $_ -ne "" }
        return @($parts | ForEach-Object { ConvertFrom-FitScalar -RawValue $_ })
    }

    if ($value.StartsWith('"') -and $value.EndsWith('"')) {
        return $value.Substring(1, $value.Length - 2)
    }

    if ($value -eq "TRUE") {
        return $true
    }

    if ($value -eq "FALSE") {
        return $false
    }

    $integerValue = 0L
    if ([int64]::TryParse($value, [ref]$integerValue)) {
        return $integerValue
    }

    $doubleValue = 0.0
    if ([double]::TryParse(
            $value,
            [System.Globalization.NumberStyles]::Float,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [ref]$doubleValue)) {
        return $doubleValue
    }

    return $value
}

function Read-FitFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $sections = [ordered]@{}
    $current = $null
    $lines = Get-Content -LiteralPath $Path
    for ($index = 0; $index -lt $lines.Count; $index++) {
        $line = $lines[$index].Trim()
        if ($line -eq "") {
            continue
        }

        if ($line -match '^\[(?<name>[^\]]+)\]$') {
            $sectionName = $Matches.name
            $current = [ordered]@{
                name = $sectionName
                line = $index + 1
                values = [ordered]@{}
            }
            $sections[$sectionName] = $current
            continue
        }

        if ($null -eq $current) {
            continue
        }

        if ($line -match '^(?<type>[A-Za-z]+(?:\[\d+\])?)\s+(?<key>.+?)\s*=\s*(?<value>.*)$') {
            $key = $Matches.key.Trim()
            $current.values[$key] = ConvertFrom-FitScalar -RawValue $Matches.value
        }
    }

    return $sections
}

function Get-FitValue {
    param(
        [Parameter(Mandatory = $false)]
        $Section,

        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $false)]
        $Default = $null
    )

    if ($null -ne $Section -and $Section.values.Contains($Name)) {
        return $Section.values[$Name]
    }

    return $Default
}

function ConvertTo-CountMap {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Rows,

        [Parameter(Mandatory = $true)]
        [string]$Property
    )

    $map = [ordered]@{}
    foreach ($row in $Rows) {
        $key = [string]$row[$Property]
        if ([string]::IsNullOrWhiteSpace($key)) {
            $key = "(none)"
        }

        if (-not $map.Contains($key)) {
            $map[$key] = 0
        }
        $map[$key]++
    }

    return $map
}

function Get-ObjectiveChildren {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Sections,

        [Parameter(Mandatory = $true)]
        [int]$Team,

        [Parameter(Mandatory = $true)]
        [int]$Objective,

        [Parameter(Mandatory = $true)]
        [string]$Kind
    )

    $children = @()
    $pattern = "^Team$Team" + "Objective$Objective" + "$Kind(\d+)$"
    foreach ($sectionName in $Sections.Keys) {
        if ($sectionName -match $pattern) {
            $children += [ordered]@{
                index = [int]$Matches[1]
                values = $Sections[$sectionName].values
            }
        }
    }

    return @($children | Sort-Object index)
}

function Convert-UnitForMarkdown {
    param($Unit)

    return ("| {0} | {1} | {2} | {3} | {4} | {5},{6} | {7} |" -f
        $Unit.index,
        $Unit.teamId,
        $Unit.csvFile,
        $Unit.brain,
        $Unit.playerPart,
        [math]::Round([double]$Unit.position.x, 1),
        [math]::Round([double]$Unit.position.y, 1),
        $Unit.squadNum)
}

$missionExtractRoot = (Resolve-Path -LiteralPath $MissionExtractPath).Path
$extractManifestPath = Join-Path $missionExtractRoot "mission-extract.json"
if (-not (Test-Path -LiteralPath $extractManifestPath -PathType Leaf)) {
    throw "Mission extract manifest is missing: $extractManifestPath"
}

$extractManifest = Get-Content -LiteralPath $extractManifestPath -Raw | ConvertFrom-Json
$fitFile = $extractManifest.files | Where-Object { $_.relativePath -like "*.fit" } | Select-Object -First 1
$ablFile = $extractManifest.files | Where-Object { $_.relativePath -like "*.abl" } | Select-Object -First 1
$pakFile = $extractManifest.files | Where-Object { $_.relativePath -like "*.pak" } | Select-Object -First 1

if (-not $fitFile -or -not (Test-Path -LiteralPath $fitFile.output -PathType Leaf)) {
    throw "FIT file is missing from extract: $missionExtractRoot"
}

if (-not $ablFile -or -not (Test-Path -LiteralPath $ablFile.output -PathType Leaf)) {
    throw "ABL file is missing from extract: $missionExtractRoot"
}

$sections = Read-FitFile -Path $fitFile.output
$missionId = $extractManifest.missionId
$packId = $extractManifest.pack.id
$missionSettings = $sections["MissionSettings"]
$camera = $sections["Cameras"]
$terrain = $sections["Terrain"]
$water = $sections["Water"]
$script = $sections["Script"]

$numWarriors = [int](Get-FitValue -Section $sections["Warriors"] -Name "NumWarriors" -Default 0)
$numParts = [int](Get-FitValue -Section $sections["Parts"] -Name "NumParts" -Default 0)
$units = @()
for ($unitIndex = 1; $unitIndex -le [math]::Max($numWarriors, $numParts); $unitIndex++) {
    $warrior = $sections["Warrior$unitIndex"]
    $part = $sections["Part$unitIndex"]
    if ($null -eq $warrior -and $null -eq $part) {
        continue
    }

    $units += [ordered]@{
        index = $unitIndex
        profile = Get-FitValue -Section $warrior -Name "Profile" -Default ""
        brain = Get-FitValue -Section $warrior -Name "Brain" -Default ""
        numCells = Get-FitValue -Section $warrior -Name "NumCells" -Default 0
        objectNumber = Get-FitValue -Section $part -Name "ObjectNumber" -Default $null
        objectProfile = Get-FitValue -Section $part -Name "ObjectProfile" -Default ""
        csvFile = Get-FitValue -Section $part -Name "CSVFile" -Default ""
        variantNumber = Get-FitValue -Section $part -Name "VariantNumber" -Default $null
        squadNum = Get-FitValue -Section $part -Name "SquadNum" -Default $null
        playerPart = Get-FitValue -Section $part -Name "PlayerPart" -Default $false
        teamId = Get-FitValue -Section $part -Name "TeamID" -Default $null
        commanderId = Get-FitValue -Section $part -Name "CommanderID" -Default $null
        pilot = Get-FitValue -Section $part -Name "Pilot" -Default $null
        controlType = Get-FitValue -Section $part -Name "ControlType" -Default $null
        active = Get-FitValue -Section $part -Name "Active" -Default $null
        exists = Get-FitValue -Section $part -Name "Exists" -Default $null
        position = [ordered]@{
            x = Get-FitValue -Section $part -Name "PositionX" -Default $null
            y = Get-FitValue -Section $part -Name "PositionY" -Default $null
            rotation = Get-FitValue -Section $part -Name "Rotation" -Default $null
        }
    }
}

$objectives = @()
foreach ($sectionName in $sections.Keys) {
    if ($sectionName -match '^Team(?<team>\d+)Objective(?<objective>\d+)$') {
        $team = [int]$Matches.team
        $objective = [int]$Matches.objective
        $section = $sections[$sectionName]
        $objectives += [ordered]@{
            team = $team
            index = $objective
            title = Get-FitValue -Section $section -Name "Title" -Default ""
            description = Get-FitValue -Section $section -Name "Description" -Default ""
            titleResourceStringId = Get-FitValue -Section $section -Name "TitleResourceStringID" -Default $null
            hiddenTrigger = Get-FitValue -Section $section -Name "HiddenTrigger" -Default $false
            activateOnFlag = Get-FitValue -Section $section -Name "ActivateOnFlag" -Default $false
            activateFlagId = Get-FitValue -Section $section -Name "ActivateFlagID" -Default ""
            allPreviousPrimaryObjectivesMustBeComplete = Get-FitValue -Section $section -Name "AllPreviousPrimaryObjectivesMustBeComplete" -Default $false
            displayMarker = Get-FitValue -Section $section -Name "DisplayMarker" -Default $false
            marker = [ordered]@{
                x = Get-FitValue -Section $section -Name "MarkerX" -Default $null
                y = Get-FitValue -Section $section -Name "MarkerY" -Default $null
            }
            resourcePoints = Get-FitValue -Section $section -Name "ResourcePoints" -Default 0
            conditions = Get-ObjectiveChildren -Sections $sections -Team $team -Objective $objective -Kind "Condition"
            actions = Get-ObjectiveChildren -Sections $sections -Team $team -Objective $objective -Kind "Action"
        }
    }
}
$objectives = @($objectives | Sort-Object team, index)

$objectiveEdges = @()
foreach ($source in $objectives) {
    foreach ($action in $source.actions) {
        if (([string](Get-FitValue -Section @{ values = $action.values } -Name "ActionSpeciesString" -Default "")) -ne "SetBooleanFlag") {
            continue
        }

        $flagId = [string](Get-FitValue -Section @{ values = $action.values } -Name "FlagID" -Default "")
        if ([string]::IsNullOrWhiteSpace($flagId)) {
            continue
        }

        foreach ($target in $objectives) {
            if ($target.activateOnFlag -and ([string]$target.activateFlagId -eq $flagId)) {
                $objectiveEdges += [ordered]@{
                    from = $source.index
                    to = $target.index
                    flagId = $flagId
                }
            }
        }
    }
}

$navMarkers = @()
foreach ($sectionName in $sections.Keys) {
    if ($sectionName -match '^NavMarker(?<index>\d+)$') {
        $section = $sections[$sectionName]
        $navMarkers += [ordered]@{
            index = [int]$Matches.index
            x = Get-FitValue -Section $section -Name "xPos" -Default $null
            y = Get-FitValue -Section $section -Name "yPos" -Default $null
            radius = Get-FitValue -Section $section -Name "radius" -Default $null
        }
    }
}
$navMarkers = @($navMarkers | Sort-Object index)

$forests = @()
foreach ($sectionName in $sections.Keys) {
    if ($sectionName -match '^Forest(?<index>\d+)$') {
        $section = $sections[$sectionName]
        $forests += [ordered]@{
            index = [int]$Matches.index
            name = Get-FitValue -Section $section -Name "Name" -Default ""
            center = [ordered]@{
                x = Get-FitValue -Section $section -Name "CenterX" -Default $null
                y = Get-FitValue -Section $section -Name "CenterY" -Default $null
            }
            radius = Get-FitValue -Section $section -Name "Radius" -Default $null
            random = Get-FitValue -Section $section -Name "Random" -Default $false
        }
    }
}
$forests = @($forests | Sort-Object index)

$ablLines = Get-Content -LiteralPath $ablFile.output
$includes = @()
$functions = @()
$scriptBooleans = @()
$checkObjectiveStatus = @()
$voiceLines = @()
for ($lineIndex = 0; $lineIndex -lt $ablLines.Count; $lineIndex++) {
    $line = $ablLines[$lineIndex]
    if ($line -match '#include_\s+"(?<include>[^"]+)"') {
        $includes += $Matches.include
    }
    if ($line -match '^\s*function\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)') {
        $functions += [ordered]@{ name = $Matches.name; line = $lineIndex + 1 }
    }
    if ($line -match '\b(?:static|eternal)\s+boolean\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)') {
        $scriptBooleans += [ordered]@{ name = $Matches.name; line = $lineIndex + 1 }
    }
    if ($line -match 'checkObjectiveStatus\((?<objective>\d+)\)') {
        $checkObjectiveStatus += [ordered]@{
            objective = [int]$Matches.objective
            line = $lineIndex + 1
            text = $line.Trim()
        }
    }
    if ($line -match 'playWave\("(?<wave>[^"]+)"') {
        $voiceLines += [ordered]@{
            wave = $Matches.wave
            line = $lineIndex + 1
        }
    }
}

$scriptSignals = @($scriptBooleans |
    Where-Object { $_.name -match '(?i)objective|trigger|patrol|dead|alarm|hangar|music|starslayer|infantry|sensor' } |
    Sort-Object { $_.line })

$outputDir = Join-Path (Join-Path $OutputRoot $packId) $missionId
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

$analysis = [ordered]@{
    schema = "mc2-mission-analysis-v1"
    createdAt = (Get-Date).ToString("o")
    missionId = $missionId
    pack = [ordered]@{
        id = $packId
        root = $extractManifest.pack.root
    }
    files = [ordered]@{
        fit = $fitFile.output
        abl = $ablFile.output
        pak = if ($pakFile) { $pakFile.output } else { $null }
    }
    mission = [ordered]@{
        name = Get-FitValue -Section $missionSettings -Name "MissionName" -Default $missionId
        author = Get-FitValue -Section $missionSettings -Name "Author" -Default ""
        timeLimit = Get-FitValue -Section $missionSettings -Name "TimeLimit" -Default $null
        dropWeightLimit = Get-FitValue -Section $missionSettings -Name "DropWeightLimit" -Default $null
        resourcePoints = Get-FitValue -Section $missionSettings -Name "ResourcePoints" -Default $null
        maximumNumberOfTeams = Get-FitValue -Section $missionSettings -Name "MaximumNumberOfTeams" -Default $null
        maximumNumberOfPlayers = Get-FitValue -Section $missionSettings -Name "MaximumNumberOfPlayers" -Default $null
        scenarioScript = Get-FitValue -Section $script -Name "ScenarioScript" -Default ""
    }
    camera = [ordered]@{
        projectionAngle = Get-FitValue -Section $camera -Name "ProjectionAngle" -Default $null
        startPosition = [ordered]@{
            x = Get-FitValue -Section $camera -Name "PositionX" -Default $null
            y = Get-FitValue -Section $camera -Name "PositionY" -Default $null
            z = Get-FitValue -Section $camera -Name "PositionZ" -Default $null
        }
        startRotation = Get-FitValue -Section $camera -Name "StartRotation" -Default $null
        newScale = Get-FitValue -Section $camera -Name "NewScale" -Default $null
        zoomMin = Get-FitValue -Section $camera -Name "ZoomMin" -Default $null
        zoomMax = Get-FitValue -Section $camera -Name "ZoomMax" -Default $null
    }
    terrain = [ordered]@{
        minX = Get-FitValue -Section $terrain -Name "TerrainMinX" -Default $null
        minY = Get-FitValue -Section $terrain -Name "TerrainMinY" -Default $null
        waterElevation = Get-FitValue -Section $water -Name "Elevation" -Default $null
    }
    counts = [ordered]@{
        warriors = $numWarriors
        parts = $numParts
        playerUnits = @($units | Where-Object { $_.playerPart }).Count
        enemyUnits = @($units | Where-Object { -not $_.playerPart }).Count
        objectives = $objectives.Count
        visibleObjectives = @($objectives | Where-Object { -not $_.hiddenTrigger }).Count
        hiddenObjectives = @($objectives | Where-Object { $_.hiddenTrigger }).Count
        navMarkers = $navMarkers.Count
        forests = $forests.Count
        scriptLines = $ablLines.Count
    }
    unitSummary = [ordered]@{
        byTeam = ConvertTo-CountMap -Rows $units -Property "teamId"
        byCsvFile = ConvertTo-CountMap -Rows $units -Property "csvFile"
        byBrain = ConvertTo-CountMap -Rows $units -Property "brain"
    }
    units = $units
    objectives = $objectives
    objectiveEdges = $objectiveEdges
    navMarkers = $navMarkers
    forests = $forests
    script = [ordered]@{
        includes = @($includes | Select-Object -Unique)
        functions = $functions
        signals = $scriptSignals
        checkedObjectives = @($checkObjectiveStatus | Sort-Object objective, line)
        voiceLines = @($voiceLines | Sort-Object line)
    }
}

$analysisPath = Join-Path $outputDir "mission-analysis.json"
$analysis | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $analysisPath -Encoding UTF8

$markdownPath = Join-Path $outputDir "mission-analysis.md"
$playerRows = @($units | Where-Object { $_.playerPart } | ForEach-Object { Convert-UnitForMarkdown -Unit $_ })
$enemyRows = @($units | Where-Object { -not $_.playerPart } | Select-Object -First 12 | ForEach-Object { Convert-UnitForMarkdown -Unit $_ })
$objectiveRows = @($objectives | ForEach-Object {
        $conditions = @($_.conditions | ForEach-Object { Get-FitValue -Section @{ values = $_.values } -Name "ConditionSpeciesString" -Default "" })
        $actions = @($_.actions | ForEach-Object { Get-FitValue -Section @{ values = $_.values } -Name "ActionSpeciesString" -Default "" })
        "| $($_.index) | $($_.title) | $($_.hiddenTrigger) | $($_.activateFlagId) | $($conditions -join ', ') | $($actions -join ', ') |"
    })

$edgeRows = @($objectiveEdges | ForEach-Object { "| $($_.from) | $($_.to) | $($_.flagId) |" })
if ($edgeRows.Count -eq 0) {
    $edgeRows = @("| - | - | - |")
}

$markdown = @(
    "# $missionId Mission Analysis",
    "",
    "Generated from pack ``$packId``.",
    "",
    "## Mission",
    "",
    "- Name: $($analysis.mission.name)",
    "- Author: $($analysis.mission.author)",
    "- Script: $($analysis.mission.scenarioScript)",
    "- Drop weight limit: $($analysis.mission.dropWeightLimit)",
    "- Starting resource points: $($analysis.mission.resourcePoints)",
    "- Teams / players: $($analysis.mission.maximumNumberOfTeams) / $($analysis.mission.maximumNumberOfPlayers)",
    "",
    "## Counts",
    "",
    "- Units: $($analysis.counts.warriors) warriors / $($analysis.counts.parts) parts",
    "- Player units: $($analysis.counts.playerUnits)",
    "- Enemy or non-player units: $($analysis.counts.enemyUnits)",
    "- Objectives: $($analysis.counts.visibleObjectives) visible, $($analysis.counts.hiddenObjectives) hidden",
    "- Nav markers: $($analysis.counts.navMarkers)",
    "- Forest regions: $($analysis.counts.forests)",
    "",
    "## Player Lance",
    "",
    "| Index | Team | Unit | Brain | Player | X,Y | Squad |",
    "|---:|---:|---|---|---|---|---:|"
) + $playerRows + @(
    "",
    "## Enemy Sample",
    "",
    "| Index | Team | Unit | Brain | Player | X,Y | Squad |",
    "|---:|---:|---|---|---|---|---:|"
) + $enemyRows + @(
    "",
    "## Objectives",
    "",
    "| # | Title | Hidden | Activate flag | Conditions | Actions |",
    "|---:|---|---|---|---|---|"
) + $objectiveRows + @(
    "",
    "## Objective Flag Edges",
    "",
    "| From objective | To objective | Flag |",
    "|---:|---:|---|"
) + $edgeRows + @(
    "",
    "## Script Signals",
    "",
    ($analysis.script.signals | Select-Object -First 30 | ForEach-Object { "- line $($_.line): $($_.name)" })
)

$markdown | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Output "Mission analyzed: $missionId"
Write-Output "JSON: $analysisPath"
Write-Output "Markdown: $markdownPath"
Write-Output ("Units: {0} player / {1} enemy" -f $analysis.counts.playerUnits, $analysis.counts.enemyUnits)
Write-Output ("Objectives: {0} visible / {1} hidden" -f $analysis.counts.visibleObjectives, $analysis.counts.hiddenObjectives)
