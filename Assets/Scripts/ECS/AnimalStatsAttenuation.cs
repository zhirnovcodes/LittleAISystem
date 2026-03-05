using Unity.Mathematics;

[System.Serializable]
public struct AnimalStatsAttenuation
{
    [HermiteCurveNormalized] public HermiteCurve Needs;
    [HermiteCurveScaled] public HermiteCurve Distance;

    public static implicit operator AnimalStatsAttenuation(GenomeData genomeData)
    {
        return new AnimalStatsAttenuation
        {
            Needs = new HermiteCurve
            {
                points = genomeData.Data.c0,
                tangents = new Unity.Mathematics.float2(genomeData.Data.c1.x, genomeData.Data.c1.y)
            },
            Distance = new HermiteCurve
            {
                points = genomeData.Data.c2,
                tangents = new Unity.Mathematics.float2(genomeData.Data.c3.x, genomeData.Data.c3.y)
            }
        };
    }

    /// <summary>
    /// Calculates the attenuated stat change after applying distance and needs attenuation
    /// </summary>
    /// <param name="currentValue">Current stat value (0-100)</param>
    /// <param name="valueChange">Stat change value</param>
    /// <param name="distance">Distance to target</param>
    /// <returns>Attenuated stat change</returns>
    public float GetStatAttenuated(float currentValue, float valueChange, float distance)
    {
        // 1 - Apply distance attenuation to the value change
        float distanceAttenuation = Distance.GetY(distance);
        float attenuatedChange = valueChange * distanceAttenuation;

        // 2 - Calculate needs attenuation of current state (normalized to 0-1)
        float currentNormalized = math.clamp(currentValue, 0f, 100f) / 100f;
        float needsAttenuation0 = Needs.GetY(currentNormalized);

        // 3 - Calculate needs attenuation of resulted state (current + attenuated change, normalized to 0-1)
        float resultedValue = math.clamp(currentValue + attenuatedChange, 0f, 100f);
        float resultedNormalized = resultedValue / 100f;
        float needsAttenuation1 = Needs.GetY(resultedNormalized);

        // 4 - Return the attenuated difference (scaled back to 0-100 range)
        return (needsAttenuation0 - needsAttenuation1) * 100f;
    }
}

