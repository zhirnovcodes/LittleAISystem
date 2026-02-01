using LittleAI.Enums;
using Unity.Entities;

public struct StatAdvertiserItem : IBufferElementData
{
    public AnimalStats AdvertisedValue;
    public ConditionFlags ActorConditions;
    public ActionTypes ActionType;
}

