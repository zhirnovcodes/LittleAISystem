using Unity.Entities;

public struct VisionComponent : IComponentData
{
    public float MaxDistance;
    public float Interval;
    public float TimeElapsed;
}

