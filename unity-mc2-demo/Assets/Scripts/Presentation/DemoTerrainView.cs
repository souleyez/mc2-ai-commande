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
    }
}
