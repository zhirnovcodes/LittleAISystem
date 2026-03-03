using System;
using LittleAI.Enums;
using Unity.Mathematics;

[Serializable]
public class StatAttenuationGenomeData : IGenomeDataConvertible
{
    public StatType StatType;
    public AnimalStatsAttenuation Attenuation;
    
    public DNAChainData GetDNAData()
    {
        return new DNAChainData
        {
            GenomeType = GenomeType.StatAttenuation,
            GenomeData = new GenomeData
            {
                Index = (int)StatType,
                Data = new float4x4(
                    Attenuation.Needs.points,
                    new float4(Attenuation.Needs.tangents.x, Attenuation.Needs.tangents.y, 0, 0),
                    Attenuation.Distance.points,
                    new float4(Attenuation.Distance.tangents.x, Attenuation.Distance.tangents.y, 0, 0)
                )
            }
        };
    }
}

