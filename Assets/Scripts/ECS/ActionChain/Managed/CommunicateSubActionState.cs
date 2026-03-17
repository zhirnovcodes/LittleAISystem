using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class CommunicateSubActionState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    private ComponentLookup<StatsIncreaseComponent> StatsIncreaseLookup;
    private BufferLookup<StatsChangeItem> StatChangeLookup;
    private ComponentLookup<GenetaliaComponent> GenetaliaLookup;
    private ComponentLookup<ReproductionComponent> ReproductionLookup;
    private BufferLookup<DNAChainItem> DNAChainLookup;
    private BufferLookup<DNAStorageItem> DNAStorageLookup;

    public CommunicateSubActionState(
        ComponentLookup<LocalTransform> transformLookup,
        ComponentLookup<AnimalStatsComponent> animalStatsLookup,
        ComponentLookup<GenetaliaComponent> genetaliaLookup,
        ComponentLookup<StatsIncreaseComponent> statsIncreaseLookup,
        BufferLookup<StatsChangeItem> statChangeLookup,
        BufferLookup<DNAChainItem> dnaChainLookup,
        BufferLookup<DNAStorageItem> dnaStorageLookup,
        ComponentLookup<ReproductionComponent> reproductionLookup)
    {
        TransformLookup = transformLookup;
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
        TransformLookup.Update(system);
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
        // Enable genitalia component
        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];
            genitalia.IsEnabled = true;
            buffer.SetComponent(entity, genitalia);
        }
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Disable genitalia component
        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];
            genitalia.IsEnabled = false;
            buffer.SetComponent(entity, genitalia);
        }
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

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // Check if target is reached
        if (entityTransform.IsTargetDistanceReached(targetTransform, SubActionConsts.Communicate.MaxDistance) == false)
        {
            return SubActionResult.Fail(2);
        }

        // if entity does not have StatsIncreaseComponent - return fail with code 3
        if (!StatsIncreaseLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        // Add stat Social with increase speed from component * delta time
        var statsIncrease = StatsIncreaseLookup[entity];
        var socialGain = statsIncrease.AnimalStats.Social * timer.DeltaTime;

        var statsChange = new AnimalStatsBuilder().WithSocial(socialGain).Build();

        if (StatChangeLookup.TryGetBuffer(entity, out var changeBuffer))
        {
            changeBuffer.Add(new StatsChangeItem
            {
                StatsChange = statsChange
            });
        }

        // Check if entity has stats component
        if (!AnimalStatsLookup.HasComponent(entity))
        {
            return SubActionResult.Running();
        }

        var animalStats = AnimalStatsLookup[entity];

        // Check if entity has genitalia and Social >= 100
        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];
            
            if (animalStats.Stats.Social >= 100f)
            {
                // If male, add DNA to target (which will also enable reproduction on target)
                if (genitalia.IsMale)
                {
                    AddDNAToTarget(entity, target, buffer, ref random);
                }
                
                return SubActionResult.Success();
            }
        }

        return SubActionResult.Running();
    }

    private void AddDNAToTarget(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        // Check if entity has DNA chain buffer
        if (!DNAChainLookup.HasBuffer(entity))
        {
            return;
        }
        
        // Check if target has DNA storage buffer (only females have this)
        if (!DNAStorageLookup.HasBuffer(target))
        {
            return;
        }
        
        // Get mother's DNA chain
        if (!DNAChainLookup.HasBuffer(target))
        {
            return;
        }
        
        // Get father's DNA chain
        var fatherDNA = DNAChainLookup[entity];
        
        var motherDNA = DNAChainLookup[target];
        
        // Check if DNA chains are compatible
        if (!DNAExtensions.IsCompatible(fatherDNA, motherDNA))
        {
            return;
        }
        
        // Append father's DNA to target's DNA storage
        for (int i = 0; i < fatherDNA.Length; i++)
        {
            buffer.AppendToBuffer(target, new DNAStorageItem
            {
                Father = entity,
                Data = fatherDNA[i].Data
            });
        }
        
        // Set Random seed on ReproductionComponent from the ref Random parameter
        if (ReproductionLookup.HasComponent(target))
        {
            var reproduction = ReproductionLookup[target];
            reproduction.Random = Random.CreateFromIndex(random.NextUInt());
            buffer.SetComponent(target, reproduction);
        }
        
        // Enable ReproductionComponent on target (female) to start gestation
        buffer.SetComponentEnabled<ReproductionComponent>(target, true);
    }
}

