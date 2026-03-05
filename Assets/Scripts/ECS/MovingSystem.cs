using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct MovingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MoveControllerInputComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new MovingJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct MovingJob : IJobEntity
{
    public float DeltaTime;

    public void Execute(
        ref LocalTransform transform,
        ref MoveControllerOutputComponent output,
        in MoveControllerInputComponent input,
        EnabledRefRO<MoveControllerInputComponent> enabled)
    {
        // Only process if enabled
        if (!enabled.ValueRO)
        {
            return;
        }

        var currentTransform = transform;
        
        // Check if position target is reached
        bool hasArrived = currentTransform.IsTargetReached(input.TargetPosition, input.TargetScale, 0.001f);
        
        // Check if looking at target direction
        bool isLookingAt = currentTransform.IsLookingTowards(input.LookDirection, 0.01f);
        
        // Update output component
        output.HasArrived = hasArrived;
        output.IsLookingAt = isLookingAt;
        
        // If both conditions are met, we're done
        if (hasArrived && isLookingAt)
        {
            return;
        }
        
        var newTransform = currentTransform;
        
        // Move towards target position if not arrived
        if (!hasArrived)
        {
            newTransform = newTransform.MovePositionTowards(input.TargetPosition, input.TargetScale, DeltaTime, input.Speed);
        }
        
        // Rotate towards look direction if not looking at it
        if (!isLookingAt)
        {
            newTransform = newTransform.RotateTowards(input.LookDirection, input.RotationSpeed * DeltaTime, 0.01f);
        }
        
        // Update transform
        transform = newTransform;
    }
}

