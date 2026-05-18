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
            ValidateSectionDamageModifiers();
            ValidateJumpCommand(BattleMission.FromJson(contractJson, combatProfiles));
            ValidateCombatSimulation(mission);
            ValidateStructureObjective(BattleMission.FromJson(contractJson, combatProfiles));

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
                            moveSpeed = 0f,
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

            for (int tick = 0; tick < 240 && !targetStructure.IsDestroyed; tick++)
            {
                mission.Tick(1f);
            }

            if (!targetStructure.IsDestroyed)
            {
                throw new InvalidDataException("Expected target structure to be destroyed during simulation.");
            }
        }
    }
}
