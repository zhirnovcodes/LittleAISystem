using System;
using Unity.Mathematics;

[Serializable]
public class ActionChainGenomeData : IGenomeDataConvertible
{
    public DNAChainData GetDNAData()
    {
        return new DNAChainData
        {
            GenomeType = GenomeType.ActionChain,
            GenomeData = new GenomeData
            {
                Index = 0,
                Data = float4x4.zero
            }
        };
    }
}

