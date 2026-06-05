# Reference Visual Restoration Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Restore the private-development Unity demo toward the original mission's readable 3D terrain, map dressing, and mech silhouettes while keeping all original assets local, ignored, and replaceable.

**Architecture:** Keep `BattleCore` authoritative for combat and mission state. Add a private reference visual pipeline beside Unity Presentation that converts or loads original local map/model/texture data into Unity-readable runtime assets; generated original-art derivatives stay under ignored `analysis-output` or `Assets/PrivateReferenceArt`. Public builds must still fall back to project-owned or generated replacement packs.

**Tech Stack:** Unity 6, C#, Windows Standalone player, PowerShell content-pack tools, Python binary exporters, ignored local `project-owned-linked-dev` reference pack, TGL/TGA/TXM/FST/PAK/FIT mission data.

---

## Baseline

Current private bridge status:

- `scripts/content-pack/export_tgl_to_obj.py` can export selected TGL shapes to ignored OBJ/MTL/TGA folders.
- `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs` can load those ignored OBJ/TGA assets at runtime.
- `Mc2DemoBootstrap` has a Unity-internal screenshot command:

```powershell
& ".\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" `
  -screen-width 1280 -screen-height 720 -screen-fullscreen 0 `
  -mc2CaptureScreenshot ".\analysis-output\unity-internal-reference-capture.png" `
  -mc2CaptureQuit `
  -logFile ".\analysis-output\unity-player-reference-capture.log"
```

Known visual gaps:

- The TGL export is enough for silhouette, but not yet a faithful model/material pipeline.
- Team-color and palette behavior is approximate, so mech textures can still read as white/flat blocks.
- Terrain uses height samples, but not original terrain texture tiles, roads, water edge treatment, or original lighting.
- Terrain objects are placeholder primitives, not original building/tree/turret props.
- Unit orientation, helper nodes, damaged variants, weapon muzzle helpers, and ejection/detached parts are not yet mapped to source model nodes.

## Guardrails

- Do not commit original assets or generated derivatives from original assets.
- Keep `.gitignore` entries for `analysis-output/tgl-obj/`, `analysis-output/unity-reference-art/`, and `unity-mc2-demo/Assets/PrivateReferenceArt/`.
- Use exact staging; do not use broad `git add -A`.
- Restore `unity-mc2-demo/Assets/Scenes/Mc2Demo.unity` if Unity only rewrites file IDs.
- Every visual task needs at least one of:
  - a no-graphics smoke log proving load/mission behavior
  - a Unity internal screenshot proving visual output
  - a generated manifest/audit JSON proving extracted counts and mappings
- Keep private reference visuals optional. If ignored assets are missing, the demo must still boot with obvious development fallback visuals.

## Definition Of "Restored Enough" For First Slice

For `mc2_01`, the first restored reference slice is acceptable when:

1. Player mechs and first enemy families are recognizable by silhouette and facing at default camera zoom.
2. The airfield area shows source-driven terrain height plus original-like ground texture regions, road/runway strips, water, and tree/building placement.
3. The hangar objective uses a recognizable private reference prop or a clearly mapped project-owned stand-in.
4. Enemy wake-up groups appear at source positions with recognizable vehicle/mech classes.
5. A Unity-internal screenshot at 1280x720 is nonblank, not a pure color-block view, and shows the commander squad, airfield props, terrain, and UI without overlap.
6. Combat smoke still passes after visuals are enabled.

## Validation Commands

Build:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 `
  -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-reference-visual.log"
```

Combat smoke:

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" `
  -batchmode -nographics `
  -mc2SmokeTest `
  -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-combat-situation.txt" `
  -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-reference-visual-smoke.log"
```

Internal screenshot:

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" `
  -screen-width 1280 -screen-height 720 -screen-fullscreen 0 `
  -mc2CaptureScreenshot "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-reference-visual-capture.png" `
  -mc2CaptureQuit `
  -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-reference-visual-capture.log"
```

---

## Task 0: Freeze The Current Private Reference Bridge

**Files:**

- Modify: `.gitignore`
- Modify: `docs-mc2-ai-commander-demo-execution-plan.md`
- Modify: `unity-mc2-demo/README.md`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Create: `scripts/content-pack/export_tgl_to_obj.py`
- Create: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- Create: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs.meta`

**Steps:**

1. Run Unity build and combat smoke with the current bridge.
2. Run internal screenshot capture and keep the PNG under `analysis-output`.
3. Confirm `git status --short` has no generated original assets outside ignored paths.
4. Restore `unity-mc2-demo/Assets/Scenes/Mc2Demo.unity` if it only contains fileID churn.
5. Commit only source, docs, and `.meta` files for the bridge.

**Acceptance:**

- Smoke exits with code `0`.
- Capture log contains `Loaded private reference OBJ mesh` and `Loaded private reference TGA texture`.
- The screenshot shows reference mechs instead of primitive capsules/cubes.

## Task 1: Replace Runtime OBJ Parsing With A Stable Reference Visual Manifest

**Files:**

- Modify: `scripts/content-pack/export_tgl_to_obj.py`
- Create: `scripts/content-pack/export_reference_visual_pack.ps1`
- Create: `analysis-output/unity-reference-art/manifest.json` (ignored output)
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`

**Steps:**

1. Add a manifest writer that records exported asset id, source TGL path, OBJ path, copied TGA paths, node count, vertex count, triangle count, and helper-node names.
2. Add a PowerShell wrapper that exports the first-slice unit set:

```powershell
& .\scripts\content-pack\export_reference_visual_pack.ps1 `
  -Names werewolf,bushwacker,centipede,harasser,lrmc,urbanmech,starslayer
```

3. Update Unity loader to read manifest first and log exactly which manifest asset was chosen for each `unitType`.
4. Keep direct folder probing only as a fallback.

**Acceptance:**

- `manifest.json` lists the seven first-slice unit visuals.
- Unity log maps every first-slice `unitType` to a manifest entry.
- Missing manifest still falls back cleanly to primitive dev visuals.

## Task 2: Restore Mech Facing, Scale, And Node Hierarchy

**Files:**

- Modify: `scripts/content-pack/export_tgl_to_obj.py`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`

**Steps:**

1. Preserve TGL shape-node names in manifest and Unity child object names.
2. Apply source spawn rotation from `UnitSpawn.position.rotation` to the reference visual.
3. Add per-asset scale/orientation overrides in manifest instead of hardcoded `ReferenceVisualScale`.
4. Keep the current presentation-only squad spread separate from mission coordinates.
5. Capture before/after screenshots for default spawn view.

**Acceptance:**

- Werewolf and Bushwacker face the original drop direction.
- Enemy UrbanMech/Starslayer/vehicles face their source spawn directions when activated.
- No unit collapses into the ground or floats above terrain at default zoom.

## Task 3: Restore TGA/TXM Materials And Team Color Properly

**Files:**

- Modify: `scripts/content-pack/export_tgl_to_obj.py`
- Create: `unity-mc2-demo/Assets/Shaders/PrivateReferenceTeamColor.shader`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`

**Steps:**

1. Record all texture references per TGL material, not only the first `.tga`.
2. Inspect related `.txm` records from `textures.fst` and document whether they are palette/metadata/atlas references.
3. Add a Unity material path that keeps base texture detail while mapping source blue team-color pixels to player/enemy tint.
4. Support alpha or glow maps where TGL/MTL references `RGBX` or map-d style textures.
5. Add a material debug screenshot command output showing one mech per row in a neutral lighting test scene or compact runtime preview.

**Acceptance:**

- Mechs no longer read as solid blue, solid white, or flat silhouettes.
- Red/green/white mechanical details remain visible.
- Player and enemy team color is visible but does not erase texture detail.

## Task 4: Decode And Apply Original Terrain Texture Regions

**Files:**

- Modify: `.gitignore`
- Modify: `scripts/content-pack/export_unity_demo_contract.ps1`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MissionContract.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/ProjectSettings/GraphicsSettings.asset`
- Create: `scripts/content-pack/export_terrain_texture_audit.ps1`
- Create: `unity-mc2-demo/Assets/Shaders/SourceTerrainVertexColor.shader`

**Status:** Completed 2026-06-05. Terrain tile ids are exported into the mission contract, the audit script records terrain/water/texture/light distributions, and the Unity demo now uses a source-driven vertex-color terrain material. Full source terrain tile textures and lighting remain Task 5.

**Validation Evidence:**

- Build log: `analysis-output/unity-build-terrain-regions.log`
- Smoke log: `analysis-output/unity-player-terrain-regions-smoke.log`
- Screenshot: `analysis-output/unity-terrain-regions-capture.png`

**Steps:**

1. Audit packet 0 fields already decoded as elevation, texture id, light, terrain type, and water flags.
2. Export per-cell texture ids and terrain type into `terrainMesh` contract data.
3. Build a Unity terrain material/mesh path that colors cells by source texture region before full tile textures exist.
4. Map common terrain ids for `mc2_01`: runway/road, grass, dirt, water edge, cliff/rock, island ground.
5. Replace placeholder single-color terrain material with a source-driven multi-region debug material.
6. Capture the airfield start view.

**Acceptance:**

- The airfield/runway and surrounding ground read as different map regions.
- Water areas and shorelines are visually separable from land.
- Combat click and movement mapping still use existing mission coordinates.

## Task 5: Restore Terrain Tile Textures And Lighting

**Files:**

- Modify: `.gitignore`
- Modify: `scripts/content-pack/export_terrain_texture_audit.ps1`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Shaders/SourceTerrainVertexColor.shader`
- Create: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceTerrainTextureLibrary.cs`

**Status:** Completed 2026-06-05. The audit/export tool now writes an ignored referenced-terrain-texture manifest for `mc2_01`, copies only used TGA/TXM/list-frame assets into `analysis-output/terrain-reference-textures/`, and Unity builds an optional runtime terrain texture composite from that local manifest. Missing private assets still fall back to source-driven terrain colors.

**Validation Evidence:**

- Export manifest: `analysis-output/terrain-reference-textures/mc2_01/manifest.json`
- Build log: `analysis-output/unity-build-terrain-textures.log`
- Smoke log: `analysis-output/unity-player-terrain-textures-smoke.log`
- Screenshot: `analysis-output/unity-terrain-textures-capture.png`

**Steps:**

1. Locate source terrain tile texture names for texture ids used by `mc2_01`.
2. Export or copy only the referenced local TGA/TXM-derived textures into ignored reference-art output.
3. Build a lightweight texture atlas or material array for the terrain mesh.
4. Apply packet light values as vertex color or material multiplier.
5. Keep source-driven vertex colors as the fallback/debug path when texture mapping is unavailable.

**Acceptance:**

- The airfield, roads, grass, dirt, and water edge look like textured terrain, not flat debug colors.
- Lighting variation is visible but does not hide units.
- Internal screenshot shows terrain texture detail under the squad.

## Task 6: Restore Buildings, Trees, Turrets, And Props

**Files:**

- Modify: `scripts/content-pack/export_unity_demo_contract.ps1`
- Modify: `scripts/content-pack/export_reference_visual_pack.ps1`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MissionContract.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/StructureState.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoStructureView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Create: `unity-mc2-demo/Assets/Scripts/Presentation/ReferencePropLibrary.cs`
- Modify: `docs-mc2-01-mission-analysis.md`

**Status:** Completed 2026-06-05. Packet 1 terrain object `fileName` values are now carried as `assetId`s, targetable Hangar structures inherit the matched packet-1 source visual and rotation, and the Unity presentation attaches private reference OBJ/TGA props for the first-slice airfield/building set while keeping primitive fallbacks elsewhere.

**Validation Evidence:**

- Reference visual manifest: `analysis-output/unity-reference-art/manifest.json`
- Build log: `analysis-output/unity-build-reference-props.log`
- Smoke log: `analysis-output/unity-player-reference-props-smoke.log`
- Screenshot: `analysis-output/unity-reference-props-capture.png`
- Runtime prop load count: `ReferenceProps=loaded 336 fallback 663`

**Steps:**

1. Build a mapping from packet 1 terrain object records to source art names using `art.fst` CSV/INI data.
2. Add prop asset ids, rotations, and dimensions to the mission contract.
3. Reuse the reference visual loader for prop TGL/AGL shapes.
4. Start with the objective hangar, nearby buildings, tree clusters, and obvious airfield props.
5. Keep primitive fallback only for unmapped prop ids, with a distinct debug material.

**Acceptance:**

- The start airfield has recognizable buildings/trees instead of small bars/cubes.
- The target hangar has a recognizable reference visual and remains targetable.
- Destroyed/damaged structure state still works.

## Task 7: Connect Damage, Ejection, And Detached Parts To Real Model Nodes

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `scripts/content-pack/export_tgl_to_obj.py`

**Status:** Completed 2026-06-05. Reference OBJ nodes are now bucketed by common cockpit/arm/leg/torso names, detached damage effects clone the real reference nodes before falling back to primitives, and the `damage-demo` capture preset forces a reproducible left-arm, leg-collapse, and cockpit-ejection scene. The local source pack has independent arm-damage TGL variants for Werewolf/Bushwacker; this slice records section node manifests and uses live real-node clones first, leaving exact damaged-variant asset swaps as the next fidelity pass.

**Validation Evidence:**

- Python syntax check: `$env:PYTHONDONTWRITEBYTECODE='1'; python -m py_compile scripts/content-pack/export_tgl_to_obj.py`
- Build log: `analysis-output/unity-build-reference-damage-nodes-r2.log`
- Smoke log: `analysis-output/unity-player-reference-damage-nodes-smoke-r2.log`
- Damage capture: `analysis-output/reference-visual-captures/damage-demo.png`
- Damage sidecar/log: `analysis-output/reference-visual-captures/damage-demo.json`, `analysis-output/reference-visual-captures/damage-demo.log`
- Damage log proves real-node paths: `Left Arm: ww_larm`, `Left Leg: bw_lfoot, bw_llleg, bw_luleg`, `Right Leg: bw_rfoot, bw_ruleg`, `Cockpit: bw_torso`

**Steps:**

1. Map common node names to cockpit, arms, legs, torso, and weapon helpers.
2. Replace primitive detached arm/cockpit effects with cloned reference node visuals when a mapped node exists.
3. Keep current primitive damage effects as fallback.
4. Use TGL damaged variants where available, beginning with Werewolf/Bushwacker arms and cockpit/ejection cues.
5. Capture a command-file scenario that forces arm loss, leg collapse, and cockpit ejection.

**Acceptance:**

- Arm loss and cockpit ejection read as mech parts, not generic cubes.
- Existing combat smoke still passes.
- A dedicated damage visual screenshot proves at least one real-node detach path.

## Task 8: Restore Original Camera Feel Without Losing Mobile Simplicity

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Use existing: `unity-mc2-demo/Assets/Scripts/BattleCore/MissionContract.cs`
- Generated ignored: `unity-mc2-demo/Assets/StreamingAssets/Missions/mc2_01/mission-contract.json`

**Status:** Completed 2026-06-05. The Unity demo now reads the mission camera block and derives the fixed tactical pitch, yaw, orthographic size, limited zoom range, and commander follow offset from the original `mc2_01` start camera while preserving the simplified mobile-friendly follow behavior.

**Validation Evidence:**

- Contract export: `scripts/content-pack/export_unity_demo_contract.ps1`
- Build log: `analysis-output/unity-build-source-camera.log`
- Smoke log: `analysis-output/unity-player-source-camera-smoke.log`
- Screenshot: `analysis-output/unity-source-camera-capture.png`
- Runtime camera line: `MC2 source camera configured: pitch=68.75 yaw=-59.625 ortho=29.109 height=80.05 zoom=0.72-1.25 followOffset=(-2.67, 0.00, -1.35)`

**Steps:**

1. Read mission camera `projectionAngle`, `startPosition`, `startRotation`, `newScale`, `zoomMin`, and `zoomMax`.
2. Use those values to derive Unity orthographic size, yaw, pitch, and follow offset.
3. Keep only limited zoom in/out for development and future player ergonomics.
4. Ensure default camera follows commander while preserving original mission framing.

**Acceptance:**

- Default view resembles original tactical perspective and keeps the commander squad readable.
- UI panels do not overlap the commander squad at 1280x720.
- Click-to-move and attack raycasts remain correct.

## Task 9: Add Visual Regression Captures

**Files:**

- Modify: `.gitignore`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Create: `scripts/unity/capture_reference_visuals.ps1`
- Create: `analysis-output/reference-visual-captures/` (ignored output)

**Status:** Completed 2026-06-05. The demo now supports named visual capture presets, writes a JSON sidecar beside each screenshot, and the PowerShell capture script can produce and sanity-check visual proof images for the reference slice.

**Validation Evidence:**

- Build log: `analysis-output/unity-build-reference-captures.log`
- Capture script: `scripts/unity/capture_reference_visuals.ps1`
- Capture output folder: `analysis-output/reference-visual-captures/`
- Verified presets: `spawn`, `airfield`, `hangar-contact`, `north-patrol`, `damage-demo`
- Sidecar highlights: `spawn` recorded 0 active hostiles; `airfield` recorded 12 active hostiles and 8 visible hostiles.

**Steps:**

1. Add named capture presets: `spawn`, `airfield`, `hangar-contact`, `north-patrol`, and `damage-demo`.
2. Let each preset run startup commands or advance seconds before capturing.
3. Save PNG and a small JSON sidecar with mission state, camera, visible hostiles, and loaded reference assets.
4. Add a simple image sanity check: dimensions, nonblank, enough unique colors, and no full-screen UI occlusion.

**Acceptance:**

- One command produces a folder of visual proof images.
- At least `spawn` and `airfield` captures pass sanity checks before each visual commit.

## Task 10: Prepare Replacement-Pack Boundaries While Using Original Reference Art

**Files:**

- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-content-pack.md`
- Create: `content-packs/project-owned-visual-slice.example.json`

**Steps:**

1. Document that private reference visual manifests are development-only.
2. Add a pack manifest field for replacement visual provenance.
3. Keep all Unity loader APIs asset-id based, so a project-owned pack can replace original-derived paths later.
4. Create an example clean pack manifest for a future investor-safe visual slice.

**Acceptance:**

- The Unity code never depends on original product names for public content.
- The same visual ids can point to original local reference assets or project-owned replacements.

---

## Recommended Execution Order

1. Task 0: Freeze current bridge.
2. Task 1: Manifest the exported private reference assets.
3. Task 3: Fix material/team-color readability, because model silhouettes are already loading but still look too flat.
4. Task 2: Correct facing/scale/node hierarchy.
5. Task 4 and Task 5: Restore terrain regions, then real terrain textures.
6. Task 6: Restore props around the airfield.
7. Task 8 and Task 9: Lock camera and regression captures.
8. Task 7: Hook damage/ejection into real nodes.
9. Task 10: Keep replacement-pack documentation aligned.

## First Commit Target

The next commit should contain only:

- current private bridge source files
- internal screenshot command
- docs updates
- no generated OBJ/TGA/PNG artifacts

Commit message:

```text
Bridge private reference visuals into Unity demo
```

After that, start Task 1 in a new small commit.
