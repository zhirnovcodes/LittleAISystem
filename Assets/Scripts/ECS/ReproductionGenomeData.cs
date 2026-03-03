using System;
using Unity.Mathematics;

[Serializable]
public class ReproductionGenomeData : IGenomeDataConvertible
{
    public bool IsMale;
    public bool IsFemale => !IsMale;
    public float GestationTime = 10f;
    
    public DNAChainData GetDNAData()
    {
        return new DNAChainData
        {
            GenomeType = GenomeType.Reproduction,
            GenomeData = new GenomeData
            {
                Index = 0,
                Data = new float4x4(
                    new float4(IsMale ? 1f : 0f, GestationTime, 0, 0),
                    float4.zero,
                    float4.zero,
                    float4.zero
                )
            }
        };
    }
}

