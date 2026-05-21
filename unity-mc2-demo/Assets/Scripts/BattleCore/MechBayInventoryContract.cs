using System;
using System.Collections.Generic;
using System.Globalization;

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
        public string pilotId;
        public string pilotDisplayName;
        public string pilotType;
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

    public sealed class MechBayOwnedRosterEntry
    {
        public string ownedMechId { get; internal set; }
        public string unitId { get; internal set; }
        public string unitType { get; internal set; }
        public string chassisId { get; internal set; }
        public string displayName { get; internal set; }
        public string activeLoadoutId { get; internal set; }
        public string loadoutStatus { get; internal set; }
        public bool hasDraftFitStub { get; internal set; }
        public bool draftFitReady { get; internal set; }
        public string draftFitStatus { get; internal set; }
        public string draftFitRequirements { get; internal set; }
        public bool hasSpareWeaponStock { get; internal set; }
        public int spareWeaponStockCount { get; internal set; }
        public int totalWeaponStockCount { get; internal set; }
        public string spareWeaponStockStatus { get; internal set; }
        public bool hasPilotAssignment { get; internal set; }
        public bool hasPilotPlaceholder { get; internal set; }
        public string pilotStatus { get; internal set; }
        public string pilotDisplayName { get; internal set; }
        public bool deployableForMission { get; internal set; }
        public string deploymentStatus { get; internal set; }
        public string deploymentRequirements { get; internal set; }
        public bool hasSquadSelectionStub { get; internal set; }
        public bool squadSelectionCandidate { get; internal set; }
        public string squadSelectionStatus { get; internal set; }
        public string squadSelectionRequirements { get; internal set; }
        public bool availableForMission { get; internal set; }
        public int conditionPercent { get; internal set; }
        public bool isWarehouseMech { get; internal set; }
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
        public int AssembledMechCount { get; internal set; }
        public string[] AssembledMechNames { get; internal set; }
        public MechBayReceiptItemDefinition[] ItemStacks { get; internal set; }
    }

    public sealed class MechBayWeaponShopPreview
    {
        public int TokenBalance { get; internal set; }
        public string Status { get; internal set; }
        public MechBayWeaponShopEntry[] Entries { get; internal set; }
    }

    public sealed class MechBayWeaponShopEntry
    {
        public string itemId { get; internal set; }
        public string displayName { get; internal set; }
        public string weaponFamily { get; internal set; }
        public int tokenCost { get; internal set; }
        public bool canAfford { get; internal set; }
        public bool purchaseEnabled { get; internal set; }
        public string purchaseStatus { get; internal set; }
    }

    public sealed class MechBayWeaponPurchasePreviewResult
    {
        public bool Accepted { get; internal set; }
        public string itemId { get; internal set; }
        public string displayName { get; internal set; }
        public int TokenCost { get; internal set; }
        public int TokenBalance { get; internal set; }
        public bool CanAfford { get; internal set; }
        public bool InventoryChanged { get; internal set; }
        public string Message { get; internal set; }
    }

    public sealed class MechBayPilotHirePreview
    {
        public int TokenBalance { get; internal set; }
        public string Status { get; internal set; }
        public MechBayPilotHireCandidate[] Candidates { get; internal set; }
    }

    public sealed class MechBayPilotHireCandidate
    {
        public string pilotId { get; internal set; }
        public string displayName { get; internal set; }
        public string pilotType { get; internal set; }
        public int hireCost { get; internal set; }
        public bool canAfford { get; internal set; }
        public bool hireEnabled { get; internal set; }
        public string hireStatus { get; internal set; }
        public string riskProfile { get; internal set; }
    }

    public sealed class MechBayPilotHireResult
    {
        public bool Accepted { get; internal set; }
        public string ownedMechId { get; internal set; }
        public string pilotId { get; internal set; }
        public string displayName { get; internal set; }
        public int TokenCost { get; internal set; }
        public int TokenBalance { get; internal set; }
        public bool CanAfford { get; internal set; }
        public bool InventoryChanged { get; internal set; }
        public string RiskProfile { get; internal set; }
        public string Message { get; internal set; }
    }

    public sealed class MechBayWarehouseDraftFitPreview
    {
        public bool Ready { get; internal set; }
        public bool InventoryChanged { get; internal set; }
        public string Status { get; internal set; }
        public string Requirements { get; internal set; }
        public string ownedMechId { get; internal set; }
        public string displayName { get; internal set; }
        public string chassisId { get; internal set; }
        public string activeLoadoutId { get; internal set; }
        public string pilotDisplayName { get; internal set; }
        public string pilotStatus { get; internal set; }
        public string weaponItemId { get; internal set; }
        public string weaponDisplayName { get; internal set; }
        public int spareWeaponStockCount { get; internal set; }
        public string PreviewNote { get; internal set; }
    }

    public sealed class MechBayWarehouseDraftFitApplyResult
    {
        public bool Accepted { get; internal set; }
        public bool InventoryChanged { get; internal set; }
        public string ownedMechId { get; internal set; }
        public string displayName { get; internal set; }
        public string activeLoadoutId { get; internal set; }
        public string weaponItemId { get; internal set; }
        public string weaponDisplayName { get; internal set; }
        public int spareWeaponStockCount { get; internal set; }
        public string Message { get; internal set; }
    }

    public sealed class MechBaySquadSelectionPreview
    {
        public bool InventoryChanged { get; internal set; }
        public string Status { get; internal set; }
        public string PreviewNote { get; internal set; }
        public bool HasRefreshedMissionSlot { get; internal set; }
        public int MissionSlotCount { get; internal set; }
        public int CandidateCount { get; internal set; }
        public bool SwapEnabled { get; internal set; }
        public string SwapStatus { get; internal set; }
        public string SwapRequirements { get; internal set; }
        public bool DryRunAvailable { get; internal set; }
        public string DryRunStatus { get; internal set; }
        public string DryRunSummary { get; internal set; }
        public string DryRunOutgoingOwnedMechId { get; internal set; }
        public string DryRunIncomingOwnedMechId { get; internal set; }
        public bool PendingSwapAvailable { get; internal set; }
        public string PendingSwapStatus { get; internal set; }
        public string PendingSwapSummary { get; internal set; }
        public MechBaySquadSelectionSlot[] MissionSlots { get; internal set; }
        public MechBaySquadSelectionSlot[] DepotCandidates { get; internal set; }
    }

    public sealed class MechBaySquadSelectionSlot
    {
        public string ownedMechId { get; internal set; }
        public string unitType { get; internal set; }
        public string chassisId { get; internal set; }
        public string displayName { get; internal set; }
        public string activeLoadoutId { get; internal set; }
        public string pilotDisplayName { get; internal set; }
        public int conditionPercent { get; internal set; }
        public string selectionStatus { get; internal set; }
        public bool isDepotMissionSlot { get; internal set; }
    }

    public sealed class MechBaySquadSelectionApplyResult
    {
        public bool Accepted { get; internal set; }
        public bool InventoryChanged { get; internal set; }
        public string Message { get; internal set; }
        public string Reason { get; internal set; }
        public string Summary { get; internal set; }
        public string OutgoingOwnedMechId { get; internal set; }
        public string IncomingOwnedMechId { get; internal set; }
    }

    public sealed class MechBaySquadSelectionDraftState
    {
        public bool InventoryChanged { get; internal set; }
        public bool Ready { get; internal set; }
        public string Status { get; internal set; }
        public string Requirements { get; internal set; }
        public string Summary { get; internal set; }
        public string OutgoingOwnedMechId { get; internal set; }
        public string IncomingOwnedMechId { get; internal set; }
        public string OutgoingDisplayName { get; internal set; }
        public string IncomingDisplayName { get; internal set; }
    }

    public sealed class MechBayMissionHandoffPreview
    {
        public bool InventoryChanged { get; internal set; }
        public bool ReadyForFutureLaunch { get; internal set; }
        public bool LaunchEnabled { get; internal set; }
        public bool IncludesDepotMissionSlot { get; internal set; }
        public int MissionSlotCount { get; internal set; }
        public string Status { get; internal set; }
        public string LaunchStatus { get; internal set; }
        public string LaunchRequirements { get; internal set; }
        public string Summary { get; internal set; }
        public string PreviewNote { get; internal set; }
        public MechBaySquadSelectionSlot[] MissionSlots { get; internal set; }
    }

    public sealed class MechBayMissionHandoffLaunchGuard
    {
        public bool Accepted { get; internal set; }
        public bool InventoryChanged { get; internal set; }
        public bool LaunchEnabled { get; internal set; }
        public bool IncludesDepotMissionSlot { get; internal set; }
        public int MissionSlotCount { get; internal set; }
        public string Message { get; internal set; }
        public string Reason { get; internal set; }
        public string Summary { get; internal set; }
    }

    public sealed class MechBayMissionRestartDryRun
    {
        public bool InventoryChanged { get; internal set; }
        public bool Ready { get; internal set; }
        public bool CreatesMissionInstance { get; internal set; }
        public bool IncludesDepotMissionSlot { get; internal set; }
        public int MissionSlotCount { get; internal set; }
        public int SpawnIntentCount { get; internal set; }
        public string Status { get; internal set; }
        public string Summary { get; internal set; }
        public string PreviewNote { get; internal set; }
        public MechBayMissionRestartSpawnIntent[] SpawnIntents { get; internal set; }
    }

    public sealed class MechBayMissionRestartSpawnIntent
    {
        public int spawnIndex { get; internal set; }
        public string spawnRole { get; internal set; }
        public string ownedMechId { get; internal set; }
        public string unitType { get; internal set; }
        public string chassisId { get; internal set; }
        public string displayName { get; internal set; }
        public string activeLoadoutId { get; internal set; }
        public string pilotDisplayName { get; internal set; }
        public bool isDepotMissionSlot { get; internal set; }
    }

    public sealed class MechBayMissionRestartApplyGuard
    {
        public bool Accepted { get; internal set; }
        public bool InventoryChanged { get; internal set; }
        public bool ApplyEnabled { get; internal set; }
        public bool CreatesMissionInstance { get; internal set; }
        public bool IncludesDepotMissionSlot { get; internal set; }
        public int SpawnIntentCount { get; internal set; }
        public string Message { get; internal set; }
        public string Reason { get; internal set; }
        public string Summary { get; internal set; }
    }

    public sealed class MechBayMissionRestartContractPreview
    {
        public bool InventoryChanged { get; internal set; }
        public bool Ready { get; internal set; }
        public bool CreatesMissionInstance { get; internal set; }
        public bool IncludesDepotMissionSlot { get; internal set; }
        public int MissionSlotCount { get; internal set; }
        public int SpawnIntentCount { get; internal set; }
        public int PlayerTeamId { get; internal set; }
        public int CommanderId { get; internal set; }
        public string MissionTemplateId { get; internal set; }
        public string ContractSchema { get; internal set; }
        public string PatchMode { get; internal set; }
        public string CommanderOwnedMechId { get; internal set; }
        public string CommanderDisplayName { get; internal set; }
        public string UnitBrain { get; internal set; }
        public string Status { get; internal set; }
        public string Requirements { get; internal set; }
        public string Summary { get; internal set; }
        public string PreviewNote { get; internal set; }
        public MechBayMissionRestartSpawnIntent[] SpawnIntents { get; internal set; }
    }

    public sealed class MechBayReceiptItemDefinition
    {
        public string itemId;
        public string displayName;
        public string category;
        public int quantity;
    }

    public sealed class MechBayAssemblyProgress
    {
        public string unitType { get; internal set; }
        public string displayName { get; internal set; }
        public int fragments { get; internal set; }
        public int requiredFragments { get; internal set; }
        public bool canAssemble { get; internal set; }
    }

    public sealed class MechBayAssemblyResult
    {
        public int AssembledMechCount { get; internal set; }
        public string[] AssembledMechNames { get; internal set; }
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

                bool hasPilotId = !string.IsNullOrWhiteSpace(mech.pilotId);
                bool hasPilotDisplayName = !string.IsNullOrWhiteSpace(mech.pilotDisplayName);
                bool hasPilotType = !string.IsNullOrWhiteSpace(mech.pilotType);
                if ((hasPilotId || hasPilotDisplayName || hasPilotType)
                    && (!hasPilotId || !hasPilotDisplayName || !hasPilotType))
                {
                    result.AddError("Owned mech pilot assignment is incomplete: " + mech.ownedMechId);
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

    public static class MechBayOwnedRosterService
    {
        public static MechBayOwnedRosterEntry[] BuildRosterPreview(MechBayInventoryContract inventory)
        {
            MechBayOwnedMechDefinition[] ownedMechs = inventory?.ownedMechs ?? Array.Empty<MechBayOwnedMechDefinition>();
            int spareWeaponStockCount = CountAvailableItems(inventory, LoadoutItemCategory.Weapon);
            int totalWeaponStockCount = CountTotalItems(inventory, LoadoutItemCategory.Weapon);
            bool hasSpareWeaponStock = spareWeaponStockCount > 0;
            string spareWeaponStockStatus = WeaponStockStatus(spareWeaponStockCount, totalWeaponStockCount);
            List<MechBayOwnedRosterEntry> entries = new();
            for (int index = 0; index < ownedMechs.Length; index++)
            {
                MechBayOwnedMechDefinition mech = ownedMechs[index];
                if (mech == null)
                {
                    continue;
                }

                entries.Add(new MechBayOwnedRosterEntry
                {
                    ownedMechId = mech.ownedMechId,
                    unitId = mech.unitId,
                    unitType = mech.unitType,
                    chassisId = mech.chassisId,
                    displayName = string.IsNullOrWhiteSpace(mech.displayName) ? mech.unitType : mech.displayName,
                    activeLoadoutId = mech.activeLoadoutId,
                    loadoutStatus = LoadoutStatus(mech),
                    hasDraftFitStub = IsPendingDepotFit(mech),
                    draftFitReady = DraftFitReady(mech, hasSpareWeaponStock),
                    draftFitStatus = DraftFitStatus(mech, hasSpareWeaponStock),
                    draftFitRequirements = DraftFitRequirements(mech, hasSpareWeaponStock),
                    hasSpareWeaponStock = hasSpareWeaponStock,
                    spareWeaponStockCount = spareWeaponStockCount,
                    totalWeaponStockCount = totalWeaponStockCount,
                    spareWeaponStockStatus = spareWeaponStockStatus,
                    hasPilotAssignment = HasPilotAssignment(mech),
                    hasPilotPlaceholder = HasPilotPlaceholder(mech),
                    pilotStatus = PilotStatus(mech),
                    pilotDisplayName = PilotDisplayName(mech),
                    deployableForMission = DeployableForMission(mech),
                    deploymentStatus = DeploymentStatus(mech),
                    deploymentRequirements = DeploymentRequirements(mech, hasSpareWeaponStock),
                    hasSquadSelectionStub = HasSquadSelectionStub(mech),
                    squadSelectionCandidate = SquadSelectionCandidate(mech),
                    squadSelectionStatus = SquadSelectionStatus(mech),
                    squadSelectionRequirements = SquadSelectionRequirements(mech, hasSpareWeaponStock),
                    availableForMission = mech.availableForMission,
                    conditionPercent = Math.Max(0, Math.Min(100, mech.conditionPercent)),
                    isWarehouseMech = IsWarehouseMech(mech)
                });
            }

            return entries.ToArray();
        }

        private static bool IsWarehouseMech(MechBayOwnedMechDefinition mech)
        {
            return StartsWith(mech?.unitId, "warehouse-") || StartsWith(mech?.ownedMechId, "assembled-");
        }

        private static string LoadoutStatus(MechBayOwnedMechDefinition mech)
        {
            if (IsPendingDepotFit(mech))
            {
                return "Needs loadout";
            }

            return string.IsNullOrWhiteSpace(mech?.activeLoadoutId) ? "No loadout" : "Ready fit";
        }

        private static bool DraftFitReady(MechBayOwnedMechDefinition mech, bool hasSpareWeaponStock)
        {
            return IsPendingDepotFit(mech) && hasSpareWeaponStock && HasPilotAssignment(mech);
        }

        private static string DraftFitStatus(MechBayOwnedMechDefinition mech, bool hasSpareWeaponStock)
        {
            if (DraftFitReady(mech, hasSpareWeaponStock))
            {
                return "Draft fitting ready";
            }

            if (IsPendingDepotFit(mech))
            {
                return "Draft fitting locked for this demo";
            }

            return IsWarehouseMech(mech) ? "Depot fit read-only" : "Use squad fit cards below";
        }

        private static string DraftFitRequirements(MechBayOwnedMechDefinition mech, bool hasSpareWeaponStock)
        {
            if (!IsPendingDepotFit(mech))
            {
                return "Current fit active";
            }

            bool hasPilotAssignment = HasPilotAssignment(mech);
            if (hasSpareWeaponStock && hasPilotAssignment)
            {
                return "Ready for future fitting";
            }

            if (hasSpareWeaponStock)
            {
                return "Need pilot";
            }

            return hasPilotAssignment ? "Need stock weapons" : "Need stock weapons + pilot";
        }

        private static bool HasPilotAssignment(MechBayOwnedMechDefinition mech)
        {
            return !IsWarehouseMech(mech) || !string.IsNullOrWhiteSpace(mech?.pilotId);
        }

        private static bool HasPilotPlaceholder(MechBayOwnedMechDefinition mech)
        {
            return IsWarehouseMech(mech) && !HasPilotAssignment(mech);
        }

        private static string PilotStatus(MechBayOwnedMechDefinition mech)
        {
            if (!HasPilotAssignment(mech))
            {
                return "Pilot required";
            }

            string pilotType = string.IsNullOrWhiteSpace(mech?.pilotType) ? "" : mech.pilotType;
            return string.Equals(pilotType, "NPC", StringComparison.OrdinalIgnoreCase) ? "Assigned NPC" : "Assigned";
        }

        private static string PilotDisplayName(MechBayOwnedMechDefinition mech)
        {
            if (!string.IsNullOrWhiteSpace(mech?.pilotDisplayName))
            {
                return mech.pilotDisplayName;
            }

            return IsWarehouseMech(mech) ? "No pilot assigned" : "Mission pilot";
        }

        private static bool DeployableForMission(MechBayOwnedMechDefinition mech)
        {
            return mech != null && mech.availableForMission;
        }

        private static string DeploymentStatus(MechBayOwnedMechDefinition mech)
        {
            if (mech != null && mech.availableForMission)
            {
                return "Deployable now";
            }

            if (!IsWarehouseMech(mech))
            {
                return "Unavailable";
            }

            if (IsPendingDepotFit(mech))
            {
                return "Held: needs depot fit";
            }

            if (string.Equals(mech?.activeLoadoutId, MechBayWarehouseDraftFitPreviewService.DemoWarehouseFitLoadoutId, StringComparison.OrdinalIgnoreCase))
            {
                return "Held: future squad selection";
            }

            return string.IsNullOrWhiteSpace(mech?.activeLoadoutId) ? "Held: no loadout" : "Held: depot mech";
        }

        private static string DeploymentRequirements(MechBayOwnedMechDefinition mech, bool hasSpareWeaponStock)
        {
            if (mech != null && mech.availableForMission)
            {
                return "Current mission squad";
            }

            if (!IsWarehouseMech(mech))
            {
                return "Repair or mission reset";
            }

            if (IsPendingDepotFit(mech))
            {
                return DraftFitRequirements(mech, hasSpareWeaponStock);
            }

            if (string.Equals(mech?.activeLoadoutId, MechBayWarehouseDraftFitPreviewService.DemoWarehouseFitLoadoutId, StringComparison.OrdinalIgnoreCase))
            {
                return "Future squad-selection flow";
            }

            return "Future depot deployment flow";
        }

        private static bool HasSquadSelectionStub(MechBayOwnedMechDefinition mech)
        {
            return IsWarehouseMech(mech);
        }

        private static bool SquadSelectionCandidate(MechBayOwnedMechDefinition mech)
        {
            return IsWarehouseMech(mech)
                && mech != null
                && !mech.availableForMission
                && HasPilotAssignment(mech)
                && string.Equals(
                    mech?.activeLoadoutId,
                    MechBayWarehouseDraftFitPreviewService.DemoWarehouseFitLoadoutId,
                    StringComparison.OrdinalIgnoreCase);
        }

        private static string SquadSelectionStatus(MechBayOwnedMechDefinition mech)
        {
            if (mech != null && mech.availableForMission)
            {
                return "Already in mission squad";
            }

            if (!IsWarehouseMech(mech))
            {
                return "Mission squad unavailable";
            }

            if (IsPendingDepotFit(mech))
            {
                return "Locked: needs depot fit";
            }

            if (SquadSelectionCandidate(mech))
            {
                return "Ready for future squad selection";
            }

            return "Depot selection locked";
        }

        private static string SquadSelectionRequirements(MechBayOwnedMechDefinition mech, bool hasSpareWeaponStock)
        {
            if (mech != null && mech.availableForMission)
            {
                return "Current mission slot";
            }

            if (!IsWarehouseMech(mech))
            {
                return "Repair or mission reset";
            }

            if (IsPendingDepotFit(mech))
            {
                return DraftFitRequirements(mech, hasSpareWeaponStock);
            }

            return SquadSelectionCandidate(mech) ? "Future squad-selection screen" : "Future deployment flow";
        }

        private static string WeaponStockStatus(int spareWeaponStockCount, int totalWeaponStockCount)
        {
            if (spareWeaponStockCount > 0)
            {
                return spareWeaponStockCount.ToString(CultureInfo.InvariantCulture)
                    + " spare / "
                    + totalWeaponStockCount.ToString(CultureInfo.InvariantCulture)
                    + " weapon stock";
            }

            if (totalWeaponStockCount > 0)
            {
                return "0 spare / " + totalWeaponStockCount.ToString(CultureInfo.InvariantCulture) + " weapon stock";
            }

            return "No weapon stock";
        }

        private static bool IsPendingDepotFit(MechBayOwnedMechDefinition mech)
        {
            return IsWarehouseMech(mech)
                && string.Equals(mech?.activeLoadoutId, "pending-loadout", StringComparison.OrdinalIgnoreCase);
        }

        private static int CountAvailableItems(MechBayInventoryContract inventory, string category)
        {
            int count = 0;
            MechBayItemStackDefinition[] itemStacks = inventory?.itemStacks ?? Array.Empty<MechBayItemStackDefinition>();
            for (int index = 0; index < itemStacks.Length; index++)
            {
                MechBayItemStackDefinition stack = itemStacks[index];
                if (stack != null && string.Equals(stack.category, category, StringComparison.Ordinal))
                {
                    count += Math.Max(0, stack.quantity - stack.equippedQuantity);
                }
            }

            return count;
        }

        private static int CountTotalItems(MechBayInventoryContract inventory, string category)
        {
            int count = 0;
            MechBayItemStackDefinition[] itemStacks = inventory?.itemStacks ?? Array.Empty<MechBayItemStackDefinition>();
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

        private static bool StartsWith(string value, string prefix)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static class MechBaySquadSelectionPreviewService
    {
        public static MechBaySquadSelectionPreview BuildPreview(MechBayInventoryContract inventory)
        {
            MechBayOwnedRosterEntry[] roster = MechBayOwnedRosterService.BuildRosterPreview(inventory);
            List<MechBaySquadSelectionSlot> missionSlots = new();
            List<MechBaySquadSelectionSlot> depotCandidates = new();
            bool hasRefreshedMissionSlot = false;
            for (int index = 0; index < roster.Length; index++)
            {
                MechBayOwnedRosterEntry entry = roster[index];
                if (entry == null)
                {
                    continue;
                }

                if (entry.availableForMission)
                {
                    hasRefreshedMissionSlot = hasRefreshedMissionSlot || entry.isWarehouseMech;
                    missionSlots.Add(SlotFromRoster(entry));
                }
                else if (entry.isWarehouseMech)
                {
                    if (entry.squadSelectionCandidate)
                    {
                        depotCandidates.Add(SlotFromRoster(entry));
                    }
                }
            }

            MechBaySquadSelectionSlot dryRunOutgoing = missionSlots.Count > 0 ? missionSlots[0] : null;
            MechBaySquadSelectionSlot dryRunIncoming = depotCandidates.Count > 0 ? depotCandidates[0] : null;
            bool pendingSwapAvailable = dryRunOutgoing != null && dryRunIncoming != null;
            return new MechBaySquadSelectionPreview
            {
                InventoryChanged = false,
                Status = PreviewStatus(depotCandidates.Count, hasRefreshedMissionSlot),
                PreviewNote = PreviewNote(hasRefreshedMissionSlot),
                HasRefreshedMissionSlot = hasRefreshedMissionSlot,
                MissionSlotCount = missionSlots.Count,
                CandidateCount = depotCandidates.Count,
                SwapEnabled = false,
                SwapStatus = SwapStatus(missionSlots.Count, depotCandidates.Count, hasRefreshedMissionSlot),
                SwapRequirements = SwapRequirements(missionSlots.Count, depotCandidates.Count, hasRefreshedMissionSlot),
                DryRunAvailable = dryRunOutgoing != null && dryRunIncoming != null,
                DryRunStatus = DryRunStatus(dryRunOutgoing, dryRunIncoming, hasRefreshedMissionSlot),
                DryRunSummary = DryRunSummary(dryRunOutgoing, dryRunIncoming, hasRefreshedMissionSlot),
                DryRunOutgoingOwnedMechId = dryRunOutgoing?.ownedMechId,
                DryRunIncomingOwnedMechId = dryRunIncoming?.ownedMechId,
                PendingSwapAvailable = pendingSwapAvailable,
                PendingSwapStatus = PendingSwapStatus(dryRunOutgoing, dryRunIncoming),
                PendingSwapSummary = PendingSwapSummary(dryRunOutgoing, dryRunIncoming, hasRefreshedMissionSlot),
                MissionSlots = missionSlots.ToArray(),
                DepotCandidates = depotCandidates.ToArray()
            };
        }

        public static MechBaySquadSelectionDraftState BuildDraftState(
            MechBayInventoryContract inventory,
            string outgoingOwnedMechId,
            string incomingOwnedMechId)
        {
            MechBaySquadSelectionPreview preview = BuildPreview(inventory);
            MechBaySquadSelectionSlot outgoing = SelectSlot(preview.MissionSlots, outgoingOwnedMechId);
            MechBaySquadSelectionSlot incoming = SelectSlot(preview.DepotCandidates, incomingOwnedMechId);
            bool ready = outgoing != null && incoming != null;
            return new MechBaySquadSelectionDraftState
            {
                InventoryChanged = false,
                Ready = ready,
                Status = ready ? "Draft swap staged" : "Draft swap unavailable",
                Requirements = ready
                    ? "Confirm staged roster swap"
                    : DraftUnavailableSummary(preview, outgoing, incoming),
                Summary = ready
                    ? "Draft " + SlotName(incoming) + " over " + SlotName(outgoing)
                    : DraftUnavailableSummary(preview, outgoing, incoming),
                OutgoingOwnedMechId = outgoing?.ownedMechId,
                IncomingOwnedMechId = incoming?.ownedMechId,
                OutgoingDisplayName = SlotName(outgoing),
                IncomingDisplayName = SlotName(incoming)
            };
        }

        public static MechBaySquadSelectionApplyResult TryApplyPendingSwap(MechBayInventoryContract inventory)
        {
            return TryApplyPendingSwap(inventory, null, null);
        }

        public static MechBaySquadSelectionApplyResult TryApplyPendingSwap(
            MechBayInventoryContract inventory,
            MechBaySquadSelectionDraftState draft)
        {
            return TryApplyPendingSwap(inventory, draft?.OutgoingOwnedMechId, draft?.IncomingOwnedMechId);
        }

        public static MechBaySquadSelectionApplyResult TryApplyPendingSwap(
            MechBayInventoryContract inventory,
            string outgoingOwnedMechId,
            string incomingOwnedMechId)
        {
            MechBaySquadSelectionPreview preview = BuildPreview(inventory);
            MechBaySquadSelectionDraftState draft =
                BuildDraftState(inventory, outgoingOwnedMechId, incomingOwnedMechId);
            MechBaySquadSelectionApplyResult result = new()
            {
                Accepted = false,
                InventoryChanged = false,
                Summary = string.IsNullOrWhiteSpace(draft?.Summary)
                    ? "No pending swap"
                    : draft.Summary,
                OutgoingOwnedMechId = draft?.OutgoingOwnedMechId,
                IncomingOwnedMechId = draft?.IncomingOwnedMechId
            };

            if (preview == null || draft == null || !draft.Ready)
            {
                result.Message = "No pending squad swap";
                result.Reason = string.IsNullOrWhiteSpace(draft?.Requirements)
                    ? "Need mission slot + fitted depot candidate"
                    : draft.Requirements;
                return result;
            }

            if (inventory == null)
            {
                result.Message = "Inventory missing";
                result.Reason = "Inventory unavailable";
                return result;
            }

            MechBayOwnedMechDefinition outgoing = FindOwnedMech(inventory, draft.OutgoingOwnedMechId);
            MechBayOwnedMechDefinition incoming = FindOwnedMech(inventory, draft.IncomingOwnedMechId);
            if (outgoing == null || incoming == null)
            {
                result.Message = "Squad swap unavailable";
                result.Reason = "Selected mech missing";
                return result;
            }

            outgoing.availableForMission = false;
            incoming.availableForMission = true;
            result.Accepted = true;
            result.InventoryChanged = true;
            result.Message = "Applied squad swap";
            result.Reason = "Roster availability swapped";
            return result;
        }

        private static MechBayOwnedMechDefinition FindOwnedMech(MechBayInventoryContract inventory, string ownedMechId)
        {
            if (string.IsNullOrWhiteSpace(ownedMechId))
            {
                return null;
            }

            MechBayOwnedMechDefinition[] ownedMechs = inventory?.ownedMechs ?? Array.Empty<MechBayOwnedMechDefinition>();
            for (int index = 0; index < ownedMechs.Length; index++)
            {
                MechBayOwnedMechDefinition mech = ownedMechs[index];
                if (mech != null && string.Equals(mech.ownedMechId, ownedMechId, StringComparison.OrdinalIgnoreCase))
                {
                    return mech;
                }
            }

            return null;
        }

        private static MechBaySquadSelectionSlot SlotFromRoster(MechBayOwnedRosterEntry entry)
        {
            return new MechBaySquadSelectionSlot
            {
                ownedMechId = entry.ownedMechId,
                unitType = entry.unitType,
                chassisId = entry.chassisId,
                displayName = string.IsNullOrWhiteSpace(entry.displayName) ? entry.unitType : entry.displayName,
                activeLoadoutId = entry.activeLoadoutId,
                pilotDisplayName = entry.pilotDisplayName,
                conditionPercent = entry.conditionPercent,
                selectionStatus = entry.squadSelectionStatus,
                isDepotMissionSlot = entry.isWarehouseMech
            };
        }

        private static string PreviewStatus(int candidateCount, bool hasRefreshedMissionSlot)
        {
            if (candidateCount > 0)
            {
                return "Read-only squad selection preview";
            }

            return hasRefreshedMissionSlot ? "Mission squad refreshed" : "No depot candidates ready";
        }

        private static string PreviewNote(bool hasRefreshedMissionSlot)
        {
            return hasRefreshedMissionSlot
                ? "Confirmed swap applied: mission squad updated"
                : "Preview only: mission squad unchanged";
        }

        private static string SwapStatus(int missionSlotCount, int candidateCount, bool hasRefreshedMissionSlot)
        {
            if (hasRefreshedMissionSlot && candidateCount <= 0)
            {
                return "Swap complete";
            }

            return missionSlotCount > 0 && candidateCount > 0 ? "Swap guarded: use Confirm" : "Swap unavailable";
        }

        private static string SwapRequirements(int missionSlotCount, int candidateCount, bool hasRefreshedMissionSlot)
        {
            if (hasRefreshedMissionSlot && candidateCount <= 0)
            {
                return "No fitted depot candidates remain";
            }

            if (missionSlotCount <= 0 && candidateCount <= 0)
            {
                return "Need mission slot + fitted depot candidate";
            }

            if (missionSlotCount <= 0)
            {
                return "Need mission slot";
            }

            return candidateCount <= 0 ? "Need fitted depot candidate" : "Confirm staged roster swap";
        }

        private static string DryRunStatus(
            MechBaySquadSelectionSlot outgoing,
            MechBaySquadSelectionSlot incoming,
            bool hasRefreshedMissionSlot)
        {
            if (hasRefreshedMissionSlot && incoming == null)
            {
                return "Dry run cleared";
            }

            return outgoing != null && incoming != null ? "Dry run ready" : "Dry run unavailable";
        }

        private static string DryRunSummary(
            MechBaySquadSelectionSlot outgoing,
            MechBaySquadSelectionSlot incoming,
            bool hasRefreshedMissionSlot)
        {
            if (hasRefreshedMissionSlot && incoming == null)
            {
                return "Confirmed swap already applied";
            }

            if (outgoing == null && incoming == null)
            {
                return "Need mission slot + fitted depot candidate";
            }

            if (outgoing == null)
            {
                return "Need mission slot";
            }

            if (incoming == null)
            {
                return "Need fitted depot candidate";
            }

            return "Replace " + SlotName(outgoing) + " with " + SlotName(incoming) + " when confirmed";
        }

        private static string DraftUnavailableSummary(
            MechBaySquadSelectionPreview preview,
            MechBaySquadSelectionSlot outgoing,
            MechBaySquadSelectionSlot incoming)
        {
            return DryRunSummary(outgoing, incoming, preview?.HasRefreshedMissionSlot == true);
        }

        private static string SlotName(MechBaySquadSelectionSlot slot)
        {
            return string.IsNullOrWhiteSpace(slot?.displayName) ? slot?.unitType ?? "mech" : slot.displayName;
        }

        private static MechBaySquadSelectionSlot SelectSlot(MechBaySquadSelectionSlot[] slots, string ownedMechId)
        {
            MechBaySquadSelectionSlot[] safeSlots = slots ?? Array.Empty<MechBaySquadSelectionSlot>();
            if (!string.IsNullOrWhiteSpace(ownedMechId))
            {
                for (int index = 0; index < safeSlots.Length; index++)
                {
                    MechBaySquadSelectionSlot slot = safeSlots[index];
                    if (slot != null
                        && string.Equals(slot.ownedMechId, ownedMechId, StringComparison.OrdinalIgnoreCase))
                    {
                        return slot;
                    }
                }
            }

            return safeSlots.Length > 0 ? safeSlots[0] : null;
        }

        private static string PendingSwapStatus(MechBaySquadSelectionSlot outgoing, MechBaySquadSelectionSlot incoming)
        {
            return outgoing != null && incoming != null ? "Pending confirmation" : "No pending swap";
        }

        private static string PendingSwapSummary(
            MechBaySquadSelectionSlot outgoing,
            MechBaySquadSelectionSlot incoming,
            bool hasRefreshedMissionSlot)
        {
            if (outgoing == null || incoming == null)
            {
                return DryRunSummary(outgoing, incoming, hasRefreshedMissionSlot);
            }

            return "Stage " + SlotName(incoming) + " over " + SlotName(outgoing) + " for confirmation";
        }
    }

    public static class MechBayMissionHandoffPreviewService
    {
        private const string DemoMissionTemplateId = "mc2_01";
        private const string DemoMissionContractSchema = "mc2-unity-demo-contract-v1";
        private const string DemoRestartPatchMode = "Replace player unit spawns";
        private const string DemoPlayerBrain = "PBrain";
        private const int DemoPlayerTeamId = 0;
        private const int DemoCommanderId = 0;

        public static MechBayMissionHandoffPreview BuildPreview(MechBayInventoryContract inventory)
        {
            MechBayOwnedRosterEntry[] roster = MechBayOwnedRosterService.BuildRosterPreview(inventory);
            List<MechBaySquadSelectionSlot> missionSlots = new();
            bool includesDepotMissionSlot = false;
            for (int index = 0; index < roster.Length; index++)
            {
                MechBayOwnedRosterEntry entry = roster[index];
                if (entry == null || !entry.availableForMission)
                {
                    continue;
                }

                includesDepotMissionSlot = includesDepotMissionSlot || entry.isWarehouseMech;
                missionSlots.Add(SlotFromRoster(entry));
            }

            bool ready = missionSlots.Count > 0;
            return new MechBayMissionHandoffPreview
            {
                InventoryChanged = false,
                ReadyForFutureLaunch = ready,
                LaunchEnabled = false,
                IncludesDepotMissionSlot = includesDepotMissionSlot,
                MissionSlotCount = missionSlots.Count,
                Status = ready ? "Future mission roster ready" : "No mission roster ready",
                LaunchStatus = LaunchStatus(ready, includesDepotMissionSlot),
                LaunchRequirements = ready ? "Future mission launch hook" : "Need available mission slot",
                Summary = Summary(missionSlots, includesDepotMissionSlot),
                PreviewNote = "Preview only: current battle state unchanged",
                MissionSlots = missionSlots.ToArray()
            };
        }

        public static MechBayMissionHandoffLaunchGuard BuildLaunchGuard(MechBayInventoryContract inventory)
        {
            MechBayMissionHandoffPreview preview = BuildPreview(inventory);
            bool ready = preview?.ReadyForFutureLaunch == true;
            return new MechBayMissionHandoffLaunchGuard
            {
                Accepted = false,
                InventoryChanged = false,
                LaunchEnabled = false,
                IncludesDepotMissionSlot = preview?.IncludesDepotMissionSlot == true,
                MissionSlotCount = preview?.MissionSlotCount ?? 0,
                Message = ready ? "Mission launch guarded" : "Mission launch unavailable",
                Reason = ready ? "Future mission restart hook not wired" : "Need available mission slot",
                Summary = string.IsNullOrWhiteSpace(preview?.Summary)
                    ? "No available mission slots"
                    : preview.Summary
            };
        }

        public static MechBayMissionRestartDryRun BuildRestartDryRun(MechBayInventoryContract inventory)
        {
            MechBayMissionHandoffPreview preview = BuildPreview(inventory);
            MechBaySquadSelectionSlot[] slots = preview?.MissionSlots ?? Array.Empty<MechBaySquadSelectionSlot>();
            MechBayMissionRestartSpawnIntent[] intents = new MechBayMissionRestartSpawnIntent[slots.Length];
            for (int index = 0; index < slots.Length; index++)
            {
                MechBaySquadSelectionSlot slot = slots[index];
                intents[index] = new MechBayMissionRestartSpawnIntent
                {
                    spawnIndex = index + 1,
                    spawnRole = index == 0 ? "Commander" : "Lancemate",
                    ownedMechId = slot?.ownedMechId,
                    unitType = slot?.unitType,
                    chassisId = slot?.chassisId,
                    displayName = string.IsNullOrWhiteSpace(slot?.displayName) ? slot?.unitType : slot.displayName,
                    activeLoadoutId = slot?.activeLoadoutId,
                    pilotDisplayName = slot?.pilotDisplayName,
                    isDepotMissionSlot = slot?.isDepotMissionSlot == true
                };
            }

            bool ready = preview?.ReadyForFutureLaunch == true && intents.Length > 0;
            return new MechBayMissionRestartDryRun
            {
                InventoryChanged = false,
                Ready = ready,
                CreatesMissionInstance = false,
                IncludesDepotMissionSlot = preview?.IncludesDepotMissionSlot == true,
                MissionSlotCount = preview?.MissionSlotCount ?? 0,
                SpawnIntentCount = intents.Length,
                Status = ready ? "Restart dry run ready" : "Restart dry run unavailable",
                Summary = RestartSummary(intents, preview?.IncludesDepotMissionSlot == true),
                PreviewNote = "Dry run only: no BattleMission created",
                SpawnIntents = intents
            };
        }

        public static MechBayMissionRestartApplyGuard BuildRestartApplyGuard(MechBayInventoryContract inventory)
        {
            MechBayMissionRestartDryRun dryRun = BuildRestartDryRun(inventory);
            bool ready = dryRun?.Ready == true;
            return new MechBayMissionRestartApplyGuard
            {
                Accepted = false,
                InventoryChanged = false,
                ApplyEnabled = false,
                CreatesMissionInstance = false,
                IncludesDepotMissionSlot = dryRun?.IncludesDepotMissionSlot == true,
                SpawnIntentCount = dryRun?.SpawnIntentCount ?? 0,
                Message = ready ? "Restart apply guarded" : "Restart apply unavailable",
                Reason = ready ? "Future BattleMission recreation hook not wired" : "Need restart dry run",
                Summary = string.IsNullOrWhiteSpace(dryRun?.Summary) ? "No spawn intents" : dryRun.Summary
            };
        }

        public static MechBayMissionRestartContractPreview BuildRestartContractPreview(MechBayInventoryContract inventory)
        {
            MechBayMissionRestartDryRun dryRun = BuildRestartDryRun(inventory);
            MechBayMissionRestartSpawnIntent[] intents = dryRun?.SpawnIntents ?? Array.Empty<MechBayMissionRestartSpawnIntent>();
            MechBayMissionRestartSpawnIntent commander = FirstRestartIntent(intents);
            bool ready = dryRun?.Ready == true && commander != null;
            return new MechBayMissionRestartContractPreview
            {
                InventoryChanged = false,
                Ready = ready,
                CreatesMissionInstance = false,
                IncludesDepotMissionSlot = dryRun?.IncludesDepotMissionSlot == true,
                MissionSlotCount = dryRun?.MissionSlotCount ?? 0,
                SpawnIntentCount = dryRun?.SpawnIntentCount ?? 0,
                PlayerTeamId = DemoPlayerTeamId,
                CommanderId = DemoCommanderId,
                MissionTemplateId = DemoMissionTemplateId,
                ContractSchema = DemoMissionContractSchema,
                PatchMode = DemoRestartPatchMode,
                CommanderOwnedMechId = commander?.ownedMechId,
                CommanderDisplayName = string.IsNullOrWhiteSpace(commander?.displayName) ? commander?.unitType : commander.displayName,
                UnitBrain = DemoPlayerBrain,
                Status = ready ? "Restart contract preview ready" : "Restart contract preview unavailable",
                Requirements = ready ? "BattleMission recreation hook" : "Need restart dry run",
                Summary = RestartContractSummary(intents, dryRun?.IncludesDepotMissionSlot == true),
                PreviewNote = "Preview only: contract not instantiated",
                SpawnIntents = intents
            };
        }

        private static MechBaySquadSelectionSlot SlotFromRoster(MechBayOwnedRosterEntry entry)
        {
            return new MechBaySquadSelectionSlot
            {
                ownedMechId = entry.ownedMechId,
                unitType = entry.unitType,
                chassisId = entry.chassisId,
                displayName = string.IsNullOrWhiteSpace(entry.displayName) ? entry.unitType : entry.displayName,
                activeLoadoutId = entry.activeLoadoutId,
                pilotDisplayName = entry.pilotDisplayName,
                conditionPercent = entry.conditionPercent,
                selectionStatus = entry.deploymentStatus,
                isDepotMissionSlot = entry.isWarehouseMech
            };
        }

        private static string LaunchStatus(bool ready, bool includesDepotMissionSlot)
        {
            if (!ready)
            {
                return "Launch unavailable";
            }

            return includesDepotMissionSlot ? "Launch preview includes depot mech" : "Launch preview uses current squad";
        }

        private static string RestartSummary(MechBayMissionRestartSpawnIntent[] intents, bool includesDepotMissionSlot)
        {
            if (intents == null || intents.Length == 0)
            {
                return "No spawn intents";
            }

            string text = "Spawn intents: ";
            int count = Math.Min(3, intents.Length);
            for (int index = 0; index < count; index++)
            {
                if (index > 0)
                {
                    text += "; ";
                }

                MechBayMissionRestartSpawnIntent intent = intents[index];
                string name = string.IsNullOrWhiteSpace(intent?.displayName) ? intent?.unitType ?? "Mech" : intent.displayName;
                string role = string.IsNullOrWhiteSpace(intent?.spawnRole) ? "Lancemate" : intent.spawnRole;
                text += intent.spawnIndex.ToString(CultureInfo.InvariantCulture) + " " + name + " " + role;
            }

            if (intents.Length > count)
            {
                text += " +" + (intents.Length - count).ToString(CultureInfo.InvariantCulture);
            }

            return includesDepotMissionSlot ? text + "  depot included" : text;
        }

        private static MechBayMissionRestartSpawnIntent FirstRestartIntent(MechBayMissionRestartSpawnIntent[] intents)
        {
            if (intents == null)
            {
                return null;
            }

            for (int index = 0; index < intents.Length; index++)
            {
                if (intents[index] != null)
                {
                    return intents[index];
                }
            }

            return null;
        }

        private static string RestartContractSummary(
            MechBayMissionRestartSpawnIntent[] intents,
            bool includesDepotMissionSlot)
        {
            if (intents == null || intents.Length == 0)
            {
                return "No BattleMission input";
            }

            MechBayMissionRestartSpawnIntent commander = FirstRestartIntent(intents);
            string commanderName = string.IsNullOrWhiteSpace(commander?.displayName)
                ? commander?.unitType ?? "Mech"
                : commander.displayName;
            string text = "BattleMission input: "
                + DemoMissionTemplateId
                + " team "
                + DemoPlayerTeamId.ToString(CultureInfo.InvariantCulture)
                + " spawns "
                + intents.Length.ToString(CultureInfo.InvariantCulture)
                + " commander "
                + commanderName;
            return includesDepotMissionSlot ? text + "  depot included" : text;
        }

        private static string Summary(List<MechBaySquadSelectionSlot> missionSlots, bool includesDepotMissionSlot)
        {
            if (missionSlots == null || missionSlots.Count == 0)
            {
                return "No available mission slots";
            }

            string text = "Next mission roster: ";
            int count = Math.Min(3, missionSlots.Count);
            for (int index = 0; index < count; index++)
            {
                if (index > 0)
                {
                    text += "; ";
                }

                MechBaySquadSelectionSlot slot = missionSlots[index];
                string name = string.IsNullOrWhiteSpace(slot?.displayName) ? slot?.unitType ?? "Mech" : slot.displayName;
                text += name;
            }

            if (missionSlots.Count > count)
            {
                text += " +" + (missionSlots.Count - count).ToString(CultureInfo.InvariantCulture);
            }

            return includesDepotMissionSlot ? text + "  depot included" : text;
        }
    }

    public static class MechBayWeaponShopPreviewService
    {
        private static readonly MechBayWeaponShopEntry[] StarterCatalog =
        {
            CatalogEntry("shop-basic-laser", "Ordinary Laser", "Energy", 900),
            CatalogEntry("shop-srm-rack", "Ordinary SRM Rack", "Missile", 1250),
            CatalogEntry("shop-autocannon", "Ordinary Autocannon", "Ballistic", 1600)
        };

        public static MechBayWeaponShopPreview BuildPreview(MechBayInventoryContract inventory)
        {
            int tokenBalance = Math.Max(0, inventory?.tokenBalance ?? 0);
            MechBayWeaponShopEntry[] entries = new MechBayWeaponShopEntry[StarterCatalog.Length];
            for (int index = 0; index < StarterCatalog.Length; index++)
            {
                MechBayWeaponShopEntry source = StarterCatalog[index];
                entries[index] = new MechBayWeaponShopEntry
                {
                    itemId = source.itemId,
                    displayName = source.displayName,
                    weaponFamily = source.weaponFamily,
                    tokenCost = source.tokenCost,
                    canAfford = tokenBalance >= source.tokenCost,
                    purchaseEnabled = tokenBalance >= source.tokenCost,
                    purchaseStatus = tokenBalance >= source.tokenCost ? "Demo purchase" : "Need token"
                };
            }

            return new MechBayWeaponShopPreview
            {
                TokenBalance = tokenBalance,
                Status = "Ordinary weapon shop preview",
                Entries = entries
            };
        }

        public static MechBayWeaponPurchasePreviewResult PreviewPurchase(
            MechBayInventoryContract inventory,
            string itemId)
        {
            int tokenBalance = Math.Max(0, inventory?.tokenBalance ?? 0);
            MechBayWeaponShopEntry entry = FindCatalogEntry(itemId);
            if (entry == null)
            {
                return new MechBayWeaponPurchasePreviewResult
                {
                    Accepted = false,
                    itemId = itemId,
                    TokenBalance = tokenBalance,
                    CanAfford = false,
                    InventoryChanged = false,
                    Message = "Weapon unavailable"
                };
            }

            return new MechBayWeaponPurchasePreviewResult
            {
                Accepted = false,
                itemId = entry.itemId,
                displayName = entry.displayName,
                TokenCost = entry.tokenCost,
                TokenBalance = tokenBalance,
                CanAfford = tokenBalance >= entry.tokenCost,
                InventoryChanged = false,
                Message = tokenBalance >= entry.tokenCost ? "Ready demo purchase" : "Need token"
            };
        }

        public static MechBayWeaponPurchasePreviewResult TryApplyDemoPurchase(
            MechBayInventoryContract inventory,
            string itemId)
        {
            MechBayWeaponPurchasePreviewResult preview = PreviewPurchase(inventory, itemId);
            if (preview == null)
            {
                return preview;
            }

            if (inventory == null)
            {
                preview.Message = "Inventory missing";
                return preview;
            }

            MechBayWeaponShopEntry entry = FindCatalogEntry(itemId);
            if (entry == null)
            {
                return preview;
            }

            if (inventory.tokenBalance < entry.tokenCost)
            {
                preview.Message = "Need " + entry.tokenCost.ToString(CultureInfo.InvariantCulture) + " token";
                return preview;
            }

            inventory.tokenBalance -= entry.tokenCost;
            AddInventoryStack(inventory, entry.itemId, entry.displayName, LoadoutItemCategory.Weapon, 1);
            preview.Accepted = true;
            preview.TokenBalance = inventory.tokenBalance;
            preview.InventoryChanged = true;
            preview.Message = "Purchased " + entry.displayName;
            return preview;
        }

        private static MechBayWeaponShopEntry CatalogEntry(
            string itemId,
            string displayName,
            string weaponFamily,
            int tokenCost)
        {
            return new MechBayWeaponShopEntry
            {
                itemId = itemId,
                displayName = displayName,
                weaponFamily = weaponFamily,
                tokenCost = Math.Max(0, tokenCost),
                purchaseEnabled = false,
                purchaseStatus = "Preview only"
            };
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

        private static MechBayWeaponShopEntry FindCatalogEntry(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            for (int index = 0; index < StarterCatalog.Length; index++)
            {
                MechBayWeaponShopEntry entry = StarterCatalog[index];
                if (entry != null && string.Equals(entry.itemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }

            return null;
        }
    }

    public static class MechBayPilotHirePreviewService
    {
        private static readonly MechBayPilotHireCandidate[] StarterCandidates =
        {
            Candidate("pilot-npc-recruit", "NPC Recruit", "NPC", 600, "NPC death risk"),
            Candidate("pilot-npc-gunner", "NPC Gunner", "NPC", 950, "NPC death risk")
        };

        public static MechBayPilotHirePreview BuildPreview(MechBayInventoryContract inventory)
        {
            int tokenBalance = Math.Max(0, inventory?.tokenBalance ?? 0);
            MechBayPilotHireCandidate[] candidates = new MechBayPilotHireCandidate[StarterCandidates.Length];
            for (int index = 0; index < StarterCandidates.Length; index++)
            {
                MechBayPilotHireCandidate source = StarterCandidates[index];
                candidates[index] = new MechBayPilotHireCandidate
                {
                    pilotId = source.pilotId,
                    displayName = source.displayName,
                    pilotType = source.pilotType,
                    hireCost = source.hireCost,
                    canAfford = tokenBalance >= source.hireCost,
                    hireEnabled = tokenBalance >= source.hireCost,
                    hireStatus = tokenBalance >= source.hireCost ? "Demo hire" : "Need token",
                    riskProfile = source.riskProfile
                };
            }

            return new MechBayPilotHirePreview
            {
                TokenBalance = tokenBalance,
                Status = "NPC pilot hire preview",
                Candidates = candidates
            };
        }

        public static MechBayPilotHireResult PreviewHire(
            MechBayInventoryContract inventory,
            string ownedMechId,
            string pilotId)
        {
            int tokenBalance = Math.Max(0, inventory?.tokenBalance ?? 0);
            MechBayPilotHireCandidate candidate = FindCandidate(pilotId);
            if (candidate == null)
            {
                return new MechBayPilotHireResult
                {
                    Accepted = false,
                    ownedMechId = ownedMechId,
                    pilotId = pilotId,
                    TokenBalance = tokenBalance,
                    CanAfford = false,
                    InventoryChanged = false,
                    Message = "Pilot unavailable"
                };
            }

            MechBayOwnedMechDefinition target = FindOwnedMech(inventory, ownedMechId);
            if (target == null)
            {
                return new MechBayPilotHireResult
                {
                    Accepted = false,
                    ownedMechId = ownedMechId,
                    pilotId = candidate.pilotId,
                    displayName = candidate.displayName,
                    TokenCost = candidate.hireCost,
                    TokenBalance = tokenBalance,
                    CanAfford = tokenBalance >= candidate.hireCost,
                    InventoryChanged = false,
                    RiskProfile = candidate.riskProfile,
                    Message = "Mech unavailable"
                };
            }

            bool canAfford = tokenBalance >= candidate.hireCost;
            string message = HirePreviewMessage(target, canAfford, candidate.hireCost);
            return new MechBayPilotHireResult
            {
                Accepted = false,
                ownedMechId = target.ownedMechId,
                pilotId = candidate.pilotId,
                displayName = candidate.displayName,
                TokenCost = candidate.hireCost,
                TokenBalance = tokenBalance,
                CanAfford = canAfford,
                InventoryChanged = false,
                RiskProfile = candidate.riskProfile,
                Message = message
            };
        }

        public static MechBayPilotHireResult TryApplyDemoHire(
            MechBayInventoryContract inventory,
            string ownedMechId,
            string pilotId)
        {
            MechBayPilotHireResult preview = PreviewHire(inventory, ownedMechId, pilotId);
            if (preview == null)
            {
                return preview;
            }

            if (inventory == null)
            {
                preview.Message = "Inventory missing";
                return preview;
            }

            MechBayPilotHireCandidate candidate = FindCandidate(pilotId);
            MechBayOwnedMechDefinition target = FindOwnedMech(inventory, ownedMechId);
            if (candidate == null || target == null)
            {
                return preview;
            }

            if (!CanHirePilotFor(target))
            {
                preview.Message = "Pilot already assigned";
                return preview;
            }

            if (inventory.tokenBalance < candidate.hireCost)
            {
                preview.Message = "Need " + candidate.hireCost.ToString(CultureInfo.InvariantCulture) + " token";
                return preview;
            }

            inventory.tokenBalance -= candidate.hireCost;
            target.pilotId = candidate.pilotId;
            target.pilotDisplayName = candidate.displayName;
            target.pilotType = candidate.pilotType;
            preview.Accepted = true;
            preview.TokenBalance = inventory.tokenBalance;
            preview.InventoryChanged = true;
            preview.Message = "Hired " + candidate.displayName;
            return preview;
        }

        private static MechBayPilotHireCandidate Candidate(
            string pilotId,
            string displayName,
            string pilotType,
            int hireCost,
            string riskProfile)
        {
            return new MechBayPilotHireCandidate
            {
                pilotId = pilotId,
                displayName = displayName,
                pilotType = pilotType,
                hireCost = Math.Max(0, hireCost),
                hireEnabled = false,
                hireStatus = "Preview only",
                riskProfile = riskProfile
            };
        }

        private static string HirePreviewMessage(MechBayOwnedMechDefinition target, bool canAfford, int hireCost)
        {
            if (!CanHirePilotFor(target))
            {
                return "Pilot already assigned";
            }

            return canAfford
                ? "Ready demo hire"
                : "Need " + hireCost.ToString(CultureInfo.InvariantCulture) + " token";
        }

        private static bool CanHirePilotFor(MechBayOwnedMechDefinition target)
        {
            return IsWarehouseMech(target) && string.IsNullOrWhiteSpace(target?.pilotId);
        }

        private static MechBayOwnedMechDefinition FindOwnedMech(MechBayInventoryContract inventory, string ownedMechId)
        {
            if (string.IsNullOrWhiteSpace(ownedMechId))
            {
                return null;
            }

            MechBayOwnedMechDefinition[] ownedMechs = inventory?.ownedMechs ?? Array.Empty<MechBayOwnedMechDefinition>();
            for (int index = 0; index < ownedMechs.Length; index++)
            {
                MechBayOwnedMechDefinition mech = ownedMechs[index];
                if (mech != null && string.Equals(mech.ownedMechId, ownedMechId, StringComparison.OrdinalIgnoreCase))
                {
                    return mech;
                }
            }

            return null;
        }

        private static MechBayPilotHireCandidate FindCandidate(string pilotId)
        {
            if (string.IsNullOrWhiteSpace(pilotId))
            {
                return null;
            }

            for (int index = 0; index < StarterCandidates.Length; index++)
            {
                MechBayPilotHireCandidate candidate = StarterCandidates[index];
                if (candidate != null && string.Equals(candidate.pilotId, pilotId, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static bool IsWarehouseMech(MechBayOwnedMechDefinition mech)
        {
            return StartsWith(mech?.unitId, "warehouse-") || StartsWith(mech?.ownedMechId, "assembled-");
        }

        private static bool StartsWith(string value, string prefix)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static class MechBayWarehouseDraftFitPreviewService
    {
        public const string DemoWarehouseFitLoadoutId = "warehouse-demo-fit";

        public static MechBayWarehouseDraftFitPreview BuildPreview(
            MechBayInventoryContract inventory,
            string ownedMechId)
        {
            MechBayOwnedMechDefinition target = FindOwnedMech(inventory, ownedMechId);
            if (target == null)
            {
                return new MechBayWarehouseDraftFitPreview
                {
                    Ready = false,
                    InventoryChanged = false,
                    Status = "Draft fit preview unavailable",
                    Requirements = "Mech unavailable",
                    ownedMechId = ownedMechId,
                    PreviewNote = "Preview only"
                };
            }

            MechBayItemStackDefinition weapon = FirstSpareWeapon(inventory);
            bool hasPilot = !string.IsNullOrWhiteSpace(target.pilotId);
            bool ready = IsPendingDepotFit(target) && hasPilot && weapon != null;
            string requirements = DraftFitRequirements(target, hasPilot, weapon != null);
            return new MechBayWarehouseDraftFitPreview
            {
                Ready = ready,
                InventoryChanged = false,
                Status = ready ? "Read-only draft fit preview" : "Draft fit preview locked",
                Requirements = requirements,
                ownedMechId = target.ownedMechId,
                displayName = string.IsNullOrWhiteSpace(target.displayName) ? target.unitType : target.displayName,
                chassisId = target.chassisId,
                activeLoadoutId = target.activeLoadoutId,
                pilotDisplayName = hasPilot ? target.pilotDisplayName : "No pilot assigned",
                pilotStatus = hasPilot ? PilotStatus(target) : "Pilot required",
                weaponItemId = weapon?.itemId,
                weaponDisplayName = weapon?.displayName,
                spareWeaponStockCount = weapon == null ? 0 : Math.Max(0, weapon.quantity - weapon.equippedQuantity),
                PreviewNote = ready ? "Preview only: no inventory or loadout changes" : "Resolve requirements before fitting"
            };
        }

        public static MechBayWarehouseDraftFitApplyResult TryApplyDemoFit(
            MechBayInventoryContract inventory,
            string ownedMechId)
        {
            MechBayWarehouseDraftFitPreview preview = BuildPreview(inventory, ownedMechId);
            MechBayWarehouseDraftFitApplyResult result = new()
            {
                Accepted = false,
                InventoryChanged = false,
                ownedMechId = preview?.ownedMechId ?? ownedMechId,
                displayName = preview?.displayName,
                activeLoadoutId = preview?.activeLoadoutId,
                weaponItemId = preview?.weaponItemId,
                weaponDisplayName = preview?.weaponDisplayName,
                spareWeaponStockCount = preview?.spareWeaponStockCount ?? 0,
                Message = preview?.Requirements ?? "Draft fit unavailable"
            };

            if (inventory == null)
            {
                result.Message = "Inventory missing";
                return result;
            }

            if (preview == null || !preview.Ready)
            {
                result.Message = preview?.Requirements ?? "Draft fit unavailable";
                return result;
            }

            MechBayOwnedMechDefinition target = FindOwnedMech(inventory, preview.ownedMechId);
            MechBayItemStackDefinition weapon = FirstSpareWeapon(inventory);
            if (target == null || weapon == null)
            {
                result.Message = "Draft fit unavailable";
                return result;
            }

            weapon.equippedQuantity++;
            target.activeLoadoutId = DemoWarehouseFitLoadoutId;
            target.availableForMission = false;
            result.Accepted = true;
            result.InventoryChanged = true;
            result.activeLoadoutId = target.activeLoadoutId;
            result.spareWeaponStockCount = Math.Max(0, weapon.quantity - weapon.equippedQuantity);
            result.Message = "Applied warehouse draft fit";
            return result;
        }

        private static string DraftFitRequirements(MechBayOwnedMechDefinition target, bool hasPilot, bool hasSpareWeapon)
        {
            if (!IsPendingDepotFit(target))
            {
                return "No pending depot fit";
            }

            if (hasPilot && hasSpareWeapon)
            {
                return "Ready for future fitting";
            }

            if (hasSpareWeapon)
            {
                return "Need pilot";
            }

            return hasPilot ? "Need stock weapons" : "Need stock weapons + pilot";
        }

        private static MechBayOwnedMechDefinition FindOwnedMech(MechBayInventoryContract inventory, string ownedMechId)
        {
            if (string.IsNullOrWhiteSpace(ownedMechId))
            {
                return null;
            }

            MechBayOwnedMechDefinition[] ownedMechs = inventory?.ownedMechs ?? Array.Empty<MechBayOwnedMechDefinition>();
            for (int index = 0; index < ownedMechs.Length; index++)
            {
                MechBayOwnedMechDefinition mech = ownedMechs[index];
                if (mech != null && string.Equals(mech.ownedMechId, ownedMechId, StringComparison.OrdinalIgnoreCase))
                {
                    return mech;
                }
            }

            return null;
        }

        private static MechBayItemStackDefinition FirstSpareWeapon(MechBayInventoryContract inventory)
        {
            MechBayItemStackDefinition[] itemStacks = inventory?.itemStacks ?? Array.Empty<MechBayItemStackDefinition>();
            for (int index = 0; index < itemStacks.Length; index++)
            {
                MechBayItemStackDefinition stack = itemStacks[index];
                if (stack != null
                    && string.Equals(stack.category, LoadoutItemCategory.Weapon, StringComparison.Ordinal)
                    && stack.quantity - stack.equippedQuantity > 0)
                {
                    return stack;
                }
            }

            return null;
        }

        private static bool IsPendingDepotFit(MechBayOwnedMechDefinition mech)
        {
            return IsWarehouseMech(mech)
                && string.Equals(mech?.activeLoadoutId, "pending-loadout", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsWarehouseMech(MechBayOwnedMechDefinition mech)
        {
            return StartsWith(mech?.unitId, "warehouse-") || StartsWith(mech?.ownedMechId, "assembled-");
        }

        private static string PilotStatus(MechBayOwnedMechDefinition mech)
        {
            return string.Equals(mech?.pilotType, "NPC", StringComparison.OrdinalIgnoreCase) ? "Assigned NPC" : "Assigned";
        }

        private static bool StartsWith(string value, string prefix)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
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

            MechBayAssemblyResult assemblyResult = MechBayAssemblyPreviewService.AutoAssembleReadyFragments(inventory);
            receipt.AssembledMechCount = assemblyResult.AssembledMechCount;
            receipt.AssembledMechNames = assemblyResult.AssembledMechNames;
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
                AssembledMechNames = Array.Empty<string>(),
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

    public static class MechBayAssemblyPreviewService
    {
        public const int DemoRequiredFragmentsPerMech = 3;

        public static MechBayAssemblyProgress[] BuildAssemblyPreview(MechBayInventoryContract inventory)
        {
            List<MechBayAssemblyProgress> progress = new();
            MechBayItemStackDefinition[] itemStacks = inventory?.itemStacks ?? Array.Empty<MechBayItemStackDefinition>();
            for (int index = 0; index < itemStacks.Length; index++)
            {
                MechBayItemStackDefinition stack = itemStacks[index];
                if (stack == null || stack.category != LoadoutItemCategory.MechFragment || stack.quantity <= 0)
                {
                    continue;
                }

                string unitType = UnitTypeFromFragmentStack(stack);
                progress.Add(new MechBayAssemblyProgress
                {
                    unitType = unitType,
                    displayName = unitType,
                    fragments = Math.Max(0, stack.quantity),
                    requiredFragments = DemoRequiredFragmentsPerMech,
                    canAssemble = stack.quantity >= DemoRequiredFragmentsPerMech
                });
            }

            progress.Sort(CompareAssemblyProgress);
            return progress.ToArray();
        }

        public static MechBayAssemblyProgress BestAssemblyProgress(MechBayInventoryContract inventory)
        {
            MechBayAssemblyProgress[] progress = BuildAssemblyPreview(inventory);
            return progress.Length == 0 ? null : progress[0];
        }

        public static MechBayAssemblyResult AutoAssembleReadyFragments(MechBayInventoryContract inventory)
        {
            MechBayAssemblyResult result = new()
            {
                AssembledMechNames = Array.Empty<string>()
            };
            if (inventory == null)
            {
                return result;
            }

            List<MechBayItemStackDefinition> itemStacks = new(inventory.itemStacks ?? Array.Empty<MechBayItemStackDefinition>());
            List<MechBayOwnedMechDefinition> ownedMechs = new(inventory.ownedMechs ?? Array.Empty<MechBayOwnedMechDefinition>());
            List<string> assembledNames = new();
            for (int index = 0; index < itemStacks.Count; index++)
            {
                MechBayItemStackDefinition stack = itemStacks[index];
                if (stack == null
                    || stack.category != LoadoutItemCategory.MechFragment
                    || stack.quantity < DemoRequiredFragmentsPerMech)
                {
                    continue;
                }

                string unitType = UnitTypeFromFragmentStack(stack);
                int assemblyCount = stack.quantity / DemoRequiredFragmentsPerMech;
                stack.quantity -= assemblyCount * DemoRequiredFragmentsPerMech;
                for (int assemblyIndex = 0; assemblyIndex < assemblyCount; assemblyIndex++)
                {
                    string ownedMechId = NextOwnedMechId(ownedMechs, unitType);
                    ownedMechs.Add(new MechBayOwnedMechDefinition
                    {
                        ownedMechId = ownedMechId,
                        unitId = "warehouse-" + ownedMechId,
                        unitType = unitType,
                        chassisId = unitType,
                        displayName = unitType + " Assembled",
                        activeLoadoutId = "pending-loadout",
                        availableForMission = false,
                        conditionPercent = 100
                    });
                    assembledNames.Add(unitType);
                }
            }

            itemStacks.RemoveAll(IsEmptyFragmentStack);
            inventory.itemStacks = itemStacks.ToArray();
            inventory.ownedMechs = ownedMechs.ToArray();
            result.AssembledMechCount = assembledNames.Count;
            result.AssembledMechNames = assembledNames.ToArray();
            return result;
        }

        private static int CompareAssemblyProgress(MechBayAssemblyProgress left, MechBayAssemblyProgress right)
        {
            if (left == null || right == null)
            {
                return left == null && right == null ? 0 : left == null ? 1 : -1;
            }

            if (left.canAssemble != right.canAssemble)
            {
                return left.canAssemble ? -1 : 1;
            }

            int fragmentCompare = right.fragments.CompareTo(left.fragments);
            if (fragmentCompare != 0)
            {
                return fragmentCompare;
            }

            return string.Compare(left.displayName, right.displayName, StringComparison.OrdinalIgnoreCase);
        }

        private static string UnitTypeFromFragmentStack(MechBayItemStackDefinition stack)
        {
            string displayName = stack.displayName ?? "";
            const string Suffix = " Fragment";
            if (displayName.EndsWith(Suffix, StringComparison.OrdinalIgnoreCase))
            {
                return displayName.Substring(0, displayName.Length - Suffix.Length).Trim();
            }

            string itemId = stack.itemId ?? "";
            const string Prefix = "fragment-";
            if (itemId.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                return HumanizeItemId(itemId.Substring(Prefix.Length));
            }

            return string.IsNullOrWhiteSpace(displayName) ? "Unknown" : displayName.Trim();
        }

        private static bool IsEmptyFragmentStack(MechBayItemStackDefinition stack)
        {
            return stack != null
                && stack.category == LoadoutItemCategory.MechFragment
                && stack.quantity <= 0
                && stack.equippedQuantity <= 0;
        }

        private static string NextOwnedMechId(List<MechBayOwnedMechDefinition> ownedMechs, string unitType)
        {
            string stem = "assembled-" + SafeId(unitType);
            for (int index = 1; index < 1000; index++)
            {
                string candidate = stem + "-" + index.ToString("00");
                if (!HasOwnedMechId(ownedMechs, candidate))
                {
                    return candidate;
                }
            }

            return stem + "-overflow";
        }

        private static bool HasOwnedMechId(List<MechBayOwnedMechDefinition> ownedMechs, string ownedMechId)
        {
            for (int index = 0; index < ownedMechs.Count; index++)
            {
                MechBayOwnedMechDefinition mech = ownedMechs[index];
                if (mech != null && string.Equals(mech.ownedMechId, ownedMechId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string SafeId(string value)
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

            string id = new string(chars).Trim('-');
            return string.IsNullOrWhiteSpace(id) ? "unknown" : id;
        }

        private static string HumanizeItemId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Unknown";
            }

            string[] parts = value.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < parts.Length; index++)
            {
                string part = parts[index];
                if (part.Length > 1)
                {
                    parts[index] = char.ToUpperInvariant(part[0]) + part.Substring(1);
                }
                else
                {
                    parts[index] = part.ToUpperInvariant();
                }
            }

            return string.Join(" ", parts);
        }
    }
}
