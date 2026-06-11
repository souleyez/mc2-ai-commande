param(
    [string]$RepoRoot = "",
    [string]$ApkPath = "",
    [string]$AaptPath = "",
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

function Get-ApkIdentity {
    param(
        [string]$Apk,
        [string]$Aapt
    )

    $identity = @{
        PackageName = ""
        ActivityName = ""
    }

    $badgingResult = Invoke-NativeCommand -FilePath $Aapt -Arguments @("dump", "badging", $Apk)
    $badging = $badgingResult.Output
    if ($badgingResult.ExitCode -ne 0) {
        Add-Failure "aapt could not read APK badging: $($badging -join ' ')"
        return $identity
    }

    foreach ($line in $badging) {
        if ([string]::IsNullOrWhiteSpace($identity.PackageName) -and $line -match "package: name='([^']+)'") {
            $identity.PackageName = $Matches[1]
        }

        if ([string]::IsNullOrWhiteSpace($identity.ActivityName) -and $line -match "launchable-activity: name='([^']+)'") {
            $identity.ActivityName = $Matches[1]
        }
    }

    return $identity
}

if (-not (Test-Path -LiteralPath $ApkPath -PathType Leaf)) {
    Add-Failure "Missing Android APK: $ApkPath"
}

if (-not (Test-Path -LiteralPath $AaptPath -PathType Leaf)) {
    Add-Failure "Missing aapt: $AaptPath"
}

$identity = $null
if ((Test-Path -LiteralPath $ApkPath -PathType Leaf) -and (Test-Path -LiteralPath $AaptPath -PathType Leaf)) {
    $identity = Get-ApkIdentity -Apk $ApkPath -Aapt $AaptPath

    if ([string]::IsNullOrWhiteSpace($identity.PackageName)) {
        Add-Failure "APK package name could not be discovered."
    }
    elseif ($identity.PackageName -ne $ExpectedPackageName) {
        Add-Failure "APK package mismatch. Expected $ExpectedPackageName, got $($identity.PackageName)."
    }
    else {
        Add-Row -Check "package" -Detail $identity.PackageName
    }

    if ([string]::IsNullOrWhiteSpace($identity.ActivityName)) {
        Add-Failure "APK launchable activity could not be discovered."
    }
    elseif ($identity.ActivityName -ne $ExpectedActivityName) {
        Add-Failure "APK launch activity mismatch. Expected $ExpectedActivityName, got $($identity.ActivityName)."
    }
    else {
        Add-Row -Check "activity" -Detail $identity.ActivityName
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Android APK identity check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android APK identity check(s) failed."
}

Write-Host "Android APK identity check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "APK: $ApkPath"
Write-Host "ExpectedPackage: $ExpectedPackageName"
Write-Host "ExpectedActivity: $ExpectedActivityName"
$rows | Format-Table -AutoSize
