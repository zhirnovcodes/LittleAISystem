using LittleAI.Enums;
using Unity.Entities;

public struct ActionRunnerComponent : IComponentData
{
    public Entity Target;
    public ActionTypes Action;
    public int CurrentSubActionIndex;
    public bool IsCancellationRequested;
}
