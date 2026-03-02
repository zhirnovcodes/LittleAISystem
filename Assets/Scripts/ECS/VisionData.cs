using Unity.Entities;
using Unity.Mathematics;

public struct VisionData : IComponentData
{
    public float MaxDistance;
    public float Interval;

    public float4 ToFloat4()
    {
        return new float4(MaxDistance, Interval, 0, 0);
    }

    public static VisionData FromFloat4(float4 data)
    {
        return new VisionData
        {
            MaxDistance = data.x,
            Interval = data.y
        };
    }
}

