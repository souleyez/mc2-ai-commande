param(
    [string]$RepoRoot = "",
    [string]$ApkPath = "",
    [long]$MinApkBytes = 1048576,
    [long]$MaxApkBytes = 104857600
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($MinApkBytes -lt 0) {
    throw "MinApkBytes must be zero or greater."
}

if ($MaxApkBytes -le 0) {
    throw "MaxApkBytes must be greater than zero."
}

if ($MinApkBytes -gt $MaxApkBytes) {
    throw "MinApkBytes cannot be greater than MaxApkBytes."
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

function Format-Bytes {
    param([long]$Bytes)

    $mib = [double]$Bytes / 1048576
    return ("{0} bytes ({1:N2} MiB)" -f $Bytes, $mib)
}

if (-not (Test-Path -LiteralPath $ApkPath -PathType Leaf)) {
    Add-Failure "Missing Android APK: $ApkPath"
}
else {
    $apk = Get-Item -LiteralPath $ApkPath
    if ($apk.Length -lt $MinApkBytes) {
        Add-Failure "Android APK is below the minimum plausible size. Actual $(Format-Bytes -Bytes $apk.Length), minimum $(Format-Bytes -Bytes $MinApkBytes)."
    }
    elseif ($apk.Length -gt $MaxApkBytes) {
        Add-Failure "Android APK exceeds the current mobile demo size budget. Actual $(Format-Bytes -Bytes $apk.Length), maximum $(Format-Bytes -Bytes $MaxApkBytes)."
    }
    else {
        Add-Row -Check "APK size" -Detail (Format-Bytes -Bytes $apk.Length)
        Add-Row -Check "size budget" -Detail ("min {0}; max {1}" -f (Format-Bytes -Bytes $MinApkBytes), (Format-Bytes -Bytes $MaxApkBytes))
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Android APK size budget check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android APK size budget check(s) failed."
}

Write-Host "Android APK size budget check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "APK: $ApkPath"
Write-Host "MinApkBytes: $MinApkBytes"
Write-Host "MaxApkBytes: $MaxApkBytes"
$rows | Format-Table -AutoSize
