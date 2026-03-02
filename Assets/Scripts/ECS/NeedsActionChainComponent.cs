using Unity.Entities;
using Unity.Mathematics;

public struct NeedsActionChainComponent : IComponentData, IEnableableComponent
{
    public float CancelThreshold;
    public float AddThreshold;

    public void SetFloat4(float4 data)
    {
        CancelThreshold = data.x;
        AddThreshold = data.y;
    }
}

