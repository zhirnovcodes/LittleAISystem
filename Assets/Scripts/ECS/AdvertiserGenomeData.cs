using System;
using LittleAI.Enums;
using Unity.Mathematics;

[Serializable]
public class AdvertiserGenomeData : IGenomeDataConvertible
{
    public AnimalStats AdvertisedValue;
    public ConditionFlags ActorConditions;
    public ActionTypes ActionType;
    
    public DNAChainData GetDNAData()
    {
        return new DNAChainData
        {
            GenomeType = GenomeType.Advertiser,
            GenomeData = new GenomeData
            {
                // Combine ActorConditions and ActionType into Index
                // ActionType in lower byte, ActorConditions in upper bits (shift left by 8)
                // This matches StatAdvertiserItem conversion: actionType = id & 0xFF, actorConditions = id >> 8
                Index = ((int)ActorConditions << 8) | (int)ActionType,
                Data = AdvertisedValue.Stats.ToFloat4x4()
            }
        };
    }
}

