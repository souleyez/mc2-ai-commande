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

$readinessScript = Join-Path $PSScriptRoot "check_android_g3_readiness.ps1"
if (-not (Test-Path -LiteralPath $readinessScript)) {
    throw "Missing Android G3 readiness checker: $readinessScript"
}

$previousErrorActionPreference = $ErrorActionPreference
$ErrorActionPreference = "Continue"
try {
    $output = & powershell -NoProfile -ExecutionPolicy Bypass -File $readinessScript -RepoRoot $RepoRoot -RequireDevice 2>&1
    $exitCode = $LASTEXITCODE
}
finally {
    $ErrorActionPreference = $previousErrorActionPreference
}
$lines = @($output | ForEach-Object { $_.ToString() })
$text = $lines -join [Environment]::NewLine

if ($exitCode -eq 0) {
    if ($text -notlike "*Android G3 readiness check OK.*") {
        Write-Host "Android G3 device requirement check failed."
        foreach ($line in $lines) {
            Write-Host " - $line"
        }

        throw "Android G3 readiness passed without the expected OK marker."
    }

    Write-Host "Android G3 device requirement check OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "Detail: authorized Android phone is available for G3 readiness."
    return
}

$hasDeviceRequirementMarker = $text -like "*Android G3 readiness requires a connected and authorized Android phone.*"
$hasNoDeviceMarker = $text -like "*No Android device rows.*" -or $text -like "*No Android device found.*" -or $text -like "*No authorized Android device found.*"

if ($hasDeviceRequirementMarker -or $hasNoDeviceMarker) {
    Write-Host "Android G3 device requirement check waiting on device."
    Write-Host "Repo: $RepoRoot"
    Write-Host "Detail: strict G3 readiness correctly requires an authorized Android phone."
    return
}

Write-Host "Android G3 device requirement check failed."
foreach ($line in $lines) {
    Write-Host " - $line"
}

throw "Android G3 readiness failed for a reason other than the missing authorized device."
