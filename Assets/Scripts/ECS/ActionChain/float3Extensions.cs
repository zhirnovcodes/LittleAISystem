using Unity.Mathematics;

public static class float3Extensions
{
    public static bool IsTargetPositionReached(this float3 position, float3 targetPosition, float distance = 0.01f)
    {
        return math.distancesq(position, targetPosition) <= distance * distance;
    }

    public static bool IsDistanceGreaterThan(this float3 position, float3 targetPosition, float distance)
    {
        return math.distancesq(position, targetPosition) > distance * distance;
    }
}

