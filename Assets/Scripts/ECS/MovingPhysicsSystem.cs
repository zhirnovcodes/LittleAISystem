using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Physics;
using Unity.Collections;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct MovingPhysicsSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        var job = new MovingPhysicsJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            TransformLookup = transformLookup
        };
        var handle0 = job.Schedule(state.Dependency);

        var job2 = new UprightCorrectionJob
        {
        };
        var handle1 = job2.Schedule(handle0);

        state.Dependency = handle1;
    }

    [BurstCompile]
    public partial struct MovingPhysicsJob : IJobEntity
    {
        public float DeltaTime;
        [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;

        public void Execute(
            in LocalTransform transform,
            ref PhysicsVelocity velocity,
            in PhysicsMass mass,
            in MoveControllerInputComponent input)
        {
            if (input.Speed + input.RotationSpeed <= 0)
            {
                return;
            }

            var currentTransform = transform;

            var targetScale = input.TargetScale;
            var targetPosition = input.TargetPosition;
            var lookDirection = input.LookDirection;

            if (input.TargetEntity == Entity.Null == false)
            {
                if (TransformLookup.TryGetComponent(input.TargetEntity, out var targetTransform) == false)
                {
                    return;
                }

                targetScale = targetTransform.Scale;
                targetPosition = targetTransform.Position;
                lookDirection = math.normalize(targetPosition - transform.Position);
            }

            bool hasArrived = currentTransform.IsTargetDistanceReached(
                targetPosition, targetScale, input.Distance);

            bool isLookingAt = currentTransform.Rotation.IsLookingTowards(
                lookDirection, 0.01f);


            if (hasArrived)
            {
                velocity.Linear += float3.zero;
            }
            else
            {
                velocity.Linear += GetLinearVelocity(transform, velocity, targetPosition, targetScale, input.Distance, input.Speed);
            }

            if (isLookingAt)
            {
                velocity.Angular = float3.zero;
            }
            else
            {

                velocity.Angular = GetAngularVelocity(lookDirection, transform.Rotation, mass, input.RotationSpeed, DeltaTime);
            }
        }
        private float3 GetLinearVelocity(
            in LocalTransform transform,
            in PhysicsVelocity velocity,
            float3 targetPosition, 
            float targetScale,
            float distance,
            float speed)
        {
            // Already at target - no velocity to add
            if (transform.IsTargetDistanceReached(targetPosition, targetScale, distance))
                return float3.zero;

            float3 toTarget = math.normalize(targetPosition - transform.Position);
            float3 desiredVelocity = toTarget * speed;
            float3 velocityToAdd = desiredVelocity - velocity.Linear;

            // If we're already at or above target speed in the desired direction, don't add more
            float currentSpeedAlongDirection = math.dot(velocity.Linear, toTarget);
            if (currentSpeedAlongDirection >= speed)
                return float3.zero;

            // Clamp so we don't overshoot the target speed
            float3 newVelocity = velocity.Linear + velocityToAdd;
            if (math.length(newVelocity) > speed)
            {
                newVelocity = math.normalize(newVelocity) * speed;
                velocityToAdd = newVelocity - velocity.Linear;
            }

            return velocityToAdd;
        }

        public static float3 GetAngularVelocity(
            in float3 lookDirection,
            in quaternion rotation,
            in PhysicsMass mass,
            float rotationSpeed,
            float deltaTime)
        {
            if (math.lengthsq(lookDirection) < 0.0001f)
                return float3.zero;

            float3 lookDir = math.normalize(lookDirection);
            float3 forward = math.forward(rotation);

            float dot = math.clamp(math.dot(forward, lookDir), -1f, 1f);

            if (dot > 0.9999f)
                return float3.zero;

            float3 cross = math.cross(forward, lookDir);
            float crossLen = math.length(cross);

            if (crossLen < 0.0001f)
            {
                float3 up = math.mul(rotation, math.up());
                cross = math.cross(forward, up);
                crossLen = math.length(cross);
            }

            float3 axis = cross / crossLen;

            float angleRad = math.acos(dot);
            float maxStepRad = math.radians(rotationSpeed*2f) * deltaTime;
            float angularSpeed = math.min(angleRad, maxStepRad) / deltaTime;

            // World-space angular velocity
            float3 angularVelocityWorld = axis * angularSpeed;

            // Convert world space -> inertia space (what PhysicsVelocity.Angular expects)
            quaternion worldFromMotion = math.mul(rotation, mass.InertiaOrientation);
            return math.rotate(math.inverse(worldFromMotion), angularVelocityWorld);
        }
    }
    [BurstCompile]
    public partial struct UprightCorrectionJob : IJobEntity
    {
        private void Execute(
            ref PhysicsVelocity velocity,
            in PhysicsMass mass,
            in LocalTransform transform)
        {
            float3 localUp = math.mul(transform.Rotation, math.up() );
            float dot = math.dot(localUp, math.up());

            if (dot > 0.99f)
                return;

            float t = 1f - math.saturate(dot);
            float3 cross = math.cross(localUp, math.up());
            float sign = math.sign(math.dot(cross, math.forward(transform.Rotation)));

            float3 axis = math.rotate(math.inverse(mass.InertiaOrientation), math.forward());
            float3 correction = axis * math.radians(100f) * t * sign;

            // Project out existing correction on this axis, then add fresh
            velocity.Angular -= axis * math.dot(velocity.Angular, axis);
            velocity.Angular += correction;
        }
    }
}