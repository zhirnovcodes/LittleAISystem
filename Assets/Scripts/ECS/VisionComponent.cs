using Unity.Entities;

public struct VisionComponent : IComponentData
{
    public float MaxDistance;

    public static implicit operator VisionComponent(GenomeData genomeData)
    {
        return new VisionComponent
        {
            MaxDistance = genomeData.Data.c0.x
        };
    }
}

