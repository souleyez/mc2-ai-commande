param(
    [string]$RepoRoot = "",
    [string]$ExpectedPackageName = "com.DefaultCompany.unitymc2demo",
    [string]$ExpectedActivityName = "com.unity3d.player.UnityPlayerGameActivity"
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

function Get-PlanValue {
    param(
        [string]$Text,
        [string]$Name
    )

    $pattern = "(?m)^" + [regex]::Escape($Name) + ":\s*(.+?)\s*$"
    $match = [regex]::Match($Text, $pattern)
    if (-not $match.Success) {
        return ""
    }

    return $match.Groups[1].Value.Trim()
}

$androidSmokeScript = Join-Path $PSScriptRoot "android_device_smoke.ps1"
$androidPreflightScript = Join-Path $PSScriptRoot "check_android_device_preflight.ps1"

$planResult = Invoke-ChildScript -ScriptPath $androidSmokeScript -Arguments @(
    "-RepoRoot",
    $RepoRoot,
    "-PlanOnly"
)

if ($planResult.ExitCode -ne 0) {
    Add-Failure "Android smoke plan failed with exit code $($planResult.ExitCode)."
    foreach ($line in $planResult.Lines) {
        Add-Failure "  $line"
    }
}

$preflightResult = Invoke-ChildScript -ScriptPath $androidPreflightScript -Arguments @(
    "-RepoRoot",
    $RepoRoot,
    "-AllowNoDevice"
)

if ($preflightResult.ExitCode -ne 0) {
    Add-Failure "Android device preflight failed with exit code $($preflightResult.ExitCode)."
    foreach ($line in $preflightResult.Lines) {
        Add-Failure "  $line"
    }
}

$planText = $planResult.Text
$preflightText = $preflightResult.Text

Assert-TextContains -Text $planText -Needle "Android device smoke plan OK." -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "Package: $ExpectedPackageName" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "Activity: $ExpectedActivityName" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "analysis-output\android-device-smoke.log" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "analysis-output\android-device-smoke.png" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "analysis-output\android-device-smoke-summary.json" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "CommandFileSmoke: True" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "mc2_01-visible-flow-audit.txt" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "UnityArguments: -mc2CommandFile" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "SmokeSuccessMarker: MC2 debrief summary assertion OK" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "SmokeSuccessMarker: MC2 loadout compact assertion OK" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "Install: True" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "Launch: True" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "LogCheck: True" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "ScreenshotCapture: True" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "LandscapeScreenshot: True" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "SummaryWrite: True" -Label "android_device_smoke.ps1 -PlanOnly"
Assert-TextContains -Text $planText -Needle "ConnectionCheck: check_android_device_connection.ps1 -RequireDevice" -Label "android_device_smoke.ps1 -PlanOnly"

$planPackageName = Get-PlanValue -Text $planText -Name "Package"
$planActivityName = Get-PlanValue -Text $planText -Name "Activity"
if ($planPackageName -ne $ExpectedPackageName) {
    Add-Failure "Plan package mismatch: expected $ExpectedPackageName, got $planPackageName"
}

if ($planActivityName -ne $ExpectedActivityName) {
    Add-Failure "Plan activity mismatch: expected $ExpectedActivityName, got $planActivityName"
}

Assert-TextContains -Text $preflightText -Needle $ExpectedPackageName -Label "check_android_device_preflight.ps1 -AllowNoDevice"
Assert-TextContains -Text $preflightText -Needle $ExpectedActivityName -Label "check_android_device_preflight.ps1 -AllowNoDevice"
Assert-TextContains -Text $preflightText -Needle "smoke artifact hygiene" -Label "check_android_device_preflight.ps1 -AllowNoDevice"
Assert-TextContains -Text $preflightText -Needle "smoke summary schema" -Label "check_android_device_preflight.ps1 -AllowNoDevice"
Assert-TextContains -Text $preflightText -Needle "Android smoke summary check self-test OK." -Label "check_android_device_preflight.ps1 -AllowNoDevice"
Assert-TextContains -Text $preflightText -Needle "APK freshness" -Label "check_android_device_preflight.ps1 -AllowNoDevice"
Assert-TextContains -Text $preflightText -Needle "APK identity" -Label "check_android_device_preflight.ps1 -AllowNoDevice"
Assert-TextContains -Text $preflightText -Needle "APK compatibility" -Label "check_android_device_preflight.ps1 -AllowNoDevice"
Assert-TextContains -Text $preflightText -Needle "APK signing" -Label "check_android_device_preflight.ps1 -AllowNoDevice"
Assert-TextContains -Text $preflightText -Needle "APK manifest" -Label "check_android_device_preflight.ps1 -AllowNoDevice"
Assert-TextContains -Text $preflightText -Needle "APK payload" -Label "check_android_device_preflight.ps1 -AllowNoDevice"
Assert-TextContains -Text $preflightText -Needle "APK size budget" -Label "check_android_device_preflight.ps1 -AllowNoDevice"
Assert-TextContains -Text $preflightText -Needle "device connection" -Label "check_android_device_preflight.ps1 -AllowNoDevice"

$preflightReadiness = ""
$deviceConnectionDetail = ""
if ($preflightText -like "*Android device connection check OK.*") {
    $preflightReadiness = "OK"
    $deviceConnectionDetail = "Android device connection check OK."
}
elseif ($preflightText -like "*Android device connection check waiting on device.*") {
    $preflightReadiness = "WAITING"
    $deviceConnectionDetail = "Android device connection check waiting on device."
}
elseif ($preflightText -like "*Android device connection check waiting on authorization.*") {
    $preflightReadiness = "WAITING"
    $deviceConnectionDetail = "Android device connection check waiting on authorization."
}
elseif ($preflightText -like "*Android device connection check waiting on online device.*") {
    $preflightReadiness = "WAITING"
    $deviceConnectionDetail = "Android device connection check waiting on online device."
}
elseif ($preflightText -like "*Android device connection check waiting on device selection.*") {
    $preflightReadiness = "WAITING"
    $deviceConnectionDetail = "Android device connection check waiting on device selection."
}
elseif ($preflightText -like "*Android device smoke preflight OK.*") {
    $preflightReadiness = "OK"
    $deviceConnectionDetail = "Android device smoke preflight OK."
}
else {
    Add-Failure "Preflight output did not expose a recognized readiness marker."
}

Add-Row -Check "plan package" -Status "OK" -Detail $planPackageName
Add-Row -Check "plan activity" -Status "OK" -Detail $planActivityName
Add-Row -Check "plan evidence outputs" -Status "OK" -Detail "log+screenshot+summary"
Add-Row -Check "plan command-file smoke" -Status "OK" -Detail "mc2_01-visible-flow-audit.txt"
Add-Row -Check "plan execution flags" -Status "OK" -Detail "install+launch+log+screenshot+summary+connection+command-file"
Add-Row -Check "preflight package/activity" -Status "OK" -Detail "$ExpectedPackageName / $ExpectedActivityName"
Add-Row -Check "preflight readiness" -Status $preflightReadiness -Detail "AllowNoDevice accepted"
Add-Row -Check "preflight device connection" -Status $preflightReadiness -Detail $deviceConnectionDetail
Add-Row -Check "summary schema" -Status "OK" -Detail "Android smoke summary check self-test OK."

if ($failures.Count -gt 0) {
    Write-Host "Android smoke plan/preflight consistency check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android smoke plan/preflight consistency check(s) failed."
}

Write-Host "Android smoke plan/preflight consistency check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
