using Unity.Entities;
using Unity.Mathematics;

public struct NeedBasedInputItem : IBufferElementData
{
    public Entity Target;
    public AnimalStats StatsAdvertised;
    public float3 Position;
    public float Scale;
}

