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

function To-RepoSlashPath {
    param([string]$Path)
    return ($Path -replace "\\", "/").TrimStart("./")
}

function Test-AndroidSmokeArtifactPath {
    param([string]$Path)

    $normalized = To-RepoSlashPath -Path $Path
    $patterns = @(
        "analysis-output/android-device-smoke.log",
        "analysis-output/android-device-smoke*.log",
        "analysis-output/android-device-smoke*.png",
        "analysis-output/*android*smoke*.log",
        "analysis-output/*android*smoke*.png",
        "unity-mc2-demo/Builds/Android/*",
        "*.apk",
        "*.aab"
    )

    foreach ($pattern in $patterns) {
        if ($normalized -like $pattern) {
            return $true
        }
    }

    return $false
}

function Assert-NoAndroidSmokeArtifacts {
    param(
        [string[]]$Paths,
        [string]$Label
    )

    $bad = @($Paths | Where-Object { Test-AndroidSmokeArtifactPath -Path $_ })
    if ($bad.Count -gt 0) {
        foreach ($path in $bad) {
            Add-Failure "$Label contains Android smoke artifact: $path"
        }
        return
    }

    Add-Row -Check $Label -Detail "$($Paths.Count) path(s)"
}

function Assert-GitignoreContains {
    param(
        [string]$Text,
        [string[]]$Markers
    )

    foreach ($marker in $Markers) {
        if ($Text.IndexOf($marker, [StringComparison]::Ordinal) -lt 0) {
            Add-Failure ".gitignore missing Android smoke artifact marker: $marker"
        }
    }

    Add-Row -Check ".gitignore Android smoke artifact markers" -Detail "$($Markers.Count) marker(s)"
}

$tracked = @(& git -C $RepoRoot ls-files | ForEach-Object { $_.ToString() })
if ($LASTEXITCODE -ne 0) {
    throw "git ls-files failed with exit code $LASTEXITCODE"
}

$staged = @(& git -C $RepoRoot diff --cached --name-only --diff-filter=ACMR | ForEach-Object { $_.ToString() })
if ($LASTEXITCODE -ne 0) {
    throw "git diff --cached --name-only failed with exit code $LASTEXITCODE"
}

Assert-NoAndroidSmokeArtifacts -Paths $tracked -Label "tracked source paths"
Assert-NoAndroidSmokeArtifacts -Paths $staged -Label "staged source paths"

$gitignorePath = Join-Path $RepoRoot ".gitignore"
if (-not (Test-Path -LiteralPath $gitignorePath)) {
    Add-Failure ".gitignore missing"
}
else {
    $gitignore = Get-Content -LiteralPath $gitignorePath -Raw
    Assert-GitignoreContains -Text $gitignore -Markers @(
        "analysis-output/*.png",
        "*.log",
        "*.apk",
        "*.aab",
        "unity-mc2-demo/Builds/"
    )
}

if ($failures.Count -gt 0) {
    Write-Host "Android smoke artifact hygiene check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android smoke artifact hygiene check(s) failed."
}

Write-Host "Android smoke artifact hygiene check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
