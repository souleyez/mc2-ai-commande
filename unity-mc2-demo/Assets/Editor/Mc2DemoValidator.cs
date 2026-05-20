using System;
using System.IO;
using MC2Demo.BattleCore;
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

            string contractJson = File.ReadAllText(contractPath);
            BattleMission mission = BattleMission.FromJson(contractJson, combatProfiles);
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
            ValidateMissionActivation(BattleMission.FromJson(contractJson, combatProfiles));
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
            }
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
            if (!patrol.IsActive || islandBandit.IsActive || starslayer.IsActive)
            {
                throw new InvalidDataException("Expected airfield completion to activate patrols only.");
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

            if (islandBandit.IsActive || islandBandit.HasMoveOrder || starslayer.IsActive || starslayer.HasMoveOrder)
            {
                throw new InvalidDataException("Expected later mc2_01 enemy groups to remain inactive and idle.");
            }
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

            if (!HasScriptSignal(bridge, "Objective_0_Decided") || !HasScriptSignal(bridge, "patrol1_triggered"))
            {
                throw new InvalidDataException("Expected script bridge to emit airfield objective and first patrol signals.");
            }

            bridge.CaptureFrame();
            if (bridge.RecentEvents.Count != 0)
            {
                throw new InvalidDataException("Expected script bridge signals to be emitted once per mission beat.");
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
                    if (condition.targetArea != null)
                    {
                        return condition.targetArea;
                    }
                }
            }

            return null;
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

        private static void ValidateMissionResults()
        {
            CombatProfileCatalog resultProfiles = MakeResultProfiles();
            BattleMission instantVictory = new(MakeResultContract(completeOnStart: true), resultProfiles);
            if (instantVictory.Result != MissionResultState.Victory)
            {
                throw new InvalidDataException("Expected minimal completed mission to resolve victory, got " + instantVictory.Result);
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
