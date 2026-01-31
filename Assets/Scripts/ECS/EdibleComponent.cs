using Unity.Entities;

public struct EdibleComponent : IComponentData
{
    public Entity EdibleBody;
    public float Nutrition;
}

