using UnityEngine;

namespace MC2Demo.BattleCore
{
    public sealed class StructureState
    {
        public string Id { get; }
        public string ObjectType { get; }
        public int TeamId { get; }
        public bool IsTargetable { get; }
        public bool IsObjectiveTarget { get; }
        public bool IsDestroyed { get; private set; }
        public float MaxStructure { get; }
        public float CurrentStructure { get; private set; }
        public float Radius { get; }
        public Vector2 MissionPosition { get; }
        public float Structure => MaxStructure <= 0f ? 0f : CurrentStructure / MaxStructure;

        public StructureState(StaticObjectSpawn spawn)
        {
            Id = string.IsNullOrWhiteSpace(spawn.objectId) ? "structure" : spawn.objectId;
            ObjectType = string.IsNullOrWhiteSpace(spawn.objectType) ? "Structure" : spawn.objectType;
            TeamId = spawn.teamId;
            IsTargetable = spawn.targetable;
            IsObjectiveTarget = spawn.objectiveTarget;
            Radius = Mathf.Max(40f, spawn.radius);
            MaxStructure = Mathf.Max(1f, spawn.maxStructure);
            CurrentStructure = MaxStructure;

            if (spawn.position != null)
            {
                MissionPosition = new Vector2(spawn.position.x, spawn.position.y);
            }
        }

        public float ApplyDamage(float damage)
        {
            if (IsDestroyed || damage <= 0f)
            {
                return 0f;
            }

            float applied = Mathf.Min(CurrentStructure, damage);
            CurrentStructure -= applied;
            if (CurrentStructure <= 0f)
            {
                IsDestroyed = true;
                CurrentStructure = 0f;
            }

            return applied;
        }
    }
}
