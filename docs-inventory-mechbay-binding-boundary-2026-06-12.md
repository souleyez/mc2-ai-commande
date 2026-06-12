# Inventory To MechBay Binding Boundary

Date: 2026-06-12

Status: F11 boundary plan. This is a documentation and gate step only. It does
not bind remote inventory into the default Unity demo.

Stable markers:

- InventoryMechBayBindingBoundary: True
- InventoryMechBayBindingOptional: True
- InventoryToMechBayMapping: True
- MechBayOnlyProjection: True
- LocalFixtureDefault: True
- NoDefaultRemoteInventoryDependency: True
- OptInReversibleBinding: True
- NoBattleCoreFrameInventoryServerCalls: True
- ServerInventoryNotCombatAuthority: True
- MobileLandscapeOnly: True
- NoLoginPaymentMarketplaceRealtimePvpChain: True
- F12RecommendedNext: Implement opt-in inventory-to-MechBay preview binding

## Purpose

F10 proved that Unity can explicitly fetch the local main-server dev account and
inventory snapshot. F11 defines how that snapshot may later become a MechLab /
garage preview without making the game depend on a server by default.

The first implementation after this plan should be an opt-in preview binding:
server inventory can populate MechBay UI state for a smoke test, but local
fixtures remain the default source for validator, Windows smoke, Android smoke,
MechLab editing, BattleCore frames and normal controlled demo evidence.

## Source And Target Contracts

Source server endpoint:

```http
POST /dev/accounts
GET /accounts/{accountId}/inventory
```

Source Unity DTOs:

- `UnityInventoryBootstrap`
- `UnityAccountRecord`
- `UnityPublicProfileRecord`
- `UnityInventorySnapshot`
- `UnityOwnedMechRecord`
- `UnityItemStackRecord`
- `OfflineFixtureFallback`

Target MechBay DTOs:

- `MechBayInventoryContract`
- `MechBayOwnedMechDefinition`
- `MechBayItemStackDefinition`
- `MechBayInventoryValidator.Schema`
- `MechBayOwnedRosterService`
- `MechBayInventoryAvailabilityResult`
- `MechBayMissionHandoffPreview`

The binding is a projection. It may create a `MechBayInventoryContract` from a
server snapshot only after validation succeeds. It must not replace
`LoadoutContract`, `MissionContract`, `UnitState`, `CombatProfile`, movement,
damage, heat, pathing, targeting or mission trigger authority.

## Field Mapping

Account fields are not combat fields.

| Server field | MechBay use | Rule |
| --- | --- | --- |
| `AccountRecord.accountId` | display/debug source id | Required for opt-in smoke, not shown as login UI. |
| `AccountRecord.publicPlayerId` | public-safe profile label | Optional display metadata only. |
| `AccountRecord.displayName` | MechBay header subtitle | Optional display metadata only. |

Inventory snapshot root:

| Server field | MechBay field | Rule |
| --- | --- | --- |
| `InventorySnapshot.tokenBalance` | `MechBayInventoryContract.tokenBalance` | Can drive local MechBay affordability preview in opt-in mode. |
| `InventorySnapshot.ownedMechs` | `MechBayInventoryContract.ownedMechs` | Must validate as a complete local roster before use. |
| `InventorySnapshot.itemStacks` | `MechBayInventoryContract.itemStacks` | Must validate quantities and recognized categories before use. |

Owned mech mapping:

| Server field | MechBay field | Rule |
| --- | --- | --- |
| `ownedMechId` | `ownedMechId` | Required stable inventory identity. |
| `unitId` | `unitId` | Optional; if missing, F12 may synthesize a local preview id. |
| `unitType` | `unitType` | Required directly or through a trusted local chassis catalog lookup. |
| `chassisId` | `chassisId` | Required directly or through a trusted local chassis catalog lookup. |
| `displayName` | `displayName` | Required directly or derived from chassis plus owned id. |
| `activeLoadoutId` | `activeLoadoutId` | Allowed only when the id exists in local loadout data for the chassis. |
| `availableForMission` | `availableForMission` | Can gate launch readiness in MechBay preview. |
| `conditionPercent` | `conditionPercent` | Must clamp/validate 0 through 100; invalid snapshot falls back. |
| `pilotId` | `pilotId` | Optional unless any pilot field is present. |
| `pilotDisplayName` | `pilotDisplayName` | Optional unless any pilot field is present. |
| `pilotType` | `pilotType` | Required when pilot id or pilot display name is present. |

Item stack mapping:

| Server field | MechBay field | Rule |
| --- | --- | --- |
| `itemId` | `itemId` | Required stable item identity. |
| `displayName` | `displayName` | Required for MechLab readability; derive only from a local catalog. |
| `category` | `category` | Must map to `Weapon`, `ArmorPlate`, `HeatSink` or `MechFragment`. |
| `quantity` | `quantity` | Must be zero or greater. |
| `equippedQuantity` | `equippedQuantity` | Must be zero through `quantity`; missing value defaults to zero only in preview mode. |

The server fixture currently uses lower-level category strings such as
`weapon`, `armor`, `heat-sink`, and `mech-fragment`. F12 must either normalize
those categories into `LoadoutItemCategory` values at the adapter boundary or
change the local dev fixture contract. The MechBay validator must receive only
recognized local category names.

## Validation Gate For Binding

Before Unity may present a server-backed MechBay preview, all of these must be
true:

1. `UnityInventoryBootstrap.fallback == null`.
2. `UnityInventoryBootstrap.account.accountId` is present.
3. `UnityInventorySnapshot.tokenBalance >= 0`.
4. The projected `MechBayInventoryContract.schema` equals
   `mc2-mech-bay-inventory-v1`.
5. `MechBayInventoryValidator.Validate(projectedInventory).IsValid == true`.
6. Every `activeLoadoutId` used by a mech resolves locally for its chassis.
7. Every item category resolves to a known local MechBay category.
8. The opt-in source label is visible in logs or sidecar metadata.

If any item fails, Unity must keep using the local fixture inventory and log the
fallback reason. It must not partially merge server mechs, token balance or item
stacks into local MechBay state.

## Fallback Rules

- FallbackServerUnavailable: keep `MechBayInventoryBuilder.BuildDemoInventory`.
- FallbackInvalidInventorySnapshot: keep local fixture and log validation errors.
- FallbackUnknownChassisOrLoadout: keep local fixture; do not create placeholder
  combat units.
- FallbackUnknownItemCategory: keep local fixture; do not hide unknown stacks.
- FallbackPilotIncomplete: keep local fixture; do not launch with half-assigned
  pilot metadata.
- FallbackServerPreviewRejected: MechBay remains usable with local fixtures.

All fallbacks are reversible. Closing the opt-in preview or restarting the demo
returns to local fixture data.

## UI Boundary

The first binding is not a new account screen, shop, marketplace or payment
flow. It may add only a compact source/status line in the existing MechBay path,
for example:

```text
Inventory Source: Local Fixture
Inventory Source: Main Server Preview
Inventory Source: Main Server Unavailable - Local Fixture
```

Mobile remains landscape-only. Any source/status line must fit the horizontal
phone MechLab layout without adding portrait-only navigation, modal login,
payment prompts, market tabs or server-debug overlays.

## Runtime Boundary

Allowed server call timing:

- Explicit command-file smoke before MechBay launch.
- Future explicit debug/preview command from MechBay.
- Future debrief reward claim after final battle result.

Forbidden call timing:

- `Update`, `FixedUpdate` or `LateUpdate`.
- BattleCore movement, firing, heat, damage, section loss, ejection, pathing,
  targeting, AI deputy decision or mission trigger evaluation.
- Android/Windows default visible-flow smoke.

Server inventory is not combat authority. BattleCore still receives local,
validated mission and loadout state.

## F12 Implementation Shape

Recommended next task:

```text
F12 implement opt-in inventory-to-MechBay preview binding
```

Expected F12 files:

- `UnityMainServerClient` may expand `UnityOwnedMechRecord` and
  `UnityItemStackRecord` to consume the full fixture fields.
- A small projection helper may convert `UnityInventorySnapshot` into
  `MechBayInventoryContract`.
- A command-file smoke may opt into the preview and assert source, token,
  roster, item stacks, validator success and fallback behavior.
- The default visible-flow command file remains offline-first.

F12 acceptance should prove both paths:

- Main-server preview path: server inventory projects into MechBay and validates.
- No-server path: local fixture remains playable and MechBay still passes the
  existing loadout compact assertion.

## Explicit Non-Goals

- No login.
- No payment, recharge, cash-out or marketplace.
- No realtime PVP.
- No chain, NFT, wallet or token withdrawal.
- No map-server upload path.
- No remote inventory dependency for default demo gates.
- No per-frame server calls.
