# Platform Reward Authority Contract

Goal: define how partner, community, or official map servers can submit battle
results without gaining authority to mint portable rewards.

This is a contract document only. It does not implement a server, API, database,
wallet, or chain integration.

## Core Rule

```text
Map servers and clients submit reward claims.
The main server alone validates claims, calculates grants, mutates inventory,
updates the token ledger, and publishes rankings.
```

Portable rewards include tokens, mech fragments, weapons, skins, cosmetic
materials, ranking points, event points, and creator revenue events.

## Actors

| Actor | May Do | Must Not Do |
| --- | --- | --- |
| Unity client | request session ticket, play battle, submit local telemetry, upload replay when required | mutate portable inventory, decide reward amount, bypass map limits |
| Map server | host room/session, serve certified map version, collect battle telemetry, submit signed result claim | mint tokens/fragments/weapons/skins, alter player-owned loadout, change certified reward table |
| Main server | issue session tickets, verify claims, calculate rewards, mutate ledger, update rankings | trust unsigned results, grant rewards from map-provided amounts without validation |
| Battle validator | replay or inspect summary/timeline using BattleCore-compatible contracts | depend on Unity presentation-only state |
| Web/ranking service | publish approved results and leaderboards | expose private account ids, anti-cheat internals, or rejected-claim detail |

## Reward Lifecycle

1. Client asks main server for a session ticket.
2. Main server signs a ticket with account id, squad hash, map id/version, seed,
   reward table id, expiration, and claim limits.
3. Client joins a map server with that ticket.
4. Map server runs the battle using the certified map version and deterministic
   seed.
5. Client and map server submit a reward claim.
6. Main server verifies ticket, signatures, map version, squad hash, result
   summary, timeline digest, caps, and replay requirements.
7. Main server calculates final reward from its own reward table.
8. Main server writes an idempotent ledger transaction.
9. Main server publishes approved ranking/result rows.

## Session Ticket

Minimum fields:

```json
{
  "ticketId": "sess_20260607_001",
  "accountId": "acct_public_or_internal_id",
  "squadHash": "sha256:squad-loadout-and-pilot-snapshot",
  "mapId": "island-airfield-contract",
  "mapVersion": "1.0.0",
  "mapHash": "sha256:certified-map-package",
  "rewardTableId": "reward.island-airfield.v1",
  "seed": "seed-opaque-main-server-issued",
  "issuedAt": "2026-06-07T00:00:00Z",
  "expiresAt": "2026-06-07T00:30:00Z",
  "limits": {
    "maxClaims": 1,
    "maxReplayDelaySeconds": 300,
    "requiresReplay": false
  },
  "mainServerSignature": "signature"
}
```

The ticket is the only reward-bearing entry authority. A map server cannot make
an uncertified session reward-bearing by itself.

## Reward Claim

Minimum fields:

```json
{
  "claimId": "claim_20260607_001",
  "ticketId": "sess_20260607_001",
  "accountId": "acct_public_or_internal_id",
  "squadHash": "sha256:squad-loadout-and-pilot-snapshot",
  "mapServerId": "mapserver.partner-01",
  "mapId": "island-airfield-contract",
  "mapVersion": "1.0.0",
  "mapHash": "sha256:certified-map-package",
  "battleCoreVersion": "battlecore-v1",
  "seed": "seed-opaque-main-server-issued",
  "startedAt": "2026-06-07T00:01:00Z",
  "endedAt": "2026-06-07T00:09:30Z",
  "result": {
    "outcome": "victory",
    "durationSeconds": 510,
    "visibleObjectives": 6,
    "completedVisibleObjectives": 6,
    "squadSurvivors": 4,
    "squadLosses": 0
  },
  "score": {
    "damageDealt": 12450,
    "debuffSeconds": 38,
    "salvageEligibleTargets": 2,
    "repairCostEstimate": 1200
  },
  "timelineDigest": "sha256:ordered-event-digest",
  "replayUri": "",
  "clientSignature": "signature",
  "mapServerSignature": "signature"
}
```

The claim may include suggested reward rows for debugging, but those rows are
advisory only. The main server recalculates the final grant.

## Validation Gates

Every claim must pass:

- ticket exists and is signed by the main server;
- ticket is not expired, consumed, revoked, or already granted;
- account id and squad hash match the ticket;
- map id, version, hash, and reward table match a certified registry entry;
- result summary is structurally valid and internally consistent;
- timeline digest matches the claimed result summary;
- map server is registered and allowed to host that map/version;
- account, map, event, and daily caps are still available;
- claim id is idempotent and not replayed with different content.

High-value claims additionally require one or more:

- full replay upload;
- server-side deterministic BattleCore re-simulation;
- client telemetry comparison;
- map-server reputation check;
- manual review for event or prize pools.

## Claim States

| State | Meaning |
| --- | --- |
| `received` | Claim accepted for validation but no rewards granted. |
| `rejected` | Claim failed validation; no portable rewards. |
| `capped` | Claim is valid, but reward is reduced by limits. |
| `pending-replay` | Claim waits for replay or deterministic re-simulation. |
| `approved` | Claim passed validation and is ready to grant. |
| `granted` | Ledger mutation completed. |
| `voided` | Prior grant was administratively reversed. |

Only `granted` changes portable inventory.

## Grant Calculation

The main server calculates grants from:

- certified reward table id;
- account state and daily/event caps;
- map difficulty and certification tier;
- result summary and score fields;
- anti-abuse and anomaly limits;
- event configuration.

Map packages reference reward table ids; they do not define direct token,
fragment, weapon, skin, or ranking grants.

## Ledger Rules

Ledger mutations must be:

- idempotent by `claimId`;
- append-only with correction rows instead of destructive edits;
- auditable by account id, map id/version, reward table id, and validator result;
- reversible by admin action through a `voided` or correction transaction;
- separate from public ranking rows.

The token ledger remains centralized for the first platform version. Chain
settlement, if used later, should mirror creator payouts or cosmetics after the
central ledger is stable.

## Rejection And Capping Examples

Reject:

- invalid or missing signature;
- ticket not issued by main server;
- map hash differs from certified version;
- squad hash differs from ticket;
- impossible duration or objective sequence;
- duplicate claim id with different content;
- map server not authorized for the map/version.

Cap:

- account already reached daily reward limit;
- map reward table has per-day or per-event ceiling;
- suspicious but not conclusive clear-time anomaly;
- repeated farming beyond normal expected distribution.

## Ranking Publication

Approved results may publish:

- public player handle or anonymized id;
- map id/version;
- season/event id;
- squad snapshot hash or public loadout snapshot;
- outcome, duration, objective count, score, and ranking points.

Do not publish:

- private account id;
- raw signatures or session secrets;
- anti-cheat thresholds;
- rejected-claim internals;
- unpublished inventory details.

## First Implementation Slice

When server work begins, implement only:

1. session ticket issue and signature;
2. reward claim receive endpoint;
3. map registry lookup;
4. summary validation;
5. idempotent ledger write;
6. basic leaderboard row for granted claims.

Replay re-simulation, partner server reputation, creator revenue, and chain
proofs are later stages.
