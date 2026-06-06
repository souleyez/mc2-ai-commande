using System.Globalization;
using System.Collections.Generic;
using MC2Demo.BattleCore;
using UnityEngine;

namespace MC2Demo.Presentation
{
    public sealed class DemoTerrainView : MonoBehaviour
    {
        private const float MissionScale = 100f;
        private const float ElevationScale = 160f;

        public static DemoTerrainView Current { get; private set; }

        private TerrainMeshDefinition terrainMesh;
        private float waterElevation;

        public void Bind(TerrainMeshDefinition meshDefinition, float missionWaterElevation)
        {
            terrainMesh = meshDefinition;
            waterElevation = missionWaterElevation;
            Current = this;
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        public float SampleWorldHeight(Vector2 missionPoint)
        {
            if (terrainMesh == null || terrainMesh.samples == null || terrainMesh.samples.Length == 0)
            {
                return 0f;
            }

            int side = Mathf.Max(1, terrainMesh.sampleSide);
            float spacing = Mathf.Max(1f, terrainMesh.worldUnitsPerVertex * Mathf.Max(1, terrainMesh.sampleStep));
            float col = Mathf.Clamp((missionPoint.x - terrainMesh.minX) / spacing, 0f, side - 1f);
            float row = Mathf.Clamp((terrainMesh.minY - missionPoint.y) / spacing, 0f, side - 1f);

            int c0 = Mathf.FloorToInt(col);
            int r0 = Mathf.FloorToInt(row);
            int c1 = Mathf.Min(c0 + 1, side - 1);
            int r1 = Mathf.Min(r0 + 1, side - 1);
            float tx = col - c0;
            float ty = row - r0;

            float h00 = ElevationToWorldHeight(GetElevation(r0, c0, side));
            float h10 = ElevationToWorldHeight(GetElevation(r0, c1, side));
            float h01 = ElevationToWorldHeight(GetElevation(r1, c0, side));
            float h11 = ElevationToWorldHeight(GetElevation(r1, c1, side));

            float top = Mathf.Lerp(h00, h10, tx);
            float bottom = Mathf.Lerp(h01, h11, tx);
            return Mathf.Lerp(top, bottom, ty);
        }

        public bool ContainsMissionPoint(Vector2 missionPoint)
        {
            if (terrainMesh == null || terrainMesh.samples == null || terrainMesh.samples.Length == 0)
            {
                return true;
            }

            int side = Mathf.Max(1, terrainMesh.sampleSide);
            float spacing = Mathf.Max(1f, terrainMesh.worldUnitsPerVertex * Mathf.Max(1, terrainMesh.sampleStep));
            float maxX = terrainMesh.minX + spacing * (side - 1);
            float minY = terrainMesh.minY - spacing * (side - 1);
            return missionPoint.x >= terrainMesh.minX
                && missionPoint.x <= maxX
                && missionPoint.y <= terrainMesh.minY
                && missionPoint.y >= minY;
        }

        public bool CanLandAt(Vector2 missionPoint)
        {
            if (!ContainsMissionPoint(missionPoint))
            {
                return false;
            }

            TerrainMeshSample sample = NearestSample(missionPoint);
            return sample == null || (sample.water == 0 && sample.elevation > waterElevation + 4f);
        }

        public float WaterWorldHeight()
        {
            return ElevationToWorldHeight(waterElevation);
        }

        public string LandingAuditSummary()
        {
            if (terrainMesh == null || terrainMesh.samples == null || terrainMesh.samples.Length == 0)
            {
                return "Landing=terrain unavailable";
            }

            int flaggedWater = 0;
            int elevationBlocked = 0;
            for (int index = 0; index < terrainMesh.samples.Length; index++)
            {
                TerrainMeshSample sample = terrainMesh.samples[index];
                if (sample == null)
                {
                    continue;
                }

                if (sample.water != 0)
                {
                    flaggedWater++;
                }

                if (sample.elevation <= waterElevation + 4f)
                {
                    elevationBlocked++;
                }
            }

            int blocked = 0;
            for (int index = 0; index < terrainMesh.samples.Length; index++)
            {
                TerrainMeshSample sample = terrainMesh.samples[index];
                if (sample != null && (sample.water != 0 || sample.elevation <= waterElevation + 4f))
                {
                    blocked++;
                }
            }

            return "Landing=DemoTerrainView totalSamples="
                + terrainMesh.samples.Length.ToString(CultureInfo.InvariantCulture)
                + " blockedSamples="
                + blocked.ToString(CultureInfo.InvariantCulture)
                + " flaggedWater="
                + flaggedWater.ToString(CultureInfo.InvariantCulture)
                + " lowElevation="
                + elevationBlocked.ToString(CultureInfo.InvariantCulture)
                + " externalPredicate=water+mapBounds";
        }

        public List<Vector2> LandingReviewBlockedMarkers(int maxMarkers)
        {
            List<Vector2> markers = new();
            if (maxMarkers <= 0 || terrainMesh == null || terrainMesh.samples == null || terrainMesh.samples.Length == 0)
            {
                return markers;
            }

            int side = Mathf.Max(1, terrainMesh.sampleSide);
            float spacing = Mathf.Max(1f, terrainMesh.worldUnitsPerVertex * Mathf.Max(1, terrainMesh.sampleStep));
            int grid = Mathf.Clamp(Mathf.CeilToInt(Mathf.Sqrt(maxMarkers)), 1, side);
            for (int rowSlot = 0; rowSlot < grid && markers.Count < maxMarkers; rowSlot++)
            {
                for (int colSlot = 0; colSlot < grid && markers.Count < maxMarkers; colSlot++)
                {
                    int row = Mathf.Clamp(Mathf.RoundToInt((rowSlot + 0.5f) * side / grid), 0, side - 1);
                    int col = Mathf.Clamp(Mathf.RoundToInt((colSlot + 0.5f) * side / grid), 0, side - 1);
                    if (!TryFindBlockedSampleNear(row, col, side, out int blockedRow, out int blockedCol))
                    {
                        continue;
                    }

                    markers.Add(SampleMissionPoint(blockedRow, blockedCol, side, spacing));
                }
            }

            return markers;
        }

        public static Vector3 MissionToWorld(Vector2 missionPoint)
        {
            return new Vector3(missionPoint.x / MissionScale, 0f, missionPoint.y / MissionScale);
        }

        public static Vector2 WorldToMission(Vector3 worldPoint)
        {
            return new Vector2(worldPoint.x * MissionScale, worldPoint.z * MissionScale);
        }

        public static float HeightAt(Vector2 missionPoint)
        {
            return Current == null ? 0f : Current.SampleWorldHeight(missionPoint);
        }

        public static float WaterHeight()
        {
            return Current == null ? 0f : Current.WaterWorldHeight();
        }

        public static bool IsUsableLandingPosition(Vector2 missionPoint)
        {
            return Current == null || Current.CanLandAt(missionPoint);
        }

        public static string CurrentLandingAuditSummary()
        {
            return Current == null ? "Landing=DemoTerrainView unavailable" : Current.LandingAuditSummary();
        }

        public static List<Vector2> CurrentLandingReviewBlockedMarkers(int maxMarkers)
        {
            return Current == null ? new List<Vector2>() : Current.LandingReviewBlockedMarkers(maxMarkers);
        }

        public static float ElevationToWorldHeight(float elevation, float waterLevel = 350f)
        {
            return (elevation - waterLevel) / ElevationScale;
        }

        private float ElevationToWorldHeight(float elevation)
        {
            return ElevationToWorldHeight(elevation, waterElevation);
        }

        private float GetElevation(int row, int col, int side)
        {
            int index = Mathf.Clamp(row * side + col, 0, terrainMesh.samples.Length - 1);
            return terrainMesh.samples[index].elevation;
        }

        private TerrainMeshSample NearestSample(Vector2 missionPoint)
        {
            int side = Mathf.Max(1, terrainMesh.sampleSide);
            float spacing = Mathf.Max(1f, terrainMesh.worldUnitsPerVertex * Mathf.Max(1, terrainMesh.sampleStep));
            int col = Mathf.RoundToInt(Mathf.Clamp((missionPoint.x - terrainMesh.minX) / spacing, 0f, side - 1f));
            int row = Mathf.RoundToInt(Mathf.Clamp((terrainMesh.minY - missionPoint.y) / spacing, 0f, side - 1f));
            int index = Mathf.Clamp(row * side + col, 0, terrainMesh.samples.Length - 1);
            return terrainMesh.samples[index];
        }

        private bool TryFindBlockedSampleNear(int row, int col, int side, out int blockedRow, out int blockedCol)
        {
            for (int radius = 0; radius <= 4; radius++)
            {
                int minRow = Mathf.Max(0, row - radius);
                int maxRow = Mathf.Min(side - 1, row + radius);
                int minCol = Mathf.Max(0, col - radius);
                int maxCol = Mathf.Min(side - 1, col + radius);
                for (int sampleRow = minRow; sampleRow <= maxRow; sampleRow++)
                {
                    for (int sampleCol = minCol; sampleCol <= maxCol; sampleCol++)
                    {
                        TerrainMeshSample sample = terrainMesh.samples[Mathf.Clamp(sampleRow * side + sampleCol, 0, terrainMesh.samples.Length - 1)];
                        if (!IsLandingBlockedSample(sample))
                        {
                            continue;
                        }

                        blockedRow = sampleRow;
                        blockedCol = sampleCol;
                        return true;
                    }
                }
            }

            blockedRow = 0;
            blockedCol = 0;
            return false;
        }

        private bool IsLandingBlockedSample(TerrainMeshSample sample)
        {
            return sample != null && (sample.water != 0 || sample.elevation <= waterElevation + 4f);
        }

        private Vector2 SampleMissionPoint(int row, int col, int side, float spacing)
        {
            int clampedRow = Mathf.Clamp(row, 0, side - 1);
            int clampedCol = Mathf.Clamp(col, 0, side - 1);
            return new Vector2(
                terrainMesh.minX + clampedCol * spacing,
                terrainMesh.minY - clampedRow * spacing);
        }
    }
}
