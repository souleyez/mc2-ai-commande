using System;
using System.Collections.Generic;
using UnityEngine;

namespace MC2Demo.BattleCore
{
    public sealed class BattleMission
    {
        public MissionContract Contract { get; }
        public IReadOnlyList<UnitState> Units => units;
        public IReadOnlyList<StructureState> Structures => structures;
        public IReadOnlyList<ObjectiveState> Objectives => objectives;
        public IReadOnlyList<CombatEvent> RecentCombatEvents => recentCombatEvents;
        public MissionResultState Result { get; private set; } = MissionResultState.InProgress;
        public string ResultReason { get; private set; } = "";

        private readonly List<UnitState> units = new();
        private readonly List<StructureState> structures = new();
        private readonly List<ObjectiveState> objectives = new();
        private readonly List<CombatEvent> recentCombatEvents = new();
        private readonly Dictionary<string, bool> flags = new(StringComparer.OrdinalIgnoreCase);

        public BattleMission(MissionContract contract, CombatProfileCatalog combatProfiles)
        {
            Contract = contract ?? throw new ArgumentNullException(nameof(contract));
            combatProfiles ??= CombatProfileCatalog.Empty;

            if (contract.units != null)
            {
                foreach (UnitSpawn spawn in contract.units)
                {
                    units.Add(new UnitState(spawn, combatProfiles));
                }
            }

            if (contract.staticObjects != null)
            {
                foreach (StaticObjectSpawn spawn in contract.staticObjects)
                {
                    structures.Add(new StructureState(spawn));
                }
            }

            if (contract.objectives != null)
            {
                foreach (ObjectiveDefinition objective in contract.objectives)
                {
                    objectives.Add(new ObjectiveState(objective));
                }
            }

            EvaluateObjectives();
            EvaluateMissionResult();
        }

        public static BattleMission FromJson(string json, CombatProfileCatalog combatProfiles = null)
        {
            MissionContract contract = JsonUtility.FromJson<MissionContract>(json);
            if (contract == null || contract.mission == null)
            {
                throw new InvalidOperationException("Mission contract JSON is empty or invalid.");
            }

            return new BattleMission(contract, combatProfiles);
        }

        public IEnumerable<UnitState> PlayerUnits()
        {
            foreach (UnitState unit in units)
            {
                if (unit.IsPlayerUnit)
                {
                    yield return unit;
                }
            }
        }

        public UnitState FindUnit(string unitId)
        {
            foreach (UnitState unit in units)
            {
                if (unit.Id == unitId)
                {
                    return unit;
                }
            }

            return null;
        }

        public void IssueSquadMove(Vector2 missionPoint)
        {
            if (Result != MissionResultState.InProgress)
            {
                return;
            }

            foreach (UnitState unit in PlayerUnits())
            {
                if (!unit.IsDetached)
                {
                    unit.SetMoveOrder(missionPoint, detached: false);
                }
            }
        }

        public void IssueDetachedMove(string unitId, Vector2 missionPoint)
        {
            if (Result != MissionResultState.InProgress)
            {
                return;
            }

            UnitState unit = FindUnit(unitId);
            if (unit != null && unit.IsPlayerUnit)
            {
                unit.SetMoveOrder(missionPoint, detached: true);
            }
        }

        public int IssueSquadJump(Vector2 missionPoint, float jumpDistance, Func<Vector2, bool> isLandingAllowed)
        {
            if (Result != MissionResultState.InProgress)
            {
                return 0;
            }

            int accepted = 0;
            foreach (UnitState unit in PlayerUnits())
            {
                if (!unit.IsDetached && unit.TryStartJumpToward(missionPoint, jumpDistance, isLandingAllowed, detached: false))
                {
                    accepted++;
                }
            }

            return accepted;
        }

        public int IssueDetachedJump(string unitId, Vector2 missionPoint, float jumpDistance, Func<Vector2, bool> isLandingAllowed)
        {
            if (Result != MissionResultState.InProgress)
            {
                return 0;
            }

            UnitState unit = FindUnit(unitId);
            if (unit != null && unit.IsPlayerUnit && unit.TryStartJumpToward(missionPoint, jumpDistance, isLandingAllowed, detached: true))
            {
                return 1;
            }

            return 0;
        }

        public void Tick(float deltaTime)
        {
            recentCombatEvents.Clear();
            if (Result != MissionResultState.InProgress)
            {
                return;
            }

            foreach (UnitState unit in units)
            {
                unit.TickMovement(deltaTime);
                unit.TickWeapon(deltaTime);
            }

            foreach (UnitState unit in units)
            {
                UnitState target = AcquireTarget(unit);
                StructureState structureTarget = target == null ? AcquireStructureTarget(unit) : null;
                unit.SetCurrentTargetId(target == null ? structureTarget?.Id : target.Id);
                if (unit.CanFireAt(target))
                {
                    recentCombatEvents.Add(unit.FireAt(target));
                }
                else if (unit.CanFireAt(structureTarget))
                {
                    recentCombatEvents.Add(unit.FireAt(structureTarget));
                }
            }

            EvaluateObjectives();
            EvaluateMissionResult();
        }

        private UnitState AcquireTarget(UnitState attacker)
        {
            if (attacker.IsDestroyed)
            {
                return null;
            }

            UnitState bestTarget = null;
            float bestDistanceSqr = float.MaxValue;
            float rangeSqr = attacker.Profile.WeaponRange * attacker.Profile.WeaponRange;

            foreach (UnitState candidate in units)
            {
                if (candidate.IsDestroyed || candidate.TeamId == attacker.TeamId)
                {
                    continue;
                }

                float distanceSqr = (candidate.MissionPosition - attacker.MissionPosition).sqrMagnitude;
                if (distanceSqr <= rangeSqr && distanceSqr < bestDistanceSqr)
                {
                    bestTarget = candidate;
                    bestDistanceSqr = distanceSqr;
                }
            }

            return bestTarget;
        }

        private StructureState AcquireStructureTarget(UnitState attacker)
        {
            if (attacker.IsDestroyed)
            {
                return null;
            }

            StructureState bestTarget = null;
            float bestDistanceSqr = float.MaxValue;
            float range = attacker.Profile.WeaponRange;

            foreach (StructureState candidate in structures)
            {
                if (!candidate.IsTargetable || candidate.IsDestroyed || candidate.TeamId == attacker.TeamId)
                {
                    continue;
                }

                float effectiveRange = range + candidate.Radius;
                float distanceSqr = (candidate.MissionPosition - attacker.MissionPosition).sqrMagnitude;
                if (distanceSqr <= effectiveRange * effectiveRange && distanceSqr < bestDistanceSqr)
                {
                    bestTarget = candidate;
                    bestDistanceSqr = distanceSqr;
                }
            }

            return bestTarget;
        }

        private void EvaluateObjectives()
        {
            foreach (ObjectiveState state in objectives)
            {
                ObjectiveDefinition objective = state.Definition;
                state.IsActive = !objective.activateOnFlag || GetFlag(objective.activateFlagId);
                if (!state.IsActive || state.IsComplete)
                {
                    continue;
                }

                if (AreConditionsMet(objective.conditions))
                {
                    state.IsComplete = true;
                    ApplyActions(objective.actions);
                }
            }
        }

        private void EvaluateMissionResult()
        {
            if (Result != MissionResultState.InProgress)
            {
                return;
            }

            if (AllVisibleObjectivesComplete())
            {
                Result = MissionResultState.Victory;
                ResultReason = "All visible objectives complete.";
                return;
            }

            if (AllPlayerUnitsDestroyed())
            {
                Result = MissionResultState.Defeat;
                ResultReason = "All player units destroyed.";
            }
        }

        private bool AllVisibleObjectivesComplete()
        {
            bool sawVisibleObjective = false;
            foreach (ObjectiveState objective in objectives)
            {
                if (objective.Definition.hidden)
                {
                    continue;
                }

                sawVisibleObjective = true;
                if (!objective.IsComplete)
                {
                    return false;
                }
            }

            return sawVisibleObjective;
        }

        private bool AllPlayerUnitsDestroyed()
        {
            bool sawPlayerUnit = false;
            foreach (UnitState unit in PlayerUnits())
            {
                sawPlayerUnit = true;
                if (!unit.IsDestroyed)
                {
                    return false;
                }
            }

            return sawPlayerUnit;
        }

        private bool AreConditionsMet(ObjectiveCondition[] conditions)
        {
            if (conditions == null || conditions.Length == 0)
            {
                return false;
            }

            foreach (ObjectiveCondition condition in conditions)
            {
                if (!IsConditionMet(condition))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsConditionMet(ObjectiveCondition condition)
        {
            if (condition == null)
            {
                return false;
            }

            switch (condition.type)
            {
                case "MoveAnyUnitToArea":
                    return AnyPlayerUnitInArea(condition.targetArea);
                case "MoveAllSurvivingMechsToArea":
                    return AllPlayerUnitsInArea(condition.targetArea);
                case "DestroySpecificEnemyUnit":
                    return IsTargetUnitDestroyed(condition.targetUnit);
                case "DestroySpecificStructure":
                    return IsTargetStructureDestroyed(condition.targetStructure);
                default:
                    return false;
            }
        }

        private bool IsTargetUnitDestroyed(TargetUnit target)
        {
            if (target == null || target.position == null)
            {
                return false;
            }

            UnitState unit = FindUnitNearPosition(new Vector2(target.position.x, target.position.y), 75f);
            return unit != null && unit.IsDestroyed;
        }

        private bool IsTargetStructureDestroyed(TargetStructure target)
        {
            if (target == null || target.position == null)
            {
                return false;
            }

            StructureState structure = FindStructureNearPosition(new Vector2(target.position.x, target.position.y), 220f);
            return structure != null && structure.IsDestroyed;
        }

        private StructureState FindStructureNearPosition(Vector2 missionPoint, float tolerance)
        {
            StructureState best = null;
            float bestDistanceSqr = tolerance * tolerance;
            foreach (StructureState structure in structures)
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

        private UnitState FindUnitNearPosition(Vector2 missionPoint, float tolerance)
        {
            UnitState best = null;
            float bestDistanceSqr = tolerance * tolerance;
            foreach (UnitState unit in units)
            {
                float distanceSqr = (unit.MissionPosition - missionPoint).sqrMagnitude;
                if (distanceSqr <= bestDistanceSqr)
                {
                    best = unit;
                    bestDistanceSqr = distanceSqr;
                }
            }

            return best;
        }

        private bool AnyPlayerUnitInArea(TargetArea area)
        {
            if (area == null)
            {
                return false;
            }

            Vector2 center = new(area.x, area.y);
            float radiusSqr = area.radius * area.radius;
            foreach (UnitState unit in PlayerUnits())
            {
                if (!unit.IsDestroyed && (unit.MissionPosition - center).sqrMagnitude <= radiusSqr)
                {
                    return true;
                }
            }

            return false;
        }

        private bool AllPlayerUnitsInArea(TargetArea area)
        {
            if (area == null)
            {
                return false;
            }

            bool sawUnit = false;
            Vector2 center = new(area.x, area.y);
            float radiusSqr = area.radius * area.radius;
            foreach (UnitState unit in PlayerUnits())
            {
                if (unit.IsDestroyed)
                {
                    continue;
                }

                sawUnit = true;
                if ((unit.MissionPosition - center).sqrMagnitude > radiusSqr)
                {
                    return false;
                }
            }

            return sawUnit;
        }

        private void ApplyActions(ObjectiveAction[] actions)
        {
            if (actions == null)
            {
                return;
            }

            foreach (ObjectiveAction action in actions)
            {
                if (action?.type == "SetBooleanFlag" && action.flag != null)
                {
                    flags[action.flag.id] = action.flag.value;
                }
            }
        }

        private bool GetFlag(string id)
        {
            return !string.IsNullOrEmpty(id) && flags.TryGetValue(id, out bool value) && value;
        }
    }

    public enum MissionResultState
    {
        InProgress,
        Victory,
        Defeat
    }
}
