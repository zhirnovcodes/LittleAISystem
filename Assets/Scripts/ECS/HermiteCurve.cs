using Unity.Mathematics;

public struct HermiteCurve
{
    public float4 points;    // (x0, y0, x1, y1)
    public float2 tangents;  // (outTangent, inTangent)
}
