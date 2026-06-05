param(
    [Parameter(Mandatory = $false)]
    [string]$MissionContractPath = "",

    [Parameter(Mandatory = $false)]
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

if ([string]::IsNullOrWhiteSpace($MissionContractPath)) {
    $MissionContractPath = Join-Path $repoRoot "unity-mc2-demo\Assets\StreamingAssets\Missions\mc2_01\mission-contract.json"
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repoRoot "analysis-output\terrain-texture-audit\mc2_01-terrain-texture-audit.json"
}

if (-not (Test-Path -LiteralPath $MissionContractPath -PathType Leaf)) {
    throw "Missing mission contract: $MissionContractPath"
}

$contract = Get-Content -LiteralPath $MissionContractPath -Raw | ConvertFrom-Json
$terrain = $contract.terrainMesh
if ($null -eq $terrain -or $null -eq $terrain.samples) {
    throw "Mission contract does not contain terrainMesh.samples: $MissionContractPath"
}

function Add-Count {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Table,

        [Parameter(Mandatory = $true)]
        [string]$Key
    )

    if (-not $Table.ContainsKey($Key)) {
        $Table[$Key] = 0
    }

    $Table[$Key]++
}

$terrainTypeCounts = @{}
$waterFlagCounts = @{}
$textureIdCounts = @{}
$textureSetCounts = @{}
$textureComboCounts = @{}
$lightCounts = @{}
$sourceSide = [int]$terrain.sampleSide
$spacing = [double]$terrain.worldUnitsPerVertex * [Math]::Max(1, [int]$terrain.sampleStep)
$minX = [double]$terrain.minX
$minY = [double]$terrain.minY
$waterElevation = [double]$contract.mission.terrain.waterElevation
$examples = @()

for ($index = 0; $index -lt $terrain.samples.Count; $index++) {
    $sample = $terrain.samples[$index]
    $textureData = [uint32]$sample.textureData
    $textureId = [int]($textureData -band 0xffff)
    $textureSet = [int](($textureData -shr 16) -band 0xffff)
    $terrainType = [int]$sample.terrainType
    $water = [int]$sample.water
    $light = [uint32]$sample.light

    Add-Count -Table $terrainTypeCounts -Key ([string]$terrainType)
    Add-Count -Table $waterFlagCounts -Key ([string]$water)
    Add-Count -Table $textureIdCounts -Key ([string]$textureId)
    Add-Count -Table $textureSetCounts -Key ([string]$textureSet)
    Add-Count -Table $textureComboCounts -Key ("terrain=$terrainType water=$water texture=$textureId set=$textureSet")
    Add-Count -Table $lightCounts -Key ([string]$light)

    if (($textureId -ne 0 -and $textureId -ne 2) -or $terrainType -ne 2) {
        $row = [int][Math]::Floor($index / $sourceSide)
        $col = [int]($index % $sourceSide)
        if ($examples.Count -lt 120) {
            $examples += [ordered]@{
                index = $index
                row = $row
                col = $col
                x = $minX + $col * $spacing
                y = $minY - $row * $spacing
                elevation = [double]$sample.elevation
                isBelowWater = ([double]$sample.elevation -le ($waterElevation + 4.0))
                terrainType = $terrainType
                water = $water
                textureId = $textureId
                textureSet = $textureSet
            }
        }
    }
}

$audit = [ordered]@{
    schema = "mc2-terrain-texture-audit-v1"
    missionId = $contract.source.missionId
    sourceContract = (Resolve-Path -LiteralPath $MissionContractPath).Path
    sampleSide = $sourceSide
    sampleCount = $terrain.samples.Count
    waterElevation = $waterElevation
    terrainTypeCounts = $terrainTypeCounts
    waterFlagCounts = $waterFlagCounts
    textureIdCounts = $textureIdCounts
    textureSetCounts = $textureSetCounts
    textureComboCounts = $textureComboCounts
    lightCounts = $lightCounts
    nonDefaultExamples = $examples
}

$outputDirectory = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$audit | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $OutputPath -Encoding UTF8
Write-Output "Terrain texture audit ready: $OutputPath"
