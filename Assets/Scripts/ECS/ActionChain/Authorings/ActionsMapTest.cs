using LittleAI.Enums;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;

[System.Serializable]
public class ActionsMapItem
{
    public ActionTypes ActionType;
    public List<SubActionTypes> SubActions;
}

public class ActionsMapTest : ActionMapBase
{
    public override List<ActionsMapItem>GetActionsMapList()
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

    public override Dictionary<SubActionTypes, ISubActionState> ConstructSubActionsStates(SystemBase system)
    {
        var transformLookup = system.GetComponentLookup<LocalTransform>();

        var subActionStates = new Dictionary<SubActionTypes, ISubActionState>
        {
            { SubActionTypes.Idle, new TestIdle() },
            { SubActionTypes.MoveTo, new TestMoveTo(transformLookup) },
            { SubActionTypes.Eat, new TestEat(transformLookup) }
        };

        return subActionStates;
    }

    public class Baker : Baker<ActionsMapTest>
    {
        public override void Bake(ActionsMapTest authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ActionMapInitializeComponent
            {
                Map = authoring
            });
        }
    }
}