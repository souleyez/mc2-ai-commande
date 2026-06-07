# Playable Demo Evidence 2026-06-07

Purpose: one concise evidence page for showing the current Windows playable Demo without committing generated screenshots or JSON sidecars.

The current evidence proves a local playable loop: MechLab fitting, squad launch, sparse tactical command, contact pressure, visible section damage, and a repeatable capture/build path. It is still a prototype evidence pack, not final public art.

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
| Squad spawn | `analysis-output/reference-visual-captures/spawn.png` and `.json` | Battle starts with a sparse HUD, commander-follow camera, 3-player squad, objective card, Jet/Map/Bay/System controls, and no combat log wall. | The default command state is simple enough for future touch control: whole squad first, status rows for exceptions. |
| Airfield contact | `analysis-output/reference-visual-captures/airfield.png` and `.json` | Terrain, water, runway/road tones, props, 12 active hostiles, 8 visible hostiles, and the next hangar objective are readable. | The map is now a tactical space with terrain and contact direction, not just colored blocks. |
| Hangar pressure | `analysis-output/reference-visual-captures/hangar-contact.png` and `.json` | A dense objective fight is visible with 20 active hostiles, 16 visible hostiles, sparse battle card, compact objective card, `unitRadii infantry=24 vehicle=54 mech=64`, and `ContactSpread` proof. | This is the current pressure-test image: it shows the battle system under load while proving the units are separated by BattleCore, not merely painted apart. |
| Damage story | `analysis-output/reference-visual-captures/damage-demo.png` and `.json` | Section damage is explicit: `damageReadability=weaponFamilies energy+missile+ballistic+explosive`, `left-arm-lost`, `legs-lost`, `cockpit-lost`, pilot risk, destroyed unit, sparse status-row confirmation, and `ContactSpread=players 2 hostiles 20 nearestPH=118 nearestHH=78`. | The selling point is not only HP bars; arms, legs, cockpit, ejection, weapon family cues, and wreck state can drive tactical drama. |
| North patrol / wider contact | `analysis-output/reference-visual-captures/north-patrol.png` and `.json` | A larger encounter slice stays readable with 24 active hostiles and 10 visible hostiles while occupancy and objective state remain tracked. | The same rules can cover broader patrol/trigger beats beyond the starting hangar fight. |

## Sidecar Highlights

Current refreshed sidecars report:

```text
mechlab: MechLabCapture=open ... weaponBlock=1 Streak ... 1x2 fillers=A+/C+ fit=Fit OK pressure=H 12/22  W 16/16  G 12/16 CellState=OK OPEN4 OCC12 OCC!0 OOB0 alwaysMounted=weapons 6/6 items 6/6 noToggle=yes
spawn: activeHostileCount=0 visibleHostileCount=0 BattleHud=active controls=statusRows+jet+map+bay+system combatPanel=h78 combatLogVisible=no objectivePanel=compactObjective objectiveH=74 missionMap=closed saveUi=disabled SparseBattleUi=statusRows+sections+solo controls=all+jet+map+bay+system combatLog=hidden accountUi=hidden debugOccupancy=sidecar-only overlays=hidden ContactSpread=players 3 hostiles 0 nearestPH=n/a nearestHH=n/a nearestPP=128
airfield: activeHostileCount=12 visibleHostileCount=8 ContactSpread=players 3 hostiles 12 nearestPH=704.7 nearestHH=108 currentObjective=Destroy Hangar
hangar-contact: activeHostileCount=20 visibleHostileCount=16 BattleOccupancy=units 23/29 unitRadii infantry=24 vehicle=54 mech=64 ContactSpread=players 3 hostiles 20 nearestPH=272.8 nearestHH=48 nearestPP=259.1 playerSpan=519.9 hostileSpan=4304.2 centroidDistance=1161.6
damage-demo: activeHostileCount=20 visibleHostileCount=16 BattleOccupancy=units 22/29 unitRadii infantry=24 vehicle=54 mech=64 SparseBattleUi=statusRows+sections+solo combatLog=hidden accountUi=hidden debugOccupancy=sidecar-only overlays=hidden ContactSpread=players 2 hostiles 20 nearestPH=118 nearestHH=78 nearestPP=207.6 playerSpan=207.6 hostileSpan=4336.7 centroidDistance=836.7 DamageStory=units 3/3 lostSections=3 arms=1 legs=1 cockpit=1 pilotRisk=1 destroyedUnits=1 story=unit-1:left-arm-lost,unit-2:legs-lost,unit-3:cockpit-lost
damageReadability: weaponFamilies energy+missile+ballistic+explosive; weaponShapes beam+arc+tracer+shock; sectionConsequences arms-firepower legs-mobility cockpit-ejection wreck-salvage; hud=section-bars+short-labels+sparse
north-patrol: activeHostileCount=24 visibleHostileCount=9 ContactSpread=players 3 hostiles 24 nearestPH=118 nearestHH=70.1 status=north encounter trigger completed.
```

## Suggested Three-Minute Use

Use these in order:

1. Start with `mechlab.png`: show fitting as the collection/preparation pillar.
2. Move to `spawn.png`: show sparse command UI and commander-follow camera.
3. Use `airfield.png` or `hangar-contact.png`: show real map contact and objective pressure.
4. Use `damage-demo.png`: show section damage, cockpit risk, and destroyed-unit state.
5. Close with `north-patrol.png`: show the mission can trigger broader encounter beats.

## Honest Limits

Do not claim these screenshots are final public art. The local captures may use private reference content as development evidence for scale, pacing, and readability. Public or commercial builds must use project-owned or properly licensed replacement content packs.

Do not present the first version as supporting realtime PVP, mobile builds, map servers, account economy, paid recharge, chain assets, AI director, or model-driven per-frame combat. The current first-version promise is a local Windows playable Demo with deterministic BattleCore, readable MechLab fitting, sparse command, visible damage, repair/relaunch flow, and optional high-level AI deputy.
