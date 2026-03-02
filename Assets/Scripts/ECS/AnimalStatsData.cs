using Unity.Entities;
using Unity.Mathematics;

public struct AnimalStatsData : IComponentData
{
    public AnimalStats Stats;

    public float4x4 ToFloat4x4()
    {
        return new float4x4(
            Stats.Stats.c0,
            Stats.Stats.c1,
            float4.zero,
            float4.zero
        );
    }

    public static AnimalStatsData FromFloat4x4(float4x4 data)
    {
        return new AnimalStatsData
        {
            Stats = new AnimalStats
            {
                Stats = new float4x2(data.c0, data.c1)
            }
        };
    }
}

