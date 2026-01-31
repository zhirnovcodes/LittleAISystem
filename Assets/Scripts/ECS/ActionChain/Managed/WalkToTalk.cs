using Unity.Entities;
using Unity.Transforms;

public class WalkToTalk : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;

    private const float MaxDistance = 1.0f;
    private const float MinDistance = 0.6f;
    private const float MoveSpeedMax = 1.0f;
    private const float MoveSpeedMin = 0.5f;
    private const float SpeedReduceDistance = 0.5f;
    private const float FailTime = 15f;
    private const float RotationSpeed = 30f;

    public WalkToTalk(ComponentLookup<LocalTransform> transformLookup)
    {
        TransformLookup = transformLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
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
            // Check if we're too close (less than MinDistance)
            if (entityTransform.IsDistanceGreaterThan(targetTransform, MinDistance))
            {
            // In the sweet spot between MinDistance and MaxDistance
                return SubActionResult.Success();
            }

            // Too close - move away and rotate away
            MoveOut(entity, entityTransform, targetTransform, buffer, timer);
            return SubActionResult.Running();
        }

        // Move towards target
        MoveTowards(entity, entityTransform, targetTransform, buffer, timer);
        return SubActionResult.Running();
    }

    private void MoveOut(Entity entity, LocalTransform entityTransform, LocalTransform targetTransform, EntityCommandBuffer buffer, in SubActionTimeComponent timer)
    {
        var transformMoveAway = entityTransform.MovePositionAwayFrom(targetTransform, timer.DeltaTime * MoveSpeedMin);

        // Rotate away from target
        var directionAwayFromTarget = entityTransform.Position - targetTransform.Position;
        transformMoveAway = transformMoveAway.RotateTowards(directionAwayFromTarget, RotationSpeed * timer.DeltaTime, 0.01f);

        buffer.SetComponent(entity, transformMoveAway);
    }

    private void MoveTowards(Entity entity, LocalTransform entityTransform, LocalTransform targetTransform, EntityCommandBuffer buffer, in SubActionTimeComponent timer)
    {
        // Determine move speed based on distance
        var isDistanceGreaterThan = entityTransform.IsDistanceGreaterThan(targetTransform, SpeedReduceDistance + MaxDistance);
        float moveSpeed = isDistanceGreaterThan ? MoveSpeedMax : MoveSpeedMin;

        // Move towards target
        var newTransform = entityTransform.MovePositionTowards(targetTransform, timer.DeltaTime, moveSpeed);

        // Rotate towards target
        newTransform = newTransform.RotateTowards(targetTransform, RotationSpeed * timer.DeltaTime, 0.01f);

        buffer.SetComponent(entity, newTransform);
    }
}

