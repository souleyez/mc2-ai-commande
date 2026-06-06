# Reference Visual Audit 2026-06-07

Purpose: record the current private reference visual capture evidence before continuing scale, occupancy, and camera readability work.

Generated captures:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

Capture outputs remain ignored under:

```text
analysis-output/reference-visual-captures/
```

## Capture Matrix

| Preset | Mission Time | Camera Ortho | Active Hostiles | Visible Hostiles | Evidence | Readability Result |
| --- | ---: | ---: | ---: | ---: | --- | --- |
| `spawn` | 0.29s | 29.11 | 0 | 0 | `spawn.png`, `spawn.json` | Squad formation is readable. The right mission panel covers the hangar edge, and terrain still reads as a large dark field with sparse detail. |
| `airfield` | 12.04s | 29.11 | 12 | 8 | `airfield.png`, `airfield.json` | First contact is readable enough to see enemy direction and reference props. Enemy vehicles and the hangar cluster begin to crowd around one focal point. |
| `hangar-contact` | 20.04s | 29.11 | 20 | 16 | `hangar-contact.png`, `hangar-contact.json` | Main failure case. Player units, hostiles, objective structure, building meshes, damage effects, and tree mass collapse into the hangar area. The fight reads as a dense knot rather than a tactical engagement. |
| `damage-demo` | 38.04s | 35.50 | 20 | 20 | `damage-demo.png`, `damage-demo.json` | Damage UI is clear, but the battlefield center is still too compressed. Expanded zoom helps, yet hostile/objective/mech overlap still hides who is hitting whom. |
| `north-patrol` | 54.04s | 29.11 | 23 | 11 | `north-patrol.png`, `north-patrol.json` | Best current combat view. Open terrain and wider hostile positions make the same assets more readable, which points to spacing/occupancy/camera composition rather than total model failure. |

## Findings

1. Reference models are now visible enough for development review. The remaining problem is not only missing art; it is local battle composition.
2. The hangar encounter over-activates visual pressure in one small area: 20 active hostiles by `hangar-contact`, with 16 visible at the default camera size.
3. `damage-demo` proves the current zoom-out is helpful, but not enough when every combatant is drawn around the objective structure.
4. Unit-vs-unit collision spacing exists, but the effective separation is too small for dense hangar fights at the current visual scale.
5. Static props and structures visually occupy space but do not yet contribute enough to deterministic landing/parking separation.
6. The right mission panel hides part of the hangar/forest area in the key engagement views. This should be considered during the fixed-camera readability pass.
7. Open-area fighting in `north-patrol` looks materially better, which means the next work should prioritize hangar-scale spacing, static obstacle occupancy, and camera composition before adding new UI or features.

## Next Engineering Actions

Task 2 should start with:

- Increase readable spacing for mech and vehicle visual footprints.
- Re-check parent scale compensation for reference unit meshes.
- Keep prop scale conservative; large props are already visually dominant near the hangar.

Task 3 should start with:

- Add deterministic static obstacle occupancy for targetable structures first.
- Add terrain-object occupancy only for large building/tree classes after structure occupancy is stable.
- Ensure jet landing and squad destination fallback reject occupied structure cores.

Task 4 should start with:

- Avoid letting the right objective panel cover the active hangar fight.
- Preserve fixed camera and limited zoom.
- Prefer a small composition offset or temporary tactical zoom over free camera rotation.

## Current Priority

Proceed in this order:

1. Tune BattleCore collision radii and enemy attack ring spacing.
2. Add structure occupancy to destination and collision resolution.
3. Re-capture `hangar-contact` and `damage-demo`.
4. Only then revisit UI panel placement or camera offset.

## Pass 1 Result

Implemented on 2026-06-07:

- Added BattleCore structure occupancy for targetable static structures.
- Pushed move destinations and jet landing checks away from live structure cores.
- Kept unit collision radii close enough to existing weapon ranges so core combat still resolves.
- Expanded enemy attack slot distribution from 16 to 24 slots while keeping the formation radius inside weapon range.
- Added validator coverage for structure-centered move fallback.

Validation evidence:

```text
analysis-output/unity-validate-structure-occupancy-r3.log
analysis-output/unity-build-structure-occupancy-r1.log
analysis-output/unity-player-structure-occupancy-smoke-r1.log
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/damage-demo.png
```

Observed effect:

- `hangar-contact` now keeps the squad parking outside the hangar core instead of collapsing directly into the target structure.
- `damage-demo` still shows a dense fight, but the unit positions are less obviously same-coordinate stacking.
- Active/visible hostile counts did not drop; this pass improved physical occupancy, not encounter density.

Remaining readability issues:

1. The right combat/objective panel still covers part of the hangar/forest flank.
2. Enemy and friendly units still cluster tightly around the hangar because all 20 hostiles can be active in the same encounter window.
3. Non-target terrain objects such as quonsets, barricades, parked aircraft, and tree masses are still visual props, not BattleCore occupancy.
4. The dark terrain and limited ground texture contrast still make silhouettes harder to parse at default zoom.

Next priority:

1. Fixed-camera/UI composition pass for `hangar-contact`.
2. Terrain-object occupancy for large airfield buildings and barricades.
3. Terrain contrast pass after physical readability is stable.

## Pass 2 Result

Implemented on 2026-06-07:

- Added BattleCore occupancy for hard terrain objects: airfield buildings, parked craft, quonsets, portable buildings, towers, domes, barricades, sandbags, and wall-like props.
- Kept forest regions out of hard collision for now because their broad circular regions would over-block the first map; forest readability should be handled through occlusion/fade first.
- Added validator coverage for terrain-object-centered move fallback.
- Added a compact mission objective card during active fire so the right mission panel no longer covers the hangar fight during the densest engagement windows.

Validation evidence:

```text
analysis-output/unity-validate-ui-terrain-occupancy-r1.log
analysis-output/unity-build-ui-terrain-occupancy-r1.log
analysis-output/unity-player-ui-terrain-occupancy-smoke-r1.log
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/damage-demo.png
```

Observed effect:

- `assert-combat-situation fire` now reports `missionBrief=compact full=6/6 active=[active] 2 Hangar h=86`.
- `hangar-contact` no longer has the full mission list covering the hangar/forest flank; the right side shows a compact objective card.
- `damage-demo` keeps the damage state visible while freeing the mid-right battlefield area.
- `damage-demo` visible hostile count dropped from 20 to 18 in the refreshed sidecar, consistent with hard terrain-object occupancy and camera composition changing which enemies are exposed in-frame.

Remaining issues:

1. The fight itself is still visually dense around the hangar.
2. Forest/tree masses still need an occlusion/fade pass rather than hard region collision.
3. Terrain contrast remains low, especially dark ground away from textured prop clusters.

## Pass 3 Result

Implemented on 2026-06-07:

- Added presentation-only occlusion fade targets for terrain-object props, terrain-object trees, forest footprint discs, forest trunks, and forest canopies.
- Fade calculation uses the fixed tactical camera and screen-space bounds near current player units and the active objective point.
- The fade pass changes renderer material alpha and brightness only; BattleCore movement, collision, targeting, and mission rules remain unchanged.
- Capture sidecars now report an `OcclusionFade=active X/Y focus Z` summary so screenshots can prove the pass is actually running.

Validation evidence:

```text
analysis-output/unity-build-occlusion-fade-r4.log
analysis-output/unity-player-occlusion-fade-smoke-r5.log
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/hangar-contact.json
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/damage-demo.json
```

Observed effect:

- `hangar-contact` reports `OcclusionFade=active 305/1493 focus 4`.
- `damage-demo` reports `OcclusionFade=active 345/1493 focus 3`.
- The full mission list remains compact during active fire, so the right side no longer masks the hangar fight.
- `damage-demo` remains readable with 20 active hostiles and 19 visible hostiles, so the occlusion pass does not hide combat information.

Remaining issues:

1. The hangar fight is still dense because many hostiles are active around the same objective window.
2. Forest/tree masses are now controlled by fade, but the terrain color contrast is still too low for a strong screenshot.
3. The next visual pass should focus on terrain contrast, roads/water/building-base readability, and then enemy density/parking spread.

## Pass 4 Result

Implemented on 2026-06-07:

- Replaced the old full-map water plane with a source-cell water surface so water no longer paints the whole battlefield.
- Added semantic terrain grading for water, shore, runway/road, dirt, grass, and building-adjacent ground.
- Added terrain composite luma reporting; the current first-slice composite reports `luma=81/98.8/187`.
- Changed the source terrain shader to render double-sided. This was the real visibility fix: the terrain mesh was present and the composite texture was readable, but the top face was being backface-culled in the player.
- Kept BattleCore terrain, click, movement, occupancy, and water/jet legality rules unchanged.
- Added optional environment-gated terrain diagnostics for development captures: `MC2_WRITE_TERRAIN_DEBUG=1` writes `analysis-output/reference-visual-captures/terrain-composite-debug.png`; `MC2_DISABLE_WATER_SURFACE=1` can isolate terrain rendering from water presentation.

Validation evidence:

```text
analysis-output/unity-build-terrain-readability-r7.log
analysis-output/unity-player-terrain-readability-smoke-r3.log
analysis-output/reference-visual-captures/airfield.png
analysis-output/reference-visual-captures/airfield.json
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/hangar-contact.json
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/damage-demo.json
```

Observed effect:

- `airfield` now shows readable green ground, blue water, softer shorelines, runway/road light strips, and building bases instead of the previous black terrain mass.
- `hangar-contact` and `damage-demo` no longer collapse into black ground blocks; units and structures stand on a visible tactical map.
- The sidecars still report the same mission scale and occlusion pass: `airfield` has 12 active / 8 visible hostiles, `hangar-contact` has 20 active / 16 visible hostiles, and `damage-demo` has 20 active / 19 visible hostiles.
- Smoke still exits with code `0`, so the presentation fix did not break mission flow.

Remaining issues:

1. The hangar fight remains dense; many hostiles are still active around one objective window.
2. Terrain is now readable, but still prototype-like: water boundaries, map-edge triangles, and road/runway softness need later art polish.
3. Unit and prop scale should be checked again now that the ground is visible.

Next priority:

1. Enemy density and parking spread around the hangar.
2. First-slice mech/vehicle/turret/prop scale audit.
3. Commander camera composition if the fight center still feels crowded after spacing.

## Pass 5 Result

Implemented on 2026-06-07:

- Reworked enemy attack slots from the older small attack ring into a deterministic 32-slot spread keyed by unit id, with a step of 3 and a separate infantry offset.
- Kept attack slots inside real combat-data weapon range. This matters because the real first-slice data uses much shorter ranges than fallback profiles: infantry and Harasser are 100, Centipede and UrbanMech are 150, LRMC is 225.
- Added an infantry ambush parking ring around the two source ambush anchors instead of sending all ambush infantry to one fixed coordinate.
- Added validator coverage for both high-pressure enemy attack spacing and infantry ambush parking spread, using the loaded `combat-data.json` profiles rather than only fallback ranges.
- Preserved the original activation pressure: no enemies were removed and mission triggers remain intact.

Validation evidence:

```text
analysis-output/unity-validate-enemy-spacing-r4.log
analysis-output/unity-build-enemy-spacing-r2.log
analysis-output/unity-player-enemy-spacing-smoke-r2.log
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/hangar-contact.json
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/damage-demo.json
```

Observed effect:

- `hangar-contact` still reports 20 active / 16 visible hostiles, and `damage-demo` still reports 20 active / 19 visible hostiles. Encounter pressure was preserved.
- The smoke log now shows ambush infantry receiving distinct parking targets such as `unit-15 -> 3547.56/-603.56`, `unit-16 -> 3612/-448`, `unit-17 -> 3547.56/-292.44`, and later units on the second ambush anchor.
- `hangar-contact` reads more like a tight fight around the hangar instead of every infantry contact sharing the same center point.
- `damage-demo` remains dense, but the enemy pressure now fans around the hangar/forest side while damaged player-unit state stays visible.

Remaining issues:

1. The first map is still visually dense because 20 enemies can be active around one objective window.
2. Unit/model scale and silhouette hierarchy now need a pass: mechs, vehicles, infantry, props, and trees still compete visually.
3. Camera composition may still need a small pass after scale tuning, especially for the hangar/forest flank.

Next priority:

1. First-slice mech/vehicle/turret/prop scale audit.
2. Commander camera composition if scale tuning does not open the fight center enough.
3. Later art polish for water edges, roads, and map-edge triangles.

## Pass 6 Result

Implemented on 2026-06-07:

- Added first-slice reference visual scale audit summaries to runtime logs and capture sidecars.
- Split unit reference visual scale by category: mechs now use a larger actor baseline, vehicles use a smaller vehicle baseline, and infantry fallback is deliberately much smaller than vehicles.
- Split terrain-object reference prop scale by category: structure, building, aircraft, vehicle, barricade, tree, smallProp, and other.
- Kept BattleCore movement, collision, hit, weapon range, mission trigger, objective and enemy count rules unchanged. This pass is visual scale and evidence only.
- Added validator coverage that the `mc2_01` contract actually exercises mech, vehicle, infantry, building, aircraft, barricade, tree, and smallProp scale categories.

Validation evidence:

```text
analysis-output/unity-validate-scale-audit.log
analysis-output/unity-build-scale-audit.log
analysis-output/reference-visual-captures/spawn.png
analysis-output/reference-visual-captures/spawn.json
analysis-output/reference-visual-captures/airfield.png
analysis-output/reference-visual-captures/airfield.json
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/hangar-contact.json
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/damage-demo.json
```

Observed scale evidence:

```text
ReferenceUnits=mech 6/0 vehicle 15/0 infantry 0/8 other 0/0 scale mech=0.92 vehicle=0.68 infantry=0.38
ReferencePropScale=structure 1/0 building 29/3 aircraft 4/0 vehicle 9/3 barricade 90/18 tree 139/594 smallProp 65/38 other 0/7
```

Observed effect:

- `spawn`, `airfield`, `hangar-contact`, and `damage-demo` sidecars now include `referenceAssets.scale`, so future screenshots can prove whether the scene is using the expected category mix.
- Mechs read more clearly as the main actors relative to vehicles and infantry.
- Infantry no longer uses the same fallback footprint as vehicles when no private reference mesh is available.
- Buildings, aircraft and barricades keep enough presence to sell the airfield scene without taking over the whole battle view.
- Encounter pressure was preserved: `airfield` remains 12 active / 8 visible hostiles, `hangar-contact` remains 20 active / 16 visible hostiles, and `damage-demo` remains 20 active / 19 visible hostiles.

Remaining issues:

1. The hangar fight is still dense because many enemies remain active around the same objective window.
2. Visual scale now has evidence, but BattleCore occupancy evidence still needs to be exposed next so visible hard objects can be checked against legal movement.
3. Camera composition still needs a fixed-view pass after occupancy evidence, especially around the hangar/forest flank.

Next priority:

1. BattleCore occupancy evidence pass.
2. Commander camera composition pass.
3. Then Phase C command-state smoke coverage.

## Pass 7 Result

Implemented on 2026-06-07:

- Added `BattleMission.OccupancySummary()` so the rules layer reports active collision units, unit radii, blocking structures, hard terrain-object blockers, max blocker radii, and destination fallback source.
- Added `DemoTerrainView.CurrentLandingAuditSummary()` so screenshots report terrain landing predicate evidence: total terrain samples, water/low-elevation blocked samples, and the external predicate source used by jet landing.
- Added top-level `occupancy` to capture sidecars.
- Added validator coverage that the first mission occupancy summary exposes unit radii, one structure blocker, hard terrain object blockers, and structure/hardProp destination fallback.
- Re-ran the partial squad jet smoke to confirm landing evidence did not change the rule: illegal landing blocks only the affected mech, while valid jumps still execute.

Validation evidence:

```text
analysis-output/unity-validate-occupancy-evidence.log
analysis-output/unity-build-occupancy-evidence.log
analysis-output/unity-player-occupancy-jet-smoke.log
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/hangar-contact.json
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/damage-demo.json
```

Observed occupancy evidence:

```text
hangar-contact: BattleOccupancy=units 23/29 unitRadii infantry=20 vehicle=42 mech=50 structures 1 maxStructureRadius=215 hardProps 80 building=21 aircraft=4 barricade=37 other=18 maxPropRadius=78 destinationFallback=structure+hardProp; Landing=DemoTerrainView totalSamples=10000 blockedSamples=9392 flaggedWater=9233 lowElevation=7722 externalPredicate=water+mapBounds
damage-demo: BattleOccupancy=units 22/29 unitRadii infantry=20 vehicle=42 mech=50 structures 1 maxStructureRadius=215 hardProps 80 building=21 aircraft=4 barricade=37 other=18 maxPropRadius=78 destinationFallback=structure+hardProp; Landing=DemoTerrainView totalSamples=10000 blockedSamples=9392 flaggedWater=9233 lowElevation=7722 externalPredicate=water+mapBounds
```

Observed effect:

- The capture sidecars now prove there are physical occupancy sources for units, the hangar structure, aircraft/building/barricade terrain objects, and water/map-bound landing rejection.
- `hangar-contact` remains 20 active / 16 visible hostiles and `damage-demo` remains 20 active / 19 visible hostiles, so encounter pressure was preserved.
- This pass did not change visual scale, camera, enemy activation, damage, weapon, movement or objective rules.

Remaining issues:

1. The battlefield is now evidence-backed but still compositionally crowded around the hangar/forest flank.
2. `blockedSamples` is high because the first capture slice contains large water regions around the island; this is correct for landing rejection, but the sidecar should be interpreted as a terrain-grid fact rather than a pathfinding navmesh.
3. The next pass should tune fixed-camera composition, not expand collision rules.

Next priority:

1. Commander camera composition pass.
2. Then Phase C command-state smoke coverage.

## Pass 8 Result

Implemented on 2026-06-07:

- Kept the source tactical yaw/pitch, orthographic camera, commander follow and limited zoom.
- Added a small objective composition offset when the active objective is far enough from the commander, so early mission framing keeps the squad and active objective in the same view sooner.
- Added `camera.compositionOffset` to capture sidecars so camera framing adjustments are auditable.
- Left `hangar-contact` and `damage-demo` at zero composition offset because the commander is already close to the active hangar objective in those presets.

Validation evidence:

```text
analysis-output/unity-build-camera-composition.log
analysis-output/reference-visual-captures/spawn.png
analysis-output/reference-visual-captures/spawn.json
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/hangar-contact.json
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/damage-demo.json
```

Observed camera evidence:

```text
spawn: compositionOffset=(0.98, 0, 1.76), ortho=29.11
hangar-contact: compositionOffset=(0, 0, 0), ortho=29.11
damage-demo: compositionOffset=(0, 0, 0), ortho=35.5
```

Observed effect:

- `spawn` now frames the initial squad and airfield direction together more clearly without adding camera rotation or manual dragging.
- `hangar-contact` still keeps the commander squad, hangar objective and enemy pressure in the main center view.
- `damage-demo` keeps the broader tactical zoom and does not lose the damaged squad/status relationship.
- Encounter pressure was preserved: `hangar-contact` remains 20 active / 16 visible hostiles, and `damage-demo` remains 20 active / 19 visible hostiles.

Remaining issues:

1. Phase B has enough visual, scale, occupancy and camera evidence to move into command-state validation.
2. Combat still needs later effect/damage polish, but that should come after Phase C proves the player command loop.

Next priority:

1. Phase C / Task C1 command state validator.
2. Status-row solo command flow.

## Stage 0 Baseline Audit Result

Implemented on 2026-06-07:

- Ran the new current-plan baseline pass after `Finalize squad jet landing rules`.
- Verified the rules layer, Windows build, visible-flow smoke and refreshed reference screenshots.
- Confirmed the Demo can already move through battle, debrief, Mech Lab, loadout compact assertions, squad swap/relaunch and commander observation export.
- Inspected `spawn`, `airfield`, `hangar-contact`, `damage-demo` and `north-patrol` screenshots directly, not just through logs.

Validation evidence:

```text
analysis-output/unity-validate-baseline-audit.log
analysis-output/unity-build-baseline-audit.log
analysis-output/unity-player-visible-flow-audit.log
analysis-output/reference-visual-captures/spawn.png
analysis-output/reference-visual-captures/spawn.json
analysis-output/reference-visual-captures/airfield.png
analysis-output/reference-visual-captures/airfield.json
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/hangar-contact.json
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/damage-demo.json
analysis-output/reference-visual-captures/north-patrol.png
analysis-output/reference-visual-captures/north-patrol.json
```

Validation results:

```text
Validator: MC2 demo contract validation OK.
Build: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
Visible-flow smoke: MC2 demo smoke test exiting with code 0.
```

Current capture matrix:

| Preset | Mission Time | Camera Ortho | Active Hostiles | Visible Hostiles | Current Readability |
| --- | ---: | ---: | ---: | ---: | --- |
| `spawn` | 1.08s | 29.11 | 0 | 0 | Squad, water, shoreline, hangar direction and terrain are readable. The right mission panel is still too large for the first screen and covers a meaningful part of the battlefield. |
| `airfield` | 12.79s | 29.11 | 12 | 8 | Enemy direction, runway/road area, hangar cluster and squad positions are readable. The right combat/objective panels still occupy too much of the tactical view. |
| `hangar-contact` | 20.76s | 29.11 | 20 | 16 | No longer looks like one same-coordinate pile. The hangar fight remains the densest local case, and the right combat panel is still bigger than the intended minimal battle HUD. |
| `damage-demo` | 38.73s | 35.50 | 20 | 19 | Status-row damage is clear, including destroyed/critical sections. The world-space fight center remains busy, and HUD panels still compete with the battle view. |
| `north-patrol` | 54.72s | 29.11 | 23 | 9 | Best current combat composition. Open terrain makes enemy direction and squad pressure readable, proving the worst readability issue is local hangar density plus HUD occupancy rather than total art failure. |

Observed sidecar evidence:

```text
hangar-contact occupancy: BattleOccupancy=units 23/29 unitRadii infantry=20 vehicle=42 mech=50 structures 1 maxStructureRadius=215 hardProps 80 building=21 aircraft=4 barricade=37 other=18 maxPropRadius=78 destinationFallback=structure+hardProp; Landing=DemoTerrainView totalSamples=10000 blockedSamples=9392 flaggedWater=9233 lowElevation=7722 externalPredicate=water+mapBounds
reference assets: terrain texture composite 800px loadedSamples=10000 missingSamples=0 manifestTextures=103 luma=81/98.8/187; ReferenceStructures=loaded 1 fallback 0; ReferenceProps=loaded 336 fallback 663; ReferenceUnits=mech 6/0 vehicle 15/0 infantry 0/8 other 0/0 scale mech=0.92 vehicle=0.68 infantry=0.38; ReferencePropScale=structure 1/0 building 29/3 aircraft 4/0 vehicle 9/3 barricade 90/18 tree 139/594 smallProp 65/38 other 0/7; OcclusionFade=active 305/1493 focus 4
```

Current judgment:

- The baseline is playable enough to continue into visible-flow lock.
- The weakest current screenshot is `damage-demo` because it combines dense combat, destroyed unit state and the largest visible HUD competition.
- `hangar-contact` remains the main composition stress case for local enemy density and hard-object occupancy.
- `north-patrol` is the best investor-style combat screenshot candidate in this batch because it shows pressure, terrain and unit direction without the hangar crowding.
- The next engineering task should be Stage 1 / Task 1.1: freeze the minimal battle HUD before adding more combat features.

Remaining issues:

1. The right combat/objective panels are still larger than the desired first-version minimal HUD.
2. The left status panel is useful and readable, but it occupies a large first-screen footprint; future polish should keep information density while reducing visual weight.
3. `hangar-contact` and `damage-demo` still need local composition work after the HUD pass.
4. Unity batch runs can dirty `unity-mc2-demo/Assets/Scenes/Mc2Demo.unity` through serialization churn; this must be checked before every commit.

Next priority:

1. Stage 0 / Task 0.2 worktree and private-output hygiene check.
2. Stage 1 / Task 1.1 minimal battle HUD.
3. Stage 2 occupancy placeholder/debug visibility only after HUD is no longer obscuring the scene.

## Stage 1.1 Minimal Battle HUD Result

Implemented on 2026-06-07:

- Changed the mission brief to use the compact objective panel throughout active battle instead of showing the full visible objective list outside fire mode.
- Reduced the right combat panel height from 154px to 112px.
- Reduced the compact objective panel height from 86px to 74px.
- Kept the left mech status rows, Jet, Map, Bay and System controls unchanged so the command loop remains the same.
- Kept BattleCore, mission triggers, enemy counts, damage, occupancy and camera behavior unchanged.

Validation evidence:

```text
analysis-output/unity-validate-minimal-battle-hud.log
analysis-output/unity-build-minimal-battle-hud.log
analysis-output/unity-player-minimal-battle-hud.log
analysis-output/reference-visual-captures/spawn.png
analysis-output/reference-visual-captures/spawn.json
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/hangar-contact.json
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/damage-demo.json
```

Validation results:

```text
Validator: MC2 demo contract validation OK.
Build: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
Combat smoke: MC2 demo smoke test exiting with code 0.
Smoke HUD assertion: missionBrief=compact full=6/6 active=[active] ... h=74.
```

Current capture matrix after HUD pass:

| Preset | Mission Time | Camera Ortho | Active Hostiles | Visible Hostiles | HUD Result |
| --- | ---: | ---: | ---: | ---: | --- |
| `spawn` | 1.06s | 29.11 | 0 | 0 | Right mission list is no longer a large objective roster; it is now a compact active objective block. More of the first-screen airfield and water are visible. |
| `hangar-contact` | 20.78s | 29.11 | 20 | 16 | Right HUD no longer reaches deep into the center-right battlefield. The hangar fight remains dense, but it is less obscured by UI. |
| `damage-demo` | 38.76s | 35.50 | 20 | 19 | Damage status remains clear on the left; the right HUD now leaves the lower-right battlefield open. Combat density, not the mission list, is the remaining weakness. |

Observed effect:

- The right side now behaves like a compact status stack instead of a secondary mission/debug dashboard.
- Existing command-state smoke still passes, including quiet, contact and fire tempo assertions.
- Enemy pressure remained the same: `hangar-contact` is still 20 active / 16 visible, and `damage-demo` is still 20 active / 19 visible.
- The visible improvement is purely presentation: less HUD coverage, same combat loop.

Remaining issues:

1. The left status panel is still visually heavy, but it is also the main control surface and should be tuned carefully rather than removed.
2. The right combat panel still shows short log text; a later polish pass can reduce it to icons/pulses once command smoke covers the same states.
3. `hangar-contact` and `damage-demo` still need local composition/occupancy review after the visible-flow smoke is guarded.

Next priority:

1. Stage 1 / Task 1.2 visible playable flow smoke.
2. Stage 1 / Task 1.3 capture walkthrough image set.
3. Stage 2 hangar composition and occupancy placeholder work.

## Stage 1.2 Visible Playable Flow Smoke Result

Implemented on 2026-06-07:

- Expanded `mc2_01-visible-flow-audit.txt` from a broad loop smoke into a complete visible-flow guard.
- Added status-row selection for `unit-1`, a single-unit map click, solo command assertion and auto-rejoin assertion.
- Added partial squad Jet coverage: one legal unit enters `jetting`, the other visible rows remain ready, and all rows return to ready after advance.
- Added squad move and squad attack accepted-count assertions.
- Added encounter pacing assertions before and during the hangar engagement.
- Kept the debrief, Mech Lab compact loadout, squad swap, hidden preview handoff, relaunch identity and commander observation checks.

Validation evidence:

```text
analysis-output/unity-player-visible-flow-audit.log
unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt
```

Validation results:

```text
Command script: actions=37.
Partial Jet: squad jump accepted=1.
Squad move: accepted=3.
Squad attack: accepted=3.
Debrief visible assertion: OK.
Debrief summary assertion: OK.
Loadout compact assertion: OK.
Smoke exit: MC2 demo smoke test exiting with code 0.
Failure scan: no assertion failed, failed:, or Debug.LogError lines.
```

Observed effect:

- The command file now proves a full player-visible loop instead of only isolated battle features.
- Status-row solo command and automatic return-to-squad are guarded by smoke.
- Per-mech Jet legality is guarded in the same end-to-end flow.
- The Mech Lab handoff and relaunch identity remain covered after the added battle steps.

Remaining issues:

1. This is a command/state smoke, not a screenshot walkthrough.
2. The next task should capture and inspect the full walkthrough image set.
3. `hangar-contact` and `damage-demo` remain the main visual pressure cases after flow is guarded.

Next priority:

1. Stage 1 / Task 1.3 capture walkthrough image set.
2. Stage 2 / Task 2.1 occupancy evidence against screenshots.
3. Stage 2 / Task 2.2 presentation collision placeholders.
