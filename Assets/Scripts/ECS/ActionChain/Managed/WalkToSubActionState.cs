using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class WalkToSubActionState : ISubActionState
{
    private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;
    private ComponentLookup<MoveControllerOutputComponent> MoveControllerOutputLookup;

    private const float MaxDistance = 0.2f;
    private const float FailTime = 30f;

    public WalkToSubActionState( ComponentLookup<MovingSpeedComponent> movingSpeedLookup, ComponentLookup<MoveControllerOutputComponent> moveControllerOutputLookup)
    {
        MovingSpeedLookup = movingSpeedLookup;
        MoveControllerOutputLookup = moveControllerOutputLookup;
    }

    public void Refresh(SystemBase system)
    {
        MovingSpeedLookup.Update(system);
        MoveControllerOutputLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        // Enable MoveController
        MoveControllerExtensions.Enable(buffer, entity);

        // Update target position
        var movingSpeed = MovingSpeedLookup[entity];

        MoveControllerExtensions.SetTarget(buffer, entity, target, MaxDistance, movingSpeed.GetWalkingSpeed(), movingSpeed.GetWalkingRotationSpeed());
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Disable using extension method
        MoveControllerExtensions.Disable(buffer, entity);
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (timer.IsTimeout(FailTime))
        {
            return SubActionResult.Fail(0);
        }

        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(1);
        }

        if (!MoveControllerOutputLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(2);
        }

        var moveOutput = MoveControllerOutputLookup[entity];

        if (moveOutput.IsFailed)
        {
            return SubActionResult.Fail(3);
        }

        if (moveOutput.HasArrived && moveOutput.IsLookingAt)
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }
}

