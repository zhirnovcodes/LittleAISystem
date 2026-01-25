using Unity.Entities;
// Component to store the BLOB reference
public struct ActionChainConfigComponent : IComponentData
{
    public BlobAssetReference<ActionsMapSettings> BlobReference;
}
