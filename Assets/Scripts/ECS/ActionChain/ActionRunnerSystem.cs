using LittleAI.Enums;
using System.Collections.Generic;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class ActionRunnerSystem : SystemBase
{
    private ActionChainConfigComponent ActionsMap;

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
        Initialize();

        EntityCommandBuffer buffer = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        ActionsMap = SystemAPI.GetSingleton<ActionChainConfigComponent>();

        RefreshAll();

        var deltaTime = SystemAPI.Time.DeltaTime;

        Entities.ForEach((Entity entity,
            ref ActionRunnerComponent runner,
            ref SubActionTimeComponent timer,
            ref ActionRandomComponent randomComponent,
            ref DynamicBuffer<ActionChainItem> chain) =>
        {
            if (runner.Action == ActionTypes.None)
            {
                SetActionIdle(ref runner);

                EnableState(entity, buffer, in runner, ref randomComponent);
            }

            timer.DeltaTime = deltaTime;
            timer.TimeElapsed += deltaTime;

            var status = runner.IsCancellationRequested ? SubActionStatus.Cancel :
                UpdateState(entity, buffer, runner, ref randomComponent, timer).Status;

            switch (status)
            {
                case SubActionStatus.Running:
                    break;
                case SubActionStatus.Success:
                    DisableState(entity, buffer, in runner);

                    SetNextSubAction(ref runner, ref chain);

                    timer.TimeElapsed = 0;

                    EnableState(entity, buffer, in runner, ref randomComponent);
                    break;
                case SubActionStatus.Fail:
                case SubActionStatus.Cancel:
                    DisableState(entity, buffer, in runner);

                    runner.IsCancellationRequested = false;
                    SetNextAction(ref runner, ref chain);

                    timer.TimeElapsed = 0;

                    EnableState(entity, buffer, in runner, ref randomComponent);
                    break;
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

#region WithoutBurst

    private Dictionary<SubActionTypes, ISubActionState> SubActionStates;

    private void Initialize()
    {
        if (AreSubActionsInitialized)
        {
            return;
        }

        var initializeComponent = SystemAPI.GetSingleton<ActionMapInitializeComponent>();
        SubActionStates = initializeComponent.Map.Value.ConstructSubActionsStates(this);
        AreSubActionsInitialized = true;
    }

    private void RefreshAll()
    {
        foreach (var subActionState in SubActionStates.Values)
        {
            subActionState.Refresh(this);
        }
    }

    private ISubActionState GetState(in ActionRunnerComponent runner)
    {
        if (ActionsMap.TryGetSubAction(runner.Action, runner.CurrentSubActionIndex, out var subaction))
        {
            return SubActionStates[subaction];
        }

        return SubActionStates[SubActionTypes.Idle];
    }

    private void DisableState(Entity entity, EntityCommandBuffer buffer, in ActionRunnerComponent runner)
    {
        //UnityEngine.Debug.Log("Disable " + entity + " " + runner.Action + " " + runner.CurrentSubActionIndex);

        var subActionState = GetState(in runner);
        subActionState.Disable(entity, runner.Target, buffer);
    }

    private void EnableState(Entity entity, EntityCommandBuffer buffer, in ActionRunnerComponent runner, ref ActionRandomComponent randomComponent)
    {
        //UnityEngine.Debug.Log("Enable " + entity + " " + runner.Action + " " + runner.CurrentSubActionIndex);

        var nextState = GetState(in runner);
        nextState.Enable(entity, runner.Target, buffer, ref randomComponent.Random);
    }

    private SubActionResult UpdateState(Entity entity, EntityCommandBuffer buffer, in ActionRunnerComponent runner, ref ActionRandomComponent randomComponent, in SubActionTimeComponent timer)
    {
        var subActionState = GetState(in runner);
        var state = subActionState.Update(entity, runner.Target, buffer, timer, ref randomComponent.Random);

        //UnityEngine.Debug.Log("Update " + entity + " " + runner.Action + " " + runner.CurrentSubActionIndex + " " + state.Status + " " + state.FailCode);

        return state;
    }
    #endregion
}
