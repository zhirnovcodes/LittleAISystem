using System;
using Unity.Mathematics;

[Serializable]
public class NeedsBasedGenomeData : IGenomeDataConvertible
{
    public float CancelThreshold;
    public float AddThreshold;
    
    public DNAChainData GetDNAData()
    {
        return new DNAChainData
        {
            GenomeType = GenomeType.NeedsBased,
            GenomeData = new GenomeData
            {
                Index = 0,
                Data = new float4x4(
                    new float4(CancelThreshold, AddThreshold, 0, 0),
                    float4.zero,
                    float4.zero,
                    float4.zero
                )
            }
        };
    }
}

