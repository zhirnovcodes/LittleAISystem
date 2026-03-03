using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct GenomeBuilderTestSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunGenomeBuilderTestComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        Debug.Log("=== Starting GenomeBuilder Tests ===");

        TestStatsIncreaseGenome(ref state);
        TestSpeedGenome(ref state);
        TestAgingGenome(ref state);
        TestVisionGenome(ref state);
        TestNeedsBasedGenome(ref state);
        TestStatsGenome(ref state);
        TestActionChainGenome(ref state);
        TestAdvertiserGenome(ref state);
        TestReproductionGenome(ref state);
        TestStatAttenuationGenome(ref state);

        Debug.Log("=== All GenomeBuilder Tests Complete ===");
    }

    private void TestStatsIncreaseGenome(ref SystemState state)
    {
        Debug.Log("\n--- Testing StatsIncrease GenomeType ---");

        // Create test data
        var testData = new StatsIncreaseGenomeData
        {
            AnimalStats = new AnimalStats
            {
                Stats = new float4x2(
                    new float4(10f, 20f, 30f, 40f), // Energy, Fullness, Toilet, Social
                    new float4(50f, 60f, 0f, 0f)    // Safety, Health
                )
            }
        };

        var testData2 = new StatsIncreaseGenomeData
        {
            AnimalStats = new AnimalStats
            {
                Stats = new float4x2(
                    new float4(5f, 15f, 25f, 35f),
                    new float4(45f, 55f, 0f, 0f)
                )
            }
        };

        // Test IGenomeDataConvertible.GetDNAData()
        DNAChainData dnaData = testData.GetDNAData();
        Debug.Log($"GenomeData Index: {dnaData.GenomeData.Index} (expected: 0)");
        Debug.Log($"GenomeData.Data.c0: {dnaData.GenomeData.Data.c0} (expected: (10, 20, 30, 40))");
        Debug.Log($"GenomeData.Data.c1: {dnaData.GenomeData.Data.c1} (expected: (50, 60, 0, 0))");
        
        AssertEqual(dnaData.GenomeData.Index, 0, "StatsIncrease Index");
        AssertEqual((int)dnaData.GenomeType, (int)GenomeType.StatsIncrease, "StatsIncrease GenomeType");
        AssertApprox(dnaData.GenomeData.Data.c0, new float4(10f, 20f, 30f, 40f), "StatsIncrease Data c0");
        AssertApprox(dnaData.GenomeData.Data.c1, new float4(50f, 60f, 0f, 0f), "StatsIncrease Data c1");

        DNAChainData dnaData2 = testData2.GetDNAData();
        AssertEqual(dnaData2.GenomeData.Index, 0, "StatsIncrease 2 Index");
        AssertApprox(dnaData2.GenomeData.Data.c0, new float4(5f, 15f, 25f, 35f), "StatsIncrease 2 Data c0");
        AssertApprox(dnaData2.GenomeData.Data.c1, new float4(45f, 55f, 0f, 0f), "StatsIncrease 2 Data c1");

        // Test Builder with multiple calls (second call overwrites first)
        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        Entity testEntity = state.EntityManager.CreateEntity();
        
        var builder = new AnimalGenomeBuilder(commandBuffer, testEntity);
        builder.WithGenome(GenomeType.StatsIncrease, dnaData.GenomeData);
        builder.WithGenome(GenomeType.StatsIncrease, dnaData2.GenomeData);
        Entity result = builder.Build();
        
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        // Verify component was added
        Assert(state.EntityManager.HasComponent<StatsIncreaseComponent>(testEntity), 
            "StatsIncrease: Should have StatsIncreaseComponent");
        
        // Verify component data (should have second data due to overwrite)
        var component = state.EntityManager.GetComponentData<StatsIncreaseComponent>(testEntity);
        AssertApprox(component.AnimalStats.Stats.c0, new float4(5f, 15f, 25f, 35f), "StatsIncrease Component c0 (from testData2)");
        AssertApprox(component.AnimalStats.Stats.c1, new float4(45f, 55f, 0f, 0f), "StatsIncrease Component c1 (from testData2)");

        // Cleanup
        state.EntityManager.DestroyEntity(testEntity);
        
        Debug.Log("✓ StatsIncrease GenomeType test passed");
    }

    private void TestSpeedGenome(ref SystemState state)
    {
        Debug.Log("\n--- Testing Speed GenomeType ---");

        var testData = new SpeedGenomeData
        {
            MaxSpeed = 5.5f,
            MaxRotationSpeed = 3.14f
        };

        DNAChainData dnaData = testData.GetDNAData();
        AssertEqual(dnaData.GenomeData.Index, 0, "Speed Index");
        AssertEqual((int)dnaData.GenomeType, (int)GenomeType.Speed, "Speed GenomeType");
        AssertApprox(dnaData.GenomeData.Data.c0.x, 5.5f, "Speed MaxSpeed");
        AssertApprox(dnaData.GenomeData.Data.c0.y, 3.14f, "Speed MaxRotationSpeed");

        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        Entity testEntity = state.EntityManager.CreateEntity();
        
        var builder = new AnimalGenomeBuilder(commandBuffer, testEntity);
        builder.WithGenome(GenomeType.Speed, dnaData.GenomeData);
        builder.Build();
        
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        Assert(state.EntityManager.HasComponent<MovingSpeedComponent>(testEntity), 
            "Speed: Should have MovingSpeedComponent");
        
        var component = state.EntityManager.GetComponentData<MovingSpeedComponent>(testEntity);
        AssertApprox(component.MaxSpeed, 5.5f, "Speed Component MaxSpeed");
        AssertApprox(component.MaxRotationSpeed, 3.14f, "Speed Component MaxRotationSpeed");

        state.EntityManager.DestroyEntity(testEntity);
        Debug.Log("✓ Speed GenomeType test passed");
    }

    private void TestAgingGenome(ref SystemState state)
    {
        Debug.Log("\n--- Testing Aging GenomeType ---");

        var testData = new AgingGenomeData
        {
            MinSize = 0.5f,
            MaxSize = 2.0f
        };

        DNAChainData dnaData = testData.GetDNAData();
        AssertEqual(dnaData.GenomeData.Index, 0, "Aging Index");
        AssertEqual((int)dnaData.GenomeType, (int)GenomeType.Aging, "Aging GenomeType");
        AssertApprox(dnaData.GenomeData.Data.c0.x, 0.5f, "Aging MinSize");
        AssertApprox(dnaData.GenomeData.Data.c0.y, 2.0f, "Aging MaxSize");

        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        Entity testEntity = state.EntityManager.CreateEntity();
        
        var builder = new AnimalGenomeBuilder(commandBuffer, testEntity);
        builder.WithGenome(GenomeType.Aging, dnaData.GenomeData);
        builder.Build();
        
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        Assert(state.EntityManager.HasComponent<AgingComponent>(testEntity), 
            "Aging: Should have AgingComponent");
        
        var component = state.EntityManager.GetComponentData<AgingComponent>(testEntity);
        AssertApprox(component.MinSize, 0.5f, "Aging Component MinSize");
        AssertApprox(component.MaxSize, 2.0f, "Aging Component MaxSize");

        state.EntityManager.DestroyEntity(testEntity);
        Debug.Log("✓ Aging GenomeType test passed");
    }

    private void TestVisionGenome(ref SystemState state)
    {
        Debug.Log("\n--- Testing Vision GenomeType ---");

        var testData = new VisionGenomeData
        {
            MaxDistance = 10.0f,
            Interval = 0.5f
        };

        DNAChainData dnaData = testData.GetDNAData();
        AssertEqual(dnaData.GenomeData.Index, 0, "Vision Index");
        AssertEqual((int)dnaData.GenomeType, (int)GenomeType.Vision, "Vision GenomeType");
        AssertApprox(dnaData.GenomeData.Data.c0.x, 10.0f, "Vision MaxDistance");
        AssertApprox(dnaData.GenomeData.Data.c0.y, 0.5f, "Vision Interval");

        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        Entity testEntity = state.EntityManager.CreateEntity();
        
        var builder = new AnimalGenomeBuilder(commandBuffer, testEntity);
        builder.WithGenome(GenomeType.Vision, dnaData.GenomeData);
        builder.Build();
        
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        Assert(state.EntityManager.HasComponent<VisionComponent>(testEntity), 
            "Vision: Should have VisionComponent");
        Assert(state.EntityManager.HasBuffer<VisibleItem>(testEntity), 
            "Vision: Should have VisibleItem buffer");
        
        var component = state.EntityManager.GetComponentData<VisionComponent>(testEntity);
        AssertApprox(component.MaxDistance, 10.0f, "Vision Component MaxDistance");
        AssertApprox(component.Interval, 0.5f, "Vision Component Interval");
        AssertApprox(component.TimeElapsed, 0f, "Vision Component TimeElapsed");

        state.EntityManager.DestroyEntity(testEntity);
        Debug.Log("✓ Vision GenomeType test passed");
    }

    private void TestNeedsBasedGenome(ref SystemState state)
    {
        Debug.Log("\n--- Testing NeedsBased GenomeType ---");

        var testData = new NeedsBasedGenomeData
        {
            CancelThreshold = 75.0f,
            AddThreshold = 25.0f
        };

        DNAChainData dnaData = testData.GetDNAData();
        AssertEqual(dnaData.GenomeData.Index, 0, "NeedsBased Index");
        AssertEqual((int)dnaData.GenomeType, (int)GenomeType.NeedsBased, "NeedsBased GenomeType");
        AssertApprox(dnaData.GenomeData.Data.c0.x, 75.0f, "NeedsBased CancelThreshold");
        AssertApprox(dnaData.GenomeData.Data.c0.y, 25.0f, "NeedsBased AddThreshold");

        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        Entity testEntity = state.EntityManager.CreateEntity();
        
        var builder = new AnimalGenomeBuilder(commandBuffer, testEntity);
        builder.WithGenome(GenomeType.NeedsBased, dnaData.GenomeData);
        builder.Build();
        
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        Assert(state.EntityManager.HasComponent<NeedsActionChainComponent>(testEntity), 
            "NeedsBased: Should have NeedsActionChainComponent");
        Assert(state.EntityManager.HasBuffer<NeedBasedInputItem>(testEntity), 
            "NeedsBased: Should have NeedBasedInputItem buffer");
        Assert(state.EntityManager.HasComponent<NeedBasedOutputComponent>(testEntity), 
            "NeedsBased: Should have NeedBasedOutputComponent");
        
        var component = state.EntityManager.GetComponentData<NeedsActionChainComponent>(testEntity);
        AssertApprox(component.CancelThreshold, 75.0f, "NeedsBased Component CancelThreshold");
        AssertApprox(component.AddThreshold, 25.0f, "NeedsBased Component AddThreshold");

        state.EntityManager.DestroyEntity(testEntity);
        Debug.Log("✓ NeedsBased GenomeType test passed");
    }

    private void TestStatsGenome(ref SystemState state)
    {
        Debug.Log("\n--- Testing Stats GenomeType ---");

        var testData = new StatsGenomeData
        {
            Stats = new AnimalStats
            {
                Stats = new float4x2(
                    new float4(100f, 90f, 80f, 70f),
                    new float4(60f, 50f, 0f, 0f)
                )
            }
        };

        DNAChainData dnaData = testData.GetDNAData();
        AssertEqual(dnaData.GenomeData.Index, 0, "Stats Index");
        AssertEqual((int)dnaData.GenomeType, (int)GenomeType.Stats, "Stats GenomeType");
        AssertApprox(dnaData.GenomeData.Data.c0, new float4(100f, 90f, 80f, 70f), "Stats Data c0");
        AssertApprox(dnaData.GenomeData.Data.c1, new float4(60f, 50f, 0f, 0f), "Stats Data c1");

        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        Entity testEntity = state.EntityManager.CreateEntity();
        
        var builder = new AnimalGenomeBuilder(commandBuffer, testEntity);
        builder.WithGenome(GenomeType.Stats, dnaData.GenomeData);
        builder.Build();
        
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        Assert(state.EntityManager.HasComponent<AnimalStatsComponent>(testEntity), 
            "Stats: Should have AnimalStatsComponent");
        Assert(state.EntityManager.HasBuffer<StatsChangeItem>(testEntity), 
            "Stats: Should have StatsChangeItem buffer");
        
        var component = state.EntityManager.GetComponentData<AnimalStatsComponent>(testEntity);
        AssertApprox(component.Stats.Stats.c0, new float4(100f, 90f, 80f, 70f), "Stats Component c0");
        AssertApprox(component.Stats.Stats.c1, new float4(60f, 50f, 0f, 0f), "Stats Component c1");

        state.EntityManager.DestroyEntity(testEntity);
        Debug.Log("✓ Stats GenomeType test passed");
    }

    private void TestActionChainGenome(ref SystemState state)
    {
        Debug.Log("\n--- Testing ActionChain GenomeType ---");

        var testData = new ActionChainGenomeData();

        DNAChainData dnaData = testData.GetDNAData();
        AssertEqual(dnaData.GenomeData.Index, 0, "ActionChain Index");
        AssertEqual((int)dnaData.GenomeType, (int)GenomeType.ActionChain, "ActionChain GenomeType");

        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        Entity testEntity = state.EntityManager.CreateEntity();
        
        uint testSeed = 42;
        var builder = new AnimalGenomeBuilder(commandBuffer, testEntity, testSeed);
        builder.WithGenome(GenomeType.ActionChain, dnaData.GenomeData);
        builder.Build();
        
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        Assert(state.EntityManager.HasComponent<ActionRunnerComponent>(testEntity), 
            "ActionChain: Should have ActionRunnerComponent");
        Assert(state.EntityManager.HasBuffer<ActionChainItem>(testEntity), 
            "ActionChain: Should have ActionChainItem buffer");
        Assert(state.EntityManager.HasComponent<SubActionTimeComponent>(testEntity), 
            "ActionChain: Should have SubActionTimeComponent");
        Assert(state.EntityManager.HasComponent<ActionRandomComponent>(testEntity), 
            "ActionChain: Should have ActionRandomComponent");

        // Verify ActionRandomComponent is initialized with correct seed
        var randomComponent = state.EntityManager.GetComponentData<ActionRandomComponent>(testEntity);
        Assert(randomComponent.Random.state != 0, "ActionRandomComponent should have initialized Random");

        state.EntityManager.DestroyEntity(testEntity);
        Debug.Log("✓ ActionChain GenomeType test passed");
    }

    private void TestAdvertiserGenome(ref SystemState state)
    {
        Debug.Log("\n--- Testing Advertiser GenomeType ---");

        var testData = new AdvertiserGenomeData
        {
            AdvertisedValue = new AnimalStats
            {
                Stats = new float4x2(
                    new float4(15f, 25f, 35f, 45f),
                    new float4(55f, 65f, 0f, 0f)
                )
            },
            ActorConditions = ConditionFlags.IsAnimal | ConditionFlags.IsPredator,
            ActionType = ActionTypes.Eat
        };
        
        // Test multiple advertisers
        var testData2 = new AdvertiserGenomeData
        {
            AdvertisedValue = new AnimalStats { Stats = new float4x2(new float4(1f, 2f, 3f, 4f), float4.zero) },
            ActorConditions = ConditionFlags.None,
            ActionType = ActionTypes.Sleep
        };

        // Test DNAChainData conversion for first advertiser
        DNAChainData dnaData = testData.GetDNAData();
        int expectedIndex = ((int)(ConditionFlags.IsAnimal | ConditionFlags.IsPredator) << 8) | (int)ActionTypes.Eat;
        AssertEqual(dnaData.GenomeData.Index, expectedIndex, "Advertiser Index");
        AssertEqual((int)dnaData.GenomeType, (int)GenomeType.Advertiser, "Advertiser GenomeType");
        AssertApprox(dnaData.GenomeData.Data.c0, new float4(15f, 25f, 35f, 45f), "Advertiser Data c0");
        AssertApprox(dnaData.GenomeData.Data.c1, new float4(55f, 65f, 0f, 0f), "Advertiser Data c1");

        // Test DNAChainData conversion for second advertiser
        DNAChainData dnaData2 = testData2.GetDNAData();
        int expectedIndex2 = ((int)ConditionFlags.None << 8) | (int)ActionTypes.Sleep;
        AssertEqual(dnaData2.GenomeData.Index, expectedIndex2, "Advertiser 2 Index");
        AssertApprox(dnaData2.GenomeData.Data.c0, new float4(1f, 2f, 3f, 4f), "Advertiser 2 Data c0");

        // Add both advertisers using a single builder instance
        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        Entity testEntity = state.EntityManager.CreateEntity();
        
        var builder = new AnimalGenomeBuilder(commandBuffer, testEntity);
        builder.WithGenome(GenomeType.Advertiser, dnaData.GenomeData);
        builder.WithGenome(GenomeType.Advertiser, dnaData2.GenomeData);
        builder.Build();
        
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        // Verify buffer was created
        Assert(state.EntityManager.HasBuffer<StatAdvertiserItem>(testEntity), 
            "Advertiser: Should have StatAdvertiserItem buffer");
        
        var buffer = state.EntityManager.GetBuffer<StatAdvertiserItem>(testEntity);
        AssertEqual(buffer.Length, 2, "Advertiser: Buffer should have 2 items");
        
        // Verify first advertiser item
        var item = buffer[0];
        AssertApprox(item.AdvertisedValue.Stats.c0, new float4(15f, 25f, 35f, 45f), "Advertiser Item 1 c0");
        AssertApprox(item.AdvertisedValue.Stats.c1, new float4(55f, 65f, 0f, 0f), "Advertiser Item 1 c1");
        AssertEqual((int)item.ActorConditions, (int)(ConditionFlags.IsAnimal | ConditionFlags.IsPredator), "Advertiser Item 1 ActorConditions");
        AssertEqual((int)item.ActionType, (int)ActionTypes.Eat, "Advertiser Item 1 ActionType");

        // Verify second advertiser item
        var item2 = buffer[1];
        AssertApprox(item2.AdvertisedValue.Stats.c0, new float4(1f, 2f, 3f, 4f), "Advertiser Item 2 c0");
        AssertEqual((int)item2.ActorConditions, (int)ConditionFlags.None, "Advertiser Item 2 ActorConditions");
        AssertEqual((int)item2.ActionType, (int)ActionTypes.Sleep, "Advertiser Item 2 ActionType");

        state.EntityManager.DestroyEntity(testEntity);
        Debug.Log("✓ Advertiser GenomeType test passed");
    }

    private void TestReproductionGenome(ref SystemState state)
    {
        Debug.Log("\n--- Testing Reproduction GenomeType ---");

        // Test Male
        var testDataMale = new ReproductionGenomeData
        {
            IsMale = true,
            GestationTime = 15f
        };

        DNAChainData dnaDataMale = testDataMale.GetDNAData();
        AssertEqual(dnaDataMale.GenomeData.Index, 0, "Reproduction Male Index");
        AssertEqual((int)dnaDataMale.GenomeType, (int)GenomeType.Reproduction, "Reproduction Male GenomeType");
        AssertApprox(dnaDataMale.GenomeData.Data.c0.x, 1f, "Reproduction Male IsMale value");
        AssertApprox(dnaDataMale.GenomeData.Data.c0.y, 15f, "Reproduction Male GestationTime value");

        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        Entity testEntityMale = state.EntityManager.CreateEntity();
        
        var builder = new AnimalGenomeBuilder(commandBuffer, testEntityMale);
        builder.WithGenome(GenomeType.Reproduction, dnaDataMale.GenomeData);
        builder.Build();
        
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        Assert(state.EntityManager.HasComponent<GenetaliaComponent>(testEntityMale), 
            "Reproduction Male: Should have GenetaliaComponent");
        Assert(state.EntityManager.HasComponent<ReproductionComponent>(testEntityMale), 
            "Reproduction Male: Should have ReproductionComponent");
        Assert(!state.EntityManager.HasBuffer<DNAStorageItem>(testEntityMale), 
            "Reproduction Male: Should NOT have DNAStorageItem buffer");
        
        var genetaliaMale = state.EntityManager.GetComponentData<GenetaliaComponent>(testEntityMale);
        Assert(genetaliaMale.IsMale, "Genitalia Component should be male");
        
        var componentMale = state.EntityManager.GetComponentData<ReproductionComponent>(testEntityMale);
        Assert(componentMale.IsMale, "Reproduction Component should be male");
        AssertApprox(componentMale.GestationTime, 15f, "Reproduction Male GestationTime");

        // Test Female
        var testDataFemale = new ReproductionGenomeData
        {
            IsMale = false,
            GestationTime = 20f
        };

        DNAChainData dnaDataFemale = testDataFemale.GetDNAData();
        AssertApprox(dnaDataFemale.GenomeData.Data.c0.x, 0f, "Reproduction Female IsMale value");
        AssertApprox(dnaDataFemale.GenomeData.Data.c0.y, 20f, "Reproduction Female GestationTime value");

        EntityCommandBuffer commandBuffer2 = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        Entity testEntityFemale = state.EntityManager.CreateEntity();
        
        var builder2 = new AnimalGenomeBuilder(commandBuffer2, testEntityFemale);
        builder2.WithGenome(GenomeType.Reproduction, dnaDataFemale.GenomeData);
        builder2.Build();
        
        commandBuffer2.Playback(state.EntityManager);
        commandBuffer2.Dispose();

        Assert(state.EntityManager.HasComponent<GenetaliaComponent>(testEntityFemale), 
            "Reproduction Female: Should have GenetaliaComponent");
        Assert(state.EntityManager.HasComponent<ReproductionComponent>(testEntityFemale), 
            "Reproduction Female: Should have ReproductionComponent");
        Assert(state.EntityManager.HasBuffer<DNAStorageItem>(testEntityFemale), 
            "Reproduction Female: Should have DNAStorageItem buffer");
        
        var genetaliaFemale = state.EntityManager.GetComponentData<GenetaliaComponent>(testEntityFemale);
        Assert(!genetaliaFemale.IsMale, "Genitalia Component should be female");
        
        var componentFemale = state.EntityManager.GetComponentData<ReproductionComponent>(testEntityFemale);
        Assert(!componentFemale.IsMale, "Reproduction Component should be female");
        AssertApprox(componentFemale.GestationTime, 20f, "Reproduction Female GestationTime");

        state.EntityManager.DestroyEntity(testEntityMale);
        state.EntityManager.DestroyEntity(testEntityFemale);
        Debug.Log("✓ Reproduction GenomeType test passed");
    }

    private void TestStatAttenuationGenome(ref SystemState state)
    {
        Debug.Log("\n--- Testing StatAttenuation GenomeType ---");

        var testData = new StatAttenuationGenomeData
        {
            StatType = StatType.Energy,
            Attenuation = new AnimalStatsAttenuation
            {
                Needs = new HermiteCurve
                {
                    points = new float4(0f, 1f, 100f, 0f),
                    tangents = new float2(-0.01f, -0.01f)
                },
                Distance = new HermiteCurve
                {
                    points = new float4(0f, 1f, 50f, 0.5f),
                    tangents = new float2(-0.02f, -0.02f)
                }
            }
        };

        // Test multiple stat attenuations
        var testData2 = new StatAttenuationGenomeData
        {
            StatType = StatType.Health,
            Attenuation = new AnimalStatsAttenuation
            {
                Needs = new HermiteCurve
                {
                    points = new float4(10f, 20f, 30f, 40f),
                    tangents = new float2(0.5f, 0.5f)
                },
                Distance = new HermiteCurve
                {
                    points = new float4(5f, 15f, 25f, 35f),
                    tangents = new float2(1f, 1f)
                }
            }
        };

        // Test DNAChainData conversion for first attenuation
        DNAChainData dnaData = testData.GetDNAData();
        AssertEqual(dnaData.GenomeData.Index, (int)StatType.Energy, "StatAttenuation Index");
        AssertEqual((int)dnaData.GenomeType, (int)GenomeType.StatAttenuation, "StatAttenuation GenomeType");
        AssertApprox(dnaData.GenomeData.Data.c0, new float4(0f, 1f, 100f, 0f), "StatAttenuation Needs points");
        AssertApprox(dnaData.GenomeData.Data.c1, new float4(-0.01f, -0.01f, 0f, 0f), "StatAttenuation Needs tangents");
        AssertApprox(dnaData.GenomeData.Data.c2, new float4(0f, 1f, 50f, 0.5f), "StatAttenuation Distance points");
        AssertApprox(dnaData.GenomeData.Data.c3, new float4(-0.02f, -0.02f, 0f, 0f), "StatAttenuation Distance tangents");

        // Test DNAChainData conversion for second attenuation
        DNAChainData dnaData2 = testData2.GetDNAData();
        AssertEqual(dnaData2.GenomeData.Index, (int)StatType.Health, "StatAttenuation 2 Index");
        AssertApprox(dnaData2.GenomeData.Data.c0, new float4(10f, 20f, 30f, 40f), "StatAttenuation 2 Needs points");
        AssertApprox(dnaData2.GenomeData.Data.c2, new float4(5f, 15f, 25f, 35f), "StatAttenuation 2 Distance points");

        // Add both stat attenuations using a single builder instance
        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        Entity testEntity = state.EntityManager.CreateEntity();
        
        var builder = new AnimalGenomeBuilder(commandBuffer, testEntity);
        builder.WithGenome(GenomeType.StatAttenuation, dnaData.GenomeData);
        builder.WithGenome(GenomeType.StatAttenuation, dnaData2.GenomeData);
        builder.Build();
        
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        // Verify component was created
        Assert(state.EntityManager.HasComponent<AnimalStatsAttenuationComponent>(testEntity), 
            "StatAttenuation: Should have AnimalStatsAttenuationComponent");
        
        // Verify both stat attenuations were set correctly
        var component = state.EntityManager.GetComponentData<AnimalStatsAttenuationComponent>(testEntity);
        
        var energyAttenuation = component.Attenuation.Energy;
        AssertApprox(energyAttenuation.Needs.points, new float4(0f, 1f, 100f, 0f), "StatAttenuation Component Energy Needs points");
        AssertApprox(energyAttenuation.Distance.points, new float4(0f, 1f, 50f, 0.5f), "StatAttenuation Component Energy Distance points");

        var healthAttenuation = component.Attenuation.Health;
        AssertApprox(healthAttenuation.Needs.points, new float4(10f, 20f, 30f, 40f), "StatAttenuation Component Health Needs points");
        AssertApprox(healthAttenuation.Distance.points, new float4(5f, 15f, 25f, 35f), "StatAttenuation Component Health Distance points");

        state.EntityManager.DestroyEntity(testEntity);
        Debug.Log("✓ StatAttenuation GenomeType test passed");
    }

    // Helper assertion methods
    private void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError($"FAILED: {message}");
        }
        else
        {
            Debug.Log($"PASSED: {message}");
        }
    }

    private void AssertEqual(int actual, int expected, string testName)
    {
        if (actual != expected)
        {
            Debug.LogError($"FAILED: {testName} - Expected: {expected}, Got: {actual}");
        }
        else
        {
            Debug.Log($"PASSED: {testName}");
        }
    }

    private void AssertApprox(float actual, float expected, string testName, float tolerance = 0.001f)
    {
        if (math.abs(actual - expected) > tolerance)
        {
            Debug.LogError($"FAILED: {testName} - Expected: {expected}, Got: {actual}");
        }
        else
        {
            Debug.Log($"PASSED: {testName}");
        }
    }

    private void AssertApprox(float4 actual, float4 expected, string testName, float tolerance = 0.001f)
    {
        bool passed = math.all(math.abs(actual - expected) < tolerance);
        if (!passed)
        {
            Debug.LogError($"FAILED: {testName} - Expected: {expected}, Got: {actual}");
        }
        else
        {
            Debug.Log($"PASSED: {testName}");
        }
    }
}

