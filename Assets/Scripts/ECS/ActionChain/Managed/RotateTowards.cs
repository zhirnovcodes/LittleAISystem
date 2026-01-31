using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class RotateTowards : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;

    private const float FailTime = 4f;
    private const float RotationSpeed = 10f; // speed in degrees per second

    public RotateTowards(ComponentLookup<LocalTransform> transformLookup)
    {
        TransformLookup = transformLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
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

        // If time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(FailTime))
        {
            return SubActionResult.Fail(2);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // Calculate direction to target
        var directionToTarget = targetTransform.Position - entityTransform.Position;
        
        // Check if already facing target
        if (entityTransform.Rotation.IsRotationTowardsTargetReached(directionToTarget, 0.01f))
        {
            return SubActionResult.Success();
        }

        // Calculate target rotation
        var targetRotation = quaternion.LookRotationSafe(directionToTarget, math.up());

        // Convert rotation speed from degrees to radians per second
        float rotationRadians = math.radians(RotationSpeed);

        // Slerp towards target rotation
        float t = math.min(1.0f, rotationRadians * timer.DeltaTime);
        var newRotation = math.slerp(entityTransform.Rotation, targetRotation, t);

        buffer.SetComponent(entity, new LocalTransform
        {
            Position = entityTransform.Position,
            Rotation = newRotation,
            Scale = entityTransform.Scale
        });

        return SubActionResult.Running();
    }
}

