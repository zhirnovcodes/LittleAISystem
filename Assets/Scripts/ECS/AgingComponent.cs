using Unity.Entities;

public struct AgingComponent : IComponentData
{
    public float MinSize;
    public float MaxSize;

    public static implicit operator AgingComponent(GenomeData genomeData)
    {
        return new AgingComponent
        {
            MinSize = genomeData.Data.c0.x,
            MaxSize = genomeData.Data.c0.y
        };
    }
}

