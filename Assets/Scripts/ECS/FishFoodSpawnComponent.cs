using Unity.Entities;
using Unity.Mathematics;

public struct FishFoodSpawnComponent : IComponentData
{
    public Entity Prefab;
    public float2 SpawnInterval;
    public float3 Position;
    public float3 Scale;
    public float TimeElapsed;
    public Random Random;
}