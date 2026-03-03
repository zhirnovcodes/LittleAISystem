using System;
using Unity.Mathematics;

[Serializable]
public class StatsIncreaseGenomeData : IGenomeDataConvertible
{
    public AnimalStats AnimalStats;
    
    public DNAChainData GetDNAData()
    {
        return new DNAChainData
        {
            GenomeType = GenomeType.StatsIncrease,
            GenomeData = new GenomeData
            {
                Index = 0,
                Data = AnimalStats.Stats.ToFloat4x4()
            }
        };
    }
}

