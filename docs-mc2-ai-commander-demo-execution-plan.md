# MC2 AI Commander Demo Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a Windows-playable Unity 6 tactical mech command demo that faithfully validates the `mc2_01` reference mission, proves the mobile-friendly command model, and leaves clean AI/CLI and content-pack seams for later productization.

**Architecture:** Keep `BattleCore` deterministic and Unity-agnostic where possible, with Unity Presentation translating clicks, visuals, and startup CLI arguments into BattleCore commands. Keep original local MC2 content as a private reference-linked pack only; every public-facing path must preserve the replaceable content-pack boundary.

**Tech Stack:** Unity 6, C#, Windows Standalone player, local PowerShell tooling, `mc2-unity-demo-contract-v1`, private reference-linked content pack, GitHub remote `ai-origin`.

---

## Existing Plan Sources

This is the first unified execution plan for the project. Earlier planning is split across narrower documents:

- `docs-content-index-notes.md`: selected `mc2_01` as the first vertical slice and records extraction/index facts.
- `docs-mc2-01-mission-analysis.md`: documents the reference mission layout, objective graph, enemy brain names, and BattleCore/Unity boundary notes.
- `docs-content-replacement-plan.md`: defines content replacement milestones for copyright-safe public builds.
- `docs-content-pack.md`: defines the replaceable content-pack contract and runtime shell flow.
- `docs-platform-ecosystem-plan.md`: defines the long-term map-server, certified reward, web ranking, creator, and optional blockchain architecture.
- `unity-mc2-demo/README.md`: records current Unity demo behavior and validation commands.

Use this file as the living execution plan. When scope changes, update this file in the same commit as the feature or in a small planning-only commit.

Note: this repository already contains a root-level file named `docs`, so this plan follows the existing flat `docs-*.md` naming convention instead of `docs/plans/...`.

## Current Baseline

Baseline commit when this plan was written:

```text
527e6a7 Run startup commander args in order
```

The Unity demo currently supports:

- loading `mc2_01` units, terrain samples, terrain objects, objectives, static target structure, nav markers, and script hooks
- source-gated objective activation and completion
- enemy activation and lightweight patrol orders
- player squad orders, detached unit orders, focused unit/structure attacks, and Jet orders
- heat, cooldown, weapon range/readiness, simple section damage, section penalties, and animated breakoff/ejection effects
- compact tactical UI with unit status, mech bay preview, mission brief, current-objective map, world objective hints, health bars, command rings, command-accept pulses, range rings, target lines, and weapon-readiness rings
- the combat situation row names the commander mech, squad readiness, detached single-order count, hostiles, targets, and recent contact tempo
- the combat panel keeps a lean two-line HUD: squad/hostile basics plus one compact battle pulse for tempo and current focus
- source-group Contacts pressure remains smoke-guarded for mission pacing, but is not shown as another default combat HUD row
- the battle mission panel uses compact objective names so later `mc2_01` tasks such as North island and Extraction stay readable in the fixed right HUD
- focused mech bay fitting view with compact squad buttons, pending-fit markers, and selected-mech fit-pressure bars so one selected payload grid owns the drawer
- selected fitting card titles now show chassis and pilot only, hiding runtime or owned-mech ids from the visible Mech Lab surface
- selected fitting pressure now uses compact `Fit OK` or `Review` state plus `H/W/G` heat, weight, and grid readouts guarded by loadout smoke
- selected fitting condition now hides zero repair cost and shows an `OK` repair button state until a damaged mech needs repair
- payload component detail now uses compact `A+`, `C+`, and `W` readouts without repeating Armor or Sink names
- payload open-slot detail now uses a shared compact `Slot x,y open` line for hovered and selected empty cells
- initial BattleCore loadout contract for chassis grids, item shapes, heat, weight, armor plates, heat sinks, radar slots, and jump jet slots
- deterministic BattleCore loadout validator for grid bounds, blocked cells, overlap, rotation, heat caps, weight caps, and special-slot compatibility
- source weapon loadout projection in the mech bay with validator status, heat/load limits, and occupied grid cell counts
- fitted weapons are always active; the mech bay weapon list selects mounted weapons for slot editing instead of exposing weapon switches
- selected fitting weapon summary line with compact weapon short code, damage, range, cooldown, heat, weight, compact cell count, and `WxH` footprint above the mounted weapon buttons
- selected weapon summary echoes base, current, and pending target grid positions when the player moves a payload block or chooses a target cell
- selected weapon summary labels Base or Moved with cyan/amber border cues for draft placement state
- selected weapon edit controls mirror the same Base or Moved grid-position chain as the summary
- selected weapon edit controls now use a compact `W# Base/Moved @x,y` header instead of repeating the weapon name
- payload detail text uses compact weapon short codes plus `C` and `WxH` shape readouts, and labels unapplied edits as Pending fit
- mech bay selector and squad replacement status text now use Pending/Outgoing/Reserve wording instead of player-facing draft language
- roster fit detail now shows Ready Bay or Reserve Bay instead of exposing internal owned-mech ids
- reserve prep logs and save-result labels now use Reserve Prep wording instead of local/candidate terminology
- the post-battle fitting drawer now presents itself as Mech Lab instead of a generic prep/squad-fit panel
- contracts, debrief, flow breadcrumb, drawer frame, and top status now route fitting/editing entries through Mech Lab wording while keeping the underlying mech bay state stable
- the top status strip now uses a player-facing `MODE` label instead of developer-style flow wording
- the top status strip now labels the resource balance as `Funds` instead of variable-style token copy
- visible Mech Lab purchase, hire, repair costs, and Repair All completion feedback now use funds wording to match the top resource strip
- the loadout compact smoke now asserts that the opened fitting route, panel title, and top status all stay on Mech Lab wording
- Mech Lab's default summary now uses Bay Ready/Bay Review, Company, and no-recent-save wording instead of Inventory, Account, or idle save/load phrasing
- Mech Lab loading, unavailable, repair, and launch-blocked states now keep Bay wording instead of surfacing Inventory terminology
- Mech Lab and debrief resource readouts now use Parts and Build wording instead of Frags or Assembly labels while keeping the underlying receipt fields unchanged
- System and debrief exit actions now read End Run instead of Exit Demo, and the debrief smoke guards that player-facing label
- Debrief copy now says Payout and points players to repair, inspect Mech Lab, and choose the next contract instead of receipt/launch-again wording
- Debrief reward rows now use short Salvage and Bounty labels instead of claims/total-bounty wording
- Debrief now has a compact Outcome row for objectives, kills, player damage, net funds, and salvage
- mission payout combat-log entries now use Payout and Salvage wording instead of Receipt/token/fragments copy
- System and debrief status copy now uses Save slot, Contracts open, and After Action wording instead of default/post-mission labels
- Mech Lab apply feedback now uses Stock short and Fit applied wording instead of inventory/demo-fit status text
- Mech Lab shop and pilot-hire rows now translate demo-service readiness into player-facing buy/hire wording
- Mech Lab pilot-hire summary now says NPC pilots instead of NPC candidates
- Mech Lab roster detail rows now translate depot/future/loadout service wording into reserve/fit player wording
- Mech Lab roster empty and state rows now use bay/squad wording instead of owned-mech or mission-state wording
- Next Squad open feedback now uses direct open wording instead of preview/candidate terminology
- Save Slot helper feedback now uses Save slot path ready and No load preview wording instead of account/idle import phrasing
- Save Slot load-preview controls now use Load and Load Preview wording instead of import-apply UI labels
- Save Slot path feedback now uses Slot and No slot path wording instead of save/default-path UI labels
- Save Slot result feedback now uses Save Result wording instead of Last Save UI labels
- Save Slot path helper button now reads Slot instead of Default while keeping the same guarded save-slot path
- Save Slot roundtrip status now uses Save check ready/failed wording instead of account save/load preview status text
- Save Slot load-check and load-apply statuses now use Load check, Load blocked, Load failed, and Save loaded wording instead of account/import/apply-preview status text
- Startup Continue now shows Save check failed for unreadable saved games instead of exposing preview/import/account service messages
- Visible save-game UI is now hidden from the player loop; saved-account import/export remains available only as a command-file and developer validation harness
- Mech Lab detail rows also keep saved-account path/check/load tools behind the hidden save-game UI flag, so normal roster/shop detail can open without exposing save tooling
- weapon selection, move, place, and reset results report the same W# Base/Moved coordinate format in the top status
- compact mounted weapon buttons use S/M/L range-band labels and `WxH` shape labels with the same color language as payload blocks
- compact mounted weapon buttons replace the active weapon number with a `>` selector for color-independent selection feedback
- compact mounted weapon buttons show `*` when a weapon has an unapplied slot move
- projected mech bay slot grid visualization with placeholder multi-cell weapon shapes exposed from BattleCore preview data
- temporary projected weapon placement edits with overlap and out-of-bounds validator feedback
- spare-load armor plate and heat sink filler projection with hardness and cooling totals
- weapon blocks prefer compact parenthetical short codes such as `AC10` when source names include them
- component blocks now use compact `A+` and `C+` grid labels for armor and heat-sink fillers
- toggleable projected filler cells that cycle between armor, heat sink, and empty
- the filler button now labels its next action as Armor, Sink, or Clear for clearer original-style slot editing
- top status, selected target status, and payload detail text now echo compact filler actions beside weapon-placement state
- selected payload target cells can now place the selected mounted weapon or cycle a filler cell, keeping grid edits closer to an original-style fitting table
- selected mounted weapons draw green/red target ghosts before placement, making overlap or out-of-bounds pressure visible before applying a draft move
- blocked selected-weapon targets now disable the Place action and label it Block instead of letting a clearly invalid target be placed by accident
- the payload detail line now reports target placement as compact T coordinate, OK/Block state, cell count, and footprint shape, while the target status row colors those states
- the compact target status row echoes the selected target coordinate beside OK/Block/Same state
- target placement detail now uses compact weapon references such as `for W1 AC10` instead of repeating the full weapon name
- clicked-slot status and the compact target status row use consistent +Armor, +Sink, or Clear filler hints
- completed filler clicks report `T x,y +Armor/+Sink/Clear` in the top status
- selected mounted weapons now show a compact Pick hint until a target grid cell is selected
- selected weapon nudge buttons now disable directions that would leave the grid or overlap another payload block
- selected weapon controls now summarize movement as compact Move OK or Block direction reasons
- the filler action button now colors Armor, Sink, and Clear states by the next cell action
- editable filler buttons now use the same +Armor, +Sink, or Clear short actions as the target row
- disabled filler targets now label themselves Lock or Stk instead of showing a misleading next action
- selected-weapon Reset stays disabled, labeled, and colored as Base until that weapon has a placement override to clear
- the fit Apply action labels and colors itself Done, Invalid, Stock, or Apply so disabled apply states do not rely only on the status line
- the fitting Apply/Reset strip uses compact Fit OK, Pending, and Stock status text so the status lane stays readable beside the buttons
- mech bay draft/apply/reset boundaries for temporary demo fits, with the draft reset button labeled and colored as Clean or Reset
- applied demo fit handoff into combat readiness, range, heat, cooldown, and weapon display stats
- applied armor hardness reduces incoming damage and applied heat sinks increase cooling
- applied armor/cooling bonuses appear in unit weapon status and blocked damage appears in combat logs
- starter mech bay inventory contract summarizes owned mechs, weapons, armor plates, heat sinks, and funds balance
- starter mech bay roster preview lists owned squad and assembled reserve mechs with a read-only detail view
- assembled reserve mechs stay held with a pending-loadout placeholder until a future fitting flow equips them
- reserve roster detail shows a disabled Fit Review affordance as a visible future fitting stub
- reserve fit stub previews missing spare weapon stock and pilot assignment requirements
- reserve roster detail shows a read-only pilot placeholder for the future pilot/social system
- reserve roster detail previews spare weapon stock counts before the shop or fitting flow exists
- mech bay summary previews an ordinary weapon shop as the future source of spare reserve weapons
- mech bay summary supports an ordinary weapon purchase that spends local funds and adds one spare weapon
- mech bay summary and roster detail support NPC pilot hiring that spends local funds and assigns a warehouse pilot
- mech bay summary exposes a compact Reserve Prep action that builds, hires, buys, fit-reviews, and opens the next-squad preview for a reserve mech through the same local services used by command files
- reserve Fit Review affordance becomes a readiness gate once spare weapon stock and a pilot are both present
- reserve Fit Review opens a read-only review showing the selected pilot and spare weapon without changing inventory
- reserve Fit Review can apply a demo placeholder fit that consumes one spare weapon and keeps the mech non-deployable
- fitted warehouse mechs show that they are ready for the next-contract squad selection path
- roster detail shows a player-facing Next Squad entry without altering current contract deployment
- Next Contract Squad panel lists current contract slots and ready reserve mechs without changing inventory or deployment
- squad-selection opens with the clicked mission slot preselected as outgoing or clicked ready reserve mech preselected as incoming
- squad-selection confirmation reads from a local squad-plan state that stages outgoing and incoming mech IDs
- squad-selection confirmation applies a guarded local roster swap by exchanging mission availability flags
- squad-selection plan controls can cycle outgoing mission slots and incoming reserve mechs while staying local-only
- squad-selection preview shows a single replace plan with a Confirm row before roster mutation
- squad-selection Out/In rows show highlighted direction cues before Set
- squad-selection Set and next-mission Launch rows use short player-facing status text
- squad-selection Set and next-mission Launch rows show Ready/Blocked color cues
- squad-selection completed-swap state shows a compact replacement summary and done lineup before next-mission Launch
- squad-selection completed-swap Launch row says it launches the updated squad
- general next-mission summary and Launch rows preserve the completed replacement cue after hiding the squad-selection panel
- post-launch mech bay log confirms which completed replacement was loaded
- command-file `mech-bay-launch` smoke hook exercises the mech-bay Launch handoff
- command-file `hide-squad-preview` smoke hook proves the completed replacement cue survives into the general next-mission handoff before Launch
- Next Contract Squad status copy now filters preview/depot/candidate wording into player-facing squad-plan and reserve-mech wording, guarded by the hidden handoff launch smoke
- squad-selection preview refreshes after confirmation so the joined reserve mech becomes a mission slot and no longer appears as an incoming option
- next-mission handoff preview reads `availableForMission` roster slots without mutating the active combat mission
- next-mission handoff area shows a player-facing Ready/Blocked summary, guarded Launch action, and selected lineup
- restart spawn intent, contract clone, and BattleMission construction validation run behind the Launch guard
- guarded runtime restart can replace the active BattleMission, clear generated Unity scene objects, rebind ports, and rebuild the world from the validated path
- starter inventory availability feedback warns on armor plate or heat sink shortages and blocks applying invalid drafts
- starter mech condition and one-click demo repair spend local funds and restore damaged mechs
- starter mission payout applies completed bounty funds and salvaged mech fragments to local inventory
- starter fragment assembly preview shows progress toward the demo mech assembly threshold and auto-assembles ready sets into local warehouse mechs
- starter mech bay inventory can now be wrapped in a read-only local demo saved-account snapshot with cloned inventory, counters, validation, and JSON round-trip coverage
- command-file `saved-account-report` can now validate and log a JSON dry-run of the local account snapshot without writing persistent data
- command-file `saved-account-save-load-preview` can now explicitly exercise JSON serialization and load validation without writing a save file
- command-file `saved-account-export <path>` and `saved-account-import-preview <path>` can now manually write and validate local account JSON files when explicitly requested
- command-file `saved-account-import-apply-preview <path>` can now guard account identity and show the delta that a future import apply would make without applying it
- mech bay summary now shows the latest import apply preview as a read-only confirmation row
- command-file `saved-account-import-apply <path>` can now apply a matching accepted preview to the local mech bay through a guarded cloned-inventory path
- command-file `saved-account-load-default-preview` and `saved-account-load-default-apply` can now preview/apply the persistent demo save file through the same guard
- command-file `saved-account-save-current-default` now writes the active account to the persistent demo save path for Save Current smoke coverage
- mech bay summary now exposes that same guarded import apply path as a disabled-until-ready manual Apply action
- mech bay summary now includes a manual saved-account JSON path field plus Preview action, so the guarded apply flow can start from UI
- mech bay summary now provides Default and Export helpers that point at a persistent demo save file and write the current account snapshot there
- mech bay summary now provides a guarded Load helper for the persistent demo save file when that file exists
- mech bay summary now keeps a compact Save Result line for the latest export, preview, apply, or blocked save/load result
- guarded account-changing actions now auto-save the current local account snapshot to the persistent demo save file
- manual demo startup now bypasses the save-game gate and enters the playable loop directly
- the saved-account panel and Continue/New Company shell are retained only as hidden developer tooling
- the runtime now tracks a lightweight demo flow screen across title, battle, mech bay, contracts, system, debrief, and hidden save-tooling states
- the top status strip now exposes the current mode state, giving the future title shell a visible state boundary before the IMGUI migration
- the system panel now opens a Contracts shell with the current `mc2_01` contract, launch, mech bay, system, and return-battle actions
- the pause/system panel now keeps the player loop focused on Resume, Restart Mission, Contracts, End Run, and Back
- hidden save tooling still has guarded Save Current, Export Copy, Reset Slot, and Back paths for command-file/developer validation
- command-file `prepare-local-candidate` can now produce a ready reserve mech through local payout assembly, NPC hiring, weapon purchase, and warehouse fit-review services
- reserve prep now records a read-only saved-account delta line so funds, mech, ready, reserve, and item-stack changes are visible before any real save file exists
- CLI/AI loop pieces:
  - `-mc2Command`
  - `-mc2AdvanceSeconds`
  - `-mc2ReportState`
  - `-mc2RestartMission`
  - `-mc2LoadDefaultSave`
  - ordered startup command execution

## Guardrails

- Keep each implementation commit small and independently verifiable.
- Use exact file staging. Do not use broad `git add -A`.
- Do not commit generated private reference artifacts under `analysis-output` or ignored content-pack output.
- If Unity rewrites `unity-mc2-demo/Assets/Scenes/Mc2Demo.unity` file IDs only, restore them before commit.
- Keep original MC2 assets local reference-only. Public/demo-safe builds require a replacement pack.
- Validate substantial Unity changes with Unity contract validation, Windows build, and no-graphics smoke.

## Standard Validation Commands

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract `
  -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-plan-step.log"
```

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 `
  -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-plan-step.log"
```

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" `
  -batchmode -nographics -mc2SmokeTest `
  -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-smoke-plan-step.log"
```

## Milestone 1: Scriptable AI/CLI Demo Loop

Goal: make the headless command loop usable as a repeatable demo harness and future AI adapter.

### Task 1: Add Commander Command File Playback

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Add `-mc2CommandFile <path>` startup argument.
2. Parse a simple line format:
   - `command squad move 3136 -789`
   - `advance 2`
   - `report`
   - blank lines and `# comments`
3. Reuse the existing ordered startup executor rather than creating another command path.
4. Add validator coverage with a small test script file.
5. Run Unity validation, build, and smoke with a command file.
6. Commit with message `Add commander command file playback`.

**Expected smoke command:**

```powershell
& .\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile .\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-demo.txt `
  -logFile .\analysis-output\unity-player-smoke-command-file.log
```

### Task 2: Add Demo Commander Script Asset

**Files:**

- Create: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-demo.txt`
- Create: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-demo.txt.meta`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Add a short deterministic script that moves to the airfield objective, reports state, sends one unit toward the hangar, advances, and reports again.
2. Keep it short enough for smoke tests.
3. Use only currently implemented commands.
4. Validate with player smoke.
5. Commit with message `Add mc2_01 commander demo script`.

### Task 3: Add Observation Timing

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Track mission time inside `BattleMission` or the startup harness.
2. Add `missionTimeSeconds` to `CommanderObservation`.
3. Add a report index to observation logs.
4. Validate that time advances after `-mc2AdvanceSeconds`.
5. Commit with message `Add commander observation timing`.

## Milestone 2: Complete mc2_01 Playable Mission Path

Goal: make the first mission reliably playable from start through victory/defeat in the Unity demo.

### Task 4: Validate First Objective to Hangar Flow

**Files:**

- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify as needed: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`

**Steps:**

1. Add validator helper that advances a scripted player force into objective 0.
2. Assert objective 0 completes, objective 1 activates, patrol group activates, and hangar remains targetable.
3. Script a squad structure attack against `structure-1-0`.
4. Advance until hangar destruction or timeout.
5. Assert objective 1 completes and next objective activates.
6. Commit with message `Validate airfield to hangar objective flow`.

### Task 5: Expose Current Objective Targets to Commander

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Add current objective target unit ids, structure ids, or target positions to `CommanderObservation`.
2. Keep hidden objectives out of public/UI hints.
3. Use current active visible objectives only.
4. Validate that objective 2 exposes target positions only after activation.
5. Commit with message `Expose current objective targets to commander`.

### Task 6: Tighten mc2_01 Enemy Encounter Behavior

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Update: `docs-mc2-01-mission-analysis.md`

**Steps:**

1. Compare current activation rules with the mission analysis notes.
2. Add validator cases for patrol group, north island group, Starslayer group, and infantry ambush timing.
3. Keep enemy AI lightweight: patrol or attack nearest player only.
4. Commit with message `Tighten mc2_01 encounter activation`.

## Milestone 3: Battle Feel and Readability

Goal: make the investor/demo battle readable and close enough to the original combat rhythm.

### Task 7: Improve Weapon Effect Identity

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatEvent.cs`

**Steps:**

1. Make missile traces arc or stagger more visibly.
2. Make ballistic hits short and punchy.
3. Make energy beams brighter but brief.
4. Keep all effects primitive/material-based until art assets are ready.
5. Run smoke test.
6. Commit with message `Improve weapon effect readability`.

### Task 8: Improve Damage State Feedback

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`

**Steps:**

1. Make destroyed arms, legs, and cockpit states more visible in the placeholder unit model.
2. Ensure section text remains readable in the status panel.
3. Keep the UI clean with no extra action buttons.
4. Run smoke test and, if safe, one manual visual check.
5. Commit with message `Improve section damage feedback`.

### Task 9: Add Mission Result Summary

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`

**Steps:**

1. Track destroyed enemies, damaged player units, destroyed structures, and completed visible objectives.
2. Show summary in mission result panel.
3. Include summary in commander observation when mission ends.
4. Commit with message `Add mission result summary`.

## Milestone 4: Rule-Based AI Commander MVP

Goal: prove AI command can use the same observation/command loop before connecting an LLM.

### Task 10: Add Rule Commander

**Files:**

- Create: `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- Create: `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs.meta`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Input: `CommanderObservation`.
2. Output: one command string compatible with `CommanderCommandPort`.
3. Rules:
   - if active hostile in range, attack closest hostile
   - else if current structure target exists, move or attack structure
   - else move toward current objective marker
   - later: if one unit is badly damaged, detach move it away
4. Validate first command on initial `mc2_01` state.
5. Commit with message `Add rule commander`.

### Task 11: Add AI Autoplay Startup Option

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/README.md`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Add `-mc2AutoCommanderSteps <n>`.
2. For each step: observe, choose rule command, issue command, advance fixed seconds, log report.
3. Keep it deterministic and capped.
4. Validate at least 3 steps without exceptions.
5. Commit with message `Add rule commander autoplay`.

## Milestone 5: Mech Lab and Loadout Core

Goal: prepare the long-term loadout fun loop without blocking the battle demo.

### Task 12: Define Loadout Data Contract

**Files:**

- Create: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- Create: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs.meta`
- Modify: `docs-mc2-01-mission-analysis.md`

**Steps:**

1. Define mech chassis, slot grid, heat limit, weight limit, and special equipment slots.
2. Define weapon shape, heat, weight, damage, range, cooldown.
3. Keep names/data source-derived for private validation only.
4. Add editor validation for a tiny synthetic loadout.
5. Commit with message `Define loadout data contract`.

### Task 13: Add Loadout Validator

**Files:**

- Create: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Create: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs.meta`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Validate slot shape placement.
2. Validate heat and weight caps.
3. Validate armor/sink single-cell equipment.
4. Keep calculation simple and deterministic.
5. Commit with message `Add loadout validator`.

## Milestone 6: Economy, Pilots, and Support Later

Goal: defer non-battle systems until the command demo is convincing.

Backlog items:

- one-token economy
- repair cost and one-click repair
- fragments and automatic mech assembly
- normal shop weapons vs rare/event-only advanced weapons
- NPC pilots with death risk
- friend/social pilots with daily wage and non-permanent death
- support hiring and loot split rules
- paper battle calculation for non-watched AI missions

Do not start this milestone until Milestones 1-4 are demonstrable.

## Milestone 7: Content Replacement and Investor-Safe Build

Goal: keep private reference validation useful while preparing a public-safe vertical slice.

Tasks:

1. Continue treating original MC2 assets as private local reference-only.
2. Use `docs-content-replacement-plan.md` milestones for product identity, text, art, effects, audio, and provenance.
3. Do not ship original names, text, models, textures, audio, campaign story, or trademarks in public builds.
4. Keep all replacement work expressed as content packs.
5. Build the first investor-safe capture only after the art-safe vertical slice milestone begins.

## Current Recommended Next Task

Start with **a full visible-flow audit, then move out of text-only polish**.

Reason:

- The old starter mission restart polish, roster swap, hidden developer save harness, post-battle lane, and first design-image-inspired UI pass are now implemented.
- The recent Mech Lab copy and density pass has smoke coverage for the highest-noise fitting rows, including compact weapon codes, component details, repair state, route labels, and funds wording.
- The current risk is no longer raw feature absence or obvious debug labels; it is whether the whole private Windows loop feels coherent when played end to end.
- The next demo milestone should make one private Windows build feel like a coherent local game: battle, debrief, repair, contracts, mech lab, squad swap, and relaunch.
- Further tiny text-only Mech Lab commits should stop unless a visible-flow audit finds a concrete confusing label or dead-end action.
- AI remains intentionally bounded to high-level directives and capability preview; do not expand model-driven combat until the local loop feels good.
- Content replacement remains a package boundary, not a blocker for private reference validation.

Current stage snapshot:

- Phase A Local Playable Loop is late active work: the private Windows loop is mostly present, but still needs one manual visible-flow audit and a small pass on any confusing dead ends.
- Phase B Battle Readability has a working foundation: weapon families, section damage, cockpit/ejection cues, combat HUD, and debrief rows exist, but the first mission still needs stronger encounter rhythm and clearer tactical spectacle.
- Phase C Mech Lab Experience has core fitting rules and block editing in place: the next meaningful improvement is a calmer dedicated Mech Lab surface, not more right-drawer microcopy.
- Phase D Public-Safe Content Pack remains later: public capture still requires replacement names, text, UI identity, models, textures, audio, and mission-facing story copy.
- Phase E AI and Platform remain bounded architecture: keep AI to high-level directives until the local single-player Windows demo is convincing.

Visible-flow audit notes, 2026-05-31:

- Smoke paths for combat situation, debrief summary, loadout compact, hidden handoff launch, and the combined visible-flow audit all pass on the current Windows build.
- A real window capture of the Battle view shows the fixed camera and left/right HUD are readable enough for the next audit pass, though the right mission panel still truncates later objective names.
- A real window capture of the Mech Lab view confirms the fitting drawer is functional but still cramped; the larger next step should be a dedicated Mech Lab surface, not more microcopy.
- The audit found visible leftover `Token` wording in the Mech Lab inventory summary and debrief funds row; the current pass fixes those to the `Funds` and `Payout` wording family and guards them in smoke.
- The current Mech Lab pass starts that larger step by giving desktop Windows a centered, wider two-column surface: Company Bay on the left, Loadout fitting on the right, with compact layout kept as a fallback.
- Follow-up visual capture showed the right Loadout column is readable, while Company Bay needed more width; the current pass widens Company Bay to about 429px at 1280x720 while preserving a 520px-plus Loadout area.
- The Company Bay left column now defaults to an Ops page for repair, contracts, shop, hire, and launch actions, with dense roster and reserve-pilot details moved behind a Roster page.
- The battle mission panel now replaces truncated long objective titles with compact objective names, guarded by the combat-situation smoke.

## Frozen Development Plan

This is the current locked execution plan for the private Windows demo. When the user says "continue by the plan", follow this order unless a bug blocks the build or the user explicitly changes priority.

### Phase A: Local Playable Loop

Status: active, late flow polish.

Goal: make one private Windows build understandable without developer narration.

Definition of done:

1. Manual startup enters the playable demo directly without a save-game gate.
2. The player can enter battle, complete or force-resolve the current `mc2_01` flow, reach debrief, repair, open contracts, inspect Mech Lab, swap one reserve mech, and relaunch the next contract.
3. Contracts, Mech Lab, Debrief, System, and top-flow labels use player-facing wording.
4. Remaining debug-only rows are hidden from normal play while command-file and log coverage remain available.
5. At least one command-file smoke path proves the visible flow after each meaningful polish step.

Next work:

1. Run a manual visible-flow audit from startup through battle, debrief, repair, contracts, Mech Lab, squad swap, and relaunch.
2. Record concrete blockers in this plan before fixing them; prefer flow dead ends, misleading actions, and layout pressure over small wording tweaks.
3. Fix only the highest-impact audit findings first, with one smoke or validator command per meaningful polish step.
4. After the audit, choose the next larger lane: dedicated Mech Lab surface if fitting still feels cramped, or battle readability if the mission does not feel exciting enough.

### Phase B: Battle Readability

Status: next, with foundation already implemented.

Goal: make `mc2_01` feel like a readable tactical mech battle, not just a functional simulation.

Definition of done:

1. Missile, ballistic, and energy weapons are visually distinct in muzzle, trace, hit, and log/status feedback.
2. Destroyed arms, legs, cockpit hits, and pilot ejection are visible enough to understand at tactical zoom.
3. Enemy activation and encounter pacing in `mc2_01` follow the reference mission rhythm without adding strong AI.
4. The combat HUD clearly communicates commander mech, squad readiness, solo orders, current hostiles, target pressure, and recent contact.
5. Debrief summarizes objectives, kills, destroyed targets, player damage, payout, and salvage in compact player-facing rows.

Next work:

1. Improve primitive/generated combat effects before importing or replacing major art, focusing on hit readability at the fixed tactical camera.
2. Strengthen persistent arm, leg, cockpit, and ejection readability so section damage is understood without zooming into tiny placeholders.
3. Tune one encounter at a time against `docs-mc2-01-mission-analysis.md`, preserving source-triggered enemies instead of adding strong AI.
4. Keep debrief counters and combat HUD smoke paths updated when battle readability changes visible flow.

### Phase C: Mech Lab Experience

Status: parallel support track after the Phase A audit; core fitting rules exist, dedicated surface still needed.

Goal: turn the current fitting drawer into an original-style mech customization surface.

Definition of done:

1. Mech Lab has a dedicated, calm layout instead of carrying too much battle-side drawer pressure.
2. Weapons, armor plates, and heat sinks read as whole grid blocks.
3. Weapon list selects mounted blocks only; fitted weapons are always active.
4. Heat, weight, grid usage, range, cooldown, damage, armor hardness, and cooling pressure are visible before applying.
5. Invalid fits are blocked by BattleCore validation, not by presentation-only checks.

Next work:

1. Preserve the current validator, command-file smoke coverage, and content-pack data boundary.
2. Move from the current right-side IMGUI drawer toward a dedicated Mech Lab surface with the same block-placement rules.
3. Keep weapons, armor plates, and heat sinks as large whole blocks with compact short-code details.
4. Keep armor math simple: armor plates raise overall hardness; section damage and cockpit risk stay visible.

### Phase D: Public-Safe Content Pack

Status: after the private demo loop and battle readability are convincing.

Goal: create a capture-safe vertical slice without original copyrighted assets, names, story text, trademarks, audio, or model/texture content.

Definition of done:

1. Original MC2 files remain private reference-linked development content only.
2. Product identity, UI strings, mech names, weapon names, pilot names, visible mission text, icons, effects, audio, and visuals have replacement provenance.
3. The public build mounts a project-owned or properly licensed content pack and still runs the same mission contract path.
4. Replacement work follows `docs-content-replacement-plan.md` and `docs-content-pack.md`.

Next work:

1. Do not block private validation on replacement art.
2. Once the demo is presentable, build a text-safe slice first, then an art-safe slice.

### Phase E: AI Commander and Platform

Status: bounded and later.

Goal: keep AI and platform architecture ready without letting them consume the first playable-demo milestone.

Definition of done for the near term:

1. AI commander remains limited to opening plan, directive token, capability window, and optional high-level refresh.
2. Model calls never choose per-frame actions, exact coordinates, exact targets, or weapon timing.
3. Local BattleCore remains responsible for movement legality, target selection, heat, cooldown, damage, objectives, and paper-result compatibility.
4. Map server, certified rewards, web ranking, creator economy, support/friend pilots, wages, multiplayer, and blockchain stay in planning until the single-player Windows demo is strong.

Next work:

1. Treat `docs-ai-commander-directive-contract.md` as the stop line for AI work.
2. Treat `docs-platform-ecosystem-plan.md` as the long-term reference only.

### Operating Rules

1. Do not start broad new systems while Phase A has unresolved visible-flow blockers.
2. Prefer one small player-visible improvement plus one smoke/validator update per commit.
3. Keep BattleCore deterministic and Unity presentation-driven code thin.
4. Use original assets and values only for private reference validation.
5. Never commit private generated reference dumps or ignored content-pack output.
6. When uncertain, improve the current playable Windows demo before expanding product scope.
7. Stop taking text-only Mech Lab density passes once existing smoke covers the label family; use the next turn for visible-flow audit, dedicated layout, or battle readability.

### Immediate Plan: Local Playable Loop

Goal: make the current Windows demo understandable without developer narration.

Tasks:

1. Launch the current Windows build manually and walk the visible path from startup through battle, debrief, repair, contracts, Mech Lab, squad swap, and relaunch.
2. Record any confusing labels, cramped controls, dead-end actions, or missing feedback directly in this plan.
3. Fix only the top visible blocker from that audit before returning to another planning checkpoint.
4. Keep every action guarded by the existing validated BattleCore paths.
5. Add or update one command-file smoke path that proves the visible loop still works after each polish step.

### Next Plan: Battle Readability

Goal: make the first mission feel closer to the original tactical rhythm before adding more systems.

Tasks:

1. Improve weapon-effect identity for missile, ballistic, and energy fire using primitive or generated Unity visuals.
2. Make destroyed arms, destroyed legs, cockpit destruction, and ejection more visible on placeholder mechs.
3. Tighten `mc2_01` enemy activation and encounter pacing against the mission-analysis notes.
4. Add or tighten compact mission result summaries for destroyed enemies, player damage, completed objectives, funds reward, and salvage.
5. Keep enemy AI lightweight and source-triggered; no strong AI director in this stage.

### Next Plan: Mech Lab Experience

Goal: turn the loadout prototype into a satisfying original-style fitting surface.

Tasks:

1. Move the fitting surface toward a dedicated Mech Lab panel instead of keeping all editing pressure inside the battle-side drawer.
2. Preserve the current rule: fitted weapons are always enabled; the weapon list selects mounted blocks for movement only.
3. Keep weapons, armor plates, and heat sinks as whole grid blocks with clear `WxH`, heat, weight, range, cooldown, and damage readouts.
4. Keep armor calculation simple: armor plates raise overall hardness while section damage and cockpit risk remain visible.
5. Use source-derived private reference values while preserving the replaceable content-pack boundary.

### Later Plan: Public-Safe Content and AI

Goal: keep the project investable without drifting into legal or technical traps.

Tasks:

1. Continue using original MC2 assets only as private reference-linked validation content.
2. Build a project-owned content pack for public captures when the local loop and battle readability are strong enough.
3. Keep AI commander work to opening plan, directive token, capability window, and optional high-level refresh.
4. Use `docs-platform-ecosystem-plan.md` for later map-server, certified reward, creator economy, web ranking, and optional blockchain decisions.
5. Defer multiplayer, support hiring, friend pilots, activity wages, deeper economy, and paper battles until the single-player loop is convincing.
6. Keep each new system reachable from command files or CLI smoke paths before making it visually richer.

### Completed Context

- Battle readability pass has begun with family-specific hit cues: bright brief energy beam plus flash/pillar/ring, staggered missile salvo-spread arcs plus blast/smoke, and ballistic tracer plus punch/sparks.
- The combat situation smoke now guards the current weapon FX cue contract with `Energy=beam+pillar+muzzle+flash Missile=arc+blast+salvo-spread Ballistic=tracer+sparks+muzzle+punch`.
- Tactical target lines now vary color and thickness for ready, cooling, and blocked shots, keeping weapon cadence visible without adding HUD rows.
- Player weapon readiness now shows compact ready, cooling, and blocked rings on active targeting mechs without adding combat HUD rows.
- Target acquisition now pulses compact auto/command lock rings at target points without adding combat HUD rows.
- Squad focus now keeps a compact pressure ring and beacon on targets shared by multiple player mechs without adding combat HUD rows.
- Hostile focus now keeps a compact warning ring and beacon on player mechs targeted by multiple enemies without adding combat HUD rows.
- Player mech damage now shows compact warning and critical rings for structure loss or section penalties without adding combat HUD rows.
- Combat tempo now shows a commander-centered pressure ring and beacon for tracking, contact, and fire states without adding combat HUD rows.
- Active hostile clusters now show a compact pressure-center ring and beacon, making enemy pressure location readable without adding combat HUD rows.
- The combat situation smoke now also guards the compact battle pulse with explicit quiet, contact, and fire expectations tied to mission events.
- The combat situation smoke now also guards source-group contact pressure internally, so active Airfield and North groups remain verifiable without occupying default battle HUD space.
- The combat situation smoke now also guards the commanded Focus contract, keeping the current target readable without exposing internal target ids.
- Accepted player and CLI commands now pulse move, attack, Jet, and single-unit cues on the battlefield so command acknowledgement reads without adding another HUD row.
- Move and Jet orders now draw compact travel path lines from player mechs to their destinations without adding combat HUD rows.
- Move and Jet orders now pulse compact arrival cues when player mechs reach their destinations without adding combat HUD rows.
- Section damage readability now has persistent missing-arm flags, red leg-collapse cues, cockpit breach/ejection/chute markers, critical-section smoke/spark vents, destroyed-mech wreck blast/smoke/marker/debris cues, compact critical/destroyed section status labels, and smoke-guarded cue contracts.
- Hot and heat-locked mechs now show battlefield vent/lock cues, so heat pressure reads at the fixed tactical camera without adding another combat HUD row.
- The first squad mech now has a subtle commander anchor/beacon in the battlefield, matching the fixed camera follow rule without adding combat HUD rows.
- Single-unit orders now emit a short return-to-squad pulse when the mech automatically rejoins squad control after completing its solo order.
- The first `mc2_01` encounter pacing smoke now proves the airfield beat: initial hostiles held, Airfield/North patrols armed after objective 0, infantry ambush held until hangar damage, and Starslayer held for area 7.
- The objective graph smoke now guards the source mission skeleton: 6 visible objectives, 3 hidden glue objectives, flag edges, hidden first-patrol flag `3`, north-island unlock, Starslayer area trigger, and Starslayer VO target count.
- The combat-situation smoke now also guards compact mission-panel objective lines so the North island and Extraction objectives remain readable in the fixed right HUD.
- Enemy activation events now spawn a short tactical wake ring, beacon, and ping at the activated group center, making source-paced contact beats visible without adding another combat HUD row.
- Current active objectives now keep a compact battlefield guide ring and beacon at the target point, improving fixed-camera navigation without adding HUD rows.
- Current active objectives now also draw a thin commander-to-target route line, making objective direction readable without changing command flow.
- Current active structure objectives now tint guide and route cues from steady to damaged and critical as the target loses structure.
- Visible objective activation and completion events now spawn short target-position pulses, keeping mission progress readable in the battlefield without expanding the combat HUD.
- Mission complete and failed script events now spawn a stronger result cue before the debrief panel takes over, keeping the battle-to-debrief transition visible without adding HUD rows.
- Jet orders now show takeoff flame, in-flight smoke trail, and landing dust cues so short jumps read clearly at the fixed tactical camera.
- Target structures now carry persistent scorch, smoke/flame, collapse dust, and collapsed-rubble/debris cues so the Hangar objective reads in the battlefield without extra HUD rows.
- ABL-style script bridge signals now spawn short source-position pulses, making Hangar, patrol, Starslayer, and result beats visible without adding HUD rows.
- The hangar ambush smoke now attacks `structure-1-0`, captures combat/script/contact events during startup advance, and verifies infantry activation only after Hangar damage.
- The Starslayer trigger smoke now moves into the hidden area-7 zone, treats destroyed enemies as already-activated for pacing counts, and verifies the west lance wakes only after the area trigger; the same pacing assertion now requires the hidden Starslayer VO hook once the lance is cleared.
- Mission rewards now add funds and salvage fragments, and ready fragment sets become local warehouse mechs.
- The mech bay now has a compact roster line and a read-only detail view for owned mechs.
- Assembled reserve mechs now stay held with `pending-loadout`, so they cannot be confused with deployable squad mechs.
- The roster detail now shows a disabled Fit Review affordance, making the future fitting workflow visible without enabling it yet.
- The reserve fit stub now previews missing spare weapon stock and pilot assignment requirements.
- The roster detail now shows a read-only pilot placeholder, so the future pilot/social system has a clear UI slot.
- The roster detail now previews spare weapon stock counts, confirming that starter source weapons are all mounted and no reserve fitting weapon stock is free yet.
- The mech bay summary now shows an ordinary weapon shop preview, so the future source of spare weapon stock is visible.
- The mech bay summary now supports a tightly scoped ordinary weapon purchase that deducts funds and adds one spare weapon stack.
- The mech bay now supports NPC pilot hiring that deducts funds and assigns a pilot to a warehouse mech.
- The reserve Fit Review affordance now becomes clickable once spare stock and a pilot are both present, giving feedback without changing loadouts.
- The reserve Fit Review affordance now opens a read-only review showing the selected pilot and spare weapon without changing inventory or loadouts.
- The reserve Fit Review now has a demo-only Apply action that consumes one spare weapon and changes the warehouse mech from `pending-loadout` to `warehouse-demo-fit`, while keeping it non-deployable.
- The roster now explains fitted reserve mechs as ready for next-mission selection instead of a future-only flow.
- The roster now shows a player-facing Next Squad entry, so the deployment path is visible before it alters mission rosters.
- The squad-selection preview now lists current mission slots and ready reserve mechs without changing deployment.
- The roster Squad button now preselects clicked mission mechs as outgoing and clicked ready reserve mechs as incoming.
- The squad-selection confirmation path now reads from a local squad-plan state container that holds selected outgoing and incoming mech IDs.
- The squad-selection plan controls now cycle mission slots and reserve mechs while keeping the selected IDs local-only.
- The squad-selection preview now collapses the old disabled Swap and Dry Run rows into one clear replace plan plus Confirm.
- The squad-selection confirmation path now applies a guarded local roster swap by exchanging mission availability flags while preserving funds and inventory counts.
- The squad-selection preview now refreshes after confirmation, so the roster detail and preview stay readable after the only current reserve mech joins the contract squad.
- The mech bay now has a next-mission handoff preview, so mission launch/restart code can consume the selected `availableForMission` roster without mutating funds or item inventory state.
- The next-mission handoff now shows a player-facing Ready/Blocked summary, guarded Launch action, and selected lineup.
- The restart validation path maps handoff slots to spawn intents, clones the mission contract, and validates replacement `BattleMission` construction before Launch can apply.
- The restart path now has a guarded runtime swap that builds a replacement `BattleMission`, clears generated Unity scene objects, rebinds command/observation ports, and rebuilds the world without changing funds or item counts.
- Startup command files now support a script-level `restart` action, and `-mc2RestartMission` exposes the same guarded runtime swap from CLI startup args.
- A dedicated `mc2_01-restart-demo.txt` command file now proves move, advance, report, repeated restart, and post-restart command playback in the built player.
- Runtime restart now immediately disables old generated scene roots before queued destruction, reducing same-frame overlap risk while the replacement world is rebuilt.
- Mech bay restart Apply now keeps the mech bay open and pauses the rebuilt mission, while system, result-panel, and CLI restarts still return directly to battle.
- Post-restart roster/condition feedback now shows active player slots, deployed roster count, ready mechs, repair needs, held reserve mechs, fit blockers, and unavailable mechs in the mech bay and restart combat log.
- Restart handoff identity now flows through `UnitSpawn` into runtime `UnitState`, commander observations, loadout card titles, restart identity logs, and inventory condition sync.
- A dedicated `mc2_01-restart-identity-swap.txt` command file now prepares a ready reserve mech, applies a squad swap, restarts, and asserts that runtime owned-mech identity includes the reserve slot.
- The squad-selection preview now appears inline in the mech bay when opened, hides lower roster detail to avoid being buried, and exposes the same guarded next-mission Launch handoff after the staged swap row.
- The mech bay next-mission area now collapses the old Launch, dry-run, contract, clone, and construction rows into a player-facing Ready/Blocked summary, guarded Launch button, and lineup preview while preserving the validated restart guards.
- The squad-selection Out/In rows now carry highlighted direction cues, so the selected replacement direction is visible before Set.
- Squad-selection Set and next-mission Launch rows now use short player-facing status text after swaps while keeping technical summaries in logs and guards.
- Squad-selection Set and next-mission Launch rows now show Ready/Blocked color cues on the action button and status line.
- The completed-swap squad-selection state now hides the Out/In picker noise and shows a compact replacement summary plus done lineup before next-mission Launch.
- The completed-swap Launch row now says it launches the updated squad, making the button consequence explicit before applying the handoff.
- The general next-mission summary and Launch row now preserve that completed replacement cue after hiding the squad-selection panel.
- Post-launch mech bay log now repeats the completed replacement summary after a completed swap Launch.
- A lightweight command-file `mech-bay-launch` smoke hook now exercises the mech-bay Launch path, checks the bay stays open and paused, and confirms the updated-squad status before identity assertion.
- A lightweight command-file `hide-squad-preview` smoke hook now hides the completed squad preview, checks the general handoff summary still names the updated squad, and launches from that path.
- A combined command-file `mc2_01-visible-flow-audit.txt` smoke path now covers battle HUD, debrief, Mech Lab, squad swap, hidden handoff, launch, restart identity, and loadout compact assertions for Phase A.
- The smoke-only reserve replacement remains as a command-file fallback, but the primary command smoke now produces a ready reserve mech through the local payout, assembly, NPC hire, weapon shop, and warehouse fit-review services.
- The mech bay summary now exposes that same ready-reserve service chain as a compact Reserve Prep action, and it opens Next Squad with the prepared reserve mech preselected.
- A tiny saved-account boundary now wraps demo inventory state as a cloned local account snapshot, validates cached counters, and round-trips through JSON without changing battle rules or requiring backend services.
- A lightweight command-file `saved-account-report` smoke hook now validates and logs a local saved-account JSON dry-run before and after reserve prep, without writing persistent user data by default.
- Reserve prep now records a read-only saved-account delta line after the receipt/assembly/pilot/shop/fit chain, so inventory growth is visible before real save/load work.
- A manual command-file `saved-account-save-load-preview` hook now exercises JSON serialization, load validation, and zero-delta round-trip checks while keeping default startup and UI flows read-only.
- A manual command-file save-file path now supports `saved-account-export <path>` and `saved-account-import-preview <path>`, so test runs can write and validate local account JSON only when a script asks for it.
- A guarded `saved-account-import-apply-preview <path>` command now checks account identity and reports what funds/mech/reserve deltas a future apply would make, without mutating the live mech bay.
- The mech bay summary now exposes the latest guarded import apply preview as a read-only confirmation row, keeping the future apply path visible without mutating live data.
- A guarded `saved-account-import-apply <path>` command now requires the latest accepted preview to match path, JSON length, and delta before replacing the local mech bay with a cloned loaded inventory.
- The mech bay summary now exposes the same guarded import apply path as a manual Apply action that stays disabled unless a matching preview is ready.
- The mech bay summary now includes a manual saved-account JSON path field and Preview action, so users can generate a guarded import apply preview without relying on startup command files.
- The mech bay summary now provides Default and Export helpers that point at a persistent demo save file and write the current local account snapshot before preview/apply.
- The mech bay summary now provides a guarded Load helper that previews and applies the persistent demo save path when the file exists.
- The mech bay summary now keeps a compact Save Result line for export, preview, apply, and blocked save/load outcomes.
- Guarded account-changing actions now auto-save the current local account snapshot to the persistent demo save file, including mission receipts, import apply, reserve prep, shop purchases, pilot hires, warehouse fits, squad selection, and repairs.
- Startup command files can now run `saved-account-load-default-preview` and `saved-account-load-default-apply`, restoring the persistent demo save path through the existing import-apply guard.
- Startup command files can now run `saved-account-save-current-default`, explicitly covering the Save Current default export path before preview/apply smoke checks.
- The explicit `-mc2LoadDefaultSave` startup flag now restores the persistent demo save path through the existing import-apply guard when the file exists, and skips cleanly when it does not.
- Manual demo startup keeps the lightweight Continue/New Company save-slot panel behind developer tooling, with account summary, funds/reserve/item counts, delta, and save timestamp available for save checks.
- The pause/system panel now exposes the same Save Slot entry, so testers can reopen Continue/New Company after launch; New Company requires an explicit confirmation and resets the active demo run while keeping the persistent save slot untouched.
- System-opened Save Slot can now return with Back, keeping the panel useful as an in-run save/title shell rather than a one-way modal.
- System-opened Save Slot now has a Save Current action that writes the active account to the persistent save slot and refreshes the displayed Continue summary.
- System-opened Save Slot now shows the latest save/load result inline, so Save Current feedback stays visible inside the same panel.
- System-opened Save Slot now has Export Copy and Reset Slot controls; Reset Slot requires confirmation, copies the old save first, then replaces the default slot with a fresh demo snapshot.
- A first lightweight demo mode state now tracks title, battle, mech bay, contracts, Save Slot, system, and debrief, with the current state visible in the top status strip.
- The system panel now opens a player-facing Contracts shell for `mc2_01`, with launch, mech bay, system, and return-battle actions.
- The debrief panel now routes post-battle flow through Mech Bay or Contracts, hiding the debrief overlay before the player repairs, saves, or chooses the next launch.
- The Contracts return action now goes back to Debrief after a completed mission, keeping the post-battle flow reversible.
- The mech bay summary now has a compact post-battle/prep lane with Repair All, Contracts, and Launch, so the main playable loop stays visible without a save-game step.
- The mech bay next-contract row now uses Contract wording to match the system and debrief flow.
- The squad replacement preview and top flow label now use Next Contract and Contracts wording while preserving internal mission-contract guards.
- The mech bay flow lane now uses player-facing After Action and Ready Bay status text, and the candidate shortcut now reads as a Reserve action rather than a debug-style prep hook.
- The Next Contract Squad panel now uses Reserve and review/launch wording instead of depot-candidate or preview-only phrasing, keeping the replacement flow player-facing while preserving the guarded launch path.
- The roster and reserve fit review panel now use Reserve/Fit Review wording consistently while preserving the same warehouse fitting services.
- Plan notes and smoke-facing reserve candidate logs now use Reserve/Fit Review wording consistently, leaving only internal service names and legacy command identifiers unchanged.
- The first design-image-inspired UI pass now applies a graphite/cyan/amber skin, top status strip, and framed squad command panel while keeping the existing IMGUI behavior stable.
- The same design pass now extends across Save Slot, System, Contracts, Debrief, and Mech Bay panels with shared framed surfaces and highlighted flow lanes.
- The first in-window visual inspection pass tightened the title save panel with an inset account summary, shorter top status strip, and side-by-side Continue/New Company actions.
- The battle/mech-bay inspection pass now frames the Combat/Mission HUD and shifts the mech bay into a right-side focused drawer with a dimmed backdrop.
- The combat situation row now names the first commander mech plus Solo count, so detached single-order state is visible without scanning every squad row.
- The mech bay payload grid now presents weapons, armor, and heat sinks as larger whole-block grid pieces using original-style empty/short/mid/long/component color cues.
- The loadout status line now includes compact heat/load/grid usage bars, so fit pressure is visible before reading the numeric caps.
- The payload grid now adds selected/hovered item detail text so each block exposes heat, weight, damage, range, cooldown, or component bonuses without opening another panel.
- The payload grid now adds a hover frame for whole blocks and open cells, making the original-style block editing target clearer before a dedicated loadout UI replaces IMGUI.
- Clicked component or open payload cells now remain selected, so armor/sink tradeoffs keep their visible frame and detail line after hover leaves.
- Slot Reset and draft Reset now clear stale selected grid coordinates, keeping the detail line aligned with the restored fit.
- Per-weapon switch controls were removed from the fitting UI: fitted weapons are treated as active, and the list only selects mounted weapons for slot movement.
- The fitting weapon list still highlights the selected mounted weapon, reducing reliance on grid-only feedback.
- The payload grid now treats clicked non-weapon cells as explicit targets, with Place and Fill actions for weapon placement and single-cell armor/sink cycling.
- The filler action is now labeled by its next result, so an empty cell offers Armor, an armor plate offers Sink, and a heat sink offers Clear.
- The top status, target status, and payload detail line now echo compact filler actions, for example `+Armor`, beside the weapon-placement state.
- The payload grid now previews the selected mounted weapon as a green/red target ghost before Place applies the draft move.
- The Place action now stays disabled and reads Block for overlap or out-of-bounds targets, keeping the ghost preview informative without allowing accidental invalid placement.
- The blocked Place button now uses the same short Block wording as the compact target and movement status rows.
- The payload detail line now mirrors target placement state in compact text, including T coordinate, OK/Block status, blocked reason, selected weapon cell count, and footprint shape.
- The target detail text now uses the same OK/Block wording as the compact target row instead of the older clear/blocked words.
- The target detail text now uses `T`, `C`, and `WxH` readouts so the lower detail line matches the compact target row.
- The target status row now colors OK, Same-slot, and Block states consistently with the Place button and target ghost.
- The target status row now uses compact `T x,y OK/Same/Block` text, so Place/Fill feedback repeats the exact selected grid cell without long state words.
- The target row now uses shorter OK/Same/Block wording, matching the compact Move OK/Block nudge status style.
- The clicked-slot status and target status row now both use `+Armor`, `+Sink`, or `Clear`, keeping filler feedback compact and consistent.
- Completed filler clicks now report `T x,y +Armor/+Sink/Clear` in the top status instead of a separate filler wording.
- The selected weapon controls now show a compact Pick hint when no target grid cell is selected yet.
- The no-target hint now uses the shorter Pick label, matching the compact OK/Same/Block target row.
- The selected weapon nudge buttons now disable illegal directions before they can create an invalid draft placement.
- The selected weapon controls now show a compact `Move OK` or `Block ...` status line listing blocked directions plus short outside/overlap reasons beside the disabled direction buttons.
- The selected-weapon nudge status now uses shorter Move OK/Block wording, keeping the direction-pad area readable in the right drawer.
- The filler action button now colors Armor, Sink, and Clear states so the next single-cell edit reads before clicking.
- The editable filler button now uses the same `+Armor`, `+Sink`, or `Clear` short action text as the compact target row.
- Disabled filler targets now read Lock or Stk, so weapon-filled or stacked cells do not look like editable armor/sink cells.
- Weapon-filled filler targets now use the shorter Lock label, keeping the target button language aligned with compact Pick/Block states.
- Stacked filler targets now use the short Stk label, matching the compact button language in the same target row.
- The mech bay now uses compact squad buttons and expands one selected fitting card at a time, reducing drawer scroll pressure before the future non-IMGUI rewrite.
- The squad fitting selector now marks pending-fit mechs and shows the selected mech's pilot, fit state, structure, plus H/W/G pressure bars in the same compact strip.
- The selected fitting card now repeats the selected weapon's combat stats, compact cell count, and `WxH` footprint above the mounted weapon buttons, reducing trips between the grid detail line and weapon selector.
- The selected weapon summary now drops verbose `Cells` and `Shape` words in favor of compact `C` plus `WxH` readouts, reducing right-drawer overflow pressure.
- Mounted weapon buttons now replace the active weapon number with `>`, making the move target readable even without relying on color.
- Mounted weapon buttons now show `*` for weapons with unapplied slot moves, so draft placement edits are visible from the list.
- Mounted weapon buttons now drop duplicate heat/load text in favor of compact S/M/L range labels, `WxH` shape labels, and range-band colors matching the payload grid.
- The selected weapon summary now echoes base, current, and pending target grid positions, so slot moves stay visible even before reaching the lower Place controls.
- The selected weapon summary now uses a compact `W# Base/Moved` prefix and cyan/amber border cue, so draft placement state is visible without adding another row.
- The selected weapon edit controls now mirror that same Base/Moved coordinate chain, keeping the nudge/place area consistent with the summary line.
- Payload detail rows now replace verbose `Cells`/`Shape` wording with compact `C` plus `WxH` readouts, and the edit lane now calls unapplied loadout changes `Pending fit`.
- The mech bay selector and squad replacement cycling now use Pending, Outgoing set, and Reserve set status text instead of player-facing draft wording.
- The roster fit detail now replaces internal owned-mech ids with Ready Bay or Reserve Bay labels, keeping the line player-facing.
- Reserve prep logs and save-result labels now use Reserve Prep wording instead of local/candidate terminology while keeping existing command names stable.
- The fitting drawer header and selector now use Mech Lab wording, nudging the current IMGUI drawer toward the dedicated Mech Lab surface planned next.
- Destroyed cockpit, arm, and leg sections now leave persistent tactical-view damage beacons in addition to the existing flashes, detached parts, and ejection effects; critical but not-yet-destroyed sections now vent small smoke/spark cues without adding more combat HUD text.
- Missile, ballistic, and energy attacks now have clearer family-specific muzzle, trace, and hit accents layered over the existing primitive combat effects, with bright brief energy cores, missiles using staggered salvo-spread arcs, and ballistic hits using punch rings.
- Hit effects now scale by incoming damage and kill state, with killing blows adding shock rings so light hits, heavy hits, and kills separate visually at tactical zoom.
- Combat hits now add short inbound direction slashes at the impact point, making attack direction readable without adding combat HUD rows.
- Weapon traces now start from family-specific arm or shoulder hardpoints, so energy, missile, and ballistic fire no longer originates from the mech center.
- Unit hit effects now land on section-aware cockpit, arm, leg, or torso points, so combat hits line up better with the reported damage section.
- Section-aware hit points now carry compact cockpit, arm, leg, and torso flash accents, improving hit readability without adding combat HUD rows.
- Armor hardness mitigation now emits short battlefield glint/spark cues, so armor plates read during hits without adding combat HUD rows.
- The debrief now compresses completed objectives, enemy kills, destroyed targets, and damaged player mechs into readable summary rows with compact section damage tags and `+n` overflow counts.
- The debrief now highlights one compact Outcome row with objectives, kills, damage, net funds, and salvage before the detailed payout rows.
- A command-file `assert-debrief-summary` hook and `mc2_01-debrief-summary.txt` smoke path now verify result counters and the compact combat summary row.
- A command-file `mc2_01-visible-flow-audit.txt` smoke path now chains combat situation, debrief summary, reserve prep, squad handoff, launch identity, and loadout compact checks as the Phase A visible-flow guard.
- The combat HUD now includes a compact situation row for squad readiness, active hostiles, live targets, and contact tempo above the recent event log.
- A command-file `assert-combat-situation` hook and `mc2_01-combat-situation.txt` smoke path now verify that row, with quiet/contact driven by recent combat events instead of generic log entries.
- Weapon selection, move, place, and selected-slot reset results now report `W# Moved @a,b>c,d` or `W# Base @a,b` in the top status.
- The selected weapon summary and payload detail now show the selected block as a concrete `WxH` footprint, making vertical and wide weapon shapes readable without counting cells.
- The payload weapon detail row now uses compact weapon short codes such as `AC10` instead of repeating full source weapon names.
- The mounted weapon buttons also include that `WxH` footprint, so the selection list itself reads as block inventory instead of plain weapon toggles.
- The selected-weapon Reset control now reads Base, stays disabled, and uses the done-state color when there is no placement override, reducing no-op clicks in the fitting controls.
- The fit Apply control now reads and colors itself as Done, Invalid, Stock, or Apply depending on draft, validation, and inventory state.
- The draft Reset control now reads Clean and uses the done-state color when the current fit already matches the applied fit.
- The selected fitting card now merges structure, movement, primary weapon, damage, range, cooldown, heat, and weight into one compact combat summary row, trimming duplicated header space.
- The payload grid side rail now uses compact Grid/A/S readouts and E/S/M/L/C swatches, moving placement controls closer to the grid without changing the fitting rules.
- Mounted weapon buttons now show only selector, range band, and `WxH` footprint, with tighter rows while the selected weapon summary keeps the full name and stats.
- The payload grid section now reserves height for the compact side-rail controls, preventing the weapon list from crowding the movement/status controls after the density pass.
- The fitting Apply/Reset row now sits inside a compact status strip with stock/draft/applied cue borders, matching the rest of the right-drawer control language.
- The selected fitting card title now uses a short `Fit` label with chassis and pilot text only, keeping owned/runtime ids out of the visible card.
- The selected fitting pressure row now uses compact `Fit OK` or `Review` state plus `H/W/G` labels instead of longer Fit Review/Heat/Load/Grid wording.
- The payload component detail row now uses compact `A+`, `C+`, and `W` labels without repeating Armor/Sink or Hard/Cool/Load wording.
- The payload open-slot detail row now uses compact `Slot x,y open` wording instead of separate Empty/Selected Slot labels.
- A command-file `assert-loadout-compact` hook and `mc2_01-loadout-compact.txt` smoke path now guard the compact fitting title, mounted-weapon button labels, and grid-section height contract.
- The selected fitting card condition row now uses compact `Cond` text and a narrower Repair action, keeping the repair/state readout aligned with the denser loadout controls.
- The selected fitting card condition row now hides zero repair cost for healthy mechs and changes the repair button to `OK` until damaged-mech repair is needed.
- The loadout compact smoke now also asserts the condition row contract, including the short `Cond` label and narrow Repair action geometry.
- The loadout compact smoke now also asserts the Apply/Reset strip's short label set and narrow button geometry, guarding the `Done/Apply/Invalid/Stock` plus `Clean/Reset` language without assuming every current fit can apply.
- The loadout compact smoke now also asserts the selected-weapon Base/Reset control, keeping that no-op/dirty-state button short and narrow beside the nudge pad.
- The loadout compact smoke now also asserts target action labels and geometry, guarding compact `Place/Block/Pick` and `+Armor/+Sink/Clear/Lock/Stk` controls beside the payload grid.
- The loadout compact smoke now also asserts the compact target detail weapon reference, keeping placement details on `for W# code` wording.
- The loadout compact smoke now also asserts the nudge pad labels and geometry, guarding `N/W/E/S` plus `Move OK/Block` feedback as compact selected-weapon movement controls.
- The loadout compact smoke now also asserts the selected-weapon summary row, guarding the compact `W# Base/Moved`, `D/R/CD/H/W/C`, and `WxH` readouts that explain the active weapon block.
- The selected-weapon summary row now uses compact weapon short codes such as `AC10` instead of repeating full source weapon names.
- The loadout compact smoke now also asserts the selected-weapon edit-control header, keeping it to a short `W# Base/Moved @x,y` coordinate line.
- The loadout compact smoke now also asserts compact `A+` and `C+` grid labels for armor and heat-sink component blocks.
- The loadout compact smoke now also asserts compact weapon block short codes such as `AC10`, avoiding duplicated `AC (AC10)` text inside small grid blocks.
- The loadout compact assertion is now split into small check helpers, keeping the growing UI contract readable before more fitting-card density rules are added.
- The fitting Apply/Reset strip now uses compact Fit OK, Pending, and Stock state text, guarded by the loadout compact smoke.
- The Save Slot title, result row, reset confirmation, and New Company confirmation now use calmer player-facing wording guarded by the loadout compact smoke.
- The Contracts panel now uses simpler player-facing task-card and navigation labels such as Available Contracts, Mission map ready, Launch, Back to Battle, and Back to Debrief guarded by the debrief smoke.
- The System and Debrief restart action now reads Restart Mission, and the System close action reads Back, keeping the pause/system flow closer to player intent and guarded by the debrief smoke.
- The Mission Map and Mech Lab top-right close actions now read Back, with both labels guarded by combat-situation and loadout compact smoke paths.
- The top status strip now says `MODE Battle/Mech Lab/Debrief` instead of `FLOW ...`, guarded by combat-situation, debrief, and loadout smoke paths.
- The top status resource readout now says `Funds` instead of `TOKEN`, guarded by combat-situation, debrief, and loadout smoke paths.
- Mech Lab shop, purchase, hire, repair cost rows, and Repair All completion feedback now say funds instead of token or bare values, guarded by the loadout compact smoke path.
- Mission payout combat-log entries now say Payout and Salvage instead of Receipt/token/fragments, guarded by the loadout compact smoke path.
- The Mech Lab save-file check row now uses Check, Load Check, and No save checked wording instead of preview-language controls, guarded by the loadout compact smoke.
- The Save Slot reset result now says fresh save and old copy instead of default-save wording, with the no-saved-game case guarded by the loadout compact smoke.
- The Next Contract Squad and Reserve Fit Review subpanels now use Back instead of Hide, guarded by the hidden handoff launch smoke.
- The README and current-plan baseline now describe the same Reserve, Fit Review, and Funds wording that the guarded Mech Lab UI exposes.
- The Mech Lab now opens as a desktop-width two-column surface on normal Windows demo resolutions, with Company Bay actions separated from the Loadout fitting grid and the compact drawer kept as fallback.
- The first dedicated Mech Lab visual audit is done, and Company Bay density is reduced with Ops/Roster paging; the next low-risk step is to switch to battle readability unless another Mech Lab visible blocker appears in play.
- Cockpit destruction now extends the ejection cue with a short chute, cord, descending pod, and landing beacon so pilot escape reads at fixed tactical zoom without adding HUD text.
- Destroyed mechs now add short hot/metal debris pieces over the existing wreck blast, smoke, scorch, and marker cues so a full kill separates from ordinary hit severity at tactical zoom.
- Enemy contact activation now has a smoke-guarded visual cue contract: `ContactWake=ring+beacon+ping`.
- Hit severity now has a smoke-guarded battlefield cue contract: `HitSeverity=damage+kill+shock`.
- Hit direction now has a smoke-guarded battlefield cue contract: `HitDirection=inbound+slash`.
- Weapon muzzle placement now has a smoke-guarded battlefield cue contract: `WeaponMuzzlePoint=energy+missile+ballistic`.
- Section-aware hit placement now has a smoke-guarded battlefield cue contract: `SectionHitPoint=cockpit+arms+legs+torso`.
- Section-aware hit flashes now have a smoke-guarded battlefield cue contract: `SectionHitCue=cockpit+arms+legs+torso`.
- Section status labels now have a smoke-guarded UI cue contract: `SectionStatus=bar+short-label+critical+destroyed`.
- Debrief player damage rows now have a smoke-guarded summary contract: `DebriefDamage=unit+section+overflow`.
- Debrief destroyed target rows now have a smoke-guarded summary contract: `DebriefTargets=count+labels+overflow`.
- Cockpit ejection now extends the section damage contract with `Cockpit=breach+ejection-pod+chute`.
- Armor mitigation now has a smoke-guarded battlefield cue contract: `ArmorMitigation=glint+spark`.
- Weapon target lines now have a smoke-guarded battlefield cue contract: `TargetLine=ready+cooling+blocked`.
- Player weapon readiness now has a smoke-guarded battlefield cue contract: `WeaponReadiness=ready+cooling+blocked`.
- Target lock acquisition now has a smoke-guarded battlefield cue contract: `TargetLock=auto+command`.
- Squad focus pressure now has a smoke-guarded battlefield cue contract: `SquadFocus=ring+pressure+beacon`.
- Hostile focus warning now has a smoke-guarded battlefield cue contract: `ThreatFocus=ring+warning+beacon`.
- Player mech damage now has a smoke-guarded battlefield cue contract: `PlayerDamage=warning+critical`.
- Combat tempo pressure now has a smoke-guarded battlefield cue contract: `CombatPressure=tracking+contact+fire+beacon`.
- Hostile pressure center now has a smoke-guarded battlefield cue contract: `HostilePressure=centroid+tempo+beacon`.
- Visible objective events now have a smoke-guarded battlefield cue contract: `ObjectivePulse=active+complete+target`.
- Active objective guide hints now have a smoke-guarded battlefield cue contract: `ObjectiveGuide=active+beacon+target`.
- Active objective route hints now have a smoke-guarded battlefield cue contract: `ObjectiveRoute=commander+target`.
- Active objective target pressure now has a smoke-guarded battlefield cue contract: `ObjectivePressure=steady+damaged+critical`.
- Jet movement now has a smoke-guarded battlefield cue contract: `Jet=takeoff+trail+landing`.
- Target structure damage now has a smoke-guarded battlefield cue contract: `Structure=scar+smoke+collapse+rubble`.
- Script bridge events now have a smoke-guarded battlefield cue contract: `ScriptCue=ring+beacon+signal`.
- Mission result transition cues now have a smoke-guarded battlefield cue contract: `ResultCue=complete+failed`.
- Destroyed mech wreck cues now have a smoke-guarded battlefield cue contract: `Wreck=blast+smoke+marker+debris`.
- Accepted command cues now have a smoke-guarded battlefield cue contract: `Command=move+attack+single`.
- Move and Jet order paths now have a smoke-guarded battlefield cue contract: `OrderPath=move+jet`.
- Move and Jet arrivals now have a smoke-guarded battlefield cue contract: `OrderArrival=move+jet`.
- Heat pressure cues now have a smoke-guarded battlefield cue contract: `Heat=vent+lock`.
- Commander follow cues now have a smoke-guarded battlefield cue contract: `Commander=anchor+beacon`.
- Solo-order completion now has a smoke-guarded battlefield cue contract: `SoloReturn=ring+beacon`.
- Selecting assembled mechs for future missions, saved accounts, event drop tables, and multiplayer support still come later.
