using System;
using System.Globalization;
using UnityEngine;

namespace MC2Demo.BattleCore
{
    public sealed class RuleCommander
    {
        public const string DirectiveAssaultObjective = "assault-objective";
        public const string DirectiveEngageHostiles = "engage-hostiles";
        public const string DirectiveRegroup = "regroup";
        public const string DirectiveHold = "hold";

        public string ChooseCommand(CommanderObservation observation)
        {
            if (observation == null || observation.missionEnded)
            {
                return "";
            }

            CommanderUnitObservation hostile = ClosestHostileInRange(observation);
            if (hostile != null)
            {
                return "squad attack unit " + hostile.id;
            }

            CommanderStructureObservation structure = CurrentStructureTarget(observation);
            if (structure != null)
            {
                return IsStructureInRange(observation.playerUnits, structure)
                    ? "squad attack structure " + structure.id
                    : MoveCommand(structure.x, structure.y);
            }

            CommanderTargetPointObservation targetPoint = CurrentObjectiveTargetPoint(observation);
            if (targetPoint != null)
            {
                return MoveCommand(targetPoint.x, targetPoint.y);
            }

            CommanderObjectiveObservation objective = FirstCurrentObjective(observation);
            return objective == null ? "" : MoveCommand(objective.markerX, objective.markerY);
        }

        public string ChooseCommandForDirective(CommanderObservation observation, string directive)
        {
            if (observation == null || observation.missionEnded)
            {
                return "";
            }

            switch (NormalizeDirective(directive))
            {
                case DirectiveHold:
                    return "";
                case DirectiveRegroup:
                    return RegroupCommand(observation);
                case DirectiveEngageHostiles:
                    CommanderUnitObservation hostile = ClosestHostileInRange(observation);
                    return hostile == null ? ChooseCommand(observation) : "squad attack unit " + hostile.id;
                case DirectiveAssaultObjective:
                default:
                    return ChooseCommand(observation);
            }
        }

        public static string NormalizeDirective(string directive)
        {
            if (string.IsNullOrWhiteSpace(directive))
            {
                return DirectiveAssaultObjective;
            }

            string normalized = directive.Trim().ToLowerInvariant().Replace('_', '-').Replace(' ', '-');
            return normalized switch
            {
                DirectiveEngageHostiles => DirectiveEngageHostiles,
                DirectiveRegroup => DirectiveRegroup,
                DirectiveHold => DirectiveHold,
                _ => DirectiveAssaultObjective
            };
        }

        private static CommanderUnitObservation ClosestHostileInRange(CommanderObservation observation)
        {
            CommanderUnitObservation best = null;
            float bestDistanceSqr = float.MaxValue;
            CommanderUnitObservation[] hostiles = observation.activeHostiles ?? Array.Empty<CommanderUnitObservation>();
            CommanderUnitObservation[] players = observation.playerUnits ?? Array.Empty<CommanderUnitObservation>();

            for (int hostileIndex = 0; hostileIndex < hostiles.Length; hostileIndex++)
            {
                CommanderUnitObservation hostile = hostiles[hostileIndex];
                if (hostile == null || hostile.destroyed || !hostile.active)
                {
                    continue;
                }

                float distanceSqr = DistanceToClosestReadyPlayerSqr(players, hostile.x, hostile.y, requireWeaponRange: true, radius: 0f);
                if (distanceSqr == float.MaxValue)
                {
                    continue;
                }

                if (distanceSqr < bestDistanceSqr || (Mathf.Approximately(distanceSqr, bestDistanceSqr) && SortId(hostile.id, best?.id) < 0))
                {
                    best = hostile;
                    bestDistanceSqr = distanceSqr;
                }
            }

            return bestDistanceSqr == float.MaxValue ? null : best;
        }

        private static CommanderStructureObservation CurrentStructureTarget(CommanderObservation observation)
        {
            CommanderObjectiveObservation[] objectives = observation.currentObjectives ?? Array.Empty<CommanderObjectiveObservation>();
            for (int objectiveIndex = 0; objectiveIndex < objectives.Length; objectiveIndex++)
            {
                CommanderObjectiveObservation objective = objectives[objectiveIndex];
                if (objective == null || objective.targetStructureIds == null)
                {
                    continue;
                }

                for (int targetIndex = 0; targetIndex < objective.targetStructureIds.Length; targetIndex++)
                {
                    CommanderStructureObservation structure = FindStructure(observation, objective.targetStructureIds[targetIndex]);
                    if (structure != null)
                    {
                        return structure;
                    }
                }
            }

            return null;
        }

        private static CommanderTargetPointObservation CurrentObjectiveTargetPoint(CommanderObservation observation)
        {
            CommanderObjectiveObservation[] objectives = observation.currentObjectives ?? Array.Empty<CommanderObjectiveObservation>();
            for (int objectiveIndex = 0; objectiveIndex < objectives.Length; objectiveIndex++)
            {
                CommanderObjectiveObservation objective = objectives[objectiveIndex];
                if (objective?.targetPoints == null)
                {
                    continue;
                }

                CommanderTargetPointObservation bestMovePoint = null;
                for (int pointIndex = 0; pointIndex < objective.targetPoints.Length; pointIndex++)
                {
                    CommanderTargetPointObservation point = objective.targetPoints[pointIndex];
                    if (point == null)
                    {
                        continue;
                    }

                    if (string.Equals(point.kind, "structure", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(point.id))
                    {
                        CommanderStructureObservation structure = FindStructure(observation, point.id);
                        if (structure != null)
                        {
                            return new CommanderTargetPointObservation
                            {
                                kind = point.kind,
                                id = point.id,
                                x = structure.x,
                                y = structure.y,
                                radius = structure.radius
                            };
                        }
                    }

                    bestMovePoint ??= point;
                }

                if (bestMovePoint != null)
                {
                    return bestMovePoint;
                }
            }

            return null;
        }

        private static CommanderObjectiveObservation FirstCurrentObjective(CommanderObservation observation)
        {
            CommanderObjectiveObservation[] objectives = observation.currentObjectives ?? Array.Empty<CommanderObjectiveObservation>();
            for (int index = 0; index < objectives.Length; index++)
            {
                if (objectives[index] != null)
                {
                    return objectives[index];
                }
            }

            return null;
        }

        private static CommanderStructureObservation FindStructure(CommanderObservation observation, string structureId)
        {
            if (string.IsNullOrEmpty(structureId))
            {
                return null;
            }

            CommanderStructureObservation[] structures = observation.targetableStructures ?? Array.Empty<CommanderStructureObservation>();
            for (int index = 0; index < structures.Length; index++)
            {
                CommanderStructureObservation structure = structures[index];
                if (structure != null
                    && !structure.destroyed
                    && string.Equals(structure.id, structureId, StringComparison.Ordinal))
                {
                    return structure;
                }
            }

            return null;
        }

        private static bool IsStructureInRange(CommanderUnitObservation[] players, CommanderStructureObservation structure)
        {
            return DistanceToClosestReadyPlayerSqr(players, structure.x, structure.y, requireWeaponRange: true, radius: structure.radius) < float.MaxValue;
        }

        private static float DistanceToClosestReadyPlayerSqr(CommanderUnitObservation[] players, float x, float y, bool requireWeaponRange, float radius)
        {
            players ??= Array.Empty<CommanderUnitObservation>();
            float bestDistanceSqr = float.MaxValue;
            for (int index = 0; index < players.Length; index++)
            {
                CommanderUnitObservation player = players[index];
                if (player == null || !player.active || player.destroyed)
                {
                    continue;
                }

                float dx = player.x - x;
                float dy = player.y - y;
                float distanceSqr = dx * dx + dy * dy;
                if (requireWeaponRange)
                {
                    float range = Mathf.Max(0f, player.weaponRange + radius);
                    if (distanceSqr > range * range)
                    {
                        continue;
                    }
                }

                bestDistanceSqr = Mathf.Min(bestDistanceSqr, distanceSqr);
            }

            return bestDistanceSqr;
        }

        private static string RegroupCommand(CommanderObservation observation)
        {
            CommanderUnitObservation[] players = observation.playerUnits ?? Array.Empty<CommanderUnitObservation>();
            for (int index = 0; index < players.Length; index++)
            {
                CommanderUnitObservation player = players[index];
                if (player != null && player.active && !player.destroyed)
                {
                    return MoveCommand(player.x, player.y);
                }
            }

            return ChooseObjectiveCommand(observation);
        }

        private static string ChooseObjectiveCommand(CommanderObservation observation)
        {
            CommanderStructureObservation structure = CurrentStructureTarget(observation);
            if (structure != null)
            {
                return MoveCommand(structure.x, structure.y);
            }

            CommanderTargetPointObservation targetPoint = CurrentObjectiveTargetPoint(observation);
            if (targetPoint != null)
            {
                return MoveCommand(targetPoint.x, targetPoint.y);
            }

            CommanderObjectiveObservation objective = FirstCurrentObjective(observation);
            return objective == null ? "" : MoveCommand(objective.markerX, objective.markerY);
        }

        private static string MoveCommand(float x, float y)
        {
            return "squad move " + FormatNumber(x) + " " + FormatNumber(y);
        }

        private static string FormatNumber(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static int SortId(string left, string right)
        {
            if (right == null)
            {
                return -1;
            }

            return string.Compare(left, right, StringComparison.Ordinal);
        }
    }
}
