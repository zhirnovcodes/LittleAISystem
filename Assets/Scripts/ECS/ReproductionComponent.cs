using Unity.Entities;
using Unity.Mathematics;

public struct ReproductionComponent : IComponentData, IEnableableComponent
{
    public bool IsMale;
    public float GestationTime;
    public float TimeElapsed;
    public Random Random;

    public static implicit operator ReproductionComponent(GenomeData genomeData)
    {
        return new ReproductionComponent
        {
            IsMale = genomeData.Data.c0.x > 0.5f,
            GestationTime = genomeData.Data.c0.y,
            TimeElapsed = 0f,
            Random = default
        };
    }
}

