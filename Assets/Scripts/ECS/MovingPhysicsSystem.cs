using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using LittlePhysics;

[BurstCompile]
[UpdateInGroup(typeof(LittlePhysicsUserSystemGroup))]
public partial struct MovingPhysicsSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsSingleton>();
        state.RequireForUpdate<MoveControllerInputComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var singleton = SystemAPI.GetSingleton<PhysicsSingleton>();
        if (!singleton.PhysicsVelocities.IsCreated || !singleton.BodiesList.IsCreated)
            return;

        var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
        var combinedDep = JobHandle.CombineDependencies(state.Dependency, singleton.PhysicsJobHandle);

        state.Dependency = new MovingPhysicsJob
        {
            PhysicsVelocities = singleton.PhysicsVelocities,
            TransformLookup = transformLookup,
        }.Schedule(combinedDep);

        singleton.PhysicsJobHandle = state.Dependency;
        SystemAPI.SetSingleton(singleton);

        var inputLookup = SystemAPI.GetComponentLookup<MoveControllerInputComponent>(true);
        var transformLookup2 = SystemAPI.GetComponentLookup<LocalTransform>(true);

        state.Dependency = new RotationJob
        {
            InputLookup = inputLookup,
            TransformLookup = transformLookup2,
            DeltaTime = SystemAPI.Time.DeltaTime,
        }.Schedule(state.Dependency);
    }

    [BurstCompile]
    public partial struct MovingPhysicsJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public NativeArray<PhysicsVelocityData> PhysicsVelocities;
        [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;

        public void Execute(
            in LocalTransform transform,
            in PhysicsBodyUpdateComponent update,
            in MoveControllerInputComponent input)
        {
            if (input.Speed <= 0)
                return;

            int index = update.Index;

            var targetPosition = input.TargetPosition;
            var targetScale = input.TargetScale;

            if (input.TargetEntity != Entity.Null)
            {
                if (TransformLookup.TryGetComponent(input.TargetEntity, out var targetTransform) == false)
                    return;

                targetScale = targetTransform.Scale;
                targetPosition = targetTransform.Position;
            }

            if (transform.IsTargetDistanceReached(targetPosition, targetScale, input.Distance))
                return;

            var velocity = PhysicsVelocities[index];
            velocity.Linear += GetLinearVelocity(transform, velocity, targetPosition, targetScale, input.Distance, input.Speed);
            PhysicsVelocities[index] = velocity;
        }

        private float3 GetLinearVelocity(
            in LocalTransform transform,
            in PhysicsVelocityData velocity,
            float3 targetPosition,
            float targetScale,
            float distance,
            float speed)
        {
            if (transform.IsTargetDistanceReached(targetPosition, targetScale, distance))
                return float3.zero;

            float3 toTarget = math.normalize(targetPosition - transform.Position);
            float3 desiredVelocity = toTarget * speed;
            float3 velocityToAdd = desiredVelocity - velocity.Linear;

            float currentSpeedAlongDirection = math.dot(velocity.Linear, toTarget);
            if (currentSpeedAlongDirection >= speed)
                return float3.zero;

            float3 newVelocity = velocity.Linear + velocityToAdd;
            if (math.length(newVelocity) > speed)
            {
                newVelocity = math.normalize(newVelocity) * speed;
                velocityToAdd = newVelocity - velocity.Linear;
            }

            return velocityToAdd;
        }
    }

    [BurstCompile]
    public partial struct RotationJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<MoveControllerInputComponent> InputLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
        public float DeltaTime;

        public void Execute(
            ref LocalTransform transform,
            in RotationHandlerComponent rotHandler)
        {
            if (!InputLookup.TryGetComponent(rotHandler.Parent, out var input))
                return;

            if (input.RotationSpeed <= 0)
                return;

            if (!TransformLookup.TryGetComponent(rotHandler.Parent, out var parentTransform))
                return;

            var lookDirection = input.LookDirection;

            if (input.TargetEntity != Entity.Null)
            {
                if (!TransformLookup.TryGetComponent(input.TargetEntity, out var targetTransform))
                    return;

                lookDirection = math.normalize(targetTransform.Position - parentTransform.Position);
            }

            if (transform.Rotation.IsLookingTowards(lookDirection, 0.01f))
                return;

            transform = transform.RotateTowards(lookDirection, input.RotationSpeed * DeltaTime, 0.01f);
        }
    }
}
