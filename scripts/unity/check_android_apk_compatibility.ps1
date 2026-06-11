param(
    [string]$RepoRoot = "",
    [string]$ApkPath = "",
    [string]$AaptPath = "",
    [int]$ExpectedMinSdkVersion = 25,
    [int]$ExpectedTargetSdkVersion = 36,
    [string[]]$ExpectedNativeCode = @("arm64-v8a")
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

function Get-ApkCompatibility {
    param(
        [string]$Apk,
        [string]$Aapt
    )

    $compatibility = @{
        MinSdkVersion = ""
        TargetSdkVersion = ""
        NativeCode = @()
    }

    $badgingResult = Invoke-NativeCommand -FilePath $Aapt -Arguments @("dump", "badging", $Apk)
    $badging = $badgingResult.Output
    if ($badgingResult.ExitCode -ne 0) {
        Add-Failure "aapt could not read APK badging: $($badging -join ' ')"
        return $compatibility
    }

    foreach ($line in $badging) {
        if ([string]::IsNullOrWhiteSpace($compatibility["MinSdkVersion"]) -and $line -match "^sdkVersion:'(\d+)'") {
            $compatibility["MinSdkVersion"] = $Matches[1]
        }

        if ([string]::IsNullOrWhiteSpace($compatibility["TargetSdkVersion"]) -and $line -match "^targetSdkVersion:'(\d+)'") {
            $compatibility["TargetSdkVersion"] = $Matches[1]
        }

        if ($compatibility["NativeCode"].Count -eq 0 -and $line -match "^native-code:") {
            $compatibility["NativeCode"] = @(Get-QuotedValues -Line $line)
        }
    }

    return $compatibility
}

if (-not (Test-Path -LiteralPath $ApkPath -PathType Leaf)) {
    Add-Failure "Missing Android APK: $ApkPath"
}

if (-not (Test-Path -LiteralPath $AaptPath -PathType Leaf)) {
    Add-Failure "Missing aapt: $AaptPath"
}

if ((Test-Path -LiteralPath $ApkPath -PathType Leaf) -and (Test-Path -LiteralPath $AaptPath -PathType Leaf)) {
    $compatibility = Get-ApkCompatibility -Apk $ApkPath -Aapt $AaptPath

    if ([string]::IsNullOrWhiteSpace($compatibility["MinSdkVersion"])) {
        Add-Failure "APK minSdkVersion could not be discovered."
    }
    elseif ([int]$compatibility["MinSdkVersion"] -ne $ExpectedMinSdkVersion) {
        Add-Failure "APK minSdkVersion mismatch. Expected $ExpectedMinSdkVersion, got $($compatibility["MinSdkVersion"])."
    }
    else {
        Add-Row -Check "minSdkVersion" -Detail $compatibility["MinSdkVersion"]
    }

    if ([string]::IsNullOrWhiteSpace($compatibility["TargetSdkVersion"])) {
        Add-Failure "APK targetSdkVersion could not be discovered."
    }
    elseif ([int]$compatibility["TargetSdkVersion"] -ne $ExpectedTargetSdkVersion) {
        Add-Failure "APK targetSdkVersion mismatch. Expected $ExpectedTargetSdkVersion, got $($compatibility["TargetSdkVersion"])."
    }
    else {
        Add-Row -Check "targetSdkVersion" -Detail $compatibility["TargetSdkVersion"]
    }

    $actualNativeCode = @($compatibility["NativeCode"] | Sort-Object)
    $expectedNativeCode = @($ExpectedNativeCode | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Sort-Object)
    if ($expectedNativeCode.Count -gt 0) {
        if ($actualNativeCode.Count -eq 0) {
            Add-Failure "APK native-code ABIs could not be discovered."
        }
        elseif (($actualNativeCode -join ",") -ne ($expectedNativeCode -join ",")) {
            Add-Failure "APK native-code mismatch. Expected $($expectedNativeCode -join ', '), got $($actualNativeCode -join ', ')."
        }
        else {
            Add-Row -Check "native-code" -Detail ($actualNativeCode -join ", ")
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Android APK compatibility check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android APK compatibility check(s) failed."
}

Write-Host "Android APK compatibility check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "APK: $ApkPath"
Write-Host "ExpectedMinSdkVersion: $ExpectedMinSdkVersion"
Write-Host "ExpectedTargetSdkVersion: $ExpectedTargetSdkVersion"
Write-Host "ExpectedNativeCode: $($ExpectedNativeCode -join ', ')"
$rows | Format-Table -AutoSize
