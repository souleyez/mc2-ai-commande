param(
    [string]$RepoRoot = "",
    [string]$UnityEditorRoot = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ([string]::IsNullOrWhiteSpace($UnityEditorRoot)) {
    $UnityEditorRoot = Join-Path $HOME "Unity\Hub\Editor\6000.4.7f1\Editor"
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

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Require-File {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "Missing file: $RelativePath"
        return $null
    }

    Add-Row -Check $RelativePath -Detail "exists"
    return $path
}

function Require-Text {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or -not $Text.Contains($Needle)) {
        Add-Failure "$Label missing marker: $Needle"
        return
    }

    Add-Row -Check $Label -Detail $Needle
}

$docPath = Require-File -RelativePath "docs-ios-feasibility-2026-06-10.md"
$mobilePlanPath = Require-File -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$projectVersionPath = Require-File -RelativePath "unity-mc2-demo\ProjectSettings\ProjectVersion.txt"

$docText = ""
if ($null -ne $docPath) {
    $docText = Get-Content -LiteralPath $docPath -Raw
}

$mobilePlanText = ""
if ($null -ne $mobilePlanPath) {
    $mobilePlanText = Get-Content -LiteralPath $mobilePlanPath -Raw
}

$projectVersionText = ""
if ($null -ne $projectVersionPath) {
    $projectVersionText = Get-Content -LiteralPath $projectVersionPath -Raw
}

$requiredDocMarkers = @(
    "IOSFeasibilityGate: True",
    "NotAnIOSBuildProof: True",
    "AndroidContinues: True",
    "MacBuildHostRequired: True",
    "XcodeRequired: True",
    "AppleSigningRequired: True",
    "UnityIOSSupportRequired: True",
    "FirstIOSSmoke: Build Xcode project -> install on iOS device -> launch visible-flow battle",
    "Builds/iOS/",
    "visible-flow battle"
)

foreach ($marker in $requiredDocMarkers) {
    Require-Text -Text $docText -Needle $marker -Label "iOS feasibility doc"
}

Require-Text -Text $projectVersionText -Needle "m_EditorVersion: 6000.4.7f1" -Label "Unity project version"

$playbackEngines = Join-Path $UnityEditorRoot "Data\PlaybackEngines"
$iosSupport = Join-Path $playbackEngines "iOSSupport"
$iosSupportInstalled = Test-Path -LiteralPath $iosSupport -PathType Container

if (-not (Test-Path -LiteralPath $UnityEditorRoot -PathType Container)) {
    Add-Failure "Unity editor root missing: $UnityEditorRoot"
}
else {
    Add-Row -Check "Unity editor root" -Detail $UnityEditorRoot
}

if (-not (Test-Path -LiteralPath $playbackEngines -PathType Container)) {
    Add-Failure "Unity playback engines folder missing: $playbackEngines"
}
else {
    $engineNames = @(Get-ChildItem -LiteralPath $playbackEngines -Directory | ForEach-Object { $_.Name } | Sort-Object)
    Add-Row -Check "Unity playback engines" -Detail ($engineNames -join ", ")
}

if ($iosSupportInstalled) {
    Require-Text -Text $docText -Needle "IOSSupportInstalled: True" -Label "iOS support local state"
}
else {
    Require-Text -Text $docText -Needle "IOSSupportInstalled: False" -Label "iOS support local state"
}

$isWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
    [System.Runtime.InteropServices.OSPlatform]::Windows
)

if ($isWindows) {
    Require-Text -Text $docText -Needle "LocalWindowsIOSBuild: Unsupported" -Label "local Windows iOS build state"
}

Require-Text -Text $mobilePlanText -Needle "| G6 | Done | iOS feasibility gate |" -Label "mobile gate order"
Require-Text -Text $mobilePlanText -Needle "F2 map authoring contract" -Label "next task marker"

if ($failures.Count -gt 0) {
    Write-Host "iOS feasibility gate check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) iOS feasibility gate check(s) failed."
}

Write-Host "iOS feasibility gate check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "UnityEditorRoot: $UnityEditorRoot"
Write-Host "IOSSupportInstalled: $iosSupportInstalled"
$rows | Format-Table -AutoSize
