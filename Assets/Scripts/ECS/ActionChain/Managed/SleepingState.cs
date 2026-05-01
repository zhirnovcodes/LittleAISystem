using Unity.Entities;
using Unity.Mathematics;

public class SleepingState : ISubActionState
{
    private ComponentLookup<MoveInputComponent> MoveInputLookup;
    private ComponentLookup<MoveOutputComponent> MoveOutputLookup;
    private ComponentLookup<SleepingPlaceComponent> SleepingPlaceLookup;
    private ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    private BufferLookup<StatsChangeItem> StatChangeLookup;

    public SleepingState(
        ComponentLookup<MoveInputComponent> moveInputLookup,
        ComponentLookup<MoveOutputComponent> moveOutputLookup,
        ComponentLookup<SleepingPlaceComponent> sleepingPlaceLookup,
        ComponentLookup<AnimalStatsComponent> animalStatsLookup,
        BufferLookup<StatsChangeItem> statChangeLookup)
    {
        MoveInputLookup = moveInputLookup;
        MoveOutputLookup = moveOutputLookup;
        SleepingPlaceLookup = sleepingPlaceLookup;
        AnimalStatsLookup = animalStatsLookup;
        StatChangeLookup = statChangeLookup;
    }

    public void Refresh(SystemBase system)
    {
        MoveInputLookup.Update(system);
        MoveOutputLookup.Update(system);
        SleepingPlaceLookup.Update(system);
        AnimalStatsLookup.Update(system);
        StatChangeLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        MoveInputLookup.Enable(entity, 0f, 0f, math.up());
        MoveInputLookup.SetTarget(entity, target, SubActionConsts.Sleeping.MaxDistance);
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        MoveInputLookup.Reset(entity);
        MoveOutputLookup.Reset(entity);
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (!MoveInputLookup.TryGetComponent(entity, out var moveInput))
        {
            return SubActionResult.Fail(0);
        }

        if (!MoveOutputLookup.TryGetComponent(entity, out var moveOutput))
        {
            return SubActionResult.Fail(1);
        }

        if (moveOutput.IsTargetDisposed)
        {
            return SubActionResult.Fail(5);
        }

        if (timer.IsTimeout(SubActionConsts.Sleeping.FailTime))
        {
            return SubActionResult.Fail(2);
        }

        if (!moveInput.IsTargetReached(moveOutput))
        {
            return SubActionResult.Fail(3);
        }

        if (!SleepingPlaceLookup.TryGetComponent(target, out var sleepingPlace))
        {
            return SubActionResult.Fail(4);
        }

        if (!AnimalStatsLookup.TryGetComponent(entity, out var animalStats))
        {
            return SubActionResult.Running();
        }

        if (animalStats.Stats.Energy >= 100f)
        {
            return SubActionResult.Success();
        }

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
