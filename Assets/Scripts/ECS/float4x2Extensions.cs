using Unity.Mathematics;

public static class float4x2Extensions
{
    public static readonly float4x2 One = new float4x2(
        new float4(1f, 1f, 1f, 1f),
        new float4(1f, 1f, 1f, 1f)
    );

    public static readonly float4x2 Zero = new float4x2(
        new float4(0f, 0f, 0f, 0f),
        new float4(0f, 0f, 0f, 0f)
    );

    public static float4x2 Clamp(this float4x2 matrix, float minValue, float maxValue)
    {
        var minBounds = One * minValue;
        var maxBounds = One * maxValue;
        matrix.c0 = math.clamp(matrix.c0, minBounds.c0, maxBounds.c0);
        matrix.c1 = math.clamp(matrix.c1, minBounds.c1, maxBounds.c1);
        return matrix;
    }

    public static float4x2 InverseLerp(float4x2 a, float4x2 b, float4x2 value)
    {
        float4x2 result = new float4x2();
        
        // Process column 0
        result.c0 = new float4(
            InverseLerpScalar(a.c0.x, b.c0.x, value.c0.x),
            InverseLerpScalar(a.c0.y, b.c0.y, value.c0.y),
            InverseLerpScalar(a.c0.z, b.c0.z, value.c0.z),
            InverseLerpScalar(a.c0.w, b.c0.w, value.c0.w)
        );
        
        // Process column 1
        result.c1 = new float4(
            InverseLerpScalar(a.c1.x, b.c1.x, value.c1.x),
            InverseLerpScalar(a.c1.y, b.c1.y, value.c1.y),
            InverseLerpScalar(a.c1.z, b.c1.z, value.c1.z),
            InverseLerpScalar(a.c1.w, b.c1.w, value.c1.w)
        );
        
        return result;
    }

    public static float GetWeight(this float4x2 value)
    {
        return value.c0.x + value.c0.y + value.c0.z + value.c0.w +
               value.c1.x + value.c1.y + value.c1.z + value.c1.w;
    }

    private static float InverseLerpScalar(float a, float b, float value)
    {
        if (math.abs(b - a) < 0.0001f)
            return 0.0f;
        return math.clamp((value - a) / (b - a), 0.0f, 1.0f);
    }
}

