using LittleAI.Enums;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[System.Serializable]
public class ActionsMapItem
{
    public ActionTypes ActionType;
    public List<SubActionTypes> SubActions;
}

public class ActionsMap : MonoBehaviour
{
    public List<ActionsMapItem> GetActionsMapList()
    {
        return new List<ActionsMapItem> {
            BuildMapItem(ActionTypes.Idle, SubActionTypes.Idle),
            BuildMapItem(ActionTypes.Eat, SubActionTypes.MoveTo, SubActionTypes.Eat )
        };
    }

    protected static ActionsMapItem BuildMapItem(ActionTypes action, params SubActionTypes[] subActions)
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

    public class Baker : Baker<ActionsMap>
    {
        public override void Bake(ActionsMap authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ActionMapInitializeComponent
            {
                Map = authoring
            });
        }
    }
}