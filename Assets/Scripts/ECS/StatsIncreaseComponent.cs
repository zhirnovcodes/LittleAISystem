using Unity.Entities;

public struct StatsIncreaseComponent : IComponentData
{
    public AnimalStats AnimalStats; // stat increase per second, when performing increasing action

    public static implicit operator StatsIncreaseComponent(GenomeData genomeData)
    {
        return new StatsIncreaseComponent
        {
            AnimalStats =  genomeData.Data
        };
    }
}

