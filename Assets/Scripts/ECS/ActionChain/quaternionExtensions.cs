using Unity.Mathematics;

public static class quaternionExtensions
{
    /// <summary>
    /// Checks if the rotation is facing towards the target direction.
    /// </summary>
    /// <param name="rotation">The current rotation</param>
    /// <param name="targetDirection">Direction to the target (not normalized position)</param>
    /// <param name="delta">Approximation threshold for comparison</param>
    public static bool IsRotationTowardsTargetReached(this quaternion rotation, float3 targetDirection, float delta = 0.01f)
    {
        float3 forward = math.mul(rotation, new float3(0, 0, 1));
        float3 direction = math.normalize(targetDirection);
        float dotProduct = math.dot(forward, direction);
        return dotProduct >= 1.0f - delta;
    }
}

