using Unity.Mathematics;

public static class quaternionExtensions
{
    public static bool IsRotationTowardsTargetReached(this quaternion rotation, float3 targetPosition, float delta = 0.01f)
    {
        float3 forward = math.mul(rotation, new float3(0, 0, 1));
        float3 direction = math.normalize(targetPosition);
        float dotProduct = math.dot(forward, direction);
        return dotProduct >= 1.0f - delta;
    }
}

