using Unity.Entities;

// Main BLOB asset structure
[System.Serializable]
public struct ActionsMapSettings
{
    public int ActionsCount;
    public int SubActionsCount;

    public BlobArray<int> ActionsMap;
}