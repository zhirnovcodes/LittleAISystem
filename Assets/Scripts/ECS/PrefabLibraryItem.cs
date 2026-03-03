using Unity.Entities;

public struct PrefabLibraryItem : IBufferElementData
{
    public ConditionFlags Flag;
    public Entity Prefab;
}

