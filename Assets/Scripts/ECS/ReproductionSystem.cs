using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct ReproductionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ReproductionComponent>();
        state.RequireForUpdate<PrefabLibraryItem>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        
        // Get the prefab library singleton entity
        var prefabLibraryEntity = SystemAPI.GetSingletonEntity<PrefabLibraryItem>();
        var prefabLibrary = SystemAPI.GetBuffer<PrefabLibraryItem>(prefabLibraryEntity);

        var laborJob = new LaborJob
        {
            DeltaTime = deltaTime,
            ECB = ecb,
            PrefabLibrary = prefabLibrary
        };

        laborJob.Run();

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    [WithAll(typeof(ReproductionComponent))]
    partial struct LaborJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer ECB;
        [ReadOnly] public DynamicBuffer<PrefabLibraryItem> PrefabLibrary;

        public void Execute(
            Entity entity,
            ref ReproductionComponent reproduction,
            in ConditionFlagsComponent flags,
            in DynamicBuffer<DNAChainItem> motherDNA,
            ref DynamicBuffer<DNAStorageItem> dnaStorage)
        {
            // Only process females with enabled reproduction component
            if (reproduction.IsMale)
                return;

            reproduction.TimeElapsed += DeltaTime;

            if (reproduction.TimeElapsed >= reproduction.GestationTime)
            {
                Labor(entity, ref reproduction, in flags, in motherDNA, ref dnaStorage);
            }
        }

        private void Labor(
            Entity mother,
            ref ReproductionComponent reproduction,
            in ConditionFlagsComponent motherFlags,
            in DynamicBuffer<DNAChainItem> motherDNA,
            ref DynamicBuffer<DNAStorageItem> dnaStorage)
        {
            // 1. Take prefab from prefab library singleton by actor's flags
            var prefab = PrefabLibrary.GetPrefab(motherFlags.Conditions);
            
            if (prefab == Entity.Null)
            {
                // No prefab found, reset and return
                reproduction.TimeElapsed = 0f;
                ECB.SetComponentEnabled<ReproductionComponent>(mother, false);
                return;
            }

            // 2. Get random father from DNAStorage
            var random = reproduction.Random;
            var father = DNAExtensions.GetRandomFather(dnaStorage, ref random);

            if (father == Entity.Null)
            {
                // No father DNA, reset and return
                reproduction.TimeElapsed = 0f;
                ECB.SetComponentEnabled<ReproductionComponent>(mother, false);
                return;
            }

            // 3. Get father's DNA
            var fatherDNAList = new NativeList<DNAChainData>(Allocator.Temp);
            DNAExtensions.GetFatherDNA(dnaStorage, father, ref fatherDNAList);
            
            // Convert mother DNA buffer to list
            var motherDNAList = new NativeList<DNAChainData>(Allocator.Temp);
            DNAExtensions.ToList(motherDNA, ref motherDNAList);
            
            // 4. Call BornEntity to create offspring
            var offspring = DNAExtensions.BornEntity(
                motherFlags.Conditions,
                fatherDNAList,
                motherDNAList,
                prefab,
                ref random,
                ECB);
            
            // Clean up
            fatherDNAList.Dispose();
            motherDNAList.Dispose();

            // 5. Clear storage
            dnaStorage.Clear();

            // Reset reproduction
            reproduction.Random = random;
            reproduction.TimeElapsed = 0f;
            ECB.SetComponentEnabled<ReproductionComponent>(mother, false);
        }
    }
}
