param(
    [string]$RepoRoot = "",
    [string]$AdbPath = "",
    [string]$DeviceId = "",
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

$androidPlayer = Join-Path $HOME "Unity\Hub\Editor\6000.4.7f1\Editor\Data\PlaybackEngines\AndroidPlayer"
if ([string]::IsNullOrWhiteSpace($AdbPath)) {
    $AdbPath = Join-Path $androidPlayer "SDK\platform-tools\adb.exe"
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

function Invoke-NativeCommand {
    param(
        [string]$FilePath,
        [string[]]$Arguments
    )

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $output = & $FilePath @Arguments 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    return [pscustomobject]@{
        ExitCode = $exitCode
        Output = @($output | ForEach-Object { $_.ToString() })
    }
}

function Get-AdbDeviceRows {
    param([string]$Adb)

    $deviceResult = Invoke-NativeCommand -FilePath $Adb -Arguments @("devices", "-l")
    if ($deviceResult.ExitCode -ne 0) {
        Add-Failure "adb devices -l failed: $($deviceResult.Output -join ' ')"
        return @()
    }

    $devices = @()
    foreach ($line in $deviceResult.Output) {
        if ($line -match "^(\S+)\s+(device|unauthorized|offline)(?:\s+(.*))?$") {
            $detail = ""
            if ($Matches.Count -gt 3) {
                $detail = $Matches[3]
            }

            $devices += [pscustomobject]@{
                Id = $Matches[1]
                State = $Matches[2]
                Detail = $detail
            }
        }
    }

    return $devices
}

if (-not (Test-Path -LiteralPath $AdbPath -PathType Leaf)) {
    Add-Failure "Missing adb: $AdbPath"
}
else {
    $versionResult = Invoke-NativeCommand -FilePath $AdbPath -Arguments @("version")
    if ($versionResult.ExitCode -ne 0) {
        Add-Failure "adb version failed: $($versionResult.Output -join ' ')"
    }
    else {
        $version = ($versionResult.Output | Select-Object -First 2) -join "; "
        Add-Row -Check "adb" -Status "OK" -Detail $version
    }
}

$devices = @()
if ($failures.Count -eq 0) {
    $devices = @(Get-AdbDeviceRows -Adb $AdbPath)
}

$summary = "none"
if ($devices.Count -gt 0) {
    $summary = ($devices | ForEach-Object {
        if ([string]::IsNullOrWhiteSpace($_.Detail)) {
            "$($_.Id):$($_.State)"
        }
        else {
            "$($_.Id):$($_.State):$($_.Detail)"
        }
    }) -join ", "
}

if ($devices.Count -eq 0) {
    Add-Row -Check "adb devices" -Status "WAITING" -Detail "no device rows"
    if ($RequireDevice) {
        Add-Failure "Android device connection requires one authorized device, but adb returned no device rows."
    }
}
else {
    Add-Row -Check "adb devices" -Status "OK" -Detail $summary

    if (-not [string]::IsNullOrWhiteSpace($DeviceId)) {
        $selected = $devices | Where-Object { $_.Id -eq $DeviceId } | Select-Object -First 1
        if ($null -eq $selected) {
            Add-Failure "Requested Android device was not found: $DeviceId"
        }
        elseif ($selected.State -ne "device") {
            Add-Row -Check "selected device" -Status "WAITING" -Detail "$($selected.Id):$($selected.State)"
            if ($RequireDevice) {
                Add-Failure "Requested Android device is not authorized and online: $($selected.Id):$($selected.State)"
            }
        }
        else {
            Add-Row -Check "selected device" -Status "OK" -Detail $selected.Id
        }
    }
    else {
        $readyDevices = @($devices | Where-Object { $_.State -eq "device" })
        $unauthorizedDevices = @($devices | Where-Object { $_.State -eq "unauthorized" })
        $offlineDevices = @($devices | Where-Object { $_.State -eq "offline" })

        if ($readyDevices.Count -eq 1) {
            Add-Row -Check "selected device" -Status "OK" -Detail $readyDevices[0].Id
        }
        elseif ($readyDevices.Count -gt 1) {
            Add-Row -Check "selected device" -Status "WAITING" -Detail "multiple authorized devices; pass -DeviceId"
            if ($RequireDevice) {
                Add-Failure "Multiple authorized Android devices found. Pass -DeviceId. Devices: $(($readyDevices | ForEach-Object { $_.Id }) -join ', ')"
            }
        }
        elseif ($unauthorizedDevices.Count -gt 0) {
            Add-Row -Check "selected device" -Status "WAITING" -Detail "authorize USB debugging on the phone"
            if ($RequireDevice) {
                Add-Failure "Android device is connected but unauthorized. Confirm the USB debugging prompt on the phone."
            }
        }
        elseif ($offlineDevices.Count -gt 0) {
            Add-Row -Check "selected device" -Status "WAITING" -Detail "device offline; reconnect USB or restart adb"
            if ($RequireDevice) {
                Add-Failure "Android device is offline. Reconnect USB or restart adb before G3 smoke."
            }
        }
        else {
            Add-Row -Check "selected device" -Status "WAITING" -Detail "no authorized online device"
            if ($RequireDevice) {
                Add-Failure "No authorized online Android device found. Current adb states: $summary"
            }
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Android device connection check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android device connection check(s) failed."
}

if ($devices.Count -eq 0) {
    Write-Host "Android device connection check waiting on device."
}
elseif ($rows | Where-Object { $_.Check -eq "selected device" -and $_.Status -eq "OK" }) {
    Write-Host "Android device connection check OK."
}
elseif ($devices | Where-Object { $_.State -eq "unauthorized" }) {
    Write-Host "Android device connection check waiting on authorization."
}
elseif ($devices | Where-Object { $_.State -eq "offline" }) {
    Write-Host "Android device connection check waiting on online device."
}
else {
    Write-Host "Android device connection check waiting on device selection."
}

Write-Host "Repo: $RepoRoot"
Write-Host "Adb: $AdbPath"
Write-Host "Devices: $summary"
$rows | Format-Table -AutoSize
