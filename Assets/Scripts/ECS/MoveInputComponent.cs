using Unity.Entities;
using Unity.Mathematics;

public struct MoveInputComponent : IComponentData
{
    public Entity Target;
    public float3 Up;
    public float MaxDistance;
    public float Speed;
    public float RotationSpeed;
    public float RotationDelta;
}

public struct MoveOutputComponent : IComponentData
{
    public float3 TargetPosition;
    public float3 Position;
    public quaternion Rotation;
}
