using Unity.Entities;
using Unity.Transforms;

public class WalkToTalk : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;

    private const float MaxDistance = 0.2f;
    private const float FailTime = 30f;

    public WalkToTalk(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<MovingSpeedComponent> movingSpeedLookup)
    {
        TransformLookup = transformLookup;
        MovingSpeedLookup = movingSpeedLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        MovingSpeedLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to enable for walk to talk
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for walk to talk
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer)
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

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // Check if we've reached the target distance
        if (entityTransform.IsTargetReached(targetTransform, MaxDistance))
        {
            return SubActionResult.Success();
        }

        // if entity does not have MovingSpeedComponent - return fail with code 3
        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        // Move towards target
        var movingSpeed = MovingSpeedLookup[entity];
        MoveTowards(entity, entityTransform, targetTransform, buffer, timer, movingSpeed);
        return SubActionResult.Running();
    }

    private void MoveTowards(Entity entity, LocalTransform entityTransform, LocalTransform targetTransform, EntityCommandBuffer buffer, in SubActionTimeComponent timer, MovingSpeedComponent movingSpeed)
    {
        // Move towards target using walking speed
        var newTransform = entityTransform.MovePositionTowards(targetTransform, timer.DeltaTime, movingSpeed.GetWalkingSpeed());

        // Rotate towards target using walking rotation speed
        newTransform = newTransform.RotateTowards(targetTransform, movingSpeed.GetWalkingRotationSpeed() * timer.DeltaTime, 0.01f);

        buffer.SetComponent(entity, newTransform);
    }
}

