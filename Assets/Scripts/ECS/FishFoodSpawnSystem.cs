using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct FishFoodSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FishSpawnComponent>();
        state.RequireForUpdate<PrefabLibraryItem>();
        state.RequireForUpdate<WorldOriginItem>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var foodQuery = SystemAPI.QueryBuilder().WithAll<EdibleComponent>().Build();
        var currentFoodCount = foodQuery.CalculateEntityCount();

        var spawnJob = new FishFoodSpawnJob
        {
            DeltaTime = deltaTime,
            ECB = ecb,
            CurrentFoodCount = currentFoodCount,
        };

        state.Dependency = spawnJob.Schedule(state.Dependency);
    }

    [BurstCompile]
    partial struct FishFoodSpawnJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer ECB;
        public int CurrentFoodCount;

        public void Execute(Entity entity, ref FishFoodSpawnComponent spawnComponent)
        {
            spawnComponent.TimeElapsed += DeltaTime;

            if (CurrentFoodCount >= spawnComponent.MaxCount)
            {
                return;
            }

            float randomInterval = spawnComponent.Random.NextFloat(
                spawnComponent.SpawnInterval.x,
                spawnComponent.SpawnInterval.y);

            if (spawnComponent.TimeElapsed <= randomInterval)
            {
                return;
            }

            var randomPosition = LocalTransformExtensions.GenerateRandomPosition(spawnComponent.Position, spawnComponent.SpawnScaleRange, ref spawnComponent.Random);

            var localTransform = new LocalTransform
            {
                Position = randomPosition,
                Scale = spawnComponent.FoodScale,
                Rotation = quaternion.identity
            };

            var foodEntity = ECB.Instantiate(spawnComponent.Prefab);
            ECB.AddComponent(foodEntity, localTransform);

            spawnComponent.TimeElapsed = 0f;
        }
    }
}
