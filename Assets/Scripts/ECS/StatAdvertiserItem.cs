using LittleAI.Enums;
using Unity.Entities;

public struct StatAdvertiserItem : IBufferElementData
{
    public AnimalStats AdvertisedValue;
    public ConditionFlags ActorConditions;
    public ActionTypes ActionType;

    public static implicit operator StatAdvertiserItem(GenomeData genomeData)
    {
        // Id contains bit-shifted and combined ActorConditions and ActionType
        int id = genomeData.Index;
        
        // Extract ActionType from lower byte
        ActionTypes actionType = (ActionTypes)(id & 0xFF);
        
        // Extract ActorConditions from upper bits (shift right by 8 bits)
        ConditionFlags actorConditions = (ConditionFlags)((uint)id >> 8);
        
        return new StatAdvertiserItem
        {
            AdvertisedValue = new AnimalStats
            {
                Stats = genomeData.Data
            },
            ActorConditions = actorConditions,
            ActionType = actionType
        };
    }
}

