param(
    [string]$RepoRoot = "",
    [string]$ApkPath = "",
    [string]$AdbPath = "",
    [string]$AaptPath = "",
    [string]$DeviceId = "",
    [switch]$AllowNoDevice
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
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

$failures = New-Object System.Collections.Generic.List[string]
$warnings = New-Object System.Collections.Generic.List[string]
$rows = New-Object System.Collections.Generic.List[object]

function Add-Failure {
    param([string]$Message)
    [void]$failures.Add($Message)
}

function Add-Warning {
    param([string]$Message)
    [void]$warnings.Add($Message)
}

function Add-Row {
    param(
        [string]$Check,
        [string]$Status,
        [string]$Detail
    )

    [void]$rows.Add([pscustomobject]@{
        Check = $Check
        Status = $Status
        Detail = $Detail
    })
}

function Invoke-NativeCommand {
    param(
        [string]$FilePath,
        [string[]]$Arguments
    )

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $output = & $FilePath @Arguments 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    return [pscustomobject]@{
        ExitCode = $exitCode
        Output = @($output | ForEach-Object { $_.ToString() })
    }
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

    $badgingResult = Invoke-NativeCommand -FilePath $Aapt -Arguments @("dump", "badging", $Apk)
    $badging = $badgingResult.Output
    if ($badgingResult.ExitCode -ne 0) {
        Add-Failure "aapt could not read APK badging: $($badging -join ' ')"
        return $metadata
    }

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

function Get-AdbDevices {
    param([string]$Adb)

    $deviceResult = Invoke-NativeCommand -FilePath $Adb -Arguments @("devices")
    $deviceLines = $deviceResult.Output
    if ($deviceResult.ExitCode -ne 0) {
        Add-Failure "adb devices failed: $($deviceLines -join ' ')"
        return @()
    }

    $devices = @()
    foreach ($line in $deviceLines) {
        if ($line -match "^(\S+)\s+(device|unauthorized|offline)$") {
            $devices += [pscustomobject]@{
                Id = $Matches[1]
                State = $Matches[2]
            }
        }
    }

    return $devices
}

if (-not (Test-Path -LiteralPath $ApkPath)) {
    Add-Failure "Missing Android APK: $ApkPath"
}
else {
    $apk = Get-Item -LiteralPath $ApkPath
    Add-Row -Check "APK" -Status "OK" -Detail ("{0} bytes" -f $apk.Length)
}

if (-not (Test-Path -LiteralPath $AdbPath)) {
    Add-Failure "Missing adb: $AdbPath"
}
else {
    $adbVersionResult = Invoke-NativeCommand -FilePath $AdbPath -Arguments @("version")
    $adbVersion = ($adbVersionResult.Output | Select-Object -First 2) -join "; "
    Add-Row -Check "adb" -Status "OK" -Detail $adbVersion
}

if (-not (Test-Path -LiteralPath $AaptPath)) {
    Add-Failure "Missing aapt: $AaptPath"
}
else {
    $aaptVersionResult = Invoke-NativeCommand -FilePath $AaptPath -Arguments @("version")
    $aaptVersion = ($aaptVersionResult.Output | Select-Object -First 1) -join " "
    Add-Row -Check "aapt" -Status "OK" -Detail $aaptVersion
}

if ((Test-Path -LiteralPath $ApkPath) -and (Test-Path -LiteralPath $AaptPath)) {
    $metadata = Get-ApkMetadata -Apk $ApkPath -Aapt $AaptPath
    if ([string]::IsNullOrWhiteSpace($metadata.PackageName)) {
        Add-Failure "APK package name could not be discovered. Rebuild the APK or pass package data to android_device_smoke.ps1."
    }
    else {
        Add-Row -Check "package" -Status "OK" -Detail $metadata.PackageName
    }

    if ([string]::IsNullOrWhiteSpace($metadata.ActivityName)) {
        Add-Warning "APK launchable activity could not be discovered; android_device_smoke.ps1 will fall back to monkey launch if PackageName is known."
    }
    else {
        Add-Row -Check "activity" -Status "OK" -Detail $metadata.ActivityName
    }
}

$apkFreshnessScript = Join-Path $PSScriptRoot "check_android_apk_freshness.ps1"
if (-not (Test-Path -LiteralPath $apkFreshnessScript)) {
    Add-Failure "Missing Android APK freshness checker: $apkFreshnessScript"
}
else {
    $freshnessResult = Invoke-NativeCommand -FilePath "powershell" -Arguments @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $apkFreshnessScript,
        "-RepoRoot",
        $RepoRoot,
        "-ApkPath",
        $ApkPath
    )

    $freshnessOutput = $freshnessResult.Output -join " "
    if ($freshnessResult.ExitCode -ne 0 -or $freshnessOutput -notlike "*Android APK freshness check OK.*") {
        Add-Failure "Android APK freshness preflight failed: $freshnessOutput"
    }
    else {
        Add-Row -Check "APK freshness" -Status "OK" -Detail "Android APK freshness check OK."
    }
}

if (Test-Path -LiteralPath $AdbPath) {
    $devices = @(Get-AdbDevices -Adb $AdbPath)
    if ($devices.Count -eq 0) {
        $message = "No Android device rows. Connect a phone, enable USB debugging, authorize this PC, then run android_device_smoke.ps1."
        if ($AllowNoDevice) {
            Add-Warning $message
            Add-Row -Check "device" -Status "WAITING" -Detail "no adb device rows"
        }
        else {
            Add-Failure $message
        }
    }
    else {
        $summary = ($devices | ForEach-Object { "$($_.Id):$($_.State)" }) -join ", "
        Add-Row -Check "adb devices" -Status "OK" -Detail $summary

        if (-not [string]::IsNullOrWhiteSpace($DeviceId)) {
            $requested = $devices | Where-Object { $_.Id -eq $DeviceId } | Select-Object -First 1
            if ($null -eq $requested) {
                Add-Failure "Requested Android device was not found: $DeviceId"
            }
            elseif ($requested.State -ne "device") {
                Add-Failure "Requested Android device is $($requested.State): $DeviceId"
            }
            else {
                Add-Row -Check "selected device" -Status "OK" -Detail $requested.Id
            }
        }
        else {
            $ready = @($devices | Where-Object { $_.State -eq "device" })
            if ($ready.Count -eq 0) {
                Add-Failure "No authorized Android device found. Current adb states: $summary"
            }
            elseif ($ready.Count -gt 1) {
                Add-Failure "Multiple authorized Android devices found. Pass -DeviceId. Devices: $(($ready | ForEach-Object { $_.Id }) -join ', ')"
            }
            else {
                Add-Row -Check "selected device" -Status "OK" -Detail $ready[0].Id
            }
        }
    }
}

foreach ($warning in $warnings) {
    Write-Host "Warning: $warning"
}

if ($failures.Count -gt 0) {
    Write-Host "Android device smoke preflight failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android device smoke preflight check(s) failed."
}

if ($AllowNoDevice -and $warnings.Count -gt 0) {
    Write-Host "Android device smoke preflight waiting on device."
}
else {
    Write-Host "Android device smoke preflight OK."
}

Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
