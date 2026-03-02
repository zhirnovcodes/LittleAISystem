using Unity.Entities;
using Unity.Mathematics;

public struct SafetyDataComponent : IComponentData
{
    public float SafeDistance;
    public float CheckInterval;

    public float4 ToFloat4()
    {
        return new float4(SafeDistance, CheckInterval, 0f, 0f);
    }

    public static SafetyDataComponent FromFloat4(float4 data)
    {
        return new SafetyDataComponent
        {
            SafeDistance = data.x,
            CheckInterval = data.y
        };
    }
}

