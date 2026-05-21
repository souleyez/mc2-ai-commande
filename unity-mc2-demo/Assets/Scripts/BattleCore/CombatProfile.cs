using System;
using UnityEngine;

namespace MC2Demo.BattleCore
{
    public sealed class CombatProfile
    {
        public float MaxStructure { get; }
        public float MoveSpeed { get; }
        public float WeaponRange { get; }
        public float WeaponDamage { get; }
        public float WeaponCooldown { get; }
        public float HeatCapacity { get; }
        public float HeatPerShot { get; }
        public float HeatDissipationPerSecond { get; }
        public string PrimaryWeaponName { get; }
        public string PrimaryWeaponType { get; }
        public int PrimarySpecialEffect { get; }
        public CombatWeaponDefinition[] Weapons { get; }
        public float TotalWeaponWeight { get; }
        public CombatSectionDefinition[] Sections { get; }
        public string SourceKind { get; }

        private CombatProfile(
            float maxStructure,
            float moveSpeed,
            float weaponRange,
            float weaponDamage,
            float weaponCooldown,
            float heatCapacity,
            float heatPerShot,
            float heatDissipationPerSecond,
            string primaryWeaponName,
            string primaryWeaponType,
            int primarySpecialEffect,
            CombatWeaponDefinition[] weapons,
            CombatSectionDefinition[] sections,
            string sourceKind)
        {
            MaxStructure = maxStructure;
            MoveSpeed = moveSpeed;
            WeaponRange = weaponRange;
            WeaponDamage = weaponDamage;
            WeaponCooldown = weaponCooldown;
            HeatCapacity = heatCapacity;
            HeatPerShot = heatPerShot;
            HeatDissipationPerSecond = heatDissipationPerSecond;
            PrimaryWeaponName = primaryWeaponName;
            PrimaryWeaponType = primaryWeaponType;
            PrimarySpecialEffect = primarySpecialEffect;
            Weapons = weapons ?? Array.Empty<CombatWeaponDefinition>();
            TotalWeaponWeight = CalculateTotalWeaponWeight(Weapons);
            Sections = sections;
            SourceKind = sourceKind;
        }

        public static CombatProfile FromData(CombatUnitProfile record, bool isPlayerUnit)
        {
            if (record?.combatProfile == null)
            {
                return Fallback(record?.unitType, isPlayerUnit);
            }

            CombatProfileFields fields = record.combatProfile;
            CombatWeaponDefinition[] weapons = record.weapons ?? Array.Empty<CombatWeaponDefinition>();
            CombatWeaponDefinition primaryWeapon = SelectPrimaryWeapon(record);
            return new CombatProfile(
                fields.maxStructure,
                fields.moveSpeed,
                fields.weaponRange,
                fields.weaponDamage,
                fields.weaponCooldown,
                CalculateHeatCapacity(record, fields),
                CalculateHeatPerShot(record),
                CalculateHeatDissipationPerSecond(record, fields),
                string.IsNullOrEmpty(primaryWeapon?.name) ? "Aggregate Weapons" : primaryWeapon.name,
                string.IsNullOrEmpty(primaryWeapon?.type) ? "Generic" : primaryWeapon.type,
                primaryWeapon?.specialEffect ?? 0,
                weapons,
                record.sections,
                string.IsNullOrEmpty(record.sourceKind) ? "combat-data" : record.sourceKind);
        }

        public static CombatProfile Fallback(string unitType, bool isPlayerUnit)
        {
            switch (unitType)
            {
                case "Werewolf":
                    return MakeFallback(145f, 250f, 780f, 22f, 1.35f, isPlayerUnit);
                case "Bushwacker":
                    return MakeFallback(175f, 220f, 900f, 28f, 1.75f, isPlayerUnit);
                case "Starslayer":
                    return MakeFallback(190f, 210f, 860f, 30f, 1.7f, isPlayerUnit);
                case "UrbanMech":
                    return MakeFallback(125f, 145f, 780f, 24f, 2.05f, isPlayerUnit);
                case "Centipede":
                    return MakeFallback(95f, 230f, 520f, 14f, 1.25f, isPlayerUnit);
                case "Harasser":
                    return MakeFallback(70f, 315f, 480f, 11f, 1.05f, isPlayerUnit);
                case "LRMC":
                    return MakeFallback(80f, 185f, 1120f, 18f, 2.15f, isPlayerUnit);
                case "Infantry":
                    return MakeFallback(32f, 170f, 360f, 5f, 0.9f, isPlayerUnit);
                default:
                    return isPlayerUnit
                        ? MakeFallback(140f, 220f, 760f, 20f, 1.6f, true)
                        : MakeFallback(80f, 180f, 520f, 12f, 1.5f, false);
            }
        }

        private static CombatProfile MakeFallback(
            float maxStructure,
            float moveSpeed,
            float weaponRange,
            float weaponDamage,
            float weaponCooldown,
            bool isPlayerUnit)
        {
            return new CombatProfile(
                maxStructure,
                moveSpeed,
                weaponRange,
                weaponDamage,
                weaponCooldown,
                Mathf.Max(1f, maxStructure * 0.15f),
                0f,
                0f,
                "Fallback Weapon",
                "Generic",
                0,
                Array.Empty<CombatWeaponDefinition>(),
                DefaultSections(maxStructure, isPlayerUnit),
                "hardcoded-fallback");
        }

        private static float CalculateTotalWeaponWeight(CombatWeaponDefinition[] weapons)
        {
            if (weapons == null || weapons.Length == 0)
            {
                return 0f;
            }

            float total = 0f;
            foreach (CombatWeaponDefinition weapon in weapons)
            {
                if (weapon != null && weapon.weight > 0f)
                {
                    total += weapon.weight;
                }
            }

            return total;
        }

        private static float CalculateHeatCapacity(CombatUnitProfile record, CombatProfileFields fields)
        {
            if (record != null && record.heatIndex > 0f)
            {
                return record.heatIndex;
            }

            return Mathf.Max(1f, fields.maxStructure * 0.15f);
        }

        private static float CalculateHeatPerShot(CombatUnitProfile record)
        {
            if (record?.weapons == null || record.weapons.Length == 0)
            {
                return 0f;
            }

            float heat = 0f;
            foreach (CombatWeaponDefinition weapon in record.weapons)
            {
                if (weapon != null && weapon.heat > 0f)
                {
                    heat += weapon.heat;
                }
            }

            return heat;
        }

        private static float CalculateHeatDissipationPerSecond(CombatUnitProfile record, CombatProfileFields fields)
        {
            float heatCapacity = CalculateHeatCapacity(record, fields);
            float heatPerShot = CalculateHeatPerShot(record);
            if (heatPerShot <= 0f)
            {
                return 0f;
            }

            float cooldown = Mathf.Max(1f, fields.weaponCooldown);
            return Mathf.Max(heatCapacity * 0.10f, heatPerShot / (cooldown * 1.35f));
        }

        private static CombatWeaponDefinition SelectPrimaryWeapon(CombatUnitProfile record)
        {
            if (record?.weapons == null || record.weapons.Length == 0)
            {
                return null;
            }

            CombatWeaponDefinition primary = null;
            float bestScore = float.MinValue;
            foreach (CombatWeaponDefinition weapon in record.weapons)
            {
                if (weapon == null)
                {
                    continue;
                }

                float score = weapon.damagePerTenSeconds > 0f
                    ? weapon.damagePerTenSeconds
                    : weapon.damage;
                score += Mathf.Max(0f, weapon.rangeMax) * 0.001f;
                if (score > bestScore)
                {
                    bestScore = score;
                    primary = weapon;
                }
            }

            return primary;
        }

        private static CombatSectionDefinition[] DefaultSections(float maxStructure, bool isPlayerUnit)
        {
            float cockpit = maxStructure * (isPlayerUnit ? 0.16f : 0.12f);
            return new[]
            {
                new CombatSectionDefinition { name = "Cockpit", structure = cockpit, critical = true },
                new CombatSectionDefinition { name = "Torso", structure = maxStructure * 0.34f },
                new CombatSectionDefinition { name = "Left Arm", structure = maxStructure * 0.17f },
                new CombatSectionDefinition { name = "Right Arm", structure = maxStructure * 0.17f },
                new CombatSectionDefinition { name = "Legs", structure = maxStructure - cockpit - (maxStructure * 0.68f) }
            };
        }
    }
}
