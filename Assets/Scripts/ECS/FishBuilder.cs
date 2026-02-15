using Unity.Collections;
using Unity.Entities;

public struct FishBuilder
{
    private Entity Entity;
    private EntityCommandBuffer CommandBuffer;

    public FishBuilder(Entity entity, EntityCommandBuffer commandBuffer)
    {
        Entity = entity;
        CommandBuffer = commandBuffer;
    }

    public FishBuilder WithVision(float maxDistance, float interval)
    {
        // Add vision component
        CommandBuffer.AddComponent(Entity, new VisionComponent
        {
            MaxDistance = maxDistance,
            Interval = interval,
            TimeElapsed = 0f
        });

        // Add visible items buffer
        CommandBuffer.AddBuffer<VisibleItem>(Entity);

        return this;
    }

    public FishBuilder WithConditionFlags(ConditionFlags flags)
    {
        CommandBuffer.AddComponent(Entity, new ConditionFlagsComponent
        {
            Conditions = flags
        });

        return this;
    }

    public FishBuilder WithSafetyCheck(NativeArray<SafetyCheckItem> items)
    {
        var buffer = CommandBuffer.AddBuffer<SafetyCheckItem>(Entity);

        for (int i = 0; i < items.Length; i++)
        {
            buffer.Add(items[i]);
        }

        return this;
    }

    public Entity Build()
    {
        return Entity;
    }
}

