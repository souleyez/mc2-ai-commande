using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MC2Demo.Presentation
{
    public static class StartupCommanderScript
    {
        private static readonly char[] TokenSeparators = { ' ', '\t' };

        public static StartupCommanderScriptAction[] LoadFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidDataException("Command file path is empty.");
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Command file missing.", path);
            }

            return ParseLines(File.ReadAllLines(path), path);
        }

        public static StartupCommanderScriptAction[] ParseLines(IEnumerable<string> lines, string sourceName)
        {
            List<StartupCommanderScriptAction> actions = new();
            int lineNumber = 0;
            foreach (string line in lines)
            {
                lineNumber++;
                if (!TryParseLine(line, lineNumber, out StartupCommanderScriptAction action, out string error))
                {
                    throw new InvalidDataException(sourceName + ":" + lineNumber + ": " + error);
                }

                if (action.Kind != StartupCommanderScriptActionKind.None)
                {
                    actions.Add(action);
                }
            }

            return actions.ToArray();
        }

        public static bool TryParseLine(string line, int lineNumber, out StartupCommanderScriptAction action, out string error)
        {
            string rawLine = line ?? string.Empty;
            string trimmed = rawLine.Trim();
            action = StartupCommanderScriptAction.None(lineNumber, rawLine);
            error = string.Empty;

            if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal))
            {
                return true;
            }

            int verbEnd = trimmed.IndexOfAny(TokenSeparators);
            string verb = verbEnd < 0 ? trimmed : trimmed.Substring(0, verbEnd);
            string payload = verbEnd < 0 ? string.Empty : trimmed.Substring(verbEnd + 1).Trim();

            if (string.Equals(verb, "command", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(payload))
                {
                    error = "Command action needs command text.";
                    return false;
                }

                action = StartupCommanderScriptAction.Command(lineNumber, rawLine, payload);
                return true;
            }

            if (string.Equals(verb, "advance", StringComparison.OrdinalIgnoreCase))
            {
                if (!float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out float seconds) || seconds < 0f)
                {
                    error = "Advance action needs a non-negative seconds value.";
                    return false;
                }

                action = StartupCommanderScriptAction.Advance(lineNumber, rawLine, seconds);
                return true;
            }

            if (string.Equals(verb, "report", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Report action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.Report(lineNumber, rawLine);
                return true;
            }

            error = "Command file action must be command, advance, or report.";
            return false;
        }
    }

    public sealed class StartupCommanderScriptAction
    {
        public StartupCommanderScriptActionKind Kind { get; private set; }
        public int LineNumber { get; private set; }
        public string SourceLine { get; private set; }
        public string CommandText { get; private set; }
        public float AdvanceSeconds { get; private set; }

        private StartupCommanderScriptAction()
        {
        }

        public static StartupCommanderScriptAction None(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.None,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction Command(int lineNumber, string sourceLine, string commandText)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.Command,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty,
                CommandText = commandText
            };
        }

        public static StartupCommanderScriptAction Advance(int lineNumber, string sourceLine, float seconds)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.Advance,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty,
                AdvanceSeconds = seconds
            };
        }

        public static StartupCommanderScriptAction Report(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.Report,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }
    }

    public enum StartupCommanderScriptActionKind
    {
        None,
        Command,
        Advance,
        Report
    }
}
