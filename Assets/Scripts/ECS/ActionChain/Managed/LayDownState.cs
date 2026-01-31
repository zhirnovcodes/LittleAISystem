using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class LayDownState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;

    private const float MoveSpeed = 1.0f;
    private const float FailTime = 15f;
    private const float Distance = 0.1f;

    public LayDownState(ComponentLookup<LocalTransform> transformLookup)
    {
        TransformLookup = transformLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to enable for lay down
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for lay down
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

        // After distance < Distance - returns success
        if (entityTransform.IsTargetPositionReached(targetTransform.Position, Distance))
        {
            return SubActionResult.Success();
        }

        // Move towards target position (targetScale = 0)
        var newTransform = entityTransform.MovePositionTowards(targetTransform.Position, 0, timer.DeltaTime, MoveSpeed);

        buffer.SetComponent(entity, newTransform);

        return SubActionResult.Running();
    }
}

