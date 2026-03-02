using Unity.Entities;

public struct VisionComponent : IComponentData
{
    public float MaxDistance;
    public float Interval;
    public float TimeElapsed;

    public static implicit operator VisionComponent(GenomeData genomeData)
    {
        return new VisionComponent
        {
            MaxDistance = genomeData.Data.c0.x,
            Interval = genomeData.Data.c0.y,
            TimeElapsed = 0f
        };
    }
}

