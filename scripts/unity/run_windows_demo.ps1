param(
    [string]$RepoRoot = "",
    [string]$ExePath = "",
    [string]$LogPath = "",
    [int]$Width = 1280,
    [int]$Height = 720,
    [switch]$NoWindowArgs,
    [switch]$CheckOnly,
    [string[]]$GameArgs = @()
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}

if ([string]::IsNullOrWhiteSpace($ExePath)) {
    $ExePath = Join-Path $RepoRoot "unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe"
}

if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $LogPath = Join-Path $RepoRoot "analysis-output\windows-demo-run.log"
}

if (-not (Test-Path -LiteralPath $ExePath)) {
    throw "Missing Windows demo build: $ExePath. Build it with MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 first."
}

$dataDir = Join-Path (Split-Path -Parent $ExePath) "MC2UnityDemo_Data"
if (-not (Test-Path -LiteralPath $dataDir)) {
    throw "Missing Unity data folder beside executable: $dataDir"
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $LogPath) | Out-Null

$arguments = New-Object System.Collections.Generic.List[string]
if (-not $NoWindowArgs) {
    $arguments.Add("-screen-width")
    $arguments.Add($Width.ToString([Globalization.CultureInfo]::InvariantCulture))
    $arguments.Add("-screen-height")
    $arguments.Add($Height.ToString([Globalization.CultureInfo]::InvariantCulture))
    $arguments.Add("-screen-fullscreen")
    $arguments.Add("0")
}

$arguments.Add("-logFile")
$arguments.Add($LogPath)
foreach ($arg in $GameArgs) {
    if (-not [string]::IsNullOrWhiteSpace($arg)) {
        $arguments.Add($arg)
    }
}

if ($CheckOnly) {
    Write-Host "Windows demo launch preflight OK."
    Write-Host "Exe: $ExePath"
    Write-Host "Data: $dataDir"
    Write-Host "Log: $LogPath"
    Write-Host "Args: $($arguments -join ' ')"
    return
}

$startInfo = [System.Diagnostics.ProcessStartInfo]::new()
$startInfo.FileName = $ExePath
$startInfo.WorkingDirectory = Split-Path -Parent $ExePath
foreach ($arg in $arguments) {
    [void]$startInfo.ArgumentList.Add($arg)
}

$process = [System.Diagnostics.Process]::Start($startInfo)
if ($null -eq $process) {
    throw "Failed to start Windows demo: $ExePath"
}

Write-Host "Windows demo started."
Write-Host "ProcessId: $($process.Id)"
Write-Host "Exe: $ExePath"
Write-Host "Log: $LogPath"
