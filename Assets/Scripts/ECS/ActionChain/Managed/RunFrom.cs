using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class RunFrom : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<MovingDataComponent> MovingDataLookup;
    private ComponentLookup<SafetyDistanceComponent> SafetyDistanceLookup;

    public RunFrom(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<MovingDataComponent> movingDataLookup, ComponentLookup<SafetyDistanceComponent> safetyDistanceLookup)
    {
        TransformLookup = transformLookup;
        MovingDataLookup = movingDataLookup;
        SafetyDistanceLookup = safetyDistanceLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        MovingDataLookup.Update(system);
        SafetyDistanceLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to enable for run from
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for run from
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer)
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

        // Get moving data from entity
        if (!MovingDataLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(7);
        }

        // Get safety distance from entity
        if (!SafetyDistanceLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(8);
        }

        var movingData = MovingDataLookup[entity];
        var safetyData = SafetyDistanceLookup[entity];

        float moveSpeed = movingData.MaxSpeed;
        float safeDistance = safetyData.SafeDistance;
        float rotationSpeed = movingData.MaxRotationSpeed;

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // If distance >= SafeDistance - success
        if (entityTransform.IsDistanceGreaterThan(targetTransform, safeDistance))
        {
            return SubActionResult.Success();
        }

        // Move in direction opposite from target
        var newTransform = entityTransform.MovePositionAwayFrom(targetTransform, timer.DeltaTime * moveSpeed);

        // Rotate away from target
        var directionAwayFromTarget = entityTransform.Position - targetTransform.Position;
        newTransform = newTransform.RotateTowards(directionAwayFromTarget, rotationSpeed * timer.DeltaTime, 0.01f);

        buffer.SetComponent(entity, newTransform);

        return SubActionResult.Running();
    }
}

