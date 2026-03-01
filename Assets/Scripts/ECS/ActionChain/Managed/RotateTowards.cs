using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class RotateTowards : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MovingDataComponent> MovingDataLookup;

    public RotateTowards(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<MovingDataComponent> movingDataLookup)
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
        // Nothing to enable for rotate
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for rotate
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
        float failTime = movingData.RotateFailTime;
        float rotationSpeed = movingData.MaxRotationSpeed;

        // If time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(failTime))
        {
            return SubActionResult.Fail(2);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // Check if already facing target
        if (entityTransform.IsLookingTowards(targetTransform.Position, 0.01f))
        {
            return SubActionResult.Success();
        }

        // Rotate towards target (multiply speed by deltaTime)
        var newTransform = entityTransform.RotateTowards(targetTransform, rotationSpeed * timer.DeltaTime, 0.01f);

        buffer.SetComponent(entity, newTransform);

        return SubActionResult.Running();
    }
}

