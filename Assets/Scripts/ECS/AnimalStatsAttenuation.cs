using Unity.Mathematics;

[System.Serializable]
public struct AnimalStatsAttenuation
{
    public HermiteCurve4x2 NeedsAttenuation;
    public HermiteCurve4x2 DistanceAttenuation;
    public float4x2 MaxDistance;

    // Matrix layout (matching AnimalStats):
    // c0.x (0,0) - Energy
    // c0.y (0,1) - Fullness
    // c0.z (0,2) - Toilet
    // c0.w (0,3) - Social
    // c1.x (1,0) - Safety
    // c1.y (1,1) - Health

    // Getters for NeedsAttenuation
    public HermiteCurve EnergyNeedsAttenuation
    {
        get => NeedsAttenuation.Value00;
        set => NeedsAttenuation.Value00 = value;
    }

    public HermiteCurve FullnessNeedsAttenuation
    {
        get => NeedsAttenuation.Value01;
        set => NeedsAttenuation.Value01 = value;
    }

    public HermiteCurve ToiletNeedsAttenuation
    {
        get => NeedsAttenuation.Value10;
        set => NeedsAttenuation.Value10 = value;
    }

    public HermiteCurve SocialNeedsAttenuation
    {
        get => NeedsAttenuation.Value11;
        set => NeedsAttenuation.Value11 = value;
    }

    public HermiteCurve SafetyNeedsAttenuation
    {
        get => NeedsAttenuation.Value20;
        set => NeedsAttenuation.Value20 = value;
    }

    public HermiteCurve HealthNeedsAttenuation
    {
        get => NeedsAttenuation.Value21;
        set => NeedsAttenuation.Value21 = value;
    }

    // Getters for DistanceAttenuation
    public HermiteCurve EnergyDistanceAttenuation
    {
        get => DistanceAttenuation.Value00;
        set => DistanceAttenuation.Value00 = value;
    }

    public HermiteCurve FullnessDistanceAttenuation
    {
        get => DistanceAttenuation.Value01;
        set => DistanceAttenuation.Value01 = value;
    }

    public HermiteCurve ToiletDistanceAttenuation
    {
        get => DistanceAttenuation.Value10;
        set => DistanceAttenuation.Value10 = value;
    }

    public HermiteCurve SocialDistanceAttenuation
    {
        get => DistanceAttenuation.Value11;
        set => DistanceAttenuation.Value11 = value;
    }

    public HermiteCurve SafetyDistanceAttenuation
    {
        get => DistanceAttenuation.Value20;
        set => DistanceAttenuation.Value20 = value;
    }

    public HermiteCurve HealthDistanceAttenuation
    {
        get => DistanceAttenuation.Value21;
        set => DistanceAttenuation.Value21 = value;
    }

    // Getters/Setters for MaxDistance
    public float EnergyMaxDistance
    {
        get => MaxDistance.c0.x;
        set => MaxDistance.c0.x = value;
    }

    public float FullnessMaxDistance
    {
        get => MaxDistance.c0.y;
        set => MaxDistance.c0.y = value;
    }

    public float ToiletMaxDistance
    {
        get => MaxDistance.c0.z;
        set => MaxDistance.c0.z = value;
    }

    public float SocialMaxDistance
    {
        get => MaxDistance.c0.w;
        set => MaxDistance.c0.w = value;
    }

    public float SafetyMaxDistance
    {
        get => MaxDistance.c1.x;
        set => MaxDistance.c1.x = value;
    }

    public float HealthMaxDistance
    {
        get => MaxDistance.c1.y;
        set => MaxDistance.c1.y = value;
    }
}

