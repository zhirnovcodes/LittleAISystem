using LittleAI.Enums;
using Unity.Mathematics;

[System.Serializable]
public struct AnimalStatsAttenuation4x4
{
    // Each column represents 4 stats
    // c0: Energy, Fullness, Toilet, Social
    // c1: Safety, Health, unused, unused
    public AnimalStatsAttenuation c0x; // Energy
    public AnimalStatsAttenuation c0y; // Fullness
    public AnimalStatsAttenuation c0z; // Toilet
    public AnimalStatsAttenuation c0w; // Social
    public AnimalStatsAttenuation c1x; // Safety
    public AnimalStatsAttenuation c1y; // Health

    // Property accessors for clarity
    public AnimalStatsAttenuation Energy
    {
        get => c0x;
        set => c0x = value;
    }

    public AnimalStatsAttenuation Fullness
    {
        get => c0y;
        set => c0y = value;
    }

    public AnimalStatsAttenuation Toilet
    {
        get => c0z;
        set => c0z = value;
    }

    public AnimalStatsAttenuation Social
    {
        get => c0w;
        set => c0w = value;
    }

    public AnimalStatsAttenuation Safety
    {
        get => c1x;
        set => c1x = value;
    }

    public AnimalStatsAttenuation Health
    {
        get => c1y;
        set => c1y = value;
    }

    // Indexer for programmatic access
    public AnimalStatsAttenuation this[int x, int y]
    {
        get
        {
            if (y == 0)
            {
                switch (x)
                {
                    case 0: return c0x;
                    case 1: return c0y;
                    case 2: return c0z;
                    case 3: return c0w;
                    default:
                        UnityEngine.Debug.LogError($"Invalid index [{x},{y}] for AnimalStatsAttenuation4x4");
                        return default;
                }
            }
            else if (y == 1)
            {
                switch (x)
                {
                    case 0: return c1x;
                    case 1: return c1y;
                    default:
                        UnityEngine.Debug.LogError($"Invalid index [{x},{y}] for AnimalStatsAttenuation4x4");
                        return default;
                }
            }
            else
            {
                UnityEngine.Debug.LogError($"Invalid index [{x},{y}] for AnimalStatsAttenuation4x4");
                return default;
            }
        }
        set
        {
            if (y == 0)
            {
                switch (x)
                {
                    case 0: c0x = value; break;
                    case 1: c0y = value; break;
                    case 2: c0z = value; break;
                    case 3: c0w = value; break;
                    default:
                        UnityEngine.Debug.LogError($"Invalid index [{x},{y}] for AnimalStatsAttenuation4x4");
                        break;
                }
            }
            else if (y == 1)
            {
                switch (x)
                {
                    case 0: c1x = value; break;
                    case 1: c1y = value; break;
                    default:
                        UnityEngine.Debug.LogError($"Invalid index [{x},{y}] for AnimalStatsAttenuation4x4");
                        break;
                }
            }
            else
            {
                UnityEngine.Debug.LogError($"Invalid index [{x},{y}] for AnimalStatsAttenuation4x4");
            }
        }
    }
    
    // Indexer for StatType access
    public AnimalStatsAttenuation this[StatType statType]
    {
        get
        {
            switch (statType)
            {
                case StatType.Energy: return c0x;
                case StatType.Fullness: return c0y;
                case StatType.Toilet: return c0z;
                case StatType.Social: return c0w;
                case StatType.Safety: return c1x;
                case StatType.Health: return c1y;
                default:
                    UnityEngine.Debug.LogError($"Invalid StatType {statType} for AnimalStatsAttenuation4x4");
                    return default;
            }
        }
        set
        {
            switch (statType)
            {
                case StatType.Energy: c0x = value; break;
                case StatType.Fullness: c0y = value; break;
                case StatType.Toilet: c0z = value; break;
                case StatType.Social: c0w = value; break;
                case StatType.Safety: c1x = value; break;
                case StatType.Health: c1y = value; break;
                default:
                    UnityEngine.Debug.LogError($"Invalid StatType {statType} for AnimalStatsAttenuation4x4");
                    break;
            }
        }
    }
}

