using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class TestMoveTo : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private Random RandomGenerator;

    public TestMoveTo(ComponentLookup<LocalTransform> transformLookup)
    {
        TransformLookup = transformLookup;
        RandomGenerator = Random.CreateFromIndex((uint)System.DateTime.Now.Ticks);
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        // Enable MoveController
        MoveControllerExtensions.Enable(buffer, entity);
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Disable using extension method
        MoveControllerExtensions.ResetInput(buffer, entity);
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
        MoveControllerExtensions.SetTarget(buffer, entity, 
            targetTransform.Position, targetTransform.Scale, rotation, 0.2f, SubActionConsts.TestMoveTo.MoveSpeed, SubActionConsts.TestMoveTo.RotationSpeed);

        return SubActionResult.Running();
    }
}

