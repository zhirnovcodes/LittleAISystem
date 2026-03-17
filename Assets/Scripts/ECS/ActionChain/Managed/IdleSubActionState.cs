using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class IdleSubActionState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MoveControllerInputComponent> InputComponent;
    private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;
    private ComponentLookup<MoveLimitationComponent> LimitationComponent;

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
        if (!TransformLookup.HasComponent(entity) || !MovingSpeedLookup.HasComponent(entity))
        {
            return;
        }

        var entityTransform = TransformLookup[entity];
        var movingSpeed = MovingSpeedLookup[entity];

        var radius = random.NextFloat(SubActionConsts.Idle.WanderRadius / 2f, SubActionConsts.Idle.WanderRadius);
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
        var speed = movingSpeed.GetWalkingSpeed() * SubActionConsts.Idle.SpeedMultiplier;
        var rotationSpeed = movingSpeed.GetWalkingRotationSpeed() * SubActionConsts.Idle.SpeedMultiplier;

        MoveControllerExtensions.Enable(buffer, entity);
        MoveControllerExtensions.SetTarget(buffer, entity, targetPosition, 0, lookDirection, 0.01f, speed, rotationSpeed);
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        MoveControllerExtensions.ResetInput(buffer, entity);
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        var time = random.NextFloat(SubActionConsts.Idle.IdleTime / 2f, SubActionConsts.Idle.IdleTime);
        if (timer.IsTimeout(time))
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

