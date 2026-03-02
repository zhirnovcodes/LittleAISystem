using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class LayDownState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<DNAComponent> DNALookup;
    private ComponentLookup<MovingDataComponent> MovingDataLookup;
    private ComponentLookup<SleepDataComponent> SleepDataLookup;

    public LayDownState(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<DNAComponent> dnaLookup, ComponentLookup<MovingDataComponent> movingDataLookup, ComponentLookup<SleepDataComponent> sleepDataLookup)
    {
        TransformLookup = transformLookup;
        DNALookup = dnaLookup;
        MovingDataLookup = movingDataLookup;
        SleepDataLookup = sleepDataLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        DNALookup.Update(system);
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

        // Get DNA entity first
        if (!DNALookup.HasComponent(entity))
        {
            return SubActionResult.Fail(7);
        }

        var dnaEntity = DNALookup[entity].DNA;

        // Get moving data from DNA entity
        if (!MovingDataLookup.HasComponent(dnaEntity))
        {
            return SubActionResult.Fail(7);
        }

        // Get sleep data from DNA entity
        if (!SleepDataLookup.HasComponent(dnaEntity))
        {
            return SubActionResult.Fail(8);
        }

        var movingData = MovingDataLookup[dnaEntity];
        var sleepData = SleepDataLookup[dnaEntity];

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

