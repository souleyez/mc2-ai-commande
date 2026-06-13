param(
    [string]$RepoRoot = "",
    [switch]$SkipReadiness,
    [int]$StepTimeoutSeconds = 180
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ($StepTimeoutSeconds -lt 10) {
    throw "StepTimeoutSeconds must be at least 10."
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

    Write-Host "Running gate step: $Name"
    $job = Start-Job -ScriptBlock {
        param([string[]]$ChildArgs)

        $output = & powershell @ChildArgs 2>&1
        [pscustomobject]@{
            ExitCode = $LASTEXITCODE
            Output = @($output | ForEach-Object { $_.ToString() })
        }
    } -ArgumentList (, $processArgs)

    $completed = $null
    try {
        $completed = Wait-Job -Job $job -Timeout $StepTimeoutSeconds
        if ($null -eq $completed) {
            Stop-Job -Job $job -ErrorAction SilentlyContinue
            $partialOutput = @(Receive-Job -Job $job -ErrorAction SilentlyContinue | ForEach-Object { $_.ToString() })
            Add-Failure "$Name timed out after $StepTimeoutSeconds second(s)."
            foreach ($line in ($partialOutput | Select-Object -Last 80)) {
                Add-Failure "  $line"
            }

            return
        }

        $jobResult = Receive-Job -Job $job
    }
    finally {
        Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
    }

    $exitCode = [int]$jobResult.ExitCode
    $lines = @($jobResult.Output | ForEach-Object { $_.ToString() })
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
        Status = if ($matchedAny -like "*waiting on device*" -or $matchedAny -like "*G3DeviceReady: False*" -or $matchedAny -like "*G3DeviceStatus: waiting*") { "WAITING" } else { "OK" }
        Detail = ($detailMarkers -join "; ")
    })
}

$handoffScript = Resolve-RepoPath -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$readinessScript = Resolve-RepoPath -RelativePath "scripts\unity\check_controlled_demo_readiness.ps1"
$sourceHygieneScript = Resolve-RepoPath -RelativePath "scripts\unity\check_demo_source_hygiene.ps1"
$androidSmokeArtifactHygieneScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_smoke_artifact_hygiene.ps1"
$aiDeputyScript = Resolve-RepoPath -RelativePath "scripts\unity\check_ai_deputy_contract.ps1"
$mobileCommandScript = Resolve-RepoPath -RelativePath "scripts\unity\check_mobile_command_model_preflight.ps1"
$mobileLandscapeContractScript = Resolve-RepoPath -RelativePath "scripts\unity\check_mobile_landscape_contract.ps1"
$landscapeTouchUiErgonomicsScript = Resolve-RepoPath -RelativePath "scripts\unity\check_landscape_touch_ui_ergonomics.ps1"
$landscapeMechLabTouchControlsScript = Resolve-RepoPath -RelativePath "scripts\unity\check_landscape_mechlab_touch_controls.ps1"
$battleHudScript = Resolve-RepoPath -RelativePath "scripts\unity\check_battle_hud_sparse_contract.ps1"
$pcVisualCaptureSanityScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_visual_capture_sanity.ps1"
$pcCaptureSidecarSchemaScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_capture_sidecar_schema.ps1"
$pcCapturePresetContractScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_capture_preset_contract.ps1"
$pcCaptureArtifactHygieneScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_capture_artifact_hygiene.ps1"
$pcWindowContractScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_window_contract.ps1"
$pcLaunchLogHygieneScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_launch_log_hygiene.ps1"
$pcBuildArtifactHygieneScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_build_artifact_hygiene.ps1"
$pcSmokeArtifactHygieneScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_smoke_artifact_hygiene.ps1"
$currentPlanQueueScript = Resolve-RepoPath -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$androidSdkToolingScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_sdk_tooling.ps1"
$androidApkFreshnessScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_freshness.ps1"
$androidApkIdentityScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_identity.ps1"
$androidApkCompatibilityScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_compatibility.ps1"
$androidApkSigningScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_signing.ps1"
$androidApkManifestScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_manifest.ps1"
$androidApkPayloadScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_payload.ps1"
$androidApkSizeBudgetScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_apk_size_budget.ps1"
$mobilePerformanceBudgetScript = Resolve-RepoPath -RelativePath "scripts\unity\check_mobile_performance_budget.ps1"
$iosFeasibilityScript = Resolve-RepoPath -RelativePath "scripts\unity\check_ios_feasibility_gate.ps1"
$mapAuthoringScript = Resolve-RepoPath -RelativePath "scripts\unity\check_map_authoring_contract.ps1"
$webRankingScript = Resolve-RepoPath -RelativePath "scripts\unity\check_web_ranking_contract.ps1"
$creatorEconomyScript = Resolve-RepoPath -RelativePath "scripts\unity\check_creator_economy_boundary.ps1"
$serverBoundaryScript = Resolve-RepoPath -RelativePath "scripts\unity\check_server_implementation_boundary.ps1"
$localMainServerScript = Resolve-RepoPath -RelativePath "scripts\server\check_local_main_server.ps1"
$unityMainServerIntegrationScript = Resolve-RepoPath -RelativePath "scripts\unity\check_unity_main_server_integration_contract.ps1"
$optionalUnityMainServerClientScript = Resolve-RepoPath -RelativePath "scripts\unity\check_optional_unity_main_server_client_adapter.ps1"
$optionalUnityMainServerLaunchDebriefScript = Resolve-RepoPath -RelativePath "scripts\unity\check_optional_unity_main_server_launch_debrief_smoke.ps1"
$optionalUnityInventoryBootstrapScript = Resolve-RepoPath -RelativePath "scripts\unity\check_optional_unity_inventory_bootstrap_smoke.ps1"
$inventoryMechBayBindingScript = Resolve-RepoPath -RelativePath "scripts\unity\check_inventory_mechbay_binding_boundary.ps1"
$optionalInventoryMechBayPreviewScript = Resolve-RepoPath -RelativePath "scripts\unity\check_optional_inventory_mechbay_preview_binding.ps1"
$inventoryMechBayPreviewEvidenceScript = Resolve-RepoPath -RelativePath "scripts\unity\capture_inventory_mechbay_preview_evidence.ps1"
$landscapePhoneMechLabSourceLineScript = Resolve-RepoPath -RelativePath "scripts\unity\capture_landscape_phone_mechlab_source_line_evidence.ps1"
$landscapeMechLabTouchEvidenceScript = Resolve-RepoPath -RelativePath "scripts\unity\capture_landscape_mechlab_touch_evidence.ps1"
$androidMechLabTouchEvidenceScript = Resolve-RepoPath -RelativePath "scripts\unity\capture_android_mechlab_touch_evidence.ps1"
$androidBattleCommandTouchEvidenceScript = Resolve-RepoPath -RelativePath "scripts\unity\capture_android_battle_command_touch_evidence.ps1"
$androidCombatEffectLogNoiseScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_combat_effect_log_noise.ps1"
$androidEntityPlaceholderCollisionPathScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_entity_placeholder_collision_path.ps1"
$androidEntityPlaceholderCollisionRuntimeEvidenceScript = Resolve-RepoPath -RelativePath "scripts\unity\capture_android_entity_placeholder_collision_runtime_evidence.ps1"
$pcControlledDemoVisualReadabilityAuditScript = Resolve-RepoPath -RelativePath "scripts\unity\audit_pc_controlled_demo_visual_readability.ps1"
$pcControlledDemoVisualReadabilityFixesScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_visual_readability_fixes.ps1"
$pcControlledDemoVisualEvidenceScript = Resolve-RepoPath -RelativePath "scripts\unity\capture_pc_controlled_demo_visual_evidence.ps1"
$pcControlledDemoCommandReadabilityAuditScript = Resolve-RepoPath -RelativePath "scripts\unity\audit_pc_controlled_demo_command_readability_formation.ps1"
$pcControlledDemoCommandReadabilityFixesScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_command_readability_fixes.ps1"
$pcControlledDemoCommandEvidenceScript = Resolve-RepoPath -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$pcControlledDemoPlayableFlowAuditScript = Resolve-RepoPath -RelativePath "scripts\unity\audit_pc_controlled_demo_playable_flow_polish.ps1"
$pcControlledDemoPlayableFlowFixesScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_playable_flow_polish_fixes.ps1"
$pcControlledDemoInvestorReadinessAuditScript = Resolve-RepoPath -RelativePath "scripts\unity\audit_pc_controlled_demo_investor_readiness.ps1"
$pcControlledDemoInvestorReadinessFixesScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_readiness_fixes.ps1"
$pcControlledDemoInvestorEvidencePackageAuditScript = Resolve-RepoPath -RelativePath "scripts\unity\audit_pc_controlled_demo_investor_evidence_package.ps1"
$pcControlledDemoInvestorEvidencePackageFixesScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_evidence_package_fixes.ps1"
$pcControlledDemoInvestorEvidenceRefreshScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_evidence_refresh.ps1"
$pcControlledDemoInvestorEvidenceRefreshAuditScript = Resolve-RepoPath -RelativePath "scripts\unity\audit_pc_controlled_demo_investor_evidence_refresh.ps1"
$pcControlledDemoInvestorEvidencePolishFixesScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_evidence_polish_fixes.ps1"
$pcControlledDemoInvestorRouteEvidenceRefreshScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_refresh.ps1"
$pcControlledDemoInvestorRouteEvidenceRefreshAuditScript = Resolve-RepoPath -RelativePath "scripts\unity\audit_pc_controlled_demo_investor_route_evidence_refresh.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixesScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fixes.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditScript = Resolve-RepoPath -RelativePath "scripts\unity\audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixesScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fixes.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditScript = Resolve-RepoPath -RelativePath "scripts\unity\audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixesScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditScript = Resolve-RepoPath -RelativePath "scripts\unity\audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditScript = Resolve-RepoPath -RelativePath "scripts\unity\audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditScript = Resolve-RepoPath -RelativePath "scripts\unity\audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditScript = Resolve-RepoPath -RelativePath "scripts\unity\audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1"
$pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshScript = Resolve-RepoPath -RelativePath "scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1"
$serverBackedReceiptSlicePlanScript = Resolve-RepoPath -RelativePath "scripts\unity\check_server_backed_receipt_slice_plan.ps1"
$serverBackedReceiptEvidenceScript = Resolve-RepoPath -RelativePath "scripts\unity\capture_server_backed_receipt_evidence.ps1"
$postReceiptInventoryRefreshBoundaryScript = Resolve-RepoPath -RelativePath "scripts\unity\check_post_receipt_inventory_refresh_boundary.ps1"
$postReceiptInventoryRefreshBindingScript = Resolve-RepoPath -RelativePath "scripts\unity\check_post_receipt_inventory_refresh_binding.ps1"
$postReceiptRefreshEvidenceScript = Resolve-RepoPath -RelativePath "scripts\unity\capture_post_receipt_refresh_evidence.ps1"
$androidDeviceConnectionScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_device_connection.ps1"
$androidAdbDriverPackageScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_adb_driver_package.ps1"
$androidDeviceWatchScript = Resolve-RepoPath -RelativePath "scripts\unity\watch_android_device_connection.ps1"
$androidG3DeviceStatusScript = Resolve-RepoPath -RelativePath "scripts\unity\write_android_g3_device_status.ps1"
$androidG3WhenReadyScript = Resolve-RepoPath -RelativePath "scripts\unity\run_android_g3_when_ready.ps1"
$androidPreflightScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_device_preflight.ps1"
$androidSmokeScript = Resolve-RepoPath -RelativePath "scripts\unity\android_device_smoke.ps1"
$androidSmokeConnectionGateScript = Resolve-RepoPath -RelativePath "scripts\unity\check_android_smoke_connection_gate.ps1"
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
    -Name "Mobile landscape contract gate" `
    -ScriptPath $mobileLandscapeContractScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Mobile landscape contract check OK.")

Invoke-GateStep `
    -Name "Landscape touch UI ergonomics gate" `
    -ScriptPath $landscapeTouchUiErgonomicsScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Landscape touch UI ergonomics check OK.")

Invoke-GateStep `
    -Name "Landscape MechLab touch controls gate" `
    -ScriptPath $landscapeMechLabTouchControlsScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Landscape MechLab touch controls check OK.")

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
    -Name "Current plan queue consistency gate" `
    -ScriptPath $currentPlanQueueScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Current plan queue consistency check OK.")

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
    -Name "Mobile performance budget gate" `
    -ScriptPath $mobilePerformanceBudgetScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Mobile performance budget check OK.")

Invoke-GateStep `
    -Name "iOS feasibility gate" `
    -ScriptPath $iosFeasibilityScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("iOS feasibility gate check OK.")

Invoke-GateStep `
    -Name "Map authoring contract gate" `
    -ScriptPath $mapAuthoringScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Map authoring contract check OK.")

Invoke-GateStep `
    -Name "Web ranking contract gate" `
    -ScriptPath $webRankingScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Web ranking contract check OK.")

Invoke-GateStep `
    -Name "Creator economy boundary gate" `
    -ScriptPath $creatorEconomyScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Creator economy boundary check OK.")

Invoke-GateStep `
    -Name "Server implementation boundary gate" `
    -ScriptPath $serverBoundaryScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Server implementation boundary check OK.")

Invoke-GateStep `
    -Name "Local main-server prototype gate" `
    -ScriptPath $localMainServerScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Local main-server prototype check OK.")

Invoke-GateStep `
    -Name "Unity main-server integration contract gate" `
    -ScriptPath $unityMainServerIntegrationScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Unity main-server integration contract check OK.")

Invoke-GateStep `
    -Name "Optional Unity main-server client adapter gate" `
    -ScriptPath $optionalUnityMainServerClientScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Optional Unity main-server client adapter check OK.")

Invoke-GateStep `
    -Name "Optional Unity main-server launch/debrief smoke gate" `
    -ScriptPath $optionalUnityMainServerLaunchDebriefScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Optional Unity main-server launch/debrief smoke check OK.")

Invoke-GateStep `
    -Name "Optional Unity inventory bootstrap smoke gate" `
    -ScriptPath $optionalUnityInventoryBootstrapScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Optional Unity inventory bootstrap smoke check OK.")

Invoke-GateStep `
    -Name "Inventory-to-MechBay binding boundary gate" `
    -ScriptPath $inventoryMechBayBindingScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Inventory-to-MechBay binding boundary check OK.")

Invoke-GateStep `
    -Name "Optional inventory-to-MechBay preview binding gate" `
    -ScriptPath $optionalInventoryMechBayPreviewScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Optional inventory-to-MechBay preview binding check OK.")

Invoke-GateStep `
    -Name "Inventory MechBay preview evidence capture gate" `
    -ScriptPath $inventoryMechBayPreviewEvidenceScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Inventory MechBay preview evidence capture OK.")

Invoke-GateStep `
    -Name "Landscape-phone MechLab source-line evidence capture gate" `
    -ScriptPath $landscapePhoneMechLabSourceLineScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Landscape-phone MechLab source-line evidence capture OK.")

Invoke-GateStep `
    -Name "Landscape MechLab touch evidence capture gate" `
    -ScriptPath $landscapeMechLabTouchEvidenceScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Landscape MechLab touch evidence capture OK.")

Invoke-GateStep `
    -Name "Android MechLab touch evidence plan gate" `
    -ScriptPath $androidMechLabTouchEvidenceScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("Android MechLab touch evidence capture plan OK.")

Invoke-GateStep `
    -Name "Android battle command touch evidence plan gate" `
    -ScriptPath $androidBattleCommandTouchEvidenceScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("Android battle command touch evidence capture plan OK.")

Invoke-GateStep `
    -Name "Android combat effect log noise gate" `
    -ScriptPath $androidCombatEffectLogNoiseScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Android combat effect log noise check OK.")

Invoke-GateStep `
    -Name "Android entity placeholder collision path gate" `
    -ScriptPath $androidEntityPlaceholderCollisionPathScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Android entity placeholder collision path check OK.")

Invoke-GateStep `
    -Name "Android entity placeholder collision runtime evidence plan gate" `
    -ScriptPath $androidEntityPlaceholderCollisionRuntimeEvidenceScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("Android entity placeholder collision runtime evidence capture plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo visual readability audit plan gate" `
    -ScriptPath $pcControlledDemoVisualReadabilityAuditScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo visual readability audit plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo visual readability fixes plan gate" `
    -ScriptPath $pcControlledDemoVisualReadabilityFixesScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo visual readability fixes plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo visual evidence refresh plan gate" `
    -ScriptPath $pcControlledDemoVisualEvidenceScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo visual evidence refresh plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo command readability formation audit plan gate" `
    -ScriptPath $pcControlledDemoCommandReadabilityAuditScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo command readability formation audit plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo command readability fixes plan gate" `
    -ScriptPath $pcControlledDemoCommandReadabilityFixesScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo command readability fixes plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo command evidence refresh plan gate" `
    -ScriptPath $pcControlledDemoCommandEvidenceScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo command evidence refresh plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo playable flow polish audit plan gate" `
    -ScriptPath $pcControlledDemoPlayableFlowAuditScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo playable flow polish audit plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo playable flow polish fixes plan gate" `
    -ScriptPath $pcControlledDemoPlayableFlowFixesScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo playable flow polish fixes plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor readiness audit plan gate" `
    -ScriptPath $pcControlledDemoInvestorReadinessAuditScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor readiness audit plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor readiness fixes plan gate" `
    -ScriptPath $pcControlledDemoInvestorReadinessFixesScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor readiness fixes plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor evidence package audit plan gate" `
    -ScriptPath $pcControlledDemoInvestorEvidencePackageAuditScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor evidence package audit plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor evidence package fixes plan gate" `
    -ScriptPath $pcControlledDemoInvestorEvidencePackageFixesScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor evidence package fixes plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor evidence refresh plan gate" `
    -ScriptPath $pcControlledDemoInvestorEvidenceRefreshScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor evidence refresh plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor evidence refresh audit plan gate" `
    -ScriptPath $pcControlledDemoInvestorEvidenceRefreshAuditScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor evidence refresh audit plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor evidence polish fixes plan gate" `
    -ScriptPath $pcControlledDemoInvestorEvidencePolishFixesScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor evidence polish fixes plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence refresh plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceRefreshScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence refresh plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence refresh audit plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceRefreshAuditScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence refresh audit plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fixes plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixesScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fixes plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fixes plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixesScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fixes plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixesScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fixes plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fixes plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes plan OK.")

Invoke-GateStep `
    -Name "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh plan gate" `
    -ScriptPath $pcControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @("PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh plan OK.")

Invoke-GateStep `
    -Name "Server-backed receipt slice plan gate" `
    -ScriptPath $serverBackedReceiptSlicePlanScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Server-backed receipt slice plan check OK.")

Invoke-GateStep `
    -Name "Server-backed receipt evidence capture gate" `
    -ScriptPath $serverBackedReceiptEvidenceScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Server-backed receipt evidence capture OK.")

Invoke-GateStep `
    -Name "Post-receipt inventory refresh boundary gate" `
    -ScriptPath $postReceiptInventoryRefreshBoundaryScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Post-receipt inventory refresh boundary check OK.")

Invoke-GateStep `
    -Name "Post-receipt inventory refresh binding gate" `
    -ScriptPath $postReceiptInventoryRefreshBindingScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Post-receipt inventory refresh binding check OK.")

Invoke-GateStep `
    -Name "Post-receipt refresh evidence capture gate" `
    -ScriptPath $postReceiptRefreshEvidenceScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("Post-receipt refresh evidence capture OK.")

Invoke-GateStep `
    -Name "Android device connection gate" `
    -ScriptPath $androidDeviceConnectionScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("WpdOnlyAndroidProbe: True", "AdbSetupHint: True") `
    -AnySuccessMarkers @(
        "Android device connection check OK.",
        "Android device connection check waiting on device.",
        "Android device connection check waiting on authorization.",
        "Android device connection check waiting on online device.",
        "Android device connection check waiting on device selection."
    )

Invoke-GateStep `
    -Name "Android ADB driver package gate" `
    -ScriptPath $androidAdbDriverPackageScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @(
        "Android ADB driver package probe OK.",
        "AdbDriverPackageProbe: True",
        "NoInstallOrLaunch: True",
        "NextGate: G3 Run Android device smoke"
    )

Invoke-GateStep `
    -Name "Android device watch gate" `
    -ScriptPath $androidDeviceWatchScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-Once", "-AllowWaiting") `
    -RequiredMarkers @("AdbWatchHint: True") `
    -AnySuccessMarkers @(
        "Android device connection watch OK.",
        "Android device connection watch waiting on device."
    )

Invoke-GateStep `
    -Name "Android G3 device status report gate" `
    -ScriptPath $androidG3DeviceStatusScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @("G3DeviceStatusReport: True", "NoInstallOrLaunch: True", "NextGate: G3 Run Android device smoke") `
    -AnySuccessMarkers @(
        "G3DeviceReady: True",
        "G3DeviceReady: False"
    )

Invoke-GateStep `
    -Name "Android G3 when-ready plan gate" `
    -ScriptPath $androidG3WhenReadyScript `
    -Arguments @("-RepoRoot", $RepoRoot, "-PlanOnly") `
    -RequiredMarkers @(
        "Android G3 when-ready plan OK.",
        "G3WhenReady: True",
        "NoInstallOrLaunchUntilDeviceReady: True",
        "NextGate: G3 Run Android device smoke"
    )

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
        "LandscapeScreenshot: True",
        "Summary:",
        "SummaryWrite: True",
        "CommandFileSmoke: True",
        "UnityArguments: -mc2CommandFile",
        "SmokeSuccessMarker: MC2 debrief summary assertion OK",
        "SmokeSuccessMarker: MC2 loadout compact assertion OK",
        "ConnectionCheck: check_android_device_connection.ps1 -RequireDevice"
    )

Invoke-GateStep `
    -Name "Android smoke connection gate" `
    -ScriptPath $androidSmokeConnectionGateScript `
    -Arguments @("-RepoRoot", $RepoRoot) `
    -RequiredMarkers @(
        "Android smoke connection gate check OK."
    ) `
    -AnySuccessMarkers @(
        "Android smoke connection gate check ready for G3 device smoke.",
        "Android smoke connection gate check waiting on device.",
        "Android smoke connection gate check waiting on authorization.",
        "Android smoke connection gate check waiting on online device.",
        "Android smoke connection gate check waiting on device selection."
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
        "ConnectionCheck: check_android_device_connection.ps1 -RequireDevice",
        "CommandFileSmoke: True",
        "SmokeSuccessMarker: MC2 debrief summary assertion OK",
        "SmokeSuccessMarker: MC2 loadout compact assertion OK",
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
