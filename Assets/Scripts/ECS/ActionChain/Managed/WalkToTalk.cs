using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class WalkToTalk : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;

    private const float Distance = 1.0f;
    private const float MoveSpeedMax = 1.0f;
    private const float MoveSpeedMin = 0.5f;
    private const float SpeedReduceDistance = 0.5f;
    private const float FailTime = 15f;

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
        if (entityTransform.IsTargetReached(targetTransform, Distance))
        {
            return SubActionResult.Success();
        }

        // Determine move speed based on distance
        float moveSpeed = !entityTransform.IsDistanceGreaterThan(targetTransform, SpeedReduceDistance + Distance) ? MoveSpeedMin : MoveSpeedMax;

        // Move towards target
        var directionToTarget = targetTransform.Position - entityTransform.Position;
        var distance = math.length(directionToTarget);
        var normalizedDirection = directionToTarget / distance;
        var moveDistance = moveSpeed * timer.DeltaTime;

        var newPosition = entityTransform.Position + normalizedDirection * moveDistance;

        buffer.SetComponent(entity, new LocalTransform
        {
            Position = newPosition,
            Rotation = entityTransform.Rotation,
            Scale = entityTransform.Scale
        });

        return SubActionResult.Running();
    }
}

