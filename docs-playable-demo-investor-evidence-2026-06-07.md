# Playable Demo Evidence 2026-06-07

Purpose: one concise evidence page for showing the current Windows playable Demo without committing generated screenshots or JSON sidecars.

The current evidence proves a local playable loop: MechLab fitting, squad launch, sparse tactical command, contact pressure, visible section damage, and a repeatable capture/build path. It is still a prototype evidence pack, not final public art.

## Current Evidence Package Refresh

Refreshed on 2026-06-12 after `Package PC controlled demo evidence`.

| Gate | Result | Evidence |
| --- | --- | --- |
| Windows build | Pass | `analysis-output/unity-build-pc-evidence-package.log` reports `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK`. |
| Windows build freshness | Pass | `scripts/unity/check_windows_demo_build_freshness.ps1` reports `Windows demo build freshness check OK` and verifies the ignored Windows player output is newer than tracked Unity build inputs. |
| Visible-flow smoke | Pass | `analysis-output/unity-player-pc-evidence-visible-flow.log` exits with code `0` and proves debrief, repair/Mech Lab, relaunch identity and compact loadout review. |
| Evidence health check | Pass | `scripts/unity/check_controlled_demo_evidence.ps1` reports `Controlled demo evidence check OK` by reading the Windows build, visible-flow log and six capture sidecars. |
| Evidence freshness | Pass | `scripts/unity/check_controlled_demo_evidence.ps1` now rejects visible-flow logs and six capture PNG/JSON sidecars older than the current Windows build/evidence inputs. Fresh ignored evidence was regenerated after the PC19 Windows build refresh. |
| Capture log freshness | Pass | `scripts/unity/check_controlled_demo_evidence.ps1` now also requires the six standard capture logs to exist, be newer than the current Windows build/capture helper, and include preset, screenshot request and sidecar write markers. |
| Readiness preflight | Pass | `scripts/unity/check_controlled_demo_readiness.ps1` reports `Controlled demo readiness preflight OK` by wrapping launch preflight, evidence health and public boundary gates. |
| Handoff consistency | Pass | `scripts/unity/check_controlled_demo_handoff.ps1` reports `Controlled demo handoff consistency check OK` by checking the main scripts and handoff docs agree on the current PC gate set. |
| Demo source hygiene | Pass | `scripts/unity/check_demo_source_hygiene.ps1` reports `Demo source hygiene check OK` and verifies tracked/staged paths plus `.gitignore` keep generated evidence, Unity builds, APK/AAB outputs and private reference art out of source commits. |
| AI deputy contract | Pass | `scripts/unity/check_ai_deputy_contract.ps1` reports `AI deputy contract check OK` without launching Unity or calling MiniMax, and verifies the model path remains opt-in, slow, high-level, rule-fallback guarded and absent from frame loops. |
| PC core playable contract | Pass | `scripts/unity/check_pc_core_playable_contract.ps1` reports `PC core playable contract check OK` and requires the Unity validator marker for command state, solo return, Jet legality, occupancy, damage/ejection and debrief/relaunch. |
| Mobile command model preflight | Pass | `scripts/unity/check_mobile_command_model_preflight.ps1` reports `Mobile command model preflight OK` and verifies active battle remains sparse and mobile-translatable: status rows, Jet, map/bay/system, compact objective, hidden dense overlays and MechLab no-toggle fitting. |
| Battle HUD sparse contract | Pass | `scripts/unity/check_battle_hud_sparse_contract.ps1` reports `Battle HUD sparse contract check OK` and verifies source, capture gate and mobile command model preflight all require sparse active-battle HUD with the mission map closed by default. |
| PC visual capture sanity | Pass | `scripts/unity/check_pc_visual_capture_sanity.ps1` reports `PC visual capture sanity check OK` by checking six PNG captures for size, sampled color variety, center visibility, luminance contrast, low magenta fallback color and non-monochrome content. |
| PC visual capture sanity self-test | Pass | `scripts/unity/check_pc_visual_capture_sanity.ps1 -SelfTest` reports `PC visual capture sanity self-test OK` by proving the thresholds distinguish valid, flat and magenta fallback sample images. |
| PC capture sidecar schema | Pass | `scripts/unity/check_pc_capture_sidecar_schema.ps1` reports `PC capture sidecar schema check OK` by proving the six JSON sidecars keep matching screenshot paths, expected dimensions, flow state, camera state, summary fields and reference-asset metadata. |
| PC capture preset contract | Pass | `scripts/unity/check_pc_capture_preset_contract.ps1` reports `PC capture preset contract check OK` by proving the standard six presets remain `mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol` across capture, evidence, sanity, schema and docs. |
| PC capture artifact hygiene | Pass | `scripts/unity/check_pc_capture_artifact_hygiene.ps1` reports `PC capture artifact hygiene check OK` by proving local reference screenshots, JSON sidecars, capture logs and visual sanity self-test images remain ignored generated evidence and absent from tracked/staged source paths. |
| PC window contract | Pass | `scripts/unity/check_pc_window_contract.ps1` reports `PC window contract check OK` by proving the controlled PC launcher and reference capture helper keep `1280x720` windowed defaults and pass `-screen-fullscreen 0`. |
| PC launch log hygiene | Pass | `scripts/unity/check_pc_launch_log_hygiene.ps1` reports `PC launch log hygiene check OK` by proving the controlled PC launcher writes `analysis-output/windows-demo-run.log`, Git ignores it, and local launch logs are absent from tracked/staged source paths. |
| PC build artifact hygiene | Pass | `scripts/unity/check_pc_build_artifact_hygiene.ps1` reports `PC build artifact hygiene check OK` by proving the Windows player output path stays under ignored `unity-mc2-demo/Builds/Windows/` and local player builds are absent from tracked/staged source paths. |
| PC smoke artifact hygiene | Pass | `scripts/unity/check_pc_smoke_artifact_hygiene.ps1` reports `PC smoke artifact hygiene check OK` by proving PC smoke, validator, build and saved-account evidence outputs remain under ignored `analysis-output/` paths and absent from tracked/staged source paths. |
| Current plan queue consistency | Pass | `scripts/unity/check_current_plan_queue.ps1` reports `Current plan queue consistency check OK` by proving README, BUILD-WIN, master/detailed/PC/mobile/evidence/handoff docs and helper scripts agree on `PC1-PC57`, `Add Android ADB driver package probe`, `WpdOnlyAndroidProbe: True`, `AdbSetupHint: True`, `AdbDriverPackageProbe: True`, `AdbWatchHint: True`, `G3DeviceStatusReport: True`, `G3WhenReady: True`, `Pass Android G3 device smoke`, landscape `G4 Touch UI pass`, `G5 Mobile performance budget`, `G6 iOS feasibility gate`, `F2 map authoring contract`, `F3 web ranking contract`, `F4 creator economy boundary`, `F5 server implementation boundary`, `F6 local main-server prototype`, `F7 document Unity main-server integration contract`, `F8 implement optional Unity main-server client adapter`, `F9 wire optional Unity main-server adapter into launch/debrief smoke`, `F10 wire optional Unity inventory bootstrap smoke`, `F11 plan inventory-to-MechBay binding boundary`, `F12 implement opt-in inventory-to-MechBay preview binding`, `F13 capture opt-in MechBay preview evidence`, and `F14 capture landscape-phone MechLab source-line evidence` as complete, with `F15 plan server-backed receipt slice` complete, `F16 implement server-backed receipt evidence gate` complete, `F17 plan post-receipt inventory refresh boundary` complete, `F18 implement opt-in post-receipt inventory refresh binding` complete, `F19 capture opt-in post-receipt refresh evidence` complete, `F20 refresh Android landscape build/smoke evidence` complete, `F21 audit landscape touch UI ergonomics` complete, `F22 audit landscape MechLab touch controls` complete, `F23 capture landscape MechLab touch evidence` complete, `F24 capture Android MechLab touch evidence` complete, `F25 capture Android battle command touch evidence` complete, with F26 reduce Android combat effect log noise complete, `F27 audit Android entity placeholder collision path` is complete. Evidence gate: `scripts/unity/check_android_entity_placeholder_collision_path.ps1` -> `Android entity placeholder collision path check OK`, and F28 capture Android entity placeholder collision runtime evidence complete, with Evidence gate: `scripts/unity/capture_android_entity_placeholder_collision_runtime_evidence.ps1` -> `Android entity placeholder collision runtime evidence capture OK`, F29 audit PC controlled-demo visual readability complete, with Evidence gate: `scripts/unity/audit_pc_controlled_demo_visual_readability.ps1` -> `PC controlled-demo visual readability audit OK`, and F30 implement PC controlled-demo visual readability fixes complete, with Evidence gate: `scripts/unity/check_pc_controlled_demo_visual_readability_fixes.ps1` -> `PC controlled-demo visual readability fixes check OK`, and F31 refresh PC controlled-demo visual evidence after readability fixes complete; Evidence gate: `scripts/unity/capture_pc_controlled_demo_visual_evidence.ps1` -> `PC controlled-demo visual evidence refresh OK`; F32 audit PC controlled-demo command readability and formation feel complete; Evidence gate: `scripts/unity/audit_pc_controlled_demo_command_readability_formation.ps1` -> `PC controlled-demo command readability formation audit OK`; `F33 implement PC controlled-demo command readability and formation fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_command_readability_fixes.ps1` -> `PC controlled-demo command readability fixes check OK`; `F34 refresh PC controlled-demo command evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F35 audit post-F34 PC controlled-demo playable flow polish` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_playable_flow_polish.ps1` -> `PC controlled-demo playable flow polish audit OK`; `F36 implement post-F34 PC controlled-demo playable flow polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_playable_flow_polish_fixes.ps1` -> `PC controlled-demo playable flow polish fixes check OK`; `F37 refresh PC controlled-demo playable-flow evidence after polish fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F38 audit post-F37 PC controlled-demo investor readiness` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_readiness.ps1` -> `PC controlled-demo investor readiness audit OK`; `F39 implement post-F37 PC controlled-demo investor readiness fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_readiness_fixes.ps1` -> `PC controlled-demo investor readiness fixes check OK`; `F40 refresh PC controlled-demo investor-readiness evidence after fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F41 audit post-F40 PC controlled-demo investor evidence package` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_package.ps1` -> `PC controlled-demo investor evidence package audit OK`; `F42 implement post-F41 PC controlled-demo investor evidence package fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_package_fixes.ps1` -> `PC controlled-demo investor evidence package fixes check OK`; formal next task: `F43 refresh PC controlled-demo investor evidence package after fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target. |
| Unity main-server integration contract | Pass | `scripts/unity/check_unity_main_server_integration_contract.ps1` reports `Unity main-server integration contract check OK` and proves `docs-unity-main-server-integration-contract-2026-06-12.md` matches the local server endpoints, signed squad request, reward claim request, duplicate-claim behavior and offline-first fallback rules. |
| Optional Unity main-server client adapter | Pass | `scripts/unity/check_optional_unity_main_server_client_adapter.ps1` reports `Optional Unity main-server client adapter check OK` and proves Unity has default-disabled DTO/client code for signed squad, reward claim, timeout handling and offline fixture fallback without adding per-frame server calls. |
| Optional Unity main-server launch/debrief smoke | Pass | `scripts/unity/check_optional_unity_main_server_launch_debrief_smoke.ps1` reports `Optional Unity main-server launch/debrief smoke check OK` and proves opt-in command-file smoke can sign a squad before launch, submit reward claim after debrief, and keep a no-server fallback command file playable. |
| Android device connection | Pass | `scripts/unity/check_android_device_connection.ps1` now reports `Android device connection check OK` for Mi 11 Lite `b5212798:device` through `winusb.inf`; the same helper still emits `WpdOnlyAndroidProbe: True` and `AdbSetupHint: True`, and can distinguish no-device, unauthorized, offline, multi-device and ready states before G3 install/launch. |
| Android WPD-only device diagnosis | Pass | The connection helper can report `WpdOnlyAndroidDevice: True` when Windows sees the connected Android phone only as WPD/MTP while adb has no device rows; that remains a G3 waiting state, not an install-ready state. |
| Android ADB setup guidance | Pass | The connection helper reports current Windows driver/provider/inf/service for MTP-only phones and ADB-ready phones; the current Mi 11 Lite state is `provider=Microsoft`, `inf=winusb.inf` and `service=WINUSB`. |
| Android ADB driver package probe | Pass | `scripts/unity/check_android_adb_driver_package.ps1` reports `Android ADB driver package probe OK`, `AdbDriverPackageProbe: True`, `CandidateDriverPackages: none`, and current Mi 11 Lite `inf=winusb.inf` / `service=WINUSB` without installing or changing drivers. |
| Android ADB readiness watch | Pass | `scripts/unity/watch_android_device_connection.ps1 -Once -AllowWaiting` reports `Android device connection watch OK` and `AdbWatchHint: True`; normal timed mode can wait for one authorized adb `device` row without installing or launching the APK. |
| Android G3 device status report | Pass | `scripts/unity/write_android_g3_device_status.ps1` reports `Android G3 device status report OK`, `G3DeviceStatusReport: True`, `G3DeviceReady: True` and `NoInstallOrLaunch: True`, then writes ignored `analysis-output\android-g3-device-status.json` with the current readiness state. |
| Android G3 when-ready runner | Pass | `scripts/unity/run_android_g3_when_ready.ps1 -PlanOnly` still reports `NoInstallOrLaunchUntilDeviceReady: True` for plan mode; with the current authorized Mi 11 Lite, `scripts/unity/run_android_g3_when_ready.ps1 -TimeoutSeconds 30 -AllowWaiting -LaunchWaitSeconds 75` installs, launches, pushes the visible-flow command file, records `G3WhenReady: True`, `SmokeTestPassed: True`, `status=smokePassed`, and writes ignored `analysis-output\android-g3-when-ready-status.json`. |
| Current plan gate | Pass | `scripts/unity/check_current_plan_gate.ps1` reports `Current plan gate check OK` by wrapping handoff/readiness, Windows build freshness, demo source hygiene, AI deputy contract, mobile command model, battle HUD sparse contract, PC visual capture sanity, PC visual capture sanity self-test, PC capture sidecar schema, PC capture preset contract, PC capture artifact hygiene, PC window contract, PC launch log hygiene, PC build artifact hygiene, PC smoke artifact hygiene, current plan queue consistency, Android device connection, Android ADB driver package probe, Android ADB readiness watch, Android G3 device status report, Android G3 when-ready runner and Android device-smoke preflight state in one command. |
| Android smoke log check | Pass | `scripts/unity/check_android_smoke_log.ps1 -SelfTest` reports `Android smoke log check self-test OK`; real-device smoke now scans captured logcat for strong crash markers before accepting launch. |
| Android smoke plan | Pass | `scripts/unity/android_device_smoke.ps1 -PlanOnly` reports `Android device smoke plan OK`, resolving APK/tool/package/activity/log path, ignored screenshot path and install/launch/log-check/screenshot actions without requiring a connected phone. |
| Android smoke connection gate | Pass | `scripts/unity/android_device_smoke.ps1 -PlanOnly` reports `ConnectionCheck: check_android_device_connection.ps1 -RequireDevice`, and real smoke fails before install or launch with `Android device smoke requires a single authorized Android device before install or launch` unless one authorized phone is available. |
| Android smoke connection gate check | Pass | `scripts/unity/check_android_smoke_connection_gate.ps1` reports `Android smoke connection gate check OK` and can report either ready-for-G3 or the fallback marker `Android smoke connection gate check waiting on device`; the check proves the real smoke fail-fast path without rewriting smoke log, screenshot or summary evidence. |
| Android visible-flow command-file smoke | Pass | `scripts/unity/android_device_smoke.ps1 -PlanOnly` reports `CommandFileSmoke: True`, `UnityArguments: -mc2CommandFile`, `SmokeSuccessMarker: MC2 debrief summary assertion OK`, and `SmokeSuccessMarker: MC2 loadout compact assertion OK`; real G3 will require those log markers after pushing `mc2_01-visible-flow-audit.txt` to the device. |
| Android smoke screenshot evidence | Pass | `scripts/unity/android_device_smoke.ps1 -PlanOnly` reports `ScreenshotCapture: True` and the ignored target `analysis-output\android-device-smoke.png`; real device smoke will capture this screenshot unless `-SkipScreenshot` is passed. |
| Android smoke summary evidence | Pass | `scripts/unity/android_device_smoke.ps1 -PlanOnly` reports `SummaryWrite: True` and the ignored target `analysis-output\android-device-smoke-summary.json`; real device smoke will write run metadata unless `-SkipSummary` is passed. |
| Android smoke summary schema | Pass | `scripts/unity/check_android_smoke_summary.ps1 -SelfTest` reports `Android smoke summary check self-test OK`; real device smoke validates the summary JSON after writing it. |
| Android smoke summary preflight | Pass | `scripts/unity/check_android_device_preflight.ps1 -AllowNoDevice` reports `smoke summary schema` and `Android smoke summary check self-test OK` before stopping at waiting-on-device. |
| Android smoke plan/preflight consistency | Pass | `scripts/unity/check_android_smoke_plan_consistency.ps1` reports `Android smoke plan/preflight consistency check OK`, proving plan mode and direct G3 preflight agree on package, activity, evidence paths, execution flags and summary schema readiness. |
| Android G3 readiness | Pass | `scripts/unity/check_android_g3_readiness.ps1 -RequireDevice` reports `Android G3 readiness check OK` after passing device preflight, plan/preflight consistency, smoke plan, log scanner self-test and summary schema self-test. |
| Android G3 device requirement | Pass | `scripts/unity/check_android_g3_device_requirement.ps1` reports `Android G3 device requirement check OK`, proving strict G3 readiness sees the authorized Android phone before the install step. |
| Android SDK tooling | Pass | `scripts/unity/check_android_sdk_tooling.ps1` reports `Android SDK tooling check OK` for Unity AndroidPlayer SDK, NDK, OpenJDK, build-tools 36.0.0, android-36, adb, aapt and apksigner. |
| Android APK freshness | Pass | `scripts/unity/check_android_apk_freshness.ps1` reports `Android APK freshness check OK`; the ignored APK was rebuilt after the current Unity input timestamp and is now accepted by G3 preflight/smoke helpers. |
| Android APK identity | Pass | `scripts/unity/check_android_apk_identity.ps1` reports `Android APK identity check OK` for package `com.DefaultCompany.unitymc2demo` and launch activity `com.unity3d.player.UnityPlayerGameActivity`. |
| Android APK compatibility | Pass | `scripts/unity/check_android_apk_compatibility.ps1` reports `Android APK compatibility check OK` for min SDK 25, target SDK 36 and native-code `arm64-v8a`. |
| Android APK signing | Pass | `scripts/unity/check_android_apk_signing.ps1` reports `Android APK signing check OK` for apksigner verify, APK Signature Scheme v2 and debug signer DN. |
| Android APK manifest | Pass | `scripts/unity/check_android_apk_manifest.ps1` reports `Android APK manifest check OK` for the permission allowlist, no required hardware features, not-required touchscreen/vulkan, and small/normal/large/xlarge screen support. |
| Android APK payload | Pass | `scripts/unity/check_android_apk_payload.ps1` reports `Android APK payload check OK` for required Unity/IL2CPP native libraries, `assets/bin/Data` runtime files and `arm64-v8a` ABI. |
| Android APK size budget | Pass | `scripts/unity/check_android_apk_size_budget.ps1` reports `Android APK size budget check OK`; the current APK stays inside the 1 MiB to 100 MiB early mobile demo budget. |
| Android smoke artifact hygiene | Pass | `scripts/unity/check_android_smoke_artifact_hygiene.ps1` reports `Android smoke artifact hygiene check OK`; APK/AAB outputs, Android smoke logs/screenshots and `Builds/Android` outputs remain ignored and absent from tracked/staged paths. |
| Android device preflight | Pass | `scripts/unity/check_android_device_preflight.ps1 -AllowNoDevice` reports APK, Android SDK tooling, adb, aapt, apksigner, package, launch activity, compatibility metadata, signing, manifest, payload, size budget, Android smoke artifact hygiene and device connection are ready. |
| Six capture presets | Pass | `capture_reference_visuals.ps1` reports `MC2 reference visual captures passed: 6 preset(s)` for `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo`, and
orth-patrol`. |
| MechLab fitting | Pass | `mechlab` capture remains the fitting proof: whole weapon block, armor/cooling filler, H/W/G pressure, `Fit OK`, `CellState=OK OPEN4 OCC12 OCC!0 OOB0`, and
oToggle=yes`. |
| Inventory MechBay preview evidence | Pass | `scripts/unity/capture_inventory_mechbay_preview_evidence.ps1` reports `Inventory MechBay preview evidence capture OK`, writing ignored `analysis-output/inventory-mechbay-preview-evidence/mechlab.png` and `.json` evidence with `Inventory Source: Main Server Preview`, `MainServerPreviewApplied: True`, `ProjectedInventoryValid: True`, and `MobileLandscapeOnly: True`. |
| Landscape-phone MechLab source-line evidence | Pass | `scripts/unity/capture_landscape_phone_mechlab_source_line_evidence.ps1` reports `Landscape-phone MechLab source-line evidence capture OK`, writing ignored `analysis-output/landscape-phone-mechlab-source-line-evidence/mechlab.png` and `.json` evidence at 2400x1080 with `Inventory Source: Main Server Preview`, MechLab whole-block fitting, `MobileTouchUi=ready orientation=landscape`, and `landscapeOnly=yes`. |
| First-map visual gate | Pass | `spawn`, `airfield`, `hangar-contact`, `damage-demo`, and
orth-patrol` sidecars report `FirstMapVisual ... status=ready`, readable terrain, sparse HUD, occupancy, and separated contact. |
| Contact and collision | Pass | Current battle sidecars report `ContactClearance ... overlaps=0 ... status=separated`, including dense objective and north patrol presets; the sidecar tolerance treats sub-1-unit clearance jitter as touching, not a gameplay overlap. |
| Damage story | Pass | `damage-demo` reports `left-arm-lost`, `legs-lost`, `cockpit-lost`, `pilotRisk=1`, `destroyedUnits=1`, and weapon families `energy+missile+ballistic+explosive`. |
| Replacement boundary | Pass | `content-packs/project-owned-starter.example.json`, `content-packs/project-owned-visual-slice.example.json`, and `content-packs/project-owned-art-safe-slice.example.json` pass the public content boundary check; current local screenshots remain development evidence, not public final art. |
| Public boundary preflight | Pass | `scripts/content-pack/check_controlled_demo_public_boundary.ps1` reports `Controlled demo public boundary preflight OK` for project-owned metadata examples; with `-CheckDevBuild`, the current Windows development build remains expected `Result: FAILED`. |

Evidence-package judgment: the current local Windows Demo evidence set now combines the PC4 Windows build, PC19 Windows build freshness check, PC20 evidence freshness check, PC21 capture log freshness check, PC22 Android APK freshness check, PC23 Android APK identity check, PC24 Android APK compatibility check, PC25 Android APK signing check, PC26 Android APK manifest check, PC27 Android APK payload check, PC28 Android APK size budget check, PC29 Android SDK tooling check, PC30 Android smoke artifact hygiene check, PC31 Android smoke screenshot evidence capture, PC32 Android smoke summary evidence output, PC33 Android smoke summary schema check, PC34 Android smoke summary preflight check, PC35 Android smoke plan/preflight consistency check, PC36 Android G3 readiness check, PC37 Android G3 device requirement check, PC38 PC visual capture sanity check, PC39 PC visual capture sanity self-test, PC40 PC capture sidecar schema check, PC41 PC capture preset contract check, PC42 PC capture artifact hygiene check, PC43 PC window contract check, PC44 PC launch log hygiene check, PC45 PC build artifact hygiene check, PC46 PC smoke artifact hygiene check, PC47 current plan queue consistency check, PC48 Android device connection check, PC49 Android smoke connection gate, PC50 Android smoke connection gate check, PC51 Android visible-flow command-file smoke, PC52 Android WPD-only device diagnosis, PC53 Android ADB setup guidance, PC54 Android ADB readiness watch, PC55 Android G3 device status report, PC56 Android G3 when-ready runner, PC57 Android ADB driver package probe, visible-flow smoke, six fresh screenshots/sidecars/logs, PC11 core playable contract check, PC12 mobile command model preflight, PC13 current plan gate check, PC14 Android smoke log crash scan, PC15 Android smoke plan mode, PC16 battle HUD sparse contract check, PC17 demo source hygiene check, PC18 AI deputy contract check, manifest-driven local visuals, a clean replacement id path, a metadata-only art-safe mission-slice target, `Pass Android G3 device smoke`, landscape `G4 Touch UI pass`, `G5 Mobile performance budget`, `G6 iOS feasibility gate`, `F2 map authoring contract`, `F3 web ranking contract`, `F4 creator economy boundary`, `F5 server implementation boundary`, `F6 local main-server prototype`, `F7 document Unity main-server integration contract`, `F8 implement optional Unity main-server client adapter`, `F9 wire optional Unity main-server adapter into launch/debrief smoke`, `F10 wire optional Unity inventory bootstrap smoke`, and `F11 plan inventory-to-MechBay binding boundary` completed; `F12 implement opt-in inventory-to-MechBay preview binding` is complete; `F13 capture opt-in MechBay preview evidence` is complete; `F14 capture landscape-phone MechLab source-line evidence` is complete; `F15 plan server-backed receipt slice` is complete; `F16 implement server-backed receipt evidence gate` is complete; `F17 plan post-receipt inventory refresh boundary` is complete; `F18 implement opt-in post-receipt inventory refresh binding` is complete; `F19 capture opt-in post-receipt refresh evidence` is complete; `F20 refresh Android landscape build/smoke evidence` is complete; `F21 audit landscape touch UI ergonomics` is complete; `F22 audit landscape MechLab touch controls` is complete; `F23 capture landscape MechLab touch evidence` is complete; `F24 capture Android MechLab touch evidence` is complete; `F25 capture Android battle command touch evidence` is complete; `F26 reduce Android combat effect log noise` is complete; `F27 audit Android entity placeholder collision path` is complete. Evidence gate: `scripts/unity/check_android_entity_placeholder_collision_path.ps1` -> `Android entity placeholder collision path check OK`; `F28 capture Android entity placeholder collision runtime evidence` is complete. Evidence gate: `scripts/unity/capture_android_entity_placeholder_collision_runtime_evidence.ps1` -> `Android entity placeholder collision runtime evidence capture OK`; `F29 audit PC controlled-demo visual readability` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_visual_readability.ps1` -> `PC controlled-demo visual readability audit OK`. `F30 implement PC controlled-demo visual readability fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_visual_readability_fixes.ps1` -> `PC controlled-demo visual readability fixes check OK`. `F31 refresh PC controlled-demo visual evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_visual_evidence.ps1` -> `PC controlled-demo visual evidence refresh OK`; `F32 audit PC controlled-demo command readability and formation feel` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_command_readability_formation.ps1` -> `PC controlled-demo command readability formation audit OK`; `F33 implement PC controlled-demo command readability and formation fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_command_readability_fixes.ps1` -> `PC controlled-demo command readability fixes check OK`; `F34 refresh PC controlled-demo command evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F35 audit post-F34 PC controlled-demo playable flow polish` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_playable_flow_polish.ps1` -> `PC controlled-demo playable flow polish audit OK`; `F36 implement post-F34 PC controlled-demo playable flow polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_playable_flow_polish_fixes.ps1` -> `PC controlled-demo playable flow polish fixes check OK`; `F37 refresh PC controlled-demo playable-flow evidence after polish fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F38 audit post-F37 PC controlled-demo investor readiness` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_readiness.ps1` -> `PC controlled-demo investor readiness audit OK`; `F39 implement post-F37 PC controlled-demo investor readiness fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_readiness_fixes.ps1` -> `PC controlled-demo investor readiness fixes check OK`; `F40 refresh PC controlled-demo investor-readiness evidence after fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F41 audit post-F40 PC controlled-demo investor evidence package` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_package.ps1` -> `PC controlled-demo investor evidence package audit OK`; `F42 implement post-F41 PC controlled-demo investor evidence package fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_package_fixes.ps1` -> `PC controlled-demo investor evidence package fixes check OK`; formal next task: `F43 refresh PC controlled-demo investor evidence package after fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

## Visible Flow Seal Refresh

Refreshed on 2026-06-12 after `Package PC controlled demo evidence`.

| Gate | Result | Evidence |
| --- | --- | --- |
| Visible-flow smoke | Pass | `analysis-output/unity-player-pc-evidence-visible-flow.log` reports `MC2 demo smoke test exiting with code 0`. |
| Combat command loop | Pass | The same log reports quiet, tracking and fire combat assertions with sparse UI, status rows, solo order, squad command, Jet, contact pressure and hidden combat log. |
| Debrief | Pass | The log reports `MC2 debrief resolve OK`, `MC2 debrief open OK`, and `MC2 debrief summary assertion OK` with `result=Victory`, objectives `6/6`, payout/salvage/bounty rows and clear overlays. |
| Repair and return path | Pass | The log reports `actions=Repair & Mech Lab/Next Contract/Retry Battle/Close`, saved account delta, compact loadout review, repair copy and Mech Lab route. |
| Relaunch identity | Pass | The log reports `assert-restart-identity depot` and `MC2 loadout compact assertion OK` after the reserve squad path. |

Visible-flow judgment: the local Windows Demo can now be described as a sealed first playable loop: fit squad, launch, issue squad/solo/Jet commands, fight to victory, read Debrief, repair or return to Mech Lab, and relaunch without relying on presenter hand-waving.

## Current Visual Gate Refresh

Refreshed on 2026-06-07 after `Gate first map visual slice`.

| Gate | Result | Evidence |
| --- | --- | --- |
| Markdown/code whitespace | Pass | `git diff --check` |
| Six capture presets | Pass | `capture_reference_visuals.ps1` reports `MC2 reference visual captures passed: 6 preset(s)` for `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo`, and
orth-patrol`. |
| MechLab fitting | Pass | `mechlab.json` reports `MechLabCapture=open`, `weaponBlock=1 Streak 1x2`, `fillers=A+/C+`, `Fit OK`, `CellState=OK OPEN4 OCC12 OCC!0 OOB0`, and
oToggle=yes`. |
| First-map visual gate | Pass | All five battle sidecars report `FirstMapVisual ... status=ready`, with `terrain=ready`, `unit=ready`, `structure=ready`, `sparseHud=ready`, `occupancy=ready`, and `contact=separated`. |
| Sparse battle UI | Pass | Battle sidecars report `combatLog=hidden`, `objective=compactObjective`, `accountUi=hidden`, `saveUi=disabled`, `debugOccupancy=sidecar-only`, and `overlays=hidden`. |
| BattleCore occupancy | Pass | Battle sidecars report `unitRadii infantry=24 vehicle=54 mech=64`, `structures 1`, `hardProps 80`, `destinationFallback=structure+hardProp`, and water/map-boundary landing predicates. |
| Contact clearance | Pass | Battle sidecars report `ContactClearance ... overlaps=0 ... status=separated`, including the dense `hangar-contact` and `damage-demo` presets. |
| Damage story | Pass | `damage-demo.json` reports `left-arm-lost`, `legs-lost`, `cockpit-lost`, `pilotRisk=1`, `destroyedUnits=1`, and `DamageReadability=weaponFamilies energy+missile+ballistic+explosive`. |
| Public content boundary | Development-only | The screenshots may use private reference visuals as local development evidence. They are not a public art-safe release package. |

Current judgment: the local Windows Demo is now a coherent investor/demo evidence slice. It shows preparation, launch, map readability, objective pressure, BattleCore collision evidence, section-damage drama and broader trigger pressure. It remains prototype art and must be swapped to project-owned or licensed content before public/commercial release.

## Handoff Gate Audit

Refreshed on 2026-06-07 after `ce5dced Add public content boundary check`.

| Gate | Result | Evidence |
| --- | --- | --- |
| Markdown/code whitespace | Pass | `git diff --check` |
| Mission validator | Pass | `analysis-output/unity-validate-handoff.log` reports `MC2 demo contract validation OK: 29 units, 3 player units, 9 objectives, 1 structure, 10000 terrain samples, 1000 terrain objects, combat simulation passed.` |
| Windows build | Pass | `analysis-output/unity-build-handoff.log` reports `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK`. |
| Visible-flow smoke | Pass | `analysis-output/unity-player-handoff-visible-flow.log` reports `MC2 demo smoke test exiting with code 0`. |
| Six capture presets | Pass | `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo`,
orth-patrol` PNG/JSON sidecars refreshed under `analysis-output/reference-visual-captures/`. |
| Clean starter boundary | Pass | `check_public_content_boundary.ps1` returns `Result: OK` for `content-packs/project-owned-starter.example.json`. |
| Current build public boundary | Development-only | The current Windows dev build returns `Result: FAILED` with 172 expected findings, including `MC2UnityDemo`, `mc2_01` command/mission ids, local paths, reference-linked pack traces and legacy unit markers. This is a correct warning, not a build failure. |

Handoff judgment: the local Windows Demo is buildable, smoke-tested, capturable and explainable as a development Demo. It is not public-safe until a clean replacement content pack and public build identity replace the current development-only markers.

## V4 Contact Occupancy Refresh

Refreshed on 2026-06-07 after `Polish crowded contact occupancy`.

| Gate | Result | Evidence |
| --- | --- | --- |
| Mission validator | Pass | `analysis-output/unity-validate-crowded-contact.log` reports `MC2 demo contract validation OK`. |
| Windows build | Pass | `analysis-output/unity-build-crowded-contact.log` reports `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK`. |
| Hangar contact capture | Pass | `analysis-output/reference-visual-captures/hangar-contact.png` and `.json` refreshed with `unitRadii infantry=24 vehicle=54 mech=64` and `ContactSpread=players 3 hostiles 20 nearestPH=272.8 nearestHH=48`. |
| Damage demo capture | Pass | `analysis-output/reference-visual-captures/damage-demo.png` and `.json` refreshed with `unitRadii infantry=24 vehicle=54 mech=64`, `ContactSpread=players 2 hostiles 20 nearestPH=118 nearestHH=78`, and the section-damage story intact. |
| Capture robustness | Pass | `scripts/unity/capture_reference_visuals.ps1` now waits longer for late damage captures and cleans up leftover local player processes after timeout. |

V4 judgment: the hangar fight remains intentionally dense, but the screenshots now have explicit BattleCore occupancy and contact-spread evidence. The next pass should refresh the full demo evidence set rather than immediately adding more battle UI.

## Full Demo Refresh

Refreshed on 2026-06-07 after V4.

| Gate | Result | Evidence |
| --- | --- | --- |
| Markdown/code whitespace | Pass | `git diff --check` |
| Mission validator | Pass | `analysis-output/unity-validate-demo-refresh.log` reports `MC2 demo contract validation OK`. |
| Windows build | Pass | `analysis-output/unity-build-demo-refresh.log` reports `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK`. |
| Visible-flow smoke | Pass | `analysis-output/unity-player-demo-refresh.log` reports `MC2 demo smoke test exiting with code 0`. |
| Six capture presets | Pass | `capture_reference_visuals.ps1` reports `MC2 reference visual captures passed: 6 preset(s)` for `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo`, and
orth-patrol`. |
| Clean starter boundary | Pass | `check_public_content_boundary.ps1` returns `Result: OK` for `content-packs/project-owned-starter.example.json`. |
| Current build public boundary | Development-only | The current Windows dev build returns expected `Result: FAILED` with 172 findings. |

Refresh judgment: the local development Demo is again validated as buildable, smoke-tested, capturable and explicitly development-only. The next product task is the public replacement content slice.

## MechLab Grid Feel Refresh

Refreshed on 2026-06-07 after `Polish MechLab grid feel`.

| Gate | Result | Evidence |
| --- | --- | --- |
| Mission validator | Pass | `analysis-output/unity-validate-mechlab-grid-feel.log` reports `MC2 demo contract validation OK`. |
| Windows build | Pass | `analysis-output/unity-build-mechlab-grid-feel.log` reports `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK`. |
| MechLab capture | Pass | `analysis-output/reference-visual-captures/mechlab.png` and `.json` refreshed with `CellState=OK OPEN4 OCC12 OCC!0 OOB0`. |

MechLab judgment: the fitting screen now proves whole weapon blocks, single-cell armor/cooling fillers, short H/W/G pressure, explicit cell-state evidence, and
oToggle=yes` for mounted weapons.

## Loadout Battle Effect Refresh

Refreshed on 2026-06-07 after `Prove loadout battle effects`.

| Gate | Result | Evidence |
| --- | --- | --- |
| Markdown/code whitespace | Pass | `git diff --check` |
| Mission validator | Pass | `analysis-output/unity-validate-loadout-battle-effect.log` reports `MC2 demo contract validation OK`. |
| Windows build | Pass | `analysis-output/unity-build-loadout-battle-effect.log` reports `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK`. |
| Visible-flow smoke | Pass | `analysis-output/unity-player-loadout-battle-effect.log` reaches victory, debrief, MechLab relaunch and loadout compact checks. |

Loadout judgment: MechLab previews now have a guarded gameplay path. BattleCore records source/mounted weapon counts, rejects invalid preview fits, and applies legal armor/heat-sink changes to the same `UnitState` combat fields used by live battle.

## Weapon And Damage Readability Refresh

Refreshed on 2026-06-07 after `Polish weapon and damage readability`.

| Gate | Result | Evidence |
| --- | --- | --- |
| Markdown/code whitespace | Pass | `git diff --check` |
| Mission validator | Pass | `analysis-output/unity-validate-damage-readability.log` reports `MC2 demo contract validation OK`. |
| Windows build | Pass | `analysis-output/unity-build-damage-readability.log` reports `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK`. |
| Damage capture | Pass | `analysis-output/reference-visual-captures/damage-demo.png` and `.json` refreshed with `damageReadability`. |
| Hangar regression capture | Pass | `analysis-output/reference-visual-captures/hangar-contact.png` and `.json` refreshed with contact spread and sparse battle HUD intact. |

Damage judgment: the damage-demo sidecar now proves weapon families, beam/arc/tracer/shock shapes, hit cue families, section consequences, sparse HUD state, `left-arm-lost`, `legs-lost`, `cockpit-lost`, pilot risk and destroyed-unit evidence in one capture.

## Sparse Battle UI Regression Guard

Refreshed on 2026-06-07 after `Guard sparse battle UI regression`.

| Gate | Result | Evidence |
| --- | --- | --- |
| Markdown/code whitespace | Pass | `git diff --check` |
| Windows build | Pass | `analysis-output/unity-build-battle-ui-regression.log` reports `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK`. |
| Visible-flow smoke | Pass | `analysis-output/unity-player-battle-ui-regression.log` reports `MC2 demo smoke test exiting with code 0` and combat situation assertions include `sparseUi=SparseBattleUi=statusRows+sections+solo`. |
| Spawn capture | Pass | `analysis-output/reference-visual-captures/spawn.json` reports hidden combat log, disabled save UI, hidden account UI, sidecar-only debug occupancy and hidden overlays. |
| Damage capture | Pass | `analysis-output/reference-visual-captures/damage-demo.json` reports the same sparse HUD contract while preserving the damage story. |

Sparse UI judgment: the first-version battle view now has a hard evidence gate. It keeps the command surface visible without letting the battle turn back into log walls, save slots, account panels or debug overlays.

## Close Contact Collision Gate

Refreshed on 2026-06-07 after `Add close contact collision gate`.

| Gate | Result | Evidence |
| --- | --- | --- |
| Mission validator | Pass | `analysis-output/unity-validate-close-contact-collision.log` reports `MC2 demo contract validation OK` and validates BattleCore contact-clearance evidence on the unit collision fixture. |
| Windows build | Pass | `analysis-output/unity-build-close-contact-collision.log` reports `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK`. |
| Hangar contact capture | Pass | `analysis-output/reference-visual-captures/hangar-contact.json` reports `ContactClearance ... overlaps=0 ... status=separated`, `hardProps 80`, blocker categories, and `landingBlockedMarkers 16`. |
| Damage regression capture | Pass | `analysis-output/reference-visual-captures/damage-demo.json` reports `ContactClearance ... overlaps=0 ... status=separated` while retaining the damage story. |

Collision judgment: the crowded hangar fight is now measurable as close contact with collision circles touching or nearly touching, not a one-point pile. Future visual work should improve scale, silhouettes and camera composition while preserving this BattleCore gate.

## Capture Command

Run from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

Generated files stay ignored under:

```text
analysis-output/reference-visual-captures/
```

## Evidence Beats

| Beat | Local Evidence | What It Proves | Talk Track |
| --- | --- | --- | --- |
| MechLab fitting | `analysis-output/reference-visual-captures/mechlab.png` and `.json` | The fitting loop is visible: whole weapon blocks, `A+` armor, `C+` cooling, H/W/G pressure, `Fit OK`, `CellState=OK OPEN4 OCC12 OCC!0 OOB0`, and mounted weapons active by default. | The player prepares the squad by arranging physical equipment blocks instead of toggling abstract rows. |
| Squad spawn | `analysis-output/reference-visual-captures/spawn.png` and `.json` | Battle starts with sparse HUD, commander-follow camera, 3-player squad, target structure, terrain/structure/unit readiness and `FirstMapVisual status=ready`. | The default command state is simple enough for future touch control: whole squad first, status rows for exceptions. |
| Airfield contact | `analysis-output/reference-visual-captures/airfield.png` and `.json` | Terrain, water, runway/road tones, props, 12 active hostiles, 8 visible hostiles, target structure and `contact=separated` are readable. | The map is now a tactical space with terrain, contact direction and objective pressure, not just colored blocks. |
| Hangar pressure | `analysis-output/reference-visual-captures/hangar-contact.png` and `.json` | A dense objective fight is visible with 20 active hostiles, 16 visible hostiles, `unitRadii infantry=24 vehicle=54 mech=64`, `hardProps 80`, `ContactClearance overlaps=0`, and `FirstMapVisual status=ready`. | This is the pressure-test image: it shows the battle system under load while proving the units are separated by BattleCore, not merely painted apart. |
| Damage story | `analysis-output/reference-visual-captures/damage-demo.png` and `.json` | Section damage is explicit: `DamageReadability=weaponFamilies energy+missile+ballistic+explosive`, `left-arm-lost`, `legs-lost`, `cockpit-lost`, `pilotRisk=1`, `destroyedUnits=1`, and sparse status-row confirmation. | The selling point is not only HP bars; arms, legs, cockpit, ejection, weapon family cues, and wreck state can drive tactical drama. |
| North patrol / wider contact | `analysis-output/reference-visual-captures/north-patrol.png` and `.json` | A larger encounter slice stays readable with 24 active hostiles, 9 visible hostiles, 27 active units, `FirstMapVisual status=ready`, and `ContactClearance overlaps=0`. | The same rules can cover broader patrol/trigger beats beyond the starting hangar fight. |

## Sidecar Highlights

Current refreshed sidecars report:

```text
mechlab: MechLabCapture=open flow=Mech Lab unit=Werewolf weaponBlock=1 Streak 1x2 fillers=A+/C+ fit=Fit OK pressure=H 12/22  W 16/16  G 12/16 CellState=OK OPEN4 OCC12 OCC!0 OOB0 layout=pressure-cards+whole-blocks+single-fillers alwaysMounted=weapons 6/6 items 6/6 noToggle=yes
spawn: FirstMapVisual status=ready terrain=ready unit=ready structure=ready sparseHud=ready occupancy=ready contact=separated playerUnits=3 activeHostiles=0 visibleHostiles=0 targetableStructures=1 ContactClearance overlaps=0 status=separated
airfield: FirstMapVisual status=ready activeHostiles=12 visibleHostiles=8 ContactClearance nearestPH=unit-1>unit-5 clearance=590.7 nearestHH=unit-5>unit-6 clearance=0 overlaps=0 status=separated
hangar-contact: FirstMapVisual status=ready activeHostiles=20 visibleHostiles=16 ContactClearance nearestPH=unit-2>unit-5 clearance=154.8 nearestHH=unit-19>unit-21 clearance=0 nearestPP=unit-1>unit-3 clearance=131.1 overlaps=0 worstClearance=0 status=separated
damage-demo: FirstMapVisual status=ready activeHostiles=20 visibleHostiles=16 SparseBattleUi=statusRows+sections+solo ContactClearance overlaps=0 status=separated DamageStory=units 3/3 lostSections=3 arms=1 legs=1 cockpit=1 pilotRisk=1 destroyedUnits=1 story=unit-1:left-arm-lost,unit-2:legs-lost,unit-3:cockpit-lost
damageReadability: weaponFamilies energy+missile+ballistic+explosive; weaponShapes beam+arc+tracer+shock; sectionConsequences arms-firepower legs-mobility cockpit-ejection wreck-salvage; hud=section-bars+short-labels+sparse
north-patrol: FirstMapVisual status=ready activeHostiles=24 visibleHostiles=9 units=27/29 ContactClearance overlaps=0 status=separated DamageStory=units 1/3 criticalSections=1 story=unit-1:right-arm-critical
terrainReadability: samples 10000 texture=composite textureStrength=0.28 waterSurface=readable-overlay alpha=0.48 water=9392 shore=92 runway=110 dirt=11 textured=188 style=land-outline+runway-contrast+water-muted pathing=unchanged
```

## Suggested Three-Minute Use

Use these in order:

1. Start with `mechlab.png`: show fitting as the collection/preparation pillar.
2. Move to `spawn.png`: show sparse command UI, commander-follow camera and whole-squad-first control.
3. Use `airfield.png`: show terrain, runway/road, structures, props and readable contact direction.
4. Use `hangar-contact.png`: show the dense objective pressure test and BattleCore separation proof.
5. Use `damage-demo.png`: show section damage, cockpit risk, ejection/destroyed-state story and weapon-family cues.
6. Close with
orth-patrol.png`: show the same mission can trigger broader encounter beats.

## Honest Limits

Do not claim these screenshots are final public art. The local captures may use private reference content as development evidence for scale, pacing, and readability. Public or commercial builds must use project-owned or properly licensed replacement content packs.

Do not present the first version as supporting realtime PVP, mobile builds, map servers, account economy, paid recharge, chain assets, AI director, or model-driven per-frame combat. The current first-version promise is a local Windows playable Demo with deterministic BattleCore, readable MechLab fitting, sparse command, visible damage, repair/relaunch flow, and optional high-level AI deputy.

## F12 Preview Binding Checkpoint

`F12 implement opt-in inventory-to-MechBay preview binding` is complete. `F13 capture opt-in MechBay preview evidence` is complete. `F14 capture landscape-phone MechLab source-line evidence` is complete. The opt-in gate is `scripts/unity/check_optional_inventory_mechbay_preview_binding.ps1`, with expected success string `Optional inventory-to-MechBay preview binding check OK`; the preview evidence gate is `scripts/unity/capture_inventory_mechbay_preview_evidence.ps1`, with expected success string `Inventory MechBay preview evidence capture OK`; the landscape-phone evidence gate is `scripts/unity/capture_landscape_phone_mechlab_source_line_evidence.ps1`, with expected success string `Landscape-phone MechLab source-line evidence capture OK`. `F15 plan server-backed receipt slice` is complete. Evidence gate: `scripts/unity/check_server_backed_receipt_slice_plan.ps1` -> `Server-backed receipt slice plan check OK`. `F16 implement server-backed receipt evidence gate` is complete. Evidence gate: `scripts/unity/capture_server_backed_receipt_evidence.ps1` -> `Server-backed receipt evidence capture OK`. `F17 plan post-receipt inventory refresh boundary` is complete. Evidence gate: `scripts/unity/check_post_receipt_inventory_refresh_boundary.ps1` -> `Post-receipt inventory refresh boundary check OK`. `F18 implement opt-in post-receipt inventory refresh binding` is complete. Evidence gate: `scripts/unity/check_post_receipt_inventory_refresh_binding.ps1` -> `Post-receipt inventory refresh binding check OK`. `F19 capture opt-in post-receipt refresh evidence` is complete. Evidence gate: `scripts/unity/capture_post_receipt_refresh_evidence.ps1` -> `Post-receipt refresh evidence capture OK`. `F20 refresh Android landscape build/smoke evidence` is complete. `F21 audit landscape touch UI ergonomics` is complete. Evidence gate: `scripts/unity/check_landscape_touch_ui_ergonomics.ps1` -> `Landscape touch UI ergonomics check OK`. `F22 audit landscape MechLab touch controls` is complete. Evidence gate: `scripts/unity/check_landscape_mechlab_touch_controls.ps1` -> `Landscape MechLab touch controls check OK`. `F23 capture landscape MechLab touch evidence` is complete. Evidence gate: `scripts/unity/capture_landscape_mechlab_touch_evidence.ps1` -> `Landscape MechLab touch evidence capture OK`. `F24 capture Android MechLab touch evidence` is complete. Evidence gate: `scripts/unity/capture_android_mechlab_touch_evidence.ps1` -> `Android MechLab touch evidence capture OK`. F25 capture Android battle command touch evidence is complete. Evidence gate: `scripts/unity/capture_android_battle_command_touch_evidence.ps1` -> `Android battle command touch evidence capture OK`. `F26 reduce Android combat effect log noise` is complete. `F27 audit Android entity placeholder collision path` is complete. Evidence gate: `scripts/unity/check_android_entity_placeholder_collision_path.ps1` -> `Android entity placeholder collision path check OK`. `F28 capture Android entity placeholder collision runtime evidence` is complete. Evidence gate: `scripts/unity/capture_android_entity_placeholder_collision_runtime_evidence.ps1` -> `Android entity placeholder collision runtime evidence capture OK`. `F29 audit PC controlled-demo visual readability` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_visual_readability.ps1` -> `PC controlled-demo visual readability audit OK`. `F30 implement PC controlled-demo visual readability fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_visual_readability_fixes.ps1` -> `PC controlled-demo visual readability fixes check OK`. `F31 refresh PC controlled-demo visual evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_visual_evidence.ps1` -> `PC controlled-demo visual evidence refresh OK`; `F32 audit PC controlled-demo command readability and formation feel` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_command_readability_formation.ps1` -> `PC controlled-demo command readability formation audit OK`; `F33 implement PC controlled-demo command readability and formation fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_command_readability_fixes.ps1` -> `PC controlled-demo command readability fixes check OK`; `F34 refresh PC controlled-demo command evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F35 audit post-F34 PC controlled-demo playable flow polish` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_playable_flow_polish.ps1` -> `PC controlled-demo playable flow polish audit OK`; `F36 implement post-F34 PC controlled-demo playable flow polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_playable_flow_polish_fixes.ps1` -> `PC controlled-demo playable flow polish fixes check OK`; `F37 refresh PC controlled-demo playable-flow evidence after polish fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F38 audit post-F37 PC controlled-demo investor readiness` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_readiness.ps1` -> `PC controlled-demo investor readiness audit OK`; `F39 implement post-F37 PC controlled-demo investor readiness fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_readiness_fixes.ps1` -> `PC controlled-demo investor readiness fixes check OK`; `F40 refresh PC controlled-demo investor-readiness evidence after fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F41 audit post-F40 PC controlled-demo investor evidence package` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_package.ps1` -> `PC controlled-demo investor evidence package audit OK`; `F42 implement post-F41 PC controlled-demo investor evidence package fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_package_fixes.ps1` -> `PC controlled-demo investor evidence package fixes check OK`; formal next task: `F43 refresh PC controlled-demo investor evidence package after fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.
