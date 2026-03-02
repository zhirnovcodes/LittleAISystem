using Unity.Entities;
using Unity.Mathematics;

public struct TransformData : IComponentData
{
    public float MinSize;
    public float MaxSize;

    public float4 ToFloat4()
    {
        return new float4(MinSize, MaxSize, 0, 0);
    }

    public static TransformData FromFloat4(float4 data)
    {
        return new TransformData
        {
            MinSize = data.x,
            MaxSize = data.y
        };
    }
}

