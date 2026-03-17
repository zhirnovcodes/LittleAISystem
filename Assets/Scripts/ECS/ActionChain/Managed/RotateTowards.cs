using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class RotateTowards : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MoveControllerInputComponent> MoveControllerInputLookup;
    private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;

    public RotateTowards(
        ComponentLookup<LocalTransform> transformLookup,
        ComponentLookup<MoveControllerInputComponent> moveControllerInputLookup,
        ComponentLookup<MovingSpeedComponent> movingSpeedLookup)
    {
        TransformLookup = transformLookup;
        MoveControllerInputLookup = moveControllerInputLookup;
        MovingSpeedLookup = movingSpeedLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        MoveControllerInputLookup.Update(system);
        MovingSpeedLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        MoveControllerInputLookup.Enable(entity);
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        MoveControllerInputLookup.ResetInput(entity);
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        // Check if entity does not exist in transform lookup, fail state. code = 0
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        // Check if target does not exist in transform lookup, fail state. code = 1
        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        // If time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(SubActionConsts.RotateTowards.FailTime))
        {
            return SubActionResult.Fail(2);
        }

        // if entity does not have MovingSpeedComponent - return fail with code 3
        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];
        var lookDirection = math.normalize(targetTransform.Position - entityTransform.Position);

        if (entityTransform.Rotation.IsLookingTowards(lookDirection, 0.01f))
        {
            return SubActionResult.Success();
        }

        MoveControllerInputLookup.SetTarget(entity, entityTransform.Position, 0, lookDirection, 0f, 0f, MovingSpeedLookup[entity].GetWalkingRotationSpeed());

        return SubActionResult.Running();
    }
}

