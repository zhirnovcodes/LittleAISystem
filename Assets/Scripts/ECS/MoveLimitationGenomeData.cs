using System;
using Unity.Mathematics;

[Serializable]
public class MoveLimitationGenomeData : IGenomeDataConvertible
{
    public float3 Central;
    public float3 Scale;

    public DNAChainData GetDNAData()
    {
        return new DNAChainData
        {
            GenomeType = GenomeType.MoveLimitation,
            GenomeData = new GenomeData
            {
                Index = 0,
                Data = new float4x4(
                    new float4(Central, 0),
                    new float4(Scale, 0),
                    float4.zero,
                    float4.zero
                )
            }
        };
    }
}
