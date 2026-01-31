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
}

