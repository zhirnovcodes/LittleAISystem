using Unity.Collections;
using Unity.Entities;

public partial struct TestAnimalSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TestAnimalSpawnComponent>();
        state.RequireForUpdate<VisibleComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        // Collect all entities with VisibleComponent
        var visibleEntitiesQuery = SystemAPI.QueryBuilder().WithAll<VisibleComponent>().Build();
        var visibleEntities = visibleEntitiesQuery.ToEntityArray(Allocator.Temp);

        EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.Temp);

        // Process TestAnimalSpawn component
        foreach (var spawnComponent in SystemAPI.Query<RefRO<TestAnimalSpawnComponent>>())
        {
            for (int i = 0; i < spawnComponent.ValueRO.AnimalsCount; i++)
            {
                var animalEntity = buffer.Instantiate(spawnComponent.ValueRO.Prefab);
                
                var builder = new ActionChainBuilder(animalEntity, buffer);
                var builtEntity = builder.Build();
                
                var testBuilder = new TestAnimalBuilder(builtEntity, buffer);
                testBuilder.WithVisibleItems(visibleEntities).Build();
            }
        }

        buffer.Playback(state.EntityManager);
        buffer.Dispose();

        visibleEntities.Dispose();
    }
}

