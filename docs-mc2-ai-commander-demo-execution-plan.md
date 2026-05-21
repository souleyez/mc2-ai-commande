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
- compact tactical UI with unit status, mech bay preview, mission brief, current-objective map, world objective hints, health bars, command rings, range rings, and target lines
- initial BattleCore loadout contract for chassis grids, item shapes, heat, weight, armor plates, heat sinks, radar slots, and jump jet slots
- deterministic BattleCore loadout validator for grid bounds, blocked cells, overlap, rotation, heat caps, weight caps, and special-slot compatibility
- source weapon loadout projection in the mech bay with validator status, heat/load limits, and occupied grid cell counts
- temporary mech bay weapon toggles that immediately recompute projected heat, load, grid occupancy, and fitting status
- projected mech bay slot grid visualization with placeholder multi-cell weapon shapes exposed from BattleCore preview data
- temporary projected weapon placement edits with overlap and out-of-bounds validator feedback
- spare-load armor plate and heat sink filler projection with hardness and cooling totals
- toggleable projected filler cells that cycle between armor, heat sink, and empty
- mech bay draft/apply/reset boundaries for temporary demo fits
- applied demo fit handoff into combat readiness, range, heat, cooldown, and weapon display stats
- applied armor hardness reduces incoming damage and applied heat sinks increase cooling
- applied armor/cooling bonuses appear in unit weapon status and blocked damage appears in combat logs
- starter mech bay inventory contract summarizes owned mechs, weapons, armor plates, heat sinks, and demo token balance
- starter mech bay roster preview lists owned squad and assembled depot mechs with a read-only detail view
- assembled depot mechs stay held with a pending-loadout placeholder until a future fitting flow equips them
- depot roster detail shows a disabled Draft Fit affordance as a visible future fitting stub
- depot fit stub previews missing spare weapon stock and pilot assignment requirements
- depot roster detail shows a read-only pilot placeholder for the future pilot/social system
- depot roster detail previews spare weapon stock counts before the shop or fitting flow exists
- mech bay summary previews an ordinary weapon shop as the future source of spare depot weapons
- mech bay summary supports a demo ordinary weapon purchase that spends local tokens and adds one spare weapon
- mech bay summary and roster detail support demo NPC pilot hiring that spends local tokens and assigns a warehouse pilot
- depot Draft Fit affordance becomes a readiness gate once spare weapon stock and a pilot are both present
- warehouse Draft Fit opens a read-only preview showing the selected pilot and spare weapon without changing inventory
- warehouse Draft Fit preview can apply a demo placeholder fit that consumes one spare weapon and keeps the mech non-deployable
- fitted warehouse mechs show a deployment preview explaining that future squad selection is still required
- roster detail shows a squad-selection placeholder without altering current mission deployment
- squad-selection preview lists current mission slots and fitted depot candidates without changing inventory or deployment
- squad-selection preview shows a disabled swap guard with future replace-slot requirements
- squad-selection preview shows a dry-run replacement summary without applying roster changes
- squad-selection preview shows a pending swap confirmation stub without applying roster changes
- starter inventory availability feedback warns on armor plate or heat sink shortages and blocks applying invalid drafts
- starter mech condition and one-click demo repair spend local token balance and restore damaged mechs
- starter mission receipt applies completed bounty tokens and salvaged mech fragments to local inventory
- starter fragment assembly preview shows progress toward the demo mech assembly threshold and auto-assembles ready sets into local warehouse mechs
- CLI/AI loop pieces:
  - `-mc2Command`
  - `-mc2AdvanceSeconds`
  - `-mc2ReportState`
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

Start with **starter squad-selection rejected apply result**.

Reason:

- Mission rewards now add tokens and salvage fragments, and ready fragment sets become local warehouse mechs.
- The mech bay now has a compact roster line and a read-only detail view for owned mechs.
- Assembled depot mechs now stay held with `pending-loadout`, so they cannot be confused with deployable squad mechs.
- The roster detail now shows a disabled Draft Fit affordance, making the future fitting workflow visible without enabling it yet.
- The depot fit stub now previews missing spare weapon stock and pilot assignment requirements.
- The roster detail now shows a read-only pilot placeholder, so the future pilot/social system has a clear UI slot.
- The roster detail now previews spare weapon stock counts, confirming that starter source weapons are all mounted and no depot fitting weapon stock is free yet.
- The mech bay summary now shows an ordinary weapon shop preview, so the future source of spare weapon stock is visible.
- The mech bay summary now supports a tightly scoped demo-only ordinary weapon purchase that deducts tokens and adds one spare weapon stack.
- The mech bay now supports demo-only NPC pilot hiring that deducts tokens and assigns a pilot to a warehouse mech.
- The depot Draft Fit affordance now becomes clickable once spare stock and a pilot are both present, giving feedback without changing loadouts.
- The depot Draft Fit affordance now opens a read-only preview showing the selected pilot and spare weapon without changing inventory or loadouts.
- The depot Draft Fit preview now has a demo-only Apply action that consumes one spare weapon and changes the warehouse mech from `pending-loadout` to `warehouse-demo-fit`, while keeping it non-deployable.
- The roster now explains why fitted depot mechs are still held until a future squad-selection flow exists.
- The roster now shows a squad-selection placeholder, so the deployment path is visible before it can alter mission rosters.
- The squad-selection preview now lists current mission slots and fitted depot candidates without changing deployment.
- The squad-selection preview now includes a disabled swap guard, so the future replace-slot action has visible rules before it can mutate mission rosters.
- The squad-selection preview now shows a dry-run replacement summary that selects one current slot and one depot candidate without applying it.
- The squad-selection preview now shows a pending swap confirmation stub, so the UI can show a staged replacement before any inventory or mission roster mutation exists.
- The next low-risk step is a rejected apply-result contract, so pressing into the future confirmation path can return a safe no-op result before real roster mutation exists.
- Selecting assembled mechs for future missions, saved accounts, event drop tables, and multiplayer support still come later.
