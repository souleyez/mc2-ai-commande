using System;
using System.Collections.Generic;
using MC2Demo.BattleCore;
using UnityEngine;

namespace MC2Demo.Presentation
{
    public sealed class DemoUnitView : MonoBehaviour
    {
        private const float CriticalSectionRatio = 0.35f;
        private const float HeatCueRatio = 0.62f;

        public UnitState Unit { get; private set; }
        private Vector3 liveScale;
        private Renderer unitRenderer;
        private Color liveColor = Color.white;
        private Color hitFlashColor = Color.white;
        private float hitFlashRemaining;
        private float hitFlashDuration = 0.18f;
        private bool destroyedPoseApplied;
        private bool legFailurePoseApplied;
        private bool ejectionEffectSpawned;
        private bool wasJumping;
        private bool wasHeatLocked;
        private float jetTrailCooldown;
        private GameObject heatVentCue;
        private GameObject heatLockCue;
        private GameObject sectionDamageRingCue;
        private GameObject sectionDamageBeaconCue;
        private readonly Dictionary<string, GameObject> sectionParts = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> destroyedSections = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> criticalSections = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<Material> ownedMaterials = new();

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

            CreateSectionParts();
            CreateHeatCueParts();
            ApplyPosition();
        }

        private void Update()
        {
            ApplyPosition();
            ApplyJetVisuals();
            ApplyHeatVisuals();
            ApplyHitFlash();
            ApplySectionPartDamageColors();
            ApplySectionDamageVisuals();
            ApplySectionDamageGroundCue();
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

        private void ApplyJetVisuals()
        {
            if (Unit == null || Unit.IsDestroyed)
            {
                wasJumping = Unit?.IsJumping == true;
                return;
            }

            if (Unit.IsJumping && !wasJumping)
            {
                SpawnJetTakeoffCue();
                jetTrailCooldown = 0f;
            }

            if (Unit.IsJumping)
            {
                jetTrailCooldown -= Time.deltaTime;
                if (jetTrailCooldown <= 0f)
                {
                    SpawnJetTrailCue();
                    jetTrailCooldown = 0.09f;
                }
            }
            else if (wasJumping)
            {
                SpawnJetLandingCue();
            }

            wasJumping = Unit.IsJumping;
        }

        private void ApplyHeatVisuals()
        {
            if (Unit == null || Unit.IsDestroyed)
            {
                SetHeatCueVisible(false, false);
                wasHeatLocked = Unit?.IsHeatLocked == true;
                return;
            }

            float heatRatio = Mathf.Clamp01(Unit.HeatRatio);
            bool heatLocked = Unit.IsHeatLocked;
            bool heatVisible = heatLocked || heatRatio >= HeatCueRatio;
            SetHeatCueVisible(heatVisible, heatLocked);

            if (heatLocked && !wasHeatLocked)
            {
                SpawnHeatLockPulse();
            }

            if (heatVisible && heatVentCue != null)
            {
                float pulse = 0.88f + Mathf.Sin(Time.time * (heatLocked ? 8.5f : 5.0f)) * 0.12f;
                float width = Mathf.Lerp(0.62f, 0.92f, heatRatio) * pulse;
                heatVentCue.transform.localScale = new Vector3(width, 0.016f, width);
                SetCueColor(heatVentCue, new Color(1f, Mathf.Lerp(0.42f, 0.16f, heatRatio), 0.06f, Mathf.Lerp(0.34f, 0.78f, heatRatio)));
            }

            if (heatLocked && heatLockCue != null)
            {
                float pulse = 0.85f + Mathf.Sin(Time.time * 10f) * 0.15f;
                heatLockCue.transform.localScale = new Vector3(0.13f * pulse, 0.46f + heatRatio * 0.22f, 0.13f * pulse);
                SetCueColor(heatLockCue, new Color(1f, 0.11f, 0.04f, 0.82f));
            }

            wasHeatLocked = heatLocked;
        }

        private void ApplySectionDamageVisuals()
        {
            if (Unit == null || Unit.Sections == null)
            {
                return;
            }

            for (int index = 0; index < Unit.Sections.Length; index++)
            {
                DamageSection section = Unit.Sections[index];
                if (section.IsDestroyed)
                {
                    if (destroyedSections.Add(section.Name))
                    {
                        SpawnSectionDestroyedEffect(section.Name);
                    }

                    continue;
                }

                if (section.Ratio < CriticalSectionRatio && criticalSections.Add(section.Name))
                {
                    SpawnCriticalSectionEffect(section.Name);
                }
            }
        }

        private void ApplySectionPartDamageColors()
        {
            if (Unit == null || Unit.Sections == null)
            {
                return;
            }

            for (int index = 0; index < Unit.Sections.Length; index++)
            {
                DamageSection section = Unit.Sections[index];
                ApplySectionPartDamageColor(section.Name, SectionDamageColor(section));
            }
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
            if (Unit == null)
            {
                return;
            }

            if (!Unit.IsDestroyed && !legFailurePoseApplied && HasDestroyedSection("Legs"))
            {
                transform.localScale = new Vector3(liveScale.x, liveScale.y * 0.62f, liveScale.z);
                transform.rotation = Quaternion.Euler(0f, 0f, Unit.IsPlayerUnit ? -7f : 7f);
                legFailurePoseApplied = true;
            }

            if (!Unit.IsDestroyed || destroyedPoseApplied)
            {
                return;
            }

            Vector3 wreckCenter = transform.position;
            transform.localScale = new Vector3(liveScale.x, liveScale.y * 0.18f, liveScale.z);
            transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            SpawnDestroyedUnitCue(wreckCenter);
            destroyedPoseApplied = true;
        }

        private void CreateSectionParts()
        {
            if (unitRenderer == null || unitRenderer.sharedMaterial == null)
            {
                return;
            }

            CreateSectionPart("Cockpit", PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0.30f), new Vector3(0.20f, 0.16f, 0.20f));
            CreateSectionPart("Left Arm", PrimitiveType.Cube, new Vector3(-0.58f, 0.05f, 0f), new Vector3(0.16f, 0.42f, 0.18f));
            CreateSectionPart("Right Arm", PrimitiveType.Cube, new Vector3(0.58f, 0.05f, 0f), new Vector3(0.16f, 0.42f, 0.18f));
            CreateSectionPart("Left Leg", PrimitiveType.Cube, new Vector3(-0.20f, -0.52f, 0f), new Vector3(0.14f, 0.28f, 0.16f));
            CreateSectionPart("Right Leg", PrimitiveType.Cube, new Vector3(0.20f, -0.52f, 0f), new Vector3(0.14f, 0.28f, 0.16f));
        }

        private void CreateHeatCueParts()
        {
            heatVentCue = CreateHeatCuePart("Heat Vent Ring", PrimitiveType.Cylinder, new Vector3(0f, -0.46f, 0f), new Vector3(0.70f, 0.016f, 0.70f), new Color(1f, 0.40f, 0.06f, 0.36f));
            heatLockCue = CreateHeatCuePart("Heat Lock Beacon", PrimitiveType.Cylinder, new Vector3(0f, 0.78f, 0.04f), new Vector3(0.13f, 0.46f, 0.13f), new Color(1f, 0.11f, 0.04f, 0.82f));
            sectionDamageRingCue = CreateHeatCuePart("Section Damage Ground Ring", PrimitiveType.Cylinder, new Vector3(0f, -0.58f, 0f), new Vector3(0.84f, 0.014f, 0.84f), new Color(1f, 0.52f, 0.08f, 0.34f));
            sectionDamageBeaconCue = CreateHeatCuePart("Section Damage Beacon", PrimitiveType.Cylinder, new Vector3(0f, 0.64f, 0.10f), new Vector3(0.052f, 0.34f, 0.052f), new Color(1f, 0.34f, 0.08f, 0.62f));
            SetHeatCueVisible(false, false);
            SetSectionDamageGroundCueVisible(false);
        }

        private void CreateSectionPart(string sectionName, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            part.name = Unit.Id + " " + sectionName;
            part.transform.SetParent(transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Renderer partRenderer = part.GetComponent<Renderer>();
            if (partRenderer != null)
            {
                partRenderer.sharedMaterial = CreateOwnedMaterial(liveColor);
            }

            sectionParts[sectionName] = part;
        }

        private GameObject CreateHeatCuePart(string cueName, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject cue = GameObject.CreatePrimitive(primitive);
            cue.name = Unit.Id + " " + cueName;
            cue.transform.SetParent(transform, false);
            cue.transform.localPosition = localPosition;
            cue.transform.localScale = localScale;
            Renderer renderer = cue.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateOwnedTransparentMaterial(color);
            }

            Collider collider = cue.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            return cue;
        }

        private void SpawnSectionDestroyedEffect(string sectionName)
        {
            if (string.Equals(sectionName, "Cockpit", StringComparison.OrdinalIgnoreCase))
            {
                CreateDamageScar("Cockpit Breach", new Vector3(0f, 0.45f, 0.34f), new Vector3(0.32f, 0.08f, 0.24f), new Color(0.02f, 0.02f, 0.02f, 1f));
                CreateDamageBeacon("Cockpit Ejection Beacon", new Vector3(0f, 0.98f, 0.36f), new Color(0.58f, 0.92f, 1f, 1f), 0.055f, 0.42f);
                CreateDamageFlag("Cockpit Lost Flag", new Vector3(0f, 1.12f, 0.18f), new Vector3(0.46f, 0.07f, 0.11f), new Color(0.62f, 0.95f, 1f, 1f));
                CreateDamageLink("Cockpit Ejection Rail", new Vector3(-0.16f, 0.64f, 0.36f), new Vector3(0.16f, 1.05f, 0.46f), new Color(0.58f, 0.92f, 1f, 0.92f), 0.018f);
                DetachPart("Cockpit", new Vector3(0f, 0.8f, 0.45f), new Color(1f, 0.65f, 0.18f, 0.72f));
                SpawnCockpitEjection();
                return;
            }

            if (string.Equals(sectionName, "Left Arm", StringComparison.OrdinalIgnoreCase))
            {
                CreateDamageScar("Left Shoulder Breach", new Vector3(-0.44f, 0.10f, 0.02f), new Vector3(0.16f, 0.34f, 0.20f), new Color(0.06f, 0.04f, 0.02f, 1f));
                CreateDamageBeacon("Left Arm Missing Beacon", new Vector3(-0.58f, 0.42f, 0.06f), new Color(1f, 0.34f, 0.08f, 1f), 0.045f, 0.32f);
                CreateDamageFlag("Left Arm Lost Flag", new Vector3(-0.32f, 0.92f, 0.12f), new Vector3(0.28f, 0.08f, 0.10f), new Color(1f, 0.32f, 0.08f, 1f));
                CreateDamageLink("Left Broken Cable", new Vector3(-0.48f, 0.25f, 0.08f), new Vector3(-0.78f, 0.02f, 0.20f), new Color(1f, 0.52f, 0.12f, 0.92f), 0.015f);
                DetachPart("Left Arm", new Vector3(-0.9f, 0.25f, 0.15f), new Color(1f, 0.42f, 0.08f, 0.68f));
                SpawnDamageBurst(SectionWorldPoint(new Vector3(-0.62f, 0.08f, 0f)), 0.8f);
                return;
            }

            if (string.Equals(sectionName, "Right Arm", StringComparison.OrdinalIgnoreCase))
            {
                CreateDamageScar("Right Shoulder Breach", new Vector3(0.44f, 0.10f, 0.02f), new Vector3(0.16f, 0.34f, 0.20f), new Color(0.06f, 0.04f, 0.02f, 1f));
                CreateDamageBeacon("Right Arm Missing Beacon", new Vector3(0.58f, 0.42f, 0.06f), new Color(1f, 0.34f, 0.08f, 1f), 0.045f, 0.32f);
                CreateDamageFlag("Right Arm Lost Flag", new Vector3(0.32f, 0.92f, 0.12f), new Vector3(0.28f, 0.08f, 0.10f), new Color(1f, 0.32f, 0.08f, 1f));
                CreateDamageLink("Right Broken Cable", new Vector3(0.48f, 0.25f, 0.08f), new Vector3(0.78f, 0.02f, 0.20f), new Color(1f, 0.52f, 0.12f, 0.92f), 0.015f);
                DetachPart("Right Arm", new Vector3(0.9f, 0.25f, 0.15f), new Color(1f, 0.42f, 0.08f, 0.68f));
                SpawnDamageBurst(SectionWorldPoint(new Vector3(0.62f, 0.08f, 0f)), 0.8f);
                return;
            }

            if (string.Equals(sectionName, "Legs", StringComparison.OrdinalIgnoreCase))
            {
                CreateDamageScar("Leg Failure Scorch", new Vector3(0f, -0.36f, 0.05f), new Vector3(0.52f, 0.12f, 0.26f), new Color(0.05f, 0.04f, 0.03f, 1f));
                CreateDamageBeacon("Leg Failure Beacon", new Vector3(0f, -0.10f, 0.12f), new Color(1f, 0.22f, 0.10f, 1f), 0.06f, 0.30f);
                CreateDamageFlag("Leg Failure Flag", new Vector3(0f, 0.78f, 0.14f), new Vector3(0.54f, 0.08f, 0.10f), new Color(1f, 0.12f, 0.08f, 1f));
                CreateDamageFlag("Leg Failure Crossbar", new Vector3(0f, -0.28f, 0.18f), new Vector3(0.70f, 0.08f, 0.10f), new Color(1f, 0.16f, 0.08f, 1f));
                CreateDamageLink("Left Leg Broken Cable", new Vector3(-0.18f, -0.28f, 0.08f), new Vector3(-0.42f, -0.58f, 0.18f), new Color(1f, 0.42f, 0.10f, 0.90f), 0.014f);
                CreateDamageLink("Right Leg Broken Cable", new Vector3(0.18f, -0.28f, 0.08f), new Vector3(0.42f, -0.58f, 0.18f), new Color(1f, 0.42f, 0.10f, 0.90f), 0.014f);
                DetachPart("Left Leg", new Vector3(-0.38f, 0.12f, -0.25f), new Color(0.82f, 0.32f, 0.12f, 0.66f));
                DetachPart("Right Leg", new Vector3(0.38f, 0.12f, -0.25f), new Color(0.82f, 0.32f, 0.12f, 0.66f));
                SpawnLegFailureGroundCues();
                SpawnDamageBurst(SectionWorldPoint(new Vector3(0f, -0.45f, 0f)), 1.0f);
                return;
            }

            CreateDamageScar("Core Damage Scorch", new Vector3(0f, 0.10f, 0.16f), new Vector3(0.40f, 0.12f, 0.26f), new Color(0.05f, 0.04f, 0.03f, 1f));
            SpawnDamageBurst(SectionWorldPoint(new Vector3(0f, 0.08f, 0f)), 1.15f);
        }

        private void DetachPart(string partName, Vector3 localImpulse, Color color)
        {
            if (!sectionParts.TryGetValue(partName, out GameObject part) || part == null)
            {
                return;
            }

            sectionParts.Remove(partName);
            Vector3 startScale = part.transform.lossyScale;
            Vector3 startPosition = part.transform.position;
            Vector3 endPosition = startPosition + transform.TransformDirection(localImpulse);
            part.transform.SetParent(null, true);
            part.transform.position = startPosition;
            part.transform.rotation = Quaternion.Euler(18f, partName.GetHashCode() % 360, 72f);

            Collider partCollider = part.GetComponent<Collider>();
            if (partCollider != null)
            {
                Destroy(partCollider);
            }

            DemoTransientEffect transient = part.AddComponent<DemoTransientEffect>();
            transient.BeginMoving(color, 2.85f, startScale, startScale * 0.62f, endPosition);
            if (IsDetachedArmPart(partName))
            {
                SpawnDetachedArmCues(startPosition, endPosition, color);
            }
        }

        private void SpawnDetachedArmCues(Vector3 startPosition, Vector3 endPosition, Color color)
        {
            Vector3 apex = Vector3.Lerp(startPosition, endPosition, 0.52f) + Vector3.up * 0.38f;
            Vector3 landing = GroundAtWorld(endPosition);
            Vector3 side = transform.TransformDirection(endPosition.x >= startPosition.x ? Vector3.right : Vector3.left);
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            CreateBeam("Detached Arm Flight Trail A", startPosition, apex, new Color(0.96f, 0.54f, 0.16f, 0.46f), 0.92f, 0.020f);
            CreateBeam("Detached Arm Flight Trail B", apex, endPosition, new Color(0.10f, 0.09f, 0.08f, 0.34f), 1.12f, 0.030f);
            CreateMovingTransient(
                "Detached Arm Spark",
                PrimitiveType.Sphere,
                startPosition + Vector3.up * 0.05f,
                endPosition,
                new Color(1f, 0.58f, 0.12f, 0.70f),
                0.78f,
                Vector3.one * 0.075f,
                Vector3.one * 0.24f);
            CreateTransient(
                "Detached Arm Landing Dust",
                PrimitiveType.Cylinder,
                landing,
                new Color(0.62f, 0.52f, 0.38f, 0.34f),
                1.30f,
                new Vector3(0.20f, 0.012f, 0.20f),
                new Vector3(0.72f, 0.006f, 0.72f));
            CreateMovingTransient(
                "Detached Arm Landing Debris",
                PrimitiveType.Cube,
                endPosition,
                landing + side * 0.30f + Vector3.up * 0.08f,
                new Color(color.r, color.g, color.b, 0.58f),
                1.45f,
                Vector3.one * 0.085f,
                Vector3.one * 0.035f);
            CreateMovingTransient(
                "Detached Arm Hot Fragment",
                PrimitiveType.Cube,
                endPosition + Vector3.up * 0.04f,
                landing - side * 0.18f + forward * 0.22f + Vector3.up * 0.06f,
                new Color(1f, 0.36f, 0.08f, 0.64f),
                1.20f,
                Vector3.one * 0.070f,
                Vector3.one * 0.030f);
        }

        private static bool IsDetachedArmPart(string partName)
        {
            return !string.IsNullOrWhiteSpace(partName)
                && partName.IndexOf("Arm", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void SpawnLegFailureGroundCues()
        {
            Vector3 center = GroundAtWorld(SectionWorldPoint(new Vector3(0f, -0.56f, 0f)));
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Quaternion forwardYaw = Quaternion.LookRotation(forward, Vector3.up);
            Color scorch = new(0.07f, 0.045f, 0.030f, 0.92f);
            Color signal = new(1f, 0.18f, 0.06f, 0.92f);
            CreateWorldDamageObject("Leg Collapse Skid L", PrimitiveType.Cube, center - right * 0.22f - forward * 0.20f, forwardYaw, new Vector3(0.11f, 0.018f, 0.66f), scorch);
            CreateWorldDamageObject("Leg Collapse Skid R", PrimitiveType.Cube, center + right * 0.22f - forward * 0.18f, forwardYaw, new Vector3(0.11f, 0.018f, 0.58f), scorch);
            CreateWorldDamageObject("Leg Immobilized Ground Bar", PrimitiveType.Cube, center + Vector3.up * 0.035f, forwardYaw * Quaternion.Euler(0f, 90f, 0f), new Vector3(0.84f, 0.026f, 0.070f), signal);
            CreateTransient("Leg Collapse Dust Ring", PrimitiveType.Cylinder, center, new Color(0.62f, 0.52f, 0.38f, 0.34f), 1.35f, new Vector3(0.34f, 0.012f, 0.34f), new Vector3(1.22f, 0.006f, 1.22f));
            CreateMovingTransient("Leg Drag Spark", PrimitiveType.Sphere, center + right * 0.20f + Vector3.up * 0.10f, center - forward * 0.46f + right * 0.08f + Vector3.up * 0.05f, new Color(1f, 0.44f, 0.08f, 0.66f), 0.85f, Vector3.one * 0.07f, Vector3.one * 0.23f);
        }

        private void SpawnDamageBurst(Vector3 position, float scale)
        {
            CreateTransient("Section Spark", PrimitiveType.Sphere, position, new Color(1f, 0.50f, 0.08f, 0.72f), 0.42f, Vector3.one * (0.10f * scale), Vector3.one * (0.70f * scale));
            CreateTransient("Section Smoke", PrimitiveType.Sphere, position + Vector3.up * 0.18f, new Color(0.10f, 0.10f, 0.10f, 0.45f), 1.25f, Vector3.one * (0.18f * scale), Vector3.one * (0.95f * scale));
        }

        private void SpawnDestroyedUnitCue(Vector3 wreckCenter)
        {
            Vector3 ground = wreckCenter;
            if (Unit != null)
            {
                ground = MissionToWorld(Unit.MissionPosition);
                ground.y = DemoTerrainView.HeightAt(Unit.MissionPosition) + 0.035f;
            }

            CreateTransient("Mech Wreck Blast", PrimitiveType.Sphere, wreckCenter + Vector3.up * 0.18f, new Color(1f, 0.42f, 0.08f, 0.72f), 0.55f, Vector3.one * 0.20f, new Vector3(1.35f, 0.85f, 1.35f));
            CreateTransient("Mech Wreck Smoke", PrimitiveType.Sphere, wreckCenter + Vector3.up * 0.48f, new Color(0.08f, 0.08f, 0.08f, 0.52f), 1.65f, Vector3.one * 0.28f, new Vector3(1.15f, 1.80f, 1.15f));
            SpawnWreckDebris(wreckCenter);
            CreateWorldDamageObject("Wreck Scorch", PrimitiveType.Cylinder, ground, Quaternion.identity, new Vector3(0.72f, 0.018f, 0.72f), new Color(0.04f, 0.03f, 0.02f, 0.95f));
            CreateWorldDamageObject("Wreck Heat Beacon", PrimitiveType.Cylinder, ground + Vector3.up * 0.20f, Quaternion.identity, new Vector3(0.06f, 0.22f, 0.06f), new Color(1f, 0.20f, 0.05f, 1f));
            CreateWorldDamageObject("Wreck Marker", PrimitiveType.Cube, ground + Vector3.up * 0.04f, Quaternion.identity, new Vector3(0.80f, 0.030f, 0.12f), new Color(1f, 0.16f, 0.08f, 1f));
            CreateWorldDamageObject("Wreck Marker Crossbar", PrimitiveType.Cube, ground + Vector3.up * 0.045f, Quaternion.Euler(0f, 90f, 0f), new Vector3(0.68f, 0.030f, 0.10f), new Color(1f, 0.24f, 0.06f, 1f));
            if (Unit != null && !Unit.IsPlayerUnit)
            {
                SpawnEnemySalvageCue(ground);
            }
        }

        private void SpawnEnemySalvageCue(Vector3 ground)
        {
            Color signal = new(0.28f, 0.95f, 0.62f, 0.92f);
            CreateWorldDamageObject("Enemy Salvage Ring", PrimitiveType.Cylinder, ground + Vector3.up * 0.025f, Quaternion.identity, new Vector3(0.46f, 0.010f, 0.46f), new Color(signal.r, signal.g, signal.b, 0.38f));
            CreateWorldDamageObject("Enemy Salvage Tag", PrimitiveType.Cube, ground + Vector3.up * 0.35f, Quaternion.identity, new Vector3(0.34f, 0.040f, 0.10f), signal);
            CreateWorldDamageObject("Enemy Salvage Tag Crossbar", PrimitiveType.Cube, ground + Vector3.up * 0.37f, Quaternion.Euler(0f, 90f, 0f), new Vector3(0.26f, 0.034f, 0.085f), new Color(signal.r, signal.g, signal.b, 0.84f));
            CreateWorldDamageObject("Enemy Salvage Beacon", PrimitiveType.Cylinder, ground + Vector3.up * 0.58f, Quaternion.identity, new Vector3(0.040f, 0.30f, 0.040f), new Color(signal.r, signal.g, signal.b, 0.82f));
        }

        private void SpawnWreckDebris(Vector3 wreckCenter)
        {
            Vector3 center = wreckCenter + Vector3.up * 0.22f;
            Color hot = new(1f, 0.46f, 0.10f, 0.78f);
            Color metal = new(0.62f, 0.66f, 0.68f, 0.82f);
            SpawnWreckDebrisPiece(center, new Vector3(0.86f, 0.34f, 0.42f), hot, 0.12f, 2.1f);
            SpawnWreckDebrisPiece(center, new Vector3(-0.78f, 0.26f, 0.34f), metal, 0.10f, 2.0f);
            SpawnWreckDebrisPiece(center, new Vector3(0.28f, 0.42f, -0.84f), hot, 0.09f, 1.9f);
            SpawnWreckDebrisPiece(center, new Vector3(-0.36f, 0.22f, -0.72f), metal, 0.08f, 1.8f);
        }

        private void SpawnWreckDebrisPiece(Vector3 center, Vector3 localImpulse, Color color, float size, float duration)
        {
            Vector3 target = center + transform.TransformDirection(localImpulse);
            CreateMovingTransient("Mech Wreck Debris", PrimitiveType.Cube, center, target, color, duration, Vector3.one * size, Vector3.one * (size * 0.35f));
        }

        private void SpawnCriticalSectionEffect(string sectionName)
        {
            Vector3 local = CriticalSectionLocalPoint(sectionName);
            CreateDamagePuff("Critical Section Smoke", local + Vector3.up * 0.10f, new Vector3(0.18f, 0.12f, 0.18f), new Color(0.12f, 0.11f, 0.10f, 0.72f));
            CreateDamagePuff("Critical Section Smoke Vent", local + Vector3.up * 0.26f + Vector3.forward * 0.03f, new Vector3(0.11f, 0.18f, 0.11f), new Color(0.08f, 0.08f, 0.08f, 0.56f));
            CreateDamageFlag("Critical Section Spark", local + Vector3.up * 0.34f, new Vector3(0.18f, 0.04f, 0.06f), new Color(1f, 0.46f, 0.08f, 1f));
        }

        private void ApplySectionDamageGroundCue()
        {
            string cue = SectionDamageGroundCueMode();
            bool visible = !string.Equals(cue, "none", StringComparison.Ordinal);
            SetSectionDamageGroundCueVisible(visible);
            if (!visible)
            {
                return;
            }

            float pulseSpeed = string.Equals(cue, "pilot", StringComparison.Ordinal) ? 5.8f : string.Equals(cue, "lost", StringComparison.Ordinal) ? 4.8f : 3.7f;
            float pulseRange = string.Equals(cue, "critical", StringComparison.Ordinal) ? 0.055f : 0.085f;
            float pulse = 0.96f + Mathf.Sin(Time.time * pulseSpeed) * pulseRange;
            float baseScale = string.Equals(cue, "pilot", StringComparison.Ordinal) ? 1.16f : string.Equals(cue, "lost", StringComparison.Ordinal) ? 1.04f : 0.92f;
            Color cueColor = SectionDamageGroundCueColor(cue);
            if (sectionDamageRingCue != null)
            {
                sectionDamageRingCue.transform.localPosition = new Vector3(0f, -0.58f, 0f);
                sectionDamageRingCue.transform.localRotation = Quaternion.Inverse(transform.rotation);
                sectionDamageRingCue.transform.localScale = new Vector3(baseScale * pulse, 0.014f, baseScale * pulse);
                SetCueColor(sectionDamageRingCue, new Color(cueColor.r, cueColor.g, cueColor.b, string.Equals(cue, "critical", StringComparison.Ordinal) ? 0.30f : 0.42f));
            }

            if (sectionDamageBeaconCue != null)
            {
                sectionDamageBeaconCue.transform.localPosition = new Vector3(0f, 0.64f, 0.10f);
                sectionDamageBeaconCue.transform.localScale = new Vector3(0.052f * pulse, 0.34f + pulse * 0.05f, 0.052f * pulse);
                SetCueColor(sectionDamageBeaconCue, new Color(cueColor.r, cueColor.g, cueColor.b, string.Equals(cue, "critical", StringComparison.Ordinal) ? 0.54f : 0.72f));
            }
        }

        private void SetSectionDamageGroundCueVisible(bool visible)
        {
            if (sectionDamageRingCue != null)
            {
                sectionDamageRingCue.SetActive(visible);
            }

            if (sectionDamageBeaconCue != null)
            {
                sectionDamageBeaconCue.SetActive(visible);
            }
        }

        private string SectionDamageGroundCueMode()
        {
            if (Unit == null || Unit.Sections == null)
            {
                return "none";
            }

            bool hasCritical = false;
            bool hasLost = false;
            bool hasPilotRisk = false;
            for (int index = 0; index < Unit.Sections.Length; index++)
            {
                DamageSection section = Unit.Sections[index];
                if (section.IsDestroyed)
                {
                    hasLost = true;
                    if (section.IsCritical || IsSectionName(section.Name, "Cockpit") || IsSectionName(section.Name, "Turret"))
                    {
                        hasPilotRisk = true;
                    }

                    continue;
                }

                if (section.Ratio < CriticalSectionRatio)
                {
                    hasCritical = true;
                }
            }

            if (hasPilotRisk)
            {
                return "pilot";
            }

            if (hasLost)
            {
                return "lost";
            }

            return hasCritical ? "critical" : "none";
        }

        private static Color SectionDamageGroundCueColor(string cue)
        {
            if (string.Equals(cue, "pilot", StringComparison.Ordinal))
            {
                return new Color(0.58f, 0.92f, 1f, 1f);
            }

            if (string.Equals(cue, "lost", StringComparison.Ordinal))
            {
                return new Color(1f, 0.18f, 0.08f, 1f);
            }

            return new Color(1f, 0.58f, 0.12f, 1f);
        }

        public static string SectionDamageCueSummary()
        {
            return "Arms=missing-socket+flag+flight+landing-debris Legs=collapse+red-cross+skid+dust Cockpit=breach+ejection-pod+chute+landing+arc+distress Critical=smoke+sparks Ground=critical+lost+pilot Wreck=blast+smoke+marker+debris+salvage";
        }

        public static string JetCueSummary()
        {
            return "Jet=takeoff+trail+landing";
        }

        public static string HeatCueSummary()
        {
            return "Heat=vent+lock";
        }

        private void SpawnJetTakeoffCue()
        {
            Vector3 ground = JetGroundPosition();
            Vector3 body = transform.position + Vector3.up * 0.08f;
            Vector3 left = transform.TransformDirection(new Vector3(-0.20f, 0f, -0.04f));
            Vector3 right = transform.TransformDirection(new Vector3(0.20f, 0f, -0.04f));
            Color flame = new(0.48f, 0.96f, 1f, 0.78f);
            CreateBeam("Jet Takeoff Flame L", body + left * 0.35f, ground + left * 0.62f, flame, 0.24f, 0.036f);
            CreateBeam("Jet Takeoff Flame R", body + right * 0.35f, ground + right * 0.62f, flame, 0.24f, 0.036f);
            CreateTransient("Jet Takeoff Dust", PrimitiveType.Cylinder, ground, new Color(0.66f, 0.58f, 0.42f, 0.34f), 0.48f, new Vector3(0.38f, 0.018f, 0.38f), new Vector3(1.08f, 0.006f, 1.08f));
            CreateTransient("Jet Takeoff Flash", PrimitiveType.Sphere, ground + Vector3.up * 0.08f, new Color(1f, 0.82f, 0.32f, 0.58f), 0.22f, Vector3.one * 0.14f, Vector3.one * 0.68f);
        }

        private void SpawnJetTrailCue()
        {
            Vector3 position = transform.position + Vector3.down * 0.28f;
            CreateTransient("Jet Smoke Trail", PrimitiveType.Sphere, position, new Color(0.10f, 0.12f, 0.13f, 0.34f), 0.72f, Vector3.one * 0.12f, Vector3.one * 0.58f);
            CreateTransient("Jet Heat Flicker", PrimitiveType.Sphere, position + Vector3.up * 0.06f, new Color(0.42f, 0.92f, 1f, 0.42f), 0.18f, Vector3.one * 0.08f, Vector3.one * 0.28f);
        }

        private void SpawnJetLandingCue()
        {
            Vector3 ground = JetGroundPosition();
            CreateTransient("Jet Landing Dust Ring", PrimitiveType.Cylinder, ground, new Color(0.72f, 0.62f, 0.42f, 0.38f), 0.62f, new Vector3(0.42f, 0.018f, 0.42f), new Vector3(1.42f, 0.006f, 1.42f));
            CreateTransient("Jet Landing Shock", PrimitiveType.Sphere, ground + Vector3.up * 0.06f, new Color(1f, 0.76f, 0.28f, 0.42f), 0.28f, Vector3.one * 0.16f, Vector3.one * 0.82f);
        }

        private Vector3 JetGroundPosition()
        {
            if (Unit == null)
            {
                return transform.position;
            }

            Vector3 ground = MissionToWorld(Unit.MissionPosition);
            ground.y = DemoTerrainView.HeightAt(Unit.MissionPosition) + 0.055f;
            return ground;
        }

        private void SpawnHeatLockPulse()
        {
            Vector3 center = SectionWorldPoint(new Vector3(0f, 0.45f, 0.10f));
            CreateTransient("Heat Lock Flash", PrimitiveType.Sphere, center, new Color(1f, 0.20f, 0.04f, 0.72f), 0.46f, Vector3.one * 0.16f, Vector3.one * 0.86f);
            CreateTransient("Heat Vent Smoke", PrimitiveType.Sphere, center + Vector3.up * 0.24f, new Color(0.10f, 0.08f, 0.06f, 0.46f), 0.96f, Vector3.one * 0.16f, Vector3.one * 0.68f);
        }

        private void SpawnCockpitEjection()
        {
            if (ejectionEffectSpawned)
            {
                return;
            }

            ejectionEffectSpawned = true;
            Vector3 start = SectionWorldPoint(new Vector3(0f, 0.7f, 0.34f));
            Vector3 end = start + Vector3.up * 2.65f + transform.TransformDirection(new Vector3(0.45f, 0f, 0.36f));
            CreateBeam("Ejection Trail", start, end, new Color(0.72f, 0.9f, 1f, 0.68f), 1.2f, 0.045f);
            CreateBeam("Ejection Smoke Trail", start + Vector3.down * 0.08f, end + Vector3.down * 0.35f, new Color(0.08f, 0.10f, 0.12f, 0.36f), 1.6f, 0.075f);
            SpawnEjectionArcCue(start, end);
            CreateTransient("Ejection Flash", PrimitiveType.Sphere, start, new Color(1f, 0.75f, 0.28f, 0.78f), 0.48f, Vector3.one * 0.18f, Vector3.one * 0.95f);
            CreateMovingTransient("Pilot Ejection Pod", PrimitiveType.Sphere, start, end, new Color(0.88f, 0.95f, 1f, 0.92f), 1.75f, Vector3.one * 0.18f, Vector3.one * 0.38f);
            SpawnEjectionChuteCue(end);
            SpawnDamageBurst(start, 1.05f);
        }

        private void SpawnEjectionArcCue(Vector3 start, Vector3 end)
        {
            Vector3 apex = Vector3.Lerp(start, end, 0.48f) + Vector3.up * 0.56f;
            CreateBeam("Ejection Arc Signal A", start, apex, new Color(0.58f, 0.94f, 1f, 0.70f), 1.70f, 0.026f);
            CreateBeam("Ejection Arc Signal B", apex, end, new Color(0.58f, 0.94f, 1f, 0.58f), 1.95f, 0.020f);
            CreateTransient("Ejection Apex Flash", PrimitiveType.Sphere, apex, new Color(0.72f, 0.96f, 1f, 0.62f), 0.95f, Vector3.one * 0.10f, Vector3.one * 0.42f);
        }

        private void SpawnEjectionChuteCue(Vector3 apex)
        {
            Vector3 drift = transform.TransformDirection(new Vector3(0.62f, 0f, 0.52f));
            Vector3 landing = GroundAtWorld(apex + drift);
            Vector3 canopy = Vector3.Lerp(apex, landing, 0.28f) + Vector3.up * 0.42f;
            Vector3 right = transform.TransformDirection(Vector3.right);
            Vector3 podPoint = Vector3.Lerp(apex, landing, 0.42f) + Vector3.up * 0.10f;

            CreateTransient("Pilot Chute Canopy", PrimitiveType.Sphere, canopy, new Color(0.82f, 0.95f, 1f, 0.72f), 2.2f, new Vector3(0.32f, 0.08f, 0.32f), new Vector3(0.72f, 0.05f, 0.72f));
            CreateBeam("Pilot Chute Cord L", canopy - right * 0.24f, podPoint, new Color(0.86f, 0.96f, 1f, 0.58f), 2.2f, 0.010f);
            CreateBeam("Pilot Chute Cord R", canopy + right * 0.24f, podPoint, new Color(0.86f, 0.96f, 1f, 0.58f), 2.2f, 0.010f);
            CreateMovingTransient("Pilot Chute Pod", PrimitiveType.Sphere, podPoint, landing + Vector3.up * 0.08f, new Color(0.88f, 0.95f, 1f, 0.72f), 2.2f, Vector3.one * 0.16f, Vector3.one * 0.22f);
            CreateTransient("Pilot Landing Beacon", PrimitiveType.Cylinder, landing, new Color(0.58f, 0.92f, 1f, 0.48f), 2.6f, new Vector3(0.18f, 0.012f, 0.18f), new Vector3(0.62f, 0.006f, 0.62f));
            CreateBeam("Pilot Landing Signal", landing + Vector3.up * 0.06f, landing + Vector3.up * 0.92f, new Color(0.58f, 0.92f, 1f, 0.56f), 2.35f, 0.022f);
            CreatePilotDistressMarker(landing);
            CreateTransient("Pilot Landing Smoke", PrimitiveType.Sphere, landing + Vector3.up * 0.12f, new Color(0.08f, 0.11f, 0.13f, 0.36f), 2.1f, Vector3.one * 0.12f, new Vector3(0.72f, 0.34f, 0.72f));
        }

        private void CreatePilotDistressMarker(Vector3 landing)
        {
            Quaternion forwardYaw = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward), Vector3.up);
            Quaternion sideYaw = forwardYaw * Quaternion.Euler(0f, 90f, 0f);
            Vector3 marker = landing + Vector3.up * 0.030f;
            Color signal = new(0.58f, 0.92f, 1f, 0.88f);
            CreateWorldDamageObject("Pilot Distress Marker", PrimitiveType.Cube, marker, forwardYaw, new Vector3(0.62f, 0.030f, 0.075f), signal);
            CreateWorldDamageObject("Pilot Distress Marker Crossbar", PrimitiveType.Cube, marker + Vector3.up * 0.006f, sideYaw, new Vector3(0.50f, 0.026f, 0.065f), signal);
            CreateWorldDamageObject("Pilot Distress Beacon", PrimitiveType.Cylinder, landing + Vector3.up * 0.22f, Quaternion.identity, new Vector3(0.045f, 0.32f, 0.045f), new Color(0.58f, 0.92f, 1f, 0.96f));
            CreateTransient("Pilot Distress Flare", PrimitiveType.Sphere, landing + Vector3.up * 0.42f, new Color(0.68f, 0.95f, 1f, 0.62f), 2.45f, Vector3.one * 0.09f, Vector3.one * 0.46f);
        }

        private static Vector3 GroundAtWorld(Vector3 worldPoint)
        {
            Vector2 missionPoint = WorldToMission(worldPoint);
            worldPoint.y = DemoTerrainView.HeightAt(missionPoint) + 0.075f;
            return worldPoint;
        }

        private bool HasDestroyedSection(string sectionName)
        {
            if (Unit == null || Unit.Sections == null)
            {
                return false;
            }

            for (int index = 0; index < Unit.Sections.Length; index++)
            {
                DamageSection section = Unit.Sections[index];
                if (section.IsDestroyed && string.Equals(section.Name, sectionName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplySectionPartDamageColor(string sectionName, Color color)
        {
            if (IsSectionName(sectionName, "Cockpit") || IsSectionName(sectionName, "Turret"))
            {
                SetSectionPartColor("Cockpit", color);
                return;
            }

            if (IsSectionName(sectionName, "Left Arm") || IsSectionName(sectionName, "Left"))
            {
                SetSectionPartColor("Left Arm", color);
                return;
            }

            if (IsSectionName(sectionName, "Right Arm") || IsSectionName(sectionName, "Right"))
            {
                SetSectionPartColor("Right Arm", color);
                return;
            }

            if (IsSectionName(sectionName, "Legs"))
            {
                SetSectionPartColor("Left Leg", color);
                SetSectionPartColor("Right Leg", color);
            }
        }

        private void SetSectionPartColor(string partName, Color color)
        {
            if (!sectionParts.TryGetValue(partName, out GameObject part) || part == null)
            {
                return;
            }

            Renderer partRenderer = part.GetComponent<Renderer>();
            if (partRenderer != null && partRenderer.sharedMaterial != null)
            {
                partRenderer.sharedMaterial.color = color;
            }
        }

        private static bool IsSectionName(string sectionName, string expected)
        {
            return string.Equals(sectionName, expected, StringComparison.OrdinalIgnoreCase);
        }

        private static Vector3 CriticalSectionLocalPoint(string sectionName)
        {
            if (IsSectionName(sectionName, "Cockpit") || IsSectionName(sectionName, "Turret"))
            {
                return new Vector3(0f, 0.52f, 0.30f);
            }

            if (IsSectionName(sectionName, "Left Arm") || IsSectionName(sectionName, "Left"))
            {
                return new Vector3(-0.54f, 0.12f, 0.08f);
            }

            if (IsSectionName(sectionName, "Right Arm") || IsSectionName(sectionName, "Right"))
            {
                return new Vector3(0.54f, 0.12f, 0.08f);
            }

            if (IsSectionName(sectionName, "Legs"))
            {
                return new Vector3(0f, -0.34f, 0.10f);
            }

            return new Vector3(0f, 0.12f, 0.18f);
        }

        private Color SectionDamageColor(DamageSection section)
        {
            if (section.IsDestroyed)
            {
                return new Color(0.10f, 0.08f, 0.06f, 1f);
            }

            if (section.Ratio < CriticalSectionRatio)
            {
                return Color.Lerp(liveColor, new Color(1f, 0.16f, 0.08f, 1f), 0.82f);
            }

            if (section.Ratio < 0.70f)
            {
                return Color.Lerp(liveColor, new Color(1f, 0.72f, 0.12f, 1f), 0.62f);
            }

            return liveColor;
        }

        private void CreateDamageScar(string scarName, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject scar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            scar.name = Unit.Id + " " + scarName;
            scar.transform.SetParent(transform, false);
            scar.transform.localPosition = localPosition;
            scar.transform.localScale = localScale;
            Renderer scarRenderer = scar.GetComponent<Renderer>();
            if (scarRenderer != null)
            {
                scarRenderer.sharedMaterial = CreateOwnedMaterial(color);
            }

            Collider scarCollider = scar.GetComponent<Collider>();
            if (scarCollider != null)
            {
                Destroy(scarCollider);
            }
        }

        private void CreateDamagePuff(string puffName, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.name = Unit.Id + " " + puffName;
            puff.transform.SetParent(transform, false);
            puff.transform.localPosition = localPosition;
            puff.transform.localScale = localScale;
            Renderer puffRenderer = puff.GetComponent<Renderer>();
            if (puffRenderer != null)
            {
                puffRenderer.sharedMaterial = CreateOwnedMaterial(color);
            }

            Collider puffCollider = puff.GetComponent<Collider>();
            if (puffCollider != null)
            {
                Destroy(puffCollider);
            }
        }

        private void CreateDamageBeacon(string beaconName, Vector3 localPosition, Color color, float radius, float height)
        {
            GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beacon.name = Unit.Id + " " + beaconName;
            beacon.transform.SetParent(transform, false);
            beacon.transform.localPosition = localPosition;
            beacon.transform.localScale = new Vector3(radius, height * 0.5f, radius);
            Renderer beaconRenderer = beacon.GetComponent<Renderer>();
            if (beaconRenderer != null)
            {
                beaconRenderer.sharedMaterial = CreateOwnedMaterial(color);
            }

            Collider beaconCollider = beacon.GetComponent<Collider>();
            if (beaconCollider != null)
            {
                Destroy(beaconCollider);
            }
        }

        private void CreateDamageFlag(string flagName, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            flag.name = Unit.Id + " " + flagName;
            flag.transform.SetParent(transform, false);
            flag.transform.localPosition = localPosition;
            flag.transform.localScale = localScale;
            Renderer flagRenderer = flag.GetComponent<Renderer>();
            if (flagRenderer != null)
            {
                flagRenderer.sharedMaterial = CreateOwnedMaterial(color);
            }

            Collider flagCollider = flag.GetComponent<Collider>();
            if (flagCollider != null)
            {
                Destroy(flagCollider);
            }
        }

        private void CreateDamageLink(string linkName, Vector3 localStart, Vector3 localEnd, Color color, float radius)
        {
            Vector3 direction = localEnd - localStart;
            float length = direction.magnitude;
            if (length <= 0.01f)
            {
                return;
            }

            GameObject link = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            link.name = Unit.Id + " " + linkName;
            link.transform.SetParent(transform, false);
            link.transform.localPosition = (localStart + localEnd) * 0.5f;
            link.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            link.transform.localScale = new Vector3(radius, length * 0.5f, radius);
            Renderer linkRenderer = link.GetComponent<Renderer>();
            if (linkRenderer != null)
            {
                linkRenderer.sharedMaterial = CreateOwnedMaterial(color);
            }

            Collider linkCollider = link.GetComponent<Collider>();
            if (linkCollider != null)
            {
                Destroy(linkCollider);
            }
        }

        private void CreateWorldDamageObject(string objectName, PrimitiveType primitive, Vector3 position, Quaternion rotation, Vector3 scale, Color color)
        {
            GameObject marker = GameObject.CreatePrimitive(primitive);
            marker.name = Unit.Id + " " + objectName;
            marker.transform.SetPositionAndRotation(position, rotation);
            marker.transform.localScale = scale;
            Renderer markerRenderer = marker.GetComponent<Renderer>();
            if (markerRenderer != null)
            {
                markerRenderer.sharedMaterial = CreateOwnedMaterial(color);
            }

            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
            {
                Destroy(markerCollider);
            }

            marker.transform.SetParent(transform, true);
        }

        private void SetHeatCueVisible(bool heatVisible, bool heatLocked)
        {
            if (heatVentCue != null)
            {
                heatVentCue.SetActive(heatVisible);
            }

            if (heatLockCue != null)
            {
                heatLockCue.SetActive(heatLocked);
            }
        }

        private static void SetCueColor(GameObject cue, Color color)
        {
            Renderer renderer = cue.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                renderer.sharedMaterial.color = color;
            }
        }

        private Material CreateOwnedMaterial(Color color)
        {
            Shader shader = Shader.Find("Standard") ?? Shader.Find("Hidden/Internal-Colored");
            Material material = new(shader)
            {
                color = color
            };
            ownedMaterials.Add(material);
            return material;
        }

        private Material CreateOwnedTransparentMaterial(Color color)
        {
            Material material = CreateOwnedMaterial(color);
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return material;
        }

        private void OnDestroy()
        {
            for (int index = 0; index < ownedMaterials.Count; index++)
            {
                if (ownedMaterials[index] != null)
                {
                    Destroy(ownedMaterials[index]);
                }
            }
        }

        private Vector3 SectionWorldPoint(Vector3 localPoint)
        {
            return transform.TransformPoint(localPoint);
        }

        private void CreateTransient(string effectName, PrimitiveType primitive, Vector3 position, Color color, float duration, Vector3 fromScale, Vector3 toScale)
        {
            GameObject effect = GameObject.CreatePrimitive(primitive);
            effect.name = effectName;
            effect.transform.position = position;
            Collider effectCollider = effect.GetComponent<Collider>();
            if (effectCollider != null)
            {
                Destroy(effectCollider);
            }

            DemoTransientEffect transient = effect.AddComponent<DemoTransientEffect>();
            transient.Begin(color, duration, fromScale, toScale);
        }

        private void CreateMovingTransient(string effectName, PrimitiveType primitive, Vector3 start, Vector3 end, Color color, float duration, Vector3 fromScale, Vector3 toScale)
        {
            GameObject effect = GameObject.CreatePrimitive(primitive);
            effect.name = effectName;
            effect.transform.position = start;
            Collider effectCollider = effect.GetComponent<Collider>();
            if (effectCollider != null)
            {
                Destroy(effectCollider);
            }

            DemoTransientEffect transient = effect.AddComponent<DemoTransientEffect>();
            transient.BeginMoving(color, duration, fromScale, toScale, end);
        }

        private void CreateBeam(string effectName, Vector3 from, Vector3 to, Color color, float duration, float radius)
        {
            Vector3 direction = to - from;
            float length = direction.magnitude;
            if (length <= 0.01f)
            {
                return;
            }

            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = effectName;
            beam.transform.position = (from + to) * 0.5f;
            beam.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            Collider beamCollider = beam.GetComponent<Collider>();
            if (beamCollider != null)
            {
                Destroy(beamCollider);
            }

            DemoTransientEffect transient = beam.AddComponent<DemoTransientEffect>();
            transient.Begin(color, duration, new Vector3(radius, length * 0.5f, radius), new Vector3(radius * 0.35f, length * 0.5f, radius * 0.35f));
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
