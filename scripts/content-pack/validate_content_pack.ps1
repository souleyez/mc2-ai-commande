param(
    [Parameter(Mandatory = $false)]
    [string]$PackPath = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
if ([string]::IsNullOrWhiteSpace($PackPath)) {
    $PackPath = Join-Path $repoRoot "mc2-run64-dev"
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

    throw "Content pack path does not exist: $InputPath"
}

$resolvedPack = Resolve-PackRoot -InputPath $PackPath

$requiredDirectories = @(
    "assets",
    "data",
    "shaders"
)

$requiredFiles = @(
    "system.cfg",
    "mission.fst",
    "tgl.fst",
    "art.fst",
    "textures.fst",
    "misc.fst",
    "camera.fst",
    "effect.fst",
    "insignia.fst",
    "testtxm.tga"
)

$missingDirectories = foreach ($relativePath in $requiredDirectories) {
    $path = Join-Path $resolvedPack $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Container)) {
        $relativePath
    }
}

$missingFiles = foreach ($relativePath in $requiredFiles) {
    $path = Join-Path $resolvedPack $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $relativePath
    }
}

if ($missingDirectories.Count -gt 0 -or $missingFiles.Count -gt 0) {
    Write-Output "Content pack validation failed: $resolvedPack"
    if ($missingDirectories.Count -gt 0) {
        Write-Output "Missing directories:"
        $missingDirectories | ForEach-Object { Write-Output "  - $_" }
    }
    if ($missingFiles.Count -gt 0) {
        Write-Output "Missing files:"
        $missingFiles | ForEach-Object { Write-Output "  - $_" }
    }
    throw "Content pack is incomplete."
}

$manifestPath = Join-Path $resolvedPack "pack.json"
$manifest = $null
if (Test-Path -LiteralPath $manifestPath -PathType Leaf) {
    try {
        $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    }
    catch {
        throw "pack.json exists but is not valid JSON: $manifestPath"
    }
}

$fstFiles = Get-ChildItem -LiteralPath $resolvedPack -Filter "*.fst" -File
$fastFileBytes = ($fstFiles | Measure-Object -Property Length -Sum).Sum
if ($null -eq $fastFileBytes) {
    $fastFileBytes = 0
}

Write-Output "Content pack OK: $resolvedPack"
Write-Output "Required directories: $($requiredDirectories.Count)"
Write-Output "Required files: $($requiredFiles.Count)"
Write-Output "Fast files: $($fstFiles.Count)"
Write-Output "Fast file bytes: $fastFileBytes"

if ($null -ne $manifest) {
    Write-Output "Manifest id: $($manifest.id)"
    Write-Output "Manifest title: $($manifest.title)"
    Write-Output "Engine contract: $($manifest.engineContract)"
}
else {
    Write-Output "Manifest: not present (optional)"
}
