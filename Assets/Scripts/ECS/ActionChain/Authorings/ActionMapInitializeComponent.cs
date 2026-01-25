using Unity.Entities;

public struct ActionMapInitializeComponent : IComponentData
{
    public UnityObjectRef<ActionsMap> Map;
}
