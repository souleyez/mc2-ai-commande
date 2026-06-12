param(
    [string]$RepoRoot = "",
    [string]$AdbPath = "",
    [string]$DeviceId = "",
    [int]$TimeoutSeconds = 60,
    [int]$PollSeconds = 2,
    [switch]$Once,
    [switch]$AllowWaiting
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

$connectionScript = Join-Path $PSScriptRoot "check_android_device_connection.ps1"
if (-not (Test-Path -LiteralPath $connectionScript -PathType Leaf)) {
    throw "Missing Android device connection checker: $connectionScript"
}

if ($TimeoutSeconds -lt 0) {
    throw "TimeoutSeconds must be greater than or equal to 0."
}

if ($PollSeconds -lt 1) {
    throw "PollSeconds must be greater than or equal to 1."
}

function Invoke-ConnectionCheck {
    $arguments = @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $connectionScript,
        "-RepoRoot",
        $RepoRoot
    )

    if (-not [string]::IsNullOrWhiteSpace($AdbPath)) {
        $arguments += @("-AdbPath", $AdbPath)
    }

    if (-not [string]::IsNullOrWhiteSpace($DeviceId)) {
        $arguments += @("-DeviceId", $DeviceId)
    }

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $output = & powershell @arguments 2>&1
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

$deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
$attempt = 0
$lastResult = $null

do {
    $attempt += 1
    $result = Invoke-ConnectionCheck
    $lastResult = $result
    $joined = $result.Lines -join [Environment]::NewLine

    if ($joined -like "*Android device connection check OK.*") {
        Write-Host "Android device connection watch OK."
        Write-Host "AdbWatchHint: True; Attempts: $attempt; DeviceReady: True"
        foreach ($line in $result.Lines) {
            Write-Host $line
        }
        exit 0
    }

    if ($Once) {
        break
    }

    if ([DateTime]::UtcNow -lt $deadline) {
        Start-Sleep -Seconds $PollSeconds
    }
} while ([DateTime]::UtcNow -lt $deadline)

Write-Host "Android device connection watch waiting on device."
Write-Host "AdbWatchHint: True; Attempts: $attempt; DeviceReady: False; TimeoutSeconds: $TimeoutSeconds"
if ($null -ne $lastResult) {
    foreach ($line in $lastResult.Lines) {
        Write-Host $line
    }
}

if ($AllowWaiting) {
    exit 0
}

throw "Android device connection watch timed out before one authorized adb device became ready."
