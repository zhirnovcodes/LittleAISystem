using Unity.Entities;
using Unity.Mathematics;

public struct TestMoveTargetItem : IBufferElementData
{
    public Entity Target;
    public float3 TargetPosition;
    public float Speed;
    public float RotationSpeed;
    public float MaxDistance;
}

public struct TestMoveComponent : IComponentData
{
    public int CurrentIndex;
}
