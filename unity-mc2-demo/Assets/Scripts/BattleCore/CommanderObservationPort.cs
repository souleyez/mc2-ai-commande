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
                    markerY = objective.Definition.marker == null ? 0f : objective.Definition.marker.y
                });
            }

            return observations.ToArray();
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
    }
}
