using Unity.Entities;

public struct DNAStorageItem : IBufferElementData
{
    public Entity Father;
    public DNAChainData Data;
}

