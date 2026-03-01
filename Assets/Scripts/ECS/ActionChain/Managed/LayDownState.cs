using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class LayDownState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MovingDataComponent> MovingDataLookup;
    private ComponentLookup<SleepDataComponent> SleepDataLookup;

    public LayDownState(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<MovingDataComponent> movingDataLookup, ComponentLookup<SleepDataComponent> sleepDataLookup)
    {
        TransformLookup = transformLookup;
        MovingDataLookup = movingDataLookup;
        SleepDataLookup = sleepDataLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        MovingDataLookup.Update(system);
        SleepDataLookup.Update(system);
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

        // Get moving data from entity
        if (!MovingDataLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(7);
        }

        // Get sleep data from entity
        if (!SleepDataLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(8);
        }

        var movingData = MovingDataLookup[entity];
        var sleepData = SleepDataLookup[entity];

        float moveSpeed = movingData.CrawlingSpeedT;
        float failTime = sleepData.LayDownFailTime;
        float distance = sleepData.Distance;

        // If time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(failTime))
        {
            return SubActionResult.Fail(2);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // After distance < Distance - returns success
        if (entityTransform.IsTargetPositionReached(targetTransform.Position, distance))
        {
            return SubActionResult.Success();
        }

        // Move towards target position (targetScale = 0)
        var newTransform = entityTransform.MovePositionTowards(targetTransform.Position, 0, timer.DeltaTime, moveSpeed);

        buffer.SetComponent(entity, newTransform);

        return SubActionResult.Running();
    }
}

