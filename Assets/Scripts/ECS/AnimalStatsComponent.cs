using Unity.Entities;
using Unity.Mathematics;

public struct AnimalStatsComponent : IComponentData
{
    public AnimalStats Stats;

    public void SetFloat4x4(float4x4 data)
    {
        Stats = new AnimalStats
        {
            Stats = new float4x2(data.c0, data.c1)
        };
    }
}

