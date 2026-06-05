param(
    [Parameter(Mandatory = $false)]
    [string]$MissionContractPath = "",

    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "",

    [Parameter(Mandatory = $false)]
    [string]$PackRoot = "",

    [Parameter(Mandatory = $false)]
    [string]$ContentIndexPath = "",

    [Parameter(Mandatory = $false)]
    [string]$TextureExportRoot = "",

    [Parameter(Mandatory = $false)]
    [switch]$ExportReferenceTextures
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

if ([string]::IsNullOrWhiteSpace($MissionContractPath)) {
    $MissionContractPath = Join-Path $repoRoot "unity-mc2-demo\Assets\StreamingAssets\Missions\mc2_01\mission-contract.json"
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repoRoot "analysis-output\terrain-texture-audit\mc2_01-terrain-texture-audit.json"
}

if ([string]::IsNullOrWhiteSpace($PackRoot)) {
    $PackRoot = Join-Path $repoRoot "content-packs\project-owned-linked-dev"
}

if ([string]::IsNullOrWhiteSpace($ContentIndexPath)) {
    $ContentIndexPath = Join-Path $repoRoot "analysis-output\content-index\project-owned-linked-dev.content-index.json"
}

if ([string]::IsNullOrWhiteSpace($TextureExportRoot)) {
    $TextureExportRoot = Join-Path $repoRoot "analysis-output\terrain-reference-textures\mc2_01"
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

function Expand-ZlibBytes {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]]$Bytes
    )

    if ($Bytes.Length -lt 7 -or $Bytes[0] -ne 0x78) {
        return $Bytes
    }

    $source = $null
    $deflate = $null
    $target = $null
    try {
        $source = [System.IO.MemoryStream]::new($Bytes, 2, $Bytes.Length - 6)
        $deflate = [System.IO.Compression.DeflateStream]::new($source, [System.IO.Compression.CompressionMode]::Decompress)
        $target = [System.IO.MemoryStream]::new()
        $deflate.CopyTo($target)
        return $target.ToArray()
    }
    finally {
        if ($null -ne $deflate) {
            $deflate.Dispose()
        }

        if ($null -ne $source) {
            $source.Dispose()
        }

        if ($null -ne $target) {
            $target.Dispose()
        }
    }
}

function Read-TgaInfo {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]]$Bytes
    )

    if ($Bytes.Length -lt 18) {
        return $null
    }

    return [ordered]@{
        format = "tga"
        width = [int](([int]$Bytes[12]) -bor (([int]$Bytes[13]) -shl 8))
        height = [int](([int]$Bytes[14]) -bor (([int]$Bytes[15]) -shl 8))
        bitsPerPixel = [int]$Bytes[16]
        imageType = [int]$Bytes[2]
    }
}

function Read-TxmInfo {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]]$Bytes
    )

    $pixels = Expand-ZlibBytes -Bytes $Bytes
    $pixelCount = [int]($pixels.Length / 4)
    $side = [int][Math]::Sqrt($pixelCount)
    if ($side * $side * 4 -ne $pixels.Length) {
        return [ordered]@{
            format = "txm"
            width = 0
            height = 0
            bitsPerPixel = 32
            decodedBytes = $pixels.Length
        }
    }

    return [ordered]@{
        format = "txm"
        width = $side
        height = $side
        bitsPerPixel = 32
        decodedBytes = $pixels.Length
    }
}

function Read-LstTexturePaths {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]]$Bytes
    )

    $text = [System.Text.Encoding]::ASCII.GetString($Bytes)
    return @(
        $text -split "`0" |
            ForEach-Object { $_.Trim() } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
}

function Export-ReferenceTerrainTextures {
    param(
        [Parameter(Mandatory = $true)]
        $Contract,

        [Parameter(Mandatory = $true)]
        [hashtable]$TextureIdSampleCounts,

        [Parameter(Mandatory = $true)]
        [string]$PackRootPath,

        [Parameter(Mandatory = $true)]
        [string]$IndexPath,

        [Parameter(Mandatory = $true)]
        [string]$ExportRoot
    )

    if (-not (Test-Path -LiteralPath $IndexPath -PathType Leaf)) {
        throw "Missing content index for terrain texture export: $IndexPath"
    }

    $textureFstPath = Join-Path $PackRootPath "textures.fst"
    if (-not (Test-Path -LiteralPath $textureFstPath -PathType Leaf)) {
        throw "Missing textures.fst: $textureFstPath"
    }

    $index = Get-Content -LiteralPath $IndexPath -Raw | ConvertFrom-Json
    $textureFst = $index.fastFiles.files | Where-Object { $_.name -eq "textures.fst" } | Select-Object -First 1
    if ($null -eq $textureFst -or $null -eq $textureFst.entries) {
        throw "Content index does not contain textures.fst entries: $IndexPath"
    }

    New-Item -ItemType Directory -Path $ExportRoot -Force | Out-Null
    $sourceBytes = [System.IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $textureFstPath).Path)
    $entries = @()
    $textureIds = @(
        $TextureIdSampleCounts.Keys |
            Where-Object { $_ -match "^-?\d+$" } |
            ForEach-Object { [int]$_ } |
            Where-Object { $_ -ge 0 -and $_ -lt $textureFst.entries.Count } |
            Sort-Object -Unique
    )

    foreach ($textureId in $textureIds) {
        $entry = $textureFst.entries[$textureId]
        if ($null -eq $entry -or [string]::IsNullOrWhiteSpace($entry.name)) {
            continue
        }

        $extension = [System.IO.Path]::GetExtension($entry.name).ToLowerInvariant()
        $mountedPath = Join-Path $PackRootPath ($entry.name.Replace("/", "\"))
        if ($extension -eq ".txm" -and (Test-Path -LiteralPath $mountedPath -PathType Leaf)) {
            $payload = [System.IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $mountedPath).Path)
        }
        else {
            $packedBytes = [int]$entry.packedBytes
            $offset = [int]$entry.offset
            if ($packedBytes -le 0 -or $offset -lt 0 -or ($offset + $packedBytes) -gt $sourceBytes.Length) {
                continue
            }

            $packed = New-Object byte[] $packedBytes
            [Array]::Copy($sourceBytes, $offset, $packed, 0, $packedBytes)
            $payload = Expand-ZlibBytes -Bytes $packed
        }

        $relativePath = $entry.name.Replace("/", "\")
        $outputPath = Join-Path $ExportRoot $relativePath
        $outputDirectory = Split-Path -Parent $outputPath
        if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
            New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
        }

        [System.IO.File]::WriteAllBytes($outputPath, $payload)
        if ($extension -eq ".lst") {
            foreach ($listedPath in (Read-LstTexturePaths -Bytes $payload)) {
                $listedRelativePath = $listedPath.Replace("/", "\")
                $listedMountedPath = Join-Path $PackRootPath $listedRelativePath
                if (-not (Test-Path -LiteralPath $listedMountedPath -PathType Leaf)) {
                    continue
                }

                $listedOutputPath = Join-Path $ExportRoot $listedRelativePath
                $listedOutputDirectory = Split-Path -Parent $listedOutputPath
                if (-not [string]::IsNullOrWhiteSpace($listedOutputDirectory)) {
                    New-Item -ItemType Directory -Path $listedOutputDirectory -Force | Out-Null
                }

                Copy-Item -LiteralPath $listedMountedPath -Destination $listedOutputPath -Force
            }
        }

        $info = if ($extension -eq ".tga") {
            Read-TgaInfo -Bytes $payload
        }
        elseif ($extension -eq ".txm") {
            Read-TxmInfo -Bytes $payload
        }
        else {
            [ordered]@{
                format = $extension.TrimStart(".")
                width = 0
                height = 0
                bitsPerPixel = 0
            }
        }

        $entries += [ordered]@{
            textureId = $textureId
            sampleCount = [int]$TextureIdSampleCounts[[string]$textureId]
            sourceName = $entry.name
            relativePath = $relativePath
            outputPath = (Resolve-Path -LiteralPath $outputPath).Path
            packedBytes = [int]$entry.packedBytes
            realBytes = [int]$entry.realBytes
            hash = $entry.hash
            format = $info.format
            width = [int]$info.width
            height = [int]$info.height
            bitsPerPixel = [int]$info.bitsPerPixel
        }
    }

    $manifest = [ordered]@{
        schema = "mc2-terrain-reference-textures-v1"
        missionId = $Contract.source.missionId
        sourceFst = (Resolve-Path -LiteralPath $textureFstPath).Path
        sourceIndex = (Resolve-Path -LiteralPath $IndexPath).Path
        outputRoot = (Resolve-Path -LiteralPath $ExportRoot).Path
        textureCount = $entries.Count
        textures = $entries
    }

    $manifestPath = Join-Path $ExportRoot "manifest.json"
    $manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
    Write-Output "Terrain reference textures ready: $manifestPath"
    Write-Output ("Terrain reference textures exported: {0}" -f $entries.Count)
}

$terrainTypeCounts = @{}
$waterFlagCounts = @{}
$textureIdCounts = @{}
$textureSetCounts = @{}
$textureComboCounts = @{}
$lightCounts = @{}
$referencedTextureIds = @{}
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
    Add-Count -Table $referencedTextureIds -Key ([string]$textureId)

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

if ($ExportReferenceTextures) {
    Export-ReferenceTerrainTextures `
        -Contract $contract `
        -TextureIdSampleCounts $referencedTextureIds `
        -PackRootPath $PackRoot `
        -IndexPath $ContentIndexPath `
        -ExportRoot $TextureExportRoot
}
