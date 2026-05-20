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

        public MiniMaxCommanderResult ChooseCommand(CommanderObservation observation)
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
                string command = ExtractCommandFromText(content);
                if (string.IsNullOrWhiteSpace(command))
                {
                    return MiniMaxCommanderResult.Failed("MiniMax returned no legal commander command.");
                }

                return MiniMaxCommanderResult.Succeeded(command, "MiniMax command accepted.");
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

        public static string ExtractCommandFromText(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return "";
            }

            string normalized = StripThinking(content).Replace("\r", "\n");
            string commandField = TryExtractJsonCommandField(normalized);
            if (TryNormalizeCommand(commandField, out string jsonCommand))
            {
                return jsonCommand;
            }

            string[] lines = normalized
                .Replace("```", "\n")
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < lines.Length; index++)
            {
                if (TryNormalizeCommand(lines[index], out string command))
                {
                    return command;
                }
            }

            return TryNormalizeCommand(normalized, out string fallbackCommand) ? fallbackCommand : "";
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
            string observationJson = JsonUtility.ToJson(observation);
            return new MiniMaxChatRequest
            {
                model = config.Model,
                temperature = 0.2f,
                max_completion_tokens = 512,
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
                        content = "Battle observation JSON:\n" + observationJson
                    }
                }
            };
        }

        private static string BuildSystemPrompt()
        {
            return string.Join(
                " ",
                "You are the tactical commander for a MechCommander-style prototype.",
                "Return exactly one command line and nothing else.",
                "Never return markdown, JSON, commentary, or examples.",
                "Valid commands are: squad move x y, squad jump x y, squad attack unit id, squad attack structure id, unit id move x y, unit id jump x y, unit id attack unit targetId, unit id attack structure targetId.",
                "Use only unit ids, structure ids, target ids, and coordinates present in the observation.",
                "Prefer attacking active hostiles in range, then attacking current objective structures, then moving toward current objective target points or markers.",
                "If missionEnded is true, return an empty string.");
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

        private static string TryExtractJsonCommandField(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            int keyIndex = text.IndexOf("\"command\"", StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
            {
                keyIndex = text.IndexOf("'command'", StringComparison.OrdinalIgnoreCase);
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

        private static bool TryNormalizeCommand(string candidate, out string command)
        {
            command = "";
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return false;
            }

            string normalized = candidate.Trim();
            int commandStart = FindCommandStart(normalized);
            if (commandStart < 0)
            {
                return false;
            }

            normalized = normalized.Substring(commandStart)
                .Trim()
                .Trim('"', '\'', '`', '.', '。', ';', '；', ',', '，');

            if (!CommanderCommandPort.TryParse(normalized, out _, out _))
            {
                return false;
            }

            command = normalized;
            return true;
        }

        private static int FindCommandStart(string text)
        {
            int squadIndex = text.IndexOf("squad ", StringComparison.OrdinalIgnoreCase);
            int unitIndex = text.IndexOf("unit ", StringComparison.OrdinalIgnoreCase);
            if (squadIndex < 0)
            {
                return unitIndex;
            }

            if (unitIndex < 0)
            {
                return squadIndex;
            }

            return Math.Min(squadIndex, unitIndex);
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
        public string Command { get; private set; }
        public string Message { get; private set; }

        private MiniMaxCommanderResult()
        {
        }

        public static MiniMaxCommanderResult Succeeded(string command, string message)
        {
            return new MiniMaxCommanderResult
            {
                Success = true,
                Command = command ?? "",
                Message = message ?? ""
            };
        }

        public static MiniMaxCommanderResult Failed(string message)
        {
            return new MiniMaxCommanderResult
            {
                Success = false,
                Command = "",
                Message = message ?? ""
            };
        }
    }
}
