using System;
using UnityEngine;

namespace MC2Demo.BattleCore
{
    public sealed class UnitState
    {
        private const float ArriveDistance = 20f;
        private const float JumpDuration = 0.45f;

        public string Id { get; }
        public string UnitType { get; }
        public string Brain { get; }
        public int TeamId { get; }
        public bool IsPlayerUnit { get; }
        public CombatProfile Profile { get; }
        public DamageSection[] Sections { get; }
        public bool IsDestroyed { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsDetached { get; private set; }
        public bool HasMoveOrder { get; private set; }
        public bool HasAttackOrder => !string.IsNullOrEmpty(AttackTargetId);
        public string AttackTargetId { get; private set; }
        public string CurrentTargetId { get; private set; }
        public string LastHitSection { get; private set; }
        public float WeaponCooldownRemaining { get; private set; }
        public float LastDamageTaken { get; private set; }
        public float JumpLift { get; private set; }
        public float CurrentStructure { get; private set; }
        public float Structure => Profile.MaxStructure <= 0f ? 0f : CurrentStructure / Profile.MaxStructure;
        public float MobilityRatio => IsDestroyed ? 0f : IsSectionDestroyed("Legs") ? 0.45f : 1f;
        public float FirepowerRatio => IsDestroyed ? 0f : CalculateFirepowerRatio();
        public bool CanUseJumpJets => !IsDestroyed && !IsSectionDestroyed("Legs");
        public bool IsHeatLocked => !IsDestroyed && EffectiveHeatPerShot() > 0f && CurrentHeat + EffectiveHeatPerShot() > Profile.HeatCapacity;
        public float CurrentHeat { get; private set; }
        public float HeatRatio => Profile.HeatCapacity <= 0f ? 0f : CurrentHeat / Profile.HeatCapacity;
        public float WeaponCooldownRatio => Profile.WeaponCooldown <= 0f ? 0f : WeaponCooldownRemaining / Profile.WeaponCooldown;
        public Vector2 MissionPosition { get; private set; }
        public Vector2 MoveTarget { get; private set; }
        private Vector2 jumpStart;
        private Vector2 jumpEnd;
        private float jumpElapsed;

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
            AttackTargetId = null;
        }

        public void SetAttackOrder(string targetId, Vector2 targetPosition, bool detached)
        {
            if (IsDestroyed || string.IsNullOrEmpty(targetId))
            {
                return;
            }

            AttackTargetId = targetId;
            CurrentTargetId = targetId;
            MoveTarget = targetPosition;
            HasMoveOrder = true;
            IsDetached = detached;
        }

        public void UpdateAttackOrder(Vector2 targetPosition, bool targetInRange)
        {
            if (!HasAttackOrder || IsDestroyed || IsJumping)
            {
                return;
            }

            MoveTarget = targetPosition;
            HasMoveOrder = !targetInRange;
        }

        public void CompleteAttackOrder()
        {
            AttackTargetId = null;
            CurrentTargetId = null;
            HasMoveOrder = false;
            IsDetached = false;
        }

        public bool TryStartJumpToward(Vector2 missionPoint, float jumpDistance, Func<Vector2, bool> isLandingAllowed, bool detached)
        {
            if (!CanUseJumpJets || IsJumping)
            {
                return false;
            }

            Vector2 toTarget = missionPoint - MissionPosition;
            float distance = toTarget.magnitude;
            if (distance <= ArriveDistance)
            {
                return false;
            }

            Vector2 landingPoint = MissionPosition + toTarget.normalized * Mathf.Min(Mathf.Max(1f, jumpDistance), distance);
            if (isLandingAllowed != null && !isLandingAllowed(landingPoint))
            {
                return false;
            }

            jumpStart = MissionPosition;
            jumpEnd = landingPoint;
            jumpElapsed = 0f;
            MoveTarget = landingPoint;
            HasMoveOrder = true;
            IsDetached = detached;
            IsJumping = true;
            JumpLift = 0f;
            AttackTargetId = null;
            CurrentTargetId = null;
            return true;
        }

        public void TickMovement(float deltaTime)
        {
            if (IsDestroyed)
            {
                IsJumping = false;
                JumpLift = 0f;
                return;
            }

            if (IsJumping)
            {
                TickJump(deltaTime);
                return;
            }

            if (!HasMoveOrder)
            {
                return;
            }

            Vector2 toTarget = MoveTarget - MissionPosition;
            float distance = toTarget.magnitude;
            if (distance <= ArriveDistance)
            {
                MissionPosition = MoveTarget;
                HasMoveOrder = false;
                if (!HasAttackOrder)
                {
                    IsDetached = false;
                }

                return;
            }

            float step = EffectiveMoveSpeed() * deltaTime;
            MissionPosition += toTarget.normalized * Mathf.Min(step, distance);
        }

        private void TickJump(float deltaTime)
        {
            jumpElapsed += Mathf.Max(0f, deltaTime);
            float progress = Mathf.Clamp01(jumpElapsed / JumpDuration);
            float eased = Mathf.SmoothStep(0f, 1f, progress);
            MissionPosition = Vector2.Lerp(jumpStart, jumpEnd, eased);
            JumpLift = Mathf.Sin(progress * Mathf.PI);

            if (progress >= 1f)
            {
                MissionPosition = jumpEnd;
                HasMoveOrder = false;
                IsDetached = false;
                IsJumping = false;
                JumpLift = 0f;
            }
        }

        public void TickWeapon(float deltaTime)
        {
            if (CurrentHeat > 0f && Profile.HeatDissipationPerSecond > 0f)
            {
                CurrentHeat = Mathf.Max(0f, CurrentHeat - (Profile.HeatDissipationPerSecond * Mathf.Max(0f, deltaTime)));
            }

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
                && EffectiveWeaponDamage() > 0.1f
                && !IsHeatLocked
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
                && EffectiveWeaponDamage() > 0.1f
                && !IsHeatLocked
                && Vector2.Distance(MissionPosition, target.MissionPosition) <= Profile.WeaponRange + target.Radius;
        }

        public CombatEvent FireAt(UnitState target)
        {
            CurrentTargetId = target.Id;
            WeaponCooldownRemaining = Profile.WeaponCooldown;
            AddWeaponHeat();
            DamageResult result = target.ApplyDamage(EffectiveWeaponDamage(), Id);
            return new CombatEvent(
                Id,
                target.Id,
                result.SectionName,
                result.DamageApplied,
                target.IsDestroyed,
                Profile.PrimaryWeaponType,
                Profile.PrimarySpecialEffect);
        }

        public CombatEvent FireAt(StructureState target)
        {
            CurrentTargetId = target.Id;
            WeaponCooldownRemaining = Profile.WeaponCooldown;
            AddWeaponHeat();
            float damage = target.ApplyDamage(EffectiveWeaponDamage());
            return new CombatEvent(
                Id,
                target.Id,
                "Structure",
                damage,
                target.IsDestroyed,
                Profile.PrimaryWeaponType,
                Profile.PrimarySpecialEffect);
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
            DamageResult result = ApplyDamageWithOverflow(section, damage);
            CurrentStructure = Mathf.Max(0f, CurrentStructure - result.DamageApplied);
            LastHitSection = result.SectionName;
            LastDamageTaken = result.DamageApplied;
            EvaluateDestruction();

            return result;
        }

        public float ApplyDirectSectionDamage(string sectionName, float damage)
        {
            if (IsDestroyed || damage <= 0f)
            {
                return 0f;
            }

            DamageSection section = FindSection(sectionName);
            if (section == null)
            {
                return 0f;
            }

            DamageResult result = ApplyDamageWithOverflow(section, damage);
            CurrentStructure = Mathf.Max(0f, CurrentStructure - result.DamageApplied);
            LastHitSection = result.SectionName;
            LastDamageTaken = result.DamageApplied;
            EvaluateDestruction();
            return result.DamageApplied;
        }

        private void EvaluateDestruction()
        {
            if (CurrentStructure <= 0f || HasDestroyedCriticalSection())
            {
                IsDestroyed = true;
                HasMoveOrder = false;
                IsDetached = false;
                AttackTargetId = null;
                CurrentTargetId = null;
                CurrentStructure = 0f;
            }
        }

        private DamageResult ApplyDamageWithOverflow(DamageSection firstSection, float damage)
        {
            float remaining = damage;
            float applied = 0f;
            string lastHitSectionName = firstSection.Name;

            remaining = ApplyDamageToSection(firstSection, remaining, ref applied, ref lastHitSectionName);
            if (remaining <= 0f)
            {
                return new DamageResult(lastHitSectionName, applied);
            }

            foreach (DamageSection section in Sections)
            {
                if (section == firstSection || section.IsDestroyed)
                {
                    continue;
                }

                remaining = ApplyDamageToSection(section, remaining, ref applied, ref lastHitSectionName);
                if (remaining <= 0f)
                {
                    break;
                }
            }

            return new DamageResult(lastHitSectionName, applied);
        }

        private static float ApplyDamageToSection(DamageSection section, float damage, ref float applied, ref string lastHitSectionName)
        {
            float remaining = section.ApplyDamage(damage);
            float sectionApplied = damage - remaining;
            if (sectionApplied > 0f)
            {
                applied += sectionApplied;
                lastHitSectionName = section.Name;
            }

            return remaining;
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

        private DamageSection FindSection(string sectionName)
        {
            foreach (DamageSection section in Sections)
            {
                if (string.Equals(section.Name, sectionName, StringComparison.OrdinalIgnoreCase))
                {
                    return section;
                }
            }

            return null;
        }

        private bool IsSectionDestroyed(string sectionName)
        {
            DamageSection section = FindSection(sectionName);
            return section != null && section.IsDestroyed;
        }

        private float CalculateFirepowerRatio()
        {
            float ratio = 1f;
            if (IsSectionDestroyed("Left Arm"))
            {
                ratio -= 0.3f;
            }

            if (IsSectionDestroyed("Right Arm"))
            {
                ratio -= 0.3f;
            }

            if (IsSectionDestroyed("Torso"))
            {
                ratio -= 0.25f;
            }

            return Mathf.Clamp(ratio, 0.25f, 1f);
        }

        private float EffectiveMoveSpeed()
        {
            return Profile.MoveSpeed * MobilityRatio;
        }

        private float EffectiveWeaponDamage()
        {
            return Profile.WeaponDamage * FirepowerRatio;
        }

        private float EffectiveHeatPerShot()
        {
            return Profile.HeatPerShot * FirepowerRatio;
        }

        private void AddWeaponHeat()
        {
            float heat = EffectiveHeatPerShot();
            if (heat <= 0f || Profile.HeatCapacity <= 0f)
            {
                return;
            }

            CurrentHeat = Mathf.Min(Profile.HeatCapacity, CurrentHeat + heat);
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
