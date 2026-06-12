# Platform Ecosystem Plan

Goal: evolve the current local Unity demo into a platform where the core fun
remains mech fitting plus tactical map combat, while community or partner map
servers can host battles and the main server controls portable rewards,
inventory, rankings, and long-term economy.

This is a long-term product architecture note. It should not distract from the
near-term Windows playable demo. The immediate priority remains battle feel,
mech lab quality, mission flow, and content-pack safety.

## Product Shape

The platform should support:

- players bringing their own mech squad to different maps
- official, partner, or community map servers hosting battle sessions
- map editors and map packages that can be shared, tested, and later certified
- rewards earned on maps becoming portable only after main-server validation
- a web site for rankings, battle records, map popularity, and creator stats
- later mech skin, weapon skin, and map customization
- optional blockchain-based revenue sharing or ownership proofs after the game
  economy is stable

The two product pillars remain:

1. Mech Lab: collect, repair, fit, tune, and skin mechs and weapons.
2. Map Combat: deploy a 1-6 mech squad into tactical missions and bring back
   certified rewards.

## High-Level Architecture

```text
Unity client
  -> main server: account, inventory, signed squad loadout, reward claim
  -> map server: room/session, map runtime, battle replay submission
  -> web site: rankings, public profiles, map pages

Map editor
  -> map package: terrain, triggers, enemy waves, objectives, metadata
  -> main server: upload, scan, certify, publish

Main server
  -> battle validator: deterministic replay/summary checks using BattleCore
  -> economy ledger: tokens, fragments, weapons, skins, creator revenue
  -> map registry: trusted maps, limits, reward tables, version history

Optional chain layer
  -> creator revenue proofs, skins/collectibles, transparent pools
```

## Trust Boundary

The key rule is:

**Maps can be open; rewards must be certified.**

Map servers may:

- host rooms and battle sessions
- run certified or uncertified map packages
- collect battle telemetry and replays
- submit result claims to the main server
- provide spectator or event-specific presentation

Map servers must not:

- mint portable rewards
- mutate player inventory
- decide final token, fragment, weapon, or skin grants
- bypass certified map limits
- change player-owned mech or weapon definitions

The main server owns:

- player identity and account state
- mech, weapon, pilot, skin, and token inventory
- signed mission entry loadouts
- map certification status
- reward tables and daily/event limits
- replay or summary validation
- rankings and public results
- creator revenue accounting

## Battle Validation Model

Near term, use a pragmatic validation ladder:

1. Local-only demo: no remote validation.
2. Trusted official server: submit battle summary and signed session ids.
3. Certified partner server: submit battle summary plus deterministic seed,
   squad loadout hash, map hash, and event timeline digest.
4. High-value reward maps: require replay upload and server-side BattleCore
   validation, with anomaly detection and sample full replays.
5. Competitive or prize events: require stricter replay retention, anti-tamper
   client checks, and server-authoritative session rules.

The validator should reuse BattleCore contracts rather than Unity presentation
objects. Unity remains a client and visualization layer.

## Map Package Model

Map packages should be content packs or sub-packs with:

- map id, version, title, author, license, and provenance
- terrain data and navigation metadata
- objectives, trigger graph, enemy waves, structures, turrets, environment props
- allowed squad sizes and expected difficulty
- reward table references, not direct reward definitions
- BattleCore compatibility version
- optional visual/audio references

Detailed map authoring contract:

- `docs-map-authoring-contract-2026-06-07.md`

That contract defines required package fields, trigger graph rules, rejected
direct-reward fields, validator failures, certification states and the first
local editor loop. It keeps the product rule explicit:

**Maps can be open; rewards must be certified.**

Certification states:

- Draft: local editor and private testing only.
- Uncertified Public: playable for fun, no portable rewards.
- Certified: eligible for portable rewards under main-server limits.
- Event: curated reward pool, ranking season, or partner campaign.
- Retired: preserved for history, disabled for new reward runs.

## Reward and Economy Rules

Portable rewards should be main-server-issued only:

- tokens
- mech fragments
- ordinary weapons
- event fragments
- skin materials
- cosmetic drops
- ranking points

Map output should be a claim, not a grant:

```text
player account + signed squad + map version + session id + result summary
  -> validation
  -> reward calculation
  -> account ledger mutation
```

Detailed reward authority contract:

- `docs-platform-reward-contract-2026-06-07.md`

That contract defines session tickets, reward claims, validation gates, claim
states, grant calculation, idempotent ledger rules, rejection/capping examples,
and ranking publication boundaries. It is intentionally a contract document,
not a server implementation.

Anti-abuse controls:

- per-map reward caps
- daily account caps
- difficulty-adjusted reward ceilings
- suspicious clear-time and damage-pattern detection
- map-server reputation
- replay sampling
- map version freeze during ranking periods

## Creator and Partner Model

Partner or community creators can contribute:

- map packages
- visual skins
- event campaigns
- server hosting capacity
- curated challenge ladders

Possible rewards:

- featured placement
- in-game token grants
- revenue share for certified or event maps
- skin sales share
- seasonal prize pools

Main-server accounting should be centralized first. Public or chain-based
settlement can be added only after fraud, refund, and moderation rules are
understood.

## Blockchain Position

Do not put core combat, mech stats, or normal inventory on-chain in the first
platform version.

Use Ethereum or an L2 later for:

- creator revenue proof or settlement
- optional cosmetic ownership
- transparent event prize pools
- limited commemorative skins or badges
- public audit trails for certified creator payouts

Avoid early chain coupling for:

- mech balance
- weapon balance
- battle outcomes
- repair costs
- normal token ledger
- anti-cheat-sensitive state

Reason: game balance, rollback, bans, fraud response, and customer support need
fast operational control while the economy is still changing.

## Suggested Service Split

Start as a modular server, not many microservices.

Initial modules:

- Auth and account
- Inventory and token ledger
- Map registry and certification
- Session ticket/signature service
- Reward claim validation
- Ranking and public profile API
- Admin/moderation tools

Split into separate services only when scale or team boundaries require it.

## Web Site Scope

First web version:

- player profiles
- top squads and mech builds
- map rankings
- seasonal leaderboards
- recent certified clears
- map author pages
- reward and event announcements

Later:

- replay viewer
- map upload and certification dashboard
- creator revenue dashboard
- skin marketplace or gallery

## Roadmap

Phase 0: Local demo

- finish Windows playable loop
- improve battle feel and section damage
- improve Mech Lab
- preserve content-pack boundaries

Phase 1: Main-server prototype

- account id
- inventory snapshot
- token ledger
- signed squad loadout
- simple reward claim endpoint
- basic leaderboard

Phase 2: Map package and editor loop

- map package schema
- local editor export
- uncertified map play
- map upload and metadata scan
- no portable rewards yet

Phase 3: Certified reward maps

- map certification states
- session tickets
- result claim validation
- reward caps and fraud checks
- web leaderboard and map pages

Phase 4: Partner/community servers

- public map server protocol
- server registration and reputation
- replay or digest upload
- event map support
- creator attribution

Phase 5: Creator economy and optional chain layer

- revenue-share accounting
- creator dashboards
- cosmetic drops or skin ownership
- optional Ethereum/L2 settlement for creator payouts or event pools

## Key Risks

- Cheating: third-party map servers can fabricate results unless portable
  rewards are main-server certified.
- Economy inflation: UGC maps can farm too efficiently unless reward caps and
  certification limits exist.
- Legal/content risk: original MC2 assets must remain private reference-only;
  public maps need project-owned or licensed assets.
- Moderation: user maps and skins need reporting, review, and takedown tools.
- Blockchain complexity: chain integration can freeze bad early economic
  decisions if introduced too soon.
- Scope creep: platform work should wait until the local battle and Mech Lab
  experience is strong enough to attract players.

## Architecture Decisions

Decision 1: Main server owns portable rewards.

- Status: Proposed.
- Consequence: third-party servers are easier to open, but reward claims need
  validation infrastructure.

Decision 2: BattleCore stays deterministic and validation-friendly.

- Status: Proposed.
- Consequence: replay and summary validation become practical, but presentation
  code must not own battle truth.

Decision 3: Blockchain is optional and late.

- Status: Proposed.
- Consequence: the project can use chain benefits for creator economics later
  without blocking the playable game or early economy tuning.

Decision 4: Map packages use certification tiers.

- Status: Proposed.
- Consequence: creators can iterate freely while the platform protects portable
  rewards and rankings.
