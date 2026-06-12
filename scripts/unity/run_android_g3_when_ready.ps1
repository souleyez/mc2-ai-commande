param(
    [string]$RepoRoot = "",
    [string]$AdbPath = "",
    [string]$DeviceId = "",
    [int]$TimeoutSeconds = 300,
    [int]$PollSeconds = 2,
    [int]$LaunchWaitSeconds = 12,
    [string]$StatusPath = "",
    [switch]$PlanOnly,
    [switch]$AllowWaiting,
    [switch]$NoInstall,
    [switch]$NoLaunch,
    [switch]$SkipScreenshot,
    [switch]$SkipSummary,
    [switch]$NoWriteStatus
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ([string]::IsNullOrWhiteSpace($StatusPath)) {
    $StatusPath = Join-Path $RepoRoot "analysis-output\android-g3-when-ready-status.json"
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

function Write-WhenReadyStatus {
    param(
        [string]$Status,
        [string]$Blocker,
        [bool]$DeviceReady,
        [bool]$InstallPolicyBlocked,
        [bool]$SmokePassed,
        [string]$NextGate = "G3 Run Android device smoke",
        [object]$WatchResult = $null,
        [object]$DeviceStatusResult = $null,
        [object]$SmokeResult = $null
    )

    if ($NoWriteStatus) {
        return
    }

    $report = [ordered]@{
        reportType = "AndroidG3WhenReadyStatus"
        g3WhenReady = $true
        timestampUtc = [DateTime]::UtcNow.ToString("o")
        repoRoot = $RepoRoot
        outputPath = $StatusPath
        nextGate = $NextGate
        status = $Status
        blocker = $Blocker
        deviceReady = $DeviceReady
        installPolicyBlocked = $InstallPolicyBlocked
        smokePassed = $SmokePassed
        noLaunchAfterInstallFailure = $InstallPolicyBlocked
        watch = if ($null -eq $WatchResult) {
            $null
        }
        else {
            [ordered]@{
                exitCode = $WatchResult.ExitCode
                lines = $WatchResult.Lines
            }
        }
        deviceStatus = if ($null -eq $DeviceStatusResult) {
            $null
        }
        else {
            [ordered]@{
                exitCode = $DeviceStatusResult.ExitCode
                lines = $DeviceStatusResult.Lines
            }
        }
        smoke = if ($null -eq $SmokeResult) {
            $null
        }
        else {
            [ordered]@{
                exitCode = $SmokeResult.ExitCode
                lines = $SmokeResult.Lines
            }
        }
    }

    $parent = Split-Path -Parent $StatusPath
    if (-not [string]::IsNullOrWhiteSpace($parent) -and -not (Test-Path -LiteralPath $parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    $report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $StatusPath -Encoding UTF8
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
    Write-WhenReadyStatus `
        -Status "watchFailed" `
        -Blocker "Android G3 when-ready watch failed." `
        -DeviceReady $false `
        -InstallPolicyBlocked $false `
        -SmokePassed $false `
        -WatchResult $watch

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
    Write-Host "WhenReadyStatus: $StatusPath"
    Write-Host "NextGate: G3 Run Android device smoke"

    Write-WhenReadyStatus `
        -Status "waitingOnDevice" `
        -Blocker "Android device is not ready for G3 install." `
        -DeviceReady $false `
        -InstallPolicyBlocked $false `
        -SmokePassed $false `
        -WatchResult $watch `
        -DeviceStatusResult $status

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
        Write-Host "WhenReadyStatus: $StatusPath"
        Write-Host "NextGate: G3 Run Android device smoke"

        Write-WhenReadyStatus `
            -Status "installPolicyBlocked" `
            -Blocker "Phone rejected adb install with INSTALL_FAILED_USER_RESTRICTED." `
            -DeviceReady $true `
            -InstallPolicyBlocked $true `
            -SmokePassed $false `
            -WatchResult $watch `
            -DeviceStatusResult $readyStatus `
            -SmokeResult $smoke

        if ($AllowWaiting) {
            exit 0
        }

        throw "Android G3 when-ready is blocked by phone-side USB install permission."
    }

    Write-WhenReadyStatus `
        -Status "smokeFailed" `
        -Blocker "Android device smoke failed after the device became ready." `
        -DeviceReady $true `
        -InstallPolicyBlocked $false `
        -SmokePassed $false `
        -WatchResult $watch `
        -DeviceStatusResult $readyStatus `
        -SmokeResult $smoke

    throw "Android G3 when-ready smoke failed with exit code $($smoke.ExitCode)."
}

Write-WhenReadyStatus `
    -Status "smokePassed" `
    -Blocker "none" `
    -DeviceReady $true `
    -InstallPolicyBlocked $false `
    -SmokePassed $true `
    -NextGate "G4 Touch UI pass" `
    -WatchResult $watch `
    -DeviceStatusResult $readyStatus `
    -SmokeResult $smoke

Write-Host "Android G3 when-ready smoke OK."
Write-Host "G3WhenReady: True"
Write-Host "WhenReadyStatus: $StatusPath"
Write-Host "CompletedGate: Pass Android G3 device smoke"
Write-Host "NextGate: G4 Touch UI pass"
