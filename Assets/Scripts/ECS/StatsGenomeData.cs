using System;
using Unity.Mathematics;

[Serializable]
public class StatsGenomeData : IGenomeDataConvertible
{
    public AnimalStats Stats;
    
    public DNAChainData GetDNAData()
    {
        return new DNAChainData
        {
            GenomeType = GenomeType.Stats,
            GenomeData = new GenomeData
            {
                Index = 0,
                Data = Stats.Stats.ToFloat4x4()
            }
        };
    }
}

