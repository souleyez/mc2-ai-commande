using System;
using System.Collections.Generic;
using System.Globalization;

namespace MC2Demo.BattleCore
{
    [Serializable]
    public sealed class MechBaySavedAccountContract
    {
        public string schema;
        public string accountId;
        public string commanderDisplayName;
        public string accountType;
        public int saveRevision;
        public MechBayInventoryContract inventory;
        public MechBaySavedAccountCounters counters;
    }

    [Serializable]
    public sealed class MechBaySavedAccountCounters
    {
        public int tokenBalance;
        public int ownedMechCount;
        public int itemStackCount;
        public int readyMissionMechCount;
        public int warehouseMechCount;
    }

    public sealed class MechBaySavedAccountValidationResult
    {
        private readonly List<string> errors = new();

        public bool IsValid => errors.Count == 0;
        public string[] Errors => errors.ToArray();
        public MechBaySavedAccountCounters Counters { get; internal set; }
        public MechBayInventoryValidationResult InventoryValidation { get; internal set; }

        internal void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                errors.Add(error);
            }
        }
    }

    public static class MechBaySavedAccountService
    {
        public const string Schema = "mc2-demo-saved-account-v1";
        private const string DemoAccountId = "demo-local-commander";
        private const string DemoCommanderName = "Demo Commander";

        public static MechBaySavedAccountContract BuildDemoSnapshot(MechBayInventoryContract inventory)
        {
            MechBayInventoryContract snapshotInventory = CloneInventory(inventory);
            return new MechBaySavedAccountContract
            {
                schema = Schema,
                accountId = DemoAccountId,
                commanderDisplayName = DemoCommanderName,
                accountType = "local-demo",
                saveRevision = 1,
                inventory = snapshotInventory,
                counters = BuildCounters(snapshotInventory)
            };
        }

        public static MechBaySavedAccountValidationResult Validate(MechBaySavedAccountContract account)
        {
            MechBaySavedAccountValidationResult result = new();
            if (account == null)
            {
                result.AddError("Saved account missing");
                result.Counters = new MechBaySavedAccountCounters();
                return result;
            }

            if (!string.Equals(account.schema, Schema, StringComparison.Ordinal))
            {
                result.AddError("Saved account schema mismatch");
            }

            if (string.IsNullOrWhiteSpace(account.accountId))
            {
                result.AddError("Saved account id missing");
            }

            result.InventoryValidation = MechBayInventoryValidator.Validate(account.inventory);
            if (result.InventoryValidation == null || !result.InventoryValidation.IsValid)
            {
                result.AddError("Saved account inventory invalid");
            }

            result.Counters = BuildCounters(account.inventory);
            ValidateCounters(account.counters, result.Counters, result);
            return result;
        }

        public static string SummaryText(MechBaySavedAccountContract account)
        {
            MechBaySavedAccountValidationResult result = Validate(account);
            MechBaySavedAccountCounters counters = result.Counters ?? new MechBaySavedAccountCounters();
            string state = result.IsValid ? "Ready" : "Review";
            string commander = string.IsNullOrWhiteSpace(account?.commanderDisplayName)
                ? "Commander"
                : account.commanderDisplayName;
            return commander
                + "  "
                + state
                + "  "
                + counters.ownedMechCount.ToString(CultureInfo.InvariantCulture)
                + " mechs  "
                + counters.readyMissionMechCount.ToString(CultureInfo.InvariantCulture)
                + " ready";
        }

        private static void ValidateCounters(
            MechBaySavedAccountCounters expected,
            MechBaySavedAccountCounters actual,
            MechBaySavedAccountValidationResult result)
        {
            if (expected == null)
            {
                result.AddError("Saved account counters missing");
                return;
            }

            if (actual == null)
            {
                result.AddError("Saved account counter calculation unavailable");
                return;
            }

            if (expected.tokenBalance != actual.tokenBalance
                || expected.ownedMechCount != actual.ownedMechCount
                || expected.itemStackCount != actual.itemStackCount
                || expected.readyMissionMechCount != actual.readyMissionMechCount
                || expected.warehouseMechCount != actual.warehouseMechCount)
            {
                result.AddError("Saved account counters mismatch");
            }
        }

        private static MechBaySavedAccountCounters BuildCounters(MechBayInventoryContract inventory)
        {
            MechBayOwnedMechDefinition[] ownedMechs = inventory?.ownedMechs ?? Array.Empty<MechBayOwnedMechDefinition>();
            int readyMissionMechs = 0;
            int warehouseMechs = 0;
            for (int index = 0; index < ownedMechs.Length; index++)
            {
                MechBayOwnedMechDefinition mech = ownedMechs[index];
                if (mech == null)
                {
                    continue;
                }

                if (mech.availableForMission)
                {
                    readyMissionMechs++;
                }

                if (IsWarehouseMech(mech))
                {
                    warehouseMechs++;
                }
            }

            return new MechBaySavedAccountCounters
            {
                tokenBalance = Math.Max(0, inventory?.tokenBalance ?? 0),
                ownedMechCount = ownedMechs.Length,
                itemStackCount = (inventory?.itemStacks ?? Array.Empty<MechBayItemStackDefinition>()).Length,
                readyMissionMechCount = readyMissionMechs,
                warehouseMechCount = warehouseMechs
            };
        }

        private static bool IsWarehouseMech(MechBayOwnedMechDefinition mech)
        {
            return mech != null
                && (!mech.availableForMission
                    || string.IsNullOrWhiteSpace(mech.unitId)
                    || mech.unitId.StartsWith("warehouse-", StringComparison.OrdinalIgnoreCase));
        }

        private static MechBayInventoryContract CloneInventory(MechBayInventoryContract inventory)
        {
            return new MechBayInventoryContract
            {
                schema = inventory?.schema,
                tokenBalance = Math.Max(0, inventory?.tokenBalance ?? 0),
                ownedMechs = CloneOwnedMechs(inventory?.ownedMechs),
                itemStacks = CloneItemStacks(inventory?.itemStacks)
            };
        }

        private static MechBayOwnedMechDefinition[] CloneOwnedMechs(MechBayOwnedMechDefinition[] ownedMechs)
        {
            if (ownedMechs == null || ownedMechs.Length == 0)
            {
                return Array.Empty<MechBayOwnedMechDefinition>();
            }

            MechBayOwnedMechDefinition[] clones = new MechBayOwnedMechDefinition[ownedMechs.Length];
            for (int index = 0; index < ownedMechs.Length; index++)
            {
                MechBayOwnedMechDefinition mech = ownedMechs[index];
                if (mech == null)
                {
                    continue;
                }

                clones[index] = new MechBayOwnedMechDefinition
                {
                    ownedMechId = mech.ownedMechId,
                    unitId = mech.unitId,
                    unitType = mech.unitType,
                    chassisId = mech.chassisId,
                    displayName = mech.displayName,
                    activeLoadoutId = mech.activeLoadoutId,
                    availableForMission = mech.availableForMission,
                    conditionPercent = mech.conditionPercent,
                    pilotId = mech.pilotId,
                    pilotDisplayName = mech.pilotDisplayName,
                    pilotType = mech.pilotType
                };
            }

            return clones;
        }

        private static MechBayItemStackDefinition[] CloneItemStacks(MechBayItemStackDefinition[] itemStacks)
        {
            if (itemStacks == null || itemStacks.Length == 0)
            {
                return Array.Empty<MechBayItemStackDefinition>();
            }

            MechBayItemStackDefinition[] clones = new MechBayItemStackDefinition[itemStacks.Length];
            for (int index = 0; index < itemStacks.Length; index++)
            {
                MechBayItemStackDefinition stack = itemStacks[index];
                if (stack == null)
                {
                    continue;
                }

                clones[index] = new MechBayItemStackDefinition
                {
                    itemId = stack.itemId,
                    displayName = stack.displayName,
                    category = stack.category,
                    quantity = stack.quantity,
                    equippedQuantity = stack.equippedQuantity
                };
            }

            return clones;
        }
    }
}
