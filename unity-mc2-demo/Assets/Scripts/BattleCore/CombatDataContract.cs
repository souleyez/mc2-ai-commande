using System;
using System.Collections.Generic;
using UnityEngine;

namespace MC2Demo.BattleCore
{
    [Serializable]
    public sealed class CombatDataContract
    {
        public string schema;
        public CombatUnitProfile[] unitProfiles;
    }

    [Serializable]
    public sealed class CombatUnitProfile
    {
        public string unitType;
        public string source;
        public string sourceKind;
        public float heatIndex;
        public CombatSectionDefinition[] sections;
        public CombatWeaponDefinition[] weapons;
        public CombatProfileFields combatProfile;
    }

    [Serializable]
    public sealed class CombatProfileFields
    {
        public float maxStructure;
        public float moveSpeed;
        public float weaponRange;
        public float weaponDamage;
        public float weaponCooldown;
        public float sourceDamagePerTenSeconds;
    }

    [Serializable]
    public sealed class CombatSectionDefinition
    {
        public string name;
        public float structure;
        public bool critical;
    }

    [Serializable]
    public sealed class CombatWeaponDefinition
    {
        public int componentId;
        public string name;
        public string type;
        public string rangeBand;
        public float rangeMax;
        public float recycleTime;
        public float heat;
        public float weight;
        public float damage;
        public float damagePerTenSeconds;
        public float battleRating;
        public float price;
        public int ammoMasterId;
        public int specialEffect;
    }

    public sealed class CombatProfileCatalog
    {
        private readonly Dictionary<string, CombatUnitProfile> profiles = new(StringComparer.OrdinalIgnoreCase);

        public static CombatProfileCatalog Empty { get; } = new(null);

        public int UnitProfileCount => profiles.Count;

        public CombatProfileCatalog(CombatDataContract contract)
        {
            if (contract?.unitProfiles == null)
            {
                return;
            }

            foreach (CombatUnitProfile profile in contract.unitProfiles)
            {
                if (!string.IsNullOrWhiteSpace(profile?.unitType))
                {
                    profiles[profile.unitType] = profile;
                }
            }
        }

        public static CombatProfileCatalog FromJson(string json)
        {
            CombatDataContract contract = JsonUtility.FromJson<CombatDataContract>(json);
            if (contract == null)
            {
                throw new InvalidOperationException("Combat data JSON is empty or invalid.");
            }

            return new CombatProfileCatalog(contract);
        }

        public CombatProfile ForUnitType(string unitType, bool isPlayerUnit)
        {
            if (!string.IsNullOrEmpty(unitType) && profiles.TryGetValue(unitType, out CombatUnitProfile record))
            {
                return CombatProfile.FromData(record, isPlayerUnit);
            }

            return CombatProfile.Fallback(unitType, isPlayerUnit);
        }
    }
}
