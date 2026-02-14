using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct VisionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<VisionComponent>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        new VisionJob
        {
            DeltaTime = deltaTime,
            PhysicsWorld = physicsWorld
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct VisionJob : IJobEntity
{
    public float DeltaTime;
    [ReadOnly] public PhysicsWorldSingleton PhysicsWorld;

    void Execute(ref VisionComponent vision, DynamicBuffer<VisibleItem> visibleBuffer, 
        in LocalTransform transform, Entity entity)
    {
        // Update timer
        vision.TimeElapsed += DeltaTime;

        // Check if it's time to perform vision check
        if (vision.TimeElapsed >= vision.Interval)
        {
            vision.TimeElapsed = 0f;

            // Clear previous visible items
            visibleBuffer.Clear();

            // Perform sphere cast
            var position = transform.Position;
            var maxDistance = vision.MaxDistance;

            // Create a list to store all hits
            var hits = new NativeList<ColliderCastHit>(Allocator.Temp);

            // Perform sphere cast to get all entities within range
            var collisionWorld = PhysicsWorld.PhysicsWorld.CollisionWorld;
            collisionWorld.SphereCastAll(
                position, 
                maxDistance, 
                float3.zero, 
                0f, 
                ref hits, 
                CollisionFilter.Default);

            // Add all detected entities to the visible buffer
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                var hitEntity = PhysicsWorld.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                
                // Don't add self to visible list
                if (hitEntity != entity)
                {
                    visibleBuffer.Add(new VisibleItem { Target = hitEntity });
                }
            }

            hits.Dispose();
        }
    }
}