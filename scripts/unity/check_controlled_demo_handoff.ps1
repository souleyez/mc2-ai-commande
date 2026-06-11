param(
    [string]$RepoRoot = "",
    [switch]$RunReadiness
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

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot $RelativePath
}

function Assert-FileExists {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path)) {
        Add-Failure "Missing file: $RelativePath"
        return $null
    }

    return $path
}

function Assert-FileContains {
    param(
        [string]$RelativePath,
        [string[]]$Markers
    )

    $path = Assert-FileExists -RelativePath $RelativePath
    if ($null -eq $path) {
        return
    }

    $text = Get-Content -LiteralPath $path -Raw
    $missing = New-Object System.Collections.Generic.List[string]
    foreach ($marker in $Markers) {
        if (-not $text.Contains($marker)) {
            Add-Failure "$RelativePath missing marker: $marker"
            [void]$missing.Add($marker)
        }
    }

    if ($missing.Count -eq 0) {
        [void]$rows.Add([pscustomobject]@{
            Check = $RelativePath
            Status = "OK"
            Detail = "$($Markers.Count) marker(s)"
        })
    }
}

function Assert-FileDoesNotContain {
    param(
        [string]$RelativePath,
        [string[]]$ForbiddenMarkers
    )

    $path = Assert-FileExists -RelativePath $RelativePath
    if ($null -eq $path) {
        return
    }

    $text = Get-Content -LiteralPath $path -Raw
    $found = New-Object System.Collections.Generic.List[string]
    foreach ($marker in $ForbiddenMarkers) {
        if ($text.Contains($marker)) {
            Add-Failure "$RelativePath still contains stale marker: $marker"
            [void]$found.Add($marker)
        }
    }

    if ($found.Count -eq 0) {
        [void]$rows.Add([pscustomobject]@{
            Check = "$RelativePath stale markers"
            Status = "OK"
            Detail = "$($ForbiddenMarkers.Count) forbidden marker(s)"
        })
    }
}

function Assert-ScriptExists {
    param([string]$RelativePath)

    $path = Assert-FileExists -RelativePath $RelativePath
    if ($null -ne $path) {
        [void]$rows.Add([pscustomobject]@{
            Check = $RelativePath
            Status = "OK"
            Detail = "script exists"
        })
    }
}

Assert-ScriptExists -RelativePath "scripts\unity\run_windows_demo.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_controlled_demo_evidence.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_controlled_demo_readiness.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
Assert-ScriptExists -RelativePath "scripts\content-pack\check_controlled_demo_public_boundary.ps1"

Assert-FileContains -RelativePath "README.md" -Markers @(
    "AI RTS Commander Lab",
    "PC1-PC9",
    "check_controlled_demo_handoff.ps1",
    "check_controlled_demo_readiness.ps1"
)

Assert-FileContains -RelativePath "BUILD-WIN.md" -Markers @(
    "Current Unity 6 Windows Demo handoff",
    "check_controlled_demo_handoff.ps1",
    "Controlled demo handoff consistency check OK",
    "Controlled demo readiness preflight OK"
)

Assert-FileContains -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md" -Markers @(
    "PC1-PC9",
    "Add controlled demo handoff consistency check",
    "check_controlled_demo_handoff.ps1",
    "PC10"
)

Assert-FileContains -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md" -Markers @(
    "PC1-PC9",
    "Add controlled demo handoff consistency check",
    "check_controlled_demo_handoff.ps1",
    "PC10"
)

Assert-FileContains -RelativePath "docs-pc-optimization-plan-2026-06-11.md" -Markers @(
    "sealed through PC9",
    "Add controlled demo handoff consistency check",
    "check_controlled_demo_handoff.ps1"
)

Assert-FileContains -RelativePath "docs-playable-demo-investor-evidence-2026-06-07.md" -Markers @(
    "Readiness preflight",
    "Handoff consistency",
    "check_controlled_demo_handoff.ps1"
)

Assert-FileContains -RelativePath "docs-machine-handoff-plan-2026-06-07.md" -Markers @(
    "PC1-PC9",
    "G3 Run Android device smoke",
    "check_controlled_demo_handoff.ps1",
    "check_controlled_demo_readiness.ps1"
)

Assert-FileDoesNotContain -RelativePath "docs-machine-handoff-plan-2026-06-07.md" -ForbiddenMarkers @(
    'Current formal next development task after handoff: `G2 Add Android build smoke path`',
    'Last completed product commit: `1044ef1 Document reward authority contract`',
    'ahead 95',
    'Expected future commit:'
)

if ($RunReadiness) {
    $readinessScript = Resolve-RepoPath -RelativePath "scripts\unity\check_controlled_demo_readiness.ps1"
    $output = & powershell -NoProfile -ExecutionPolicy Bypass -File $readinessScript -RepoRoot $RepoRoot 2>&1
    $exitCode = $LASTEXITCODE
    $joined = (@($output | ForEach-Object { $_.ToString() })) -join [Environment]::NewLine
    if ($exitCode -ne 0 -or $joined -notlike "*Controlled demo readiness preflight OK.*") {
        Add-Failure "Readiness preflight failed during handoff consistency check."
        foreach ($line in @($output | ForEach-Object { $_.ToString() })) {
            Add-Failure "  $line"
        }
    }
    else {
        [void]$rows.Add([pscustomobject]@{
            Check = "readiness preflight"
            Status = "OK"
            Detail = "RunReadiness"
        })
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Controlled demo handoff consistency check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) controlled demo handoff consistency check(s) failed."
}

Write-Host "Controlled demo handoff consistency check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
