param(
    [string]$RepoRoot = "",
    [string[]]$CleanPaths = @(),
    [switch]$CheckDevBuild,
    [string]$DevBuildPath = "",
    [int]$MaxFindings = 200
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($MaxFindings -lt 1) {
    throw "MaxFindings must be at least 1."
}

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

$checker = Join-Path $RepoRoot "scripts\content-pack\check_public_content_boundary.ps1"
if (-not (Test-Path -LiteralPath $checker)) {
    throw "Missing public boundary checker: $checker"
}

if ($CleanPaths.Count -eq 0) {
    $CleanPaths = @(
        "content-packs\project-owned-starter.example.json",
        "content-packs\project-owned-text-safe-slice.example.json",
        "content-packs\project-owned-visual-slice.example.json",
        "content-packs\project-owned-art-safe-slice.example.json"
    )
}

if ([string]::IsNullOrWhiteSpace($DevBuildPath)) {
    $DevBuildPath = Join-Path $RepoRoot "unity-mc2-demo\Builds\Windows"
}

$failures = New-Object System.Collections.Generic.List[string]
$checkedCleanPaths = New-Object System.Collections.Generic.List[string]

function Add-Failure {
    param([string]$Message)
    [void]$failures.Add($Message)
}

function Resolve-RepoRelativePath {
    param([string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return (Resolve-Path -LiteralPath $Path).Path
    }

    return (Resolve-Path -LiteralPath (Join-Path $RepoRoot $Path)).Path
}

function Invoke-PublicBoundaryCheck {
    param([string]$TargetPath)

    $arguments = @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $checker,
        "-Path",
        $TargetPath,
        "-DryRun",
        "-MaxFindings",
        $MaxFindings.ToString([Globalization.CultureInfo]::InvariantCulture)
    )

    $output = & powershell @arguments 2>&1
    $exitCode = $LASTEXITCODE

    return [pscustomobject]@{
        ExitCode = $exitCode
        Output = @($output | ForEach-Object { $_.ToString() })
    }
}

function Get-ResultLine {
    param([string[]]$Output)

    foreach ($line in $Output) {
        if ($line -match "^Result:\s+") {
            return $line
        }
    }

    return ""
}

foreach ($path in $CleanPaths) {
    try {
        $target = Resolve-RepoRelativePath -Path $path
    }
    catch {
        Add-Failure "Clean boundary target missing: $path"
        continue
    }

    $result = Invoke-PublicBoundaryCheck -TargetPath $target
    $resultLine = Get-ResultLine -Output $result.Output
    if ($result.ExitCode -ne 0 -or $resultLine -ne "Result: OK") {
        Add-Failure "Clean boundary target failed: $path"
        foreach ($line in $result.Output) {
            Add-Failure "  $line"
        }
        continue
    }

    [void]$checkedCleanPaths.Add($path)
}

$devBuildResult = $null
if ($CheckDevBuild) {
    try {
        $devBuild = Resolve-RepoRelativePath -Path $DevBuildPath
    }
    catch {
        Add-Failure "Development build path missing: $DevBuildPath"
        $devBuild = $null
    }

    if ($null -ne $devBuild) {
        $devBuildResult = Invoke-PublicBoundaryCheck -TargetPath $devBuild
        $devResultLine = Get-ResultLine -Output $devBuildResult.Output
        if ($devBuildResult.ExitCode -eq 0 -or $devResultLine -ne "Result: FAILED") {
            Add-Failure "Development build did not fail the public boundary check as expected: $DevBuildPath"
            foreach ($line in $devBuildResult.Output) {
                Add-Failure "  $line"
            }
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Controlled demo public boundary preflight failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) controlled demo public boundary preflight check(s) failed."
}

Write-Host "Controlled demo public boundary preflight OK."
Write-Host "Clean metadata targets:"
foreach ($path in $checkedCleanPaths) {
    Write-Host " - $path"
}

if ($CheckDevBuild) {
    $findingLine = ""
    foreach ($line in $devBuildResult.Output) {
        if ($line -match "^Findings:\s+") {
            $findingLine = $line
            break
        }
    }

    if ([string]::IsNullOrWhiteSpace($findingLine)) {
        $findingLine = "Findings: not reported"
    }

    Write-Host "Development build public boundary status: expected FAILED."
    Write-Host $findingLine
}
