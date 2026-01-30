using Unity.Entities;

public struct SafetyCheckItem : IBufferElementData
{
    public ConditionFlags ActorConditions;
    public float SafetyRecession;
}

