using LittlePhysics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(ExportPhysicsDataSystem))]
public partial struct MoveRotationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RotationHandlerComponent>();
        state.RequireForUpdate<MoveInputComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new RotateTowardsTargetJob
        {
            InputLookup = SystemAPI.GetComponentLookup<MoveInputComponent>(true),
            OutputLookup = SystemAPI.GetComponentLookup<MoveOutputComponent>(false),
            TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(false),
            DeltaTime = SystemAPI.Time.DeltaTime,
        }.Schedule(state.Dependency);
    }

    [BurstCompile]
    public partial struct RotateTowardsTargetJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<MoveInputComponent> InputLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<MoveOutputComponent> OutputLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> TransformLookup;
        public float DeltaTime;

        public void Execute(
            Entity entity,
            in RotationHandlerComponent handler)
        {
            if (!InputLookup.TryGetComponent(handler.Parent, out var input))
                return;

            if (input.RotationSpeed <= 0f || input.Target == Entity.Null)
                return;

            if (!OutputLookup.HasComponent(handler.Parent))
                return;

            if (!TransformLookup.TryGetComponent(entity, out var handlerTransform))
                return;

            ref var output = ref OutputLookup.GetRefRW(handler.Parent).ValueRW;

            float3 toTarget = math.normalizesafe(output.TargetPosition - output.Position);
            if (math.lengthsq(toTarget) < 0.0001f)
                return;

            quaternion targetWorldRot = quaternion.LookRotationSafe(toTarget, input.Up);

            quaternion parentWorldRot = TransformLookup.TryGetComponent(handler.Parent, out var parentTransform)
                ? parentTransform.Rotation
                : quaternion.identity;

            quaternion currentWorldRot = math.mul(parentWorldRot, handlerTransform.Rotation);

            float angleDot = math.abs(math.clamp(math.dot(currentWorldRot.value, targetWorldRot.value), -1f, 1f));
            float remainingAngleRad = 2f * math.acos(angleDot);

            quaternion newWorldRot;
            if (remainingAngleRad < 0.0001f)
            {
                newWorldRot = targetWorldRot;
            }
            else
            {
                float maxStepRad = math.radians(input.RotationSpeed) * DeltaTime;
                float t = math.min(maxStepRad / remainingAngleRad, 1f);
                newWorldRot = math.slerp(currentWorldRot, targetWorldRot, t);
            }
            var newTransform = TransformLookup[entity];
            newTransform.Rotation = math.mul(math.inverse(parentWorldRot), newWorldRot);
            TransformLookup[entity] = newTransform;
            output.Rotation = newWorldRot;
        }
    }
}
