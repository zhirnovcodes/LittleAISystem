using Unity.Mathematics;

public struct AnimalStatsBuilder
{
    private AnimalStats Stats;

    public AnimalStatsBuilder WithEnergy(float value)
    {
        Stats.SetEnergy(value);
        return this;
    }

    public AnimalStatsBuilder WithFullness(float value)
    {
        Stats.SetFullness(value);
        return this;
    }

    public AnimalStatsBuilder WithToilet(float value)
    {
        Stats.SetToilet(value);
        return this;
    }

    public AnimalStatsBuilder WithSocial(float value)
    {
        Stats.SetSocial(value);
        return this;
    }

    public AnimalStatsBuilder WithSafety(float value)
    {
        Stats.SetSafety(value);
        return this;
    }

    public AnimalStatsBuilder WithHealth(float value)
    {
        Stats.SetHealth(value);
        return this;
    }

    public AnimalStats Build()
    {
        return Stats;
    }
}

