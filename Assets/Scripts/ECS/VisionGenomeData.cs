using System;
using Unity.Mathematics;

[Serializable]
public class VisionGenomeData : IGenomeDataConvertible
{
    public float MaxDistance;
    public float Interval;
    
    public DNAChainData GetDNAData()
    {
        return new DNAChainData
        {
            GenomeType = GenomeType.Vision,
            GenomeData = new GenomeData
            {
                Index = 0,
                Data = new float4x4(
                    new float4(MaxDistance, Interval, 0, 0),
                    float4.zero,
                    float4.zero,
                    float4.zero
                )
            }
        };
    }
}

