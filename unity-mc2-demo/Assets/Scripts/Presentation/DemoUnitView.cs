using MC2Demo.BattleCore;
using UnityEngine;

namespace MC2Demo.Presentation
{
    public sealed class DemoUnitView : MonoBehaviour
    {
        private const float MissionScale = 100f;

        public UnitState Unit { get; private set; }
        private Vector3 liveScale;
        private bool destroyedPoseApplied;

        public void Bind(UnitState unit)
        {
            Unit = unit;
            name = unit.Id + " " + unit.UnitType;
            liveScale = transform.localScale;
            ApplyPosition();
        }

        private void Update()
        {
            ApplyPosition();
            ApplyDamagePose();
        }

        private void ApplyPosition()
        {
            if (Unit == null)
            {
                return;
            }

            transform.position = MissionToWorld(Unit.MissionPosition);
        }

        private void ApplyDamagePose()
        {
            if (Unit == null || !Unit.IsDestroyed || destroyedPoseApplied)
            {
                return;
            }

            transform.localScale = new Vector3(liveScale.x, liveScale.y * 0.18f, liveScale.z);
            transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            destroyedPoseApplied = true;
        }

        public static Vector3 MissionToWorld(Vector2 missionPoint)
        {
            return new Vector3(missionPoint.x / MissionScale, 0f, missionPoint.y / MissionScale);
        }

        public static Vector2 WorldToMission(Vector3 worldPoint)
        {
            return new Vector2(worldPoint.x * MissionScale, worldPoint.z * MissionScale);
        }
    }
}
