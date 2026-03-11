using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public static class MoveControllerExtensions
{
    /// <summary>
    /// Enables the MoveControllerInputComponent
    /// </summary>
    public static void Enable(EntityCommandBuffer buffer, Entity entity)
    {
        buffer.SetComponentEnabled<MoveControllerInputComponent>(entity, true);
    }

    /// <summary>
    /// Sets the target - calculates look direction automatically and resets output
    /// </summary>
    public static void SetTarget(EntityCommandBuffer buffer, Entity entity, float3 targetPosition, float targetScale, float3 lookDirection, float distance, float speed, float rotationSpeed)
    {
        buffer.SetComponent(entity, new MoveControllerInputComponent
        {
            TargetPosition = targetPosition,
            LookDirection = lookDirection,
            TargetScale = targetScale,
            Speed = speed,
            RotationSpeed = rotationSpeed,
            Distance = distance
        });
    }

    public static void SetTarget(EntityCommandBuffer buffer, Entity entity, Entity targetEntity, float distance, float speed, float rotationSpeed)
    {
        buffer.SetComponent(entity, new MoveControllerInputComponent
        {
            TargetEntity = targetEntity,
            Speed = speed,
            RotationSpeed = rotationSpeed,
            Distance = distance
        });
    }

    public static void ResetOutput(EntityCommandBuffer buffer, Entity entity)
    {
        buffer.SetComponent(entity, new MoveControllerOutputComponent
        {
            HasArrived = false,
            IsLookingAt = false,
            IsFailed = false
        });
    }

    public static void ResetInput(EntityCommandBuffer buffer, Entity entity)
    {
        buffer.SetComponent(entity, new MoveControllerInputComponent
        {
            TargetEntity = Entity.Null,
            TargetPosition = float.MaxValue * new float3(1, 1, 1),
            LookDirection = float3.zero,
            TargetScale = 0,
            Speed = 0,
            RotationSpeed = 0,
            Distance = 0
        });
    }

    /// <summary>
    /// Disables the MoveControllerInputComponent and resets the output
    /// </summary>
    public static void Disable(EntityCommandBuffer buffer, Entity entity)
    {
        buffer.SetComponentEnabled<MoveControllerInputComponent>(entity, false);

        ResetOutput(buffer, entity);

        ResetInput(buffer, entity);
    }
}

