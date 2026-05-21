using System;
using System.IO;
using MC2Demo.BattleCore;
using MC2Demo.Presentation;
using UnityEditor;
using UnityEngine;

namespace MC2Demo.EditorTools
{
    public static class Mc2DemoValidator
    {
        public static void ValidateMissionContract()
        {
            ValidateMissionContractWithoutExit();
            EditorApplication.Exit(0);
        }

        public static void ValidateMissionContractWithoutExit()
        {
            string contractPath = Path.Combine(
                Application.dataPath,
                "StreamingAssets",
                "Missions",
                "mc2_01",
                "mission-contract.json");

            if (!File.Exists(contractPath))
            {
                throw new FileNotFoundException("Mission contract missing.", contractPath);
            }

            string combatDataPath = Path.Combine(
                Application.dataPath,
                "StreamingAssets",
                "Data",
                "combat-data.json");

            if (!File.Exists(combatDataPath))
            {
                throw new FileNotFoundException("Combat data missing.", combatDataPath);
            }

            CombatProfileCatalog combatProfiles = CombatProfileCatalog.FromJson(File.ReadAllText(combatDataPath));
            if (combatProfiles.UnitProfileCount < 8)
            {
                throw new InvalidDataException("Expected at least 8 combat profiles, got " + combatProfiles.UnitProfileCount);
            }

            ValidateSourceDrivenProfiles(combatProfiles);
            ValidateSourceLoadoutPreview(combatProfiles);
            ValidateRuntimeLoadoutCombatOverride(combatProfiles);
            ValidateLoadoutContractShape();

            string contractJson = File.ReadAllText(contractPath);
            BattleMission mission = BattleMission.FromJson(contractJson, combatProfiles);
            ValidateMechBayInventoryContract(mission);
            if (mission.Units.Count != 29)
            {
                throw new InvalidDataException("Expected 29 units, got " + mission.Units.Count);
            }

            if (mission.Objectives.Count != 9)
            {
                throw new InvalidDataException("Expected 9 objectives, got " + mission.Objectives.Count);
            }

            if (mission.Structures.Count != 1)
            {
                throw new InvalidDataException("Expected 1 targetable structure, got " + mission.Structures.Count);
            }

            int terrainObjectCount = mission.Contract.terrainObjects == null ? 0 : mission.Contract.terrainObjects.Length;
            if (terrainObjectCount != 1000)
            {
                throw new InvalidDataException("Expected 1000 terrain objects, got " + terrainObjectCount);
            }

            if (mission.Contract.terrainMesh == null || mission.Contract.terrainMesh.samples == null)
            {
                throw new InvalidDataException("Expected source terrain mesh in mission contract.");
            }

            if (mission.Contract.terrainMesh.sampleSide != 100 || mission.Contract.terrainMesh.samples.Length != 10000)
            {
                throw new InvalidDataException(
                    "Expected 100x100 terrain mesh samples, got side="
                    + mission.Contract.terrainMesh.sampleSide
                    + " samples="
                    + mission.Contract.terrainMesh.samples.Length);
            }

            int playerUnits = 0;
            foreach (UnitState unit in mission.Units)
            {
                if (unit.IsPlayerUnit)
                {
                    playerUnits++;
                }
            }

            if (playerUnits != 3)
            {
                throw new InvalidDataException("Expected 3 player units, got " + playerUnits);
            }

            if (mission.Result != MissionResultState.InProgress)
            {
                throw new InvalidDataException("Expected reference mission to start in progress, got " + mission.Result);
            }

            ValidateMissionResults();
            ValidateObjectivePrerequisites();
            ValidateSectionDamageModifiers();
            ValidateHeatManagement();
            ValidateCommanderCommandPort();
            ValidateCommanderCommandFilePlayback();
            ValidateCommanderObservationPort();
            ValidateRuleCommander(BattleMission.FromJson(contractJson, combatProfiles));
            ValidateMiniMaxCommanderDirectiveExtraction();
            ValidateCommanderObjectiveTargets(BattleMission.FromJson(contractJson, combatProfiles));
            ValidateMissionActivation(BattleMission.FromJson(contractJson, combatProfiles));
            ValidateEncounterActivationTiming(
                BattleMission.FromJson(contractJson, combatProfiles),
                BattleMission.FromJson(contractJson, combatProfiles));
            ValidateAirfieldToHangarObjectiveFlow(BattleMission.FromJson(contractJson, combatProfiles));
            ValidateScriptBridgeSignals(BattleMission.FromJson(contractJson, combatProfiles));
            ValidateNavMarkerPatrolOrders();
            ValidateJumpCommand(BattleMission.FromJson(contractJson, combatProfiles));
            ValidateCombatSimulation(mission);
            ValidateStructureObjective(new BattleMission(MakeStructureObjectiveContract(), CombatProfileCatalog.Empty));

            Debug.Log("MC2 demo contract validation OK: 29 units, 3 player units, 9 objectives, 1 structure, 10000 terrain samples, 1000 terrain objects, combat simulation passed.");
        }

        private static void ValidateSourceDrivenProfiles(CombatProfileCatalog combatProfiles)
        {
            string[] unitTypes =
            {
                "Werewolf",
                "Bushwacker",
                "Starslayer",
                "UrbanMech",
                "Centipede",
                "Harasser",
                "LRMC",
                "Infantry"
            };

            foreach (string unitType in unitTypes)
            {
                CombatProfile profile = combatProfiles.ForUnitType(unitType, false);
                if (profile.SourceKind == "hardcoded-fallback" || profile.SourceKind == "temporary-vehicle-or-infantry-default")
                {
                    throw new InvalidDataException("Combat profile is still fallback-driven: " + unitType);
                }

                if (profile.Weapons.Length == 0)
                {
                    throw new InvalidDataException("Combat profile has no source weapon loadout: " + unitType);
                }

                if (profile.TotalWeaponWeight <= 0f)
                {
                    throw new InvalidDataException("Combat profile has no source weapon weight: " + unitType);
                }

                if (profile.HeatCapacity <= 0f)
                {
                    throw new InvalidDataException("Combat profile has no heat capacity: " + unitType);
                }

                bool hasWeaponMetrics = false;
                foreach (CombatWeaponDefinition weapon in profile.Weapons)
                {
                    if (weapon != null && weapon.rangeMax > 0f && (weapon.damage > 0f || weapon.damagePerTenSeconds > 0f))
                    {
                        hasWeaponMetrics = true;
                        break;
                    }
                }

                if (!hasWeaponMetrics)
                {
                    throw new InvalidDataException("Combat profile has no weapon range/damage metrics: " + unitType);
                }
            }
        }

        private static void ValidateLoadoutContractShape()
        {
            LoadoutContract contract = MakeSyntheticLoadoutContract();
            if (contract.schema != "mc2-loadout-contract-v1")
            {
                throw new InvalidDataException("Unexpected loadout schema: " + contract.schema);
            }

            if (contract.chassisDefinitions == null || contract.chassisDefinitions.Length != 1)
            {
                throw new InvalidDataException("Expected one synthetic chassis definition.");
            }

            LoadoutChassisDefinition chassis = contract.chassisDefinitions[0];
            if (chassis.slotGrid == null || chassis.slotGrid.width != 3 || chassis.slotGrid.height != 3)
            {
                throw new InvalidDataException("Expected a 3x3 synthetic loadout grid.");
            }

            if (chassis.equipmentSlots == null || chassis.equipmentSlots.Length != 2)
            {
                throw new InvalidDataException("Expected radar and jump jet equipment slots.");
            }

            if (contract.loadouts == null || contract.loadouts.Length != 1)
            {
                throw new InvalidDataException("Expected one synthetic loadout build.");
            }

            LoadoutBuildDefinition loadout = contract.loadouts[0];
            if (loadout.placedItems == null || loadout.placedItems.Length != 5)
            {
                throw new InvalidDataException("Expected weapon, armor, heat sink, radar, and jump jet items.");
            }

            float heat = 0f;
            float weight = 0f;
            int gridItems = 0;
            int slottedItems = 0;
            foreach (LoadoutPlacedItemDefinition placedItem in loadout.placedItems)
            {
                LoadoutItemDefinition item = FindLoadoutItem(contract, placedItem.itemId);
                heat += item.heat;
                weight += item.weight;

                if (string.IsNullOrEmpty(placedItem.equipmentSlotId))
                {
                    gridItems++;
                    if (item.shapeCells == null || item.shapeCells.Length == 0)
                    {
                        throw new InvalidDataException("Grid item has no occupied cells: " + item.itemId);
                    }
                }
                else
                {
                    slottedItems++;
                    LoadoutEquipmentSlotDefinition slot = FindLoadoutEquipmentSlot(chassis, placedItem.equipmentSlotId);
                    if (!ItemAllowsEquipmentSlot(item, slot.slotType))
                    {
                        throw new InvalidDataException("Item does not allow equipment slot type: " + item.itemId);
                    }
                }
            }

            if (gridItems != 3 || slottedItems != 2)
            {
                throw new InvalidDataException("Expected three grid items and two equipment-slot items.");
            }

            if (heat > chassis.heatLimit || weight > chassis.weightLimit)
            {
                throw new InvalidDataException("Synthetic loadout exceeds heat or weight limits.");
            }

            LoadoutItemDefinition verticalWeapon = FindLoadoutItem(contract, "synthetic-ppc");
            if (verticalWeapon.shapeCells.Length != 3)
            {
                throw new InvalidDataException("Expected synthetic weapon to occupy three cells.");
            }

            if (FindLoadoutItem(contract, "synthetic-armor-plate").armorHardnessBonus <= 0f)
            {
                throw new InvalidDataException("Expected armor plate to define a hardness bonus.");
            }

            if (FindLoadoutItem(contract, "synthetic-heat-sink").heatDissipationBonus <= 0f)
            {
                throw new InvalidDataException("Expected heat sink to define a dissipation bonus.");
            }

            ValidateSyntheticLoadoutValidator(contract, loadout);
        }

        private static void ValidateMechBayInventoryContract(BattleMission mission)
        {
            MechBayInventoryContract inventory = MechBayInventoryBuilder.BuildDemoInventory(mission.PlayerUnits());
            MechBayInventoryValidationResult result = MechBayInventoryValidator.Validate(inventory);
            if (!result.IsValid)
            {
                throw new InvalidDataException("Starter mech bay inventory is invalid: " + FirstInventoryError(result));
            }

            if (inventory.schema != MechBayInventoryValidator.Schema)
            {
                throw new InvalidDataException("Unexpected starter mech bay inventory schema: " + inventory.schema);
            }

            if (result.Summary.MechCount != 3)
            {
                throw new InvalidDataException("Expected starter inventory to expose 3 player mechs.");
            }

            MechBayOwnedRosterEntry[] starterRoster = MechBayOwnedRosterService.BuildRosterPreview(inventory);
            if (starterRoster.Length != 3 || HasWarehouseRosterEntry(starterRoster, "Raven"))
            {
                throw new InvalidDataException("Expected starter owned-mech roster to expose only the initial player squad.");
            }

            if (string.IsNullOrWhiteSpace(starterRoster[0].ownedMechId)
                || string.IsNullOrWhiteSpace(starterRoster[0].activeLoadoutId)
                || starterRoster[0].loadoutStatus != "Ready fit"
                || starterRoster[0].hasDraftFitStub
                || starterRoster[0].draftFitReady
                || starterRoster[0].draftFitStatus != "Use squad fit cards below"
                || starterRoster[0].draftFitRequirements != "Current fit active"
                || !starterRoster[0].hasPilotAssignment
                || starterRoster[0].hasSpareWeaponStock
                || starterRoster[0].spareWeaponStockCount != 0
                || starterRoster[0].totalWeaponStockCount <= 0
                || string.IsNullOrWhiteSpace(starterRoster[0].spareWeaponStockStatus)
                || starterRoster[0].hasPilotPlaceholder
                || starterRoster[0].pilotStatus != "Assigned"
                || starterRoster[0].pilotDisplayName != "Mission pilot"
                || starterRoster[0].conditionPercent <= 0)
            {
                throw new InvalidDataException("Expected starter owned-mech roster entries to include detail preview fields.");
            }

            if (result.Summary.TokenBalance <= 0)
            {
                throw new InvalidDataException("Expected starter inventory to expose demo token balance.");
            }

            MechBayPilotHirePreview pilotHirePreview = MechBayPilotHirePreviewService.BuildPreview(inventory);
            if (pilotHirePreview == null
                || pilotHirePreview.TokenBalance != inventory.tokenBalance
                || pilotHirePreview.Status != "NPC pilot hire preview"
                || pilotHirePreview.Candidates == null
                || pilotHirePreview.Candidates.Length != 2
                || string.IsNullOrWhiteSpace(pilotHirePreview.Candidates[0].pilotId)
                || string.IsNullOrWhiteSpace(pilotHirePreview.Candidates[0].displayName)
                || pilotHirePreview.Candidates[0].pilotType != "NPC"
                || pilotHirePreview.Candidates[0].hireCost <= 0
                || !pilotHirePreview.Candidates[0].canAfford
                || !pilotHirePreview.Candidates[0].hireEnabled
                || pilotHirePreview.Candidates[0].hireStatus != "Demo hire"
                || pilotHirePreview.Candidates[0].riskProfile != "NPC death risk")
            {
                throw new InvalidDataException("Expected starter pilot hire preview to expose demo-hirable NPC candidates.");
            }

            MechBayWeaponShopPreview shopPreview = MechBayWeaponShopPreviewService.BuildPreview(inventory);
            if (shopPreview == null
                || shopPreview.TokenBalance != inventory.tokenBalance
                || shopPreview.Status != "Ordinary weapon shop preview"
                || shopPreview.Entries == null
                || shopPreview.Entries.Length != 3
                || !shopPreview.Entries[0].purchaseEnabled
                || !shopPreview.Entries[0].canAfford
                || shopPreview.Entries[0].tokenCost <= 0
                || shopPreview.Entries[0].purchaseStatus != "Demo purchase"
                || string.IsNullOrWhiteSpace(shopPreview.Entries[0].displayName))
            {
                throw new InvalidDataException("Expected starter weapon shop preview to expose demo-purchasable ordinary weapons.");
            }

            int tokenBeforePurchasePreview = inventory.tokenBalance;
            int weaponCountBeforePurchasePreview = result.Summary.WeaponCount;
            MechBayWeaponPurchasePreviewResult purchasePreview = MechBayWeaponShopPreviewService.PreviewPurchase(
                inventory,
                shopPreview.Entries[0].itemId);
            MechBayInventoryValidationResult afterPurchasePreview = MechBayInventoryValidator.Validate(inventory);
            if (purchasePreview == null
                || purchasePreview.Accepted
                || purchasePreview.InventoryChanged
                || purchasePreview.TokenBalance != tokenBeforePurchasePreview
                || purchasePreview.TokenCost != shopPreview.Entries[0].tokenCost
                || !purchasePreview.CanAfford
                || purchasePreview.Message != "Ready demo purchase"
                || inventory.tokenBalance != tokenBeforePurchasePreview
                || !afterPurchasePreview.IsValid
                || afterPurchasePreview.Summary.WeaponCount != weaponCountBeforePurchasePreview)
            {
                throw new InvalidDataException("Expected starter weapon purchase stub to preview without changing inventory.");
            }

            MechBayInventoryContract purchaseInventory = MechBayInventoryBuilder.BuildDemoInventory(mission.PlayerUnits());
            MechBayInventoryValidationResult purchaseBefore = MechBayInventoryValidator.Validate(purchaseInventory);
            int tokenBeforePurchase = purchaseInventory.tokenBalance;
            int weaponCountBeforePurchase = purchaseBefore.Summary.WeaponCount;
            MechBayWeaponPurchasePreviewResult purchaseResult = MechBayWeaponShopPreviewService.TryApplyDemoPurchase(
                purchaseInventory,
                shopPreview.Entries[0].itemId);
            MechBayInventoryValidationResult purchaseAfter = MechBayInventoryValidator.Validate(purchaseInventory);
            MechBayOwnedRosterEntry[] purchaseRoster = MechBayOwnedRosterService.BuildRosterPreview(purchaseInventory);
            if (purchaseResult == null
                || !purchaseResult.Accepted
                || !purchaseResult.InventoryChanged
                || purchaseResult.TokenBalance != tokenBeforePurchase - shopPreview.Entries[0].tokenCost
                || purchaseInventory.tokenBalance != purchaseResult.TokenBalance
                || !purchaseAfter.IsValid
                || purchaseAfter.Summary.WeaponCount != weaponCountBeforePurchase + 1
                || purchaseRoster.Length == 0
                || !purchaseRoster[0].hasSpareWeaponStock
                || purchaseRoster[0].spareWeaponStockCount != 1)
            {
                throw new InvalidDataException("Expected demo weapon purchase to spend tokens and add one spare weapon.");
            }

            if (result.Summary.WeaponCount <= 0)
            {
                throw new InvalidDataException("Expected starter inventory to expose source weapon stacks.");
            }

            if (result.Summary.ArmorPlateCount <= 0 || result.Summary.HeatSinkCount <= 0)
            {
                throw new InvalidDataException("Expected starter inventory to expose armor plates and heat sinks.");
            }

            MechBayInventoryAvailabilityResult defaultAvailability = MechBayInventoryValidator.ValidateUsage(
                inventory,
                BuildMechBayInventoryUsage(mission));
            if (!defaultAvailability.IsValid)
            {
                throw new InvalidDataException("Starter mech bay inventory does not cover the default draft: " + FirstInventoryAvailabilityError(defaultAvailability));
            }

            MechBayInventoryUsage excessArmorUsage = new();
            excessArmorUsage.AddArmorPlates(result.Summary.ArmorPlateCount + 1);
            if (MechBayInventoryValidator.ValidateUsage(inventory, excessArmorUsage).IsValid)
            {
                throw new InvalidDataException("Starter mech bay inventory did not block excess armor plate usage.");
            }

            MechBayInventoryUsage excessSinkUsage = new();
            excessSinkUsage.AddHeatSinks(result.Summary.HeatSinkCount + 1);
            if (MechBayInventoryValidator.ValidateUsage(inventory, excessSinkUsage).IsValid)
            {
                throw new InvalidDataException("Starter mech bay inventory did not block excess heat sink usage.");
            }

            UnitState player = FirstPlayerUnit(mission);
            float structureBeforeDamage = player.CurrentStructure;
            float appliedDamage = player.ApplyDirectSectionDamage(player.Sections[0].Name, 5f);
            int repairCost = MechBayRepairService.EstimateRepairCostResourcePoints(player);
            if (appliedDamage <= 0f || repairCost <= 0)
            {
                throw new InvalidDataException("Starter repair affordance did not detect player mech damage.");
            }

            int tokenBeforeRepair = inventory.tokenBalance;
            MechBayRepairResult repairResult = MechBayRepairService.TryRepair(inventory, player);
            if (!repairResult.Accepted)
            {
                throw new InvalidDataException("Starter repair affordance was blocked: " + repairResult.Message);
            }

            if (inventory.tokenBalance != tokenBeforeRepair - repairCost)
            {
                throw new InvalidDataException("Starter repair affordance did not spend the expected token amount.");
            }

            if (Math.Abs(player.CurrentStructure - structureBeforeDamage) > 0.001f || MechBayRepairService.EstimateRepairCostResourcePoints(player) != 0)
            {
                throw new InvalidDataException("Starter repair affordance did not restore player mech condition.");
            }
        }

        private static MechBayInventoryUsage BuildMechBayInventoryUsage(BattleMission mission)
        {
            MechBayInventoryUsage usage = new();
            foreach (UnitState unit in mission.PlayerUnits())
            {
                usage.AddLoadoutPreview(CombatLoadoutPreviewBuilder.Build(unit.UnitType, unit.Profile));
            }

            return usage;
        }

        private static void ValidateSourceLoadoutPreview(CombatProfileCatalog combatProfiles)
        {
            string[] mechUnitTypes =
            {
                "Werewolf",
                "Bushwacker",
                "Starslayer",
                "UrbanMech"
            };

            bool testedFillerOverride = false;
            foreach (string unitType in mechUnitTypes)
            {
                CombatProfile profile = combatProfiles.ForUnitType(unitType, true);
                if (profile.Tonnage <= 0f || profile.LoadLimit <= 0f)
                {
                    throw new InvalidDataException("Expected source tonnage and load limit for mech: " + unitType);
                }

                CombatLoadoutPreview preview = CombatLoadoutPreviewBuilder.Build(unitType, profile);
                if (preview.GridCapacity < profile.Weapons.Length)
                {
                    throw new InvalidDataException("Projected loadout grid is too small for " + unitType);
                }

                if (preview.GridWidth != 4 || preview.GridHeight <= 0)
                {
                    throw new InvalidDataException("Projected loadout grid dimensions are invalid for " + unitType);
                }

                if (preview.Validation.OccupiedGridCells != preview.OccupiedCells.Length)
                {
                    throw new InvalidDataException("Projected loadout validation and visual grid disagree for " + unitType);
                }

                if (preview.OccupiedCells.Length < profile.Weapons.Length)
                {
                    throw new InvalidDataException("Projected loadout should expose at least one visual grid cell per source weapon for " + unitType);
                }

                if (!HasMultiCellWeaponProjection(preview))
                {
                    throw new InvalidDataException("Projected loadout did not expose any multi-cell weapon shape for " + unitType);
                }

                if (Math.Abs(preview.Validation.TotalHeat - profile.HeatPerShot) > 0.001f)
                {
                    throw new InvalidDataException("Projected loadout heat does not match source heat for " + unitType);
                }

                if (preview.Validation.TotalWeight + 0.001f < profile.TotalWeaponWeight)
                {
                    throw new InvalidDataException("Projected loadout weight dropped below source weapon weight for " + unitType);
                }

                if (Math.Abs(preview.HeatLimit - profile.HeatCapacity) > 0.001f
                    || Math.Abs(preview.WeightLimit - profile.LoadLimit) > 0.001f)
                {
                    throw new InvalidDataException("Projected loadout limits do not match source limits for " + unitType);
                }

                bool[] enabledWeapons = new bool[profile.Weapons.Length];
                for (int index = 0; index < enabledWeapons.Length; index++)
                {
                    enabledWeapons[index] = true;
                }

                enabledWeapons[0] = false;
                CombatLoadoutPreview disabledPreview = CombatLoadoutPreviewBuilder.Build(unitType, profile, enabledWeapons);
                if (HasPreviewCellForWeapon(disabledPreview, 0))
                {
                    throw new InvalidDataException("Projected disabled loadout still exposes disabled weapon cell for " + unitType);
                }

                if (disabledPreview.Validation.TotalHeat >= preview.Validation.TotalHeat)
                {
                    throw new InvalidDataException("Projected disabled loadout did not reduce heat for " + unitType);
                }

                if (preview.Validation.TotalArmorHardnessBonus <= 0f
                    && preview.Validation.TotalHeatDissipationBonus <= 0f
                    && preview.Validation.TotalWeight < preview.WeightLimit)
                {
                    throw new InvalidDataException("Projected loadout did not add any filler item despite spare load for " + unitType);
                }

                CombatLoadoutPreviewGridCell fillerCell = FirstFillerCell(preview);
                if (fillerCell != null)
                {
                    testedFillerOverride = true;
                    CombatLoadoutPreview emptyFillerPreview = CombatLoadoutPreviewBuilder.Build(
                        unitType,
                        profile,
                        null,
                        null,
                        new[]
                        {
                            new CombatLoadoutFillerOverride
                            {
                                gridX = fillerCell.X,
                                gridY = fillerCell.Y,
                                category = LoadoutFillerOverrideCategory.Empty
                            }
                        });
                    if (HasFillerCellAt(emptyFillerPreview, fillerCell.X, fillerCell.Y, null))
                    {
                        throw new InvalidDataException("Projected filler empty override did not clear the selected cell for " + unitType);
                    }

                    CombatLoadoutPreview heatSinkFillerPreview = CombatLoadoutPreviewBuilder.Build(
                        unitType,
                        profile,
                        null,
                        null,
                        new[]
                        {
                            new CombatLoadoutFillerOverride
                            {
                                gridX = fillerCell.X,
                                gridY = fillerCell.Y,
                                category = LoadoutItemCategory.HeatSink
                            }
                        });
                    if (!HasFillerCellAt(heatSinkFillerPreview, fillerCell.X, fillerCell.Y, LoadoutItemCategory.HeatSink))
                    {
                        throw new InvalidDataException("Projected filler heat-sink override did not place a heat sink for " + unitType);
                    }

                    if (heatSinkFillerPreview.Validation.TotalHeatDissipationBonus <= emptyFillerPreview.Validation.TotalHeatDissipationBonus)
                    {
                        throw new InvalidDataException("Projected filler heat-sink override did not increase cooling for " + unitType);
                    }
                }

                CombatLoadoutPreviewItem firstItem = preview.Items[0];
                CombatLoadoutPreviewItem secondItem = preview.Items[Math.Min(1, preview.Items.Length - 1)];
                CombatLoadoutPreview overlapPreview = CombatLoadoutPreviewBuilder.Build(
                    unitType,
                    profile,
                    null,
                    new[]
                    {
                        new CombatLoadoutPlacementOverride
                        {
                            sourceWeaponIndex = firstItem.SourceWeaponIndex,
                            gridX = secondItem.GridX,
                            gridY = secondItem.GridY
                        }
                    });
                if (overlapPreview.Validation.IsValid || !HasLoadoutErrorContaining(overlapPreview, "occupied by both"))
                {
                    throw new InvalidDataException("Projected loadout placement override did not surface overlap validation for " + unitType);
                }

                CombatLoadoutPreview outOfBoundsPreview = CombatLoadoutPreviewBuilder.Build(
                    unitType,
                    profile,
                    null,
                    new[]
                    {
                        new CombatLoadoutPlacementOverride
                        {
                            sourceWeaponIndex = firstItem.SourceWeaponIndex,
                            gridX = preview.GridWidth,
                            gridY = 0
                        }
                    });
                if (outOfBoundsPreview.Validation.IsValid || !HasLoadoutErrorContaining(outOfBoundsPreview, "out-of-bounds"))
                {
                    throw new InvalidDataException("Projected loadout placement override did not surface bounds validation for " + unitType);
                }
            }

            if (!testedFillerOverride)
            {
                throw new InvalidDataException("Expected at least one source loadout to exercise filler overrides.");
            }
        }

        private static CombatLoadoutPreviewGridCell FirstFillerCell(CombatLoadoutPreview preview)
        {
            foreach (CombatLoadoutPreviewGridCell cell in preview.OccupiedCells)
            {
                if (cell != null && cell.SourceWeaponIndex < 0)
                {
                    return cell;
                }
            }

            return null;
        }

        private static bool HasFillerCellAt(CombatLoadoutPreview preview, int x, int y, string category)
        {
            foreach (CombatLoadoutPreviewGridCell cell in preview.OccupiedCells)
            {
                if (cell == null || cell.SourceWeaponIndex >= 0 || cell.X != x || cell.Y != y)
                {
                    continue;
                }

                if (category == null || cell.Category == category)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasPreviewCellForWeapon(CombatLoadoutPreview preview, int sourceWeaponIndex)
        {
            foreach (CombatLoadoutPreviewGridCell cell in preview.OccupiedCells)
            {
                if (cell != null && cell.SourceWeaponIndex == sourceWeaponIndex)
                {
                    return true;
                }
            }

            return false;
        }

        private static int CountPreviewCellsForWeapon(CombatLoadoutPreview preview, int sourceWeaponIndex)
        {
            int count = 0;
            foreach (CombatLoadoutPreviewGridCell cell in preview.OccupiedCells)
            {
                if (cell != null && cell.SourceWeaponIndex == sourceWeaponIndex)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool HasMultiCellWeaponProjection(CombatLoadoutPreview preview)
        {
            foreach (CombatLoadoutPreviewGridCell cell in preview.OccupiedCells)
            {
                if (cell != null && CountPreviewCellsForWeapon(preview, cell.SourceWeaponIndex) > 1)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasLoadoutErrorContaining(CombatLoadoutPreview preview, string text)
        {
            foreach (string error in preview.Validation.Errors)
            {
                if (!string.IsNullOrEmpty(error)
                    && error.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateRuntimeLoadoutCombatOverride(CombatProfileCatalog combatProfiles)
        {
            UnitState unit = new(new UnitSpawn
            {
                spawnId = "loadout-override-player",
                teamId = 0,
                isPlayerUnit = true,
                unitType = "Werewolf",
                position = new MissionPose()
            }, combatProfiles);

            float sourceRange = unit.CombatWeaponRange;
            float sourceDamage = unit.CombatWeaponDamage;
            unit.ApplyDemoLoadout(new UnitLoadoutCombatOverride
            {
                weaponRange = 321f,
                weaponDamage = 7f,
                weaponCooldown = 0.8f,
                heatPerShot = 2f,
                heatDissipationPerSecond = 9f,
                armorHardnessBonus = 4f,
                totalWeaponWeight = 12f,
                primaryWeaponName = "Runtime Test Laser",
                primaryWeaponType = "Energy",
                primarySpecialEffect = 4
            });

            if (!unit.HasAppliedDemoLoadout
                || Math.Abs(unit.CombatWeaponRange - 321f) > 0.001f
                || Math.Abs(unit.CombatWeaponDamage - 7f) > 0.001f
                || Math.Abs(unit.CombatWeaponCooldown - 0.8f) > 0.001f
                || Math.Abs(unit.CombatHeatPerShot - 2f) > 0.001f
                || Math.Abs(unit.CombatHeatDissipationPerSecond - 9f) > 0.001f
                || Math.Abs(unit.CombatArmorHardnessBonus - 4f) > 0.001f
                || unit.CombatIncomingDamageMultiplier >= 1f
                || unit.CombatPrimaryWeaponName != "Runtime Test Laser"
                || unit.CombatPrimaryWeaponType != "Energy"
                || unit.CombatPrimarySpecialEffect != 4)
            {
                throw new InvalidDataException("Runtime loadout combat override was not reflected by UnitState effective stats.");
            }

            unit.ClearDemoLoadout();
            if (unit.HasAppliedDemoLoadout
                || Math.Abs(unit.CombatWeaponRange - sourceRange) > 0.001f
                || Math.Abs(unit.CombatWeaponDamage - sourceDamage) > 0.001f
                || unit.CombatArmorHardnessBonus > 0f)
            {
                throw new InvalidDataException("Runtime loadout combat override did not clear back to source stats.");
            }

            UnitState unarmored = new(new UnitSpawn
            {
                spawnId = "unarmored-damage-test",
                teamId = 0,
                isPlayerUnit = true,
                unitType = "Werewolf",
                position = new MissionPose()
            }, combatProfiles);
            UnitState armored = new(new UnitSpawn
            {
                spawnId = "armored-damage-test",
                teamId = 0,
                isPlayerUnit = true,
                unitType = "Werewolf",
                position = new MissionPose()
            }, combatProfiles);
            armored.ApplyDemoLoadout(new UnitLoadoutCombatOverride
            {
                weaponRange = armored.Profile.WeaponRange,
                weaponDamage = armored.Profile.WeaponDamage,
                weaponCooldown = armored.Profile.WeaponCooldown,
                heatPerShot = armored.Profile.HeatPerShot,
                heatDissipationPerSecond = armored.Profile.HeatDissipationPerSecond,
                armorHardnessBonus = 4f,
                totalWeaponWeight = armored.Profile.TotalWeaponWeight,
                primaryWeaponName = armored.Profile.PrimaryWeaponName,
                primaryWeaponType = armored.Profile.PrimaryWeaponType,
                primarySpecialEffect = armored.Profile.PrimarySpecialEffect
            });

            float unarmoredDamage = unarmored.ApplyDirectSectionDamage("Torso", 20f);
            float armoredDamage = armored.ApplyDirectSectionDamage("Torso", 20f);
            if (armoredDamage <= 0f || armoredDamage >= unarmoredDamage)
            {
                throw new InvalidDataException("Runtime armor hardness did not reduce incoming section damage.");
            }

            UnitState attacker = new(new UnitSpawn
            {
                spawnId = "armor-event-attacker",
                teamId = 1,
                isPlayerUnit = false,
                unitType = "Centipede",
                position = new MissionPose()
            }, combatProfiles);
            UnitState armorEventTarget = new(new UnitSpawn
            {
                spawnId = "armor-event-target",
                teamId = 0,
                isPlayerUnit = true,
                unitType = "Werewolf",
                position = new MissionPose()
            }, combatProfiles);
            armorEventTarget.ApplyDemoLoadout(new UnitLoadoutCombatOverride
            {
                weaponRange = armorEventTarget.Profile.WeaponRange,
                weaponDamage = armorEventTarget.Profile.WeaponDamage,
                weaponCooldown = armorEventTarget.Profile.WeaponCooldown,
                heatPerShot = armorEventTarget.Profile.HeatPerShot,
                heatDissipationPerSecond = armorEventTarget.Profile.HeatDissipationPerSecond,
                armorHardnessBonus = 4f,
                totalWeaponWeight = armorEventTarget.Profile.TotalWeaponWeight,
                primaryWeaponName = armorEventTarget.Profile.PrimaryWeaponName,
                primaryWeaponType = armorEventTarget.Profile.PrimaryWeaponType,
                primarySpecialEffect = armorEventTarget.Profile.PrimarySpecialEffect
            });
            CombatEvent armorEvent = attacker.FireAt(armorEventTarget);
            if (armorEvent.Damage <= 0f || armorEvent.MitigatedDamage <= 0f)
            {
                throw new InvalidDataException("Runtime armor hardness did not surface mitigation in combat events.");
            }
        }

        private static LoadoutItemDefinition FindLoadoutItem(LoadoutContract contract, string itemId)
        {
            foreach (LoadoutItemDefinition item in contract.itemDefinitions)
            {
                if (item != null && item.itemId == itemId)
                {
                    return item;
                }
            }

            throw new InvalidDataException("Missing synthetic loadout item: " + itemId);
        }

        private static LoadoutEquipmentSlotDefinition FindLoadoutEquipmentSlot(LoadoutChassisDefinition chassis, string slotId)
        {
            foreach (LoadoutEquipmentSlotDefinition slot in chassis.equipmentSlots)
            {
                if (slot != null && slot.slotId == slotId)
                {
                    return slot;
                }
            }

            throw new InvalidDataException("Missing synthetic equipment slot: " + slotId);
        }

        private static bool ItemAllowsEquipmentSlot(LoadoutItemDefinition item, string slotType)
        {
            if (item.allowedEquipmentSlotTypes == null)
            {
                return false;
            }

            foreach (string allowedSlotType in item.allowedEquipmentSlotTypes)
            {
                if (allowedSlotType == slotType)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateSyntheticLoadoutValidator(LoadoutContract contract, LoadoutBuildDefinition stockLoadout)
        {
            AssertLoadoutValid(contract, stockLoadout, "stock synthetic loadout");
            LoadoutValidationResult stockResult = LoadoutValidator.Validate(contract, stockLoadout);
            if (Math.Abs(stockResult.TotalHeat - 8f) > 0.001f || Math.Abs(stockResult.TotalWeight - 11f) > 0.001f)
            {
                throw new InvalidDataException("Unexpected synthetic loadout heat/weight totals.");
            }

            if (Math.Abs(stockResult.TotalArmorHardnessBonus - 3f) > 0.001f
                || Math.Abs(stockResult.TotalHeatDissipationBonus - 2f) > 0.001f)
            {
                throw new InvalidDataException("Unexpected synthetic loadout armor/heat-sink totals.");
            }

            AssertLoadoutValid(
                contract,
                MakeSyntheticLoadout(
                    "synthetic-rotated-weapon",
                    new LoadoutPlacedItemDefinition { itemId = "synthetic-ppc", gridX = 0, gridY = 2, rotated = true, sectionName = "Right Arm" },
                    new LoadoutPlacedItemDefinition { itemId = "synthetic-armor-plate", gridX = 1, gridY = 0, sectionName = "Torso" },
                    new LoadoutPlacedItemDefinition { itemId = "synthetic-heat-sink", gridX = 1, gridY = 1, sectionName = "Torso" },
                    new LoadoutPlacedItemDefinition { itemId = "synthetic-radar", equipmentSlotId = "radar-slot", sectionName = "Torso" },
                    new LoadoutPlacedItemDefinition { itemId = "synthetic-jump-jets", equipmentSlotId = "jump-slot", sectionName = "Legs" }),
                "rotated synthetic weapon loadout");

            AssertLoadoutInvalid(
                contract,
                MakeSyntheticLoadout(
                    "synthetic-overlap",
                    new LoadoutPlacedItemDefinition { itemId = "synthetic-ppc", gridX = 0, gridY = 0 },
                    new LoadoutPlacedItemDefinition { itemId = "synthetic-armor-plate", gridX = 0, gridY = 1 }),
                "overlapping grid cells");

            AssertLoadoutInvalid(
                contract,
                MakeSyntheticLoadout(
                    "synthetic-blocked-cell",
                    new LoadoutPlacedItemDefinition { itemId = "synthetic-armor-plate", gridX = 2, gridY = 0 }),
                "blocked grid cell");

            AssertLoadoutInvalid(
                contract,
                MakeSyntheticLoadout(
                    "synthetic-out-of-bounds",
                    new LoadoutPlacedItemDefinition { itemId = "synthetic-ppc", gridX = 2, gridY = 0 }),
                "out-of-bounds grid cell");

            AssertLoadoutInvalid(
                contract,
                MakeSyntheticLoadout(
                    "synthetic-slot-mismatch",
                    new LoadoutPlacedItemDefinition { itemId = "synthetic-radar", equipmentSlotId = "jump-slot" }),
                "equipment slot type mismatch");

            AssertLoadoutInvalid(
                contract,
                MakeSyntheticLoadout(
                    "synthetic-duplicate-slot",
                    new LoadoutPlacedItemDefinition { itemId = "synthetic-radar", equipmentSlotId = "radar-slot" },
                    new LoadoutPlacedItemDefinition { itemId = "synthetic-radar", equipmentSlotId = "radar-slot" }),
                "duplicate equipment slot use");

            LoadoutChassisDefinition chassis = contract.chassisDefinitions[0];
            float oldHeatLimit = chassis.heatLimit;
            chassis.heatLimit = 7f;
            AssertLoadoutInvalid(contract, stockLoadout, "heat limit overrun");
            chassis.heatLimit = oldHeatLimit;

            float oldWeightLimit = chassis.weightLimit;
            chassis.weightLimit = 10f;
            AssertLoadoutInvalid(contract, stockLoadout, "weight limit overrun");
            chassis.weightLimit = oldWeightLimit;
        }

        private static void AssertLoadoutValid(LoadoutContract contract, LoadoutBuildDefinition loadout, string scenario)
        {
            LoadoutValidationResult result = LoadoutValidator.Validate(contract, loadout);
            if (!result.IsValid)
            {
                throw new InvalidDataException("Expected valid loadout for " + scenario + ", got: " + FirstLoadoutError(result));
            }
        }

        private static void AssertLoadoutInvalid(LoadoutContract contract, LoadoutBuildDefinition loadout, string scenario)
        {
            LoadoutValidationResult result = LoadoutValidator.Validate(contract, loadout);
            if (result.IsValid)
            {
                throw new InvalidDataException("Expected invalid loadout for " + scenario + ".");
            }
        }

        private static string FirstLoadoutError(LoadoutValidationResult result)
        {
            string[] errors = result.Errors;
            return errors.Length == 0 ? "no errors" : errors[0];
        }

        private static string FirstInventoryError(MechBayInventoryValidationResult result)
        {
            string[] errors = result.Errors;
            return errors.Length == 0 ? "no errors" : errors[0];
        }

        private static string FirstInventoryAvailabilityError(MechBayInventoryAvailabilityResult result)
        {
            string[] errors = result.Errors;
            return errors.Length == 0 ? "no errors" : errors[0];
        }

        private static bool HasWarehouseRosterEntry(MechBayOwnedRosterEntry[] roster, string unitType)
        {
            return FindWarehouseRosterEntry(roster, unitType) != null;
        }

        private static MechBayOwnedRosterEntry FindWarehouseRosterEntry(MechBayOwnedRosterEntry[] roster, string unitType)
        {
            MechBayOwnedRosterEntry[] entries = roster ?? Array.Empty<MechBayOwnedRosterEntry>();
            for (int index = 0; index < entries.Length; index++)
            {
                MechBayOwnedRosterEntry entry = entries[index];
                if (entry != null
                    && entry.isWarehouseMech
                    && string.Equals(entry.unitType, unitType, StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }

            return null;
        }

        private static LoadoutBuildDefinition MakeSyntheticLoadout(string loadoutId, params LoadoutPlacedItemDefinition[] placedItems)
        {
            return new LoadoutBuildDefinition
            {
                loadoutId = loadoutId,
                chassisId = "synthetic-light-mech",
                displayName = loadoutId,
                placedItems = placedItems
            };
        }

        private static LoadoutContract MakeSyntheticLoadoutContract()
        {
            return new LoadoutContract
            {
                schema = "mc2-loadout-contract-v1",
                chassisDefinitions = new[]
                {
                    new LoadoutChassisDefinition
                    {
                        chassisId = "synthetic-light-mech",
                        displayName = "Synthetic Light Mech",
                        sourceKind = "validator-synthetic",
                        heatLimit = 12f,
                        weightLimit = 18f,
                        slotGrid = new LoadoutSlotGridDefinition
                        {
                            width = 3,
                            height = 3,
                            blockedCells = new[]
                            {
                                new LoadoutGridCell { x = 2, y = 0 }
                            }
                        },
                        equipmentSlots = new[]
                        {
                            new LoadoutEquipmentSlotDefinition
                            {
                                slotId = "radar-slot",
                                slotType = LoadoutEquipmentSlotType.Radar,
                                sectionName = "Torso"
                            },
                            new LoadoutEquipmentSlotDefinition
                            {
                                slotId = "jump-slot",
                                slotType = LoadoutEquipmentSlotType.JumpJet,
                                sectionName = "Legs"
                            }
                        },
                        sections = new[]
                        {
                            new LoadoutSectionDefinition { sectionName = "Cockpit", baseStructure = 15f, critical = true },
                            new LoadoutSectionDefinition { sectionName = "Torso", baseStructure = 45f },
                            new LoadoutSectionDefinition { sectionName = "Left Arm", baseStructure = 22f },
                            new LoadoutSectionDefinition { sectionName = "Right Arm", baseStructure = 22f },
                            new LoadoutSectionDefinition { sectionName = "Legs", baseStructure = 28f }
                        }
                    }
                },
                itemDefinitions = new[]
                {
                    new LoadoutItemDefinition
                    {
                        itemId = "synthetic-ppc",
                        displayName = "Synthetic PPC",
                        category = LoadoutItemCategory.Weapon,
                        weaponType = "EnergyWeapon",
                        sourceComponentId = 140,
                        heat = 8f,
                        weight = 6f,
                        damage = 18f,
                        range = 780f,
                        cooldown = 3f,
                        shapeCells = new[]
                        {
                            new LoadoutGridCell { x = 0, y = 0 },
                            new LoadoutGridCell { x = 0, y = 1 },
                            new LoadoutGridCell { x = 0, y = 2 }
                        }
                    },
                    new LoadoutItemDefinition
                    {
                        itemId = "synthetic-armor-plate",
                        displayName = "Synthetic Armor Plate",
                        category = LoadoutItemCategory.ArmorPlate,
                        weight = 1f,
                        armorHardnessBonus = 3f,
                        shapeCells = new[]
                        {
                            new LoadoutGridCell { x = 0, y = 0 }
                        }
                    },
                    new LoadoutItemDefinition
                    {
                        itemId = "synthetic-heat-sink",
                        displayName = "Synthetic Heat Sink",
                        category = LoadoutItemCategory.HeatSink,
                        weight = 1f,
                        heatDissipationBonus = 2f,
                        shapeCells = new[]
                        {
                            new LoadoutGridCell { x = 0, y = 0 }
                        }
                    },
                    new LoadoutItemDefinition
                    {
                        itemId = "synthetic-radar",
                        displayName = "Synthetic Radar",
                        category = LoadoutItemCategory.Radar,
                        weight = 1f,
                        allowedEquipmentSlotTypes = new[] { LoadoutEquipmentSlotType.Radar }
                    },
                    new LoadoutItemDefinition
                    {
                        itemId = "synthetic-jump-jets",
                        displayName = "Synthetic Jump Jets",
                        category = LoadoutItemCategory.JumpJet,
                        weight = 2f,
                        allowedEquipmentSlotTypes = new[] { LoadoutEquipmentSlotType.JumpJet }
                    }
                },
                loadouts = new[]
                {
                    new LoadoutBuildDefinition
                    {
                        loadoutId = "synthetic-light-mech-stock",
                        chassisId = "synthetic-light-mech",
                        displayName = "Synthetic Stock Loadout",
                        placedItems = new[]
                        {
                            new LoadoutPlacedItemDefinition { itemId = "synthetic-ppc", gridX = 0, gridY = 0, sectionName = "Right Arm" },
                            new LoadoutPlacedItemDefinition { itemId = "synthetic-armor-plate", gridX = 1, gridY = 0, sectionName = "Torso" },
                            new LoadoutPlacedItemDefinition { itemId = "synthetic-heat-sink", gridX = 1, gridY = 1, sectionName = "Torso" },
                            new LoadoutPlacedItemDefinition { itemId = "synthetic-radar", equipmentSlotId = "radar-slot", sectionName = "Torso" },
                            new LoadoutPlacedItemDefinition { itemId = "synthetic-jump-jets", equipmentSlotId = "jump-slot", sectionName = "Legs" }
                        }
                    }
                }
            };
        }

        private static void ValidateCombatSimulation(BattleMission mission)
        {
            UnitState player = null;
            UnitState enemy = null;
            foreach (UnitState unit in mission.Units)
            {
                if (player == null && unit.IsPlayerUnit)
                {
                    player = unit;
                }
                else if (enemy == null && !unit.IsPlayerUnit)
                {
                    enemy = unit;
                }
            }

            if (player == null || enemy == null)
            {
                throw new InvalidDataException("Combat simulation requires one player unit and one enemy unit.");
            }

            ActivateAirfieldPatrolsIfNeeded(mission, player, enemy);
            int accepted = mission.IssueDetachedAttackUnit(player.Id, enemy.Id);
            if (accepted != 1 || !player.HasAttackOrder || player.AttackTargetId != enemy.Id)
            {
                throw new InvalidDataException("Expected detached attack order to lock the selected enemy target.");
            }

            for (int tick = 0; tick < 80 && enemy.Structure >= 1f; tick++)
            {
                mission.Tick(1f);
            }

            if (mission.RecentCombatEvents.Count == 0)
            {
                throw new InvalidDataException("Expected at least one combat event during simulation.");
            }

            if (!AnyEnemyDamaged(mission))
            {
                throw new InvalidDataException("Expected enemy to take damage during simulation.");
            }
        }

        private static void ValidateMissionActivation(BattleMission mission)
        {
            UnitState player = FirstPlayerUnit(mission);
            UnitState patrol = mission.FindUnit("unit-4");
            UnitState islandBandit = mission.FindUnit("unit-6");
            UnitState starslayer = mission.FindUnit("unit-9");
            if (player == null || patrol == null || islandBandit == null || starslayer == null)
            {
                throw new InvalidDataException("Mission activation validation requires known mc2_01 units.");
            }

            if (patrol.IsActive || islandBandit.IsActive || starslayer.IsActive)
            {
                throw new InvalidDataException("Expected mc2_01 scripted enemies to start inactive.");
            }

            MovePlayerIntoObjectiveArea(mission, player, 0);
            if (!patrol.IsActive || !islandBandit.IsActive || starslayer.IsActive)
            {
                throw new InvalidDataException("Expected airfield completion to activate patrol groups without Starslayer.");
            }

            if (mission.RecentUnitActivationEvents.Count == 0)
            {
                throw new InvalidDataException("Expected patrol activation to emit activation events.");
            }

            if (!HasObjectiveEvent(mission, 0, ObjectiveEventKind.Completed) || !HasObjectiveEvent(mission, 1, ObjectiveEventKind.Activated))
            {
                throw new InvalidDataException("Expected airfield completion to emit objective completion and unlock events.");
            }

            Vector2 patrolStart = patrol.MissionPosition;
            mission.Tick(1f);
            if (!patrol.HasMoveOrder && Vector2.Distance(patrol.MissionPosition, patrolStart) <= 0.01f)
            {
                throw new InvalidDataException("Expected activated patrol to receive a lightweight brain movement order.");
            }

            if (!islandBandit.IsActive || !islandBandit.HasMoveOrder || starslayer.IsActive || starslayer.HasMoveOrder)
            {
                throw new InvalidDataException("Expected north island patrols to start after airfield while Starslayer remains inactive.");
            }
        }

        private static void ValidateEncounterActivationTiming(BattleMission patrolMission, BattleMission starslayerMission)
        {
            string[] firstPatrol =
            {
                "unit-4",
                "unit-5",
                "unit-10",
                "unit-11",
                "unit-12",
                "unit-27",
                "unit-28",
                "unit-29"
            };

            string[] northIsland =
            {
                "unit-6",
                "unit-7",
                "unit-8",
                "unit-13"
            };

            string[] infantryAmbush =
            {
                "unit-15",
                "unit-16",
                "unit-17",
                "unit-18",
                "unit-19",
                "unit-20",
                "unit-21",
                "unit-22"
            };

            string[] starslayerLance =
            {
                "unit-9",
                "unit-14",
                "unit-23",
                "unit-24",
                "unit-25",
                "unit-26"
            };

            AssertUnitsActive(patrolMission, firstPatrol, false, "first patrol should start inactive");
            AssertUnitsActive(patrolMission, northIsland, false, "north island patrol should start inactive");
            AssertUnitsActive(patrolMission, infantryAmbush, false, "infantry ambush should start inactive");
            AssertUnitsActive(patrolMission, starslayerLance, false, "Starslayer lance should start inactive");

            UnitState player = FirstPlayerUnit(patrolMission);
            if (player == null)
            {
                throw new InvalidDataException("Encounter timing validation requires one player unit.");
            }

            MovePlayerIntoObjectiveArea(patrolMission, player, 0);
            AssertUnitsActive(patrolMission, firstPatrol, true, "first patrol should activate after airfield investigation");
            AssertUnitsActive(patrolMission, northIsland, true, "north island patrol should activate after airfield investigation");
            AssertUnitsActive(patrolMission, infantryAmbush, false, "infantry ambush should wait for hangar damage");
            AssertUnitsActive(patrolMission, starslayerLance, false, "Starslayer lance should wait for hidden Starslayer area");

            patrolMission.Tick(1f);
            AssertAnyUnitHasBrainOrder(patrolMission, firstPatrol, "first patrol should receive a lightweight patrol order");
            AssertAnyUnitHasBrainOrder(patrolMission, northIsland, "north island patrol should receive a lightweight patrol order");

            DamageStructureUntilChanged(patrolMission, "structure-1-0", maxTicks: 160);
            AssertUnitsActive(patrolMission, infantryAmbush, true, "infantry ambush should activate after hangar damage");
            if (!HasActivationEventForBrain(patrolMission, "mc2_01_infantry_ambush"))
            {
                throw new InvalidDataException("Expected hangar damage to emit infantry ambush activation events.");
            }

            patrolMission.Tick(1f);
            AssertAnyUnitHasBrainOrder(patrolMission, infantryAmbush, "infantry ambush should receive an ambush move or attack order");

            UnitState starslayerPlayer = FirstPlayerUnit(starslayerMission);
            if (starslayerPlayer == null)
            {
                throw new InvalidDataException("Starslayer timing validation requires one player unit.");
            }

            MovePlayerIntoObjectiveArea(starslayerMission, starslayerPlayer, 7);
            AssertUnitsActive(starslayerMission, starslayerLance, true, "Starslayer lance should activate from hidden objective 7 area");
            AssertUnitsActive(starslayerMission, firstPatrol, false, "first patrol should not activate from Starslayer area alone");
            AssertUnitsActive(starslayerMission, northIsland, false, "north island patrol should not activate from Starslayer area alone");
            AssertUnitsActive(starslayerMission, infantryAmbush, false, "infantry ambush should not activate from Starslayer area alone");

            starslayerMission.Tick(1f);
            AssertAnyUnitHasBrainOrder(starslayerMission, starslayerLance, "Starslayer lance should receive a lightweight patrol or attack order");
        }

        private static void ValidateScriptBridgeSignals(BattleMission mission)
        {
            UnitState player = FirstPlayerUnit(mission);
            if (player == null)
            {
                throw new InvalidDataException("Script bridge validation requires one player unit.");
            }

            MissionScriptBridge bridge = new(mission);
            MovePlayerIntoObjectiveArea(mission, player, 0);
            bridge.CaptureFrame();

            if (!HasScriptSignal(bridge, "Objective_0_Decided")
                || !HasScriptSignal(bridge, "patrol1_triggered")
                || !HasScriptSignal(bridge, "patrol2_triggered")
                || !HasScriptSignal(bridge, "patrol3_triggered"))
            {
                throw new InvalidDataException("Expected script bridge to emit airfield objective and patrol group signals.");
            }

            bridge.CaptureFrame();
            if (bridge.RecentEvents.Count != 0)
            {
                throw new InvalidDataException("Expected script bridge signals to be emitted once per mission beat.");
            }
        }

        private static void ValidateAirfieldToHangarObjectiveFlow(BattleMission mission)
        {
            UnitState player = FirstPlayerUnit(mission);
            if (player == null)
            {
                throw new InvalidDataException("Airfield to hangar flow requires one player unit.");
            }

            ObjectiveState airfield = FindObjective(mission, 0);
            ObjectiveState hangarObjective = FindObjective(mission, 1);
            ObjectiveState patrolObjective = FindObjective(mission, 2);
            StructureState hangar = mission.FindStructure("structure-1-0");
            if (airfield == null || hangarObjective == null || patrolObjective == null || hangar == null)
            {
                throw new InvalidDataException("Airfield to hangar flow requires objective 0, 1, 2, and structure-1-0.");
            }

            if (!airfield.IsActive || airfield.IsComplete || hangarObjective.IsActive || patrolObjective.IsActive)
            {
                throw new InvalidDataException("Expected only airfield objective to be active at mission start.");
            }

            MovePlayerForceIntoObjectiveArea(mission, 0);
            if (!airfield.IsComplete || !hangarObjective.IsActive || patrolObjective.IsActive)
            {
                throw new InvalidDataException("Expected airfield completion to unlock hangar objective only.");
            }

            if (!HasActiveBrainPrefix(mission, "mc2_01_Pat1"))
            {
                throw new InvalidDataException("Expected airfield completion to activate the first patrol group.");
            }

            if (!hangar.IsTargetable || hangar.IsDestroyed || hangar.Structure <= 0f)
            {
                throw new InvalidDataException("Expected hangar to remain targetable before structure attack.");
            }

            DestroyStructureWithSquad(mission, hangar.Id, expectedAccepted: 3, maxTicks: 240);
            if (!hangarObjective.IsComplete || !patrolObjective.IsActive)
            {
                throw new InvalidDataException("Expected hangar destruction to complete objective 1 and activate objective 2.");
            }

            if (mission.Result != MissionResultState.InProgress)
            {
                throw new InvalidDataException("Expected mission to continue after hangar destruction, got " + mission.Result);
            }
        }

        private static void ValidateCommanderObjectiveTargets(BattleMission mission)
        {
            CommanderObservationPort observationPort = new(mission);
            CommanderObservation initial = observationPort.Observe();
            CommanderObjectiveObservation airfield = FindObservedObjective(initial, 0);
            if (airfield == null || airfield.targetPoints.Length != 1 || airfield.targetPoints[0].kind != "area")
            {
                throw new InvalidDataException("Expected initial commander observation to expose airfield area target.");
            }

            if (airfield.targetUnitIds.Length != 0 || airfield.targetStructureIds.Length != 0 || FindObservedObjective(initial, 2) != null)
            {
                throw new InvalidDataException("Expected initial commander observation to hide future unit targets.");
            }

            MovePlayerForceIntoObjectiveArea(mission, 0);
            CommanderObservation afterAirfield = observationPort.Observe();
            CommanderObjectiveObservation hangar = FindObservedObjective(afterAirfield, 1);
            if (hangar == null
                || hangar.targetStructureIds.Length != 1
                || hangar.targetStructureIds[0] != "structure-1-0"
                || hangar.targetPoints.Length != 1
                || hangar.targetPoints[0].kind != "structure"
                || hangar.targetPoints[0].id != "structure-1-0")
            {
                throw new InvalidDataException("Expected commander observation to expose active hangar structure target: " + DescribeObservedObjective(hangar));
            }

            if (FindObservedObjective(afterAirfield, 2) != null)
            {
                throw new InvalidDataException("Expected commander observation to keep patrol objective hidden until hangar is destroyed.");
            }

            DestroyStructureWithSquad(mission, "structure-1-0", expectedAccepted: 3, maxTicks: 240);
            CommanderObservation afterHangar = observationPort.Observe();
            CommanderObjectiveObservation patrol = FindObservedObjective(afterHangar, 2);
            if (patrol == null || patrol.targetUnitIds.Length != 8 || patrol.targetPoints.Length != 8)
            {
                throw new InvalidDataException("Expected commander observation to expose active patrol unit targets after hangar destruction.");
            }

            string[] expectedUnits =
            {
                "unit-4",
                "unit-5",
                "unit-10",
                "unit-11",
                "unit-12",
                "unit-27",
                "unit-28",
                "unit-29"
            };

            foreach (string unitId in expectedUnits)
            {
                if (!ContainsString(patrol.targetUnitIds, unitId))
                {
                    throw new InvalidDataException("Expected patrol objective target ids to include " + unitId);
                }
            }

            if (FindObservedObjective(afterHangar, 0) != null || FindObservedObjective(afterHangar, 1) != null)
            {
                throw new InvalidDataException("Expected completed objectives to disappear from commander current objective observation.");
            }
        }

        private static void DestroyStructureWithSquad(BattleMission mission, string structureId, int expectedAccepted, int maxTicks)
        {
            StructureState structure = mission.FindStructure(structureId);
            if (structure == null)
            {
                throw new InvalidDataException("Expected structure to exist: " + structureId);
            }

            int accepted = mission.IssueSquadAttackStructure(structure.Id);
            if (accepted != expectedAccepted)
            {
                throw new InvalidDataException("Expected squad structure attack accepted count " + expectedAccepted + ", got " + accepted);
            }

            for (int tick = 0; tick < maxTicks && !structure.IsDestroyed && mission.Result == MissionResultState.InProgress; tick++)
            {
                mission.Tick(1f);
            }

            if (!structure.IsDestroyed)
            {
                throw new InvalidDataException(
                    "Expected structure to be destroyed during simulation. id="
                    + structure.Id
                    + " remaining="
                    + structure.CurrentStructure
                    + " missionTime="
                    + mission.MissionTimeSeconds
                    + " result="
                    + mission.Result);
            }
        }

        private static void DamageStructureUntilChanged(BattleMission mission, string structureId, int maxTicks)
        {
            StructureState structure = mission.FindStructure(structureId);
            if (structure == null)
            {
                throw new InvalidDataException("Expected structure to exist: " + structureId);
            }

            float before = structure.CurrentStructure;
            int accepted = mission.IssueSquadAttackStructure(structure.Id);
            if (accepted < 1)
            {
                throw new InvalidDataException("Expected at least one squad unit to accept structure attack on " + structure.Id);
            }

            for (int tick = 0; tick < maxTicks && structure.CurrentStructure >= before && mission.Result == MissionResultState.InProgress; tick++)
            {
                mission.Tick(1f);
            }

            if (structure.CurrentStructure >= before)
            {
                throw new InvalidDataException(
                    "Expected structure to take damage during simulation. id="
                    + structure.Id
                    + " current="
                    + structure.CurrentStructure
                    + " before="
                    + before
                    + " missionTime="
                    + mission.MissionTimeSeconds
                    + " result="
                    + mission.Result);
            }
        }

        private static void ActivateAirfieldPatrolsIfNeeded(BattleMission mission, UnitState player, UnitState enemy)
        {
            if (enemy.IsActive)
            {
                return;
            }

            MovePlayerIntoObjectiveArea(mission, player, 0);
            if (!enemy.IsActive)
            {
                throw new InvalidDataException("Expected first enemy target to activate after airfield objective.");
            }
        }

        private static void ValidateNavMarkerPatrolOrders()
        {
            BattleMission mission = new(MakeNavMarkerPatrolContract(), CombatProfileCatalog.Empty);
            UnitState patrol = mission.FindUnit("unit-4");
            if (patrol == null || !patrol.IsActive)
            {
                throw new InvalidDataException("Nav marker patrol validation requires an active patrol unit.");
            }

            mission.Tick(0.1f);
            Vector2 expected = new(1240f, 1000f);
            if (!patrol.HasMoveOrder || Vector2.Distance(patrol.MoveTarget, expected) > 0.01f)
            {
                throw new InvalidDataException(
                    "Expected mc2_01 patrol to use nav marker 0 as its patrol anchor, got "
                    + patrol.MoveTarget);
            }
        }

        private static UnitState FirstPlayerUnit(BattleMission mission)
        {
            foreach (UnitState unit in mission.Units)
            {
                if (unit.IsPlayerUnit)
                {
                    return unit;
                }
            }

            return null;
        }

        private static void MovePlayerIntoObjectiveArea(BattleMission mission, UnitState player, int objectiveIndex)
        {
            TargetArea area = FirstObjectiveArea(mission, objectiveIndex);
            if (area == null)
            {
                throw new InvalidDataException("Expected objective " + objectiveIndex + " to have a target area.");
            }

            Vector2 center = new(area.x, area.y);
            player.SetMoveOrder(center, detached: false);
            for (int tick = 0; tick < 120; tick++)
            {
                mission.Tick(1f);
                if (Vector2.Distance(player.MissionPosition, center) <= area.radius)
                {
                    return;
                }
            }

            throw new InvalidDataException("Expected player to reach objective " + objectiveIndex + " area.");
        }

        private static void MovePlayerForceIntoObjectiveArea(BattleMission mission, int objectiveIndex)
        {
            TargetArea area = FirstObjectiveArea(mission, objectiveIndex);
            if (area == null)
            {
                throw new InvalidDataException("Expected objective " + objectiveIndex + " to have a target area.");
            }

            Vector2 center = new(area.x, area.y);
            int accepted = mission.IssueSquadMove(center);
            if (accepted < 1)
            {
                throw new InvalidDataException("Expected player force to accept move order for objective " + objectiveIndex + ".");
            }

            for (int tick = 0; tick < 120; tick++)
            {
                mission.Tick(1f);
                if (AllLivePlayerUnitsInArea(mission, center, area.radius))
                {
                    return;
                }
            }

            throw new InvalidDataException("Expected player force to reach objective " + objectiveIndex + " area.");
        }

        private static bool AllLivePlayerUnitsInArea(BattleMission mission, Vector2 center, float radius)
        {
            bool sawLivePlayer = false;
            float radiusSqr = radius * radius;
            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit.IsDestroyed)
                {
                    continue;
                }

                sawLivePlayer = true;
                if ((unit.MissionPosition - center).sqrMagnitude > radiusSqr)
                {
                    return false;
                }
            }

            return sawLivePlayer;
        }

        private static TargetArea FirstObjectiveArea(BattleMission mission, int objectiveIndex)
        {
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (objective.Definition.index != objectiveIndex || objective.Definition.conditions == null)
                {
                    continue;
                }

                foreach (ObjectiveCondition condition in objective.Definition.conditions)
                {
                    if (condition.type == "MoveAnyUnitToArea" || condition.type == "MoveAllSurvivingMechsToArea")
                    {
                        return condition.targetArea;
                    }
                }
            }

            return null;
        }

        private static ObjectiveState FindObjective(BattleMission mission, int objectiveIndex)
        {
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (objective.Definition.index == objectiveIndex)
                {
                    return objective;
                }
            }

            return null;
        }

        private static bool HasActiveBrainPrefix(BattleMission mission, string brainPrefix)
        {
            foreach (UnitState unit in mission.Units)
            {
                if (!unit.IsPlayerUnit
                    && unit.IsActive
                    && !unit.IsDestroyed
                    && !string.IsNullOrEmpty(unit.Brain)
                    && unit.Brain.StartsWith(brainPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AssertUnitsActive(BattleMission mission, string[] unitIds, bool expectedActive, string context)
        {
            foreach (string unitId in unitIds)
            {
                UnitState unit = mission.FindUnit(unitId);
                if (unit == null)
                {
                    throw new InvalidDataException("Expected unit to exist for encounter timing validation: " + unitId);
                }

                if (unit.IsActive != expectedActive)
                {
                    throw new InvalidDataException(
                        "Expected "
                        + unitId
                        + " active="
                        + expectedActive
                        + " because "
                        + context
                        + ", got active="
                        + unit.IsActive
                        + " brain="
                        + unit.Brain);
                }
            }
        }

        private static void AssertAnyUnitHasBrainOrder(BattleMission mission, string[] unitIds, string context)
        {
            foreach (string unitId in unitIds)
            {
                UnitState unit = mission.FindUnit(unitId);
                if (unit != null && unit.IsActive && (unit.HasMoveOrder || unit.HasAttackOrder))
                {
                    return;
                }
            }

            throw new InvalidDataException("Expected at least one unit to have a brain order because " + context);
        }

        private static bool HasActivationEventForBrain(BattleMission mission, string brain)
        {
            foreach (UnitActivationEvent activationEvent in mission.RecentUnitActivationEvents)
            {
                if (string.Equals(activationEvent.Brain, brain, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static CommanderObjectiveObservation FindObservedObjective(CommanderObservation observation, int objectiveIndex)
        {
            if (observation.currentObjectives == null)
            {
                return null;
            }

            foreach (CommanderObjectiveObservation objective in observation.currentObjectives)
            {
                if (objective.index == objectiveIndex)
                {
                    return objective;
                }
            }

            return null;
        }

        private static string DescribeObservedObjective(CommanderObjectiveObservation objective)
        {
            if (objective == null)
            {
                return "<missing>";
            }

            string pointDescription = "<none>";
            if (objective.targetPoints != null && objective.targetPoints.Length > 0)
            {
                pointDescription = objective.targetPoints[0].kind + ":" + objective.targetPoints[0].id;
            }

            return "index="
                + objective.index
                + " structureIds="
                + (objective.targetStructureIds == null ? -1 : objective.targetStructureIds.Length)
                + " firstStructure="
                + (objective.targetStructureIds == null || objective.targetStructureIds.Length == 0 ? "<none>" : objective.targetStructureIds[0])
                + " points="
                + (objective.targetPoints == null ? -1 : objective.targetPoints.Length)
                + " firstPoint="
                + pointDescription;
        }

        private static bool ContainsString(string[] values, string value)
        {
            if (values == null)
            {
                return false;
            }

            foreach (string item in values)
            {
                if (item == value)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AnyEnemyDamaged(BattleMission mission)
        {
            foreach (UnitState unit in mission.Units)
            {
                if (!unit.IsPlayerUnit && unit.Structure < 1f)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateSectionDamageModifiers()
        {
            BattleMission mission = new(MakeSectionModifierContract(), MakeSectionModifierProfiles());
            UnitState player = mission.FindUnit("modifier-player");
            UnitState target = mission.FindUnit("modifier-target");
            if (player == null || target == null)
            {
                throw new InvalidDataException("Section modifier simulation requires player and target units.");
            }

            float structureBefore = target.CurrentStructure;
            player.FireAt(target);
            float fullDamage = structureBefore - target.CurrentStructure;
            if (fullDamage < 99f)
            {
                throw new InvalidDataException("Expected undamaged player to apply full weapon damage, got " + fullDamage);
            }

            player.ApplyDirectSectionDamage("Left Arm", 10f);
            player.ApplyDirectSectionDamage("Right Arm", 10f);
            if (player.FirepowerRatio > 0.41f || player.FirepowerRatio < 0.39f)
            {
                throw new InvalidDataException("Expected destroyed arms to reduce firepower to 40%, got " + player.FirepowerRatio);
            }

            structureBefore = target.CurrentStructure;
            player.FireAt(target);
            float reducedDamage = structureBefore - target.CurrentStructure;
            if (reducedDamage > 41f || reducedDamage < 39f)
            {
                throw new InvalidDataException("Expected damaged arms to reduce weapon damage to 40, got " + reducedDamage);
            }

            Vector2 start = player.MissionPosition;
            player.SetMoveOrder(start + new Vector2(1000f, 0f), detached: false);
            player.TickMovement(1f);
            float normalMove = Vector2.Distance(start, player.MissionPosition);
            player.ApplyDirectSectionDamage("Legs", 10f);
            if (player.CanUseJumpJets)
            {
                throw new InvalidDataException("Expected destroyed legs to disable jump jets.");
            }

            start = player.MissionPosition;
            player.SetMoveOrder(start + new Vector2(1000f, 0f), detached: false);
            player.TickMovement(1f);
            float damagedMove = Vector2.Distance(start, player.MissionPosition);
            if (damagedMove >= normalMove * 0.6f || player.MobilityRatio > 0.46f || player.TryStartJumpToward(start + new Vector2(1000f, 0f), 520f, _ => true, detached: false))
            {
                throw new InvalidDataException("Expected destroyed legs to slow movement and reject jump commands.");
            }
        }

        private static void ValidateHeatManagement()
        {
            BattleMission mission = new(MakeHeatContract(), MakeHeatProfiles());
            UnitState player = mission.FindUnit("heat-player");
            UnitState target = mission.FindUnit("heat-target");
            UnitState farTarget = mission.FindUnit("heat-far-target");
            if (player == null || target == null || farTarget == null)
            {
                throw new InvalidDataException("Heat simulation requires player and target units.");
            }

            if (!player.CanFireAt(target))
            {
                throw new InvalidDataException("Expected cool unit to be able to fire.");
            }

            CombatEvent heatShot = player.FireAt(target);
            if (player.CurrentHeat < 7.9f || player.HeatRatio < 0.79f)
            {
                throw new InvalidDataException("Expected firing to add weapon heat, got " + player.CurrentHeat);
            }

            if (heatShot.WeaponType != "EnergyWeapon" || heatShot.SpecialEffect != 15)
            {
                throw new InvalidDataException("Expected combat event to carry source weapon visual metadata.");
            }

            if (player.WeaponReadinessRatio >= 1f || !player.IsWeaponCoolingDown || !player.IsInWeaponRange(target))
            {
                throw new InvalidDataException("Expected firing to expose cooldown and range readiness state.");
            }

            player.TickWeapon(0.2f);
            if (player.CanFireAt(target))
            {
                throw new InvalidDataException("Expected high heat to lock out the next shot.");
            }

            player.TickWeapon(10f);
            if (player.IsHeatLocked || !player.CanFireAt(target))
            {
                throw new InvalidDataException("Expected cooling to restore firing.");
            }

            if (player.IsInWeaponRange(farTarget))
            {
                throw new InvalidDataException("Expected weapon range helper to report distant target out of range.");
            }
        }

        private static void ValidateCommanderCommandPort()
        {
            BattleMission mission = new(MakeCommandPortContract(), CombatProfileCatalog.Empty);
            CommanderCommandPort port = new(mission, 520f, _ => true);
            UnitState playerOne = mission.FindUnit("player-1");
            UnitState playerTwo = mission.FindUnit("player-2");

            CommanderCommandResult squadMove = port.IssueText("squad move 500 0");
            if (!squadMove.Accepted || squadMove.AcceptedCount != 2 || playerOne.MoveTarget.x != 500f || playerTwo.MoveTarget.x != 500f)
            {
                throw new InvalidDataException("Expected command port to issue squad move commands.");
            }

            CommanderCommandResult detachedMove = port.IssueText("unit player-1 move 100 100");
            if (!detachedMove.Accepted || detachedMove.AcceptedCount != 1 || !playerOne.IsDetached)
            {
                throw new InvalidDataException("Expected command port to issue detached unit move commands.");
            }

            CommanderCommandResult squadAttack = port.IssueText("squad attack unit enemy-1");
            if (!squadAttack.Accepted || squadAttack.AcceptedCount != 1 || playerTwo.AttackTargetId != "enemy-1" || playerOne.AttackTargetId == "enemy-1")
            {
                throw new InvalidDataException("Expected squad attack to skip detached units and target enemy-1.");
            }

            CommanderCommandResult structureAttack = port.IssueText("unit player-1 attack structure structure-1");
            if (!structureAttack.Accepted || playerOne.AttackTargetId != "structure-1")
            {
                throw new InvalidDataException("Expected detached structure attack command to lock structure-1.");
            }

            CommanderCommandResult jump = port.IssueText("unit player-1 jump 1000 0");
            if (!jump.Accepted || !playerOne.IsJumping)
            {
                throw new InvalidDataException("Expected command port to issue unit jump commands.");
            }

            CommanderCommandResult blocked = port.IssueText("squad dance 1 2");
            if (blocked.Accepted)
            {
                throw new InvalidDataException("Expected malformed command text to be rejected.");
            }
        }

        private static void ValidateCommanderCommandFilePlayback()
        {
            StartupCommanderScriptAction[] actions = StartupCommanderScript.ParseLines(
                new[]
                {
                    "# validator command file",
                    "",
                    "command squad move 500 0",
                    "advance 0.5",
                    "report",
                    "command unit player-1 attack structure structure-1"
                },
                "validator-command-file");

            if (actions.Length != 4
                || actions[0].Kind != StartupCommanderScriptActionKind.Command
                || actions[1].Kind != StartupCommanderScriptActionKind.Advance
                || actions[2].Kind != StartupCommanderScriptActionKind.Report
                || actions[3].CommandText != "unit player-1 attack structure structure-1")
            {
                throw new InvalidDataException("Expected command file parser to preserve command, advance, and report actions.");
            }

            if (Math.Abs(actions[1].AdvanceSeconds - 0.5f) > 0.001f)
            {
                throw new InvalidDataException("Expected command file parser to read advance seconds.");
            }

            BattleMission mission = new(MakeCommandPortContract(), CombatProfileCatalog.Empty);
            CommanderCommandPort port = new(mission, 520f, _ => true);
            foreach (StartupCommanderScriptAction action in actions)
            {
                switch (action.Kind)
                {
                    case StartupCommanderScriptActionKind.Command:
                        CommanderCommandResult result = port.IssueText(action.CommandText);
                        if (!result.Accepted)
                        {
                            throw new InvalidDataException("Expected command file command to be accepted: " + action.CommandText);
                        }
                        break;
                    case StartupCommanderScriptActionKind.Advance:
                        mission.Tick(action.AdvanceSeconds);
                        break;
                    case StartupCommanderScriptActionKind.Report:
                        string json = new CommanderObservationPort(mission).ToJson();
                        if (string.IsNullOrEmpty(json) || !json.Contains("validator-command-port"))
                        {
                            throw new InvalidDataException("Expected command file report action to produce commander observation JSON.");
                        }
                        break;
                }
            }

            UnitState playerOne = mission.FindUnit("player-1");
            UnitState playerTwo = mission.FindUnit("player-2");
            if (playerOne.AttackTargetId != "structure-1" || playerTwo.MoveTarget.x != 500f)
            {
                throw new InvalidDataException("Expected command file playback to affect the battle mission.");
            }

            if (Math.Abs(mission.MissionTimeSeconds - 0.5f) > 0.001f)
            {
                throw new InvalidDataException("Expected command file advance action to progress mission time.");
            }

            if (StartupCommanderScript.TryParseLine("advance nope", 1, out _, out _)
                || StartupCommanderScript.TryParseLine("command", 1, out _, out _)
                || StartupCommanderScript.TryParseLine("report now", 1, out _, out _))
            {
                throw new InvalidDataException("Expected malformed command file lines to be rejected.");
            }
        }

        private static void ValidateCommanderObservationPort()
        {
            BattleMission mission = new(MakeCommandPortContract(), CombatProfileCatalog.Empty);
            CommanderCommandPort commandPort = new(mission, 520f, _ => true);
            commandPort.IssueText("unit player-1 move 100 100");
            commandPort.IssueText("squad attack unit enemy-1");

            CommanderObservationPort observationPort = new(mission);
            CommanderObservation observation = observationPort.Observe();
            if (observation.missionId != "validator-command-port" || observation.result != "InProgress")
            {
                throw new InvalidDataException("Expected commander observation to include mission identity and result state.");
            }

            if (observation.missionEnded || observation.resultSummary == null)
            {
                throw new InvalidDataException("Expected in-progress commander observation to include a live result summary without marking mission ended.");
            }

            if (observation.reportIndex != 1 || Math.Abs(observation.missionTimeSeconds) > 0.001f)
            {
                throw new InvalidDataException("Expected first commander observation to include report index and zero mission time.");
            }

            if (observation.playerUnits.Length != 2 || observation.activeHostiles.Length != 1 || observation.targetableStructures.Length != 1)
            {
                throw new InvalidDataException("Expected commander observation to include player units, active hostiles, and targetable structures.");
            }

            CommanderUnitObservation playerOne = FindObservedUnit(observation.playerUnits, "player-1");
            CommanderUnitObservation playerTwo = FindObservedUnit(observation.playerUnits, "player-2");
            if (playerOne == null || playerTwo == null || !playerOne.detached || playerOne.moveTargetX != 100f)
            {
                throw new InvalidDataException("Expected observation to expose detached unit move state.");
            }

            if (playerTwo.attackTargetId != "enemy-1" || playerTwo.weaponRange <= 0f || playerTwo.sections.Length == 0)
            {
                throw new InvalidDataException("Expected observation to expose attack targets, weapon range, and section state.");
            }

            mission.Tick(1.25f);
            CommanderObservation timedObservation = observationPort.Observe();
            if (timedObservation.reportIndex != 2 || Math.Abs(timedObservation.missionTimeSeconds - 1.25f) > 0.001f)
            {
                throw new InvalidDataException("Expected commander observation to expose advancing mission time and report index.");
            }

            string json = observationPort.ToJson();
            if (string.IsNullOrEmpty(json)
                || !json.Contains("validator-command-port")
                || !json.Contains("playerUnits")
                || !json.Contains("missionTimeSeconds")
                || !json.Contains("reportIndex"))
            {
                throw new InvalidDataException("Expected commander observation JSON to include mission and unit fields.");
            }
        }

        private static void ValidateRuleCommander(BattleMission initialMission)
        {
            RuleCommander commander = new();
            CommanderObservation initial = new CommanderObservationPort(initialMission).Observe();
            string initialCommand = commander.ChooseCommand(initial);
            if (string.IsNullOrWhiteSpace(initialCommand) || !initialCommand.StartsWith("squad move ", StringComparison.Ordinal))
            {
                throw new InvalidDataException("Expected rule commander to move toward the initial mc2_01 objective, got: " + initialCommand);
            }

            if (!CommanderCommandPort.TryParse(initialCommand, out CommanderCommand parsedInitial, out string initialError)
                || parsedInitial.Kind != CommanderCommandKind.Move
                || parsedInitial.Scope != CommanderCommandScope.Squad)
            {
                throw new InvalidDataException("Expected rule commander initial command to parse as squad move: " + initialError);
            }

            CommanderCommandResult initialResult = new CommanderCommandPort(initialMission, 520f, _ => true).IssueText(initialCommand);
            if (!initialResult.Accepted || initialResult.AcceptedCount != 3)
            {
                throw new InvalidDataException("Expected initial rule commander move to be accepted by all three player units.");
            }

            CommanderObservation attackObservation = new()
            {
                missionId = "validator-rule-attack",
                result = MissionResultState.InProgress.ToString(),
                missionEnded = false,
                playerUnits = new[]
                {
                    new CommanderUnitObservation
                    {
                        id = "player-1",
                        active = true,
                        destroyed = false,
                        weaponRange = 250f,
                        x = 0f,
                        y = 0f
                    }
                },
                activeHostiles = new[]
                {
                    new CommanderUnitObservation
                    {
                        id = "enemy-far",
                        active = true,
                        destroyed = false,
                        x = 220f,
                        y = 0f
                    },
                    new CommanderUnitObservation
                    {
                        id = "enemy-near",
                        active = true,
                        destroyed = false,
                        x = 120f,
                        y = 0f
                    }
                },
                targetableStructures = Array.Empty<CommanderStructureObservation>(),
                currentObjectives = Array.Empty<CommanderObjectiveObservation>()
            };

            string attackCommand = commander.ChooseCommand(attackObservation);
            if (attackCommand != "squad attack unit enemy-near")
            {
                throw new InvalidDataException("Expected rule commander to attack the closest hostile in range: " + attackCommand);
            }

            if (!CommanderCommandPort.TryParse(attackCommand, out CommanderCommand parsedAttack, out string attackError)
                || parsedAttack.Kind != CommanderCommandKind.AttackUnit)
            {
                throw new InvalidDataException("Expected rule commander attack command to parse: " + attackError);
            }

            CommanderObservation structureObservation = new()
            {
                missionId = "validator-rule-structure",
                result = MissionResultState.InProgress.ToString(),
                missionEnded = false,
                playerUnits = new[]
                {
                    new CommanderUnitObservation
                    {
                        id = "player-1",
                        active = true,
                        destroyed = false,
                        weaponRange = 100f,
                        x = 0f,
                        y = 0f
                    }
                },
                activeHostiles = Array.Empty<CommanderUnitObservation>(),
                targetableStructures = new[]
                {
                    new CommanderStructureObservation
                    {
                        id = "structure-1",
                        destroyed = false,
                        x = 80f,
                        y = 0f,
                        radius = 40f
                    }
                },
                currentObjectives = new[]
                {
                    new CommanderObjectiveObservation
                    {
                        index = 1,
                        title = "Destroy structure",
                        targetStructureIds = new[] { "structure-1" },
                        targetPoints = Array.Empty<CommanderTargetPointObservation>()
                    }
                }
            };

            string structureCommand = commander.ChooseCommand(structureObservation);
            if (structureCommand != "squad attack structure structure-1")
            {
                throw new InvalidDataException("Expected rule commander to attack current structure target in range: " + structureCommand);
            }

            if (!CommanderCommandPort.TryParse(structureCommand, out CommanderCommand parsedStructure, out string structureError)
                || parsedStructure.Kind != CommanderCommandKind.AttackStructure)
            {
                throw new InvalidDataException("Expected rule commander structure command to parse: " + structureError);
            }

            if (commander.ChooseCommandForDirective(structureObservation, RuleCommander.DirectiveHold) != "")
            {
                throw new InvalidDataException("Expected hold directive to produce no immediate local command.");
            }

            string directiveCommand = commander.ChooseCommandForDirective(attackObservation, RuleCommander.DirectiveEngageHostiles);
            if (directiveCommand != "squad attack unit enemy-near")
            {
                throw new InvalidDataException("Expected engage-hostiles directive to use local target selection: " + directiveCommand);
            }
        }

        private static void ValidateMiniMaxCommanderDirectiveExtraction()
        {
            string directive = MiniMaxCommander.ExtractDirectiveFromText("<think>choose target</think>\n\nassault-objective");
            if (directive != RuleCommander.DirectiveAssaultObjective)
            {
                throw new InvalidDataException("Expected MiniMax commander to strip thinking text, got: " + directive);
            }

            directive = MiniMaxCommander.ExtractDirectiveFromText("{\"directive\":\"engage-hostiles\"}");
            if (directive != RuleCommander.DirectiveEngageHostiles)
            {
                throw new InvalidDataException("Expected MiniMax commander to extract JSON directive, got: " + directive);
            }

            directive = MiniMaxCommander.ExtractDirectiveFromText("```text\nregroup\n```");
            if (directive != RuleCommander.DirectiveRegroup)
            {
                throw new InvalidDataException("Expected MiniMax commander to extract fenced directive, got: " + directive);
            }
        }

        private static void ValidateMissionResults()
        {
            CombatProfileCatalog resultProfiles = MakeResultProfiles();
            BattleMission instantVictory = new(MakeResultContract(completeOnStart: true), resultProfiles);
            if (instantVictory.Result != MissionResultState.Victory)
            {
                throw new InvalidDataException("Expected minimal completed mission to resolve victory, got " + instantVictory.Result);
            }

            MissionResultSummary victorySummary = instantVictory.ResultSummary;
            if (victorySummary.completedVisibleObjectives != 1
                || victorySummary.visibleObjectives != 1
                || victorySummary.damagedPlayerUnits != 0
                || victorySummary.destroyedEnemyUnits != 0
                || victorySummary.completedRewardResourcePoints != 1000
                || victorySummary.visibleRewardResourcePoints != 1000
                || victorySummary.repairCostResourcePoints != 0
                || victorySummary.netResourcePoints != 1000)
            {
                throw new InvalidDataException("Expected victory result summary to count objectives, bounty, and repair.");
            }

            CommanderObservation victoryObservation = new CommanderObservationPort(instantVictory).Observe();
            if (!victoryObservation.missionEnded
                || victoryObservation.resultSummary == null
                || victoryObservation.resultSummary.completedVisibleObjectives != 1)
            {
                throw new InvalidDataException("Expected ended commander observation to include victory result summary.");
            }

            MechBayInventoryContract receiptInventory = MechBayInventoryBuilder.BuildDemoInventory(instantVictory.PlayerUnits());
            int tokenBeforeReceipt = receiptInventory.tokenBalance;
            MissionResultSummary receiptSummary = new()
            {
                completedRewardResourcePoints = 1250,
                destroyedEnemyUnitLabels = new[] { "enemy-1 Raven", "enemy-2 Raven", "enemy-3 Centipede" },
                salvageClaimCount = 3
            };
            MechBayMissionReceipt receipt = MechBayMissionReceiptService.ApplyMissionReceipt(receiptInventory, receiptSummary);
            if (!receipt.Applied
                || receipt.TokenDelta != 1250
                || receipt.TokenBalance != tokenBeforeReceipt + 1250
                || receipt.SalvageFragmentCount != 3)
            {
                throw new InvalidDataException("Expected mission receipt to apply token rewards and salvage fragments.");
            }

            MechBayInventoryValidationResult receiptInventoryResult = MechBayInventoryValidator.Validate(receiptInventory);
            if (!receiptInventoryResult.IsValid || receiptInventoryResult.Summary.MechFragmentCount != 3)
            {
                throw new InvalidDataException("Expected mission receipt to keep inventory valid and expose mech fragments.");
            }

            int mechCountBeforeAssembly = receiptInventoryResult.Summary.MechCount;
            MechBayAssemblyProgress[] assemblyProgress = MechBayAssemblyPreviewService.BuildAssemblyPreview(receiptInventory);
            MechBayAssemblyProgress bestProgress = MechBayAssemblyPreviewService.BestAssemblyProgress(receiptInventory);
            if (assemblyProgress.Length != 2
                || bestProgress == null
                || bestProgress.unitType != "Raven"
                || bestProgress.fragments != 2
                || bestProgress.requiredFragments != MechBayAssemblyPreviewService.DemoRequiredFragmentsPerMech
                || bestProgress.canAssemble)
            {
                throw new InvalidDataException("Expected starter assembly preview to show Raven fragment progress below threshold.");
            }

            MechBayMissionReceipt assemblyReceipt = MechBayMissionReceiptService.ApplyMissionReceipt(
                receiptInventory,
                new MissionResultSummary
                {
                    destroyedEnemyUnitLabels = new[] { "enemy-4 Raven" },
                    salvageClaimCount = 1
                });
            receiptInventoryResult = MechBayInventoryValidator.Validate(receiptInventory);
            bestProgress = MechBayAssemblyPreviewService.BestAssemblyProgress(receiptInventory);
            if (assemblyReceipt.AssembledMechCount != 1
                || assemblyReceipt.AssembledMechNames == null
                || assemblyReceipt.AssembledMechNames.Length != 1
                || assemblyReceipt.AssembledMechNames[0] != "Raven"
                || !receiptInventoryResult.IsValid
                || receiptInventoryResult.Summary.MechCount != mechCountBeforeAssembly + 1
                || receiptInventoryResult.Summary.MechFragmentCount != 1)
            {
                throw new InvalidDataException("Expected ready starter fragments to auto-assemble one Raven and leave only remaining fragments.");
            }

            if (bestProgress == null || bestProgress.unitType != "Centipede" || bestProgress.fragments != 1 || bestProgress.canAssemble)
            {
                throw new InvalidDataException("Expected auto-assembly to consume ready Raven fragments and leave Centipede progress.");
            }

            MechBayOwnedRosterEntry[] assembledRoster = MechBayOwnedRosterService.BuildRosterPreview(receiptInventory);
            MechBayOwnedRosterEntry assembledRaven = FindWarehouseRosterEntry(assembledRoster, "Raven");
            if (assembledRaven == null
                || assembledRaven.conditionPercent != 100
                || assembledRaven.availableForMission
                || assembledRaven.activeLoadoutId != "pending-loadout"
                || assembledRaven.loadoutStatus != "Needs loadout"
                || !assembledRaven.hasDraftFitStub
                || assembledRaven.draftFitReady
                || assembledRaven.draftFitStatus != "Draft fitting locked for this demo"
                || assembledRaven.draftFitRequirements != "Need stock weapons + pilot"
                || assembledRaven.hasSpareWeaponStock
                || assembledRaven.spareWeaponStockCount != 0
                || string.IsNullOrWhiteSpace(assembledRaven.spareWeaponStockStatus)
                || assembledRaven.hasPilotAssignment
                || !assembledRaven.hasPilotPlaceholder
                || assembledRaven.pilotStatus != "Pilot required"
                || assembledRaven.pilotDisplayName != "No pilot assigned"
                || string.IsNullOrWhiteSpace(assembledRaven.ownedMechId))
            {
                throw new InvalidDataException("Expected assembled warehouse Raven to stay held with a pending loadout placeholder.");
            }

            MechBayPilotHirePreview warehousePilotHirePreview = MechBayPilotHirePreviewService.BuildPreview(receiptInventory);
            MechBayPilotHireCandidate hireCandidate = warehousePilotHirePreview.Candidates[0];
            int tokenBeforePilotHirePreview = receiptInventory.tokenBalance;
            MechBayPilotHireResult hirePreview = MechBayPilotHirePreviewService.PreviewHire(
                receiptInventory,
                assembledRaven.ownedMechId,
                hireCandidate.pilotId);
            MechBayOwnedRosterEntry[] pilotPreviewRoster = MechBayOwnedRosterService.BuildRosterPreview(receiptInventory);
            assembledRaven = FindWarehouseRosterEntry(pilotPreviewRoster, "Raven");
            if (hirePreview == null
                || hirePreview.Accepted
                || hirePreview.InventoryChanged
                || hirePreview.TokenBalance != tokenBeforePilotHirePreview
                || hirePreview.TokenCost != hireCandidate.hireCost
                || !hirePreview.CanAfford
                || hirePreview.Message != "Ready demo hire"
                || hirePreview.RiskProfile != "NPC death risk"
                || receiptInventory.tokenBalance != tokenBeforePilotHirePreview
                || assembledRaven == null
                || assembledRaven.hasPilotAssignment
                || !assembledRaven.hasPilotPlaceholder)
            {
                throw new InvalidDataException("Expected starter pilot hire stub to preview without changing inventory.");
            }

            MechBayPilotHireResult hireResult = MechBayPilotHirePreviewService.TryApplyDemoHire(
                receiptInventory,
                assembledRaven.ownedMechId,
                hireCandidate.pilotId);
            receiptInventoryResult = MechBayInventoryValidator.Validate(receiptInventory);
            MechBayOwnedRosterEntry[] hiredRoster = MechBayOwnedRosterService.BuildRosterPreview(receiptInventory);
            assembledRaven = FindWarehouseRosterEntry(hiredRoster, "Raven");
            if (hireResult == null
                || !hireResult.Accepted
                || !hireResult.InventoryChanged
                || hireResult.TokenBalance != tokenBeforePilotHirePreview - hireCandidate.hireCost
                || receiptInventory.tokenBalance != hireResult.TokenBalance
                || !receiptInventoryResult.IsValid
                || assembledRaven == null
                || !assembledRaven.hasPilotAssignment
                || assembledRaven.hasPilotPlaceholder
                || assembledRaven.pilotStatus != "Assigned NPC"
                || assembledRaven.pilotDisplayName != hireCandidate.displayName
                || assembledRaven.draftFitReady
                || assembledRaven.draftFitRequirements != "Need stock weapons"
                || assembledRaven.availableForMission)
            {
                throw new InvalidDataException("Expected demo pilot hire to spend tokens and assign one NPC pilot to the warehouse mech.");
            }

            MechBayWeaponShopPreview fitGateShopPreview = MechBayWeaponShopPreviewService.BuildPreview(receiptInventory);
            MechBayWeaponPurchasePreviewResult fitGatePurchase = MechBayWeaponShopPreviewService.TryApplyDemoPurchase(
                receiptInventory,
                fitGateShopPreview.Entries[0].itemId);
            receiptInventoryResult = MechBayInventoryValidator.Validate(receiptInventory);
            MechBayOwnedRosterEntry[] fitGateRoster = MechBayOwnedRosterService.BuildRosterPreview(receiptInventory);
            assembledRaven = FindWarehouseRosterEntry(fitGateRoster, "Raven");
            if (fitGatePurchase == null
                || !fitGatePurchase.Accepted
                || !fitGatePurchase.InventoryChanged
                || !receiptInventoryResult.IsValid
                || assembledRaven == null
                || !assembledRaven.hasDraftFitStub
                || !assembledRaven.draftFitReady
                || assembledRaven.draftFitStatus != "Draft fitting ready"
                || assembledRaven.draftFitRequirements != "Ready for future fitting"
                || !assembledRaven.hasSpareWeaponStock
                || !assembledRaven.hasPilotAssignment
                || assembledRaven.availableForMission)
            {
                throw new InvalidDataException("Expected demo pilot hire plus spare weapon stock to unlock the warehouse draft-fit readiness gate.");
            }

            int tokenBeforeDraftFitPreview = receiptInventory.tokenBalance;
            int weaponCountBeforeDraftFitPreview = receiptInventoryResult.Summary.WeaponCount;
            MechBayWarehouseDraftFitPreview draftFitPreview =
                MechBayWarehouseDraftFitPreviewService.BuildPreview(receiptInventory, assembledRaven.ownedMechId);
            MechBayInventoryValidationResult draftFitPreviewInventoryResult = MechBayInventoryValidator.Validate(receiptInventory);
            if (draftFitPreview == null
                || !draftFitPreview.Ready
                || draftFitPreview.InventoryChanged
                || draftFitPreview.Status != "Read-only draft fit preview"
                || draftFitPreview.Requirements != "Ready for future fitting"
                || draftFitPreview.ownedMechId != assembledRaven.ownedMechId
                || draftFitPreview.pilotDisplayName != hireCandidate.displayName
                || draftFitPreview.pilotStatus != "Assigned NPC"
                || draftFitPreview.weaponItemId != fitGatePurchase.itemId
                || draftFitPreview.weaponDisplayName != fitGatePurchase.displayName
                || draftFitPreview.spareWeaponStockCount != 1
                || draftFitPreview.PreviewNote != "Preview only: no inventory or loadout changes"
                || receiptInventory.tokenBalance != tokenBeforeDraftFitPreview
                || !draftFitPreviewInventoryResult.IsValid
                || draftFitPreviewInventoryResult.Summary.WeaponCount != weaponCountBeforeDraftFitPreview)
            {
                throw new InvalidDataException("Expected warehouse draft-fit preview to show selected pilot and spare weapon without changing inventory.");
            }

            BattleMission defeat = new(MakeResultContract(completeOnStart: false), resultProfiles);
            for (int tick = 0; tick < 10 && defeat.Result == MissionResultState.InProgress; tick++)
            {
                defeat.Tick(1f);
            }

            if (defeat.Result != MissionResultState.Defeat)
            {
                throw new InvalidDataException("Expected minimal combat mission to resolve defeat, got " + defeat.Result);
            }

            MissionResultSummary defeatSummary = defeat.ResultSummary;
            if (defeatSummary.damagedPlayerUnits != 1
                || defeatSummary.completedVisibleObjectives != 0
                || defeatSummary.completedRewardResourcePoints != 0
                || defeatSummary.visibleRewardResourcePoints != 1000
                || defeatSummary.repairCostResourcePoints <= 0
                || defeatSummary.netResourcePoints >= 0)
            {
                throw new InvalidDataException("Expected defeat result summary to count the destroyed player unit, unpaid bounty, and repair debt.");
            }

            CommanderObservation defeatObservation = new CommanderObservationPort(defeat).Observe();
            if (!defeatObservation.missionEnded
                || defeatObservation.resultSummary == null
                || defeatObservation.resultSummary.damagedPlayerUnitLabels.Length != 1)
            {
                throw new InvalidDataException("Expected ended commander observation to include defeat result summary.");
            }
        }

        private static void ValidateObjectivePrerequisites()
        {
            BattleMission mission = new(MakePrerequisiteObjectiveContract(), CombatProfileCatalog.Empty);
            ObjectiveState first = ObjectiveByIndex(mission, 0);
            ObjectiveState second = ObjectiveByIndex(mission, 1);
            UnitState player = FirstPlayerUnit(mission);
            if (first == null || second == null || player == null)
            {
                throw new InvalidDataException("Prerequisite objective validation requires two objectives and one player unit.");
            }

            if (!first.IsActive || first.IsComplete || second.IsActive || second.IsComplete)
            {
                throw new InvalidDataException("Expected prerequisite objective to stay locked until previous primary objective is complete.");
            }

            MovePlayerIntoObjectiveArea(mission, player, 0);
            if (!first.IsComplete || !second.IsActive || !second.IsComplete)
            {
                throw new InvalidDataException("Expected prerequisite objective to activate after previous primary objective completion.");
            }
        }

        private static ObjectiveState ObjectiveByIndex(BattleMission mission, int index)
        {
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (objective.Definition.index == index)
                {
                    return objective;
                }
            }

            return null;
        }

        private static bool HasObjectiveEvent(BattleMission mission, int objectiveIndex, ObjectiveEventKind kind)
        {
            foreach (ObjectiveEvent objectiveEvent in mission.RecentObjectiveEvents)
            {
                if (objectiveEvent.ObjectiveIndex == objectiveIndex && objectiveEvent.Kind == kind)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasScriptSignal(MissionScriptBridge bridge, string signal)
        {
            foreach (MissionScriptEvent scriptEvent in bridge.RecentEvents)
            {
                if (scriptEvent.Signal == signal)
                {
                    return true;
                }
            }

            return false;
        }

        private static CommanderUnitObservation FindObservedUnit(CommanderUnitObservation[] units, string unitId)
        {
            foreach (CommanderUnitObservation unit in units)
            {
                if (unit.id == unitId)
                {
                    return unit;
                }
            }

            return null;
        }

        private static CombatProfileCatalog MakeResultProfiles()
        {
            return new CombatProfileCatalog(new CombatDataContract
            {
                unitProfiles = new[]
                {
                    new CombatUnitProfile
                    {
                        unitType = "ValidatorPlayer",
                        sourceKind = "validator",
                        sections = new[]
                        {
                            new CombatSectionDefinition { name = "Cockpit", structure = 1f },
                            new CombatSectionDefinition { name = "Torso", structure = 1f },
                            new CombatSectionDefinition { name = "Left Arm", structure = 1f },
                            new CombatSectionDefinition { name = "Right Arm", structure = 1f },
                            new CombatSectionDefinition { name = "Legs", structure = 1f }
                        },
                        combatProfile = new CombatProfileFields
                        {
                            maxStructure = 5f,
                            moveSpeed = 0f,
                            weaponRange = 10f,
                            weaponDamage = 0f,
                            weaponCooldown = 1f
                        }
                    },
                    new CombatUnitProfile
                    {
                        unitType = "ValidatorEnemy",
                        sourceKind = "validator",
                        combatProfile = new CombatProfileFields
                        {
                            maxStructure = 80f,
                            moveSpeed = 0f,
                            weaponRange = 100f,
                            weaponDamage = 20f,
                            weaponCooldown = 0.1f
                        }
                    }
                }
            });
        }

        private static MissionContract MakeResultContract(bool completeOnStart)
        {
            return new MissionContract
            {
                mission = new MissionDefinition
                {
                    id = completeOnStart ? "validator-victory" : "validator-defeat",
                    terrain = new TerrainDefinition { minX = -1000f, minY = 1000f, waterElevation = 350f }
                },
                units = new[]
                {
                    new UnitSpawn
                    {
                        spawnId = "player-1",
                        isPlayerUnit = true,
                        teamId = 0,
                        unitType = "ValidatorPlayer",
                        position = new MissionPose { x = 0f, y = 0f, rotation = 0f }
                    },
                    new UnitSpawn
                    {
                        spawnId = "enemy-1",
                        isPlayerUnit = false,
                        teamId = 1,
                        unitType = "ValidatorEnemy",
                        position = new MissionPose { x = 0f, y = 0f, rotation = 0f }
                    }
                },
                objectives = new[]
                {
                    new ObjectiveDefinition
                    {
                        id = "primary-1",
                        index = 0,
                        hidden = false,
                        rewardResourcePoints = 1000,
                        conditions = new[]
                        {
                            new ObjectiveCondition
                            {
                                type = "MoveAnyUnitToArea",
                                targetArea = new TargetArea
                                {
                                    x = completeOnStart ? 0f : 5000f,
                                    y = 0f,
                                    radius = 40f
                                }
                            }
                        }
                    }
                }
            };
        }

        private static MissionContract MakeNavMarkerPatrolContract()
        {
            return new MissionContract
            {
                mission = new MissionDefinition
                {
                    id = "mc2_01",
                    terrain = new TerrainDefinition { minX = -4000f, minY = 4000f, waterElevation = 350f }
                },
                units = new[]
                {
                    new UnitSpawn
                    {
                        spawnId = "player-1",
                        isPlayerUnit = true,
                        teamId = 0,
                        unitType = "Werewolf",
                        position = new MissionPose { x = -3000f, y = -3000f, rotation = 0f }
                    },
                    new UnitSpawn
                    {
                        spawnId = "unit-4",
                        isPlayerUnit = false,
                        teamId = 1,
                        unitType = "Centipede",
                        brain = "mc2_01_Pat1",
                        position = new MissionPose { x = 0f, y = 0f, rotation = 0f }
                    }
                },
                objectives = new[]
                {
                    new ObjectiveDefinition
                    {
                        id = "hidden-activate-airfield",
                        index = 0,
                        hidden = true,
                        conditions = new[]
                        {
                            new ObjectiveCondition
                            {
                                type = "MoveAnyUnitToArea",
                                targetArea = new TargetArea { x = -3000f, y = -3000f, radius = 40f }
                            }
                        },
                        actions = new[]
                        {
                            new ObjectiveAction
                            {
                                type = "SetBooleanFlag",
                                flag = new FlagAction { id = "0", value = true }
                            }
                        }
                    }
                },
                navMarkers = new[]
                {
                    new NavMarker { index = 0, x = 1000f, y = 1000f, radius = 240f }
                }
            };
        }

        private static MissionContract MakeStructureObjectiveContract()
        {
            return new MissionContract
            {
                mission = new MissionDefinition
                {
                    id = "validator-structure-objective",
                    terrain = new TerrainDefinition { minX = -1000f, minY = 1000f, waterElevation = 350f }
                },
                units = new[]
                {
                    new UnitSpawn
                    {
                        spawnId = "structure-player-1",
                        isPlayerUnit = true,
                        teamId = 0,
                        unitType = "Werewolf",
                        position = new MissionPose { x = 0f, y = 0f, rotation = 0f }
                    },
                    new UnitSpawn
                    {
                        spawnId = "structure-player-2",
                        isPlayerUnit = true,
                        teamId = 0,
                        unitType = "Bushwacker",
                        position = new MissionPose { x = 80f, y = 0f, rotation = 0f }
                    }
                },
                staticObjects = new[]
                {
                    new StaticObjectSpawn
                    {
                        objectId = "validator-structure",
                        objectType = "Structure",
                        teamId = 1,
                        targetable = true,
                        objectiveTarget = true,
                        position = new MissionPose { x = 500f, y = 0f, rotation = 0f },
                        radius = 80f,
                        maxStructure = 45f
                    }
                }
            };
        }

        private static MissionContract MakePrerequisiteObjectiveContract()
        {
            return new MissionContract
            {
                mission = new MissionDefinition
                {
                    id = "validator-objective-prerequisites",
                    terrain = new TerrainDefinition { minX = -1000f, minY = 1000f, waterElevation = 350f }
                },
                units = new[]
                {
                    new UnitSpawn
                    {
                        spawnId = "player-1",
                        isPlayerUnit = true,
                        teamId = 0,
                        unitType = "Werewolf",
                        position = new MissionPose { x = 0f, y = 0f, rotation = 0f }
                    }
                },
                objectives = new[]
                {
                    new ObjectiveDefinition
                    {
                        id = "primary-1",
                        index = 0,
                        title = "First primary",
                        hidden = false,
                        conditions = new[]
                        {
                            new ObjectiveCondition
                            {
                                type = "MoveAnyUnitToArea",
                                targetArea = new TargetArea { x = 500f, y = 0f, radius = 60f }
                            }
                        }
                    },
                    new ObjectiveDefinition
                    {
                        id = "primary-2",
                        index = 1,
                        title = "Second primary",
                        hidden = false,
                        requiresAllPreviousPrimary = true,
                        conditions = new[]
                        {
                            new ObjectiveCondition
                            {
                                type = "MoveAnyUnitToArea",
                                targetArea = new TargetArea { x = 500f, y = 0f, radius = 60f }
                            }
                        }
                    }
                }
            };
        }

        private static CombatProfileCatalog MakeSectionModifierProfiles()
        {
            return new CombatProfileCatalog(new CombatDataContract
            {
                unitProfiles = new[]
                {
                    new CombatUnitProfile
                    {
                        unitType = "ModifierPlayer",
                        sourceKind = "validator",
                        sections = new[]
                        {
                            new CombatSectionDefinition { name = "Cockpit", structure = 50f, critical = true },
                            new CombatSectionDefinition { name = "Torso", structure = 50f },
                            new CombatSectionDefinition { name = "Left Arm", structure = 10f },
                            new CombatSectionDefinition { name = "Right Arm", structure = 10f },
                            new CombatSectionDefinition { name = "Legs", structure = 10f }
                        },
                        combatProfile = new CombatProfileFields
                        {
                            maxStructure = 130f,
                            moveSpeed = 100f,
                            weaponRange = 1000f,
                            weaponDamage = 100f,
                            weaponCooldown = 0.1f
                        }
                    },
                    new CombatUnitProfile
                    {
                        unitType = "ModifierTarget",
                        sourceKind = "validator",
                        combatProfile = new CombatProfileFields
                        {
                            maxStructure = 1000f,
                            moveSpeed = 3000f,
                            weaponRange = 0f,
                            weaponDamage = 0f,
                            weaponCooldown = 1f
                        }
                    }
                }
            });
        }

        private static MissionContract MakeSectionModifierContract()
        {
            return new MissionContract
            {
                mission = new MissionDefinition
                {
                    id = "validator-section-modifiers",
                    terrain = new TerrainDefinition { minX = -1000f, minY = 1000f, waterElevation = 350f }
                },
                units = new[]
                {
                    new UnitSpawn
                    {
                        spawnId = "modifier-player",
                        isPlayerUnit = true,
                        teamId = 0,
                        unitType = "ModifierPlayer",
                        position = new MissionPose { x = 0f, y = 0f, rotation = 0f }
                    },
                    new UnitSpawn
                    {
                        spawnId = "modifier-target",
                        isPlayerUnit = false,
                        teamId = 1,
                        unitType = "ModifierTarget",
                        position = new MissionPose { x = 20f, y = 0f, rotation = 0f }
                    }
                },
                objectives = Array.Empty<ObjectiveDefinition>()
            };
        }

        private static CombatProfileCatalog MakeHeatProfiles()
        {
            return new CombatProfileCatalog(new CombatDataContract
            {
                unitProfiles = new[]
                {
                    new CombatUnitProfile
                    {
                        unitType = "HeatPlayer",
                        sourceKind = "validator",
                        heatIndex = 10f,
                        weapons = new[]
                        {
                            new CombatWeaponDefinition
                            {
                                name = "Validator Laser",
                                type = "EnergyWeapon",
                                heat = 8f,
                                damage = 10f,
                                damagePerTenSeconds = 100f,
                                recycleTime = 0.1f,
                                specialEffect = 15
                            }
                        },
                        combatProfile = new CombatProfileFields
                        {
                            maxStructure = 100f,
                            moveSpeed = 0f,
                            weaponRange = 1000f,
                            weaponDamage = 10f,
                            weaponCooldown = 0.1f
                        }
                    },
                    new CombatUnitProfile
                    {
                        unitType = "HeatTarget",
                        sourceKind = "validator",
                        combatProfile = new CombatProfileFields
                        {
                            maxStructure = 1000f,
                            moveSpeed = 0f,
                            weaponRange = 0f,
                            weaponDamage = 0f,
                            weaponCooldown = 1f
                        }
                    }
                }
            });
        }

        private static MissionContract MakeHeatContract()
        {
            return new MissionContract
            {
                mission = new MissionDefinition
                {
                    id = "validator-heat",
                    terrain = new TerrainDefinition { minX = -1000f, minY = 1000f, waterElevation = 350f }
                },
                units = new[]
                {
                    new UnitSpawn
                    {
                        spawnId = "heat-player",
                        isPlayerUnit = true,
                        teamId = 0,
                        unitType = "HeatPlayer",
                        position = new MissionPose { x = 0f, y = 0f, rotation = 0f }
                    },
                    new UnitSpawn
                    {
                        spawnId = "heat-target",
                        isPlayerUnit = false,
                        teamId = 1,
                        unitType = "HeatTarget",
                        position = new MissionPose { x = 20f, y = 0f, rotation = 0f }
                    },
                    new UnitSpawn
                    {
                        spawnId = "heat-far-target",
                        isPlayerUnit = false,
                        teamId = 1,
                        unitType = "HeatTarget",
                        position = new MissionPose { x = 2500f, y = 0f, rotation = 0f }
                    }
                },
                objectives = Array.Empty<ObjectiveDefinition>()
            };
        }

        private static MissionContract MakeCommandPortContract()
        {
            return new MissionContract
            {
                mission = new MissionDefinition
                {
                    id = "validator-command-port",
                    terrain = new TerrainDefinition { minX = -1000f, minY = 1000f, waterElevation = 350f }
                },
                units = new[]
                {
                    new UnitSpawn
                    {
                        spawnId = "player-1",
                        isPlayerUnit = true,
                        teamId = 0,
                        unitType = "Werewolf",
                        position = new MissionPose { x = 0f, y = 0f, rotation = 0f }
                    },
                    new UnitSpawn
                    {
                        spawnId = "player-2",
                        isPlayerUnit = true,
                        teamId = 0,
                        unitType = "Bushwacker",
                        position = new MissionPose { x = 80f, y = 0f, rotation = 0f }
                    },
                    new UnitSpawn
                    {
                        spawnId = "enemy-1",
                        isPlayerUnit = false,
                        teamId = 1,
                        unitType = "Centipede",
                        position = new MissionPose { x = 600f, y = 0f, rotation = 0f }
                    }
                },
                staticObjects = new[]
                {
                    new StaticObjectSpawn
                    {
                        objectId = "structure-1",
                        objectType = "Structure",
                        teamId = 1,
                        targetable = true,
                        objectiveTarget = true,
                        position = new MissionPose { x = 700f, y = 0f, rotation = 0f },
                        radius = 80f,
                        maxStructure = 45f
                    }
                },
                objectives = Array.Empty<ObjectiveDefinition>()
            };
        }

        private static void ValidateJumpCommand(BattleMission mission)
        {
            UnitState player = null;
            foreach (UnitState unit in mission.Units)
            {
                if (unit.IsPlayerUnit)
                {
                    player = unit;
                    break;
                }
            }

            if (player == null)
            {
                throw new InvalidDataException("Jump simulation requires one player unit.");
            }

            Vector2 start = player.MissionPosition;
            int accepted = mission.IssueDetachedJump(player.Id, start + new Vector2(1000f, 0f), 520f, _ => true);
            if (accepted != 1)
            {
                throw new InvalidDataException("Expected jump command to be accepted.");
            }

            mission.Tick(0.25f);
            if (!player.IsJumping || player.JumpLift <= 0f)
            {
                throw new InvalidDataException("Expected jump command to produce active lift.");
            }

            mission.Tick(1f);
            if (player.IsJumping || player.IsDetached || player.HasMoveOrder)
            {
                throw new InvalidDataException("Expected jump command to auto rejoin after landing.");
            }

            if (Vector2.Distance(start, player.MissionPosition) < 100f)
            {
                throw new InvalidDataException("Expected jump command to move the unit.");
            }

            Vector2 beforeBlockedJump = player.MissionPosition;
            int blocked = mission.IssueDetachedJump(player.Id, beforeBlockedJump + new Vector2(1000f, 0f), 520f, _ => false);
            if (blocked != 0 || Vector2.Distance(beforeBlockedJump, player.MissionPosition) > 0.01f)
            {
                throw new InvalidDataException("Expected invalid jump landing to keep the unit in place.");
            }
        }

        private static void ValidateStructureObjective(BattleMission mission)
        {
            StructureState targetStructure = mission.Structures[0];
            UnitState player = null;
            foreach (UnitState unit in mission.Units)
            {
                if (unit.IsPlayerUnit && !unit.IsDestroyed)
                {
                    player = unit;
                    break;
                }
            }

            if (player == null)
            {
                throw new InvalidDataException("Structure simulation requires one live player unit.");
            }

            int accepted = mission.IssueDetachedAttackStructure(player.Id, targetStructure.Id);
            if (accepted != 1 || !player.HasAttackOrder || player.AttackTargetId != targetStructure.Id)
            {
                throw new InvalidDataException("Expected detached attack order to lock the target structure.");
            }

            int squadAccepted = mission.IssueSquadAttackStructure(targetStructure.Id);
            if (squadAccepted < 1)
            {
                throw new InvalidDataException("Expected squadmates to join target structure attack.");
            }

            for (int tick = 0; tick < 360 && !targetStructure.IsDestroyed; tick++)
            {
                mission.Tick(1f);
            }

            if (!targetStructure.IsDestroyed)
            {
                int livePlayers = 0;
                foreach (UnitState unit in mission.PlayerUnits())
                {
                    if (!unit.IsDestroyed)
                    {
                        livePlayers++;
                    }
                }

                throw new InvalidDataException(
                    "Expected target structure to be destroyed during simulation. Remaining="
                    + targetStructure.CurrentStructure
                    + " livePlayers="
                    + livePlayers
                    + " result="
                    + mission.Result);
            }
        }
    }
}
