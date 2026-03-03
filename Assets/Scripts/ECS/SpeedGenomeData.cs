using System;
using Unity.Mathematics;

[Serializable]
public class SpeedGenomeData : IGenomeDataConvertible
{
    public float MaxSpeed;
    public float MaxRotationSpeed;
    
    public GenomeData GetGenomeData()
    {
        return new GenomeData
        {
            Index = 0,
            Data = new float4x4(
                new float4(MaxSpeed, MaxRotationSpeed, 0, 0),
                float4.zero,
                float4.zero,
                float4.zero
            )
        };
    }
}

