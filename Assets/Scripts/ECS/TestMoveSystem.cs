using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct TestMoveSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TestMoveComponent>();
        state.RequireForUpdate<MoveInputComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new AdvanceTargetJob().Schedule(state.Dependency);
    }

    [BurstCompile]
    public partial struct AdvanceTargetJob : IJobEntity
    {
        public void Execute(
            ref MoveInputComponent input,
            ref MoveOutputComponent output,
            ref TestMoveComponent testMove,
            in DynamicBuffer<TestMoveTargetItem> targets)
        {
            bool shouldAdvance = testMove.CurrentIndex == -1
                || (input.IsTargetReached(output));// && input.IsLookingTowards(output));

            if (!shouldAdvance)
                return;

            testMove.CurrentIndex++;

            if (testMove.CurrentIndex >= targets.Length)
            {
                input.Reset();
                output.Reset();
                testMove.CurrentIndex = -1;
                return;
            }

            var target = targets[testMove.CurrentIndex];
            input.SetTarget(target.Target, target.MaxDistance);
            input.Enable(target.Speed, target.RotationSpeed, input.Up);
            output.Reset();
        }
    }
}
