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
$androidSmokeArtifactHygieneScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_smoke_artifact_hygiene.ps1"
$aiDeputyScript = Resolve-RepoPath -RelativePath "scripts\unity\check_ai_deputy_contract.ps1"
$mobileCommandScript = Resolve-RepoPath -RelativePath "scripts\unity\check_mobile_command_model_preflight.ps1"
$battleHudScript = Resolve-RepoPath -RelativePath "scripts\unity\check_battle_hud_sparse_contract.ps1"
$pcVisualCaptureSanityScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_visual_capture_sanity.ps1"
$pcCaptureSidecarSchemaScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_capture_sidecar_schema.ps1"
$pcCapturePresetContractScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_capture_preset_contract.ps1"
$pcCaptureArtifactHygieneScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_capture_artifact_hygiene.ps1"
$pcWindowContractScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_window_contract.ps1"
$pcLaunchLogHygieneScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_launch_log_hygiene.ps1"
$pcBuildArtifactHygieneScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_build_artifact_hygiene.ps1"
$pcSmokeArtifactHygieneScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_smoke_artifact_hygiene.ps1"
$androidSdkToolingScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_sdk_tooling.ps1"
$androidApkFreshnessScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_freshness.ps1"
$androidApkIdentityScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_identity.ps1"
$androidApkCompatibilityScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_compatibility.ps1"
$androidApkSigningScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_signing.ps1"
$androidApkManifestScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_manifest.ps1"
$androidApkPayloadScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_payload.ps1"
$androidApkSizeBudgetScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_size_budget.ps1"
$androidPreflightScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_device_preflight.ps1"
$androidSmokeScript = Resolve-RepoPath -RelativePath "scripts\unity\android_device_smoke.ps1"
$androidSmokePlanConsistencyScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_smoke_plan_consistency.ps1"
$androidG3ReadinessScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_g3_readiness.ps1"
$androidG3DeviceRequirementScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_g3_device_requirement.ps1"
$androidLogScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_smoke_log.ps1"
$androidSummaryScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_smoke_summary.ps1"

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
    -Name "Android smoke artifact hygiene gate" `
    -ScriptPath $androidSmokeArtifactHygieneScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Android smoke artifact hygiene check OK.")

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
    -Name "PC visual capture sanity gate" `
    -ScriptPath $pcVisualCaptureSanityScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("PC visual capture sanity check OK.")

Invoke-GateStep `
    -Name "PC visual capture sanity self-test gate" `
    -ScriptPath $pcVisualCaptureSanityScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-SelfTest") `
    -RequiredMarkers @("PC visual capture sanity self-test OK.")

Invoke-GateStep `
    -Name "PC capture sidecar schema gate" `
    -ScriptPath $pcCaptureSidecarSchemaScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("PC capture sidecar schema check OK.")

Invoke-GateStep `
    -Name "PC capture preset contract gate" `
    -ScriptPath $pcCapturePresetContractScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("PC capture preset contract check OK.")

Invoke-GateStep `
    -Name "PC capture artifact hygiene gate" `
    -ScriptPath $pcCaptureArtifactHygieneScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("PC capture artifact hygiene check OK.")

Invoke-GateStep `
    -Name "PC window contract gate" `
    -ScriptPath $pcWindowContractScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("PC window contract check OK.")

Invoke-GateStep `
    -Name "PC launch log hygiene gate" `
    -ScriptPath $pcLaunchLogHygieneScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("PC launch log hygiene check OK.")

Invoke-GateStep `
    -Name "PC build artifact hygiene gate" `
    -ScriptPath $pcBuildArtifactHygieneScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("PC build artifact hygiene check OK.")

Invoke-GateStep `
    -Name "PC smoke artifact hygiene gate" `
    -ScriptPath $pcSmokeArtifactHygieneScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("PC smoke artifact hygiene check OK.")

Invoke-GateStep `
    -Name "Android SDK tooling gate" `
    -ScriptPath $androidSdkToolingScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Android SDK tooling check OK.")

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
    -Name "Android APK payload gate" `
    -ScriptPath $androidApkPayloadScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Android APK payload check OK.")

Invoke-GateStep `
    -Name "Android APK size budget gate" `
    -ScriptPath $androidApkSizeBudgetScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Android APK size budget check OK.")

Invoke-GateStep `
    -Name "Android device gate" `
    -ScriptPath $androidPreflightScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-AllowNoDevice") `
    -RequiredMarkers @(
        "smoke summary schema",
        "Android smoke summary check self-test OK."
    ) `
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
    -Name "Android smoke summary parser gate" `
    -ScriptPath $androidSummaryScript `
    -Arguments @("-SelfTest") `
    -RequiredMarkers @("Android smoke summary check self-test OK.")

Invoke-GateStep `
    -Name "Android smoke plan gate" `
    -ScriptPath $androidSmokeScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @(
        "Android device smoke plan OK.",
        "Screenshot:",
        "ScreenshotCapture: True",
        "Summary:",
        "SummaryWrite: True"
    )

Invoke-GateStep `
    -Name "Android smoke plan consistency gate" `
    -ScriptPath $androidSmokePlanConsistencyScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Android smoke plan/preflight consistency check OK.")

Invoke-GateStep `
    -Name "Android G3 readiness gate" `
    -ScriptPath $androidG3ReadinessScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @(
        "Android smoke plan/preflight consistency check OK.",
        "Android device smoke plan OK.",
        "Android smoke log check self-test OK.",
        "Android smoke summary check self-test OK."
    ) `
    -AnySuccessMarkers @(
        "Android G3 readiness check OK.",
        "Android G3 readiness check waiting on device."
    )

Invoke-GateStep `
    -Name "Android G3 device requirement gate" `
    -ScriptPath $androidG3DeviceRequirementScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -AnySuccessMarkers @(
        "Android G3 device requirement check OK.",
        "Android G3 device requirement check waiting on device."
    )

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
