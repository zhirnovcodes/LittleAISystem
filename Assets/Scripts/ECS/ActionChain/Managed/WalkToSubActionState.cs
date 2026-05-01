using Unity.Entities;
using Unity.Mathematics;

public class WalkToSubActionState : ISubActionState
{
    private ComponentLookup<MoveInputComponent> MoveInputLookup;
    private ComponentLookup<MoveOutputComponent> MoveOutputLookup;
    private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;

    public WalkToSubActionState(
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

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        if (!MovingSpeedLookup.TryGetComponent(entity, out var movingSpeed))
        {
            return;
        }

        MoveInputLookup.Enable(entity, movingSpeed.GetWalkingSpeed(), movingSpeed.GetWalkingRotationSpeed(), math.up());
        MoveInputLookup.SetTarget(entity, target, SubActionConsts.WalkTo.MaxDistance);
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        MoveInputLookup.Reset(entity);
        MoveOutputLookup.Reset(entity);
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (timer.IsTimeout(SubActionConsts.WalkTo.FailTime))
        {
            return SubActionResult.Fail(0);
        }

        if (!MoveInputLookup.TryGetComponent(entity, out var moveInput))
        {
            return SubActionResult.Fail(1);
        }

        if (!MoveOutputLookup.TryGetComponent(entity, out var moveOutput))
        {
            return SubActionResult.Fail(2);
        }

        if (moveOutput.IsTargetDisposed)
        {
            return SubActionResult.Fail(3);
        }

        if (moveInput.IsTargetReached(moveOutput) && moveInput.IsLookingTowards(moveOutput))
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }
}
