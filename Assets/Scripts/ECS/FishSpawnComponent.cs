using Unity.Entities;
using Unity.Mathematics;

public struct FishSpawnComponent : IComponentData, IEnableableComponent
{
    public float2 SpawnInterval;
    public float TimeElapsed;
    public float3 SpawnPosition;
    public uint Count;
    public uint MaxCount;
    public Random Random;
}
