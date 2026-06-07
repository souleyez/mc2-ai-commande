using System;
using System.Collections.Generic;
using UnityEngine;

namespace MC2Demo.BattleCore
{
    public sealed class CommanderObservationPort
    {
        public const string CompactObservationSchema = "mc2-ai-observation-compact-v1";
        private const int MaxCompactPlayerUnits = 6;
        private const int MaxCompactThreats = 3;
        private const float NearbyThreatRange = 900f;

        private readonly BattleMission mission;
        private int reportIndex;

        public CommanderObservationPort(BattleMission mission)
        {
            this.mission = mission ?? throw new ArgumentNullException(nameof(mission));
        }

        public CommanderObservation Observe()
        {
            reportIndex++;
            CommanderUnitObservation[] playerUnits = PlayerUnitObservations();
            CommanderUnitObservation[] activeHostiles = ActiveHostileObservations();
            CommanderStructureObservation[] targetableStructures = StructureObservations();
            CommanderObjectiveObservation[] currentObjectives = CurrentObjectiveObservations();
            return new CommanderObservation
            {
                missionId = mission.Contract?.mission?.id ?? "",
                reportIndex = reportIndex,
                missionTimeSeconds = mission.MissionTimeSeconds,
                result = mission.Result.ToString(),
                resultReason = mission.ResultReason,
                missionEnded = mission.Result != MissionResultState.InProgress,
                resultSummary = mission.ResultSummary,
                playerUnits = playerUnits,
                activeHostiles = activeHostiles,
                targetableStructures = targetableStructures,
                currentObjectives = currentObjectives,
                compact = BuildCompactObservation(playerUnits, activeHostiles, targetableStructures, currentObjectives)
            };
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(Observe());
        }

        public string ToCompactJson()
        {
            return JsonUtility.ToJson(Observe().compact);
        }

        private CommanderUnitObservation[] PlayerUnitObservations()
        {
            List<CommanderUnitObservation> observations = new();
            foreach (UnitState unit in mission.Units)
            {
                if (unit.IsPlayerUnit)
                {
                    observations.Add(UnitObservation(unit));
                }
            }

            return observations.ToArray();
        }

        private CommanderUnitObservation[] ActiveHostileObservations()
        {
            List<CommanderUnitObservation> observations = new();
            foreach (UnitState unit in mission.Units)
            {
                if (!unit.IsPlayerUnit && unit.IsActive && !unit.IsDestroyed)
                {
                    observations.Add(UnitObservation(unit));
                }
            }

            return observations.ToArray();
        }

        private CommanderStructureObservation[] StructureObservations()
        {
            List<CommanderStructureObservation> observations = new();
            foreach (StructureState structure in mission.Structures)
            {
                if (!structure.IsTargetable)
                {
                    continue;
                }

                observations.Add(new CommanderStructureObservation
                {
                    id = structure.Id,
                    type = structure.ObjectType,
                    teamId = structure.TeamId,
                    objectiveTarget = structure.IsObjectiveTarget,
                    destroyed = structure.IsDestroyed,
                    structureRatio = structure.Structure,
                    x = structure.MissionPosition.x,
                    y = structure.MissionPosition.y,
                    radius = structure.Radius
                });
            }

            return observations.ToArray();
        }

        private CommanderObjectiveObservation[] CurrentObjectiveObservations()
        {
            List<CommanderObjectiveObservation> observations = new();
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (objective.Definition.hidden || !objective.IsActive || objective.IsComplete)
                {
                    continue;
                }

                observations.Add(new CommanderObjectiveObservation
                {
                    index = objective.Definition.index,
                    title = objective.Definition.title,
                    markerX = objective.Definition.marker == null ? 0f : objective.Definition.marker.x,
                    markerY = objective.Definition.marker == null ? 0f : objective.Definition.marker.y,
                    targetUnitIds = ObjectiveTargetUnitIds(objective.Definition),
                    targetStructureIds = ObjectiveTargetStructureIds(objective.Definition),
                    targetPoints = ObjectiveTargetPoints(objective.Definition)
                });
            }

            return observations.ToArray();
        }

        private string[] ObjectiveTargetUnitIds(ObjectiveDefinition objective)
        {
            List<string> ids = new();
            if (objective.conditions == null)
            {
                return ids.ToArray();
            }

            foreach (ObjectiveCondition condition in objective.conditions)
            {
                if (condition?.type != "DestroySpecificEnemyUnit" || condition.targetUnit?.position == null)
                {
                    continue;
                }

                UnitState unit = FindUnitNearSpawnPosition(new Vector2(condition.targetUnit.position.x, condition.targetUnit.position.y), 75f);
                if (unit != null && !ContainsString(ids, unit.Id))
                {
                    ids.Add(unit.Id);
                }
            }

            return ids.ToArray();
        }

        private string[] ObjectiveTargetStructureIds(ObjectiveDefinition objective)
        {
            List<string> ids = new();
            if (objective.conditions == null)
            {
                return ids.ToArray();
            }

            foreach (ObjectiveCondition condition in objective.conditions)
            {
                if (condition?.type != "DestroySpecificStructure" || condition.targetStructure?.position == null)
                {
                    continue;
                }

                StructureState structure = FindStructureNearPosition(new Vector2(condition.targetStructure.position.x, condition.targetStructure.position.y), 220f);
                if (structure != null && !ContainsString(ids, structure.Id))
                {
                    ids.Add(structure.Id);
                }
            }

            return ids.ToArray();
        }

        private CommanderTargetPointObservation[] ObjectiveTargetPoints(ObjectiveDefinition objective)
        {
            List<CommanderTargetPointObservation> points = new();
            if (objective.conditions == null)
            {
                return points.ToArray();
            }

            foreach (ObjectiveCondition condition in objective.conditions)
            {
                if (condition == null)
                {
                    continue;
                }

                if (condition.type == "MoveAnyUnitToArea" || condition.type == "MoveAllSurvivingMechsToArea")
                {
                    points.Add(new CommanderTargetPointObservation
                    {
                        kind = "area",
                        id = "",
                        x = condition.targetArea.x,
                        y = condition.targetArea.y,
                        radius = condition.targetArea.radius
                    });
                    continue;
                }

                if (condition.type == "DestroySpecificStructure" && condition.targetStructure?.position != null)
                {
                    StructureState structure = FindStructureNearPosition(new Vector2(condition.targetStructure.position.x, condition.targetStructure.position.y), 220f);
                    points.Add(new CommanderTargetPointObservation
                    {
                        kind = "structure",
                        id = structure == null ? "" : structure.Id,
                        x = condition.targetStructure.position.x,
                        y = condition.targetStructure.position.y,
                        radius = structure == null ? 0f : structure.Radius
                    });
                    continue;
                }

                if (condition.type == "DestroySpecificEnemyUnit" && condition.targetUnit?.position != null)
                {
                    UnitState unit = FindUnitNearSpawnPosition(new Vector2(condition.targetUnit.position.x, condition.targetUnit.position.y), 75f);
                    points.Add(new CommanderTargetPointObservation
                    {
                        kind = "unit",
                        id = unit == null ? "" : unit.Id,
                        x = condition.targetUnit.position.x,
                        y = condition.targetUnit.position.y,
                        radius = 0f
                    });
                }
            }

            return points.ToArray();
        }

        private UnitState FindUnitNearSpawnPosition(Vector2 missionPoint, float tolerance)
        {
            UnitState best = null;
            float bestDistanceSqr = tolerance * tolerance;
            foreach (UnitState unit in mission.Units)
            {
                float distanceSqr = (unit.SpawnPosition - missionPoint).sqrMagnitude;
                if (distanceSqr <= bestDistanceSqr)
                {
                    best = unit;
                    bestDistanceSqr = distanceSqr;
                }
            }

            return best;
        }

        private StructureState FindStructureNearPosition(Vector2 missionPoint, float tolerance)
        {
            StructureState best = null;
            float bestDistanceSqr = tolerance * tolerance;
            foreach (StructureState structure in mission.Structures)
            {
                float distanceSqr = (structure.MissionPosition - missionPoint).sqrMagnitude;
                if (distanceSqr <= bestDistanceSqr)
                {
                    best = structure;
                    bestDistanceSqr = distanceSqr;
                }
            }

            return best;
        }

        private static bool ContainsString(List<string> values, string value)
        {
            foreach (string item in values)
            {
                if (string.Equals(item, value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static CommanderUnitObservation UnitObservation(UnitState unit)
        {
            return new CommanderUnitObservation
            {
                id = unit.Id,
                ownedMechId = unit.OwnedMechId ?? "",
                type = unit.UnitType,
                pilotDisplayName = unit.PilotDisplayName ?? "",
                activeLoadoutId = unit.ActiveLoadoutId ?? "",
                teamId = unit.TeamId,
                active = unit.IsActive,
                destroyed = unit.IsDestroyed,
                detached = unit.IsDetached,
                moving = unit.HasMoveOrder,
                jumping = unit.IsJumping,
                heatLocked = unit.IsHeatLocked,
                attackTargetId = unit.AttackTargetId ?? "",
                currentTargetId = unit.CurrentTargetId ?? "",
                structureRatio = unit.Structure,
                heatRatio = unit.HeatRatio,
                weaponReadyRatio = unit.WeaponReadinessRatio,
                weaponRange = unit.CombatWeaponRange,
                x = unit.MissionPosition.x,
                y = unit.MissionPosition.y,
                moveTargetX = unit.MoveTarget.x,
                moveTargetY = unit.MoveTarget.y,
                sections = SectionObservations(unit)
            };
        }

        private static CommanderSectionObservation[] SectionObservations(UnitState unit)
        {
            CommanderSectionObservation[] sections = new CommanderSectionObservation[unit.Sections.Length];
            for (int index = 0; index < unit.Sections.Length; index++)
            {
                DamageSection section = unit.Sections[index];
                sections[index] = new CommanderSectionObservation
                {
                    name = section.Name,
                    ratio = section.Ratio,
                    critical = section.IsCritical,
                    destroyed = section.IsDestroyed
                };
            }

            return sections;
        }

        private CommanderCompactObservation BuildCompactObservation(
            CommanderUnitObservation[] playerUnits,
            CommanderUnitObservation[] activeHostiles,
            CommanderStructureObservation[] targetableStructures,
            CommanderObjectiveObservation[] currentObjectives)
        {
            CommanderUnitObservation commander = FirstActivePlayer(playerUnits) ?? FirstPlayer(playerUnits);
            int activePlayers = 0;
            int damagedPlayers = 0;
            int detachedPlayers = 0;
            int destroyedPlayers = 0;
            int heatLockedPlayers = 0;
            float totalStructure = 0f;
            float maxHeat = 0f;
            CommanderCompactUnitObservation[] compactPlayers = CompactPlayerUnits(playerUnits);
            for (int index = 0; index < (playerUnits?.Length ?? 0); index++)
            {
                CommanderUnitObservation unit = playerUnits[index];
                if (unit == null)
                {
                    continue;
                }

                if (unit.active && !unit.destroyed)
                {
                    activePlayers++;
                }

                if (unit.destroyed)
                {
                    destroyedPlayers++;
                }

                if (unit.detached)
                {
                    detachedPlayers++;
                }

                if (unit.heatLocked)
                {
                    heatLockedPlayers++;
                }

                if (unit.structureRatio < 0.995f || HasDamagedSection(unit))
                {
                    damagedPlayers++;
                }

                totalStructure += Mathf.Clamp01(unit.structureRatio);
                maxHeat = Mathf.Max(maxHeat, Mathf.Clamp01(unit.heatRatio));
            }

            CommanderCompactObjectiveObservation objective = CompactObjective(currentObjectives, playerUnits);
            CommanderCompactThreatObservation[] threats = CompactThreats(playerUnits, activeHostiles);
            int hostileCount = activeHostiles?.Length ?? 0;
            int nearbyThreatCount = CountNearbyThreats(playerUnits, activeHostiles);
            int inRangeThreatCount = CountThreatsInRange(playerUnits, activeHostiles);
            return new CommanderCompactObservation
            {
                schema = CompactObservationSchema,
                missionId = mission.Contract?.mission?.id ?? "",
                reportIndex = reportIndex,
                missionPhase = MissionPhase(hostileCount, nearbyThreatCount, inRangeThreatCount),
                missionTimeSeconds = Mathf.RoundToInt(mission.MissionTimeSeconds),
                missionEnded = mission.Result != MissionResultState.InProgress,
                result = mission.Result.ToString(),
                commanderUnitId = commander?.id ?? "",
                commanderOwnedMechId = commander?.ownedMechId ?? "",
                commanderType = commander?.type ?? "",
                playerUnitCount = playerUnits?.Length ?? 0,
                activePlayerUnitCount = activePlayers,
                damagedPlayerUnitCount = damagedPlayers,
                detachedPlayerUnitCount = detachedPlayers,
                destroyedPlayerUnitCount = destroyedPlayers,
                heatLockedPlayerUnitCount = heatLockedPlayers,
                averagePlayerStructurePercent = playerUnits == null || playerUnits.Length == 0
                    ? 0
                    : Mathf.RoundToInt((totalStructure / playerUnits.Length) * 100f),
                hottestPlayerHeatPercent = Mathf.RoundToInt(maxHeat * 100f),
                hostileCount = hostileCount,
                nearbyThreatCount = nearbyThreatCount,
                inRangeThreatCount = inRangeThreatCount,
                threatLevel = ThreatLevel(hostileCount, nearbyThreatCount, inRangeThreatCount),
                targetableStructureCount = targetableStructures?.Length ?? 0,
                currentObjectiveCount = currentObjectives?.Length ?? 0,
                objective = objective,
                playerStates = compactPlayers,
                nearbyThreats = threats,
                availableIntents = new[]
                {
                    RuleCommander.DirectiveAssaultObjective,
                    RuleCommander.DirectiveEngageHostiles,
                    RuleCommander.DirectiveRegroup,
                    RuleCommander.DirectiveWithdrawIfCritical,
                    RuleCommander.DirectiveHold
                },
                detailBudget = "phase-summary-only"
            };
        }

        private static CommanderCompactUnitObservation[] CompactPlayerUnits(CommanderUnitObservation[] playerUnits)
        {
            playerUnits ??= Array.Empty<CommanderUnitObservation>();
            int count = Math.Min(MaxCompactPlayerUnits, playerUnits.Length);
            CommanderCompactUnitObservation[] result = new CommanderCompactUnitObservation[count];
            for (int index = 0; index < count; index++)
            {
                CommanderUnitObservation unit = playerUnits[index];
                result[index] = new CommanderCompactUnitObservation
                {
                    id = unit?.id ?? "",
                    type = unit?.type ?? "",
                    role = index == 0 ? "commander" : "lancemate",
                    active = unit?.active == true,
                    destroyed = unit?.destroyed == true,
                    detached = unit?.detached == true,
                    moving = unit?.moving == true,
                    jumping = unit?.jumping == true,
                    heatLocked = unit?.heatLocked == true,
                    structurePercent = Mathf.RoundToInt(Mathf.Clamp01(unit?.structureRatio ?? 0f) * 100f),
                    heatPercent = Mathf.RoundToInt(Mathf.Clamp01(unit?.heatRatio ?? 0f) * 100f),
                    weaponReadyPercent = Mathf.RoundToInt(Mathf.Clamp01(unit?.weaponReadyRatio ?? 0f) * 100f),
                    sectionDamage = SectionDamageSummary(unit)
                };
            }

            return result;
        }

        private static CommanderCompactObjectiveObservation CompactObjective(
            CommanderObjectiveObservation[] objectives,
            CommanderUnitObservation[] playerUnits)
        {
            CommanderObjectiveObservation objective = null;
            for (int index = 0; index < (objectives?.Length ?? 0); index++)
            {
                if (objectives[index] != null)
                {
                    objective = objectives[index];
                    break;
                }
            }

            if (objective == null)
            {
                return new CommanderCompactObjectiveObservation
                {
                    index = -1,
                    title = "none",
                    targetKind = "none",
                    targetCount = 0
                };
            }

            CommanderTargetPointObservation point = FirstTargetPoint(objective);
            return new CommanderCompactObjectiveObservation
            {
                index = objective.index,
                title = objective.title ?? "",
                targetKind = point?.kind ?? "marker",
                targetCount = (objective.targetUnitIds?.Length ?? 0)
                    + (objective.targetStructureIds?.Length ?? 0)
                    + (objective.targetPoints?.Length ?? 0),
                rangeHint = TargetRangeHint(playerUnits, point?.x ?? objective.markerX, point?.y ?? objective.markerY, point?.radius ?? 0f)
            };
        }

        private static CommanderCompactThreatObservation[] CompactThreats(
            CommanderUnitObservation[] playerUnits,
            CommanderUnitObservation[] activeHostiles)
        {
            activeHostiles ??= Array.Empty<CommanderUnitObservation>();
            List<CommanderCompactThreatObservation> threats = new();
            bool[] used = new bool[activeHostiles.Length];
            for (int emitted = 0; emitted < MaxCompactThreats; emitted++)
            {
                int bestIndex = -1;
                float bestDistance = float.MaxValue;
                bool bestInRange = false;
                for (int index = 0; index < activeHostiles.Length; index++)
                {
                    if (used[index])
                    {
                        continue;
                    }

                    CommanderUnitObservation hostile = activeHostiles[index];
                    if (hostile == null || hostile.destroyed || !hostile.active)
                    {
                        continue;
                    }

                    float distance = DistanceToClosestPlayer(playerUnits, hostile.x, hostile.y);
                    if (distance < bestDistance)
                    {
                        bestIndex = index;
                        bestDistance = distance;
                        bestInRange = IsThreatInRange(playerUnits, hostile);
                    }
                }

                if (bestIndex < 0)
                {
                    break;
                }

                used[bestIndex] = true;
                CommanderUnitObservation best = activeHostiles[bestIndex];
                threats.Add(new CommanderCompactThreatObservation
                {
                    type = best.type ?? "",
                    distanceBand = DistanceBand(bestDistance),
                    inWeaponRange = bestInRange,
                    structurePercent = Mathf.RoundToInt(Mathf.Clamp01(best.structureRatio) * 100f)
                });
            }

            return threats.ToArray();
        }

        private string MissionPhase(int hostileCount, int nearbyThreatCount, int inRangeThreatCount)
        {
            if (mission.Result != MissionResultState.InProgress)
            {
                return "ended";
            }

            if (inRangeThreatCount > 0)
            {
                return "engaged";
            }

            if (nearbyThreatCount > 0 || hostileCount > 0)
            {
                return "contact";
            }

            return "maneuver";
        }

        private static string ThreatLevel(int hostileCount, int nearbyThreatCount, int inRangeThreatCount)
        {
            if (hostileCount <= 0)
            {
                return "none";
            }

            if (inRangeThreatCount >= 2 || nearbyThreatCount >= 4)
            {
                return "high";
            }

            if (inRangeThreatCount > 0 || nearbyThreatCount > 0)
            {
                return "medium";
            }

            return "low";
        }

        private static CommanderUnitObservation FirstPlayer(CommanderUnitObservation[] playerUnits)
        {
            return playerUnits != null && playerUnits.Length > 0 ? playerUnits[0] : null;
        }

        private static CommanderUnitObservation FirstActivePlayer(CommanderUnitObservation[] playerUnits)
        {
            for (int index = 0; index < (playerUnits?.Length ?? 0); index++)
            {
                CommanderUnitObservation unit = playerUnits[index];
                if (unit != null && unit.active && !unit.destroyed)
                {
                    return unit;
                }
            }

            return null;
        }

        private static CommanderTargetPointObservation FirstTargetPoint(CommanderObjectiveObservation objective)
        {
            CommanderTargetPointObservation[] points = objective?.targetPoints ?? Array.Empty<CommanderTargetPointObservation>();
            for (int index = 0; index < points.Length; index++)
            {
                if (points[index] != null)
                {
                    return points[index];
                }
            }

            return null;
        }

        private static bool HasDamagedSection(CommanderUnitObservation unit)
        {
            CommanderSectionObservation[] sections = unit?.sections ?? Array.Empty<CommanderSectionObservation>();
            for (int index = 0; index < sections.Length; index++)
            {
                if (sections[index] != null && sections[index].ratio < 0.995f)
                {
                    return true;
                }
            }

            return false;
        }

        private static string SectionDamageSummary(CommanderUnitObservation unit)
        {
            CommanderSectionObservation[] sections = unit?.sections ?? Array.Empty<CommanderSectionObservation>();
            string text = "";
            for (int index = 0; index < sections.Length; index++)
            {
                CommanderSectionObservation section = sections[index];
                if (section == null || section.ratio >= 0.995f)
                {
                    continue;
                }

                if (text.Length > 0)
                {
                    text += ",";
                }

                text += ShortSectionName(section.name)
                    + (section.destroyed ? ":destroyed" : ":" + Mathf.RoundToInt(Mathf.Clamp01(section.ratio) * 100f).ToString());
            }

            return string.IsNullOrWhiteSpace(text) ? "OK" : text;
        }

        private static string ShortSectionName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "?";
            }

            string normalized = name.Trim().ToLowerInvariant();
            if (normalized.Contains("cockpit"))
            {
                return "CP";
            }

            if (normalized.Contains("left"))
            {
                return "LA";
            }

            if (normalized.Contains("right"))
            {
                return "RA";
            }

            if (normalized.Contains("leg"))
            {
                return "LG";
            }

            if (normalized.Contains("torso"))
            {
                return "T";
            }

            return name.Length <= 3 ? name : name.Substring(0, 3);
        }

        private static int CountNearbyThreats(CommanderUnitObservation[] playerUnits, CommanderUnitObservation[] activeHostiles)
        {
            int count = 0;
            for (int index = 0; index < (activeHostiles?.Length ?? 0); index++)
            {
                CommanderUnitObservation hostile = activeHostiles[index];
                if (hostile != null && DistanceToClosestPlayer(playerUnits, hostile.x, hostile.y) <= NearbyThreatRange)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountThreatsInRange(CommanderUnitObservation[] playerUnits, CommanderUnitObservation[] activeHostiles)
        {
            int count = 0;
            for (int index = 0; index < (activeHostiles?.Length ?? 0); index++)
            {
                CommanderUnitObservation hostile = activeHostiles[index];
                if (hostile != null && IsThreatInRange(playerUnits, hostile))
                {
                    count++;
                }
            }

            return count;
        }

        private static bool IsThreatInRange(CommanderUnitObservation[] playerUnits, CommanderUnitObservation hostile)
        {
            for (int index = 0; index < (playerUnits?.Length ?? 0); index++)
            {
                CommanderUnitObservation player = playerUnits[index];
                if (player == null || !player.active || player.destroyed)
                {
                    continue;
                }

                float range = Mathf.Max(0f, player.weaponRange);
                if (range <= 0f)
                {
                    continue;
                }

                float dx = player.x - hostile.x;
                float dy = player.y - hostile.y;
                if ((dx * dx) + (dy * dy) <= range * range)
                {
                    return true;
                }
            }

            return false;
        }

        private static float DistanceToClosestPlayer(CommanderUnitObservation[] playerUnits, float x, float y)
        {
            float best = float.MaxValue;
            for (int index = 0; index < (playerUnits?.Length ?? 0); index++)
            {
                CommanderUnitObservation player = playerUnits[index];
                if (player == null || !player.active || player.destroyed)
                {
                    continue;
                }

                float dx = player.x - x;
                float dy = player.y - y;
                best = Mathf.Min(best, Mathf.Sqrt((dx * dx) + (dy * dy)));
            }

            return best == float.MaxValue ? 99999f : best;
        }

        private static string DistanceBand(float distance)
        {
            if (distance <= 350f)
            {
                return "close";
            }

            if (distance <= NearbyThreatRange)
            {
                return "near";
            }

            return "far";
        }

        private static string TargetRangeHint(CommanderUnitObservation[] playerUnits, float x, float y, float radius)
        {
            float distance = DistanceToClosestPlayer(playerUnits, x, y);
            if (distance >= 99999f)
            {
                return radius > 0f ? "area" : "objective";
            }

            return DistanceBand(Mathf.Max(0f, distance - radius));
        }
    }

    [Serializable]
    public sealed class CommanderObservation
    {
        public string missionId;
        public int reportIndex;
        public float missionTimeSeconds;
        public string result;
        public string resultReason;
        public bool missionEnded;
        public MissionResultSummary resultSummary;
        public CommanderUnitObservation[] playerUnits;
        public CommanderUnitObservation[] activeHostiles;
        public CommanderStructureObservation[] targetableStructures;
        public CommanderObjectiveObservation[] currentObjectives;
        public CommanderCompactObservation compact;
    }

    [Serializable]
    public sealed class CommanderCompactObservation
    {
        public string schema;
        public string missionId;
        public int reportIndex;
        public string missionPhase;
        public int missionTimeSeconds;
        public bool missionEnded;
        public string result;
        public string commanderUnitId;
        public string commanderOwnedMechId;
        public string commanderType;
        public int playerUnitCount;
        public int activePlayerUnitCount;
        public int damagedPlayerUnitCount;
        public int detachedPlayerUnitCount;
        public int destroyedPlayerUnitCount;
        public int heatLockedPlayerUnitCount;
        public int averagePlayerStructurePercent;
        public int hottestPlayerHeatPercent;
        public int hostileCount;
        public int nearbyThreatCount;
        public int inRangeThreatCount;
        public string threatLevel;
        public int targetableStructureCount;
        public int currentObjectiveCount;
        public CommanderCompactObjectiveObservation objective;
        public CommanderCompactUnitObservation[] playerStates;
        public CommanderCompactThreatObservation[] nearbyThreats;
        public string[] availableIntents;
        public string detailBudget;
    }

    [Serializable]
    public sealed class CommanderCompactObjectiveObservation
    {
        public int index;
        public string title;
        public string targetKind;
        public int targetCount;
        public string rangeHint;
    }

    [Serializable]
    public sealed class CommanderCompactUnitObservation
    {
        public string id;
        public string type;
        public string role;
        public bool active;
        public bool destroyed;
        public bool detached;
        public bool moving;
        public bool jumping;
        public bool heatLocked;
        public int structurePercent;
        public int heatPercent;
        public int weaponReadyPercent;
        public string sectionDamage;
    }

    [Serializable]
    public sealed class CommanderCompactThreatObservation
    {
        public string type;
        public string distanceBand;
        public bool inWeaponRange;
        public int structurePercent;
    }

    [Serializable]
    public sealed class CommanderUnitObservation
    {
        public string id;
        public string ownedMechId;
        public string type;
        public string pilotDisplayName;
        public string activeLoadoutId;
        public int teamId;
        public bool active;
        public bool destroyed;
        public bool detached;
        public bool moving;
        public bool jumping;
        public bool heatLocked;
        public string attackTargetId;
        public string currentTargetId;
        public float structureRatio;
        public float heatRatio;
        public float weaponReadyRatio;
        public float weaponRange;
        public float x;
        public float y;
        public float moveTargetX;
        public float moveTargetY;
        public CommanderSectionObservation[] sections;
    }

    [Serializable]
    public sealed class CommanderSectionObservation
    {
        public string name;
        public float ratio;
        public bool critical;
        public bool destroyed;
    }

    [Serializable]
    public sealed class CommanderStructureObservation
    {
        public string id;
        public string type;
        public int teamId;
        public bool objectiveTarget;
        public bool destroyed;
        public float structureRatio;
        public float x;
        public float y;
        public float radius;
    }

    [Serializable]
    public sealed class CommanderObjectiveObservation
    {
        public int index;
        public string title;
        public float markerX;
        public float markerY;
        public string[] targetUnitIds;
        public string[] targetStructureIds;
        public CommanderTargetPointObservation[] targetPoints;
    }

    [Serializable]
    public sealed class CommanderTargetPointObservation
    {
        public string kind;
        public string id;
        public float x;
        public float y;
        public float radius;
    }
}
