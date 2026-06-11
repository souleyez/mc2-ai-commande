param(
    [string]$RepoRoot = "",
    [string]$AndroidPlayerPath = "",
    [string]$ExpectedBuildTools = "36.0.0",
    [string]$ExpectedPlatform = "android-36"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ([string]::IsNullOrWhiteSpace($AndroidPlayerPath)) {
    $AndroidPlayerPath = Join-Path $HOME "Unity\Hub\Editor\6000.4.7f1\Editor\Data\PlaybackEngines\AndroidPlayer"
}

$failures = New-Object System.Collections.Generic.List[string]
$rows = New-Object System.Collections.Generic.List[object]

function Add-Failure {
    param([string]$Message)
    [void]$failures.Add($Message)
}

function Add-Row {
    param(
        [string]$Check,
        [string]$Detail
    )

    [void]$rows.Add([pscustomobject]@{
        Check = $Check
        Status = "OK"
        Detail = $Detail
    })
}

function Invoke-Tool {
    param(
        [string]$Path,
        [string[]]$Arguments
    )

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $output = & $Path @Arguments 2>&1
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

function Require-Path {
    param(
        [string]$Path,
        [string]$Label,
        [switch]$Leaf
    )

    $pathType = if ($Leaf) { "Leaf" } else { "Container" }
    if (-not (Test-Path -LiteralPath $Path -PathType $pathType)) {
        Add-Failure ("Missing {0}: {1}" -f $Label, $Path)
        return $false
    }

    Add-Row -Check $Label -Detail $Path
    return $true
}

$sdkPath = Join-Path $AndroidPlayerPath "SDK"
$buildToolsPath = Join-Path $sdkPath "build-tools\$ExpectedBuildTools"
$platformPath = Join-Path $sdkPath "platforms\$ExpectedPlatform"
$adbPath = Join-Path $sdkPath "platform-tools\adb.exe"
$aaptPath = Join-Path $buildToolsPath "aapt.exe"
$apksignerPath = Join-Path $buildToolsPath "apksigner.bat"
$androidJarPath = Join-Path $platformPath "android.jar"
$ndkPath = Join-Path $AndroidPlayerPath "NDK"
$openJdkPath = Join-Path $AndroidPlayerPath "OpenJDK"

[void](Require-Path -Path $AndroidPlayerPath -Label "AndroidPlayer")
[void](Require-Path -Path $sdkPath -Label "SDK")
[void](Require-Path -Path $buildToolsPath -Label "build-tools")
[void](Require-Path -Path $platformPath -Label "platform")
[void](Require-Path -Path $ndkPath -Label "NDK")
[void](Require-Path -Path $openJdkPath -Label "OpenJDK")
$adbExists = Require-Path -Path $adbPath -Label "adb" -Leaf
$aaptExists = Require-Path -Path $aaptPath -Label "aapt" -Leaf
$apksignerExists = Require-Path -Path $apksignerPath -Label "apksigner" -Leaf
[void](Require-Path -Path $androidJarPath -Label "android.jar" -Leaf)

if ($adbExists) {
    $adbVersion = Invoke-Tool -Path $adbPath -Arguments @("version")
    $adbOutput = $adbVersion.Output -join "; "
    if ($adbVersion.ExitCode -ne 0 -or $adbOutput -notlike "*Android Debug Bridge version*") {
        Add-Failure "adb version check failed: $adbOutput"
    }
    elseif ($adbOutput -notlike "*Version 37.*") {
        Add-Failure "adb version drifted from the current Unity Android SDK expectation: $adbOutput"
    }
    else {
        Add-Row -Check "adb version" -Detail $adbOutput
    }
}

if ($aaptExists) {
    $aaptVersion = Invoke-Tool -Path $aaptPath -Arguments @("version")
    $aaptOutput = $aaptVersion.Output -join "; "
    if ($aaptVersion.ExitCode -ne 0 -or $aaptOutput -notlike "*Android Asset Packaging Tool*") {
        Add-Failure "aapt version check failed: $aaptOutput"
    }
    else {
        Add-Row -Check "aapt version" -Detail $aaptOutput
    }
}

if ($apksignerExists) {
    $apksignerVersion = Invoke-Tool -Path $apksignerPath -Arguments @("version")
    $apksignerOutput = $apksignerVersion.Output -join "; "
    if ($apksignerVersion.ExitCode -ne 0 -or [string]::IsNullOrWhiteSpace($apksignerOutput)) {
        Add-Failure "apksigner version check failed: $apksignerOutput"
    }
    else {
        Add-Row -Check "apksigner version" -Detail $apksignerOutput
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Android SDK tooling check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android SDK tooling check(s) failed."
}

Write-Host "Android SDK tooling check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "AndroidPlayer: $AndroidPlayerPath"
Write-Host "ExpectedBuildTools: $ExpectedBuildTools"
Write-Host "ExpectedPlatform: $ExpectedPlatform"
$rows | Format-Table -AutoSize
