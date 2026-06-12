# Creator Economy Boundary

Status: Done for the first platform contract pass.

Date: 2026-06-12

## Stable Markers

```text
CreatorEconomyBoundary: True
CentralizedLedgerFirst: True
CreatorRevenueAccountingBeforeChain: True
OptionalChainLayerLater: True
CoreGameplayOffChain: True
CombatStatsOffChain: True
NormalInventoryOffChain: True
RepairCostsOffChain: True
FirstCreatorTarget: Accounting contract before marketplace or chain implementation
```

## Product Rule

Creator economy work must support maps, skins, events and partner hosting
without turning the first playable game into a chain or marketplace project.
The main server remains the authority for token, inventory, reward and creator
revenue accounting.

The first creator economy pass is a boundary contract, not a marketplace,
payment system, smart contract, NFT system, or creator dashboard
implementation.

## Centralized Ledger First

The first server-side economy should keep these ledgers centralized:

- token ledger;
- inventory ledger;
- reward grant ledger;
- creator revenue accounting ledger;
- creator payout status ledger;
- refund, rollback and moderation adjustment ledger.

Each ledger entry should be idempotent, auditable and reversible by operation
tools before any optional public proof layer exists.

## Creator Contributions

Creators or partners may later contribute:

- certified map packages;
- visual mech skins;
- weapon skins;
- event campaigns;
- hosted map server capacity;
- curated challenge ladders;
- cosmetic badge or commemorative item designs.

Contribution records should include creator identity, public creator id,
content id, version, license/provenance, moderation state, certification state
and revenue policy id.

## Revenue Share Scope

Revenue share can exist before chain integration. The first accounting model
should support:

- map clear revenue share;
- skin sale revenue share;
- event prize pool allocation;
- featured creator pool allocation;
- partner-hosted server capacity bonus;
- admin adjustment, refund and rollback entries.

The total portable reward or sale revenue pool remains main-server calculated.
Map servers, Web pages and client builds can display approved summaries but
must not directly mutate creator revenue balances.

## Optional Chain Layer Later

Ethereum or an L2 can be considered only after centralized accounting,
moderation, refund and fraud handling are operational.

Suitable late chain uses:

- proof of revenue share;
- transparent creator pools;
- cosmetic ownership proof;
- commemorative items;
- public audit trail for certified creator payouts.

Do not use chain integration in the first platform version for:

- core combat;
- mech stats;
- weapon stats;
- pilot skills;
- repair costs;
- ordinary token ledger;
- ordinary inventory mutation;
- battle outcomes;
- anti-cheat-sensitive state;
- reward claim validation.

## Moderation and Rollback Boundary

Creator content must remain removable and adjustable:

- delist maps or skins after moderation failure;
- freeze revenue sharing during fraud review;
- cap abusive maps without exposing exact anti-abuse internals;
- refund purchases or event entries;
- reverse mistaken ledger entries with audit records;
- preserve public history only after privacy and legal review.

This is the reason chain is late: early economy decisions need operational
control.

## First Implementation Target

The first implementation after this contract should be a server boundary plan
or a small main-server prototype, not a marketplace:

1. Define account, inventory, token ledger and creator accounting tables.
2. Define reward claim and creator revenue event ids.
3. Define admin reversal and moderation states.
4. Define public summaries that Web can display safely.
5. Keep chain integration out of the first implementation.

## Verification

Run from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_creator_economy_boundary.ps1 -RepoRoot .
```

Expected success string:

```text
Creator economy boundary check OK.
```
