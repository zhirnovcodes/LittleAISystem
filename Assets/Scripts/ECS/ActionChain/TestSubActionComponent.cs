using Unity.Entities;

public struct TestSubActionComponent : IComponentData
{
    public int CurrentSubActionIndex;
    public Entity Target;
}

