# AI Commander Directive Contract

Goal: keep model latency and uncertainty out of the frame-by-frame battle loop, and keep development focused on the game itself.

The AI commander is a strategic draft assistant, not a direct unit controller. It may propose an opening plan, expose a small capability window, and choose one high-level directive for a short phase of the mission. BattleCore then converts that directive into deterministic local commands such as squad movement, target focus, regrouping, heat handling, and future avoidance behavior.

## Product Boundary

- AI is optional flavor and planning support, not the core combat engine.
- AI can draft an opening plan before or at mission start.
- AI can show a capability window: current directive, available broad options, confidence, and why a plan is suggested.
- AI should not be used for continuous steering, exact coordinates, exact target IDs, weapon timing, or per-mech micro.
- After this prototype boundary is proven, development priority returns to the game: map flow, combat feel, damage feedback, mech fitting, mission UI, rewards, and progression.

## Runtime Boundary

- Model input: compact battle summary, not the full simulation state.
- Model output: one directive token.
- Local responsibility: pathing, exact target selection, movement legality, jump landing checks, heat/cooldown timing, weapon fire, damage, and objective completion.
- Failure behavior: if the model is slow, unavailable, or returns invalid text, use `assault-objective`.

## Compact Observation V1

Schema id: `mc2-ai-observation-compact-v1`.

Allowed model input:

- mission id, report index, mission phase, mission time, mission ended flag, and result state;
- commander identity: commander unit id, owned mech id, and chassis/type;
- current objective summary: active objective count, title, target kind, target count, and range band;
- player squad summary: unit counts, active/damaged/detached/destroyed/heat-locked counts, average structure percent, hottest heat percent;
- bounded player states for up to six units: role, chassis/type, active/destroyed/detached/moving/jumping/heat-locked state, structure percent, heat percent, weapon-ready percent, and compact section-damage tags;
- hostile pressure summary: active hostile count, nearby hostile count, hostiles in weapon range, threat level, targetable structure count, and up to three nearby threat bands;
- available high-level directive tokens.

Forbidden model input:

- full `playerUnits`, `activeHostiles`, `targetableStructures`, or `currentObjectives` arrays;
- exact hostile ids, exact objective coordinates, exact unit positions, exact move targets, or exact attack target ids;
- projectile history, path graphs, per-frame traces, weapon cooldown timelines, or hit-by-hit logs;
- inventory, account, save-slot, token, purchase, or repair-management data.

Size budget:

- Compact observation JSON should stay below 2400 characters for the first demo.
- MiniMax strategic prompt should stay below 1600 characters and should be readable as a phase summary.

The full `CommanderObservation` can remain available to local deterministic systems such as `RuleCommander`, validator diagnostics, and explicit debug reports. The model-facing path must use the compact summary when present.

## Directive Tokens

```text
assault-objective
engage-hostiles
regroup
hold
```

- `assault-objective`: local commander advances the mission objective, attacking objective structures if in range.
- `engage-hostiles`: local commander prioritizes hostile units already in weapon range; if no good target exists, it continues the objective.
- `regroup`: local commander pulls the squad toward the commander unit before continuing.
- `hold`: local commander issues no new startup command for this phase.

## Latency Rules

- Do not ask the model for per-shot, per-mech, or per-frame decisions.
- Do not ask the model to choose coordinates or target IDs.
- Treat one model call as a phase decision for roughly 10-30 seconds of simulated mission time.
- Cache the latest directive in future interactive builds; the battle must keep running if a new directive is pending.
- Prefer pre-mission or paused/system-panel calls over live-combat calls.

## First Demo Scope

The current startup path uses `-mc2MinimaxCommanderSteps <n>` for smoke testing. Each step:

1. Builds a compact strategic summary.
2. Requests one directive from MiniMax.
3. Converts the directive with `RuleCommander`.
4. Executes the resulting local command.
5. Advances the local simulation.

The next production pass should make the model call asynchronous and let the current directive remain active until a new directive arrives.

## Near-Term Stop Line

Do not expand AI integration beyond:

1. Opening plan draft.
2. Small capability window.
3. Optional high-level directive refresh.

Everything below this line belongs to local game development until the playable loop is strong.
