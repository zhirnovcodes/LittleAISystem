using Unity.Entities;
using Unity.Mathematics;

public struct EatDataComponent : IComponentData
{
    public float Interval;
    public float FailTime;
    public float MaxDistance;
    public float BiteSize;

    public float4 ToFloat4()
    {
        return new float4(Interval, FailTime, MaxDistance, BiteSize);
    }

    public static EatDataComponent FromFloat4(float4 data)
    {
        return new EatDataComponent
        {
            Interval = data.x,
            FailTime = data.y,
            MaxDistance = data.z,
            BiteSize = data.w
        };
    }
}

