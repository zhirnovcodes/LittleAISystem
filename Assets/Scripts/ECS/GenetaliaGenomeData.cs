using System;
using Unity.Mathematics;

[Serializable]
public class GenetaliaGenomeData : IGenomeDataConvertible
{
    public bool IsMale;
    public bool IsFemale => !IsMale;
    
    public DNAChainData GetDNAData()
    {
        return new DNAChainData
        {
            GenomeType = GenomeType.Genitalia,
            GenomeData = new GenomeData
            {
                Index = 0,
                Data = new float4x4(
                    new float4(IsMale ? 1f : 0f, 0, 0, 0),
                    float4.zero,
                    float4.zero,
                    float4.zero
                )
            }
        };
    }
}

