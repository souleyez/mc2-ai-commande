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
movement/pathfinding data. Packet 0 decodes to a 100 x 100 `PostcompVertex`
grid, where each 32-byte record contains normal, elevation, texture, light,
terrain type, and water flags. The Unity demo contract now exports all 10000
height samples as `terrainMesh`, and the runtime builds a low-poly source-driven
terrain mesh plus a water plane from that data.

Packet 1 decodes to 1000 original terrain object records, each matching the
original 40-byte `ObjDataLoader` layout. The Unity demo contract exports those
objects as lightweight props: mostly trees plus 44 building records. The
objective hangar remains represented by the targetable `staticObjects` entry so
combat logic can damage it cleanly.

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

## Encounter Timing Notes

The first playable Unity slice now mirrors the key trigger timing rather than
trying to run the full ABL warrior scripts:

- The mission ABL sets `patrol1_triggered`, `patrol2_triggered`, and
  `patrol3_triggered` together when objective `0` completes. The Unity demo
  follows that timing for the first patrol and north island patrol groups, while
  keeping their objective targets hidden until the visible objective chain
  reaches them.
- The infantry ambush warrior scripts do not simply wake on the airfield
  trigger. They watch the hangar object damage and switch to ambush behavior
  when `HangarAttacked` becomes true. The Unity demo activates the infantry
  ambush only after `structure-1-0` takes damage.
- The `Starslayer`, `Urbies`, and west-side `LRMs` group remains tied to hidden
  objective `7`, which completes when a player unit enters the large Starslayer
  area. Their voice-over hook remains represented by hidden objective `8`.
- East and west `mc2_01_LRMs` are classified by spawn position, not current
  position, so patrol movement cannot accidentally move a unit across the
  activation rule boundary.

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

## Loadout Contract Direction

The first loadout contract is intentionally BattleCore-only and UI-agnostic. It
models the player-facing fitting loop as:

- a chassis definition with heat limit, weight limit, section health, a fixed
  rectangular slot grid, blocked grid cells, and optional equipment slots
- item definitions for weapons, armor plates, heat sinks, radar, and jump jets
- item shape cells for grid placement, including tall weapon shapes such as the
  original vertical three-cell style
- placed loadout items with grid coordinates or a special equipment slot id

The Unity validator now carries a tiny synthetic contract that proves the data
shape can express a 3 x 3 chassis grid, a vertical weapon, single-cell armor and
heat-sink items, plus radar and jump-jet slots. BattleCore also validates
placement overlap, blocked cells, rotation, bounds, heat caps, weight caps, and
special-slot compatibility before the mech bay becomes editable. The current
mech bay UI projects each source weapon loadout into a simple validator-backed
preview so the player can see heat, load, grid occupancy, fitting status, and a
compact occupied-slot grid before direct editing is added. The projection now
uses placeholder multi-cell shapes for source weapon families such as LRM racks,
SRM packs, autocannons, PPCs, and large lasers. It also supports temporary weapon
on/off toggles and projected weapon nudges, proving that fitting edits can
immediately recompute heat, load, grid occupancy, overlap, bounds, and validator
status without touching live battle damage yet. Spare load is now projected into
free cells as simple armor plates and heat sinks, exposing hardness and cooling
totals before full inventory editing exists. Those projected filler cells can
also be cycled between armor, heat sink, and empty so the first fitting tradeoff
is interactive without committing to a saved inventory format yet. The UI now
separates draft edits from the last applied demo fit, with reset returning to
that applied state; this is still a runtime preview boundary, not saved inventory.
Applied demo fits now hand effective weapon range, damage, cooldown, heat, and
cooling into combat and commander observation without changing source profile data.
Armor hardness uses a single incoming-damage multiplier, while heat sinks add to
the runtime cooling rate; both remain demo-only effects until saved loadouts exist.
Applied armor/cooling bonuses now appear in the unit weapon status, and combat
events carry blocked damage so the log can explain why a hit landed for less.
The mech bay now builds a starter runtime inventory from the player squad, source
weapons, projected armor plates, projected heat sinks, and a demo token balance,
giving later shop/salvage/repair work a small contract to grow from.
Current draft fits are now checked against that starter inventory, so extra
armor plate or heat sink choices show a shortage and cannot be applied.
The same starter inventory now owns the demo token balance used by the mech bay:
damaged player mechs show condition and repair cost, and a one-click repair
spends local tokens to restore structure and damaged sections.
Mission end now also creates a local inventory receipt: completed bounty points
increase the demo token balance, while destroyed enemy unit labels become
mech-fragment stacks for later assembly rules.
Those fragment stacks now feed a read-only starter assembly preview that shows
the nearest mech type, current fragment count, demo threshold, and ready state.
When a stack reaches the demo threshold, the local inventory now consumes the
ready fragments and adds a 100% condition warehouse mech for later roster work.
The mech bay now reads that same inventory as a compact owned-mech roster, so
assembled warehouse mechs are visible before any future squad-selection flow.
Roster entries can now be cycled into a read-only detail preview with source,
condition, availability, chassis, loadout id, and inventory id.
Assembled depot mechs are intentionally held with a pending-loadout placeholder,
so they are visible as assets but not treated as deployable squad mechs yet.
The roster detail now includes a disabled Draft Fit control, reserving UI space
for the later depot fitting flow without changing mission deployment rules.
That stub now previews two future requirements: spare weapon stock and a pilot
assignment, both missing for newly assembled depot mechs in the current demo.
The roster detail also includes a read-only pilot placeholder, so depot mechs
have an explicit future pilot/social system slot without enabling pilot hiring
or assignment yet.
It now exposes spare weapon stock counts too, making it clear when all starter
weapons are mounted on the active squad and no depot fitting weapon stock is
available yet.
The mech bay also exposes an ordinary weapon shop preview, establishing where
future spare depot weapons come from before full shop balancing or fitting
actions are enabled.
That shop now has a tightly scoped demo purchase path: buying the first ordinary
weapon spends local tokens and adds one unequipped weapon stack, making the
depot spare-weapon requirement visibly resolvable without full shop balancing.
The same panel now previews starter NPC pilot hire candidates with token cost
and death-risk status. The demo hire path can now spend local tokens and assign
one NPC pilot to a warehouse mech, resolving the pilot side of the future depot
fitting gate while leaving friend pilots, wages, and real pilot death for later.
When a warehouse mech has both spare weapon stock and an assigned pilot, the
Draft Fit affordance now opens a readiness gate: it becomes clickable and gives
player feedback without consuming inventory or creating a real loadout yet.
That gate now opens a read-only warehouse draft-fit preview, showing the
selected warehouse mech, assigned pilot, first spare weapon stack, and a clear
note that no inventory or loadout changes are applied yet.
