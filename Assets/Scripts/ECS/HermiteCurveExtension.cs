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

