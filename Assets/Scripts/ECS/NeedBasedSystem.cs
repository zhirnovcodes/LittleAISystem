using LittleAI.Enums;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct NeedBasedSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<VisibleComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // First job: Fill NeedBasedInputItem buffer from VisibleItem buffer
        var collectionJob = new NeedItemsCollectionJob
        {
            TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            StatAdvertiserLookup = SystemAPI.GetBufferLookup<StatAdvertiserItem>(true)
        };
        state.Dependency = collectionJob.ScheduleParallel(state.Dependency);

        // Second job: Calculate need-based decisions
        var calculationJob = new NeedBasedCalculationJob();
        state.Dependency = calculationJob.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    public partial struct NeedItemsCollectionJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
        [ReadOnly] public BufferLookup<StatAdvertiserItem> StatAdvertiserLookup;

        private void Execute(
            in DynamicBuffer<VisibleItem> visibleItems,
            ref DynamicBuffer<NeedBasedInputItem> needBasedInputs)
        {
            needBasedInputs.Clear();

            for (int i = 0; i < visibleItems.Length; i++)
            {
                var targetEntity = visibleItems[i].Target;

                // Check if target has required components
                if (!TransformLookup.TryGetComponent(targetEntity, out var targetTransform))
                    continue;

                if (!StatAdvertiserLookup.TryGetBuffer(targetEntity, out var statAdvertisers))
                    continue;

                // For each advertiser, create a NeedBasedInputItem
                for (int j = 0; j < statAdvertisers.Length; j++)
                {
                    needBasedInputs.Add(new NeedBasedInputItem
                    {
                        Target = targetEntity,
                        StatsAdvertised = statAdvertisers[j].AdvertisedValue,
                        Position = targetTransform.Position,
                        Scale = targetTransform.Scale
                    });
                }
            }
        }
    }

    [BurstCompile]
    public partial struct NeedBasedCalculationJob : IJobEntity
    {
        public void Execute(
            Entity entity,
            in LocalTransform selfTransform,
            in DynamicBuffer<NeedBasedInputItem> needBasedInputs,
            in AnimalStatsComponent statsComponent,
            in AnimalStatsAttenuationComponent attenuationComponent,
            ref NeedBasedOutputComponent output)
        {
            if (needBasedInputs.Length == 0)
            {
                output.Target = Entity.Null;
                output.Action = ActionTypes.Idle;
                output.StatsWeight = 0;
                return;
            }

            Entity bestTarget = Entity.Null;
            ActionTypes bestAction = ActionTypes.Idle;
            float maxWeight = float.MinValue;

            for (int i = 0; i < needBasedInputs.Length; i++)
            {
                var item = needBasedInputs[i];

                float weight = CalculateWeight(selfTransform, item, statsComponent, attenuationComponent);

                // Check if this is the best option (advertiser with max weight wins)
                if (weight > maxWeight)
                {
                    maxWeight = weight;
                    bestTarget = item.Target;
                    // TODO: Determine action type based on the advertised stats
                    bestAction = ActionTypes.Idle; // Placeholder
                }
            }

            // If weight is not positive, set action to Idle
            if (maxWeight <= 0)
            {   
                bestTarget = entity;
                maxWeight = 0;
                bestAction = ActionTypes.Idle;
            }

            // Set output
            output.Target = bestTarget;
            output.Action = bestAction;
            output.StatsWeight = (int)maxWeight;
        }

        public static float CalculateWeight(
            in LocalTransform selfTransform,
            in NeedBasedInputItem item,
            in AnimalStatsComponent statsComponent,
            in AnimalStatsAttenuationComponent attenuationComponent)
        {
            // 1 - Calculate distance with scales (if distance = scale1/2 + scale2/2, distance = 0)
            float3 deltaPos = item.Position - selfTransform.Position;
            float rawDistance = math.length(deltaPos);
            float distance = math.max(0, rawDistance - (selfTransform.Scale / 2.0f + item.Scale / 2.0f));

            // 2 - Create distance multiplier (inverse lerp for each stat: invlerp(0, maxDistance[x,y], distance))
            float4x2 distanceMultiplier = float4x2Extensions.InverseLerp(
                float4x2Extensions.Zero, 
                attenuationComponent.Attenuation.MaxDistance, 
                float4x2Extensions.One * distance);

            // 3 - Get distance attenuation for all stats
            float4x2 distanceAttenuationNormalized = attenuationComponent.Attenuation.DistanceAttenuation.GetYs(distanceMultiplier);

            // 5 - Multiply advertised stats with distance attenuation to get statsAttenuated
            float4x2 statsAttenuated = item.StatsAdvertised.Stats * distanceAttenuationNormalized;

            // 6 - Calculate attenuated value of current stats (stats0 = NeedsAttenuation.GetYs(StatsComponent.stats / 100))
            float4x2 currentStatsNormalized = statsComponent.Stats.Stats / 100.0f;
            float4x2 stats0 = attenuationComponent.Attenuation.NeedsAttenuation.GetYs(currentStatsNormalized);

            // 7 - Calculate attenuated value of resulted stats (stats1 = NeedsAttenuation.GetYs((StatsComponent.stats + statsAttenuated) / 100))
            float4x2 resultedStatsNormalized = (statsComponent.Stats.Stats + statsAttenuated) / 100.0f;
            float4x2 stats1 = attenuationComponent.Attenuation.NeedsAttenuation.GetYs(resultedStatsNormalized);

            // 8 - Calculate stats difference attenuated (statsDifferenceAttenuated = stats1 - stats0)
            float4x2 statsDifferenceAttenuated = stats1 - stats0;

            // 9 - Calculate weight (sum of all stats difference values)
            return statsDifferenceAttenuated.GetWeight();
        }
    }
}

