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

                if (!TransformLookup.TryGetComponent(targetEntity, out var targetTransform))
                    continue;

                if (!StatAdvertiserLookup.TryGetBuffer(targetEntity, out var statAdvertisers))
                    continue;

                for (int j = 0; j < statAdvertisers.Length; j++)
                {
                    var advertiser = statAdvertisers[j];
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
                output.Target = entity;
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

                float weight = CalculateWeight(selfTransform, item, statsComponent.Stats, attenuationComponent.Attenuation);

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
            in AnimalStats actorStats,
            in AnimalStatsAttenuation4x4 attenuation)
        {
            // 1 - Calculate distance with scales (if distance = scale1/2 + scale2/2, distance = 0)
            float distance = selfTransform.CalculateDistance(item.Position, item.Scale);

            // 2 - Calculate attenuated stats change using the extension method
            AnimalStats attenuatedChange = attenuation.GetStatsAttenuated(actorStats, item.StatsAdvertised, distance);

            // 3 - Calculate weight (sum of all attenuated stats change values)
            return attenuatedChange.GetWeight();
        }
    }
}

