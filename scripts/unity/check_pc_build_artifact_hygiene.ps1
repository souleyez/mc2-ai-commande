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

$expectedBuildDir = "unity-mc2-demo/Builds/Windows/"
$expectedScriptBuildDir = "unity-mc2-demo\Builds\Windows"
$requiredDocs = @(
    "README.md",
    "BUILD-WIN.md",
    "docs-pc-optimization-plan-2026-06-11.md"
)
$requiredGitignoreMarkers = @(
    "unity-mc2-demo/Build/",
    "unity-mc2-demo/Builds/",
    "*.exe",
    "*.dll",
    "*.pdb"
)
$ignoredProbePaths = @(
    "unity-mc2-demo/Builds/Windows/MC2UnityDemo.exe",
    "unity-mc2-demo/Builds/Windows/MC2UnityDemo_Data/Managed/Assembly-CSharp.dll",
    "unity-mc2-demo/Build/Windows/MC2UnityDemo.exe"
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

function Test-PcBuildArtifactPath {
    param([string]$Path)

    $normalized = To-RepoSlashPath -Path $Path
    return $normalized.StartsWith("unity-mc2-demo/Build/", [StringComparison]::OrdinalIgnoreCase) `
        -or $normalized.StartsWith("unity-mc2-demo/Builds/", [StringComparison]::OrdinalIgnoreCase)
}

function Assert-NoPcBuildArtifacts {
    param(
        [string[]]$Paths,
        [string]$Label
    )

    $pathCount = 0
    if ($null -ne $Paths) {
        $pathCount = @($Paths).Length
    }

    $bad = @($Paths | Where-Object { Test-PcBuildArtifactPath -Path $_ })
    if ($bad.Count -gt 0) {
        foreach ($path in $bad) {
            Add-Failure "$Label contains PC build artifact: $path"
        }
        return
    }

    Add-Row -Check $Label -Detail "$pathCount path(s)"
}

$runScript = Read-RequiredText -RelativePath "scripts\unity\run_windows_demo.ps1"
Assert-Contains -Text $runScript -Needle $expectedScriptBuildDir -Label "run_windows_demo.ps1 build output path"

$freshnessScript = Read-RequiredText -RelativePath "scripts\unity\check_windows_demo_build_freshness.ps1"
Assert-Contains -Text $freshnessScript -Needle $expectedScriptBuildDir -Label "check_windows_demo_build_freshness.ps1 build output path"

foreach ($relativePath in $requiredDocs) {
    $text = Read-RequiredText -RelativePath $relativePath
    Assert-Contains -Text $text -Needle $expectedBuildDir -Label "$relativePath Windows build output documentation"
}

$gitignoreText = Read-RequiredText -RelativePath ".gitignore"
foreach ($marker in $requiredGitignoreMarkers) {
    Assert-Contains -Text $gitignoreText -Needle $marker -Label ".gitignore PC build marker"
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
Assert-NoPcBuildArtifacts -Paths $tracked -Label "tracked source paths"
Assert-NoPcBuildArtifacts -Paths $staged -Label "staged source paths"

if ($failures.Count -gt 0) {
    Write-Host "PC build artifact hygiene check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC build artifact hygiene check(s) failed."
}

Write-Host "PC build artifact hygiene check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "BuildDir: $expectedBuildDir"
$rows | Format-Table -AutoSize
