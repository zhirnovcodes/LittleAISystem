using LittleAI.Enums;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(ActionRunnerSystem))]
[BurstCompile]
public partial struct TestActionChainFillSystem : ISystem
{
    private Random Random;
    private double NextUpdateTime;
    private const float UpdateInterval = 0.5f; // Run every 0.5 seconds

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        Random = new Random((uint)SystemAPI.Time.ElapsedTime + 1);
        NextUpdateTime = 0.0;
        
        state.RequireForUpdate<ActionRunnerComponent>();
        state.RequireForUpdate<ActionChainItem>();
        state.RequireForUpdate<VisibleItem>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Check if enough time has passed since last update
        if (SystemAPI.Time.ElapsedTime < NextUpdateTime)
        {
            return;
        }

        // Update the next scheduled time
        NextUpdateTime = SystemAPI.Time.ElapsedTime + UpdateInterval;

        var job = new FillActionChainJob
        {
            RandomSeed = Random.NextUInt()
        };

        state.Dependency = job.Schedule(state.Dependency);
    }

    [BurstCompile]
    private partial struct FillActionChainJob : IJobEntity
    {
        public uint RandomSeed;

        private void Execute(
            [EntityIndexInQuery] int entityIndex,
            DynamicBuffer<ActionChainItem> chain,
            DynamicBuffer<VisibleItem> visionItems)
        {
            // Only add action if chain is empty and currently idle
            if (chain.IsEmpty == false)
            {
                return;
            }

            // Check if there are any visible items
            if (visionItems.Length <= 0)
            {
                return;
            }

            // Create unique random for this entity using seed + entity index
            var random = new Random(RandomSeed + (uint)entityIndex);

            // Pick a random item from vision
            var randomIndex = random.NextInt(0, visionItems.Length);
            var targetEntity = visionItems[randomIndex].Target;

            // Add Eat action to the chain
            chain.Add(new ActionChainItem
            {
                Action = ActionTypes.Eat,
                Target = targetEntity
            });
        }
    }
}

