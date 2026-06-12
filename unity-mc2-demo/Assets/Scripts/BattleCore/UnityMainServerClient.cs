using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace MC2Demo.BattleCore
{
    [Serializable]
    public sealed class UnityServerSettings
    {
        public const bool DefaultEnabled = false;
        public const int DefaultTimeoutMilliseconds = 2500;
        public const string DefaultBaseUrl = "http://127.0.0.1:8787";

        public bool enabled = DefaultEnabled;
        public string baseUrl = DefaultBaseUrl;
        public int timeoutMilliseconds = DefaultTimeoutMilliseconds;

        public bool IsEnabled => enabled && !string.IsNullOrWhiteSpace(baseUrl);

        public static UnityServerSettings DisabledDefault()
        {
            return new UnityServerSettings
            {
                enabled = DefaultEnabled,
                baseUrl = DefaultBaseUrl,
                timeoutMilliseconds = DefaultTimeoutMilliseconds
            };
        }
    }

    [Serializable]
    public sealed class UnityServerStatus
    {
        public bool enabled;
        public bool reachable;
        public bool healthy;
        public bool unityOfflineFirst;
        public string service;
        public string version;
        public string battleCoreVersion;
        public string rewardRulesVersion;
        public string status;
        public string fallbackReason;
        public string message;

        public static UnityServerStatus Offline(string reason, string message)
        {
            return new UnityServerStatus
            {
                enabled = false,
                reachable = false,
                healthy = false,
                unityOfflineFirst = UnityMainServerClient.UnityOfflineFirst,
                status = "offline-fixture",
                fallbackReason = reason,
                message = message
            };
        }
    }

    [Serializable]
    public sealed class UnityInventoryBootstrap
    {
        public string schema;
        public UnityAccountRecord account;
        public UnityPublicProfileRecord publicProfile;
        public UnityInventorySnapshot inventory;
        public OfflineFixtureFallback fallback;
    }

    [Serializable]
    public sealed class UnityAccountRecord
    {
        public string accountId;
        public string publicPlayerId;
        public string displayName;
    }

    [Serializable]
    public sealed class UnityPublicProfileRecord
    {
        public string publicPlayerId;
        public string displayName;
    }

    [Serializable]
    public sealed class UnityInventorySnapshot
    {
        public string schema;
        public int tokenBalance;
        public UnityOwnedMechRecord[] ownedMechs;
        public UnityItemStackRecord[] itemStacks;
    }

    [Serializable]
    public sealed class UnityOwnedMechRecord
    {
        public string ownedMechId;
        public string activeLoadoutId;
        public bool availableForMission;
        public int conditionPercent;
        public string pilotId;
        public string pilotDisplayName;
    }

    [Serializable]
    public sealed class UnityItemStackRecord
    {
        public string itemId;
        public string displayName;
        public string category;
        public int quantity;
    }

    [Serializable]
    public sealed class UnitySquadSignRequest
    {
        public string accountId;
        public string mapId;
        public string mapVersion;
        public string battleCoreVersion;
        public string[] ownedMechIds;
    }

    [Serializable]
    public sealed class UnitySignedSquadResult
    {
        public string schema;
        public string status;
        public UnitySignedSquad signedSquad;
        public string error;
        public string message;
        public string clientStatus;
        public OfflineFixtureFallback fallback;

        public bool IsSigned => signedSquad != null && string.Equals(status, "signed", StringComparison.Ordinal);

        public static UnitySignedSquadResult UnsignedSquad(string reason, string message)
        {
            return new UnitySignedSquadResult
            {
                schema = "SignedSquadLoadoutResponse",
                status = "offline-fixture",
                clientStatus = "UnsignedSquad",
                message = message,
                fallback = OfflineFixtureFallback.ForUnsignedSquad(reason, message)
            };
        }
    }

    [Serializable]
    public sealed class UnitySignedSquad
    {
        public string schema;
        public string signedSquadId;
        public string accountId;
        public string publicPlayerId;
        public string mapId;
        public string mapVersion;
        public string battleCoreVersion;
        public string loadoutHash;
        public string signature;
        public string expiresAt;
        public int unitCount;
        public string[] ownedMechIds;
    }

    [Serializable]
    public sealed class UnityBattleResultClaim
    {
        public string accountId;
        public string idempotencyKey;
        public string signedSquadId;
        public string mapId;
        public string mapVersion;
        public string battleCoreVersion;
        public string battleSummaryHash;
        public string claimSource;
        public UnityBattleResultSummary resultSummary;
    }

    [Serializable]
    public sealed class UnityBattleResultSummary
    {
        public string result;
        public int completedRewardResourcePoints;
        public int causedDamageScore;
        public int debuffSeconds;
        public int objectivesCompleted;
        public int enemiesDestroyed;
        public int squadLosses;
    }

    [Serializable]
    public sealed class UnityRewardClaimResult
    {
        public string schema;
        public UnityRewardClaim rewardClaim;
        public UnityRewardGrant rewardGrant;
        public UnityTokenLedgerEntry tokenLedgerEntry;
        public UnityInventorySnapshot inventorySnapshot;
        public UnityLeaderboardRow leaderboardRow;
        public string clientStatus;
        public string message;
        public OfflineFixtureFallback fallback;

        public bool IsApproved => rewardClaim != null && string.Equals(rewardClaim.status, "approved", StringComparison.Ordinal);

        public static UnityRewardClaimResult RejectedClaim(string reason, string message)
        {
            return new UnityRewardClaimResult
            {
                schema = "RewardClaimResponse",
                clientStatus = "RejectedClaim",
                message = message,
                fallback = OfflineFixtureFallback.ForRejectedClaim(reason, message)
            };
        }
    }

    [Serializable]
    public sealed class UnityRewardClaim
    {
        public string schema;
        public string claimId;
        public string accountId;
        public string publicPlayerId;
        public string signedSquadId;
        public string mapId;
        public string mapVersion;
        public string battleCoreVersion;
        public string battleSummaryHash;
        public string status;
        public string claimSource;
        public string error;
        public string message;
    }

    [Serializable]
    public sealed class UnityRewardGrant
    {
        public string schema;
        public string grantId;
        public string accountId;
        public string claimId;
        public int tokenDelta;
        public bool capped;
        public string rewardRulesVersion;
    }

    [Serializable]
    public sealed class UnityTokenLedgerEntry
    {
        public string schema;
        public string ledgerEntryId;
        public string accountId;
        public string idempotencyKey;
        public int delta;
        public int balanceAfter;
        public string reason;
    }

    [Serializable]
    public sealed class UnityLeaderboardRow
    {
        public string schema;
        public string leaderboardId;
        public string publicPlayerId;
        public string displayName;
        public string mapId;
        public string mapVersion;
        public string battleId;
        public int score;
        public string rewardClaimId;
        public string resultState;
    }

    [Serializable]
    public sealed class OfflineFixtureFallback
    {
        public const string ServerUnavailable = "ServerUnavailable";
        public const string UnsignedSquad = "UnsignedSquad";
        public const string RejectedClaim = "RejectedClaim";
        public const string DuplicateClaim = "DuplicateClaim";

        public bool useLocalFixture;
        public bool rewardClaimEnabled;
        public string reason;
        public string message;
        public string consequence;

        public static OfflineFixtureFallback ServerUnavailableFallback(string message)
        {
            return new OfflineFixtureFallback
            {
                useLocalFixture = true,
                rewardClaimEnabled = false,
                reason = ServerUnavailable,
                message = message,
                consequence = "Launch from local fixture squad; do not block validator, Windows smoke, Android smoke or MechLab."
            };
        }

        public static OfflineFixtureFallback ForUnsignedSquad(string reason, string message)
        {
            return new OfflineFixtureFallback
            {
                useLocalFixture = true,
                rewardClaimEnabled = false,
                reason = string.IsNullOrWhiteSpace(reason) ? UnsignedSquad : reason,
                message = message,
                consequence = "Local fixture launch is allowed; reward claim is disabled for this unsigned run."
            };
        }

        public static OfflineFixtureFallback ForRejectedClaim(string reason, string message)
        {
            return new OfflineFixtureFallback
            {
                useLocalFixture = true,
                rewardClaimEnabled = false,
                reason = string.IsNullOrWhiteSpace(reason) ? RejectedClaim : reason,
                message = message,
                consequence = "Keep local debrief visible and do not mutate server inventory, token, fragment or leaderboard state."
            };
        }

        public static OfflineFixtureFallback DuplicateClaimSuccess(string message)
        {
            return new OfflineFixtureFallback
            {
                useLocalFixture = false,
                rewardClaimEnabled = true,
                reason = DuplicateClaim,
                message = message,
                consequence = "Treat an idempotent approved duplicate claim as success and do not double-apply local inventory deltas."
            };
        }
    }

    [Serializable]
    internal sealed class UnityHealthResponse
    {
        public string schema;
        public string status;
        public string service;
        public string version;
        public bool unityOfflineFirst;
    }

    [Serializable]
    internal sealed class UnityVersionResponse
    {
        public string schema;
        public string service;
        public string version;
        public string battleCoreVersion;
        public string rewardRulesVersion;
        public bool unityOfflineFirst;
        public string[] excludedFirstSliceFeatures;
    }

    [Serializable]
    internal sealed class UnityDevAccountRequest
    {
        public string displayName;
    }

    [Serializable]
    internal sealed class UnityDevAccountResponse
    {
        public string schema;
        public UnityAccountRecord account;
        public UnityPublicProfileRecord publicProfile;
        public int tokenBalance;
    }

    [Serializable]
    internal sealed class UnityInventorySnapshotResponse
    {
        public string schema;
        public UnityAccountRecord account;
        public UnityInventorySnapshot inventory;
    }

    public sealed class UnityMainServerClient
    {
        public const bool UnityOfflineFirst = true;
        public const bool NoRuntimeServerDependency = true;
        public const bool NoPerFrameServerCalls = true;
        public const bool NoLoginPaymentMarketplaceRealtimePvpChainInUnityAdapter = true;
        public const string ServiceName = "mc2-main-server-local";
        public const string BattleCoreVersion = "mc2-unity-demo-contract-v1";
        public const string EndpointHealth = "/healthz";
        public const string EndpointVersion = "/version";
        public const string EndpointDevAccounts = "/dev/accounts";
        public const string EndpointSquadsSign = "/squads/sign";
        public const string EndpointRewardClaims = "/reward-claims";

        private readonly UnityServerSettings settings;

        public UnityMainServerClient(UnityServerSettings settings)
        {
            this.settings = settings ?? UnityServerSettings.DisabledDefault();
        }

        public UnityServerStatus Probe()
        {
            if (!settings.IsEnabled)
            {
                return UnityServerStatus.Offline(OfflineFixtureFallback.ServerUnavailable, "Unity main-server adapter disabled by default.");
            }

            try
            {
                UnityHealthResponse health = GetJson<UnityHealthResponse>(EndpointHealth);
                UnityVersionResponse version = GetJson<UnityVersionResponse>(EndpointVersion);
                bool serviceOk = string.Equals(health?.service, ServiceName, StringComparison.Ordinal)
                    && string.Equals(version?.service, ServiceName, StringComparison.Ordinal);
                bool versionOk = string.Equals(version?.battleCoreVersion, BattleCoreVersion, StringComparison.Ordinal);
                bool offlineFirst = health != null && version != null && health.unityOfflineFirst && version.unityOfflineFirst;
                bool excludedBoundaryOk = ExcludesFirstSliceFeatures(version);
                bool healthy = serviceOk && versionOk && offlineFirst && excludedBoundaryOk;
                return new UnityServerStatus
                {
                    enabled = true,
                    reachable = true,
                    healthy = healthy,
                    unityOfflineFirst = offlineFirst,
                    service = version?.service ?? health?.service ?? "",
                    version = version?.version ?? health?.version ?? "",
                    battleCoreVersion = version?.battleCoreVersion ?? "",
                    rewardRulesVersion = version?.rewardRulesVersion ?? "",
                    status = healthy ? "available" : "incompatible",
                    fallbackReason = healthy ? "" : OfflineFixtureFallback.ServerUnavailable,
                    message = healthy ? "Unity main-server probe accepted." : "Unity main-server probe incompatible; use local fixture fallback."
                };
            }
            catch (Exception exception)
            {
                return UnityServerStatus.Offline(OfflineFixtureFallback.ServerUnavailable, "Unity main-server probe failed: " + exception.Message);
            }
        }

        public UnityInventoryBootstrap TryBootstrapInventory(string displayName)
        {
            if (!settings.IsEnabled)
            {
                return OfflineInventory("Unity main-server adapter disabled by default.");
            }

            try
            {
                UnityDevAccountResponse account = PostJson<UnityDevAccountRequest, UnityDevAccountResponse>(
                    EndpointDevAccounts,
                    new UnityDevAccountRequest { displayName = displayName ?? "" });
                string accountId = account?.account?.accountId ?? "";
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    return OfflineInventory("Unity main-server dev account response missing accountId.");
                }

                UnityInventorySnapshotResponse inventory = GetJson<UnityInventorySnapshotResponse>(
                    "/accounts/" + Uri.EscapeDataString(accountId) + "/inventory");
                if (inventory?.inventory == null)
                {
                    return OfflineInventory("Unity main-server inventory response missing inventory.");
                }

                return new UnityInventoryBootstrap
                {
                    schema = "UnityInventoryBootstrap",
                    account = account.account,
                    publicProfile = account.publicProfile,
                    inventory = inventory.inventory
                };
            }
            catch (Exception exception)
            {
                return OfflineInventory("Unity main-server inventory bootstrap failed: " + exception.Message);
            }
        }

        public UnitySignedSquadResult TrySignSquad(UnitySquadSignRequest request)
        {
            if (!settings.IsEnabled)
            {
                return UnitySignedSquadResult.UnsignedSquad(OfflineFixtureFallback.ServerUnavailable, "Unity main-server adapter disabled by default.");
            }

            if (request == null)
            {
                return UnitySignedSquadResult.UnsignedSquad(OfflineFixtureFallback.UnsignedSquad, "Squad sign request is empty.");
            }

            try
            {
                UnitySignedSquadResult result = PostJson<UnitySquadSignRequest, UnitySignedSquadResult>(EndpointSquadsSign, request);
                if (!IsSignedSquadCompatible(result, request))
                {
                    return UnitySignedSquadResult.UnsignedSquad(OfflineFixtureFallback.UnsignedSquad, result?.message ?? "Signed squad response was rejected or incompatible.");
                }

                result.clientStatus = "SignedSquadBeforeLaunch";
                return result;
            }
            catch (Exception exception)
            {
                return UnitySignedSquadResult.UnsignedSquad(OfflineFixtureFallback.UnsignedSquad, "Unity main-server squad signing failed: " + exception.Message);
            }
        }

        public UnityRewardClaimResult TrySubmitRewardClaim(UnityBattleResultClaim claim, UnitySignedSquadResult signedSquad)
        {
            if (!settings.IsEnabled)
            {
                return UnityRewardClaimResult.RejectedClaim(OfflineFixtureFallback.ServerUnavailable, "Unity main-server adapter disabled by default.");
            }

            if (claim == null)
            {
                return UnityRewardClaimResult.RejectedClaim(OfflineFixtureFallback.RejectedClaim, "Reward claim request is empty.");
            }

            if (!CanSubmitRewardClaim(claim, signedSquad))
            {
                return UnityRewardClaimResult.RejectedClaim(OfflineFixtureFallback.UnsignedSquad, "Reward claim disabled because active squad is unsigned or mismatched.");
            }

            try
            {
                UnityRewardClaimResult result = PostJson<UnityBattleResultClaim, UnityRewardClaimResult>(EndpointRewardClaims, claim);
                if (result != null && result.IsApproved)
                {
                    result.clientStatus = "ApprovedOrDuplicateClaim";
                    return result;
                }

                return UnityRewardClaimResult.RejectedClaim(OfflineFixtureFallback.RejectedClaim, result?.rewardClaim?.message ?? "Reward claim was rejected.");
            }
            catch (Exception exception)
            {
                return UnityRewardClaimResult.RejectedClaim(OfflineFixtureFallback.RejectedClaim, "Unity main-server reward claim failed: " + exception.Message);
            }
        }

        private UnityInventoryBootstrap OfflineInventory(string message)
        {
            return new UnityInventoryBootstrap
            {
                schema = "UnityInventoryBootstrap",
                fallback = OfflineFixtureFallback.ServerUnavailableFallback(message)
            };
        }

        private bool CanSubmitRewardClaim(UnityBattleResultClaim claim, UnitySignedSquadResult result)
        {
            UnitySignedSquad signed = result?.signedSquad;
            return signed != null
                && !string.IsNullOrWhiteSpace(claim.signedSquadId)
                && string.Equals(claim.signedSquadId, signed.signedSquadId, StringComparison.Ordinal)
                && string.Equals(claim.accountId, signed.accountId, StringComparison.Ordinal)
                && string.Equals(claim.mapId, signed.mapId, StringComparison.Ordinal)
                && string.Equals(claim.mapVersion, signed.mapVersion, StringComparison.Ordinal)
                && string.Equals(claim.battleCoreVersion, signed.battleCoreVersion, StringComparison.Ordinal);
        }

        private static bool IsSignedSquadCompatible(UnitySignedSquadResult result, UnitySquadSignRequest request)
        {
            UnitySignedSquad signed = result?.signedSquad;
            return result != null
                && result.IsSigned
                && string.Equals(result.schema, "SignedSquadLoadoutResponse", StringComparison.Ordinal)
                && string.Equals(signed.schema, "SignedSquadLoadout", StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(signed.signedSquadId)
                && !string.IsNullOrWhiteSpace(signed.loadoutHash)
                && !string.IsNullOrWhiteSpace(signed.signature)
                && string.Equals(signed.mapId, request.mapId, StringComparison.Ordinal)
                && string.Equals(signed.mapVersion, request.mapVersion, StringComparison.Ordinal)
                && string.Equals(signed.battleCoreVersion, request.battleCoreVersion, StringComparison.Ordinal)
                && signed.unitCount >= 1
                && signed.unitCount <= 6
                && SameOrder(signed.ownedMechIds, request.ownedMechIds);
        }

        private static bool SameOrder(string[] left, string[] right)
        {
            left ??= Array.Empty<string>();
            right ??= Array.Empty<string>();
            if (left.Length != right.Length)
            {
                return false;
            }

            for (int index = 0; index < left.Length; index++)
            {
                if (!string.Equals(left[index], right[index], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ExcludesFirstSliceFeatures(UnityVersionResponse version)
        {
            string[] excluded = version?.excludedFirstSliceFeatures ?? Array.Empty<string>();
            return ContainsText(excluded, "payment")
                && ContainsText(excluded, "marketplace")
                && ContainsText(excluded, "realtime PVP")
                && ContainsText(excluded, "chain integration");
        }

        private static bool ContainsText(string[] values, string expected)
        {
            for (int index = 0; index < values.Length; index++)
            {
                if (string.Equals(values[index], expected, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private TResponse GetJson<TResponse>(string endpoint)
        {
            string json = SendJson("GET", endpoint, "");
            return JsonUtility.FromJson<TResponse>(json);
        }

        private TResponse PostJson<TRequest, TResponse>(string endpoint, TRequest body)
        {
            string json = JsonUtility.ToJson(body);
            string responseJson = SendJson("POST", endpoint, json);
            return JsonUtility.FromJson<TResponse>(responseJson);
        }

        private string SendJson(string method, string endpoint, string body)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BuildUrl(endpoint));
            request.Method = method;
            request.Timeout = Math.Max(250, settings.timeoutMilliseconds);
            request.ReadWriteTimeout = Math.Max(250, settings.timeoutMilliseconds);
            request.Accept = "application/json";

            if (string.Equals(method, "POST", StringComparison.Ordinal))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(body ?? "");
                request.ContentType = "application/json";
                request.ContentLength = bytes.Length;
                using Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using WebResponse response = request.GetResponse();
            using Stream responseStream = response.GetResponseStream();
            using StreamReader reader = new(responseStream ?? Stream.Null, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        private string BuildUrl(string endpoint)
        {
            string baseUrl = settings.baseUrl?.TrimEnd('/') ?? UnityServerSettings.DefaultBaseUrl;
            return baseUrl + "/" + (endpoint ?? "").TrimStart('/');
        }
    }
}
