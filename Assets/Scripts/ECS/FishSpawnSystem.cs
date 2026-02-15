using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial struct FishSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FishSpawnComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        // Get singleton component
        var spawnComponent = SystemAPI.GetSingleton<FishSpawnComponent>();

        EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.Temp);

        var random = Unity.Mathematics.Random.CreateFromIndex(spawnComponent.RandomSeed);

        for (int i = 0; i < spawnComponent.Count; i++)
        {
            var fishEntity = buffer.Instantiate(spawnComponent.Prefab);
            
            // Add action chain components
            var actionBuilder = new ActionChainBuilder(fishEntity, buffer);
            var builtEntity = actionBuilder.Build();
            
            // Add need-based AI components
            var needBuilder = new NeedBasedAIBuilder(builtEntity, buffer);
            builtEntity = needBuilder.Build();
            
            // Add vision and other fish-specific components
            var fishBuilder = new FishBuilder(builtEntity, buffer);
            
            // Randomize vision parameters within the specified ranges
            float visionRange = random.NextFloat(spawnComponent.VisionRange.x, spawnComponent.VisionRange.y);
            float visionInterval = random.NextFloat(spawnComponent.VisionInterval.x, spawnComponent.VisionInterval.y);
            
            fishBuilder.WithVision(visionRange, visionInterval).Build();
        }

        buffer.Playback(state.EntityManager);
        buffer.Dispose();
    }
}

