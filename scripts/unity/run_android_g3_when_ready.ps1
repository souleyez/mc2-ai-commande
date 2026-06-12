param(
    [string]$RepoRoot = "",
    [string]$AdbPath = "",
    [string]$DeviceId = "",
    [int]$TimeoutSeconds = 300,
    [int]$PollSeconds = 2,
    [int]$LaunchWaitSeconds = 12,
    [switch]$PlanOnly,
    [switch]$AllowWaiting,
    [switch]$NoInstall,
    [switch]$NoLaunch,
    [switch]$SkipScreenshot,
    [switch]$SkipSummary
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

$watchScript = Join-Path $PSScriptRoot "watch_android_device_connection.ps1"
$statusScript = Join-Path $PSScriptRoot "write_android_g3_device_status.ps1"
$smokeScript = Join-Path $PSScriptRoot "android_device_smoke.ps1"

foreach ($script in @($watchScript, $statusScript, $smokeScript)) {
    if (-not (Test-Path -LiteralPath $script -PathType Leaf)) {
        throw "Missing Android G3 when-ready dependency: $script"
    }
}

function Invoke-ChildScript {
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

function Add-OptionalDeviceArgs {
    param([string[]]$Arguments)

    $result = @($Arguments)
    if (-not [string]::IsNullOrWhiteSpace($AdbPath)) {
        $result += @("-AdbPath", $AdbPath)
    }

    if (-not [string]::IsNullOrWhiteSpace($DeviceId)) {
        $result += @("-DeviceId", $DeviceId)
    }

    return $result
}

$smokeArgs = @("-RepoRoot", $RepoRoot, "-LaunchWaitSeconds", $LaunchWaitSeconds.ToString())
$smokeArgs = Add-OptionalDeviceArgs -Arguments $smokeArgs
if ($NoInstall) {
    $smokeArgs += "-NoInstall"
}
if ($NoLaunch) {
    $smokeArgs += "-NoLaunch"
}
if ($SkipScreenshot) {
    $smokeArgs += "-SkipScreenshot"
}
if ($SkipSummary) {
    $smokeArgs += "-SkipSummary"
}

if ($PlanOnly) {
    $planArgs = @($smokeArgs + "-PlanOnly")
    $plan = Invoke-ChildScript -ScriptPath $smokeScript -Arguments $planArgs
    foreach ($line in $plan.Lines) {
        Write-Host $line
    }

    if ($plan.ExitCode -ne 0) {
        throw "Android G3 when-ready plan failed with exit code $($plan.ExitCode)."
    }

    Write-Host "Android G3 when-ready plan OK."
    Write-Host "G3WhenReady: True"
    Write-Host "NoInstallOrLaunchUntilDeviceReady: True"
    Write-Host "WaitsFor: watch_android_device_connection.ps1"
    Write-Host "StatusReport: write_android_g3_device_status.ps1"
    Write-Host "SmokeScript: android_device_smoke.ps1"
    Write-Host "NextGate: G3 Run Android device smoke"
    exit 0
}

$watchArgs = @(
    "-RepoRoot",
    $RepoRoot,
    "-TimeoutSeconds",
    $TimeoutSeconds.ToString(),
    "-PollSeconds",
    $PollSeconds.ToString()
)
$watchArgs = Add-OptionalDeviceArgs -Arguments $watchArgs
if ($AllowWaiting) {
    $watchArgs += "-AllowWaiting"
}

$watch = Invoke-ChildScript -ScriptPath $watchScript -Arguments $watchArgs
$watchText = $watch.Lines -join [Environment]::NewLine
foreach ($line in $watch.Lines) {
    Write-Host $line
}

if ($watch.ExitCode -ne 0) {
    throw "Android G3 when-ready watch failed with exit code $($watch.ExitCode)."
}

if ($watchText -notlike "*Android device connection watch OK.*") {
    $statusArgs = Add-OptionalDeviceArgs -Arguments @("-RepoRoot", $RepoRoot)
    $status = Invoke-ChildScript -ScriptPath $statusScript -Arguments $statusArgs
    foreach ($line in $status.Lines) {
        Write-Host $line
    }

    Write-Host "Android G3 when-ready waiting on device."
    Write-Host "G3WhenReady: True"
    Write-Host "NoInstallOrLaunchUntilDeviceReady: True"
    Write-Host "NextGate: G3 Run Android device smoke"

    if ($AllowWaiting) {
        exit 0
    }

    throw "Android G3 when-ready cannot continue until one authorized adb device is ready."
}

$readyStatusArgs = Add-OptionalDeviceArgs -Arguments @("-RepoRoot", $RepoRoot)
$readyStatus = Invoke-ChildScript -ScriptPath $statusScript -Arguments $readyStatusArgs
foreach ($line in $readyStatus.Lines) {
    Write-Host $line
}

$smoke = Invoke-ChildScript -ScriptPath $smokeScript -Arguments $smokeArgs
foreach ($line in $smoke.Lines) {
    Write-Host $line
}

if ($smoke.ExitCode -ne 0) {
    $smokeText = $smoke.Lines -join [Environment]::NewLine
    $installPolicyBlocked = (
        $smokeText -like "*INSTALL_FAILED_USER_RESTRICTED*" -or
        $smokeText -like "*Install canceled by user*"
    )

    if ($installPolicyBlocked) {
        Write-Host "Android G3 when-ready waiting on phone install permission."
        Write-Host "G3WhenReady: True"
        Write-Host "G3DeviceReady: True"
        Write-Host "G3InstallPolicyBlocked: True"
        Write-Host "NoLaunchAfterInstallFailure: True"
        Write-Host "NextGate: G3 Run Android device smoke"

        if ($AllowWaiting) {
            exit 0
        }

        throw "Android G3 when-ready is blocked by phone-side USB install permission."
    }

    throw "Android G3 when-ready smoke failed with exit code $($smoke.ExitCode)."
}

Write-Host "Android G3 when-ready smoke OK."
Write-Host "G3WhenReady: True"
Write-Host "NextGate: G3 Run Android device smoke"
