using LittleAI.Enums;
using Unity.Entities;

public struct NeedBasedOutputComponent : IComponentData
{
    public Entity Target;
    public ActionTypes Action;
    public float StatsWeight;
}

