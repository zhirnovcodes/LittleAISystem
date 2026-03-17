using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class WalkToSubActionState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;

    public WalkToSubActionState(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<MovingSpeedComponent> movingSpeedLookup)
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
        MoveControllerExtensions.Enable(buffer, entity);

        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return;
        }

        var movingSpeed = MovingSpeedLookup[entity];

        MoveControllerExtensions.SetTarget(buffer, entity, target, SubActionConsts.WalkTo.MaxDistance, movingSpeed.GetWalkingSpeed(), movingSpeed.GetWalkingRotationSpeed());
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Disable using extension method
        MoveControllerExtensions.ResetInput(buffer, entity);
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (timer.IsTimeout(SubActionConsts.WalkTo.FailTime))
        {
            return SubActionResult.Fail(0);
        }

        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(1);
        }

        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(2);
        }

        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(3);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];
        if (entityTransform.IsArrivedAndLooking(targetTransform, SubActionConsts.WalkTo.MaxDistance))
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }
}

