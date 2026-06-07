using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MC2Demo.BattleCore
{
    public sealed class MiniMaxCommander
    {
        private const string DefaultBaseUrl = "https://api.minimaxi.com/v1";
        private const string DefaultModel = "MiniMax-M2.5";
        private const int DefaultTimeoutMilliseconds = 45000;

        private readonly MiniMaxCommanderConfig config;

        public MiniMaxCommander(MiniMaxCommanderConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public MiniMaxCommanderResult ChooseDirective(CommanderObservation observation)
        {
            if (observation == null)
            {
                return MiniMaxCommanderResult.Failed("Observation is empty.");
            }

            if (!config.IsConfigured)
            {
                return MiniMaxCommanderResult.Failed("MINIMAX_API_KEY is not configured.");
            }

            try
            {
                string requestJson = JsonUtility.ToJson(MakeRequest(observation));
                string responseJson = PostJson(config.EndpointUrl, requestJson);
                MiniMaxChatResponse response = JsonUtility.FromJson<MiniMaxChatResponse>(responseJson);
                string content = response?.choices == null || response.choices.Length == 0
                    ? ""
                    : response.choices[0]?.message?.content ?? "";
                string directive = ExtractDirectiveFromText(content);
                if (string.IsNullOrWhiteSpace(directive))
                {
                    return MiniMaxCommanderResult.Failed("MiniMax returned no legal commander directive.");
                }

                return MiniMaxCommanderResult.Succeeded(directive, "MiniMax directive accepted.");
            }
            catch (WebException exception)
            {
                return MiniMaxCommanderResult.Failed("MiniMax HTTP request failed: " + DescribeWebException(exception));
            }
            catch (Exception exception)
            {
                return MiniMaxCommanderResult.Failed("MiniMax commander failed: " + exception.Message);
            }
        }

        public static string ExtractDirectiveFromText(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return "";
            }

            string normalized = StripThinking(content).Replace("\r", "\n");
            string directiveField = TryExtractJsonField(normalized, "directive");
            if (TryNormalizeDirective(directiveField, out string jsonDirective))
            {
                return jsonDirective;
            }

            string[] lines = normalized
                .Replace("```", "\n")
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < lines.Length; index++)
            {
                if (TryNormalizeDirective(lines[index], out string directive))
                {
                    return directive;
                }
            }

            return TryNormalizeDirective(normalized, out string fallbackDirective) ? fallbackDirective : "";
        }

        public static MiniMaxCommanderConfig ConfigFromEnvironment()
        {
            return new MiniMaxCommanderConfig
            {
                ApiKey = NormalizeApiKey(ReadEnvironmentOrDefault("MINIMAX_API_KEY", "")),
                BaseUrl = ReadEnvironmentOrDefault("MINIMAX_BASE_URL", DefaultBaseUrl),
                Model = ReadEnvironmentOrDefault("MINIMAX_MODEL", DefaultModel),
                TimeoutMilliseconds = DefaultTimeoutMilliseconds
            };
        }

        private MiniMaxChatRequest MakeRequest(CommanderObservation observation)
        {
            return new MiniMaxChatRequest
            {
                model = config.Model,
                temperature = 0.2f,
                max_completion_tokens = 32,
                stream = false,
                messages = new[]
                {
                    new MiniMaxChatMessage
                    {
                        role = "system",
                        content = BuildSystemPrompt()
                    },
                    new MiniMaxChatMessage
                    {
                        role = "user",
                        content = BuildStrategicSummary(observation)
                    }
                }
            };
        }

        private static string BuildSystemPrompt()
        {
            return string.Join(
                " ",
                "You are the tactical commander for an AI tactical RTS prototype.",
                "You make slow, high-level decisions only; local battle code handles movement, target choice, firing, heat, and avoidance.",
                "Return exactly one directive token and nothing else.",
                "Valid directive tokens: assault-objective, engage-hostiles, regroup, withdraw-if-critical, hold.",
                "Use assault-objective when unsure.",
                "Never return coordinates, unit ids, markdown, JSON, commentary, or examples.",
                "If missionEnded is true, return hold.");
        }

        private static string BuildStrategicSummary(CommanderObservation observation)
        {
            return observation?.compact == null
                ? BuildLegacyStrategicSummary(observation)
                : BuildCompactStrategicSummary(observation.compact);
        }

        public static string BuildStrategicSummaryForValidation(CommanderObservation observation)
        {
            return BuildStrategicSummary(observation);
        }

        private static string BuildCompactStrategicSummary(CommanderCompactObservation compact)
        {
            StringBuilder builder = new();
            builder.Append("Compact AI observation ")
                .Append(compact.schema)
                .Append(". mission=")
                .Append(Safe(compact.missionId))
                .Append(" phase=")
                .Append(Safe(compact.missionPhase))
                .Append(" result=")
                .Append(Safe(compact.result))
                .Append(" ended=")
                .Append(compact.missionEnded)
                .Append(" time=")
                .Append(compact.missionTimeSeconds)
                .AppendLine();

            builder.Append("Commander identity: unit=")
                .Append(Safe(compact.commanderUnitId))
                .Append(" ownedMech=")
                .Append(Safe(compact.commanderOwnedMechId))
                .Append(" type=")
                .Append(Safe(compact.commanderType))
                .AppendLine();

            CommanderCompactObjectiveObservation objective = compact.objective;
            builder.Append("Objective summary: active=")
                .Append(compact.currentObjectiveCount)
                .Append(" title=")
                .Append(Safe(objective?.title))
                .Append(" kind=")
                .Append(Safe(objective?.targetKind))
                .Append(" targets=")
                .Append(objective?.targetCount ?? 0)
                .Append(" range=")
                .Append(Safe(objective?.rangeHint))
                .AppendLine();

            builder.Append("Squad summary: active=")
                .Append(compact.activePlayerUnitCount)
                .Append("/")
                .Append(compact.playerUnitCount)
                .Append(" damaged=")
                .Append(compact.damagedPlayerUnitCount)
                .Append(" detached=")
                .Append(compact.detachedPlayerUnitCount)
                .Append(" destroyed=")
                .Append(compact.destroyedPlayerUnitCount)
                .Append(" heatLocked=")
                .Append(compact.heatLockedPlayerUnitCount)
                .Append(" avgStructure=")
                .Append(compact.averagePlayerStructurePercent)
                .Append("% hot=")
                .Append(compact.hottestPlayerHeatPercent)
                .Append("%")
                .AppendLine();

            builder.Append("Squad states: ");
            AppendCompactPlayers(builder, compact.playerStates);
            builder.AppendLine();

            builder.Append("Threat summary: level=")
                .Append(Safe(compact.threatLevel))
                .Append(" active=")
                .Append(compact.hostileCount)
                .Append(" nearby=")
                .Append(compact.nearbyThreatCount)
                .Append(" inWeaponRange=")
                .Append(compact.inRangeThreatCount)
                .Append(" structures=")
                .Append(compact.targetableStructureCount)
                .AppendLine();

            builder.Append("Nearby threats: ");
            AppendCompactThreats(builder, compact.nearbyThreats);
            builder.AppendLine();

            builder.Append("Available intents: ")
                .Append(string.Join(", ", compact.availableIntents ?? Array.Empty<string>()))
                .AppendLine();
            builder.Append("Choose one directive token for the next 10-30 seconds.");
            return builder.ToString();
        }

        private static string BuildLegacyStrategicSummary(CommanderObservation observation)
        {
            StringBuilder builder = new();
            builder.Append("Battle strategic summary. missionEnded=")
                .Append(observation?.missionEnded)
                .Append(" result=")
                .Append(observation?.result)
                .Append(" time=")
                .Append(Mathf.RoundToInt(observation?.missionTimeSeconds ?? 0f))
                .AppendLine();

            builder.Append("Player units: ");
            AppendUnits(builder, observation?.playerUnits, includeDetached: true);
            builder.AppendLine();

            builder.Append("Active hostiles: ");
            AppendUnits(builder, observation?.activeHostiles, includeDetached: false);
            builder.AppendLine();

            builder.Append("Current objectives: ");
            CommanderObjectiveObservation[] objectives = observation?.currentObjectives ?? Array.Empty<CommanderObjectiveObservation>();
            if (objectives.Length == 0)
            {
                builder.Append("none");
            }

            for (int index = 0; index < objectives.Length; index++)
            {
                CommanderObjectiveObservation objective = objectives[index];
                if (objective == null)
                {
                    continue;
                }

                if (index > 0)
                {
                    builder.Append(" | ");
                }

                builder.Append(objective.title)
                    .Append(" marker=(")
                    .Append(Mathf.RoundToInt(objective.markerX))
                    .Append(",")
                    .Append(Mathf.RoundToInt(objective.markerY))
                    .Append(")");
            }

            builder.AppendLine();
            builder.Append("Choose one directive token for the next 10-30 seconds.");
            return builder.ToString();
        }

        private static void AppendCompactPlayers(StringBuilder builder, CommanderCompactUnitObservation[] units)
        {
            units ??= Array.Empty<CommanderCompactUnitObservation>();
            if (units.Length == 0)
            {
                builder.Append("none");
                return;
            }

            for (int index = 0; index < units.Length; index++)
            {
                CommanderCompactUnitObservation unit = units[index];
                if (unit == null)
                {
                    continue;
                }

                if (index > 0)
                {
                    builder.Append(" | ");
                }

                builder.Append(Safe(unit.role))
                    .Append("/")
                    .Append(Safe(unit.type))
                    .Append(" active=")
                    .Append(unit.active)
                    .Append(" destroyed=")
                    .Append(unit.destroyed)
                    .Append(" detached=")
                    .Append(unit.detached)
                    .Append(" moving=")
                    .Append(unit.moving)
                    .Append(" jumping=")
                    .Append(unit.jumping)
                    .Append(" hp=")
                    .Append(unit.structurePercent)
                    .Append("% heat=")
                    .Append(unit.heatPercent)
                    .Append("% ready=")
                    .Append(unit.weaponReadyPercent)
                    .Append("% damage=")
                    .Append(Safe(unit.sectionDamage));
            }
        }

        private static void AppendCompactThreats(StringBuilder builder, CommanderCompactThreatObservation[] threats)
        {
            threats ??= Array.Empty<CommanderCompactThreatObservation>();
            if (threats.Length == 0)
            {
                builder.Append("none");
                return;
            }

            for (int index = 0; index < threats.Length; index++)
            {
                CommanderCompactThreatObservation threat = threats[index];
                if (threat == null)
                {
                    continue;
                }

                if (index > 0)
                {
                    builder.Append(" | ");
                }

                builder.Append(Safe(threat.type))
                    .Append(" band=")
                    .Append(Safe(threat.distanceBand))
                    .Append(" inRange=")
                    .Append(threat.inWeaponRange)
                    .Append(" hp=")
                    .Append(threat.structurePercent)
                    .Append("%");
            }
        }

        private static string Safe(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? "none" : text.Trim();
        }

        private static void AppendUnits(StringBuilder builder, CommanderUnitObservation[] units, bool includeDetached)
        {
            units ??= Array.Empty<CommanderUnitObservation>();
            if (units.Length == 0)
            {
                builder.Append("none");
                return;
            }

            int emitted = 0;
            for (int index = 0; index < units.Length && emitted < 8; index++)
            {
                CommanderUnitObservation unit = units[index];
                if (unit == null)
                {
                    continue;
                }

                if (emitted > 0)
                {
                    builder.Append(" | ");
                }

                builder.Append(unit.id)
                    .Append(" active=")
                    .Append(unit.active)
                    .Append(" destroyed=")
                    .Append(unit.destroyed)
                    .Append(" hp=")
                    .Append(Mathf.RoundToInt(unit.structureRatio * 100f))
                    .Append("% heat=")
                    .Append(Mathf.RoundToInt(unit.heatRatio * 100f))
                    .Append("% range=")
                    .Append(Mathf.RoundToInt(unit.weaponRange))
                    .Append(" pos=(")
                    .Append(Mathf.RoundToInt(unit.x))
                    .Append(",")
                    .Append(Mathf.RoundToInt(unit.y))
                    .Append(")");

                if (includeDetached)
                {
                    builder.Append(" detached=").Append(unit.detached);
                }

                emitted++;
            }
        }

        private string PostJson(string url, string json)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers[HttpRequestHeader.Authorization] = "Bearer " + config.ApiKey;
            request.Timeout = config.TimeoutMilliseconds;
            request.ReadWriteTimeout = config.TimeoutMilliseconds;
            request.ContentLength = bytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader reader = new(responseStream ?? Stream.Null, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private static string StripThinking(string content)
        {
            return Regex.Replace(content, "<think>[\\s\\S]*?</think>", " ", RegexOptions.IgnoreCase).Trim();
        }

        private static string TryExtractJsonField(string text, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            int keyIndex = text.IndexOf("\"" + fieldName + "\"", StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
            {
                keyIndex = text.IndexOf("'" + fieldName + "'", StringComparison.OrdinalIgnoreCase);
            }

            if (keyIndex < 0)
            {
                return "";
            }

            int colonIndex = text.IndexOf(':', keyIndex);
            if (colonIndex < 0)
            {
                return "";
            }

            int quoteIndex = IndexOfQuote(text, colonIndex + 1);
            if (quoteIndex < 0)
            {
                return "";
            }

            char quote = text[quoteIndex];
            StringBuilder builder = new();
            bool escaped = false;
            for (int index = quoteIndex + 1; index < text.Length; index++)
            {
                char current = text[index];
                if (escaped)
                {
                    builder.Append(current);
                    escaped = false;
                    continue;
                }

                if (current == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (current == quote)
                {
                    return builder.ToString();
                }

                builder.Append(current);
            }

            return "";
        }

        private static int IndexOfQuote(string text, int startIndex)
        {
            for (int index = startIndex; index < text.Length; index++)
            {
                if (text[index] == '"' || text[index] == '\'')
                {
                    return index;
                }
            }

            return -1;
        }

        private static bool TryNormalizeDirective(string candidate, out string directive)
        {
            directive = "";
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return false;
            }

            string normalized = candidate.Trim().ToLowerInvariant().Replace('_', '-').Replace(' ', '-');
            string[] directives =
            {
                RuleCommander.DirectiveAssaultObjective,
                RuleCommander.DirectiveEngageHostiles,
                RuleCommander.DirectiveRegroup,
                RuleCommander.DirectiveWithdrawIfCritical,
                RuleCommander.DirectiveHold
            };
            for (int index = 0; index < directives.Length; index++)
            {
                if (normalized.IndexOf(directives[index], StringComparison.Ordinal) >= 0)
                {
                    directive = directives[index];
                    return true;
                }

            }

            return false;
        }

        private static string DescribeWebException(WebException exception)
        {
            if (exception.Response is HttpWebResponse response)
            {
                return "status " + (int)response.StatusCode + " " + response.StatusCode;
            }

            return exception.Status.ToString();
        }

        private static string ReadEnvironmentOrDefault(string name, string defaultValue)
        {
            string value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        }

        private static string NormalizeApiKey(string apiKey)
        {
            const string BearerPrefix = "Bearer ";
            string normalized = apiKey.Trim().Trim('"', '\'');
            return normalized.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase)
                ? normalized.Substring(BearerPrefix.Length).Trim()
                : normalized;
        }

        [Serializable]
        private sealed class MiniMaxChatRequest
        {
            public string model;
            public float temperature;
            public int max_completion_tokens;
            public bool stream;
            public MiniMaxChatMessage[] messages;
        }

        [Serializable]
        private sealed class MiniMaxChatMessage
        {
            public string role;
            public string content;
        }

        [Serializable]
        private sealed class MiniMaxChatResponse
        {
            public MiniMaxChatChoice[] choices;
        }

        [Serializable]
        private sealed class MiniMaxChatChoice
        {
            public MiniMaxChatMessageResponse message;
        }

        [Serializable]
        private sealed class MiniMaxChatMessageResponse
        {
            public string content;
        }
    }

    public sealed class MiniMaxCommanderConfig
    {
        public string ApiKey;
        public string BaseUrl;
        public string Model;
        public int TimeoutMilliseconds;

        public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);

        public string EndpointUrl
        {
            get
            {
                string baseUrl = string.IsNullOrWhiteSpace(BaseUrl) ? "https://api.minimaxi.com/v1" : BaseUrl.TrimEnd('/');
                return baseUrl.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase)
                    ? baseUrl
                    : baseUrl + "/chat/completions";
            }
        }

        public string DescribeWithoutSecrets()
        {
            return "endpoint=" + EndpointUrl + " model=" + Model + " configured=" + IsConfigured;
        }
    }

    public sealed class MiniMaxCommanderResult
    {
        public bool Success { get; private set; }
        public string Directive { get; private set; }
        public string Message { get; private set; }

        private MiniMaxCommanderResult()
        {
        }

        public static MiniMaxCommanderResult Succeeded(string directive, string message)
        {
            return new MiniMaxCommanderResult
            {
                Success = true,
                Directive = directive ?? "",
                Message = message ?? ""
            };
        }

        public static MiniMaxCommanderResult Failed(string message)
        {
            return new MiniMaxCommanderResult
            {
                Success = false,
                Directive = "",
                Message = message ?? ""
            };
        }
    }
}
