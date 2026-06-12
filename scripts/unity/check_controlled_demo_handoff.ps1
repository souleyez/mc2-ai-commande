param(
    [string]$RepoRoot = "",
    [switch]$RunReadiness
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

function Assert-FileExists {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path)) {
        Add-Failure "Missing file: $RelativePath"
        return $null
    }

    return $path
}

function Assert-FileContains {
    param(
        [string]$RelativePath,
        [string[]]$Markers
    )

    $path = Assert-FileExists -RelativePath $RelativePath
    if ($null -eq $path) {
        return
    }

    $text = Get-Content -LiteralPath $path -Raw
    $missing = New-Object System.Collections.Generic.List[string]
    foreach ($marker in $Markers) {
        if (-not $text.Contains($marker)) {
            Add-Failure "$RelativePath missing marker: $marker"
            [void]$missing.Add($marker)
        }
    }

    if ($missing.Count -eq 0) {
        [void]$rows.Add([pscustomobject]@{
            Check = $RelativePath
            Status = "OK"
            Detail = "$($Markers.Count) marker(s)"
        })
    }
}

function Assert-FileDoesNotContain {
    param(
        [string]$RelativePath,
        [string[]]$ForbiddenMarkers
    )

    $path = Assert-FileExists -RelativePath $RelativePath
    if ($null -eq $path) {
        return
    }

    $text = Get-Content -LiteralPath $path -Raw
    $found = New-Object System.Collections.Generic.List[string]
    foreach ($marker in $ForbiddenMarkers) {
        if ($text.Contains($marker)) {
            Add-Failure "$RelativePath still contains stale marker: $marker"
            [void]$found.Add($marker)
        }
    }

    if ($found.Count -eq 0) {
        [void]$rows.Add([pscustomobject]@{
            Check = "$RelativePath stale markers"
            Status = "OK"
            Detail = "$($ForbiddenMarkers.Count) forbidden marker(s)"
        })
    }
}

function Assert-ScriptExists {
    param([string]$RelativePath)

    $path = Assert-FileExists -RelativePath $RelativePath
    if ($null -ne $path) {
        [void]$rows.Add([pscustomobject]@{
            Check = $RelativePath
            Status = "OK"
            Detail = "script exists"
        })
    }
}

Assert-ScriptExists -RelativePath "scripts\unity\run_windows_demo.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_windows_demo_build_freshness.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_controlled_demo_evidence.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_controlled_demo_readiness.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_demo_source_hygiene.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_smoke_artifact_hygiene.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_ai_deputy_contract.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_device_preflight.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_sdk_tooling.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_apk_freshness.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_apk_identity.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_apk_compatibility.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_apk_signing.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_apk_manifest.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_apk_payload.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_apk_size_budget.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_device_connection.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\watch_android_device_connection.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\write_android_g3_device_status.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\run_android_g3_when_ready.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_smoke_connection_gate.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\android_device_smoke.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_smoke_plan_consistency.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_g3_readiness.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_g3_device_requirement.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_pc_core_playable_contract.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_mobile_command_model_preflight.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_battle_hud_sparse_contract.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_pc_visual_capture_sanity.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_pc_capture_sidecar_schema.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_pc_capture_preset_contract.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_pc_capture_artifact_hygiene.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_pc_window_contract.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_pc_launch_log_hygiene.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_pc_build_artifact_hygiene.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_pc_smoke_artifact_hygiene.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_current_plan_queue.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_current_plan_gate.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_smoke_log.ps1"
Assert-ScriptExists -RelativePath "scripts\unity\check_android_smoke_summary.ps1"
Assert-ScriptExists -RelativePath "scripts\content-pack\check_controlled_demo_public_boundary.ps1"

Assert-FileContains -RelativePath "README.md" -Markers @(
    "AI RTS Commander Lab",
    "PC1-PC56",
    "check_controlled_demo_handoff.ps1",
    "check_windows_demo_build_freshness.ps1",
    "check_demo_source_hygiene.ps1",
    "check_android_smoke_artifact_hygiene.ps1",
    "check_ai_deputy_contract.ps1",
    "check_android_device_preflight.ps1",
    "check_android_sdk_tooling.ps1",
    "check_android_apk_compatibility.ps1",
    "check_android_apk_signing.ps1",
    "check_android_apk_manifest.ps1",
    "check_android_apk_payload.ps1",
    "check_android_apk_size_budget.ps1",
    "check_android_smoke_artifact_hygiene.ps1",
    "android_device_smoke.ps1",
    "android-device-smoke.png",
    "android-device-smoke-summary.json",
    "check_pc_core_playable_contract.ps1",
    "check_mobile_command_model_preflight.ps1",
    "check_battle_hud_sparse_contract.ps1",
    "check_pc_visual_capture_sanity.ps1",
    "PC visual capture sanity check",
    "PC visual capture sanity self-test OK",
    "check_pc_capture_sidecar_schema.ps1",
    "PC capture sidecar schema check OK",
    "check_pc_capture_preset_contract.ps1",
    "PC capture preset contract check OK",
    "check_pc_capture_artifact_hygiene.ps1",
    "PC capture artifact hygiene check OK",
    "check_pc_window_contract.ps1",
    "PC window contract check OK",
    "check_pc_launch_log_hygiene.ps1",
    "PC launch log hygiene check OK",
    "check_pc_build_artifact_hygiene.ps1",
    "PC build artifact hygiene check OK",
    "check_pc_smoke_artifact_hygiene.ps1",
    "PC smoke artifact hygiene check OK",
    "check_current_plan_queue.ps1",
    "Current plan queue consistency check OK",
    "check_android_device_connection.ps1",
    "Android device connection check waiting on device",
    "WpdOnlyAndroidProbe: True",
    "AdbSetupHint: True",
    "AdbWatchHint: True",
    "G3DeviceStatusReport: True",
    "G3WhenReady: True",
    "NoInstallOrLaunchUntilDeviceReady: True",
    "Wire Android smoke connection gate",
    "ConnectionCheck: check_android_device_connection.ps1 -RequireDevice",
    "Add Android smoke connection gate check",
    "check_android_smoke_connection_gate.ps1",
    "Android smoke connection gate check OK",
    "Android smoke connection gate check waiting on device",
    "Add Android visible-flow command-file smoke",
    "Add Android WPD-only device diagnosis",
    "Add Android ADB setup guidance",
    "Add Android ADB readiness watch",
    "Add Android G3 when-ready runner",
    "CommandFileSmoke: True",
    "UnityArguments: -mc2CommandFile",
    "SmokeSuccessMarker: MC2 debrief summary assertion OK",
    "SmokeSuccessMarker: MC2 loadout compact assertion OK",
    "check_current_plan_gate.ps1",
    "check_android_smoke_log.ps1",
    "check_android_smoke_summary.ps1",
    "check_android_smoke_plan_consistency.ps1",
    "check_android_g3_readiness.ps1",
    "check_android_g3_device_requirement.ps1",
    "Android smoke summary check self-test OK",
    "Android smoke plan/preflight consistency check OK",
    "Android G3 readiness check",
    "Android G3 device requirement check",
    "check_controlled_demo_readiness.ps1"
)

Assert-FileContains -RelativePath "BUILD-WIN.md" -Markers @(
    "Current Unity 6 Windows Demo handoff",
    "check_windows_demo_build_freshness.ps1",
    "Windows demo build freshness check OK",
    "check_pc_core_playable_contract.ps1",
    "PC core playable contract check OK",
    "check_mobile_command_model_preflight.ps1",
    "Mobile command model preflight OK",
    "check_battle_hud_sparse_contract.ps1",
    "Battle HUD sparse contract check OK",
    "check_pc_visual_capture_sanity.ps1",
    "PC visual capture sanity check OK",
    "PC visual capture sanity self-test OK",
    "check_pc_capture_sidecar_schema.ps1",
    "PC capture sidecar schema check OK",
    "check_pc_capture_preset_contract.ps1",
    "PC capture preset contract check OK",
    "check_pc_capture_artifact_hygiene.ps1",
    "PC capture artifact hygiene check OK",
    "check_pc_window_contract.ps1",
    "PC window contract check OK",
    "check_pc_launch_log_hygiene.ps1",
    "PC launch log hygiene check OK",
    "check_pc_build_artifact_hygiene.ps1",
    "PC build artifact hygiene check OK",
    "check_pc_smoke_artifact_hygiene.ps1",
    "PC smoke artifact hygiene check OK",
    "check_current_plan_queue.ps1",
    "Current plan queue consistency check OK",
    "check_android_device_connection.ps1",
    "watch_android_device_connection.ps1",
    "write_android_g3_device_status.ps1",
    "run_android_g3_when_ready.ps1",
    "Android device connection check waiting on device",
    "WpdOnlyAndroidProbe: True",
    "AdbSetupHint: True",
    "AdbWatchHint: True",
    "G3DeviceStatusReport: True",
    "G3WhenReady: True",
    "NoInstallOrLaunchUntilDeviceReady: True",
    "check_android_smoke_connection_gate.ps1",
    "Android smoke connection gate check OK",
    "Android smoke connection gate check waiting on device",
    "check_current_plan_gate.ps1",
    "Current plan gate check OK",
    "check_android_smoke_log.ps1",
    "Android smoke log check self-test OK",
    "check_android_smoke_summary.ps1",
    "Android smoke summary check self-test OK",
    "check_android_smoke_plan_consistency.ps1",
    "Android smoke plan/preflight consistency check OK",
    "check_android_g3_readiness.ps1",
    "Android G3 readiness check",
    "check_android_g3_device_requirement.ps1",
    "Android G3 device requirement check",
    "smoke summary schema",
    "android_device_smoke.ps1",
    "Android device smoke plan OK",
    "android-device-smoke.png",
    "ScreenshotCapture: True",
    "android-device-smoke-summary.json",
    "SummaryWrite: True",
    "ConnectionCheck: check_android_device_connection.ps1 -RequireDevice",
    "WpdOnlyAndroidProbe: True",
    "AdbSetupHint: True",
    "AdbWatchHint: True",
    "G3DeviceStatusReport: True",
    "G3WhenReady: True",
    "NoInstallOrLaunchUntilDeviceReady: True",
    "CommandFileSmoke: True",
    "UnityArguments: -mc2CommandFile",
    "SmokeSuccessMarker: MC2 debrief summary assertion OK",
    "SmokeSuccessMarker: MC2 loadout compact assertion OK",
    "Android SDK tooling",
    "Android APK freshness",
    "Android APK identity",
    "Android APK compatibility",
    "Android APK signing",
    "Android APK manifest",
    "Android APK payload",
    "Android APK size budget",
    "Android smoke artifact hygiene",
    "check_android_smoke_artifact_hygiene.ps1",
    "Android smoke artifact hygiene check OK",
    "check_controlled_demo_handoff.ps1",
    "Controlled demo handoff consistency check OK",
    "check_demo_source_hygiene.ps1",
    "Demo source hygiene check OK",
    "check_ai_deputy_contract.ps1",
    "AI deputy contract check OK",
    "Controlled demo readiness preflight OK"
)

Assert-FileContains -RelativePath "BUILD-MOBILE.md" -Markers @(
    "check_android_device_preflight.ps1",
    "check_android_apk_freshness.ps1",
    "check_android_apk_identity.ps1",
    "check_android_apk_compatibility.ps1",
    "check_android_apk_signing.ps1",
    "check_android_apk_manifest.ps1",
    "check_android_apk_payload.ps1",
    "check_android_apk_size_budget.ps1",
    "check_android_sdk_tooling.ps1",
    "check_android_smoke_artifact_hygiene.ps1",
    "Android smoke artifact hygiene check OK",
    "check_android_smoke_summary.ps1",
    "Android smoke summary check self-test OK",
    "summary schema",
    "Android APK freshness check OK",
    "Android APK identity check OK",
    "Android APK compatibility check OK",
    "Android APK signing check OK",
    "Android APK manifest check OK",
    "Android APK payload check OK",
    "Android APK size budget check OK",
    "Android SDK tooling check OK",
    "Android smoke artifact hygiene check OK",
    "android-device-smoke.png",
    "ScreenshotCapture: True",
    "android-device-smoke-summary.json",
    "SummaryWrite: True",
    "ConnectionCheck: check_android_device_connection.ps1 -RequireDevice",
    "WpdOnlyAndroidProbe: True",
    "AdbSetupHint: True",
    "AdbWatchHint: True",
    "G3DeviceStatusReport: True",
    "G3WhenReady: True",
    "NoInstallOrLaunchUntilDeviceReady: True",
    "CommandFileSmoke: True",
    "UnityArguments: -mc2CommandFile",
    "SmokeSuccessMarker: MC2 debrief summary assertion OK",
    "SmokeSuccessMarker: MC2 loadout compact assertion OK",
    "check_android_smoke_connection_gate.ps1",
    "Android smoke connection gate check OK",
    "Android smoke connection gate check waiting on device",
    "Android device smoke preflight waiting on device",
    "check_android_smoke_plan_consistency.ps1",
    "Android smoke plan/preflight consistency check OK",
    "check_android_g3_readiness.ps1",
    "Android G3 readiness check",
    "check_android_g3_device_requirement.ps1",
    "Android G3 device requirement check",
    "android_device_smoke.ps1"
)

Assert-FileContains -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md" -Markers @(
    "PC1-PC56",
    "Add controlled demo handoff consistency check",
    "Add demo source hygiene check",
    "Add AI deputy contract check",
    "Add Windows demo build freshness check",
    "Add controlled demo evidence freshness check",
    "Add controlled demo capture log freshness check",
    "Add Android APK freshness check",
    "Add Android APK identity check",
    "Add Android APK compatibility check",
    "Add Android APK signing check",
    "Add Android APK manifest check",
    "Add Android APK payload check",
    "Add Android APK size budget check",
    "Add Android SDK tooling check",
    "Add Android smoke artifact hygiene check",
    "Add Android smoke screenshot evidence capture",
    "Add Android smoke summary evidence output",
    "Add Android smoke summary schema check",
    "Add Android smoke summary preflight check",
    "Add Android device smoke preflight",
    "Add PC core playable contract check",
    "Add mobile command model preflight",
    "Add battle HUD sparse contract check",
    "Add PC visual capture sanity check",
    "Add PC visual capture sanity self-test",
    "Add PC capture sidecar schema check",
    "Add PC capture preset contract check",
    "Add PC capture artifact hygiene check",
    "Add PC window contract check",
    "Add PC launch log hygiene check",
    "Add PC build artifact hygiene check",
    "Add PC smoke artifact hygiene check",
    "Add current plan queue consistency check",
    "Add Android device connection check",
    "Wire Android smoke connection gate",
    "Add Android smoke connection gate check",
    "Add Android visible-flow command-file smoke",
    "Add Android WPD-only device diagnosis",
    "Add Android ADB setup guidance",
    "Add Android ADB readiness watch",
    "Add Android G3 when-ready runner",
    "Add current plan gate check",
    "Add Android smoke log crash scan",
    "Add Android smoke plan mode",
    "Add Android smoke plan/preflight consistency check",
    "Add Android G3 readiness check",
    "Add Android G3 device requirement check",
    "check_controlled_demo_handoff.ps1",
    "check_windows_demo_build_freshness.ps1",
    "check_demo_source_hygiene.ps1",
    "check_ai_deputy_contract.ps1",
    "check_pc_core_playable_contract.ps1",
    "check_mobile_command_model_preflight.ps1",
    "check_battle_hud_sparse_contract.ps1",
    "check_pc_visual_capture_sanity.ps1",
    "check_pc_capture_sidecar_schema.ps1",
    "check_pc_capture_preset_contract.ps1",
    "check_pc_capture_artifact_hygiene.ps1",
    "check_pc_window_contract.ps1",
    "check_pc_launch_log_hygiene.ps1",
    "check_pc_build_artifact_hygiene.ps1",
    "check_pc_smoke_artifact_hygiene.ps1",
    "check_current_plan_queue.ps1",
    "check_android_device_connection.ps1",
    "watch_android_device_connection.ps1",
    "write_android_g3_device_status.ps1",
    "run_android_g3_when_ready.ps1",
    "check_android_smoke_connection_gate.ps1",
    "check_current_plan_gate.ps1",
    "check_android_apk_freshness.ps1",
    "check_android_apk_identity.ps1",
    "check_android_apk_compatibility.ps1",
    "check_android_apk_signing.ps1",
    "check_android_apk_manifest.ps1",
    "check_android_apk_payload.ps1",
    "check_android_apk_size_budget.ps1",
    "check_android_sdk_tooling.ps1",
    "check_android_smoke_artifact_hygiene.ps1",
    "android-device-smoke.png",
    "ScreenshotCapture: True",
    "android-device-smoke-summary.json",
    "SummaryWrite: True",
    "ConnectionCheck: check_android_device_connection.ps1 -RequireDevice",
    "WpdOnlyAndroidProbe: True",
    "AdbSetupHint: True",
    "AdbWatchHint: True",
    "G3DeviceStatusReport: True",
    "G3WhenReady: True",
    "NoInstallOrLaunchUntilDeviceReady: True",
    "CommandFileSmoke: True",
    "UnityArguments: -mc2CommandFile",
    "SmokeSuccessMarker: MC2 debrief summary assertion OK",
    "SmokeSuccessMarker: MC2 loadout compact assertion OK",
    "Android smoke connection gate check OK",
    "Android smoke connection gate check waiting on device",
    "check_android_smoke_log.ps1",
    "check_android_smoke_summary.ps1",
    "android_device_smoke.ps1",
    "check_android_smoke_plan_consistency.ps1",
    "check_android_g3_readiness.ps1",
    "check_android_g3_device_requirement.ps1"
)

Assert-FileContains -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md" -Markers @(
    "PC1-PC56",
    "Add controlled demo handoff consistency check",
    "Add demo source hygiene check",
    "Add AI deputy contract check",
    "Add Windows demo build freshness check",
    "Add controlled demo evidence freshness check",
    "Add controlled demo capture log freshness check",
    "Add Android APK freshness check",
    "Add Android APK identity check",
    "Add Android APK compatibility check",
    "Add Android APK signing check",
    "Add Android APK manifest check",
    "Add Android APK payload check",
    "Add Android APK size budget check",
    "Add Android SDK tooling check",
    "Add Android smoke artifact hygiene check",
    "Add Android smoke screenshot evidence capture",
    "Add Android smoke summary evidence output",
    "Add Android smoke summary schema check",
    "Add Android smoke summary preflight check",
    "Add Android device smoke preflight",
    "Add PC core playable contract check",
    "Add mobile command model preflight",
    "Add battle HUD sparse contract check",
    "Add PC visual capture sanity check",
    "Add PC visual capture sanity self-test",
    "Add PC capture sidecar schema check",
    "Add PC capture preset contract check",
    "Add PC capture artifact hygiene check",
    "Add PC window contract check",
    "Add PC launch log hygiene check",
    "Add PC build artifact hygiene check",
    "Add PC smoke artifact hygiene check",
    "Add current plan queue consistency check",
    "Add Android device connection check",
    "Wire Android smoke connection gate",
    "Add Android smoke connection gate check",
    "Add Android visible-flow command-file smoke",
    "Add Android WPD-only device diagnosis",
    "Add Android ADB setup guidance",
    "Add Android ADB readiness watch",
    "Add Android G3 when-ready runner",
    "Add current plan gate check",
    "Add Android smoke log crash scan",
    "Add Android smoke plan mode",
    "Add Android smoke plan/preflight consistency check",
    "Add Android G3 readiness check",
    "Add Android G3 device requirement check",
    "check_controlled_demo_handoff.ps1",
    "check_windows_demo_build_freshness.ps1",
    "check_demo_source_hygiene.ps1",
    "check_ai_deputy_contract.ps1",
    "check_pc_core_playable_contract.ps1",
    "check_mobile_command_model_preflight.ps1",
    "check_battle_hud_sparse_contract.ps1",
    "check_pc_visual_capture_sanity.ps1",
    "check_pc_capture_sidecar_schema.ps1",
    "check_pc_capture_preset_contract.ps1",
    "check_pc_capture_artifact_hygiene.ps1",
    "check_pc_window_contract.ps1",
    "check_pc_launch_log_hygiene.ps1",
    "check_pc_build_artifact_hygiene.ps1",
    "check_pc_smoke_artifact_hygiene.ps1",
    "check_current_plan_queue.ps1",
    "check_android_device_connection.ps1",
    "check_android_smoke_connection_gate.ps1",
    "check_current_plan_gate.ps1",
    "check_android_apk_freshness.ps1",
    "check_android_apk_identity.ps1",
    "check_android_apk_compatibility.ps1",
    "check_android_apk_signing.ps1",
    "check_android_apk_manifest.ps1",
    "check_android_apk_payload.ps1",
    "check_android_apk_size_budget.ps1",
    "check_android_sdk_tooling.ps1",
    "check_android_smoke_artifact_hygiene.ps1",
    "android-device-smoke.png",
    "ScreenshotCapture: True",
    "android-device-smoke-summary.json",
    "SummaryWrite: True",
    "ConnectionCheck: check_android_device_connection.ps1 -RequireDevice",
    "CommandFileSmoke: True",
    "UnityArguments: -mc2CommandFile",
    "SmokeSuccessMarker: MC2 debrief summary assertion OK",
    "SmokeSuccessMarker: MC2 loadout compact assertion OK",
    "Android smoke connection gate check OK",
    "Android smoke connection gate check waiting on device",
    "check_android_smoke_log.ps1",
    "check_android_smoke_summary.ps1",
    "android_device_smoke.ps1",
    "check_android_smoke_plan_consistency.ps1",
    "check_android_g3_readiness.ps1",
    "check_android_g3_device_requirement.ps1"
)

Assert-FileContains -RelativePath "docs-pc-optimization-plan-2026-06-11.md" -Markers @(
    "sealed through PC1-PC56",
    "Add controlled demo handoff consistency check",
    "Add demo source hygiene check",
    "Add AI deputy contract check",
    "Add Windows demo build freshness check",
    "Add controlled demo evidence freshness check",
    "Add controlled demo capture log freshness check",
    "Add Android APK freshness check",
    "Add Android APK identity check",
    "Add Android APK compatibility check",
    "Add Android APK signing check",
    "Add Android APK manifest check",
    "Add Android APK payload check",
    "Add Android APK size budget check",
    "Add Android SDK tooling check",
    "Add Android smoke artifact hygiene check",
    "Add Android smoke screenshot evidence capture",
    "Add Android smoke summary evidence output",
    "Add Android smoke summary schema check",
    "Add Android smoke summary preflight check",
    "Add Android device smoke preflight",
    "Add PC core playable contract check",
    "Add mobile command model preflight",
    "Add battle HUD sparse contract check",
    "Add PC visual capture sanity check",
    "Add PC visual capture sanity self-test",
    "Add PC capture sidecar schema check",
    "Add PC capture preset contract check",
    "Add PC capture artifact hygiene check",
    "Add PC window contract check",
    "Add PC launch log hygiene check",
    "Add PC build artifact hygiene check",
    "Add PC smoke artifact hygiene check",
    "Add current plan queue consistency check",
    "Add Android device connection check",
    "Wire Android smoke connection gate",
    "Add Android smoke connection gate check",
    "Add Android visible-flow command-file smoke",
    "Add Android WPD-only device diagnosis",
    "Add Android ADB setup guidance",
    "Add Android ADB readiness watch",
    "Add Android G3 when-ready runner",
    "Add current plan gate check",
    "Add Android smoke log crash scan",
    "Add Android smoke plan mode",
    "Add Android smoke plan/preflight consistency check",
    "Add Android G3 readiness check",
    "Add Android G3 device requirement check",
    "check_controlled_demo_handoff.ps1",
    "check_windows_demo_build_freshness.ps1",
    "check_demo_source_hygiene.ps1",
    "check_ai_deputy_contract.ps1",
    "check_pc_visual_capture_sanity.ps1",
    "check_pc_capture_sidecar_schema.ps1",
    "check_pc_capture_preset_contract.ps1",
    "check_pc_capture_artifact_hygiene.ps1",
    "check_pc_window_contract.ps1",
    "check_pc_launch_log_hygiene.ps1",
    "check_pc_build_artifact_hygiene.ps1",
    "check_pc_smoke_artifact_hygiene.ps1",
    "check_current_plan_queue.ps1",
    "check_android_device_connection.ps1",
    "check_android_smoke_connection_gate.ps1",
    "check_android_apk_freshness.ps1",
    "check_android_apk_identity.ps1",
    "check_android_apk_compatibility.ps1",
    "check_android_apk_signing.ps1",
    "check_android_apk_manifest.ps1",
    "check_android_apk_payload.ps1",
    "check_android_apk_size_budget.ps1",
    "check_android_sdk_tooling.ps1",
    "check_android_smoke_artifact_hygiene.ps1",
    "android-device-smoke.png",
    "ScreenshotCapture: True",
    "android-device-smoke-summary.json",
    "SummaryWrite: True",
    "ConnectionCheck: check_android_device_connection.ps1 -RequireDevice",
    "CommandFileSmoke: True",
    "UnityArguments: -mc2CommandFile",
    "SmokeSuccessMarker: MC2 debrief summary assertion OK",
    "SmokeSuccessMarker: MC2 loadout compact assertion OK",
    "Android smoke connection gate check OK",
    "Android smoke connection gate check waiting on device",
    "check_android_smoke_summary.ps1",
    "check_battle_hud_sparse_contract.ps1",
    "check_android_smoke_plan_consistency.ps1",
    "check_android_g3_readiness.ps1",
    "check_android_g3_device_requirement.ps1"
)

Assert-FileContains -RelativePath "docs-playable-demo-investor-evidence-2026-06-07.md" -Markers @(
    "Demo source hygiene",
    "Windows build freshness",
    "Evidence freshness",
    "Capture log freshness",
    "Android APK freshness",
    "Android APK identity",
    "Android APK compatibility",
    "Android APK signing",
    "Android APK manifest",
    "Android APK payload",
    "Android APK size budget",
    "Android SDK tooling",
    "Android smoke artifact hygiene",
    "Android smoke screenshot evidence",
    "Android smoke summary evidence",
    "Android smoke summary schema",
    "Android smoke summary preflight",
    "AI deputy contract",
    "PC core playable contract",
    "Mobile command model preflight",
    "Battle HUD sparse contract",
    "PC visual capture sanity",
    "PC visual capture sanity self-test",
    "PC capture sidecar schema",
    "PC capture preset contract",
    "PC capture artifact hygiene",
    "PC window contract",
    "PC launch log hygiene",
    "PC build artifact hygiene",
    "PC smoke artifact hygiene",
    "Current plan queue consistency",
    "Android device connection",
    "Android WPD-only device diagnosis",
    "Android ADB setup guidance",
    "Android ADB readiness watch",
    "Android G3 device status report",
    "Android smoke connection gate check",
    "Android visible-flow command-file smoke",
    "Current plan gate",
    "Android smoke log check",
    "Android smoke plan",
    "Android smoke plan/preflight consistency",
    "Android G3 readiness",
    "Android G3 device requirement",
    "Readiness preflight",
    "Handoff consistency",
    "check_demo_source_hygiene.ps1",
    "check_windows_demo_build_freshness.ps1",
    "check_ai_deputy_contract.ps1",
    "check_pc_core_playable_contract.ps1",
    "check_mobile_command_model_preflight.ps1",
    "check_battle_hud_sparse_contract.ps1",
    "check_pc_visual_capture_sanity.ps1",
    "check_pc_capture_sidecar_schema.ps1",
    "check_pc_capture_preset_contract.ps1",
    "check_pc_capture_artifact_hygiene.ps1",
    "check_pc_window_contract.ps1",
    "check_pc_launch_log_hygiene.ps1",
    "check_pc_build_artifact_hygiene.ps1",
    "check_pc_smoke_artifact_hygiene.ps1",
    "check_current_plan_queue.ps1",
    "check_android_device_connection.ps1",
    "watch_android_device_connection.ps1",
    "write_android_g3_device_status.ps1",
    "run_android_g3_when_ready.ps1",
    "check_android_smoke_connection_gate.ps1",
    "check_current_plan_gate.ps1",
    "check_android_smoke_log.ps1",
    "check_android_smoke_summary.ps1",
    "android_device_smoke.ps1",
    "check_android_smoke_plan_consistency.ps1",
    "check_android_g3_readiness.ps1",
    "check_android_g3_device_requirement.ps1",
    "android-device-smoke.png",
    "ScreenshotCapture: True",
    "android-device-smoke-summary.json",
    "SummaryWrite: True",
    "ConnectionCheck: check_android_device_connection.ps1 -RequireDevice",
    "WpdOnlyAndroidProbe: True",
    "AdbSetupHint: True",
    "AdbWatchHint: True",
    "G3DeviceStatusReport: True",
    "G3WhenReady: True",
    "NoInstallOrLaunchUntilDeviceReady: True",
    "CommandFileSmoke: True",
    "UnityArguments: -mc2CommandFile",
    "SmokeSuccessMarker: MC2 debrief summary assertion OK",
    "SmokeSuccessMarker: MC2 loadout compact assertion OK",
    "Android smoke connection gate check OK",
    "Android smoke connection gate check waiting on device",
    "check_android_apk_freshness.ps1",
    "check_android_apk_identity.ps1",
    "check_android_apk_compatibility.ps1",
    "check_android_apk_signing.ps1",
    "check_android_apk_manifest.ps1",
    "check_android_apk_payload.ps1",
    "check_android_apk_size_budget.ps1",
    "check_android_sdk_tooling.ps1",
    "check_android_smoke_artifact_hygiene.ps1",
    "check_controlled_demo_handoff.ps1"
)

Assert-FileContains -RelativePath "docs-machine-handoff-plan-2026-06-07.md" -Markers @(
    "PC1-PC56",
    "Add PC visual capture sanity check",
    "Add PC visual capture sanity self-test",
    "Add PC capture sidecar schema check",
    "Add PC capture preset contract check",
    "Add PC capture artifact hygiene check",
    "Add PC window contract check",
    "Add PC launch log hygiene check",
    "Add PC build artifact hygiene check",
    "Add PC smoke artifact hygiene check",
    "Add current plan queue consistency check",
    "Add Android device connection check",
    "Wire Android smoke connection gate",
    "Add Android smoke connection gate check",
    "Add Android visible-flow command-file smoke",
    "Add Android WPD-only device diagnosis",
    "Add Android ADB setup guidance",
    "Add Android ADB readiness watch",
    "Add Android G3 when-ready runner",
    "Add Android smoke summary evidence output",
    "Add Android smoke summary schema check",
    "Add Android smoke summary preflight check",
    "Add Android smoke plan/preflight consistency check",
    "Add Android G3 readiness check",
    "Add Android G3 device requirement check",
    "Add Android smoke screenshot evidence capture",
    "Add Android smoke artifact hygiene check",
    "Add Android SDK tooling check",
    "Add Android APK size budget check",
    "Add Android APK payload check",
    "Add Android APK manifest check",
    "Add Android APK signing check",
    "Add Android APK compatibility check",
    "Add Android APK identity check",
    "Add Android APK freshness check",
    "Add controlled demo capture log freshness check",
    "G3 Run Android device smoke",
    "check_windows_demo_build_freshness.ps1",
    "check_demo_source_hygiene.ps1",
    "check_ai_deputy_contract.ps1",
    "check_pc_core_playable_contract.ps1",
    "check_mobile_command_model_preflight.ps1",
    "check_battle_hud_sparse_contract.ps1",
    "check_pc_visual_capture_sanity.ps1",
    "check_pc_capture_sidecar_schema.ps1",
    "check_pc_capture_preset_contract.ps1",
    "check_pc_capture_artifact_hygiene.ps1",
    "check_pc_window_contract.ps1",
    "check_pc_launch_log_hygiene.ps1",
    "check_pc_build_artifact_hygiene.ps1",
    "check_pc_smoke_artifact_hygiene.ps1",
    "check_current_plan_queue.ps1",
    "check_android_device_connection.ps1",
    "watch_android_device_connection.ps1",
    "write_android_g3_device_status.ps1",
    "run_android_g3_when_ready.ps1",
    "check_android_smoke_connection_gate.ps1",
    "check_current_plan_gate.ps1",
    "check_android_smoke_log.ps1",
    "check_android_smoke_summary.ps1",
    "android_device_smoke.ps1",
    "check_android_smoke_plan_consistency.ps1",
    "check_android_g3_readiness.ps1",
    "check_android_g3_device_requirement.ps1",
    "android-device-smoke.png",
    "ScreenshotCapture: True",
    "android-device-smoke-summary.json",
    "SummaryWrite: True",
    "ConnectionCheck: check_android_device_connection.ps1 -RequireDevice",
    "WpdOnlyAndroidProbe: True",
    "AdbSetupHint: True",
    "AdbWatchHint: True",
    "G3DeviceStatusReport: True",
    "G3WhenReady: True",
    "NoInstallOrLaunchUntilDeviceReady: True",
    "CommandFileSmoke: True",
    "UnityArguments: -mc2CommandFile",
    "SmokeSuccessMarker: MC2 debrief summary assertion OK",
    "SmokeSuccessMarker: MC2 loadout compact assertion OK",
    "Android smoke connection gate check OK",
    "Android smoke connection gate check waiting on device",
    "check_android_apk_freshness.ps1",
    "check_android_apk_identity.ps1",
    "check_android_apk_compatibility.ps1",
    "check_android_apk_signing.ps1",
    "check_android_apk_manifest.ps1",
    "check_android_apk_payload.ps1",
    "check_android_apk_size_budget.ps1",
    "check_android_sdk_tooling.ps1",
    "check_android_smoke_artifact_hygiene.ps1",
    "check_android_device_preflight.ps1",
    "check_controlled_demo_handoff.ps1",
    "check_controlled_demo_readiness.ps1"
)

Assert-FileDoesNotContain -RelativePath "docs-machine-handoff-plan-2026-06-07.md" -ForbiddenMarkers @(
    'Current formal next development task after handoff: `G2 Add Android build smoke path`',
    'Last completed product commit: `1044ef1 Document reward authority contract`',
    'ahead 95',
    'Expected future commit:'
)

if ($RunReadiness) {
    $readinessScript = Resolve-RepoPath -RelativePath "scripts\unity\check_controlled_demo_readiness.ps1"
    $output = & powershell -NoProfile -ExecutionPolicy Bypass -File $readinessScript -RepoRoot $RepoRoot 2>&1
    $exitCode = $LASTEXITCODE
    $joined = (@($output | ForEach-Object { $_.ToString() })) -join [Environment]::NewLine
    if ($exitCode -ne 0 -or $joined -notlike "*Controlled demo readiness preflight OK.*") {
        Add-Failure "Readiness preflight failed during handoff consistency check."
        foreach ($line in @($output | ForEach-Object { $_.ToString() })) {
            Add-Failure "  $line"
        }
    }
    else {
        [void]$rows.Add([pscustomobject]@{
            Check = "readiness preflight"
            Status = "OK"
            Detail = "RunReadiness"
        })
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Controlled demo handoff consistency check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) controlled demo handoff consistency check(s) failed."
}

Write-Host "Controlled demo handoff consistency check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize
