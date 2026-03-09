using LittleAI.Enums;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct ActionRunnerUnmanagedSystem : ISystem
{
    // Component lookups
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<EdibleComponent> EdibleLookup;
    private ComponentLookup<GenetaliaComponent> GenetaliaLookup;
    private ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    private ComponentLookup<StatsIncreaseComponent> StatsIncreaseLookup;
    private ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;
    private ComponentLookup<SleepingPlaceComponent> SleepingPlaceLookup;
    private ComponentLookup<ReproductionComponent> ReproductionLookup;
    private BufferLookup<DNAChainItem> DNAChainLookup;
    private BufferLookup<DNAStorageItem> DNAStorageLookup;
    private ComponentLookup<MoveControllerOutputComponent> MoveControllerOutputLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ActionChainConfigComponent>();
        state.RequireForUpdate<ActionChainItem>();
        state.RequireForUpdate<ActionRunnerComponent>();
        state.RequireForUpdate<ActionMapInitializeComponent>();

        TransformLookup = state.GetComponentLookup<LocalTransform>(true);
        EdibleLookup = state.GetComponentLookup<EdibleComponent>(true);
        GenetaliaLookup = state.GetComponentLookup<GenetaliaComponent>(true);
        AnimalStatsLookup = state.GetComponentLookup<AnimalStatsComponent>(true);
        StatsIncreaseLookup = state.GetComponentLookup<StatsIncreaseComponent>(true);
        MovingSpeedLookup = state.GetComponentLookup<MovingSpeedComponent>(true);
        SleepingPlaceLookup = state.GetComponentLookup<SleepingPlaceComponent>(true);
        ReproductionLookup = state.GetComponentLookup<ReproductionComponent>(true);
        DNAChainLookup = state.GetBufferLookup<DNAChainItem>(true);
        DNAStorageLookup = state.GetBufferLookup<DNAStorageItem>(true);
        MoveControllerOutputLookup = state.GetComponentLookup<MoveControllerOutputComponent>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        TransformLookup.Update(ref state);
        EdibleLookup.Update(ref state);
        GenetaliaLookup.Update(ref state);
        AnimalStatsLookup.Update(ref state);
        StatsIncreaseLookup.Update(ref state);
        MovingSpeedLookup.Update(ref state);
        SleepingPlaceLookup.Update(ref state);
        ReproductionLookup.Update(ref state);
        DNAChainLookup.Update(ref state);
        DNAStorageLookup.Update(ref state);
        MoveControllerOutputLookup.Update(ref state);

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var buffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var job = new ActionRunnerJob
        {
            ActionsMap = SystemAPI.GetSingleton<ActionChainConfigComponent>(),
            DeltaTime = SystemAPI.Time.DeltaTime,
            Buffer = buffer,
            TransformLookup = TransformLookup,
            EdibleLookup = EdibleLookup,
            GenetaliaLookup = GenetaliaLookup,
            AnimalStatsLookup = AnimalStatsLookup,
            StatsIncreaseLookup = StatsIncreaseLookup,
            MovingSpeedLookup = MovingSpeedLookup,
            SleepingPlaceLookup = SleepingPlaceLookup,
            ReproductionLookup = ReproductionLookup,
            DNAChainLookup = DNAChainLookup,
            DNAStorageLookup = DNAStorageLookup,
            MoveControllerOutputLookup = MoveControllerOutputLookup,
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

    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
    [ReadOnly] public ComponentLookup<EdibleComponent> EdibleLookup;
    [ReadOnly] public ComponentLookup<GenetaliaComponent> GenetaliaLookup;
    [ReadOnly] public ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    [ReadOnly] public ComponentLookup<StatsIncreaseComponent> StatsIncreaseLookup;
    [ReadOnly] public ComponentLookup<MovingSpeedComponent> MovingSpeedLookup;
    [ReadOnly] public ComponentLookup<SleepingPlaceComponent> SleepingPlaceLookup;
    [ReadOnly] public ComponentLookup<ReproductionComponent> ReproductionLookup;
    [ReadOnly] public BufferLookup<DNAChainItem> DNAChainLookup;
    [ReadOnly] public BufferLookup<DNAStorageItem> DNAStorageLookup;
    [ReadOnly] public ComponentLookup<MoveControllerOutputComponent> MoveControllerOutputLookup;

    // Constants for sub-actions
    private const float Idle_IdleTime = 10f;
    private const float Idle_WanderRadius = 10f;

    private const float MoveTo_MaxDistance = 0.2f;
    private const float MoveTo_FailTime = 30f;

    private const float MoveToTalk_MaxDistance = 0.2f;
    private const float MoveToTalk_FailTime = 30f;

    private const float RunFrom_SafeDistance = 10f;

    private const float RotateTowards_FailTime = 10f;

    private const float Eat_FailTime = 20f;
    private const float Eat_MaxDistance = 0.2f;

    private const float MoveInto_FailTime = 5f;
    private const float MoveInto_Distance = 0.01f;

    private const float Sleep_FailTime = 100f;
    private const float Sleep_MaxDistance = 0.01f;

    private const float StumbleUpon_FailTime = 2f;

    private const float Communicate_MaxDistance = 0.3f;

    public void Execute(
        Entity entity,
        ref ActionRunnerComponent runner,
        ref SubActionTimeComponent timer,
        ref ActionRandomComponent randomComponent,
        ref DynamicBuffer<ActionChainItem> chain)
    {
        timer.DeltaTime = DeltaTime;
        timer.TimeElapsed += DeltaTime;

        if (runner.Action == ActionTypes.None)
        {
            SetActionIdle(ref runner);
            EnableState(entity, in runner, ref randomComponent);
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
                Disable_Idle(entity, runner.Target);
                break;
            case SubActionTypes.MoveTo:
                Disable_MoveTo(entity, runner.Target);
                break;
            case SubActionTypes.MoveToTalk:
                Disable_MoveToTalk(entity, runner.Target);
                break;
            case SubActionTypes.RunFrom:
                Disable_RunFrom(entity, runner.Target);
                break;
            case SubActionTypes.RotateTowards:
                Disable_RotateTowards(entity, runner.Target);
                break;
            case SubActionTypes.Eat:
                Disable_Eat(entity, runner.Target);
                break;
            case SubActionTypes.MoveInto:
                Disable_MoveInto(entity, runner.Target);
                break;
            case SubActionTypes.Sleep:
                Disable_Sleep(entity, runner.Target);
                break;
            case SubActionTypes.StumbleUpon:
                Disable_StumbleUpon(entity, runner.Target);
                break;
            case SubActionTypes.Communicate:
                Disable_Communicate(entity, runner.Target);
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
                Enable_Idle(entity, runner.Target, ref randomComponent.Random);
                break;
            case SubActionTypes.MoveTo:
                Enable_MoveTo(entity, runner.Target, ref randomComponent.Random);
                break;
            case SubActionTypes.MoveToTalk:
                Enable_MoveToTalk(entity, runner.Target, ref randomComponent.Random);
                break;
            case SubActionTypes.RunFrom:
                Enable_RunFrom(entity, runner.Target, ref randomComponent.Random);
                break;
            case SubActionTypes.RotateTowards:
                Enable_RotateTowards(entity, runner.Target, ref randomComponent.Random);
                break;
            case SubActionTypes.Eat:
                Enable_Eat(entity, runner.Target, ref randomComponent.Random);
                break;
            case SubActionTypes.MoveInto:
                Enable_MoveInto(entity, runner.Target, ref randomComponent.Random);
                break;
            case SubActionTypes.Sleep:
                Enable_Sleep(entity, runner.Target, ref randomComponent.Random);
                break;
            case SubActionTypes.StumbleUpon:
                Enable_StumbleUpon(entity, runner.Target, ref randomComponent.Random);
                break;
            case SubActionTypes.Communicate:
                Enable_Communicate(entity, runner.Target, ref randomComponent.Random);
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
                return Update_Idle(entity, runner.Target, in timer, ref randomComponent.Random);
            case SubActionTypes.MoveTo:
                return Update_MoveTo(entity, runner.Target, in timer, ref randomComponent.Random);
            case SubActionTypes.MoveToTalk:
                return Update_MoveToTalk(entity, runner.Target, in timer, ref randomComponent.Random);
            case SubActionTypes.RunFrom:
                return Update_RunFrom(entity, runner.Target, in timer, ref randomComponent.Random);
            case SubActionTypes.RotateTowards:
                return Update_RotateTowards(entity, runner.Target, in timer, ref randomComponent.Random);
            case SubActionTypes.Eat:
                return Update_Eat(entity, runner.Target, in timer, ref randomComponent.Random);
            case SubActionTypes.MoveInto:
                return Update_MoveInto(entity, runner.Target, in timer, ref randomComponent.Random);
            case SubActionTypes.Sleep:
                return Update_Sleep(entity, runner.Target, in timer, ref randomComponent.Random);
            case SubActionTypes.StumbleUpon:
                return Update_StumbleUpon(entity, runner.Target, in timer, ref randomComponent.Random);
            case SubActionTypes.Communicate:
                return Update_Communicate(entity, runner.Target, in timer, ref randomComponent.Random);
            default:
                return SubActionResult.Fail(-1);
        }
    }

    // =========================================================================
    // Idle
    // =========================================================================

    private void Enable_Idle(Entity entity, Entity target, ref Random random)
    {
        if (!TransformLookup.HasComponent(entity) || !MovingSpeedLookup.HasComponent(entity))
        {
            return;
        }

        var entityTransform = TransformLookup[entity];
        var movingSpeed = MovingSpeedLookup[entity];

        var radius = random.NextFloat(Idle_WanderRadius / 2f, Idle_WanderRadius);
        var targetPosition = LocalTransformExtensions.GenerateRandomPosition(entityTransform.Position, radius, ref random);
        var lookDirection = math.normalize(targetPosition - entityTransform.Position);

        MoveControllerExtensions.Enable(Buffer, entity);
        MoveControllerExtensions.SetTarget(Buffer, entity, targetPosition, 0, lookDirection, 0.01f, movingSpeed.GetWalkingSpeed(), movingSpeed.GetWalkingRotationSpeed());
    }

    private void Disable_Idle(Entity entity, Entity target)
    {
        MoveControllerExtensions.Disable(Buffer, entity);
    }

    private SubActionResult Update_Idle(Entity entity, Entity target, in SubActionTimeComponent timer, ref Random random)
    {
        if (timer.IsTimeout(Idle_IdleTime))
        {
            return SubActionResult.Success();
        }

        if (MoveControllerOutputLookup.TryGetComponent(entity, out var moveOutput))
        {
            if (moveOutput.HasArrived)
            {
                return SubActionResult.Success();
            }
        }

        return SubActionResult.Running();
    }

    // =========================================================================
    // MoveTo
    // =========================================================================

    private void Enable_MoveTo(Entity entity, Entity target, ref Random random)
    {
        MoveControllerExtensions.Enable(Buffer, entity);
    }

    private void Disable_MoveTo(Entity entity, Entity target)
    {
        MoveControllerExtensions.Disable(Buffer, entity);
    }

    private SubActionResult Update_MoveTo(Entity entity, Entity target, in SubActionTimeComponent timer, ref Random random)
    {
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        if (timer.IsTimeout(MoveTo_FailTime))
        {
            return SubActionResult.Fail(2);
        }

        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        if (!MoveControllerOutputLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(4);
        }

        var moveOutput = MoveControllerOutputLookup[entity];

        if (moveOutput.HasArrived && moveOutput.IsLookingAt)
        {
            return SubActionResult.Success();
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];
        var movingSpeed = MovingSpeedLookup[entity];
        var lookDirection = math.normalize(targetTransform.Position - entityTransform.Position);

        MoveControllerExtensions.SetTarget(Buffer, entity, targetTransform.Position, targetTransform.Scale, lookDirection, MoveTo_MaxDistance, movingSpeed.GetWalkingSpeed(), movingSpeed.GetWalkingRotationSpeed());

        return SubActionResult.Running();
    }

    // =========================================================================
    // MoveToTalk
    // =========================================================================

    private void Enable_MoveToTalk(Entity entity, Entity target, ref Random random)
    {
        MoveControllerExtensions.Enable(Buffer, entity);
    }

    private void Disable_MoveToTalk(Entity entity, Entity target)
    {
        MoveControllerExtensions.Disable(Buffer, entity);
    }

    private SubActionResult Update_MoveToTalk(Entity entity, Entity target, in SubActionTimeComponent timer, ref Random random)
    {
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        if (timer.IsTimeout(MoveToTalk_FailTime))
        {
            return SubActionResult.Fail(2);
        }

        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        if (!MoveControllerOutputLookup.TryGetComponent(entity, out var output))
        {
            return SubActionResult.Fail(4);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        if (output.HasArrived && output.IsLookingAt)
        {
            return SubActionResult.Success();
        }

        var lookDirection = math.normalize(targetTransform.Position - entityTransform.Position);
        MoveControllerExtensions.SetTarget(Buffer, entity, targetTransform.Position, targetTransform.Scale, lookDirection, MoveToTalk_MaxDistance, MovingSpeedLookup[entity].GetWalkingSpeed(),
            MovingSpeedLookup[entity].GetWalkingRotationSpeed());

        return SubActionResult.Running();
    }

    // =========================================================================
    // RunFrom
    // =========================================================================

    private void SetRandomEscapeTarget(Entity entity, float3 entityPosition, float3 targetPosition, ref Random random)
    {
        var movingSpeed = MovingSpeedLookup[entity];
        var safeDistance = new float2(1, 1.5f) * RunFrom_SafeDistance;
        var escapePosition = LocalTransformExtensions.GenerateRandomEscapePosition(entityPosition, targetPosition, safeDistance, ref random);
        var lookDirection = math.normalize(escapePosition - entityPosition);

        MoveControllerExtensions.SetTarget(Buffer, entity, escapePosition, 0, lookDirection, 0.01f, movingSpeed.GetRunningSpeed(), movingSpeed.GetRunningRotationSpeed());
        MoveControllerExtensions.ResetOutput(Buffer, entity);
    }

    private void Enable_RunFrom(Entity entity, Entity target, ref Random random)
    {
        if (!TransformLookup.TryGetComponent(entity, out var entityTransform) ||
            !TransformLookup.TryGetComponent(target, out var targetTransform))
        {
            return;
        }

        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return;
        }

        MoveControllerExtensions.Enable(Buffer, entity);
        SetRandomEscapeTarget(entity, entityTransform.Position, targetTransform.Position, ref random);
    }

    private void Disable_RunFrom(Entity entity, Entity target)
    {
        MoveControllerExtensions.Disable(Buffer, entity);
    }

    private SubActionResult Update_RunFrom(Entity entity, Entity target, in SubActionTimeComponent timer, ref Random random)
    {
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        if (entityTransform.IsDistanceGreaterThan(targetTransform, RunFrom_SafeDistance))
        {
            return SubActionResult.Success();
        }

        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(2);
        }

        if (!MoveControllerOutputLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        var moveOutput = MoveControllerOutputLookup[entity];

        if (moveOutput.HasArrived)
        {
            SetRandomEscapeTarget(entity, entityTransform.Position, targetTransform.Position, ref random);
        }

        return SubActionResult.Running();
    }

    // =========================================================================
    // RotateTowards
    // =========================================================================

    private void Enable_RotateTowards(Entity entity, Entity target, ref Random random)
    {
        MoveControllerExtensions.Enable(Buffer, entity);
    }

    private void Disable_RotateTowards(Entity entity, Entity target)
    {
        MoveControllerExtensions.Disable(Buffer, entity);
    }

    private SubActionResult Update_RotateTowards(Entity entity, Entity target, in SubActionTimeComponent timer, ref Random random)
    {
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        if (timer.IsTimeout(RotateTowards_FailTime))
        {
            return SubActionResult.Fail(2);
        }

        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        if (!MoveControllerOutputLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(4);
        }

        var moveOutput = MoveControllerOutputLookup[entity];

        if (moveOutput.IsLookingAt)
        {
            return SubActionResult.Success();
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];
        var lookDirection = math.normalize(targetTransform.Position - entityTransform.Position);

        MoveControllerExtensions.SetTarget(Buffer, entity, entityTransform.Position, 0, lookDirection, 0f, 0f, MovingSpeedLookup[entity].GetWalkingRotationSpeed());

        return SubActionResult.Running();
    }

    // =========================================================================
    // Eat
    // =========================================================================

    private void Enable_Eat(Entity entity, Entity target, ref Random random)
    {
        // Nothing to enable for eat
    }

    private void Disable_Eat(Entity entity, Entity target)
    {
        // Nothing to disable for eat
    }

    private SubActionResult Update_Eat(Entity entity, Entity target, in SubActionTimeComponent timer, ref Random random)
    {
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        if (timer.IsTimeout(Eat_FailTime))
        {
            return SubActionResult.Fail(2);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        if (!entityTransform.IsTargetDistanceReached(targetTransform, Eat_MaxDistance))
        {
            return SubActionResult.Fail(3);
        }

        if (!EdibleLookup.HasComponent(target))
        {
            return SubActionResult.Fail(4);
        }

        var edibleComponent = EdibleLookup[target];

        if (!TransformLookup.HasComponent(edibleComponent.EdibleBody))
        {
            return SubActionResult.Fail(5);
        }

        var edibleBodyTransform = TransformLookup[edibleComponent.EdibleBody];

        if (edibleBodyTransform.Scale <= 0)
        {
            return SubActionResult.Fail(6);
        }

        if (!AnimalStatsLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(7);
        }

        if (!StatsIncreaseLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(8);
        }

        var animalStats = AnimalStatsLookup[entity];
        if (animalStats.Stats.Fullness >= 100f)
        {
            return SubActionResult.Success();
        }

        var statsIncrease = StatsIncreaseLookup[entity];
        Eat(entity, target, edibleComponent, edibleBodyTransform, statsIncrease.AnimalStats.Fullness, timer.DeltaTime);

        return SubActionResult.Running();
    }

    private void Eat(Entity entity, Entity target, EdibleComponent edibleComponent, LocalTransform edibleBodyTransform, float eatingSpeed, float deltaTime)
    {
        float biteSize = (eatingSpeed / 100f) * deltaTime;

        var actualBiteSize = biteSize;
        var newScale = edibleBodyTransform.Scale - biteSize;

        if (newScale < 0)
        {
            actualBiteSize = edibleBodyTransform.Scale;
            newScale = 0;
        }

        edibleBodyTransform.Scale = newScale;
        Buffer.SetComponent(edibleComponent.EdibleBody, edibleBodyTransform);

        var nutritionGained = actualBiteSize * edibleComponent.Nutrition;

        var statsChange = new AnimalStatsBuilder().WithFullness(nutritionGained).Build();

        Buffer.AppendToBuffer(entity, new StatsChangeItem
        {
            StatsChange = statsChange
        });

        if (newScale <= 0)
        {
            Buffer.DestroyEntity(target);
        }
    }

    // =========================================================================
    // MoveInto (LayDown)
    // =========================================================================

    private void Enable_MoveInto(Entity entity, Entity target, ref Random random)
    {
        MoveControllerExtensions.Enable(Buffer, entity);
    }

    private void Disable_MoveInto(Entity entity, Entity target)
    {
        MoveControllerExtensions.Disable(Buffer, entity);
    }

    private SubActionResult Update_MoveInto(Entity entity, Entity target, in SubActionTimeComponent timer, ref Random random)
    {
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        if (timer.IsTimeout(MoveInto_FailTime))
        {
            return SubActionResult.Fail(2);
        }

        if (!MovingSpeedLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        if (!MoveControllerOutputLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(4);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        if (entityTransform.IsTargetPositionReached(targetTransform.Position, MoveInto_Distance))
        {
            return SubActionResult.Success();
        }

        var lookDirection = math.normalize(targetTransform.Position - entityTransform.Position);
        MoveControllerExtensions.SetTarget(Buffer, entity, targetTransform.Position, 0, lookDirection, 0.01f, MovingSpeedLookup[entity].GetCrawlingSpeed(), 0f);

        return SubActionResult.Running();
    }

    // =========================================================================
    // Sleep
    // =========================================================================

    private void Enable_Sleep(Entity entity, Entity target, ref Random random)
    {
        // Nothing to enable for sleeping
    }

    private void Disable_Sleep(Entity entity, Entity target)
    {
        // Nothing to disable for sleeping
    }

    private SubActionResult Update_Sleep(Entity entity, Entity target, in SubActionTimeComponent timer, ref Random random)
    {
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        if (timer.IsTimeout(Sleep_FailTime))
        {
            return SubActionResult.Fail(2);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        if (!entityTransform.IsTargetDistanceReached(targetTransform, Sleep_MaxDistance))
        {
            return SubActionResult.Fail(3);
        }

        if (!SleepingPlaceLookup.HasComponent(target))
        {
            return SubActionResult.Fail(4);
        }

        var animalStats = AnimalStatsLookup[entity];
        if (animalStats.Stats.Energy >= 100f)
        {
            return SubActionResult.Success();
        }

        var sleepingPlace = SleepingPlaceLookup[target];
        var energyGain = sleepingPlace.EnergyReplanish * timer.DeltaTime;

        var statsChange = new AnimalStatsBuilder().WithEnergy(energyGain).Build();

        Buffer.AppendToBuffer(entity, new StatsChangeItem
        {
            StatsChange = statsChange
        });

        return SubActionResult.Running();
    }

    // =========================================================================
    // StumbleUpon
    // =========================================================================

    private void Enable_StumbleUpon(Entity entity, Entity target, ref Random random)
    {
        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];
            genitalia.IsEnabled = true;
            Buffer.SetComponent(entity, genitalia);
        }
    }

    private void Disable_StumbleUpon(Entity entity, Entity target)
    {
        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];
            genitalia.IsEnabled = false;
            Buffer.SetComponent(entity, genitalia);
        }
    }

    private SubActionResult Update_StumbleUpon(Entity entity, Entity target, in SubActionTimeComponent timer, ref Random random)
    {
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        if (timer.IsTimeout(StumbleUpon_FailTime))
        {
            return SubActionResult.Fail(2);
        }

        if (!GenetaliaLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        var genitalia = GenetaliaLookup[entity];

        if (AnimalStatsLookup.HasComponent(entity))
        {
            var animalStats = AnimalStatsLookup[entity];
            if (animalStats.Stats.Social >= 100f)
            {
                return SubActionResult.Fail(4);
            }
        }

        if (!GenetaliaLookup.HasComponent(target))
        {
            return SubActionResult.Fail(5);
        }

        var targetGenitalia = GenetaliaLookup[target];

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

    private void Enable_Communicate(Entity entity, Entity target, ref Random random)
    {
        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];
            genitalia.IsEnabled = true;
            Buffer.SetComponent(entity, genitalia);
        }
    }

    private void Disable_Communicate(Entity entity, Entity target)
    {
        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];
            genitalia.IsEnabled = false;
            Buffer.SetComponent(entity, genitalia);
        }
    }

    private SubActionResult Update_Communicate(Entity entity, Entity target, in SubActionTimeComponent timer, ref Random random)
    {
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        if (entityTransform.IsTargetDistanceReached(targetTransform, Communicate_MaxDistance) == false)
        {
            return SubActionResult.Fail(2);
        }

        if (!StatsIncreaseLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        var statsIncrease = StatsIncreaseLookup[entity];
        var socialGain = statsIncrease.AnimalStats.Social * timer.DeltaTime;

        var statsChange = new AnimalStatsBuilder().WithSocial(socialGain).Build();

        Buffer.AppendToBuffer(entity, new StatsChangeItem
        {
            StatsChange = statsChange
        });

        if (!AnimalStatsLookup.HasComponent(entity))
        {
            return SubActionResult.Running();
        }

        var animalStats = AnimalStatsLookup[entity];

        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];

            if (animalStats.Stats.Social >= 100f)
            {
                if (genitalia.IsMale)
                {
                    AddDNAToTarget(entity, target, ref random);
                }

                return SubActionResult.Success();
            }
        }

        return SubActionResult.Running();
    }

    private void AddDNAToTarget(Entity entity, Entity target, ref Random random)
    {
        if (!DNAChainLookup.HasBuffer(entity))
        {
            return;
        }

        if (!DNAStorageLookup.HasBuffer(target))
        {
            return;
        }

        if (!DNAChainLookup.HasBuffer(target))
        {
            return;
        }

        var fatherDNA = DNAChainLookup[entity];
        var motherDNA = DNAChainLookup[target];

        if (!DNAExtensions.IsCompatible(fatherDNA, motherDNA))
        {
            return;
        }

        for (int i = 0; i < fatherDNA.Length; i++)
        {
            Buffer.AppendToBuffer(target, new DNAStorageItem
            {
                Father = entity,
                Data = fatherDNA[i].Data
            });
        }

        if (ReproductionLookup.HasComponent(target))
        {
            var reproduction = ReproductionLookup[target];
            reproduction.Random = Random.CreateFromIndex(random.NextUInt());
            Buffer.SetComponent(target, reproduction);
        }

        Buffer.SetComponentEnabled<ReproductionComponent>(target, true);
    }
}