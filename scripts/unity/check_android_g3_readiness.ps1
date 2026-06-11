param(
    [string]$RepoRoot = "",
    [switch]$RequireDevice
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
$waitingOnDevice = $false

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

function Invoke-ChildScript {
    param(
        [string]$ScriptPath,
        [string[]]$Arguments
    )

    if (-not (Test-Path -LiteralPath $ScriptPath)) {
        return [pscustomobject]@{
            ExitCode = 1
            Lines = @("Missing script: $ScriptPath")
            Text = "Missing script: $ScriptPath"
        }
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

    return [pscustomobject]@{
        ExitCode = $exitCode
        Lines = $lines
        Text = ($lines -join [Environment]::NewLine)
    }
}

function Assert-TextContains {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ($Text -notlike "*$Needle*") {
        Add-Failure "$Label missing marker: $Needle"
    }
}

function Add-ChildFailures {
    param(
        [string]$Label,
        [object]$Result
    )

    Add-Failure "$Label failed with exit code $($Result.ExitCode)."
    foreach ($line in $Result.Lines) {
        Add-Failure "  $line"
    }
}

$androidPreflightScript = Join-Path $PSScriptRoot "check_android_device_preflight.ps1"
$androidPlanConsistencyScript = Join-Path $PSScriptRoot "check_android_smoke_plan_consistency.ps1"
$androidSmokeScript = Join-Path $PSScriptRoot "android_device_smoke.ps1"
$androidLogScript = Join-Path $PSScriptRoot "check_android_smoke_log.ps1"
$androidSummaryScript = Join-Path $PSScriptRoot "check_android_smoke_summary.ps1"

$preflightArgs = @("-RepoRoot", $RepoRoot)
if (-not $RequireDevice) {
    $preflightArgs += "-AllowNoDevice"
}

$preflightResult = Invoke-ChildScript -ScriptPath $androidPreflightScript -Arguments $preflightArgs
if ($preflightResult.ExitCode -ne 0) {
    Add-ChildFailures -Label "Android device preflight" -Result $preflightResult
}

Assert-TextContains -Text $preflightResult.Text -Needle "smoke summary schema" -Label "Android device preflight"
Assert-TextContains -Text $preflightResult.Text -Needle "Android smoke summary check self-test OK." -Label "Android device preflight"

if ($preflightResult.Text -like "*Android device smoke preflight waiting on device.*") {
    $waitingOnDevice = $true
    Add-Row -Check "device preflight" -Status "WAITING" -Detail "authorized Android phone not connected"
}
elseif ($preflightResult.Text -like "*Android device smoke preflight OK.*") {
    Add-Row -Check "device preflight" -Status "READY" -Detail "authorized Android phone selected"
}
else {
    Add-Failure "Android device preflight did not report OK or waiting-on-device."
}

$planConsistencyResult = Invoke-ChildScript -ScriptPath $androidPlanConsistencyScript -Arguments @("-RepoRoot", $RepoRoot)
if ($planConsistencyResult.ExitCode -ne 0) {
    Add-ChildFailures -Label "Android smoke plan/preflight consistency" -Result $planConsistencyResult
}

Assert-TextContains -Text $planConsistencyResult.Text -Needle "Android smoke plan/preflight consistency check OK." -Label "Android smoke plan/preflight consistency"
Add-Row -Check "plan/preflight consistency" -Status "OK" -Detail "package+activity+evidence paths"

$planResult = Invoke-ChildScript -ScriptPath $androidSmokeScript -Arguments @(
    "-RepoRoot",
    $RepoRoot,
    "-PlanOnly"
)
if ($planResult.ExitCode -ne 0) {
    Add-ChildFailures -Label "Android smoke plan" -Result $planResult
}

Assert-TextContains -Text $planResult.Text -Needle "Android device smoke plan OK." -Label "Android smoke plan"
Assert-TextContains -Text $planResult.Text -Needle "Package: com.DefaultCompany.unitymc2demo" -Label "Android smoke plan"
Assert-TextContains -Text $planResult.Text -Needle "Activity: com.unity3d.player.UnityPlayerGameActivity" -Label "Android smoke plan"
Assert-TextContains -Text $planResult.Text -Needle "ScreenshotCapture: True" -Label "Android smoke plan"
Assert-TextContains -Text $planResult.Text -Needle "SummaryWrite: True" -Label "Android smoke plan"
Add-Row -Check "smoke plan" -Status "OK" -Detail "install+launch+log+screenshot+summary"

$logResult = Invoke-ChildScript -ScriptPath $androidLogScript -Arguments @("-SelfTest")
if ($logResult.ExitCode -ne 0) {
    Add-ChildFailures -Label "Android smoke log scanner" -Result $logResult
}

Assert-TextContains -Text $logResult.Text -Needle "Android smoke log check self-test OK." -Label "Android smoke log scanner"
Add-Row -Check "log scanner" -Status "OK" -Detail "self-test"

$summaryResult = Invoke-ChildScript -ScriptPath $androidSummaryScript -Arguments @("-SelfTest")
if ($summaryResult.ExitCode -ne 0) {
    Add-ChildFailures -Label "Android smoke summary schema" -Result $summaryResult
}

Assert-TextContains -Text $summaryResult.Text -Needle "Android smoke summary check self-test OK." -Label "Android smoke summary schema"
Add-Row -Check "summary schema" -Status "OK" -Detail "self-test"

if ($RequireDevice -and $waitingOnDevice) {
    Add-Failure "Android G3 readiness requires a connected and authorized Android phone."
}

if ($failures.Count -gt 0) {
    Write-Host "Android G3 readiness check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android G3 readiness check(s) failed."
}

Write-Host "Android smoke plan/preflight consistency check OK."
Write-Host "Android device smoke plan OK."
Write-Host "Android smoke log check self-test OK."
Write-Host "Android smoke summary check self-test OK."

if ($waitingOnDevice) {
    Write-Host "Android G3 readiness check waiting on device."
}
else {
    Write-Host "Android G3 readiness check OK."
}

Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
