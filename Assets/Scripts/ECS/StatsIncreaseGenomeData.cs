using System;
using Unity.Mathematics;

[Serializable]
public class StatsIncreaseGenomeData : IGenomeDataConvertible
{
    public AnimalStats AnimalStats;
    
    public GenomeData GetGenomeData()
    {
        return new GenomeData
        {
            Index = 0,
            Data = AnimalStats.Stats.ToFloat4x4()
        };
    }
}

