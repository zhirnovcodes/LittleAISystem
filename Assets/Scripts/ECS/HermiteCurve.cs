using Unity.Mathematics;

[System.Serializable]
public struct HermiteCurve
{
    public float4 points;    // (x0, y0, x1, y1)
    public float2 tangents;  // (outTangent, inTangent)

    public float4x4 ToFloat4x4()
    {
        return new float4x4(
            points,
            new float4(tangents.x, tangents.y, 0, 0),
            float4.zero,
            float4.zero
        );
    }

    public static HermiteCurve FromFloat4x4(float4x4 data)
    {
        return new HermiteCurve
        {
            points = data.c0,
            tangents = new float2(data.c1.x, data.c1.y)
        };
    }
}
