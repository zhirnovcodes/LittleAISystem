using Unity.Mathematics;
using UnityEngine;

public static class HermiteCurveExtension
{
    public static float GetY(this HermiteCurve curve, float x)
    {
        float x0 = curve.points.x;
        float y0 = curve.points.y;
        float x1 = curve.points.z;
        float y1 = curve.points.w;
        float outTan = curve.tangents.x;
        float inTan = curve.tangents.y;

        if (x <= x0) return y0;
        if (x >= x1) return y1;

        float dt = x1 - x0;
        float t = (x - x0) / dt;

        float t2 = t * t;
        float t3 = t2 * t;

        float h00 = 2f * t3 - 3f * t2 + 1f;
        float h10 = t3 - 2f * t2 + t;
        float h01 = -2f * t3 + 3f * t2;
        float h11 = t3 - t2;

        return h00 * y0 + h10 * outTan * dt + h01 * y1 + h11 * inTan * dt;
    }

    public static float4 GetYs(this HermiteCurve4 curves, float4 inputs)
    {
        return new float4(
            curves.x.GetY(inputs.x),
            curves.y.GetY(inputs.y),
            curves.z.GetY(inputs.z),
            curves.w.GetY(inputs.w)
        );
    }

    public static float4x2 GetYs(this HermiteCurve4x2 curves, float4x2 inputs)
    {
        float4x2 result;
            
        // Process column 0
        result.c0 = curves.c0.GetYs(inputs.c0);
            
        // Process column 1
        result.c1 = curves.c1.GetYs(inputs.c1);

        return result;
    }

    public static HermiteCurve ConvertFromAnimationCurve(AnimationCurve curve)
    {
        if (curve.length != 2)
        {
            Debug.LogError("AnimationCurve must have 2 keys!");
            return default;
        }

        Keyframe a = curve[0];
        Keyframe b = curve[1];

        HermiteCurve result;
        result.points = new float4(a.time, a.value, b.time, b.value);
        result.tangents = new float2(a.outTangent, b.inTangent);

        return result;
    }
}

