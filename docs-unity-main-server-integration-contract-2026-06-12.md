# Unity Main-Server Integration Contract

Date: 2026-06-12

Status: F7 contract. This is a documentation and gate step only. It does not add
a Unity runtime dependency on the server.

Stable markers:

- UnityMainServerIntegrationContract: True
- UnityServerIntegrationOptional: True
- UnityOfflineFirst: True
- NoRuntimeServerDependency: True
- NoLoginPaymentMarketplaceRealtimePvpChainInUnityAdapter: True
- NoPerFrameServerCalls: True
- SignedSquadBeforeLaunch: True
- RewardClaimAfterBattle: True

## Purpose

Unity may later talk to the local main-server prototype for two boundary calls:

1. Before mission launch, request a signed squad loadout.
2. After battle debrief, submit a reward claim.

The active battle loop stays local. BattleCore movement, targeting, damage,
heat, section loss, ejection, AI deputy fallback and mission script execution
must never wait on the server during a frame.

## Current Server Surface

The Unity adapter must use the endpoint names already implemented by
`server/main-server/main-server.mjs`:

- `GET /healthz`
- `GET /version`
- `POST /dev/accounts`
- `GET /accounts/{accountId}/inventory`
- `POST /squads/sign`
- `POST /reward-claims`
- `GET /leaderboards/basic`
- `POST /dev/reset`

`POST /dev/accounts` and `POST /dev/reset` are local development helpers. A
production account path will replace them later, but the first Unity adapter can
use the deterministic local account while proving request and fallback behavior.

## Unity Adapter Records

The future Unity adapter should keep these records separate from BattleCore
runtime state:

- `UnityServerSettings`
- `UnityServerStatus`
- `UnityInventoryBootstrap`
- `UnitySquadSignRequest`
- `UnitySignedSquadResult`
- `UnityBattleResultClaim`
- `UnityRewardClaimResult`
- `OfflineFixtureFallback`

These records are boundary DTOs. They must not replace `MissionContract`,
`UnitState`, `LoadoutContract` or existing offline fixtures.

## Startup And Health

Optional startup probe:

```http
GET /healthz
GET /version
```

Unity may mark the server as available only when both responses prove:

- `service` is `mc2-main-server-local`
- `unityOfflineFirst` is `true`
- `battleCoreVersion` matches `mc2-unity-demo-contract-v1`
- `excludedFirstSliceFeatures` still excludes payment, marketplace, realtime
  PVP and chain integration

If this probe fails or times out, Unity must continue with the local fixture.

FallbackServerUnavailable: local fixture squad

## Inventory Bootstrap

Optional development bootstrap:

```http
POST /dev/accounts
GET /accounts/{accountId}/inventory
```

Minimum fields Unity can consume:

- `AccountRecord.accountId`
- `AccountRecord.publicPlayerId`
- `InventorySnapshot.tokenBalance`
- `InventorySnapshot.ownedMechs`
- `ownedMechId`
- `activeLoadoutId`
- `availableForMission`
- `conditionPercent`
- `pilotId`
- `pilotDisplayName`
- `itemStacks`

If inventory bootstrap fails, Unity uses the same local fixture path used by
validator, Windows smoke, Android smoke and MechLab.

## Signed Squad Before Launch

Unity request:

```json
{
  "accountId": "local-dev-account",
  "mapId": "mc2_01",
  "mapVersion": "local-fixture-v1",
  "battleCoreVersion": "mc2-unity-demo-contract-v1",
  "ownedMechIds": ["demo-mech-01", "demo-mech-02", "demo-mech-03"]
}
```

Server response must be treated as signed only when:

- `schema` is `SignedSquadLoadoutResponse`
- `status` is `signed`
- `signedSquad.signedSquadId` is present
- `signedSquad.loadoutHash` is present
- `signedSquad.signature` is present
- `signedSquad.mapId` and `signedSquad.mapVersion` match the mission
- `signedSquad.battleCoreVersion` matches the Unity build contract
- `signedSquad.unitCount` is 1 through 6
- `signedSquad.ownedMechIds` match the selected local squad order

Unity should persist the signed fields only for the active mission session:

- `signedSquadId`
- `accountId`
- `publicPlayerId`
- `mapId`
- `mapVersion`
- `battleCoreVersion`
- `loadoutHash`
- `signature`
- `expiresAt`
- `ownedMechIds`

FallbackUnsignedSquad: local fixture launch with reward claim disabled

If signing is rejected, times out or returns an incompatible version, the mission
can still launch from local fixtures. The debrief may show local results, but it
must not submit a reward claim for that unsigned run.

## Reward Claim After Battle

Unity may submit a claim only after the mission result is final and the signed
squad is still associated with the current session.

Unity request:

```json
{
  "accountId": "local-dev-account",
  "idempotencyKey": "battle-summary-hash-or-guid",
  "signedSquadId": "signed-squad-...",
  "mapId": "mc2_01",
  "mapVersion": "local-fixture-v1",
  "battleCoreVersion": "mc2-unity-demo-contract-v1",
  "battleSummaryHash": "battle-summary-hash-or-guid",
  "claimSource": "unity-demo",
  "resultSummary": {
    "result": "success",
    "completedRewardResourcePoints": 1800,
    "causedDamageScore": 4000,
    "debuffSeconds": 30,
    "objectivesCompleted": 2,
    "enemiesDestroyed": 4,
    "squadLosses": 0
  }
}
```

Accepted response fields:

- `schema`: `RewardClaimResponse`
- `rewardClaim.status`: `approved`
- `rewardGrant.tokenDelta`
- `rewardGrant.rewardRulesVersion`
- `tokenLedgerEntry.ledgerEntryId`
- `tokenLedgerEntry.delta`
- `tokenLedgerEntry.balanceAfter`
- `inventorySnapshot.tokenBalance`
- `leaderboardRow.resultState`: `approved`

FallbackRejectedClaim: local debrief remains and inventory is not mutated

If the claim is rejected, Unity keeps the local battle result visible but does
not apply server inventory, token, fragment or leaderboard changes.

FallbackDuplicateClaim: idempotent approved response is success

If the same `idempotencyKey` is submitted twice and the server returns the same
approved ledger entry, Unity treats the second response as success and does not
double-apply local inventory deltas.

## Offline-First Rules

The current Unity demo remains offline-first:

- Validator still runs without server.
- Windows and Android smoke still run without server.
- MechLab can use local fixtures.
- Server failure must not block local BattleCore development.
- No account login is required in the first adapter.
- No payment, recharge, cash-out, marketplace, realtime PVP, chain integration,
  NFT minting or public map upload belongs in the Unity adapter.
- Server calls are limited to preparation and debrief; no per-frame polling, AI
  decisions, damage calculation or pathing calls are allowed.

## First Adapter Acceptance

A future Unity client adapter is acceptable only if:

- It can be disabled by config and the demo still passes existing offline gates.
- It uses the endpoint names in this document without changing
  `server/main-server`.
- It treats server data as account/inventory/reward metadata, not BattleCore
  authority during combat.
- It keeps local fixtures as the fallback for every failure path listed above.

Recommended next task: `F8 implement optional Unity main-server client adapter`.
