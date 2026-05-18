using UnityEngine;

namespace MC2Demo.BattleCore
{
    public sealed class UnitState
    {
        private const float ArriveDistance = 20f;

        public string Id { get; }
        public string UnitType { get; }
        public string Brain { get; }
        public int TeamId { get; }
        public bool IsPlayerUnit { get; }
        public CombatProfile Profile { get; }
        public DamageSection[] Sections { get; }
        public bool IsDestroyed { get; private set; }
        public bool IsDetached { get; private set; }
        public bool HasMoveOrder { get; private set; }
        public string CurrentTargetId { get; private set; }
        public string LastHitSection { get; private set; }
        public float WeaponCooldownRemaining { get; private set; }
        public float LastDamageTaken { get; private set; }
        public float CurrentStructure { get; private set; }
        public float Structure => Profile.MaxStructure <= 0f ? 0f : CurrentStructure / Profile.MaxStructure;
        public float WeaponCooldownRatio => Profile.WeaponCooldown <= 0f ? 0f : WeaponCooldownRemaining / Profile.WeaponCooldown;
        public Vector2 MissionPosition { get; private set; }
        public Vector2 MoveTarget { get; private set; }

        public UnitState(UnitSpawn spawn, CombatProfileCatalog profiles)
        {
            Id = spawn.spawnId;
            UnitType = spawn.unitType;
            Brain = spawn.brain;
            TeamId = spawn.teamId;
            IsPlayerUnit = spawn.isPlayerUnit;
            Profile = (profiles ?? CombatProfileCatalog.Empty).ForUnitType(spawn.unitType, spawn.isPlayerUnit);
            Sections = CreateSections(Profile);
            CurrentStructure = Profile.MaxStructure;
            MissionPosition = new Vector2(spawn.position.x, spawn.position.y);
            MoveTarget = MissionPosition;
        }

        public void SetMoveOrder(Vector2 missionPoint, bool detached)
        {
            if (IsDestroyed)
            {
                return;
            }

            MoveTarget = missionPoint;
            HasMoveOrder = true;
            IsDetached = detached;
        }

        public void TickMovement(float deltaTime)
        {
            if (!HasMoveOrder || IsDestroyed)
            {
                return;
            }

            Vector2 toTarget = MoveTarget - MissionPosition;
            float distance = toTarget.magnitude;
            if (distance <= ArriveDistance)
            {
                MissionPosition = MoveTarget;
                HasMoveOrder = false;
                IsDetached = false;
                return;
            }

            float step = Profile.MoveSpeed * deltaTime;
            MissionPosition += toTarget.normalized * Mathf.Min(step, distance);
        }

        public void TickWeapon(float deltaTime)
        {
            if (WeaponCooldownRemaining > 0f)
            {
                WeaponCooldownRemaining = Mathf.Max(0f, WeaponCooldownRemaining - deltaTime);
            }
        }

        public bool CanFireAt(UnitState target)
        {
            return !IsDestroyed
                && target != null
                && !target.IsDestroyed
                && target.TeamId != TeamId
                && WeaponCooldownRemaining <= 0f
                && Vector2.Distance(MissionPosition, target.MissionPosition) <= Profile.WeaponRange;
        }

        public bool CanFireAt(StructureState target)
        {
            return !IsDestroyed
                && target != null
                && target.IsTargetable
                && !target.IsDestroyed
                && target.TeamId != TeamId
                && WeaponCooldownRemaining <= 0f
                && Vector2.Distance(MissionPosition, target.MissionPosition) <= Profile.WeaponRange + target.Radius;
        }

        public CombatEvent FireAt(UnitState target)
        {
            CurrentTargetId = target.Id;
            WeaponCooldownRemaining = Profile.WeaponCooldown;
            DamageResult result = target.ApplyDamage(Profile.WeaponDamage, Id);
            return new CombatEvent(Id, target.Id, result.SectionName, result.DamageApplied, target.IsDestroyed);
        }

        public CombatEvent FireAt(StructureState target)
        {
            CurrentTargetId = target.Id;
            WeaponCooldownRemaining = Profile.WeaponCooldown;
            float damage = target.ApplyDamage(Profile.WeaponDamage);
            return new CombatEvent(Id, target.Id, "Structure", damage, target.IsDestroyed);
        }

        public void SetCurrentTarget(UnitState target)
        {
            CurrentTargetId = target?.Id;
        }

        public void SetCurrentTargetId(string targetId)
        {
            CurrentTargetId = targetId;
        }

        public bool IsNear(Vector2 missionPoint, float distance)
        {
            return (MissionPosition - missionPoint).sqrMagnitude <= distance * distance;
        }

        private DamageResult ApplyDamage(float damage, string attackerId)
        {
            if (IsDestroyed)
            {
                return new DamageResult("", 0f);
            }

            DamageSection section = SelectDamageSection(attackerId);
            float remaining = section.ApplyDamage(damage);
            float applied = damage - remaining;
            CurrentStructure = Mathf.Max(0f, CurrentStructure - applied);
            LastHitSection = section.Name;
            LastDamageTaken = applied;

            if (CurrentStructure <= 0f || HasDestroyedCriticalSection())
            {
                IsDestroyed = true;
                HasMoveOrder = false;
                IsDetached = false;
                CurrentTargetId = null;
                CurrentStructure = 0f;
            }

            return new DamageResult(section.Name, applied);
        }

        private bool HasDestroyedCriticalSection()
        {
            foreach (DamageSection section in Sections)
            {
                if (section.IsCritical && section.IsDestroyed)
                {
                    return true;
                }
            }

            return false;
        }

        private DamageSection SelectDamageSection(string attackerId)
        {
            int hash = (Id + attackerId).GetHashCode() & 0x7fffffff;
            int roll = hash % 100;
            if (roll < 12)
            {
                return Sections[0];
            }

            if (roll < 34)
            {
                return Sections[1];
            }

            if (roll < 56)
            {
                return Sections[2];
            }

            if (roll < 78)
            {
                return Sections[3];
            }

            return Sections[4];
        }

        private static DamageSection[] CreateSections(CombatProfile profile)
        {
            if (profile.Sections != null && profile.Sections.Length > 0)
            {
                DamageSection[] sections = new DamageSection[profile.Sections.Length];
                for (int index = 0; index < profile.Sections.Length; index++)
                {
                    CombatSectionDefinition definition = profile.Sections[index];
                    string name = string.IsNullOrEmpty(definition.name) ? "Section " + index : definition.name;
                    sections[index] = new DamageSection(name, Mathf.Max(1f, definition.structure), definition.critical);
                }

                return sections;
            }

            float maxStructure = profile.MaxStructure;
            return new[]
            {
                new DamageSection("Cockpit", maxStructure * 0.12f, true),
                new DamageSection("Torso", maxStructure * 0.34f),
                new DamageSection("Left Arm", maxStructure * 0.17f),
                new DamageSection("Right Arm", maxStructure * 0.17f),
                new DamageSection("Legs", maxStructure * 0.20f)
            };
        }

        private readonly struct DamageResult
        {
            public string SectionName { get; }
            public float DamageApplied { get; }

            public DamageResult(string sectionName, float damageApplied)
            {
                SectionName = sectionName;
                DamageApplied = damageApplied;
            }
        }
    }
}
