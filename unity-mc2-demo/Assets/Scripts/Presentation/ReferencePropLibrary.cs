using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MC2Demo.BattleCore;
using UnityEngine;

namespace MC2Demo.Presentation
{
    public static class ReferencePropLibrary
    {
        private const int FirstSliceTerrainObjectLimit = 260;
        private const string PropCategoryStructure = "structure";
        private const string PropCategoryBuilding = "building";
        private const string PropCategoryAircraft = "aircraft";
        private const string PropCategoryVehicle = "vehicle";
        private const string PropCategoryBarricade = "barricade";
        private const string PropCategoryTree = "tree";
        private const string PropCategorySmallProp = "smallProp";
        private const string PropCategoryOther = "other";
        private static readonly Dictionary<string, ReferencePropVisualAuditCounter> PropVisualAudit = new(StringComparer.OrdinalIgnoreCase);

        public static void ResetScaleAudit()
        {
            PropVisualAudit.Clear();
        }

        public static string ScaleAuditSummary()
        {
            return "ReferencePropScale="
                + PropAuditSegment(PropCategoryStructure)
                + " "
                + PropAuditSegment(PropCategoryBuilding)
                + " "
                + PropAuditSegment(PropCategoryAircraft)
                + " "
                + PropAuditSegment(PropCategoryVehicle)
                + " "
                + PropAuditSegment(PropCategoryBarricade)
                + " "
                + PropAuditSegment(PropCategoryTree)
                + " "
                + PropAuditSegment(PropCategorySmallProp)
                + " "
                + PropAuditSegment(PropCategoryOther);
        }

        public static bool TryAttachStructure(StructureState structure, Transform parent, Color fallbackColor, out Renderer renderer)
        {
            renderer = null;
            if (structure == null || parent == null)
            {
                return false;
            }

            string category = PropCategoryStructure;
            foreach (string candidate in Candidates(structure.VisualAssetId, structure.ObjectProfile, structure.ObjectType))
            {
                if (ReferenceObjMeshLibrary.TryAttachReferenceAsset(
                    candidate,
                    parent,
                    fallbackColor,
                    useTeamColor: false,
                    isPlayerTeam: structure.TeamId == 0,
                    visualScaleMultiplier: StructureVisualScaleMultiplierFor(structure),
                    compensateParentScale: true,
                    groundOffsetY: 0f,
                    out renderer))
                {
                    RecordPropVisualAudit(category, loaded: true);
                    return true;
                }
            }

            RecordPropVisualAudit(category, loaded: false);
            return false;
        }

        public static bool TryAttachTerrainObject(TerrainObjectSpawn terrainObject, Transform parent, Color fallbackColor, out Renderer renderer)
        {
            renderer = null;
            if (terrainObject == null || parent == null || !ShouldUseReferenceTerrainObject(terrainObject))
            {
                if (terrainObject != null && parent != null)
                {
                    RecordPropVisualAudit(TerrainVisualCategoryFor(terrainObject), loaded: false);
                }

                return false;
            }

            string category = TerrainVisualCategoryFor(terrainObject);
            foreach (string candidate in Candidates(terrainObject.assetId, terrainObject.fileName))
            {
                if (ReferenceObjMeshLibrary.TryAttachReferenceAsset(
                    candidate,
                    parent,
                    fallbackColor,
                    useTeamColor: false,
                    isPlayerTeam: terrainObject.teamId == 0,
                    visualScaleMultiplier: TerrainVisualScaleMultiplierFor(category, terrainObject),
                    compensateParentScale: true,
                    groundOffsetY: 0f,
                    out renderer))
                {
                    RecordPropVisualAudit(category, loaded: true);
                    return true;
                }
            }

            RecordPropVisualAudit(category, loaded: false);
            return false;
        }

        public static bool ShouldUseReferenceTerrainObject(TerrainObjectSpawn terrainObject)
        {
            if (terrainObject == null)
            {
                return false;
            }

            if (string.Equals(terrainObject.objectClass, "BUILDING", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return (terrainObject.sourceIndex >= 0 && terrainObject.sourceIndex <= FirstSliceTerrainObjectLimit)
                || IsInFirstSliceAirfield(terrainObject);
        }

        private static bool IsInFirstSliceAirfield(TerrainObjectSpawn terrainObject)
        {
            if (terrainObject?.position == null)
            {
                return false;
            }

            return terrainObject.position.x >= 2200f
                && terrainObject.position.x <= 3950f
                && terrainObject.position.y >= -1300f
                && terrainObject.position.y <= 350f;
        }

        public static string Summary(int referenceCount, int fallbackCount)
        {
            return "ReferenceProps=loaded "
                + referenceCount.ToString(CultureInfo.InvariantCulture)
                + " fallback "
                + fallbackCount.ToString(CultureInfo.InvariantCulture);
        }

        public static string TerrainVisualCategoryFor(TerrainObjectSpawn terrainObject)
        {
            if (terrainObject == null)
            {
                return PropCategoryOther;
            }

            string text = SearchText(terrainObject.assetId, terrainObject.fileName, terrainObject.objectClass, terrainObject.specialType);
            if (ContainsAny(text, "shilone", "privatejet", "slayerp", "aircraft", "aero"))
            {
                return PropCategoryAircraft;
            }

            if (ContainsAny(text, "truckprop", "armoredcarprop", "trooptransportprop", "jeepprop", "dunebuggyprop", "ordnancetruckprop", "motorcycle"))
            {
                return PropCategoryVehicle;
            }

            if (ContainsAny(text, "barricade", "sandbag", "wirefence", "fence", "camonetting", "sawhorse"))
            {
                return PropCategoryBarricade;
            }

            if (ContainsAny(text, "runwaylight", "nav1", "crate", "junk", "dumpster", "mechpiece", "rock_"))
            {
                return PropCategorySmallProp;
            }

            if (ContainsAny(text, "palm", "maple", "oak", "tc1_"))
            {
                return PropCategoryTree;
            }

            if (string.Equals(terrainObject.objectClass, "BUILDING", StringComparison.OrdinalIgnoreCase)
                || ContainsAny(text, "hangar", "quonset", "portable", "tent", "tower", "dome", "genericmilitary", "hqtent"))
            {
                return PropCategoryBuilding;
            }

            if (string.Equals(terrainObject.objectClass, "TREE", StringComparison.OrdinalIgnoreCase))
            {
                return PropCategoryOther;
            }

            return PropCategoryOther;
        }

        private static float StructureVisualScaleMultiplierFor(StructureState structure)
        {
            if (structure == null)
            {
                return 0.36f;
            }

            if (ContainsAny(SearchText(structure.VisualAssetId, structure.ObjectProfile, structure.ObjectType), "hangar"))
            {
                return 0.38f;
            }

            return 0.36f;
        }

        private static float TerrainVisualScaleMultiplierFor(string category, TerrainObjectSpawn terrainObject)
        {
            if (string.Equals(category, PropCategoryBuilding, StringComparison.OrdinalIgnoreCase))
            {
                string text = SearchText(terrainObject?.assetId, terrainObject?.fileName);
                if (ContainsAny(text, "hangar", "quonset", "portable", "tower", "dome"))
                {
                    return 0.36f;
                }

                return 0.32f;
            }

            if (string.Equals(category, PropCategoryAircraft, StringComparison.OrdinalIgnoreCase))
            {
                return 0.34f;
            }

            if (string.Equals(category, PropCategoryVehicle, StringComparison.OrdinalIgnoreCase))
            {
                return 0.28f;
            }

            if (string.Equals(category, PropCategoryBarricade, StringComparison.OrdinalIgnoreCase))
            {
                return 0.24f;
            }

            if (string.Equals(category, PropCategoryTree, StringComparison.OrdinalIgnoreCase))
            {
                return 0.30f;
            }

            if (string.Equals(category, PropCategorySmallProp, StringComparison.OrdinalIgnoreCase))
            {
                return 0.22f;
            }

            return 0.28f;
        }

        private static void RecordPropVisualAudit(string category, bool loaded)
        {
            string normalizedCategory = string.IsNullOrWhiteSpace(category) ? PropCategoryOther : category;
            if (!PropVisualAudit.TryGetValue(normalizedCategory, out ReferencePropVisualAuditCounter counter))
            {
                counter = new ReferencePropVisualAuditCounter();
                PropVisualAudit[normalizedCategory] = counter;
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

        private static string PropAuditSegment(string category)
        {
            PropVisualAudit.TryGetValue(category, out ReferencePropVisualAuditCounter counter);
            int loaded = counter?.Loaded ?? 0;
            int fallback = counter?.Fallback ?? 0;
            return category
                + " "
                + loaded.ToString(CultureInfo.InvariantCulture)
                + "/"
                + fallback.ToString(CultureInfo.InvariantCulture);
        }

        private static string SearchText(params string[] values)
        {
            List<string> parts = new();
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    parts.Add(value.Trim().ToLowerInvariant());
                }
            }

            return string.Join(" ", parts);
        }

        private static bool ContainsAny(string value, params string[] fragments)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            foreach (string fragment in fragments)
            {
                if (!string.IsNullOrWhiteSpace(fragment)
                    && value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<string> Candidates(params string[] names)
        {
            HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
            foreach (string rawName in names)
            {
                string normalized = NormalizeAssetName(rawName);
                if (string.IsNullOrWhiteSpace(normalized) || !seen.Add(normalized))
                {
                    continue;
                }

                yield return normalized;

                foreach (string alias in AliasesFor(normalized))
                {
                    if (seen.Add(alias))
                    {
                        yield return alias;
                    }
                }
            }
        }

        private static IEnumerable<string> AliasesFor(string assetName)
        {
            if (string.Equals(assetName, "slayerp", StringComparison.OrdinalIgnoreCase))
            {
                yield return "slayerparked";
            }
        }

        private static string NormalizeAssetName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

                return Path.GetFileNameWithoutExtension(value.Trim());
        }

        private sealed class ReferencePropVisualAuditCounter
        {
            public int Loaded;
            public int Fallback;
        }
    }
}
