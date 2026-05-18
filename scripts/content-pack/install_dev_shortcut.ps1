param(
    [Parameter(Mandatory = $false)]
    [string]$ShortcutName = "MC2 Content Pack Dev",

    [Parameter(Mandatory = $false)]
    [string]$RunPath = "",

    [Parameter(Mandatory = $false)]
    [string]$PackPath = "",

    [Parameter(Mandatory = $false)]
    [switch]$RebuildShell,

    [Parameter(Mandatory = $false)]
    [switch]$Force,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$startScript = Join-Path $PSScriptRoot "start_runtime_shell.ps1"

if ([string]::IsNullOrWhiteSpace($RunPath)) {
    $RunPath = Join-Path $repoRoot "runtime-shell-dev"
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

if (-not (Test-Path -LiteralPath $startScript -PathType Leaf)) {
    throw "Start script is missing: $startScript"
}

$desktopPath = [Environment]::GetFolderPath("Desktop")
$shortcutPath = Join-Path $desktopPath "$ShortcutName.lnk"
$powerShellPath = Join-Path $env:SystemRoot "System32\WindowsPowerShell\v1.0\powershell.exe"

$argumentParts = @(
    "-NoProfile",
    "-ExecutionPolicy", "Bypass",
    "-File", "`"$startScript`"",
    "-RunPath", "`"$RunPath`"",
    "-PackPath", "`"$PackPath`""
)

if ($RebuildShell) {
    $argumentParts += "-RebuildShell"
}

if ($Force) {
    $argumentParts += "-Force"
}

$arguments = $argumentParts -join " "
$iconPath = Join-Path $RunPath "mc2.exe"

Write-Output "Shortcut path: $shortcutPath"
Write-Output "Target: $powerShellPath"
Write-Output "Arguments: $arguments"
Write-Output "Working directory: $repoRoot"

if ($DryRun) {
    Write-Output "Dry run complete. Remove -DryRun to create the shortcut."
    exit 0
}

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $powerShellPath
$shortcut.Arguments = $arguments
$shortcut.WorkingDirectory = $repoRoot
$shortcut.Description = "Start MC2 with the local content-pack runtime shell"

if (Test-Path -LiteralPath $iconPath -PathType Leaf) {
    $shortcut.IconLocation = $iconPath
}

$shortcut.Save()

Write-Output "Shortcut installed: $shortcutPath"
