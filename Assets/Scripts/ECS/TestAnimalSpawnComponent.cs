using Unity.Entities;

public struct TestAnimalSpawnComponent : IComponentData
{
    public Entity Prefab;
    public int AnimalsCount;
}

