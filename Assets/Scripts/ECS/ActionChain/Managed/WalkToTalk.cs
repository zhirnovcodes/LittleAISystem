using Unity.Entities;
using Unity.Transforms;

public class WalkToTalk : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<TalkingDataComponent> TalkingDataLookup;
    private ComponentLookup<MovingDataComponent> MovingDataLookup;

    public WalkToTalk(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<TalkingDataComponent> talkingDataLookup, ComponentLookup<MovingDataComponent> movingDataLookup)
    {
        TransformLookup = transformLookup;
        TalkingDataLookup = talkingDataLookup;
        MovingDataLookup = movingDataLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        TalkingDataLookup.Update(system);
        MovingDataLookup.Update(system);
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

        // Get talking data from entity
        if (!TalkingDataLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(7);
        }

        // Get moving data from entity
        if (!MovingDataLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(8);
        }

        var talkingData = TalkingDataLookup[entity];
        var movingData = MovingDataLookup[entity];

        float maxSpeed = movingData.MaxSpeed;
        float maxRotationSpeed = movingData.MaxRotationSpeed;
        float maxDistance = talkingData.MaxDistance;
        float moveSpeed = movingData.WalkingSpeedT * maxSpeed;
        float failTime = movingData.MoveFailTime;
        float rotationSpeed = movingData.WalkingRotationSpeedT * maxRotationSpeed;

        // If time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(failTime))
        {
            return SubActionResult.Fail(2);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // Check if we've reached the target distance
        if (entityTransform.IsTargetReached(targetTransform, maxDistance))
        {
            return SubActionResult.Success();
        }

        // Move towards target
        MoveTowards(entity, entityTransform, targetTransform, buffer, timer, moveSpeed, rotationSpeed);
        return SubActionResult.Running();
    }

    private void MoveTowards(Entity entity, LocalTransform entityTransform, LocalTransform targetTransform, EntityCommandBuffer buffer, in SubActionTimeComponent timer, float moveSpeed, float rotationSpeed)
    {
        // Move towards target
        var newTransform = entityTransform.MovePositionTowards(targetTransform, timer.DeltaTime, moveSpeed);

        // Rotate towards target
        newTransform = newTransform.RotateTowards(targetTransform, rotationSpeed * timer.DeltaTime, 0.01f);

        buffer.SetComponent(entity, newTransform);
    }
}

