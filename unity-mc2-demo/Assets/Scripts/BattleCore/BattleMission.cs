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
        public IReadOnlyList<UnitActivationEvent> RecentUnitActivationEvents => recentUnitActivationEvents;
        public IReadOnlyList<ObjectiveEvent> RecentObjectiveEvents => recentObjectiveEvents;
        public MissionResultState Result { get; private set; } = MissionResultState.InProgress;
        public string ResultReason { get; private set; } = "";
        public float MissionTimeSeconds { get; private set; }

        private readonly List<UnitState> units = new();
        private readonly List<StructureState> structures = new();
        private readonly List<ObjectiveState> objectives = new();
        private readonly List<CombatEvent> recentCombatEvents = new();
        private readonly List<UnitActivationEvent> recentUnitActivationEvents = new();
        private readonly List<ObjectiveEvent> recentObjectiveEvents = new();
        private readonly Dictionary<string, int> enemyPatrolSteps = new(StringComparer.OrdinalIgnoreCase);
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

            EvaluateObjectives(reportEvents: false);
            RefreshUnitActivation(reportEvents: false);
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

        public StructureState FindStructure(string structureId)
        {
            foreach (StructureState structure in structures)
            {
                if (structure.Id == structureId)
                {
                    return structure;
                }
            }

            return null;
        }

        public int IssueSquadMove(Vector2 missionPoint)
        {
            if (Result != MissionResultState.InProgress)
            {
                return 0;
            }

            int accepted = 0;
            foreach (UnitState unit in PlayerUnits())
            {
                if (!unit.IsDetached)
                {
                    unit.SetMoveOrder(missionPoint, detached: false);
                    accepted++;
                }
            }

            return accepted;
        }

        public int IssueDetachedMove(string unitId, Vector2 missionPoint)
        {
            if (Result != MissionResultState.InProgress)
            {
                return 0;
            }

            UnitState unit = FindUnit(unitId);
            if (unit != null && unit.IsPlayerUnit)
            {
                unit.SetMoveOrder(missionPoint, detached: true);
                return 1;
            }

            return 0;
        }

        public int IssueSquadAttackUnit(string targetUnitId)
        {
            UnitState target = FindAttackableUnit(targetUnitId, playerTeam: true);
            if (Result != MissionResultState.InProgress || target == null)
            {
                return 0;
            }

            int accepted = 0;
            foreach (UnitState unit in PlayerUnits())
            {
                if (!unit.IsDetached)
                {
                    unit.SetAttackOrder(target.Id, target.MissionPosition, detached: false);
                    accepted++;
                }
            }

            return accepted;
        }

        public int IssueDetachedAttackUnit(string unitId, string targetUnitId)
        {
            UnitState unit = FindUnit(unitId);
            UnitState target = FindAttackableUnit(targetUnitId, playerTeam: true);
            if (Result != MissionResultState.InProgress || unit == null || !unit.IsPlayerUnit || target == null)
            {
                return 0;
            }

            unit.SetAttackOrder(target.Id, target.MissionPosition, detached: true);
            return 1;
        }

        public int IssueSquadAttackStructure(string structureId)
        {
            StructureState target = FindAttackableStructure(structureId, playerTeam: true);
            if (Result != MissionResultState.InProgress || target == null)
            {
                return 0;
            }

            int accepted = 0;
            foreach (UnitState unit in PlayerUnits())
            {
                if (!unit.IsDetached)
                {
                    unit.SetAttackOrder(target.Id, target.MissionPosition, detached: false);
                    accepted++;
                }
            }

            return accepted;
        }

        public int IssueDetachedAttackStructure(string unitId, string structureId)
        {
            UnitState unit = FindUnit(unitId);
            StructureState target = FindAttackableStructure(structureId, playerTeam: true);
            if (Result != MissionResultState.InProgress || unit == null || !unit.IsPlayerUnit || target == null)
            {
                return 0;
            }

            unit.SetAttackOrder(target.Id, target.MissionPosition, detached: true);
            return 1;
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
            recentUnitActivationEvents.Clear();
            recentObjectiveEvents.Clear();
            if (Result != MissionResultState.InProgress)
            {
                return;
            }

            float clampedDeltaTime = Mathf.Max(0f, deltaTime);
            MissionTimeSeconds += clampedDeltaTime;
            foreach (UnitState unit in units)
            {
                if (!unit.IsActive)
                {
                    continue;
                }

                RefreshAttackOrder(unit);
                ApplyEnemyBrainOrder(unit);
                unit.TickMovement(clampedDeltaTime);
                unit.TickWeapon(clampedDeltaTime);
            }

            foreach (UnitState unit in units)
            {
                if (!unit.IsActive)
                {
                    continue;
                }

                StructureState assignedStructureTarget = AcquireAssignedStructureTarget(unit);
                UnitState target = assignedStructureTarget == null ? AcquireTarget(unit) : null;
                StructureState structureTarget = assignedStructureTarget ?? (target == null ? AcquireStructureTarget(unit) : null);
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

            EvaluateObjectives(reportEvents: true);
            RefreshUnitActivation(reportEvents: true);
            EvaluateMissionResult();
        }

        private UnitState AcquireTarget(UnitState attacker)
        {
            if (!attacker.IsActive || attacker.IsDestroyed)
            {
                return null;
            }

            UnitState assignedTarget = FindAttackableUnit(attacker.AttackTargetId, playerTeam: attacker.IsPlayerUnit);
            if (assignedTarget != null)
            {
                return assignedTarget;
            }

            UnitState bestTarget = null;
            float bestDistanceSqr = float.MaxValue;
            float rangeSqr = attacker.Profile.WeaponRange * attacker.Profile.WeaponRange;

            foreach (UnitState candidate in units)
            {
                if (!candidate.IsActive || candidate.IsDestroyed || candidate.TeamId == attacker.TeamId)
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

        private StructureState AcquireAssignedStructureTarget(UnitState attacker)
        {
            if (!attacker.IsActive || attacker.IsDestroyed || string.IsNullOrEmpty(attacker.AttackTargetId))
            {
                return null;
            }

            return FindAttackableStructure(attacker.AttackTargetId, playerTeam: attacker.IsPlayerUnit);
        }

        private StructureState AcquireStructureTarget(UnitState attacker)
        {
            if (!attacker.IsActive || attacker.IsDestroyed)
            {
                return null;
            }

            StructureState assignedTarget = FindAttackableStructure(attacker.AttackTargetId, playerTeam: attacker.IsPlayerUnit);
            if (assignedTarget != null)
            {
                return assignedTarget;
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

        private void ApplyEnemyBrainOrder(UnitState unit)
        {
            if (unit.IsPlayerUnit || unit.IsDestroyed || unit.HasMoveOrder || unit.HasAttackOrder)
            {
                return;
            }

            UnitState playerTarget = FindNearestPlayerUnit(unit, EnemyAlertRange(unit));
            if (playerTarget != null)
            {
                unit.SetAttackOrder(playerTarget.Id, playerTarget.MissionPosition, detached: false);
                return;
            }

            if (TryGetMissionPatrolPoint(unit, out Vector2 patrolPoint))
            {
                unit.SetMoveOrder(patrolPoint, detached: false);
            }
        }

        private UnitState FindNearestPlayerUnit(UnitState seeker, float maxDistance)
        {
            UnitState bestTarget = null;
            float bestDistanceSqr = maxDistance * maxDistance;

            foreach (UnitState candidate in units)
            {
                if (!candidate.IsActive || candidate.IsDestroyed || !candidate.IsPlayerUnit)
                {
                    continue;
                }

                float distanceSqr = (candidate.MissionPosition - seeker.MissionPosition).sqrMagnitude;
                if (distanceSqr <= bestDistanceSqr)
                {
                    bestTarget = candidate;
                    bestDistanceSqr = distanceSqr;
                }
            }

            return bestTarget;
        }

        private static float EnemyAlertRange(UnitState unit)
        {
            return Mathf.Max(900f, unit.Profile.WeaponRange * 1.8f);
        }

        private bool TryGetMissionPatrolPoint(UnitState unit, out Vector2 patrolPoint)
        {
            patrolPoint = unit.MissionPosition;
            if (!IsMission("mc2_01"))
            {
                return false;
            }

            if (BrainStartsWith(unit, "mc2_01_Pat1") || IsEastLrmGroup(unit))
            {
                patrolPoint = NavPatrolPoint(0, new Vector2(3136f, -789.333f), 360f, NextEnemyPatrolStep(unit.Id));
                return true;
            }

            if (BrainEquals(unit, "mc2_01_infantry_ambush") || BrainEquals(unit, "mc2_01_infantry_ambush2"))
            {
                patrolPoint = BrainEquals(unit, "mc2_01_infantry_ambush2")
                    ? new Vector2(3520f, -448f)
                    : new Vector2(3392f, -448f);
                return true;
            }

            if (BrainStartsWith(unit, "mc2_01_Pat2") || BrainEquals(unit, "mc2_01_Pat4"))
            {
                patrolPoint = NavPatrolPoint(1, new Vector2(832f, 2752f), 420f, NextEnemyPatrolStep(unit.Id));
                return true;
            }

            if (BrainEquals(unit, "mc2_01_Starslayer")
                || BrainEquals(unit, "mc2_01_Urbies")
                || IsWestLrmGroup(unit))
            {
                patrolPoint = PatrolPoint(new Vector2(-2240f, 1600f), 520f, NextEnemyPatrolStep(unit.Id));
                return true;
            }

            return false;
        }

        private Vector2 NavPatrolPoint(int markerIndex, Vector2 fallbackAnchor, float fallbackRadius, int step)
        {
            NavMarker marker = FindNavMarker(markerIndex);
            if (marker == null)
            {
                return PatrolPoint(fallbackAnchor, fallbackRadius, step);
            }

            Vector2 anchor = new(marker.x, marker.y);
            float radius = Mathf.Max(160f, marker.radius);
            return PatrolPoint(anchor, radius, step);
        }

        private NavMarker FindNavMarker(int markerIndex)
        {
            if (Contract.navMarkers == null)
            {
                return null;
            }

            foreach (NavMarker marker in Contract.navMarkers)
            {
                if (marker != null && marker.index == markerIndex)
                {
                    return marker;
                }
            }

            return null;
        }

        private int NextEnemyPatrolStep(string unitId)
        {
            if (string.IsNullOrEmpty(unitId))
            {
                unitId = "";
            }

            if (!enemyPatrolSteps.TryGetValue(unitId, out int step))
            {
                step = UnitIdNumber(unitId) % 4;
            }

            enemyPatrolSteps[unitId] = step + 1;
            return step;
        }

        private static Vector2 PatrolPoint(Vector2 anchor, float radius, int step)
        {
            switch (Mathf.Abs(step) % 4)
            {
                case 0:
                    return anchor + new Vector2(radius, 0f);
                case 1:
                    return anchor + new Vector2(0f, radius);
                case 2:
                    return anchor + new Vector2(-radius, 0f);
                default:
                    return anchor + new Vector2(0f, -radius);
            }
        }

        private static int UnitIdNumber(string unitId)
        {
            int value = 0;
            if (string.IsNullOrEmpty(unitId))
            {
                return value;
            }

            foreach (char c in unitId)
            {
                if (c >= '0' && c <= '9')
                {
                    value = (value * 10) + (c - '0');
                }
            }

            return value;
        }

        private void RefreshAttackOrder(UnitState attacker)
        {
            if (!attacker.HasAttackOrder)
            {
                return;
            }

            UnitState unitTarget = FindAttackableUnit(attacker.AttackTargetId, playerTeam: attacker.IsPlayerUnit);
            if (unitTarget != null)
            {
                bool inRange = Vector2.Distance(attacker.MissionPosition, unitTarget.MissionPosition) <= attacker.Profile.WeaponRange;
                attacker.UpdateAttackOrder(unitTarget.MissionPosition, inRange);
                return;
            }

            StructureState structureTarget = FindAttackableStructure(attacker.AttackTargetId, playerTeam: attacker.IsPlayerUnit);
            if (structureTarget != null)
            {
                bool inRange = Vector2.Distance(attacker.MissionPosition, structureTarget.MissionPosition)
                    <= attacker.Profile.WeaponRange + structureTarget.Radius;
                attacker.UpdateAttackOrder(structureTarget.MissionPosition, inRange);
                return;
            }

            attacker.CompleteAttackOrder();
        }

        private UnitState FindAttackableUnit(string targetUnitId, bool playerTeam)
        {
            UnitState target = FindUnit(targetUnitId);
            if (target == null || !target.IsActive || target.IsDestroyed || target.IsPlayerUnit == playerTeam)
            {
                return null;
            }

            return target;
        }

        private StructureState FindAttackableStructure(string structureId, bool playerTeam)
        {
            StructureState target = FindStructure(structureId);
            if (target == null || !target.IsTargetable || target.IsDestroyed)
            {
                return null;
            }

            int playerTeamId = FirstPlayerTeamId();
            if (playerTeam && target.TeamId == playerTeamId)
            {
                return null;
            }

            return target;
        }

        private int FirstPlayerTeamId()
        {
            foreach (UnitState unit in PlayerUnits())
            {
                return unit.TeamId;
            }

            return 0;
        }

        private void EvaluateObjectives(bool reportEvents)
        {
            foreach (ObjectiveState state in objectives)
            {
                ObjectiveDefinition objective = state.Definition;
                bool wasActive = state.IsActive;
                bool wasComplete = state.IsComplete;
                state.IsActive = IsObjectiveActive(objective);
                if (reportEvents && !wasActive && state.IsActive)
                {
                    recentObjectiveEvents.Add(new ObjectiveEvent(objective.index, objective.title, objective.hidden, ObjectiveEventKind.Activated));
                }

                if (!state.IsActive || state.IsComplete)
                {
                    continue;
                }

                if (AreConditionsMet(objective.conditions))
                {
                    state.IsComplete = true;
                    ApplyActions(objective.actions);
                    if (reportEvents && !wasComplete)
                    {
                        recentObjectiveEvents.Add(new ObjectiveEvent(objective.index, objective.title, objective.hidden, ObjectiveEventKind.Completed));
                    }
                }
            }
        }

        private bool IsObjectiveActive(ObjectiveDefinition objective)
        {
            if (objective.activateOnFlag && !GetFlag(objective.activateFlagId))
            {
                return false;
            }

            return !objective.requiresAllPreviousPrimary || ArePreviousPrimaryObjectivesComplete(objective.index);
        }

        private bool ArePreviousPrimaryObjectivesComplete(int objectiveIndex)
        {
            foreach (ObjectiveState objective in objectives)
            {
                if (objective.Definition.hidden || objective.Definition.index >= objectiveIndex)
                {
                    continue;
                }

                if (!objective.IsComplete)
                {
                    return false;
                }
            }

            return true;
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

        private void RefreshUnitActivation(bool reportEvents)
        {
            foreach (UnitState unit in units)
            {
                bool wasActive = unit.IsActive;
                bool shouldBeActive = ShouldUnitBeActive(unit);
                unit.SetActive(shouldBeActive);
                if (reportEvents && !wasActive && unit.IsActive)
                {
                    recentUnitActivationEvents.Add(new UnitActivationEvent(unit.Id, unit.UnitType, unit.Brain));
                }
            }
        }

        private bool ShouldUnitBeActive(UnitState unit)
        {
            if (unit.IsPlayerUnit || unit.IsDestroyed)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(unit.ActivationFlagId))
            {
                return GetFlag(unit.ActivationFlagId);
            }

            if (unit.ActivatesOnObjective)
            {
                return IsObjectiveComplete(unit.ActivationObjectiveIndex);
            }

            int missionObjective = MissionSpecificActivationObjective(unit);
            if (missionObjective >= 0)
            {
                return IsObjectiveComplete(missionObjective);
            }

            if (MissionSpecificRequiresHangarDamage(unit))
            {
                return IsHangarAmbushTriggered();
            }

            string missionFlag = MissionSpecificActivationFlag(unit);
            return string.IsNullOrEmpty(missionFlag) || GetFlag(missionFlag);
        }

        private bool IsObjectiveComplete(int objectiveIndex)
        {
            foreach (ObjectiveState objective in objectives)
            {
                if (objective.Definition.index == objectiveIndex)
                {
                    return objective.IsComplete;
                }
            }

            return false;
        }

        private int MissionSpecificActivationObjective(UnitState unit)
        {
            if (!IsMission("mc2_01"))
            {
                return -1;
            }

            if (BrainEquals(unit, "mc2_01_Starslayer") || BrainEquals(unit, "mc2_01_Urbies"))
            {
                return 7;
            }

            if (IsWestLrmGroup(unit))
            {
                return 7;
            }

            return -1;
        }

        private string MissionSpecificActivationFlag(UnitState unit)
        {
            if (!IsMission("mc2_01"))
            {
                return null;
            }

            if (BrainStartsWith(unit, "mc2_01_Pat1"))
            {
                return "0";
            }

            if (IsEastLrmGroup(unit))
            {
                return "0";
            }

            if (BrainStartsWith(unit, "mc2_01_Pat2") || BrainEquals(unit, "mc2_01_Pat4"))
            {
                return "0";
            }

            return null;
        }

        private bool MissionSpecificRequiresHangarDamage(UnitState unit)
        {
            return IsMission("mc2_01")
                && (BrainEquals(unit, "mc2_01_infantry_ambush") || BrainEquals(unit, "mc2_01_infantry_ambush2"));
        }

        private bool IsHangarAmbushTriggered()
        {
            StructureState hangar = FindStructure("structure-1-0");
            return hangar != null && hangar.CurrentStructure < hangar.MaxStructure;
        }

        private bool IsMission(string missionId)
        {
            return Contract?.mission != null
                && string.Equals(Contract.mission.id, missionId, StringComparison.OrdinalIgnoreCase);
        }

        private static bool BrainEquals(UnitState unit, string brain)
        {
            return string.Equals(unit.Brain, brain, StringComparison.OrdinalIgnoreCase);
        }

        private static bool BrainStartsWith(UnitState unit, string prefix)
        {
            return !string.IsNullOrEmpty(unit.Brain)
                && unit.Brain.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEastLrmGroup(UnitState unit)
        {
            return BrainEquals(unit, "mc2_01_LRMs") && unit.SpawnPosition.x > 0f;
        }

        private static bool IsWestLrmGroup(UnitState unit)
        {
            return BrainEquals(unit, "mc2_01_LRMs") && unit.SpawnPosition.x < 0f;
        }
    }

    public sealed class UnitActivationEvent
    {
        public string UnitId { get; }
        public string UnitType { get; }
        public string Brain { get; }

        public UnitActivationEvent(string unitId, string unitType, string brain)
        {
            UnitId = unitId;
            UnitType = unitType;
            Brain = brain;
        }
    }

    public sealed class ObjectiveEvent
    {
        public int ObjectiveIndex { get; }
        public string Title { get; }
        public bool IsHidden { get; }
        public ObjectiveEventKind Kind { get; }

        public ObjectiveEvent(int objectiveIndex, string title, bool isHidden, ObjectiveEventKind kind)
        {
            ObjectiveIndex = objectiveIndex;
            Title = title;
            IsHidden = isHidden;
            Kind = kind;
        }
    }

    public enum ObjectiveEventKind
    {
        Activated,
        Completed
    }

    public enum MissionResultState
    {
        InProgress,
        Victory,
        Defeat
    }
}
