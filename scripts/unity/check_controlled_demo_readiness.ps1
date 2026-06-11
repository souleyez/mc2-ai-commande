param(
    [string]$RepoRoot = "",
    [switch]$SkipEvidence,
    [switch]$SkipPublicBoundary,
    [switch]$SkipDevBuildBoundary,
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

$failures = New-Object System.Collections.Generic.List[string]
$rows = New-Object System.Collections.Generic.List[object]

function Add-Failure {
    param([string]$Message)
    [void]$failures.Add($Message)
}

function Invoke-ReadinessStep {
    param(
        [string]$Name,
        [string]$ScriptPath,
        [string[]]$Arguments = @(),
        [string[]]$RequiredMarkers = @()
    )

    if (-not (Test-Path -LiteralPath $ScriptPath)) {
        Add-Failure "$Name script missing: $ScriptPath"
        return
    }

    $processArgs = @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $ScriptPath
    ) + $Arguments

    $output = & powershell @processArgs 2>&1
    $exitCode = $LASTEXITCODE
    $lines = @($output | ForEach-Object { $_.ToString() })
    $joined = $lines -join [Environment]::NewLine

    $missingMarkers = New-Object System.Collections.Generic.List[string]
    foreach ($marker in $RequiredMarkers) {
        if ($joined -notlike "*$marker*") {
            [void]$missingMarkers.Add($marker)
        }
    }

    if ($exitCode -ne 0 -or $missingMarkers.Count -gt 0) {
        Add-Failure "$Name failed with exit code $exitCode."
        foreach ($marker in $missingMarkers) {
            Add-Failure "$Name missing marker: $marker"
        }

        foreach ($line in $lines) {
            Add-Failure "  $line"
        }
        return
    }

    [void]$rows.Add([pscustomobject]@{
        Step = $Name
        Status = "OK"
        Markers = ($RequiredMarkers -join "; ")
    })
}

$launchScript = Join-Path $RepoRoot "scripts\unity\run_windows_demo.ps1"
$evidenceScript = Join-Path $RepoRoot "scripts\unity\check_controlled_demo_evidence.ps1"
$publicBoundaryScript = Join-Path $RepoRoot "scripts\content-pack\check_controlled_demo_public_boundary.ps1"

Invoke-ReadinessStep `
    -Name "Windows launch preflight" `
    -ScriptPath $launchScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-CheckOnly") `
    -RequiredMarkers @("Windows demo launch preflight OK.")

if (-not $SkipEvidence) {
    Invoke-ReadinessStep `
        -Name "Controlled demo evidence" `
        -ScriptPath $evidenceScript `
        -Arguments @("-RepoRoot", $RepoRoot) `
        -RequiredMarkers @("Controlled demo evidence check OK.")
}

if (-not $SkipPublicBoundary) {
    $boundaryArgs = @(
        "-RepoRoot",
        $RepoRoot,
        "-MaxFindings",
        $MaxFindings.ToString([Globalization.CultureInfo]::InvariantCulture)
    )

    $requiredBoundaryMarkers = @("Controlled demo public boundary preflight OK.")
    if (-not $SkipDevBuildBoundary) {
        $boundaryArgs += "-CheckDevBuild"
        $requiredBoundaryMarkers += "Development build public boundary status: expected FAILED."
    }

    Invoke-ReadinessStep `
        -Name "Public boundary preflight" `
        -ScriptPath $publicBoundaryScript `
        -Arguments $boundaryArgs `
        -RequiredMarkers $requiredBoundaryMarkers
}

if ($failures.Count -gt 0) {
    Write-Host "Controlled demo readiness preflight failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) controlled demo readiness preflight check(s) failed."
}

Write-Host "Controlled demo readiness preflight OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
