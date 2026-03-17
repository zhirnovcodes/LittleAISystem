using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class EatSubActionState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    private ComponentLookup<StatsIncreaseComponent> StatsIncreaseLookup;
    private BufferLookup<BiteItem> BiteLookup;

    public EatSubActionState(ComponentLookup<LocalTransform> transformLookup, BufferLookup<BiteItem> biteLookup, ComponentLookup<AnimalStatsComponent> animalStatsLookup, ComponentLookup<StatsIncreaseComponent> statsIncreaseLookup)
    {
        TransformLookup = transformLookup;
        AnimalStatsLookup = animalStatsLookup;
        StatsIncreaseLookup = statsIncreaseLookup;
        BiteLookup = biteLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        AnimalStatsLookup.Update(system);
        StatsIncreaseLookup.Update(system);
        BiteLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        // Nothing to enable for eat
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for eat
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        // if actor entity does not exist in transform lookup, fail state. code = 0
        if (!TransformLookup.TryGetComponent(entity, out var entityTransform))
        {
            return SubActionResult.Fail(0);
        }

        // if target does not exist in transform lookup, fail state. code = 1
        if (!TransformLookup.TryGetComponent(target, out var targetTransform))
        {
            return SubActionResult.Fail(1);
        }

        // if time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(SubActionConsts.Eat.FailTime))
        {
            return SubActionResult.Fail(2);
        }

        // if distance between transforms > MaxDistance - fail with error code 3
        if (entityTransform.IsTargetDistanceReached(targetTransform, SubActionConsts.Eat.MaxDistance) == false)
        {
            return SubActionResult.Fail(3);
        }

        // if target does not exist in EdibleBody lookup, fail state. code = 4
        if (BiteLookup.HasBuffer(target) == false)
        {
            return SubActionResult.Fail(4);
        }

        // if animal does not have AnimalStatsComponent - return fail with code 7
        if (!AnimalStatsLookup.TryGetComponent(entity, out var animalStats))
        {
            return SubActionResult.Fail(7);
        }

        // Check if Fullness >= 100 - returns Success
        if (animalStats.Stats.Fullness >= 100f)
        {
            return SubActionResult.Success();
        }

        // Process eating continuously based on EatingSpeed
        var biteValue = StatsIncreaseLookup[entity].AnimalStats.Fullness * timer.DeltaTime;

        buffer.AppendToBuffer(target, new BiteItem
        {
            Actor = entity,
            BittenNutritionValue = biteValue
        });

        return SubActionResult.Running();
    }
}
