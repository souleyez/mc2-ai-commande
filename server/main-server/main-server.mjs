import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { createServer as createHttpServer } from "node:http";
import { fileURLToPath } from "node:url";

export const SERVICE_NAME = "mc2-main-server-local";
export const SERVER_VERSION = "0.1.0-local";
export const BATTLE_CORE_VERSION = "mc2-unity-demo-contract-v1";
export const REWARD_RULES_VERSION = "local-reward-rules-v1";
export const UNITY_OFFLINE_FIRST = true;
export const EXCLUDED_FIRST_SLICE_FEATURES = [
  "payment",
  "recharge",
  "cash-out",
  "marketplace",
  "realtime PVP",
  "chain integration",
  "NFT minting",
  "public map server registration",
  "remote server dependency for the Unity demo"
];

const DEFAULT_HOST = "127.0.0.1";
const DEFAULT_PORT = 8787;
const DEFAULT_SIGNED_SQUAD_EXPIRES_AT = "2026-06-30T00:00:00.000Z";
const fixtureUrl = new URL("./fixtures/local-dev-fixture.json", import.meta.url);

export function loadLocalFixture() {
  return JSON.parse(readFileSync(fixtureUrl, "utf8"));
}

export function createInitialState(fixture = loadLocalFixture()) {
  const account = deepClone(fixture.account);
  const profile = deepClone(fixture.publicProfile);
  const inventory = deepClone(fixture.inventory);

  return {
    fixture: deepClone(fixture),
    rewardRules: deepClone(fixture.rewardRules),
    accounts: new Map([[account.accountId, account]]),
    publicProfiles: new Map([[profile.publicPlayerId, profile]]),
    inventories: new Map([[account.accountId, inventory]]),
    tokenLedgers: new Map([[account.accountId, deepClone(fixture.tokenLedger ?? [])]]),
    signedLoadouts: new Map(),
    rewardClaims: new Map(),
    leaderboardRows: [],
    auditEvents: [
      {
        schema: "AuditEvent",
        auditEventId: "audit-seed-local-dev",
        actor: "AdminDev",
        action: "seed-fixture",
        targetId: account.accountId
      }
    ]
  };
}

export function createMainServer(options = {}) {
  const state = options.state ?? createInitialState(options.fixture);
  return createHttpServer((request, response) => {
    handleRequest(request, response, state).catch((error) => {
      writeJson(response, 500, {
        schema: "ErrorResponse",
        error: "internal_error",
        message: error instanceof Error ? error.message : String(error)
      });
    });
  });
}

async function handleRequest(request, response, state) {
  const url = new URL(request.url ?? "/", `http://${request.headers.host ?? "localhost"}`);
  const method = request.method ?? "GET";

  if (method === "GET" && url.pathname === "/healthz") {
    writeJson(response, 200, {
      schema: "HealthResponse",
      status: "ok",
      service: SERVICE_NAME,
      version: SERVER_VERSION,
      unityOfflineFirst: UNITY_OFFLINE_FIRST,
      localOnly: true
    });
    return;
  }

  if (method === "GET" && url.pathname === "/version") {
    writeJson(response, 200, {
      schema: "VersionResponse",
      service: SERVICE_NAME,
      version: SERVER_VERSION,
      battleCoreVersion: BATTLE_CORE_VERSION,
      rewardRulesVersion: REWARD_RULES_VERSION,
      unityOfflineFirst: UNITY_OFFLINE_FIRST,
      excludedFirstSliceFeatures: EXCLUDED_FIRST_SLICE_FEATURES
    });
    return;
  }

  if (method === "POST" && url.pathname === "/dev/reset") {
    resetState(state);
    writeJson(response, 200, {
      schema: "AdminDevResetResponse",
      status: "reset",
      service: SERVICE_NAME,
      accountId: state.fixture.account.accountId
    });
    return;
  }

  if (method === "POST" && url.pathname === "/dev/accounts") {
    const body = await readJsonBody(request);
    const account = getFixtureAccount(state, body);
    writeJson(response, 200, {
      schema: "DevAccountResponse",
      account,
      publicProfile: state.publicProfiles.get(account.publicPlayerId),
      tokenBalance: state.inventories.get(account.accountId)?.tokenBalance ?? 0
    });
    return;
  }

  const inventoryMatch = url.pathname.match(/^\/accounts\/([^/]+)\/inventory$/);
  if (method === "GET" && inventoryMatch) {
    const accountId = decodeURIComponent(inventoryMatch[1]);
    const inventory = state.inventories.get(accountId);
    if (!inventory) {
      writeError(response, 404, "account_not_found", `Unknown accountId: ${accountId}`);
      return;
    }

    writeJson(response, 200, {
      schema: "InventorySnapshotResponse",
      account: state.accounts.get(accountId),
      inventory
    });
    return;
  }

  if (method === "POST" && url.pathname === "/squads/sign") {
    const body = await readJsonBody(request);
    const result = signSquadLoadout(state, body);
    writeJson(response, 200, result);
    return;
  }

  if (method === "POST" && url.pathname === "/reward-claims") {
    const body = await readJsonBody(request);
    const result = acceptRewardClaim(state, body);
    writeJson(response, result.rewardClaim.status === "rejected" ? 400 : 200, result);
    return;
  }

  if (method === "GET" && url.pathname === "/leaderboards/basic") {
    const limit = clampInteger(Number(url.searchParams.get("limit") ?? 10), 1, 50);
    writeJson(response, 200, {
      schema: "BasicLeaderboardResponse",
      leaderboardId: "basic-local",
      rows: state.leaderboardRows.slice(0, limit)
    });
    return;
  }

  writeError(response, 404, "route_not_found", `${method} ${url.pathname}`);
}

function resetState(state) {
  const fresh = createInitialState(state.fixture);
  state.rewardRules = fresh.rewardRules;
  state.accounts = fresh.accounts;
  state.publicProfiles = fresh.publicProfiles;
  state.inventories = fresh.inventories;
  state.tokenLedgers = fresh.tokenLedgers;
  state.signedLoadouts = fresh.signedLoadouts;
  state.rewardClaims = fresh.rewardClaims;
  state.leaderboardRows = fresh.leaderboardRows;
  state.auditEvents = fresh.auditEvents;
}

function getFixtureAccount(state, body) {
  const fixtureAccount = state.fixture.account;
  const account = deepClone(state.accounts.get(fixtureAccount.accountId));
  if (body?.displayName && typeof body.displayName === "string") {
    account.displayName = body.displayName.slice(0, 48);
  }

  state.accounts.set(account.accountId, account);
  state.publicProfiles.set(account.publicPlayerId, {
    ...state.publicProfiles.get(account.publicPlayerId),
    displayName: account.displayName
  });
  appendAudit(state, "Account", "upsert-dev-account", account.accountId);
  return account;
}

function signSquadLoadout(state, body) {
  const accountId = requireString(body, "accountId");
  const account = state.accounts.get(accountId);
  if (!account) {
    return rejectedSignedSquad("account_not_found", accountId);
  }

  const inventory = state.inventories.get(accountId);
  const mapId = body.mapId || "mc2_01";
  const mapVersion = body.mapVersion || "local-fixture-v1";
  const battleCoreVersion = body.battleCoreVersion || BATTLE_CORE_VERSION;
  const requestedOwnedMechIds = Array.isArray(body.ownedMechIds)
    ? body.ownedMechIds.map(String)
    : [];
  const ownedMechIds = requestedOwnedMechIds.length > 0
    ? requestedOwnedMechIds
    : (inventory?.ownedMechs ?? [])
      .filter((mech) => mech.availableForMission)
      .slice(0, 6)
      .map((mech) => mech.ownedMechId);

  const missing = ownedMechIds.filter((ownedMechId) => !hasOwnedMech(inventory, ownedMechId));
  if (missing.length > 0 || ownedMechIds.length === 0 || ownedMechIds.length > 6) {
    return rejectedSignedSquad(
      "invalid_squad",
      missing.length > 0 ? `Missing ownedMechId: ${missing.join(", ")}` : "Squad size must be 1-6"
    );
  }

  const signedPayload = {
    accountId,
    publicPlayerId: account.publicPlayerId,
    mapId,
    mapVersion,
    battleCoreVersion,
    ownedMechIds,
    activeLoadoutIds: ownedMechIds.map((id) => findOwnedMech(inventory, id).activeLoadoutId)
  };
  const loadoutHash = sha256(stableStringify(signedPayload));
  const signedSquadId = `signed-squad-${loadoutHash.slice(0, 16)}`;
  const signedSquad = {
    schema: "SignedSquadLoadout",
    signedSquadId,
    accountId,
    publicPlayerId: account.publicPlayerId,
    mapId,
    mapVersion,
    battleCoreVersion,
    loadoutHash,
    signature: `local-dev.${loadoutHash}`,
    expiresAt: DEFAULT_SIGNED_SQUAD_EXPIRES_AT,
    unitCount: ownedMechIds.length,
    ownedMechIds
  };

  state.signedLoadouts.set(signedSquadId, signedSquad);
  appendAudit(state, "SquadSigning", "sign-squad-loadout", signedSquadId);
  return {
    schema: "SignedSquadLoadoutResponse",
    status: "signed",
    signedSquad
  };
}

function acceptRewardClaim(state, body) {
  const accountId = requireString(body, "accountId");
  const idempotencyKey = body.idempotencyKey || body.battleSummaryHash || "";
  if (!idempotencyKey) {
    return rejectedReward("missing_idempotency_key", "RewardClaim needs idempotencyKey or battleSummaryHash.");
  }

  if (state.rewardClaims.has(idempotencyKey)) {
    return deepClone(state.rewardClaims.get(idempotencyKey));
  }

  const account = state.accounts.get(accountId);
  if (!account) {
    return rejectedReward("account_not_found", accountId);
  }

  const signedSquadId = requireString(body, "signedSquadId");
  const signedSquad = state.signedLoadouts.get(signedSquadId);
  if (!signedSquad || signedSquad.accountId !== accountId) {
    return rejectedReward("signed_squad_not_found", signedSquadId);
  }

  const battleCoreVersion = body.battleCoreVersion || BATTLE_CORE_VERSION;
  if (battleCoreVersion !== signedSquad.battleCoreVersion) {
    return rejectedReward("battlecore_version_mismatch", battleCoreVersion);
  }

  const mapId = body.mapId || signedSquad.mapId;
  const mapVersion = body.mapVersion || signedSquad.mapVersion;
  if (mapId !== signedSquad.mapId || mapVersion !== signedSquad.mapVersion) {
    return rejectedReward("map_mismatch", `${mapId}@${mapVersion}`);
  }

  const resultSummary = body.resultSummary ?? {};
  const rawTokenDelta = calculateTokenDelta(state.rewardRules, resultSummary);
  const tokenDelta = Math.min(rawTokenDelta, state.rewardRules.maxTokenDeltaPerClaim);
  const inventory = state.inventories.get(accountId);
  inventory.tokenBalance += tokenDelta;
  syncCurrencyStack(inventory);

  const claimHash = sha256(stableStringify({
    accountId,
    signedSquadId,
    mapId,
    mapVersion,
    battleCoreVersion,
    battleSummaryHash: body.battleSummaryHash || "",
    resultSummary
  }));
  const claimId = `reward-claim-${claimHash.slice(0, 16)}`;
  const ledgerEntry = {
    schema: "TokenLedgerEntry",
    ledgerEntryId: `ledger-${claimHash.slice(0, 16)}`,
    accountId,
    idempotencyKey,
    delta: tokenDelta,
    balanceAfter: inventory.tokenBalance,
    reason: "reward-claim",
    source: {
      claimId,
      mapId,
      mapVersion
    }
  };
  state.tokenLedgers.get(accountId).push(ledgerEntry);

  const rewardGrant = {
    schema: "RewardGrant",
    grantId: `grant-${claimHash.slice(0, 16)}`,
    accountId,
    claimId,
    tokenDelta,
    capped: rawTokenDelta > tokenDelta,
    rewardRulesVersion: state.rewardRules.rewardRulesVersion
  };
  const rewardClaim = {
    schema: "RewardClaim",
    claimId,
    accountId,
    publicPlayerId: account.publicPlayerId,
    signedSquadId,
    mapId,
    mapVersion,
    battleCoreVersion,
    battleSummaryHash: body.battleSummaryHash || claimHash,
    status: "approved",
    claimSource: body.claimSource || "local-smoke"
  };
  const leaderboardRow = buildLeaderboardRow(state, rewardClaim, rewardGrant, resultSummary);
  state.leaderboardRows = [leaderboardRow, ...state.leaderboardRows]
    .sort((left, right) => right.score - left.score || left.publicPlayerId.localeCompare(right.publicPlayerId))
    .slice(0, 50);

  const response = {
    schema: "RewardClaimResponse",
    rewardClaim,
    rewardGrant,
    tokenLedgerEntry: ledgerEntry,
    inventorySnapshot: inventory,
    leaderboardRow
  };
  state.rewardClaims.set(idempotencyKey, deepClone(response));
  appendAudit(state, "RewardClaims", "accept-reward-claim", claimId);
  return response;
}

function calculateTokenDelta(rewardRules, resultSummary) {
  const objectiveReward = clampInteger(
    Number(resultSummary.completedRewardResourcePoints ?? resultSummary.objectiveReward ?? 0),
    0,
    rewardRules.objectiveTokenCap
  );
  const damageReward = Math.floor(clampInteger(Number(resultSummary.causedDamageScore ?? 0), 0, 100000) / rewardRules.damageTokenDivisor);
  const debuffReward = clampInteger(Number(resultSummary.debuffSeconds ?? 0), 0, 1000) * rewardRules.debuffTokenPerSecond;
  const clearBonus = resultSummary.result === "success" ? 250 : 0;
  return objectiveReward + damageReward + debuffReward + clearBonus;
}

function buildLeaderboardRow(state, rewardClaim, rewardGrant, resultSummary) {
  const score = Math.min(
    state.rewardRules.leaderboardScoreCap,
    rewardGrant.tokenDelta
      + clampInteger(Number(resultSummary.objectivesCompleted ?? 0), 0, 20) * 250
      + clampInteger(Number(resultSummary.enemiesDestroyed ?? 0), 0, 100) * 100
      - clampInteger(Number(resultSummary.squadLosses ?? 0), 0, 6) * 150
  );

  return {
    schema: "LeaderboardRow",
    leaderboardId: "basic-local",
    publicPlayerId: rewardClaim.publicPlayerId,
    displayName: state.publicProfiles.get(rewardClaim.publicPlayerId)?.displayName ?? rewardClaim.publicPlayerId,
    mapId: rewardClaim.mapId,
    mapVersion: rewardClaim.mapVersion,
    battleId: rewardClaim.battleSummaryHash,
    score,
    rewardClaimId: rewardClaim.claimId,
    resultState: "approved"
  };
}

function rejectedSignedSquad(code, message) {
  return {
    schema: "SignedSquadLoadoutResponse",
    status: "rejected",
    error: code,
    message
  };
}

function rejectedReward(code, message) {
  return {
    schema: "RewardClaimResponse",
    rewardClaim: {
      schema: "RewardClaim",
      status: "rejected",
      error: code,
      message
    }
  };
}

function appendAudit(state, actor, action, targetId) {
  const auditHash = sha256(`${actor}:${action}:${targetId}:${state.auditEvents.length}`);
  state.auditEvents.push({
    schema: "AuditEvent",
    auditEventId: `audit-${auditHash.slice(0, 16)}`,
    actor,
    action,
    targetId
  });
}

function hasOwnedMech(inventory, ownedMechId) {
  return Boolean(findOwnedMech(inventory, ownedMechId));
}

function findOwnedMech(inventory, ownedMechId) {
  return (inventory?.ownedMechs ?? []).find((mech) => mech.ownedMechId === ownedMechId);
}

function syncCurrencyStack(inventory) {
  const stack = (inventory.itemStacks ?? []).find((item) => item.itemId === "token-credit");
  if (stack) {
    stack.quantity = inventory.tokenBalance;
  }
}

function requireString(body, fieldName) {
  const value = body?.[fieldName];
  if (typeof value !== "string" || value.trim() === "") {
    throw new Error(`Missing required string field: ${fieldName}`);
  }

  return value.trim();
}

async function readJsonBody(request) {
  const chunks = [];
  let size = 0;
  for await (const chunk of request) {
    size += chunk.length;
    if (size > 1024 * 1024) {
      throw new Error("Request body too large.");
    }

    chunks.push(chunk);
  }

  if (chunks.length === 0) {
    return {};
  }

  return JSON.parse(Buffer.concat(chunks).toString("utf8"));
}

function writeError(response, statusCode, error, message) {
  writeJson(response, statusCode, {
    schema: "ErrorResponse",
    error,
    message
  });
}

function writeJson(response, statusCode, body) {
  const payload = `${JSON.stringify(body, null, 2)}\n`;
  response.writeHead(statusCode, {
    "content-type": "application/json; charset=utf-8",
    "cache-control": "no-store"
  });
  response.end(payload);
}

function stableStringify(value) {
  if (value === null || typeof value !== "object") {
    return JSON.stringify(value);
  }

  if (Array.isArray(value)) {
    return `[${value.map((item) => stableStringify(item)).join(",")}]`;
  }

  return `{${Object.keys(value)
    .sort()
    .map((key) => `${JSON.stringify(key)}:${stableStringify(value[key])}`)
    .join(",")}}`;
}

function sha256(value) {
  return createHash("sha256").update(value).digest("hex");
}

function deepClone(value) {
  return JSON.parse(JSON.stringify(value));
}

function clampInteger(value, min, max) {
  if (!Number.isFinite(value)) {
    return min;
  }

  return Math.min(max, Math.max(min, Math.floor(value)));
}

if (process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1]) {
  const host = process.env.MC2_MAIN_SERVER_HOST || DEFAULT_HOST;
  const port = Number(process.env.MC2_MAIN_SERVER_PORT || DEFAULT_PORT);
  const server = createMainServer();
  server.listen(port, host, () => {
    const address = server.address();
    const resolvedPort = typeof address === "object" && address ? address.port : port;
    console.log(`${SERVICE_NAME} listening on http://${host}:${resolvedPort}`);
    console.log("UnityOfflineFirst: True");
    console.log("NoPaymentMarketplaceRealtimePvpChain: True");
  });
}
