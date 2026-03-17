using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class TestEat : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MoveControllerInputComponent> MoveControllerInputLookup;

    public TestEat(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<MoveControllerInputComponent> moveControllerInputLookup)
    {
        TransformLookup = transformLookup;
        MoveControllerInputLookup = moveControllerInputLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        MoveControllerInputLookup.Update(system);
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
        // Check if both entities have required components
        if (!TransformLookup.HasComponent(entity) || !TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // Update target for rotation only (target position = entity position for rotation only)
        var rotation = math.normalize(targetTransform.Position - entityTransform.Position);
        MoveControllerInputLookup.SetTarget(entity,
            targetTransform.Position, targetTransform.Scale, rotation, 0.2f, SubActionConsts.TestEat.MoveSpeed, SubActionConsts.TestEat.RotationSpeed);

        // Check if eating duration is complete
        if (timer.TimeElapsed >= SubActionConsts.TestEat.EatDuration)
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }
}

