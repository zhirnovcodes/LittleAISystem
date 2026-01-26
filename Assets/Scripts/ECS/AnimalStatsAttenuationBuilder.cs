using Unity.Mathematics;

public struct AnimalStatsAttenuationBuilder
{
    private AnimalStatsAttenuation Attenuation;

    // Methods for setting individual NeedsAttenuation curves
    public AnimalStatsAttenuationBuilder WithEnergyNeeds(HermiteCurve curve)
    {
        Attenuation.EnergyNeedsAttenuation = curve;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithFullnessNeeds(HermiteCurve curve)
    {
        Attenuation.FullnessNeedsAttenuation = curve;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithToiletNeeds(HermiteCurve curve)
    {
        Attenuation.ToiletNeedsAttenuation = curve;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithSocialNeeds(HermiteCurve curve)
    {
        Attenuation.SocialNeedsAttenuation = curve;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithSafetyNeeds(HermiteCurve curve)
    {
        Attenuation.SafetyNeedsAttenuation = curve;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithHealthNeeds(HermiteCurve curve)
    {
        Attenuation.HealthNeedsAttenuation = curve;
        return this;
    }

    // Methods for setting individual DistanceAttenuation curves
    public AnimalStatsAttenuationBuilder WithEnergyDistance(HermiteCurve curve)
    {
        Attenuation.EnergyDistanceAttenuation = curve;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithFullnessDistance(HermiteCurve curve)
    {
        Attenuation.FullnessDistanceAttenuation = curve;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithToiletDistance(HermiteCurve curve)
    {
        Attenuation.ToiletDistanceAttenuation = curve;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithSocialDistance(HermiteCurve curve)
    {
        Attenuation.SocialDistanceAttenuation = curve;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithSafetyDistance(HermiteCurve curve)
    {
        Attenuation.SafetyDistanceAttenuation = curve;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithHealthDistance(HermiteCurve curve)
    {
        Attenuation.HealthDistanceAttenuation = curve;
        return this;
    }

    // Methods for setting MaxDistance values
    public AnimalStatsAttenuationBuilder WithEnergyMaxDistance(float distance)
    {
        Attenuation.EnergyMaxDistance = distance;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithFullnessMaxDistance(float distance)
    {
        Attenuation.FullnessMaxDistance = distance;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithToiletMaxDistance(float distance)
    {
        Attenuation.ToiletMaxDistance = distance;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithSocialMaxDistance(float distance)
    {
        Attenuation.SocialMaxDistance = distance;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithSafetyMaxDistance(float distance)
    {
        Attenuation.SafetyMaxDistance = distance;
        return this;
    }

    public AnimalStatsAttenuationBuilder WithHealthMaxDistance(float distance)
    {
        Attenuation.HealthMaxDistance = distance;
        return this;
    }

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

