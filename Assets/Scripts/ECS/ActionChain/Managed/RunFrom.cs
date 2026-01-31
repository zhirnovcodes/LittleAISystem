using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class RunFrom : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;

    private const float MoveSpeed = 1.0f;
    private const float SafeDistance = 5.0f;

    public RunFrom(ComponentLookup<LocalTransform> transformLookup)
    {
        TransformLookup = transformLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to enable for run from
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for run from
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

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // If distance >= SafeDistance - success
        if (entityTransform.IsDistanceGreaterThan(targetTransform, SafeDistance))
        {
            return SubActionResult.Success();
        }

        // Move in direction opposite from target
        var directionFromTarget = entityTransform.Position - targetTransform.Position;
        var normalizedDirection = math.normalize(directionFromTarget);
        var moveDistance = MoveSpeed * timer.DeltaTime;

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

