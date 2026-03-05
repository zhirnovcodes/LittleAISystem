using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class WalkToTalk : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;
    private ComponentLookup<MoveControllerOutputComponent> MoveControllerOutputLookup;

    private const float MaxDistance = 0.2f;
    private const float FailTime = 30f;

    public WalkToTalk(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<MovingSpeedComponent> movingSpeedLookup, ComponentLookup<MoveControllerOutputComponent> moveControllerOutputLookup)
    {
        TransformLookup = transformLookup;
        MovingSpeedLookup = movingSpeedLookup;
        MoveControllerOutputLookup = moveControllerOutputLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        MovingSpeedLookup.Update(system);
        MoveControllerOutputLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        // Enable MoveController
        MoveControllerExtensions.Enable(buffer, entity);
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Disable using extension method
        MoveControllerExtensions.Disable(buffer, entity);
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

        // If time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(FailTime))
        {
            return SubActionResult.Fail(2);
        }

        // if entity does not have MovingSpeedComponent - return fail with code 3
        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        // if entity does not have MoveControllerOutputComponent - return fail with code 4
        if (!MoveControllerOutputLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(4);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // Check if we've reached the target distance
        if (entityTransform.IsTargetReached(targetTransform, MaxDistance))
        {
            return SubActionResult.Success();
        }

        // Update target position
        MoveControllerExtensions.SetTarget(buffer, entity, entityTransform.Position,
            targetTransform.Position, entityTransform.Scale, targetTransform.Scale, MovingSpeedLookup[entity].GetWalkingSpeed(), 
            MovingSpeedLookup[entity].GetWalkingRotationSpeed());

        return SubActionResult.Running();
    }
}

