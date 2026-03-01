using Unity.Entities;
using Unity.Mathematics;

public struct SleepDataComponent : IComponentData
{
    public float FailTime;
    public float MaxDistance;
    public float LayDownFailTime;
    public float Distance;

    public float4 ToFloat4()
    {
        return new float4(FailTime, MaxDistance, LayDownFailTime, Distance);
    }

    public static SleepDataComponent FromFloat4(float4 data)
    {
        return new SleepDataComponent
        {
            FailTime = data.x,
            MaxDistance = data.y,
            LayDownFailTime = data.z,
            Distance = data.w
        };
    }
}

