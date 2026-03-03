using Unity.Mathematics;

public static class AnimalStatsAttenuation4x4Extension
{
    /// <summary>
    /// Calculates how current stats will change after applying stats change, attenuated by distance and normalized
    /// </summary>
    /// <param name="attenuation">The attenuation configuration</param>
    /// <param name="currentStats">Current animal stats</param>
    /// <param name="statsChange">Stats change to apply</param>
    /// <param name="distance">Distance to target</param>
    /// <returns>Attenuated stats change</returns>
    public static AnimalStats GetStatsAttenuated(
        this AnimalStatsAttenuation4x4 attenuation, 
        AnimalStats currentStats, 
        AnimalStats statsChange, 
        float distance)
    {
        float4x2 attenuatedChange;
        
        // Process column 0 (Energy, Fullness, Toilet, Social)
        attenuatedChange.c0 = new float4(
            attenuation.c0x.GetStatAttenuated(currentStats.Stats.c0.x, statsChange.Stats.c0.x, distance),
            attenuation.c0y.GetStatAttenuated(currentStats.Stats.c0.y, statsChange.Stats.c0.y, distance),
            attenuation.c0z.GetStatAttenuated(currentStats.Stats.c0.z, statsChange.Stats.c0.z, distance),
            attenuation.c0w.GetStatAttenuated(currentStats.Stats.c0.w, statsChange.Stats.c0.w, distance)
        );
        
        // Process column 1 (Safety, Health, unused, unused)
        attenuatedChange.c1 = new float4(
            attenuation.c1x.GetStatAttenuated(currentStats.Stats.c1.x, statsChange.Stats.c1.x, distance),
            attenuation.c1y.GetStatAttenuated(currentStats.Stats.c1.y, statsChange.Stats.c1.y, distance),
            0f,
            0f
        );
        
        return new AnimalStats { Stats = attenuatedChange };
    }
}

