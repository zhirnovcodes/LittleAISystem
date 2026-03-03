using Unity.Entities;

public struct GenetaliaComponent : IComponentData
{
    public bool IsMale;
    public bool IsEnabled;

    public static implicit operator GenetaliaComponent(GenomeData genomeData)
    {
        return new GenetaliaComponent
        {
            IsMale = genomeData.Data.c0.x > 0.5f,
            IsEnabled = false
        };
    }
}

