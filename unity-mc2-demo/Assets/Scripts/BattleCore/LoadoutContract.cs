using System;

namespace MC2Demo.BattleCore
{
    [Serializable]
    public sealed class LoadoutContract
    {
        public string schema;
        public LoadoutChassisDefinition[] chassisDefinitions;
        public LoadoutItemDefinition[] itemDefinitions;
        public LoadoutBuildDefinition[] loadouts;
    }

    [Serializable]
    public sealed class LoadoutChassisDefinition
    {
        public string chassisId;
        public string displayName;
        public string sourceKind;
        public float heatLimit;
        public float weightLimit;
        public LoadoutSlotGridDefinition slotGrid;
        public LoadoutEquipmentSlotDefinition[] equipmentSlots;
        public LoadoutSectionDefinition[] sections;
    }

    [Serializable]
    public sealed class LoadoutSlotGridDefinition
    {
        public int width;
        public int height;
        public LoadoutGridCell[] blockedCells;
    }

    [Serializable]
    public sealed class LoadoutEquipmentSlotDefinition
    {
        public string slotId;
        public string slotType;
        public string sectionName;
    }

    [Serializable]
    public sealed class LoadoutSectionDefinition
    {
        public string sectionName;
        public float baseStructure;
        public bool critical;
    }

    [Serializable]
    public sealed class LoadoutItemDefinition
    {
        public string itemId;
        public string displayName;
        public string category;
        public string weaponType;
        public int sourceComponentId;
        public float heat;
        public float weight;
        public float damage;
        public float range;
        public float cooldown;
        public float armorHardnessBonus;
        public float heatDissipationBonus;
        public LoadoutGridCell[] shapeCells;
        public string[] allowedEquipmentSlotTypes;
    }

    [Serializable]
    public sealed class LoadoutBuildDefinition
    {
        public string loadoutId;
        public string chassisId;
        public string displayName;
        public LoadoutPlacedItemDefinition[] placedItems;
    }

    [Serializable]
    public sealed class LoadoutPlacedItemDefinition
    {
        public string itemId;
        public int gridX;
        public int gridY;
        public bool rotated;
        public string equipmentSlotId;
        public string sectionName;
    }

    [Serializable]
    public sealed class LoadoutGridCell
    {
        public int x;
        public int y;
    }

    public static class LoadoutItemCategory
    {
        public const string Weapon = "Weapon";
        public const string ArmorPlate = "ArmorPlate";
        public const string HeatSink = "HeatSink";
        public const string Radar = "Radar";
        public const string JumpJet = "JumpJet";
        public const string MechFragment = "MechFragment";
    }

    public static class LoadoutEquipmentSlotType
    {
        public const string Radar = "Radar";
        public const string JumpJet = "JumpJet";
    }
}
