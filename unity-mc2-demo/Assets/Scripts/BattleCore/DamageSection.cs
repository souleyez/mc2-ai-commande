namespace MC2Demo.BattleCore
{
    public sealed class DamageSection
    {
        public string Name { get; }
        public float MaxHitPoints { get; }
        public bool IsCritical { get; }
        public float HitPoints { get; private set; }
        public bool IsDestroyed => HitPoints <= 0f;
        public float Ratio => MaxHitPoints <= 0f ? 0f : HitPoints / MaxHitPoints;

        public DamageSection(string name, float maxHitPoints, bool isCritical = false)
        {
            Name = name;
            MaxHitPoints = maxHitPoints;
            IsCritical = isCritical;
            HitPoints = maxHitPoints;
        }

        public float ApplyDamage(float damage)
        {
            float applied = damage < HitPoints ? damage : HitPoints;
            HitPoints -= applied;
            return damage - applied;
        }
    }
}
