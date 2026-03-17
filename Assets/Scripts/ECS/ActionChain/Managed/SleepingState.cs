using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class SleepingState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<SleepingPlaceComponent> SleepingPlaceLookup;
    private ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    private BufferLookup<StatsChangeItem> StatChangeLookup;

    public SleepingState(
        ComponentLookup<LocalTransform> transformLookup,
        ComponentLookup<SleepingPlaceComponent> sleepingPlaceLookup,
        ComponentLookup<AnimalStatsComponent> animalStatsLookup,
        BufferLookup<StatsChangeItem> statChangeLookup)
    {
        TransformLookup = transformLookup;
        SleepingPlaceLookup = sleepingPlaceLookup;
        AnimalStatsLookup = animalStatsLookup;
        StatChangeLookup = statChangeLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        SleepingPlaceLookup.Update(system);
        AnimalStatsLookup.Update(system);
        StatChangeLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        // Nothing to enable for sleeping
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for sleeping
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        // if actor entity does not exist in transform lookup, fail state. code = 0
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        // if target does not exist in transform lookup, fail state. code = 1
        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        // if time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(SubActionConsts.Sleeping.FailTime))
        {
            return SubActionResult.Fail(2);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // if distance between transforms > MaxDistance - fail with error code 3
        if (!entityTransform.IsTargetDistanceReached(targetTransform, SubActionConsts.Sleeping.MaxDistance))
        {
            return SubActionResult.Fail(3);
        }

        // if target does not exist in SleepingPlaceComponent lookup, fail state. code = 4
        if (!SleepingPlaceLookup.HasComponent(target))
        {
            return SubActionResult.Fail(4);
        }

        // if animal does not have AnimalStatsComponent - fail implicitly handled by lookup

        // Check if AnimalStatsComponent.Energy >= 100 - return success
        var animalStats = AnimalStatsLookup[entity];
        if (animalStats.Stats.Energy >= 100f)
        {
            return SubActionResult.Success();
        }

        // Add to buffer StatsChangeItem EnergyReplanish * DeltaTime
        var sleepingPlace = SleepingPlaceLookup[target];
        var energyGain = sleepingPlace.EnergyReplanish * timer.DeltaTime;

        var statsChange = new AnimalStatsBuilder().WithEnergy(energyGain).Build();

        if (StatChangeLookup.TryGetBuffer(entity, out var changeBuffer))
        {
            changeBuffer.Add(new StatsChangeItem
            {
                StatsChange = statsChange
            });
        }

        return SubActionResult.Running();
    }
}

