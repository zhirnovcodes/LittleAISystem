using Unity.Entities;

public struct AnimalStatsComponent : IComponentData
{
    public AnimalStats Stats;

    public static implicit operator AnimalStatsComponent(GenomeData genomeData)
    {
        return new AnimalStatsComponent
        {
            Stats = genomeData.Data
        };
    }
}

