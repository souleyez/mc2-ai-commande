import { createInitialState, createMainServer } from "./main-server.mjs";

const state = createInitialState();
const server = createMainServer({ state });

try {
  await listen(server);
  const address = server.address();
  const baseUrl = `http://127.0.0.1:${address.port}`;

  const reset = await request(baseUrl, "POST", "/dev/reset", {});
  assert(reset.status === "reset", "dev reset must reset local state");

  const health = await request(baseUrl, "GET", "/healthz");
  assert(health.status === "ok", "healthz must be ok");
  assert(health.unityOfflineFirst === true, "healthz must keep Unity offline-first");

  const version = await request(baseUrl, "GET", "/version");
  assert(version.version === "0.1.0-local", "version route must expose local version");
  assert(version.excludedFirstSliceFeatures.includes("payment"), "version must exclude payment");
  assert(version.excludedFirstSliceFeatures.includes("marketplace"), "version must exclude marketplace");
  assert(version.excludedFirstSliceFeatures.includes("realtime PVP"), "version must exclude realtime PVP");
  assert(version.excludedFirstSliceFeatures.includes("chain integration"), "version must exclude chain integration");

  const accountResponse = await request(baseUrl, "POST", "/dev/accounts", {
    idempotencyKey: "smoke-dev-account-v1",
    displayName: "Local Commander"
  });
  const accountId = accountResponse.account.accountId;
  assert(accountId === "local-dev-account", "dev account must be deterministic");

  const inventoryResponse = await request(baseUrl, "GET", `/accounts/${encodeURIComponent(accountId)}/inventory`);
  assert(inventoryResponse.inventory.tokenBalance === 12000, "fixture inventory token balance must be seeded");
  assert(inventoryResponse.inventory.ownedMechs.length >= 3, "fixture inventory must include mechs");

  const signedResponse = await request(baseUrl, "POST", "/squads/sign", {
    accountId,
    mapId: "mc2_01",
    mapVersion: "local-fixture-v1",
    battleCoreVersion: "mc2-unity-demo-contract-v1",
    ownedMechIds: ["demo-mech-01", "demo-mech-02", "demo-mech-03"]
  });
  assert(signedResponse.status === "signed", "squad signing must succeed");
  assert(signedResponse.signedSquad.unitCount === 3, "signed squad must include requested mechs");

  const rewardClaimBody = {
    accountId,
    idempotencyKey: "smoke-reward-claim-v1",
    signedSquadId: signedResponse.signedSquad.signedSquadId,
    mapId: "mc2_01",
    mapVersion: "local-fixture-v1",
    battleCoreVersion: "mc2-unity-demo-contract-v1",
    battleSummaryHash: "battle-summary-smoke-v1",
    claimSource: "local-smoke",
    resultSummary: {
      result: "success",
      completedRewardResourcePoints: 1800,
      causedDamageScore: 4000,
      debuffSeconds: 30,
      objectivesCompleted: 2,
      enemiesDestroyed: 4,
      squadLosses: 0
    }
  };
  const claimResponse = await request(baseUrl, "POST", "/reward-claims", rewardClaimBody);
  assert(claimResponse.rewardClaim.status === "approved", "reward claim must be approved");
  assert(claimResponse.rewardGrant.tokenDelta === 2310, "reward claim token grant must be deterministic");
  assert(claimResponse.inventorySnapshot.tokenBalance === 14310, "reward claim must update token balance once");

  const duplicateClaimResponse = await request(baseUrl, "POST", "/reward-claims", rewardClaimBody);
  assert(
    duplicateClaimResponse.tokenLedgerEntry.ledgerEntryId === claimResponse.tokenLedgerEntry.ledgerEntryId,
    "duplicate reward claim must return same ledger entry"
  );
  assert(
    duplicateClaimResponse.inventorySnapshot.tokenBalance === 14310,
    "duplicate reward claim must not double-spend ledger"
  );

  const leaderboard = await request(baseUrl, "GET", "/leaderboards/basic?limit=5");
  assert(leaderboard.rows.length === 1, "leaderboard must publish one approved row");
  assert(leaderboard.rows[0].resultState === "approved", "leaderboard row must be approved");

  console.log("Local main-server smoke OK.");
  console.log("Service: mc2-main-server-local");
  console.log("Endpoints: GET /healthz; GET /version; POST /dev/accounts; GET /accounts/{accountId}/inventory; POST /squads/sign; POST /reward-claims; GET /leaderboards/basic; POST /dev/reset");
  console.log("UnityOfflineFirst: True");
  console.log("NoRemoteUnityDependency: True");
  console.log("NoPaymentMarketplaceRealtimePvpChain: True");
  console.log("RewardClaimId:", claimResponse.rewardClaim.claimId);
  console.log("LeaderboardRows:", leaderboard.rows.length.toString());
}
finally {
  await close(server);
}

function listen(serverToListen) {
  return new Promise((resolve, reject) => {
    serverToListen.once("error", reject);
    serverToListen.listen(0, "127.0.0.1", () => {
      serverToListen.off("error", reject);
      resolve();
    });
  });
}

function close(serverToClose) {
  return new Promise((resolve, reject) => {
    serverToClose.close((error) => {
      if (error) {
        reject(error);
        return;
      }

      resolve();
    });
  });
}

async function request(baseUrl, method, path, body) {
  const response = await fetch(`${baseUrl}${path}`, {
    method,
    headers: body === undefined ? {} : { "content-type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body)
  });
  const payload = await response.json();
  if (!response.ok) {
    throw new Error(`${method} ${path} failed: ${response.status} ${JSON.stringify(payload)}`);
  }

  return payload;
}

function assert(condition, message) {
  if (!condition) {
    throw new Error(message);
  }
}
