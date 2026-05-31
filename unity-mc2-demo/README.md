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
- tracks a lightweight demo mode state across title, battle, mech bay, contracts, Save Slot, system, and debrief
- shows the current mode state and resource balance in the top status strip with player-facing `MODE` and `Funds` labels
- opens a Contracts shell from the system panel with the current `mc2_01` contract, launch, mech bay, system, and return-battle actions
- routes the post-battle debrief through Mech Bay or Contracts, hiding the debrief overlay before repair, save, or relaunch work
- shows a compact After Action or Ready Bay flow lane for Repair All, Save, Contracts, and Launch after battle or during pre-mission prep
- applies a first graphite/cyan/amber UI skin pass to the HUD, buttons, top status strip, and squad command panel
- extends that UI direction to Save Slot, System, Contracts, Debrief, and Mech Bay flow panels with shared framed surfaces
- tightens the title save panel with an inset account summary and side-by-side Continue/New Company actions
- frames the battle Combat/Mission HUD and shifts the mech bay into a right-side focused drawer with a dimmed backdrop
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
- focuses the mech bay fitting view on one selected squad mech at a time with compact squad buttons, draft markers, selected-mech fit-pressure bars, and player-facing chassis/pilot titles
- defines the first BattleCore loadout contract for chassis grids, item shapes, heat, weight, and special equipment slots
- validates loadout grid placement, rotation, blocked cells, overlap, heat, weight, and equipment slot compatibility
- shows projected source loadout validation status, heat/load limits, and occupied grid cells in the mech bay
- draws compact `H/W/G` heat, weight, and grid usage readouts plus bars so fitting pressure is visible before reading the numeric limits
- treats fitted weapons as always active and uses the weapon list only to select mounted weapons for slot editing
- shows a selected-weapon summary line with damage, range, cooldown, heat, weight, compact cell count, and `WxH` footprint above the fitting weapon buttons
- echoes the selected weapon's base, current, and pending target grid positions in that summary line
- labels the selected weapon summary as Base or Moved with matching cyan/amber border cues
- mirrors the same Base or Moved grid-position chain in the selected weapon edit controls
- reports weapon selection, move, place, and reset results with the same W# Base/Moved coordinate format in the top status
- uses compact S/M/L range-band and `WxH` shape labels with matching colors on mounted weapon buttons
- highlights the selected mounted weapon row in the fitting list
- visualizes the projected mech bay slot grid with placeholder multi-cell weapon shapes
- renders projected payload items as larger whole-block grid pieces with original-style empty/short/mid/long/component color bands
- shows selected or hovered payload details for weapon heat, weight, damage, range, cooldown, and compact component bonuses
- highlights hovered payload blocks and empty cells so the grid reads as direct block editing instead of tiny cell picking
- keeps clicked component or empty cells selected with compact slot detail so armor/sink tradeoffs can be inspected without holding the pointer still
- lets the mech bay select and nudge projected weapons to preview overlap and bounds validation
- marks the selected mounted weapon button with a `>` selector so the active weapon is visible without relying only on color
- marks mounted weapon buttons with `*` when that weapon has an unapplied slot move
- lets a clicked target cell place the selected mounted weapon or cycle the selected filler cell through armor, heat sink, and empty
- labels the filler action as Armor, Sink, or Clear so the next single-cell change is visible before clicking
- echoes compact filler actions, such as +Armor, +Sink, or Clear, in the top status, target status, and payload detail text
- draws a green/red target ghost for the selected mounted weapon before Place applies the draft move
- disables the Place action and labels it Block while the selected weapon target overlaps another payload item or leaves the grid
- reports target detail as compact T coordinate, OK/Block state, cell count, and footprint shape
- echoes the selected target coordinate in the compact OK/Block/Same target status row beside Place and Fill
- uses +Armor, +Sink, or Clear filler hints consistently across target-row and clicked-slot feedback
- reports completed filler clicks as T x,y +Armor, +Sink, or Clear in the top status
- shows a compact Pick hint when a mounted weapon is selected but no target grid cell is selected yet
- disables nudge direction buttons when the selected weapon would leave the grid or overlap another payload block
- summarizes selected-weapon movement as compact Move OK or Block direction reasons so disabled movement buttons explain themselves
- colors the Armor, Sink, and Clear filler button by the next filler action
- labels editable filler buttons with the same +Armor, +Sink, or Clear short actions used by the target row
- labels disabled filler targets as Lock or Stk when the selected cell cannot cycle armor/sink/empty
- projects spare-load armor plates and heat sinks into free cells with hardness and cooling readouts
- lets projected filler cells cycle between armor, heat sink, and empty to preview loadout tradeoffs
- disables, labels, and colors the selected-weapon Reset action as Base until that weapon has a placement override to clear
- labels and colors the fit Apply action as Done, Invalid, Stock, or Apply so disabled states are visible in the button itself
- marks mech bay edits as draft/applied and labels the draft reset action as Clean or Reset depending on whether there is anything to revert
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
- shows a read-only local demo account snapshot summary built from the current mech bay inventory
- previews an ordinary weapon shop as the future source of spare depot weapons
- supports a demo ordinary weapon purchase that spends local tokens and adds one spare weapon
- supports demo NPC pilot hiring that spends local tokens and assigns a warehouse pilot
- exposes a compact Reserve action that builds, hires, buys, draft-fits, and opens the next-squad preview for a depot mech
- enables a Draft Fit readiness gate when a warehouse mech has both spare weapon stock and a pilot
- opens a read-only warehouse draft-fit preview showing the selected pilot and spare weapon before real fitting exists
- applies a demo warehouse draft fit that consumes one spare weapon and keeps the mech non-deployable
- marks fitted warehouse mechs as ready for the next-contract squad selection path
- shows a player-facing Next Squad entry for choosing future contract replacements
- opens a Next Contract Squad preview listing current contract slots and fitted depot candidates
- preselects the clicked mission mech as Out or clicked depot candidate as In when opening squad selection
- stages the first current slot and fitted depot candidate in a local squad-selection draft state
- lets the local squad-selection draft cycle outgoing mission slots and incoming depot candidates
- highlights Out/In rows with direction cues before setting the next squad
- shows one replace plan that names the outgoing and incoming mechs before Set
- uses short player-facing Set and Launch status text after squad swaps
- marks Set and Launch rows with Ready/Blocked color cues
- shows a compact completed-swap replacement summary and lineup before next-mission Launch
- labels completed-swap Launch as launching the updated squad
- carries that completed replacement cue into the general next-mission summary and Launch row after the squad preview is hidden
- logs the completed replacement summary after post-launch restart when the mech bay stays open
- supports a command-file `mech-bay-launch` smoke action for the mech-bay Launch handoff
- supports a command-file `hide-squad-preview` smoke action that proves the general next-mission handoff keeps the completed replacement cue before Launch
- supports a command-file `saved-account-report` smoke action that validates and logs a JSON dry-run of the local account snapshot without writing persistent data
- supports a command-file `saved-account-save-load-preview` smoke action that explicitly round-trips the local account JSON without writing a save file
- supports explicit command-file `saved-account-export <path>` and `saved-account-import-preview <path>` actions for manual local account JSON file checks
- supports explicit command-file `saved-account-import-apply-preview <path>` to show what a future import apply would change without mutating the mech bay
- shows the latest import apply preview in the mech bay summary as a read-only confirmation row
- supports explicit command-file `saved-account-import-apply <path>` to apply a matching accepted preview to the local mech bay through a cloned-inventory guard
- supports explicit command-file `saved-account-load-default-preview` and `saved-account-load-default-apply` actions for the persistent demo save file
- supports explicit command-file `saved-account-save-current-default` to write the active account to the persistent demo save file
- supports `-mc2LoadDefaultSave` to explicitly restore the persistent demo save during startup when that file exists
- shows a lightweight Continue/New Company startup panel for manual runs when the persistent demo save exists, including account summary, token/depot/item counts, delta, and save timestamp
- exposes the same Save Slot entry from the pause/system panel, with Save Current, Export Copy, Reset Slot confirmation, inline Last Save feedback, Back support, and an explicit New Company confirmation that resets the active demo run while keeping the persistent save slot
- exposes that same guarded import apply path as a mech bay Apply action that stays disabled until a matching preview is ready
- includes a mech bay JSON path field and Preview action for manually generating the guarded import apply preview
- includes Default and Export helpers that use a persistent demo save path for the current local account snapshot
- includes a guarded Load helper for the persistent demo save path when that file exists
- shows a compact Last Save result line for the latest export, preview, apply, or blocked save/load outcome
- auto-saves the current local account snapshot to the persistent demo save path after guarded account-changing actions
- logs a read-only saved-account delta after local candidate prep so candidate inventory growth is inspectable before real save/load is wired
- applies pending squad swap confirmation by exchanging local mission availability flags
- refreshes squad-selection status after confirmation so the joined depot mech is no longer shown as a candidate
- keeps the squad-selection preview visible inside the mech bay and exposes the guarded next-mission Launch handoff there
- previews the next-mission handoff roster from `availableForMission` without restarting the current battle
- shows a player-facing next-mission Ready/Blocked summary with a guarded Launch action and lineup preview
- maps the handoff roster into restart spawn intents behind the Launch guard
- supports shared UI and command-file local candidate setup that runs receipt assembly, NPC hiring, weapon purchase, draft fit, squad swap, and restart identity assertions
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
- summarizes completed objectives, enemy kills, and damaged player mechs in the debrief without expanding the panel
- auto-acquires hostile units in weapon range
- gives missile, ballistic, and energy attacks distinct muzzle and trace accents
- resolves temporary weapon cooldown, damage, cockpit/torso/arms/legs sections,
  and destruction
- keeps destroyed cockpit, arm, and leg sections readable from the tactical view
  with persistent damage beacons in addition to flashes and detached parts
- shows player unit structure and section damage in the left status panel
- prints recent combat events in the top-right combat panel
- shows a compact combat situation row for squad readiness, active hostiles, live targets, and contact tempo

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

Run the player with a startup command file that applies a demo depot squad swap, uses the mech-bay Launch path, and asserts the updated squad was loaded while the bay stayed open:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile ".\Assets\StreamingAssets\CommanderScripts\mc2_01-mech-bay-launch-swap.txt" `
  -logFile "$PWD\..\analysis-output\unity-player-mech-bay-launch-swap.log"
```

Run the player with a startup command file that applies a demo depot squad swap, hides the squad preview, then launches from the general next-mission handoff:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile ".\Assets\StreamingAssets\CommanderScripts\mc2_01-hidden-handoff-launch-swap.txt" `
  -logFile "$PWD\..\analysis-output\unity-player-hidden-handoff-launch-swap.log"
```

Run the player with a startup command file that reports saved-account snapshots before and after local candidate prep without writing persistent data:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile ".\Assets\StreamingAssets\CommanderScripts\mc2_01-saved-account-report.txt" `
  -logFile "$PWD\..\analysis-output\unity-player-saved-account-report.log"
```

Run the player with a startup command file that previews local saved-account JSON save/load without writing persistent data:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile ".\Assets\StreamingAssets\CommanderScripts\mc2_01-saved-account-save-load-preview.txt" `
  -logFile "$PWD\..\analysis-output\unity-player-saved-account-save-load-preview.log"
```

Run the player with a startup command file that explicitly exports and imports a local saved-account JSON file:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile ".\Assets\StreamingAssets\CommanderScripts\mc2_01-saved-account-file-preview.txt" `
  -logFile "$PWD\..\analysis-output\unity-player-saved-account-file-preview.log"
```

Run the player with a startup command file that previews what applying an imported saved-account JSON file would change:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile ".\Assets\StreamingAssets\CommanderScripts\mc2_01-saved-account-import-apply-preview.txt" `
  -logFile "$PWD\..\analysis-output\unity-player-saved-account-import-apply-preview.log"
```

Run the player with a startup command file that previews and then explicitly applies an imported saved-account JSON file:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile ".\Assets\StreamingAssets\CommanderScripts\mc2_01-saved-account-import-apply.txt" `
  -logFile "$PWD\..\analysis-output\unity-player-saved-account-import-apply.log"
```

Run the player with a startup command file that creates a default saved account, previews loading that persistent default save, applies it through the same guard, and previews again:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile ".\Assets\StreamingAssets\CommanderScripts\mc2_01-saved-account-load-default.txt" `
  -logFile "$PWD\..\analysis-output\unity-player-saved-account-load-default.log"
```

Run the player with a startup flag that explicitly restores the persistent default saved account if it exists:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2LoadDefaultSave `
  -mc2ReportState `
  -logFile "$PWD\..\analysis-output\unity-player-load-default-save-arg.log"
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

Command files also support `prepare-local-candidate`, `prepare-depot-candidate`, `squad-swap`, `hide-squad-preview`, `saved-account-report`, `saved-account-save-load-preview`, `saved-account-export <path>`, `saved-account-import-preview <path>`, `saved-account-import-apply-preview <path>`, `saved-account-import-apply <path>`, `saved-account-load-default-preview`, `saved-account-save-current-default`, `saved-account-load-default-apply`, `mech-bay-launch`, `assert-restart-identity depot`, `assert-debrief-summary`, `assert-combat-situation`, and `assert-loadout-compact`. `prepare-local-candidate` runs the demo receipt, assembly, NPC hire, weapon shop, and warehouse draft-fit services before the swap, then logs a read-only saved-account delta and auto-saves the account snapshot; `hide-squad-preview` verifies the completed replacement cue survives into the general next-mission handoff; `assert-debrief-summary` verifies result counters and compact debrief combat rows; `assert-combat-situation` verifies the combat HUD situation row counters and recent-contact tempo; `assert-loadout-compact` verifies compact mech-bay fitting title, weapon-button, and grid-spacing contracts; `saved-account-report` validates and logs a JSON dry-run of the local account snapshot without writing a file; `saved-account-save-load-preview` serializes and loads that snapshot in memory to prove the future save/load boundary; `saved-account-export` writes a JSON file only when the script provides a path, `saved-account-import-preview` validates a file without applying it to the mech bay, `saved-account-import-apply-preview` adds the identity guard plus would-change delta for a future apply, then exposes the latest preview as a guarded mech bay Apply row with a manual JSON path Preview field, Default/Export/Load helpers, and a compact Last Save result line, `saved-account-import-apply` requires that matching accepted preview before replacing the local mech bay with a cloned loaded inventory, then auto-saves the resulting local account, `saved-account-save-current-default` writes the active account to the persistent demo save path, and the default-load commands reuse the same guard against that path; `prepare-depot-candidate` remains a direct smoke fallback.

Commander observation reports include `reportIndex` and `missionTimeSeconds` so future AI adapters can correlate decisions with elapsed battle time.

Interactive build output:

```text
Builds/Windows/MC2UnityDemo.exe
```
