# Web Ranking Contract

Status: Done for the first platform contract pass.

Date: 2026-06-12

## Stable Markers

```text
WebRankingContract: True
PublishedResultsCertifiedOnly: True
PublicProfilesPrivacySafe: True
NoPrivateAccountIds: True
NoApiKeys: True
NoUnpublishedInventory: True
NoAntiCheatInternals: True
RankingReadsApprovedClaimsOnly: True
FirstWebTarget: Static ranking and profile contract before server implementation
```

## Product Rule

The Web site presents certified achievements, not raw account or anti-cheat
state. Rankings and battle records are public storytelling and discovery
surfaces backed by main-server-approved result rows.

The Web/ranking service may publish:

- season leaderboard rows;
- map ranking rows;
- player public profiles;
- squad loadout snapshots;
- battle record details;
- creator or map author pages;
- event pages and certified clear summaries.

The Web/ranking service must not publish:

- private account ids;
- email, phone, OAuth, device or payment identifiers;
- API keys, session tokens or signing secrets;
- unpublished inventory or warehouse contents;
- rejected reward claims with exploit details;
- exact anti-cheat thresholds, heuristics or rule internals;
- raw replay files before moderation policy exists.

## Ranking Sources

Ranking rows must come from approved main-server records:

| Source | Allowed Use |
| --- | --- |
| approved reward claim | battle record, reward summary, ranking points |
| certified map registry | map title, version, author, certification state |
| public player profile | display name, avatar, opt-in squad snapshots |
| season/event registry | leaderboard windows and rule labels |
| creator registry | public author profile and featured maps |

Rejected, pending, suspicious or admin-only claims can affect internal
moderation queues but should not appear as public rankings.

## Public Pages

### Season Leaderboard

Fields:

- `seasonId`
- `rank`
- `publicPlayerId`
- `displayName`
- `score`
- `rankingPoints`
- `clears`
- `bestClearTime`
- `featuredSquadSnapshotId`
- `updatedAt`

### Map Ranking

Fields:

- `mapId`
- `mapVersion`
- `mapTitle`
- `authorPublicId`
- `certificationState`
- `clearCount`
- `averageClearTime`
- `bestClearTime`
- `topSquadSnapshotId`
- `rewardTableId`
- `rankingCategory`

### Player Public Profile

Fields:

- `publicPlayerId`
- `displayName`
- `avatarId`
- `joinedSeason`
- `publicBadges`
- `featuredSquads`
- `recentCertifiedClears`
- `favoriteMaps`
- `optInPilotFriendCode` if social pilot features are enabled later

### Squad Loadout Snapshot

Fields:

- `snapshotId`
- `publicPlayerId`
- `createdAt`
- `mechs`
- `weapons`
- `armorHardnessSummary`
- `heatBudget`
- `weightBudget`
- `pilotPublicNames`
- `battleCoreVersion`

Snapshots must be immutable once published. They are public performance
artifacts, not live inventory.

### Battle Record Detail

Fields:

- `battleId`
- `mapId`
- `mapVersion`
- `seasonId`
- `publicPlayerId`
- `squadSnapshotId`
- `result`
- `duration`
- `objectivesCompleted`
- `damageDealt`
- `damageTaken`
- `mechsLostOrDisabled`
- `pilotEscapeSummary`
- `rewardSummary`
- `rankingPoints`
- `claimStatus`
- `validatorSummary`

`validatorSummary` should be public-safe, for example `approved`,
`capped`, `practiceOnly` or `eventEligible`. It must not expose exact
anti-cheat internals.

### Creator Or Map Author Page

Fields:

- `authorPublicId`
- `displayName`
- `verifiedPartner`
- `publishedMaps`
- `certifiedMaps`
- `featuredEvents`
- `aggregatePlayCount`
- `aggregateCertifiedClears`

Creator revenue data belongs to a later F4 contract and should not be
published here except for public badges or opt-in aggregate labels.

## Privacy Boundary

Use stable public ids instead of internal account ids. The Web contract should
support account deletion and profile hiding later by separating public profile
records from core inventory and ledger records.

Never expose:

- internal account id;
- email, phone, OAuth subject or payment identifiers;
- precise login/device/IP history;
- unpublished mech, weapon, pilot or token inventory;
- unmoderated replay uploads;
- raw result claim payloads;
- private map package assets;
- anti-cheat thresholds or suspicious-pattern details.

## Anti-Abuse Boundary

Public pages can display a result as approved, capped, practice-only or
unranked. They should not explain the exact detection rule that caused a cap or
rejection. Internal moderation tools can see full details; public Web cannot.

Recommended public result states:

- `approved`
- `approvedCapped`
- `practiceOnly`
- `unranked`
- `removed`

## First Implementation Target

The first Web implementation can be static or read-only:

1. Publish a season leaderboard from approved claim rows.
2. Publish map ranking pages for certified maps.
3. Publish player public profiles with opt-in squad snapshots.
4. Publish battle record details for approved claims.
5. Keep all data sourced from main-server-approved rows.

No account login, payment, inventory mutation, moderation workflow or creator
revenue dashboard is required for F3.

## Verification

Run from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_web_ranking_contract.ps1 -RepoRoot .
```

Expected:

```text
Web ranking contract check OK.
```
