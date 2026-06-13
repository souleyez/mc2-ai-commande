# PC Demo Optimization Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 在 Android G3 真机验证等待设备期间，继续把 Windows/PC 可玩 Demo 打磨成能稳定演示、能截图、能解释战斗乐趣的版本。

**Architecture:** `BattleCore` 仍然是规则真相，PC 优化只改善可见性、输入舒适度、截图证据、构建和演示包装。任何 PC 便利功能都不能改变移动端的核心指挥模型：默认全队、状态栏单选、点地/点目标、Jet、任务地图、系统按钮。

**Tech Stack:** Unity 6, C#, deterministic BattleCore, Windows player build/smoke, PowerShell validator/build/capture scripts, ignored visual evidence under `analysis-output/`.

---

## Product Decision

Mobile support remains the product priority. `G3 Android Device Smoke` has now passed on a physical Mi 11 Lite with USB debugging authorized, and this PC optimization plan is retained as the wait-state evidence package that kept the Windows demo moving while the phone blocker existed.

The current PC/mobile wait-state optimization pass is now sealed through PC1-PC57. `Pass Android G3 device smoke`, landscape `G4 Touch UI pass`, `G5 Mobile performance budget`, `G6 iOS feasibility gate`, `F2 map authoring contract`, `F3 web ranking contract`, `F4 creator economy boundary`, `F5 server implementation boundary`, `F6 local main-server prototype`, `F7 document Unity main-server integration contract`, `F8 implement optional Unity main-server client adapter`, `F9 wire optional Unity main-server adapter into launch/debrief smoke`, `F10 wire optional Unity inventory bootstrap smoke`, and `F11 plan inventory-to-MechBay binding boundary` are recorded; `F12 implement opt-in inventory-to-MechBay preview binding` is complete; `F13 capture opt-in MechBay preview evidence` is complete; `F15 plan server-backed receipt slice` is complete; the `F16 implement server-backed receipt evidence gate` is complete; `F17 plan post-receipt inventory refresh boundary` is complete; `F18 implement opt-in post-receipt inventory refresh binding` is complete; `F19 capture opt-in post-receipt refresh evidence` is complete; `F20 refresh Android landscape build/smoke evidence` is complete; `F21 audit landscape touch UI ergonomics` is complete; `F22 audit landscape MechLab touch controls` is complete; `F23 capture landscape MechLab touch evidence` is complete; `F24 capture Android MechLab touch evidence` is complete; `F25 capture Android battle command touch evidence` is complete. Evidence gate: `scripts/unity/capture_android_battle_command_touch_evidence.ps1` -> `Android battle command touch evidence capture OK`; `F26 reduce Android combat effect log noise` is complete; `F27 audit Android entity placeholder collision path` is complete. Evidence gate: `scripts/unity/check_android_entity_placeholder_collision_path.ps1` -> `Android entity placeholder collision path check OK`; `F28 capture Android entity placeholder collision runtime evidence` is complete. Evidence gate: `scripts/unity/capture_android_entity_placeholder_collision_runtime_evidence.ps1` -> `Android entity placeholder collision runtime evidence capture OK`. `F29 audit PC controlled-demo visual readability` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_visual_readability.ps1` -> `PC controlled-demo visual readability audit OK`. `F30 implement PC controlled-demo visual readability fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_visual_readability_fixes.ps1` -> `PC controlled-demo visual readability fixes check OK`. `F31 refresh PC controlled-demo visual evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_visual_evidence.ps1` -> `PC controlled-demo visual evidence refresh OK`; `F32 audit PC controlled-demo command readability and formation feel` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_command_readability_formation.ps1` -> `PC controlled-demo command readability formation audit OK`; `F33 implement PC controlled-demo command readability and formation fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_command_readability_fixes.ps1` -> `PC controlled-demo command readability fixes check OK`; `F34 refresh PC controlled-demo command evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F35 audit post-F34 PC controlled-demo playable flow polish` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_playable_flow_polish.ps1` -> `PC controlled-demo playable flow polish audit OK`; `F36 implement post-F34 PC controlled-demo playable flow polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_playable_flow_polish_fixes.ps1` -> `PC controlled-demo playable flow polish fixes check OK`; `F37 refresh PC controlled-demo playable-flow evidence after polish fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F38 audit post-F37 PC controlled-demo investor readiness` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_readiness.ps1` -> `PC controlled-demo investor readiness audit OK`; `F39 implement post-F37 PC controlled-demo investor readiness fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_readiness_fixes.ps1` -> `PC controlled-demo investor readiness fixes check OK`; `F40 refresh PC controlled-demo investor-readiness evidence after fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F41 audit post-F40 PC controlled-demo investor evidence package` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_package.ps1` -> `PC controlled-demo investor evidence package audit OK`; `F42 implement post-F41 PC controlled-demo investor evidence package fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_package_fixes.ps1` -> `PC controlled-demo investor evidence package fixes check OK`; `F43 refresh PC controlled-demo investor evidence package after fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh check OK`; `F44 audit post-F43 PC controlled-demo investor evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh audit OK`; `F45 implement post-F44 PC controlled-demo investor evidence polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_polish_fixes.ps1` -> `PC controlled-demo investor evidence polish fixes check OK`; `F46 refresh PC controlled-demo investor route evidence after polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh check OK`; `F47 audit post-F46 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh audit OK`; formal next task: `F48 implement post-F47 PC controlled-demo investor route evidence audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

## Definition Of Done

The current PC optimization pass is complete when:

- Unity mission validator passes.
- Windows player build passes.
- Visible-flow smoke reaches debrief, repair and relaunch.
- `mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol` captures pass.
- First battle view clearly separates terrain, units, buildings, water, roads/runway, objectives and contact direction.
- Dense contact still reports no true overlap in sidecar evidence.
- Battle UI remains sparse: status rows, Jet, objective/map, system/pause; no large logs, save slots, account UI or debug overlays in normal battle.
- MechLab PC flow is easy to read: mounted weapon blocks are whole-grid objects, all installed weapons are active, heat/weight/slot legality is visible, no enable/disable toggle returns.
- Controlled Windows demo launch has a preflight helper and defaults to a stable 1280x720 windowed launch.
- Controlled Windows demo window sizing can be checked without launching Unity, proving the demo launcher and reference capture helper keep `1280x720` windowed defaults and pass `-screen-fullscreen 0`.
- Controlled Windows demo build freshness can be checked without launching Unity, proving the ignored player output is newer than tracked Unity build inputs.
- Controlled Windows demo evidence can be checked by script without rerunning Unity.
- Controlled Windows demo evidence freshness can be checked by script, proving visible-flow logs, six capture PNG/JSON sidecars and six capture logs are newer than the current Windows build and evidence inputs.
- Controlled Windows demo public boundary metadata can be checked by script without packaging or creating artifacts.
- Controlled Windows demo readiness can be checked by one script that wraps launch, evidence and public boundary gates.
- Controlled Windows demo handoff consistency can be checked by one script that validates docs and helper scripts agree on the current gate set.
- Android device-smoke readiness can be checked without installing or launching the app, and can explicitly stop at waiting-on-device when no phone is connected.
- PC core playable contract can be checked by one script that runs the Unity/BattleCore validator and requires command-state, solo-return, Jet, occupancy, damage/ejection and debrief/relaunch coverage.
- Mobile command model preflight can be checked without launching Unity, proving the current PC command surface still maps to status rows, Jet, map/bay/system, compact objective, sparse HUD and MechLab no-toggle fitting.
- Current plan gate can be checked by one script that wraps handoff/readiness, Windows build freshness, demo source hygiene, AI deputy contract, mobile command model, battle HUD sparse contract, PC visual capture sanity, PC visual capture sanity self-test, PC capture sidecar schema, PC capture preset contract, PC capture artifact hygiene, PC window contract, PC launch log hygiene, PC build artifact hygiene, PC smoke artifact hygiene, current plan queue consistency, Android device connection and Android device-smoke preflight state.
- Android device smoke scans captured logcat for strong crash markers before accepting a real-device launch.
- Android device smoke can be previewed with `-PlanOnly` without a connected phone.
- Android SDK tooling can be checked before G3, proving Unity's AndroidPlayer SDK, NDK, OpenJDK, build tools, platform and command-line tools are present.
- Android APK freshness can be checked before G3, proving the ignored APK is newer than tracked Unity build inputs.
- Android APK identity can be checked before G3, proving the package name and launch activity match the install/launch commands.
- Android APK compatibility can be checked before G3, proving min SDK, target SDK and native ABI metadata match the intended Android smoke target.
- Android APK signing can be checked before G3, proving `apksigner verify` and APK Signature Scheme v2 pass before any device install.
- Android APK manifest install-target metadata can be checked before G3, proving permissions stay expected, no required hardware features narrow install targets, and supported screens remain broad.
- Android APK payload can be checked before G3, proving required Unity/IL2CPP native libraries and `assets/bin/Data` runtime files are present.
- Android APK size budget can be checked before G3, proving the early mobile demo package has not accidentally bloated past the current install-readiness budget.
- Android smoke artifact hygiene can be checked before G3, proving APK/AAB outputs, Android smoke logs/screenshots and `Builds/Android` outputs are ignored and absent from tracked/staged paths.
- Android smoke screenshot evidence capture can be previewed before G3, proving the real-device smoke helper will write ignored `analysis-output\android-device-smoke.png` visual evidence.
- Android smoke summary evidence output can be previewed before G3, proving the real-device smoke helper will write ignored `analysis-output\android-device-smoke-summary.json` run metadata.
- Android smoke summary schema can be checked before G3, proving the ignored run metadata has the required fields, package name, timestamp, evidence paths and execution flags.
- Android smoke summary schema is checked inside the direct G3 device-smoke preflight before it reports waiting-on-device or OK.
- Android smoke plan/preflight consistency can be checked before G3, proving plan mode and direct preflight agree on package, activity, evidence paths, execution flags and summary schema readiness.
- Android G3 readiness can be checked before install, bundling device preflight, plan/preflight consistency, smoke plan, log scanner self-test and summary schema self-test in one direct mobile gate.
- Android G3 device requirement can be checked before install, proving strict readiness cannot pass without an authorized Android phone.
- PC visual capture sanity can be checked without launching Unity, proving the six controlled-demo PNG captures have expected dimensions, size, color variety, center visibility, contrast, low magenta fallback color and non-monochrome content.
- PC visual capture sanity self-test can be checked without launching Unity, proving the gate distinguishes valid, flat and magenta fallback sample images.
- PC capture sidecar schema can be checked without launching Unity, proving the six controlled-demo JSON sidecars keep matching screenshot paths, expected dimensions, flow state, camera state, summary fields and reference-asset metadata.
- PC capture preset contract can be checked without launching Unity, proving the standard six controlled-demo presets remain `mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol` across capture generation, evidence, visual sanity, sidecar schema and handoff docs.
- PC capture artifact hygiene can be checked without launching Unity, proving local reference screenshots, JSON sidecars, capture logs and visual sanity self-test images remain ignored generated evidence and are absent from tracked/staged source paths.
- PC window contract can be checked without launching Unity through `check_pc_window_contract.ps1`, reporting `PC window contract check OK`.
- PC launch log hygiene can be checked without launching Unity through `check_pc_launch_log_hygiene.ps1`, proving the controlled launcher writes `analysis-output/windows-demo-run.log` and that local launch logs stay ignored and absent from tracked/staged paths.
- PC build artifact hygiene can be checked without launching Unity through `check_pc_build_artifact_hygiene.ps1`, proving Windows player output stays under `unity-mc2-demo/Builds/Windows/`, is ignored, and is absent from tracked/staged paths.
- PC smoke artifact hygiene can be checked without launching Unity through `check_pc_smoke_artifact_hygiene.ps1`, proving PC smoke, validator, build and saved-account evidence outputs stay under ignored `analysis-output/` paths and out of tracked/staged paths.
- Current plan queue consistency can be checked without launching Unity through `check_current_plan_queue.ps1`, proving README, BUILD-WIN, master/detailed/PC/mobile/evidence/handoff docs and helper scripts agree on the latest PC/mobile/platform checkpoint, `F13 capture opt-in MechBay preview evidence` as complete, and `F14 capture landscape-phone MechLab source-line evidence` as complete, with `F15 plan server-backed receipt slice` complete, `F16 implement server-backed receipt evidence gate` complete, `F17 plan post-receipt inventory refresh boundary` complete, `F18 implement opt-in post-receipt inventory refresh binding` complete, `F19 capture opt-in post-receipt refresh evidence` complete, `F20 refresh Android landscape build/smoke evidence` complete, `F21 audit landscape touch UI ergonomics` complete, `F22 audit landscape MechLab touch controls` complete, `F23 capture landscape MechLab touch evidence` complete, `F24 capture Android MechLab touch evidence` complete, `F25 capture Android battle command touch evidence` complete, with F26 reduce Android combat effect log noise complete, `F27 audit Android entity placeholder collision path` is complete. Evidence gate: `scripts/unity/check_android_entity_placeholder_collision_path.ps1` -> `Android entity placeholder collision path check OK`, and F28 capture Android entity placeholder collision runtime evidence complete, with Evidence gate: `scripts/unity/capture_android_entity_placeholder_collision_runtime_evidence.ps1` -> `Android entity placeholder collision runtime evidence capture OK`, F29 audit PC controlled-demo visual readability complete, with Evidence gate: `scripts/unity/audit_pc_controlled_demo_visual_readability.ps1` -> `PC controlled-demo visual readability audit OK`, and F30 implement PC controlled-demo visual readability fixes complete, with Evidence gate: `scripts/unity/check_pc_controlled_demo_visual_readability_fixes.ps1` -> `PC controlled-demo visual readability fixes check OK`, and F31 refresh PC controlled-demo visual evidence after readability fixes complete; Evidence gate: `scripts/unity/capture_pc_controlled_demo_visual_evidence.ps1` -> `PC controlled-demo visual evidence refresh OK`; F32 audit PC controlled-demo command readability and formation feel complete; Evidence gate: `scripts/unity/audit_pc_controlled_demo_command_readability_formation.ps1` -> `PC controlled-demo command readability formation audit OK`; `F33 implement PC controlled-demo command readability and formation fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_command_readability_fixes.ps1` -> `PC controlled-demo command readability fixes check OK`; `F34 refresh PC controlled-demo command evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F35 audit post-F34 PC controlled-demo playable flow polish` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_playable_flow_polish.ps1` -> `PC controlled-demo playable flow polish audit OK`; `F36 implement post-F34 PC controlled-demo playable flow polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_playable_flow_polish_fixes.ps1` -> `PC controlled-demo playable flow polish fixes check OK`; `F37 refresh PC controlled-demo playable-flow evidence after polish fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F38 audit post-F37 PC controlled-demo investor readiness` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_readiness.ps1` -> `PC controlled-demo investor readiness audit OK`; `F39 implement post-F37 PC controlled-demo investor readiness fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_readiness_fixes.ps1` -> `PC controlled-demo investor readiness fixes check OK`; `F40 refresh PC controlled-demo investor-readiness evidence after fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F41 audit post-F40 PC controlled-demo investor evidence package` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_package.ps1` -> `PC controlled-demo investor evidence package audit OK`; `F42 implement post-F41 PC controlled-demo investor evidence package fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_package_fixes.ps1` -> `PC controlled-demo investor evidence package fixes check OK`; `F43 refresh PC controlled-demo investor evidence package after fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh check OK`; `F44 audit post-F43 PC controlled-demo investor evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh audit OK`; `F45 implement post-F44 PC controlled-demo investor evidence polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_polish_fixes.ps1` -> `PC controlled-demo investor evidence polish fixes check OK`; `F46 refresh PC controlled-demo investor route evidence after polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh check OK`; `F47 audit post-F46 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh audit OK`; formal next task: `F48 implement post-F47 PC controlled-demo investor route evidence audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.
- Android device connection can be checked without launching Unity through `check_android_device_connection.ps1`, proving `adb devices -l` is readable and reports no-device, unauthorized, offline, multi-device or ready states before G3 tries to install or launch the APK.
- Android WPD-only device diagnosis can be checked without launching Unity through `check_android_device_connection.ps1`, proving `WpdOnlyAndroidProbe: True` is emitted and a Windows-only WPD/MTP phone remains a G3 waiting state until adb shows one authorized `device` row.
- Android ADB setup guidance can be checked without launching Unity through `check_android_device_connection.ps1`, proving `AdbSetupHint: True` is emitted with current Windows driver/provider/inf/service when the connected phone is still MTP-only.
- Android ADB driver package probe can be checked without launching Unity through `check_android_adb_driver_package.ps1`, proving `AdbDriverPackageProbe: True`, current phone driver details and installed ADB/WinUSB candidate package state are reported without installing or changing drivers.
- Android ADB readiness watch can be checked without launching Unity through `watch_android_device_connection.ps1 -Once -AllowWaiting`, proving `AdbWatchHint: True` is emitted and the helper can report waiting until adb shows one authorized `device` row or OK after the phone is authorized.
- Android G3 device status report can be written without launching Unity through `write_android_g3_device_status.ps1`, proving `G3DeviceStatusReport: True`, ready/waiting status and blocker details are captured in ignored JSON without install or launch.
- Android G3 when-ready runner can be previewed without launching Unity through `run_android_g3_when_ready.ps1 -PlanOnly`, proving `G3WhenReady: True` and `NoInstallOrLaunchUntilDeviceReady: True` before it waits for adb and calls real smoke.
- Android smoke connection gate wiring can be checked without a device through `android_device_smoke.ps1 -PlanOnly`, proving the real smoke path runs `check_android_device_connection.ps1 -RequireDevice` before install or launch and fails early with `Android device smoke requires a single authorized Android device before install or launch` when no authorized phone is available.
- Android smoke connection gate behavior can be checked without a device through `check_android_smoke_connection_gate.ps1`, proving the real smoke fail-fast path is enforced before install or launch and ignored smoke evidence is not rewritten while no valid device is selected.
- Android visible-flow command-file smoke can be previewed without a device through `android_device_smoke.ps1 -PlanOnly`, proving real G3 will push `mc2_01-visible-flow-audit.txt`, launch with `-mc2CommandFile`, and require debrief/loadout compact success markers.
- Sparse battle HUD can be checked without launching Unity through `check_battle_hud_sparse_contract.ps1`.
- Demo source hygiene can be checked without launching Unity through `check_demo_source_hygiene.ps1`.
- AI deputy contract can be checked without launching Unity or calling the model through `check_ai_deputy_contract.ps1`.
- Android SDK tooling can be checked without launching Unity through `check_android_sdk_tooling.ps1`.
- Android APK freshness can be checked without launching Unity through `check_android_apk_freshness.ps1`.
- Android APK identity can be checked without launching Unity through `check_android_apk_identity.ps1`.
- Android APK compatibility can be checked without launching Unity through `check_android_apk_compatibility.ps1`.
- Android APK signing can be checked without launching Unity through `check_android_apk_signing.ps1`.
- Android APK manifest can be checked without launching Unity through `check_android_apk_manifest.ps1`.
- Android APK payload can be checked without launching Unity through `check_android_apk_payload.ps1`.
- Android APK size budget can be checked without launching Unity through `check_android_apk_size_budget.ps1`.
- Android smoke artifact hygiene can be checked without launching Unity through `check_android_smoke_artifact_hygiene.ps1`.
- Android smoke screenshot evidence can be previewed without a device through `android_device_smoke.ps1 -PlanOnly`, which prints `ScreenshotCapture: True`.
- Android smoke summary evidence can be previewed without a device through `android_device_smoke.ps1 -PlanOnly`, which prints `SummaryWrite: True`.
- Android smoke summary schema can be self-tested without a device through `check_android_smoke_summary.ps1 -SelfTest`.
- Android device-smoke preflight can self-test the summary schema without a device through `check_android_device_preflight.ps1 -AllowNoDevice`.
- Android smoke plan/preflight consistency can be checked without a device through `check_android_smoke_plan_consistency.ps1`.
- Android G3 readiness can be checked without installing through `check_android_g3_readiness.ps1`, reporting waiting-on-device when no authorized phone is connected.
- Android G3 device requirement can be checked without installing through `check_android_g3_device_requirement.ps1`, reporting waiting-on-device when strict readiness only lacks a phone.
- PC visual capture sanity can be checked without launching Unity through `check_pc_visual_capture_sanity.ps1`, reporting `PC visual capture sanity check OK`.
- PC visual capture sanity thresholds can be self-tested without launching Unity through `check_pc_visual_capture_sanity.ps1 -SelfTest`, reporting `PC visual capture sanity self-test OK`.
- PC capture sidecar schema can be checked without launching Unity through `check_pc_capture_sidecar_schema.ps1`, reporting `PC capture sidecar schema check OK`.
- PC capture preset contract can be checked without launching Unity through `check_pc_capture_preset_contract.ps1`, reporting `PC capture preset contract check OK`.
- PC capture artifact hygiene can be checked without launching Unity through `check_pc_capture_artifact_hygiene.ps1`, reporting `PC capture artifact hygiene check OK`.
- No generated screenshot, JSON sidecar, log, Windows build output, APK/AAB, or private reference export is staged.

## Execution Gate Order

| Gate | Status | Purpose | Required Before Next Gate |
| --- | --- | --- | --- |
| PC0 | Done | Existing Windows baseline | Prior validator/build/smoke and visual captures have passed |
| PC1 | Done | Audit current PC baseline | Re-run validator, Windows build, visible-flow smoke and six captures; record exact current weakness |
| PC2 | Done | Polish battle readability | Fix only the highest-impact visible issue from PC1 |
| PC3 | Done | Polish MechLab PC flow | Improve grid/loadout readability without adding weapon toggles |
| PC4 | Done | Package controlled PC demo evidence | Refresh walkthrough/evidence and keep generated artifacts ignored |
| PC5 | Done | Add PC demo launch preflight | Check Windows build presence and launch with stable window args |
| PC6 | Done | Add controlled demo evidence check | Check build, visible-flow log and six capture sidecars |
| PC7 | Done | Add controlled demo public boundary preflight | Check clean project-owned metadata examples and optionally confirm the dev build remains blocked for public packaging |
| PC8 | Done | Add controlled demo readiness preflight | Wrap launch, evidence and public boundary gates into one command |
| PC9 | Done | Add controlled demo handoff consistency check | Check scripts and docs agree on the current controlled demo gate set |
| PC10 | Done | Add Android device smoke preflight | Check APK/tooling/package/device state before the real G3 install/launch smoke |
| PC11 | Done | Add PC core playable contract check | Run Unity/BattleCore validator through a script and require the PC core playable marker |
| PC12 | Done | Add mobile command model preflight | Check sidecar/source/doc markers for the mobile-low-complexity command model without launching Unity |
| PC13 | Done | Add current plan gate check | Run one current-state command for handoff/readiness, mobile command model and Android waiting/ready state |
| PC14 | Done | Add Android smoke log crash scan | Scan logcat for strong crash markers after real-device smoke launch |
| PC15 | Done | Add Android smoke plan mode | Preview the Android device smoke helper's resolved paths and actions without selecting a device |
| PC16 | Done | Add battle HUD sparse contract check | Check source, capture gate and mobile command preflight agree on sparse active-battle HUD without launching Unity |
| PC17 | Done | Add demo source hygiene check | Check tracked/staged paths and ignore markers keep generated evidence, Unity builds and private reference art out of source commits |
| PC18 | Done | Add AI deputy contract check | Check MiniMax stays optional, slow, high-level, rule-fallback guarded, absent from frame loops and not invoked by default smoke |
| PC19 | Done | Add Windows demo build freshness check | Check ignored Windows player output is newer than tracked Unity build inputs and wire it into readiness preflight |
| PC20 | Done | Add controlled demo evidence freshness check | Check visible-flow log and six capture PNG/JSON sidecars are newer than the current Windows build/evidence inputs |
| PC21 | Done | Add controlled demo capture log freshness check | Check six capture logs exist, are fresh, and prove preset, screenshot request and sidecar write markers |
| PC22 | Done | Add Android APK freshness check | Check ignored Android APK is newer than tracked Unity build inputs and wire it into G3 preflight/smoke helpers |
| PC23 | Done | Add Android APK identity check | Check package name and launch activity match expected G3 install/launch identity and wire it into G3 preflight/smoke helpers |
| PC24 | Done | Add Android APK compatibility check | Check min SDK, target SDK and native ABI match expected Android smoke compatibility metadata and wire it into G3 preflight/smoke helpers |
| PC25 | Done | Add Android APK signing check | Check apksigner verification, APK Signature Scheme v2 and signer DN before G3 install/launch |
| PC26 | Done | Add Android APK manifest check | Check permission allowlist, required hardware features and supported screens before G3 install/launch |
| PC27 | Done | Add Android APK payload check | Check Unity/IL2CPP runtime payload and ABI folders before G3 install/launch |
| PC28 | Done | Add Android APK size budget check | Check current APK package size stays within the early mobile demo install-readiness budget |
| PC29 | Done | Add Android SDK tooling check | Check Unity AndroidPlayer SDK, NDK, OpenJDK, build-tools, platform, adb, aapt and apksigner before G3 install/launch |
| PC30 | Done | Add Android smoke artifact hygiene check | Check Android smoke logs/screenshots, APK/AAB outputs and `Builds/Android` paths stay ignored and out of tracked/staged source |
| PC31 | Done | Add Android smoke screenshot evidence capture | Preview and real-device smoke both include ignored screenshot evidence at `analysis-output\android-device-smoke.png` |
| PC32 | Done | Add Android smoke summary evidence output | Preview and real-device smoke both include ignored JSON run metadata at `analysis-output\android-device-smoke-summary.json` |
| PC33 | Done | Add Android smoke summary schema check | Self-test and real-device smoke validate the ignored summary JSON schema before accepting G3 evidence |
| PC34 | Done | Add Android smoke summary preflight check | Direct G3 device-smoke preflight runs the summary schema self-test before reporting waiting-on-device or OK |
| PC35 | Done | Add Android smoke plan/preflight consistency check | Plan mode and direct G3 preflight agree on package, activity, evidence paths, execution flags and summary readiness |
| PC36 | Done | Add Android G3 readiness check | Bundle direct G3 no-install readiness checks before real-device smoke |
| PC37 | Done | Add Android G3 device requirement check | Strict G3 readiness cannot pass without an authorized Android phone |
| PC38 | Done | Add PC visual capture sanity check | Six controlled-demo PNG captures cannot regress to blank, flat, pink-box or low-information images |
| PC39 | Done | Add PC visual capture sanity self-test | The visual sanity gate proves it detects valid, flat and magenta fallback sample images |
| PC40 | Done | Add PC capture sidecar schema check | Six controlled-demo JSON sidecars keep matching screenshot paths, flow, camera, summary fields and reference-asset metadata |
| PC41 | Done | Add PC capture preset contract check | The standard six controlled-demo presets stay consistent across capture, evidence, sanity, schema and docs |
| PC42 | Done | Add PC capture artifact hygiene check | Local reference screenshots, sidecars, logs and visual sanity self-test outputs remain ignored and absent from tracked/staged source paths |
| PC43 | Done | Add PC window contract check | Controlled PC launcher and capture helper keep stable 1280x720 windowed defaults |
| PC44 | Done | Add PC launch log hygiene check | Controlled PC launcher runtime logs stay fixed to the ignored `analysis-output/windows-demo-run.log` path and out of tracked/staged source paths |
| PC45 | Done | Add PC build artifact hygiene check | Windows player output stays fixed to ignored `unity-mc2-demo/Builds/Windows/` paths and out of tracked/staged source paths |
| PC46 | Done | Add PC smoke artifact hygiene check | PC smoke, validator, build and saved-account evidence outputs stay under ignored `analysis-output/` paths and out of tracked/staged source paths |
| PC47 | Done | Add current plan queue consistency check | Current docs and helper scripts agreed that the then-current wait-state package was sealed through PC1-PC47 and that G3 real-device smoke remained next |
| PC48 | Done | Add Android device connection check | `adb devices -l` reports no-device, unauthorized, offline, multi-device or ready state before G3 install/launch |
| PC49 | Done | Wire Android smoke connection gate | `android_device_smoke.ps1` exposes `ConnectionCheck: check_android_device_connection.ps1 -RequireDevice` and real smoke fails before install/launch without one authorized phone |
| PC50 | Done | Add Android smoke connection gate check | `check_android_smoke_connection_gate.ps1` proves real smoke fails before install/launch and leaves smoke log/screenshot/summary evidence unchanged without one authorized phone |
| PC51 | Done | Add Android visible-flow command-file smoke | `android_device_smoke.ps1 -PlanOnly` exposes `CommandFileSmoke: True`, `UnityArguments: -mc2CommandFile`, and both visible-flow success markers for future real-device G3 |
| PC52 | Done | Add Android WPD-only device diagnosis | `check_android_device_connection.ps1` exposes `WpdOnlyAndroidProbe: True` and identifies WPD/MTP-only phones as waiting, not ready |
| PC53 | Done | Add Android ADB setup guidance | `check_android_device_connection.ps1` exposes `AdbSetupHint: True` with current Windows driver/provider/inf/service and next setup action |
| PC54 | Done | Add Android ADB readiness watch | `watch_android_device_connection.ps1 -Once -AllowWaiting` exposes `AdbWatchHint: True`, can wait for adb `device`, and stays no-device-safe while the phone is WPD/MTP-only |
| PC55 | Done | Add Android G3 device status report | `write_android_g3_device_status.ps1` writes ignored JSON and exposes `G3DeviceStatusReport: True`, `G3DeviceReady`, blocker and no-install/no-launch markers |
| PC56 | Done | Add Android G3 when-ready runner | `run_android_g3_when_ready.ps1 -PlanOnly` exposes `G3WhenReady: True` and `NoInstallOrLaunchUntilDeviceReady: True`; real smoke only runs after adb is ready |
| PC57 | Done | Add Android ADB driver package probe | `check_android_adb_driver_package.ps1` exposes `AdbDriverPackageProbe: True`, current phone driver and candidate driver package state without install/launch |

Do not open another PC polish gate from visual inspection alone. If the issue is collision, damage, command state or objective logic, first prove it in `BattleCore`.

## Completed Target: PC1 Audit Current PC Baseline

**Precondition:**

- `git status --short --branch --untracked-files=all` is clean or only contains this plan update.
- Unity exists at `$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe`.
- Windows build output remains ignored under `unity-mc2-demo\Builds\Windows`.
- Private reference visuals, if present, remain local-only and ignored.

**Action:**

1. Run the mission validator.
2. Run the Windows player build.
3. Run visible-flow smoke through the command file.
4. Run the six standard captures.
5. Inspect sidecars and screenshots only enough to choose one next PC polish target.
6. Update `docs-reference-visual-audit-2026-06-07.md` or `docs-playable-demo-investor-evidence-2026-06-07.md` only if the fresh evidence changes the written judgment.

**Commands:**

```powershell
$Repo = (Get-Location).Path
$Unity = "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"

& $Unity `
  -batchmode -quit `
  -projectPath "$Repo\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract `
  -logFile "$Repo\analysis-output\unity-validate-pc-baseline.log"

& $Unity `
  -batchmode -quit `
  -projectPath "$Repo\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 `
  -logFile "$Repo\analysis-output\unity-build-pc-baseline.log"

& "$Repo\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" `
  -batchmode -nographics `
  -mc2SmokeTest `
  -mc2CommandFile "$Repo\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" `
  -logFile "$Repo\analysis-output\unity-player-pc-visible-flow-baseline.log"

powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Output:**

- ignored validator/build/player logs under `analysis-output/`;
- ignored screenshots and JSON sidecars under `analysis-output/reference-visual-captures/`;
- optional docs note summarizing the next PC polish target;
- no generated artifact staged.

**Verification:**

```powershell
git diff --check
git status --short --branch --untracked-files=all
Select-String -Path .\analysis-output\unity-validate-pc-baseline.log -Pattern "MC2 demo contract validation OK"
Select-String -Path .\analysis-output\unity-build-pc-baseline.log -Pattern "Build Finished, Result: Success","MC2 Unity demo Windows build OK"
Select-String -Path .\analysis-output\unity-player-pc-visible-flow-baseline.log -Pattern "MC2 demo smoke test exiting with code 0"
```

Expected capture result:

```text
MC2 reference visual captures passed
```

**Failure Handling:**

- Validator failure: fix the smallest BattleCore/contract issue first; do not mask with Unity visuals.
- Windows build failure: fix compile or Unity build setting issue before visual work.
- Visible-flow smoke failure: inspect command state and startup flow; do not continue to screenshots until smoke passes.
- Capture failure: classify whether the issue is blank frame, UI regression, contact overlap, missing asset fallback, or unreadable framing.
- Generated artifacts in git: leave them ignored or unstage them; do not commit logs, screenshots, sidecars or builds.

**Commit Scope:**

- Allowed: plan docs, evidence docs, minimal script fix if PC smoke/capture command is broken.
- Not allowed: `analysis-output/`, `unity-mc2-demo/Builds/`, generated screenshots, JSON sidecars, logs, private reference exports.

**Completed Evidence 2026-06-11:**

```text
analysis-output/unity-validate-pc-baseline.log: MC2 demo contract validation OK.
analysis-output/unity-build-pc-baseline.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
analysis-output/unity-player-pc-visible-flow-baseline.log: MC2 demo smoke test exiting with code 0.
capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol: MC2 reference visual captures passed: 6 preset(s).
docs-reference-visual-audit-2026-06-07.md records the PC1 baseline and selects PC2 terrain/water/land readability as the next highest-impact polish target.
```

**Commit:** `Audit PC demo baseline`

## Completed Target: PC2 Polish Battle Readability

**Goal:** 根据 PC1 证据，只修一个最高影响的战场可读性问题：地形、水域、岸线、道路/跑道和可战斗陆地区域在默认 PC 镜头下必须更清楚。

**Likely Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoStructureView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `scripts/unity/capture_reference_visuals.ps1`
- Modify if needed: `docs-reference-visual-audit-2026-06-07.md`

**Executable Requirements:**

| ID | Requirement | Pass Standard |
| --- | --- | --- |
| PC2-R1 | Terrain and water are distinct | `spawn`, `airfield` and
orth-patrol` do not read as one broad blue field with yellow-green noise |
| PC2-R2 | Units have readable silhouettes | player squad and first enemies are distinguishable at default camera |
| PC2-R3 | Contact is not a visual pile | `hangar-contact` sidecar still reports no true overlaps |
| PC2-R4 | Damage story remains visible | `damage-demo` still shows section damage/ejection/wreck cues |
| PC2-R5 | Sparse HUD is preserved | no large log, save, account or debug overlay appears in normal battle |

**Validation:**

```powershell
git diff --check
& "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "$PWD\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "$PWD\analysis-output\unity-build-pc-readability.log"
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo
```

**Completed Evidence 2026-06-11:**

```text
analysis-output/unity-build-pc-terrain-readability.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol: MC2 reference visual captures passed: 5 preset(s).
sidecar terrainReadability: texture=composite textureStrength=0.28 waterSurface=readable-overlay alpha=0.48 style=land-outline+runway-contrast+water-muted pathing=unchanged.
sidecar contactClearance: all five PC2 battle captures still report overlaps=0 status=separated.
```

**Commit:** `Polish PC battle readability`

## Completed Target: PC3 Polish MechLab PC Flow

**Goal:** 让 PC 端装配界面更接近“整块装备放入格子”的直观感觉，不回退到武器启用/关闭。

**Likely Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Modify if needed: `scripts/unity/capture_reference_visuals.ps1`

**Executable Requirements:**

| ID | Requirement | Pass Standard |
| --- | --- | --- |
| PC3-R1 | Weapon blocks are whole shapes | mounted weapons occupy their full shape visually |
| PC3-R2 | All mounted weapons are active | there is no enable/disable weapon toggle |
| PC3-R3 | Heat and weight pressure are visible | legal/illegal state is understandable without a long explanation |
| PC3-R4 | Armor/sink fillers remain simple | single-cell fillers are clear and do not become a second game system |
| PC3-R5 | Layout fits desktop demo | no text overlap at 1280x720 and normal Windows player size |

**Validation:**

```powershell
git diff --check
& "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "$PWD\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "$PWD\analysis-output\unity-build-pc-mechlab.log"
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\capture_reference_visuals.ps1 -Presets mechlab
```

**Completed Evidence 2026-06-11:**

```text
analysis-output/unity-build-pc-mechlab.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
capture_reference_visuals.ps1 -Presets mechlab: MC2 reference visual captures passed: 1 preset(s).
sidecar mechLab: layout=pressure-cards+whole-blocks+single-fillers, alwaysMounted=weapons 6/6 items 6/6 noToggle=yes.
```

**Commit:** `Polish PC MechLab flow`

## Completed Target: PC4 Package Controlled PC Demo Evidence

**Goal:** 在 PC 可展示质量收稳后，刷新演示脚本和证据页，方便拿给外部人看。

**Files:**

- Modify: `docs-playable-demo-walkthrough-2026-06-07.md`
- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`
- Modify if needed: `README.md`
- Read: `BUILD-WIN.md`

**Validation:**

```powershell
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Docs describe what the PC demo can actually show today.
- Docs do not imply private reference assets are public-safe final art.
- The demo can be run from the Windows build path without explaining internal scripts to the viewer.

**Completed Evidence 2026-06-12:**

```text
analysis-output/unity-build-pc-evidence-package.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
analysis-output/unity-player-pc-evidence-visible-flow.log: MC2 demo smoke test exiting with code 0; debrief, repair/Mech Lab, relaunch identity and compact loadout checks passed.
capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol: MC2 reference visual captures passed: 6 preset(s).
sidecar terrainReadability: texture=composite textureStrength=0.28 waterSurface=readable-overlay alpha=0.48 style=land-outline+runway-contrast+water-muted pathing=unchanged.
sidecar mechLab: layout=pressure-cards+whole-blocks+single-fillers, pressure=H 12/22 W 16/16 G 12/16, alwaysMounted=weapons 6/6 items 6/6 noToggle=yes.
sidecar contactClearance: all five battle captures report overlaps=0 status=separated; evidence tolerance now treats sub-1-unit clearance jitter as touching, not gameplay overlap.
docs-playable-demo-walkthrough-2026-06-07.md and docs-playable-demo-investor-evidence-2026-06-07.md describe the current PC demo without claiming private reference visuals are public-safe final art.
```

**Commit:** `Package PC controlled demo evidence`

## Completed Target: PC5 Add PC Demo Launch Preflight

**Goal:** 在 G3 真机仍不可用时，只收稳当前 Windows 受控演示启动入口，不改玩法。

**Files:**

- Create: `scripts/unity/run_windows_demo.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_windows_demo.ps1 -CheckOnly
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- `-CheckOnly` validates the Windows executable and Unity data folder without launching a player.
- Normal launch passes `-screen-width 1280 -screen-height 720 -screen-fullscreen 0`.
- Runtime log path stays ignored under `analysis-output/windows-demo-run.log`.
- No gameplay, BattleCore, HUD, MechLab, content-pack or generated evidence behavior changes.

**Commit:** `Add PC demo launch preflight`

## Completed Target: PC6 Add Controlled Demo Evidence Check

**Goal:** 在 G3 真机仍不可用时，只把当前 Windows 受控演示证据包变成可机器检查状态，不改玩法。

**Files:**

- Create: `scripts/unity/check_controlled_demo_evidence.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_evidence.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Checks Windows build and Unity data folder.
- Checks visible-flow log for smoke exit `0`, debrief summary and compact loadout assertions.
- Checks six capture PNG/JSON pairs, MechLab no-toggle fitting, terrain readability, sparse HUD, contact separation and damage-demo section story.
- Reads ignored local evidence only and does not create artifacts.
- Does not change gameplay, BattleCore, HUD, MechLab, content packs or generated evidence behavior.

**Commit:** `Add controlled demo evidence check`

## Completed Target: PC7 Add Controlled Demo Public Boundary Preflight

**Goal:** 在 G3 真机仍不可用时，只把当前受控演示的公开内容边界做成可机器检查状态，不改玩法、不生成素材、不把开发构建误标成 public-safe。

**Files:**

- Create: `scripts/content-pack/check_controlled_demo_public_boundary.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs and evidence page

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\content-pack\check_controlled_demo_public_boundary.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\content-pack\check_controlled_demo_public_boundary.ps1 -CheckDevBuild
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Checks `project-owned-starter`, `project-owned-text-safe-slice`, `project-owned-visual-slice`, and `project-owned-art-safe-slice`.
- Requires each clean metadata target to return `Result: OK`.
- With `-CheckDevBuild`, confirms the current Windows development build returns expected `Result: FAILED`.
- Reads existing files only and does not create artifacts.
- Does not change gameplay, BattleCore, HUD, MechLab, content packs or generated evidence behavior.

**Commit:** `Add controlled demo public boundary preflight`

## Completed Target: PC8 Add Controlled Demo Readiness Preflight

**Goal:** 在 G3 真机仍不可用时，只把 PC 受控演示前的检查入口收成一条命令，不改玩法、不启动 Unity、不重跑截图。

**Files:**

- Create: `scripts/unity/check_controlled_demo_readiness.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs and evidence page

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Runs Windows launch preflight in `-CheckOnly` mode.
- Runs controlled demo evidence health check.
- Runs controlled demo public boundary preflight with expected dev-build failure check.
- Reports one top-level readiness result and per-step OK rows.
- Does not start Unity, rebuild, regenerate screenshots, alter BattleCore, change HUD/MechLab behavior, or stage generated artifacts.

**Commit:** `Add controlled demo readiness preflight`

## Completed Target: PC9 Add Controlled Demo Handoff Consistency Check

**Goal:** 在 G3 真机仍不可用时，只把换机和演示前最容易漂移的文档/脚本入口做成可机器检查状态，不改玩法、不启动 Unity、不重跑截图。

**Files:**

- Create: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: `docs-machine-handoff-plan-2026-06-07.md`
- Modify: current plan docs and evidence page

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Checks key controlled demo scripts exist.
- Checks README, BUILD-WIN, current master plan, current detailed plan, PC optimization plan, investor evidence and machine handoff plan all mention the current controlled demo gate set.
- Rejects stale machine-handoff markers such as the old G2 next-task checkpoint, old ahead-count and old reward-contract commit.
- `-RunReadiness` proves the handoff check can also call the full readiness preflight.
- Does not start Unity, rebuild, regenerate screenshots, alter BattleCore, change HUD/MechLab behavior, or stage generated artifacts.

**Commit:** `Add controlled demo handoff consistency check`

## Completed Target: PC10 Add Android Device Smoke Preflight

**Goal:** 在 G3 真机仍不可用时，只把 Android 真机 smoke 的前置条件做成可机器检查状态，证明当前链路只缺授权设备，不提前做 G4/G5。

**Files:**

- Create: `scripts/unity/check_android_device_preflight.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-MOBILE.md`
- Modify: `README.md`
- Modify: current plan docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Checks Android APK, adb and aapt.
- Extracts package name and launchable activity from the APK.
- Strict mode fails when no authorized Android device is connected.
- `-AllowNoDevice` passes the current waiting state with an explicit waiting-on-device message.
- Does not install, launch, rebuild, capture logs, alter BattleCore, change HUD/MechLab behavior, or stage generated artifacts.

**Commit:** `Add Android device smoke preflight`

## Completed Target: PC11 Add PC Core Playable Contract Check

**Goal:** 在 G3 真机仍不可用时，不继续扩大 PC 玩法，只把受控演示最核心的规则状态做成单独可运行的机器检查。

**Files:**

- Create: `scripts/unity/check_pc_core_playable_contract.ps1`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs, evidence page and handoff plan

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_core_playable_contract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Runs the Unity editor validator from a single command.
- Requires `MC2 PC core playable contract OK`.
- Requires `MC2 demo contract validation OK`.
- Keeps generated validator logs under ignored `analysis-output/`.
- Does not launch the player, rebuild, regenerate screenshots, alter HUD/MechLab behavior, install Android packages, or stage generated artifacts.

**Commit:** `Add PC core playable contract check`

## Completed Target: PC12 Add Mobile Command Model Preflight

**Goal:** 在 G3 真机仍不可用时，不提前做触控 UI；先把 PC 演示当前指挥面是否仍能迁移到移动端做成单独机器检查。

**Files:**

- Create: `scripts/unity/check_mobile_command_model_preflight.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs, evidence page and handoff plan

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Reads existing ignored capture sidecars and tracked source/docs only.
- Requires battle sidecars to keep status rows, Jet, map, bay/system, compact objective, closed mission map, hidden combat log, hidden account UI, disabled save UI and hidden overlays.
- Requires MechLab sidecar to keep whole-block fitting, all mounted weapons active and no weapon enable/disable toggles.
- Keeps generated artifacts untouched and does not launch Unity, rebuild, install Android packages or alter gameplay.

**Commit:** `Add mobile command model preflight`

## Completed Target: PC13 Add Current Plan Gate Check

**Goal:** 在 G3 真机仍不可用时，不继续扩大 PC 玩法；把当前计划状态收成一条可重复命令，便于每次继续前确认可交接、可演示、移动指挥模型未回退，且 Android 只差授权设备。

**Files:**

- Create: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs, evidence page and handoff plan

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Runs handoff/readiness, mobile command model and Android device-smoke preflight checks.
- Accepts either a ready Android device or the explicit waiting-on-device state.
- Does not launch Unity, rebuild, regenerate screenshots, install Android packages, alter gameplay or stage generated artifacts.

**Commit:** `Add current plan gate check`

## Completed Target: PC14 Add Android Smoke Log Crash Scan

**Goal:** 在 G3 真机仍不可用时，不越过 G3；先增强真机 smoke 的失败判定，让设备到位后能自动识别 logcat 里的强崩溃信号。

**Files:**

- Create: `scripts/unity/check_android_smoke_log.ps1`
- Modify: `scripts/unity/android_device_smoke.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-MOBILE.md`
- Modify: README/plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_log.ps1 -SelfTest
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- `android_device_smoke.ps1` scans logcat after capture unless `-SkipLogCheck` is used.
- Scanner catches fatal exception, fatal signal, `SIGSEGV`, `SIGABRT`, ANR for the package, package process death, forced activity finish and Unity crash marker.
- `-SelfTest` proves both clean and crash samples without requiring a device.
- Current plan gate includes the scanner self-test.

**Commit:** `Add Android smoke log crash scan`

## Completed Target: PC15 Add Android Smoke Plan Mode

**Goal:** 在 G3 真机仍不可用时，不越过 G3；让真机 smoke helper 可以无设备预演，提前证明 APK/tool/package/activity/log path 和动作开关解析正确。

**Files:**

- Modify: `scripts/unity/android_device_smoke.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-MOBILE.md`
- Modify: README/plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- `-PlanOnly` exits before adb device selection and performs no install, launch or logcat capture.
- It prints `Android device smoke plan OK`.
- It resolves APK, adb, aapt, package, activity or monkey fallback, log path and install/launch/log-check switch state.
- Current plan gate includes the plan mode.

**Commit:** `Add Android smoke plan mode`

## Completed Target: PC16 Add Battle HUD Sparse Contract Check

**Goal:** 在 G3 真机仍不可用时，不扩大玩法；把“战斗中不用显示太多信息”做成源码级和 capture gate 级合约，避免普通战斗 HUD 重新长出大日志、存档、账号或调试覆盖层。

**Files:**

- Create: `scripts/unity/check_battle_hud_sparse_contract.ps1`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_battle_hud_sparse_contract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The script reads current Unity presentation source, capture gate and mobile command model preflight without launching Unity.
- `SparseBattleUiRegressionSummaryOk` requires `missionMap=available-closed`.
- `capture_reference_visuals.ps1` fails battle HUD sidecars that do not report `missionMap=available-closed`.
- Current plan gate includes the sparse HUD contract check.

**Commit:** `Add battle HUD sparse contract check`

## Completed Target: PC17 Add Demo Source Hygiene Check

**Goal:** 在 G3 真机仍不可用时，不扩大玩法；把“生成截图、日志、Unity build、APK/AAB、私有参考导出不能进入源码提交”做成轻量检查，避免后续换机、演示或提交时污染仓库。

**Files:**

- Create: `scripts/unity/check_demo_source_hygiene.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The script checks tracked and staged paths without requiring a clean working tree.
- It fails tracked/staged generated evidence, Unity builds, APK/AAB outputs, logs, private reference art, non-example content packs and reference export paths.
- It checks `.gitignore` still contains the expected generated/private-output markers.
- Current plan gate includes the demo source hygiene check.

**Commit:** `Add demo source hygiene check`

## Completed Target: PC18 Add AI Deputy Contract Check

**Goal:** 在 G3 真机仍不可用时，不扩大玩法、不消耗模型 token；把 AI 副官只做慢频高层决策、本地规则继续负责具体战斗的边界做成轻量机器检查。

**Files:**

- Create: `scripts/unity/check_ai_deputy_contract.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_ai_deputy_contract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The script reads source, command files and docs without launching Unity.
- The check performs no MiniMax/API request and requires no key.
- MiniMax prompt remains directive-only, slow and high-level; no coordinates, unit ids, JSON, markdown or tactical micro output.
- Startup MiniMax use remains opt-in through `-mc2MinimaxCommanderSteps`, clamped to a small count, and falls back to local rules when unavailable.
- Normal visible-flow/demo scripts do not request MiniMax steps by default.
- Unity frame loops do not instantiate or call MiniMax commander logic.
- Current plan gate includes the AI deputy contract check.

**Commit:** `Add AI deputy contract check`

## Completed Target: PC19 Add Windows Demo Build Freshness Check

**Goal:** 在 G3 真机仍不可用时，不扩大玩法；把“可展示 Windows player 是否对应当前 Unity 输入”做成轻量检查，避免拿旧构建做受控演示。

**Files:**

- Create: `scripts/unity/check_windows_demo_build_freshness.ps1`
- Modify: `scripts/unity/check_controlled_demo_readiness.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_windows_demo_build_freshness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The script reads tracked Unity `Assets`, `ProjectSettings` and `Packages` inputs plus ignored Windows player outputs.
- It fails if the Windows player output is missing or older than tracked Unity build inputs.
- It excludes the generated scene file from timestamp freshness because the builder can rewrite scene fileIDs after build.
- It does not launch Unity or create artifacts.
- `check_controlled_demo_readiness.ps1` includes the build freshness gate.
- Generated Windows build output remains ignored and unstaged.

**Commit:** `Add Windows demo build freshness check`

## Completed Target: PC20 Add Controlled Demo Evidence Freshness Check

**Goal:** 在 G3 真机仍不可用时，不扩大玩法；把受控演示证据包从“文件存在且内容正确”加强为“文件存在、内容正确且晚于当前 Windows build/证据输入”，避免旧日志旧截图误判为当前可展示状态。

**Files:**

- Modify: `scripts/unity/check_controlled_demo_evidence.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_evidence.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The evidence checker fails if `unity-player-pc-evidence-visible-flow.log` is older than the current Windows build or visible-flow command file.
- It fails if any standard capture PNG/JSON sidecar is older than the current Windows build or `capture_reference_visuals.ps1`.
- It still checks visible-flow success, debrief, loadout compact assertion, six capture sidecars, MechLab no-toggle, terrain readability, sparse HUD, contact separation and damage story.
- It does not launch Unity or create artifacts.
- Fresh ignored visible-flow and six capture outputs exist locally after validation.
- Generated evidence remains ignored and unstaged.

**Commit:** `Add controlled demo evidence freshness check`

## Completed Target: PC21 Add Controlled Demo Capture Log Freshness Check

**Goal:** 在 G3 真机仍不可用时，不扩大玩法；把六个标准截图的 `.log` 也纳入受控演示证据检查，确保截图失败或视觉异常时有对应的当前运行日志可追溯。

**Files:**

- Modify: `scripts/unity/check_controlled_demo_evidence.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_evidence.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The evidence checker fails if any standard capture log is missing.
- It fails if any standard capture log is older than the current Windows build or `capture_reference_visuals.ps1`.
- It requires each log to contain the matching capture preset, screenshot request and sidecar write markers.
- It does not launch Unity or create artifacts.
- Generated evidence remains ignored and unstaged.

**Commit:** `Add controlled demo capture log freshness check`

## Completed Target: PC22 Add Android APK Freshness Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 G3 依赖的 ignored Android APK 是否落后于当前 Unity 输入做成机器检查，避免设备到位后安装旧包。

**Files:**

- Create: `scripts/unity/check_android_apk_freshness.ps1`
- Modify: `scripts/unity/check_android_device_preflight.ps1`
- Modify: `scripts/unity/android_device_smoke.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: README/BUILD-MOBILE/BUILD-WIN/current plans/evidence/handoff docs
- Refresh ignored local output: `unity-mc2-demo/Builds/Android/MC2UnityDemo.apk`

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_freshness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The freshness checker fails if `MC2UnityDemo.apk` is missing, empty or older than tracked Unity `Assets`, `ProjectSettings` or `Packages` inputs.
- Generated scene fileID churn is excluded from the timestamp anchor, matching the Windows freshness policy.
- `check_android_device_preflight.ps1 -AllowNoDevice` reports APK freshness OK before stopping at waiting-on-device.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke both reject stale APK before install/launch.
- `check_current_plan_gate.ps1` includes an explicit Android APK freshness gate.
- Refreshed APK remains ignored and unstaged.

**Commit:** `Add Android APK freshness check`

## Completed Target: PC23 Add Android APK Identity Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 G3 安装/启动依赖的 APK 包名和 launch activity 做成机器检查，避免设备到位后安装错包或启动错入口。

**Files:**

- Create: `scripts/unity/check_android_apk_identity.ps1`
- Modify: `scripts/unity/check_android_device_preflight.ps1`
- Modify: `scripts/unity/android_device_smoke.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: README/BUILD-MOBILE/BUILD-WIN/current plans/evidence/handoff/mobile docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_identity.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The identity checker fails if `aapt` cannot read APK badging.
- It fails unless package name is `com.DefaultCompany.unitymc2demo`.
- It fails unless launch activity is `com.unity3d.player.UnityPlayerGameActivity`.
- `check_android_device_preflight.ps1 -AllowNoDevice` reports APK identity OK before stopping at waiting-on-device.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke both reject wrong APK identity before install/launch.
- `check_current_plan_gate.ps1` includes an explicit Android APK identity gate.
- No APK, log or generated output is staged.

**Commit:** `Add Android APK identity check`

## Completed Target: PC24 Add Android APK Compatibility Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 G3 安装前的 Android 兼容性元数据做成机器检查，避免设备到位后才发现 SDK 或 ABI 配置漂移。

**Scope:**

- Add `scripts/unity/check_android_apk_compatibility.ps1`.
- Read APK badging through Unity Android SDK `aapt`.
- Require `minSdkVersion` 25.
- Require `targetSdkVersion` 36.
- Require native ABI `arm64-v8a`.
- Wire compatibility into `check_android_device_preflight.ps1`, `android_device_smoke.ps1`, and `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the compatibility pass aligned with the then-current PC/mobile wait-state status.

**Acceptance:**

- The Android APK compatibility checker fails if `aapt` cannot read APK badging.
- It fails unless `minSdkVersion` is 25.
- It fails unless `targetSdkVersion` is 36.
- It fails unless native-code ABI is exactly `arm64-v8a`.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks APK compatibility before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject incompatible APK metadata before install/launch.
- `check_current_plan_gate.ps1` includes an explicit Android APK compatibility gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_compatibility.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android APK compatibility check`

## Completed Target: PC25 Add Android APK Signing Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 Android 安装前的 APK 签名有效性做成机器检查，避免设备到位后才发现签名包无法安装。

**Scope:**

- Add `scripts/unity/check_android_apk_signing.ps1`.
- Run Unity Android SDK `apksigner verify --verbose --print-certs`.
- Require the APK verifies successfully.
- Require APK Signature Scheme v2 verification.
- Require the current debug signer DN `C=US, O=Android, CN=Android Debug`.
- Record the signer SHA-256 digest for visibility without locking it as a cross-machine requirement.
- Wire signing into `check_android_device_preflight.ps1`, `android_device_smoke.ps1`, and `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the signing pass aligned with the then-current PC/mobile wait-state status.

**Acceptance:**

- The Android APK signing checker fails if `apksigner` cannot verify the APK.
- It fails unless the APK verifies.
- It fails unless APK Signature Scheme v2 is verified.
- It fails unless the signer DN is `C=US, O=Android, CN=Android Debug`.
- It prints the signer SHA-256 digest for diagnosis.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks APK signing before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject unsigned or wrongly signed APK metadata before install/launch.
- `check_current_plan_gate.ps1` includes an explicit Android APK signing gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_signing.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android APK signing check`

## Completed Target: PC26 Add Android APK Manifest Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 Android 安装前的 APK manifest 安装范围做成机器检查，避免设备到位后才发现权限、硬件 feature 或屏幕支持配置漂移。

**Scope:**

- Add `scripts/unity/check_android_apk_manifest.ps1`.
- Read APK badging through Unity Android SDK `aapt`.
- Require permissions to stay within the current allowlist:
  - `android.permission.INTERNET`
  - `com.DefaultCompany.unitymc2demo.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION`
- Fail if any required hardware feature appears.
- Require `android.hardware.touchscreen` and `android.hardware.vulkan.version` to remain not-required features.
- Require screen support for `small`,
ormal`, `large`, and `xlarge`.
- Wire manifest checking into `check_android_device_preflight.ps1`, `android_device_smoke.ps1`, and `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the then-current PC/mobile wait-state status aligned with the manifest checkpoint.

**Acceptance:**

- The Android APK manifest checker fails if `aapt` cannot read APK badging.
- It fails if unexpected permissions appear or expected permissions disappear.
- It fails if any required hardware feature appears.
- It fails unless touchscreen and Vulkan are not-required features.
- It fails unless all four screen classes are supported.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks APK manifest before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject manifest drift before install/launch.
- `check_current_plan_gate.ps1` includes an explicit Android APK manifest gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_manifest.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android APK manifest check`

## Completed Target: PC27 Add Android APK Payload Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 Android APK 的 Unity/IL2CPP 运行载荷做成机器检查，避免设备到位后才发现 APK 缺 native library、`assets/bin/Data` 或 ABI 目录异常。

**Scope:**

- Add `scripts/unity/check_android_apk_payload.ps1`.
- Read the APK as a zip without launching Unity.
- Require one ABI folder: `arm64-v8a`.
- Require the core APK entries, IL2CPP native libraries, Unity metadata and scene/runtime data entries.
- Fail if `assets/bin/Data` or `lib` entries look truncated.
- Wire payload checking into `check_android_device_preflight.ps1`, `android_device_smoke.ps1`, and `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the then-current PC/mobile wait-state status aligned with the payload checkpoint.

**Acceptance:**

- The Android APK payload checker fails if the APK is missing.
- It fails if ABI folders drift from `arm64-v8a`.
- It fails if required native libraries or Unity data files are missing.
- It fails if data/native entry counts look too small for the Unity player output.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks APK payload before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject payload drift before install/launch.
- `check_current_plan_gate.ps1` includes an explicit Android APK payload gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_payload.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android APK payload check`

## Completed Target: PC28 Add Android APK Size Budget Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 Android APK 包体大小做成机器检查，避免设备到位后才发现误打包素材或构建产物异常导致安装包暴涨。

**Scope:**

- Add `scripts/unity/check_android_apk_size_budget.ps1`.
- Require the APK to be at least 1 MiB, catching implausibly truncated output.
- Require the APK to stay at or below 100 MiB for the current early mobile demo.
- Wire size-budget checking into `check_android_device_preflight.ps1`, `android_device_smoke.ps1`, and `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the then-current PC/mobile wait-state status aligned with the size-budget checkpoint.

**Acceptance:**

- The Android APK size budget checker fails if the APK is missing.
- It fails if the APK is implausibly small.
- It fails if the APK exceeds the current 100 MiB early mobile demo budget.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks APK size before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject size-budget drift before install/launch.
- `check_current_plan_gate.ps1` includes an explicit Android APK size budget gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_size_budget.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android APK size budget check`

## Completed Target: PC29 Add Android SDK Tooling Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 Android SDK 工具链做成机器检查，避免设备到位后才发现 adb、aapt、apksigner、build-tools、platform、NDK 或 OpenJDK 环境漂移。

**Scope:**

- Add `scripts/unity/check_android_sdk_tooling.ps1`.
- Require Unity AndroidPlayer SDK, NDK and OpenJDK paths to exist.
- Require `build-tools;36.0.0`, `platforms;android-36`, `android.jar`, adb, aapt and apksigner.
- Check stable version output for adb, aapt and apksigner.
- Wire SDK tooling checking into `check_android_device_preflight.ps1`, `android_device_smoke.ps1`, and `check_current_plan_gate.ps1`.
- Updated handoff, mobile and evidence docs for the PC29 checkpoint before the later PC30 hygiene gate superseded the current wait-state seal.

**Acceptance:**

- The Android SDK tooling checker fails if AndroidPlayer, SDK, NDK or OpenJDK is missing.
- It fails if build-tools 36.0.0 or android-36 platform is missing.
- It fails if adb, aapt or apksigner is missing or cannot report a version.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks SDK tooling before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject SDK tooling drift before install/launch.
- `check_current_plan_gate.ps1` includes an explicit Android SDK tooling gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_sdk_tooling.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android SDK tooling check`

## Completed Target: PC30 Add Android Smoke Artifact Hygiene Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 Android 真机 smoke 会产生的 log、截图、APK/AAB 和 `Builds/Android` 输出纳入机器检查，避免换机、演示或提交时把 ignored 生成物误带进源码。

**Scope:**

- Add `scripts/unity/check_android_smoke_artifact_hygiene.ps1`.
- Require `.gitignore` markers for logs, PNG capture outputs, APK/AAB files and Unity `Builds/`.
- Fail if tracked or staged paths contain Android smoke logs/screenshots, APK/AAB files or Android build outputs.
- Wire artifact hygiene into `check_android_device_preflight.ps1`, `check_current_plan_gate.ps1`, and `check_controlled_demo_handoff.ps1`.
- Updated README, BUILD-WIN, BUILD-MOBILE, mobile, evidence and handoff docs for the PC30 checkpoint before the later PC31 screenshot evidence gate superseded the current wait-state seal.

**Acceptance:**

- The Android smoke artifact hygiene checker fails if `.gitignore` no longer ignores logs, PNG outputs, APK/AAB files or Unity builds.
- It fails if Android smoke logs/screenshots, APK/AAB files or Android build outputs are tracked or staged.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks artifact hygiene before reporting the expected waiting-on-device state.
- `check_current_plan_gate.ps1` includes an explicit Android smoke artifact hygiene gate.
- No APK, log, screenshot, sidecar or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke artifact hygiene check`

## Completed Target: PC31 Add Android Smoke Screenshot Evidence Capture

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 Android 真机 smoke 的视觉证据捕获接好。设备到位后，helper 应同时留下 logcat 和启动截图；设备未到位时，`-PlanOnly` 必须证明截图路径和截图开关已接入。

**Scope:**

- Add `-ScreenshotPath` and `-SkipScreenshot` to `scripts/unity/android_device_smoke.ps1`.
- Default screenshot output to ignored `analysis-output\android-device-smoke.png`.
- Capture screenshots through `adb exec-out screencap -p` with binary stream copying.
- Fail real-device smoke if the screenshot file is implausibly small.
- Make `android_device_smoke.ps1 -PlanOnly` print `Screenshot:` and `ScreenshotCapture: True`.
- Update `check_current_plan_gate.ps1` so Android smoke plan mode must include the screenshot markers.
- Updated README, BUILD-WIN, BUILD-MOBILE, mobile, evidence and handoff docs for the PC31 checkpoint before the later PC32 summary evidence gate superseded the current wait-state seal.

**Acceptance:**

- `android_device_smoke.ps1 -PlanOnly` prints `Android device smoke plan OK.`, `Screenshot:` and `ScreenshotCapture: True`.
- Real device smoke writes ignored `analysis-output\android-device-smoke.png` unless `-SkipScreenshot` is passed.
- `check_current_plan_gate.ps1` fails if Android smoke plan mode no longer reports screenshot capture.
- No APK, log, screenshot, sidecar or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke screenshot evidence capture`

## Completed Target: PC32 Add Android Smoke Summary Evidence Output

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 Android 真机 smoke 的结果摘要输出接好。设备到位后，helper 应在 ignored JSON 里记录设备、包、日志、截图、进程和时间戳，避免之后只能从控制台手工摘录。

**Scope:**

- Add `-SummaryPath` and `-SkipSummary` to `scripts/unity/android_device_smoke.ps1`.
- Default summary output to ignored `analysis-output\android-device-smoke-summary.json`.
- Write JSON with result, UTC timestamp, device id, model, Android version, package/activity, process, APK path, log path, screenshot path and execution flags.
- Make `android_device_smoke.ps1 -PlanOnly` print `Summary:` and `SummaryWrite: True`.
- Update `check_current_plan_gate.ps1` so Android smoke plan mode must include summary markers.
- Update README, BUILD-WIN, BUILD-MOBILE, mobile, evidence and handoff docs for the then-current PC32 wait-state status.

**Acceptance:**

- `android_device_smoke.ps1 -PlanOnly` prints `Android device smoke plan OK.`, `Summary:` and `SummaryWrite: True`.
- Real device smoke writes ignored `analysis-output\android-device-smoke-summary.json` unless `-SkipSummary` is passed.
- `check_current_plan_gate.ps1` fails if Android smoke plan mode no longer reports summary output.
- No APK, log, screenshot, summary or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke summary evidence output`

## Completed Target: PC33 Add Android Smoke Summary Schema Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 Android 真机 smoke 的 summary 输出变成可校验证据。真实设备到位后，helper 写完 ignored JSON 后必须立刻校验字段、包名、时间戳、设备/进程、证据路径和执行标记。

**Scope:**

- Add `scripts/unity/check_android_smoke_summary.ps1`.
- Provide `-SelfTest` without requiring a device or creating artifacts.
- Wire the checker into `scripts/unity/android_device_smoke.ps1` after summary write.
- Wire the checker into `scripts/unity/check_current_plan_gate.ps1`.
- Tighten Android smoke artifact hygiene and `.gitignore` for summary JSON.
- Update README, BUILD-WIN, BUILD-MOBILE, mobile, evidence and handoff docs for the then-current PC33 wait-state status.

**Acceptance:**

- `check_android_smoke_summary.ps1 -SelfTest` prints `Android smoke summary check self-test OK`.
- Real device smoke validates `analysis-output\android-device-smoke-summary.json` after writing it unless `-SkipSummary` is passed.
- `check_current_plan_gate.ps1` fails if summary schema self-test no longer passes.
- No APK, log, screenshot, summary or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_summary.ps1 -SelfTest
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke summary schema check`

## Completed Target: PC34 Add Android Smoke Summary Preflight Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 Android 真机 smoke 的直接前置入口补齐。`check_android_device_preflight.ps1 -AllowNoDevice` 应该直接运行 summary schema 自测，而不是只靠 current plan gate 旁路检查。

**Scope:**

- Modify `scripts/unity/check_android_device_preflight.ps1`.
- Run `check_android_smoke_summary.ps1 -SelfTest` during device-smoke preflight.
- Add a `smoke summary schema` OK row to preflight output.
- Update README, BUILD-WIN, BUILD-MOBILE, mobile, evidence and handoff docs for the then-current PC34 checkpoint.

**Acceptance:**

- `check_android_device_preflight.ps1 -AllowNoDevice` prints `Android device smoke preflight waiting on device` when no phone is connected.
- The same output includes `smoke summary schema` and `Android smoke summary check self-test OK`.
- `check_current_plan_gate.ps1` and `check_controlled_demo_handoff.ps1 -RunReadiness` keep passing.
- No APK, log, screenshot, summary or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_summary.ps1 -SelfTest
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke summary preflight check`

## Completed Target: PC35 Add Android Smoke Plan/Preflight Consistency Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 Android smoke 的无设备预演和直接 preflight 做成同一份机器合同，避免 plan mode 和 G3 入口对 package、activity、证据路径或执行开关产生漂移。

**Scope:**

- Add `scripts/unity/check_android_smoke_plan_consistency.ps1`.
- Run `android_device_smoke.ps1 -PlanOnly`.
- Run `check_android_device_preflight.ps1 -AllowNoDevice`.
- Require matching package and launch activity.
- Require ignored log, screenshot and summary output paths.
- Require install, launch, log check, screenshot capture and summary write flags.
- Require the direct preflight to expose smoke summary schema readiness.
- Wire the check into `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs for the then-current PC35 checkpoint.

**Acceptance:**

- `check_android_smoke_plan_consistency.ps1` prints `Android smoke plan/preflight consistency check OK`.
- It passes with no phone connected when the preflight reports waiting-on-device.
- `check_current_plan_gate.ps1` includes an explicit Android smoke plan consistency gate.
- No APK, log, screenshot, summary or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_plan_consistency.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke plan/preflight consistency check`

## Completed Target: PC36 Add Android G3 Readiness Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 Android G3 的无安装前置状态收成一个直接移动 gate，避免每次要人工拼 preflight、plan、log scanner 和 summary schema 检查。

**Scope:**

- Add `scripts/unity/check_android_g3_readiness.ps1`.
- Run `check_android_device_preflight.ps1 -AllowNoDevice`.
- Run `check_android_smoke_plan_consistency.ps1`.
- Run `android_device_smoke.ps1 -PlanOnly`.
- Run `check_android_smoke_log.ps1 -SelfTest`.
- Run `check_android_smoke_summary.ps1 -SelfTest`.
- Report waiting-on-device when the only missing piece is an authorized Android phone.
- Wire readiness checking into `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the then-current PC/mobile wait-state status sealed through the PC36 checkpoint.

**Acceptance:**

- `check_android_g3_readiness.ps1` prints `Android G3 readiness check waiting on device` on the current no-device machine.
- It prints `Android G3 readiness check OK` when the underlying device preflight sees a valid authorized phone.
- `check_current_plan_gate.ps1` includes an explicit Android G3 readiness gate.
- No APK, log, screenshot, summary or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android G3 readiness check`

## Completed Target: PC37 Add Android G3 Device Requirement Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；明确区分“前置环境已准备好但等待设备”和“真实 G3 已完成”。严格 G3 readiness 必须要求授权 Android 手机，不能被无设备 readiness bundle 误判通过。

**Scope:**

- Add `scripts/unity/check_android_g3_device_requirement.ps1`.
- Run `check_android_g3_readiness.ps1 -RequireDevice`.
- Accept the current no-phone machine only as `waiting on device`.
- Accept OK only when strict readiness passes with an authorized phone.
- Fail for non-device readiness failures.
- Wire device requirement checking into `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the then-current PC/mobile wait-state status sealed through the PC37 checkpoint.

**Acceptance:**

- `check_android_g3_device_requirement.ps1` prints `Android G3 device requirement check waiting on device` on the current no-device machine.
- It prints `Android G3 device requirement check OK` only when strict readiness sees an authorized phone.
- `check_current_plan_gate.ps1` includes an explicit Android G3 device requirement gate.
- No APK, log, screenshot, summary or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_device_requirement.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android G3 device requirement check`

## Completed Target: PC38 Add PC Visual Capture Sanity Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把当前受控演示六张 PNG 截图的基本图像质量做成轻量门禁，避免空白、纯色、粉框或低信息量色块被误当成可展示证据。

**Scope:**

- Add `scripts/unity/check_pc_visual_capture_sanity.ps1`.
- Read `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo` and
orth-patrol` PNGs from `analysis-output\reference-visual-captures`.
- Check resolution, PNG size, sampled unique colors, center unique colors, center lit ratio, luminance standard deviation, magenta fallback ratio and near-monochrome ratio.
- Wire visual capture sanity checking into `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the then-current PC/mobile wait-state status sealed through the PC38 checkpoint.

**Acceptance:**

- `check_pc_visual_capture_sanity.ps1` prints `PC visual capture sanity check OK` on the current six captures.
- `check_current_plan_gate.ps1` includes an explicit PC visual capture sanity gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` includes the script and docs markers.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_visual_capture_sanity.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC visual capture sanity check`

## Completed Target: PC39 Add PC Visual Capture Sanity Self-Test

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；强化 PC38 的截图质量门禁，证明它能识别正常图、纯色坏图和粉色 fallback 坏图。

**Scope:**

- Add `-SelfTest` to `scripts/unity/check_pc_visual_capture_sanity.ps1`.
- Generate ignored synthetic images under `analysis-output\pc-visual-sanity-selftest`.
- Validate one multi-color good image, one flat gray bad image and one magenta fallback bad image.
- Wire the self-test into `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the then-current PC/mobile wait-state status sealed through the PC39 checkpoint.

**Acceptance:**

- `check_pc_visual_capture_sanity.ps1 -SelfTest` prints `PC visual capture sanity self-test OK`.
- `check_pc_visual_capture_sanity.ps1` still prints `PC visual capture sanity check OK` on the current six captures.
- `check_current_plan_gate.ps1` includes an explicit PC visual capture sanity self-test gate.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_visual_capture_sanity.ps1 -SelfTest
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_visual_capture_sanity.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC visual capture sanity self-test`

## Completed Target: PC40 Add PC Capture Sidecar Schema Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；强化受控演示证据包，证明六张截图对应的 JSON sidecar 结构没有漂移。

**Scope:**

- Add `scripts/unity/check_pc_capture_sidecar_schema.ps1`.
- Read `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo` and
orth-patrol` sidecars under `analysis-output\reference-visual-captures`.
- Check matching screenshot paths, `1280x720` dimensions, flow state, mission/status fields, nonnegative counters, objective presence, camera vectors, summary fields and reference-asset metadata.
- Wire the checker into `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the then-current PC/mobile wait-state status sealed.

**Acceptance:**

- `check_pc_capture_sidecar_schema.ps1` prints `PC capture sidecar schema check OK`.
- `check_current_plan_gate.ps1` includes an explicit PC capture sidecar schema gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` includes the script and docs markers.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_sidecar_schema.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC capture sidecar schema check`

## Completed Target: PC41 Add PC Capture Preset Contract Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；固定受控演示截图标准集合，避免默认 capture、evidence、sanity、schema 或文档入口各自维护一套不同 preset。

**Scope:**

- Update `scripts/unity/capture_reference_visuals.ps1` default presets to `mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol`.
- Add `scripts/unity/check_pc_capture_preset_contract.ps1`.
- Check the standard preset list across capture helper, evidence checker, visual sanity checker, sidecar schema checker, README, BUILD-WIN and this PC plan.
- Wire the checker into `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the then-current PC/mobile wait-state status sealed.

**Acceptance:**

- `check_pc_capture_preset_contract.ps1` prints `PC capture preset contract check OK`.
- `check_current_plan_gate.ps1` includes an explicit PC capture preset contract gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` includes the script and docs markers.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_preset_contract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC capture preset contract check`

## Completed Target: PC42 Add PC Capture Artifact Hygiene Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把本地重采集截图带来的生成物边界做成独立门禁，避免 `analysis-output\reference-visual-captures`、no-placeholders 截图或 PC 视觉 sanity 自测图误进 tracked/staged 源码。

**Scope:**

- Add `scripts/unity/check_pc_capture_artifact_hygiene.ps1`.
- Check tracked and staged git paths for PC capture artifact directories.
- Check `.gitignore` contains the reference capture, no-placeholders capture, PC visual sanity self-test, PNG and log markers.
- Check existing local PC capture artifacts are ignored by git.
- Wire the checker into `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the then-current PC/mobile wait-state status sealed.

**Acceptance:**

- `check_pc_capture_artifact_hygiene.ps1` prints `PC capture artifact hygiene check OK`.
- `check_current_plan_gate.ps1` includes an explicit PC capture artifact hygiene gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` includes the script and docs markers.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC capture artifact hygiene check`

## Completed Target: PC43 Add PC Window Contract Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 PC 受控演示窗口尺寸固定成机器可检查契约，避免 Windows player 恢复到异常巨大窗口或截图尺寸与演示窗口漂移。

**Scope:**

- Add `scripts/unity/check_pc_window_contract.ps1`.
- Check `run_windows_demo.ps1` default width/height are `1280x720` and it passes `-screen-width`, `-screen-height` and `-screen-fullscreen 0`.
- Check `capture_reference_visuals.ps1` default width/height are `1280x720` and it passes the same windowed Unity arguments.
- Check README, BUILD-WIN and this PC plan explicitly document `1280x720`.
- Run `run_windows_demo.ps1 -CheckOnly` and require the resolved argument line to contain the controlled window settings.
- Wire the checker into `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the then-current PC/mobile wait-state status sealed through PC43.

**Acceptance:**

- `check_pc_window_contract.ps1` prints `PC window contract check OK`.
- `check_current_plan_gate.ps1` includes an explicit PC window contract gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` includes the script and docs markers.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_window_contract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC window contract check`

## Completed Target: PC44 Add PC Launch Log Hygiene Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 PC 受控演示运行日志路径和 Git 卫生固定成机器可检查契约，避免 `analysis-output/windows-demo-run.log` 或其他本地 launch log 被误提交。

**Scope:**

- Add `scripts/unity/check_pc_launch_log_hygiene.ps1`.
- Check `run_windows_demo.ps1` keeps the default runtime log path at `analysis-output\windows-demo-run.log`.
- Check README, BUILD-WIN and this PC plan explicitly document `analysis-output/windows-demo-run.log`.
- Check `.gitignore` keeps launch logs ignored through the shared `*.log` rule.
- Check `git check-ignore` accepts `analysis-output/windows-demo-run.log`.
- Check tracked and staged source paths contain no `analysis-output/*.log` launch logs.
- Wire the checker into `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the then-current PC44 wait-state status sealed.

**Acceptance:**

- `check_pc_launch_log_hygiene.ps1` prints `PC launch log hygiene check OK`.
- `check_current_plan_gate.ps1` includes an explicit PC launch log hygiene gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` includes the script and docs markers.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_launch_log_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC launch log hygiene check`

## Completed Target: PC45 Add PC Build Artifact Hygiene Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 PC Windows player build 输出路径和 Git 卫生固定成机器可检查契约，避免本地 `unity-mc2-demo/Builds/Windows/` player 输出进入源码提交。

**Scope:**

- Add `scripts/unity/check_pc_build_artifact_hygiene.ps1`.
- Check `run_windows_demo.ps1` and `check_windows_demo_build_freshness.ps1` still point to `unity-mc2-demo\Builds\Windows`.
- Check README, BUILD-WIN and this PC plan explicitly document `unity-mc2-demo/Builds/Windows/`.
- Check `.gitignore` keeps Unity build outputs and common player binary artifacts ignored.
- Check `git check-ignore` accepts representative Windows player output files.
- Check tracked and staged source paths contain no `unity-mc2-demo/Build/` or `unity-mc2-demo/Builds/` artifacts.
- Wire the checker into `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the then-current PC45 wait-state status sealed.

**Acceptance:**

- `check_pc_build_artifact_hygiene.ps1` prints `PC build artifact hygiene check OK`.
- `check_current_plan_gate.ps1` includes an explicit PC build artifact hygiene gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` includes the script and docs markers.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_build_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC build artifact hygiene check`

## Completed Target: PC46 Add PC Smoke Artifact Hygiene Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 PC smoke、validator、build 和 saved-account 运行证据的 Git 卫生固定成机器可检查契约，避免 `analysis-output/` 下的本地运行产物进入源码提交。

**Scope:**

- Add `scripts/unity/check_pc_smoke_artifact_hygiene.ps1`.
- Check README, BUILD-WIN and this PC plan explicitly document ignored `analysis-output/` PC smoke output paths.
- Check `.gitignore` keeps `*.log`, `analysis-output/*saved-account*.json` and `analysis-output/*validator*.json` ignored.
- Check `git check-ignore` accepts representative PC player smoke logs, Unity build/validator logs, validator JSON and saved-account JSON outputs.
- Check tracked and staged source paths contain no `analysis-output/*.log`, `analysis-output/*saved-account*.json` or `analysis-output/*validator*.json` artifacts.
- Wire the checker into `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the then-current PC/mobile wait-state status sealed through PC46.

**Acceptance:**

- `check_pc_smoke_artifact_hygiene.ps1` prints `PC smoke artifact hygiene check OK`.
- `check_current_plan_gate.ps1` includes an explicit PC smoke artifact hygiene gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` includes the script and docs markers.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_smoke_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC smoke artifact hygiene check`

## Completed Target: PC47 Add Current Plan Queue Consistency Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把当前计划队列、封口点和正式下一步做成机器可检查契约，避免多文档继续开发时漂移。

**Scope:**

- Add `scripts/unity/check_current_plan_queue.ps1`.
- Check README, BUILD-WIN, master/detailed/PC/mobile/evidence/handoff docs contain the then-current `PC1-PC47`, `Add current plan queue consistency check`, `check_current_plan_queue.ps1`, `Current plan queue consistency check OK` and `G3 Run Android device smoke` markers.
- Check mobile plan still keeps G3 as Waiting on Device and G4/G5 as Later.
- Check handoff docs still list `G3 Run Android device smoke` as the formal next development task.
- Wire the checker into `check_current_plan_gate.ps1`.
- Update handoff consistency markers so the machine handoff also checks PC47.

**Acceptance:**

- `check_current_plan_queue.ps1` prints `Current plan queue consistency check OK`.
- `check_current_plan_gate.ps1` includes an explicit current plan queue consistency gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` includes the script and docs markers.
- No G4/G5 implementation starts before G3 real-device smoke.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add current plan queue consistency check`

## Completed Target: PC48 Add Android Device Connection Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 `adb devices -l` 连接状态做成独立诊断，避免设备接上后才临时区分未连接、未授权、离线或多设备。

**Scope:**

- Add `scripts/unity/check_android_device_connection.ps1`.
- Check adb exists through Unity AndroidPlayer SDK and can print `adb version`.
- Parse `adb devices -l` and report no-device, unauthorized, offline, multi-device selection or ready state.
- Support `-RequireDevice` for strict G3 usage while default mode reports waiting states without failing.
- Wire the checker into `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the current PC/mobile wait-state status sealed through PC48.

**Acceptance:**

- `check_android_device_connection.ps1` prints `Android device connection check waiting on device` on the current no-phone machine.
- `check_current_plan_gate.ps1` includes an explicit Android device connection gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` includes the script and docs markers.
- No Android install, launch, screenshot or log capture is attempted by the connection checker.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_connection.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android device connection check`

## Completed Target: PC49 Wire Android Smoke Connection Gate

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 PC48 的设备连接诊断接到真实 `android_device_smoke.ps1`，确保真实 smoke 在安装、启动、截图或 logcat 前先确认一台授权设备。

**Scope:**

- Add `ConnectionCheck: check_android_device_connection.ps1 -RequireDevice` to `android_device_smoke.ps1 -PlanOnly`.
- Run `check_android_device_connection.ps1 -RequireDevice` inside real `android_device_smoke.ps1` before `adb install` or launch.
- Make `check_android_device_preflight.ps1 -AllowNoDevice` report its device state from the same connection checker.
- Update plan consistency, current gate, handoff consistency and docs to keep the current PC/mobile wait-state status sealed through PC49.

**Acceptance:**

- `android_device_smoke.ps1 -PlanOnly` prints the connection check marker.
- On the current no-phone machine, real `android_device_smoke.ps1` fails before install or launch with `Android device smoke requires a single authorized Android device before install or launch`.
- `check_android_device_preflight.ps1 -AllowNoDevice` reports `Android device smoke preflight waiting on device` with a `device connection` row.
- `check_android_smoke_plan_consistency.ps1` and `check_current_plan_gate.ps1` require the connection marker.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_plan_consistency.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Wire Android smoke connection gate`

## Completed Target: PC50 Add Android Smoke Connection Gate Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 PC49 的严格连接门禁做成独立自测，证明无授权设备时真实 Android smoke 只能在安装/启动前失败，并且不会刷新 smoke log、截图或 summary evidence。

**Scope:**

- Add `scripts/unity/check_android_smoke_connection_gate.ps1`.
- Reuse `check_android_device_connection.ps1` to classify ready, no-device, unauthorized, offline and multi-device states.
- If an authorized device is ready, report that G3 device smoke is ready and do not install or launch.
- If no valid device is selected, run the real `android_device_smoke.ps1` only to prove the strict failure marker.
- Snapshot `analysis-output\android-device-smoke.log`, `analysis-output\android-device-smoke.png` and `analysis-output\android-device-smoke-summary.json` and require them to remain unchanged.
- Wire the new gate into `check_current_plan_gate.ps1`, `check_current_plan_queue.ps1`, handoff consistency, mobile preflight and current docs.

**Acceptance:**

- `check_android_smoke_connection_gate.ps1` prints `Android smoke connection gate check OK`.
- On the current no-phone machine, it prints `Android smoke connection gate check waiting on device`.
- The real smoke failure marker remains `Android device smoke requires a single authorized Android device before install or launch`.
- The smoke log, screenshot and summary output snapshots remain unchanged before a valid device is selected.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_connection_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke connection gate check`

## Completed Target: PC51 Add Android Visible-Flow Command-File Smoke

**Goal:** 真机 G3 仍等待设备，但真实 Android smoke 到位后必须验证核心可见流程，而不是只接受启动和进程存活。

**Acceptance:**

- `android_device_smoke.ps1 -PlanOnly` prints `CommandFileSmoke: True`.
- `android_device_smoke.ps1 -PlanOnly` prints `UnityArguments: -mc2CommandFile`.
- `android_device_smoke.ps1 -PlanOnly` prints `SmokeSuccessMarker: MC2 debrief summary assertion OK`.
- `android_device_smoke.ps1 -PlanOnly` prints `SmokeSuccessMarker: MC2 loadout compact assertion OK`.
- `check_android_smoke_summary.ps1 -SelfTest`, `check_android_smoke_plan_consistency.ps1`, `check_android_g3_readiness.ps1`, current plan gate, handoff and mobile command preflight all accept the new command-file smoke contract.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_summary.ps1 -SelfTest
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_plan_consistency.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android visible-flow command-file smoke`

## Completed Target: PC52 Add Android WPD-Only Device Diagnosis

**Goal:** 手机已经接到 Windows 时，不把 adb 仍无设备行的情况误判成 Unity 或 APK 问题。连接脚本需要把 WPD/MTP-only Android 手机单独报出来。

**Acceptance:**

- `check_android_device_connection.ps1` prints `WpdOnlyAndroidProbe: True`.
- On the current Mi 11 Lite WPD/MTP-only state, the helper can print `WpdOnlyAndroidDevice: True`.
- The script still reports `Android device connection check waiting on device` until adb exposes exactly one authorized `device` row.
- Current plan queue, current plan gate, handoff and mobile command preflight accept PC1-PC53.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_connection.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
```

**Commit:** `Add Android WPD-only device diagnosis`

## Completed Target: PC53 Add Android ADB Setup Guidance

**Goal:** 当手机已经连到 Windows、但仍是 Microsoft MTP driver 而不是 ADB interface 时，连接检查需要给出 driver/provider/inf/service 和下一步 ADB 设置提示。

**Acceptance:**

- `check_android_device_connection.ps1` prints `AdbSetupHint: True`.
- Mi 11 Lite WPD/MTP-only sample output includes `provider=Microsoft`, `inf=wpdmtp.inf` and `service=WUDFWpdMtp`; the current ADB-ready state reports `inf=winusb.inf` and `service=WINUSB`.
- Current plan gate requires both `WpdOnlyAndroidProbe: True` and `AdbSetupHint: True`.
- Current plan queue, handoff and mobile command preflight accept PC1-PC54.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_connection.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
```

**Commit:** `Add Android ADB setup guidance`

## Completed Target: PC54 Add Android ADB Readiness Watch

**Goal:** 手机侧正在切 USB 调试、RSA 授权或 Windows ADB driver 时，需要一个安全等待入口，不安装、不启动 APK，只轮询现有连接检查直到 adb 出现一个授权 `device`。

**Acceptance:**

- `watch_android_device_connection.ps1 -Once -AllowWaiting` prints `AdbWatchHint: True`.
- Mi 11 Lite WPD/MTP-only sample output reports `Android device connection watch waiting on device`; the current ADB-ready state reports `Android device connection watch OK`.
- Current plan gate includes an Android device watch gate and still treats the real G3 smoke as waiting until adb shows one authorized `device` row.
- Current plan queue, handoff and mobile command preflight accept PC1-PC54.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\watch_android_device_connection.ps1 -Once -AllowWaiting
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
```

**Commit:** `Add Android ADB readiness watch`

## Completed Target: PC55 Add Android G3 Device Status Report

**Goal:** Keep G3 as the next real mobile gate while making the current device blocker machine-readable. The report must not install, launch, or mutate Android device state.

**Acceptance:**

- `write_android_g3_device_status.ps1` prints `G3DeviceStatusReport: True`.
- Mi 11 Lite WPD/MTP-only sample output prints `G3DeviceReady: False` and `NoInstallOrLaunch: True`; the current ADB-ready state prints `G3DeviceReady: True` while still not installing or launching.
- The script writes ignored `analysis-output/android-g3-device-status.json` with ready/waiting state, blocker, next gate and raw helper output.
- Current plan queue, current plan gate, handoff and mobile command preflight accept PC1-PC57.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\write_android_g3_device_status.ps1
powershell -NoProfile -Command "Get-Content .\analysis-output\android-g3-device-status.json -Raw | ConvertFrom-Json | Out-Null; 'Android G3 device status JSON parse OK'"
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
```

**Commit:** `Add Android G3 device status report`

## Completed Target: PC56 Add Android G3 When-Ready Runner

**Goal:** Provide one safe G3 entry point that waits for an authorized adb `device` and then calls the real Android smoke path. Plan and waiting modes must not install or launch.

**Acceptance:**

- `run_android_g3_when_ready.ps1 -PlanOnly` prints `Android G3 when-ready plan OK.`.
- The plan output includes `G3WhenReady: True` and `NoInstallOrLaunchUntilDeviceReady: True`.
- `run_android_g3_when_ready.ps1 -TimeoutSeconds 30 -AllowWaiting -LaunchWaitSeconds 75` installs and launches on the ready Mi 11 Lite, then reports `G3WhenReady: True`, `SmokeTestPassed: True` and `status=smokePassed`.
- If adb is ready but a future phone rejects ADB installation with `INSTALL_FAILED_USER_RESTRICTED`, `-AllowWaiting` reports `G3InstallPolicyBlocked: True` and keeps G3 waiting on phone-side USB install permission.
- Current plan queue, current plan gate, handoff and mobile command preflight accept PC1-PC57, `Pass Android G3 device smoke` and `G4 Touch UI pass`.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_android_g3_when_ready.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_android_g3_when_ready.ps1 -TimeoutSeconds 30 -AllowWaiting -LaunchWaitSeconds 75
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
```

**Commit:** `Add Android G3 when-ready runner`

## Completed Target: PC57 Add Android ADB Driver Package Probe

**Goal:** Clarify the WPD/MTP-only blocker with a read-only driver package probe before changing phone, cable, or driver state.

**Acceptance:**

- `check_android_adb_driver_package.ps1` prints `Android ADB driver package probe OK.`.
- It prints `AdbDriverPackageProbe: True` and `NoInstallOrLaunch: True`.
- On the current machine it reports `CandidateDriverPackages: none`.
- It reports the current Mi 11 Lite driver state; WPD/MTP-only samples include `inf=wpdmtp.inf` and `service=WUDFWpdMtp`, while the current ADB-ready state reports `inf=winusb.inf` and `service=WINUSB`.
- Current plan queue, current plan gate, handoff and mobile command preflight accept PC1-PC57.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_adb_driver_package.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
```

**Commit:** `Add Android ADB driver package probe`

## Stop Conditions

Stop and reassess before committing if:

- Android phone becomes available; then run G3 before moving G4/G5 or any mobile-specific work.
- PC optimization would add controls that cannot translate to mobile's simple command model.
- A visual fix hides a BattleCore collision or damage bug instead of fixing the rule.
- Unity scene churn is only fileID noise.
- Any private original-derived file, generated screenshot, JSON sidecar, log or build output is about to be staged.
- Battle UI grows dense again.

## F12 Preview Binding Checkpoint

`F12 implement opt-in inventory-to-MechBay preview binding` is complete. `F13 capture opt-in MechBay preview evidence` is complete. `F14 capture landscape-phone MechLab source-line evidence` is complete. The opt-in gate is `scripts/unity/check_optional_inventory_mechbay_preview_binding.ps1`, with expected success string `Optional inventory-to-MechBay preview binding check OK`; the preview evidence gate is `scripts/unity/capture_inventory_mechbay_preview_evidence.ps1`, with expected success string `Inventory MechBay preview evidence capture OK`; the landscape-phone evidence gate is `scripts/unity/capture_landscape_phone_mechlab_source_line_evidence.ps1`, with expected success string `Landscape-phone MechLab source-line evidence capture OK`. `F15 plan server-backed receipt slice` is complete. Evidence gate: `scripts/unity/check_server_backed_receipt_slice_plan.ps1` -> `Server-backed receipt slice plan check OK`. `F16 implement server-backed receipt evidence gate` is complete. Evidence gate: `scripts/unity/capture_server_backed_receipt_evidence.ps1` -> `Server-backed receipt evidence capture OK`. `F17 plan post-receipt inventory refresh boundary` is complete. Evidence gate: `scripts/unity/check_post_receipt_inventory_refresh_boundary.ps1` -> `Post-receipt inventory refresh boundary check OK`. `F18 implement opt-in post-receipt inventory refresh binding` is complete. Evidence gate: `scripts/unity/check_post_receipt_inventory_refresh_binding.ps1` -> `Post-receipt inventory refresh binding check OK`. `F19 capture opt-in post-receipt refresh evidence` is complete. Evidence gate: `scripts/unity/capture_post_receipt_refresh_evidence.ps1` -> `Post-receipt refresh evidence capture OK`. `F20 refresh Android landscape build/smoke evidence` is complete. `F21 audit landscape touch UI ergonomics` is complete. Evidence gate: `scripts/unity/check_landscape_touch_ui_ergonomics.ps1` -> `Landscape touch UI ergonomics check OK`. `F22 audit landscape MechLab touch controls` is complete. Evidence gate: `scripts/unity/check_landscape_mechlab_touch_controls.ps1` -> `Landscape MechLab touch controls check OK`. `F23 capture landscape MechLab touch evidence` is complete. Evidence gate: `scripts/unity/capture_landscape_mechlab_touch_evidence.ps1` -> `Landscape MechLab touch evidence capture OK`. `F24 capture Android MechLab touch evidence` is complete. Evidence gate: `scripts/unity/capture_android_mechlab_touch_evidence.ps1` -> `Android MechLab touch evidence capture OK`. F25 capture Android battle command touch evidence is complete. Evidence gate: `scripts/unity/capture_android_battle_command_touch_evidence.ps1` -> `Android battle command touch evidence capture OK`. `F26 reduce Android combat effect log noise` is complete. `F27 audit Android entity placeholder collision path` is complete. Evidence gate: `scripts/unity/check_android_entity_placeholder_collision_path.ps1` -> `Android entity placeholder collision path check OK`. `F28 capture Android entity placeholder collision runtime evidence` is complete. Evidence gate: `scripts/unity/capture_android_entity_placeholder_collision_runtime_evidence.ps1` -> `Android entity placeholder collision runtime evidence capture OK`. `F29 audit PC controlled-demo visual readability` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_visual_readability.ps1` -> `PC controlled-demo visual readability audit OK`. `F30 implement PC controlled-demo visual readability fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_visual_readability_fixes.ps1` -> `PC controlled-demo visual readability fixes check OK`. `F31 refresh PC controlled-demo visual evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_visual_evidence.ps1` -> `PC controlled-demo visual evidence refresh OK`; `F32 audit PC controlled-demo command readability and formation feel` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_command_readability_formation.ps1` -> `PC controlled-demo command readability formation audit OK`; `F33 implement PC controlled-demo command readability and formation fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_command_readability_fixes.ps1` -> `PC controlled-demo command readability fixes check OK`; `F34 refresh PC controlled-demo command evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F35 audit post-F34 PC controlled-demo playable flow polish` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_playable_flow_polish.ps1` -> `PC controlled-demo playable flow polish audit OK`; `F36 implement post-F34 PC controlled-demo playable flow polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_playable_flow_polish_fixes.ps1` -> `PC controlled-demo playable flow polish fixes check OK`; `F37 refresh PC controlled-demo playable-flow evidence after polish fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F38 audit post-F37 PC controlled-demo investor readiness` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_readiness.ps1` -> `PC controlled-demo investor readiness audit OK`; `F39 implement post-F37 PC controlled-demo investor readiness fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_readiness_fixes.ps1` -> `PC controlled-demo investor readiness fixes check OK`; `F40 refresh PC controlled-demo investor-readiness evidence after fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F41 audit post-F40 PC controlled-demo investor evidence package` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_package.ps1` -> `PC controlled-demo investor evidence package audit OK`; `F42 implement post-F41 PC controlled-demo investor evidence package fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_package_fixes.ps1` -> `PC controlled-demo investor evidence package fixes check OK`; `F43 refresh PC controlled-demo investor evidence package after fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh check OK`; `F44 audit post-F43 PC controlled-demo investor evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh audit OK`; `F45 implement post-F44 PC controlled-demo investor evidence polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_polish_fixes.ps1` -> `PC controlled-demo investor evidence polish fixes check OK`; `F46 refresh PC controlled-demo investor route evidence after polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh check OK`; `F47 audit post-F46 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh audit OK`; formal next task: `F48 implement post-F47 PC controlled-demo investor route evidence audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.
