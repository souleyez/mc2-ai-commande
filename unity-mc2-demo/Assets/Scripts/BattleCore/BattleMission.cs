using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MC2Demo.BattleCore
{
    public sealed class BattleMission
    {
        private const float CriticalSectionSummaryRatio = 0.35f;
        private const float SquadMoveFormationSpacing = 240f;
        private const float SquadAttackFormationSpacing = 260f;
        private const float SquadAttackUnitStandOff = 120f;
        private const float SquadAttackStructureStandOffPadding = 100f;
        private const int EnemyAttackFormationSlots = 32;
        private const int EnemyAttackFormationStride = 3;
        private const float EnemyAttackRangeCushion = 5f;
        private const float EnemyAttackInfantryMinFormationRadius = 160f;
        private const float EnemyAttackInfantryMaxFormationRadius = 260f;
        private const float EnemyAttackVehicleMinFormationRadius = 200f;
        private const float EnemyAttackVehicleMaxFormationRadius = 370f;
        private const float EnemyAttackMechMinFormationRadius = 220f;
        private const float EnemyAttackMechMaxFormationRadius = 400f;
        private const float UnitCollisionInfantryRadius = 20f;
        private const float UnitCollisionVehicleRadius = 42f;
        private const float UnitCollisionMechRadius = 50f;
        private const float UnitCollisionMaxPushPerPass = 35f;
        private const int UnitCollisionPasses = 3;
        private const float StructureCollisionPadding = 35f;
        private const float StructureCollisionMaxPushPerPass = 70f;
        private const float TerrainObjectCollisionMaxPushPerPass = 45f;

        public MissionContract Contract { get; }
        public IReadOnlyList<UnitState> Units => units;
        public IReadOnlyList<StructureState> Structures => structures;
        public IReadOnlyList<ObjectiveState> Objectives => objectives;
        public IReadOnlyList<CombatEvent> RecentCombatEvents => recentCombatEvents;
        public IReadOnlyList<UnitActivationEvent> RecentUnitActivationEvents => recentUnitActivationEvents;
        public IReadOnlyList<ObjectiveEvent> RecentObjectiveEvents => recentObjectiveEvents;
        public MissionResultState Result { get; private set; } = MissionResultState.InProgress;
        public string ResultReason { get; private set; } = "";
        public MissionResultSummary ResultSummary => BuildResultSummary();
        public float MissionTimeSeconds { get; private set; }

        private readonly List<UnitState> units = new();
        private readonly List<StructureState> structures = new();
        private readonly List<TerrainObjectObstacle> terrainObjectObstacles = new();
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

            if (contract.terrainObjects != null)
            {
                foreach (TerrainObjectSpawn spawn in contract.terrainObjects)
                {
                    if (TryCreateTerrainObjectObstacle(spawn, out TerrainObjectObstacle obstacle)
                        && !IsCoveredByStructureCenter(obstacle.Position))
                    {
                        terrainObjectObstacles.Add(obstacle);
                    }
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

            List<UnitState> squadUnits = CommandableSquadUnits();
            Vector2 approach = SquadApproachDirection(squadUnits, missionPoint);
            int accepted = 0;
            for (int index = 0; index < squadUnits.Count; index++)
            {
                UnitState unit = squadUnits[index];
                Vector2 destination = ResolveMoveDestination(
                    unit,
                    missionPoint + SquadMoveFormationOffset(index, squadUnits.Count, approach),
                    approach);
                unit.SetMoveOrder(destination, detached: false);
                accepted++;
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
                unit.SetMoveOrder(
                    ResolveMoveDestination(unit, missionPoint, missionPoint - unit.MissionPosition),
                    detached: true);
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

            List<UnitState> squadUnits = CommandableSquadUnits();
            Vector2 approach = SquadApproachDirection(squadUnits, target.MissionPosition);
            int accepted = 0;
            for (int index = 0; index < squadUnits.Count; index++)
            {
                UnitState unit = squadUnits[index];
                Vector2 offset = SquadAttackFormationOffset(index, squadUnits.Count, approach, SquadAttackUnitStandOff);
                unit.SetAttackOrder(target.Id, target.MissionPosition, detached: false, offset);
                accepted++;
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

            List<UnitState> squadUnits = CommandableSquadUnits();
            Vector2 approach = SquadApproachDirection(squadUnits, target.MissionPosition);
            float standOff = Mathf.Max(SquadAttackUnitStandOff, target.Radius + SquadAttackStructureStandOffPadding);
            int accepted = 0;
            for (int index = 0; index < squadUnits.Count; index++)
            {
                UnitState unit = squadUnits[index];
                Vector2 offset = SquadAttackFormationOffset(index, squadUnits.Count, approach, standOff);
                unit.SetAttackOrder(target.Id, target.MissionPosition, detached: false, offset);
                accepted++;
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
                if (!unit.IsDetached
                    && unit.TryStartJumpToward(
                        missionPoint,
                        jumpDistance,
                        landingPoint => IsLandingAllowed(unit, landingPoint, isLandingAllowed),
                        detached: false))
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
            if (unit != null
                && unit.IsPlayerUnit
                && unit.TryStartJumpToward(
                    missionPoint,
                    jumpDistance,
                    landingPoint => IsLandingAllowed(unit, landingPoint, isLandingAllowed),
                    detached: true))
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

            ResolveUnitCollisions();
            ResolveStructureCollisions();
            ResolveTerrainObjectCollisions();
            ResolveUnitCollisions();

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

        public int CompleteVisibleObjectivesForPresentationAudit()
        {
            if (Result != MissionResultState.InProgress)
            {
                return 0;
            }

            int completed = 0;
            foreach (ObjectiveState state in objectives)
            {
                if (state.Definition.hidden)
                {
                    continue;
                }

                state.IsActive = true;
                if (!state.IsComplete)
                {
                    state.IsComplete = true;
                    completed++;
                }

                ApplyActions(state.Definition.actions);
            }

            RefreshUnitActivation(reportEvents: true);
            EvaluateMissionResult();
            return completed;
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
            float rangeSqr = attacker.CombatWeaponRange * attacker.CombatWeaponRange;

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
            float range = attacker.CombatWeaponRange;

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
                unit.SetAttackOrder(playerTarget.Id, playerTarget.MissionPosition, detached: false, EnemyAttackFormationOffset(unit));
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
            return Mathf.Max(900f, unit.CombatWeaponRange * 1.8f);
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
                bool secondAmbush = BrainEquals(unit, "mc2_01_infantry_ambush2");
                Vector2 anchor = secondAmbush
                    ? new Vector2(3520f, -448f)
                    : new Vector2(3392f, -448f);
                int step = UnitIdNumber(unit.Id) + (secondAmbush ? 2 : 0);
                patrolPoint = PatrolRingPoint(anchor, 220f, step, 8);
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

        private static Vector2 PatrolRingPoint(Vector2 anchor, float radius, int step, int slots)
        {
            int safeSlots = Mathf.Max(1, slots);
            float angle = Mathf.Abs(step % safeSlots) * (Mathf.PI * 2f / safeSlots);
            return anchor + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private List<UnitState> CommandableSquadUnits()
        {
            List<UnitState> squadUnits = new();
            foreach (UnitState unit in PlayerUnits())
            {
                if (unit != null && !unit.IsDetached && unit.IsActive && !unit.IsDestroyed)
                {
                    squadUnits.Add(unit);
                }
            }

            return squadUnits;
        }

        private void ResolveUnitCollisions()
        {
            for (int pass = 0; pass < UnitCollisionPasses; pass++)
            {
                for (int firstIndex = 0; firstIndex < units.Count; firstIndex++)
                {
                    UnitState first = units[firstIndex];
                    if (!CanResolveUnitCollision(first))
                    {
                        continue;
                    }

                    for (int secondIndex = firstIndex + 1; secondIndex < units.Count; secondIndex++)
                    {
                        UnitState second = units[secondIndex];
                        if (!CanResolveUnitCollision(second))
                        {
                            continue;
                        }

                        float minimumDistance = UnitCollisionRadius(first) + UnitCollisionRadius(second);
                        Vector2 delta = second.MissionPosition - first.MissionPosition;
                        float distanceSqr = delta.sqrMagnitude;
                        if (distanceSqr >= minimumDistance * minimumDistance)
                        {
                            continue;
                        }

                        float distance = 0f;
                        Vector2 direction;
                        if (distanceSqr <= 0.0001f)
                        {
                            direction = PairSeparationDirection(first.Id, second.Id);
                        }
                        else
                        {
                            distance = Mathf.Sqrt(distanceSqr);
                            direction = delta / Mathf.Max(0.001f, distance);
                        }

                        float push = Mathf.Min((minimumDistance - distance) * 0.5f, UnitCollisionMaxPushPerPass);
                        if (push <= 0f)
                        {
                            continue;
                        }

                        first.ApplyCollisionDisplacement(-direction * push);
                        second.ApplyCollisionDisplacement(direction * push);
                    }
                }
            }
        }

        private void ResolveStructureCollisions()
        {
            if (structures.Count == 0)
            {
                return;
            }

            foreach (UnitState unit in units)
            {
                if (!CanResolveUnitCollision(unit))
                {
                    continue;
                }

                foreach (StructureState structure in structures)
                {
                    if (!IsBlockingStructure(structure))
                    {
                        continue;
                    }

                    float minimumDistance = StructureCollisionRadius(structure) + UnitCollisionRadius(unit);
                    Vector2 delta = unit.MissionPosition - structure.MissionPosition;
                    float distanceSqr = delta.sqrMagnitude;
                    if (distanceSqr >= minimumDistance * minimumDistance)
                    {
                        continue;
                    }

                    float distance = 0f;
                    Vector2 direction;
                    if (distanceSqr <= 0.0001f)
                    {
                        direction = PairSeparationDirection(structure.Id, unit.Id);
                    }
                    else
                    {
                        distance = Mathf.Sqrt(distanceSqr);
                        direction = delta / Mathf.Max(0.001f, distance);
                    }

                    float push = Mathf.Min(minimumDistance - distance, StructureCollisionMaxPushPerPass);
                    if (push <= 0f)
                    {
                        continue;
                    }

                    bool shiftMoveTarget = IsPointInsideStructureObstacle(unit.MoveTarget, unit, structure);
                    unit.ApplyCollisionDisplacement(direction * push, shiftMoveTarget);
                }
            }
        }

        private void ResolveTerrainObjectCollisions()
        {
            if (terrainObjectObstacles.Count == 0)
            {
                return;
            }

            foreach (UnitState unit in units)
            {
                if (!CanResolveUnitCollision(unit))
                {
                    continue;
                }

                foreach (TerrainObjectObstacle obstacle in terrainObjectObstacles)
                {
                    float minimumDistance = obstacle.Radius + UnitCollisionRadius(unit);
                    Vector2 delta = unit.MissionPosition - obstacle.Position;
                    float distanceSqr = delta.sqrMagnitude;
                    if (distanceSqr >= minimumDistance * minimumDistance)
                    {
                        continue;
                    }

                    float distance = 0f;
                    Vector2 direction;
                    if (distanceSqr <= 0.0001f)
                    {
                        direction = PairSeparationDirection(obstacle.Id, unit.Id);
                    }
                    else
                    {
                        distance = Mathf.Sqrt(distanceSqr);
                        direction = delta / Mathf.Max(0.001f, distance);
                    }

                    float push = Mathf.Min(minimumDistance - distance, TerrainObjectCollisionMaxPushPerPass);
                    if (push <= 0f)
                    {
                        continue;
                    }

                    bool shiftMoveTarget = IsPointInsideTerrainObjectObstacle(unit.MoveTarget, unit, obstacle);
                    unit.ApplyCollisionDisplacement(direction * push, shiftMoveTarget);
                }
            }
        }

        private static bool CanResolveUnitCollision(UnitState unit)
        {
            return unit != null && unit.IsActive && !unit.IsDestroyed && !unit.IsJumping;
        }

        private static float UnitCollisionRadius(UnitState unit)
        {
            if (unit == null)
            {
                return UnitCollisionVehicleRadius;
            }

            if (IsInfantryUnit(unit))
            {
                return UnitCollisionInfantryRadius;
            }

            return IsMechLikeUnit(unit) ? UnitCollisionMechRadius : UnitCollisionVehicleRadius;
        }

        private static bool IsInfantryUnit(UnitState unit)
        {
            return unit != null
                && !string.IsNullOrWhiteSpace(unit.UnitType)
                && unit.UnitType.IndexOf("Infantry", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsLightVehicleUnit(UnitState unit)
        {
            return ContainsIgnoreCase(unit?.UnitType, "Centipede")
                || ContainsIgnoreCase(unit?.UnitType, "Harasser")
                || ContainsIgnoreCase(unit?.UnitType, "LRMC");
        }

        private static bool IsMechLikeUnit(UnitState unit)
        {
            if (unit == null || unit.Sections == null)
            {
                return false;
            }

            bool hasCockpit = false;
            bool hasLegs = false;
            for (int index = 0; index < unit.Sections.Length; index++)
            {
                DamageSection section = unit.Sections[index];
                if (section == null)
                {
                    continue;
                }

                hasCockpit |= string.Equals(section.Name, "Cockpit", StringComparison.OrdinalIgnoreCase);
                hasLegs |= string.Equals(section.Name, "Legs", StringComparison.OrdinalIgnoreCase);
            }

            return hasCockpit && hasLegs;
        }

        private static Vector2 PairSeparationDirection(string firstId, string secondId)
        {
            int first = UnitIdNumber(firstId);
            int second = UnitIdNumber(secondId);
            int seed = Mathf.Abs((first * 73856093) ^ (second * 19349663));
            float angle = (seed % 1024) * (Mathf.PI * 2f / 1024f);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        private static Vector2 SquadMoveFormationOffset(int index, int count, Vector2 approach)
        {
            return SquadFormationOffset(index, count, approach, SquadMoveFormationSpacing, 0f);
        }

        private static Vector2 SquadAttackFormationOffset(int index, int count, Vector2 approach, float standOff)
        {
            return SquadFormationOffset(index, count, approach, SquadAttackFormationSpacing, Mathf.Max(0f, standOff));
        }

        private static Vector2 SquadFormationOffset(int index, int count, Vector2 approach, float spacing, float standOff)
        {
            Vector2 forward = NormalizedOrDefault(approach, Vector2.right);
            Vector2 right = new(forward.y, -forward.x);
            float back = Mathf.Max(0f, standOff);
            float slotSpacing = Mathf.Max(80f, spacing);

            switch (Mathf.Clamp(index, 0, 5))
            {
                case 0:
                    return -forward * back;
                case 1:
                    return (-forward * (back + slotSpacing * 0.1f)) - (right * slotSpacing);
                case 2:
                    return (-forward * (back + slotSpacing * 0.1f)) + (right * slotSpacing);
                case 3:
                    return -forward * (back + slotSpacing * 0.95f);
                case 4:
                    return (-forward * (back + slotSpacing * 0.75f)) - (right * slotSpacing * 1.7f);
                default:
                    return (-forward * (back + slotSpacing * 0.75f)) + (right * slotSpacing * 1.7f);
            }
        }

        private static Vector2 SquadApproachDirection(IReadOnlyList<UnitState> squadUnits, Vector2 target)
        {
            if (squadUnits == null || squadUnits.Count == 0)
            {
                return Vector2.right;
            }

            Vector2 centroid = Vector2.zero;
            int count = 0;
            for (int index = 0; index < squadUnits.Count; index++)
            {
                UnitState unit = squadUnits[index];
                if (unit == null)
                {
                    continue;
                }

                centroid += unit.MissionPosition;
                count++;
            }

            if (count <= 0)
            {
                return Vector2.right;
            }

            centroid /= count;
            return NormalizedOrDefault(target - centroid, Vector2.right);
        }

        private static Vector2 EnemyAttackFormationOffset(UnitState unit)
        {
            if (unit == null)
            {
                return Vector2.zero;
            }

            int unitNumber = UnitIdNumber(unit.Id);
            int slot = Mathf.Abs(((unitNumber * EnemyAttackFormationStride) + EnemyFormationSlotOffset(unit))
                % EnemyAttackFormationSlots);
            int ring = Mathf.Abs(((unitNumber * 5) + EnemyFormationSlotOffset(unit)) % 3);
            float angle = slot * (Mathf.PI * 2f / EnemyAttackFormationSlots);
            float desiredRadius = EnemyAttackDesiredRadius(unit, ring);
            float radius = Mathf.Min(desiredRadius, Mathf.Max(40f, unit.CombatWeaponRange - EnemyAttackRangeCushion));

            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private static float EnemyAttackDesiredRadius(UnitState unit, int ring)
        {
            if (IsInfantryUnit(unit))
            {
                return Mathf.Clamp(
                    (unit.CombatWeaponRange * 0.46f) + (ring * 45f),
                    EnemyAttackInfantryMinFormationRadius,
                    EnemyAttackInfantryMaxFormationRadius);
            }

            if (IsLightVehicleUnit(unit))
            {
                return Mathf.Clamp(
                    (unit.CombatWeaponRange * 0.42f) + (ring * 62f),
                    EnemyAttackVehicleMinFormationRadius,
                    EnemyAttackVehicleMaxFormationRadius);
            }

            return Mathf.Clamp(
                (unit.CombatWeaponRange * 0.40f) + (ring * 70f),
                EnemyAttackMechMinFormationRadius,
                EnemyAttackMechMaxFormationRadius);
        }

        private static int EnemyFormationSlotOffset(UnitState unit)
        {
            if (IsInfantryUnit(unit))
            {
                return 15;
            }

            return 0;
        }

        private Vector2 ResolveMoveDestination(UnitState unit, Vector2 desiredPoint, Vector2 approach)
        {
            if (unit == null || (structures.Count == 0 && terrainObjectObstacles.Count == 0))
            {
                return desiredPoint;
            }

            Vector2 resolvedPoint = desiredPoint;
            for (int pass = 0; pass < 2; pass++)
            {
                foreach (StructureState structure in structures)
                {
                    if (!TryGetStructureEscape(unit, structure, resolvedPoint, approach, out Vector2 escape))
                    {
                        continue;
                    }

                    resolvedPoint += escape;
                }

                foreach (TerrainObjectObstacle obstacle in terrainObjectObstacles)
                {
                    if (!TryGetTerrainObjectEscape(unit, obstacle, resolvedPoint, approach, out Vector2 escape))
                    {
                        continue;
                    }

                    resolvedPoint += escape;
                }
            }

            return resolvedPoint;
        }

        private bool IsLandingAllowed(UnitState unit, Vector2 landingPoint, Func<Vector2, bool> externalLandingAllowed)
        {
            if (externalLandingAllowed != null && !externalLandingAllowed(landingPoint))
            {
                return false;
            }

            return !IsPointInsideAnyStructureObstacle(unit, landingPoint);
        }

        private bool IsPointInsideAnyStructureObstacle(UnitState unit, Vector2 point)
        {
            foreach (StructureState structure in structures)
            {
                if (IsPointInsideStructureObstacle(point, unit, structure))
                {
                    return true;
                }
            }

            foreach (TerrainObjectObstacle obstacle in terrainObjectObstacles)
            {
                if (IsPointInsideTerrainObjectObstacle(point, unit, obstacle))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryGetStructureEscape(
            UnitState unit,
            StructureState structure,
            Vector2 point,
            Vector2 approach,
            out Vector2 escape)
        {
            escape = Vector2.zero;
            if (unit == null || !IsBlockingStructure(structure))
            {
                return false;
            }

            float minimumDistance = StructureCollisionRadius(structure) + UnitCollisionRadius(unit);
            Vector2 delta = point - structure.MissionPosition;
            float distanceSqr = delta.sqrMagnitude;
            if (distanceSqr >= minimumDistance * minimumDistance)
            {
                return false;
            }

            float distance = 0f;
            Vector2 direction;
            if (distanceSqr <= 0.0001f)
            {
                direction = NormalizedOrDefault(-approach, PairSeparationDirection(structure.Id, unit.Id));
            }
            else
            {
                distance = Mathf.Sqrt(distanceSqr);
                direction = delta / Mathf.Max(0.001f, distance);
            }

            escape = direction * (minimumDistance - distance);
            return escape.sqrMagnitude > 0.0001f;
        }

        private bool IsPointInsideStructureObstacle(Vector2 point, UnitState unit, StructureState structure)
        {
            if (unit == null || !IsBlockingStructure(structure))
            {
                return false;
            }

            float minimumDistance = StructureCollisionRadius(structure) + UnitCollisionRadius(unit);
            return (point - structure.MissionPosition).sqrMagnitude < minimumDistance * minimumDistance;
        }

        private bool TryGetTerrainObjectEscape(
            UnitState unit,
            TerrainObjectObstacle obstacle,
            Vector2 point,
            Vector2 approach,
            out Vector2 escape)
        {
            escape = Vector2.zero;
            if (unit == null || obstacle == null)
            {
                return false;
            }

            float minimumDistance = obstacle.Radius + UnitCollisionRadius(unit);
            Vector2 delta = point - obstacle.Position;
            float distanceSqr = delta.sqrMagnitude;
            if (distanceSqr >= minimumDistance * minimumDistance)
            {
                return false;
            }

            float distance = 0f;
            Vector2 direction;
            if (distanceSqr <= 0.0001f)
            {
                direction = NormalizedOrDefault(-approach, PairSeparationDirection(obstacle.Id, unit.Id));
            }
            else
            {
                distance = Mathf.Sqrt(distanceSqr);
                direction = delta / Mathf.Max(0.001f, distance);
            }

            escape = direction * (minimumDistance - distance);
            return escape.sqrMagnitude > 0.0001f;
        }

        private bool IsPointInsideTerrainObjectObstacle(Vector2 point, UnitState unit, TerrainObjectObstacle obstacle)
        {
            if (unit == null || obstacle == null)
            {
                return false;
            }

            float minimumDistance = obstacle.Radius + UnitCollisionRadius(unit);
            return (point - obstacle.Position).sqrMagnitude < minimumDistance * minimumDistance;
        }

        private static bool IsBlockingStructure(StructureState structure)
        {
            return structure != null && !structure.IsDestroyed && structure.Radius > 0f;
        }

        private static float StructureCollisionRadius(StructureState structure)
        {
            return Mathf.Max(60f, structure.Radius + StructureCollisionPadding);
        }

        private bool IsCoveredByStructureCenter(Vector2 position)
        {
            foreach (StructureState structure in structures)
            {
                if (structure != null && Vector2.Distance(position, structure.MissionPosition) < 40f)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryCreateTerrainObjectObstacle(TerrainObjectSpawn spawn, out TerrainObjectObstacle obstacle)
        {
            obstacle = null;
            if (spawn?.position == null)
            {
                return false;
            }

            string objectClass = spawn.objectClass ?? "";
            string name = TerrainObjectName(spawn);
            float radius = 0f;
            if (string.Equals(objectClass, "BUILDING", StringComparison.OrdinalIgnoreCase))
            {
                radius = TerrainBuildingObstacleRadius(name);
            }
            else if (string.Equals(objectClass, "TREE", StringComparison.OrdinalIgnoreCase)
                && IsHardTerrainTreeObstacle(name))
            {
                radius = TerrainHardTreeObstacleRadius(name);
            }

            if (radius <= 0f)
            {
                return false;
            }

            obstacle = new TerrainObjectObstacle(
                string.IsNullOrWhiteSpace(spawn.objectId)
                    ? "terrain-object-" + spawn.sourceIndex.ToString(CultureInfo.InvariantCulture)
                    : spawn.objectId,
                new Vector2(spawn.position.x, spawn.position.y),
                radius,
                name);
            return true;
        }

        private static string TerrainObjectName(TerrainObjectSpawn spawn)
        {
            if (spawn == null)
            {
                return "";
            }

            if (!string.IsNullOrWhiteSpace(spawn.assetId))
            {
                return spawn.assetId;
            }

            return spawn.fileName ?? "";
        }

        private static float TerrainBuildingObstacleRadius(string name)
        {
            if (ContainsIgnoreCase(name, "Hangar"))
            {
                return 145f;
            }

            if (ContainsIgnoreCase(name, "Quonset"))
            {
                return 72f;
            }

            if (ContainsIgnoreCase(name, "GenericMilitary")
                || ContainsIgnoreCase(name, "GeodesicDome"))
            {
                return 78f;
            }

            if (ContainsIgnoreCase(name, "PrivateJet")
                || ContainsIgnoreCase(name, "Shilone")
                || ContainsIgnoreCase(name, "Slayer"))
            {
                return 68f;
            }

            if (ContainsIgnoreCase(name, "Portable"))
            {
                return 54f;
            }

            if (ContainsIgnoreCase(name, "Tower"))
            {
                return 44f;
            }

            return 58f;
        }

        private static bool IsHardTerrainTreeObstacle(string name)
        {
            return ContainsIgnoreCase(name, "Barricade")
                || ContainsIgnoreCase(name, "SandBag")
                || ContainsIgnoreCase(name, "Wall")
                || ContainsIgnoreCase(name, "Barrier");
        }

        private static float TerrainHardTreeObstacleRadius(string name)
        {
            if (ContainsIgnoreCase(name, "Barricade"))
            {
                return 38f;
            }

            if (ContainsIgnoreCase(name, "SandBag"))
            {
                return 32f;
            }

            return 34f;
        }

        private static bool ContainsIgnoreCase(string value, string fragment)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private sealed class TerrainObjectObstacle
        {
            public TerrainObjectObstacle(string id, Vector2 position, float radius, string label)
            {
                Id = string.IsNullOrWhiteSpace(id) ? "terrain-object" : id;
                Position = position;
                Radius = Mathf.Max(1f, radius);
                Label = label ?? "";
            }

            public string Id { get; }
            public Vector2 Position { get; }
            public float Radius { get; }
            public string Label { get; }
        }

        private static Vector2 NormalizedOrDefault(Vector2 value, Vector2 fallback)
        {
            if (value.sqrMagnitude > 0.0001f)
            {
                return value.normalized;
            }

            return fallback.sqrMagnitude > 0.0001f ? fallback.normalized : Vector2.right;
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
                bool inRange = Vector2.Distance(attacker.MissionPosition, unitTarget.MissionPosition) <= attacker.CombatWeaponRange;
                attacker.UpdateAttackOrder(unitTarget.MissionPosition, inRange);
                return;
            }

            StructureState structureTarget = FindAttackableStructure(attacker.AttackTargetId, playerTeam: attacker.IsPlayerUnit);
            if (structureTarget != null)
            {
                bool inRange = Vector2.Distance(attacker.MissionPosition, structureTarget.MissionPosition)
                    <= attacker.CombatWeaponRange + structureTarget.Radius;
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

        private MissionResultSummary BuildResultSummary()
        {
            List<string> destroyedEnemies = new();
            List<string> damagedPlayers = new();
            List<string> destroyedStructures = new();
            List<string> completedObjectives = new();
            int visibleObjectives = 0;
            int completedRewardResourcePoints = 0;
            int visibleRewardResourcePoints = 0;
            int repairCostResourcePoints = 0;

            foreach (UnitState unit in units)
            {
                if (unit.IsPlayerUnit)
                {
                    if (IsPlayerUnitDamaged(unit))
                    {
                        damagedPlayers.Add(UnitDamageSummaryLabel(unit));
                    }

                    repairCostResourcePoints += EstimateRepairCostResourcePoints(unit);
                    continue;
                }

                if (unit.IsDestroyed)
                {
                    destroyedEnemies.Add(UnitSummaryLabel(unit));
                }
            }

            foreach (StructureState structure in structures)
            {
                if (structure.IsDestroyed)
                {
                    destroyedStructures.Add(StructureSummaryLabel(structure));
                }
            }

            foreach (ObjectiveState objective in objectives)
            {
                if (objective.Definition.hidden)
                {
                    continue;
                }

                visibleObjectives++;
                int reward = Math.Max(0, objective.Definition.rewardResourcePoints);
                visibleRewardResourcePoints += reward;
                if (objective.IsComplete)
                {
                    completedObjectives.Add(objective.Definition.title);
                    completedRewardResourcePoints += reward;
                }
            }

            return new MissionResultSummary
            {
                destroyedEnemyUnits = destroyedEnemies.Count,
                damagedPlayerUnits = damagedPlayers.Count,
                destroyedStructures = destroyedStructures.Count,
                completedVisibleObjectives = completedObjectives.Count,
                visibleObjectives = visibleObjectives,
                completedRewardResourcePoints = completedRewardResourcePoints,
                visibleRewardResourcePoints = visibleRewardResourcePoints,
                repairCostResourcePoints = repairCostResourcePoints,
                netResourcePoints = completedRewardResourcePoints - repairCostResourcePoints,
                salvageClaimCount = destroyedEnemies.Count,
                destroyedEnemyUnitLabels = destroyedEnemies.ToArray(),
                damagedPlayerUnitLabels = damagedPlayers.ToArray(),
                destroyedStructureLabels = destroyedStructures.ToArray(),
                completedVisibleObjectiveTitles = completedObjectives.ToArray()
            };
        }

        private static int EstimateRepairCostResourcePoints(UnitState unit)
        {
            return MechBayRepairService.EstimateRepairCostResourcePoints(unit);
        }

        private static bool IsPlayerUnitDamaged(UnitState unit)
        {
            if (unit.IsDestroyed || unit.Structure < 0.995f)
            {
                return true;
            }

            foreach (DamageSection section in unit.Sections)
            {
                if (section.Ratio < 0.995f)
                {
                    return true;
                }
            }

            return false;
        }

        private static string UnitSummaryLabel(UnitState unit)
        {
            return unit.Id + " " + unit.UnitType;
        }

        private static string UnitDamageSummaryLabel(UnitState unit)
        {
            string sections = UnitDamageSectionSummary(unit);
            string displayName = string.IsNullOrWhiteSpace(unit.UnitType) ? unit.Id : unit.UnitType;
            return string.IsNullOrEmpty(sections)
                ? displayName
                : displayName + " " + sections;
        }

        private static string UnitDamageSectionSummary(UnitState unit)
        {
            if (unit?.Sections == null || unit.Sections.Length == 0)
            {
                return "";
            }

            List<string> damagedSections = new();
            for (int index = 0; index < unit.Sections.Length; index++)
            {
                DamageSection section = unit.Sections[index];
                if (section == null || section.Ratio >= 0.995f)
                {
                    continue;
                }

                damagedSections.Add(DamageSectionSummaryLabel(section));
            }

            if (damagedSections.Count == 0)
            {
                return "";
            }

            int shown = Math.Min(2, damagedSections.Count);
            string text = string.Join("/", damagedSections.GetRange(0, shown));
            int remaining = damagedSections.Count - shown;
            return remaining > 0 ? text + " +" + remaining.ToString(CultureInfo.InvariantCulture) : text;
        }

        private static string DamageSectionSummaryLabel(DamageSection section)
        {
            if (section.IsDestroyed)
            {
                return ShortSectionName(section.Name) + " X";
            }

            int percent = Mathf.RoundToInt(Mathf.Clamp01(section.Ratio) * 100f);
            string marker = section.Ratio < CriticalSectionSummaryRatio ? " !" : " ";
            return ShortSectionName(section.Name) + marker + percent.ToString(CultureInfo.InvariantCulture);
        }

        private static string ShortSectionName(string sectionName)
        {
            switch (sectionName)
            {
                case "Cockpit":
                    return "CP";
                case "Torso":
                    return "TR";
                case "Left Arm":
                    return "LA";
                case "Right Arm":
                    return "RA";
                case "Legs":
                    return "LG";
                case "Front":
                    return "FR";
                case "Rear":
                    return "RR";
                case "Turret":
                    return "TU";
                case "Left":
                    return "L";
                case "Right":
                    return "R";
                default:
                    return string.IsNullOrWhiteSpace(sectionName) ? "SEC" : sectionName;
            }
        }

        private static string StructureSummaryLabel(StructureState structure)
        {
            return structure.Id + " " + structure.ObjectType;
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
                float distanceSqr = Mathf.Min(
                    (unit.MissionPosition - missionPoint).sqrMagnitude,
                    (unit.SpawnPosition - missionPoint).sqrMagnitude);
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

    [Serializable]
    public sealed class MissionResultSummary
    {
        public int destroyedEnemyUnits;
        public int damagedPlayerUnits;
        public int destroyedStructures;
        public int completedVisibleObjectives;
        public int visibleObjectives;
        public int completedRewardResourcePoints;
        public int visibleRewardResourcePoints;
        public int repairCostResourcePoints;
        public int netResourcePoints;
        public int salvageClaimCount;
        public string[] destroyedEnemyUnitLabels;
        public string[] damagedPlayerUnitLabels;
        public string[] destroyedStructureLabels;
        public string[] completedVisibleObjectiveTitles;
    }

    public enum MissionResultState
    {
        InProgress,
        Victory,
        Defeat
    }
}
