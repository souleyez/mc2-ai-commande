using System;
using System.Collections.Generic;

namespace MC2Demo.BattleCore
{
    [Serializable]
    public sealed class MechBayInventoryContract
    {
        public string schema;
        public int tokenBalance;
        public MechBayOwnedMechDefinition[] ownedMechs;
        public MechBayItemStackDefinition[] itemStacks;
    }

    [Serializable]
    public sealed class MechBayOwnedMechDefinition
    {
        public string ownedMechId;
        public string unitId;
        public string unitType;
        public string chassisId;
        public string displayName;
        public string activeLoadoutId;
        public bool availableForMission;
        public int conditionPercent;
    }

    [Serializable]
    public sealed class MechBayItemStackDefinition
    {
        public string itemId;
        public string displayName;
        public string category;
        public int quantity;
        public int equippedQuantity;
    }

    public sealed class MechBayInventoryValidationResult
    {
        private readonly List<string> errors = new();

        public bool IsValid => errors.Count == 0;
        public string[] Errors => errors.ToArray();
        public MechBayInventorySummary Summary { get; } = new();

        internal void AddError(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                errors.Add(error);
            }
        }
    }

    public sealed class MechBayInventorySummary
    {
        public int TokenBalance { get; internal set; }
        public int MechCount { get; internal set; }
        public int WeaponCount { get; internal set; }
        public int ArmorPlateCount { get; internal set; }
        public int HeatSinkCount { get; internal set; }
    }

    public static class MechBayInventoryValidator
    {
        public const string Schema = "mc2-mech-bay-inventory-v1";

        public static MechBayInventoryValidationResult Validate(MechBayInventoryContract contract)
        {
            MechBayInventoryValidationResult result = new();
            if (contract == null)
            {
                result.AddError("Mech bay inventory contract is missing.");
                return result;
            }

            result.Summary.TokenBalance = Math.Max(0, contract.tokenBalance);
            if (!string.Equals(contract.schema, Schema, StringComparison.Ordinal))
            {
                result.AddError("Unexpected mech bay inventory schema: " + contract.schema);
            }

            ValidateOwnedMechs(contract.ownedMechs, result);
            ValidateItemStacks(contract.itemStacks, result);
            return result;
        }

        private static void ValidateOwnedMechs(
            MechBayOwnedMechDefinition[] ownedMechs,
            MechBayInventoryValidationResult result)
        {
            if (ownedMechs == null || ownedMechs.Length == 0)
            {
                result.AddError("Mech bay inventory has no owned mechs.");
                return;
            }

            HashSet<string> ownedIds = new(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < ownedMechs.Length; index++)
            {
                MechBayOwnedMechDefinition mech = ownedMechs[index];
                if (mech == null)
                {
                    result.AddError("Owned mech is missing at index " + index + ".");
                    continue;
                }

                result.Summary.MechCount++;
                if (string.IsNullOrWhiteSpace(mech.ownedMechId))
                {
                    result.AddError("Owned mech has no inventory id at index " + index + ".");
                }
                else if (!ownedIds.Add(mech.ownedMechId))
                {
                    result.AddError("Duplicate owned mech id: " + mech.ownedMechId);
                }

                if (string.IsNullOrWhiteSpace(mech.unitType) || string.IsNullOrWhiteSpace(mech.chassisId))
                {
                    result.AddError("Owned mech is missing unit type or chassis id: " + mech.ownedMechId);
                }

                if (mech.conditionPercent < 0 || mech.conditionPercent > 100)
                {
                    result.AddError("Owned mech condition is out of range: " + mech.ownedMechId);
                }
            }
        }

        private static void ValidateItemStacks(
            MechBayItemStackDefinition[] itemStacks,
            MechBayInventoryValidationResult result)
        {
            if (itemStacks == null || itemStacks.Length == 0)
            {
                result.AddError("Mech bay inventory has no item stacks.");
                return;
            }

            HashSet<string> itemIds = new(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < itemStacks.Length; index++)
            {
                MechBayItemStackDefinition stack = itemStacks[index];
                if (stack == null)
                {
                    result.AddError("Item stack is missing at index " + index + ".");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(stack.itemId))
                {
                    result.AddError("Item stack has no item id at index " + index + ".");
                }
                else if (!itemIds.Add(stack.itemId))
                {
                    result.AddError("Duplicate item stack id: " + stack.itemId);
                }

                if (stack.quantity < 0 || stack.equippedQuantity < 0 || stack.equippedQuantity > stack.quantity)
                {
                    result.AddError("Item stack quantity is invalid: " + stack.itemId);
                }

                AddStackToSummary(stack, result);
            }
        }

        private static void AddStackToSummary(MechBayItemStackDefinition stack, MechBayInventoryValidationResult result)
        {
            switch (stack.category)
            {
                case LoadoutItemCategory.Weapon:
                    result.Summary.WeaponCount += Math.Max(0, stack.quantity);
                    break;
                case LoadoutItemCategory.ArmorPlate:
                    result.Summary.ArmorPlateCount += Math.Max(0, stack.quantity);
                    break;
                case LoadoutItemCategory.HeatSink:
                    result.Summary.HeatSinkCount += Math.Max(0, stack.quantity);
                    break;
                default:
                    result.AddError("Unknown inventory item category: " + stack.category);
                    break;
            }
        }
    }

    public static class MechBayInventoryBuilder
    {
        private const int DemoTokenBalance = 12000;

        public static MechBayInventoryContract BuildDemoInventory(IEnumerable<UnitState> units)
        {
            List<MechBayOwnedMechDefinition> ownedMechs = new();
            List<MechBayItemStackDefinition> itemStacks = new();
            if (units != null)
            {
                int mechIndex = 0;
                foreach (UnitState unit in units)
                {
                    if (unit == null)
                    {
                        continue;
                    }

                    mechIndex++;
                    ownedMechs.Add(new MechBayOwnedMechDefinition
                    {
                        ownedMechId = "demo-mech-" + mechIndex.ToString("00"),
                        unitId = unit.Id,
                        unitType = unit.UnitType,
                        chassisId = unit.UnitType,
                        displayName = unit.UnitType + " " + unit.Id,
                        activeLoadoutId = unit.UnitType + "-source",
                        availableForMission = unit.IsActive && !unit.IsDestroyed,
                        conditionPercent = ConditionPercent(unit)
                    });

                    AddWeaponStacks(itemStacks, unit.Profile?.Weapons);
                    AddPreviewEquipmentStacks(itemStacks, CombatLoadoutPreviewBuilder.Build(unit.UnitType, unit.Profile));
                }
            }

            return new MechBayInventoryContract
            {
                schema = MechBayInventoryValidator.Schema,
                tokenBalance = DemoTokenBalance,
                ownedMechs = ownedMechs.ToArray(),
                itemStacks = itemStacks.ToArray()
            };
        }

        private static void AddWeaponStacks(List<MechBayItemStackDefinition> itemStacks, CombatWeaponDefinition[] weapons)
        {
            if (weapons == null)
            {
                return;
            }

            for (int index = 0; index < weapons.Length; index++)
            {
                CombatWeaponDefinition weapon = weapons[index];
                if (weapon == null)
                {
                    continue;
                }

                AddItemStack(
                    itemStacks,
                    WeaponItemId(weapon, index),
                    string.IsNullOrWhiteSpace(weapon.name) ? "Source Weapon" : weapon.name,
                    LoadoutItemCategory.Weapon,
                    1,
                    1);
            }
        }

        private static void AddPreviewEquipmentStacks(List<MechBayItemStackDefinition> itemStacks, CombatLoadoutPreview preview)
        {
            CombatLoadoutPreviewGridCell[] cells = preview?.OccupiedCells ?? Array.Empty<CombatLoadoutPreviewGridCell>();
            for (int index = 0; index < cells.Length; index++)
            {
                CombatLoadoutPreviewGridCell cell = cells[index];
                if (cell == null)
                {
                    continue;
                }

                if (cell.Category == LoadoutItemCategory.ArmorPlate)
                {
                    AddItemStack(itemStacks, "demo-armor-plate", "Armor Plate", LoadoutItemCategory.ArmorPlate, 1, 1);
                }
                else if (cell.Category == LoadoutItemCategory.HeatSink)
                {
                    AddItemStack(itemStacks, "demo-heat-sink", "Heat Sink", LoadoutItemCategory.HeatSink, 1, 1);
                }
            }
        }

        private static void AddItemStack(
            List<MechBayItemStackDefinition> itemStacks,
            string itemId,
            string displayName,
            string category,
            int quantity,
            int equippedQuantity)
        {
            if (quantity <= 0 && equippedQuantity <= 0)
            {
                return;
            }

            MechBayItemStackDefinition stack = FindItemStack(itemStacks, itemId);
            if (stack == null)
            {
                itemStacks.Add(new MechBayItemStackDefinition
                {
                    itemId = itemId,
                    displayName = displayName,
                    category = category,
                    quantity = Math.Max(0, quantity),
                    equippedQuantity = Math.Max(0, equippedQuantity)
                });
                return;
            }

            stack.quantity += Math.Max(0, quantity);
            stack.equippedQuantity += Math.Max(0, equippedQuantity);
        }

        private static MechBayItemStackDefinition FindItemStack(List<MechBayItemStackDefinition> itemStacks, string itemId)
        {
            for (int index = 0; index < itemStacks.Count; index++)
            {
                MechBayItemStackDefinition stack = itemStacks[index];
                if (stack != null && string.Equals(stack.itemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    return stack;
                }
            }

            return null;
        }

        private static string WeaponItemId(CombatWeaponDefinition weapon, int index)
        {
            if (weapon.componentId > 0)
            {
                return "source-component-" + weapon.componentId;
            }

            return "source-weapon-" + index;
        }

        private static int ConditionPercent(UnitState unit)
        {
            if (unit?.Profile == null || unit.Profile.MaxStructure <= 0f)
            {
                return 0;
            }

            float ratio = Math.Max(0f, Math.Min(1f, unit.CurrentStructure / unit.Profile.MaxStructure));
            return (int)Math.Round(ratio * 100f);
        }
    }
}
