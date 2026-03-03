using Unity.Entities;

public static class PrefabLibraryExtensions
{
    /// <summary>
    /// Gets a prefab entity from the library that matches the specified condition flags.
    /// Returns Entity.Null if no matching prefab is found.
    /// </summary>
    public static Entity GetPrefab(this DynamicBuffer<PrefabLibraryItem> library, ConditionFlags flags)
    {
        for (int i = 0; i < library.Length; i++)
        {
            if (library[i].Flag == flags)
            {
                return library[i].Prefab;
            }
        }
        
        return Entity.Null;
    }
}

