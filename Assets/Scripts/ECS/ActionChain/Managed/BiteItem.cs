using Unity.Entities;

public struct BiteItem : IBufferElementData
{
    public Entity Actor;
    public float BittenNutritionValue;
}

