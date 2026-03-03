using Unity.Mathematics;

public struct AnimalStatsAttenuationBuilder
{
    private AnimalStatsAttenuation4x4 Attenuation;

    // Individual stat setters
    public AnimalStatsAttenuationBuilder WithEnergy(HermiteCurve needs, HermiteCurve distance)
    {
        Attenuation.Energy = new AnimalStatsAttenuation { Needs = needs, Distance = distance };
        return this;
    }

    public AnimalStatsAttenuationBuilder WithFullness(HermiteCurve needs, HermiteCurve distance)
    {
        Attenuation.Fullness = new AnimalStatsAttenuation { Needs = needs, Distance = distance };
        return this;
    }

    public AnimalStatsAttenuationBuilder WithToilet(HermiteCurve needs, HermiteCurve distance)
    {
        Attenuation.Toilet = new AnimalStatsAttenuation { Needs = needs, Distance = distance };
        return this;
    }

    public AnimalStatsAttenuationBuilder WithSocial(HermiteCurve needs, HermiteCurve distance)
    {
        Attenuation.Social = new AnimalStatsAttenuation { Needs = needs, Distance = distance };
        return this;
    }

    public AnimalStatsAttenuationBuilder WithSafety(HermiteCurve needs, HermiteCurve distance)
    {
        Attenuation.Safety = new AnimalStatsAttenuation { Needs = needs, Distance = distance };
        return this;
    }

    public AnimalStatsAttenuationBuilder WithHealth(HermiteCurve needs, HermiteCurve distance)
    {
        Attenuation.Health = new AnimalStatsAttenuation { Needs = needs, Distance = distance };
        return this;
    }

    public AnimalStatsAttenuation4x4 Build()
    {
        return Attenuation;
    }
}

