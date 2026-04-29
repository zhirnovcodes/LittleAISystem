using LittlePhysics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;


[BurstCompile]
[UpdateInGroup(typeof(LittlePhysicsUserSystemGroup))]
public partial struct MoveLinearSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsSingleton>();
        state.RequireForUpdate<MoveInputComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var singleton = SystemAPI.GetSingleton<PhysicsSingleton>();
        if (!singleton.PhysicsVelocities.IsCreated || !singleton.BodiesList.IsCreated)
            return;

        var combinedDep = JobHandle.CombineDependencies(state.Dependency, singleton.PhysicsJobHandle);

        state.Dependency = new MoveJob
        {
            PhysicsVelocities = singleton.PhysicsVelocities,
            BodiesList = singleton.BodiesList,
            UpdateLookup = SystemAPI.GetComponentLookup<PhysicsBodyUpdateComponent>(true),
        }.Schedule(combinedDep);

        singleton.PhysicsJobHandle = state.Dependency;
        SystemAPI.SetSingleton(singleton);
    }

    [BurstCompile]
    public partial struct MoveJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public NativeArray<PhysicsVelocityData> PhysicsVelocities;
        [ReadOnly] public NativeArray<PhysicsBodyData> BodiesList;
        [ReadOnly] public ComponentLookup<PhysicsBodyUpdateComponent> UpdateLookup;

        public void Execute(
            in PhysicsBodyUpdateComponent update,
            in MoveInputComponent input,
            ref MoveOutputComponent output)
        {
            if (!update.IsEnabled || input.Speed <= 0f || input.Target == Entity.Null)
                return;

            if (!UpdateLookup.TryGetComponent(input.Target, out var targetUpdate))
                return;
                
            if (!targetUpdate.IsEnabled)
                return;

            var selfBody = BodiesList[update.Index];
            var targetBody = BodiesList[targetUpdate.Index];

            output.Position = selfBody.Position;
            output.Scale = selfBody.Scale;
            output.TargetPosition = targetBody.Position;
            output.TargetScale = targetBody.Scale;

            int index = update.Index;
            var velocity = PhysicsVelocities[index];
            velocity.Linear += GetLinearVelocity(selfBody.Position, velocity, targetBody.Position, selfBody.Scale, targetBody.Scale, input.MaxDistance, input.Speed);
            PhysicsVelocities[index] = velocity;
        }

        private static float3 GetLinearVelocity(
            float3 position,
            in PhysicsVelocityData velocity,
            float3 targetPosition,
            float selfScale,
            float targetScale,
            float maxDistance,
            float speed)
        {
            float threshold = (selfScale + targetScale) * 0.5f + maxDistance;
            float distance = math.distance(position, targetPosition);

            if (distance <= threshold)
                return float3.zero;

            float3 toTarget = math.normalizesafe(targetPosition - position);
            float3 desired = toTarget * speed;
            float3 toAdd = desired - velocity.Linear;

            float currentSpeedAlongDir = math.dot(velocity.Linear, toTarget);
            if (currentSpeedAlongDir >= speed)
                return float3.zero;

            float3 next = velocity.Linear + toAdd;
            if (math.length(next) > speed)
            {
                next = math.normalize(next) * speed;
                toAdd = next - velocity.Linear;
            }

            return toAdd;
        }
    }
}
