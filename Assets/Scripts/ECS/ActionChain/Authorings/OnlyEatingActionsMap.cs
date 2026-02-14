using LittleAI.Enums;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;

public class OnlyEatingActionsMap : ActionMapBase
{
    public override Dictionary<SubActionTypes, ISubActionState> ConstructSubActionsStates(SystemBase system)
    {
        var transformLookup = system.GetComponentLookup<LocalTransform>(true);
        var edibleLookup = system.GetComponentLookup<EdibleComponent>(true);
        var animalStatsLookup = system.GetComponentLookup<AnimalStatsComponent>(true);
        var sleepingPlaceLookup = system.GetComponentLookup<SleepingPlaceComponent>(true);

        // Initialize list of ISubActionState
        var subActions = new Dictionary<SubActionTypes, ISubActionState>
        {
            { SubActionTypes.Idle, new IdleSubActionState() },
            { SubActionTypes.MoveTo, new WalkToSubActionState(transformLookup) },
            { SubActionTypes.MoveToTalk, new WalkToTalk(transformLookup) },
            { SubActionTypes.RunFrom, new RunFrom(transformLookup) },
            { SubActionTypes.RotateTowards, new RotateTowards(transformLookup) },
            { SubActionTypes.Eat, new EatSubActionState(transformLookup, edibleLookup, animalStatsLookup) },
            { SubActionTypes.MoveInto, new LayDownState(transformLookup) },
            { SubActionTypes.Sleep, new SleepingState(transformLookup, sleepingPlaceLookup, animalStatsLookup) }
        };

        return subActions;
    }

    public override List<ActionsMapItem> GetActionsMapList()
    {
        return new List<ActionsMapItem> {
            ActionTypes.Idle.   BuildMapItem( SubActionTypes.Idle ),
            ActionTypes.Eat.    BuildMapItem( SubActionTypes.MoveTo, SubActionTypes.RotateTowards, SubActionTypes.Eat ),
            ActionTypes.Sleep.  BuildMapItem( SubActionTypes.MoveTo, SubActionTypes.MoveInto, SubActionTypes.Sleep ),
            ActionTypes.Escape. BuildMapItem( SubActionTypes.RunFrom)
        };
    }

    public class Baker : Baker<OnlyEatingActionsMap>
    {
        public override void Bake(OnlyEatingActionsMap authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ActionMapInitializeComponent
            {
                Map = authoring
            });
        }
    }
}