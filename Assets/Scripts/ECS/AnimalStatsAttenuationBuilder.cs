using Unity.Mathematics;

public struct AnimalStatsAttenuationBuilder
{
    private AnimalStatsAttenuation Attenuation;

    // Bulk setters
    public AnimalStatsAttenuationBuilder WithNeedsAttenuations(HermiteCurve4x2 curves)
    {
        Attenuation.NeedsAttenuation = curves;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithDistanceAttenuations(HermiteCurve4x2 curves)
    {
        Attenuation.DistanceAttenuation = curves;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithMaxDistances(float4x2 distances)
    {
        Attenuation.MaxDistance = distances;
        return this;
    }

    public AnimalStatsAttenuation Build()
    {
        return Attenuation;
    }
}

