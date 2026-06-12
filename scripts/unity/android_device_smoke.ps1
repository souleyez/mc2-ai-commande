param(
    [string]$RepoRoot = "",
    [string]$ApkPath = "",
    [string]$AdbPath = "",
    [string]$AaptPath = "",
    [string]$ApksignerPath = "",
    [string]$PackageName = "",
    [string]$ActivityName = "",
    [string]$DeviceId = "",
    [string]$LogPath = "",
    [string]$ScreenshotPath = "",
    [string]$SummaryPath = "",
    [string]$CommandFilePath = "",
    [string]$DeviceCommandFilePath = "",
    [int]$LaunchWaitSeconds = 12,
    [switch]$NoInstall,
    [switch]$NoLaunch,
    [switch]$NoCommandFileSmoke,
    [switch]$SkipLogCheck,
    [switch]$SkipScreenshot,
    [switch]$SkipSummary,
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

if ([string]::IsNullOrWhiteSpace($ApksignerPath)) {
    $ApksignerPath = Join-Path $androidPlayer "SDK\build-tools\36.0.0\apksigner.bat"
}

if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $LogPath = Join-Path $RepoRoot "analysis-output\android-device-smoke.log"
}

if ([string]::IsNullOrWhiteSpace($ScreenshotPath)) {
    $ScreenshotPath = Join-Path $RepoRoot "analysis-output\android-device-smoke.png"
}

if ([string]::IsNullOrWhiteSpace($SummaryPath)) {
    $SummaryPath = Join-Path $RepoRoot "analysis-output\android-device-smoke-summary.json"
}

if ([string]::IsNullOrWhiteSpace($CommandFilePath)) {
    $CommandFilePath = Join-Path $RepoRoot "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt"
}
elseif (-not [System.IO.Path]::IsPathRooted($CommandFilePath)) {
    $CommandFilePath = Join-Path $RepoRoot $CommandFilePath
}

if (-not (Test-Path -LiteralPath $ApkPath)) {
    throw "Missing Android APK: $ApkPath"
}

if (-not (Test-Path -LiteralPath $AdbPath)) {
    throw "Missing adb: $AdbPath"
}

if (-not $NoCommandFileSmoke -and -not (Test-Path -LiteralPath $CommandFilePath -PathType Leaf)) {
    throw "Missing Android smoke command file: $CommandFilePath"
}

$sdkToolingScript = Join-Path $PSScriptRoot "check_android_sdk_tooling.ps1"
if (-not (Test-Path -LiteralPath $sdkToolingScript)) {
    throw "Missing Android SDK tooling checker: $sdkToolingScript"
}

$toolingOutput = & powershell -NoProfile -ExecutionPolicy Bypass -File $sdkToolingScript -RepoRoot $RepoRoot -AndroidPlayerPath $androidPlayer 2>&1
if ($LASTEXITCODE -ne 0) {
    foreach ($line in @($toolingOutput | ForEach-Object { $_.ToString() })) {
        Write-Host $line
    }

    throw "Android SDK tooling check failed with exit code $LASTEXITCODE"
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

$apkCompatibilityScript = Join-Path $PSScriptRoot "check_android_apk_compatibility.ps1"
if (-not (Test-Path -LiteralPath $apkCompatibilityScript)) {
    throw "Missing Android APK compatibility checker: $apkCompatibilityScript"
}

$compatibilityOutput = & powershell -NoProfile -ExecutionPolicy Bypass -File $apkCompatibilityScript -RepoRoot $RepoRoot -ApkPath $ApkPath -AaptPath $AaptPath 2>&1
if ($LASTEXITCODE -ne 0) {
    foreach ($line in @($compatibilityOutput | ForEach-Object { $_.ToString() })) {
        Write-Host $line
    }

    throw "Android APK compatibility check failed with exit code $LASTEXITCODE"
}

$apkSigningScript = Join-Path $PSScriptRoot "check_android_apk_signing.ps1"
if (-not (Test-Path -LiteralPath $apkSigningScript)) {
    throw "Missing Android APK signing checker: $apkSigningScript"
}

$signingOutput = & powershell -NoProfile -ExecutionPolicy Bypass -File $apkSigningScript -RepoRoot $RepoRoot -ApkPath $ApkPath -ApksignerPath $ApksignerPath 2>&1
if ($LASTEXITCODE -ne 0) {
    foreach ($line in @($signingOutput | ForEach-Object { $_.ToString() })) {
        Write-Host $line
    }

    throw "Android APK signing check failed with exit code $LASTEXITCODE"
}

$apkManifestScript = Join-Path $PSScriptRoot "check_android_apk_manifest.ps1"
if (-not (Test-Path -LiteralPath $apkManifestScript)) {
    throw "Missing Android APK manifest checker: $apkManifestScript"
}

$manifestOutput = & powershell -NoProfile -ExecutionPolicy Bypass -File $apkManifestScript -RepoRoot $RepoRoot -ApkPath $ApkPath -AaptPath $AaptPath 2>&1
if ($LASTEXITCODE -ne 0) {
    foreach ($line in @($manifestOutput | ForEach-Object { $_.ToString() })) {
        Write-Host $line
    }

    throw "Android APK manifest check failed with exit code $LASTEXITCODE"
}

$apkPayloadScript = Join-Path $PSScriptRoot "check_android_apk_payload.ps1"
if (-not (Test-Path -LiteralPath $apkPayloadScript)) {
    throw "Missing Android APK payload checker: $apkPayloadScript"
}

$payloadOutput = & powershell -NoProfile -ExecutionPolicy Bypass -File $apkPayloadScript -RepoRoot $RepoRoot -ApkPath $ApkPath 2>&1
if ($LASTEXITCODE -ne 0) {
    foreach ($line in @($payloadOutput | ForEach-Object { $_.ToString() })) {
        Write-Host $line
    }

    throw "Android APK payload check failed with exit code $LASTEXITCODE"
}

$apkSizeBudgetScript = Join-Path $PSScriptRoot "check_android_apk_size_budget.ps1"
if (-not (Test-Path -LiteralPath $apkSizeBudgetScript)) {
    throw "Missing Android APK size budget checker: $apkSizeBudgetScript"
}

$sizeBudgetOutput = & powershell -NoProfile -ExecutionPolicy Bypass -File $apkSizeBudgetScript -RepoRoot $RepoRoot -ApkPath $ApkPath 2>&1
if ($LASTEXITCODE -ne 0) {
    foreach ($line in @($sizeBudgetOutput | ForEach-Object { $_.ToString() })) {
        Write-Host $line
    }

    throw "Android APK size budget check failed with exit code $LASTEXITCODE"
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

function Join-ProcessArguments {
    param([string[]]$Arguments)

    $quoted = foreach ($argument in $Arguments) {
        if ($argument -match '[\s"]') {
            '"' + ($argument -replace '"', '\"') + '"'
        }
        else {
            $argument
        }
    }

    return ($quoted -join " ")
}

function Invoke-BinaryCommandToFile {
    param(
        [string]$FilePath,
        [string[]]$Arguments,
        [string]$OutputPath
    )

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath) | Out-Null

    $processInfo = New-Object System.Diagnostics.ProcessStartInfo
    $processInfo.FileName = $FilePath
    $processInfo.Arguments = Join-ProcessArguments -Arguments $Arguments
    $processInfo.RedirectStandardOutput = $true
    $processInfo.RedirectStandardError = $true
    $processInfo.UseShellExecute = $false
    $processInfo.CreateNoWindow = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $processInfo

    $fileStream = [System.IO.File]::Open($OutputPath, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write)
    try {
        [void]$process.Start()
        $process.StandardOutput.BaseStream.CopyTo($fileStream)
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()
        $exitCode = $process.ExitCode
    }
    finally {
        $fileStream.Dispose()
        $process.Dispose()
    }

    if ($exitCode -ne 0) {
        throw "Binary command failed with exit code $($exitCode): $stderr"
    }
}

function Write-SmokeSummary {
    param(
        [string]$Path,
        [object]$Data
    )

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
    $Data | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Get-AndroidShellDirectory {
    param([string]$Path)

    $normalized = $Path -replace "\\", "/"
    $lastSlash = $normalized.LastIndexOf("/")
    if ($lastSlash -le 0) {
        return ""
    }

    return $normalized.Substring(0, $lastSlash)
}

function Test-LogContains {
    param(
        [string]$Path,
        [string]$Needle
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $false
    }

    $text = Get-Content -LiteralPath $Path -Raw
    return $text -like "*$Needle*"
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

$commandFileSmoke = -not $NoCommandFileSmoke
if ($commandFileSmoke -and [string]::IsNullOrWhiteSpace($DeviceCommandFilePath)) {
    $DeviceCommandFilePath = "/sdcard/Android/data/$PackageName/files/" + [System.IO.Path]::GetFileName($CommandFilePath)
}

$unityArguments = ""
if ($commandFileSmoke) {
    if ([string]::IsNullOrWhiteSpace($ActivityName)) {
        throw "Android command-file smoke requires a launchable activity. Pass -ActivityName or rebuild the APK with a launchable activity."
    }

    $unityArguments = "-mc2CommandFile $DeviceCommandFilePath"
}

if ($PlanOnly) {
    Write-Host "Android device smoke plan OK."
    Write-Host "APK: $ApkPath"
    Write-Host "adb: $AdbPath"
    Write-Host "aapt: $AaptPath"
    Write-Host "apksigner: $ApksignerPath"
    Write-Host "Package: $PackageName"
    if (-not [string]::IsNullOrWhiteSpace($ActivityName)) {
        Write-Host "Activity: $ActivityName"
    }
    else {
        Write-Host "Activity: fallback monkey launch"
    }
    Write-Host "Log: $LogPath"
    Write-Host "Screenshot: $ScreenshotPath"
    Write-Host "Summary: $SummaryPath"
    Write-Host "CommandFileSmoke: $commandFileSmoke"
    if ($commandFileSmoke) {
        Write-Host "CommandFile: $CommandFilePath"
        Write-Host "DeviceCommandFile: $DeviceCommandFilePath"
        Write-Host "UnityArguments: $unityArguments"
        Write-Host "SmokeSuccessMarker: MC2 debrief summary assertion OK"
        Write-Host "SmokeSuccessMarker: MC2 loadout compact assertion OK"
    }
    Write-Host "Install: $(-not $NoInstall)"
    Write-Host "Launch: $(-not $NoLaunch)"
    Write-Host "LogCheck: $(-not $SkipLogCheck)"
    Write-Host "ScreenshotCapture: $(-not $SkipScreenshot)"
    Write-Host "SummaryWrite: $(-not $SkipSummary)"
    Write-Host "ConnectionCheck: check_android_device_connection.ps1 -RequireDevice"
    return
}

$deviceConnectionScript = Join-Path $PSScriptRoot "check_android_device_connection.ps1"
if (-not (Test-Path -LiteralPath $deviceConnectionScript)) {
    throw "Missing Android device connection checker: $deviceConnectionScript"
}

$deviceConnectionArgs = @(
    "-RepoRoot",
    $RepoRoot,
    "-AdbPath",
    $AdbPath,
    "-RequireDevice"
)
if (-not [string]::IsNullOrWhiteSpace($DeviceId)) {
    $deviceConnectionArgs += @("-DeviceId", $DeviceId)
}

$previousErrorActionPreference = $ErrorActionPreference
$ErrorActionPreference = "Continue"
try {
    $deviceConnectionOutput = & powershell -NoProfile -ExecutionPolicy Bypass -File $deviceConnectionScript @deviceConnectionArgs 2>&1
    $deviceConnectionExitCode = $LASTEXITCODE
}
finally {
    $ErrorActionPreference = $previousErrorActionPreference
}

$deviceConnectionLines = @($deviceConnectionOutput | ForEach-Object { $_.ToString() })
$deviceConnectionText = $deviceConnectionLines -join [Environment]::NewLine
if ($deviceConnectionExitCode -ne 0 -or $deviceConnectionText -notlike "*Android device connection check OK.*") {
    foreach ($line in $deviceConnectionLines) {
        Write-Host $line
    }

    throw "Android device smoke requires a single authorized Android device before install or launch."
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

if ($commandFileSmoke) {
    $deviceCommandFileDirectory = Get-AndroidShellDirectory -Path $DeviceCommandFilePath
    if ([string]::IsNullOrWhiteSpace($deviceCommandFileDirectory)) {
        throw "Device command file path must include a directory: $DeviceCommandFilePath"
    }

    & $AdbPath @adbArgs shell mkdir -p $deviceCommandFileDirectory
    if ($LASTEXITCODE -ne 0) {
        throw "adb shell mkdir failed for Android smoke command file directory: $deviceCommandFileDirectory"
    }

    & $AdbPath @adbArgs push $CommandFilePath $DeviceCommandFilePath
    if ($LASTEXITCODE -ne 0) {
        throw "adb push failed for Android smoke command file: $CommandFilePath -> $DeviceCommandFilePath"
    }
}

& $AdbPath @adbArgs logcat -c
if (-not $NoLaunch) {
    if (-not [string]::IsNullOrWhiteSpace($ActivityName)) {
        if ($commandFileSmoke) {
            & $AdbPath @adbArgs shell am start -n "$PackageName/$ActivityName" -e unity $unityArguments
        }
        else {
            & $AdbPath @adbArgs shell am start -n "$PackageName/$ActivityName"
        }
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

$smokeTestPassed = $false
if ($commandFileSmoke -and -not $NoLaunch) {
    if ((Test-LogContains -Path $LogPath -Needle "MC2 debrief summary assertion OK") `
        -and (Test-LogContains -Path $LogPath -Needle "MC2 loadout compact assertion OK")) {
        $smokeTestPassed = $true
    }
    elseif ((Test-LogContains -Path $LogPath -Needle "assertion failed") `
        -or (Test-LogContains -Path $LogPath -Needle "command file blocked") `
        -or (Test-LogContains -Path $LogPath -Needle "Command file blocked")) {
        throw "Android command-file smoke reported an assertion or command-file failure. See log: $LogPath"
    }
    else {
        throw "Android command-file smoke did not report the visible-flow success markers. See log: $LogPath"
    }
}

if (-not $SkipScreenshot) {
    Invoke-BinaryCommandToFile -FilePath $AdbPath -Arguments ($adbArgs + @("exec-out", "screencap", "-p")) -OutputPath $ScreenshotPath
    $screenshot = Get-Item -LiteralPath $ScreenshotPath
    if ($screenshot.Length -lt 1024) {
        throw "Android smoke screenshot is too small to be useful: $ScreenshotPath ($($screenshot.Length) bytes)"
    }
}

$pidOutput = (& $AdbPath @adbArgs shell pidof $PackageName) -join " "

if (-not $SkipLogCheck) {
    $logCheckScript = Join-Path $PSScriptRoot "check_android_smoke_log.ps1"
    & powershell -NoProfile -ExecutionPolicy Bypass -File $logCheckScript -LogPath $LogPath -PackageName $PackageName
    if ($LASTEXITCODE -ne 0) {
        throw "Android smoke log check failed with exit code $LASTEXITCODE"
    }
}

if (-not $SkipSummary) {
    $summary = [pscustomobject]@{
        result = "completed"
        timestampUtc = [DateTime]::UtcNow.ToString("o")
        deviceId = $device
        model = $model.Trim()
        androidVersion = $androidVersion.Trim()
        packageName = $PackageName
        activityName = $ActivityName
        process = $pidOutput.Trim()
        apkPath = $ApkPath
        logPath = $LogPath
        screenshotPath = if ($SkipScreenshot) { "" } else { $ScreenshotPath }
        commandFileSmoke = $commandFileSmoke
        commandFilePath = if ($commandFileSmoke) { $CommandFilePath } else { "" }
        deviceCommandFilePath = if ($commandFileSmoke) { $DeviceCommandFilePath } else { "" }
        unityArguments = if ($commandFileSmoke) { $unityArguments } else { "" }
        smokeTestPassed = $smokeTestPassed
        launchWaitSeconds = $LaunchWaitSeconds
        installed = -not $NoInstall
        launched = -not $NoLaunch
        logChecked = -not $SkipLogCheck
        screenshotCaptured = -not $SkipScreenshot
    }

    Write-SmokeSummary -Path $SummaryPath -Data $summary

    $summaryCheckScript = Join-Path $PSScriptRoot "check_android_smoke_summary.ps1"
    if (-not (Test-Path -LiteralPath $summaryCheckScript)) {
        throw "Missing Android smoke summary checker: $summaryCheckScript"
    }

    & powershell -NoProfile -ExecutionPolicy Bypass -File $summaryCheckScript -RepoRoot $RepoRoot -SummaryPath $SummaryPath -ExpectedPackageName $PackageName
    if ($LASTEXITCODE -ne 0) {
        throw "Android smoke summary check failed with exit code $LASTEXITCODE"
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
if ($commandFileSmoke) {
    Write-Host "CommandFileSmoke: True"
    Write-Host "CommandFile: $CommandFilePath"
    Write-Host "DeviceCommandFile: $DeviceCommandFilePath"
    Write-Host "UnityArguments: $unityArguments"
    Write-Host "SmokeTestPassed: $smokeTestPassed"
}
if (-not $SkipScreenshot) {
    Write-Host "Screenshot: $ScreenshotPath"
}
if (-not $SkipSummary) {
    Write-Host "Summary: $SummaryPath"
}

if ([string]::IsNullOrWhiteSpace($pidOutput) -and -not $smokeTestPassed) {
    throw "Package did not remain running after launch: $PackageName"
}
