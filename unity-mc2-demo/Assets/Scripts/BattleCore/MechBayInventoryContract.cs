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
        public int MechFragmentCount { get; internal set; }
    }

    public sealed class MechBayInventoryUsage
    {
        public int ArmorPlateCount { get; private set; }
        public int HeatSinkCount { get; private set; }

        public void AddLoadoutPreview(CombatLoadoutPreview preview)
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
                    AddArmorPlates(1);
                }
                else if (cell.Category == LoadoutItemCategory.HeatSink)
                {
                    AddHeatSinks(1);
                }
            }
        }

        public void AddArmorPlates(int count)
        {
            ArmorPlateCount += Math.Max(0, count);
        }

        public void AddHeatSinks(int count)
        {
            HeatSinkCount += Math.Max(0, count);
        }
    }

    public sealed class MechBayInventoryAvailabilityResult
    {
        private readonly List<string> errors = new();

        public bool IsValid => errors.Count == 0;
        public string[] Errors => errors.ToArray();
        public MechBayInventoryUsage Usage { get; internal set; }
        public int AvailableArmorPlateCount { get; internal set; }
        public int AvailableHeatSinkCount { get; internal set; }

        internal void AddError(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                errors.Add(error);
            }
        }
    }

    public sealed class MechBayRepairResult
    {
        public bool Accepted { get; internal set; }
        public string Message { get; internal set; }
        public int Cost { get; internal set; }
        public int TokenBalance { get; internal set; }
    }

    public sealed class MechBayMissionReceipt
    {
        public bool Applied { get; internal set; }
        public int TokenDelta { get; internal set; }
        public int TokenBalance { get; internal set; }
        public int SalvageFragmentCount { get; internal set; }
        public MechBayReceiptItemDefinition[] ItemStacks { get; internal set; }
    }

    public sealed class MechBayReceiptItemDefinition
    {
        public string itemId;
        public string displayName;
        public string category;
        public int quantity;
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

        public static MechBayInventoryAvailabilityResult ValidateUsage(
            MechBayInventoryContract contract,
            MechBayInventoryUsage usage)
        {
            MechBayInventoryAvailabilityResult result = new()
            {
                Usage = usage ?? new MechBayInventoryUsage(),
                AvailableArmorPlateCount = CountAvailableItems(contract, LoadoutItemCategory.ArmorPlate),
                AvailableHeatSinkCount = CountAvailableItems(contract, LoadoutItemCategory.HeatSink)
            };

            MechBayInventoryValidationResult contractResult = Validate(contract);
            foreach (string error in contractResult.Errors)
            {
                result.AddError(error);
            }

            if (result.Usage.ArmorPlateCount > result.AvailableArmorPlateCount)
            {
                result.AddError(
                    "Armor plates "
                    + result.Usage.ArmorPlateCount
                    + "/"
                    + result.AvailableArmorPlateCount);
            }

            if (result.Usage.HeatSinkCount > result.AvailableHeatSinkCount)
            {
                result.AddError(
                    "Heat sinks "
                    + result.Usage.HeatSinkCount
                    + "/"
                    + result.AvailableHeatSinkCount);
            }

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
                case LoadoutItemCategory.MechFragment:
                    result.Summary.MechFragmentCount += Math.Max(0, stack.quantity);
                    break;
                default:
                    result.AddError("Unknown inventory item category: " + stack.category);
                    break;
            }
        }

        private static int CountAvailableItems(MechBayInventoryContract contract, string category)
        {
            int count = 0;
            MechBayItemStackDefinition[] itemStacks = contract?.itemStacks ?? Array.Empty<MechBayItemStackDefinition>();
            for (int index = 0; index < itemStacks.Length; index++)
            {
                MechBayItemStackDefinition stack = itemStacks[index];
                if (stack != null && string.Equals(stack.category, category, StringComparison.Ordinal))
                {
                    count += Math.Max(0, stack.quantity);
                }
            }

            return count;
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

    public static class MechBayRepairService
    {
        public static int EstimateRepairCostResourcePoints(UnitState unit)
        {
            if (unit == null)
            {
                return 0;
            }

            float missingStructure = Math.Max(0f, unit.Profile.MaxStructure - unit.CurrentStructure);
            float missingSectionStructure = 0f;
            foreach (DamageSection section in unit.Sections)
            {
                missingSectionStructure += Math.Max(0f, section.MaxHitPoints - section.HitPoints);
            }

            return (int)Math.Ceiling((missingStructure * 30f) + (missingSectionStructure * 12f));
        }

        public static MechBayRepairResult TryRepair(MechBayInventoryContract inventory, UnitState unit)
        {
            int cost = EstimateRepairCostResourcePoints(unit);
            MechBayRepairResult result = new()
            {
                Cost = cost,
                TokenBalance = Math.Max(0, inventory?.tokenBalance ?? 0)
            };

            if (inventory == null)
            {
                result.Message = "Inventory missing";
                return result;
            }

            if (unit == null)
            {
                result.Message = "No mech selected";
                return result;
            }

            if (cost <= 0)
            {
                result.Accepted = true;
                result.Message = "No repair needed";
                return result;
            }

            if (inventory.tokenBalance < cost)
            {
                result.Message = "Need " + cost + " token";
                return result;
            }

            inventory.tokenBalance -= cost;
            unit.RepairToFull();
            result.Accepted = true;
            result.TokenBalance = inventory.tokenBalance;
            result.Message = "Repaired " + unit.UnitType;
            return result;
        }
    }

    public static class MechBayMissionReceiptService
    {
        public static MechBayMissionReceipt ApplyMissionReceipt(
            MechBayInventoryContract inventory,
            MissionResultSummary summary)
        {
            MechBayMissionReceipt receipt = BuildMissionReceipt(summary);
            receipt.TokenBalance = Math.Max(0, inventory?.tokenBalance ?? 0);
            if (inventory == null)
            {
                return receipt;
            }

            inventory.tokenBalance = Math.Max(0, inventory.tokenBalance + receipt.TokenDelta);
            MechBayReceiptItemDefinition[] itemStacks = receipt.ItemStacks ?? Array.Empty<MechBayReceiptItemDefinition>();
            for (int index = 0; index < itemStacks.Length; index++)
            {
                MechBayReceiptItemDefinition item = itemStacks[index];
                if (item == null)
                {
                    continue;
                }

                AddInventoryStack(inventory, item.itemId, item.displayName, item.category, item.quantity);
            }

            receipt.Applied = true;
            receipt.TokenBalance = inventory.tokenBalance;
            return receipt;
        }

        public static MechBayMissionReceipt BuildMissionReceipt(MissionResultSummary summary)
        {
            List<MechBayReceiptItemDefinition> itemStacks = new();
            string[] destroyedEnemyLabels = summary?.destroyedEnemyUnitLabels ?? Array.Empty<string>();
            for (int index = 0; index < destroyedEnemyLabels.Length; index++)
            {
                string unitType = ExtractUnitType(destroyedEnemyLabels[index]);
                string itemId = "fragment-" + SafeItemId(unitType);
                AddReceiptStack(
                    itemStacks,
                    itemId,
                    unitType + " Fragment",
                    LoadoutItemCategory.MechFragment,
                    1);
            }

            int tokenDelta = Math.Max(0, summary?.completedRewardResourcePoints ?? 0);
            int salvageCount = 0;
            for (int index = 0; index < itemStacks.Count; index++)
            {
                salvageCount += Math.Max(0, itemStacks[index]?.quantity ?? 0);
            }

            return new MechBayMissionReceipt
            {
                TokenDelta = tokenDelta,
                SalvageFragmentCount = salvageCount,
                ItemStacks = itemStacks.ToArray()
            };
        }

        private static void AddReceiptStack(
            List<MechBayReceiptItemDefinition> itemStacks,
            string itemId,
            string displayName,
            string category,
            int quantity)
        {
            if (quantity <= 0)
            {
                return;
            }

            MechBayReceiptItemDefinition stack = FindReceiptStack(itemStacks, itemId);
            if (stack == null)
            {
                itemStacks.Add(new MechBayReceiptItemDefinition
                {
                    itemId = itemId,
                    displayName = displayName,
                    category = category,
                    quantity = quantity
                });
                return;
            }

            stack.quantity += quantity;
        }

        private static MechBayReceiptItemDefinition FindReceiptStack(
            List<MechBayReceiptItemDefinition> itemStacks,
            string itemId)
        {
            for (int index = 0; index < itemStacks.Count; index++)
            {
                MechBayReceiptItemDefinition stack = itemStacks[index];
                if (stack != null && string.Equals(stack.itemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    return stack;
                }
            }

            return null;
        }

        private static void AddInventoryStack(
            MechBayInventoryContract inventory,
            string itemId,
            string displayName,
            string category,
            int quantity)
        {
            if (inventory == null || quantity <= 0)
            {
                return;
            }

            List<MechBayItemStackDefinition> itemStacks = new(inventory.itemStacks ?? Array.Empty<MechBayItemStackDefinition>());
            MechBayItemStackDefinition stack = FindInventoryStack(itemStacks, itemId);
            if (stack == null)
            {
                itemStacks.Add(new MechBayItemStackDefinition
                {
                    itemId = itemId,
                    displayName = displayName,
                    category = category,
                    quantity = quantity,
                    equippedQuantity = 0
                });
            }
            else
            {
                stack.quantity += quantity;
            }

            inventory.itemStacks = itemStacks.ToArray();
        }

        private static MechBayItemStackDefinition FindInventoryStack(
            List<MechBayItemStackDefinition> itemStacks,
            string itemId)
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

        private static string ExtractUnitType(string unitLabel)
        {
            if (string.IsNullOrWhiteSpace(unitLabel))
            {
                return "Unknown";
            }

            string trimmed = unitLabel.Trim();
            int firstSpace = trimmed.IndexOf(' ');
            return firstSpace < 0 || firstSpace + 1 >= trimmed.Length
                ? trimmed
                : trimmed.Substring(firstSpace + 1).Trim();
        }

        private static string SafeItemId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "unknown";
            }

            char[] chars = value.Trim().ToLowerInvariant().ToCharArray();
            for (int index = 0; index < chars.Length; index++)
            {
                char character = chars[index];
                if ((character < 'a' || character > 'z') && (character < '0' || character > '9'))
                {
                    chars[index] = '-';
                }
            }

            return new string(chars).Trim('-');
        }
    }
}
