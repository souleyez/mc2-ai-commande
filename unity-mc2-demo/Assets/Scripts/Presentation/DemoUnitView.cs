using MC2Demo.BattleCore;
using UnityEngine;

namespace MC2Demo.Presentation
{
    public sealed class DemoUnitView : MonoBehaviour
    {
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

            Vector3 position = MissionToWorld(Unit.MissionPosition);
            position.y = DemoTerrainView.HeightAt(Unit.MissionPosition) + Mathf.Max(0.2f, transform.localScale.y * 0.5f);
            if (Unit.IsJumping)
            {
                position.y += Unit.JumpLift * 2.6f;
            }

            transform.position = position;
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
            return DemoTerrainView.MissionToWorld(missionPoint);
        }

        public static Vector2 WorldToMission(Vector3 worldPoint)
        {
            return DemoTerrainView.WorldToMission(worldPoint);
        }
    }
}
