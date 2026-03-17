using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class RunFrom : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MoveControllerInputComponent> MoveControllerInputLookup;
    private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;

    public RunFrom(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<MoveControllerInputComponent> moveControllerInputLookup, ComponentLookup<MovingSpeedComponent> movingSpeedLookup)
    {
        TransformLookup = transformLookup;
        MoveControllerInputLookup = moveControllerInputLookup;
        MovingSpeedLookup = movingSpeedLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        MoveControllerInputLookup.Update(system);
        MovingSpeedLookup.Update(system);
    }

    private void SetRandomEscapeTarget(EntityCommandBuffer buffer, Entity entity, float3 entityPosition, float3 targetPosition, ref Random random)
    {
        // Generate new random position
        var movingSpeed = MovingSpeedLookup[entity];
        var safeDistance = new float2(1, 1.5f) * SubActionConsts.RunFrom.SafeDistance;
        var escapePoition = LocalTransformExtensions.GenerateRandomEscapePosition(entityPosition, targetPosition, safeDistance, ref random);
        var lookDirection = math.normalize(escapePoition - entityPosition);

        MoveControllerExtensions.SetTarget(buffer, entity, escapePoition, 0, lookDirection, 0.01f, movingSpeed.GetRunningSpeed(), movingSpeed.GetRunningRotationSpeed());
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        // Check if entity does not exist in transform lookup, skip setup
        if (!TransformLookup.TryGetComponent(entity, out var entityTransform) || 
            !TransformLookup.TryGetComponent(target, out var targetTransform))
        {
            return;
        }

        // if entity does not have MovingSpeedComponent - skip setup
        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return;
        }


        // Enable and set initial target
        MoveControllerExtensions.Enable(buffer, entity);
        SetRandomEscapeTarget(buffer, entity, entityTransform.Position, targetTransform.Position, ref random);
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Disable using extension method
        MoveControllerExtensions.ResetInput(buffer, entity);
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        // Check if entity does not exist in transform lookup, fail state. code = 0
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        // Check if target does not exist in transform lookup, fail state. code = 1
        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // If distance >= SafeDistance - success
        if (entityTransform.IsDistanceGreaterThan(targetTransform, SubActionConsts.RunFrom.SafeDistance))
        {
            return SubActionResult.Success();
        }

        // if entity does not have MovingSpeedComponent - return fail with code 2
        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(2);
        }

        if (!MoveControllerInputLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        var moveInput = MoveControllerInputLookup[entity];

        // If arrived at current target, set new random target
        if (entityTransform.IsTargetDistanceReached(moveInput.TargetPosition, moveInput.TargetScale, moveInput.Distance))
        {
            SetRandomEscapeTarget(buffer, entity, entityTransform.Position, targetTransform.Position, ref random);
        }

        return SubActionResult.Running();
    }
}

