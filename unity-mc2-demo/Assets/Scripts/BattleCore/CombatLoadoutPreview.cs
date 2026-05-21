using System;

namespace MC2Demo.BattleCore
{
    public sealed class CombatLoadoutPreview
    {
        public LoadoutValidationResult Validation { get; }
        public int GridCapacity { get; }
        public int GridWidth { get; }
        public int GridHeight { get; }
        public CombatLoadoutPreviewGridCell[] OccupiedCells { get; }
        public float HeatLimit { get; }
        public float WeightLimit { get; }

        internal CombatLoadoutPreview(
            LoadoutValidationResult validation,
            int gridCapacity,
            int gridWidth,
            int gridHeight,
            CombatLoadoutPreviewGridCell[] occupiedCells,
            float heatLimit,
            float weightLimit)
        {
            Validation = validation;
            GridCapacity = gridCapacity;
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            OccupiedCells = occupiedCells ?? Array.Empty<CombatLoadoutPreviewGridCell>();
            HeatLimit = heatLimit;
            WeightLimit = weightLimit;
        }
    }

    public sealed class CombatLoadoutPreviewGridCell
    {
        public int X { get; }
        public int Y { get; }
        public int SourceWeaponIndex { get; }
        public string ItemId { get; }
        public string DisplayName { get; }

        internal CombatLoadoutPreviewGridCell(int x, int y, int sourceWeaponIndex, string itemId, string displayName)
        {
            X = x;
            Y = y;
            SourceWeaponIndex = sourceWeaponIndex;
            ItemId = itemId;
            DisplayName = displayName;
        }
    }

    public static class CombatLoadoutPreviewBuilder
    {
        private const int ProjectedGridWidth = 4;

        public static CombatLoadoutPreview Build(string chassisId, CombatProfile profile)
        {
            return Build(chassisId, profile, null);
        }

        public static CombatLoadoutPreview Build(string chassisId, CombatProfile profile, bool[] enabledWeapons)
        {
            CombatWeaponDefinition[] weapons = profile?.Weapons ?? Array.Empty<CombatWeaponDefinition>();
            int gridHeight = Math.Max(1, (weapons.Length + ProjectedGridWidth - 1) / ProjectedGridWidth);
            int gridCapacity = ProjectedGridWidth * gridHeight;
            float heatCapacity = profile == null ? 0f : profile.HeatCapacity;
            float heatPerShot = profile == null ? 0f : profile.HeatPerShot;
            float loadLimit = profile == null ? 0f : profile.LoadLimit;
            float totalWeaponWeight = profile == null ? 0f : profile.TotalWeaponWeight;
            float heatLimit = heatCapacity > 0f ? heatCapacity : Math.Max(1f, heatPerShot);
            float weightLimit = loadLimit > 0f ? loadLimit : Math.Max(1f, totalWeaponWeight);
            string safeChassisId = string.IsNullOrEmpty(chassisId) ? "source-profile" : chassisId;

            int enabledCount = CountEnabledWeapons(weapons.Length, enabledWeapons);
            LoadoutItemDefinition[] items = new LoadoutItemDefinition[enabledCount];
            LoadoutPlacedItemDefinition[] placedItems = new LoadoutPlacedItemDefinition[enabledCount];
            CombatLoadoutPreviewGridCell[] occupiedCells = new CombatLoadoutPreviewGridCell[enabledCount];
            int placedIndex = 0;
            for (int index = 0; index < weapons.Length; index++)
            {
                if (!IsWeaponEnabled(enabledWeapons, index))
                {
                    continue;
                }

                CombatWeaponDefinition weapon = weapons[index];
                string itemId = "source-weapon-" + index;
                items[placedIndex] = new LoadoutItemDefinition
                {
                    itemId = itemId,
                    displayName = string.IsNullOrEmpty(weapon?.name) ? itemId : weapon.name,
                    category = LoadoutItemCategory.Weapon,
                    weaponType = weapon?.type,
                    sourceComponentId = weapon?.componentId ?? 0,
                    heat = weapon?.heat ?? 0f,
                    weight = weapon?.weight ?? 0f,
                    damage = weapon?.damage ?? 0f,
                    range = weapon?.rangeMax ?? 0f,
                    cooldown = weapon?.recycleTime ?? 0f,
                    shapeCells = new[]
                    {
                        new LoadoutGridCell { x = 0, y = 0 }
                    }
                };

                placedItems[placedIndex] = new LoadoutPlacedItemDefinition
                {
                    itemId = itemId,
                    gridX = index % ProjectedGridWidth,
                    gridY = index / ProjectedGridWidth
                };
                occupiedCells[placedIndex] = new CombatLoadoutPreviewGridCell(
                    index % ProjectedGridWidth,
                    index / ProjectedGridWidth,
                    index,
                    itemId,
                    string.IsNullOrEmpty(weapon?.name) ? itemId : weapon.name);
                placedIndex++;
            }

            LoadoutContract contract = new()
            {
                schema = "mc2-loadout-preview-v1",
                chassisDefinitions = new[]
                {
                    new LoadoutChassisDefinition
                    {
                        chassisId = safeChassisId,
                        displayName = safeChassisId,
                        heatLimit = heatLimit,
                        weightLimit = weightLimit,
                        slotGrid = new LoadoutSlotGridDefinition
                        {
                            width = ProjectedGridWidth,
                            height = gridHeight
                        }
                    }
                },
                itemDefinitions = items,
                loadouts = new[]
                {
                    new LoadoutBuildDefinition
                    {
                        loadoutId = safeChassisId + "-source",
                        chassisId = safeChassisId,
                        displayName = safeChassisId + " source projection",
                        placedItems = placedItems
                    }
                }
            };

            LoadoutValidationResult validation = LoadoutValidator.Validate(contract, contract.loadouts[0]);
            return new CombatLoadoutPreview(validation, gridCapacity, ProjectedGridWidth, gridHeight, occupiedCells, heatLimit, weightLimit);
        }

        private static int CountEnabledWeapons(int weaponCount, bool[] enabledWeapons)
        {
            if (enabledWeapons == null)
            {
                return weaponCount;
            }

            int count = 0;
            for (int index = 0; index < weaponCount; index++)
            {
                if (IsWeaponEnabled(enabledWeapons, index))
                {
                    count++;
                }
            }

            return count;
        }

        private static bool IsWeaponEnabled(bool[] enabledWeapons, int index)
        {
            return enabledWeapons == null || index >= enabledWeapons.Length || enabledWeapons[index];
        }
    }
}
