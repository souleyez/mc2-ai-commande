param(
    [string]$RepoRoot = "",
    [string]$ApkPath = "",
    [string]$AdbPath = "",
    [string]$AaptPath = "",
    [string]$PackageName = "",
    [string]$ActivityName = "",
    [string]$DeviceId = "",
    [string]$LogPath = "",
    [int]$LaunchWaitSeconds = 12,
    [switch]$NoInstall,
    [switch]$NoLaunch,
    [switch]$SkipLogCheck,
    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}

if ([string]::IsNullOrWhiteSpace($ApkPath)) {
    $ApkPath = Join-Path $RepoRoot "unity-mc2-demo\Builds\Android\MC2UnityDemo.apk"
}

$androidPlayer = Join-Path $HOME "Unity\Hub\Editor\6000.4.7f1\Editor\Data\PlaybackEngines\AndroidPlayer"
if ([string]::IsNullOrWhiteSpace($AdbPath)) {
    $AdbPath = Join-Path $androidPlayer "SDK\platform-tools\adb.exe"
}

if ([string]::IsNullOrWhiteSpace($AaptPath)) {
    $AaptPath = Join-Path $androidPlayer "SDK\build-tools\36.0.0\aapt.exe"
}

if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $LogPath = Join-Path $RepoRoot "analysis-output\android-device-smoke.log"
}

if (-not (Test-Path -LiteralPath $ApkPath)) {
    throw "Missing Android APK: $ApkPath"
}

if (-not (Test-Path -LiteralPath $AdbPath)) {
    throw "Missing adb: $AdbPath"
}

$apkFreshnessScript = Join-Path $PSScriptRoot "check_android_apk_freshness.ps1"
if (-not (Test-Path -LiteralPath $apkFreshnessScript)) {
    throw "Missing Android APK freshness checker: $apkFreshnessScript"
}

$freshnessOutput = & powershell -NoProfile -ExecutionPolicy Bypass -File $apkFreshnessScript -RepoRoot $RepoRoot -ApkPath $ApkPath 2>&1
if ($LASTEXITCODE -ne 0) {
    foreach ($line in @($freshnessOutput | ForEach-Object { $_.ToString() })) {
        Write-Host $line
    }

    throw "Android APK freshness check failed with exit code $LASTEXITCODE"
}

$apkIdentityScript = Join-Path $PSScriptRoot "check_android_apk_identity.ps1"
if (-not (Test-Path -LiteralPath $apkIdentityScript)) {
    throw "Missing Android APK identity checker: $apkIdentityScript"
}

$identityOutput = & powershell -NoProfile -ExecutionPolicy Bypass -File $apkIdentityScript -RepoRoot $RepoRoot -ApkPath $ApkPath -AaptPath $AaptPath 2>&1
if ($LASTEXITCODE -ne 0) {
    foreach ($line in @($identityOutput | ForEach-Object { $_.ToString() })) {
        Write-Host $line
    }

    throw "Android APK identity check failed with exit code $LASTEXITCODE"
}

function Get-ApkMetadata {
    param(
        [string]$Apk,
        [string]$Aapt
    )

    $metadata = @{
        PackageName = ""
        ActivityName = ""
    }

    if (-not (Test-Path -LiteralPath $Aapt)) {
        return $metadata
    }

    $badging = & $Aapt dump badging $Apk
    foreach ($line in $badging) {
        if ([string]::IsNullOrWhiteSpace($metadata.PackageName) -and $line -match "package: name='([^']+)'") {
            $metadata.PackageName = $Matches[1]
        }

        if ([string]::IsNullOrWhiteSpace($metadata.ActivityName) -and $line -match "launchable-activity: name='([^']+)'") {
            $metadata.ActivityName = $Matches[1]
        }
    }

    return $metadata
}

function Get-AdbDevice {
    param(
        [string]$Adb,
        [string]$RequestedDeviceId
    )

    $deviceLines = & $Adb devices
    $devices = @()
    foreach ($line in $deviceLines) {
        if ($line -match "^(\S+)\s+(device|unauthorized|offline)$") {
            $devices += [pscustomobject]@{
                Id = $Matches[1]
                State = $Matches[2]
            }
        }
    }

    if ($devices.Count -eq 0) {
        throw "No Android device found. Connect a phone, enable USB debugging, authorize this PC, then rerun this script."
    }

    if (-not [string]::IsNullOrWhiteSpace($RequestedDeviceId)) {
        $match = $devices | Where-Object { $_.Id -eq $RequestedDeviceId } | Select-Object -First 1
        if ($null -eq $match) {
            throw "Requested Android device was not found: $RequestedDeviceId"
        }

        if ($match.State -ne "device") {
            throw "Requested Android device is $($match.State): $RequestedDeviceId"
        }

        return $match.Id
    }

    $ready = @($devices | Where-Object { $_.State -eq "device" })
    if ($ready.Count -eq 0) {
        $summary = ($devices | ForEach-Object { "$($_.Id):$($_.State)" }) -join ", "
        throw "No authorized Android device found. Current adb states: $summary"
    }

    if ($ready.Count -gt 1) {
        $summary = ($ready | ForEach-Object { $_.Id }) -join ", "
        throw "Multiple Android devices found. Pass -DeviceId. Devices: $summary"
    }

    return $ready[0].Id
}

$metadata = Get-ApkMetadata -Apk $ApkPath -Aapt $AaptPath
if ([string]::IsNullOrWhiteSpace($PackageName)) {
    $PackageName = $metadata.PackageName
}

if ([string]::IsNullOrWhiteSpace($ActivityName)) {
    $ActivityName = $metadata.ActivityName
}

if ([string]::IsNullOrWhiteSpace($PackageName)) {
    throw "PackageName is unknown. Pass -PackageName or ensure aapt can read the APK."
}

if ($PlanOnly) {
    Write-Host "Android device smoke plan OK."
    Write-Host "APK: $ApkPath"
    Write-Host "adb: $AdbPath"
    Write-Host "aapt: $AaptPath"
    Write-Host "Package: $PackageName"
    if (-not [string]::IsNullOrWhiteSpace($ActivityName)) {
        Write-Host "Activity: $ActivityName"
    }
    else {
        Write-Host "Activity: fallback monkey launch"
    }
    Write-Host "Log: $LogPath"
    Write-Host "Install: $(-not $NoInstall)"
    Write-Host "Launch: $(-not $NoLaunch)"
    Write-Host "LogCheck: $(-not $SkipLogCheck)"
    return
}

$device = Get-AdbDevice -Adb $AdbPath -RequestedDeviceId $DeviceId
$adbArgs = @("-s", $device)

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $LogPath) | Out-Null

$model = (& $AdbPath @adbArgs shell getprop ro.product.model) -join " "
$androidVersion = (& $AdbPath @adbArgs shell getprop ro.build.version.release) -join " "

if (-not $NoInstall) {
    & $AdbPath @adbArgs install -r $ApkPath
    if ($LASTEXITCODE -ne 0) {
        throw "adb install failed with exit code $LASTEXITCODE"
    }
}

& $AdbPath @adbArgs logcat -c
if (-not $NoLaunch) {
    if (-not [string]::IsNullOrWhiteSpace($ActivityName)) {
        & $AdbPath @adbArgs shell am start -n "$PackageName/$ActivityName"
    }
    else {
        & $AdbPath @adbArgs shell monkey -p $PackageName -c android.intent.category.LAUNCHER 1
    }

    if ($LASTEXITCODE -ne 0) {
        throw "adb launch failed with exit code $LASTEXITCODE"
    }

    Start-Sleep -Seconds $LaunchWaitSeconds
}

& $AdbPath @adbArgs logcat -d > $LogPath
$pidOutput = (& $AdbPath @adbArgs shell pidof $PackageName) -join " "

if (-not $SkipLogCheck) {
    $logCheckScript = Join-Path $PSScriptRoot "check_android_smoke_log.ps1"
    & powershell -NoProfile -ExecutionPolicy Bypass -File $logCheckScript -LogPath $LogPath -PackageName $PackageName
    if ($LASTEXITCODE -ne 0) {
        throw "Android smoke log check failed with exit code $LASTEXITCODE"
    }
}

Write-Host "Android device smoke complete."
Write-Host "Device: $device"
Write-Host "Model: $model"
Write-Host "Android: $androidVersion"
Write-Host "Package: $PackageName"
if (-not [string]::IsNullOrWhiteSpace($ActivityName)) {
    Write-Host "Activity: $ActivityName"
}
Write-Host "Process: $pidOutput"
Write-Host "Log: $LogPath"

if ([string]::IsNullOrWhiteSpace($pidOutput)) {
    throw "Package did not remain running after launch: $PackageName"
}
