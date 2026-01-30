using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct NeedBasedAITestSystem : ISystem
{
    private struct TestAdvertiser
    {
        public AnimalStats AdvertisedValue;
        public float Distance;
    }

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunNeedBasedAITestComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        Debug.Log("=== Starting NeedBasedAI Tests ===");

        TestCase1();
        TestCase2();
        TestCase3();
        TestCase4();
        TestCase5();

        Debug.Log("=== All NeedBasedAI Tests Complete ===");
    }

    private void TestCase1()
    {
        Debug.Log("\n--- Test Case 1: Attenuation off, biggest weight wins ---");
        
        var distanceAttenuation = CreateHermiteCurve4x2(HermiteCurveType.Linear1);
        var maxDistances = CreateFloat4x2(1f);
        var statsAttenuation = CreateHermiteCurve4x2(HermiteCurveType.Linear01); // Identity function for no attenuation
        var currentStats = CreateAnimalStats(0f);

        var adv0 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStatsCustom(10f, 0f, 0f, 0f, 10f, 0f), // hunger + safety = 20
            Distance = 1f
        };

        var adv1 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStatsCustom(5f, 0f, 0f, 0f, 1f, 0f), // energy + safety = 6
            Distance = 2f
        };

        var adv2 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStatsCustom(0f, 0f, 3f, 0f, 0f, 10f), // toilet + health = 13
            Distance = 0f
        };

        int resultIndex = RunTestCase(
            distanceAttenuation,
            maxDistances,
            statsAttenuation,
            currentStats,
            adv0,
            adv1,
            adv2
        );

        AssertEquals(resultIndex, 0, "TestCase1");
        Debug.Log($"Test Case 1: Expected Advertiser0, Got {GetAdvertiserName(resultIndex)} - {(resultIndex == 0 ? "PASSED" : "FAILED")}");
    }

    private void TestCase2()
    {
        Debug.Log("\n--- Test Case 2: All advertisers advertise 0, should return idle ---");
        
        var distanceAttenuation = CreateHermiteCurve4x2(HermiteCurveType.Linear1);
        var maxDistances = CreateFloat4x2(1f);
        var statsAttenuation = CreateHermiteCurve4x2(HermiteCurveType.Linear01); // Identity function
        var currentStats = CreateAnimalStats(0f);

        var adv0 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStatsCustom(0f, 0f, 0f, 0f, 0f, 0f),
            Distance = 1f
        };

        var adv1 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStats(0f),
            Distance = 2f
        };

        var adv2 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStats(0f),
            Distance = 2f
        };

        int resultIndex = RunTestCase(
            distanceAttenuation,
            maxDistances,
            statsAttenuation,
            currentStats,
            adv0,
            adv1,
            adv2
        );

        AssertEquals(resultIndex, -1, "TestCase2");
        Debug.Log($"Test Case 2: Expected Idle, Got {GetAdvertiserName(resultIndex)} - {(resultIndex == -1 ? "PASSED" : "FAILED")}");
    }

    private void TestCase3()
    {
        Debug.Log("\n--- Test Case 3: Same value, best distance wins (Linear10 - closer is better) ---");
        
        var distanceAttenuation = CreateHermiteCurve4x2(HermiteCurveType.Linear10);
        var maxDistances = CreateFloat4x2(3f);
        var statsAttenuation = CreateHermiteCurve4x2(HermiteCurveType.Linear01); // Identity function
        var currentStats = CreateAnimalStats(0f);

        var adv0 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStatsCustom(0f, 0f, 0f, 0f, 10f, 0f), // safety = 10
            Distance = 1f
        };

        var adv1 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStatsCustom(10f, 0f, 0f, 0f, 0f, 0f), // hunger = 10
            Distance = 2f
        };

        var adv2 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStatsCustom(0f, 0f, 0f, 0f, 0f, 10f), // health = 10
            Distance = 3f
        };

        int resultIndex = RunTestCase(
            distanceAttenuation,
            maxDistances,
            statsAttenuation,
            currentStats,
            adv0,
            adv1,
            adv2
        );

        AssertEquals(resultIndex, 0, "TestCase3");
        Debug.Log($"Test Case 3: Expected Advertiser0, Got {GetAdvertiserName(resultIndex)} - {(resultIndex == 0 ? "PASSED" : "FAILED")}");
    }

    private void TestCase4()
    {
        Debug.Log("\n--- Test Case 4: Same value, best distance wins (Linear01 - farther is better) ---");
        
        var distanceAttenuation = CreateHermiteCurve4x2(HermiteCurveType.Linear01);
        var maxDistances = CreateFloat4x2(3f);
        var statsAttenuation = CreateHermiteCurve4x2(HermiteCurveType.Linear01); // Identity function
        var currentStats = CreateAnimalStats(0f);

        var adv0 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStatsCustom(0f, 0f, 0f, 0f, 10f, 0f), // safety = 10
            Distance = 1f
        };

        var adv1 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStatsCustom(10f, 0f, 0f, 0f, 0f, 0f), // hunger = 10
            Distance = 2f
        };

        var adv2 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStatsCustom(0f, 0f, 0f, 0f, 0f, 10f), // health = 10
            Distance = 3f
        };

        int resultIndex = RunTestCase(
            distanceAttenuation,
            maxDistances,
            statsAttenuation,
            currentStats,
            adv0,
            adv1,
            adv2
        );

        AssertEquals(resultIndex, 2, "TestCase4");
        Debug.Log($"Test Case 4: Expected Advertiser2, Got {GetAdvertiserName(resultIndex)} - {(resultIndex == 2 ? "PASSED" : "FAILED")}");
    }

    private void TestCase5()
    {
        Debug.Log("\n--- Test Case 5: Best need according to attenuated value wins ---");
        
        // Create mixed attenuation curves
        var statsAttenuation = new HermiteCurve4x2();
        // Energy (0,0) - Linear01 (identity - no special attenuation)
        statsAttenuation[0, 0] = CreateHermiteCurve(HermiteCurveType.Linear01);
        // Fullness (1,0) - Linear01 (identity)
        statsAttenuation[1, 0] = CreateHermiteCurve(HermiteCurveType.Linear01);
        // Toilet (2,0) - Linear01 (identity)
        statsAttenuation[2, 0] = CreateHermiteCurve(HermiteCurveType.Linear01);
        // Social (3,0) - Linear01 (identity)
        statsAttenuation[3, 0] = CreateHermiteCurve(HermiteCurveType.Linear01);
        // Safety (0,1) - Linear01 (identity)
        statsAttenuation[0, 1] = CreateHermiteCurve(HermiteCurveType.Linear01);
        // Health (1,1) - Square01 (convex - increases importance at higher values)
        statsAttenuation[1, 1] = CreateHermiteCurve(HermiteCurveType.Square01);
        // Unused (2,1) - Linear01
        statsAttenuation[2, 1] = CreateHermiteCurve(HermiteCurveType.Linear01);
        // Unused (3,1) - Linear01
        statsAttenuation[3, 1] = CreateHermiteCurve(HermiteCurveType.Linear01);

        var distanceAttenuation = CreateHermiteCurve4x2(HermiteCurveType.Linear1);
        var maxDistances = CreateFloat4x2(1f);
        var currentStats = CreateAnimalStats(50f);

        var adv0 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStatsCustom(0f, 0f, 0f, 0f, 25f, 0f), // safety = 25
            Distance = 1f
        };

        var adv1 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStatsCustom(0f, 0f, 0f, 0f, 0f, 25f), // health = 25
            Distance = 1f
        };

        var adv2 = new TestAdvertiser
        {
            AdvertisedValue = CreateAnimalStatsCustom(25f, 0f, 0f, 0f, 0f, 0f), // hunger = 25
            Distance = 0f
        };

        int resultIndex = RunTestCase(
            distanceAttenuation,
            maxDistances,
            statsAttenuation,
            currentStats,
            adv0,
            adv1,
            adv2
        );

        AssertEquals(resultIndex, 1, "TestCase5");
        Debug.Log($"Test Case 5: Expected Advertiser1, Got {GetAdvertiserName(resultIndex)} - {(resultIndex == 1 ? "PASSED" : "FAILED")}");
    }

    private int RunTestCase(
        HermiteCurve4x2 DistanceAttenuation,
        float4x2 MaxDistances,
        HermiteCurve4x2 StatsAttenuation,
        AnimalStats CurrentStats,
        TestAdvertiser Advertiser0,
        TestAdvertiser Advertiser1,
        TestAdvertiser Advertiser2)
    {
        // Create attenuation component
        var attenuationBuilder = new AnimalStatsAttenuationBuilder();
        var attenuation = attenuationBuilder
            .WithNeedsAttenuations(StatsAttenuation)
            .WithDistanceAttenuations(DistanceAttenuation)
            .WithMaxDistances(MaxDistances)
            .Build();

        var statsComponent = new AnimalStatsComponent { Stats = CurrentStats };
        var attenuationComponent = new AnimalStatsAttenuationComponent { Attenuation = attenuation };

        // Create local transform at 0,0,0
        var selfTransform = new LocalTransform
        {
            Position = float3.zero,
            Rotation = quaternion.identity,
            Scale = 1f
        };

        // Create NeedBasedInputItems for each advertiser
        var item0 = new NeedBasedInputItem
        {
            Target = Entity.Null,
            StatsAdvertised = Advertiser0.AdvertisedValue,
            Position = new float3(Advertiser0.Distance, 0, 0),
            Scale = 1f
        };

        var item1 = new NeedBasedInputItem
        {
            Target = Entity.Null,
            StatsAdvertised = Advertiser1.AdvertisedValue,
            Position = new float3(Advertiser1.Distance, 0, 0),
            Scale = 1f
        };

        var item2 = new NeedBasedInputItem
        {
            Target = Entity.Null,
            StatsAdvertised = Advertiser2.AdvertisedValue,
            Position = new float3(Advertiser2.Distance, 0, 0),
            Scale = 1f
        };

        // Call CalculateWeight directly from the Job
        float weight0 = NeedBasedSystem.NeedBasedCalculationJob.CalculateWeight(
            selfTransform, item0, statsComponent, attenuationComponent);
        float weight1 = NeedBasedSystem.NeedBasedCalculationJob.CalculateWeight(
            selfTransform, item1, statsComponent, attenuationComponent);
        float weight2 = NeedBasedSystem.NeedBasedCalculationJob.CalculateWeight(
            selfTransform, item2, statsComponent, attenuationComponent);

        Debug.Log($"  Weight0: {weight0}, Weight1: {weight1}, Weight2: {weight2}");

        // Determine the best advertiser
        float maxWeight = math.max(math.max(weight0, weight1), weight2);
        
        if (maxWeight <= 0)
        {
            return -1; // Idle
        }

        if (weight0 == maxWeight)
        {
            return 0;
        }
        else if (weight1 == maxWeight)
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }

    // Helper methods
    private enum HermiteCurveType
    {
        Linear0,
        Linear1,
        Linear01,
        Linear10,
        Square01,
        Square10
    }

    private HermiteCurve CreateHermiteCurve(HermiteCurveType type)
    {
        switch (type)
        {
            case HermiteCurveType.Linear0:
                // Returns 0 for all values of x
                return new HermiteCurve
                {
                    points = new float4(0f, 0f, 1f, 0f),
                    tangents = new float2(0f, 0f)
                };

            case HermiteCurveType.Linear1:
                // Returns 1 for all values of x
                return new HermiteCurve
                {
                    points = new float4(0f, 1f, 1f, 1f),
                    tangents = new float2(0f, 0f)
                };

            case HermiteCurveType.Linear01:
                // Linear function growing from 0 to 1 with x from 0 to 1
                return new HermiteCurve
                {
                    points = new float4(0f, 0f, 1f, 1f),
                    tangents = new float2(1f, 1f)
                };

            case HermiteCurveType.Linear10:
                // Linear function declining from 1 to 0 with x from 0 to 1
                return new HermiteCurve
                {
                    points = new float4(0f, 1f, 1f, 0f),
                    tangents = new float2(-1f, -1f)
                };

            case HermiteCurveType.Square01:
                // Square function rising from 0 to 1 (convex)
                return new HermiteCurve
                {
                    points = new float4(0f, 0f, 1f, 1f),
                    tangents = new float2(0f, 2f)
                };

            case HermiteCurveType.Square10:
                // Square function falling from 1 to 0 (concave)
                return new HermiteCurve
                {
                    points = new float4(0f, 1f, 1f, 0f),
                    tangents = new float2(-2f, 0f)
                };

            default:
                return new HermiteCurve
                {
                    points = new float4(0f, 0f, 1f, 1f),
                    tangents = new float2(0f, 0f)
                };
        }
    }

    private HermiteCurve4x2 CreateHermiteCurve4x2(HermiteCurveType type)
    {
        var result = new HermiteCurve4x2();
        var curve = CreateHermiteCurve(type);

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                result[i, j] = curve;
            }
        }

        return result;
    }

    private float4x2 CreateFloat4x2(float value)
    {
        return new float4x2(
            new float4(value, value, value, value),
            new float4(value, value, 0f, 0f)
        );
    }

    private AnimalStats CreateAnimalStats(float value)
    {
        var stats = new AnimalStats();
        stats.Stats = new float4x2(
            new float4(value, value, value, value),
            new float4(value, value, 0f, 0f)
        );
        return stats;
    }

    private AnimalStats CreateAnimalStatsCustom(float energy, float fullness, float toilet, float social, float safety, float health)
    {
        var stats = new AnimalStats();
        stats.Stats = new float4x2(
            new float4(energy, fullness, toilet, social),
            new float4(safety, health, 0f, 0f)
        );
        return stats;
    }

    private string GetAdvertiserName(int index)
    {
        switch (index)
        {
            case -1: return "Idle";
            case 0: return "Advertiser0";
            case 1: return "Advertiser1";
            case 2: return "Advertiser2";
            default: return "Unknown";
        }
    }

    private void AssertEquals(int actual, int expected, string testName)
    {
        if (actual != expected)
        {
            Debug.LogError($"FAILED: {testName} - Expected: {expected}, Got: {actual}");
        }
        else
        {
            Debug.Log($"PASSED: {testName}");
        }
    }
}

