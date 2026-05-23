using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

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

    [Serializable]
    public sealed class MechBaySavedAccountDelta
    {
        public int tokenDelta;
        public int ownedMechDelta;
        public int itemStackDelta;
        public int readyMissionMechDelta;
        public int warehouseMechDelta;
        public bool hasChanges;
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

    public sealed class MechBaySavedAccountJsonPreviewResult
    {
        public bool Accepted { get; internal set; }
        public string Message { get; internal set; }
        public int JsonCharCount { get; internal set; }
        public MechBaySavedAccountContract LoadedAccount { get; internal set; }
        public MechBaySavedAccountValidationResult Validation { get; internal set; }
        public MechBaySavedAccountDelta Delta { get; internal set; }
    }

    public sealed class MechBaySavedAccountFileResult
    {
        public bool Accepted { get; internal set; }
        public string Message { get; internal set; }
        public string FilePath { get; internal set; }
        public int JsonCharCount { get; internal set; }
        public MechBaySavedAccountContract LoadedAccount { get; internal set; }
        public MechBaySavedAccountValidationResult Validation { get; internal set; }
        public MechBaySavedAccountDelta Delta { get; internal set; }
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

        public static MechBaySavedAccountDelta BuildDelta(
            MechBaySavedAccountContract before,
            MechBaySavedAccountContract after)
        {
            MechBaySavedAccountCounters beforeCounters = Validate(before).Counters ?? new MechBaySavedAccountCounters();
            MechBaySavedAccountCounters afterCounters = Validate(after).Counters ?? new MechBaySavedAccountCounters();
            int tokenDelta = afterCounters.tokenBalance - beforeCounters.tokenBalance;
            int ownedMechDelta = afterCounters.ownedMechCount - beforeCounters.ownedMechCount;
            int itemStackDelta = afterCounters.itemStackCount - beforeCounters.itemStackCount;
            int readyMissionMechDelta = afterCounters.readyMissionMechCount - beforeCounters.readyMissionMechCount;
            int warehouseMechDelta = afterCounters.warehouseMechCount - beforeCounters.warehouseMechCount;
            return new MechBaySavedAccountDelta
            {
                tokenDelta = tokenDelta,
                ownedMechDelta = ownedMechDelta,
                itemStackDelta = itemStackDelta,
                readyMissionMechDelta = readyMissionMechDelta,
                warehouseMechDelta = warehouseMechDelta,
                hasChanges = tokenDelta != 0
                    || ownedMechDelta != 0
                    || itemStackDelta != 0
                    || readyMissionMechDelta != 0
                    || warehouseMechDelta != 0
            };
        }

        public static string DeltaText(MechBaySavedAccountDelta delta)
        {
            if (delta == null || !delta.hasChanges)
            {
                return "Delta none";
            }

            return "Delta token "
                + FormatSigned(delta.tokenDelta)
                + "  mechs "
                + FormatSigned(delta.ownedMechDelta)
                + "  ready "
                + FormatSigned(delta.readyMissionMechDelta)
                + "  depot "
                + FormatSigned(delta.warehouseMechDelta)
                + "  stacks "
                + FormatSigned(delta.itemStackDelta);
        }

        public static MechBaySavedAccountJsonPreviewResult PreviewJsonSaveLoad(MechBaySavedAccountContract account)
        {
            MechBaySavedAccountJsonPreviewResult result = new();
            MechBaySavedAccountValidationResult sourceValidation = Validate(account);
            if (!sourceValidation.IsValid)
            {
                result.Validation = sourceValidation;
                result.Message = "Source invalid: " + FirstValidationError(sourceValidation);
                return result;
            }

            try
            {
                string json = JsonUtility.ToJson(account);
                result.JsonCharCount = json?.Length ?? 0;
                if (string.IsNullOrWhiteSpace(json) || !json.Contains(Schema))
                {
                    result.Validation = sourceValidation;
                    result.Message = "JSON preview missing schema";
                    return result;
                }

                result.LoadedAccount = JsonUtility.FromJson<MechBaySavedAccountContract>(json);
                result.Validation = Validate(result.LoadedAccount);
                result.Delta = BuildDelta(account, result.LoadedAccount);
                result.Accepted = result.Validation.IsValid && result.Delta?.hasChanges == false;
                result.Message = result.Accepted
                    ? "JSON save/load preview OK"
                    : result.Validation.IsValid
                        ? "JSON save/load preview changed counters"
                        : "Loaded invalid: " + FirstValidationError(result.Validation);
                return result;
            }
            catch (Exception exception)
            {
                result.Validation = sourceValidation;
                result.Message = "JSON preview exception: " + exception.Message;
                return result;
            }
        }

        public static MechBaySavedAccountFileResult ExportJsonFile(
            MechBaySavedAccountContract account,
            string filePath)
        {
            MechBaySavedAccountFileResult result = new()
            {
                FilePath = filePath
            };

            if (string.IsNullOrWhiteSpace(filePath))
            {
                result.Message = "Export path missing";
                return result;
            }

            MechBaySavedAccountValidationResult sourceValidation = Validate(account);
            result.Validation = sourceValidation;
            if (!sourceValidation.IsValid)
            {
                result.Message = "Source invalid: " + FirstValidationError(sourceValidation);
                return result;
            }

            try
            {
                string json = JsonUtility.ToJson(account);
                result.JsonCharCount = json?.Length ?? 0;
                if (string.IsNullOrWhiteSpace(json) || !json.Contains(Schema))
                {
                    result.Message = "Export JSON missing schema";
                    return result;
                }

                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, json);
                result.Accepted = File.Exists(filePath);
                result.Message = result.Accepted ? "JSON export OK" : "JSON export failed";
                return result;
            }
            catch (Exception exception)
            {
                result.Message = "JSON export exception: " + exception.Message;
                return result;
            }
        }

        public static MechBaySavedAccountFileResult PreviewImportJsonFile(
            string filePath,
            MechBaySavedAccountContract currentAccount)
        {
            MechBaySavedAccountFileResult result = new()
            {
                FilePath = filePath
            };

            if (string.IsNullOrWhiteSpace(filePath))
            {
                result.Message = "Import path missing";
                return result;
            }

            if (!File.Exists(filePath))
            {
                result.Message = "Import file missing";
                return result;
            }

            MechBaySavedAccountValidationResult currentValidation = Validate(currentAccount);
            if (!currentValidation.IsValid)
            {
                result.Validation = currentValidation;
                result.Message = "Current account invalid: " + FirstValidationError(currentValidation);
                return result;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                result.JsonCharCount = json?.Length ?? 0;
                if (string.IsNullOrWhiteSpace(json) || !json.Contains(Schema))
                {
                    result.Message = "Import JSON missing schema";
                    return result;
                }

                result.LoadedAccount = JsonUtility.FromJson<MechBaySavedAccountContract>(json);
                result.Validation = Validate(result.LoadedAccount);
                result.Delta = BuildDelta(currentAccount, result.LoadedAccount);
                result.Accepted = result.Validation.IsValid;
                result.Message = result.Accepted
                    ? "JSON import preview OK"
                    : "Loaded invalid: " + FirstValidationError(result.Validation);
                return result;
            }
            catch (Exception exception)
            {
                result.Message = "JSON import exception: " + exception.Message;
                return result;
            }
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

        private static string FormatSigned(int value)
        {
            return value > 0
                ? "+" + value.ToString(CultureInfo.InvariantCulture)
                : value.ToString(CultureInfo.InvariantCulture);
        }

        private static string FirstValidationError(MechBaySavedAccountValidationResult validation)
        {
            string[] errors = validation?.Errors ?? Array.Empty<string>();
            return errors.Length == 0 ? "unknown" : errors[0];
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
