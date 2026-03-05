using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class TestEat : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;

    private const float EatDuration = 3.0f;
    private const float RotationSpeed = 180.0f; // Degrees per second

    public TestEat(ComponentLookup<LocalTransform> transformLookup)
    {
        TransformLookup = transformLookup;
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
        MoveControllerExtensions.Disable(buffer, entity);
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
        MoveControllerExtensions.SetTarget(buffer, entity, entityTransform.Position,
            targetTransform.Position, entityTransform.Scale, targetTransform.Scale, 0f, RotationSpeed);

        // Check if eating duration is complete
        if (timer.TimeElapsed >= EatDuration)
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }
}

