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

$standardPresets = @("mechlab", "spawn", "airfield", "hangar-contact", "damage-demo", "north-patrol")
$battlePresets = @("spawn", "airfield", "hangar-contact", "damage-demo", "north-patrol")
$standardCsv = $standardPresets -join ","
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

function Assert-ArrayLiteral {
    param(
        [string]$Text,
        [string]$VariableName,
        [string[]]$Expected,
        [string]$Label
    )

    $escapedName = [regex]::Escape($VariableName)
    $pattern = '\$' + $escapedName + '\s*=\s*@\(([^)]*)\)'
    $match = [regex]::Match($Text, $pattern)
    if (-not $match.Success) {
        Add-Failure "$Label missing array literal: `$$VariableName"
        return
    }

    $items = [regex]::Matches($match.Groups[1].Value, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
    $actual = @($items)
    $expectedText = $Expected -join ","
    $actualText = $actual -join ","
    if ($actualText -ne $expectedText) {
        Add-Failure "$Label preset mismatch: $actualText, expected $expectedText"
        return
    }

    Add-Row -Check $Label -Detail $actualText
}

function Assert-StandardCsv {
    param(
        [string]$Text,
        [string]$RelativePath
    )

    Assert-Contains -Text $Text -Needle $standardCsv -Label $RelativePath
    Add-Row -Check $RelativePath -Detail $standardCsv
}

$captureScript = Read-RequiredText -RelativePath "scripts\unity\capture_reference_visuals.ps1"
Assert-ArrayLiteral -Text $captureScript -VariableName "Presets" -Expected $standardPresets -Label "capture_reference_visuals.ps1 default presets"

$evidenceScript = Read-RequiredText -RelativePath "scripts\unity\check_controlled_demo_evidence.ps1"
Assert-ArrayLiteral -Text $evidenceScript -VariableName "capturePresets" -Expected $standardPresets -Label "check_controlled_demo_evidence.ps1 standard presets"

$visualSanityScript = Read-RequiredText -RelativePath "scripts\unity\check_pc_visual_capture_sanity.ps1"
Assert-ArrayLiteral -Text $visualSanityScript -VariableName "Presets" -Expected $standardPresets -Label "check_pc_visual_capture_sanity.ps1 default presets"

$sidecarSchemaScript = Read-RequiredText -RelativePath "scripts\unity\check_pc_capture_sidecar_schema.ps1"
Assert-ArrayLiteral -Text $sidecarSchemaScript -VariableName "expectedPresets" -Expected $standardPresets -Label "check_pc_capture_sidecar_schema.ps1 expected presets"
Assert-ArrayLiteral -Text $sidecarSchemaScript -VariableName "battlePresets" -Expected $battlePresets -Label "check_pc_capture_sidecar_schema.ps1 battle presets"

foreach ($relativePath in @(
    "README.md",
    "BUILD-WIN.md",
    "docs-pc-optimization-plan-2026-06-11.md"
)) {
    $text = Read-RequiredText -RelativePath $relativePath
    Assert-StandardCsv -Text $text -RelativePath $relativePath
}

if ($failures.Count -gt 0) {
    Write-Host "PC capture preset contract check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC capture preset contract check(s) failed."
}

Write-Host "PC capture preset contract check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "StandardPresets: $standardCsv"
$rows | Format-Table -AutoSize
