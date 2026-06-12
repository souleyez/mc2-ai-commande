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

$expectedLogPath = "analysis-output/windows-demo-run.log"
$expectedScriptPath = "analysis-output\windows-demo-run.log"
$requiredDocs = @(
    "README.md",
    "BUILD-WIN.md",
    "docs-pc-optimization-plan-2026-06-11.md"
)
$requiredGitignoreMarkers = @(
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

function Test-LaunchLogPath {
    param([string]$Path)

    $normalized = To-RepoSlashPath -Path $Path
    return $normalized.StartsWith("analysis-output/", [StringComparison]::OrdinalIgnoreCase) `
        -and $normalized.EndsWith(".log", [StringComparison]::OrdinalIgnoreCase)
}

function Assert-NoLaunchLogs {
    param(
        [string[]]$Paths,
        [string]$Label
    )

    $pathCount = 0
    if ($null -ne $Paths) {
        $pathCount = @($Paths).Length
    }

    $bad = @($Paths | Where-Object { Test-LaunchLogPath -Path $_ })
    if ($bad.Count -gt 0) {
        foreach ($path in $bad) {
            Add-Failure "$Label contains PC launch log: $path"
        }
        return
    }

    Add-Row -Check $Label -Detail "$pathCount path(s)"
}

$runScript = Read-RequiredText -RelativePath "scripts\unity\run_windows_demo.ps1"
Assert-Contains -Text $runScript -Needle $expectedScriptPath -Label "run_windows_demo.ps1 default log path"

foreach ($relativePath in $requiredDocs) {
    $text = Read-RequiredText -RelativePath $relativePath
    Assert-Contains -Text $text -Needle $expectedLogPath -Label "$relativePath launch log documentation"
}

$gitignoreText = Read-RequiredText -RelativePath ".gitignore"
foreach ($marker in $requiredGitignoreMarkers) {
    Assert-Contains -Text $gitignoreText -Needle $marker -Label ".gitignore launch log marker"
}

& git -C $RepoRoot check-ignore --quiet -- $expectedLogPath
if ($LASTEXITCODE -ne 0) {
    Add-Failure "$expectedLogPath is not ignored by git"
}
else {
    Add-Row -Check "git ignore rule" -Detail $expectedLogPath
}

$tracked = Get-GitPaths -Arguments @("ls-files") -FailureLabel "git ls-files"
$staged = Get-GitPaths -Arguments @("diff", "--cached", "--name-only", "--diff-filter=ACMR") -FailureLabel "git diff --cached --name-only"
Assert-NoLaunchLogs -Paths $tracked -Label "tracked source paths"
Assert-NoLaunchLogs -Paths $staged -Label "staged source paths"

if ($failures.Count -gt 0) {
    Write-Host "PC launch log hygiene check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC launch log hygiene check(s) failed."
}

Write-Host "PC launch log hygiene check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "LogPath: $expectedLogPath"
$rows | Format-Table -AutoSize
