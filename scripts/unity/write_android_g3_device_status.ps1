param(
    [string]$RepoRoot = "",
    [string]$OutputPath = "",
    [string]$AdbPath = "",
    [string]$DeviceId = "",
    [switch]$NoWrite
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $RepoRoot "analysis-output\android-g3-device-status.json"
}

$connectionScript = Join-Path $PSScriptRoot "check_android_device_connection.ps1"
$watchScript = Join-Path $PSScriptRoot "watch_android_device_connection.ps1"
$requirementScript = Join-Path $PSScriptRoot "check_android_g3_device_requirement.ps1"

foreach ($script in @($connectionScript, $watchScript, $requirementScript)) {
    if (-not (Test-Path -LiteralPath $script -PathType Leaf)) {
        throw "Missing required Android G3 device status dependency: $script"
    }
}

function Invoke-StatusScript {
    param(
        [string]$ScriptPath,
        [string[]]$Arguments
    )

    $processArgs = @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $ScriptPath
    ) + $Arguments

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $output = & powershell @processArgs 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    return [pscustomobject]@{
        ExitCode = $exitCode
        Lines = @($output | ForEach-Object { $_.ToString() })
    }
}

$connectionArgs = @("-RepoRoot", $RepoRoot)
$watchArgs = @("-RepoRoot", $RepoRoot, "-Once", "-AllowWaiting")

if (-not [string]::IsNullOrWhiteSpace($AdbPath)) {
    $connectionArgs += @("-AdbPath", $AdbPath)
    $watchArgs += @("-AdbPath", $AdbPath)
}

if (-not [string]::IsNullOrWhiteSpace($DeviceId)) {
    $connectionArgs += @("-DeviceId", $DeviceId)
    $watchArgs += @("-DeviceId", $DeviceId)
}

$connection = Invoke-StatusScript -ScriptPath $connectionScript -Arguments $connectionArgs
$watch = Invoke-StatusScript -ScriptPath $watchScript -Arguments $watchArgs
$connectionText = $connection.Lines -join [Environment]::NewLine
$watchText = $watch.Lines -join [Environment]::NewLine

$connectionBlocked = (
    $connectionText -like "*WpdOnlyAndroidDevice: True*" -or
    $connectionText -like "*unauthorized*" -or
    $connectionText -like "*offline*" -or
    $connectionText -like "*no device rows*" -or
    $watchText -like "*DeviceReady: False*"
)

if ($connectionBlocked) {
    $requirement = [pscustomobject]@{
        ExitCode = 0
        Lines = @(
            "Android G3 device requirement check waiting on device.",
            "Repo: $RepoRoot",
            "Detail: skipped strict G3 readiness because adb has no authorized device yet."
        )
    }
}
else {
    $requirement = Invoke-StatusScript -ScriptPath $requirementScript -Arguments @("-RepoRoot", $RepoRoot)
}

$requirementText = $requirement.Lines -join [Environment]::NewLine

$deviceReady = (
    $connectionText -like "*Android device connection check OK.*" -and
    $watchText -like "*Android device connection watch OK.*" -and
    $requirementText -like "*Android G3 device requirement check OK.*"
)

$blocker = ""
if ($deviceReady) {
    $blocker = "none"
}
elseif ($connectionText -like "*WpdOnlyAndroidDevice: True*") {
    $blocker = "Windows sees an Android phone as WPD/MTP only, but adb has no authorized device row."
}
elseif ($connectionText -like "*unauthorized*") {
    $blocker = "Android device is connected but USB debugging authorization is not accepted."
}
elseif ($connectionText -like "*offline*") {
    $blocker = "Android device is visible to adb but offline."
}
elseif ($connectionText -like "*no device rows*") {
    $blocker = "adb returned no Android device rows."
}
else {
    $blocker = "G3 strict device requirement is not ready. See raw helper output."
}

$status = if ($deviceReady) { "ready" } else { "waiting" }
$timestampUtc = [DateTime]::UtcNow.ToString("o")

$report = [ordered]@{
    reportType = "AndroidG3DeviceStatus"
    g3DeviceStatusReport = $true
    timestampUtc = $timestampUtc
    repoRoot = $RepoRoot
    outputPath = $OutputPath
    nextGate = "G3 Run Android device smoke"
    noInstallOrLaunch = $true
    status = $status
    deviceReady = $deviceReady
    blocker = $blocker
    connection = [ordered]@{
        exitCode = $connection.ExitCode
        lines = $connection.Lines
    }
    watch = [ordered]@{
        exitCode = $watch.ExitCode
        lines = $watch.Lines
    }
    deviceRequirement = [ordered]@{
        exitCode = $requirement.ExitCode
        lines = $requirement.Lines
    }
}

if (-not $NoWrite) {
    $parent = Split-Path -Parent $OutputPath
    if (-not [string]::IsNullOrWhiteSpace($parent) -and -not (Test-Path -LiteralPath $parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    $report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $OutputPath -Encoding UTF8
}

Write-Host "Android G3 device status report OK."
Write-Host "G3DeviceStatusReport: True"
Write-Host "G3DeviceReady: $deviceReady"
Write-Host "G3DeviceStatus: $status"
Write-Host "NoInstallOrLaunch: True"
Write-Host "NextGate: G3 Run Android device smoke"
Write-Host "Report: $OutputPath"
Write-Host "Blocker: $blocker"
