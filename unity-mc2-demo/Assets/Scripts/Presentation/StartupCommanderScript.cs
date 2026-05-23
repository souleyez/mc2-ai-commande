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

            if (string.Equals(verb, "restart", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Restart action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.Restart(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "mech-bay-launch", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Mech bay launch action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.MechBayLaunch(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "hide-squad-preview", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Hide squad preview action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.HideSquadPreview(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "saved-account-report", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Saved account report action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.SavedAccountReport(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "saved-account-save-load-preview", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Saved account save/load preview action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.SavedAccountSaveLoadPreview(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "prepare-depot-candidate", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Prepare depot candidate action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.PrepareDepotCandidate(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "prepare-local-candidate", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Prepare local candidate action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.PrepareLocalCandidate(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "squad-swap", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Squad swap action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.SquadSwap(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "assert-restart-identity", StringComparison.OrdinalIgnoreCase))
            {
                bool requireDepot = false;
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    if (!string.Equals(payload, "depot", StringComparison.OrdinalIgnoreCase))
                    {
                        error = "Assert restart identity action only accepts optional 'depot'.";
                        return false;
                    }

                    requireDepot = true;
                }

                action = StartupCommanderScriptAction.AssertRestartIdentity(lineNumber, rawLine, requireDepot);
                return true;
            }

            error = "Command file action must be command, advance, report, restart, mech-bay-launch, hide-squad-preview, saved-account-report, saved-account-save-load-preview, prepare-depot-candidate, prepare-local-candidate, squad-swap, or assert-restart-identity.";
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
        public bool RequireDepotIdentity { get; private set; }

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

        public static StartupCommanderScriptAction Restart(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.Restart,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction MechBayLaunch(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.MechBayLaunch,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction HideSquadPreview(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.HideSquadPreview,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction SavedAccountReport(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.SavedAccountReport,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction SavedAccountSaveLoadPreview(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.SavedAccountSaveLoadPreview,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction PrepareDepotCandidate(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.PrepareDepotCandidate,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction PrepareLocalCandidate(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.PrepareLocalCandidate,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction SquadSwap(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.SquadSwap,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction AssertRestartIdentity(
            int lineNumber,
            string sourceLine,
            bool requireDepotIdentity)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.AssertRestartIdentity,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty,
                RequireDepotIdentity = requireDepotIdentity
            };
        }
    }

    public enum StartupCommanderScriptActionKind
    {
        None,
        Command,
        Advance,
        Report,
        Restart,
        MechBayLaunch,
        HideSquadPreview,
        SavedAccountReport,
        SavedAccountSaveLoadPreview,
        PrepareDepotCandidate,
        PrepareLocalCandidate,
        SquadSwap,
        AssertRestartIdentity
    }
}
