using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(StatsUpdateSystem))]
public partial struct SafetyCheckSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SafetyCheckItem>();
        state.RequireForUpdate<VisibleItem>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var conditionFlagsLookup = SystemAPI.GetComponentLookup<ConditionFlagsComponent>(true);

        var job = new SafetyCheckJob
        {
            ConditionFlagsLookup = conditionFlagsLookup
        };

        job.ScheduleParallel();
    }

    [BurstCompile]
    public partial struct SafetyCheckJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<ConditionFlagsComponent> ConditionFlagsLookup;

        public void Execute(
            in DynamicBuffer<SafetyCheckItem> safetyChecks,
            in DynamicBuffer<VisibleItem> visibleItems,
            ref DynamicBuffer<StatsChangeItem> statsChanges)
        {
            // Process each visible item
            for (int i = 0; i < visibleItems.Length; i++)
            {
                var visibleEntity = visibleItems[i].Target;

                // Check if visible entity has condition flags
                if (!ConditionFlagsLookup.HasComponent(visibleEntity))
                    continue;

                var visibleConditions = ConditionFlagsLookup[visibleEntity].Conditions;

                // Check against all safety check items
                for (int j = 0; j < safetyChecks.Length; j++)
                {
                    var safetyCheck = safetyChecks[j];

                    // If the visible entity matches the actor conditions
                    if (visibleConditions.IsConditionMet(safetyCheck.ActorConditions))
                    {
                        // Create a stats change item with negative safety
                        var change = new AnimalStats();
                        change.SetSafety(-safetyCheck.SafetyRecession);

                        statsChanges.Add(new StatsChangeItem
                        {
                            StatsChange = change
                        });
                    }
                }
            }
        }
    }
}

