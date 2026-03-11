using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class IdleSubActionState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;
    private ComponentLookup<MoveControllerOutputComponent> OutputComponent;
    private ComponentLookup<MoveLimitationComponent> LimitationComponent;

    private const float IdleTime = 20f;
    private const float WanderRadius = 10f;

    public IdleSubActionState(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<MovingSpeedComponent> movingSpeedLookup, ComponentLookup<MoveControllerOutputComponent> outputLookup, ComponentLookup<MoveLimitationComponent> limitationComponent)
    {
        TransformLookup = transformLookup;
        MovingSpeedLookup = movingSpeedLookup;
        OutputComponent = outputLookup;
        LimitationComponent = limitationComponent;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        MovingSpeedLookup.Update(system);
        OutputComponent.Update(system);
        LimitationComponent.Update(system);
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
        var radius = random.NextFloat(WanderRadius / 2f, WanderRadius);
        float3 targetPosition;

        if (LimitationComponent.TryGetComponent(entity, out var limitation))
        {
            targetPosition = LocalTransformExtensions.GenerateRandomPosition(entityTransform.Position, radius, ref random);
        }
        else
        {
            targetPosition = LocalTransformExtensions.GenerateRandomPosition(limitation.Central, limitation.Scale, ref random);
        }

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

        if (OutputComponent.TryGetComponent(entity, out var moveOutput))
        {
            if (moveOutput.HasArrived)
            {
                return SubActionResult.Success();
            }
        }

        return SubActionResult.Running();
    }
}

