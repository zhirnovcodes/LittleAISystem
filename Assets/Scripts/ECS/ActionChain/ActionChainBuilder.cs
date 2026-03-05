using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;

public struct ActionChainBuilder
{
    private Entity Entity;
    private EntityCommandBuffer CommandBuffer;

    public ActionChainBuilder(Entity entity, EntityCommandBuffer commandBuffer, uint randomSeed = 1)
    {
        Entity = entity;
        CommandBuffer = commandBuffer;

        // Add ActionRunnerComponent with idle state
        CommandBuffer.AddComponent(entity, new ActionRunnerComponent
        {
            Target = entity,
            Action = ActionTypes.None,
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
        
        // Add ActionRandomComponent
        CommandBuffer.AddComponent(entity, new ActionRandomComponent
        {
            Random = Random.CreateFromIndex(randomSeed)
        });
    }

    public Entity Build()
    {
        return Entity;
    }
}
