using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct AttenuationTestSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunAttenuationTestComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        Debug.Log("=== Starting Attenuation Extension Tests ===");

        TestGetY();
        TestGetYs();
        TestConvertFromAnimationCurve();
        TestAnimalStatsAttenuation();

        Debug.Log("=== All Attenuation Tests Complete ===");
    }

    private void TestGetY()
    {
        Debug.Log("\n--- Testing HermiteCurve.GetY() ---");

        // Create a simple hermite curve from (0, 0) to (1, 1) with tangents 1 and 1
        HermiteCurve curve = new HermiteCurve
        {
            points = new float4(0f, 0f, 1f, 1f),
            tangents = new float2(1f, 1f)
        };

        // Test at start point
        float resultAtStart = curve.GetY(0f);
        Debug.Log($"GetY at x=0: {resultAtStart} (expected: 0)");
        AssertApprox(resultAtStart, 0f, "GetY at start");

        // Test at end point
        float resultAtEnd = curve.GetY(1f);
        Debug.Log($"GetY at x=1: {resultAtEnd} (expected: 1)");
        AssertApprox(resultAtEnd, 1f, "GetY at end");

        // Test at midpoint
        float resultAtMid = curve.GetY(0.5f);
        Debug.Log($"GetY at x=0.5: {resultAtMid}");

        // Test before start (should clamp)
        float resultBefore = curve.GetY(-1f);
        Debug.Log($"GetY at x=-1: {resultBefore} (expected: 0, clamped)");
        AssertApprox(resultBefore, 0f, "GetY before start (clamped)");

        // Test after end (should clamp)
        float resultAfter = curve.GetY(2f);
        Debug.Log($"GetY at x=2: {resultAfter} (expected: 1, clamped)");
        AssertApprox(resultAfter, 1f, "GetY after end (clamped)");

        // Test a curve from (0, 50) to (10, 100) with flat tangents
        HermiteCurve curve2 = new HermiteCurve
        {
            points = new float4(0f, 50f, 10f, 100f),
            tangents = new float2(0f, 0f)
        };

        float result2AtStart = curve2.GetY(0f);
        Debug.Log($"GetY at x=0: {result2AtStart} (expected: 50)");
        AssertApprox(result2AtStart, 50f, "GetY curve2 at start");

        float result2AtEnd = curve2.GetY(10f);
        Debug.Log($"GetY at x=10: {result2AtEnd} (expected: 100)");
        AssertApprox(result2AtEnd, 100f, "GetY curve2 at end");

        float result2AtMid = curve2.GetY(5f);
        Debug.Log($"GetY at x=5: {result2AtMid}");
    }

    private void TestGetYs()
    {
        Debug.Log("\n--- Testing HermiteCurve4x2.GetYs() ---");

        // Create HermiteCurve4x2 with simple linear curves
        HermiteCurve4x2 curves = new HermiteCurve4x2();

        // Set up 8 curves (4x2) - all simple linear from (0,0) to (1,1)
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                curves[i, j] = new HermiteCurve
                {
                    points = new float4(0f, 0f, 1f, 1f),
                    tangents = new float2(1f, 1f)
                };
            }
        }

        // Test with midpoint inputs
        float4x2 inputs = new float4x2(
            new float4(0.5f, 0.5f, 0.5f, 0.5f),
            new float4(0.5f, 0.5f, 0.5f, 0.5f)
        );

        float4x2 results = curves.GetYs(inputs);

        Debug.Log($"GetYs results c0: {results.c0}");
        Debug.Log($"GetYs results c1: {results.c1}");

        // Test with start points
        float4x2 startInputs = new float4x2(
            new float4(0f, 0f, 0f, 0f),
            new float4(0f, 0f, 0f, 0f)
        );

        float4x2 startResults = curves.GetYs(startInputs);
        Debug.Log($"GetYs at start c0: {startResults.c0} (expected all 0)");
        Debug.Log($"GetYs at start c1: {startResults.c1} (expected all 0)");

        // Test with end points
        float4x2 endInputs = new float4x2(
            new float4(1f, 1f, 1f, 1f),
            new float4(1f, 1f, 1f, 1f)
        );

        float4x2 endResults = curves.GetYs(endInputs);
        Debug.Log($"GetYs at end c0: {endResults.c0} (expected all 1)");
        Debug.Log($"GetYs at end c1: {endResults.c1} (expected all 1)");
    }

    private void TestConvertFromAnimationCurve()
    {
        Debug.Log("\n--- Testing ConvertFromAnimationCurve() ---");

        // Create an AnimationCurve with 2 keyframes
        AnimationCurve animCurve = new AnimationCurve();
        animCurve.AddKey(new Keyframe(0f, 0f, 0f, 2f));  // time=0, value=0, inTangent=0, outTangent=2
        animCurve.AddKey(new Keyframe(5f, 100f, 1f, 0f)); // time=5, value=100, inTangent=1, outTangent=0

        HermiteCurve converted = HermiteCurveExtension.ConvertFromAnimationCurve(animCurve);

        Debug.Log($"Converted points: {converted.points} (expected: (0, 0, 5, 100))");
        Debug.Log($"Converted tangents: {converted.tangents} (expected: (2, 1))");

        AssertApprox(converted.points.x, 0f, "Converted x0");
        AssertApprox(converted.points.y, 0f, "Converted y0");
        AssertApprox(converted.points.z, 5f, "Converted x1");
        AssertApprox(converted.points.w, 100f, "Converted y1");
        AssertApprox(converted.tangents.x, 2f, "Converted outTangent");
        AssertApprox(converted.tangents.y, 1f, "Converted inTangent");

        // Test with invalid curve (not 2 keys)
        AnimationCurve invalidCurve = new AnimationCurve();
        invalidCurve.AddKey(new Keyframe(0f, 0f));
        invalidCurve.AddKey(new Keyframe(5f, 100f));
        invalidCurve.AddKey(new Keyframe(10f, 50f));

        Debug.Log("Testing invalid curve (should log error)...");
        HermiteCurve invalidResult = HermiteCurveExtension.ConvertFromAnimationCurve(invalidCurve);
    }

    private void TestAnimalStatsAttenuation()
    {
        Debug.Log("\n--- Testing AnimalStatsAttenuation Integration ---");

        // Create an attenuation structure
        var attenuationBuilder = new AnimalStatsAttenuationBuilder();

        // Create simple needs attenuation curves
        HermiteCurve4x2 needsCurves = new HermiteCurve4x2();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                needsCurves[i, j] = new HermiteCurve
                {
                    points = new float4(0f, 1f, 100f, 0f), // High attenuation when need is low
                    tangents = new float2(-0.01f, -0.01f)
                };
            }
        }

        // Create simple distance attenuation curves
        HermiteCurve4x2 distanceCurves = new HermiteCurve4x2();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                distanceCurves[i, j] = new HermiteCurve
                {
                    points = new float4(0f, 1f, 100f, 0f), // Attenuation decreases with distance
                    tangents = new float2(-0.01f, -0.01f)
                };
            }
        }

        var attenuation = attenuationBuilder
            .WithNeedsAttenuations(needsCurves)
            .WithDistanceAttenuations(distanceCurves)
            .Build();

        // Test needs attenuation evaluation
        float4x2 needsValues = new float4x2(
            new float4(25f, 50f, 75f, 100f),
            new float4(0f, 50f, 0f, 0f)
        );

        float4x2 needsAttenuationResults = attenuation.NeedsAttenuation.GetYs(needsValues);
        Debug.Log($"Needs attenuation c0: {needsAttenuationResults.c0}");
        Debug.Log($"Needs attenuation c1: {needsAttenuationResults.c1}");

        // Test distance attenuation evaluation
        float4x2 distanceValues = new float4x2(
            new float4(0f, 2.5f, 5f, 7.5f),
            new float4(10f, 5f, 0f, 0f)
        );

        float4x2 distanceAttenuationResults = attenuation.DistanceAttenuation.GetYs(distanceValues);
        Debug.Log($"Distance attenuation c0: {distanceAttenuationResults.c0}");
        Debug.Log($"Distance attenuation c1: {distanceAttenuationResults.c1}");

        // Test individual property accessors for needs and distance attenuation
        Debug.Log($"EnergyNeedsAttenuation: {attenuation.EnergyNeedsAttenuation.points}");
        Debug.Log($"EnergyDistanceAttenuation: {attenuation.EnergyDistanceAttenuation.points}");
    }

    private void AssertApprox(float actual, float expected, string testName, float tolerance = 0.001f)
    {
        if (math.abs(actual - expected) > tolerance)
        {
            Debug.LogError($"FAILED: {testName} - Expected: {expected}, Got: {actual}");
        }
        else
        {
            Debug.Log($"PASSED: {testName}");
        }
    }
}

