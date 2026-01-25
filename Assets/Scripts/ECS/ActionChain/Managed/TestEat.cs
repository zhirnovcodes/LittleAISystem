using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class TestEat : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;

    private const float EatDuration = 3.0f;
    private const float RotationSpeed = 5.0f;

    public TestEat(ComponentLookup<LocalTransform> transformLookup)
    {
        TransformLookup = transformLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to enable for eat
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for eat
    }

    public SubActionStatus Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer)
    {
        // Check if both entities have required components
        if (!TransformLookup.HasComponent(entity) || !TransformLookup.HasComponent(target))
        {
            return SubActionStatus.Fail;
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // Calculate direction to target
        var directionToTarget = targetTransform.Position - entityTransform.Position;
        
        // Rotate towards target using transform rotation
        if (math.lengthsq(directionToTarget) > 0.001f)
        {
            directionToTarget = math.normalize(directionToTarget);
            
            // Calculate target rotation to face the target
            var targetRotation = quaternion.LookRotationSafe(directionToTarget, new float3(0, 1, 0));
            
            // Smoothly interpolate rotation
            var t = math.min(1.0f, RotationSpeed * timer.DeltaTime);
            var newRotation = math.slerp(entityTransform.Rotation, targetRotation, t);
            
            buffer.SetComponent(entity, new LocalTransform
            {
                Position = entityTransform.Position,
                Rotation = newRotation,
                Scale = entityTransform.Scale
            });
        }

        // Check if eating duration is complete
        if (timer.TimeElapsed >= EatDuration)
        {
            return SubActionStatus.Success;
        }

        return SubActionStatus.Running;
    }
}

