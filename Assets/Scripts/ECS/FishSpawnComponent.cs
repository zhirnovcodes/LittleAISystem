using Unity.Entities;
using Unity.Mathematics;

public struct FishSpawnComponent : IComponentData
{
    public Entity Prefab;
    public int Count;
    public float2 VisionRange;
    public float2 VisionInterval;
    public uint RandomSeed;
}

