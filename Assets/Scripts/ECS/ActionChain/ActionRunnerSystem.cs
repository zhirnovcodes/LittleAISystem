using LittleAI.Enums;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class ActionRunnerSystem : SystemBase
{
    private ActionChainConfigComponent ActionsMap;

    private Dictionary<SubActionTypes, ISubActionState> SubActionStates;

    protected override void OnCreate()
    {
        var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>();

        RequireForUpdate<ActionChainConfigComponent>();
        RequireForUpdate<ActionChainItem>();
        RequireForUpdate<ActionRunnerComponent>();

        SubActionStates = new Dictionary<SubActionTypes, ISubActionState>
        {
            { SubActionTypes.Idle, new TestIdle() },
            { SubActionTypes.MoveTo, new TestMoveTo(transformLookup) },
            { SubActionTypes.Eat, new TestEat(transformLookup) }
        };
    }

    protected override void OnUpdate()
    {
        Enabled = false;

        EntityCommandBuffer buffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        ActionsMap = SystemAPI.GetSingleton<ActionChainConfigComponent>();

        foreach (var subActionState in SubActionStates.Values)
        {
            subActionState.Refresh(this);
        }

        var deltaTime = SystemAPI.Time.DeltaTime;

        Entities.ForEach((Entity entity, 
            ref ActionRunnerComponent runner, 
            ref SubActionTimeComponent timer,
            ref DynamicBuffer<ActionChainItem> chain) =>
        {
            // if entity doesnt exist
            // if target doesnt exist
            
            var subActionState = GetState(in runner);

            timer.DeltaTime = deltaTime;
            timer.TimeElapsed += deltaTime;

            var result = subActionState.Update(entity, runner.Target, buffer, timer);

            switch (result.Status)
            {
                case SubActionStatus.Running:
                    return;
                case SubActionStatus.Success:
                    subActionState.Disable(entity, runner.Target, buffer);

                    SetNextSubAction(ref runner, ref chain);

                    var nextState = GetState(in runner);
                    nextState.Enable(entity, runner.Target, buffer);

                    timer.TimeElapsed = 0;
                    return;
                case SubActionStatus.Fail:
                case SubActionStatus.Cancel:
                    subActionState.Disable(entity, runner.Target, buffer);

                    SetNextAction(ref runner, ref chain);

                    var idleState = GetState(in runner);
                    idleState.Enable(entity, runner.Target, buffer);

                    timer.TimeElapsed = 0;
                    return;
            }
        }).WithoutBurst().Run();

        buffer.Playback(EntityManager);
        buffer.Dispose();
    }

    private void SetActionIdle(ref ActionRunnerComponent runner)
    {
        runner.Action = ActionTypes.Idle;
        runner.CurrentSubActionIndex = 0;
        runner.Target = Entity.Null;
    }

    private void SetNextSubAction(ref ActionRunnerComponent runner, ref DynamicBuffer<ActionChainItem> chain)
    {
        runner.CurrentSubActionIndex++;
        
        if (ActionsMap.TryGetSubAction(runner.Action, runner.CurrentSubActionIndex, out var subaction))
        {
            return;
        }

        SetNextAction(ref runner, ref chain);
    }

    private void SetNextAction(ref ActionRunnerComponent runner, ref DynamicBuffer<ActionChainItem> chain)
    {
        if (chain.IsEmpty)
        {
            SetActionIdle(ref runner);
            return;
        }

        var nextAction = chain[0];

        chain.RemoveAt(0);

        runner.Action = nextAction.Action;
        runner.CurrentSubActionIndex = 0;
        runner.Target = nextAction.Target;
    }

    private ISubActionState GetState(in ActionRunnerComponent runner)
    {
        if (ActionsMap.TryGetSubAction(runner.Action, runner.CurrentSubActionIndex, out var subaction))
        {
            return SubActionStates[subaction];
        }

        return SubActionStates[SubActionTypes.Idle];
    }
}
