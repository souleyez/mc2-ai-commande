param(
    [Parameter(Mandatory = $true)]
    [string]$PackId,

    [Parameter(Mandatory = $false)]
    [string]$Title = "",

    [Parameter(Mandatory = $false)]
    [string]$OutputRoot = "",

    [Parameter(Mandatory = $false)]
    [string]$ReferencePackPath = "",

    [Parameter(Mandatory = $false)]
    [ValidateSet("Empty", "ReferenceLinks")]
    [string]$SeedMode = "Empty",

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$validateScript = Join-Path $PSScriptRoot "validate_content_pack.ps1"

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "content-packs"
}

if ([string]::IsNullOrWhiteSpace($ReferencePackPath)) {
    $ReferencePackPath = Join-Path $repoRoot "content-packs\mc2-original.local.example.json"
}

if ([string]::IsNullOrWhiteSpace($Title)) {
    $Title = $PackId
}

if ($PackId -notmatch "^[a-z0-9][a-z0-9._-]*$") {
    throw "PackId must use lowercase letters, numbers, dots, underscores, or hyphens: $PackId"
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

    throw "ReferencePackPath does not exist: $InputPath"
}

$resolvedOutputRoot = (Resolve-Path -LiteralPath $OutputRoot).Path
$packRoot = Join-Path $resolvedOutputRoot $PackId
$referenceRoot = Resolve-PackRoot -InputPath $ReferencePackPath

if ($SeedMode -eq "ReferenceLinks") {
    & $validateScript -PackPath $referenceRoot | Out-Host
}

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

$manifest = [ordered]@{
    id = $PackId
    title = $Title
    kind = "replacement"
    license = "project-owned-or-cleared"
    version = "0.1.0"
    engineContract = "mc2-content-pack-v1"
    product = [ordered]@{
        id = $PackId
        title = $Title
        language = "zh-CN"
        audience = "development"
    }
    sourcePath = "."
    seedMode = $SeedMode
    replacementStatus = "scaffold"
    notes = @(
        "This pack is a scaffold for project-owned or properly licensed content.",
        "Do not add original copyrighted assets to a public replacement pack."
    )
}

$checklist = @"
# $Title Content Replacement Checklist

Pack id: `$PackId`
Engine contract: `mc2-content-pack-v1`
Seed mode: `$SeedMode`

## Must Replace Before Public Build

- Product name, logo, splash, menus, and UI strings
- Mech names, chassis text, pilot names, faction names, and mission text
- Textures, models, effects, audio, video, and portraits
- Campaign and mission scripts if they contain original story or trademarks
- Any direct trademark, faction, setting, or lore references

## Runtime Contract

Required directories:

- assets
- data
- shaders

Required files:

- system.cfg
- mission.fst
- tgl.fst
- art.fst
- textures.fst
- misc.fst
- camera.fst
- effect.fst
- insignia.fst
- testtxm.tga

## Validation

Run:

```powershell
& .\scripts\content-pack\validate_content_pack.ps1 -PackPath .\content-packs\$PackId
```

Empty scaffold packs are not expected to validate until real or linked runtime
content exists.
"@

Write-Output "New content pack: $packRoot"
Write-Output "Seed mode: $SeedMode"
Write-Output "Reference root: $referenceRoot"
Write-Output "Dry run: $($DryRun.IsPresent)"

if (Test-Path -LiteralPath $packRoot) {
    if (-not $Force) {
        if ($DryRun) {
            Write-Output "would-skip existing pack root without -Force"
            exit 0
        }
        throw "Pack root already exists. Use -Force to archive it before recreating: $packRoot"
    }

    if ($DryRun) {
        Write-Output "would-backup existing pack root: $packRoot"
    }
    else {
        $backupScript = Join-Path $HOME ".codex\bin\Safe-RemoveToBackup.ps1"
        if (-not (Test-Path -LiteralPath $backupScript -PathType Leaf)) {
            throw "Refusing to replace pack because backup helper is missing: $backupScript"
        }

        & $backupScript `
            -Path $packRoot `
            -Label "mc2-content-pack-scaffold" `
            -Reason "Recreate content pack scaffold $PackId" | Out-Host
    }
}

if ($DryRun) {
    Write-Output "would-create pack.json"
    Write-Output "would-create REPLACEMENT-CHECKLIST.md"
    foreach ($relativePath in $requiredDirectories) {
        Write-Output "would-create-dir $relativePath"
    }
    if ($SeedMode -eq "ReferenceLinks") {
        foreach ($relativePath in $requiredFiles) {
            Write-Output "would-link-file $relativePath"
        }
    }
    Write-Output "Dry run complete. Remove -DryRun to create the content pack scaffold."
    exit 0
}

New-Item -ItemType Directory -Path $packRoot -Force | Out-Null

foreach ($relativePath in $requiredDirectories) {
    $target = Join-Path $packRoot $relativePath
    if ($SeedMode -eq "ReferenceLinks") {
        New-Item -ItemType Junction -Path $target -Target (Join-Path $referenceRoot $relativePath) | Out-Null
    }
    else {
        New-Item -ItemType Directory -Path $target -Force | Out-Null
    }
}

if ($SeedMode -eq "ReferenceLinks") {
    foreach ($relativePath in $requiredFiles) {
        New-Item `
            -ItemType HardLink `
            -Path (Join-Path $packRoot $relativePath) `
            -Target (Join-Path $referenceRoot $relativePath) | Out-Null
    }
}

$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $packRoot "pack.json") -Encoding UTF8
$checklist | Set-Content -LiteralPath (Join-Path $packRoot "REPLACEMENT-CHECKLIST.md") -Encoding UTF8

Write-Output "Content pack scaffold created: $packRoot"
if ($SeedMode -eq "ReferenceLinks") {
    & $validateScript -PackPath $packRoot | Out-Host
}
else {
    Write-Output "Empty scaffold created. It will validate after runtime content is added."
}
