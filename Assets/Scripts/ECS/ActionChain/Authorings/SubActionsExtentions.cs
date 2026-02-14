using LittleAI.Enums;
using System.Collections.Generic;

public static class SubActionsExtentions
{
    public static ActionsMapItem BuildMapItem(this ActionTypes action, params SubActionTypes[] subActions)
    {
        var result = new ActionsMapItem
        {
            ActionType = action,
            SubActions = new List<SubActionTypes>()
        };

        foreach (var sub in subActions)
        {
            result.SubActions.Add(sub);
        }

        return result;
    }
}