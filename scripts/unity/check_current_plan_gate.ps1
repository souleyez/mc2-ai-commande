param(
    [string]$RepoRoot = "",
    [switch]$SkipReadiness
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

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot $RelativePath
}

function Invoke-GateStep {
    param(
        [string]$Name,
        [string]$ScriptPath,
        [string[]]$Arguments = @(),
        [string[]]$RequiredMarkers = @(),
        [string[]]$AnySuccessMarkers = @()
    )

    if (-not (Test-Path -LiteralPath $ScriptPath)) {
        Add-Failure "$Name script missing: $ScriptPath"
        return
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
    $joined = $lines -join [Environment]::NewLine

    $missing = New-Object System.Collections.Generic.List[string]
    foreach ($marker in $RequiredMarkers) {
        if ($joined -notlike "*$marker*") {
            [void]$missing.Add($marker)
        }
    }

    $matchedAny = ""
    if ($AnySuccessMarkers.Count -gt 0) {
        foreach ($marker in $AnySuccessMarkers) {
            if ($joined -like "*$marker*") {
                $matchedAny = $marker
                break
            }
        }

        if ([string]::IsNullOrWhiteSpace($matchedAny)) {
            [void]$missing.Add("one of: $($AnySuccessMarkers -join ' | ')")
        }
    }

    if ($exitCode -ne 0 -or $missing.Count -gt 0) {
        Add-Failure "$Name failed with exit code $exitCode."
        foreach ($marker in $missing) {
            Add-Failure "$Name missing marker: $marker"
        }

        foreach ($line in $lines) {
            Add-Failure "  $line"
        }

        return
    }

    $detailMarkers = @($RequiredMarkers)
    if (-not [string]::IsNullOrWhiteSpace($matchedAny)) {
        $detailMarkers += $matchedAny
    }

    [void]$rows.Add([pscustomobject]@{
        Step = $Name
        Status = if ($matchedAny -like "*waiting on device*") { "WAITING" } else { "OK" }
        Detail = ($detailMarkers -join "; ")
    })
}

$handoffScript = Resolve-RepoPath -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$readinessScript = Resolve-RepoPath -RelativePath "scripts\unity\check_controlled_demo_readiness.ps1"
$sourceHygieneScript = Resolve-RepoPath -RelativePath "scripts\unity\check_demo_source_hygiene.ps1"
$aiDeputyScript = Resolve-RepoPath -RelativePath "scripts\unity\check_ai_deputy_contract.ps1"
$mobileCommandScript = Resolve-RepoPath -RelativePath "scripts\unity\check_mobile_command_model_preflight.ps1"
$battleHudScript = Resolve-RepoPath -RelativePath "scripts\unity\check_battle_hud_sparse_contract.ps1"
$androidApkFreshnessScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_freshness.ps1"
$androidApkIdentityScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_identity.ps1"
$androidApkCompatibilityScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_compatibility.ps1"
$androidApkSigningScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_signing.ps1"
$androidApkManifestScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_manifest.ps1"
$androidPreflightScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_device_preflight.ps1"
$androidSmokeScript = Resolve-RepoPath -RelativePath "scripts\unity\android_device_smoke.ps1"
$androidLogScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_smoke_log.ps1"

$handoffArgs = @("-RepoRoot", $RepoRoot)

Invoke-GateStep `
    -Name "Controlled demo handoff gate" `
    -ScriptPath $handoffScript `
    -Arguments $handoffArgs `
    -RequiredMarkers @("Controlled demo handoff consistency check OK.")

if (-not $SkipReadiness) {
    Invoke-GateStep `
        -Name "Controlled demo readiness gate" `
        -ScriptPath $readinessScript `
        -Arguments @("-RepoRoot", $RepoRoot) `
        -RequiredMarkers @("Controlled demo readiness preflight OK.")
}

Invoke-GateStep `
    -Name "Demo source hygiene gate" `
    -ScriptPath $sourceHygieneScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Demo source hygiene check OK.")

Invoke-GateStep `
    -Name "AI deputy contract gate" `
    -ScriptPath $aiDeputyScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("AI deputy contract check OK.")

Invoke-GateStep `
    -Name "Mobile command model gate" `
    -ScriptPath $mobileCommandScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Mobile command model preflight OK.")

Invoke-GateStep `
    -Name "Battle HUD sparse contract gate" `
    -ScriptPath $battleHudScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Battle HUD sparse contract check OK.")

Invoke-GateStep `
    -Name "Android APK freshness gate" `
    -ScriptPath $androidApkFreshnessScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Android APK freshness check OK.")

Invoke-GateStep `
    -Name "Android APK identity gate" `
    -ScriptPath $androidApkIdentityScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Android APK identity check OK.")

Invoke-GateStep `
    -Name "Android APK compatibility gate" `
    -ScriptPath $androidApkCompatibilityScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Android APK compatibility check OK.")

Invoke-GateStep `
    -Name "Android APK signing gate" `
    -ScriptPath $androidApkSigningScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Android APK signing check OK.")

Invoke-GateStep `
    -Name "Android APK manifest gate" `
    -ScriptPath $androidApkManifestScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Android APK manifest check OK.")

Invoke-GateStep `
    -Name "Android device gate" `
    -ScriptPath $androidPreflightScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-AllowNoDevice") `
    -AnySuccessMarkers @(
        "Android device smoke preflight OK.",
        "Android device smoke preflight waiting on device."
    )

Invoke-GateStep `
    -Name "Android smoke log parser gate" `
    -ScriptPath $androidLogScript `
    -Arguments @("-SelfTest") `
    -RequiredMarkers @("Android smoke log check self-test OK.")

Invoke-GateStep `
    -Name "Android smoke plan gate" `
    -ScriptPath $androidSmokeScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("Android device smoke plan OK.")

if ($failures.Count -gt 0) {
    Write-Host "Current plan gate check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) current plan gate check(s) failed."
}

Write-Host "Current plan gate check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
