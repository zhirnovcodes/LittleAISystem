using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public class CommunicateSubActionState : ISubActionState
{
    private ComponentLookup<MoveInputComponent> MoveInputLookup;
    private ComponentLookup<MoveOutputComponent> MoveOutputLookup;
    [ReadOnly] private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;
    [ReadOnly] private ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    [ReadOnly] private ComponentLookup<StatsIncreaseComponent> StatsIncreaseLookup;
    private BufferLookup<StatsChangeItem> StatChangeLookup;
    [ReadOnly] private ComponentLookup<GenetaliaComponent> GenetaliaLookup;
    [ReadOnly] private ComponentLookup<ReproductionComponent> ReproductionLookup;
    [ReadOnly] private BufferLookup<DNAChainItem> DNAChainLookup;
    [ReadOnly] private BufferLookup<DNAStorageItem> DNAStorageLookup;

    public CommunicateSubActionState(
        ComponentLookup<MoveInputComponent> moveInputLookup,
        ComponentLookup<MoveOutputComponent> moveOutputLookup,
        ComponentLookup<MovingSpeedComponent> movingSpeedLookup,
        ComponentLookup<AnimalStatsComponent> animalStatsLookup,
        ComponentLookup<GenetaliaComponent> genetaliaLookup,
        ComponentLookup<StatsIncreaseComponent> statsIncreaseLookup,
        BufferLookup<StatsChangeItem> statChangeLookup,
        BufferLookup<DNAChainItem> dnaChainLookup,
        BufferLookup<DNAStorageItem> dnaStorageLookup,
        ComponentLookup<ReproductionComponent> reproductionLookup)
    {
        MoveInputLookup = moveInputLookup;
        MoveOutputLookup = moveOutputLookup;
        MovingSpeedLookup = movingSpeedLookup;
        AnimalStatsLookup = animalStatsLookup;
        StatsIncreaseLookup = statsIncreaseLookup;
        StatChangeLookup = statChangeLookup;
        GenetaliaLookup = genetaliaLookup;
        DNAChainLookup = dnaChainLookup;
        DNAStorageLookup = dnaStorageLookup;
        ReproductionLookup = reproductionLookup;
    }

    public void Refresh(SystemBase system)
    {
        MoveInputLookup.Update(system);
        MoveOutputLookup.Update(system);
        MovingSpeedLookup.Update(system);
        AnimalStatsLookup.Update(system);
        StatsIncreaseLookup.Update(system);
        StatChangeLookup.Update(system);
        GenetaliaLookup.Update(system);
        DNAChainLookup.Update(system);
        DNAStorageLookup.Update(system);
        ReproductionLookup.Update(system);
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
        MoveInputLookup.SetTarget(entity, target, SubActionConsts.Communicate.MaxDistance);
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
            return SubActionResult.Fail(4);
        }

        if (!moveInput.IsTargetReached(moveOutput))
        {
            return SubActionResult.Fail(2);
        }

        if (!StatsIncreaseLookup.TryGetComponent(entity, out var statsIncrease))
        {
            return SubActionResult.Fail(3);
        }

        var socialGain = statsIncrease.AnimalStats.Social * timer.DeltaTime;
        var statsChange = new AnimalStatsBuilder().WithSocial(socialGain).Build();

        if (StatChangeLookup.TryGetBuffer(entity, out var changeBuffer))
        {
            changeBuffer.Add(new StatsChangeItem
            {
                StatsChange = statsChange
            });
        }

        if (!AnimalStatsLookup.TryGetComponent(entity, out var animalStats))
        {
            return SubActionResult.Running();
        }

        if (!GenetaliaLookup.TryGetComponent(entity, out var entityGenitalia))
        {
            return SubActionResult.Running();
        }

        if (animalStats.Stats.Social >= 100f)
        {
            if (entityGenitalia.IsMale)
            {
                AddDNAToTarget(entity, target, buffer, ref random);
            }

            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }

    private void AddDNAToTarget(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        if (!DNAChainLookup.TryGetBuffer(entity, out var fatherDNA))
        {
            return;
        }

        if (!DNAStorageLookup.HasBuffer(target))
        {
            return;
        }

        if (!DNAChainLookup.TryGetBuffer(target, out var motherDNA))
        {
            return;
        }

        if (!DNAExtensions.IsCompatible(fatherDNA, motherDNA))
        {
            return;
        }

        for (int i = 0; i < fatherDNA.Length; i++)
        {
            buffer.AppendToBuffer(target, new DNAStorageItem
            {
                Father = entity,
                Data = fatherDNA[i].Data
            });
        }

        if (ReproductionLookup.TryGetComponent(target, out var reproduction))
        {
            reproduction.Random = Random.CreateFromIndex(random.NextUInt());
            buffer.SetComponent(target, reproduction);
        }

        buffer.SetComponentEnabled<ReproductionComponent>(target, true);
    }
}
