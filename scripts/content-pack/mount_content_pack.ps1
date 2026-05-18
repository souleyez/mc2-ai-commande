param(
    [Parameter(Mandatory = $true)]
    [string]$PackPath,

    [Parameter(Mandatory = $false)]
    [string]$RunPath = "",

    [Parameter(Mandatory = $false)]
    [ValidateSet("Link", "Copy")]
    [string]$Mode = "Link",

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$IncludePreferences,

    [Parameter(Mandatory = $false)]
    [switch]$OnlyPreferences
    ,
    [Parameter(Mandatory = $false)]
    [switch]$MarkerOnly
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$validateScript = Join-Path $PSScriptRoot "validate_content_pack.ps1"

if ([string]::IsNullOrWhiteSpace($RunPath)) {
    $RunPath = Join-Path $repoRoot "mc2-run64-dev"
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
        $manifestFile = (Resolve-Path -LiteralPath $InputPath).Path
        return @{
            Path = $manifestFile
            Data = (Get-Content -LiteralPath $manifestFile -Raw | ConvertFrom-Json)
        }
    }

    $packManifest = Join-Path $PackRoot "pack.json"
    if (Test-Path -LiteralPath $packManifest -PathType Leaf) {
        return @{
            Path = (Resolve-Path -LiteralPath $packManifest).Path
            Data = (Get-Content -LiteralPath $packManifest -Raw | ConvertFrom-Json)
        }
    }

    return $null
}

function Write-MountedMarker {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RunRoot,

        [Parameter(Mandatory = $true)]
        [string]$PackRoot,

        [Parameter(Mandatory = $false)]
        [object]$ManifestInfo
    )

    $manifest = $null
    $manifestPath = $null
    if ($null -ne $ManifestInfo) {
        $manifest = $ManifestInfo.Data
        $manifestPath = $ManifestInfo.Path
    }

    $marker = [ordered]@{
        engineContract = "mc2-content-pack-v1"
        mountedAt = (Get-Date).ToString("o")
        packRoot = $PackRoot
        manifestPath = $manifestPath
        id = if ($null -ne $manifest -and $manifest.id) { $manifest.id } else { Split-Path -Leaf $PackRoot }
        title = if ($null -ne $manifest -and $manifest.title) { $manifest.title } else { Split-Path -Leaf $PackRoot }
        kind = if ($null -ne $manifest -and $manifest.kind) { $manifest.kind } else { "unknown" }
        license = if ($null -ne $manifest -and $manifest.license) { $manifest.license } else { "unknown" }
        version = if ($null -ne $manifest -and $manifest.version) { $manifest.version } else { "unknown" }
        product = if ($null -ne $manifest) { $manifest.product } else { $null }
        mode = $Mode
        includePreferences = $IncludePreferences.IsPresent
        onlyPreferences = $OnlyPreferences.IsPresent
    }

    $markerPath = Join-Path $RunRoot ".content-pack-mounted.json"
    $marker | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $markerPath -Encoding UTF8
    Write-Output "Mounted marker written: $markerPath"
}

function Format-RelativeAction {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Action,

        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    Write-Output ("{0,-10} {1}" -f $Action, $RelativePath)
}

function Backup-ExistingPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    $backupScript = Join-Path $HOME ".codex\bin\Safe-RemoveToBackup.ps1"
    if (-not (Test-Path -LiteralPath $backupScript -PathType Leaf)) {
        throw "Refusing to replace $RelativePath because backup helper is missing: $backupScript"
    }

    & $backupScript `
        -Path $Path `
        -Label "mc2-content-pack" `
        -Reason "Replace runtime content entry $RelativePath while mounting a content pack" | Out-Host
}

function Mount-Directory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Source,

        [Parameter(Mandatory = $true)]
        [string]$Target,

        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    if ($DryRun) {
        Format-RelativeAction "mount-dir" $RelativePath
        return
    }

    Backup-ExistingPath -Path $Target -RelativePath $RelativePath
    if ($Mode -eq "Link") {
        New-Item -ItemType Junction -Path $Target -Target $Source | Out-Null
    }
    else {
        Copy-Item -LiteralPath $Source -Destination $Target -Recurse
    }
}

function Mount-File {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Source,

        [Parameter(Mandatory = $true)]
        [string]$Target,

        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    if ($DryRun) {
        Format-RelativeAction "mount-file" $RelativePath
        return
    }

    Backup-ExistingPath -Path $Target -RelativePath $RelativePath
    if ($Mode -eq "Link") {
        try {
            New-Item -ItemType HardLink -Path $Target -Target $Source | Out-Null
        }
        catch {
            Write-Output "Hard link failed for $RelativePath; copying instead."
            Copy-Item -LiteralPath $Source -Destination $Target
        }
    }
    else {
        Copy-Item -LiteralPath $Source -Destination $Target
    }
}

$packRoot = Resolve-PackRoot -InputPath $PackPath
$resolvedRun = (Resolve-Path -LiteralPath $RunPath).Path
$manifestInfo = Read-PackManifest -InputPath $PackPath -PackRoot $packRoot

if (-not (Test-Path -LiteralPath (Join-Path $resolvedRun "mc2.exe") -PathType Leaf)) {
    throw "RunPath does not look like an MC2 runtime shell because mc2.exe is missing: $resolvedRun"
}

& $validateScript -PackPath $packRoot | Out-Host

if ($MarkerOnly) {
    if ($DryRun) {
        Write-Output "would-write mounted marker for: $packRoot"
    }
    else {
        Write-MountedMarker -RunRoot $resolvedRun -PackRoot $packRoot -ManifestInfo $manifestInfo
    }
    exit 0
}

if ($packRoot -eq $resolvedRun) {
    Write-Output "PackPath and RunPath are the same directory. Nothing to mount."
    exit 0
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

$optionalPreferenceFiles = @()
if ($IncludePreferences -or $OnlyPreferences) {
    $optionalPreferenceFiles = Get-ChildItem -LiteralPath $packRoot -File |
        Where-Object { $_.Name -eq "options.cfg" -or $_.Name -like "*prefs*.cfg" } |
        ForEach-Object { $_.Name }
}

Write-Output "Mount mode: $Mode"
Write-Output "Dry run: $($DryRun.IsPresent)"
Write-Output "Pack root: $packRoot"
Write-Output "Run path: $resolvedRun"
Write-Output "Only preferences: $($OnlyPreferences.IsPresent)"

if (-not $OnlyPreferences) {
    foreach ($relativePath in $requiredDirectories) {
        Mount-Directory `
            -Source (Join-Path $packRoot $relativePath) `
            -Target (Join-Path $resolvedRun $relativePath) `
            -RelativePath $relativePath
    }

    foreach ($relativePath in $requiredFiles) {
        Mount-File `
            -Source (Join-Path $packRoot $relativePath) `
            -Target (Join-Path $resolvedRun $relativePath) `
            -RelativePath $relativePath
    }
}

foreach ($relativePath in $optionalPreferenceFiles) {
    Mount-File `
        -Source (Join-Path $packRoot $relativePath) `
        -Target (Join-Path $resolvedRun $relativePath) `
        -RelativePath $relativePath
}

if ($DryRun) {
    Write-Output "Dry run complete. Remove -DryRun to mount this pack."
}
else {
    if (-not $OnlyPreferences) {
        Write-MountedMarker -RunRoot $resolvedRun -PackRoot $packRoot -ManifestInfo $manifestInfo
    }
    Write-Output "Content pack mounted."
}
