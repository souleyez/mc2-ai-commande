using System;
using System.Collections.Generic;

namespace MC2Demo.BattleCore
{
    public sealed class LoadoutValidationResult
    {
        private readonly List<string> errors = new();

        public bool IsValid => errors.Count == 0;
        public string[] Errors => errors.ToArray();
        public float TotalHeat { get; internal set; }
        public float TotalWeight { get; internal set; }
        public int OccupiedGridCells { get; internal set; }

        internal void AddError(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                errors.Add(error);
            }
        }
    }

    public static class LoadoutValidator
    {
        public static LoadoutValidationResult Validate(LoadoutContract contract, LoadoutBuildDefinition loadout)
        {
            LoadoutValidationResult result = new();
            if (contract == null)
            {
                result.AddError("Loadout contract is missing.");
                return result;
            }

            if (loadout == null)
            {
                result.AddError("Loadout build is missing.");
                return result;
            }

            LoadoutChassisDefinition chassis = FindChassis(contract, loadout.chassisId);
            if (chassis == null)
            {
                result.AddError("Unknown chassis: " + loadout.chassisId);
                return result;
            }

            if (chassis.slotGrid == null || chassis.slotGrid.width <= 0 || chassis.slotGrid.height <= 0)
            {
                result.AddError("Chassis has no valid slot grid: " + chassis.chassisId);
                return result;
            }

            Dictionary<string, string> occupiedGrid = new(StringComparer.Ordinal);
            Dictionary<string, string> occupiedSlots = new(StringComparer.Ordinal);
            HashSet<string> blockedCells = BuildBlockedCellSet(chassis.slotGrid.blockedCells);

            LoadoutPlacedItemDefinition[] placedItems = loadout.placedItems ?? Array.Empty<LoadoutPlacedItemDefinition>();
            for (int index = 0; index < placedItems.Length; index++)
            {
                LoadoutPlacedItemDefinition placedItem = placedItems[index];
                if (placedItem == null)
                {
                    result.AddError("Placed item is missing at index " + index + ".");
                    continue;
                }

                LoadoutItemDefinition item = FindItem(contract, placedItem.itemId);
                if (item == null)
                {
                    result.AddError("Unknown item: " + placedItem.itemId);
                    continue;
                }

                result.TotalHeat += Math.Max(0f, item.heat);
                result.TotalWeight += Math.Max(0f, item.weight);

                if (!string.IsNullOrEmpty(placedItem.equipmentSlotId))
                {
                    ValidateEquipmentSlot(chassis, item, placedItem, occupiedSlots, result);
                    continue;
                }

                ValidateGridPlacement(chassis.slotGrid, item, placedItem, blockedCells, occupiedGrid, result);
            }

            result.OccupiedGridCells = occupiedGrid.Count;
            if (chassis.heatLimit > 0f && result.TotalHeat > chassis.heatLimit)
            {
                result.AddError("Loadout heat exceeds chassis limit.");
            }

            if (chassis.weightLimit > 0f && result.TotalWeight > chassis.weightLimit)
            {
                result.AddError("Loadout weight exceeds chassis limit.");
            }

            return result;
        }

        private static void ValidateEquipmentSlot(
            LoadoutChassisDefinition chassis,
            LoadoutItemDefinition item,
            LoadoutPlacedItemDefinition placedItem,
            Dictionary<string, string> occupiedSlots,
            LoadoutValidationResult result)
        {
            LoadoutEquipmentSlotDefinition slot = FindEquipmentSlot(chassis, placedItem.equipmentSlotId);
            if (slot == null)
            {
                result.AddError("Unknown equipment slot: " + placedItem.equipmentSlotId);
                return;
            }

            if (!ItemAllowsEquipmentSlot(item, slot.slotType))
            {
                result.AddError("Item " + item.itemId + " cannot use equipment slot type " + slot.slotType + ".");
            }

            if (occupiedSlots.TryGetValue(slot.slotId, out string previousItemId))
            {
                result.AddError("Equipment slot " + slot.slotId + " is occupied by both " + previousItemId + " and " + item.itemId + ".");
                return;
            }

            occupiedSlots[slot.slotId] = item.itemId;
        }

        private static void ValidateGridPlacement(
            LoadoutSlotGridDefinition grid,
            LoadoutItemDefinition item,
            LoadoutPlacedItemDefinition placedItem,
            HashSet<string> blockedCells,
            Dictionary<string, string> occupiedGrid,
            LoadoutValidationResult result)
        {
            LoadoutGridCell[] shapeCells = TransformShapeCells(item.shapeCells, placedItem.rotated);
            if (shapeCells.Length == 0)
            {
                result.AddError("Grid item has no shape cells: " + item.itemId);
                return;
            }

            for (int index = 0; index < shapeCells.Length; index++)
            {
                LoadoutGridCell shapeCell = shapeCells[index];
                int x = placedItem.gridX + shapeCell.x;
                int y = placedItem.gridY + shapeCell.y;
                string cellKey = CellKey(x, y);

                if (x < 0 || y < 0 || x >= grid.width || y >= grid.height)
                {
                    result.AddError("Item " + item.itemId + " occupies out-of-bounds cell " + cellKey + ".");
                    continue;
                }

                if (blockedCells.Contains(cellKey))
                {
                    result.AddError("Item " + item.itemId + " occupies blocked cell " + cellKey + ".");
                    continue;
                }

                if (occupiedGrid.TryGetValue(cellKey, out string previousItemId))
                {
                    result.AddError("Grid cell " + cellKey + " is occupied by both " + previousItemId + " and " + item.itemId + ".");
                    continue;
                }

                occupiedGrid[cellKey] = item.itemId;
            }
        }

        private static LoadoutGridCell[] TransformShapeCells(LoadoutGridCell[] cells, bool rotated)
        {
            if (cells == null || cells.Length == 0)
            {
                return Array.Empty<LoadoutGridCell>();
            }

            LoadoutGridCell[] transformed = new LoadoutGridCell[cells.Length];
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            for (int index = 0; index < cells.Length; index++)
            {
                LoadoutGridCell cell = cells[index] ?? new LoadoutGridCell();
                int x = rotated ? cell.y : cell.x;
                int y = rotated ? -cell.x : cell.y;
                transformed[index] = new LoadoutGridCell { x = x, y = y };
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
            }

            if (!rotated)
            {
                return transformed;
            }

            for (int index = 0; index < transformed.Length; index++)
            {
                transformed[index].x -= minX;
                transformed[index].y -= minY;
            }

            return transformed;
        }

        private static HashSet<string> BuildBlockedCellSet(LoadoutGridCell[] blockedCells)
        {
            HashSet<string> blocked = new(StringComparer.Ordinal);
            if (blockedCells == null)
            {
                return blocked;
            }

            for (int index = 0; index < blockedCells.Length; index++)
            {
                LoadoutGridCell cell = blockedCells[index];
                if (cell != null)
                {
                    blocked.Add(CellKey(cell.x, cell.y));
                }
            }

            return blocked;
        }

        private static LoadoutChassisDefinition FindChassis(LoadoutContract contract, string chassisId)
        {
            LoadoutChassisDefinition[] chassisDefinitions = contract.chassisDefinitions ?? Array.Empty<LoadoutChassisDefinition>();
            for (int index = 0; index < chassisDefinitions.Length; index++)
            {
                LoadoutChassisDefinition chassis = chassisDefinitions[index];
                if (chassis != null && string.Equals(chassis.chassisId, chassisId, StringComparison.Ordinal))
                {
                    return chassis;
                }
            }

            return null;
        }

        private static LoadoutItemDefinition FindItem(LoadoutContract contract, string itemId)
        {
            LoadoutItemDefinition[] itemDefinitions = contract.itemDefinitions ?? Array.Empty<LoadoutItemDefinition>();
            for (int index = 0; index < itemDefinitions.Length; index++)
            {
                LoadoutItemDefinition item = itemDefinitions[index];
                if (item != null && string.Equals(item.itemId, itemId, StringComparison.Ordinal))
                {
                    return item;
                }
            }

            return null;
        }

        private static LoadoutEquipmentSlotDefinition FindEquipmentSlot(LoadoutChassisDefinition chassis, string slotId)
        {
            LoadoutEquipmentSlotDefinition[] slots = chassis.equipmentSlots ?? Array.Empty<LoadoutEquipmentSlotDefinition>();
            for (int index = 0; index < slots.Length; index++)
            {
                LoadoutEquipmentSlotDefinition slot = slots[index];
                if (slot != null && string.Equals(slot.slotId, slotId, StringComparison.Ordinal))
                {
                    return slot;
                }
            }

            return null;
        }

        private static bool ItemAllowsEquipmentSlot(LoadoutItemDefinition item, string slotType)
        {
            string[] allowedTypes = item.allowedEquipmentSlotTypes ?? Array.Empty<string>();
            for (int index = 0; index < allowedTypes.Length; index++)
            {
                if (string.Equals(allowedTypes[index], slotType, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string CellKey(int x, int y)
        {
            return x + "," + y;
        }
    }
}
