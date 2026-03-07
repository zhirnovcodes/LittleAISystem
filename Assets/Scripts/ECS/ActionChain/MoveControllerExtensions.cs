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

    public static void ResetOutput(EntityCommandBuffer buffer, Entity entity)
    {
        buffer.SetComponent(entity, new MoveControllerOutputComponent
        {
            HasArrived = false,
            IsLookingAt = false
        });
    }

    /// <summary>
    /// Disables the MoveControllerInputComponent and resets the output
    /// </summary>
    public static void Disable(EntityCommandBuffer buffer, Entity entity)
    {
        buffer.SetComponentEnabled<MoveControllerInputComponent>(entity, false);

        ResetOutput(buffer, entity);
    }
}

