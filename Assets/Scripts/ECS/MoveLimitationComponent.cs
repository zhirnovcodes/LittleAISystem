using Unity.Entities;
using Unity.Mathematics;
public struct MoveLimitationComponent : IComponentData
{
    public float3 Central;
    public float3 Scale;

    public static implicit operator MoveLimitationComponent(GenomeData genomeData)
    {
        return new MoveLimitationComponent
        {
            Central = genomeData.Data.c0.xyz,
            Scale = genomeData.Data.c1.xyz
        };
    }
}