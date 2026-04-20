using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct FishBuilder
{
    private Entity Entity;
    private EntityCommandBuffer CommandBuffer;
    private uint RandomSeed;

    public FishBuilder(Entity entity, EntityCommandBuffer commandBuffer, uint randomSeed = 1)
    {
        Entity = entity;
        CommandBuffer = commandBuffer;
        RandomSeed = randomSeed;
    }
    
    public FishBuilder WithActionChain()
    {
        CommandBuffer.AddComponent<ActionRunnerComponent>(Entity);
        CommandBuffer.AddBuffer<ActionChainItem>(Entity);
        CommandBuffer.AddComponent<SubActionTimeComponent>(Entity);
        CommandBuffer.AddComponent(Entity, new ActionRandomComponent
        {
            Random = Random.CreateFromIndex(RandomSeed)
        });
        
        return this;
    }

    public FishBuilder WithVision(float maxDistance, float interval)
    {
        // Add vision component
        CommandBuffer.AddComponent(Entity, new VisionComponent
        {
            MaxDistance = maxDistance,
        });

        // Add visible items buffer
        CommandBuffer.AddBuffer<VisibleItem>(Entity);

        return this;
    }

    public FishBuilder WithConditionFlags(ConditionFlags flags)
    {
        CommandBuffer.AddComponent(Entity, new ConditionFlagsComponent
        {
            Conditions = flags
        });

        return this;
    }

    public FishBuilder WithSafetyCheck(NativeArray<SafetyCheckItem> items)
    {
        var buffer = CommandBuffer.AddBuffer<SafetyCheckItem>(Entity);

        for (int i = 0; i < items.Length; i++)
        {
            buffer.Add(items[i]);
        }

        return this;
    }

    public FishBuilder WithFemaleReproduction(float gestationTime = 10f)
    {
        CommandBuffer.AddComponent(Entity, new GenetaliaComponent
        {
            IsMale = false,
            IsEnabled = false
        });
        
        CommandBuffer.AddComponent(Entity, new ReproductionComponent
        {
            IsMale = false,
            GestationTime = gestationTime,
            TimeElapsed = 0f
        });
        CommandBuffer.SetComponentEnabled<ReproductionComponent>(Entity, false);

        CommandBuffer.AddBuffer<DNAStorageItem>(Entity);

        return this;
    }

    public FishBuilder WithMaleReproduction()
    {
        CommandBuffer.AddComponent(Entity, new GenetaliaComponent
        {
            IsMale = true,
            IsEnabled = false
        });
        
        CommandBuffer.AddComponent(Entity, new ReproductionComponent
        {
            IsMale = true,
            GestationTime = 0f,
            TimeElapsed = 0f
        });
        CommandBuffer.SetComponentEnabled<ReproductionComponent>(Entity, false);

        return this;
    }

    public FishBuilder WithMoving(float maxSpeed, float maxRotationSpeed, float rotateFailTime, float moveFailTime, 
        float crawlingSpeedT, float walkingSpeedT, float walkingRotationSpeedT, float idleTime)
    {
        CommandBuffer.AddComponent(Entity, new MovingDataComponent
        {
            MaxSpeed = maxSpeed,
            MaxRotationSpeed = maxRotationSpeed,
            RotateFailTime = rotateFailTime,
            MoveFailTime = moveFailTime,
            CrawlingSpeedT = crawlingSpeedT,
            WalkingSpeedT = walkingSpeedT,
            WalkingRotationSpeedT = walkingRotationSpeedT,
            IdleTime = idleTime
        });

        return this;
    }

    public FishBuilder WithTalking(float stumbleFailTime, float maxDistance, float socialIncrease)
    {
        CommandBuffer.AddComponent(Entity, new TalkingDataComponent
        {
            StumbleFailTime = stumbleFailTime,
            MaxDistance = maxDistance,
            SocialIncrease = socialIncrease
        });

        return this;
    }

    public FishBuilder WithSleeping(float failTime, float maxDistance, float layDownFailTime, float distance)
    {
        CommandBuffer.AddComponent(Entity, new SleepDataComponent
        {
            FailTime = failTime,
            MaxDistance = maxDistance,
            LayDownFailTime = layDownFailTime,
            Distance = distance
        });

        return this;
    }

    public FishBuilder WithEating(float interval, float failTime, float maxDistance, float biteSize)
    {
        CommandBuffer.AddComponent(Entity, new EatDataComponent
        {
            Interval = interval,
            FailTime = failTime,
            MaxDistance = maxDistance,
            BiteSize = biteSize
        });

        return this;
    }

    public FishBuilder WithSafety(float safeDistance, float checkInterval)
    {
        CommandBuffer.AddComponent(Entity, new SafetyDataComponent
        {
            SafeDistance = safeDistance,
            CheckInterval = checkInterval
        });

        return this;
    }

    public Entity Build()
    {
        return Entity;
    }
}

