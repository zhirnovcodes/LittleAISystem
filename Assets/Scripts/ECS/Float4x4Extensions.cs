using Unity.Mathematics;

public static class Float4x4Extensions
{
    /// <summary>
    /// Linearly interpolates between two float4x4 matrices using a single interpolation factor.
    /// </summary>
    /// <param name="a">The first matrix</param>
    /// <param name="b">The second matrix</param>
    /// <param name="t">The interpolation factor (0-1)</param>
    /// <returns>The interpolated matrix</returns>
    public static float4x4 Lerp(float4x4 a, float4x4 b, float t)
    {
        return new float4x4(
            math.lerp(a.c0, b.c0, t),
            math.lerp(a.c1, b.c1, t),
            math.lerp(a.c2, b.c2, t),
            math.lerp(a.c3, b.c3, t)
        );
    }
    
    /// <summary>
    /// Linearly interpolates between two float4x4 matrices using a matrix of interpolation factors.
    /// Each element is interpolated independently.
    /// </summary>
    /// <param name="a">The first matrix</param>
    /// <param name="b">The second matrix</param>
    /// <param name="t">The interpolation factors matrix (0-1 per element)</param>
    /// <returns>The interpolated matrix</returns>
    public static float4x4 Lerp(float4x4 a, float4x4 b, float4x4 t)
    {
        return new float4x4(
            math.lerp(a.c0, b.c0, t.c0),
            math.lerp(a.c1, b.c1, t.c1),
            math.lerp(a.c2, b.c2, t.c2),
            math.lerp(a.c3, b.c3, t.c3)
        );
    }
    
    /// <summary>
    /// Creates a float4x4 matrix with random values between 0 and 1.
    /// </summary>
    /// <param name="random">Reference to a Random generator</param>
    /// <returns>A matrix filled with random values</returns>
    public static float4x4 CreateRandom(ref Random random)
    {
        return new float4x4(
            random.NextFloat4(),
            random.NextFloat4(),
            random.NextFloat4(),
            random.NextFloat4()
        );
    }
}

