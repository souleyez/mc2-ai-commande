param(
    [string]$RepoRoot = "",
    [string]$ApkPath = "",
    [string]$AdbPath = "",
    [string]$AaptPath = "",
    [string]$PackageName = "",
    [string]$ActivityName = "",
    [string]$DeviceId = "",
    [string]$OutputDir = "",
    [string]$CapturePreset = "north-patrol",
    [int]$SampleSeconds = 10,
    [int]$WarmupSeconds = 2,
    [int]$PostSampleWaitSeconds = 2,
    [int]$MaxMarkerWaitSeconds = 45,
    [int]$MinAverageFps = 25,
    [int64]$MaxTotalPssKb = 1572864,
    [int64]$MaxApkBytes = 524288000,
    [switch]$NoInstall,
    [switch]$SkipForceStop
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot "analysis-output"
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

if ($SampleSeconds -lt 3 -or $SampleSeconds -gt 120) {
    throw "SampleSeconds must be between 3 and 120."
}

if ($WarmupSeconds -lt 0 -or $WarmupSeconds -gt 30) {
    throw "WarmupSeconds must be between 0 and 30."
}

if ($PostSampleWaitSeconds -lt 1 -or $PostSampleWaitSeconds -gt 30) {
    throw "PostSampleWaitSeconds must be between 1 and 30."
}

if ($MaxMarkerWaitSeconds -lt 10 -or $MaxMarkerWaitSeconds -gt 180) {
    throw "MaxMarkerWaitSeconds must be between 10 and 180."
}

if (-not (Test-Path -LiteralPath $ApkPath -PathType Leaf)) {
    throw "Missing Android APK: $ApkPath"
}

if (-not (Test-Path -LiteralPath $AdbPath -PathType Leaf)) {
    throw "Missing adb: $AdbPath"
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$logPath = Join-Path $OutputDir "android-performance-logcat.txt"
$meminfoPath = Join-Path $OutputDir "android-performance-meminfo.txt"
$gfxinfoPath = Join-Path $OutputDir "android-performance-gfxinfo.txt"
$batteryPath = Join-Path $OutputDir "android-performance-battery.txt"
$thermalPath = Join-Path $OutputDir "android-performance-thermal.txt"
$summaryPath = Join-Path $OutputDir "android-performance-baseline.json"

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

function Write-NativeOutputFile {
    param(
        [string]$Path,
        [string]$FilePath,
        [string[]]$Arguments
    )

    $result = Invoke-NativeCommand -FilePath $FilePath -Arguments $Arguments
    $result.Output | Set-Content -LiteralPath $Path -Encoding UTF8
    return $result
}

function Get-AdbDevice {
    param(
        [string]$Adb,
        [string]$RequestedDeviceId
    )

    $result = Invoke-NativeCommand -FilePath $Adb -Arguments @("devices", "-l")
    if ($result.ExitCode -ne 0) {
        throw "adb devices failed: $($result.Output -join ' ')"
    }

    $devices = @()
    foreach ($line in $result.Output) {
        if ($line -match "^(\S+)\s+device(?:\s|$)") {
            $devices += $Matches[1]
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($RequestedDeviceId)) {
        if ($devices -notcontains $RequestedDeviceId) {
            throw "Requested Android device is not connected and authorized: $RequestedDeviceId"
        }

        return $RequestedDeviceId
    }

    if ($devices.Count -ne 1) {
        throw "Android performance baseline requires exactly one authorized device. Found: $($devices -join ', ')"
    }

    return $devices[0]
}

function Get-ApkMetadata {
    param(
        [string]$Apk,
        [string]$Aapt
    )

    $metadata = [ordered]@{
        PackageName = ""
        ActivityName = ""
    }

    if (-not (Test-Path -LiteralPath $Aapt -PathType Leaf)) {
        return $metadata
    }

    $badging = Invoke-NativeCommand -FilePath $Aapt -Arguments @("dump", "badging", $Apk)
    if ($badging.ExitCode -ne 0) {
        return $metadata
    }

    foreach ($line in $badging.Output) {
        if ([string]::IsNullOrWhiteSpace($metadata.PackageName) -and $line -match "package: name='([^']+)'") {
            $metadata.PackageName = $Matches[1]
        }

        if ([string]::IsNullOrWhiteSpace($metadata.ActivityName) -and $line -match "launchable-activity: name='([^']+)'") {
            $metadata.ActivityName = $Matches[1]
        }
    }

    return $metadata
}

function Get-RegexValue {
    param(
        [string]$Text,
        [string]$Pattern,
        [string]$Default = ""
    )

    if ($Text -match $Pattern) {
        return $Matches[1]
    }

    return $Default
}

function Get-RegexDouble {
    param(
        [string]$Text,
        [string]$Pattern
    )

    $value = Get-RegexValue -Text $Text -Pattern $Pattern
    if ([string]::IsNullOrWhiteSpace($value)) {
        return 0.0
    }

    return [double]::Parse($value, [Globalization.CultureInfo]::InvariantCulture)
}

function Get-RegexInt64 {
    param(
        [string]$Text,
        [string]$Pattern
    )

    $value = Get-RegexValue -Text $Text -Pattern $Pattern
    if ([string]::IsNullOrWhiteSpace($value)) {
        return 0
    }

    return [int64]$value
}

$apk = Get-Item -LiteralPath $ApkPath
if ($apk.Length -gt $MaxApkBytes) {
    throw "APK exceeds G5 prototype size budget: $($apk.Length) bytes > $MaxApkBytes bytes."
}

$metadata = Get-ApkMetadata -Apk $ApkPath -Aapt $AaptPath
if ([string]::IsNullOrWhiteSpace($PackageName)) {
    $PackageName = [string]$metadata.PackageName
}

if ([string]::IsNullOrWhiteSpace($ActivityName)) {
    $ActivityName = [string]$metadata.ActivityName
}

if ([string]::IsNullOrWhiteSpace($PackageName) -or [string]::IsNullOrWhiteSpace($ActivityName)) {
    throw "Could not resolve Android package/activity from APK. Pass -PackageName and -ActivityName."
}

$device = Get-AdbDevice -Adb $AdbPath -RequestedDeviceId $DeviceId
$adbArgs = @("-s", $device)

$model = ((Invoke-NativeCommand -FilePath $AdbPath -Arguments ($adbArgs + @("shell", "getprop", "ro.product.model"))).Output -join " ").Trim()
$androidVersion = ((Invoke-NativeCommand -FilePath $AdbPath -Arguments ($adbArgs + @("shell", "getprop", "ro.build.version.release"))).Output -join " ").Trim()

if (-not $SkipForceStop) {
    [void](Invoke-NativeCommand -FilePath $AdbPath -Arguments ($adbArgs + @("shell", "am", "force-stop", $PackageName)))
}

if (-not $NoInstall) {
    $install = Invoke-NativeCommand -FilePath $AdbPath -Arguments ($adbArgs + @("install", "-r", "--no-streaming", $ApkPath))
    if ($install.ExitCode -ne 0 -or (($install.Output -join " ") -notmatch "Success")) {
        throw "adb install failed: $($install.Output -join ' ')"
    }
}

[void](Invoke-NativeCommand -FilePath $AdbPath -Arguments ($adbArgs + @("logcat", "-c")))

$unityArguments = "-mc2CapturePreset $CapturePreset -mc2PerformanceWarmupSeconds $WarmupSeconds -mc2PerformanceLogSeconds $SampleSeconds"
$markerStopwatch = [Diagnostics.Stopwatch]::StartNew()
$launch = Invoke-NativeCommand -FilePath $AdbPath -Arguments ($adbArgs + @(
    "shell",
    "am",
    "start",
    "-W",
    "-n",
    "$PackageName/$ActivityName",
    "--es",
    "unity",
    "'$unityArguments'"
))

if ($launch.ExitCode -ne 0) {
    throw "adb launch failed: $($launch.Output -join ' ')"
}

$performanceLine = ""
$latestLogLines = @()
$deadline = (Get-Date).AddSeconds($MaxMarkerWaitSeconds)
while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 2
    $logDump = Invoke-NativeCommand -FilePath $AdbPath -Arguments ($adbArgs + @("logcat", "-d"))
    $latestLogLines = $logDump.Output
    $performanceLine = (($latestLogLines | Where-Object { $_ -like "*MC2 mobile performance baseline:*" }) | Select-Object -Last 1)
    if (-not [string]::IsNullOrWhiteSpace($performanceLine)) {
        break
    }
}

$markerObservedSeconds = [math]::Round($markerStopwatch.Elapsed.TotalSeconds, 2)
if ([string]::IsNullOrWhiteSpace($performanceLine)) {
    $latestLogLines | Set-Content -LiteralPath $logPath -Encoding UTF8
    throw "Missing Unity performance marker in logcat within $MaxMarkerWaitSeconds seconds: MC2 mobile performance baseline"
}

Start-Sleep -Seconds $PostSampleWaitSeconds

$pidOutput = (Invoke-NativeCommand -FilePath $AdbPath -Arguments ($adbArgs + @("shell", "pidof", $PackageName))).Output -join " "
$processIdText = $pidOutput.Trim()
if ([string]::IsNullOrWhiteSpace($processIdText)) {
    throw "Android performance baseline process is not running: $PackageName"
}

[void](Write-NativeOutputFile -Path $meminfoPath -FilePath $AdbPath -Arguments ($adbArgs + @("shell", "dumpsys", "meminfo", $PackageName)))
[void](Write-NativeOutputFile -Path $gfxinfoPath -FilePath $AdbPath -Arguments ($adbArgs + @("shell", "dumpsys", "gfxinfo", $PackageName, "framestats")))
[void](Write-NativeOutputFile -Path $batteryPath -FilePath $AdbPath -Arguments ($adbArgs + @("shell", "dumpsys", "battery")))
[void](Write-NativeOutputFile -Path $thermalPath -FilePath $AdbPath -Arguments ($adbArgs + @("shell", "dumpsys", "thermalservice")))
[void](Write-NativeOutputFile -Path $logPath -FilePath $AdbPath -Arguments ($adbArgs + @("logcat", "-d")))

if (-not $SkipForceStop) {
    [void](Invoke-NativeCommand -FilePath $AdbPath -Arguments ($adbArgs + @("shell", "am", "force-stop", $PackageName)))
}

$logText = Get-Content -LiteralPath $logPath -Raw
$meminfoText = Get-Content -LiteralPath $meminfoPath -Raw
$batteryText = Get-Content -LiteralPath $batteryPath -Raw
$thermalText = Get-Content -LiteralPath $thermalPath -Raw
$launchText = $launch.Output -join [Environment]::NewLine

$finalPerformanceLine = (($logText -split "`r?`n") | Where-Object { $_ -like "*MC2 mobile performance baseline:*" } | Select-Object -Last 1)
if (-not [string]::IsNullOrWhiteSpace($finalPerformanceLine)) {
    $performanceLine = $finalPerformanceLine
}

$averageFps = Get-RegexDouble -Text $performanceLine -Pattern "avgFps=([0-9.]+)"
$maxFrameMs = Get-RegexDouble -Text $performanceLine -Pattern "maxFrameMs=([0-9.]+)"
$frames = [int](Get-RegexInt64 -Text $performanceLine -Pattern "frames=([0-9]+)")
$unitySeconds = Get-RegexDouble -Text $performanceLine -Pattern "seconds=([0-9.]+)"
$unityWarmupSeconds = Get-RegexDouble -Text $performanceLine -Pattern "warmupSeconds=([0-9.]+)"
$unityTotalMb = Get-RegexDouble -Text $performanceLine -Pattern "unityTotalMB=([0-9.]+)"
$unityReservedMb = Get-RegexDouble -Text $performanceLine -Pattern "unityReservedMB=([0-9.]+)"
$monoUsedMb = Get-RegexDouble -Text $performanceLine -Pattern "monoUsedMB=([0-9.]+)"
$systemMemoryMb = [int](Get-RegexInt64 -Text $performanceLine -Pattern "systemMemoryMB=([0-9]+)")
$screen = Get-RegexValue -Text $performanceLine -Pattern "screen=([0-9]+x[0-9]+)"
$targetFrameRate = [int](Get-RegexInt64 -Text $performanceLine -Pattern "targetFrameRate=(-?[0-9]+)")
$vSync = [int](Get-RegexInt64 -Text $performanceLine -Pattern "vSync=([0-9]+)")
$flow = Get-RegexValue -Text $performanceLine -Pattern "flow=([^ ]+)"
$missionResult = Get-RegexValue -Text $performanceLine -Pattern "result=([^ ]+)"
$missionTime = Get-RegexDouble -Text $performanceLine -Pattern "missionTime=([0-9.]+)"

$totalPssKb = Get-RegexInt64 -Text $meminfoText -Pattern "TOTAL PSS:\s+([0-9]+)"
$totalRssKb = Get-RegexInt64 -Text $meminfoText -Pattern "TOTAL RSS:\s+([0-9]+)"
$totalSwapPssKb = Get-RegexInt64 -Text $meminfoText -Pattern "TOTAL SWAP PSS:\s+([0-9]+)"

$batteryLevel = [int](Get-RegexInt64 -Text $batteryText -Pattern "level:\s+([0-9]+)")
$batteryTemperatureTenthsC = [int](Get-RegexInt64 -Text $batteryText -Pattern "temperature:\s+([0-9]+)")
$batteryTemperatureC = [math]::Round($batteryTemperatureTenthsC / 10.0, 1)
$usbPowered = ($batteryText -match "USB powered:\s+true")
$thermalStatus = [int](Get-RegexInt64 -Text $thermalText -Pattern "Thermal Status:\s+([0-9]+)")

$launchTotalMs = [int](Get-RegexInt64 -Text $launchText -Pattern "TotalTime:\s+([0-9]+)")
$launchWaitMs = [int](Get-RegexInt64 -Text $launchText -Pattern "WaitTime:\s+([0-9]+)")

$status = "completed"
$failures = New-Object System.Collections.Generic.List[string]
if ($averageFps -lt $MinAverageFps) {
    [void]$failures.Add("average FPS below budget: $averageFps < $MinAverageFps")
}

if ($totalPssKb -gt $MaxTotalPssKb) {
    [void]$failures.Add("TOTAL PSS above budget: $totalPssKb KB > $MaxTotalPssKb KB")
}

if ($thermalStatus -gt 1) {
    [void]$failures.Add("thermal status above light/no throttling budget: $thermalStatus")
}

if ($failures.Count -gt 0) {
    $status = "failed"
}

$summary = [ordered]@{
    result = $status
    timestampUtc = [DateTime]::UtcNow.ToString("o")
    deviceId = $device
    model = $model
    androidVersion = $androidVersion
    packageName = $PackageName
    activityName = $ActivityName
    process = $processIdText
    apkPath = $ApkPath
    apkBytes = $apk.Length
    capturePreset = $CapturePreset
    sampleSeconds = $SampleSeconds
    warmupSeconds = $WarmupSeconds
    postSampleWaitSeconds = $PostSampleWaitSeconds
    maxMarkerWaitSeconds = $MaxMarkerWaitSeconds
    minAverageFps = $MinAverageFps
    maxTotalPssKb = $MaxTotalPssKb
    maxApkBytes = $MaxApkBytes
    launchTotalMs = $launchTotalMs
    launchWaitMs = $launchWaitMs
    battleBaselineObservedWithinSeconds = $markerObservedSeconds
    frames = $frames
    unitySeconds = $unitySeconds
    unityWarmupSeconds = $unityWarmupSeconds
    averageFps = $averageFps
    maxFrameMs = $maxFrameMs
    unityTotalMb = $unityTotalMb
    unityReservedMb = $unityReservedMb
    monoUsedMb = $monoUsedMb
    systemMemoryMb = $systemMemoryMb
    totalPssKb = $totalPssKb
    totalRssKb = $totalRssKb
    totalSwapPssKb = $totalSwapPssKb
    batteryLevel = $batteryLevel
    batteryTemperatureC = $batteryTemperatureC
    usbPowered = $usbPowered
    thermalStatus = $thermalStatus
    screen = $screen
    targetFrameRate = $targetFrameRate
    vSync = $vSync
    flow = $flow
    missionResult = $missionResult
    missionTimeSeconds = $missionTime
    performanceMarker = $performanceLine.Trim()
    logPath = $logPath
    meminfoPath = $meminfoPath
    gfxinfoPath = $gfxinfoPath
    batteryPath = $batteryPath
    thermalPath = $thermalPath
    failures = @($failures)
}

$summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $summaryPath -Encoding UTF8

if ($failures.Count -gt 0) {
    Write-Host "Android performance baseline check failed."
    $failures | ForEach-Object { Write-Host " - $_" }
    throw "$($failures.Count) Android performance budget check(s) failed."
}

Write-Host "Android performance baseline capture OK."
Write-Host "Repo: $RepoRoot"
Write-Host "Summary: $summaryPath"
Write-Host "Device: $device / $model / Android $androidVersion"
Write-Host "APK: $($apk.Length) bytes"
Write-Host "LaunchTotalMs: $launchTotalMs"
Write-Host "WarmupSeconds: $WarmupSeconds"
Write-Host "BattleBaselineObservedWithinSeconds: $markerObservedSeconds"
Write-Host "AverageFps: $averageFps"
Write-Host "MaxFrameMs: $maxFrameMs"
Write-Host "TotalPssKb: $totalPssKb"
Write-Host "ThermalStatus: $thermalStatus"
Write-Host "Battery: level=$batteryLevel tempC=$batteryTemperatureC usbPowered=$usbPowered"
