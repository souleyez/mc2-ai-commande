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
mech bay UI focuses one selected squad mech at a time through compact squad
buttons with draft markers and selected-mech fit-pressure bars, then projects that source weapon loadout into a simple validator-backed
preview so the player can see heat, load, grid occupancy, fitting status, and a
compact occupied-slot grid before direct editing is added. Compact heat, load,
and grid usage bars now sit under the numeric fit line, making over-limit pressure
visible without opening another inspector. The projection now
uses placeholder multi-cell shapes for source weapon families such as LRM racks,
SRM packs, autocannons, PPCs, and large lasers. Fitted weapons are now always
active, matching the original payload model more closely: the weapon list only
selects mounted weapons for projected nudges, while fitting edits immediately
recompute heat, load, grid occupancy, overlap, bounds, and validator status
without touching live battle damage yet. The weapon list highlights the selected
mounted weapon and repeats its current or pending grid position, damage, range,
cooldown, heat, weight, footprint cells, and footprint shape above compact S/M/L plus `WxH` range/shape buttons instead of adding a separate weapon switch layer; the active button also replaces its number with `>` for color-independent selection feedback. The payload preview now draws
those projected items as larger whole-block grid pieces, following the original
payload-model color idea of orange empty cells, green/blue/red weapon range bands,
and yellow components. The grid also exposes selected or hovered block details
for weapon heat, weight, damage, range, cooldown, and component bonuses, with a
clear hover frame around whole blocks or open cells. Clicked component and open
cells stay selected, keeping armor/sink tradeoff details visible after the pointer
moves away; the same selected target cell can now place the selected mounted
weapon or cycle a spare filler cell through armor, heat sink, and empty. The
single-cell filler action now labels the next action as Armor, Sink, or Clear
instead of a generic Fill button, and the top status plus selected target status
echo a compact transition such as Empty > Armor beside the weapon-placement state. Before
Place applies that draft move, the selected weapon now draws a target ghost over
the destination cells, with green for a clear placement and red for overlap or
out-of-bounds pressure; blocked targets also disable the Place action and label
it Blocked, and the target status row now carries matching clear/current/blocked colors. When no destination cell has
been selected yet, the same control area prompts Pick slot instead of going blank. The detail line mirrors that signal with target
clear/blocked text, blocked reason, filler transition, selected weapon footprint cells, and concrete footprint shape, so the player does not
have to infer the result from color alone. The selected-weapon nudge buttons now disable directions that would leave the grid or overlap another payload block,
so invalid draft placement is blocked before Apply; a compact nudge status line lists those blocked directions with short outside/overlap reasons beside the disabled buttons. The selected-weapon Reset control
stays disabled as Base with a done-state color until that weapon has a placement override to clear. The
draft reset button now reads Clean with the same done-state color when there is no draft to revert. The filler action button now colors Armor, Sink, and Clear states by the next single-cell edit, while disabled weapon-filled or stacked targets read Locked or Stack. Slot
and draft reset actions clear that UI selection so restored fits do not keep pointing at stale grid coordinates.
Spare load is now projected into free cells as simple armor plates and heat sinks,
exposing hardness and cooling totals before full inventory editing exists. Those
projected filler cells can also be cycled between armor, heat sink, and empty so
the first fitting tradeoff is interactive without committing to a persistent loadout format yet. The UI now
separates draft edits from the last applied demo fit, with reset returning to
that applied state, and the Apply control labels and colors itself Done, Invalid, Stock, or
Apply based on draft, validation, and inventory state; this is still a runtime preview boundary, not a persistent
saved loadout.
Applied demo fits now hand effective weapon range, damage, cooldown, heat, and
cooling into combat and commander observation without changing source profile data.
Armor hardness uses a single incoming-damage multiplier, while heat sinks add to
the runtime cooling rate; both remain demo-only effects until saved loadouts exist.
Applied armor/cooling bonuses now appear in the unit weapon status, and combat
events carry blocked damage so the log can explain why a hit landed for less.
The mech bay now builds a starter runtime inventory from the player squad, source
weapons, projected armor plates, projected heat sinks, and a demo token balance,
giving later shop/salvage/repair work a small contract to grow from.
That inventory can now be wrapped in a local demo saved-account snapshot with
schema, account id, cloned inventory data, cached counters, validation, and JSON
round-trip coverage; the mech bay surfaces it only as a read-only account
summary, not a persistent user-data write. The command-file harness can also run
`saved-account-report`, which validates the snapshot and logs a JSON dry-run
before and after local candidate prep without writing any save file. Candidate
prep now also emits a read-only saved-account delta that names token, mech,
ready, depot, and item-stack count changes before persistent save/load is added.
The explicit `saved-account-save-load-preview` command serializes and loads that
snapshot in memory, validates the loaded account, and requires a zero-delta
round trip before the future file export/import path exists.
The explicit `saved-account-export <path>` and `saved-account-import-preview
<path>` commands now move that same JSON boundary onto disk only when a command
file provides a path; import preview validates and reports the delta without
applying the loaded account to the live mech bay.
The explicit `saved-account-import-apply-preview <path>` command adds the future
apply gate: it requires the loaded account to match the current account identity
and then reports the token/mech/depot delta that would be applied, while still
leaving the live mech bay unchanged.
The mech bay summary now shows the latest guarded apply preview as a read-only
confirmation row, so the pending save/load direction is visible before any real
apply command exists.
The explicit `saved-account-import-apply <path>` command now consumes that guard:
it only applies when the latest preview still matches the same path, JSON length,
and delta, then swaps the live demo mech bay to a cloned loaded inventory.
The mech bay summary exposes the same guard as an `Apply` action that stays
disabled until a matching import apply preview is ready.
The same summary now includes a saved-account JSON path field and `Preview`
action, so a manual import apply preview can be generated from the UI instead
of only from startup command files.
`Default` and `Export` helpers now fill a persistent demo save path and write
the current local account snapshot there before preview/apply.
A guarded `Load` helper now previews and applies that persistent demo save path
when the default save file exists.
The same summary also keeps a compact `Last Save` result line, so the latest
export, preview, apply, or blocked save/load outcome stays visible.
Guarded account-changing actions now auto-save the current local account
snapshot to that persistent demo save file after successful receipts, imports,
candidate prep, purchases, hires, fits, squad selection, or repairs.
Startup command files can also preview and apply that persistent default save
through `saved-account-load-default-preview` and
`saved-account-load-default-apply`, reusing the same identity and file-change
guard as manual imports.
They can now also run `saved-account-save-current-default` to write the active
account to that same persistent default save path before preview/apply checks.
The explicit `-mc2LoadDefaultSave` startup flag now uses that same guarded path
when the persistent default save file exists, and skips cleanly on first run.
Manual demo startup now shows a lightweight Continue/New Game panel when that
persistent default save exists and no automation startup args are present. The
panel previews account summary, token/depot/item counts, delta, and save
timestamp before enabling Continue. The pause/system panel can reopen the same
Save Choices entry, and New Game resets the active demo run without deleting
the persistent default save. New Game now requires an explicit confirmation,
system-opened Save Choices can return with Back, and Save Current writes the
active account to the persistent default save before refreshing the Continue
summary. The same panel now shows the latest save/load result inline, can
export a timestamped copy, and can reset the default slot only after confirmation
while copying the old default save first.
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
assembled warehouse mechs are visible before they are selected for a future
mission squad.
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
The preview now has a demo-only Apply action: it consumes one spare weapon stack
into the warehouse mech, swaps `pending-loadout` for a placeholder
`warehouse-demo-fit`, and still keeps that mech out of mission deployment.
Fitted warehouse mechs now show a deployment preview line in the roster detail,
marking them as ready for next-mission squad selection instead of silently
looking ready for the current mission.
The same roster detail now exposes a player-facing Next Squad entry. It
marks fitted depot mechs as candidates for the next mission while keeping the
current mission roster and deployment flags unchanged until confirmation.
That entry now opens a Next Mission Squad preview, listing the current mission
slots and any fitted depot candidates while keeping inventory and mission
deployment unchanged until the user confirms.
If the user opens it from a current mission mech, that mech is preselected as
the outgoing slot; opening it from a fitted depot candidate preselects that
mech as the incoming slot.
When both a current mission slot and a fitted depot candidate exist, the preview
now shows one replace plan that names the outgoing and incoming mechs.
That pair now becomes a local squad-selection draft state, preserving the
selected outgoing and incoming mech IDs separately from the read-only preview.
The draft row now has tiny cycle controls for outgoing mission slots and
incoming depot candidates, but cycling only changes the local draft IDs.
The Out/In rows now carry compact direction cues so the replacement direction
is visible before Confirm.
That plan feeds a pending confirmation row, so the staged replace-slot flow can
mutate only local mission availability flags.
The confirmation path now swaps the outgoing and incoming `availableForMission`
flags while preserving token, mech count, weapon count, and current combat
state.
After confirmation, the squad-selection preview now refreshes into a completed
state: the joined depot mech is treated as a current mission slot, no longer
appears as an incoming candidate, and the draft summary reports that the swap
has already been applied.
The mech bay now also shows a next-mission handoff built from the
`availableForMission` roster. It can include the joined depot mech after a
confirmed swap, and it now presents that state as a player-facing Ready/Blocked
summary, guarded Launch action, and lineup preview.
Behind that Launch action, each available mission slot is still mapped to a
spawn intent with commander/lancemate role, pilot, loadout, and depot-slot
marker before any live restart is allowed.
The guarded path also keeps its restart contract validation: it names the
`mc2_01` template, player team, commander slot, player-unit patch mode, and
spawn-intent count that the `BattleMission` recreation call consumes.
The next validation layer clones the template `MissionContract` and patches
only the cloned player unit spawns from the handoff roster. Enemy units,
objectives, terrain, triggers, and the source template remain unchanged.
That prepared contract is now also passed through a throwaway `BattleMission`
construction dry run. The dry run verifies that BattleCore can instantiate the
patched payload and reports unit, player-unit, structure, objective, and initial
result counts, but it never replaces the active combat mission.
The Unity presentation can now consume the same validated path for an actual
restart: it builds a replacement `BattleMission`, clears generated runtime scene
objects and transient command UI state, rebinds command/observation ports, and
rebuilds the world views without mutating inventory token or item counts.
The startup command harness can now trigger the same restart path with a
script-level `restart` action or `-mc2RestartMission`, so command-file smoke
tests can verify that post-restart observation and movement commands still run
against the rebuilt mission.
Repeated restart is also covered: generated scene roots are deactivated before
they are queued for destruction, and the restart demo script runs two rebuilds
before issuing another movement command.
The presentation now preserves the management context when restart is applied
from the mech bay: the rebuilt mission stays paused and the mech bay remains
open, while system, result-panel, and CLI restarts still resume directly into
the battle view.
The mech bay also reports the rebuilt roster state after restart: active player
slots, deployed owned mechs, fully ready mechs, repair needs, held depot mechs,
fit blockers, and unavailable mission mechs are visible in the summary line and
the combat log.
Restart identity now survives the runtime swap as explicit spawn metadata:
`UnitSpawn` carries owned mech id, pilot display name, and active loadout id,
`UnitState` exposes those fields to UI and commander observations, and inventory
condition sync prefers owned mech id before falling back to legacy unit id.
The command-file harness now has a demo-only depot swap identity check:
`prepare-depot-candidate`, `squad-swap`, `restart`, and
`assert-restart-identity depot` prove that a warehouse/depot slot can become a
runtime player unit and still report its owned mech identity after rebuild.
The harness also has a less smoke-only setup path:
`prepare-local-candidate` applies a local receipt, assembles a warehouse mech,
hires an NPC pilot, buys an ordinary weapon, applies the warehouse draft fit,
and then lets the same `squad-swap` path consume that ready depot candidate.
The visible mech bay summary now has a compact Candidate Prep action that calls
the same local setup chain, so the player-facing path can produce a ready depot
candidate and immediately open Next Squad with that candidate preselected.
The visible mech bay path now mirrors that handoff more closely: opening Squad
selection replaces the lower roster detail with an inline preview, keeps the
staged swap visible, and adds the same guarded next-mission Launch action inside
that preview instead of relying only on the summary rows above it.
The next-mission area now hides the earlier technical dry-run ladder from the
player-facing mech bay. It shows a Ready/Blocked summary, a guarded Launch
button, and the selected lineup, while still using the same cloned-contract and
replacement-`BattleMission` validation path before any live restart is allowed.
The Confirm and Launch rows now use shorter player-facing status text after a
swap, while the technical roster and restart summaries remain in logs and guard
state.
Those rows now also carry Ready/Blocked color cues on the action button and
status line, making disabled launch or confirm states visible without reading
the full text.
The active Out/In selection rows now have highlighted row backgrounds and the
replacement action is labeled Set, making the next-mission squad choice read
more like a player-facing operation than a debug confirmation.
After a confirmed swap, the squad-selection preview now switches into a compact
done state: it hides the Out/In picker rows, shows a `SET` replacement summary,
summarizes the updated lineup, and keeps only the confirmation status and
next-mission Launch action visible.
That completed-swap Launch row now states that it launches the updated squad,
so the handoff consequence is visible before the mission is rebuilt. If the
squad-selection preview is hidden after a completed swap, the general
next-mission handoff summary and Launch row preserve that replacement cue.
After that launch path runs with the mech bay kept open, the combat log repeats
the completed replacement summary.
The startup command-file smoke path now has a `mech-bay-launch` action that
exercises that same handoff without manual clicking. A second
`hide-squad-preview` smoke action hides the completed squad panel first, then
checks that the general next-mission handoff still names the updated squad
before Launch.
