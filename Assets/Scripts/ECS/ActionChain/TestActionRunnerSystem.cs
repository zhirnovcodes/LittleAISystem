using LittleAI.Enums;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct TestActionRunnerSystem : ISystem
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private Random RandomGenerator;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        TransformLookup = state.GetComponentLookup<LocalTransform>();
        RandomGenerator = Random.CreateFromIndex((uint)System.DateTime.Now.Ticks);

        state.RequireForUpdate<ActionChainConfigComponent>();
        state.RequireForUpdate<ActionChainItem>();
        state.RequireForUpdate<ActionRunnerComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        return;
        TransformLookup.Update(ref state);

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var actionsMap = SystemAPI.GetSingleton<ActionChainConfigComponent>();
        var deltaTime = SystemAPI.Time.DeltaTime;
        var transformLookup = TransformLookup;
        var randomGenerator = RandomGenerator;

        new ActionRunnerJob
        {
            ActionsMap = actionsMap,
            DeltaTime = deltaTime,
            TransformLookup = transformLookup,
            RandomGenerator = randomGenerator,
            Ecb = ecb
        }.Schedule();

        RandomGenerator = randomGenerator;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    private partial struct ActionRunnerJob : IJobEntity
    {
        public ActionChainConfigComponent ActionsMap;
        public float DeltaTime;
        [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
        public Random RandomGenerator;
        public EntityCommandBuffer Ecb;

        private void Execute(Entity entity,
            ref ActionRunnerComponent runner,
            ref SubActionTimeComponent timer,
            ref DynamicBuffer<ActionChainItem> chain)
        {
            if (!ActionsMap.TryGetSubAction(runner.Action, runner.CurrentSubActionIndex, out var currentSubActionType))
            {
                currentSubActionType = SubActionTypes.Idle;
            }

            timer.DeltaTime = DeltaTime;
            timer.TimeElapsed += DeltaTime;

            var result = UpdateSubAction(currentSubActionType, entity, runner.Target, Ecb, timer, TransformLookup, ref RandomGenerator);

            switch (result.Status)
            {
                case SubActionStatus.Running:
                    return;
                case SubActionStatus.Success:
                    DisableSubAction(currentSubActionType, entity, runner.Target, Ecb);

                    SetNextSubAction(ref runner, ref chain);

                    if (!ActionsMap.TryGetSubAction(runner.Action, runner.CurrentSubActionIndex, out var nextSubActionType))
                    {
                        nextSubActionType = SubActionTypes.Idle;
                    }
                    EnableSubAction(nextSubActionType, entity, runner.Target, Ecb);

                    timer.TimeElapsed = 0;
                    return;
                case SubActionStatus.Fail:
                case SubActionStatus.Cancel:
                    DisableSubAction(currentSubActionType, entity, runner.Target, Ecb);

                    SetNextAction(ref runner, ref chain);

                    if (!ActionsMap.TryGetSubAction(runner.Action, runner.CurrentSubActionIndex, out var idleSubActionType))
                    {
                        idleSubActionType = SubActionTypes.Idle;
                    }
                    EnableSubAction(idleSubActionType, entity, runner.Target, Ecb);

                    timer.TimeElapsed = 0;
                    return;
            }
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

        // Enable method with switch for all SubActionTypes
        [BurstCompile]
        private static void EnableSubAction(SubActionTypes subActionType, Entity entity, Entity target, EntityCommandBuffer ecb)
        {
            switch (subActionType)
            {
                case SubActionTypes.Idle:
                    EnableIdle(entity, target, ecb);
                    break;
                case SubActionTypes.MoveTo:
                    EnableMoveTo(entity, target, ecb);
                    break;
                case SubActionTypes.Eat:
                    EnableEat(entity, target, ecb);
                    break;
                case SubActionTypes.Search:
                case SubActionTypes.MoveInto:
                case SubActionTypes.MoveToTalk:
                case SubActionTypes.RunFrom:
                case SubActionTypes.RotateTowards:
                case SubActionTypes.Sleep:
                case SubActionTypes.StumbleUpon:
                case SubActionTypes.Communicate:
                    // Not implemented yet
                    break;
            }
        }

        // Update method with switch for all SubActionTypes
        [BurstCompile]
        private static SubActionResult UpdateSubAction(SubActionTypes subActionType, Entity entity, Entity target,
            EntityCommandBuffer ecb, in SubActionTimeComponent timer, ComponentLookup<LocalTransform> transformLookup,
            ref Random randomGenerator)
        {
            switch (subActionType)
            {
                case SubActionTypes.Idle:
                    return UpdateIdle(entity, target, ecb, timer);
                case SubActionTypes.MoveTo:
                    return UpdateMoveTo(entity, target, ecb, timer, transformLookup, ref randomGenerator);
                case SubActionTypes.Eat:
                    return UpdateEat(entity, target, ecb, timer, transformLookup);
                case SubActionTypes.Search:
                case SubActionTypes.MoveInto:
                case SubActionTypes.MoveToTalk:
                case SubActionTypes.RunFrom:
                case SubActionTypes.RotateTowards:
                case SubActionTypes.Sleep:
                case SubActionTypes.StumbleUpon:
                case SubActionTypes.Communicate:
                    // Not implemented yet
                    return SubActionResult.Success();
                default:
                    return SubActionResult.Fail();
            }
        }

        // Disable method with switch for all SubActionTypes
        [BurstCompile]
        private static void DisableSubAction(SubActionTypes subActionType, Entity entity, Entity target, EntityCommandBuffer ecb)
        {
            switch (subActionType)
            {
                case SubActionTypes.Idle:
                    DisableIdle(entity, target, ecb);
                    break;
                case SubActionTypes.MoveTo:
                    DisableMoveTo(entity, target, ecb);
                    break;
                case SubActionTypes.Eat:
                    DisableEat(entity, target, ecb);
                    break;
                case SubActionTypes.Search:
                case SubActionTypes.MoveInto:
                case SubActionTypes.MoveToTalk:
                case SubActionTypes.RunFrom:
                case SubActionTypes.RotateTowards:
                case SubActionTypes.Sleep:
                case SubActionTypes.StumbleUpon:
                case SubActionTypes.Communicate:
                    // Not implemented yet
                    break;
            }
        }

        // ============= IDLE =============
        [BurstCompile]
        private static void EnableIdle(Entity entity, Entity target, EntityCommandBuffer ecb)
        {
            // Nothing to enable for idle
        }

        [BurstCompile]
        private static SubActionResult UpdateIdle(Entity entity, Entity target, EntityCommandBuffer ecb, in SubActionTimeComponent timer)
        {
            if (timer.TimeElapsed >= 2.0f)
            {
                return SubActionResult.Success();
            }

            return SubActionResult.Running();
        }

        [BurstCompile]
        private static void DisableIdle(Entity entity, Entity target, EntityCommandBuffer ecb)
        {
            // Nothing to disable for idle
        }

        // ============= MOVE TO =============
        private const float MoveSpeed = 5.0f;

        [BurstCompile]
        private static void EnableMoveTo(Entity entity, Entity target, EntityCommandBuffer ecb)
        {
            // Nothing to enable for move
        }

        [BurstCompile]
        private static SubActionResult UpdateMoveTo(Entity entity, Entity target, EntityCommandBuffer ecb,
            in SubActionTimeComponent timer, ComponentLookup<LocalTransform> transformLookup, ref Random randomGenerator)
        {
            // Check if both entities have required components
            if (!transformLookup.HasComponent(entity) || !transformLookup.HasComponent(target))
            {
                return SubActionResult.Fail(1);
            }

            var entityTransform = transformLookup[entity];
            var targetTransform = transformLookup[target];

            var directionToTarget = targetTransform.Position - entityTransform.Position;
            var distanceToTarget = math.length(directionToTarget);

            // Check if reached target
            var reachDistance = entityTransform.Scale / 2f + targetTransform.Scale / 2f;
            if (distanceToTarget <= reachDistance)
            {
                return SubActionResult.Success();
            }

            // Move towards target using transform position
            var normalizedDirection = directionToTarget / distanceToTarget;
            var moveDistance = MoveSpeed * timer.DeltaTime;

            // Clamp movement to not overshoot target
            if (moveDistance > distanceToTarget)
            {
                moveDistance = distanceToTarget;
            }

            var newPosition = entityTransform.Position + normalizedDirection * moveDistance;

            ecb.SetComponent(entity, new LocalTransform
            {
                Position = newPosition,
                Rotation = entityTransform.Rotation,
                Scale = entityTransform.Scale
            });

            return SubActionResult.Running();
        }

        [BurstCompile]
        private static void DisableMoveTo(Entity entity, Entity target, EntityCommandBuffer ecb)
        {
            // Nothing to disable for move
        }

        // ============= EAT =============
        private const float EatDuration = 3.0f;
        private const float RotationSpeed = 5.0f;

        [BurstCompile]
        private static void EnableEat(Entity entity, Entity target, EntityCommandBuffer ecb)
        {
            // Nothing to enable for eat
        }

        [BurstCompile]
        private static SubActionResult UpdateEat(Entity entity, Entity target, EntityCommandBuffer ecb,
            in SubActionTimeComponent timer, ComponentLookup<LocalTransform> transformLookup)
        {
            // Check if both entities have required components
            if (!transformLookup.HasComponent(entity) || !transformLookup.HasComponent(target))
            {
                return SubActionResult.Fail(1);
            }

            var entityTransform = transformLookup[entity];
            var targetTransform = transformLookup[target];

            // Calculate direction to target
            var directionToTarget = targetTransform.Position - entityTransform.Position;

            // Rotate towards target using transform rotation
            if (math.lengthsq(directionToTarget) > 0.001f)
            {
                directionToTarget = math.normalize(directionToTarget);

                // Calculate target rotation to face the target
                var targetRotation = quaternion.LookRotationSafe(directionToTarget, new float3(0, 1, 0));

                // Smoothly interpolate rotation
                var t = math.min(1.0f, RotationSpeed * timer.DeltaTime);
                var newRotation = math.slerp(entityTransform.Rotation, targetRotation, t);

                ecb.SetComponent(entity, new LocalTransform
                {
                    Position = entityTransform.Position,
                    Rotation = newRotation,
                    Scale = entityTransform.Scale
                });
            }

            // Check if eating duration is complete
            if (timer.TimeElapsed >= EatDuration)
            {
                return SubActionResult.Success();
            }

            return SubActionResult.Running();
        }

        [BurstCompile]
        private static void DisableEat(Entity entity, Entity target, EntityCommandBuffer ecb)
        {
            // Nothing to disable for eat
        }
    }
}

