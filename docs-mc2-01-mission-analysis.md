# mc2_01 Reference Mission Analysis

`mc2_01` is the first reference mission selected for the playable command-demo
slice. It is small enough to inspect quickly, but still uses the normal campaign
mission shape: a `.fit` mission configuration, an `.abl` script, and a `.pak`
mission map payload.

Generated local artifacts:

```text
analysis-output/mission-extract/project-owned-linked-dev/mc2_01/
analysis-output/mission-analysis/project-owned-linked-dev/mc2_01/mission-analysis.json
analysis-output/mission-analysis/project-owned-linked-dev/mc2_01/mission-analysis.md
analysis-output/unity-demo-contract/project-owned-linked-dev/mc2_01/mission-contract.json
```

The generated artifacts are ignored by git because they are derived from the
private local reference content pack.

The mission `.pak` is also indexed locally with:

```text
analysis-output/pak-unpack/mc2_01.pak-safe/packet-index.json
analysis-output/mission-packet-audit/project-owned-linked-dev/mc2_01/mission-packet-audit.json
```

The packet audit now validates all `1906 / 1906` mission-map packets. The
earlier `172042` byte extraction was caused by `File::createWithCase(...)`
creating binary files through `_creat`, which let Windows text mode expand LF
bytes to CRLF while unpacking `mission.fst`. The fixed path uses `_open(...,
_O_BINARY, ...)`, so `data/missions/mc2_01.pak` now matches the fast-file table:
`171423` real bytes.

Packet 0 is the terrain payload, packet 1 is the terrain object payload used by
`GameObjectManager::countTerrainObjects(...)`, and packet 4 onward contains
movement/pathfinding data. Packet 1 decodes to 1000 original terrain object
records, each matching the original 40-byte `ObjDataLoader` layout. The Unity
demo contract now exports those objects as lightweight props: mostly trees plus
44 building records. The objective hangar remains represented by the targetable
`staticObjects` entry so combat logic can damage it cleanly.

One task-critical static object is now recovered without waiting on full packet
decompression: objective `1` (`Destroy Hangar`) includes a
`DestroySpecificStructure` condition at `3221.333,-277.333`. The Unity contract
exports that as `structure-1-0`, a targetable `Hangar` owned by the enemy team.
The BattleCore demo can now damage this structure and complete the corresponding
objective once it is destroyed.

## Runtime Facts

- Mission name: `mc2_01`
- Script: `mc2_01`
- Author: `mantis`
- Drop weight limit: `190`
- Starting resource points: `22000`
- Teams / players: `2 / 2`
- Terrain minimums: `-6400, 6400`
- Water elevation: `350`
- Starting camera: `x=2228.962158`, `y=-2076.076416`, rotation `59.625`,
  scale `0.444542`

## Unit Layout

The mission contains 29 configured warriors and 29 matching parts.

Player force:

| Index | Unit | Brain | Position | Squad |
|---:|---|---|---|---:|
| 1 | `Werewolf` | `PBrain` | `2496,-1941.3` | 2 |
| 2 | `Bushwacker` | `PBrain` | `2368,-1941.3` | 4 |
| 3 | `Bushwacker` | `PBrain` | `2538.7,-2069.3` | 5 |

Enemy and non-player force summary:

| Unit | Count |
|---|---:|
| `Infantry` | 8 |
| `LRMC` | 7 |
| `Centipede` | 4 |
| `Harasser` | 4 |
| `UrbanMech` | 2 |
| `Starslayer` | 1 |

The original mission does not need a complex commander AI for the first slice.
Most enemy behavior is expressed through named brains such as `mc2_01_Pat1`,
`mc2_01_LRMs`, `mc2_01_Urbies`, and `mc2_01_Starslayer`, plus objective or area
triggers.

## Objective Flow

The `.fit` file contains 6 visible objectives and 3 hidden trigger objectives.
The visible mission chain is:

1. Investigate Abandoned Airfield
2. Destroy Hangar
3. Destroy Bandit Patrol
4. Repair and Proceed to North Island
5. Locate and Destroy Bandits
6. Move All 'Mechs to Extraction Point

Flag edges recovered from the objective actions:

| From objective | To objective | Flag |
|---:|---:|---|
| 0 | 1 | `0` |
| 1 | 2 | `2` |
| 6 | 3 | `3` |
| 3 | 4 | `4` |
| 4 | 5 | `5` |

The interesting detail is objective `6`, a hidden duplicate of the first bandit
patrol kill condition. It sets flag `3`, which unlocks the "Repair and Proceed
to North Island" objective. This is a good example of how the original game uses
hidden objectives as mission-state glue instead of putting every transition in
script code.

Other hidden triggers:

- Objective `7`: moving into a large area powers up the `Starslayer` encounter.
- Objective `8`: killing the Starslayer lance triggers voice-over logic.

## Script Role

The ABL script is mostly glue around the declarative objective graph:

- initializes mission booleans and tutorial/camera state
- watches objective completion via `checkObjectiveStatus(...)`
- turns patrol, Starslayer, infantry, hangar, sensor, and music triggers on
- plays voice-over and objective music
- drives tutorial-style camera movement and movie mode
- returns `ScenarioResult`

For the Unity demo, the first pass should import the objective graph and unit
layout from data. The ABL behavior can be reduced to event hooks first, then
expanded only where a mission beat depends on it.

## Demo Implementation Notes

Use this mission as a data contract test:

- `MissionDefinition`: mission settings, camera, terrain extents, water height,
  nav markers, objective graph, and script name
- `UnitSpawn`: unit id, team, commander, pilot, unit type, brain, squad, starting
  position, rotation, and player flag
- `ObjectiveDefinition`: visible/hidden, activation flag, completion conditions,
  actions, marker, reward points, and title text
- `BattleCore`: evaluates objective conditions, unit life/death state, movement
  commands, attack targets, and mission result
- `UnityPresentation`: renders terrain, units, UI state bars, click commands,
  effects, and camera follow/zoom
- `ScriptBridge`: keeps a narrow event-hook interface for ABL-like mission beats
  and future AI/CLI control

This keeps the first Unity slice faithful to the original mission while avoiding
a direct dependency on the old script runtime.

The current exporter writes this contract as `mc2-unity-demo-contract-v1`. It is
not a final save format; it is a bridge format for the first playable Unity
prototype.
