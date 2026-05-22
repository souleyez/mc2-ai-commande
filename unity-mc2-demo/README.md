# MC2 Unity Demo

This Unity 6 project is the first playable shell for the MC2-style command demo.
It reads `mc2-unity-demo-contract-v1` from `StreamingAssets` and builds a
placeholder battlefield at runtime.

Current demo behavior:

- loads `mc2_01` unit spawns and objective graph
- gates objectives with source flag and previous-primary prerequisites
- emits objective activation and completion events for mission logging
- routes objective, contact, combat, and result events through a script-signal bridge for later ABL/AI hooks
- builds a source-driven 100 x 100 terrain mesh and water plane from packet data
- places original terrain object records as lightweight trees and buildings
- activates enemy groups from source mission brain names and objective progress
- moves activated enemy groups with lightweight source-brain patrol orders backed by source nav markers
- follows the first player mech as commander
- defaults to squad orders, with status-bar click for detached single-unit order
- routes player commands through a CLI-ready commander command port for AI draft/directive tests
- exposes a CLI-ready commander observation JSON for compact AI planning summaries
- supports repeatable scriptable runtime restart from startup args or command files
- lets map clicks on hostile units or targetable buildings issue focused attack orders
- renders command rings for selected units, order destinations, and focused targets
- shows world objective area hints only for the current active objective
- draws tactical target lines from player units to active targets, colored by weapon readiness
- shows a compact mission brief with objective progress and target structure integrity
- shows compact world health bars for damaged or player-targeted enemies and structures
- shows a subtle weapon range ring for selected, detached, attacking, or firing units
- renders short weapon beams, hit flashes, and impact bursts for combat events
- shows animated limb breakoff, section sparks, smoke, and cockpit ejection cues
- applies simple section penalties: destroyed arms reduce firepower, destroyed legs slow movement and disable Jet
- tracks source-derived weapon heat, cooling, and heat lockouts in combat
- carries source weapon type and special-effect metadata into combat visuals
- shows weapon name, range, cooldown, heat lock, and range readiness in the unit panel
- opens a mech bay preview with source weapon loadout, heat, weight, range, damage, and section status
- defines the first BattleCore loadout contract for chassis grids, item shapes, heat, weight, and special equipment slots
- validates loadout grid placement, rotation, blocked cells, overlap, heat, weight, and equipment slot compatibility
- shows projected source loadout validation status, heat/load limits, and occupied grid cells in the mech bay
- lets the mech bay temporarily toggle stock weapons on or off and immediately recompute validation readouts
- visualizes the projected mech bay slot grid with placeholder multi-cell weapon shapes
- lets the mech bay select and nudge projected weapons to preview overlap and bounds validation
- projects spare-load armor plates and heat sinks into free cells with hardness and cooling readouts
- lets projected filler cells cycle between armor, heat sink, and empty to preview loadout tradeoffs
- marks mech bay edits as draft/applied and can reset a draft back to the last applied demo fit
- hands applied demo fits to combat readiness, range, heat, cooldown, and weapon display stats
- applies armor hardness as simple incoming damage reduction and heat sinks as faster cooling
- shows applied armor/cooling bonuses in unit weapon status and blocked damage in combat logs
- builds a starter mech bay inventory summary for owned mechs, weapons, armor plates, heat sinks, and demo token balance
- shows a compact owned-mech roster and read-only roster detail preview in the mech bay, including assembled depot mechs
- marks assembled depot mechs as held with a pending-loadout placeholder before any future deployment flow
- shows a disabled Draft Fit affordance for depot mechs so the future fitting path is visible but inactive
- previews depot fitting requirements such as spare weapon stock and pilot assignment
- shows a read-only pilot placeholder for depot mechs before the future pilot/social loop exists
- previews spare weapon stock counts for future depot fitting
- previews an ordinary weapon shop as the future source of spare depot weapons
- supports a demo ordinary weapon purchase that spends local tokens and adds one spare weapon
- supports demo NPC pilot hiring that spends local tokens and assigns a warehouse pilot
- enables a Draft Fit readiness gate when a warehouse mech has both spare weapon stock and a pilot
- opens a read-only warehouse draft-fit preview showing the selected pilot and spare weapon before real fitting exists
- applies a demo warehouse draft fit that consumes one spare weapon and keeps the mech non-deployable
- previews fitted warehouse deployment status and explains why fitted depot mechs are still held
- shows a squad-selection placeholder before warehouse mechs can alter the mission squad
- opens a squad-selection preview listing current mission slots and fitted depot candidates
- preselects the clicked mission mech as Out or clicked depot candidate as In when opening squad selection
- stages the first current slot and fitted depot candidate in a local squad-selection draft state
- lets the local squad-selection draft cycle outgoing mission slots and incoming depot candidates
- shows one replace plan that names the outgoing and incoming mechs before Confirm
- applies pending squad swap confirmation by exchanging local mission availability flags
- refreshes squad-selection status after confirmation so the joined depot mech is no longer shown as a candidate
- keeps the squad-selection preview visible inside the mech bay and exposes the guarded next-mission Launch handoff there
- previews the next-mission handoff roster from `availableForMission` without restarting the current battle
- shows a player-facing next-mission Ready/Blocked summary with a guarded Launch action and lineup preview
- maps the handoff roster into restart spawn intents behind the Launch guard
- supports command-file depot swap smoke actions for preparing a demo depot candidate, applying the swap, and asserting restart identity
- enables Launch only after a validated BattleMission construction path is available
- keeps MissionContract clone and BattleMission construction validation behind that guarded Launch path
- can restart the active battle by building a replacement BattleMission, immediately disabling old runtime scene roots, and rebuilding Unity views from the guarded path
- keeps the mech bay open and paused when restart Apply is triggered from the mech bay, while system/CLI restarts return directly to battle
- shows a post-restart mission-state roster line with active, deployed, ready, repair, held, fit, and unavailable counts
- carries restart handoff identity into runtime player units with owned mech id, pilot display name, and active loadout id
- warns when draft armor plate or heat sink usage exceeds starter inventory and blocks applying that fit
- shows starter mech condition and one-click demo repair that spends local token balance
- applies a local mission receipt for completed bounty tokens and salvaged mech fragments at mission end
- previews starter mech assembly progress and auto-assembles ready fragment sets into local warehouse mechs
- supports one-shot Jet orders with terrain-aware landing rejection
- provides a toggleable current-objective mission map and a pause/restart/end system panel
- resolves mission victory or defeat through BattleCore result state
- estimates contract bounty, repair cost, net token result, and salvage claims in mission debriefs
- auto-acquires hostile units in weapon range
- resolves temporary weapon cooldown, damage, cockpit/torso/arms/legs sections,
  and destruction
- shows player unit structure and section damage in the left status panel
- prints recent combat events in the top-right combat panel

Local setup:

```powershell
& ..\scripts\content-pack\extract_mission_from_pack.ps1 -MissionId mc2_01
& ..\scripts\content-pack\analyze_mission_extract.ps1
& ..\scripts\content-pack\export_unity_demo_contract.ps1
Copy-Item ..\analysis-output\unity-demo-contract\project-owned-linked-dev\mc2_01\mission-contract.json .\Assets\StreamingAssets\Missions\mc2_01\mission-contract.json -Force
```

The contract JSON is generated from the private local reference content pack and
is ignored by git.

Rebuild the demo scene and Windows player:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "$PWD" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64
```

Run the player smoke test:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -logFile "$PWD\..\analysis-output\unity-player-smoke.log"
```

Run the player with a startup commander command:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2Command "squad move 3136 -789" `
  -logFile "$PWD\..\analysis-output\unity-player-command.log"
```

Run the player with a startup command and a commander state report:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2Command "squad move 3136 -789" `
  -mc2AdvanceSeconds 2 `
  -mc2ReportState `
  -mc2Command "unit unit-1 move 3221 -277" `
  -mc2AdvanceSeconds 1 `
  -mc2ReportState `
  -logFile "$PWD\..\analysis-output\unity-player-report.log"
```

Run the player with a startup commander command file:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile ".\Assets\StreamingAssets\CommanderScripts\mc2_01-demo.txt" `
  -logFile "$PWD\..\analysis-output\unity-player-command-file.log"
```

Run the player with a startup command file that restarts and rebuilds the active mission:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile ".\Assets\StreamingAssets\CommanderScripts\mc2_01-restart-demo.txt" `
  -logFile "$PWD\..\analysis-output\unity-player-restart-command-file.log"
```

Run the player with a startup command file that applies a demo depot squad swap, restarts, and asserts owned-mech identity:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile ".\Assets\StreamingAssets\CommanderScripts\mc2_01-restart-identity-swap.txt" `
  -logFile "$PWD\..\analysis-output\unity-player-restart-identity-swap.log"
```

Run the player with a direct startup restart:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2Command "squad move 3136 -789" `
  -mc2AdvanceSeconds 1 `
  -mc2RestartMission `
  -mc2ReportState `
  -logFile "$PWD\..\analysis-output\unity-player-restart-arg.log"
```

Run the player with MiniMax driving startup commander directives:

```powershell
$env:MINIMAX_API_KEY = "<your-api-key>"
$env:MINIMAX_BASE_URL = "https://api.minimaxi.com/v1"
$env:MINIMAX_MODEL = "MiniMax-M2.5"
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2MinimaxCommanderSteps 1 `
  -logFile "$PWD\..\analysis-output\unity-player-minimax-commander.log"
```

The MiniMax adapter reads only environment variables, logs the selected endpoint/model without printing the key, and asks the model only for a high-level directive. The local `RuleCommander` converts that directive into concrete movement or attack commands so battle execution remains deterministic and responsive. AI scope is intentionally limited to opening drafts, a future capability window, and broad directive refreshes.

Command files run in order and support blank lines plus `#` comments:

```text
command squad move 3136 -789
advance 2
report
restart
assert-restart-identity
command unit unit-1 move 3221 -277
```

Relative command-file paths are checked from the current working directory first, then from the player `StreamingAssets` folder.

Command files also support `prepare-depot-candidate`, `squad-swap`, and `assert-restart-identity depot` for the demo-only restart identity smoke path.

Commander observation reports include `reportIndex` and `missionTimeSeconds` so future AI adapters can correlate decisions with elapsed battle time.

Interactive build output:

```text
Builds/Windows/MC2UnityDemo.exe
```
