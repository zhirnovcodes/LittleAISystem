using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;

public struct StatAdvertiserItem : IBufferElementData
{
    public AnimalStats AdvertisedValue;
    public ConditionFlags ActorConditions;
    public ActionTypes ActionType;

    public void SetFloat4x4(float4x4 data)
    {
        int id = (int)data.c2.x;
        FromID(id, out ActionTypes actionType, out ConditionFlags actorConditions);

        AdvertisedValue = new AnimalStats
        {
            Stats = new float4x2(data.c0, data.c1)
        };
        ActionType = actionType;
        ActorConditions = actorConditions;
    }

    public static int GetID(ActionTypes actionType, ConditionFlags actorConditions)
    {
        // Combine ActionType and ActorConditions using bit shift
        // ActionType in lower bits, ActorConditions in upper bits
        return ((int)actorConditions << 16) | (int)actionType;
    }

    public static void FromID(int id, out ActionTypes actionType, out ConditionFlags actorConditions)
    {
        // Extract ActionType from lower 16 bits
        actionType = (ActionTypes)(id & 0xFFFF);
        // Extract ActorConditions from upper 16 bits
        actorConditions = (ConditionFlags)(id >> 16);
    }
}

