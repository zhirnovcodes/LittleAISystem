using Unity.Entities;
using Unity.Mathematics;

public struct NeedBasedData : IComponentData
{
    public float CancelThreshold;
    public float AddThreshold;

    public float4 ToFloat4()
    {
        return new float4(CancelThreshold, AddThreshold, 0, 0);
    }

    public static NeedBasedData FromFloat4(float4 data)
    {
        return new NeedBasedData
        {
            CancelThreshold = data.x,
            AddThreshold = data.y
        };
    }
}

