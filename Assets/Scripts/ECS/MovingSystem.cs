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
        }.Schedule();
    }
}

[BurstCompile]
public partial struct MovingJob : IJobEntity
{
    public float DeltaTime;

    public void Execute(
        ref LocalTransform transform,
        ref MoveControllerOutputComponent output,
        in MoveControllerInputComponent input)
    {
        var currentTransform = transform;
        
        // Check if position target is reached
        bool hasArrived = currentTransform.IsTargetDistanceReached(input.TargetPosition, input.TargetScale, input.Distance);
        
        // Check if looking at target direction
        bool isLookingAt = currentTransform.Rotation.IsLookingTowards(input.LookDirection, 0.01f);
        
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
            newTransform = newTransform.MovePositionTowards(input.TargetPosition, input.TargetScale, input.Distance, input.Speed, DeltaTime);
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

