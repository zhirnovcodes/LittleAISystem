using LittleAI.Enums;
using Unity.Entities;

public struct ActionChainBuilder
{
    private Entity Entity;
    private EntityCommandBuffer CommandBuffer;

    public ActionChainBuilder(Entity entity, EntityCommandBuffer commandBuffer)
    {
        Entity = entity;
        CommandBuffer = commandBuffer;

        // Add ActionRunnerComponent with idle state
        CommandBuffer.AddComponent(entity, new ActionRunnerComponent
        {
            Target = entity,
            Action = ActionTypes.Idle,
            CurrentSubActionIndex = 0
        });

        // Add ActionChainItem buffer
        CommandBuffer.AddBuffer<ActionChainItem>(entity);

        // Add SubActionTimeComponent
        CommandBuffer.AddComponent(entity, new SubActionTimeComponent
        {
            DeltaTime = 0f,
            TimeElapsed = 0f
        });
    }

    public Entity Build()
    {
        return Entity;
    }
}
