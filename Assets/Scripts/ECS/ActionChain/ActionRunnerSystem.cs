using LittleAI.Enums;
using System.Collections.Generic;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class ActionRunnerSystem : SystemBase
{
    private ActionChainConfigComponent ActionsMap;

    private Dictionary<SubActionTypes, ISubActionState> SubActionStates;

    private bool AreSubActionsInitialized;

    protected override void OnCreate()
    {
        RequireForUpdate<ActionChainConfigComponent>();
        RequireForUpdate<ActionChainItem>();
        RequireForUpdate<ActionRunnerComponent>();
        RequireForUpdate<ActionMapInitializeComponent>();
    }

    protected override void OnUpdate()
    {
        if (AreSubActionsInitialized == false)
        {
            var initializeComponent = SystemAPI.GetSingleton<ActionMapInitializeComponent>();
            SubActionStates = initializeComponent.Map.Value.ConstructSubActionsStates(this);
            AreSubActionsInitialized = true;
        }

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

            var status = runner.IsCancellationRequested ? SubActionStatus.Cancel : subActionState.Update(entity, runner.Target, buffer, timer).Status;

            switch (status)
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

                    runner.IsCancellationRequested = false;
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
