using Unity.Entities;
using Unity.Mathematics;

public struct MoveControllerInputComponent : IComponentData
{
    public Entity TargetEntity;
    public float Speed;
    public float RotationSpeed;
    public float3 TargetPosition;
    public float3 LookDirection;
    public float TargetScale;
    public float Distance;
}

