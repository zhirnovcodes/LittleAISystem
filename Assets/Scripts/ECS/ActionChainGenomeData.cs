using System;
using Unity.Mathematics;

[Serializable]
public class ActionChainGenomeData : IGenomeDataConvertible
{
    public GenomeData GetGenomeData()
    {
        return new GenomeData
        {
            Index = 0,
            Data = float4x4.zero
        };
    }
}

