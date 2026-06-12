param(
    [string]$RepoRoot = "",
    [string]$AdbPath = "",
    [string]$DeviceId = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

$androidPlayer = Join-Path $HOME "Unity\Hub\Editor\6000.4.7f1\Editor\Data\PlaybackEngines\AndroidPlayer"
if ([string]::IsNullOrWhiteSpace($AdbPath)) {
    $AdbPath = Join-Path $androidPlayer "SDK\platform-tools\adb.exe"
}

$connectionScript = Join-Path $PSScriptRoot "check_android_device_connection.ps1"
$smokeScript = Join-Path $PSScriptRoot "android_device_smoke.ps1"
$expectedFailure = "Android device smoke requires a single authorized Android device before install or launch."

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

    if (-not (Test-Path -LiteralPath $ScriptPath -PathType Leaf)) {
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

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $output = & powershell @processArgs 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    $lines = @($output | ForEach-Object { $_.ToString() })
    return [pscustomobject]@{
        ExitCode = $exitCode
        Lines = $lines
        Text = ($lines -join [Environment]::NewLine)
    }
}

function Get-FileSnapshot {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return [pscustomobject]@{
            Exists = $false
            Length = 0
            LastWriteTimeUtc = [DateTime]::MinValue
        }
    }

    $item = Get-Item -LiteralPath $Path
    return [pscustomobject]@{
        Exists = $true
        Length = $item.Length
        LastWriteTimeUtc = $item.LastWriteTimeUtc
    }
}

function Assert-SnapshotUnchanged {
    param(
        [string]$Path,
        [object]$Before
    )

    $after = Get-FileSnapshot -Path $Path
    if ($Before.Exists -ne $after.Exists -or $Before.Length -ne $after.Length -or $Before.LastWriteTimeUtc -ne $after.LastWriteTimeUtc) {
        Add-Failure "Android smoke connection gate changed evidence output before a valid device was selected: $Path"
        return
    }

    Add-Row -Check "artifact unchanged" -Status "OK" -Detail $Path
}

if (-not (Test-Path -LiteralPath $connectionScript -PathType Leaf)) {
    Add-Failure "Missing Android device connection checker: $connectionScript"
}

if (-not (Test-Path -LiteralPath $smokeScript -PathType Leaf)) {
    Add-Failure "Missing Android device smoke helper: $smokeScript"
}

if ($failures.Count -eq 0) {
    $connectionArgs = @("-RepoRoot", $RepoRoot, "-AdbPath", $AdbPath)
    if (-not [string]::IsNullOrWhiteSpace($DeviceId)) {
        $connectionArgs += @("-DeviceId", $DeviceId)
    }

    $connectionResult = Invoke-ChildScript -ScriptPath $connectionScript -Arguments $connectionArgs
    if ($connectionResult.ExitCode -ne 0) {
        Add-Failure "Android device connection check failed unexpectedly."
        foreach ($line in $connectionResult.Lines) {
            Add-Failure "  $line"
        }
    }
    elseif ($connectionResult.Text -like "*Android device connection check OK.*") {
        Add-Row -Check "connection" -Status "READY" -Detail "authorized Android phone is available"
        Write-Host "Android smoke connection gate check OK."
        Write-Host "Android smoke connection gate check ready for G3 device smoke."
        Write-Host "Repo: $RepoRoot"
        $rows | Format-Table -AutoSize
        return
    }
    else {
        $waitingMarker = ""
        if ($connectionResult.Text -like "*Android device connection check waiting on device.*") {
            $waitingMarker = "Android smoke connection gate check waiting on device."
        }
        elseif ($connectionResult.Text -like "*Android device connection check waiting on authorization.*") {
            $waitingMarker = "Android smoke connection gate check waiting on authorization."
        }
        elseif ($connectionResult.Text -like "*Android device connection check waiting on online device.*") {
            $waitingMarker = "Android smoke connection gate check waiting on online device."
        }
        elseif ($connectionResult.Text -like "*Android device connection check waiting on device selection.*") {
            $waitingMarker = "Android smoke connection gate check waiting on device selection."
        }
        else {
            Add-Failure "Android device connection check did not report OK or a known waiting state."
            foreach ($line in $connectionResult.Lines) {
                Add-Failure "  $line"
            }
        }

        if ($failures.Count -eq 0) {
            Add-Row -Check "connection" -Status "WAITING" -Detail $waitingMarker

            $logPath = Join-Path $RepoRoot "analysis-output\android-device-smoke.log"
            $screenshotPath = Join-Path $RepoRoot "analysis-output\android-device-smoke.png"
            $summaryPath = Join-Path $RepoRoot "analysis-output\android-device-smoke-summary.json"
            $beforeLog = Get-FileSnapshot -Path $logPath
            $beforeScreenshot = Get-FileSnapshot -Path $screenshotPath
            $beforeSummary = Get-FileSnapshot -Path $summaryPath

            $smokeArgs = @("-RepoRoot", $RepoRoot, "-AdbPath", $AdbPath)
            if (-not [string]::IsNullOrWhiteSpace($DeviceId)) {
                $smokeArgs += @("-DeviceId", $DeviceId)
            }

            $smokeResult = Invoke-ChildScript -ScriptPath $smokeScript -Arguments $smokeArgs
            if ($smokeResult.ExitCode -eq 0) {
                Add-Failure "Android device smoke unexpectedly passed before a valid device was selected."
            }

            if ($smokeResult.Text -notlike "*$expectedFailure*") {
                Add-Failure "Android device smoke did not report the strict connection failure marker."
                foreach ($line in $smokeResult.Lines) {
                    Add-Failure "  $line"
                }
            }
            else {
                Add-Row -Check "strict smoke failure" -Status "OK" -Detail $expectedFailure
            }

            Assert-SnapshotUnchanged -Path $logPath -Before $beforeLog
            Assert-SnapshotUnchanged -Path $screenshotPath -Before $beforeScreenshot
            Assert-SnapshotUnchanged -Path $summaryPath -Before $beforeSummary

            if ($failures.Count -eq 0) {
                Write-Host "Android smoke connection gate check OK."
                Write-Host $waitingMarker
            }
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Android smoke connection gate check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android smoke connection gate check(s) failed."
}

Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
