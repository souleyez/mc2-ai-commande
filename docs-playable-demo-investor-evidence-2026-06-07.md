# Playable Demo Evidence 2026-06-07

Purpose: one concise evidence page for showing the current Windows playable Demo without committing generated screenshots or JSON sidecars.

The current evidence proves a local playable loop: MechLab fitting, squad launch, sparse tactical command, contact pressure, visible section damage, and a repeatable capture/build path. It is still a prototype evidence pack, not final public art.

## Current Evidence Package Refresh

Refreshed on 2026-06-12 after `Package PC controlled demo evidence`.

| Gate | Result | Evidence |
| --- | --- | --- |
| Windows build | Pass | `analysis-output/unity-build-pc-evidence-package.log` reports `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK`. |
| Visible-flow smoke | Pass | `analysis-output/unity-player-pc-evidence-visible-flow.log` exits with code `0` and proves debrief, repair/Mech Lab, relaunch identity and compact loadout review. |
| Evidence health check | Pass | `scripts/unity/check_controlled_demo_evidence.ps1` reports `Controlled demo evidence check OK` by reading the Windows build, visible-flow log and six capture sidecars. |
| Readiness preflight | Pass | `scripts/unity/check_controlled_demo_readiness.ps1` reports `Controlled demo readiness preflight OK` by wrapping launch preflight, evidence health and public boundary gates. |
| Handoff consistency | Pass | `scripts/unity/check_controlled_demo_handoff.ps1` reports `Controlled demo handoff consistency check OK` by checking the main scripts and handoff docs agree on the current PC gate set. |
| PC core playable contract | Pass | `scripts/unity/check_pc_core_playable_contract.ps1` reports `PC core playable contract check OK` and requires the Unity validator marker for command state, solo return, Jet legality, occupancy, damage/ejection and debrief/relaunch. |
| Mobile command model preflight | Pass | `scripts/unity/check_mobile_command_model_preflight.ps1` reports `Mobile command model preflight OK` and verifies active battle remains sparse and mobile-translatable: status rows, Jet, map/bay/system, compact objective, hidden dense overlays and MechLab no-toggle fitting. |
| Current plan gate | Pass | `scripts/unity/check_current_plan_gate.ps1` reports `Current plan gate check OK` by wrapping handoff/readiness, mobile command model and Android device-smoke preflight state in one command. |
| Android smoke log check | Pass | `scripts/unity/check_android_smoke_log.ps1 -SelfTest` reports `Android smoke log check self-test OK`; real-device smoke now scans captured logcat for strong crash markers before accepting launch. |
| Android device preflight | Waiting on Device | `scripts/unity/check_android_device_preflight.ps1 -AllowNoDevice` reports APK, adb, aapt, package and launch activity are ready, then stops at waiting on an authorized Android phone. |
| Six capture presets | Pass | `capture_reference_visuals.ps1` reports `MC2 reference visual captures passed: 6 preset(s)` for `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo`, and `north-patrol`. |
| MechLab fitting | Pass | `mechlab` capture remains the fitting proof: whole weapon block, armor/cooling filler, H/W/G pressure, `Fit OK`, `CellState=OK OPEN4 OCC12 OCC!0 OOB0`, and `noToggle=yes`. |
| First-map visual gate | Pass | `spawn`, `airfield`, `hangar-contact`, `damage-demo`, and `north-patrol` sidecars report `FirstMapVisual ... status=ready`, readable terrain, sparse HUD, occupancy, and separated contact. |
| Contact and collision | Pass | Current battle sidecars report `ContactClearance ... overlaps=0 ... status=separated`, including dense objective and north patrol presets; the sidecar tolerance treats sub-1-unit clearance jitter as touching, not a gameplay overlap. |
| Damage story | Pass | `damage-demo` reports `left-arm-lost`, `legs-lost`, `cockpit-lost`, `pilotRisk=1`, `destroyedUnits=1`, and weapon families `energy+missile+ballistic+explosive`. |
| Replacement boundary | Pass | `content-packs/project-owned-starter.example.json`, `content-packs/project-owned-visual-slice.example.json`, and `content-packs/project-owned-art-safe-slice.example.json` pass the public content boundary check; current local screenshots remain development evidence, not public final art. |
| Public boundary preflight | Pass | `scripts/content-pack/check_controlled_demo_public_boundary.ps1` reports `Controlled demo public boundary preflight OK` for project-owned metadata examples; with `-CheckDevBuild`, the current Windows development build remains expected `Result: FAILED`. |

Evidence-package judgment: the current local Windows Demo evidence set now combines the PC4 Windows build, visible-flow smoke, six fresh screenshots/sidecars, PC11 core playable contract check, PC12 mobile command model preflight, PC13 current plan gate check, PC14 Android smoke log crash scan, manifest-driven local visuals, a clean replacement id path, and a metadata-only art-safe mission-slice target. It is suitable for controlled external demonstration, not public release.

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
| Six capture presets | Pass | `capture_reference_visuals.ps1` reports `MC2 reference visual captures passed: 6 preset(s)` for `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo`, and `north-patrol`. |
| MechLab fitting | Pass | `mechlab.json` reports `MechLabCapture=open`, `weaponBlock=1 Streak 1x2`, `fillers=A+/C+`, `Fit OK`, `CellState=OK OPEN4 OCC12 OCC!0 OOB0`, and `noToggle=yes`. |
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
| Six capture presets | Pass | `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo`, `north-patrol` PNG/JSON sidecars refreshed under `analysis-output/reference-visual-captures/`. |
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
| Six capture presets | Pass | `capture_reference_visuals.ps1` reports `MC2 reference visual captures passed: 6 preset(s)` for `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo`, and `north-patrol`. |
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

MechLab judgment: the fitting screen now proves whole weapon blocks, single-cell armor/cooling fillers, short H/W/G pressure, explicit cell-state evidence, and `noToggle=yes` for mounted weapons.

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
6. Close with `north-patrol.png`: show the same mission can trigger broader encounter beats.

## Honest Limits

Do not claim these screenshots are final public art. The local captures may use private reference content as development evidence for scale, pacing, and readability. Public or commercial builds must use project-owned or properly licensed replacement content packs.

Do not present the first version as supporting realtime PVP, mobile builds, map servers, account economy, paid recharge, chain assets, AI director, or model-driven per-frame combat. The current first-version promise is a local Windows playable Demo with deterministic BattleCore, readable MechLab fitting, sparse command, visible damage, repair/relaunch flow, and optional high-level AI deputy.
