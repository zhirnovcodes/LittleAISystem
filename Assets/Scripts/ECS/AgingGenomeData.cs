using System;
using Unity.Mathematics;

[Serializable]
public class AgingGenomeData : IGenomeDataConvertible
{
    public float MinSize;
    public float MaxSize;
    
    public GenomeData GetGenomeData()
    {
        return new GenomeData
        {
            Index = 0,
            Data = new float4x4(
                new float4(MinSize, MaxSize, 0, 0),
                float4.zero,
                float4.zero,
                float4.zero
            )
        };
    }
}

