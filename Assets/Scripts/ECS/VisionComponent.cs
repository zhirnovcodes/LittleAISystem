using Unity.Entities;
using Unity.Mathematics;

public struct VisionComponent : IComponentData
{
    public float MaxDistance;
    public float Interval;
    public float TimeElapsed;

    public void SetFloat4(float4 data)
    {
        MaxDistance = data.x;
        Interval = data.y;
    }
}

