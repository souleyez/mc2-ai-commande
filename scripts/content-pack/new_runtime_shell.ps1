param(
    [Parameter(Mandatory = $false)]
    [string]$ShellSourcePath = "",

    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "",

    [Parameter(Mandatory = $false)]
    [string]$PackPath = "",

    [Parameter(Mandatory = $false)]
    [ValidateSet("Link", "Copy")]
    [string]$ContentMode = "Link",

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$Force,

    [Parameter(Mandatory = $false)]
    [switch]$IncludeDebugSymbols,

    [Parameter(Mandatory = $false)]
    [switch]$IncludePreferences
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$mountScript = Join-Path $PSScriptRoot "mount_content_pack.ps1"
$validateScript = Join-Path $PSScriptRoot "validate_content_pack.ps1"

if ([string]::IsNullOrWhiteSpace($ShellSourcePath)) {
    $ShellSourcePath = Join-Path $repoRoot "mc2-run64-dev"
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repoRoot "runtime-shell-dev"
}

if (-not (Test-Path -LiteralPath $ShellSourcePath -PathType Container)) {
    throw "ShellSourcePath does not exist or is not a directory: $ShellSourcePath"
}

$resolvedSource = (Resolve-Path -LiteralPath $ShellSourcePath).Path
$resolvedOutputParent = Resolve-Path -LiteralPath (Split-Path -Parent $OutputPath)
$resolvedOutput = Join-Path $resolvedOutputParent.Path (Split-Path -Leaf $OutputPath)

$requiredShellFiles = @(
    "mc2.exe",
    "mc2res_64.dll",
    "SDL2.dll",
    "SDL2_mixer.dll",
    "SDL2_ttf.dll",
    "glew32.dll",
    "zlib1.dll"
)

$missingShellFiles = foreach ($relativePath in $requiredShellFiles) {
    $path = Join-Path $resolvedSource $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $relativePath
    }
}

if ($missingShellFiles.Count -gt 0) {
    Write-Output "Runtime shell source is missing required files:"
    $missingShellFiles | ForEach-Object { Write-Output "  - $_" }
    throw "Runtime shell source is incomplete."
}

if (-not [string]::IsNullOrWhiteSpace($PackPath)) {
    & $validateScript -PackPath $PackPath | Out-Host
}

Write-Output "Runtime shell source: $resolvedSource"
Write-Output "Runtime shell output: $resolvedOutput"
Write-Output "Dry run: $($DryRun.IsPresent)"
Write-Output "Force replace output: $($Force.IsPresent)"

if (Test-Path -LiteralPath $resolvedOutput) {
    if (-not $Force -and -not $DryRun) {
        throw "OutputPath already exists. Use -Force to archive it before recreating: $resolvedOutput"
    }

    if ($DryRun) {
        Write-Output "would-backup existing output directory: $resolvedOutput"
    }
    else {
        $backupScript = Join-Path $HOME ".codex\bin\Safe-RemoveToBackup.ps1"
        if (-not (Test-Path -LiteralPath $backupScript -PathType Leaf)) {
            throw "Refusing to replace output because backup helper is missing: $backupScript"
        }

        & $backupScript `
            -Path $resolvedOutput `
            -Label "mc2-runtime-shell" `
            -Reason "Recreate MC2 runtime shell output directory" | Out-Host
    }
}

foreach ($relativePath in $requiredShellFiles) {
    $source = Join-Path $resolvedSource $relativePath
    $target = Join-Path $resolvedOutput $relativePath
    if ($DryRun) {
        Write-Output ("copy-shell {0}" -f $relativePath)
    }
    else {
        if (-not (Test-Path -LiteralPath $resolvedOutput -PathType Container)) {
            New-Item -ItemType Directory -Path $resolvedOutput | Out-Null
        }
        Copy-Item -LiteralPath $source -Destination $target
    }
}

if ($IncludeDebugSymbols) {
    $debugFiles = Get-ChildItem -LiteralPath $resolvedSource -File -Filter "*.pdb"
    foreach ($debugFile in $debugFiles) {
        $target = Join-Path $resolvedOutput $debugFile.Name
        if ($DryRun) {
            Write-Output ("copy-debug {0}" -f $debugFile.Name)
        }
        else {
            Copy-Item -LiteralPath $debugFile.FullName -Destination $target
        }
    }
}

if (-not [string]::IsNullOrWhiteSpace($PackPath)) {
    if ($DryRun) {
        Write-Output "would-mount content pack: $PackPath"
        Write-Output "content mode: $ContentMode"
        if ($IncludePreferences) {
            Write-Output "would-include preferences"
        }
    }
    else {
        $mountArgs = @{
            PackPath = $PackPath
            RunPath = $resolvedOutput
            Mode = $ContentMode
        }
        if ($IncludePreferences) {
            $mountArgs.IncludePreferences = $true
        }
        & $mountScript @mountArgs | Out-Host
    }
}

if ($DryRun) {
    Write-Output "Dry run complete. Remove -DryRun to create the runtime shell."
}
else {
    Write-Output "Runtime shell ready: $resolvedOutput"
}
