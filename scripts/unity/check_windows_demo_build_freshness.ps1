param(
    [string]$RepoRoot = "",
    [int]$AllowedSkewSeconds = 5
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($AllowedSkewSeconds -lt 0) {
    throw "AllowedSkewSeconds must be zero or greater."
}

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
    return Join-Path $RepoRoot $RelativePath
}

function Format-Time {
    param([datetime]$Value)
    return $Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", [Globalization.CultureInfo]::InvariantCulture)
}

$buildFiles = @(
    "unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe",
    "unity-mc2-demo\Builds\Windows\MC2UnityDemo_Data\Managed\Assembly-CSharp.dll",
    "unity-mc2-demo\Builds\Windows\MC2UnityDemo_Data\globalgamemanagers",
    "unity-mc2-demo\Builds\Windows\MC2UnityDemo_Data\level0",
    "unity-mc2-demo\Builds\Windows\MC2UnityDemo_Data\sharedassets0.assets"
)

foreach ($relativePath in $buildFiles) {
    $path = Resolve-RepoPath -RelativePath $relativePath
    if (-not (Test-Path -LiteralPath $path)) {
        Add-Failure "Missing Windows build output: $relativePath"
    }
}

$dataDir = Resolve-RepoPath -RelativePath "unity-mc2-demo\Builds\Windows\MC2UnityDemo_Data"
if (-not (Test-Path -LiteralPath $dataDir)) {
    Add-Failure "Missing Windows build data folder: unity-mc2-demo\Builds\Windows\MC2UnityDemo_Data"
}

$trackedInputPaths = @(& git -C $RepoRoot ls-files -- `
    "unity-mc2-demo/Assets" `
    "unity-mc2-demo/ProjectSettings" `
    "unity-mc2-demo/Packages" 2>$null)

if ($LASTEXITCODE -ne 0 -or $trackedInputPaths.Count -eq 0) {
    Add-Failure "Could not list tracked Unity build inputs through git."
}

$newestInput = $null
$missingInputs = New-Object System.Collections.Generic.List[string]
foreach ($gitPath in $trackedInputPaths) {
    $relativePath = $gitPath -replace "/", "\"
    if ($relativePath -like "unity-mc2-demo\Assets\Scenes\*.unity") {
        continue
    }

    $path = Resolve-RepoPath -RelativePath $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        [void]$missingInputs.Add($relativePath)
        continue
    }

    $item = Get-Item -LiteralPath $path
    if ($null -eq $newestInput -or $item.LastWriteTimeUtc -gt $newestInput.LastWriteTimeUtc) {
        $newestInput = [pscustomobject]@{
            RelativePath = $relativePath
            LastWriteTimeUtc = $item.LastWriteTimeUtc
        }
    }
}

foreach ($relativePath in $missingInputs) {
    Add-Failure "Tracked Unity input is missing locally: $relativePath"
}

$newestOutput = $null
foreach ($relativePath in $buildFiles) {
    $path = Resolve-RepoPath -RelativePath $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        continue
    }

    $item = Get-Item -LiteralPath $path
    if ($null -eq $newestOutput -or $item.LastWriteTimeUtc -gt $newestOutput.LastWriteTimeUtc) {
        $newestOutput = [pscustomobject]@{
            RelativePath = $relativePath
            LastWriteTimeUtc = $item.LastWriteTimeUtc
        }
    }
}

if ($null -eq $newestInput) {
    Add-Failure "No tracked Unity build input timestamp could be resolved."
}

if ($null -eq $newestOutput) {
    Add-Failure "No Windows build output timestamp could be resolved."
}

if ($null -ne $newestInput -and $null -ne $newestOutput) {
    $freshThreshold = $newestOutput.LastWriteTimeUtc.AddSeconds($AllowedSkewSeconds)
    if ($freshThreshold -lt $newestInput.LastWriteTimeUtc) {
        Add-Failure ("Windows build is stale. Newest input {0} at {1}; newest build output {2} at {3}." -f `
            $newestInput.RelativePath,
            (Format-Time -Value $newestInput.LastWriteTimeUtc),
            $newestOutput.RelativePath,
            (Format-Time -Value $newestOutput.LastWriteTimeUtc))
    }
    else {
        Add-Row -Check "freshness" -Detail ("newest input {0}; newest output {1}" -f $newestInput.RelativePath, $newestOutput.RelativePath)
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Windows demo build freshness check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Windows demo build freshness check(s) failed."
}

Write-Host "Windows demo build freshness check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "NewestInput: $($newestInput.RelativePath) $((Format-Time -Value $newestInput.LastWriteTimeUtc))"
Write-Host "NewestOutput: $($newestOutput.RelativePath) $((Format-Time -Value $newestOutput.LastWriteTimeUtc))"
$rows | Format-Table -AutoSize
