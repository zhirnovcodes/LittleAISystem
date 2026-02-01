using Unity.Entities;

public struct NeedBasedAIBuilder
{
    private Entity Entity;
    private EntityCommandBuffer CommandBuffer;

    public NeedBasedAIBuilder(Entity entity, EntityCommandBuffer commandBuffer)
    {
        Entity = entity;
        CommandBuffer = commandBuffer;

        // Add NeedBasedInputItem buffer
        CommandBuffer.AddBuffer<NeedBasedInputItem>(entity);

        // Add NeedBasedOutputComponent
        CommandBuffer.AddComponent(entity, new NeedBasedOutputComponent
        {
            Target = Entity.Null,
            Action = LittleAI.Enums.ActionTypes.Idle,
            StatsWeight = 0f
        });
    }

    public NeedBasedAIBuilder WithActionChainManipulation(float cancelThreshold, float addThreshold)
    {
        // Add NeedsActionChainComponent
        CommandBuffer.AddComponent(Entity, new NeedsActionChainComponent
        {
            CancelThreshold = cancelThreshold,
            AddThreshold = addThreshold
        });

        // Enable the component by default
        CommandBuffer.SetComponentEnabled<NeedsActionChainComponent>(Entity, true);

        return this;
    }

    public Entity Build()
    {
        return Entity;
    }
}

