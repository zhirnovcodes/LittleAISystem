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

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        Random = new Random((uint)SystemAPI.Time.ElapsedTime + 1);
        
        state.RequireForUpdate<ActionRunnerComponent>();
        state.RequireForUpdate<ActionChainItem>();
        state.RequireForUpdate<VisibleItem>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var random = Random;

        foreach (var (chain, visionItems) in SystemAPI.Query<
            DynamicBuffer<ActionChainItem>,
            DynamicBuffer<VisibleItem>>())
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

        Random = random;
    }
}

