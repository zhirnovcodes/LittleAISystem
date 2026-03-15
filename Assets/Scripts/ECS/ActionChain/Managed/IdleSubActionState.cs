using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class IdleSubActionState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MoveControllerInputComponent> InputComponent;
    private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;
    private ComponentLookup<MoveLimitationComponent> LimitationComponent;

    private const float IdleTime = 20f;
    private const float WanderRadius = 10f;

    public IdleSubActionState(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<MoveControllerInputComponent> inputLookup, ComponentLookup<MovingSpeedComponent> movingSpeedLookup, ComponentLookup<MoveLimitationComponent> limitationComponent)
    {
        TransformLookup = transformLookup;
        InputComponent = inputLookup;
        MovingSpeedLookup = movingSpeedLookup;
        LimitationComponent = limitationComponent;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        InputComponent.Update(system);
        MovingSpeedLookup.Update(system);
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
            targetPosition = LocalTransformExtensions.GenerateRandomPosition(limitation.Central, limitation.Scale, ref random);
        }
        else
        {
            targetPosition = LocalTransformExtensions.GenerateRandomPosition(entityTransform.Position, radius, ref random);
        }

        var lookDirection = math.normalize(targetPosition - entityTransform.Position);

        // Enable and set random target
        MoveControllerExtensions.Enable(buffer, entity);
        MoveControllerExtensions.SetTarget(buffer, entity, targetPosition, 0, lookDirection, 0.01f, movingSpeed.GetWalkingSpeed(), movingSpeed.GetWalkingRotationSpeed());
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        MoveControllerExtensions.ResetInput(buffer, entity);
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (timer.IsTimeout(IdleTime))
        {
            return SubActionResult.Success();
        }

        if (TransformLookup.TryGetComponent(entity, out var entityTransform) &&
            InputComponent.TryGetComponent(entity, out var moveInput) &&
            entityTransform.IsTargetDistanceReached(moveInput.TargetPosition, moveInput.TargetScale, moveInput.Distance))
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }
}

