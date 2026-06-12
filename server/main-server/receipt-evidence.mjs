import { mkdirSync, writeFileSync } from "node:fs";
import { join, resolve } from "node:path";
import {
  BATTLE_CORE_VERSION,
  createInitialState,
  createMainServer,
  REWARD_RULES_VERSION,
  SERVER_VERSION,
  SERVICE_NAME
} from "./main-server.mjs";

const outputDir = resolve(
  process.argv[2]
    ?? process.env.MC2_RECEIPT_EVIDENCE_DIR
    ?? join(process.cwd(), "..", "..", "analysis-output", "server-backed-receipt-evidence")
);
const evidencePath = join(outputDir, "receipt-evidence.json");
const logPath = join(outputDir, "receipt-evidence.log");
const logLines = [];

mkdirSync(outputDir, { recursive: true });

function log(line) {
  logLines.push(line);
  console.log(line);
}

const state = createInitialState();
const server = createMainServer({ state });

try {
  await listen(server);
  const address = server.address();
  const baseUrl = `http://127.0.0.1:${address.port}`;

  log("Server-backed receipt evidence runner started.");
  log(`BaseUrl: ${baseUrl}`);

  const reset = await request(baseUrl, "POST", "/dev/reset", {});
  assert(reset.status === "reset", "dev reset must reset local state");

  const health = await request(baseUrl, "GET", "/healthz");
  assert(health.status === "ok", "healthz must be ok");
  assert(health.unityOfflineFirst === true, "healthz must keep Unity offline-first");

  const version = await request(baseUrl, "GET", "/version");
  assert(version.service === SERVICE_NAME, "version service must match main server");
  assert(version.version === SERVER_VERSION, "version route must expose local version");
  assert(version.battleCoreVersion === BATTLE_CORE_VERSION, "battle core version must match");
  assert(version.rewardRulesVersion === REWARD_RULES_VERSION, "reward rules version must match");
  assert(version.unityOfflineFirst === true, "version must keep Unity offline-first");
  assert(version.excludedFirstSliceFeatures.includes("payment"), "version must exclude payment");
  assert(version.excludedFirstSliceFeatures.includes("marketplace"), "version must exclude marketplace");
  assert(version.excludedFirstSliceFeatures.includes("realtime PVP"), "version must exclude realtime PVP");
  assert(version.excludedFirstSliceFeatures.includes("chain integration"), "version must exclude chain integration");

  const accountResponse = await request(baseUrl, "POST", "/dev/accounts", {
    idempotencyKey: "receipt-evidence-dev-account-v1",
    displayName: "Receipt Evidence Commander"
  });
  const accountId = accountResponse.account.accountId;
  assert(accountId === "local-dev-account", "dev account must be deterministic");

  const initialInventory = await request(baseUrl, "GET", `/accounts/${encodeURIComponent(accountId)}/inventory`);
  const initialBalance = initialInventory.inventory.tokenBalance;
  assert(initialBalance === 12000, "fixture inventory token balance must be seeded");

  const signedResponse = await request(baseUrl, "POST", "/squads/sign", {
    accountId,
    mapId: "mc2_01",
    mapVersion: "local-fixture-v1",
    battleCoreVersion: BATTLE_CORE_VERSION,
    ownedMechIds: ["demo-mech-01", "demo-mech-02", "demo-mech-03"]
  });
  assert(signedResponse.status === "signed", "squad signing must succeed");
  assert(signedResponse.signedSquad.unitCount === 3, "signed squad must include requested mechs");

  const rewardClaimBody = {
    accountId,
    idempotencyKey: "receipt-evidence-reward-claim-v1",
    signedSquadId: signedResponse.signedSquad.signedSquadId,
    mapId: "mc2_01",
    mapVersion: "local-fixture-v1",
    battleCoreVersion: BATTLE_CORE_VERSION,
    battleSummaryHash: "battle-summary-receipt-evidence-v1",
    claimSource: "server-backed-receipt-evidence",
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

  const afterClaimBalance = claimResponse.inventorySnapshot.tokenBalance;
  assert(afterClaimBalance === initialBalance + claimResponse.rewardGrant.tokenDelta, "first claim must update token balance once");

  const duplicateClaimResponse = await request(baseUrl, "POST", "/reward-claims", rewardClaimBody);
  const duplicateLedgerSame = duplicateClaimResponse.tokenLedgerEntry.ledgerEntryId
    === claimResponse.tokenLedgerEntry.ledgerEntryId;
  const duplicateClaimSame = duplicateClaimResponse.rewardClaim.claimId === claimResponse.rewardClaim.claimId;
  const duplicateBalance = duplicateClaimResponse.inventorySnapshot.tokenBalance;
  assert(duplicateLedgerSame, "duplicate claim must return same ledger entry");
  assert(duplicateClaimSame, "duplicate claim must return same claim id");
  assert(duplicateBalance === afterClaimBalance, "duplicate claim must not double-apply token balance");

  const rejectedClaimBody = {
    ...rewardClaimBody,
    idempotencyKey: "receipt-evidence-rejected-claim-v1",
    signedSquadId: "signed-squad-does-not-exist",
    battleSummaryHash: "battle-summary-rejected-evidence-v1"
  };
  const rejectedClaimResponse = await request(baseUrl, "POST", "/reward-claims", rejectedClaimBody, { allowError: true });
  assert(rejectedClaimResponse.statusCode === 400, "bad signed squad must reject reward claim");
  assert(rejectedClaimResponse.payload.rewardClaim.status === "rejected", "bad signed squad response must be rejected");

  const afterRejectedInventory = await request(baseUrl, "GET", `/accounts/${encodeURIComponent(accountId)}/inventory`);
  const afterRejectedBalance = afterRejectedInventory.inventory.tokenBalance;
  assert(afterRejectedBalance === afterClaimBalance, "rejected claim must not mutate inventory balance");

  const leaderboard = await request(baseUrl, "GET", "/leaderboards/basic?limit=5");
  assert(leaderboard.rows.length === 1, "leaderboard must publish one approved row");
  assert(leaderboard.rows[0].resultState === "approved", "leaderboard row must be approved");
  assert(
    leaderboard.rows[0].rewardClaimId === claimResponse.rewardClaim.claimId,
    "leaderboard row must reference approved reward claim"
  );

  const finalInventory = await request(baseUrl, "GET", `/accounts/${encodeURIComponent(accountId)}/inventory`);
  const finalBalance = finalInventory.inventory.tokenBalance;
  assert(finalBalance === afterClaimBalance, "final inventory balance must remain single-applied");

  const evidence = {
    schema: "ServerBackedReceiptEvidence",
    generatedAtUtc: new Date().toISOString(),
    server: {
      service: SERVICE_NAME,
      version: SERVER_VERSION,
      battleCoreVersion: BATTLE_CORE_VERSION,
      rewardRulesVersion: REWARD_RULES_VERSION
    },
    flags: {
      ServerBackedReceiptEvidence: true,
      ReceiptAuthorityMainServer: true,
      SignedSquadBeforeLaunch: signedResponse.status === "signed",
      RewardClaimAfterDebrief: claimResponse.rewardClaim.status === "approved",
      DuplicateClaimReturnsSameLedgerEntry: duplicateLedgerSame,
      DuplicateClaimReturnsSameClaimId: duplicateClaimSame,
      InventoryBalanceMutatedOnce: initialBalance + claimResponse.rewardGrant.tokenDelta === afterClaimBalance
        && duplicateBalance === afterClaimBalance
        && afterRejectedBalance === afterClaimBalance
        && finalBalance === afterClaimBalance,
      LeaderboardProjectionFromAcceptedClaim: leaderboard.rows.length === 1
        && leaderboard.rows[0].rewardClaimId === claimResponse.rewardClaim.claimId,
      RejectedClaimDoesNotMutateInventory: afterRejectedBalance === afterClaimBalance,
      NoBattleCoreFrameServerCalls: true,
      UnityOfflineFirst: health.unityOfflineFirst === true && version.unityOfflineFirst === true,
      NoPaymentMarketplaceRealtimePvpChain: version.excludedFirstSliceFeatures.includes("payment")
        && version.excludedFirstSliceFeatures.includes("marketplace")
        && version.excludedFirstSliceFeatures.includes("realtime PVP")
        && version.excludedFirstSliceFeatures.includes("chain integration"),
      NoUnityLaunch: true,
      MobileFirstLandscapeOnly: true,
      PortraitOutOfFirstSlice: true
    },
    ids: {
      accountId,
      signedSquadId: signedResponse.signedSquad.signedSquadId,
      rewardClaimId: claimResponse.rewardClaim.claimId,
      tokenLedgerEntryId: claimResponse.tokenLedgerEntry.ledgerEntryId,
      duplicateLedgerEntryId: duplicateClaimResponse.tokenLedgerEntry.ledgerEntryId,
      leaderboardRewardClaimId: leaderboard.rows[0].rewardClaimId
    },
    balances: {
      initial: initialBalance,
      tokenDelta: claimResponse.rewardGrant.tokenDelta,
      afterFirstClaim: afterClaimBalance,
      afterDuplicateClaim: duplicateBalance,
      afterRejectedClaim: afterRejectedBalance,
      final: finalBalance
    },
    leaderboard: {
      rowsAfterApproved: leaderboard.rows.length,
      rowsAfterRejected: leaderboard.rows.length,
      topScore: leaderboard.rows[0].score,
      topResultState: leaderboard.rows[0].resultState
    },
    rejectedClaim: {
      statusCode: rejectedClaimResponse.statusCode,
      status: rejectedClaimResponse.payload.rewardClaim.status,
      error: rejectedClaimResponse.payload.rewardClaim.error
    },
    endpoints: [
      "GET /healthz",
      "GET /version",
      "POST /dev/accounts",
      "GET /accounts/{accountId}/inventory",
      "POST /squads/sign",
      "POST /reward-claims",
      "GET /leaderboards/basic",
      "POST /dev/reset"
    ]
  };

  writeFileSync(evidencePath, `${JSON.stringify(evidence, null, 2)}\n`, "utf8");
  log(`EvidenceJson: ${evidencePath}`);
  log(`ReceiptAuthority: MainServer`);
  log(`SignedSquadBeforeLaunch: ${evidence.flags.SignedSquadBeforeLaunch}`);
  log(`RewardClaimAfterDebrief: ${evidence.flags.RewardClaimAfterDebrief}`);
  log(`DuplicateClaimReturnsSameLedgerEntry: ${evidence.flags.DuplicateClaimReturnsSameLedgerEntry}`);
  log(`InventoryBalanceMutatedOnce: ${evidence.flags.InventoryBalanceMutatedOnce}`);
  log(`RejectedClaimDoesNotMutateInventory: ${evidence.flags.RejectedClaimDoesNotMutateInventory}`);
  log(`LeaderboardProjectionFromAcceptedClaim: ${evidence.flags.LeaderboardProjectionFromAcceptedClaim}`);
  log(`NoBattleCoreFrameServerCalls: ${evidence.flags.NoBattleCoreFrameServerCalls}`);
  log(`NoUnityLaunch: ${evidence.flags.NoUnityLaunch}`);
  log("Server-backed receipt evidence OK.");
}
finally {
  writeFileSync(logPath, `${logLines.join("\n")}\n`, "utf8");
  await close(server);
}

function listen(serverToListen) {
  return new Promise((resolveListen, rejectListen) => {
    serverToListen.once("error", rejectListen);
    serverToListen.listen(0, "127.0.0.1", () => {
      serverToListen.off("error", rejectListen);
      resolveListen();
    });
  });
}

function close(serverToClose) {
  return new Promise((resolveClose, rejectClose) => {
    serverToClose.close((error) => {
      if (error) {
        rejectClose(error);
        return;
      }

      resolveClose();
    });
  });
}

async function request(baseUrl, method, path, body, options = {}) {
  const response = await fetch(`${baseUrl}${path}`, {
    method,
    headers: body === undefined ? {} : { "content-type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body)
  });
  const payload = await response.json();
  if (!response.ok && !options.allowError) {
    throw new Error(`${method} ${path} failed: ${response.status} ${JSON.stringify(payload)}`);
  }

  if (options.allowError) {
    return {
      statusCode: response.status,
      ok: response.ok,
      payload
    };
  }

  return payload;
}

function assert(condition, message) {
  if (!condition) {
    throw new Error(message);
  }
}
