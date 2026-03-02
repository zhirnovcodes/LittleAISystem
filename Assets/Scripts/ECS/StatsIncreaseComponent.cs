using Unity.Entities;

public struct StatsIncreaseComponent : IComponentData
{
    public AnimalStats AnimalStats; // stat increase per second, when performing increasing action
}

