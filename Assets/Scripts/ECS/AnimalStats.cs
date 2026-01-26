using System;
using Unity.Mathematics;

[System.Serializable]
public struct AnimalStats
{
    public float4x2 Stats;

    // Matrix where values are:
    // 0 0 - Energy
    // 0 1 - Fullness
    // 0 2 - Toilet
    // 0 3 - Social
    // 1 0 - Safety
    // 1 1 - Health
    // Values may vary from 0 to 100

    // Getters
    public float Energy => Stats.c0.x;
    public float Fullness => Stats.c0.y;
    public float Toilet => Stats.c0.z;
    public float Social => Stats.c0.w;
    public float Safety => Stats.c1.x;
    public float Health => Stats.c1.y;

    // Setters
    public void SetEnergy(float value)
    {
        Stats.c0.x = math.clamp(value, 0f, 100f);
    }

    public void SetFullness(float value)
    {
        Stats.c0.y = math.clamp(value, 0f, 100f);
    }

    public void SetToilet(float value)
    {
        Stats.c0.z = math.clamp(value, 0f, 100f);
    }

    public void SetSocial(float value)
    {
        Stats.c0.w = math.clamp(value, 0f, 100f);
    }

    public void SetSafety(float value)
    {
        Stats.c1.x = math.clamp(value, 0f, 100f);
    }

    public void SetHealth(float value)
    {
        Stats.c1.y = math.clamp(value, 0f, 100f);
    }

    // Method to get total weight (sum of all stats)
    public float GetWeight()
    {
        return Energy + Fullness + Toilet + Social + Safety + Health;
    }

    // Operator overloads
    public static AnimalStats operator +(AnimalStats a, AnimalStats b)
    {
        AnimalStats result = new AnimalStats();
        result.Stats.c0 = a.Stats.c0 + b.Stats.c0;
        result.Stats.c1 = a.Stats.c1 + b.Stats.c1;
        return result;
    }

    public static AnimalStats operator -(AnimalStats a, AnimalStats b)
    {
        AnimalStats result = new AnimalStats();
        result.Stats.c0 = a.Stats.c0 - b.Stats.c0;
        result.Stats.c1 = a.Stats.c1 - b.Stats.c1;
        return result;
    }
}

