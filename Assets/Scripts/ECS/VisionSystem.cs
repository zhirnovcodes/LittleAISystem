using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using LittlePhysics;

public struct WeightedVisionItem : IEquatable<WeightedVisionItem>, IComparable<WeightedVisionItem>
{
    public uint TargetIndex;
    public float Weight;

    public bool Equals(WeightedVisionItem other) => TargetIndex == other.TargetIndex;

    public int CompareTo(WeightedVisionItem other) => other.Weight.CompareTo(Weight);
}

[BurstCompile]
[UpdateInGroup(typeof(LittlePhysicsUserSystemGroup))]
public partial struct VisionSystem : ISystem
{
    public LittleHashMap<WeightedVisionItem> WeightedItems;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsSingleton>();
        state.RequireForUpdate<VisionComponent>();
        state.RequireForUpdate<LittlePhysicsTimeComponent>();
    }

    public void OnDestroy(ref SystemState state)
    {
        if (WeightedItems.IsCreated) WeightedItems.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var singleton = SystemAPI.GetSingleton<PhysicsSingleton>();
        if (!singleton.BodiesList.IsCreated || !singleton.Collisions.Collisions.IsCreated)
            return;

        if (!WeightedItems.IsCreated)
        {
            ref var lod = ref singleton.Settings.BlobRef.Value.LodData;
            WeightedItems = new LittleHashMap<WeightedVisionItem>(
                lod.MaxEntityCount,
                lod.MaxCollisionsPerEntity,
                Allocator.Persistent);
        }

        int bodyCount = singleton.Settings.BlobRef.Value.LodData.MaxEntityCount;
        var combinedDep = JobHandle.CombineDependencies(state.Dependency, singleton.PhysicsJobHandle);

        var clearDep = new ClearWeightedItemsJob
        {
            WeightedItems = WeightedItems
        }.Schedule(combinedDep);

        state.Dependency = new VisionJob
        {
            Collisions = singleton.Collisions.Collisions,
            BodiesList = singleton.BodiesList,
            BodiesCount = singleton.BodiesCount,
            WeightedItems = WeightedItems,
            VisionLookup = SystemAPI.GetComponentLookup<VisionComponent>(true),
            SafetyCheckLookup = SystemAPI.GetBufferLookup<SafetyCheckItem>(true),
            ConditionFlagsLookup = SystemAPI.GetComponentLookup<ConditionFlagsComponent>(true),
            StatAdvertiserLookup = SystemAPI.GetBufferLookup<StatAdvertiserItem>(true),
            StatsLookup = SystemAPI.GetComponentLookup<AnimalStatsComponent>(true)
        }.Schedule(bodyCount, 32, clearDep);

        var physicsTime = SystemAPI.GetSingleton<LittlePhysicsTimeComponent>();

        state.Dependency = new FillVisibleBufferJob
        {
            WeightedItems = WeightedItems,
            BodiesList = singleton.BodiesList,
            BodiesCount = singleton.BodiesCount,
            VisibleItemLookup = SystemAPI.GetBufferLookup<VisibleItem>(),
            ElapsedTime = physicsTime.ElapsedTime
        }.Schedule(bodyCount, 32, state.Dependency);

        singleton.PhysicsJobHandle = state.Dependency;
        SystemAPI.SetSingleton(singleton);
    }
}

[BurstCompile]
public struct ClearWeightedItemsJob : IJob
{
    public LittleHashMap<WeightedVisionItem> WeightedItems;

    public void Execute()
    {
        WeightedItems.Clear();
    }
}

[BurstCompile]
public struct VisionJob : IJobParallelFor
{
    [ReadOnly] public LittleHashMap<CollisionData> Collisions;
    [ReadOnly] public NativeArray<PhysicsBodyData> BodiesList;
    [ReadOnly] public NativeReference<uint> BodiesCount;
    [NativeDisableContainerSafetyRestriction] public LittleHashMap<WeightedVisionItem> WeightedItems;
    [ReadOnly] public ComponentLookup<VisionComponent> VisionLookup;
    [ReadOnly] public BufferLookup<SafetyCheckItem> SafetyCheckLookup;
    [ReadOnly] public ComponentLookup<ConditionFlagsComponent> ConditionFlagsLookup;
    [ReadOnly] public BufferLookup<StatAdvertiserItem> StatAdvertiserLookup;
    [ReadOnly] public ComponentLookup<AnimalStatsComponent> StatsLookup;

    public void Execute(int index)
    {
        if ((uint)index >= BodiesCount.Value)
            return;

        var body = BodiesList[index];

        if (!VisionLookup.TryGetComponent(body.Main, out var vision))
            return;

        FillOrderList(body.Main, body.Position, vision.MaxDistance, index);
    }

    private void FillOrderList(Entity entity, float3 selfPosition, float maxDistance, int bodyIndex)
    {
        var iterator = Collisions.GetSingleIterator(bodyIndex);
        while (Collisions.Traverse(ref iterator, out var pair))
        {
            var collision = pair.Item2;
            uint otherIndex = (uint)bodyIndex == collision.Body1 ? collision.Body2 : collision.Body1;
            var otherBody = BodiesList[(int)otherIndex];

            float weight = CalculateWeight(entity, selfPosition, maxDistance, otherBody.Main, otherBody.Position);
            WeightedItems.TryAdd((uint)bodyIndex, new WeightedVisionItem
            {
                TargetIndex = otherIndex,
                Weight = weight
            });
        }
    }

    private float CalculateWeight(Entity entity, float3 selfPosition, float maxDistance, Entity hitEntity, float3 hitPosition)
    {
        float distanceWeight = GetDistanceWeight(selfPosition, maxDistance, hitPosition);
        float safetyWeight = GetSafetyWeight(entity, hitEntity);
        float statNeedWeight = GetStatNeedWeight(entity, hitEntity);
        return safetyWeight * 1000f + statNeedWeight * distanceWeight;
    }

    private float GetSafetyWeight(Entity entity, Entity hitEntity)
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

    private float GetDistanceWeight(float3 selfPosition, float maxDistance, float3 hitPosition)
    {
        if (maxDistance <= 0f)
            return 0f;

        float distance = math.distance(selfPosition, hitPosition);
        return 1f - math.clamp(distance / maxDistance, 0f, 1f);
    }

    private float GetStatNeedWeight(Entity entity, Entity hitEntity)
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
}

[BurstCompile]
public struct FillVisibleBufferJob : IJobParallelFor
{
    [ReadOnly] public LittleHashMap<WeightedVisionItem> WeightedItems;
    [ReadOnly] public NativeArray<PhysicsBodyData> BodiesList;
    [ReadOnly] public NativeReference<uint> BodiesCount;
    [NativeDisableParallelForRestriction] public BufferLookup<VisibleItem> VisibleItemLookup;
    public double ElapsedTime;

    const double DeleteTime = 0.5;

    public void Execute(int index)
    {
        if ((uint)index >= BodiesCount.Value)
            return;

        var body = BodiesList[index];

        if (!VisibleItemLookup.TryGetBuffer(body.Main, out var visibleBuffer))
            return;

        for (int i = visibleBuffer.Length - 1; i >= 0; i--)
        {
            if (ElapsedTime - visibleBuffer[i].TimeAdded >= DeleteTime)
                visibleBuffer.RemoveAt(i);
        }

        float bestWeight = float.MinValue;
        uint bestTargetIndex = uint.MaxValue;

        var iterator = WeightedItems.GetSingleIterator(index);
        while (WeightedItems.Traverse(ref iterator, out var pair))
        {
            var item = pair.Item2;
            if (item.Weight > bestWeight)
            {
                bestWeight = item.Weight;
                bestTargetIndex = item.TargetIndex;
            }
        }

        if (bestTargetIndex == uint.MaxValue)
            return;

        var bestTarget = BodiesList[(int)bestTargetIndex].Main;

        for (int i = 0; i < visibleBuffer.Length; i++)
        {
            if (visibleBuffer[i].Target == bestTarget)
                return;
        }

        visibleBuffer.Add(new VisibleItem { Target = bestTarget, TimeAdded = ElapsedTime });
    }
}
