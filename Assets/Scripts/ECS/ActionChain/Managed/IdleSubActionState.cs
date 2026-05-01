using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public class IdleSubActionState : ISubActionState
{
    private ComponentLookup<MoveOutputComponent> MoveOutputLookup;
    private ComponentLookup<MoveInputComponent> MoveInputLookup;
    [ReadOnly] private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;
    [ReadOnly] private ComponentLookup<MoveLimitationComponent> LimitationComponent;

    public IdleSubActionState(
        ComponentLookup<MoveOutputComponent> moveOutputLookup,
        ComponentLookup<MoveInputComponent> moveInputLookup,
        ComponentLookup<MovingSpeedComponent> movingSpeedLookup,
        ComponentLookup<MoveLimitationComponent> limitationComponent)
    {
        MoveOutputLookup = moveOutputLookup;
        MoveInputLookup = moveInputLookup;
        MovingSpeedLookup = movingSpeedLookup;
        LimitationComponent = limitationComponent;
    }

    public void Refresh(SystemBase system)
    {
        MoveOutputLookup.Update(system);
        MoveInputLookup.Update(system);
        MovingSpeedLookup.Update(system);
        LimitationComponent.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        if (!MoveOutputLookup.TryGetComponent(entity, out var moveOutput))
        {
            return;
        }

        if (!MovingSpeedLookup.TryGetComponent(entity, out var movingSpeed))
        {
            return;
        }

        var radius = random.NextFloat(SubActionConsts.Idle.WanderRadius / 2f, SubActionConsts.Idle.WanderRadius);
        float3 targetPosition;

        if (LimitationComponent.TryGetComponent(entity, out var limitation))
        {
            targetPosition = LocalTransformExtensions.GenerateRandomPosition(limitation.Central, limitation.Scale, ref random);
        }
        else
        {
            targetPosition = LocalTransformExtensions.GenerateRandomPosition(moveOutput.Position, radius, ref random);
        }

        var speed = movingSpeed.GetWalkingSpeed();
        var rotationSpeed = movingSpeed.GetWalkingRotationSpeed();

        MoveInputLookup.Enable(entity, speed, rotationSpeed, math.up());
        MoveInputLookup.SetTarget(entity, targetPosition, SubActionConsts.Idle.MoveDelta);
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        MoveInputLookup.Reset(entity);
        MoveOutputLookup.Reset(entity);
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        var time = random.NextFloat(SubActionConsts.Idle.IdleTime / 2f, SubActionConsts.Idle.IdleTime);
        if (timer.IsTimeout(time))
        {
            return SubActionResult.Success();
        }

        if (!MoveInputLookup.TryGetComponent(entity, out var moveInput))
        {
            return SubActionResult.Running();
        }

        if (!MoveOutputLookup.TryGetComponent(entity, out var moveOutput))
        {
            return SubActionResult.Running();
        }

        if (moveInput.IsWaiting(moveOutput))
        {
            return SubActionResult.Running();
        }

        if (moveInput.IsTargetReached(moveOutput))
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }
}
