param(
    [string]$RepoRoot = "",
    [string]$BudgetDoc = "",
    [string]$SummaryPath = "",
    [int]$MinAverageFps = 25,
    [int64]$MaxTotalPssKb = 1572864,
    [int64]$MaxApkBytes = 524288000,
    [switch]$SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ([string]::IsNullOrWhiteSpace($BudgetDoc)) {
    $BudgetDoc = Join-Path $RepoRoot "docs-mobile-performance-budget-2026-06-10.md"
}

if ([string]::IsNullOrWhiteSpace($SummaryPath)) {
    $SummaryPath = Join-Path $RepoRoot "analysis-output\android-performance-baseline.json"
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
        [string]$Status,
        [string]$Detail
    )

    [void]$rows.Add([pscustomobject]@{
        Check = $Check
        Status = $Status
        Detail = $Detail
    })
}

function Assert-TextContains {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or $Text -notlike "*$Needle*") {
        Add-Failure "$Label missing marker: $Needle"
    }
    else {
        Add-Row -Check $Label -Status "OK" -Detail $Needle
    }
}

if ($SelfTest) {
    $sample = [pscustomobject]@{
        result = "completed"
        model = "SelfTest Phone"
        androidVersion = "15"
        apkBytes = 20971520
        averageFps = 30.1
        totalPssKb = 300000
        launchTotalMs = 1200
        battleBaselineObservedWithinSeconds = 14
        thermalStatus = 0
        batteryTemperatureC = 34.2
        screen = "2400x1080"
        targetFrameRate = 30
        performanceMarker = "MC2 mobile performance baseline: frames=300 seconds=10 avgFps=30.1"
    }

    if ($sample.averageFps -lt $MinAverageFps) {
        throw "Mobile performance budget self-test failed: average FPS."
    }

    if ($sample.totalPssKb -gt $MaxTotalPssKb) {
        throw "Mobile performance budget self-test failed: total PSS."
    }

    Write-Host "Mobile performance budget check self-test OK."
    return
}

if (-not (Test-Path -LiteralPath $BudgetDoc -PathType Leaf)) {
    Add-Failure "Budget doc missing: $BudgetDoc"
}
else {
    $docText = Get-Content -LiteralPath $BudgetDoc -Raw
    Assert-TextContains -Text $docText -Needle "G5 Mobile Performance Budget" -Label "budget doc title"
    Assert-TextContains -Text $docText -Needle "Mi 11 Lite" -Label "budget doc device"
    Assert-TextContains -Text $docText -Needle "FPS" -Label "budget doc FPS"
    Assert-TextContains -Text $docText -Needle "Memory" -Label "budget doc memory"
    Assert-TextContains -Text $docText -Needle "Launch" -Label "budget doc launch"
    Assert-TextContains -Text $docText -Needle "Thermal" -Label "budget doc thermal"
    Assert-TextContains -Text $docText -Needle "Battery" -Label "budget doc battery"
    Assert-TextContains -Text $docText -Needle "analysis-output\android-performance-baseline.json" -Label "budget doc evidence"
}

if (-not (Test-Path -LiteralPath $SummaryPath -PathType Leaf)) {
    Add-Failure "Performance summary missing: $SummaryPath"
}
else {
    try {
        $summary = Get-Content -LiteralPath $SummaryPath -Raw | ConvertFrom-Json
    }
    catch {
        Add-Failure "Performance summary is not valid JSON: $SummaryPath"
        $summary = $null
    }

    if ($null -ne $summary) {
        if ($summary.result -ne "completed") {
            Add-Failure "Performance summary result is not completed: $($summary.result)"
        }
        else {
            Add-Row -Check "summary result" -Status "OK" -Detail $summary.result
        }

        if ([double]$summary.averageFps -lt $MinAverageFps) {
            Add-Failure "Average FPS below budget: $($summary.averageFps) < $MinAverageFps"
        }
        else {
            Add-Row -Check "average FPS" -Status "OK" -Detail "$($summary.averageFps) >= $MinAverageFps"
        }

        if ([int64]$summary.totalPssKb -gt $MaxTotalPssKb) {
            Add-Failure "Total PSS above budget: $($summary.totalPssKb) KB > $MaxTotalPssKb KB"
        }
        else {
            Add-Row -Check "memory" -Status "OK" -Detail "$($summary.totalPssKb) KB"
        }

        if ([int64]$summary.apkBytes -gt $MaxApkBytes) {
            Add-Failure "APK above G5 budget: $($summary.apkBytes) bytes > $MaxApkBytes bytes"
        }
        else {
            Add-Row -Check "APK size" -Status "OK" -Detail "$($summary.apkBytes) bytes"
        }

        if ([int]$summary.thermalStatus -gt 1) {
            Add-Failure "Thermal status above budget: $($summary.thermalStatus)"
        }
        else {
            Add-Row -Check "thermal" -Status "OK" -Detail "status=$($summary.thermalStatus)"
        }

        if ([string]$summary.screen -notmatch "^\d+x\d+$") {
            Add-Failure "Screen summary is missing or malformed: $($summary.screen)"
        }
        else {
            Add-Row -Check "screen" -Status "OK" -Detail $summary.screen
        }

        if ([int]$summary.targetFrameRate -ne 30) {
            Add-Failure "Mobile target frame rate should be 30 for G5 baseline: $($summary.targetFrameRate)"
        }
        else {
            Add-Row -Check "target frame rate" -Status "OK" -Detail "30"
        }

        if ([string]::IsNullOrWhiteSpace([string]$summary.performanceMarker)) {
            Add-Failure "Performance summary missing Unity marker."
        }
        else {
            Add-Row -Check "Unity performance marker" -Status "OK" -Detail "present"
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Mobile performance budget check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) mobile performance budget check(s) failed."
}

Write-Host "Mobile performance budget check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "BudgetDoc: $BudgetDoc"
Write-Host "Summary: $SummaryPath"
$rows | Format-Table -AutoSize
