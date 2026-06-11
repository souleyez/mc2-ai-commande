param(
    [string]$RepoRoot = "",
    [string]$ApkPath = "",
    [string]$AaptPath = "",
    [string[]]$AllowedPermissions = @(
        "android.permission.INTERNET",
        "com.DefaultCompany.unitymc2demo.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION"
    ),
    [string[]]$ExpectedNotRequiredFeatures = @(
        "android.hardware.touchscreen",
        "android.hardware.vulkan.version"
    ),
    [string[]]$ExpectedSupportedScreens = @("small", "normal", "large", "xlarge")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ([string]::IsNullOrWhiteSpace($ApkPath)) {
    $ApkPath = Join-Path $RepoRoot "unity-mc2-demo\Builds\Android\MC2UnityDemo.apk"
}

$androidPlayer = Join-Path $HOME "Unity\Hub\Editor\6000.4.7f1\Editor\Data\PlaybackEngines\AndroidPlayer"
if ([string]::IsNullOrWhiteSpace($AaptPath)) {
    $AaptPath = Join-Path $androidPlayer "SDK\build-tools\36.0.0\aapt.exe"
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

function Get-QuotedValues {
    param([string]$Line)

    $values = @()
    $quotedMatches = [regex]::Matches($Line, "'([^']+)'")
    foreach ($quotedMatch in $quotedMatches) {
        $values += $quotedMatch.Groups[1].Value
    }

    return $values
}

function Compare-Set {
    param(
        [string[]]$Actual,
        [string[]]$Expected
    )

    $actualSorted = @($Actual | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Sort-Object)
    $expectedSorted = @($Expected | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Sort-Object)

    return [pscustomobject]@{
        Matches = (($actualSorted -join "|") -eq ($expectedSorted -join "|"))
        Actual = $actualSorted
        Expected = $expectedSorted
    }
}

function Get-ApkManifestFacts {
    param(
        [string]$Apk,
        [string]$Aapt
    )

    $facts = @{
        Permissions = @()
        RequiredFeatures = @()
        NotRequiredFeatures = @()
        SupportedScreens = @()
    }

    $badgingResult = Invoke-NativeCommand -FilePath $Aapt -Arguments @("dump", "badging", $Apk)
    $badging = $badgingResult.Output
    if ($badgingResult.ExitCode -ne 0) {
        Add-Failure "aapt could not read APK badging: $($badging -join ' ')"
        return $facts
    }

    foreach ($line in $badging) {
        if ($line -match "^uses-permission: name='([^']+)'") {
            $facts["Permissions"] += $Matches[1]
        }

        if ($line -match "^\s*uses-feature: name='([^']+)'") {
            $facts["RequiredFeatures"] += $Matches[1]
        }

        if ($line -match "^\s*uses-feature-not-required: name='([^']+)'") {
            $facts["NotRequiredFeatures"] += $Matches[1]
        }

        if ($line -match "^supports-screens:") {
            $facts["SupportedScreens"] = @(Get-QuotedValues -Line $line)
        }
    }

    return $facts
}

if (-not (Test-Path -LiteralPath $ApkPath -PathType Leaf)) {
    Add-Failure "Missing Android APK: $ApkPath"
}

if (-not (Test-Path -LiteralPath $AaptPath -PathType Leaf)) {
    Add-Failure "Missing aapt: $AaptPath"
}

if ((Test-Path -LiteralPath $ApkPath -PathType Leaf) -and (Test-Path -LiteralPath $AaptPath -PathType Leaf)) {
    $facts = Get-ApkManifestFacts -Apk $ApkPath -Aapt $AaptPath

    $permissionComparison = Compare-Set -Actual $facts["Permissions"] -Expected $AllowedPermissions
    if (-not $permissionComparison.Matches) {
        Add-Failure "APK permissions mismatch. Expected $($permissionComparison.Expected -join ', '), got $($permissionComparison.Actual -join ', ')."
    }
    else {
        Add-Row -Check "permissions" -Detail ($permissionComparison.Actual -join ", ")
    }

    $requiredFeatures = @($facts["RequiredFeatures"] | Sort-Object)
    if ($requiredFeatures.Count -gt 0) {
        Add-Failure "APK has required hardware features that may narrow install targets: $($requiredFeatures -join ', ')."
    }
    else {
        Add-Row -Check "required features" -Detail "none"
    }

    $notRequiredComparison = Compare-Set -Actual $facts["NotRequiredFeatures"] -Expected $ExpectedNotRequiredFeatures
    if (-not $notRequiredComparison.Matches) {
        Add-Failure "APK not-required features mismatch. Expected $($notRequiredComparison.Expected -join ', '), got $($notRequiredComparison.Actual -join ', ')."
    }
    else {
        Add-Row -Check "not-required features" -Detail ($notRequiredComparison.Actual -join ", ")
    }

    $screenComparison = Compare-Set -Actual $facts["SupportedScreens"] -Expected $ExpectedSupportedScreens
    if (-not $screenComparison.Matches) {
        Add-Failure "APK supports-screens mismatch. Expected $($screenComparison.Expected -join ', '), got $($screenComparison.Actual -join ', ')."
    }
    else {
        Add-Row -Check "supports-screens" -Detail ($screenComparison.Actual -join ", ")
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Android APK manifest check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android APK manifest check(s) failed."
}

Write-Host "Android APK manifest check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "APK: $ApkPath"
Write-Host "AllowedPermissions: $($AllowedPermissions -join ', ')"
Write-Host "ExpectedNotRequiredFeatures: $($ExpectedNotRequiredFeatures -join ', ')"
Write-Host "ExpectedSupportedScreens: $($ExpectedSupportedScreens -join ', ')"
$rows | Format-Table -AutoSize
