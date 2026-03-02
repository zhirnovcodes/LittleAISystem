using LittleAI.Enums;
using Unity.Mathematics;

[System.Serializable]
public struct AnimalStatsAttenuation
{
    public HermiteCurve4x2 NeedsAttenuation;
    public HermiteCurve4x2 DistanceAttenuation;

    // Matrix layout (matching AnimalStats):
    // c0.x - Energy
    // c0.y - Fullness
    // c0.z - Toilet
    // c0.w - Social
    // c1.x - Safety
    // c1.y - Health

    // Getters for NeedsAttenuation
    public HermiteCurve EnergyNeedsAttenuation
    {
        get => NeedsAttenuation.c0.x;
        set => NeedsAttenuation.c0.x = value;
    }

    public HermiteCurve FullnessNeedsAttenuation
    {
        get => NeedsAttenuation.c0.y;
        set => NeedsAttenuation.c0.y = value;
    }

    public HermiteCurve ToiletNeedsAttenuation
    {
        get => NeedsAttenuation.c0.z;
        set => NeedsAttenuation.c0.z = value;
    }

    public HermiteCurve SocialNeedsAttenuation
    {
        get => NeedsAttenuation.c0.w;
        set => NeedsAttenuation.c0.w = value;
    }

    public HermiteCurve SafetyNeedsAttenuation
    {
        get => NeedsAttenuation.c1.x;
        set => NeedsAttenuation.c1.x = value;
    }

    public HermiteCurve HealthNeedsAttenuation
    {
        get => NeedsAttenuation.c1.y;
        set => NeedsAttenuation.c1.y = value;
    }

    // Getters for DistanceAttenuation
    public HermiteCurve EnergyDistanceAttenuation
    {
        get => DistanceAttenuation.c0.x;
        set => DistanceAttenuation.c0.x = value;
    }

    public HermiteCurve FullnessDistanceAttenuation
    {
        get => DistanceAttenuation.c0.y;
        set => DistanceAttenuation.c0.y = value;
    }

    public HermiteCurve ToiletDistanceAttenuation
    {
        get => DistanceAttenuation.c0.z;
        set => DistanceAttenuation.c0.z = value;
    }

    public HermiteCurve SocialDistanceAttenuation
    {
        get => DistanceAttenuation.c0.w;
        set => DistanceAttenuation.c0.w = value;
    }

    public HermiteCurve SafetyDistanceAttenuation
    {
        get => DistanceAttenuation.c1.x;
        set => DistanceAttenuation.c1.x = value;
    }

    public HermiteCurve HealthDistanceAttenuation
    {
        get => DistanceAttenuation.c1.y;
        set => DistanceAttenuation.c1.y = value;
    }

    // Helper methods
    public HermiteCurve GetNeedsCurve(StatType statType)
    {
        return statType switch
        {
            StatType.Energy => EnergyNeedsAttenuation,
            StatType.Fullness => FullnessNeedsAttenuation,
            StatType.Toilet => ToiletNeedsAttenuation,
            StatType.Social => SocialNeedsAttenuation,
            StatType.Safety => SafetyNeedsAttenuation,
            StatType.Health => HealthNeedsAttenuation,
            _ => default
        };
    }

    public HermiteCurve GetDistanceCurve(StatType statType)
    {
        return statType switch
        {
            StatType.Energy => EnergyDistanceAttenuation,
            StatType.Fullness => FullnessDistanceAttenuation,
            StatType.Toilet => ToiletDistanceAttenuation,
            StatType.Social => SocialDistanceAttenuation,
            StatType.Safety => SafetyDistanceAttenuation,
            StatType.Health => HealthDistanceAttenuation,
            _ => default
        };
    }

    public void SetNeedsCurve(StatType statType, HermiteCurve curve)
    {
        switch (statType)
        {
            case StatType.Energy: EnergyNeedsAttenuation = curve; break;
            case StatType.Fullness: FullnessNeedsAttenuation = curve; break;
            case StatType.Toilet: ToiletNeedsAttenuation = curve; break;
            case StatType.Social: SocialNeedsAttenuation = curve; break;
            case StatType.Safety: SafetyNeedsAttenuation = curve; break;
            case StatType.Health: HealthNeedsAttenuation = curve; break;
        }
    }

    public void SetDistanceCurve(StatType statType, HermiteCurve curve)
    {
        switch (statType)
        {
            case StatType.Energy: EnergyDistanceAttenuation = curve; break;
            case StatType.Fullness: FullnessDistanceAttenuation = curve; break;
            case StatType.Toilet: ToiletDistanceAttenuation = curve; break;
            case StatType.Social: SocialDistanceAttenuation = curve; break;
            case StatType.Safety: SafetyDistanceAttenuation = curve; break;
            case StatType.Health: HealthDistanceAttenuation = curve; break;
        }
    }
}

