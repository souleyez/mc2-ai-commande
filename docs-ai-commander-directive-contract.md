# AI Commander Directive Contract

Goal: keep model latency and uncertainty out of the frame-by-frame battle loop.

The AI commander is a strategic planner, not a direct unit controller. It chooses one high-level directive for the next short phase of the mission. BattleCore then converts that directive into deterministic local commands such as squad movement, target focus, regrouping, heat handling, and future avoidance behavior.

## Runtime Boundary

- Model input: compact battle summary, not the full simulation state.
- Model output: one directive token.
- Local responsibility: pathing, exact target selection, movement legality, jump landing checks, heat/cooldown timing, weapon fire, damage, and objective completion.
- Failure behavior: if the model is slow, unavailable, or returns invalid text, use `assault-objective`.

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

## First Demo Scope

The current startup path uses `-mc2MinimaxCommanderSteps <n>` for smoke testing. Each step:

1. Builds a compact strategic summary.
2. Requests one directive from MiniMax.
3. Converts the directive with `RuleCommander`.
4. Executes the resulting local command.
5. Advances the local simulation.

The next production pass should make the model call asynchronous and let the current directive remain active until a new directive arrives.
