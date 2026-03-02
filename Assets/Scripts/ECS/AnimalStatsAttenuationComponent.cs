using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;

public struct AnimalStatsAttenuationComponent : IComponentData
{
    public AnimalStatsAttenuation Attenuation;

    public float4x4 GetFloat4x4(StatType statType)
    {
        HermiteCurve needsCurve = Attenuation.GetNeedsCurve(statType);
        HermiteCurve distanceCurve = Attenuation.GetDistanceCurve(statType);

        float4x4 needsData = needsCurve.ToFloat4x4();
        float4x4 distanceData = distanceCurve.ToFloat4x4();

        // Combine both curves into one float4x4
        return new float4x4(
            needsData.c0,      // needs points
            needsData.c1,      // needs tangents
            distanceData.c0,   // distance points
            distanceData.c1    // distance tangents
        );
    }

    public void SetFloat4x4(StatType statType, float4x4 data)
    {
        // Extract needs curve from first 2 columns
        float4x4 needsData = new float4x4(data.c0, data.c1, float4.zero, float4.zero);
        HermiteCurve needsCurve = HermiteCurve.FromFloat4x4(needsData);

        // Extract distance curve from last 2 columns
        float4x4 distanceData = new float4x4(data.c2, data.c3, float4.zero, float4.zero);
        HermiteCurve distanceCurve = HermiteCurve.FromFloat4x4(distanceData);

        Attenuation.SetNeedsCurve(statType, needsCurve);
        Attenuation.SetDistanceCurve(statType, distanceCurve);
    }
}

