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
            DNAStorageLookup = SystemAPI.GetBufferLookup<DNAStorageItem>(true),
            GenetaliaLookup = SystemAPI.GetComponentLookup<GenetaliaComponent>(true),
            LimitationComponent = SystemAPI.GetComponentLookup<MoveLimitationComponent>(true),
            MovingSpeedLookup = SystemAPI.GetComponentLookup<MovingSpeedComponent>(true),
            ReproductionLookup = SystemAPI.GetComponentLookup<ReproductionComponent>(true),
            SleepingPlaceLookup = SystemAPI.GetComponentLookup<SleepingPlaceComponent>(true),
            StatsIncreaseLookup = SystemAPI.GetComponentLookup<StatsIncreaseComponent>(true),

            MoveOutputLookup = SystemAPI.GetComponentLookup<MoveOutputComponent>(false),
            DNAChainLookup = SystemAPI.GetBufferLookup<DNAChainItem>(false),
            MoveInputLookup = SystemAPI.GetComponentLookup<MoveInputComponent>(false),
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

    public BufferLookup<DNAChainItem> DNAChainLookup;
    public ComponentLookup<MoveInputComponent> MoveInputLookup;
    public BufferLookup<StatsChangeItem> StatChangeLookup;
    public ComponentLookup<MoveOutputComponent> MoveOutputLookup;

    [ReadOnly] public ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    [ReadOnly] public BufferLookup<BiteItem> BiteLookup;
    [ReadOnly] public BufferLookup<DNAStorageItem> DNAStorageLookup;
    [ReadOnly] public ComponentLookup<GenetaliaComponent> GenetaliaLookup;
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
        if (!MoveOutputLookup.TryGetComponent(entity, out var moveOutput))
        {
            return;
        }

        if (!MovingSpeedLookup.TryGetComponent(entity, out var movingSpeed))
        {
            return;
        }

        var radius = random.NextFloat(SubActionConsts.Idle.WanderRadius / 2f, SubActionConsts.Idle.WanderRadius);
        float3 targetPosition;

        if (LimitationComponent.TryGetComponent(entity, out var limitation))
        {
            targetPosition = LocalTransformExtensions.GenerateRandomPosition(limitation.Central, limitation.Scale, ref random);
        }
        else
        {
            targetPosition = LocalTransformExtensions.GenerateRandomPosition(moveOutput.Position, radius, ref random);
        }

        var speed = movingSpeed.GetWalkingSpeed();
        var rotationSpeed = movingSpeed.GetWalkingRotationSpeed();

        MoveInputLookup.Enable(entity, speed, rotationSpeed, math.up());
        MoveInputLookup.SetTarget(entity, targetPosition, SubActionConsts.Idle.MoveDelta);
    }

    public void Disable_Idle(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        MoveInputLookup.Reset(entity);
        MoveOutputLookup.Reset(entity);
    }

    public SubActionResult Update_Idle(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        var time = random.NextFloat(SubActionConsts.Idle.IdleTime / 2f, SubActionConsts.Idle.IdleTime);
        if (timer.IsTimeout(time))
        {
            return SubActionResult.Success();
        }

        if (!MoveInputLookup.TryGetComponent(entity, out var moveInput))
        {
            return SubActionResult.Running();
        }

        if (!MoveOutputLookup.TryGetComponent(entity, out var moveOutput))
        {
            return SubActionResult.Running();
        }

        if (moveInput.IsTargetReached(moveOutput))
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
        if (!MovingSpeedLookup.TryGetComponent(entity, out var movingSpeed))
        {
            return;
        }

        MoveInputLookup.Enable(entity, movingSpeed.GetWalkingSpeed(), movingSpeed.GetWalkingRotationSpeed(), math.up());
        MoveInputLookup.SetTarget(entity, target, SubActionConsts.WalkTo.MaxDistance);
    }

    public void Disable_MoveTo(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        MoveInputLookup.Reset(entity);
        MoveOutputLookup.Reset(entity);
    }

    public SubActionResult Update_MoveTo(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (timer.IsTimeout(SubActionConsts.WalkTo.FailTime))
        {
            return SubActionResult.Fail(0);
        }

        if (!MoveInputLookup.TryGetComponent(entity, out var moveInput))
        {
            return SubActionResult.Fail(1);
        }

        if (!MoveOutputLookup.TryGetComponent(entity, out var moveOutput))
        {
            return SubActionResult.Fail(2);
        }

        if (moveInput.IsTargetReached(moveOutput) && moveInput.IsLookingTowards(moveOutput))
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
        if (!MovingSpeedLookup.TryGetComponent(entity, out var movingSpeed))
        {
            return;
        }

        MoveInputLookup.Enable(entity, 0f, movingSpeed.GetWalkingRotationSpeed(), math.up());
        MoveInputLookup.SetTarget(entity, target, SubActionConsts.Eat.MaxDistance * 2f);
    }

    public void Disable_Eat(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        MoveInputLookup.Reset(entity);
        MoveOutputLookup.Reset(entity);
    }

    public SubActionResult Update_Eat(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (!MoveInputLookup.TryGetComponent(entity, out var moveInput))
        {
            return SubActionResult.Fail(0);
        }

        if (!MoveOutputLookup.TryGetComponent(entity, out var moveOutput))
        {
            return SubActionResult.Fail(1);
        }

        if (timer.IsTimeout(SubActionConsts.Eat.FailTime))
        {
            return SubActionResult.Fail(2);
        }

        if (!moveInput.IsTargetReached(moveOutput))
        {
            return SubActionResult.Fail(3);
        }

        if (!BiteLookup.HasBuffer(target))
        {
            return SubActionResult.Fail(4);
        }

        if (!AnimalStatsLookup.TryGetComponent(entity, out var animalStats))
        {
            return SubActionResult.Fail(7);
        }

        if (animalStats.Stats.Fullness >= 100f)
        {
            return SubActionResult.Success();
        }

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
        if (!MovingSpeedLookup.TryGetComponent(entity, out var movingSpeed))
        {
            return;
        }

        MoveInputLookup.Enable(entity, 0f, movingSpeed.GetWalkingRotationSpeed(), math.up());
        MoveInputLookup.SetTarget(entity, target, SubActionConsts.LayDown.Distance);
    }

    public void Disable_MoveInto(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        MoveInputLookup.Reset(entity);
        MoveOutputLookup.Reset(entity);
    }

    public SubActionResult Update_MoveInto(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (!MoveInputLookup.TryGetComponent(entity, out var moveInput))
        {
            return SubActionResult.Fail(0);
        }

        if (!MoveOutputLookup.TryGetComponent(entity, out var moveOutput))
        {
            return SubActionResult.Fail(1);
        }

        if (timer.IsTimeout(SubActionConsts.LayDown.FailTime))
        {
            return SubActionResult.Fail(2);
        }

        if (moveInput.IsTargetReached(moveOutput))
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }

    // =========================================================================
    // Sleep
    // =========================================================================

    public void Enable_Sleep(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        MoveInputLookup.Enable(entity, 0f, 0f, math.up());
        MoveInputLookup.SetTarget(entity, target, SubActionConsts.Sleeping.MaxDistance);
    }

    public void Disable_Sleep(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        MoveInputLookup.Reset(entity);
        MoveOutputLookup.Reset(entity);
    }

    public SubActionResult Update_Sleep(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (!MoveInputLookup.TryGetComponent(entity, out var moveInput))
        {
            return SubActionResult.Fail(0);
        }

        if (!MoveOutputLookup.TryGetComponent(entity, out var moveOutput))
        {
            return SubActionResult.Fail(1);
        }

        if (timer.IsTimeout(SubActionConsts.Sleeping.FailTime))
        {
            return SubActionResult.Fail(2);
        }

        if (!moveInput.IsTargetReached(moveOutput))
        {
            return SubActionResult.Fail(3);
        }

        if (!SleepingPlaceLookup.TryGetComponent(target, out var sleepingPlace))
        {
            return SubActionResult.Fail(4);
        }

        if (!AnimalStatsLookup.TryGetComponent(entity, out var animalStats))
        {
            return SubActionResult.Running();
        }

        if (animalStats.Stats.Energy >= 100f)
        {
            return SubActionResult.Success();
        }

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
        var safeDistance = new float2(1, 1.5f) * SubActionConsts.RunFrom.SafeDistance;
        var escapePosition = LocalTransformExtensions.GenerateRandomEscapePosition(entityPosition, targetPosition, safeDistance, ref random);
        MoveInputLookup.SetTarget(entity, escapePosition, 0.01f);
    }

    public void Enable_RunFrom(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        if (!MoveOutputLookup.TryGetComponent(entity, out var entityOutput))
        {
            return;
        }

        if (!MoveOutputLookup.TryGetComponent(target, out var targetOutput))
        {
            return;
        }

        if (!MovingSpeedLookup.TryGetComponent(entity, out var movingSpeed))
        {
            return;
        }

        MoveInputLookup.Enable(entity, movingSpeed.GetRunningSpeed(), movingSpeed.GetRunningRotationSpeed(), math.up());
        RunFrom_SetRandomEscapeTarget(entity, entityOutput.Position, targetOutput.Position, ref random);
    }

    public void Disable_RunFrom(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        MoveInputLookup.Reset(entity);
        MoveOutputLookup.Reset(entity);
    }

    public SubActionResult Update_RunFrom(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (!MoveOutputLookup.TryGetComponent(entity, out var entityOutput))
        {
            return SubActionResult.Fail(0);
        }

        if (!MoveOutputLookup.TryGetComponent(target, out var targetOutput))
        {
            return SubActionResult.Fail(1);
        }

        if (math.distance(entityOutput.Position, targetOutput.Position) >= SubActionConsts.RunFrom.SafeDistance)
        {
            return SubActionResult.Success();
        }

        if (!MovingSpeedLookup.TryGetComponent(entity, out _))
        {
            return SubActionResult.Fail(2);
        }

        if (!MoveInputLookup.TryGetComponent(entity, out var moveInput))
        {
            return SubActionResult.Fail(3);
        }

        if (moveInput.IsTargetReached(entityOutput))
        {
            RunFrom_SetRandomEscapeTarget(entity, entityOutput.Position, targetOutput.Position, ref random);
        }

        return SubActionResult.Running();
    }

    // =========================================================================
    // StumbleUpon
    // =========================================================================

    public void Enable_StumbleUpon(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        if (GenetaliaLookup.TryGetComponent(entity, out var genitalia))
        {
            genitalia.IsEnabled = true;
            buffer.SetComponent(entity, genitalia);
        }

        if (!MovingSpeedLookup.TryGetComponent(entity, out var movingSpeed))
        {
            return;
        }

        MoveInputLookup.Enable(entity, 0f, movingSpeed.GetWalkingRotationSpeed(), math.up());
        MoveInputLookup.SetTarget(entity, target, SubActionConsts.StumbleUpon.MaxDistance);
    }

    public void Disable_StumbleUpon(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        if (GenetaliaLookup.TryGetComponent(entity, out var genitalia))
        {
            genitalia.IsEnabled = false;
            buffer.SetComponent(entity, genitalia);
        }

        MoveInputLookup.Reset(entity);
        MoveOutputLookup.Reset(entity);
    }

    public SubActionResult Update_StumbleUpon(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (!MoveOutputLookup.TryGetComponent(entity, out _))
        {
            return SubActionResult.Fail(0);
        }

        if (!MoveOutputLookup.TryGetComponent(target, out _))
        {
            return SubActionResult.Fail(1);
        }

        if (timer.IsTimeout(SubActionConsts.StumbleUpon.FailTime))
        {
            return SubActionResult.Fail(2);
        }

        if (!GenetaliaLookup.TryGetComponent(entity, out var genitalia))
        {
            return SubActionResult.Fail(3);
        }

        if (AnimalStatsLookup.TryGetComponent(entity, out var animalStats))
        {
            if (animalStats.Stats.Social >= 100f)
            {
                return SubActionResult.Fail(4);
            }
        }

        if (!GenetaliaLookup.TryGetComponent(target, out var targetGenitalia))
        {
            return SubActionResult.Fail(5);
        }

        if (genitalia.IsMale != targetGenitalia.IsMale)
        {
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
        if (GenetaliaLookup.TryGetComponent(entity, out var genitalia))
        {
            genitalia.IsEnabled = true;
            buffer.SetComponent(entity, genitalia);
        }

        if (!MovingSpeedLookup.TryGetComponent(entity, out var movingSpeed))
        {
            return;
        }

        MoveInputLookup.Enable(entity, 0f, movingSpeed.GetWalkingRotationSpeed(), math.up());
        MoveInputLookup.SetTarget(entity, target, SubActionConsts.Communicate.MaxDistance);
    }

    public void Disable_Communicate(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        if (GenetaliaLookup.TryGetComponent(entity, out var genitalia))
        {
            genitalia.IsEnabled = false;
            buffer.SetComponent(entity, genitalia);
        }

        MoveInputLookup.Reset(entity);
        MoveOutputLookup.Reset(entity);
    }

    public SubActionResult Update_Communicate(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (!MoveInputLookup.TryGetComponent(entity, out var moveInput))
        {
            return SubActionResult.Fail(0);
        }

        if (!MoveOutputLookup.TryGetComponent(entity, out var moveOutput))
        {
            return SubActionResult.Fail(1);
        }

        if (!moveInput.IsTargetReached(moveOutput))
        {
            return SubActionResult.Fail(2);
        }

        if (!StatsIncreaseLookup.TryGetComponent(entity, out var statsIncrease))
        {
            return SubActionResult.Fail(3);
        }

        var socialGain = statsIncrease.AnimalStats.Social * timer.DeltaTime;
        var statsChange = new AnimalStatsBuilder().WithSocial(socialGain).Build();

        if (StatChangeLookup.TryGetBuffer(entity, out var changeBuffer))
        {
            changeBuffer.Add(new StatsChangeItem
            {
                StatsChange = statsChange
            });
        }

        if (!AnimalStatsLookup.TryGetComponent(entity, out var animalStats))
        {
            return SubActionResult.Running();
        }

        if (!GenetaliaLookup.TryGetComponent(entity, out var entityGenitalia))
        {
            return SubActionResult.Running();
        }

        if (animalStats.Stats.Social >= 100f)
        {
            if (entityGenitalia.IsMale)
            {
                Communicate_AddDNAToTarget(entity, target, buffer, ref random);
            }

            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }

    private void Communicate_AddDNAToTarget(Entity entity, Entity target, EntityCommandBuffer buffer, ref Random random)
    {
        if (!DNAChainLookup.TryGetBuffer(entity, out var fatherDNA))
        {
            return;
        }

        if (!DNAStorageLookup.HasBuffer(target))
        {
            return;
        }

        if (!DNAChainLookup.TryGetBuffer(target, out var motherDNA))
        {
            return;
        }

        if (!DNAExtensions.IsCompatible(fatherDNA, motherDNA))
        {
            return;
        }

        for (int i = 0; i < fatherDNA.Length; i++)
        {
            buffer.AppendToBuffer(target, new DNAStorageItem
            {
                Father = entity,
                Data = fatherDNA[i].Data
            });
        }

        if (ReproductionLookup.TryGetComponent(target, out var reproduction))
        {
            reproduction.Random = Random.CreateFromIndex(random.NextUInt());
            buffer.SetComponent(target, reproduction);
        }

        buffer.SetComponentEnabled<ReproductionComponent>(target, true);
    }
}
