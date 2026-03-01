using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class WalkToSubActionState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MovingDataComponent> MovingDataLookup;

    public WalkToSubActionState(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<MovingDataComponent> movingDataLookup)
    {
        TransformLookup = transformLookup;
        MovingDataLookup = movingDataLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        MovingDataLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to enable for walk
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for walk
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

        // Get moving data from entity
        if (!MovingDataLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(7);
        }

        var movingData = MovingDataLookup[entity];
        float moveSpeed = movingData.MaxSpeed * movingData.WalkingSpeedT;
        float failTime = movingData.MoveFailTime;
        float rotationSpeed = movingData.MaxRotationSpeed * movingData.WalkingRotationSpeedT;

        // If time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(failTime))
        {
            return SubActionResult.Fail(2);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // After distance < 0.001 - returns success
        if (entityTransform.IsTargetReached(targetTransform, 0.001f))
        {
            return SubActionResult.Success();
        }

        // Move towards target
        var newTransform = entityTransform.MovePositionTowards(targetTransform, timer.DeltaTime, moveSpeed);

        // Rotate towards target
        newTransform = newTransform.RotateTowards(targetTransform, rotationSpeed * timer.DeltaTime, 0.01f);

        buffer.SetComponent(entity, newTransform);

        return SubActionResult.Running();
    }
}

