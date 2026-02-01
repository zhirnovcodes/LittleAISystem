using Unity.Collections;
using Unity.Entities;

public struct TestAnimalBuilder
{
    private Entity Entity;
    private EntityCommandBuffer CommandBuffer;

    public TestAnimalBuilder(Entity entity, EntityCommandBuffer commandBuffer)
    {
        Entity = entity;
        CommandBuffer = commandBuffer;
    }

    public TestAnimalBuilder WithVisibleItems(NativeArray<Entity> entities)
    {
        var buffer = CommandBuffer.AddBuffer<VisibleItem>(Entity);

        for (int i = 0; i < entities.Length; i++)
        {
            buffer.Add(new VisibleItem { Target = entities[i] });
        }

        return this;
    }

    public TestAnimalBuilder WithConditionFlags(ConditionFlags flags)
    {
        CommandBuffer.AddComponent(Entity, new ConditionFlagsComponent
        {
            Conditions = flags
        });

        return this;
    }

    public TestAnimalBuilder WithSafetyCheck(NativeArray<SafetyCheckItem> items)
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

