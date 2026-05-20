using System;
using System.Collections.Generic;
using UnityEngine;

namespace MC2Demo.BattleCore
{
    public sealed class CommanderObservationPort
    {
        private readonly BattleMission mission;
        private int reportIndex;

        public CommanderObservationPort(BattleMission mission)
        {
            this.mission = mission ?? throw new ArgumentNullException(nameof(mission));
        }

        public CommanderObservation Observe()
        {
            reportIndex++;
            return new CommanderObservation
            {
                missionId = mission.Contract?.mission?.id ?? "",
                reportIndex = reportIndex,
                missionTimeSeconds = mission.MissionTimeSeconds,
                result = mission.Result.ToString(),
                resultReason = mission.ResultReason,
                playerUnits = PlayerUnitObservations(),
                activeHostiles = ActiveHostileObservations(),
                targetableStructures = StructureObservations(),
                currentObjectives = CurrentObjectiveObservations()
            };
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(Observe());
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
                type = unit.UnitType,
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
                weaponRange = unit.Profile.WeaponRange,
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
    }

    [Serializable]
    public sealed class CommanderObservation
    {
        public string missionId;
        public int reportIndex;
        public float missionTimeSeconds;
        public string result;
        public string resultReason;
        public CommanderUnitObservation[] playerUnits;
        public CommanderUnitObservation[] activeHostiles;
        public CommanderStructureObservation[] targetableStructures;
        public CommanderObjectiveObservation[] currentObjectives;
    }

    [Serializable]
    public sealed class CommanderUnitObservation
    {
        public string id;
        public string type;
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
