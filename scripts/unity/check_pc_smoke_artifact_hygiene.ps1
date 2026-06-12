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

$expectedOutputRoot = "analysis-output/"
$requiredDocs = @(
    "README.md",
    "BUILD-WIN.md",
    "docs-pc-optimization-plan-2026-06-11.md"
)
$requiredGitignoreMarkers = @(
    "*.log",
    "analysis-output/*saved-account*.json",
    "analysis-output/*validator*.json"
)
$ignoredProbePaths = @(
    "analysis-output/unity-player-pc-evidence-visible-flow.log",
    "analysis-output/unity-build-pc-evidence-package.log",
    "analysis-output/unity-validate-handoff.log",
    "analysis-output/validator-command-file-account.json",
    "analysis-output/mc2_01-saved-account-file-preview.json"
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

function Read-RequiredText {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "$RelativePath missing"
        return ""
    }

    return Get-Content -LiteralPath $path -Raw
}

function Assert-Contains {
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

function Test-PcSmokeArtifactPath {
    param([string]$Path)

    $normalized = To-RepoSlashPath -Path $Path
    if (-not $normalized.StartsWith("analysis-output/", [StringComparison]::OrdinalIgnoreCase)) {
        return $false
    }

    if ($normalized.EndsWith(".log", [StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }

    if ($normalized.EndsWith(".json", [StringComparison]::OrdinalIgnoreCase) `
        -and ($normalized.IndexOf("saved-account", [StringComparison]::OrdinalIgnoreCase) -ge 0 `
            -or $normalized.IndexOf("validator", [StringComparison]::OrdinalIgnoreCase) -ge 0)) {
        return $true
    }

    return $false
}

function Assert-NoPcSmokeArtifacts {
    param(
        [string[]]$Paths,
        [string]$Label
    )

    $pathCount = 0
    if ($null -ne $Paths) {
        $pathCount = @($Paths).Length
    }

    $bad = @($Paths | Where-Object { Test-PcSmokeArtifactPath -Path $_ })
    if ($bad.Count -gt 0) {
        foreach ($path in $bad) {
            Add-Failure "$Label contains PC smoke artifact: $path"
        }
        return
    }

    Add-Row -Check $Label -Detail "$pathCount path(s)"
}

foreach ($relativePath in $requiredDocs) {
    $text = Read-RequiredText -RelativePath $relativePath
    Assert-Contains -Text $text -Needle $expectedOutputRoot -Label "$relativePath PC smoke output documentation"
}

$gitignoreText = Read-RequiredText -RelativePath ".gitignore"
foreach ($marker in $requiredGitignoreMarkers) {
    Assert-Contains -Text $gitignoreText -Needle $marker -Label ".gitignore PC smoke marker"
}

foreach ($path in $ignoredProbePaths) {
    & git -C $RepoRoot check-ignore --quiet -- $path
    if ($LASTEXITCODE -ne 0) {
        Add-Failure "$path is not ignored by git"
    }
    else {
        Add-Row -Check "git ignore rule" -Detail $path
    }
}

$tracked = Get-GitPaths -Arguments @("ls-files") -FailureLabel "git ls-files"
$staged = Get-GitPaths -Arguments @("diff", "--cached", "--name-only", "--diff-filter=ACMR") -FailureLabel "git diff --cached --name-only"
Assert-NoPcSmokeArtifacts -Paths $tracked -Label "tracked source paths"
Assert-NoPcSmokeArtifacts -Paths $staged -Label "staged source paths"

if ($failures.Count -gt 0) {
    Write-Host "PC smoke artifact hygiene check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC smoke artifact hygiene check(s) failed."
}

Write-Host "PC smoke artifact hygiene check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "OutputRoot: $expectedOutputRoot"
$rows | Format-Table -AutoSize
