using Unity.Entities;
using Unity.Mathematics;

public class StumbleUponSubActionState : ISubActionState
{
    private ComponentLookup<MoveInputComponent> MoveInputLookup;
    private ComponentLookup<MoveOutputComponent> MoveOutputLookup;
    private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;
    private ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    private ComponentLookup<GenetaliaComponent> GenetaliaLookup;

    public StumbleUponSubActionState(
        ComponentLookup<MoveInputComponent> moveInputLookup,
        ComponentLookup<MoveOutputComponent> moveOutputLookup,
        ComponentLookup<MovingSpeedComponent> movingSpeedLookup,
        ComponentLookup<AnimalStatsComponent> animalStatsLookup,
        ComponentLookup<GenetaliaComponent> genetaliaLookup)
    {
        MoveInputLookup = moveInputLookup;
        MoveOutputLookup = moveOutputLookup;
        MovingSpeedLookup = movingSpeedLookup;
        AnimalStatsLookup = animalStatsLookup;
        GenetaliaLookup = genetaliaLookup;
    }

    public void Refresh(SystemBase system)
    {
        MoveInputLookup.Update(system);
        MoveOutputLookup.Update(system);
        MovingSpeedLookup.Update(system);
        AnimalStatsLookup.Update(system);
        GenetaliaLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        if (GenetaliaLookup.TryGetComponent(entity, out var genitalia))
        {
            genitalia.IsEnabled = true;
            buffer.SetComponent(entity, genitalia);
        }

        if (!MovingSpeedLookup.TryGetComponent(entity, out var movingSpeed))
        {
            return;
        }

        MoveInputLookup.Enable(entity, 0f, movingSpeed.GetWalkingRotationSpeed(), math.up());
        MoveInputLookup.SetTarget(entity, target, SubActionConsts.StumbleUpon.MaxDistance);
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        if (GenetaliaLookup.TryGetComponent(entity, out var genitalia))
        {
            genitalia.IsEnabled = false;
            buffer.SetComponent(entity, genitalia);
        }

        MoveInputLookup.Reset(entity);
        MoveOutputLookup.Reset(entity);
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (!MoveOutputLookup.TryGetComponent(entity, out _))
        {
            return SubActionResult.Fail(0);
        }

        if (!MoveOutputLookup.TryGetComponent(target, out _))
        {
            return SubActionResult.Fail(1);
        }

        if (timer.IsTimeout(SubActionConsts.StumbleUpon.FailTime))
        {
            return SubActionResult.Fail(2);
        }

        if (!GenetaliaLookup.TryGetComponent(entity, out var genitalia))
        {
            return SubActionResult.Fail(3);
        }

        if (AnimalStatsLookup.TryGetComponent(entity, out var animalStats))
        {
            if (animalStats.Stats.Social >= 100f)
            {
                return SubActionResult.Fail(4);
            }
        }

        if (!GenetaliaLookup.TryGetComponent(target, out var targetGenitalia))
        {
            return SubActionResult.Fail(5);
        }

        if (genitalia.IsMale != targetGenitalia.IsMale)
        {
            if (targetGenitalia.IsEnabled)
            {
                return SubActionResult.Success();
            }

            return SubActionResult.Running();
        }

        return SubActionResult.Fail(6);
    }
}
