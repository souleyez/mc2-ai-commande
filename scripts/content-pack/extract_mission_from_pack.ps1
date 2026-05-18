param(
    [Parameter(Mandatory = $false)]
    [string]$MissionId = "mc2_01",

    [Parameter(Mandatory = $false)]
    [string]$PackPath = "",

    [Parameter(Mandatory = $false)]
    [string]$OutputRoot = "",

    [Parameter(Mandatory = $false)]
    [string]$UnpackRoot = "",

    [Parameter(Mandatory = $false)]
    [string]$MakeFstPath = "",

    [Parameter(Mandatory = $false)]
    [switch]$RefreshUnpack
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

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "analysis-output\mission-extract"
}

if ([string]::IsNullOrWhiteSpace($UnpackRoot)) {
    $UnpackRoot = Join-Path $repoRoot "analysis-output\fst-unpack"
}

if ([string]::IsNullOrWhiteSpace($MakeFstPath)) {
    $MakeFstPath = Join-Path $repoRoot "build64\out\data_tools\Release\makefst.exe"
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

function Find-UnpackedMissionFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SearchRoot,

        [Parameter(Mandatory = $true)]
        [string]$RelativeMissionPath
    )

    $target = $RelativeMissionPath.ToLowerInvariant().Replace("\", "/")
    $matches = Get-ChildItem -LiteralPath $SearchRoot -Recurse -File |
        Where-Object {
            $_.FullName.Replace("\", "/").ToLowerInvariant().EndsWith($target)
        }

    if ($matches.Count -gt 1) {
        return ($matches | Sort-Object FullName | Select-Object -First 1).FullName
    }

    if ($matches.Count -eq 1) {
        return $matches[0].FullName
    }

    return $null
}

$packRoot = Resolve-PackRoot -InputPath $PackPath
& $validateScript -PackPath $packRoot | Out-Host
$manifest = Read-PackManifest -InputPath $PackPath -PackRoot $packRoot
$packId = if ($null -ne $manifest -and $manifest.id) { $manifest.id } else { Split-Path -Leaf $packRoot }

$missionFst = Join-Path $packRoot "mission.fst"
if (-not (Test-Path -LiteralPath $missionFst -PathType Leaf)) {
    throw "mission.fst is missing from pack: $packRoot"
}

if (-not (Test-Path -LiteralPath $MakeFstPath -PathType Leaf)) {
    throw "makefst.exe is missing: $MakeFstPath"
}

$resolvedUnpackRoot = Join-Path (Resolve-Path -LiteralPath (Split-Path -Parent $UnpackRoot)).Path (Split-Path -Leaf $UnpackRoot)
$missionUnpackRoot = Join-Path $resolvedUnpackRoot "mission.fst"

if ($RefreshUnpack -and (Test-Path -LiteralPath $missionUnpackRoot)) {
    $backupScript = Join-Path $HOME ".codex\bin\Safe-RemoveToBackup.ps1"
    if (-not (Test-Path -LiteralPath $backupScript -PathType Leaf)) {
        throw "Refusing to refresh unpack output because backup helper is missing: $backupScript"
    }

    & $backupScript `
        -Path $missionUnpackRoot `
        -Label "mc2-mission-unpack" `
        -Reason "Refresh unpacked mission.fst analysis output" | Out-Host
}

$expectedPaths = @(
    "data/missions/$MissionId.abl",
    "data/missions/$MissionId.fit",
    "data/missions/$MissionId.pak"
)

$needsUnpack = -not (Test-Path -LiteralPath $missionUnpackRoot -PathType Container)
if (-not $needsUnpack) {
    foreach ($relativePath in $expectedPaths) {
        if (-not (Find-UnpackedMissionFile -SearchRoot $missionUnpackRoot -RelativeMissionPath $relativePath)) {
            $needsUnpack = $true
        }
    }
}

if ($needsUnpack) {
    New-Item -ItemType Directory -Path $resolvedUnpackRoot -Force | Out-Null
    Write-Output "Unpacking mission.fst to: $resolvedUnpackRoot"
    Push-Location $packRoot
    try {
        & $MakeFstPath -d -f "mission.fst" -p $resolvedUnpackRoot | Out-Host
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Output "Using existing unpack output: $missionUnpackRoot"
}

if (-not (Test-Path -LiteralPath $missionUnpackRoot -PathType Container)) {
    $candidate = Get-ChildItem -LiteralPath $resolvedUnpackRoot -Directory |
        Where-Object { $_.Name -eq "mission.fst" -or $_.Name -eq "mission" } |
        Select-Object -First 1
    if ($candidate) {
        $missionUnpackRoot = $candidate.FullName
    }
}

$missionOutputRoot = Join-Path (Join-Path $OutputRoot $packId) $MissionId
New-Item -ItemType Directory -Path $missionOutputRoot -Force | Out-Null

$extracted = @()
foreach ($relativePath in $expectedPaths) {
    $source = Find-UnpackedMissionFile -SearchRoot $missionUnpackRoot -RelativeMissionPath $relativePath
    if (-not $source) {
        throw "Mission file not found after unpack: $relativePath"
    }

    $target = Join-Path $missionOutputRoot $relativePath.Replace("/", "\")
    $targetParent = Split-Path -Parent $target
    New-Item -ItemType Directory -Path $targetParent -Force | Out-Null
    Copy-Item -LiteralPath $source -Destination $target -Force

    $sourceItem = Get-Item -LiteralPath $source
    $targetItem = Get-Item -LiteralPath $target
    $extracted += [ordered]@{
        relativePath = $relativePath
        source = $source
        output = $targetItem.FullName
        bytes = [int64]$targetItem.Length
        sourceRelativeToUnpack = ConvertTo-RelativePath -Root $missionUnpackRoot -Path $sourceItem.FullName
    }
}

$extractManifest = [ordered]@{
    schema = "mc2-mission-extract-v1"
    createdAt = (Get-Date).ToString("o")
    missionId = $MissionId
    pack = [ordered]@{
        id = $packId
        title = if ($null -ne $manifest) { $manifest.title } else { $packId }
        root = $packRoot
    }
    source = [ordered]@{
        missionFst = $missionFst
        unpackRoot = $missionUnpackRoot
        makeFst = (Resolve-Path -LiteralPath $MakeFstPath).Path
    }
    files = $extracted
}

$manifestPath = Join-Path $missionOutputRoot "mission-extract.json"
$extractManifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $manifestPath -Encoding UTF8

Write-Output "Mission extracted: $MissionId"
Write-Output "Output: $missionOutputRoot"
Write-Output "Manifest: $manifestPath"
foreach ($file in $extracted) {
    Write-Output ("  {0} ({1} bytes)" -f $file.relativePath, $file.bytes)
}
