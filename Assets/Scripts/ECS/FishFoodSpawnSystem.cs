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

        var spawnJob = new FishFoodSpawnJob
        {
            DeltaTime = deltaTime,
            ECB = ecb,
        };

        state.Dependency = spawnJob.Schedule(state.Dependency);

    }

    [BurstCompile]
    partial struct FishFoodSpawnJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer ECB;

        public void Execute(Entity entity, ref FishFoodSpawnComponent spawnComponent)
        {
            spawnComponent.TimeElapsed += DeltaTime;

            // Check if it's time to spawn
            float randomInterval = spawnComponent.Random.NextFloat(
                spawnComponent.SpawnInterval.x,
                spawnComponent.SpawnInterval.y);

            if (spawnComponent.TimeElapsed <= randomInterval)
            {
                return;
            }

            var randomPosition = LocalTransformExtensions.GenerateRandomPosition(spawnComponent.Position, spawnComponent.Scale, ref spawnComponent.Random);

            var localTransform = new LocalTransform
            {
                Position = randomPosition,
                Scale = 1,
                Rotation = quaternion.identity
            };

            var foodEntity = ECB.Instantiate(spawnComponent.Prefab);
            ECB.AddComponent(foodEntity, localTransform);


            // Reset timer
            spawnComponent.TimeElapsed = 0f;

        }
    }
}
