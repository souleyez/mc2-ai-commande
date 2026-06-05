using System;
using System.Collections.Generic;
using System.IO;
using MC2Demo.BattleCore;
using UnityEngine;

namespace MC2Demo.Presentation
{
    internal static class ReferencePropLibrary
    {
        private const int FirstSliceTerrainObjectLimit = 260;

        public static bool TryAttachStructure(StructureState structure, Transform parent, Color fallbackColor, out Renderer renderer)
        {
            renderer = null;
            if (structure == null || parent == null)
            {
                return false;
            }

            foreach (string candidate in Candidates(structure.VisualAssetId, structure.ObjectProfile, structure.ObjectType))
            {
                if (ReferenceObjMeshLibrary.TryAttachReferenceAsset(
                    candidate,
                    parent,
                    fallbackColor,
                    useTeamColor: false,
                    isPlayerTeam: structure.TeamId == 0,
                    visualScaleMultiplier: 0.36f,
                    compensateParentScale: true,
                    groundOffsetY: 0f,
                    out renderer))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TryAttachTerrainObject(TerrainObjectSpawn terrainObject, Transform parent, Color fallbackColor, out Renderer renderer)
        {
            renderer = null;
            if (terrainObject == null || parent == null || !ShouldUseReferenceTerrainObject(terrainObject))
            {
                return false;
            }

            foreach (string candidate in Candidates(terrainObject.assetId, terrainObject.fileName))
            {
                if (ReferenceObjMeshLibrary.TryAttachReferenceAsset(
                    candidate,
                    parent,
                    fallbackColor,
                    useTeamColor: false,
                    isPlayerTeam: terrainObject.teamId == 0,
                    visualScaleMultiplier: 0.36f,
                    compensateParentScale: true,
                    groundOffsetY: 0f,
                    out renderer))
                {
                    return true;
                }
            }

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
                + referenceCount.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + " fallback "
                + fallbackCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
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
    }
}
