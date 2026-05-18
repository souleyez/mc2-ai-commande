param(
    [Parameter(Mandatory = $false)]
    [string]$PackPath = "",

    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "",

    [Parameter(Mandatory = $false)]
    [switch]$IncludeFileList
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$validateScript = Join-Path $PSScriptRoot "validate_content_pack.ps1"

if ([string]::IsNullOrWhiteSpace($PackPath)) {
    $linkedDevPack = Join-Path $repoRoot "content-packs\project-owned-linked-dev"
    if (Test-Path -LiteralPath $linkedDevPack -PathType Container) {
        $PackPath = $linkedDevPack
    }
    else {
        $PackPath = Join-Path $repoRoot "content-packs\mc2-original.local.example.json"
    }
}

function Resolve-PackRoot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$InputPath
    )

    if (Test-Path -LiteralPath $InputPath -PathType Leaf) {
        $manifestFile = (Resolve-Path -LiteralPath $InputPath).Path
        $manifestDir = Split-Path -Parent $manifestFile
        $manifest = Get-Content -LiteralPath $manifestFile -Raw | ConvertFrom-Json
        if ([string]::IsNullOrWhiteSpace($manifest.sourcePath)) {
            throw "Manifest does not include sourcePath: $manifestFile"
        }

        if ([System.IO.Path]::IsPathRooted($manifest.sourcePath)) {
            return (Resolve-Path -LiteralPath $manifest.sourcePath).Path
        }

        return (Resolve-Path -LiteralPath (Join-Path $manifestDir $manifest.sourcePath)).Path
    }

    if (Test-Path -LiteralPath $InputPath -PathType Container) {
        return (Resolve-Path -LiteralPath $InputPath).Path
    }

    throw "PackPath does not exist: $InputPath"
}

function Read-PackManifest {
    param(
        [Parameter(Mandatory = $true)]
        [string]$InputPath,

        [Parameter(Mandatory = $true)]
        [string]$PackRoot
    )

    if (Test-Path -LiteralPath $InputPath -PathType Leaf) {
        return Get-Content -LiteralPath (Resolve-Path -LiteralPath $InputPath).Path -Raw | ConvertFrom-Json
    }

    $packManifest = Join-Path $PackRoot "pack.json"
    if (Test-Path -LiteralPath $packManifest -PathType Leaf) {
        return Get-Content -LiteralPath $packManifest -Raw | ConvertFrom-Json
    }

    return $null
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

function Get-ResourceCategory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $lower = $Path.ToLowerInvariant()
    if ($lower -match "mission|missions|\.abl$|\.fit$") {
        return "mission-script-data"
    }
    if ($lower -match "mech|mover|warrior|pilot|chassis") {
        return "unit-pilot-data"
    }
    if ($lower -match "weapon|ammo|effect|particle|explosion") {
        return "weapon-effect-data"
    }
    if ($lower -match "interface|ui|cursor|font|glyph|bitmap|graphics|insignia") {
        return "ui-graphics"
    }
    if ($lower -match "texture|\.txm$|\.tga$|\.bmp$") {
        return "texture-art"
    }
    if ($lower -match "sound|\.wav$|\.mp3$|\.ogg$") {
        return "audio"
    }
    if ($lower -match "movie|video|\.bik$|\.avi$|\.wmv$") {
        return "video"
    }
    if ($lower -match "object|tgl|\.agl$|\.ase$|\.pak$") {
        return "model-object"
    }
    return "other"
}

function Add-Count {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Map,

        [Parameter(Mandatory = $true)]
        [string]$Key,

        [Parameter(Mandatory = $false)]
        [int64]$Bytes = 0
    )

    if (-not $Map.ContainsKey($Key)) {
        $Map[$Key] = [ordered]@{
            count = 0
            bytes = [int64]0
        }
    }

    $Map[$Key].count++
    $Map[$Key].bytes += $Bytes
}

function Convert-CountMap {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Map
    )

    return @(
        $Map.GetEnumerator() |
            Sort-Object Name |
            ForEach-Object {
                [ordered]@{
                    key = $_.Key
                    count = $_.Value.count
                    bytes = $_.Value.bytes
                }
            }
    )
}

function Read-FastFileIndex {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $reader = $null
    try {
        $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
        $reader = [System.IO.BinaryReader]::new($stream)

        $version = $reader.ReadUInt32()
        $numFiles = $reader.ReadUInt32()
        $versionPlain = [uint32]::Parse("CADDECAF", [System.Globalization.NumberStyles]::HexNumber)
        $versionCompressed = [uint32]::Parse("FADDECAF", [System.Globalization.NumberStyles]::HexNumber)
        $isCompressed = $version -eq $versionCompressed
        $validVersion = $version -eq $versionPlain -or $isCompressed

        $entries = @()
        if ($validVersion) {
            for ($i = 0; $i -lt $numFiles; $i++) {
                $offset = $reader.ReadUInt32()
                $size = $reader.ReadUInt32()
                $realSize = $reader.ReadUInt32()
                $hash = $reader.ReadUInt32()
                $nameBytes = $reader.ReadBytes(250)
                $nameLength = [Array]::IndexOf($nameBytes, [byte]0)
                if ($nameLength -lt 0) {
                    $nameLength = $nameBytes.Length
                }
                $name = [System.Text.Encoding]::ASCII.GetString($nameBytes, 0, $nameLength).Replace("\", "/")
                $extension = [System.IO.Path]::GetExtension($name).ToLowerInvariant()
                if ([string]::IsNullOrWhiteSpace($extension)) {
                    $extension = "[none]"
                }
                $entries += [ordered]@{
                    name = $name
                    extension = $extension
                    category = Get-ResourceCategory -Path $name
                    offset = [uint32]$offset
                    packedBytes = [uint32]$size
                    realBytes = [uint32]$realSize
                    hash = ("0x{0:X8}" -f $hash)
                }
            }
        }

        return [ordered]@{
            name = [System.IO.Path]::GetFileName($Path)
            path = $Path
            version = ("0x{0:X8}" -f $version)
            validVersion = $validVersion
            compressed = $isCompressed
            fileCount = [uint32]$numFiles
            entries = $entries
        }
    }
    finally {
        if ($null -ne $reader) {
            $reader.Close()
        }
    }
}

$packRoot = Resolve-PackRoot -InputPath $PackPath
& $validateScript -PackPath $packRoot | Out-Host
$manifest = Read-PackManifest -InputPath $PackPath -PackRoot $packRoot

$packId = if ($null -ne $manifest -and $manifest.id) { $manifest.id } else { Split-Path -Leaf $packRoot }
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $outputDir = Join-Path $repoRoot "analysis-output\content-index"
    $OutputPath = Join-Path $outputDir "$packId.content-index.json"
}

$filesystemExtensionCounts = @{}
$filesystemCategoryCounts = @{}
$filesystemRootCounts = @{}
$filesystemFiles = @()

function Add-FileToIndex {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.FileInfo]$File,

        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    if ([string]::IsNullOrWhiteSpace($RelativePath)) {
        $relative = ConvertTo-RelativePath -Root $packRoot -Path $File.FullName
    }
    else {
        $relative = $RelativePath.Replace("\", "/")
    }

    $extension = $File.Extension.ToLowerInvariant()
    if ([string]::IsNullOrWhiteSpace($extension)) {
        $extension = "[none]"
    }
    $category = Get-ResourceCategory -Path $relative
    $rootName = ($relative -split "/")[0]

    Add-Count -Map $filesystemExtensionCounts -Key $extension -Bytes $File.Length
    Add-Count -Map $filesystemCategoryCounts -Key $category -Bytes $File.Length
    Add-Count -Map $filesystemRootCounts -Key $rootName -Bytes $File.Length

    if ($IncludeFileList) {
        $filesystemFiles += [ordered]@{
            path = $relative
            extension = $extension
            category = $category
            bytes = [int64]$File.Length
        }
    }
}

$topLevelFiles = Get-ChildItem -LiteralPath $packRoot -File
foreach ($file in $topLevelFiles) {
    Add-FileToIndex -File $file -RelativePath $file.Name
}

$knownContentRoots = @("assets", "data", "shaders")
foreach ($rootName in $knownContentRoots) {
    $rootPath = Join-Path $packRoot $rootName
    if (-not (Test-Path -LiteralPath $rootPath -PathType Container)) {
        continue
    }

    Get-ChildItem -LiteralPath $rootPath -Recurse -File | ForEach-Object {
        $relativeToRoot = ConvertTo-RelativePath -Root $rootPath -Path $_.FullName
        Add-FileToIndex -File $_ -RelativePath "$rootName/$relativeToRoot"
    }
}

$fastFileSummaries = @()
$fastFileExtensionCounts = @{}
$fastFileCategoryCounts = @{}

Get-ChildItem -LiteralPath $packRoot -Filter "*.fst" -File | Sort-Object Name | ForEach-Object {
    $fastFile = Read-FastFileIndex -Path $_.FullName
    foreach ($entry in $fastFile.entries) {
        Add-Count -Map $fastFileExtensionCounts -Key $entry.extension -Bytes $entry.realBytes
        Add-Count -Map $fastFileCategoryCounts -Key $entry.category -Bytes $entry.realBytes
    }

    $summary = [ordered]@{
        name = $fastFile.name
        version = $fastFile.version
        validVersion = $fastFile.validVersion
        compressed = $fastFile.compressed
        fileCount = $fastFile.fileCount
    }
    if ($IncludeFileList) {
        $summary.entries = $fastFile.entries
    }

    $fastFileSummaries += $summary
}

$index = [ordered]@{
    schema = "mc2-content-index-v1"
    createdAt = (Get-Date).ToString("o")
    pack = [ordered]@{
        id = $packId
        title = if ($null -ne $manifest) { $manifest.title } else { $packId }
        kind = if ($null -ne $manifest) { $manifest.kind } else { "unknown" }
        engineContract = if ($null -ne $manifest) { $manifest.engineContract } else { "mc2-content-pack-v1" }
        root = $packRoot
    }
    filesystem = [ordered]@{
        byRoot = Convert-CountMap -Map $filesystemRootCounts
        byExtension = Convert-CountMap -Map $filesystemExtensionCounts
        byCategory = Convert-CountMap -Map $filesystemCategoryCounts
    }
    fastFiles = [ordered]@{
        files = $fastFileSummaries
        byExtension = Convert-CountMap -Map $fastFileExtensionCounts
        byCategory = Convert-CountMap -Map $fastFileCategoryCounts
    }
}

if ($IncludeFileList) {
    $index.filesystem.files = $filesystemFiles
}

$outputParent = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outputParent -PathType Container)) {
    New-Item -ItemType Directory -Path $outputParent -Force | Out-Null
}

$index | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $OutputPath -Encoding UTF8

Write-Output "Content index written: $OutputPath"
Write-Output "Pack id: $packId"
Write-Output "Filesystem roots: $($filesystemRootCounts.Count)"
Write-Output "Fast files: $($fastFileSummaries.Count)"
