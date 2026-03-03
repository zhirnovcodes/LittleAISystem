using System;
using Unity.Mathematics;

[Serializable]
public class StatsGenomeData : IGenomeDataConvertible
{
    public AnimalStats Stats;
    
    public GenomeData GetGenomeData()
    {
        return new GenomeData
        {
            Index = 0,
            Data = Stats.Stats.ToFloat4x4()
        };
    }
}

