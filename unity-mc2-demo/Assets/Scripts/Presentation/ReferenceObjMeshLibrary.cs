using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MC2Demo.BattleCore;
using UnityEngine;

namespace MC2Demo.Presentation
{
    public static class ReferenceObjMeshLibrary
    {
        private const float DefaultReferenceVisualScale = 3.0f;
        private const float MechReferenceVisualScaleMultiplier = 0.92f;
        private const float VehicleReferenceVisualScaleMultiplier = 0.68f;
        private const float InfantryReferenceVisualScaleMultiplier = 0.38f;
        private const float OtherUnitReferenceVisualScaleMultiplier = 0.72f;
        private const float DefaultGroundOffsetY = -0.5f;
        private const string UnitCategoryMech = "mech";
        private const string UnitCategoryVehicle = "vehicle";
        private const string UnitCategoryInfantry = "infantry";
        private const string UnitCategoryOther = "other";
        private static readonly Dictionary<string, ReferenceVisualMeshSet> MeshSetCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, bool> MissingCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Texture2D> TextureCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, bool> MissingTextureCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly List<LoadedReferenceManifest> ManifestCache = new();
        private static readonly HashSet<string> LoggedManifestMappings = new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> LoggedManifestObjectMisses = new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> LoggedLooseObjectFallbacks = new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> LoggedObjectFallbackMisses = new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> LoggedTextureFallbacks = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, ReferenceUnitVisualAuditCounter> UnitVisualAudit = new(StringComparer.OrdinalIgnoreCase);
        private static bool manifestLoadAttempted;
        private static bool loggedReferenceShader;

        public static void ResetScaleAudit()
        {
            UnitVisualAudit.Clear();
        }

        public static string ScaleAuditSummary()
        {
            return "ReferenceUnits="
                + UnitAuditSegment(UnitCategoryMech)
                + " "
                + UnitAuditSegment(UnitCategoryVehicle)
                + " "
                + UnitAuditSegment(UnitCategoryInfantry)
                + " "
                + UnitAuditSegment(UnitCategoryOther)
                + " scale mech="
                + MechReferenceVisualScaleMultiplier.ToString(CultureInfo.InvariantCulture)
                + " vehicle="
                + VehicleReferenceVisualScaleMultiplier.ToString(CultureInfo.InvariantCulture)
                + " infantry="
                + InfantryReferenceVisualScaleMultiplier.ToString(CultureInfo.InvariantCulture);
        }

        public static bool TryAttachReferenceVisual(UnitState unit, Transform parent, Color color, out Renderer renderer)
        {
            renderer = null;
            if (unit == null || parent == null)
            {
                return false;
            }

            string category = UnitVisualCategoryFor(unit.UnitType);
            string assetName = AssetNameForUnitType(unit.UnitType);
            if (string.IsNullOrWhiteSpace(assetName))
            {
                RecordUnitVisualAudit(category, loaded: false);
                return false;
            }

            bool loaded = TryAttachReferenceAsset(
                assetName,
                parent,
                color,
                useTeamColor: true,
                isPlayerTeam: unit.IsPlayerUnit,
                visualScaleMultiplier: UnitReferenceVisualScaleMultiplierFor(category),
                compensateParentScale: true,
                groundOffsetY: float.NaN,
                out renderer);
            RecordUnitVisualAudit(category, loaded);
            return loaded;
        }

        public static bool TryAttachReferenceAsset(
            string assetName,
            Transform parent,
            Color color,
            bool useTeamColor,
            bool isPlayerTeam,
            out Renderer renderer)
        {
            return TryAttachReferenceAsset(
                assetName,
                parent,
                color,
                useTeamColor,
                isPlayerTeam,
                visualScaleMultiplier: 1f,
                compensateParentScale: false,
                groundOffsetY: float.NaN,
                out renderer);
        }

        public static bool TryAttachReferenceAsset(
            string assetName,
            Transform parent,
            Color color,
            bool useTeamColor,
            bool isPlayerTeam,
            float visualScaleMultiplier,
            bool compensateParentScale,
            float groundOffsetY,
            out Renderer renderer)
        {
            renderer = null;
            if (string.IsNullOrWhiteSpace(assetName) || parent == null)
            {
                return false;
            }

            if (!TryLoadMeshSet(assetName, out ReferenceVisualMeshSet meshSet))
            {
                return false;
            }

            TryGetManifestEntry(assetName, out ReferenceVisualManifestEntry manifestEntry, out _, out _);
            GameObject visual = new(parent.name + " reference " + assetName);
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = new Vector3(
                0f,
                float.IsNaN(groundOffsetY) ? GroundOffsetYFor(manifestEntry) : groundOffsetY,
                0f);
            visual.transform.localRotation = Quaternion.Euler(0f, UnityYawDegreesFor(manifestEntry), 0f);
            float visualScale = UnityScaleFor(manifestEntry) * Mathf.Max(0.001f, visualScaleMultiplier);
            visual.transform.localScale = compensateParentScale
                ? new Vector3(
                    visualScale / SafeScale(parent.localScale.x),
                    visualScale / SafeScale(parent.localScale.y),
                    visualScale / SafeScale(parent.localScale.z))
                : Vector3.one * visualScale;

            Material material = CreateMaterial(assetName, useTeamColor, isPlayerTeam, color);
            Renderer firstRenderer = null;
            HashSet<string> childNames = new(StringComparer.OrdinalIgnoreCase);
            foreach (ReferenceVisualNodeMesh nodeMesh in meshSet.Nodes)
            {
                GameObject child = new(string.IsNullOrWhiteSpace(nodeMesh.Name) ? assetName + " shape" : nodeMesh.Name);
                child.transform.SetParent(visual.transform, false);
                childNames.Add(child.name);

                MeshFilter filter = child.AddComponent<MeshFilter>();
                filter.sharedMesh = nodeMesh.Mesh;
                MeshRenderer meshRenderer = child.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = material;
                firstRenderer ??= meshRenderer;
            }

            CreateManifestNodeAnchors(visual.transform, manifestEntry, childNames);
            renderer = firstRenderer;
            return renderer != null;
        }

        private static float SafeScale(float value)
        {
            return Mathf.Abs(value) < 0.001f ? 1f : value;
        }

        public static bool IsTallReferenceUnit(string unitType)
        {
            string assetName = AssetNameForUnitType(unitType);
            return string.Equals(assetName, "werewolf", StringComparison.OrdinalIgnoreCase)
                || string.Equals(assetName, "bushwacker", StringComparison.OrdinalIgnoreCase)
                || string.Equals(assetName, "urbanmech", StringComparison.OrdinalIgnoreCase)
                || string.Equals(assetName, "starslayer", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsInfantryUnit(string unitType)
        {
            return string.Equals(UnitVisualCategoryFor(unitType), UnitCategoryInfantry, StringComparison.OrdinalIgnoreCase);
        }

        public static string UnitVisualCategoryFor(string unitType)
        {
            if (string.IsNullOrWhiteSpace(unitType))
            {
                return UnitCategoryOther;
            }

            switch (unitType.Trim().ToLowerInvariant())
            {
                case "werewolf":
                case "bushwacker":
                case "urbanmech":
                case "starslayer":
                    return UnitCategoryMech;
                case "centipede":
                case "harasser":
                case "lrmc":
                    return UnitCategoryVehicle;
                case "infantry":
                case "poweredarmor":
                    return UnitCategoryInfantry;
                default:
                    return UnitCategoryOther;
            }
        }

        private static float UnitReferenceVisualScaleMultiplierFor(string category)
        {
            if (string.Equals(category, UnitCategoryMech, StringComparison.OrdinalIgnoreCase))
            {
                return MechReferenceVisualScaleMultiplier;
            }

            if (string.Equals(category, UnitCategoryVehicle, StringComparison.OrdinalIgnoreCase))
            {
                return VehicleReferenceVisualScaleMultiplier;
            }

            if (string.Equals(category, UnitCategoryInfantry, StringComparison.OrdinalIgnoreCase))
            {
                return InfantryReferenceVisualScaleMultiplier;
            }

            return OtherUnitReferenceVisualScaleMultiplier;
        }

        private static void RecordUnitVisualAudit(string category, bool loaded)
        {
            string normalizedCategory = string.IsNullOrWhiteSpace(category) ? UnitCategoryOther : category;
            if (!UnitVisualAudit.TryGetValue(normalizedCategory, out ReferenceUnitVisualAuditCounter counter))
            {
                counter = new ReferenceUnitVisualAuditCounter();
                UnitVisualAudit[normalizedCategory] = counter;
            }

            if (loaded)
            {
                counter.Loaded++;
            }
            else
            {
                counter.Fallback++;
            }
        }

        private static string UnitAuditSegment(string category)
        {
            UnitVisualAudit.TryGetValue(category, out ReferenceUnitVisualAuditCounter counter);
            int loaded = counter?.Loaded ?? 0;
            int fallback = counter?.Fallback ?? 0;
            return category
                + " "
                + loaded.ToString(CultureInfo.InvariantCulture)
                + "/"
                + fallback.ToString(CultureInfo.InvariantCulture);
        }

        public static bool TryCloneSectionPart(
            Transform visualRoot,
            string sectionName,
            bool hideSource,
            out GameObject part)
        {
            part = null;
            if (visualRoot == null || string.IsNullOrWhiteSpace(sectionName))
            {
                return false;
            }

            List<Transform> sources = FindSectionSourceNodes(visualRoot, sectionName);
            if (sources.Count == 0)
            {
                return false;
            }

            part = CloneDrawableSources(sectionName, sources, hideSource);
            return part != null;
        }

        private static bool TryLoadMeshSet(string assetName, out ReferenceVisualMeshSet meshSet)
        {
            if (MeshSetCache.TryGetValue(assetName, out meshSet))
            {
                return true;
            }

            if (MissingCache.ContainsKey(assetName))
            {
                meshSet = null;
                return false;
            }

            string objPath = FindObjPath(assetName);
            if (string.IsNullOrWhiteSpace(objPath))
            {
                MissingCache[assetName] = true;
                return false;
            }

            try
            {
                meshSet = LoadObjMeshSet(objPath, assetName);
                MeshSetCache[assetName] = meshSet;
                Debug.Log("Loaded private reference OBJ mesh: " + objPath + " groups=" + meshSet.Nodes.Length.ToString(CultureInfo.InvariantCulture));
                return true;
            }
            catch (Exception ex)
            {
                MissingCache[assetName] = true;
                Debug.LogWarning("Failed to load private reference OBJ mesh " + objPath + ": " + ex.Message);
                meshSet = null;
                return false;
            }
        }

        private static ReferenceVisualMeshSet LoadObjMeshSet(string path, string assetName)
        {
            List<Vector3> sourceVertices = new();
            List<Vector2> sourceUvs = new();
            List<ObjGroupBuilder> groups = new();
            ObjGroupBuilder currentGroup = null;

            foreach (string rawLine in File.ReadLines(path))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }

                if (line.StartsWith("v ", StringComparison.Ordinal))
                {
                    string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    sourceVertices.Add(new Vector3(ParseFloat(parts[1]), ParseFloat(parts[2]), ParseFloat(parts[3])));
                    continue;
                }

                if (line.StartsWith("vt ", StringComparison.Ordinal))
                {
                    string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    sourceUvs.Add(new Vector2(ParseFloat(parts[1]), ParseFloat(parts[2])));
                    continue;
                }

                if (line.StartsWith("g ", StringComparison.Ordinal))
                {
                    string groupName = line.Length > 2 ? line.Substring(2).Trim() : "shape";
                    currentGroup = new ObjGroupBuilder(groupName);
                    groups.Add(currentGroup);
                    continue;
                }

                if (!line.StartsWith("f ", StringComparison.Ordinal))
                {
                    continue;
                }

                currentGroup ??= EnsureDefaultGroup(groups);
                string[] faceParts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (faceParts.Length < 4)
                {
                    continue;
                }

                int faceStart = currentGroup.Vertices.Count;
                for (int index = 1; index < faceParts.Length; index++)
                {
                    ParseFaceToken(faceParts[index], out int vertexIndex, out int uvIndex);
                    if (vertexIndex < 0 || vertexIndex >= sourceVertices.Count)
                    {
                        currentGroup.Vertices.Add(Vector3.zero);
                    }
                    else
                    {
                        currentGroup.Vertices.Add(sourceVertices[vertexIndex]);
                    }

                    if (uvIndex >= 0 && uvIndex < sourceUvs.Count)
                    {
                        currentGroup.Uvs.Add(sourceUvs[uvIndex]);
                    }
                    else
                    {
                        currentGroup.Uvs.Add(Vector2.zero);
                    }
                }

                for (int index = 1; index < faceParts.Length - 2; index++)
                {
                    currentGroup.Triangles.Add(faceStart);
                    currentGroup.Triangles.Add(faceStart + index);
                    currentGroup.Triangles.Add(faceStart + index + 1);
                }
            }

            List<ReferenceVisualNodeMesh> nodeMeshes = new();
            foreach (ObjGroupBuilder group in groups)
            {
                if (group.Triangles.Count == 0)
                {
                    continue;
                }

                Mesh mesh = new()
                {
                    name = "Reference " + assetName + " " + group.Name
                };
                mesh.SetVertices(group.Vertices);
                mesh.SetUVs(0, group.Uvs);
                mesh.SetTriangles(group.Triangles, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                nodeMeshes.Add(new ReferenceVisualNodeMesh(group.Name, mesh));
            }

            if (nodeMeshes.Count == 0)
            {
                throw new InvalidDataException("OBJ does not contain drawable groups.");
            }

            return new ReferenceVisualMeshSet(nodeMeshes.ToArray());
        }

        private static List<Transform> FindSectionSourceNodes(Transform visualRoot, string sectionName)
        {
            List<Transform> drawableNodes = DrawableReferenceNodes(visualRoot);
            List<Transform> sources = new();
            HashSet<Transform> sourceSet = new();

            foreach (string manifestNodeName in ManifestNodeNamesForSection(visualRoot, sectionName))
            {
                AddNamedDrawableNode(drawableNodes, manifestNodeName, sources, sourceSet);
            }

            if (sources.Count == 0)
            {
                for (int index = 0; index < drawableNodes.Count; index++)
                {
                    Transform drawableNode = drawableNodes[index];
                    if (drawableNode != null && NodeNameMatchesSection(drawableNode.name, sectionName))
                    {
                        AddUniqueNode(drawableNode, sources, sourceSet);
                    }
                }
            }

            if (sources.Count == 0 && IsCockpitSection(sectionName))
            {
                AddUniqueNode(FindCockpitFallbackDrawableNode(visualRoot, drawableNodes), sources, sourceSet);
            }

            return sources;
        }

        private static List<Transform> DrawableReferenceNodes(Transform visualRoot)
        {
            List<Transform> nodes = new();
            if (visualRoot == null)
            {
                return nodes;
            }

            Transform[] descendants = visualRoot.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < descendants.Length; index++)
            {
                Transform descendant = descendants[index];
                if (descendant == null || descendant == visualRoot)
                {
                    continue;
                }

                if (descendant.GetComponent<Renderer>() != null)
                {
                    nodes.Add(descendant);
                }
            }

            return nodes;
        }

        private static string[] ManifestNodeNamesForSection(Transform visualRoot, string sectionName)
        {
            string assetName = AssetNameFromVisualRoot(visualRoot);
            if (string.IsNullOrWhiteSpace(assetName)
                || !TryGetManifestEntry(assetName, out ReferenceVisualManifestEntry entry, out _, out _))
            {
                return Array.Empty<string>();
            }

            if (IsCockpitSection(sectionName))
            {
                return entry.cockpitNodeNames ?? Array.Empty<string>();
            }

            if (IsLeftArmSection(sectionName))
            {
                return entry.leftArmNodeNames ?? Array.Empty<string>();
            }

            if (IsRightArmSection(sectionName))
            {
                return entry.rightArmNodeNames ?? Array.Empty<string>();
            }

            if (IsLeftLegSection(sectionName))
            {
                return entry.leftLegNodeNames ?? Array.Empty<string>();
            }

            if (IsRightLegSection(sectionName))
            {
                return entry.rightLegNodeNames ?? Array.Empty<string>();
            }

            if (IsTorsoSection(sectionName))
            {
                return entry.torsoNodeNames ?? Array.Empty<string>();
            }

            return Array.Empty<string>();
        }

        private static string AssetNameFromVisualRoot(Transform visualRoot)
        {
            if (visualRoot == null || string.IsNullOrWhiteSpace(visualRoot.name))
            {
                return "";
            }

            const string marker = " reference ";
            int markerIndex = visualRoot.name.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
            {
                return "";
            }

            return visualRoot.name.Substring(markerIndex + marker.Length).Trim();
        }

        private static void AddNamedDrawableNode(
            List<Transform> drawableNodes,
            string nodeName,
            List<Transform> sources,
            HashSet<Transform> sourceSet)
        {
            if (string.IsNullOrWhiteSpace(nodeName))
            {
                return;
            }

            string normalizedTarget = NormalizeNodeName(nodeName);
            for (int index = 0; index < drawableNodes.Count; index++)
            {
                Transform candidate = drawableNodes[index];
                if (candidate == null)
                {
                    continue;
                }

                if (string.Equals(candidate.name, nodeName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(NormalizeNodeName(candidate.name), normalizedTarget, StringComparison.Ordinal))
                {
                    AddUniqueNode(candidate, sources, sourceSet);
                }
            }
        }

        private static void AddUniqueNode(Transform node, List<Transform> sources, HashSet<Transform> sourceSet)
        {
            if (node == null || !sourceSet.Add(node))
            {
                return;
            }

            sources.Add(node);
        }

        private static bool NodeNameMatchesSection(string nodeName, string sectionName)
        {
            string normalizedName = NormalizeNodeName(nodeName);
            if (IsCockpitSection(sectionName))
            {
                return ContainsAny(normalizedName, "cockpit", "canopy", "head", "pilot");
            }

            if (IsLeftArmSection(sectionName))
            {
                if (ContainsAny(
                    normalizedName,
                    "rightarm",
                    "rarm",
                    "ruarm",
                    "rlarm",
                    "rgun",
                    "rhand",
                    "rmlauncher",
                    "weaponrightarm"))
                {
                    return false;
                }

                return ContainsAny(
                    normalizedName,
                    "leftarm",
                    "larm",
                    "luarm",
                    "llarm",
                    "lgun",
                    "lhand",
                    "lmlauncher",
                    "weaponleftarm");
            }

            if (IsRightArmSection(sectionName))
            {
                if (ContainsAny(
                    normalizedName,
                    "leftarm",
                    "larm",
                    "luarm",
                    "llarm",
                    "lgun",
                    "lhand",
                    "lmlauncher",
                    "weaponleftarm"))
                {
                    return false;
                }

                return ContainsAny(
                    normalizedName,
                    "rightarm",
                    "rarm",
                    "ruarm",
                    "rlarm",
                    "rgun",
                    "rhand",
                    "rmlauncher",
                    "weaponrightarm");
            }

            if (IsLeftLegSection(sectionName))
            {
                if (ContainsAny(normalizedName, "rightleg", "rleg", "rlleg", "rmleg", "ruleg", "rfoot", "rtoe", "rankle"))
                {
                    return false;
                }

                return ContainsAny(
                    normalizedName,
                    "leftleg",
                    "lleg",
                    "llleg",
                    "lmleg",
                    "luleg",
                    "lfoot",
                    "ltoe",
                    "lankle");
            }

            if (IsRightLegSection(sectionName))
            {
                if (ContainsAny(normalizedName, "leftleg", "lleg", "llleg", "lmleg", "luleg", "lfoot", "ltoe", "lankle"))
                {
                    return false;
                }

                return ContainsAny(
                    normalizedName,
                    "rightleg",
                    "rleg",
                    "rlleg",
                    "rmleg",
                    "ruleg",
                    "rfoot",
                    "rtoe",
                    "rankle");
            }

            if (IsTorsoSection(sectionName))
            {
                return ContainsAny(normalizedName, "torso", "centertorso", "hip", "hips");
            }

            return false;
        }

        private static bool ContainsAny(string value, params string[] needles)
        {
            for (int index = 0; index < needles.Length; index++)
            {
                if (value.IndexOf(needles[index], StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static Transform FindDescendantByName(Transform root, string name)
        {
            if (root == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < descendants.Length; index++)
            {
                Transform descendant = descendants[index];
                if (descendant != null && string.Equals(descendant.name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return descendant;
                }
            }

            return null;
        }

        private static Transform FindNearestDrawableNode(Transform anchor, List<Transform> drawableNodes)
        {
            if (anchor == null || drawableNodes == null || drawableNodes.Count == 0)
            {
                return null;
            }

            Transform best = null;
            float bestScore = float.MaxValue;
            Vector3 anchorPosition = anchor.position;
            for (int index = 0; index < drawableNodes.Count; index++)
            {
                Transform candidate = drawableNodes[index];
                if (candidate == null || !TryGetRendererBounds(candidate, out Bounds bounds))
                {
                    continue;
                }

                float distance = (bounds.center - anchorPosition).sqrMagnitude;
                float score = distance + RendererBoundsVolume(bounds) * 0.0005f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        private static Transform FindCockpitFallbackDrawableNode(Transform visualRoot, List<Transform> drawableNodes)
        {
            if (visualRoot == null || drawableNodes == null || drawableNodes.Count == 0)
            {
                return null;
            }

            Transform best = null;
            float bestScore = float.MinValue;
            for (int index = 0; index < drawableNodes.Count; index++)
            {
                Transform candidate = drawableNodes[index];
                if (candidate == null || !TryGetRendererBounds(candidate, out Bounds bounds))
                {
                    continue;
                }

                string normalizedName = NormalizeNodeName(candidate.name);
                Vector3 localCenter = visualRoot.InverseTransformPoint(bounds.center);
                float score = localCenter.y * 2.0f + localCenter.z * 0.35f;
                if (ContainsAny(normalizedName, "torso", "centertorso"))
                {
                    score += 8.0f;
                }

                if (ContainsAny(normalizedName, "hip", "hips"))
                {
                    score += 1.5f;
                }

                if (ContainsAny(normalizedName, "foot", "toe", "leg", "arm", "gun", "hand"))
                {
                    score -= 6.0f;
                }

                score -= RendererBoundsVolume(bounds) * 0.0002f;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        private static GameObject CloneDrawableSources(string sectionName, List<Transform> sources, bool hideSource)
        {
            if (sources == null || sources.Count == 0 || !TryGetCombinedRendererBounds(sources, out Bounds bounds))
            {
                return null;
            }

            GameObject root = new("Reference Detached " + sectionName);
            root.transform.position = bounds.center;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            List<string> clonedNames = new();
            for (int index = 0; index < sources.Count; index++)
            {
                Transform source = sources[index];
                if (source == null)
                {
                    continue;
                }

                GameObject clone = UnityEngine.Object.Instantiate(source.gameObject, source.position, source.rotation);
                clone.name = source.name + " detached";
                clone.transform.localScale = source.lossyScale;
                clone.transform.SetParent(root.transform, true);
                SetRenderersEnabled(clone.transform, true);
                RemoveCloneColliders(clone);
                clonedNames.Add(source.name);

                if (hideSource)
                {
                    SetRenderersEnabled(source, false);
                }
            }

            if (clonedNames.Count == 0)
            {
                UnityEngine.Object.Destroy(root);
                return null;
            }

            Debug.Log(
                "Detached private reference visual nodes for "
                + sectionName
                + ": "
                + string.Join(", ", clonedNames));
            return root;
        }

        private static bool TryGetCombinedRendererBounds(List<Transform> sources, out Bounds bounds)
        {
            bounds = new Bounds(Vector3.zero, Vector3.zero);
            bool hasBounds = false;
            for (int sourceIndex = 0; sourceIndex < sources.Count; sourceIndex++)
            {
                Transform source = sources[sourceIndex];
                if (source == null)
                {
                    continue;
                }

                Renderer[] renderers = source.GetComponentsInChildren<Renderer>(true);
                for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    Renderer renderer = renderers[rendererIndex];
                    if (renderer == null)
                    {
                        continue;
                    }

                    if (!hasBounds)
                    {
                        bounds = renderer.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }

            return hasBounds;
        }

        private static bool TryGetRendererBounds(Transform source, out Bounds bounds)
        {
            if (source == null)
            {
                bounds = new Bounds(Vector3.zero, Vector3.zero);
                return false;
            }

            List<Transform> singleSource = new() { source };
            return TryGetCombinedRendererBounds(singleSource, out bounds);
        }

        private static float RendererBoundsVolume(Bounds bounds)
        {
            Vector3 size = bounds.size;
            return Mathf.Abs(size.x * size.y * size.z);
        }

        private static void RemoveCloneColliders(GameObject clone)
        {
            if (clone == null)
            {
                return;
            }

            Collider[] colliders = clone.GetComponentsInChildren<Collider>(true);
            for (int index = 0; index < colliders.Length; index++)
            {
                if (colliders[index] != null)
                {
                    UnityEngine.Object.Destroy(colliders[index]);
                }
            }
        }

        private static void SetRenderersEnabled(Transform source, bool enabled)
        {
            if (source == null)
            {
                return;
            }

            Renderer[] renderers = source.GetComponentsInChildren<Renderer>(true);
            for (int index = 0; index < renderers.Length; index++)
            {
                if (renderers[index] != null)
                {
                    renderers[index].enabled = enabled;
                }
            }
        }

        private static string NormalizeNodeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            string lower = value.ToLowerInvariant();
            char[] buffer = new char[lower.Length];
            int count = 0;
            for (int index = 0; index < lower.Length; index++)
            {
                char c = lower[index];
                if (char.IsLetterOrDigit(c))
                {
                    buffer[count++] = c;
                }
            }

            return new string(buffer, 0, count);
        }

        private static bool IsCockpitSection(string sectionName)
        {
            return string.Equals(sectionName, "Cockpit", StringComparison.OrdinalIgnoreCase)
                || string.Equals(sectionName, "Turret", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsLeftArmSection(string sectionName)
        {
            return string.Equals(sectionName, "Left Arm", StringComparison.OrdinalIgnoreCase)
                || string.Equals(sectionName, "Left", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsRightArmSection(string sectionName)
        {
            return string.Equals(sectionName, "Right Arm", StringComparison.OrdinalIgnoreCase)
                || string.Equals(sectionName, "Right", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsLeftLegSection(string sectionName)
        {
            return string.Equals(sectionName, "Left Leg", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsRightLegSection(string sectionName)
        {
            return string.Equals(sectionName, "Right Leg", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTorsoSection(string sectionName)
        {
            return string.Equals(sectionName, "Torso", StringComparison.OrdinalIgnoreCase);
        }

        private static ObjGroupBuilder EnsureDefaultGroup(List<ObjGroupBuilder> groups)
        {
            if (groups.Count > 0)
            {
                return groups[groups.Count - 1];
            }

            ObjGroupBuilder group = new("shape");
            groups.Add(group);
            return group;
        }

        private static void ParseFaceToken(string token, out int vertexIndex, out int uvIndex)
        {
            vertexIndex = -1;
            uvIndex = -1;
            string[] parts = token.Split('/');
            if (parts.Length > 0 && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int vertex))
            {
                vertexIndex = vertex - 1;
            }

            if (parts.Length > 1 && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int uv))
            {
                uvIndex = uv - 1;
            }
        }

        private static float ParseFloat(string value)
        {
            return float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private static string FindObjPath(string assetName)
        {
            if (TryGetManifestEntry(assetName, out ReferenceVisualManifestEntry entry, out string objPath, out LoadedReferenceManifest manifest))
            {
                LogManifestMapping(assetName, entry, objPath, manifest);
                return objPath;
            }

            string loosePath = FindLooseObjPath(assetName);
            if (!string.IsNullOrWhiteSpace(loosePath))
            {
                LogLooseObjFallback(assetName, loosePath);
                return loosePath;
            }

            LogObjFallbackMiss(assetName);
            return "";
        }

        private static string FindLooseObjPath(string assetName)
        {
            foreach (string root in CandidateRoots())
            {
                string candidate = Path.Combine(root, assetName, assetName + ".obj");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return "";
        }

        private static void LogManifestMapping(string assetName, ReferenceVisualManifestEntry entry, string objPath, LoadedReferenceManifest manifest)
        {
            string key = assetName + "|" + objPath;
            if (!LoggedManifestMappings.Add(key))
            {
                return;
            }

            string manifestAsset = string.IsNullOrWhiteSpace(entry.assetId) ? "<unnamed>" : entry.assetId;
            string assetClass = string.IsNullOrWhiteSpace(entry.assetClass) ? "<unknown>" : entry.assetClass;
            string manifestPath = manifest == null ? "<unknown>" : manifest.ManifestPath;
            Debug.Log("Mapped private reference visual manifest asset: "
                + assetName
                + " -> "
                + manifestAsset
                + " class="
                + assetClass
                + " nodes="
                + entry.nodeCount.ToString(CultureInfo.InvariantCulture)
                + " tris="
                + entry.triangles.ToString(CultureInfo.InvariantCulture)
                + " manifest="
                + manifestPath
                + " obj="
                + objPath);
        }

        private static void LogManifestObjMissing(
            string assetName,
            ReferenceVisualManifestEntry entry,
            string objPath,
            LoadedReferenceManifest manifest)
        {
            string key = assetName + "|" + objPath;
            if (!LoggedManifestObjectMisses.Add(key))
            {
                return;
            }

            string manifestAsset = string.IsNullOrWhiteSpace(entry?.assetId) ? "<unnamed>" : entry.assetId;
            string manifestPath = manifest == null ? "<unknown>" : manifest.ManifestPath;
            Debug.LogWarning("Private reference visual manifest entry has no loadable OBJ: request="
                + assetName
                + " assetId="
                + manifestAsset
                + " obj="
                + objPath
                + " manifest="
                + manifestPath
                + "; trying loose OBJ fallback.");
        }

        private static void LogLooseObjFallback(string assetName, string objPath)
        {
            if (!LoggedLooseObjectFallbacks.Add(assetName + "|" + objPath))
            {
                return;
            }

            Debug.Log("Using loose private reference OBJ fallback: " + assetName + " obj=" + objPath);
        }

        private static void LogObjFallbackMiss(string assetName)
        {
            if (!LoggedObjectFallbackMisses.Add(assetName))
            {
                return;
            }

            Debug.Log("No private reference OBJ found for " + assetName + "; using development primitive fallback.");
        }

        private static void CreateManifestNodeAnchors(
            Transform visual,
            ReferenceVisualManifestEntry manifestEntry,
            HashSet<string> existingNames)
        {
            if (visual == null || manifestEntry == null)
            {
                return;
            }

            foreach (string nodeName in manifestEntry.shapeNodeNames ?? Array.Empty<string>())
            {
                CreateEmptyNodeAnchor(visual, nodeName, existingNames);
            }

            foreach (string helperName in manifestEntry.helperNodeNames ?? Array.Empty<string>())
            {
                CreateEmptyNodeAnchor(visual, helperName, existingNames);
            }
        }

        private static void CreateEmptyNodeAnchor(Transform visual, string nodeName, HashSet<string> existingNames)
        {
            if (string.IsNullOrWhiteSpace(nodeName) || existingNames.Contains(nodeName))
            {
                return;
            }

            GameObject anchor = new(nodeName);
            anchor.transform.SetParent(visual, false);
            existingNames.Add(nodeName);
        }

        private static float UnityScaleFor(ReferenceVisualManifestEntry entry)
        {
            return entry != null && entry.unityScale > 0f
                ? entry.unityScale
                : DefaultReferenceVisualScale;
        }

        private static float UnityYawDegreesFor(ReferenceVisualManifestEntry entry)
        {
            return entry?.unityYawDegrees ?? 0f;
        }

        private static float GroundOffsetYFor(ReferenceVisualManifestEntry entry)
        {
            return entry != null && Math.Abs(entry.groundOffsetY) > float.Epsilon
                ? entry.groundOffsetY
                : DefaultGroundOffsetY;
        }

        private static IEnumerable<string> CandidateRoots()
        {
            string current = Directory.GetCurrentDirectory();
            string dataPath = Application.dataPath;
            yield return FullPath(current, "analysis-output", "unity-reference-art", "assets");
            yield return FullPath(current, "..", "analysis-output", "unity-reference-art", "assets");
            yield return FullPath(dataPath, "..", "..", "analysis-output", "unity-reference-art", "assets");
            yield return FullPath(dataPath, "..", "..", "..", "analysis-output", "unity-reference-art", "assets");
            yield return FullPath(dataPath, "..", "..", "..", "..", "analysis-output", "unity-reference-art", "assets");
            yield return FullPath(current, "analysis-output", "tgl-obj");
            yield return FullPath(current, "..", "analysis-output", "tgl-obj");
            yield return FullPath(dataPath, "..", "..", "analysis-output", "tgl-obj");
            yield return FullPath(dataPath, "..", "..", "..", "analysis-output", "tgl-obj");
            yield return FullPath(dataPath, "..", "..", "..", "..", "analysis-output", "tgl-obj");
        }

        private static IEnumerable<string> CandidateManifestPaths()
        {
            string current = Directory.GetCurrentDirectory();
            string dataPath = Application.dataPath;
            yield return FullPath(current, "analysis-output", "unity-reference-art", "manifest.json");
            yield return FullPath(current, "..", "analysis-output", "unity-reference-art", "manifest.json");
            yield return FullPath(dataPath, "..", "..", "analysis-output", "unity-reference-art", "manifest.json");
            yield return FullPath(dataPath, "..", "..", "..", "analysis-output", "unity-reference-art", "manifest.json");
            yield return FullPath(dataPath, "..", "..", "..", "..", "analysis-output", "unity-reference-art", "manifest.json");
            yield return FullPath(current, "analysis-output", "tgl-obj", "manifest.json");
            yield return FullPath(current, "..", "analysis-output", "tgl-obj", "manifest.json");
            yield return FullPath(dataPath, "..", "..", "analysis-output", "tgl-obj", "manifest.json");
            yield return FullPath(dataPath, "..", "..", "..", "analysis-output", "tgl-obj", "manifest.json");
            yield return FullPath(dataPath, "..", "..", "..", "..", "analysis-output", "tgl-obj", "manifest.json");
        }

        private static string FullPath(params string[] parts)
        {
            return Path.GetFullPath(Path.Combine(parts));
        }

        private static Material CreateMaterial(string assetName, bool useTeamColor, bool isPlayerUnit, Color fallbackColor)
        {
            Shader shader = Shader.Find("MC2Demo/Private Reference Team Color")
                ?? Shader.Find("Legacy Shaders/Diffuse")
                ?? Shader.Find("Unlit/Texture")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Hidden/Internal-Colored");
            LogReferenceShader(shader);
            Color teamColor = useTeamColor
                ? isPlayerUnit
                    ? Color.Lerp(fallbackColor, new Color(0.25f, 0.78f, 1.0f), 0.56f)
                    : Color.Lerp(fallbackColor, new Color(0.95f, 0.28f, 0.18f), 0.62f)
                : fallbackColor;
            Material material = new(shader)
            {
                name = useTeamColor
                    ? isPlayerUnit ? "PrivateReferencePlayer" : "PrivateReferenceHostile"
                    : "PrivateReferenceProp",
                color = teamColor
            };
            if (TryLoadTexture(assetName, out Texture2D texture))
            {
                material.mainTexture = texture;
                Color baseTint = useTeamColor
                    ? Color.white
                    : Color.Lerp(Color.white, fallbackColor, 0.30f);
                material.color = baseTint;
                if (material.HasProperty("_TeamColor"))
                {
                    material.SetColor("_TeamColor", teamColor);
                }

                if (material.HasProperty("_TeamStrength"))
                {
                    material.SetFloat("_TeamStrength", useTeamColor ? 0.86f : 0f);
                }

                if (material.HasProperty("_BaseTint"))
                {
                    material.SetColor("_BaseTint", baseTint);
                }
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.12f);
            }

            return material;
        }

        private static void LogReferenceShader(Shader shader)
        {
            if (loggedReferenceShader)
            {
                return;
            }

            loggedReferenceShader = true;
            string shaderName = shader == null ? "<missing>" : shader.name;
            Debug.Log("Private reference visual shader: " + shaderName);
        }

        private static bool TryLoadTexture(string assetName, out Texture2D texture)
        {
            if (TextureCache.TryGetValue(assetName, out texture))
            {
                return true;
            }

            if (MissingTextureCache.ContainsKey(assetName))
            {
                return false;
            }

            string texturePath = FindTexturePath(assetName);
            if (string.IsNullOrWhiteSpace(texturePath))
            {
                MissingTextureCache[assetName] = true;
                LogTextureFallback(assetName);
                return false;
            }

            try
            {
                texture = LoadTgaTexture(texturePath, assetName);
                TextureCache[assetName] = texture;
                Debug.Log("Loaded private reference TGA texture: " + texturePath);
                return true;
            }
            catch (Exception ex)
            {
                MissingTextureCache[assetName] = true;
                Debug.LogWarning("Failed to load private reference TGA texture " + texturePath + ": " + ex.Message);
                texture = null;
                return false;
            }
        }

        private static string FindTexturePath(string assetName)
        {
            if (TryGetManifestEntry(assetName, out ReferenceVisualManifestEntry entry, out _, out LoadedReferenceManifest manifest))
            {
                foreach (ReferenceVisualTextureRecord record in entry.textureRecords ?? Array.Empty<ReferenceVisualTextureRecord>())
                {
                    if (!string.IsNullOrWhiteSpace(record.outputPath))
                    {
                        string candidate = ResolveManifestPath(record.outputPath, manifest.ManifestDirectory);
                        if (File.Exists(candidate))
                        {
                            return candidate;
                        }
                    }
                }

                foreach (string rawPath in entry.generatedPaths?.textures ?? Array.Empty<string>())
                {
                    string candidate = ResolveManifestPath(rawPath, manifest.ManifestDirectory);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }

                foreach (string rawPath in entry.copiedTexturePaths ?? Array.Empty<string>())
                {
                    string candidate = ResolveManifestPath(rawPath, manifest.ManifestDirectory);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }

                string outputDir = ResolveManifestPath(entry.outputDir, manifest.ManifestDirectory);
                foreach (string textureName in entry.copiedTextures ?? Array.Empty<string>())
                {
                    string candidate = Path.Combine(outputDir, textureName);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
            }

            foreach (string root in CandidateRoots())
            {
                string folder = Path.Combine(root, assetName);
                if (!Directory.Exists(folder))
                {
                    continue;
                }

                string[] files = Directory.GetFiles(folder, "*.tga");
                if (files.Length > 0)
                {
                    Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                    return files[0];
                }
            }

            return "";
        }

        private static void LogTextureFallback(string assetName)
        {
            if (!LoggedTextureFallbacks.Add(assetName))
            {
                return;
            }

            string warningSummary = "";
            if (TryGetManifestEntry(assetName, out ReferenceVisualManifestEntry entry, out _, out _))
            {
                foreach (string warning in entry.warnings ?? Array.Empty<string>())
                {
                    if (!string.IsNullOrWhiteSpace(warning))
                    {
                        warningSummary = " manifestWarning=" + warning;
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(warningSummary))
                {
                    foreach (ReferenceVisualTextureRecord record in entry.textureRecords ?? Array.Empty<ReferenceVisualTextureRecord>())
                    {
                        if (!string.IsNullOrWhiteSpace(record.warning))
                        {
                            warningSummary = " manifestWarning=" + record.warning;
                            break;
                        }
                    }
                }
            }

            Debug.LogWarning("No loadable private reference TGA texture for "
                + assetName
                + "; using tinted fallback material."
                + warningSummary);
        }

        private static bool TryGetManifestEntry(string assetName, out ReferenceVisualManifestEntry entry, out string objPath, out LoadedReferenceManifest loadedManifest)
        {
            entry = null;
            objPath = "";
            loadedManifest = null;

            foreach (LoadedReferenceManifest manifest in LoadedManifests())
            {
                if (manifest.Manifest?.exports == null)
                {
                    continue;
                }

                for (int index = 0; index < manifest.Manifest.exports.Length; index++)
                {
                    ReferenceVisualManifestEntry candidate = manifest.Manifest.exports[index];
                    if (candidate == null || !ManifestEntryMatches(candidate, assetName))
                    {
                        continue;
                    }

                    string resolvedObj = ResolveManifestPath(ManifestObjPath(candidate), manifest.ManifestDirectory);
                    if (!File.Exists(resolvedObj))
                    {
                        LogManifestObjMissing(assetName, candidate, resolvedObj, manifest);
                        continue;
                    }

                    entry = candidate;
                    objPath = resolvedObj;
                    loadedManifest = manifest;
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<LoadedReferenceManifest> LoadedManifests()
        {
            if (manifestLoadAttempted)
            {
                return ManifestCache;
            }

            manifestLoadAttempted = true;
            HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
            int checkedPathCount = 0;
            foreach (string manifestPath in CandidateManifestPaths())
            {
                if (!seen.Add(manifestPath) || !File.Exists(manifestPath))
                {
                    continue;
                }

                checkedPathCount++;
                try
                {
                    ReferenceVisualManifest manifest = JsonUtility.FromJson<ReferenceVisualManifest>(File.ReadAllText(manifestPath));
                    if (manifest?.exports == null || manifest.exports.Length == 0)
                    {
                        Debug.LogWarning("Private reference visual manifest has no exports: " + manifestPath);
                        continue;
                    }

                    ManifestCache.Add(new LoadedReferenceManifest(manifestPath, manifest));
                    Debug.Log("Loaded private reference visual manifest: "
                        + manifestPath
                        + " schema="
                        + manifest.schema
                        + " version="
                        + manifest.manifestVersion.ToString(CultureInfo.InvariantCulture)
                        + " exports="
                        + manifest.exports.Length.ToString(CultureInfo.InvariantCulture));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Failed to load private reference visual manifest " + manifestPath + ": " + ex.Message);
                }
            }

            if (ManifestCache.Count == 0)
            {
                Debug.Log("Private reference visual manifest not found or empty; checked "
                    + checkedPathCount.ToString(CultureInfo.InvariantCulture)
                    + " existing manifest candidate(s). Loose OBJ and primitive development fallbacks remain available.");
            }

            return ManifestCache;
        }

        private static string ManifestObjPath(ReferenceVisualManifestEntry entry)
        {
            if (entry == null)
            {
                return "";
            }

            if (!string.IsNullOrWhiteSpace(entry.generatedPaths?.obj))
            {
                return entry.generatedPaths.obj;
            }

            return entry.obj;
        }

        private static bool ManifestEntryMatches(ReferenceVisualManifestEntry entry, string assetName)
        {
            if (string.Equals(entry.assetId, assetName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.sourceName, assetName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(entry.obj))
            {
                string stem = Path.GetFileNameWithoutExtension(entry.obj);
                return string.Equals(stem, assetName, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static string ResolveManifestPath(string rawPath, string manifestDirectory)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                return "";
            }

            if (Path.IsPathRooted(rawPath))
            {
                return Path.GetFullPath(rawPath);
            }

            string manifestRelative = Path.GetFullPath(Path.Combine(manifestDirectory, rawPath));
            if (File.Exists(manifestRelative) || Directory.Exists(manifestRelative))
            {
                return manifestRelative;
            }

            string currentRelative = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), rawPath));
            if (File.Exists(currentRelative) || Directory.Exists(currentRelative))
            {
                return currentRelative;
            }

            string repoRelative = Path.GetFullPath(Path.Combine(manifestDirectory, "..", "..", rawPath));
            if (File.Exists(repoRelative) || Directory.Exists(repoRelative))
            {
                return repoRelative;
            }

            return currentRelative;
        }

        private static Texture2D LoadTgaTexture(string path, string assetName)
        {
            byte[] data = File.ReadAllBytes(path);
            if (data.Length < 18)
            {
                throw new InvalidDataException("TGA header is truncated.");
            }

            int idLength = data[0];
            int colorMapType = data[1];
            int imageType = data[2];
            int width = data[12] | (data[13] << 8);
            int height = data[14] | (data[15] << 8);
            int bitsPerPixel = data[16];
            int descriptor = data[17];
            if (colorMapType != 0)
            {
                throw new NotSupportedException("Color-mapped TGA is not supported.");
            }

            if (width <= 0 || height <= 0)
            {
                throw new InvalidDataException("TGA dimensions are invalid.");
            }

            int bytesPerPixel = bitsPerPixel / 8;
            if (bytesPerPixel != 3 && bytesPerPixel != 4)
            {
                throw new NotSupportedException("Only 24-bit and 32-bit TGA textures are supported.");
            }

            int offset = 18 + idLength;
            Color32[] pixels = new Color32[width * height];
            if (imageType == 2)
            {
                DecodeRawTga(data, offset, bytesPerPixel, descriptor, width, height, pixels);
            }
            else if (imageType == 10)
            {
                DecodeRleTga(data, offset, bytesPerPixel, descriptor, width, height, pixels);
            }
            else
            {
                throw new NotSupportedException("Unsupported TGA image type " + imageType.ToString(CultureInfo.InvariantCulture) + ".");
            }

            Texture2D texture = new(width, height, TextureFormat.RGBA32, true)
            {
                name = "Reference Texture " + assetName,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            return texture;
        }

        private static void DecodeRawTga(byte[] data, int offset, int bytesPerPixel, int descriptor, int width, int height, Color32[] pixels)
        {
            int cursor = offset;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (cursor + bytesPerPixel > data.Length)
                    {
                        throw new InvalidDataException("TGA pixel data is truncated.");
                    }

                    SetTgaPixel(data, cursor, bytesPerPixel, descriptor, width, height, x, y, pixels);
                    cursor += bytesPerPixel;
                }
            }
        }

        private static void DecodeRleTga(byte[] data, int offset, int bytesPerPixel, int descriptor, int width, int height, Color32[] pixels)
        {
            int cursor = offset;
            int pixelIndex = 0;
            int pixelCount = width * height;
            while (pixelIndex < pixelCount)
            {
                if (cursor >= data.Length)
                {
                    throw new InvalidDataException("TGA RLE packet is truncated.");
                }

                int header = data[cursor++];
                int runLength = (header & 0x7F) + 1;
                bool runPacket = (header & 0x80) != 0;
                if (runPacket)
                {
                    if (cursor + bytesPerPixel > data.Length)
                    {
                        throw new InvalidDataException("TGA RLE color is truncated.");
                    }

                    for (int index = 0; index < runLength && pixelIndex < pixelCount; index++)
                    {
                        int x = pixelIndex % width;
                        int y = pixelIndex / width;
                        SetTgaPixel(data, cursor, bytesPerPixel, descriptor, width, height, x, y, pixels);
                        pixelIndex++;
                    }

                    cursor += bytesPerPixel;
                    continue;
                }

                for (int index = 0; index < runLength && pixelIndex < pixelCount; index++)
                {
                    if (cursor + bytesPerPixel > data.Length)
                    {
                        throw new InvalidDataException("TGA RLE raw data is truncated.");
                    }

                    int x = pixelIndex % width;
                    int y = pixelIndex / width;
                    SetTgaPixel(data, cursor, bytesPerPixel, descriptor, width, height, x, y, pixels);
                    cursor += bytesPerPixel;
                    pixelIndex++;
                }
            }
        }

        private static void SetTgaPixel(byte[] data, int cursor, int bytesPerPixel, int descriptor, int width, int height, int sourceX, int sourceY, Color32[] pixels)
        {
            bool originTop = (descriptor & 0x20) != 0;
            bool originRight = (descriptor & 0x10) != 0;
            int x = originRight ? width - 1 - sourceX : sourceX;
            int y = originTop ? height - 1 - sourceY : sourceY;
            int target = y * width + x;
            byte blue = data[cursor];
            byte green = data[cursor + 1];
            byte red = data[cursor + 2];
            byte alpha = bytesPerPixel == 4 ? data[cursor + 3] : (byte)255;
            pixels[target] = new Color32(red, green, blue, alpha);
        }

        private static string AssetNameForUnitType(string unitType)
        {
            if (string.IsNullOrWhiteSpace(unitType))
            {
                return "";
            }

            switch (unitType.Trim().ToLowerInvariant())
            {
                case "werewolf":
                    return "werewolf";
                case "bushwacker":
                    return "bushwacker";
                case "centipede":
                    return "centipede";
                case "harasser":
                    return "harasser";
                case "lrmc":
                    return "lrmc";
                case "urbanmech":
                    return "urbanmech";
                case "starslayer":
                    return "starslayer";
                default:
                    return "";
            }
        }

        [Serializable]
        private sealed class ReferenceVisualManifest
        {
            public string schema;
            public int manifestVersion;
            public ReferenceVisualManifestEntry[] exports;
        }

        [Serializable]
        private sealed class ReferenceVisualManifestEntry
        {
            public string assetId;
            public string assetClass;
            public string sourceName;
            public string obj;
            public string mtl;
            public string outputDir;
            public ReferenceVisualGeneratedPaths generatedPaths;
            public string[] materialIds;
            public int[] textureIds;
            public ReferenceVisualTextureRecord[] textureRecords;
            public string[] textures;
            public string[] copiedTextures;
            public string[] copiedTexturePaths;
            public string[] cockpitNodeNames;
            public string[] leftArmNodeNames;
            public string[] rightArmNodeNames;
            public string[] leftLegNodeNames;
            public string[] rightLegNodeNames;
            public string[] torsoNodeNames;
            public string[] shapeNodeNames;
            public string[] helperNodeNames;
            public int nodeCount;
            public int shapeNodeCount;
            public int vertices;
            public int triangles;
            public float unityScale;
            public float unityYawDegrees;
            public float groundOffsetY;
            public string[] warnings;
        }

        [Serializable]
        private sealed class ReferenceVisualGeneratedPaths
        {
            public string outputDir;
            public string obj;
            public string mtl;
            public string summary;
            public string[] textures;
        }

        [Serializable]
        private sealed class ReferenceVisualTextureRecord
        {
            public int textureId;
            public string sourceName;
            public string fileName;
            public string materialId;
            public bool alpha;
            public bool copied;
            public string outputPath;
            public string warning;
        }

        private sealed class LoadedReferenceManifest
        {
            public LoadedReferenceManifest(string manifestPath, ReferenceVisualManifest manifest)
            {
                ManifestPath = manifestPath;
                ManifestDirectory = Path.GetDirectoryName(manifestPath) ?? "";
                Manifest = manifest;
            }

            public string ManifestPath { get; }
            public string ManifestDirectory { get; }
            public ReferenceVisualManifest Manifest { get; }
        }

        private sealed class ReferenceUnitVisualAuditCounter
        {
            public int Loaded;
            public int Fallback;
        }

        private sealed class ReferenceVisualMeshSet
        {
            public ReferenceVisualMeshSet(ReferenceVisualNodeMesh[] nodes)
            {
                Nodes = nodes ?? Array.Empty<ReferenceVisualNodeMesh>();
            }

            public ReferenceVisualNodeMesh[] Nodes { get; }
        }

        private sealed class ReferenceVisualNodeMesh
        {
            public ReferenceVisualNodeMesh(string name, Mesh mesh)
            {
                Name = name ?? "";
                Mesh = mesh;
            }

            public string Name { get; }
            public Mesh Mesh { get; }
        }

        private sealed class ObjGroupBuilder
        {
            public ObjGroupBuilder(string name)
            {
                Name = string.IsNullOrWhiteSpace(name) ? "shape" : name;
            }

            public string Name { get; }
            public List<Vector3> Vertices { get; } = new();
            public List<Vector2> Uvs { get; } = new();
            public List<int> Triangles { get; } = new();
        }
    }
}
