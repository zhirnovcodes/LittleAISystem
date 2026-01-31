using LittleAI.Enums;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class TestSubActionsSystem : SystemBase
{
    private List<ISubActionState> SubActions;
    private int PreviousSubActionIndex = -1;

    protected override void OnCreate()
    {
        RequireForUpdate<TestSubActionComponent>();

        // Initialize empty list of ISubActionState (can be populated later)
        SubActions = new List<ISubActionState>();
    }

    protected override void OnUpdate()
    {
        var testComponent = SystemAPI.GetSingletonRW<TestSubActionComponent>();
        var currentIndex = testComponent.ValueRO.CurrentSubActionIndex;

        // Handle keyboard input for keys 1-9
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) 
                && (i < SubActions.Count))
            {
                PreviousSubActionIndex = currentIndex;
                testComponent.ValueRW.CurrentSubActionIndex = i;
                currentIndex = i;
                HandleStateChange(ref testComponent);
                break;
            }
        }

        // Handle Space key to reset
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PreviousSubActionIndex = currentIndex;
            testComponent.ValueRW.CurrentSubActionIndex = -1;
            currentIndex = -1;
            HandleStateChange(ref testComponent);
        }

        // If active, call Refresh and Update
        if (currentIndex == -1)
        {
            return;
        }

        var subAction = SubActions[currentIndex];
        var buffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        
        subAction.Refresh(this);

        // Create a dummy timer for testing
        var timer = new SubActionTimeComponent { TimeElapsed = 0, DeltaTime = SystemAPI.Time.DeltaTime };
        
        var result = subAction.Update(Entity.Null, testComponent.ValueRO.Target, buffer, timer);

        buffer.Playback(EntityManager);
        buffer.Dispose();

        // Handle Success or Fail status
        if (result.Status == SubActionStatus.Success)
        {
            Debug.Log($"SubAction {currentIndex} returned Success");
            testComponent.ValueRW.CurrentSubActionIndex = -1;
        }
        else if (result.Status == SubActionStatus.Fail)
        {
            Debug.Log($"SubAction {currentIndex} returned Fail with code {result.FailCode}");
            testComponent.ValueRW.CurrentSubActionIndex = -1;
        }
    }

    private void HandleStateChange(ref RefRW<TestSubActionComponent> testComponent)
    {
        var buffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var currentIndex = testComponent.ValueRO.CurrentSubActionIndex;
        var target = testComponent.ValueRO.Target;

        // Disable previous state
        if (PreviousSubActionIndex >= 0)
        {
            SubActions[PreviousSubActionIndex].Disable(Entity.Null, target, buffer);
            Debug.Log($"Disabled SubAction {PreviousSubActionIndex}");
        }

        // Enable next state
        if (currentIndex >= 0)
        {
            SubActions[currentIndex].Enable(Entity.Null, target, buffer);
            Debug.Log($"Enabled SubAction {currentIndex}");
        }

        buffer.Playback(EntityManager);
        buffer.Dispose();
    }
}

