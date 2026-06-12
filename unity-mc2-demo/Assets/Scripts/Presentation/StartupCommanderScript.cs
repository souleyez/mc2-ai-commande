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

            if (string.Equals(verb, "status-row", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(payload))
                {
                    error = "Status row action needs squad, all, or a unit id.";
                    return false;
                }

                action = StartupCommanderScriptAction.StatusRowSelect(lineNumber, rawLine, payload);
                return true;
            }

            if (string.Equals(verb, "battle-click", StringComparison.OrdinalIgnoreCase))
            {
                string[] tokens = payload.Split(TokenSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != 2
                    || !float.TryParse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float missionX)
                    || !float.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float missionY))
                {
                    error = "Battle click action needs mission X and Y coordinates.";
                    return false;
                }

                action = StartupCommanderScriptAction.BattleClick(lineNumber, rawLine, missionX, missionY);
                return true;
            }

            if (string.Equals(verb, "battle-target", StringComparison.OrdinalIgnoreCase))
            {
                string[] tokens = payload.Split(TokenSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != 2
                    || (!string.Equals(tokens[0], "unit", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(tokens[0], "structure", StringComparison.OrdinalIgnoreCase)))
                {
                    error = "Battle target action needs unit ID or structure ID.";
                    return false;
                }

                action = StartupCommanderScriptAction.BattleTarget(lineNumber, rawLine, tokens[0], tokens[1]);
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

            if (string.Equals(verb, "complete-visible-objectives", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Complete visible objectives action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.CompleteVisibleObjectives(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "open-debrief", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Open debrief action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.OpenDebrief(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "main-server-smoke", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Main server smoke action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.MainServerSmoke(lineNumber, rawLine);
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

            if (string.Equals(verb, "saved-account-export", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(payload))
                {
                    error = "Saved account export action needs a file path.";
                    return false;
                }

                action = StartupCommanderScriptAction.SavedAccountExport(lineNumber, rawLine, payload);
                return true;
            }

            if (string.Equals(verb, "saved-account-import-preview", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(payload))
                {
                    error = "Saved account import preview action needs a file path.";
                    return false;
                }

                action = StartupCommanderScriptAction.SavedAccountImportPreview(lineNumber, rawLine, payload);
                return true;
            }

            if (string.Equals(verb, "saved-account-import-apply-preview", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(payload))
                {
                    error = "Saved account import apply preview action needs a file path.";
                    return false;
                }

                action = StartupCommanderScriptAction.SavedAccountImportApplyPreview(lineNumber, rawLine, payload);
                return true;
            }

            if (string.Equals(verb, "saved-account-import-apply", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(payload))
                {
                    error = "Saved account import apply action needs a file path.";
                    return false;
                }

                action = StartupCommanderScriptAction.SavedAccountImportApply(lineNumber, rawLine, payload);
                return true;
            }

            if (string.Equals(verb, "saved-account-load-default-preview", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Saved account default load preview action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.SavedAccountLoadDefaultPreview(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "saved-account-save-current-default", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Saved account default current save action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.SavedAccountSaveCurrentDefault(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "saved-account-load-default-apply", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Saved account default load apply action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.SavedAccountLoadDefaultApply(lineNumber, rawLine);
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

            if (string.Equals(verb, "assert-debrief-summary", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Assert debrief summary action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.AssertDebriefSummary(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "assert-debrief-visible", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Assert debrief visible action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.AssertDebriefVisible(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "assert-main-server-smoke", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Assert main server smoke action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.AssertMainServerSmoke(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "assert-command-result", StringComparison.OrdinalIgnoreCase))
            {
                bool hasExpectation = false;
                bool expectedBlocked = false;
                int expectedAcceptedCount = -1;
                if (string.IsNullOrWhiteSpace(payload))
                {
                    error = "Assert command result action needs blocked or accepted=N.";
                    return false;
                }

                string[] tokens = payload.Split(TokenSeparators, StringSplitOptions.RemoveEmptyEntries);
                for (int index = 0; index < tokens.Length; index++)
                {
                    string token = tokens[index].Trim().ToLowerInvariant();
                    if (string.Equals(token, "blocked", StringComparison.Ordinal))
                    {
                        if (hasExpectation)
                        {
                            error = "Assert command result action accepts only one expectation.";
                            return false;
                        }

                        hasExpectation = true;
                        expectedBlocked = true;
                        continue;
                    }

                    if (token.StartsWith("accepted=", StringComparison.Ordinal))
                    {
                        if (hasExpectation)
                        {
                            error = "Assert command result action accepts only one expectation.";
                            return false;
                        }

                        string value = token.Substring("accepted=".Length);
                        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out expectedAcceptedCount)
                            || expectedAcceptedCount <= 0)
                        {
                            error = "Assert command result accepted argument must be accepted=N with a positive integer.";
                            return false;
                        }

                        hasExpectation = true;
                        continue;
                    }

                    error = "Assert command result action only accepts blocked or accepted=N.";
                    return false;
                }

                action = StartupCommanderScriptAction.AssertCommandResult(lineNumber, rawLine, expectedBlocked, expectedAcceptedCount);
                return true;
            }

            if (string.Equals(verb, "assert-combat-situation", StringComparison.OrdinalIgnoreCase))
            {
                string expectedTempo = "";
                int expectedSoloCount = -1;
                int expectedJumpingCount = -1;
                int expectedSalvageCount = -1;
                string expectedSelection = "";
                string expectedRowUnitId = "";
                string expectedRowState = "";
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    string[] tokens = payload.Split(TokenSeparators, StringSplitOptions.RemoveEmptyEntries);
                    for (int index = 0; index < tokens.Length; index++)
                    {
                        string token = tokens[index].Trim().ToLowerInvariant();
                        if (string.Equals(token, "quiet", StringComparison.Ordinal)
                            || string.Equals(token, "contact", StringComparison.Ordinal)
                            || string.Equals(token, "tracking", StringComparison.Ordinal)
                            || string.Equals(token, "fire", StringComparison.Ordinal))
                        {
                            expectedTempo = token;
                            continue;
                        }

                        if (token.StartsWith("solo=", StringComparison.Ordinal))
                        {
                            string value = token.Substring("solo=".Length);
                            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out expectedSoloCount)
                                || expectedSoloCount < 0)
                            {
                                error = "Assert combat situation solo argument must be solo=N with a non-negative integer.";
                                return false;
                            }

                            continue;
                        }

                        if (token.StartsWith("jumping=", StringComparison.Ordinal))
                        {
                            string value = token.Substring("jumping=".Length);
                            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out expectedJumpingCount)
                                || expectedJumpingCount < 0)
                            {
                                error = "Assert combat situation jumping argument must be jumping=N with a non-negative integer.";
                                return false;
                            }

                            continue;
                        }

                        if (token.StartsWith("salvage=", StringComparison.Ordinal))
                        {
                            string value = token.Substring("salvage=".Length);
                            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out expectedSalvageCount)
                                || expectedSalvageCount < 0)
                            {
                                error = "Assert combat situation salvage argument must be salvage=N with a non-negative integer.";
                                return false;
                            }

                            continue;
                        }

                        if (token.StartsWith("selection=", StringComparison.Ordinal))
                        {
                            expectedSelection = token.Substring("selection=".Length);
                            if (string.IsNullOrWhiteSpace(expectedSelection))
                            {
                                error = "Assert combat situation selection argument must be selection=squad or selection=unit-id.";
                                return false;
                            }

                            continue;
                        }

                        if (token.StartsWith("row=", StringComparison.Ordinal))
                        {
                            string value = token.Substring("row=".Length);
                            int splitIndex = value.IndexOf(':');
                            if (splitIndex <= 0 || splitIndex >= value.Length - 1)
                            {
                                error = "Assert combat situation row argument must be row=unit-id:state.";
                                return false;
                            }

                            expectedRowUnitId = value.Substring(0, splitIndex);
                            expectedRowState = value.Substring(splitIndex + 1);
                            continue;
                        }

                        error = "Assert combat situation action only accepts optional quiet, contact, tracking, fire, solo=N, jumping=N, salvage=N, selection=VALUE, or row=UNIT:STATE.";
                        return false;
                    }
                }

                action = StartupCommanderScriptAction.AssertCombatSituation(
                    lineNumber,
                    rawLine,
                    expectedTempo,
                    expectedSoloCount,
                    expectedJumpingCount,
                    expectedSalvageCount,
                    expectedSelection,
                    expectedRowUnitId,
                    expectedRowState);
                return true;
            }

            if (string.Equals(verb, "assert-encounter-pacing", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Assert encounter pacing action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.AssertEncounterPacing(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "assert-objective-graph", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Assert objective graph action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.AssertObjectiveGraph(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "assert-loadout-compact", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Assert loadout compact action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.AssertLoadoutCompact(lineNumber, rawLine);
                return true;
            }

            if (string.Equals(verb, "assert-ai-deputy-window", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    error = "Assert AI deputy window action does not accept arguments.";
                    return false;
                }

                action = StartupCommanderScriptAction.AssertAiDeputyWindow(lineNumber, rawLine);
                return true;
            }

            error = "Command file action must be command, status-row, battle-click, battle-target, advance, report, restart, mech-bay-launch, hide-squad-preview, complete-visible-objectives, open-debrief, main-server-smoke, saved-account-report, saved-account-save-load-preview, saved-account-export, saved-account-import-preview, saved-account-import-apply-preview, saved-account-import-apply, saved-account-load-default-preview, saved-account-save-current-default, saved-account-load-default-apply, prepare-depot-candidate, prepare-local-candidate, squad-swap, assert-restart-identity, assert-debrief-summary, assert-debrief-visible, assert-main-server-smoke, assert-command-result, assert-combat-situation, assert-encounter-pacing, assert-objective-graph, assert-loadout-compact, or assert-ai-deputy-window.";
            return false;
        }
    }

    public sealed class StartupCommanderScriptAction
    {
        public StartupCommanderScriptActionKind Kind { get; private set; }
        public int LineNumber { get; private set; }
        public string SourceLine { get; private set; }
        public string CommandText { get; private set; }
        public string StatusRowUnitId { get; private set; }
        public string BattleTargetKind { get; private set; }
        public string BattleTargetId { get; private set; }
        public string FilePath { get; private set; }
        public float AdvanceSeconds { get; private set; }
        public float MissionX { get; private set; }
        public float MissionY { get; private set; }
        public bool RequireDepotIdentity { get; private set; }
        public string ExpectedCombatTempo { get; private set; }
        public int ExpectedSoloCount { get; private set; } = -1;
        public int ExpectedJumpingCount { get; private set; } = -1;
        public int ExpectedSalvageCount { get; private set; } = -1;
        public string ExpectedSelection { get; private set; }
        public string ExpectedRowUnitId { get; private set; }
        public string ExpectedRowState { get; private set; }
        public bool ExpectedCommandBlocked { get; private set; }
        public int ExpectedCommandAcceptedCount { get; private set; } = -1;

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

        public static StartupCommanderScriptAction StatusRowSelect(int lineNumber, string sourceLine, string unitId)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.StatusRowSelect,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty,
                StatusRowUnitId = unitId ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction BattleClick(int lineNumber, string sourceLine, float missionX, float missionY)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.BattleClick,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty,
                MissionX = missionX,
                MissionY = missionY
            };
        }

        public static StartupCommanderScriptAction BattleTarget(int lineNumber, string sourceLine, string targetKind, string targetId)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.BattleTarget,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty,
                BattleTargetKind = targetKind ?? string.Empty,
                BattleTargetId = targetId ?? string.Empty
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

        public static StartupCommanderScriptAction CompleteVisibleObjectives(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.CompleteVisibleObjectives,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction OpenDebrief(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.OpenDebrief,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction MainServerSmoke(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.MainServerSmoke,
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

        public static StartupCommanderScriptAction SavedAccountExport(int lineNumber, string sourceLine, string filePath)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.SavedAccountExport,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty,
                FilePath = filePath
            };
        }

        public static StartupCommanderScriptAction SavedAccountImportPreview(int lineNumber, string sourceLine, string filePath)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.SavedAccountImportPreview,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty,
                FilePath = filePath
            };
        }

        public static StartupCommanderScriptAction SavedAccountImportApplyPreview(int lineNumber, string sourceLine, string filePath)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.SavedAccountImportApplyPreview,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty,
                FilePath = filePath
            };
        }

        public static StartupCommanderScriptAction SavedAccountImportApply(int lineNumber, string sourceLine, string filePath)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.SavedAccountImportApply,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty,
                FilePath = filePath
            };
        }

        public static StartupCommanderScriptAction SavedAccountLoadDefaultPreview(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.SavedAccountLoadDefaultPreview,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction SavedAccountSaveCurrentDefault(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.SavedAccountSaveCurrentDefault,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction SavedAccountLoadDefaultApply(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.SavedAccountLoadDefaultApply,
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

        public static StartupCommanderScriptAction AssertDebriefSummary(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.AssertDebriefSummary,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction AssertDebriefVisible(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.AssertDebriefVisible,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction AssertMainServerSmoke(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.AssertMainServerSmoke,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction AssertCommandResult(
            int lineNumber,
            string sourceLine,
            bool expectedBlocked,
            int expectedAcceptedCount)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.AssertCommandResult,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty,
                ExpectedCommandBlocked = expectedBlocked,
                ExpectedCommandAcceptedCount = expectedAcceptedCount
            };
        }

        public static StartupCommanderScriptAction AssertCombatSituation(
            int lineNumber,
            string sourceLine,
            string expectedTempo,
            int expectedSoloCount,
            int expectedJumpingCount,
            int expectedSalvageCount,
            string expectedSelection,
            string expectedRowUnitId,
            string expectedRowState)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.AssertCombatSituation,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty,
                ExpectedCombatTempo = expectedTempo ?? string.Empty,
                ExpectedSoloCount = expectedSoloCount,
                ExpectedJumpingCount = expectedJumpingCount,
                ExpectedSalvageCount = expectedSalvageCount,
                ExpectedSelection = expectedSelection ?? string.Empty,
                ExpectedRowUnitId = expectedRowUnitId ?? string.Empty,
                ExpectedRowState = expectedRowState ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction AssertEncounterPacing(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.AssertEncounterPacing,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction AssertObjectiveGraph(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.AssertObjectiveGraph,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction AssertLoadoutCompact(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.AssertLoadoutCompact,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }

        public static StartupCommanderScriptAction AssertAiDeputyWindow(int lineNumber, string sourceLine)
        {
            return new StartupCommanderScriptAction
            {
                Kind = StartupCommanderScriptActionKind.AssertAiDeputyWindow,
                LineNumber = lineNumber,
                SourceLine = sourceLine ?? string.Empty
            };
        }
    }

    public enum StartupCommanderScriptActionKind
    {
        None,
        Command,
        StatusRowSelect,
        BattleClick,
        BattleTarget,
        Advance,
        Report,
        Restart,
        MechBayLaunch,
        HideSquadPreview,
        CompleteVisibleObjectives,
        OpenDebrief,
        MainServerSmoke,
        SavedAccountReport,
        SavedAccountSaveLoadPreview,
        SavedAccountExport,
        SavedAccountImportPreview,
        SavedAccountImportApplyPreview,
        SavedAccountImportApply,
        SavedAccountLoadDefaultPreview,
        SavedAccountSaveCurrentDefault,
        SavedAccountLoadDefaultApply,
        PrepareDepotCandidate,
        PrepareLocalCandidate,
        SquadSwap,
        AssertRestartIdentity,
        AssertDebriefSummary,
        AssertDebriefVisible,
        AssertMainServerSmoke,
        AssertCommandResult,
        AssertCombatSituation,
        AssertEncounterPacing,
        AssertObjectiveGraph,
        AssertLoadoutCompact,
        AssertAiDeputyWindow
    }
}
