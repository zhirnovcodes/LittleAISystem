using LittleAI.Enums;
using Unity.Entities;

public struct ActionChainItem : IBufferElementData
{
    public Entity Target;
    public ActionTypes Action;
}
