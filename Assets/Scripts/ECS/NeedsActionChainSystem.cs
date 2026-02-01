using LittleAI.Enums;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(NeedBasedSystem))]
[UpdateBefore(typeof(ActionRunnerSystem))]
public partial struct NeedsActionChainSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NeedsActionChainComponent>();
        state.RequireForUpdate<NeedBasedOutputComponent>();
        state.RequireForUpdate<ActionRunnerComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var job = new NeedsActionChainManipulationJob
        {
            TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            StatAdvertiserLookup = SystemAPI.GetBufferLookup<StatAdvertiserItem>(true)
        };
        
        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    public partial struct NeedsActionChainManipulationJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
        [ReadOnly] public BufferLookup<StatAdvertiserItem> StatAdvertiserLookup;

        private void Execute(
            ref ActionRunnerComponent runner,
            ref DynamicBuffer<ActionChainItem> actionChain,
            in NeedsActionChainComponent needsConfig,
            in NeedBasedOutputComponent needBasedOutput,
            in LocalTransform selfTransform,
            in AnimalStatsComponent statsComponent,
            in AnimalStatsAttenuationComponent attenuationComponent)
        {
            // Check if the need-based action is already the current action
            if (runner.Action == needBasedOutput.Action && 
                runner.Target == needBasedOutput.Target)
            {
                return;
            }

            // Check if the need-based action is already in the action chain
            bool isAlreadyInChain = false;

            for (int i = 0; i < actionChain.Length; i++)
            {
                if (actionChain[i].Action == needBasedOutput.Action && 
                    actionChain[i].Target == needBasedOutput.Target)
                {
                    isAlreadyInChain = true;
                    break;
                }
            }

            if (isAlreadyInChain)
            {
                return;
            }

            // Calculate current action's weight (if not Idle)
            float currentWeight = 0f;
            if (runner.Target != Entity.Null)
            {
                currentWeight = CalculateCurrentActionWeight(
                    runner.Target,
                    selfTransform,
                    statsComponent,
                    attenuationComponent,
                    runner.Action);
            }

            // Check if we should cancel current action and prioritize need-based action
            if (needBasedOutput.StatsWeight >= needsConfig.CancelThreshold + currentWeight)
            {
                // Add to front of action chain
                actionChain.Insert(0, new ActionChainItem
                {
                    Target = needBasedOutput.Target,
                    Action = needBasedOutput.Action
                });

                // Request cancellation of current action
                runner.IsCancellationRequested = true;
            }
            // Check if we should add to action chain
            else if (needBasedOutput.StatsWeight >= needsConfig.AddThreshold)
            {
                // Add to end of action chain
                actionChain.Add(new ActionChainItem
                {
                    Target = needBasedOutput.Target,
                    Action = needBasedOutput.Action
                });
            }
        }

        private float CalculateCurrentActionWeight(
            Entity targetEntity,
            in LocalTransform selfTransform,
            in AnimalStatsComponent statsComponent,
            in AnimalStatsAttenuationComponent attenuationComponent,
            ActionTypes currentAction)
        {
            // Check if target has required components
            if (!TransformLookup.TryGetComponent(targetEntity, out var targetTransform))
                return 0f;

            if (!StatAdvertiserLookup.TryGetBuffer(targetEntity, out var statAdvertisers))
                return 0f;

            // Calculate distance with scales
            float distance = selfTransform.CalculateDistance(targetTransform);

            // Find the best advertiser weight for the current target and action
            for (int i = 0; i < statAdvertisers.Length; i++)
            {
                var advertiser = statAdvertisers[i];
                
                // Only calculate weight if the advertiser's action matches the current action
                if (advertiser.ActionType != currentAction)
                    continue;
                
                float weight = NeedBasedSystem.NeedBasedCalculationJob.CalculateWeight(
                    distance,
                    statsComponent.Stats,
                    advertiser.AdvertisedValue,
                    attenuationComponent.Attenuation.NeedsAttenuation,
                    attenuationComponent.Attenuation.DistanceAttenuation
                );

                return weight;
            }

            return 0;
        }
    }
}

