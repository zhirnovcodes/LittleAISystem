using Unity.Entities;
using Unity.Mathematics;

public struct FishSpawnComponent : IComponentData, IEnableableComponent
{
    public float2 SpawnInterval;
    public float3 SpawnPosition;
    public float3 SpawnScale;
    public float TimeElapsed;
    public uint Count;
    public uint MaxCount;
    public uint OneTimeSpawn;
    public Random Random;
}
