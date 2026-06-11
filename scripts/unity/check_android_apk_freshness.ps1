param(
    [string]$RepoRoot = "",
    [string]$ApkPath = "",
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

if ([string]::IsNullOrWhiteSpace($ApkPath)) {
    $ApkPath = Join-Path $RepoRoot "unity-mc2-demo\Builds\Android\MC2UnityDemo.apk"
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

$trackedInputPaths = @(& git -C $RepoRoot ls-files -- `
    "unity-mc2-demo/Assets" `
    "unity-mc2-demo/ProjectSettings" `
    "unity-mc2-demo/Packages" 2>$null)

if ($LASTEXITCODE -ne 0 -or $trackedInputPaths.Count -eq 0) {
    Add-Failure "Could not list tracked Unity Android build inputs through git."
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
    Add-Failure "Tracked Unity Android build input is missing locally: $relativePath"
}

if (-not (Test-Path -LiteralPath $ApkPath -PathType Leaf)) {
    Add-Failure "Missing Android APK: $ApkPath"
}

$apk = $null
if (Test-Path -LiteralPath $ApkPath -PathType Leaf) {
    $apk = Get-Item -LiteralPath $ApkPath
    if ($apk.Length -le 0) {
        Add-Failure "Android APK is empty: $ApkPath"
    }
    else {
        Add-Row -Check "apk" -Detail ("{0} bytes" -f $apk.Length)
    }
}

if ($null -eq $newestInput) {
    Add-Failure "No tracked Unity Android build input timestamp could be resolved."
}

if ($null -ne $newestInput -and $null -ne $apk) {
    $freshThreshold = $apk.LastWriteTimeUtc.AddSeconds($AllowedSkewSeconds)
    if ($freshThreshold -lt $newestInput.LastWriteTimeUtc) {
        Add-Failure ("Android APK is stale. Newest input {0} at {1}; APK at {2}." -f `
            $newestInput.RelativePath,
            (Format-Time -Value $newestInput.LastWriteTimeUtc),
            (Format-Time -Value $apk.LastWriteTimeUtc))
    }
    else {
        Add-Row -Check "freshness" -Detail ("newest input {0}; apk {1}" -f $newestInput.RelativePath, (Split-Path -Leaf $ApkPath))
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Android APK freshness check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android APK freshness check(s) failed."
}

Write-Host "Android APK freshness check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "APK: $ApkPath"
Write-Host "NewestInput: $($newestInput.RelativePath) $((Format-Time -Value $newestInput.LastWriteTimeUtc))"
Write-Host "ApkTime: $((Format-Time -Value $apk.LastWriteTimeUtc))"
$rows | Format-Table -AutoSize
