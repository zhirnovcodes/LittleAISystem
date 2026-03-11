using LittleAI.Enums;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(StatsUpdateSystem))]
public partial class TestSubActionsSystem : SystemBase
{
    private List<ISubActionState> SubActions;

    protected override void OnCreate()
    {
        var transformLookup = GetComponentLookup<LocalTransform>(true);
        var biteLookup = GetBufferLookup<BiteItem>();
        var animalStatsLookup = GetComponentLookup<AnimalStatsComponent>(true);
        var statsIncreaseLookup = GetComponentLookup<StatsIncreaseComponent>(true);
        var movingSpeedLookup = GetComponentLookup<MovingSpeedComponent>(true);
        var sleepingPlaceLookup = GetComponentLookup<SleepingPlaceComponent>(true);
        var moveControllerOutputLookup = GetComponentLookup<MoveControllerOutputComponent>(true);
        var limitationLookup = GetComponentLookup<MoveLimitationComponent>(true);

        // Initialize list of ISubActionState
        SubActions = new List<ISubActionState>
        {
            new IdleSubActionState(transformLookup, movingSpeedLookup, moveControllerOutputLookup, limitationLookup),
            new WalkToSubActionState( movingSpeedLookup, moveControllerOutputLookup),
            new RunFrom(transformLookup, movingSpeedLookup, moveControllerOutputLookup),
            new RotateTowards(transformLookup, movingSpeedLookup, moveControllerOutputLookup),
            new EatSubActionState(transformLookup, biteLookup, animalStatsLookup, statsIncreaseLookup),
            new LayDownState(transformLookup, movingSpeedLookup, moveControllerOutputLookup),
            new SleepingState(transformLookup, sleepingPlaceLookup, animalStatsLookup)
        };
    }

    protected override void OnUpdate()
    {
        // Handle keyboard input for keys 1-9 (applies to all entities)
        int keyboardInput = -2; // -2 means no input, -1 means reset (space), 0-8 means action
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && (i < SubActions.Count))
            {
                keyboardInput = i;
                break;
            }
        }

        // Handle Space key to reset
        if (Input.GetKeyDown(KeyCode.Space))
        {
            keyboardInput = -1;
        }

        var buffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        // Query for all entities with TestSubActionComponent and SubActionTimeComponent
        foreach (var (testComponent, timer, random, entity) in SystemAPI.Query<RefRW<TestSubActionComponent>, 
            RefRW<SubActionTimeComponent>,
            RefRW<ActionRandomComponent>>().WithEntityAccess())
        {
            var currentIndex = testComponent.ValueRO.CurrentSubActionIndex;

            // Apply keyboard input
            if (keyboardInput != -2)
            {
                int previousIndex = currentIndex;
                testComponent.ValueRW.CurrentSubActionIndex = keyboardInput;
                currentIndex = keyboardInput;
                HandleStateChange(entity, previousIndex, currentIndex, testComponent.ValueRO.Target, timer, buffer, ref random.ValueRW.Random);
            }

            // If active, call Refresh and Update
            if (currentIndex == -1)
            {
                continue;
            }

            var subAction = SubActions[currentIndex];
            
            subAction.Refresh(this);

            // Update timer
            float deltaTime = SystemAPI.Time.DeltaTime;
            timer.ValueRW.TimeElapsed += deltaTime;
            timer.ValueRW.DeltaTime = deltaTime;
            
            var result = subAction.Update(entity, testComponent.ValueRO.Target, buffer, timer.ValueRO, ref random.ValueRW.Random);

            // Handle Success or Fail status
            if (result.Status == SubActionStatus.Success)
            {
                Debug.Log($"Entity {entity.Index} SubAction {currentIndex} returned Success after {timer.ValueRO.TimeElapsed:F2}s");
                testComponent.ValueRW.CurrentSubActionIndex = -1;
                timer.ValueRW.TimeElapsed = 0f;
            }
            else if (result.Status == SubActionStatus.Fail)
            {
                Debug.Log($"Entity {entity.Index} SubAction {currentIndex} returned Fail with code {result.FailCode} after {timer.ValueRO.TimeElapsed:F2}s");
                testComponent.ValueRW.CurrentSubActionIndex = -1;
                timer.ValueRW.TimeElapsed = 0f;
            }
        }

        buffer.Playback(EntityManager);
        buffer.Dispose();
    }

    private void HandleStateChange(Entity entity, int previousIndex, int currentIndex, Entity target, RefRW<SubActionTimeComponent> timer, EntityCommandBuffer buffer, ref Unity.Mathematics.Random random)
    {
        timer.ValueRW.TimeElapsed = 0f;

        // Disable previous state
        if (previousIndex >= 0)
        {
            SubActions[previousIndex].Disable(entity, target, buffer);
            Debug.Log($"Entity {entity.Index} Disabled SubAction {previousIndex}");
        }

        // Enable next state
        if (currentIndex >= 0)
        {
            SubActions[currentIndex].Enable(entity, target, buffer, ref random);
            Debug.Log($"Entity {entity.Index} Enabled SubAction {currentIndex}");
        }
    }
}

