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

function Test-ForbiddenPath {
    param([string]$Path)

    $normalized = To-RepoSlashPath -Path $Path

    $forbiddenPrefixes = @(
        "analysis-output/",
        "runtime-shell-dev/",
        "mc2-run64-dev/",
        "release-run/",
        "unity-mc2-demo/Build/",
        "unity-mc2-demo/Builds/",
        "unity-mc2-demo/Library/",
        "unity-mc2-demo/Logs/",
        "unity-mc2-demo/Temp/",
        "unity-mc2-demo/UserSettings/",
        "unity-mc2-demo/MemoryCaptures/",
        "unity-mc2-demo/Assets/PrivateReferenceArt/"
    )

    foreach ($prefix in $forbiddenPrefixes) {
        if ($normalized.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    if ($normalized.StartsWith("content-packs/", [StringComparison]::OrdinalIgnoreCase) `
        -and -not $normalized.EndsWith(".example.json", [StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }

    foreach ($extension in @(".apk", ".aab", ".log", ".tga", ".obj")) {
        if ($normalized.EndsWith($extension, [StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

function Assert-NoForbiddenPaths {
    param(
        [string[]]$Paths,
        [string]$Label
    )

    $bad = @($Paths | Where-Object { Test-ForbiddenPath -Path $_ })
    if ($bad.Count -gt 0) {
        foreach ($path in $bad) {
            Add-Failure "$Label contains generated/private path: $path"
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
            Add-Failure ".gitignore missing marker: $marker"
        }
    }

    Add-Row -Check ".gitignore demo hygiene markers" -Detail "$($Markers.Count) marker(s)"
}

$tracked = @(& git -C $RepoRoot ls-files | ForEach-Object { $_.ToString() })
if ($LASTEXITCODE -ne 0) {
    throw "git ls-files failed with exit code $LASTEXITCODE"
}

$staged = @(& git -C $RepoRoot diff --cached --name-only --diff-filter=ACMR | ForEach-Object { $_.ToString() })
if ($LASTEXITCODE -ne 0) {
    throw "git diff --cached --name-only failed with exit code $LASTEXITCODE"
}

Assert-NoForbiddenPaths -Paths $tracked -Label "tracked source paths"
Assert-NoForbiddenPaths -Paths $staged -Label "staged source paths"

$gitignorePath = Join-Path $RepoRoot ".gitignore"
if (-not (Test-Path -LiteralPath $gitignorePath)) {
    Add-Failure ".gitignore missing"
}
else {
    $gitignore = Get-Content -LiteralPath $gitignorePath -Raw
    Assert-GitignoreContains -Text $gitignore -Markers @(
        "analysis-output/reference-visual-captures/",
        "analysis-output/unity-reference-art/",
        "analysis-output/*.png",
        "unity-mc2-demo/Builds/",
        "unity-mc2-demo/Assets/PrivateReferenceArt/",
        "content-packs/*",
        "!content-packs/*.example.json",
        "mc2-run64-dev/",
        "*.log",
        "*.tga",
        "*.obj"
    )
}

if ($failures.Count -gt 0) {
    Write-Host "Demo source hygiene check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) demo source hygiene check(s) failed."
}

Write-Host "Demo source hygiene check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
