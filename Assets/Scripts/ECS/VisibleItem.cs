using Unity.Entities;

// TODO to the same entity as vision
public struct VisibleItem : IBufferElementData
{
    public Entity Target;
    public double TimeAdded;
}

