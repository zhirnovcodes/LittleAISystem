using System;
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
            PhysicsWorld = physicsWorld,
            SafetyCheckLookup = SystemAPI.GetBufferLookup<SafetyCheckItem>(true),
            ConditionFlagsLookup = SystemAPI.GetComponentLookup<ConditionFlagsComponent>(true),
            StatAdvertiserLookup = SystemAPI.GetBufferLookup<StatAdvertiserItem>(true),
            StatsLookup = SystemAPI.GetComponentLookup<AnimalStatsComponent>(true)
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct VisionJob : IJobEntity
{
    public float DeltaTime;
    [ReadOnly] public PhysicsWorldSingleton PhysicsWorld;
    [ReadOnly] public BufferLookup<SafetyCheckItem> SafetyCheckLookup;
    [ReadOnly] public ComponentLookup<ConditionFlagsComponent> ConditionFlagsLookup;
    [ReadOnly] public BufferLookup<StatAdvertiserItem> StatAdvertiserLookup;
    [ReadOnly] public ComponentLookup<AnimalStatsComponent> StatsLookup;

    const int MaxVisionItems = 16;

    struct WeightedVisionItem : IComparable<WeightedVisionItem>
    {
        public int HitIndex;
        public float Weight;

        public int CompareTo(WeightedVisionItem other)
        {
            int weightComparison = other.Weight.CompareTo(Weight);
            if (weightComparison != 0)
                return weightComparison;

            return HitIndex.CompareTo(other.HitIndex);
        }
    }

    void Execute(ref VisionComponent vision, DynamicBuffer<VisibleItem> visibleBuffer, 
        in LocalTransform transform, Entity entity)
    {
        // Update timer
        vision.TimeElapsed += DeltaTime;

        // Check if it's time to perform vision check
        if (vision.TimeElapsed < vision.Interval)
        {
            return;
        }

        vision.TimeElapsed = 0f;

        // Clear previous visible items
        visibleBuffer.Clear();

        // Perform sphere cast
        var position = transform.Position;
        var maxDistance = vision.MaxDistance;

        // Create a list to store all hits
        var hits = new NativeList<ColliderCastHit>(Allocator.Temp);
        var order = new NativeList<WeightedVisionItem>(Allocator.Temp);

        var collisionFilter = new CollisionFilter
        {
            BelongsTo = (uint)Layers.Vision,              // This query belongs to all layers (or specify if needed)
            CollidesWith = (uint)Layers.Animal, // Only collide with the Fish layer
            GroupIndex = 0
        };

        // Perform sphere cast to get all entities within range
        var collisionWorld = PhysicsWorld.PhysicsWorld.CollisionWorld;
        collisionWorld.SphereCastAll(
            position,
            maxDistance,
            float3.zero,
            0f,
            ref hits,
            collisionFilter);

        FillOrderList(entity, position, maxDistance, hits, ref order);
        OrderListByWeightDescending(ref order);

        for (int i = 0; i < math.min( MaxVisionItems, order.Length); i++)
        {
            var hit = hits[order[i].HitIndex];
            var hitEntity = hit.Entity;
            visibleBuffer.Add(new VisibleItem { Target = hitEntity });
        }

        hits.Dispose();
        order.Dispose();
    }

    void FillOrderList(
        Entity entity,
        float3 selfPosition,
        float maxDistance,
        in NativeList<ColliderCastHit> hits,
        ref NativeList<WeightedVisionItem> order)
    {
        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            var hitEntity = hit.Entity;

            if (hitEntity == entity)
                continue;

            order.Add(new WeightedVisionItem
            {
                HitIndex = i,
                Weight = CalculateWeight(entity, selfPosition, maxDistance, hitEntity, hit)
            });
        }
    }

    float CalculateWeight(
        Entity entity,
        float3 selfPosition,
        float maxDistance,
        Entity hitEntity,
        in ColliderCastHit hit)
    {
        float distanceWeight = GetDistanceWeight(selfPosition, maxDistance, hit);
        float safetyWeight = GetSafetyWeight(entity, hitEntity);
        float statNeedWeight = GetStatNeedWeight(entity, hitEntity);
        return safetyWeight * 1000f + statNeedWeight * distanceWeight;
    }

    float GetSafetyWeight(Entity entity, Entity hitEntity)
    {
        if (!SafetyCheckLookup.TryGetBuffer(entity, out var safetyChecks))
            return 0f;

        if (!ConditionFlagsLookup.TryGetComponent(hitEntity, out var visibleConditions))
            return 0f;

        for (int i = 0; i < safetyChecks.Length; i++)
        {
            if (visibleConditions.Conditions.IsAllConditionMet(safetyChecks[i].ActorConditions))
                return 2f;
        }

        return 0f;
    }

    float GetDistanceWeight(float3 selfPosition, float maxDistance, in ColliderCastHit hit)
    {
        if (maxDistance <= 0f)
            return 0f;

        float distance = math.distance(selfPosition, hit.Position);
        return 1f - math.clamp(distance / maxDistance, 0f, 1f);
    }

    float GetStatNeedWeight(Entity entity, Entity hitEntity)
    {
        if (!StatsLookup.TryGetComponent(entity, out var actorStatsComponent))
            return 0f;

        if (!StatAdvertiserLookup.TryGetBuffer(hitEntity, out var advertisedItems))
            return 0f;

        ConditionFlags actorConditions = ConditionFlags.None;
        if (ConditionFlagsLookup.TryGetComponent(entity, out var actorConditionFlags))
            actorConditions = actorConditionFlags.Conditions;

        float totalWeight = 0f;
        var actorStats = actorStatsComponent.Stats;

        for (int i = 0; i < advertisedItems.Length; i++)
        {
            var advertisedItem = advertisedItems[i];
            if (!actorConditions.IsAllConditionMet(advertisedItem.ActorConditions))
                continue;

            AnimalStats delta =
                (actorStats + advertisedItem.AdvertisedValue).Clamp(0f, 100f) - actorStats;
            totalWeight += delta.GetWeight();
        }

        return totalWeight;
    }

    void OrderListByWeightDescending(ref NativeList<WeightedVisionItem> order)
    {
        order.Sort();
    }
}