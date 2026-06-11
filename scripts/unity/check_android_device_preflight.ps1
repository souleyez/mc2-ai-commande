param(
    [string]$RepoRoot = "",
    [string]$ApkPath = "",
    [string]$AdbPath = "",
    [string]$AaptPath = "",
    [string]$ApksignerPath = "",
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

if ([string]::IsNullOrWhiteSpace($ApksignerPath)) {
    $ApksignerPath = Join-Path $androidPlayer "SDK\build-tools\36.0.0\apksigner.bat"
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

if (-not (Test-Path -LiteralPath $ApksignerPath)) {
    Add-Failure "Missing apksigner: $ApksignerPath"
}
else {
    Add-Row -Check "apksigner" -Status "OK" -Detail $ApksignerPath
}

$sdkToolingScript = Join-Path $PSScriptRoot "check_android_sdk_tooling.ps1"
if (-not (Test-Path -LiteralPath $sdkToolingScript)) {
    Add-Failure "Missing Android SDK tooling checker: $sdkToolingScript"
}
else {
    $toolingResult = Invoke-NativeCommand -FilePath "powershell" -Arguments @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $sdkToolingScript,
        "-RepoRoot",
        $RepoRoot,
        "-AndroidPlayerPath",
        $androidPlayer
    )

    $toolingOutput = $toolingResult.Output -join " "
    if ($toolingResult.ExitCode -ne 0 -or $toolingOutput -notlike "*Android SDK tooling check OK.*") {
        Add-Failure "Android SDK tooling preflight failed: $toolingOutput"
    }
    else {
        Add-Row -Check "SDK tooling" -Status "OK" -Detail "Android SDK tooling check OK."
    }
}

$smokeArtifactHygieneScript = Join-Path $PSScriptRoot "check_android_smoke_artifact_hygiene.ps1"
if (-not (Test-Path -LiteralPath $smokeArtifactHygieneScript)) {
    Add-Failure "Missing Android smoke artifact hygiene checker: $smokeArtifactHygieneScript"
}
else {
    $hygieneResult = Invoke-NativeCommand -FilePath "powershell" -Arguments @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $smokeArtifactHygieneScript,
        "-RepoRoot",
        $RepoRoot
    )

    $hygieneOutput = $hygieneResult.Output -join " "
    if ($hygieneResult.ExitCode -ne 0 -or $hygieneOutput -notlike "*Android smoke artifact hygiene check OK.*") {
        Add-Failure "Android smoke artifact hygiene preflight failed: $hygieneOutput"
    }
    else {
        Add-Row -Check "smoke artifact hygiene" -Status "OK" -Detail "Android smoke artifact hygiene check OK."
    }
}

$smokeSummaryScript = Join-Path $PSScriptRoot "check_android_smoke_summary.ps1"
if (-not (Test-Path -LiteralPath $smokeSummaryScript)) {
    Add-Failure "Missing Android smoke summary checker: $smokeSummaryScript"
}
else {
    $summaryResult = Invoke-NativeCommand -FilePath "powershell" -Arguments @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $smokeSummaryScript,
        "-RepoRoot",
        $RepoRoot,
        "-SelfTest"
    )

    $summaryOutput = $summaryResult.Output -join " "
    if ($summaryResult.ExitCode -ne 0 -or $summaryOutput -notlike "*Android smoke summary check self-test OK.*") {
        Add-Failure "Android smoke summary schema preflight failed: $summaryOutput"
    }
    else {
        Add-Row -Check "smoke summary schema" -Status "OK" -Detail "Android smoke summary check self-test OK."
    }
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

$apkIdentityScript = Join-Path $PSScriptRoot "check_android_apk_identity.ps1"
if (-not (Test-Path -LiteralPath $apkIdentityScript)) {
    Add-Failure "Missing Android APK identity checker: $apkIdentityScript"
}
else {
    $identityResult = Invoke-NativeCommand -FilePath "powershell" -Arguments @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $apkIdentityScript,
        "-RepoRoot",
        $RepoRoot,
        "-ApkPath",
        $ApkPath,
        "-AaptPath",
        $AaptPath
    )

    $identityOutput = $identityResult.Output -join " "
    if ($identityResult.ExitCode -ne 0 -or $identityOutput -notlike "*Android APK identity check OK.*") {
        Add-Failure "Android APK identity preflight failed: $identityOutput"
    }
    else {
        Add-Row -Check "APK identity" -Status "OK" -Detail "Android APK identity check OK."
    }
}

$apkCompatibilityScript = Join-Path $PSScriptRoot "check_android_apk_compatibility.ps1"
if (-not (Test-Path -LiteralPath $apkCompatibilityScript)) {
    Add-Failure "Missing Android APK compatibility checker: $apkCompatibilityScript"
}
else {
    $compatibilityResult = Invoke-NativeCommand -FilePath "powershell" -Arguments @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $apkCompatibilityScript,
        "-RepoRoot",
        $RepoRoot,
        "-ApkPath",
        $ApkPath,
        "-AaptPath",
        $AaptPath
    )

    $compatibilityOutput = $compatibilityResult.Output -join " "
    if ($compatibilityResult.ExitCode -ne 0 -or $compatibilityOutput -notlike "*Android APK compatibility check OK.*") {
        Add-Failure "Android APK compatibility preflight failed: $compatibilityOutput"
    }
    else {
        Add-Row -Check "APK compatibility" -Status "OK" -Detail "Android APK compatibility check OK."
    }
}

$apkSigningScript = Join-Path $PSScriptRoot "check_android_apk_signing.ps1"
if (-not (Test-Path -LiteralPath $apkSigningScript)) {
    Add-Failure "Missing Android APK signing checker: $apkSigningScript"
}
else {
    $signingResult = Invoke-NativeCommand -FilePath "powershell" -Arguments @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $apkSigningScript,
        "-RepoRoot",
        $RepoRoot,
        "-ApkPath",
        $ApkPath,
        "-ApksignerPath",
        $ApksignerPath
    )

    $signingOutput = $signingResult.Output -join " "
    if ($signingResult.ExitCode -ne 0 -or $signingOutput -notlike "*Android APK signing check OK.*") {
        Add-Failure "Android APK signing preflight failed: $signingOutput"
    }
    else {
        Add-Row -Check "APK signing" -Status "OK" -Detail "Android APK signing check OK."
    }
}

$apkManifestScript = Join-Path $PSScriptRoot "check_android_apk_manifest.ps1"
if (-not (Test-Path -LiteralPath $apkManifestScript)) {
    Add-Failure "Missing Android APK manifest checker: $apkManifestScript"
}
else {
    $manifestResult = Invoke-NativeCommand -FilePath "powershell" -Arguments @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $apkManifestScript,
        "-RepoRoot",
        $RepoRoot,
        "-ApkPath",
        $ApkPath,
        "-AaptPath",
        $AaptPath
    )

    $manifestOutput = $manifestResult.Output -join " "
    if ($manifestResult.ExitCode -ne 0 -or $manifestOutput -notlike "*Android APK manifest check OK.*") {
        Add-Failure "Android APK manifest preflight failed: $manifestOutput"
    }
    else {
        Add-Row -Check "APK manifest" -Status "OK" -Detail "Android APK manifest check OK."
    }
}

$apkPayloadScript = Join-Path $PSScriptRoot "check_android_apk_payload.ps1"
if (-not (Test-Path -LiteralPath $apkPayloadScript)) {
    Add-Failure "Missing Android APK payload checker: $apkPayloadScript"
}
else {
    $payloadResult = Invoke-NativeCommand -FilePath "powershell" -Arguments @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $apkPayloadScript,
        "-RepoRoot",
        $RepoRoot,
        "-ApkPath",
        $ApkPath
    )

    $payloadOutput = $payloadResult.Output -join " "
    if ($payloadResult.ExitCode -ne 0 -or $payloadOutput -notlike "*Android APK payload check OK.*") {
        Add-Failure "Android APK payload preflight failed: $payloadOutput"
    }
    else {
        Add-Row -Check "APK payload" -Status "OK" -Detail "Android APK payload check OK."
    }
}

$apkSizeBudgetScript = Join-Path $PSScriptRoot "check_android_apk_size_budget.ps1"
if (-not (Test-Path -LiteralPath $apkSizeBudgetScript)) {
    Add-Failure "Missing Android APK size budget checker: $apkSizeBudgetScript"
}
else {
    $sizeBudgetResult = Invoke-NativeCommand -FilePath "powershell" -Arguments @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $apkSizeBudgetScript,
        "-RepoRoot",
        $RepoRoot,
        "-ApkPath",
        $ApkPath
    )

    $sizeBudgetOutput = $sizeBudgetResult.Output -join " "
    if ($sizeBudgetResult.ExitCode -ne 0 -or $sizeBudgetOutput -notlike "*Android APK size budget check OK.*") {
        Add-Failure "Android APK size budget preflight failed: $sizeBudgetOutput"
    }
    else {
        Add-Row -Check "APK size budget" -Status "OK" -Detail "Android APK size budget check OK."
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
