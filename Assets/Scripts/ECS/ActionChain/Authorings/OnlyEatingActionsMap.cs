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
        var femaleGenetaliaLookup = system.GetComponentLookup<FemaleGenetaliaComponent>(true);
        var maleGenetaliaLookup = system.GetComponentLookup<MaleGenetaliaComponent>(true);
        var movingDataLookup = system.GetComponentLookup<MovingDataComponent>(true);
        var talkingDataLookup = system.GetComponentLookup<TalkingDataComponent>(true);
        var sleepDataLookup = system.GetComponentLookup<SleepDataComponent>(true);
        var eatDataLookup = system.GetComponentLookup<EatDataComponent>(true);
        var safetyDistanceLookup = system.GetComponentLookup<SafetyDistanceComponent>(true);

        // Initialize list of ISubActionState
        var subActions = new Dictionary<SubActionTypes, ISubActionState>
        {
            { SubActionTypes.Idle, new IdleSubActionState(movingDataLookup) },
            { SubActionTypes.MoveTo, new WalkToSubActionState(transformLookup, movingDataLookup) },
            { SubActionTypes.MoveToTalk, new WalkToTalk(transformLookup, talkingDataLookup, movingDataLookup) },
            { SubActionTypes.RunFrom, new RunFrom(transformLookup, movingDataLookup, safetyDistanceLookup) },
            { SubActionTypes.RotateTowards, new RotateTowards(transformLookup, movingDataLookup) },
            { SubActionTypes.Eat, new EatSubActionState(transformLookup, edibleLookup, animalStatsLookup, eatDataLookup) },
            { SubActionTypes.MoveInto, new LayDownState(transformLookup, movingDataLookup, sleepDataLookup) },
            { SubActionTypes.Sleep, new SleepingState(transformLookup, sleepingPlaceLookup, animalStatsLookup, sleepDataLookup) },
            { SubActionTypes.StumbleUpon, new StumbleUponSubActionState(transformLookup, animalStatsLookup, femaleGenetaliaLookup, maleGenetaliaLookup, talkingDataLookup) },
            { SubActionTypes.Communicate, new CommunicateSubActionState(transformLookup, animalStatsLookup, femaleGenetaliaLookup, maleGenetaliaLookup, talkingDataLookup) }
        };

        return subActions;
    }

    public override List<ActionsMapItem> GetActionsMapList()
    {
        return new List<ActionsMapItem> {
            ActionTypes.Idle.           BuildMapItem( SubActionTypes.Idle ),
            ActionTypes.Eat.            BuildMapItem( SubActionTypes.MoveTo, SubActionTypes.RotateTowards, SubActionTypes.Eat ),
            ActionTypes.Sleep.          BuildMapItem( SubActionTypes.MoveTo, SubActionTypes.MoveInto, SubActionTypes.Sleep ),
            ActionTypes.Escape.         BuildMapItem( SubActionTypes.RunFrom ),
            ActionTypes.Communicate.    BuildMapItem( SubActionTypes.MoveToTalk, SubActionTypes.RotateTowards, SubActionTypes.StumbleUpon, SubActionTypes.Communicate, SubActionTypes.RunFrom )
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