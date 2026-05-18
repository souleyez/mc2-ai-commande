# Content Index Notes

Generated index:

```text
analysis-output/content-index/project-owned-linked-dev.content-index.json
```

The JSON artifact is ignored by git because it is generated from the local
reference-linked content pack.

## Current Pack

- Pack id: `project-owned-linked-dev`
- Runtime contract: `mc2-content-pack-v1`
- Seed mode: reference-linked local development pack

## Top-Level Runtime Data

Filesystem roots:

- `data`: 3593 files, about 406 MB
- `assets`: 24 files, about 2 MB
- `shaders`: 16 files, about 14 KB
- fast files: 8 archives, about 262 MB total

FST archives:

- `mission.fst`: 1346 entries
- `tgl.fst`: 4667 entries
- `textures.fst`: 1396 entries
- `art.fst`: 666 entries
- `insignia.fst`: 55 entries
- `misc.fst`: 43 entries
- `camera.fst`: 2 entries
- `effect.fst`: 1 entry

Important FST extension counts:

- `.tga`: 2951
- `.tgl`: 2030
- `.abl`: 991
- `.ini`: 773
- `.agl`: 750
- `.fit`: 417
- `.pak`: 44
- `.csv`: 44

## Mission Entry Pattern

Main mission entries follow this pattern:

```text
data/missions/mc2_01.abl
data/missions/mc2_01.fit
data/missions/mc2_01.pak
```

The same pattern continues through `mc2_24`. The `.abl` file is mission logic,
the `.fit` file is mission configuration, and the `.pak` file is mission map or
packed mission-local data.

Useful candidate missions:

- `mc2_01`: first campaign mission, good vertical-slice candidate
- `m0101`: small legacy/test mission candidate
- `tut_01`: tutorial candidate for interaction study
- `e3demo`: demo candidate, likely useful for presentation but less canonical

## First Demo Slice Choice

Use `mc2_01` as the first reference mission unless runtime testing shows a
blocker.

Reasons:

- It is a real campaign mission rather than a pure test map.
- It has the normal mission triad: `.abl`, `.fit`, `.pak`.
- Its `.pak` is small for a campaign map, about 171 KB.
- It should exercise real objective scripting without starting from a huge map.

## Mission Extraction

The selected mission triad can now be extracted from `mission.fst` into a local
analysis folder. That lets us inspect and translate the map/objective/script
structure without unpacking every archive by hand.

Command:

```powershell
& .\scripts\content-pack\extract_mission_from_pack.ps1 -MissionId mc2_01
```

Analyze the extracted mission:

```powershell
& .\scripts\content-pack\analyze_mission_extract.ps1
```

Export the Unity-facing contract:

```powershell
& .\scripts\content-pack\export_unity_demo_contract.ps1
```

Tracked notes for the first slice are in `docs-mc2-01-mission-analysis.md`.

## Next Development Step

Use the exported `mc2-unity-demo-contract-v1` JSON as the first BattleCore input
shape: mission settings, unit spawns, objective graph, trigger hooks, and map
payload boundaries.
