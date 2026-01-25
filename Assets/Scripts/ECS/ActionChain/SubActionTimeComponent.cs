using Unity.Entities;

public struct SubActionTimeComponent : IComponentData
{
    public float DeltaTime;
    public float TimeElapsed;
}
