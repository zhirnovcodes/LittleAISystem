using LittleAI.Enums;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
public static class ActionsMapExtentions
{

    private static bool TryGetSubAction(ref BlobArray<int> actions, ActionTypes action, int index, out SubActionTypes subAction, int actionsCount, int subActionsCount)
    {
        var actionIndex = (int)action;
        var subActionIndex = index < 0 || index >= subActionsCount ? -1 : (index + subActionsCount * actionIndex);
        
        if (subActionIndex == -1 || actions[subActionIndex] == -1)
        {
            subAction = SubActionTypes.Idle;
            return false;
        }

        subAction = (SubActionTypes)actions[subActionIndex];
        return true;
    }

    public static bool TryGetSubAction(this ActionChainConfigComponent dto, ActionTypes action, int index, out SubActionTypes subAction)
    {
        return TryGetSubAction(ref dto.BlobReference.Value.ActionsMap, action, index, 
            out subAction, dto.BlobReference.Value.ActionsCount, dto.BlobReference.Value.SubActionsCount);
    }

}
