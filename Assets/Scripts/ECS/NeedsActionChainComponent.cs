using Unity.Entities;

public struct NeedsActionChainComponent : IComponentData, IEnableableComponent
{
    public float CancelThreshold;
    public float AddThreshold;

    public static implicit operator NeedsActionChainComponent(GenomeData genomeData)
    {
        return new NeedsActionChainComponent
        {
            CancelThreshold = genomeData.Data.c0.x,
            AddThreshold = genomeData.Data.c0.y
        };
    }
}

