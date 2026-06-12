# Post-Receipt Inventory Refresh Boundary

Date: 2026-06-12

Status: F17 boundary plan. This is a documentation and gate step only. It does
not make the default Unity demo depend on a server.

Stable markers:

- PostReceiptInventoryRefreshBoundary: True
- ServerReceiptInventorySnapshotAuthority: True
- AcceptedClaimRefreshesInventorySnapshot: True
- DebriefUsesServerGrantAsOverlay: True
- MechBayRefreshAfterAcceptedClaim: True
- DuplicateClaimNoDoubleApplyLocalRefresh: True
- RejectedClaimKeepsLocalDebriefAndInventory: True
- LocalFixtureFallbackDefault: True
- NoDefaultRemoteReceiptDependency: True
- RefreshIsPostDebriefOnly: True
- BattleCoreDebriefIsReceiptSourceOnly: True
- NoBattleCoreFrameReceiptServerCalls: True
- ServerReceiptNotCombatAuthority: True
- MobileLandscapeOnly: True
- MobileFirstLandscapeOnly: True
- PortraitOutOfFirstSlice: True
- NoLoginPaymentMarketplaceRealtimePvpChain: True
- F18RecommendedNext: Implement opt-in post-receipt inventory refresh binding

## Purpose

F16 proved the local main server can sign a squad, accept a debrief-derived
reward claim, return the same response for duplicate claims, mutate the token
balance once, reject invalid claims without inventory mutation, and publish a
leaderboard row. F17 defines how Unity may consume that successful receipt
response after battle without turning the server into tactical authority.

The first implementation after this plan should be opt-in only. When the
optional main-server adapter is enabled and a claim is accepted, Unity may use
the returned `inventorySnapshot` to refresh the debrief summary and the next
MechBay / garage view. The default Windows demo, Android smoke, validator and
local MechLab path still use local fixtures when the adapter is disabled,
unavailable or rejected.

## Source Contract

Accepted server response:

```http
POST /reward-claims
```

Required response fields:

- `RewardClaimResponse.rewardClaim`
- `RewardClaimResponse.rewardGrant`
- `RewardClaimResponse.tokenLedgerEntry`
- `RewardClaimResponse.inventorySnapshot`
- `RewardClaimResponse.leaderboardRow`

Unity DTOs that already model the response:

- `UnityRewardClaimResult`
- `UnityRewardClaim`
- `UnityRewardGrant`
- `UnityTokenLedgerEntry`
- `UnityInventorySnapshot`
- `UnityLeaderboardRow`
- `OfflineFixtureFallback`

The accepted claim response is authoritative for reward, token balance,
inventory snapshot and leaderboard projection. It is not authoritative for
movement, weapons, damage, heat, mission triggers, AI decisions or camera state.

## Refresh Packet

F18 should introduce or equivalent-model a small post-receipt refresh packet
with these fields:

| Field | Source | Rule |
| --- | --- | --- |
| `source` | constant | `MainServerRewardClaim` when accepted or duplicate-approved. |
| `claimId` | `rewardClaim.claimId` | Required for debrief evidence and duplicate detection. |
| `ledgerEntryId` | `tokenLedgerEntry.ledgerEntryId` | Required; repeated ids must not reapply a local delta. |
| `tokenDelta` | `rewardGrant.tokenDelta` | Display only after accepted claim; do not recalculate in Unity. |
| `tokenBalance` | `inventorySnapshot.tokenBalance` | The next MechBay token balance after validated projection. |
| `inventorySnapshot` | `inventorySnapshot` | Must project through `UnityInventoryToMechBayProjector` before MechBay use. |
| `leaderboardRow` | `leaderboardRow` | Optional debrief/public score display; no gameplay authority. |
| `fallback` | `OfflineFixtureFallback` | Present when disabled, unavailable, rejected or invalid. |

Duplicate accepted claims are success responses, but Unity must treat a repeated
`ledgerEntryId` as already applied. The debrief can show the server accepted the
claim, but local preview state must not add `tokenDelta` a second time.

## Allowed UI Effects

Accepted claim:

- Debrief may show a compact line such as `Server Reward +2310`.
- Debrief may show the updated token balance from `inventorySnapshot`.
- Debrief may show a compact leaderboard/rank note from `leaderboardRow`.
- Returning to MechBay may refresh the inventory source line to
  `Inventory Source: Main Server Reward`.
- MechBay may use the validated projected inventory snapshot as the next
  garage state for token balance, owned mechs and item stacks.

Duplicate accepted claim:

- Debrief may show `Server Reward Already Applied`.
- MechBay must keep the already-refreshed inventory state and must not add a
  second local delta.

Rejected or unavailable claim:

- Debrief remains visible from local BattleCore data.
- MechBay keeps local fixture or the last validated local preview.
- The source line may show a short fallback reason.
- No token, fragment, mech assembly or leaderboard state is mutated locally from
  the rejected response.

## Validation Before MechBay Refresh

Before an accepted `inventorySnapshot` can replace the MechBay preview state:

1. `UnityRewardClaimResult.IsApproved == true`.
2. `rewardClaim.status == "approved"`.
3. `rewardGrant.claimId == rewardClaim.claimId`.
4. `tokenLedgerEntry.reason == "reward-claim"`.
5. `tokenLedgerEntry.balanceAfter == inventorySnapshot.tokenBalance`.
6. `inventorySnapshot.accountId == rewardClaim.accountId`.
7. `UnityInventoryToMechBayProjector.Project(...)` accepts the snapshot.
8. `MechBayInventoryValidator.Validate(projectedInventory).IsValid == true`.
9. The refresh is attached to the post-debrief transition, not to `Update`,
   `FixedUpdate` or `LateUpdate`.

If any validation fails, Unity keeps local fixture state and records the fallback
reason. Partial refresh is forbidden: do not merge just the token balance or
just item stacks from an invalid snapshot.

## Runtime Boundary

Allowed server call timing:

- Optional pre-launch squad signing.
- Optional post-battle reward claim after BattleCore debrief summary exists.
- Future explicit refresh button in MechBay debug/preview mode.

Forbidden call timing:

- `Update`, `FixedUpdate` or `LateUpdate`.
- BattleCore movement, firing, heat, damage, section loss, cockpit breach,
  ejection, pathing, targeting, AI deputy directive evaluation or mission
  trigger evaluation.
- Default validator, Windows visible-flow smoke or Android visible-flow smoke.

BattleCore remains the source of the battle result summary. The server may
approve and grant rewards from that summary, but it does not change the battle
that already happened.

## Mobile Boundary

The first phone version is landscape-only. Post-receipt refresh may add only
short, scan-friendly debrief and MechBay source text that fits the existing
horizontal phone layout. It must not add portrait UI, modal login, payment,
marketplace tabs, wallet prompts, map server publishing or server debug panels.

## F18 Implementation Shape

Recommended next task:

```text
F18 implement opt-in post-receipt inventory refresh binding
```

Expected F18 files:

- `UnityMainServerClient` may expose a typed helper that turns
  `UnityRewardClaimResult` into a refresh packet.
- `Mc2DemoBootstrap` may consume the packet only in the opt-in post-debrief
  path and then rebuild the MechBay preview from the projected snapshot.
- Command-file smoke may assert accepted, duplicate and rejected refresh
  behavior without changing the default visible-flow smoke.
- Evidence should remain under ignored `analysis-output/`.

F18 acceptance should prove:

- accepted claim refreshes debrief and MechBay token balance from the server
  snapshot;
- duplicate claim does not double-apply token delta;
- rejected claim leaves local debrief and MechBay inventory intact;
- default offline demo remains playable with local fixtures;
- no per-frame server calls are introduced;
- landscape phone MechLab/debrief layout remains the first mobile target.

## Explicit Non-Goals

- No login.
- No payment, recharge, cash-out or marketplace.
- No realtime PVP.
- No chain, wallet, NFT or withdrawable token.
- No map-server upload path.
- No anti-cheat replay adjudication.
- No portrait mobile UI.
- No required remote server dependency for local Windows or Android smoke.
- No per-frame server calls.

## Verification

Run from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_post_receipt_inventory_refresh_boundary.ps1 -RepoRoot .
```

Expected success string:

```text
Post-receipt inventory refresh boundary check OK.
```
