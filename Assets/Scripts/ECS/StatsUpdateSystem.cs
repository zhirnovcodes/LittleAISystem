using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

//TODO non run
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct StatsUpdateSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimalStatsComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var job = new StatsUpdateJob();
        job.Run();
    }

    [BurstCompile]
    public partial struct StatsUpdateJob : IJobEntity
    {
        public void Execute(
            ref AnimalStatsComponent statsComponent,
            ref DynamicBuffer<StatsChangeItem> statsChanges)
        {
            // Reset safety to 100 at the start of each calculation
            statsComponent.Stats.SetSafety(100f);

            // Sum all stat changes from the buffer
            var totalChange = new AnimalStats();
            
            for (int i = 0; i < statsChanges.Length; i++)
            {
                totalChange += statsChanges[i].StatsChange;
            }

            // Clamp all values between 0 and 100
            statsComponent.Stats += totalChange;

            // Clear the buffer for next frame
            statsChanges.Clear();
        }
    }
}

