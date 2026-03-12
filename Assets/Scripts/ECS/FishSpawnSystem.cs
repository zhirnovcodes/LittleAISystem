using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct FishSpawnSystem : ISystem
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

        // Get singletons
        var prefabLibraryEntity = SystemAPI.GetSingletonEntity<PrefabLibraryItem>();
        var prefabLibrary = SystemAPI.GetBuffer<PrefabLibraryItem>(prefabLibraryEntity);
        
        var spawnJob = new FishSpawnJob
        {
            DeltaTime = deltaTime,
            ECB = ecb,
            PrefabLibrary = prefabLibrary,
            ParentDNALookup = SystemAPI.GetBufferLookup<DNAChainItem>(true),
            ParentFlagsLookup = SystemAPI.GetComponentLookup<ConditionFlagsComponent>(true)
        };

        state.Dependency = spawnJob.Schedule(state.Dependency);

    }

    [BurstCompile]
    partial struct FishSpawnJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer ECB;
        [ReadOnly] public DynamicBuffer<PrefabLibraryItem> PrefabLibrary;
        [ReadOnly] public BufferLookup<DNAChainItem> ParentDNALookup;
        [ReadOnly] public ComponentLookup<ConditionFlagsComponent> ParentFlagsLookup;

        public void Execute(Entity entity, ref FishSpawnComponent spawnComponent, in DynamicBuffer<WorldOriginItem> originDNA)
        {
            spawnComponent.TimeElapsed += DeltaTime;

            // Check if it's time to spawn
            float randomInterval = spawnComponent.Random.NextFloat(
                spawnComponent.SpawnInterval.x, 
                spawnComponent.SpawnInterval.y);

            if (spawnComponent.TimeElapsed >= randomInterval)
            {
                spawnComponent.TimeElapsed = 0f;

                // Check if we have at least 2 parents
                if (originDNA.Length < 2)
                    return;

                for (int i = 0; i < spawnComponent.OneTimeSpawn; i++)
                {
                    // Select 2 random different parents
                    int fatherIndex = spawnComponent.Random.NextInt(0, originDNA.Length);
                    int motherIndex;
                    do
                    {
                        motherIndex = spawnComponent.Random.NextInt(0, originDNA.Length);
                    } while (motherIndex == fatherIndex);

                    var fatherEntity = originDNA[fatherIndex].Parent;
                    var motherEntity = originDNA[motherIndex].Parent;

                    // Check if both parents have DNA
                    if (!ParentDNALookup.HasBuffer(fatherEntity) || !ParentDNALookup.HasBuffer(motherEntity))
                        return;

                    var fatherDNA = ParentDNALookup[fatherEntity];
                    var motherDNA = ParentDNALookup[motherEntity];

                    // Check compatibility
                    if (!DNAExtensions.IsCompatible(fatherDNA, motherDNA))
                        return;

                    // Get flags from one of the parents (assuming they should be compatible)
                    ConditionFlags flags = ConditionFlags.None;
                    if (ParentFlagsLookup.HasComponent(motherEntity))
                    {
                        flags = ParentFlagsLookup[motherEntity].Conditions;
                    }
                    else if (ParentFlagsLookup.HasComponent(fatherEntity))
                    {
                        flags = ParentFlagsLookup[fatherEntity].Conditions;
                    }

                    // Get prefab from library
                    var prefab = PrefabLibrary.GetPrefab(flags);
                    if (prefab == Entity.Null)
                        return;

                    // Call BornEntity
                    var offspring = DNAExtensions.BornEntity(
                        flags,
                        fatherDNA,
                        motherDNA,
                        prefab,
                        ref spawnComponent.Random,
                        ECB);

                    var position = LocalTransformExtensions.GenerateRandomPosition(spawnComponent.SpawnPosition, spawnComponent.SpawnScale,
                        ref spawnComponent.Random);

                    ECB.SetComponent(offspring, new LocalTransform
                    {
                        Position = position,
                        Scale = 1,
                        Rotation = quaternion.identity
                    }) ;

                    spawnComponent.Count++;


                    if (spawnComponent.Count >= spawnComponent.MaxCount)
                    {
                        ECB.SetComponentEnabled<FishSpawnComponent>(entity, false);
                        return;
                    }
                }
            }
        }
    }
}
