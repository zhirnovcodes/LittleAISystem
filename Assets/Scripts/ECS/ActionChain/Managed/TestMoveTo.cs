using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class TestMoveTo : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MoveControllerInputComponent> MoveControllerInputLookup;
    private Random RandomGenerator;

    public TestMoveTo(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<MoveControllerInputComponent> moveControllerInputLookup)
    {
        TransformLookup = transformLookup;
        MoveControllerInputLookup = moveControllerInputLookup;
        RandomGenerator = Random.CreateFromIndex((uint)System.DateTime.Now.Ticks);
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

        // Check if reached target
        if (entityTransform.IsTargetDistanceReached(targetTransform, 0.001f))
        {
            return SubActionResult.Success();
        }

        // Update target position
        var rotation = math.normalize(targetTransform.Position - entityTransform.Position);
        MoveControllerInputLookup.SetTarget(entity,
            targetTransform.Position, targetTransform.Scale, rotation, 0.2f, SubActionConsts.TestMoveTo.MoveSpeed, SubActionConsts.TestMoveTo.RotationSpeed);

        return SubActionResult.Running();
    }
}

