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

$expectedWidth = 1280
$expectedHeight = 720
$expectedResolution = "${expectedWidth}x${expectedHeight}"
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
    return Join-Path $RepoRoot $RelativePath
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
    }
}

function Assert-IntParamDefault {
    param(
        [string]$Text,
        [string]$VariableName,
        [int]$Expected,
        [string]$Label
    )

    $escapedName = [regex]::Escape($VariableName)
    $pattern = '\[int\]\$' + $escapedName + '\s*=\s*([0-9]+)'
    $match = [regex]::Match($Text, $pattern)
    if (-not $match.Success) {
        Add-Failure "$Label missing int param default: `$$VariableName"
        return
    }

    $actual = [int]$match.Groups[1].Value
    if ($actual -ne $Expected) {
        Add-Failure "$Label default `$$VariableName is $actual, expected $Expected"
        return
    }

    Add-Row -Check "$Label `$$VariableName" -Detail $actual.ToString([Globalization.CultureInfo]::InvariantCulture)
}

function Assert-WindowArgs {
    param(
        [string]$Text,
        [string]$Label
    )

    foreach ($marker in @(
        "-screen-width",
        "-screen-height",
        "-screen-fullscreen"
    )) {
        Assert-Contains -Text $Text -Needle $marker -Label $Label
    }

    if ($Text -notmatch '"0"') {
        Add-Failure "$Label missing windowed fullscreen value 0"
        return
    }

    Add-Row -Check "$Label window args" -Detail "-screen-width/-screen-height/-screen-fullscreen 0"
}

$runScript = Read-RequiredText -RelativePath "scripts\unity\run_windows_demo.ps1"
Assert-IntParamDefault -Text $runScript -VariableName "Width" -Expected $expectedWidth -Label "run_windows_demo.ps1"
Assert-IntParamDefault -Text $runScript -VariableName "Height" -Expected $expectedHeight -Label "run_windows_demo.ps1"
Assert-WindowArgs -Text $runScript -Label "run_windows_demo.ps1"

$captureScript = Read-RequiredText -RelativePath "scripts\unity\capture_reference_visuals.ps1"
Assert-IntParamDefault -Text $captureScript -VariableName "Width" -Expected $expectedWidth -Label "capture_reference_visuals.ps1"
Assert-IntParamDefault -Text $captureScript -VariableName "Height" -Expected $expectedHeight -Label "capture_reference_visuals.ps1"
Assert-WindowArgs -Text $captureScript -Label "capture_reference_visuals.ps1"

foreach ($relativePath in @(
    "README.md",
    "BUILD-WIN.md",
    "docs-pc-optimization-plan-2026-06-11.md"
)) {
    $text = Read-RequiredText -RelativePath $relativePath
    Assert-Contains -Text $text -Needle $expectedResolution -Label $relativePath
    Add-Row -Check "$relativePath controlled resolution" -Detail $expectedResolution
}

$launchScript = Resolve-RepoPath -RelativePath "scripts\unity\run_windows_demo.ps1"
$output = & powershell -NoProfile -ExecutionPolicy Bypass -File $launchScript -RepoRoot $RepoRoot -CheckOnly 2>&1
$exitCode = $LASTEXITCODE
$joined = (@($output | ForEach-Object { $_.ToString() })) -join [Environment]::NewLine
if ($exitCode -ne 0 -or $joined -notlike "*Windows demo launch preflight OK.*") {
    Add-Failure "run_windows_demo.ps1 -CheckOnly failed during window contract check."
    foreach ($line in @($output | ForEach-Object { $_.ToString() })) {
        Add-Failure "  $line"
    }
}
else {
    foreach ($marker in @(
        "-screen-width $expectedWidth",
        "-screen-height $expectedHeight",
        "-screen-fullscreen 0"
    )) {
        if ($joined -notlike "*$marker*") {
            Add-Failure "run_windows_demo.ps1 -CheckOnly output missing marker: $marker"
        }
    }

    Add-Row -Check "run_windows_demo.ps1 -CheckOnly" -Detail $expectedResolution
}

if ($failures.Count -gt 0) {
    Write-Host "PC window contract check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC window contract check(s) failed."
}

Write-Host "PC window contract check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "Resolution: $expectedResolution"
$rows | Format-Table -AutoSize
