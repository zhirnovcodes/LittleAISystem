using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class IdleSubActionState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;

    private const float IdleTime = 10f;
    private const float WanderRadius = 10f;

    public IdleSubActionState(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<MovingSpeedComponent> movingSpeedLookup)
    {
        TransformLookup = transformLookup;
        MovingSpeedLookup = movingSpeedLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        MovingSpeedLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        // Check if entity has required components
        if (!TransformLookup.HasComponent(entity) || !MovingSpeedLookup.HasComponent(entity))
        {
            return;
        }

        var entityTransform = TransformLookup[entity];
        var movingSpeed = MovingSpeedLookup[entity];

        // Generate random position around entity
        var targetPosition = LocalTransformExtensions.GenerateRandomPosition(entityTransform.Position, WanderRadius, ref random);
        var lookDirection = math.normalize(targetPosition - entityTransform.Position);

        // Enable and set random target
        MoveControllerExtensions.Enable(buffer, entity);
        MoveControllerExtensions.SetTarget(buffer, entity, targetPosition, 0, lookDirection, 0.01f, movingSpeed.GetWalkingSpeed(), movingSpeed.GetWalkingRotationSpeed());
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        MoveControllerExtensions.Disable(buffer, entity);
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (timer.IsTimeout(IdleTime))
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }
}

