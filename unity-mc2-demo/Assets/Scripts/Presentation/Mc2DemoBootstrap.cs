using System.Collections.Generic;
using System.IO;
using System;
using MC2Demo.BattleCore;
using UnityEngine;

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
        private readonly Dictionary<int, bool> objectiveCompletionState = new();
        private readonly Dictionary<string, Material> materialCache = new(StringComparer.Ordinal);
        private readonly List<Material> ownedMaterials = new();
        private readonly List<string> combatLog = new();
        private BattleMission mission;
        private CombatProfileCatalog combatProfiles = CombatProfileCatalog.Empty;
        private string pendingDetachedUnitId;
        private bool pendingJumpOrder;
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

            mission.Tick(Time.deltaTime);
            CaptureCombatEvents();
            CaptureObjectiveEvents();
            HandleWorldClick();
            FollowCommander();
        }

        private void OnDestroy()
        {
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
                string line = combatEvent.AttackerId
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
                Debug.Log("MC2 combat: " + line);
            }
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

            if (Input.mousePosition.x < 360f)
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
                DemoStructureView structureView = hit.collider.GetComponentInParent<DemoStructureView>();
                if (structureView != null && structureView.Structure != null && !structureView.Structure.IsDestroyed)
                {
                    IssueMoveOrder(structureView.Structure.MissionPosition, "Attack " + structureView.Structure.ObjectType);
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
            DrawObjectivePanel();
            DrawStructurePanel();
            DrawCombatPanel();
        }

        private void DrawUnitPanel()
        {
            float y = 54f;
            GUI.Label(new Rect(18, y, 320, 22), "Lance");
            y += 24f;

            if (GUI.Button(new Rect(18, y, 92, 28), "All"))
            {
                pendingDetachedUnitId = null;
                pendingJumpOrder = false;
                statusText = "Squad selected";
            }

            if (GUI.Button(new Rect(116, y, 92, 28), pendingJumpOrder ? "Jet..." : "Jet"))
            {
                pendingJumpOrder = true;
                statusText = string.IsNullOrEmpty(pendingDetachedUnitId)
                    ? "Select squad jet destination"
                    : "Select jet destination for " + pendingDetachedUnitId;
            }

            GUI.Button(new Rect(214, y, 108, 28), "System");
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
                else if (unit.HasMoveOrder)
                {
                    label += "  MOVING";
                }
                else if (!string.IsNullOrEmpty(unit.CurrentTargetId))
                {
                    label += "  FIRING";
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

                DrawSectionLine(unit, y + 38);
                y += 60f;
            }
        }

        private void DrawObjectivePanel()
        {
            float y = 306f;
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

            float y = 478f;
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

        private void DrawCombatPanel()
        {
            float x = Screen.width - 360f;
            GUI.Box(new Rect(x, 12, 344, 178), "Combat");
            GUI.Label(new Rect(x + 12, 38, 320, 22), "Active units: " + CountLiveUnits() + " / " + mission.Units.Count);
            float y = 64f;
            foreach (string line in combatLog)
            {
                GUI.Label(new Rect(x + 12, y, 320, 20), line);
                y += 20f;
            }
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
                if (!unit.IsDestroyed)
                {
                    count++;
                }
            }

            return count;
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
