param(
    [string]$RepoRoot = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

$failures = New-Object System.Collections.Generic.List[string]
$rows = New-Object System.Collections.Generic.List[object]

function Add-Failure {
    param([string]$Message)
    [void]$failures.Add($Message)
}

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Assert-FileContains {
    param(
        [string]$RelativePath,
        [string[]]$Markers
    )

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "$RelativePath missing"
        return
    }

    $text = Get-Content -LiteralPath $path -Raw -Encoding UTF8
    $missing = New-Object System.Collections.Generic.List[string]
    foreach ($marker in $Markers) {
        if (-not $text.Contains($marker)) {
            Add-Failure "$RelativePath missing marker: $marker"
            [void]$missing.Add($marker)
        }
    }

    if ($missing.Count -eq 0) {
        [void]$rows.Add([pscustomobject]@{
            Check = $RelativePath
            Status = "OK"
            Detail = "$($Markers.Count) marker(s)"
        })
    }
}

Assert-FileContains -RelativePath "unity-mc2-demo\ProjectSettings\ProjectSettings.asset" -Markers @(
    "defaultScreenOrientation: 3",
    "defaultScreenWidth: 1920",
    "defaultScreenHeight: 1080",
    "allowedAutorotateToPortrait: 0",
    "allowedAutorotateToPortraitUpsideDown: 0",
    "allowedAutorotateToLandscapeRight: 1",
    "allowedAutorotateToLandscapeLeft: 1",
    "androidResizeableActivity: 0",
    "androidDefaultWindowWidth: 1920",
    "androidDefaultWindowHeight: 1080"
)

Assert-FileContains -RelativePath "unity-mc2-demo\Assets\Editor\Mc2DemoBuilder.cs" -Markers @(
    "ConfigureMobileLandscapePlayerSettings();",
    "PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;",
    "PlayerSettings.allowedAutorotateToPortrait = false;",
    "PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;",
    "PlayerSettings.allowedAutorotateToLandscapeLeft = true;",
    "PlayerSettings.allowedAutorotateToLandscapeRight = true;",
    "landscape-only touch build"
)

Assert-FileContains -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs" -Markers @(
    "MobileTouchUi=ready orientation=landscape",
    "landscapeOnly=yes",
    "Screen.autorotateToPortrait = false;",
    "Screen.autorotateToPortraitUpsideDown = false;",
    "Screen.autorotateToLandscapeLeft = true;",
    "Screen.autorotateToLandscapeRight = true;",
    "Screen.orientation = ScreenOrientation.AutoRotation;"
)

Assert-FileContains -RelativePath "scripts\unity\check_android_apk_manifest.ps1" -Markers @(
    "android.hardware.screen.landscape",
    "APK activity screenOrientation is not landscape-only",
    "ExpectedActivityScreenOrientations"
)

Assert-FileContains -RelativePath "scripts\unity\android_device_smoke.ps1" -Markers @(
    "LandscapeScreenshot: True",
    "Android smoke screenshot must be landscape",
    'screenshotDimensions.Orientation -ne "landscape"'
)

Assert-FileContains -RelativePath "scripts\unity\check_android_smoke_summary.ps1" -Markers @(
    'Summary.screenshotOrientation -ne "landscape"',
    "Summary screenshot dimensions must be landscape"
)

Assert-FileContains -RelativePath "README.md" -Markers @(
    "G4 Touch UI pass",
    "horizontal phone game",
    "2400x1080",
    "check_mobile_landscape_contract.ps1",
    "Mobile landscape contract check OK"
)

Assert-FileContains -RelativePath "BUILD-MOBILE.md" -Markers @(
    "the first phone version is landscape-only",
    "horizontal phone game version",
    "Portrait layout is not a supported first-version target",
    "check_mobile_landscape_contract.ps1",
    "Mobile landscape contract check OK."
)

Assert-FileContains -RelativePath "docs-mobile-first-plan-2026-06-10.md" -Markers @(
    "The first phone version is landscape-only",
    "horizontal phone game build",
    "layout is not a supported first-version target",
    "screenshot orientation OK landscape"
)

Assert-FileContains -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md" -Markers @(
    "Mobile phones remain landscape-only for the first playable target",
    "horizontal phone game",
    "portrait UI is not part of the first version",
    "G4 Touch UI pass"
)

Assert-FileContains -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md" -Markers @(
    "horizontal phone game",
    "G4 Touch UI pass",
    "G5 Mobile performance budget",
    "G6 iOS feasibility gate"
)

if ($failures.Count -gt 0) {
    Write-Host "Mobile landscape contract check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) mobile landscape contract check(s) failed."
}

Write-Host "Mobile landscape contract check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
