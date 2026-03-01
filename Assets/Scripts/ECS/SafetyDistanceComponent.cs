using Unity.Entities;
using Unity.Mathematics;

public struct SafetyDistanceComponent : IComponentData
{
    public float SafeDistance;
    public float CheckInterval;

    public float4 ToFloat4()
    {
        return new float4(SafeDistance, CheckInterval, 0f, 0f);
    }

    public static SafetyDistanceComponent FromFloat4(float4 data)
    {
        return new SafetyDistanceComponent
        {
            SafeDistance = data.x,
            CheckInterval = data.y
        };
    }
}

