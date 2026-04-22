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
        state.RequireForUpdate<LittlePhysicsTimeComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var singleton = SystemAPI.GetSingleton<PhysicsSingleton>();
        if (!singleton.PhysicsVelocities.IsCreated || !singleton.BodiesList.IsCreated)
            return;

        var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
        var combinedDep = JobHandle.CombineDependencies(state.Dependency, singleton.PhysicsJobHandle);

        var time = SystemAPI.GetSingleton<LittlePhysicsTimeComponent>();

        var friction = new AirFrictionJob
        {
            PhysicsVelocities = singleton.PhysicsVelocities,
            DeltaTime = time.DeltaTime,
        }.Schedule(combinedDep);

        var move = new MovingPhysicsJob
        {
            PhysicsVelocities = singleton.PhysicsVelocities,
            TransformLookup = transformLookup,
        }.Schedule(friction);

        var inputLookup = SystemAPI.GetComponentLookup<MoveControllerInputComponent>(true);
        var worldTransformLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);

        state.Dependency = new RotationJob
        {
            InputLookup = inputLookup,
            WorldTransformLookup = worldTransformLookup,
            DeltaTime = time.DeltaTime,
        }.Schedule(move);

        singleton.PhysicsJobHandle = state.Dependency;
        SystemAPI.SetSingleton(singleton);
    }

    [BurstCompile]
    public partial struct AirFrictionJob : IJobEntity
    {
        private const float AirFriction = 100f;

        [NativeDisableParallelForRestriction] public NativeArray<PhysicsVelocityData> PhysicsVelocities;
        public float DeltaTime;

        public void Execute(in PhysicsBodyUpdateComponent update)
        {

            if (update.IsEnabled == false)
            {
                return;
            }
            
            int index = update.Index;
            var velocity = PhysicsVelocities[index];

            float speed = math.length(velocity.Linear);
            float reducedSpeed = math.max(0f, speed - speed * AirFriction * DeltaTime);
            velocity.Linear = math.normalizesafe(velocity.Linear) * reducedSpeed;

            PhysicsVelocities[index] = velocity;
        }
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
            if (update.IsEnabled == false)
            {
                return;
            }

            if (input.Speed <= 0)
            {
                return;
            }

            var targetPosition = input.TargetPosition;
            var targetScale = input.TargetScale;

            if (input.TargetEntity != Entity.Null)
            {
                if (TransformLookup.TryGetComponent(input.TargetEntity, out var targetTransform) == false)
                {
                    return;
                }

                targetScale = targetTransform.Scale;
                targetPosition = targetTransform.Position;
            }

            if (transform.IsTargetDistanceReached(targetPosition, targetScale, input.Distance))
            {
                return;
            }

            int index = update.Index;
            var velocity = PhysicsVelocities[index];
            velocity.Linear += GetLinearVelocity(transform, velocity, targetPosition, targetScale, input.Distance, input.Speed);
            PhysicsVelocities[index] = velocity;
        }

        private static float3 GetLinearVelocity(
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
        [ReadOnly] public ComponentLookup<LocalToWorld> WorldTransformLookup;
        public float DeltaTime;

        public void Execute(ref LocalTransform transform, in RotationHandlerComponent rotHandler)
        {
            if (!InputLookup.TryGetComponent(rotHandler.Parent, out var input))
            {
                return;
            }

            if (input.RotationSpeed <= 0)
            {
                return;
            }

            if (!WorldTransformLookup.TryGetComponent(rotHandler.Parent, out var parentWorld))
            {
                return;
            }

            var lookDirection = input.LookDirection;

            if (input.TargetEntity != Entity.Null)
            {
                if (!WorldTransformLookup.TryGetComponent(input.TargetEntity, out var targetWorld))
                {
                    return;
                }

                lookDirection = math.normalize(targetWorld.Position - parentWorld.Position);
            }

            if (transform.Rotation.IsLookingTowards(lookDirection))
            {   
                return;
            }
            
            transform = transform.RotateTowards(lookDirection, input.RotationSpeed * DeltaTime);
        }
    }
}
