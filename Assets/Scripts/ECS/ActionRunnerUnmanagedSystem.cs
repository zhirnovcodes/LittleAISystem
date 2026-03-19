using LittleAI.Enums;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[BurstCompile]
public partial struct ActionRunnerUnmanagedSystem : ISystem
{
    private EntityQuery actionRunnerQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        actionRunnerQuery = SystemAPI.QueryBuilder()
            .WithAll<ActionRunnerComponent, SubActionTimeComponent, ActionRandomComponent, ActionChainItem>()
            .Build();

        state.RequireForUpdate<ActionChainConfigComponent>();
        state.RequireForUpdate<ActionChainUnmanagedTag>();
        state.RequireForUpdate<ActionMapInitializeComponent>();
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate(actionRunnerQuery);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        const int entitiesPerBatch = 1000;
        var actionRunnerCount = actionRunnerQuery.CalculateEntityCount();
        var batchesCount = math.max(1, (actionRunnerCount + entitiesPerBatch - 1) / entitiesPerBatch);

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var buffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var job = new ActionRunnerJob
        {
            ActionsMap = SystemAPI.GetSingleton<ActionChainConfigComponent>(),
            DeltaTime = SystemAPI.Time.DeltaTime,
            Buffer = buffer,
            BatchesCount = batchesCount,

            AnimalStatsLookup = SystemAPI.GetComponentLookup<AnimalStatsComponent>(true),
            BiteLookup = SystemAPI.GetBufferLookup<BiteItem>(true),
            DNAChainLookup = SystemAPI.GetBufferLookup<DNAChainItem>(true),
            DNAStorageLookup = SystemAPI.GetBufferLookup<DNAStorageItem>(true),
            GenetaliaLookup = SystemAPI.GetComponentLookup<GenetaliaComponent>(true),
            TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            LimitationComponent = SystemAPI.GetComponentLookup<MoveLimitationComponent>(true),
            MovingSpeedLookup = SystemAPI.GetComponentLookup<MovingSpeedComponent>(true),
            ReproductionLookup = SystemAPI.GetComponentLookup<ReproductionComponent>(true),
            SleepingPlaceLookup = SystemAPI.GetComponentLookup<SleepingPlaceComponent>(true),
            StatsIncreaseLookup = SystemAPI.GetComponentLookup<StatsIncreaseComponent>(true),

            InputComponent = SystemAPI.GetComponentLookup<MoveControllerInputComponent>(false),
            StatChangeLookup = SystemAPI.GetBufferLookup<StatsChangeItem>(false),
        };

        state.Dependency = job.Schedule(state.Dependency);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}

[BurstCompile]
public partial struct ActionRunnerJob : IJobEntity
{
    public ActionChainConfigComponent ActionsMap;
    public float DeltaTime;

    public EntityCommandBuffer Buffer;

    public int BatchesCount;

    public ComponentLookup<MoveControllerInputComponent> InputComponent;
    public BufferLookup<StatsChangeItem> StatChangeLookup;

    [ReadOnly] public ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    [ReadOnly] public BufferLookup<BiteItem> BiteLookup;
    [ReadOnly] public BufferLookup<DNAChainItem> DNAChainLookup;
    [ReadOnly] public BufferLookup<DNAStorageItem> DNAStorageLookup;
    [ReadOnly] public ComponentLookup<GenetaliaComponent> GenetaliaLookup;
    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
    [ReadOnly] public ComponentLookup<MoveLimitationComponent> LimitationComponent;
    [ReadOnly] public ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;
    [ReadOnly] public ComponentLookup<ReproductionComponent> ReproductionLookup;
    [ReadOnly] public ComponentLookup<SleepingPlaceComponent> SleepingPlaceLookup;
    [ReadOnly] public ComponentLookup<StatsIncreaseComponent> StatsIncreaseLookup;

    public void Execute(
        Entity entity,
        ref ActionRunnerComponent runner,
        ref SubActionTimeComponent timer,
        ref ActionRandomComponent randomComponent,
        ref DynamicBuffer<ActionChainItem> chain)
    {
        timer.DeltaTime += DeltaTime;
        timer.TimeElapsed += DeltaTime;

        if (runner.Action == ActionTypes.None)
        {
            SetActionIdle(ref runner);
            EnableState(entity, in runner, ref randomComponent);
        }


        bool shouldUpdate = timer.FramesElapsed % (float)BatchesCount == entity.Index % (float)BatchesCount ||
            timer.FramesElapsed == 0;

        timer.FramesElapsed++;

        if (shouldUpdate == false)
        {
            return;
        }

        var status = runner.IsCancellationRequested ? SubActionStatus.Cancel :
            UpdateState(entity, in runner, ref randomComponent, in timer).Status;

        switch (status)
        {
            case SubActionStatus.Running:
                break;
            case SubActionStatus.Success:
                DisableState(entity, in runner);
                SetNextSubAction(ref runner, ref chain);
                timer.TimeElapsed = 0;
                EnableState(entity, in runner, ref randomComponent);
                break;
            case SubActionStatus.Fail:
            case SubActionStatus.Cancel:
                DisableState(entity, in runner);
                runner.IsCancellationRequested = false;
                SetNextAction(ref runner, ref chain);
                timer.TimeElapsed = 0;
                EnableState(entity, in runner, ref randomComponent);
                break;
        }

        timer.DeltaTime = 0;
    }

    // =========================================================================
    // Action flow control
    // =========================================================================

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

    // =========================================================================
    // DisableState
    // =========================================================================

    private void DisableState(Entity entity, in ActionRunnerComponent runner)
    {
        if (ActionsMap.TryGetSubAction(runner.Action, runner.CurrentSubActionIndex, out var subaction) == false)
        {
            return;
        }

        switch (subaction)
        {
            case SubActionTypes.Idle:
                Disable_Idle(entity, runner.Target, Buffer);
                break;
            case SubActionTypes.MoveTo:
                Disable_MoveTo(entity, runner.Target, Buffer);
                break;
            case SubActionTypes.Eat:
                Disable_Eat(entity, runner.Target, Buffer);
                break;
            case SubActionTypes.MoveInto:
                Disable_MoveInto(entity, runner.Target, Buffer);
                break;
            case SubActionTypes.Sleep:
                Disable_Sleep(entity, runner.Target, Buffer);
                break;
            case SubActionTypes.RunFrom:
                Disable_RunFrom(entity, runner.Target, Buffer);
                break;
            case SubActionTypes.StumbleUpon:
                Disable_StumbleUpon(entity, runner.Target, Buffer);
                break;
            case SubActionTypes.Communicate:
                Disable_Communicate(entity, runner.Target, Buffer);
                break;
        }
    }

    // =========================================================================
    // EnableState
    // =========================================================================

    private void EnableState(Entity entity, in ActionRunnerComponent runner, ref ActionRandomComponent randomComponent)
    {
        if (ActionsMap.TryGetSubAction(runner.Action, runner.CurrentSubActionIndex, out var subaction) == false)
        {
            return;
        }

        switch (subaction)
        {
            case SubActionTypes.Idle:
                Enable_Idle(entity, runner.Target, Buffer, ref randomComponent.Random);
                break;
            case SubActionTypes.MoveTo:
                Enable_MoveTo(entity, runner.Target, Buffer, ref randomComponent.Random);
                break;
            case SubActionTypes.Eat:
                Enable_Eat(entity, runner.Target, Buffer, ref randomComponent.Random);
                break;
            case SubActionTypes.MoveInto:
                Enable_MoveInto(entity, runner.Target, Buffer, ref randomComponent.Random);
                break;
            case SubActionTypes.Sleep:
                Enable_Sleep(entity, runner.Target, Buffer, ref randomComponent.Random);
                break;
            case SubActionTypes.RunFrom:
                Enable_RunFrom(entity, runner.Target, Buffer, ref randomComponent.Random);
                break;
            case SubActionTypes.StumbleUpon:
                Enable_StumbleUpon(entity, runner.Target, Buffer, ref randomComponent.Random);
                break;
            case SubActionTypes.Communicate:
                Enable_Communicate(entity, runner.Target, Buffer, ref randomComponent.Random);
                break;
        }
    }

    // =========================================================================
    // UpdateState
    // =========================================================================

    private SubActionResult UpdateState(Entity entity, in ActionRunnerComponent runner, ref ActionRandomComponent randomComponent, in SubActionTimeComponent timer)
    {
        if (ActionsMap.TryGetSubAction(runner.Action, runner.CurrentSubActionIndex, out var subaction) == false)
        {
            return SubActionResult.Fail(-1);
        }

        switch (subaction)
        {
            case SubActionTypes.Idle:
                return Update_Idle(entity, runner.Target, Buffer, in timer, ref randomComponent.Random);
            case SubActionTypes.MoveTo:
                return Update_MoveTo(entity, runner.Target, Buffer, in timer, ref randomComponent.Random);
            case SubActionTypes.Eat:
                return Update_Eat(entity, runner.Target, Buffer, in timer, ref randomComponent.Random);
            case SubActionTypes.MoveInto:
                return Update_MoveInto(entity, runner.Target, Buffer, in timer, ref randomComponent.Random);
            case SubActionTypes.Sleep:
                return Update_Sleep(entity, runner.Target, Buffer, in timer, ref randomComponent.Random);
            case SubActionTypes.RunFrom:
                return Update_RunFrom(entity, runner.Target, Buffer, in timer, ref randomComponent.Random);
            case SubActionTypes.StumbleUpon:
                return Update_StumbleUpon(entity, runner.Target, Buffer, in timer, ref randomComponent.Random);
            case SubActionTypes.Communicate:
                return Update_Communicate(entity, runner.Target, Buffer, in timer, ref randomComponent.Random);
            default:
                return SubActionResult.Fail(-1);
        }
    }

    // =========================================================================
    // Idle
    // =========================================================================

    public void Enable_Idle(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        if (!TransformLookup.HasComponent(entity) || !MovingSpeedLookup.HasComponent(entity))
        {
            return;
        }

        var entityTransform = TransformLookup[entity];
        var movingSpeed = MovingSpeedLookup[entity];

        var radius = random.NextFloat(SubActionConsts.Idle.WanderRadius / 2f, SubActionConsts.Idle.WanderRadius);
        float3 targetPosition;

        if (LimitationComponent.TryGetComponent(entity, out var limitation))
        {
            targetPosition = LocalTransformExtensions.GenerateRandomPosition(limitation.Central, limitation.Scale, ref random);
        }
        else
        {
            targetPosition = LocalTransformExtensions.GenerateRandomPosition(entityTransform.Position, radius, ref random);
        }

        var lookDirection = math.normalize(targetPosition - entityTransform.Position);
        var speed = movingSpeed.GetWalkingSpeed() * SubActionConsts.Idle.SpeedMultiplier;
        var rotationSpeed = movingSpeed.GetWalkingRotationSpeed() * SubActionConsts.Idle.SpeedMultiplier;

        InputComponent.Enable(entity);
        InputComponent.SetTarget(entity, targetPosition, 0, lookDirection, 0.01f, speed, rotationSpeed);
    }

    public void Disable_Idle(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        InputComponent.ResetInput(entity);
    }

    public SubActionResult Update_Idle(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        var time = random.NextFloat(SubActionConsts.Idle.IdleTime / 2f, SubActionConsts.Idle.IdleTime);
        if (timer.IsTimeout(time))
        {
            return SubActionResult.Success();
        }

        if (TransformLookup.TryGetComponent(entity, out var entityTransform) &&
            InputComponent.TryGetComponent(entity, out var moveInput) &&
            entityTransform.IsTargetDistanceReached(moveInput.TargetPosition, moveInput.TargetScale, moveInput.Distance))
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }

    // =========================================================================
    // MoveTo
    // =========================================================================

    public void Enable_MoveTo(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        InputComponent.Enable(entity);

        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return;
        }

        var movingSpeed = MovingSpeedLookup[entity];

        InputComponent.SetTarget(entity, target, SubActionConsts.WalkTo.MaxDistance, movingSpeed.GetWalkingSpeed(), movingSpeed.GetWalkingRotationSpeed());
    }

    public void Disable_MoveTo(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        InputComponent.ResetInput(entity);
    }

    public SubActionResult Update_MoveTo(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (timer.IsTimeout(SubActionConsts.WalkTo.FailTime))
        {
            return SubActionResult.Fail(0);
        }

        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(1);
        }

        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(2);
        }

        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(3);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];
        if (entityTransform.IsArrivedAndLooking(targetTransform, SubActionConsts.WalkTo.MaxDistance))
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }

    // =========================================================================
    // Eat
    // =========================================================================

    public void Enable_Eat(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        // Nothing to enable for eat
    }

    public void Disable_Eat(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for eat
    }

    public SubActionResult Update_Eat(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        // if actor entity does not exist in transform lookup, fail state. code = 0
        if (!TransformLookup.TryGetComponent(entity, out var entityTransform))
        {
            return SubActionResult.Fail(0);
        }

        // if target does not exist in transform lookup, fail state. code = 1
        if (!TransformLookup.TryGetComponent(target, out var targetTransform))
        {
            return SubActionResult.Fail(1);
        }

        // if time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(SubActionConsts.Eat.FailTime))
        {
            return SubActionResult.Fail(2);
        }

        // if distance between transforms > MaxDistance - fail with error code 3
        if (entityTransform.IsTargetDistanceReached(targetTransform, SubActionConsts.Eat.MaxDistance) == false)
        {
            return SubActionResult.Fail(3);
        }

        // if target does not exist in EdibleBody lookup, fail state. code = 4
        if (BiteLookup.HasBuffer(target) == false)
        {
            return SubActionResult.Fail(4);
        }

        // if animal does not have AnimalStatsComponent - return fail with code 7
        if (!AnimalStatsLookup.TryGetComponent(entity, out var animalStats))
        {
            return SubActionResult.Fail(7);
        }

        // Check if Fullness >= 100 - returns Success
        if (animalStats.Stats.Fullness >= 100f)
        {
            return SubActionResult.Success();
        }

        // Process eating continuously based on EatingSpeed
        var biteValue = StatsIncreaseLookup[entity].AnimalStats.Fullness * timer.DeltaTime;

        buffer.AppendToBuffer(target, new BiteItem
        {
            Actor = entity,
            BittenNutritionValue = biteValue
        });

        return SubActionResult.Running();
    }

    // =========================================================================
    // MoveInto
    // =========================================================================

    public void Enable_MoveInto(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        InputComponent.Enable(entity);
    }

    public void Disable_MoveInto(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        InputComponent.ResetInput(entity);
    }

    public SubActionResult Update_MoveInto(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        // Check if entity does not exist in transform lookup, fail state. code = 0
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        // Check if target does not exist in transform lookup, fail state. code = 1
        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        // If time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(SubActionConsts.LayDown.FailTime))
        {
            return SubActionResult.Fail(2);
        }

        // if entity does not have MovingSpeedComponent - return fail with code 3
        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // After distance < Distance - returns success
        if (entityTransform.IsTargetPositionReached(targetTransform.Position, SubActionConsts.LayDown.Distance))
        {
            return SubActionResult.Success();
        }

        // Update target position (using crawling speed, no rotation)
        var lookDirection = math.normalize(targetTransform.Position - entityTransform.Position);
        InputComponent.SetTarget(entity, targetTransform.Position, 0, lookDirection, 0.01f, MovingSpeedLookup[entity].GetCrawlingSpeed(), 0f);

        return SubActionResult.Running();
    }

    // =========================================================================
    // Sleep
    // =========================================================================

    public void Enable_Sleep(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        // Nothing to enable for sleeping
    }

    public void Disable_Sleep(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for sleeping
    }

    public SubActionResult Update_Sleep(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        // if actor entity does not exist in transform lookup, fail state. code = 0
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        // if target does not exist in transform lookup, fail state. code = 1
        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        // if time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(SubActionConsts.Sleeping.FailTime))
        {
            return SubActionResult.Fail(2);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // if distance between transforms > MaxDistance - fail with error code 3
        if (!entityTransform.IsTargetDistanceReached(targetTransform, SubActionConsts.Sleeping.MaxDistance))
        {
            return SubActionResult.Fail(3);
        }

        // if target does not exist in SleepingPlaceComponent lookup, fail state. code = 4
        if (!SleepingPlaceLookup.HasComponent(target))
        {
            return SubActionResult.Fail(4);
        }

        // if animal does not have AnimalStatsComponent - fail implicitly handled by lookup

        // Check if AnimalStatsComponent.Energy >= 100 - return success
        var animalStats = AnimalStatsLookup[entity];
        if (animalStats.Stats.Energy >= 100f)
        {
            return SubActionResult.Success();
        }

        // Add to buffer StatsChangeItem EnergyReplanish * DeltaTime
        var sleepingPlace = SleepingPlaceLookup[target];
        var energyGain = sleepingPlace.EnergyReplanish * timer.DeltaTime;

        var statsChange = new AnimalStatsBuilder().WithEnergy(energyGain).Build();

        if (StatChangeLookup.TryGetBuffer(entity, out var changeBuffer))
        {
            changeBuffer.Add(new StatsChangeItem
            {
                StatsChange = statsChange
            });
        }

        return SubActionResult.Running();
    }

    // =========================================================================
    // RunFrom
    // =========================================================================

    private void RunFrom_SetRandomEscapeTarget(Entity entity, float3 entityPosition, float3 targetPosition, ref Random random)
    {
        // Generate new random position
        var movingSpeed = MovingSpeedLookup[entity];
        var safeDistance = new float2(1, 1.5f) * SubActionConsts.RunFrom.SafeDistance;
        var escapePoition = LocalTransformExtensions.GenerateRandomEscapePosition(entityPosition, targetPosition, safeDistance, ref random);
        var lookDirection = math.normalize(escapePoition - entityPosition);

        InputComponent.SetTarget(entity, escapePoition, 0, lookDirection, 0.01f, movingSpeed.GetRunningSpeed()*1.5f, movingSpeed.GetRunningRotationSpeed());
    }

    public void Enable_RunFrom(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        // Check if entity does not exist in transform lookup, skip setup
        if (!TransformLookup.TryGetComponent(entity, out var entityTransform) || 
            !TransformLookup.TryGetComponent(target, out var targetTransform))
        {
            return;
        }

        // if entity does not have MovingSpeedComponent - skip setup
        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return;
        }


        // Enable and set initial target
        InputComponent.Enable(entity);
        RunFrom_SetRandomEscapeTarget(entity, entityTransform.Position, targetTransform.Position, ref random);
    }

    public void Disable_RunFrom(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        InputComponent.ResetInput(entity);
    }

    public SubActionResult Update_RunFrom(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        // Check if entity does not exist in transform lookup, fail state. code = 0
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        // Check if target does not exist in transform lookup, fail state. code = 1
        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // If distance >= SafeDistance - success
        if (entityTransform.IsDistanceGreaterThan(targetTransform, SubActionConsts.RunFrom.SafeDistance))
        {
            return SubActionResult.Success();
        }

        // if entity does not have MovingSpeedComponent - return fail with code 2
        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(2);
        }

        if (!InputComponent.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        var moveInput = InputComponent[entity];

        // If arrived at current target, set new random target
        if (entityTransform.IsTargetDistanceReached(moveInput.TargetPosition, moveInput.TargetScale, moveInput.Distance))
        {
            RunFrom_SetRandomEscapeTarget(entity, entityTransform.Position, targetTransform.Position, ref random);
        }

        return SubActionResult.Running();
    }

    // =========================================================================
    // StumbleUpon
    // =========================================================================

    public void Enable_StumbleUpon(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];
            genitalia.IsEnabled = true;
            buffer.SetComponent(entity, genitalia);
        }
    }

    public void Disable_StumbleUpon(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Disable genitalia component
        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];
            genitalia.IsEnabled = false;
            buffer.SetComponent(entity, genitalia);
        }
    }

    public SubActionResult Update_StumbleUpon(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        // if actor entity does not exist in transform lookup, fail state. code = 0
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        // if target does not exist in transform lookup, fail state. code = 1
        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        // if time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(SubActionConsts.StumbleUpon.FailTime))
        {
            return SubActionResult.Fail(2);
        }
        /*
        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];
        // Check if target is not reached
        if (entityTransform.IsTargetDistanceReached(targetTransform, SubActionConsts.StumbleUpon.MaxDistance) == false)
        {
            return SubActionResult.Running();
        }

        // Check if looking towards target
        if (entityTransform.IsLookingTowards(targetTransform, SubActionConsts.StumbleUpon.Delta) == false)
        {
            return SubActionResult.Running();
        }*/

        // Check if entity has genitalia and enable it
        if (!GenetaliaLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        var genitalia = GenetaliaLookup[entity];

        // If Social >= 100, fail (already satisfied)
        if (AnimalStatsLookup.HasComponent(entity))
        {
            var animalStats = AnimalStatsLookup[entity];
            if (animalStats.Stats.Social >= 100f)
            {
                return SubActionResult.Fail(4);
            }
        }

        // Check if target has genitalia and is opposite sex
        if (!GenetaliaLookup.HasComponent(target))
        {
            return SubActionResult.Fail(5);
        }

        var targetGenitalia = GenetaliaLookup[target];
        
        // Check if opposite sex (male with female or female with male)
        if (genitalia.IsMale != targetGenitalia.IsMale)
        {
            // Check if target's genitalia is enabled
            if (targetGenitalia.IsEnabled)
            {
                return SubActionResult.Success();
            }
            
            return SubActionResult.Running();
        }

        return SubActionResult.Fail(6);
    }

    // =========================================================================
    // Communicate
    // =========================================================================

    public void Enable_Communicate(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        // Enable genitalia component
        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];
            genitalia.IsEnabled = true;
            buffer.SetComponent(entity, genitalia);
        }
    }

    public void Disable_Communicate(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Disable genitalia component
        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];
            genitalia.IsEnabled = false;
            buffer.SetComponent(entity, genitalia);
        }
    }

    public SubActionResult Update_Communicate(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        // if actor entity does not exist in transform lookup, fail state. code = 0
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        // if target does not exist in transform lookup, fail state. code = 1
        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // Check if target is reached
        if (entityTransform.IsTargetDistanceReached(targetTransform, SubActionConsts.Communicate.MaxDistance) == false)
        {
            return SubActionResult.Fail(2);
        }

        // if entity does not have StatsIncreaseComponent - return fail with code 3
        if (!StatsIncreaseLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        // Add stat Social with increase speed from component * delta time
        var statsIncrease = StatsIncreaseLookup[entity];
        var socialGain = statsIncrease.AnimalStats.Social * timer.DeltaTime;

        var statsChange = new AnimalStatsBuilder().WithSocial(socialGain).Build();

        if (StatChangeLookup.TryGetBuffer(entity, out var changeBuffer))
        {
            changeBuffer.Add(new StatsChangeItem
            {
                StatsChange = statsChange
            });
        }

        // Check if entity has stats component
        if (!AnimalStatsLookup.HasComponent(entity))
        {
            return SubActionResult.Running();
        }

        var animalStats = AnimalStatsLookup[entity];

        // Check if entity has genitalia and Social >= 100
        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];
            
            if (animalStats.Stats.Social >= 100f)
            {
                // If male, add DNA to target (which will also enable reproduction on target)
                if (genitalia.IsMale)
                {
                    Communicate_AddDNAToTarget(entity, target, buffer, ref random);
                }
                
                return SubActionResult.Success();
            }
        }

        return SubActionResult.Running();
    }

    private void Communicate_AddDNAToTarget(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        // Check if entity has DNA chain buffer
        if (!DNAChainLookup.HasBuffer(entity))
        {
            return;
        }
        
        // Check if target has DNA storage buffer (only females have this)
        if (!DNAStorageLookup.HasBuffer(target))
        {
            return;
        }
        
        // Get mother's DNA chain
        if (!DNAChainLookup.HasBuffer(target))
        {
            return;
        }
        
        // Get father's DNA chain
        var fatherDNA = DNAChainLookup[entity];
        
        var motherDNA = DNAChainLookup[target];
        
        // Check if DNA chains are compatible
        if (!DNAExtensions.IsCompatible(fatherDNA, motherDNA))
        {
            return;
        }
        
        // Append father's DNA to target's DNA storage
        for (int i = 0; i < fatherDNA.Length; i++)
        {
            buffer.AppendToBuffer(target, new DNAStorageItem
            {
                Father = entity,
                Data = fatherDNA[i].Data
            });
        }
        
        // Set Random seed on ReproductionComponent from the ref Random parameter
        if (ReproductionLookup.HasComponent(target))
        {
            var reproduction = ReproductionLookup[target];
            reproduction.Random = Random.CreateFromIndex(random.NextUInt());
            buffer.SetComponent(target, reproduction);
        }
        
        // Enable ReproductionComponent on target (female) to start gestation
        buffer.SetComponentEnabled<ReproductionComponent>(target, true);
    }
}
