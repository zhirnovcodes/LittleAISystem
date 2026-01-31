using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class TestMoveTo : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private Random RandomGenerator;

    private const float MoveSpeed = 5.0f;

    public TestMoveTo(ComponentLookup<LocalTransform> transformLookup)
    {
        TransformLookup = transformLookup;
        RandomGenerator = Random.CreateFromIndex((uint)System.DateTime.Now.Ticks);
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to enable for move
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for move
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer)
    {
        // Check if both entities have required components
        if (!TransformLookup.HasComponent(entity) || !TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        var directionToTarget = targetTransform.Position - entityTransform.Position;
        var distanceToTarget = math.length(directionToTarget);

        // Check if reached target
        var reachDistance = entityTransform.Scale / 2f + targetTransform.Scale / 2f;
        if (distanceToTarget <= reachDistance)
        {
            return SubActionResult.Success();
        }

        // Move towards target using transform position
        var normalizedDirection = directionToTarget / distanceToTarget;
        var moveDistance = MoveSpeed * timer.DeltaTime;
        
        // Clamp movement to not overshoot target
        if (moveDistance > distanceToTarget)
        {
            moveDistance = distanceToTarget;
        }

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

