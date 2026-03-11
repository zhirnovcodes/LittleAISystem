using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct BiteResolveSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        
        var transform = SystemAPI.GetComponentLookup<LocalTransform>(true);
        var change = SystemAPI.GetBufferLookup<StatsChangeItem>(true);

        new BiteResolveJob()
        {
            Ecb = ecb,
            TransformLookup = transform,
            ChangeItemLookup = change
        }.Schedule();
    }

    [BurstCompile]
    partial struct BiteResolveJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;

        [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
        [ReadOnly] public BufferLookup<StatsChangeItem> ChangeItemLookup;

        void Execute
            (
                Entity entity,
                ref DynamicBuffer<BiteItem> buffer,
                in LocalTransform transform,
                in EdibleComponent edible
            )
        {
            if (buffer.Length <= 0)
            {
                return;
            }

            if (TransformLookup.TryGetComponent(edible.EdibleBody, out var edibleTransform) == false)
            {
                return;
            }

            foreach (var bite in buffer)
            {
                if (ChangeItemLookup.HasBuffer(bite.Actor) == false)
                {
                    continue;
                }

                var scale = transform.Scale;
                var maxNutrition = scale * edible.Nutrition;
                var currentScale = edibleTransform.Scale;
                var currentNutrition = maxNutrition * currentScale;

                var biteValue = bite.BittenNutritionValue;

                var newNutrition = math.max(0, currentNutrition - biteValue);
                var bittenNutrition = newNutrition - currentNutrition;

                Ecb.AppendToBuffer(bite.Actor, new StatsChangeItem
                {
                    StatsChange = new AnimalStatsBuilder().WithFullness(bittenNutrition).Build()
                });

                if (newNutrition <= 0)
                {
                    Ecb.DestroyEntity(entity);
                    return;
                }

                var newScale = newNutrition / maxNutrition;

                Ecb.SetComponent(edible.EdibleBody, new LocalTransform
                {
                    Position = edibleTransform.Position,
                    Rotation = edibleTransform.Rotation,
                    Scale = newScale
                });
            }

            buffer.Clear();
        }
    }
}
