# Server-Backed Receipt Slice Plan

Status: Done for the F15 planning pass.

Date: 2026-06-12

## Stable Markers

```text
ServerBackedReceiptSlice: True
ReceiptAuthority: MainServer
BattleCoreFrameLoopServerCalls: False
NoBattleCoreFrameServerCalls: True
UnityOfflineFirst: True
ReceiptSource: DebriefSummary
RewardClaimIdempotent: True
TokenLedgerAuthoritative: True
InventorySnapshotAfterGrant: True
LeaderboardProjectionFromAcceptedClaim: True
SignedSquadValidationRequired: True
MapServerCannotGrantRewards: True
ClientCannotMutateInventoryDirectly: True
DuplicateClaimReturnsSameLedgerEntry: True
RejectedClaimDoesNotMutateInventory: True
ServerReceiptsNotRealtimePvp: True
NoPaymentNoCashoutNoChain: True
MobileLandscapeUnaffected: True
MobileFirstLandscapeOnly: True
PortraitOutOfFirstSlice: True
F16RecommendedNext: Implement server-backed receipt evidence gate
```

## Product Decision

The first server-backed receipt slice should make the main server authoritative
for accepted battle receipts, token grants, inventory snapshots and leaderboard
projection. It must not move tactical battle simulation into the server, and it
must not add per-frame server calls to BattleCore.

Unity stays offline-first. The local Demo must still launch, complete the
mission and open MechLab when the main server is disabled or unavailable. The
server is only allowed to sign a squad before launch, accept a debrief-derived
claim after battle, and return the resulting token ledger, inventory snapshot
and leaderboard row.

The mobile first version remains landscape-only. F15 does not introduce portrait
layout work, portrait screenshots or phone portrait acceptance criteria; the
first phone version is landscape-only.

## Receipt Flow

1. Unity launches from a local or server-projected inventory.
2. If the optional server adapter is enabled, Unity asks the main server to sign
   the selected squad before launch.
3. BattleCore runs locally. The server does not own movement, targeting, damage,
   AI commander actions, camera, UI or frame-loop state.
4. The debrief summary becomes the only first-slice receipt source.
5. Unity submits a reward claim with account id, signed squad id, map id/version,
   BattleCore version, battle summary hash, idempotency key and result summary.
6. The main server validates the claim boundary, applies deterministic reward
   rules and caps, writes one token ledger entry, returns the updated inventory
   snapshot and publishes one approved leaderboard projection.
7. Duplicate claims with the same idempotency key return the same ledger entry
   and do not apply a second token grant.
8. Rejected claims do not mutate inventory, token ledger or leaderboard state.

## Authority Boundary

| Area | First-slice authority | Notes |
| --- | --- | --- |
| Tactical simulation | Unity BattleCore | No server frame-loop calls. |
| Visual state | Unity presentation | No remote rendering or UI authority. |
| Squad eligibility | Main server when enabled | Signed squad required for server-backed grants. |
| Reward claim | Main server | Claims are idempotent and validated after debrief. |
| Token balance | Main server | Token ledger is the authoritative mutation log. |
| Inventory snapshot | Main server | Snapshot returned after approved reward grants. |
| Leaderboard | Main server | Rows are projections from approved reward claims. |
| Map server | Not first slice | A map server cannot grant rewards directly. |

## Implementation Slice

Use the existing local main-server prototype as the anchor:

- keep `GET /healthz` and `GET /version`;
- keep `POST /dev/accounts` for local fixture account setup;
- keep `GET /accounts/{accountId}/inventory` for bootstrap and MechBay preview;
- keep `POST /squads/sign` as the pre-launch authority check;
- keep `POST /reward-claims` as the post-debrief mutation endpoint;
- keep `GET /leaderboards/basic` as the first public score projection;
- keep `POST /dev/reset` for deterministic smoke tests.

F16 should add a focused evidence gate that starts the local main server, signs a
squad, submits one approved claim, submits the same claim again, proves the
duplicate response returns the same ledger entry, verifies the inventory balance
only changes once, and verifies the leaderboard row exists. It should emit
ignored evidence under `analysis-output/` and should not launch Unity unless a
later slice explicitly asks for it.

## Explicit Non-Goals

F15 and the next F16 evidence slice do not include:

- payment, recharge, cash-out or external token withdrawal;
- realtime PVP;
- chain integration, NFT minting or on-chain rewards;
- public map server registration;
- map-author reward payout;
- anti-cheat replay adjudication;
- portrait mobile UI;
- any required remote server dependency for local Windows or Android smoke.

## Verification

Run from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_server_backed_receipt_slice_plan.ps1 -RepoRoot .
```

Expected success string:

```text
Server-backed receipt slice plan check OK.
```
