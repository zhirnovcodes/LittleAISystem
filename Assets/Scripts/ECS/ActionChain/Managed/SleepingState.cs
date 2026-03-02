using Unity.Entities;
using Unity.Transforms;

public class SleepingState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<SleepingPlaceComponent> SleepingPlaceLookup;
    private ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    private ComponentLookup<DNAComponent> DNALookup;
    private ComponentLookup<SleepDataComponent> SleepDataLookup;

    public SleepingState(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<SleepingPlaceComponent> sleepingPlaceLookup, ComponentLookup<AnimalStatsComponent> animalStatsLookup, ComponentLookup<DNAComponent> dnaLookup, ComponentLookup<SleepDataComponent> sleepDataLookup)
    {
        TransformLookup = transformLookup;
        SleepingPlaceLookup = sleepingPlaceLookup;
        AnimalStatsLookup = animalStatsLookup;
        DNALookup = dnaLookup;
        SleepDataLookup = sleepDataLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        SleepingPlaceLookup.Update(system);
        AnimalStatsLookup.Update(system);
        DNALookup.Update(system);
        SleepDataLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to enable for sleeping
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for sleeping
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer)
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

        // Get DNA entity first
        if (!DNALookup.HasComponent(entity))
        {
            return SubActionResult.Fail(7);
        }

        var dnaEntity = DNALookup[entity].DNA;

        // Get sleep data from DNA entity
        if (!SleepDataLookup.HasComponent(dnaEntity))
        {
            return SubActionResult.Fail(7);
        }

        var sleepData = SleepDataLookup[dnaEntity];
        float failTime = sleepData.FailTime;
        float maxDistance = sleepData.MaxDistance;

        // if time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(failTime))
        {
            return SubActionResult.Fail(2);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // if distance between transforms > MaxDistance - fail with error code 3
        if (!entityTransform.IsTargetReached(targetTransform, maxDistance))
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

        buffer.AppendToBuffer(entity, new StatsChangeItem
        {
            StatsChange = statsChange
        });

        return SubActionResult.Running();
    }
}

