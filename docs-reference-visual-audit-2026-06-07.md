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
