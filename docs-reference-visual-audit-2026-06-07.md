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

## Stage 1.3 Capture Walkthrough Image Set Result

Implemented on 2026-06-07:

- Refreshed the full visible-flow screenshot set after the minimal HUD and complete visible-flow smoke passes.
- Directly inspected `spawn`, `airfield`, `hangar-contact`, `damage-demo` and `north-patrol`.
- Compared screenshot readability against sidecar evidence for active/visible hostiles, BattleCore occupancy, terrain landing predicates, reference asset scale, occlusion fade and camera composition offset.

Validation command:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

Validation evidence:

```text
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

Current capture matrix:

| Preset | Mission Time | Camera Ortho | Active Hostiles | Visible Hostiles | Camera Offset | Walkthrough Result |
| --- | ---: | ---: | ---: | ---: | --- | --- |
| `spawn` | 1.04s | 29.11 | 0 | 0 | `0.98/0/1.76` | Terrain, water, squad formation, hangar direction and target marker are readable. The left status/control surface remains visually heavy for an opening shot. |
| `airfield` | 12.77s | 29.11 | 12 | 8 | `0/0/0` | Enemy direction, command line, objective building and squad approach are readable. The fight is understandable, though the left status block still competes with the map. |
| `hangar-contact` | 20.76s | 29.11 | 20 | 16 | `0/0/0` | Pressure is clear and the objective fight no longer looks like one exact-coordinate pile. It is still the densest local encounter, with player/enemy/building silhouettes packed around the hangar mouth. |
| `damage-demo` | 38.66s | 35.50 | 20 | 19 | `0/0/0` | Status-row damage is readable, including cockpit/leg/critical/destroyed states, but the world-space damage event is too small and compressed for the screenshot that should sell the damage fantasy. |
| `north-patrol` | 54.74s | 29.11 | 23 | 9 | `7.09/0/-2.44` | Best current investor screenshot candidate. Open terrain, bridge/water separation, enemy direction and squad pressure are readable without the hangar crowding. |

Observed sidecar constants:

```text
ReferenceUnits=mech 6/0 vehicle 15/0 infantry 0/8 other 0/0 scale mech=0.92 vehicle=0.68 infantry=0.38
ReferencePropScale=structure 1/0 building 29/3 aircraft 4/0 vehicle 9/3 barricade 90/18 tree 139/594 smallProp 65/38 other 0/7
Landing=DemoTerrainView totalSamples=10000 blockedSamples=9392 flaggedWater=9233 lowElevation=7722 externalPredicate=water+mapBounds
```

Observed occupancy evidence:

```text
spawn: BattleOccupancy=units 3/29 unitRadii infantry=20 vehicle=42 mech=50 structures 1 maxStructureRadius=215 hardProps 80 building=21 aircraft=4 barricade=37 other=18 maxPropRadius=78 destinationFallback=structure+hardProp
airfield: BattleOccupancy=units 15/29 unitRadii infantry=20 vehicle=42 mech=50 structures 1 maxStructureRadius=215 hardProps 80 building=21 aircraft=4 barricade=37 other=18 maxPropRadius=78 destinationFallback=structure+hardProp
hangar-contact: BattleOccupancy=units 23/29 unitRadii infantry=20 vehicle=42 mech=50 structures 1 maxStructureRadius=215 hardProps 80 building=21 aircraft=4 barricade=37 other=18 maxPropRadius=78 destinationFallback=structure+hardProp
damage-demo: BattleOccupancy=units 22/29 unitRadii infantry=20 vehicle=42 mech=50 structures 1 maxStructureRadius=215 hardProps 80 building=21 aircraft=4 barricade=37 other=18 maxPropRadius=78 destinationFallback=structure+hardProp
north-patrol: BattleOccupancy=units 26/29 unitRadii infantry=20 vehicle=42 mech=50 structures 1 maxStructureRadius=215 hardProps 80 building=21 aircraft=4 barricade=37 other=18 maxPropRadius=78 destinationFallback=structure+hardProp
```

Investor screenshot candidate:

- `north-patrol`: it shows tactical direction, spacing, water/land separation and enemy pressure with the least explanation.

Must-fix screenshot:

- `damage-demo`: it should demonstrate the most distinctive combat selling point, but currently relies too much on the left status panel. The next damage pass should make arm loss, leg collapse, cockpit ejection and wreck/debris more readable in the world view.

Current judgment:

- The Demo is now screenshot-auditable and no longer stuck at "all color blocks".
- The first visible-flow proof is good enough to move from general flow locking into occupancy/composition and damage-readability work.
- The left status/control surface is useful, but it is now the main UI weight problem across all battle captures.
- The hangar pile-up is not obviously missing all collision evidence: sidecars already report units, structure blockers, hard terrain props and water/map-bound landing predicates. The next task should decide which remaining visual density is rule spacing, camera compression, UI weight or model/effect scale.

Next priority:

1. Stage 2 / Task 2.1 occupancy evidence against `hangar-contact` and `damage-demo` screenshots.
2. Stage 2 / Task 2.2 presentation collision placeholders to make hard occupancy visible during review.
3. Then tune hangar composition or damage-world cues based on that evidence, instead of guessing.

## Stage 2.1 Battle Occupancy Evidence Audit Result

Implemented on 2026-06-07:

- Re-ran the mission contract validator after the walkthrough capture baseline.
- Refreshed only the two stress screenshots: `hangar-contact` and `damage-demo`.
- Audited current screenshots against the rules-side occupancy evidence and the known terrain-object classification rules.
- Did not change BattleCore rules in this pass because the current evidence already covers the obvious hard blockers.

Validation commands:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-occupancy-evidence.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

Validation evidence:

```text
analysis-output/unity-validate-occupancy-evidence.log
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/hangar-contact.json
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/damage-demo.json
```

Validation result:

```text
Validator: MC2 demo contract validation OK: 29 units, 3 player units, 9 objectives, 1 structure, 10000 terrain samples, 1000 terrain objects, combat simulation passed.
```

Current stress capture evidence:

| Preset | Mission Time | Camera Ortho | Active Hostiles | Visible Hostiles | Battle Occupancy |
| --- | ---: | ---: | ---: | ---: | --- |
| `hangar-contact` | 20.81s | 29.11 | 20 | 16 | `units 23/29`, `structures 1`, `hardProps 80`, `maxStructureRadius=215`, `maxPropRadius=78` |
| `damage-demo` | 38.78s | 35.50 | 20 | 19 | `units 22/29`, `structures 1`, `hardProps 80`, `maxStructureRadius=215`, `maxPropRadius=78` |

Occupancy classification:

| Screenshot object class | Current evidence | Judgment |
| --- | --- | --- |
| Objective hangar / target structure | `structures 1 maxStructureRadius=215` and validator `ValidateStructureCollisionOccupancy` | BattleCore hard blocker. |
| Airfield buildings, quonsets, portable buildings, tower, dome, aircraft | `hardProps 80 building=21 aircraft=4 other=18`, created from `BUILDING` terrain objects | BattleCore hard blockers. |
| Barricades, sandbags, walls, barriers | `hardProps 80 barricade=37`, created from selected `TREE` terrain objects whose names imply hard obstacles | BattleCore hard blockers. |
| Water, shore and map edge | `Landing=DemoTerrainView ... externalPredicate=water+mapBounds` and validator jump rejection for hard terrain objects | Landing and Jet legality evidence is present. |
| Forest/tree masses and rocky visual clusters | `OcclusionFade=active 305/1493` on `hangar-contact`, `345/1493` on `damage-demo`; not counted in `hardProps` unless named as barricade/wall/sandbag | Presentation/occlusion objects, not first-version hard movement blockers. |
| Left status/control surface | Not gameplay occupancy | UI weight issue, not collision issue. |

Current diagnosis:

- The remaining hangar crowding is not primarily caused by missing baseline occupancy. The sidecars prove unit radii, structure blockers, hard terrain-object blockers and water/map-bound landing predicates.
- `hangar-contact` remains dense because 20 enemies are active and 16 are visible around one hangar objective at fixed camera yaw/pitch.
- `damage-demo` remains weak as a selling screenshot because its camera zooms out to `ortho=35.50`, making the world-space damage event small while the left status panel carries most of the information.
- The visible forest/tree/rock mass is intentionally not a hard blocker in this first slice; it is handled by occlusion fade and should not be converted to broad collision without a specific movement failure.
- No Unity-only collider or presentation-only object should be promoted to gameplay truth without adding BattleCore validation first.

Next priority:

1. Stage 2 / Task 2.2 presentation collision placeholders, so hard blockers can be reviewed visually against the screenshot.
2. Stage 2 / Task 2.3 hangar composition tuning if placeholders show rules occupancy is correct but visual pressure remains too tight.
3. Stage 3 / damage-world cues if `damage-demo` remains weaker because damage events are too small rather than because units lack legal spacing.

## Stage 2.2 Presentation Collision Placeholder Result

Implemented on 2026-06-07:

- Added `BattleMission.OccupancyPlaceholderRegions()` as the rules-side source for reviewable hard occupancy regions.
- Added a `BattleOccupancyRegion` read-only data type with kind, id, label, mission position and radius.
- Added Unity presentation placeholders that draw subtle low ground discs for those regions only when `MC2_SHOW_OCCUPANCY_PLACEHOLDERS=1` or `-mc2ShowOccupancyPlaceholders` is set.
- Added a top-level `occupancyPlaceholders` field to capture sidecars.
- Updated `scripts/unity/capture_reference_visuals.ps1` so reference captures enable placeholders by default; use `-NoOccupancyPlaceholders` for clean non-audit captures.
- Kept movement, landing, targeting, collision and mission rules unchanged.

Modified files:

```text
unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs
unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs
scripts/unity/capture_reference_visuals.ps1
```

Validation commands:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-occupancy-placeholders.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-occupancy-placeholders.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

Validation evidence:

```text
analysis-output/unity-validate-occupancy-placeholders.log
analysis-output/unity-build-occupancy-placeholders.log
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/hangar-contact.json
analysis-output/reference-visual-captures/hangar-contact.log
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/damage-demo.json
analysis-output/reference-visual-captures/damage-demo.log
```

Validation results:

```text
Validator: MC2 demo contract validation OK.
Build: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
hangar-contact: OccupancyPlaceholders=enabled total 81 structures 1 hardProps 80 source=BattleMission.OccupancyPlaceholderRegions.
damage-demo: OccupancyPlaceholders=enabled total 81 structures 1 hardProps 80 source=BattleMission.OccupancyPlaceholderRegions.
```

Observed effect:

- `hangar-contact` now shows faint structure/hard-prop footprint discs around the hangar and nearby hard objects without turning the screenshot into a debug grid.
- `damage-demo` shows the same hard occupancy review layer while keeping the damage status UI and battlefield readable.
- The placeholder count matches the previously audited occupancy summary: one targetable structure plus 80 hard terrain-object blockers.
- The placeholders make it clear that the remaining dense hangar read is not caused by missing obvious hard-object occupancy. It is still mostly encounter pressure, fixed camera compression, UI weight and damage/effect readability.

Remaining issues:

1. The placeholder layer is useful for audit but should stay out of normal player-facing presentation.
2. The left status/control surface remains heavy.
3. `hangar-contact` still needs composition tuning, and `damage-demo` still needs stronger world-space damage/ejection cues.

Next priority:

1. Stage 2 / Task 2.3 tune hangar encounter composition without reducing enemy pressure.
2. Then Stage 3 damage and weapon readability work, especially for `damage-demo`.

## Stage 2.3 Hangar Encounter Composition Result

Implemented on 2026-06-07:

- Changed `mc2_01` enemy attack target selection so active enemy pressure is deterministically distributed across the available player squad instead of collapsing onto one nearest player unit.
- Kept source enemy activation counts, source brain names and existing attack-slot offsets intact.
- Added `ValidateEnemyAttackTargetSpread` to prove the `mc2_01` pressure set uses all three player targets and does not silently dogpile one squad member.
- Kept attack movement inside existing `EnemyAttackFormationOffset` and weapon-range validation.
- Restored Unity scene fileID churn after build/capture; no scene content change is part of this task.

Modified files:

```text
unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs
unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs
```

Validation commands:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-hangar-composition.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-hangar-composition.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-after-hangar-composition.log"
```

Validation evidence:

```text
analysis-output/unity-validate-hangar-composition.log
analysis-output/unity-build-hangar-composition.log
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/hangar-contact.json
analysis-output/reference-visual-captures/hangar-contact.log
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/damage-demo.json
analysis-output/reference-visual-captures/damage-demo.log
analysis-output/unity-player-visible-flow-after-hangar-composition.log
```

Validation results:

```text
Validator: MC2 demo contract validation OK.
Build: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
hangar-contact: activeHostileCount 20, visibleHostileCount 16, OccupancyPlaceholders=enabled total 81 structures 1 hardProps 80.
damage-demo: activeHostileCount 20, visibleHostileCount 20, OccupancyPlaceholders=enabled total 81 structures 1 hardProps 80.
Visible-flow smoke: MC2 demo smoke test exiting with code 0.
```

Observed effect:

- Encounter pressure was preserved: `hangar-contact` still has 20 active / 16 visible hostiles and `damage-demo` still has 20 active hostiles.
- The hangar fight now distributes enemy attack pressure across the player squad, so player units, enemy groups and objective pressure read as a tight fight around the hangar rather than one purely same-target knot.
- Hard occupancy evidence stayed visible and unchanged: one targetable structure plus 80 hard terrain-object blockers.
- `damage-demo` now keeps all 20 hostiles visible at the wider camera, but it still depends too much on the left status panel to sell damage events.

Remaining issues:

1. `hangar-contact` is still intentionally dense because 20 enemies are active around one objective at a fixed camera angle.
2. The left status/control surface remains visually heavy in battle screenshots.
3. The next visible gain should come from Stage 3 weapon-family cues and stronger world-space damage/ejection events, not from reducing enemy count.

Next priority:

1. Stage 3 / Task 3.1 regress weapon family cues.
2. Stage 3 / Task 3.2 lock section damage and ejection cues.

## Stage 3.1 Weapon Family Cue Result

Implemented on 2026-06-07:

- Added a weapon-direction cue pass in `Mc2DemoBootstrap` after the existing weapon trace and before the impact/aftermath cues.
- Energy weapons now add a longer direction core with short forward ticks, so laser-like fire has a readable source-to-target line.
- Missile weapons now add persistent approach pips and smoke streaks along their existing arc lanes.
- Ballistic weapons now add a short snap-line and arrowhead at the hit end, making the fast shot direction easier to read.
- Generic/explosive fallback shots now add a shock direction pulse and short afterglow vector.
- Hit direction cues now include an approach wedge at the target, in addition to the existing inbound slash.
- The combat situation assertion summary now guards `direction-core`, `approach-pips`, `snap-line`, `Explosive=shock-pulse+smoke+afterglow` and `HitDirection=inbound+slash+approach-wedge`.

Modified files:

```text
unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs
```

Validation commands:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-weapon-family-cues.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-weapon-family-cues.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-after-weapon-cues.log"
```

Validation evidence:

```text
analysis-output/unity-validate-weapon-family-cues.log
analysis-output/unity-build-weapon-family-cues.log
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/damage-demo.json
analysis-output/reference-visual-captures/damage-demo.log
analysis-output/unity-player-visible-flow-after-weapon-cues.log
```

Validation results:

```text
Validator: MC2 demo contract validation OK.
Build: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
damage-demo: activeHostileCount 20, visibleHostileCount 20, OccupancyPlaceholders=enabled total 81 structures 1 hardProps 80.
Visible-flow smoke: MC2 demo smoke test exiting with code 0.
Smoke assertion: fx=Energy=beam+pillar+muzzle+flash+scorch+direction-core Missile=arc+blast+salvo-spread+crater+approach-pips Ballistic=tracer+sparks+muzzle+punch+debris+snap-line Explosive=shock-pulse+smoke+afterglow; hitDirectionFx=HitDirection=inbound+slash+approach-wedge.
```

Observed effect:

- `damage-demo` keeps the same 20 active / 20 visible hostile pressure while showing stronger blue/orange directional weapon cues near the hangar fight.
- The new cue layer stays in the battlefield and does not cover the left mech status rows.
- The improvement helps source-to-target readability, but the screenshot is still a far, dense combat view.

Remaining issues:

1. `damage-demo` still needs larger world-space section damage and ejection events to sell the mech damage fantasy without relying on status text.
2. The left status/control surface remains visually heavy, though this task did not add new UI.
3. Explosive-specific fallback is guarded as a cue language, but current source weapon data still mainly uses energy, missile and ballistic types.

Next priority:

1. Stage 3 / Task 3.2 lock section damage and ejection cues.
2. Stage 3 / Task 3.3 lock armor hardness damage rule.

## Stage 3.2 Section Damage And Ejection Cue Result

Implemented on 2026-06-07:

- Added stronger world-space section damage cues in `DemoUnitView`.
- Destroyed arms now add firepower-lost markers on top of the existing missing socket, flag, cable, detached part and landing debris cues.
- Destroyed legs now add a ground danger ring and mobility-lost beacon on top of the existing collapse, red cross, skid, dust and broken cable cues.
- Destroyed cockpit now adds a taller escape column and evacuation direction marker, and the ejection chute now draws a visible escape route beam.
- Expanded `SectionDamageCueSummary()` and the combat situation smoke assertion so the new cue language is guarded by automated flow tests.
- Added validator evidence that destroying the cockpit critical section destroys the unit, matching the ejection path.
- Restored Unity scene fileID churn after the Windows build; no scene content change is part of this task.

Modified files:

```text
unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs
unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs
unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs
docs-reference-visual-audit-2026-06-07.md
docs-playable-demo-locked-execution-plan-2026-06-07.md
```

Validation commands:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-section-damage-lock.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-section-damage-lock.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-after-section-damage-lock.log"
```

Validation evidence:

```text
analysis-output/unity-validate-section-damage-lock.log
analysis-output/unity-build-section-damage-lock.log
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/damage-demo.json
analysis-output/reference-visual-captures/damage-demo.log
analysis-output/unity-player-visible-flow-after-section-damage-lock.log
```

Validation results:

```text
git diff --check: clean, with Windows line-ending warnings only.
Validator: MC2 demo contract validation OK.
Build: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
damage-demo: activeHostileCount 20, visibleHostileCount 20, OccupancyPlaceholders=enabled total 81 structures 1 hardProps 80.
Visible-flow smoke: MC2 demo smoke test exiting with code 0.
Smoke assertion: sectionFx=Arms=missing-socket+flag+flight+landing-debris+firepower-marker Legs=collapse+red-cross+skid+dust+danger-ring+mobility-beacon Cockpit=breach+ejection-pod+chute+landing+arc+distress+escape-column+route.
```

Observed effect:

- `damage-demo` still preserves the same heavy encounter pressure: 20 active / 20 visible hostiles around the hangar.
- World-space section cues are easier to spot in the fight center: red/orange mobility and firepower loss markers sit near the damaged player units, and the blue cockpit escape route/pilot landing cue is visible near the ejection path.
- The left status rows still agree with the forced damage setup: unit-1 left arm, unit-2 legs and unit-3 cockpit/destroyed state are represented in the UI.
- Weapon-family direction cues and hard occupancy placeholder evidence did not regress.

Remaining issues:

1. `damage-demo` remains a wide, dense screenshot; it proves the cue language but still reads busy as an investor-facing image.
2. The left status surface remains visually heavy, but it is still the main first-version command/status interface.
3. Future polish should make damage events easier to read in motion or add a closer damage-focused capture preset, instead of weakening the actual mission pressure.

Next priority:

1. Stage 3 / Task 3.3 lock armor hardness damage rule.
2. Then Stage 4 / Task 4.1 audit mounted weapon semantics.

## Stage 3.3 Armor Hardness Rule Result

Implemented on 2026-06-07:

- Kept armor plates as one overall hardness value, exposed through loadout preview totals and applied to `UnitState` as `CombatArmorHardnessBonus`.
- Confirmed damage mitigation stays simple: incoming raw damage is reduced by `CombatIncomingDamageMultiplier` before section damage allocation.
- Added validator evidence that armored and unarmored units take different section damage from the same direct hit.
- Added validator evidence that high enough damage still destroys a non-critical section on an armored unit, proving armor hardness does not erase the section damage system.
- Kept cockpit, torso, arm and leg section destruction behavior intact.

Modified files:

```text
unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs
docs-mc2-detailed-development-plan.md
docs-reference-visual-audit-2026-06-07.md
docs-playable-demo-locked-execution-plan-2026-06-07.md
```

Validation commands:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-armor-hardness-lock.log"
```

Validation evidence:

```text
analysis-output/unity-validate-armor-hardness-lock.log
```

Validation results:

```text
git diff --check: clean, with Windows line-ending warnings only.
Validator: MC2 demo contract validation OK.
```

Observed effect:

- Armor is now locked as a cheap deterministic rule suitable for later server validation.
- The MechLab rule stays easy to explain: armor plates increase overall hardness, but they do not add per-location armor bookkeeping.
- The validator proves both halves of the intended behavior: armor reduces incoming section damage, and enough damage can still destroy a section.

Remaining issues:

1. The MechLab UI still needs Stage 4 polish so players see armor plates and heat sinks as clear single-cell fillers.
2. Stage 4 should now audit mounted weapon semantics before changing grid visuals.

Next priority:

1. Stage 4 / Task 4.1 audit mounted weapon semantics.
2. Then Stage 4 / Task 4.2 make MechLab grid blocks explicit.

## Stage 4.1 Mounted Weapon Semantics Audit Result

Implemented on 2026-06-07:

- Audited weapon toggle wording across BattleCore, editor validator, presentation code and public README files.
- Renamed the internal loadout preview weapon mask from `enabledWeapons` / `IsWeaponEnabled` to `mountedWeapons` / `IsWeaponMounted`.
- Renamed the validator's disabled weapon preview language to unmounted weapon preview language.
- Kept the internal preview ability to model an unmounted weapon for fitting validation, while preserving the player rule that a mounted weapon is active.
- Confirmed remaining generic `Enable` / `Disable` hits are unrelated GUI, material, collider, renderer, purchase, hire, launch or save-slot state controls, not weapon toggles.

Modified files:

```text
unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs
unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs
docs-reference-visual-audit-2026-06-07.md
docs-playable-demo-locked-execution-plan-2026-06-07.md
```

Validation commands:

```powershell
rg -n "enabledWeapons|IsWeaponEnabled|disabledPreview|disabled loadout|disabled weapon|weapon.*toggle|toggle.*weapon" unity-mc2-demo/Assets/Scripts unity-mc2-demo/Assets/Editor unity-mc2-demo/README.md README.md
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-mounted-weapon-semantics.log"
```

Validation evidence:

```text
analysis-output/unity-validate-mounted-weapon-semantics.log
```

Validation results:

```text
Weapon-toggle rg audit: no matches for enabledWeapons, IsWeaponEnabled, disabledPreview, disabled loadout, disabled weapon, weapon toggle or toggle weapon.
git diff --check: clean, with Windows line-ending warnings only.
Validator: MC2 demo contract validation OK.
```

Observed effect:

- The codebase no longer names mounted weapon filtering as enable/disable.
- Installed/mounted weapons remain the active combat concept.
- Stage 4 can now polish grid block fitting without reintroducing weapon on/off UI.

Remaining issues:

1. MechLab still needs clearer contiguous weapon blocks and more direct armor/sink filler readability.
2. The next commit should focus on grid block visuals and validation, not broader MechLab redesign.

Next priority:

1. Stage 4 / Task 4.2 make MechLab grid blocks explicit.
2. Then Stage 4 / Task 4.3 prove loadout battle effects.

## Stage 4.2 MechLab Grid Block Result

Implemented on 2026-06-07:

- Strengthened the MechLab projected grid rendering so mounted weapons read as contiguous blocks instead of independent colored cells.
- Added block-level outer frames, selected weapon frames and subtle internal cell dividers for multi-cell weapon shapes.
- Added a single-cell filler frame language for armor plates and heat sinks.
- Added a compact smoke assertion summary that guards the grid cue language: `GridBlock=outer-frame+contiguous-weapon+cell-dividers+single-cell-filler+shape-label`.
- Kept the existing click/select/nudge placement model and did not add any weapon enable/disable UI.
- Confirmed the default Bushwacker loadout preview has a multi-cell weapon block and shape label; it currently exposes filler placement language as `filler:target` because the default fit is overweight and does not auto-place armor/sink fillers.
- Restored Unity scene fileID churn after the Windows build; no scene content change is part of this task.

Modified files:

```text
unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs
docs-reference-visual-audit-2026-06-07.md
docs-playable-demo-locked-execution-plan-2026-06-07.md
```

Validation commands:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-mechlab-grid-blocks.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-mechlab-grid-blocks.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-loadout-compact.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-mechlab-grid-blocks.log"
```

Validation evidence:

```text
analysis-output/unity-validate-mechlab-grid-blocks.log
analysis-output/unity-build-mechlab-grid-blocks.log
analysis-output/unity-player-mechlab-grid-blocks.log
```

Validation results:

```text
git diff --check: clean, with Windows line-ending warnings only.
Validator: MC2 demo contract validation OK.
Build: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
Loadout compact smoke: MC2 demo smoke test exiting with code 0.
Smoke assertion: GridBlock=outer-frame+contiguous-weapon+cell-dividers+single-cell-filler+shape-label preview=weaponBlock:yes/filler:target/shape:yes.
```

Observed effect:

- The fitting grid now has a stronger visual hierarchy: mounted weapons are framed as blocks, selected weapons get an additional frame, and multi-cell weapons show internal cell division without returning to per-cell weapon toggles.
- The compact smoke summary proves that the selected MechLab preview has a contiguous weapon block and shape label.
- The one-cell armor/sink filler language is guarded through labels, component detail and target placement actions, while the default overweight fit correctly does not auto-place new filler pieces.

Remaining issues:

1. Resolved by Stage 4.3: fitted weapons, armor and heat sinks now have BattleCore validator evidence.
2. A future visual/manual pass can capture an actual MechLab screenshot or a filler-applied preview if the default fit is adjusted.

Next priority:

1. Stage 4.3 loadout battle effect proof, Stage 5 / C1 debrief cleanup and Stage 5 / C2 repair/relaunch are complete; next priority is Stage 6 / D1 AI observation.

## Stage 4.3 Loadout Battle Effect Result

Implemented on 2026-06-07:

- Moved the MechLab preview-to-combat conversion into BattleCore with `UnitLoadoutCombatOverrideBuilder`.
- Kept Unity presentation as a caller of that shared helper, so the UI no longer owns a separate interpretation of fitted combat stats.
- Added validator evidence that the full default Bushwacker preview applies mounted weapon range, damage, cooldown, heat, weight, armor hardness and heat dissipation to `UnitState`.
- Added validator evidence that a reduced mounted weapon preview lowers battle-ready heat and total fitted weight.
- Added validator evidence that an armor filler increases armor hardness and lowers incoming damage multiplier.
- Added validator evidence that a heat-sink filler increases combat heat dissipation.
- Preserved the existing full-loadout profile numbers for current demo behavior; subset previews only change stats when fitted weapons are actually removed.
- Restored Unity scene fileID churn after the Windows build; no scene content change is part of this task.

Modified files:

```text
unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs
unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs
unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs
docs-reference-visual-audit-2026-06-07.md
docs-playable-demo-locked-execution-plan-2026-06-07.md
docs-mc2-detailed-development-plan.md
```

Validation commands:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-loadout-battle-effects.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-loadout-battle-effects.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-after-loadout-battle-effects.log"
```

Validation evidence:

```text
analysis-output/unity-validate-loadout-battle-effects.log
analysis-output/unity-build-loadout-battle-effects.log
analysis-output/unity-player-visible-flow-after-loadout-battle-effects.log
```

Validation results:

```text
git diff --check: clean, with Windows line-ending warnings only.
Validator: MC2 demo contract validation OK.
Build: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
Visible-flow smoke: MC2 demo smoke test exiting with code 0.
```

Observed effect:

- MechLab fitting now has a guarded gameplay path: fitted weapons, armor plates and heat sinks change the same UnitState fields BattleCore uses during combat.
- The code path is deterministic and catalog-driven; this task does not rebalance weapon numbers or add new economy/save behavior.
- The visible-flow smoke still passes after moving the loadout combat conversion out of presentation code.

Remaining issues:

1. Resolved by Stage 5.1: the debrief screen now exposes clean repair/contract/retry/close actions and normal smoke no longer carries save/account wording.
2. A future visual/manual pass can capture a filler-applied MechLab screenshot once the default fit exposes a clean spare-cell example.

Next priority:

1. Stage 5 / C2 guard repair and relaunch loop is complete; next priority is Stage 6 / D1 AI observation.

## Stage 5.1 Debrief Player Flow Result

Implemented on 2026-06-07:

- Replaced normal debrief panel actions with `Repair & Mech Lab`, `Next Contract`, `Retry Battle` and `Close`.
- Updated post-mission status copy to `Repair path: Mech Lab` and `Next contract list`.
- Removed `End Run` and `Restart Mission` from the normal debrief surface and debrief smoke assertion.
- Tightened the debrief summary assertion so it rejects save-slot, account, continue, `End Run` and `Restart Mission` copy in normal debrief actions.
- Kept saved-account diagnostic scripts separate from the normal debrief smoke.
- Renamed the hidden automatic mission receipt persistence log to `MC2 company snapshot updated` and stopped printing the saved-account file path in that normal-flow log.
- Restored Unity scene fileID churn after each Windows build; no scene content change is part of this task.

Modified files:

```text
unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs
unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-debrief-summary.txt
docs-reference-visual-audit-2026-06-07.md
docs-playable-demo-locked-execution-plan-2026-06-07.md
docs-mc2-detailed-development-plan.md
```

Validation commands:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-debrief-player-flow.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-debrief-summary.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-debrief-summary.log"
rg -n "End Run|Restart Mission|Save slot|save slot|saved account|saved-account|account|continue|Start Fresh|Load Check|Save Result|Slot " analysis-output/unity-player-debrief-summary.log unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-debrief-summary.txt
```

Validation evidence:

```text
analysis-output/unity-build-debrief-player-flow.log
analysis-output/unity-player-debrief-summary.log
```

Validation results:

```text
git diff --check: clean, with Windows line-ending warnings only.
Build: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
Debrief smoke: MC2 demo smoke test exiting with code 0.
Debrief assertion: actions=Repair & Mech Lab/Next Contract/Retry Battle/Close.
Copy audit: no matches for End Run, Restart Mission, save-slot, saved-account, account, continue, Start Fresh, Load Check, Save Result or Slot in the debrief smoke log and debrief command script.
```

Observed effect:

- A player finishing a mission sees the immediate next choices instead of system lifecycle language.
- Result, objective count, damage, salvage, bounty, payout and repair cost remain visible.
- Normal debrief evidence is now clean; explicit saved-account scripts remain available for separate diagnostics.

Remaining issues:

1. Resolved by Stage 5.2: validator now proves immediate repair restores relaunch eligibility.
2. The Mech Lab can still contain hidden saved-account diagnostic affordances; those should stay out of normal flow unless explicitly requested.

Next priority:

1. Stage 6 / D3 show optional AI advice window.

## Stage 6.2 AI Directive Adapter Result

Implemented on 2026-06-07:

- Added validator coverage for all four legal directive tokens: `assault-objective`, `engage-hostiles`, `regroup` and `hold`.
- Added invalid directive fallback coverage: unknown model text normalizes through `assault-objective`.
- Added ended-observation coverage: every directive returns no local command once the mission is ended.
- Added no-direct-mutation coverage: directive conversion returns a command string and does not mutate `BattleMission` orders by itself.
- Added missing-key fallback coverage for the MiniMax result path.
- Updated startup `-mc2MinimaxCommanderSteps` so missing `MINIMAX_API_KEY` logs model unavailability and continues through local rule fallback instead of blocking.

Modified files:

```text
unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs
unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs
docs-ai-commander-directive-contract.md
docs-playable-demo-fine-grained-current-plan-2026-06-07.md
docs-playable-demo-locked-execution-plan-2026-06-07.md
docs-reference-visual-audit-2026-06-07.md
```

Validation commands:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-directive.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-ai-directive.log"
$env:MINIMAX_API_KEY=''; & "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2MinimaxCommanderSteps 1 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-ai-directive-fallback.log"
```

Validation evidence:

```text
analysis-output/unity-validate-ai-directive.log
analysis-output/unity-build-ai-directive.log
analysis-output/unity-player-ai-directive-fallback.log
```

Validation results:

```text
git diff --check: clean, with Windows line-ending warnings only.
Validator: MC2 demo contract validation OK.
Build: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
Fallback smoke: source=rule_fallback directive=assault-objective command=squad move 3136 -789.333.
Fallback smoke exit: MC2 demo smoke test exiting with code 0.
```

Observed effect:

- AI directives are now ordinary local command suggestions, not mission mutations.
- Missing model configuration no longer stops the startup commander path.
- The first-demo local fallback remains deterministic and uses `assault-objective`.

Remaining issues:

1. D3 still needs to surface this AI capability as a small optional UI window rather than a debug log.
2. Later work should keep model calls asynchronous or pre-mission/paused, not live frame-by-frame.

Next priority:

1. Stage 6 / D3 show optional AI advice window.

## Stage 6.1 Compact AI Observation Result

Implemented on 2026-06-07:

- Added compact observation schema `mc2-ai-observation-compact-v1` beside the existing full `CommanderObservation`.
- The compact observation includes mission phase, commander identity, objective summary, bounded player states, section damage tags, detached command count, hostile pressure, nearby threat summaries and available directive intents.
- Full observation remains available to local deterministic systems such as `RuleCommander`, validator diagnostics and explicit debug reports.
- `MiniMaxCommander` now prefers the compact summary for model prompts and caps completion output to a one-token directive-sized budget.
- Validator coverage now proves compact schema, compact player damage state, hostile pressure, available intents, compact JSON size, MiniMax prompt size and forbidden detail exclusions.

Modified files:

```text
unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs
unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs
unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs
docs-ai-commander-directive-contract.md
docs-playable-demo-fine-grained-current-plan-2026-06-07.md
docs-playable-demo-locked-execution-plan-2026-06-07.md
docs-reference-visual-audit-2026-06-07.md
```

Validation commands:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-observation.log"
```

Validation evidence:

```text
analysis-output/unity-validate-ai-observation.log
```

Validation results:

```text
git diff --check: clean, with Windows line-ending warnings only.
Validator: MC2 demo contract validation OK.
```

Observed effect:

- AI model input is now a compact phase summary instead of full unit/hostile/objective arrays.
- The prompt excludes exact unit positions, move targets, attack target ids, projectile history, path graphs and per-frame traces.
- Local battle continues to run without AI, and local command selection still has access to the full observation.

Remaining issues:

1. D2 still needs to guard model directive output as ordinary BattleCore command intent with strict fallback.
2. D3 still needs the small optional AI advice window in the first-demo UI.

Next priority:

1. Stage 6 / D2 guard AI directive adapter.

## Stage 5.2 Repair Relaunch Result

Implemented on 2026-06-07:

- Added validator coverage for the first-version repair/relaunch loop.
- The validator now creates an isolated mission, destroys every player mech through its critical section, and confirms relaunch/runtime swap is blocked while all player mechs are destroyed.
- The validator then repairs those mechs immediately with sufficient one-token currency, verifies the exact token delta, and confirms all repaired roster entries return to 100% condition and deployable mission state.
- The validator confirms equipped weapon stock is not consumed by repair, matching the first-demo rule that ordinary weapon loss is a cost/rebuy concern rather than a blocked flow.
- The validator confirms `TryBuildRestartRuntimeSwap` can construct a new BattleMission after repair and that repaired mech `activeLoadoutId` values are preserved through relaunch.
- Visible-flow smoke still passes through debrief, Mech Lab launch, restart identity and loadout compact checks.

Modified files:

```text
unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs
docs-reference-visual-audit-2026-06-07.md
docs-playable-demo-locked-execution-plan-2026-06-07.md
docs-mc2-detailed-development-plan.md
```

Validation commands:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-repair-relaunch.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-repair-relaunch.log"
```

Validation evidence:

```text
analysis-output/unity-validate-repair-relaunch.log
analysis-output/unity-player-repair-relaunch.log
```

Validation results:

```text
git diff --check: clean, with Windows line-ending warnings only.
Validator: MC2 demo contract validation OK.
Visible-flow smoke: MC2 demo smoke test exiting with code 0.
Smoke checkpoints: debrief summary OK, mech bay launch accepted, restart identity OK, loadout compact OK.
```

Observed effect:

- The battle loop now has proof for the intended first-demo rule: damaged or destroyed owned mechs can be repaired immediately with currency and used again.
- Repair does not introduce a wait timer, permanent first-demo loss, save-slot management or weapon-stock mutation.
- Relaunch continues to use the repaired roster and loadout identity.

Remaining issues:

1. AI deputy work is still only partially framed; D1 should freeze the compact observation contract before adding any advice window.
2. Mech Lab still has hidden saved-account diagnostics for explicit scripts; keep them hidden from normal player flow.

Next priority:

1. Stage 6 / D1 freeze compact AI observation.

## Stage 6.3 Optional AI Advice Window Result

Implemented on 2026-06-07:

- Added a compact AI Deputy / AI副官 subsection to the System panel.
- The panel shows `State`, `Mode`, `Intent`, and one short `Advice` line.
- The UI state builder reads `MiniMaxCommander.ConfigFromEnvironment()` and a temporary local `CommanderObservationPort` observation only; drawing the panel does not make a model call.
- The local fallback directive is derived from `RuleCommander`, keeping AI as a high-level deputy window instead of a live combat controller.
- Added `assert-ai-deputy-window` to startup command files and visible-flow smoke.
- Added parser validator coverage for the new assertion and malformed payload rejection.
- Restored Unity scene fileID churn after the Windows build; no scene content change is part of this task.

Modified files:

```text
unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs
unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs
unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs
unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt
docs-ai-commander-directive-contract.md
docs-playable-demo-current-execution-plan-2026-06-07.md
docs-reference-visual-audit-2026-06-07.md
```

Validation commands:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-advice-window.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-ai-advice-window.log"
$env:MINIMAX_API_KEY=''; & "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-ai-advice-window.log"
```

Validation evidence:

```text
analysis-output/unity-validate-ai-advice-window.log
analysis-output/unity-build-ai-advice-window.log
analysis-output/unity-player-ai-advice-window.log
```

Validation results:

```text
git diff --check: clean, with Windows line-ending warnings only.
Validator: MC2 demo contract validation OK.
Build: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
Visible-flow smoke: MC2 demo smoke test exiting with code 0.
AI deputy assertion: state=Offline mode=Local fallback intent=assault-objective advice=Advance objective.
```

Observed effect:

- AI now appears as an optional deputy capability in a pause/system surface, not as required gameplay.
- Missing model configuration no longer blocks or hides the capability story; the first-demo local path remains complete.
- Normal battle HUD stays sparse because the AI window is not drawn in the active combat surface.

Remaining issues:

1. AI calls should stay asynchronous or pre-mission/paused in later work.
2. The next product priority returns to game visibility: re-audit `hangar-contact` and `damage-demo` occupancy/readability with current screenshots and sidecars.

Next priority:

1. V1 re-audit battle occupancy readability.

## V1 Battle Occupancy Readability Re-Audit Result

Implemented on 2026-06-07:

- Refreshed `spawn`, `airfield`, `hangar-contact`, `damage-demo`, and `north-patrol` reference captures at 1280x720.
- Re-read sidecars for active/visible hostile counts, BattleCore occupancy, occupancy placeholders, scale summaries, terrain summaries, and occlusion fade.
- Inspected the key pressure screenshots directly.
- This task did not change gameplay, mission triggers, enemy count, collision, camera, UI, or art code.

Validation command:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
git diff --check
```

Refreshed evidence:

| Preset | Mission Time | Camera Ortho | Active Hostiles | Visible Hostiles | Occupancy Evidence | Readability Judgment |
| --- | ---: | ---: | ---: | ---: | --- | --- |
| `spawn` | 1.06s | 29.11 | 0 | 0 | `hardProps=80`, `placeholders=81` | Squad, shore, terrain, water and prop scale are readable. No combat pressure yet. |
| `airfield` | 12.79s | 29.11 | 12 | 8 | `hardProps=80`, `placeholders=81` | Contact is readable. Enemy group is visible but still near the hangar objective, with manageable density. |
| `hangar-contact` | 20.77s | 29.11 | 20 | 16 | `BattleOccupancy=units 23/29`, `hardProps=80`, `placeholders=81` | Units no longer appear to share one coordinate, but the fight compresses around the hangar, trees, structure mesh and damage effects. |
| `damage-demo` | 38.64s | 35.50 | 20 | 20 | `BattleOccupancy=units 22/29`, `hardProps=80`, `placeholders=81` | Weakest screenshot. Status rows prove damage, but the world-space damage story is buried in a fully visible 20-hostile knot around the same objective. |
| `north-patrol` | 54.69s | 29.11 | 24 | 10 | `hardProps=80`, `placeholders=81` | Best combat readability. Same assets read better in open terrain, proving the main issue is local encounter composition rather than missing models alone. |

Failure classification:

- Physical overlap: not the primary current failure. BattleCore reports unit, structure, hard prop, water and map-bound occupancy, and placeholders expose `81` blocker regions from `BattleMission.OccupancyPlaceholderRegions`.
- Camera compression: major contributor in `hangar-contact` and `damage-demo`. `damage-demo` is already zoomed out to `35.50`, yet all 20 hostiles are visible around one target window.
- UI occlusion: secondary. Left status rows are visually heavy but not the main reason the hangar fight compresses; right combat/objective panels stay out of the central fight.
- Model scale: not the primary failure. Sidecars still report stable reference category scale, and `north-patrol` proves the same mechs/vehicles/props can read in open terrain.
- Dark or flat material: not the primary failure. Terrain remains readable with `luma=81/98.8/187`.
- Too much FX: secondary. Damage and attack cues help status readability, but they add clutter where the encounter is already dense.
- Enemy density and local encounter composition: primary failure. `hangar-contact` has 20 active / 16 visible hostiles, while `damage-demo` has 20 active / 20 visible hostiles centered on the same hangar objective.

Current conclusion:

The next fix should not start by inventing new Unity-only collision. The blocker evidence is present and auditable. V2 should improve battlefield readability by reducing hangar-window compression: camera composition, local enemy/attack-slot spread, selective FX scale/visibility, and possibly a stronger world-space damage spotlight for `damage-demo`. Enemy count and mission trigger semantics should stay intact unless a later task explicitly decides to retune encounter pacing.

Single weakest screenshot:

```text
analysis-output/reference-visual-captures/damage-demo.png
```

Next priority:

1. V2 improve reference visual readability.

## V2 Reference Visual Readability Result

Implemented on 2026-06-07:

- Strengthened player-unit damage ground cues in `DemoUnitView`: damaged player mechs now get larger ground rings and taller beacons for critical, lost-section, and pilot-risk states.
- Strengthened the presentation-level player damage warning markers in `Mc2DemoBootstrap`: critical player damage now gets a larger ground spotlight, taller beacon, and a compact world-space flag.
- Added auditable summary tokens: `PlayerDamage=warning+critical+beacon+spotlight+flag` and `Ground=critical+lost+pilot+spotlight`.
- Kept BattleCore mission rules, enemy count, trigger timing, movement, landing legality, collision occupancy, camera, and loadout rules unchanged.
- Restored Unity scene fileID churn after build; no scene content change is part of this pass.

Modified files:

```text
unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs
unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs
docs-reference-visual-audit-2026-06-07.md
docs-playable-demo-current-execution-plan-2026-06-07.md
docs-playable-demo-overall-detailed-plan-2026-06-07.md
```

Validation commands:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-visual-readability.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-visual-readability.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

Validation evidence:

```text
analysis-output/unity-validate-visual-readability.log
analysis-output/unity-build-visual-readability.log
analysis-output/reference-visual-captures/spawn.png
analysis-output/reference-visual-captures/airfield.png
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/north-patrol.png
analysis-output/reference-visual-captures/hangar-contact.json
analysis-output/reference-visual-captures/damage-demo.json
```

Validation results:

```text
git diff --check: clean, with Windows line-ending warnings only.
Validator: MC2 demo contract validation OK.
Build: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
Reference captures: command exited successfully for spawn, airfield, hangar-contact, damage-demo, north-patrol.
```

Observed effect:

- `damage-demo` still carries the same encounter pressure: 20 active / 20 visible hostiles, `orthographicSize=35.50`, `hardProps=80`, `placeholders=81`.
- `hangar-contact` still carries the same encounter pressure: 20 active / 16 visible hostiles, `orthographicSize=29.11`, `hardProps=80`, `placeholders=81`.
- The world now gives damaged player mechs a clearer red/orange spotlight and flag instead of relying only on the left status rows.
- The fix does not hide or remove hostiles; it improves the player damage story inside the existing dense fight.

Remaining issues:

1. The hangar fight is still dense by design because 20 hostiles are active around one objective window.
2. The next collision-related work should focus on a debug/review occupancy layer, not on more invisible rule changes.
3. A later combat-feel pass can still add more expressive limb/cockpit event animation, but this pass makes the current damage state more screenshot-readable.

Next priority:

1. V3 lock occupancy placeholder review layer.
