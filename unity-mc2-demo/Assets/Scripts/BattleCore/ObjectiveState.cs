namespace MC2Demo.BattleCore
{
    public sealed class ObjectiveState
    {
        public ObjectiveDefinition Definition { get; }
        public bool IsActive { get; set; }
        public bool IsComplete { get; set; }

        public ObjectiveState(ObjectiveDefinition definition)
        {
            Definition = definition;
        }
    }
}
