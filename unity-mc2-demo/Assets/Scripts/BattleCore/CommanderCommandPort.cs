using System;
using System.Globalization;
using UnityEngine;

namespace MC2Demo.BattleCore
{
    public sealed class CommanderCommandPort
    {
        private readonly BattleMission mission;
        private readonly float jumpDistance;
        private readonly Func<Vector2, bool> isLandingAllowed;

        public CommanderCommandPort(BattleMission mission, float jumpDistance, Func<Vector2, bool> isLandingAllowed)
        {
            this.mission = mission ?? throw new ArgumentNullException(nameof(mission));
            this.jumpDistance = jumpDistance;
            this.isLandingAllowed = isLandingAllowed;
        }

        public CommanderCommandResult Issue(CommanderCommand command)
        {
            if (command == null)
            {
                return CommanderCommandResult.Blocked("Command is empty.");
            }

            int accepted = command.Kind switch
            {
                CommanderCommandKind.Move => IssueMove(command),
                CommanderCommandKind.Jump => IssueJump(command),
                CommanderCommandKind.AttackUnit => IssueAttackUnit(command),
                CommanderCommandKind.AttackStructure => IssueAttackStructure(command),
                _ => 0
            };

            if (accepted <= 0)
            {
                return CommanderCommandResult.Blocked("Command blocked: " + command.Kind);
            }

            return CommanderCommandResult.Accept(accepted, Describe(command, accepted));
        }

        public CommanderCommandResult IssueText(string commandLine)
        {
            if (!TryParse(commandLine, out CommanderCommand command, out string error))
            {
                return CommanderCommandResult.Blocked(error);
            }

            return Issue(command);
        }

        public static bool TryParse(string commandLine, out CommanderCommand command, out string error)
        {
            command = null;
            error = "";
            if (string.IsNullOrWhiteSpace(commandLine))
            {
                error = "Command is empty.";
                return false;
            }

            string[] parts = commandLine.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                error = "Command needs a scope and action.";
                return false;
            }

            int index = 0;
            CommanderCommandScope scope;
            string unitId = null;
            if (EqualsToken(parts[index], "squad"))
            {
                scope = CommanderCommandScope.Squad;
                index++;
            }
            else if (EqualsToken(parts[index], "unit"))
            {
                if (parts.Length < 3)
                {
                    error = "Unit command needs a unit id.";
                    return false;
                }

                scope = CommanderCommandScope.Unit;
                unitId = parts[index + 1];
                index += 2;
            }
            else
            {
                error = "Command scope must be squad or unit.";
                return false;
            }

            if (index >= parts.Length)
            {
                error = "Command action is missing.";
                return false;
            }

            string action = parts[index++];
            if (EqualsToken(action, "move") || EqualsToken(action, "jump"))
            {
                if (!TryReadPoint(parts, index, out Vector2 missionPoint))
                {
                    error = "Move and jump commands need x y coordinates.";
                    return false;
                }

                command = new CommanderCommand
                {
                    Scope = scope,
                    UnitId = unitId,
                    Kind = EqualsToken(action, "move") ? CommanderCommandKind.Move : CommanderCommandKind.Jump,
                    MissionPoint = missionPoint
                };
                return true;
            }

            if (EqualsToken(action, "attack"))
            {
                if (parts.Length < index + 2)
                {
                    error = "Attack command needs target type and target id.";
                    return false;
                }

                string targetType = parts[index++];
                string targetId = parts[index];
                CommanderCommandKind kind;
                if (EqualsToken(targetType, "unit"))
                {
                    kind = CommanderCommandKind.AttackUnit;
                }
                else if (EqualsToken(targetType, "structure"))
                {
                    kind = CommanderCommandKind.AttackStructure;
                }
                else
                {
                    error = "Attack target type must be unit or structure.";
                    return false;
                }

                command = new CommanderCommand
                {
                    Scope = scope,
                    UnitId = unitId,
                    Kind = kind,
                    TargetId = targetId
                };
                return true;
            }

            error = "Unknown command action: " + action;
            return false;
        }

        public CommanderCommandResult MoveSquad(Vector2 missionPoint)
        {
            return Issue(CommanderCommand.SquadMove(missionPoint));
        }

        public CommanderCommandResult MoveUnit(string unitId, Vector2 missionPoint)
        {
            return Issue(CommanderCommand.UnitMove(unitId, missionPoint));
        }

        public CommanderCommandResult JumpSquad(Vector2 missionPoint)
        {
            return Issue(CommanderCommand.SquadJump(missionPoint));
        }

        public CommanderCommandResult JumpUnit(string unitId, Vector2 missionPoint)
        {
            return Issue(CommanderCommand.UnitJump(unitId, missionPoint));
        }

        public CommanderCommandResult AttackUnit(string unitId, string targetUnitId)
        {
            return Issue(CommanderCommand.AttackUnit(unitId, targetUnitId));
        }

        public CommanderCommandResult AttackStructure(string unitId, string structureId)
        {
            return Issue(CommanderCommand.AttackStructure(unitId, structureId));
        }

        private int IssueMove(CommanderCommand command)
        {
            return command.Scope == CommanderCommandScope.Unit
                ? mission.IssueDetachedMove(command.UnitId, command.MissionPoint)
                : mission.IssueSquadMove(command.MissionPoint);
        }

        private int IssueJump(CommanderCommand command)
        {
            return command.Scope == CommanderCommandScope.Unit
                ? mission.IssueDetachedJump(command.UnitId, command.MissionPoint, jumpDistance, isLandingAllowed)
                : mission.IssueSquadJump(command.MissionPoint, jumpDistance, isLandingAllowed);
        }

        private int IssueAttackUnit(CommanderCommand command)
        {
            return command.Scope == CommanderCommandScope.Unit
                ? mission.IssueDetachedAttackUnit(command.UnitId, command.TargetId)
                : mission.IssueSquadAttackUnit(command.TargetId);
        }

        private int IssueAttackStructure(CommanderCommand command)
        {
            return command.Scope == CommanderCommandScope.Unit
                ? mission.IssueDetachedAttackStructure(command.UnitId, command.TargetId)
                : mission.IssueSquadAttackStructure(command.TargetId);
        }

        private static string Describe(CommanderCommand command, int accepted)
        {
            string scope = command.Scope == CommanderCommandScope.Unit ? command.UnitId : "squad";
            return command.Kind switch
            {
                CommanderCommandKind.Move => scope + " move accepted.",
                CommanderCommandKind.Jump => scope + " jump accepted: " + accepted,
                CommanderCommandKind.AttackUnit => scope + " attack unit " + command.TargetId + ".",
                CommanderCommandKind.AttackStructure => scope + " attack structure " + command.TargetId + ".",
                _ => "Command accepted."
            };
        }

        private static bool TryReadPoint(string[] parts, int index, out Vector2 point)
        {
            point = Vector2.zero;
            if (parts.Length < index + 2)
            {
                return false;
            }

            if (!float.TryParse(parts[index], NumberStyles.Float, CultureInfo.InvariantCulture, out float x)
                || !float.TryParse(parts[index + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            {
                return false;
            }

            point = new Vector2(x, y);
            return true;
        }

        private static bool EqualsToken(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class CommanderCommand
    {
        public CommanderCommandKind Kind { get; set; }
        public CommanderCommandScope Scope { get; set; }
        public string UnitId { get; set; }
        public string TargetId { get; set; }
        public Vector2 MissionPoint { get; set; }

        public static CommanderCommand SquadMove(Vector2 missionPoint)
        {
            return new CommanderCommand { Kind = CommanderCommandKind.Move, Scope = CommanderCommandScope.Squad, MissionPoint = missionPoint };
        }

        public static CommanderCommand UnitMove(string unitId, Vector2 missionPoint)
        {
            return new CommanderCommand { Kind = CommanderCommandKind.Move, Scope = CommanderCommandScope.Unit, UnitId = unitId, MissionPoint = missionPoint };
        }

        public static CommanderCommand SquadJump(Vector2 missionPoint)
        {
            return new CommanderCommand { Kind = CommanderCommandKind.Jump, Scope = CommanderCommandScope.Squad, MissionPoint = missionPoint };
        }

        public static CommanderCommand UnitJump(string unitId, Vector2 missionPoint)
        {
            return new CommanderCommand { Kind = CommanderCommandKind.Jump, Scope = CommanderCommandScope.Unit, UnitId = unitId, MissionPoint = missionPoint };
        }

        public static CommanderCommand AttackUnit(string unitId, string targetUnitId)
        {
            return new CommanderCommand
            {
                Kind = CommanderCommandKind.AttackUnit,
                Scope = string.IsNullOrEmpty(unitId) ? CommanderCommandScope.Squad : CommanderCommandScope.Unit,
                UnitId = unitId,
                TargetId = targetUnitId
            };
        }

        public static CommanderCommand AttackStructure(string unitId, string structureId)
        {
            return new CommanderCommand
            {
                Kind = CommanderCommandKind.AttackStructure,
                Scope = string.IsNullOrEmpty(unitId) ? CommanderCommandScope.Squad : CommanderCommandScope.Unit,
                UnitId = unitId,
                TargetId = structureId
            };
        }
    }

    public sealed class CommanderCommandResult
    {
        public bool Accepted { get; }
        public int AcceptedCount { get; }
        public string Message { get; }

        private CommanderCommandResult(bool accepted, int acceptedCount, string message)
        {
            Accepted = accepted;
            AcceptedCount = acceptedCount;
            Message = message;
        }

        public static CommanderCommandResult Accept(int acceptedCount, string message)
        {
            return new CommanderCommandResult(true, acceptedCount, message);
        }

        public static CommanderCommandResult Blocked(string message)
        {
            return new CommanderCommandResult(false, 0, message);
        }
    }

    public enum CommanderCommandKind
    {
        Move,
        Jump,
        AttackUnit,
        AttackStructure
    }

    public enum CommanderCommandScope
    {
        Squad,
        Unit
    }
}
