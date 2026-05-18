param(
    [Parameter(Mandatory = $false)]
    [string]$RunPath = "",

    [Parameter(Mandatory = $false)]
    [string]$ShellSourcePath = "",

    [Parameter(Mandatory = $false)]
    [string]$PackPath = "",

    [Parameter(Mandatory = $false)]
    [ValidateSet("Link", "Copy")]
    [string]$ContentMode = "Link",

    [Parameter(Mandatory = $false)]
    [string]$Mission = "",

    [Parameter(Mandatory = $false)]
    [string[]]$ExtraArgs = @(),

    [Parameter(Mandatory = $false)]
    [switch]$RebuildShell,

    [Parameter(Mandatory = $false)]
    [switch]$Force,

    [Parameter(Mandatory = $false)]
    [switch]$NoPreferences,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$newShellScript = Join-Path $PSScriptRoot "new_runtime_shell.ps1"
$mountScript = Join-Path $PSScriptRoot "mount_content_pack.ps1"
$validateScript = Join-Path $PSScriptRoot "validate_content_pack.ps1"

if ([string]::IsNullOrWhiteSpace($RunPath)) {
    $RunPath = Join-Path $repoRoot "runtime-shell-dev"
}

if ([string]::IsNullOrWhiteSpace($ShellSourcePath)) {
    $ShellSourcePath = Join-Path $repoRoot "mc2-run64-dev"
}

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

function Read-MountedMarker {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RuntimeRoot
    )

    $markerPath = Join-Path $RuntimeRoot ".content-pack-mounted.json"
    if (-not (Test-Path -LiteralPath $markerPath -PathType Leaf)) {
        return $null
    }

    return Get-Content -LiteralPath $markerPath -Raw | ConvertFrom-Json
}

$runExists = Test-Path -LiteralPath $RunPath -PathType Container
$needsPreferenceRebuild = $false
$requestedPackRoot = Resolve-PackRoot -InputPath $PackPath
$needsContentSwitch = $false
$needsMarkerWrite = $false

if ($runExists -and -not $RebuildShell) {
    $runtimeRoot = (Resolve-Path -LiteralPath $RunPath).Path
    $marker = Read-MountedMarker -RuntimeRoot $runtimeRoot
    if ($null -eq $marker) {
        $needsMarkerWrite = $true
    }
    elseif ($marker.packRoot -ne $requestedPackRoot) {
        $needsContentSwitch = $true
    }
}

if ($runExists -and -not $NoPreferences) {
    $requiredPreferenceFiles = @("prefs.cfg", "options.cfg")
    foreach ($relativePath in $requiredPreferenceFiles) {
        if (-not (Test-Path -LiteralPath (Join-Path $RunPath $relativePath) -PathType Leaf)) {
            $needsPreferenceRebuild = $true
        }
    }
}

if ($needsContentSwitch -and -not $RebuildShell) {
    if ($DryRun) {
        Write-Output "would-switch content pack to: $requestedPackRoot"
        & $mountScript `
            -PackPath $PackPath `
            -RunPath $RunPath `
            -Mode $ContentMode `
            -DryRun | Out-Host
    }
    else {
        & $mountScript `
            -PackPath $PackPath `
            -RunPath $RunPath `
            -Mode $ContentMode | Out-Host
    }
}

if ($needsPreferenceRebuild -and -not $RebuildShell) {
    if ($DryRun) {
        Write-Output "would-mount missing preference files"
    }
    else {
        & $mountScript `
            -PackPath $PackPath `
            -RunPath $RunPath `
            -Mode $ContentMode `
            -OnlyPreferences | Out-Host
    }
}

if ($needsMarkerWrite -and -not $RebuildShell -and -not $needsContentSwitch) {
    if ($DryRun) {
        Write-Output "would-write missing content pack marker"
    }
    else {
        & $mountScript `
            -PackPath $PackPath `
            -RunPath $RunPath `
            -Mode $ContentMode `
            -MarkerOnly | Out-Host
    }
}

if ($RebuildShell -or -not $runExists) {
    $newShellArgs = @{
        ShellSourcePath = $ShellSourcePath
        OutputPath = $RunPath
        PackPath = $PackPath
        ContentMode = $ContentMode
    }

    if ($Force) {
        $newShellArgs.Force = $true
    }

    if (-not $NoPreferences) {
        $newShellArgs.IncludePreferences = $true
    }

    if ($DryRun) {
        $newShellArgs.DryRun = $true
    }

    & $newShellScript @newShellArgs | Out-Host
}

if ($DryRun) {
    $resolvedRun = Join-Path (Resolve-Path -LiteralPath (Split-Path -Parent $RunPath)).Path (Split-Path -Leaf $RunPath)
}
else {
    $resolvedRun = (Resolve-Path -LiteralPath $RunPath).Path
}

if (-not $DryRun) {
    if (-not (Test-Path -LiteralPath $resolvedRun -PathType Container)) {
        throw "Runtime shell does not exist: $resolvedRun"
    }

    & $validateScript -PackPath $resolvedRun | Out-Host

    $exePath = Join-Path $resolvedRun "mc2.exe"
    if (-not (Test-Path -LiteralPath $exePath -PathType Leaf)) {
        throw "mc2.exe is missing from runtime shell: $exePath"
    }
}

$arguments = @("-nodialog")
if (-not [string]::IsNullOrWhiteSpace($Mission)) {
    $arguments += @("-mission", $Mission)
}
if ($ExtraArgs.Count -gt 0) {
    $arguments += $ExtraArgs
}

Write-Output "Runtime shell: $resolvedRun"
Write-Output "Arguments: $($arguments -join ' ')"

if ($DryRun) {
    Write-Output "Dry run complete. Remove -DryRun to start the game."
    exit 0
}

Start-Process `
    -FilePath (Join-Path $resolvedRun "mc2.exe") `
    -WorkingDirectory $resolvedRun `
    -ArgumentList $arguments

Write-Output "Started mc2.exe"
