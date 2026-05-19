using System.Collections.Generic;
using System.IO;
using System;
using MC2Demo.BattleCore;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MC2Demo.Presentation
{
    public sealed class Mc2DemoBootstrap : MonoBehaviour
    {
        [SerializeField] private string missionContractRelativePath = "Missions/mc2_01/mission-contract.json";
        [SerializeField] private string combatDataRelativePath = "Data/combat-data.json";
        [SerializeField] private float cameraHeight = 62f;
        [SerializeField] private float cameraPitch = 58f;
        [SerializeField] private float cameraYaw = 45f;
        private const float JumpDistance = 520f;

        private readonly Dictionary<string, DemoUnitView> unitViews = new();
        private readonly Dictionary<string, DemoStructureView> structureViews = new();
        private readonly Dictionary<string, GameObject> unitSelectionMarkers = new();
        private readonly Dictionary<string, GameObject> unitOrderMarkers = new();
        private readonly Dictionary<string, GameObject> unitFocusMarkers = new();
        private readonly Dictionary<string, GameObject> unitRangeMarkers = new();
        private readonly Dictionary<string, GameObject> unitTargetLines = new();
        private readonly Dictionary<string, GameObject> unitHealthBarBacks = new();
        private readonly Dictionary<string, GameObject> unitHealthBarFills = new();
        private readonly Dictionary<string, GameObject> structureHealthBarBacks = new();
        private readonly Dictionary<string, GameObject> structureHealthBarFills = new();
        private readonly Dictionary<int, bool> objectiveCompletionState = new();
        private readonly Dictionary<string, Material> materialCache = new(StringComparer.Ordinal);
        private readonly List<Material> ownedMaterials = new();
        private readonly List<string> combatLog = new();
        private BattleMission mission;
        private CombatProfileCatalog combatProfiles = CombatProfileCatalog.Empty;
        private string pendingDetachedUnitId;
        private bool pendingJumpOrder;
        private bool showMissionMap;
        private bool showSystemPanel;
        private bool isPaused;
        private MissionResultState lastMissionResult = MissionResultState.InProgress;
        private Camera mainCamera;
        private string statusText = "Loading";

        private void Start()
        {
            LoadMission();
            BuildWorld();
            ScheduleSmokeTestQuitIfRequested();
        }

        private void Update()
        {
            if (mission == null)
            {
                return;
            }

            if (!isPaused && mission.Result == MissionResultState.InProgress)
            {
                mission.Tick(Time.deltaTime);
                CaptureCombatEvents();
                CaptureObjectiveEvents();
                CaptureUnitActivationEvents();
                HandleWorldClick();
            }

            CaptureMissionResult();
            UpdateUnitVisibility();
            UpdateCommandOverlays();
            FollowCommander();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            foreach (Material material in ownedMaterials)
            {
                Destroy(material);
            }
        }

        private void LoadMission()
        {
            combatProfiles = LoadCombatProfiles();
            string path = Path.Combine(Application.streamingAssetsPath, missionContractRelativePath);
            if (!File.Exists(path))
            {
                statusText = "Missing mission contract: " + path;
                Debug.LogError(statusText);
                return;
            }

            mission = BattleMission.FromJson(File.ReadAllText(path), combatProfiles);
            statusText = "Loaded " + mission.Contract.mission.id;
            Debug.Log("MC2 demo loaded mission contract: " + mission.Contract.mission.id);
        }

        private CombatProfileCatalog LoadCombatProfiles()
        {
            string path = Path.Combine(Application.streamingAssetsPath, combatDataRelativePath);
            if (!File.Exists(path))
            {
                Debug.LogWarning("MC2 combat data missing, using fallback profiles: " + path);
                return CombatProfileCatalog.Empty;
            }

            CombatProfileCatalog catalog = CombatProfileCatalog.FromJson(File.ReadAllText(path));
            Debug.Log("MC2 demo loaded combat profiles: " + catalog.UnitProfileCount);
            return catalog;
        }

        private void BuildWorld()
        {
            if (mission == null)
            {
                return;
            }

            CreateGround();
            CreateLights();
            CreateCamera();
            CreateTerrainObjects();
            CreateUnits();
            CreateStaticObjects();
            CreateMarkers();
            CreateForests();
            CreateObjectiveAreas();
            CreateCommandOverlays();
            Debug.Log(
                "MC2 demo world built: units="
                + mission.Units.Count
                + ", structures="
                + mission.Structures.Count
                + ", objectives="
                + mission.Objectives.Count
                + ", terrainSamples="
                + (mission.Contract.terrainMesh == null || mission.Contract.terrainMesh.samples == null ? 0 : mission.Contract.terrainMesh.samples.Length)
                + ", terrainObjects="
                + (mission.Contract.terrainObjects == null ? 0 : mission.Contract.terrainObjects.Length)
                + ", forests="
                + (mission.Contract.forests == null ? 0 : mission.Contract.forests.Length));
        }

        private void ScheduleSmokeTestQuitIfRequested()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int index = 0; index < args.Length; index++)
            {
                if (args[index] == "-mc2SmokeTest")
                {
                    Invoke(nameof(QuitSmokeTest), 0.25f);
                    return;
                }
            }
        }

        private void QuitSmokeTest()
        {
            int exitCode = mission == null ? 1 : 0;
            Debug.Log("MC2 demo smoke test exiting with code " + exitCode);
            Application.Quit(exitCode);
        }

        private void CaptureCombatEvents()
        {
            foreach (CombatEvent combatEvent in mission.RecentCombatEvents)
            {
                string line = WeaponLabel(combatEvent.WeaponType)
                    + " "
                    + combatEvent.AttackerId
                    + " hit "
                    + combatEvent.TargetId
                    + " "
                    + combatEvent.SectionName
                    + " for "
                    + Mathf.RoundToInt(combatEvent.Damage);
                if (combatEvent.DestroyedTarget)
                {
                    line += " destroyed";
                }

                AddCombatLogLine(line);
                SpawnCombatEffect(combatEvent);
                Debug.Log("MC2 combat: " + line);
            }
        }

        private void SpawnCombatEffect(CombatEvent combatEvent)
        {
            if (combatEvent.Damage <= 0f
                || !TryGetCombatPoint(combatEvent.AttackerId, out Vector3 attackerPoint)
                || !TryGetCombatPoint(combatEvent.TargetId, out Vector3 targetPoint))
            {
                return;
            }

            Color weaponColor = combatEvent.DestroyedTarget
                ? new Color(1f, 0.42f, 0.08f, 0.85f)
                : WeaponColor(combatEvent.WeaponType);
            CreateWeaponTrace(attackerPoint, targetPoint, combatEvent, weaponColor);
            CreateImpact(targetPoint, weaponColor, combatEvent.DestroyedTarget, ImpactScale(combatEvent.WeaponType));
            PulseTarget(combatEvent.TargetId, new Color(1f, 0.9f, 0.52f), combatEvent.DestroyedTarget ? 0.28f : 0.18f);
        }

        private bool TryGetCombatPoint(string id, out Vector3 point)
        {
            if (unitViews.TryGetValue(id, out DemoUnitView unitView) && unitView != null && unitView.Unit != null)
            {
                point = unitView.transform.position + Vector3.up * 0.55f;
                return true;
            }

            if (structureViews.TryGetValue(id, out DemoStructureView structureView) && structureView != null && structureView.Structure != null)
            {
                point = structureView.transform.position + Vector3.up * 0.65f;
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        private void PulseTarget(string targetId, Color color, float duration)
        {
            if (unitViews.TryGetValue(targetId, out DemoUnitView unitView) && unitView != null)
            {
                unitView.PulseHit(color, duration);
                return;
            }

            if (structureViews.TryGetValue(targetId, out DemoStructureView structureView) && structureView != null)
            {
                structureView.PulseHit(color, duration);
            }
        }

        private static Color WeaponColor(string weaponType)
        {
            if (ContainsWeaponType(weaponType, "Energy"))
            {
                return new Color(0.32f, 0.88f, 1f, 0.82f);
            }

            if (ContainsWeaponType(weaponType, "Missile"))
            {
                return new Color(1f, 0.56f, 0.14f, 0.80f);
            }

            if (ContainsWeaponType(weaponType, "Ballistic"))
            {
                return new Color(1f, 0.92f, 0.68f, 0.76f);
            }

            return new Color(1f, 0.82f, 0.28f, 0.78f);
        }

        private static string WeaponLabel(string weaponType)
        {
            if (ContainsWeaponType(weaponType, "Energy"))
            {
                return "Energy";
            }

            if (ContainsWeaponType(weaponType, "Missile"))
            {
                return "Missile";
            }

            if (ContainsWeaponType(weaponType, "Ballistic"))
            {
                return "Ballistic";
            }

            return "Weapon";
        }

        private static float ImpactScale(string weaponType)
        {
            return ContainsWeaponType(weaponType, "Missile") ? 1.35f : 1f;
        }

        private static bool ContainsWeaponType(string weaponType, string value)
        {
            return !string.IsNullOrEmpty(weaponType)
                && weaponType.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void CreateWeaponTrace(Vector3 from, Vector3 to, CombatEvent combatEvent, Color color)
        {
            if (ContainsWeaponType(combatEvent.WeaponType, "Missile"))
            {
                Vector3 lift = Vector3.up * 0.35f;
                CreateBeam(from + lift, to + lift * 0.35f, color, 0.28f, 0.07f);
                CreateImpact(to + Vector3.up * 0.08f, new Color(0.12f, 0.12f, 0.12f, 0.42f), false, 0.65f);
                return;
            }

            if (ContainsWeaponType(combatEvent.WeaponType, "Ballistic"))
            {
                Vector3 side = Vector3.Cross((to - from).normalized, Vector3.up) * 0.08f;
                CreateBeam(from + side, to + side, color, 0.09f, 0.024f);
                CreateBeam(from - side, to - side, color, 0.09f, 0.018f);
                return;
            }

            if (ContainsWeaponType(combatEvent.WeaponType, "Energy"))
            {
                CreateBeam(from, to, color, 0.20f, 0.034f);
                return;
            }

            CreateBeam(from, to, color, 0.16f, 0.045f);
        }

        private void CreateBeam(Vector3 from, Vector3 to, Color color, float duration, float radius)
        {
            Vector3 direction = to - from;
            float length = direction.magnitude;
            if (length <= 0.01f)
            {
                return;
            }

            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = "Weapon Beam";
            beam.transform.position = (from + to) * 0.5f;
            beam.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            Collider beamCollider = beam.GetComponent<Collider>();
            if (beamCollider != null)
            {
                Destroy(beamCollider);
            }

            DemoTransientEffect transient = beam.AddComponent<DemoTransientEffect>();
            transient.Begin(color, duration, new Vector3(radius, length * 0.5f, radius), new Vector3(radius * 0.28f, length * 0.5f, radius * 0.28f));
        }

        private void CreateImpact(Vector3 position, Color color, bool destroyedTarget, float scale)
        {
            GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            impact.name = destroyedTarget ? "Kill Impact" : "Hit Impact";
            impact.transform.position = position;
            Collider impactCollider = impact.GetComponent<Collider>();
            if (impactCollider != null)
            {
                Destroy(impactCollider);
            }

            float duration = destroyedTarget ? 0.36f : 0.22f;
            Vector3 fromScale = Vector3.one * (destroyedTarget ? 0.24f : 0.14f) * scale;
            Vector3 toScale = Vector3.one * (destroyedTarget ? 1.25f : 0.72f) * scale;
            DemoTransientEffect transient = impact.AddComponent<DemoTransientEffect>();
            transient.Begin(color, duration, fromScale, toScale);
        }

        private void CaptureObjectiveEvents()
        {
            foreach (ObjectiveState objective in mission.Objectives)
            {
                int objectiveIndex = objective.Definition.index;
                if (!objectiveCompletionState.TryGetValue(objectiveIndex, out bool wasComplete))
                {
                    objectiveCompletionState[objectiveIndex] = objective.IsComplete;
                    continue;
                }

                if (!wasComplete && objective.IsComplete)
                {
                    objectiveCompletionState[objectiveIndex] = true;
                    if (!objective.Definition.hidden)
                    {
                        AddCombatLogLine("Objective done: " + objective.Definition.title);
                        Debug.Log("MC2 objective complete: " + objective.Definition.title);
                    }
                }
            }
        }

        private void CaptureUnitActivationEvents()
        {
            foreach (UnitActivationEvent activationEvent in mission.RecentUnitActivationEvents)
            {
                string line = "Contact: " + activationEvent.UnitType;
                AddCombatLogLine(line);
                Debug.Log("MC2 enemy activated: " + activationEvent.UnitId + " " + activationEvent.UnitType + " " + activationEvent.Brain);
            }
        }

        private void CaptureMissionResult()
        {
            if (mission.Result == lastMissionResult)
            {
                return;
            }

            lastMissionResult = mission.Result;
            if (mission.Result == MissionResultState.Victory)
            {
                statusText = "Mission complete";
                AddCombatLogLine("Mission complete");
                Debug.Log("MC2 mission complete: " + mission.ResultReason);
                SetPaused(true);
            }
            else if (mission.Result == MissionResultState.Defeat)
            {
                statusText = "Mission failed";
                AddCombatLogLine("Mission failed");
                Debug.Log("MC2 mission failed: " + mission.ResultReason);
                SetPaused(true);
            }
        }

        private void AddCombatLogLine(string line)
        {
            combatLog.Insert(0, line);
            while (combatLog.Count > 6)
            {
                combatLog.RemoveAt(combatLog.Count - 1);
            }
        }

        private void CreateGround()
        {
            if (mission.Contract.terrainMesh == null || mission.Contract.terrainMesh.samples == null)
            {
                GameObject fallbackGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
                fallbackGround.name = "Mission Ground";
                fallbackGround.transform.localScale = new Vector3(140f, 1f, 140f);
                AssignMaterial(fallbackGround, "Ground", new Color(0.16f, 0.20f, 0.16f));
                return;
            }

            GameObject terrainObject = new("Mission Terrain");
            MeshFilter meshFilter = terrainObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = terrainObject.AddComponent<MeshRenderer>();
            DemoTerrainView terrainView = terrainObject.AddComponent<DemoTerrainView>();
            terrainView.Bind(mission.Contract.terrainMesh, mission.Contract.mission.terrain.waterElevation);

            meshFilter.sharedMesh = BuildTerrainMesh(mission.Contract.terrainMesh);
            meshRenderer.sharedMaterial = MakeMaterial("GroundMesh", new Color(0.16f, 0.22f, 0.15f));

            CreateWaterPlane(mission.Contract.terrainMesh);
        }

        private Mesh BuildTerrainMesh(TerrainMeshDefinition terrain)
        {
            int side = terrain.sampleSide;
            int sampleCount = terrain.samples.Length;
            Vector3[] vertices = new Vector3[sampleCount];
            Color[] colors = new Color[sampleCount];
            Vector2[] uvs = new Vector2[sampleCount];
            float spacing = Mathf.Max(1f, terrain.worldUnitsPerVertex * Mathf.Max(1, terrain.sampleStep));

            for (int row = 0; row < side; row++)
            {
                for (int col = 0; col < side; col++)
                {
                    int index = row * side + col;
                    TerrainMeshSample sample = terrain.samples[index];
                    float missionX = terrain.minX + col * spacing;
                    float missionY = terrain.minY - row * spacing;
                    Vector3 worldPosition = DemoTerrainView.MissionToWorld(new Vector2(missionX, missionY));
                    worldPosition.y = DemoTerrainView.ElevationToWorldHeight(sample.elevation, mission.Contract.mission.terrain.waterElevation);
                    vertices[index] = worldPosition;
                    colors[index] = TerrainVertexColor(sample);
                    uvs[index] = new Vector2(col / (float)(side - 1), row / (float)(side - 1));
                }
            }

            int[] triangles = new int[(side - 1) * (side - 1) * 6];
            int triangleIndex = 0;
            for (int row = 0; row < side - 1; row++)
            {
                for (int col = 0; col < side - 1; col++)
                {
                    int topLeft = row * side + col;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + side;
                    int bottomRight = bottomLeft + 1;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = bottomRight;
                }
            }

            Mesh mesh = new()
            {
                name = "MC2 Source Terrain",
                vertices = vertices,
                triangles = triangles,
                colors = colors,
                uv = uvs
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void CreateWaterPlane(TerrainMeshDefinition terrain)
        {
            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = "Mission Water";
            float sourceSize = (terrain.sampleSide - 1) * terrain.worldUnitsPerVertex / 1000f;
            water.transform.localScale = new Vector3(sourceSize, 1f, sourceSize);
            Vector3 center = DemoTerrainView.MissionToWorld(new Vector2(0f, 0f));
            center.y = DemoTerrainView.WaterHeight() + 0.015f;
            water.transform.position = center;
            AssignMaterial(water, "Water", new Color(0.08f, 0.24f, 0.34f, 0.52f));
        }

        private Color TerrainVertexColor(TerrainMeshSample sample)
        {
            float waterLevel = mission.Contract.mission.terrain.waterElevation;
            if (sample.elevation <= waterLevel + 4f)
            {
                return new Color(0.10f, 0.28f, 0.22f);
            }

            if (sample.terrainType == 13 || sample.terrainType == 14 || sample.terrainType == 15 || sample.terrainType == 16)
            {
                return new Color(0.32f, 0.29f, 0.22f);
            }

            float heightT = Mathf.InverseLerp(mission.Contract.terrainMesh.elevationMin, mission.Contract.terrainMesh.elevationMax, sample.elevation);
            return Color.Lerp(new Color(0.13f, 0.24f, 0.12f), new Color(0.36f, 0.36f, 0.25f), heightT);
        }

        private void CreateLights()
        {
            GameObject lightObject = new("Key Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(55f, 30f, 0f);
        }

        private void CreateCamera()
        {
            GameObject cameraObject = new("Demo Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 42f;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.06f, 0.07f, 0.075f);
            Camera.SetupCurrent(mainCamera);
        }

        private void CreateUnits()
        {
            foreach (UnitState unit in mission.Units)
            {
                PrimitiveType primitive = unit.IsPlayerUnit ? PrimitiveType.Capsule : PrimitiveType.Cube;
                GameObject unitObject = GameObject.CreatePrimitive(primitive);
                unitObject.transform.localScale = unit.IsPlayerUnit
                    ? new Vector3(1.3f, 1.5f, 1.3f)
                    : new Vector3(1.2f, 0.7f, 1.2f);
                AssignMaterial(
                    unitObject,
                    unit.Id,
                    unit.IsPlayerUnit ? new Color(0.1f, 0.65f, 0.86f) : new Color(0.74f, 0.20f, 0.18f));

                DemoUnitView view = unitObject.AddComponent<DemoUnitView>();
                view.Bind(unit);
                unitObject.SetActive(unit.IsActive || unit.IsPlayerUnit || unit.IsDestroyed);
                unitViews[unit.Id] = view;
            }
        }

        private void CreateStaticObjects()
        {
            foreach (StructureState structure in mission.Structures)
            {
                GameObject structureObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                structureObject.name = structure.Id + " " + structure.ObjectType;
                structureObject.transform.localScale = StructureScale(structure);
                AssignMaterial(
                    structureObject,
                    structure.Id,
                    structure.IsTargetable ? new Color(0.62f, 0.48f, 0.34f) : new Color(0.32f, 0.34f, 0.34f));

                DemoStructureView view = structureObject.AddComponent<DemoStructureView>();
                view.Bind(structure);
                structureViews[structure.Id] = view;
            }
        }

        private Vector3 StructureScale(StructureState structure)
        {
            if (structure.ObjectType == "Hangar")
            {
                return new Vector3(3.8f, 1.3f, 2.4f);
            }

            float footprint = Mathf.Clamp(structure.Radius / 80f, 1.4f, 3.2f);
            return new Vector3(footprint, 1f, footprint);
        }

        private void CreateTerrainObjects()
        {
            if (mission.Contract.terrainObjects == null)
            {
                return;
            }

            foreach (TerrainObjectSpawn terrainObject in mission.Contract.terrainObjects)
            {
                if (terrainObject.position == null || IsCoveredByTargetStructure(terrainObject))
                {
                    continue;
                }

                PrimitiveType primitive = TerrainPrimitive(terrainObject);
                GameObject prop = GameObject.CreatePrimitive(primitive);
                prop.name = terrainObject.objectId + " " + terrainObject.fileName;
                Vector3 scale = TerrainObjectScale(terrainObject);
                Vector3 position = DemoUnitView.MissionToWorld(new Vector2(terrainObject.position.x, terrainObject.position.y));
                Vector2 missionPosition = new(terrainObject.position.x, terrainObject.position.y);
                position.y = DemoTerrainView.HeightAt(missionPosition) + Mathf.Max(0.03f, scale.y * 0.5f);
                prop.transform.position = position;
                prop.transform.rotation = Quaternion.Euler(0f, -terrainObject.position.rotation, 0f);
                prop.transform.localScale = scale;

                Collider collider = prop.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }

                AssignMaterial(prop, TerrainMaterialName(terrainObject), TerrainObjectColor(terrainObject));
            }
        }

        private bool IsCoveredByTargetStructure(TerrainObjectSpawn terrainObject)
        {
            Vector2 objectPosition = new(terrainObject.position.x, terrainObject.position.y);
            foreach (StructureState structure in mission.Structures)
            {
                if (Vector2.Distance(objectPosition, structure.MissionPosition) < 40f)
                {
                    return true;
                }
            }

            return false;
        }

        private static PrimitiveType TerrainPrimitive(TerrainObjectSpawn terrainObject)
        {
            if (string.Equals(terrainObject.objectClass, "TREE", StringComparison.OrdinalIgnoreCase))
            {
                return PrimitiveType.Capsule;
            }

            if (string.Equals(terrainObject.objectClass, "BUILDING", StringComparison.OrdinalIgnoreCase))
            {
                return PrimitiveType.Cube;
            }

            return PrimitiveType.Cylinder;
        }

        private static Vector3 TerrainObjectScale(TerrainObjectSpawn terrainObject)
        {
            if (string.Equals(terrainObject.objectClass, "TREE", StringComparison.OrdinalIgnoreCase))
            {
                if (ContainsIgnoreCase(terrainObject.fileName, "Light"))
                {
                    return new Vector3(0.12f, 0.7f, 0.12f);
                }

                return new Vector3(0.36f, 1.15f, 0.36f);
            }

            if (string.Equals(terrainObject.objectClass, "BUILDING", StringComparison.OrdinalIgnoreCase))
            {
                if (terrainObject.fitId == 559 || ContainsIgnoreCase(terrainObject.fileName, "Hangar"))
                {
                    return new Vector3(3.8f, 1.2f, 2.4f);
                }

                return new Vector3(1.25f, 0.7f, 1.25f);
            }

            return new Vector3(0.45f, 0.35f, 0.45f);
        }

        private static string TerrainMaterialName(TerrainObjectSpawn terrainObject)
        {
            if (terrainObject.damage != 0)
            {
                return "TerrainDamaged";
            }

            if (string.Equals(terrainObject.objectClass, "TREE", StringComparison.OrdinalIgnoreCase))
            {
                return "TerrainTree";
            }

            if (string.Equals(terrainObject.objectClass, "BUILDING", StringComparison.OrdinalIgnoreCase))
            {
                return terrainObject.teamId >= 0 ? "TerrainTeamBuilding" : "TerrainBuilding";
            }

            return "TerrainProp";
        }

        private static Color TerrainObjectColor(TerrainObjectSpawn terrainObject)
        {
            if (terrainObject.damage != 0)
            {
                return new Color(0.30f, 0.27f, 0.22f);
            }

            if (string.Equals(terrainObject.objectClass, "TREE", StringComparison.OrdinalIgnoreCase))
            {
                if (ContainsIgnoreCase(terrainObject.fileName, "Light"))
                {
                    return new Color(0.82f, 0.73f, 0.46f);
                }

                return new Color(0.13f, 0.34f, 0.17f);
            }

            if (string.Equals(terrainObject.objectClass, "BUILDING", StringComparison.OrdinalIgnoreCase))
            {
                return terrainObject.teamId >= 0
                    ? new Color(0.55f, 0.44f, 0.33f)
                    : new Color(0.33f, 0.35f, 0.34f);
            }

            return new Color(0.42f, 0.42f, 0.38f);
        }

        private static bool ContainsIgnoreCase(string value, string fragment)
        {
            return !string.IsNullOrEmpty(value)
                && value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void CreateMarkers()
        {
            if (mission.Contract.navMarkers == null)
            {
                return;
            }

            foreach (NavMarker marker in mission.Contract.navMarkers)
            {
                GameObject markerObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                markerObject.name = "Nav Marker " + marker.index;
                Vector3 markerPosition = DemoUnitView.MissionToWorld(new Vector2(marker.x, marker.y));
                markerPosition.y = 0.04f;
                markerObject.transform.position = markerPosition;
                markerObject.transform.localScale = new Vector3(marker.radius / 100f, 0.04f, marker.radius / 100f);
                AssignMaterial(markerObject, "Marker" + marker.index, new Color(0.95f, 0.76f, 0.22f, 0.48f));
            }
        }

        private void CreateForests()
        {
            if (mission.Contract.forests == null)
            {
                return;
            }

            foreach (ForestRegion forest in mission.Contract.forests)
            {
                if (forest.center == null || forest.radius <= 0f)
                {
                    continue;
                }

                Vector2 missionCenter = new(forest.center.x, forest.center.y);
                float worldRadius = Mathf.Max(1.8f, forest.radius / 100f);
                GameObject root = new("Forest " + forest.index + " " + forest.name);
                root.transform.position = DemoUnitView.MissionToWorld(missionCenter);

                GameObject groundPatch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                groundPatch.name = "Forest Canopy Footprint";
                groundPatch.transform.SetParent(root.transform, false);
                groundPatch.transform.localPosition = new Vector3(0f, 0.025f, 0f);
                groundPatch.transform.localScale = new Vector3(worldRadius, 0.02f, worldRadius);
                AssignMaterial(groundPatch, "ForestFootprint" + forest.index, new Color(0.09f, 0.24f, 0.12f, 0.45f));

                int treeCount = Mathf.Clamp(Mathf.RoundToInt(worldRadius * 1.4f), 8, 18);
                for (int index = 0; index < treeCount; index++)
                {
                    float angle = (forest.index * 43f + index * 137.5f) * Mathf.Deg2Rad;
                    float ring = 0.18f + (((index * 37 + forest.index * 11) % 100) / 100f) * 0.72f;
                    Vector3 localPosition = new(
                        Mathf.Cos(angle) * worldRadius * ring,
                        0f,
                        Mathf.Sin(angle) * worldRadius * ring);
                    CreateTree(root.transform, forest.index, index, localPosition);
                }
            }
        }

        private void CreateTree(Transform parent, int forestIndex, int treeIndex, Vector3 localPosition)
        {
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Tree Trunk";
            trunk.transform.SetParent(parent, false);
            trunk.transform.localPosition = localPosition + new Vector3(0f, 0.22f, 0f);
            trunk.transform.localScale = new Vector3(0.08f, 0.22f, 0.08f);
            AssignMaterial(trunk, "TreeTrunk" + forestIndex + "-" + treeIndex, new Color(0.32f, 0.22f, 0.12f));

            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Tree Canopy";
            canopy.transform.SetParent(parent, false);
            canopy.transform.localPosition = localPosition + new Vector3(0f, 0.56f, 0f);
            float canopyScale = 0.34f + ((treeIndex + forestIndex) % 4) * 0.04f;
            canopy.transform.localScale = new Vector3(canopyScale, canopyScale * 0.82f, canopyScale);
            AssignMaterial(canopy, "TreeCanopy" + forestIndex + "-" + treeIndex, new Color(0.12f, 0.36f, 0.17f));
        }

        private void CreateObjectiveAreas()
        {
            HashSet<string> renderedAreas = new();
            foreach (ObjectiveState objective in mission.Objectives)
            {
                foreach (ObjectiveCondition condition in objective.Definition.conditions)
                {
                    TargetArea area = condition.targetArea;
                    if (area == null || area.radius <= 0f)
                    {
                        continue;
                    }

                    string key = Mathf.RoundToInt(area.x) + ":" + Mathf.RoundToInt(area.y) + ":" + Mathf.RoundToInt(area.radius);
                    if (!renderedAreas.Add(key))
                    {
                        continue;
                    }

                    CreateAreaDisc(
                        "Objective Area " + objective.Definition.index,
                        new Vector2(area.x, area.y),
                        area.radius,
                        objective.Definition.hidden ? new Color(0.34f, 0.42f, 0.52f, 0.32f) : new Color(0.15f, 0.55f, 0.78f, 0.34f));
                }
            }
        }

        private void CreateAreaDisc(string objectName, Vector2 missionCenter, float missionRadius, Color color)
        {
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = objectName;
            Vector3 position = DemoUnitView.MissionToWorld(missionCenter);
            position.y = 0.035f;
            disc.transform.position = position;
            float worldRadius = Mathf.Max(0.6f, missionRadius / 100f);
            disc.transform.localScale = new Vector3(worldRadius, 0.018f, worldRadius);
            AssignMaterial(disc, objectName, color);
        }

        private void CreateCommandOverlays()
        {
            foreach (UnitState unit in mission.PlayerUnits())
            {
                unitSelectionMarkers[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Selection Ring",
                    "CommandSelection",
                    new Color(0.22f, 0.92f, 1f, 0.32f),
                    new Vector3(1.45f, 0.012f, 1.45f));
                unitOrderMarkers[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Order Marker",
                    "CommandMove",
                    new Color(0.15f, 0.66f, 1f, 0.42f),
                    new Vector3(0.88f, 0.014f, 0.88f));
                unitFocusMarkers[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Focus Marker",
                    "CommandFocus",
                    new Color(1f, 0.58f, 0.12f, 0.48f),
                    new Vector3(1.18f, 0.016f, 1.18f));
                unitRangeMarkers[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Weapon Range",
                    "CommandWeaponRange",
                    new Color(0.25f, 0.82f, 1f, 0.12f),
                    Vector3.one);
                unitTargetLines[unit.Id] = CreateTargetLine(unit.Id + " Target Line");
            }
        }

        private GameObject CreateMarkerDisc(string objectName, string materialName, Color color, Vector3 scale)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = objectName;
            marker.transform.localScale = scale;
            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
            {
                markerCollider.enabled = false;
            }

            AssignMaterial(marker, materialName, color);
            marker.SetActive(false);
            return marker;
        }

        private void UpdateCommandOverlays()
        {
            if (mission == null)
            {
                return;
            }

            foreach (UnitState unit in mission.PlayerUnits())
            {
                UpdateSelectionMarker(unit);
                UpdateOrderMarker(unit);
                UpdateFocusMarker(unit);
                UpdateRangeMarker(unit);
                UpdateTargetLine(unit);
            }

            UpdateTargetHealthBars();
        }

        private void UpdateUnitVisibility()
        {
            foreach (UnitState unit in mission.Units)
            {
                if (!unitViews.TryGetValue(unit.Id, out DemoUnitView view) || view == null)
                {
                    continue;
                }

                view.gameObject.SetActive(unit.IsActive || unit.IsPlayerUnit || unit.IsDestroyed);
            }
        }

        private void UpdateSelectionMarker(UnitState unit)
        {
            if (!unitSelectionMarkers.TryGetValue(unit.Id, out GameObject marker))
            {
                return;
            }

            bool isVisible = !unit.IsDestroyed && (unit.IsDetached || pendingDetachedUnitId == unit.Id);
            marker.SetActive(isVisible);
            if (isVisible)
            {
                marker.transform.position = GroundMarkerPosition(unit.MissionPosition, 0.06f);
            }
        }

        private void UpdateOrderMarker(UnitState unit)
        {
            if (!unitOrderMarkers.TryGetValue(unit.Id, out GameObject marker))
            {
                return;
            }

            bool isVisible = !unit.IsDestroyed && (unit.HasMoveOrder || unit.IsJumping || unit.HasAttackOrder);
            marker.SetActive(isVisible);
            if (isVisible)
            {
                marker.transform.position = GroundMarkerPosition(unit.MoveTarget, 0.08f);
            }
        }

        private void UpdateFocusMarker(UnitState unit)
        {
            if (!unitFocusMarkers.TryGetValue(unit.Id, out GameObject marker))
            {
                return;
            }

            Vector2 position = default;
            float radius = 0f;
            bool isVisible = !unit.IsDestroyed
                && unit.HasAttackOrder
                && TryGetTargetMarker(unit.AttackTargetId, out position, out radius);
            marker.SetActive(isVisible);
            if (isVisible)
            {
                marker.transform.position = GroundMarkerPosition(position, 0.1f);
                float scale = Mathf.Clamp(radius / 95f, 1.1f, 4.2f);
                marker.transform.localScale = new Vector3(scale, 0.016f, scale);
            }
        }

        private void UpdateRangeMarker(UnitState unit)
        {
            if (!unitRangeMarkers.TryGetValue(unit.Id, out GameObject marker))
            {
                return;
            }

            bool isVisible = !unit.IsDestroyed
                && unit.Profile.WeaponRange > 0f
                && (pendingDetachedUnitId == unit.Id
                    || unit.IsDetached
                    || unit.HasAttackOrder
                    || !string.IsNullOrEmpty(unit.CurrentTargetId));
            marker.SetActive(isVisible);
            if (isVisible)
            {
                marker.transform.position = GroundMarkerPosition(unit.MissionPosition, 0.045f);
                float worldDiameter = Mathf.Clamp((unit.Profile.WeaponRange / 100f) * 2f, 1.2f, 8f);
                marker.transform.localScale = new Vector3(worldDiameter, 0.01f, worldDiameter);
            }
        }

        private void UpdateTargetLine(UnitState unit)
        {
            if (!unitTargetLines.TryGetValue(unit.Id, out GameObject line))
            {
                return;
            }

            string targetId = string.IsNullOrEmpty(unit.AttackTargetId) ? unit.CurrentTargetId : unit.AttackTargetId;
            bool hasTargetId = !string.IsNullOrEmpty(targetId);
            Vector3 targetPoint = Vector3.zero;
            bool hasTargetPoint = hasTargetId && TryGetTargetWorldPoint(targetId, out targetPoint);
            bool hasUnitView = unitViews.TryGetValue(unit.Id, out DemoUnitView unitView) && unitView != null;
            bool isVisible = !unit.IsDestroyed
                && hasTargetId
                && hasTargetPoint
                && hasUnitView;
            line.SetActive(isVisible);
            if (!isVisible)
            {
                return;
            }

            Vector3 unitPoint = unitView.transform.position + Vector3.up * 0.18f;
            targetPoint += Vector3.up * 0.18f;
            PositionLine(line, unitPoint, targetPoint, 0.028f);
            AssignMaterial(line, TargetLineMaterialName(unit, targetId), TargetLineColor(unit, targetId));
        }

        private GameObject CreateTargetLine(string objectName)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            line.name = objectName;
            Collider lineCollider = line.GetComponent<Collider>();
            if (lineCollider != null)
            {
                lineCollider.enabled = false;
            }

            AssignMaterial(line, "TargetLineReady", new Color(0.28f, 0.78f, 1f, 0.48f));
            line.SetActive(false);
            return line;
        }

        private void PositionLine(GameObject line, Vector3 from, Vector3 to, float radius)
        {
            Vector3 direction = to - from;
            float length = direction.magnitude;
            if (length <= 0.01f)
            {
                line.SetActive(false);
                return;
            }

            line.transform.position = (from + to) * 0.5f;
            line.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            line.transform.localScale = new Vector3(radius, length * 0.5f, radius);
        }

        private bool TryGetTargetWorldPoint(string targetId, out Vector3 point)
        {
            if (unitViews.TryGetValue(targetId, out DemoUnitView unitView)
                && unitView != null
                && unitView.Unit != null
                && unitView.Unit.IsActive
                && !unitView.Unit.IsDestroyed)
            {
                point = unitView.transform.position;
                return true;
            }

            if (structureViews.TryGetValue(targetId, out DemoStructureView structureView) && structureView != null && structureView.Structure != null && !structureView.Structure.IsDestroyed)
            {
                point = structureView.transform.position;
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        private string TargetLineMaterialName(UnitState unit, string targetId)
        {
            if (unit.IsHeatLocked || !IsTargetInWeaponRange(unit, targetId))
            {
                return "TargetLineBlocked";
            }

            if (unit.IsWeaponCoolingDown)
            {
                return "TargetLineCooling";
            }

            return "TargetLineReady";
        }

        private Color TargetLineColor(UnitState unit, string targetId)
        {
            if (unit.IsHeatLocked || !IsTargetInWeaponRange(unit, targetId))
            {
                return new Color(1f, 0.18f, 0.12f, 0.50f);
            }

            if (unit.IsWeaponCoolingDown)
            {
                return new Color(1f, 0.62f, 0.14f, 0.46f);
            }

            return new Color(0.28f, 0.78f, 1f, 0.48f);
        }

        private bool IsTargetInWeaponRange(UnitState unit, string targetId)
        {
            UnitState targetUnit = mission.FindUnit(targetId);
            if (targetUnit != null)
            {
                return unit.IsInWeaponRange(targetUnit);
            }

            StructureState targetStructure = mission.FindStructure(targetId);
            return targetStructure != null && unit.IsInWeaponRange(targetStructure);
        }

        private void UpdateTargetHealthBars()
        {
            foreach (UnitState unit in mission.Units)
            {
                if (unit.IsPlayerUnit)
                {
                    continue;
                }

                EnsureUnitHealthBar(unit);
                bool isVisible = unit.IsActive && !unit.IsDestroyed && (unit.Structure < 0.995f || IsPlayerTargeting(unit.Id));
                UpdateHealthBar(
                    unitHealthBarBacks[unit.Id],
                    unitHealthBarFills[unit.Id],
                    isVisible,
                    UnitHealthBarPosition(unit),
                    1.35f,
                    unit.Structure);
            }

            foreach (StructureState structure in mission.Structures)
            {
                if (!structure.IsTargetable)
                {
                    continue;
                }

                EnsureStructureHealthBar(structure);
                bool isVisible = !structure.IsDestroyed && (structure.Structure < 0.995f || IsPlayerTargeting(structure.Id));
                UpdateHealthBar(
                    structureHealthBarBacks[structure.Id],
                    structureHealthBarFills[structure.Id],
                    isVisible,
                    StructureHealthBarPosition(structure),
                    Mathf.Clamp(structure.Radius / 70f, 1.65f, 3.3f),
                    structure.Structure);
            }
        }

        private void EnsureUnitHealthBar(UnitState unit)
        {
            if (unitHealthBarBacks.ContainsKey(unit.Id))
            {
                return;
            }

            unitHealthBarBacks[unit.Id] = CreateHealthBarPart(unit.Id + " Health Back", "TargetHealthBack", new Color(0.06f, 0.06f, 0.06f, 0.82f));
            unitHealthBarFills[unit.Id] = CreateHealthBarPart(unit.Id + " Health Fill", "TargetHealthFill", new Color(0.24f, 0.88f, 0.28f, 0.92f));
        }

        private void EnsureStructureHealthBar(StructureState structure)
        {
            if (structureHealthBarBacks.ContainsKey(structure.Id))
            {
                return;
            }

            structureHealthBarBacks[structure.Id] = CreateHealthBarPart(structure.Id + " Health Back", "TargetHealthBack", new Color(0.06f, 0.06f, 0.06f, 0.82f));
            structureHealthBarFills[structure.Id] = CreateHealthBarPart(structure.Id + " Health Fill", "TargetHealthFill", new Color(0.24f, 0.88f, 0.28f, 0.92f));
        }

        private GameObject CreateHealthBarPart(string objectName, string materialName, Color color)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = objectName;
            Collider partCollider = part.GetComponent<Collider>();
            if (partCollider != null)
            {
                partCollider.enabled = false;
            }

            AssignMaterial(part, materialName, color);
            part.SetActive(false);
            return part;
        }

        private void UpdateHealthBar(GameObject back, GameObject fill, bool isVisible, Vector3 center, float width, float ratio)
        {
            back.SetActive(isVisible);
            fill.SetActive(isVisible);
            if (!isVisible)
            {
                return;
            }

            float clampedRatio = Mathf.Clamp01(ratio);
            back.transform.position = center;
            back.transform.localScale = new Vector3(width, 0.055f, 0.12f);

            float fillWidth = Mathf.Max(0.03f, width * clampedRatio);
            fill.transform.position = center + Vector3.right * ((fillWidth - width) * 0.5f);
            fill.transform.localScale = new Vector3(fillWidth, 0.062f, 0.13f);
            AssignMaterial(fill, HealthFillMaterialName(clampedRatio), HealthFillColor(clampedRatio));
        }

        private Vector3 UnitHealthBarPosition(UnitState unit)
        {
            if (unitViews.TryGetValue(unit.Id, out DemoUnitView view) && view != null)
            {
                return view.transform.position + Vector3.up * 1.15f;
            }

            return GroundMarkerPosition(unit.MissionPosition, 1.15f);
        }

        private Vector3 StructureHealthBarPosition(StructureState structure)
        {
            if (structureViews.TryGetValue(structure.Id, out DemoStructureView view) && view != null)
            {
                return view.transform.position + Vector3.up * 1.05f;
            }

            return GroundMarkerPosition(structure.MissionPosition, 1.05f);
        }

        private bool IsPlayerTargeting(string targetId)
        {
            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit.AttackTargetId == targetId || unit.CurrentTargetId == targetId)
                {
                    return true;
                }
            }

            return false;
        }

        private static string HealthFillMaterialName(float ratio)
        {
            if (ratio <= 0.33f)
            {
                return "TargetHealthCritical";
            }

            if (ratio <= 0.66f)
            {
                return "TargetHealthDamaged";
            }

            return "TargetHealthFill";
        }

        private static Color HealthFillColor(float ratio)
        {
            if (ratio <= 0.33f)
            {
                return new Color(0.95f, 0.16f, 0.12f, 0.94f);
            }

            if (ratio <= 0.66f)
            {
                return new Color(1f, 0.72f, 0.14f, 0.94f);
            }

            return new Color(0.24f, 0.88f, 0.28f, 0.92f);
        }

        private bool TryGetTargetMarker(string targetId, out Vector2 missionPosition, out float radius)
        {
            UnitState unit = mission.FindUnit(targetId);
            if (unit != null && !unit.IsDestroyed)
            {
                missionPosition = unit.MissionPosition;
                radius = 115f;
                return true;
            }

            StructureState structure = mission.FindStructure(targetId);
            if (structure != null && !structure.IsDestroyed)
            {
                missionPosition = structure.MissionPosition;
                radius = structure.Radius;
                return true;
            }

            missionPosition = default;
            radius = 0f;
            return false;
        }

        private static Vector3 GroundMarkerPosition(Vector2 missionPoint, float lift)
        {
            Vector3 position = DemoUnitView.MissionToWorld(missionPoint);
            position.y = DemoTerrainView.HeightAt(missionPoint) + lift;
            return position;
        }

        private void AssignMaterial(GameObject target, string materialName, Color color)
        {
            Material material = MakeMaterial(materialName, color);
            if (material == null)
            {
                return;
            }

            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private Material MakeMaterial(string materialName, Color color)
        {
            string cacheKey = materialName + ":" + ColorUtility.ToHtmlStringRGBA(color);
            if (materialCache.TryGetValue(cacheKey, out Material cachedMaterial))
            {
                return cachedMaterial;
            }

            Shader shader = Shader.Find("Standard") ?? Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Debug.LogWarning("No shader available for material " + materialName + "; skipping custom material.");
                return null;
            }

            Material material = new(shader)
            {
                name = materialName,
                color = color
            };
            if (color.a < 0.99f)
            {
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }

            ownedMaterials.Add(material);
            materialCache[cacheKey] = material;
            return material;
        }

        private void HandleWorldClick()
        {
            if (!Input.GetMouseButtonDown(0) || mainCamera == null)
            {
                return;
            }

            Vector2 guiPoint = new(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            if (IsGuiPointBlocked(guiPoint))
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
                DemoUnitView unitView = hit.collider.GetComponentInParent<DemoUnitView>();
                if (unitView != null && unitView.Unit != null && unitView.Unit.IsActive && !unitView.Unit.IsPlayerUnit && !unitView.Unit.IsDestroyed)
                {
                    if (pendingJumpOrder)
                    {
                        IssueMoveOrder(unitView.Unit.MissionPosition, "Jet toward " + unitView.Unit.UnitType);
                    }
                    else
                    {
                        IssueUnitAttackOrder(unitView.Unit);
                    }

                    return;
                }

                DemoStructureView structureView = hit.collider.GetComponentInParent<DemoStructureView>();
                if (structureView != null && structureView.Structure != null && !structureView.Structure.IsDestroyed)
                {
                    if (pendingJumpOrder)
                    {
                        IssueMoveOrder(structureView.Structure.MissionPosition, "Jet toward " + structureView.Structure.ObjectType);
                    }
                    else
                    {
                        IssueStructureAttackOrder(structureView.Structure);
                    }

                    return;
                }
            }

            Plane groundPlane = new(Vector3.up, Vector3.zero);
            if (!groundPlane.Raycast(ray, out float distance))
            {
                return;
            }

            Vector2 target = DemoUnitView.WorldToMission(ray.GetPoint(distance));
            IssueMoveOrder(target, null);
        }

        private void IssueMoveOrder(Vector2 target, string label)
        {
            if (pendingJumpOrder)
            {
                IssueJumpOrder(target, label);
                return;
            }

            if (!string.IsNullOrEmpty(pendingDetachedUnitId))
            {
                mission.IssueDetachedMove(pendingDetachedUnitId, target);
                statusText = string.IsNullOrEmpty(label) ? "Detached order: " + pendingDetachedUnitId : label + " with " + pendingDetachedUnitId;
                pendingDetachedUnitId = null;
            }
            else
            {
                mission.IssueSquadMove(target);
                statusText = string.IsNullOrEmpty(label) ? "Squad move" : label;
            }
        }

        private void IssueUnitAttackOrder(UnitState target)
        {
            int accepted;
            string detachedUnitId = pendingDetachedUnitId;
            if (!string.IsNullOrEmpty(detachedUnitId))
            {
                accepted = mission.IssueDetachedAttackUnit(detachedUnitId, target.Id);
                statusText = accepted > 0 ? "Focus " + target.UnitType + " with " + detachedUnitId : "Attack blocked";
            }
            else
            {
                accepted = mission.IssueSquadAttackUnit(target.Id);
                statusText = accepted > 0 ? "Squad focus: " + target.UnitType : "Attack blocked";
            }

            pendingDetachedUnitId = null;
        }

        private void IssueStructureAttackOrder(StructureState target)
        {
            int accepted;
            string detachedUnitId = pendingDetachedUnitId;
            if (!string.IsNullOrEmpty(detachedUnitId))
            {
                accepted = mission.IssueDetachedAttackStructure(detachedUnitId, target.Id);
                statusText = accepted > 0 ? "Attack " + target.ObjectType + " with " + detachedUnitId : "Attack blocked";
            }
            else
            {
                accepted = mission.IssueSquadAttackStructure(target.Id);
                statusText = accepted > 0 ? "Squad attack: " + target.ObjectType : "Attack blocked";
            }

            pendingDetachedUnitId = null;
        }

        private void IssueJumpOrder(Vector2 target, string label)
        {
            int accepted;
            string detachedUnitId = pendingDetachedUnitId;
            if (!string.IsNullOrEmpty(detachedUnitId))
            {
                accepted = mission.IssueDetachedJump(detachedUnitId, target, JumpDistance, DemoTerrainView.IsUsableLandingPosition);
                statusText = accepted > 0
                    ? (string.IsNullOrEmpty(label) ? "Jet order: " + detachedUnitId : "Jet vector: " + detachedUnitId)
                    : "Jet blocked: " + detachedUnitId;
            }
            else
            {
                accepted = mission.IssueSquadJump(target, JumpDistance, DemoTerrainView.IsUsableLandingPosition);
                statusText = accepted > 0
                    ? (string.IsNullOrEmpty(label) ? "Squad jet: " + accepted : "Squad jet vector: " + accepted)
                    : "Jet blocked: no legal landing";
            }

            pendingDetachedUnitId = null;
            pendingJumpOrder = false;
        }

        private void FollowCommander()
        {
            UnitState commander = null;
            foreach (UnitState unit in mission.PlayerUnits())
            {
                commander = unit;
                break;
            }

            if (commander == null || mainCamera == null)
            {
                return;
            }

            Vector3 commanderWorld = DemoUnitView.MissionToWorld(commander.MissionPosition);
            Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -cameraHeight);
            mainCamera.transform.SetPositionAndRotation(commanderWorld + offset, rotation);
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(12, 12, 330, 34), statusText);

            if (mission == null)
            {
                return;
            }

            DrawUnitPanel();
            DrawCombatPanel();
            DrawMissionBriefPanel();
            DrawMissionMap();
            DrawSystemPanel();
            DrawMissionResultPanel();
        }

        private void DrawUnitPanel()
        {
            float y = 54f;
            GUI.Label(new Rect(18, y, 320, 22), "Lance");
            y += 24f;

            if (GUI.Button(new Rect(18, y, 70, 28), "All"))
            {
                pendingDetachedUnitId = null;
                pendingJumpOrder = false;
                statusText = "Squad selected";
            }

            if (GUI.Button(new Rect(94, y, 70, 28), pendingJumpOrder ? "Jet..." : "Jet"))
            {
                pendingJumpOrder = true;
                statusText = string.IsNullOrEmpty(pendingDetachedUnitId)
                    ? "Select squad jet destination"
                    : "Select jet destination for " + pendingDetachedUnitId;
            }

            if (GUI.Button(new Rect(170, y, 70, 28), showMissionMap ? "Map-" : "Map"))
            {
                showMissionMap = !showMissionMap;
                statusText = showMissionMap ? "Mission map open" : "Mission map closed";
            }

            if (GUI.Button(new Rect(246, y, 76, 28), "System"))
            {
                OpenSystemPanel();
            }

            y += 36f;

            foreach (UnitState unit in mission.PlayerUnits())
            {
                string label = unit.UnitType;
                if (unit.IsDestroyed)
                {
                    label += "  DESTROYED";
                }
                else if (unit.IsJumping)
                {
                    label += "  JET";
                }
                else if (unit.IsDetached)
                {
                    label += "  DETACHED";
                }
                else if (unit.HasAttackOrder)
                {
                    label += "  TARGET";
                }
                else if (unit.HasMoveOrder)
                {
                    label += "  MOVING";
                }
                else if (!string.IsNullOrEmpty(unit.CurrentTargetId))
                {
                    label += "  FIRING";
                }

                if (unit.IsHeatLocked)
                {
                    label += "  HOT";
                }

                if (!unit.IsDestroyed && (unit.MobilityRatio < 0.99f || unit.FirepowerRatio < 0.99f))
                {
                    label += "  M" + Mathf.RoundToInt(unit.MobilityRatio * 100f) + "/F" + Mathf.RoundToInt(unit.FirepowerRatio * 100f);
                }

                if (GUI.Button(new Rect(18, y, 304, 34), label))
                {
                    pendingDetachedUnitId = unit.Id;
                    statusText = pendingJumpOrder
                        ? "Select jet destination for " + unit.UnitType
                        : "Select destination for " + unit.UnitType;
                }

                Rect barBack = new(24, y + 24, 292, 4);
                GUI.DrawTexture(barBack, Texture2D.grayTexture);
                Rect bar = new(barBack.x, barBack.y, barBack.width * unit.Structure, barBack.height);
                DrawColorRect(bar, unit.IsDestroyed ? Color.red : Color.green);

                if (unit.Profile.HeatPerShot > 0f)
                {
                    Rect heatBack = new(24, y + 31, 292, 4);
                    GUI.DrawTexture(heatBack, Texture2D.grayTexture);
                    Rect heatBar = new(heatBack.x, heatBack.y, heatBack.width * Mathf.Clamp01(unit.HeatRatio), heatBack.height);
                    DrawColorRect(heatBar, unit.IsHeatLocked ? Color.red : new Color(1f, 0.62f, 0.12f));
                }

                Rect readyBack = new(24, y + 38, 292, 3);
                GUI.DrawTexture(readyBack, Texture2D.grayTexture);
                Rect readyBar = new(readyBack.x, readyBack.y, readyBack.width * unit.WeaponReadinessRatio, readyBack.height);
                DrawColorRect(readyBar, unit.IsHeatLocked ? Color.red : new Color(0.24f, 0.72f, 1f));

                GUI.Label(new Rect(24, y + 42, 300, 16), WeaponStatusText(unit));
                DrawSectionLine(unit, y + 58);
                y += 78f;
            }
        }

        private string WeaponStatusText(UnitState unit)
        {
            string weapon = ShortWeaponName(unit.Profile.PrimaryWeaponName);
            string state = "Ready";
            if (unit.IsDestroyed)
            {
                state = "Offline";
            }
            else if (unit.IsHeatLocked)
            {
                state = "Hot";
            }
            else if (unit.IsWeaponCoolingDown)
            {
                state = "CD " + Mathf.RoundToInt(unit.WeaponReadinessRatio * 100f) + "%";
            }
            else if (HasAttackTargetOutOfRange(unit))
            {
                state = "Out of range";
            }

            return weapon + "  R" + Mathf.RoundToInt(unit.Profile.WeaponRange) + "  " + state;
        }

        private string ShortWeaponName(string weaponName)
        {
            if (string.IsNullOrWhiteSpace(weaponName))
            {
                return "Weapon";
            }

            return weaponName.Length <= 20 ? weaponName : weaponName.Substring(0, 20);
        }

        private bool HasAttackTargetOutOfRange(UnitState unit)
        {
            if (unit == null || string.IsNullOrEmpty(unit.AttackTargetId))
            {
                return false;
            }

            UnitState targetUnit = mission.FindUnit(unit.AttackTargetId);
            if (targetUnit != null)
            {
                return !unit.IsInWeaponRange(targetUnit);
            }

            StructureState targetStructure = mission.FindStructure(unit.AttackTargetId);
            return targetStructure != null && !unit.IsInWeaponRange(targetStructure);
        }

        private void DrawObjectivePanel()
        {
            float y = 350f;
            GUI.Label(new Rect(18, y, 320, 22), "Objectives");
            y += 24f;

            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (objective.Definition.hidden)
                {
                    continue;
                }

                string state = objective.IsComplete ? "[done] " : objective.IsActive ? "[active] " : "[locked] ";
                GUI.Label(new Rect(18, y, 320, 24), state + objective.Definition.title);
                y += 24f;
            }
        }

        private void DrawStructurePanel()
        {
            if (mission.Structures.Count == 0)
            {
                return;
            }

            float y = 522f;
            GUI.Label(new Rect(18, y, 320, 22), "Targets");
            y += 24f;

            foreach (StructureState structure in mission.Structures)
            {
                string label = structure.ObjectType;
                if (structure.IsDestroyed)
                {
                    label += "  DESTROYED";
                }

                GUI.Label(new Rect(18, y, 304, 20), label);
                Rect barBack = new(24, y + 20, 292, 5);
                GUI.DrawTexture(barBack, Texture2D.grayTexture);
                Rect bar = new(barBack.x, barBack.y, barBack.width * structure.Structure, barBack.height);
                DrawColorRect(bar, structure.IsDestroyed ? Color.red : new Color(0.95f, 0.62f, 0.18f));
                y += 34f;
            }
        }

        private void DrawMissionBriefPanel()
        {
            if (!ShouldDrawMissionBriefPanel())
            {
                return;
            }

            Rect panel = MissionBriefPanelRect();
            GUI.Box(panel, "Mission");
            float x = panel.x + 12f;
            float y = panel.y + 32f;
            float width = panel.width - 24f;
            int visibleObjectives = CountVisibleObjectives();
            int completedObjectives = CountCompletedVisibleObjectives();
            int liveStructures = CountLiveStructures();
            GUI.Label(
                new Rect(x, y, width, 20f),
                "Objectives " + completedObjectives + "/" + visibleObjectives + "    Targets " + liveStructures + "/" + mission.Structures.Count);
            y += 24f;

            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (objective.Definition.hidden)
                {
                    continue;
                }

                if (y > panel.yMax - 54f)
                {
                    GUI.Label(new Rect(x, y, width, 20f), "...");
                    y += 20f;
                    break;
                }

                Color previous = GUI.color;
                GUI.color = objective.IsComplete
                    ? new Color(0.55f, 1f, 0.55f)
                    : objective.IsActive ? new Color(1f, 0.88f, 0.42f) : new Color(0.62f, 0.68f, 0.72f);
                GUI.Label(new Rect(x, y, width, 20f), ObjectiveStateLabel(objective) + TruncateText(objective.Definition.title, 36));
                GUI.color = previous;
                y += 20f;
            }

            if (mission.Structures.Count == 0 || y > panel.yMax - 42f)
            {
                return;
            }

            y += 4f;
            GUI.Label(new Rect(x, y, width, 18f), "Target structures");
            y += 20f;
            foreach (StructureState structure in mission.Structures)
            {
                if (y > panel.yMax - 22f)
                {
                    break;
                }

                DrawStructureBrief(structure, x, y, width);
                y += 28f;
            }
        }

        private bool ShouldDrawMissionBriefPanel()
        {
            return !showMissionMap && mission.Result == MissionResultState.InProgress;
        }

        private static string ObjectiveStateLabel(ObjectiveState objective)
        {
            if (objective.IsComplete)
            {
                return "[done] ";
            }

            return objective.IsActive ? "[active] " : "[locked] ";
        }

        private void DrawStructureBrief(StructureState structure, float x, float y, float width)
        {
            string label = TruncateText(structure.ObjectType, 32);
            if (structure.IsDestroyed)
            {
                label += " destroyed";
            }

            GUI.Label(new Rect(x, y, width, 18f), label);
            Rect back = new(x, y + 18f, width, 5f);
            GUI.DrawTexture(back, Texture2D.grayTexture);
            DrawColorRect(
                new Rect(back.x, back.y, back.width * Mathf.Clamp01(structure.Structure), back.height),
                structure.IsDestroyed ? Color.red : new Color(0.95f, 0.62f, 0.18f));
        }

        private int CountVisibleObjectives()
        {
            int count = 0;
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (!objective.Definition.hidden)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountCompletedVisibleObjectives()
        {
            int count = 0;
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (!objective.Definition.hidden && objective.IsComplete)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountLiveStructures()
        {
            int count = 0;
            foreach (StructureState structure in mission.Structures)
            {
                if (!structure.IsDestroyed)
                {
                    count++;
                }
            }

            return count;
        }

        private void DrawCombatPanel()
        {
            Rect panel = CombatPanelRect();
            float x = panel.x;
            GUI.Box(panel, "Combat");
            GUI.Label(new Rect(x + 12, 38, 320, 22), "Active units: " + CountLiveUnits() + " / " + mission.Units.Count);
            float y = 64f;
            foreach (string line in combatLog)
            {
                GUI.Label(new Rect(x + 12, y, 320, 20), line);
                y += 20f;
            }
        }

        private void DrawMissionMap()
        {
            if (!showMissionMap)
            {
                return;
            }

            Rect panel = MissionMapRect();
            GUI.Box(panel, "Mission Map");
            Rect map = new(panel.x + 14f, panel.y + 34f, panel.width - 28f, panel.height - 58f);
            DrawColorRect(map, new Color(0.035f, 0.045f, 0.048f, 0.92f));
            DrawColorRect(new Rect(map.x, map.y, map.width, 1f), new Color(0.35f, 0.42f, 0.44f));
            DrawColorRect(new Rect(map.x, map.yMax - 1f, map.width, 1f), new Color(0.35f, 0.42f, 0.44f));
            DrawColorRect(new Rect(map.x, map.y, 1f, map.height), new Color(0.35f, 0.42f, 0.44f));
            DrawColorRect(new Rect(map.xMax - 1f, map.y, 1f, map.height), new Color(0.35f, 0.42f, 0.44f));

            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (objective.Definition.hidden)
                {
                    continue;
                }

                DrawObjectiveMapMarker(map, objective);
            }

            foreach (StructureState structure in mission.Structures)
            {
                DrawMapMarker(map, structure.MissionPosition, structure.IsDestroyed ? Color.gray : new Color(0.95f, 0.55f, 0.16f), 8f);
            }

            foreach (UnitState unit in mission.Units)
            {
                if (!unit.IsActive && !unit.IsPlayerUnit)
                {
                    continue;
                }

                Color color = unit.IsDestroyed
                    ? Color.gray
                    : unit.IsPlayerUnit ? new Color(0.2f, 0.78f, 1f) : new Color(0.94f, 0.22f, 0.18f);
                DrawMapMarker(map, unit.MissionPosition, color, unit.IsPlayerUnit ? 7f : 5f);
            }

            if (GUI.Button(new Rect(panel.xMax - 66f, panel.y + 6f, 52f, 24f), "Close"))
            {
                showMissionMap = false;
                statusText = "Mission map closed";
            }
        }

        private void DrawObjectiveMapMarker(Rect map, ObjectiveState objective)
        {
            if (objective.Definition.conditions == null)
            {
                return;
            }

            foreach (ObjectiveCondition condition in objective.Definition.conditions)
            {
                if (condition == null)
                {
                    continue;
                }

                if (condition.targetArea != null)
                {
                    Vector2 center = new(condition.targetArea.x, condition.targetArea.y);
                    DrawMapMarker(map, center, objective.IsComplete ? Color.green : Color.yellow, objective.IsActive ? 9f : 6f);
                    continue;
                }

                if (condition.targetUnit?.position != null)
                {
                    DrawMapMarker(
                        map,
                        new Vector2(condition.targetUnit.position.x, condition.targetUnit.position.y),
                        objective.IsComplete ? Color.green : Color.yellow,
                        9f);
                }

                if (condition.targetStructure?.position != null)
                {
                    DrawMapMarker(
                        map,
                        new Vector2(condition.targetStructure.position.x, condition.targetStructure.position.y),
                        objective.IsComplete ? Color.green : new Color(1f, 0.72f, 0.15f),
                        10f);
                }
            }
        }

        private void DrawSystemPanel()
        {
            if (!showSystemPanel)
            {
                return;
            }

            Rect panel = SystemPanelRect();
            GUI.Box(panel, "System");
            GUI.Label(new Rect(panel.x + 18f, panel.y + 36f, panel.width - 36f, 24f), isPaused ? "Paused" : "Running");

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 70f, panel.width - 36f, 30f), isPaused ? "Resume" : "Pause"))
            {
                if (mission.Result == MissionResultState.InProgress)
                {
                    SetPaused(!isPaused);
                    statusText = isPaused ? "Paused" : "Resumed";
                }
                else
                {
                    statusText = MissionResultText();
                }
            }

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 108f, panel.width - 36f, 30f), "Restart Mission"))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 146f, panel.width - 36f, 30f), "End Demo"))
            {
                Application.Quit(0);
            }

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 184f, panel.width - 36f, 30f), "Close"))
            {
                showSystemPanel = false;
                if (mission.Result == MissionResultState.InProgress)
                {
                    SetPaused(false);
                }

                statusText = "System closed";
            }
        }

        private void DrawMissionResultPanel()
        {
            if (mission.Result == MissionResultState.InProgress)
            {
                return;
            }

            Rect panel = new((Screen.width - 320f) * 0.5f, 72f, 320f, 148f);
            GUI.Box(panel, MissionResultText());
            GUI.Label(new Rect(panel.x + 18f, panel.y + 36f, panel.width - 36f, 42f), mission.ResultReason);

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 88f, 132f, 30f), "Restart"))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (GUI.Button(new Rect(panel.x + 170f, panel.y + 88f, 132f, 30f), "End Demo"))
            {
                Application.Quit(0);
            }
        }

        private string MissionResultText()
        {
            if (mission.Result == MissionResultState.Victory)
            {
                return "Mission Complete";
            }

            if (mission.Result == MissionResultState.Defeat)
            {
                return "Mission Failed";
            }

            return isPaused ? "Paused" : "Running";
        }

        private void OpenSystemPanel()
        {
            showSystemPanel = true;
            if (mission.Result == MissionResultState.InProgress)
            {
                SetPaused(true);
            }

            pendingDetachedUnitId = null;
            pendingJumpOrder = false;
            statusText = "System open";
        }

        private void SetPaused(bool paused)
        {
            isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        private bool IsGuiPointBlocked(Vector2 guiPoint)
        {
            if (guiPoint.x < 360f)
            {
                return true;
            }

            if (CombatPanelRect().Contains(guiPoint))
            {
                return true;
            }

            if (ShouldDrawMissionBriefPanel() && MissionBriefPanelRect().Contains(guiPoint))
            {
                return true;
            }

            if (showMissionMap && MissionMapRect().Contains(guiPoint))
            {
                return true;
            }

            return showSystemPanel && SystemPanelRect().Contains(guiPoint);
        }

        private Rect MissionMapRect()
        {
            float width = Mathf.Clamp(Screen.width * 0.32f, 280f, 390f);
            float height = Mathf.Clamp(Screen.height * 0.38f, 230f, 330f);
            return new Rect(Screen.width - width - 16f, Screen.height - height - 16f, width, height);
        }

        private Rect SystemPanelRect()
        {
            float width = 260f;
            float height = 232f;
            return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        }

        private Rect CombatPanelRect()
        {
            return new Rect(Screen.width - 360f, 12f, 344f, 178f);
        }

        private Rect MissionBriefPanelRect()
        {
            Rect combatPanel = CombatPanelRect();
            float y = combatPanel.yMax + 8f;
            float height = Mathf.Min(Mathf.Clamp(Screen.height * 0.28f, 150f, 230f), Mathf.Max(120f, Screen.height - y - 16f));
            return new Rect(combatPanel.x, y, combatPanel.width, height);
        }

        private void DrawMapMarker(Rect map, Vector2 missionPoint, Color color, float size)
        {
            Vector2 point = MissionToMapPoint(map, missionPoint);
            float half = size * 0.5f;
            DrawColorRect(new Rect(point.x - half, point.y - half, size, size), color);
        }

        private Vector2 MissionToMapPoint(Rect map, Vector2 missionPoint)
        {
            GetMissionBounds(out float minX, out float maxX, out float minY, out float maxY);
            float x = map.x + Mathf.InverseLerp(minX, maxX, missionPoint.x) * map.width;
            float y = map.y + (1f - Mathf.InverseLerp(minY, maxY, missionPoint.y)) * map.height;
            return new Vector2(x, y);
        }

        private void GetMissionBounds(out float minX, out float maxX, out float minY, out float maxY)
        {
            TerrainMeshDefinition terrain = mission.Contract.terrainMesh;
            if (terrain != null && terrain.samples != null && terrain.samples.Length > 0)
            {
                int side = Mathf.Max(1, terrain.sampleSide);
                float spacing = Mathf.Max(1f, terrain.worldUnitsPerVertex * Mathf.Max(1, terrain.sampleStep));
                minX = terrain.minX;
                maxX = terrain.minX + spacing * (side - 1);
                maxY = terrain.minY;
                minY = terrain.minY - spacing * (side - 1);
                return;
            }

            minX = mission.Contract.mission.terrain.minX;
            maxY = mission.Contract.mission.terrain.minY;
            maxX = -minX;
            minY = -maxY;
        }

        private void DrawSectionLine(UnitState unit, float y)
        {
            float x = 24f;
            for (int index = 0; index < unit.Sections.Length; index++)
            {
                DamageSection section = unit.Sections[index];
                float width = 54f;
                Rect back = new(x, y, width, 4);
                GUI.DrawTexture(back, Texture2D.grayTexture);
                DrawColorRect(new Rect(x, y, width * section.Ratio, 4), section.IsDestroyed ? Color.red : Color.cyan);
                GUI.Label(new Rect(x, y + 4, width + 8, 16), ShortSectionName(section.Name));
                x += 58f;
            }
        }

        private string ShortSectionName(string sectionName)
        {
            switch (sectionName)
            {
                case "Cockpit":
                    return "CP";
                case "Torso":
                    return "TR";
                case "Left Arm":
                    return "LA";
                case "Right Arm":
                    return "RA";
                case "Legs":
                    return "LG";
                case "Front":
                    return "FR";
                case "Rear":
                    return "RR";
                case "Turret":
                    return "TU";
                case "Left":
                    return "L";
                case "Right":
                    return "R";
                default:
                    return sectionName;
            }
        }

        private int CountLiveUnits()
        {
            int count = 0;
            foreach (UnitState unit in mission.Units)
            {
                if (unit.IsActive && !unit.IsDestroyed)
                {
                    count++;
                }
            }

            return count;
        }

        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            {
                return text;
            }

            return text.Substring(0, Mathf.Max(0, maxLength - 3)) + "...";
        }

        private void DrawColorRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
