using Unity.Mathematics;
using Unity.Transforms;

public static class LocalTransformExtensions
{
    public static bool IsTargetPositionReached(this LocalTransform transform, float3 position, float distance = 0.01f)
    {
        return transform.Position.IsTargetPositionReached(position, distance);
    }

    public static bool IsRotationTowardsTargetReached(this LocalTransform transform, float3 targetPosition, float delta = 0.01f)
    {
        return transform.Rotation.IsRotationTowardsTargetReached(targetPosition, delta);
    }

    public static bool IsDistanceGreaterThan(this LocalTransform transform, float3 position, float distance)
    {
        return transform.Position.IsDistanceGreaterThan(position, distance);
    }

    public static bool IsTargetReached(this LocalTransform transform, float3 position, float scale, float distance = 0.01f)
    {
        float reachThreshold = (transform.Scale + scale) * 0.5f + distance;
        return math.distancesq(transform.Position, position) <= reachThreshold * reachThreshold;
    }

    public static bool IsTargetReached(this LocalTransform transform, LocalTransform other, float distance = 0.01f)
    {
        return transform.IsTargetReached(other.Position, other.Scale, distance);
    }

    public static bool IsDistanceGreaterThan(this LocalTransform transform, float3 position, float scale, float distance)
    {
        float distanceThreshold = (transform.Scale + scale) * 0.5f + distance;
        return math.distancesq(transform.Position, position) > distanceThreshold * distanceThreshold;
    }

    public static bool IsDistanceGreaterThan(this LocalTransform transform, LocalTransform other, float distance)
    {
        return transform.IsDistanceGreaterThan(other.Position, other.Scale, distance);
    }
}

