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
        [string]$Status,
        [string]$Detail
    )

    [void]$rows.Add([pscustomobject]@{
        Check = $Check
        Status = $Status
        Detail = $Detail
    })
}

function Get-PnpPropertyText {
    param(
        [string]$InstanceId,
        [string]$KeyName
    )

    $property = Get-PnpDeviceProperty -InstanceId $InstanceId -KeyName $KeyName -ErrorAction SilentlyContinue
    if ($null -eq $property -or $null -eq $property.Data) {
        return ""
    }

    if ($property.Data -is [array]) {
        return ($property.Data | ForEach-Object { $_.ToString() }) -join ", "
    }

    return $property.Data.ToString()
}

function Get-DriverBlocks {
    param([string[]]$Lines)

    $blocks = @()
    $current = New-Object System.Collections.Generic.List[string]
    foreach ($line in $Lines) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            if ($current.Count -gt 0) {
                $blocks += ,@($current)
                $current.Clear()
            }
            continue
        }

        if ($line -notmatch "^Microsoft PnP Utility$") {
            [void]$current.Add($line)
        }
    }

    if ($current.Count -gt 0) {
        $blocks += ,@($current)
    }

    return $blocks
}

function Get-BlockField {
    param(
        [string[]]$Block,
        [string]$Name
    )

    $line = $Block | Where-Object { $_ -match "^$([regex]::Escape($Name)):\s*(.*)$" } | Select-Object -First 1
    if ($null -eq $line) {
        return ""
    }

    return ($line -replace "^$([regex]::Escape($Name)):\s*", "").Trim()
}

$androidVendorPattern = "VID_(18D1|2717|04E8|12D1|22B8|2A70|0BB4|1004|19D2|2D95|2D04|05C6)"
$namePattern = "Android|ADB|Xiaomi|\bMi\s|Redmi|POCO|Galaxy|Huawei|Honor|OnePlus|OPPO|vivo|Phone|Pixel"
$driverCandidatePattern = "Android|ADB|Xiaomi|Google|WinUSB|android_winusb|usb_driver|miusb|xiaomi|Composite ADB"

$pnpDevices = @()
if ($null -eq (Get-Command Get-PnpDevice -ErrorAction SilentlyContinue)) {
    Add-Failure "Get-PnpDevice is unavailable; cannot inspect current Windows Android driver state."
}
else {
    $pnpDevices = @(
        Get-PnpDevice -PresentOnly -ErrorAction SilentlyContinue |
            Where-Object {
                $_.InstanceId -match $androidVendorPattern -or
                $_.FriendlyName -match $namePattern
            } |
            ForEach-Object {
                [pscustomobject]@{
                    Status = $_.Status
                    Class = $_.Class
                    FriendlyName = $_.FriendlyName
                    InstanceId = $_.InstanceId
                    Service = Get-PnpPropertyText -InstanceId $_.InstanceId -KeyName "DEVPKEY_Device_Service"
                    DriverProvider = Get-PnpPropertyText -InstanceId $_.InstanceId -KeyName "DEVPKEY_Device_DriverProvider"
                    DriverDesc = Get-PnpPropertyText -InstanceId $_.InstanceId -KeyName "DEVPKEY_Device_DriverDesc"
                    DriverInfPath = Get-PnpPropertyText -InstanceId $_.InstanceId -KeyName "DEVPKEY_Device_DriverInfPath"
                    Manufacturer = Get-PnpPropertyText -InstanceId $_.InstanceId -KeyName "DEVPKEY_Device_Manufacturer"
                    HardwareIds = Get-PnpPropertyText -InstanceId $_.InstanceId -KeyName "DEVPKEY_Device_HardwareIds"
                    CompatibleIds = Get-PnpPropertyText -InstanceId $_.InstanceId -KeyName "DEVPKEY_Device_CompatibleIds"
                }
            }
    )
}

$pnputil = Get-Command pnputil.exe -ErrorAction SilentlyContinue
if ($null -eq $pnputil) {
    Add-Failure "pnputil.exe is unavailable; cannot enumerate installed driver packages."
}

$candidateSummaries = @()
if ($null -ne $pnputil) {
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $pnputilOutput = & $pnputil.Source /enum-drivers 2>&1
        $pnputilExitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($pnputilExitCode -ne 0) {
        Add-Failure "pnputil /enum-drivers failed with exit code $pnputilExitCode`: $($pnputilOutput -join ' ')"
    }
    else {
        $blocks = @(Get-DriverBlocks -Lines @($pnputilOutput | ForEach-Object { $_.ToString() }))
        foreach ($block in $blocks) {
            $blockText = $block -join " "
            if ($blockText -match $driverCandidatePattern) {
                $publishedName = Get-BlockField -Block $block -Name "Published Name"
                $originalName = Get-BlockField -Block $block -Name "Original Name"
                $providerName = Get-BlockField -Block $block -Name "Provider Name"
                $className = Get-BlockField -Block $block -Name "Class Name"

                $summaryParts = @()
                if (-not [string]::IsNullOrWhiteSpace($publishedName)) { $summaryParts += "published=$publishedName" }
                if (-not [string]::IsNullOrWhiteSpace($originalName)) { $summaryParts += "original=$originalName" }
                if (-not [string]::IsNullOrWhiteSpace($providerName)) { $summaryParts += "provider=$providerName" }
                if (-not [string]::IsNullOrWhiteSpace($className)) { $summaryParts += "class=$className" }

                if ($summaryParts.Count -eq 0) {
                    $summaryParts += ($blockText.Substring(0, [Math]::Min(160, $blockText.Length)))
                }

                $candidateSummaries += ($summaryParts -join "; ")
            }
        }
    }
}

if ($pnpDevices.Count -eq 0) {
    Add-Row -Check "current android pnp" -Status "WAITING" -Detail "No current Android-like PnP device found."
}
else {
    foreach ($device in $pnpDevices) {
        $detail = "name=$($device.FriendlyName); class=$($device.Class); provider=$($device.DriverProvider); desc=$($device.DriverDesc); inf=$($device.DriverInfPath); service=$($device.Service); hardwareIds=$($device.HardwareIds)"
        $isAdb = ($device.FriendlyName -match "ADB|Android Composite ADB|Android Bootloader" -or $device.Class -match "AndroidUsbDevice" -or $device.Service -match "WinUSB")
        $isMtp = ($device.Class -eq "WPD" -or $device.Service -match "WpdMtp" -or $device.DriverInfPath -match "wpdmtp\.inf")
        $status = if ($isAdb) { "OK" } elseif ($isMtp) { "WAITING" } else { "INFO" }
        Add-Row -Check "current android pnp" -Status $status -Detail $detail
    }
}

$candidateStatus = if ($candidateSummaries.Count -gt 0) { "OK" } else { "WAITING" }
$candidateDetail = if ($candidateSummaries.Count -gt 0) {
    ($candidateSummaries | Select-Object -First 8) -join " | "
}
else {
    "none found by pnputil pattern: $driverCandidatePattern"
}
Add-Row -Check "installed adb driver packages" -Status $candidateStatus -Detail $candidateDetail

$currentPhoneDriver = "none"
$currentPhone = $pnpDevices | Where-Object { $_.InstanceId -match "VID_2717" -or $_.FriendlyName -match "Mi 11|Xiaomi|Redmi|POCO" } | Select-Object -First 1
if ($null -ne $currentPhone) {
    $currentPhoneDriver = "name=$($currentPhone.FriendlyName); class=$($currentPhone.Class); provider=$($currentPhone.DriverProvider); desc=$($currentPhone.DriverDesc); inf=$($currentPhone.DriverInfPath); service=$($currentPhone.Service)"
}

if ($failures.Count -gt 0) {
    Write-Host "Android ADB driver package probe failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android ADB driver package probe check(s) failed."
}

Write-Host "Android ADB driver package probe OK."
Write-Host "Repo: $RepoRoot"
Write-Host "AdbDriverPackageProbe: True"
Write-Host "NoInstallOrLaunch: True"
Write-Host "CandidateDriverPackages: $(if ($candidateSummaries.Count -gt 0) { 'found' } else { 'none' })"
Write-Host "CurrentPhoneDriver: $currentPhoneDriver"
Write-Host "NextGate: G3 Run Android device smoke"
$rows | Format-Table -AutoSize
