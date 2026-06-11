param(
    [string]$RepoRoot = "",
    [string]$ApkPath = "",
    [string]$ApksignerPath = "",
    [string]$ExpectedSignerCertificateDn = "C=US, O=Android, CN=Android Debug",
    [switch]$AllowAnySigner
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
if ([string]::IsNullOrWhiteSpace($ApksignerPath)) {
    $ApksignerPath = Join-Path $androidPlayer "SDK\build-tools\36.0.0\apksigner.bat"
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

function Get-ApkSigning {
    param(
        [string]$Apk,
        [string]$Apksigner
    )

    $signing = @{
        Verifies = $false
        V2 = $false
        SignerDn = ""
        SignerSha256 = ""
    }

    $verifyResult = Invoke-NativeCommand -FilePath $Apksigner -Arguments @("verify", "--verbose", "--print-certs", $Apk)
    $verifyOutput = $verifyResult.Output
    if ($verifyResult.ExitCode -ne 0) {
        Add-Failure "apksigner could not verify APK signing: $($verifyOutput -join ' ')"
        return $signing
    }

    foreach ($line in $verifyOutput) {
        if ($line -match "^Verifies$") {
            $signing["Verifies"] = $true
        }

        if ($line -match "^Verified using v2 scheme .*: true$") {
            $signing["V2"] = $true
        }

        if ([string]::IsNullOrWhiteSpace($signing["SignerDn"]) -and $line -match "^Signer #1 certificate DN: (.+)$") {
            $signing["SignerDn"] = $Matches[1].Trim()
        }

        if ([string]::IsNullOrWhiteSpace($signing["SignerSha256"]) -and $line -match "^Signer #1 certificate SHA-256 digest: (.+)$") {
            $signing["SignerSha256"] = $Matches[1].Trim()
        }
    }

    return $signing
}

if (-not (Test-Path -LiteralPath $ApkPath -PathType Leaf)) {
    Add-Failure "Missing Android APK: $ApkPath"
}

if (-not (Test-Path -LiteralPath $ApksignerPath -PathType Leaf)) {
    Add-Failure "Missing apksigner: $ApksignerPath"
}

if ((Test-Path -LiteralPath $ApkPath -PathType Leaf) -and (Test-Path -LiteralPath $ApksignerPath -PathType Leaf)) {
    $signing = Get-ApkSigning -Apk $ApkPath -Apksigner $ApksignerPath

    if (-not $signing["Verifies"]) {
        Add-Failure "APK signing verification marker was not found."
    }
    else {
        Add-Row -Check "verifies" -Detail "apksigner verify"
    }

    if (-not $signing["V2"]) {
        Add-Failure "APK Signature Scheme v2 was not verified."
    }
    else {
        Add-Row -Check "v2 scheme" -Detail "true"
    }

    if ([string]::IsNullOrWhiteSpace($signing["SignerDn"])) {
        Add-Failure "APK signer certificate DN could not be discovered."
    }
    elseif (-not $AllowAnySigner -and $signing["SignerDn"] -ne $ExpectedSignerCertificateDn) {
        Add-Failure "APK signer certificate DN mismatch. Expected $ExpectedSignerCertificateDn, got $($signing["SignerDn"])."
    }
    else {
        Add-Row -Check "signer DN" -Detail $signing["SignerDn"]
    }

    if ([string]::IsNullOrWhiteSpace($signing["SignerSha256"])) {
        Add-Failure "APK signer certificate SHA-256 digest could not be discovered."
    }
    else {
        Add-Row -Check "signer SHA-256" -Detail $signing["SignerSha256"]
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Android APK signing check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android APK signing check(s) failed."
}

Write-Host "Android APK signing check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "APK: $ApkPath"
Write-Host "ExpectedSignerCertificateDn: $ExpectedSignerCertificateDn"
Write-Host "AllowAnySigner: $AllowAnySigner"
$rows | Format-Table -AutoSize
