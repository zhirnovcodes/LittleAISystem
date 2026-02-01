using Unity.Entities;

public struct NeedsActionChainComponent : IComponentData, IEnableableComponent
{
    public float CancelThreshold;
    public float AddThreshold;
}

