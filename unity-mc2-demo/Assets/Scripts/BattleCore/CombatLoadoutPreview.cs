using System;
using System.Collections.Generic;

namespace MC2Demo.BattleCore
{
    public sealed class CombatLoadoutPreview
    {
        public LoadoutValidationResult Validation { get; }
        public int GridCapacity { get; }
        public int GridWidth { get; }
        public int GridHeight { get; }
        public CombatLoadoutPreviewGridCell[] OccupiedCells { get; }
        public CombatLoadoutPreviewItem[] Items { get; }
        public float HeatLimit { get; }
        public float WeightLimit { get; }

        internal CombatLoadoutPreview(
            LoadoutValidationResult validation,
            int gridCapacity,
            int gridWidth,
            int gridHeight,
            CombatLoadoutPreviewGridCell[] occupiedCells,
            CombatLoadoutPreviewItem[] items,
            float heatLimit,
            float weightLimit)
        {
            Validation = validation;
            GridCapacity = gridCapacity;
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            OccupiedCells = occupiedCells ?? Array.Empty<CombatLoadoutPreviewGridCell>();
            Items = items ?? Array.Empty<CombatLoadoutPreviewItem>();
            HeatLimit = heatLimit;
            WeightLimit = weightLimit;
        }
    }

    public sealed class CombatLoadoutPreviewItem
    {
        public int SourceWeaponIndex { get; }
        public int GridX { get; }
        public int GridY { get; }
        public string ItemId { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public string WeaponType { get; }

        internal CombatLoadoutPreviewItem(
            int sourceWeaponIndex,
            int gridX,
            int gridY,
            string itemId,
            string displayName,
            string category,
            string weaponType)
        {
            SourceWeaponIndex = sourceWeaponIndex;
            GridX = gridX;
            GridY = gridY;
            ItemId = itemId;
            DisplayName = displayName;
            Category = category;
            WeaponType = weaponType;
        }
    }

    public sealed class CombatLoadoutPreviewGridCell
    {
        public int X { get; }
        public int Y { get; }
        public int SourceWeaponIndex { get; }
        public string ItemId { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public string WeaponType { get; }

        internal CombatLoadoutPreviewGridCell(
            int x,
            int y,
            int sourceWeaponIndex,
            string itemId,
            string displayName,
            string category,
            string weaponType)
        {
            X = x;
            Y = y;
            SourceWeaponIndex = sourceWeaponIndex;
            ItemId = itemId;
            DisplayName = displayName;
            Category = category;
            WeaponType = weaponType;
        }
    }

    public sealed class CombatLoadoutPlacementOverride
    {
        public int sourceWeaponIndex;
        public int gridX;
        public int gridY;
    }

    public static class CombatLoadoutPreviewBuilder
    {
        private const int ProjectedGridWidth = 4;
        private const int ProjectedFillerRows = 1;
        private const float ProjectedArmorPlateWeight = 0.5f;
        private const float ProjectedArmorHardnessBonus = 1f;
        private const float ProjectedHeatSinkWeight = 1f;
        private const float ProjectedHeatDissipationBonus = 1.5f;

        public static CombatLoadoutPreview Build(string chassisId, CombatProfile profile)
        {
            return Build(chassisId, profile, null, null);
        }

        public static CombatLoadoutPreview Build(string chassisId, CombatProfile profile, bool[] enabledWeapons)
        {
            return Build(chassisId, profile, enabledWeapons, null);
        }

        public static CombatLoadoutPreview Build(
            string chassisId,
            CombatProfile profile,
            bool[] enabledWeapons,
            CombatLoadoutPlacementOverride[] placementOverrides)
        {
            CombatWeaponDefinition[] weapons = profile?.Weapons ?? Array.Empty<CombatWeaponDefinition>();
            LoadoutGridCell[][] weaponShapes = BuildWeaponShapes(weapons);
            LoadoutPlacedItemDefinition[] sourcePlacements = PackWeaponShapes(weaponShapes, ProjectedGridWidth, out int gridHeight);
            ApplyPlacementOverrides(sourcePlacements, placementOverrides);
            gridHeight += ProjectedFillerRows;
            int gridCapacity = ProjectedGridWidth * gridHeight;
            float heatCapacity = profile == null ? 0f : profile.HeatCapacity;
            float heatPerShot = profile == null ? 0f : profile.HeatPerShot;
            float loadLimit = profile == null ? 0f : profile.LoadLimit;
            float totalWeaponWeight = profile == null ? 0f : profile.TotalWeaponWeight;
            float heatLimit = heatCapacity > 0f ? heatCapacity : Math.Max(1f, heatPerShot);
            float weightLimit = loadLimit > 0f ? loadLimit : Math.Max(1f, totalWeaponWeight);
            string safeChassisId = string.IsNullOrEmpty(chassisId) ? "source-profile" : chassisId;

            List<LoadoutItemDefinition> items = new();
            List<LoadoutPlacedItemDefinition> placedItems = new();
            List<CombatLoadoutPreviewGridCell> occupiedCells = new();
            List<CombatLoadoutPreviewItem> previewItems = new();
            bool[,] occupiedInBounds = new bool[ProjectedGridWidth, gridHeight];
            float projectedWeight = 0f;
            for (int index = 0; index < weapons.Length; index++)
            {
                if (!IsWeaponEnabled(enabledWeapons, index))
                {
                    continue;
                }

                CombatWeaponDefinition weapon = weapons[index];
                string itemId = "source-weapon-" + index;
                LoadoutItemDefinition item = new()
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
                    shapeCells = weaponShapes[index]
                };
                items.Add(item);

                placedItems.Add(new LoadoutPlacedItemDefinition
                {
                    itemId = itemId,
                    gridX = sourcePlacements[index].gridX,
                    gridY = sourcePlacements[index].gridY
                });
                previewItems.Add(new CombatLoadoutPreviewItem(
                    index,
                    sourcePlacements[index].gridX,
                    sourcePlacements[index].gridY,
                    itemId,
                    string.IsNullOrEmpty(weapon?.name) ? itemId : weapon.name,
                    LoadoutItemCategory.Weapon,
                    weapon?.type));
                foreach (LoadoutGridCell shapeCell in weaponShapes[index])
                {
                    int cellX = sourcePlacements[index].gridX + shapeCell.x;
                    int cellY = sourcePlacements[index].gridY + shapeCell.y;
                    occupiedCells.Add(new CombatLoadoutPreviewGridCell(
                        cellX,
                        cellY,
                        index,
                        itemId,
                        string.IsNullOrEmpty(weapon?.name) ? itemId : weapon.name,
                        LoadoutItemCategory.Weapon,
                        weapon?.type));
                    if (IsInsideGrid(cellX, cellY, ProjectedGridWidth, gridHeight))
                    {
                        occupiedInBounds[cellX, cellY] = true;
                    }
                }

                projectedWeight += Math.Max(0f, item.weight);
            }

            AddProjectedFillerItems(
                items,
                placedItems,
                previewItems,
                occupiedCells,
                occupiedInBounds,
                gridHeight,
                weightLimit,
                projectedWeight,
                heatPerShot,
                heatLimit);

            LoadoutItemDefinition[] itemDefinitions = items.ToArray();
            LoadoutPlacedItemDefinition[] placedItemDefinitions = placedItems.ToArray();
            CombatLoadoutPreviewItem[] previewItemDefinitions = previewItems.ToArray();
            CombatLoadoutPreviewGridCell[] occupiedCellDefinitions = occupiedCells.ToArray();

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
                itemDefinitions = itemDefinitions,
                loadouts = new[]
                {
                    new LoadoutBuildDefinition
                    {
                        loadoutId = safeChassisId + "-source",
                        chassisId = safeChassisId,
                        displayName = safeChassisId + " source projection",
                        placedItems = placedItemDefinitions
                    }
                }
            };

            LoadoutValidationResult validation = LoadoutValidator.Validate(contract, contract.loadouts[0]);
            return new CombatLoadoutPreview(
                validation,
                gridCapacity,
                ProjectedGridWidth,
                gridHeight,
                occupiedCellDefinitions,
                previewItemDefinitions,
                heatLimit,
                weightLimit);
        }

        private static void AddProjectedFillerItems(
            List<LoadoutItemDefinition> items,
            List<LoadoutPlacedItemDefinition> placedItems,
            List<CombatLoadoutPreviewItem> previewItems,
            List<CombatLoadoutPreviewGridCell> occupiedCells,
            bool[,] occupiedInBounds,
            int gridHeight,
            float weightLimit,
            float projectedWeight,
            float heatPerShot,
            float heatLimit)
        {
            float remainingWeight = weightLimit > 0f ? weightLimit - projectedWeight : 0f;
            int fillerIndex = 0;
            int heatSinkCount = 0;
            bool prefersCooling = heatLimit > 0f && heatPerShot > heatLimit * 0.60f;
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < ProjectedGridWidth; x++)
                {
                    if (occupiedInBounds[x, y])
                    {
                        continue;
                    }

                    bool useHeatSink = remainingWeight >= ProjectedHeatSinkWeight
                        && heatSinkCount < 2
                        && (prefersCooling || fillerIndex % 4 == 0);
                    if (!useHeatSink && remainingWeight < ProjectedArmorPlateWeight)
                    {
                        return;
                    }

                    string category = useHeatSink ? LoadoutItemCategory.HeatSink : LoadoutItemCategory.ArmorPlate;
                    string itemId = "projected-" + (useHeatSink ? "heat-sink-" : "armor-plate-") + fillerIndex;
                    string displayName = useHeatSink ? "Projected Heat Sink" : "Projected Armor Plate";
                    float itemWeight = useHeatSink ? ProjectedHeatSinkWeight : ProjectedArmorPlateWeight;
                    items.Add(new LoadoutItemDefinition
                    {
                        itemId = itemId,
                        displayName = displayName,
                        category = category,
                        weight = itemWeight,
                        armorHardnessBonus = useHeatSink ? 0f : ProjectedArmorHardnessBonus,
                        heatDissipationBonus = useHeatSink ? ProjectedHeatDissipationBonus : 0f,
                        shapeCells = SingleCellShape()
                    });
                    placedItems.Add(new LoadoutPlacedItemDefinition
                    {
                        itemId = itemId,
                        gridX = x,
                        gridY = y
                    });
                    int fillerSourceIndex = -1 - fillerIndex;
                    previewItems.Add(new CombatLoadoutPreviewItem(
                        fillerSourceIndex,
                        x,
                        y,
                        itemId,
                        displayName,
                        category,
                        null));
                    occupiedCells.Add(new CombatLoadoutPreviewGridCell(
                        x,
                        y,
                        fillerSourceIndex,
                        itemId,
                        displayName,
                        category,
                        null));
                    occupiedInBounds[x, y] = true;
                    remainingWeight -= itemWeight;
                    if (useHeatSink)
                    {
                        heatSinkCount++;
                    }

                    fillerIndex++;
                }
            }
        }

        private static bool IsWeaponEnabled(bool[] enabledWeapons, int index)
        {
            return enabledWeapons == null || index >= enabledWeapons.Length || enabledWeapons[index];
        }

        private static bool IsInsideGrid(int x, int y, int width, int height)
        {
            return x >= 0 && y >= 0 && x < width && y < height;
        }

        private static void ApplyPlacementOverrides(
            LoadoutPlacedItemDefinition[] placements,
            CombatLoadoutPlacementOverride[] placementOverrides)
        {
            if (placements == null || placementOverrides == null)
            {
                return;
            }

            for (int index = 0; index < placementOverrides.Length; index++)
            {
                CombatLoadoutPlacementOverride placementOverride = placementOverrides[index];
                if (placementOverride == null
                    || placementOverride.sourceWeaponIndex < 0
                    || placementOverride.sourceWeaponIndex >= placements.Length)
                {
                    continue;
                }

                placements[placementOverride.sourceWeaponIndex].gridX = placementOverride.gridX;
                placements[placementOverride.sourceWeaponIndex].gridY = placementOverride.gridY;
            }
        }

        private static LoadoutGridCell[][] BuildWeaponShapes(CombatWeaponDefinition[] weapons)
        {
            LoadoutGridCell[][] shapes = new LoadoutGridCell[weapons.Length][];
            for (int index = 0; index < weapons.Length; index++)
            {
                shapes[index] = BuildWeaponShape(weapons[index]);
            }

            return shapes;
        }

        private static LoadoutGridCell[] BuildWeaponShape(CombatWeaponDefinition weapon)
        {
            string name = weapon?.name ?? "";
            string type = weapon?.type ?? "";
            if (ContainsText(type, "Missile"))
            {
                return ContainsText(name, "LRM") ? VerticalShape(3) : VerticalShape(2);
            }

            if (ContainsText(type, "Ballistic"))
            {
                if (ContainsText(name, "MG") || (weapon?.weight ?? 0f) <= 2.5f)
                {
                    return SingleCellShape();
                }

                return new[]
                {
                    new LoadoutGridCell { x = 0, y = 0 },
                    new LoadoutGridCell { x = 1, y = 0 }
                };
            }

            if (ContainsText(type, "Energy"))
            {
                if (ContainsText(name, "PPC") || ContainsText(name, "Large") || (weapon?.weight ?? 0f) >= 4f)
                {
                    return VerticalShape(2);
                }
            }

            return SingleCellShape();
        }

        private static LoadoutGridCell[] SingleCellShape()
        {
            return new[] { new LoadoutGridCell { x = 0, y = 0 } };
        }

        private static LoadoutGridCell[] VerticalShape(int height)
        {
            LoadoutGridCell[] cells = new LoadoutGridCell[Math.Max(1, height)];
            for (int index = 0; index < cells.Length; index++)
            {
                cells[index] = new LoadoutGridCell { x = 0, y = index };
            }

            return cells;
        }

        private static LoadoutPlacedItemDefinition[] PackWeaponShapes(LoadoutGridCell[][] weaponShapes, int width, out int gridHeight)
        {
            int totalCells = CountShapeCells(weaponShapes);
            int minimumHeight = Math.Max(TallestShape(weaponShapes), Math.Max(1, (totalCells + width - 1) / width));
            int maximumHeight = Math.Max(minimumHeight, totalCells + 1);
            for (gridHeight = minimumHeight; gridHeight <= maximumHeight; gridHeight++)
            {
                if (TryPackWeaponShapes(weaponShapes, width, gridHeight, out LoadoutPlacedItemDefinition[] placements))
                {
                    return placements;
                }
            }

            gridHeight = maximumHeight;
            TryPackWeaponShapes(weaponShapes, width, gridHeight, out LoadoutPlacedItemDefinition[] fallbackPlacements);
            return fallbackPlacements;
        }

        private static bool TryPackWeaponShapes(
            LoadoutGridCell[][] weaponShapes,
            int width,
            int height,
            out LoadoutPlacedItemDefinition[] placements)
        {
            bool[,] occupied = new bool[width, height];
            placements = new LoadoutPlacedItemDefinition[weaponShapes.Length];
            for (int index = 0; index < weaponShapes.Length; index++)
            {
                LoadoutGridCell[] shape = weaponShapes[index];
                bool placed = false;
                for (int y = 0; y < height && !placed; y++)
                {
                    for (int x = 0; x < width && !placed; x++)
                    {
                        if (!ShapeFits(shape, x, y, width, height, occupied))
                        {
                            continue;
                        }

                        placements[index] = new LoadoutPlacedItemDefinition { gridX = x, gridY = y };
                        MarkShape(shape, x, y, occupied);
                        placed = true;
                    }
                }

                if (!placed)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ShapeFits(LoadoutGridCell[] shape, int originX, int originY, int width, int height, bool[,] occupied)
        {
            foreach (LoadoutGridCell cell in shape)
            {
                int x = originX + cell.x;
                int y = originY + cell.y;
                if (x < 0 || y < 0 || x >= width || y >= height || occupied[x, y])
                {
                    return false;
                }
            }

            return true;
        }

        private static void MarkShape(LoadoutGridCell[] shape, int originX, int originY, bool[,] occupied)
        {
            foreach (LoadoutGridCell cell in shape)
            {
                occupied[originX + cell.x, originY + cell.y] = true;
            }
        }

        private static int CountShapeCells(LoadoutGridCell[][] weaponShapes)
        {
            int count = 0;
            foreach (LoadoutGridCell[] shape in weaponShapes)
            {
                count += Math.Max(1, shape?.Length ?? 0);
            }

            return count;
        }

        private static int TallestShape(LoadoutGridCell[][] weaponShapes)
        {
            int tallest = 1;
            foreach (LoadoutGridCell[] shape in weaponShapes)
            {
                foreach (LoadoutGridCell cell in shape)
                {
                    tallest = Math.Max(tallest, cell.y + 1);
                }
            }

            return tallest;
        }

        private static bool ContainsText(string text, string value)
        {
            return !string.IsNullOrEmpty(text)
                && text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
