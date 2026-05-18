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

            BattleMission mission = BattleMission.FromJson(File.ReadAllText(contractPath), combatProfiles);
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

            ValidateCombatSimulation(mission);
            ValidateStructureObjective(mission);

            Debug.Log("MC2 demo contract validation OK: 29 units, 3 player units, 9 objectives, 1 structure, 1000 terrain objects, combat simulation passed.");
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

            mission.IssueDetachedMove(player.Id, enemy.MissionPosition + new Vector2(120f, 0f));
            mission.Tick(20f);

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

            mission.IssueDetachedMove(player.Id, targetStructure.MissionPosition + new Vector2(80f, 0f));
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
