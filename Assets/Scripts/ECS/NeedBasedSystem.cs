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
            in ConditionFlagsComponent selfConditions,
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
                    var advertiser = statAdvertisers[j];
                    
                    // Check if conditions are met
                    if (!selfConditions.Conditions.IsConditionMet(advertiser.ActorConditions))
                        continue;

                    needBasedInputs.Add(new NeedBasedInputItem
                    {
                        Target = targetEntity,
                        StatsAdvertised = advertiser.AdvertisedValue,
                        Position = targetTransform.Position,
                        Scale = targetTransform.Scale,
                        ActionType = advertiser.ActionType
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
                    bestAction = item.ActionType;
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
            output.StatsWeight = maxWeight;
        }

        public static float CalculateWeight(
            in LocalTransform selfTransform,
            in NeedBasedInputItem item,
            in AnimalStatsComponent statsComponent,
            in AnimalStatsAttenuationComponent attenuationComponent)
        {
            // 1 - Calculate distance with scales (if distance = scale1/2 + scale2/2, distance = 0)
            float distance = selfTransform.CalculateDistance(item.Position, item.Scale);

            // 2 - Use the new CalculateWeight method
            return CalculateWeight(
                distance,
                statsComponent.Stats,
                item.StatsAdvertised,
                attenuationComponent.Attenuation.NeedsAttenuation,
                attenuationComponent.Attenuation.DistanceAttenuation
            );
        }

        public static float CalculateWeight(
                float distance, 
                AnimalStats ActorStats,
                AnimalStats StatsAdvertised,
                HermiteCurve4x2 NeedsAttenuation,
                HermiteCurve4x2 DistanceAttenuation
            )
        {
            // 1 - Create a float4x2 with the distance for all stats
            float4x2 distanceInputs =  float4x2Extensions.One * distance;

            // 2 - Get distance attenuation for all stats
            float4x2 distanceAttenuationNormalized = DistanceAttenuation.GetYs(distanceInputs);

            // 3 - Multiply advertised stats with distance attenuation to get statsAttenuated
            float4x2 statsAttenuated = StatsAdvertised.Stats * distanceAttenuationNormalized;

            // 4 - Calculate attenuated value of current stats (stats0 = NeedsAttenuation.GetYs(ActorStats.Stats / 100))
            float4x2 currentStatsNormalized = ActorStats.Stats / 100.0f;
            float4x2 stats0 = NeedsAttenuation.GetYs(currentStatsNormalized);

            // 5 - Calculate attenuated value of resulted stats (stats1 = NeedsAttenuation.GetYs((ActorStats.Stats + statsAttenuated) / 100))
            float4x2 resultedStatsNormalized = (ActorStats.Stats + statsAttenuated) / 100.0f;
            float4x2 stats1 = NeedsAttenuation.GetYs(resultedStatsNormalized);

            // 6 - Calculate stats difference attenuated (statsDifferenceAttenuated = (stats1 - stats0) * 100)
            float4x2 statsDifferenceAttenuated = (stats1 - stats0) * 100f;

            // 7 - Calculate weight (sum of all stats difference values)
            return statsDifferenceAttenuated.GetWeight();
        }
    }
}

