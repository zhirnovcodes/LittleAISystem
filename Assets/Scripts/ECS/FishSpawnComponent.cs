using Unity.Entities;
using Unity.Mathematics;

public struct FishSpawnComponent : IComponentData
{
    public float2 SpawnInterval;
    public float TimeElapsed;
    public float3 SpawnPosition;
    public Random Random;
}
