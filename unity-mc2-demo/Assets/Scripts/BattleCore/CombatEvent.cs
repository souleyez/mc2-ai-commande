namespace MC2Demo.BattleCore
{
    public readonly struct CombatEvent
    {
        public string AttackerId { get; }
        public string TargetId { get; }
        public string SectionName { get; }
        public float Damage { get; }
        public bool DestroyedTarget { get; }
        public string WeaponType { get; }
        public int SpecialEffect { get; }

        public CombatEvent(
            string attackerId,
            string targetId,
            string sectionName,
            float damage,
            bool destroyedTarget,
            string weaponType = "Generic",
            int specialEffect = 0)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            SectionName = sectionName;
            Damage = damage;
            DestroyedTarget = destroyedTarget;
            WeaponType = weaponType;
            SpecialEffect = specialEffect;
        }
    }
}
