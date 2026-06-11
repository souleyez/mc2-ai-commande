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

$artifactDirs = @(
    "analysis-output/reference-visual-captures/",
    "analysis-output/reference-visual-captures-no-placeholders/",
    "analysis-output/pc-visual-sanity-selftest/"
)

$requiredGitignoreMarkers = @(
    "analysis-output/reference-visual-captures/",
    "analysis-output/reference-visual-captures-no-placeholders/",
    "analysis-output/pc-visual-sanity-selftest/",
    "analysis-output/*.png",
    "*.log"
)

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

function To-RepoSlashPath {
    param([string]$Path)
    return ($Path -replace "\\", "/").TrimStart("./")
}

function Get-GitPaths {
    param(
        [string[]]$Arguments,
        [string]$FailureLabel
    )

    $output = @(& git -C $RepoRoot @Arguments 2>$null | ForEach-Object { $_.ToString() })
    if ($LASTEXITCODE -ne 0) {
        throw "$FailureLabel failed with exit code $LASTEXITCODE"
    }

    return $output
}

function Test-CaptureArtifactPath {
    param([string]$Path)

    $normalized = To-RepoSlashPath -Path $Path
    foreach ($dir in $artifactDirs) {
        if ($normalized.StartsWith($dir, [StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

function Assert-NoCaptureArtifacts {
    param(
        [string[]]$Paths,
        [string]$Label
    )

    $pathCount = 0
    if ($null -ne $Paths) {
        $pathCount = @($Paths).Length
    }

    $bad = @($Paths | Where-Object { Test-CaptureArtifactPath -Path $_ })
    if ($bad.Count -gt 0) {
        foreach ($path in $bad) {
            Add-Failure "$Label contains PC capture artifact: $path"
        }
        return
    }

    Add-Row -Check $Label -Detail "$pathCount path(s)"
}

function Assert-GitignoreContains {
    param([string]$Text)

    foreach ($marker in $requiredGitignoreMarkers) {
        if ($Text.IndexOf($marker, [StringComparison]::Ordinal) -lt 0) {
            Add-Failure ".gitignore missing PC capture artifact marker: $marker"
        }
    }

    Add-Row -Check ".gitignore PC capture artifact markers" -Detail "$($requiredGitignoreMarkers.Count) marker(s)"
}

function Get-ExistingCaptureArtifacts {
    $paths = New-Object System.Collections.Generic.List[string]
    $repoPrefix = $RepoRoot.TrimEnd("\", "/") + [IO.Path]::DirectorySeparatorChar
    foreach ($dir in $artifactDirs) {
        $absoluteDir = Resolve-RepoPath -RelativePath $dir
        if (-not (Test-Path -LiteralPath $absoluteDir -PathType Container)) {
            continue
        }

        Get-ChildItem -LiteralPath $absoluteDir -Recurse -File | ForEach-Object {
            if (-not $_.FullName.StartsWith($repoPrefix, [StringComparison]::OrdinalIgnoreCase)) {
                Add-Failure "PC capture artifact is outside repo root: $($_.FullName)"
                return
            }

            $relativePath = $_.FullName.Substring($repoPrefix.Length)
            [void]$paths.Add((To-RepoSlashPath -Path $relativePath))
        }
    }

    return $paths.ToArray()
}

function Assert-IgnoredArtifacts {
    param([string[]]$Paths)

    $pathCount = 0
    if ($null -ne $Paths) {
        $pathCount = @($Paths).Length
    }

    foreach ($path in $Paths) {
        & git -C $RepoRoot check-ignore --quiet -- $path
        if ($LASTEXITCODE -ne 0) {
            Add-Failure "PC capture artifact is not ignored by git: $path"
        }
    }

    Add-Row -Check "existing PC capture artifacts ignored" -Detail "$pathCount artifact(s)"
}

$tracked = Get-GitPaths -Arguments @("ls-files") -FailureLabel "git ls-files"
$staged = Get-GitPaths -Arguments @("diff", "--cached", "--name-only", "--diff-filter=ACMR") -FailureLabel "git diff --cached --name-only"

Assert-NoCaptureArtifacts -Paths $tracked -Label "tracked source paths"
Assert-NoCaptureArtifacts -Paths $staged -Label "staged source paths"

$gitignorePath = Resolve-RepoPath -RelativePath ".gitignore"
if (-not (Test-Path -LiteralPath $gitignorePath -PathType Leaf)) {
    Add-Failure ".gitignore missing"
}
else {
    $gitignore = Get-Content -LiteralPath $gitignorePath -Raw
    Assert-GitignoreContains -Text $gitignore
}

$existingArtifacts = @(Get-ExistingCaptureArtifacts)
Assert-IgnoredArtifacts -Paths $existingArtifacts

if ($failures.Count -gt 0) {
    Write-Host "PC capture artifact hygiene check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC capture artifact hygiene check(s) failed."
}

Write-Host "PC capture artifact hygiene check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
