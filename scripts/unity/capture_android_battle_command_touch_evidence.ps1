param(
    [string]$RepoRoot = "",
    [string]$OutputDir = "",
    [string]$ApkPath = "",
    [string]$AdbPath = "",
    [string]$AaptPath = "",
    [string]$ApksignerPath = "",
    [string]$DeviceId = "",
    [string]$CommandFilePath = "",
    [string]$DeviceCommandFilePath = "",
    [int]$LaunchWaitSeconds = 16,
    [int]$AdbInstallTimeoutSeconds = 120,
    [int]$LogcatTailLines = 30000,
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
    $OutputDir = Join-Path $RepoRoot "analysis-output\android-battle-command-touch-evidence"
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

$logPath = Join-Path $OutputDir "android-battle-command-touch.log"
$screenshotPath = Join-Path $OutputDir "android-battle-command-touch.png"
$summaryPath = Join-Path $OutputDir "android-battle-command-touch-summary.json"

$requiredSmokeMarkers = @(
    "MC2 battle touch controls assertion OK",
    "MC2 combat situation assertion OK",
    "selection=squad",
    "selection=unit-1",
    "row=unit-1:solo",
    "row=unit-1:ready",
    "jumping=3",
    "MC2 commander mission map open OK",
    "MC2 commander mission map closed OK",
    "MC2 commander system open OK",
    "MC2 commander system closed OK",
    "BattleHud=active controls=statusRows+jet+map+bay+system",
    "SparseBattleUi=statusRows+sections+solo",
    "controls=all+jet+map+bay+system",
    "MobileTouchUi=ready",
    "orientation=landscape",
    "landscapeOnly=yes",
    "noDragBox=yes",
    "combatLog=hidden",
    "MC2 commander command: CLI command: squad jump 3000 -1500 accepted=3",
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

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Convert-ToRepoRelativePath {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $repoRootFull = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd("\", "/")
    if (-not $fullPath.StartsWith($repoRootFull, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path is outside RepoRoot: $Path"
    }

    return $fullPath.Substring($repoRootFull.Length).TrimStart("\", "/") -replace "\\", "/"
}

function Require-File {
    param(
        [string]$Path,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "$Label missing: $Path"
    }
}

function Require-Text {
    param(
        [string]$Path,
        [string]$Needle,
        [string]$Label
    )

    $text = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    if (-not $text.Contains($Needle)) {
        throw "$Label missing marker: $Needle"
    }
}

function Invoke-CheckedScript {
    param(
        [string]$ScriptPath,
        [string[]]$Arguments,
        [string]$SuccessMarker
    )

    Require-File -Path $ScriptPath -Label "Script"
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
    $joined = $lines -join [Environment]::NewLine

    if ($exitCode -ne 0 -or (-not [string]::IsNullOrWhiteSpace($SuccessMarker) -and $joined -notlike "*$SuccessMarker*")) {
        foreach ($line in $lines) {
            Write-Host $line
        }

        if ($exitCode -ne 0) {
            throw "$ScriptPath failed with exit code $exitCode."
        }

        throw "$ScriptPath did not print expected marker: $SuccessMarker"
    }

    return $lines
}

function Get-PngDimensions {
    param([string]$Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    if ($bytes.Length -lt 24 `
        -or $bytes[0] -ne 0x89 `
        -or $bytes[1] -ne 0x50 `
        -or $bytes[2] -ne 0x4E `
        -or $bytes[3] -ne 0x47) {
        throw "Screenshot is not a PNG file: $Path"
    }

    $width = ([int]$bytes[16] -shl 24) -bor ([int]$bytes[17] -shl 16) -bor ([int]$bytes[18] -shl 8) -bor [int]$bytes[19]
    $height = ([int]$bytes[20] -shl 24) -bor ([int]$bytes[21] -shl 16) -bor ([int]$bytes[22] -shl 8) -bor [int]$bytes[23]

    return [pscustomobject]@{
        Width = $width
        Height = $height
        Orientation = if ($width -ge $height) { "landscape" } else { "portrait" }
    }
}

function Require-LogMarker {
    param(
        [string]$LogText,
        [string]$Marker
    )

    if (-not $LogText.Contains($Marker)) {
        throw "Android battle command touch evidence log missing marker: $Marker"
    }
}

Require-File -Path $ApkPath -Label "Android APK"
Require-File -Path $AdbPath -Label "adb"
Require-File -Path $AaptPath -Label "aapt"
Require-File -Path $ApksignerPath -Label "apksigner"
Require-File -Path $CommandFilePath -Label "Android battle command touch command file"

$gitignore = Resolve-RepoPath -RelativePath ".gitignore"
Require-Text -Path $gitignore -Needle "analysis-output/android-battle-command-touch-evidence/" -Label ".gitignore"
Require-Text -Path $CommandFilePath -Needle "assert-battle-touch-controls" -Label "Android battle command touch command file"
Require-Text -Path $CommandFilePath -Needle "status-row unit-1" -Label "Android battle command touch command file"
Require-Text -Path $CommandFilePath -Needle "battle-click 2520 -1941" -Label "Android battle command touch command file"
Require-Text -Path $CommandFilePath -Needle "command squad jump 3000 -1500" -Label "Android battle command touch command file"
Require-Text -Path $CommandFilePath -Needle "open-map" -Label "Android battle command touch command file"
Require-Text -Path $CommandFilePath -Needle "open-system" -Label "Android battle command touch command file"
Require-Text -Path $CommandFilePath -Needle "command squad attack structure structure-1-0" -Label "Android battle command touch command file"
Require-Text -Path (Resolve-RepoPath -RelativePath "BUILD-MOBILE.md") -Needle "Mobile orientation decision: the first phone version is landscape-only." -Label "BUILD-MOBILE.md"
Require-Text -Path (Resolve-RepoPath -RelativePath "unity-mc2-demo\ProjectSettings\ProjectSettings.asset") -Needle "allowedAutorotateToPortrait: 0" -Label "ProjectSettings.asset"
Require-Text -Path (Resolve-RepoPath -RelativePath "unity-mc2-demo\ProjectSettings\ProjectSettings.asset") -Needle "allowedAutorotateToLandscapeRight: 1" -Label "ProjectSettings.asset"
Require-Text -Path (Resolve-RepoPath -RelativePath "unity-mc2-demo\ProjectSettings\ProjectSettings.asset") -Needle "allowedAutorotateToLandscapeLeft: 1" -Label "ProjectSettings.asset"

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$smokeScript = Resolve-RepoPath -RelativePath "scripts\unity\android_device_smoke.ps1"
$smokeArgs = @(
    "-RepoRoot", $RepoRoot,
    "-ApkPath", $ApkPath,
    "-AdbPath", $AdbPath,
    "-AaptPath", $AaptPath,
    "-ApksignerPath", $ApksignerPath,
    "-LogPath", $logPath,
    "-ScreenshotPath", $screenshotPath,
    "-SummaryPath", $summaryPath,
    "-CommandFilePath", $CommandFilePath,
    "-LaunchWaitSeconds", $LaunchWaitSeconds.ToString([System.Globalization.CultureInfo]::InvariantCulture),
    "-AdbInstallTimeoutSeconds", $AdbInstallTimeoutSeconds.ToString([System.Globalization.CultureInfo]::InvariantCulture),
    "-LogcatTailLines", $LogcatTailLines.ToString([System.Globalization.CultureInfo]::InvariantCulture),
    "-RequiredSmokeMarkersText", ($requiredSmokeMarkers -join "|||"),
    "-ForbiddenSmokeMarkersText", ($forbiddenSmokeMarkers -join "|||")
)

if (-not [string]::IsNullOrWhiteSpace($DeviceId)) {
    $smokeArgs += @("-DeviceId", $DeviceId)
}

if (-not [string]::IsNullOrWhiteSpace($DeviceCommandFilePath)) {
    $smokeArgs += @("-DeviceCommandFilePath", $DeviceCommandFilePath)
}

if ($NoInstall) {
    $smokeArgs += "-NoInstall"
}

if ($PlanOnly) {
    $planOutput = Invoke-CheckedScript -ScriptPath $smokeScript -Arguments ($smokeArgs + "-PlanOnly") -SuccessMarker "Android device smoke plan OK."
    foreach ($line in $planOutput) {
        Write-Host $line
    }

    Write-Host "Android battle command touch evidence capture plan OK."
    Write-Host "OutputDir: $OutputDir"
    Write-Host "Log: $logPath"
    Write-Host "Screenshot: $screenshotPath"
    Write-Host "Summary: $summaryPath"
    Write-Host "LandscapeOnly: True"
    return
}

$smokeOutput = Invoke-CheckedScript -ScriptPath $smokeScript -Arguments $smokeArgs -SuccessMarker "Android device smoke complete."
foreach ($line in $smokeOutput) {
    Write-Host $line
}

Require-File -Path $logPath -Label "Android battle command touch log"
Require-File -Path $screenshotPath -Label "Android battle command touch screenshot"
Require-File -Path $summaryPath -Label "Android battle command touch summary"

$logText = Get-Content -LiteralPath $logPath -Raw -Encoding UTF8
foreach ($marker in $requiredSmokeMarkers) {
    Require-LogMarker -LogText $logText -Marker $marker
}

foreach ($marker in $forbiddenSmokeMarkers) {
    if ($logText.Contains($marker)) {
        throw "Android battle command touch evidence log contains forbidden marker: $marker"
    }
}

$screenshot = Get-Item -LiteralPath $screenshotPath
if ($screenshot.Length -lt $MinimumScreenshotBytes) {
    throw "Android battle command touch screenshot is too small: $($screenshot.Length) bytes, expected at least $MinimumScreenshotBytes"
}

$dimensions = Get-PngDimensions -Path $screenshotPath
if ($dimensions.Orientation -ne "landscape") {
    throw "Android battle command touch screenshot must be landscape, got $($dimensions.Width)x$($dimensions.Height)"
}

$summary = Get-Content -LiteralPath $summaryPath -Raw -Encoding UTF8 | ConvertFrom-Json
$summary | Add-Member -Force -NotePropertyName evidenceKind -NotePropertyValue "android-battle-command-touch"
$summary | Add-Member -Force -NotePropertyName landscapeOnly -NotePropertyValue $true
$summary | Add-Member -Force -NotePropertyName battleTouchControlsPassed -NotePropertyValue $true
$summary | Add-Member -Force -NotePropertyName statusRowSingleSelectPassed -NotePropertyValue $true
$summary | Add-Member -Force -NotePropertyName squadReturnPassed -NotePropertyValue $true
$summary | Add-Member -Force -NotePropertyName jetPassed -NotePropertyValue $true
$summary | Add-Member -Force -NotePropertyName mapSystemEntryPassed -NotePropertyValue $true
$summary | Add-Member -Force -NotePropertyName sparseHudPassed -NotePropertyValue $true
$summary | Add-Member -Force -NotePropertyName requiredSmokeMarkers -NotePropertyValue $requiredSmokeMarkers
$summary | Add-Member -Force -NotePropertyName outputDir -NotePropertyValue (Convert-ToRepoRelativePath -Path $OutputDir)
$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath -Encoding UTF8

$summaryCheckScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_smoke_summary.ps1"
Invoke-CheckedScript -ScriptPath $summaryCheckScript -Arguments @("-RepoRoot", $RepoRoot, "-SummaryPath", $summaryPath, "-ExpectedPackageName", $summary.packageName) -SuccessMarker "Android smoke summary check OK." | ForEach-Object { Write-Host $_ }

$logCheckScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_smoke_log.ps1"
Invoke-CheckedScript -ScriptPath $logCheckScript -Arguments @("-LogPath", $logPath, "-PackageName", $summary.packageName) -SuccessMarker "Android smoke log check OK." | ForEach-Object { Write-Host $_ }

Write-Host "Android battle command touch evidence capture OK."
Write-Host "Device: $($summary.deviceId) / $($summary.model) / Android $($summary.androidVersion)"
Write-Host "Screenshot: $screenshotPath"
Write-Host "ScreenshotSize: $($dimensions.Width)x$($dimensions.Height)"
Write-Host "ScreenshotBytes: $($screenshot.Length)"
Write-Host "Log: $logPath"
Write-Host "Summary: $summaryPath"
Write-Host "LandscapeOnly: True"
