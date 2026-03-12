using Unity.Entities;
using Unity.Mathematics;

public static class MoveControllerExtensions
{
    // =========================================================================
    // Lookup-based methods (faster - direct memory access)
    // =========================================================================

    /// <summary>
    /// Sets the target using direct lookup access (faster than ECB)
    /// </summary>
    public static void SetTarget(
        this ref ComponentLookup<MoveControllerInputComponent> inputLookup,
        Entity entity,
        float3 targetPosition,
        float targetScale,
        float3 lookDirection,
        float distance,
        float speed,
        float rotationSpeed)
    {
        inputLookup[entity] = new MoveControllerInputComponent
        {
            TargetPosition = targetPosition,
            LookDirection = lookDirection,
            TargetScale = targetScale,
            Speed = speed,
            RotationSpeed = rotationSpeed,
            Distance = distance
        };
    }

    /// <summary>
    /// Sets the target entity using direct lookup access (faster than ECB)
    /// </summary>
    public static void SetTarget(
        this ref ComponentLookup<MoveControllerInputComponent> inputLookup,
        Entity entity,
        Entity targetEntity,
        float distance,
        float speed,
        float rotationSpeed)
    {
        inputLookup[entity] = new MoveControllerInputComponent
        {
            TargetEntity = targetEntity,
            Speed = speed,
            RotationSpeed = rotationSpeed,
            Distance = distance
        };
    }

    /// <summary>
    /// Resets the output using direct lookup access (faster than ECB)
    /// </summary>
    public static void ResetOutput(
        this ref ComponentLookup<MoveControllerOutputComponent> outputLookup,
        Entity entity)
    {
        outputLookup[entity] = new MoveControllerOutputComponent
        {
            HasArrived = false,
            IsLookingAt = false,
            IsFailed = false
        };
    }

    /// <summary>
    /// Resets the input using direct lookup access (faster than ECB)
    /// </summary>
    public static void ResetInput(
        this ref ComponentLookup<MoveControllerInputComponent> inputLookup,
        Entity entity)
    {
        inputLookup[entity] = new MoveControllerInputComponent
        {
            TargetEntity = Entity.Null,
            TargetPosition = float.MaxValue * new float3(1, 1, 1),
            LookDirection = float3.zero,
            TargetScale = 0,
            Speed = 0,
            RotationSpeed = 0,
            Distance = 0
        };
    }

    // =========================================================================
    // Enable/Disable (still require ECB for SetComponentEnabled)
    // =========================================================================

    /// <summary>
    /// Enables the MoveControllerInputComponent and sets speed to 0
    /// </summary>
    public static void Enable(
        this ref ComponentLookup<MoveControllerInputComponent> inputLookup,
        Entity entity)
    {
        inputLookup[entity] = new MoveControllerInputComponent
        {
            TargetEntity = Entity.Null,
            TargetPosition = float.MaxValue * new float3(1, 1, 1),
            LookDirection = float3.zero,
            TargetScale = 0,
            Speed = 0,
            RotationSpeed = 0,
            Distance = 0
        };
    }

    /// <summary>
    /// Disables the MoveControllerInputComponent, resets output, and sets speeds to 0
    /// </summary>
    public static void Disable(
        this ref ComponentLookup<MoveControllerInputComponent> inputLookup,
        ref ComponentLookup<MoveControllerOutputComponent> outputLookup,
        Entity entity)
    {
        outputLookup[entity] = new MoveControllerOutputComponent
        {
            HasArrived = false,
            IsLookingAt = false,
            IsFailed = false
        };

        inputLookup[entity] = new MoveControllerInputComponent
        {
            TargetEntity = Entity.Null,
            TargetPosition = float.MaxValue * new float3(1, 1, 1),
            LookDirection = float3.zero,
            TargetScale = 0,
            Speed = 0,
            RotationSpeed = 0,
            Distance = 0
        };
    }

    // =========================================================================
    // Legacy ECB-based methods (kept for compatibility)
    // =========================================================================

    /// <summary>
    /// [Legacy] Enables the MoveControllerInputComponent using ECB
    /// </summary>
    public static void Enable(EntityCommandBuffer buffer, Entity entity)
    {
        //buffer.SetComponentEnabled<MoveControllerInputComponent>(entity, true);
    }

    /// <summary>
    /// [Legacy] Sets the target using ECB (slower - deferred execution)
    /// </summary>
    public static void SetTarget(
        EntityCommandBuffer buffer,
        Entity entity,
        float3 targetPosition,
        float targetScale,
        float3 lookDirection,
        float distance,
        float speed,
        float rotationSpeed)
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

    /// <summary>
    /// [Legacy] Sets the target entity using ECB (slower - deferred execution)
    /// </summary>
    public static void SetTarget(
        EntityCommandBuffer buffer,
        Entity entity,
        Entity targetEntity,
        float distance,
        float speed,
        float rotationSpeed)
    {
        buffer.SetComponent(entity, new MoveControllerInputComponent
        {
            TargetEntity = targetEntity,
            Speed = speed,
            RotationSpeed = rotationSpeed,
            Distance = distance
        });
    }

    /// <summary>
    /// [Legacy] Resets the output using ECB (slower - deferred execution)
    /// </summary>
    public static void ResetOutput(EntityCommandBuffer buffer, Entity entity)
    {
        buffer.SetComponent(entity, new MoveControllerOutputComponent
        {
            HasArrived = false,
            IsLookingAt = false,
            IsFailed = false
        });
    }

    /// <summary>
    /// [Legacy] Resets the input using ECB (slower - deferred execution)
    /// </summary>
    public static void ResetInput(EntityCommandBuffer buffer, Entity entity)
    {
        buffer.SetComponent(entity, new MoveControllerInputComponent
        {
            TargetEntity = Entity.Null,
            TargetPosition = float.MaxValue * new float3(1, 1, 1),
            LookDirection = float3.zero,
            TargetScale = 0,
            RotationSpeed = 0,
            Distance = 0
        });
    }

    /// <summary>
    /// [Legacy] Disables the MoveControllerInputComponent and resets output using ECB
    /// </summary>
    public static void Disable(EntityCommandBuffer buffer, Entity entity)
    {
        //buffer.SetComponentEnabled<MoveControllerInputComponent>(entity, false);
        ResetOutput(buffer, entity);
        ResetInput(buffer, entity);
    }
}