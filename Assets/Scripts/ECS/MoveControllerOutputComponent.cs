using Unity.Entities;

public struct MoveControllerOutputComponent : IComponentData
{
    public bool HasArrived;
    public bool IsLookingAt;
}

