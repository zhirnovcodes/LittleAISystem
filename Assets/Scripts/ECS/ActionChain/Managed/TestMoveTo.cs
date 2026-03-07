using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class TestMoveTo : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MoveControllerOutputComponent> MoveControllerOutputLookup;
    private Random RandomGenerator;

    private const float MoveSpeed = 5.0f;
    private const float RotationSpeed = 180.0f;

    public TestMoveTo(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<MoveControllerOutputComponent> moveControllerOutputLookup)
    {
        TransformLookup = transformLookup;
        MoveControllerOutputLookup = moveControllerOutputLookup;
        RandomGenerator = Random.CreateFromIndex((uint)System.DateTime.Now.Ticks);
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        MoveControllerOutputLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        // Enable MoveController
        MoveControllerExtensions.Enable(buffer, entity);
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Disable using extension method
        MoveControllerExtensions.Disable(buffer, entity);
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        // Check if both entities have required components
        if (!TransformLookup.HasComponent(entity) || !TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        // if entity does not have MoveControllerOutputComponent - return fail with code 2
        if (!MoveControllerOutputLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(2);
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
            targetTransform.Position, targetTransform.Scale, rotation, 0.2f, MoveSpeed, RotationSpeed);

        return SubActionResult.Running();
    }
}

