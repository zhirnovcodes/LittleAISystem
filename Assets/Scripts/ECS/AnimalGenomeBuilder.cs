using Unity.Entities;

public struct AnimalGenomeBuilder
{
    private readonly EntityCommandBuffer CommandBuffer;
    private readonly Entity Entity;
    private bool IsAdvertiserBufferCreated;
    
    public AnimalGenomeBuilder(EntityCommandBuffer commandBuffer, Entity entity)
    {
        CommandBuffer = commandBuffer;
        Entity = entity;
        IsAdvertiserBufferCreated = false;
    }
    
    public AnimalGenomeBuilder WithBaseConditionFlags()
    {
        // Add base condition flags component
        CommandBuffer.AddComponent<ConditionFlagsComponent>(Entity);
        return this;
    }
    
    public AnimalGenomeBuilder WithGenome(GenomeType type, IGenomeDataConvertible genomeDataConvertible)
    {
        GenomeData data = genomeDataConvertible.GetGenomeData();
        
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
}

