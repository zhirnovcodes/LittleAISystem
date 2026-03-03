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
        var genetaliaLookup = system.GetComponentLookup<GenetaliaComponent>(true);

        var animalStatsLookup = system.GetComponentLookup<AnimalStatsComponent>(true);
        var statsIncreaseLookup = system.GetComponentLookup<StatsIncreaseComponent>(true);
        var movingSpeedLookup = system.GetComponentLookup<MovingSpeedComponent>(true);
        var sleepingPlaceLookup = system.GetComponentLookup<SleepingPlaceComponent>(true);
        var reproductionLookup = system.GetComponentLookup<ReproductionComponent>(false);
        var dnaChainLookup = system.GetBufferLookup<DNAChainItem>(true);
        var dnaStorageLookup = system.GetBufferLookup<DNAStorageItem>(false);

        // Initialize list of ISubActionState
        var subActions = new Dictionary<SubActionTypes, ISubActionState>
        {
            { SubActionTypes.Idle, new IdleSubActionState() },
            { SubActionTypes.MoveTo, new WalkToSubActionState(transformLookup, movingSpeedLookup) },
            { SubActionTypes.MoveToTalk, new WalkToTalk(transformLookup, movingSpeedLookup) },
            { SubActionTypes.RunFrom, new RunFrom(transformLookup, movingSpeedLookup) },
            { SubActionTypes.RotateTowards, new RotateTowards(transformLookup, movingSpeedLookup) },
            { SubActionTypes.Eat, new EatSubActionState(transformLookup, edibleLookup, animalStatsLookup, statsIncreaseLookup) },
            { SubActionTypes.MoveInto, new LayDownState(transformLookup, movingSpeedLookup) },
            { SubActionTypes.Sleep, new SleepingState(transformLookup, sleepingPlaceLookup, animalStatsLookup) },
            { SubActionTypes.StumbleUpon, new StumbleUponSubActionState(transformLookup, animalStatsLookup, genetaliaLookup) },
            { SubActionTypes.Communicate, new CommunicateSubActionState(transformLookup, animalStatsLookup, genetaliaLookup, statsIncreaseLookup, dnaChainLookup, dnaStorageLookup, reproductionLookup) }
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