using LittleAI.Enums;
using System;
using Unity.Entities;

public struct AnimalGenomeBuilder
{
    private readonly EntityCommandBuffer CommandBuffer;
    private readonly Entity Entity;
    private bool IsAdvertiserBufferCreated;
    private bool IsStatAttenuationCreated;
    private AnimalStatsAttenuation4x4 StatAttenuation;
    private uint RandomSeed;
    
    public AnimalGenomeBuilder(EntityCommandBuffer commandBuffer, Entity entity, uint randomSeed = 1)
    {
        CommandBuffer = commandBuffer;
        Entity = entity;
        IsAdvertiserBufferCreated = false;
        IsStatAttenuationCreated = false;
        StatAttenuation = default;
        RandomSeed = randomSeed;
        
        // Add DNAGenomeItem buffer
        commandBuffer.AddBuffer<DNAChainItem>(entity);
    }
    
    public AnimalGenomeBuilder WithBaseConditionFlags(ConditionFlags flags = ConditionFlags.None)
    {
        // Add base condition flags component
        CommandBuffer.AddComponent(Entity, new ConditionFlagsComponent
        {
            Conditions = flags
        });
        return this;
    }
    
    public AnimalGenomeBuilder WithDNA(Unity.Collections.NativeList<DNAChainData> dnaList)
    {
        // Add each DNA chain data to the entity's buffer
        for (int i = 0; i < dnaList.Length; i++)
        {
            WithGenome(dnaList[i].GenomeType, dnaList[i].GenomeData);
        }
        return this;
    }
    
    public AnimalGenomeBuilder WithGenome(GenomeType type, GenomeData data)
    {
        // Add to DNA genome buffer
        CommandBuffer.AppendToBuffer(Entity, new DNAChainItem
        {
            Data = new DNAChainData
            {
                GenomeType = type,
                GenomeData = data
            }
        });
        
        switch (type)
        {
            case GenomeType.StatsIncrease:
                WithStatsIncrease(data);
                break;
            case GenomeType.Speed:
                WithSpeed(data);
                break;
            case GenomeType.Aging:
                WithAging(data);
                break;
            case GenomeType.Vision:
                WithVision(data);
                break;
            case GenomeType.NeedsBased:
                WithNeedsBased(data);
                break;
            case GenomeType.Stats:
                WithStats(data);
                break;
            case GenomeType.ActionChain:
                WithActionChain(data);
                break;
            case GenomeType.Advertiser:
                WithAdvertiser(data);
                break;
            case GenomeType.Reproduction:
                WithReproduction(data);
                break;
            case GenomeType.StatAttenuation:
                WithStatAttenuation(data);
                break;
            case GenomeType.MoveLimitation:
                WithMoveLimitation(data);
                break;
        }
        
        return this;
    }

    public Entity Build()
    {
        return Entity;
    }
    
    private void WithStatsIncrease(GenomeData data)
    {
        CommandBuffer.AddComponent(Entity, (StatsIncreaseComponent)data);
    }
    
    private void WithSpeed(GenomeData data)
    {
        CommandBuffer.AddComponent(Entity, (MovingSpeedComponent)data);
        
        // Add MoveControllerInputComponent
        CommandBuffer.AddComponent(Entity, new MoveControllerInputComponent());
    }
    
    private void WithAging(GenomeData data)
    {
        CommandBuffer.AddComponent(Entity, (AgingComponent)data);
    }
    
    private void WithVision(GenomeData data)
    {
        CommandBuffer.AddComponent(Entity, (VisionComponent)data);
        CommandBuffer.AddBuffer<VisibleItem>(Entity);
    }
    
    private void WithNeedsBased(GenomeData data)
    {
        CommandBuffer.AddBuffer<NeedBasedInputItem>(Entity);
        CommandBuffer.AddComponent<NeedBasedOutputComponent>(Entity);
        CommandBuffer.AddComponent(Entity, (NeedsActionChainComponent)data);
    }
    
    private void WithStats(GenomeData data)
    {
        CommandBuffer.AddComponent(Entity, (AnimalStatsComponent)data);
        CommandBuffer.AddBuffer<StatsChangeItem>(Entity);
    }
    
    private void WithActionChain(GenomeData data)
    {
        CommandBuffer.AddComponent<ActionRunnerComponent>(Entity);
        CommandBuffer.AddBuffer<ActionChainItem>(Entity);
        CommandBuffer.AddComponent<SubActionTimeComponent>(Entity);
        CommandBuffer.AddComponent(Entity, new ActionRandomComponent
        {
            Random = Unity.Mathematics.Random.CreateFromIndex(RandomSeed)
        });
    }
    
    private void WithAdvertiser(GenomeData data)
    {        
        if (IsAdvertiserBufferCreated == false)
        {
            CommandBuffer.AddBuffer<StatAdvertiserItem>(Entity);
            IsAdvertiserBufferCreated = true;
        }

        CommandBuffer.AppendToBuffer(Entity, (StatAdvertiserItem)data);
    }
    
    private void WithReproduction(GenomeData data)
    {
        // Add GenetaliaComponent
        GenetaliaComponent genetaliaComponent = (GenetaliaComponent)data;
        
        // Add ReproductionComponent (with GestationTime from data)
        ReproductionComponent reproductionComponent = (ReproductionComponent)data;

        CommandBuffer.AddComponent(Entity, genetaliaComponent);
        CommandBuffer.AddComponent(Entity, reproductionComponent);
        CommandBuffer.SetComponentEnabled<ReproductionComponent>(Entity, false);
        
        if (!reproductionComponent.IsMale)
        {
            CommandBuffer.AddBuffer<DNAStorageItem>(Entity);
        }
    }
    
    private void WithStatAttenuation(GenomeData data)
    {
        if (IsStatAttenuationCreated == false)
        {
            CommandBuffer.AddComponent(Entity, new AnimalStatsAttenuationComponent());
            IsStatAttenuationCreated = true;
        }

        // Convert GenomeData to AnimalStatsAttenuation
        AnimalStatsAttenuation attenuation = data;
        
        // Get the stat type from the Index
        StatType statType = (StatType)data.Index;
        
        // Set the corresponding attenuation using StatType indexer
        StatAttenuation[statType] = attenuation;
        
        // Set the component
        CommandBuffer.SetComponent(Entity, new AnimalStatsAttenuationComponent
        {
            Attenuation = StatAttenuation
        });
    }

    private void WithMoveLimitation(GenomeData data)
    {
        var moveLimitation = (MoveLimitationComponent)data;

        CommandBuffer.AddComponent(Entity, moveLimitation);
    }
}
