# Server Implementation Boundary

Status: Done for the first platform contract pass.

Date: 2026-06-12

## Stable Markers

```text
ServerImplementationBoundary: True
ModularMainServerFirst: True
NoMicroservicesFirst: True
UnityDemoOfflineFirst: True
MapServerOutOfFirstSlice: True
NoPaymentsMarketplaceChainRealtimePvpInFirstServer: True
BattleCoreValidationContractOnly: True
FirstServerTarget: Local main-server prototype before remote platform dependency
```

## Product Rule

The first server implementation should be a small local main-server prototype,
not the full online platform. It should prove the authority boundaries for
account, inventory, token ledger, signed squad loadout, reward claims and basic
leaderboard data while keeping the current Unity demo runnable without any
remote service.

The server is allowed to prepare contracts for future map servers. It must not
make the first Unity demo depend on an external server to start, load
`mc2_01`, run BattleCore, complete visible-flow smoke, or open MechLab.

## First Server Slice

The first server slice may include:

- account id;
- token ledger;
- inventory snapshot;
- signed squad loadout;
- reward claim endpoint;
- basic leaderboard;
- local admin reset for development;
- health and version endpoint.

The first slice should run locally, use deterministic test fixtures, and stay
small enough to validate in CI or a single developer command.

## Explicit Exclusions

Do not include these in the first server implementation:

- payment;
- recharge;
- cash-out;
- marketplace;
- realtime PVP;
- chain integration;
- NFT minting;
- public map server registration;
- full moderation dashboard;
- creator payout execution;
- anti-cheat model training;
- remote server dependency for the Unity demo.

Those features need later product, security, legal and operations work.

## Suggested Local Modules

Start as a modular server, not many microservices:

| Module | First Responsibility |
| --- | --- |
| Account | local account id and public player id |
| Inventory | mech, weapon, pilot, skin and fragment snapshot |
| TokenLedger | idempotent token changes and balance view |
| SquadSigning | signed squad loadout hash and expiry |
| RewardClaims | submit result claim and return accepted/capped/rejected |
| Leaderboard | basic approved claim ranking rows |
| AdminDev | local reset, seed fixtures and audit dump |

Split services only after the contracts have real load, team ownership or
deployment pressure.

## Data Contracts

Minimum records:

- `AccountRecord`
- `PublicProfileRecord`
- `InventorySnapshot`
- `TokenLedgerEntry`
- `SignedSquadLoadout`
- `RewardClaim`
- `RewardGrant`
- `LeaderboardRow`
- `AuditEvent`

Each mutation should have an idempotency key. Reward claims should reference
map id/version, signed squad id, battle summary hash, BattleCore version and
claim source.

## API Boundary

First local endpoints can be narrow:

- `GET /healthz`
- `GET /version`
- `POST /dev/accounts`
- `GET /accounts/{accountId}/inventory`
- `POST /squads/sign`
- `POST /reward-claims`
- `GET /leaderboards/basic`
- `POST /dev/reset`

No payment, marketplace, chain, Web login, public map upload or realtime
session endpoints are required for the first server slice.

## Unity Client Boundary

Unity may later call the main server for signed loadouts and reward claims, but
the current demo must remain offline-first:

1. Validator still runs without server.
2. Windows and Android smoke still run without server.
3. MechLab can use local fixtures.
4. Reward claim submission can be simulated by a local script or fixture until
   the server prototype exists.
5. Server failure must not block local BattleCore development.

## BattleCore Boundary

The server should consume BattleCore summaries or replay validation contracts.
It should not duplicate Unity presentation behavior or own visual state.

First validation scope:

- verify known BattleCore version;
- verify signed squad loadout hash;
- verify map id/version;
- verify result summary shape;
- apply reward table rules and caps;
- publish approved leaderboard rows.

Full replay validation and anti-cheat scoring can be later slices.

## First Implementation Target

The next implementation task can scaffold a local main-server prototype with:

1. local process start command;
2. health/version route;
3. fixture account and inventory;
4. idempotent token ledger entry;
5. signed squad loadout response;
6. reward claim acceptance fixture;
7. basic leaderboard output;
8. smoke script that proves the server can start and answer without touching
   Unity build artifacts.

## Verification

Run from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_server_implementation_boundary.ps1 -RepoRoot .
```

Expected success string:

```text
Server implementation boundary check OK.
```
