param(
    [string]$RepoRoot = "",
    [string]$OutputDir = "",
    [string]$ApkPath = "",
    [string]$AdbPath = "",
    [string]$AaptPath = "",
    [string]$ApksignerPath = "",
    [string]$DeviceId = "",
    [string]$CommandFilePath = "",
    [int]$LaunchWaitSeconds = 18,
    [int]$AdbInstallTimeoutSeconds = 120,
    [int]$LogcatTailLines = 40000,
    [int]$MinimumScreenshotBytes = 100000,
    [switch]$NoInstall,
    [switch]$PlanOnly
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
    $OutputDir = Join-Path $RepoRoot "analysis-output\android-entity-placeholder-collision-runtime-evidence"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
$androidPlayer = Join-Path $HOME "Unity\Hub\Editor\6000.4.7f1\Editor\Data\PlaybackEngines\AndroidPlayer"

if ([string]::IsNullOrWhiteSpace($ApkPath)) {
    $ApkPath = Join-Path $RepoRoot "unity-mc2-demo\Builds\Android\MC2UnityDemo.apk"
}
elseif (-not [System.IO.Path]::IsPathRooted($ApkPath)) {
    $ApkPath = Join-Path $RepoRoot $ApkPath
}

if ([string]::IsNullOrWhiteSpace($AdbPath)) {
    $AdbPath = Join-Path $androidPlayer "SDK\platform-tools\adb.exe"
}

if ([string]::IsNullOrWhiteSpace($AaptPath)) {
    $AaptPath = Join-Path $androidPlayer "SDK\build-tools\36.0.0\aapt.exe"
}

if ([string]::IsNullOrWhiteSpace($ApksignerPath)) {
    $ApksignerPath = Join-Path $androidPlayer "SDK\build-tools\36.0.0\apksigner.bat"
}

if ([string]::IsNullOrWhiteSpace($CommandFilePath)) {
    $CommandFilePath = Join-Path $RepoRoot "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-android-battle-touch-evidence.txt"
}
elseif (-not [System.IO.Path]::IsPathRooted($CommandFilePath)) {
    $CommandFilePath = Join-Path $RepoRoot $CommandFilePath
}

$logPath = Join-Path $OutputDir "android-entity-placeholder-collision-runtime.log"
$adbScreenshotPath = Join-Path $OutputDir "android-entity-placeholder-collision-runtime-adb.png"
$sidecarDir = Join-Path $OutputDir "sidecar-log"
$sidecarPath = Join-Path $sidecarDir "android-entity-placeholder-collision-runtime-sidecar-log.json"
$summaryPath = Join-Path $OutputDir "android-entity-placeholder-collision-runtime-summary.json"

$requiredSmokeMarkers = @(
    "MC2 capture preset: spawn",
    "MC2 screenshot capture requested:",
    "MC2 capture sidecar written:",
    "MC2 occupancy placeholders: OccupancyPlaceholders=enabled",
    "MC2 capture sidecar occupancy:",
    "MC2 capture sidecar contact clearance:",
    "MC2 capture sidecar occupancy placeholders:",
    "MC2 capture sidecar first map visual:",
    "MC2 battle touch controls assertion OK",
    "MC2 combat situation assertion OK",
    "BattleHud=active controls=statusRows+jet+map+bay+system",
    "SparseBattleUi=statusRows+sections+solo",
    "MobileTouchUi=ready",
    "orientation=landscape",
    "landscapeOnly=yes",
    "MC2 commander command: CLI command: squad attack structure structure-1-0 accepted=3"
)

$forbiddenSmokeMarkers = @(
    "assertion failed",
    "command file blocked",
    "Command file blocked",
    "class 'CapsuleCollider' doesn't exist",
    "class 'SphereCollider' doesn't exist",
    "class 'BoxCollider' doesn't exist"
)

$sidecarFragments = @(
    "BattleOccupancy=units",
    "unitRadii infantry=24 vehicle=54 mech=64",
    "structures ",
    "hardProps ",
    "destinationFallback=structure+hardProp",
    "Landing=DemoTerrainView",
    "externalPredicate=water+mapBounds",
    "ContactClearance=players",
    "overlaps=0",
    "status=separated",
    "OccupancyPlaceholders=enabled",
    "source=BattleMission.OccupancyPlaceholderRegions+DemoTerrainView.LandingReviewBlockedMarkers",
    "FirstMapVisual=terrain+unit+structure+sparse-ui+occupancy+contact",
    "occupancy=ready",
    "contact=separated",
    "visualOnly=yes pathing=unchanged collision=unchanged",
    "visualOnly=yes",
    "collision=unchanged"
)

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Convert-ToRepoRelativePath {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    if ($fullPath.StartsWith($RepoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $fullPath.Substring($RepoRoot.Length).TrimStart("\", "/")
    }

    return $fullPath
}

function Require-File {
    param(
        [string]$Path,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Missing $Label`: $Path"
    }
}

function Require-Text {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or -not $Text.Contains($Needle)) {
        throw "$Label missing marker: $Needle"
    }
}

function Get-PngDimensions {
    param([string]$Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    if ($bytes.Length -lt 24) {
        throw "PNG file is too small to contain dimensions: $Path"
    }

    $pngSignature = @(0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a)
    for ($index = 0; $index -lt $pngSignature.Count; $index++) {
        if ($bytes[$index] -ne $pngSignature[$index]) {
            throw "Screenshot is not a PNG file: $Path"
        }
    }

    $width = ([int]$bytes[16] -shl 24) -bor ([int]$bytes[17] -shl 16) -bor ([int]$bytes[18] -shl 8) -bor [int]$bytes[19]
    $height = ([int]$bytes[20] -shl 24) -bor ([int]$bytes[21] -shl 16) -bor ([int]$bytes[22] -shl 8) -bor [int]$bytes[23]

    return [pscustomobject]@{
        Width = $width
        Height = $height
        Orientation = if ($width -ge $height) { "landscape" } else { "portrait" }
    }
}

function Get-ApkMetadata {
    param(
        [string]$Apk,
        [string]$Aapt
    )

    Require-File -Path $Apk -Label "Android APK"
    Require-File -Path $Aapt -Label "aapt"

    $metadata = @{
        PackageName = ""
        ActivityName = ""
    }

    $badging = & $Aapt dump badging $Apk
    if ($LASTEXITCODE -ne 0) {
        throw "aapt dump badging failed for APK: $Apk"
    }

    foreach ($line in $badging) {
        if ([string]::IsNullOrWhiteSpace($metadata.PackageName) -and $line -match "package: name='([^']+)'") {
            $metadata.PackageName = $Matches[1]
        }

        if ([string]::IsNullOrWhiteSpace($metadata.ActivityName) -and $line -match "launchable-activity: name='([^']+)'") {
            $metadata.ActivityName = $Matches[1]
        }
    }

    if ([string]::IsNullOrWhiteSpace($metadata.PackageName)) {
        throw "PackageName is unknown. Ensure aapt can read the APK."
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
        throw "No Android device found."
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
    if ($ready.Count -ne 1) {
        $summary = ($devices | ForEach-Object { "$($_.Id):$($_.State)" }) -join ", "
        throw "Expected exactly one authorized Android device, got: $summary"
    }

    return $ready[0].Id
}

function Invoke-AdbForceStop {
    param(
        [string]$Device,
        [string]$PackageName
    )

    $adbArgs = @("-s", $Device, "shell", "am", "force-stop", $PackageName)
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $output = & $AdbPath @adbArgs 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($exitCode -ne 0) {
        foreach ($line in @($output | ForEach-Object { $_.ToString() })) {
            Write-Host $line
        }

        throw "adb force-stop failed for package: $PackageName"
    }
}

function Invoke-CheckedScript {
    param(
        [string]$ScriptPath,
        [string[]]$Arguments,
        [string]$SuccessMarker
    )

    Require-File -Path $ScriptPath -Label "script"
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $output = & powershell -NoProfile -ExecutionPolicy Bypass -File $ScriptPath @Arguments 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    $lines = @($output | ForEach-Object { $_.ToString() })
    $text = $lines -join [Environment]::NewLine
    if ($exitCode -ne 0 -or -not $text.Contains($SuccessMarker)) {
        foreach ($line in $lines) {
            Write-Host $line
        }

        throw "Script failed or missing success marker '$SuccessMarker': $ScriptPath"
    }

    return $lines
}

function Invoke-AdbPull {
    param(
        [string]$Device,
        [string]$RemotePath,
        [string]$LocalPath,
        [string]$Label
    )

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $LocalPath) | Out-Null
    $adbArgs = @()
    if (-not [string]::IsNullOrWhiteSpace($Device)) {
        $adbArgs += @("-s", $Device)
    }

    $adbArgs += @("pull", $RemotePath, $LocalPath)
    $output = & $AdbPath @adbArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        foreach ($line in @($output | ForEach-Object { $_.ToString() })) {
            Write-Host $line
        }

        throw "adb pull failed for $Label`: $RemotePath -> $LocalPath"
    }
}

function Assert-LandscapePng {
    param(
        [string]$Path,
        [string]$Label
    )

    Require-File -Path $Path -Label $Label
    $file = Get-Item -LiteralPath $Path
    if ($file.Length -lt $MinimumScreenshotBytes) {
        throw "$Label is too small: $($file.Length) bytes, expected at least $MinimumScreenshotBytes"
    }

    $dimensions = Get-PngDimensions -Path $Path
    if ($dimensions.Orientation -ne "landscape") {
        throw "$Label must be landscape, got $($dimensions.Width)x$($dimensions.Height)"
    }

    return [pscustomobject]@{
        Width = $dimensions.Width
        Height = $dimensions.Height
        Bytes = $file.Length
    }
}

function Get-LastLogPayload {
    param(
        [string]$LogText,
        [string]$Prefix
    )

    $lines = @($LogText -split "`r?`n")
    for ($index = $lines.Count - 1; $index -ge 0; $index--) {
        $line = $lines[$index]
        $offset = $line.IndexOf($Prefix, [System.StringComparison]::Ordinal)
        if ($offset -ge 0) {
            return $line.Substring($offset + $Prefix.Length).Trim()
        }
    }

    throw "Android runtime log missing payload prefix: $Prefix"
}

Require-File -Path $AdbPath -Label "adb"
Require-File -Path $ApksignerPath -Label "apksigner"
Require-File -Path $CommandFilePath -Label "Android battle command touch command file"
Require-Text -Text (Get-Content -LiteralPath (Resolve-RepoPath -RelativePath "BUILD-MOBILE.md") -Raw -Encoding UTF8) `
    -Needle "Mobile orientation decision: the first phone version is landscape-only." `
    -Label "BUILD-MOBILE.md"

$metadata = Get-ApkMetadata -Apk $ApkPath -Aapt $AaptPath
$packageName = [string]$metadata.PackageName
$deviceBasePath = "/sdcard/Android/data/$packageName/files"
$deviceCommandFilePath = "$deviceBasePath/" + [System.IO.Path]::GetFileName($CommandFilePath)
$deviceUnityScreenshotPath = "$deviceBasePath/mc2-f28-entity-placeholder-collision-runtime.png"
$deviceSidecarPath = "$deviceBasePath/mc2-f28-entity-placeholder-collision-runtime.json"
$extraUnityArguments = "-mc2ShowOccupancyPlaceholders -mc2CapturePreset spawn -mc2CaptureScreenshot $deviceUnityScreenshotPath -mc2CaptureSidecar $deviceSidecarPath"

$smokeScript = Resolve-RepoPath -RelativePath "scripts\unity\android_device_smoke.ps1"
$smokeArgs = @(
    "-RepoRoot", $RepoRoot,
    "-ApkPath", $ApkPath,
    "-AdbPath", $AdbPath,
    "-AaptPath", $AaptPath,
    "-ApksignerPath", $ApksignerPath,
    "-LogPath", $logPath,
    "-ScreenshotPath", $adbScreenshotPath,
    "-SummaryPath", $summaryPath,
    "-CommandFilePath", $CommandFilePath,
    "-DeviceCommandFilePath", $deviceCommandFilePath,
    "-ExtraUnityArguments", $extraUnityArguments,
    "-LaunchWaitSeconds", $LaunchWaitSeconds.ToString([System.Globalization.CultureInfo]::InvariantCulture),
    "-AdbInstallTimeoutSeconds", $AdbInstallTimeoutSeconds.ToString([System.Globalization.CultureInfo]::InvariantCulture),
    "-LogcatTailLines", $LogcatTailLines.ToString([System.Globalization.CultureInfo]::InvariantCulture),
    "-RequiredSmokeMarkersText", ($requiredSmokeMarkers -join "|||"),
    "-ForbiddenSmokeMarkersText", ($forbiddenSmokeMarkers -join "|||")
)

if (-not [string]::IsNullOrWhiteSpace($DeviceId)) {
    $smokeArgs += @("-DeviceId", $DeviceId)
}

if ($NoInstall) {
    $smokeArgs += "-NoInstall"
}

if ($PlanOnly) {
    $planOutput = Invoke-CheckedScript -ScriptPath $smokeScript -Arguments ($smokeArgs + "-PlanOnly") -SuccessMarker "Android device smoke plan OK."
    foreach ($line in $planOutput) {
        Write-Host $line
    }

    Write-Host "Android entity placeholder collision runtime evidence capture plan OK."
    Write-Host "OutputDir: $OutputDir"
    Write-Host "Log: $logPath"
    Write-Host "AdbScreenshot: $adbScreenshotPath"
    Write-Host "SidecarLog: $sidecarPath"
    Write-Host "Summary: $summaryPath"
    Write-Host "DeviceUnityScreenshot: $deviceUnityScreenshotPath"
    Write-Host "DeviceSidecar: $deviceSidecarPath"
    Write-Host "ExtraUnityArguments: $extraUnityArguments"
    Write-Host "LandscapeOnly: True"
    return
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$launchDevice = Get-AdbDevice -Adb $AdbPath -RequestedDeviceId $DeviceId
Invoke-AdbForceStop -Device $launchDevice -PackageName $packageName

$smokeOutput = Invoke-CheckedScript -ScriptPath $smokeScript -Arguments $smokeArgs -SuccessMarker "Android device smoke complete."
foreach ($line in $smokeOutput) {
    Write-Host $line
}

Require-File -Path $logPath -Label "Android entity placeholder collision runtime log"
Require-File -Path $adbScreenshotPath -Label "Android entity placeholder collision ADB screenshot"
Require-File -Path $summaryPath -Label "Android entity placeholder collision summary"

$summary = Get-Content -LiteralPath $summaryPath -Raw -Encoding UTF8 | ConvertFrom-Json
$device = [string]$summary.deviceId
if ([string]::IsNullOrWhiteSpace($device)) {
    throw "Android smoke summary did not include deviceId."
}

$logText = Get-Content -LiteralPath $logPath -Raw -Encoding UTF8
foreach ($marker in $requiredSmokeMarkers) {
    Require-Text -Text $logText -Needle $marker -Label "Android runtime log"
}

foreach ($marker in $forbiddenSmokeMarkers) {
    if ($logText.Contains($marker)) {
        throw "Android runtime log contains forbidden marker: $marker"
    }
}

$adbScreenshot = Assert-LandscapePng -Path $adbScreenshotPath -Label "Android entity placeholder collision ADB screenshot"

New-Item -ItemType Directory -Force -Path $sidecarDir | Out-Null
$sidecarEvidence = [pscustomobject]@{
    source = "android-logcat-sidecar-summaries"
    preset = "spawn"
    flowScreen = "Battle"
    screenshot = $deviceUnityScreenshotPath
    screenWidth = $summary.screenshotWidth
    screenHeight = $summary.screenshotHeight
    occupancy = Get-LastLogPayload -LogText $logText -Prefix "MC2 capture sidecar occupancy: "
    contactClearance = Get-LastLogPayload -LogText $logText -Prefix "MC2 capture sidecar contact clearance: "
    occupancyPlaceholders = Get-LastLogPayload -LogText $logText -Prefix "MC2 capture sidecar occupancy placeholders: "
    firstMapVisual = Get-LastLogPayload -LogText $logText -Prefix "MC2 capture sidecar first map visual: "
}
$sidecarEvidence | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $sidecarPath -Encoding UTF8

$sidecarText = Get-Content -LiteralPath $sidecarPath -Raw -Encoding UTF8
foreach ($fragment in $sidecarFragments) {
    Require-Text -Text $sidecarText -Needle $fragment -Label "Android runtime sidecar"
}

$sidecar = $sidecarText | ConvertFrom-Json
if ($sidecar.flowScreen -ne "Battle") {
    throw "Android runtime sidecar must be in Battle flow, got: $($sidecar.flowScreen)"
}

if ($sidecar.preset -ne "spawn") {
    throw "Android runtime sidecar preset must be spawn, got: $($sidecar.preset)"
}

if ([int]$sidecar.screenWidth -lt [int]$sidecar.screenHeight) {
    throw "Android runtime sidecar screen must be landscape, got: $($sidecar.screenWidth)x$($sidecar.screenHeight)"
}

$summary | Add-Member -Force -NotePropertyName evidenceKind -NotePropertyValue "android-entity-placeholder-collision-runtime"
$summary | Add-Member -Force -NotePropertyName landscapeOnly -NotePropertyValue $true
$summary | Add-Member -Force -NotePropertyName entityPlaceholderCollisionRuntimePassed -NotePropertyValue $true
$summary | Add-Member -Force -NotePropertyName colliderClassNoisePassed -NotePropertyValue $true
$summary | Add-Member -Force -NotePropertyName sidecarPath -NotePropertyValue (Convert-ToRepoRelativePath -Path $sidecarPath)
$summary | Add-Member -Force -NotePropertyName sidecarSource -NotePropertyValue "android-logcat-sidecar-summaries"
$summary | Add-Member -Force -NotePropertyName adbScreenshotPath -NotePropertyValue (Convert-ToRepoRelativePath -Path $adbScreenshotPath)
$summary | Add-Member -Force -NotePropertyName deviceSidecarPath -NotePropertyValue $deviceSidecarPath
$summary | Add-Member -Force -NotePropertyName deviceUnityScreenshotPath -NotePropertyValue $deviceUnityScreenshotPath
$summary | Add-Member -Force -NotePropertyName sidecarFragments -NotePropertyValue $sidecarFragments
$summary | Add-Member -Force -NotePropertyName forbiddenSmokeMarkers -NotePropertyValue $forbiddenSmokeMarkers
$summary | Add-Member -Force -NotePropertyName adbScreenshotBytes -NotePropertyValue $adbScreenshot.Bytes
$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath -Encoding UTF8

$summaryCheckScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_smoke_summary.ps1"
Invoke-CheckedScript -ScriptPath $summaryCheckScript -Arguments @("-RepoRoot", $RepoRoot, "-SummaryPath", $summaryPath, "-ExpectedPackageName", $summary.packageName) -SuccessMarker "Android smoke summary check OK." | ForEach-Object { Write-Host $_ }

$sidecarCheckScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_entity_placeholder_collision_path.ps1"
Invoke-CheckedScript -ScriptPath $sidecarCheckScript -Arguments @("-RepoRoot", $RepoRoot, "-SidecarDirectory", $sidecarDir) -SuccessMarker "Android entity placeholder collision path check OK." | ForEach-Object { Write-Host $_ }

Write-Host "Android entity placeholder collision runtime evidence capture OK."
Write-Host "Device: $($summary.deviceId) / $($summary.model) / Android $($summary.androidVersion)"
Write-Host "Log: $logPath"
Write-Host "AdbScreenshot: $adbScreenshotPath"
Write-Host "AdbScreenshotSize: $($summary.screenshotWidth)x$($summary.screenshotHeight)"
Write-Host "SidecarLog: $sidecarPath"
Write-Host "Summary: $summaryPath"
Write-Host "LandscapeOnly: True"
