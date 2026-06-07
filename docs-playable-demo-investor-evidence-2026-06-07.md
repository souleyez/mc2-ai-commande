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
| MechLab fitting | `analysis-output/reference-visual-captures/mechlab.png` and `.json` | The fitting loop is visible: whole weapon blocks, `A+` armor, `C+` cooling, H/W/G pressure, `Fit OK`, and mounted weapons active by default. | The player prepares the squad by arranging physical equipment blocks instead of toggling abstract rows. |
| Squad spawn | `analysis-output/reference-visual-captures/spawn.png` and `.json` | Battle starts with a sparse HUD, commander-follow camera, 3-player squad, objective card, Jet/Map/Bay/System controls, and no combat log wall. | The default command state is simple enough for future touch control: whole squad first, status rows for exceptions. |
| Airfield contact | `analysis-output/reference-visual-captures/airfield.png` and `.json` | Terrain, water, runway/road tones, props, 12 active hostiles, 8 visible hostiles, and the next hangar objective are readable. | The map is now a tactical space with terrain and contact direction, not just colored blocks. |
| Hangar pressure | `analysis-output/reference-visual-captures/hangar-contact.png` and `.json` | A dense objective fight is visible with 20 active hostiles, 16 visible hostiles, sparse battle card, compact objective card, and occupancy sidecar proof. | This is the current pressure-test image: it shows the battle system under load and also shows where later combat readability polish should focus. |
| Damage story | `analysis-output/reference-visual-captures/damage-demo.png` and `.json` | Section damage is explicit: `left-arm-lost`, `legs-lost`, `cockpit-lost`, pilot risk, destroyed unit, and sparse status-row confirmation. | The selling point is not only HP bars; arms, legs, cockpit, ejection, and wreck state can drive tactical drama. |
| North patrol / wider contact | `analysis-output/reference-visual-captures/north-patrol.png` and `.json` | A larger encounter slice stays readable with 24 active hostiles and 10 visible hostiles while occupancy and objective state remain tracked. | The same rules can cover broader patrol/trigger beats beyond the starting hangar fight. |

## Sidecar Highlights

Current refreshed sidecars report:

```text
mechlab: MechLabCapture=open ... weaponBlock=1 Streak ... 1x2 fillers=A+/C+ fit=Fit OK pressure=H 12/22  W 16/16  G 12/16 alwaysMounted=weapons 6/6 items 6/6 noToggle=yes
spawn: BattleHud=active controls=statusRows+jet+map+bay+system combatPanel=h78 combatLogVisible=no objectivePanel=compactObjective objectiveH=74 missionMap=closed saveUi=disabled
airfield: activeHostileCount=12 visibleHostileCount=8 currentObjective=Destroy Hangar
hangar-contact: activeHostileCount=20 visibleHostileCount=16 OccupancyPlaceholders=enabled total 120 units 23 playerUnits 3 hostileUnits 20 structures 1 hardProps 80 landingBlockedMarkers 16
damage-demo: activeHostileCount=20 visibleHostileCount=16 DamageStory=units 3/3 lostSections=3 arms=1 legs=1 cockpit=1 pilotRisk=1 destroyedUnits=1 story=unit-1:left-arm-lost,unit-2:legs-lost,unit-3:cockpit-lost
north-patrol: activeHostileCount=24 visibleHostileCount=10 status=north encounter trigger completed.
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
