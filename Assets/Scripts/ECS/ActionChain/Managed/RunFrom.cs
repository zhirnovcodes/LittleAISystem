using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public class RunFrom : ISubActionState
{
    private ComponentLookup<MoveInputComponent> MoveInputLookup;
    private ComponentLookup<MoveOutputComponent> MoveOutputLookup;
    [ReadOnly] private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;

    public RunFrom(
        ComponentLookup<MoveInputComponent> moveInputLookup,
        ComponentLookup<MoveOutputComponent> moveOutputLookup,
        ComponentLookup<MovingSpeedComponent> movingSpeedLookup)
    {
        MoveInputLookup = moveInputLookup;
        MoveOutputLookup = moveOutputLookup;
        MovingSpeedLookup = movingSpeedLookup;
    }

    public void Refresh(SystemBase system)
    {
        MoveInputLookup.Update(system);
        MoveOutputLookup.Update(system);
        MovingSpeedLookup.Update(system);
    }

    private void SetRandomEscapeTarget(Entity entity, float3 entityPosition, float3 targetPosition, ref Random random)
    {
        var safeDistance = new float2(1, 1.5f) * SubActionConsts.RunFrom.SafeDistance;
        var escapePosition = LocalTransformExtensions.GenerateRandomEscapePosition(entityPosition, targetPosition, safeDistance, ref random);
        MoveInputLookup.SetTarget(entity, escapePosition, 0.01f);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        if (!MoveOutputLookup.TryGetComponent(entity, out var entityOutput))
        {
            return;
        }

        if (!MoveOutputLookup.TryGetComponent(target, out var targetOutput))
        {
            return;
        }

        if (!MovingSpeedLookup.TryGetComponent(entity, out var movingSpeed))
        {
            return;
        }

        MoveInputLookup.Enable(entity, movingSpeed.GetRunningSpeed(), movingSpeed.GetRunningRotationSpeed(), math.up());
        SetRandomEscapeTarget(entity, entityOutput.Position, targetOutput.Position, ref random);
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        MoveInputLookup.Reset(entity);
        MoveOutputLookup.Reset(entity);
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (!MoveOutputLookup.TryGetComponent(entity, out var entityOutput))
        {
            return SubActionResult.Fail(0);
        }

        if (!MovingSpeedLookup.TryGetComponent(entity, out _))
        {
            return SubActionResult.Fail(2);
        }

        if (!MoveInputLookup.TryGetComponent(entity, out var moveInput))
        {
            return SubActionResult.Fail(3);
        }

        if (moveInput.IsWaiting(entityOutput))
        {
            return SubActionResult.Running();
        }

        if (math.distance(entityOutput.Position, entityOutput.TargetPosition) >= SubActionConsts.RunFrom.SafeDistance)
        {
            return SubActionResult.Success();
        }

        if (moveInput.IsTargetReached(entityOutput))
        {
            SetRandomEscapeTarget(entity, entityOutput.Position, entityOutput.TargetPosition, ref random);
        }

        return SubActionResult.Running();
    }
}
