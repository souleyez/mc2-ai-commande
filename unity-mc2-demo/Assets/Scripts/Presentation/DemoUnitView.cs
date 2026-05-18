using MC2Demo.BattleCore;
using UnityEngine;

namespace MC2Demo.Presentation
{
    public sealed class DemoUnitView : MonoBehaviour
    {
        public UnitState Unit { get; private set; }
        private Vector3 liveScale;
        private Renderer unitRenderer;
        private Color liveColor = Color.white;
        private Color hitFlashColor = Color.white;
        private float hitFlashRemaining;
        private float hitFlashDuration = 0.18f;
        private bool destroyedPoseApplied;

        public void Bind(UnitState unit)
        {
            Unit = unit;
            name = unit.Id + " " + unit.UnitType;
            liveScale = transform.localScale;
            unitRenderer = GetComponent<Renderer>();
            if (unitRenderer != null && unitRenderer.sharedMaterial != null)
            {
                liveColor = unitRenderer.sharedMaterial.color;
            }

            ApplyPosition();
        }

        private void Update()
        {
            ApplyPosition();
            ApplyHitFlash();
            ApplyDamagePose();
        }

        public void PulseHit(Color color, float duration = 0.18f)
        {
            if (Unit == null || Unit.IsDestroyed)
            {
                return;
            }

            hitFlashColor = color;
            hitFlashDuration = Mathf.Max(0.05f, duration);
            hitFlashRemaining = hitFlashDuration;
            ApplyHitFlash();
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

        private void ApplyHitFlash()
        {
            if (unitRenderer == null || unitRenderer.sharedMaterial == null || Unit == null || Unit.IsDestroyed)
            {
                return;
            }

            if (hitFlashRemaining <= 0f)
            {
                unitRenderer.sharedMaterial.color = liveColor;
                return;
            }

            float t = Mathf.Clamp01(hitFlashRemaining / hitFlashDuration);
            unitRenderer.sharedMaterial.color = Color.Lerp(liveColor, hitFlashColor, t);
            hitFlashRemaining = Mathf.Max(0f, hitFlashRemaining - Time.deltaTime);
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
