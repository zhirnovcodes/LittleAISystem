using Unity.Entities;

public struct StatAdvertiserItem : IBufferElementData
{
    public AnimalStats AdvertisedValue;
    public ConditionFlags ActorConditions;
}

