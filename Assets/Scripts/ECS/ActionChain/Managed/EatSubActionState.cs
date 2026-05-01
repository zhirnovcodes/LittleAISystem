using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public class EatSubActionState : ISubActionState
{
    private ComponentLookup<MoveInputComponent> MoveInputLookup;
    private ComponentLookup<MoveOutputComponent> MoveOutputLookup;
    [ReadOnly] private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;
    [ReadOnly] private ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    [ReadOnly] private ComponentLookup<StatsIncreaseComponent> StatsIncreaseLookup;
    [ReadOnly] private BufferLookup<BiteItem> BiteLookup;

    public EatSubActionState(
        ComponentLookup<MoveInputComponent> moveInputLookup,
        ComponentLookup<MoveOutputComponent> moveOutputLookup,
        ComponentLookup<MovingSpeedComponent> movingSpeedLookup,
        BufferLookup<BiteItem> biteLookup,
        ComponentLookup<AnimalStatsComponent> animalStatsLookup,
        ComponentLookup<StatsIncreaseComponent> statsIncreaseLookup)
    {
        MoveInputLookup = moveInputLookup;
        MoveOutputLookup = moveOutputLookup;
        MovingSpeedLookup = movingSpeedLookup;
        AnimalStatsLookup = animalStatsLookup;
        StatsIncreaseLookup = statsIncreaseLookup;
        BiteLookup = biteLookup;
    }

    public void Refresh(SystemBase system)
    {
        MoveInputLookup.Update(system);
        MoveOutputLookup.Update(system);
        MovingSpeedLookup.Update(system);
        AnimalStatsLookup.Update(system);
        StatsIncreaseLookup.Update(system);
        BiteLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        if (!MovingSpeedLookup.TryGetComponent(entity, out var movingSpeed))
        {
            return;
        }

        MoveInputLookup.Enable(entity, 0f, movingSpeed.GetWalkingRotationSpeed(), math.up());
        MoveInputLookup.SetTarget(entity, target, SubActionConsts.Eat.MaxDistance * 2f);
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
            return SubActionResult.Fail(8);
        }

        if (timer.IsTimeout(SubActionConsts.Eat.FailTime))
        {
            return SubActionResult.Fail(2);
        }

        if (moveInput.IsWaiting(moveOutput))
        {
            return SubActionResult.Running();
        }

        if (!moveInput.IsTargetReached(moveOutput))
        {
            return SubActionResult.Fail(3);
        }

        if (!BiteLookup.HasBuffer(target))
        {
            return SubActionResult.Fail(4);
        }

        if (!AnimalStatsLookup.TryGetComponent(entity, out var animalStats))
        {
            return SubActionResult.Fail(7);
        }

        if (animalStats.Stats.Fullness >= 100f)
        {
            return SubActionResult.Success();
        }

        var biteValue = StatsIncreaseLookup[entity].AnimalStats.Fullness * timer.DeltaTime;

        buffer.AppendToBuffer(target, new BiteItem
        {
            Actor = entity,
            BittenNutritionValue = biteValue
        });

        return SubActionResult.Running();
    }
}
