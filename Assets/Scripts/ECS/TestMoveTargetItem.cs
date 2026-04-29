using Unity.Entities;

public struct TestMoveTargetItem : IBufferElementData
{
    public Entity Target;
    public float Speed;
    public float RotationSpeed;
    public float MaxDistance;
}

public struct TestMoveComponent : IComponentData
{
    public int CurrentIndex;
}
