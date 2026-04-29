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

        var targetTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        state.Dependency = new RotationJob
        {
            TargetTransformLookup = targetTransformLookup,
            PhysicsVelocities = singleton.PhysicsVelocities,
            DeltaTime = time.DeltaTime,
        }.Schedule(move);

        singleton.PhysicsJobHandle = state.Dependency;
        SystemAPI.SetSingleton(singleton);
    }

    [BurstCompile]
    public partial struct AirFrictionJob : IJobEntity
    {
        private const float AirFriction = 1f;

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

            float linSpeed = math.length(velocity.Linear);
            float reducedLinSpeed = math.max(0f, linSpeed - linSpeed * AirFriction * DeltaTime);
            velocity.Linear = math.normalizesafe(velocity.Linear) * reducedLinSpeed;

            float angSpeed = math.length(velocity.Angular);
            float reducedAngSpeed = math.max(0f, angSpeed - angSpeed * AirFriction * DeltaTime);
            velocity.Angular = math.normalizesafe(velocity.Angular) * reducedAngSpeed;

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
        [ReadOnly] public ComponentLookup<LocalTransform> TargetTransformLookup;
        [NativeDisableParallelForRestriction] public NativeArray<PhysicsVelocityData> PhysicsVelocities;
        public float DeltaTime;

        public void Execute(
            in LocalTransform transform,
            in PhysicsBodyUpdateComponent update,
            in MoveControllerInputComponent input)
        {
            if (input.RotationSpeed <= 0 || update.IsEnabled == false)
                return;

            var lookDirection = input.LookDirection;

            if (input.TargetEntity != Entity.Null)
            {
                if (!TargetTransformLookup.TryGetComponent(input.TargetEntity, out var targetTransform))
                    return;

                lookDirection = math.normalize(targetTransform.Position - transform.Position);
            }

            float3 forward = math.forward(transform.Rotation);
            float dot = math.clamp(math.dot(forward, lookDirection), -1f, 1f);

            if (dot > 0.9999f)
                return;

            float3 cross = math.cross(forward, lookDirection);
            float crossLen = math.length(cross);

            if (crossLen < 0.0001f)
            {
                cross = math.cross(forward, math.mul(transform.Rotation, math.up()));
                crossLen = math.length(cross);
            }

            float3 axis = cross / crossLen;
            float angleRad = math.acos(dot);
            float rotSpeedRad = math.radians(input.RotationSpeed);

            // max angular speed that won't overshoot the remaining angle this step
            float maxSpeed = angleRad / DeltaTime;

            var velocity = PhysicsVelocities[update.Index];
            float existingSpeed = math.dot(velocity.Angular, axis);

            bool isCruising = existingSpeed >= rotSpeedRad * 0.9999f;

            if (isCruising)
            {
                // already at target speed - only correct if we'd overshoot
                if (existingSpeed > maxSpeed)
                {
                    velocity.Angular += axis * (maxSpeed - existingSpeed);
                    PhysicsVelocities[update.Index] = velocity;
                }
            }
            else
            {
                // accelerate, then clamp to not overshoot
                float newSpeed = math.min(existingSpeed + rotSpeedRad, maxSpeed);
                float speedToAdd = math.max(0f, newSpeed - existingSpeed);
                if (speedToAdd > 0f)
                {
                    velocity.Angular += axis * speedToAdd;
                    PhysicsVelocities[update.Index] = velocity;
                }
            }
        }
    }
}
