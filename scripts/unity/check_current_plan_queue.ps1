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

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
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

function Assert-DoesNotContain {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if (-not [string]::IsNullOrWhiteSpace($Text) -and $Text.Contains($Needle)) {
        Add-Failure "$Label still contains stale marker: $Needle"
        return
    }

    Add-Row -Check "$Label stale marker" -Detail $Needle
}

$requiredPlanMarkers = @(
    "PC1-PC57",
    "Add Android ADB driver package probe",
    "WpdOnlyAndroidProbe: True",
    "AdbSetupHint: True",
    "AdbDriverPackageProbe: True",
    "AdbWatchHint: True",
    "G3DeviceStatusReport: True",
    "G3WhenReady: True",
    "CommandFileSmoke: True",
    "SmokeSuccessMarker: MC2 debrief summary assertion OK",
    "SmokeSuccessMarker: MC2 loadout compact assertion OK",
    "Pass Android G3 device smoke",
    "G4 Touch UI pass"
)

$docsToCheck = @(
    "README.md",
    "BUILD-WIN.md",
    "docs-ai-rts-commander-current-master-plan-2026-06-07.md",
    "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md",
    "docs-pc-optimization-plan-2026-06-11.md",
    "docs-mobile-first-plan-2026-06-10.md",
    "docs-machine-handoff-plan-2026-06-07.md",
    "docs-playable-demo-investor-evidence-2026-06-07.md"
)

foreach ($relativePath in $docsToCheck) {
    $text = Read-RequiredText -RelativePath $relativePath
    foreach ($marker in $requiredPlanMarkers) {
        Assert-Contains -Text $text -Needle $marker -Label "$relativePath plan queue marker"
    }

    Assert-DoesNotContain -Text $text -Needle "define PC55 before implementation" -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle "必须先定义 PC55" -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle "必须先写清 PC55" -Label $relativePath
}

$mobilePlan = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
Assert-Contains -Text $mobilePlan -Needle "| G3 | Done | Android device smoke |" -Label "mobile gate order"
Assert-Contains -Text $mobilePlan -Needle "| G4 | Done | Touch UI pass |" -Label "mobile gate order"
Assert-Contains -Text $mobilePlan -Needle "| G5 | Next | Mobile performance budget |" -Label "mobile gate order"

$handoff = Read-RequiredText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
Assert-Contains -Text $handoff -Needle 'Current formal next development task after handoff: `G5 Mobile performance budget`' -Label "handoff next task"
Assert-Contains -Text $handoff -Needle 'Next planned work: `G5 Mobile performance budget`' -Label "handoff next planned work"

$currentGate = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
Assert-Contains -Text $currentGate -Needle 'CommandFileSmoke: True' -Label "current gate script marker"
Assert-Contains -Text $currentGate -Needle 'WpdOnlyAndroidProbe: True' -Label "current gate device diagnosis marker"
Assert-Contains -Text $currentGate -Needle 'AdbSetupHint: True' -Label "current gate adb setup marker"
Assert-Contains -Text $currentGate -Needle 'AdbDriverPackageProbe: True' -Label "current gate adb driver package marker"
Assert-Contains -Text $currentGate -Needle 'AdbWatchHint: True' -Label "current gate adb watch marker"
Assert-Contains -Text $currentGate -Needle 'G3DeviceStatusReport: True' -Label "current gate g3 device status marker"
Assert-Contains -Text $currentGate -Needle 'G3WhenReady: True' -Label "current gate g3 when-ready marker"
Assert-Contains -Text $currentGate -Needle 'SmokeSuccessMarker: MC2 loadout compact assertion OK' -Label "current gate success marker"
Assert-Contains -Text $currentGate -Needle 'LandscapeScreenshot: True' -Label "current gate landscape screenshot marker"

$handoffScript = Read-RequiredText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
Assert-Contains -Text $handoffScript -Needle 'CommandFileSmoke: True' -Label "handoff script marker"
Assert-Contains -Text $handoffScript -Needle 'WpdOnlyAndroidProbe: True' -Label "handoff script device diagnosis marker"
Assert-Contains -Text $handoffScript -Needle 'AdbSetupHint: True' -Label "handoff script adb setup marker"
Assert-Contains -Text $handoffScript -Needle 'AdbDriverPackageProbe: True' -Label "handoff script adb driver package marker"
Assert-Contains -Text $handoffScript -Needle 'AdbWatchHint: True' -Label "handoff script adb watch marker"
Assert-Contains -Text $handoffScript -Needle 'G3DeviceStatusReport: True' -Label "handoff script g3 device status marker"
Assert-Contains -Text $handoffScript -Needle 'G3WhenReady: True' -Label "handoff script g3 when-ready marker"
Assert-Contains -Text $handoffScript -Needle 'SmokeSuccessMarker: MC2 loadout compact assertion OK' -Label "handoff script success marker"

$tracked = @(& git -C $RepoRoot ls-files 2>$null | ForEach-Object { $_.ToString() })
if ($LASTEXITCODE -ne 0) {
    throw "git ls-files failed with exit code $LASTEXITCODE"
}

$staged = @(& git -C $RepoRoot diff --cached --name-only --diff-filter=ACMR 2>$null | ForEach-Object { $_.ToString() })
if ($LASTEXITCODE -ne 0) {
    throw "git diff --cached --name-only failed with exit code $LASTEXITCODE"
}

$sourcePaths = @($tracked + $staged)
$unexpectedPlanCopies = @($sourcePaths | Where-Object {
    $_ -match '(^|/)docs/' -or $_ -match 'current-plan-copy' -or $_ -match 'plan-backup'
})
if ($unexpectedPlanCopies.Count -gt 0) {
    foreach ($path in $unexpectedPlanCopies) {
        Add-Failure "Unexpected plan copy/source path: $path"
    }
}
else {
    Add-Row -Check "tracked/staged plan path shape" -Detail "$($sourcePaths.Count) path(s)"
}

if ($failures.Count -gt 0) {
    Write-Host "Current plan queue consistency check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) current plan queue consistency check(s) failed."
}

Write-Host "Current plan queue consistency check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
